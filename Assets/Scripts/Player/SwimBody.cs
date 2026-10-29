using System.Collections.Generic;
using UnityEngine;

/// <summary>Cómo nada el cuerpo ahora mismo.</summary>
public enum SwimStyle { None, Tread, Breaststroke, Crawl, Underwater }

/// <summary>
/// NADAR DE VERDAD (Berón 2026-10-29: «hasta ahora no he visto a nadie nadar, mucho menos bucear; en el mar Kushal parece
/// que camina o da saltitos»). Un mismo cuerpo de nado para el jugador y para cualquiera que mueva el código:
///   · AGUA PROFUNDA (más que el pecho): el cuerpo se pone HORIZONTAL —boca abajo, la cabeza alzada para respirar— y nada
///     a CROL (deprisa: brazos alternos por encima del agua + patada de batido) o a BRAZA (despacio y bajo el agua: tirón,
///     recobro y patada de rana). Parado, se MANTIENE a flote vertical (brazos que «remolinean» y piernas de batidora).
///     El ritmo del ciclo va con la velocidad. Mientras nada, el andar (<see cref="CubeWalkCycle"/>) cede todos sus huesos.
///   · BUCEAR: el cuerpo se inclina según hacia dónde va (de cabeza al bajar). Con la cabeza bajo la superficie se gasta el
///     ALIENTO (<see cref="Breath01"/>; aguanta más quien tiene más resistencia); al asomar, se recupera. Sin aliento, sube.
///   · La SUPERFICIE y la profundidad salen de <see cref="WaterZone"/> por posición (el mar, la columna de agua del árbol…).
/// API para personajes (lo usan <see cref="PathMover"/> y el árbol): <see cref="Of"/>, <see cref="Begin"/>/<see cref="End"/>,
/// <see cref="SwimTo"/> (un paso por frame, como PathMover.StepToward), <see cref="UpdateWater"/> (empieza/acaba según la
/// profundidad) y <see cref="FloatPivotY"/> (a qué altura flota el pivote).
/// </summary>
[DefaultExecutionOrder(200)]   // después del andar y de las posturas: este frame, los huesos del nado mandan
[DisallowMultipleComponent]
public class SwimBody : MonoBehaviour
{
    [Header("Nado")]
    [Tooltip("Velocidad de nado (m/s) de los personajes que no son el jugador.")]
    public float npcSwimSpeed = 1.3f;
    [Tooltip("Por encima de esta velocidad (m/s), en la superficie se nada a crol; por debajo, a braza.")]
    public float crawlAbove = 1.6f;
    [Tooltip("Subida por flotabilidad (m/s) cuando, bajo el agua, no se nada ni arriba ni abajo.")]
    public float buoyantRise = 0.35f;

    [Header("Aliento")]
    [Tooltip("Segundos de apnea con resistencia 1 (se escala con la resistencia del Anima).")]
    public float breathHoldSeconds = 50f;
    [Tooltip("Segundos para recuperar todo el aliento con la cabeza fuera del agua.")]
    public float breathRefillSeconds = 4f;

    // ── API ───────────────────────────────────────────────────────────────────────────────────────

    /// <summary>El cuerpo de nado de <paramref name="c"/> (lo añade si no lo tiene).</summary>
    public static SwimBody Of(Component c) => c.GetOrAdd<SwimBody>();
    /// <summary>¿Está nadando ese personaje?</summary>
    public static bool Swimming(GameObject g) => g != null && g.TryGetComponent(out SwimBody s) && s.Active;

