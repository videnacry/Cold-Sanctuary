using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UPA-YOGA DE CUELLO Y HOMBROS — lo PRIMERO que enseña la Magnate (antes de Yoga Namaskar y del Saludo al Sol).
/// De pie, **talones a la anchura de las caderas**, **espalda recta y relajada**, brazos sueltos; en los 4 primeros
/// ejercicios SOLO se mueve el cuello y en los 2 últimos SOLO los hombros (escápula). Respiración **sin sonido**
/// (no Ujjayi), 8 s por inhalación/exhalación; el movimiento es fluido y lento (ocupa toda la respiración,
/// `reachFraction = 1`). Cada ejercicio son 3 repeticiones y termina en la posición neutra (cuello recto, cara al frente).
///
/// 1. Flexión/extensión: exhala barbilla al pecho · inhala cabeza atrás (×3).
/// 2. Rotación: inhala cara a la derecha (+~20° atrás) · exhala al frente · inhala a la izquierda · exhala al frente (×3).
/// 3. Inclinación lateral ("torsión"): exhala oreja al hombro derecho · inhala arriba · exhala al hombro izquierdo (×3).
/// 4. Círculo completo: barbilla→oreja dcha→atrás→oreja izq→barbilla; inhala al subir (hacia atrás), exhala al bajar
///    (a la barbilla). 3 giros y 3 en sentido contrario.
/// 5. Hombros: 3 giros verticales hacia delante (inhala arriba-atrás, exhala abajo-delante).
/// 6. Hombros: 3 giros en sentido contrario.
///
/// Ejes (verificados en CubeHumanoid): Head X+ barbilla abajo / X− cabeza atrás; Y+ cara a la derecha / Y− izquierda;
/// Z− oreja derecha al hombro / Z+ izquierda. Escápula: Z eleva, X adelanta/atrasa. Ángulos aproximados (afinado en Unity).
/// </summary>
public static class UpaYoga
{
    public const float Breath = 8f;
    const float Stand = -1f;

    static readonly BodyPart[] Joints =
    {
        BodyPart.Neck, BodyPart.Head, BodyPart.ScapulaLeft, BodyPart.ScapulaRight,
    };

    public static List<QueuedSpell> NeckSequence(float breath = Breath)
    {
        var seq = new List<QueuedSpell>();

        // 1. FLEXIÓN / EXTENSIÓN del cuello.
        for (int r = 0; r < 3; r++)
        {
            Add(seq, "exhala: barbilla al pecho", breath, Head(45, 0, 0));
            Add(seq, "inhala: cabeza atrás", breath, Head(-35, 0, 0));
        }
        Add(seq, "exhala: cuello recto", breath, Neutral);

        // 2. ROTACIÓN (cara a un lado, ~20° atrás).
        for (int r = 0; r < 3; r++)
        {
            Add(seq, "inhala: cara a la derecha", breath, Head(-20, 70, 0));
            Add(seq, "exhala: al frente", breath, Neutral);
            Add(seq, "inhala: cara a la izquierda", breath, Head(-20, -70, 0));
            Add(seq, "exhala: al frente", breath, Neutral);
        }

        // 3. INCLINACIÓN LATERAL (oreja al hombro).
        for (int r = 0; r < 3; r++)
        {
            Add(seq, "exhala: oreja al hombro derecho", breath, Head(0, 0, -40));
            Add(seq, "inhala: cabeza arriba", breath, Neutral);
            Add(seq, "exhala: oreja al hombro izquierdo", breath, Head(0, 0, 40));
            Add(seq, "inhala: cabeza arriba", breath, Neutral);
        }

        // 4. CÍRCULO COMPLETO (3 en cada sentido). Subir hacia atrás = inhala; bajar a la barbilla = exhala.
        for (int r = 0; r < 3; r++)   // sentido horario: barbilla → oreja dcha → atrás → oreja izq
        {
            Add(seq, "exhala: barbilla al pecho", breath, Head(45, 0, 0));
            Add(seq, "inhala: rueda a la oreja derecha", breath, Head(0, 0, -40));
            Add(seq, "inhala: rueda hacia atrás", breath, Head(-35, 0, 0));
            Add(seq, "exhala: rueda a la oreja izquierda", breath, Head(0, 0, 40));
        }
        for (int r = 0; r < 3; r++)   // sentido antihorario
        {
            Add(seq, "exhala: barbilla al pecho", breath, Head(45, 0, 0));
            Add(seq, "inhala: rueda a la oreja izquierda", breath, Head(0, 0, 40));
            Add(seq, "inhala: rueda hacia atrás", breath, Head(-35, 0, 0));
            Add(seq, "exhala: rueda a la oreja derecha", breath, Head(0, 0, -40));
        }
        Add(seq, "exhala: cuello recto", breath, Neutral);

        // 5. HOMBROS — giros verticales hacia DELANTE (inhala arriba-atrás, exhala abajo-delante).
        for (int r = 0; r < 3; r++)
        {
            Add(seq, "inhala: hombros arriba y atrás", breath, Scapula(-15, 20));
            Add(seq, "exhala: hombros abajo y delante", breath, Scapula(15, -10));
        }
        Add(seq, "exhala: hombros neutros", breath, Neutral);

        // 6. HOMBROS — giros en sentido CONTRARIO (inhala arriba-delante, exhala abajo-atrás).
        for (int r = 0; r < 3; r++)
        {
            Add(seq, "inhala: hombros arriba y delante", breath, Scapula(15, 20));
            Add(seq, "exhala: hombros abajo y atrás", breath, Scapula(-15, -10));
        }
        Add(seq, "exhala: hombros neutros", breath, Neutral);

        return seq;
    }

    /// <summary>Duración total (para quien la imita).</summary>
    public static float Duration(float breath = Breath)
        => (3 * 2 + 1 + 3 * 4 + 3 * 4 + 3 * 4 + 3 * 4 + 1 + 3 * 2 + 1 + 3 * 2 + 1) * breath;

    // ── Posiciones (solo cuello/hombros; el resto del cuerpo queda recto y relajado = 0) ────────────
    static (BodyPart, Vector3)[] Neutral => System.Array.Empty<(BodyPart, Vector3)>();
    static (BodyPart, Vector3)[] Head(float x, float y, float z) => new[] { (BodyPart.Head, new Vector3(x, y, z)) };
    // Hombros simétricos: x = adelante(+)/atrás(−); z = elevar. Lados espejados.
    static (BodyPart, Vector3)[] Scapula(float x, float elevate) => new[]
    { (BodyPart.ScapulaLeft, new Vector3(x, 0f, elevate)), (BodyPart.ScapulaRight, new Vector3(x, 0f, -elevate)) };

    static void Add(List<QueuedSpell> seq, string name, float breath, (BodyPart, Vector3)[] set)
    {
        var all = new (BodyPart, Vector3)[Joints.Length];
        for (int i = 0; i < Joints.Length; i++)
        {
            Vector3 e = Vector3.zero;
            foreach (var s in set) if (s.Item1 == Joints[i]) e = s.Item2;
            all[i] = (Joints[i], e);
        }
        seq.Add(new AsanaPoseSpell { asanaName = name, parts = all, duration = breath, reachFraction = 1f, rootHeight = Stand });
    }
}
