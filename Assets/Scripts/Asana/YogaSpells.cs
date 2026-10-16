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
    [Range(0.05f, 0.9f)] public float reachFraction = 0.3f;   // 30% del tiempo en LLEGAR a la postura; el 70% restante, SOSTENIÉNDOLA
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
    public const float Hold = 10f;   // duración TOTAL por asana (30% llegar + 70% sostener, ver AsanaPoseSpell)
    const float Stand = -1f;   // pelvis a la altura de pie

    static readonly BodyPart[] Joints =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head,
        BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft, BodyPart.ForearmRight, BodyPart.HandLeft, BodyPart.HandRight,
        BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };

    static AsanaPoseSpell Pose(string name, float duration, float rootHeight, params (BodyPart part, Vector3 euler)[] set)
    {
        var all = new (BodyPart, Vector3)[Joints.Length];
        for (int i = 0; i < Joints.Length; i++)
        {
            Vector3 e = Vector3.zero;
            foreach (var s in set) if (s.part == Joints[i]) e = s.euler;
            all[i] = (Joints[i], e);
        }
        return new AsanaPoseSpell { asanaName = name, parts = all, duration = duration, rootHeight = rootHeight };
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
    static (BodyPart, Vector3)[] ArmsUp => new[] { J(BodyPart.ShoulderLeft, 0, -170), J(BodyPart.ShoulderRight, 0, 170) };
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

    public static List<QueuedSpell> Sequence(float holdPerPose = Hold)
    {
        float h = holdPerPose;
        return new List<QueuedSpell>
        {
            // 1. Oración: de pie, palmas juntas delante del pecho.
            Pose("Pranamasana (oración)", h, Stand, Prayer),
            // 2. Brazos arriba con extensión suave de la columna.
            Pose("Hasta Uttanasana (brazos arriba)", h, Stand, Join(ArmsUp, Spine(-30), new[] { J(BodyPart.Head, -15) })),
            // 3. Flexión adelante: torso sobre las piernas, manos al suelo.
            Pose("Uttanasana (flexión adelante)", h, Stand, Join(Spine(115), HandsDown(100), new[] { J(BodyPart.Head, 15) })),
            // 4. Zancada: pierna derecha atrás, izquierda delante, pelvis baja, mirada al frente.
            Pose("Ashwa Sanchalanasana (zancada)", h, 0.5f, Join(Lunge(true), Spine(35), HandsDown(60), new[] { J(BodyPart.Head, -35) })),
            // 5. Plancha: cuerpo recto y en diagonal, brazos rectos al suelo.
            Pose("Dandasana (plancha)", h, 0.5f, Join(new[] { J(BodyPart.Hips, 72), J(BodyPart.FootLeft, 50), J(BodyPart.FootRight, 50),
                                                              J(BodyPart.Head, -20) }, HandsDown(72))),
            // 6. Ocho apoyos: rodillas, pecho y barbilla al suelo, cadera alta, codos doblados.
            Pose("Ashtanga Namaskara (ocho miembros)", h, 0.35f, Join(new[] { J(BodyPart.Hips, 80), J(BodyPart.ShinLeft, 35), J(BodyPart.ShinRight, 35),
                J(BodyPart.ShoulderLeft, -20), J(BodyPart.ShoulderRight, -20), J(BodyPart.ForearmLeft, -100), J(BodyPart.ForearmRight, -100) }, Spine(-15))),
            // 7. Cobra: tumbado, pecho arriba con la espalda arqueada.
            Pose("Bhujangasana (cobra)", h, 0.18f, Join(new[] { J(BodyPart.Hips, 90), J(BodyPart.FootLeft, 80), J(BodyPart.FootRight, 80),
                J(BodyPart.Head, -25) }, Spine(-60), HandsDown(30))),
            // 8. Perro boca abajo: V invertida, cadera arriba, brazos alineados con el torso.
            Pose("Adho Mukha Svanasana (perro boca abajo)", h, Stand, Join(new[] { J(BodyPart.ThighLeft, 50), J(BodyPart.ThighRight, 50),
                J(BodyPart.Head, 20) }, Spine(80), ArmsUp)),
            // 9. Zancada con la otra pierna delante.
            Pose("Ashwa Sanchalanasana (zancada)", h, 0.5f, Join(Lunge(false), Spine(35), HandsDown(60), new[] { J(BodyPart.Head, -35) })),
            // 10–12. Vuelta: flexión, brazos arriba, oración.
            Pose("Uttanasana (flexión adelante)", h, Stand, Join(Spine(115), HandsDown(100), new[] { J(BodyPart.Head, 15) })),
            Pose("Hasta Uttanasana (brazos arriba)", h, Stand, Join(ArmsUp, Spine(-30), new[] { J(BodyPart.Head, -15) })),
            Pose("Pranamasana (oración)", h, Stand, Prayer),
        };
    }

    /// <summary>Duración total de la secuencia (para quien la imita). Cada asana ya incluye llegar + sostener.</summary>
    public static float Duration(float holdPerPose = Hold) => 12 * holdPerPose;
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
