using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ESCENAS DE PRUEBA (Assets/Scenes/Pruebas; tabla completa en docs/escenas-de-prueba.md). Berón 2026-11-01: «las escenas de
/// prueba de yoga parece que cargan un santuario entero, deberían cargar solo la sala de yoga; enfermería y cocina de igual
/// manera… y deberían estar vinculadas a los pisos actuales». Antes cada escena era una copia de SampleScene con TODAS las
/// torres (BuildAreaStructures) más su sandbox, generada a mano con un menú: las de la carpeta eran del 21-10, con el santuario
/// de antes de las torres-árbol.
///
/// Dos clases de escena, una sola tabla (<see cref="TestScenes"/>) y un solo constructor:
///  · DE ÁREA (<see cref="TestSceneSpec.keep"/>): se COPIA el SampleScene recién construido (los mismos modelos, muebles y
///    personajes que el juego) y se PODA: se queda el piso del área (y todo lo que está físicamente en él: la sala, su anillo,
///    sus muebles aunque cuelguen de otro objeto), los personajes que pide la prueba, el suelo y los sistemas sin cuerpo (reloj,
///    UI, jornada, pruebas…). Se regeneran solas al final de «Build Sample Scene Blockout», así que no se desfasan.
///  · DE SANDBOX (sin keep): suelo + sistemas + su sandbox; sin torres (los sandboxes no usan ninguna área).
/// Cada escena corre con «test» solo los grupos de su área (<see cref="ColdSanctuary.TestRunner.onlyGroups"/>).
/// </summary>
public static partial class SampleSceneBuilder
{
    const string TestScenesFolder = "Assets/Scenes/Pruebas";
    const string MainScenePath = "Assets/Scenes/SampleScene.unity";

    /// <summary>Si «Build Sample Scene Blockout» regenera también las escenas de prueba de área (unos 2 min más).</summary>
    public static bool AlsoAreaTestScenes = true;

    class TestSceneSpec
    {
        public string scene, purpose;
        public string[] keep;                                   // pisos (objetos del santuario) que se quedan; null = escena de sandbox
        public string[] drop = new string[0];                   // hijos de esos pisos que no van (otras plantas del área)
        public string[] people = new string[0];                 // personajes (Anima) que se quedan; los que no estén en el piso se traen
        public int[] groups = new int[0];                       // grupos de prueba de «test» (vacío: la escena no lleva pruebas)
        public System.Action<Transform>[] build = new System.Action<Transform>[0];   // sandboxes que se añaden
        public System.Func<Vector3?> playerAt;                  // dónde empieza Kushal (null: en el centro del piso)
        public bool IsArea => keep != null;
    }

    // Personal del santuario (WorkdayDirector.staff sin el jugador): la clase de yoga y el servicio del restaurante lo toman de aquí.
    static readonly string[] Staff = { "Linduarda", "Lizmibet", "Goluis", "Panterilia", "Gohageneis", "Irosene", "JefeConstruccion", "Andrea", "Nesvitor", "Amykhuz", "Karen", "Alicita", "Ducristin", "Josedrilo", "Maite", "Shalva", "Adaguchi", "Gallarle", "Julian", "Fausto", "David", "Adonis", "Yulo", "Lale", "Ivelyna", "Luciana", "Malena", "Consol", "Fernando" };
    static readonly string[] YogaSessions = { "Upa-yoga de cuello", "Yoga Namaskar", "Saludo al Sol", "Ashtanga Primaria", YogaClass.DeepSession };

