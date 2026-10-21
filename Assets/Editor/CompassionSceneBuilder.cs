using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SIMULACIONES DE COMPASIÓN (docs/compassion-simulations.md): momentos históricos reales en que una persona o un pueblo
/// protegió a otros. Como todo microcosmos, cada una es un ÁREA DEL SANTUARIO AGRANDADA ×10 (`AreaMicrocosmos.Build`):
/// - Le Chambon-sur-Lignon (1942) sobre la COCINA: el pueblo que alimentaba y escondía (la cocina de Magda Trocmé, la
///   panadería, las granjas, la pensión, la escuela).
/// - Irena Sendler (Varsovia, 1942–43) sobre la ENFERMERÍA: la trabajadora social con pase sanitario que sacó niños del
///   gueto (ambulancia, tribunal, alcantarillas) y apuntó sus nombres en frascos bajo un manzano.
/// En el hueco libre del suelo del área se levanta el pueblo (edificios urbanos de cubos), con mirmidones con ropa de los
/// años 40. La lógica es la del santuario reutilizada: la cocina da el pan, la enfermería da el pase, los talleres hacen
/// los papeles. Patrullas genéricas, sin uniformes ni símbolos, y sin violencia en pantalla.
/// </summary>
public static class CompassionSceneBuilder
{
    internal static readonly (string areaGo, string display, string scene, Color tint, System.Func<Transform, int> story)[] Sims =
    {
        ("Kitchen_Area",   "Le Chambon-sur-Lignon", "Microcosmos_LeChambon",     new Color(0.75f, 0.45f, 0.30f), new System.Func<Transform, int>(LeChambon)),
        ("Infirmary_Area", "Irena Sendler",         "Microcosmos_IrenaSendler",  new Color(0.85f, 0.95f, 0.90f), new System.Func<Transform, int>(IrenaSendler)),
    };

