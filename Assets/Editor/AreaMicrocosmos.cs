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

    internal static void Build(string areaGo, string display, SampleSceneBuilder.MissionKind kind, Color tint, Vector3 o)
    {
        var root = new GameObject($"Microcosmos_{display}");
        float clear = 50f;
        int dressed = 0;

        // 1) La HISTORIA del área (en el claro central).
        if (kind == SampleSceneBuilder.MissionKind.Heal) dressed = InfirmaryStory(root.transform);
        else if (kind == SampleSceneBuilder.MissionKind.KitchenChannel) dressed = KitchenStory(root.transform);
        else if (kind == SampleSceneBuilder.MissionKind.GardenChannel) dressed = GardenStory(root.transform);
        else dressed = TribeStory(root.transform, display, 8);

        // 2) El ÁREA del santuario, ×10, alrededor (sin conducta), con el claro libre.
        GameObject src = GameObject.Find(areaGo);
        if (src != null)
        {
            GameObject giant = Object.Instantiate(src);
            giant.name = $"{display}_gigante";
            foreach (MonoBehaviour mb in giant.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(mb);
            foreach (string n in new[] { "Estructura_Elevada" }) { Transform t = giant.transform.Find(n); if (t != null) Object.DestroyImmediate(t.gameObject); }
            foreach (Transform t in giant.GetComponentsInChildren<Transform>(true)) if (t != null && t.name.EndsWith("_VirtualizationMachine")) Object.DestroyImmediate(t.gameObject);
            giant.transform.SetParent(root.transform, false);
            giant.transform.localPosition = Vector3.zero; giant.transform.localRotation = Quaternion.identity;
            giant.transform.localScale = Vector3.one * AreaScale;
            var kill = new List<GameObject>();
            foreach (Transform c in giant.transform)
            {
                if (c.name == "Mat" || c.name.StartsWith("Suelo")) { c.localPosition = new Vector3(c.localPosition.x, -0.02f, c.localPosition.z); continue; }   // su suelo, bajo el del claro
                if (c.childCount > 0 && (c.name == "Equipamiento" || c.name.StartsWith("Gimnasio") || c.name.StartsWith("Esterillas")))
                { foreach (Transform it in c) { Vector3 p = it.position; if (Mathf.Abs(p.x) < clear && Mathf.Abs(p.z) < clear) kill.Add(it.gameObject); } continue; }
                Vector3 q = c.position; if (Mathf.Abs(q.x) < clear && Mathf.Abs(q.z) < clear) kill.Add(c.gameObject);
            }
            foreach (GameObject g in kill) if (g != null) Object.DestroyImmediate(g);
        }
        else Debug.LogWarning($"[Microcosmos] No encuentro {areaGo} (abre la escena principal): sin área gigante.");

        // Suelo del claro.
        Box(root.transform, "Suelo_claro", new Vector3(0f, -0.05f, 0f), new Vector3(clear * 2f + 20f, 0.1f, clear * 2f + 20f), Mat("Micro_Tierra", new Color(0.45f, 0.36f, 0.25f)), true);

        // 3) Flora y fauna a escala de hormiga.
        int animals = Nature(root.transform, clear + 2f, 140f, display.GetHashCode());

        root.transform.position = o;

        // Entrada y salida (el portal muestra su diálogo al acercarse).
        var spawn = new GameObject("MobSpawnPoint"); spawn.transform.SetParent(root.transform, false);
        spawn.transform.localPosition = new Vector3(0f, 1f, -14f); spawn.transform.localRotation = Quaternion.LookRotation(Vector3.forward);
        spawn.AddComponent<MobSpawnPoint>();
        var portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "WorldExitPortal"; portal.transform.SetParent(root.transform, false);
        portal.transform.localPosition = new Vector3(0f, 1.5f, -20f); portal.transform.localScale = new Vector3(2f, 3f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = Mat("MobWorldExitPortal", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<WorldExitPortal>().label = "Salir del microcosmos";

        // Las misiones del área (yoga, textil, mecánica) siguen en su tablero.
        if (kind == SampleSceneBuilder.MissionKind.Yoga || kind == SampleSceneBuilder.MissionKind.Channel)
        {
            MobMission[] missions = SampleSceneBuilder.BuildAreaMissions(root.transform, root.transform, display, tint, kind);
            foreach (MobMission m in missions) { if (m == null) continue; m.transform.position = o + new Vector3(0f, 0f, 6f); foreach (MeditationMissionBase b in m.GetComponents<MeditationMissionBase>()) b.endMode = MissionEndMode.Standalone; }
            var board = root.AddComponent<MobWorldMissionBoard>(); board.areaName = display; board.missions = missions;
        }

        var navGo = new GameObject("NavMesh_Microcosmos"); navGo.transform.SetParent(root.transform, false);
        var nav = navGo.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
        nav.collectObjects = Unity.AI.Navigation.CollectObjects.Volume;
        float span = 300f; if (src != null) { Renderer[] rr = src.GetComponentsInChildren<Renderer>(); if (rr.Length > 0) { Bounds bb = rr[0].bounds; foreach (Renderer x in rr) bb.Encapsulate(x.bounds); span = Mathf.Max(300f, Mathf.Max(bb.size.x, bb.size.z) * AreaScale + 40f); } }
        nav.center = Vector3.up * 15f; nav.size = new Vector3(span, 60f, span);
        nav.BuildNavMesh();
        Debug.Log($"[Microcosmos] {display}: área ×{AreaScale}, {dressed} hormigas humanizadas/insectos de la historia y {animals} animales a escala de hormiga.");
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
            // Los depredadores siguen siendo INSECTOS (a escala de la hormiga); las hormigas, humanizadas.
            if (a.name.Contains("Mariquita")) { CubeInsect.Dress(a.gameObject, CubeInsect.Kind.Beetle, c, 2.4f); n++; continue; }
            if (a.name.Contains("Escarabajo")) { CubeInsect.Dress(a.gameObject, CubeInsect.Kind.Beetle, c, 5f); n++; continue; }
            if (a.name.Contains("Ara")) { CubeInsect.Dress(a.gameObject, CubeInsect.Kind.Spider, c, 4f); n++; continue; }
            CubeInsect.DressHuman(a.gameObject, c, aphid ? AntHeight * 1.2f : AntHeight, aphid ? 1.6f : 1f, aphid);
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
            Ant(root, $"Hormiga_{i + 1}", new Vector3(-14f + Mathf.Cos(a) * 8f, 0f, Mathf.Sin(a) * 8f), new Color(0.4f, 0.25f, 0.18f), 1.8f, 1f).AddComponent<MicroForager>();
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
            Ant(root, $"Hormiga_{i + 1}", new Vector3(Mathf.Cos(a) * 10f, 0f, Mathf.Sin(a) * 10f), new Color(0.4f, 0.25f, 0.18f), 1.8f, 1f).AddComponent<MicroForager>();
        }
        return count;
    }

    // ── Piezas ────────────────────────────────────────────────────────────────────────────────────────────────────

    // Una HORMIGA HUMANIZADA: bípeda, con cara; NavMeshAgent para rodear los muebles gigantes.
    static GameObject Ant(Transform root, string name, Vector3 pos, Color c, float height, float bulk)
    {
        var g = new GameObject(name);
        g.transform.SetParent(root, false); g.transform.localPosition = pos;
        g.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        var col = g.AddComponent<CapsuleCollider>(); col.height = height; col.radius = 0.35f * bulk; col.center = Vector3.up * height * 0.5f;
        var ag = g.AddComponent<UnityEngine.AI.NavMeshAgent>(); ag.radius = 0.4f * bulk; ag.height = height; ag.speed = 3f; ag.angularSpeed = 360f;
        CubeInsect.DressHuman(g, c, height, bulk);
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
    static int Nature(Transform root, float r0, float r1, int seed)
    {
        var rnd = new System.Random(seed);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        Vector3 Ring(float a0, float a1) { float a = R(0f, Mathf.PI * 2f), r = R(a0, a1); return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r); }
        var flora = new GameObject("Flora_a_escala").transform; flora.SetParent(root, false);
        Material grass = Mat("Micro_Hierba", new Color(0.62f, 0.68f, 0.3f)), grassDry = Mat("Micro_Hierba_seca", new Color(0.78f, 0.72f, 0.42f));
        Material fern = Mat("Micro_Helecho", new Color(0.25f, 0.5f, 0.25f)), fernStem = Mat("Micro_Helecho_tallo", new Color(0.3f, 0.4f, 0.2f));
        Material stem = Mat("Micro_Pie_Seta", new Color(0.9f, 0.86f, 0.75f)), cap = Mat("Micro_Sombrero", new Color(0.7f, 0.35f, 0.2f));
        Material moss = Mat("Micro_Musgo", new Color(0.3f, 0.45f, 0.2f)), mossTip = Mat("Micro_Musgo_punta", new Color(0.45f, 0.6f, 0.25f));
        Material rock = Mat("Micro_Grano", new Color(0.55f, 0.52f, 0.48f));
        for (int i = 0; i < 260; i++) { Vector3 p = Ring(i < 60 ? 16f : r0, r1); for (int k = 0; k < 4; k++) { float hh = R(0.4f, 1.2f); Box(flora, "Musgo", p + new Vector3(R(-0.6f, 0.6f), hh * 0.5f, R(-0.6f, 0.6f)), new Vector3(0.18f, hh, 0.18f), Quaternion.Euler(R(-8f, 8f), 0f, R(-8f, 8f)), k % 2 == 0 ? moss : mossTip, false); } }
        for (int i = 0; i < 45; i++) { Vector3 p = Ring(r0, r1); float hh = R(2f, 6f); Box(flora, "Hongo_pie", p + Vector3.up * hh * 0.5f, new Vector3(hh * 0.15f, hh, hh * 0.15f), Quaternion.identity, stem, true); Box(flora, "Hongo_sombrero", p + Vector3.up * hh, new Vector3(hh * 0.8f, hh * 0.18f, hh * 0.8f), Quaternion.Euler(0f, R(0f, 90f), 0f), cap, false); }
        for (int i = 0; i < 90; i++) { Vector3 p = Ring(r0, r1); float hh = R(10f, 25f), lean = R(-18f, 18f), yaw = R(0f, 180f); Material m = i % 3 == 0 ? grassDry : grass; Box(flora, "Brizna", p + Vector3.up * hh * 0.3f, new Vector3(0.25f, hh * 0.6f, 1.2f), Quaternion.Euler(lean * 0.4f, yaw, 0f), m, true); Box(flora, "Brizna_punta", p + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, hh * 0.78f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.25f), new Vector3(0.2f, hh * 0.4f, 0.8f), Quaternion.Euler(lean, yaw, 0f), m, false); }
        for (int i = 0; i < 25; i++) { Vector3 p = Ring(r0 + 10f, r1); float hh = R(12f, 22f); Box(flora, "Helecho_tallo", p + Vector3.up * hh * 0.5f, new Vector3(0.6f, hh, 0.6f), Quaternion.identity, fernStem, true); for (int k = 0; k < 7; k++) Box(flora, "Helecho_fronda", p + Vector3.up * hh + Quaternion.Euler(0f, k * 51f, 0f) * new Vector3(0f, -1.5f, 5f), new Vector3(2.4f, 0.2f, 11f), Quaternion.Euler(-20f, k * 51f, 0f), fern, false); }
        for (int i = 0; i < 70; i++) { Vector3 p = Ring(r0, r1); float s = R(1.5f, 6f); Box(flora, "Grano", p + Vector3.up * s * 0.4f, new Vector3(s, s * 0.8f, s * 1.1f), Quaternion.Euler(R(0f, 25f), R(0f, 90f), R(0f, 15f)), rock, true); }

        // FAUNA: cada bicho cuelga de un padre MicroSize que lo mantiene a su tamaño relativo a la hormiga (el prefab
        // fija su propia escala al nacer; por eso antes se veían diminutos).
        var fauna = new GameObject("Fauna_a_escala").transform; fauna.SetParent(root, false);
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
                Renderer[] rs = a.GetComponentsInChildren<Renderer>();   // y ya en el editor, a su tamaño
                if (rs.Length > 0) { Bounds b = rs[0].bounds; foreach (Renderer x in rs) b.Encapsulate(x.bounds); float now = Mathf.Max(b.size.x, b.size.z); if (now > 1e-4f) holder.localScale = Vector3.one * (len / now); }
                placed++;
            }
        }
        return placed;
    }

    static Transform Box(Transform parent, string name, Vector3 local, Vector3 size, Material m, bool solid) => Box(parent, name, local, size, Quaternion.identity, m, solid);

    static Transform Box(Transform parent, string name, Vector3 local, Vector3 size, Quaternion rot, Material m, bool solid)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localRotation = rot; g.transform.localScale = size;
        if (!solid) Object.DestroyImmediate(g.GetComponent<Collider>());
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g.transform;
    }

    static readonly Dictionary<string, Material> _mats = new Dictionary<string, Material>();
    static Material Mat(string name, Color c)
    {
        if (_mats.TryGetValue(name, out Material cached) && cached != null) return cached;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh) { name = name, color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return _mats[name] = m;
    }
}
