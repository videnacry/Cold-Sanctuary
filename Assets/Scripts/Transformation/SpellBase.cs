using UnityEngine;

/// <summary>
/// Cómo responde un hechizo a MANTENER pulsada su tecla (docs/stats-as-truth.md §hechizos). No es una sola
/// conducta pegada a la base: cada hechizo elige su modo.
///   • <b>Instant</b> — un disparo por pulsación (mantener no hace nada).
///   • <b>Repeat</b>  — RELANZA cada `repeatCooldown` mientras se mantiene (p.ej. fireball: sale una tras otra).
///   • <b>Channel</b> — SOSTIENE/aplica el efecto mientras se mantiene (p.ej. Jalar: el forcejeo actúa).
/// El CHARGE ya NO es un modo: es un bonus ORTOGONAL (chargeKey/LeftShift) del sistema de `powerBonus` (abajo),
/// disponible para cualquier hechizo sin importar su `castMode`.
/// </summary>
public enum CastMode { Instant, Repeat, Channel }

/// <summary>
/// Base abstracta para todos los hechizos del juego (docs/stats-as-truth.md §hechizos).
///
/// Un hechizo tiene tres parámetros universales:
///   • <see cref="range"/>    — alcance máximo (0 = tacto; el caster debe estar junto al target).
///   • <see cref="force"/>    — magnitud del efecto (tiro, carga, potencia…).
///   • <see cref="duration"/> — duración del efecto (0 = instantáneo).
///
/// Subclases implementan <see cref="CanCast"/> y <see cref="Cast"/>.
/// El target es un <see cref="ITarget"/> — cualquier Anima, FoodItem, FishSchool u objeto del mundo
/// que implemente la interfaz. Esto permite lanzar sobre uno mismo, sobre otro ser vivo o
/// sobre elementos del entorno (la transformación al mundo entero, p. ej.).
///
/// Para hechizos de MagicReserves (coste elemental/energía) usar <see cref="MagicReserves.Pay"/>
/// en la implementación de <see cref="Cast"/> antes de ejecutar el efecto.
/// </summary>
public abstract class SpellBase : MonoBehaviour
{
    [Header("Parámetros universales del hechizo")]
    [Tooltip("Alcance máximo (m). 0 = tacto (el caster debe estar pegado al target).")]
    [Min(0f)] public float range = 0f;

    [Tooltip("Magnitud del efecto: fuerza de tiro, potencia de curación, intensidad de transformación…")]
    [Min(0f)] public float force = 1f;

    [Tooltip("Duración del efecto en segundos. 0 = instantáneo.")]
    [Min(0f)] public float duration = 0f;

    [Tooltip("Coste de energía (barra de ATP/CharacterLevel) por cada uso. 0 = gratis.")]
    [Min(0f)] public float energyCost = 0f;

    [Header("Modo de lanzamiento (mantener tecla)")]
    [Tooltip("Cómo responde a mantener la tecla: Instant / Repeat / Channel / Charge.")]
    public CastMode castMode = CastMode.Instant;
    [Tooltip("Tecla para lanzar/mantener con input directo. None = el hechizo no usa input propio (lo dispara la IA/otro).")]
    public KeyCode spellKey = KeyCode.None;
    [Tooltip("Repeat: segundos entre relanzamientos mientras se mantiene.")]
    [Min(0.05f)] public float repeatCooldown = 0.5f;

    float _repeatTimer;

    // ── Manejo de input por modo (OPT-IN: la subclase lo llama desde su Update) ─
    /// <summary>Dispatch de input según `castMode` sobre `spellKey`. Llamar desde el `Update` de la subclase.
    /// Invoca los hooks `OnCast*` correspondientes. Si `spellKey` es None, no hace nada.</summary>
    protected void PollInput()
    {
        if (spellKey == KeyCode.None) return;
        switch (castMode)
        {
            case CastMode.Instant:
                if (Input.GetKeyDown(spellKey)) OnCastPressed();
                break;
            case CastMode.Repeat:
                if (Input.GetKey(spellKey))
                {
                    _repeatTimer -= Time.deltaTime;
                    if (_repeatTimer <= 0f) { OnCastPressed(); _repeatTimer = repeatCooldown; }
                }
                else _repeatTimer = 0f;
                break;
            case CastMode.Channel:
                if (Input.GetKeyDown(spellKey)) OnChannelStart();
                if (Input.GetKey(spellKey)) OnChannelTick(Time.deltaTime);
                if (Input.GetKeyUp(spellKey)) OnChannelEnd();
                break;
        }
    }

