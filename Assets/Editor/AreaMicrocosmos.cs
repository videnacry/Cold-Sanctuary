using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// EL MICROCOSMOS DE CADA ÁREA (docs/microcosmos-areas.md): es como si de pronto el mundo fuera de insectos. El JUGADOR
/// (≈ 1,8 m) es una HORMIGA, y las hormigas son los HUMANOS del microcosmos: bípedas, con cara e intelecto humano. Todo lo
/// demás está a su escala:
/// - el ÁREA del santuario, ×10 (sus muebles son montañas);
/// - la flora: el musgo y los hongos hacen de hierba y arbustos; las briznas y los helechos, de árboles;
/// - la fauna de antes del fuego, a tamaño relativo a la hormiga (de pulgón 1 m a mantis 10 m).
/// Cada área tiene su HISTORIA:
/// - Enfermería: la muerte de Ambrosio (alba / cueva).
/// - Cocina (era del fuego): las reencarnaciones. La tribu recoge restos de comida y los lleva a la guarida; la lidera
///   Medea renacida, una hormiga poderosísima, que con Nasatya (Ambrosio renacido) es blanda.
/// - Huerto: La Sembradora (Ruth renacida) enseña a la tribu a cultivar el hongo.
/// - Resto de áreas: la tribu viviendo en su área gigante.
/// </summary>
public static class AreaMicrocosmos
{
    public const float AreaScale = 10f;
    const float StoryScale = 2.25f, AntHeight = 1.8f;

