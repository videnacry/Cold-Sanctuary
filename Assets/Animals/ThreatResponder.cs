using UnityEngine;

/// <summary>Reacción ante una amenaza (docs/behavior-system.md): huir, plantar cara, o pegar-y-correr.</summary>
public enum Reaction { Flee, Fight, HitAndRun }

/// <summary>
/// Componente de RESPUESTA A AMENAZAS (docs/anima-dissolving-animal.md, etapa 1). Extrae de `Animal` la
/// **política de decisión** luchar/huir/pegar-y-correr, que es **portable** porque sale de STATS
/// (`Predation.EffectivePower` = fuerza/masa/textura/agilidad + manada por facción), del `autoabandono` (valentía)
/// y de los **bonds** (no dañar a quien quieres; defender crías). Cualquier `Anima` puede llevarlo — no depende de
/// la maquinaria de `Animal`.
///
/// Incluye la EVALUACIÓN (`Assess`, ya stat-based) y la DECISIÓN (`Decide`). La DETECCIÓN (`SenseThreats`) y las
/// ACCIONES (coroutines `Flee`/`Fight`/`HitAndRun`, que usan NavMesh/animación) siguen en `Animal`; se migrarán con
/// `Locomotion`. El contexto de CRÍAS (Family/`Group`) lo calcula `Animal` y se pasa aquí (`defendingCubs`/`cubBond`),
/// porque el sistema de familia aún vive en `Animal`; la defensa de crías es EMERGENTE (bond + autoabandono), sin flag.
/// </summary>
public class ThreatResponder : MonoBehaviour
{
    [Tooltip("Margen de poder para atacar en vez de huir: se pelea si myPower > enemyPower × este factor (y hay agresividad).")]
    public float fightPowerMargin = 1.5f;
    [Tooltip("Agresividad mínima para atacar cuando se es claramente más fuerte.")]
    public float aggressionGate = 0.5f;
    [Tooltip("Cuánto suma al miedo un aura mágica DESTRUCTIVA del origen (por unidad de aura). Tunable.")]
    public float auraFear = 1f;
    [Tooltip("Convierte el 'miedo' (fracción de mi poder) en un ALCANCE en metros para la alerta de proximidad: " +
             "más peligroso → reacciono de más lejos; más cerca → reacción plena en vez de solo nervios. Tunable.")]
    public float alertReach = 10f;

    /// <summary>EVALÚA cuán amenazante es `source` para `self`, por STATS: poder depredador EFECTIVO del origen
    /// (con manada) relativo al mío. 1 = parejo; &gt;1 me supera; acotado [0.2, 4]. Un aura destructiva asusta más;
    /// un BOND lo desactiva (bond 100 → amenaza 0). El resultado se compara con `ThreatThreshold` (fracción de mi
    /// poder a partir de la cual me alarmo). Portable: no usa `rig.mass`/NavMesh.</summary>
    public float Assess(Anima self, GameObject source)
    {
        if (self == null || source == null) return 0f;
        Anima src = source.GetComponent<Anima>();
        if (src == null) return 0f;

        float threat = Mathf.Clamp(Predation.EffectivePower(src) / Mathf.Max(0.1f, Predation.EffectivePower(self)), 0.2f, 4f);
        if (src.magicAura < 0f) threat += -src.magicAura * auraFear;   // aura destructiva → más temido
        ITarget srcT = source.GetComponent<ITarget>();
        if (srcT != null) { Bond b = self.GetBond(srcT); if (b != null) threat *= Mathf.Clamp01(1f - b.value / 100f); }
        return threat;
    }

    /// <summary>Decide la reacción por STATS + autoabandono + bonds. `self` = quien reacciona; el resto es contexto
    /// que aporta el llamante (crías/agresividad/pegar-y-correr son aún específicos de la especie).</summary>
    public Reaction Decide(Anima self, GameObject threat, float autoabandono,
                           bool defendingCubs, float cubBond, float aggressiveness, bool canHitAndRun)
    {
        if (self == null || threat == null) return Reaction.Flee;

        // No dañar a un ser con el que hay vínculo suficiente (bond) → huir.
        ITarget tt = threat.GetComponent<ITarget>();
        if (tt != null && !self.CanHarm(tt)) return Reaction.Flee;

        Anima threatAnima = threat.GetComponent<Anima>();
        float myPower    = Predation.EffectivePower(self) * (1f + autoabandono);   // stats + manada + valentía
        float enemyPower = threatAnima != null ? Predation.EffectivePower(threatAnima) : 0f;

        if (myPower > enemyPower * fightPowerMargin && aggressiveness > aggressionGate)
            return Reaction.Fight;   // claramente más fuerte

        // Modelo bonds+threat+autoabandono: por las crías, plantar cara si (autoabandono + vínculo) > peligro.
        if (defendingCubs)
        {
            float peligro = Mathf.Max(0f, enemyPower / Mathf.Max(0.1f, myPower) - 1f);   // 0 = parejo, >0 = en desventaja
            if ((autoabandono + cubBond) > peligro)
                return canHitAndRun ? Reaction.HitAndRun : Reaction.Fight;
        }
        return Reaction.Flee;
    }
}
