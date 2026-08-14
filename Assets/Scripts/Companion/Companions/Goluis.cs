using UnityEngine;

/// <summary>
/// El Maestro Goluis — cocina, doble turno, presión y resistencia. Comportamiento propio (sobre `MoodState`);
/// stats desde el arquetipo "Goluis" (`SoulComposition`). Anchor clave: "yoga_skepticism". Ver docs/creature-stats.md.
/// </summary>
public class Goluis : MonoBehaviour
{
    [Header("Goluis — Dialogue")]
    public DialogueSequence greetingSequence;
    public DialogueSequence pressureSequence;

    [Header("Goluis — Pressure System")]
    [Tooltip("Sube estrés del jugador pero también su resistencia mental.")]
    public bool pressureActive;
    public float pressureStressRate = 0.005f;
    [Range(0f, 1f)] public float resistanceBuilt;

    MoodState _mood;

    void Awake() { _mood = GetComponent<MoodState>(); }
    void OnEnable()  { if (_mood != null) _mood.OnPlayerEnter += OnNearby; }
    void OnDisable() { if (_mood != null) _mood.OnPlayerEnter -= OnNearby; }

    void Update()
    {
        if (_mood == null) return;

        if (pressureActive && _mood.PlayerInRange && _mood.PlayerMind != null)
        {
            _mood.PlayerMind.DrainMind(pressureStressRate * Time.deltaTime, MindChannel.Stress);
            resistanceBuilt = Mathf.Clamp01(resistanceBuilt + 0.00001f * Time.deltaTime);
        }

        if (_mood.bondWithPlayer > 60f)
            _mood.ShiftAnchor("yoga_skepticism", -0.2f, Time.deltaTime);   // el arco lo abre poco a poco
    }

    void OnNearby()
    {
        if (DialogueManager.Instance == null || DialogueManager.Instance.IsPlaying) return;
        if (pressureActive && pressureSequence != null) DialogueManager.Instance.Play(pressureSequence);
        else if (greetingSequence != null) DialogueManager.Instance.Play(greetingSequence);
    }
}
