using UnityEngine;

/// <summary>
/// Una "parte" de la **orquesta** emocional (docs/emotion-model.md §orquesta / traducción). Reacciona a la
/// señal del <see cref="EmotionExpression"/> moviendo su hueso: **PASIVA** (activación/valencia → offset
/// sostenido: erguir/hundir, abrir/cerrar) y **VIOLENTA** (`Jolt` → tic rápido que decae: quijada, uñas,
/// antena, oreja…). Es la **traducción entre especies**: la misma señal mueve orejas, brazos, antenas o alas
/// según qué parte sea y sus **ganancias** (una oreja de gato y un brazo humano con "cerrarse ante el miedo"
/// alto reaccionan igual). Resuelve el hueso vía <see cref="CreatureRig"/> (o un `boneOverride` para el
/// sandbox). Un ser = muchos de éstos, cada uno su rol.
/// </summary>
public class BodyPartReactor : MonoBehaviour
{
    public EmotionExpression conductor;   // auto: en el padre
    public CreatureRig rig;               // auto: en el padre (resuelve el hueso)
    public BodyPart part = BodyPart.Head;
    [Tooltip("Si no hay rig, mueve este Transform (sandbox con cubos). Si null, se mueve a sí mismo.")]
    public Transform boneOverride;

    [Header("Ganancias — el 'rol' de esta parte (traducción entre especies)")]
    public Vector3 axis = Vector3.right;
    [Tooltip("Grados: activación → erguir/perk (negativo = se hunde con la activación).")]
    public float arousalGain = 20f;
    [Tooltip("Grados: valencia BAJA → cerrar/hundir.")]
    public float valenceGain = 8f;
    [Tooltip("Amplitud del tic ante cambios VIOLENTOS de stats.")]
    public float joltGain = 25f;
    [Tooltip("Qué rápido se calma el tic.")]
    public float twitchDecay = 6f;
    [Tooltip("Grados extra de hundimiento con el 'peso' Laban (valencia/energía bajas).")]
    public float heavinessDroop = 10f;
    [Tooltip("Amplitud del temblor tenso con el 'flujo ligado' Laban (tensión alta).")]
    public float boundJitter = 2f;

    Transform _t;
    Quaternion _home;
    float _twitch, _current;

    void Start()
    {
        if (conductor == null) conductor = GetComponentInParent<EmotionExpression>();
        if (rig == null) rig = GetComponentInParent<CreatureRig>();
        _t = boneOverride != null ? boneOverride : (rig != null ? rig.Get(part) : null);
        if (_t == null) _t = transform;
        _home = _t.localRotation;
    }

    void Update()
    {
        if (conductor == null) return;
        // PASIVO: la activación erige/perk; la valencia baja cierra/hunde; el PESO (Laban) hunde más.
        float passive = (conductor.Arousal - 0.5f) * arousalGain
                        - (0.5f - conductor.Valence) * valenceGain
                        - conductor.Heaviness * heavinessDroop;
        // VIOLENTO: el Jolt inyecta un tic que luego decae.
        _twitch += conductor.Jolt * joltGain * Time.deltaTime * 8f;
        _twitch = Mathf.Lerp(_twitch, 0f, Time.deltaTime * twitchDecay);
        // FLOW ligado (Laban): temblor tenso proporcional a la tensión.
        float jitter = conductor.Boundness * boundJitter * Mathf.Sin(Time.time * 30f);
        float target = passive + _twitch + jitter;

        Vector3 a = axis.sqrMagnitude > 0.0001f ? axis.normalized : Vector3.right;
        // TIME (Laban): la activación alta reacciona rápido; la baja, sostenida/lenta.
        float speed = 2f + conductor.Quickness * 10f;
        _current = Mathf.Lerp(_current, target, Time.deltaTime * speed);
        _t.localRotation = _home * Quaternion.AngleAxis(_current, a);
    }
}