    /// <param name="storyOverride">Otra historia en vez de la del área (p. ej. las simulaciones de compasión): construye en
    /// el Transform dado, sobre el suelo del área, y devuelve cuántos personajes vistió.</param>
    internal static void Build(string areaGo, string display, SampleSceneBuilder.MissionKind kind, Color tint, Vector3 o, System.Func<Transform, int> storyOverride = null, MicrocosmSpec spec = null)
    {
        var root = new GameObject($"Microcosmos_{display}");
        // Su FECHA (Eras): las conductas consultan Eras.Allows (sin ordeño de pulgones antes del Neolítico, etc.).
        var era = Eras.OfMicrocosmos(display);
        if (era.HasValue) { MicroEra me = root.AddComponent<MicroEra>(); me.year = era.Value.year; me.place = era.Value.place; }
        if (spec != null) MicrocosmTemplate.Stamp(root, spec);   // la plantilla: fecha, hora y plataforma
        int dressed = 0;

        // 1) El ÁREA del santuario, ×10, ENTERA (sin conducta): el microcosmos ES el área vista desde el suelo, con sus
        //    muebles convertidos en montañas. Antes se vaciaba un claro de 50 m en el centro para la historia, y en la
        //    cocina (cocina abierta en el centro, comedor alrededor) eso borraba justo la cocina: ya no se reconocía.
        GameObject src = GameObject.Find(areaGo), giant = null;
        // Sus POSICIONES RESERVADAS (regla 8, PlatformSlots): la historia, los personajes, la fauna y la flora del microcosmos
        // van a las plazas que el piso dejó libres al construirse. Están en locales del área: en el área ×10 (clonada en el
        // origen de la raíz, sin girar), plaza × AreaScale.
        PlatformSlots slots = src != null ? src.GetComponent<PlatformSlots>() : null;
        if (src != null && slots == null) Debug.LogWarning($"[Microcosmos] {display}: «{areaGo}» no tiene posiciones reservadas (PlatformSlots; reconstruye la escena principal): historia, flora y fauna van como antes.");
        if (src != null)
        {
            giant = Object.Instantiate(src);
            giant.name = $"{display}_gigante";
            CopyAttached(areaGo, src.transform, giant.transform);
            foreach (MonoBehaviour mb in giant.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(mb);
            foreach (string n in new[] { "Estructura_Elevada" }) { Transform t = giant.transform.Find(n); if (t != null) Object.DestroyImmediate(t.gameObject); }
            foreach (Transform t in giant.GetComponentsInChildren<Transform>(true)) if (t != null && t.name.EndsWith("_VirtualizationMachine")) Object.DestroyImmediate(t.gameObject);
            giant.transform.SetParent(root.transform, false);
            giant.transform.localPosition = Vector3.zero; giant.transform.localRotation = Quaternion.identity;
            giant.transform.localScale = Vector3.one * AreaScale;
            foreach (Transform c in giant.transform)
                if (c.name == "Mat" || c.name.StartsWith("Suelo")) c.localPosition = new Vector3(c.localPosition.x, -0.02f, c.localPosition.z);   // su suelo, bajo el resto
            // Una copia de las plazas en el área gigante (con su escala ×10): las pruebas miran si se usaron.
            if (slots != null)
            {
                PlatformSlots copy = giant.AddComponent<PlatformSlots>();
                copy.story.AddRange(slots.story); copy.characters.AddRange(slots.characters); copy.fauna.AddRange(slots.fauna); copy.flora.AddRange(slots.flora);
                copy.floorSize = slots.floorSize; copy.storyClearance = slots.storyClearance; copy.freeCells = slots.freeCells;
            }
        }
        else Debug.LogWarning($"[Microcosmos] No encuentro {areaGo} (abre la escena principal): sin área gigante.");

        // 2) Dónde va la HISTORIA: el hueco más libre del SUELO del área (entre mesas, junto a la barra…), no su centro.
        Bounds foot = new Bounds(Vector3.zero, new Vector3(100f, 1f, 100f));
        var floors = new List<Renderer>(); _blocked.Clear();
        if (giant != null) foot = Classify(giant, floors, _blocked);
        Vector3 anchor = Vector3.zero; float free = 50f;
        // Con posiciones reservadas, la HISTORIA va a su plaza principal (el punto del piso más alejado de los muebles, calculado
        // al construir el piso, ya con sus alfombras); sin ellas, se busca aquí.
        if (giant != null && slots != null && slots.story.Count > 0) { anchor = slots.story[0] * AreaScale; anchor.y = 0f; free = slots.storyClearance * AreaScale; }
        else if (giant != null) anchor = FreeSpot(foot, floors, _blocked, out free);
        // HOSPITAL: la historia va DENTRO, por su pasillo central (30 m de ancho a ×10), empezando junto a la entrada; el hueco
        // «más libre» caía en el borde y la historia quedaba fuera del hospital (Berón, 2026-10-23). Si una plaza de historia cae
        // en el pasillo, cerca de la entrada, va a esa plaza.
        Transform aisle = giant != null && kind == SampleSceneBuilder.MissionKind.Heal ? FindDeep(giant.transform, "Pasillo_central") : null;
        if (aisle != null && aisle.GetComponent<Renderer>() != null)
        {
            Bounds ab = aisle.GetComponent<Renderer>().bounds;
            anchor = new Vector3(ab.center.x, 0f, ab.min.z + 30f); free = ab.size.x;
            float bestD = 15f;
            if (slots != null)
                foreach (Vector3 s in slots.story)
                {
                    Vector3 p = s * AreaScale; p.y = 0f;
                    float d = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(anchor.x, anchor.z));
                    if (InsideXZ(ab, p, -2f) && d < bestD) { bestD = d; anchor = p; }
                }
        }
        anchor.y = FloorAt(anchor, floors, 0f);
        var story = new GameObject("Historia").transform; story.SetParent(root.transform, false); story.localPosition = anchor;

        // Entrada y salida, dentro del hueco de la historia (el portal muestra su diálogo al acercarse).
        var spawn = new GameObject("MobSpawnPoint"); spawn.transform.SetParent(story, false);
        spawn.transform.localPosition = new Vector3(0f, 1f, -14f); spawn.transform.localRotation = Quaternion.LookRotation(Vector3.forward);
        spawn.AddComponent<MobSpawnPoint>();
        var portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "WorldExitPortal"; portal.transform.SetParent(story, false);
        portal.transform.localPosition = new Vector3(0f, 1.5f, -20f); portal.transform.localScale = new Vector3(2f, 3f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = Mat("MobWorldExitPortal", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<WorldExitPortal>().label = "Salir del microcosmos";
        GalaxyPortal.Dress(portal.transform);   // un agujero negro en una pequeña galaxia (Berón 2026-10-27)

        Transform cast = new GameObject("Elenco").transform; cast.SetParent(story, false);
        if (storyOverride != null) dressed = storyOverride(cast);
        else if (kind == SampleSceneBuilder.MissionKind.Heal) dressed = InfirmaryStory(cast);
        else if (kind == SampleSceneBuilder.MissionKind.KitchenChannel) dressed = KitchenStory(cast);
        else if (kind == SampleSceneBuilder.MissionKind.GardenChannel) dressed = GardenStory(cast);
        else dressed = TribeStory(cast, display, 8);
        // Las historias propias (Patanjali, Planta Atómica, Hospital…) traen su elenco pero ninguna recolectora: sin ellas los
        // mirmidones de ese microcosmos no viven del entorno (semillas; melaza desde el Neolítico, Eras.Dairying). Se añade
        // una cuadrilla pequeña con su guarida, a un lado del claro de la historia (dentro de él: la flora no la tapa).
        if ((spec == null || spec.foragers) && cast.GetComponentInChildren<MicroForager>(true) == null) dressed += ForagingBand(cast, new Vector3(0f, 0f, 26f), 6);

        // 3) Solo se aparta el mueble que pisa la historia (una mesa, una silla), no el área.
        int moved = giant != null ? ClearUnder(story, giant.transform, foot) : 0;
        // La historia de la enfermería (el sandbox de Ambrosio) borra piezas al montarse: sin quitar sus suelos de la lista,
        // el microcosmos del Hospital fallaba (MissingReference en EdgeDist) y se quedaba la escena vieja, sin el hospital.
        floors.RemoveAll(f => f == null);
        if (giant == null)
            Box(root.transform, "Suelo_claro", new Vector3(0f, -0.05f, 0f), new Vector3(120f, 0.1f, 120f), Mat("Micro_Tierra", new Color(0.45f, 0.36f, 0.25f)), true);

        // 4) La FLORA a escala de hormiga — musgo = hierba, briznas y helechos = árboles — crece por TODA la superficie, hasta
        //    el agua: también por el suelo libre del área (entre sus muebles gigantes), menos en el claro de la historia. Cada
        //    planta es un interactuable (Affordance) que algún insecto usa; y su fauna.
        float r0 = giant != null ? Mathf.Max(foot.extents.x, foot.extents.z) * 1.15f + 8f : 52f, r1 = r0 + 110f;
        // El SUELO DEL ÁREA es el más bajo bajo su centro (no el más alto: anillos, entreplantas y pisos de arriba también son
        // «suelos»; con el más alto, la flora del suelo real se descartaba y el microcosmos se quedaba pelado, Berón 2026-10-28).
        float groundY = giant != null ? FloorLowest(foot.center, floors, 0f) - 0.02f : 0f;
        // El suelo de alrededor, del color de su tierra (arena de Egipto, barro helado de Flandes, mantillo de la taiga…).
        MicroBiome.Biome bio = MicroBiome.For(display);
        Color soil = bio != null ? bio.ground : new Color(0.45f, 0.36f, 0.25f);
        Box(root.transform, "Suelo_bosque", new Vector3(foot.center.x, groundY - 0.05f, foot.center.z), new Vector3(r1 * 2f + 20f, 0.1f, r1 * 2f + 20f), Mat("Micro_Tierra_" + (bio != null ? display : "generica"), soil), true);
        Bounds storyCore = new Bounds(anchor, Vector3.zero);   // lo que ocupa la historia (guarida, cueva, bancal…): ahí no crece nada
        foreach (Renderer r in story.GetComponentsInChildren<Renderer>(true))
        {
            Bounds rb = r.bounds;
            if (r.GetComponentInParent<MicroFood>() != null || (rb.size.y < 0.5f && Mathf.Max(rb.size.x, rb.size.z) > 40f)) continue;   // restos y suelos/agua de la historia no cuentan
            storyCore.Encapsulate(rb);
        }
        // Con posiciones reservadas, la flora y la fauna DEL ÁREA van a sus plazas (×10); el bosque de fuera, como siempre.
        bool useSlots = giant != null && slots != null;
        List<Vector3> floraAt = useSlots ? slots.flora.ConvertAll(s => s * AreaScale) : null, faunaAt = useSlots ? slots.fauna.ConvertAll(s => s * AreaScale) : null;
        PlatformSlots giantSlots = giant != null ? giant.GetComponent<PlatformSlots>() : null;
        int animals = Nature(root.transform, new Vector3(foot.center.x, groundY, foot.center.z), r1, display.GetHashCode(), storyCore, giant != null ? foot : (Bounds?)null, floors, MicroBiome.For(display),
                             floraAt, faunaAt, giantSlots != null ? giantSlots.floraUnderStory : null);

        // Los PERSONAJES que andan por el área (las recolectoras de la tribu, MicroForager) empiezan en sus plazas de personaje,
        // alrededor de la historia; los de papel fijo (la líder, Nasatya, la Sembradora…) siguen junto a su estructura.
        int cast0 = 0;
        if (useSlots)
            foreach (MicroForager mf in cast.GetComponentsInChildren<MicroForager>(true))
            {
                if (cast0 >= slots.characters.Count) break;
                if (!mf.enabled) continue;   // papeles guionizados (los porteadores del mito empiezan quietos, en su sitio)
                Vector3 p = slots.characters[cast0++] * AreaScale;
                p.y = FloorNear(p, floors, p.y);
                mf.transform.position = root.transform.TransformPoint(p);
            }
        if (useSlots) Debug.Log($"[Plazas] Microcosmos {display}: historia en su plaza ({anchor.x:0}, {anchor.z:0}), {cast0}/{slots.characters.Count} personajes, flora en {slots.flora.Count - (giantSlots != null ? giantSlots.floraUnderStory.Count : 0)}/{slots.flora.Count} plazas (el resto bajo la historia), fauna en {_faunaOnSlots}/{slots.fauna.Count} plazas.");

        root.transform.position = o;

        // Las misiones del área (yoga, textil, mecánica) siguen en su tablero.
        if (storyOverride == null && (kind == SampleSceneBuilder.MissionKind.Yoga || kind == SampleSceneBuilder.MissionKind.Channel))
        {
            MobMission[] missions = SampleSceneBuilder.BuildAreaMissions(root.transform, root.transform, display, tint, kind);
            foreach (MobMission m in missions) { if (m == null) continue; m.transform.position = story.TransformPoint(new Vector3(0f, 0f, 6f)); foreach (MeditationMissionBase b in m.GetComponents<MeditationMissionBase>()) b.endMode = MissionEndMode.Standalone; }
            var board = root.AddComponent<MobWorldMissionBoard>(); board.areaName = display; board.missions = missions;
        }

        var navGo = new GameObject("NavMesh_Microcosmos"); navGo.transform.SetParent(root.transform, false);
        navGo.transform.localPosition = new Vector3(foot.center.x, 0f, foot.center.z);
        var nav = navGo.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
        nav.collectObjects = Unity.AI.Navigation.CollectObjects.Volume;
        // Solo lo SÓLIDO (colliders): los decorados sin colisión (alfombras, ríos pintados, losas ocultas…) entraban como
        // escalones y partían el suelo en islas (Patanjali: la guarida y el claro de la historia, aislados del resto).
        nav.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
        Physics.SyncTransforms();   // con colliders, el horneado consulta la física: sin sincronizar, no ve lo recién creado y movido
        float span = r1 * 2f + 20f;
        nav.center = Vector3.up * 15f; nav.size = new Vector3(span, 60f, span);
        nav.BuildNavMesh();
        Debug.Log($"[Microcosmos] {display}: área ×{AreaScale} entera (se apartan {moved} muebles bajo la historia, hueco libre {free:0} m entre {_blocked.Count} muebles), {dressed} mirmidones e insectos de la historia y {animals} animales a escala de hormiga.");
    }

    // ── Dónde cabe la historia dentro del área ───────────────────────────────────────────────────────────────────

    // Partes del área que el constructor NO cuelga de su GameObject (y que el área ×10 necesita para reconocerse). La
    // COCINA de verdad (puestos, neveras, barra de pase) vive en Restaurante_AUTO/Sala, no bajo Kitchen_Area: sin
    // copiarla, el microcosmos de la cocina era solo el comedor, con un hueco vacío en medio (el «hueco de 127 m»).
    static readonly Dictionary<string, string[]> Attached = new Dictionary<string, string[]>
    {
        { "Kitchen_Area", new[] { "Restaurante_AUTO/Sala" } },
    };

    /// <summary>Copia en <paramref name="copy"/> (el área recién clonada, aún a escala 1) las partes de <see cref="Attached"/>,
    /// con su misma pose relativa al área.</summary>
    static void CopyAttached(string areaGo, Transform src, Transform copy)
    {
        if (!Attached.TryGetValue(areaGo, out string[] paths)) return;
        foreach (string path in paths)
        {
            string[] parts = path.Split('/');
            GameObject top = GameObject.Find(parts[0]);
            Transform part = top == null ? null : parts.Length > 1 ? top.transform.Find(string.Join("/", parts, 1, parts.Length - 1)) : top.transform;
            if (part == null) { Debug.LogWarning($"[Microcosmos] {areaGo}: no encuentro {path}; el área ×10 irá sin esa parte."); continue; }
            Transform c = Object.Instantiate(part.gameObject).transform;
            c.name = part.name;
            c.SetParent(copy, false);
            c.localPosition = src.InverseTransformPoint(part.position);
            c.localRotation = Quaternion.Inverse(src.rotation) * part.rotation;
            c.localScale = part.lossyScale;
        }
    }

    // Bloques sólidos del área gigante (lo que no es suelo); restos de comida y briznas los evitan.
    static readonly List<Bounds> _blocked = new List<Bounds>();

    static bool IsFloor(Bounds b) => b.size.y < 1.5f && b.max.y < 3f;

    /// <summary>Separa el SUELO (planos bajos) de los MUEBLES (bloques) y devuelve la huella del área.</summary>
    static Bounds Classify(GameObject giant, List<Renderer> floors, List<Bounds> blocked)
    {
        Bounds foot = default; bool any = false;
        foreach (Renderer r in giant.GetComponentsInChildren<Renderer>())
        {
            Bounds b = r.bounds;
            if (!any) { foot = b; any = true; } else foot.Encapsulate(b);
            if (IsFloor(b)) { if (b.size.x > 4f && b.size.z > 4f) floors.Add(r); }   // suelo (no un tapete suelto)
            else blocked.Add(b);
        }
        return any ? foot : new Bounds(Vector3.zero, new Vector3(100f, 1f, 100f));
    }

    static float DistXZ(Bounds b, Vector3 p)
    {
        float dx = Mathf.Max(b.min.x - p.x, 0f, p.x - b.max.x), dz = Mathf.Max(b.min.z - p.z, 0f, p.z - b.max.z);
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    /// <summary>El punto del SUELO del área más alejado de cualquier mueble y del borde del suelo (rejilla de 6 m). Solo
    /// cuentan puntos que pisan un suelo del área: la huella incluye escaleras, rótulos y piezas que sobresalen, y sin
    /// esto el «hueco más libre» caía fuera de la plataforma (en la Cocina daba 127 m libres, lejos de los muebles).</summary>
    static Vector3 FreeSpot(Bounds foot, List<Renderer> floors, List<Bounds> blocked, out float free)
    {
        Vector3 best = new Vector3(foot.center.x, 0f, foot.center.z); free = 0f;
        float inset = Mathf.Min(10f, Mathf.Min(foot.extents.x, foot.extents.z) * 0.5f);
        for (float x = foot.min.x + inset; x <= foot.max.x - inset; x += 6f)
            for (float z = foot.min.z + inset; z <= foot.max.z - inset; z += 6f)
            {
                var p = new Vector3(x, 0f, z);
                float d = floors.Count == 0 ? float.MaxValue : EdgeDist(p, floors);
                if (d <= free) continue;   // fuera del suelo (0) o más cerca del borde que el mejor
                foreach (Bounds b in blocked) { d = Mathf.Min(d, DistXZ(b, p)); if (d <= free) break; }
                if (d > free) { free = d; best = p; }
            }
        return best;
    }

    /// <summary>Distancia de <paramref name="p"/> al borde del suelo más grande que lo cubre (0 si no pisa ninguno).</summary>
    static float EdgeDist(Vector3 p, List<Renderer> floors)
    {
        float best = 0f;
        foreach (Renderer r in floors)
        {
            Bounds b = r.bounds;
            if (p.x < b.min.x || p.x > b.max.x || p.z < b.min.z || p.z > b.max.z) continue;
            best = Mathf.Max(best, Mathf.Min(Mathf.Min(p.x - b.min.x, b.max.x - p.x), Mathf.Min(p.z - b.min.z, b.max.z - p.z)));
        }
        return best;
    }

    /// <summary>Altura del suelo del área en ese punto (la cara de arriba del plano más alto que lo cubre).</summary>
    // El suelo bajo p cuya cara de arriba queda más cerca de <paramref name="near"/> (no el más alto: hay plantas encima).
    static float FloorNear(Vector3 p, List<Renderer> floors, float near)
    {
        float y = near, best = float.MaxValue;
        foreach (Renderer r in floors)
        {
            if (r == null) continue;
            Bounds b = r.bounds;
            if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z && Mathf.Abs(b.max.y - near) < best) { best = Mathf.Abs(b.max.y - near); y = b.max.y; }
        }
        return y;
    }

    // El suelo más BAJO bajo p (el del área; por encima hay anillos y entreplantas).
    static float FloorLowest(Vector3 p, List<Renderer> floors, float fallback)
    {
        float y = float.MaxValue;
        foreach (Renderer r in floors)
        {
            if (r == null) continue;
            Bounds b = r.bounds;
            if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z) y = Mathf.Min(y, b.max.y);
        }
        return y == float.MaxValue ? fallback : y;
    }

    static float FloorAt(Vector3 p, List<Renderer> floors, float fallback)
    {
        float y = float.MinValue;
        foreach (Renderer r in floors)
        {
            if (r == null) continue;
            Bounds b = r.bounds;
            if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z) y = Mathf.Max(y, b.max.y);
        }
        return y == float.MinValue ? fallback : y;
    }

