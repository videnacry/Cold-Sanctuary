using UnityEngine;

/// <summary>
/// **Conductor** de la orquesta emocional de un `Anima` (docs/emotion-model.md). Lee los `Humores` (valencia =
/// `Positividad`, activación = `Energia` del **circumplex**), los sesga por la **disposición** (aptitudes de
/// `Anima` + `afabilidad`/`sensibilidad`) y **publica una señal** (Valence/Arousal/Tension + **Jolt** = la
/// variación *violenta*) que leen los <see cref="BodyPartReactor"/> (las partes que reaccionan). Es
/// **universal**: la misma señal la realiza cada especie con SUS partes (orejas↔brazos↔antenas↔alas) — la
/// traducción entre respuestas humana/animal/insecto. Null-safe: sin `Mind` usa un `Humores` local; sin
/// reactores, solo computa la señal (p. ej. para la cámara).
/// </summary>
public class EmotionExpression : MonoBehaviour
{
    [Tooltip("Fuente de humores. Si es null, se busca un Mind; si no hay, usa un Humores local.")]
    public Mind mind;

    [Header("Debug (sandbox): oscila los humores para ver la orquesta sin jugador")]
    public bool debugDrive = false;
    public float debugSpeed = 0.6f;

    // ── Señal emocional publicada (la leen los BodyPartReactor) ──
    public float Valence { get; private set; }   // 0..1 (agradable)
    public float Arousal { get; private set; }   // 0..1 (activado)
    public float Tension { get; private set; }   // 0..1 (cortisol/adrenalina)
    public float Jolt    { get; private set; }   // 0..1 pico transitorio por cambio VIOLENTO

    Anima _anima;
    Humores _local;
    float _prevA, _prevT;

    void Start()
    {
        if (mind == null) { mind = GetComponent<Mind>(); if (mind == null) mind = GetComponentInChildren<Mind>(); }
        _anima = GetComponent<Anima>();
    }

    Humores H()
    {
        if (mind != null && mind.humores != null) return mind.humores;
        if (_local == null) _local = new Humores();
        return _local;
    }

    void Update()
    {
        Humores h = H();

        if (debugDrive)   // sandbox: ondas desfasadas de serotonina/cortisol/adrenalina
        {
            h.serotonina = 0.5f + 0.4f * Mathf.Sin(Time.time * debugSpeed);
            h.cortisol   = 0.4f + 0.4f * Mathf.Sin(Time.time * debugSpeed * 1.7f + 1f);
            h.adrenalina = 0.4f + 0.4f * Mathf.Sin(Time.time * debugSpeed * 2.3f + 2f);
        }

        float valence = Mathf.Clamp01((h.Positividad + 1f) * 0.5f);   // -1..1 → 0..1
        float arousal = Mathf.Clamp01(h.Energia);
        float tension = Mathf.Clamp01(Mathf.Max(h.cortisol, h.adrenalina));

        // Disposición: compostura amortigua el swing (hacia neutro); afabilidad sube la valencia.
        float comp = _anima != null ? _anima.composure : 1f;
        float afab = _anima != null ? _anima.afabilidad : 1f;
        float sens = _anima != null ? _anima.sensibilidad : 1f;
        float damp = Mathf.Clamp01(1f - (comp - 1f) * 0.3f);
        valence = Mathf.Clamp01(Mathf.Lerp(0.5f, valence, damp) + (afab - 1f) * 0.1f);
        arousal = Mathf.Lerp(0.5f, arousal, damp);

        // Jolt: variación VIOLENTA de activación/tensión, amplificada por la sensibilidad emocional.
        float delta = Mathf.Abs(arousal - _prevA) + Mathf.Abs(tension - _prevT);
        Jolt = Mathf.Clamp01(delta * sens * 12f);
        _prevA = arousal; _prevT = tension;

        Valence = valence; Arousal = arousal; Tension = tension;
    }
}
