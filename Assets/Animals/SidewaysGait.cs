using UnityEngine;

/// <summary>
/// MARCHA DE LADO del cangrejo (docs/era-species-behavior.md §5): el cangrejo fantasma corre de costado —sus patas se
/// doblan mejor hacia los lados— y es de los artrópodos más rápidos (Burrows &amp; Hoyle 1973, J Exp Biol 58:327; Blickhan
/// &amp; Full 1987, J Exp Biol 130:155). El NavMeshAgent sigue llevando la ruta; aquí solo se gira el CUERPO para que el
/// costado mire hacia donde avanza (alternando el lado de vez en cuando, como hacen). Parado, o erguido y lúcido (camina
/// como una persona), mira al frente. Auto-añadido
/// en `Animal.Init` a las especies de <see cref="Has"/>.
/// </summary>
[RequireComponent(typeof(Animal))]
public class SidewaysGait : MonoBehaviour
{
    public static bool Has(string species) => species == "GhostCrab";

    [Tooltip("Velocidad de giro del cuerpo (°/s).")]
    public float turnSpeed = 540f;

    Animal _a;
    float _side = 1f, _nextSwap;

    void Start() { _a = GetComponent<Animal>(); }

    void LateUpdate()
    {
        if (_a == null || _a.nav == null || !_a.nav.enabled || _a.death) return;
        Vector3 v = _a.nav.velocity; v.y = 0f;
        // Erguido (lúcido, con mente humana) camina de frente como una persona; de lado solo cuando manda el cuerpo.
        bool moving = v.sqrMagnitude > 0.04f && !(TryGetComponent(out Lucidity l) && l.Lucid);
        _a.nav.updateRotation = !moving;
        if (!moving) return;
        if (Time.time >= _nextSwap) { _nextSwap = Time.time + Random.Range(2f, 6f); if (Random.value < 0.35f) _side = -_side; }
        // Costado hacia el avance: el frente queda a 90° del movimiento.
        Quaternion want = Quaternion.LookRotation(Quaternion.Euler(0f, 90f * _side, 0f) * v.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, want, turnSpeed * Time.deltaTime);
    }
}