    /// <summary>Aparta SOLO los muebles que pisa la historia (su guarida, su gente, el portal); el resto del área queda.</summary>
    static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    static int ClearUnder(Transform story, Transform giant, Bounds foot)
    {
        // Solo se aparta el mueble que CHOCA con una pieza sólida de la historia (no todo lo que cae dentro de la caja que la
        // envuelve): las historias grandes (el bosque y la cueva de Ambrosio) se MEZCLAN con el área, que sigue reconociéndose
        // (Berón, 2026-10-23: el microcosmos reutiliza el área y la puebla por encima). Los suelos de la historia no cuentan.
        var solids = new List<Bounds>();
        foreach (Renderer r in story.GetComponentsInChildren<Renderer>(true))
        {
            if (r.GetComponentInParent<MicroFood>() != null) continue;   // los restos se recolocan luego
            Bounds rb = r.bounds;
            if (rb.size.y < 0.5f && Mathf.Max(rb.size.x, rb.size.z) > 6f) continue;   // suelo / agua de la historia
            rb.Expand(new Vector3(1f, 0f, 1f)); solids.Add(rb);
        }
        foreach (MobSpawnPoint sp in story.GetComponentsInChildren<MobSpawnPoint>(true)) solids.Add(new Bounds(sp.transform.position, new Vector3(6f, 4f, 6f)));   // se llega a un sitio libre
        if (solids.Count == 0) return 0;
        var kill = new HashSet<GameObject>();
        foreach (Renderer r in giant.GetComponentsInChildren<Renderer>())
        {
            Bounds b = r.bounds;
            if (IsFloor(b)) continue;
            bool hit = false;
            foreach (Bounds s in solids) if (b.max.x >= s.min.x && b.min.x <= s.max.x && b.max.z >= s.min.z && b.min.z <= s.max.z && b.min.y <= s.max.y) { hit = true; break; }
            if (hit) kill.Add(Piece(r.transform, giant, foot).gameObject);
        }
        foreach (GameObject g in kill) if (g != null) Object.DestroyImmediate(g);
        _blocked.Clear();
        foreach (Renderer r in giant.GetComponentsInChildren<Renderer>()) if (!IsFloor(r.bounds)) _blocked.Add(r.bounds);
        // Restos de comida que hayan caído dentro de un mueble: al suelo libre más cercano de su zona.
        foreach (MicroFood f in story.GetComponentsInChildren<MicroFood>(true))
            for (int k = 0; k < 12 && Blocked(f.transform.position); k++)
            {
                Vector2 r = Random.insideUnitCircle * f.spread * 0.5f;
                f.transform.position = new Vector3(story.position.x + r.x, f.transform.position.y, story.position.z + r.y);
            }
        return kill.Count;
    }

    // El MUEBLE entero al que pertenece un cubo (la mesa, no su pata): el hijo del área si es pequeño, o el de su grupo.
    static Transform Piece(Transform t, Transform giant, Bounds foot)
    {
        Transform top = t;
        while (top.parent != null && top.parent != giant) top = top.parent;
        if (top == t || Footprint(top) < 0.15f * foot.size.x * foot.size.z) return top;
        Transform piece = t;
        while (piece.parent != null && piece.parent != top) piece = piece.parent;
        return piece;
    }

