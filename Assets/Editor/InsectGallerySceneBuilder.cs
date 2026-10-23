using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GALERÍA DE INSECTOS-HUMANOS (Berón, 2026-10-23: «una escena donde se cargan todos en postura erguida y normal, así
/// solo con entrar allí puedo validarlos todos»). Escena propia, Assets/Scenes/Galeria_InsectosHumanos.unity, con una
/// columna por especie (InsectMorphology) más el mirmidón y Ambrosio:
/// - fila 1, ERGUIDOS (lúcidos: centauro con brazos y cara): la PAREJA, hembra y macho (InsectMorphology.ForSex);
/// - fila 2, en POSTURA ANIMAL (a todas las patas): uno;
/// - fila 3, su CICLO DE VIDA: larva → pupa → adulto (metamorfosis completa) o ninfa → adulto (incompleta); la hormiga
///   león, de la larva cazadora al adulto alado.
/// Todos del mismo largo y con su nombre. La postura queda fijada también en Play (CubeInsect.PoseNow). Deja una captura
/// en Temp/PoseGallery/Galeria_InsectosHumanos_N.png (de 4 en 4 especies). Se monta lejos del mundo (5000, 0, −5000).
/// </summary>
public static class InsectGallerySceneBuilder
{
    const string ScenePath = "Assets/Scenes/Galeria_InsectosHumanos.unity";
    const float Length = 2.4f, Step = 3.6f, Column = 3f * Step + 2f, RowGap = 7f;

    enum Dev { Complete, Incomplete, None }

    static readonly (string species, string label, Dev dev)[] Cast =
    {
        ("Ant", "Hormiga (obrera ♀ / macho alado ♂)", Dev.Complete), ("Aphid", "Pulgón (♀ áptera / ♂ alado)", Dev.Incomplete),
        ("Ladybug", "Mariquita", Dev.Complete), ("DungBeetle", "Escarabajo pelotero (♂ con cuerno)", Dev.Complete),
        ("Grasshopper", "Saltamontes", Dev.Incomplete), ("Cricket", "Grillo (♀ con oviscapto)", Dev.Incomplete),
        ("Mantis", "Mantis (4 brazos)", Dev.Incomplete), ("Antlion", "Hormiga león", Dev.Complete),
        ("Termite", "Termita (obreros ♀ y ♂ iguales)", Dev.Incomplete), ("SnowBee", "Abeja de hielo (obrera / zángano)", Dev.Complete),
        ("Spider", "Araña lobo (♂ con bulbos)", Dev.None),
    };

    [MenuItem("Tools/Cold Sanctuary/Build Galería de insectos-humanos")]
    public static void Build()
    {
        MicrocosmosAreaSceneBuilder.DropUntitled();
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        var sun = new GameObject("Sol").AddComponent<Light>(); sun.type = LightType.Directional; sun.transform.rotation = Quaternion.Euler(45f, 150f, 0f);
        int n = Cast.Length + 2;
        float width = (n - 1) * Column;
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.name = "Suelo";
        floor.transform.position = new Vector3(width * 0.5f, -0.05f, RowGap); floor.transform.localScale = new Vector3(width + 16f, 0.1f, RowGap * 2f + 16f);
        floor.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.55f, 0.5f, 0.42f));

        int i = 0;
        foreach (var (sp, label, dev) in Cast) Column3(sp, label, dev, i++ * Column, false, false);
        Column3("Ant", "Mirmidón (hormiga humana)", Dev.None, i++ * Column, true, false);
        Column3("Aphid", "Ambrosio (pulgón)", Dev.None, i * Column, true, true);

        // Todo bajo una raíz LEJOS del mundo (al construirla, la escena principal sigue cargada y saldría en la captura).
        var root = new GameObject("Galeria").transform;
        foreach (GameObject g in scene.GetRootGameObjects()) if (g.transform != root) g.transform.SetParent(root, true);
        root.position = Origin;
        // Capturas: de frente, de 4 en 4 especies, para ver bien las tres filas (cámara temporal).
        var camGo = new GameObject("Camara_captura");
        Camera cam = camGo.AddComponent<Camera>(); cam.fieldOfView = 45f; cam.farClipPlane = 600f;
        for (int g = 0; g * 4 < n; g++)
        {
            float cx = Origin.x + (g * 4 + 1.5f) * Column + Step * 0.5f;
            camGo.transform.position = new Vector3(cx, 16f, Origin.z - 20f);
            camGo.transform.LookAt(new Vector3(cx, 0f, Origin.z + RowGap));
            Snapshot(cam, g + 1);
        }
        Object.DestroyImmediate(camGo);
        // El EXPLORADOR (anda y vuela por la galería): delante de la fila de las parejas, mirándolas.
        GalleryExplorer.Spawn(Origin + new Vector3(Step, 0.1f, -8f), 0f).transform.SetParent(null);

        EditorSceneManager.SaveScene(scene, ScenePath);
        // En la lista de escenas (Build Settings), para encontrarla en el selector.
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!list.Exists(s => s.path == ScenePath)) { list.Add(new EditorBuildSettingsScene(ScenePath, true)); EditorBuildSettings.scenes = list.ToArray(); }
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
        Debug.Log($"[Galería] {n} especies: pareja erguida (♀ y ♂), postura animal y ciclo de vida → {ScenePath} (capturas en Temp/PoseGallery/Galeria_InsectosHumanos_N.png).");
    }

    static void Column3(string sp, string label, Dev dev, float x, bool myrmidon, bool aphid)
    {
        Label(label, new Vector3(x + Step * 0.5f, 3.6f, 0f), 0.13f);
        // Fila 1: la pareja, erguida.
        Make(sp, "♀ erguida", new Vector3(x, 0f, 0f), true, true, Metamorphosis.Phase.Adult, myrmidon, aphid);
        if (!myrmidon) Make(sp, "♂ erguido", new Vector3(x + Step, 0f, 0f), true, false, Metamorphosis.Phase.Adult, false, false);
        // Fila 2: postura animal.
        Make(sp, "animal", new Vector3(x + Step * 0.5f, 0f, RowGap), false, true, Metamorphosis.Phase.Adult, myrmidon, aphid);
        // Fila 3: el ciclo de vida.
        Metamorphosis.Phase[] stages = dev == Dev.Complete ? new[] { Metamorphosis.Phase.Larva, Metamorphosis.Phase.Pupa, Metamorphosis.Phase.Adult }
                                     : dev == Dev.Incomplete ? new[] { Metamorphosis.Phase.Nymph, Metamorphosis.Phase.Adult } : new Metamorphosis.Phase[0];
        for (int k = 0; k < stages.Length; k++)
        {
            Vector3 at = new Vector3(x + k * Step * 0.75f, 0f, 2f * RowGap);
            Make(sp, StageName(stages[k], sp), at, false, true, stages[k], false, false);
            Label(StageName(stages[k], sp), at + new Vector3(0f, 2.2f, 0f), 0.09f);
        }
    }

    static string StageName(Metamorphosis.Phase p, string sp) =>
        p == Metamorphosis.Phase.Larva ? (sp == "Antlion" ? "larva (cazadora)" : "larva") : p == Metamorphosis.Phase.Pupa ? "pupa" :
        p == Metamorphosis.Phase.Nymph ? "ninfa" : (sp == "Antlion" ? "adulto alado" : "adulto");

    static void Make(string sp, string what, Vector3 at, bool upright, bool female, Metamorphosis.Phase phase, bool myrmidon, bool aphid)
    {
        var host = new GameObject($"{sp} · {what}");
        host.transform.position = at; host.transform.rotation = Quaternion.Euler(0f, 180f, 0f);   // de cara a la cámara (−Z)
        CubeInsect ci = myrmidon ? CubeInsect.DressMyrmidon(host, aphid ? new Color(0.62f, 0.75f, 0.4f) : new Color(0.4f, 0.25f, 0.18f), Length / 1.35f, aphid ? 1.6f : 1f, aphid)
                                 : CubeInsect.Dress(host, sp, Tint(sp), Length, phase, female);
        if (phase == Metamorphosis.Phase.Adult || phase == Metamorphosis.Phase.Nymph) ci.PoseNow(upright);
    }

    static void Label(string text, Vector3 at, float size)
    {
        var go = new GameObject("Nombre"); go.transform.position = at;   // el texto mira a −Z, hacia la cámara
        TextMesh tm = go.AddComponent<TextMesh>(); tm.text = text; tm.anchor = TextAnchor.MiddleCenter; tm.characterSize = size; tm.fontSize = 40; tm.color = Color.black;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (tm.font != null) go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
    }

    static Color Tint(string sp)
    {
        switch (sp)
        {
            case "Ant": return new Color(0.4f, 0.25f, 0.18f);
            case "Aphid": return new Color(0.62f, 0.75f, 0.4f);
            case "Ladybug": return new Color(0.8f, 0.15f, 0.12f);
            case "DungBeetle": return new Color(0.2f, 0.18f, 0.16f);
            case "Grasshopper": return new Color(0.45f, 0.62f, 0.25f);
            case "Cricket": return new Color(0.3f, 0.22f, 0.15f);
            case "Mantis": return new Color(0.4f, 0.68f, 0.3f);
            case "Antlion": return new Color(0.55f, 0.48f, 0.38f);
            case "Termite": return new Color(0.9f, 0.85f, 0.72f);
            case "SnowBee": return new Color(0.8f, 0.88f, 0.95f);
            default: return new Color(0.35f, 0.3f, 0.28f);
        }
    }

    static readonly Vector3 Origin = new Vector3(5000f, 0f, -5000f);

    static void Snapshot(Camera cam, int part)
    {
        var rt = new RenderTexture(1800, 1000, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var tex = new Texture2D(1800, 1000, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1800, 1000), 0, 0); tex.Apply();
        string dir = Path.Combine(Application.dataPath, "..", "Temp", "PoseGallery"); Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, $"Galeria_InsectosHumanos_{part}.png"), tex.EncodeToPNG());
        RenderTexture.active = null; cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
    }
}
