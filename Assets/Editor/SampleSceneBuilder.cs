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
        BuildWildlifePopulation(root.transform);
        BuildGrassPatches(root.transform);
        BuildFishSchools(root.transform);
        BuildBirds(root.transform);
        BuildWorldSystems(root.transform);
        BuildCharacters(root.transform);
        SanctuaryArea[] areas = BuildAreaStructures(root.transform);
        WireSanctuaryDirector(areas);
        BuildUI(root.transform);
        EnsurePlayerSystems();
        BuildHolographicMenu(root.transform);
        BuildKitchenContent(root.transform);
        BakeNavMesh();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SampleSceneBuilder] Blockout listo bajo 'LevelDressing_AUTO'. Revisar y ajustar posiciones a mano.");
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
        ground.transform.localScale = new Vector3(25f, 1f, 25f); // ~250x250 units
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
        // PlayerController cuando el jugador entra/sale.
        GameObject swimZone = new GameObject("Sea_SwimZone");
        swimZone.transform.SetParent(parent);
        swimZone.transform.position = new Vector3(200f, -8f, 0f);
        BoxCollider swimCollider = swimZone.AddComponent<BoxCollider>();
        swimCollider.isTrigger = true;
        swimCollider.size = new Vector3(380f, 14f, 380f); // cubre el area visual del mar (plane 400x400) con margen
        swimZone.AddComponent<WaterZone>();

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
        string[] nestSpecies = { "Bunny", "Bunny", "Fox", "Wolf", "Husky" };
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

    // Bioma balanceado ~70% herbívoros / 30% carnívoros (aprobado con el usuario):
    // terrestres Bunny(5) x4=20 + Deer(6) x2=12 = 32 herbívoros vs Wolf(6) + Fox(7) =
    // 13 carnívoros → 32/45 ≈ 71% herbívoro. Los marinos (Whale, Seal) van aparte —
    // no compiten por el mismo territorio que los terrestres, así que no entran en
    // esa proporción. Cada entrada es una familia SEPARADA (no una manada única) para
    // que se vean grupos familiares distintos repartidos por el área en vez de un
    // solo clúster gigante. A diferencia del resto del blockout (que instancia en
    // tiempo de Editor), FamilyGenerator.Start() genera recién al entrar en Play —
    // el área se ve vacía en el Editor hasta que se corre el juego.
    static void BuildWildlifePopulation(Transform parent)
    {
        GameObject go = new GameObject("WildlifePopulation_AUTO");
        go.transform.SetParent(parent);
        FamilyGenerator generator = go.AddComponent<FamilyGenerator>();

        var families = new System.Collections.Generic.List<FamilyGenerator.Family>();
        void AddFamily(string species, Vector3 pos, float radius, float height = 0f)
        {
            GameObject prefab = LoadAnimalPrefab(species);
            if (prefab == null)
            {
                Debug.LogWarning($"[SampleSceneBuilder] WildlifePopulation: no se encontro el prefab de {species}, se omite esa familia.");
                return;
            }
            families.Add(new FamilyGenerator.Family { animalPrefab = prefab, position = pos, radius = radius, renderHeight = height });
        }

        // Herbívoros — base de la pirámide
        AddFamily("Bunny", new Vector3(-90f, 0f, 30f), 8f);
        AddFamily("Bunny", new Vector3(-70f, 0f, 40f), 8f);
        AddFamily("Bunny", new Vector3(-50f, 0f, 25f), 8f);
        AddFamily("Bunny", new Vector3(-85f, 0f, -10f), 8f);
        AddFamily("Deer", new Vector3(-60f, 0f, -20f), 12f);
        AddFamily("Deer", new Vector3(-95f, 0f, 15f), 12f);

        // Carnívoros — depredadores tope, posicionados más al margen del área
        AddFamily("Wolf", new Vector3(-75f, 0f, -35f), 14f);
        AddFamily("Fox", new Vector3(-40f, 0f, -30f), 12f);

        // Oso Polar — apex predator, territorio amplio (BearBehaviour.homeRadius=300)
        // posicionado hacia el lado costero del bioma terrestre (más cerca de x=0, rumbo
        // a la zona marina en x≈180-200) ya que su Diet prioriza Seal > Bunny > Wolf.
        AddFamily("PolarBear", new Vector3(-15f, 0f, -50f), 22f);

        // Marinos — sobre el Sea_Placeholder, aparte del bioma terrestre
        AddFamily("Whale", new Vector3(200f, -1f, 15f), 30f, -1f);
        AddFamily("Seal", new Vector3(180f, -1f, -20f), 15f, -1f);

        generator.families = families.ToArray();
    }

    // Áreas de pasto para los herbívoros terrestres — Herbivore.Feed() camina hasta la más
    // cercana antes de comer (ver Herbivore.cs/GrassPatch.cs). Repartidas para cubrir el
    // territorio donde están las familias de Bunny/Deer de arriba (x: -95..-50, z: -35..40).
    // Los marinos (Whale, Seal) usan FishSchool en vez de esto — ver BuildFishSchools().
    static void BuildGrassPatches(Transform parent)
    {
        GameObject group = new GameObject("GrassPatches_AUTO");
        group.transform.SetParent(parent);

        Vector3[] positions =
        {
            new Vector3(-90f, 0f, 15f),
            new Vector3(-65f, 0f, 10f),
            new Vector3(-80f, 0f, -15f),
        };

        Material grassMat = MakeMaterial("GrassPatch_MAT", new Color(0.45f, 0.6f, 0.3f));
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            patch.name = $"GrassPatch_{i}";
            patch.transform.SetParent(group.transform);
            patch.transform.position = positions[i] + new Vector3(0f, 0.03f, 0f);
            patch.transform.localScale = new Vector3(18f, 0.06f, 18f);
            patch.GetComponent<Renderer>().sharedMaterial = grassMat;
            Object.DestroyImmediate(patch.GetComponent<Collider>()); // decorativo — no debe bloquear NavMesh/jugador
            patch.AddComponent<GrassPatch>();
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

    // ── Kitchen mobs ──────────────────────────────────────────────────────────
    //
    // Mobs start INACTIVE — KitchenScaleController activates them once the
    // miniaturization transition completes. The entrance trigger sits at the
    // open +Z side of the Kitchen U-shape (open toward the player).
    //
    // Fragment prefab is created on demand in Assets/Prefabs/ if it doesn't exist.

    static void BuildKitchenContent(Transform parent)
    {
        const string kitchenAreaName = "Kitchen_Area";
        GameObject kitchenArea = GameObject.Find(kitchenAreaName);
        if (kitchenArea == null)
        {
            Debug.LogWarning("[SampleSceneBuilder] Kitchen_Area no encontrada — mobs de cocina omitidos.");
            return;
        }

        // Content root: this is what KitchenScaleController scales up ×8
        GameObject contentRoot = new GameObject("Kitchen_Content");
        contentRoot.transform.SetParent(kitchenArea.transform, worldPositionStays: false);

        GameObject fragmentPrefab = CreateFragmentPrefabIfNeeded();

        // (ingredientName, elementSymbol, maxHealth, attackDamage, boxColor, canReproduce)
        (string n, string sym, float hp, float dmg, Color col, bool repro)[] defs =
        {
            ("Sal",        "Na",  60f,  8f, new Color(0.95f, 0.95f, 0.90f), false),
            ("Levadura",   "C",   80f,  6f, new Color(0.95f, 0.90f, 0.40f), true),
            ("Alliina",    "S",  100f, 12f, new Color(0.75f, 0.55f, 0.85f), false),
            ("Capsaicina", "C",  120f, 18f, new Color(0.95f, 0.25f, 0.20f), false),
        };

        // Positions relative to Kitchen_Content root (kitchen center)
        Vector3[] offsets =
        {
            new Vector3(-2.5f, 0.5f, -2.0f),
            new Vector3( 1.5f, 0.5f, -2.5f),
            new Vector3(-1.5f, 0.5f,  1.5f),
            new Vector3( 2.0f, 0.5f,  1.0f),
        };

        var mobList = new List<IngredientMob>();
        for (int i = 0; i < defs.Length; i++)
        {
            var d = defs[i];
            GameObject mobGO = MakeMobBox(d.n, offsets[i], d.col);
            mobGO.transform.SetParent(contentRoot.transform, worldPositionStays: false);

            IngredientMob mob  = mobGO.AddComponent<IngredientMob>();
            mob.ingredientName   = d.n;
            mob.elementSymbol    = d.sym;
            mob.maxHealth        = d.hp;
            mob.attackDamage     = d.dmg;
            mob.discoveryChance  = 0.4f;
            mob.canReproduce     = d.repro;
            mob.fragmentPrefab   = fragmentPrefab;
            // spawnPrefab: left null — Levadura spawning requires a prefab asset,
            // not a scene object. Wire it up manually in the Inspector once a
            // Levadura prefab is saved.

            mobGO.SetActive(false);   // KitchenScaleController activates on entry
            mobList.Add(mob);
        }

        // Scale system — KitchenScaleController lives on its own GO (no Collider needed;
        // entry is triggered programmatically by KitchenEntrance, not OnTriggerEnter).
        GameObject scaleSystemGO = new GameObject("Kitchen_ScaleSystem");
        scaleSystemGO.transform.SetParent(kitchenArea.transform, worldPositionStays: false);

        KitchenScaleController ksc = scaleSystemGO.AddComponent<KitchenScaleController>();
        ksc.kitchenRoot    = contentRoot.transform;
        ksc.ingredientMobs = mobList.ToArray();
        // playerCamera left null — KitchenScaleController.Awake() falls back to Camera.main

        // Entrance trigger — open +Z side of the U-shape (Kitchen size: 10×9).
        // This is an IInteractable, NOT an auto-trigger; InteractionController handles
        // proximity detection, prompt display, and dispatching to KitchenEntrance.Interact().
        GameObject entranceGO = new GameObject("Kitchen_EntranceTrigger");
        entranceGO.transform.SetParent(kitchenArea.transform, worldPositionStays: false);
        entranceGO.transform.localPosition = new Vector3(0f, 1f, 4.5f);

        BoxCollider bc = entranceGO.AddComponent<BoxCollider>();
        bc.isTrigger   = true;
        bc.size        = new Vector3(10f, 3f, 1.5f);

        KitchenEntrance entrance = entranceGO.AddComponent<KitchenEntrance>();
        entrance.kitchenScaleController = ksc;
        entrance.areaName = "Cocina";

        // Visual cue — slightly transparent box so the player can see the entrance marker
        MeshRenderer mr = entranceGO.GetComponent<MeshRenderer>();
        if (mr == null) mr = entranceGO.AddComponent<MeshRenderer>();
        // The BoxCollider IS the visual — a Cube primitive would add a redundant Collider,
        // so just tint the MeshRenderer that might have been auto-added. If you want a
        // visible mesh, swap this with GameObject.CreatePrimitive(PrimitiveType.Cube) + isTrigger.

        Debug.Log($"[SampleSceneBuilder] {mobList.Count} IngredientMobs (boxes) colocados en Kitchen_Content " +
                  "(inactivos hasta entrada). KitchenEntrance listo — interactúa con F o clic.");
    }

    static GameObject MakeMobBox(string mobName, Vector3 localOffset, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Mob_{mobName}";
        go.transform.localPosition = localOffset;
        go.transform.localScale    = new Vector3(0.8f, 1.0f, 0.8f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"Mob_{mobName}_MAT", color);
        // BoxCollider required by IngredientMob already added by CreatePrimitive
        return go;
    }

    static GameObject CreateFragmentPrefabIfNeeded()
    {
        const string path = "Assets/Prefabs/ElementFragment.prefab";

        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "ElementFragment";
        go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        go.GetComponent<Renderer>().sharedMaterial =
            MakeMaterial("Fragment_Pickup_MAT", new Color(0.30f, 0.90f, 0.70f));

        // Trigger so the player walks through it to pick it up (ElementFragment.OnTriggerEnter)
        BoxCollider col = go.GetComponent<BoxCollider>();
        col.isTrigger = true;

        go.AddComponent<ElementFragment>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        Debug.Log($"[SampleSceneBuilder] ElementFragment prefab guardado en {path}.");
        return prefab;
    }
}