    static float Footprint(Transform t)
    {
        Bounds b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return any ? b.size.x * b.size.z : 0f;
    }

    static bool Blocked(Vector3 p)
    {
        foreach (Bounds b in _blocked) if (p.x > b.min.x && p.x < b.max.x && p.z > b.min.z && p.z < b.max.z && p.y < b.max.y) return true;
        return false;
    }

    // ── HISTORIAS ─────────────────────────────────────────────────────────────────────────────────────────────────

    // ENFERMERÍA: la muerte de Ambrosio. Los builders del alba, a escala de hormiga, SIN los duplicados del mapa de
    // juego (otro Ambrosio, otra Kushal-hormiga: Kushal eres tú).
    static int InfirmaryStory(Transform root)
    {
        var story = new GameObject("Historia_Ambrosio").transform; story.SetParent(root, false);
        SampleSceneBuilder.BuildMicrocosmosSandbox(story);
        SampleSceneBuilder.BuildNivel1Sandbox(story);
        foreach (string dup in new[] { "Ambrosio_Aphid", "HoneydewDrop_Sample", "Kushal_Ant" })
            foreach (Transform t in story.GetComponentsInChildren<Transform>(true)) if (t != null && t.name == dup) { Object.DestroyImmediate(t.gameObject); break; }
        story.localScale = Vector3.one * StoryScale;
        // Los dos sandboxes se montan con su propio desplazamiento en el mundo (a cientos de metros): se traen junto al hueco de
        // la historia, a lo largo del pasillo central (+z, hacia el fondo): primero la cueva y luego el bosque, mezclados con
        // las camas y el equipo de los lados en vez de taparlo entero. La entrada y el portal quedan detrás (−z).
        float[] at = { 25f, 90f }; int k = 0;
        foreach (Transform top in story)
        {
            Renderer[] rs = top.GetComponentsInChildren<Renderer>(true); if (rs.Length == 0) continue;
            Bounds bb = rs[0].bounds; foreach (Renderer r in rs) bb.Encapsulate(r.bounds);
            Vector3 want = story.position + new Vector3(0f, 0f, at[Mathf.Min(k++, at.Length - 1)]);
            // Su suelo (el del bosque, el de la cueva) quedaba ~3 m BAJO el suelo del hospital: el elenco andaba enterrado. Se
            // sube a ras del suelo del hospital y ese suelo propio se oculta: se pisa el del hospital, con el bosque encima.
            float floorTop = float.MinValue; var ownFloors = new List<Renderer>();
            foreach (Renderer r in rs) { Bounds b = r.bounds; if (b.size.y < 0.5f && Mathf.Max(b.size.x, b.size.z) > 6f) { ownFloors.Add(r); floorTop = Mathf.Max(floorTop, b.max.y); } }
            float dy = floorTop > float.MinValue ? story.position.y - floorTop : 0f;
            top.position += new Vector3(want.x - bb.center.x, dy, want.z - bb.center.z);
            foreach (Renderer r in ownFloors) { r.enabled = false; Collider c = r.GetComponent<Collider>(); if (c != null) c.enabled = false; }
        }
        int n = 0;
        foreach (MeshFilter mfa in story.GetComponentsInChildren<MeshFilter>(true))   // todas las cápsulas: el elenco y los depredadores
        {
            MeshFilter mf = mfa; Component a = mfa;
            if (mf == null || mf.sharedMesh == null || !mf.sharedMesh.name.Contains("Capsule")) continue;
            Renderer r = a.GetComponent<Renderer>();
            Color c = r != null && r.sharedMaterial != null ? r.sharedMaterial.color : new Color(0.5f, 0.4f, 0.3f);
            bool aphid = a.name.Contains("Ambrosio");
            // Los depredadores siguen siendo INSECTOS (a escala de la hormiga); las hormigas, MIRMIDONES (hormigas humanas).
            if (a.name.Contains("Mariquita")) { CubeInsect.Dress(a.gameObject, "Ladybug", c, 2.4f); n++; continue; }
            if (a.name.Contains("Escarabajo")) { CubeInsect.Dress(a.gameObject, "DungBeetle", c, 5f); n++; continue; }
            if (a.name.Contains("Ara")) { CubeInsect.Dress(a.gameObject, "Spider", c, 4f); n++; continue; }
            CubeInsect.DressMyrmidon(a.gameObject, c, aphid ? AntHeight * 1.2f : AntHeight, aphid ? 1.6f : 1f, aphid);
            n++;
        }
        return n;
    }

    // COCINA (era del fuego): la tribu de las reencarnaciones recoge restos de comida y los lleva a la guarida.
    static int KitchenStory(Transform root)
    {
        Transform lair = Lair(root, Vector3.zero, "Guarida de la tribu");
        Food(root, false, 26, 42f);
        int n = 0;
        // Medea renacida: LA TIRANA DE LAS FEROMONAS, la líder (enorme y poderosa).
        GameObject leader = Ant(root, "La Tirana de las Feromonas", new Vector3(0f, 0f, 5f), new Color(0.45f, 0.12f, 0.12f), 2.3f, 1.25f); n++;
        // Ambrosio renacido: NASATYA, la «almohada gigante», mansa y fortísima, recostada en una postura rara.
        GameObject nasatya = Ant(root, "Nasatya", new Vector3(8f, 0f, 4f), new Color(0.75f, 0.68f, 0.55f), 2.4f, 2f); n++;
        nasatya.AddComponent<MicroLounger>();
        var ml = leader.AddComponent<MicroLeader>(); ml.lair = lair; ml.nasatya = nasatya.transform;
        // Héspero renacido: EL SEÑOR DEL FUEGO, junto a la brasa de la tribu; Sakshi: EL CHAMÁN; Momo: EL BUFÓN.
        Transform ember = Box(root, "Brasa", new Vector3(-6f, 0.4f, 3f), new Vector3(1.2f, 0.8f, 1.2f), Mat("Micro_Brasa", new Color(1f, 0.45f, 0.15f)), true);
        Ant(root, "El Señor del Fuego", new Vector3(-7.5f, 0f, 3f), new Color(0.3f, 0.2f, 0.18f), 2f, 1.1f); n++;
        Ant(root, "El Chamán", new Vector3(-5f, 0f, 5.5f), new Color(0.35f, 0.4f, 0.45f), 1.8f, 0.95f); n++;
        Ant(root, "El Bufón", new Vector3(4f, 0f, 8f), new Color(0.75f, 0.5f, 0.2f), 1.7f, 0.95f).AddComponent<MicroForager>(); n++;
        for (int i = 0; i < 12; i++)
        {
            float a = i * 30f * Mathf.Deg2Rad;
            Ant(root, $"Recolectora_{i + 1}", new Vector3(Mathf.Cos(a) * 12f, 0f, Mathf.Sin(a) * 12f), new Color(0.4f, 0.25f, 0.18f), 1.8f, 1f).AddComponent<MicroForager>();
            n++;
        }
        return n;
    }

    // HUERTO: La Sembradora (Ruth renacida) cuida el bancal de hongos y enseña a la tribu a cultivar.
    static int GardenStory(Transform root)
    {
        Lair(root, new Vector3(-14f, 0f, 0f), "Guarida de la tribu");
        Transform bed = new GameObject("Bancal_de_hongos").transform; bed.SetParent(root, false); bed.localPosition = new Vector3(10f, 0f, 2f);
        Box(bed, "Lecho", new Vector3(0f, 0.15f, 0f), new Vector3(10f, 0.3f, 7f), Mat("Micro_Lecho", new Color(0.35f, 0.28f, 0.2f)), true);
        Material st = Mat("Micro_Pie_Seta", new Color(0.9f, 0.86f, 0.75f)), cp = Mat("Micro_Hongo_cultivo", new Color(0.92f, 0.9f, 0.8f));
        for (int i = 0; i < 12; i++)
        {
            Transform m = new GameObject($"Hongo_{i + 1}").transform; m.SetParent(bed, false);
            m.localPosition = new Vector3(-4f + (i % 4) * 2.6f, 0.3f, -2.5f + (i / 4) * 2.5f);
            Box(m, "Pie", new Vector3(0f, 0.3f, 0f), new Vector3(0.2f, 0.6f, 0.2f), st, false);
            Box(m, "Sombrero", new Vector3(0f, 0.65f, 0f), new Vector3(0.8f, 0.2f, 0.8f), cp, false);
        }
        Food(root, true, 20, 40f);   // hojas para abonar el hongo
        Food(root, false, 12, 40f);  // y comida silvestre que aún se recoge
        int n = 0;
        var teacher = Ant(root, "La Sembradora", new Vector3(12f, 0f, -3f), new Color(0.35f, 0.45f, 0.25f), 1.8f, 1f).AddComponent<MicroTeacher>(); n++;
        teacher.bed = bed;
        for (int i = 0; i < 10; i++)
        {
            float a = i * 36f * Mathf.Deg2Rad;
            Ant(root, $"Mirmidón_{i + 1}", new Vector3(-14f + Mathf.Cos(a) * 8f, 0f, Mathf.Sin(a) * 8f), new Color(0.4f, 0.25f, 0.18f), 1.8f, 1f).AddComponent<MicroForager>();
            n++;
        }
        return n;
    }

