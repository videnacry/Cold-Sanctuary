using System.Collections.Generic;
using UnityEngine;

/// <summary>Cantidad de un elemento (símbolo de la tabla periódica, `Chemistry`). 1.0 = normal.</summary>
[System.Serializable]
public class ElementAmount
{
    public string symbol = "C";
    public float amount = 1f;
}

/// <summary>
/// CONSTITUCIÓN química de un ser (docs/stats-as-truth.md §9) — el **fundamento** de los stats base, por
/// **niveles de organización**: **elementos** (símbolos reales de la tabla periódica, `Chemistry`) →
/// **compuestos** (proteína/ATP/minerales/lípidos) → **células** (músculo/**glóbulos**=Fe+proteína/neurona/
/// hueso) → **stats base**. Los elementos se validan contra `PeriodicTableManager` y son **alimentables**
/// (`AddElement`) desde el juego (absorber/comer elementos → cambia la constitución → mueve los stats).
/// Aplica por el **mismo delta gestionado** que la composición (no pisa evolución/transform; opt-in). Neutro
/// por defecto (todo a 1 → delta 0).
/// </summary>
public class Constitution : MonoBehaviour
{
    public Anima anima;

    [Header("Nivel 1 — elementos (símbolos de la tabla periódica). Vacío = se siembran los bioelementos.")]
    public List<ElementAmount> elements = new List<ElementAmount>();

    [Tooltip("Velocidad con que el cuerpo se reconfigura al cambiar la química.")]
    public float adaptSpeed = 0.5f;

    // Nivel 2 — compuestos. Nivel 3 — células. (Derivados; solo lectura en juego.)
    float _protein, _atp, _minerals, _lipids;
    float _muscle, _blood, _neuron, _bone;
    // Delta aplicado a los stats base (gestionado).
    float _aStr, _aAgi, _aEnd, _aMass, _aPer;

    static readonly string[] BioElements = { "C", "H", "O", "N", "Ca", "Fe" };

    void Awake()
    {
        if (anima == null) anima = GetComponent<Anima>();
        if (elements.Count == 0)
            foreach (string s in BioElements) elements.Add(new ElementAmount { symbol = s, amount = 1f });
        ValidateAgainstChemistry();
    }

    void ValidateAgainstChemistry()
    {
        PeriodicTableManager pt = PeriodicTableManager.Instance;
        if (pt == null) return;   // sin tabla en escena → no validar
        foreach (ElementAmount e in elements)
            if (e != null && !string.IsNullOrEmpty(e.symbol) && pt.GetData(e.symbol) == null)
                Debug.LogWarning($"[Constitution] «{name}»: símbolo fuera de la tabla periódica: {e.symbol}");
    }

    /// <summary>Cantidad del elemento (1.0 si no está en la lista = normal, no altera).</summary>
    public float El(string symbol)
    {
        foreach (ElementAmount e in elements)
            if (e != null && e.symbol == symbol) return e.amount;
        return 1f;
    }

    /// <summary>Alimenta la constitución (absorber/comer un elemento del juego → cambia los stats base).</summary>
    public void AddElement(string symbol, float delta)
    {
        foreach (ElementAmount e in elements)
            if (e != null && e.symbol == symbol) { e.amount = Mathf.Max(0f, e.amount + delta); return; }
        elements.Add(new ElementAmount { symbol = symbol, amount = Mathf.Max(0f, 1f + delta) });
    }

    void Update()
    {
        if (anima == null) return;
        DeriveUpward();

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
        _protein  = (El("N") + El("C") + El("H")) / 3f;
        _atp      = (El("C") + El("H") + El("O")) / 3f;
        _minerals = (El("Ca") + El("Fe")) / 2f;
        _lipids   = (El("C") + El("H")) / 2f;
        // Nivel 3: células desde compuestos.
        _muscle = (_protein + _atp) / 2f;
        _blood  = (El("Fe") + _protein) / 2f;     // glóbulos (hierro + proteína)
        _neuron = (_lipids + _atp) / 2f;
        _bone   = (_minerals + _protein) / 2f;
    }

    static void Step(ref float field, ref float applied, float target, float k)
    {
        float next = Mathf.Lerp(applied, target, k);
        field += next - applied;
        applied = next;
    }
}
