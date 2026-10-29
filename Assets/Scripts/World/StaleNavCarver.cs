using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// MUEBLES QUE EL NAVMESH NO VE: el NavMesh de la escena se horneó antes de colocar parte del mobiliario (las máquinas del
/// gimnasio, los bloques de la central nuclear, la grúa y la miniexcavadora de la obra…), así que el camino pasaba POR
/// DENTRO de ellos y la gente los atravesaba (JornadaTest 2026-10-29: Linduarda y Goluis por el «Bloque_Intercambiador»,
/// Panterilia por la prensa de piernas). Al empezar, a cada caja sólida con NavMesh dentro de su planta se le pone un
/// <see cref="NavMeshObstacle"/> que TALLA el NavMesh (solo una vez: están quietas). Son pocas (≈ 70 de 3500 cajas).
/// No toca los corrales y cuadras de los animales (sus agentes están dentro), ni raíces ni trineos.
/// </summary>
public class StaleNavCarver : MonoBehaviour
{
    public static int Carved { get; private set; }

    static readonly string[] Skip = { "Cria_", "Cuadra", "Raiz", "Trineo", "Barrera_agua", "Inodoro", "Cabina", "Puerta", "Cama", "Silla", "Asiento" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<StaleNavCarver>() != null) return;
        new GameObject("StaleNavCarver").AddComponent<StaleNavCarver>();
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);   // (la escena termina de montarse: obras, gimnasio)
        int n = 0;
        foreach (BoxCollider c in FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
        {
            if (c == null || c.isTrigger || !c.enabled || !c.gameObject.activeInHierarchy) continue;
            if (c.GetComponentInParent<Anima>() != null || c.GetComponent<NavMeshObstacle>() != null) continue;
            if (c.attachedRigidbody != null && !c.attachedRigidbody.isKinematic) continue;
            Bounds b = c.bounds;
            if (b.size.y < 0.4f || b.size.x > 10f || b.size.z > 10f || b.min.y < -0.5f) continue;   // suelos, losas, muros largos, el hielo
            bool skip = false; foreach (string s in Skip) if (c.name.StartsWith(s)) { skip = true; break; }
            if (skip) continue;
            if (!NavInside(b)) continue;
            NavMeshObstacle o = c.gameObject.AddComponent<NavMeshObstacle>();
            o.shape = NavMeshObstacleShape.Box;
            o.center = c.center; o.size = c.size;
            o.carving = true; o.carveOnlyStationary = true;
            n++;
        }
        Carved = n;
        Debug.Log($"[NavMesh] {n} muebles sin hueco en el NavMesh: ahora lo tallan (no se atraviesan).");
    }

    // ¿Hay NavMesh dentro de su planta, a la altura de su base? (en ≥ 2 de 9 puntos)
    static bool NavInside(Bounds b)
    {
        int inside = 0;
        for (float x = -0.3f; x <= 0.31f; x += 0.3f)
            for (float z = -0.3f; z <= 0.31f; z += 0.3f)
            {
                Vector3 p = new Vector3(b.center.x + x * b.size.x, b.min.y + 0.1f, b.center.z + z * b.size.z);
                if (NavMesh.SamplePosition(p, out NavMeshHit h, 0.2f, NavMesh.AllAreas) && h.position.y < b.max.y && h.position.y > b.min.y - 0.3f && ++inside >= 2) return true;
            }
        return false;
    }
}
