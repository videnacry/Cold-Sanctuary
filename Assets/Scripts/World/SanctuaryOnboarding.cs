using UnityEngine;

/// <summary>
/// ONBOARDING de un aspirante al santuario (docs/sanctuary1-level-and-live-tests §4, founding-trio §7). Un recién
/// llegado se vuelve **miembro** cuando <see cref="Linduarda"/> lo marca **apto**, y eso ocurre tras: (1) **análisis**
/// clínico + (2) **vacuna** (su <see cref="HealthRecord"/> deja de necesitar cuidado) y (3) **3 misiones introductorias**.
/// Al completar la 3ª y estar sano → `ReadyForApt`; Linduarda lo certifica (`apt=true`). Entonces nace en la
/// <see cref="Magnate"/> el pensamiento de **presentarlo** y llevarlo a la sala de yoga (upa-yoga) y a la cocina.
/// No es scriptado: cada paso lo disparan los mismos hechizos/impulsos de siempre; este componente solo lleva la cuenta.
///
/// Las misiones reales (enfermería/limpieza/muestras…) llaman a <see cref="CompleteMission"/>. Para las escenas AUTO,
/// `autoCompleteForDemo` avanza el contador solo, de modo que la cadena completa se ve en una prueba en vivo.
/// </summary>
[RequireComponent(typeof(HealthRecord))]
public class SanctuaryOnboarding : MonoBehaviour
{
    [Tooltip("Cuántas misiones introductorias hacen falta para ser apto (el diseño dice 3).")]
    [Min(1)] public int introMissionsRequired = 3;
    [Min(0)] public int introMissionsDone = 0;

    [Tooltip("Linduarda lo pone a true cuando está sano y completó las misiones. No tocar a mano.")]
    public bool apt = false;
    [Tooltip("La Magnate lo pone a true cuando ya lo presentó ante el santuario.")]
    public bool presented = false;
    [Tooltip("Linduarda lo pone a true tras llevarlo a la máquina de virtualización (iniciación al Microcosmos).")]
    public bool initiated = false;

    [Header("Solo para escenas AUTO (prueba en vivo de toda la cadena)")]
    public bool autoCompleteForDemo = false;
    [Min(0.5f)] public float autoMissionEvery = 6f;
    float _nextAuto;

    HealthRecord _health;

    void Awake() { _health = GetComponent<HealthRecord>(); _nextAuto = Time.time + 4f; }

    void Update()
    {
        if (!autoCompleteForDemo) return;
        // Demo: simula que el aspirante va cumpliendo sus misiones introductorias una tras otra (solo tras estar sano).
        if (HealthDone && introMissionsDone < introMissionsRequired && Time.time >= _nextAuto)
        {
            _nextAuto = Time.time + autoMissionEvery;
            CompleteMission("auto-demo");
        }
    }

    public bool HealthDone => _health == null || !_health.NeedsCare;

    /// <summary>Una misión introductoria completada. La 3ª deja listo al aspirante para ser certificado apto.</summary>
    public void CompleteMission(string label = null)
    {
        if (apt) return;
        introMissionsDone++;
        Debug.Log($"[Onboarding] «{name}»: misión introductoria {introMissionsDone}/{introMissionsRequired}" +
                  (string.IsNullOrEmpty(label) ? "." : $" ({label})."));
    }

    /// <summary>Sano + misiones hechas y aún sin certificar: lo que mira Linduarda para marcarlo apto.</summary>
    public bool ReadyForApt => !apt && HealthDone && introMissionsDone >= introMissionsRequired;

    /// <summary>Linduarda lo certifica (nace el pensamiento de la Magnate de presentarlo).</summary>
    public void CertifyApt()
    {
        if (apt) return;
        apt = true;
        Debug.Log($"[Onboarding] «{name}» CERTIFICADO APTO por la doctora → la Magnate lo presentará.");
    }
}
