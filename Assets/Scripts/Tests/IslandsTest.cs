using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ARCHIPIÉLAGO (docs/ice-sanctuary-ecology): las islas se cruzan NADANDO y la IA usa los enlaces de nado
/// (NavMeshLink). Comprueba que hay camino completo del hub a otras islas, que la laguna central es agua (sin NavMesh),
/// que la fauna marina vive en la laguna y que la casa del árbol y la enfermería existen.
/// </summary>
public class IslandsTest : MonoBehaviour, ITestUnit
{
    public int Group => 25;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Islas (nado entre casquetes, laguna, casa del árbol, enfermería)");
        yield return null;

        Vector3 hub = new Vector3(5f, 0f, -10f);
        bool OnMesh(Vector3 p, out Vector3 snapped)
        {
            snapped = p;
            if (NavMesh.SamplePosition(p, out NavMeshHit h, 4f, NavMesh.AllAreas)) { snapped = h.position; return true; }
            return false;
        }

        TestProbe.Check("hay NavMesh en el hub", OnMesh(hub, out Vector3 a));
        foreach (var (name, target) in new[] { ("la casa del árbol (Guardia)", new Vector3(-20f, 0f, 47f)), ("la torre de Servicios (cocina y hospital)", new Vector3(5f, 0f, 84f)), ("el lóbulo de la antigua enfermería", new Vector3(55f, 0f, 106f)) })
        {
            bool onB = OnMesh(target, out Vector3 b);
            var path = new NavMeshPath();
            bool ok = onB && NavMesh.CalculatePath(a, b, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
            TestProbe.Check($"camino completo del hub a {name} (cruzando a nado)", ok, onB ? $"estado {path.status}, {path.corners.Length} esquinas" : "sin NavMesh en el destino");
        }

        TestProbe.Check("la laguna central es agua (sin NavMesh en su centro)", !NavMesh.SamplePosition(new Vector3(60f, 0f, 25f), out _, 6f, NavMesh.AllAreas));

        int marineInLagoon = 0, marine = 0;
        foreach (Animal an in FindObjectsByType<Animal>(FindObjectsSortMode.None))
        {
            string n = an.name.ToLowerInvariant();
            if (!(n.Contains("whale") || n.Contains("orca") || n.Contains("seal"))) continue;
            marine++;
            Vector3 d = an.transform.position - new Vector3(60f, 0f, 25f); d.y = 0f;
            if (d.magnitude < 90f) marineInLagoon++;   // la laguna y sus alrededores (las ballenas pasean lejos)
        }
        TestProbe.Check("la fauna marina vive en la laguna", marine > 0 && marineInLagoon * 2 >= marine, $"{marineInLagoon}/{marine} en la laguna");

        GameObject tree = GameObject.Find("CasaArbolGuardia");
        TestProbe.NotNull("la casa del árbol de guardia existe", tree);
        TestProbe.Check("la casa del árbol no es carnívora", tree != null && tree.GetComponent<IceCarnivorousTree>() == null);
        // Sin escaleras verticales (Berón 2026-10-27, docs/alfombras-plan.md): la casa del árbol se sube en alfombra.
        int ladders = 0;
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name == "Escalera" && t.Find("Peldano") != null) ladders++;
        TestProbe.Check("ya no hay escaleras verticales (se sube en alfombra)", ladders == 0, $"{ladders} escaleras");
        TestProbe.Check("la casa del árbol de guardia tiene alfombra en el suelo y arriba", CarpetPad.All.FindAll(p => p.building == "Casa del guarda").Count >= 2);
        TestProbe.NotNull("la enfermería tiene analizador clínico", FindFirstObjectByType<ClinicalStation>());
        TestProbe.End();
    }
}
