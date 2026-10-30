using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

/// <summary>
/// MUEBLES QUE EL NAVMESH NO VE: el NavMesh de la escena se horneó antes de colocar parte del mobiliario (las máquinas del
/// gimnasio, los bloques de la central nuclear, la grúa y la miniexcavadora de la obra…), así que el camino pasaba POR
/// DENTRO de ellos y la gente los atravesaba (JornadaTest 2026-10-29: Linduarda y Goluis por el «Bloque_Intercambiador»,
/// Panterilia por la prensa de piernas). Al empezar, a cada caja sólida con NavMesh dentro de su planta se le pone un
/// <see cref="NavMeshObstacle"/> que TALLA el NavMesh (solo una vez: están quietas). Son pocas (≈ 70 de 3500 cajas).
/// No toca los corrales y cuadras de los animales (sus agentes están dentro) ni los trineos. Las RAÍCES de los árboles sí
/// tallan (JornadaTest 2026-10-29: un oso y un pingüino se metían en la «Raiz» camino de su comida).
/// </summary>
public class StaleNavCarver : MonoBehaviour
{
    public static int Carved { get; private set; }

    static readonly string[] Skip = { "Cria_", "Cuadra", "Trineo", "Barrera_agua", "Inodoro", "Cabina", "Puerta", "Cama", "Silla", "Asiento" };

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
            // Suelos, losas, muros largos y el hielo (su cara de arriba es el suelo: max.y ≤ 0,35). Antes se descartaba todo lo
            // que bajara de −0,5 m, y con eso las RAÍCES grandes —inclinadas, se hunden en el hielo— no tallaban nunca: el
            // pingüino de (−134, 0, 18) seguía metiéndose en la «Raiz».
            if (b.size.y < 0.4f || b.size.x > 10f || b.size.z > 10f || b.max.y < 0.35f) continue;
            bool skip = false; foreach (string s in Skip) if (c.name.StartsWith(s)) { skip = true; break; }
            if (skip) continue;
            // Lo que tiene su PROPIO NavMesh (el Árbol de los Campos, que gira con él) no talla el general.
            NavMeshSurface own = c.GetComponentInParent<NavMeshSurface>();
            if (own != null && own.collectObjects == CollectObjects.Children) continue;
            if (!NavInside(b) && !NavThrough(c, b)) continue;
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

    // ¿Pasa el NavMesh POR DENTRO del sólido a cualquier altura de su caja? (lo que se hunde en el suelo: la base queda bajo el
    // NavMesh y la prueba de arriba, a la altura de la base, no lo encontraba). Por cada punto de su planta busca el NavMesh en
    // su columna y mira si ese punto, a la altura de un pie, queda DENTRO del collider (no encima: su tapa transitable no cuenta).
    static bool NavThrough(BoxCollider c, Bounds b)
    {
        for (float x = -0.3f; x <= 0.31f; x += 0.3f)
            for (float z = -0.3f; z <= 0.31f; z += 0.3f)
            {
                Vector3 p = new Vector3(b.center.x + x * b.size.x, b.center.y, b.center.z + z * b.size.z);
                if (!NavMesh.SamplePosition(p, out NavMeshHit h, b.extents.y + 0.3f, NavMesh.AllAreas)) continue;
                if (Mathf.Abs(h.position.x - p.x) > 0.3f || Mathf.Abs(h.position.z - p.z) > 0.3f) continue;
                if (h.position.y > b.max.y - 0.25f) continue;
                Vector3 foot = h.position + Vector3.up * 0.1f;
                if ((c.ClosestPoint(foot) - foot).sqrMagnitude < 1e-6f) return true;   // (ClosestPoint de un punto interior = él mismo: el punto es preciso, basta uno)
            }
        return false;
    }
}
