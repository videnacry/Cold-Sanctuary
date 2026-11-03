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
            { tint = new Color(0.85f, 0.95f, 0.9f), kind = SampleSceneBuilder.MissionKind.Heal, story = AmbrosioCast, foragers = false };   // historia cerrada de la novela: sin recolectoras de otras escenas
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
        AreaMicrocosmos.Box(cast, "Sendero_a_la_cueva", new Vector3(0f, 0.02f, caveZ * 0.5f - 4f), new Vector3(9f, 0.04f, caveZ + 12f), soil, true);   // sólido: se pisa ENCIMA (antes las patas quedaban hundidas)

        // LA CUEVA: un cerro de roca con la boca oscura; es el REFUGIO de los dos viejos (CarryToRefuge).
        Transform cave = new GameObject("Cueva").transform; cave.SetParent(cast, false); cave.localPosition = new Vector3(0f, 0f, caveZ);
        for (int k = 0; k < 4; k++) AreaMicrocosmos.Box(cave, "Roca", new Vector3(0f, 1.6f + k * 2.4f, 6f + k * 0.8f), new Vector3(22f - k * 5f, 3.2f, 10f - k * 1.8f), rock, true);
        AreaMicrocosmos.Box(cave, "Boca", new Vector3(0f, 1.8f, 0.95f), new Vector3(4.5f, 3.6f, 0.1f), dark, false);
        var refuge = cave.gameObject.AddComponent<CarryToRefuge>(); refuge.radius = 5f; refuge.needed = 4;   // los cuatro viejos de la novela
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
        // Los otros dos viejos de los cuatro que «desertaron para quedarse con ellos» (docs/novela.md).
        Member("Anciano_Pintor", new Vector3(-4f, 0f, -3.7f), new Color(0.55f, 0.5f, 0.42f), 1.55f, 0.9f, weak: true);
        Member("Anciana", new Vector3(4f, 0f, -3.7f), new Color(0.48f, 0.4f, 0.45f), 1.5f, 0.85f, weak: true);
        Member("Atlas", new Vector3(-2.4f, 0f, -5.4f), new Color(0.3f, 0.2f, 0.15f), 2.0f, 1.25f);
        Member("Medea", new Vector3(1.0f, 0f, -5.5f), new Color(0.45f, 0.15f, 0.15f), 1.55f, 0.85f);
        Member("Momo", new Vector3(2.2f, 0f, -5.5f), new Color(0.75f, 0.5f, 0.2f), 1.75f, 1f);
        Member("Ruth", new Vector3(3f, 0f, caveZ - 3f), new Color(0.35f, 0.45f, 0.25f), 1.75f, 1f);
        Member("Hespero", new Vector3(-3f, 0f, caveZ - 3f), new Color(0.28f, 0.2f, 0.18f), 1.9f, 1.1f);
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
    /// <summary>
    /// EL RÍO DE SAKSHI, hecho con la PLANTILLA (Berón 2026-10-27): el microcosmos de la SALA DE YOGA (su plataforma ×10),
    /// antes de Ambrosio, al amanecer, con la flora y los insectos de la taiga. La Magnate envía aquí al recién llegado tras
    /// su primera clase de yoga. Historia: <see cref="SakshiRiverStory"/> (docs/microcosmos-sakshi-origin.md).
    /// </summary>
    public static void BuildScene0()
    {
        var spec = new MicrocosmSpec(SceneName0, "Sakshi (el río)", "primavera, antes de Ambrosio (el río del origen de Sakshi)", 6f, "YogaRoom_Area")
            { tint = new Color(0.75f, 0.65f, 0.85f), story = SakshiRiverCast, foragers = false };
        MicrocosmTemplate.Build(spec, O0);
        Debug.Log($"[MicrocosmosSceneBuilder] {ScenePath0}: el río de Sakshi, con la plantilla (sala de yoga ×10, amanecer).");
    }

    // El río, la orilla con Sakshi y la tribu que llega por la orilla (la entrada queda detrás, a −z).
    static int SakshiRiverCast(Transform cast)
    {
        Material water = AreaMicrocosmos.Mat("Rio_Sakshi", new Color(0.3f, 0.55f, 0.85f)), shore = AreaMicrocosmos.Mat("Orilla_Sakshi", new Color(0.36f, 0.3f, 0.2f));
        // EL RÍO, de lado a lado delante de la orilla: su corriente arrastra (RiverCurrent) a quien entra.
        Transform river = new GameObject("Rio").transform; river.SetParent(cast, false); river.localPosition = new Vector3(0f, 0.3f, 4f);
        BoxCollider rb = river.gameObject.AddComponent<BoxCollider>(); rb.isTrigger = true; rb.size = new Vector3(80f, 3f, 8f);
        RiverCurrent rc = river.gameObject.AddComponent<RiverCurrent>(); rc.flowDirection = Vector3.right; rc.pushPower = 3f; rc.massResist = 1f; rc.dragEnergyPerSecond = 2f;
        AreaMicrocosmos.Box(river, "Agua", new Vector3(0f, -0.26f, 0f), new Vector3(80f, 0.06f, 8f), water, false);
        AreaMicrocosmos.Box(cast, "Orilla", new Vector3(0f, 0.02f, -6f), new Vector3(60f, 0.04f, 12f), shore, true);
        int n = 0;
        GameObject Member(string name, Vector3 at, Color c, float h, float bulk)
        {
            var g = new GameObject(name); g.transform.SetParent(cast, false); g.transform.localPosition = at;
            var col = g.AddComponent<CapsuleCollider>(); col.height = h; col.radius = 0.35f * bulk; col.center = Vector3.up * h * 0.5f;
            CubeInsect.DressMyrmidon(g, c, h, bulk);
            g.AddComponent<Anima>();
            n++;
            return g;
        }
        // SAKSHI, una cría (más pequeña), en la orilla, de cara al río.
        Member("Sakshi", new Vector3(1.5f, 0f, -3f), new Color(0.38f, 0.42f, 0.48f), 1.1f, 0.7f);
        // La TRIBU, que huye de los depredadores del norte hacia el único sitio seguro: la orilla (llega por el oeste).
        Transform arrive = new GameObject("Llegada_tribu").transform; arrive.SetParent(cast, false); arrive.localPosition = new Vector3(-30f, 0f, -6f);
        Member("Hespero_joven", new Vector3(-32f, 0f, -5f), new Color(0.28f, 0.2f, 0.18f), 1.8f, 1f);
        Member("Ruth_joven", new Vector3(-33f, 0f, -7f), new Color(0.35f, 0.45f, 0.25f), 1.7f, 0.95f);
        Member("Anciana_Aurea", new Vector3(-35f, 0f, -5.5f), new Color(0.75f, 0.55f, 0.25f), 1.55f, 0.9f);   // el alma que será Momo
        Member("Anciana_Vela", new Vector3(-35.5f, 0f, -7.5f), new Color(0.45f, 0.18f, 0.18f), 1.5f, 0.85f);   // el alma que será Medea
        cast.root.gameObject.AddComponent<SakshiRiverStory>();
        return n;
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
