using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HECHIZO-POSTURA de UNA parte del cuerpo (docs/typed-spells-and-queues.md §4.1): manda una señal a un `BodyPart` para
/// que adopte una orientación. Si el anima **no tiene** esa parte (el `CreatureRig` no la mapea), la señal no hace nada
/// y lo dice ("no tengo X disponible"). Es el ladrillo de las posturas de yoga escritas por el jugador
/// (p. ej. espaldaRecta / hombrosAbajo). Rota el hueso del rig hacia el objetivo durante `hold` segundos.
/// </summary>
public class PostureSpell : QueuedSpell
{
    public BodyPart part;
    public Vector3 localEuler;
    public float hold = 1.2f;
    public float lerp = 4f;
    float _until = -1f;

    public override string Label => $"postura {part}";

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        CreatureRig rig = self.GetComponent<CreatureRig>();
        Transform bone = rig != null ? rig.Get(part) : null;
        if (bone == null) { Debug.Log($"[Yoga] «{self.name}»: no tengo {part} disponible."); return true; }
        if (_until < 0f) _until = Time.time + hold;
        rig.LockPose(part);   // mientras coloco este hueso, la emoción/andar no lo pisan (evita el bucle entrar/salir)
        bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(localEuler), lerp * Time.deltaTime);
        return Time.time >= _until;
    }
}

/// <summary>Una ASANA = varias partes a la vez (colas en paralelo por bodyPart, §4.1). En un tiempo TOTAL `duration`,
/// el `reachFraction` inicial (30%) LLEGA a la postura (desde la actual, con aceleración y frenado) y el resto (70%) la
/// MANTIENE quieta. Subir/bajar `duration` conserva el 30/70. Mientras coloca cada hueso lo BLOQUEA en el rig, para que
/// la emoción (`BodyPartReactor`) y el andar (`CubeWalkCycle`) no lo pisen (si no, la asana "entra y sale" en bucle).
/// `rootHeight` (opcional, en unidades del cuerpo; la cadera de pie está a ~0,95) baja o sube la pelvis para las
/// posturas en el suelo (plancha, cobra…).</summary>
public class AsanaPoseSpell : QueuedSpell
{
    public string asanaName = "asana";
    public (BodyPart part, Vector3 euler)[] parts = System.Array.Empty<(BodyPart, Vector3)>();
    public float duration = 10f;          // tiempo TOTAL de la asana (llegar + sostener)
    [Range(0.03f, 1f)] public float reachFraction = 0.3f;   // fracción del tiempo en LLEGAR a la postura; el resto, SOSTENIÉNDOLA (asanas 0.3; upa-yoga ≈1 = el rezo tarda toda la respiración)
    public float rootHeight = -1f;        // -1 = la de pie (reposo)
    public float stanceWidth = -1f;       // anchura de piernas (distancia entre pies); 0 = talones juntos; -1 = no tocar
    [Tooltip("Apoyar el punto más bajo del cuerpo en el suelo (y = 0 del cuerpo). false = la pelvis queda en rootHeight tal cual: colgado de una barra, sobre un banco o un cajón (gimnasio).")]
    public bool ground = true;
    public (BodyPart end, Vector3 localTarget)[] reaches = System.Array.Empty<(BodyPart, Vector3)>();   // IK: lleva mano/pie a un punto (palmas juntas, dedos al suelo)
    float _start = -1f;
    Quaternion[] _from;
    Vector3 _rootFrom, _thighFromL, _thighFromR;

