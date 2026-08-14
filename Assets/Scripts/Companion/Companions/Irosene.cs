using UnityEngine;

/// <summary>
/// Irosene (70+) — pasión, motivación, sociabilidad. Comportamiento propio (sobre `MoodState`); stats desde el
/// arquetipo "Irosene". Canal primario = Satisfacción (se configura en `MoodState`). Ver docs/character-irosene.md.
/// </summary>
public class Irosene : MonoBehaviour
{
    [Header("Irosene — Dialogue")]
    public DialogueSequence greetingSequence;
    public DialogueSequence motivationalSequence;
    public DialogueSequence melancholicSequence;

    [Header("Irosene — Motivación")]
    [Range(0f, 1f)] public float encouragementBurst = 0.05f;
    [Tooltip("Bonus de satisfacción/seg en misiones de compañeros (leído por el sistema de misiones cuando exista).")]
    public float companionMissionSatisfactionBonus = 0.01f;

    MoodState _mood;

    void Awake() { _mood = GetComponent<MoodState>(); }
    void OnEnable()  { if (_mood != null) _mood.OnPlayerEnter += OnNearby; }
    void OnDisable() { if (_mood != null) _mood.OnPlayerEnter -= OnNearby; }

    void Update()
    {
        if (_mood == null) return;
        // Arco: la rebeldía y el orgullo se suavizan al sanar; el amor por los suyos crece.
        _mood.ShiftAnchor("rebellion", 0.4f, Time.deltaTime);
        _mood.ShiftAnchor("pride", 0.3f, Time.deltaTime);
        _mood.ShiftAnchor("love_your_people", 1f, Time.deltaTime);
    }

    void OnNearby()
    {
        if (_mood.PlayerMind != null && encouragementBurst > 0f)
            _mood.PlayerMind.RestoreMind(encouragementBurst, MindChannel.Satisfaction);

        if (DialogueManager.Instance == null || DialogueManager.Instance.IsPlaying) return;
        if (_mood.mood >= 0.75f && motivationalSequence != null) DialogueManager.Instance.Play(motivationalSequence);
        else if (_mood.mood <= 0.45f && melancholicSequence != null) DialogueManager.Instance.Play(melancholicSequence);
        else if (greetingSequence != null) DialogueManager.Instance.Play(greetingSequence);
    }
}
