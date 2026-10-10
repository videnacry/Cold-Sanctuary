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
    // Atajos de orientación (grados) por hueso según la postura.
    static (BodyPart, Vector3) Chest(float x) => (BodyPart.Chest, new Vector3(x, 0, 0));
    static (BodyPart, Vector3) Neck(float x)  => (BodyPart.Head,  new Vector3(x, 0, 0));
    static (BodyPart, Vector3) ArmsUp()       => (BodyPart.ShoulderLeft, new Vector3(0, 0, -150));
    static (BodyPart, Vector3) ArmsUpR()      => (BodyPart.ShoulderRight, new Vector3(0, 0, 150));
    static (BodyPart, Vector3) ArmsDown()     => (BodyPart.ShoulderLeft, new Vector3(0, 0, -20));
    static (BodyPart, Vector3) ArmsDownR()    => (BodyPart.ShoulderRight, new Vector3(0, 0, 20));

    static AsanaPoseSpell Pose(string name, float hold, params (BodyPart, Vector3)[] parts)
        => new AsanaPoseSpell { asanaName = name, parts = parts, hold = hold };

    public static List<QueuedSpell> Sequence(float holdPerPose = 1.5f)
    {
        float h = holdPerPose;
        return new List<QueuedSpell>
        {
            Pose("Pranamasana (oración)",        h, Chest(0),   Neck(0),   ArmsDown(), ArmsDownR()),
            Pose("Hasta Uttanasana (brazos arriba)", h, Chest(-25), Neck(-20), ArmsUp(),  ArmsUpR()),
            Pose("Uttanasana (flexión adelante)", h, Chest(70),  Neck(30),  ArmsDown(), ArmsDownR()),
            Pose("Ashwa Sanchalanasana (zancada)", h, Chest(10), Neck(-10), ArmsDown(), ArmsDownR()),
            Pose("Dandasana (plancha)",          h, Chest(0),   Neck(0),   ArmsDown(), ArmsDownR()),
            Pose("Ashtanga Namaskara (ocho miembros)", h, Chest(15), Neck(-5), ArmsDown(), ArmsDownR()),
            Pose("Bhujangasana (cobra)",         h, Chest(-45), Neck(-30), ArmsDown(), ArmsDownR()),
            Pose("Adho Mukha Svanasana (perro boca abajo)", h, Chest(60), Neck(20), ArmsUp(), ArmsUpR()),
            Pose("Ashwa Sanchalanasana (zancada)", h, Chest(10), Neck(-10), ArmsDown(), ArmsDownR()),
            Pose("Uttanasana (flexión adelante)", h, Chest(70),  Neck(30),  ArmsDown(), ArmsDownR()),
            Pose("Hasta Uttanasana (brazos arriba)", h, Chest(-25), Neck(-20), ArmsUp(), ArmsUpR()),
            Pose("Pranamasana (oración)",        h, Chest(0),   Neck(0),   ArmsDown(), ArmsDownR()),
        };
    }
}
