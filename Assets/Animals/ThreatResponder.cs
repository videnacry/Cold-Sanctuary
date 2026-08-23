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
    [Tooltip("Agresividad de ESTE ser (config por especie): por encima de aggressionGate, ataca si es más fuerte. " +
             "Dirección: dejará de ser un escalar y pasará a HISTÓRICO (semilla innata + confianza-por-uso) — " +
             "rebanada D de capabilities-and-embodiment.md.")]
    public float aggressiveness = 0f;
    [Tooltip("Cuánto suma al miedo un aura mágica DESTRUCTIVA del origen (por unidad de aura). Tunable.")]
    public float auraFear = 1f;
    [Tooltip("Convierte el 'miedo' (fracción de mi poder) en un ALCANCE en metros para la alerta de proximidad: " +
             "más peligroso → reacciono de más lejos; más cerca → reacción plena en vez de solo nervios. Tunable.")]
    public float alertReach = 10f;
    [Tooltip("Percepción a partir de la cual LEO la amenaza con claridad plena; por debajo, la amenaza percibida " +
             "escala hacia 'no detecto nada'. Con la percepción base de la fauna (≥1) la lectura es plena → sin " +
             "cambio de balance; solo un ser de percepción baja (garrapata/ciego) ataca 'a ciegas'. Tunable.")]
    public float perceptionForFullRead = 1f;
    [Tooltip("Amenaza percibida cuando NO puedo leer al otro (0 = no me alarmo por lo que no percibo).")]
    public float unawareThreat = 0f;
    [Tooltip("Cuánto suma a la agresividad EFECTIVA la confianza en el combate (0–100 → ×este/100). Con confianza 0 " +
             "(recién nacido) no cambia nada; el que caza/pelea con éxito se vuelve más osado — temperamento HISTÓRICO.")]
    public float confidenceAggressionWeight = 1f;

    /// <summary>EVALÚA cuán amenazante es `source` para `self`, por STATS: poder depredador EFECTIVO del origen
    /// (con manada) relativo al mío. 1 = parejo; &gt;1 me supera; acotado [0.2, 4]. Un aura destructiva asusta más;
    /// un BOND lo desactiva (bond 100 → amenaza 0). El resultado se compara con `ThreatThreshold` (fracción de mi
    /// poder a partir de la cual me alarmo). Portable: no usa `rig.mass`/NavMesh.
    ///
    /// GATEADO POR SENTIDOS (rebanada C, capabilities-and-embodiment.md §3): solo percibo la amenaza tan bien como la
    /// LEO — mi percepción × la legibilidad del otro. Poca claridad → tiende a `unawareThreat` ("no detecto nada"), así
    /// una garrapata/ser ciego ataca a un gigante sin alarmarse; darle ojos (subir percepción) recalibra su respuesta.
    /// El BOND se aplica DESPUÉS del gateo: es MEMORIA, no percepción del momento (a un amigo lo reconozco aunque no lo lea bien).</summary>
    public float Assess(Anima self, GameObject source)
    {
        if (self == null || source == null) return 0f;
        Anima src = source.GetComponent<Anima>();
        if (src == null) return 0f;

        float threat = Mathf.Clamp(Predation.EffectivePower(src) / Mathf.Max(0.1f, Predation.EffectivePower(self)), 0.2f, 4f);
        if (src.magicAura < 0f) threat += -src.magicAura * auraFear;   // aura destructiva → más temido

        // Claridad de la lectura = mi percepción (hasta plena en perceptionForFullRead) × legibilidad del otro.
        float legibility = 1f;   // HOOK: a futuro baja con tamaño diminuto / quietud / camuflaje (insecto difícil de leer)
        float clarity = Mathf.Clamp01(self.Perception / Mathf.Max(0.01f, perceptionForFullRead)) * legibility;
        threat = Mathf.Lerp(unawareThreat, threat, clarity);   // poca claridad → no percibo el peligro

        ITarget srcT = source.GetComponent<ITarget>();
        if (srcT != null) { Bond b = self.GetBond(srcT); if (b != null) threat *= Mathf.Clamp01(1f - b.value / 100f); }
        return threat;
    }

    /// <summary>Decide la reacción por STATS + autoabandono + bonds. `self` = quien reacciona; el contexto de CRÍAS
    /// lo aporta el llamante. El **acoso** (pegar-y-correr) ya NO es un flag de especie: EMERGE del margen de poder al
    /// defender crías (no puedo con ella → acoso en vez de standup). La **agresividad EFECTIVA** = `aggressiveness`
    /// innata + la **confianza en el combate** (histórico de uso): el que hiere con éxito se vuelve más osado (D2,
    /// capabilities-and-embodiment.md §4). Con confianza 0 (recién nacido) = comportamiento de antes.</summary>
    public Reaction Decide(Anima self, GameObject threat, float autoabandono, bool defendingCubs, float cubBond)
    {
        if (self == null || threat == null) return Reaction.Flee;

        // No dañar a un ser con el que hay vínculo suficiente (bond) → huir.
        ITarget tt = threat.GetComponent<ITarget>();
        if (tt != null && !self.CanHarm(tt)) return Reaction.Flee;

        Anima threatAnima = threat.GetComponent<Anima>();
        float myPower    = Predation.EffectivePower(self) * (1f + autoabandono);   // stats + manada + valentía
        float enemyPower = threatAnima != null ? Predation.EffectivePower(threatAnima) : 0f;

        // Agresividad EFECTIVA = innata + confianza acumulada en el combate (0 al nacer → sin cambio; sube con el uso exitoso).
        float effectiveAggression = aggressiveness + self.Confidence(Capability.Combat) / 100f * confidenceAggressionWeight;
        if (myPower > enemyPower * fightPowerMargin && effectiveAggression > aggressionGate)
            return Reaction.Fight;   // claramente más fuerte

        // Modelo bonds+threat+autoabandono: por las crías, plantar cara si (autoabandono + vínculo) > peligro.
        if (defendingCubs)
        {
            float peligro = Mathf.Max(0f, enemyPower / Mathf.Max(0.1f, myPower) - 1f);   // 0 = parejo, >0 = en desventaja
            if ((autoabandono + cubBond) > peligro)
                // Fight vs ACOSO emerge del margen de poder (ya no un flag de especie): me comprometo a una pelea plena
                // solo si soy claramente más fuerte; si no puedo con ella, acoso (golpe + retirada hacia el nido,
                // repetido) — la coneja que muerde la cola de la serpiente y se retira. Ver capabilities-and-embodiment.md §4.
                return myPower > enemyPower * fightPowerMargin ? Reaction.Fight : Reaction.HitAndRun;
        }
        return Reaction.Flee;
    }
}
