using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ESCENAS DE CICLO DE VIDA DE UNA PERSONA (Berón 2026-11-02: «¿el ciclo de vida está probado para los humanos? El ciclo de vida
/// de los humanos y de los insectos-humanos es el mismo»). Como las islas de los animales (SampleSceneBuilder.CicloVida), dos
/// escenas en Assets/Scenes/CicloVida/ (Inicio las lista en «Ciclo de vida»):
///   · CicloVida_Humano: una mujer y un hombre adultos (cuerpos de cubos), todavía sin pareja, en una CASITA (la misma casa que
///     reciben las familias de los pueblos: CivicBuilder.LifeCycleHouse) con despensa inagotable, en un prado;
///   · CicloVida_InsectoHumano: una pareja de insectos-humanos de especies distintas (con su árbol genealógico) en un SOLAR de
///     Kapilavastu (s. V a. C.): su casa de ladrillo, la calle y el horario de su era (MicroJornada).
/// Cada una con el reloj (un día = 1 h a ×1, el año comprimido), el panel (<see cref="CicloHumanoPanel"/>), el explorador y el
/// orquestador de pruebas (solo el grupo 59, <see cref="CicloHumanoTest"/>: LA MISMA prueba en las dos). Fotos en Temp/CicloHumano/.
/// </summary>
public static partial class SampleSceneBuilder
{
    [MenuItem("Tools/Cold Sanctuary/Build escenas de ciclo de vida de una persona (humana e insecto-humana)")]
    public static void BuildCicloHumanoScenes()
    {
        MicrocosmosAreaSceneBuilder.DropUntitled();
        if (!AssetDatabase.IsValidFolder(CicloFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "CicloVida");
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        int n = 0;
        foreach (bool insects in new[] { false, true })
        {
            try { if (BuildCicloHumano(insects, list)) n++; }
            catch (System.Exception e) { Debug.LogError($"[CicloHumano] {(insects ? "insectos-humanos" : "humanos")}: {e}"); }
        }
        EditorBuildSettings.scenes = list.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log($"[CicloHumano] {n}/2 escenas de ciclo de vida de una persona en {CicloFolder} (fotos en Temp/CicloHumano/).");
    }

    static bool BuildCicloHumano(bool insects, List<EditorBuildSettingsScene> list)
    {
        string sceneName = insects ? "CicloVida_InsectoHumano" : "CicloVida_Humano";
        string display = insects ? "Pareja de insectos-humanos (Kapilavastu)" : "Pareja humana";
        string path = $"{CicloFolder}/{sceneName}.unity";
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        var sun = new GameObject("Sol").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.15f; sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        RenderSettings.fog = false;
        RenderSettings.ambientLight = insects ? new Color(0.72f, 0.66f, 0.58f) : new Color(0.62f, 0.68f, 0.72f);

        // ── El terreno: un prado (humanos) o el solar de tierra de un pueblo de la era (insectos-humanos), con su calle ──
        var land = new GameObject(insects ? "Solar_de_Kapilavastu" : "Prado");
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Suelo_del_terreno";
        ground.transform.SetParent(land.transform, false);
        ground.transform.localPosition = new Vector3(0f, -0.1f, 0f); ground.transform.localScale = new Vector3(48f, 0.2f, 48f);
        ground.GetComponent<Renderer>().sharedMaterial = MakeMaterial(insects ? "CicloHumano_Tierra_MAT" : "CicloHumano_Prado_MAT",
            insects ? new Color(0.62f, 0.5f, 0.36f) : new Color(0.42f, 0.6f, 0.34f));
        var street = GameObject.CreatePrimitive(PrimitiveType.Cube); street.name = insects ? "Calle_este_oeste" : "Camino";
        street.transform.SetParent(land.transform, false);
        street.transform.localPosition = new Vector3(0f, 0.005f, -14f); street.transform.localScale = new Vector3(48f, 0.01f, 2f * MicroBuildRules.Street);
        street.GetComponent<Renderer>().sharedMaterial = MakeMaterial(insects ? "CicloHumano_Calle_MAT" : "CicloHumano_Camino_MAT",
            insects ? new Color(0.5f, 0.42f, 0.32f) : new Color(0.66f, 0.6f, 0.5f));

        // ── La pareja (todavía sin pareja: el cortejo se ve aquí) ──
        var people = new GameObject("Personas");
        GameObject she, he;
        if (insects)
        {
            she = AreaMicrocosmos.Folk(people.transform, "Vecina de Kapilavastu", new Vector3(-2f, 0f, -9f), 1.7f, 1f, "Ladybug", null, true);
            he = AreaMicrocosmos.Folk(people.transform, "Vecino de Kapilavastu", new Vector3(2f, 0f, -9f), 1.78f, 1f, "Spider", null, false);
            var rnd = new System.Random(7);
            foreach (GameObject g in new[] { she, he })
            {
                CubeInsect.WearShirt(g, Color.HSVToRGB(Mathf.Abs(g.name.GetHashCode() % 100) / 100f, 0.35f, 0.75f));
                string R() => InsectCast.Folk[rnd.Next(InsectCast.Folk.Length)];
                g.GetComponent<FolkLineage>().ancestors = new List<InsectGenealogy.Ancestor> { new InsectGenealogy.Ancestor("madre", R(), 1), new InsectGenealogy.Ancestor("padre", R(), 1),
                    new InsectGenealogy.Ancestor("abuela", R(), 2), new InsectGenealogy.Ancestor("abuelo", R(), 2), new InsectGenealogy.Ancestor("bisabuelo", "Centipede", 3) };
            }
        }
        else
        {
            she = Person(people.transform, "Elena", new Vector3(-2f, 0f, -9f), 1.68f, true, new Color(0.55f, 0.35f, 0.5f));
            he = Person(people.transform, "Tomás", new Vector3(2f, 0f, -9f), 1.8f, false, new Color(0.3f, 0.4f, 0.55f));
        }

        // ── Su casa: la misma que reciben las familias de los pueblos (CivicBuilder.Homes), con despensa inagotable ──
        AreaFunctionSite house = CivicBuilder.LifeCycleHouse(land.transform, new List<GameObject> { she, he }, insects);
        Household home = house.GetComponentInChildren<Household>();
        if (insects)
            foreach (GameObject g in new[] { she, he })
            {
                var j = g.AddComponent<MicroJornada>();   // el horario de su era encima del ciclo: trabaja en casa, come y descansa allí
                j.work = house; j.dine = house; j.home = house; j.role = "";
            }
        else foreach (GameObject g in new[] { she, he }) g.GetComponent<PersonLife>().freeLife = true;   // sin horario: sus necesidades, a cualquier hora
        // Dentro de casa, junto a la mesa.
        Vector3 table = home.table != null ? home.table.position : house.transform.position;
        she.transform.position = HomeNeedSpell.UsePoint(home.table != null ? home.table : house.transform, table + Vector3.left * 3f, 1.7f) + Vector3.left * 0.8f;
        he.transform.position = she.transform.position + Vector3.right * 1.6f;

        // NavMesh del terreno y la casa.
        NavMeshSurface surf = land.AddComponent<NavMeshSurface>();
        surf.collectObjects = CollectObjects.Children;
        surf.BuildNavMesh();

        // ── Sistemas: reloj, panel, pruebas, explorador ──
        var clock = new GameObject("Clock_AUTO").AddComponent<Clock>();
        clock.startHour = 8f; clock.realMinutesPerDay = 60f; clock.daysPerYear = CicloVidaPanel.CompressedYear;
        var panel = new GameObject("CicloHumano_Panel").AddComponent<CicloHumanoPanel>();
        panel.displayName = display; panel.insects = insects;
        new GameObject("TestRunner_AUTO").AddComponent<ColdSanctuary.TestRunner>().onlyGroups = new[] { 59 };   // solo su prueba
        var label = new GameObject("Rotulo"); label.transform.position = new Vector3(0f, 0f, 14f);
        WorldLabel wl = label.AddComponent<WorldLabel>(); wl.height = 5f;
        wl.text = insects ? "Ciclo de vida: insectos-humanos\nuna mariquita y una araña (y su árbol genealógico) en su casa de Kapilavastu"
                          : "Ciclo de vida: humanos\nuna mujer y un hombre en su casita (despensa inagotable)";
        GalleryExplorer.Spawn(new Vector3(0f, 0.1f, -20f), 0f);

        // Foto de conjunto (Temp/CicloHumano/<escena>.png).
        Vector3 look = house.transform.position;
        HumanShot(look + new Vector3(-9f, 9f, -16f), look + Vector3.up * 0.8f, sceneName);

        EditorSceneManager.SaveScene(scene, path);
        if (!list.Exists(s => s.path == path)) list.Add(new EditorBuildSettingsScene(path, true));
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
        Debug.Log($"[CicloHumano] {path}: {display} en «{house.title}» ({home.beds.Count} camas, retrete, lavabo, mesa con {home.seats.Count} sillas, despensa inagotable).");
        return true;
    }

    // Una PERSONA humana: cuerpo de cubos a su estatura, cápsula y agente (anda por el NavMesh) y el ciclo de vida de una persona.
    static GameObject Person(Transform parent, string name, Vector3 pos, float h, bool female, Color clothes)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false); g.transform.position = pos;
        var col = g.AddComponent<CapsuleCollider>(); col.height = h; col.radius = 0.2f * h; col.center = Vector3.up * h * 0.5f;
        var ag = g.AddComponent<UnityEngine.AI.NavMeshAgent>(); ag.radius = 0.3f; ag.height = h; ag.speed = 1.6f; ag.angularSpeed = 360f;
        CubeHumanoid.Dress(g, clothes, new Color(0.86f, 0.7f, 0.58f), h);
        var wl = g.AddComponent<WorldLabel>(); wl.text = name; wl.height = h + 0.3f;
        PersonLife.Equip(g, female);
        return g;
    }

    /// <summary>Una foto 1600 × 900 de la escena abierta en Temp/CicloHumano/<paramref name="file"/>.png.</summary>
    internal static void HumanShot(Vector3 from, Vector3 to, string file)
    {
        var camGo = new GameObject("Camara_foto");
        Camera cam = camGo.AddComponent<Camera>(); cam.fieldOfView = 50f; cam.farClipPlane = 1500f;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.62f, 0.75f, 0.88f);
        camGo.transform.position = from; camGo.transform.LookAt(to);
        var rt = new RenderTexture(1600, 900, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); tex.Apply();
        string dir = Path.Combine(Application.dataPath, "..", "Temp", "CicloHumano"); Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, file + ".png"), tex.EncodeToPNG());
        RenderTexture.active = null; cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
        Object.DestroyImmediate(camGo);
    }
}
