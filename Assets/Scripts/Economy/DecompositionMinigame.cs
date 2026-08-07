using System.Collections.Generic;
using UnityEngine;

/// <summary>Una muestra de la jornada (compuesto / átomo / núcleo / nucleón según el santuario).</summary>
[System.Serializable]
public class DecompSample
{
    public string trueName = "H2O";
    [Tooltip("Nombres mostrados en la fase de IDENTIFICAR (uno es el correcto). Vacío = solo trueName.")]
    public List<string> nameOptions = new List<string>();
    [Tooltip("Componentes que libera al romperse bien (símbolo → gramos).")]
    public List<ElementAmount> components = new List<ElementAmount>();
    [Tooltip("Energía (J) que libera romperlo con timing perfecto.")]
    public float energyJoules = 0f;
}

/// <summary>
/// MINIJUEGO DE DESCOMPOSICIÓN (docs/magic-metabolism-progression.md §17). La **jornada** presenta un lote de
/// muestras en **TRES FASES** sobre **las mismas muestras y el mismo orden**:
///   1) **IDENTIFICAR** — se muestra la muestra con varios nombres alrededor (uno correcto); el jugador
///      selecciona el correcto **antes de que se acabe el tiempo**.
///   2) **ROMPER** — vuelven en el mismo orden; el jugador ejecuta la ruptura (calidad por **timing** 0..1 →
///      cuánta energía captura).
///   3) **CLASIFICAR** — vuelven en el mismo orden; el jugador **arrastra** cada componente liberado a su
///      **contenedor** (símbolo) antes de que se acabe el tiempo; al acabarse, se **guardan los bien
///      clasificados** y pasa a la siguiente muestra.
/// Un componente cuenta en el `yield` si su muestra fue **identificada + rota + bien clasificada** (escalado por
/// la calidad de ruptura). Al terminar la jornada, vuelca `yield`/`energyJoules` en el `DecompositionJob` y llama
/// a `Complete()` (reparto **economía + paga**). Más rápido = más muestras por jornada = más progreso. Opt-in;
/// prototipo OnGUI para probar en escena. *Falta:* la UI real de arrastre + fichas/visuales por nivel.
/// </summary>
public class DecompositionMinigame : MonoBehaviour
{
    public DecompositionJob job;

    [Header("Muestras de la jornada (mismo orden en las 3 fases)")]
    public List<DecompSample> batch = new List<DecompSample>();
    [Tooltip("Tiempo por muestra en cada fase (s). Más rápido = más muestras por jornada.")]
    public float perItemSeconds = 5f;

    public enum Phase { Idle, Identify, Break, Classify, Finished }
    public Phase phase = Phase.Idle;

    int _i;             // muestra actual en la fase
    int _classifyIdx;   // componente actual en clasificación
    float _timer;
    bool[] _identified;
    float[] _breakQ;
    bool[][] _classified;   // [muestra][componente]

    readonly List<ElementAmount> _yield = new List<ElementAmount>();
    float _energy;

    public DecompSample Current => (_i >= 0 && _i < batch.Count) ? batch[_i] : null;
    public float ItemTimeLeft => _timer;

    /// <summary>Empieza la jornada (fase 1: identificar).</summary>
    public void Begin()
    {
        if (batch.Count == 0) { Debug.Log("[Descomp-minijuego] jornada vacía."); return; }
        _identified = new bool[batch.Count];
        _breakQ = new float[batch.Count];
        _classified = new bool[batch.Count][];
        for (int k = 0; k < batch.Count; k++)
        {
            int n = (batch[k] != null && batch[k].components != null) ? batch[k].components.Count : 0;
            _classified[k] = new bool[n];
        }
        _yield.Clear();
        _energy = 0f;
        StartPhase(Phase.Identify);
    }

    void StartPhase(Phase p) { phase = p; _i = 0; _classifyIdx = 0; _timer = perItemSeconds; }

