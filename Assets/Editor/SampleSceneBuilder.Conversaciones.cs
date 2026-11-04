using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ESCENA DE PRUEBA DE LAS CHARLAS (Assets/Scenes/Pruebas/Prueba_Conversaciones; docs/conversaciones.md §Escena de prueba; Berón
/// 2026-11-04: «también debería haber una escena específica creada para poder probar esta lógica al detalle»). Como las de ciclo de
/// vida de una persona (SampleSceneBuilder.CicloHumano): una escena PROPIA y pequeña, sin copiar el santuario (no abre ni poda
/// SampleScene: poca memoria), lejos del origen. Una sala de charlas: suelo, una mesa larga y dos bancos, y seis personas sentadas
/// por parejas frente a frente:
///   · Elena y Tomás (humanos, pareja con dos hijos) en la cabecera oeste,
///   · Rosa (humana) y Ciro (insecto-humano, hormiga) en el centro: la pareja mixta,
///   · Teodora (mariquita) y Bruno (araña), insectos-humanos de especies distintas, en la cabecera este.
/// Con el reloj (día de 1 h real), el director de las charlas, el contexto (lugar, época e historia del sitio), los nombres y los
/// bocadillos, el explorador y el orquestador de pruebas (solo el grupo 88, <see cref="ConversationTest"/>). Foto en Temp/Conversaciones/.
/// </summary>
public static partial class SampleSceneBuilder
{
    [MenuItem("Tools/Cold Sanctuary/Build Prueba_Conversaciones (charlas entre personas)")]
    public static void BuildConversationScene()
    {
        MicrocosmosAreaSceneBuilder.DropUntitled();
        if (!AssetDatabase.IsValidFolder(TestScenesFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Pruebas");
        string path = $"{TestScenesFolder}/{ConversationTest.Scene}.unity";
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            string what = BuildConversationRoom(scene);
            EditorSceneManager.SaveScene(scene, path);
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!list.Exists(s => s.path == path)) list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Charlas] {path}: {what} (foto en Temp/Conversaciones/).");
        }
        catch (System.Exception e) { Debug.LogError($"[Charlas] {ConversationTest.Scene} falló: {e}"); }
        finally
        {
            EditorSceneManager.CloseScene(scene, removeScene: true);
            if (prev.IsValid()) SceneManager.SetActiveScene(prev);
        }
    }

    static string BuildConversationRoom(Scene scene)
    {
        var sun = new GameObject("Sol").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.1f; sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        RenderSettings.fog = false;
        RenderSettings.ambientLight = new Color(0.64f, 0.68f, 0.74f);

        // ── La sala: suelo, una mesa larga (9 × 1 m, 0,75 m de alto) y dos bancos (0,45 m) a lo largo ──
        Vector3 O = new Vector3(-3000f, 0f, -3400f);   // lejos del santuario (el constructor la abre en aditivo encima)
        var room = new GameObject("Sala_de_charlas"); room.transform.position = O;
        Material floorM = MakeMaterial("Charlas_Suelo_MAT", new Color(0.78f, 0.8f, 0.82f));
        Material woodM = MakeMaterial("Charlas_Madera_MAT", new Color(0.55f, 0.38f, 0.24f));
        ConvBox(room.transform, "Suelo", new Vector3(0f, -0.1f, 0f), new Vector3(18f, 0.2f, 12f), floorM);
        Transform table = ConvBox(room.transform, "Mesa", new Vector3(0f, 0.72f, 0f), new Vector3(9f, 0.06f, 1f), woodM);
        foreach (float x in new[] { -4.2f, 4.2f }) foreach (float z in new[] { -0.4f, 0.4f })
            ConvBox(room.transform, "Pata_mesa", new Vector3(x, 0.35f, z), new Vector3(0.08f, 0.7f, 0.08f), woodM);
        foreach (float z in new[] { -0.95f, 0.95f })
        {
            ConvBox(room.transform, z < 0f ? "Banco_sur" : "Banco_norte", new Vector3(0f, 0.43f, z), new Vector3(9f, 0.05f, 0.36f), woodM);
            foreach (float x in new[] { -4.2f, 0f, 4.2f }) ConvBox(room.transform, "Pata_banco", new Vector3(x, 0.2f, z), new Vector3(0.08f, 0.4f, 0.3f), woodM);
        }
        WorldLabel tl = table.gameObject.AddComponent<WorldLabel>(); tl.text = "Mesa de las charlas"; tl.height = 0.4f;

        // ── Las seis personas por parejas frente a frente (1,9–2,8 m), a 3,5 m de la pareja de al lado: los humanos sentados en el banco, los insectos-humanos (sin pose de sentado) de pie detrás de él ──
        var people = new GameObject("Personas"); people.transform.position = O;
        Vector3 Seat(float x, float z) => O + new Vector3(x, 0f, z);
        GameObject elena = Person(people.transform, "Elena", Seat(-3.5f, -0.95f), 1.68f, true, new Color(0.55f, 0.35f, 0.5f));
        GameObject tomas = Person(people.transform, "Tomás", Seat(-3.5f, 0.95f), 1.8f, false, new Color(0.3f, 0.4f, 0.55f));
        GameObject rosa = Person(people.transform, "Rosa", Seat(0f, -0.95f), 1.7f, true, new Color(0.7f, 0.45f, 0.3f));
        GameObject ciro = ConvFolk(people.transform, "Ciro", Seat(0f, 1.4f) - O, "Ant", false, new[] { ("madre", "Ant"), ("padre", "Cricket"), ("abuela", "Ant"), ("abuelo", "Termite") });
        GameObject teodora = ConvFolk(people.transform, "Teodora", Seat(3.5f, -1.4f) - O, "Ladybug", true, new[] { ("madre", "Ladybug"), ("padre", "Firefly"), ("abuela", "Ladybug"), ("abuelo", "Grasshopper") });
        GameObject bruno = ConvFolk(people.transform, "Bruno", Seat(3.5f, 1.4f) - O, "Spider", false, new[] { ("madre", "Spider"), ("padre", "Mantis"), ("abuela", "Earwig"), ("abuelo", "Spider") });
        var all = new[] { elena, tomas, rosa, ciro, teodora, bruno };
        foreach (GameObject g in all)
        {
            PersonLife pl = g.GetComponent<PersonLife>();
            pl.freeLife = true; pl.maxChildren = 0;   // sin embarazos ni partos en la sala
            Vector3 look = new Vector3(g.transform.position.x, 0f, O.z) - new Vector3(g.transform.position.x, 0f, g.transform.position.z);
            g.transform.rotation = Quaternion.LookRotation(look.sqrMagnitude > 1e-4f ? look : Vector3.forward);   // mirando a la mesa
        }
        // Elena y Tomás son pareja con dos hijos (tema «familia»).
        PersonLife pe = elena.GetComponent<PersonLife>(), pt = tomas.GetComponent<PersonLife>();
        pe.partner = pt; pt.partner = pe; pe.courtship = pt.courtship = 1f; pe.children = pt.children = 2;

        // ── Sistemas ──
        var director = new GameObject("ConversationDirector").AddComponent<ConversationDirector>();
        director.maxConcurrent = 4;
        var ctx = new GameObject("Contexto_de_las_charlas").AddComponent<ConversationContext>();
        ctx.place = "la sala de charlas"; ctx.era = "hoy, en el santuario helado";
        ctx.story = new[]
        {
            "Dicen que la primera mesa del santuario la hizo Goluis con madera que trajo el mar.",
            "Cuentan que en el microcosmos de Kapilavastu los insectos-humanos comen todos juntos al mediodía.",
            "Hubo un invierno en que el hielo llegó hasta la puerta del árbol y nadie dejó de trabajar.",
            "Los mayores dicen que una charla a tiempo cura más que un remedio.",
        };
        var sandbox = new GameObject("Charlas_Sandbox").AddComponent<ConversationSandbox>();
        var cast = new List<Anima>(); foreach (GameObject g in all) cast.Add(g.GetComponent<Anima>());
        sandbox.cast = cast.ToArray();
        var clock = new GameObject("Clock_AUTO").AddComponent<Clock>();
        clock.startHour = 12f; clock.realMinutesPerDay = 60f;
        new GameObject("AnimaNameplate_AUTO").AddComponent<AnimaNameplate>();
        var tr = new GameObject("TestRunner_AUTO").AddComponent<ColdSanctuary.TestRunner>();
        tr.onlyGroups = new[] { 88 };
        tr.gameObject.AddComponent<ConversationTest>();
        var sign = new GameObject("Rotulo"); sign.transform.position = O + new Vector3(0f, 0f, 4.5f);
        WorldLabel wl = sign.AddComponent<WorldLabel>(); wl.height = 3.2f;
        wl.text = "Charlas entre personas\nhumano-humano (oeste) · humano-insecto (centro) · insecto-insecto (este)";

        NavMeshSurface surf = room.AddComponent<NavMeshSurface>();
        surf.collectObjects = CollectObjects.Children;
        surf.BuildNavMesh();
        GalleryExplorer.Spawn(O + new Vector3(0f, 0.1f, -6.5f), 0f);

        ConversationShot(O + new Vector3(0f, 4.2f, -7.5f), O + new Vector3(0f, 0.8f, 0f), ConversationTest.Scene, scene);
        return "sala con mesa y dos bancos; Elena y Tomás (humanos), Rosa (humana) y Ciro (hormiga), Teodora (mariquita) y Bruno (araña)";
    }

    // Un insecto-humano de la sala: su especie, su sexo y su árbol genealógico (madre, padre, abuelos).
    static GameObject ConvFolk(Transform parent, string name, Vector3 localPos, string species, bool female, (string who, string sp)[] ancestors)
    {
        GameObject g = AreaMicrocosmos.Folk(parent, name, localPos, female ? 1.7f : 1.76f, 1f, species, null, female);
        CubeInsect.WearShirt(g, Color.HSVToRGB(Mathf.Abs(name.GetHashCode() % 100) / 100f, 0.35f, 0.75f));
        var anc = new List<InsectGenealogy.Ancestor>();
        foreach (var x in ancestors) anc.Add(new InsectGenealogy.Ancestor(x.who, x.sp, x.who == "madre" || x.who == "padre" ? 1 : 2));
        g.GetComponent<FolkLineage>().ancestors = anc;
        return g;
    }

    static Transform ConvBox(Transform parent, string name, Vector3 local, Vector3 size, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name;
        g.transform.SetParent(parent, false); g.transform.localPosition = local; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
        if (name != "Suelo") g.AddComponent<NavMeshModifier>().ignoreFromBuild = true;   // los muebles no recortan el NavMesh: nadie se recoloca al arrancar
        return g.transform;
    }

    /// <summary>Una foto 1600 × 900 de la sala en Temp/Conversaciones/<paramref name="file"/>.png.</summary>
    static void ConversationShot(Vector3 from, Vector3 to, string file, Scene only)
    {
        var camGo = new GameObject("Camara_foto");
        Camera cam = camGo.AddComponent<Camera>(); cam.fieldOfView = 50f; cam.farClipPlane = 500f;
        SceneManager.MoveGameObjectToScene(camGo, only); cam.scene = only;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.62f, 0.75f, 0.88f);
        camGo.transform.position = from; camGo.transform.LookAt(to);
        var rt = new RenderTexture(1600, 900, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); tex.Apply();
        string dir = Path.Combine(Application.dataPath, "..", "Temp", "Conversaciones"); Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, file + ".png"), tex.EncodeToPNG());
        RenderTexture.active = null; cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
        Object.DestroyImmediate(camGo);
    }
}
