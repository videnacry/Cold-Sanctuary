using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// YOGA NAMASKAR (lo que la Magnate enseña DESPUÉS del upa-yoga de cuello y ANTES del Saludo al Sol). Secuencia de pie
/// + cuclillas con las **palmas siempre juntas en rezo** (anjali) y la **espalda recta y erguida salvo en Balasana**.
/// Respiración **Ujjayi** (sonora, por la glotis). Va POR RESPIRACIONES: cada movimiento ocurre en UNA respiración de
/// `breath` s (8) y el rezo se mueve **toda** la respiración (fluido y lento → `reachFraction = 1`).
///
///   • DE PIE × 3: rezo al pecho → inhala **sobre la cabeza** → exhala a la **nuca** → inhala arriba → exhala **al pecho**.
///     La 3ª exhalación baja además a CUCLILLAS.
///   • CUCLILLAS × 2 (plantas en el suelo): arriba → nuca → arriba → pecho → inhala **brazos al frente** → exhala arriba.
///   • Cierre: BALASANA (postura del niño) 2 min — la única con la espalda plegada.
///
/// Rotaciones por hueso aproximadas (el compañero afina en Unity); lo exacto es la secuencia, la respiración y que las
/// palmas no se separen. Eje: X+ flexiona adelante; Hombro X− sube el brazo por delante; Z abre lateral.
/// </summary>
public static class YogaNamaskar
{
    public const float Breath = 8f;
    public const float BalasanaHold = 120f;
    const float Stand = -1f;
    const float SquatRoot = 0.42f;

