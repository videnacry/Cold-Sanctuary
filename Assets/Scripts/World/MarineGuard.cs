using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// LOS CETÁCEOS NO PISAN EL HIELO: ballenas (belugas) y orcas viven solo en el agua (afinidad de tierra &lt; 0,2,
/// <see cref="Archetypes"/>). Un NavMeshAgent activo los "pega" al NavMesh más cercano —que es el hielo— al nacer una cría
/// o al recolocarse, y acababan andando sobre los casquetes. Cada 2 s, cualquier cetáceo que esté sobre el hielo pierde
/// el agente y vuelve a la laguna (repartidos, no todos en el mismo punto). Las focas y pingüinos (anfibios) no se tocan.
/// </summary>
public class MarineGuard : MonoBehaviour
{
    static readonly Vector3 Lagoon = new Vector3(60f, 0f, 25f);   // = PolarLandscapeBuilder.Lagoon
    const float LagoonRadius = 24f, MapHalf = 250f, SwimY = -1f;
    float _next;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<MarineGuard>() != null) return;
        new GameObject("MarineGuard_AUTO").AddComponent<MarineGuard>();
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + 2f;
        foreach (Anima a in FindObjectsByType<Anima>(FindObjectsSortMode.None))
        {
            if (a.death || a.LandAffinity >= 0.2f) continue;
            Vector3 p = a.transform.position;
            if (Mathf.Abs(p.x) > MapHalf || Mathf.Abs(p.z) > MapHalf) continue;   // escenarios de prueba fuera del mapa
            if (!OnIce(a)) continue;
            if (a.TryGetComponent(out NavMeshAgent nav)) nav.enabled = false;
            float ang = Random.value * Mathf.PI * 2f, d = Random.Range(4f, LagoonRadius);
            a.transform.position = new Vector3(Lagoon.x + Mathf.Cos(ang) * d, SwimY, Lagoon.z + Mathf.Sin(ang) * d);
            Debug.Log($"[Marinos] «{a.name}» estaba sobre el hielo en ({p.x:0},{p.z:0}): vuelve a la laguna.");
        }
    }

    static bool OnIce(Anima a)
    {
        Vector3 p = a.transform.position;
        if (p.y < -0.6f) return false;   // nadando
        if (a.TryGetComponent(out NavMeshAgent nav) && nav.enabled && nav.isOnNavMesh) return true;
        return Physics.Raycast(p + Vector3.up, Vector3.down, out RaycastHit h, 2.5f, ~0, QueryTriggerInteraction.Ignore)
               && h.collider.GetComponentInParent<Anima>() != a && h.point.y > -0.4f;
    }
}
