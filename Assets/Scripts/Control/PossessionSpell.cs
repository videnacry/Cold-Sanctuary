using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hechizo de POSESIÓN del jugador (docs/anima-architecture.md §11.5). Inserta al jugador (un
/// <see cref="PlayerBrain"/> con relevancia = <see cref="power"/>) en cualquier `Anima` con
/// <see cref="AnimaController"/>; si <c>power</c> supera la relevancia propia del ser, el jugador toma el
/// mando. <c>power</c> y <c>range</c> CRECEN al mejorar el hechizo (dominar seres más poderosos y en mayor
/// número). El jugador es "solo un input" que se enchufa a otro cuerpo en runtime, y al soltarlo la IA del
/// ser retoma su vida.
/// </summary>
public class PossessionSpell : MonoBehaviour
{
    [Tooltip("Fuerza de posesión. Debe superar la selfRelevance del objetivo para dominarlo. Crece con el hechizo.")]
    public float power = 2f;

    [Tooltip("Alcance (m) desde el que se puede poseer. Crece con el hechizo.")]
    public float range = 8f;

    [Tooltip("Coste del hechizo en elementos (se paga de MagicReserves si el lanzador la tiene). Vacío = gratis.")]
    public List<ElementCost> cost = new List<ElementCost>();
    [Tooltip("Coste en ENERGÍA (julios): la activación/canalización del hechizo, además de la materia. 0 = gratis.")]
    public float energyCost = 0f;

    PlayerBrain _current;   // el cuerpo poseído ahora (si hay)

    /// <summary>Poseído actual (null si el jugador no está en ningún cuerpo ajeno).</summary>
    public AnimaController Current => _current != null ? _current.GetComponent<AnimaController>() : null;

    /// <summary>Posee un objetivo concreto: le inyecta/activa un PlayerBrain con relevancia = power.</summary>
    public void Possess(AnimaController target)
    {
        if (target == null) return;
        // Coste de magia: si el lanzador tiene reservas, debe poder pagar (agotado → no hay hechizo).
        MagicReserves mr = GetComponent<MagicReserves>();
        if (mr != null && !mr.Pay(cost, energyCost)) { Debug.Log($"[Posesión] «{name}» sin reservas (materia/energía) para el hechizo."); return; }
        Release();   // suelta el cuerpo anterior

        PlayerBrain pb = target.GetComponent<PlayerBrain>();
        if (pb == null) pb = target.gameObject.AddComponent<PlayerBrain>();
        pb.possessionRelevance = power;
        target.RefreshBrains();
        _current = pb;
        Debug.Log($"[Posesión] «{name}» posee «{target.name}» con poder {power:0.00} (alcance {range:0.0}). " +
                  $"El AnimaController decidirá si domina (power > selfRelevance del ser).");
    }

    /// <summary>Suelta el cuerpo actual: su IA retoma el mando.</summary>
    public void Release()
    {
        if (_current != null) { _current.Release(); _current = null; }
    }

    /// <summary>Posee al Anima poseíble más cercano dentro del alcance (el más a mano).</summary>
    public AnimaController PossessNearest()
    {
        AnimaController best = null;
        float bestDist = range;
        foreach (AnimaController c in FindObjectsOfType<AnimaController>())
        {
            if (c.gameObject == gameObject) continue;
            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d <= bestDist) { bestDist = d; best = c; }
        }
        if (best != null) Possess(best);
        else Debug.Log($"[Posesión] «{name}» no halló ningún Anima poseíble en {range:0.0} m.");
        return best;
    }
}
