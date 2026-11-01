using UnityEngine;

/// <summary>
/// LOS MUEBLES DETALLADOS DE LOS MICROCOSMOS (docs/microcosmos-reglas-edificacion.md, regla 3; Berón 2026-11-01: «la cocina y
/// los muebles parecían cubos»). Cada mueble es una unión de piezas que se reconoce por su forma, a tamaño real de persona
/// (los insectos-humanos miden 1,6–1,85 m), y lleva sus DEPENDENCIAS: la cocina, su sartén, su olla, su tabla con cuchillo, sus
/// cucharones, cuencos y platos; la mesa, sus sillas; la cama, su colchón, sábana y almohada. Reutiliza las piezas de la
/// cocina del santuario (<see cref="KitchenProps"/>: encimera, fregadero, sartén, olla, tabla y cuchillo, bote de utensilios,
/// cuencos, platos, estantería de despensa).
/// La raíz de cada mueble lleva <see cref="MicroPiece"/> (qué es y qué necesidad básica cubre) y un BoxCollider sólido que
/// envuelve sus piezas (<see cref="KitchenProps.Fit"/>): sobre ella se pone el <see cref="AreaFunctionStation"/> del puesto.
/// La época (<c>year</c>) elige la versión: hogar de piedra o cocina de leña o de gas; letrina o retrete; palangana o lavabo.
/// </summary>
internal static class MicroFurniture
{
    static Color H(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out Color c); return c; }
    static readonly Color Wood = KitchenProps.Wood, WoodDark = KitchenProps.WoodDark, Iron = H("3B3B3E"), Stone = H("8C857A"), Clay = H("B0714A"),
        Linen = H("EDE9DE"), Wool = H("8A5A44"), Pillow = H("F6F3EA"), Brass = H("C9A24A"), Water = H("6E9CC0"), Ash = H("4A4440"), Ember = H("E0702A"),
        Straw = H("D2B56E"), Paper = H("EFE8D6"), Green = H("4E7A3E"), Soil = H("5A4430"), Chalk = H("F0F0E8"), Slate = H("2F3A36"), Sack = H("C9B48C");

    /// <summary>Las claves que <see cref="Build"/> sabe construir (cualquier otra sale como mesa con dos sillas).</summary>
    internal static readonly string[] Keys =
    {
        "fogon", "cocina", "horno", "barra", "mesa", "mesa_laboratorio", "cama", "camilla", "inodoro", "lavabo", "lavadero", "silla", "banco",
        "estante", "armario", "pizarra", "banco_taller", "torno", "tunel", "muneco", "altar", "puesto", "granero", "torno_alfarero", "parapeto",
        "pozo", "telar", "tatami", "esterilla", "bancal",
    };

    static Transform C(Transform t, Vector3 p, Vector3 s, Color c, float yaw = 0f, string n = "pieza") => KitchenProps.C(t, p, s, c, yaw, n);

    /// <summary>
    /// Construye el mueble de la clave <paramref name="key"/> (las de <see cref="AreaFunctionStation.station"/> y las de las
    /// necesidades básicas: «inodoro», «lavabo», «silla», «banco») en <paramref name="pos"/> (local de <paramref name="parent"/>,
    /// la base en el suelo), girado <paramref name="yaw"/> (el frente de uso mira a −z local). <paramref name="size"/> solo lo
    /// usan los que se pisan (tatami, esterilla, bancal: su ancho × fondo). Devuelve la raíz (con MicroPiece y su caja).
    /// </summary>
    internal static Transform Build(Transform parent, string key, Vector3 pos, float yaw = 0f, string label = null, int year = 1950, Vector2? size = null)
    {
        string name = string.IsNullOrEmpty(label) ? key : label;
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        string need = "", kind = key; bool walk = false;
        switch (key)
        {
            case "fogon": case "cocina": Kitchen(t, year); need = "cocinar"; kind = "cocina"; break;
            case "horno": Oven(t); need = "cocinar"; break;
            case "barra": Bar(t); need = "comer"; break;
            case "mesa": TableWithChairs(t, year, 4); need = "comer"; break;
            case "mesa_laboratorio": LabBench(t); break;
            case "cama": Bed(t, year, false); need = "dormir"; break;
            case "camilla": Bed(t, year, true); need = "dormir"; break;
            case "inodoro": Toilet(t, year); need = "aseo"; break;
            case "lavabo": Washbasin(t, year); need = "lavarse"; break;
            case "lavadero": WashTub(t); need = "lavarse"; break;
            case "silla": Chair(t, Vector3.zero, 0f); need = "sentarse"; break;
            case "banco": Bench(t, Vector3.zero, 1.6f); need = "sentarse"; break;
            case "estante": case "armario": Shelf(t, name.GetHashCode()); break;
            case "pizarra": Blackboard(t); break;
            case "banco_taller": Workbench(t); break;
            case "torno": Lathe(t); break;
            case "tunel": WindTunnel(t); break;
            case "muneco": WoodenDummy(t); break;
            case "altar": Altar(t); break;
            case "puesto": Stall(t, name.GetHashCode()); break;
            case "granero": Granary(t); need = "cocinar"; break;
            case "torno_alfarero": PotterWheel(t); break;
            case "parapeto": Sandbags(t); break;
            case "pozo": Well(t); need = "lavarse"; break;
            case "telar": Loom(t); break;
            case "tatami": case "esterilla": case "bancal": Mat(t, key, size ?? new Vector2(key == "tatami" ? 6f : key == "bancal" ? 2.5f : 1f, key == "tatami" ? 5f : 2f)); walk = true; break;
            default: TableWithChairs(t, year, 2); need = "comer"; break;
        }
        var mp = t.gameObject.AddComponent<MicroPiece>(); mp.kind = kind; mp.need = need; mp.walkable = walk;
        KitchenProps.Fit(t, !walk);
        return t;
    }

    /// <summary>La huella (ancho x, fondo z) del mueble <paramref name="key"/> sin girar, medida construyéndolo aparte.</summary>
    internal static Bounds LocalBounds(Transform piece)
    {
        Bounds b = new Bounds(Vector3.zero, Vector3.zero); bool any = false;
        foreach (Renderer r in piece.GetComponentsInChildren<Renderer>(true))
        {
            Bounds rb = r.bounds;
            Vector3 mn = piece.InverseTransformPoint(rb.min), mx = piece.InverseTransformPoint(rb.max);
            Bounds lb = new Bounds((mn + mx) * 0.5f, new Vector3(Mathf.Abs(mx.x - mn.x), Mathf.Abs(mx.y - mn.y), Mathf.Abs(mx.z - mn.z)));
            if (!any) { b = lb; any = true; } else b.Encapsulate(lb);
        }
        return b;
    }

    // ── COCINA: encimera con armarios + fogón de la época + utensilios (lo que el trabajador necesita) ──────────────────
    static void Kitchen(Transform t, int year)
    {
        if (year < -2000) { GroundHearth(t, year); return; }
        // Encimera corrida de 2,4 m (armarios, puertas, tiradores, tablero) con el fogón en medio y el fregadero o barreño.
        Transform run = KitchenProps.CounterRun(t, "Encimera", new Vector3(0f, 0f, 0f), 2.4f);
        Object.DestroyImmediate(run.GetComponent<BoxCollider>());
        float top = KitchenProps.CounterTop;
        Transform stove = new GameObject("Fogon").transform; stove.SetParent(t, false); stove.localPosition = new Vector3(-0.55f, 0f, 0f);
        if (year < 1850)
        {   // hogar de piedra sobre la encimera de obra, con brasas y trébede
            for (int k = 0; k < 4; k++) C(stove, Quaternion.Euler(0f, k * 90f, 0f) * new Vector3(0f, top + 0.08f, 0.22f), new Vector3(0.5f, 0.16f, 0.08f), Stone, k * 90f, "Piedra_del_hogar");
            C(stove, new Vector3(0f, top + 0.03f, 0f), new Vector3(0.36f, 0.04f, 0.36f), Ash, 0f, "Ceniza");
            for (int k = 0; k < 3; k++) C(stove, new Vector3(-0.08f + k * 0.08f, top + 0.06f, 0.02f * k), new Vector3(0.05f, 0.05f, 0.2f), Ember, 20f * k, "Brasa");
            for (int k = 0; k < 3; k++) C(stove, Quaternion.Euler(0f, k * 120f, 0f) * new Vector3(0f, top + 0.12f, 0.14f), new Vector3(0.03f, 0.24f, 0.03f), Iron, 0f, "Trebede");
        }
        else if (year < 1940)
        {   // cocina económica de hierro (leña o carbón): cuerpo, chapa con aros, puerta del fuego y tubo
            C(stove, new Vector3(0f, top + 0.02f, 0f), new Vector3(0.7f, 0.04f, 0.6f), Iron, 0f, "Chapa");
            for (int k = -1; k <= 1; k += 2) C(stove, new Vector3(k * 0.16f, top + 0.045f, 0f), new Vector3(0.2f, 0.01f, 0.2f), H("555559"), 45f, "Aro");
            C(stove, new Vector3(0f, 0.55f, -0.33f), new Vector3(0.36f, 0.24f, 0.02f), H("2A2A2C"), 0f, "Puerta_del_fuego");
            C(stove, new Vector3(0.12f, 0.55f, -0.35f), new Vector3(0.05f, 0.03f, 0.03f), Brass, 0f, "Tirador");
            C(stove, new Vector3(0f, top + 0.6f, 0.28f), new Vector3(0.12f, 1.2f, 0.12f), Iron, 0f, "Tubo");
        }
        else
        {   // fogón de gas de dos fuegos (wok en el de la izquierda: el de una cocina china)
            C(stove, new Vector3(0f, top + 0.05f, 0f), new Vector3(0.7f, 0.1f, 0.55f), H("6B6E72"), 0f, "Fogon_de_gas");
            for (int k = -1; k <= 1; k += 2)
            {
                C(stove, new Vector3(k * 0.17f, top + 0.11f, 0f), new Vector3(0.14f, 0.02f, 0.14f), Iron, 45f, "Quemador");
                for (int s = 0; s < 3; s++) C(stove, new Vector3(k * 0.17f, top + 0.13f, 0f), new Vector3(0.24f, 0.015f, 0.02f), Iron, s * 60f, "Rejilla");
                C(stove, new Vector3(k * 0.17f, top + 0.05f, -0.28f), new Vector3(0.04f, 0.04f, 0.03f), Ash, 0f, "Mando");
            }
            Transform wok = new GameObject("Wok").transform; wok.SetParent(stove, false); wok.localPosition = new Vector3(-0.17f, top + 0.14f, 0f);
            C(wok, new Vector3(0f, 0.02f, 0f), new Vector3(0.18f, 0.03f, 0.18f), Iron, 0f, "Fondo");
            C(wok, new Vector3(0f, 0.06f, 0f), new Vector3(0.34f, 0.05f, 0.34f), Iron, 45f, "Cuenco");
            C(wok, new Vector3(0f, 0.08f, -0.26f), new Vector3(0.03f, 0.03f, 0.2f), WoodDark, 0f, "Mango");
        }
        // Utensilios: la sartén y la olla en el fuego, tabla y cuchillo, bote de cucharones, cuencos y platos en la encimera.
        KitchenProps.Pan(t, "Sarten", new Vector3(-0.38f, top + (year < 1850 ? 0.24f : 0.13f), 0.05f), 0f);
        KitchenProps.Pot(t, "Olla", new Vector3(-0.72f, top + (year < 1850 ? 0.24f : 0.13f), 0.05f));
        KitchenProps.BoardAndKnife(t, "Tabla_y_cuchillo", new Vector3(0.3f, top, -0.05f), 0f);
        KitchenProps.UtensilCrock(t, "Cucharones", new Vector3(0.95f, top, 0.18f));
        for (int k = 0; k < 2; k++) KitchenProps.Bowl(t, "Cuenco", new Vector3(0.75f + k * 0.18f, top, -0.15f));
        for (int k = 0; k < 3; k++) KitchenProps.Plate(t, "Plato", new Vector3(1.0f, top + k * 0.018f, -0.15f));
        // El agua: barreño de madera (o pila con grifo desde 1900) en el otro extremo.
        if (year >= 1900)
        {
            C(t, new Vector3(0.62f, top - 0.04f, 0.12f), new Vector3(0.4f, 0.1f, 0.3f), KitchenProps.Steel, 0f, "Pila");
            C(t, new Vector3(0.62f, top + 0.15f, 0.27f), new Vector3(0.03f, 0.3f, 0.03f), KitchenProps.Steel, 0f, "Grifo");
        }
        else
        {
            C(t, new Vector3(0.62f, top + 0.08f, 0.12f), new Vector3(0.36f, 0.16f, 0.3f), WoodDark, 0f, "Barreno");
            C(t, new Vector3(0.62f, top + 0.12f, 0.12f), new Vector3(0.3f, 0.02f, 0.24f), Water, 0f, "Agua");
        }
    }

    // El HOGAR EN EL SUELO de las aldeas y las primeras ciudades: anillo de piedras, brasas, la losa de asar, la piedra de moler
    // con su mano, el cuchillo de sílex sobre una piedra plana y, desde la cerámica (~7000 a. C.), la olla de barro en las brasas.
    static void GroundHearth(Transform t, int year)
    {
        for (int k = 0; k < 8; k++) C(t, Quaternion.Euler(0f, k * 45f, 0f) * new Vector3(0f, 0.08f, 0.42f), new Vector3(0.26f, 0.16f, 0.18f), Stone, k * 45f, "Piedra_del_hogar");
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(0.6f, 0.04f, 0.6f), Ash, 0f, "Ceniza");
        for (int k = 0; k < 4; k++) C(t, new Vector3(-0.15f + k * 0.1f, 0.07f, 0.05f * (k % 2)), new Vector3(0.06f, 0.06f, 0.26f), Ember, 25f * k, "Brasa");
        C(t, new Vector3(0.15f, 0.12f, -0.1f), new Vector3(0.3f, 0.04f, 0.24f), Stone, 10f, "Losa_de_asar");
        if (year >= -7000)
        {
            C(t, new Vector3(-0.15f, 0.18f, 0.1f), new Vector3(0.26f, 0.22f, 0.26f), Clay, 0f, "Olla_de_barro");
            C(t, new Vector3(-0.15f, 0.3f, 0.1f), new Vector3(0.2f, 0.03f, 0.2f), Clay, 45f, "Boca");
        }
        C(t, new Vector3(0f, 0.06f, -0.75f), new Vector3(0.5f, 0.12f, 0.3f), Stone, 0f, "Piedra_de_moler");
        C(t, new Vector3(0.05f, 0.15f, -0.75f), new Vector3(0.2f, 0.07f, 0.1f), Stone, 15f, "Mano_de_moler");
        C(t, new Vector3(0.55f, 0.05f, -0.4f), new Vector3(0.3f, 0.1f, 0.25f), Stone, -10f, "Piedra_de_cortar");
        C(t, new Vector3(0.55f, 0.11f, -0.4f), new Vector3(0.14f, 0.015f, 0.04f), H("4A4A52"), 20f, "Cuchillo_de_silex");
        C(t, new Vector3(-0.55f, 0.06f, -0.4f), new Vector3(0.22f, 0.1f, 0.22f), WoodDark, 0f, "Cuenco_de_madera");
    }

    static void Oven(Transform t)
    {
        C(t, new Vector3(0f, 0.45f, 0.2f), new Vector3(1.4f, 0.9f, 1.1f), Clay, 0f, "Base");
        for (int k = 0; k < 3; k++) C(t, new Vector3(0f, 1.0f + k * 0.18f, 0.2f), new Vector3(1.2f - k * 0.3f, 0.2f, 0.95f - k * 0.25f), Clay, k * 15f, "Boveda");
        C(t, new Vector3(0f, 1.0f, -0.36f), new Vector3(0.45f, 0.3f, 0.02f), Ash, 0f, "Boca");
        C(t, new Vector3(0f, 0.92f, -0.37f), new Vector3(0.35f, 0.04f, 0.02f), Ember, 0f, "Brasas");
        // Mesa de amasar al lado, con la pala del horno y dos hogazas.
        Transform tb = new GameObject("Mesa_de_amasar").transform; tb.SetParent(t, false); tb.localPosition = new Vector3(1.3f, 0f, 0f);
        TableTop(tb, new Vector2(0.9f, 0.7f), 0.85f, Wood);
        C(tb, new Vector3(0f, 0.9f, 0f), new Vector3(0.12f, 0.04f, 1.4f), Wood, 80f, "Pala_del_horno");
        for (int k = 0; k < 2; k++) C(tb, new Vector3(-0.2f + k * 0.35f, 0.92f, 0.15f), new Vector3(0.22f, 0.1f, 0.16f), H("C88B5A"), 15f, "Hogaza");
    }

    static void Bar(Transform t)
    {
        C(t, new Vector3(0f, 0.5f, 0f), new Vector3(3f, 1.0f, 0.55f), WoodDark, 0f, "Barra");
        C(t, new Vector3(0f, 1.02f, -0.04f), new Vector3(3.06f, 0.05f, 0.65f), H("D9D2C2"), 0f, "Tablero");
        C(t, new Vector3(0f, 0.1f, -0.3f), new Vector3(3f, 0.04f, 0.04f), Brass, 0f, "Reposapies");
        for (int k = 0; k < 4; k++)
        {
            float x = -1.1f + k * 0.73f;
            Transform s = new GameObject("Taburete").transform; s.SetParent(t, false); s.localPosition = new Vector3(x, 0f, -0.7f);
            C(s, new Vector3(0f, 0.36f, 0f), new Vector3(0.06f, 0.72f, 0.06f), Iron, 0f, "Pie");
            C(s, new Vector3(0f, 0.74f, 0f), new Vector3(0.36f, 0.06f, 0.36f), H("8E2A26"), 0f, "Asiento");
            C(s, new Vector3(0f, 0.03f, 0f), new Vector3(0.36f, 0.04f, 0.36f), Iron, 45f, "Base");
            C(t, new Vector3(x, 1.08f, 0f), new Vector3(0.08f, 0.08f, 0.08f), KitchenProps.Ceramic, 0f, "Taza");
        }
        C(t, new Vector3(1.25f, 1.12f, 0.1f), new Vector3(0.18f, 0.16f, 0.18f), H("6E4B2E"), 0f, "Tetera");
        C(t, new Vector3(1.36f, 1.15f, 0.1f), new Vector3(0.08f, 0.03f, 0.03f), H("6E4B2E"), 0f, "Pitorro");
    }

    static void TableTop(Transform t, Vector2 size, float h, Color c)
    {
        C(t, new Vector3(0f, h - 0.02f, 0f), new Vector3(size.x, 0.04f, size.y), c, 0f, "Tablero");
        foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) })
            C(t, new Vector3(x * (size.x * 0.5f - 0.05f), (h - 0.04f) * 0.5f, z * (size.y * 0.5f - 0.05f)), new Vector3(0.05f, h - 0.04f, 0.05f), WoodDark, 0f, "Pata");
    }

    static void Chair(Transform parent, Vector3 at, float yaw)
    {
        Transform s = KitchenProps.Root(parent, "Silla", at, yaw);
        C(s, new Vector3(0f, 0.45f, 0f), new Vector3(0.42f, 0.04f, 0.42f), Wood, 0f, "Asiento");
        foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) }) C(s, new Vector3(x * 0.18f, 0.22f, z * 0.18f), new Vector3(0.04f, 0.44f, 0.04f), WoodDark, 0f, "Pata");
        C(s, new Vector3(0f, 0.72f, 0.19f), new Vector3(0.42f, 0.5f, 0.04f), Wood, 0f, "Respaldo");
        var mp = s.gameObject.AddComponent<MicroPiece>(); mp.kind = "silla"; mp.need = "sentarse";
    }

    static void Bench(Transform parent, Vector3 at, float len)
    {
        Transform s = new GameObject("Banco").transform; s.SetParent(parent, false); s.localPosition = at;
        C(s, new Vector3(0f, 0.44f, 0f), new Vector3(len, 0.05f, 0.32f), Wood, 0f, "Asiento");
        for (int k = -1; k <= 1; k += 2) C(s, new Vector3(k * (len * 0.5f - 0.1f), 0.21f, 0f), new Vector3(0.06f, 0.42f, 0.28f), WoodDark, 0f, "Pata");
        var mp = s.gameObject.AddComponent<MicroPiece>(); mp.kind = "banco"; mp.need = "sentarse";
    }

    // Mesa con sillas (o bancos) a los lados, platos, cuencos y palillos o cubiertos.
    static void TableWithChairs(Transform t, int year, int seats)
    {
        if (year < -2000)
        {   // antes de las mesas: la estera de comer en el suelo, sus cuencos de madera y troncos para sentarse
            C(t, new Vector3(0f, 0.02f, 0f), new Vector3(1.4f, 0.04f, 0.9f), Straw, 0f, "Estera_de_comer");
            for (int k = 0; k < 3; k++) C(t, new Vector3(-0.4f + k * 0.4f, 0.08f, 0f), new Vector3(0.2f, 0.08f, 0.2f), WoodDark, 0f, "Cuenco_de_madera");
            for (int k = -1; k <= 1; k += 2)
            {
                Transform log = new GameObject("Tronco_asiento").transform; log.SetParent(t, false); log.localPosition = new Vector3(0f, 0f, k * 0.75f);
                C(log, new Vector3(0f, 0.2f, 0f), new Vector3(1.2f, 0.4f, 0.35f), H("6B4F35"), 0f, "Tronco");
                var lp = log.gameObject.AddComponent<MicroPiece>(); lp.kind = "banco"; lp.need = "sentarse";
            }
            return;
        }
        Transform tb = new GameObject("Mesa").transform; tb.SetParent(t, false);
        TableTop(tb, new Vector2(1.4f, 0.8f), 0.75f, Wood);
        if (year < 1700) { Bench(t, new Vector3(0f, 0f, -0.68f), 1.4f); Bench(t, new Vector3(0f, 0f, 0.68f), 1.4f); }
        else for (int k = 0; k < seats; k++)
            {
                float x = seats <= 2 ? 0f : (k % 2 == 0 ? -0.35f : 0.35f);
                bool front = seats <= 2 ? k == 0 : k < 2;
                Chair(t, new Vector3(x, 0f, front ? -0.62f : 0.62f), front ? 0f : 180f);
            }
        for (int k = 0; k < Mathf.Max(2, seats); k++)
        {
            float x = seats <= 2 ? 0f : (k % 2 == 0 ? -0.35f : 0.35f); float z = (seats <= 2 ? k == 0 : k < 2) ? -0.22f : 0.22f;
            KitchenProps.Plate(tb, "Plato", new Vector3(x, 0.75f, z));
            KitchenProps.Bowl(tb, "Cuenco", new Vector3(x + 0.13f, 0.75f, z));
            C(tb, new Vector3(x - 0.16f, 0.76f, z), new Vector3(0.01f, 0.01f, 0.2f), year >= 1900 ? WoodDark : KitchenProps.Steel, 0f, year >= 1900 ? "Palillos" : "Cuchara");
        }
        C(tb, new Vector3(0f, 0.8f, 0f), new Vector3(0.14f, 0.1f, 0.14f), KitchenProps.Ceramic, 0f, "Jarra");
    }

    static void LabBench(Transform t)
    {
        TableTop(t, new Vector2(2.2f, 0.8f), 0.9f, H("3A3633"));
        C(t, new Vector3(0f, 0.4f, 0.1f), new Vector3(2.0f, 0.6f, 0.5f), WoodDark, 0f, "Cajones");
        for (int k = 0; k < 4; k++) C(t, new Vector3(-0.8f + k * 0.18f, 1.02f, 0.15f), new Vector3(0.08f, 0.22f, 0.08f), KitchenProps.Glass, 0f, "Matraz");
        C(t, new Vector3(0.1f, 0.96f, 0f), new Vector3(0.08f, 0.1f, 0.08f), Brass, 0f, "Mechero");
        C(t, new Vector3(0.1f, 1.1f, 0f), new Vector3(0.18f, 0.02f, 0.18f), Iron, 0f, "Rejilla");
        C(t, new Vector3(0.7f, 0.91f, -0.15f), new Vector3(0.3f, 0.02f, 0.22f), Paper, 10f, "Cuaderno");
        C(t, new Vector3(0.75f, 1.0f, 0.2f), new Vector3(0.25f, 0.18f, 0.2f), H("6A6A6A"), 0f, "Electrometro");
        Chair(t, new Vector3(0f, 0f, -0.7f), 0f);
    }

    // Cama de la época: estera (antes de −500), catre de madera o cama de hierro, colchón, sábana, manta y almohada.
    static void Bed(Transform t, int year, bool stretcher)
    {
        float w = stretcher ? 0.75f : 1.0f, l = 2.0f;
        if (year < -500 && !stretcher)
        {
            C(t, new Vector3(0f, 0.04f, 0f), new Vector3(w, 0.08f, l), Straw, 0f, "Estera");
            C(t, new Vector3(0f, 0.1f, 0.75f), new Vector3(0.5f, 0.08f, 0.3f), Linen, 0f, "Almohada");
            C(t, new Vector3(0f, 0.09f, -0.2f), new Vector3(w * 0.95f, 0.03f, 1.1f), Wool, 0f, "Manta");
            return;
        }
        Color frame = year >= 1850 ? Iron : WoodDark;
        foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) }) C(t, new Vector3(x * (w * 0.5f - 0.04f), 0.2f, z * (l * 0.5f - 0.04f)), new Vector3(0.05f, 0.4f, 0.05f), frame, 0f, "Pata");
        C(t, new Vector3(0f, 0.3f, 0f), new Vector3(w, 0.06f, l), frame, 0f, "Bastidor");
        C(t, new Vector3(0f, 0.42f, 0f), new Vector3(w - 0.06f, 0.18f, l - 0.06f), H("E6DFCF"), 0f, "Colchon");
        C(t, new Vector3(0f, 0.52f, -0.15f), new Vector3(w - 0.02f, 0.03f, l * 0.6f), stretcher ? Linen : Wool, 0f, stretcher ? "Sabana" : "Manta");
        C(t, new Vector3(0f, 0.56f, l * 0.5f - 0.25f), new Vector3(w * 0.6f, 0.08f, 0.32f), Pillow, 0f, "Almohada");
        if (!stretcher) C(t, new Vector3(0f, 0.6f, l * 0.5f), new Vector3(w, 0.6f, 0.05f), frame, 0f, "Cabecero");
    }

    // Retrete de la época: letrina (banco con agujero y cubo) antes de 1880; inodoro de porcelana con cisterna alta después.
    static void Toilet(Transform t, int year)
    {
        if (year < -2000)
        {   // pozo negro: el hoyo rodeado de piedras, dos piedras planas para los pies y un montón de tierra y hojas para tapar
            for (int k = 0; k < 6; k++) C(t, Quaternion.Euler(0f, k * 60f, 0f) * new Vector3(0f, 0.08f, 0.38f), new Vector3(0.3f, 0.16f, 0.16f), Stone, k * 60f, "Piedra");
            C(t, new Vector3(0f, 0.02f, 0f), new Vector3(0.4f, 0.03f, 0.4f), H("1E1A16"), 0f, "Hoyo");
            for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.22f, 0.05f, -0.05f), new Vector3(0.18f, 0.08f, 0.4f), Stone, 0f, "Piedra_de_los_pies");
            C(t, new Vector3(0.65f, 0.12f, 0f), new Vector3(0.4f, 0.24f, 0.4f), Soil, 20f, "Tierra_para_tapar");
            C(t, new Vector3(0.65f, 0.26f, 0f), new Vector3(0.3f, 0.04f, 0.3f), Green, 50f, "Hojas");
            return;
        }
        if (year < 1880)
        {
            C(t, new Vector3(0f, 0.22f, 0f), new Vector3(0.8f, 0.44f, 0.55f), WoodDark, 0f, "Banco_de_la_letrina");
            C(t, new Vector3(0f, 0.445f, -0.02f), new Vector3(0.24f, 0.01f, 0.22f), H("1E1A16"), 0f, "Agujero");
            C(t, new Vector3(0f, 0.47f, 0.2f), new Vector3(0.6f, 0.04f, 0.1f), Wood, 0f, "Tapa");
            C(t, new Vector3(0.6f, 0.15f, 0f), new Vector3(0.25f, 0.3f, 0.25f), Wood, 0f, "Cubo_de_ceniza");
            C(t, new Vector3(0.6f, 0.33f, 0f), new Vector3(0.18f, 0.04f, 0.18f), Ash, 0f, "Ceniza");
            return;
        }
        C(t, new Vector3(0f, 0.2f, 0.05f), new Vector3(0.36f, 0.4f, 0.5f), KitchenProps.White, 0f, "Taza");
        C(t, new Vector3(0f, 0.41f, 0f), new Vector3(0.4f, 0.03f, 0.48f), KitchenProps.Black, 0f, "Asiento");
        C(t, new Vector3(0f, 0.4f, -0.05f), new Vector3(0.24f, 0.01f, 0.28f), Water, 0f, "Agua");
        C(t, new Vector3(0f, 1.0f, 0.3f), new Vector3(0.04f, 1.2f, 0.04f), KitchenProps.Steel, 0f, "Tubo");
        C(t, new Vector3(0f, 1.7f, 0.3f), new Vector3(0.45f, 0.25f, 0.2f), year >= 1930 ? KitchenProps.White : Iron, 0f, "Cisterna");
        C(t, new Vector3(0.2f, 1.45f, 0.22f), new Vector3(0.01f, 0.4f, 0.01f), Brass, 0f, "Cadena");
        C(t, new Vector3(0.35f, 0.6f, 0.2f), new Vector3(0.12f, 0.12f, 0.1f), Paper, 0f, "Papel");
    }

    // Lavabo: palangana sobre un aguamanil con jarra (antes de 1900) o lavabo de pie con grifo y espejo.
    static void Washbasin(Transform t, int year)
    {
        if (year < -2000)
        {   // pila de piedra con agua del río y un cuenco (calabaza) para echársela
            C(t, new Vector3(0f, 0.25f, 0f), new Vector3(0.9f, 0.5f, 0.6f), Stone, 0f, "Pila_de_piedra");
            C(t, new Vector3(0f, 0.48f, 0f), new Vector3(0.75f, 0.04f, 0.45f), Water, 0f, "Agua");
            C(t, new Vector3(0.6f, 0.08f, 0f), new Vector3(0.2f, 0.16f, 0.2f), H("C9A050"), 0f, "Calabaza");
            return;
        }
        if (year < 1900)
        {
            TableTop(t, new Vector2(0.6f, 0.45f), 0.8f, Wood);
            C(t, new Vector3(0f, 0.85f, 0f), new Vector3(0.4f, 0.1f, 0.4f), KitchenProps.Ceramic, 45f, "Palangana");
            C(t, new Vector3(0f, 0.88f, 0f), new Vector3(0.32f, 0.02f, 0.32f), Water, 45f, "Agua");
            C(t, new Vector3(0.22f, 0.95f, 0.12f), new Vector3(0.12f, 0.26f, 0.12f), KitchenProps.Ceramic, 0f, "Jarra");
            C(t, new Vector3(-0.32f, 0.7f, 0f), new Vector3(0.03f, 0.3f, 0.3f), Linen, 0f, "Toalla");
            return;
        }
        C(t, new Vector3(0f, 0.4f, 0.05f), new Vector3(0.16f, 0.8f, 0.16f), KitchenProps.White, 0f, "Pie");
        C(t, new Vector3(0f, 0.84f, 0f), new Vector3(0.55f, 0.12f, 0.42f), KitchenProps.White, 0f, "Lavabo");
        C(t, new Vector3(0f, 0.88f, -0.02f), new Vector3(0.42f, 0.04f, 0.28f), Water, 0f, "Agua");
        C(t, new Vector3(0f, 1.0f, 0.18f), new Vector3(0.03f, 0.2f, 0.03f), KitchenProps.Steel, 0f, "Grifo");
        C(t, new Vector3(0f, 1.45f, 0.22f), new Vector3(0.4f, 0.5f, 0.02f), KitchenProps.Glass, 0f, "Espejo");
        C(t, new Vector3(0.12f, 0.92f, 0.12f), new Vector3(0.08f, 0.04f, 0.05f), H("E8D9A6"), 0f, "Jabon");
    }

    static void WashTub(Transform t)
    {
        C(t, new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 0.7f), WoodDark, 0f, "Pila");
        C(t, new Vector3(0f, 0.57f, 0f), new Vector3(1.05f, 0.04f, 0.55f), Water, 0f, "Agua");
        C(t, new Vector3(0.3f, 0.75f, 0.2f), new Vector3(0.4f, 0.5f, 0.04f), Wood, 0f, "Tabla_de_lavar");
        for (int k = 0; k < 5; k++) C(t, new Vector3(0.3f, 0.58f + k * 0.08f, 0.18f), new Vector3(0.38f, 0.015f, 0.02f), WoodDark, 0f, "Estria");
        C(t, new Vector3(-0.85f, 0.2f, 0f), new Vector3(0.3f, 0.4f, 0.3f), Wood, 0f, "Cubo");
        C(t, new Vector3(-0.85f, 0.42f, 0f), new Vector3(0.28f, 0.04f, 0.28f), Linen, 0f, "Ropa");
    }

    static void Shelf(Transform t, int seed) { Transform s = KitchenProps.Shelving(t, "Estanteria", Vector3.zero, 0f, seed); Object.DestroyImmediate(s.GetComponent<BoxCollider>()); }

    static void Blackboard(Transform t)
    {
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.7f, 0.9f, 0.1f), new Vector3(0.05f, 1.8f, 0.05f), Wood, 0f, "Pata_del_caballete");
        C(t, new Vector3(0f, 1.0f, 0f), new Vector3(1.4f, 1.0f, 0.04f), Slate, 0f, "Pizarra");
        C(t, new Vector3(0f, 0.48f, -0.04f), new Vector3(1.3f, 0.03f, 0.08f), Wood, 0f, "Repisa");
        C(t, new Vector3(0.2f, 0.51f, -0.04f), new Vector3(0.08f, 0.015f, 0.015f), Chalk, 0f, "Tiza");
        for (int k = 0; k < 3; k++) C(t, new Vector3(-0.3f + k * 0.25f, 1.15f - k * 0.1f, -0.025f), new Vector3(0.4f - k * 0.1f, 0.015f, 0.005f), Chalk, 0f, "Escrito");
    }

    static void Workbench(Transform t)
    {
        TableTop(t, new Vector2(2.0f, 0.75f), 0.9f, Wood);
        C(t, new Vector3(0f, 0.2f, 0f), new Vector3(1.9f, 0.04f, 0.6f), WoodDark, 0f, "Balda");
        C(t, new Vector3(-0.8f, 0.98f, -0.3f), new Vector3(0.2f, 0.12f, 0.15f), Iron, 0f, "Tornillo_de_banco");
        C(t, new Vector3(-0.3f, 0.92f, 0f), new Vector3(0.3f, 0.03f, 0.04f), Iron, 20f, "Llave_inglesa");
        C(t, new Vector3(0f, 0.92f, 0.1f), new Vector3(0.25f, 0.03f, 0.05f), WoodDark, -10f, "Martillo");
        C(t, new Vector3(0.5f, 1.0f, 0.1f), new Vector3(0.55f, 0.2f, 0.04f), Iron, 0f, "Llanta_de_bicicleta");
        C(t, new Vector3(0f, 1.4f, 0.36f), new Vector3(1.8f, 0.8f, 0.03f), WoodDark, 0f, "Panel_de_herramientas");
    }

    static void Lathe(Transform t)
    {
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.6f, 0.4f, 0f), new Vector3(0.2f, 0.8f, 0.5f), Iron, 0f, "Pie");
        C(t, new Vector3(0f, 0.85f, 0f), new Vector3(1.5f, 0.1f, 0.3f), Iron, 0f, "Bancada");
        C(t, new Vector3(-0.55f, 1.02f, 0f), new Vector3(0.25f, 0.25f, 0.25f), H("4A5560"), 0f, "Cabezal");
        C(t, new Vector3(0.5f, 1.0f, 0f), new Vector3(0.15f, 0.18f, 0.18f), H("4A5560"), 0f, "Contrapunto");
        C(t, new Vector3(0f, 1.02f, 0f), new Vector3(0.8f, 0.04f, 0.04f), KitchenProps.Steel, 0f, "Pieza");
        C(t, new Vector3(-0.75f, 1.0f, 0f), new Vector3(0.05f, 0.45f, 0.45f), Iron, 0f, "Volante");
    }

    static void WindTunnel(Transform t)
    {
        C(t, new Vector3(0f, 0.95f, 0f), new Vector3(0.5f, 0.05f, 1.8f), Wood, 0f, "Tapa");
        C(t, new Vector3(0f, 0.75f, 0f), new Vector3(0.5f, 0.05f, 1.8f), Wood, 0f, "Fondo");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.25f, 0.85f, 0f), new Vector3(0.03f, 0.25f, 1.8f), Wood, 0f, "Lado");
        foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) }) C(t, new Vector3(x * 0.22f, 0.36f, z * 0.8f), new Vector3(0.05f, 0.72f, 0.05f), WoodDark, 0f, "Pata");
        C(t, new Vector3(0f, 0.85f, -0.95f), new Vector3(0.45f, 0.45f, 0.06f), Iron, 45f, "Ventilador");
        C(t, new Vector3(0f, 0.85f, 0.2f), new Vector3(0.2f, 0.01f, 0.06f), Brass, 0f, "Balanza_de_sustentacion");
    }

    static void WoodenDummy(Transform t)
    {
        C(t, new Vector3(0f, 0.85f, 0f), new Vector3(0.3f, 1.7f, 0.3f), Wood, 0f, "Tronco");
        C(t, new Vector3(0f, 0.85f, 0f), new Vector3(0.3f, 1.7f, 0.3f), Wood, 45f, "Tronco");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.12f, 1.25f, -0.35f), new Vector3(0.06f, 0.06f, 0.4f), WoodDark, k * 10f, "Brazo_alto");
        C(t, new Vector3(0f, 1.0f, -0.35f), new Vector3(0.06f, 0.06f, 0.4f), WoodDark, 0f, "Brazo_medio");
        C(t, new Vector3(0f, 0.35f, -0.3f), new Vector3(0.07f, 0.55f, 0.07f), WoodDark, 0f, "Pierna");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.45f, 1.0f, 0.15f), new Vector3(0.06f, 2.0f, 0.06f), WoodDark, 0f, "Poste_del_bastidor");
        for (int k = 0; k < 2; k++) C(t, new Vector3(0f, 1.5f - k * 0.9f, 0.15f), new Vector3(0.96f, 0.06f, 0.06f), WoodDark, 0f, "Travesano");
    }

    static void Altar(Transform t)
    {
        C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 0.9f, 0.7f), Stone, 0f, "Mesa_de_ofrendas");
        C(t, new Vector3(0f, 0.92f, 0f), new Vector3(1.5f, 0.05f, 0.8f), H("D8CFBE"), 0f, "Losa");
        for (int k = 0; k < 3; k++) C(t, new Vector3(-0.45f + k * 0.45f, 1.05f, 0f), new Vector3(0.18f, 0.22f, 0.18f), Clay, 0f, "Vasija");
        C(t, new Vector3(0.6f, 0.98f, 0.2f), new Vector3(0.06f, 0.08f, 0.06f), Ember, 0f, "Lamparilla");
    }

    static void Stall(Transform t, int seed)
    {
        TableTop(t, new Vector2(1.8f, 0.9f), 0.85f, Wood);
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.88f, 1.0f, 0.42f), new Vector3(0.05f, 2.0f, 0.05f), WoodDark, 0f, "Poste");
        C(t, new Vector3(0f, 2.0f, 0.2f), new Vector3(1.9f, 0.03f, 0.6f), H("B5462F"), 0f, "Toldo");
        var rnd = new System.Random(seed);
        for (int k = 0; k < 5; k++)
        {
            Color c = new[] { H("E8C35A"), H("C8763E"), H("8CB070"), H("D9CBA0"), Clay }[rnd.Next(5)];
            C(t, new Vector3(-0.7f + k * 0.35f, 0.92f, 0f), new Vector3(0.28f, 0.14f, 0.28f), Sack, 0f, "Cesto");
            C(t, new Vector3(-0.7f + k * 0.35f, 1.0f, 0f), new Vector3(0.22f, 0.04f, 0.22f), c, 0f, "Mercancia");
        }
        Bench(t, new Vector3(0f, 0f, 0.75f), 1.2f);
    }

    static void Granary(Transform t)
    {
        for (int k = 0; k < 4; k++) C(t, new Vector3(-0.5f, 0.3f + k * 0.4f, 0f), new Vector3(1.2f - k * 0.15f, 0.4f, 1.2f - k * 0.15f), Clay, k * 22f, "Silo");
        C(t, new Vector3(-0.5f, 1.9f, 0f), new Vector3(0.5f, 0.12f, 0.5f), WoodDark, 0f, "Tapa");
        C(t, new Vector3(0.75f, 0.12f, 0f), new Vector3(0.6f, 0.24f, 0.4f), Stone, 0f, "Molino_de_mano");
        C(t, new Vector3(0.75f, 0.28f, 0f), new Vector3(0.3f, 0.08f, 0.2f), Stone, 0f, "Mano_del_molino");
        C(t, new Vector3(0.75f, 0.25f, -0.35f), new Vector3(0.3f, 0.2f, 0.3f), Sack, 0f, "Saco_de_cebada");
    }

    static void PotterWheel(Transform t)
    {
        C(t, new Vector3(0f, 0.2f, 0f), new Vector3(0.5f, 0.4f, 0.5f), WoodDark, 0f, "Pie");
        C(t, new Vector3(0f, 0.42f, 0f), new Vector3(0.6f, 0.04f, 0.6f), Wood, 45f, "Plato_del_torno");
        C(t, new Vector3(0f, 0.52f, 0f), new Vector3(0.24f, 0.16f, 0.24f), Clay, 0f, "Vasija");
        Bench(t, new Vector3(0f, 0f, -0.6f), 0.6f);
        for (int k = 0; k < 4; k++) C(t, new Vector3(0.55f, 0.06f + k * 0.07f, 0.2f), new Vector3(0.2f, 0.07f, 0.2f), Clay, 0f, "Cuencos_de_borde_biselado");
    }

    static void Sandbags(Transform t)
    {
        for (int row = 0; row < 4; row++)
            for (int k = 0; k < 6; k++)
                C(t, new Vector3(-2.5f + k * 1.0f + (row % 2) * 0.5f, 0.12f + row * 0.24f, 0f), new Vector3(0.95f, 0.24f, 0.5f), Sack, (k + row) % 2 == 0 ? 2f : -2f, "Saco_terrero");
    }

    // POZO: brocal de piedra, agua al fondo, dos postes con travesaño, torno con soga y cubo, y una pila al lado.
    static void Well(Transform t)
    {
        for (int k = 0; k < 8; k++) C(t, Quaternion.Euler(0f, k * 45f, 0f) * new Vector3(0f, 0.4f, 0.55f), new Vector3(0.45f, 0.8f, 0.22f), Stone, k * 45f, "Brocal");
        C(t, new Vector3(0f, 0.15f, 0f), new Vector3(0.8f, 0.04f, 0.8f), Water, 22f, "Agua");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.6f, 1.1f, 0f), new Vector3(0.1f, 2.2f, 0.1f), WoodDark, 0f, "Poste");
        C(t, new Vector3(0f, 1.9f, 0f), new Vector3(1.3f, 0.12f, 0.12f), Wood, 0f, "Torno");
        C(t, new Vector3(0.68f, 1.9f, -0.12f), new Vector3(0.04f, 0.3f, 0.04f), WoodDark, 0f, "Manivela");
        C(t, new Vector3(0f, 1.45f, 0f), new Vector3(0.02f, 0.9f, 0.02f), Straw, 0f, "Soga");
        C(t, new Vector3(0f, 0.92f, 0f), new Vector3(0.24f, 0.26f, 0.24f), Wood, 0f, "Cubo");
        C(t, new Vector3(1.0f, 0.2f, 0f), new Vector3(0.5f, 0.4f, 0.8f), Stone, 0f, "Pila");
        C(t, new Vector3(1.0f, 0.38f, 0f), new Vector3(0.4f, 0.03f, 0.7f), Water, 0f, "Agua_de_la_pila");
    }

    // TELAR de bajo lizo: bastidor, enjulio de urdimbre y de tela, los hilos, el lizo, el peine, los pedales y el banco del tejedor.
    static void Loom(Transform t)
    {
        foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) }) C(t, new Vector3(x * 0.7f, 0.75f, z * 0.6f), new Vector3(0.08f, 1.5f, 0.08f), WoodDark, 0f, "Pie");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(0f, 0.85f, k * 0.6f), new Vector3(1.5f, 0.1f, 0.1f), Wood, 0f, k < 0 ? "Enjulio_de_tela" : "Enjulio_de_urdimbre");
        for (int k = 0; k < 12; k++) C(t, new Vector3(-0.55f + k * 0.1f, 0.88f, 0f), new Vector3(0.01f, 0.01f, 1.2f), Linen, 0f, "Hilo_de_urdimbre");
        C(t, new Vector3(0f, 0.86f, -0.35f), new Vector3(1.2f, 0.02f, 0.5f), H("A0523A"), 0f, "Tela_tejida");
        C(t, new Vector3(0f, 1.3f, 0.1f), new Vector3(1.3f, 0.5f, 0.04f), Wood, 0f, "Lizos");
        C(t, new Vector3(0f, 1.0f, -0.15f), new Vector3(1.3f, 0.25f, 0.05f), WoodDark, 0f, "Peine");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.2f, 0.06f, 0.2f), new Vector3(0.12f, 0.04f, 0.6f), Wood, 0f, "Pedal");
        C(t, new Vector3(0.3f, 0.9f, -0.3f), new Vector3(0.25f, 0.05f, 0.06f), Wood, 0f, "Lanzadera");
        Bench(t, new Vector3(0f, 0f, -0.95f), 1.0f);
    }

    static void Mat(Transform t, string key, Vector2 size)
    {
        if (key == "bancal")
        {
            C(t, new Vector3(0f, 0.08f, 0f), new Vector3(size.x, 0.16f, size.y), Soil, 0f, "Tierra");
            for (float z = -size.y * 0.5f + 0.3f; z < size.y * 0.5f; z += 0.5f)
                for (float x = -size.x * 0.5f + 0.25f; x < size.x * 0.5f; x += 0.5f) C(t, new Vector3(x, 0.24f, z), new Vector3(0.18f, 0.16f, 0.18f), Green, (x + z) * 40f, "Mata");
            return;
        }
        Color c = key == "tatami" ? H("C9BC86") : H("9A7346");
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(size.x, 0.06f, size.y), c, 0f, key == "tatami" ? "Tatami" : "Esterilla");
        if (key == "tatami")
        {
            for (int k = 0; k < 4; k++) C(t, Quaternion.Euler(0f, k * 90f, 0f) * new Vector3(0f, 0.065f, (k % 2 == 0 ? size.y : size.x) * 0.5f - 0.04f), new Vector3(k % 2 == 0 ? size.x : size.y, 0.01f, 0.08f), H("3B4A2E"), k * 90f, "Borde");
            for (float x = -size.x * 0.5f + 1f; x < size.x * 0.5f - 0.1f; x += 1f) C(t, new Vector3(x, 0.062f, 0f), new Vector3(0.02f, 0.005f, size.y - 0.1f), H("B3A673"), 0f, "Junta");
        }
    }
}
