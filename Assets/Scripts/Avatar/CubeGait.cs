using UnityEngine;

/// <summary>
/// MARCHA de la fauna de cubos (<see cref="CubeCreature"/>): mueve los HUESOS según cómo se desplaza el animal.
///   · Cuadrúpedo: paso diagonal (mano izquierda con pie derecho), rodilla/codo que se doblan, cabeza que cabecea,
///     cola que se mece (más deprisa si está contento, quieta y baja si tiene miedo).
///   · Cetáceo y foca: ONDULACIÓN de la columna (cabeza → cola → aleta caudal) en vertical, también en reposo, más
///     amplia al nadar rápido.
///   · Ave (pingüino): bamboleo de cadera, pasitos cortos y aletas.
///   · Insecto: trípode alterno (patas 1-4-5 contra 2-3-6) y antenas.
/// Se aplica SOBRE la postura de reposo de cada hueso y respeta los huesos que el yoga tiene bloqueados.
/// </summary>
public class CubeGait : MonoBehaviour
{
    public CubeCreature.Plan plan;
    [Tooltip("Velocidad (m/s) a la que el ciclo va a amplitud completa.")]
    public float fullSpeed = 1.5f;

    CreatureRig _rig;
    Anima _a;
    UnityEngine.AI.NavMeshAgent _agent;
    Vector3 _last;
    float _phase, _speed;
    readonly System.Collections.Generic.Dictionary<BodyPart, CubeJoint> _j = new System.Collections.Generic.Dictionary<BodyPart, CubeJoint>();

    void Start()
    {
        _rig = GetComponent<CreatureRig>();
        _a = GetComponent<Anima>();
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _last = transform.position;
        foreach (CubeJoint j in GetComponentsInChildren<CubeJoint>())
            if (!_j.ContainsKey(j.part) && j.name == j.part.ToString()) _j[j.part] = j;
        // A escala real: un oso da pasos más largos que un zorro (la velocidad de ciclo se ajusta al tamaño).
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        if (rs.Length > 0) { Bounds b = rs[0].bounds; foreach (Renderer r in rs) b.Encapsulate(r.bounds); fullSpeed = Mathf.Max(0.3f, b.size.magnitude * 0.9f); }
    }