    public override string Label => $"asana {asanaName}";

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        CreatureRig rig = self.GetComponent<CreatureRig>();
        if (_start < 0f)
        {
            _start = Time.time;
            _from = new Quaternion[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                Transform b = rig != null ? rig.Get(parts[i].part) : null;
                _from[i] = b != null ? b.localRotation : Quaternion.identity;
            }
            Transform hips = rig != null ? rig.Get(BodyPart.Hips) : null;
            if (hips != null) _rootFrom = hips.localPosition;
            Transform thL = rig != null ? rig.Get(BodyPart.ThighLeft) : null;
            Transform thR = rig != null ? rig.Get(BodyPart.ThighRight) : null;
            if (thL != null) _thighFromL = thL.localPosition;
            if (thR != null) _thighFromR = thR.localPosition;
        }
        // Transición = reachFraction del total; el resto es estancamiento (t se queda en 1). Así subir/bajar `duration`
        // mantiene el 30/70 sin cambiar nada más. Entra con aceleración y frena (SmoothStep).
        float transition = Mathf.Max(0.01f, reachFraction * duration);
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Time.time - _start) / transition));
        Apply(self, rig, t);
        return Time.time - _start >= duration;
    }

    /// <summary>Coloca el cuerpo en la postura FINAL de golpe (sin transición): galería de posturas y pruebas visuales.</summary>
    public void ApplyFinal(Anima self)
    {
        CreatureRig rig = self.GetComponent<CreatureRig>();
        if (rig == null) return;
        Transform hips = rig.Get(BodyPart.Hips); if (hips != null) _rootFrom = hips.localPosition;
        Transform thL = rig.Get(BodyPart.ThighLeft), thR = rig.Get(BodyPart.ThighRight);
        if (thL != null) _thighFromL = thL.localPosition;
        if (thR != null) _thighFromR = thR.localPosition;
        _from = new Quaternion[parts.Length];
        for (int i = 0; i < parts.Length; i++) _from[i] = Quaternion.identity;
        Apply(self, rig, 1f);
    }

    void Apply(Anima self, CreatureRig rig, float t)
    {
        if (rig != null)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                rig.LockPose(parts[i].part);   // el yoga manda sobre este hueso: ni emoción ni andar lo pisan (sin bucle)
                Transform bone = rig.Get(parts[i].part);
                if (bone != null) bone.localRotation = Quaternion.Slerp(_from[i], Quaternion.Euler(parts[i].euler), t);
            }
            Transform hips = rig.Get(BodyPart.Hips);
            CubeJoint hj = hips != null ? hips.GetComponent<CubeJoint>() : null;
            if (hj != null)
            {
                Vector3 rest = hj.RestPosition;
                Vector3 goal = rootHeight >= 0f ? new Vector3(rest.x, rootHeight, rest.z) : rest;
                hips.localPosition = Vector3.Lerp(_rootFrom, goal, t);
            }

            // STANCE: anchura de piernas (talones juntos = 0). Mueve la RAÍZ de los muslos lateralmente.
            if (stanceWidth >= 0f)
            {
                Stance(rig, BodyPart.ThighLeft, -stanceWidth * 0.5f, _thighFromL, t);
                Stance(rig, BodyPart.ThighRight, stanceWidth * 0.5f, _thighFromR, t);
            }

            // ALCANCE (IK de 2 huesos): lleva mano/pie a un punto del cuerpo (palmas juntas, dedos al suelo…).
            if (reaches != null && reaches.Length > 0)
            {
                Transform root = hips != null && hips.parent != null ? hips.parent : self.transform;
                foreach (var rc in reaches) Reach(rig, root, rc.end, rc.localTarget, t);
            }

            // APOYO EN EL SUELO: el punto más bajo del cuerpo toca el suelo (y = 0 del cuerpo). Sin esto las posturas
            // sentadas flotaban (piernas dobladas con la pelvis a la altura de pie) y la silla se hundía en el suelo.
            if (hj != null && ground) Ground(rig, hips);
            // Las manos/pies que van AL SUELO se vuelven a colocar tras apoyar el cuerpo (el apoyo mueve la pelvis).
            if (hj != null && ground && reaches != null && reaches.Length > 0)
            {
                Transform root = hips.parent != null ? hips.parent : self.transform;
                bool any = false;
                foreach (var rc in reaches) if (rc.localTarget.y < 0.3f) { Reach(rig, root, rc.end, rc.localTarget, t); any = true; }
                if (any) Ground(rig, hips);
            }
        }
    }


    static void Ground(CreatureRig rig, Transform hips)
    {
        Transform body = hips.parent; if (body == null) return;
        float lowest = float.MaxValue;
        foreach (CubeJoint j in hips.GetComponentsInChildren<CubeJoint>())
        {
            Transform s = j.segment; if (s == null) continue;
            for (int c = 0; c < 8; c++)
            {
                Vector3 corner = new Vector3((c & 1) == 0 ? -0.5f : 0.5f, (c & 2) == 0 ? -0.5f : 0.5f, (c & 4) == 0 ? -0.5f : 0.5f);
                lowest = Mathf.Min(lowest, body.InverseTransformPoint(s.TransformPoint(corner)).y);
            }
        }
        if (lowest < float.MaxValue) hips.localPosition -= new Vector3(0f, lowest, 0f);
    }
    static void Stance(CreatureRig rig, BodyPart part, float targetX, Vector3 from, float t)
    {
        Transform th = rig.Get(part); if (th == null) return;
        CubeJoint cj = th.GetComponent<CubeJoint>();
        Vector3 rest = cj != null ? cj.RestPosition : th.localPosition;
        // "Talones juntos" = piernas UNA AL LADO DE LA OTRA, no en el mismo sitio (antes las dos se metían en el centro).
        float minX = (cj != null && cj.segment != null ? cj.segment.localScale.x * 0.5f : 0.07f) + 0.005f;
        if (Mathf.Abs(targetX) < minX) targetX = Mathf.Sign(rest.x) * minX;
        th.localPosition = Vector3.Lerp(from, new Vector3(targetX, rest.y, rest.z), t);
        rig.LockPose(part);
    }

    static readonly Vector3 ChestRest = new Vector3(0f, 1.29f, 0f);   // pivote del pecho de pie (cadera 0,95 + lumbar 0,1 + 0,24)

    static void Reach(CreatureRig rig, Transform root, BodyPart end, Vector3 localTarget, float t)
    {
        BodyPart midP, rootP;
        switch (end)
        {
            case BodyPart.HandLeft:  midP = BodyPart.ForearmLeft;  rootP = BodyPart.ShoulderLeft;  break;
            case BodyPart.HandRight: midP = BodyPart.ForearmRight; rootP = BodyPart.ShoulderRight; break;
            case BodyPart.FootLeft:  midP = BodyPart.ShinLeft;     rootP = BodyPart.ThighLeft;     break;
            case BodyPart.FootRight: midP = BodyPart.ShinRight;    rootP = BodyPart.ThighRight;    break;
            default: return;
        }
        Transform endT = rig.Get(end), midT = rig.Get(midP), rootT = rig.Get(rootP);
        if (endT == null || midT == null || rootT == null) return;
        // Al SUELO (y < 0,3): punto fijo del cuerpo. Al CUERPO (rezo al pecho, sobre la cabeza…): relativo al PECHO, que
        // sigue a la pelvis y a la columna (en cuclillas o inclinado las manos no atraviesan la cabeza ni el torso).
        Transform chest = rig.Get(BodyPart.Chest);
        Vector3 world = localTarget.y < 0.3f || chest == null ? root.TransformPoint(localTarget) : chest.TransformPoint(localTarget - ChestRest);
        bool arm = end == BodyPart.HandLeft || end == BodyPart.HandRight;
        // Codo hacia FUERA y abajo (antes hacia atrás: en el rezo se metía dentro del torso); rodilla hacia delante.
        float side = Mathf.Sign(root.InverseTransformPoint(rootT.position).x);
        Vector3 pole = midT.position + (arm ? (root.right * side * 0.35f - root.up * 0.2f) : (root.forward * 0.3f));
        TwoBoneIK.Solve(rootT, midT, endT, world, pole, t);
        rig.LockPose(rootP); rig.LockPose(midP); rig.LockPose(end);
    }
}

