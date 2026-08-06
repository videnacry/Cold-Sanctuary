using UnityEngine;

/// <summary>
/// CONSTITUCIÓN química de un ser (docs/stats-as-truth.md §9) — el **fundamento** de los stats base, por
/// **niveles de organización** (progresivo): **elementos** (tabla periódica) → **compuestos** → **células** →
/// **stats**. Ej.: Fe+proteína → glóbulos → aguante; proteína+ATP → músculo → fuerza/agilidad; minerales+
/// proteína → hueso → fuerza/masa; lípidos+ATP → neurona → percepción. Aplica el resultado a los campos de
/// `Anima` con el **mismo delta gestionado** que la composición (no pisa evolución/transformación; opt-in).
/// **Neutro por defecto** (todo a 1 → células a 1 → delta 0); cambiar la química = mover los stats base.
/// Fase futura: derivar los **elementos** de `Chemistry` (tabla periódica) y las recetas reales.
/// </summary>
public class Constitution : MonoBehaviour
{
    public Anima anima;

    [Header("Nivel 1 — elementos (1.0 = normal; ver Chemistry)")]
    public float carbon = 1f, hydrogen = 1f, oxygen = 1f, nitrogen = 1f, calcium = 1f, iron = 1f;

    [Tooltip("Velocidad con que el cuerpo se reconfigura al cambiar la química.")]
    public float adaptSpeed = 0.5f;

    // Nivel 2 — compuestos (derivados). Nivel 3 — células (derivadas). Solo lectura en juego.
    float _protein, _atp, _minerals, _lipids;
    float _muscle, _blood, _neuron, _bone;
    // Delta aplicado a los stats base (gestionado).
    float _aStr, _aAgi, _aEnd, _aMass, _aPer;

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); }

    void Update()
    {
        if (anima == null) return;
        DeriveUpward();

        // Stats objetivo desde las CÉLULAS (baseline 1.0 → sin las células a 1, delta 0).
        float tStr = _muscle * 0.6f + _bone * 0.4f;
        float tEnd = _muscle * 0.4f + _blood * 0.6f;
        float tAgi = _muscle * 0.4f + _neuron * 0.4f + 0.2f;
        float tPer = _neuron * 0.7f + 0.3f;
        float tMass = _bone * 0.5f + _muscle * 0.5f;

        float k = Mathf.Clamp01(Time.deltaTime * adaptSpeed);
        Step(ref anima.strength,   ref _aStr,  tStr  - 1f, k);
        Step(ref anima.endurance,  ref _aEnd,  tEnd  - 1f, k);
        Step(ref anima.agility,    ref _aAgi,  tAgi  - 1f, k);
        Step(ref anima.perception, ref _aPer,  tPer  - 1f, k);
        Step(ref anima.bodyMass,   ref _aMass, tMass - 1f, k);
    }

    void DeriveUpward()
    {
        // Nivel 2: compuestos desde elementos (recetas aproximadas, tuneables).
        _protein  = (nitrogen + carbon + hydrogen) / 3f;
        _atp      = (carbon + hydrogen + oxygen) / 3f;
        _minerals = (calcium + iron) / 2f;
        _lipids   = (carbon + hydrogen) / 2f;
        // Nivel 3: células desde compuestos.
        _muscle = (_protein + _atp) / 2f;
        _blood  = (iron + _protein) / 2f;      // glóbulos (hierro + proteína)
        _neuron = (_lipids + _atp) / 2f;
        _bone   = (_minerals + _protein) / 2f;
    }

    // Lleva `applied` hacia `target` (reconfiguración progresiva) y ajusta el campo por la diferencia (gestionado).
    static void Step(ref float field, ref float applied, float target, float k)
    {
        float next = Mathf.Lerp(applied, target, k);
        field += next - applied;
        applied = next;
    }
}