    /// <summary>¿Nada ahora? (Begin…End)</summary>
    public bool Active { get; private set; }
    public SwimStyle Style { get; private set; }
    /// <summary>Aliento: 1 lleno, 0 sin aire.</summary>
    public float Breath01 { get; private set; } = 1f;
    public bool OutOfBreath => Breath01 <= 0f;
    public bool HeadAboveWater { get; private set; } = true;
    /// <summary>Superficie del agua en la que nada (NaN si no se conoce).</summary>
    public float Surface { get; private set; } = float.NaN;
    /// <summary>Velocidad suavizada (m/s, 3D) medida del propio desplazamiento.</summary>
    public Vector3 Velocity { get; private set; }
    public float Speed => Velocity.magnitude;
    /// <summary>Ciclos de brazada por segundo (va con la velocidad).</summary>
    public float StrokeRate { get; private set; }
    /// <summary>Ciclos de brazada completados desde que empezó a nadar.</summary>
    public int Strokes { get; private set; }
    /// <summary>Inclinación del cuerpo respecto a la vertical (0 = de pie, 90 = horizontal, 180 = cabeza abajo).</summary>
    public float BodyPitch => _pitch;
    /// <summary>¿Tiene cuerpo de cubos humano que animar? (los animales tienen su propia marcha, CubeGait)</summary>
    public bool HasBody { get { Cache(); return _hips != null; } }

    /// <summary>Lo que hay de agua en un punto (los pies del personaje).</summary>
    public struct Water
    {
        public bool inWater;     // los pies bajo la superficie
        public float surface;    // altura de la superficie
        public float depth;      // de la superficie al fondo que hay debajo (999 = sin fondo)
        public float chest;      // a qué profundidad le cubre el pecho a este cuerpo
        public float feetDepth;  // cuánto le cubre desde los pies
    }

    /// <summary>Agua bajo <paramref name="who"/> con el pivote en <paramref name="pivot"/>.</summary>
    public static Water Probe(Transform who, Vector3 pivot, float pivotToFeet, float bodyHeight)
    {
        Water w = default;
        float feet = pivot.y - pivotToFeet;
        WaterZone z = WaterZone.At(new Vector3(pivot.x, feet + 0.05f, pivot.z));
        if (z == null) return w;
        w.surface = z.Surface;
        w.feetDepth = w.surface - feet;
        w.inWater = w.feetDepth > 0.02f;
        w.chest = bodyHeight * 0.72f;
        if (!w.inWater) return w;   // de pie sobre el hielo, encima del mar: no hace falta medir el fondo
        w.depth = 999f;
        // El fondo: lo más alto bajo la superficie… o lo que ASOMA un poco sobre ella (el canto del hielo, una orilla, el
        // umbral de una puerta): entonces la profundidad sale negativa y no se nada (se sale del agua por ahí).
        int n = Physics.RaycastNonAlloc(new Vector3(pivot.x, w.surface + 1f, pivot.z), Vector3.down, _hits, 301f, ~0, QueryTriggerInteraction.Ignore);
        float best = float.NegativeInfinity;
        for (int i = 0; i < n; i++)
        {
            RaycastHit h = _hits[i];
            if (who != null && h.transform.IsChildOf(who)) continue;
            if (h.point.y <= best || h.point.y > w.surface + 0.3f || h.collider is CharacterController || h.collider.GetComponentInParent<Anima>() != null || h.collider.GetComponentInParent<SwimBody>() != null) continue;   // otro nadador no es el fondo
            best = h.point.y;
        }
        if (!float.IsNegativeInfinity(best)) w.depth = w.surface - best;
        return w;
    }
    static readonly RaycastHit[] _hits = new RaycastHit[16];

    /// <summary>El agua bajo este personaje ahora.</summary>
    public Water ProbeHere() => Probe(transform, transform.position, PivotToFeet, BodyHeight);

    /// <summary>¿Con esa agua hay que nadar? (más honda que el pecho; con histéresis para no alternar en el límite)</summary>
    public bool ShouldSwim(Water w) => w.inWater && w.depth > w.chest + (Active ? -0.2f : 0.05f);

    /// <summary>Empieza o acaba de nadar según la profundidad donde está (agua más honda que el pecho → nada; menos → vadea).
    /// Lo llaman quienes mueven al personaje (el jugador cada frame, PathMover en cada paso). Devuelve si nada.</summary>
    public bool UpdateWater()
    {
        _drivenAt = Time.time;
        Water w = ProbeHere();
        bool swim = ShouldSwim(w);
        if (swim) { Surface = w.surface; Begin(); }
        else if (Active) End();
        return swim;
    }