    // CUADRILLA DE RECOLECTORAS para los microcosmos de historia: su guarida y unas mirmidonas que recogen lo que da el entorno.
    internal static int ForagingBand(Transform root, Vector3 at, int count)
    {
        Lair(root, at, "Guarida de las recolectoras");
        // Restos junto a la guarida (como TribeStory): donde el área gigante tapa el suelo (el Hospital ×10 ocupa casi todo
        // y sus paredes cierran tres lados) la flora queda a >130 m y las recolectoras no tenían nada al alcance.
        Transform scraps = new GameObject("Restos_de_la_guarida").transform; scraps.SetParent(root, false); scraps.localPosition = at;
        Food(scraps, false, 12, 24f);   // en anillo de 14–24 m alrededor de la guarida
        for (int i = 0; i < count; i++)
        {
            float a = i * (360f / count) * Mathf.Deg2Rad;
            Ant(root, $"Recolectora_{i + 1}", at + new Vector3(Mathf.Cos(a) * 8f, 0f, Mathf.Sin(a) * 8f), new Color(0.4f, 0.25f, 0.18f), 1.8f, 1f).AddComponent<MicroForager>();
        }
        return count;
    }

    // RESTO DE ÁREAS: la tribu en su área gigante, con su guarida y sus recolectoras.
    static int TribeStory(Transform root, string display, int count)
    {
        Lair(root, Vector3.zero, $"Guarida ({display})");
        Food(root, false, 16, 40f);
        for (int i = 0; i < count; i++)
        {
            float a = i * (360f / count) * Mathf.Deg2Rad;
            Ant(root, $"Mirmidón_{i + 1}", new Vector3(Mathf.Cos(a) * 10f, 0f, Mathf.Sin(a) * 10f), new Color(0.4f, 0.25f, 0.18f), 1.8f, 1f).AddComponent<MicroForager>();
        }
        return count;
    }

    // ── Piezas ────────────────────────────────────────────────────────────────────────────────────────────────────

    // Una HORMIGA HUMANIZADA: bípeda, con cara; NavMeshAgent para rodear los muebles gigantes.
    internal static GameObject Ant(Transform root, string name, Vector3 pos, Color c, float height, float bulk)
    {
        var g = new GameObject(name);
        g.transform.SetParent(root, false); g.transform.localPosition = pos;
        g.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        var col = g.AddComponent<CapsuleCollider>(); col.height = height; col.radius = 0.35f * bulk; col.center = Vector3.up * height * 0.5f;
        var ag = g.AddComponent<UnityEngine.AI.NavMeshAgent>(); ag.radius = 0.4f * bulk; ag.height = height; ag.speed = 3f; ag.angularSpeed = 360f;
        CubeInsect.DressMyrmidon(g, c, height, bulk);
        WorldLabel wl = g.AddComponent<WorldLabel>(); wl.text = name; wl.height = height + 0.4f;
        return g;
    }

    static Transform Lair(Transform root, Vector3 pos, string label)
    {
        Transform l = new GameObject("Guarida").transform; l.SetParent(root, false); l.localPosition = pos;
        Material earth = Mat("Micro_Guarida", new Color(0.4f, 0.3f, 0.2f)), dark = Mat("Micro_Boca_guarida", new Color(0.08f, 0.06f, 0.05f));
        for (int k = 0; k < 4; k++)
        {
            Transform m = Box(l, "Monticulo", new Vector3(0f, 0.6f + k * 1.1f, -3f), new Vector3(12f - k * 2.6f, 1.2f, 9f - k * 2f), earth, true);
            var mod = m.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>(); mod.overrideArea = true; mod.area = 1;   // no se anda por encima: su cima era una isla del NavMesh
        }
        Box(l, "Entrada", new Vector3(0f, 1.2f, 1.55f), new Vector3(2.4f, 2.4f, 0.1f), dark, false);
        l.gameObject.AddComponent<MicroLair>().doorOffset = new Vector3(0f, 0f, 3.2f);   // delante de la boca: el centro queda dentro del montículo
        WorldLabel wl = l.gameObject.AddComponent<WorldLabel>(); wl.text = label; wl.height = 6f;
        return l;
    }

    // Restos de comida (o hojas): migas, granos y trozos de fruta de ~1 m, a escala de hormiga.
    static void Food(Transform root, bool leaves, int n, float spread)
    {
        Material crumb = Mat("Micro_Miga", new Color(0.9f, 0.75f, 0.45f)), fruit = Mat("Micro_Fruta", new Color(0.85f, 0.3f, 0.25f)), leaf = Mat("Micro_Hoja", new Color(0.35f, 0.6f, 0.25f));
        var rnd = new System.Random(n * 31 + (leaves ? 7 : 0));
        for (int i = 0; i < n; i++)
        {
            float a = (float)rnd.NextDouble() * Mathf.PI * 2f, r = 14f + (float)rnd.NextDouble() * (spread - 14f);
            Vector3 p = new Vector3(Mathf.Cos(a) * r, 0.4f, Mathf.Sin(a) * r);
            Transform f = Box(root, leaves ? "Hoja" : "Resto_comida", p, leaves ? new Vector3(1.6f, 0.08f, 1f) : Vector3.one * (0.6f + (float)rnd.NextDouble() * 0.5f),
                              Quaternion.Euler(0f, (float)rnd.NextDouble() * 180f, 0f), leaves ? leaf : (i % 3 == 0 ? fruit : crumb), false);
            var mf = f.gameObject.AddComponent<MicroFood>(); mf.leaf = leaves; mf.spread = spread * 0.8f;
        }
    }

    // FLORA y FAUNA a escala de hormiga por toda la superficie (docs/natural-interactions.md). Nada es solo decorado:
    // cada planta, piedra o mancha de arena lleva el Affordance de lo que los insectos hacen con ella de verdad.
    //   · Brizna (pasto): hierba que comen saltamontes y grillos y que cortan las termitas; savia para el pulgón; posadero
    //     para asolearse (saltamontes) o tender la tela (araña); y espiga de SEMILLAS que cosechan los mirmidones.
    //   · Helecho: savia, hojas que la tribu lleva al jardín de hongos, posadero de la mantis y refugio bajo sus frondas.
    //   · Hongo: comida de las termitas cultivadoras y refugio bajo el sombrero.
    //   · Flor: polen/néctar (mariquita) y el sitio donde la mantis acecha a los que vienen a la flor.
    //   · Musgo y piedra: refugio húmedo (grillo en su madriguera, mariquitas agregadas).
    //   · Arena fina: donde la hormiga león cava su embudo. · Boñiga: lo que rueda el pelotero.
    /// <param name="floraSlots">Las plazas de flora del área (PlatformSlots, ya ×10, en el espacio de la raíz): dentro del área la
    /// flora crece solo ahí. <paramref name="floraUnderStory"/> recibe las que tapa la historia.</param>
    /// <param name="faunaSlots">Las plazas de fauna del área (ídem): el bicho que caería dentro del área va a la siguiente.</param>
    static int Nature(Transform root, Vector3 center, float r1, int seed, Bounds story, Bounds? area, List<Renderer> floors, MicroBiome.Biome biome = null,
                      List<Vector3> floraSlots = null, List<Vector3> faunaSlots = null, List<int> floraUnderStory = null)
    {
        var rnd = new System.Random(seed);
        float biomeWeight = 0f; if (biome != null) foreach (MicroBiome.Plant pl in biome.plants) biomeWeight += pl.weight;
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        Vector3 Ring(float a0, float a1) { float a = R(0f, Mathf.PI * 2f), r = R(a0, a1); return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r); }
        var flora = new GameObject("Flora_a_escala").transform; flora.SetParent(root, false); flora.localPosition = center;
        Material grass = Mat("Micro_Hierba", new Color(0.62f, 0.68f, 0.3f)), grassDry = Mat("Micro_Hierba_seca", new Color(0.78f, 0.72f, 0.42f));
        Material fern = Mat("Micro_Helecho", new Color(0.25f, 0.5f, 0.25f)), fernStem = Mat("Micro_Helecho_tallo", new Color(0.3f, 0.4f, 0.2f));
        Material stem = Mat("Micro_Pie_Seta", new Color(0.9f, 0.86f, 0.75f)), cap = Mat("Micro_Sombrero", new Color(0.7f, 0.35f, 0.2f));
        Material moss = Mat("Micro_Musgo", new Color(0.3f, 0.45f, 0.2f)), mossTip = Mat("Micro_Musgo_punta", new Color(0.45f, 0.6f, 0.25f));
        Material rock = Mat("Micro_Grano", new Color(0.55f, 0.52f, 0.48f)), seedMat = Mat("Micro_Espiga", new Color(0.82f, 0.7f, 0.42f));
        Material petal = Mat("Micro_Petalo", new Color(0.97f, 0.95f, 0.9f)), petal2 = Mat("Micro_Petalo_lila", new Color(0.8f, 0.6f, 0.9f)), pollen = Mat("Micro_Polen", new Color(0.98f, 0.8f, 0.2f));
        Material sand = Mat("Micro_Arena", new Color(0.82f, 0.72f, 0.5f)), dung = Mat("Micro_Estiercol", new Color(0.35f, 0.25f, 0.15f));

