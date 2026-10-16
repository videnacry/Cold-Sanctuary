using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UPA-YOGA DE CUELLO (lo PRIMERO que enseña la Magnate; docs/upa-yoga-mission.md). Es un calentamiento de hombros/cuello
/// con **respiración Ujjayi** (la "victoriosa": se exhala como hacia la boca cerrada y el aire sale por la nariz cerrando
/// parcialmente la **glotis** → sonido oceánico). Cada inhalación/exhalación dura `breath` s (8) y el rezo **se mueve
/// toda la respiración** (fluido y lento → `reachFraction = 1`), no una transición rápida + estancamiento como las asanas.
///
/// Estructura (según el maestro):
///   • DE PIE (3 repeticiones): de rezo al pecho → inhala rezo SOBRE LA CABEZA → exhala manos a la NUCA → inhala arriba
///     → exhala rezo AL PECHO. La 3ª exhalación al pecho baja además a CUCLILLAS.
///   • CUCLILLAS (2 repeticiones, plantas completas en el suelo, rezo al pecho por delante de las rodillas): arriba →
///     nuca → arriba → pecho → inhala brazos AL FRENTE (rezo extendido) → exhala recoge y apunta arriba.
///   • Cierre: BALASANA (postura del niño) 2 min.
///
/// Rotaciones por hueso aproximadas (el compañero afina en Unity, como el Saludo al Sol); lo exacto es la SECUENCIA,
/// la respiración y qué grupos se mueven. Mismo eje que SunSalutation: X+ flexiona adelante; Z abre/eleva los brazos.
/// </summary>
public static class UpaYoga
{
    public const float Breath = 8f;     // cada inhalación o exhalación
    public const float BalasanaHold = 120f;
    const float Stand = -1f;            // pelvis de pie
    const float SquatRoot = 0.42f;      // pelvis en cuclillas (plantas en el suelo)

