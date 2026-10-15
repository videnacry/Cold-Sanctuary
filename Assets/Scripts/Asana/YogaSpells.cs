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
        bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(localEuler), lerp * Time.deltaTime);
        return Time.time >= _until;
    }
}

/// <summary>Una ASANA = varias partes a la vez (colas en paralelo por bodyPart, §4.1). Rota todas las partes que el
/// rig tenga hacia su objetivo durante `hold`; las que falten se ignoran. La unidad del Saludo al Sol.</summary>
public class AsanaPoseSpell : QueuedSpell
{
    public string asanaName = "asana";
    public (BodyPart part, Vector3 euler)[] parts = System.Array.Empty<(BodyPart, Vector3)>();
    public float hold = 1.2f;
    public float lerp = 4f;
    float _until = -1f;

    public override string Label => $"asana {asanaName}";

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        CreatureRig rig = self.GetComponent<CreatureRig>();
        if (_until < 0f) _until = Time.time + hold;
        if (rig != null)
            foreach (var p in parts)
            {
                Transform bone = rig.Get(p.part);
                if (bone != null) bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(p.euler), lerp * Time.deltaTime);
            }
        return Time.time >= _until;
    }
}

/// <summary>SALUDO AL SOL (Surya Namaskar) — las 12 asanas reales, en código (no como ScriptableObject). Devuelve la
/// secuencia de <see cref="AsanaPoseSpell"/> para encolar. Las rotaciones por hueso (Chest/Neck/Shoulders/Feet del
/// `CreatureRig`) son aproximadas (el compañero afina en Unity); lo exacto es el ORDEN y los grupos musculares.</summary>
public static class SunSalutation
{
    // POSTURA COMPLETA por articulación del cuerpo de cubos (CubeHumanoid): cada pose fija TODAS las articulaciones
    // (las que no se nombran vuelven a 0 = de pie) para que una zancada no deje la pierna adelantada en la siguiente.
    // Ejes (rotación local, grados): X+ inclina hacia DELANTE (cintura dobla el torso; muslo hacia atrás; espinilla
    // dobla la rodilla hacia atrás), X- lleva el miembro hacia DELANTE; Z abre los brazos hacia los lados/arriba.
    static readonly BodyPart[] Joints =
    {
        BodyPart.Chest, BodyPart.Head, BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft, BodyPart.ForearmRight,
        BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };

    static AsanaPoseSpell Pose(string name, float hold, params (BodyPart part, Vector3 euler)[] set)
    {
        var all = new (BodyPart, Vector3)[Joints.Length];
        for (int i = 0; i < Joints.Length; i++)
        {
            Vector3 e = Vector3.zero;
            foreach (var s in set) if (s.part == Joints[i]) e = s.euler;
            all[i] = (Joints[i], e);
        }
        return new AsanaPoseSpell { asanaName = name, parts = all, hold = hold };
    }

    static (BodyPart, Vector3) J(BodyPart p, float x, float z = 0f) => (p, new Vector3(x, 0f, z));
    static (BodyPart, Vector3)[] ArmsUp => new[] { J(BodyPart.ShoulderLeft, 0, -165), J(BodyPart.ShoulderRight, 0, 165) };
    static (BodyPart, Vector3)[] Prayer => new[] { J(BodyPart.ShoulderLeft, -20, 15), J(BodyPart.ShoulderRight, -20, -15),
                                                   J(BodyPart.ForearmLeft, -110, -40), J(BodyPart.ForearmRight, -110, 40) };
    static (BodyPart, Vector3)[] Plus(params (BodyPart, Vector3)[][] groups)
    {
        var l = new List<(BodyPart, Vector3)>();
        foreach (var g in groups) l.AddRange(g);
        return l.ToArray();
    }

    public static List<QueuedSpell> Sequence(float holdPerPose = 1.5f)
    {
        float h = holdPerPose;
        var lungeR = new[] { J(BodyPart.Chest, 25), J(BodyPart.Head, -25), J(BodyPart.ThighRight, -95), J(BodyPart.ShinRight, 90),
                             J(BodyPart.ThighLeft, 35), J(BodyPart.ShoulderLeft, -60), J(BodyPart.ShoulderRight, -60) };
        var lungeL = new[] { J(BodyPart.Chest, 25), J(BodyPart.Head, -25), J(BodyPart.ThighLeft, -95), J(BodyPart.ShinLeft, 90),
                             J(BodyPart.ThighRight, 35), J(BodyPart.ShoulderLeft, -60), J(BodyPart.ShoulderRight, -60) };
        return new List<QueuedSpell>
        {
            Pose("Pranamasana (oración)", h, Prayer),
            Pose("Hasta Uttanasana (brazos arriba)", h, Plus(ArmsUp, new[] { J(BodyPart.Chest, -25), J(BodyPart.Head, -20) })),
            Pose("Uttanasana (flexión adelante)", h, J(BodyPart.Chest, 95), J(BodyPart.Head, 20)),
            Pose("Ashwa Sanchalanasana (zancada)", h, lungeR),
            Pose("Dandasana (plancha)", h, J(BodyPart.Chest, 80), J(BodyPart.ThighLeft, 10), J(BodyPart.ThighRight, 10),
                 J(BodyPart.ShoulderLeft, -80), J(BodyPart.ShoulderRight, -80)),
            Pose("Ashtanga Namaskara (ocho miembros)", h, J(BodyPart.Chest, 70), J(BodyPart.ShinLeft, 70), J(BodyPart.ShinRight, 70),
                 J(BodyPart.ShoulderLeft, -30), J(BodyPart.ShoulderRight, -30), J(BodyPart.ForearmLeft, -60), J(BodyPart.ForearmRight, -60)),
            Pose("Bhujangasana (cobra)", h, J(BodyPart.Chest, -45), J(BodyPart.Head, -30),
                 J(BodyPart.ShoulderLeft, -20), J(BodyPart.ShoulderRight, -20)),
            Pose("Adho Mukha Svanasana (perro boca abajo)", h, Plus(ArmsUp, new[] { J(BodyPart.Chest, 100), J(BodyPart.Head, 25) })),
            Pose("Ashwa Sanchalanasana (zancada)", h, lungeL),
            Pose("Uttanasana (flexión adelante)", h, J(BodyPart.Chest, 95), J(BodyPart.Head, 20)),
            Pose("Hasta Uttanasana (brazos arriba)", h, Plus(ArmsUp, new[] { J(BodyPart.Chest, -25), J(BodyPart.Head, -20) })),
            Pose("Pranamasana (oración)", h, Prayer),
        };
    }

    /// <summary>Duración total de la secuencia (para quien la imita).</summary>
    public static float Duration(float holdPerPose = 1.5f) => 12 * holdPerPose;
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
        BodyPart.Chest, BodyPart.Neck, BodyPart.Head, BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft,
        BodyPart.ForearmRight, BodyPart.HandLeft, BodyPart.HandRight, BodyPart.ThighLeft, BodyPart.ThighRight,
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
        return done;
    }
}