/// <summary>SALUDO AL SOL (Surya Namaskar) — las 12 asanas reales, en código (no como ScriptableObject). Devuelve la
/// secuencia de <see cref="AsanaPoseSpell"/> para encolar. Las rotaciones por hueso (Chest/Neck/Shoulders/Feet del
/// `CreatureRig`) son aproximadas (el compañero afina en Unity); lo exacto es el ORDEN y los grupos musculares.</summary>
public static class SunSalutation
{
    // Cada pose fija TODAS las articulaciones del cuerpo de cubos (las no nombradas vuelven a 0 = de pie) y la altura de
    // la pelvis. Ejes (rotación local, grados): X+ flexiona hacia DELANTE (columna dobla el torso; muslo va hacia atrás;
    // espinilla dobla la rodilla), X- lleva el miembro hacia DELANTE; Z abre/eleva los brazos por los lados.
    // Hips (la pelvis, raíz del cuerpo) X+ inclina el cuerpo entero hacia delante: 80–90 = tumbado boca abajo.
    // EL YOGA VA POR RESPIRACIONES, no por tiempo fijo. Una respiración = inhalación (8 s) + exhalación (8 s).
    // Cada cambio de postura ocurre EN UNA respiración (una fase de 8 s) y la postura se MANTIENE n respiraciones.
    // Saludo al Sol = 1 respiración por postura → 1 fase para llegar + 2 fases de mantenimiento = 24 s/postura.
    public const float Breath = 8f;   // una fase (inhalación O exhalación)
    const float Stand = -1f;   // pelvis a la altura de pie

