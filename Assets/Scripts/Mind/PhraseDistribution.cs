using System.Collections.Generic;
using UnityEngine;

/// <summary>Cómo se reparten las vivencias entre los seres al iniciar una partida (docs anima §11).</summary>
public enum NarrativeMode
{
    /// <summary>Cada ser conserva las vivencias de SU identidad. La propiedad se respeta (guionizado).</summary>
    Estricta,
    /// <summary>Los pensamientos NO bloqueados se vuelven públicos y se reparten al azar (emergente).</summary>
    Libre
}

/// <summary>Una fuente de pensamientos: su identidad autoral y si son privados.</summary>
public struct PhraseHolder
{
    public string identity;   // "Magnate", "Goluis", "Ötzi"… "" = anónimo
    public bool locked;       // true = privados: no van al pool público

    public PhraseHolder(string identity, bool locked) { this.identity = identity; this.locked = locked; }
}

/// <summary>
/// Reparto de vivencias entre seres (docs anima §11). Dos modos:
///
/// - <b>Estricta</b> (narrativa guionizada): cada ser recibe las vivencias de SU identidad. La propiedad se
///   mantiene → los personajes son quienes son (incl. la Magnate y los históricos).
/// - <b>Libre</b> (emergente): los pensamientos de los seres NO bloqueados se mueven a un <i>pool público</i>
///   y se redistribuyen al azar entre ellos → efecto mariposa, cada partida distinta. Los seres BLOQUEADOS
///   (Magnate, históricos) conservan los suyos como base y quedan FUERA del pool: nadie hereda sus
///   pensamientos y ellos siempre los tienen. El bloqueo se marca/desmarca por personaje a conveniencia.
///
/// <see cref="Plan"/> calcula el reparto (puro, sin escena → testeable/loggeable);
/// <see cref="Distribute"/> lo aplica a los <see cref="Mind"/> reales.
/// </summary>
public static class PhraseDistribution
{
    /// <summary>Calcula qué vivencias acaba teniendo cada holder, sin tocar la escena.</summary>
    public static List<MindPhrase>[] Plan(NarrativeMode mode, IList<PhraseHolder> holders)
    {
        int n = holders.Count;
        var result = new List<MindPhrase>[n];
        for (int i = 0; i < n; i++) result[i] = new List<MindPhrase>();

        if (mode == NarrativeMode.Estricta)
        {
            for (int i = 0; i < n; i++)
                result[i].AddRange(PhraseLibrary.VivenciasOf(holders[i].identity));
            return result;
        }

        // ── Libre ────────────────────────────────────────────────────────────────────────────────────
        var publicPool = new List<MindPhrase>();
        var freeIdx = new List<int>();
        for (int i = 0; i < n; i++)
        {
            List<MindPhrase> mine = PhraseLibrary.VivenciasOf(holders[i].identity);
            if (holders[i].locked)
                result[i].AddRange(mine);        // bloqueado: los conserva como base, no van al pool
            else
            {
                publicPool.AddRange(mine);       // libre: sus pensamientos se vuelven públicos
                freeIdx.Add(i);
            }
        }

        if (freeIdx.Count == 0) return result;   // todos bloqueados → nada que repartir

        Shuffle(publicPool);
        Shuffle(freeIdx);
        // Round-robin sobre los seres libres (reparto parejo) desde el pool barajado.
        for (int k = 0; k < publicPool.Count; k++)
        {
            int target = freeIdx[k % freeIdx.Count];
            MindPhrase p = publicPool[k];
            if (!result[target].Contains(p)) result[target].Add(p);
        }
        return result;
    }

    /// <summary>Aplica el reparto a las mentes reales (siembra <see cref="Mind.thoughts"/>).</summary>
    public static void Distribute(NarrativeMode mode, IList<Mind> beings)
    {
        var holders = new List<PhraseHolder>(beings.Count);
        foreach (Mind m in beings) holders.Add(new PhraseHolder(m.identity, m.thoughtsLocked));

        List<MindPhrase>[] plan = Plan(mode, holders);
        for (int i = 0; i < beings.Count; i++) beings[i].thoughts = plan[i];
    }

    static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
    }
}
