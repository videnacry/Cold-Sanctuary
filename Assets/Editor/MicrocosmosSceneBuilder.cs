using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Genera la ESCENA propia del MICROCOSMOS **Scene1 (Ambrosio / el alba)** — el nivel introductorio de la cueva y el
/// pulgón (docs/microcosmos-level1.md, microcosmos-insects.md §13). El microcosmos es su PROPIO plano: sus niveles son
/// escenas separadas, NO objetos dentro de la escena del mesocosmos (Santuario 1). Scene1 (Ambrosio, anterior en la
/// historia) y <see cref="MobWorldSceneBuilder"/> (Mesopotamia, la ciudad-insecto) son escenas **HERMANAS** del mismo
/// microcosmos. Escenas-desde-código: la .unity no se versiona (como SampleScene/Mesopotamia) pero se regenera aquí.
///
/// Reúne el contenido ya existente (`SampleSceneBuilder.BuildMicrocosmosSandbox` = hormiguero/pulgón-guía/familia caída;
/// `BuildNivel1Sandbox` = mapa abierto con hormigas + depredadores + hechizos de Kushal) bajo un root en su propia escena,
/// con luz y NavMesh horneado. Se añade a Build Settings para cargarla por nombre en runtime (additive sobre el jugador).
///
/// Uso: Tools → Cold Sanctuary → Build Microcosmos Scene1 (Ambrosio). También la regenera "Build Sample Scene Blockout".
/// Carga en runtime: `MobWorldLoader.Instance.EnterMobWorld("Microcosmos_Scene1_Ambrosio")` (genérico, no solo mob) →
/// teletransporta al jugador al `MobSpawnPoint`; el `YogaPortal` lo devuelve. Falta cablear QUÉ dispara la entrada (un
/// trigger del prólogo/alba); el resto (offset + spawn + portal) ya está resuelto aquí.
/// </summary>
public static class MicrocosmosSceneBuilder
{
    public const string SceneName = "Microcosmos_Scene1_Ambrosio";
    const string ScenePath = "Assets/Scenes/" + SceneName + ".unity";

