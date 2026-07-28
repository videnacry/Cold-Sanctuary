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

    [Header("Movimiento (cuando el jugador conduce este cuerpo)")]
    [Tooltip("Velocidad de desplazamiento (m/s), escalable por la agilidad del ser en el futuro.")]
    public float moveSpeed = 4f;
    [Tooltip("Velocidad de giro hacia la dirección de avance (grados/s).")]
    public float turnSpeed = 540f;

    public float Relevance => possessionRelevance;
    public string BrainName => "Jugador";

    bool _logged;
    CharacterController _cc;

    void Awake() => _cc = GetComponent<CharacterController>();

    public void Act(AnimaController ctrl)
    {
        if (!_logged)
        {
            Debug.Log($"[Control] El jugador conduce «{ctrl.name}» (posesión {possessionRelevance:0.00}).");
            _logged = true;
        }

        // Input → mover ESTE cuerpo (los ejes por defecto de Unity: WASD / flechas).
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (_cc != null && _cc.enabled) _cc.SimpleMove(move * moveSpeed);          // respeta gravedad/colisiones
        else transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);   // fallback sin CharacterController

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
        }
    }

    /// <summary>Libera el control (el jugador abandona este cuerpo → su IA retoma el mando).</summary>
    public void Release()
    {
        possessionRelevance = 0f;
        _logged = false;
    }
}
