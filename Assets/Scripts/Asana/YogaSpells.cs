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
    float _start = -1f;
    Quaternion[] _from;
    Vector3 _rootFrom;

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
        }
        return Time.time - _start >= duration;
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

    static AsanaPoseSpell Pose(string name, float duration, float reach, float rootHeight, params (BodyPart part, Vector3 euler)[] set)
    {
        var all = new (BodyPart, Vector3)[Joints.Length];
        for (int i = 0; i < Joints.Length; i++)
        {
            Vector3 e = Vector3.zero;
            foreach (var s in set) if (s.part == Joints[i]) e = s.euler;
            all[i] = (Joints[i], e);
        }
        return new AsanaPoseSpell { asanaName = name, parts = all, duration = duration, reachFraction = reach, rootHeight = rootHeight };
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

    // Las 12 posturas con su altura de pelvis (root) y su juego de huesos. El tejido de respiraciones lo pone Sequence().
    static readonly (string name, float root, System.Func<(BodyPart, Vector3)[]> set)[] Postures =
    {
        ("Pranamasana (oración)",                 Stand, () => Prayer),
        ("Hasta Uttanasana (brazos arriba)",      Stand, () => Join(ArmsUp, Spine(-30), new[] { J(BodyPart.Head, -15) })),
        ("Uttanasana (flexión adelante)",         Stand, () => Join(Spine(115), HandsDown(100), new[] { J(BodyPart.Head, 15) })),
        ("Ashwa Sanchalanasana (zancada)",        0.5f,  () => Join(Lunge(true), Spine(35), HandsDown(60), new[] { J(BodyPart.Head, -35) })),
        ("Dandasana (plancha)",                   0.5f,  () => Join(new[] { J(BodyPart.Hips, 72), J(BodyPart.FootLeft, 25), J(BodyPart.FootRight, 25), J(BodyPart.Head, -20) }, HandsDown(72))),
        ("Ashtanga Namaskara (ocho miembros)",    0.30f, () => Join(new[] { J(BodyPart.Hips, 48), J(BodyPart.ThighLeft, -20), J(BodyPart.ThighRight, -20),
                                                                           J(BodyPart.ShinLeft, 70), J(BodyPart.ShinRight, 70), J(BodyPart.Head, 10),
                                                                           J(BodyPart.ShoulderLeft, -25), J(BodyPart.ShoulderRight, -25), J(BodyPart.ForearmLeft, -110), J(BodyPart.ForearmRight, -110) }, Spine(55))),
        ("Bhujangasana (cobra)",                  0.5f,  () => Join(new[] { J(BodyPart.Hips, 90), J(BodyPart.FootLeft, 80), J(BodyPart.FootRight, 80), J(BodyPart.Head, -25) }, Spine(-60), HandsDown(30))),
        ("Adho Mukha Svanasana (perro boca abajo)", Stand, () => Join(new[] { J(BodyPart.ThighLeft, 50), J(BodyPart.ThighRight, 50), J(BodyPart.Head, 20) }, Spine(80), ArmsUp)),
        ("Ashwa Sanchalanasana (zancada)",        0.5f,  () => Join(Lunge(false), Spine(35), HandsDown(60), new[] { J(BodyPart.Head, -35) })),
        ("Uttanasana (flexión adelante)",         Stand, () => Join(Spine(115), HandsDown(100), new[] { J(BodyPart.Head, 15) })),
        ("Hasta Uttanasana (brazos arriba)",      Stand, () => Join(ArmsUp, Spine(-30), new[] { J(BodyPart.Head, -15) })),
        ("Pranamasana (oración)",                 Stand, () => Prayer),
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
            seq.Add(Pose($"{p.name} · {Lbl(phase)}", breath, 1f, p.root, set)); phase++;                 // llegar (se mueve)
            seq.Add(Pose($"{p.name} · mantén ({Lbl(phase)})", breath, 1f, p.root, set)); phase++;        // mantener (1 respiración…)
            seq.Add(Pose($"{p.name} · mantén ({Lbl(phase)})", breath, 1f, p.root, set)); phase++;        // …completa
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