    /// <summary>Empieza a nadar (pone el cuerpo de nado). Idempotente.</summary>
    public void Begin()
    {
        _drivenAt = Time.time;
        if (Active) return;
        Active = true; Strokes = 0;
        RefreshSurface();
    }

    /// <summary>Deja de nadar (el cuerpo vuelve a su postura en un momento). Idempotente.</summary>
    public void End()
    {
        if (!Active) return;
        Active = false; Style = SwimStyle.None;
    }

    /// <summary>A qué altura queda el PIVOTE flotando boca abajo en la superficie: la pelvis justo bajo el agua y la cabeza
    /// fuera para respirar.</summary>
    public float FloatPivotY => FloatPivotYFor(float.IsNaN(Surface) ? transform.position.y : Surface);
    public float FloatPivotYFor(float surface)
    {
        Cache();
        float hipsAbovePivot = _hips != null ? _hips.position.y - transform.position.y : 1f - PivotToFeet;   // sin cuerpo: pelvis ≈ 1 m sobre los pies
        return surface - 0.06f - hipsAbovePivot;
    }

    /// <summary>Nada con esa velocidad (m/s, mundo) este frame: el jugador lo llama con su entrada. Sin intención vertical,
    /// flota hacia la superficie (cerca de ella se queda a flote; más abajo sube despacio por flotabilidad); nunca sale por
    /// encima de la línea de flotación; sin aliento, sube. Mueve con el CharacterController si lo hay. Devuelve la velocidad usada.</summary>
    public Vector3 Drive(Vector3 velocity, float dt)
    {
        Begin();
        RefreshSurface();
        float floatY = FloatPivotY, y = transform.position.y;
        bool vertical = Mathf.Abs(velocity.y) > 0.25f;
        if (OutOfBreath && _isPlayer) { velocity.y = Mathf.Max(velocity.y, 1.2f); vertical = true; }   // sin aire: arriba
        if (!vertical) velocity.y = y > floatY - 0.7f ? Mathf.Clamp((floatY - y) * 3f, -1.5f, 1.5f) : buoyantRise;
        else if (velocity.y > 0f) velocity.y = Mathf.Min(velocity.y, Mathf.Max(0f, (floatY - y) * 4f));   // no salta fuera del agua
        if (y > floatY + 0.02f && velocity.y > -0.5f) velocity.y = Mathf.Min(velocity.y, (floatY - y) * 3f);   // cayó al agua: baja a flote
        Move(velocity * dt);
        return velocity;
    }

    /// <summary>Un paso NADANDO en línea recta hacia <paramref name="target"/> (posición del pivote; puede estar arriba, abajo
    /// o al otro lado): para recorrer la columna de agua del árbol, de una puerta a otra. Dentro del agua no pasa de la línea
    /// de flotación. Orienta el cuerpo hacia donde va (de cabeza al bajar). Devuelve la distancia que queda; quien conduce
    /// llama a <see cref="End"/> al salir del agua (si no, se acaba solo en cuanto deja de estar en el agua).</summary>
    public float SwimTo(Vector3 target, float speed = -1f)
    {
        if (speed <= 0f) speed = npcSwimSpeed;
        Begin();
        RefreshSurface();
        WaterZone z = WaterZone.At(target);
        if (z != null) target.y = Mathf.Min(target.y, FloatPivotYFor(z.Surface));
        Vector3 d = target - transform.position;
        float dist = d.magnitude;
        if (dist < 1e-3f) return 0f;
        Move(d / dist * Mathf.Min(speed * Time.deltaTime, dist));
        Vector3 flat = new Vector3(d.x, 0f, d.z);
        if (flat.sqrMagnitude > 0.01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 1f - Mathf.Exp(-6f * Time.deltaTime));
        return (target - transform.position).magnitude;
    }

    // ── Medidas del cuerpo ─────────────────────────────────────────────────────────────────────────

    /// <summary>Del pivote a los pies (CharacterController, cuerpo de cubos o cápsula).</summary>
    public float PivotToFeet
    {
        get
        {
            if (TryGetComponent(out CharacterController cc)) return (cc.height * 0.5f - cc.center.y) * transform.lossyScale.y;
            Cache();
            if (_body != null) return transform.position.y - _body.position.y;
            if (TryGetComponent(out CapsuleCollider c)) return (c.height * 0.5f - c.center.y) * transform.lossyScale.y;
            return 1f;
        }
    }

