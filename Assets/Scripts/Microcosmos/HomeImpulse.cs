using UnityEngine;

/// <summary>
/// Impulso de HOGAR — atracción continua y baja hacia <see cref="homePosition"/>
/// (el nido/cueva). Actúa sobre el <see cref="ImpulseController"/> del mismo GO.
///
/// La magnitud base sube cuando el estrés del ser sube (quiere más seguridad) y
/// se multiplica por el bond con el grupo (si hay compañeros, el hogar colectivo importa
/// más que el propio). Así cuando todas las hormigas tienen estrés por los depredadores,
/// el impulso de hogar domina progresivamente sobre el vagabundeo.
///
/// Si el ser llega a <see cref="arrivalRadius"/> del home, el impulso se pone a cero
/// (ya está en casa).
///
/// Nivel 1: las hormigas viejas tienen HomePosition = cueva; sin hechizo de debilitamiento
/// este impulso las llevaría solas hacia allí. El debilitamiento les quita la energía para
/// actuar sobre el impulso (ImpulseController.walkEnergyCostPerSecond no puede pagarse).
/// </summary>
[RequireComponent(typeof(ImpulseController))]
public class HomeImpulse : MonoBehaviour
{
    [Header("Hogar")]
    [Tooltip("Posición del hogar/nido. Puede actualizarse en runtime (hogar dinámico).")]
    public Vector3 homePosition;

    [Tooltip("Magnitud base del impulso de hogar (baja = fondo de comportamiento).")]
    [Min(0f)] public float baseMagnitude = 0.4f;

    [Tooltip("Magnitud extra añadida por cada 0.1 de estrés del ser (hasta un máximo de stressMaxBonus).")]
    [Min(0f)] public float stressBonusPerUnit = 1.5f;

    [Tooltip("Máximo de magnitud por estrés (tope para evitar que se vuelva infinito).")]
    [Min(0f)] public float stressMaxBonus = 4f;

    [Tooltip("Distancia a la que el ser se considera 'en casa' y el impulso cae a cero.")]
    [Min(0.1f)] public float arrivalRadius = 2.5f;

    [Tooltip("Frecuencia de actualización del impulso (s).")]
    [Min(0.1f)] public float updateRate = 0.5f;

    // ── Estado ─────────────────────────────────────────────────────────────

    ImpulseController _ctrl;
    Anima             _anima;
    float             _next;

    const string TAG = "home";

    // ── Ciclo ──────────────────────────────────────────────────────────────

    void Awake()
    {
        _ctrl  = GetComponent<ImpulseController>();
        _anima = GetComponent<Anima>();
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + updateRate;

        Vector3 toHome = homePosition - transform.position;

        // Ya en casa: eliminar impulso.
        if (toHome.magnitude <= arrivalRadius)
        {
            _ctrl.RemoveByTag(TAG);
            return;
        }

        float stress = _anima != null ? _anima.stress : 0f;
        float bonus  = Mathf.Min(stress * stressBonusPerUnit, stressMaxBonus);
        float mag    = baseMagnitude + bonus;

        _ctrl.RemoveByTag(TAG);
        _ctrl.AddImpulse(new MovementImpulse(TAG, toHome, mag, 0f)); // 0 decaimiento = persistente
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.9f, 0.4f, 0.4f);
        Gizmos.DrawWireSphere(homePosition, arrivalRadius);
        Gizmos.DrawLine(transform.position, homePosition);
    }
}
