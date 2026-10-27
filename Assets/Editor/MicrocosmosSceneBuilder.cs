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
/// teletransporta al jugador al `MobSpawnPoint`; el `WorldExitPortal` lo devuelve. Falta cablear QUÉ dispara la entrada (un
/// trigger del prólogo/alba); el resto (offset + spawn + portal) ya está resuelto aquí.
/// </summary>
public static class MicrocosmosSceneBuilder
{
    public const string SceneName = "Microcosmos_Scene1_Ambrosio";
    const string ScenePath = "Assets/Scenes/" + SceneName + ".unity";

    // Origen LEJANO y DISTINTO del de Mesopotamia (5000,0,5000): así, cargada additive sobre el mundo base (o junto a la
    // otra escena hermana), el contenido —autorado en coords base— no solapa a nadie. Todo el root se desplaza aquí.
    static readonly Vector3 O = new Vector3(-5000f, 0f, 5000f);

    /// <summary>
    /// EL MICROCOSMOS DE AMBROSIO (el primero del recién llegado), hecho con la PLANTILLA (Berón 2026-10-27): la planta del
    /// HOSPITAL —de donde sale el tanque— agrandada ×10, con la flora y los insectos de la taiga de antes del fuego, al
    /// ALBA. La historia va por su pasillo central: la familia de Ambrosio empuja a Sakshi y a Shanidar hasta la cueva
    /// (<see cref="AmbrosioStory"/>), todos con cuerpo de mirmidón.
    /// </summary>
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos Scene1 (Ambrosio)")]
    public static void BuildScene1()
    {
        var spec = new MicrocosmSpec(SceneName, "Ambrosio (nivel 1)", "verano corto de la taiga, antes del fuego", 5.5f, "Infirmary_Area")
            { tint = new Color(0.85f, 0.95f, 0.9f), kind = SampleSceneBuilder.MissionKind.Heal, story = AmbrosioCast };
        MicrocosmTemplate.Build(spec, O);
        Debug.Log($"[MicrocosmosSceneBuilder] {ScenePath}: el microcosmos de Ambrosio, con la plantilla (hospital ×10, taiga, alba).");
    }

