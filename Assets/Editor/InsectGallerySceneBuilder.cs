using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GALERÍA DE INSECTOS-HUMANOS (Berón, 2026-10-23/24: «una escena donde se cargan todos en postura erguida y normal…
/// esperaba ver todos los nuevos de cada era»). Escena propia, Assets/Scenes/Galeria_InsectosHumanos.unity, ordenada por
/// ERA Y LUGAR: primero el santuario de hoy (las especies base, el mirmidón y Ambrosio) y luego cada microcosmos, del más
/// antiguo al más reciente (Eras.OfMicrocosmos), con las especies NATIVAS de su bioma (MicroBiome: su nombre real, su
/// color y su tamaño). Cada especie, en una columna:
/// - fila 1, ERGUIDOS (lúcidos: centauro con brazos y cara): la PAREJA, hembra y macho (InsectMorphology.ForSex);
/// - fila 2, en POSTURA ANIMAL (a todas las patas): uno;
/// - fila 3, su CICLO DE VIDA: larva → pupa → adulto (metamorfosis completa) o ninfa → adulto (incompleta), cada fase con
///   su TAMAÑO RELATIVO al adulto (InsectMorphology.PhaseScale: la larva de mariquita es mayor que el adulto, la de la
///   hormiga león mucho menor…).
/// Las especies nuevas que se añadan a MicroBiome aparecen solas. Se recorre con el explorador (GalleryExplorer). Deja una
/// captura por grupo en Temp/PoseGallery/Galeria_InsectosHumanos_N.png. Se monta lejos del mundo (5000, 0, −5000).
/// </summary>
public static class InsectGallerySceneBuilder
{
    const string ScenePath = "Assets/Scenes/Galeria_InsectosHumanos.unity";
    const float Length = 2.4f, Step = 3.6f, Column = 3f * Step + 2f, RowGap = 7f, GroupGap = 3f * RowGap + 10f;
    static readonly Vector3 Origin = new Vector3(5000f, 0f, -5000f);

    enum Dev { Complete, Incomplete, None }

    static Dev DevOf(string species)
    {
        switch (InsectMorphology.For(species).name)
        {
            case "Ant": case "Ladybug": case "DungBeetle": case "Antlion": case "SnowBee": return Dev.Complete;
            case "Spider": return Dev.None;
            default: return Dev.Incomplete;
        }
    }

    // El santuario de hoy: las especies base del juego.
    static readonly (string species, string label, Color tint)[] Base =
    {
        ("Ant", "Hormiga (obrera ♀ / macho alado ♂)", new Color(0.4f, 0.25f, 0.18f)), ("Aphid", "Pulgón (♀ áptera / ♂ alado)", new Color(0.62f, 0.75f, 0.4f)),
        ("Ladybug", "Mariquita", new Color(0.8f, 0.15f, 0.12f)), ("DungBeetle", "Escarabajo pelotero (♂ con cuerno)", new Color(0.2f, 0.18f, 0.16f)),
        ("Grasshopper", "Saltamontes", new Color(0.45f, 0.62f, 0.25f)), ("Cricket", "Grillo (♀ con oviscapto)", new Color(0.3f, 0.22f, 0.15f)),
        ("Mantis", "Mantis (4 brazos)", new Color(0.4f, 0.68f, 0.3f)), ("Antlion", "Hormiga león", new Color(0.55f, 0.48f, 0.38f)),
        ("Termite", "Termita (obreros ♀ y ♂ iguales)", new Color(0.9f, 0.85f, 0.72f)), ("SnowBee", "Abeja de hielo (obrera / zángano)", new Color(0.8f, 0.88f, 0.95f)),
        ("Spider", "Araña lobo (♂ con bulbos)", new Color(0.35f, 0.3f, 0.28f)),
    };

    [MenuItem("Tools/Cold Sanctuary/Build Galería de insectos-humanos")]
    public static void Build()
    {
        MicrocosmosAreaSceneBuilder.DropUntitled();
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        var root = new GameObject("Galeria").transform;
        var sun = new GameObject("Sol").AddComponent<Light>(); sun.type = LightType.Directional; sun.transform.SetParent(root, false); sun.transform.rotation = Quaternion.Euler(45f, 150f, 0f);

        // Los grupos: el santuario de hoy y cada microcosmos con bioma, por su fecha.
        var groups = new List<(string title, List<(string sp, string label, Color tint, bool myrmidon, bool aphid)> cast)>();
        var today = new List<(string, string, Color, bool, bool)>();
        foreach (var (sp, label, tint) in Base) today.Add((sp, label, tint, false, false));
        today.Add(("Ant", "Mirmidón (hormiga humana)", new Color(0.4f, 0.25f, 0.18f), true, false));
        today.Add(("Aphid", "Ambrosio (pulgón)", new Color(0.62f, 0.75f, 0.4f), true, true));
        groups.Add(("El santuario, hoy (especies base)", today));
        var eras = new List<(int year, string title, List<(string, string, Color, bool, bool)> cast)>();
        var seen = new HashSet<string>();
        foreach (var (_, display, _) in EraHubSceneBuilder.All)
        {
            MicroBiome.Biome bio = MicroBiome.For(display);
            if (bio == null || !seen.Add(bio.title)) continue;   // un grupo por bioma (Cocina y Hospital comparten la taiga)
            var e = Eras.OfMicrocosmos(display);
            var cast = new List<(string, string, Color, bool, bool)>();
            foreach (MicroBiome.Bug b in bio.bugs) cast.Add((b.species, b.name, b.tint, false, false));
            eras.Add((e.HasValue ? e.Value.year : Eras.Present, $"{(e.HasValue ? Eras.Format(e.Value.year) + " · " : "")}{bio.title}", cast));
        }
        eras.Sort((a, b) => a.year.CompareTo(b.year));
        foreach (var e in eras) groups.Add((e.title, e.cast));

        int maxCols = 0, species = 0;
        for (int gi = 0; gi < groups.Count; gi++)
        {
            float gz = gi * GroupGap;
            var (title, cast) = groups[gi];
            maxCols = Mathf.Max(maxCols, cast.Count);
            Label(root, title, new Vector3(-4f, 5.2f, gz - 2f), 0.2f, TextAnchor.MiddleLeft);
            for (int i = 0; i < cast.Count; i++, species++) Column3(root, cast[i].sp, cast[i].label, cast[i].tint, i * Column, gz, cast[i].myrmidon, cast[i].aphid);
        }
        float width = Mathf.Max(1, maxCols - 1) * Column, depth = (groups.Count - 1) * GroupGap + 2f * RowGap;
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.name = "Suelo"; floor.transform.SetParent(root, false);
        floor.transform.localPosition = new Vector3(width * 0.5f, -0.05f, depth * 0.5f); floor.transform.localScale = new Vector3(width + 20f, 0.1f, depth + 24f);
        floor.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.55f, 0.5f, 0.42f));
        root.position = Origin;

        // Capturas: una por grupo (hasta 6 especies por imagen), de frente.
        var camGo = new GameObject("Camara_captura");
        Camera cam = camGo.AddComponent<Camera>(); cam.fieldOfView = 45f; cam.farClipPlane = 600f;
        int shot = 0;
        for (int gi = 0; gi < groups.Count; gi++)
            for (int c0 = 0; c0 < groups[gi].cast.Count; c0 += 6)
            {
                int n = Mathf.Min(6, groups[gi].cast.Count - c0);
                float cx = Origin.x + (c0 + (n - 1) * 0.5f) * Column + Step * 0.5f, gz = Origin.z + gi * GroupGap;
                camGo.transform.position = new Vector3(cx, 14f + n * 2f, gz - 18f - n * 3f);
                camGo.transform.LookAt(new Vector3(cx, 0f, gz + RowGap));
                Snapshot(cam, ++shot);
            }
        Object.DestroyImmediate(camGo);
        GalleryExplorer.Spawn(Origin + new Vector3(Step, 0.1f, -8f), 0f);

        EditorSceneManager.SaveScene(scene, ScenePath);
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!list.Exists(s => s.path == ScenePath)) { list.Add(new EditorBuildSettingsScene(ScenePath, true)); EditorBuildSettings.scenes = list.ToArray(); AssetDatabase.SaveAssets(); }
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
        Debug.Log($"[Galería] {groups.Count} grupos (hoy + {groups.Count - 1} eras), {species} especies: pareja erguida, postura animal y ciclo de vida a escala → {ScenePath} ({shot} capturas en Temp/PoseGallery/Galeria_InsectosHumanos_N.png).");
    }

    static void Column3(Transform root, string sp, string label, Color tint, float x, float z0, bool myrmidon, bool aphid)
    {
        Label(root, label, new Vector3(x + Step * 0.5f, 3.6f, z0 - 1.5f), 0.11f);
        // Fila 1: la pareja, erguida.
        Make(root, sp, tint, "♀ erguida", new Vector3(x, 0f, z0), true, true, Metamorphosis.Phase.Adult, myrmidon, aphid);
        if (!myrmidon) Make(root, sp, tint, "♂ erguido", new Vector3(x + Step, 0f, z0), true, false, Metamorphosis.Phase.Adult, false, false);
        // Fila 2: postura animal.
        Make(root, sp, tint, "animal", new Vector3(x + Step * 0.5f, 0f, z0 + RowGap), false, true, Metamorphosis.Phase.Adult, myrmidon, aphid);
        // Fila 3: el ciclo de vida, a escala.
        Dev dev = myrmidon ? Dev.None : DevOf(sp);
        Metamorphosis.Phase[] stages = dev == Dev.Complete ? new[] { Metamorphosis.Phase.Larva, Metamorphosis.Phase.Pupa, Metamorphosis.Phase.Adult }
                                     : dev == Dev.Incomplete ? new[] { Metamorphosis.Phase.Nymph, Metamorphosis.Phase.Adult } : new Metamorphosis.Phase[0];
        for (int k = 0; k < stages.Length; k++)
        {
            Vector3 at = new Vector3(x + k * Step * 0.75f, 0f, z0 + 2f * RowGap);
            Make(root, sp, tint, StageName(stages[k], sp), at, false, true, stages[k], false, false);
            Label(root, $"{StageName(stages[k], sp)} ×{InsectMorphology.PhaseScale(sp, stages[k]):0.#}", at + new Vector3(0f, 2.2f, -1.2f), 0.08f);
        }
    }

    static string StageName(Metamorphosis.Phase p, string sp) =>
        p == Metamorphosis.Phase.Larva ? (InsectMorphology.For(sp).name == "Antlion" ? "larva (cazadora)" : "larva") : p == Metamorphosis.Phase.Pupa ? "pupa" :
        p == Metamorphosis.Phase.Nymph ? "ninfa" : (InsectMorphology.For(sp).name == "Antlion" ? "adulto alado" : "adulto");

    static void Make(Transform root, string sp, Color tint, string what, Vector3 at, bool upright, bool female, Metamorphosis.Phase phase, bool myrmidon, bool aphid)
    {
        var host = new GameObject($"{sp} · {what}"); host.transform.SetParent(root, false);
        host.transform.localPosition = at; host.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);   // de cara a la cámara (−Z)
        float len = Length * InsectMorphology.PhaseScale(sp, phase);
        CubeInsect ci = myrmidon ? CubeInsect.DressMyrmidon(host, tint, Length / 1.35f, aphid ? 1.6f : 1f, aphid)
                                 : CubeInsect.Dress(host, sp, tint, len, phase, female);
        if (phase == Metamorphosis.Phase.Adult || phase == Metamorphosis.Phase.Nymph) ci.PoseNow(upright);
    }

    static void Label(Transform root, string text, Vector3 at, float size, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        var go = new GameObject("Nombre"); go.transform.SetParent(root, false); go.transform.localPosition = at;   // el texto mira a −Z, hacia la cámara
        TextMesh tm = go.AddComponent<TextMesh>(); tm.text = text; tm.anchor = anchor; tm.characterSize = size; tm.fontSize = 40; tm.color = Color.black;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (tm.font != null) go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
    }

    static void Snapshot(Camera cam, int part)
    {
        var rt = new RenderTexture(1800, 1000, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var tex = new Texture2D(1800, 1000, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1800, 1000), 0, 0); tex.Apply();
        string dir = Path.Combine(Application.dataPath, "..", "Temp", "PoseGallery"); Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, $"Galeria_InsectosHumanos_{part}.png"), tex.EncodeToPNG());
        RenderTexture.active = null; cam.targetTexture = null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
    }
}
