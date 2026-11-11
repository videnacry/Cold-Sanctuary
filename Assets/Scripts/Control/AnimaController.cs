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

    float _lodNext;
    void Update()
    {
        IBrain best = PickBest();   // (cada fotograma: es barato, y POSEER tiene que mandar al momento — [1238] fallaba con el salto de abajo antes)
        if (best != _active)
        {
            Debug.Log($"[Control] «{name}» ahora conducido por: {(best != null ? best.BrainName : "nadie")}" +
                      $" (relevancia {(best != null ? best.Relevance : 0f):0.00}).");
            _active = best;
        }
        // (2026-11-09) Lejos y fuera de la vista, el cerebro ACTÚA cada 0,75 s (SimulationLod); nunca el del jugador ni uno poseído.
        if (_active == null || (!(_active is PlayerBrain) && !gameObject.CompareTag("Player") && SimulationLod.Skip(transform.position, ref _lodNext))) return;
        _active.Act(this);
    }

    IBrain PickBest()
    {
        IBrain best = null;
        float bestRel = float.NegativeInfinity;
        foreach (IBrain b in _brains)
        {
            // OJO: `b == null` compara por el tipo estático `IBrain` (interfaz) y NO detecta un
            // MonoBehaviour ya destruido (Destroy) — Unity solo sobrecarga `==` en `UnityEngine.Object`.
            // Sin el cast, un cerebro destruido (p. ej. el FollowBrain temporal de HelpRequest.EndShare)
            // sigue "vivo" para este chequeo y `PickBest` puede elegirlo, provocando
            // MissingReferenceException en `_active.Act(this)`.
            if ((b as Object) == null) continue;
            float r = b.Relevance;
            if (r > bestRel) { bestRel = r; best = b; }
        }
        return best;
    }
}