    static readonly BodyPart[] Joints =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head,
        BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft, BodyPart.ForearmRight, BodyPart.HandLeft, BodyPart.HandRight,
        BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };

    // Muñecas: palmas enfrentadas (el rezo nunca se separa, en todas las posiciones).
    static (BodyPart, Vector3)[] Palms => new[] { J(BodyPart.HandLeft, 0, -20), J(BodyPart.HandRight, 0, 20) };
    // Rezo al pecho: codos doblados, palmas juntas delante del esternón.
    static (BodyPart, Vector3)[] PrayerChest => Join(new[] { J(BodyPart.ShoulderLeft, -25, 12), J(BodyPart.ShoulderRight, -25, -12),
        J(BodyPart.ForearmLeft, -105, -45), J(BodyPart.ForearmRight, -105, 45) }, Palms);
    // Rezo sobre la cabeza: brazos subidos por delante (flexión), codos rectos, palmas juntas arriba.
    static (BodyPart, Vector3)[] Overhead => Join(new[] { J(BodyPart.ShoulderLeft, -172, 6), J(BodyPart.ShoulderRight, -172, -6),
        J(BodyPart.ForearmLeft, 0, -8), J(BodyPart.ForearmRight, 0, 8) }, Palms);
    // Rezo a la nuca: hombros arriba, codos muy doblados → el rezo cae tras el cuello (palmas aún juntas).
    static (BodyPart, Vector3)[] Nape => Join(new[] { J(BodyPart.ShoulderLeft, -120, 8), J(BodyPart.ShoulderRight, -120, -8),
        J(BodyPart.ForearmLeft, -140, -20), J(BodyPart.ForearmRight, -140, 20) }, Palms);
    // Rezo extendido al frente: brazos horizontales adelante, palmas juntas.
    static (BodyPart, Vector3)[] Forward => Join(new[] { J(BodyPart.ShoulderLeft, -90, 6), J(BodyPart.ShoulderRight, -90, -6),
        J(BodyPart.ForearmLeft, 0, -8), J(BodyPart.ForearmRight, 0, 8) }, Palms);

    // Rodillas ABIERTAS hacia fuera (malasana: sin abrirlas los muslos chocan con el torso), espalda algo inclinada y
    // planta del pie PLANA en el suelo (el pie compensa los 33° que acaba girado tras muslo + espinilla).
    static (BodyPart, Vector3)[] SquatLegs => new[] { J(BodyPart.ThighLeft, -112, -22), J(BodyPart.ThighRight, -112, 22),
        J(BodyPart.ShinLeft, 145), J(BodyPart.ShinRight, 145), J(BodyPart.FootLeft, -33), J(BodyPart.FootRight, -33), J(BodyPart.Back, 12) };

    public static List<QueuedSpell> Sequence(float breath = Breath, float balasanaHold = BalasanaHold)
    {
        var seq = new List<QueuedSpell>();

        // Preparación: de pie, piernas a la anchura de los hombros, rezo al pecho (una respiración para asentarse).
        seq.Add(Pose("prepara: de pie, rezo al pecho", "inhala", breath, 1f, Stand, PrayerChest));

        for (int r = 0; r < 3; r++)   // DE PIE × 3
        {
            seq.Add(Pose("inhala: rezo sobre la cabeza", "inhala", breath, 1f, Stand, Overhead));
            seq.Add(Pose("exhala (ujjayi): manos a la nuca", "exhala", breath, 1f, Stand, Nape));
            seq.Add(Pose("inhala: rezo sobre la cabeza", "inhala", breath, 1f, Stand, Overhead));
            if (r < 2) seq.Add(Pose("exhala (ujjayi): rezo al pecho", "exhala", breath, 1f, Stand, PrayerChest));
            else       seq.Add(Pose("exhala (ujjayi): rezo al pecho, baja a cuclillas", "exhala", breath, 1f, SquatRoot, Join(PrayerChest, SquatLegs)));
        }

        for (int r = 0; r < 2; r++)   // CUCLILLAS × 2
        {
            seq.Add(Pose("inhala: rezo sobre la cabeza (cuclillas)", "inhala", breath, 1f, SquatRoot, Join(Overhead, SquatLegs)));
            seq.Add(Pose("exhala (ujjayi): manos a la nuca", "exhala", breath, 1f, SquatRoot, Join(Nape, SquatLegs)));
            seq.Add(Pose("inhala: rezo sobre la cabeza", "inhala", breath, 1f, SquatRoot, Join(Overhead, SquatLegs)));
            seq.Add(Pose("exhala (ujjayi): rezo al pecho", "exhala", breath, 1f, SquatRoot, Join(PrayerChest, SquatLegs)));
            seq.Add(Pose("inhala: brazos al frente", "inhala", breath, 1f, SquatRoot, Join(Forward, SquatLegs)));
            seq.Add(Pose("exhala (ujjayi): recoge, rezo arriba", "exhala", breath, 1f, SquatRoot, Join(Overhead, SquatLegs)));
        }

        seq.Add(Balasana(balasanaHold));   // cierre: la única con la espalda plegada
        return seq;
    }

    /// <summary>Duración total (para quien la imita): prep + 3×4 + 2×6 respiraciones + Balasana.</summary>
    public static float Duration(float breath = Breath, float balasanaHold = BalasanaHold)
        => (1 + 3 * 4 + 2 * 6) * breath + balasanaHold;

    static AsanaPoseSpell Pose(string name, string breath, float duration, float reach, float rootHeight, params (BodyPart part, Vector3 euler)[] set)
    {
        var all = new (BodyPart, Vector3)[Joints.Length];
        for (int i = 0; i < Joints.Length; i++)
        {
            Vector3 e = Vector3.zero;
            foreach (var s in set) if (s.part == Joints[i]) e = s.euler;
            all[i] = (Joints[i], e);
        }
        // Rezo al pecho: las palmas se JUNTAN delante del esternón por IK (con solo ángulos los codos se metían en el torso).
        bool prayer = false;
        foreach (var s in set) if (s.part == BodyPart.ForearmLeft && Mathf.Approximately(s.euler.x, -105f)) prayer = true;
        bool overhead = false;   // rezo SOBRE la cabeza: las palmas también se juntan (con solo ángulos quedaban separadas)
        foreach (var s in set) if (s.part == BodyPart.ShoulderLeft && Mathf.Approximately(s.euler.x, -172f)) overhead = true;
        var reaches = prayer ? new[] { (BodyPart.HandLeft, new Vector3(-0.035f, 1.34f, 0.27f)), (BodyPart.HandRight, new Vector3(0.035f, 1.34f, 0.27f)) }
                    : overhead ? new[] { (BodyPart.HandLeft, new Vector3(-0.035f, 2.07f, 0.1f)), (BodyPart.HandRight, new Vector3(0.035f, 2.07f, 0.1f)) }
                    : System.Array.Empty<(BodyPart, Vector3)>();
        return new AsanaPoseSpell { asanaName = $"{name} · {breath}", parts = all, duration = duration, reachFraction = reach, rootHeight = rootHeight, reaches = reaches };
    }

    static AsanaPoseSpell Balasana(float hold)
    {
        var set = Join(
            new[] { J(BodyPart.Back, 55), J(BodyPart.Chest, 45), J(BodyPart.Head, 15) },
            new[] { J(BodyPart.ThighLeft, -118), J(BodyPart.ThighRight, -118), J(BodyPart.ShinLeft, 150), J(BodyPart.ShinRight, 150) },
            new[] { J(BodyPart.ShoulderLeft, -150, 6), J(BodyPart.ShoulderRight, -150, -6) });
        return Pose("Balasana (postura del niño)", "respira", hold, 0.05f, 0.30f, set);
    }

    static (BodyPart, Vector3) J(BodyPart p, float x, float z = 0f) => (p, new Vector3(x, 0f, z));
    static (BodyPart, Vector3)[] Join(params (BodyPart, Vector3)[][] groups)
    {
        var l = new List<(BodyPart, Vector3)>();
        foreach (var g in groups) l.AddRange(g);
        return l.ToArray();
    }
}
