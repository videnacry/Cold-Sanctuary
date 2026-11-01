using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ESCENAS DE CICLO DE VIDA (Berón 2026-11-01: «bastaría con un casquete de hielo en medio del mar, 2 animales de distinto
/// sexo adultos para que hagan una familia y […] un cuenco con toneladas de su alimento favorito para que nunca se acabe, se
/// crearía una escena por animal del santuario»; y luego: «una pequeña isla en el mar con el hábitat de ese animal»).
///
/// Una escena por especie de las que vive en SampleScene (<see cref="CicloVidaPanel.Species"/>): la fauna de hielo
/// (oso polar), la de la laguna (foca, ballena, orca), la colonia de la orilla (pingüino), la Cría (malamute) y el bosque de
/// árboles carnívoros (abeja y araña de las nieves). Todas son de HIELO, así que cada isla es una placa de banquisa en mar
/// abierto, con su hábitat:
///   · Hielo (oso, malamute): la isla, su guarida/caseta y el cuenco de carne (<see cref="BottomlessFood"/>).
///   · Costa (pingüino): la isla y, en el agua junto a la orilla, el banco de peces inagotable del que pesca.
///   · Mar (foca, ballena, orca): la laguna (la de MarineLife: (60, 25), 26 m) con su banco inagotable y la isla al lado,
///     donde la foca sale a descansar.
///   · Árbol (abejas, arañas): la isla con un árbol carnívoro de hielo y su panal (<see cref="CubeTree"/>); la miel no se
///     acaba y, para la araña, la colonia de abejas es su comida.
/// Una hembra y un macho ADULTOS, el reloj (un día = 1 h a ×1; el año, comprimido a 2 días), la rejilla de rastros (el celo),
/// el panel que marca cada proceso (<see cref="CicloVidaPanel"/>), el explorador para mirar y el orquestador de pruebas
/// (grupo 58, <see cref="CicloVidaTest"/>). Se guardan en Assets/Scenes/CicloVida/ y se añaden a Build Settings: Inicio las
/// lista en su grupo «Ciclo de vida». Una foto de cada una en Temp/CicloVida/.
/// </summary>
public static partial class SampleSceneBuilder
{
    const string CicloFolder = "Assets/Scenes/CicloVida";
    // La laguna de los marinos (MarineLife.Lagoon) y la isla al norte de ella: su orilla sur toca el agua de la laguna.
    static readonly Vector3 CvLagoon = new Vector3(60f, 0f, 25f);

    [MenuItem("Tools/Cold Sanctuary/Build escenas de ciclo de vida (una por especie)")]
    public static void BuildCicloVidaScenes()
    {
        MicrocosmosAreaSceneBuilder.DropUntitled();
        if (!AssetDatabase.IsValidFolder(CicloFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "CicloVida");
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        int n = 0;
        foreach (var (sp, scene, name, habitat) in CicloVidaPanel.Species)
        {
            try { if (BuildCicloVida(sp, scene, name, habitat, list)) n++; }
            catch (System.Exception e) { Debug.LogError($"[CicloVida] {sp}: {e}"); }
        }
        EditorBuildSettings.scenes = list.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log($"[CicloVida] {n}/{CicloVidaPanel.Species.Length} escenas de ciclo de vida en {CicloFolder} (fotos en Temp/CicloVida/).");
    }

    static bool BuildCicloVida(string sp, string sceneName, string display, CicloVidaPanel.Habitat habitat, List<EditorBuildSettingsScene> list)
    {
        GameObject prefab = LoadAnimalPrefab(sp);
        if (prefab == null) { Debug.LogWarning($"[CicloVida] sin prefab de {sp}: se omite su escena."); return false; }
        string path = $"{CicloFolder}/CicloVida_{sceneName}.unity";
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        bool tree = habitat == CicloVidaPanel.Habitat.Arbol;
        float R = tree ? 18f : 14f;
        Vector3 I = new Vector3(CvLagoon.x, 0f, CvLagoon.z + 26f + R - 1f);   // la orilla sur de la isla, 1 m dentro de la laguna

        var sun = new GameObject("Sol").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.1f; sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(40f, -35f, 0f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.80f, 0.85f, 0.90f); RenderSettings.fogDensity = 0.006f;
        RenderSettings.ambientLight = new Color(0.62f, 0.68f, 0.75f);

        // ── El mar ──
        var sea = GameObject.CreatePrimitive(PrimitiveType.Plane); sea.name = "Sea_Placeholder";
        sea.transform.position = new Vector3(CvLagoon.x, PolarLandscapeBuilder.WaterLevel, CvLagoon.z + 20f);
        sea.transform.localScale = new Vector3(60f, 1f, 60f);
        PolarLandscapeBuilder.IgnoreForNav(sea);
        sea.GetComponent<Renderer>().sharedMaterial = MakeWaterMaterial("CicloVida_Mar_MAT", new Color(0.30f, 0.55f, 0.70f, 0.45f));
        Object.DestroyImmediate(sea.GetComponent<Collider>());
        var swim = new GameObject("Sea_SwimZone");
        swim.transform.position = new Vector3(CvLagoon.x, PolarLandscapeBuilder.WaterLevel - 8f, CvLagoon.z + 20f);
        var sc = swim.AddComponent<BoxCollider>(); sc.isTrigger = true; sc.size = new Vector3(600f, 16f, 600f);
        swim.AddComponent<WaterZone>();
        swim.AddComponent<MediumZone>().medium = Medium.Water;
        var bed = new GameObject("FondoMarino");
        bed.transform.position = new Vector3(CvLagoon.x, PolarLandscapeBuilder.WaterLevel - 17f, CvLagoon.z + 20f);
        bed.AddComponent<BoxCollider>().size = new Vector3(600f, 2f, 600f);
        PolarLandscapeBuilder.IgnoreForNav(bed);

        // ── La isla: una placa de banquisa (0 a −1,5 m, como los casquetes del santuario) ──
        var island = new GameObject("Isla_" + sceneName);
        island.transform.position = I;
        Material snow = MakeMaterial("CicloVida_Nieve_MAT", new Color(0.86f, 0.9f, 0.93f));
        Material ice = MakeMaterial("CicloVida_Hielo_MAT", new Color(0.7f, 0.84f, 0.92f));
        var slab = GameObject.CreatePrimitive(PrimitiveType.Cylinder); slab.name = "Casquete";
        slab.transform.SetParent(island.transform, false);
        slab.transform.localPosition = new Vector3(0f, -0.75f, 0f); slab.transform.localScale = new Vector3(2f * R, 0.75f, 2f * R);
        slab.GetComponent<Renderer>().sharedMaterial = snow;
        Object.DestroyImmediate(slab.GetComponent<Collider>()); slab.AddComponent<MeshCollider>();
        var rnd = new System.Random(sp.GetHashCode());
        for (int i = 0; i < 7; i++)   // bloques y montículos de hielo (paisaje; los animales los rodean)
        {
            float a = (float)rnd.NextDouble() * Mathf.PI * 2f, d = R * (0.55f + 0.35f * (float)rnd.NextDouble());
            var b = GameObject.CreatePrimitive(PrimitiveType.Cube); b.name = "Bloque_hielo";
            b.transform.SetParent(island.transform, false);
            float s = 0.8f + 1.6f * (float)rnd.NextDouble();
            b.transform.localPosition = new Vector3(Mathf.Cos(a) * d, s * 0.35f, Mathf.Sin(a) * d);
            b.transform.localScale = new Vector3(s * 1.4f, s * 0.7f, s);
            b.transform.localRotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 90f, (float)rnd.NextDouble() * 12f);
            b.GetComponent<Renderer>().sharedMaterial = ice;
        }
        for (int i = 0; i < 6; i++)   // témpanos sueltos alrededor (fuera del NavMesh)
        {
            float a = (float)rnd.NextDouble() * Mathf.PI * 2f, d = R + 6f + 18f * (float)rnd.NextDouble();
            Vector3 p = I + new Vector3(Mathf.Cos(a) * d, PolarLandscapeBuilder.WaterLevel + 0.1f, Mathf.Sin(a) * d);
            if (Vector3.Distance(new Vector3(p.x, 0f, p.z), CvLagoon) < 30f) continue;   // la laguna, libre
            var f = GameObject.CreatePrimitive(PrimitiveType.Cube); f.name = "Tempano";
            f.transform.position = p; f.transform.localScale = new Vector3(2f + 3f * (float)rnd.NextDouble(), 0.4f, 2f + 3f * (float)rnd.NextDouble());
            f.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 180f, 0f);
            f.GetComponent<Renderer>().sharedMaterial = ice;
            PolarLandscapeBuilder.IgnoreForNav(f);
        }

        // ── Hábitat y comida ──
        string food = "";
        if (habitat == CicloVidaPanel.Habitat.Hielo)
        {
            Vector3 bowl = I + new Vector3(4f, 0f, 2f);
            Bowl(island.transform, bowl, "carne");
            food = "carne (cuenco)";
            if (sp == "Bear") Den(island.transform, I + new Vector3(-6f, 0f, 5f), ice, "Guarida de nieve\n(la osa pare en ella; decorado: el juego aún no la usa)", 3.2f);
            else Den(island.transform, I + new Vector3(-6f, 0f, 5f), MakeMaterial("CicloVida_Madera_MAT", new Color(0.45f, 0.32f, 0.2f)), "Caseta\n(decorado: el juego aún no la usa)", 1.4f);
        }
        if (habitat == CicloVidaPanel.Habitat.Costa || habitat == CicloVidaPanel.Habitat.Mar)
        {
            bool shore = habitat == CicloVidaPanel.Habitat.Costa;
            // El banco junto a la orilla (el pingüino pesca desde el borde y bucea hasta él) o en el centro de la laguna.
            Vector3 at = shore ? new Vector3(I.x, -2f, I.z - R - 6f) : new Vector3(CvLagoon.x, -2f, CvLagoon.z + 4f);
            School(at, SwarmKind.Fish, "Fish", new Color(0.35f, 0.55f, 0.65f, 0.6f));
            if (sp == "Penguin" || sp == "Whale") School(at + new Vector3(8f, 0f, -3f), SwarmKind.Krill, "Krill", new Color(0.75f, 0.45f, 0.4f, 0.6f));
            food = sp == "Penguin" || sp == "Whale" ? "peces y krill (bancos inagotables)" : "peces (banco inagotable)";
        }
        GameObject cubeTree = null;
        if (tree)
        {
            // Un árbol carnívoro de hielo con panal (el bosque de SampleScene). Se prueba con semillas hasta que tenga nido.
            for (int seed = 1234; seed < 1234 + 40 && cubeTree == null; seed += 77)
            {
                GameObject t = CubeTree.Build(island.transform, "ArbolCarnivoroHielo", I + new Vector3(0f, 0f, 3f), seed, 16f);
                bool nest = false; foreach (Transform c in t.GetComponentsInChildren<Transform>()) if (c.name == "Nido_Abejas") { nest = true; break; }
                if (nest) cubeTree = t; else Object.DestroyImmediate(t);
            }
            if (cubeTree != null)
            {
                BottomlessFood bf = cubeTree.AddComponent<BottomlessFood>();   // la miel del panal no se acaba
                WorldLabel wl = cubeTree.AddComponent<WorldLabel>(); wl.text = "Árbol carnívoro de hielo con su panal\n(la miel no se acaba)"; wl.height = 20f;
            }
            food = sp == "SnowBee" ? "miel del panal (inagotable)" : "abejas de las nieves (la colonia del panal)";
        }

        // NavMesh solo de la isla (sus hijos: la placa, los bloques, la guarida, el árbol).
        NavMeshSurface surf = island.AddComponent<NavMeshSurface>();
        surf.collectObjects = CollectObjects.Children;
        surf.BuildNavMesh();

        // ── La pareja ──
        var fauna = new GameObject("Fauna");
        Vector3 fPos, mPos;
        if (habitat == CicloVidaPanel.Habitat.Mar) { fPos = new Vector3(CvLagoon.x - 3f, -1f, CvLagoon.z + 8f); mPos = new Vector3(CvLagoon.x + 3f, -1f, CvLagoon.z + 8f); }
        else if (tree) { fPos = I + new Vector3(2f, 6f, 3f); mPos = I + new Vector3(-2f, 6f, 3f); }
        else { fPos = I + new Vector3(-2f, 0.1f, -1f); mPos = I + new Vector3(2f, 0.1f, -1f); }
        Animal female = Adult(prefab, fauna.transform, fPos, Sex.female, display + "_hembra");
        Animal male = Adult(prefab, fauna.transform, mPos, Sex.male, display + "_macho");
        if (sp == "SnowBee")
        {
            Queen(female, true); Queen(male, false);   // la hembra es la REINA; el macho, un zángano (el juego no tiene oficio de zángano)
        }
        if (sp == "SnowSpider")
        {
            // Su comida: una colonia de abejas de las nieves en el panal del árbol (la reina repone las que caza).
            GameObject bee = LoadAnimalPrefab("SnowBee");
            if (bee != null)
                for (int i = 0; i < 4; i++)
                {
                    Animal b = Adult(bee, fauna.transform, I + new Vector3(1f + i * 0.5f, 7f, 3f), Sex.female, "Abeja_nieves_" + (i + 1));
                    Queen(b, i == 0);
                }
        }

        // ── Sistemas: reloj, rastros, panel, pruebas, explorador ──
        var clock = new GameObject("Clock_AUTO").AddComponent<Clock>();
        clock.startHour = 8f; clock.realMinutesPerDay = 60f; clock.daysPerYear = CicloVidaPanel.CompressedYear;
        new GameObject("TraceField_AUTO").AddComponent<TraceField>();
        var panelGo = new GameObject("CicloVida_Panel");
        CicloVidaPanel panel = panelGo.AddComponent<CicloVidaPanel>();
        panel.species = sp; panel.displayName = display;
        new GameObject("TestRunner_AUTO").AddComponent<ColdSanctuary.TestRunner>();
        var label = new GameObject("Rotulo_isla"); label.transform.position = I + new Vector3(0f, 0f, R - 2f);
        WorldLabel il = label.AddComponent<WorldLabel>(); il.height = 5f;
        il.text = $"Ciclo de vida: {display}\nhembra y macho adultos · comida: {food}";
        Vector3 eye = habitat == CicloVidaPanel.Habitat.Mar ? new Vector3(I.x + 6f, 0.1f, I.z - R + 2.5f) : I + new Vector3(0f, 0.1f, -R + 2.5f);
        GalleryExplorer.Spawn(eye, habitat == CicloVidaPanel.Habitat.Mar ? 190f : 0f);

        // Foto de conjunto (Temp/CicloVida/CicloVida_<Especie>.png).
        Vector3 look = habitat == CicloVidaPanel.Habitat.Mar ? (I + CvLagoon) * 0.5f : I;
        CicloShot(look + new Vector3(0f, tree ? 22f : 16f, -(R + 26f)), look + Vector3.up * (tree ? 6f : 0f), $"CicloVida_{sceneName}");

        EditorSceneManager.SaveScene(scene, path);
        if (!list.Exists(s => s.path == path)) list.Add(new EditorBuildSettingsScene(path, true));
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
        Debug.Log($"[CicloVida] {path}: {display} ({sp}), hábitat {habitat}, comida: {food}.");
        return true;
    }

    static Animal Adult(GameObject prefab, Transform parent, Vector3 at, char sex, string name)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name = name;
        go.transform.position = at;
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Animal a = go.GetComponent<Animal>();
        if (a != null)
        {
            a.lifeStage = LifeStage.adult;   // el prefab arranca joven
            a.sex = sex;
            PrefabUtility.RecordPrefabInstancePropertyModifications(a);
        }
        return a;
    }

    static void Queen(Animal a, bool queen)
    {
        if (a == null || !a.TryGetComponent(out IceInsectLife life)) return;
        life.isQueen = queen; life.ageDays = queen ? 30f : 25f;
        PrefabUtility.RecordPrefabInstancePropertyModifications(life);
    }

    // El CUENCO: un cuenco de verdad con una montaña de carne (FoodItem de 2 t que BottomlessFood rellena).
    static void Bowl(Transform parent, Vector3 at, string what)
    {
        var bowl = new GameObject("Cuenco_inagotable"); bowl.transform.SetParent(parent, true); bowl.transform.position = at;
        var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder); rim.name = "Cuenco"; rim.transform.SetParent(bowl.transform, false);
        rim.transform.localScale = new Vector3(2.2f, 0.18f, 2.2f); rim.transform.localPosition = Vector3.up * 0.18f;
        rim.GetComponent<Renderer>().sharedMaterial = MakeMaterial("CicloVida_Cuenco_MAT", new Color(0.55f, 0.6f, 0.68f));
        Object.DestroyImmediate(rim.GetComponent<Collider>());
        var meat = GameObject.CreatePrimitive(PrimitiveType.Sphere); meat.name = "Carne"; meat.transform.SetParent(bowl.transform, false);
        meat.transform.localScale = new Vector3(1.7f, 0.6f, 1.7f); meat.transform.localPosition = Vector3.up * 0.42f;
        meat.GetComponent<Renderer>().sharedMaterial = MakeMaterial("CicloVida_Carne_MAT", new Color(0.62f, 0.2f, 0.2f));
        Object.DestroyImmediate(meat.GetComponent<Collider>());
        FoodItem fi = bowl.AddComponent<FoodItem>(); fi.material = OrganicMaterial.Meat; fi.grams = 2000000f;
        bowl.AddComponent<BottomlessFood>();
        WorldLabel wl = bowl.AddComponent<WorldLabel>(); wl.text = $"Cuenco inagotable: {what}"; wl.height = 1.6f;
    }

    // Un banco inagotable en el agua (Swarm + BottomlessFood que lo rellena y no lo deja alejarse).
    static void School(Vector3 at, SwarmKind kind, string unit, Color c)
    {
        var school = GameObject.CreatePrimitive(PrimitiveType.Cube);
        school.name = $"Banco_inagotable_{unit}";
        PolarLandscapeBuilder.IgnoreForNav(school);
        school.transform.position = at;
        school.transform.localScale = kind == SwarmKind.Fish ? new Vector3(8f, 2f, 8f) : new Vector3(6f, 1.5f, 6f);
        school.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"CicloVida_{unit}_MAT", c);
        Object.DestroyImmediate(school.GetComponent<Collider>());
        Swarm s = school.AddComponent<Swarm>();
        s.kind = kind; s.unitName = unit; s.count = 60f; s.maxCount = 60f; s.wanderRadius = 5f;
        if (kind == SwarmKind.Krill) { s.schoolSpread = 3f; s.perUnitMass = 0.05f; }
        school.AddComponent<BottomlessFood>().leash = 6f;
        WorldLabel wl = school.AddComponent<WorldLabel>(); wl.text = kind == SwarmKind.Fish ? "Banco de peces inagotable" : "Krill inagotable"; wl.height = 3.5f;
    }

    // Guarida/caseta: tres bloques en arco (techo y dos paredes), abierta por delante.
    static void Den(Transform parent, Vector3 at, Material m, string text, float size)
    {
        var den = new GameObject("Guarida"); den.transform.SetParent(parent, true); den.transform.position = at;
        void Part(Vector3 p, Vector3 s) { var b = GameObject.CreatePrimitive(PrimitiveType.Cube); b.name = "Guarida_pieza"; b.transform.SetParent(den.transform, false); b.transform.localPosition = p; b.transform.localScale = s; b.GetComponent<Renderer>().sharedMaterial = m; }
        Part(new Vector3(-size * 0.6f, size * 0.4f, 0f), new Vector3(size * 0.25f, size * 0.8f, size * 1.3f));
        Part(new Vector3(size * 0.6f, size * 0.4f, 0f), new Vector3(size * 0.25f, size * 0.8f, size * 1.3f));
        Part(new Vector3(0f, size * 0.9f, 0f), new Vector3(size * 1.5f, size * 0.25f, size * 1.4f));
        Part(new Vector3(0f, size * 0.4f, size * 0.6f), new Vector3(size * 1.2f, size * 0.8f, size * 0.2f));
        WorldLabel wl = den.AddComponent<WorldLabel>(); wl.text = text; wl.height = size + 1f;
    }

    static void CicloShot(Vector3 from, Vector3 to, string file)
    {
        var camGo = new GameObject("Camara_foto");
        Camera cam = camGo.AddComponent<Camera>(); cam.fieldOfView = 50f; cam.farClipPlane = 1500f;
        camGo.transform.position = from; camGo.transform.LookAt(to);
        var rt = new RenderTexture(1600, 900, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); tex.Apply();
        string dir = Path.Combine(Application.dataPath, "..", "Temp", "CicloVida"); Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, file + ".png"), tex.EncodeToPNG());
        RenderTexture.active = null; cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
        Object.DestroyImmediate(camGo);
    }
}