    // Hooks que la subclase sobreescribe según su modo (por defecto no hacen nada).
    protected virtual void OnCastPressed() { }              // Instant / Repeat
    protected virtual void OnChannelStart() { }             // Channel: al empezar a mantener
    protected virtual void OnChannelTick(float dt) { }      // Channel: cada frame mientras se mantiene
    protected virtual void OnChannelEnd() { }               // Channel: al soltar

    // ── Bonus de poder UNIFICADO: charge + channeling + forcejeo → un solo `powerBonus` que DECAE ─────
    // (docs/stats-as-truth §hechizos). Charge y channeling se pulsan JUNTO a la acción del hechizo:
    //   • CHARGE (chargeKey = LeftShift): mantener acumula SIN aplicar el efecto (el ser "toma postura"); al
    //     SOLTAR inyecta el acumulado de golpe (burst) y dispara → OnChargeReleased. Tope maxPowerWithCharge.
    //   • CHANNELING (channelKey = RightShift): SUELO/tope dinámico en maxPowerWithChanneling — si el bonus está
    //     por debajo, sube gradual hasta él; si la carga lo dejó por encima, impide que decaiga por debajo de él.
    //   • FORCEJEO — sube al FALLAR el efecto (ReportResult(false)); ahora vive en el MISMO powerBonus.
    // Sin sostener nada, el powerBonus DECAE a 0 (vuelta al hechizo base). Las aptitudes escalan los TOPES:
    // físicas el de charge/forcejeo, mentales el de channeling (a futuro, override de stats por hechizo).
    [Header("Bonus de poder (charge + channeling + forcejeo, unificado)")]
    [Tooltip("Tecla de CARGA: mantener acumula sin aplicar; soltar inyecta el burst y dispara.")]
    public KeyCode chargeKey = KeyCode.LeftShift;
    [Tooltip("Tecla de CANALIZAR: suelo/tope dinámico (sube hasta su max o impide que decaiga por debajo).")]
    public KeyCode channelKey = KeyCode.RightShift;
    [Tooltip("Cuánto DECAE el powerBonus por segundo cuando no se sostiene (vuelta a la base).")]
    [Min(0f)] public float decayPerSecond = 0.5f;
    [Tooltip("Cuánto acumula la CARGA por segundo mientras mantienes chargeKey.")]
    [Min(0f)] public float chargeRampPerSecond = 1f;
    [Tooltip("Tope base del bonus por CARGA (× aptitudes físicas).")]
    [Min(0f)] public float maxPowerWithCharge = 3f;
    [Tooltip("Cuánto sube el CANALIZAR por segundo hacia su tope.")]
    [Min(0f)] public float channelRampPerSecond = 1f;
    [Tooltip("Tope base del bonus por CANALIZAR (× aptitudes mentales). También es el SUELO mientras se canaliza.")]
    [Min(0f)] public float maxPowerWithChanneling = 3f;
    [Tooltip("Cuánto sube el FORCEJEO por cada intento fallido.")]
    [Min(0f)] public float forcejeoStep = 0.05f;
    [Tooltip("Tope base del bonus por FORCEJEO (× aptitudes físicas).")]
    [Min(0f)] public float maxPowerWithForcejeo = 3f;

    float _powerBonus;    // el bonus unificado (se suma a `force`/velocidad; decae con el tiempo)
    float _chargeAccum;   // acumulado durante la carga; se inyecta a _powerBonus al soltar chargeKey
    bool  _charging;

    public float PowerBonus  => _powerBonus;
    public float ChargeAccum => _chargeAccum;
    public bool  IsCharging  => _charging;

    static float PhysFactor(Anima c) => c == null ? 1f : Mathf.Max(0.1f, (c.strength + c.endurance + c.bodyMass) / 3f);
    static float MindFactor(Anima c) => c == null ? 1f : Mathf.Max(0.1f, (c.reasoning + c.memory + c.creativity) / 3f);

