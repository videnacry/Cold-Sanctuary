using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ORQUESTA (coreografía; Berón 2026-10-29): en vez de que uno ARRASTRE al otro de la mano («jalones»: lo teletransportaba
/// detrás de sí paso a paso, se tropezaban y en el lavabotas quedaban las piernas bajo el suelo), cada participante tiene
/// su PARTITURA: sabe a qué sitios va, en orden, qué postura hace allí y a qué señal espera. Nadie mueve el cuerpo de otro:
/// cada uno anda SOLO (<see cref="PathMover"/>, a paso de andar: las piernas se animan; alfombras incluidas), se coloca y
/// ESPERA su señal. Las señales las dan las animaciones del trabajador («hecho» al terminar su gesto): así van al RITMO
/// del trabajador y nunca chocan.
///
/// La partitura son MOVIMIENTOS; cada movimiento da a cada PAPEL (trabajador, recién llegado, la Magnate…) su lista de
/// entradas (<see cref="Kind"/>): ir a (sitio + mirar a), postura (fotogramas A/B con su periodo), esperar una señal, dar
/// una señal, quedarse unos segundos, hacer algo (un instante) o esperar a que algo se cumpla. Los papeles avanzan cada uno
/// a su aire; el movimiento siguiente empieza cuando TODOS acabaron el actual (y las señales son de su movimiento).
/// Teletransportes: ninguno, salvo el de emergencia si alguien lleva <see cref="StuckSeconds"/> s sin poder dar un paso
/// (se registra y se cuenta en <see cref="Fallbacks"/>; en una partida normal, 0). Al jugador se le sujeta el cuerpo
/// (<see cref="PlayerController.HoldBody"/>) mientras está en la orquesta; la cámara sigue libre.
/// </summary>
public class Orchestra
{
    public enum Kind { GoTo, Pose, WaitFor, Emit, Hold, Do, Until }

    public class Cue
    {
        public Kind kind;
        public string label;
        public Func<Vector3> at, face;              // GoTo: adónde; GoTo/Pose: hacia dónde mirar
        public Func<AsanaPoseSpell> a, b;           // Pose: los dos fotogramas (b null = solo a)
        public float period = 2f, seconds = -1f;    // Pose/Hold: cada cuánto alterna y cuánto dura (−1 = hasta `trigger`)
        public string trigger;                      // WaitFor/Emit: la señal; Pose: la señal que la termina
        public Action act;                          // Do
        public Func<bool> until;                    // Until
    }

    public class Role
    {
        public readonly string name;
        public readonly Transform body;
        public readonly Anima anima;
        internal int cue = -1;
        internal bool started, frameB, held;
        internal float cueStart, nextFrame, lastMove, warnAt;
        internal Vector3 pin, anchor, from;
        internal bool pinned;
        /// <summary>¿Está andando ahora mismo (una entrada «ir a» sin terminar)?</summary>
        public bool Walking { get; internal set; }
        /// <summary>Lo que hace ahora (para el registro y las pruebas).</summary>
        public string Doing { get; internal set; } = "";
        internal float maxBlend, walked, best; internal int reversals, frames, sideSign; internal Vector3 lastSample, lastDir; internal bool fellBack;
        internal Vector3 goalWas = new Vector3(float.NaN, 0f, 0f);   // el sitio al que iba (si se corre, se recalcula el camino)
        internal Role(string n, Transform t) { name = n; body = t; anima = t != null ? t.GetComponentInParent<Anima>() : null; }
    }

    class Mov { public string name; public readonly Dictionary<Role, List<Cue>> parts = new Dictionary<Role, List<Cue>>(); }

    // ── Estadísticas (para las pruebas) ──
    /// <summary>Teletransportes de emergencia desde que arrancó el juego (alguien atascado <see cref="StuckSeconds"/> s).</summary>
    public static int Fallbacks { get; private set; }
    /// <summary>Entradas «ir a» en que se anduvo de verdad (≥ 0,6 m) y cuántas de ellas con las piernas animadas.</summary>
    public static int WalksDone { get; private set; }
    public static int WalksAnimated { get; private set; }
    public static void ResetStats() { Fallbacks = 0; WalksDone = 0; WalksAnimated = 0; Displaced = 0; Walks.Clear(); }
    /// <summary>Veces que otra cosa movió (más de 1,5 m) a alguien que estaba quieto en su sitio y hubo que devolverlo.</summary>
    public static int Displaced { get; private set; }
    /// <summary>Las orquestas que suenan ahora.</summary>
    public static readonly List<Orchestra> Playing = new List<Orchestra>();

    public const float StuckSeconds = 40f, MinGap = 0.65f;

    /// <summary>¿<paramref name="g"/> está ocupado por una coreografía (participa en una orquesta, inicia a alguien o la
    /// Magnate lo acompaña)? Entonces nadie más lo conduce: ni su merodear ni su jornada.</summary>
    public static bool Busy(GameObject g)
    {
        if (g == null) return false;
        for (int i = 0; i < Playing.Count; i++)
            foreach (Role r in Playing[i]._roles) if (r.body != null && r.body.gameObject == g) return true;
        if (g.TryGetComponent(out NewcomerInitiator ni) && ni.InProgress) return true;
        if (AreaInitiator.Leading(g)) return true;
        return g.TryGetComponent(out Magnate m) && m.IsEscorting;
    }

    // ── Cada tramo andado (para las pruebas: «dar vueltas en círculo») ──
    /// <summary>Un «ir a» terminado: quién, cuánto anduvo de verdad, la distancia en línea recta y cuántas veces dio media
    /// vuelta (giros de más de 120° entre tramos de 0,25 m).</summary>
    public struct WalkRecord { public string who, orchestra, label; public bool player; public float walked, straight; public int reversals; public bool fallback; }
    public static readonly List<WalkRecord> Walks = new List<WalkRecord>();
    /// <summary>¿Ese tramo dio vueltas? (anduvo más de 1,6× la recta + 3 m, o dio media vuelta más de 2 veces)</summary>
    public static bool Circled(WalkRecord w) => w.walked > 1.6f * w.straight + 3f || w.reversals > 2;

    public readonly string title;
    public float walkSpeed = 1.8f;
    readonly List<Role> _roles = new List<Role>();
    readonly List<Mov> _movs = new List<Mov>();
    readonly HashSet<string> _signals = new HashSet<string>();
    int _mov;
    bool _stopped;

    public Orchestra(string title, float walkSpeed = 1.8f) { this.title = title; this.walkSpeed = walkSpeed; }

    public bool Done => _stopped || _mov >= _movs.Count;
    public string MovementName => _mov < _movs.Count ? _movs[_mov].name : "fin";
    public int MovementIndex => _mov;
    public IReadOnlyList<Role> Roles => _roles;

    // ── Escribir la partitura ──────────────────────────────────────────────────────────────────────────────────
    /// <summary>Un participante. Si es el jugador, se le sujeta el cuerpo hasta que la orquesta acaba.</summary>
    public Role Join(string name, Transform body)
    {
        Role r = new Role(name, body);
        _roles.Add(r);
        PlayerController pc = body != null ? body.GetComponent<PlayerController>() : null;
        if (pc != null) { pc.HoldBody(true); r.held = true; }
        else if (body != null && body.TryGetComponent(out ActionQueue q)) { q.ClearOwn(); q.ClearPlayer(); }   // lo que tuviera encolado (ir al baño, merodear…) espera: lo llevaría a otro sitio a mitad
        if (!Playing.Contains(this)) Playing.Add(this);
        return r;
    }

    /// <summary>Empieza un movimiento nuevo: lo que se escriba después va en él.</summary>
    public Orchestra Movement(string name) { _movs.Add(new Mov { name = name }); return this; }

    Orchestra Add(Role r, Cue c)
    {
        if (_movs.Count == 0) Movement("único");
        Mov m = _movs[_movs.Count - 1];
        if (!m.parts.TryGetValue(r, out List<Cue> l)) m.parts[r] = l = new List<Cue>();
        l.Add(c);
        return this;
    }

    public Orchestra GoTo(Role r, Func<Vector3> at, Func<Vector3> face = null, string label = null) => Add(r, new Cue { kind = Kind.GoTo, at = at, face = face, label = label ?? "va a su sitio" });
    public Orchestra Pose(Role r, Func<AsanaPoseSpell> a, Func<AsanaPoseSpell> b, float period, float seconds, string untilSignal = null, Func<Vector3> face = null, string label = null)
        => Add(r, new Cue { kind = Kind.Pose, a = a, b = b, period = Mathf.Max(0.2f, period), seconds = seconds, trigger = untilSignal, face = face, label = label ?? "postura" });
    public Orchestra WaitFor(Role r, string signal) => Add(r, new Cue { kind = Kind.WaitFor, trigger = signal, label = $"espera «{signal}»" });
    public Orchestra Emit(Role r, string signal) => Add(r, new Cue { kind = Kind.Emit, trigger = signal, label = $"señal «{signal}»" });
    public Orchestra Hold(Role r, float seconds, Func<Vector3> face = null) => Add(r, new Cue { kind = Kind.Hold, seconds = seconds, face = face, label = "quieto" });
    public Orchestra Do(Role r, Action act, string label = null) => Add(r, new Cue { kind = Kind.Do, act = act, label = label ?? "hace" });
    public Orchestra Until(Role r, Func<bool> cond, Func<Vector3> face = null, string label = null) => Add(r, new Cue { kind = Kind.Until, until = cond, face = face, label = label ?? "espera" });

    /// <summary>¿Se dio ya esa señal en el movimiento actual?</summary>
    public bool Signaled(string s) => _signals.Contains(s);

    // ── Sonar ──────────────────────────────────────────────────────────────────────────────────────────────────
    /// <summary>Un fotograma: cada papel avanza por su partitura; si todos acabaron el movimiento, empieza el siguiente.</summary>
    public void Tick()
    {
        if (Done) { Finish(); return; }
        Mov m = _movs[_mov];
        bool all = true;
        foreach (Role r in _roles)
        {
            if (_stopped) return;
            if (!m.parts.TryGetValue(r, out List<Cue> l)) continue;
            for (int guard = 0; guard < 24; guard++)   // las entradas instantáneas (señal, hacer) encadenan en el mismo fotograma
            {
                if (r.cue < 0) { r.cue = 0; r.started = false; }
                if (r.cue >= l.Count) break;
                if (!r.started) { r.started = true; r.cueStart = Time.time; Begin(r, l[r.cue]); }
                if (!Run(r, l[r.cue])) break;
                End(r, l[r.cue]);
                r.cue++; r.started = false;
                if (_stopped) return;
            }
            if (r.cue < l.Count) all = false;
        }
        if (!all || _stopped) return;
        _mov++; _signals.Clear();
        foreach (Role r in _roles) { r.cue = -1; r.started = false; }
        if (Done) Finish();
    }

    /// <summary>Tras las animaciones (LateUpdate): quien está quieto en su sitio no se mueve de ahí (su jornada, su navegación
    /// o un empujón lo paseaban a mitad del gesto).</summary>
    public void LateTick()
    {
        if (Done) return;
        foreach (Role r in _roles)
        {
            if (!r.pinned || r.body == null || r.Walking) continue;
            Vector3 d = r.pin - r.body.position; d.y = 0f;
            if (d.sqrMagnitude <= 0.0025f) continue;
            if (d.sqrMagnitude > 2.25f) Displaced++;
            if (d.sqrMagnitude > 2.25f && Time.time >= r.warnAt)
            {
                r.warnAt = Time.time + 2f;
                UnityEngine.AI.NavMeshAgent ag = r.body.GetComponent<UnityEngine.AI.NavMeshAgent>();
                Debug.LogWarning($"[Orquesta {title}] a {r.name} lo movió otra cosa {d.magnitude:0.0} m mientras estaba en su sitio ({r.Doing}): vuelve. Agente: {(ag == null ? "no" : $"{(ag.enabled ? "activo" : "apagado")}, en malla {ag.enabled && ag.isOnNavMesh}, con camino {ag.enabled && ag.isOnNavMesh && ag.hasPath}")}; cola: {QueueHead(r.body)}; de ({r.body.position.x:0.0},{r.body.position.z:0.0}) a ({r.pin.x:0.0},{r.pin.z:0.0}); {(r.warnAt < 0f ? "" : Movers(r.body))}.");
            }
            WorkdayDirector.Teleport(r.body, new Vector3(r.pin.x, r.body.position.y, r.pin.z));
        }
    }

    /// <summary>Corta la orquesta (sin terminarla): suelta al jugador.</summary>
    public void Stop() { _stopped = true; Finish(); }

    /// <summary>Suelta ya el cuerpo del jugador de ese papel (p. ej. justo antes de meterlo en el tanque).</summary>
    public void Release(Role r)
    {
        if (r == null || !r.held) return;
        r.held = false; r.pinned = false;
        PlayerController pc = r.body != null ? r.body.GetComponent<PlayerController>() : null;
        if (pc != null) pc.HoldBody(false);
    }

    bool _finished;
    void Finish()
    {
        if (_finished) return;
        _finished = true;
        foreach (Role r in _roles) { Release(r); r.Walking = false; }
        Playing.Remove(this);
    }

    void Begin(Role r, Cue c)
    {
        r.Doing = c.label;
        Debug.Log($"[Orquesta {title}] {_movs[_mov].name} · {r.name}: {c.label}.");
        switch (c.kind)
        {
            case Kind.GoTo:
                r.Walking = true; r.pinned = false; r.maxBlend = 0f;
                if (r.body != null) { r.anchor = r.from = r.lastSample = r.body.position; PathMover.Of(r.body).Restart(); }
                r.walked = 0f; r.reversals = 0; r.lastDir = Vector3.zero; r.fellBack = false; r.goalWas = new Vector3(float.NaN, 0f, 0f);
                r.lastMove = Time.time; r.best = float.MaxValue; r.frames = 0;
                if (r.anima != null) DailyPoses.Stand().ApplyFinal(r.anima);   // de pie antes de andar (venía sentado o en un gesto)
                break;
            case Kind.Pose:
                r.frameB = false; r.nextFrame = 0f; Pin(r);
                break;
            case Kind.WaitFor: case Kind.Hold: case Kind.Until:
                Pin(r);
                break;
            case Kind.Emit:
                _signals.Add(c.trigger);
                break;
        }
    }

    static void Pin(Role r) { if (r.body != null) { r.pin = r.body.position; r.pinned = true; } }

    void End(Role r, Cue c)
    {
        if (c.kind != Kind.GoTo) return;
        r.Walking = false;
        if (r.body == null) return;
        Pin(r);
        Vector3 at = r.body.position, goal = c.at != null ? c.at() : at;
        if (!r.fellBack) r.walked += Flat(at - r.lastSample).magnitude;
        float straight = Flat(at - r.from).magnitude;
        var w = new WalkRecord { who = r.name, orchestra = title, label = c.label, player = r.held || (r.body.GetComponent<PlayerController>() != null), walked = r.walked, straight = straight, reversals = r.reversals, fallback = r.fellBack };
        Walks.Add(w);
        string msg = $"[Orquesta {title}] {r.name} llega en {Time.time - r.cueStart:0.0} s: anduvo {r.walked:0.0} m (en línea recta {straight:0.0} m, {r.reversals} medias vueltas), queda a {Flat(goal - at).magnitude:0.00} m de su sitio ({at.x:0.0},{at.y:0.0},{at.z:0.0}).";
        if (Circled(w)) Debug.LogWarning(msg + " ¡DIO VUELTAS!"); else Debug.Log(msg);
        if (straight >= 0.6f && r.frames >= 2)   // (acelerado, un tramo de 1 o 2 fotogramas no da tiempo al ciclo de andar)
        {
            WalksDone++;
            if (r.maxBlend > 0.01f) WalksAnimated++;
            else Debug.LogWarning($"[Orquesta {title}] {r.name} anduvo sin mover las piernas (¿sin ciclo de andar?).");
        }
        Face(r, c.face, 1f);
    }

    // ¿Terminó esta entrada?
    bool Run(Role r, Cue c)
    {
        float t = Time.time - r.cueStart;
        switch (c.kind)
        {
            case Kind.Emit: return true;
            case Kind.Do:
                c.act?.Invoke();
                if (r.pinned && r.body != null) r.pin = r.body.position;   // (sentarse/levantarse lo cambia de sitio él mismo)
                return true;
            case Kind.WaitFor: Face(r, c.face, Time.deltaTime * 6f); return _signals.Contains(c.trigger);
            case Kind.Hold: Face(r, c.face, Time.deltaTime * 6f); return t >= c.seconds;
            case Kind.Until: Face(r, c.face, Time.deltaTime * 6f); return c.until == null || c.until();
            case Kind.Pose:
                if (Time.time >= r.nextFrame && r.anima != null)
                {
                    r.nextFrame = Time.time + c.period;
                    Func<AsanaPoseSpell> f = r.frameB && c.b != null ? c.b : c.a;
                    f?.Invoke()?.ApplyFinal(r.anima);
                    r.frameB = !r.frameB;
                }
                Face(r, c.face, Time.deltaTime * 6f);
                return c.seconds >= 0f ? t >= c.seconds : (c.trigger == null || _signals.Contains(c.trigger));
            case Kind.GoTo: return Walk(r, c, t);
        }
        return true;
    }

    // ANDAR solo hasta su sitio (con PathMover: camino del NavMesh, suelo bajo los pies, alfombras entre pisos).
    bool Walk(Role r, Cue c, float t)
    {
        if (r.body == null || c.at == null) return true;
        Vector3 goal = Free(r, Snap(c.at()));
        Vector3 pos = r.body.position;
        float flat = Flat(goal - pos).magnitude;
        bool samePlane = Mathf.Abs(pos.y - goal.y) < 2.2f;
        if (flat < 0.3f && samePlane) return true;
        float still = Time.time - r.lastMove;
        // Algo en medio (un mueble sobre el sitio, el otro ya colocado): a menos de 1,2 m y sin poder avanzar, ya está.
        // …o da vueltas cerca sin acercarse (lo empuja el otro, que está junto a su sitio): a menos de 2 m y 6 s sin acercarse, ya está.
        if ((still > 3f && flat < 1.2f && samePlane) || (still > 6f && flat < 2f && samePlane)) return true;
        if (still > StuckSeconds || t > StuckSeconds * 6f)
        {
            r.fellBack = true;
            Emergency(r.body, goal, $"[Orquesta {title}] {r.name} lleva {still:0} s sin poder andar hacia su sitio ({_movs[_mov].name})");
            return true;
        }
        // SI SU SITIO SE CORRE (Free: el otro ya se sentó junto a él), camino nuevo YA: PathMover solo recalcula si el destino se
        // mueve más de 3 m o cada 4 s, y seguía hasta el sitio viejo y volvía (Linduarda, 16 m para 8 y dos medias vueltas).
        if (!float.IsNaN(r.goalWas.x) && Flat(goal - r.goalWas).magnitude > 0.4f) PathMover.Of(r.body).Restart();
        r.goalWas = goal;
        PathMover.Of(r.body).StepToward(goal, walkSpeed);
        Separate(r, goal);
        Track(r);
        CubeWalkCycle wc = r.body.GetComponent<CubeWalkCycle>();
        if (wc != null) r.maxBlend = Mathf.Max(r.maxBlend, wc.Blend);
        // PROGRESO: acercarse de verdad a su sitio (0,3 m más cerca que nunca). Lejos (más de 3 m) o en alfombra, también vale
        // moverse (rodear una pared o ir al andén aleja un rato). Cerca, moverse sin acercarse es dar vueltas: no cuenta.
        r.frames++;
        float now = Flat(goal - r.body.position).magnitude;
        bool closer = now < r.best - 0.3f, moved = (r.body.position - r.anchor).sqrMagnitude > 0.09f;
        if (closer) r.best = now;
        if (closer || FlyingCarpet.Riding(r.body.gameObject) || (moved && now > 3f)) { r.anchor = r.body.position; r.lastMove = Time.time; }
        else if (moved) r.anchor = r.body.position;
        return false;
    }

    // Un sitio que no pise a otro: si quien ya está quieto en el suyo queda a menos de MinGap del destino (pasa cuando los dos
    // sitios, junto a un mueble fuera del NavMesh, se ajustan al mismo borde), el destino se corre a su lado. Si no, el que
    // llegaba se apartaba del otro y volvía a su sitio una y otra vez: daba vueltas (Berón 2026-10-29).
    Vector3 Free(Role r, Vector3 goal)
    {
        foreach (Role o in _roles)
        {
            if (o == r || o.body == null || o.Walking) continue;
            Vector3 d = Flat(goal - o.body.position);
            if (d.magnitude >= MinGap + 0.15f || Mathf.Abs(goal.y - o.body.position.y) > 2.2f) continue;
            Vector3 dir = d.sqrMagnitude > 1e-4f ? d.normalized : Flat(r.body.position - o.body.position).normalized;
            if (dir.sqrMagnitude < 0.5f) dir = Vector3.right;
            goal = o.body.position + dir * (MinGap + 0.3f); goal.y = Snap(goal).y;
        }
        return goal;
    }

    // Lo andado de verdad y las medias vueltas (giros > 120° entre tramos de 0,25 m): así se ve si da vueltas en círculo.
    static void Track(Role r)
    {
        if (FlyingCarpet.Riding(r.body.gameObject)) { r.lastSample = r.body.position; r.lastDir = Vector3.zero; return; }   // (en alfombra no anda)
        Vector3 seg = Flat(r.body.position - r.lastSample);
        float m = seg.magnitude;
        if (m < 0.25f) return;
        r.walked += m;
        Vector3 dir = seg / m;
        if (r.lastDir.sqrMagnitude > 0.5f && Vector3.Angle(r.lastDir, dir) > 120f) r.reversals++;
        r.lastDir = dir; r.lastSample = r.body.position;
    }

    // NO CHOCAR: si otro participante le queda a menos de MinGap, se aparta él (solo su propio cuerpo) y lo rodea por un lado.
    void Separate(Role r, Vector3 goal)
    {
        bool touched = false;
        foreach (Role o in _roles)
        {
            if (o == r || o.body == null) continue;
            Vector3 d = Flat(r.body.position - o.body.position);
            float dist = d.magnitude;
            if (dist >= MinGap + 0.25f || Mathf.Abs(r.body.position.y - o.body.position.y) > 1.5f) continue;
            // Si los dos andan, cede solo uno (el que entró después en la orquesta: el trabajador cede al recién llegado); si
            // cedían los dos, cada uno esquivaba hacia un lado distinto en cada fotograma y daban media vuelta (zigzag).
            if (o.Walking && _roles.IndexOf(o) > _roles.IndexOf(r) && dist >= MinGap) continue;
            Vector3 away = dist > 1e-3f ? d / dist : Flat(r.body.right).normalized;
            Vector3 toGoal = Flat(goal - r.body.position);
            // Rodea: de lado, hacia el lado que le acerca a su sitio.
            Vector3 side = Vector3.Cross(Vector3.up, away);
            if (r.sideSign == 0) r.sideSign = Vector3.Dot(side, toGoal) < 0f ? -1 : 1;   // el lado se elige una vez por encuentro
            side *= r.sideSign; touched = true;
            // (Cerca de su sitio no rodea: si el otro está junto a él, daba vueltas a su alrededor; ahí solo se aparta y llega.)
            Vector3 push = toGoal.magnitude > MinGap + 0.8f ? side * walkSpeed * 0.6f * Time.deltaTime : Vector3.zero;
            if (dist < MinGap) push += away * (MinGap - dist);
            PathMover pm = PathMover.Of(r.body);
            Vector3 p = r.body.position + push;
            Vector3 g = pm.OnGround(new Vector3(p.x, r.body.position.y - pm.PivotHeight, p.z));   // (OnGround toma el punto de los pies)
            if (Mathf.Abs(g.y - r.body.position.y) < 0.5f) r.body.position = g;   // sin subirse a un mueble ni caer por un borde
        }
        if (!touched) r.sideSign = 0;
    }

    /// <summary>EL ÚNICO TELETRANSPORTE: el de emergencia, cuando alguien no puede llegar andando tras mucho rato. Lo registra y
    /// lo cuenta (<see cref="Fallbacks"/>); <paramref name="feet"/> es el punto del suelo (los pies quedan en él).</summary>
    public static void Emergency(Transform body, Vector3 feet, string why)
    {
        if (body == null) return;
        Vector3 from = body.position, to = PathMover.Of(body).OnGround(Snap(feet));
        Fallbacks++;
        Debug.LogWarning($"{why}: se coloca (teletransporte de emergencia nº {Fallbacks}) de ({from.x:0.0},{from.y:0.0},{from.z:0.0}) a ({to.x:0.0},{to.y:0.0},{to.z:0.0}).");
        WorkdayDirector.Teleport(body, to);
    }

    // Quién podría moverlo (los componentes activos que no son de la orquesta ni del cuerpo): para encontrar al culpable.
    public static string Movers(Transform t)
    {
        var sb = new System.Text.StringBuilder("activos: ");
        foreach (MonoBehaviour m in t.GetComponents<MonoBehaviour>()) if (m != null && m.enabled) sb.Append(m.GetType().Name).Append(' ');
        return sb.ToString();
    }

    static string QueueHead(Transform t) { ActionQueue q = t.GetComponent<ActionQueue>(); return q == null ? "sin cola" : q.Own.Count > 0 ? "propia: " + q.Own[0].Label : q.Player.Count > 0 ? "jugador: " + q.Player[0].Label : "vacía"; }

    static Vector3 Snap(Vector3 p) => NavMesh.SamplePosition(p, out NavMeshHit h, 4f, NavMesh.AllAreas) ? h.position : p;   // (como PathMover al calcular el camino)

    static void Face(Role r, Func<Vector3> face, float k)
    {
        if (r.body == null || face == null) return;
        Vector3 d = Flat(face() - r.body.position);
        if (d.sqrMagnitude < 1e-4f) return;
        r.body.rotation = Quaternion.Slerp(r.body.rotation, Quaternion.LookRotation(d.normalized), Mathf.Clamp01(k));
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }
}