    static readonly BodyPart[] Joints =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head,
        BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft, BodyPart.ForearmRight, BodyPart.HandLeft, BodyPart.HandRight,
        BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };

    static AsanaPoseSpell Pose(string name, float duration, float reach, float rootHeight, (BodyPart, Vector3)[] set,
                              float stanceWidth = -1f, (BodyPart, Vector3)[] reaches = null)
    {
        var all = new (BodyPart, Vector3)[Joints.Length];
        for (int i = 0; i < Joints.Length; i++)
        {
            Vector3 e = Vector3.zero;
            foreach (var s in set) if (s.Item1 == Joints[i]) e = s.Item2;
            all[i] = (Joints[i], e);
        }
        return new AsanaPoseSpell { asanaName = name, parts = all, duration = duration, reachFraction = reach, rootHeight = rootHeight,
                                    stanceWidth = stanceWidth, reaches = reaches ?? System.Array.Empty<(BodyPart, Vector3)>() };
    }

    static (BodyPart, Vector3) J(BodyPart p, float x, float z = 0f) => (p, new Vector3(x, 0f, z));
    static (BodyPart, Vector3)[] Join(params (BodyPart, Vector3)[][] groups)
    {
        var l = new List<(BodyPart, Vector3)>();
        foreach (var g in groups) l.AddRange(g);
        return l.ToArray();
    }
    // La columna reparte la flexión: 55 % lumbar, 45 % torácica.
    static (BodyPart, Vector3)[] Spine(float x) => new[] { J(BodyPart.Back, x * 0.55f), J(BodyPart.Chest, x * 0.45f) };
    // Brazos arriba por FLEXIÓN (X−, suben por delante hasta la vertical): quedan PARALELOS a la anchura de los hombros
    // (en Hasta Uttanasana hay un hueco entre manos y codos = la distancia entre hombros). Antes se abrían en lateral (Z)
    // y convergían/abrían mal; para el perro boca abajo, además, así quedan EN LÍNEA con el torso plegado.
    static (BodyPart, Vector3)[] ArmsUp => new[] { J(BodyPart.ShoulderLeft, -175), J(BodyPart.ShoulderRight, -175) };
    static (BodyPart, Vector3)[] Prayer => new[] { J(BodyPart.ShoulderLeft, -25, 12), J(BodyPart.ShoulderRight, -25, -12),
        J(BodyPart.ForearmLeft, -105, -45), J(BodyPart.ForearmRight, -105, 45), J(BodyPart.HandLeft, 0, -20), J(BodyPart.HandRight, 0, 20) };
    // Manos al suelo con el torso inclinado X grados: el brazo apunta al suelo (−X respecto al torso).
    static (BodyPart, Vector3)[] HandsDown(float torsoTilt) => new[] { J(BodyPart.ShoulderLeft, -torsoTilt), J(BodyPart.ShoulderRight, -torsoTilt) };
    static (BodyPart, Vector3)[] Lunge(bool leftForward) => new[]
    {
        J(leftForward ? BodyPart.ThighLeft : BodyPart.ThighRight, -100), J(leftForward ? BodyPart.ShinLeft : BodyPart.ShinRight, 95),
        J(leftForward ? BodyPart.ThighRight : BodyPart.ThighLeft, 55),  J(leftForward ? BodyPart.ShinRight : BodyPart.ShinLeft, 10),
        J(leftForward ? BodyPart.FootRight : BodyPart.FootLeft, 60),
    };

    // Alcances de ejemplo (IK): palmas juntas delante del pecho (Pranamasana) y diez dedos al suelo junto a los pies
    // (Uttanasana). Coordenadas en el cuerpo NATIVO (pies y≈0, cadera y≈0.95). Opt-in: el resto de posturas no usan IK.
    static (BodyPart, Vector3)[] PalmsTogether => new[] { (BodyPart.HandLeft, new Vector3(-0.035f, 1.34f, 0.27f)), (BodyPart.HandRight, new Vector3(0.035f, 1.34f, 0.27f)) };
    static (BodyPart, Vector3)[] HandsToFloor => FloorHands(0.16f, 0.2f);
    /// <summary>Palmas en el suelo a ±<paramref name="x"/> del centro y <paramref name="z"/> por delante de la pelvis.</summary>
    public static (BodyPart, Vector3)[] FloorHands(float x, float z) => new[] { (BodyPart.HandLeft, new Vector3(-x, 0.03f, z)), (BodyPart.HandRight, new Vector3(x, 0.03f, z)) };

    // BISAGRA DE CADERA: la flexión hacia delante sale de la PELVIS (Hips X+), no solo de la columna; los muslos giran al
    // revés para que las piernas sigan verticales. Así el torso baja de verdad y las manos llegan al suelo.
    public static (BodyPart, Vector3)[] Hinge(float hip, float legsFromVertical = 0f) => new[] { J(BodyPart.Hips, hip), J(BodyPart.ThighLeft, legsFromVertical - hip), J(BodyPart.ThighRight, legsFromVertical - hip) };

    // ── Las posturas, una a una (las usan el Saludo al Sol y la Serie Primaria de Ashtanga) ──
    /// <summary>UTTANASANA: piernas rectas, pliegue desde la cadera, el pecho hacia los muslos, la cabeza colgando y las
    /// palmas planas en el suelo a los lados de los pies.</summary>
    public static (BodyPart, Vector3)[] Uttanasana => Join(Hinge(112f), Spine(32), new[] { J(BodyPart.Head, 15) }, HandsDown(140));
    /// <summary>ARDHA UTTANASANA (media flexión): espalda plana y larga, horizontal, mirada al frente; manos a las espinillas.</summary>
    public static (BodyPart, Vector3)[] ArdhaUttanasana => Join(Hinge(88f), Spine(-6), new[] { J(BodyPart.Head, -30) }, HandsDown(88));
    /// <summary>ASHWA SANCHALANASANA (zancada): pie delantero entre las manos (rodilla sobre el tobillo), la pierna de atrás
    /// estirada con la rodilla en el suelo, el torso inclinado sobre el muslo y las palmas en el suelo junto al pie.</summary>
    public static (BodyPart, Vector3)[] Sanchalanasana(bool leftForward) => new[]
    {
        J(BodyPart.Hips, 48f), J(BodyPart.Back, 22f), J(BodyPart.Chest, 18f), J(BodyPart.Head, -45f),
        J(leftForward ? BodyPart.ThighLeft : BodyPart.ThighRight, -140f), J(leftForward ? BodyPart.ShinLeft : BodyPart.ShinRight, 94f),   // espinilla delantera VERTICAL: rodilla sobre el tobillo
        J(leftForward ? BodyPart.ThighRight : BodyPart.ThighLeft, 28f), J(leftForward ? BodyPart.ShinRight : BodyPart.ShinLeft, 30f),
        J(leftForward ? BodyPart.FootRight : BodyPart.FootLeft, 55f),
        J(BodyPart.ShoulderLeft, -88f), J(BodyPart.ShoulderRight, -88f),
    };
    /// <summary>CHATURANGA DANDASANA: cuerpo recto como una tabla, muy cerca del suelo, codos doblados a 90° pegados a las
    /// costillas (antebrazos verticales, manos bajo los codos), sobre las puntas de los pies.</summary>
    public static (BodyPart, Vector3)[] Chaturanga => new[]
    {
        J(BodyPart.Hips, 80f), J(BodyPart.Head, -12f), J(BodyPart.FootLeft, 0f), J(BodyPart.FootRight, 0f),
        J(BodyPart.ShoulderLeft, 20f), J(BodyPart.ShoulderRight, 20f), J(BodyPart.ForearmLeft, -100f), J(BodyPart.ForearmRight, -100f),
    };
    /// <summary>URDHVA MUKHA SVANASANA (perro boca arriba): brazos rectos y verticales, pecho abierto hacia delante y arriba,
    /// espalda arqueada, piernas estiradas con el empeine en el suelo y los muslos despegados, mirada arriba.</summary>
    public static (BodyPart, Vector3)[] UrdhvaMukha => new[]
    {
        J(BodyPart.Hips, 88f), J(BodyPart.Back, -30f), J(BodyPart.Chest, -28f), J(BodyPart.Neck, -10f), J(BodyPart.Head, -25f),
        J(BodyPart.FootLeft, 75f), J(BodyPart.FootRight, 75f), J(BodyPart.ShoulderLeft, -28f), J(BodyPart.ShoulderRight, -28f),
    };
    /// <summary>ADHO MUKHA SVANASANA (perro boca abajo): una V invertida, la cadera en lo más alto; brazos rectos en línea
    /// con la espalda (orejas entre los brazos), piernas rectas con los talones hacia el suelo.</summary>
    public static (BodyPart, Vector3)[] AdhoMukha => Join(Hinge(124f, 36f), new[] { J(BodyPart.Head, 20f), J(BodyPart.ShoulderLeft, -172f), J(BodyPart.ShoulderRight, -172f), J(BodyPart.FootLeft, -25f), J(BodyPart.FootRight, -25f) });

    // Dónde quedan las manos (en el marco del cuerpo de pie; la pelvis en z = 0): delante de los pies, bajo los hombros…
    public static (BodyPart, Vector3)[] UttanasanaHands => FloorHands(0.17f, 0.22f);
    public static (BodyPart, Vector3)[] SanchalanaHands => FloorHands(0.24f, 0.55f);
    public static (BodyPart, Vector3)[] ChaturangaHands => FloorHands(0.2f, 0.42f);
    public static (BodyPart, Vector3)[] UrdhvaHands => FloorHands(0.2f, 0.38f);
    public static (BodyPart, Vector3)[] AdhoMukhaHands => FloorHands(0.2f, 1.0f);

    // Las 12 posturas: altura de pelvis (root), anchura de pies (stance; 0 = talones juntos, −1 = no tocar), huesos y alcance IK.
    static readonly (string name, float root, float stance, System.Func<(BodyPart, Vector3)[]> set, System.Func<(BodyPart, Vector3)[]> reaches)[] Postures =
    {
        ("Pranamasana (oración)",                 Stand, 0f,   () => Prayer, () => PalmsTogether),
        ("Hasta Uttanasana (brazos arriba)",      Stand, 0f,   () => Join(ArmsUp, Spine(-30), new[] { J(BodyPart.Head, -15) }), null),
        ("Uttanasana (flexión adelante)",         Stand, 0f,   () => Uttanasana, () => UttanasanaHands),
        ("Ashwa Sanchalanasana (zancada)",        0.5f,  -1f,  () => Sanchalanasana(true), () => SanchalanaHands),
        ("Dandasana (plancha)",                   0.5f,  -1f,  () => Join(new[] { J(BodyPart.Hips, 72), J(BodyPart.FootLeft, 25), J(BodyPart.FootRight, 25), J(BodyPart.Head, -20) }, HandsDown(72)), null),
        ("Ashtanga Namaskara (ocho miembros)",    0.30f, -1f,  () => Join(new[] { J(BodyPart.Hips, 48), J(BodyPart.ThighLeft, -20), J(BodyPart.ThighRight, -20),
                                                                           J(BodyPart.ShinLeft, 70), J(BodyPart.ShinRight, 70), J(BodyPart.Head, 10),
                                                                           J(BodyPart.ShoulderLeft, -25), J(BodyPart.ShoulderRight, -25), J(BodyPart.ForearmLeft, -110), J(BodyPart.ForearmRight, -110) }, Spine(55)), null),
        ("Bhujangasana (cobra)",                  0.5f,  -1f,  () => Join(new[] { J(BodyPart.Hips, 90), J(BodyPart.FootLeft, 80), J(BodyPart.FootRight, 80), J(BodyPart.Head, -25) }, Spine(-60), HandsDown(30)), null),
        ("Adho Mukha Svanasana (perro boca abajo)", Stand, 0f, () => AdhoMukha, () => AdhoMukhaHands),
        ("Ashwa Sanchalanasana (zancada)",        0.5f,  -1f,  () => Sanchalanasana(false), () => SanchalanaHands),
        ("Uttanasana (flexión adelante)",         Stand, 0f,   () => Uttanasana, () => UttanasanaHands),
        ("Hasta Uttanasana (brazos arriba)",      Stand, 0f,   () => Join(ArmsUp, Spine(-30), new[] { J(BodyPart.Head, -15) }), null),
        ("Pranamasana (oración)",                 Stand, 0f,   () => Prayer, () => PalmsTogether),
    };

    /// <summary>SURYA NAMASKARA A de ASHTANGA (la apertura de la Serie Primaria): Samasthitih, Urdhva Vrksasana, Uttanasana,
    /// Ardha Uttanasana, Chaturanga Dandasana, Urdhva Mukha Svanasana, Adho Mukha Svanasana (5 respiraciones), Ardha
    /// Uttanasana, Uttanasana, Urdhva Vrksasana y Samasthitih. Un movimiento por respiración (vinyasa).</summary>
    public static List<QueuedSpell> AshtangaA(float breath = Breath)
    {
        var steps = new (string name, float root, float stance, (BodyPart, Vector3)[] set, (BodyPart, Vector3)[] reaches, int holdPhases)[]
        {
            ("Samasthitih (de pie)", Stand, 0f, new (BodyPart, Vector3)[0], null, 1),
            ("Urdhva Vrksasana (brazos arriba)", Stand, 0f, Join(ArmsUp, Spine(-12), new[] { J(BodyPart.Head, -25) }), null, 1),
            ("Uttanasana (flexión adelante)", Stand, 0f, Uttanasana, UttanasanaHands, 1),
            ("Ardha Uttanasana (media flexión)", Stand, 0f, ArdhaUttanasana, null, 1),
            ("Chaturanga Dandasana", 0.3f, -1f, Chaturanga, ChaturangaHands, 1),
            ("Urdhva Mukha Svanasana (perro boca arriba)", 0.2f, -1f, UrdhvaMukha, UrdhvaHands, 1),
            ("Adho Mukha Svanasana (perro boca abajo)", Stand, 0f, AdhoMukha, AdhoMukhaHands, 9),
            ("Ardha Uttanasana (media flexión)", Stand, 0f, ArdhaUttanasana, null, 1),
            ("Uttanasana (flexión adelante)", Stand, 0f, Uttanasana, UttanasanaHands, 1),
            ("Urdhva Vrksasana (brazos arriba)", Stand, 0f, Join(ArmsUp, Spine(-12), new[] { J(BodyPart.Head, -25) }), null, 1),
            ("Samasthitih (de pie)", Stand, 0f, new (BodyPart, Vector3)[0], null, 1),
        };
        var seq = new List<QueuedSpell>();
        int phase = 0;
        foreach (var s in steps)
        {
            seq.Add(Pose($"{s.name} · {Lbl(phase)}", breath, 1f, s.root, s.set, s.stance, s.reaches)); phase++;
            for (int i = 0; i < s.holdPhases; i++) { seq.Add(Pose($"{s.name} · mantén ({Lbl(phase)})", breath, 1f, s.root, s.set, s.stance, s.reaches)); phase++; }
        }
        return seq;
    }

    /// <summary>Saludo al Sol POR RESPIRACIONES: cada postura = 1 fase para llegar (se mueve toda la respiración) + 2 fases
    /// de mantenimiento (= 1 respiración completa). A 8 s/fase son 24 s/postura. Las fases alternan inhalar/exhalar.</summary>
    public static List<QueuedSpell> Sequence(float breath = Breath)
    {
        var seq = new List<QueuedSpell>();
        int phase = 0;
        foreach (var p in Postures)
        {
            var set = p.set();
            var reach = p.reaches != null ? p.reaches() : null;
            seq.Add(Pose($"{p.name} · {Lbl(phase)}", breath, 1f, p.root, set, p.stance, reach)); phase++;             // llegar (se mueve)
            seq.Add(Pose($"{p.name} · mantén ({Lbl(phase)})", breath, 1f, p.root, set, p.stance, reach)); phase++;    // mantener (1 respiración…)
            seq.Add(Pose($"{p.name} · mantén ({Lbl(phase)})", breath, 1f, p.root, set, p.stance, reach)); phase++;    // …completa
        }
        return seq;
    }

    static string Lbl(int phase) => (phase % 2 == 0) ? "inhala" : "exhala";

    /// <summary>Nº de posturas (12). La secuencia encolada tiene 3 fases por postura.</summary>
    public static int PostureCount => Postures.Length;

    /// <summary>Duración total (para quien la imita): 12 posturas × 3 fases × 8 s = 288 s.</summary>
    public static float Duration(float breath = Breath) => Postures.Length * 3 * breath;
}

