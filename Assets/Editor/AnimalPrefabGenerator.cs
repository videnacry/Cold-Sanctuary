using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Genera un GameObject/prefab por especie a partir del FBX ya importado en
/// Assets/Animals/[Especie]/Models/ y del script de comportamiento existente
/// (WolfBehavior, FoxBehavior, etc). Complementa a AnimalModelImporter.cs, que
/// solo configura el import del modelo — este paso agrega los componentes de
/// juego (NavMeshAgent, Rigidbody, Animator, BoxCollider, PostNatalManager y
/// el behavior de la especie) y guarda el resultado como .prefab.
///
/// Uso: menú Tools > Cold Sanctuary > Generate Animal Prefabs.
/// Solo crea el prefab si no existe uno ya en esa ruta (no pisa ajustes manuales).
/// Especies sin FBX o sin script de comportamiento se saltan con un log.
/// </summary>
public static class AnimalPrefabGenerator
{
    // Carpeta de especie -> tipo de MonoBehaviour de comportamiento ya existente.
    static readonly (string species, string behaviourType)[] Species =
    {
        ("Wolf",      "WolfBehavior"),
        ("Deer",      "DeerBehavior"),
        ("Fox",       "FoxBehavior"),
        ("Malamute",  "MalamuteBehavior"),
        ("Bunny",     "BunnyBehavior"),
        ("PolarBear", "BearBehaviour"),
        ("Seal",      "SealBehavior"),
        ("Whale",     "WhaleBehavior"),
    };

    const string AnimalsRoot = "Assets/Animals";

    [MenuItem("Tools/Cold Sanctuary/Generate Animal Prefabs")]
    public static void GenerateAll()
    {
        int created = 0, skipped = 0;
        foreach ((string species, string behaviourType) in Species)
        {
            string result = GenerateOne(species, behaviourType);
            if (result == null) skipped++;
            else created++;
        }
        Debug.Log($"[AnimalPrefabGenerator] Listo — {created} prefab(s) creados, {skipped} especie(s) saltadas (ver log arriba).");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static string GenerateOne(string species, string behaviourTypeName)
    {
        string modelsDir = $"{AnimalsRoot}/{species}/Models";
        string prefabPath = $"{AnimalsRoot}/{species}/{species}.prefab";

        if (File.Exists(prefabPath))
        {
            Debug.Log($"[AnimalPrefabGenerator] {species}: ya existe {prefabPath}, se omite.");
            return null;
        }

        string fbxPath = FindFbx(modelsDir, species);
        if (fbxPath == null)
        {
            Debug.LogWarning($"[AnimalPrefabGenerator] {species}: no se encontro ningun .fbx en {modelsDir}/, se omite.");
            return null;
        }

        System.Type behaviourType = System.Type.GetType(behaviourTypeName)
            ?? System.AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(behaviourTypeName))
                .FirstOrDefault(t => t != null);
        if (behaviourType == null)
        {
            Debug.LogWarning($"[AnimalPrefabGenerator] {species}: no se encontro el tipo {behaviourTypeName}, se omite.");
            return null;
        }

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbxAsset == null)
        {
            Debug.LogWarning($"[AnimalPrefabGenerator] {species}: no se pudo cargar el asset en {fbxPath}, se omite.");
            return null;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
        instance.name = species;

        if (instance.GetComponent<Rigidbody>() == null)
            instance.AddComponent<Rigidbody>();

        if (instance.GetComponent<BoxCollider>() == null)
            instance.AddComponent<BoxCollider>();

        if (instance.GetComponent<Animator>() == null)
            instance.AddComponent<Animator>();

        if (instance.GetComponent<NavMeshAgent>() == null)
            instance.AddComponent<NavMeshAgent>();

        if (instance.GetComponent<PostNatalManager>() == null)
            instance.AddComponent<PostNatalManager>();

        Component behaviour = instance.GetComponent(behaviourType);
        if (behaviour == null)
            behaviour = instance.AddComponent(behaviourType);

        ApplyRealisticScale(instance, behaviour, species);
        ApplyPhysicsDefaults(instance, species);
        CorrectMeshFacing(instance);

        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);