    static readonly TestSceneSpec[] TestScenes =
    {
        // ── DE ÁREA: un piso del santuario actual ──
        new TestSceneSpec { scene = "Prueba_Yoga", keep = new[] { "YogaRoom_Area" },
            purpose = "Pisos de yoga (Hatha y Ashtanga, salas occidental y tradicional con su kund): las clases (Josedrilo, Hatha; Consol, Ashtanga; en la tarima, y cada practicante en su esterilla), la iniciación de upa-yoga de cuello y el minijuego de upa-yoga por teclas",
            people = new[] { "Josedrilo", "Consol", "Andrea", "Panterilia", "Gohageneis", "Irosene", "Goluis", "Lizmibet", "Magnate" },
            groups = new[] { 15, 42, 51, 64 }, build = new System.Action<Transform>[] { BuildUpaYogaSandbox } },
        new TestSceneSpec { scene = "Prueba_YogaSesiones", keep = new[] { "YogaRoom_Area" },
            purpose = "Sesiones de yoga en bucle, una por esterilla (Upa-yoga de cuello, Yoga Namaskar, Saludo al Sol, Ashtanga primaria), para revisar cada postura",
            build = new System.Action<Transform>[] { BuildYogaSessionMats } },
        new TestSceneSpec { scene = "Prueba_Enfermeria", keep = new[] { "Infirmary_Area" }, drop = new[] { "Planta_hospitalizacion", "Planta_veterinaria" },
            purpose = "Enfermería (planta de urgencias del hospital): Linduarda y Liz analizan y vacunan al recién llegado y al lobo paciente",
            people = new[] { "Linduarda", "Lizmibet", "Lobo_paciente", "Magnate" } },
        new TestSceneSpec { scene = "Prueba_Cocina", keep = new[] { "Kitchen_Area" },
            purpose = "Cocina y restaurante (piso del restaurante: cocina abierta y comedor en su anillo): cocinar platillos para el stock y la iniciación de la cocina",
            people = Staff.Concat(new[] { "Magnate" }).ToArray(), groups = new[] { 41 } },   // (el servicio con turistas, 45, necesita la torre: llegan a su pie, suben en alfombra y nadan al anillo; corre en SampleScene)

        new TestSceneSpec { scene = KushalMaskSceneTest.Scene, keep = new[] { TreeSpec.RootName },
            purpose = "Máscaras de los edificios-árbol: Kushal empieza bajo el agua del tronco del Árbol de los Campos sin máscara, nada a la estación más cercana y se la pone (oxígeno ilimitado)",
            groups = new[] { 72 }, playerAt = () => TreeSpec.Site + new Vector3(-3f, 33f, 4f) },

        // ── DE SANDBOX: suelo + sistemas + el sandbox (ningún área) ──
        new TestSceneSpec { scene = "Prueba_CocinaPrototipos", purpose = "Prototipos de la cocina en el suelo: manchas y limpieza (paso A), paseo guiado y desayuno (paso B), motor de virtualización y descomposición",
            build = new System.Action<Transform>[] { BuildKitchenSandbox, BuildKitchenOnboarding, BuildVirtualizationSandbox, BuildDecompositionSandbox } },
        new TestSceneSpec { scene = "Prueba_TorreGuardia", purpose = "El árbol gigante y los puestos de patrulla (la sala de guardia va en la torre de Servicios)",
            build = new System.Action<Transform>[] { BuildGiantTree }, playerAt = () => new Vector3(-38f, 1.2f, 65f) },
        new TestSceneSpec { scene = "Prueba_Farming", purpose = "Farming no violento: jugar con criaturas baja su tensión → recursos, XP y comida", build = new System.Action<Transform>[] { BuildFarmingSandbox } },
        new TestSceneSpec { scene = "Prueba_Mente", purpose = "Mente: pensamientos por tono, frases y campo de pensamiento", build = new System.Action<Transform>[] { BuildMindSandbox } },
        new TestSceneSpec { scene = "Prueba_Posesion", purpose = "Posesión: ceder el mando a otro cuerpo (PossessionSpell) y volver", build = new System.Action<Transform>[] { BuildPossessionSandbox } },
        new TestSceneSpec { scene = "Prueba_Oficios", purpose = "Oficios: huerto y forja virtualizados, esfuerzo, mecánica, camión, construcción, reparto de reparaciones y cría",
            build = new System.Action<Transform>[] { BuildGardenVirtualization, BuildForgeVirtualization, BuildExertionSandbox, BuildMechanicsBeginner,
                                                     BuildTruckMaintenance, BuildConstructionBeginner, BuildDispatchDemo, BuildCriaBeginner } },
        new TestSceneSpec { scene = "Prueba_Prologo", purpose = "Prólogo: guion por beats, avisos entre planos y llevar a los débiles al refugio", build = new System.Action<Transform>[] { BuildPrologueSandbox } },
        new TestSceneSpec { scene = "Prueba_CamaraEmocion", purpose = "Cámara y emoción: tintes de pantalla por estado y la orquesta emocional", build = new System.Action<Transform>[] { BuildScreenEffectsSandbox, BuildEmotionOrchestraSandbox } },
        new TestSceneSpec { scene = "Prueba_Magia", purpose = "Magia: reservas, fuego con coste físico, demo de hechizos y walkTo universal", build = new System.Action<Transform>[] { BuildMagicSandbox, BuildSpellDemoSandbox, BuildWalkUniversalSandbox } },
        new TestSceneSpec { scene = "Prueba_Alma", purpose = "Alma: mezclas de alma y alma compartida", build = new System.Action<Transform>[] { BuildSoulBlendSandbox, BuildSharedSoulSandbox } },
        new TestSceneSpec { scene = "Prueba_Fauna", purpose = "Fauna: familias y ciclo de vida, interacciones y escenarios por especie", build = new System.Action<Transform>[] { BuildFamilyLifeTest, BuildInteractionTests, BuildSpeciesScenarios } },
    };

