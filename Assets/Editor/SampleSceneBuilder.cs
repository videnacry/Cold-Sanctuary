using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// One-shot rough blockout for SampleScene: ground, sea + fog (with a swimmable trigger
/// zone), a handful of animal prefab instances (some clustered at CubAreaTrigger as loose
/// cubs), placeholder companions/teacher at orientative posts, 7 gameplay areas built as
/// primitive-box structures (floor mat + U-shaped walls) wired to SanctuaryDirector, the
/// screen-space holographic menu, and a minimal UI Canvas if none exists.
///
/// Everything gets parented under "LevelDressing_AUTO" so re-running this
/// tool is safe — it deletes and rebuilds that group instead of duplicating.
///
/// Uso: menu Tools > Cold Sanctuary > Build Sample Scene Blockout.
/// </summary>
[InitializeOnLoad]
public static class SampleSceneBuilder
{
    const string RootName = "LevelDressing_AUTO";
    const string AnimalsRoot = "Assets/Animals";
    const string EnvRoot = "Assets/Environment";
    const string CharactersRoot = "Assets/Characters";
    const string AutoRunPrefKey = "ColdSanctuary_SampleSceneBuilder_AutoRan";

    // The Tools menu wasn't picking up this MenuItem after the big package import earlier
    // (stale menu tree — a known Editor quirk, would need an Editor restart to fix cleanly).
    // Piggyback on the domain reload that already reliably happens on recompile instead.
    static SampleSceneBuilder()
    {
        if (EditorPrefs.GetBool(AutoRunPrefKey, false)) return;
        EditorApplication.delayCall += TryAutoBuild;
    }

    // Runs after the domain reload has fully settled, so scene queries are reliable
    // (checking the active scene name directly inside the static constructor above
    // was too early — it can read back an empty scene name mid-reload).
    static void TryAutoBuild()
    {
        if (EditorPrefs.GetBool(AutoRunPrefKey, false)) return;
        if (EditorSceneManager.GetActiveScene().name != "SampleScene") return;

        EditorPrefs.SetBool(AutoRunPrefKey, true);
        Build();
    }