    [MenuItem("Tools/Cold Sanctuary/Build simulaciones de compasión")]
    public static void BuildAll()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        int i = 0;
        foreach (var s in Sims)
        {
            Vector3 o = new Vector3(6000f + i * 900f, 0f, 8000f);
            string path = $"Assets/Scenes/{s.scene}.unity";
            Scene prev = EditorSceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var sun = new GameObject("Sun");
            sun.transform.SetPositionAndRotation(o + Vector3.up * 10f, Quaternion.Euler(50f, -30f, 0f));
            sun.AddComponent<Light>().type = LightType.Directional;
            AreaMicrocosmos.Build(s.areaGo, s.display, SampleSceneBuilder.MissionKind.Channel, s.tint, o, s.story);
            MicrocosmosAreaSceneBuilder.Finish(scene, path, prev);
            i++;
        }
        Debug.Log($"[Compasión] {i} simulaciones de compasión generadas.");
    }

    // ── LE CHAMBON-SUR-LIGNON, 1942 ──────────────────────────────────────────────────────────────────────────────

    static int LeChambon(Transform t)
    {
        Color stone = new Color(0.78f, 0.75f, 0.68f), granite = new Color(0.55f, 0.53f, 0.5f), roof = new Color(0.45f, 0.25f, 0.2f), slate = new Color(0.3f, 0.32f, 0.36f);
        Road(t, new Vector3(0f, 0f, 0f), new Vector3(44f, 0.05f, 4f));
        Road(t, new Vector3(0f, 0f, 0f), new Vector3(4f, 0.05f, 44f));
        Building(t, "Templo protestante", new Vector3(0f, 0f, 22f), new Vector3(10f, 7f, 8f), stone, slate);
        Building(t, "Estación (llegan los refugiados)", new Vector3(-22f, 0f, 20f), new Vector3(9f, 3.5f, 4f), granite, roof);
        Transform pres = Building(t, "Presbiterio de los Trocmé", new Vector3(-13f, 0f, 9f), new Vector3(7f, 5f, 6f), stone, roof);
        Transform farm = Building(t, "Granja del plateau", new Vector3(-22f, 0f, -10f), new Vector3(9f, 4.5f, 6f), granite, roof);
        Transform inn = Building(t, "Pensión «Maison des Roches»", new Vector3(14f, 0f, 10f), new Vector3(8f, 6f, 6f), stone, slate);
        Transform school = Building(t, "Escuela Nueva Cevenol", new Vector3(18f, 0f, -9f), new Vector3(9f, 5f, 7f), stone, roof);
        Transform bakery = Building(t, "Panadería", new Vector3(-7f, 0f, -9f), new Vector3(6f, 4f, 5f), granite, roof);
        Transform print = Building(t, "Taller de Oscar (papeles)", new Vector3(5f, 0f, -18f), new Vector3(5f, 3.5f, 5f), granite, slate);

        Hiding(pres, "el presbiterio", 4, false);
        Hiding(farm, "la granja", 3, true);
        Hiding(inn, "la pensión", 4, true);
        Hiding(school, "la escuela", 4, true);
        bakery.gameObject.AddComponent<BreadSource>().label = "la panadería";
        var pf = print.gameObject.AddComponent<PaperForge>(); pf.label = "el taller de Oscar"; pf.blanks = 6;
        Transform swiss = Sign(t, "Camino a Suiza (con la Cimade)", new Vector3(24f, 0f, -24f));
        swiss.gameObject.AddComponent<SafeHaven>().label = "el camino a Suiza";

        int n = 0;
        n += Person(t, "André Trocmé", new Vector3(-2f, 0f, 16f), new Color(0.15f, 0.15f, 0.18f), Hat.Brimmed);
        n += Person(t, "Magda Trocmé", new Vector3(-10f, 0f, 4f), new Color(0.35f, 0.3f, 0.42f), Hat.None);
        n += Person(t, "Oscar Rosowsky", new Vector3(5f, 0f, -13f), new Color(0.4f, 0.36f, 0.3f), Hat.Cap);
        n += Person(t, "Daniel Trocmé", new Vector3(14f, 0f, 5f), new Color(0.3f, 0.35f, 0.3f), Hat.None);
        n += Person(t, "Édouard Theis", new Vector3(18f, 0f, -3f), new Color(0.2f, 0.22f, 0.28f), Hat.Brimmed);

        string[] names = { "Hanna", "Max", "Rachel", "Pierre-Jacques", "Ruth", "Simon", "Liliane", "Jakob", "Esther", "Marcel" };
        for (int i = 0; i < names.Length; i++)
            n += RefugeeAt(t, names[i], new Vector3(-24f + (i % 5) * 1.6f, 0f, 16f - (i / 5) * 1.6f), i % 3 == 0 ? 1.4f : 1.8f, 5f);

        PatrolAt(t, "Patrulla", new[] { new Vector3(-18f, 0f, 2f), new Vector3(18f, 0f, 2f), new Vector3(18f, 0f, -2f), new Vector3(-18f, 0f, -2f) });
        PatrolAt(t, "Patrulla", new[] { new Vector3(2f, 0f, 18f), new Vector3(2f, 0f, -20f) });

        var m = t.gameObject.AddComponent<CompassionMission>();
        m.title = "Le Chambon-sur-Lignon, 1942";
        m.goal = 10;
        m.intro = "Francia ocupada. En el plateau de Vivarais-Lignon, un pueblo protestante de montaña abre sus casas a quien huye, sobre todo a niños judíos. " +
                  "Llegan en tren. Recíbelos (E: «sígueme»), escóndelos en casas, granjas, la pensión o la escuela, llévales pan de la panadería, " +
                  "consíguales papeles en el taller de Oscar o guíalos hacia Suiza. Que no te vean las patrullas con alguien sin papeles.";
        m.history = new[]
        {
            "Entre 1940 y 1944, los habitantes de Le Chambon-sur-Lignon y de los pueblos del plateau acogieron y escondieron a miles de personas perseguidas — se estima que entre 3.000 y 5.000, muchas de ellas niños judíos — en sus casas, granjas, pensiones y escuelas.",
            "Los animaban el pastor André Trocmé, su esposa Magda y el pastor Édouard Theis. Cuando alguien llamaba a la puerta, Magda decía: «Naturalmente, entre, entre».",
            "Oscar Rosowsky, un refugiado de 19 años, falsificaba carnés de identidad y de racionamiento; y la Cimade guiaba a algunos refugiados hasta Suiza.",
            "Daniel Trocmé, primo de André, dirigía la Maison des Roches: fue detenido en 1943 y murió en el campo de Majdanek en 1944.",
            "En 1990 Yad Vashem reconoció a todo el pueblo como Justos entre las Naciones, uno de los poquísimos reconocimientos colectivos.",
        };
        m.sources = new[]
        {
            "United States Holocaust Memorial Museum, Holocaust Encyclopedia: «Le Chambon-sur-Lignon».",
            "Yad Vashem, The Righteous Among the Nations: «Le Chambon-sur-Lignon» y «André and Magda Trocmé».",
            "Philip Hallie, «Lest Innocent Blood Be Shed» (1979).",
        };
        return n;
    }

    // ── IRENA SENDLER, VARSOVIA 1942–43 ─────────────────────────────────────────────────────────────────────────

    static int IrenaSendler(Transform t)
    {
        Color brick = new Color(0.6f, 0.32f, 0.25f), plaster = new Color(0.78f, 0.74f, 0.66f), roof = new Color(0.32f, 0.3f, 0.3f), wall = new Color(0.5f, 0.42f, 0.36f);
        // El GUETO: un recinto amurallado al oeste, sin más salidas que los pasos.
        Vector3 g = new Vector3(-14f, 0f, 0f); float gx = 8f, gz = 10f;
        Material wm = AreaMicrocosmos.Mat("Compasion_Muro", wall);
        AreaMicrocosmos.Box(t, "Muro_N", g + new Vector3(0f, 1.75f, gz), new Vector3(gx * 2f, 3.5f, 0.5f), wm, true);
        AreaMicrocosmos.Box(t, "Muro_S", g + new Vector3(0f, 1.75f, -gz), new Vector3(gx * 2f, 3.5f, 0.5f), wm, true);
        AreaMicrocosmos.Box(t, "Muro_O", g + new Vector3(-gx, 1.75f, 0f), new Vector3(0.5f, 3.5f, gz * 2f), wm, true);
        AreaMicrocosmos.Box(t, "Muro_E", g + new Vector3(gx, 1.75f, 0f), new Vector3(0.5f, 3.5f, gz * 2f), wm, true);
        Label(t, "El gueto", g + Vector3.up * 5f);
        Building(t, "Casa del gueto", g + new Vector3(-4f, 0f, 6f), new Vector3(5f, 4f, 4f), brick, roof);
        Building(t, "Casa del gueto", g + new Vector3(-4f, 0f, -6f), new Vector3(5f, 4f, 4f), brick, roof);
        Road(t, new Vector3(10f, 0f, 0f), new Vector3(4f, 0.05f, 44f));
        Road(t, new Vector3(4f, 0f, -16f), new Vector3(36f, 0.05f, 4f));

        // Los PASOS: la puerta (solo el pase de Irena), el tribunal de Leszno, las alcantarillas y la ambulancia.
        PassagePair(t, "la puerta del gueto (pase sanitario)", g + new Vector3(gx - 1.5f, 0f, -gz + 2f), g + new Vector3(gx + 2f, 0f, -gz + 2f), 0, 0f, true);
        PassagePair(t, "la puerta del gueto (pase sanitario)", g + new Vector3(gx + 3f, 0f, -gz + 4.5f), g + new Vector3(gx - 2.5f, 0f, -gz + 4.5f), 0, 0f, true);
        Transform court = Building(t, "Tribunal de la calle Leszno", g + new Vector3(gx, 0f, 4f), new Vector3(5f, 6f, 6f), plaster, roof);
        PassagePair(t, "el tribunal (dos puertas)", g + new Vector3(gx - 3.2f, 0f, 4f), g + new Vector3(gx + 3.5f, 0f, 4f), 3, 15f, false);
        PassagePair(t, "las alcantarillas", g + new Vector3(2f, 0f, -3f), new Vector3(6f, 0f, -20f), 2, 10f, false, manhole: true);
        Transform amb = AreaMicrocosmos.Box(t, "Ambulancia", g + new Vector3(3f, 1f, 8f), new Vector3(2f, 2f, 4f), AreaMicrocosmos.Mat("Compasion_Ambulancia", new Color(0.92f, 0.92f, 0.88f)), true);
        Label(amb, "Ambulancia", Vector3.up * 1.2f);
        PassagePair(t, "la ambulancia", g + new Vector3(3f, 0f, 5.2f), new Vector3(18f, 0f, 14f), 2, 25f, false);

        // FUERA: la oficina de Żegota (papeles), el convento, el orfanato, una familia y el manzano de los frascos.
        Transform office = Building(t, "Oficina de Żegota (documentos)", new Vector3(4f, 0f, 10f), new Vector3(5f, 4f, 5f), plaster, roof);
        var pf = office.gameObject.AddComponent<PaperForge>(); pf.label = "la oficina de Żegota"; pf.blanks = 8;
        Transform conv = Building(t, "Convento de las Franciscanas", new Vector3(22f, 0f, -8f), new Vector3(8f, 6f, 7f), plaster, roof);
        var c = conv.gameObject.AddComponent<SafeHaven>(); c.label = "el convento"; c.capacity = 5; c.radius = 6f;
        Transform orph = Building(t, "Orfanato del Padre Boduen", new Vector3(22f, 0f, 6f), new Vector3(8f, 5f, 6f), brick, roof);
        var o = orph.gameObject.AddComponent<SafeHaven>(); o.label = "el orfanato"; o.capacity = 4; o.radius = 6f;
        Transform fam = Building(t, "Familia de acogida", new Vector3(16f, 0f, 22f), new Vector3(6f, 4f, 5f), brick, roof);
        var f = fam.gameObject.AddComponent<SafeHaven>(); f.label = "la familia de acogida"; f.capacity = 3; f.radius = 5f;
        Transform tree = new GameObject("Manzano (jardín de Jadwiga Piotrowska)").transform; tree.SetParent(t, false); tree.localPosition = new Vector3(-2f, 0f, -24f);
        AreaMicrocosmos.Box(tree, "Tronco", new Vector3(0f, 1.5f, 0f), new Vector3(0.6f, 3f, 0.6f), AreaMicrocosmos.Mat("Compasion_Tronco", new Color(0.4f, 0.28f, 0.18f)), true);
        AreaMicrocosmos.Box(tree, "Copa", new Vector3(0f, 3.8f, 0f), new Vector3(4f, 2.4f, 4f), AreaMicrocosmos.Mat("Compasion_Copa", new Color(0.3f, 0.55f, 0.25f)), false);
        AreaMicrocosmos.Box(tree, "Frasco", new Vector3(0.9f, 0.25f, 0.6f), new Vector3(0.35f, 0.5f, 0.35f), AreaMicrocosmos.Mat("Compasion_Frasco", new Color(0.75f, 0.85f, 0.9f)), false);
        tree.gameObject.AddComponent<NameJar>();
        Label(tree, "Manzano: los frascos con los nombres", Vector3.up * 5.5f);

        int n = 0;
        n += Person(t, "Irena Sendler", g + new Vector3(gx + 3f, 0f, -gz + 1f), new Color(0.3f, 0.32f, 0.4f), Hat.Brimmed);
        n += Person(t, "Jadwiga Piotrowska", new Vector3(0f, 0f, -22f), new Color(0.38f, 0.3f, 0.3f), Hat.None);

        string[] names = { "Elżbieta", "Michał", "Bieta", "Jerzy", "Hanna", "Piotr", "Lea", "Szymon", "Ruta", "Józef" };
        for (int i = 0; i < names.Length; i++)
            n += RefugeeAt(t, names[i], g + new Vector3(-2f + (i % 4) * 1.6f, 0f, -2f + (i / 4) * 1.8f), 1.3f, 9f);

        PatrolAt(t, "Patrulla", new[] { new Vector3(8f, 0f, -14f), new Vector3(8f, 0f, 16f), new Vector3(14f, 0f, 16f), new Vector3(14f, 0f, -14f) });
        PatrolAt(t, "Patrulla", new[] { new Vector3(-6f, 0f, -14f), new Vector3(20f, 0f, -14f) });

        var m = t.gameObject.AddComponent<CompassionMission>();
        m.title = "Irena Sendler, Varsovia 1942";
        m.goal = 10;
        m.recordNames = true;
        m.intro = "Varsovia ocupada. Irena Sendler, trabajadora social del ayuntamiento, tiene un pase sanitario para entrar al gueto. " +
                  "Los niños no pueden salir por la puerta: sácalos por el tribunal de dos puertas, las alcantarillas o la ambulancia, " +
                  "consíguales papeles en la oficina de Żegota y llévalos al convento, al orfanato o a una familia. " +
                  "Cada nombre real queda escrito en el frasco bajo el manzano, para devolverlos algún día a los suyos.";
        m.history = new[]
        {
            "Irena Sendler (1910–2008) trabajaba en el Departamento de Bienestar Social de Varsovia y, con un pase del servicio de control de epidemias, podía entrar al gueto.",
            "Desde 1942, al frente de la sección infantil de Żegota (el Consejo de Ayuda a los Judíos), ella y su red sacaron del gueto a unos 2.500 niños: en ambulancias, por el edificio del tribunal, por pasos subterráneos y alcantarillas, escondidos en cajas y sacos.",
            "Los colocó con familias polacas, en conventos y en orfanatos, con nombres nuevos. Apuntaba los verdaderos en listas que enterró en frascos bajo un manzano, en el jardín de su compañera Jadwiga Piotrowska.",
            "En 1943 la detuvo la Gestapo y fue condenada a muerte; Żegota sobornó a un guardia y logró salir. Yad Vashem la reconoció como Justa entre las Naciones en 1965.",
            "Ella decía que cada niño salvado era la justificación de su existencia en la Tierra, y no un título de gloria.",
        };
        m.sources = new[]
        {
            "Yad Vashem, The Righteous Among the Nations: «Irena Sendler».",
            "POLIN Museum / Polscy Sprawiedliwi (Polish Righteous): «Irena Sendlerowa».",
            "United States Holocaust Memorial Museum, Holocaust Encyclopedia: «Irena Sendler».",
        };
        return n;
    }

    // ── Piezas ───────────────────────────────────────────────────────────────────────────────────────────────────

    enum Hat { None, Cap, Brimmed }

    /// <summary>Un edificio de pueblo/ciudad de los años 40: cuerpo, tejado a dos aguas, puerta y ventanas, con su nombre.</summary>
    static Transform Building(Transform parent, string label, Vector3 pos, Vector3 size, Color walls, Color roofColor)
    {
        Transform b = new GameObject(label).transform; b.SetParent(parent, false); b.localPosition = pos;
        Vector3 flat = new Vector3(-pos.x, 0f, -pos.z);
        if (flat.sqrMagnitude > 0.01f) b.localRotation = Quaternion.LookRotation(flat.normalized);   // la puerta mira a la plaza
        Material wm = AreaMicrocosmos.Mat($"Compasion_Pared_{ColorUtility.ToHtmlStringRGB(walls)}", walls);
        Material rm = AreaMicrocosmos.Mat($"Compasion_Tejado_{ColorUtility.ToHtmlStringRGB(roofColor)}", roofColor);
        Material dark = AreaMicrocosmos.Mat("Compasion_Puerta", new Color(0.25f, 0.17f, 0.12f)), glass = AreaMicrocosmos.Mat("Compasion_Ventana", new Color(0.55f, 0.65f, 0.72f));
        AreaMicrocosmos.Box(b, "Cuerpo", new Vector3(0f, size.y * 0.5f, 0f), size, wm, true);
        float half = size.x * 0.5f + 0.3f, rise = Mathf.Min(size.x * 0.35f, 2.5f), slope = Mathf.Atan2(rise, half) * Mathf.Rad2Deg, len = Mathf.Sqrt(half * half + rise * rise);
        AreaMicrocosmos.Box(b, "Tejado", new Vector3(-half * 0.5f, size.y + rise * 0.5f, 0f), new Vector3(len, 0.2f, size.z + 0.6f), Quaternion.Euler(0f, 0f, slope), rm, false);
        AreaMicrocosmos.Box(b, "Tejado", new Vector3(half * 0.5f, size.y + rise * 0.5f, 0f), new Vector3(len, 0.2f, size.z + 0.6f), Quaternion.Euler(0f, 0f, -slope), rm, false);
        AreaMicrocosmos.Box(b, "Puerta", new Vector3(0f, 1.1f, size.z * 0.5f + 0.03f), new Vector3(1.2f, 2.2f, 0.06f), dark, false);
        for (int k = -1; k <= 1; k += 2)
        {
            AreaMicrocosmos.Box(b, "Ventana", new Vector3(k * size.x * 0.3f, size.y * 0.6f, size.z * 0.5f + 0.03f), new Vector3(1f, 1f, 0.06f), glass, false);
            AreaMicrocosmos.Box(b, "Ventana", new Vector3(k * (size.x * 0.5f + 0.03f), size.y * 0.6f, 0f), new Vector3(0.06f, 1f, 1f), glass, false);
        }
        Label(b, label, Vector3.up * (size.y + rise + 0.8f));
        return b;
    }

    static void Hiding(Transform building, string label, int capacity, bool saved)
    {
        var h = building.gameObject.AddComponent<HidingPlace>();
        h.label = label; h.capacity = capacity; h.countsAsSaved = saved; h.food = capacity;
        Renderer body = building.Find("Cuerpo")?.GetComponent<Renderer>();
        h.radius = body != null ? Mathf.Max(body.bounds.extents.x, body.bounds.extents.z) + 0.5f : 4f;
    }

    static void Road(Transform parent, Vector3 pos, Vector3 size) =>
        AreaMicrocosmos.Box(parent, "Calle", pos + Vector3.up * 0.03f, size, AreaMicrocosmos.Mat("Compasion_Adoquin", new Color(0.42f, 0.4f, 0.38f)), false);

    static Transform Sign(Transform parent, string label, Vector3 pos)
    {
        Transform s = new GameObject(label).transform; s.SetParent(parent, false); s.localPosition = pos;
        AreaMicrocosmos.Box(s, "Poste", new Vector3(0f, 1.2f, 0f), new Vector3(0.2f, 2.4f, 0.2f), AreaMicrocosmos.Mat("Compasion_Poste", new Color(0.45f, 0.32f, 0.2f)), true);
        AreaMicrocosmos.Box(s, "Tablilla", new Vector3(0f, 2.2f, 0f), new Vector3(1.6f, 0.5f, 0.08f), AreaMicrocosmos.Mat("Compasion_Tablilla", new Color(0.85f, 0.8f, 0.65f)), false);
        Label(s, label, Vector3.up * 3f);
        return s;
    }

    static void Label(Transform parent, string text, Vector3 local)
    {
        var g = new GameObject("Etiqueta"); g.transform.SetParent(parent, false); g.transform.localPosition = local;
        WorldLabel wl = g.AddComponent<WorldLabel>(); wl.text = text; wl.height = 0f;
    }

    static void PassagePair(Transform parent, string label, Vector3 from, Vector3 to, int perTrip, float cooldown, bool alone, bool manhole = false)
    {
        Material m = AreaMicrocosmos.Mat(manhole ? "Compasion_Alcantarilla" : "Compasion_Paso", manhole ? new Color(0.2f, 0.2f, 0.22f) : new Color(0.6f, 0.55f, 0.4f));
        Transform a = AreaMicrocosmos.Box(parent, $"Paso: {label}", from + Vector3.up * 0.04f, new Vector3(1.4f, 0.08f, 1.4f), m, false);
        Transform b = AreaMicrocosmos.Box(parent, $"Salida: {label}", to + Vector3.up * 0.04f, new Vector3(1.4f, 0.08f, 1.4f), m, false);
        var p = a.gameObject.AddComponent<Passage>();
        p.label = label; p.exit = b; p.perTrip = perTrip; p.cooldown = cooldown; p.alone = alone; p.radius = 1.2f;
        if (manhole) { var back = b.gameObject.AddComponent<Passage>(); back.label = label; back.exit = a; back.radius = 1.2f; back.alone = true; }
    }

    // Un MIRMIDÓN con ropa de los años 40: abrigo del color dado y sombrero (el cuerpo de cubos dibuja la ropa en detalle).
    static int Person(Transform parent, string name, Vector3 pos, Color coat, Hat hat)
    {
        GameObject p = AreaMicrocosmos.Ant(parent, name, pos, coat, 1.8f, 1f);
        Dress1940s(p.transform, 1.8f, hat);
        return 1;
    }

    static void Dress1940s(Transform p, float height, Hat hat)
    {
        Material felt = AreaMicrocosmos.Mat("Compasion_Fieltro", new Color(0.22f, 0.2f, 0.18f)), wool = AreaMicrocosmos.Mat("Compasion_Lana", new Color(0.55f, 0.18f, 0.16f));
        if (hat == Hat.Brimmed)
        {
            AreaMicrocosmos.Box(p, "Sombrero_ala", new Vector3(0f, height + 0.02f, 0f), new Vector3(0.55f, 0.04f, 0.55f), felt, false);
            AreaMicrocosmos.Box(p, "Sombrero_copa", new Vector3(0f, height + 0.12f, 0f), new Vector3(0.32f, 0.18f, 0.32f), felt, false);
        }
        else if (hat == Hat.Cap) AreaMicrocosmos.Box(p, "Gorra", new Vector3(0f, height + 0.04f, 0.05f), new Vector3(0.36f, 0.08f, 0.42f), felt, false);
        AreaMicrocosmos.Box(p, "Bufanda", new Vector3(0f, height * 0.8f, 0f), new Vector3(0.4f, 0.1f, 0.35f), wool, false);
    }

    static int RefugeeAt(Transform parent, string name, Vector3 pos, float height, float homeRadius)
    {
        Color[] coats = { new Color(0.35f, 0.3f, 0.25f), new Color(0.25f, 0.3f, 0.4f), new Color(0.45f, 0.38f, 0.3f), new Color(0.3f, 0.35f, 0.3f) };
        GameObject g = AreaMicrocosmos.Ant(parent, name, pos, coats[Mathf.Abs(name.GetHashCode()) % coats.Length], height, height < 1.5f ? 0.8f : 1f);
        Dress1940s(g.transform, height, height < 1.5f ? Hat.Cap : Hat.None);
        var r = g.AddComponent<Refugee>(); r.displayName = name; r.homeRadius = homeRadius;
        return 1;
    }

    static void PatrolAt(Transform parent, string name, Vector3[] route)
    {
        GameObject g = AreaMicrocosmos.Ant(parent, name, route[0], new Color(0.32f, 0.33f, 0.3f), 1.9f, 1.05f);
        Dress1940s(g.transform, 1.9f, Hat.Cap);
        var p = g.AddComponent<Patrol>();
        p.route = new Vector3[route.Length];
        for (int i = 0; i < route.Length; i++) p.route[i] = route[i];
        p.space = parent;   // la ruta está en coordenadas del pueblo
    }
}