    /// <summary>Escenas de Pruebas que ya no se generan (fusionadas o sustituidas): se borran y salen de Build Settings.</summary>
    static readonly string[] RetiredTestScenes = { "Prueba_Hospital", "Prueba_Restaurante", "Yoga_UpaCuello", "Yoga_Namaskar", "Yoga_SaludoSol", "Yoga_Ashtanga1" };

    [MenuItem("Tools/Cold Sanctuary/Build Test Scenes (una escena por prueba)")]
    public static void BuildTestScenes() => BuildTestScenes(TestScenes);

    [MenuItem("Tools/Cold Sanctuary/Build Test Scenes de área (yoga, enfermería, cocina)")]
    public static void BuildAreaTestScenes() => BuildTestScenes(TestScenes.Where(s => s.IsArea).ToArray());

    static void BuildTestScenes(TestSceneSpec[] specs)
    {
        double t0 = EditorApplication.timeSinceStartup;
        Scene active = EditorSceneManager.GetActiveScene();
        if (active.path != MainScenePath) { Debug.LogError($"[TestScenes] Abre {MainScenePath} (las escenas de prueba salen de él)."); return; }
        if (specs.Any(s => s.IsArea) && GameObject.Find(RootName) == null) { Debug.LogError("[TestScenes] SampleScene sin construir: primero «Build Sample Scene Blockout»."); return; }
        EditorSceneManager.SaveScene(active);
        if (!AssetDatabase.IsValidFolder(TestScenesFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Pruebas");

        var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (string r in RetiredTestScenes)
        {
            string p = $"{TestScenesFolder}/{r}.unity";
            if (buildScenes.RemoveAll(s => s.path == p) > 0 || System.IO.File.Exists(p)) Debug.Log($"[TestScenes] retirada: {p}");
            AssetDatabase.DeleteAsset(p);
        }
        buildScenes.RemoveAll(s => s.path.EndsWith("/Microcosmos_SalaYoga.unity"));   // el microcosmos genérico de la sala de yoga no vuelve (lo sustituyó el río)

        int n = 0;
        foreach (TestSceneSpec spec in specs)
        {
            string path = $"{TestScenesFolder}/{spec.scene}.unity";
            try
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CopyAsset(MainScenePath, path);   // misma cámara, luz, jugador y (si es de área) el santuario recién construido
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                string what = spec.IsArea ? BuildAreaScene(spec) : BuildSandboxScene(spec);
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                if (!buildScenes.Exists(s => s.path == path)) buildScenes.Add(new EditorBuildSettingsScene(path, true));
                n++;
                Debug.Log($"[TestScenes] {path}: {what}");
            }
            catch (System.Exception e) { Debug.LogError($"[TestScenes] {spec.scene} falló: {e}"); }
        }
        EditorBuildSettings.scenes = buildScenes.ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        Debug.Log($"[TestScenes] {n} escenas de prueba en {TestScenesFolder} ({EditorApplication.timeSinceStartup - t0:0} s).");
    }

    // ── DE SANDBOX ─────────────────────────────────────────────────────────────

    static string BuildSandboxScene(TestSceneSpec spec)
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);
        GameObject root = new GameObject(RootName);
        BuildTestBase(root.transform);
        foreach (var b in spec.build)
        {
            try { b(root.transform); }
            catch (System.Exception e) { Debug.LogError($"[TestScenes] {spec.scene}: {b.Method.Name} falló: {e}"); }
        }
        DressHumanoids();
        if (spec.playerAt != null && spec.playerAt() is Vector3 at) { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p != null) p.transform.position = at; }
        BakeNavMesh();
        return $"sandbox ({string.Join(", ", spec.build.Select(b => b.Method.Name))}), sin torres.";
    }

    // Lo mínimo para que un sandbox funcione: suelo, sistemas del mundo, UI, sistemas del jugador, economía (recompensas),
    // rastros, reloj y los HUD de estado/pensamiento/nombres. Sin áreas: ningún sandbox usa una (van en el suelo, junto al hub).
    static void BuildTestBase(Transform root)
    {
        BuildGround(root);
        BuildWorldSystems(root);
        WireSanctuaryDirector(new SanctuaryArea[0]);
        BuildUI(root);
        EnsurePlayerSystems();
        BuildSanctuaryEconomy(root);
        new GameObject("TraceField_AUTO").AddComponent<TraceField>().transform.SetParent(root);
        new GameObject("Clock_AUTO").AddComponent<Clock>().transform.SetParent(root);
        new GameObject("AnimaStatusHUD_AUTO").AddComponent<AnimaStatusHUD>().transform.SetParent(root);
        new GameObject("AnimaThoughtHUD_AUTO").AddComponent<AnimaThoughtHUD>().transform.SetParent(root);
        new GameObject("AnimaNameplate_AUTO").AddComponent<AnimaNameplate>().transform.SetParent(root);
    }

    // ── DE ÁREA: podar el santuario hasta su piso ─────────────────────────────

    static string BuildAreaScene(TestSceneSpec spec)
    {
        Transform root = GameObject.Find(RootName)?.transform;
        if (root == null) throw new System.Exception("la copia del SampleScene no tiene el santuario");
        var floors = new List<Transform>();
        foreach (string k in spec.keep)
        {
            Transform f = Find(root, k);
            if (f == null) throw new System.Exception($"no hay «{k}» en el santuario");
            floors.Add(f);
        }
        // Las otras plantas del área (p. ej. hospitalización y veterinaria, hijas del hospital), fuera ya.
        foreach (Transform f in floors)
            foreach (string d in spec.drop) { Transform c = Find(f, d); if (c != null) Object.DestroyImmediate(c.gameObject); }

        // El VOLUMEN de cada piso: sus renderers (losa, sala, anillo) — todo lo que caiga dentro es de ese piso aunque cuelgue de otro objeto.
        var boxes = floors.Select(AreaFloorBox).ToList();
        bool InBox(Vector3 p) => boxes.Any(b => b.Contains(p));

        var keep = new HashSet<Transform>(floors);
        var people = new List<GameObject>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        foreach (GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (Transform t in g.GetComponentsInChildren<Transform>(true))
            {
                if (t == null || keep.Contains(t)) continue;
                if (player != null && t.IsChildOf(player.transform)) { keep.Add(player.transform); continue; }
                if (t.GetComponent<Camera>() != null || t.name == "Ground_Placeholder" || (t.TryGetComponent(out Light l) && l.type == LightType.Directional)
                    || t.GetComponent<UnityEngine.EventSystems.EventSystem>() != null) { keep.Add(t); continue; }
                if (t.GetComponent<Anima>() != null)
                {
                    if (spec.people.Contains(t.name) || (t.name.EndsWith("_Post") && spec.people.Contains(t.name.Substring(0, t.name.Length - 5)))) { keep.Add(t); people.Add(t.gameObject); }   // (los compañeros se llaman «Goluis_Post»…: WorkdayDirector.Body)
                    continue;   // el resto de animas (fauna, personal que la prueba no usa), fuera aunque esté en el piso
                }
                if (t.GetComponentInParent<Anima>() != null) continue;   // las partes de un cuerpo van con su dueño
                Renderer r = t.GetComponent<Renderer>();
                if (r != null ? InBox(r.bounds.center) : (t.GetComponent<Collider>() != null || t.GetComponent<Light>() != null) && InBox(t.position)) keep.Add(t);
            }

        // Los personajes que se quedan, a un grupo propio: colgaban de su área (SanctuaryDirector los mete en ella) y la habrían
        // dejado viva, vacía y en el aire —y al arrancar los devolvía allí: Goluis a la cocina en la escena de la sala de yoga—.
        Transform cast = new GameObject("Personajes_de_la_prueba").transform; cast.SetParent(root, false);
        foreach (GameObject g in people)
            if (g != null && !floors.Any(f => g.transform.IsChildOf(f)) && g.transform.parent != null && g.transform.parent != root)
                g.transform.SetParent(cast, true);

        int before = CountTransforms();
        var anc = new HashSet<Transform>();
        foreach (Transform k in keep) for (Transform x = k != null ? k.parent : null; x != null && anc.Add(x); x = x.parent) { }
        foreach (GameObject g in SceneManager.GetActiveScene().GetRootGameObjects()) Prune(g.transform, keep, anc);
        int removed = before - CountTransforms();
        // Un área que haya quedado (vacía) solo por colgar de ella algo del piso deja de ser área: nadie va ni es enviado allí.
        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (!floors.Any(f => a.transform.IsChildOf(f))) { try { Object.DestroyImmediate(a); } catch (System.Exception) { a.enabled = false; } }
        int fixedRefs = CompactMissingRefs();

        // Sin la JORNADA del santuario: al arrancar coloca a cada uno en el área de su primer turno y luego los lleva de un área a
        // otra (en una escena de un piso, a pisos que no están: Goluis y Liz se quedaban en el aire donde estaba la cocina y la
        // enfermería, y la clase de yoga no reunía a sus practicantes). Aquí se quedan en el piso.
        foreach (WorkdayDirector wd in Object.FindObjectsByType<WorkdayDirector>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(wd.gameObject);

        // Las pruebas: solo las de su piso (o ninguna).
        ColdSanctuary.TestRunner tr = Object.FindAnyObjectByType<ColdSanctuary.TestRunner>(FindObjectsInactive.Include);
        if (tr != null)
        {
            if (spec.groups.Length == 0) Object.DestroyImmediate(tr.gameObject);
            else
            {
                foreach (MonoBehaviour mb in tr.GetComponents<MonoBehaviour>())
                    if (mb is ITestUnit u && !spec.groups.Contains(u.Group)) Object.DestroyImmediate(mb);
                tr.onlyGroups = spec.groups;
            }
        }

        foreach (var b in spec.build)
        {
            try { b(root); }
            catch (System.Exception e) { Debug.LogError($"[TestScenes] {spec.scene}: {b.Method.Name} falló: {e}"); }
        }
        BakeNavMesh();

        // Al piso: Kushal y los personajes que estaban en otro sitio, en puntos andables del piso.
        Bounds main = boxes[0];
        var spots = WalkableSpots(main, 2.2f);
        int si = 0;
        Vector3 Next() => spots.Count > 0 ? spots[si++ % spots.Count] : main.center;
        Vector3? pAt = spec.playerAt != null ? spec.playerAt() : null;
        if (player != null) player.transform.position = (pAt ?? Next()) + Vector3.up * 1.1f;
        int moved = 0;
        foreach (GameObject g in people)
        {
            if (g == null || main.Contains(g.transform.position) || InBox(g.transform.position)) continue;
            g.transform.position = Next() + Vector3.up * 1f; moved++;
        }
        return $"piso {string.Join(" + ", spec.keep)} ({removed} objetos del santuario fuera, {fixedRefs} referencias limpiadas), " +
               $"{people.Count} personajes ({moved} traídos al piso), pruebas: {(spec.groups.Length == 0 ? "ninguna" : string.Join(", ", spec.groups))}.";
    }

    static Transform Find(Transform under, string name)
    {
        foreach (Transform t in under.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    /// <summary>El volumen del piso: la caja de sus renderers, de su losa hacia arriba (sin lo que cuelga por debajo: columnas, tornapuntas).</summary>
    static Bounds AreaFloorBox(Transform f)
    {
        Bounds b = new Bounds(f.position, Vector3.one);
        foreach (Renderer r in f.GetComponentsInChildren<Renderer>(true))
            if (r.bounds.max.y > f.position.y - 1f) b.Encapsulate(r.bounds);
        Vector3 min = b.min, max = b.max;
        min.y = f.position.y - 1.5f; max.y = Mathf.Min(max.y, f.position.y + 14f);
        b.SetMinMax(min - new Vector3(1f, 0f, 1f), max + new Vector3(1f, 0f, 1f));
        return b;
    }

    /// <summary>¿Tiene CUERPO en el mundo (malla, collider, anima, luz, partículas) o lo PUEBLA al arrancar (familias de fauna,
    /// enjambres, colmenas, generadores)? Eso se quita si no se queda; un objeto sin cuerpo es un sistema y se queda.</summary>
    static bool Body(Transform t) =>
        t.GetComponent<Renderer>() != null || t.GetComponent<Collider>() != null || t.GetComponent<Anima>() != null
        || t.GetComponent<UnityEngine.AI.NavMeshAgent>() != null || t.GetComponent<Light>() != null || t.GetComponent<ParticleSystem>() != null
        || t.GetComponent<FamilyGenerator>() != null || t.GetComponent<Generator>() != null || t.GetComponent<Respawn>() != null
        || t.GetComponent<Swarm>() != null || t.GetComponent<Hive>() != null;

    /// <summary>Quita lo que tiene cuerpo y no se queda (con todo lo que cuelga de ello); en los grupos sin cuerpo entra y quita
    /// lo que tenga cuerpo dentro, y si el grupo se queda vacío, también el grupo. Los sistemas sin cuerpo se quedan.
    /// <paramref name="anc"/>: los antepasados de lo que se queda (se entra en ellos, nunca se quitan).</summary>
    static void Prune(Transform t, HashSet<Transform> keep, HashSet<Transform> anc)
    {
        if (t == null || keep.Contains(t)) return;
        if (!anc.Contains(t) && Body(t)) { Kill(t); return; }
        var children = new List<Transform>(); foreach (Transform c in t) children.Add(c);
        foreach (Transform c in children) Prune(c, keep, anc);
        if (t != null && children.Count > 0 && !anc.Contains(t) && t.childCount == 0 && t.GetComponents<Component>().Length == 1) Kill(t);   // un grupo que se ha quedado vacío
    }
    static void Kill(Transform t)
    {
        GameObject g = t.gameObject;
        if (PrefabUtility.IsPartOfPrefabInstance(g) && !PrefabUtility.IsOutermostPrefabInstanceRoot(g)) return;   // dentro de un prefab: se va con su raíz o se queda
        Object.DestroyImmediate(g);
    }

    static int CountTransforms() => SceneManager.GetActiveScene().GetRootGameObjects().Sum(g => g.GetComponentsInChildren<Transform>(true).Length);

    /// <summary>Las listas que apuntaban a objetos podados (directores de áreas, rutas, alfombras de destino) se quedan sin los huecos.</summary>
    static int CompactMissingRefs()
    {
        int n = 0;
        foreach (GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (Component c in g.GetComponentsInChildren<Component>(true))
            {
                if (c == null || c is Transform) continue;
                var so = new SerializedObject(c);
                SerializedProperty it = so.GetIterator();
                var arrays = new List<string>();
                while (it.Next(true))
                    if (it.isArray && it.propertyType == SerializedPropertyType.Generic && it.arraySize > 0 && it.GetArrayElementAtIndex(0).propertyType == SerializedPropertyType.ObjectReference)
                        arrays.Add(it.propertyPath);
                bool changed = false;
                foreach (string path in arrays)
                {
                    SerializedProperty a = so.FindProperty(path);
                    for (int i = a.arraySize - 1; i >= 0; i--)
                    {
                        SerializedProperty e = a.GetArrayElementAtIndex(i);
                        if (e.objectReferenceValue == null && !e.objectReferenceEntityIdValue.Equals(default(EntityId)))
                        {
                            e.objectReferenceValue = null; a.DeleteArrayElementAtIndex(i); changed = true; n++;
                        }
                    }
                }
                if (changed) so.ApplyModifiedPropertiesWithoutUndo();
            }
        return n;
    }

    /// <summary>Puntos andables del piso (sobre el NavMesh recién horneado), a <paramref name="gap"/> m unos de otros, del centro afuera.</summary>
    static List<Vector3> WalkableSpots(Bounds box, float gap)
    {
        var l = new List<Vector3>();
        Vector3 c = box.center; float y = box.min.y + 1.5f;
        for (int ring = 0; ring < 8 && l.Count < 40; ring++)
            for (int i = 0; i < Mathf.Max(1, ring * 6) && l.Count < 40; i++)
            {
                float ang = i * Mathf.PI * 2f / Mathf.Max(1, ring * 6);
                Vector3 p = new Vector3(c.x + Mathf.Cos(ang) * ring * gap, y + 1f, c.z + Mathf.Sin(ang) * ring * gap);
                if (!UnityEngine.AI.NavMesh.SamplePosition(p, out UnityEngine.AI.NavMeshHit hit, 1.2f, UnityEngine.AI.NavMesh.AllAreas)) continue;
                if (Mathf.Abs(hit.position.y - (box.min.y + 1.5f)) > 1.2f) continue;   // en la losa, no en un mueble alto
                if (l.Any(q => (q - hit.position).sqrMagnitude < gap * gap * 0.8f)) continue;
                l.Add(hit.position);
            }
        return l;
    }

    // UNA SESIÓN POR ESTERILLA (antes, una escena por sesión con un demostrador en el hub): cada yogui, en su esterilla de la sala
    // de yoga del santuario, ejecuta su secuencia en bucle. Extensible: al añadir una sesión a YogaLibrary, basta añadirla a YogaSessions.
    static void BuildYogaSessionMats(Transform parent)
    {
        Transform room = GameObject.Find("YogaRoom_Area")?.transform;
        var mats = new List<Transform>();
        if (room != null)
            foreach (CarpetPad p in room.GetComponentsInChildren<CarpetPad>(true))
                if (p.name.StartsWith("Esterilla_")) mats.Add(p.transform);
        mats.Sort((a, b) => string.CompareOrdinal(a.name.PadLeft(14), b.name.PadLeft(14)));
        for (int i = 0; i < YogaSessions.Length; i++)
        {
            BuildYogaSessionScene(parent, YogaSessions[i]);
            Transform y = parent.Find($"Yoga_{YogaSessions[i]}_AUTO");
            Transform d = y != null && y.childCount > 0 ? y.GetChild(0) : null;
            if (d == null) continue;
            int m = i * Mathf.Max(1, mats.Count / YogaSessions.Length);
            if (m < mats.Count) d.SetPositionAndRotation(mats[m].position + Vector3.up * 1f, mats[m].rotation);
            else if (room != null) d.position = room.TransformPoint(new Vector3(-6f + i * 4f, 1f, -6f));
        }
    }
}
