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
    internal static void Build(string areaGo, string display, SampleSceneBuilder.MissionKind kind, Color tint, Vector3 o, System.Func<Transform, int> storyOverride = null)
    {
        var root = new GameObject($"Microcosmos_{display}");
        int dressed = 0;

        // 1) El ÁREA del santuario, ×10, ENTERA (sin conducta): el microcosmos ES el área vista desde el suelo, con sus
        //    muebles convertidos en montañas. Antes se vaciaba un claro de 50 m en el centro para la historia, y en la
        //    cocina (cocina abierta en el centro, comedor alrededor) eso borraba justo la cocina: ya no se reconocía.
        GameObject src = GameObject.Find(areaGo), giant = null;
        if (src != null)
        {
            giant = Object.Instantiate(src);
            giant.name = $"{display}_gigante";
            foreach (MonoBehaviour mb in giant.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(mb);
            foreach (string n in new[] { "Estructura_Elevada" }) { Transform t = giant.transform.Find(n); if (t != null) Object.DestroyImmediate(t.gameObject); }
            foreach (Transform t in giant.GetComponentsInChildren<Transform>(true)) if (t != null && t.name.EndsWith("_VirtualizationMachine")) Object.DestroyImmediate(t.gameObject);
            giant.transform.SetParent(root.transform, false);
            giant.transform.localPosition = Vector3.zero; giant.transform.localRotation = Quaternion.identity;
            giant.transform.localScale = Vector3.one * AreaScale;
            foreach (Transform c in giant.transform)
                if (c.name == "Mat" || c.name.StartsWith("Suelo")) c.localPosition = new Vector3(c.localPosition.x, -0.02f, c.localPosition.z);   // su suelo, bajo el resto
        }
        else Debug.LogWarning($"[Microcosmos] No encuentro {areaGo} (abre la escena principal): sin área gigante.");

        // 2) Dónde va la HISTORIA: el hueco más libre del SUELO del área (entre mesas, junto a la barra…), no su centro.
        Bounds foot = new Bounds(Vector3.zero, new Vector3(100f, 1f, 100f));
        var floors = new List<Renderer>(); _blocked.Clear();
        if (giant != null) foot = Classify(giant, floors, _blocked);
        Vector3 anchor = Vector3.zero; float free = 50f;
        if (giant != null) anchor = FreeSpot(foot, _blocked, out free);
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

        Transform cast = new GameObject("Elenco").transform; cast.SetParent(story, false);
        if (storyOverride != null) dressed = storyOverride(cast);
        else if (kind == SampleSceneBuilder.MissionKind.Heal) dressed = InfirmaryStory(cast);
        else if (kind == SampleSceneBuilder.MissionKind.KitchenChannel) dressed = KitchenStory(cast);
        else if (kind == SampleSceneBuilder.MissionKind.GardenChannel) dressed = GardenStory(cast);
        else dressed = TribeStory(cast, display, 8);

        // 3) Solo se aparta el mueble que pisa la historia (una mesa, una silla), no el área.
        int moved = giant != null ? ClearUnder(story, giant.transform, foot) : 0;
        if (giant == null)
            Box(root.transform, "Suelo_claro", new Vector3(0f, -0.05f, 0f), new Vector3(120f, 0.1f, 120f), Mat("Micro_Tierra", new Color(0.45f, 0.36f, 0.25f)), true);

        // 4) Fuera del área (alrededor, hasta el agua): el bosque a escala de hormiga — musgo = hierba, briznas y helechos =
        //    árboles — y su fauna. Dentro del área no crece nada: es la sala del santuario.
        float r0 = giant != null ? Mathf.Max(foot.extents.x, foot.extents.z) * 1.15f + 8f : 52f, r1 = r0 + 110f;
        float groundY = giant != null ? FloorAt(foot.center, floors, 0f) - 0.02f : 0f;
        Box(root.transform, "Suelo_bosque", new Vector3(foot.center.x, groundY - 0.05f, foot.center.z), new Vector3(r1 * 2f + 20f, 0.1f, r1 * 2f + 20f), Mat("Micro_Tierra", new Color(0.45f, 0.36f, 0.25f)), true);
        int animals = Nature(root.transform, new Vector3(foot.center.x, groundY, foot.center.z), r0, r1, giant == null, display.GetHashCode());

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
        float span = r1 * 2f + 20f;
        nav.center = Vector3.up * 15f; nav.size = new Vector3(span, 60f, span);
        nav.BuildNavMesh();
        Debug.Log($"[Microcosmos] {display}: área ×{AreaScale} entera (se apartan {moved} muebles bajo la historia, hueco libre {free:0} m), {dressed} mirmidones e insectos de la historia y {animals} animales a escala de hormiga.");
    }

    // ── Dónde cabe la historia dentro del área ───────────────────────────────────────────────────────────────────

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
            if (IsFloor(b)) floors.Add(r); else blocked.Add(b);
        }
        return any ? foot : new Bounds(Vector3.zero, new Vector3(100f, 1f, 100f));
    }

    static float DistXZ(Bounds b, Vector3 p)
    {
        float dx = Mathf.Max(b.min.x - p.x, 0f, p.x - b.max.x), dz = Mathf.Max(b.min.z - p.z, 0f, p.z - b.max.z);
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    /// <summary>El punto del suelo del área más alejado de cualquier mueble (rejilla de 6 m).</summary>
    static Vector3 FreeSpot(Bounds foot, List<Bounds> blocked, out float free)
    {
        Vector3 best = new Vector3(foot.center.x, 0f, foot.center.z); free = 0f;
        if (blocked.Count == 0) { free = Mathf.Min(foot.extents.x, foot.extents.z); return best; }
        float inset = Mathf.Min(10f, Mathf.Min(foot.extents.x, foot.extents.z) * 0.5f);
        for (float x = foot.min.x + inset; x <= foot.max.x - inset; x += 6f)
            for (float z = foot.min.z + inset; z <= foot.max.z - inset; z += 6f)
            {
                var p = new Vector3(x, 0f, z); float d = float.MaxValue;
                foreach (Bounds b in blocked) { d = Mathf.Min(d, DistXZ(b, p)); if (d <= free) break; }
                if (d > free) { free = d; best = p; }
            }
        return best;
    }

    /// <summary>Altura del suelo del área en ese punto (la cara de arriba del plano más alto que lo cubre).</summary>
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
    static int ClearUnder(Transform story, Transform giant, Bounds foot)
    {
        Bounds core = default; bool any = false;
        foreach (Renderer r in story.GetComponentsInChildren<Renderer>(true))
        {
            if (r.GetComponentInParent<MicroFood>() != null) continue;   // los restos se recolocan luego
            if (!any) { core = r.bounds; any = true; } else core.Encapsulate(r.bounds);
        }
        if (!any) return 0;
        core.Expand(new Vector3(2f, 0f, 2f));
        var kill = new HashSet<GameObject>();
        foreach (Renderer r in giant.GetComponentsInChildren<Renderer>())
        {
            Bounds b = r.bounds;
            if (IsFloor(b) || b.max.x < core.min.x || b.min.x > core.max.x || b.max.z < core.min.z || b.min.z > core.max.z) continue;
            kill.Add(Piece(r.transform, giant, foot).gameObject);
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
        for (int k = 0; k < 4; k++) Box(l, "Monticulo", new Vector3(0f, 0.6f + k * 1.1f, -3f), new Vector3(12f - k * 2.6f, 1.2f, 9f - k * 2f), earth, true);
        Box(l, "Entrada", new Vector3(0f, 1.2f, 1.55f), new Vector3(2.4f, 2.4f, 0.1f), dark, false);
        l.gameObject.AddComponent<MicroLair>();
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

    // FLORA y FAUNA a escala de hormiga, en el anillo entre el claro y el borde.
    static int Nature(Transform root, Vector3 center, float r0, float r1, bool openGround, int seed)
    {
        var rnd = new System.Random(seed);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        Vector3 Ring(float a0, float a1) { float a = R(0f, Mathf.PI * 2f), r = R(a0, a1); return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r); }
        var flora = new GameObject("Flora_a_escala").transform; flora.SetParent(root, false); flora.localPosition = center;
        Material grass = Mat("Micro_Hierba", new Color(0.62f, 0.68f, 0.3f)), grassDry = Mat("Micro_Hierba_seca", new Color(0.78f, 0.72f, 0.42f));
        Material fern = Mat("Micro_Helecho", new Color(0.25f, 0.5f, 0.25f)), fernStem = Mat("Micro_Helecho_tallo", new Color(0.3f, 0.4f, 0.2f));
        Material stem = Mat("Micro_Pie_Seta", new Color(0.9f, 0.86f, 0.75f)), cap = Mat("Micro_Sombrero", new Color(0.7f, 0.35f, 0.2f));
        Material moss = Mat("Micro_Musgo", new Color(0.3f, 0.45f, 0.2f)), mossTip = Mat("Micro_Musgo_punta", new Color(0.45f, 0.6f, 0.25f));
        Material rock = Mat("Micro_Grano", new Color(0.55f, 0.52f, 0.48f));
        for (int i = 0; i < 260; i++) { Vector3 p = Ring(i < 60 && openGround ? 16f : r0, r1); for (int k = 0; k < 4; k++) { float hh = R(0.4f, 1.2f); Box(flora, "Musgo", p + new Vector3(R(-0.6f, 0.6f), hh * 0.5f, R(-0.6f, 0.6f)), new Vector3(0.18f, hh, 0.18f), Quaternion.Euler(R(-8f, 8f), 0f, R(-8f, 8f)), k % 2 == 0 ? moss : mossTip, false); } }
        for (int i = 0; i < 45; i++) { Vector3 p = Ring(r0, r1); float hh = R(2f, 6f); Box(flora, "Hongo_pie", p + Vector3.up * hh * 0.5f, new Vector3(hh * 0.15f, hh, hh * 0.15f), Quaternion.identity, stem, true); Box(flora, "Hongo_sombrero", p + Vector3.up * hh, new Vector3(hh * 0.8f, hh * 0.18f, hh * 0.8f), Quaternion.Euler(0f, R(0f, 90f), 0f), cap, false); }
        for (int i = 0; i < 90; i++) { Vector3 p = Ring(r0, r1); float hh = R(10f, 25f), lean = R(-18f, 18f), yaw = R(0f, 180f); Material m = i % 3 == 0 ? grassDry : grass; Box(flora, "Brizna", p + Vector3.up * hh * 0.3f, new Vector3(0.25f, hh * 0.6f, 1.2f), Quaternion.Euler(lean * 0.4f, yaw, 0f), m, true); Box(flora, "Brizna_punta", p + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, hh * 0.78f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.25f), new Vector3(0.2f, hh * 0.4f, 0.8f), Quaternion.Euler(lean, yaw, 0f), m, false); }
        for (int i = 0; i < 25; i++) { Vector3 p = Ring(r0 + 10f, r1); float hh = R(12f, 22f); Box(flora, "Helecho_tallo", p + Vector3.up * hh * 0.5f, new Vector3(0.6f, hh, 0.6f), Quaternion.identity, fernStem, true); for (int k = 0; k < 7; k++) Box(flora, "Helecho_fronda", p + Vector3.up * hh + Quaternion.Euler(0f, k * 51f, 0f) * new Vector3(0f, -1.5f, 5f), new Vector3(2.4f, 0.2f, 11f), Quaternion.Euler(-20f, k * 51f, 0f), fern, false); }
        for (int i = 0; i < 70; i++) { Vector3 p = Ring(r0, r1); float s = R(1.5f, 6f); Box(flora, "Grano", p + Vector3.up * s * 0.4f, new Vector3(s, s * 0.8f, s * 1.1f), Quaternion.Euler(R(0f, 25f), R(0f, 90f), R(0f, 15f)), rock, true); }

        // FAUNA: cada bicho cuelga de un padre MicroSize que lo mantiene a su tamaño relativo a la hormiga (el prefab
        // fija su propia escala al nacer; por eso antes se veían diminutos).
        var fauna = new GameObject("Fauna_a_escala").transform; fauna.SetParent(root, false); fauna.localPosition = center;
        Material sand = Mat("Micro_Arena", new Color(0.82f, 0.72f, 0.5f)), dung = Mat("Micro_Estiercol", new Color(0.35f, 0.25f, 0.15f));
        (string sp, int n, float len)[] list = { ("Antlion", 5, 3f), ("DungBeetle", 5, 5f), ("Termite", 10, 1.6f), ("Mantis", 3, 10f), ("Cricket", 5, 5f), ("Grasshopper", 5, 7f), ("Aphid", 8, 1f), ("Ladybug", 5, 2.4f) };
        int placed = 0;
        foreach (var (sp, n, len) in list)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Animals/{sp}/{sp}.prefab");
            for (int i = 0; i < n; i++)
            {
                Vector3 p = Ring(r0, r1 - 10f);
                if (sp == "Antlion") for (int k = 0; k < 5; k++) Box(fauna, "Embudo_arena", p + Vector3.up * (0.02f + k * 0.02f), new Vector3(10f - k * 1.9f, 0.08f, 10f - k * 1.9f), Quaternion.Euler(0f, k * 18f, 0f), sand, false);
                if (sp == "DungBeetle") Box(fauna, "Bola_estiercol", p + new Vector3(4f, 2f, 0f), Vector3.one * 4f, Quaternion.Euler(R(0f, 90f), R(0f, 90f), 0f), dung, true);
                if (prefab == null) continue;
                var holder = new GameObject($"{sp}_a_escala").transform; holder.SetParent(fauna, false); holder.localPosition = p;
                holder.gameObject.AddComponent<MicroSize>().length = len;
                GameObject a = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                a.transform.SetParent(holder, false); a.transform.localPosition = Vector3.zero;
                // FORMA REAL (docs/insect-morphology.md): el cuerpo de cubos articulado de su especie sustituye al modelo;
                // la conducta, el ciclo de vida y la biología siguen siendo los del prefab.
                foreach (Renderer x in a.GetComponentsInChildren<Renderer>(true)) x.enabled = false;
                CubeInsect.Dress(a, sp, SpeciesColor(sp), 1f);
                Bounds bb = default; bool any = false;
                foreach (Renderer x in a.GetComponentsInChildren<Renderer>()) { if (!x.enabled) continue; if (!any) { bb = x.bounds; any = true; } else bb.Encapsulate(x.bounds); }
                if (any) { float now = Mathf.Max(bb.size.x, bb.size.z); if (now > 1e-4f) holder.localScale = Vector3.one * (len / now); }
                placed++;
            }
        }
        return placed;
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