    // El elenco y el camino de la historia, en el hueco de la historia (la entrada queda detrás, a −z).
    static int AmbrosioCast(Transform cast)
    {
        const float caveZ = 88f;
        Transform start = new GameObject("Inicio_familia").transform; start.SetParent(cast, false); start.localPosition = new Vector3(0f, 0f, -4f);
        Material soil = AreaMicrocosmos.Mat("Ambrosio_Sendero", new Color(0.36f, 0.3f, 0.22f)), rock = AreaMicrocosmos.Mat("Ambrosio_Roca", new Color(0.45f, 0.42f, 0.38f)),
                 dark = AreaMicrocosmos.Mat("Ambrosio_Boca_cueva", new Color(0.06f, 0.05f, 0.04f));
        AreaMicrocosmos.Box(cast, "Sendero_a_la_cueva", new Vector3(0f, 0.03f, caveZ * 0.5f - 4f), new Vector3(9f, 0.04f, caveZ + 12f), soil, false);

        // LA CUEVA: un cerro de roca con la boca oscura; es el REFUGIO de los dos viejos (CarryToRefuge).
        Transform cave = new GameObject("Cueva").transform; cave.SetParent(cast, false); cave.localPosition = new Vector3(0f, 0f, caveZ);
        for (int k = 0; k < 4; k++) AreaMicrocosmos.Box(cave, "Roca", new Vector3(0f, 1.6f + k * 2.4f, 6f + k * 0.8f), new Vector3(22f - k * 5f, 3.2f, 10f - k * 1.8f), rock, true);
        AreaMicrocosmos.Box(cave, "Boca", new Vector3(0f, 1.8f, 0.95f), new Vector3(4.5f, 3.6f, 0.1f), dark, false);
        var refuge = cave.gameObject.AddComponent<CarryToRefuge>(); refuge.radius = 3.5f; refuge.needed = 2;
        WorldLabel cl = cave.gameObject.AddComponent<WorldLabel>(); cl.text = "La cueva"; cl.height = 9f;

        int n = 0;
        GameObject Member(string name, Vector3 at, Color c, float h, float bulk, bool aphid = false, bool weak = false)
        {
            var g = new GameObject(name); g.transform.SetParent(cast, false); g.transform.localPosition = at;
            var col = g.AddComponent<CapsuleCollider>(); col.height = h; col.radius = 0.35f * bulk; col.center = Vector3.up * h * 0.5f;
            CubeInsect.DressMyrmidon(g, c, h, bulk, aphid);
            g.AddComponent<Anima>();
            if (weak) g.AddComponent<WeakOne>();
            n++;
            return g;
        }
        // La familia (docs/microcosmos-level1.md): Ambrosio es un PULGÓN grande y manso; Sakshi, la anciana que lo crió.
        Member("Ambrosio", new Vector3(-1.6f, 0f, -5.6f), new Color(0.74f, 0.84f, 0.5f), 2.0f, 1.6f, aphid: true);
        Member("Sakshi", new Vector3(-1.6f, 0f, -4f), new Color(0.38f, 0.42f, 0.48f), 1.6f, 0.9f, weak: true);
        Member("Shanidar", new Vector3(1.6f, 0f, -4f), new Color(0.5f, 0.45f, 0.4f), 1.6f, 0.95f, weak: true);
        Member("Atlas", new Vector3(-2.4f, 0f, -5.4f), new Color(0.3f, 0.2f, 0.15f), 2.0f, 1.25f);
        Member("Medea", new Vector3(1.0f, 0f, -5.5f), new Color(0.45f, 0.15f, 0.15f), 1.55f, 0.85f);
        Member("Momo", new Vector3(2.2f, 0f, -5.5f), new Color(0.75f, 0.5f, 0.2f), 1.75f, 1f);
        Member("Ruth", new Vector3(3f, 0f, caveZ - 3f), new Color(0.35f, 0.45f, 0.25f), 1.75f, 1f);
        Member("Hespero", new Vector3(-3f, 0f, caveZ - 3f), new Color(0.28f, 0.2f, 0.18f), 1.9f, 1.1f);
        // La tribu que vive allí (recolectoras con su guarida), a un lado del camino.
        n += AreaMicrocosmos.ForagingBand(cast, new Vector3(-26f, 0f, 30f), 5);
        cast.root.gameObject.AddComponent<AmbrosioStory>();
        return n;
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

        BuildAmbientInsects(root.transform);   // insectos NEUTROS de ambiente (no afectan el nivel; dan vida real)

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
        portal.name = "WorldExitPortal"; portal.transform.SetParent(root.transform);
        portal.transform.localPosition = shore + new Vector3(4f, 0.6f, -2f);
        portal.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = MakeMat("MicroWorldExitPortal0", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<WorldExitPortal>();

        BakeMicroNavMesh();

        MicroWater.SurroundScene(scene);
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

    // INSECTOS NEUTROS de ambiente: usa el FACTORY/prefabs de fauna para poblar la escena con bichos que NO afectan al
    // nivel (dan vida/sonido real). Carga el prefab por ruta (editor); si no existe (fbx pendiente), se salta. Dispersa
    // unos pocos de cada especie lejos de la orilla de Sakshi. docs/anyma-factory-and-control.md.
    static void BuildAmbientInsects(Transform root)
    {
        GameObject amb = new GameObject("InsectosAmbiente"); amb.transform.SetParent(root);
        (string species, int count)[] fauna = { ("Cricket", 4), ("Aphid", 5), ("Ladybug", 3), ("Grasshopper", 4) };
        foreach (var (species, count) in fauna)
        {
            GameObject prefab = AnimalPrefabGenerator.LoadOrCreate(species);   // lo genera si falta (especies nuevas)
            if (prefab == null) { Debug.Log($"[Micro] ambiente: sin prefab {species} (pon el .fbx y genera) — se omite."); continue; }
            for (int i = 0; i < count; i++)
            {
                Vector2 r = Random.insideUnitCircle * 20f;
                Vector3 p = new Vector3(r.x, 0.4f, Mathf.Abs(r.y));   // zona norte/centro, lejos de la orilla sur
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.SetParent(amb.transform); go.transform.position = p;
            }
        }
    }

    static void AddToBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