    void Update()
    {
        if (phase == Phase.Idle || phase == Phase.Finished) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f) NextItem();
    }

    /// <summary>Fase 1: el jugador elige un nombre. Correcto → muestra identificada.</summary>
    public void SubmitName(string chosen)
    {
        if (phase != Phase.Identify || Current == null) return;
        if (chosen == Current.trueName) _identified[_i] = true;
        NextItem();
    }

    /// <summary>Fase 2: romper con calidad de timing 0..1 (1 = perfecto → toda la energía). Sin identificar, nada.</summary>
    public void SubmitBreak(float quality)
    {
        if (phase != Phase.Break || Current == null) return;
        if (_identified[_i]) { _breakQ[_i] = Mathf.Clamp01(quality); _energy += Current.energyJoules * _breakQ[_i]; }
        NextItem();
    }

    /// <summary>Fase 3: clasificar el componente actual en un contenedor (símbolo). Correcto si coincide.</summary>
    public void Classify(string containerSymbol)
    {
        if (phase != Phase.Classify || Current == null) return;
        List<ElementAmount> comps = Current.components;
        if (comps != null && _classifyIdx < comps.Count)
        {
            ElementAmount c = comps[_classifyIdx];
            if (c != null && c.symbol == containerSymbol) _classified[_i][_classifyIdx] = true;
            _classifyIdx++;
        }
        if (comps == null || _classifyIdx >= comps.Count) NextItem();
    }

    void NextItem()
    {
        _i++;
        _classifyIdx = 0;
        _timer = perItemSeconds;
        if (_i >= batch.Count)
        {
            if (phase == Phase.Identify) StartPhase(Phase.Break);
            else if (phase == Phase.Break) StartPhase(Phase.Classify);
            else Finish();
        }
    }

    void Finish()
    {
        phase = Phase.Finished;
        // yield = componentes de muestras identificadas + rotas + bien clasificadas (escalado por calidad de ruptura).
        for (int k = 0; k < batch.Count; k++)
        {
            if (batch[k] == null || !_identified[k] || _breakQ[k] <= 0f) continue;
            List<ElementAmount> comps = batch[k].components;
            if (comps == null) continue;
            for (int c = 0; c < comps.Count && c < _classified[k].Length; c++)
                if (_classified[k][c] && comps[c] != null) AddYield(comps[c].symbol, comps[c].amount * _breakQ[k]);
        }
        if (job != null) { job.yield = _yield; job.energyJoules = _energy; job.Complete(); }
        Debug.Log($"[Descomp-minijuego] jornada terminada: {_yield.Count} elementos + {_energy:0} J → reparto (economía+paga).");
    }

    void AddYield(string symbol, float grams)
    {
        if (grams <= 0f) return;
        foreach (ElementAmount e in _yield) if (e != null && e.symbol == symbol) { e.amount += grams; return; }
        _yield.Add(new ElementAmount { symbol = symbol, amount = grams });
    }

    // --- Prototipo OnGUI (para probar el flujo en escena; la UI real será arrastre + visuales) ---
    void OnGUI()
    {
        if (phase == Phase.Idle)
        {
            if (GUI.Button(new Rect(10, 10, 180, 30), "Iniciar jornada")) Begin();
            return;
        }
        if (phase == Phase.Finished)
        {
            GUI.Label(new Rect(10, 10, 500, 24), $"Jornada terminada: {_yield.Count} elementos + {_energy:0} J.");
            return;
        }
        GUI.Label(new Rect(10, 10, 520, 24), $"Fase {phase} — muestra {_i + 1}/{batch.Count} — {_timer:0.0}s");
        DecompSample s = Current;
        if (s == null) return;

        if (phase == Phase.Identify)
        {
            GUI.Label(new Rect(10, 40, 520, 24), "Identifica la muestra (elige el nombre correcto):");
            List<string> opts = (s.nameOptions != null && s.nameOptions.Count > 0)
                ? s.nameOptions : new List<string> { s.trueName };
            for (int o = 0; o < opts.Count; o++)
                if (GUI.Button(new Rect(10, 70 + o * 30, 220, 26), opts[o])) SubmitName(opts[o]);
        }
        else if (phase == Phase.Break)
        {
            GUI.Label(new Rect(10, 40, 520, 24), $"Rompe «{s.trueName}» (calidad por timing):");
            if (GUI.Button(new Rect(10, 70, 120, 26), "Perfecto")) SubmitBreak(1f);
            if (GUI.Button(new Rect(140, 70, 120, 26), "Regular")) SubmitBreak(0.5f);
            if (GUI.Button(new Rect(270, 70, 120, 26), "Fallo")) SubmitBreak(0f);
        }
        else if (phase == Phase.Classify)
        {
            List<ElementAmount> comps = s.components;
            if (comps != null && _classifyIdx < comps.Count)
            {
                ElementAmount c = comps[_classifyIdx];
                GUI.Label(new Rect(10, 40, 520, 24),
                    $"Arrastra el componente «{(c != null ? c.symbol : "?")}» ({_classifyIdx + 1}/{comps.Count}) a su contenedor:");
                List<string> syms = new List<string>();
                foreach (ElementAmount cc in comps) if (cc != null && !syms.Contains(cc.symbol)) syms.Add(cc.symbol);
                for (int o = 0; o < syms.Count; o++)
                    if (GUI.Button(new Rect(10 + o * 70, 70, 64, 26), syms[o])) Classify(syms[o]);
            }
        }
    }
}