    /// <summary>Estatura (m).</summary>
    public float BodyHeight { get { Cache(); return _body != null ? 1.9f * _body.lossyScale.y : PivotToFeet * 1.75f; } }

    // ── Interno ───────────────────────────────────────────────────────────────────────────────────

    static readonly BodyPart[] Parts =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head,
        BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft, BodyPart.ForearmRight, BodyPart.HandLeft, BodyPart.HandRight,
        BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };

    readonly Dictionary<BodyPart, CubeJoint> _j = new Dictionary<BodyPart, CubeJoint>();
    readonly Dictionary<BodyPart, Quaternion> _cur = new Dictionary<BodyPart, Quaternion>();
    readonly Dictionary<BodyPart, Quaternion> _tgt = new Dictionary<BodyPart, Quaternion>();
    Transform _body, _hips, _head;
    CreatureRig _rig;
    CharacterController _cc;
    Anima _anima;
    bool _cached, _isPlayer, _applied;
    Vector3 _last;
    float _w, _pitch, _phase, _drivenAt = -999f, _nextAuto;

    void Cache()
    {
        if (_cached) return;
        _cached = true;
        _body = transform.Find(CubeHumanoid.BodyName);
        TryGetComponent(out _rig); TryGetComponent(out _cc); TryGetComponent(out _anima);
        _isPlayer = CompareTag("Player");
        _j.Clear();
        // Solo cuerpos humanos de cubos: los animales (CubeGait) nadan con su propia ondulación.
        if (_body != null && GetComponent<CubeGait>() == null)
            foreach (CubeJoint j in _body.GetComponentsInChildren<CubeJoint>(true))
                if (!_j.ContainsKey(j.part) && j.name == j.part.ToString()) _j[j.part] = j;
        _hips = _j.TryGetValue(BodyPart.Hips, out CubeJoint h) && _j.ContainsKey(BodyPart.ShoulderLeft) && _j.ContainsKey(BodyPart.ThighLeft) ? h.transform : null;
        _head = _j.TryGetValue(BodyPart.Head, out CubeJoint hd) ? hd.transform : null;
        foreach (BodyPart p in Parts) _cur[p] = Quaternion.identity;
    }

    void Awake() { _last = transform.position; }

    void RefreshSurface()
    {
        Cache();
        WaterZone z = WaterZone.At(new Vector3(transform.position.x, transform.position.y - PivotToFeet + 0.05f, transform.position.z));
        if (z != null) Surface = z.Surface;
    }

    void Move(Vector3 delta)
    {
        Cache();
        if (_cc != null && _cc.enabled) _cc.Move(delta);
        else transform.position += delta;
    }

    float HoldSeconds => breathHoldSeconds * Mathf.Clamp(_anima != null ? _anima.endurance : 1f, 0.6f, 3f);

    /// <summary>Dónde queda la cabeza (el centro del cubo de la cabeza; sin cuerpo, 0,7 m sobre el pivote).</summary>
    public Vector3 HeadPosition { get { Cache(); return _head != null ? _head.TransformPoint(new Vector3(0f, 0.13f, 0f)) : transform.position + Vector3.up * 0.7f; } }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        Cache();
        Vector3 v = (transform.position - _last) / dt; _last = transform.position;
        if (v.sqrMagnitude > 400f) v = Vector3.zero;   // un salto de sitio (escena, tanque): no es nadar
        Velocity = Vector3.Lerp(Velocity, v, 1f - Mathf.Exp(-6f * dt));

        // Nadie lo conduce y ya no está en el agua (lo sacaron por una puerta, un teletransporte…): deja de nadar.
        if (Active && Time.time - _drivenAt > 0.5f && Time.time >= _nextAuto)
        {
            _nextAuto = Time.time + 0.25f;
            if (!ProbeHere().inWater) End();
        }
        if (Active) RefreshSurface();

        Breathe(dt);
        Animate(dt);
        if (_isPlayer) Tint();
    }

    void Breathe(float dt)
    {
        if (!Active && Breath01 >= 1f) { HeadAboveWater = true; return; }
        Vector3 head = HeadPosition;
        HeadAboveWater = !(WaterZone.SurfaceAt(head, out float s) && head.y < s - 0.02f);
        Breath01 = HeadAboveWater ? Mathf.Min(1f, Breath01 + dt / Mathf.Max(0.1f, breathRefillSeconds))
                                  : Mathf.Max(0f, Breath01 - dt / Mathf.Max(1f, HoldSeconds));
    }

    // ── Brazadas ──────────────────────────────────────────────────────────────────────────────────

    void Animate(float dt)
    {
        if (_hips == null) return;
        _w = Mathf.MoveTowards(_w, Active ? 1f : 0f, dt / 0.35f);
        if (_w <= 0f)
        {
            if (_applied) { foreach (var kv in _j) if (System.Array.IndexOf(Parts, kv.Key) >= 0 && kv.Value != null) kv.Value.transform.localRotation = kv.Value.RestRotation; _applied = false; }
            Style = SwimStyle.None;
            return;
        }

        Vector3 vel = Velocity;
        float horiz = new Vector2(vel.x, vel.z).magnitude, speed = vel.magnitude;
        float floatY = FloatPivotY;
        bool atSurface = !float.IsNaN(Surface) && transform.position.y > floatY - 0.35f;
        SwimStyle style;
        if (!Active) style = Style == SwimStyle.None ? SwimStyle.Tread : Style;
        else if (speed < 0.25f) style = SwimStyle.Tread;
        else if (!atSurface || vel.y < -0.3f) style = SwimStyle.Underwater;
        else style = speed > crawlAbove ? SwimStyle.Crawl : SwimStyle.Breaststroke;
        Style = style;

        // Inclinación del cuerpo: horizontal en la superficie, de cabeza al bajar, más erguido al subir, vertical parado.
        float pitchTarget;
        switch (style)
        {
            case SwimStyle.Tread: pitchTarget = 12f; break;
            case SwimStyle.Crawl: pitchTarget = 84f; break;
            case SwimStyle.Breaststroke: pitchTarget = 78f; break;
            default: pitchTarget = Mathf.Clamp(90f + Mathf.Atan2(-vel.y, Mathf.Max(horiz, 0.05f)) * Mathf.Rad2Deg, 5f, 175f); break;
        }
        _pitch = Mathf.Lerp(_pitch, pitchTarget, 1f - Mathf.Exp(-4f * dt));

        // Ritmo: un ciclo cada «largo de brazada» recorrido (crol ~1,9 m, braza ~1,4 m); a flote, un remolino tranquilo.
        float strokeLen = style == SwimStyle.Crawl ? 1.9f : 1.4f;
        StrokeRate = style == SwimStyle.Tread ? 0.7f : Mathf.Clamp(speed / strokeLen, 0.35f, 1.8f);
        float before = _phase;
        _phase += StrokeRate * dt;
        if (Mathf.Floor(_phase) > Mathf.Floor(before)) Strokes++;
        float u = _phase - Mathf.Floor(_phase);

        _tgt.Clear();
        float roll = 0f;
        switch (style)
        {
            case SwimStyle.Crawl: Crawl(u, out roll); break;
            case SwimStyle.Tread: Tread(u); break;
            default: Breast(u, style == SwimStyle.Underwater); break;
        }
        _tgt[BodyPart.Hips] = Quaternion.Euler(_pitch, 0f, 0f) * Quaternion.Euler(0f, roll, 0f);

        float k = 1f - Mathf.Exp(-14f * dt);
        foreach (BodyPart p in Parts)
        {
            if (!_j.TryGetValue(p, out CubeJoint j) || j == null) continue;
            Quaternion t = _tgt.TryGetValue(p, out Quaternion q) ? q : Quaternion.identity;
            Quaternion c = Quaternion.Slerp(_cur[p], t, k);
            _cur[p] = c;
            j.transform.localRotation = j.RestRotation * Quaternion.Slerp(Quaternion.identity, c, _w);
            if (_rig != null) _rig.LockPose(p, 0.25f);   // el andar, la emoción y la mirada ceden estos huesos
        }
        _applied = true;
    }

    void T(BodyPart p, float x, float y = 0f, float z = 0f) => _tgt[p] = Quaternion.Euler(x, y, z);

    // Convenios del cuerpo de cubos (como CubeWalkCycle): hombro X+ lleva el brazo hacia la espalda y, pasado 180°, por
    // delante; antebrazo X− dobla el codo hacia delante; muslo X− flexiona la cadera; espinilla X+ dobla la rodilla; pie
    // X+ estira la punta. Con el cuerpo boca abajo, «la espalda» queda hacia el cielo.

    // CROL: brazos alternos en molino (recobro por encima del agua con el codo alto, tirón por debajo), giro del cuerpo con
    // cada brazada, la cabeza que gira para respirar y batido de piernas (seis patadas por ciclo).
    void Crawl(float u, out float roll)
    {
        float th = u * Mathf.PI * 2f;
        float deg = u * 360f;
        T(BodyPart.ShoulderLeft, deg);
        T(BodyPart.ShoulderRight, deg + 180f);
        float sL = Mathf.Sin(th), sR = -sL;
        T(BodyPart.ForearmLeft, sL > 0f ? -70f * sL : -30f * -sL);
        T(BodyPart.ForearmRight, sR > 0f ? -70f * sR : -30f * -sR);
        roll = 25f * Mathf.Cos(th);
        float breathe = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(th)), 2f);   // respira hacia el lado del brazo que recobra
        T(BodyPart.Neck, -22f);
        T(BodyPart.Head, -18f, 55f * breathe, 0f);
        float kick = Mathf.Sin(th * 3f);
        T(BodyPart.ThighLeft, 16f * kick);
        T(BodyPart.ThighRight, -16f * kick);
        T(BodyPart.ShinLeft, 22f * Mathf.Max(0f, kick));
        T(BodyPart.ShinRight, 22f * Mathf.Max(0f, -kick));
        T(BodyPart.FootLeft, 55f); T(BodyPart.FootRight, 55f);
    }

    // BRAZA: tirón (las manos abren y bajan), recobro (se juntan bajo el pecho y avanzan), patada de rana (talones al
    // culo, abrir y cerrar) y deslizamiento estirado. En la superficie el pecho sube en el tirón para respirar.
    void Breast(float u, bool underwater)
    {
        float sh, out_, fore, thigh, thighOut, shin, lift;
        if (u < 0.35f)        { float a = u / 0.35f;            sh = 180f + 55f * a; out_ = 45f * Mathf.Sin(a * Mathf.PI) + 15f * a; fore = -90f * a; thigh = 0f; thighOut = 0f; shin = 0f; lift = Mathf.Sin(a * Mathf.PI); }
        else if (u < 0.55f)   { float a = (u - 0.35f) / 0.2f;   sh = 235f - 55f * a; out_ = 15f * (1f - a); fore = -90f * (1f - a); thigh = -45f * a; thighOut = 12f * a; shin = 115f * a; lift = 0f; }
        else if (u < 0.7f)    { float a = (u - 0.55f) / 0.15f;  sh = 180f; out_ = 0f; fore = 0f; thigh = -45f * (1f - a); thighOut = 12f + 25f * Mathf.Sin(a * Mathf.PI) - 12f * a; shin = 115f * (1f - a); lift = 0f; }
        else                  { sh = 180f; out_ = 0f; fore = 0f; thigh = 0f; thighOut = 0f; shin = 0f; lift = 0f; }
        T(BodyPart.ShoulderLeft, sh, 0f, -out_);
        T(BodyPart.ShoulderRight, sh, 0f, out_);
        T(BodyPart.ForearmLeft, fore); T(BodyPart.ForearmRight, fore);
        T(BodyPart.ThighLeft, thigh, 0f, -thighOut); T(BodyPart.ThighRight, thigh, 0f, thighOut);
        T(BodyPart.ShinLeft, shin); T(BodyPart.ShinRight, shin);
        float feet = u >= 0.35f && u < 0.6f ? -20f : 50f;   // pies en flexión para empujar el agua, luego en punta
        T(BodyPart.FootLeft, feet); T(BodyPart.FootRight, feet);
        if (underwater) { T(BodyPart.Neck, -8f); T(BodyPart.Head, -8f); }
        else { T(BodyPart.Back, -6f * lift); T(BodyPart.Chest, -8f * lift); T(BodyPart.Neck, -22f - 8f * lift); T(BodyPart.Head, -20f - 8f * lift); }
    }

    // A FLOTE (parado): el cuerpo casi vertical, los brazos abiertos remando en ochos y las piernas de «batidora»
    // (cada pierna da círculos, alternas), sin desplazarse.
    void Tread(float u)
    {
        float th = u * Mathf.PI * 2f, s = Mathf.Sin(th), c = Mathf.Cos(th);
        T(BodyPart.ShoulderLeft, -25f + 15f * c, 0f, -(50f + 12f * s));
        T(BodyPart.ShoulderRight, -25f + 15f * c, 0f, 50f + 12f * s);
        T(BodyPart.ForearmLeft, -55f - 10f * s); T(BodyPart.ForearmRight, -55f - 10f * s);
        T(BodyPart.ThighLeft, -50f + 10f * s, 15f * c, -25f);
        T(BodyPart.ThighRight, -50f - 10f * s, -15f * c, 25f);
        T(BodyPart.ShinLeft, 85f + 20f * s); T(BodyPart.ShinRight, 85f - 20f * s);
        T(BodyPart.FootLeft, -15f); T(BodyPart.FootRight, -15f);
        T(BodyPart.Neck, -6f); T(BodyPart.Head, -4f);
    }

    // ── Bajo el agua: tinte y aliento en pantalla (solo el jugador) ──────────────────────────────

    static bool _tinted, _savedFog;
    static Color _savedFogColor;
    static float _savedFogDensity;
    static FogMode _savedFogMode;

    void Tint()
    {
        Camera cam = Camera.main;
        WaterZone z = cam != null ? WaterZone.At(cam.transform.position) : null;
        bool under = z != null && cam.transform.position.y < z.Surface;
        if (under == _tinted) return;
        if (under)
        {
            _savedFog = RenderSettings.fog; _savedFogColor = RenderSettings.fogColor; _savedFogDensity = RenderSettings.fogDensity; _savedFogMode = RenderSettings.fogMode;
            RenderSettings.fog = true; RenderSettings.fogColor = z.underwaterFogColor; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = 0.045f;
        }
        else Untint();
        _tinted = under;
    }

    static void Untint()
    {
        if (!_tinted) return;
        RenderSettings.fog = _savedFog; RenderSettings.fogColor = _savedFogColor; RenderSettings.fogDensity = _savedFogDensity; RenderSettings.fogMode = _savedFogMode;
        _tinted = false;
    }

    void OnDisable()
    {
        if (_isPlayer) Untint();
        if (_applied) { foreach (var kv in _j) if (kv.Value != null) kv.Value.transform.localRotation = kv.Value.RestRotation; _applied = false; }
        Active = false; _w = 0f;
    }

    void OnGUI()
    {
        if (!_isPlayer || (!Active && Breath01 >= 0.999f) || (HeadAboveWater && Breath01 >= 0.999f)) return;
        float w = 220f, h = 14f, x = (Screen.width - w) * 0.5f, y = Screen.height - 120f;
        GUI.color = new Color(0f, 0f, 0f, 0.5f); GUI.DrawTexture(new Rect(x - 2f, y - 2f, w + 4f, h + 4f), Texture2D.whiteTexture);
        GUI.color = Breath01 > 0.25f ? new Color(0.55f, 0.85f, 1f, 0.9f) : new Color(1f, 0.4f, 0.3f, 0.95f);
        GUI.DrawTexture(new Rect(x, y, w * Breath01, h), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y - 20f, w, 20f), OutOfBreath ? "Aliento: ¡sin aire, sube!" : "Aliento");
    }
}
