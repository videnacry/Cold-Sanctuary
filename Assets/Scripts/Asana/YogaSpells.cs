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
        }
        return Time.time - _start >= duration;
    }

    static void Stance(CreatureRig rig, BodyPart part, float targetX, Vector3 from, float t)
    {
        Transform th = rig.Get(part); if (th == null) return;
        CubeJoint cj = th.GetComponent<CubeJoint>();
        Vector3 rest = cj != null ? cj.RestPosition : th.localPosition;
        th.localPosition = Vector3.Lerp(from, new Vector3(targetX, rest.y, rest.z), t);
        rig.LockPose(part);
    }

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
        Vector3 world = root.TransformPoint(localTarget);
        bool arm = end == BodyPart.HandLeft || end == BodyPart.HandRight;
        // Codo hacia abajo-atrás; rodilla hacia delante.
        Vector3 pole = midT.position + (arm ? (-root.up * 0.3f - root.forward * 0.1f) : (root.forward * 0.3f));
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
    static (BodyPart, Vector3)[] PalmsTogether => new[] { (BodyPart.HandLeft, new Vector3(0f, 1.25f, 0.20f)), (BodyPart.HandRight, new Vector3(0f, 1.25f, 0.20f)) };
    static (BodyPart, Vector3)[] HandsToFloor => new[] { (BodyPart.HandLeft, new Vector3(-0.09f, 0.03f, 0.14f)), (BodyPart.HandRight, new Vector3(0.09f, 0.03f, 0.14f)) };

    // Las 12 posturas: altura de pelvis (root), anchura de pies (stance; 0 = talones juntos, −1 = no tocar), huesos y alcance IK.
    static readonly (string name, float root, float stance, System.Func<(BodyPart, Vector3)[]> set, System.Func<(BodyPart, Vector3)[]> reaches)[] Postures =
    {
        ("Pranamasana (oración)",                 Stand, 0f,   () => Prayer, () => PalmsTogether),
        ("Hasta Uttanasana (brazos arriba)",      Stand, 0f,   () => Join(ArmsUp, Spine(-30), new[] { J(BodyPart.Head, -15) }), null),
        ("Uttanasana (flexión adelante)",         Stand, 0f,   () => Join(Spine(115), HandsDown(100), new[] { J(BodyPart.Head, 15) }), () => HandsToFloor),
        ("Ashwa Sanchalanasana (zancada)",        0.5f,  -1f,  () => Join(Lunge(true), Spine(35), HandsDown(60), new[] { J(BodyPart.Head, -35) }), null),
        ("Dandasana (plancha)",                   0.5f,  -1f,  () => Join(new[] { J(BodyPart.Hips, 72), J(BodyPart.FootLeft, 25), J(BodyPart.FootRight, 25), J(BodyPart.Head, -20) }, HandsDown(72)), null),
        ("Ashtanga Namaskara (ocho miembros)",    0.30f, -1f,  () => Join(new[] { J(BodyPart.Hips, 48), J(BodyPart.ThighLeft, -20), J(BodyPart.ThighRight, -20),
                                                                           J(BodyPart.ShinLeft, 70), J(BodyPart.ShinRight, 70), J(BodyPart.Head, 10),
                                                                           J(BodyPart.ShoulderLeft, -25), J(BodyPart.ShoulderRight, -25), J(BodyPart.ForearmLeft, -110), J(BodyPart.ForearmRight, -110) }, Spine(55)), null),
        ("Bhujangasana (cobra)",                  0.5f,  -1f,  () => Join(new[] { J(BodyPart.Hips, 90), J(BodyPart.FootLeft, 80), J(BodyPart.FootRight, 80), J(BodyPart.Head, -25) }, Spine(-60), HandsDown(30)), null),
        ("Adho Mukha Svanasana (perro boca abajo)", Stand, 0f, () => Join(new[] { J(BodyPart.ThighLeft, 50), J(BodyPart.ThighRight, 50), J(BodyPart.Head, 20) }, Spine(80), ArmsUp), null),
        ("Ashwa Sanchalanasana (zancada)",        0.5f,  -1f,  () => Join(Lunge(false), Spine(35), HandsDown(60), new[] { J(BodyPart.Head, -35) }), null),
        ("Uttanasana (flexión adelante)",         Stand, 0f,   () => Join(Spine(115), HandsDown(100), new[] { J(BodyPart.Head, 15) }), () => HandsToFloor),
        ("Hasta Uttanasana (brazos arriba)",      Stand, 0f,   () => Join(ArmsUp, Spine(-30), new[] { J(BodyPart.Head, -15) }), null),
        ("Pranamasana (oración)",                 Stand, 0f,   () => Prayer, () => PalmsTogether),
    };

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
