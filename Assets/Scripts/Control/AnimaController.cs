using UnityEngine;

/// <summary>
/// Capa de CONTROL intercambiable de un `Anima` (docs/anima-architecture.md §11.5): reúne los cerebros
/// (<see cref="IBrain"/>) del objeto y cede el mando al de MAYOR relevancia. Así el mismo ser lo conduce
/// su IA o el jugador (posesión) sin cambiar de clase — "controlar = enchufar el cerebro de input en
/// vez del de IA". Barato: solo compara relevancias y delega en el activo.
/// </summary>
[DisallowMultipleComponent]
public class AnimaController : MonoBehaviour
{
    IBrain[] _brains = new IBrain[0];
    IBrain _active;

    /// <summary>El cerebro que conduce ahora (mayor relevancia). Null si no hay ninguno.</summary>
    public IBrain Active => _active;

    void Awake() => RefreshBrains();

    /// <summary>Re-escanea los cerebros del objeto. Llámalo si añades/quitas uno en runtime (p. ej. posesión).</summary>
    public void RefreshBrains() => _brains = GetComponents<IBrain>();

    void Update()
    {
        IBrain best = PickBest();
        if (best != _active)
        {
            Debug.Log($"[Control] «{name}» ahora conducido por: {(best != null ? best.BrainName : "nadie")}" +
                      $" (relevancia {(best != null ? best.Relevance : 0f):0.00}).");
            _active = best;
        }
        if (_active != null) _active.Act(this);
    }

    IBrain PickBest()
    {
        IBrain best = null;
        float bestRel = float.NegativeInfinity;
        foreach (IBrain b in _brains)
        {
            if (b == null) continue;
            float r = b.Relevance;
            if (r > bestRel) { bestRel = r; best = b; }
        }
        return best;
    }
}
