using UnityEngine;

/// <summary>
/// El input del JUGADOR como un cerebro más (docs/anima-architecture.md §11.5: "el jugador es solo un
/// input"). Su relevancia = la fuerza de posesión inyectada por el <see cref="PossessionSpell"/>; 0 si el
/// jugador no está en este ser. Cuando es el cerebro activo, traduce input a acciones (mover/interactuar);
/// en este MVP solo lo registra por consola. Se inserta y se retira en runtime → posesión dinámica.
/// </summary>
public class PlayerBrain : MonoBehaviour, IBrain
{
    [Tooltip("Fuerza de posesión actual. 0 = el jugador no conduce este ser. La fija el PossessionSpell.")]
    public float possessionRelevance = 0f;

    public float Relevance => possessionRelevance;
    public string BrainName => "Jugador";

    bool _logged;

    public void Act(AnimaController ctrl)
    {
        // MVP: aquí iría la lectura real de input (mover/interactuar) enrutada a ESTE cuerpo.
        if (!_logged)
        {
            Debug.Log($"[Control] El jugador conduce «{ctrl.name}» (posesión {possessionRelevance:0.00}).");
            _logged = true;
        }
    }

    /// <summary>Libera el control (el jugador abandona este cuerpo → su IA retoma el mando).</summary>
    public void Release()
    {
        possessionRelevance = 0f;
        _logged = false;
    }
}
