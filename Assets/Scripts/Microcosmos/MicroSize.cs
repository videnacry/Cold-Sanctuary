using UnityEngine;

/// <summary>
/// TAMAÑO EN EL MICROCOSMOS: un insecto de la fauna (prefab real, que al nacer fija su propia escala por edad/especie)
/// cuelga de este padre, que ajusta SU escala para que el bicho mida <see cref="length"/> m, relativo a la hormiga, que
/// mide como una persona. Se corrige cada poco (si crece o cambia de edad, sigue a escala).
/// </summary>
public class MicroSize : MonoBehaviour
{
    public float length = 2f;
    float _next;
    UnityEngine.AI.NavMeshAgent _agent;

    // El bicho es un Animal con NavMeshAgent, pero cuelga de un padre ESCALADO (cientos de veces): el agente heredaba un
    // radio y una altura absurdos y no quedaba sobre el NavMesh del microcosmos → la IA no podía moverlo (insectos quietos).
    // Se le da un tamaño de agente acorde a su largo y se le posa en el NavMesh más cercano.
    void Start()
    {
        _agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        FitAgent();
        if (_agent != null && UnityEngine.AI.NavMesh.SamplePosition(_agent.transform.position, out UnityEngine.AI.NavMeshHit hit, length * 3f + 10f, UnityEngine.AI.NavMesh.AllAreas))
            _agent.Warp(hit.position);
    }

    void FitAgent()
    {
        if (_agent == null) return;
        float s = Mathf.Max(1e-4f, Mathf.Max(_agent.transform.lossyScale.x, _agent.transform.lossyScale.z));
        _agent.radius = Mathf.Clamp(length * 0.25f, 0.3f, 3f) / s;
        _agent.height = Mathf.Clamp(length * 0.4f, 0.5f, 6f) / Mathf.Max(1e-4f, _agent.transform.lossyScale.y);
        _agent.baseOffset = 0f;
    }

    void LateUpdate()
    {
        if (Time.time < _next) return;
        _next = Time.time + 0.5f;
        // Un insecto de cubos mide su largo de ADULTO por la fase en que está (InsectMorphology.PhaseScale): la ninfa, la
        // larva o la cría salen a su tamaño real relativo al adulto. Se mide su propio cuerpo (la raíz «Insecto», en
        // unidades de su largo), no la caja de las patas, para que adulto y cría se comparen igual que en la galería.
        CubeInsect ci = GetComponentInChildren<CubeInsect>();
        if (ci != null && !string.IsNullOrEmpty(ci.species) && ci.transform.Find("Insecto") is Transform body)
        {
            float kk = length * InsectMorphology.PhaseScale(ci.species, ci.Shown) / Mathf.Max(1e-6f, body.lossyScale.z);
            if (Mathf.Abs(kk - 1f) > 0.05f) { transform.localScale *= Mathf.Clamp(kk, 0.01f, 1000f); FitAgent(); }
            return;
        }
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return;
        bool any = false; Bounds b = default;
        foreach (Renderer r in rs) { if (!r.enabled) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        if (!any) return;
        float now = Mathf.Max(b.size.x, b.size.z);
        if (now < 1e-4f) return;
        float k = length / now;
        if (Mathf.Abs(k - 1f) > 0.05f) { transform.localScale *= Mathf.Clamp(k, 0.01f, 1000f); FitAgent(); }
    }
}
