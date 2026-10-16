using UnityEngine;

/// <summary>
/// ANDAR del cuerpo de cubos: mientras el ser se desplaza, balancea muslos, rodillas y brazos en oposición (pierna
/// izquierda con brazo derecho), con la amplitud y el ritmo de su velocidad. Parado, no toca nada (las asanas y la
/// emoción mandan sobre las articulaciones). Sin esto los cuerpos de cubos se deslizaban rígidos.
/// </summary>
public class CubeWalkCycle : MonoBehaviour
{
    [Tooltip("Grados de balanceo del muslo a velocidad de paseo (~1,5 m/s).")]
    public float legSwing = 28f;
    public float armSwing = 22f;
    [Tooltip("Zancadas por metro recorrido.")]
    public float stepsPerMeter = 0.9f;

    CreatureRig _rig;
    Vector3 _last;
    float _phase, _blend;

    void Awake() { _rig = GetComponent<CreatureRig>(); _last = transform.position; }

    void LateUpdate()
    {
        if (_rig == null || Time.deltaTime <= 0f) return;
        Vector3 d = transform.position - _last; d.y = 0f;
        _last = transform.position;
        float speed = d.magnitude / Time.deltaTime;
        float target = speed > 0.2f ? Mathf.Clamp01(speed / 1.5f) : 0f;
        bool wasWalking = _blend > 0.01f;
        _blend = Mathf.MoveTowards(_blend, target, 3f * Time.deltaTime);
        if (_blend <= 0.01f && !wasWalking) return;   // parado: las articulaciones son de las asanas/emoción

        _phase += d.magnitude * stepsPerMeter * Mathf.PI * 2f;
        float s = Mathf.Sin(_phase) * _blend;
        float knee = Mathf.Max(0f, -Mathf.Cos(_phase)) * _blend;
        Set(BodyPart.ThighLeft,  -legSwing * s);
        Set(BodyPart.ThighRight,  legSwing * s);
        Set(BodyPart.ShinLeft,  legSwing * 1.4f * Mathf.Max(0f,  Mathf.Sin(_phase - 1f)) * _blend);
        Set(BodyPart.ShinRight, legSwing * 1.4f * Mathf.Max(0f, -Mathf.Sin(_phase - 1f)) * _blend);
        Set(BodyPart.ShoulderLeft,   armSwing * s);
        Set(BodyPart.ShoulderRight, -armSwing * s);
        Set(BodyPart.ForearmLeft,  -15f * _blend - 10f * knee);
        Set(BodyPart.ForearmRight, -15f * _blend - 10f * knee);
    }

    void Set(BodyPart p, float x)
    {
        if (_rig.IsPoseLocked(p)) return;   // el yoga manda sobre este hueso: no lo balanceamos al andar
        Transform t = _rig.Get(p);
        if (t != null) t.localRotation = Quaternion.Euler(x, 0f, 0f);
    }
}
