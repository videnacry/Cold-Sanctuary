using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// CONCIENCIA DE MANADA — lógica de ayuda emergente entre compañeros.
///
/// Cada ser con este componente escanea compañeros cercanos (con ThreatScanner)
/// y decide si ir a ayudar basándose en tres variables:
///
///   autoabandono  = disposición al auto-sacrificio del AYUDANTE (Anima.autoabandono)
///   vínculo       = bond entre ayudante y compañero (Anima.BondWith o fallback 0.4)
///   peligroEspec  = peligro específico del COMPAÑERO = compañero.PerceivedDanger - selfPower
///
/// CONDICIÓN DE AYUDA:
///   (autoabandono + vínculo) > peligroEspec
///
///   Lectura: "mi disposición a sacrificarme + mi vínculo supera el peligro neto
///            que corre mi compañero dado mi propio poder". Si yo soy más fuerte
///            (selfPower alto), el peligro neto del compañero se reduce para mí.
///
/// RESULTADO:
///   • Si la condición se cumple: añado un impulso hacia el compañero en más peligro
///     (magnitud = autoabandono + vínculo − peligroEspec).
///   • El impulso compite con el flee y el home impulse naturalmente.
///   • Se activa "modo misión": autoabandono sube temporalmente (momentum altruista).
///
/// PAUSA ANTES DE HUIR:
///   El escaneo corre al mismo rate que ThreatScanner. Si el compañero más necesitado
///   supera la condición, el impulso de ayuda neutraliza parcialmente el flee,
///   produciendo el "pause before fleeing" emergente.
///
/// PELIGRO AMBIENTAL (ThreatEmitter):
///   El suelo de selva con ThreatEmitter llena PerceivedDanger de todas las hormigas,
///   incluso las que no ven depredadores. Las viejas tienen menos selfPower → mayor
///   peligroEspec → más difícil ayudarlas (requiere más autoabandono + vínculo).
/// </summary>
[RequireComponent(typeof(ImpulseController))]
public class PackAwareness : MonoBehaviour
{
    [Header("Escaneo de compañeros")]
    [Tooltip("Radio en el que se buscan compañeros con ThreatScanner. " +
             "~6-10 m = distancia de visión realista en escala insecto.")]
    [Min(0.5f)] public float packRadius = 8f;

    [Tooltip("Frecuencia de evaluación de compañeros (s).")]
    [Min(0.1f)] public float scanRate = 0.5f;

    [Header("Modo mision")]
    [Tooltip("Bonus de autoabandono durante el modo mision (se suma temporalmente).")]
    [Min(0f)] public float missionAbandonoBonus = 0.25f;

    [Tooltip("Duracion del bonus de autoabandono en modo mision (s).")]
    [Min(0f)] public float missionDuration = 8f;

    [Tooltip("Magnitud maxima del impulso de ayuda.")]
    [Min(0f)] public float maxHelpMagnitude = 5f;

    // ── Estado ─────────────────────────────────────────────────────────────

    ImpulseController _ctrl;
    Anima             _self;
    float             _selfPower;
    float             _next;
    float             _missionEndTime = -1f;

    const string TAG = "pack_help";

    // ── Ciclo ──────────────────────────────────────────────────────────────

    void Awake()
    {
        _ctrl = GetComponent<ImpulseController>();
        _self = GetComponent<Anima>();
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + scanRate;

        _ctrl.RemoveByTag(TAG);

        _selfPower = Predation.PredatorPower(_self);

        // Calcular autoabandono efectivo (base + bonus de mision si activo).
        float effectiveAbandono = _self != null ? _self.autoabandono : 0.3f;
        if (_missionEndTime > Time.time)
            effectiveAbandono = Mathf.Min(1f, effectiveAbandono + missionAbandonoBonus);

        // Escanear compañeros en radio.
        float bestScore    = 0f; // (autoabandono+vinculo) - peligroEspec → positivo = ayudar
        Vector3 helpDir    = Vector3.zero;
        bool    foundNeed  = false;

        var cols = Physics.OverlapSphere(transform.position, packRadius);
        foreach (var col in cols)
        {
            if (col.gameObject == this.gameObject) continue;

            // Solo compañeros con ThreatScanner (tienen PerceivedDanger calculado).
            var compScanner = col.GetComponent<ThreatScanner>();
            if (compScanner == null) continue;

            // Solo si el compañero realmente tiene peligro.
            float compDanger = compScanner.PerceivedDanger;
            if (compDanger <= 0f) continue;

            // Peligro específico: cuánto peligro NETO supera mi poder.
            float peligroEspec = compDanger - _selfPower;

            // Vínculo con este compañero.
            float bond = GetBond(col.gameObject);

            // Condición de ayuda: autoabandono + vínculo > peligro específico.
            float score = effectiveAbandono + bond - peligroEspec;
            if (score <= 0f) continue; // no me atrevo a ayudar a este compañero

            // Elegir al compañero con el score más alto (más necesitado y más alcanzable).
            if (score > bestScore)
            {
                bestScore = score;
                helpDir   = col.transform.position - transform.position;
                foundNeed = true;
            }
        }

        if (!foundNeed) return;

        // Activar modo misión si no está activo.
        if (_missionEndTime <= Time.time)
        {
            _missionEndTime = Time.time + missionDuration;
            if (_self != null)
                _self.stress = Mathf.Min(1f, _self.stress + 0.1f); // la decisión de ayudar cuesta algo
        }

        float mag = Mathf.Clamp(bestScore, 0f, maxHelpMagnitude);
        _ctrl.AddImpulse(new MovementImpulse(TAG, helpDir, mag, 0f));
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>Bond 0–1 entre este ser y un GO compañero.</summary>
    float GetBond(GameObject other)
    {
        if (_self == null) return 0.4f;

        var otherTarget = other.GetComponent<ITarget>();
        if (otherTarget != null)
        {
            Bond b = _self.GetBond(otherTarget);
            return b != null ? Mathf.Clamp01(b.value / 100f) : 0f;
        }

        // Fallback sandbox (SimpleAnima sin ITarget): bond de grupo base.
        return other.GetComponent<Anima>() != null ? 0.4f : 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, packRadius);

        // Mostrar si modo mision esta activo.
        if (_missionEndTime > Time.time)
        {
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
