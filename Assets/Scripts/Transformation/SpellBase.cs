using UnityEngine;

/// <summary>
/// Cómo responde un hechizo a MANTENER pulsada su tecla (docs/stats-as-truth.md §hechizos). No es una sola
/// conducta pegada a la base: cada hechizo elige su modo.
///   • <b>Instant</b> — un disparo por pulsación (mantener no hace nada).
///   • <b>Repeat</b>  — RELANZA cada `repeatCooldown` mientras se mantiene (p.ej. fireball: sale una tras otra).
///   • <b>Channel</b> — SOSTIENE/acumula el efecto mientras se mantiene (p.ej. Jalar/Caminar: el forcejeo sube).
///   • <b>Charge</b>  — acumula mientras mantienes y SUELTA al soltar; más carga = efecto mayor (p.ej. la
///                      transformación: mantener alarga la duración/potencia).
/// </summary>
public enum CastMode { Instant, Repeat, Channel, Charge }

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
    float _chargeTime;

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
            case CastMode.Charge:
                if (Input.GetKeyDown(spellKey)) _chargeTime = 0f;
                if (Input.GetKey(spellKey)) _chargeTime += Time.deltaTime;
                if (Input.GetKeyUp(spellKey)) { OnChargeRelease(_chargeTime); _chargeTime = 0f; }
                break;
        }
    }

    // Hooks que la subclase sobreescribe según su modo (por defecto no hacen nada).
    protected virtual void OnCastPressed() { }              // Instant / Repeat
    protected virtual void OnChannelStart() { }             // Channel: al empezar a mantener
    protected virtual void OnChannelTick(float dt) { }      // Channel: cada frame mientras se mantiene
    protected virtual void OnChannelEnd() { }               // Channel: al soltar
    protected virtual void OnChargeRelease(float chargeTime) { }  // Charge: al soltar, con el tiempo acumulado

    // ── Forcejeo / Channeling: bonos de poder compartidos por TODO hechizo ──────
    // Dos bonos ortogonales que se suman al `force`:
    //   • FORCEJEO — sube SOLO cuando el hechizo no logra su efecto (ReportResult(false)); persiste; escala con
    //     aptitudes FÍSICAS. "Mantener la tecla sola".
    //   • CHANNELING — sube mientras se canaliza (tecla + channelKey) y DECAE al soltar; escala con aptitudes
    //     MENTALES. Se recupera volviendo a canalizar.
    [Header("Forcejeo / Channeling (bonos de poder)")]
    [Tooltip("Modificador de CANALIZAR: mantener esta tecla junto a la del hechizo = channeling (bonus mental).")]
    public KeyCode channelKey = KeyCode.LeftShift;
    [Tooltip("Cuánto sube el bonus de FORCEJEO por cada intento sin lograr el efecto.")]
    [Min(0f)] public float forcejeoStep = 0.05f;
    [Min(0f)] public float forcejeoMaxBonus = 3f;
    [Tooltip("Cuánto sube el bonus de CHANNELING por segundo mientras se canaliza.")]
    [Min(0f)] public float channelRampPerSecond = 1f;
    [Min(0f)] public float channelMaxBonus = 3f;
    [Tooltip("Cuánto DECAE el channeling por segundo al soltar (el forcejeo NO decae).")]
    [Min(0f)] public float channelDecayPerSecond = 0.5f;

    float _forcejeo;   // bonus físico (persiste; sube al fallar)
    float _channel;    // bonus mental (sube canalizando, decae si no)

    public float RawForcejeo => _forcejeo;
    public float RawChannel  => _channel;

    static float PhysFactor(Anima c) => c == null ? 1f : Mathf.Max(0.1f, (c.strength + c.endurance + c.bodyMass) / 3f);
    static float MindFactor(Anima c) => c == null ? 1f : Mathf.Max(0.1f, (c.reasoning + c.memory + c.creativity) / 3f);

    /// <summary>Bonus de forcejeo escalado por aptitudes FÍSICAS.</summary>
    protected float ForcejeoBonus(Anima c) => _forcejeo * PhysFactor(c);
    /// <summary>Bonus de channeling escalado por aptitudes MENTALES.</summary>
    protected float ChannelBonus(Anima c) => _channel * MindFactor(c);
    /// <summary>Bonus total sobre el `force` base (forcejeo físico + channeling mental).</summary>
    protected float BonusPower(Anima c) => ForcejeoBonus(c) + ChannelBonus(c);

    /// <summary>Sube el channeling si se canaliza; si no, decae. Llamar cada frame desde la subclase.</summary>
    protected void TickChanneling(bool channeling, float dt)
    {
        if (channeling) _channel = Mathf.Min(channelMaxBonus, _channel + channelRampPerSecond * dt);
        else            _channel = Mathf.Max(0f, _channel - channelDecayPerSecond * dt);
    }

    /// <summary>Reporta un intento: si NO logró el efecto, sube el forcejeo (persiste). El éxito NO lo baja.</summary>
    protected void ReportResult(bool success)
    {
        if (!success) _forcejeo = Mathf.Min(forcejeoMaxBonus, _forcejeo + forcejeoStep);
    }
    protected void ResetForcejeo() => _forcejeo = 0f;

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
