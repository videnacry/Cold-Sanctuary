using UnityEngine;

/// <summary>
/// ESTACIÓN DE ANÁLISIS CLÍNICO (el analizador de sangre): F → analiza al paciente más cercano a la máquina (humano o
/// animal, que no sea quien la usa) con el <see cref="ClinicalAnalysisSpell"/> y marca su historia como analizada.
/// </summary>
public class ClinicalStation : MonoBehaviour, IInteractable
{
    public float patientRadius = 8f;
    public string InteractLabel => "Hacer un análisis clínico";
    public bool CanInteract => true;

    public void Interact()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        Analyze(p != null ? p.GetComponent<Anima>() : null);
    }

    /// <summary>Analiza al paciente más cercano; devuelve quién fue (null si no hay nadie).</summary>
    public Anima Analyze(Anima by)
    {
        Anima best = null; float bd = patientRadius;
        foreach (Anima a in FindObjectsByType<Anima>(FindObjectsSortMode.None))
        {
            if (a == by || a.death || a.GetComponent<Linduarda>() != null) continue;
            float d = Vector3.Distance(a.transform.position, transform.position);
            if (d < bd) { bd = d; best = a; }
        }
        if (best == null) { Debug.Log("[Clínico] No hay ningún paciente junto al analizador."); return null; }
        if (!(best is ITarget it)) { if (!best.TryGetComponent(out AnimaTarget at)) at = best.gameObject.AddComponent<AnimaTarget>(); it = at; }
        ClinicalAnalysisSpell spell = GetComponent<ClinicalAnalysisSpell>();
        if (spell == null) spell = gameObject.AddComponent<ClinicalAnalysisSpell>();
        spell.Cast(by, it);
        if (best.TryGetComponent(out HealthRecord hr)) hr.analyzed = true;
        Debug.Log($"[Clínico] Análisis de «{best.name}» hecho en el analizador.");
        return best;
    }
}
