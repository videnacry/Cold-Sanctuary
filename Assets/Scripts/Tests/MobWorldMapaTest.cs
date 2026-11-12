using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PRUEBA DEL MAPA A MANO EN EL MOBWORLD DE MESOPOTAMIA (2026-11-12; petición 75, regla M9; docs/microcosmos-mapas-a-mano.md §Mesopotamia):
/// la escena MobWorld_Mesopotamia (Tools → Cold Sanctuary → Build MobWorld Mesopotamia) siembra la flora y la fauna del mapa de Uruk
/// (MicroStoryMaps «Mesopotamia») con AreaMicrocosmos.Nature, escalado a su suelo de 120 m:
/// - bajo «Flora_a_escala», las plantas del mapa (≥ la mitad de sus plantas, y al menos 6 de sus rodales distintos);
/// - bajo «Fauna_a_escala», ≥ 8 bichos;
/// - todo dentro del suelo (a ≤ 60 m del centro de la ciudad).
/// Carga en aditivo con espera acotada y descarga si la cargó ella.
/// </summary>
public class MobWorldMapaTest : MonoBehaviour, ITestUnit
{
    public int Group => 116;
    public bool ParallelSafe => false;
    public float loadTimeout = 120f;
    const string SceneName = "MobWorld_Mesopotamia";
    static readonly Vector3 O = new Vector3(5000f, 0f, 5000f);   // el origen lejano del MobWorld (MobWorldSceneBuilder.O)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<MobWorldMapaTest>() == null) tr.gameObject.AddComponent<MobWorldMapaTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("MobWorld de Mesopotamia: siembra la flora y la fauna de su mapa a mano (Uruk)");
        MicroStoryMaps.Map map = MicroStoryMaps.For("Mesopotamia");
        if (!TestProbe.Check("el mapa de Mesopotamia está hecho", map != null && MicroStoryMaps.Authored("Mesopotamia"))) { TestProbe.End(); yield break; }
        if (!Application.CanStreamedLevelBeLoaded(SceneName)) { TestProbe.Check($"{SceneName} está en Build Settings", false, "genera «Tools/Cold Sanctuary/Build MobWorld Mesopotamia»"); TestProbe.End(); yield break; }

        Scene s = SceneManager.GetSceneByName(SceneName);
        bool loadedHere = false;
        if (!s.isLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (op != null && !op.isDone && Time.realtimeSinceStartup < until) yield return null;
            s = SceneManager.GetSceneByName(SceneName);
            if (!TestProbe.Check($"{SceneName} se carga", s.isLoaded)) { TestProbe.End(); yield break; }
            loadedHere = true;
        }
        yield return null;

        var prefixes = map.flora.Select(p => p.plant).Distinct().ToList();
        int plants = 0, animals = 0, outside = 0; var kinds = new HashSet<string>(); string far = "";
        foreach (GameObject root in s.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Flora_a_escala")
                    foreach (Transform pl in t)
                    {
                        string pre = prefixes.FirstOrDefault(x => pl.name.StartsWith(x));
                        if (pre == null) continue;
                        plants++; kinds.Add(pre);
                        Vector3 d = pl.position - O; d.y = 0f;
                        if (Mathf.Abs(d.x) > 60f || Mathf.Abs(d.z) > 60f) { outside++; if (far.Length == 0) far = $"{pl.name} a ({d.x:0}, {d.z:0})"; }
                    }
                if (t.name == "Fauna_a_escala") animals += t.childCount;
            }
        TestProbe.Check($"las plantas del mapa están sembradas (≥ {map.PlantCount / 2} de {map.PlantCount})", plants >= map.PlantCount / 2, $"{plants} plantas del mapa");
        TestProbe.Check("al menos 6 de sus rodales distintos", kinds.Count >= 6, string.Join(", ", kinds));
        TestProbe.Check("≥ 8 bichos del mapa", animals >= 8, $"{animals} bichos");
        TestProbe.Check("todo dentro del suelo de 120 m", outside == 0, far);

        if (loadedHere)
        {
            AsyncOperation un = SceneManager.UnloadSceneAsync(s);
            float until = Time.realtimeSinceStartup + 30f;
            while (un != null && !un.isDone && Time.realtimeSinceStartup < until) yield return null;
        }
        TestProbe.End();
    }
}