        Debug.Log($"[AnimalPrefabGenerator] {species}: creado {prefabPath} (modelo: {fbxPath}, behavior: {behaviourTypeName}).");
        return savedPrefab != null ? prefabPath : null;
    }

    // Los prefabs ya generados tenian el BoxCollider por defecto (size 1,1,1 en el
    // origen, sin ajustar al mesh real) y el Rigidbody con gravedad activa peleando
    // con el NavMeshAgent por el control de la posicion — por eso los animales
    // caian sin fondo al entrar en Play. Este fix es no-destructivo: ajusta el
    // collider y el rigidbody de los prefabs que YA existen, sin tocar el resto.
    [MenuItem("Tools/Cold Sanctuary/Fix Animal Colliders And Rigidbodies")]
    public static void FixExistingPrefabs()
    {
        int fixedCount = 0;
        foreach ((string species, string behaviourTypeName) in Species)
        {
            string prefabPath = $"{AnimalsRoot}/{species}/{species}.prefab";
            if (!File.Exists(prefabPath)) continue;

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);

            System.Type behaviourType = System.Type.GetType(behaviourTypeName)
                ?? System.AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType(behaviourTypeName))
                    .FirstOrDefault(t => t != null);
            Component behaviour = behaviourType != null ? root.GetComponent(behaviourType) : null;

            ApplyRealisticScale(root, behaviour, species);
            ApplyPhysicsDefaults(root, species);
            CorrectMeshFacing(root);

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            fixedCount++;
            Debug.Log($"[AnimalPrefabGenerator] {species}: escala/collider/orientacion ajustados, rigidbody kinematico.");
        }
        Debug.Log($"[AnimalPrefabGenerator] Fix listo — {fixedCount} prefab(s) corregidos.");
        AssetDatabase.SaveAssets();
    }

    // Factor de escala uniforme (x=y=z) por especie, calculado a partir del tamaño
    // "crudo" medido con Measure Raw Animal Sizes (localScale=1) contra un tamaño
    // real objetivo. HxH = altura de hombro (cuadrúpedos parados); LxL = longitud
    // corporal (especies horizontales: foca, ballena).
    //
    // Esta es la ÚNICA fuente de verdad para el tamaño de los prefabs — en un
    // principio se pensó usar Physiognomy.baseScale (el campo "defaultBody" de cada
    // *Behavior.cs) como fuente, pero se detectó que el valor estático compilado no
    // reflejaba ediciones de código fuente de forma confiable en este entorno (un
    // problema de compilación/caché de Unity ajeno a este script — ver DEVLOG). Este
    // método escribe el valor tanto en el Transform del prefab (apariencia en editor)
    // como en el campo "body" serializado (para que LifeStage.SetScale() en Play
    // también lo use), sin depender de releer ningún campo estático en runtime.
    static readonly System.Collections.Generic.Dictionary<string, float> RealisticScaleFactor = new()
    {
        { "Wolf",  0.268f },  // altura cruda 2.984m -> objetivo 0.8m
        { "Deer",  0.219f },  // altura cruda 5.489m -> objetivo 1.2m
        { "Fox",   0.134f },  // altura cruda 2.984m -> objetivo 0.4m
        { "Malamute", 0.185f },  // altura cruda 3.388m -> objetivo 0.63m (mesh husky reutilizado; sustituir por malamute)
        { "Bunny", 0.0106f }, // altura cruda 23.52m -> objetivo 0.25m
        { "Whale", 1.157f },  // longitud cruda 10.372m -> objetivo 12m
        { "PolarBear", 0.267f }, // altura cruda 4.870m -> objetivo 1.3m (longitud resultante ~2.0m)
        { "Seal", 0.165f },   // longitud cruda 10.329m -> objetivo 1.7m (foca ártica genérica; no es cuadrúpedo de pie, se referencia por longitud como Whale)
    };

    static void ApplyRealisticScale(GameObject root, Component behaviour, string species)
    {
        if (!RealisticScaleFactor.TryGetValue(species, out float factor)) return;

        Vector3 scale = new Vector3(factor, factor, factor);
        root.transform.localScale = scale;

        if (behaviour == null) return;
        System.Reflection.FieldInfo bodyField = behaviour.GetType().GetField("body");
        if (bodyField == null || bodyField.FieldType != typeof(Physiognomy)) return;

        // Mutar baseScale en el objeto Physiognomy YA existente (no reemplazar la
        // referencia por "defaultBody"), así solo cambia el tamaño y el resto de los
        // datos de la especie (masa, pesos de comida) quedan intactos tal como estén
        // serializados en este prefab.
        Physiognomy body = (Physiognomy)bodyField.GetValue(behaviour);
        if (body != null) body.baseScale = scale;
    }

    // Los FBX importados (Quaternius y Sketchfab por igual — confirmado en Oso Polar, Lobo y
    // Ciervo) traen el mesh/armature mirando hacia -Z local en vez de +Z. El root del prefab
    // NO se puede rotar (el NavMeshAgent usa su +Z local como "adelante" y lo re-orienta cada
    // frame hacia la velocidad — updateRotation), así que la corrección va en los hijos
    // directos (mesh/armature), rotándolos 180° en Y para que el frente visual coincida con
    // el frente de movimiento. Asignación absoluta (no acumulativa) para que sea idempotente
    // si se re-corre sobre un prefab ya corregido.
    static void CorrectMeshFacing(GameObject root)
    {
        foreach (Transform child in root.transform)
            child.localRotation = Quaternion.Euler(0f, 180f, 0f);
    }

    // Rigidbody cinematico (el NavMeshAgent es quien controla la posicion, un
    // Rigidbody dinamico con gravedad pelea con el y el animal cae sin fondo) y
    // BoxCollider ajustado al mesh real (el default size 1,1,1 en el origen no
    // encierra el modelo). Usado tanto al generar prefabs nuevos como al arreglar
    // los ya existentes, para que el bug no pueda reaparecer en una especie nueva.
    static void ApplyPhysicsDefaults(GameObject root, string species)
    {
        Rigidbody rig = root.GetComponent<Rigidbody>();
        if (rig != null)
        {
            rig.isKinematic = true;
            rig.useGravity = false;
        }

        BoxCollider box = root.GetComponent<BoxCollider>();
        if (box != null)
        {
            Bounds? combined = ComputeCombinedRendererBounds(root);
            if (combined.HasValue)
            {
                Vector3 lossyScale = root.transform.lossyScale;
                box.center = root.transform.InverseTransformPoint(combined.Value.center);
                box.size = new Vector3(
                    combined.Value.size.x / Mathf.Max(lossyScale.x, 0.0001f),
                    combined.Value.size.y / Mathf.Max(lossyScale.y, 0.0001f),
                    combined.Value.size.z / Mathf.Max(lossyScale.z, 0.0001f));
            }
            else
            {
                Debug.LogWarning($"[AnimalPrefabGenerator] {species}: no se encontraron Renderers para ajustar el BoxCollider.");
            }
        }
    }

    static Bounds? ComputeCombinedRendererBounds(GameObject root)
    {
        Bounds? combined = null;
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
        {
            if (combined == null) combined = r.bounds;
            else { Bounds b = combined.Value; b.Encapsulate(r.bounds); combined = b; }
        }
        return combined;
    }

    // Diagnostico de una sola vez: mide cada especie con localScale forzado a
    // (1,1,1) para ver el tamaño "crudo" del mesh importado, sin ningun factor
    // aplicado todavia. No guarda nada — solo loguea. Sirve para calcular a mano
    // que factor de escala (Physiognomy.baseScale en cada *Behavior.cs) hace
    // falta para llegar a un tamaño real realista por especie.
    [MenuItem("Tools/Cold Sanctuary/Measure Raw Animal Sizes (diagnostic)")]
    public static void MeasureRawSizes()
    {
        foreach ((string species, _) in Species)
        {
            string prefabPath = $"{AnimalsRoot}/{species}/{species}.prefab";
            if (!File.Exists(prefabPath)) continue;

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            Vector3 savedScale = root.transform.localScale;
            root.transform.localScale = Vector3.one;

            Bounds? combined = ComputeCombinedRendererBounds(root);
            if (combined.HasValue)
            {
                float height = combined.Value.size.y;
                float length = Mathf.Max(combined.Value.size.x, combined.Value.size.z);
                Debug.Log($"[AnimalPrefabGenerator] {species}: crudo (localScale=1) height={height:F3}m, length={length:F3}m");
            }
            else
            {
                Debug.LogWarning($"[AnimalPrefabGenerator] {species}: no se encontraron Renderers para medir.");
            }

            root.transform.localScale = savedScale;
            PrefabUtility.UnloadPrefabContents(root); // nunca se guarda — solo diagnostico
        }
    }

    // El caso de Deer es especial: la carpeta trae Deer.fbx (hembra/sin cornamenta)
    // y Stag.fbx (con cornamenta). Se prioriza Stag.fbx si esta presente.
    static string FindFbx(string modelsDir, string species)
    {
        if (!AssetDatabase.IsValidFolder(modelsDir)) return null;

        if (species == "Deer")
        {
            string stagPath = $"{modelsDir}/Stag.fbx";
            if (File.Exists(stagPath)) return stagPath;
        }

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { modelsDir });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                return path;
        }
        return null;
    }
}