    [MenuItem("Tools/Cold Sanctuary/Build Sample Scene Blockout")]
    public static void Build()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null) Object.DestroyImmediate(existing);

        GameObject root = new GameObject(RootName);

        BuildGround(root.transform);
        BuildSeaAndFog(root.transform);
        BuildAnimals(root.transform);
        System.Collections.Generic.List<NestSpec> nests = BuildWildlifePopulation(root.transform);
        BuildGrassPatches(root.transform, nests);
        BuildHabitatCover(root.transform, nests);
        BuildFishSchools(root.transform);
        BuildBirds(root.transform);
        BuildWorldSystems(root.transform);
        BuildCharacters(root.transform);
        SanctuaryArea[] areas = BuildAreaStructures(root.transform);
        WireSanctuaryDirector(areas);
        BuildUI(root.transform);
        EnsurePlayerSystems();
        BuildHolographicMenu(root.transform);
        BuildMeditationContent(root.transform); // reemplaza la entrada legacy de cocina (KitchenEntrance/KitchenScaleController)
        BuildSanctuaryEconomy(root.transform);  // recursos por santuario + HUD (Mesocosmos) — docs/world-topology-and-planes.md §4/§7
        BuildFarmingSandbox(root.transform);    // MVP de farming no-violento — docs/world-topology-and-planes.md §4.1
        BakeNavMesh();

        // Genera también la escena del mundo mob (Mesopotamia) → todo listo de un click.
        // La cocina ya apunta a ella (mobWorldSceneName). Guardado por si la API de escenas falla.
        try { MobWorldSceneBuilder.BuildMesopotamia(); }
        catch (System.Exception e) { Debug.LogWarning($"[SampleSceneBuilder] No se pudo generar la escena mob: {e.Message}"); }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SampleSceneBuilder] Blockout listo bajo 'LevelDressing_AUTO'. Revisar y ajustar posiciones a mano.");
    }

    // ── Economía de santuario (recursos + HUD) ─────────────────────────────────
    //
    // Backbone del mundo grande (docs/world-topology-and-planes.md §4/§7): cada área produce su
    // recurso (AreaProducer, pasivo + bonus por personaje asignado) hacia el ledger global
    // (SanctuaryResources), y un HUD prototipo lo muestra en el Mesocosmos.

    static void BuildSanctuaryEconomy(Transform parent)
    {
        GameObject econ = new GameObject("SanctuaryEconomy_AUTO");
        econ.transform.SetParent(parent);
        var resources = econ.AddComponent<SanctuaryResources>();
        resources.sanctuaryName = "Santuario Terrestre";
        econ.AddComponent<SanctuaryResourceHUD>();

        // Cada área produce lo que le corresponde (docs §4). Valores placeholder, ajustables.
        AddAreaProducer("Kitchen_Area",          SanctuaryResource.Food,     12f, 18f);
        AddAreaProducer("Garden_Area",           SanctuaryResource.Food,     18f, 20f);
        AddAreaProducer("VehicleWorkshop_Area",  SanctuaryResource.Materials, 14f, 16f);
        AddAreaProducer("TextileStudio_Area",    SanctuaryResource.Materials, 10f, 14f);
        AddAreaProducer("Infirmary_Area",        SanctuaryResource.Research,   8f, 12f);
        AddAreaProducer("VeterinaryClinic_Area", SanctuaryResource.Research,   8f, 12f);

        Debug.Log("[SampleSceneBuilder] Economía de santuario cableada: SanctuaryResources + HUD + AreaProducers por área.");
    }

    static void AddAreaProducer(string areaName, SanctuaryResource resource, float perGameMinute, float perWorkerBonus)
    {
        GameObject area = GameObject.Find(areaName);
        if (area == null) return; // el área puede no existir en este blockout — se omite en silencio

        AreaProducer p = area.GetComponent<AreaProducer>();
        if (p == null) p = area.AddComponent<AreaProducer>();
        p.resource       = resource;
        p.perGameMinute  = perGameMinute;
        p.perWorkerBonus = perWorkerBonus;
    }

    // ── Farming no-violento (MVP) ───────────────────────────────────────────────
    //
    // docs/world-topology-and-planes.md §4.1: jugar con la criatura baja su tensión; al serenarse
    // suelta recursos/monedas y se le puede dar de comer (F/clic) para que descanse. PlayController
    // en el jugador (tecla V) + criaturas placeholder para probar.

    static void BuildFarmingSandbox(Transform parent)
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            if (player.GetComponent<PlayController>() == null)  player.AddComponent<PlayController>();
            if (player.GetComponent<CharacterLevel>() == null)  player.AddComponent<CharacterLevel>(); // XP del farming
        }

        GameObject group = new GameObject("FarmingSandbox_AUTO");
        group.transform.SetParent(parent);
        group.AddComponent<FarmingSandboxItems>(); // crea items placeholder y los asigna como drops (runtime)

        // Criaturas de distinta dificultad/recompensa (dischargePerTouch menor = más difícil).
        // Suave/Media: criadas por humanos, seguras. Dura: puede perder el control si te excitas y no esquivas.
        // Salvaje: NO criada → no juega (rige la ley natural; V no hace nada) — demuestra el gateo por bond.
        AddPlayCreature(group.transform, "PlayCreature_Suave",   new Vector3( 6f, 1f, 6f), 0.15f, SanctuaryResource.Food,      15f, 2, 20f, handRaised: true,  canLoseControl: false);
        AddPlayCreature(group.transform, "PlayCreature_Media",   new Vector3( 9f, 1f, 4f), 0.10f, SanctuaryResource.Materials, 25f, 4, 35f, handRaised: true,  canLoseControl: false);
        AddPlayCreature(group.transform, "PlayCreature_Dura",    new Vector3(11f, 1f, 8f), 0.06f, SanctuaryResource.Research,  40f, 8, 60f, handRaised: true,  canLoseControl: true);
        AddPlayCreature(group.transform, "PlayCreature_Salvaje", new Vector3( 4f, 1f, 9f), 0.10f, SanctuaryResource.Research,  40f, 8, 60f, handRaised: false, canLoseControl: true);

        Debug.Log("[SampleSceneBuilder] Farming sandbox: PlayController (tecla V) + CharacterLevel en el jugador + 4 PlayableCreatures. " +
                  "Juega (V) hasta serenarlas (dan recursos/monedas/XP/items); F/clic para darles de comer. " +
                  "La 'Dura' puede golpearte si te excitas y no esquivas; la 'Salvaje' no juega (ley natural).");
    }

    static void AddPlayCreature(Transform parent, string name, Vector3 pos, float discharge,
                                SanctuaryResource res, float amount, int coins, float xp,
                                bool handRaised = true, bool canLoseControl = false)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"{name}_MAT", new Color(0.9f, 0.35f, 0.3f));

        PlayableCreature pc = go.AddComponent<PlayableCreature>();
        pc.dischargePerTouch = discharge;
        pc.dropResource      = res;
        pc.dropAmount        = amount;
        pc.dropCoins         = coins;
        pc.xpReward          = xp;
        pc.handRaised        = handRaised;
        pc.canLoseControl    = canLoseControl;
    }

    // ── World systems (singletons from the world-simulation/combat/economy commit) ──

    static void BuildWorldSystems(Transform parent)
    {
        GameObject systems = new GameObject("WorldSystems_AUTO");
        systems.transform.SetParent(parent);

        systems.AddComponent<PeriodicTableManager>();
        systems.AddComponent<SanctuaryDirector>();
        // DialogueManager.panel is left unassigned here — DialoguePanel is a UI
        // prefab that still needs to be designed/built in the Canvas by hand.
        systems.AddComponent<DialogueManager>();
        systems.AddComponent<MissionTracker>();
    }

    // Player-scoped singletons: live on the Player GameObject itself, not under
    // LevelDressing_AUTO, so this only adds what's missing instead of rebuilding.
    static void EnsurePlayerSystems()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("[SampleSceneBuilder] No se encontro 'Player' en la escena — se omiten los sistemas de combate/economia.");
            return;
        }

        EnsurePlayerVisual(player);

        // PlayerController expects a "CameraPivot" child holding both camera anchors
        // (it applies mouse/arrow pitch to the pivot, yaw to the Player body itself).
        // The anchors used to sit directly under Player — move them under the pivot,
        // preserving world position so the camera framing doesn't jump.
        Transform cameraPivot = player.transform.Find("CameraPivot");
        if (cameraPivot == null)
        {
            GameObject pivotGO = new GameObject("CameraPivot");
            pivotGO.transform.SetParent(player.transform);
            pivotGO.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            pivotGO.transform.localRotation = Quaternion.identity;
            cameraPivot = pivotGO.transform;
        }

        Transform thirdAnchor = player.transform.Find("ThirdPersonAnchor");
        if (thirdAnchor != null && thirdAnchor.parent != cameraPivot)
            thirdAnchor.SetParent(cameraPivot, worldPositionStays: true);

        Transform firstAnchor = player.transform.Find("FirstPersonAnchor");
        if (firstAnchor != null && firstAnchor.parent != cameraPivot)
            firstAnchor.SetParent(cameraPivot, worldPositionStays: true);

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller == null) controller = player.AddComponent<PlayerController>();
        controller.cameraPivot = cameraPivot;

        // Los campos de tecla ya estaban serializados en la escena desde antes de que
        // el default pasara de flechas a J/L/I/K (mecanografia) — un campo serializado
        // no se actualiza solo cuando cambia el default en el codigo fuente (mismo
        // problema que con Physiognomy.baseScale, ver AnimalPrefabGenerator). Se
        // reafirma a mano para que no pueda quedar desincronizado en silencio.
        controller.lookLeftKey  = KeyCode.J;
        controller.lookRightKey = KeyCode.L;
        controller.lookUpKey    = KeyCode.I;
        controller.lookDownKey  = KeyCode.K;

        if (player.GetComponent<PlayerCombat>() == null) player.AddComponent<PlayerCombat>();
        if (player.GetComponent<Inventory>() == null) player.AddComponent<Inventory>();
        if (player.GetComponent<CoinWallet>() == null) player.AddComponent<CoinWallet>();
        if (player.GetComponent<CombatAbilityBar>() == null) player.AddComponent<CombatAbilityBar>();
        if (player.GetComponent<CombatTargetSelector>() == null) player.AddComponent<CombatTargetSelector>();
        if (player.GetComponent<AsanaDetector>() == null) player.AddComponent<AsanaDetector>();
        if (player.GetComponent<InteractionController>() == null) player.AddComponent<InteractionController>();

        WorldCharacter playerWC = player.GetComponent<WorldCharacter>();
        if (playerWC == null) playerWC = player.AddComponent<WorldCharacter>();
        playerWC.characterName = "Player";
        playerWC.isPlayer = true;
    }

    // Player never had a visual mesh — just CharacterController + gameplay scripts,
    // so it rendered invisible (only the camera existed). Kushal.fbx was imported
    // during the Blender pipeline pass but never attached to anything.
    static void EnsurePlayerVisual(GameObject player)
    {
        Transform existing = player.transform.Find("Kushal_Model");
        GameObject kushal;
        if (existing != null)
        {
            kushal = existing.gameObject;
        }
        else
        {
            GameObject kushalAsset = LoadAndImport("Assets/Player/Models/Kushal.fbx");
            if (kushalAsset == null)
            {
                Debug.LogWarning("[SampleSceneBuilder] No se pudo cargar Kushal.fbx — el Player se queda sin mesh visual.");
                return;
            }

            kushal = (GameObject)PrefabUtility.InstantiatePrefab(kushalAsset, player.transform);
            kushal.name = "Kushal_Model";
            kushal.transform.localPosition = Vector3.zero;
            kushal.transform.localRotation = Quaternion.identity;
        }

        ApplyRealisticPlayerScale(kushal);
    }

    // Igual que con los animales (ver AnimalPrefabGenerator.RealisticScaleFactor):
    // Kushal.fbx vino con una unidad de importación inconsistente — medía ~3.38m de
    // alto en vez de los ~1.75m de una persona adulta real. Mide el tamaño crudo
    // (localScale=1) y reajusta de forma idempotente, así se puede re-correr el
    // blockout sin ir agrandando/achicando el modelo cada vez.
    const float TargetPlayerHeightMeters = 1.75f;
    static void ApplyRealisticPlayerScale(GameObject kushal)
    {
        Vector3 savedScale = kushal.transform.localScale;
        kushal.transform.localScale = Vector3.one;

        Bounds? combined = null;
        foreach (Renderer r in kushal.GetComponentsInChildren<Renderer>())
        {
            if (combined == null) combined = r.bounds;
            else { Bounds b = combined.Value; b.Encapsulate(r.bounds); combined = b; }
        }

        if (!combined.HasValue)
        {
            kushal.transform.localScale = savedScale;
            Debug.LogWarning("[SampleSceneBuilder] Kushal_Model: no se encontraron Renderers para ajustar la escala.");
            return;
        }

        float factor = TargetPlayerHeightMeters / Mathf.Max(combined.Value.size.y, 0.0001f);
        kushal.transform.localScale = new Vector3(factor, factor, factor);
    }

    // ── Material helper ──────────────────────────────────────────────────────

    static Material MakeMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material mat = new Material(shader) { name = name };
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        return mat;
    }

    // ── Ground ───────────────────────────────────────────────────────────────

    static void BuildGround(Transform parent)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground_Placeholder";
        ground.transform.SetParent(parent);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(50f, 1f, 50f); // ~500x500 units — agrandado 2x (antes 250x250) para dar espacio real entre territorios de fauna, ver BuildWildlifePopulation
        ground.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Ground_Snow_MAT", new Color(0.85f, 0.88f, 0.9f));

        if (ground.GetComponent<Collider>() == null)
            ground.AddComponent<MeshCollider>();

        ground.AddComponent<NavMeshSurface>().collectObjects = CollectObjects.All;
    }

    // Los NavMeshAgent de los animales fallan en silencio (y spamean SetDestination)
    // si no hay NavMesh horneado en la escena. Se hornea al final de Build(), una
    // vez colocados el suelo, las estructuras y demás obstáculos.
    static void BakeNavMesh()
    {
        GameObject ground = GameObject.Find("Ground_Placeholder");
        if (ground == null) return;
        NavMeshSurface surface = ground.GetComponent<NavMeshSurface>();
        if (surface == null) surface = ground.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.BuildNavMesh();
    }

    // ── Sea + fog ────────────────────────────────────────────────────────────

    static void BuildSeaAndFog(Transform parent)
    {
        GameObject sea = GameObject.CreatePrimitive(PrimitiveType.Plane);
        sea.name = "Sea_Placeholder";
        sea.transform.SetParent(parent);
        sea.transform.position = new Vector3(200f, -1f, 0f); // beside the ground block, slightly lower
        sea.transform.localScale = new Vector3(40f, 1f, 40f);
        sea.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Sea_Placeholder_MAT", new Color(0.10f, 0.28f, 0.45f));
        Object.DestroyImmediate(sea.GetComponent<Collider>()); // el MeshCollider del Plane es solo una superficie 2D, no sirve como volumen nadable

        // Zona de nado real — antes el mar era puramente decorativo (sin collider de ningún
        // tipo). Un BoxCollider trigger aparte, no atado a la escala del Plane visual (evita
        // líos de escala heredada), cubre el volumen bajo la superficie. WaterZone avisa a
        // PlayerController cuando el jugador entra/sale; MediumZone hace lo mismo para
        // cualquier LivingEntity (fauna) — ambos coexisten en el mismo trigger sin pisarse,
        // cada uno ignora los objetos que no tienen su componente requerido.
        GameObject swimZone = new GameObject("Sea_SwimZone");
        swimZone.transform.SetParent(parent);
        swimZone.transform.position = new Vector3(200f, -8f, 0f);
        BoxCollider swimCollider = swimZone.AddComponent<BoxCollider>();
        swimCollider.isTrigger = true;
        swimCollider.size = new Vector3(380f, 14f, 380f); // cubre el area visual del mar (plane 400x400) con margen
        swimZone.AddComponent<WaterZone>();
        swimZone.AddComponent<MediumZone>().medium = Medium.Water;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.80f, 0.85f, 0.90f);
        RenderSettings.fogDensity = 0.008f;
    }

    // ── Animals ──────────────────────────────────────────────────────────────

    static GameObject LoadAnimalPrefab(string species)
        => AssetDatabase.LoadAssetAtPath<GameObject>($"{AnimalsRoot}/{species}/{species}.prefab");

    static void BuildAnimals(Transform parent)
    {
        GameObject nestArea = GameObject.Find("CubAreaTrigger");
        Vector3 nestPos = nestArea != null ? nestArea.transform.position : new Vector3(10f, 0f, 10f);

        GameObject animalsGroup = new GameObject("Animals_Placed");
        animalsGroup.transform.SetParent(parent);

        // Loose cubs in the artificial nest area
        string[] nestSpecies = { "Bunny", "Bunny", "Fox", "Wolf", "Deer" };  // Malamute es mascota (no familia salvaje); Deer da presa al lobo
        for (int i = 0; i < nestSpecies.Length; i++)
        {
            GameObject prefab = LoadAnimalPrefab(nestSpecies[i]);
            if (prefab == null) continue;
            Vector3 offset = new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f));
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, animalsGroup.transform);
            instance.name = $"{nestSpecies[i]}_Cub_{i}";
            instance.transform.position = nestPos + offset;
        }

        // La presencia general de fauna adulta (antes: un puñado de instancias sueltas
        // hardcodeadas) ahora la maneja BuildWildlifePopulation() vía FamilyGenerator —
        // familias reales con padres+crías en vez de individuos aislados.
    }

    enum NestKind { Prey, Predator }

    // Un "nido" es el centro real de una familia — FamilyGenerator lo usa como punto de
    // dispersión al generar (radius) y, desde el fix en FamilyGenerator.Start(), también como
    // HomeOrigin COMPARTIDO de todos los miembros (antes cada hermano fijaba su propio
    // HomeOrigin a donde individualmente apareció — ver docs/refuge-and-adult-behavior.md
    // "Montaje de escena: nidos antes que familias").
    struct NestSpec
    {
        public string species;
        public Vector3 position;
        public float radius;
        public NestKind kind;
        public float height;
    }

    // homeRadius pequeño y realmente local — el único caso donde "nido de presa lejos de
    // depredadores" es una separación alcanzable dentro de este mapa (ver comentario en
    // ValidateNestSeparation). El resto de las especies tienen homeRadius mayor que medio
    // mapa (Fox 150, Wolf 200, Deer 250, Bear 300) — roaming realista, no aplica.
    static readonly System.Collections.Generic.Dictionary<string, float> SmallHomeRadius = new()
    {
        { "Bunny", 20f },
    };

    // Bioma balanceado ~70% herbívoros / 30% carnívoros (aprobado con el usuario):
    // terrestres Bunny(5) x4=20 + Deer(6) x2=12 = 32 herbívoros vs Wolf(6) + Fox(7) =
    // 13 carnívoros → 32/45 ≈ 71% herbívoro. Los marinos (Whale, Seal) van aparte —
    // no compiten por el mismo territorio que los terrestres, así que no entran en
    // esa proporción. Cada entrada es una familia SEPARADA (no una manada única) para
    // que se vean grupos familiares distintos repartidos por el área en vez de un
    // solo clúster gigante. A diferencia del resto del blockout (que instancia en
    // tiempo de Editor), FamilyGenerator.Start() genera recién al entrar en Play —
    // el área se ve vacía en el Editor hasta que se corre el juego.
    //
    // Nidos repartidos por TODO el lado oeste del suelo (x en [-250,-20]; el este,
    // x en [15,80], es de los edificios del santuario) en tres bandas por z con margen amplio
    // entre ellas. El suelo se agrandó 2x (250x250 -> 500x500, ver BuildGround) específicamente
    // para esto — antes, incluso repartidas por las tres bandas, las familias quedaban con poco
    // margen real entre nidos de presa y depredador (mínimo ~60u). Con el mapa más grande la
    // separación mínima ronda ahora los 100-400u, sensación de territorio mucho más realista.
    // También se agrandó un poco el radio de dispersión de cada familia (más espacio entre los
    // individuos de un mismo grupo, no solo entre grupos). Devuelve la lista de nidos para que
    // BuildGrassPatches/BuildHabitatCover coloquen comida y vegetación cerca de cada nido de presa.
    static System.Collections.Generic.List<NestSpec> BuildWildlifePopulation(Transform parent)
    {
        GameObject go = new GameObject("WildlifePopulation_AUTO");
        go.transform.SetParent(parent);
        FamilyGenerator generator = go.AddComponent<FamilyGenerator>();

        var nests = new System.Collections.Generic.List<NestSpec>();
        var families = new System.Collections.Generic.List<FamilyGenerator.Family>();
        void AddFamily(string species, Vector3 pos, float radius, NestKind kind, float height = 0f)
        {
            nests.Add(new NestSpec { species = species, position = pos, radius = radius, kind = kind, height = height });

            GameObject prefab = LoadAnimalPrefab(species);
            if (prefab == null)
            {
                Debug.LogWarning($"[SampleSceneBuilder] WildlifePopulation: no se encontro el prefab de {species}, se omite esa familia.");
                return;
            }
            families.Add(new FamilyGenerator.Family { animalPrefab = prefab, position = pos, radius = radius, renderHeight = height });
        }

        // Herbívoros — banda norte y centro-oeste
        AddFamily("Bunny", new Vector3(-220f, 0f, 190f), 10f, NestKind.Prey);
        AddFamily("Bunny", new Vector3(-60f, 0f, 200f), 10f, NestKind.Prey);
        AddFamily("Bunny", new Vector3(-230f, 0f, 20f), 10f, NestKind.Prey);
        AddFamily("Bunny", new Vector3(-100f, 0f, -30f), 10f, NestKind.Prey);
        AddFamily("Deer", new Vector3(-140f, 0f, 150f), 15f, NestKind.Prey);
        AddFamily("Deer", new Vector3(-40f, 0f, 40f), 15f, NestKind.Prey);

        // Carnívoros — banda sur, lejos de los nidos de presa
        AddFamily("Wolf", new Vector3(-190f, 0f, -190f), 18f, NestKind.Predator);
        AddFamily("Fox", new Vector3(-90f, 0f, -210f), 15f, NestKind.Predator);

        // Oso Polar — apex predator, territorio amplio (BearBehaviour.homeRadius=300);
        // frontera sur-este del bioma terrestre, más cerca de x=0 (rumbo a la zona marina en
        // x≈180-200) ya que su Diet prioriza Seal > Bunny > Wolf.
        AddFamily("PolarBear", new Vector3(-30f, 0f, -110f), 28f, NestKind.Predator);

        ValidateNestSeparation(nests);

        // Marinos — sobre el Sea_Placeholder, aparte del bioma terrestre
        AddFamily("Whale", new Vector3(200f, -1f, 15f), 30f, NestKind.Prey, -1f);
        AddFamily("Seal", new Vector3(180f, -1f, -20f), 15f, NestKind.Prey, -1f);

        generator.families = families.ToArray();
        return nests;
    }

    // Solo advierte (no reubica) — el ajuste fino de posiciones en este archivo siempre se hace
    // a mano, igual que el resto del blockout. Solo evalúa a Bunny (SmallHomeRadius): es la
    // única especie cuyo homeRadius es más chico que el mapa, así que es la única separación
    // presa/depredador realmente alcanzable aquí (ver nota en BuildWildlifePopulation).
    static void ValidateNestSeparation(System.Collections.Generic.List<NestSpec> nests)
    {
        foreach (NestSpec prey in nests)
        {
            if (prey.kind != NestKind.Prey || !SmallHomeRadius.TryGetValue(prey.species, out float homeRadius)) continue;

            float minBuffer = homeRadius + 40f;
            foreach (NestSpec predator in nests)
            {
                if (predator.kind != NestKind.Predator) continue;
                float dist = Vector3.Distance(prey.position, predator.position);
                if (dist < minBuffer)
                    Debug.LogWarning($"[SampleSceneBuilder] Nido de {prey.species} en {prey.position} queda a {dist:F0}u de {predator.species} " +
                        $"(mínimo recomendado {minBuffer:F0}u) — considerar reubicar a mano.");
            }
        }
    }

    // Áreas de pasto para los herbívoros terrestres — Herbivore.Feed() camina hasta la más
    // cercana antes de comer (ver Herbivore.cs/GrassPatch.cs). Una por cada nido de presa
    // terrestre (Bunny/Deer) para que ningún nido quede sin comida cerca tras el reparto por
    // bandas — antes eran 3 parches fijos pensados para las posiciones viejas, amontonadas.
    // Los marinos (Whale, Seal; height < 0) usan FishSchool en vez de esto — ver BuildFishSchools().
    static void BuildGrassPatches(Transform parent, System.Collections.Generic.List<NestSpec> nests)
    {
        GameObject group = new GameObject("GrassPatches_AUTO");
        group.transform.SetParent(parent);

        Material grassMat = MakeMaterial("GrassPatch_MAT", new Color(0.45f, 0.6f, 0.3f));
        int i = 0;
        foreach (NestSpec nest in nests)
        {
            if (nest.kind != NestKind.Prey || nest.height < 0f) continue;

            GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            patch.name = $"GrassPatch_{i}";
            patch.transform.SetParent(group.transform);
            patch.transform.position = nest.position + new Vector3(0f, 0.03f, 0f);
            float size = Mathf.Max(14f, nest.radius * 1.8f);
            patch.transform.localScale = new Vector3(size, 0.06f, size);
            patch.GetComponent<Renderer>().sharedMaterial = grassMat;
            Object.DestroyImmediate(patch.GetComponent<Collider>()); // decorativo — no debe bloquear NavMesh/jugador
            patch.AddComponent<GrassPatch>();
            i++;
        }
    }

    // Equivalente marino de BuildGrassPatches() — cardúmenes que Whale/Seal buscan antes de
    // comer (Herbivore.Feed() con GrazesOnLand == false). Repartidos cerca de donde viven esas
    // familias (Whale en 200,-1,15 radio 30; Seal en 180,-1,-20 radio 15, ver arriba).
    static void BuildFishSchools(Transform parent)
    {
        GameObject group = new GameObject("FishSchools_AUTO");
        group.transform.SetParent(parent);

        Vector3[] positions =
        {
            new Vector3(205f, -2f, 20f),
            new Vector3(185f, -2f, -15f),
        };

        Material fishMat = MakeMaterial("FishSchool_MAT", new Color(0.35f, 0.55f, 0.65f, 0.6f));
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject school = GameObject.CreatePrimitive(PrimitiveType.Cube);
            school.name = $"FishSchool_{i}";
            school.transform.SetParent(group.transform);
            school.transform.position = positions[i];
            school.transform.localScale = new Vector3(10f, 2f, 10f);
            school.GetComponent<Renderer>().sharedMaterial = fishMat;
            Object.DestroyImmediate(school.GetComponent<Collider>()); // decorativo — no debe bloquear nado/NavMesh
            school.AddComponent<FishSchool>();
        }
    }

    // Cobertura vegetal decorativa alrededor de cada nido de presa terrestre — para que "se
    // vean un poco escondidos" (pedido explícito). Puramente visual: primitivas sin collider
    // (igual que GrassPatch/FishSchool), sin componente de lógica — NO implementa detección ni
    // ocultamiento real, eso queda documentado como pendiente en
    // docs/refuge-and-adult-behavior.md ("empezar documentando, luego implementar").
    static void BuildHabitatCover(Transform parent, System.Collections.Generic.List<NestSpec> nests)
    {
        GameObject group = new GameObject("HabitatCover_AUTO");
        group.transform.SetParent(parent);

        Material trunkMat = MakeMaterial("Tree_Trunk_MAT", new Color(0.35f, 0.24f, 0.15f));
        Material canopyMat = MakeMaterial("Tree_Canopy_MAT", new Color(0.20f, 0.42f, 0.22f));
        Material bushMat = MakeMaterial("Bush_MAT", new Color(0.18f, 0.35f, 0.18f));

        int propIndex = 0;
        foreach (NestSpec nest in nests)
        {
            if (nest.kind != NestKind.Prey || nest.height < 0f) continue; // solo presas terrestres

            int propCount = Random.Range(3, 6);
            for (int i = 0; i < propCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * (nest.radius * 1.3f);
                Vector3 pos = nest.position + new Vector3(offset.x, 0f, offset.y);
                if (Random.value > 0.5f) BuildTree(group.transform, pos, trunkMat, canopyMat, $"Tree_{propIndex}");
                else BuildBush(group.transform, pos, bushMat, $"Bush_{propIndex}");
                propIndex++;
            }
        }
    }

    static void BuildTree(Transform parent, Vector3 pos, Material trunkMat, Material canopyMat, string name)
    {
        GameObject tree = new GameObject(name);
        tree.transform.SetParent(parent);
        tree.transform.position = pos;

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform);
        float height = Random.Range(3f, 5f);
        trunk.transform.localPosition = new Vector3(0f, height / 2f, 0f);
        trunk.transform.localScale = new Vector3(0.4f, height / 2f, 0.4f);
        trunk.GetComponent<Renderer>().sharedMaterial = trunkMat;
        Object.DestroyImmediate(trunk.GetComponent<Collider>());

        GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.name = "Canopy";
        canopy.transform.SetParent(tree.transform);
        canopy.transform.localPosition = new Vector3(0f, height + 1f, 0f);
        canopy.transform.localScale = new Vector3(2.6f, 2.2f, 2.6f);
        canopy.GetComponent<Renderer>().sharedMaterial = canopyMat;
        Object.DestroyImmediate(canopy.GetComponent<Collider>());
    }

    static void BuildBush(Transform parent, Vector3 pos, Material bushMat, string name)
    {
        GameObject bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bush.name = name;
        bush.transform.SetParent(parent);
        bush.transform.position = pos + new Vector3(0f, 0.5f, 0f);
        bush.transform.localScale = new Vector3(1.8f, 1f, 1.8f);
        bush.GetComponent<Renderer>().sharedMaterial = bushMat;
        Object.DestroyImmediate(bush.GetComponent<Collider>());
    }

    // ── Birds ────────────────────────────────────────────────────────────────

    // No bird model/prefab exists yet in the project — these are primitive
    // placeholders, just enough for BirdBehavior.population to be non-empty.
    // LifeStage.Wander() (land animals) reads that population to pick a wander
    // target, and silently no-ops if it's empty; it doesn't crash anymore either
    // way (see LifeStage.cs fix), but without real birds every land animal's
    // Wander event is a permanent no-op. Full design + known issues with this
    // whole bird subsystem are written up in docs/bird-logic.md — read that
    // before spending real time on birds, since it's likely to get replaced.
    static void BuildBirds(Transform parent)
    {
        GameObject birdsGroup = new GameObject("Birds_Placed");
        birdsGroup.transform.SetParent(parent);

        for (int i = 0; i < 6; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-20f, 20f), Random.Range(4f, 10f), Random.Range(-20f, 20f));
            GameObject bird = MakePlaceholderBird($"Bird_{i}", pos);
            bird.transform.SetParent(birdsGroup.transform);
            BirdBehavior.population.Add(bird);
            Respawn.birds.Add(bird);
        }
    }

    static GameObject MakePlaceholderBird(string name, Vector3 pos)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.3f, 0.2f, 0.5f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_MAT", new Color(0.25f, 0.25f, 0.3f));
        Object.DestroyImmediate(go.GetComponent<Collider>()); // decorative only — must not block the player/NavMesh

        BirdBehavior behavior = go.AddComponent<BirdBehavior>();
        behavior.min = 4;
        behavior.max = 6;
        behavior.speed = 3f;
        behavior.tripTime = 120f;

        return go;
    }

    // ── Characters at orientative posts ─────────────────────────────────────

    static GameObject MakePlaceholderPerson(string name, Vector3 pos, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.position = pos;
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_MAT", color);
        return go;
    }

    static void BuildCharacters(Transform parent)
    {
        GameObject charactersGroup = new GameObject("Characters_Placed");
        charactersGroup.transform.SetParent(parent);

        GameObject maestraAsset = LoadAndImport($"{CharactersRoot}/Maestra/Models/Maestra.fbx");
        GameObject maestra = maestraAsset != null
            ? (GameObject)PrefabUtility.InstantiatePrefab(maestraAsset, charactersGroup.transform)
            : MakePlaceholderPerson("Maestra_Post", new Vector3(50f, 1f, -10f), new Color(0.9f, 0.8f, 0.4f));
        maestra.name = "Maestra_Post";
        maestra.transform.SetParent(charactersGroup.transform);
        maestra.transform.position = new Vector3(50f, 0f, -10f);
        maestra.AddComponent<MaestraTeacher>().teacherName = "Maestra";
        WorldCharacter magnateWC = maestra.AddComponent<WorldCharacter>();
        magnateWC.characterName = "Maestra";

        GameObject gohageneis = MakePlaceholderPerson("Gohageneis_Post", new Vector3(5f, 1f, -5f), new Color(0.95f, 0.5f, 0.2f));
        gohageneis.transform.SetParent(charactersGroup.transform);
        var goha = gohageneis.AddComponent<Gohageneis>();
        goha.companionName = "Gohageneis";
        gohageneis.AddComponent<WorldCharacter>().characterName = "Gohageneis";

        GameObject goluis = MakePlaceholderPerson("Goluis_Post", new Vector3(-5f, 1f, -5f), new Color(0.4f, 0.4f, 0.55f));
        goluis.transform.SetParent(charactersGroup.transform);
        var gol = goluis.AddComponent<Goluis>();
        gol.companionName = "Goluis";
        goluis.AddComponent<WorldCharacter>().characterName = "Goluis";

        GameObject panterilia = MakePlaceholderPerson("Panterilia_Post", new Vector3(0f, 1f, -12f), new Color(0.55f, 0.25f, 0.5f));
        panterilia.transform.SetParent(charactersGroup.transform);
        var pant = panterilia.AddComponent<Panterilia>();
        pant.companionName = "Panterilia";
        panterilia.AddComponent<WorldCharacter>().characterName = "Panterilia";

        // Irosene — compañera motivacional. Reside en el nivel submarino pero visita a Panterilia,
        // así que su puesto orientativo va junto a ella (ver docs/character-irosene.md).
        GameObject irosene = MakePlaceholderPerson("Irosene_Post", new Vector3(3f, 1f, -13f), new Color(0.90f, 0.55f, 0.40f));
        irosene.transform.SetParent(charactersGroup.transform);
        var iro = irosene.AddComponent<Irosene>();
        iro.companionName = "Irosene";
        irosene.AddComponent<WorldCharacter>().characterName = "Irosene";

        // The Magnate (Maestra) is the SanctuaryDirector's arrival gatekeeper.
        SanctuaryDirector director = parent.GetComponentInChildren<SanctuaryDirector>();
        if (director != null) director.magnateCharacter = magnateWC;
    }

    // ── Buildings ────────────────────────────────────────────────────────────

    static GameObject LoadAndImport(string path)
    {
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    // Las estructuras anteriores (ResearchStation.fbx, YogaStudio.obj, Kitchen.obj) se
    // reemplazaron por completo con hologramas... no, con cajas: pedido explícito del
    // usuario porque los modelos "se ven fatal". Cada área es una esterilla (caja plana en
    // el suelo) + 3 paredes en U (caja alta, un lado abierto de entrada) + un SanctuaryArea
    // real conectado a SanctuaryDirector — antes Director.allAreas estaba vacío y sin usar.
    // Los .fbx/.obj originales siguen en Assets/Environment/Buildings/ por si se quieren
    // volver a colocar a mano más adelante — esto no los borra, solo deja de instanciarlos.
    static SanctuaryArea[] BuildAreaStructures(Transform parent)
    {
        GameObject areasGroup = new GameObject("Areas_Placed");
        areasGroup.transform.SetParent(parent);

        var areas = new List<SanctuaryArea>
        {
            BuildAreaBox(areasGroup.transform, "Kitchen_Area", "Cocina", SanctuaryAreaType.Kitchen, 1,
                new Vector3(20f, 0f, 55f), new Vector2(10f, 9f), new Color(0.75f, 0.45f, 0.30f)),

            BuildAreaBox(areasGroup.transform, "Infirmary_Area", "Enfermería", SanctuaryAreaType.Infirmary, 1,
                new Vector3(45f, 0f, 55f), new Vector2(8f, 8f), new Color(0.85f, 0.95f, 0.90f)),

            BuildAreaBox(areasGroup.transform, "VeterinaryClinic_Area", "Veterinaria", SanctuaryAreaType.VeterinaryClinic, 1,
                new Vector3(70f, 0f, 55f), new Vector2(8f, 8f), new Color(0.55f, 0.75f, 0.70f)),

            BuildAreaBox(areasGroup.transform, "YogaRoom_Area", "Sala de Yoga", SanctuaryAreaType.YogaRoom, 1,
                new Vector3(70f, 0f, 25f), new Vector2(9f, 9f), new Color(0.75f, 0.65f, 0.85f)),

            BuildAreaBox(areasGroup.transform, "VehicleWorkshop_Area", "Mecánica", SanctuaryAreaType.VehicleWorkshop, 2,
                new Vector3(70f, 0f, -5f), new Vector2(10f, 9f), new Color(0.55f, 0.55f, 0.60f)),

            BuildAreaBox(areasGroup.transform, "TextileStudio_Area", "Estudio Textil", SanctuaryAreaType.TextileStudio, 2,
                new Vector3(45f, 0f, -20f), new Vector2(8f, 8f), new Color(0.85f, 0.60f, 0.65f)),

            BuildAreaBox(areasGroup.transform, "Garden_Area", "Huerto", SanctuaryAreaType.Garden, 2,
                new Vector3(20f, 0f, -5f), new Vector2(9f, 9f), new Color(0.50f, 0.70f, 0.40f)),
        };

        return areas.ToArray();
    }

    const float AreaWallHeight = 2.4f;
    const float AreaWallThickness = 0.3f;

    static SanctuaryArea BuildAreaBox(Transform parent, string name, string displayName,
        SanctuaryAreaType areaType, int progressionTier, Vector3 center, Vector2 size, Color matColor)
    {
        GameObject area = new GameObject(name);
        area.transform.SetParent(parent);
        area.transform.position = center;

        GameObject mat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mat.name = "Mat";
        mat.transform.SetParent(area.transform);
        mat.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        mat.transform.localScale = new Vector3(size.x, 0.1f, size.y);
        mat.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"{name}_Mat_MAT", matColor);

        // Paredes en U — entrada abierta hacia +Z, para no tener que animar una puerta.
        Material wallMat = MakeMaterial($"{name}_Wall_MAT", Color.Lerp(matColor, Color.white, 0.35f));
        CreateWallSegment(area.transform, "Wall_Back",
            new Vector3(0f, AreaWallHeight / 2f, -size.y / 2f), new Vector3(size.x, AreaWallHeight, AreaWallThickness), wallMat);
        CreateWallSegment(area.transform, "Wall_Left",
            new Vector3(-size.x / 2f, AreaWallHeight / 2f, 0f), new Vector3(AreaWallThickness, AreaWallHeight, size.y), wallMat);
        CreateWallSegment(area.transform, "Wall_Right",
            new Vector3(size.x / 2f, AreaWallHeight / 2f, 0f), new Vector3(AreaWallThickness, AreaWallHeight, size.y), wallMat);

        CreateAreaLabel(area.transform, displayName, new Vector3(0f, AreaWallHeight + 0.4f, 0f));

        SanctuaryArea sanctuaryArea = area.AddComponent<SanctuaryArea>();
        sanctuaryArea.areaType = areaType;
        sanctuaryArea.displayName = displayName;
        sanctuaryArea.progressionTier = progressionTier;
        return sanctuaryArea;
    }

    static void CreateWallSegment(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.localPosition = localPos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().sharedMaterial = mat;
    }

    static void CreateAreaLabel(Transform parent, string text, Vector3 localPos)
    {
        GameObject canvasGO = new GameObject("Label_Canvas", typeof(Canvas));
        canvasGO.transform.SetParent(parent);
        canvasGO.transform.localPosition = localPos;
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = Vector3.one * 0.03f;
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(240f, 60f);

        GameObject textGO = new GameObject("Text", typeof(TextMeshProUGUI));
        textGO.transform.SetParent(canvasGO.transform, false);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    static void WireSanctuaryDirector(SanctuaryArea[] areas)
    {
        SanctuaryDirector director = Object.FindAnyObjectByType<SanctuaryDirector>();
        if (director == null)
        {
            Debug.LogWarning("[SampleSceneBuilder] No hay SanctuaryDirector todavia — las areas no quedan registradas (BuildWorldSystems debe correr antes).");
            return;
        }
        director.allAreas = areas;
    }

    // ── UI scaffold ──────────────────────────────────────────────────────────

    static void BuildUI(Transform parent)
    {
        // Buscar por nombre, no por tipo: CreateAreaLabel() ya puso un Canvas world-space
        // ("Label_Canvas") sobre cada una de las 7 areas antes de que este metodo corra, asi
        // que FindAnyObjectByType<Canvas>() encuentra uno de esos en vez de a este y lo
        // confunde con "ya hay un Canvas" — dejando el Canvas_AUTO de pantalla sin crear.
        if (GameObject.Find("Canvas_AUTO") != null)
        {
            Debug.Log("[SampleSceneBuilder] Ya existe un Canvas_AUTO en la escena — no se toca.");
            return;
        }

        GameObject canvasGO = new GameObject("Canvas_AUTO", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        canvasGO.transform.SetParent(parent);
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem_AUTO",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        Debug.Log("[SampleSceneBuilder] Canvas + EventSystem minimos creados.");

        BuildConfirmationPanel(canvasGO.transform);
    }


    // ── Holographic menu (Menú → Magia / Yoga / Sistema) ────────────────────────
    // Ver docs/ui-holographic-menu.md para el diseno completo. Sistema de UI propio en
    // pantalla (screen-space, no world-space) — no reutiliza FollowingArrays/UI (ese sigue
    // intacto para el radar de animales, ver ui-following-arrays.md), porque los requisitos
    // cambiaron a "hologramas planos, chicos, anclados a una esquina, pool reutilizable" que
    // no encajan con el patron de caja 3D + Collider de FollowingArrays.

    static void BuildHolographicMenu(Transform parent)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("[SampleSceneBuilder] No hay Player todavia — el menu holografico se omite.");
            return;
        }

        AsanaDetector asanaDetector = player.GetComponent<AsanaDetector>();

        // Limpieza del sistema world-space anterior (cajas 3D flotando frente al jugador,
        // superadas por este menu en pantalla) — MenuAnchor colgaba de CameraPivot, que es
        // persistente y no se destruye/recrea como LevelDressing_AUTO, asi que se queda
        // huerfano si no se borra a mano.
        Transform oldAnchor = player.transform.Find("CameraPivot/MenuAnchor");
        if (oldAnchor != null) Object.DestroyImmediate(oldAnchor.gameObject);

        GameObject existingMenu = GameObject.Find("HologramMenu_AUTO");
        if (existingMenu != null) Object.DestroyImmediate(existingMenu);

        // Por nombre, no por tipo — mismo motivo que en BuildUI(): con los Label_Canvas
        // world-space de las areas ya en la escena, FindAnyObjectByType<Canvas>() puede
        // devolver cualquiera de esos en vez del Canvas_AUTO de pantalla, y el menu entero
        // terminaria colgado del cartel de una sola area en vez de anclado a la pantalla.
        GameObject canvasGO = GameObject.Find("Canvas_AUTO");
        if (canvasGO == null)
        {
            Debug.LogWarning("[SampleSceneBuilder] No hay Canvas_AUTO todavia — el menu holografico se omite (BuildUI debe correr antes).");
            return;
        }

        GameObject menuRoot = new GameObject("HologramMenu_AUTO");
        menuRoot.transform.SetParent(canvasGO.transform, false);
        RectTransform menuRootRect = menuRoot.AddComponent<RectTransform>();
        menuRootRect.anchorMin = Vector2.zero;
        menuRootRect.anchorMax = Vector2.one;
        menuRootRect.offsetMin = Vector2.zero;
        menuRootRect.offsetMax = Vector2.zero;

        HologramPool pool = menuRoot.AddComponent<HologramPool>();
        pool.canvasRoot = menuRootRect;
        pool.poolSize = 118; // tamano completo de la tabla periodica — el mayor listado que el menu necesita mostrar a la vez

        HologramMenuController controller = menuRoot.AddComponent<HologramMenuController>();
        controller.pool = pool;
        controller.asanaDetector = asanaDetector;
        controller.bodyPositions = BuildBodyPositionList();
        controller.volumeSlider = CreateVolumeSlider(menuRootRect);

        Debug.Log("[SampleSceneBuilder] Menu holografico listo — clic en el holograma 'Menú' (esquina inferior derecha) abre Magia/Yoga/Sistema.");
    }

    static List<BodyPosition> BuildBodyPositionList()
    {
        // Teclas elegidas para no chocar con WASD/Shift/Space/J-L-I-K/1-0/Tab/Enter/V/Escape/Z-X-C-V-B.
        // Ver docs/ui-holographic-menu.md para la tabla completa y la colision conocida de V.
        (BodyPart part, string description, KeyCode hotkey)[] defs =
        {
            (BodyPart.Elbows,    "Codos",    KeyCode.Q),
            (BodyPart.Hands,     "Manos",    KeyCode.E),
            (BodyPart.Knees,     "Rodillas", KeyCode.R),
            (BodyPart.Feet,      "Pies",     KeyCode.T),
            (BodyPart.Hips,      "Caderas",  KeyCode.Y),
            (BodyPart.Back,      "Espalda",  KeyCode.U),
            (BodyPart.Shoulders, "Hombros",  KeyCode.O),
            (BodyPart.Head,      "Cabeza",   KeyCode.P),
        };

        List<BodyPosition> list = new List<BodyPosition>();
        foreach ((BodyPart part, string description, KeyCode hotkey) in defs)
            list.Add(new BodyPosition(part, description, hotkey));
        return list;
    }

    static UnityEngine.UI.Slider CreateVolumeSlider(RectTransform parent)
    {
        GameObject sliderGO = new GameObject("VolumeSlider_AUTO", typeof(RectTransform), typeof(UnityEngine.UI.Slider));
        sliderGO.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(140f, 28f);

        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        bgGO.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgGO.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f, 0.2f);

        GameObject fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillGO.GetComponent<UnityEngine.UI.Image>().color = new Color(0.3f, 0.85f, 0.95f, 0.9f);

        UnityEngine.UI.Slider slider = sliderGO.GetComponent<UnityEngine.UI.Slider>();
        slider.fillRect = fillRect;
        slider.targetGraphic = bgGO.GetComponent<UnityEngine.UI.Image>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        sliderGO.SetActive(false);
        return slider;
    }

    // ── Confirmation panel ────────────────────────────────────────────────────
    //
    // Simple yes/no modal — used by KitchenEntrance and any future area gate.
    // Created inside Canvas_AUTO and wired to ConfirmationPanel MonoBehaviour.

    static void BuildConfirmationPanel(Transform canvasParent)
    {
        if (GameObject.Find("ConfirmationPanel_AUTO") != null) return;

        // Root: dark semi-transparent background
        GameObject panelGO = new GameObject("ConfirmationPanel_AUTO",
            typeof(RectTransform),
            typeof(UnityEngine.UI.Image),
            typeof(CanvasGroup));
        panelGO.transform.SetParent(canvasParent, worldPositionStays: false);

        RectTransform panelRect  = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin       = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax       = new Vector2(0.5f, 0.5f);
        panelRect.pivot           = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta       = new Vector2(420f, 210f);
        panelRect.anchoredPosition = Vector2.zero;

        panelGO.GetComponent<UnityEngine.UI.Image>().color = new Color(0.04f, 0.04f, 0.10f, 0.92f);

        ConfirmationPanel cp = panelGO.AddComponent<ConfirmationPanel>();

        // Title text (centred, top third)
        cp.titleText   = CreateConfirmText(panelGO.transform, "Title",   new Vector2(0f,  68f), 22, "Área");
        // Message text (centred, middle)
        cp.messageText = CreateConfirmText(panelGO.transform, "Message", new Vector2(0f,  18f), 16, "¿Empezar turno?");
        // Hint: Enter / Escape
        CreateConfirmText(panelGO.transform, "Hint",
            new Vector2(0f, -22f), 12, "Enter → Confirmar   Esc → Cancelar")
            .color = new Color(0.7f, 0.7f, 0.7f);

        // Confirm button (green-ish, left of centre)
        (UnityEngine.UI.Button cb, UnityEngine.UI.Text cl) =
            CreateConfirmButton(panelGO.transform, "ConfirmBtn",
                new Vector2(-95f, -68f), "Entrar", new Color(0.18f, 0.55f, 0.28f));
        cp.confirmButton = cb;
        cp.confirmLabel  = cl;

        // Cancel button (red-ish, right of centre)
        (UnityEngine.UI.Button xb, UnityEngine.UI.Text xl) =
            CreateConfirmButton(panelGO.transform, "CancelBtn",
                new Vector2(95f, -68f), "Cancelar", new Color(0.50f, 0.14f, 0.14f));
        cp.cancelButton = xb;
        cp.cancelLabel  = xl;

        Debug.Log("[SampleSceneBuilder] ConfirmationPanel creado en Canvas_AUTO.");
    }

    static UnityEngine.UI.Text CreateConfirmText(Transform parent, string name,
        Vector2 anchoredPos, int fontSize, string content)
    {
        GameObject go = new GameObject(name,
            typeof(RectTransform), typeof(UnityEngine.UI.Text));
        go.transform.SetParent(parent, worldPositionStays: false);

        RectTransform rt   = go.GetComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(380f, 38f);
        rt.anchoredPosition = anchoredPos;

        UnityEngine.UI.Text t = go.GetComponent<UnityEngine.UI.Text>();
        t.text      = content;
        t.fontSize  = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color     = Color.white;
        return t;
    }

    static (UnityEngine.UI.Button btn, UnityEngine.UI.Text lbl) CreateConfirmButton(
        Transform parent, string name, Vector2 anchoredPos, string label, Color bgColor)
    {
        GameObject go = new GameObject(name,
            typeof(RectTransform),
            typeof(UnityEngine.UI.Image),
            typeof(UnityEngine.UI.Button));
        go.transform.SetParent(parent, worldPositionStays: false);

        RectTransform rt   = go.GetComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(160f, 46f);
        rt.anchoredPosition = anchoredPos;

        go.GetComponent<UnityEngine.UI.Image>().color = bgColor;

        UnityEngine.UI.Text lbl = CreateConfirmText(go.transform, "Label", Vector2.zero, 16, label);
        UnityEngine.UI.Button btn = go.GetComponent<UnityEngine.UI.Button>();
        return (btn, lbl);
    }

    // ── Microcosmos (reemplaza la entrada legacy de la cocina) ──────────────────
    //
    // Dos únicas vías de entrada al mundo mob: la VirtualizationMachine (por área) o el hechizo de
    // loto (LotusMeditationAbility en el jugador). Ya no hay puerta. La cocina legacy
    // (KitchenScaleController/KitchenEntrance + BuildKitchenContent/MakeMobBox/
    // CreateFragmentPrefabIfNeeded) se retiró 2026-07-23: superada por VirtualizationMachine +
    // RealityShiftController + MobWorldLoader.

    enum MissionKind { KitchenChannel, GardenChannel, Channel, Heal, Yoga }

    static void BuildMeditationContent(Transform parent)
    {
        WireMeditationArea(parent, "Kitchen_Area",          "Cocina",         new Color(0.75f, 0.45f, 0.30f), MissionKind.KitchenChannel, MobWorldSceneBuilder.SceneName); // modo escena → mundo mob propio
        WireMeditationArea(parent, "YogaRoom_Area",         "Sala de Yoga",   new Color(0.75f, 0.65f, 0.85f), MissionKind.Yoga);
        WireMeditationArea(parent, "Garden_Area",           "Huerto",         new Color(0.50f, 0.70f, 0.40f), MissionKind.GardenChannel);
        WireMeditationArea(parent, "Infirmary_Area",        "Enfermería",     new Color(0.85f, 0.95f, 0.90f), MissionKind.Heal);
        WireMeditationArea(parent, "VeterinaryClinic_Area", "Veterinaria",    new Color(0.55f, 0.75f, 0.70f), MissionKind.Heal);
        WireMeditationArea(parent, "TextileStudio_Area",    "Estudio Textil", new Color(0.85f, 0.60f, 0.65f), MissionKind.Channel);
        WireMeditationArea(parent, "VehicleWorkshop_Area",  "Mecánica",       new Color(0.55f, 0.55f, 0.60f), MissionKind.Channel);

        // Segunda vía de entrada: el hechizo de loto (arranca bloqueado; se activa al aprenderlo).
        GameObject player = GameObject.Find("Player");
        if (player != null && player.GetComponent<LotusMeditationAbility>() == null)
        {
            var lotus = player.AddComponent<LotusMeditationAbility>();
            lotus.hasMobMissionSpell = false; // shift/missions se cablean por contexto al desbloquearlo
        }

        // Director del mundo mob (eventos que hacen avanzar las eras). Auto-bootstrap en runtime,
        // pero lo dejamos en escena para poder configurarlo (autoEvents off = eventos por historia).
        if (Object.FindObjectOfType<MobWorldDirector>() == null)
        {
            GameObject dirGO = new GameObject("MobWorldDirector");
            dirGO.transform.SetParent(parent);
            dirGO.AddComponent<MobWorldDirector>();
        }

        Debug.Log("[SampleSceneBuilder] Microcosmos cableado: VirtualizationMachine por área + loto en el jugador.");
    }

    static void WireMeditationArea(Transform parent, string areaName, string displayName, Color tint,
                                    MissionKind kind, string mobSceneName = null)
    {
        GameObject area = GameObject.Find(areaName);
        if (area == null)
        {
            Debug.LogWarning($"[SampleSceneBuilder] {areaName} no encontrada — Microcosmos omitido para esa área.");
            return;
        }

        // Máquina de virtualización en la entrada abierta (+Z).
        GameObject machineGO = new GameObject($"{areaName}_VirtualizationMachine");
        machineGO.transform.SetParent(area.transform, worldPositionStays: false);
        machineGO.transform.localPosition = new Vector3(0f, 1f, 4.2f);
        BoxCollider bc = machineGO.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(3f, 3f, 1.5f);
        VirtualizationMachine machine = machineGO.AddComponent<VirtualizationMachine>();
        machine.areaName = displayName;

        // MODO ESCENA: la máquina carga el mundo mob propio (sin shift/misiones in-place).
        if (!string.IsNullOrEmpty(mobSceneName))
        {
            machine.mobWorldSceneName = mobSceneName;
            return;
        }

        // MODO IN-PLACE: root de entorno que el reality shift escala ×8 (decor "gigante" tras el negro).
        GameObject mobRoot = new GameObject($"{areaName}_MobRoot");
        mobRoot.transform.SetParent(area.transform, worldPositionStays: false);
        for (int i = 0; i < 3; i++)
        {
            GameObject giant = GameObject.CreatePrimitive(PrimitiveType.Cube);
            giant.name = $"Giant_{i}";
            giant.transform.SetParent(mobRoot.transform);
            giant.transform.localPosition = new Vector3((i - 1) * 2f, 0.5f, -2f);
            giant.transform.localScale = Vector3.one * 0.6f;
            Collider gcol = giant.GetComponent<Collider>(); if (gcol != null) Object.DestroyImmediate(gcol);
            giant.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"{areaName}_giant{i}_MAT", Color.Lerp(tint, Color.white, 0.2f));
        }

        GameObject shiftGO = new GameObject($"{areaName}_ShiftSystem");
        shiftGO.transform.SetParent(area.transform, worldPositionStays: false);
        RealityShiftController shift = shiftGO.AddComponent<RealityShiftController>();
        shift.environmentRoot = mobRoot.transform;
        // playerCamera queda null → RealityShiftController usa Camera.main

        machine.shift = shift;
        machine.missions = BuildAreaMissions(parent, area.transform, displayName, tint, kind);
    }

    static MobMission[] BuildAreaMissions(Transform parent, Transform areaT, string displayName, Color tint, MissionKind kind)
    {
        if (kind == MissionKind.Yoga)
        {
            MobMission visualize = MakeMissionGO(areaT, "Mission_Visualizar", "Visualizar postura",
                "Huye de los pensamientos hasta disolverlos.", MissionCategory.Visualization);
            var pv = visualize.gameObject.AddComponent<PostureVisualizationMission>();
            pv.mission = visualize; pv.targetCount = 5; pv.reward.observationGain = 0.25f;

            MobMission form = MakeMissionGO(areaT, "Mission_Formar", "Formar posturas",
                "Persigue y sostén cada postura.", MissionCategory.Visualization);
            var af = form.gameObject.AddComponent<AsanaFormationMission>();
            af.mission = form; af.targetCount = 3; af.reward.observationGain = 0.25f;

            MobMission root = MakeMissionGO(areaT, "Mission_Raiz", "Buscar la raíz",
                "No puedes huir; ve a su raíz.", MissionCategory.RootInquiry);
            var ri = root.gameObject.AddComponent<RootInquiryMission>();
            ri.mission = root; ri.targetCount = 3; ri.reward.observationGain = 0.3f;

            return new[] { visualize, form, root };
        }

        if (kind == MissionKind.Heal)
        {
            MobMission heal = MakeMissionGO(areaT, "Mission_Curar", $"Curar en {displayName}",
                "Quédate cerca hasta sanar a cada uno.", MissionCategory.LovingKindness);
            var hm = heal.gameObject.AddComponent<HealingMission>();
            hm.mission = heal; hm.targetCount = 4; hm.placeholderColor = Color.Lerp(tint, Color.white, 0.1f);
            hm.reward.observationGain = 0.2f; hm.reward.coinReward = 5;
            return new[] { heal };
        }

        // Variantes de Channel (Kitchen / Garden / Textile / Workshop)
        string name = kind == MissionKind.KitchenChannel ? "Procesar ingredientes"
                    : kind == MissionKind.GardenChannel  ? "Limpiar la plaga"
                    : $"Atender en {displayName}";
        MobMission ch = MakeMissionGO(areaT, "Mission_Canalizar", name,
            "Acércate y mantente presente hasta resolverlos.", MissionCategory.NoThinking);
        var cm = ch.gameObject.AddComponent<ChannelMission>();
        cm.mission = ch; cm.targetCount = 4; cm.placeholderColor = Color.Lerp(tint, Color.white, 0.1f);
        cm.reward.observationGain = 0.2f; cm.reward.coinReward = 5;

        if (kind == MissionKind.KitchenChannel)
            ch.mobSet = BuildKitchenMobHub(parent, areaT); // ciudad-insecto mínima: tiendas + portal

        return new[] { ch };
    }

    static MobMission MakeMissionGO(Transform areaT, string goName, string missionName, string desc, MissionCategory cat)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(areaT, worldPositionStays: false);
        MobMission mm = go.AddComponent<MobMission>();
        mm.missionName = missionName;
        mm.description = desc;
        mm.category    = cat;
        mm.isAvailable = true;
        return mm;
    }

    // Ciudad-insecto mínima = MESOPOTAMIA del amanecer: chozas de adobe (estructura representativa de
    // la era; castillos/rascacielos quedan deseables para eras superiores) + yoga-portal de salida.
    // Escala mundo (NO bajo el MobRoot escalado); inactiva hasta que la misión la activa (mobSet).
    static GameObject BuildKitchenMobHub(Transform parent, Transform areaT)
    {
        GameObject hub = new GameObject("Kitchen_MobHub_Mesopotamia");
        hub.transform.SetParent(parent);
        hub.transform.position = areaT.position;

        Color mud = new Color(0.72f, 0.55f, 0.36f); // adobe

        // Anclas de piedra en sus edificios de dominio (prototipo: 3; el resto al reskinear).
        AddResidentWithHut(hub.transform, "Guardián del Fuego", "El Hogar",   new Vector3(0f, 1f, -2.5f), mud, new Color(0.95f, 0.50f, 0.20f));
        AddResidentWithHut(hub.transform, "El Tallador",        "La Fragua",  new Vector3(3f, 1f,  0f),   mud, new Color(0.60f, 0.60f, 0.65f));
        AddResidentWithHut(hub.transform, "La Recolectora",     "El Granero", new Vector3(-3f, 1f, 0f),   mud, new Color(0.50f, 0.70f, 0.35f));

        // Yoga-portal de salida.
        GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "YogaPortal";
        portal.transform.SetParent(hub.transform);
        portal.transform.localPosition = new Vector3(0f, 1f, 3f);
        portal.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = MakeMaterial("YogaPortal_MAT", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<YogaPortal>();

        hub.SetActive(false); // RealityShiftController lo activa al entrar
        return hub;
    }

    // Una choza de adobe (estructura sólida) + su habitante delante de la puerta (trigger, interactuable).
    static void AddResidentWithHut(Transform parentT, string residentName, string role, Vector3 localPos,
                                    Color hutColor, Color bodyColor)
    {
        GameObject hut = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hut.name = $"Hut_{residentName}";
        hut.transform.SetParent(parentT);
        hut.transform.localPosition = localPos;
        hut.transform.localScale = new Vector3(2.2f, 2f, 2.2f);
        hut.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"Hut_{residentName}_MAT", hutColor);

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = residentName;
        go.transform.SetParent(parentT);
        go.transform.localPosition = localPos + new Vector3(0f, 0f, 1.6f); // frente a la choza
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"{residentName}_MAT", bodyColor);
        go.GetComponent<Collider>().isTrigger = true;
        MobResident res = go.AddComponent<MobResident>();
        res.residentName = residentName;
        res.role = role;
    }
}
