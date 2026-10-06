using UnityEngine;

/// <summary>
/// NECESIDAD DE DEFECAR (popó) — una pulsión como el hambre o el sueño, anclada en biología real: lo que el intestino
/// no digiere (**fibra**) forma el bolo y empuja el **tránsito**; cuanto más se come (y más fibra: herbívoros/`eatsGrass`),
/// antes llega la urgencia. Crece hasta volverse fuerte; al máximo, el ser **defeca**: se alivia, deja **excremento**
/// (rastro de olor + prefab opcional = abono / bola del pelotero) y recupera algo de satisfacción. Cuando la urgencia
/// es alta **incomoda** (sube `stress`) → participa del campo de pensamientos y del control-por-necesidad
/// (`AcuteStressResponse`) igual que el hambre. docs/microcosmos-dungbeetle-level.md (necesidades) + magic-metabolism.
///
/// Es también la "acción natural" del futuro **hechizo de generar excremento** (SpawnSpell): defecar a demanda deja el
/// prefab en el sitio. Opt-in por ahora (añádelo al ser); barato.
/// </summary>
[RequireComponent(typeof(Anima))]
public class DefecationNeed : MonoBehaviour
{
    [Header("Ritmo (fracción de urgencia por minuto de juego)")]
    [Tooltip("Cuánto sube la urgencia por minuto de juego cuando el ser está BIEN alimentado (digiriendo).")]
    [Range(0f, 1f)] public float basePerMinute = 0.25f;
    [Tooltip("Multiplicador de fibra: dietas con `eatsGrass` (herbívoro) transitan más rápido.")]
    [Min(1f)] public float fiberFactor = 1.6f;

    [Header("Molestia")]
    [Tooltip("Urgencia a partir de la cual empieza a incomodar (sube estrés).")]
    [Range(0f, 1f)] public float discomfortAt = 0.7f;

    [Header("Excremento")]
    [Tooltip("Prefab opcional a soltar al defecar (abono / bola del pelotero). Vacío = solo alivio + olor.")]
    public GameObject fecesPrefab;
    [Tooltip("Radio del rastro de olor que deja el excremento (via Scent si está disponible).")]
    [Min(0f)] public float scentRadius = 3f;

    [Range(0f, 1f)] public float urge;   // 0..1 estado de la necesidad (visible/leíble como los otros drives)

    Anima _anima;
    Animal _animal;

    void Awake()
    {
        _anima = GetComponent<Anima>();
        _animal = GetComponent<Animal>();
    }

    void Update()
    {
        if (_anima == null || _anima.death) return;
        float dt = Time.deltaTime;
        float minuteSecs = TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 60f;

        // BIEN ALIMENTADO digiere → produce bolo; el hambre alta (poco que digerir) frena. Fibra acelera.
        float fed = _animal != null ? 1f - Mathf.Clamp01(_animal.hungry) : 0.5f;
        float fiber = (_animal != null && _animal.Forage != null && _animal.Forage.eatsGrass) ? fiberFactor : 1f;
        urge = Mathf.Clamp01(urge + basePerMinute * fed * fiber * (dt / minuteSecs));

        // SUPRESIÓN (idea del autor): NO se defeca en peligro ni dormido (se aguanta por una necesidad más fuerte: la
        // de un entorno seguro / el sueño); y el CONTROL MENTAL (disciplina+compostura) sube el umbral de continencia.
        // La urgencia sigue subiendo (tope 1) y aguantar INCOMODA (sube estrés) → compite en el campo de pensamientos y
        // en el control-por-necesidad, pudiendo a su vez vencer a otros impulsos si es lo bastante fuerte.
        bool safe = !_anima.asleep && !_anima.aware && _anima.alertness < 0.5f;
        float mentalControl = Mathf.Clamp01((_anima.discipline + _anima.composure) / 4f);
        float continence = Mathf.Min(0.98f, 0.55f + 0.43f * mentalControl);   // más control → aguanta hasta más urgencia

        if (urge > discomfortAt)
            _anima.stress = Mathf.Clamp01(_anima.stress + 0.05f * (urge - discomfortAt) * (safe ? 1f : 1.6f) * (dt / minuteSecs));

        if (safe && urge >= continence) Defecate();
    }

    /// <summary>Defecar: alivio + excremento (prefab opcional) + rastro de olor. También es lo que dispararía el hechizo
    /// de generación a demanda.</summary>
    public void Defecate()
    {
        urge = 0f;
        _anima.satisfaction = Mathf.Clamp01(_anima.satisfaction + 0.1f);           // el alivio da algo de satisfacción
        _anima.stress = Mathf.Max(0f, _anima.stress - 0.05f);

        Vector3 pos = transform.position + transform.forward * -0.3f;              // detrás del ser
        if (fecesPrefab != null)
        {
            GameObject feces = Instantiate(fecesPrefab, pos, Quaternion.identity);
            if (scentRadius > 0f && feces.GetComponent<ScentEmitter>() == null)
            {
                ScentEmitter se = feces.AddComponent<ScentEmitter>();
                se.radius = scentRadius;
            }
        }
    }
}
