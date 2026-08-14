using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estado interno de un compañero (mood/fatiga/anchors) + vínculo con el jugador + efecto de proximidad —
/// **extraído de `CompanionBase`** (que se retira, fase 5). Un compañero = `SimpleAnima` + `SoulComposition`
/// (stats por arquetipo) + `Mind` + **`MoodState`** + su componente de comportamiento propio. Implementa
/// `IBondable` por composición. **Parametrizable** por compañero mediante curvas (mood/resting/fatiga), que antes
/// eran overrides de clase. La restauración al jugador y el drift de ánimo se hacen aquí.
/// </summary>
public class MoodState : MonoBehaviour, IBondable
{
    public Anima anima;

    [Range(0f, 100f)] public float bondWithPlayer = 20f;
    [Range(0f, 1f)] public float mood = 0.7f;
    [Range(0f, 1f)] public float fatigue = 0f;
    public float proximityRadius = 4f;
    public float baseRestorationRate = 0.02f;
    public MindChannel primaryChannel = MindChannel.MentalFatigue;

    [Header("Curvas por compañero (antes eran overrides de CompanionBase)")]
    public float fatigueRate = 0.0001f;
    public float restingMoodMin = 0.3f, restingMoodMax = 0.7f;   // resting = lerp(min,max, bond/100)
    public float moodModMin = 0.3f, moodModMax = 1f;             // mod    = lerp(min,max, mood)
    [Range(0f, 1f)] public float stressPenalty = 0f;            // mod *= lerp(1, 1−penalty, stress)

    public List<ThoughtAnchor> anchors = new List<ThoughtAnchor>();

    readonly Dictionary<MonoBehaviour, float> _otherBonds = new Dictionary<MonoBehaviour, float>();
    IMind _playerMind; Transform _playerTransform; MonoBehaviour _playerEntity; bool _inRange;

    /// <summary>El jugador está en rango (para los comportamientos propios).</summary>
    public bool PlayerInRange => _inRange;
    public IMind PlayerMind => _playerMind;
    public Transform PlayerTransform => _playerTransform;
    /// <summary>Se disparan una vez al entrar/salir el jugador del rango.</summary>
    public event System.Action OnPlayerEnter, OnPlayerLeave;

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { _playerMind = p.GetComponent<IMind>(); _playerTransform = p.transform; _playerEntity = p.GetComponent<PlayerStats>(); }
    }

    void Update()
    {
        fatigue = Mathf.Clamp01(fatigue + fatigueRate * Time.deltaTime);
        mood = Mathf.MoveTowards(mood, RestingMood(), 0.001f * Time.deltaTime);

        if (_playerMind == null || _playerTransform == null) return;
        bool inRange = Vector3.Distance(transform.position, _playerTransform.position) <= proximityRadius;
        if (inRange && !_inRange) { _inRange = true; OnPlayerEnter?.Invoke(); }
        else if (!inRange && _inRange) { _inRange = false; OnPlayerLeave?.Invoke(); }
        if (_inRange && _playerEntity != null)
        {
            float e = GetProximityEffect(_playerEntity, primaryChannel);
            if (!Mathf.Approximately(e, 0f))
            {
                if (e > 0f) _playerMind.RestoreMind(e * Time.deltaTime, primaryChannel);
                else _playerMind.DrainMind(-e * Time.deltaTime, primaryChannel);
            }
        }
    }

    public float RestingMood() => Mathf.Lerp(restingMoodMin, restingMoodMax, bondWithPlayer / 100f);

    public float GetMoodModifier()
    {
        float stress = anima != null ? anima.stress : 0f;
        float m = Mathf.Lerp(moodModMin, moodModMax, mood);
        return stressPenalty > 0f ? m * Mathf.Lerp(1f, 1f - stressPenalty, stress) : m;
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
        return baseRestorationRate * bondFactor * stateFactor * GetMoodModifier();
    }

    // ── Anchors ────────────────────────────────────────────────────────────────
    public float GetAnchor(string key)
    {
        foreach (ThoughtAnchor a in anchors) if (a.key == key) return a.weight;
        return 0f;
    }

    public void ShiftAnchor(string key, float targetWeight, float deltaTime)
    {
        foreach (ThoughtAnchor a in anchors) if (a.key == key) { a.ShiftToward(targetWeight, deltaTime); return; }
    }
}
