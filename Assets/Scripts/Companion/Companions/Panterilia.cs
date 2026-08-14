using UnityEngine;

/// <summary>
/// Panterilia — limpieza, nutrición, observación. Comportamiento propio (sobre `MoodState`); stats desde el
/// arquetipo "Panterilia". Sube el radio de Observación del jugador por cercanía. Ver docs/creature-stats.md.
/// </summary>
public class Panterilia : MonoBehaviour
{
    [Header("Panterilia — Observation Bonus")]
    [Tooltip("Bonus de radio de observación mientras Panterilia está cerca.")]
    public float observationRadiusBonus = 1.5f;

    bool _bonusApplied;
    PlayerStats _playerStats;
    MoodState _mood;

    void Awake()
    {
        _mood = GetComponent<MoodState>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerStats = player.GetComponent<PlayerStats>();
    }
    void OnEnable()  { if (_mood != null) { _mood.OnPlayerEnter += OnNearby; _mood.OnPlayerLeave += OnLeft; } }
    void OnDisable() { if (_mood != null) { _mood.OnPlayerEnter -= OnNearby; _mood.OnPlayerLeave -= OnLeft; } }

    void Update()
    {
        if (_mood == null) return;
        // Arco: chemical_reliance se desvanece, trust_nature crece durante su estancia.
        _mood.ShiftAnchor("chemical_reliance", 0f, Time.deltaTime);
        _mood.ShiftAnchor("trust_nature", 1f, Time.deltaTime);
    }

    void OnNearby()
    {
        if (_playerStats != null && !_bonusApplied) { _playerStats.observationRadius += observationRadiusBonus; _bonusApplied = true; }
    }

    void OnLeft()
    {
        if (_playerStats != null && _bonusApplied) { _playerStats.observationRadius -= observationRadiusBonus; _bonusApplied = false; }
    }
}
