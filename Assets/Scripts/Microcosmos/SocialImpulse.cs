using UnityEngine;

/// <summary>
/// IMPULSO SOCIAL (docs/microcosmos-level1.md §Impulsos sociales, rebanada 2): una fuente de <see cref="MovementImpulse"/>
/// dirigida hacia (o lejos de) OTRO ser, según un DRIVE social. Se apoya en el <see cref="ImpulseController"/> ya
/// existente (igual que <see cref="HomeImpulse"/>/<see cref="ThreatScanner"/>): cada tick calcula un objetivo y añade
/// un impulso con su tag; el controlador suma todo y mueve al agente. Varios drives conviven en un mismo ser
/// (p.ej. Sakshi = Tend(Ambrosio) que gana a Follow(tribu) → se separa del grupo → baja la cohesión).
///
/// Drives (mapean los del diseño):
///  • <b>Follow</b>  — hacia el CENTROIDE de la tribu cercana (cohesión de manada).
///  • <b>Tend</b>    — hacia el más DÉBIL/enfermo cercano (cuidar). En rango de cuidado, cura un poco su estrés.
///  • <b>Adore</b>   — hacia un individuo MAGNÉTICO (fixedTarget, p.ej. Momo).
///  • <b>Obey</b>    — hacia el LÍDER (fixedTarget, p.ej. Héspero).
///  • <b>Observe</b> — hacia un estímulo (fixedTarget) y, al llegar, se QUEDA (contemplación: Sakshi "varada").
///  • <b>Grief</b>   — hacia un MUERTO/colapsado cercano; deposita cortisol (duelo).
///  • <b>Cull</b>    — LEJOS del más débil cercano (Héspero abandona a los débiles).
///  • <b>Gather</b>  — hacia la COMIDA cercana (Ruth y el hongo/melaza).
///
/// Additivo y balance-safe: sin ImpulseController/NavMesh el ser no se mueve (igual que antes); sin objetivo, no añade
/// impulso. La MAGNITUD se modula por la sociabilidad del ser (más social → tira más fuerte de lo social).
/// </summary>
[RequireComponent(typeof(ImpulseController))]
public class SocialImpulse : MonoBehaviour
{
    public enum SocialDrive { Follow, Tend, Adore, Obey, Observe, Grief, Cull, Gather }

    [Tooltip("Qué pulsión social dirige este impulso.")]
    public SocialDrive drive = SocialDrive.Follow;

    [Tooltip("Objetivo fijo (Adore/Obey/Observe). Para Follow/Tend/Grief/Cull/Gather se busca por cercanía.")]
    public Transform fixedTarget;

    [Tooltip("Magnitud base del impulso (se escala por la sociabilidad del ser).")]
    [Min(0f)] public float magnitude = 1f;

    [Tooltip("Radio de búsqueda/percepción social.")]
    [Min(1f)] public float range = 15f;

    [Tooltip("Distancia a la que se considera 'llegado': el impulso de atracción cae a 0 (se queda al lado).")]
    [Min(0.1f)] public float arrivalRadius = 1.5f;

    [Tooltip("Frecuencia de re-evaluación (s).")]
    [Min(0.1f)] public float updateRate = 0.4f;

    ImpulseController _ctrl;
    Anima _self;
    float _next;
    string _tag;

    void Awake()
    {
        _ctrl = GetComponent<ImpulseController>();
        _self = GetComponent<Anima>();
        _tag  = "social_" + drive;
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + updateRate;
        _ctrl.RemoveByTag(_tag);

        bool repel = drive == SocialDrive.Cull;
        Transform target = FindTarget();
        if (target == null) return;

        Vector3 to = target.position - transform.position; to.y = 0f;
        float dist = to.magnitude;

        // Atracción: al llegar, no empujar más (se queda al lado / contempla / cuida).
        if (!repel && dist <= arrivalRadius)
        {
            if (drive == SocialDrive.Tend) TendCare(target);   // cuidar reduce el estrés del cuidado
            return;
        }
        if (dist < 0.01f) return;

        float social = _self != null ? Mathf.Max(0.2f, _self.sociability) : 1f;   // más social → tira más de lo social
        float mag = magnitude * social;
        Vector3 dir = repel ? -to : to;
        _ctrl.AddImpulse(new MovementImpulse(_tag, dir, mag, 0f));   // persistente (0 decaimiento); se recalcula cada tick
    }

    Transform FindTarget()
    {
        switch (drive)
        {
            case SocialDrive.Adore:
            case SocialDrive.Obey:
            case SocialDrive.Observe: return fixedTarget;
            case SocialDrive.Follow:  return TribeCentroid();
            case SocialDrive.Tend:    return fixedTarget != null ? fixedTarget : NearestWeak();   // cuida a uno concreto (guion) o al más débil
            case SocialDrive.Cull:    return NearestWeak();
            case SocialDrive.Grief:   return fixedTarget != null ? fixedTarget : NearestDead();
            case SocialDrive.Gather:  return NearestFood();
        }
        return null;
    }

    // Centroide de las Ánimas cercanas (excluyéndome): el "centro de la tribu" → cohesión.
    Transform TribeCentroid()
    {
        Vector3 sum = Vector3.zero; int n = 0; Transform any = null;
        foreach (Collider c in Physics.OverlapSphere(transform.position, range))
        {
            Anima a = c.GetComponentInParent<Anima>();
            if (a == null || a == _self) continue;
            sum += a.transform.position; n++; any = a.transform;
        }
        if (n == 0) return null;
        if (n == 1) return any;
        // objetivo virtual en el centroide
        _centroid.position = sum / n;
        return _centroid;
    }

    static Transform _centroidHolder;
    Transform _centroid
    {
        get
        {
            if (_centroidHolder == null) _centroidHolder = new GameObject("SocialCentroid_TMP").transform;
            return _centroidHolder;
        }
    }

    Transform NearestWeak()
    {
        Transform best = null; float bestSq = range * range;
        foreach (Collider c in Physics.OverlapSphere(transform.position, range))
        {
            WeakOne w = c.GetComponentInParent<WeakOne>();
            if (w == null || w.transform == transform) continue;
            float d = (w.transform.position - transform.position).sqrMagnitude;
            if (d < bestSq) { bestSq = d; best = w.transform; }
        }
        return best;
    }

    Transform NearestDead()
    {
        Transform best = null; float bestSq = range * range;
        foreach (Collider c in Physics.OverlapSphere(transform.position, range))
        {
            Anima a = c.GetComponentInParent<Anima>();
            if (a == null || a == _self || !a.death) continue;
            float d = (a.transform.position - transform.position).sqrMagnitude;
            if (d < bestSq) { bestSq = d; best = a.transform; }
        }
        return best;
    }

    Transform NearestFood()
    {
        Transform best = null; float bestSq = range * range;
        foreach (Collider c in Physics.OverlapSphere(transform.position, range))
        {
            HoneydewProducer h = c.GetComponentInParent<HoneydewProducer>();
            if (h == null) continue;
            float d = (h.transform.position - transform.position).sqrMagnitude;
            if (d < bestSq) { bestSq = d; best = h.transform; }
        }
        return best;
    }

    // Cuidar: reduce lentamente el estrés del CUIDADO (no del cuidado-ero). Aproxima el "tender" del diseño.
    void TendCare(Transform cared)
    {
        Anima a = cared.GetComponentInParent<Anima>();
        if (a != null) a.stress = Mathf.Max(0f, a.stress - 0.2f * updateRate);
    }
}
