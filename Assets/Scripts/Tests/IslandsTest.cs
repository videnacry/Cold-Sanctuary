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
        foreach (var (name, target) in new[] { ("la casa del árbol (Guardia)", new Vector3(-20f, 0f, 47f)), ("la Cocina", new Vector3(5f, 0f, 84f)), ("la Enfermería", new Vector3(55f, 0f, 106f)) })
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
        TestProbe.Check("la escalera vertical tiene su pie interactuable", FindObjectsByType<Ladder>(FindObjectsSortMode.None).Length >= 2);
        TestProbe.Check("las escaleras verticales tienen su enlace del NavMesh (la IA trepa)", FindObjectsByType<Ladder>(FindObjectsSortMode.None).Length > 0
            && System.Array.TrueForAll(FindObjectsByType<Ladder>(FindObjectsSortMode.None), l => l.ladder != null && l.ladder.GetComponent<Unity.AI.Navigation.NavMeshLink>() != null));
        TestProbe.NotNull("la enfermería tiene analizador clínico", FindFirstObjectByType<ClinicalStation>());

        // SUBIR y BAJAR por la escalera vertical (un cuerpo de prueba con CharacterController, como el jugador).
        Ladder foot = null;
        foreach (Ladder ld in FindObjectsByType<Ladder>(FindObjectsSortMode.None)) if (!ld.top) foot = ld;
        if (foot != null)
        {
            GameObject body = new GameObject("Trepador_TEST");
            body.AddComponent<CharacterController>();
            body.transform.position = foot.transform.position;
            float speed = foot.speed; foot.speed = 20f;   // rápido para el test
            yield return StartCoroutine(foot.Climb(body.transform, true));
            float top = body.transform.position.y - foot.ladder.position.y;
            TestProbe.Near("subir la escalera deja al cuerpo en la plataforma de la casa del árbol", top, foot.deckY + 1.2f, 0.3f);
            yield return StartCoroutine(foot.Climb(body.transform, false));
            TestProbe.Check("bajar la escalera lo devuelve al suelo", body.transform.position.y - foot.ladder.position.y < 2f, $"y={body.transform.position.y:0.0}");
            foot.speed = speed;
            Destroy(body);
        }
        TestProbe.End();
    }
}