        // Rejilla con jitter (una planta por celda, sin amontonarse) sobre todo el disco hasta el agua.
        const float cell = 12f, storyClear = 20f;
        var sandPits = new List<Vector3>(); var dungs = new List<Vector3>();
        int placed0 = 0;
        for (float x = -r1; x <= r1; x += cell)
            for (float z = -r1; z <= r1; z += cell)
            {
                Vector3 p = new Vector3(x + R(-cell * 0.4f, cell * 0.4f), 0f, z + R(-cell * 0.4f, cell * 0.4f));
                if (p.magnitude > r1) continue;
                Vector3 w = center + p;   // posición en el espacio del microcosmos (el área gigante está en el mismo)
                if (new Vector2(w.x - story.center.x, w.z - story.center.z).magnitude < storyClear || InsideXZ(story, w, 4f)) continue;   // el claro de la historia
                // Con posiciones reservadas, dentro del área solo crece en sus plazas de flora (abajo).
                if (floraSlots != null && area.HasValue && InsideXZ(area.Value, w, 4f)) continue;
                if (area.HasValue && InsideXZ(area.Value, w, 4f))
                {
                    // Dentro del área: solo sobre su suelo libre (entre muebles), a la altura de ese suelo.
                    if (floors.Count > 0 && EdgeDist(w, floors) < 2f) continue;
                    if (NearBlocked(w, 3f)) continue;
                    // En SU planta (la más cercana a la altura del área): antes tomaba el suelo más alto y en el hospital (3 plantas)
                    // las plantas salían en el piso de arriba o el tejado, fuera del área (Berón 2026-10-27).
                    float fy = FloorNear(w, floors, center.y);
                    if (Mathf.Abs(fy - center.y) > 1.5f) continue;
                    p.y = fy - center.y;
                }
                if (Sow(p, false)) placed0++;
            }

        // La flora DEL ÁREA, en sus POSICIONES RESERVADAS (PlatformSlots.flora, ya ×10): una planta en cada una, a ras del suelo
        // del área (el más cercano a la altura de la plaza), menos las que tapa la historia (su claro).
        int onSlots = 0;
        if (floraSlots != null)
            for (int i = 0; i < floraSlots.Count; i++)
            {
                Vector3 w = floraSlots[i];
                if (new Vector2(w.x - story.center.x, w.z - story.center.z).magnitude < storyClear || InsideXZ(story, w, 4f)) { floraUnderStory?.Add(i); continue; }
                float fy = FloorNear(w, floors, w.y);
                if (Sow(new Vector3(w.x - center.x, fy - center.y, w.z - center.z), true)) { placed0++; onSlots++; }
            }

        // Una planta (o piedra, arena, boñiga) en p; false = suelo desnudo. <paramref name="always"/>: una plaza de flora siempre
        // lleva algo.
        bool Sow(Vector3 p, bool always)
        {
            if (biome != null)   // FLORA NATIVA de su era y región (MicroBiome); un 12 % de suelo desnudo para pasar
            {
                if (R(0f, 1f) < 0.12f && !always) return false;
                float pick = R(0f, biomeWeight);
                foreach (MicroBiome.Plant pl in biome.plants) { pick -= pl.weight; if (pick <= 0f) { NativePlant(pl, p); break; } }
                return true;
            }
            float roll = R(0f, always ? 89f : 100f);
            if (roll < 27f) Grass(p);
            else if (roll < 40f) Moss(p);
            else if (roll < 50f) Fungus(p);
            else if (roll < 59f) Fern(p);
            else if (roll < 67f) Flower(p);
            else if (roll < 81f) Stone(p);
            else if (roll < 86f) { Transform a = Plant("Arena_fina", p); Box(a, "Arena", Vector3.up * 0.03f, new Vector3(9f, 0.06f, 9f), Quaternion.identity, sand, false); Affordance.Put(a.gameObject, AffordanceKind.SandPit, 1); sandPits.Add(p); }
            else if (roll < 89f) { Transform d = Plant("Boñiga", p); Box(d, "Estiercol", Vector3.up * 0.9f, new Vector3(4f, 1.8f, 3.5f), Quaternion.identity, dung, true); Affordance.Put(d.gameObject, AffordanceKind.Dung, 2, 3f, 0.001f); dungs.Add(p); }
            else return false;   // suelo desnudo: también hace falta sitio para pasar
            return true;
        }

