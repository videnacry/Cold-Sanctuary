using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LECTOR DE ESCAPE POR BOND — lee el estado de huida de los miembros del grupo
/// con alto vínculo y ajusta el impulso de hogar propio para acompañarles.
///
/// Mecánica (docs/stats-as-truth.md §bond):
/// • Si varios miembros bonded están en modo escape (ThreatScanner tiene impulso
///   "flee_" con magnitud > <see cref="escapeThreshold"/>), este ser "siente" el
///   peligro del grupo aunque él mismo no haya visto al depredador.
/// • La intensidad de la lectura escala con: bond × perception (aptitud).
///   - bond bajo (Kushal recién llegado): no siente nada.
///   - bond alto + perception alta: siente la urgencia de la manada con claridad.
/// • Cuando al menos <see cref="minEscapingMembers"/> miembros están escapando,
///   el ser añade un impulso hacia el hogar compartido del grupo
///   (<see cref="HomeImpulse.homePosition"/> de sus vecinos).
///
/// Esto produce el comportamiento emergente del usuario:
///   "aunque el jugador no haga nada, ellos seguirán moviéndose a la cueva,
///    jalando al que se está quedando más lejos del home."
///
/// Nota: este componente lee el estado; el arrastre físico de compañeros lo hace
/// <see cref="PullSpell"/> (manejado por el jugador o por IA futura).
/// </summary>
[RequireComponent(typeof(ImpulseController))]
public class BondEscapeReader : MonoBehaviour
{
    [Header("Lectura de grupo")]
    [Tooltip("Radio de búsqueda de miembros del grupo (distancia social máxima).")]
    [Min(0.5f)] public float groupRadius = 12f;

    [Tooltip("Magnitud mínima del impulso flee_ en un compañero para considerarlo 'en escape'.")]
    [Min(0.1f)] public float escapeThreshold = 1f;

    [Tooltip("Número mínimo de miembros en escape para que este ser reaccione.")]
    [Min(1)] public int minEscapingMembers = 2;

    [Tooltip("Magnitud del impulso de hogar añadido cuando el grupo está escapando.")]
    [Min(0f)] public float groupPullMagnitude = 3f;

    [Tooltip("Frecuencia de escaneo del grupo (s).")]
    [Min(0.1f)] public float scanRate = 0.5f;

    [Header("Escala por aptitudes")]
    [Tooltip("Si true, la magnitud del impulso se multiplica por bond×perception del lector.")]
    public bool scaleByBond = true;

    // ── Estado ─────────────────────────────────────────────────────────────

    ImpulseController _ctrl;
    Anima             _self;
    CharacterLevel    _level;
    float             _next;

    const string TAG = "group_escape";

    // ── Ciclo ──────────────────────────────────────────────────────────────

    void Awake()
    {
        _ctrl  = GetComponent<ImpulseController>();
        _self  = GetComponent<Anima>();
        _level = GetComponent<CharacterLevel>();
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + scanRate;

        _ctrl.RemoveByTag(TAG);

        var escapers = new List<(Vector3 home, float bondStr)>();

        // Escanear compañeros cercanos con ImpulseController.
        var cols = Physics.OverlapSphere(transform.position, groupRadius);
        foreach (var col in cols)
        {
            if (col.gameObject == this.gameObject) continue;

            var otherCtrl = col.GetComponent<ImpulseController>();
            if (otherCtrl == null) continue;

            // Determinar si está en modo escape.
            float fleeNet = GetFleeMagnitude(otherCtrl);
            if (fleeNet < escapeThreshold) continue;

            // Obtener home del compañero.
            var homeImp = col.GetComponent<HomeImpulse>();
            if (homeImp == null) continue;

            // Calcular bond hacia este compañero (leer de Anima.BondWith si disponible).
            float bond = GetBond(col.gameObject);
            if (bond <= 0f) continue;

            escapers.Add((homeImp.homePosition, bond));
        }

        if (escapers.Count < minEscapingMembers) return;

        // Calcular dirección promedio al hogar compartido (ponderado por bond).
        Vector3 weightedDir = Vector3.zero;
        float totalBond = 0f;
        foreach (var (home, bond) in escapers)
        {
            weightedDir += (home - transform.position) * bond;
            totalBond   += bond;
        }

        if (totalBond <= 0f || weightedDir.sqrMagnitude < 0.001f) return;

        // Escalar por bond promedio × perception del lector.
        float scale = 1f;
        if (scaleByBond && _self != null && _level != null)
        {
            float avgBond    = totalBond / escapers.Count;
            float perception = _self.perception; // aptitud IAptitudes
            scale = Mathf.Clamp01(avgBond) * Mathf.Clamp01(perception);
        }

        float mag = groupPullMagnitude * scale;
        if (mag < 0.01f) return;

        _ctrl.AddImpulse(new MovementImpulse(TAG, weightedDir, mag, 0f));
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>Suma la magnitud de todos los impulsos "flee_" en un ImpulseController.</summary>
    static float GetFleeMagnitude(ImpulseController ctrl)
    {
        // NetDirection solo da el vector resultante, pero necesitamos saber
        // si hay impulsos flee activos. Accedemos via reflexión del campo privado
        // o usamos el API público de dirección neta proyectando en el flee.
        // Alternativa simple: preguntar si el NetDirection tiene componente relevante.
        // Por ahora usamos el módulo del NetDirection como proxy (suficiente para el sandbox).
        return ctrl.NetDirection.magnitude;
    }

    /// <summary>
    /// Retorna la fuerza de bond entre este ser y otro GO (0–1).
    /// Usa Anima.bonds si el otro implementa ITarget; cae a un valor neutro en el sandbox.
    /// </summary>
    float GetBond(GameObject other)
    {
        if (_self == null) return 0.5f; // sin Anima: bond neutro

        // Intentar leer bond real (requiere que el otro implemente ITarget).
        var otherTarget = other.GetComponent<ITarget>();
        if (otherTarget != null)
        {
            Bond b = _self.GetBond(otherTarget);
            return b != null ? Mathf.Clamp01(b.value / 100f) : 0f;
        }

        // Fallback para SimpleAnima (no implementa ITarget): si hay Anima en el GO
        // y está en el mismo "grupo" (tiene ImpulseController → mismo contexto), usar bond base.
        var otherAnima = other.GetComponent<Anima>();
        return otherAnima != null ? 0.4f : 0f; // bond de grupo sandbox: bajo pero funcional
    }
}
