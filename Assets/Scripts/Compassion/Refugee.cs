using UnityEngine;

/// <summary>
/// Una persona a la que hay que PONER A SALVO en las simulaciones de compasión (docs/compassion-simulations.md): un
/// refugiado en Le Chambon, un niño del gueto con Irena Sendler. Reutiliza la marca de <see cref="WeakOne"/> (el débil
/// que el grupo cuida). El jugador se acerca y pulsa E: le SIGUE. En un <see cref="HidingPlace"/> o un
/// <see cref="SafeHaven"/> se queda. Una <see cref="Patrol"/> que lo vea fuera de un escondite y sin papeles lo
/// detiene: vuelve a su punto de partida (la misión retrocede; nunca se muestra violencia).
/// </summary>
[RequireComponent(typeof(WeakOne))]
public class Refugee : MonoBehaviour
{
    [Tooltip("Nombre que se guarda al ponerlo a salvo (Irena escribía los nombres reales para devolver a cada niño a su familia).")]
    public string displayName = "";
    [Tooltip("Tiene papeles falsos: las patrullas no lo detienen.")]
    public bool hasPapers;
    public float followSpeed = 3f;
    public float followDistance = 1.6f;
    [Tooltip("Radio alrededor de su punto de partida donde las patrullas no lo detienen (dentro del gueto, en la estación de llegada).")]
    public float homeRadius = 4f;

    public Transform Leader { get; private set; }
    public HidingPlace HiddenIn { get; private set; }
    public bool Saved { get; private set; }
    public bool Hidden => HiddenIn != null;
    public int TimesCaught { get; private set; }
    public Vector3 Origin => _origin;
    /// <summary>A la vista: fuera de un escondite, de su zona de partida y sin papeles.</summary>
    public bool Exposed => !Saved && !Hidden && !hasPapers && (transform.position - _origin).sqrMagnitude > homeRadius * homeRadius;

    Vector3 _origin;
    UnityEngine.AI.NavMeshAgent _nav;

    void Awake()
    {
        _origin = transform.position;
        _nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (string.IsNullOrEmpty(displayName)) displayName = name;
    }

    /// <summary>Le sigue (sale del escondite si estaba en uno).</summary>
    public void Follow(Transform leader)
    {
        if (Saved) return;
        if (HiddenIn != null) { HiddenIn.Release(this); HiddenIn = null; }
        Leader = leader;
    }

    public void Hide(HidingPlace place) { HiddenIn = place; Leader = null; Stop(); }

    public void MarkSaved() { Saved = true; Leader = null; Stop(); GetComponent<WeakOne>().safe = true; }

    /// <summary>Lo detiene una patrulla: vuelve al punto de partida, sin escondite ni guía.</summary>
    public void Caught()
    {
        if (Saved) return;
        TimesCaught++;
        if (HiddenIn != null) { HiddenIn.Release(this); HiddenIn = null; }
        Leader = null; Stop();
        if (_nav != null && _nav.isOnNavMesh) _nav.Warp(_origin); else transform.position = _origin;
    }

    void Stop() { if (_nav != null && _nav.isOnNavMesh) _nav.ResetPath(); }

    void Update()
    {
        if (Leader == null || Saved) return;
        Vector3 to = Leader.position - transform.position; to.y = 0f;
        if (to.magnitude <= followDistance) { Stop(); return; }
        if (_nav != null && _nav.isOnNavMesh) { _nav.speed = followSpeed; _nav.SetDestination(Leader.position); return; }
        Vector3 dir = to.normalized;
        transform.position += dir * followSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), 360f * Time.deltaTime);
    }
}
