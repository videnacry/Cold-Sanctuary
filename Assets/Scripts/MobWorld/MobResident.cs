using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lightweight resident of a mob world (docs mob-world-architecture §5). Unlike the human
/// <c>WorldCharacter</c> (full autonomous simulation), a MobResident is STATIC at its post and
/// only moves / levels up when the MobWorldDirector fires an event. It offers a role/service
/// (WoW-style shop NPC) and is interactable.
///
/// Cheap by design: many of these can populate a mob city without the cost of the human sim.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MobResident : MonoBehaviour, IInteractable
{
    [Header("Identity")]
    public string residentName = "Habitante";
    [Tooltip("Rol/servicio que ofrece (Cocinero, Jardinero, Vendedor…).")]
    public string role = "Vendedor";

    [Min(1)] public int level = 1;

    [Header("Movement (event-driven only)")]
    [Tooltip("Velocidad al desplazarse durante un evento. Fuera de eventos, está quieto en su puesto.")]
    public float moveSpeed = 2f;
    [Tooltip("Distancia a la que se considera que llegó a su destino.")]
    public float arriveDistance = 0.3f;

    [Header("Interaction")]
    [Tooltip("Se dispara al interactuar (abrir tienda/diálogo/misión de su rol).")]
    public UnityEvent onInteract;

    Vector3 _home;
    Vector3 _target;
    bool    _moving;

    // ── IInteractable ─────────────────────────────────────────────────────────

    public string InteractLabel => $"Hablar con {residentName} ({role})";
    public bool   CanInteract   => !_moving;              // atiende sólo en su puesto
    public void   Interact()    => onInteract?.Invoke();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        _home = transform.position;
        _target = _home;

        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger) col.isTrigger = true;

        MobWorldDirector.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (MobWorldDirector.HasInstance) MobWorldDirector.Instance.Unregister(this);
    }

    void Update()
    {
        if (!_moving) return;

        transform.position = Vector3.MoveTowards(transform.position, _target, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, _target) <= arriveDistance)
            _moving = false;
    }

    // ── Event-driven API (called by MobWorldDirector) ──────────────────────────

    public Vector3 HomePost => _home;

    public void LevelUp(int amount = 1) => level = Mathf.Max(1, level + amount);

    public void MoveTo(Vector3 worldPos) { _target = worldPos; _moving = true; }

    public void ReturnHome() { _target = _home; _moving = true; }
}