    /// <summary>Actualiza el powerBonus (charge/channeling/decay) según chargeKey/channelKey. Llamar cada frame
    /// desde la subclase con su caster. El FORCEJEO se sube aparte con <see cref="ReportResult"/> al fallar.</summary>
    protected void TickPowerBonus(Anima c, float dt)
    {
        float maxCharge = maxPowerWithCharge     * PhysFactor(c);
        float maxChan   = maxPowerWithChanneling * MindFactor(c);

        bool charging   = chargeKey  != KeyCode.None && Input.GetKey(chargeKey);
        bool channeling = channelKey != KeyCode.None && Input.GetKey(channelKey);

        // CHARGE: acumula mientras se mantiene; al soltar, inyecta de golpe y dispara el burst.
        if (charging)
            _chargeAccum = Mathf.Min(maxCharge, _chargeAccum + chargeRampPerSecond * dt);
        else if (_charging)   // soltó la carga este frame
        {
            _powerBonus = Mathf.Max(_powerBonus, _chargeAccum);
            float injected = _chargeAccum;
            _chargeAccum = 0f;
            OnChargeReleased(injected);
        }
        _charging = charging;

        // CHANNELING: sube hacia su tope si está por debajo (parte de lo que dejó la carga).
        if (channeling && _powerBonus < maxChan)
            _powerBonus = Mathf.Min(maxChan, _powerBonus + channelRampPerSecond * dt);

        // DECAY: hacia el suelo (maxChan si se canaliza; 0 si no). Nunca por debajo del suelo.
        float floor = channeling ? maxChan : 0f;
        if (_powerBonus > floor)
            _powerBonus = Mathf.Max(floor, _powerBonus - decayPerSecond * dt);
    }

    /// <summary>Reporta un intento: si NO logró el efecto, sube el FORCEJEO dentro del mismo powerBonus.</summary>
    protected void ReportResult(Anima c, bool success)
    {
        if (success) return;
        float maxForc = maxPowerWithForcejeo * PhysFactor(c);
        if (_powerBonus < maxForc)
            _powerBonus = Mathf.Min(maxForc, _powerBonus + forcejeoStep);
    }

    /// <summary>Descarta el bonus acumulado (vuelta inmediata a la base).</summary>
    protected void ResetPowerBonus() { _powerBonus = 0f; _chargeAccum = 0f; }

    /// <summary>Hook: se llama al SOLTAR la carga, con el bonus inyectado. La subclase dispara aquí su burst
    /// (arrancar con la velocidad acumulada / lanzar la esfera gigante).</summary>
    protected virtual void OnChargeReleased(float injected) { }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// ¿Puede el <paramref name="caster"/> lanzar este hechizo sobre <paramref name="target"/>?
    /// Las subclases deben comprobar al menos que el target no está muerto y que está en rango.
    /// </summary>
    public abstract bool CanCast(Anima caster, ITarget target);

    /// <summary>
    /// Lanza el hechizo. Solo llamar si <see cref="CanCast"/> devuelve true.
    /// </summary>
    public abstract void Cast(Anima caster, ITarget target);

    // ── Utilidades para subclases ─────────────────────────────────────────────

    /// <summary>¿Está <paramref name="target"/> dentro del alcance del hechizo?</summary>
    protected bool InRange(Anima caster, ITarget target)
    {
        if (range <= 0f) return true; // tacto: siempre en rango (validar distancia física en CanCast)
        return Vector3.Distance(caster.transform.position, target.transform.position) <= range;
    }

    /// <summary>
    /// ¿Tiene <paramref name="caster"/> suficiente energía para lanzar?
    /// Si <see cref="energyCost"/> == 0, siempre true.
    /// </summary>
    protected bool HasEnergy(Anima caster)
    {
        if (energyCost <= 0f) return true;
        var cl = caster.GetComponent<CharacterLevel>();
        return cl == null || cl.currentEnergy >= energyCost;
    }

    /// <summary>
    /// Paga el coste de energía. Devuelve false si no hay suficiente (sin pagar).
    /// Llamar en <see cref="Cast"/> justo antes de ejecutar el efecto.
    /// </summary>
    protected bool PayEnergy(Anima caster)
    {
        if (energyCost <= 0f) return true;
        var cl = caster.GetComponent<CharacterLevel>();
        return cl == null || cl.SpendEnergy(energyCost);
    }

    /// <summary>
    /// Fuerza efectiva del hechizo contra <paramref name="target"/>: descuenta la masa del objetivo
    /// y la fuerza que ejerce si se resiste (si <paramref name="targetResistForce"/> > 0).
    /// Retorna 0 si no hay fuerza suficiente para moverlo.
    /// </summary>
    protected float EffectiveForce(Anima caster, ITarget target, float targetResistForce = 0f)
    {
        float casterForce = force + caster.Strength;          // aptitud fuerza del lanzador
        float targetWeight = target.Mass + targetResistForce; // peso + resistencia activa
        return Mathf.Max(0f, casterForce - targetWeight);
    }
}