        // Una planta NATIVA: su forma según el tipo (gramínea, musgo, seta, helecho, flor, mata, piedra, arena, boñiga,
        // escarcha), su porte real (1 cm ≈ 0,25 m) y sus colores; el nodo lleva su nombre común y científico.
        void NativePlant(MicroBiome.Plant pl, Vector3 p)
        {
            Material a = Mat("Bio_" + pl.name + "_1", pl.main), b = Mat("Bio_" + pl.name + "_2", pl.accent);
            float hh = Mathf.Clamp(pl.cm * 0.25f, 2f, 40f) * R(0.8f, 1.2f);
            Transform t = Plant(pl.name, p);
            switch (pl.kind)
            {
                case MicroBiome.Kind.Grass:
                {
                    float lean = R(-18f, 18f);
                    Box(t, "Tallo", Vector3.up * hh * 0.3f, new Vector3(0.25f, hh * 0.6f, 1.2f), Quaternion.Euler(lean * 0.4f, 0f, 0f), a, true);
                    Box(t, "Punta", new Vector3(0f, hh * 0.78f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.25f), new Vector3(0.2f, hh * 0.4f, 0.8f), Quaternion.Euler(lean, 0f, 0f), a, false);
                    Box(t, "Espiga", new Vector3(0f, hh * 0.95f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.35f), new Vector3(0.7f, Mathf.Max(1.5f, hh * 0.12f), 1.2f), Quaternion.Euler(lean, 0f, 0f), b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Grass, 2, 1f, 0.004f);
                    Affordance.Put(t.gameObject, AffordanceKind.PlantSap, 3, 1f, 0.01f);
                    Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
                    Transform seeds = Plant("Semillas", p + new Vector3(1.6f, 0f, 0f));
                    Box(seeds, "Semilla", Vector3.up * 0.25f, new Vector3(0.6f, 0.4f, 0.8f), Quaternion.identity, b, false);
                    Affordance.Put(seeds.gameObject, AffordanceKind.Seed, 2, 3f, 0.003f);
                    break;
                }
                case MicroBiome.Kind.Moss:
                    Box(t, "Cojin", Vector3.up * Mathf.Min(hh, 3f) * 0.25f, new Vector3(4f, Mathf.Min(hh, 3f) * 0.5f, 4f), Quaternion.identity, a, false);
                    for (int k = 0; k < 3; k++) Box(t, "Punta", new Vector3(R(-1.4f, 1.4f), Mathf.Min(hh, 3f) * 0.55f, R(-1.4f, 1.4f)), new Vector3(0.3f, 0.5f, 0.3f), Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                case MicroBiome.Kind.Fungus:
                    Box(t, "Pie", Vector3.up * hh * 0.5f, new Vector3(hh * 0.15f, hh, hh * 0.15f), Quaternion.identity, a, true);
                    Box(t, "Sombrero", Vector3.up * hh, new Vector3(hh * 0.8f, hh * 0.18f, hh * 0.8f), Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Fungus, 3, 2f, 0.003f);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                case MicroBiome.Kind.Fern:
                    Box(t, "Tallo", Vector3.up * hh * 0.5f, new Vector3(0.6f, hh, 0.6f), Quaternion.identity, b, true);
                    for (int k = 0; k < 7; k++) Box(t, "Fronda", Vector3.up * hh + Quaternion.Euler(0f, k * 51f, 0f) * new Vector3(0f, -1.5f, hh * 0.25f), new Vector3(2.4f, 0.2f, hh * 0.5f), Quaternion.Euler(-20f, k * 51f, 0f), a, false);
                    Affordance.Put(t.gameObject, AffordanceKind.PlantSap, 4, 1f, 0.01f);
                    Affordance.Put(t.gameObject, AffordanceKind.Leaf, 3, 3f, 0.002f);
                    Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                case MicroBiome.Kind.Flower:
                    Box(t, "Tallo", Vector3.up * hh * 0.5f, new Vector3(0.4f, hh, 0.4f), Quaternion.identity, a, true);
                    for (int k = 0; k < 5; k++) Box(t, "Petalo", Vector3.up * hh + Quaternion.Euler(0f, k * 72f, 0f) * new Vector3(0f, 0f, 2.2f), new Vector3(2f, 0.15f, 3.2f), Quaternion.Euler(-12f, k * 72f, 0f), b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Flower, 2, 1f, 0.004f);
                    Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
                    break;
                case MicroBiome.Kind.Shrub:   // mata leñosa: un cojín de hojas con sus flores o bayas
                    Box(t, "Mata", Vector3.up * hh * 0.45f, new Vector3(hh * 0.8f, hh * 0.9f, hh * 0.8f), Quaternion.identity, a, true);
                    for (int k = 0; k < 5; k++) Box(t, "Flor_o_baya", new Vector3(R(-0.35f, 0.35f) * hh, hh * R(0.6f, 0.92f), R(-0.35f, 0.35f) * hh), Vector3.one * Mathf.Max(0.6f, hh * 0.07f), Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Leaf, 3, 3f, 0.002f);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
                    break;
                case MicroBiome.Kind.Stone:
                {
                    float sz = R(1.5f, 6f);
                    Box(t, "Roca", Vector3.up * sz * 0.4f, new Vector3(sz, sz * 0.8f, sz * 1.1f), Quaternion.Euler(R(0f, 25f), 0f, R(0f, 15f)), a, true);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                }
                case MicroBiome.Kind.Sand:
                    Box(t, "Arena", Vector3.up * 0.03f, new Vector3(9f, 0.06f, 9f), Quaternion.identity, a, false);
                    Affordance.Put(t.gameObject, AffordanceKind.SandPit, 1); sandPits.Add(p);
                    break;
                case MicroBiome.Kind.Dung:
                    Box(t, "Bulto", Vector3.up * 0.9f, new Vector3(4f, 1.8f, 3.5f), Quaternion.identity, a, true);
                    Affordance.Put(t.gameObject, AffordanceKind.Dung, 2, 3f, 0.001f); dungs.Add(p);
                    break;
                case MicroBiome.Kind.Snow:
                    Box(t, "Escarcha", Vector3.up * 0.04f, new Vector3(R(8f, 14f), 0.08f, R(8f, 14f)), Quaternion.identity, a, false);
                    break;
            }
        }

        // Cada planta: un nodo SIN escala en su pie (lleva los Affordance) y sus piezas como hijos (sin deformarse).
        Transform Plant(string name, Vector3 p) { var g = new GameObject(name).transform; g.SetParent(flora, false); g.localPosition = p; g.localRotation = Quaternion.Euler(0f, R(0f, 360f), 0f); return g; }

        void Grass(Vector3 p)
        {
            float hh = R(10f, 25f), lean = R(-18f, 18f);
            Transform b = Plant("Brizna", p);
            Box(b, "Tallo", Vector3.up * hh * 0.3f, new Vector3(0.25f, hh * 0.6f, 1.2f), Quaternion.Euler(lean * 0.4f, 0f, 0f), rnd.Next(3) == 0 ? grassDry : grass, true);
            Box(b, "Punta", new Vector3(0f, hh * 0.78f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.25f), new Vector3(0.2f, hh * 0.4f, 0.8f), Quaternion.Euler(lean, 0f, 0f), grass, false);
            Affordance.Put(b.gameObject, AffordanceKind.Grass, 2, 1f, 0.004f);
            Affordance.Put(b.gameObject, AffordanceKind.PlantSap, 3, 1f, 0.01f);
            Affordance.Put(b.gameObject, AffordanceKind.Perch, 1);
            if (rnd.Next(2) == 0)
            {
                // Espiga madura: las semillas caen al pie y los mirmidones las cosechan.
                Box(b, "Espiga", new Vector3(0f, hh * 0.95f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.35f), new Vector3(0.7f, 2.5f, 1.2f), Quaternion.Euler(lean, 0f, 0f), seedMat, false);
                Transform seeds = Plant("Semillas", p + new Vector3(1.6f, 0f, 0f));
                Box(seeds, "Semilla", Vector3.up * 0.25f, new Vector3(0.6f, 0.4f, 0.8f), Quaternion.identity, seedMat, false);
                Box(seeds, "Semilla", new Vector3(0.7f, 0.2f, 0.3f), new Vector3(0.5f, 0.35f, 0.7f), Quaternion.Euler(0f, 40f, 0f), seedMat, false);
                Affordance.Put(seeds.gameObject, AffordanceKind.Seed, 2, 3f, 0.003f);
            }
        }
        void Moss(Vector3 p)
        {
            Transform m = Plant("Musgo", p);
            Box(m, "Cojin", Vector3.up * 0.5f, new Vector3(3f, 1f, 3f), Quaternion.identity, moss, false);
            for (int k = 0; k < 3; k++) Box(m, "Musgo_punta", new Vector3(R(-1f, 1f), 1.15f, R(-1f, 1f)), new Vector3(0.25f, 0.5f, 0.25f), Quaternion.identity, mossTip, false);
            Affordance.Put(m.gameObject, AffordanceKind.Shelter, 2);
        }
        void Fungus(Vector3 p)
        {
            float hh = R(2f, 6f);
            Transform f = Plant("Hongo", p);
            Box(f, "Pie", Vector3.up * hh * 0.5f, new Vector3(hh * 0.15f, hh, hh * 0.15f), Quaternion.identity, stem, true);
            Box(f, "Sombrero", Vector3.up * hh, new Vector3(hh * 0.8f, hh * 0.18f, hh * 0.8f), Quaternion.identity, cap, false);
            Affordance.Put(f.gameObject, AffordanceKind.Fungus, 3, 2f, 0.003f);
            Affordance.Put(f.gameObject, AffordanceKind.Shelter, 2);
        }
        void Fern(Vector3 p)
        {
            float hh = R(12f, 22f);
            Transform t = Plant("Helecho", p);
            Box(t, "Tallo", Vector3.up * hh * 0.5f, new Vector3(0.6f, hh, 0.6f), Quaternion.identity, fernStem, true);
            for (int k = 0; k < 7; k++) Box(t, "Fronda", Vector3.up * hh + Quaternion.Euler(0f, k * 51f, 0f) * new Vector3(0f, -1.5f, 5f), new Vector3(2.4f, 0.2f, 11f), Quaternion.Euler(-20f, k * 51f, 0f), fern, false);
            Affordance.Put(t.gameObject, AffordanceKind.PlantSap, 4, 1f, 0.01f);
            Affordance.Put(t.gameObject, AffordanceKind.Leaf, 3, 3f, 0.002f);
            Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
            Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
        }
        void Flower(Vector3 p)
        {
            float hh = R(8f, 16f);
            Transform t = Plant("Flor", p);
            Box(t, "Tallo", Vector3.up * hh * 0.5f, new Vector3(0.4f, hh, 0.4f), Quaternion.identity, fernStem, true);
            Material pm = rnd.Next(2) == 0 ? petal : petal2;
            for (int k = 0; k < 5; k++) Box(t, "Petalo", Vector3.up * hh + Quaternion.Euler(0f, k * 72f, 0f) * new Vector3(0f, 0f, 2.2f), new Vector3(2f, 0.15f, 3.2f), Quaternion.Euler(-12f, k * 72f, 0f), pm, false);
            Box(t, "Centro", Vector3.up * (hh + 0.2f), new Vector3(1.8f, 0.5f, 1.8f), Quaternion.identity, pollen, false);
            Affordance.Put(t.gameObject, AffordanceKind.Flower, 2, 1f, 0.004f);
            Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
        }
        void Stone(Vector3 p)
        {
            float sz = R(1.5f, 6f);
            Transform g = Plant("Piedra", p);
            Box(g, "Roca", Vector3.up * sz * 0.4f, new Vector3(sz, sz * 0.8f, sz * 1.1f), Quaternion.Euler(R(0f, 25f), 0f, R(0f, 15f)), rock, true);
            Affordance.Put(g.gameObject, AffordanceKind.Shelter, 2);
        }

        // FAUNA: cada bicho cuelga de un padre MicroSize que lo mantiene a su tamaño relativo a la hormiga (el prefab
        // fija su propia escala al nacer; por eso antes se veían diminutos).
        var fauna = new GameObject("Fauna_a_escala").transform; fauna.SetParent(root, false); fauna.localPosition = center;
        (string sp, int n, float len)[] generic = { ("Antlion", 5, 3f), ("DungBeetle", 5, 5f), ("Termite", 10, 1.6f), ("Mantis", 3, 10f), ("Cricket", 5, 5f), ("Grasshopper", 5, 7f), ("Aphid", 8, 1f), ("Ladybug", 5, 2.4f) };
        // Con bioma, su FAUNA NATIVA (activa en la estación de la historia): cada especie real con el cuerpo de su grupo.
        var list = new List<(string sp, int n, float len, Color tint, string name, string dormant)>();
        if (biome != null) foreach (MicroBiome.Bug bg in biome.bugs) list.Add((bg.species, bg.count, bg.Length, bg.tint, bg.name, bg.dormant));
        else foreach (var (gs, gn, gl) in generic) list.Add((gs, gn, gl, SpeciesColor(gs), gs, null));
        int placed = 0, faunaNext = 0;
        foreach (var (sp, n, len, tint, label, dormant) in list)
        {
            GameObject prefab = AnimalPrefabGenerator.LoadOrCreate(sp);   // lo genera si falta (especies nuevas)
            for (int i = 0; i < n; i++)
            {
                // Cada uno nace junto a lo que usa: la hormiga león en la arena fina, el pelotero junto a una boñiga.
                Vector3 p = sp == "Antlion" && sandPits.Count > 0 ? sandPits[rnd.Next(sandPits.Count)]
                          : sp == "DungBeetle" && dungs.Count > 0 ? dungs[rnd.Next(dungs.Count)] + new Vector3(5f, 0f, 0f)
                          : Ring(storyClear, r1 - 10f);
                // Con posiciones reservadas: el que caería DENTRO del área (sobre el suelo gigante o encima de un mueble) va a la
                // siguiente plaza de fauna; si ya están todas tomadas, fuera del área.
                bool ringed = !(sp == "Antlion" && sandPits.Count > 0) && !(sp == "DungBeetle" && dungs.Count > 0);
                if (ringed && faunaSlots != null && area.HasValue && InsideXZ(area.Value, center + p, 4f))
                {
                    if (faunaNext < faunaSlots.Count) { Vector3 w = faunaSlots[faunaNext++]; p = new Vector3(w.x - center.x, FloorNear(w, floors, w.y) - center.y, w.z - center.z); }
                    else for (int k = 0; k < 12 && InsideXZ(area.Value, center + p, 4f); k++) p = Ring(storyClear, r1 - 10f);
                }
                if (prefab == null) continue;
                var holder = new GameObject($"{label}_a_escala").transform; holder.SetParent(fauna, false); holder.localPosition = p;
                holder.gameObject.AddComponent<MicroSize>().length = len;
                GameObject a = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                a.transform.SetParent(holder, false); a.transform.localPosition = Vector3.zero;
                // FORMA REAL (docs/insect-morphology.md): el cuerpo de cubos articulado de su especie sustituye al modelo;
                // la conducta, el ciclo de vida y la biología siguen siendo los del prefab.
                foreach (Renderer x in a.GetComponentsInChildren<Renderer>(true)) x.enabled = false;
                // POBLACIÓN MEZCLADA: la mayoría ADULTOS (erguidos cuando están lúcidos) y algunas crías (larva, ninfa o
                // joven según su desarrollo). Antes el prefab nacía sin etapa → todos jóvenes, nadie erguido.
                Animal an = a.GetComponent<Animal>();
                if (an != null) an.lifeStage = i < Mathf.Max(1, Mathf.CeilToInt(n * 0.7f)) ? LifeStage.adult : LifeStage.child;
                CubeInsect.Dress(a, sp, tint, 1f, an != null ? Metamorphosis.PhaseOf(an) : Metamorphosis.Phase.Adult, an == null || an.sex != Sex.male);
                if (dormant != null) a.AddComponent<Dormant>().why = dormant;   // aletargado en esta estación: está, pero quieto
                Bounds bb = default; bool any = false;
                foreach (Renderer x in a.GetComponentsInChildren<Renderer>()) { if (!x.enabled) continue; if (!any) { bb = x.bounds; any = true; } else bb.Encapsulate(x.bounds); }
                // A su largo de cuerpo (el de la raíz «Insecto», sin la caja de patas y alas), como lo mantiene MicroSize.
                Transform ins = a.transform.Find("Insecto");
                if (ins != null && ins.lossyScale.z > 1e-6f) holder.localScale = Vector3.one * (len / ins.lossyScale.z);
                else if (any) { float now = Mathf.Max(bb.size.x, bb.size.z); if (now > 1e-4f) holder.localScale = Vector3.one * (len / now); }
                placed++;
            }
        }
        _faunaOnSlots = faunaNext;
        Debug.Log($"[Microcosmos] Flora interactuable: {placed0} plantas, piedras, arenas y boñigas por toda la superficie (radio {r1:0} m{(floraSlots != null ? $"; {onSlots} en las plazas de flora del área" : "")})" + (biome != null ? $" · bioma: {biome.title}, {biome.plants.Length} plantas y {biome.bugs.Length} bichos nativos." : "."));
        return placed;
    }

