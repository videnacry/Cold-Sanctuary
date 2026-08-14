using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PILAR BOND por COMPOSICIÓN (docs/anima-architecture.md · soul-composition-blend.md — fase 5). Implementa
/// <see cref="IBondable"/> como **componente**, para que **cualquier `Anima` sea "compañero"** sin heredar de
/// `CompanionBase`. Extrae el vínculo con el jugador + efecto de proximidad que hoy vive dentro de `CompanionBase`
/// → primer paso para **disolver `CompanionBase`**. Coexiste con `WorldBondable` (que ya usa `IBondable` por
/// composición). Opt-in; no toca a los compañeros actuales.
///   compañero = `Anima` + `SoulComposition` (stats por arquetipo/blend) + `BondPillar` (+ `Mind` opcional).
/// </summary>
public class BondPillar : MonoBehaviour, IBondable
{
    public Anima anima;

    [Range(0f, 100f)] public float bondWithPlayer = 20f;
    [Range(0f, 1f)] public float mood = 0.7f;
    [Range(0f, 1f)] public float fatigue = 0f;
    public float proximityRadius = 4f;
    public float baseRestorationRate = 0.02f;
    public MindChannel primaryChannel = MindChannel.MentalFatigue;
    [Tooltip("Multiplicador de personalidad sobre la restauración (lo que en CompanionBase era GetMoodModifier).")]
    public float moodModifier = 1f;

    readonly Dictionary<MonoBehaviour, float> _otherBonds = new Dictionary<MonoBehaviour, float>();
    IMind _playerMind;
    Transform _playerTransform;
    MonoBehaviour _playerEntity;

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerMind = player.GetComponent<IMind>();
            _playerTransform = player.transform;
            _playerEntity = player.GetComponent<PlayerStats>();
        }
    }

    // ── IBondable ──────────────────────────────────────────────────────────────
    public float GetBondStrength(MonoBehaviour source)
    {
        if (source != null && source == _playerEntity) return bondWithPlayer;
        _otherBonds.TryGetValue(source, out float v);
        return v;
    }

    public void GrowBond(MonoBehaviour source, float amount)
    {
        if (source == null) return;
        if (source == _playerEntity) { bondWithPlayer = Mathf.Clamp(bondWithPlayer + amount, 0f, 100f); return; }
        _otherBonds.TryGetValue(source, out float cur);
        _otherBonds[source] = Mathf.Clamp(cur + amount, 0f, 100f);
    }

    public float GetProximityEffect(MonoBehaviour source, MindChannel channel)
    {
        if (channel != primaryChannel) return 0f;
        float bondFactor = GetBondStrength(source) / 100f;
        float stress = anima != null ? anima.stress : 0f;
        float stateFactor = Mathf.Clamp(mood - fatigue * 0.5f - stress * 0.3f, -0.5f, 1f);
        return baseRestorationRate * bondFactor * stateFactor * moodModifier;
    }

    // ── Runtime: aplica el efecto de proximidad al jugador (como CompanionBase.CheckPlayerProximity) ──
    void Update()
    {
        if (_playerMind == null || _playerTransform == null || _playerEntity == null) return;
        if (Vector3.Distance(transform.position, _playerTransform.position) > proximityRadius) return;
        float effect = GetProximityEffect(_playerEntity, primaryChannel);
        if (Mathf.Approximately(effect, 0f)) return;
        if (effect > 0f) _playerMind.RestoreMind(effect * Time.deltaTime, primaryChannel);
        else _playerMind.DrainMind(-effect * Time.deltaTime, primaryChannel);
    }
}