    static readonly BodyPart[] Joints =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head,
        BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft, BodyPart.ForearmRight, BodyPart.HandLeft, BodyPart.HandRight,
        BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };

    // ── Posiciones del rezo (hombros = brazo entero; antebrazo = codo; mano = muñeca) ───────────────
    // Rezo al pecho: codos doblados, palmas juntas delante del esternón.
    static (BodyPart, Vector3)[] PrayerChest => new[] { J(BodyPart.ShoulderLeft, -25, 12), J(BodyPart.ShoulderRight, -25, -12),
        J(BodyPart.ForearmLeft, -105, -45), J(BodyPart.ForearmRight, -105, 45), J(BodyPart.HandLeft, 0, -20), J(BodyPart.HandRight, 0, 20) };
    // Rezo sobre la cabeza: brazos subidos POR DELANTE (flexión X−) hasta la vertical, antebrazos rectos y palmas juntas
    // (ligera aducción Z para que el rezo se mantenga unido arriba). El compañero afina la convergencia en Unity.
    static (BodyPart, Vector3)[] Overhead => new[] { J(BodyPart.ShoulderLeft, -172, 6), J(BodyPart.ShoulderRight, -172, -6),
        J(BodyPart.ForearmLeft, 0, -4), J(BodyPart.ForearmRight, 0, 4), J(BodyPart.HandLeft, 0, -6), J(BodyPart.HandRight, 0, 6) };
    // Manos a la nuca: hombros abiertos arriba, codos muy doblados → las manos caen tras el cuello.
    static (BodyPart, Vector3)[] Nape => new[] { J(BodyPart.ShoulderLeft, 0, -130), J(BodyPart.ShoulderRight, 0, 130),
        J(BodyPart.ForearmLeft, -125, -25), J(BodyPart.ForearmRight, -125, 25), J(BodyPart.HandLeft, 0, -15), J(BodyPart.HandRight, 0, 15) };
    // Rezo extendido al frente: brazos horizontales hacia delante (X−), palmas juntas.
    static (BodyPart, Vector3)[] Forward => new[] { J(BodyPart.ShoulderLeft, -90, 6), J(BodyPart.ShoulderRight, -90, -6),
        J(BodyPart.ForearmLeft, 0, -6), J(BodyPart.ForearmRight, 0, 6), J(BodyPart.HandLeft, 0, -6), J(BodyPart.HandRight, 0, 6) };

    // Piernas en cuclillas profunda (plantas en el suelo): caderas muy flexionadas, rodillas dobladas, pies planos.
    static (BodyPart, Vector3)[] SquatLegs => new[] { J(BodyPart.ThighLeft, -112), J(BodyPart.ThighRight, -112),
        J(BodyPart.ShinLeft, 145), J(BodyPart.ShinRight, 145), J(BodyPart.FootLeft, 12), J(BodyPart.FootRight, 12) };

    /// <summary>La secuencia completa de upa-yoga de cuello, lista para encolar (o para que la imite el alumno).</summary>
    public static List<QueuedSpell> NeckWarmup(float breath = Breath, float balasanaHold = BalasanaHold)
    {
        var seq = new List<QueuedSpell>();

        // Preparación: de pie, piernas a la anchura de los hombros, rezo al pecho (una respiración para asentarse).
        seq.Add(Pose("prepara: de pie, rezo al pecho", breath, 1f, Stand, PrayerChest));

        // DE PIE × 3.
        for (int r = 0; r < 3; r++)
        {
            seq.Add(Pose("inhala: rezo sobre la cabeza", breath, 1f, Stand, Overhead));
            seq.Add(Pose("exhala (ujjayi): manos a la nuca", breath, 1f, Stand, Nape));
            seq.Add(Pose("inhala: rezo sobre la cabeza", breath, 1f, Stand, Overhead));
            // La 3ª exhalación al pecho baja además a cuclillas.
            if (r < 2) seq.Add(Pose("exhala (ujjayi): rezo al pecho", breath, 1f, Stand, PrayerChest));
            else       seq.Add(Pose("exhala (ujjayi): rezo al pecho, baja a cuclillas", breath, 1f, SquatRoot, Join(PrayerChest, SquatLegs)));
        }

        // CUCLILLAS × 2 (añade la variante de brazos al frente).
        for (int r = 0; r < 2; r++)
        {
            seq.Add(Pose("inhala: rezo sobre la cabeza (cuclillas)", breath, 1f, SquatRoot, Join(Overhead, SquatLegs)));
            seq.Add(Pose("exhala (ujjayi): manos a la nuca", breath, 1f, SquatRoot, Join(Nape, SquatLegs)));
            seq.Add(Pose("inhala: rezo sobre la cabeza", breath, 1f, SquatRoot, Join(Overhead, SquatLegs)));
            seq.Add(Pose("exhala (ujjayi): rezo al pecho", breath, 1f, SquatRoot, Join(PrayerChest, SquatLegs)));
            seq.Add(Pose("inhala: brazos al frente", breath, 1f, SquatRoot, Join(Forward, SquatLegs)));
            seq.Add(Pose("exhala (ujjayi): recoge, rezo arriba", breath, 1f, SquatRoot, Join(Overhead, SquatLegs)));
        }

        // Cierre: Balasana (postura del niño), 2 min. Llega rápido (reach 0.05) y se mantiene.
        seq.Add(Balasana(balasanaHold));
        return seq;
    }

    /// <summary>Duración total (para quien la imita): prep + 3×4 + 2×6 respiraciones + Balasana.</summary>
    public static float Duration(float breath = Breath, float balasanaHold = BalasanaHold)
        => (1 + 3 * 4 + 2 * 6) * breath + balasanaHold;

    // ── Builders ────────────────────────────────────────────────────────────────────────────────
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

    // Balasana (postura del niño): arrodillado, sentado sobre los talones, torso plegado adelante, brazos extendidos.
    static AsanaPoseSpell Balasana(float hold)
    {
        var set = Join(
            new[] { J(BodyPart.Back, 55), J(BodyPart.Chest, 45), J(BodyPart.Head, 15) },                 // torso plegado
            new[] { J(BodyPart.ThighLeft, -118), J(BodyPart.ThighRight, -118), J(BodyPart.ShinLeft, 150), J(BodyPart.ShinRight, 150) }, // sentado sobre los talones
            new[] { J(BodyPart.ShoulderLeft, -150, 6), J(BodyPart.ShoulderRight, -150, -6) });            // brazos al frente en el suelo
        return Pose("Balasana (postura del niño)", hold, 0.05f, 0.30f, set);
    }

    static (BodyPart, Vector3) J(BodyPart p, float x, float z = 0f) => (p, new Vector3(x, 0f, z));
    static (BodyPart, Vector3)[] Join(params (BodyPart, Vector3)[][] groups)
    {
        var l = new List<(BodyPart, Vector3)>();
        foreach (var g in groups) l.AddRange(g);
        return l.ToArray();
    }
}