    static int _faunaOnSlots;   // cuántos bichos fueron a las plazas de fauna (el último Nature)

    static bool InsideXZ(Bounds b, Vector3 p, float margin) => p.x > b.min.x - margin && p.x < b.max.x + margin && p.z > b.min.z - margin && p.z < b.max.z + margin;

    static bool NearBlocked(Vector3 p, float margin)
    {
        foreach (Bounds b in _blocked) if (p.x > b.min.x - margin && p.x < b.max.x + margin && p.z > b.min.z - margin && p.z < b.max.z + margin) return true;
        return false;
    }

    static Color SpeciesColor(string sp)
    {
        switch (sp)
        {
            case "Ladybug": return new Color(0.85f, 0.12f, 0.1f);
            case "DungBeetle": return new Color(0.15f, 0.12f, 0.1f);
            case "Grasshopper": return new Color(0.45f, 0.62f, 0.25f);
            case "Cricket": return new Color(0.22f, 0.16f, 0.1f);
            case "Mantis": return new Color(0.4f, 0.68f, 0.3f);
            case "Antlion": return new Color(0.62f, 0.52f, 0.38f);
            case "Termite": return new Color(0.9f, 0.85f, 0.72f);
            case "Aphid": return new Color(0.55f, 0.78f, 0.35f);
            default: return new Color(0.3f, 0.2f, 0.15f);
        }
    }

    internal static Transform Box(Transform parent, string name, Vector3 local, Vector3 size, Material m, bool solid) => Box(parent, name, local, size, Quaternion.identity, m, solid);

    internal static Transform Box(Transform parent, string name, Vector3 local, Vector3 size, Quaternion rot, Material m, bool solid)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localRotation = rot; g.transform.localScale = size;
        if (!solid) Object.DestroyImmediate(g.GetComponent<Collider>());
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g.transform;
    }

    static readonly Dictionary<string, Material> _mats = new Dictionary<string, Material>();
    internal static Material Mat(string name, Color c)
    {
        if (_mats.TryGetValue(name, out Material cached) && cached != null) return cached;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh) { name = name, color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return _mats[name] = m;
    }
}
