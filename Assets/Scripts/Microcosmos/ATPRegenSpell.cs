using UnityEngine;

/// <summary>
/// REGENERACIÓN PASIVA DE ENERGÍA (ATP) — hechizo de duración ilimitada siempre activo.
///
/// Convierte <see cref="Anima.fatReserves"/> (reservas metabólicas) en
/// <see cref="CharacterLevel.currentEnergy"/> (barra de ATP) a ritmo constante.
///
/// Comportamiento:
/// • Si la barra de energía está llena, el hechizo está idle (sin coste).
/// • Cuando la barra baja, el regen se activa automáticamente (solo "visible"
///   para el jugador en ese momento: la barra empieza a subir).
/// • <see cref="WeaknessEffect.blockReserveRegen"/> == true detiene el regen:
///   el debilitamiento congela las reservas metabólicas, el ser no puede
///   recargar energía por sí solo (necesita ayuda externa: miel, jalón, etc.)
/// • Si fatReserves baja de <see cref="minFatThreshold"/>, no hay combustible
///   para generar ATP: el regen se pausa igualmente.
///
/// Diseño docs/stats-as-truth.md: fatReserves → elemento → energía (ATP).
/// </summary>
[RequireComponent(typeof(CharacterLevel))]
public class ATPRegenSpell : MonoBehaviour
{
    [Header("Tasa de regeneración")]
    [Tooltip("Energía (currentEnergy) restaurada por segundo cuando la barra no está llena.")]
    [Min(0.01f)] public float regenPerSecond = 4f;

    [Tooltip("fatReserves consumidos por cada unidad de energía generada (0 = sin coste metabólico).")]
    [Min(0f)] public float fatCostPerEnergy = 0.02f;

    [Tooltip("fatReserves mínimas para poder generar energía. Por debajo: el regen se pausa.")]
    [Min(0f)] public float minFatThreshold = 0.05f;

    // ── Depuración ────────────────────────────────────────────────────────

    /// <summary>Solo lectura — true si el regen está activo este frame.</summary>
    [field: SerializeField, HideInInspector]
    public bool IsRegenerating { get; private set; }

    // ── Privado ───────────────────────────────────────────────────────────

    CharacterLevel _level;
    Anima          _anima;
    WeaknessEffect _weakness;

    void Awake()
    {
        _level   = GetComponent<CharacterLevel>();
        _anima   = GetComponent<Anima>();
        _weakness = GetComponent<WeaknessEffect>();
    }

    void Update()
    {
        IsRegenerating = false;

        if (_level == null) return;

        // Barra llena: idle.
        if (_level.currentEnergy >= _level.MaxEnergy) return;

        // WeaknessEffect bloquea el acceso a las reservas metabólicas.
        if (_weakness != null && _weakness.blockReserveRegen) return;

        // Sin reservas metabólicas suficientes: sin combustible.
        if (_anima != null && _anima.fatReserves < minFatThreshold) return;

        // Generar energía.
        float gain = Mathf.Min(regenPerSecond * Time.deltaTime,
                               _level.MaxEnergy - _level.currentEnergy);

        _level.currentEnergy += gain;
        IsRegenerating = true;

        // Consumir reservas metabólicas (hambre diferida).
        if (_anima != null && fatCostPerEnergy > 0f)
            _anima.fatReserves = Mathf.Max(0f, _anima.fatReserves - gain * fatCostPerEnergy);
    }
}
