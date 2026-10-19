using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ASHTANGA — Serie Primaria (Yoga Chikitsa), "nivel 1" (docs/ashtanga-primary-series.md). Vinyasa por respiraciones
/// (Ujjayi), posturas principales mantenidas **5 respiraciones**. Aquí va el **núcleo runnable** (apertura + posturas
/// SAGITALES + cierre); las laterales/invertidas (triángulo, torsiones, vela, parada de cabeza) están en el doc para
/// añadirlas con validación visual en Unity. Reutiliza el Saludo al Sol (`SunSalutation`) para la apertura.
/// Ángulos aproximados (el compañero afina en Unity); lo exacto es la secuencia, la respiración y qué se mueve.
/// </summary>
public static class AshtangaPrimary
{
    public const float Breath = 8f;
    const float Stand = -1f;
    const float Seat = 0.5f;    // pelvis sentado
    const float Lie = 0.12f;    // pelvis tumbado

    static readonly BodyPart[] Joints =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head,
        BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft, BodyPart.ForearmRight, BodyPart.HandLeft, BodyPart.HandRight,
        BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };

    /// <summary>Serie Primaria (núcleo). `holdBreaths` = respiraciones de mantenimiento por postura (Ashtanga = 5).</summary>
    public static List<QueuedSpell> Sequence(float breath = Breath, int holdBreaths = 5)
    {
        var seq = new List<QueuedSpell>();
        int phase = 0;
        int holdPhases = Mathf.Max(0, holdBreaths) * 2;   // una respiración = 2 fases

        // APERTURA: Surya Namaskara A (reutilizado).
        seq.AddRange(SunSalutation.AshtangaA(breath));   // Surya Namaskara A de Ashtanga (chaturanga, perro boca arriba y abajo)

        // DE PIE (sagitales).
        Posture(seq, ref phase, breath, holdPhases, "Padangusthasana (dedos de pie)", Stand, 0.25f,
            SunSalutation.Uttanasana, SunSalutation.FloorHands(0.1f, 0.2f));   // pliegue desde la cadera; índice y corazón agarran los dedos gordos
        Posture(seq, ref phase, breath, holdPhases, "Utkatasana (silla)", 0.75f, 0f,
            Join(ArmsUp, new[] { J(BodyPart.ThighLeft, -30), J(BodyPart.ThighRight, -30), J(BodyPart.ShinLeft, 45), J(BodyPart.ShinRight, 45) }, Spine(20)));
        Posture(seq, ref phase, breath, holdPhases, "Virabhadrasana I (guerrero I)", 0.6f, -1f,
            Join(Lunge(true), ArmsUp, Spine(-10), new[] { J(BodyPart.Head, -10) }));
        Posture(seq, ref phase, breath, holdPhases, "Prasarita Padottanasana (piernas abiertas)", Stand, 0.9f,
            Join(SunSalutation.Hinge(105f), Spine(40), HandsDown(140), new[] { J(BodyPart.Head, 15) }), SunSalutation.FloorHands(0.16f, 0.05f));   // coronilla hacia el suelo, manos entre los pies

        // SENTADAS (Yoga Chikitsa, núcleo).
        Posture(seq, ref phase, breath, holdPhases, "Dandasana (bastón)", Seat, 0.12f,
            new[] { J(BodyPart.ThighLeft, -90), J(BodyPart.ThighRight, -90) });
        Posture(seq, ref phase, breath, holdPhases, "Paschimottanasana (pinza sentada)", Seat, 0.12f,
            Join(new[] { J(BodyPart.ThighLeft, -90), J(BodyPart.ThighRight, -90) }, Spine(80), HandsDown(80)));
        Posture(seq, ref phase, breath, holdPhases, "Navasana (barca)", Seat, 0.12f,
            new[] { J(BodyPart.Hips, -40), J(BodyPart.ThighLeft, -55), J(BodyPart.ThighRight, -55), J(BodyPart.ShoulderLeft, -90), J(BodyPart.ShoulderRight, -90) });
        Posture(seq, ref phase, breath, holdPhases, "Baddha Konasana (ángulo atado)", Seat, 0.12f,
            new[] { J(BodyPart.ThighLeft, new Vector3(-50, 0, 35)), J(BodyPart.ThighRight, new Vector3(-50, 0, -35)),
                    J(BodyPart.ShinLeft, 120), J(BodyPart.ShinRight, 120), J(BodyPart.Back, new Vector3(10, 0, 0)) });

        // CIERRE.
        Posture(seq, ref phase, breath, holdPhases, "Setu Bandhasana (puente)", 0.35f, 0.12f,
            new[] { J(BodyPart.Hips, -40), J(BodyPart.ThighLeft, -30), J(BodyPart.ThighRight, -30), J(BodyPart.ShinLeft, 70), J(BodyPart.ShinRight, 70) });
        // Savasana: tumbado boca arriba, relajado. Se mantiene varios minutos.
        seq.Add(Pose("Savasana (relajación final) · exhala", breath, 1f, Lie,
            new[] { J(BodyPart.Hips, -90) }));
        for (int i = 0; i < 14; i++) { seq.Add(Pose($"Savasana · respira", breath, 1f, Lie, new[] { J(BodyPart.Hips, -90) })); }

        return seq;
    }

    /// <summary>Duración aproximada (para quien la imita).</summary>
    public static float Duration(float breath = Breath, int holdBreaths = 5)
        => SunSalutation.AshtangaA(breath).Count * breath + (10 * (1 + holdBreaths * 2) + 15) * breath;

    // Una postura = 1 fase para llegar + holdPhases fases de mantenimiento (misma pose).
    static void Posture(List<QueuedSpell> seq, ref int phase, float breath, int holdPhases, string name, float root, float stance,
                        (BodyPart, Vector3)[] set, (BodyPart, Vector3)[] reaches = null)
    {
        seq.Add(Pose($"{name} · {Lbl(phase)}", breath, 1f, root, set, stance, reaches)); phase++;
        for (int i = 0; i < holdPhases; i++) { seq.Add(Pose($"{name} · mantén ({Lbl(phase)})", breath, 1f, root, set, stance, reaches)); phase++; }
    }

    static string Lbl(int phase) => (phase % 2 == 0) ? "inhala" : "exhala";

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

    static (BodyPart, Vector3)[] Floor() => new[] { (BodyPart.HandLeft, new Vector3(-0.1f, 0.03f, 0.14f)), (BodyPart.HandRight, new Vector3(0.1f, 0.03f, 0.14f)) };
    static (BodyPart, Vector3) J(BodyPart p, float x) => (p, new Vector3(x, 0f, 0f));
    static (BodyPart, Vector3) J(BodyPart p, Vector3 e) => (p, e);
    static (BodyPart, Vector3)[] Spine(float x) => new[] { J(BodyPart.Back, x * 0.55f), J(BodyPart.Chest, x * 0.45f) };
    static (BodyPart, Vector3)[] ArmsUp => new[] { J(BodyPart.ShoulderLeft, -175f), J(BodyPart.ShoulderRight, -175f) };
    static (BodyPart, Vector3)[] HandsDown(float t) => new[] { J(BodyPart.ShoulderLeft, -t), J(BodyPart.ShoulderRight, -t) };
    static (BodyPart, Vector3)[] Lunge(bool leftForward) => new[]
    {
        J(leftForward ? BodyPart.ThighLeft : BodyPart.ThighRight, -100f), J(leftForward ? BodyPart.ShinLeft : BodyPart.ShinRight, 95f),
        J(leftForward ? BodyPart.ThighRight : BodyPart.ThighLeft, 55f),  J(leftForward ? BodyPart.ShinRight : BodyPart.ShinLeft, 10f),
    };
    static (BodyPart, Vector3)[] Join(params (BodyPart, Vector3)[][] groups)
    {
        var l = new List<(BodyPart, Vector3)>();
        foreach (var g in groups) l.AddRange(g);
        return l.ToArray();
    }
}