/// <summary>
/// IMITAR al maestro: durante <see cref="duration"/> s copia, articulación por articulación, la postura que tiene AHORA el
/// cuerpo del <see cref="teacher"/> (su <see cref="CreatureRig"/>). Es como aprende Kushal: la Magnate hace el Saludo al
/// Sol y él la sigue. Con poco nivel de yoga la sigue con retraso (lerp lento); al terminar vuelve a estar de pie.
/// </summary>
public class MimicPoseSpell : QueuedSpell
{
    public Transform teacher;
    public float duration = 18f;
    public float follow = 3f;   // rapidez del seguimiento (más nivel de yoga → más preciso)
    float _until = -1f;

    static readonly BodyPart[] Joints =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head, BodyPart.ShoulderLeft, BodyPart.ShoulderRight,
        BodyPart.ForearmLeft, BodyPart.ForearmRight, BodyPart.HandLeft, BodyPart.HandRight, BodyPart.ThighLeft, BodyPart.ThighRight,
        BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };

    public override string Label => teacher != null ? $"imitar a {teacher.name}" : "imitar";

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        if (_until < 0f) _until = Time.time + duration;
        CreatureRig mine = self.GetComponent<CreatureRig>();
        CreatureRig theirs = teacher != null ? teacher.GetComponent<CreatureRig>() : null;
        bool done = Time.time >= _until || theirs == null;
        if (mine == null) return done;
        foreach (BodyPart p in Joints)
        {
            Transform b = mine.Get(p);
            if (b == null) continue;
            Transform t = !done && theirs != null ? theirs.Get(p) : null;
            Quaternion goal = t != null ? t.localRotation : Quaternion.identity;   // al acabar: de pie
            b.localRotation = Quaternion.Slerp(b.localRotation, goal, follow * Time.deltaTime);
        }
        // La altura de la pelvis también (plancha, cobra…): sigue la del maestro, o vuelve a la de pie al acabar.
        Transform myHips = mine.Get(BodyPart.Hips);
        CubeJoint mj = myHips != null ? myHips.GetComponent<CubeJoint>() : null;
        if (mj != null)
        {
            Transform th = !done && theirs != null ? theirs.Get(BodyPart.Hips) : null;
            Vector3 goalPos = th != null ? new Vector3(mj.RestPosition.x, th.localPosition.y, mj.RestPosition.z) : mj.RestPosition;
            myHips.localPosition = Vector3.Lerp(myHips.localPosition, goalPos, follow * Time.deltaTime);
        }
        return done;
    }
}
