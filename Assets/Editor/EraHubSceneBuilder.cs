using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SALA DE ERAS (Berón, 2026-10-23: «una escena que tenga cada área y era existentes y pobladas, así puedo ir allí y
/// verificar cada área/era»). Assets/Scenes/Sala_de_Eras.unity: una fila de PORTALES, uno por microcosmos, ordenados del
/// más antiguo al más reciente (Eras.OfMicrocosmos), cada uno con su año, su lugar, su bioma y su historia. Se recorre con
/// el explorador (GalleryExplorer); al cruzar un portal se entra en ese microcosmos y su portal de salida devuelve aquí.
/// </summary>
public static class EraHubSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Sala_de_Eras.unity";
    static readonly Vector3 Origin = new Vector3(-5000f, 0f, 5000f);

    // (escena, nombre del microcosmos en Eras/MicroBiome, historia)
    static readonly (string scene, string display, string story)[] All =
    {
        ("Microcosmos_Hospital", "Hospital", "Ambrosio, antes del fuego"),
        ("Microcosmos_Cocina", "Cocina", "la era del fuego: las reencarnaciones"),
        ("Microcosmos_SalaYoga", "Sala de Yoga", "una tribu (Sala de Yoga)"),
        ("Microcosmos_Textil", "Estudio Textil", "una tribu (Estudio Textil)"),
        ("Microcosmos_Mecanica", "Mecánica", "una tribu (Mecánica)"),
        ("Microcosmos_Huerto", "Huerto", "La Sembradora: los primeros cultivos"),
        ("Microcosmos_Egina", "Egina", "Éaco, Zeus y los mirmidones"),
        ("Microcosmos_Buda", "Kapilavastu", "Siddhārtha y los cuatro encuentros"),
        ("Microcosmos_Eratostenes", "Alejandría", "Eratóstenes mide la Tierra"),
        ("Microcosmos_Patanjali", "Patanjali", "Patanjali y los ocho miembros del yoga"),
        ("Microcosmos_Nightingale", "Escútari", "Florence Nightingale"),
        ("Microcosmos_Wright", "Dayton", "los hermanos Wright"),
        ("Microcosmos_PlantaAtomica", "Planta Atómica", "Newton, Curie y Einstein"),
        ("Microcosmos_Tregua1914", "Tregua 1914", "la tregua de Navidad"),
        ("Microcosmos_IrenaSendler", "Irena Sendler", "Irena Sendler y los niños del gueto"),
        ("Microcosmos_LeChambon", "Le Chambon-sur-Lignon", "el pueblo que escondió a miles"),
    };

    [MenuItem("Tools/Cold Sanctuary/Build Sala de Eras (portales a cada microcosmos)")]
    public static void Build()
    {
        MicrocosmosAreaSceneBuilder.DropUntitled();
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        var root = new GameObject("Sala_de_Eras").transform; root.position = Origin;
        var sun = new GameObject("Sol").AddComponent<Light>(); sun.type = LightType.Directional; sun.transform.SetParent(root, false); sun.transform.rotation = Quaternion.Euler(45f, 150f, 0f);

        var list = new List<(string scene, string display, string story, int year, string place)>();
        foreach (var (sc, d, st) in All)
        {
            if (!System.IO.File.Exists($"Assets/Scenes/{sc}.unity")) { Debug.LogWarning($"[Eras] falta {sc}"); continue; }
            var e = Eras.OfMicrocosmos(d);
            list.Add((sc, d, st, e.HasValue ? e.Value.year : Eras.Present, e.HasValue ? e.Value.place : d));
        }
        list.Sort((a, b) => a.year.CompareTo(b.year));

        const float step = 7f;
        float len = list.Count * step + 10f;
        Box(root, "Suelo", new Vector3(len * 0.5f - 5f, -0.05f, 0f), new Vector3(len, 0.1f, 16f), new Color(0.6f, 0.58f, 0.55f), true);
        Box(root, "Linea_del_tiempo", new Vector3(len * 0.5f - 5f, 0.02f, -3f), new Vector3(len, 0.02f, 0.3f), new Color(0.25f, 0.25f, 0.3f), false);
        for (int i = 0; i < list.Count; i++)
        {
            var it = list[i];
            float x = i * step;
            Transform p = Box(root, $"Portal_{it.display}", new Vector3(x, 1.6f, 2f), new Vector3(3f, 3.2f, 0.3f), Tint(it.year), true);
            EraPortal ep = p.gameObject.AddComponent<EraPortal>(); ep.scene = it.scene; ep.label = it.display;
            p.localRotation = Quaternion.Euler(0f, 180f, 0f);   // de cara al explorador (−Z)
            Box(root, "Marco", new Vector3(x, 3.4f, 2f), new Vector3(3.4f, 0.3f, 0.4f), new Color(0.3f, 0.28f, 0.25f), false);
            var bio = MicroBiome.For(it.display);
            Text(root, $"{Eras.Format(it.year)}\n{it.place}\n<{it.story}>" + (bio != null ? $"\n{bio.title}" : ""), new Vector3(x, 4.6f, 2f), 0.07f);
            Text(root, Eras.Format(it.year), new Vector3(x, 0.05f, -3.6f), 0.06f).localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        GalleryExplorer.Spawn(Origin + new Vector3(0f, 0.1f, -8f), 0f);
        Debug.Log($"[Eras] Sala de Eras: {list.Count} portales del {Eras.Format(list[0].year)} al {Eras.Format(list[list.Count - 1].year)} → {ScenePath}");

        EditorSceneManager.SaveScene(scene, ScenePath);
        var bs = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!bs.Exists(s => s.path == ScenePath)) { bs.Add(new EditorBuildSettingsScene(ScenePath, true)); EditorBuildSettings.scenes = bs.ToArray(); }
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
    }

    static Color Tint(int year) => year < -10000 ? new Color(0.45f, 0.35f, 0.25f) : year < 0 ? new Color(0.75f, 0.6f, 0.35f) : year < 1900 ? new Color(0.5f, 0.6f, 0.75f) : new Color(0.55f, 0.65f, 0.55f);

    static Transform Box(Transform parent, string name, Vector3 local, Vector3 size, Color c, bool solid)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localScale = size;
        if (!solid) Object.DestroyImmediate(g.GetComponent<Collider>());
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
        return g.transform;
    }

    static Transform Text(Transform parent, string text, Vector3 local, float size)
    {
        var go = new GameObject("Cartel"); go.transform.SetParent(parent, false); go.transform.localPosition = local;
        TextMesh tm = go.AddComponent<TextMesh>(); tm.text = text; tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center;
        tm.characterSize = size; tm.fontSize = 40; tm.color = Color.black;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (tm.font != null) go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        return go.transform;
    }
}
