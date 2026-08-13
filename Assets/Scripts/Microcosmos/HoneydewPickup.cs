using UnityEngine;

/// <summary>
/// GOTA DE MELAZA recolectable — aparece en el suelo junto a Ambrosio cuando
/// <see cref="HoneydewProducer"/> la «suelta». Al entrar el jugador en el trigger,
/// añade una carga a su <see cref="HoneydewSpell"/> y se destruye.
///
/// Nivel 1, Microcosmos: es la forma física del inventario de maleza (el jugador
/// ve cuántas gotas ha recogido por el contador <c>charges</c> del hechizo).
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoneydewPickup : MonoBehaviour
{
    [Tooltip("Segundos que dura la gota antes de desaparecer (0 = eterna).")]
    [Min(0f)] public float lifetime = 60f;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        var spell = other.GetComponent<HoneydewSpell>();
        if (spell == null) return;
        if (spell.AddCharge())
            Destroy(gameObject);
        // Si ya está al máximo de cargas, la gota permanece.
    }
}