    void LateUpdate()
    {
        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        Vector3 v = _agent != null && _agent.enabled ? _agent.velocity : (transform.position - _last) / dt;
        _last = transform.position;
        v.y = 0f;
        _speed = Mathf.Lerp(_speed, v.magnitude, 1f - Mathf.Exp(-6f * dt));
        float amp = Mathf.Clamp01(_speed / fullSpeed);
        bool asleep = _a != null && (_a.asleep || _a.death);
        _phase += dt * Mathf.Lerp(1.2f, 7f, amp) * (asleep ? 0.2f : 1f);

        switch (plan)
        {
            case CubeCreature.Plan.Cetacean:
            case CubeCreature.Plan.Pinniped:
                float wave = Mathf.Lerp(4f, 14f, amp) * (asleep ? 0.3f : 1f);
                Rot(BodyPart.Head, wave * 0.25f * Mathf.Sin(_phase + 1.2f), 0f, 0f);
                Rot(BodyPart.Back, wave * 0.4f * Mathf.Sin(_phase - 0.6f), 0f, 0f);
                Rot(BodyPart.Tail, wave * 0.7f * Mathf.Sin(_phase - 1.2f), 0f, 0f);
                Rot(BodyPart.Tail2, wave * Mathf.Sin(_phase - 1.8f), 0f, 0f);
                Rot(BodyPart.Fluke, wave * 1.2f * Mathf.Sin(_phase - 2.4f), 0f, 0f);
                Rot(BodyPart.ShoulderLeft, 0f, 0f, 8f * Mathf.Sin(_phase * 0.5f));
                Rot(BodyPart.ShoulderRight, 0f, 0f, -8f * Mathf.Sin(_phase * 0.5f));
                Rot(BodyPart.ThighLeft, 0f, 10f * Mathf.Sin(_phase), 0f);
                Rot(BodyPart.ThighRight, 0f, -10f * Mathf.Sin(_phase), 0f);
                break;

            case CubeCreature.Plan.Bird:
                float w = 28f * amp;
                Rot(BodyPart.Hips, 0f, 0f, 9f * amp * Mathf.Sin(_phase));   // bamboleo
                Rot(BodyPart.ThighLeft, w * Mathf.Sin(_phase), 0f, 0f);
                Rot(BodyPart.ThighRight, -w * Mathf.Sin(_phase), 0f, 0f);
                Rot(BodyPart.ShoulderLeft, 0f, 0f, -12f * amp * Mathf.Abs(Mathf.Sin(_phase)));
                Rot(BodyPart.ShoulderRight, 0f, 0f, 12f * amp * Mathf.Abs(Mathf.Sin(_phase)));
                Rot(BodyPart.Head, 4f * Mathf.Sin(_phase * 2f) * amp, 0f, 0f);
                break;

            case CubeCreature.Plan.Insect:
                float a = 22f * amp;
                BodyPart[] legs = { BodyPart.LimbA, BodyPart.LimbB, BodyPart.LimbC, BodyPart.LimbD, BodyPart.LimbE, BodyPart.LimbF };
                for (int i = 0; i < 6; i++)
                {
                    bool groupA = i == 0 || i == 3 || i == 4;
                    Rot(legs[i], 0f, (groupA ? 1f : -1f) * a * Mathf.Sin(_phase * 1.5f), 0f);
                }
                Rot(BodyPart.AntennaLeft, 8f * Mathf.Sin(_phase * 0.7f), 0f, 0f);
                Rot(BodyPart.AntennaRight, 8f * Mathf.Sin(_phase * 0.7f + 1f), 0f, 0f);
                Rot(BodyPart.ScapulaLeft, 0f, 0f, 35f * Mathf.Sin(_phase * 12f));    // alas (si las tiene)
                Rot(BodyPart.ScapulaRight, 0f, 0f, -35f * Mathf.Sin(_phase * 12f));
                break;

            default:   // cuadrúpedo
                float sw = 30f * amp;
                float sA = Mathf.Sin(_phase), sB = Mathf.Sin(_phase + Mathf.PI);
                Rot(BodyPart.ShoulderLeft, sw * sA, 0f, 0f);  Rot(BodyPart.ThighRight, sw * sA, 0f, 0f);
                Rot(BodyPart.ShoulderRight, sw * sB, 0f, 0f); Rot(BodyPart.ThighLeft, sw * sB, 0f, 0f);
                Rot(BodyPart.ForearmLeft, -25f * amp * Mathf.Max(0f, -sA), 0f, 0f);  Rot(BodyPart.ShinRight, 30f * amp * Mathf.Max(0f, -sA), 0f, 0f);
                Rot(BodyPart.ForearmRight, -25f * amp * Mathf.Max(0f, -sB), 0f, 0f); Rot(BodyPart.ShinLeft, 30f * amp * Mathf.Max(0f, -sB), 0f, 0f);
                Rot(BodyPart.Head, 5f * amp * Mathf.Sin(_phase * 2f), 0f, 0f);
                // Cola: barómetro emocional (contento → se mece rápido; miedo → baja y quieta; dormido → reposo).
                float wag = 10f, low = 0f, speed = 3f;
                if (_a != null)
                {
                    if (_a.aware) { wag = 2f; low = 30f; }
                    else if (_a.satisfaction > 0.6f) { wag = 28f; speed = 9f; low = -15f; }
                    if (asleep) { wag = 0f; low = 20f; }
                }
                float t = Time.time * speed;
                Rot(BodyPart.Tail, low * -0.5f, wag * Mathf.Sin(t), 0f);
                Rot(BodyPart.Tail2, 0f, wag * 0.8f * Mathf.Sin(t - 0.6f), 0f);
                Rot(BodyPart.Tail3, 0f, wag * 0.6f * Mathf.Sin(t - 1.2f), 0f);
                break;
        }
    }

    void Rot(BodyPart p, float x, float y, float z)
    {
        if (!_j.TryGetValue(p, out CubeJoint j) || j == null) return;
        if (_rig != null && _rig.IsPoseLocked(p)) return;
        j.transform.localRotation = j.RestRotation * Quaternion.Euler(x, y, z);
    }
}
