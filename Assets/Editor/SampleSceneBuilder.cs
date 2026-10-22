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
public static partial class SampleSceneBuilder
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
        BuildPhytoplankton(root.transform);   // productor base (primero: los enjambres lo pastan)
        BuildKrill(root.transform);           // krill: pasta fitoplancton, comido por pez/pingüino/ballena
        BuildSwarms(root.transform);          // peces: pastan krill, comidos por foca/oso/pingüino/orca
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
        // Las PRUEBAS (farming, mente, posesión, cocina, oficios, prólogo, yoga, cámara/emoción, magia, alma, fauna) ya no van
        // aquí: cada grupo tiene su propia escena en Assets/Scenes/Pruebas (menú «Build Test Scenes»). Aquí solo el santuario.
        // El MICROCOSMOS es OTRA ESCENA (no va aquí, en el mesocosmos): Scene1 (Ambrosio/alba) y Mesopotamia son
        // escenas HERMANAS del microcosmos. Se generan abajo con MicrocosmosSceneBuilder/MobWorldSceneBuilder.
        new GameObject("MigrationDiagnostics_AUTO").AddComponent<MigrationDiagnostics>().transform.SetParent(root.transform); // vuelca validación por consola en Play
        // Rejilla de rastros PERSISTENTE (singleton): sin ella, todo el sistema de trazas (celo, byproducts, wander por
        // olor) es no-op en juego. Vacía = no hace nada hasta que algo deposita. docs/environmental-navigation.md.
        new GameObject("TraceField_AUTO").AddComponent<TraceField>().transform.SetParent(root.transform);
        new GameObject("Clock_AUTO").AddComponent<Clock>().transform.SetParent(root.transform);  // reloj del día (hora inicial + tic); la velocidad corre las horas

        // Tests automáticos coordinados por TestRunner (grupos en serie; [TEST] en Editor.log). Las unidades ITestUnit
        // van en el MISMO GO; el TestRunner las reúne y ordena. Ver docs/testing-checklist.md §32.
        GameObject tests = new GameObject("TestRunner_AUTO");
        tests.transform.SetParent(root.transform);
        tests.AddComponent<TraceFieldTest>();   // grupo 0: rejilla (pura lógica)
        tests.AddComponent<FaunaChecks>();          // grupo 1: conducta/stats sobre la fauna real
        tests.AddComponent<EmergenceAuto>();        // grupo 2: bucle de temperamento
        tests.AddComponent<GrimoireTest>();         // grupo 3: repertorio de doble vía (cuerpo ∨ magia)
        tests.AddComponent<EstrusTest>();           // grupo 4: celo (hechizo-estado) → rastro Estrus
        tests.AddComponent<LifeCycleAuto>();        // grupo 5: parto → cría (cierra el ciclo de vida)
        tests.AddComponent<SicknessTest>();         // grupo 6: enfermedad → presa fácil + rastro
        tests.AddComponent<StarvationTest>();       // grupo 7: inanición → masa↓ + enfermedad
        tests.AddComponent<SleepTest>();            // grupo 8: sueño día/noche (reloj → asleep; amenaza despierta)
        tests.AddComponent<RiverCurrentTest>();     // grupo 9: río (arrastra a la cría, la grande aguanta, debilita)
        tests.AddComponent<DietPreferenceTest>();   // grupo 10: preferencia de dieta (apetencia por especie)
        tests.AddComponent<ObservationTest>();      // grupo 11: observación (amortigua el sufrimiento del apremio)
        tests.AddComponent<StateSpellsTest>();      // grupo 12: hechizos-estado (hub alostático, miedo, adicción)
        tests.AddComponent<IdealStateTest>();       // grupo 13: estado ideal (elementos por masa + actividad ACWR)
        tests.AddComponent<SensesTest>();            // grupo 14: sentidos/órganos (observar = canales según el receptor)
        tests.AddComponent<YogaTest>();              // grupo 15: yoga (postura rota el rig; Saludo al Sol = 12 asanas)
        tests.AddComponent<BodyMetabolismTest>();    // grupo 16: cuerpo (hambre/sueño por metabolismo; células de mago)
        tests.AddComponent<ChecklistMagicTest>();    // grupo 21: checklist §magia (reservas, fuego, elementos, descomposición, poder)
        tests.AddComponent<ChecklistSoulTest>();     // grupo 22: checklist §alma (blends, compañeros, alma compartida, karma)
        tests.AddComponent<ChecklistFaunaTest>();    // grupo 23: checklist §fauna (amenaza, presa, medio, rastros, celo, inanición)
        tests.AddComponent<ChecklistProgressTest>(); // grupo 24: checklist §progresión (margas, mente, campo, ECO)
        tests.AddComponent<IslandsTest>();           // grupo 25: archipiélago (caminos a nado, laguna, casa del árbol, enfermería)
        tests.AddComponent<BodyShapeTest>();         // grupo 26: las stats físicas moldean el cuerpo (estatura, músculo, grasa)
        tests.AddComponent<GardenVisitorTest>().harePrefab = LoadAnimalPrefab("Bunny");   // grupo 27: visitantes del huerto = animas reales (hambre + dieta de cultivos)
        tests.AddComponent<SanctuaryLifeTest>();     // grupo 28: elevado + musgo, dormitorios, solar, insectos de hielo, energía del cuerpo
        tests.AddComponent<ChecklistAuto>();         // grupo 20: bloques del checklist que pedían inspector (apremio, velocidad de tiempo)
        tests.AddComponent<ColdSanctuary.TestRunner>();   // orquesta lo anterior + TOTAL (namespace por CS0434 vs UnityEditor.TestRunner; global:: no lo resuelve)
        new GameObject("WasdMission_AUTO").AddComponent<ReachGoalMission>().transform.SetParent(root.transform);  // 1ª misión WASD-test (reporta por TestProbe al jugarla)
        new GameObject("EcoObservation_AUTO").AddComponent<EcosystemObservation>().transform.SetParent(root.transform);  // misión-observación: HUD de status del ecosistema + log/alertas de balance
        new GameObject("AnimaStatusHUD_AUTO").AddComponent<AnimaStatusHUD>().transform.SetParent(root.transform);  // HUD de estado de un Anima (drives/actividad/elementos con color) — docs consciousness-mechanics §2
        new GameObject("AnimaThoughtHUD_AUTO").AddComponent<AnimaThoughtHUD>().transform.SetParent(root.transform);  // HUD del PENSAMIENTO del ser que maneja el jugador — docs microcosmos-dungbeetle-level §5
        new GameObject("AnimaNameplate_AUTO").AddComponent<AnimaNameplate>().transform.SetParent(root.transform);  // ETIQUETAS de nombre sobre las animas (para dirigir hechizos por teclado) — docs typed-spells-and-queues
        BuildAnimaStatusPanels(root.transform);  // HUD DECLARATIVO generado por CÓDIGO (paneles-GameObject por elemento) — demuestra prefabs-por-código
        BuildRestaurantSandbox(root.transform);  // RESTAURANTE jugable (cocinar platillos para todos → stock → XP + inventario). docs/kitchen-simulation.md
        BuildGoluisKitchen(root.transform);      // GOLUIS, jefe de cocina: inicia a los recién llegados (sígueme→limpiar→recetas). docs/kitchen-simulation.md
        BuildMagnate(root.transform);            // LA MAGNATE: maestra-guardiana (estado infinito; pacifica; enseña yoga al rezagado). docs/anyma-factory-and-control.md
        BuildLinduarda(root.transform);          // LINDUARDA (enfermería): guardiana de salud emergente (status→vacuna/analiza al que tenga HealthRecord false, p.ej. Kushal)
        BuildGuardTower(root.transform);          // TORRE DE GUARDIA (tablero de misiones de vigía) + puestos de patrulla
        ResidentialBuilder.Build(root.transform, new Vector3(-60f, 0f, 95f));   // ARBOLEDA RESIDENCIAL: un dormitorio elevado por persona
        BuildGourmetDining(root.transform);      // RESTAURANTE: el comedor gourmet para 30, en la misma plataforma que la cocina abierta
        BuildIceFlora(root.transform);           // FLORA de hielo: árbol carnívoro (señuelo+sopor) + abejas de las nieves — docs/ice-sanctuary-ecology.md
        new GameObject("Jornada_AUTO").AddComponent<WorkdayDirector>().transform.SetParent(root.transform);   // jornada de 8 h y rotación diaria (docs/jornada-y-rotacion.md)
        { GameObject inf = GameObject.Find("Infirmary_Area"); if (inf != null) inf.AddComponent<InfirmaryGate>(); }   // la iniciación es bloqueante: la Magnate guarda la salida
        PlaceStaff();       // cada personaje nuevo en su área (Sadhguru en la tarima de yoga, el jefe de obra en Construcción…)
        DressHumanoids();   // Kushal, la Magnate, compañeros y seres de prueba → cuerpo de cubos articulado (sin .fbx)
        BuildPolarIce(root.transform);   // banquisa: casquetes de hielo con canales y puentes + sotobosque polar
        BakeNavMesh();

        // Genera también las ESCENAS HERMANAS del MICROCOSMOS (cada una es su propia .unity, no van en el mesocosmos):
        //  · Scene1 (Ambrosio/alba) — el nivel introductorio de la cueva/pulgón (anterior en la historia).
        //  · Mesopotamia (mundo mob) — la ciudad-insecto; la cocina ya apunta a ella (mobWorldSceneName).
        // Todo listo de un click. Guardado en try por si la API de escenas falla.
        try { MicrocosmosSceneBuilder.BuildScene0(); }
        catch (System.Exception e) { Debug.LogWarning($"[SampleSceneBuilder] No se pudo generar la escena Microcosmos Scene0 (Sakshi): {e.Message}"); }
        try { MicrocosmosSceneBuilder.BuildScene1(); }
        catch (System.Exception e) { Debug.LogWarning($"[SampleSceneBuilder] No se pudo generar la escena Microcosmos Scene1: {e.Message}"); }
        try { MobWorldSceneBuilder.BuildMesopotamia(); }
        catch (System.Exception e) { Debug.LogWarning($"[SampleSceneBuilder] No se pudo generar la escena mob: {e.Message}"); }
        try { MicrocosmosAreaSceneBuilder.BuildAll(); }
        catch (System.Exception e) { Debug.LogWarning($"[SampleSceneBuilder] No se pudieron generar las escenas de microcosmos por área: {e.Message}"); }
        try { CompassionSceneBuilder.BuildAll(); }
        catch (System.Exception e) { Debug.LogWarning($"[SampleSceneBuilder] No se pudieron generar las simulaciones de compasión: {e.Message}"); }
        try { MythSceneBuilder.BuildAll(); }
        catch (System.Exception e) { Debug.LogWarning($"[SampleSceneBuilder] No se pudo generar la escena de Egina (mirmidones): {e.Message}"); }
        try { StorySceneBuilder.BuildAll(); }
        catch (System.Exception e) { Debug.LogWarning($"[SampleSceneBuilder] No se pudieron generar los microcosmos de historias: {e.Message}"); }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        ClearBridgeLandings();   // al final, ya amueblado: despeja el paso de cada puente (lo que estorbe en su llegada)
        Debug.Log("[SampleSceneBuilder] Blockout listo bajo 'LevelDressing_AUTO'. Revisar y ajustar posiciones a mano.");
    }

    // ── ESCENAS DE PRUEBA (una por grupo) ─────────────────────────────────────
    //
    // Antes ~30 sandboxes vivían a la vez en la escena principal: tardaba en cargar, sus paneles se pisaban y se
    // interferían (p. ej. la posesión le quitaba el mando al jugador al arrancar). Ahora la escena principal es solo el
    // santuario jugable y cada grupo de pruebas se genera en su propia escena (copia de la principal → base mínima +
    // el grupo). Se abren desde Assets/Scenes/Pruebas o desde Build Settings.
    const string TestScenesFolder = "Assets/Scenes/Pruebas";

    static readonly (string scene, System.Action<Transform>[] build)[] TestScenes =
    {
        ("Prueba_Farming",        new System.Action<Transform>[] { BuildFarmingSandbox }),
        ("Prueba_Mente",          new System.Action<Transform>[] { BuildMindSandbox }),
        ("Prueba_Posesion",       new System.Action<Transform>[] { BuildPossessionSandbox }),
        ("Prueba_Enfermeria",    new System.Action<Transform>[] { BuildLinduarda, PlacePlayerAtInfirmary }),   // enfermería equipada + Linduarda + análisis clínico
        ("Prueba_TorreGuardia",  new System.Action<Transform>[] { BuildGuardTower, PlacePlayerAtTower }),
        ("Prueba_Restaurante",   new System.Action<Transform>[] { BuildRestaurantSandbox, PlacePlayerAtKitchen }),   // cocina real: 4 cocineros + misión «Preparar platillos»
        ("Prueba_Cocina",         new System.Action<Transform>[] { BuildKitchenSandbox, BuildKitchenOnboarding, BuildVirtualizationSandbox, BuildDecompositionSandbox }),
        ("Prueba_Oficios",        new System.Action<Transform>[] { BuildGardenVirtualization, BuildForgeVirtualization, BuildExertionSandbox,
                                                                   BuildMechanicsBeginner, BuildTruckMaintenance, BuildConstructionBeginner,
                                                                   BuildDispatchDemo, BuildCriaBeginner }),
        ("Prueba_Prologo",        new System.Action<Transform>[] { BuildPrologueSandbox }),
        ("Prueba_Yoga",           new System.Action<Transform>[] { BuildUpaYogaSandbox }),   // minijuego de virtualización (cuello por teclas)
        // UNA ESCENA POR SESIÓN de yoga (demostrador que ejecuta la secuencia en bucle; se revisan por separado).
        ("Yoga_UpaCuello",        new System.Action<Transform>[] { BuildYogaUpaNeck }),
        ("Yoga_Namaskar",         new System.Action<Transform>[] { BuildYogaNamaskar }),
        ("Yoga_SaludoSol",        new System.Action<Transform>[] { BuildYogaSaludoSol }),
        ("Yoga_Ashtanga1",        new System.Action<Transform>[] { BuildYogaAshtanga1 }),
        ("Prueba_CamaraEmocion",  new System.Action<Transform>[] { BuildScreenEffectsSandbox, BuildEmotionOrchestraSandbox }),
        ("Prueba_Magia",          new System.Action<Transform>[] { BuildMagicSandbox, BuildSpellDemoSandbox, BuildWalkUniversalSandbox }),
        ("Prueba_Alma",           new System.Action<Transform>[] { BuildSoulBlendSandbox, BuildSharedSoulSandbox }),
        ("Prueba_Fauna",          new System.Action<Transform>[] { BuildFamilyLifeTest, BuildInteractionTests, BuildSpeciesScenarios }),
    };

    [MenuItem("Tools/Cold Sanctuary/Build Test Scenes (una escena por prueba)")]
    public static void BuildTestScenes()
    {
        string main = "Assets/Scenes/SampleScene.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        if (!AssetDatabase.IsValidFolder(TestScenesFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Pruebas");

        var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var (sceneName, build) in TestScenes)
        {
            string path = $"{TestScenesFolder}/{sceneName}.unity";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CopyAsset(main, path);          // misma cámara, luz y Player que la principal
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            GameObject old = GameObject.Find(RootName);
            if (old != null) Object.DestroyImmediate(old);
            GameObject root = new GameObject(RootName);
            BuildTestBase(root.transform);
            foreach (var b in build)
            {
                try { b(root.transform); }
                catch (System.Exception e) { Debug.LogError($"[TestScenes] {sceneName}: {b.Method.Name} falló: {e}"); }
            }
            DressHumanoids();
            BakeNavMesh();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            if (!buildScenes.Exists(s => s.path == path)) buildScenes.Add(new EditorBuildSettingsScene(path, true));
            Debug.Log($"[TestScenes] {path}: {build.Length} sandbox(es).");
        }
        EditorBuildSettings.scenes = buildScenes.ToArray();
        EditorSceneManager.OpenScene(main, OpenSceneMode.Single);
        Debug.Log($"[TestScenes] {TestScenes.Length} escenas de prueba en {TestScenesFolder}.");
    }

    // Lo mínimo para que una prueba funcione: suelo, sistemas del mundo, áreas (algunas pruebas las usan), HUD/UI,
    // sistemas del jugador, economía (recompensas), rastros, reloj y los HUD de estado/pensamiento/nombres.
    static void BuildTestBase(Transform root)
    {
        BuildGround(root);
        BuildWorldSystems(root);
        SanctuaryArea[] areas = BuildAreaStructures(root);
        WireSanctuaryDirector(areas);
        BuildUI(root);
        EnsurePlayerSystems();
        BuildSanctuaryEconomy(root);
        new GameObject("TraceField_AUTO").AddComponent<TraceField>().transform.SetParent(root);
        new GameObject("Clock_AUTO").AddComponent<Clock>().transform.SetParent(root);
        new GameObject("AnimaStatusHUD_AUTO").AddComponent<AnimaStatusHUD>().transform.SetParent(root);
        new GameObject("AnimaThoughtHUD_AUTO").AddComponent<AnimaThoughtHUD>().transform.SetParent(root);
        new GameObject("AnimaNameplate_AUTO").AddComponent<AnimaNameplate>().transform.SetParent(root);
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
            if (player.GetComponent<PlayController>() == null) player.AddComponent<PlayController>();
            if (player.GetComponent<PlayerClimber>() == null) player.AddComponent<PlayerClimber>(); // trepar (tecla Espacio)

            CharacterLevel cl = player.GetComponent<CharacterLevel>();
            if (cl == null) cl = player.AddComponent<CharacterLevel>(); // XP del farming; pools derivados de aptitudes
            // Perfil de Kushal (algo por encima de la media) para que se note la derivación de pools.
            Aptitudes apt = Aptitudes.Default;
            apt.strength = 1.2f; apt.endurance = 1.2f; apt.agility = 1.1f;
            cl.aptitudes = apt;
        }

        GameObject group = new GameObject("FarmingSandbox_AUTO");
        group.transform.SetParent(parent);
        group.AddComponent<FarmingSandboxItems>(); // crea items placeholder y los asigna como drops (runtime)

        // Criaturas de distinta dificultad/recompensa (dischargePerTouch menor = más difícil).
        // Suave/Media: criadas por humanos, seguras. Dura: puede perder el control si te excitas y no esquivas.
        // Salvaje: NO criada → no juega (rige la ley natural; V no hace nada) — demuestra el gateo por bond.
        AddPlayCreature(group.transform, "PlayCreature_Suave",   new Vector3( 6f, 1f, 6f), 0.15f, SanctuaryResource.Food,      15f, 2, 20f, handRaised: true,  canLoseControl: false);
        AddPlayCreature(group.transform, "PlayCreature_Media",   new Vector3( 9f, 1f, 4f), 0.10f, SanctuaryResource.Materials, 25f, 4, 35f, handRaised: true,  canLoseControl: false);
        AddPlayCreature(group.transform, "PlayCreature_Dura",    new Vector3(11f, 1f, 8f), 0.06f, SanctuaryResource.Research,  40f, 8, 60f, handRaised: true,  canLoseControl: true,  looseControlDamage: 35f);
        AddPlayCreature(group.transform, "PlayCreature_Salvaje", new Vector3( 4f, 1f, 9f), 0.10f, SanctuaryResource.Research,  40f, 8, 60f, handRaised: false, canLoseControl: true,  looseControlDamage: 35f);

        // Árbol trepable de prueba (docs §Trepar): acércate y mantén Espacio para subir (gasta energía).
        GameObject tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tree.name = "ClimbTree";
        tree.transform.SetParent(group.transform);
        tree.transform.position   = new Vector3(14f, 3f, 6f);
        tree.transform.localScale = new Vector3(0.6f, 3f, 0.6f); // ~6 de alto
        tree.GetComponent<Renderer>().sharedMaterial = MakeMaterial("ClimbTree_MAT", new Color(0.45f, 0.30f, 0.18f));
        tree.AddComponent<Climbable>().topY = 6f;

        Debug.Log("[SampleSceneBuilder] Farming sandbox: PlayController (tecla V) + PlayerClimber (Espacio) + CharacterLevel en el jugador + 4 PlayableCreatures + ClimbTree. " +
                  "Juega (V) hasta serenarlas (dan recursos/monedas/XP/items); F/clic para darles de comer. " +
                  "La 'Dura' puede golpearte si te excitas y no esquivas; la 'Salvaje' no juega (ley natural).");
    }

    static void AddPlayCreature(Transform parent, string name, Vector3 pos, float discharge,
                                SanctuaryResource res, float amount, int coins, float xp,
                                bool handRaised = true, bool canLoseControl = false,
                                float looseControlDamage = 8f)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"{name}_MAT", new Color(0.9f, 0.35f, 0.3f));

        PlayableCreature pc = go.AddComponent<PlayableCreature>();
        pc.dischargePerTouch   = discharge;
        pc.dropResource        = res;
        pc.dropAmount          = amount;
        pc.dropCoins           = coins;
        pc.xpReward            = xp;
        pc.handRaised          = handRaised;
        pc.canLoseControl      = canLoseControl;
        pc.looseControlDamage  = looseControlDamage;
    }

    // ── Mind sandbox (MVP de mente: frases por tono elemental) ──────────────────
    //
    // docs/anima-architecture.md §6/§10: 3 ánimas con Mind + aptitudes distintas → cada una tiende a un
    // tono elemental y expresa frases hasta donde su poder mental alcanza. Mira la Console.

    static void BuildMindSandbox(Transform parent)
    {
        GameObject group = new GameObject("MindSandbox_AUTO");
        group.transform.SetParent(parent);

        // Roca: fuerte/resistente, poco intelecto → tono Tierra, frases cortas.
        AddMindBeing(group.transform, "Anima_Roca", new Vector3(-4f, 1f, 10f), new Color(0.50f, 0.45f, 0.40f),
            a => { a.strength = 1.6f; a.endurance = 1.5f; a.bodyMass = 1.4f; a.reasoning = 0.6f; a.memory = 0.7f; a.creativity = 0.4f; return a; });

        // Fuego apasionado: creatividad/sociabilidad altas → tono Fuego, frases más largas.
        AddMindBeing(group.transform, "Anima_Fuego", new Vector3(-1f, 1f, 11f), new Color(0.90f, 0.40f, 0.20f),
            a => { a.creativity = 1.7f; a.sociability = 1.6f; a.reasoning = 1.3f; a.memory = 1.3f; return a; });

        // Viento disperso: agilidad/razón/percepción → tono Viento.
        AddMindBeing(group.transform, "Anima_Viento", new Vector3(2f, 1f, 10f), new Color(0.70f, 0.80f, 0.90f),
            a => { a.agility = 1.6f; a.reasoning = 1.5f; a.perception = 1.4f; a.discipline = 1.3f; return a; });

        // Campo de pensamiento de prueba (docs anima §5): empuja hacia Agua + sube serotonina cerca.
        GameObject fieldGO = new GameObject("ThoughtField_Agua");
        fieldGO.transform.SetParent(group.transform);
        fieldGO.transform.position = new Vector3(-1f, 0f, 10f);
        ThoughtField field = fieldGO.AddComponent<ThoughtField>();
        field.tone = ElementalTone.Agua; field.radius = 6f; field.pull = 4f;
        field.nudgesHumor = true; field.humor = Humor.Serotonina; field.humorPerSecond = 0.05f;

        Debug.Log("[SampleSceneBuilder] Mind sandbox: 3 ánimas con Mind + aptitudes distintas + un ThoughtField(Agua). " +
                  "Console: cada una tiende a su tono, pero dentro del campo se inclinan a Agua y suben serotonina (docs anima §5/§6).");

        LogPhrasePools();
    }

    /// <summary>Diagnóstico: al compilar/construir, valida en consola que las pools de frases cargan bien.</summary>
    static void LogPhrasePools()
    {
        Debug.Log($"[Frases] Total={PhraseLibrary.All.Count} | " +
                  $"Elemental={PhraseLibrary.ForCategory(PhraseCategory.Elemental).Count} " +
                  $"Vivencia={PhraseLibrary.ForCategory(PhraseCategory.Vivencia).Count} " +
                  $"Deseo={PhraseLibrary.ForCategory(PhraseCategory.Deseo).Count}");

        foreach (string src in new[] { "Goluis", "Panterilia", "Gohageneis", "Irosene" })
            Debug.Log($"[Frases] Biografía de {src}: {PhraseLibrary.VivenciasOf(src).Count} vivencias.");

        // Reparto de ejemplo: crear un ser con 3 vivencias al azar sin repetir (modo espontáneo).
        var taken = new System.Collections.Generic.HashSet<MindPhrase>();
        var hand = PhraseLibrary.DealVivencias(3, taken);
        var sb = new System.Text.StringBuilder("[Frases] Ser de ejemplo recibe: ");
        foreach (MindPhrase p in hand) sb.Append($"[{p.source}·{p.tone}] \"{p.positive[0]}\"  ");
        Debug.Log(sb.ToString());

        LogDistribution();
    }

    /// <summary>Diagnóstico del reparto por modo (docs anima §11): Magnate + histórico bloqueados vs libres.</summary>
    static void LogDistribution()
    {
        // 6 seres: la Magnate y un histórico BLOQUEADOS; los 4 compañeros LIBRES.
        var holders = new System.Collections.Generic.List<PhraseHolder>
        {
            new PhraseHolder("Magnate", true),     // nadie hereda sus pensamientos
            new PhraseHolder("Ötzi", true),        // histórico del Microcosmos, bloqueado
            new PhraseHolder("Goluis", false),
            new PhraseHolder("Panterilia", false),
            new PhraseHolder("Gohageneis", false),
            new PhraseHolder("Irosene", false),
            new PhraseHolder("", false),           // ser anónimo emergente: solo recibe del pool
        };

        foreach (NarrativeMode mode in new[] { NarrativeMode.Estricta, NarrativeMode.Libre })
        {
            var plan = PhraseDistribution.Plan(mode, holders);
            Debug.Log($"[Frases] Reparto {mode}:");
            for (int i = 0; i < holders.Count; i++)
            {
                string who = holders[i].identity == "" ? "(anónimo)" : holders[i].identity;
                string lk = holders[i].locked ? " 🔒" : "";
                var sb = new System.Text.StringBuilder($"   {who}{lk} → ");
                foreach (MindPhrase p in plan[i]) sb.Append($"[{p.source}·{p.tone}] ");
                Debug.Log(sb.ToString());
            }
        }
    }

    /// <summary>
    /// Sandbox del control intercambiable + posesión por relevancia (docs anima §11.5). Dos seres con
    /// `AnimaController` + `AiBrain`: uno DÉBIL (selfRelevance=1) y uno FUERTE (selfRelevance=3). A ambos
    /// se les inyecta un `PlayerBrain` de posesión=2 → en Play, el AnimaController debe dar el mando del
    /// débil al JUGADOR (2 > 1) y dejar el fuerte en su IA (2 &lt; 3). Un caster con `PossessionSpell` queda
    /// en escena para probar `PossessNearest()` en runtime.
    /// </summary>
    static void BuildPossessionSandbox(Transform parent)
    {
        GameObject group = new GameObject("PossessionSandbox_AUTO");
        group.transform.SetParent(parent);

        // Ser DÉBIL (selfRelevance 1): la posesión del jugador (power 2 > 1) lo domina → lo mueves con WASD.
        GameObject weak = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        weak.name = "Anima_Debil"; weak.transform.SetParent(group.transform);
        weak.transform.position = new Vector3(-3f, 1f, 16f);
        weak.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Anima_Debil_MAT", new Color(0.5f, 0.7f, 0.5f));
        weak.AddComponent<AiBrain>().selfRelevance = 1f;
        weak.AddComponent<AnimaController>();

        // Ser FUERTE (selfRelevance 3): la misma posesión (2 < 3) NO lo domina → su IA conserva el mando.
        GameObject strong = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        strong.name = "Anima_Fuerte"; strong.transform.SetParent(group.transform);
        strong.transform.position = new Vector3(0f, 1f, 16f);
        strong.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Anima_Fuerte_MAT", new Color(0.7f, 0.4f, 0.4f));
        strong.AddComponent<AiBrain>().selfRelevance = 3f;
        strong.AddComponent<AnimaController>();

        // "Kushal": su mente liberada SIGUE al cuerpo débil (que conduce el jugador) por la zona.
        GameObject follower = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        follower.name = "Kushal_Follow"; follower.transform.SetParent(group.transform);
        follower.transform.position = new Vector3(-5f, 1f, 16f);
        follower.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Kushal_Follow_MAT", new Color(0.35f, 0.55f, 0.85f));
        follower.AddComponent<AiBrain>().selfRelevance = 1f;
        FollowBrain fb = follower.AddComponent<FollowBrain>();
        fb.target = weak.transform; fb.relevance = 1.5f;   // por encima del idle → conduce mientras siga
        AnimaController followerCtrl = follower.AddComponent<AnimaController>();
        follower.AddComponent<HelpResponder>().acceptChance = 1f;   // acepta peticiones (demo)

        // El "alma" del jugador: posee al más cercano (el débil) al empezar; Tab intenta saltar al otro.
        GameObject core = new GameObject("PlayerCore_AUTO");
        core.transform.SetParent(group.transform);
        core.transform.position = new Vector3(-3f, 1f, 14f);   // más cerca del débil que del fuerte
        PossessionSpell spell = core.AddComponent<PossessionSpell>();
        spell.power = 2f; spell.range = 30f;
        PlayerCore pc = core.AddComponent<PlayerCore>();
        pc.spell = spell; pc.possessNearestOnStart = true;
        pc.followCamera = false;   // no pelear con la cámara del PlayerController real en este sandbox

        // Petición → alma compartida (docs anima §11.7): un aldeano pide a Kushal ir juntos a un punto.
        GameObject goal = new GameObject("HelpGoal");
        goal.transform.SetParent(group.transform);
        goal.transform.position = new Vector3(3f, 1f, 18f);
        GameObject asker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        asker.name = "Aldeano_Pide"; asker.transform.SetParent(group.transform);
        asker.transform.position = new Vector3(1f, 1f, 18f);
        asker.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Aldeano_Pide_MAT", new Color(0.85f, 0.75f, 0.4f));
        asker.AddComponent<AiBrain>().selfRelevance = 1f;
        asker.AddComponent<AnimaController>();
        HelpRequest req = asker.AddComponent<HelpRequest>();
        req.autoAskOnStart = true; req.autoResponder = followerCtrl; req.autoGoal = goal.transform; req.shareDuration = 8f;

        Debug.Log("[SampleSceneBuilder] Possession sandbox: en Play el jugador posee «Anima_Debil» " +
                  "(2 > 1) y lo mueves con WASD; «Kushal_Follow» lo sigue; con Tab intentas saltar a " +
                  "«Anima_Fuerte» pero su IA aguanta (2 < 3). «Aldeano_Pide» pide a Kushal ir juntos a " +
                  "«HelpGoal» → alma compartida ~8s (logs [Petición]). Ver [Control]/[Jugador]/[Posesión] " +
                  "(docs anima §11.5/§11.7). Nota: WASD mueve también al Player real (comparten input).");
    }

    /// <summary>
    /// Sandbox del paso A de la cocina (docs/kitchen-simulation.md §5): una `DirtArea` genera manchas; al
    /// pasar del umbral activa la misión de limpieza; un `Cleaner` (auto) las borra mancha a mancha y, al
    /// vaciarse, la completa. Todo por consola (`[Cocina]`).
    /// </summary>
    static void BuildKitchenSandbox(Transform parent)
    {
        GameObject group = new GameObject("KitchenSandbox_AUTO");
        group.transform.SetParent(parent);

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Kitchen_Floor"; floor.transform.SetParent(group.transform);
        floor.transform.position = new Vector3(12f, 0f, 16f);
        floor.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
        floor.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Kitchen_Floor_MAT", new Color(0.80f, 0.80f, 0.75f));

        GameObject dirt = new GameObject("KitchenDirtArea");
        dirt.transform.SetParent(group.transform);
        dirt.transform.position = new Vector3(12f, 0f, 16f);
        DirtArea area = dirt.AddComponent<DirtArea>();
        area.areaSize = new Vector2(8f, 8f); area.spawnInterval = 1.5f; area.maxSpots = 8; area.missionThreshold = 5;

        GameObject pinche = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        pinche.name = "Pinche_Limpia"; pinche.transform.SetParent(group.transform);
        pinche.transform.position = new Vector3(12f, 1f, 16f);
        pinche.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Pinche_MAT", new Color(0.50f, 0.60f, 0.70f));
        Cleaner c = pinche.AddComponent<Cleaner>();
        c.auto = true; c.autoInterval = 2f; c.reach = 12f;   // alcance amplio para el demo sin moverse

        Debug.Log("[SampleSceneBuilder] Kitchen sandbox (paso A): «KitchenDirtArea» genera manchas; al pasar " +
                  "de 5 activa la misión de limpieza; «Pinche_Limpia» (Cleaner auto) las borra mancha a mancha " +
                  "y al vaciarse la completa. Logs [Cocina] (docs kitchen §5).");
    }

    /// <summary>
    /// Paseo guiado (docs/kitchen-simulation.md §1) + paso B (loop de desayuno + contenedor, §3/§6). El
    /// «Anfitrion» recorre las estaciones con el «Novato» (alma compartida); el «Cocinero» rellena el
    /// contenedor y el «Comensal» come. Todo por consola: [Paseo]/[Cocina].
    /// </summary>
    static void BuildKitchenOnboarding(Transform parent)
    {
        GameObject group = new GameObject("KitchenOnboarding_AUTO");
        group.transform.SetParent(parent);

        TourStation nevera     = MakeStation(group.transform, "Nevera",     new Vector3(8f, 1f, 16f),  "sacar y guardar ingredientes.");
        TourStation plancha    = MakeStation(group.transform, "Plancha",    new Vector3(12f, 1f, 20f), "cocinar: revolver, sellar, secar.");
        TourStation mesones    = MakeStation(group.transform, "Mesones",    new Vector3(16f, 1f, 16f), "preparar y emplatar.");
        TourStation contenedor = MakeStation(group.transform, "Contenedor", new Vector3(12f, 1f, 12f), "dejar la comida lista para que coman.");

        // Contenedor de servicio (paso B), en la estación Contenedor.
        GameObject foodGO = new GameObject("Contenedor_Desayuno");
        foodGO.transform.SetParent(group.transform);
        foodGO.transform.position = new Vector3(12f, 1f, 12f);
        FoodContainer food = foodGO.AddComponent<FoodContainer>();
        food.dishName = "huevos revueltos"; food.capacity = 20;

        // Anfitrión (hace el paseo) + novato (lo acompaña por alma compartida).
        GameObject host = MakeKitchenPerson(group.transform, "Anfitrion", new Vector3(12f, 1f, 15f), new Color(0.50f, 0.40f, 0.55f));
        host.AddComponent<AiBrain>().selfRelevance = 1f;
        host.AddComponent<AnimaController>();
        host.AddComponent<HelpRequest>();
        GameObject guest = MakeKitchenPerson(group.transform, "Novato", new Vector3(11f, 1f, 14f), new Color(0.60f, 0.70f, 0.50f));
        guest.AddComponent<AiBrain>().selfRelevance = 1f;
        AnimaController guestCtrl = guest.AddComponent<AnimaController>();
        guest.AddComponent<HelpResponder>().acceptChance = 1f;
        GuidedTour tour = host.AddComponent<GuidedTour>();
        tour.guest = guestCtrl;
        tour.stations = new[] { nevera, plancha, mesones, contenedor };
        tour.arriveDistance = 1.5f; tour.dwell = 2f; tour.startOnPlay = true;

        // Cocinero (paso B) rellena el contenedor + comensal que come.
        GameObject cook = MakeKitchenPerson(group.transform, "Cocinero", new Vector3(12f, 1f, 19f), new Color(0.85f, 0.80f, 0.60f));
        cook.AddComponent<BreakfastCook>().container = food;
        GameObject eater = MakeKitchenPerson(group.transform, "Comensal", new Vector3(13f, 1f, 12f), new Color(0.70f, 0.55f, 0.50f));
        Eater e = eater.AddComponent<Eater>(); e.eatInterval = 3f; e.reach = 15f;

        Debug.Log("[SampleSceneBuilder] Kitchen onboarding: «Anfitrion» hace el PASEO por Nevera/Plancha/" +
                  "Mesones/Contenedor con «Novato» (alma compartida); «Cocinero» rellena el contenedor con " +
                  "el loop de desayuno y «Comensal» come. Logs [Paseo]/[Cocina] (docs kitchen §1/§3/§6).");
    }

    /// <summary>
    /// Sandbox del MOTOR de virtualización (docs/kitchen-simulation.md §3b): estaciones funcionales
    /// (partes manipulables) + una receta de huevos revueltos + el `VirtualPointer` (teclado/ratón/touch).
    /// En Play: mueve el puntero (flechas / ratón / touch), apunta a las partes EN ORDEN y confirma
    /// (Espacio / clic / toque). Logs [Virtual]/[Producción]/[Cocina].
    /// </summary>
    static void BuildVirtualizationSandbox(Transform parent)
    {
        GameObject group = new GameObject("VirtualizationSandbox_AUTO");
        group.transform.SetParent(parent);

        // Contenedor de producción.
        GameObject foodGO = new GameObject("Contenedor_Virtual");
        foodGO.transform.SetParent(group.transform);
        foodGO.transform.position = new Vector3(22f, 1f, 15f);
        FoodContainer food = foodGO.AddComponent<FoodContainer>();
        food.dishName = "huevos revueltos"; food.capacity = 20;

        // Estaciones funcionales (partes manipulables).
        MakeStationPart(group.transform, "Meson",  "AbrirPuerta",   "abres el mesón",           new Vector3(18f, 1f, 14f), new Color(0.55f, 0.40f, 0.30f));
        MakeStationPart(group.transform, "Meson",  "TomarSarten",   "tomas la sartén",          new Vector3(18f, 1f, 15f), new Color(0.35f, 0.35f, 0.35f));
        MakeStationPart(group.transform, "Nevera", "AbrirPuerta",   "abres la nevera",          new Vector3(18f, 1f, 18f), new Color(0.70f, 0.85f, 0.95f));
        MakeStationPart(group.transform, "Nevera", "TomarHuevo",    "tomas los huevos",         new Vector3(18f, 1f, 19f), new Color(0.95f, 0.90f, 0.70f));
        MakeStationPart(group.transform, "Cocina", "PonerSarten",   "pones la sartén al fuego", new Vector3(22f, 1f, 17f), new Color(0.30f, 0.30f, 0.35f));
        MakeStationPart(group.transform, "Cocina", "PonerHuevo",    "cascas el huevo",          new Vector3(22f, 1f, 18f), new Color(0.95f, 0.85f, 0.55f));
        // El fogón es una acción TEMPORIZADA: se acelera tecleando (docs kitchen §4b).
        StationPart fogon = MakeStationPart(group.transform, "Cocina", "EncenderFuego", "enciendes el fuego (teclea para cocinar)", new Vector3(22f, 1f, 19f), new Color(0.90f, 0.45f, 0.20f));
        TypingChallenge cook = fogon.gameObject.AddComponent<TypingChallenge>();
        cook.baseTime = 9f; cook.reductionPerWord = 1.5f; cook.language = "en";
        cook.words = new[] { "cook", "eggs", "protein", "healthy", "tasty", "b2" };
        fogon.timed = cook;

        // La receta (misión de producción): la cadena en orden.
        GameObject orderGO = new GameObject("Receta_Desayuno");
        orderGO.transform.SetParent(group.transform);
        ProductionOrder order = orderGO.AddComponent<ProductionOrder>();
        order.productName = "desayuno"; order.output = food; order.quota = 3;
        order.stepStation = new[] { "Meson", "Meson", "Nevera", "Nevera", "Cocina", "Cocina", "Cocina" };
        order.stepAction  = new[] { "AbrirPuerta", "TomarSarten", "AbrirPuerta", "TomarHuevo", "PonerSarten", "PonerHuevo", "EncenderFuego" };
        order.stepLabel   = new[] { "abres el mesón", "tomas la sartén", "abres la nevera", "tomas los huevos", "pones la sartén al fuego", "cascas el huevo", "enciendes el fuego" };
        // El jugador cocina → se desgasta (vía A). Estar de pie/abrir cuesta poco; encender el fuego/cascar, más.
        order.worker = GameObject.Find("Player")?.GetComponent<Anima>();
        order.stepExertion = new[] {
            new ExertionCost { glucose = 0.01f, minerals = 0.01f, fatigue = 0.01f },  // abrir el mesón
            new ExertionCost { glucose = 0.02f, minerals = 0.01f, fatigue = 0.02f },  // tomar la sartén (peso)
            new ExertionCost { glucose = 0.01f, minerals = 0.01f, fatigue = 0.01f },  // abrir la nevera
            new ExertionCost { glucose = 0.01f, minerals = 0.01f, fatigue = 0.01f },  // tomar los huevos
            new ExertionCost { glucose = 0.02f, minerals = 0.01f, fatigue = 0.02f },  // poner la sartén
            new ExertionCost { glucose = 0.02f, minerals = 0.02f, fatigue = 0.02f },  // cascar el huevo
            new ExertionCost { glucose = 0.04f, minerals = 0.02f, fatigue = 0.03f },  // encender el fuego (esfuerzo)
        };

        // El puntero input-agnóstico.
        GameObject ptr = new GameObject("VirtualPointer_AUTO");
        ptr.transform.SetParent(group.transform);
        ptr.AddComponent<VirtualPointer>();

        Debug.Log("[SampleSceneBuilder] Virtualization sandbox (motor, docs kitchen §3b): mueve el puntero " +
                  "(flechas / ratón / touch), apunta a las partes EN ORDEN (Mesón→Nevera→Cocina) y confirma " +
                  "(Espacio / clic / toque). 3 desayunos = misión cumplida. Logs [Virtual]/[Producción].");
    }

    /// <summary>
    /// El MISMO motor de virtualización aplicado al Huerto (docs/garden-simulation.md §8): otras estaciones
    /// y otra receta (abonar→arar→trasplantar→regar→cosechar). Reutiliza el `VirtualPointer` del sandbox de
    /// cocina (uno por jugador; sus pasos llegan a TODAS las `ProductionOrder`).
    /// </summary>
    static void BuildGardenVirtualization(Transform parent)
    {
        GameObject group = new GameObject("GardenVirtualization_AUTO");
        group.transform.SetParent(parent);

        // Cesto de producción (la cosecha).
        GameObject cestoGO = new GameObject("Cesto_Cosecha");
        cestoGO.transform.SetParent(group.transform);
        cestoGO.transform.position = new Vector3(34f, 1f, 15f);
        FoodContainer cesto = cestoGO.AddComponent<FoodContainer>();
        cesto.dishName = "grano"; cesto.capacity = 20;

        // Estaciones del huerto (herramientas/recursos a la izquierda, la parcela a la derecha).
        MakeStationPart(group.transform, "Compostero", "TomarAbono",     "tomas abono (carretilla)",  new Vector3(28f, 1f, 13f), new Color(0.45f, 0.30f, 0.20f));
        MakeStationPart(group.transform, "Cobertizo",  "TomarAzada",     "tomas la azada",            new Vector3(28f, 1f, 15f), new Color(0.55f, 0.50f, 0.45f));
        MakeStationPart(group.transform, "Semillero",  "TomarPlantula",  "tomas una plántula/maceta", new Vector3(28f, 1f, 17f), new Color(0.45f, 0.70f, 0.40f));
        MakeStationPart(group.transform, "Agua",       "LlenarRegadera", "llenas la regadera",        new Vector3(28f, 1f, 19f), new Color(0.55f, 0.75f, 0.95f));
        MakeStationPart(group.transform, "Parcela",    "Abonar",         "abonas la tierra",          new Vector3(32f, 1f, 13f), new Color(0.40f, 0.28f, 0.18f));
        MakeStationPart(group.transform, "Parcela",    "Arar",           "aras los surcos",           new Vector3(32f, 1f, 15f), new Color(0.50f, 0.38f, 0.25f));
        MakeStationPart(group.transform, "Parcela",    "Trasplantar",    "trasplantas al surco",      new Vector3(32f, 1f, 17f), new Color(0.40f, 0.65f, 0.35f));
        MakeStationPart(group.transform, "Parcela",    "Regar",          "riegas",                    new Vector3(32f, 1f, 19f), new Color(0.50f, 0.70f, 0.90f));
        MakeStationPart(group.transform, "Parcela",    "Cosechar",       "cosechas al cesto",         new Vector3(32f, 1f, 21f), new Color(0.90f, 0.80f, 0.35f));

        GameObject orderGO = new GameObject("Receta_Huerto");
        orderGO.transform.SetParent(group.transform);
        ProductionOrder order = orderGO.AddComponent<ProductionOrder>();
        order.productName = "cosecha"; order.output = cesto; order.quota = 3;
        order.stepStation = new[] { "Compostero", "Parcela", "Cobertizo", "Parcela", "Semillero", "Parcela", "Agua", "Parcela", "Parcela" };
        order.stepAction  = new[] { "TomarAbono", "Abonar", "TomarAzada", "Arar", "TomarPlantula", "Trasplantar", "LlenarRegadera", "Regar", "Cosechar" };
        order.stepLabel   = new[] { "tomas abono", "abonas la tierra", "tomas la azada", "aras los surcos", "tomas una plántula", "trasplantas", "llenas la regadera", "riegas", "cosechas" };

        Debug.Log("[SampleSceneBuilder] Garden virtualization (docs garden §8): misma mecánica, receta " +
                  "agrícola (abonar→arar→trasplantar→regar→cosechar), 9 pasos, cuota 3. Reusa el VirtualPointer.");
    }

    /// <summary>
    /// El mismo motor aplicado a la FORJA del **Microcosmos** (era de los metales; docs/forge-simulation.md
    /// §2): receta de herramienta de bronce (TomarMineral → Fundir[typing] → Verter → Forjar → Templar). El
    /// crisol es una acción temporizada que se acelera tecleando elementos/compuestos. (La **Mecánica** del
    /// Mesocosmos —reparar máquinas/vehículos— usará recetas distintas, aún por construir.)
    /// </summary>
    static void BuildForgeVirtualization(Transform parent)
    {
        GameObject group = new GameObject("ForgeVirtualization_AUTO");
        group.transform.SetParent(parent);

        GameObject binGO = new GameObject("Contenedor_Herramientas");
        binGO.transform.SetParent(group.transform);
        binGO.transform.position = new Vector3(44f, 1f, 16f);
        FoodContainer bin = binGO.AddComponent<FoodContainer>();
        bin.dishName = "herramienta de bronce"; bin.capacity = 20;

        MakeStationPart(group.transform, "Cantera", "TomarMineral", "tomas mineral (cobre+estaño)", new Vector3(38f, 1f, 14f), new Color(0.45f, 0.42f, 0.38f));
        // El crisol es TEMPORIZADO: se acelera tecleando elementos/compuestos (docs kitchen §4b + química).
        StationPart crisol = MakeStationPart(group.transform, "Crisol", "Fundir", "fundes el metal (teclea para acelerar)", new Vector3(38f, 1f, 16f), new Color(0.90f, 0.50f, 0.20f));
        TypingChallenge smelt = crisol.gameObject.AddComponent<TypingChallenge>();
        smelt.baseTime = 10f; smelt.reductionPerWord = 1.5f; smelt.language = "en";
        smelt.words = new[] { "bronze", "copper", "tin", "cu", "sn", "melt" };
        crisol.timed = smelt;
        MakeStationPart(group.transform, "Molde",  "Verter",  "viertes en el molde",   new Vector3(40f, 1f, 16f), new Color(0.55f, 0.45f, 0.35f));
        MakeStationPart(group.transform, "Yunque", "Forjar",  "forjas en el yunque",   new Vector3(42f, 1f, 16f), new Color(0.40f, 0.40f, 0.45f));
        MakeStationPart(group.transform, "Agua",   "Templar", "templas en agua",       new Vector3(42f, 1f, 18f), new Color(0.55f, 0.75f, 0.95f));

        GameObject orderGO = new GameObject("Receta_Forja");
        orderGO.transform.SetParent(group.transform);
        ProductionOrder order = orderGO.AddComponent<ProductionOrder>();
        order.productName = "herramienta de bronce"; order.output = bin; order.quota = 3;
        order.stepStation = new[] { "Cantera", "Crisol", "Molde", "Yunque", "Agua" };
        order.stepAction  = new[] { "TomarMineral", "Fundir", "Verter", "Forjar", "Templar" };
        order.stepLabel   = new[] { "tomas mineral", "fundes el metal", "viertes en el molde", "forjas en el yunque", "templas en agua" };

        Debug.Log("[SampleSceneBuilder] Forge virtualization (docs forge-simulation): receta de bronce, " +
                  "5 pasos, el Crisol se acelera tecleando (bronze/copper/tin/cu/sn/melt). Reusa el VirtualPointer.");
    }

    /// <summary>
    /// DESGASTE POR TRABAJO (vía A — docs/soul-relations §2b). Un **trabajador Anima** (arquetipo Goluis, con
    /// <see cref="MoodDynamics"/> y comportamiento) asignado a una receta de **labor pesada**: cada paso le gasta
    /// reservas (glucosa/minerales) y acumula fatiga según el <c>stepExertion</c> (cargar/moler pesan más que
    /// apilar). Conduce la receta con el puntero y observa en el Inspector cómo, turno a turno, le baja la glucosa,
    /// sube la fatiga y el cortisol → `Anima.stress` sube → **`Goluis.UnderPressure` se activa solo** (mal humor
    /// situacional). Nota: hoy el que confirma es el jugador (aún no-Anima), así que el trabajador es el "obrero"
    /// asignado; cuando el jugador sea un Anima, bastará con ponerlo de `worker`.
    /// </summary>
    static void BuildExertionSandbox(Transform parent)
    {
        GameObject group = new GameObject("Desgaste_AUTO");
        group.transform.SetParent(parent);

        // El trabajador: Anima con humores dinámicos + comportamiento de Goluis (para ver UnderPressure).
        GameObject workerGO = new GameObject("Obrero_Goluis");
        workerGO.transform.SetParent(group.transform);
        workerGO.transform.position = new Vector3(50f, 1f, 16f);
        MakeCompanionCore(workerGO, "Goluis");          // Anima+Mind+SoulComposition+SocialField+MoodDynamics+MoodState
        workerGO.AddComponent<Goluis>();
        Anima worker = workerGO.GetComponent<Anima>();

        FoodContainer bin = new GameObject("Pila_Obra").AddComponent<FoodContainer>();
        bin.transform.SetParent(group.transform);
        bin.transform.position = new Vector3(50f, 1f, 20f);
        bin.dishName = "carga movida"; bin.capacity = 30;

        MakeStationPart(group.transform, "ObraCantera", "CargarPiedra", "cargas una piedra (fuerza)", new Vector3(48f, 1f, 14f), new Color(0.45f, 0.42f, 0.38f));
        MakeStationPart(group.transform, "ObraMuela",   "Moler",        "mueles a mano (esfuerzo)",   new Vector3(48f, 1f, 16f), new Color(0.55f, 0.50f, 0.45f));
        MakeStationPart(group.transform, "ObraPila",    "Apilar",       "apilas (de pie)",            new Vector3(48f, 1f, 18f), new Color(0.60f, 0.55f, 0.40f));

        GameObject orderGO = new GameObject("Receta_LaborPesada");
        orderGO.transform.SetParent(group.transform);
        ProductionOrder order = orderGO.AddComponent<ProductionOrder>();
        order.productName = "carga movida"; order.output = bin; order.quota = 10;   // cuota alta → el desgaste se acumula
        order.stepStation = new[] { "ObraCantera",  "ObraMuela", "ObraPila" };
        order.stepAction  = new[] { "CargarPiedra", "Moler",     "Apilar" };
        order.stepLabel   = new[] { "cargas piedra", "mueles a mano", "apilas" };
        order.worker = worker;
        order.stepExertion = new[] {
            new ExertionCost { glucose = 0.06f, minerals = 0.04f, fatigue = 0.05f },   // cargar: fuerza
            new ExertionCost { glucose = 0.05f, minerals = 0.02f, fatigue = 0.04f },   // moler: esfuerzo sostenido
            new ExertionCost { glucose = 0.02f, minerals = 0.03f, fatigue = 0.03f, sleepiness = 0.01f },  // apilar: de pie
        };

        Debug.Log("[SampleSceneBuilder] Desgaste_AUTO (vía A): «Obrero_Goluis» trabaja una receta de labor pesada; " +
                  "cada paso le gasta reservas/fatiga → MoodDynamics lo vuelve cortisol/estrés → Goluis.UnderPressure. " +
                  "Conduce la receta (ObraCantera/CargarPiedra → ObraMuela/Moler → ObraPila/Apilar) y mira sus humores.");
    }

    /// <summary>
    /// Arranque de la MECÁNICA (Mesocosmos, docs/forge-simulation.md §1): las tareas más simples para
    /// alguien sin experiencia, igual que en la cocina — 1) LIMPIAR el taller (`DirtArea`/`Cleaner`),
    /// 2) ABASTECER/ordenar (cajas en la puerta → estantes, `StockingTask`: aprender dónde va cada cosa),
    /// 3) REPARACIÓN simple (receta con diagnóstico tecleado). Antes de tareas avanzadas (vehículos, drones…).
    /// </summary>
    static void BuildMechanicsBeginner(Transform parent)
    {
        GameObject group = new GameObject("MechanicsBeginner_AUTO");
        group.transform.SetParent(parent);

        // 1) LIMPIAR el taller (virutas/aceite) — reusa el sistema de suciedad.
        GameObject dirtGO = new GameObject("Taller_Suciedad");
        dirtGO.transform.SetParent(group.transform);
        dirtGO.transform.position = new Vector3(50f, 0f, 10f);
        DirtArea dirt = dirtGO.AddComponent<DirtArea>();
        dirt.areaSize = new Vector2(6f, 6f); dirt.spawnInterval = 2f; dirt.maxSpots = 6; dirt.missionThreshold = 4;
        GameObject sweeper = MakeKitchenPerson(group.transform, "Aprendiz_Limpia", new Vector3(50f, 1f, 10f), new Color(0.50f, 0.55f, 0.60f));
        Cleaner sw = sweeper.AddComponent<Cleaner>(); sw.auto = true; sw.autoInterval = 2.5f; sw.reach = 10f;

        // 2) ABASTECER: cajas en la puerta → estantes (StockingTask). Coger de "Caja", colocar en "Despensa".
        MakeStationPart(group.transform, "Caja", "Tornillos", "coges tornillos",   new Vector3(46f,   1f, 20f), new Color(0.60f, 0.60f, 0.40f));
        MakeStationPart(group.transform, "Caja", "Aceite",    "coges aceite",      new Vector3(47.2f, 1f, 20f), new Color(0.50f, 0.40f, 0.20f));
        MakeStationPart(group.transform, "Caja", "Repuesto",  "coges un repuesto", new Vector3(48.4f, 1f, 20f), new Color(0.55f, 0.55f, 0.60f));
        MakeStationPart(group.transform, "Despensa", "Tornillos", "estante: tornillos", new Vector3(46f,   1f, 12f), new Color(0.60f, 0.60f, 0.40f));
        MakeStationPart(group.transform, "Despensa", "Aceite",    "estante: aceite",    new Vector3(47.2f, 1f, 12f), new Color(0.50f, 0.40f, 0.20f));
        MakeStationPart(group.transform, "Despensa", "Repuesto",  "estante: repuestos", new Vector3(48.4f, 1f, 12f), new Color(0.55f, 0.55f, 0.60f));
        GameObject stockGO = new GameObject("Tarea_Abastecer");
        stockGO.transform.SetParent(group.transform);
        StockingTask stock = stockGO.AddComponent<StockingTask>();
        stock.pickStation = "Caja"; stock.slotStation = "Despensa"; stock.total = 3; stock.areaLabel = "el taller";

        // 3) REPARACIÓN simple (receta con diagnóstico tecleado).
        GameObject binGO = new GameObject("Maquinas_Reparadas");
        binGO.transform.SetParent(group.transform);
        binGO.transform.position = new Vector3(54f, 1f, 15f);
        FoodContainer bin = binGO.AddComponent<FoodContainer>(); bin.dishName = "máquina reparada"; bin.capacity = 10;
        StationPart diag = MakeStationPart(group.transform, "Maquina", "Diagnosticar", "diagnosticas (teclea)", new Vector3(52f, 1f, 14f), new Color(0.40f, 0.60f, 0.70f));
        TypingChallenge check = diag.gameObject.AddComponent<TypingChallenge>();
        check.baseTime = 8f; check.reductionPerWord = 1.3f; check.language = "en";
        check.words = new[] { "check", "oil", "bolt", "gear", "fix" };
        diag.timed = check;
        MakeStationPart(group.transform, "Maquina", "Destornillar",   "destornillas la tapa", new Vector3(52f, 1f, 15f), new Color(0.50f, 0.50f, 0.55f));
        MakeStationPart(group.transform, "Maquina", "SustituirPieza", "sustituyes la pieza",  new Vector3(52f, 1f, 16f), new Color(0.55f, 0.55f, 0.60f));
        MakeStationPart(group.transform, "Maquina", "Probar",         "pruebas la máquina",   new Vector3(52f, 1f, 17f), new Color(0.50f, 0.70f, 0.50f));
        GameObject orderGO = new GameObject("Receta_Reparacion");
        orderGO.transform.SetParent(group.transform);
        ProductionOrder order = orderGO.AddComponent<ProductionOrder>();
        order.productName = "máquina reparada"; order.output = bin; order.quota = 2;
        order.stepStation = new[] { "Maquina", "Maquina", "Maquina", "Maquina" };
        order.stepAction  = new[] { "Diagnosticar", "Destornillar", "SustituirPieza", "Probar" };
        order.stepLabel   = new[] { "diagnosticas", "destornillas la tapa", "sustituyes la pieza", "pruebas" };

        Debug.Log("[SampleSceneBuilder] Mechanics beginner (Meso, docs forge §1): 1) LIMPIAR (DirtArea) → " +
                  "2) ABASTECER (cajas→estantes, StockingTask: aprender dónde va cada cosa) → 3) REPARAR " +
                  "(receta con diagnóstico tecleado). Logs [Cocina]/[Abastecer]/[Producción].");
    }

    /// <summary>
    /// Arranque de la CONSTRUCCIÓN (Meso, docs/construction-simulation.md §4): limpiar el solar → abastecer
    /// materiales (cajas→almacén) → construir simple (cimentar[typing]→muro→techar→probar). Como todas las
    /// áreas, empieza por lo simple. Reusa el motor de virtualización.
    /// </summary>
    static void BuildConstructionBeginner(Transform parent)
    {
        GameObject group = new GameObject("ConstructionBeginner_AUTO");
        group.transform.SetParent(parent);

        // 1) LIMPIAR el solar (escombros).
        GameObject dirtGO = new GameObject("Solar_Escombros");
        dirtGO.transform.SetParent(group.transform);
        dirtGO.transform.position = new Vector3(60f, 0f, 10f);
        DirtArea dirt = dirtGO.AddComponent<DirtArea>();
        dirt.areaSize = new Vector2(6f, 6f); dirt.spawnInterval = 2f; dirt.maxSpots = 6; dirt.missionThreshold = 4;
        GameObject sweeper = MakeKitchenPerson(group.transform, "Peon_Limpia", new Vector3(60f, 1f, 10f), new Color(0.55f, 0.50f, 0.45f));
        Cleaner sw = sweeper.AddComponent<Cleaner>(); sw.auto = true; sw.autoInterval = 2.5f; sw.reach = 10f;

        // 2) ABASTECER materiales (cajas → almacén).
        MakeStationPart(group.transform, "Caja", "Ladrillo", "coges ladrillos", new Vector3(56f,   1f, 20f), new Color(0.70f, 0.35f, 0.25f));
        MakeStationPart(group.transform, "Caja", "Madera",   "coges madera",    new Vector3(57.2f, 1f, 20f), new Color(0.55f, 0.40f, 0.20f));
        MakeStationPart(group.transform, "Caja", "Teja",     "coges tejas",     new Vector3(58.4f, 1f, 20f), new Color(0.60f, 0.45f, 0.45f));
        MakeStationPart(group.transform, "Almacen", "Ladrillo", "estante: ladrillos", new Vector3(56f,   1f, 12f), new Color(0.70f, 0.35f, 0.25f));
        MakeStationPart(group.transform, "Almacen", "Madera",   "estante: madera",    new Vector3(57.2f, 1f, 12f), new Color(0.55f, 0.40f, 0.20f));
        MakeStationPart(group.transform, "Almacen", "Teja",     "estante: tejas",     new Vector3(58.4f, 1f, 12f), new Color(0.60f, 0.45f, 0.45f));
        GameObject stockGO = new GameObject("Tarea_Abastecer_Obra");
        stockGO.transform.SetParent(group.transform);
        StockingTask stock = stockGO.AddComponent<StockingTask>();
        stock.pickStation = "Caja"; stock.slotStation = "Almacen"; stock.total = 3; stock.areaLabel = "la obra";

        // 3) CONSTRUIR simple (cimentar tecleando → muro → techar → probar).
        GameObject binGO = new GameObject("Refugios_Levantados");
        binGO.transform.SetParent(group.transform);
        binGO.transform.position = new Vector3(64f, 1f, 15f);
        FoodContainer bin = binGO.AddComponent<FoodContainer>(); bin.dishName = "refugio"; bin.capacity = 10;
        StationPart cimentar = MakeStationPart(group.transform, "Obra", "Cimentar", "cimentas (teclea)", new Vector3(62f, 1f, 14f), new Color(0.50f, 0.45f, 0.40f));
        TypingChallenge lay = cimentar.gameObject.AddComponent<TypingChallenge>();
        lay.baseTime = 8f; lay.reductionPerWord = 1.3f; lay.language = "en";
        lay.words = new[] { "build", "brick", "wall", "wood", "roof" };
        cimentar.timed = lay;
        MakeStationPart(group.transform, "Obra", "Muro",   "levantas el muro", new Vector3(62f, 1f, 15f), new Color(0.70f, 0.35f, 0.25f));
        MakeStationPart(group.transform, "Obra", "Techar", "pones el techo",   new Vector3(62f, 1f, 16f), new Color(0.60f, 0.45f, 0.45f));
        MakeStationPart(group.transform, "Obra", "Probar", "compruebas que aguanta", new Vector3(62f, 1f, 17f), new Color(0.50f, 0.70f, 0.50f));
        GameObject orderGO = new GameObject("Receta_Construccion");
        orderGO.transform.SetParent(group.transform);
        ProductionOrder order = orderGO.AddComponent<ProductionOrder>();
        order.productName = "refugio"; order.output = bin; order.quota = 2;
        order.stepStation = new[] { "Obra", "Obra", "Obra", "Obra" };
        order.stepAction  = new[] { "Cimentar", "Muro", "Techar", "Probar" };
        order.stepLabel   = new[] { "cimentas", "levantas el muro", "pones el techo", "compruebas" };

        Debug.Log("[SampleSceneBuilder] Construction beginner (Meso, docs construction §4): limpiar → " +
                  "abastecer (cajas→almacén) → construir (cimentar tecleando→muro→techar→probar). Logs [Abastecer]/[Producción].");
    }

    /// <summary>
    /// Demo del bucle de REPARACIÓN POR DISPATCH (docs/forge-simulation.md §5): un `ServiceHub` (taller) con
    /// banco de herramientas + un `RepairTicket` (nevera averiada en la Cocina, lejos) cuya receta
    /// `requiresTools`. En Play: sin herramientas la reparación se rechaza; toma herramientas en el hub, ve
    /// a la nevera, repárala (ticket se cierra), vuelve y devuélvelas.
    /// </summary>
    static void BuildDispatchDemo(Transform parent)
    {
        GameObject group = new GameObject("DispatchDemo_AUTO");
        group.transform.SetParent(parent);

        // Taller: banco de herramientas + tablero.
        GameObject hubGO = new GameObject("Taller_Servicio");
        hubGO.transform.SetParent(group.transform);
        ServiceHub hub = hubGO.AddComponent<ServiceHub>();
        hub.hubName = "Taller (Mecánica/Construcción)"; hub.toolStation = "Herramientas";
        MakeStationPart(group.transform, "Herramientas", "Tomar",    "tomas la llave inglesa",     new Vector3(50f, 1f, 18f), new Color(0.85f, 0.70f, 0.25f));
        MakeStationPart(group.transform, "Herramientas", "Devolver", "devuelves las herramientas", new Vector3(51.5f, 1f, 18f), new Color(0.55f, 0.55f, 0.55f));

        // PRIMERA reparación real (la más simple/típica): un GRIFO QUE GOTEA en la Cocina (fontanería).
        // Arreglo real: cerrar la llave de paso → desmontar el grifo → cambiar la junta/goma → montar →
        // abrir y probar. Lejos del taller (junto a la Cocina), y la receta requiere herramientas.
        GameObject binGO = new GameObject("Averias_Resueltas");
        binGO.transform.SetParent(group.transform);
        binGO.transform.position = new Vector3(12f, 1f, 22f);
        FoodContainer bin = binGO.AddComponent<FoodContainer>(); bin.dishName = "avería resuelta"; bin.capacity = 5;
        MakeStationPart(group.transform, "Grifo", "CerrarLlave",  "cierras la llave de paso",   new Vector3(10.5f, 1f, 22f), new Color(0.60f, 0.60f, 0.60f));
        MakeStationPart(group.transform, "Grifo", "Desmontar",    "desmontas la maneta",        new Vector3(11.5f, 1f, 22f), new Color(0.70f, 0.70f, 0.75f));
        MakeStationPart(group.transform, "Grifo", "CambiarJunta", "cambias la junta gastada",   new Vector3(12.5f, 1f, 22f), new Color(0.30f, 0.30f, 0.30f));
        MakeStationPart(group.transform, "Grifo", "Montar",       "montas de nuevo el grifo",   new Vector3(13.5f, 1f, 22f), new Color(0.70f, 0.70f, 0.75f));
        MakeStationPart(group.transform, "Grifo", "AbrirProbar",  "abres la llave y compruebas (ya no gotea)", new Vector3(14.5f, 1f, 22f), new Color(0.50f, 0.75f, 0.90f));
        GameObject orderGO = new GameObject("Receta_RepararGrifo");
        orderGO.transform.SetParent(group.transform);
        ProductionOrder order = orderGO.AddComponent<ProductionOrder>();
        order.productName = "grifo reparado"; order.output = bin; order.quota = 1; order.requiresTools = true;
        order.stepStation = new[] { "Grifo", "Grifo", "Grifo", "Grifo", "Grifo" };
        order.stepAction  = new[] { "CerrarLlave", "Desmontar", "CambiarJunta", "Montar", "AbrirProbar" };
        order.stepLabel   = new[] { "cierras la llave de paso", "desmontas la maneta", "cambias la junta", "montas de nuevo", "abres y compruebas" };
        GameObject ticketGO = new GameObject("Ticket_GrifoCocina");
        ticketGO.transform.SetParent(group.transform);
        RepairTicket ticket = ticketGO.AddComponent<RepairTicket>();
        ticket.area = "Cocina"; ticket.what = "grifo que gotea"; ticket.repair = order;

        Debug.Log("[SampleSceneBuilder] Dispatch demo (docs forge §5): «Taller_Servicio» lista el ticket " +
                  "«grifo que gotea» de la Cocina. Sin herramientas la reparación se rechaza; toma la llave " +
                  "en el banco, ve al grifo (lejos), arréglalo (cerrar→desmontar→cambiar junta→montar→probar) " +
                  "y vuelve a devolver las herramientas. Logs [Servicio]/[Herramientas]/[Producción]/[Ticket].");
    }

    /// <summary>
    /// 1ª simulación de MECÁNICA (docs/forge-simulation.md §5): el **camión** (que lleva las cajas de
    /// suministros a las áreas — las mismas de `StockingTask`) viene al taller a mantenimiento. La primera
    /// reparación es el **CAMBIO DE RUEDA** (procedimiento real: aflojar→gato→quitar→poner→apretar→bajar).
    /// Extras: cambio de aceite (2 pasos) y agua (1 paso). Reusa el motor de virtualización.
    /// </summary>
    static void BuildTruckMaintenance(Transform parent)
    {
        GameObject group = new GameObject("TruckMaintenance_AUTO");
        group.transform.SetParent(parent);

        GameObject truck = GameObject.CreatePrimitive(PrimitiveType.Cube);
        truck.name = "Camion"; truck.transform.SetParent(group.transform);
        truck.transform.position = new Vector3(50f, 1f, 6f);
        truck.transform.localScale = new Vector3(2.5f, 1.5f, 4f);
        truck.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Camion_MAT", new Color(0.35f, 0.45f, 0.55f));

        GameObject binGO = new GameObject("Camion_Mantenido");
        binGO.transform.SetParent(group.transform);
        binGO.transform.position = new Vector3(53f, 1f, 6f);
        FoodContainer bin = binGO.AddComponent<FoodContainer>(); bin.dishName = "mantenimiento"; bin.capacity = 10;

        // 1ª SIMULACIÓN: CAMBIO DE RUEDA (procedimiento real, 6 pasos).
        MakeStationPart(group.transform, "Rueda", "Aflojar",  "aflojas los tornillos",            new Vector3(47f, 1f, 4f),  new Color(0.50f, 0.50f, 0.55f));
        MakeStationPart(group.transform, "Rueda", "Levantar", "levantas con el gato",             new Vector3(47f, 1f, 5f),  new Color(0.60f, 0.60f, 0.40f));
        MakeStationPart(group.transform, "Rueda", "Quitar",   "quitas tornillos y rueda",         new Vector3(47f, 1f, 6f),  new Color(0.40f, 0.40f, 0.40f));
        MakeStationPart(group.transform, "Rueda", "Poner",    "pones la rueda nueva",             new Vector3(47f, 1f, 7f),  new Color(0.28f, 0.28f, 0.28f));
        MakeStationPart(group.transform, "Rueda", "Apretar",  "aprietas los tornillos a mano",    new Vector3(47f, 1f, 8f),  new Color(0.50f, 0.50f, 0.55f));
        MakeStationPart(group.transform, "Rueda", "Bajar",    "bajas el gato y aprietas en cruz", new Vector3(47f, 1f, 9f),  new Color(0.50f, 0.70f, 0.50f));
        GameObject wheelGO = new GameObject("Receta_CambioRueda");
        wheelGO.transform.SetParent(group.transform);
        ProductionOrder wheel = wheelGO.AddComponent<ProductionOrder>();
        wheel.productName = "rueda cambiada"; wheel.output = bin; wheel.quota = 1;
        wheel.stepStation = new[] { "Rueda", "Rueda", "Rueda", "Rueda", "Rueda", "Rueda" };
        wheel.stepAction  = new[] { "Aflojar", "Levantar", "Quitar", "Poner", "Apretar", "Bajar" };
        wheel.stepLabel   = new[] { "aflojas los tornillos", "levantas con el gato", "quitas la rueda", "pones la nueva", "aprietas a mano", "bajas y aprietas en cruz" };

        // Extras del mantenimiento: aceite (2 pasos) y agua (1 paso).
        MakeStationPart(group.transform, "Aceite", "Vaciar",   "vacías el aceite viejo", new Vector3(53f, 1f, 4f), new Color(0.30f, 0.25f, 0.15f));
        MakeStationPart(group.transform, "Aceite", "Rellenar", "rellenas aceite limpio", new Vector3(53f, 1f, 5f), new Color(0.60f, 0.50f, 0.20f));
        GameObject oilGO = new GameObject("Receta_CambioAceite");
        oilGO.transform.SetParent(group.transform);
        ProductionOrder oil = oilGO.AddComponent<ProductionOrder>();
        oil.productName = "aceite cambiado"; oil.output = bin; oil.quota = 1;
        oil.stepStation = new[] { "Aceite", "Aceite" };
        oil.stepAction  = new[] { "Vaciar", "Rellenar" };
        oil.stepLabel   = new[] { "vacías el viejo", "rellenas limpio" };

        MakeStationPart(group.transform, "Agua", "Rellenar", "rellenas el agua del radiador", new Vector3(53f, 1f, 7f), new Color(0.50f, 0.75f, 0.90f));
        GameObject waterGO = new GameObject("Receta_Agua");
        waterGO.transform.SetParent(group.transform);
        ProductionOrder water = waterGO.AddComponent<ProductionOrder>();
        water.productName = "agua rellenada"; water.output = bin; water.quota = 1;
        water.stepStation = new[] { "Agua" };
        water.stepAction  = new[] { "Rellenar" };
        water.stepLabel   = new[] { "rellenas el radiador" };

        Debug.Log("[SampleSceneBuilder] Truck maintenance (docs forge §5): el «Camion» (lleva las cajas a las " +
                  "áreas) viene al taller. 1ª simulación de Mecánica: CAMBIO DE RUEDA (aflojar→gato→quitar→poner→" +
                  "apretar→bajar). Extras: aceite y agua. Logs [Producción].");
    }

    /// <summary>
    /// Sandbox del PRÓLOGO (docs/area-progression.md "Apertura"): el guion (`PrologueSequence`, auto-demo) se
    /// registra en consola; dos `WeakOne` (débiles) caminan a la `CarryToRefuge` (cueva-refugio) y, al
    /// reunirlos, el **Mesocosmos** envía un aviso (`PlaneMessenger`) de volver por la sala de meditación
    /// (`WorldExitPortal`). La escena real (Enfermería→máquina→Microcosmos→salida) se monta en Unity; esto scaffold.
    /// </summary>
    /// <summary>
    /// Área de CRÍA — el corazón del santuario (docs/cria-simulation.md, fauna-gameplay.md): cuidar crías
    /// para ganar su **bond** (que se gana, no se da). Arranque como las demás: 1) limpiar el nido, 2)
    /// abastecer (biberón/comida/paja), 3) **rutina de cuidado** (leer estado → calmar → alimentar → asear →
    /// arrullar) reutilizando `ProductionOrder`, 4) llevar la cría al **nido** (`CarryToRefuge`/`WeakOne`).
    /// </summary>
    /// <summary>
    /// 1ª misión del MICROCOSMOS insecto (docs/microcosmos-insects.md §4): un **hormiguero** (refugio), un
    /// **pulgón** que produce melaza (mirmecofilia) y hace de **mascota-guía** hacia su **familia caída**, y
    /// dos **hormigas** que lo cuidan (lo siguen por la melaza). El pulgón lleva a Kushal/al grupo a la
    /// familia, la rescata y la guía al hormiguero (`CarryToRefuge`/`WeakOne`). Auto-demo por consola.
    /// </summary>
    internal static void BuildMicrocosmosSandbox(Transform parent)
    {
        // Nivel introductorio (alba/cueva) — historia y almas canónicas en docs/microcosmos-insects.md §13.
        // Tableau del momento de la LLEGADA/muerte: Ambrosio (pulgón) se derrumba en la entrada; la tribu
        // (Héspero + Ruth) vela desde la cueva. Cada ser lleva su SoulRecord (nombre de alma · hilo · vida 2).
        GameObject group = new GameObject("MicrocosmosSandbox_AUTO");
        group.transform.SetParent(parent);

        // CUEVA = refugio natural (aún NO hay hormiguero/reina/feromonas — es el amanecer).
        GameObject cave = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cave.name = "Cueva"; cave.transform.SetParent(group.transform);
        cave.transform.position = new Vector3(-30f, 1f, 10f); cave.transform.localScale = new Vector3(2.5f, 1.5f, 2.5f);
        cave.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Cueva_MAT", new Color(0.35f, 0.32f, 0.30f));
        CarryToRefuge refuge = cave.AddComponent<CarryToRefuge>(); refuge.needed = 2; refuge.radius = 4f;

        // ── El ELENCO con IDENTIDAD REAL por ALMA-MEZCLA (rebanada 1, docs/microcosmos-level1.md §2 + soul-composition-blend.md) ──
        // Cada uno es un Ánima real cuya identidad sale de un BLEND de arquetipos (SoulComposition), no de aptitudes a mano:
        //  · CUERPO = su INSECTO real (Ant; Ambrosio = Aphid) dominante + un poco de Human (la "desviación" proto-humana
        //    que los hace especiales). Ambrosio suma GALLINA (la "madre de la naturaleza": cuerpo nutridor, cluecas).
        //  · MENTE = HUMANO de ESTA ERA (~50%, el amanecer del alma: medio emergido del instinto, MUCHO menos individuado
        //    que un humano moderno ≈90%) + su MENTE de insecto (instinto de colonia) + un arquetipo ELEMENTAL/animal que
        //    captura su esencia (Medea=Fuego, Sakshi=Agua-calma, Momo=Mono-juego, Atlas=Bear-firmeza, Héspero=Rock-frío…).
        // SoulComposition (resolveOnStart) computa aptitudes/tono/pensamientos desde los arquetipos → personalidad EMERGENTE.

        // Héspero: vigía de estrellas, abandona a los débiles (hilo A) → Señor del Fuego. Mente fría/jerárquica (Rock).
        Cast(group.transform, "Hespero", new Vector3(-30f, 1f, 12f), new Vector3(0.6f, 0.6f, 0.8f),
            new Color(0.20f, 0.22f, 0.35f), "A", "vigia de las estrellas; villano que abandona a los debiles",
            "Senor del Fuego", "mira al cielo / atraido a lo alto; misma alma-astro (atardecer->alba)",
            new[] { ("Ant", 85f), ("Human", 15f) }, new[] { ("Human", 50f), ("Ant", 30f), ("Rock", 20f) });

        // Ruth: recolectora sumisa (hilo C) → La Sembradora. La MENOS individuada (menos Human, más colonia Ant).
        Cast(group.transform, "Ruth", new Vector3(-31.6f, 1f, 12.2f), new Vector3(0.5f, 0.5f, 0.7f),
            new Color(0.45f, 0.40f, 0.30f), "C", "recolectora sumisa de hongo; la ultima en comer",
            "La Sembradora", "acarrea hongo a todas partes; de recoger a sembrar",
            new[] { ("Ant", 85f), ("Human", 15f) }, new[] { ("Human", 40f), ("Ant", 60f) });
        GameObject hongo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hongo.name = "Hongo_de_Ruth"; hongo.transform.SetParent(group.transform);
        hongo.transform.position = new Vector3(-32.2f, 0.9f, 12.2f); hongo.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        hongo.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Hongo_MAT", new Color(0.75f, 0.70f, 0.55f));

        // Ambrosio (pulgón mártir NUTRIDOR) → Nasatya. CUERPO = Aphid + GALLINA (madre de la naturaleza) + Human; MENTE gentil (Agua).
        GameObject aphid = Cast(group.transform, "Ambrosio", new Vector3(-30f, 0.7f, 13.5f), new Vector3(0.9f, 0.7f, 1.1f),
            new Color(0.60f, 0.80f, 0.45f), "-", "el pulgon martir; cuerpo blando y grande que nutre a todos (madre-gallina)",
            "Nasatya", "postura rara (eco de la deformidad); manso pese a fuerza; cuerpo-almohada / clueca",
            new[] { ("Aphid", 40f), ("Gallina", 40f), ("Human", 20f) }, new[] { ("Human", 45f), ("Aphid", 35f), ("Agua", 20f) });
        aphid.AddComponent<HoneydewProducer>().interval = 30f; // "dias sin producir jugos"

        // Medea: gemela débil, forja veneno (hilo B) → tirana del veneno (E). Mente ígnea (Fuego).
        Cast(group.transform, "Medea", new Vector3(-30.8f, 0.8f, 14.2f), new Vector3(0.35f, 0.35f, 0.5f),
            new Color(0.30f, 0.20f, 0.22f), "B", "gemela debil; defiende a Ambrosio; forja veneno y armas",
            "tirana del veneno/feromonas (vida 2 -> hilo E)", "mandibula apretada; se aferra al veneno/la muerte",
            new[] { ("Ant", 85f), ("Human", 15f) }, new[] { ("Human", 45f), ("Fire", 35f), ("Ant", 20f) });

        // Momo: bromista magnética, fuerza oculta (hilo G) → el bufón. Mente juguetona (Mono).
        Cast(group.transform, "Momo", new Vector3(-29.2f, 0.8f, 14.4f), new Vector3(0.45f, 0.45f, 0.6f),
            new Color(0.55f, 0.45f, 0.20f), "G", "bromista magnetica; fuerza oculta; trono infantil",
            "el bufon de la era del fuego (propuesta)", "todo a broma; mas fuerte de lo que aparenta",
            new[] { ("Ant", 80f), ("Human", 20f) }, new[] { ("Human", 50f), ("Mono", 30f), ("Ant", 20f) });

        // Atlas: el único adulto, el más fuerte, sostén (hilo E) → fuerza benévola. CUERPO con Bear; mente firme (Bear).
        Cast(group.transform, "Atlas", new Vector3(-31.5f, 1f, 16.5f), new Vector3(0.6f, 0.6f, 0.9f),
            new Color(0.30f, 0.28f, 0.24f), "E", "el mas fuerte; sosten del bienestar; enmienda a Momo; pilar de Medea",
            "fuerza benevola (propuesta)", "carga el peso de otros",
            new[] { ("Ant", 65f), ("Bear", 20f), ("Human", 15f) }, new[] { ("Human", 55f), ("Bear", 25f), ("Ant", 20f) });

        // Sakshi: observadora de la mente (hilo F) → El Chamán. La MÁS individuada (más Human); mente contemplativa (Agua).
        Cast(group.transform, "Sakshi", new Vector3(-28.7f, 1f, 16.8f), new Vector3(0.5f, 0.5f, 0.7f),
            new Color(0.35f, 0.40f, 0.45f), "F", "observadora de la mente; crio a Ambrosio; se queda varada",
            "El Chaman (propuesta)", "se pierde en la contemplacion hasta que la traen de vuelta",
            new[] { ("Ant", 85f), ("Human", 15f) }, new[] { ("Human", 60f), ("Agua", 20f), ("Ant", 20f) });

        // Los ANCIANOS frágiles (inmóviles hasta ser cargados) — WeakOne. Uno es el primer PINTOR de la cueva (hilo D).
        GameObject pintor = Cast(group.transform, "Anciano_Pintor", new Vector3(-31f, 1f, 19.5f), new Vector3(0.5f, 0.5f, 0.6f),
            new Color(0.50f, 0.46f, 0.40f), "D", "el primero que marca/pinta la cueva al llegar",
            "", "deja huellas/marcas por donde pasa (hilo D, nombre pendiente)",
            new[] { ("Ant", 88f), ("Human", 12f) }, new[] { ("Human", 55f), ("Mono", 25f), ("Ant", 20f) });
        pintor.AddComponent<WeakOne>();
        pintor.AddComponent<AiBrain>().selfRelevance = 1f;
        pintor.AddComponent<AnimaController>();

        // SHANIDAR: el anciano herido que la banda cuida. Lleva el nombre y las heridas del neandertal Shanidar 1 (sin
        // antebrazo derecho, ojo izquierdo dañado, cojera, sordera), que llegó a viejo porque su grupo lo cuidó
        // (doi 10.1371/journal.pone.0186684). Ver docs/compassion-simulations.md.
        GameObject anciano = Cast(group.transform, "Shanidar", new Vector3(-29.4f, 1f, 19.5f), new Vector3(0.5f, 0.5f, 0.6f),
            new Color(0.50f, 0.46f, 0.40f), "", "anciano herido de la banda: sin antebrazo derecho, medio ciego y sordo, cojo; vive porque lo cuidan",
            "", "necesita que lo carguen al refugio",
            new[] { ("Ant", 90f), ("Human", 10f) }, new[] { ("Human", 40f), ("Ant", 60f) });
        BodyInjuries wounds = anciano.AddComponent<BodyInjuries>();
        wounds.missingRightForearm = true; wounds.blindLeftEye = true; wounds.limp = true; wounds.hearingLoss = true;
        anciano.AddComponent<WeakOne>();
        anciano.AddComponent<AiBrain>().selfRelevance = 1f;
        anciano.AddComponent<AnimaController>();

        // ── REBANADA 2: MOVIMIENTO por IMPULSOS SOCIALES + COHESIÓN ──────────────────────────────────────────
        // El elenco deja de ser estático: se mueve por drives sociales (sobre el ImpulseController). La tribu VELA y
        // CUIDA a Ambrosio (Tend); Sakshi se vuelca (magnitud alta → se separa del grupo → baja la cohesión); Héspero
        // se APARTA de los débiles (Cull); Ruth obedece a Héspero y recoge comida. Ambrosio (colapsado) no se mueve.
        Vector3 caveArea = new Vector3(-30f, 0.5f, 12f);   // querencia suave a la cueva
        Transform ambrosioT = aphid.transform;
        foreach (string who in new[] { "Sakshi", "Medea", "Momo", "Atlas" })
        {
            Transform t = group.transform.Find(who); if (t == null) continue;
            Mobilize(t.gameObject, caveArea);
            Drive(t.gameObject, SocialImpulse.SocialDrive.Tend, ambrosioT, who == "Sakshi" ? 1.4f : 1.0f);   // Sakshi se vuelca (se separa)
        }
        Transform hesperoT = group.transform.Find("Hespero");
        if (hesperoT != null) { Mobilize(hesperoT.gameObject, caveArea); Drive(hesperoT.gameObject, SocialImpulse.SocialDrive.Cull, null, 0.8f); }
        Transform ruthT = group.transform.Find("Ruth");
        if (ruthT != null)
        {
            Mobilize(ruthT.gameObject, caveArea);
            Drive(ruthT.gameObject, SocialImpulse.SocialDrive.Obey, hesperoT, 0.6f);
            Drive(ruthT.gameObject, SocialImpulse.SocialDrive.Gather, null, 0.5f);
        }

        // Piso de la cueva → NavMesh bajo el tableau (si no hay, los impulsos no mueven: graceful).
        GameObject caveFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        caveFloor.name = "CaveFloor"; caveFloor.transform.SetParent(group.transform);
        caveFloor.transform.position = new Vector3(-30f, 0f, 15f); caveFloor.transform.localScale = new Vector3(1.2f, 1f, 1.6f);
        caveFloor.GetComponent<Renderer>().sharedMaterial = MakeMaterial("CaveFloor_MAT", new Color(0.28f, 0.26f, 0.24f));

        // Medidor de COHESIÓN/ABANDONO (legible) + DIRECTOR de beats (rebanada 3): secuencia los beats del alba
        // sembrando circunstancias (campos de pensamiento/humor) y leyendo el estado; engancha la emoción del elenco.
        group.AddComponent<TribeCohesion>();
        group.AddComponent<Level1Director>();

        Debug.Log("[SampleSceneBuilder] Microcosmos — tableau del alba (docs microcosmos-level1.md): el elenco tiene " +
                  "IDENTIDAD por ALMA-MEZCLA + IMPULSOS SOCIALES + COHESIÓN, y un LEVEL1DIRECTOR que secuencia los beats " +
                  "(observar→cuidar→abandono→desercion→muerte) sembrando circunstancias + emoción (rebanadas 1-3).");
    }

    // Rebanada 2: da a un miembro del elenco el stack de MOVIMIENTO (NavMeshAgent + ImpulseController + querencia suave a
    // la cueva). Sin coste de energía (el tableau no es el reto de energía del mapa jugable). Idempotente.
    internal static void Mobilize(GameObject go, Vector3 home)
    {
        UnityEngine.AI.NavMeshAgent agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null) agent = go.AddComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = 0.8f; agent.radius = 0.25f; agent.height = 0.6f;
        ImpulseController ctrl = go.GetComponent<ImpulseController>();
        if (ctrl == null) ctrl = go.AddComponent<ImpulseController>();
        ctrl.walkEnergyCostPerSecond = 0f;
        HomeImpulse home_i = go.GetComponent<HomeImpulse>();
        if (home_i == null) home_i = go.AddComponent<HomeImpulse>();
        home_i.homePosition = home; home_i.baseMagnitude = 0.25f;
    }

    // Rebanada 2: añade un drive social (impulso) a un miembro. Varios drives conviven (tags distintos).
    internal static SocialImpulse Drive(GameObject go, SocialImpulse.SocialDrive drive, Transform target, float mag)
    {
        SocialImpulse s = go.AddComponent<SocialImpulse>();
        s.drive = drive; s.fixedTarget = target; s.magnitude = mag;
        return s;
    }

    /// <summary>Rebanada 1 de identidad por ALMA-MEZCLA (docs/microcosmos-level1.md §2 + soul-composition-blend.md): crea un
    /// miembro del elenco como ÁNIMA REAL — visual (MakeInsect) + <see cref="SoulRecord"/> + <see cref="Anima"/> +
    /// <see cref="Mind"/> + <see cref="SoulComposition"/> con los BLEND-slots de cuerpo/mente (arquetipo, %). En Start
    /// (resolveOnStart) el blend computa aptitudes/tono/pensamientos desde los arquetipos reales. `applyScale=false` para
    /// respetar la escala autorada del insecto. Aditivo: NO añade IA/movimiento (rebanadas 2-3). Devuelve el GameObject.</summary>
    internal static GameObject Cast(Transform parent, string name, Vector3 pos, Vector3 scale, Color col,
                           string hilo, string vida1, string vida2, string tell,
                           (string arch, float dom)[] bodies, (string arch, float dom)[] minds)
    {
        GameObject go = MakeInsect(parent, name, pos, scale, col);
        AddSoul(go, name, hilo, vida1, vida2, tell);

        go.AddComponent<Anima>();
        Mind m = go.AddComponent<Mind>();
        m.identity = name;

        SoulComposition soul = go.AddComponent<SoulComposition>();
        soul.applyScale = false;   // conservar la escala autorada del insecto (no la altura del blend)
        foreach (var (arch, dom) in bodies) soul.bodies.Add(new BlendSlot { archetype = arch, domain = dom });
        foreach (var (arch, dom) in minds)  soul.minds.Add(new BlendSlot { archetype = arch, domain = dom });
        go.AddComponent<AcuteStressResponse>();   // control-por-necesidad también en animas COMPUESTAS (todo es Anima): el "body" puja por el mando
        go.AddComponent<ActionQueue>();           // cola de acciones (own/player)
        go.AddComponent<IdleBehavior>();          // libre → se encola merodeos (usa la cola en vez de actuar directo)
        return go;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // NIVEL 1 GAMEPLAY — mapa abierto hormiga
    // SpellBase / WeaknessEffect / PullSpell / HoneydewSpell / FormicAcidSpray
    // (PR #62, docs microcosmos-insects.md §13)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Monta el sandbox jugable del Nivel 1 Microcosmos:
    /// <list type="bullet">
    ///   <item>Suelo plano 40×40 m (escala insecto).</item>
    ///   <item><b>Cueva-checkpoint</b>: <see cref="CarryToRefuge"/> needed=4.</item>
    ///   <item><b>Hormigas viejas</b>: <see cref="Anima"/>+<see cref="CharacterLevel"/>+
    ///         <see cref="WeaknessEffect"/>+<see cref="NavMeshAgent"/>+<see cref="ImpulseController"/>
    ///         +<see cref="HomeImpulse"/> (→ cueva) + <see cref="ThreatScanner"/>+<see cref="WeakOne"/>.
    ///         Sin energía → ImpulseController no puede pagar walkCost → se paran (comportamiento emergente).</item>
    ///   <item><b>Ambrosio</b>: pulgón con <see cref="HoneydewProducer"/> (×5/8 s).</item>
    ///   <item><b>Kushal</b>: <see cref="FormicAcidSpray"/>+<see cref="PullSpell"/>+
    ///         <see cref="HoneydewSpell"/>+<see cref="CombatTargetSelector"/>.</item>
    ///   <item><b>Depredadores</b>: Mariquita/Escarabajo/Araña lobo con stats escalados.</item>
    /// </list>
    /// ⚠ Requiere NavMesh bakeado (Window → AI → Navigation → Bake) antes de Play.
    /// </summary>
    internal static void BuildNivel1Sandbox(Transform parent)
    {
        GameObject root = new GameObject("Nivel1Microcosmos_AUTO");
        root.transform.SetParent(parent);

        // ── Suelo ──────────────────────────────────────────────────────────
        // Plano grande que simula el suelo del bosque a escala insecto.
        // ThreatEmitter: la selva ES peligrosa — todas las hormigas perciben amenaza
        // ambiental constante aunque no vean depredadores. Las viejas (bajo poder) sienten
        // más peligro específico que las jóvenes, lo que dispara PackAwareness en las jóvenes.
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "ForestFloor"; floor.transform.SetParent(root.transform);
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(4f, 1f, 4f); // 40x40 m a escala insecto
        floor.GetComponent<Renderer>().sharedMaterial =
            MakeMaterial("ForestFloor_MAT", new Color(0.25f, 0.18f, 0.10f));
        // Añadir un Collider al plano (Plane ya lo tiene) y ThreatEmitter.
        var floorThreat = floor.AddComponent<ThreatEmitter>();
        floorThreat.threatPower = 1.5f;  // amenaza baja-media: real pero tolerable para los fuertes
        floorThreat.radius      = 30f;   // cubre todo el mapa
        floorThreat.falloff     = 0.2f;  // casi constante (la selva peligrosa no tiene zona segura)

        // ── Cueva-checkpoint ────────────────────────────────────────────────
        // El grupo debe llegar aquí. CarryToRefuge cuenta las WeakOne en radio.
        Vector3 cavePos = new Vector3(0f, 0.5f, -16f);
        GameObject cave = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cave.name = "CuevaCheckpoint"; cave.transform.SetParent(root.transform);
        cave.transform.position = cavePos; cave.transform.localScale = new Vector3(4f, 2f, 4f);
        cave.GetComponent<Renderer>().sharedMaterial =
            MakeMaterial("Cave_MAT", new Color(0.30f, 0.27f, 0.24f));
        CarryToRefuge checkpoint = cave.AddComponent<CarryToRefuge>();
        checkpoint.radius = 4f; checkpoint.needed = 4; // 4 hormigas viejas → misión completa

        // ── Hormigas viejas (comportamiento emergente) ──────────────────────
        // ImpulseController suma: HomeImpulse(→cueva) + ThreatScanner(flee depredadores).
        // WeaknessEffect drena energía → ImpulseController.walkEnergyCostPerSecond no puede
        // pagarse → se paran sin que nadie les diga explícitamente que paren.
        // Hay 5 pero solo se necesitan 4 → tensión: Kushal no puede salvar a todas.
        string[] antNames = { "Vieja_1", "Vieja_2", "Vieja_3", "Vieja_4", "Vieja_5" };
        Vector3[] antPositions = {
            new Vector3(-3f, 0.5f, 8f),
            new Vector3(0f,  0.5f, 10f),
            new Vector3(3f,  0.5f, 7f),
            new Vector3(-2f, 0.5f, 14f),
            new Vector3(4f,  0.5f, 12f),
        };
        foreach (var (antName, pos) in System.Linq.Enumerable.Zip(antNames, antPositions,
            (n, p) => (n, p)))
        {
            GameObject ant = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            ant.name = antName; ant.transform.SetParent(root.transform);
            ant.transform.position = pos; ant.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            ant.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial($"{antName}_MAT", new Color(0.55f, 0.48f, 0.38f));

            var anima = ant.AddComponent<Anima>();
            anima.agility = 0.3f; anima.strength = 0.2f; anima.bodyMass = 0.15f;
            anima.endurance = 0.2f;
            // autoabandono bajo: son viejas y débiles, su prioridad es sobrevivir.
            // Las PackAwareness de las hormigas JÓVENES (Kushal y futuras) serán las que
            // evalúen ayudarlas: selfPower(joven) alto → peligroEspec bajo → condición se cumple.
            anima.autoabandono = 0.15f;

            var cl = ant.AddComponent<CharacterLevel>();
            cl.stats.level = 1; cl.yoga.level = 1; cl.bonds.level = 1;

            // WeaknessEffect: drena energía. controlAgent = false porque
            // ImpulseController gestiona el movimiento vía walkEnergyCostPerSecond.
            var weak = ant.AddComponent<WeaknessEffect>();
            weak.drainPerSecond = 4f;
            weak.controlAgent = false; // ImpulseController controla si puede moverse

            var agent = ant.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = 1.0f; agent.radius = 0.2f; agent.height = 0.4f;

            // Sistema de impulsos: reemplaza WalkToGoal.
            var impulseCtrl = ant.AddComponent<ImpulseController>();
            impulseCtrl.walkEnergyCostPerSecond = 1.5f; // sin energía → se paran

            var homeImp = ant.AddComponent<HomeImpulse>();
            homeImp.homePosition = cavePos;
            homeImp.baseMagnitude = 0.5f;
            homeImp.stressBonusPerUnit = 2f; // más estrés → más prisa por llegar al nido
            homeImp.arrivalRadius = 3f;

            // ThreatScanner: detecta depredadores Anima + ThreatEmitter (suelo de selva).
            // Radio amplio para recoger el ThreatEmitter del suelo aunque esté lejos.
            var scanner = ant.AddComponent<ThreatScanner>();
            scanner.scanRadius = 25f;    // debe ser >= floorThreat.radius para detectar el suelo
            scanner.fearThreshold = 0.2f; // sensibles (viejas = poca confianza)
            scanner.maxFleeMagnitude = 4f;
            scanner.decayRate = 1.5f;

            // ATPRegenSpell: bloqueado por WeaknessEffect.blockReserveRegen → no se autorecarga.
            var regen = ant.AddComponent<ATPRegenSpell>();
            regen.regenPerSecond = 6f;

            // PackAwareness: cada hormiga escanea compañeras con ThreatScanner.
            // Las viejas tienen autoabandono bajo → rara vez ayudan.
            // Las jóvenes (Kushal, futuras) con más autoabandono + selfPower → condición cumplida.
            var pack = ant.AddComponent<PackAwareness>();
            pack.packRadius = 10f;
            pack.missionAbandonoBonus = 0.2f;

            ant.AddComponent<WeakOne>(); // CarryToRefuge las cuenta al entrar en radio
        }

        // ── Ambrosio (pulgón productor) ─────────────────────────────────────
        // Cuerpo blando y grande. Produce 5 gotas (recurso limitado) cada 8 s.
        // El pickup (HoneydewPickup) se spawna vía prefab → en el sandbox usamos el inspector.
        Vector3 aphidPos = new Vector3(-1f, 0.6f, 4f);
        GameObject aphid = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        aphid.name = "Ambrosio_Aphid"; aphid.transform.SetParent(root.transform);
        aphid.transform.position = aphidPos; aphid.transform.localScale = new Vector3(0.8f, 0.6f, 0.9f);
        aphid.GetComponent<Renderer>().sharedMaterial =
            MakeMaterial("Ambrosio_MAT", new Color(0.60f, 0.80f, 0.45f));
        var producer = aphid.AddComponent<HoneydewProducer>();
        producer.interval = 8f; producer.maxTotal = 5;
        // dropPickup se asigna en inspector (prefab de HoneydewPickup esfera amarilla pequeña).
        aphid.AddComponent<Anima>().bodyMass = 0.8f; // grande y lento = fácil de ignorar

        // ── Honeydew pickup de ejemplo (sin prefab aún) ─────────────────────
        // Esfera pequeña amarilla en el suelo junto a Ambrosio para probar la recolección.
        GameObject drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        drop.name = "HoneydewDrop_Sample"; drop.transform.SetParent(root.transform);
        drop.transform.position = aphidPos + new Vector3(0.8f, 0.1f, 0f);
        drop.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        drop.GetComponent<Renderer>().sharedMaterial =
            MakeMaterial("Honeydew_MAT", new Color(0.95f, 0.85f, 0.20f));
        drop.AddComponent<HoneydewPickup>().lifetime = 120f;

        // ── Kushal (jugador) ─────────────────────────────────────────────────
        // Hormiga jugadora: FormicAcidSpray (Q) + PullSpell (E→target) + HoneydewSpell (E→honeydew).
        // En el sandbox es una cápsula; el avatar real se conecta desde la escena de Unity.
        Vector3 kushalPos = new Vector3(0f, 0.5f, 2f);
        GameObject kushal = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        kushal.name = "Kushal_Ant"; kushal.transform.SetParent(root.transform);
        kushal.transform.position = kushalPos; kushal.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        kushal.GetComponent<Renderer>().sharedMaterial =
            MakeMaterial("Kushal_MAT", new Color(0.22f, 0.40f, 0.65f));

        var kAnima = kushal.AddComponent<Anima>();
        kAnima.agility = 0.6f; kAnima.strength = 0.5f; kAnima.bodyMass = 0.2f; kAnima.endurance = 0.6f;
        // Kushal tiene autoabandono alto: es el personaje del jugador, dispuesto a ayudar.
        // selfPower(Kushal) > viejas → peligroEspec(vieja) = floorDanger - kushalPower es bajo
        // → condición: autoabandono(0.7) + bond(0.4) > peligroEspec → siempre cumplida para viejas.
        kAnima.autoabandono = 0.7f;

        var kLevel = kushal.AddComponent<CharacterLevel>();
        kLevel.stats.level = 1; kLevel.yoga.level = 1; kLevel.bonds.level = 1;

        // Selector de target (Tab para ciclar, Escape para deseleccionar).
        kushal.AddComponent<CombatTargetSelector>();

        // ThreatScanner + PackAwareness: Kushal detecta el peligro ambiental y evalúa ayudar.
        // Con selfPower alto y autoabandono=0.7, casi siempre decide ayudar a las viejas.
        var kScanner = kushal.AddComponent<ThreatScanner>();
        kScanner.scanRadius = 25f; kScanner.fearThreshold = 0.3f; kScanner.maxFleeMagnitude = 3f;
        var kPack = kushal.AddComponent<PackAwareness>();
        kPack.packRadius = 12f; kPack.missionAbandonoBonus = 0.15f;

        // Hechizos.
        // PullSpell (F mantenido): forcejeo adaptativo.
        // force = power inicial; sube (rampRate) solo si la hormiga no avanza.
        // Techo = min(maxPullPower, force + Kushal.Strength).
        // Energía del caster = casterEnergyPerPower × power × dt.
        var pull = kushal.AddComponent<PullSpell>();
        pull.range = 4f; pull.force = 1.5f; pull.rampRate = 0.8f; pull.maxPullPower = 7f;
        pull.casterEnergyPerPower = 0.4f; pull.targetEnergyCostPerSecond = 1.2f;

        var honeydew = kushal.AddComponent<HoneydewSpell>();
        honeydew.range = 3f; honeydew.energyRestore = 40f; honeydew.maxCharges = 5;
        honeydew.castKey = KeyCode.E;

        // Defensa corporal (ácido fórmico, Q).
        // FormicAcidSpray hereda SpellBase: range = radio del spray, force = estrés por hit.
        var acid = kushal.AddComponent<FormicAcidSpray>();
        acid.range = 1.2f; acid.force = 0.35f; acid.maxCharges = 3;

        // ── Depredadores (3 zonas de dificultad) ───────────────────────────────
        // Zona fácil (Z=15-18): MARIQUITA — come pulgones/piojos; baja masa, poco peligrosa.
        BuildPredator(root.transform, "Mariquita", new Vector3(-5f, 0.5f, 16f),
            new Vector3(0.3f, 0.2f, 0.35f), new Color(0.85f, 0.15f, 0.10f),
            agility: 0.5f, strength: 0.3f, mass: 0.2f);

        // Zona media (Z=8-12): ESCARABAJO TERRESTRE — resistente, más lento.
        BuildPredator(root.transform, "Escarabajo", new Vector3(8f, 0.5f, 10f),
            new Vector3(0.5f, 0.35f, 0.55f), new Color(0.15f, 0.10f, 0.05f),
            agility: 0.35f, strength: 0.6f, mass: 0.5f);

        // Zona difícil (Z=18-22): ARAÑA LOBO — rápida, gran masa, muy peligrosa.
        BuildPredator(root.transform, "Arana_Lobo", new Vector3(5f, 0.5f, 20f),
            new Vector3(0.65f, 0.5f, 0.7f), new Color(0.40f, 0.38f, 0.30f),
            agility: 0.75f, strength: 0.8f, mass: 0.7f);

        Debug.Log("[SampleSceneBuilder] Nivel1Microcosmos_AUTO: mapa abierto (40x40 m), cueva-checkpoint " +
                  "(CarryToRefuge×4), 5 hormigas viejas (ImpulseController+HomeImpulse+ThreatScanner+WeaknessEffect+WeakOne), " +
                  "Ambrosio (HoneydewProducer ×5/8s), Kushal hormiga (PullSpell/HoneydewSpell/FormicAcidSpray), " +
                  "3 depredadores por zona (Mariquita/Escarabajo/Araña). " +
                  "NavMesh: lo hornea MicrocosmosSceneBuilder.BakeMicroNavMesh (no hace falta hornearlo a mano).");
    }

    /// <summary>Crea un depredador de sandbox (cápsula + Anima con stats calibrados).</summary>
    static void BuildPredator(Transform parent, string predName, Vector3 pos, Vector3 scale, Color col,
        float agility, float strength, float mass)
    {
        GameObject pred = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        pred.name = predName; pred.transform.SetParent(parent);
        pred.transform.position = pos; pred.transform.localScale = scale;
        pred.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"{predName}_MAT", col);

        var anima = pred.AddComponent<Anima>();
        anima.agility = agility; anima.strength = strength; anima.bodyMass = mass;
        // Sin ITarget/faction: sandbox básico — la depredación real requiere Animal.
        pred.AddComponent<AiBrain>().selfRelevance = 1.5f;
        pred.AddComponent<AnimaController>();
    }

    // UNA ESCENA POR SESIÓN de yoga: un demostrador (cuerpo de cubos) que ejecuta la secuencia en bucle, para revisarla
    // por separado. Extensible: al añadir una sesión a YogaLibrary, basta una entrada más en TestScenes + su wrapper.
    static void BuildYogaUpaNeck(Transform parent)   => BuildYogaSessionScene(parent, "Upa-yoga de cuello");
    static void BuildYogaNamaskar(Transform parent)  => BuildYogaSessionScene(parent, "Yoga Namaskar");
    static void BuildYogaSaludoSol(Transform parent) => BuildYogaSessionScene(parent, "Saludo al Sol");
    static void BuildYogaAshtanga1(Transform parent) => BuildYogaSessionScene(parent, "Ashtanga Primaria");

    static void BuildYogaSessionScene(Transform parent, string session)
    {
        GameObject root = new GameObject($"Yoga_{session}_AUTO"); root.transform.SetParent(parent);
        GameObject d = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        d.name = $"Yogui ({session})"; d.transform.SetParent(root.transform); d.transform.position = new Vector3(0f, 1f, 0f);
        var r = d.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = MakeMaterial("Yogui_MAT", new Color(0.8f, 0.78f, 0.9f));
        d.AddComponent<Anima>();
        d.AddComponent<Mind>().identity = "Yogui";
        d.AddComponent<YogaSessionRunner>().sessionName = session;   // viste cuerpo de cubos + ejecuta la sesión en bucle
        d.AddComponent<WorldLabel>().text = session;
    }

    // 1ª virtualización de yoga: solo necesita el componente en escena (UI OnGUI, arranca sola en Play).
    static void BuildUpaYogaSandbox(Transform parent)
    {
        GameObject go = new GameObject("UpaYogaSandbox_AUTO");
        go.transform.SetParent(parent);
        go.AddComponent<UpaYogaSession>();   // autoStart = true → muestra paneles-tecla + secuencia al entrar en Play
        Debug.Log("[SampleSceneBuilder] Upa-yoga (cuello): UpaYogaSandbox_AUTO. En Play muestra los paneles-tecla " +
                  "(WASD/IJKL con letra), título=parte del cuerpo, barra de aliento; F = siguiente fase (docs upa-yoga-mission.md).");
    }

    // Cámara artística: efectos de pantalla por estado. debugAutoCycle oscila la intensidad para verlos sin jugador.
    static void BuildScreenEffectsSandbox(Transform parent)
    {
        GameObject go = new GameObject("ScreenEffectsSandbox_AUTO");
        go.transform.SetParent(parent);
        ScreenEffects fx = go.AddComponent<ScreenEffects>();
        fx.debugAutoCycle = true;   // sube/baja la intensidad en Play → se ven párpados/gris/viñeta roja
        Debug.Log("[SampleSceneBuilder] ScreenEffects: ScreenEffectsSandbox_AUTO (debugAutoCycle). En Play oscilan " +
                  "sueño (párpados)/fatiga (gris)/estrés (viñeta roja). Con jugador real, leen sus stats (docs stats-as-truth §6).");
    }

    // Orquesta emocional: un conductor (EmotionExpression, debugDrive) + partes-cubo que reaccionan.
    static void BuildEmotionOrchestraSandbox(Transform parent)
    {
        GameObject root = new GameObject("EmotionOrchestraSandbox_AUTO");
        root.transform.SetParent(parent);
        root.transform.position = new Vector3(6f, 1.5f, 6f);
        EmotionExpression exp = root.AddComponent<EmotionExpression>();
        exp.debugDrive = true;   // oscila humores → la orquesta reacciona
        exp.field = root.AddComponent<ThoughtField>();   // contagio: proyecta la emoción intensa a los cercanos

        // Cada cubo = una "parte" con un rol distinto (traducción entre especies).
        MakeReactor(root.transform, "Quijada", new Vector3(0f, 0f, 0f), Vector3.right, 4f, 4f, 40f);   // tic ante el Jolt (violento)
        MakeReactor(root.transform, "Antena", new Vector3(0.5f, 0.3f, 0f), Vector3.forward, 30f, 6f, 15f); // se mece con la activación
        MakeReactor(root.transform, "Hombro", new Vector3(-0.5f, 0f, 0f), Vector3.right, 6f, 30f, 8f);  // se hunde con la valencia baja
        MakeReactor(root.transform, "Conejo_freeze", new Vector3(0f, 0.4f, 0.4f), Vector3.right, 20f, 6f, 10f).freezeOnFear = true; // se CONGELA ante el miedo

        // Legibilidad: un lector cercano traduce la orquesta a "qué siente / quiere / hará".
        GameObject reader = new GameObject("EmotionReader_AUTO");
        reader.transform.SetParent(parent);
        reader.transform.position = new Vector3(4f, 1.5f, 6f);
        reader.AddComponent<EmotionReader>();

        Debug.Log("[SampleSceneBuilder] Orquesta emocional: EmotionOrchestraSandbox_AUTO (debugDrive). En Play, " +
                  "«Quijada» da tics ante cambios violentos, «Antena» se mece con la activación, «Hombro» se hunde " +
                  "con la valencia baja; «EmotionReader_AUTO» muestra la lectura sobre la orquesta (docs emotion-model.md).");
    }

    // ── Descomposición (cocina como trabajo) ───────────────────────────────────
    // Minijuego de 3 fases (identificar/romper/clasificar) SIN Anima: el lote descompuesto va a la economía
    // del santuario (workerCut=0) → Elements/Energy suben en el HUD de recursos. docs magic-metabolism §14/§17.
    static void BuildDecompositionSandbox(Transform parent)
    {
        GameObject group = new GameObject("Descomposicion_AUTO");
        group.transform.SetParent(parent);
        group.transform.position = new Vector3(8f, 1.5f, 6f);

        DecompositionJob job = group.AddComponent<DecompositionJob>();
        job.level = DecompositionLevel.Compound;
        job.workerCut = 0f;   // sin obrero → TODO a la economía del santuario (visible en el HUD de recursos)

        DecompositionMinigame game = group.AddComponent<DecompositionMinigame>();
        game.job = job;
        game.perItemSeconds = 6f;
        game.batch = new List<DecompSample>
        {
            MakeDecompSample("H2O (agua)", new[] { "H2O (agua)", "CO2", "NaCl", "O2" }, 200f,
                new[] { "H", "O" }, new[] { 2f, 16f }),
            MakeDecompSample("NaCl (sal)", new[] { "NaCl (sal)", "H2O (agua)", "CaCO3", "N2" }, 150f,
                new[] { "Na", "Cl" }, new[] { 23f, 35f }),
            MakeDecompSample("CO2", new[] { "CO2", "O2", "CH4", "H2O (agua)" }, 300f,
                new[] { "C", "O" }, new[] { 12f, 32f }),
        };

        Debug.Log("[SampleSceneBuilder] Descomposicion_AUTO: en Play, botón «Iniciar jornada» → 3 fases " +
                  "(identificar/romper/clasificar). Al terminar, Elements/Energy suben en el HUD de recursos (docs magic-metabolism §17).");
    }

    static DecompSample MakeDecompSample(string trueName, string[] options, float joules, string[] syms, float[] grams)
    {
        DecompSample s = new DecompSample { trueName = trueName, energyJoules = joules };
        s.nameOptions = new List<string>(options);
        s.components = new List<ElementAmount>();
        for (int i = 0; i < syms.Length && i < grams.Length; i++)
            s.components.Add(new ElementAmount { symbol = syms[i], amount = grams[i] });
        return s;
    }

    // ── Magia (bucle comer→desbloquear→lanzar) ──────────────────────────────────
    // Sin Anima concreto: los componentes de magia son null-safe. Un HUD de prueba (MagicSandboxDriver) conduce
    // aprender el 1er hechizo, comer, quarks y lanzar fuego. docs magic-metabolism §16, testing-checklist §15.
    static void BuildMagicSandbox(Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Magia_AUTO";
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(10f, 1f, 6f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Magia_AUTO_MAT", new Color(0.40f, 0.30f, 0.70f));

        Anima anima = go.AddComponent<Anima>();   // Anima concreto mínimo (no-op): comer sube stats + exceso→grasa
        Constitution con = go.AddComponent<Constitution>();
        con.anima = anima;
        Metabolism met = go.AddComponent<Metabolism>();
        met.anima = anima; met.constitution = con;
        MagicReserves res = go.AddComponent<MagicReserves>();
        res.anima = anima;
        QuarkReserve q = go.AddComponent<QuarkReserve>();
        Grimoire grim = go.AddComponent<Grimoire>();
        grim.anima = anima; grim.reserves = res;
        FireSpell fire = go.AddComponent<FireSpell>();
        fire.caster = anima;
        ElementalSpell elem = go.AddComponent<ElementalSpell>();   // agua/tierra/viento (perfiles de coste distintos)
        elem.caster = anima;
        SupplySpell supply = go.AddComponent<SupplySpell>();        // abastecer/trasplante a otro personaje (healer)
        supply.caster = anima;

        // Objetivo del abastecimiento: otro ser con reservas vacías (aliado/otro jugador a "curar").
        GameObject tgtGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        tgtGo.name = "Magia_AUTO_Objetivo";
        tgtGo.transform.SetParent(parent);
        tgtGo.transform.position = new Vector3(12f, 1f, 6f);
        tgtGo.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Magia_AUTO_Objetivo_MAT", new Color(0.30f, 0.55f, 0.45f));
        Anima tgtAnima = tgtGo.AddComponent<Anima>();
        MagicReserves tgtRes = tgtGo.AddComponent<MagicReserves>();
        tgtRes.anima = tgtAnima; tgtRes.unlocked = true;   // ya "despertado" para poder recibir
        tgtGo.AddComponent<QuarkReserve>();

        MagicSandboxDriver drv = go.AddComponent<MagicSandboxDriver>();
        drv.reserves = res; drv.grimoire = grim; drv.quarks = q; drv.fire = fire; drv.elemental = elem;
        drv.metabolism = met; drv.anima = anima; drv.supply = supply; drv.supplyTarget = tgtAnima; drv.targetReserves = tgtRes;

        Debug.Log("[SampleSceneBuilder] Magia_AUTO: HUD de prueba (abajo-izq en Play) para el bucle con Anima real — " +
                  "aprender 1er hechizo → comer sube stats (Constitution)+exceso→grasa y rellena pools → lanzar fuego (testing §19).");
    }

    // ── Hechizos: forcejeo / channeling (fuego múltiple/carga + caminar/esprintar) ──
    // Cápsula con FireSpell (G / +LShift carga / +RShift canaliza) + WalkSpell (ESDF) sobre un Anima con
    // reservas y ATP. Demuestra el powerBonus unificado de SpellBase (charge + channeling + forcejeo, con decaimiento).
    static void BuildSpellDemoSandbox(Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "SpellDemo_AUTO";
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(14f, 1f, 6f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial("SpellDemo_AUTO_MAT", new Color(0.75f, 0.45f, 0.25f));

        Anima anima = go.AddComponent<Anima>();
        go.AddComponent<CharacterLevel>();                    // barra de ATP (aptitudes Default → energía ~100)
        MagicReserves res = go.AddComponent<MagicReserves>();
        res.anima = anima;
        res.unlocked = true;                                 // pools abiertas y con materia/energía para lanzar
        res.Store("C", 100f); res.Store("H", 100f); res.Store("O", 100f);
        res.energy = 1_000_000f;
        go.AddComponent<QuarkReserve>();

        FireSpell fire = go.AddComponent<FireSpell>();
        fire.caster = anima;
        fire.spellKey = KeyCode.G;                            // G = disparo múltiple; +LShift = cargar; +RShift = canalizar
        fire.SetTier(FireTier.Flamethrower);

        WalkSpell walk = go.AddComponent<WalkSpell>();        // ESDF = andar; LShift = postura de salida; RShift = punta

        SpellDemoHUD hud = go.AddComponent<SpellDemoHUD>();
        hud.fire = fire; hud.walk = walk; hud.anima = anima;

        Debug.Log("[SampleSceneBuilder] SpellDemo_AUTO: en Play, HUD arriba-centro. G = fuego múltiple (el forcejeo " +
                  "agranda la llama); G+LShift = cargar la esfera gigante (sale al soltar); G+RShift = canalizar (mantiene " +
                  "el tamaño). ESDF = andar; +LShift = postura de salida (arranque rápido); +RShift = punta. El powerBonus " +
                  "decae al soltar. Ver crecer el bonus y bajar la energía (testing §19g).");
    }

    // Locomoción UNIVERSAL: un ser conducido por su brain (IA) que camina hacia un destino con el MISMO WalkSpell
    // que usaría el jugador (PlayerBrain). Demuestra que andar/correr es un hechizo que cualquier brain conduce.
    static void BuildWalkUniversalSandbox(Transform parent)
    {
        GameObject group = new GameObject("WalkUniversal_AUTO");
        group.transform.SetParent(parent);

        // El destino (un cubo hacia el que caminar).
        GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
        target.name = "Destino";
        target.transform.SetParent(group.transform);
        target.transform.position = new Vector3(22f, 0.5f, 6f);
        target.GetComponent<Renderer>().sharedMaterial = MakeMaterial("WalkUniversal_Target_MAT", new Color(0.4f, 0.8f, 0.4f));

        // El caminante IA.
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "CaminanteIA";
        go.transform.SetParent(group.transform);
        go.transform.position = new Vector3(16f, 1f, 6f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial("WalkUniversal_Walker_MAT", new Color(0.35f, 0.55f, 0.85f));

        go.AddComponent<Anima>();
        go.AddComponent<CharacterLevel>();                       // ATP para el coste de andar

        WalkSpell walk = go.AddComponent<WalkSpell>();
        walk.selfDriven = false;                                 // lo conduce el brain, no su propio input
        walk.baseSpeed = 2.5f;

        go.AddComponent<AnimaController>();                      // cede el mando al brain de mayor relevancia
        AiBrain ai = go.AddComponent<AiBrain>();                 // su IA conduce el WalkSpell hacia el destino
        ai.moveTarget = target.transform;

        Debug.Log("[SampleSceneBuilder] WalkUniversal_AUTO: en Play, «CaminanteIA» camina al «Destino» usando el " +
                  "MISMO WalkSpell que el jugador (aquí lo conduce AiBrain; un PlayerBrain lo movería con ESDF). " +
                  "Locomoción = hechizo universal conducido por brains.");
    }

    // ── Alma por MEZCLA (fase 1): seres compuestos por arquetipos de cuerpo/mente + bonusPacks ──
    static void BuildSoulBlendSandbox(Transform parent)
    {
        GameObject group = new GameObject("AlmaBlend_AUTO");
        group.transform.SetParent(parent);

        // Panterilia: mente Human 90 + Lion 5 + resto (shareDomain) = 90% persona con rastro felino.
        MakeBlendBeing(group.transform, "Panterilia_Blend", new Vector3(16f, 1f, 6f), new Color(0.70f, 0.50f, 0.80f),
            new[] { ("Human", 90f, false), ("Bear", 0f, true), ("Bunny", 0f, true) },
            new[] { ("Human", 90f, false), ("Lion", 5f, false), ("Fire", 0f, true), ("Agua", 0f, true) },
            new string[0]);

        // Oso con MENTE humana: cuerpo de oso, decide como humano (→ podría aprender yoga y crecer único).
        // Con Mind → su tono/pensar emergen del blend mental (humano → Viento/Fuego), no del cuerpo de oso.
        MakeBlendBeing(group.transform, "OsoMenteHumana", new Vector3(18.5f, 1f, 6f), new Color(0.50f, 0.40f, 0.30f),
            new[] { ("Bear", 100f, false) },
            new[] { ("Human", 90f, false), ("Bear", 10f, false) },
            new string[0], true);

        // Oso + bonusPack3: MISMA personalidad (solo BearMind → tono Tierra) pero stats altísimos.
        MakeBlendBeing(group.transform, "Oso_bonusPack3", new Vector3(21f, 1f, 6f), new Color(0.35f, 0.30f, 0.28f),
            new[] { ("Bear", 100f, false) },
            new[] { ("Bear", 100f, false) },
            new[] { "bonusPack3" }, true);

        // Compañero por COMPOSICIÓN (fase 5): reproduce a Panterilia SIN heredar CompanionBase.
        // = Anima + SoulComposition (arquetipo Panterilia) + Mind + BondPillar (IBondable por componente).
        GameObject compa = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        compa.name = "Panterilia_SinClase"; compa.transform.SetParent(group.transform); compa.transform.position = new Vector3(23.5f, 1f, 8f);
        compa.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Panterilia_SinClase_MAT", new Color(0.70f, 0.50f, 0.80f));
        compa.AddComponent<Anima>();
        compa.AddComponent<Mind>();
        SoulComposition compaSoul = compa.AddComponent<SoulComposition>();
        compaSoul.bodies.Add(new BlendSlot { archetype = "Panterilia", domain = 100f });
        compaSoul.minds.Add(new BlendSlot { archetype = "Panterilia", domain = 100f });
        compaSoul.applyScale = false;   // no reescalar (es un humano)
        compaSoul.speciesBonds.Add(new BlendSlot { archetype = "Human", domain = 100f });   // karma humana (agrado a perros, etc.)
        compa.AddComponent<BondPillar>();   // familiariza con cualquier ITarget cercano (incl. el jugador) por circunstancia + karma

        // Los otros 3 compañeros por composición (arquetipos capturados de sus Base* reales) — prueban la disolución de CompanionBase.
        MakeComposedCompanion(group.transform, "Goluis_SinClase", "Goluis", new Vector3(23.5f, 1f, 9.5f), new Color(0.50f, 0.55f, 0.40f));
        MakeComposedCompanion(group.transform, "Gohageneis_SinClase", "Gohageneis", new Vector3(23.5f, 1f, 11f), new Color(0.80f, 0.60f, 0.30f));
        MakeComposedCompanion(group.transform, "Irosene_SinClase", "Irosene", new Vector3(23.5f, 1f, 12.5f), new Color(0.85f, 0.50f, 0.55f));

        // Ser con forma distinta (Toro cuerpo + Bear mente) para probar la CONVERSIÓN (transformación/reencarnación).
        GameObject amb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        amb.name = "Ambrosio_Convert"; amb.transform.SetParent(group.transform); amb.transform.position = new Vector3(23.5f, 1f, 6f);
        amb.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Ambrosio_Convert_MAT", new Color(0.60f, 0.80f, 0.45f));
        amb.AddComponent<Anima>();
        SoulComposition ambSoul = amb.AddComponent<SoulComposition>();
        ambSoul.bodies.Add(new BlendSlot { archetype = "Toro", domain = 100f });
        ambSoul.minds.Add(new BlendSlot { archetype = "Bear", domain = 100f });
        SoulConvertDemo demo = amb.AddComponent<SoulConvertDemo>();
        demo.soul = ambSoul;

        Debug.Log("[SampleSceneBuilder] AlmaBlend_AUTO: 3 seres por MEZCLA + Ambrosio_Convert (Toro+Bear). En Play " +
                  "cada uno resuelve sus 12 aptitudes+tamaño por blend (logs [Alma]); Ambrosio_Convert tiene HUD para " +
                  "CONVERTIR a hormiga+humano en modo A/relativa o B/literal (reencarnación) (docs soul-relations-reincarnation §1).");
    }

    // ── Alma COMPARTIDA: dos cuerpos = una anima (reencarnaciones conectadas) ──
    static void BuildSharedSoulSandbox(Transform parent)
    {
        GameObject group = new GameObject("AlmaCompartida_AUTO");
        group.transform.SetParent(parent);
        SharedSoul soul = group.AddComponent<SharedSoul>();
        soul.soulName = "Ambrosio";

        SoulComposition melaza = MakeSharedBody(group.transform, "AmbrosioMelaza", new Vector3(16f, 1f, 9f),
            new Color(0.60f, 0.80f, 0.45f), "Toro", "Bear", soul);
        SoulComposition hormiga = MakeSharedBody(group.transform, "AmbrosioHormiga", new Vector3(18f, 1f, 9f),
            new Color(0.40f, 0.50f, 0.30f), "Ant", "Human", soul);

        SharedSoulDemo demo = group.AddComponent<SharedSoulDemo>();
        demo.soul = soul; demo.bodyA = melaza; demo.bodyB = hormiga;

        Debug.Log("[SampleSceneBuilder] AlmaCompartida_AUTO: 2 cuerpos (melaza Toro+Bear, hormiga Ant+Human) comparten " +
                  "UNA alma. HUD: entrenar/lesionar el poder se propaga a ambos; +bond lo reciben TODAS las " +
                  "reencarnaciones (docs soul-relations-reincarnation §4).");
    }

    // Núcleo de un compañero por composición: Anima + Mind + SoulComposition(arquetipo) + MoodState.
    static MoodState MakeCompanionCore(GameObject go, string archetype)
    {
        go.AddComponent<Anima>();
        go.AddComponent<Mind>();
        SoulComposition sc = go.AddComponent<SoulComposition>();
        sc.bodies.Add(new BlendSlot { archetype = archetype, domain = 100f });
        sc.minds.Add(new BlendSlot { archetype = archetype, domain = 100f });
        sc.applyScale = false;
        sc.speciesBonds.Add(new BlendSlot { archetype = "Human", domain = 100f });
        go.AddComponent<SocialField>();   // (c) su "actitud" (ánimo/fiesta) contagia a vecinos con bond — emergente
        go.AddComponent<MoodDynamics>();  // el estrés (cortisol) sube por fatiga/sueño/hambre → mal humor situacional
        return go.AddComponent<MoodState>();
    }

    // Compañero por COMPOSICIÓN (fase 5): reproduce a una companion desde su arquetipo, sin heredar CompanionBase.
    static void MakeComposedCompanion(Transform parent, string name, string archetype, Vector3 pos, Color col)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name; go.transform.SetParent(parent); go.transform.position = pos;
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_MAT", col);
        go.AddComponent<Anima>();
        go.AddComponent<Mind>();
        SoulComposition sc = go.AddComponent<SoulComposition>();
        sc.bodies.Add(new BlendSlot { archetype = archetype, domain = 100f });
        sc.minds.Add(new BlendSlot { archetype = archetype, domain = 100f });
        sc.applyScale = false;
        sc.speciesBonds.Add(new BlendSlot { archetype = "Human", domain = 100f });
        go.AddComponent<BondPillar>();
    }

    static SoulComposition MakeSharedBody(Transform parent, string name, Vector3 pos, Color col, string body, string mind, SharedSoul soul)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name; go.transform.SetParent(parent); go.transform.position = pos;
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_MAT", col);
        go.AddComponent<Anima>();
        SoulComposition sc = go.AddComponent<SoulComposition>();
        sc.bodies.Add(new BlendSlot { archetype = body, domain = 100f });
        sc.minds.Add(new BlendSlot { archetype = mind, domain = 100f });
        sc.sharedSoul = soul;
        return sc;
    }

    static void MakeBlendBeing(Transform parent, string name, Vector3 pos, Color col,
        (string a, float d, bool sh)[] bodies, (string a, float d, bool sh)[] minds, string[] packs, bool withMind = false)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name; go.transform.SetParent(parent); go.transform.position = pos;
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_MAT", col);
        go.AddComponent<Anima>();
        if (withMind) go.AddComponent<Mind>();   // Fase 2: el tono/pensar emergen de las aptitudes del blend
        SoulComposition sc = go.AddComponent<SoulComposition>();
        foreach ((string a, float d, bool sh) in bodies) sc.bodies.Add(new BlendSlot { archetype = a, domain = d, shareDomain = sh });
        foreach ((string a, float d, bool sh) in minds)  sc.minds.Add(new BlendSlot { archetype = a, domain = d, shareDomain = sh });
        foreach (string p in packs) sc.bonusPacks.Add(p);
    }

    static BodyPartReactor MakeReactor(Transform parent, string name, Vector3 pos, Vector3 axis, float arousalGain, float valenceGain, float joltGain)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.SetParent(parent);
        go.transform.localPosition = pos; go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_MAT", new Color(0.7f, 0.5f, 0.6f));
        BodyPartReactor r = go.AddComponent<BodyPartReactor>();
        r.axis = axis; r.arousalGain = arousalGain; r.valenceGain = valenceGain; r.joltGain = joltGain;
        return r;
    }

    // Crea un «insecto» (cápsula con material) para el sandbox del Microcosmos.
    static GameObject MakeInsect(Transform parent, string name, Vector3 pos, Vector3 scale, Color col)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name; go.transform.SetParent(parent);
        go.transform.position = pos; go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_MAT", col);
        return go;
    }

    // Adjunta la ficha de alma (docs/microcosmos-insects.md §13) a un ser del Microcosmos.
    static void AddSoul(GameObject go, string soulName, string hilo, string vida1, string vida2, string tell)
    {
        if (go == null) return;
        SoulRecord s = go.AddComponent<SoulRecord>();
        s.soulName = soulName; s.hilo = hilo; s.vida1Role = vida1; s.vida2Name = vida2; s.tell = tell;
    }

    static void BuildCriaBeginner(Transform parent)
    {
        GameObject group = new GameObject("CriaBeginner_AUTO");
        group.transform.SetParent(parent);

        // 1) LIMPIAR el nido.
        GameObject dirtGO = new GameObject("Nido_Suciedad");
        dirtGO.transform.SetParent(group.transform);
        dirtGO.transform.position = new Vector3(-20f, 0f, 10f);
        DirtArea dirt = dirtGO.AddComponent<DirtArea>();
        dirt.areaSize = new Vector2(6f, 6f); dirt.spawnInterval = 2f; dirt.maxSpots = 6; dirt.missionThreshold = 4;
        GameObject sweeper = MakeKitchenPerson(group.transform, "Cuidador_Limpia", new Vector3(-20f, 1f, 10f), new Color(0.55f, 0.60f, 0.55f));
        Cleaner sw = sweeper.AddComponent<Cleaner>(); sw.auto = true; sw.autoInterval = 2.5f; sw.reach = 10f;

        // 2) ABASTECER (cajas → almacén de la cría).
        MakeStationPart(group.transform, "Caja", "Biberon", "coges biberones",  new Vector3(-24f,   1f, 20f), new Color(0.85f, 0.85f, 0.75f));
        MakeStationPart(group.transform, "Caja", "Comida",  "coges comida",     new Vector3(-22.8f, 1f, 20f), new Color(0.60f, 0.45f, 0.25f));
        MakeStationPart(group.transform, "Caja", "Paja",    "coges paja/lecho", new Vector3(-21.6f, 1f, 20f), new Color(0.80f, 0.75f, 0.40f));
        MakeStationPart(group.transform, "Almacen", "Biberon", "estante: biberones", new Vector3(-24f,   1f, 12f), new Color(0.85f, 0.85f, 0.75f));
        MakeStationPart(group.transform, "Almacen", "Comida",  "estante: comida",    new Vector3(-22.8f, 1f, 12f), new Color(0.60f, 0.45f, 0.25f));
        MakeStationPart(group.transform, "Almacen", "Paja",    "estante: lecho",     new Vector3(-21.6f, 1f, 12f), new Color(0.80f, 0.75f, 0.40f));
        GameObject stockGO = new GameObject("Tarea_Abastecer_Cria");
        stockGO.transform.SetParent(group.transform);
        StockingTask stock = stockGO.AddComponent<StockingTask>();
        stock.pickStation = "Caja"; stock.slotStation = "Almacen"; stock.total = 3; stock.areaLabel = "la cría";

        // 3) RUTINA DE CUIDADO (el bond se gana: leer estado → calmar → alimentar → asear → arrullar).
        GameObject binGO = new GameObject("Bond_Ganado");
        binGO.transform.SetParent(group.transform);
        binGO.transform.position = new Vector3(-16f, 1f, 15f);
        FoodContainer bin = binGO.AddComponent<FoodContainer>(); bin.dishName = "sesión de cuidado"; bin.capacity = 10;
        MakeStationPart(group.transform, "Cria", "LeerEstado", "lees su estado (hambre/estrés/bond)", new Vector3(-18f, 1f, 14f), new Color(0.70f, 0.75f, 0.85f));
        MakeStationPart(group.transform, "Cria", "Calmar",     "te sientas cerca (presencia tranquila)", new Vector3(-18f, 1f, 15f), new Color(0.65f, 0.80f, 0.70f));
        MakeStationPart(group.transform, "Cria", "Alimentar",  "das el biberón",                    new Vector3(-18f, 1f, 16f), new Color(0.90f, 0.85f, 0.70f));
        MakeStationPart(group.transform, "Cria", "Asear",      "la aseas (grooming)",               new Vector3(-18f, 1f, 17f), new Color(0.75f, 0.75f, 0.60f));
        MakeStationPart(group.transform, "Cria", "Arrullar",   "respondes a su llanto (vocal)",     new Vector3(-18f, 1f, 18f), new Color(0.80f, 0.70f, 0.80f));
        GameObject orderGO = new GameObject("Receta_CuidadoCria");
        orderGO.transform.SetParent(group.transform);
        ProductionOrder order = orderGO.AddComponent<ProductionOrder>();
        order.productName = "bond de la cría"; order.output = bin; order.quota = 3;
        order.stepStation = new[] { "Cria", "Cria", "Cria", "Cria", "Cria" };
        order.stepAction  = new[] { "LeerEstado", "Calmar", "Alimentar", "Asear", "Arrullar" };
        order.stepLabel   = new[] { "lees su estado", "presencia tranquila", "das el biberón", "grooming", "respondes a su llanto" };

        // 4) Llevar la cría al NIDO cálido (reusa CarryToRefuge/WeakOne).
        GameObject nest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nest.name = "Nido_Calido"; nest.transform.SetParent(group.transform);
        nest.transform.position = new Vector3(-14f, 1f, 20f); nest.transform.localScale = new Vector3(2f, 1f, 2f);
        nest.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Nido_MAT", new Color(0.70f, 0.60f, 0.40f));
        CarryToRefuge nestTask = nest.AddComponent<CarryToRefuge>(); nestTask.needed = 1; nestTask.radius = 2.5f;
        GameObject cub = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        cub.name = "Cria_Bebe"; cub.transform.SetParent(group.transform);
        cub.transform.position = new Vector3(-17f, 1f, 22f); cub.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        cub.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Cria_Bebe_MAT", new Color(0.85f, 0.80f, 0.65f));
        cub.AddComponent<WeakOne>();
        cub.AddComponent<CriaCareTarget>();   // engancha la rutina a drives reales (aquí placeholder: sin Animal → solo registra)
        cub.AddComponent<AiBrain>().selfRelevance = 1f;
        FollowBrain cfb = cub.AddComponent<FollowBrain>(); cfb.target = nest.transform; cfb.relevance = 2f; cfb.stopDistance = 1.5f;
        cub.AddComponent<AnimaController>();

        Debug.Log("[SampleSceneBuilder] Cría beginner (corazón del santuario, docs cria-simulation): limpiar el " +
                  "nido → abastecer (biberón/comida/paja) → RUTINA DE CUIDADO (leer estado→calmar→alimentar→asear→" +
                  "arrullar; el bond se gana) → llevar la «Cria_Bebe» al «Nido_Calido». Logs [Abastecer]/[Producción]/[Cuidado].");
    }

    static void BuildPrologueSandbox(Transform parent)
    {
        GameObject group = new GameObject("PrologueSandbox_AUTO");
        group.transform.SetParent(parent);

        group.AddComponent<PlaneMessenger>();

        GameObject seqGO = new GameObject("Prologue");
        seqGO.transform.SetParent(group.transform);
        PrologueSequence seq = seqGO.AddComponent<PrologueSequence>();
        seq.autoDemo = true; seq.autoInterval = 3f;

        GameObject cave = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cave.name = "Cueva_Refugio"; cave.transform.SetParent(group.transform);
        cave.transform.position = new Vector3(-8f, 1f, 22f);
        cave.transform.localScale = new Vector3(3f, 2f, 3f);
        cave.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Cueva_MAT", new Color(0.35f, 0.32f, 0.30f));
        CarryToRefuge carry = cave.AddComponent<CarryToRefuge>();
        carry.needed = 2; carry.radius = 3f;
        carry.onComplete.AddListener(() => PlaneMessenger.Send("Mesocosmos",
            "Kushal, cuando termines, ve a la sala de meditación para volver.", 8f));

        for (int i = 0; i < 2; i++)
        {
            GameObject w = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            w.name = $"Debil_{i}"; w.transform.SetParent(group.transform);
            w.transform.position = new Vector3(-12f + i, 1f, 26f);
            w.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"Debil_{i}_MAT", new Color(0.60f, 0.60f, 0.50f));
            w.AddComponent<WeakOne>();
            w.AddComponent<AiBrain>().selfRelevance = 1f;
            FollowBrain fb = w.AddComponent<FollowBrain>();
            fb.target = cave.transform; fb.relevance = 2f; fb.stopDistance = 2f;
            w.AddComponent<AnimaController>();
        }

        Debug.Log("[SampleSceneBuilder] Prologue sandbox: el guion se auto-reproduce (logs [Prólogo]); dos " +
                  "«Debil» caminan a la «Cueva_Refugio» (CarryToRefuge → logs [Cuidado]); al reunirlos, el " +
                  "Mesocosmos avisa (PlaneMessenger) de volver por la sala de meditación. docs area-progression Apertura.");
    }

    static StationPart MakeStationPart(Transform parent, string station, string action, string label, Vector3 pos, Color col)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"{station}_{action}";
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"{station}_{action}_MAT", col);
        StationPart sp = go.AddComponent<StationPart>();
        sp.stationId = station; sp.actionId = action; sp.label = label;
        return sp;
    }

    static TourStation MakeStation(Transform parent, string label, Vector3 pos, string canDo)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Station_" + label;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Station_" + label + "_MAT", new Color(0.55f, 0.55f, 0.60f));
        TourStation st = go.AddComponent<TourStation>();
        st.stationName = label; st.canDoHere = canDo;
        return st;
    }

    static GameObject MakeKitchenPerson(Transform parent, string name, Vector3 pos, Color col)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(name + "_MAT", col);
        return go;
    }

    static void AddMindBeing(Transform parent, string name, Vector3 pos, Color col,
                             System.Func<Aptitudes, Aptitudes> config)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"{name}_MAT", col);

        Mind mind = go.AddComponent<Mind>();
        mind.aptitudes = config(Aptitudes.Default);
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

        // PUNTO DE APARICIÓN despejado: el Player de la escena empezaba en (22,-6), donde se solapan el suelo del restaurante,
        // la esterilla del Huerto y su decorado de virtualización; el CharacterController quedaba dentro de un collider y
        // acababa cayendo bajo el suelo (visto en vivo: y = −2,7 y luego caída sin fin).
        player.transform.position = new Vector3(30f, 1.2f, -13f);

        // CONTROL POR NECESIDAD SIEMPRE ACTIVO (todo es Anima; el jugador NUNCA tiene control total):
        // el "body" (AcuteStressResponse) puja por el mando como una fuerza más. No es un switch.
        if (player.GetComponent<Anima>() != null && player.GetComponent<AcuteStressResponse>() == null)
            player.AddComponent<AcuteStressResponse>();
        // Lanzador de hechizos por TECLADO + cola de acciones (jugador programa al poseído tipo Sims).
        if (player.GetComponent<ActionQueue>() == null) player.AddComponent<ActionQueue>();
        if (player.GetComponent<GroundGuard>() == null) player.AddComponent<GroundGuard>();   // si queda bajo tierra, lo recoloca
        // La energía (ATP) se recupera sola de las reservas: sin esto, al vaciarse (andar, nadar, la clase de yoga) el
        // jugador se quedaba CLAVADO para siempre (WalkSpell no avanza sin ATP).
        if (player.GetComponent<ATPRegenSpell>() == null) player.AddComponent<ATPRegenSpell>();
        if (player.GetComponent<FallImpact>() == null) player.AddComponent<FallImpact>();   // las caídas hacen daño (salvo sobre el musgo o al agua)
        if (player.TryGetComponent(out CharacterController pcc)) pcc.stepOffset = 0.45f;   // salir nadando a un casquete (borde a ~0,3 m del agua)
        if (Object.FindObjectOfType<SpellConsole>() == null)
            new GameObject("SpellConsole_AUTO").AddComponent<SpellConsole>().transform.SetParent(player.transform);

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

        Transform thirdAnchor = player.transform.Find("ThirdPersonAnchor") ?? cameraPivot.Find("ThirdPersonAnchor");
        if (thirdAnchor != null && thirdAnchor.parent != cameraPivot)
            thirdAnchor.SetParent(cameraPivot, worldPositionStays: true);
        // Tercera persona mirando un poco hacia abajo: con la cámara horizontal el personaje quedaba cortado en el borde inferior.
        if (thirdAnchor != null) { thirdAnchor.localPosition = new Vector3(0f, 1.1f, -4.2f); thirdAnchor.localRotation = Quaternion.Euler(14f, 0f, 0f); }

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

        // El jugador ES un Anima: su ESTRÉS emerge del estado (fatiga/sueño/trabajo) igual que en todos, vía
        // MoodDynamics. Sin Mind → deriva el estrés directo de las drives universales (docs soul-relations §2b).
        Anima playerAnima = player.GetComponent<Anima>();
        if (playerAnima != null && player.GetComponent<MoodDynamics>() == null)
            player.AddComponent<MoodDynamics>().anima = playerAnima;

        // Locomoción UNIVERSAL: el jugador anda/corre con el mismo WalkSpell (carga-postura + punta), conducido por
        // PlayerController (mantiene cámara/gravedad/salto). base = caminar; carga/channel suben hasta ~esprintar.
        WalkSpell playerWalk = player.GetComponent<WalkSpell>();
        if (playerWalk == null) playerWalk = player.AddComponent<WalkSpell>();
        playerWalk.selfDriven = false;                                       // lo conduce el PlayerController
        playerWalk.baseSpeed = controller.walkSpeed;
        playerWalk.maxPowerWithCharge     = Mathf.Max(0f, controller.sprintSpeed - controller.walkSpeed);
        playerWalk.maxPowerWithChanneling = Mathf.Max(0f, controller.sprintSpeed - controller.walkSpeed);

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

    // Material de AGUA transparente (URP Lit en modo Transparent, mezcla alfa): deja ver peces, focas y el borde
    // sumergido del hielo. Sin escribir profundidad, para que lo de debajo se vea a través.
    static Material MakeWaterMaterial(string name, Color color)
    {
        Material m = MakeMaterial(name, color);
        if (m.HasProperty("_Surface"))
        {
            m.SetFloat("_Surface", 1f);   // Transparent
            m.SetFloat("_Blend", 0f);     // Alpha
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_Smoothness", 0.9f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {   // Standard (fallback)
            m.SetFloat("_Mode", 3f);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_ALPHABLEND_ON");
            m.renderQueue = 3000;
        }
        return m;
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
    // CUERPOS DE CUBOS: todo anima humano de la escena (no Animal, no criaturas de farming) cambia su cápsula o su .fbx por
    // un cuerpo de cubos articulado (CubeHumanoid), con el color de su material actual como ropa. Así el yoga mueve
    // hombro/codo/rodilla de verdad: los .fbx no tienen un esqueleto adecuado y las cápsulas no tienen partes.
    static void DressHumanoids()
    {
        Color skin = new Color(0.86f, 0.70f, 0.58f);
        int n = 0, alive = 0;
        foreach (Anima a in Object.FindObjectsOfType<Anima>())
        {
            if (a is Animal || a.GetComponent<PlayableCreature>() != null) continue;
            // Ya vestido o no, se (re)viste: así cada cambio de aspecto (docs/diseno-personajes.md) llega a la escena.
            Renderer r = a.GetComponentInChildren<Renderer>();
            if (r == null) continue;
            Color clothes = r.sharedMaterial != null ? r.sharedMaterial.color : new Color(0.5f, 0.5f, 0.6f);
            CubeHumanoid.Dress(a.gameObject, clothes, skin);
            // CUERPO con metabolismo (hambre/sueño) para todo humano; la Magnate, por sus células de mago, no los siente.
            if (a.GetComponent<BodyMetabolism>() == null) a.gameObject.AddComponent<BodyMetabolism>();
            if (a.GetComponent<BodyAging>() == null) a.gameObject.AddComponent<BodyAging>();   // edad = efecto del cuerpo
            // ATENCIÓN VISUAL (docs/atencion-visual.md): cada persona con su perfil de mirada.
            if (a.GetComponent<Mind>() != null || a.CompareTag("Player"))
            {
                VisualAttention va = a.GetComponent<VisualAttention>() ?? a.gameObject.AddComponent<VisualAttention>();
                va.fieldOfView = 200f; va.range = 30f;   // ojos humanos: ~200° de campo, ~120° binocular al frente
                string nm = a.name;
                va.ApplyProfile(nm.StartsWith("Gohageneis") ? VisualAttention.Profile.Vigilante : nm.StartsWith("Irosene") || nm.StartsWith("Goluis") ? VisualAttention.Profile.Esteta
                    : nm.StartsWith("Sadhguru") || nm.StartsWith("Magnate") ? VisualAttention.Profile.Contemplativo : nm.StartsWith("Lizmibet") || nm.StartsWith("Andrea") ? VisualAttention.Profile.Afectivo : VisualAttention.Profile.Equilibrado);
            }
            if (a.GetComponent<Magnate>() != null && a.GetComponent<MageCells>() == null) a.gameObject.AddComponent<MageCells>().fraction = 1f;
            n++;
            // Los compañeros (con mente, no el jugador ni la Magnate) estaban quietos: les damos cuerpo que ANDA (WalkSpell),
            // cola de acciones y vida libre (IdleBehavior → merodear). CharacterLevel para que la Magnate pueda enseñarles.
            if (a.GetComponent<Mind>() != null && !a.CompareTag("Player") && a.GetComponent<Magnate>() == null)
            {
                if (a.GetComponent<WalkSpell>() == null) { WalkSpell w = a.gameObject.AddComponent<WalkSpell>(); w.selfDriven = false; }
                if (a.GetComponent<ActionQueue>() == null) a.gameObject.AddComponent<ActionQueue>();
                if (a.GetComponent<IdleBehavior>() == null) a.gameObject.AddComponent<IdleBehavior>();
                if (a.GetComponent<CharacterLevel>() == null) a.gameObject.AddComponent<CharacterLevel>();
                if (a.GetComponent<GroundGuard>() == null) a.gameObject.AddComponent<GroundGuard>();
                if (a.GetComponent<NeedsDriver>() == null) a.gameObject.AddComponent<NeedsDriver>();   // el cuerpo pide comer/dormir
                if (a.GetComponent<ATPRegenSpell>() == null) a.gameObject.AddComponent<ATPRegenSpell>();   // recupera energía para seguir andando
                alive++;
            }
        }
        Debug.Log($"[SampleSceneBuilder] Cuerpos de cubos: {n} anima humanos vestidos (CubeHumanoid); {alive} compañeros con vida libre (merodean).");
    }

    // BANQUISA: el suelo único (un plano de nieve) se convierte en CASQUETES de hielo separados por canales de agua con
    // puentes, y mar abierto con témpanos al este. Va al final (antes del NavMesh) para proteger con hielo firme todo lo
    // que ya está colocado: áreas, hub, seres, estaciones, nidos. Y siembra el SOTOBOSQUE polar (se genera en Play).
    static void BuildPolarIce(Transform root)
    {
        GameObject ground = GameObject.Find("Ground_Placeholder");
        if (ground == null) return;
        // ARCHIPIÉLAGO: el hielo son las islas del diseño (PolarLandscapeBuilder.Islands); todo lo demás es agua. Ya no
        // se fuerza hielo bajo cada objeto (eso unía las islas): se AVISA de lo que haya quedado en el agua para moverlo.
        PolarLandscapeBuilder.Protect.Clear();
        var wet = new List<string>();
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
        {
            Transform t = r.transform;
            if (t.IsChildOf(ground.transform) || t.name.StartsWith("Sea") || t.name.StartsWith("Swarm") || t.name.StartsWith("Phyto")) continue;
            if (t.GetComponentInParent<IceCarnivorousTree>() != null || t.GetComponentInParent<WorldLabel>() is WorldLabel w0 && w0.name.StartsWith("Casa")) continue;   // las ramas vuelan sobre el agua
            Bounds b = r.bounds;
            if (b.center.y < -0.5f || b.extents.x > 40f || b.extents.z > 40f) continue;
            if (!PolarLandscapeBuilder.NaturalIce(b.center.x, b.center.z))
            {
                Transform top = t; while (top.parent != null && top.parent != root) top = top.parent;
                string id = $"{t.name}@({b.center.x:0},{b.center.z:0})"; if (wet.Count < 40 && !wet.Contains(id)) wet.Add(id);
            }
        }
        string[] airy = { "Rama", "Escarcha", "Nido_Abejas", "Hoja", "Panal", "Celdilla", "Cera_de_union", "Borde_panal", "Piquera" };
        wet.RemoveAll(w => System.Array.Exists(airy, a => w.StartsWith(a)));   // ramas que vuelan sobre el agua: normal
        if (wet.Count > 0) Debug.LogWarning($"[Islas] {wet.Count} grupos con piezas sobre el agua: {string.Join(", ", wet)}");

        // El plano deja de ser suelo: queda como portador del NavMeshSurface y padre de los casquetes.
        Object.DestroyImmediate(ground.GetComponent<MeshCollider>());
        Object.DestroyImmediate(ground.GetComponent<MeshRenderer>());
        Object.DestroyImmediate(ground.GetComponent<MeshFilter>());
        ground.transform.localScale = Vector3.one;
        int boxes = PolarLandscapeBuilder.BuildFloes(ground.transform, MakeMaterial("Ice_Floe_MAT", new Color(0.86f, 0.91f, 0.95f)));
        int links = PolarLandscapeBuilder.BuildSwimLinks(ground.transform);
        Debug.Log($"[Islas] {PolarLandscapeBuilder.CapCount} casquetes ({PolarLandscapeBuilder.Islands.Length} lóbulos, {boxes} placas de hielo) alrededor de la laguna central; {links} enlaces de nado para la IA.");

        GameObject under = new GameObject("SotobosquePolar_AUTO");
        under.transform.SetParent(root, false);
        under.AddComponent<PolarUnderstory>();
    }

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
        sea.transform.position = new Vector3(0f, PolarLandscapeBuilder.WaterLevel, 0f); // BANQUISA: el mar está bajo TODO el hielo y asoma por los canales
        sea.transform.localScale = new Vector3(120f, 1f, 120f);                          // 1200 m: más allá del hielo, mar abierto
        PolarLandscapeBuilder.IgnoreForNav(sea);                                          // que nadie «ande» sobre el agua
        sea.GetComponent<Renderer>().sharedMaterial = MakeWaterMaterial("Sea_Placeholder_MAT", new Color(0.30f, 0.55f, 0.70f, 0.42f));   // agua TRANSPARENTE: se ve lo que nada debajo
        Object.DestroyImmediate(sea.GetComponent<Collider>()); // el MeshCollider del Plane es solo una superficie 2D, no sirve como volumen nadable

        // Zona de nado real — antes el mar era puramente decorativo (sin collider de ningún
        // tipo). Un BoxCollider trigger aparte, no atado a la escala del Plane visual (evita
        // líos de escala heredada), cubre el volumen bajo la superficie. WaterZone avisa a
        // PlayerController cuando el jugador entra/sale; MediumZone hace lo mismo para
        // cualquier Anima (fauna) — ambos coexisten en el mismo trigger sin pisarse,
        // cada uno ignora los objetos que no tienen su componente requerido.
        GameObject swimZone = new GameObject("Sea_SwimZone");
        swimZone.transform.SetParent(parent);
        swimZone.transform.position = new Vector3(0f, PolarLandscapeBuilder.WaterLevel - 8f, 0f);
        BoxCollider swimCollider = swimZone.AddComponent<BoxCollider>();
        swimCollider.isTrigger = true;
        swimCollider.size = new Vector3(1200f, 16f, 1200f); // todo el mar: bajo la banquisa, en los canales y alrededor
        swimZone.AddComponent<WaterZone>();
        swimZone.AddComponent<MediumZone>().medium = Medium.Water;

        // Fondo marino: sin él, quien cae a un canal se hundiría para siempre. Invisible y fuera del NavMesh.
        GameObject seabed = new GameObject("FondoMarino");
        seabed.transform.SetParent(parent);
        seabed.transform.position = new Vector3(0f, PolarLandscapeBuilder.WaterLevel - 17f, 0f);
        seabed.AddComponent<BoxCollider>().size = new Vector3(1200f, 2f, 1200f);
        PolarLandscapeBuilder.IgnoreForNav(seabed);

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
        GameObject animalsGroup = new GameObject("Animals_Placed");
        animalsGroup.transform.SetParent(parent);

        // (Las crías sueltas de antes —conejo, zorro, lobo y ciervo junto a un «CubAreaTrigger» que ya no existía— se
        // retiraron: eran fauna templada y no tenían nido. Las crías viven ahora en el área de CRÍA, ver FurnishCria.)

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
        // EQUILIBRIO con los humanos (PopulationPlan): cada especie reparte su objetivo entre sus grupos.
        var groupsOf = new System.Collections.Generic.Dictionary<string, int> { { "Bear", 1 }, { "Seal", 3 }, { "Whale", 2 }, { "Penguin", 2 }, { "Orca", 1 } };
        var used = new System.Collections.Generic.Dictionary<string, int>();
        void AddFamily(string species, Vector3 pos, float radius, NestKind kind, float height = 0f)
        {
            int gi = used.TryGetValue(species, out int u) ? u : 0; used[species] = gi + 1;
            int qty = groupsOf.TryGetValue(species, out int gn) ? PopulationPlan.Split(species, gn)[Mathf.Min(gi, gn - 1)] : 0;
            nests.Add(new NestSpec { species = species, position = pos, radius = radius, kind = kind, height = height });

            GameObject prefab = LoadAnimalPrefab(species);
            if (prefab == null)
            {
                Debug.LogWarning($"[SampleSceneBuilder] WildlifePopulation: no se encontro el prefab de {species}, se omite esa familia.");
                return;
            }
            families.Add(new FamilyGenerator.Family { animalPrefab = prefab, position = pos, radius = radius, renderHeight = height, quantity = qty });   // qty 0 = el tamaño de su familia
        }

        // SANTUARIO 1 = HIELO (docs/ice-sanctuary-ecology.md). El suelo ya es nieve (Ground_Snow_MAT); la
        // inconsistencia era la fauna TEMPLADA. Retirados conejo/ciervo/lobo/zorro — RESERVADOS para un santuario
        // templado (no pertenecen al hielo). La vida TERRESTRE de hielo (árbol carnívoro, abeja/araña de nieves,
        // pingüino) se añade al implementar esas especies. Base viva del hielo = marina: plancton → pez → foca → oso.
        //   Reservados (santuario templado): Bunny×4, Deer×2, Wolf, Fox — ver git para las posiciones previas.

        // Oso Polar — apex del hielo; junto al borde de agua/hielo para cazar focas (homeRadius amplio ~300).
        AddFamily("Bear", new Vector3(132f, 0f, -15f), 12f, NestKind.Predator);   // "Bear" = el prefab/catálogo real (el oso del hielo)

        ValidateNestSeparation(nests);

        // Marinos — sobre el Sea_Placeholder (base marina del hielo).
        AddFamily("Whale", new Vector3(60f, -1f, 25f), 14f, NestKind.Prey, -1f);
        // POBLACIÓN EQUILIBRADA (pirámide trófica): más presas que depredadores, repartidas por la laguna. Peces/krill (bancos)
        // → 3 grupos de focas (6 adultas + 3 crías) y 2 de ballenas (4 + 2) y pingüinos (4 + 4) → 1 familia de osos (2 + 1)
        // y un pod de orcas (2 + 1).
        AddFamily("Seal", new Vector3(48f, -1f, 14f), 8f, NestKind.Prey, -1f);
        AddFamily("Seal", new Vector3(74f, -1f, 36f), 8f, NestKind.Prey, -1f);
        AddFamily("Seal", new Vector3(66f, -1f, 8f), 8f, NestKind.Prey, -1f);
        AddFamily("Whale", new Vector3(50f, -1f, 34f), 14f, NestKind.Prey, -1f);
        // Fauna de hielo NUEVA (requieren prefab en Assets/Animals/{Especie}/{Especie}.prefab; si falta, se omiten con aviso).
        AddFamily("Penguin", new Vector3(60f, 0f, -22f), 6f, NestKind.Prey, 0f);      // colonia en el borde hielo/agua; presa de foca/orca
        AddFamily("Penguin", new Vector3(66f, 0f, -19f), 6f, NestKind.Prey, 0f);
        AddFamily("Orca", new Vector3(72f, -1f, 32f), 12f, NestKind.Predator, -1f);  // apex marino; caza focas/ballenas

        // NOTA (2026-09-01): los 5 insectos (Ant/Aphid/Ladybug/Spider/Cricket) NO van aquí — se crearon para el
        // Microcosmos Nivel 1 (mundo insecto), no para este bioma. Su generación irá en la escena del Microcosmos con
        // su cadena trófica micro. La fauna de ESTE santuario (de HIELO) se está reconciliando: ver
        // docs/ice-sanctuary-ecology.md (árbol carnívoro de hielo, abeja/araña de las nieves, pingüino, plancton…).

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
    // Los marinos (Whale, Seal; height < 0) usan Swarm en vez de esto — ver BuildSwarms().
    static void BuildGrassPatches(Transform parent, System.Collections.Generic.List<NestSpec> nests)
    {
        GameObject group = new GameObject("GrassPatches_AUTO");
        group.transform.SetParent(parent);

        Material grassMat = MakeMaterial("GrassPatch_MAT", new Color(0.60f, 0.66f, 0.55f));   // musgo de tundra (pasto de los herbívoros)
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

    // Bancos/enjambres marinos (Swarm, antes FishSchool): organismos-banco que derivan, huyen y crecen pastando el
    // fitoplancton (ver BuildPhytoplankton). Los depredadores (oso/foca/pez) los comen por MORDISCO-POR-COLISIÓN
    // (Swarm.OnTriggerStay). Repartidos cerca de donde viven las familias marinas (Whale 200,-1,15; Seal 180,-1,-20).
    static void BuildSwarms(Transform parent)
    {
        GameObject group = new GameObject("Swarms_AUTO");
        group.transform.SetParent(parent);

        Vector3[] positions =
        {
            new Vector3(56f, -2f, 22f),
            new Vector3(66f, -2f, 30f),
        };

        Material fishMat = MakeMaterial("Swarm_Fish_MAT", new Color(0.35f, 0.55f, 0.65f, 0.6f));
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject school = GameObject.CreatePrimitive(PrimitiveType.Cube);
            school.name = $"Swarm_Fish_{i}";
            PolarLandscapeBuilder.IgnoreForNav(school);   // los bancos no son suelo: sin esto había NavMesh «sobre» la laguna
            school.transform.SetParent(group.transform);
            school.transform.position = positions[i];
            school.transform.localScale = new Vector3(10f, 2f, 10f);
            school.GetComponent<Renderer>().sharedMaterial = fishMat;
            Object.DestroyImmediate(school.GetComponent<Collider>()); // el trigger lo añade Swarm.Start en runtime
            Swarm swarm = school.AddComponent<Swarm>();
            swarm.kind = SwarmKind.Fish;
            swarm.unitName = "Fish";
        }
    }

    // Krill: enjambre de clase Krill (dieta propia). Pasta el FITOPLANCTON para crecer; comido por pez (proximidad) y por
    // ballena/pingüino (mordisco-por-colisión). Sembrado en las mismas aguas, entre el fitoplancton y los peces.
    static void BuildKrill(Transform parent)
    {
        GameObject group = new GameObject("Krill_AUTO");
        group.transform.SetParent(parent);

        Vector3[] positions =
        {
            new Vector3(62f, -2f, 20f),
            new Vector3(52f, -2f, 28f),
        };

        Material krillMat = MakeMaterial("Swarm_Krill_MAT", new Color(0.75f, 0.45f, 0.4f, 0.6f));
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject school = GameObject.CreatePrimitive(PrimitiveType.Cube);
            school.name = $"Swarm_Krill_{i}";
            PolarLandscapeBuilder.IgnoreForNav(school);   // los bancos no son suelo: sin esto había NavMesh «sobre» la laguna
            school.transform.SetParent(group.transform);
            school.transform.position = positions[i];
            school.transform.localScale = new Vector3(8f, 1.5f, 8f);
            school.GetComponent<Renderer>().sharedMaterial = krillMat;
            Object.DestroyImmediate(school.GetComponent<Collider>()); // el trigger lo añade Swarm.Start en runtime
            Swarm swarm = school.AddComponent<Swarm>();
            swarm.kind = SwarmKind.Krill;
            swarm.unitName = "Krill";
            swarm.schoolSpread = 3f;      // banco más compacto que el de peces
            swarm.perUnitMass = 0.05f;    // cada krill pesa poquísimo
        }
    }

    // Fitoplancton: el PRODUCTOR base de la cadena marina ("césped del mar", vive de agua+luz, se autoregenera).
    // Los enjambres (Swarm) lo pastan por cercanía para crecer (fitoplancton→krill/pez). Marcadores baratos, sin
    // collider (como GrassPatch). Sembrado en las mismas aguas que los bancos. Ver docs/ice-sanctuary-ecology.md §2.2.
    // HUD DECLARATIVO generado por CÓDIGO (responde a "¿se pueden generar prefabs por código?" — SÍ): crea un panel-
    // GameObject por elemento (Quad + TextMesh) y los VINCULA con AnimaStatusPanels (color/valor vivos desde ElementsStatus).
    // No hace falta prefab manual en Unity; todo queda versionado en este .cs. (También se podría PrefabUtility.SaveAsPrefabAsset
    // para un .prefab reutilizable, como AnimalPrefabGenerator; aquí se construye en escena, que es aún más "code-first".)
    // RESTAURANTE jugable (sandbox): 3 platillos con su stock + gestor + una estación JUGABLE (en el Player) + 2 NPCs
    // cocinando. El jugador avanza cada paso con E; al llenar el stock de todos → misión cumplida (XP + inventario).
    static void BuildRestaurantSandbox(Transform parent)
    {
        GameObject rest = new GameObject("Restaurante_AUTO");
        rest.transform.SetParent(parent);

        string[] dishNames = { "Huevos revueltos", "Ensalada", "Avena", "Yogur con fruta", "Bowl de quinoa", "Menestra" };
        var stocks = new System.Collections.Generic.List<RestaurantKitchen.DishStock>();
        foreach (string dn in dishNames)
        {
            GameObject g = new GameObject($"Stock_{dn}");
            g.transform.SetParent(rest.transform);
            FoodContainer fc = g.AddComponent<FoodContainer>();
            fc.dishName = dn;
            stocks.Add(new RestaurantKitchen.DishStock { container = fc, target = 6 });
        }
        rest.AddComponent<AreaHours>();   // el restaurante CIERRA de noche (usa el reloj; de noche todos duermen)
        RestaurantKitchen kitchen = rest.AddComponent<RestaurantKitchen>();
        kitchen.dishes = stocks.ToArray();

        // Estación JUGABLE en el Player (si existe); si no, una suelta de demostración.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject cookGo = player != null ? player : new GameObject("Cocinero_Jugador");
        if (player == null) cookGo.transform.SetParent(rest.transform);
        if (cookGo.GetComponent<CharacterLevel>() == null) cookGo.AddComponent<CharacterLevel>();
        CookingStation ps = cookGo.GetComponent<CookingStation>() ?? cookGo.AddComponent<CookingStation>();
        ps.kitchen = kitchen; ps.playerDriven = true;

        BuildRealKitchen(rest.transform, kitchen, stocks);   // cocina real: 4 puestos + 4 cocineros visibles + utensilios e ingredientes de cubos
    }

    // COCINA REAL (docs/kitchen-simulation.md): dentro del ÁREA de Cocina, a tamaño real y para CUATRO cocineros a la
    // vez. Cada puesto = fogón (con sartén y olla) + tramo de encimera (tabla con cuchillo, bol, bote de utensilios).
    // Compartido: dos neveras, estanterías de despensa llenas, fregadero, encimera de ingredientes, isla de emplatado y
    // barra de servicio con los contenedores del stock. Todo de UNIONES DE CUBOS (KitchenProps) para que cada cosa se
    // distinga. Los nombres son los de siempre (Nevera, Huevos, Sarten 1, Olla 1, Bol 1, Mesa de emplatado,
    // Despensa, Plancha…), así que los hechizos walkTo/take/putIn/beat/use siguen funcionando.
    static void BuildRealKitchen(Transform rest, RestaurantKitchen kitchen, List<RestaurantKitchen.DishStock> stocks)
    {
        GameObject area = GameObject.Find("Kitchen_Area");
        GameObject roomGO = new GameObject("Sala"); roomGO.transform.SetParent(rest);
        Transform room = roomGO.transform;
        room.position = area != null ? area.transform.TransformPoint(new Vector3(0f, 0.1f, 0f)) : new Vector3(14f, 0f, 0f);   // la cocina en el CENTRO y el comedor alrededor (cocina ABIERTA al público)
        if (area != null) room.rotation = area.transform.rotation * Quaternion.Euler(0f, 180f, 0f);   // el área se abre a +Z: ahí va la entrada de la sala
        if (area != null)   // la máquina de virtualización del área, a la esquina de la entrada (no en medio de un puesto)
            foreach (Transform t in area.GetComponentsInChildren<Transform>())
                if (t.name.EndsWith("_VirtualizationMachine")) { t.localPosition = new Vector3(18.4f, 1f, -17.4f); t.localRotation = Quaternion.Euler(0f, -45f, 0f); }   // en la esquina del fondo, fuera del paso y de las mesas

        // Suelo de baldosa y contorno bajo (no tapa la vista), con la entrada al sur.
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Suelo_Cocina"; floor.transform.SetParent(room, false);
        floor.transform.localPosition = new Vector3(0f, 0.005f, 0f); floor.transform.localScale = new Vector3(19.4f, 0.04f, 15.4f);   // por encima del suelo del área
        floor.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Cocina_Baldosa_MAT", new Color(0.9f, 0.91f, 0.9f));
        for (int i = 0; i < 20; i++)   // juntas de la baldosa
        {
            GameObject j = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(j.GetComponent<Collider>());
            j.name = "Junta"; j.transform.SetParent(room, false);
            bool vx = i < 10;
            j.transform.localPosition = vx ? new Vector3(-9f + i * 2f, 0.027f, 0f) : new Vector3(0f, 0.027f, -7f + (i - 10) * 1.6f);
            j.transform.localScale = vx ? new Vector3(0.02f, 0.01f, 15.4f) : new Vector3(19.4f, 0.01f, 0.02f);
            j.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Cocina_Junta_MAT", new Color(0.78f, 0.8f, 0.8f));
        }

        // ── COCINA CÉNTRICA ── en el CENTRO, las grandes neveras y estanterías intercaladas (cada puesto tiene la suya, con
        // su cartel en la puerta); en el BORDE, los PUESTOS de trabajo completos, mirando al centro: plancha de inducción,
        // encimera con tabla y bol, fregadero, utensilios, sartén y olla, platos y dos máquinas (horno, amasadora, wok…),
        // así el mismo cocinero elabora y emplata. El cocinero toma los ingredientes del centro y los lleva a su puesto.
        var cooks = new List<KitchenCook>();
        (Vector3 pos, float yaw)[] stations =
        {
            (new Vector3(-6.2f, 0f, 6.7f), 0f), (new Vector3(0f, 0f, 6.7f), 0f), (new Vector3(6.2f, 0f, 6.7f), 0f),
            (new Vector3(-8.7f, 0f, 1.2f), -90f), (new Vector3(8.7f, 0f, 1.2f), 90f), (new Vector3(-8.7f, 0f, -3.6f), -90f),
        };
        for (int i = 0; i < stations.Length; i++)
        {
            string n = (i + 1).ToString();
            Transform st = KitchenProps.Root(room, $"Puesto_{n}", stations[i].pos, stations[i].yaw);
            WorldLabel sl = st.gameObject.AddComponent<WorldLabel>(); sl.text = $"Puesto {n}"; sl.height = 2.2f;
            Transform counter = KitchenProps.CounterRun(st, i == 0 ? "Encimera cocina" : $"Encimera {n}", new Vector3(-0.75f, 0f, 0f), 1.25f);
            Transform stove = KitchenProps.Stove(st, i == 0 ? "Plancha" : $"Plancha {n}", new Vector3(0.35f, 0f, 0f));
            Transform sinkN = KitchenProps.Sink(st, i == 0 ? "Fregadero" : $"Fregadero {n}", new Vector3(1.15f, 0f, 0f));
            Transform pan = KitchenProps.Pan(st, $"Sarten {n}", new Vector3(0.17f, KitchenProps.CounterTop + 0.02f, -0.13f), 0f);
            Transform pot = KitchenProps.Pot(st, $"Olla {n}", new Vector3(0.53f, KitchenProps.CounterTop + 0.02f, 0.15f));
            pan.SetParent(stove, true); pot.SetParent(stove, true);
            Transform board = KitchenProps.BoardAndKnife(st, $"Tabla {n}", new Vector3(-0.9f, KitchenProps.CounterTop, -0.05f));
            Transform bowl = KitchenProps.Bowl(st, $"Bol {n}", new Vector3(-0.45f, KitchenProps.CounterTop, -0.1f));
            KitchenProps.UtensilCrock(st, $"Utensilios {n}", new Vector3(-0.35f, KitchenProps.CounterTop, 0.2f));
            for (int p = 0; p < 4; p++) KitchenProps.Plate(st, "Plato", new Vector3(-1.2f, KitchenProps.CounterTop + p * 0.018f, 0.15f));
            board.SetParent(counter, true); bowl.SetParent(counter, true);
            foreach (Transform t in new[] { stove, pan, pot, board, bowl }) if (t.GetComponent<WorldLabel>() == null) { WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = t.name; wl.height = 0.5f; }
        }

        // Centro: neveras grandes y estanterías intercaladas, espalda con espalda (una fila mira a los puestos del fondo y
        // la otra a los de los lados y al pase). Cada nevera es la de un puesto: su cartel va en la puerta.
        (string name, float x, float z, float yaw, bool fridge, int station)[] core =
        {
            ("Nevera", -2.6f, 3.05f, 180f, true, 1), ("Estanteria", -1.3f, 3.05f, 180f, false, 1), ("Nevera 2", 0f, 3.05f, 180f, true, 2),
            ("Estanteria 3", 1.3f, 3.05f, 180f, false, 3), ("Nevera 3", 2.6f, 3.05f, 180f, true, 3),
            ("Nevera 4", -2.6f, 1.95f, 0f, true, 4), ("Despensa", -1.3f, 1.95f, 0f, false, 4), ("Nevera 5", 0f, 1.95f, 0f, true, 5),
            ("Estanteria 6", 1.3f, 1.95f, 0f, false, 6), ("Nevera 6", 2.6f, 1.95f, 0f, true, 6),
        };
        Transform desp = null, est2 = null;
        foreach (var c in core)
        {
            Transform t = c.fridge ? KitchenProps.Fridge(room, c.name, new Vector3(c.x, 0f, c.z), c.yaw) : KitchenProps.Shelving(room, c.name, new Vector3(c.x, 0f, c.z), c.yaw, 11 + c.station);
            if (c.fridge)
            {
                t.localScale = new Vector3(1.3f, 1.25f, 1.2f);   // neveras GRANDES: los ingredientes de la carta del día
                Transform sign = KitchenProps.C(t, new Vector3(0f, 1.25f, -0.38f), new Vector3(0.6f, 0.4f, 0.01f), new Color(0.12f, 0.13f, 0.12f), 0f, "Cartel_puerta");
                WorldLabel fl = t.gameObject.AddComponent<WorldLabel>(); fl.text = $"{c.name} · puesto {c.station}"; fl.height = 2.6f;
            }
            if (c.name == "Despensa") { desp = t; t.gameObject.AddComponent<StockShelf>().label = "Despensa"; t.gameObject.AddComponent<WorldLabel>().text = "Despensa"; }
            if (c.name == "Estanteria") est2 = t;
        }

        // ── Encimera de ingredientes (delante del bloque central) con los ingredientes de siempre ──
        KitchenProps.CounterRun(room, "Encimera ingredientes", new Vector3(0f, 0f, 0.7f), 5.5f, 0f);
        string[] items = { "Huevos", "Verduras", "Avena", "Fruta", "Yogur", "Legumbres" };
        for (int i = 0; i < items.Length; i++)
        {
            Transform holder = KitchenProps.Root(room, items[i], new Vector3(-2.1f + i * 0.85f, KitchenProps.CounterTop, 0.7f));
            KitchenProps.Ingredient(holder, items[i], Vector3.zero);
            KitchenProps.Fit(holder, false);
            WorldLabel wl = holder.gameObject.AddComponent<WorldLabel>(); wl.text = items[i]; wl.height = 0.35f;
        }

        // ── Isla de emplatado (entre el centro y el pase) con platos apilados ──
        Transform island = KitchenProps.CounterRun(room, "Mesa de emplatado", new Vector3(1.5f, 0f, -2.6f), 4.2f, 0f, 1.2f);
        island.gameObject.AddComponent<WorldLabel>().text = "Mesa de emplatado";
        for (int s = 0; s < 3; s++)
            for (int p = 0; p < 4; p++) KitchenProps.Plate(island, "Plato", new Vector3(-1.6f + s * 1.6f, KitchenProps.CounterTop + p * 0.018f, 0.3f));

        // ── Barra de servicio (junto a la entrada) con los contenedores del stock ──
        Transform pass = KitchenProps.CounterRun(room, "Barra de servicio", new Vector3(4.5f, 0f, -5.2f), 5f, 180f);
        pass.gameObject.AddComponent<WorldLabel>().text = "Barra de servicio";
        for (int i = 0; i < stocks.Count; i++)
        {
            FoodContainer fc = stocks[i].container;
            fc.transform.SetParent(pass, false);
            fc.transform.localPosition = new Vector3(-2.1f + i * 0.84f, KitchenProps.CounterTop + 0.02f, 0.15f);
            KitchenProps.C(fc.transform, new Vector3(0f, 0.02f, 0f), new Vector3(0.6f, 0.04f, 0.32f), KitchenProps.Steel, 0f, "Bandeja");
            KitchenProps.Dish(fc.transform, fc.dishName, new Vector3(0f, 0.04f, 0f));
        }

        foreach (KitchenCook kc in cooks)
        {
            kc.fridges = new[] { room.Find("Nevera"), room.Find("Nevera 2") };
            kc.pantries = new[] { desp, est2 };
            kc.island = island; kc.pass = pass;
        }

        // Contorno bajo de la sala (entrada de 3 m al sur).
        Color edge = new Color(0.55f, 0.58f, 0.62f);
        MakeOutline(room, "Contorno N", new Vector3(0f, 0.2f, 7.7f), new Vector3(19.4f, 0.4f, 0.15f), edge);
        MakeOutline(room, "Contorno O", new Vector3(-9.7f, 0.2f, 0f), new Vector3(0.15f, 0.4f, 15.4f), edge);
        MakeOutline(room, "Contorno E", new Vector3(9.7f, 0.2f, 0f), new Vector3(0.15f, 0.4f, 15.4f), edge);
        MakeOutline(room, "Contorno S1", new Vector3(-5.7f, 0.2f, -7.7f), new Vector3(8f, 0.4f, 0.15f), edge);
        MakeOutline(room, "Contorno S2", new Vector3(5.7f, 0.2f, -7.7f), new Vector3(8f, 0.4f, 0.15f), edge);

        GourmetKitchen(room);   // aspecto de cocina abierta de restaurante gourmet
        ProKitchen(room);       // equipo profesional y obrador para la carta (docs/cocina-profesional.md)

        GameObject spot = GameObject.Find("CocinaSpot");   // la Magnate lleva a los nuevos a la ENTRADA de la cocina
        if (spot != null && area != null) spot.transform.position = area.transform.TransformPoint(new Vector3(0f, -ElevatedHeight, 19f + Ladder.Gap + Ladder.FootOffset));   // al pie de la escalera vertical
    }

    // TORRE DE GUARDIA + puestos de vigía para la patrulla (docs/spells-suprahechizo-and-thoughts.md §5b).
    static void BuildGuardTower(Transform parent)
    {
        GameObject root = new GameObject("Guardia_AUTO"); root.transform.SetParent(parent);
        GuardTower.Build(root.transform, new Vector3(-20f, 0f, 57f));   // en su isla, al noroeste del hub
        (string name, Vector3 pos)[] posts =
        {
            ("PuestoVigia_1_Arrozal", new Vector3(-50f, 0f, -5f)),
            ("PuestoVigia_2_Rotacion", new Vector3(18f, 0f, -86f)),
            ("PuestoVigia_3_Pinguinos", new Vector3(60f, 0f, -17f)),
            ("PuestoVigia_4_Cocina", new Vector3(16f, 0f, 108f)),
        };
        foreach (var (n, p) in posts)
        {
            GameObject post = new GameObject(n); post.transform.SetParent(root.transform); post.transform.position = p;
            KitchenProps.C(post.transform, new Vector3(0f, 1.5f, 0f), new Vector3(0.18f, 3f, 0.18f), new Color(0.55f, 0.45f, 0.36f), 0f, "Poste");
            KitchenProps.C(post.transform, new Vector3(0f, 3.15f, 0f), new Vector3(0.35f, 0.4f, 0.35f), new Color(1f, 0.85f, 0.45f), 0f, "Farol");
            KitchenProps.C(post.transform, new Vector3(0.35f, 2.6f, 0f), new Vector3(0.6f, 0.35f, 0.03f), new Color(0.75f, 0.65f, 0.88f), 0f, "Banderin");
            WorldLabel wl = post.AddComponent<WorldLabel>(); wl.text = "Puesto de vigía"; wl.height = 3.8f;
        }
    }

    // En su escena de prueba: el jugador empieza al pie de la escalera de la torre.
    static void PlacePlayerAtTower(Transform root)
    {
        GameObject p = GameObject.Find("Player");
        if (p != null) p.transform.position = new Vector3(-20f, 1.2f, 57f - 8f);
    }

    // En la escena de prueba de la cocina: el jugador empieza en la entrada.
    static void PlacePlayerAtKitchen(Transform root)
    {
        GameObject p = GameObject.Find("Player");
        GameObject area = GameObject.Find("Kitchen_Area");
        if (p != null && area != null) p.transform.position = area.transform.TransformPoint(new Vector3(0f, 1.2f, 12f));   // sobre la plataforma, en el comedor frente a la cocina
    }

    static void MakeLabeledPrimitive(Transform parent, string label, Vector3 pos, Vector3 scale, Color col)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = label; g.transform.SetParent(parent); g.transform.localPosition = pos; g.transform.localScale = scale;
        var r = g.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = MakeMaterial($"Rest_{label}", col);
        WorldLabel wl = g.AddComponent<WorldLabel>();
        wl.text = label;
        wl.height = scale.y * 0.5f + 0.4f;
    }

    // Borde bajo SIN etiqueta (contorno de la sala).
    static void MakeOutline(Transform parent, string name, Vector3 pos, Vector3 scale, Color col)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent); g.transform.localPosition = pos; g.transform.localScale = scale;
        var r = g.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = MakeMaterial("Rest_Contorno", col);
    }

    // GOLUIS en la cocina: jefe que inicia a los recién llegados (los hace seguirlo y les enseña limpiar + recetas base).
    static void BuildGoluisKitchen(Transform parent)
    {
        // Un solo Goluis: el personaje (con alma, mente y ánimo) es también el jefe de cocina. Antes había otro Goluis
        // (una cápsula) solo para la cocina: dos humanos con el mismo nombre.
        GameObject g = GameObject.Find("Goluis_Post");
        if (g == null) { Debug.LogWarning("[Cocina] sin Goluis_Post: la cocina se queda sin jefe."); return; }
        if (g.GetComponent<KitchenBoss>() == null) g.AddComponent<KitchenBoss>();
        WorldLabel wl = g.GetOrAdd<WorldLabel>();
        wl.text = "Goluis (jefe de cocina)";
    }

    // LA MAGNATE en el santuario: cápsula visual + Anima + Mind("Magnate") + Magnate (estado infinito/guardiana).
    static void BuildMagnate(Transform parent)
    {
        GameObject m = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        m.name = "Magnate"; m.transform.SetParent(parent); m.transform.position = new Vector3(6f, 1f, 6f);
        var r = m.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = MakeMaterial("Magnate_MAT", new Color(0.85f, 0.8f, 0.95f));
        m.AddComponent<Anima>();
        m.AddComponent<Mind>().identity = "Magnate";
        Magnate mag = m.AddComponent<Magnate>();

        // Destinos de la GUÍA del nuevo integrante (la Magnate lo lleva aquí a hacer upa-yoga y luego a aprender un área).
        mag.yogaSpot = MakeSpot(parent, "SalaYoga", GameObject.Find("YogaRoom_Area") != null ? GameObject.Find("YogaRoom_Area").transform.position : new Vector3(2f, 0f, 14f), new Color(0.75f, 0.85f, 0.95f));
        GameObject kArea = GameObject.Find("Kitchen_Area");   // al pie de la escalera de la cocina (elevada)
        mag.kitchenSpot = MakeSpot(parent, "CocinaSpot", kArea != null ? kArea.transform.TransformPoint(new Vector3(0f, -ElevatedHeight, 19f + Ladder.Gap + Ladder.FootOffset)) : new Vector3(16f, 0f, 14f), new Color(0.95f, 0.8f, 0.55f));
    }

    // Marcador de destino (cubo plano etiquetado) al que la Magnate guía al nuevo integrante.
    static Transform MakeSpot(Transform parent, string name, Vector3 pos, Color col)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent); g.transform.position = pos; g.transform.localScale = new Vector3(2f, 0.1f, 2f);
        var r = g.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = MakeMaterial($"{name}_MAT", col);
        Object.DestroyImmediate(g.GetComponent<Collider>());   // solo marca el sitio, no estorba
        g.AddComponent<WorldLabel>().text = name;
        return g.transform;
    }

    // PRUEBA EN VIVO de VIDA FAMILIAR: una familia aislada con comida puesta → no gastan tiempo cazando/pastando ni
    // interactúan con otra fauna; se observa SOLO pareja/padres/crías (+ el deseo `nurse`). Lobos (pack familiar, con
    // "tíos" = más adultos) con carne; conejos con pasto. Lejos del resto del mapa. Si falta el prefab, se omite.
    static void BuildFamilyLifeTest(Transform parent)
    {
        GameObject root = new GameObject("PruebaVidaFamiliar_AUTO"); root.transform.SetParent(parent);

        // Lobos: familia + tíos (minParentsCount=3) con carne alrededor → test puro de vida de manada.
        Vector3 wolfPos = new Vector3(-140f, 0f, 0f);
        GameObject wolfPrefab = LoadAnimalPrefab("Wolf");
        if (wolfPrefab != null)
        {
            GameObject wg = new GameObject("Lobos_familia"); wg.transform.SetParent(root.transform);
            FamilyGenerator fg = wg.AddComponent<FamilyGenerator>();
            fg.families = new[] { new FamilyGenerator.Family { animalPrefab = wolfPrefab, position = wolfPos, quantity = 6, minParentsCount = 3, radius = 10f } };
            for (int i = 0; i < 6; i++)   // carne puesta (no necesitan cazar)
            {
                GameObject f = new GameObject("Carne"); f.transform.SetParent(root.transform);
                f.transform.position = wolfPos + new Vector3((i % 3) * 3f - 3f, 0.3f, (i / 3) * 3f - 1.5f);
                FoodItem fi = f.AddComponent<FoodItem>(); fi.material = OrganicMaterial.Meat; fi.grams = 200f;
            }
        }
        else Debug.LogWarning("[FamilyLifeTest] sin prefab Wolf (genera prefabs — ahora salen cubos si no hay .fbx), se omite la familia de lobos.");

        // Conejos: familia con pasto → test de vida familiar herbívora.
        Vector3 bunnyPos = new Vector3(-160f, 0f, 25f);
        GameObject bunnyPrefab = LoadAnimalPrefab("Bunny");
        if (bunnyPrefab != null)
        {
            GameObject bg = new GameObject("Conejos_familia"); bg.transform.SetParent(root.transform);
            FamilyGenerator fg = bg.AddComponent<FamilyGenerator>();
            fg.families = new[] { new FamilyGenerator.Family { animalPrefab = bunnyPrefab, position = bunnyPos, quantity = 5, minParentsCount = 2, radius = 8f } };
            for (int i = 0; i < 4; i++)   // parches de pasto (no necesitan buscar lejos)
            {
                GameObject g = new GameObject("Pasto"); g.transform.SetParent(root.transform);
                g.transform.position = bunnyPos + new Vector3((i % 2) * 4f - 2f, 0f, (i / 2) * 4f - 2f);
                g.AddComponent<GrassPatch>();
            }
        }
        else Debug.LogWarning("[FamilyLifeTest] sin prefab Bunny, se omite la familia de conejos.");
    }

    // ESCENARIOS DEDICADOS de interacción (aislados, al oeste del mapa): cada uno pone solo los actores + la comida
    // necesaria para que el test sea PURO (ver docs/sanctuary1-level-and-live-tests §2). Si falta un prefab, se omite.
    static void BuildInteractionTests(Transform parent)
    {
        GameObject root = new GameObject("PruebasInteraccion_AUTO"); root.transform.SetParent(parent);

        // 1) APAREAMIENTO: una pareja de lobos aislada con carne (no cazan) → entran en celo y se reproducen (deseo `mate`).
        TestPack(root.transform, "Wolf", new Vector3(-210f, 0f, -40f), 2, 2);
        for (int i = 0; i < 2; i++) TestMeat(root.transform, new Vector3(-210f + i * 2f, 0.3f, -40f));

        // 2) CAZA: 1 lobo (sin carne → caza) + conejos con pasto → acecho/persecución/depredación limpia.
        TestPack(root.transform, "Wolf", new Vector3(-210f, 0f, 0f), 1, 1);
        TestPack(root.transform, "Bunny", new Vector3(-200f, 0f, 8f), 4, 2);
        for (int i = 0; i < 3; i++) TestGrass(root.transform, new Vector3(-200f + i * 3f, 0f, 8f));

        // 3) 1 vs 1: dos lobos y UNA carne (recurso escaso) → competencia/conflicto.
        TestPack(root.transform, "Wolf", new Vector3(-210f, 0f, 40f), 1, 1);
        TestPack(root.transform, "Wolf", new Vector3(-204f, 0f, 40f), 1, 1);
        TestMeat(root.transform, new Vector3(-207f, 0.3f, 40f));

        // 4) PACK vs 1: manada de lobos + un oso → defensa/ataque de grupo (PackAwareness).
        TestPack(root.transform, "Wolf", new Vector3(-240f, 0f, -20f), 6, 3);
        TestPack(root.transform, "Bear", new Vector3(-232f, 0f, -20f), 1, 1);   // "Bear" = el prefab/catálogo real (no existe "PolarBear")

        // 5) PACK vs PACK: dos manadas de lobos contiguas.
        TestPack(root.transform, "Wolf", new Vector3(-240f, 0f, 20f), 5, 2);
        TestPack(root.transform, "Wolf", new Vector3(-228f, 0f, 24f), 5, 2);
    }

    static void TestPack(Transform root, string species, Vector3 pos, int qty, int minParents)
    {
        GameObject prefab = LoadAnimalPrefab(species);
        if (prefab == null) { Debug.LogWarning($"[InteractionTest] sin prefab {species} (genera prefabs — salen cubos si no hay .fbx), se omite."); return; }
        GameObject g = new GameObject($"{species}_x{qty}"); g.transform.SetParent(root);
        FamilyGenerator fg = g.AddComponent<FamilyGenerator>();
        fg.families = new[] { new FamilyGenerator.Family { animalPrefab = prefab, position = pos, quantity = qty, minParentsCount = minParents, radius = Mathf.Max(3f, qty * 1.5f) } };
    }

    static void TestMeat(Transform root, Vector3 pos)
    {
        GameObject f = new GameObject("Carne"); f.transform.SetParent(root); f.transform.position = pos;
        FoodItem fi = f.AddComponent<FoodItem>(); fi.material = OrganicMaterial.Meat; fi.grams = 200f;
    }

    static void TestGrass(Transform root, Vector3 pos)
    {
        GameObject g = new GameObject("Pasto"); g.transform.SetParent(root); g.transform.position = pos;
        g.AddComponent<GrassPatch>();
    }

    static void TestFish(Transform root, Vector3 pos)
    {
        GameObject f = new GameObject("Pescado"); f.transform.SetParent(root); f.transform.position = pos;
        FoodItem fi = f.AddComponent<FoodItem>(); fi.material = OrganicMaterial.Fish; fi.grams = 150f;
    }

    // Mini-cardumen (pez o krill) para los escenarios por especie: cubo translúcido + Swarm (el trigger lo pone Swarm.Start).
    static void MiniSwarm(Transform root, string name, SwarmKind kind, Vector3 pos)
    {
        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
        s.name = name; s.transform.SetParent(root); s.transform.position = pos; s.transform.localScale = new Vector3(6f, 1.5f, 6f);
        Color col = kind == SwarmKind.Krill ? new Color(0.75f, 0.45f, 0.4f, 0.6f) : new Color(0.35f, 0.55f, 0.65f, 0.6f);
        var r = s.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = MakeMaterial($"{name}_MAT", col);
        Object.DestroyImmediate(s.GetComponent<Collider>());
        Swarm sw = s.AddComponent<Swarm>(); sw.kind = kind; sw.unitName = kind == SwarmKind.Krill ? "Krill" : "Fish";
    }

    // ESCENARIOS POR ESPECIE (docs/ice-sanctuary-ecology.md): cada animal en un sandbox AISLADO con su CICLO/INTERACCIÓN
    // representativa (dieta + presa/depredador + convivencia). Coherentes con el Santuario 1 (hielo); los templados
    // (zorro/ciervo/malamute) quedan como demostración reservada. Reutilizan TestPack/TestFish/TestMeat/TestGrass/MiniSwarm;
    // los ciclos (celo/sueño/caza/depredación) se AUTOCONFIGURAN desde el arquetipo de la especie en runtime (Animal.Init).
    static void BuildSpeciesScenarios(Transform parent)
    {
        GameObject root = new GameObject("EscenariosEspecie_AUTO"); root.transform.SetParent(parent);
        Transform r = root.transform;

        // ── Hielo (coherentes con el bioma) ─────────────────────────────────────────────────────
        // 1) OSO — pesca en el hielo + una foca como presa potencial (ápice; come peces y presa).
        TestPack(r, "Bear", new Vector3(300f, 0f, -60f), 1, 1);
        MiniSwarm(r, "Peces_oso", SwarmKind.Fish, new Vector3(304f, -1f, -60f));
        TestFish(r, new Vector3(302f, 0.2f, -58f));
        TestPack(r, "Seal", new Vector3(308f, 0f, -62f), 1, 1);

        // 2) FOCA — descansa y come peces; una ORCA merodea (depredador) → huida al avistarla.
        TestPack(r, "Seal", new Vector3(300f, 0f, -30f), 2, 1);
        MiniSwarm(r, "Peces_foca", SwarmKind.Fish, new Vector3(303f, -1f, -30f));
        TestPack(r, "Orca", new Vector3(312f, 0f, -30f), 1, 1);

        // 3) PINGÜINO — colonia (familia grande) con krill y peces; una FOCA depredadora cerca.
        TestPack(r, "Penguin", new Vector3(300f, 0f, 0f), 6, 2);
        MiniSwarm(r, "Krill_ping", SwarmKind.Krill, new Vector3(304f, -1f, 2f));
        MiniSwarm(r, "Peces_ping", SwarmKind.Fish, new Vector3(296f, -1f, -2f));
        TestPack(r, "Seal", new Vector3(310f, 0f, 2f), 1, 1);

        // 4) ORCA — caza: una orca y una pequeña colonia de pingüinos de presa.
        TestPack(r, "Orca", new Vector3(300f, 0f, 30f), 1, 1);
        TestPack(r, "Penguin", new Vector3(306f, 0f, 32f), 3, 1);

        // 5) BALLENA — filtra krill en paz (filter feeder; sin amenaza).
        TestPack(r, "Whale", new Vector3(300f, 0f, 60f), 1, 1);
        MiniSwarm(r, "Krill_ballena", SwarmKind.Krill, new Vector3(305f, -1f, 60f));

        // ── Templados (reserva demostrativa; fuera del bioma de hielo) ───────────────────────────
        // 6) ZORRO — acecho oportunista de un conejo + peces (come presa y pez).
        TestPack(r, "Fox", new Vector3(340f, 0f, -40f), 1, 1);
        TestPack(r, "Bunny", new Vector3(346f, 0f, -38f), 3, 2);
        for (int i = 0; i < 2; i++) TestGrass(r, new Vector3(346f + i * 2f, 0f, -38f));

        // 7) CIERVO — manada que pasta; un lobo de amenaza → huida en grupo.
        TestPack(r, "Deer", new Vector3(340f, 0f, 0f), 4, 2);
        for (int i = 0; i < 3; i++) TestGrass(r, new Vector3(340f + i * 2f, 0f, 0f));
        TestPack(r, "Wolf", new Vector3(352f, 0f, 2f), 1, 1);

        // 8) MALAMUTE — perro de trineo domesticado: junto a carne, convive en paz.
        TestPack(r, "Malamute", new Vector3(340f, 0f, 40f), 2, 1);
        for (int i = 0; i < 2; i++) TestMeat(r, new Vector3(340f + i * 2f, 0.3f, 40f));
    }

    // FLORA DE HIELO (docs/ice-sanctuary-ecology.md): el árbol carnívoro (atrae hambrientos con olor a comida → sopor →
    // muerte → crece) + una colonia de abejas de las nieves que anida cerca. Primitivas (independiente de modelos).
    static void BuildIceFlora(Transform parent)
    {
        // BOSQUE DE HIELO con EDADES: arboledas de un árbol madre (adulto o viejo) rodeado de jóvenes y brotes, y árboles
        // sueltos, esparcidos por todo el hielo (docs/ice-sanctuary-ecology §2). Lejos de las áreas, del hub, de los nidos
        // y de los canales, para no cortar el paso. Los brotes aún no cazan; los viejos tienen más raíces y una rama seca.
        GameObject root = new GameObject("FloraHielo_AUTO"); root.transform.SetParent(parent);
        // VOLUMEN de cada área (todas sus piezas: plataforma, pisos superiores, galerías, barandillas, con 1,5 m de margen):
        // las ramas lo esquivan, como en la arboleda residencial. Las áreas apiladas (el textil sobre la Construcción a
        // 10 m) son su propio SanctuaryArea, así que su piso también cuenta.
        var volumes = new List<Bounds>();
        foreach (SanctuaryArea sa in Object.FindObjectsOfType<SanctuaryArea>())
        {
            Renderer[] rs = sa.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) continue;
            Bounds bb = rs[0].bounds; foreach (Renderer rr in rs) bb.Encapsulate(rr.bounds);
            bb.Expand(new Vector3(3f, 3f, 3f)); volumes.Add(bb);
        }
        bool Built(Vector3 q) { foreach (Bounds bb in volumes) if (bb.Contains(q)) return true; return false; }
        var avoid = new List<Vector3>
        {
            new Vector3(5f, 0f, 38f),       // hub
            new Vector3(30f, -13f, 12f),    // aparición del jugador
            new Vector3(130f, -10f, 34f),   // osos
            new Vector3(155f, 5f, 24f),     // pingüinos
        };
        foreach (SanctuaryArea a in Object.FindObjectsOfType<SanctuaryArea>())
            avoid.Add(new Vector3(a.transform.position.x, a.transform.position.z, 24f));
        foreach (GardenPlot gp in Object.FindObjectsOfType<GardenPlot>())   // los huertos tampoco van bajo un árbol carnívoro
            avoid.Add(new Vector3(gp.transform.position.x, gp.transform.position.z, gp.size.magnitude * 0.5f + 8f));
        avoid.Add(new Vector3(-60f, 95f, 28f));   // la arboleda residencial (sus árboles no cazan)
        GameObject tower = GameObject.Find("CasaArbolGuardia");   // ni junto a la casa del árbol
        if (tower != null) avoid.Add(new Vector3(tower.transform.position.x, tower.transform.position.z - 6f, 22f));
        var plan = PolarLandscapeBuilder.PlanForest(avoid, 9173, groves: 9, loose: 0);   // menos árboles: solo en los dos casquetes salvajes
        GameObject first = null;
        var hiveTrees = new List<Vector3>();
        var count = new Dictionary<PolarLandscapeBuilder.TreeAge, int>();
        for (int i = 0; i < plan.Count; i++)
        {
            var (pos, age, h) = plan[i];
            GameObject tree = CubeTree.Build(root.transform, $"ArbolCarnivoroHielo_{age}_{i + 1}", pos, 1234 + i * 77, h, blocked: Built);
            count[age] = count.TryGetValue(age, out int c) ? c + 1 : 1;
            if (first == null && age != PolarLandscapeBuilder.TreeAge.Brote) first = tree;
            if (age >= PolarLandscapeBuilder.TreeAge.Adulto && hiveTrees.Count < 4 && i % 3 == 0) hiveTrees.Add(pos);
        }
        if (first != null)
        {
            WorldLabel wl = first.AddComponent<WorldLabel>();
            wl.text = "Bosque de árboles carnívoros de hielo";
            wl.height = 24f;
        }

        GameObject beePrefab = LoadAnimalPrefab("SnowBee");
        if (beePrefab != null)
        {
            // Colmenas de abejas de las nieves en árboles grandes (anidan en sus ramas, miel de los muertos).
            for (int h = 0; h < hiveTrees.Count; h++)
            {
                GameObject bg = new GameObject($"Abejas_nieve_{h + 1}"); bg.transform.SetParent(root.transform);
                FamilyGenerator fg = bg.AddComponent<FamilyGenerator>();
                fg.families = new[] { new FamilyGenerator.Family { animalPrefab = beePrefab, position = hiveTrees[h] + new Vector3(4f, 0f, 0f), quantity = 5, minParentsCount = 1, radius = 5f } };
            }
        }
        GameObject spiderPrefab = LoadAnimalPrefab("SnowSpider");
        if (spiderPrefab != null)   // ARAÑAS DE LAS NIEVES: dos por árbol con colmena (acechan en el tronco)
            for (int h = 0; h < hiveTrees.Count; h++)
            {
                GameObject sg = new GameObject($"Aranas_nieve_{h + 1}"); sg.transform.SetParent(root.transform);
                FamilyGenerator sfg = sg.AddComponent<FamilyGenerator>();
                sfg.families = new[] { new FamilyGenerator.Family { animalPrefab = spiderPrefab, position = hiveTrees[h] + new Vector3(-2f, 0f, 1f), quantity = 2, minParentsCount = 1, radius = 2f } };
            }
        else Debug.LogWarning("[IceFlora] sin prefab SnowBee (genera prefabs — salen cubos si no hay .fbx), se omiten las abejas.");
        string ages = string.Join(", ", count.Select(kv => $"{kv.Value} {kv.Key.ToString().ToLowerInvariant()}"));
        Debug.Log($"[IceFlora] Árboles carnívoros de bloques: {plan.Count} esparcidos ({ages}).");

        // NATURALEZA Y ÁREAS FUSIONADAS: junto a cada área humana crecen árboles de hielo grandes (24–36 m) que NO cazan;
        // las áreas elevadas quedan entre sus troncos (la gente vive con la naturaleza, no en un claro aparte).
        var rndF = new System.Random(5150);
        int fused = 0;
        // Huella de cada área (sus piezas, con 3 m de margen): ningún árbol crece DENTRO de una plataforma.
        var prints = new List<Bounds>();
        foreach (SanctuaryArea sa in Object.FindObjectsOfType<SanctuaryArea>())
        {
            Renderer[] rs = sa.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) continue;
            Bounds bb = rs[0].bounds; foreach (Renderer rr in rs) bb.Encapsulate(rr.bounds);
            bb.Expand(new Vector3(6f, 0f, 6f)); prints.Add(bb);
        }
        bool InsideArea(Vector3 q) { foreach (Bounds bb in prints) if (q.x > bb.min.x && q.x < bb.max.x && q.z > bb.min.z && q.z < bb.max.z) return true; return false; }
        foreach (SanctuaryArea a in Object.FindObjectsOfType<SanctuaryArea>())
        {
            Vector3 c = a.transform.position; c.y = 0f;
            Renderer[] ars = a.GetComponentsInChildren<Renderer>();
            float reach = 16f;
            if (ars.Length > 0) { Bounds ab = ars[0].bounds; foreach (Renderer rr in ars) ab.Encapsulate(rr.bounds); reach = Mathf.Max(16f, new Vector2(ab.extents.x, ab.extents.z).magnitude + 3f); }
            for (int k = 0; k < 3; k++)
            {
                float ang = (k * 120f + 30f + (float)rndF.NextDouble() * 30f) * Mathf.Deg2Rad;   // nunca delante de la escalera (+Z)
                if (Mathf.Sin(ang) > 0.85f) ang += 0.6f;
                Vector3 p = c + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * (reach + (float)rndF.NextDouble() * 4f);
                bool ice = true;   // con hielo alrededor (las raíces no cuelgan sobre el agua)
                for (int q = 0; q < 8 && ice; q++) ice = PolarLandscapeBuilder.NaturalIce(p.x + Mathf.Cos(q * 0.785f) * 5f, p.z + Mathf.Sin(q * 0.785f) * 5f);
                if (!ice || !PolarLandscapeBuilder.NaturalIce(p.x, p.z) || InsideArea(p)) continue;
                GameObject dining = GameObject.Find("Comedor_Gourmet");
                if (dining != null && Vector2.Distance(new Vector2(p.x, p.z), new Vector2(dining.transform.position.x, dining.transform.position.z)) < 15f) continue;
                CubeTree.Build(root.transform, $"ArbolHielo_{a.displayName}_{k + 1}", p, 7000 + fused * 31, 24f + (float)rndF.NextDouble() * 12f, carnivorous: false, blocked: Built);
                fused++;
            }
        }
        Debug.Log($"[IceFlora] {fused} árboles junto a las áreas (no cazan).");
    }

    // LINDUARDA en la enfermería + un paciente demo (Kushal) con HealthRecord en false → ella, por su pensamiento de
    // salud, lanza el status global y va a atenderlo (análisis + vacuna). Guion EMERGENTE (no scriptado).
    /// <summary>Un personaje del juego (cápsula que <c>DressHumanoids</c> viste con su aspecto de CubeHair.For).</summary>
    static GameObject MakeCharacter(Transform parent, string name, string label, Vector3 pos, Color clothes, float strength, float agility, float endurance, float mass)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        g.name = name; g.transform.SetParent(parent); g.transform.position = pos;
        var r = g.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = MakeMaterial(name + "_MAT", clothes);
        Anima a = g.AddComponent<Anima>();
        a.strength = strength; a.agility = agility; a.endurance = endurance; a.bodyMass = mass;
        g.AddComponent<Mind>().identity = name;
        g.AddComponent<WorldLabel>().text = label;
        g.AddComponent<WorldCharacter>().characterName = name;
        return g;
    }

    static void BuildLinduarda(Transform parent)
    {
        GameObject root = new GameObject("Enfermeria_AUTO"); root.transform.SetParent(parent);

        GameObject l = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        l.name = "Linduarda"; l.transform.SetParent(root.transform); l.transform.position = new Vector3(8f, 1f, 8f);
        var r = l.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = MakeMaterial("Linduarda_MAT", new Color(0.9f, 0.8f, 0.85f));
        Anima la = l.AddComponent<Anima>();
        // Deportista, gimnasio a diario (docs/character-linduarda.md): ágil y resistente, fuerza algo por encima de la media.
        la.strength = 1.15f; la.agility = 1.35f; la.endurance = 1.45f; la.bodyMass = 1.05f;
        l.AddComponent<Mind>().identity = "Linduarda";
        l.AddComponent<Linduarda>();

        // LIZMIBET (docs/character-irosene.md): la gran amiga de Irosene, de su misma edad (70+), llegada con ella desde
        // Ecuador, co-abuela de Leo; también MÉDICA: atiende con Linduarda en la enfermería.
        GameObject liz = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        liz.name = "Lizmibet"; liz.transform.SetParent(root.transform); liz.transform.position = new Vector3(10f, 1f, 8f);
        var lr = liz.GetComponent<Renderer>(); if (lr != null) lr.sharedMaterial = MakeMaterial("Lizmibet_MAT", new Color(0.95f, 0.95f, 0.97f));
        Anima lza = liz.AddComponent<Anima>();
        lza.strength = 0.95f; lza.agility = 1.05f; lza.endurance = 1.2f; lza.bodyMass = 1.05f;   // activa a sus 70 y tantos
        liz.AddComponent<Mind>().identity = "Lizmibet";
        WorldLabel lzl = liz.AddComponent<WorldLabel>(); lzl.text = "Lizmibet (médica)";
        liz.AddComponent<WorldCharacter>().characterName = "Lizmibet";

        // Su ESPOSO, JEFE DE CONSTRUCCIÓN (docs/construction-simulation.md): repara y levanta las estructuras de las áreas
        // humanas desde el área de Construcción (maquinaria y material). SADHGURU da las clases en el centro de yoga.
        // ANDREA es uno de los DUEÑOS del santuario (los turistas son una de sus vías de ingresos).
        MakeCharacter(root.transform, "JefeConstruccion", "Jefe de construcción (esposo de Lizmibet)", new Vector3(12f, 1f, 8f), new Color(0.78f, 0.8f, 0.82f), 1.3f, 0.95f, 1.25f, 1.15f);
        MakeCharacter(root.transform, "Sadhguru", "Sadhguru (maestro de yoga)", new Vector3(14f, 1f, 8f), new Color(0.5f, 0.72f, 0.62f), 1.0f, 1.2f, 1.4f, 1.0f);
        MakeCharacter(root.transform, "Andrea", "Andrea (dueño del santuario)", new Vector3(16f, 1f, 8f), new Color(0.08f, 0.08f, 0.09f), 1.15f, 1.2f, 1.2f, 1.0f);

        GameObject wolf = LoadAnimalPrefab("Wolf");
        if (wolf != null)
        {
            GameObject p = (GameObject)PrefabUtility.InstantiatePrefab(wolf);
            p.name = "Lobo_paciente"; p.transform.SetParent(root.transform); p.transform.position = new Vector3(12f, 0f, 8f);   // paciente animal (veterinaria); Kushal es el JUGADOR
            HealthRecord hr = p.AddComponent<HealthRecord>(); hr.vaccinated = false; hr.analyzed = false;
            // (el lobo solo necesita su análisis y vacuna; no es aspirante a miembro)
        }
        else Debug.LogWarning("[Linduarda] sin prefab Wolf para el paciente demo (genera prefabs — salen cubos), se omite.");
        // KUSHAL (el jugador) es el RECIÉN LLEGADO: empieza en la enfermería sin análisis ni vacuna, y su iniciación con
        // Linduarda termina con su primer viaje al microcosmos (1 misión introductoria); después lo inicia la Magnate.
        GameObject kushal = GameObject.Find("Player");
        if (kushal != null)
        {
            if (!kushal.TryGetComponent(out HealthRecord khr)) khr = kushal.AddComponent<HealthRecord>();
            khr.vaccinated = false; khr.analyzed = false;
            if (!kushal.TryGetComponent(out SanctuaryOnboarding kon)) kon = kushal.AddComponent<SanctuaryOnboarding>();
            kon.introMissionsRequired = 1; kon.autoCompleteForDemo = false;
        }
        BuildInfirmaryRoom(parent);   // la enfermería equipada (y Linduarda + paciente dentro)
    }

    // ENFERMERÍA equipada (docs/sanctuary1-level-and-live-tests §4): dentro del área, parte HUMANA (camas con suero y
    // monitor, camilla, mesa del médico, botiquín, rayos X, laboratorio y analizador de sangre = estación de análisis
    // clínico con F) y parte VETERINARIA (mesa hidráulica con lámpara, cuadra para animales grandes, jaulas, báscula y
    // ecógrafo). Linduarda trabaja aquí y el paciente de prueba espera en la cuadra.
    static void BuildInfirmaryRoom(Transform root)
    {
        GameObject area = GameObject.Find("Infirmary_Area");
        if (area == null) return;
        Transform r = new GameObject("Equipamiento").transform;
        r.SetParent(area.transform, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Suelo_Enfermeria"; floor.transform.SetParent(r, false);
        floor.transform.localPosition = new Vector3(0f, 0.005f, 0f); floor.transform.localScale = new Vector3(25.6f, 0.04f, 19.6f);
        floor.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Enfermeria_Suelo_MAT", new Color(0.86f, 0.94f, 0.92f));
        // Franja que separa la parte humana (oeste) de la veterinaria (este).
        KitchenProps.C(r, new Vector3(0f, 0.03f, 0f), new Vector3(0.12f, 0.01f, 13.6f), new Color(0.55f, 0.75f, 0.7f), 0f, "Separacion");

        Color[] blankets = { new Color(0.62f, 0.8f, 0.92f), new Color(0.75f, 0.9f, 0.8f), new Color(0.95f, 0.85f, 0.7f) };
        for (int i = 0; i < 3; i++) Label(InfirmaryProps.HospitalBed(r, $"Cama {i + 1}", new Vector3(-7.4f + i * 2.1f, 0f, -4.8f), 0f, blankets[i]), $"Cama {i + 1}", 1.4f);   // urgencias: a pie de suelo
        Label(InfirmaryProps.ExamTable(r, "Camilla", new Vector3(-6.9f, 0f, 1.2f), 90f), "Camilla de exploración", 1.3f);
        // SILLA DE EXÁMENES (iniciación: constantes, pupilas, extracción y vacuna): sentado, con reposabrazos para el brazo.
        Transform exam = KitchenProps.Root(r, "Silla_examen", new Vector3(-5.2f, 0f, -0.6f), 90f);
        Color seatC = new Color(0.35f, 0.55f, 0.65f);
        KitchenProps.C(exam, new Vector3(0f, 0.45f, 0f), new Vector3(0.6f, 0.1f, 0.6f), seatC, 0f, "Asiento_examen");
        KitchenProps.C(exam, new Vector3(0f, 0.85f, -0.27f), new Vector3(0.6f, 0.75f, 0.08f), seatC, 0f, "Respaldo");
        KitchenProps.C(exam, new Vector3(0.36f, 0.68f, 0.05f), new Vector3(0.1f, 0.06f, 0.6f), seatC, 0f, "Reposabrazos");
        KitchenProps.C(exam, new Vector3(0f, 0.2f, 0f), new Vector3(0.08f, 0.4f, 0.08f), KitchenProps.Steel, 0f, "Pie");
        KitchenProps.C(exam, new Vector3(0.75f, 0.45f, 0.3f), new Vector3(0.4f, 0.9f, 0.4f), KitchenProps.Steel, 0f, "Carrito_examen");   // tensiómetro, linterna, tubos y vacunas
        Label(exam, "Silla de exámenes (iniciación)", 1.4f);
        Label(InfirmaryProps.DoctorDesk(r, "Mesa del medico", new Vector3(-4.2f, 0f, 3.8f), 180f), "Mesa del médico", 1.5f);
        Label(InfirmaryProps.MedicineCabinet(r, "Botiquin", new Vector3(-7.8f, 0f, -1.6f), -90f), "Botiquín", 2.2f);
        Label(InfirmaryProps.XRay(r, "Rayos X", new Vector3(-1.4f, 0f, -4.6f), 0f), "Rayos X", 2.6f);
        Label(InfirmaryProps.LabBench(r, "Laboratorio", new Vector3(1.6f, 0f, -5.2f), 180f), "Laboratorio (microscopio, centrífuga)", 1.4f);
        Transform an = InfirmaryProps.BloodAnalyzer(r, "Analizador clinico", new Vector3(4.2f, 0f, -5.2f), 180f);
        an.gameObject.AddComponent<ClinicalStation>();
        Label(an, "Analizador clínico (F)", 1.8f);
        KitchenProps.Sink(r, "Lavamanos", new Vector3(-7.8f, 0f, 4.6f), -90f);

        // CAPACIDAD PARA 30 (residentes y turistas): las otras 27 camas, ARRIBA, en una GALERÍA EN U de 2,2 m (plataforma
        // hija: alas oeste y este + fondo) con dos escaleras inclinadas (la oeste junto a la entrada; la este, de lado). Abajo ya no hay filas de camas
        // contra las paredes: exploración al oeste, veterinaria al este y el paso libre (docs/construcciones-elevadas.md §Pisos).
        Color[] bl = { new Color(0.62f, 0.8f, 0.92f), new Color(0.75f, 0.9f, 0.8f), new Color(0.95f, 0.85f, 0.7f), new Color(0.9f, 0.78f, 0.85f) };
        int bed = 4;
        float g = MezzanineHeight;
        Transform west = Deck(r, "Galeria_oeste", new Vector3(-10.6f, 0f, -2.2f), new Vector2(4.4f, 15.2f));
        Transform east = Deck(r, "Galeria_este", new Vector3(10.6f, 0f, -2.2f), new Vector2(4.4f, 15.2f));
        Transform back = Deck(r, "Galeria_fondo", new Vector3(0f, 0f, -7.9f), new Vector2(16.8f, 3.8f));
        for (int i = 0; i < 9; i++, bed++) Label(InfirmaryProps.HospitalBed(r, $"Cama {bed}", new Vector3(-11.7f, g, -8.6f + i * 1.65f), 90f, bl[i % 4]), $"Cama {bed}", 1.4f);
        for (int i = 0; i < 9; i++, bed++) Label(InfirmaryProps.HospitalBed(r, $"Cama {bed}", new Vector3(-7.4f + i * 1.85f, g, -8.7f), 0f, bl[i % 4]), $"Cama {bed}", 1.4f);
        for (int i = 0; i < 9; i++, bed++) Label(InfirmaryProps.HospitalBed(r, $"Cama {bed}", new Vector3(11.7f, g, -8.6f + i * 1.65f), -90f, bl[i % 4]), $"Cama {bed}", 1.4f);
        // Barandillas en los bordes interiores (y en el frente de las alas, con el hueco de su escalera).
        float run = StairRun();
        DeckRail(r, new Vector3(-8.4f, g, -6f), new Vector3(-8.4f, g, 5.4f));
        DeckRail(r, new Vector3(8.4f, g, -6f), new Vector3(8.4f, g, 2.8f)); DeckRail(r, new Vector3(8.4f, g, 4.2f), new Vector3(8.4f, g, 5.4f));
        DeckRail(r, new Vector3(-8.4f, g, -6f), new Vector3(8.4f, g, -6f));
        DeckRail(r, new Vector3(-12.8f, g, 5.4f), new Vector3(-11.4f, g, 5.4f)); DeckRail(r, new Vector3(-9.8f, g, 5.4f), new Vector3(-8.4f, g, 5.4f));
        DeckRail(r, new Vector3(8.4f, g, 5.4f), new Vector3(12.8f, g, 5.4f));
        InclinedStair(r, "Escalera_galeria_oeste", new Vector3(-10.6f, 0f, 5.4f + run), 180f);
        InclinedStair(r, "Escalera_galeria_este", new Vector3(8.4f - run, 0f, 3.5f), 90f);   // de lado: al frente del ala está el tanque
        Label(west, "Sala de camas (oeste)", 1.8f); Label(east, "Sala de camas (este)", 1.8f); Label(back, "Sala de camas (fondo)", 1.8f);
        Transform wait = KitchenProps.Root(r, "Sala_de_espera", new Vector3(4.3f, 0f, 7.8f));   // a un lado de la entrada (no la tapa)
        for (int i = 0; i < 10; i++)
        {
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.45f, (i / 5) * 1.2f), new Vector3(0.5f, 0.08f, 0.5f), new Color(0.45f, 0.62f, 0.72f), 0f, "Silla");
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.75f, (i / 5) * 1.2f + 0.24f), new Vector3(0.5f, 0.55f, 0.06f), new Color(0.45f, 0.62f, 0.72f), 0f, "Respaldo");
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.22f, (i / 5) * 1.2f), new Vector3(0.06f, 0.44f, 0.06f), KitchenProps.Steel, 0f, "Pata");
        }
        Label(wait, "Sala de espera", 1.5f);
        Aseos(r, new Vector3(-4.7f, 0f, 10f - 0.9f), 180f);   // junto a la entrada, entre la escalera oeste y la vertical

        Label(InfirmaryProps.VetTable(r, "Mesa veterinaria", new Vector3(3.6f, 0f, 0.4f), 90f), "Mesa veterinaria", 1.4f);
        Label(InfirmaryProps.LargeAnimalStall(r, "Cuadra", new Vector3(6.4f, 0f, -4.2f), 0f), "Cuadra (animales grandes)", 1.8f);
        Label(InfirmaryProps.Cages(r, "Jaulas", new Vector3(7.2f, 0f, 1.6f), 90f), "Jaulas", 1.6f);
        Label(InfirmaryProps.FloorScale(r, "Bascula", new Vector3(5.8f, 0f, 5.8f), 0f), "Báscula", 1.5f);
        Label(InfirmaryProps.Ultrasound(r, "Ecografo", new Vector3(1.8f, 0f, 1.4f), 0f), "Ecógrafo", 1.6f);

        // Linduarda en su mesa; el paciente de prueba (lobo «Kushal») en la cuadra.
        GameObject l = GameObject.Find("Linduarda");
        if (l != null) l.transform.position = area.transform.TransformPoint(new Vector3(-4.2f, 1f, 2.6f));
        GameObject lz = GameObject.Find("Lizmibet");
        if (lz != null) lz.transform.position = area.transform.TransformPoint(new Vector3(-10.4f, 0.1f + MezzanineHeight + 1f, -3f));   // pasando visita en la galería
        GameObject p = GameObject.Find("Lobo_paciente");
        if (p != null) p.transform.position = area.transform.TransformPoint(new Vector3(6.4f, 0.15f, -4.2f));
        // Kushal (el jugador) EMPIEZA AQUÍ, junto a la primera cama: es el recién llegado.
        GameObject kushal = GameObject.Find("Player");
        if (kushal != null) kushal.transform.position = area.transform.TransformPoint(new Vector3(-5.5f, 1.3f, -2.6f));
        // La doctora inicia a los recién llegados en el TANQUE de la enfermería (no en cualquier máquina).
        VirtualizationMachine vm = area.GetComponentInChildren<VirtualizationMachine>();
        if (l != null && vm != null && l.TryGetComponent(out Linduarda lin)) lin.virtualizationMachine = vm;
    }

    static void PlacePlayerAtInfirmary(Transform root)
    {
        GameObject p = GameObject.Find("Player"), area = GameObject.Find("Infirmary_Area");
        if (p != null && area != null) p.transform.position = area.transform.TransformPoint(new Vector3(0f, 1.2f, 4.5f));   // sobre la plataforma
    }

    static void Label(Transform t, string text, float height)
    {
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = text; wl.height = height;
    }

    static void BuildAnimaStatusPanels(Transform parent)
    {
        GameObject group = new GameObject("AnimaStatusPanels_AUTO");
        group.transform.SetParent(parent);

        string[] els = { "O", "C", "H", "N", "Ca", "Fe" };
        var mapping = new System.Collections.Generic.List<AnimaStatusPanels.ElementPanel>();
        for (int i = 0; i < els.Length; i++)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            panel.name = $"Panel_{els[i]}";
            panel.transform.SetParent(group.transform);
            panel.transform.localPosition = new Vector3(0f, 6f - i * 0.6f, 0f);   // pila vertical (blockout)
            panel.transform.localScale = new Vector3(2f, 0.5f, 1f);
            Object.DestroyImmediate(panel.GetComponent<Collider>());              // decorativo, no bloquea

            GameObject label = new GameObject("Label");
            label.transform.SetParent(panel.transform);
            label.transform.localPosition = new Vector3(0f, 0f, -0.05f);
            label.transform.localScale = Vector3.one * 0.15f;
            TextMesh tm = label.AddComponent<TextMesh>();
            tm.text = els[i]; tm.anchor = TextAnchor.MiddleCenter; tm.characterSize = 0.5f; tm.fontSize = 48;

            mapping.Add(new AnimaStatusPanels.ElementPanel { element = els[i], panel = panel });
        }

        AnimaStatusPanels binder = group.AddComponent<AnimaStatusPanels>();
        binder.panels = mapping.ToArray();   // vincula cada panel a su elemento → color/valor vivos por ElementsStatus
    }

    static void BuildPhytoplankton(Transform parent)
    {
        GameObject group = new GameObject("Phytoplankton_AUTO");
        group.transform.SetParent(parent);

        Vector3[] positions =
        {
            new Vector3(55f, -2f, 25f),
            new Vector3(68f, -2f, 18f),
            new Vector3(60f, -2f, 33f),
        };

        Material planktonMat = MakeMaterial("Phytoplankton_MAT", new Color(0.3f, 0.6f, 0.45f, 0.5f));
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject cloud = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloud.name = $"Phytoplankton_{i}";
            cloud.transform.SetParent(group.transform);
            cloud.transform.position = positions[i];
            PolarLandscapeBuilder.IgnoreForNav(cloud);
            cloud.transform.localScale = new Vector3(12f, 1f, 12f);
            cloud.GetComponent<Renderer>().sharedMaterial = planktonMat;
            Object.DestroyImmediate(cloud.GetComponent<Collider>()); // decorativo — pastado por cercanía, no por colisión
            cloud.AddComponent<Phytoplankton>();
        }
    }

    // Cobertura vegetal decorativa alrededor de cada nido de presa terrestre — para que "se
    // vean un poco escondidos" (pedido explícito). Puramente visual: primitivas sin collider
    // (igual que GrassPatch/Swarm), sin componente de lógica — NO implementa detección ni
    // ocultamiento real, eso queda documentado como pendiente en
    // docs/refuge-and-adult-behavior.md ("empezar documentando, luego implementar").
    static void BuildHabitatCover(Transform parent, System.Collections.Generic.List<NestSpec> nests)
    {
        GameObject group = new GameObject("HabitatCover_AUTO");
        group.transform.SetParent(parent);

        // Bioma de HIELO (docs/ice-sanctuary-ecology §2.1): cobertura de tundra helada, no bosque templado verde.
        Material trunkMat = MakeMaterial("Tree_Trunk_MAT", new Color(0.32f, 0.34f, 0.40f));
        Material canopyMat = MakeMaterial("Tree_Canopy_MAT", new Color(0.78f, 0.86f, 0.88f));   // copa escarchada
        Material bushMat = MakeMaterial("Bush_MAT", new Color(0.52f, 0.58f, 0.50f));             // líquenes/musgo

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

        // (Ya no hay «Maestra_Post»: era un humano de más. La maestra del santuario ES la Magnate; ver BuildMagnate.)

        // Compañeros POR COMPOSICIÓN (fase 5, CompanionBase retirado): Anima + SoulComposition(arquetipo) +
        // Mind + MoodState(curvas+anchors) + su componente de comportamiento propio.
        GameObject gohageneis = MakePlaceholderPerson("Gohageneis_Post", new Vector3(5f, 1f, -5f), new Color(0.95f, 0.5f, 0.2f));
        gohageneis.transform.SetParent(charactersGroup.transform);
        MoodState gohaMood = MakeCompanionCore(gohageneis, "Gohageneis");
        gohaMood.restingMoodMin = 0.65f; gohaMood.restingMoodMax = 0.9f; gohaMood.moodModMin = 0.4f; gohaMood.moodModMax = 1.2f; gohaMood.fatigueRate = 0.00008f;
        gohaMood.anchors.Add(new ThoughtAnchor("celebrate_life", 0.9f, 0.001f));
        gohaMood.anchors.Add(new ThoughtAnchor("include_everyone", 0.85f, 0.001f));
        gohaMood.anchors.Add(new ThoughtAnchor("hide_pain", 0.7f, 0.005f));
        gohaMood.anchors.Add(new ThoughtAnchor("trust_joy", 0.8f, 0.001f));
        gohageneis.AddComponent<Gohageneis>();
        gohageneis.AddComponent<CarnivoreKeeper>();   // GUARDIA: patrulla y alimenta a los carnívoros para que no cacen
        gohageneis.AddComponent<WorldCharacter>().characterName = "Gohageneis";

        GameObject goluis = MakePlaceholderPerson("Goluis_Post", new Vector3(-5f, 1f, -5f), new Color(0.4f, 0.4f, 0.55f));
        goluis.transform.SetParent(charactersGroup.transform);
        MoodState golMood = MakeCompanionCore(goluis, "Goluis");
        golMood.restingMoodMin = 0.3f; golMood.restingMoodMax = 0.65f; golMood.moodModMin = -0.3f; golMood.moodModMax = 0.7f; golMood.fatigueRate = 0.0003f;
        golMood.anchors.Add(new ThoughtAnchor("yoga_skepticism", -0.9f, 0.002f));
        golMood.anchors.Add(new ThoughtAnchor("work_hard", 0.85f, 0.001f));
        golMood.anchors.Add(new ThoughtAnchor("family_first", 0.95f, 0f));
        golMood.anchors.Add(new ThoughtAnchor("trust_people", -0.4f, 0.005f));
        goluis.AddComponent<Goluis>();
        goluis.AddComponent<WorldCharacter>().characterName = "Goluis";

        GameObject panterilia = MakePlaceholderPerson("Panterilia_Post", new Vector3(0f, 1f, -12f), new Color(0.55f, 0.25f, 0.5f));
        panterilia.transform.SetParent(charactersGroup.transform);
        MoodState pantMood = MakeCompanionCore(panterilia, "Panterilia");
        pantMood.restingMoodMin = 0.55f; pantMood.restingMoodMax = 0.8f; pantMood.moodModMin = 0.3f; pantMood.moodModMax = 1f; pantMood.stressPenalty = 0.6f; pantMood.fatigueRate = 0.00015f;
        pantMood.anchors.Add(new ThoughtAnchor("work_hard", 0.85f, 0.001f));
        pantMood.anchors.Add(new ThoughtAnchor("protect_daughter", 0.95f, 0f));
        pantMood.anchors.Add(new ThoughtAnchor("trust_nature", 0.3f, 0.01f));
        pantMood.anchors.Add(new ThoughtAnchor("chemical_reliance", 0.6f, 0.02f));
        panterilia.AddComponent<Panterilia>();
        panterilia.AddComponent<WorldCharacter>().characterName = "Panterilia";

        // Irosene — compañera motivacional (canal primario = Satisfacción). Ver docs/character-irosene.md.
        GameObject irosene = MakePlaceholderPerson("Irosene_Post", new Vector3(3f, 1f, -13f), new Color(0.90f, 0.55f, 0.40f));
        irosene.transform.SetParent(charactersGroup.transform);
        MoodState iroMood = MakeCompanionCore(irosene, "Irosene");
        iroMood.primaryChannel = MindChannel.Satisfaction; iroMood.baseRestorationRate = 0.03f;
        iroMood.restingMoodMin = 0.6f; iroMood.restingMoodMax = 0.9f; iroMood.moodModMin = 0.5f; iroMood.moodModMax = 1.2f; iroMood.stressPenalty = 0.5f; iroMood.fatigueRate = 0.0001f;
        iroMood.anchors.Add(new ThoughtAnchor("alpha_and_omega", 0.95f, 0f));
        iroMood.anchors.Add(new ThoughtAnchor("pursue_dreams", 0.9f, 0f));
        iroMood.anchors.Add(new ThoughtAnchor("love_your_people", 0.85f, 0.005f));
        iroMood.anchors.Add(new ThoughtAnchor("rebellion", 0.8f, 0.003f));
        iroMood.anchors.Add(new ThoughtAnchor("pride", 0.6f, 0.004f));
        irosene.AddComponent<Irosene>();
        irosene.AddComponent<WorldCharacter>().characterName = "Irosene";

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
                new Vector3(5f, 0f, 95f), new Vector2(40f, 38f), new Color(0.75f, 0.45f, 0.30f)),   // restaurante: cocina abierta en el CENTRO y el comedor (para 30+) alrededor

            BuildAreaBox(areasGroup.transform, "Infirmary_Area", "Enfermería", SanctuaryAreaType.Infirmary, 1,
                new Vector3(55f, 0f, 97f), new Vector2(26f, 20f), new Color(0.85f, 0.95f, 0.90f)),   // para atender a 30 personas (turistas incluidos)

            BuildAreaBox(areasGroup.transform, "YogaRoom_Area", "Sala de Yoga", SanctuaryAreaType.YogaRoom, 1,
                new Vector3(105f, 0f, 35f), new Vector2(18f, 15f), new Color(0.75f, 0.65f, 0.85f)),

            BuildAreaBox(areasGroup.transform, "VehicleWorkshop_Area", "Mecánica", SanctuaryAreaType.VehicleWorkshop, 2,
                new Vector3(105f, 0f, -25f), new Vector2(10f, 9f), new Color(0.55f, 0.55f, 0.60f)),

            BuildAreaBox(areasGroup.transform, "TextileStudio_Area", "Estudio Textil", SanctuaryAreaType.TextileStudio, 2,
                new Vector3(55f, 0f, -55f), new Vector2(12f, 10f), new Color(0.85f, 0.60f, 0.65f)),   // más grande: telares, rueca, tintes…

            // LABORATORIO DE CARNE CULTIVADA (área propia, nivel 3; docs/carne-cultivada.md): carne para los depredadores
            // sin matar animales. En el casquete Este, cerca del territorio de los osos (su comedero).
            BuildAreaBox(areasGroup.transform, "CulturedMeatLab_Area", "Carne Cultivada", SanctuaryAreaType.CulturedMeatLab, 3,
                new Vector3(118f, 0f, 6f), new Vector2(16f, 12f), new Color(0.85f, 0.7f, 0.68f)),

            // CONSTRUCCIÓN (docs/construction-simulation.md): obras y reparaciones de las áreas humanas; aquí se guardan la
            // maquinaria y el material. La dirige el esposo de Lizmibet. En el hub, al oeste del huerto.
            BuildAreaBox(areasGroup.transform, "Construction_Area", "Construcción", SanctuaryAreaType.Construction, 2,
                new Vector3(-20f, 0f, 10f), new Vector2(18f, 14f), new Color(0.85f, 0.72f, 0.4f)),
            // ENTRENAMIENTO (2 h diarias, docs/jornada-y-rotacion.md): gimnasio abierto; ballet, dojo y calistenia EN OBRAS.
            BuildAreaBox(areasGroup.transform, "Gym_Area", "Gimnasio", SanctuaryAreaType.Gym, 1,
                new Vector3(26f, 0f, 6f), new Vector2(22f, 16f), new Color(0.55f, 0.6f, 0.7f)),   // 30 máquinas (10 tipos × 3) + zona libre
            BuildAreaBox(areasGroup.transform, "Ballet_Area", "Sala de Ballet", SanctuaryAreaType.Ballet, 1,
                new Vector3(12f, 0f, 24f), new Vector2(10f, 9f), new Color(0.9f, 0.75f, 0.8f)),
            BuildAreaBox(areasGroup.transform, "MartialArts_Area", "Dojo (artes marciales)", SanctuaryAreaType.MartialArts, 1,
                new Vector3(-5f, 0f, 22f), new Vector2(10f, 9f), new Color(0.8f, 0.7f, 0.55f)),
            BuildAreaBox(areasGroup.transform, "Calisthenics_Area", "Calistenia", SanctuaryAreaType.Calisthenics, 1,
                new Vector3(-28f, 0f, -12f), new Vector2(10f, 9f), new Color(0.65f, 0.75f, 0.6f)),

            // CRÍA (docs/cria-simulation.md): el corazón del santuario, una sala de crías del hielo (recintos con crías de
            // verdad, cocina de leche, almacén). En el sitio que dejó el Estudio Textil al subir sobre la Construcción.
            BuildAreaBox(areasGroup.transform, "CubCare_Area", "Cría", SanctuaryAreaType.CubCare, 1,
                new Vector3(55f, 0f, -55f), new Vector2(26f, 20f), new Color(0.8f, 0.72f, 0.55f)),

            BuildAreaBox(areasGroup.transform, "Garden_Area", "Huerto", SanctuaryAreaType.Garden, 2,
                new Vector3(5f, 0f, -30f), new Vector2(24f, 20f), new Color(0.50f, 0.70f, 0.40f)),
        };
        // ÁREAS ESPACIADAS (~50 m entre vecinas, antes 25): entre ellas caben árboles y animales sin cortar el paso.
        // El HUERTO es más grande y tiene cultivos de verdad (flora modificada para el restaurante).
        PlantGarden(areas[areas.Count - 1].transform);
        // Todo lo construido, ELEVADO (6 m, sobre columnas, con escalera): el suelo queda para la naturaleza. El huerto no.
        // UNA SOLA ENFERMERÍA para personas y animales (docs/area-progression.md: "Una sola Enfermería que cuida a TODOS"):
        // ya no hay veterinaria aparte; la mesa veterinaria y el paciente lobo están dentro de la enfermería.
        Vector2[] sizes = { new Vector2(40f, 38f), new Vector2(26f, 20f), new Vector2(18f, 15f), new Vector2(10f, 9f), new Vector2(12f, 10f), new Vector2(16f, 12f), new Vector2(18f, 14f), new Vector2(22f, 16f), new Vector2(10f, 9f), new Vector2(10f, 9f), new Vector2(10f, 9f), new Vector2(26f, 20f) };
        // PISOS (docs/construcciones-elevadas.md §Pisos): el BALLET es el piso de arriba de la sala de YOGA y el DOJO el del
        // GIMNASIO (10 m más arriba, sin tapar la vista): una base, dos salas, sin plataforma ni puente aparte. Igual los
        // TALLERES: el ESTUDIO TEXTIL sobre la CONSTRUCCIÓN. (La Mecánica, de 10 × 9, es pequeña para cargar otro piso.)
        const int Ballet = 8, Dojo = 9, Textile = 4;
        var upper = new Dictionary<int, int> { { Ballet, 2 }, { Dojo, 7 }, { Textile, 6 } };   // piso de arriba → el de abajo
        for (int i = 0; i < sizes.Length; i++) if (!upper.ContainsKey(i)) ElevateArea(areas[i], sizes[i], ElevatedHeight);
        foreach (var kv in upper) sizes[kv.Key] = StackOn(areas[kv.Key].transform, areas[kv.Value].transform, sizes[kv.Value]);
        ElevateArea(areas[areas.Count - 1], new Vector2(24f, 20f), ElevatedHeight);   // el Huerto (área) también, con sus bancales
        UpperGarden(areas[areas.Count - 1].transform, new Vector2(24f, 20f));         // y encima, a 10 m, el segundo huerto
        FurnishYoga(areas[2].transform, sizes[2]);
        FurnishTextile(areas[4].transform, sizes[4]);   // el estudio textil de Irosene: telares, rueca, lanas, tintes, corte y costura
        FurnishConstruction(areas[6].transform, sizes[6]);   // maquinaria, material, herramientas y taller de obra
        FurnishGym(areas[7].transform, sizes[7]);                                                   // gimnasio abierto
        FurnishCria(areas[11].transform, sizes[11]);                                                // la sala de crías
        MakeConstructionSite(areas[8].transform, sizes[8], "Sala de Ballet", 5);                     // en obras: material de un día a pie de obra
        MakeConstructionSite(areas[9].transform, sizes[9], "Dojo (artes marciales)", 6);
        MakeConstructionSite(areas[10].transform, sizes[10], "Parque de calistenia", 4);
        // ASEOS en cada área humana (el restaurante y la enfermería los ponen al amueblarse).
        Aseos(areas[2].transform, new Vector3(-sizes[2].x * 0.5f + 0.9f, 0.1f, 0f), 90f);       // sala de yoga (en el lateral: las esquinas son de las estatuas)
        Aseos(areas[3].transform, new Vector3(-sizes[3].x * 0.5f + 0.2f + 1.9f, 0.1f, sizes[3].y * 0.5f - 0.9f), 180f, 1);    // mecánica
        Aseos(areas[4].transform, new Vector3(4.4f, 0.1f, -sizes[4].y * 0.5f + 0.9f), 0f, 1);    // textil (al fondo, junto a los ovillos: delante, a −X, llega la escalera del piso)
        Aseos(areas[5].transform, new Vector3(sizes[5].x * 0.5f - 0.2f - 1.9f, 0.1f, -sizes[5].y * 0.5f + 0.9f), 0f, 1);      // laboratorio
        Aseos(areas[6].transform, new Vector3(sizes[6].x * 0.5f - 0.2f - 1.9f, 0.1f, sizes[6].y * 0.5f - 0.9f), 180f, 1);    // construcción
        // Comedero de los depredadores junto a los osos: ahí deja el laboratorio las raciones de carne cultivada.
        GameObject feeder = new GameObject("Comedero_depredadores"); feeder.transform.SetParent(areasGroup.transform, false); feeder.transform.position = new Vector3(126f, 0f, -6f);
        KitchenProps.C(feeder.transform, new Vector3(0f, 0.08f, 0f), new Vector3(1.6f, 0.16f, 1.6f), KitchenProps.DarkSteel, 0f, "Bandeja");
        WorldLabel fwl = feeder.AddComponent<WorldLabel>(); fwl.text = "Comedero (carne cultivada)"; fwl.height = 1f;
        CulturedMeatLab.Build(areas[5].transform, sizes[5], feeder.transform);
        // PUENTES de 8 m entre plataformas cercanas y una PISTA de alfombras voladoras por grupo (docs/transporte-alfombras.md).
        // (Los pisos de arriba no llevan puente propio: se llega por la escalera desde el de abajo.)
        var ground = new List<SanctuaryArea>(); var groundSizes = new List<Vector2>();
        for (int i = 0; i < areas.Count; i++)
        {
            if (upper.ContainsKey(i)) continue;
            ground.Add(areas[i]); groundSizes.Add(i < sizes.Length ? sizes[i] : new Vector2(24f, 20f));
        }
        BuildBridgesAndPads(areasGroup.transform, ground.ToArray(), groundSizes.ToArray());
        // ENERGÍA SOLAR: aquí no hay gas. Una fila de paneles (sobre postes) junto a cada edificio y un campo en el hub.
        for (int i = 0; i < sizes.Length; i++)
            if (!upper.ContainsKey(i))
            SolarArray.Build(areasGroup.transform, $"Paneles_{areas[i].displayName}", areas[i].transform.TransformPoint(new Vector3(-(sizes[i].x * 0.5f + 4f), -ElevatedHeight, 0f)), areas[i].transform.eulerAngles.y + 90f, 6);
        for (int r = 0; r < 3; r++) SolarArray.Build(areasGroup.transform, $"CampoSolar_{r + 1}", new Vector3(26f, 0f, -26f - r * 4f), 0f, 8);

        return areas.ToArray();
    }

    // HUERTOS POR GREMIO repartidos por el mapa (docs/huertos-y-cultivos.md): en vez de un único terreno, varias
    // parcelas de policultivo —cada una con plantas que se ayudan— cerca del agua o de las áreas que las usan. Las
    // plantas las genera GardenPlot al arrancar; aquí se pone el suelo (con collider, para el NavMesh) y el cartel.
    static void PlantGarden(Transform garden)
    {
        // Hortalizas asociadas dentro del área del Huerto (delante de la máquina de virtualización).
        MakePlot(garden, GardenPlot.Guild.Hortalizas, garden.TransformPoint(new Vector3(0f, 0.1f, -3.5f)), new Vector2(20f, 12f), 11, false);
        Transform parent = garden.parent;
        MakePlot(parent, GardenPlot.Guild.Milpa,            new Vector3(-45f, 0f, -75f), new Vector2(20f, 20f), 12, true);
        MakePlot(parent, GardenPlot.Guild.ChacraAndina,     new Vector3(-10f, 0f, -118f), new Vector2(18f, 14f), 13, true);
        MakePlot(parent, GardenPlot.Guild.Arrozal,          new Vector3(-60f, 0f, -15f),   new Vector2(22f, 16f), 14, true);
        // (La ROTACIÓN ya no tiene base propia: es el piso de arriba del Huerto, ver UpperGarden.)
        MakePlot(parent, GardenPlot.Guild.BosqueComestible, new Vector3(-78f, 0f, 45f),   new Vector2(30f, 26f), 16, true);
        MakePlot(parent, GardenPlot.Guild.Bambusal,         new Vector3(-42f, 0f, 0f),    new Vector2(16f, 12f), 17, true);   // guadua y cáñamo: el material de Construcción
    }

    // SEGUNDO HUERTO en la misma base: el de ROTACIÓN, 10 m sobre el Huerto (dos huertos, una base, sin puente aparte).
    static void UpperGarden(Transform garden, Vector2 gardenSize)
    {
        Vector2 size = new Vector2(gardenSize.x - 2f, gardenSize.y - SetBack - 2f);   // + 2 m de suelo alrededor = el piso
        GameObject go = MakePlot(garden.parent, GardenPlot.Guild.Rotacion, garden.TransformPoint(new Vector3(0f, 0f, -SetBack * 0.5f)), size, 15, true, upperOf: garden);
        go.transform.rotation = garden.rotation;
    }

    static GameObject MakePlot(Transform parent, GardenPlot.Guild guild, Vector3 center, Vector2 size, int seed, bool withSoil, Transform upperOf = null)
    {
        GameObject go = new GameObject($"Huerto_{guild}");
        go.transform.SetParent(parent, true);
        go.transform.position = center;
        GardenPlot plot = go.AddComponent<GardenPlot>();
        plot.guild = guild; plot.size = size; plot.seed = seed;
        if (withSoil)
        {
            GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            soil.name = "Suelo_Huerto";
            soil.transform.SetParent(go.transform, false);
            soil.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            soil.transform.localScale = new Vector3(size.x + 2f, 0.06f, size.y + 2f);
            soil.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"Huerto_{guild}_Suelo_MAT",
                guild == GardenPlot.Guild.Arrozal ? new Color(0.62f, 0.78f, 0.84f) : new Color(0.70f, 0.64f, 0.58f));
        }
        CreateAreaLabel(go.transform, GardenPlot.Title(guild), new Vector3(0f, 2.2f, -size.y * 0.5f - 1f));
        // Los CULTIVOS también van ELEVADOS (docs/construcciones-elevadas.md): el huerto no deja el suelo del bosque en
        // sombra ni lo pisa; abajo crece el musgo y pasan los animales.
        if (upperOf != null) Elevate(go.transform, new Vector2(size.x + 2f, size.y + 2f), UpperFloorGap, groundBelow: false, ladderX: -(size.x + 2f) * 0.3f);
        else if (withSoil) Elevate(go.transform, new Vector2(size.x + 2f, size.y + 2f), ElevatedHeight);
        return go;
    }


    // CONSTRUCCIONES ELEVADAS (docs/construcciones-elevadas.md): el santuario no ocupa el suelo que pertenece a la
    // naturaleza. Cada área se sube a una PLATAFORMA a `height` m sobre columnas finas (bajo ella pasan animales y crece
    // la flora) y se sube por una ESCALERA VERTICAL (con su enlace del NavMesh) desde el lado abierto (+Z).
    // Barandilla de 1 m alrededor (el contorno bajo de antes no basta a 6 m de altura).
    public const float ElevatedHeight = 12f;   // a 12 m las áreas humanas no tienen al bosque en sombra ni lo molestan con su ruido

    static void ElevateArea(SanctuaryArea a, Vector2 size, float h) => Elevate(a.transform, size, h);

    /// <summary>Sube cualquier cosa (un área, un huerto) a una plataforma sobre columnas, con barandilla y escalera vertical.
    /// <paramref name="groundBelow"/> = false para un PISO SUPERIOR (debajo hay otra plataforma, no el suelo: sin colchón de
    /// musgo); <paramref name="ladderX"/> desplaza la escalera (y el hueco de la barandilla) a lo largo del lado +Z.</summary>
    static void Elevate(Transform t, Vector2 size, float h, bool groundBelow = true, float ladderX = 0f)
    {
        t.position += Vector3.up * h;
        Color wood = new Color(0.62f, 0.52f, 0.42f), woodDark = new Color(0.48f, 0.4f, 0.33f);
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        Transform s = new GameObject("Estructura_Elevada").transform; s.SetParent(t, false);

        // Losa bajo el suelo del área + columnas (esquinas y cada ~5 m).
        KitchenProps.C(s, new Vector3(0f, -0.18f, 0f), new Vector3(size.x + 0.4f, 0.3f, size.y + 0.4f), woodDark, 0f, "Losa");
        for (float x = -hx + 0.4f; x <= hx - 0.39f; x += Mathf.Max(3f, (size.x - 0.8f) / Mathf.Ceil((size.x - 0.8f) / 5f)))
            for (int sz = -1; sz <= 1; sz += 2)
                Solid(s, new Vector3(x, -h * 0.5f - 0.15f, sz * (hz - 0.4f)), new Vector3(0.4f, h, 0.4f), wood, "Columna");
        Solid(s, new Vector3(-hx + 0.4f, -h * 0.5f - 0.15f, 0f), new Vector3(0.4f, h, 0.4f), wood, "Columna");
        Solid(s, new Vector3(hx - 0.4f, -h * 0.5f - 0.15f, 0f), new Vector3(0.4f, h, 0.4f), wood, "Columna");

        // Barandilla (1 m) en los tres lados cerrados y a ambos lados de la escalera.
        float rail = 1f;
        void Rail(Vector3 from, Vector3 to)
        {
            Vector3 mid = (from + to) * 0.5f, d = to - from;
            Transform r = Solid(s, mid + Vector3.up * rail, new Vector3(Mathf.Abs(d.x) + 0.08f, 0.08f, Mathf.Abs(d.z) + 0.08f), wood, "Barandilla");
            Transform low = Solid(s, mid + Vector3.up * 2f, new Vector3(Mathf.Abs(d.x) + 0.06f, 4f, Mathf.Abs(d.z) + 0.06f), wood, "Barandilla_Barrera");   // 4 m (ni saltando): no se salta
            low.GetComponent<Renderer>().enabled = false;   // barrera invisible: nadie cae, la vista no se tapa
            int posts = Mathf.Max(2, Mathf.RoundToInt(d.magnitude / 1.5f));
            for (int i = 0; i <= posts; i++) KitchenProps.C(s, Vector3.Lerp(from, to, i / (float)posts) + Vector3.up * rail * 0.5f, new Vector3(0.07f, rail, 0.07f), woodDark, 0f, "Balaustre");
        }
        Rail(new Vector3(-hx, 0f, -hz), new Vector3(hx, 0f, -hz));
        Rail(new Vector3(-hx, 0f, -hz), new Vector3(-hx, 0f, hz));
        Rail(new Vector3(hx, 0f, -hz), new Vector3(hx, 0f, hz));
        Rail(new Vector3(-hx, 0f, hz), new Vector3(ladderX - 0.6f, 0f, hz));   // hueco = donde llega la escalera vertical
        Rail(new Vector3(ladderX + 0.6f, 0f, hz), new Vector3(hx, 0f, hz));

        // COLCHÓN DE MUSGO bajo la plataforma y alrededor (+6 m): si alguien cae, amortigua el golpe; y es una planta más.
        if (groundBelow) SoftGround.Plant(s, t.TransformPoint(new Vector3(0f, -h - 0.1f, 0f)), new Vector2(size.x + 12f, size.y + 12f), size.GetHashCode());

        // ESCALERA VERTICAL por el lado abierto (+Z): se sube con F / Espacio / `climb`; la IA, por su enlace del NavMesh.
        Ladder.Build(s, new Vector3(ladderX, -h, hz + Ladder.Gap), 180f, h, wood, woodDark);
    }

    static Transform Solid(Transform parent, Vector3 local, Vector3 size, Color c, string name)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
        return g.transform;
    }

    // SALA DE YOGA para 30: esterillas en 6×5 con su espacio, tarima del maestro delante, cojines y campana.
    static void FurnishYoga(Transform area, Vector2 size)
    {
        // SALA CÉNTRICA: la PLATAFORMA en el centro (se ve desde todas partes), las 30 esterillas en dos anillos alrededor
        // MIRANDO a la plataforma, y las cuatro estatuas en las esquinas.
        Transform r = new GameObject("Esterillas").transform; r.SetParent(area, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        Color[] mats = { new Color(0.72f, 0.62f, 0.86f), new Color(0.62f, 0.78f, 0.86f), new Color(0.86f, 0.7f, 0.72f), new Color(0.7f, 0.84f, 0.7f) };
        int k = 0;
        (int n, float rad)[] rings = { (12, 3.4f), (18, 5.4f) };
        foreach (var ring in rings)
            for (int i = 0; i < ring.n; i++, k++)
            {
                float a = (i + (ring.n == 18 ? 0.5f : 0f)) / ring.n * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * ring.rad, 0.012f, Mathf.Sin(a) * ring.rad * 0.82f);   // algo achatado: la sala es más ancha que honda
                float yaw = Mathf.Atan2(-p.x, -p.z) * Mathf.Rad2Deg;   // de cara al centro
                Transform mat = KitchenProps.C(r, p, new Vector3(0.7f, 0.02f, 1.8f), mats[k % mats.Length], yaw, $"Esterilla_{k + 1}");
                KitchenProps.C(r, p + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0.45f, 0.06f, -0.75f), new Vector3(0.3f, 0.12f, 0.3f), new Color(0.9f, 0.85f, 0.75f), yaw + 45f, "Cojin");
            }
        KitchenProps.C(r, new Vector3(0f, 0.2f, 0f), new Vector3(2.6f, 0.4f, 2.6f), new Color(0.6f, 0.5f, 0.42f), 0f, "Tarima del maestro");
        KitchenProps.C(r, new Vector3(0f, 0.41f, 0f), new Vector3(0.8f, 0.02f, 2f), new Color(0.95f, 0.88f, 0.6f), 0f, "Esterilla del maestro");
        KitchenProps.C(r, new Vector3(1f, 0.5f, 0.9f), new Vector3(0.25f, 0.2f, 0.25f), new Color(0.85f, 0.7f, 0.35f), 0f, "Cuenco tibetano");
        YogaStatues.BuildCorners(r, size.x * 0.5f, size.y * 0.5f);   // Patanjali, Buda, Aum y Shiva en las esquinas
    }

    // Contorno, no muro: 2,4 m tapaban la vista del área desde fuera (se dibuja el espacio sin ocultarlo).
    const float AreaWallHeight = 0.5f;
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
        panelGO.GetComponent<CanvasGroup>().alpha = 0f;   // oculto hasta Show(): si no, «¿Empezar turno?» se ve en la escena fuera de Play

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

    internal enum MissionKind { KitchenChannel, GardenChannel, Channel, Heal, Yoga }

    // MICROCOSMOS POR ÁREA: cada área tiene su propia escena de microcosmos (no se juega dentro del mesocosmos agrandando
    // el decorado). Se entra con el hechizo de virtualización: desde la máquina del área, o con magia (consola `virtualize`).
    internal static readonly (string areaGo, string display, string key, Color tint, MissionKind kind)[] MicroAreas =
    {
        ("Kitchen_Area",          "Cocina",         "Cocina",      new Color(0.75f, 0.45f, 0.30f), MissionKind.KitchenChannel),   // la era del fuego: las reencarnaciones
        ("YogaRoom_Area",         "Sala de Yoga",   "SalaYoga",    new Color(0.75f, 0.65f, 0.85f), MissionKind.Yoga),
        ("Garden_Area",           "Huerto",         "Huerto",      new Color(0.50f, 0.70f, 0.40f), MissionKind.GardenChannel),
        ("Infirmary_Area",        "Enfermería",     "Enfermeria",  new Color(0.85f, 0.95f, 0.90f), MissionKind.Heal),
        ("TextileStudio_Area",    "Estudio Textil", "Textil",      new Color(0.85f, 0.60f, 0.65f), MissionKind.Channel),
        ("VehicleWorkshop_Area",  "Mecánica",       "Mecanica",    new Color(0.55f, 0.55f, 0.60f), MissionKind.Channel),
    };
    internal static string MicroSceneName(string key) => "Microcosmos_" + key;

    static void BuildMeditationContent(Transform parent)
    {
        foreach (var m in MicroAreas)   // cada área → su escena de microcosmos
            WireMeditationArea(parent, m.areaGo, m.display, m.tint, m.kind, MicroSceneName(m.key));
        // Cada máquina solo tiene lo de SU área: las áreas con misiones y sin microcosmos también tienen la suya (solo virtualización).
        WireMeditationArea(parent, "CulturedMeatLab_Area", "Carne Cultivada", new Color(0.85f, 0.7f, 0.68f), MissionKind.Channel, null, virtualOnly: true);
        WireMeditationArea(parent, "Gym_Area", "Gimnasio", new Color(0.55f, 0.6f, 0.7f), MissionKind.Channel, null, virtualOnly: true);
        WireMeditationArea(parent, "Construction_Area", "Construcción", new Color(0.85f, 0.72f, 0.4f), MissionKind.Channel, null, virtualOnly: true);
        WireMeditationArea(parent, "CubCare_Area", "Cría", new Color(0.8f, 0.72f, 0.55f), MissionKind.Channel, null, virtualOnly: true);

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
                                    MissionKind kind, string mobSceneName = null, bool virtualOnly = false)
    {
        GameObject area = GameObject.Find(areaName);
        if (area == null)
        {
            Debug.LogWarning($"[SampleSceneBuilder] {areaName} no encontrada — Microcosmos omitido para esa área.");
            return;
        }

        // Máquina de virtualización en una ESQUINA del área (junto a la entrada, +X +Z), mirando al centro: no corta el paso.
        GameObject machineGO = new GameObject($"{areaName}_VirtualizationMachine");
        machineGO.transform.SetParent(area.transform, worldPositionStays: false);
        Transform mat = area.transform.Find("Mat");
        Vector2 sz = mat != null ? new Vector2(mat.localScale.x, mat.localScale.z) : new Vector2(10f, 10f);
        machineGO.transform.localPosition = new Vector3(sz.x * 0.5f - 1.7f, 1f, sz.y * 0.5f - 1.9f);
        machineGO.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);   // la puerta (−Z del tanque) mira al centro del área
        BoxCollider bc = machineGO.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(3f, 3f, 1.5f);
        VirtualizationMachine machine = machineGO.AddComponent<VirtualizationMachine>();
        machine.areaName = displayName;
        VirtualizationTank.Build(machineGO.transform);   // la máquina ES un tanque de agua (máscara + sumergirse)
        if (virtualOnly) return;   // solo VIRTUALIZACIÓN (misiones de trabajo del área): esta área aún no tiene microcosmos

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

    internal static MobMission[] BuildAreaMissions(Transform parent, Transform areaT, string displayName, Color tint, MissionKind kind)
    {
        if (kind == MissionKind.Yoga)
        {
            MobMission visualize = MakeMissionGO(areaT, "Mission_Visualizar", "Visualizar postura",
                "Huye de los pensamientos hasta disolverlos.", MissionCategory.Visualization);
            var pv = visualize.gameObject.AddComponent<PostureVisualizationMission>();
            pv.mission = visualize; pv.targetCount = 5; pv.reward.observationGain = 0.25f; pv.reward.yogaXp = 30f;

            MobMission form = MakeMissionGO(areaT, "Mission_Formar", "Formar posturas",
                "Persigue y sostén cada postura.", MissionCategory.Visualization);
            var af = form.gameObject.AddComponent<AsanaFormationMission>();
            af.mission = form; af.targetCount = 3; af.reward.observationGain = 0.25f; af.reward.yogaXp = 40f;

            MobMission root = MakeMissionGO(areaT, "Mission_Raiz", "Buscar la raíz",
                "No puedes huir; ve a su raíz.", MissionCategory.RootInquiry);
            var ri = root.gameObject.AddComponent<RootInquiryMission>();
            ri.mission = root; ri.targetCount = 3; ri.reward.observationGain = 0.3f; ri.reward.yogaXp = 50f;

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

        // HUERTO: ya no es «limpiar la plaga» (una bola que se resolvía sola al pasar cerca). Llegan animalitos con hambre a
        // por los cultivos y se les da de comer (F o «alimentar») para que no muerdan las plantas. docs/garden.
        if (kind == MissionKind.GardenChannel)
        {
            MobMission feed = MakeMissionGO(areaT, "Mission_Alimentar", "Alimentar a los visitantes",
                "Un animal hambriento va a por el cultivo: dale de comer (F) antes de que lo muerda.", MissionCategory.LovingKindness);
            var fv = feed.gameObject.AddComponent<FeedVisitorMission>();
            fv.mission = feed; fv.targetCount = 4; fv.spawnInterval = 5f; fv.minSpawnDistance = 8f; fv.spawnRadius = 14f;
            fv.visitorPrefabs = new[] { LoadAnimalPrefab("Bunny"), LoadAnimalPrefab("Deer") }.Where(p => p != null).ToArray();   // animales reales del santuario
            fv.reward.observationGain = 0.2f; fv.reward.coinReward = 5;
            return new[] { feed };
        }

        // Variantes de Channel (Kitchen / Textile / Workshop)
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

    internal static MobMission MakeMissionGO(Transform areaT, string goName, string missionName, string desc, MissionCategory cat)
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
        portal.name = "WorldExitPortal";
        portal.transform.SetParent(hub.transform);
        portal.transform.localPosition = new Vector3(0f, 1f, 3f);
        portal.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = MakeMaterial("WorldExitPortal_MAT", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<WorldExitPortal>();

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