    // Origen LEJANO y DISTINTO del de Mesopotamia (5000,0,5000): así, cargada additive sobre el mundo base (o junto a la
    // otra escena hermana), el contenido —autorado en coords base— no solapa a nadie. Todo el root se desplaza aquí.
    static readonly Vector3 O = new Vector3(-5000f, 0f, 5000f);

    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos Scene1 (Ambrosio)")]
    public static void BuildScene1()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        Scene prev  = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);   // los GameObjects nuevos caen en esta escena

        // Luz direccional (el alba). Para una luz direccional la posición es irrelevante (solo cuenta la rotación).
        GameObject lightGO = new GameObject("Sun");
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95f, 0.85f);   // luz cálida de amanecer

        // Contenido del microcosmos Nivel 1 (reutiliza los builders existentes; ahora viven en SU escena). Se construye
        // con el root en el ORIGEN (los builders fijan posiciones en coords base) y luego se DESPLAZA el root entero a O
        // → todo el contenido se mueve coherentemente sin tocar los builders. Las posiciones autoradas quedan como
        // offsets locales respecto de O.
        GameObject root = new GameObject("Microcosmos_Scene1_AUTO");
        SampleSceneBuilder.BuildMicrocosmosSandbox(root.transform);   // cueva/pulgón-guía/familia caída (el tableau del alba)
        SampleSceneBuilder.BuildNivel1Sandbox(root.transform);        // mapa abierto: hormigas + depredadores + hechizos de Kushal
        root.transform.position = O;                                 // desplaza TODO a un origen lejano (no solapa el mundo base)

        // Entrada/salida del jugador (genérico vía MobWorldLoader, como Mesopotamia): teletransporta al MobSpawnPoint al
        // entrar; el YogaPortal devuelve al mundo normal. Ambos parentados al root → caen ya en O.
        GameObject spawn = new GameObject("MobSpawnPoint");
        spawn.transform.SetParent(root.transform);
        spawn.transform.localPosition = new Vector3(0f, 1f, -6f);
        spawn.transform.localRotation = Quaternion.LookRotation(Vector3.forward);
        spawn.AddComponent<MobSpawnPoint>();

        GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "YogaPortal";
        portal.transform.SetParent(root.transform);
        portal.transform.localPosition = new Vector3(0f, 1f, -5f);
        portal.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = MakeMat("MicroYogaPortal", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<YogaPortal>();

        BakeMicroNavMesh();   // tras el offset → el NavMesh se hornea en O (donde están de verdad las hormigas)

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings(ScenePath);
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);

        Debug.Log($"[MicrocosmosSceneBuilder] Escena guardada en {ScenePath} y añadida a Build Settings " +
                  $"(hermana de {MobWorldSceneBuilder.SceneName}).");
    }

    // ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════
    //  SCENE0 — Origen de Sakshi (el PRIMER viaje de Kushal). ANTERIOR a Scene1 (Ambrosio). docs/microcosmos-sakshi-origin.md
    // ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════
    public const string SceneName0 = "Microcosmos_Scene0_SakshiRio";
    const string ScenePath0 = "Assets/Scenes/" + SceneName0 + ".unity";
    static readonly Vector3 O0 = new Vector3(5000f, 0f, -5000f);   // origen lejano, distinto de Scene1 y Mesopotamia

    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos Scene0 (Sakshi / el rio)")]
    public static void BuildScene0()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");

        Scene prev  = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        GameObject lightGO = new GameObject("Sun");
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional; light.color = new Color(1f, 0.95f, 0.85f);

        GameObject root = new GameObject("Microcosmos_Scene0_AUTO");

        // ── Suelo (orilla fangosa) ~60x60 ──────────────────────────────────────
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "ForestFloor"; floor.transform.SetParent(root.transform);
        floor.transform.position = Vector3.zero; floor.transform.localScale = new Vector3(6f, 1f, 6f);   // 60x60
        floor.GetComponent<Renderer>().sharedMaterial = MakeMat("Shore_MAT", new Color(0.30f, 0.24f, 0.16f));

        // La orilla de Sakshi = borde SUR (z negativo). Es el PUNTO SEGURO (más lejos del anillo) y la ENTRADA.
        Vector3 shore = new Vector3(0f, 0.4f, -25f);

        // ── RÍO: franja N→S que desemboca en la orilla; arrastra aguas abajo (-Z) y debilita ──
        GameObject river = new GameObject("Rio");
        river.transform.SetParent(root.transform);
        river.transform.position = new Vector3(0f, 0.2f, 3f);
        BoxCollider rbox = river.AddComponent<BoxCollider>();
        rbox.size = new Vector3(7f, 3f, 52f);   // corre casi todo el mapa en Z
        RiverCurrent rc = river.AddComponent<RiverCurrent>();
        rc.flowDirection = new Vector3(0f, 0f, -1f);   // aguas abajo = hacia la orilla sur
        rc.pushPower = 3f; rc.massResist = 1f; rc.dragEnergyPerSecond = 2f;
        // visual del cauce (fino, decorativo)
        GameObject riverVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        riverVis.name = "Cauce_vis"; riverVis.transform.SetParent(river.transform);
        riverVis.transform.localPosition = Vector3.zero; riverVis.transform.localScale = new Vector3(7f, 0.1f, 52f);
        Object.DestroyImmediate(riverVis.GetComponent<Collider>());
        riverVis.GetComponent<Renderer>().sharedMaterial = MakeMat("Rio_MAT", new Color(0.30f, 0.55f, 0.85f, 0.6f));

        // ── SAKSHI (cría) en la orilla, DEBILITADA por el arrastre → solo observa (ventana de bond) ──
        // Alma-mezcla del alba: cuerpo Ant+Human; mente Human(alba)+Agua(contemplativa)+Ant.
        GameObject sakshi = SampleSceneBuilder.Cast(root.transform, "Sakshi", shore + new Vector3(-2f, 0f, 0f),
            new Vector3(0.30f, 0.30f, 0.42f), new Color(0.35f, 0.40f, 0.45f),
            "F", "cria que el rio arrastro lejos de los suyos; llega sin fuerzas, solo observa",
            "El Chaman (propuesta)", "de aqui le nace detenerse a observar/indagar/asombrarse",
            new[] { ("Ant", 85f), ("Human", 15f) }, new[] { ("Human", 55f), ("Agua", 25f), ("Ant", 20f) });
        sakshi.AddComponent<CharacterLevel>();
        WeaknessEffect sweak = sakshi.AddComponent<WeaknessEffect>();
        sweak.drainPerSecond = 6f; sweak.controlAgent = false;   // debilitada: sin ATP no puede huir
        sakshi.AddComponent<MoodDynamics>();      // el Guardián (estrés del estado, amortiguado por la observación)
        sakshi.AddComponent<ObserveSpell>();      // los ojos: débil, solo puede OBSERVAR → gana ecuanimidad (no huye, se deja acompañar)

        // ── El DEPREDADOR DÉBIL, de poca hambre, junto a la orilla (prefiere hormiga a gusano — mecánica pendiente) ──
        GameObject weakPred = MakePredator(root.transform, "Depredador_debil", shore + new Vector3(9f, 0.1f, 4f),
            new Vector3(0.7f, 0.7f, 1.0f), new Color(0.5f, 0.35f, 0.3f), threatPower: 0.6f, threatRadius: 10f);

        // ── TRIÁNGULO DE DEPREDADORES (fuertes) en el NORTE/flancos → la base SUR (orilla de Sakshi) queda DESPEJADA ──
        // Corrección: antes había 2 depredadores cerca de la orilla; la tribu se quedaba en el centro. Ahora TODOS en
        // z ≥ 4 (mitad norte) → el gradiente "alejarse del peligro" apunta limpio al SUR, hacia Sakshi.
        Vector3[] ring = { new Vector3(0f, 0.1f, 28f),   // vértice norte
                           new Vector3(22f, 0.1f, 18f), new Vector3(-22f, 0.1f, 18f),   // flancos altos
                           new Vector3(30f, 0.1f, 4f),  new Vector3(-30f, 0.1f, 4f) };  // flancos medios (nada al sur)
        for (int i = 0; i < ring.Length; i++)
            MakePredator(root.transform, $"Depredador_anillo_{i}", ring[i], new Vector3(1.1f, 1.1f, 1.5f),
                new Color(0.4f, 0.2f, 0.2f), threatPower: 2.5f, threatRadius: 22f);

        // ── LA TRIBU (elenco de la era; almas de Momo/Medea en ancianas) — arranca al norte y CONVERGE ──
        // Huyen del anillo (ThreatScanner) + querencia a la orilla (HomeImpulse) → van solas hacia Sakshi. Emergente.
        (string name, string desc, string soul)[] tribe = {
            ("Hespero_joven",  "vigia joven de la banda",            "A"),
            ("Ruth_joven",     "recolectora joven",                  "C"),
            ("Anciana_Aurea",  "anciana; porta el alma que sera Momo (muere cerca de su nacimiento)",  "G"),
            ("Anciana_Vela",   "anciana; porta el alma que sera Medea (muere cerca de su nacimiento)",  "B"),
        };
        Vector3 tribeStart = new Vector3(0f, 0.4f, 18f);
        for (int i = 0; i < tribe.Length; i++)
        {
            Vector3 p = tribeStart + new Vector3((i - 1.5f) * 2.2f, 0f, 0f);
            GameObject ant = SampleSceneBuilder.Cast(root.transform, tribe[i].name, p, new Vector3(0.4f, 0.4f, 0.55f),
                new Color(0.5f, 0.46f, 0.38f), tribe[i].soul, tribe[i].desc, "", "era pre-Ambrosio (alma en cuerpo distinto)",
                new[] { ("Ant", 85f), ("Human", 15f) }, new[] { ("Human", 45f), ("Ant", 55f) });
            SampleSceneBuilder.Mobilize(ant, shore);              // querencia a la orilla segura (home)
            ThreatScanner ts = ant.AddComponent<ThreatScanner>();
            ts.scanRadius = 30f; ts.fearThreshold = 0.2f; ts.maxFleeMagnitude = 4f; ts.decayRate = 1.5f;
        }

        // ── Entrada del jugador (Kushal-gusano) en la orilla + salida ──
        root.transform.position = O0;   // desplaza TODO a un origen lejano (no solapa el mundo base)

        GameObject spawn = new GameObject("MobSpawnPoint");
        spawn.transform.SetParent(root.transform);
        spawn.transform.localPosition = shore + new Vector3(2f, 0.6f, -1f);
        spawn.transform.localRotation = Quaternion.LookRotation(Vector3.forward);
        spawn.AddComponent<MobSpawnPoint>();

        GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "YogaPortal"; portal.transform.SetParent(root.transform);
        portal.transform.localPosition = shore + new Vector3(4f, 0.6f, -2f);
        portal.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = MakeMat("MicroYogaPortal0", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<YogaPortal>();

        BakeMicroNavMesh();

        EditorSceneManager.SaveScene(scene, ScenePath0);
        AddToBuildSettings(ScenePath0);
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);

        Debug.Log($"[MicrocosmosSceneBuilder] Scene0 (origen de Sakshi) guardada en {ScenePath0}. BLOCKOUT: rio (RiverCurrent) " +
                  "+ orilla/entrada + Sakshi cria debilitada + depredador debil + anillo de depredadores (vertice seguro = " +
                  "orilla) + tribu que converge por ThreatScanner. Pendiente (mecanicas): preferencia de dieta (prefiere " +
                  "hormiga a gusano), empujar del gusano, escalada de hambre, y el subsistema de observacion.");
    }

    /// <summary>Depredador de blockout: cápsula + Anima + AiBrain + ThreatEmitter (para que la tribu lo perciba y huya).</summary>
    static GameObject MakePredator(Transform parent, string name, Vector3 pos, Vector3 scale, Color col, float threatPower, float threatRadius)
    {
        GameObject pred = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        pred.name = name; pred.transform.SetParent(parent);
        pred.transform.position = pos; pred.transform.localScale = scale;
        pred.GetComponent<Renderer>().sharedMaterial = MakeMat($"{name}_MAT", col);
        Anima a = pred.AddComponent<Anima>();
        a.strength = threatPower; a.bodyMass = scale.y;
        pred.AddComponent<AiBrain>().selfRelevance = 1.5f;
        pred.AddComponent<AnimaController>();
        ThreatEmitter te = pred.AddComponent<ThreatEmitter>();
        te.threatPower = threatPower; te.radius = threatRadius; te.falloff = 0.3f;
        return pred;
    }

    // Hornea el NavMesh sobre el suelo del bosque (ForestFloor lo crea BuildNivel1Sandbox), para que las hormigas naveguen.
    static void BakeMicroNavMesh()
    {
        GameObject floor = GameObject.Find("ForestFloor");
        if (floor == null) return;
        NavMeshSurface surface = floor.GetComponent<NavMeshSurface>();
        if (surface == null) surface = floor.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.BuildNavMesh();
    }

    static Material MakeMat(string name, Color c)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        return new Material(shader) { name = name, color = c };
    }

    static void AddToBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
