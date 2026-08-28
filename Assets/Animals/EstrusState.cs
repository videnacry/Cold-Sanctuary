using UnityEngine;

/// <summary>
/// CELO (estro) como **hechizo-estado** (docs/environmental-navigation.md §4.2; reproducción paso 1). Un ADULTO entra
/// en celo por un ciclo rítmico; mientras dura, **deposita el canal `Estrus`** en la `TraceField` (rastro que atrae
/// parejas) y expone <see cref="InEstrus"/> para los deseos/cortejo (paso 2). Auto-añadido en `Animal.Init`.
///
/// Balance-safe: solo emite un rastro barato (O(1)) y pone un flag; **nada consume aún `InEstrus`/`Estrus` para
/// reproducirse** (eso llega con cortejo/gestación), así que la conducta no cambia todavía. Los individuos se
/// **desincronizan** (fase inicial aleatoria) para que no entren en celo todos a la vez.
/// </summary>
public class EstrusState : MonoBehaviour
{
    [Tooltip("Período del ciclo completo (segundos × velocidad de juego): celo + reposo.")]
    [Min(1f)] public float cyclePeriod = 120f;
    [Tooltip("Fracción del ciclo en CELO (0.15 ≈ celo corto).")]
    [Range(0f, 1f)] public float estrusFraction = 0.15f;
    [Tooltip("Intensidad del rastro de Estrus depositado mientras dura el celo.")]
    [Min(0f)] public float scentStrength = 8f;
    [Tooltip("Segundos entre depósitos (rate-limit; el rastro es continuo pero barato).")]
    [Min(0.1f)] public float depositInterval = 1f;

    Animal _animal;
    float _phase;
    float _nextDeposit;

    /// <summary>¿Está en celo AHORA? Lo leen el deseo "mate" y el cortejo (paso 2).</summary>
    public bool InEstrus { get; private set; }

    void Awake()
    {
        _animal = GetComponent<Animal>();
        _phase = Random.Range(0f, cyclePeriod);   // desincronizar entre individuos
    }

    void Update()
    {
        // Solo los ADULTOS entran en celo.
        if (_animal == null || _animal.lifeStage != LifeStage.adult) { InEstrus = false; return; }

        int speed = TimeController.timeController != null ? Mathf.Max(1, TimeController.timeController.TimeSpeed) : 1;
        _phase += Time.deltaTime * speed;
        if (_phase >= cyclePeriod) _phase -= cyclePeriod;
        InEstrus = _phase < cyclePeriod * estrusFraction;

        if (InEstrus && Time.time >= _nextDeposit)
        {
            _nextDeposit = Time.time + depositInterval;
            Emit();
        }
    }

    /// <summary>Deposita el rastro de Estrus en la rejilla (no-op si no hay `TraceField`). Público para el test.</summary>
    public void Emit() => TraceField.Leave(transform.position, TraceChannel.Estrus, scentStrength);
}
