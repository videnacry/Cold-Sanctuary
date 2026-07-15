using UnityEngine;

/// <summary>
/// Irosene (70+ años) — pasión, motivación, sociabilidad.
/// Primary channel: Satisfaction — su energía y pasión llenan la moral del jugador.
///
/// Madre de 9, superviviente de dos hogares violentos que levantó un negocio de dulces de la
/// nada. En el santuario tuvo una transformación física y mental enorme: de casi no poder caminar
/// (obesidad + secuelas de cáncer) a correr, escalar y bucear profundo. Junto a Lizmibet pasó mucho
/// más tiempo del normal en el Nivel 1 y aprendió hechizos avanzados para acelerar el cambio; hoy
/// reside en el nivel submarino (Tier 4) pero visita a Panterilia y tuvo grandes momentos en el
/// huerto. Ver docs/character-irosene.md y docs/creature-stats.md.
///
/// Personalidad: carácter fuerte, algo altiva/soberbia, pero también un peluche cariñoso. Muy
/// expresiva (cara, manos, brazos). Salta de la alegría a la ira, a la melancolía y a la sabiduría.
/// Da mensajes motivacionales con muchísimo sentimiento ("yo soy el alfa y el omega", "persigue
/// tus sueños").
/// </summary>
public class Irosene : CompanionBase
{
    [Header("Irosene — Dialogue")]
    [Tooltip("Líneas al entrar por primera vez en su rango.")]
    public DialogueSequence greetingSequence;

    [Tooltip("Arengas motivacionales ('yo soy el alfa y el omega', 'persigue tus sueños').")]
    public DialogueSequence motivationalSequence;

    [Tooltip("Momentos melancólicos ('soy rebelde porque la vida me hizo así'), recuerdos de niñez.")]
    public DialogueSequence melancholicSequence;

    [Header("Irosene — Motivación")]
    [Tooltip("Golpe de satisfacción inmediato (0–1) cuando el jugador entra en su rango: su energía levanta el ánimo.")]
    [Range(0f, 1f)] public float encouragementBurst = 0.05f;

    [Tooltip("Bonus de satisfacción por segundo que aporta en misiones de compañeros (leído por el sistema de misiones cuando exista).")]
    public float companionMissionSatisfactionBonus = 0.01f;

    // Aptitudes (1.0 = media real humana). Codifican ORIGEN + PRESENTE transformado.
    // Origen: obesidad y poca movilidad, secuelas de cáncer → fuerza/agilidad/resistencia muy bajas.
    // Presente (santuario, con magia): corre, escala y bucea profundo → subieron con fuerza.
    protected override float BaseAgility      => 1.2f;   // origen bajo; destreza de envolver dulces + transformación → ahora trepa
    protected override float BasePerception   => 1.3f;   // autodidacta (remedios naturales), lee a la gente, detecta injusticias
    protected override float BaseStrength     => 1.2f;   // de casi no caminar a nadar profundo/escalar
    protected override float BaseBodyMass     => 1.3f;   // sobrepeso; masa alta aunque ya funcional
    protected override float BaseAdaptability => 1.6f;   // dos hogares violentos superados + negocio desde cero + grandes transformaciones
    protected override float BaseComposure    => 1.4f;   // carácter fuerte; se enfrenta a la injusticia, firme y orgullosa
    protected override float BaseEndurance    => 1.3f;   // ahora esfuerzo sostenido (correr/escalar/bucear); antes casi nula
    protected override float BaseReasoning    => 1.1f;   // sabiduría práctica/autodidacta (recetas, remedios, negocio), no académica
    protected override float BaseMemory       => 1.3f;   // narra su vida con gran detalle; recetas
    protected override float BaseCreativity   => 1.5f;   // recetas de dulces, karaoke, ingenio para sobrevivir
    protected override float BaseSociability  => 1.7f;   // hiperexpresiva, motivadora, querida por nietos y desconocidos
    protected override float BaseDiscipline   => 1.1f;   // montó negocio y puso a los hijos a estudiar+trabajar; tragaperras/deudas lo moderan

    protected override void SetupAnchors()
    {
        anchors.Add(new ThoughtAnchor("alpha_and_omega", 0.95f, 0.0f));    // autoafirmación — fijo
        anchors.Add(new ThoughtAnchor("pursue_dreams",   0.9f,  0.0f));    // motor motivacional — fijo
        anchors.Add(new ThoughtAnchor("love_your_people",0.85f, 0.005f));  // llenar de amor a los suyos — crece
        anchors.Add(new ThoughtAnchor("rebellion",       0.8f,  0.003f));  // "rebelde porque la vida me hizo así" — se suaviza al sanar
        anchors.Add(new ThoughtAnchor("pride",           0.6f,  0.004f));  // altivez/soberbia — se ablanda con el vínculo
    }

    protected override float GetMoodModifier()
    {
        // Irosene se entrega: incluso cansada da mucho, y con buen ánimo desborda (puede superar 1).
        // El estrés agudo sí la modera.
        return Mathf.Lerp(0.5f, 1.2f, mood) * Mathf.Lerp(1f, 0.5f, stress);
    }

    protected override float FatigueRatePerSecond() => 0.0001f; // transformada y con una energía enorme — se cansa poco

    protected override float GetRestingMood()
    {
        // Vive de la pasión: ánimo alto de base, aún más con vínculo.
        return Mathf.Lerp(0.6f, 0.9f, bondWithPlayer / 100f);
    }

    protected override void Update()
    {
        base.Update();

        // Arco: al sanar en el santuario, la rebeldía de origen se suaviza y el orgullo baja
        // hacia su lado "peluche"; el amor por los suyos crece.
        ShiftAnchor("rebellion",        0.4f, Time.deltaTime);
        ShiftAnchor("pride",            0.3f, Time.deltaTime);
        ShiftAnchor("love_your_people", 1f,   Time.deltaTime);
    }

    protected override void OnPlayerNearby()
    {
        // Su presencia levanta el ánimo de inmediato.
        if (_playerMind != null && encouragementBurst > 0f)
            _playerMind.RestoreMind(encouragementBurst, MindChannel.Satisfaction);

        if (DialogueManager.Instance == null || DialogueManager.Instance.IsPlaying) return;

        // Con buen ánimo areng; con el ánimo bajo se pone melancólica; si no, saluda.
        if (mood >= 0.75f && motivationalSequence != null)
            DialogueManager.Instance.Play(motivationalSequence);
        else if (mood <= 0.45f && melancholicSequence != null)
            DialogueManager.Instance.Play(melancholicSequence);
        else if (greetingSequence != null)
            DialogueManager.Instance.Play(greetingSequence);
    }
}
