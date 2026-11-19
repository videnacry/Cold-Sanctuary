using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HAWTHORNE 2008, EL DISTRITO DESDE CERO (PB-165, Berón 2026-11-19: «primero montamos un buen distrito sin huecos, bien estructurado,
/// como GTA en cuanto a estar lleno de edificaciones y espacios urbanos»). Plan, plano y fuentes: docs/hawthorne-distrito.md.
///
/// El microcosmos ya NO se monta sobre el piso del santuario (MicrocosmSpec.ownGround): es un rectángulo urbano de
/// <see cref="HawthorneGround"/> que esta historia llena entero. Coordenadas del plano en el marco de la FÁBRICA (F: su centro en el
/// origen, x = este, z = norte); la fábrica (StorySceneBuilder.Musk) queda al oeste de Crenshaw Boulevard, como en la realidad.
///
///   I-105 elevada + estación Crenshaw de la Green Line (z 115…140) ·· park-and-ride (oeste) · casas de Hollypark (este)
///   W 120th St (z 58…78)
///   aeropuerto (terminal con Nat's, hangares) · aparcamiento norte · fábrica + nave anexa · bomberos ║ Crenshaw (x 54…86) ║ gasolinera ·
///     strip mall (donuts, taquería, lavandería, licorería) · supermercado · taller de coches · apartamentos ║ Wilkie Ave ║ naves y casas
///   Rocket Road (z −40…−28) · aparcamiento sur de SpaceX · naves del Century Business Center
///
/// REGLAS de este distrito:
/// - edificios CONTIGUOS y calles con todo su mobiliario (farolas cobra, postes de madera con cables, semáforos de báculo, hidrantes,
///   paradas de Metro, bordillos de color del California Vehicle Code §21458, pasos de cebra, palmeras, plazas pintadas, coches);
/// - cada edificio con FACHADA DE VERDAD (escaparates o ventanas, rótulo, toldo, azotea con climatización) e INTERIOR con la maquinaria
///   de su uso (lo que hace falta para usarlo);
/// - en Hawthorne todos son insectos-humanos: SOLO mobiliario de insecto-humano (mesa alta con apoyo de abdomen; nada de sillas de persona).
/// [C] = confirmado con fuente, [S] = supuesto (docs/hawthorne-distrito.md §3).
/// </summary>
public static partial class StorySceneBuilder
{
    /// <summary>El rectángulo urbano (m): de la nave de Northrop al oeste a las naves de Wilkie Ave al este, y de Rocket Road a la I-105.</summary>
    internal static Vector2 HawthorneGround => new Vector2(496f, 322f);   // (propiedad: la lee el inicializador de OwnGround, en otro archivo)
    /// <summary>El centro del rectángulo urbano en el plano F (ya corrido por <see cref="Spread"/>).</summary>
    static Vector2 HawthorneCenterF => new Vector2(46.5f, 7f);
    /// <summary>Dónde va la fábrica respecto al centro del rectángulo (la historia está en su centro): el distrito, centrado.</summary>
    static Vector3 HawthorneShift => new Vector3(-HawthorneCenterF.x, 0f, -10f - HawthorneCenterF.y);
    /// <summary>(Berón 2026-11-19 06:21) La fábrica, ×2 en planta (sus muebles y máquinas ×2, su paso ×2) y sus muros, puertas y ventanas ×3:
    /// de 44 × 30 m a 88 × 60 m y de 9 a 27 m de alto (<see cref="ScaleFactory"/>).</summary>
    internal const float MuskK = 2f;

    // ── materiales del distrito (mates, sin brillo: regla de materiales eficientes) ──
    static Material _asph, _conc, _curb, _paint, _yellowPaint, _glass, _glassDark, _metal, _dark, _wood, _grass, _redCurb;
    static void DistrictMaterials()
    {
        _asph = M("Asfalto_Crenshaw", 0.22f, 0.22f, 0.23f); _conc = M("Acera_hormigon", 0.74f, 0.73f, 0.7f); _curb = M("Bordillo", 0.82f, 0.81f, 0.78f);
        _paint = M("Pintura_vial_blanca", 0.95f, 0.95f, 0.93f); _yellowPaint = M("Pintura_vial_amarilla", 0.95f, 0.78f, 0.15f);
        _glass = M("Escaparate", 0.55f, 0.68f, 0.75f); _glassDark = M("Vidrio_oscuro", 0.2f, 0.27f, 0.32f); _metal = M("Acero_galvanizado", 0.62f, 0.64f, 0.65f);
        _dark = M("Negro_mate", 0.09f, 0.09f, 0.1f); _wood = M("Poste_de_madera", 0.42f, 0.32f, 0.22f); _grass = M("Cesped_regado", 0.36f, 0.55f, 0.27f);
        _redCurb = M("Bordillo_rojo", 0.78f, 0.15f, 0.12f);
    }

    /// <summary>Un punto del plano (marco de la fábrica) en locales de la historia.</summary>
    static Vector3 D(float x, float z, float y = 0f) { Vector2 q = Spread(x, z); return MuskFactoryAt + new Vector3(q.x, y, q.y); }
    /// <summary>Un punto del plano F tal cual (sin <see cref="Spread"/>): lo que va pegado a la fábrica ya agrandada.</summary>
    static Vector3 DRaw(float x, float z, float y = 0f) => MuskFactoryAt + new Vector3(x, y, z);
    /// <summary>EL DISTRITO SE ABRE PARA LA FÁBRICA ×2: el plano se dibujó con la nave de 44 × 30 m; lo que queda al este de x 20 se corre
    /// 50 m al este, lo del oeste de x −30, 46 m al oeste, y lo del norte de z 20 y del sur de z −20, 16 m (la nave pasa a 88 × 60 m, y la
    /// nave anexa y los bomberos quedan a su lado). Las calles se corren por sus extremos, así que siguen enteras.</summary>
    static Vector2 Spread(float x, float z) => new Vector2(x > 20f ? x + 50f : x < -30f ? x - 46f : x, z > 20f ? z + 16f : z < -20f ? z - 16f : z);

    static Transform Node(Transform parent, string name, Vector3 local, float yaw = 0f)
    {
        var g = new GameObject(name).transform; g.SetParent(parent, false); g.localPosition = local; g.localRotation = Quaternion.Euler(0f, yaw, 0f);
        return g;
    }

    // ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════
    /// <summary>El distrito entero (lo que no es la fábrica, que la levanta <see cref="ElonMusk"/>). Devuelve cuántos personajes puso.</summary>
    static int HawthorneDistrict(Transform t, Transform fac)
    {
        DistrictMaterials();
        _shellParts.Clear();
        // «Calles de…»: el constructor del microcosmos lo reconoce como PUEBLO (AreaMicrocosmos.InTown): sus calles y edificios no son el
        // claro de la historia, y la flora crece entre ellos (ronda del 2026-11-19: «flora dentro del piso: 0 plantas»).
        Transform city = Node(t, "Calles de Hawthorne (2008)", Vector3.zero);
        Streets(city);
        UrbanFurniture(city);
        var sites = new Dictionary<string, AreaFunctionSite>();
        void Add(AreaFunctionSite s) { if (s != null) sites[s.title] = s; }
        Add(FireStation(city));
        Add(AirportTerminal(city));
        Hangars(city);
        FreewayAndStation(city);
        ParkAndRide(city);
        Add(GasStation(city));
        foreach (AreaFunctionSite x in StripMall(city)) Add(x);
        Add(Supermarket(city));
        Add(AutoShop(city));
        Add(Apartments(city));
        foreach (AreaFunctionSite x in Houses(city)) Add(x);
        foreach (AreaFunctionSite x in Warehouses(city)) Add(x);
        Add(Gym(city));
        Add(Clinic(city));
        SpaceXLots(city);
        int lowered = ClampUsable(city);
        int cleared = 0, spread = 0;
        foreach (AreaFunctionSite x in sites.Values) { Physics.SyncTransforms(); cleared += ClearDoors(x.transform); spread += Declutter(x.transform); }
        MoveEntry(t);
        int n = CivicBuilder.HawthornePeople(t, fac.GetComponent<AreaFunctionSite>(), sites);
        MicroBuildRules.Validate(t, "Hawthorne (2008)");
        Debug.Log($"[Hawthorne] distrito: {sites.Count} edificios con interior, {lowered} superficies de uso bajadas al pecho ({ChestHeight:0.00} m), {cleared} piezas apartadas de las puertas, {spread} corridas para dejar el paso, {n} vecinos y trabajadores del barrio.");
        return n;
    }

    /// <summary>La entrada (MobSpawnPoint) y la salida (WorldExitPortal) delante de la puerta de la fábrica, en su explanada (la plantilla
    /// las pone a 14 m del centro del rectángulo, que aquí cae en la nave anexa).</summary>
    static void MoveEntry(Transform t)
    {
        Transform story = t.parent;
        if (story == null) return;
        Transform spawn = story.Find("MobSpawnPoint"), portal = story.Find("WorldExitPortal");
        Vector3 Local(Vector3 d) => story.InverseTransformPoint(t.TransformPoint(d));
        if (spawn != null) { spawn.localPosition = Local(D(6f, -21f, 1f)); spawn.localRotation = Quaternion.LookRotation(Vector3.forward); }
        if (portal != null) portal.localPosition = Local(D(16f, -21f));
    }

    // ═══ CALLES ══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>Una calle recta paralela a un eje (de <paramref name="a"/> a <paramref name="b"/>, en el plano F), con su asfalto, sus
    /// carriles, su mediana (si <paramref name="median"/> &gt; 0), sus bordillos y sus aceras de <paramref name="walk"/> m.</summary>
    static Transform Road(Transform parent, string name, Vector2 a, Vector2 b, float width, int lanesPerSide, float median, float walk, bool centerYellow = true)
    {
        a = Spread(a.x, a.y); b = Spread(b.x, b.y);
        bool alongZ = Mathf.Abs(b.y - a.y) > Mathf.Abs(b.x - a.x);
        float len = alongZ ? Mathf.Abs(b.y - a.y) : Mathf.Abs(b.x - a.x);
        Vector2 c = (a + b) * 0.5f;
        Transform r = Node(parent, name, DRaw(c.x, c.y), alongZ ? 0f : 90f);   // en locales: la calle va a lo largo de z
        Prop(r, "Asfalto", new Vector3(0f, 0.03f, 0f), new Vector3(width, 0.06f, len), _asph, false);
        float half = width * 0.5f;
        // Mediana con bordillo y palmeras, o doble línea amarilla.
        if (median > 0f)
        {
            Prop(r, "Mediana", new Vector3(0f, 0.12f, 0f), new Vector3(median, 0.18f, len - 30f), _curb, true);
            Prop(r, "Cesped_de_la_mediana", new Vector3(0f, 0.215f, 0f), new Vector3(median - 0.5f, 0.02f, len - 31f), _grass, false);
            for (float z = -len * 0.5f + 22f; z < len * 0.5f - 22f; z += 24f) Palm(r, new Vector3(0f, 0.22f, z), "Palmera de la mediana (Washingtonia robusta)");
        }
        else if (centerYellow)
            for (int s = -1; s <= 1; s += 2) Prop(r, "Doble_linea_amarilla", new Vector3(s * 0.12f, 0.065f, 0f), new Vector3(0.1f, 0.01f, len), _yellowPaint, false);
        // Las líneas discontinuas entre carriles (3 m pintados cada 12 m, el estándar de Caltrans) y la línea de borde.
        float lane = (half - median * 0.5f - 2.4f) / Mathf.Max(1, lanesPerSide);
        for (int s = -1; s <= 1; s += 2)
        {
            for (int k = 1; k < lanesPerSide; k++)
            {
                float x = s * (median * 0.5f + k * lane);
                for (float z = -len * 0.5f + 4f; z < len * 0.5f - 4f; z += 12f) Prop(r, "Linea_de_carril", new Vector3(x, 0.065f, z), new Vector3(0.12f, 0.01f, 3f), _paint, false);
            }
            Prop(r, "Linea_de_borde", new Vector3(s * (half - 2.3f), 0.065f, 0f), new Vector3(0.12f, 0.01f, len), _paint, false);
            // Bordillo (rojo junto a los cruces y las paradas: prohibido parar, CVC §21458) y acera.
            Prop(r, "Bordillo", new Vector3(s * (half + 0.1f), 0.08f, 0f), new Vector3(0.2f, 0.16f, len), _curb, false);
            foreach (float e in new[] { -1f, 1f }) Prop(r, "Bordillo_rojo", new Vector3(s * (half + 0.1f), 0.17f, e * (len * 0.5f - 6f)), new Vector3(0.22f, 0.01f, 12f), _redCurb, false);
            if (walk > 0f) Prop(r, "Acera", new Vector3(s * (half + walk * 0.5f), 0.08f, 0f), new Vector3(walk, 0.16f, len), _conc, false);
            if (walk > 0f) for (float z = -len * 0.5f + 1.5f; z < len * 0.5f; z += 1.5f) Prop(r, "Junta_de_acera", new Vector3(s * (half + walk * 0.5f), 0.165f, z), new Vector3(walk, 0.005f, 0.04f), _curb, false);
        }
        return r;
    }

    /// <summary>Un paso de cebra (de franjas anchas, el «continental» de California) de <paramref name="len"/> m, a lo largo de x local.</summary>
    static void Crosswalk(Transform parent, Vector3 at, float len, float yaw)
    {
        Transform cw = Node(parent, "Paso_de_cebra", at, yaw);
        for (float x = -len * 0.5f + 0.5f; x < len * 0.5f; x += 1.2f) Prop(cw, "Franja", new Vector3(x, 0.07f, 0f), new Vector3(0.6f, 0.01f, 3.6f), _paint, false);
        Prop(cw, "Linea_de_detencion", new Vector3(0f, 0.07f, -3.2f), new Vector3(len, 0.01f, 0.4f), _paint, false);
    }

    static void Streets(Transform city)
    {
        // Crenshaw Boulevard [C]: 3 carriles por sentido, mediana con giros a la izquierda, aceras de ~3–4 m [S].
        Road(city, "Crenshaw Boulevard", new Vector2(70f, -133f), new Vector2(70f, 147f), 32f, 3, 4.6f, 4f);
        // W 120th Street [C]: 2 por sentido y carril central de giro [S].
        Road(city, "West 120th Street", new Vector2(-150f, 68f), new Vector2(240f, 68f), 20f, 2, 0f, 4f);
        // Rocket Road (el acceso a la fábrica desde Crenshaw) [S en su sección].
        Road(city, "Rocket Road", new Vector2(-60f, -34f), new Vector2(54f, -34f), 12f, 1, 0f, 4f);
        // Wilkie Avenue [C: «cruzando Crenshaw» desde la sede], residencial e industrial.
        Road(city, "Wilkie Avenue", new Vector2(182f, -133f), new Vector2(182f, 58f), 12f, 1, 0f, 4f);
        // La calle de servicio de la antigua planta de Northrop, entre la nave y el aeropuerto [S].
        Road(city, "Calle de servicio de la planta de Northrop", new Vector2(-42f, -40f), new Vector2(-42f, 58f), 9f, 1, 0f, 3f);
        // Los pasos de cebra de los cruces con semáforo.
        Transform x = Node(city, "Pasos de cebra", Vector3.zero);
        Crosswalk(x, D(70f, 55f), 32f, 0f); Crosswalk(x, D(70f, 81f), 32f, 180f);
        Crosswalk(x, D(50f, 68f), 20f, 90f); Crosswalk(x, D(90f, 68f), 20f, 270f);
        Crosswalk(x, D(70f, -44f), 32f, 0f); Crosswalk(x, D(70f, -24f), 32f, 180f); Crosswalk(x, D(50f, -34f), 12f, 90f);
        // Los cruces se pavimentan encima de las aceras.
        Prop(x, "Asfalto_cruce_Crenshaw_120", D(70f, 68f, 0.035f), new Vector3(32f, 0.07f, 20f), _asph, false);
        Prop(x, "Asfalto_cruce_Crenshaw_Rocket_Road", D(52f, -34f, 0.035f), new Vector3(4.5f, 0.07f, 12f), _asph, false);
        Prop(x, "Asfalto_cruce_Wilkie_120", D(182f, 56f, 0.035f), new Vector3(12f, 0.07f, 4.5f), _asph, false);
        Prop(x, "Asfalto_cruce_servicio_120", D(-42f, 56f, 0.035f), new Vector3(9f, 0.07f, 4.5f), _asph, false);
        Prop(x, "Asfalto_cruce_servicio_Rocket_Road", D(-42f, -34f, 0.035f), new Vector3(9f, 0.07f, 12f), _asph, false);
        // El suelo de las manzanas (asfalto viejo de polígono y tierra con césped en las casas) bajo todo, sin huecos.
        // Va con el suelo del piso (fuera de la historia): ni es una calle donde no pueda crecer nada ni agranda el claro de la historia.
        Transform slab = Prop(city, "Suelo_del_distrito", DRaw(HawthorneCenterF.x, HawthorneCenterF.y, -0.01f), new Vector3(HawthorneGround.x, 0.02f, HawthorneGround.y), M("Asfalto_viejo", 0.36f, 0.35f, 0.34f), false);
        for (Transform p = city; p != null; p = p.parent) if (p.name == "Historia") { if (p.parent != null) slab.SetParent(p.parent, true); break; }
    }

    // ═══ MOBILIARIO URBANO ═══════════════════════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>Farola «cobra» de LA (báculo gris galvanizado de ~9 m con el brazo curvo; [S], el modelo de los años 60–2000).</summary>
    static void Cobra(Transform p, Vector3 at, float yaw)
    {
        Transform l = Node(p, "Farola cobra", at, yaw);
        Prop(l, "Baculo", new Vector3(0f, 4.5f, 0f), new Vector3(0.22f, 9f, 0.22f), _metal, true);
        Prop(l, "Brazo", new Vector3(0f, 8.9f, 1.4f), new Vector3(0.14f, 0.14f, 2.8f), _metal, false);
        Prop(l, "Luminaria", new Vector3(0f, 8.75f, 2.9f), new Vector3(0.45f, 0.22f, 0.9f), _metal, false);
        Prop(l, "Base", new Vector3(0f, 0.35f, 0f), new Vector3(0.45f, 0.7f, 0.45f), _conc, false);
    }

    /// <summary>Poste de madera de la compañía eléctrica (Southern California Edison), con travesaño, aisladores y un transformador cada
    /// tres postes; los cables los tienden <see cref="Wires"/>.</summary>
    static void UtilityPole(Transform p, Vector3 at, bool transformer)
    {
        Transform u = Node(p, "Poste de madera (tendido eléctrico)", at);
        Prop(u, "Poste", new Vector3(0f, 5.5f, 0f), new Vector3(0.3f, 11f, 0.3f), _wood, true);
        Prop(u, "Traviesa", new Vector3(0f, 10.4f, 0f), new Vector3(2.4f, 0.15f, 0.15f), _wood, false);
        for (int k = -1; k <= 1; k++) Prop(u, "Aislador", new Vector3(k * 1f, 10.6f, 0f), new Vector3(0.1f, 0.25f, 0.1f), M("Aislador_gris", 0.55f, 0.58f, 0.55f), false);
        if (transformer) Prop(u, "Transformador", new Vector3(0f, 8.2f, 0.35f), new Vector3(0.55f, 1f, 0.55f), M("Transformador_gris", 0.5f, 0.53f, 0.52f), false);
    }

    static void Wires(Transform p, Vector3 a, Vector3 b)
    {
        Vector3 d = b - a; float len = new Vector2(d.x, d.z).magnitude; if (len < 1f) return;
        float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        for (int k = -1; k <= 1; k++)
        {
            Vector3 off = Quaternion.Euler(0f, yaw, 0f) * new Vector3(k * 1f, 0f, 0f);
            AreaMicrocosmos.Box(p, "Cable", (a + b) * 0.5f + off + Vector3.up * 10.3f, new Vector3(0.03f, 0.03f, len), Quaternion.Euler(0f, yaw, 0f), _dark, false);
        }
    }

    /// <summary>Semáforo de báculo con brazo largo (el de Caltrans), cabeza de tres luces y pulsador peatonal.</summary>
    static void Signal(Transform p, Vector3 at, float yaw, float arm)
    {
        Transform s = Node(p, "Semáforo", at, yaw);
        Material pole = M("Baculo_semaforo", 0.55f, 0.57f, 0.55f), head = M("Cabeza_semaforo", 0.85f, 0.7f, 0.1f);
        Prop(s, "Baculo", new Vector3(0f, 3.5f, 0f), new Vector3(0.3f, 7f, 0.3f), pole, true);
        Prop(s, "Brazo", new Vector3(arm * 0.5f, 6.8f, 0f), new Vector3(arm, 0.22f, 0.22f), pole, false);
        foreach (float x in new[] { arm * 0.45f, arm * 0.8f })
        {
            Prop(s, "Cabeza", new Vector3(x, 6.0f, 0f), new Vector3(0.45f, 1.2f, 0.35f), head, false);
            Prop(s, "Luz_roja", new Vector3(x, 6.4f, -0.18f), new Vector3(0.28f, 0.28f, 0.02f), M("Luz_roja", 0.9f, 0.1f, 0.1f), false);
            Prop(s, "Luz_ambar", new Vector3(x, 6.0f, -0.18f), new Vector3(0.28f, 0.28f, 0.02f), M("Luz_ambar", 0.95f, 0.6f, 0.1f), false);
            Prop(s, "Luz_verde", new Vector3(x, 5.6f, -0.18f), new Vector3(0.28f, 0.28f, 0.02f), M("Luz_verde", 0.1f, 0.8f, 0.35f), false);
        }
        Prop(s, "Placa_de_calle", new Vector3(arm * 0.25f, 7.3f, 0f), new Vector3(2.4f, 0.45f, 0.05f), M("Placa_verde", 0.1f, 0.42f, 0.25f), false);
        Prop(s, "Senal_peatonal", new Vector3(0f, 3f, -0.25f), new Vector3(0.4f, 0.45f, 0.2f), _dark, false);
        Prop(s, "Pulsador_peatonal", new Vector3(0f, 1.1f, -0.18f), new Vector3(0.14f, 0.2f, 0.08f), head, false);
    }

    /// <summary>Parada de Metro de 2008: marquesina de acero con techo curvo, panel de publicidad, banco… y el poste de la «M». Las del
    /// Metro Rapid 710 llevan el rojo de la marca [C: la línea; S: el color de la marquesina].</summary>
    static Transform BusStop(Transform p, string name, Vector3 at, float yaw, bool rapid)
    {
        Transform b = Node(p, name, at, yaw);
        Material frame = rapid ? M("Rojo_Metro_Rapid", 0.75f, 0.12f, 0.12f) : M("Marquesina_gris", 0.35f, 0.37f, 0.4f);
        foreach (float x in new[] { -1.9f, 1.9f }) Prop(b, "Pilar", new Vector3(x, 1.3f, 0.5f), new Vector3(0.12f, 2.6f, 0.12f), frame, true);
        Prop(b, "Techo_curvo", new Vector3(0f, 2.7f, 0.1f), new Vector3(4.2f, 0.12f, 1.8f), frame, false);
        Prop(b, "Mampara_trasera", new Vector3(0f, 1.4f, 0.95f), new Vector3(3.6f, 1.8f, 0.05f), _glass, true);
        Prop(b, "Panel_de_publicidad", new Vector3(-2.0f, 1.3f, 0.4f), new Vector3(0.12f, 1.8f, 1.2f), M("Anuncio_luminoso", 0.85f, 0.85f, 0.75f), true);
        Prop(b, "Banco_de_apoyo_(insecto-humano)", new Vector3(0.6f, 0.45f, 0.6f), new Vector3(2.2f, 0.12f, 0.5f), _metal, true);
        Prop(b, "Poste_de_la_parada", new Vector3(2.6f, 1.6f, -0.6f), new Vector3(0.08f, 3.2f, 0.08f), _metal, true);
        Prop(b, "Placa_M_de_Metro", new Vector3(2.6f, 2.9f, -0.6f), new Vector3(0.5f, 0.5f, 0.04f), rapid ? frame : M("Naranja_Metro", 0.92f, 0.45f, 0.1f), false);
        Prop(b, "Horario", new Vector3(2.6f, 1.8f, -0.56f), new Vector3(0.35f, 0.6f, 0.02f), _paint, false);
        Prop(b, "Papelera", new Vector3(-1.4f, 0.45f, -0.6f), new Vector3(0.55f, 0.9f, 0.55f), M("Papelera_verde", 0.2f, 0.32f, 0.22f), true);
        Prop(b, "Bordillo_blanco", new Vector3(0f, 0.17f, -1.95f), new Vector3(12f, 0.01f, 0.22f), _paint, false);
        Label(b, name, 3.4f);
        return b;
    }

    static void Hydrant(Transform p, Vector3 at)
    {
        Transform h = Node(p, "Hidrante", at);
        Material y = M("Hidrante_amarillo", 0.92f, 0.78f, 0.12f);
        Prop(h, "Cuerpo", new Vector3(0f, 0.4f, 0f), new Vector3(0.3f, 0.8f, 0.3f), y, true);
        Prop(h, "Tapas", new Vector3(0f, 0.55f, 0f), new Vector3(0.55f, 0.14f, 0.14f), y, false);
        Prop(h, "Bordillo_rojo", new Vector3(0f, 0.17f, -0.7f), new Vector3(9f, 0.01f, 0.22f), _redCurb, false);
    }

    static void UrbanFurniture(Transform city)
    {
        Transform f = Node(city, "Mobiliario urbano", Vector3.zero);
        // Farolas cobra cada ~32 m en Crenshaw (las dos aceras, mirando a la calzada), en la 120 y en Rocket Road.
        for (float z = -125f; z <= 140f; z += 32f) { Cobra(f, D(52.6f, z), 90f); Cobra(f, D(87.4f, z + 16f), 270f); }
        for (float x = -140f; x <= 235f; x += 34f) { if (Mathf.Abs(x - 70f) < 24f) continue; Cobra(f, D(x, 56.6f), 0f); }
        for (float x = -50f; x <= 40f; x += 30f) Cobra(f, D(x, -27f), 180f);
        // Los postes de madera con su tendido: por la acera norte de la 120 y por Wilkie (en Crenshaw iba soterrado [S]).
        var north = new List<Vector3>(); for (float x = -146f; x <= 236f; x += 38f) { if (Mathf.Abs(x - 70f) < 20f) continue; north.Add(D(x, 80.8f)); }
        var wilk = new List<Vector3>(); for (float z = -128f; z <= 50f; z += 36f) wilk.Add(D(190.8f, z));
        foreach (var line in new[] { north, wilk })
            for (int i = 0; i < line.Count; i++) { UtilityPole(f, line[i], i % 3 == 1); if (i > 0) Wires(f, line[i - 1], line[i]); }
        // Semáforos: Crenshaw y la 120, Crenshaw y Rocket Road (este último con el nombre que buscan las misiones).
        Transform c1 = Node(f, "Cruce de Crenshaw Boulevard y West 120th Street (semáforos)", D(70f, 68f));
        foreach (var (x, z, yaw) in new[] { (-17.5f, -11.5f, 0f), (17.5f, -11.5f, 270f), (17.5f, 11.5f, 180f), (-17.5f, 11.5f, 90f) }) Signal(c1, new Vector3(x, 0f, z), yaw, 12f);
        Transform c2 = Node(f, "Cruce de Crenshaw Boulevard y Rocket Road (semáforos)", D(70f, -34f));
        foreach (var (x, z, yaw) in new[] { (-17.5f, -8f, 0f), (17.5f, -8f, 270f), (17.5f, 8f, 180f), (-17.5f, 8f, 90f) }) Signal(c2, new Vector3(x, 0f, z), yaw, 12f);
        Label(c2, "Cruce de Crenshaw Boulevard y Rocket Road", 8f);
        // STOP de cuatro vías en Wilkie y la 120 [S].
        foreach (var (x, z) in new[] { (175.5f, 59.5f), (188.5f, 76.5f) })
        {
            Transform s = Node(f, "Señal de STOP", D(x, z));
            Prop(s, "Poste", new Vector3(0f, 1.3f, 0f), new Vector3(0.07f, 2.6f, 0.07f), _metal, true);
            Prop(s, "Octogono", new Vector3(0f, 2.4f, -0.05f), new Vector3(0.75f, 0.75f, 0.03f), M("Rojo_STOP", 0.8f, 0.1f, 0.1f), false);
        }
        // Paradas de Metro en Crenshaw: la 210 (local) junto a la 120 y el Rapid 710 junto a Rocket Road, en las dos aceras [C: líneas].
        BusStop(f, "Parada de Metro 210 (Crenshaw / 120th, hacia el norte)", D(88f, 36f), 90f, false);
        BusStop(f, "Parada de Metro 210 (Crenshaw / 120th, hacia el sur)", D(52f, 96f), 270f, false);
        BusStop(f, "Parada del Metro Rapid 710 (Crenshaw / Rocket Road, hacia el norte)", D(88f, -58f), 90f, true);
        BusStop(f, "Parada del Metro Rapid 710 (Crenshaw / Rocket Road, hacia el sur)", D(52f, -14f), 270f, true);
        // Hidrantes cada ~90 m de acera (el código de incendios de LA) y unas papeleras.
        foreach (var (x, z) in new[] { (52.4f, 24f), (87.6f, 0f), (52.4f, -70f), (87.6f, -96f), (87.6f, 100f), (-30f, 56.4f), (120f, 56.4f), (220f, 79.6f), (174f, -60f), (190f, -20f), (-20f, -26.4f) })
            Hydrant(f, D(x, z));
        foreach (var (x, z) in new[] { (52.6f, 50f), (87.4f, 62f), (87.4f, -30f), (52.6f, -40f), (100f, 79.6f), (20f, 56.6f) })
            Prop(f, "Papelera", D(x, z, 0.45f), new Vector3(0.55f, 0.9f, 0.55f), M("Papelera_verde", 0.2f, 0.32f, 0.22f), true);
        // Jacarandas en alcorques en la acera este de Crenshaw (las palmeras van en la mediana y en las aceras de la nave).
        for (float z = -118f; z <= 130f; z += 22f)
        {
            if (Mathf.Abs(z - 36f) < 6f || Mathf.Abs(z + 58f) < 6f || Mathf.Abs(z - 68f) < 16f || Mathf.Abs(z + 34f) < 10f) continue;
            Jacaranda(f, D(89f, z));
        }
        for (float z = -118f; z <= 46f; z += 24f) { if (Mathf.Abs(z + 34f) < 10f || Mathf.Abs(z + 14f) < 5f) continue; Palm(f, D(51.6f, z)); }
        for (float z = -30f; z <= 50f; z += 10f) Palm(f, D(-48f, z));   // la fila de palmeras de la calle de servicio (la del piloto)
        // La patrulla de la policía de Hawthorne (Hawthorne PD, 12501 S. Hawthorne Blvd. [C]; Crown Victoria blanca y negra [S]) en Rocket Road.
        Transform car = Node(f, "Patrulla de la policía de Hawthorne", D(-20f, -31.5f), 90f);
        Prop(car, "Carroceria", new Vector3(0f, 0.65f, 0f), new Vector3(1.95f, 0.7f, 5.4f), M("Negro_patrulla", 0.06f, 0.06f, 0.08f), true);
        Prop(car, "Puertas_blancas", new Vector3(0f, 0.7f, 0.2f), new Vector3(1.97f, 0.5f, 2.2f), _paint, false);
        Prop(car, "Cabina", new Vector3(0f, 1.25f, -0.2f), new Vector3(1.8f, 0.5f, 2.6f), _glassDark, false);
        Prop(car, "Puente_de_luces", new Vector3(0f, 1.55f, 0f), new Vector3(1.4f, 0.12f, 0.3f), M("Luz_roja", 0.9f, 0.1f, 0.1f), false);
        Prop(car, "Puente_de_luces_azul", new Vector3(0.4f, 1.56f, 0f), new Vector3(0.5f, 0.13f, 0.31f), M("Luz_azul", 0.1f, 0.3f, 0.95f), false);
    }

    static void Jacaranda(Transform p, Vector3 at)
    {
        Transform j = Node(p, "Jacaranda (Jacaranda mimosifolia)", at);
        Prop(j, "Alcorque", new Vector3(0f, 0.17f, 0f), new Vector3(1.4f, 0.02f, 1.4f), M("Tierra_alcorque", 0.36f, 0.27f, 0.2f), false);
        Prop(j, "Tronco", new Vector3(0f, 2f, 0f), new Vector3(0.3f, 4f, 0.3f), M("Corteza_gris", 0.4f, 0.36f, 0.32f), true);
        Prop(j, "Copa", new Vector3(0f, 5f, 0f), new Vector3(5f, 2.6f, 5f), M("Jacaranda_verde_lila", 0.45f, 0.5f, 0.48f), false);
        Prop(j, "Flores", new Vector3(0.6f, 6.1f, 0.3f), new Vector3(3.2f, 0.8f, 3.2f), M("Jacaranda_lila", 0.6f, 0.5f, 0.78f), false);
    }

    // ═══ EDIFICIOS ═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// UN EDIFICIO DEL DISTRITO, cerrado, con su FACHADA de verdad. Planta <paramref name="size"/> centrada en <paramref name="center"/>
    /// (plano F); la fachada principal mira a <paramref name="faceYaw"/> (0 = sur, 90 = oeste, 180 = norte, 270 = este; en locales del
    /// edificio la fachada está siempre en −z). Escaparate (<paramref name="storefront"/>: vidrio del zócalo al dintel, con montantes) o
    /// ventanas en banda; puerta (<see cref="MicroDoor"/> con su paso para la navegación), rótulo, toldo opcional, azotea con pretil y
    /// climatización, ventanas en los laterales y vista de corte. Devuelve el edificio (con su <see cref="AreaFunctionSite"/>).
    /// </summary>
    static AreaFunctionSite Shell(Transform parent, string title, string template, Vector2 center, Vector2 size, float faceYaw, float h,
                                  Color wall, Color trim, bool storefront, string sign, Color signColor, float doorW = 1.8f, float doorX = 0f,
                                  bool awning = false, Color? awningColor = null, bool backDoor = true, bool sideWindows = true)
    {
        Transform b = Node(parent, title, D(center.x, center.y), 0f);
        h *= ShellK; doorW *= ShellK;   // (Berón 2026-11-19) puertas, muros y ventanas ×3
        // En locales del edificio, la fachada está en −z: se gira para que mire a faceYaw.
        b.localRotation = Quaternion.Euler(0f, faceYaw, 0f);   // (−z local girado faceYaw: 0 → sur, 90 → oeste, 180 → norte, 270 → este)
        // Si gira 90° o 270°, el ancho de la fachada es el lado z del plano.
        bool swap = Mathf.Abs(Mathf.DeltaAngle(faceYaw, 90f)) < 1f || Mathf.Abs(Mathf.DeltaAngle(faceYaw, 270f)) < 1f;
        Vector2 s = swap ? new Vector2(size.y, size.x) : size;
        float hx = s.x * 0.5f, hz = s.y * 0.5f, th = CivicBuilder.WallThick;
        Material mw = M("Muro_" + title.GetHashCode().ToString("X"), wall.r, wall.g, wall.b), mt = M("Remate_" + trim.GetHashCode().ToString("X"), trim.r, trim.g, trim.b);
        // Suelo (no «Suelo»: CivicBuilder.Settle corre los edificios con «Suelo»; aquí nada se corre).
        Prop(b, "Losa", new Vector3(0f, 0.05f, 0f), new Vector3(s.x, 0.1f, s.y), M("Suelo_interior_terrazo", 0.78f, 0.76f, 0.72f), false);
        // Muros laterales y trasero, con ventanas altas en los laterales.
        Prop(b, "Muro_norte", new Vector3(0f, h * 0.5f, hz - th * 0.5f), new Vector3(s.x, h, th), mw, true);
        foreach (int sd in new[] { -1, 1 })
        {
            Prop(b, sd < 0 ? "Muro_oeste" : "Muro_este", new Vector3(sd * (hx - th * 0.5f), h * 0.5f, 0f), new Vector3(th, h, s.y), mw, true);
            // Ventanas en los costados (no en los locales pegados a otro: ahí el muro es medianero). «Muro_…»: la vista de corte las
            // desvanece con su muro (ronda del 2026-11-19: dentro se veían paneles oscuros flotando donde se había ocultado el muro).
            if (sideWindows)
                for (float z = -hz + 4f; z < hz - 3f; z += 7f)
                    Prop(b, "Muro_ventana_lateral", new Vector3(sd * (hx + 0.01f), h * 0.55f, z), new Vector3(0.02f, Mathf.Min(1.2f * ShellK, h * 0.45f), Mathf.Min(1.6f * ShellK, 3.5f)), _glassDark, false);
        }
        if (backDoor) Prop(b, "Puerta_trasera_de_servicio", new Vector3(hx * 0.6f, 1.05f * ShellK, hz + 0.02f), new Vector3(1.0f * ShellK, 2.1f * ShellK, 0.05f), M("Puerta_metal_gris", 0.45f, 0.46f, 0.48f), false);
        // Fachada (−z): zócalo, escaparate o ventanas, montantes, dintel y la puerta.
        float dw = MicroDoor.BuildWidth(doorW), a1 = doorX - dw * 0.5f, b0 = doorX + dw * 0.5f;
        float doorH = Mathf.Min(2.2f * ShellK, h - 0.6f);
        MicroDoor door = MicroDoor.Place(b, "Puerta", new Vector3(doorX, 0f, -hz + th * 0.5f), 0f, doorW, doorH, "puerta real ×3 (Berón 2026-11-19)");
        door.height = Mathf.Max(door.height, doorH);
        CivicBuilder.DoorLink(door, th);
        float head = Mathf.Min(h - 0.8f, (storefront ? 3.1f : 2.4f) * ShellK);
        void Facade(float x0, float x1)
        {
            if (x1 - x0 < 0.05f) return;
            float cx = (x0 + x1) * 0.5f, w = x1 - x0;
            Prop(b, "Muro_sur_zocalo", new Vector3(cx, storefront ? 0.25f : 0.5f * ShellK, -hz + th * 0.5f), new Vector3(w, storefront ? 0.5f : 1.0f * ShellK, th), mt, true);
            Prop(b, "Muro_sur_dintel", new Vector3(cx, (head + h) * 0.5f, -hz + th * 0.5f), new Vector3(w, h - head, th), mw, true);
            float sill = storefront ? 0.5f : 1.0f * ShellK;
            // Vidrio entre zócalo y dintel (sólido: no se cruza), con montantes cada ~1,8 m.
            Prop(b, storefront ? "Muro_escaparate" : "Muro_ventana", new Vector3(cx, (sill + head) * 0.5f, -hz + th * 0.5f), new Vector3(w, head - sill, 0.06f), storefront ? _glass : _glassDark, true);
            // Montantes ×3 (cada ~5,4 m, uno cada 1,8 m reales), más gruesos, y un travesaño a la altura de la puerta: la carpintería
            // del escaparate (antes, a 1,8 m y finos, eran barrotes de 9 m).
            for (float x = x0; x <= x1 + 0.01f; x += Mathf.Max(3.6f, w / Mathf.Max(1, Mathf.Round(w / (1.8f * ShellK)))))
                Prop(b, "Muro_montante", new Vector3(x, (sill + head) * 0.5f, -hz + th * 0.5f - 0.05f), new Vector3(0.25f, head - sill, 0.12f), mt, false);
            if (head - sill > 4f) Prop(b, "Muro_travesano", new Vector3(cx, Mathf.Min(head - 0.5f, 2.2f * ShellK), -hz + th * 0.5f - 0.05f), new Vector3(w, 0.2f, 0.12f), mt, false);
        }
        Facade(-hx, a1); Facade(b0, hx);
        Prop(b, "Muro_sur_dintel_puerta", new Vector3(doorX, (doorH + h) * 0.5f, -hz + th * 0.5f), new Vector3(dw, h - doorH, th), mw, true);
        // La hoja de la puerta, corrida por dentro junto al hueco (antes salía en ángulo hacia la acera).
        Prop(b, "Muro_hoja_corredera", new Vector3(doorX - dw * 0.5f - Mathf.Min(dw * 0.5f, 2f) * 0.5f, doorH * 0.5f, -hz + th + 0.05f), new Vector3(Mathf.Min(dw * 0.5f, 2f), doorH - 0.1f, 0.05f), _glass, false);
        // Rótulo sobre la fachada y toldo.
        // El rótulo, del ancho de SU fachada y con las letras ajustadas al panel (antes las letras medían 0,9 m cada una y se salían:
        // se montaban con las del local de al lado y, vistas por detrás, salían en espejo). En las casas, el número junto a la puerta.
        bool plaque = sign.Length <= 6;
        float pw = plaque ? 1.4f : Mathf.Min(s.x - 1.5f, Mathf.Max(4f, sign.Length * 0.9f)), ph = plaque ? 0.6f : Mathf.Min(2.2f, h * 0.18f);
        Vector3 signAt = plaque ? new Vector3(doorX + dw * 0.5f + 1.2f, Mathf.Min(doorH, 2.6f), -hz - 0.1f) : new Vector3(0f, Mathf.Max(doorH + ph * 0.5f + 0.3f, h - 1.6f), -hz - 0.12f);
        Transform signT = Node(b, "Rotulo", signAt);
        Prop(signT, "Muro_rotulo", Vector3.zero, new Vector3(pw, ph, 0.12f), M("Rotulo_" + signColor.GetHashCode().ToString("X"), signColor.r, signColor.g, signColor.b), false);
        // Letras de TextMesh (fontSize 60): ~0,1 × characterSize m de alto por punto; ancho medio de una letra ≈ 0,55 × su alto.
        float cs = Mathf.Min(ph * 0.7f / 6f, pw * 0.9f / Mathf.Max(1, sign.Length) / (0.55f * 6f));
        Transform letters = Text3D(signT, sign, new Vector3(0f, 0f, -0.08f), cs, Color.white);
        letters.localRotation = Quaternion.identity; letters.name = "Muro_letras_" + sign;
        if (awning)
        {
            Color ac = awningColor ?? new Color(0.7f, 0.15f, 0.12f);
            AreaMicrocosmos.Box(b, "Toldo", new Vector3(0f, head + 0.6f, -hz - 1.5f), new Vector3(s.x - 0.4f, 0.1f, 3.2f), Quaternion.Euler(-18f, 0f, 0f), M("Toldo_" + ac.GetHashCode().ToString("X"), ac.r, ac.g, ac.b), false);
        }
        // Azotea: cubierta (sin colisión), pretil y equipos de climatización (las «rooftop units» de LA).
        Prop(b, "Cubierta", new Vector3(0f, h + 0.15f, 0f), new Vector3(s.x, 0.3f, s.y), M("Cubierta_bitumen", 0.5f, 0.5f, 0.5f), false);
        foreach (int sd in new[] { -1, 1 })
        {
            Prop(b, "Pretil", new Vector3(0f, h + 0.6f, sd * (hz - 0.1f)), new Vector3(s.x, 0.9f, 0.2f), mw, false);
            Prop(b, "Pretil", new Vector3(sd * (hx - 0.1f), h + 0.6f, 0f), new Vector3(0.2f, 0.9f, s.y), mw, false);
        }
        int units = Mathf.Clamp(Mathf.RoundToInt(s.x * s.y / 150f), 1, 6);
        for (int k = 0; k < units; k++)
            Prop(b, "Climatizador_de_azotea", new Vector3(-hx + (k + 0.5f) * s.x / units, h + 0.9f, hz * 0.3f), new Vector3(2.2f, 1.2f, 1.5f), M("Climatizador", 0.72f, 0.73f, 0.72f), false);
        var site = b.gameObject.AddComponent<AreaFunctionSite>(); site.building = template; site.title = title; site.region = CivicBuilder.Region;
        CivicBuilder.Tag(site);
        Label(b, title, h + 1.6f);
        CutawayView.Ensure(b);
        _shellParts[b] = b.childCount;   // lo de aquí en adelante es su interior (ClearDoors, Declutter)
        return site;
    }

    // ═══ LA FÁBRICA A LA NUEVA ESCALA (Berón 2026-11-19 06:21) ══════════════════════════════════════════════════════════════════════
    static readonly string[] RealSize = { "Falcon", "Carretilla", "Camión", "Furgoneta", "Bandera", "Rótulo", "Rotulo", "Garita", "Pozo", "Entrega", "Tarjeta", "Motor" };
    static readonly string[] TallWall = { "Muro", "Dintel", "Nervio", "Ventana", "Puerta_enrollable", "Lama", "Sala_cristal" };
    static readonly string[] Overhead = { "Cubierta", "Tragaluz" };
    static bool Starts(string n, string[] l) { foreach (string p in l) if (n.StartsWith(p)) return true; return false; }

    /// <summary>
    /// LA FÁBRICA DE SPACEX A LA ESCALA DEL DISTRITO: el piloto se hizo a la medida del cuerpo encogido (44 × 30 m, muros de 9 m). Aquí se
    /// agranda tal cual está montada: todo ×<see cref="MuskK"/> en planta (las distancias, los muebles, las máquinas, el paso entre ellos,
    /// los muros y el suelo) y los muros, puertas, ventanas y la cubierta ×<see cref="ShellK"/> en alto. Lo que tiene su medida real (la
    /// etapa del Falcon 1, la carretilla, los camiones, la bandera, los rótulos) solo cambia de sitio. Las puertas (MicroDoor) se ensanchan
    /// con su hueco y su paso de NavMesh. La gente y lo que la historia puso junto a la nave (en <paramref name="t"/>) se corre con ella:
    /// lo de dentro, ×2 desde el centro; lo de fuera (la nave anexa, la lonchera), con el distrito (<see cref="Spread"/>).
    /// </summary>
    static void ScaleFactory(Transform t, Transform fac)
    {
        float k = MuskK, ky = ShellK;
        var kids = new List<Transform>(); foreach (Transform c in fac) kids.Add(c);
        foreach (Transform c in kids)
        {
            Vector3 lp = c.localPosition;
            MicroDoor door = c.GetComponent<MicroDoor>();
            if (door != null)
            {
                c.localPosition = new Vector3(lp.x * k, lp.y, lp.z * k);
                door.width *= k; door.height *= ky;
                foreach (Unity.AI.Navigation.NavMeshLink link in c.GetComponentsInChildren<Unity.AI.Navigation.NavMeshLink>(true)) link.width = Mathf.Max(0.5f, door.width - 0.2f);
                continue;
            }
            bool tall = Starts(c.name, TallWall), over = Starts(c.name, Overhead);
            c.localPosition = new Vector3(lp.x * k, tall || over ? lp.y * ky : lp.y, lp.z * k);
            if (Starts(c.name, RealSize)) { if (c.name.StartsWith("Rótulo de SpaceX")) c.localPosition += Vector3.up * 14f; continue; }
            Vector3 ls = c.localScale;
            // Las vigas de la grúa crecen a lo largo, no a lo ancho: en tramos de < 4 m² la vista de corte no las toma por techo.
            bool beam = c.name.StartsWith("Viga") || c.name.StartsWith("Puente_de");
            c.localScale = new Vector3(ls.x * (beam && ls.x < ls.z ? 1f : k), tall ? ls.y * ky : ls.y, ls.z * (beam && ls.z < ls.x ? 1f : k));
        }
        // Las etiquetas y letras (TextMesh) no se estiran: vuelven a su proporción.
        foreach (TextMesh tm in fac.GetComponentsInChildren<TextMesh>(true))
        {
            Vector3 l = tm.transform.lossyScale;
            if (Mathf.Abs(l.x) < 1e-4f || Mathf.Abs(l.z) < 1e-4f) continue;
            float u = Mathf.Abs(l.y);
            tm.transform.localScale = Vector3.Scale(tm.transform.localScale, new Vector3(u / Mathf.Abs(l.x), 1f, u / Mathf.Abs(l.z)));
        }
        // Lo de la historia junto a la nave: la gente de dentro, ×2 desde el centro; lo de fuera, con el distrito.
        Vector3 o = fac.localPosition; float hx = MuskFactorySize.x * 0.5f + 4f, hz = MuskFactorySize.y * 0.5f + 9f;
        foreach (Transform c in t)
        {
            if (c == fac) continue;
            Vector3 d = c.localPosition - o;
            if (Mathf.Abs(d.x) <= hx && Mathf.Abs(d.z) <= hz) c.localPosition = o + new Vector3(d.x * k, d.y, d.z * k);
            else { Vector2 q = Spread(d.x, d.z); c.localPosition = o + new Vector3(q.x, d.y, q.y); }
        }
    }

    // ═══ ESCALA (Berón 2026-11-19 06:21) ═══════════════════════════════════════════════════════════════════════════════════════════
    // «puertas y paredes y ventanas *3, en objetos *2 y la restricción de altura para muebles que deben estar bajo el pecho para usarse».
    // El cuerpo del insecto-humano no cambia (s = 0,5). Los objetos de este archivo (máquinas, coches, vitrinas) van a su medida REAL,
    // que ya es el doble de la del cuerpo encogido; los muebles del registro (MicroFurniture) se hacen a la medida del cuerpo y aquí se
    // agrandan ×2 en planta (<see cref="DPiece"/>); las superficies de uso se bajan al pecho (<see cref="ClampUsable"/>).
    /// <summary>Puertas, muros y ventanas ×3.</summary>
    internal const float ShellK = 3f;
    /// <summary>Muebles y objetos ×2 en X y Z.</summary>
    internal const float FurnitureK = 2f;
    /// <summary>La altura del pecho del insecto-humano (de pie, torso erguido): el tope de lo que se usa con las manos.</summary>
    internal static float ChestHeight { get { MicroBody.Fit f = MicroBody.OfSpecies("Ant"); return Mathf.Max(f.hands + 0.1f, f.height * 0.72f); } }

    /// <summary>Un mueble del registro a la medida del cuerpo, agrandado ×2 en planta (la altura, la suya: ya le queda a las manos).</summary>
    static Transform DPiece(Transform parent, string key, Vector3 at, float yaw, string label, Vector2? size = null, MicroBody.Fit? body = null, IList<MicroBody.Fit> users = null, bool station = true)
    {
        Transform p = Piece(parent, key, at, yaw, label, size, body, users, station);
        p.localScale = new Vector3(p.localScale.x * FurnitureK, p.localScale.y, p.localScale.z * FurnitureK);
        return p;
    }

    static readonly string[] Usable = { "Mostrador", "Barra", "Mesa_", "Cinta_", "Vitrina", "Mueble", "Oficina_del_taller", "Banco_del", "Isla_", "Cubeta", "Caja_registradora", "Bandeja", "Comal", "Base_del_comal", "Fregadero", "Freidora", "Vaporera", "Lavadora", "Fruta", "Escritorio", "Sierra", "Cafeteras", "Maquina_de_loteria" };
    /// <summary>Baja al pecho del cuerpo las superficies de uso (mostradores, barras, mesas, cintas, vitrinas, fregaderos, lavadoras de
    /// carga superior…) que se hicieron a la medida de una persona, apoyadas en el mismo suelo; lo de encima (la caja, las cubetas,
    /// la fruta) baja con ellas. Devuelve cuántas bajó.</summary>
    static int ClampUsable(Transform root)
    {
        float chest = ChestHeight; int n = 0;
        var tops = new List<(Transform t, float drop)>();
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
        {
            Transform c = r.transform;
            if (c.GetComponentInParent<MicroPiece>() != null) continue;   // los muebles del registro ya van a la medida
            bool use = false; foreach (string k in Usable) if (c.name.StartsWith(k)) { use = true; break; }
            if (!use) continue;
            float h = c.localScale.y, bottom = c.localPosition.y - h * 0.5f;
            if (bottom > 1.2f) continue;               // algo en una planta alta o colgado: no es un mueble del suelo
            float top = bottom + h;
            if (top <= chest) continue;
            float drop = top - chest;
            if (h > 0.2f) { float nh = Mathf.Max(0.05f, chest - Mathf.Max(0f, bottom)); c.localScale = new Vector3(c.localScale.x, nh, c.localScale.z); c.localPosition = new Vector3(c.localPosition.x, Mathf.Max(0f, bottom) + nh * 0.5f, c.localPosition.z); }
            else c.localPosition -= Vector3.up * drop;  // una pieza fina encima (bandeja, cubeta, caja registradora): baja a la superficie
            n++;
        }
        return n;
    }

    // ═══ EL PASO Y LAS PUERTAS, COMPROBADOS CON LAS CAJAS DE VERDAD (ronda del 2026-11-19) ════════════════════════════════════════
    // Los muebles del registro miden lo que su cuerpo pide (y aquí ×2 en planta): sus medidas no se saben al escribir el plano. Tras
    // amueblar, cada edificio se repasa con las cajas reales: nada tapa una puerta (la oficina de la nave del proveedor tapaba la suya y
    // su maquinista no salía: camino PARCIAL, MIC-93) y entre muebles queda el paso de 2,4 m (MIC-87: 2,30 m en los bomberos).

    /// <summary>Hijos de cada edificio de <see cref="Shell"/> que son su cáscara (los de después son su interior).</summary>
    static readonly Dictionary<Transform, int> _shellParts = new Dictionary<Transform, int>();

    static readonly string[] Fixed = { "Muro", "Losa", "Suelo", "Cubierta", "Tejado", "Forjado", "Peldaño", "Galeria", "Cesped", "Valla", "Buzon", "Rotulo", "Pretil", "Climatizador", "Puerta", "Mancha", "Viga", "Tabique", "Campana", "Menu", "Television", "Fotos", "Espejo", "Tensiometro", "Tarjeta", "Numero" };

    /// <summary>La planta útil del edificio, en sus locales (dentro de los muros, con 0,3 m de margen).</summary>
    static bool Interior(Transform b, out Bounds inner)
    {
        inner = default;
        Transform slab = b.Find("Losa");
        if (slab == null) return false;
        float mx = slab.localScale.x * 0.5f - CivicBuilder.WallThick - 0.3f, mz = slab.localScale.z * 0.5f - CivicBuilder.WallThick - 0.3f;
        inner = new Bounds(Vector3.zero, new Vector3(2f * mx, 200f, 2f * mz));
        return true;
    }

    /// <summary>La caja de <paramref name="w"/> (mundo) en los locales de <paramref name="frame"/>.</summary>
    static Bounds ToLocal(Transform frame, Bounds w)
    {
        Bounds l = new Bounds(frame.InverseTransformPoint(w.center), Vector3.zero);
        for (int i = 0; i < 8; i++)
            l.Encapsulate(frame.InverseTransformPoint(new Vector3((i & 1) == 0 ? w.min.x : w.max.x, (i & 2) == 0 ? w.min.y : w.max.y, (i & 4) == 0 ? w.min.z : w.max.z)));
        return l;
    }

    /// <summary>La caja (mundo) de lo que se ve de <paramref name="t"/>, sin las etiquetas de texto ni la gente.</summary>
    static bool SolidBox(Transform t, out Bounds b)
    {
        b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>(true))
        {
            if (r.GetComponent<TextMesh>() != null || r.GetComponentInParent<CubeInsect>(true) != null) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        return any;
    }

    /// <summary>¿Es <paramref name="c"/> (hijo del edificio, de su interior) un mueble o una máquina que se puede correr?</summary>
    static bool Movable(Transform c)
    {
        foreach (string f in Fixed) if (c.name.StartsWith(f)) return false;
        if (c.GetComponentInChildren<MicroDoor>(true) != null || c.GetComponentInChildren<Collider>(true) == null) return false;
        return SolidBox(c, out Bounds b) && b.size.y > 0.3f;
    }

    /// <summary>Lo que estorba (mundo) en el edificio <paramref name="b"/>, salvo <paramref name="self"/>: sus muebles, máquinas y
    /// tabiques a ras de suelo (no el suelo, los forjados ni lo colgado).</summary>
    static List<Bounds> Obstacles(Transform b, Transform self)
    {
        var l = new List<Bounds>();
        for (int i = 0; i < b.childCount; i++)
        {
            Transform c = b.GetChild(i);
            if (c == self || self.IsChildOf(c) || c.name.StartsWith("Losa") || c.name.StartsWith("Forjado") || c.name.StartsWith("Galeria") || c.name.StartsWith("Cubierta")) continue;
            if (c.GetComponentInChildren<Collider>(true) == null || !SolidBox(c, out Bounds w)) continue;
            if (w.min.y > b.position.y + 1.5f || w.size.y < 0.2f) continue;
            l.Add(w);
        }
        return l;
    }

    static bool Overlaps(Bounds a, Bounds b, float pad) => a.min.x < b.max.x + pad && a.max.x > b.min.x - pad && a.min.z < b.max.z + pad && a.max.z > b.min.z - pad;

    /// <summary>Corre <paramref name="t"/> (de caja <paramref name="w"/>) el desplazamiento más corto de <paramref name="moves"/> (mundo) que lo
    /// deja dentro del edificio sin pisar otra cosa (ni a menos de <paramref name="gap"/> de otro mueble). Devuelve si lo corrió.</summary>
    static bool Shove(Transform b, Transform t, Bounds w, List<Vector3> moves, Bounds inner, float gap, List<(Transform t, Bounds b)> pieces)
    {
        moves.Sort((x, y) => x.sqrMagnitude.CompareTo(y.sqrMagnitude));
        List<Bounds> obs = Obstacles(b, t);
        foreach (Vector3 m in moves)
        {
            Bounds n = w; n.center += m;
            Bounds l = ToLocal(b, n);
            if (l.min.x < inner.min.x || l.max.x > inner.max.x || l.min.z < inner.min.z || l.max.z > inner.max.z) continue;
            bool clash = false;
            foreach (Bounds o in obs) if (!o.Intersects(w) && Overlaps(n, o, 0.05f)) { clash = true; break; }
            if (!clash && pieces != null)
                foreach (var p in pieces)
                    if (p.t != t && !p.t.IsChildOf(t) && !t.IsChildOf(p.t) && p.b.max.y > n.min.y + 0.3f && n.max.y > p.b.min.y + 0.3f && MicroBuildRules.GapXZ(n, p.b) < gap) { clash = true; break; }
            if (clash) continue;
            t.position += m;
            return true;
        }
        return false;
    }

    /// <summary>Despeja cada puerta del edificio (la de la calle y las de sus tabiques): ningún mueble ni máquina a menos de 2,6 m de
    /// su hueco por ningún lado. Devuelve cuántas piezas corrió.</summary>
    static int ClearDoors(Transform b)
    {
        if (!_shellParts.TryGetValue(b, out int first) || !Interior(b, out Bounds inner)) return 0;
        int moved = 0;
        foreach (MicroDoor d in b.GetComponentsInChildren<MicroDoor>(true))
        {
            float half = d.width * 0.5f + 0.4f, reach = 2.6f;
            for (int i = first; i < b.childCount; i++)
            {
                Transform c = b.GetChild(i);
                if (!Movable(c) || !SolidBox(c, out Bounds w) || w.min.y > b.position.y + 1.5f) continue;
                Bounds l = ToLocal(d.transform, w);
                if (l.max.x < -half || l.min.x > half || l.max.z < -reach || l.min.z > reach) continue;
                var moves = new List<Vector3>
                {
                    d.transform.TransformVector(new Vector3(0f, 0f, reach + 0.05f - l.min.z)),
                    d.transform.TransformVector(new Vector3(0f, 0f, -reach - 0.05f - l.max.z)),
                    d.transform.TransformVector(new Vector3(half + 0.05f - l.min.x, 0f, 0f)),
                    d.transform.TransformVector(new Vector3(-half - 0.05f - l.max.x, 0f, 0f)),
                };
                for (int k = 0; k < moves.Count; k++) moves[k] = new Vector3(moves[k].x, 0f, moves[k].z);
                if (Shove(b, c, w, moves, inner, 0f, null)) moved++;
                else Debug.LogWarning($"[Hawthorne] «{c.name}» tapa la puerta «{d.name}» de «{b.name}» y no cabe en otro sitio.");
            }
        }
        return moved;
    }

    /// <summary>El paso de <see cref="MicroBuildRules.Passage"/> entre los muebles de <paramref name="b"/> (los que mide el validador, a la
    /// misma altura): corre el que cabe con el desplazamiento más corto. Devuelve cuántas veces corrió uno.</summary>
    static int Declutter(Transform b)
    {
        if (!Interior(b, out Bounds inner)) return 0;
        float gap = MicroBuildRules.Passage + 0.05f;
        int moved = 0;
        for (int round = 0; round < 60; round++)
        {
            var ps = MicroBuildRules.Pieces(b);
            bool fixedOne = false;
            for (int i = 0; i < ps.Count && !fixedOne; i++)
                for (int j = i + 1; j < ps.Count && !fixedOne; j++)
                {
                    Bounds A = ps[i].b, B = ps[j].b;
                    if (ps[i].t.IsChildOf(ps[j].t) || ps[j].t.IsChildOf(ps[i].t)) continue;
                    if (A.max.y < B.min.y + 0.3f || B.max.y < A.min.y + 0.3f) continue;   // en plantas distintas
                    if (MicroBuildRules.GapXZ(A, B) >= gap - 0.04f) continue;
                    bool Try(Transform mt, Bounds w, Bounds o)
                    {
                        if (mt == b || mt.GetComponentInParent<MicroDoor>(true) != null) return false;
                        var moves = new List<Vector3>
                        {
                            new Vector3(o.max.x + gap - w.min.x, 0f, 0f), new Vector3(o.min.x - gap - w.max.x, 0f, 0f),
                            new Vector3(0f, 0f, o.max.z + gap - w.min.z), new Vector3(0f, 0f, o.min.z - gap - w.max.z),
                        };
                        return Shove(b, mt, w, moves, inner, gap - 0.04f, ps);
                    }
                    if (Try(ps[j].t, B, A) || Try(ps[i].t, A, B)) { fixedOne = true; moved++; }
                    if (!fixedOne) Debug.LogWarning($"[Hawthorne] «{b.name}»: no hay sitio para dejar {MicroBuildRules.Passage:0.0} m entre «{ps[i].t.name}» y «{ps[j].t.name}».");
                }
            if (!fixedOne) break;
        }
        return moved;
    }

    // En locales de un edificio de Shell: x a lo largo de la fachada, z hacia el fondo (la fachada en −z).
    static readonly List<MicroBody.Fit> InsectOnly1 = new List<MicroBody.Fit>();
    static List<MicroBody.Fit> Insects => InsectOnly1.Count > 0 ? InsectOnly1 : FillInsects();
    static List<MicroBody.Fit> FillInsects() { InsectOnly1.Clear(); InsectOnly1.Add(MicroBody.OfSpecies("Ant")); return InsectOnly1; }

    /// <summary>Mesa alta de insecto-humano con sus apoyos de abdomen (C1), no un puesto.</summary>
    static Transform InsectTable(Transform b, Vector3 at, string label, Vector2? size = null)
        => DPiece(b, "mesa", at, 0f, label, size ?? new Vector2(1.6f, 0.9f), null, Insects, station: false);

    /// <summary>Una fila de estantería de supermercado o tienda (góndola) con sus baldas y producto de colores.</summary>
    static void Gondola(Transform b, string name, Vector3 at, float len, float yaw, Color[] goods)
    {
        Transform g = Node(b, name, at, yaw);
        Prop(g, "Montante_central", new Vector3(0f, 0.95f, 0f), new Vector3(len, 1.9f, 0.12f), _metal, true);
        for (int k = 0; k < 5; k++)
            foreach (int sd in new[] { -1, 1 })
            {
                Prop(g, "Balda", new Vector3(0f, 0.15f + k * 0.4f, sd * 0.3f), new Vector3(len, 0.03f, 0.5f), _metal, false);
                Color c = goods[(k + (sd > 0 ? 2 : 0)) % goods.Length];
                Prop(g, "Producto", new Vector3(0f, 0.3f + k * 0.4f, sd * 0.3f), new Vector3(len - 0.1f, 0.25f, 0.42f), M("Producto_" + c.GetHashCode().ToString("X"), c.r, c.g, c.b), false);
            }
        Prop(g, "Cartel_del_pasillo", new Vector3(0f, 2.6f, 0f), new Vector3(1.6f, 0.4f, 0.05f), M("Cartel_pasillo_azul", 0.15f, 0.3f, 0.6f), false);
        Label(g, name, 3.1f);
    }

    // ── 1 · ESTACIÓN DE BOMBEROS 162 DEL CONDADO (12151 S. Crenshaw Blvd. [C]) ──
    static AreaFunctionSite FireStation(Transform city)
    {
        var site = Shell(city, "Bomberos", "Comisaría", new Vector2(36f, 38f), new Vector2(26f, 30f), 270f, 6f,
            new Color(0.86f, 0.8f, 0.7f), new Color(0.6f, 0.2f, 0.15f), false, "LA COUNTY FIRE · STATION 162", new Color(0.6f, 0.12f, 0.1f), 4.2f, 4.5f, backDoor: true);
        site.title = "Bomberos (estación 162 del Condado de Los Ángeles)"; site.notHome = true;
        Transform b = site.transform;
        // Las dos cocheras (puertas seccionales levantadas), la autobomba y el camión escala [S: modelos de la época].
        Transform engine = Node(b, "Autobomba (Engine 162)", new Vector3(4.5f, 0f, 2f));
        Material red = M("Rojo_bomberos", 0.72f, 0.08f, 0.07f);
        Prop(engine, "Chasis", new Vector3(0f, 1.6f, 0f), new Vector3(2.5f, 2.4f, 9.5f), red, true);
        Prop(engine, "Cabina", new Vector3(0f, 2.9f, -3.4f), new Vector3(2.5f, 1.0f, 2.4f), red, false);
        Prop(engine, "Mangueras", new Vector3(0f, 2.95f, 1.5f), new Vector3(2.2f, 0.3f, 4f), M("Manguera_beige", 0.8f, 0.75f, 0.6f), false);
        foreach (var (x, z) in new[] { (-1.2f, -3f), (1.2f, -3f), (-1.2f, 3f), (1.2f, 3f) }) Prop(engine, "Rueda", new Vector3(x, 0.55f, z), new Vector3(0.4f, 1.1f, 1.1f), _dark, false);
        Prop(engine, "Puente_de_luces", new Vector3(0f, 3.5f, -4.2f), new Vector3(2f, 0.15f, 0.3f), M("Luz_roja", 0.9f, 0.1f, 0.1f), false);
        Prop(b, "Taquillas_de_los_trajes_de_intervención", new Vector3(-10f, 1f, 9f), new Vector3(5f, 2f, 0.6f), M("Taquilla_amarilla", 0.85f, 0.7f, 0.2f), true);
        Prop(b, "Compresor_de_botellas_de_aire", new Vector3(-10f, 0.8f, 4f), new Vector3(1.2f, 1.6f, 0.8f), _metal, true);
        DPiece(b, "cocina", new Vector3(-9f, 0f, -1.5f), 0f, "Cocina del parque de bomberos");
        InsectTable(b, new Vector3(-8f, 0f, -8.5f), "Mesa alta del parque (comen juntos)");
        DPiece(b, "cama", new Vector3(-3f, 0f, 11.5f), 0f, "Catre de guardia (24 h)", null, station: false);
        DPiece(b, "inodoro", new Vector3(-12f, 0f, 12.5f), 0f, "Aseo del parque", null, station: false);
        DPiece(b, "lavabo", new Vector3(-12f, 0f, 9.5f), 0f, "Lavabo del parque", null, station: false);
        Prop(b, "Mastil_de_bandera", new Vector3(-10f, 4f, -16f), new Vector3(0.12f, 8f, 0.12f), _metal, true);
        return site;
    }

    // ── 2 · TERMINAL DEL AEROPUERTO MUNICIPAL CON NAT'S AIRPORT CAFE ([C]: terminal de 2 plantas de 1966; Nat's desde 1994) ──
    static AreaFunctionSite AirportTerminal(Transform city)
    {
        var site = Shell(city, "Nat's Airport Cafe (terminal del aeropuerto de Hawthorne)", "Restaurante", new Vector2(-95f, 36f), new Vector2(36f, 22f), 0f, 7.6f,
            new Color(0.93f, 0.9f, 0.82f), new Color(0.3f, 0.45f, 0.6f), true, "NAT'S AIRPORT CAFE", new Color(0.2f, 0.35f, 0.6f), 1.8f, -10f, awning: true, awningColor: new Color(0.2f, 0.35f, 0.6f));
        Transform b = site.transform;
        // Planta baja: el café (barra con plancha y freidora, cafeteras, vitrina de tartas, mesas junto al ventanal de la pista).
        Prop(b, "Barra_del_cafe", new Vector3(8f, 0.55f, 4f), new Vector3(10f, 1.1f, 0.8f), M("Formica_roja", 0.7f, 0.18f, 0.15f), true);
        DPiece(b, "fogon", new Vector3(8f, 0f, 8.5f), 0f, "Plancha de los desayunos (Nat's)");
        Prop(b, "Freidora_doble", new Vector3(12f, 0.5f, 8.5f), new Vector3(0.8f, 1f, 0.8f), _metal, true);
        Prop(b, "Cafeteras_de_filtro", new Vector3(4f, 1.25f, 8.8f), new Vector3(1.4f, 0.4f, 0.4f), _dark, false);
        Prop(b, "Vitrina_de_tartas", new Vector3(13.5f, 1.2f, 4f), new Vector3(1.2f, 0.6f, 0.7f), _glass, false);
        Prop(b, "Caja_registradora", new Vector3(3.5f, 1.25f, 4f), new Vector3(0.45f, 0.3f, 0.4f), _dark, false);
        // (ronda del 2026-11-19) cada 6 m (a 4,6 m quedaban a 1,88 m), sin la columna de la puerta (x −10), y lejos de lo de arriba.
        foreach (float x in new[] { -16f, -4f, 2f }) InsectTable(b, new Vector3(x, 0f, -7f), "Mesa alta junto al ventanal de la pista");
        foreach (float x in new[] { -16f, -4f, 2f }) InsectTable(b, new Vector3(x, 0f, -1.5f), "Mesa alta del café");
        // Primera planta (a 3,8 m): el salón de banquetes con su bar [C], con la escalera.
        Prop(b, "Forjado_primera_planta", new Vector3(0f, 3.85f * ShellK, 6f), new Vector3(35f, 0.2f, 10f), M("Forjado_hormigon", 0.7f, 0.7f, 0.68f), true);
        for (int k = 0; k < 30; k++) Prop(b, "Peldaño", new Vector3(16f, 0.19f + k * 0.385f, -9f + k * 0.32f), new Vector3(2.4f, 0.385f, 0.32f), _conc, true);
        Prop(b, "Salon_barra_de_banquetes", new Vector3(12f, 3.85f * ShellK + 0.1f + ChestHeight * 0.5f, 9.5f), new Vector3(6f, ChestHeight, 1.2f), M("Madera_barra", 0.45f, 0.3f, 0.2f), true);
        // Arriba, sobre el forjado (z 1…11) y en planta lejos de las mesas y los muebles de abajo: el validador mide el paso en planta.
        foreach (float x in new[] { -11f, -4.5f, 2f }) InsectTable(b, new Vector3(x, 3.85f * ShellK + 0.1f, 6.5f), "Mesa del salón de banquetes (500 personas en la real)");
        Prop(b, "Fotos_de_aviones_de_Northrop", new Vector3(0f, 2.2f, 10.6f), new Vector3(10f, 1f, 0.04f), M("Fotos_sepia", 0.6f, 0.5f, 0.38f), false);
        DPiece(b, "inodoro", new Vector3(-16f, 0f, 9f), 0f, "Aseo del café", null, station: false);
        DPiece(b, "lavabo", new Vector3(-16f, 0f, 6f), 0f, "Lavabo del café", null, station: false);
        return site;
    }

    // ── 3 · HANGARES, TORRE Y PLATAFORMA (de fondo, con dos avionetas) ──
    static void Hangars(Transform city)
    {
        Transform a = Node(city, "Aeropuerto municipal de Hawthorne (Jack Northrop Field)", D(-100f, 0f));
        Material chapa = M("Hangar_chapa_ondulada", 0.74f, 0.75f, 0.76f), apron = M("Plataforma_hormigon", 0.62f, 0.62f, 0.6f);
        Prop(a, "Plataforma_de_estacionamiento", new Vector3(0f, 0.03f, 0f), new Vector3(100f, 0.06f, 26f), apron, false);
        for (int k = 0; k < 2; k++)
        {
            Transform h = Node(a, "Hangar de avionetas (chapa ondulada, años 50)", new Vector3(-30f + k * 40f, 0f, -26f));
            Prop(h, "Muro_trasero", new Vector3(0f, 4.5f, -12f), new Vector3(30f, 9f, 0.3f), chapa, true);
            foreach (int sd in new[] { -1, 1 }) Prop(h, "Muro_lateral", new Vector3(sd * 15f, 4.5f, -2f), new Vector3(0.3f, 9f, 20f), chapa, true);
            Prop(h, "Cubierta_curva", new Vector3(0f, 9.4f, -2f), new Vector3(31f, 0.8f, 21f), chapa, false);
            Prop(h, "Banco_del_mecanico_de_aviones", new Vector3(-10f, 0.5f, -10f), new Vector3(3f, 1f, 0.9f), _metal, true);
            Prop(h, "Carro_de_combustible_100LL", new Vector3(10f, 0.7f, -9f), new Vector3(1.4f, 1.4f, 3f), M("Rojo_combustible", 0.7f, 0.15f, 0.12f), true);
            Cessna(h, new Vector3(0f, 0f, -3f), 180f);
        }
        Transform tower = Node(a, "Torre de control (FAA, 1961)", new Vector3(-30f, 0f, 22f));
        Prop(tower, "Fuste", new Vector3(0f, 6.5f, 0f), new Vector3(4f, 13f, 4f), M("Hormigon_torre", 0.82f, 0.8f, 0.76f), true);
        Prop(tower, "Cabina_acristalada", new Vector3(0f, 14.2f, 0f), new Vector3(6f, 2.6f, 6f), _glassDark, false);
        Prop(tower, "Techo", new Vector3(0f, 15.7f, 0f), new Vector3(6.6f, 0.4f, 6.6f), _dark, false);
        Prop(a, "Manga_de_viento", new Vector3(40f, 4f, 10f), new Vector3(0.1f, 8f, 0.1f), _metal, true);
        Prop(a, "Manga", new Vector3(40.9f, 7.8f, 10f), new Vector3(1.8f, 0.5f, 0.5f), M("Naranja_manga", 0.95f, 0.45f, 0.1f), false);
        Cessna(a, new Vector3(30f, 0f, 2f), 90f);
        Prop(a, "Valla_de_malla_del_aeropuerto", new Vector3(46f, 1.2f, -12.5f), new Vector3(0.05f, 2.4f, 65f), M("Malla_metalica", 0.6f, 0.62f, 0.62f), true);
    }

    static void Cessna(Transform p, Vector3 at, float yaw)
    {
        Transform c = Node(p, "Avioneta Cessna 172", at, yaw);
        Material w = M("Avioneta_blanca", 0.95f, 0.95f, 0.96f);
        Prop(c, "Fuselaje", new Vector3(0f, 1.2f, 0f), new Vector3(1.1f, 1.3f, 8.3f), w, true);
        Prop(c, "Ala_alta", new Vector3(0f, 2.3f, 0.6f), new Vector3(11f, 0.12f, 1.5f), w, false);
        Prop(c, "Estabilizador", new Vector3(0f, 1.6f, -3.8f), new Vector3(3.4f, 0.08f, 0.9f), w, false);
        Prop(c, "Deriva", new Vector3(0f, 2.3f, -3.9f), new Vector3(0.08f, 1.4f, 1.0f), M("Franja_azul", 0.15f, 0.25f, 0.6f), false);
        Prop(c, "Helice", new Vector3(0f, 1.2f, 4.2f), new Vector3(1.9f, 0.12f, 0.05f), _dark, false);
    }

    // ── 4 · LA I-105 ELEVADA Y LA ESTACIÓN CRENSHAW DE LA GREEN LINE ([C]: andén central en la mediana de la autopista, abierta en 1995) ──
    static void FreewayAndStation(Transform city)
    {
        Transform fw = Node(city, "Autopista I-105 (Glenn Anderson Freeway), elevada", DRaw(HawthorneCenterF.x, Spread(0f, 128f).y));
        Material concrete = M("Hormigon_viaducto", 0.72f, 0.71f, 0.68f), block = M("Bloque_muro_de_sonido", 0.66f, 0.64f, 0.6f);
        Prop(fw, "Tablero", new Vector3(0f, 9f, 0f), new Vector3(HawthorneGround.x, 1.2f, 26f), concrete, true);
        Prop(fw, "Calzada", new Vector3(0f, 9.62f, 0f), new Vector3(HawthorneGround.x, 0.04f, 26f), _asph, false);
        foreach (int sd in new[] { -1, 1 }) Prop(fw, "Muro_de_sonido", new Vector3(0f, 11.2f, sd * 13f), new Vector3(HawthorneGround.x, 3.2f, 0.3f), block, false);
        for (float x = -HawthorneGround.x * 0.5f + 10f; x <= HawthorneGround.x * 0.5f - 10f; x += 24f) foreach (int sd in new[] { -1, 1 }) Prop(fw, "Pila", new Vector3(x, 4.2f, sd * 8f), new Vector3(1.6f, 8.4f, 1.6f), concrete, true);
        // Las vías de la Green Line en la mediana y el andén central sobre Crenshaw (x local ≈ 25).
        foreach (int sd in new[] { -1, 1 }) Prop(fw, "Via_de_la_Green_Line", new Vector3(0f, 9.7f, sd * 2.6f), new Vector3(HawthorneGround.x, 0.15f, 1.5f), _dark, false);
        Transform st = Node(fw, "Estación Crenshaw (Metro Green Line)", new Vector3(Spread(70f, 0f).x - HawthorneCenterF.x, 0f, 0f));   // sobre la mediana de Crenshaw
        Prop(st, "Anden_central", new Vector3(0f, 10.4f, 0f), new Vector3(80f, 1.1f, 3.6f), _conc, true);
        Prop(st, "Marquesina_de_lamas", new Vector3(0f, 14f, 0f), new Vector3(70f, 0.3f, 5f), _metal, false);
        for (float x = -30f; x <= 30f; x += 10f) Prop(st, "Pilar_de_la_marquesina", new Vector3(x, 12.2f, 0f), new Vector3(0.3f, 3.4f, 0.3f), _metal, false);
        // El tren: dos coches de la Green Line (Siemens P2000) parados en la vía norte [S: modelo].
        for (int k = 0; k < 2; k++) Prop(st, "Coche_del_tren_P2000", new Vector3(-14f + k * 28f, 11.6f, 2.6f), new Vector3(27f, 3.6f, 2.7f), M("Tren_plata", 0.78f, 0.79f, 0.8f), false);
        // A ras de calle (oeste de Crenshaw, bajo el viaducto): la plaza con las máquinas de billetes, los validadores, el aparcabicis,
        // los azulejos «Crenshaw Stories» de Buzz Spector [C] y la escalera y el ascensor al andén.
        Transform plaza = Node(city, "Plaza de la estación Crenshaw", D(40f, 112f));
        Prop(plaza, "Pavimento_de_la_plaza", new Vector3(0f, 0.09f, 0f), new Vector3(16f, 0.02f, 10f), M("Losetas_plaza", 0.78f, 0.72f, 0.64f), false);
        for (int k = 0; k < 3; k++) Prop(plaza, "Maquina_de_billetes_(TVM)", new Vector3(-5f + k * 2f, 0.9f, 3.5f), new Vector3(1f, 1.8f, 0.6f), M("TVM_azul", 0.15f, 0.3f, 0.55f), true);
        for (int k = 0; k < 2; k++) Prop(plaza, "Validador", new Vector3(2f + k * 1.6f, 0.6f, 2.5f), new Vector3(0.3f, 1.2f, 0.3f), _dark, true);
        Prop(plaza, "Azulejos_Crenshaw_Stories_(Buzz_Spector)", new Vector3(0f, 1.2f, 4.9f), new Vector3(12f, 2.4f, 0.05f), M("Azulejos_ocre", 0.82f, 0.68f, 0.45f), false);
        Prop(plaza, "Aparcabicis", new Vector3(-6f, 0.45f, -3f), new Vector3(3f, 0.9f, 0.4f), _metal, true);
        Prop(plaza, "Hueco_del_ascensor", new Vector3(6f, 5.2f, 6.5f), new Vector3(2.6f, 10.4f, 2.6f), _glassDark, true);
        for (int k = 0; k < 26; k++) Prop(plaza, "Peldaño_al_anden", new Vector3(-6.5f, 0.2f + k * 0.4f, 6f + k * 0.3f), new Vector3(2.4f, 0.4f, 0.3f), _conc, true);
        Label(plaza, "Estación Crenshaw (Green Line): máquinas de billetes y acceso al andén", 3.5f);
    }

    // ── 5 · EL PARK-AND-RIDE DE LA ESTACIÓN ([C]: 506 plazas; aquí, un trozo) ──
    static void ParkAndRide(Transform city)
    {
        Transform lot = Node(city, "Park-and-ride de la estación Crenshaw", D(-10f, 97f));
        ParkingLot(lot, new Vector2(100f, 26f), 2, 2.6f, 22);
    }

    /// <summary>Un aparcamiento en superficie: asfalto, filas de plazas pintadas de blanco (2,6 × 5,5 m), topes de hormigón, la plaza
    /// azul de discapacidad y coches de 2008 aparcados en una de cada <paramref name="every"/>.</summary>
    static void ParkingLot(Transform lot, Vector2 size, int rows, float stall, int cars, int every = 2)
    {
        Prop(lot, "Asfalto_del_aparcamiento", new Vector3(0f, 0.025f, 0f), new Vector3(size.x, 0.05f, size.y), _asph, false);
        int perRow = Mathf.FloorToInt((size.x - 2f) / stall);
        int placed = 0;
        for (int r = 0; r < rows; r++)
        {
            float z = rows == 1 ? 0f : -size.y * 0.5f + 3.2f + r * (size.y - 6.4f);
            int dir = r % 2 == 0 ? 1 : -1;
            for (int i = 0; i <= perRow; i++)
            {
                float x = -size.x * 0.5f + 1f + i * stall;
                Prop(lot, "Raya_de_plaza", new Vector3(x, 0.055f, z), new Vector3(0.12f, 0.01f, 5.5f), _paint, false);
                if (i == perRow) continue;
                Prop(lot, "Tope_de_hormigon", new Vector3(x + stall * 0.5f, 0.08f, z - dir * 2.5f), new Vector3(1.8f, 0.15f, 0.2f), _conc, false);
                if (i == 0) Prop(lot, "Plaza_azul_de_discapacidad", new Vector3(x + stall * 0.5f, 0.056f, z), new Vector3(stall - 0.2f, 0.01f, 5.4f), M("Azul_discapacidad", 0.15f, 0.3f, 0.7f), false);
                else if (placed < cars && (i + r) % every == 0) { Car(lot, new Vector3(x + stall * 0.5f, 0f, z), dir > 0 ? 0f : 180f, placed); placed++; }
            }
        }
    }

    static readonly Color[] CarColors = { new Color(0.75f, 0.76f, 0.78f), new Color(0.1f, 0.1f, 0.12f), new Color(0.92f, 0.92f, 0.9f), new Color(0.5f, 0.1f, 0.1f), new Color(0.2f, 0.28f, 0.45f), new Color(0.42f, 0.45f, 0.4f) };
    /// <summary>Un coche de 2008 (sedán tipo Camry/Civic o todoterreno, [S]): carrocería, cabina acristalada y ruedas.</summary>
    static void Car(Transform p, Vector3 at, float yaw, int i)
    {
        Color c = CarColors[i % CarColors.Length]; bool suv = i % 4 == 3;
        Transform car = Node(p, suv ? "Todoterreno aparcado (2008)" : "Sedán aparcado (2008)", at, yaw);
        Material body = M("Coche_" + c.GetHashCode().ToString("X"), c.r, c.g, c.b);
        float len = suv ? 4.8f : 4.6f, hh = suv ? 1.1f : 0.75f;
        Prop(car, "Carroceria", new Vector3(0f, 0.3f + hh * 0.5f, 0f), new Vector3(1.8f, hh, len), body, true);
        Prop(car, "Cabina", new Vector3(0f, 0.3f + hh + 0.3f, suv ? -0.2f : -0.1f), new Vector3(1.6f, 0.6f, suv ? 3f : 2.4f), _glassDark, false);
        foreach (var (x, z) in new[] { (-0.85f, 1.4f), (0.85f, 1.4f), (-0.85f, -1.4f), (0.85f, -1.4f) }) Prop(car, "Rueda", new Vector3(x, 0.33f, z), new Vector3(0.24f, 0.66f, 0.66f), _dark, false);
    }

    // ── 6 · LA GASOLINERA DE LA ESQUINA (Crenshaw / 120th) [S] ──
    static AreaFunctionSite GasStation(Transform city)
    {
        Transform g = Node(city, "Gasolinera de Crenshaw y la 120", D(112f, 39f));
        Prop(g, "Explanada_de_hormigon", new Vector3(0f, 0.04f, 0f), new Vector3(40f, 0.08f, 26f), _conc, false);
        Material canopy = M("Marquesina_blanca_roja", 0.92f, 0.92f, 0.9f), stripe = M("Franja_roja_gasolinera", 0.75f, 0.12f, 0.1f);
        Prop(g, "Marquesina_de_surtidores", new Vector3(-6f, 5.6f, 0f), new Vector3(22f, 0.8f, 14f), canopy, false);
        Prop(g, "Franja_de_la_marquesina", new Vector3(-6f, 5.6f, -7.02f), new Vector3(22f, 0.5f, 0.04f), stripe, false);
        for (int i = 0; i < 2; i++) for (int j = 0; j < 2; j++)
        {
            Vector3 c = new Vector3(-12f + i * 12f, 0f, -3.5f + j * 7f);
            Prop(g, "Pilar_de_la_marquesina", c + new Vector3(0f, 2.8f, 0f), new Vector3(0.4f, 5.6f, 0.4f), canopy, true);
            Prop(g, "Isleta", c + new Vector3(0f, 0.12f, 0f), new Vector3(1.2f, 0.24f, 4.5f), _curb, true);
            Transform pump = Node(g, "Surtidor de gasolina (87 · 89 · 91)", c + new Vector3(0f, 0.24f, 1.3f));
            Prop(pump, "Cuerpo", new Vector3(0f, 0.9f, 0f), new Vector3(0.9f, 1.8f, 0.5f), M("Surtidor_blanco", 0.9f, 0.9f, 0.9f), true);
            Prop(pump, "Pantalla_de_precios", new Vector3(0f, 1.35f, -0.26f), new Vector3(0.6f, 0.35f, 0.02f), _dark, false);
            Prop(pump, "Manguera", new Vector3(0.5f, 0.9f, 0f), new Vector3(0.06f, 1.2f, 0.06f), _dark, false);
        }
        Prop(g, "Mastil_de_precios_(galon_2008)", new Vector3(-18f, 3.5f, -11f), new Vector3(0.3f, 7f, 0.3f), _metal, true);
        Prop(g, "Panel_de_precios", new Vector3(-18f, 6.2f, -11f), new Vector3(2.6f, 2f, 0.3f), M("Panel_precios", 0.95f, 0.95f, 0.92f), false);
        Prop(g, "Compresor_de_aire_y_agua", new Vector3(17f, 0.7f, -10f), new Vector3(0.7f, 1.4f, 0.5f), M("Compresor_azul", 0.15f, 0.3f, 0.6f), true);
        Prop(g, "Contenedor_de_basura", new Vector3(18f, 0.8f, 11f), new Vector3(2f, 1.6f, 1.4f), M("Contenedor_verde", 0.15f, 0.35f, 0.2f), true);
        // La tienda (minimercado) al fondo de la explanada, mirando a Crenshaw.
        var site = Shell(city, "Tienda de la gasolinera (minimercado)", "Mercado", new Vector2(124f, 39f), new Vector2(14f, 18f), 90f, 4.5f,
            new Color(0.92f, 0.9f, 0.86f), new Color(0.75f, 0.12f, 0.1f), true, "FOOD MART", new Color(0.75f, 0.12f, 0.1f), 1.8f, 0f);
        site.notHome = true;
        Transform b = site.transform;
        Prop(b, "Mostrador_con_caja", new Vector3(-4f, 0.55f, -3f), new Vector3(3f, 1.1f, 0.8f), M("Mostrador_laminado", 0.55f, 0.45f, 0.35f), true);
        Prop(b, "Expositor_de_tabaco_y_loteria", new Vector3(-4f, 1.5f, -0.8f), new Vector3(3f, 1.6f, 0.4f), M("Expositor_loteria", 0.3f, 0.5f, 0.3f), true);
        Prop(b, "Maquina_de_loteria_(California_Lottery)", new Vector3(-2.4f, 1.3f, -3f), new Vector3(0.5f, 0.4f, 0.4f), _dark, false);
        Prop(b, "Neveras_de_bebidas_(puertas_de_cristal)", new Vector3(0f, 1.1f, 6.1f), new Vector3(12f, 2.2f, 0.9f), _glass, true);
        Gondola(b, "Pasillo de aperitivos", new Vector3(2f, 0f, 2f), 6f, 0f, new[] { new Color(0.9f, 0.6f, 0.1f), new Color(0.8f, 0.15f, 0.1f), new Color(0.2f, 0.4f, 0.8f) });
        Prop(b, "Maquina_de_granizado_y_cafe", new Vector3(5.5f, 1.2f, -2f), new Vector3(1.2f, 0.8f, 0.6f), M("Granizadora", 0.3f, 0.6f, 0.85f), false);
        DPiece(b, "puesto", new Vector3(-4f, 0f, -5.4f), 0f, "Caja de la gasolinera", new Vector2(1.6f, 0.8f));
        return site;
    }

    // ── 7 · EL STRIP MALL DE CRENSHAW: locales contiguos de una planta con el aparcamiento delante (la tipología de LA) [S] ──
    static List<AreaFunctionSite> StripMall(Transform city)
    {
        var l = new List<AreaFunctionSite>();
        Transform lot = Node(city, "Aparcamiento del strip mall", D(112f, 2f));
        ParkingLot(lot, new Vector2(40f, 42f), 1, 2.6f, 6, 3);
        Prop(lot, "Mastil_del_rotulo_del_centro", new Vector3(-19f, 4f, 18f), new Vector3(0.5f, 8f, 0.5f), _metal, true);
        Prop(lot, "Rotulo_Crenshaw_Plaza", new Vector3(-19f, 7.2f, 18f), new Vector3(0.6f, 3f, 4f), M("Rotulo_plaza", 0.85f, 0.75f, 0.4f), false);
        Color stucco = new Color(0.94f, 0.88f, 0.76f), brown = new Color(0.45f, 0.3f, 0.2f);
        // Las fachadas miran al oeste (a Crenshaw y al aparcamiento): en el plano, x 136…168; de norte a sur.
        l.Add(Donuts(Shell(city, "Donuts de Crenshaw", "Restaurante", new Vector2(152f, 18.5f), new Vector2(32f, 11f), 90f, 4.6f, stucco, brown, true, "DONUTS · COFFEE", new Color(0.85f, 0.35f, 0.55f), 1.8f, 0f, true, new Color(0.85f, 0.35f, 0.55f), sideWindows: false)));
        l.Add(Taqueria(Shell(city, "Taquería de Crenshaw", "Restaurante", new Vector2(152f, 5.5f), new Vector2(32f, 15f), 90f, 4.6f, stucco, brown, true, "TACOS · BURRITOS · TORTAS", new Color(0.1f, 0.5f, 0.25f), 1.8f, 0f, true, new Color(0.1f, 0.5f, 0.25f), sideWindows: false)));
        l.Add(Laundry(Shell(city, "Lavandería de Crenshaw (coin laundry)", "Taller", new Vector2(152f, -7.5f), new Vector2(32f, 11f), 90f, 4.6f, stucco, brown, true, "COIN LAUNDRY", new Color(0.15f, 0.35f, 0.65f), 1.8f, 0f, sideWindows: false)));
        l.Add(Liquor(Shell(city, "Licorería y tienda de la esquina", "Mercado", new Vector2(152f, -17f), new Vector2(32f, 8f), 90f, 4.6f, stucco, brown, true, "LIQUOR · MARKET", new Color(0.7f, 0.1f, 0.1f), 1.8f, 0f, sideWindows: false)));
        return l;
    }

    /// <summary>LA TAQUERÍA (Berón: «por fuera era un cubo sin ventanas… sillas humanas junto a sillas humano-insecto… escasas mesas»):
    /// escaparate con rejas, menú iluminado, plancha y comal, trompo al pastor, vaporera de carnes, barra de salsas, nevera vertical de
    /// refrescos y DIEZ mesas altas de insecto-humano. La plancha se llama «Plancha de los tacos» (la misión de CodeShip la busca).</summary>
    static AreaFunctionSite Taqueria(AreaFunctionSite site)
    {
        Transform b = site.transform; // 15 m de fachada × 32 m de fondo (locales: x ±7,5, z ±16)
        Material steel = _metal, tile = M("Azulejo_talavera", 0.85f, 0.8f, 0.6f);
        // Rejas de seguridad del escaparate (las tenían casi todos los locales de Crenshaw [S]).
        for (float x = -7f; x <= 7f; x += 0.5f) { if (Mathf.Abs(x) < 1.2f) continue; Prop(b, "Muro_reja", new Vector3(x, 1.8f, -16.2f), new Vector3(0.03f, 2.6f, 0.03f), _dark, false); }
        // Cocina abierta al fondo, tras el mostrador.
        Prop(b, "Mostrador_de_pedidos", new Vector3(0f, 0.55f, 4f), new Vector3(13f, 1.1f, 0.8f), tile, true);
        Prop(b, "Menu_iluminado", new Vector3(0f, 3.2f, 15.6f), new Vector3(10f, 1.4f, 0.1f), M("Menu_luminoso", 0.95f, 0.9f, 0.7f), false);
        Prop(b, "Caja_registradora", new Vector3(-5f, 1.25f, 4f), new Vector3(0.45f, 0.3f, 0.4f), _dark, false);
        DPiece(b, "fogon", new Vector3(-3f, 0f, 9f), 0f, "Plancha de los tacos");
        Prop(b, "Comal_de_tortillas", new Vector3(0.5f, 0.95f, 9f), new Vector3(1.4f, 0.1f, 0.9f), _dark, true);
        Prop(b, "Base_del_comal", new Vector3(0.5f, 0.45f, 9f), new Vector3(1.4f, 0.9f, 0.9f), steel, true);
        Transform trompo = Node(b, "Trompo al pastor", new Vector3(3.5f, 0f, 9f));
        Prop(trompo, "Base", new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.9f), steel, true);
        Prop(trompo, "Carne_al_pastor_en_el_trompo", new Vector3(0f, 1.35f, 0f), new Vector3(0.45f, 0.9f, 0.45f), M("Adobo_pastor", 0.72f, 0.3f, 0.15f), false);
        Prop(trompo, "Piña", new Vector3(0f, 1.9f, 0f), new Vector3(0.22f, 0.2f, 0.22f), M("Piña", 0.9f, 0.75f, 0.2f), false);
        Prop(trompo, "Quemador_vertical", new Vector3(0f, 1.35f, 0.4f), new Vector3(0.6f, 1f, 0.1f), M("Quemador_rojo", 0.8f, 0.3f, 0.1f), false);
        Prop(b, "Vaporera_de_carnes_(asada,_carnitas,_lengua,_cabeza)", new Vector3(-6f, 0.5f, 9f), new Vector3(1.2f, 1f, 0.9f), steel, true);
        Prop(b, "Mesa_de_preparacion_inox", new Vector3(0f, 0.45f, 13.8f), new Vector3(8f, 0.9f, 0.8f), steel, true);
        Prop(b, "Campana_extractora", new Vector3(0f, 3.2f, 9f), new Vector3(12f, 0.7f, 1.6f), steel, false);
        Prop(b, "Fregadero_de_tres_senos", new Vector3(6f, 0.45f, 13.8f), new Vector3(1.8f, 0.9f, 0.7f), steel, true);
        Prop(b, "Camara_frigorifica_de_la_taqueria", new Vector3(-5.5f, 1.2f, 14f), new Vector3(2.5f, 2.4f, 2.6f), steel, true);
        Prop(b, "Tarjeta_de_inspeccion_sanitaria_A", new Vector3(5f, 2.2f, -16.12f), new Vector3(0.3f, 0.4f, 0.02f), M("Tarjeta_A_azul", 0.2f, 0.35f, 0.75f), false);
        // Comedor: barra de salsas, nevera de refrescos, servilleteros… y 10 mesas altas de insecto-humano (5 × 2 filas).
        Transform salsas = Node(b, "Barra de salsas (roja, verde, pico de gallo, rábanos, limones)", new Vector3(5.5f, 0f, 1f));
        Prop(salsas, "Mueble", new Vector3(0f, 0.5f, 0f), new Vector3(1f, 1f, 2.4f), tile, true);
        Color[] sal = { new Color(0.75f, 0.12f, 0.08f), new Color(0.3f, 0.55f, 0.2f), new Color(0.85f, 0.45f, 0.3f), new Color(0.95f, 0.85f, 0.85f), new Color(0.55f, 0.75f, 0.2f) };
        for (int k = 0; k < sal.Length; k++) Prop(salsas, "Cubeta", new Vector3(0f, 1.05f, -0.9f + k * 0.45f), new Vector3(0.35f, 0.1f, 0.35f), M("Salsa_" + k, sal[k].r, sal[k].g, sal[k].b), false);
        Prop(b, "Nevera_vertical_de_refrescos_(Jarritos,_Coca-Cola)", new Vector3(-6.8f, 1.05f, 1f), new Vector3(0.8f, 2.1f, 1.6f), _glass, true);
        // 6 mesas (3 × 2): con sus apoyos de abdomen cada una ocupa ~2,8 × 4,3 m; 2,4 m de paso entre ellas y la puerta despejada.
        for (int r = 0; r < 2; r++) for (int k = 0; k < 3; k++)
            InsectTable(b, new Vector3(-5.25f + k * 5.25f, 0f, -11f + r * 6.8f), "Mesa alta de la taquería", new Vector2(1.4f, 0.8f));
        Prop(b, "Television_de_tubo_(futbol)", new Vector3(-7.1f, 2.6f, -8f), new Vector3(0.6f, 0.6f, 0.8f), _dark, false);
        DPiece(b, "inodoro", new Vector3(5.5f, 0f, 14f), 0f, "Aseo de la taquería", null, station: false);
        DPiece(b, "lavabo", new Vector3(3.5f, 0f, 14.6f), 0f, "Lavamanos de la taquería", null, station: false);
        return site;
    }

    static AreaFunctionSite Donuts(AreaFunctionSite site)
    {
        Transform b = site.transform;
        Prop(b, "Vitrina_de_donuts", new Vector3(0f, 0.6f, 3f), new Vector3(8f, 1.2f, 0.9f), _glass, true);
        for (int k = 0; k < 6; k++) Prop(b, "Bandeja_de_donuts", new Vector3(-3.4f + k * 1.35f, 0.9f, 3f), new Vector3(1.1f, 0.08f, 0.6f), M("Donut_" + (k % 3), 0.75f + 0.05f * (k % 3), 0.5f, 0.35f + 0.15f * (k % 2)), false);
        Prop(b, "Freidora_de_donuts", new Vector3(-2f, 0.5f, 10f), new Vector3(2.4f, 1f, 1.2f), _metal, true);
        Prop(b, "Amasadora_planetaria", new Vector3(2f, 0.7f, 10f), new Vector3(0.8f, 1.4f, 0.8f), _metal, true);
        Prop(b, "Carro_de_bandejas", new Vector3(4f, 0.9f, 12f), new Vector3(0.7f, 1.8f, 0.9f), _metal, true);
        Prop(b, "Cafeteras_de_filtro", new Vector3(-4f, 1.25f, 6f), new Vector3(1.6f, 0.4f, 0.4f), _dark, false);
        DPiece(b, "horno", new Vector3(3.5f, 0f, 6.5f), 0f, "Horno de la panadería de donuts");
        for (int k = 0; k < 4; k++) InsectTable(b, new Vector3(-2.5f + (k % 2) * 5f, 0f, -11f + (k / 2) * 6.8f), "Mesa alta de fórmica (donuts)", new Vector2(1.2f, 0.8f));
        return site;
    }

    static AreaFunctionSite Laundry(AreaFunctionSite site)
    {
        Transform b = site.transform;
        Material white = M("Lavadora_blanca", 0.93f, 0.93f, 0.92f), steel = _metal;
        for (int k = 0; k < 12; k++) Prop(b, "Lavadora_de_monedas_(carga_superior)", new Vector3(-4.6f, 0.5f, -12f + k * 2.2f), new Vector3(0.75f, 1f, 0.75f), white, true);
        for (int k = 0; k < 8; k++) Prop(b, "Secadora_de_monedas_(apilada_doble)", new Vector3(4.8f, 1.0f, -10f + k * 3f), new Vector3(0.9f, 2f, 0.9f), steel, true);
        Prop(b, "Mesa_de_doblar", new Vector3(0f, 0.45f, 0f), new Vector3(1.2f, 0.9f, 6f), M("Mesa_formica_blanca", 0.9f, 0.88f, 0.85f), true);
        Prop(b, "Maquina_de_cambio_de_monedas", new Vector3(-5f, 0.9f, 14f), new Vector3(0.6f, 1.8f, 0.5f), _dark, true);
        Prop(b, "Expendedora_de_detergente", new Vector3(-3.6f, 0.9f, 14f), new Vector3(0.7f, 1.8f, 0.5f), M("Expendedora_naranja", 0.9f, 0.5f, 0.1f), true);
        for (int k = 0; k < 4; k++) Prop(b, "Carro_de_ropa", new Vector3(1.8f, 0.5f, -12f + k * 3.5f), new Vector3(0.6f, 1f, 0.9f), steel, true);
        DPiece(b, "lavadero", new Vector3(0f, 0f, 12f), 0f, "Pila de lavar a mano");
        DPiece(b, "banco", new Vector3(0f, 0f, -14.5f), 0f, "Banco de apoyo (esperar la colada)", null, MicroBody.OfSpecies("Ant"), null, station: false);
        return site;
    }

    static AreaFunctionSite Liquor(AreaFunctionSite site)
    {
        Transform b = site.transform;
        Prop(b, "Mostrador_blindado", new Vector3(0f, 0.6f, -1f), new Vector3(6f, 1.2f, 0.8f), M("Mostrador_laminado", 0.55f, 0.45f, 0.35f), true);
        Prop(b, "Mampara_antibalas", new Vector3(0f, 1.9f, -1f), new Vector3(6f, 1.4f, 0.05f), _glass, false);
        Prop(b, "Estanteria_de_licores", new Vector3(0f, 1.1f, 1.2f), new Vector3(6.5f, 2.2f, 0.5f), M("Botellas_ambar", 0.55f, 0.35f, 0.15f), true);
        Prop(b, "Neveras_de_cerveza_y_leche", new Vector3(3.2f, 1.1f, 8f), new Vector3(0.9f, 2.2f, 14f), _glass, true);
        Gondola(b, "Pasillo de conservas y pan de molde", new Vector3(-1f, 0f, 8f), 7f, 90f, new[] { new Color(0.8f, 0.2f, 0.15f), new Color(0.9f, 0.8f, 0.4f), new Color(0.3f, 0.5f, 0.3f) });
        DPiece(b, "puesto", new Vector3(0f, 0f, -3.2f), 0f, "Mostrador", new Vector2(1.6f, 0.8f));
        DPiece(b, "estante", new Vector3(-2.2f, 0f, 13f), 0f, "Estantes", new Vector2(0.8f, 1.6f));
        return site;
    }

    // ── 8 · EL SUPERMERCADO ([S]: no se ha encontrado cuál había; uno de cadena de 2008, ~2 500 m² reales, aquí 48 × 50 m) ──
    static AreaFunctionSite Supermarket(Transform city)
    {
        Transform lot = Node(city, "Aparcamiento del supermercado", D(105f, -51f));
        ParkingLot(lot, new Vector2(26f, 50f), 1, 2.6f, 8, 2);
        Prop(lot, "Corral_de_carros", new Vector3(8f, 0.5f, 0f), new Vector3(2f, 1f, 6f), _metal, false);
        var site = Shell(city, "Supermercado del barrio", "Mercado", new Vector2(144f, -51f), new Vector2(48f, 50f), 90f, 7.5f,
            new Color(0.9f, 0.88f, 0.82f), new Color(0.2f, 0.45f, 0.3f), true, "NEIGHBORHOOD MARKET · SUPERMERCADO", new Color(0.15f, 0.45f, 0.25f), 3.6f, -14f, awning: false);
        site.notHome = true;
        Transform b = site.transform; // fachada de 50 m (x ±25), fondo 48 (z ±24)
        // Cajas con cinta (4) junto a la entrada.
        for (int k = 0; k < 4; k++)
        {
            float x = -18f + k * 4.2f;
            string name = k < 2 ? $"Caja registradora {k + 1}" : $"Caja {k + 1} (con cinta)";
            if (k < 2) DPiece(b, "puesto", new Vector3(x, 0f, -16f), 0f, name, new Vector2(1.6f, 0.8f));
            Prop(b, "Cinta_transportadora", new Vector3(x + 1.2f, 0.45f, -16f), new Vector3(0.7f, 0.9f, 3.2f), _dark, true);
            Prop(b, "Numero_de_caja", new Vector3(x, 2.4f, -16f), new Vector3(0.4f, 0.4f, 0.05f), M("Luz_caja", 0.95f, 0.85f, 0.3f), false);
        }
        Prop(b, "Fila_de_carros", new Vector3(-6f, 0.5f, -21f), new Vector3(6f, 1f, 1f), _metal, true);
        // Seis pasillos de góndolas (los nombres que buscan las misiones van en los tres primeros).
        string[] aisles = { "Pasillo de conservas", "Pasillo de cereales y pan", "Productos de limpieza", "Pasillo de pasta, arroz y frijoles", "Pasillo de bebidas", "Pasillo de cuidado personal" };
        Color[][] goods = {
            new[] { new Color(0.8f, 0.2f, 0.15f), new Color(0.9f, 0.75f, 0.3f), new Color(0.3f, 0.5f, 0.3f) },
            new[] { new Color(0.9f, 0.7f, 0.3f), new Color(0.85f, 0.4f, 0.2f), new Color(0.95f, 0.9f, 0.75f) },
            new[] { new Color(0.2f, 0.5f, 0.85f), new Color(0.95f, 0.95f, 0.95f), new Color(0.9f, 0.85f, 0.2f) },
            new[] { new Color(0.95f, 0.88f, 0.7f), new Color(0.45f, 0.25f, 0.2f), new Color(0.85f, 0.6f, 0.3f) },
            new[] { new Color(0.8f, 0.1f, 0.1f), new Color(0.2f, 0.6f, 0.3f), new Color(0.3f, 0.3f, 0.8f) },
            new[] { new Color(0.6f, 0.8f, 0.9f), new Color(0.9f, 0.6f, 0.7f), new Color(0.95f, 0.95f, 0.9f) } };
        for (int k = 0; k < 6; k++)
        {
            if (k < 3) DPiece(b, "estante", new Vector3(-16f + k * 5.4f, 0f, -8.5f), 0f, aisles[k], new Vector2(0.8f, 1.6f));
            Gondola(b, aisles[k] + " (góndola)", new Vector3(-16f + k * 5.4f, 0f, 3f), 18f, 90f, goods[k]);
        }
        // Frutas y verduras (mesas inclinadas con nebulizadores), carnicería y panadería al fondo, murales de lácteos y congelados al este.
        DPiece(b, "puesto", new Vector3(18f, 0f, -12f), 0f, "Frutas y verduras", new Vector2(2.4f, 1f));
        Transform produce = Node(b, "Sección de frutas y verduras", new Vector3(18f, 0f, -4f));
        Color[] fv = { new Color(0.9f, 0.2f, 0.15f), new Color(0.95f, 0.6f, 0.1f), new Color(0.35f, 0.65f, 0.2f), new Color(0.95f, 0.85f, 0.2f), new Color(0.5f, 0.25f, 0.4f), new Color(0.2f, 0.5f, 0.15f) };
        for (int k = 0; k < 6; k++)
        {
            Prop(produce, "Mesa_inclinada", new Vector3((k % 2) * 4f - 2f, 0.45f, (k / 2) * 3.6f), new Vector3(2.4f, 0.9f, 1.6f), M("Madera_expositor", 0.6f, 0.45f, 0.3f), true);
            Prop(produce, "Fruta_o_verdura", new Vector3((k % 2) * 4f - 2f, 1.0f, (k / 2) * 3.6f), new Vector3(2.2f, 0.2f, 1.4f), M("Fruta_" + k, fv[k].r, fv[k].g, fv[k].b), false);
        }
        Prop(b, "Mural_refrigerado_de_verduras_(con_nebulizadores)", new Vector3(24.2f, 1.1f, -6f), new Vector3(0.9f, 2.2f, 12f), _glass, true);
        DPiece(b, "armario", new Vector3(23.6f, 0f, 10f), 0f, "Cámara de lácteos y congelados", new Vector2(1.6f, 0.8f));
        Prop(b, "Murales_de_lacteos_y_congelados", new Vector3(24.2f, 1.1f, 14f), new Vector3(0.9f, 2.2f, 14f), _glass, true);
        Prop(b, "Vitrina_de_la_carniceria", new Vector3(4f, 0.6f, 21f), new Vector3(12f, 1.2f, 1f), _glass, true);
        Prop(b, "Sierra_de_hueso_y_picadora", new Vector3(4f, 0.8f, 23f), new Vector3(2f, 1.6f, 0.9f), _metal, true);
        Prop(b, "Horno_de_pan_de_la_panaderia", new Vector3(-12f, 1.1f, 22.8f), new Vector3(3f, 2.2f, 1.6f), _metal, true);
        Prop(b, "Estanteria_de_pan_recien_hecho", new Vector3(-12f, 0.9f, 18f), new Vector3(5f, 1.8f, 0.6f), M("Madera_expositor", 0.6f, 0.45f, 0.3f), true);
        // El almacén y el muelle trasero (al este, a Wilkie Ave) con el camión del distribuidor.
        CivicBuilder.Partition(b, "almacen", new Vector2(-24.7f, 18.5f), new Vector2(-14f, 18.5f), 4f * ShellK, new Color(0.85f, 0.85f, 0.82f), 5f, 1.8f * ShellK, "supuesto ×3: puerta batiente del almacén");
        Prop(b, "Estanteria_de_palés_del_almacen", new Vector3(-20f, 2f, 22.5f), new Vector3(7f, 4f, 1.2f), M("Rack_naranja", 0.9f, 0.45f, 0.1f), true);
        Prop(b, "Transpaleta", new Vector3(-17f, 0.4f, 20.5f), new Vector3(0.7f, 0.8f, 1.6f), M("Transpaleta_roja", 0.75f, 0.15f, 0.1f), true);
        Transform truck = Node(b, "Camión del distribuidor (reparto al supermercado)", new Vector3(-10f, 0f, 30f), 90f);
        Prop(truck, "Remolque", new Vector3(0f, 2f, -2f), new Vector3(2.5f, 2.8f, 9f), M("Remolque_blanco", 0.92f, 0.92f, 0.9f), true);
        Prop(truck, "Tractora", new Vector3(0f, 1.6f, 4f), new Vector3(2.4f, 2.4f, 2.6f), M("Tractora_azul", 0.2f, 0.3f, 0.55f), true);
        Prop(truck, "Plataforma_elevadora", new Vector3(0f, 0.6f, -6.8f), new Vector3(2.4f, 0.1f, 1.4f), _metal, false);
        for (int k = 0; k < 3; k++) Prop(truck, "Palé_de_cajas", new Vector3(-0.4f + k * 0.4f, 0.6f, -9f - k * 1.3f), new Vector3(1.1f, 1.2f, 1.1f), M("Cajas_carton", 0.7f, 0.55f, 0.35f), true);
        return site;
    }

    // ── 9 · EL TALLER DE COCHES (Crenshaw) [S] ──
    static AreaFunctionSite AutoShop(Transform city)
    {
        var site = Shell(city, "Taller de coches de Crenshaw", "Taller", new Vector2(115f, -93f), new Vector2(30f, 22f), 90f, 6f,
            new Color(0.82f, 0.82f, 0.8f), new Color(0.15f, 0.3f, 0.6f), false, "AUTO REPAIR · SMOG CHECK · BRAKES", new Color(0.15f, 0.3f, 0.6f), 3.6f, 5f);
        site.notHome = true;
        Transform b = site.transform; // fachada 22 (x ±11), fondo 30 (z ±15)
        for (int k = 0; k < 2; k++)
        {
            Transform lift = Node(b, "Elevador de dos columnas", new Vector3(-6f + k * 8f, 0f, 2f));
            foreach (int sd in new[] { -1, 1 }) Prop(lift, "Columna", new Vector3(sd * 1.7f, 1.9f, 0f), new Vector3(0.35f, 3.8f, 0.35f), M("Elevador_azul", 0.15f, 0.3f, 0.6f), true);
            Car(lift, new Vector3(0f, 1.4f, 0f), 0f, k + 1);
        }
        DPiece(b, "banco_taller", new Vector3(-8f, 0f, 12.5f), 0f, "Banco del mecánico", new Vector2(2f, 0.9f));
        DPiece(b, "armario", new Vector3(-3f, 0f, 13f), 0f, "Recambios", new Vector2(1.2f, 0.6f));
        Prop(b, "Carro_de_herramientas_rojo", new Vector3(1f, 0.55f, 11f), new Vector3(1f, 1.1f, 0.5f), M("Carro_rojo", 0.75f, 0.12f, 0.1f), true);
        Prop(b, "Compresor_de_aire", new Vector3(9.5f, 0.8f, 13.5f), new Vector3(1f, 1.6f, 0.8f), M("Compresor_azul", 0.15f, 0.3f, 0.6f), true);
        Prop(b, "Desmontadora_de_neumaticos", new Vector3(6f, 0.5f, 12f), new Vector3(0.9f, 1f, 0.9f), _metal, true);
        Prop(b, "Equilibradora_de_ruedas", new Vector3(8.5f, 0.6f, 9.5f), new Vector3(0.8f, 1.2f, 0.9f), _metal, true);
        Prop(b, "Analizador_de_gases_(smog_check)", new Vector3(-9.5f, 0.8f, 6f), new Vector3(0.7f, 1.6f, 0.6f), M("Analizador_gris", 0.7f, 0.7f, 0.68f), true);
        Prop(b, "Bidones_de_aceite_usado", new Vector3(-9.5f, 0.5f, -2f), new Vector3(0.8f, 1f, 1.8f), M("Bidon_negro", 0.12f, 0.12f, 0.12f), true);
        Prop(b, "Mancha_de_aceite", new Vector3(-6f, 0.105f, 2f), new Vector3(1.6f, 0.01f, 1.4f), M("Mancha_aceite", 0.2f, 0.2f, 0.2f), false);
        Prop(b, "Oficina_del_taller_(mostrador)", new Vector3(-8f, 0.55f, -11f), new Vector3(3f, 1.1f, 0.8f), M("Mostrador_laminado", 0.55f, 0.45f, 0.35f), true);
        return site;
    }

    // ── 10 · LOS APARTAMENTOS DE LOS INGENIEROS JÓVENES (dos plantas, galería exterior; la tipología «dingbat» de LA [S]) ──
    static AreaFunctionSite Apartments(Transform city)
    {
        var site = Shell(city, "Apartamentos de ingenieros", "Casa", new Vector2(151f, -110f), new Vector2(38f, 20f), 270f, 6.4f,
            new Color(0.93f, 0.86f, 0.72f), new Color(0.55f, 0.4f, 0.3f), false, "CRENSHAW GARDEN APARTMENTS", new Color(0.4f, 0.3f, 0.25f), 1.2f, -6f, backDoor: false);   // (la puerta estaba en x −12, fuera de una fachada de ±10: el edificio quedaba cerrado)
        Transform b = site.transform; // fachada 20 (x ±10), fondo 38 (z ±19), al este (Wilkie)
        // Tres apartamentos en planta baja, separados por tabiques con su puerta, cada uno con cama, cocina, mesa alta, retrete y lavabo.
        for (int u = 0; u < 3; u++)
        {
            float z0 = -19f + u * 12.6f;
            if (u > 0) CivicBuilder.Partition(b, "apartamento_" + u, new Vector2(-9.85f, z0), new Vector2(9.85f, z0), 3.1f * ShellK, new Color(0.9f, 0.86f, 0.78f), 3f, 0.9f * ShellK, "supuesto ×3: puerta de apartamento de EE. UU. (36 in)");
            DPiece(b, "cama", new Vector3(6f, 0f, z0 + 4f), 0f, $"Cama del apartamento {u + 1}", null, station: false);
            DPiece(b, "cocina", new Vector3(-7.5f, 0f, z0 + 9f), 0f, $"Cocina del apartamento {u + 1}");
            InsectTable(b, new Vector3(-2f, 0f, z0 + 6f), $"Mesa alta del apartamento {u + 1}", new Vector2(1.2f, 0.8f));
            DPiece(b, "inodoro", new Vector3(8.4f, 0f, z0 + 10.5f), 0f, $"Retrete del apartamento {u + 1}", null, station: false);
            DPiece(b, "lavabo", new Vector3(8.4f, 0f, z0 + 8f), 0f, $"Lavabo del apartamento {u + 1}", null, station: false);
            Prop(b, "Escritorio_con_portatil", new Vector3(-7.5f, 0.45f, z0 + 3f), new Vector3(1.4f, 0.9f, 0.7f), M("Escritorio_madera", 0.55f, 0.42f, 0.3f), true);
        }
        // La planta de arriba (sus apartamentos se ven desde la galería) y la escalera exterior de hormigón.
        Prop(b, "Forjado_planta_alta", new Vector3(0f, 3.2f * ShellK, 0f), new Vector3(20f, 0.2f, 38f), M("Forjado_hormigon", 0.7f, 0.7f, 0.68f), true);
        Prop(b, "Galeria_exterior", new Vector3(1.5f, 3.2f * ShellK, -21f), new Vector3(23f, 0.2f, 2.6f), M("Forjado_hormigon", 0.7f, 0.7f, 0.68f), true);
        for (int k = 0; k < 24; k++) Prop(b, "Peldaño_exterior", new Vector3(11.6f, 0.2f + k * 0.4f, -13f - k * 0.32f), new Vector3(2.4f, 0.4f, 0.32f), _conc, true);
        for (int k = 0; k < 4; k++) Prop(b, "Ventana_planta_alta", new Vector3(-7.5f + k * 5f, 4.8f * ShellK, -19.42f), new Vector3(3.6f, 3f, 0.04f), _glassDark, false);
        // El patio con césped y el limonero (regla 11: las residencias tienen su frutal).
        Prop(city, "Patio_de_los_apartamentos", D(151f, -126.5f, 0.03f), new Vector3(38f, 0.04f, 11f), _grass, false);
        CivicBuilder.FruitTree(Node(city, "Patio de los apartamentos (frutal)", D(160f, -126f)), 7);
        return site;
    }

    // ── 11 · CASAS DE ESTUCO (Hollypark, al norte de la 120 y al este de Wilkie): una planta, años 40–50, garaje, césped y valla [S] ──
    static List<AreaFunctionSite> Houses(Transform city)
    {
        var l = new List<AreaFunctionSite>();
        Color[] walls = { new Color(0.95f, 0.9f, 0.8f), new Color(0.92f, 0.82f, 0.7f), new Color(0.88f, 0.9f, 0.86f), new Color(0.96f, 0.86f, 0.78f) };
        int i = 0;
        // (ronda del 2026-11-19) Casas de 16 × 13 m (antes 13 × 11: con los muebles ×2 no quedaba paso a la cocina) cada 18 m.
        // Fila norte (frente a la 120, mirando al sur), de x 100 a 226.
        for (float x = 100f; x <= 226f; x += 18f) l.Add(House(city, new Vector2(x, 97f), 0f, walls[i % walls.Length], i++));
        // Fila de Wilkie (al este, mirando al oeste), de z −122 a −68.
        for (float z = -122f; z <= -68f; z += 18f) l.Add(House(city, new Vector2(214f, z), 90f, walls[i % walls.Length], i++));
        return l;
    }

    static AreaFunctionSite House(Transform city, Vector2 at, float face, Color wall, int i)
    {
        var site = Shell(city, $"Casa de Hawthorne {i + 1}", "Casa", at, face == 0f ? new Vector2(16f, 13f) : new Vector2(13f, 16f), face, 3.2f,
            wall, new Color(0.45f, 0.35f, 0.28f), false, $"{11900 + i * 12}", new Color(0.3f, 0.25f, 0.2f), 0.9f, 0f);
        Transform b = site.transform; // fachada 16 (x ±8), fondo 13 (z ±6,5); la puerta en el centro
        // Al oeste, el salón con la cocina al fondo y la mesa; al este, tras el tabique (puerta junto a la fachada), el dormitorio y el baño.
        CivicBuilder.Partition(b, "dormitorio", new Vector2(2.5f, -6.35f), new Vector2(2.5f, 6.35f), 2.9f * ShellK, new Color(0.94f, 0.92f, 0.88f), 3.2f, 0.8f * ShellK, "supuesto ×3: puerta interior de 32 in");
        DPiece(b, "cocina", new Vector3(-4.5f, 0f, 4.6f), 0f, "Cocina de la casa");
        InsectTable(b, new Vector3(-4.8f, 0f, -1.2f), "Mesa alta de la casa", new Vector2(1.2f, 0.8f));
        DPiece(b, "cama", new Vector3(5.4f, 0f, 4.2f), 0f, "Cama", null, station: false);
        DPiece(b, "lavabo", new Vector3(6.6f, 0f, -0.6f), 0f, "Lavabo de la casa", null, station: false);
        DPiece(b, "inodoro", new Vector3(6.6f, 0f, -5.2f), 0f, "Retrete de la casa", null, station: false);
        // Tejado a cuatro aguas de teja de asfalto (encima de la azotea de Shell) y el jardín delantero con su valla.
        Prop(b, "Tejado_de_teja_asfaltica", new Vector3(0f, 3.2f * ShellK + 0.7f, 0f), new Vector3(17f, 0.9f, 14f), M("Teja_asfaltica", 0.35f, 0.3f, 0.28f), false);
        Prop(b, "Cesped_delantero", new Vector3(0f, 0.02f, -9.25f), new Vector3(16f, 0.04f, 5.5f), _grass, false);
        Prop(b, "Valla_de_hierro", new Vector3(5f, 0.5f, -11.95f), new Vector3(6f, 1f, 0.05f), _dark, true);   // a la izquierda, la entrada del coche
        Prop(b, "Buzon", new Vector3(2.4f, 0.6f, -11.8f), new Vector3(0.25f, 1.2f, 0.25f), _metal, true);
        Prop(b, "Entrada_del_coche", new Vector3(-5.5f, 0.03f, -9.25f), new Vector3(3.6f, 0.04f, 5.5f), _conc, false);
        Car(b, new Vector3(-5.5f, 0f, -9.2f), 0f, i);
        CivicBuilder.FruitTree(Node(b, "Frutal del jardín", new Vector3(5.5f, 0f, -9f)), i);   // regla 11: cada residencia con su frutal
        return site;
    }

    // ── 12 · LAS NAVES «TILT-UP» (Century Business Center y Wilkie Ave [C: tipo, 7,3 m libres]) ──
    static List<AreaFunctionSite> Warehouses(Transform city)
    {
        var l = new List<AreaFunctionSite>();
        Color tilt = new Color(0.86f, 0.82f, 0.74f), band = new Color(0.45f, 0.5f, 0.55f);
        var a = Shell(city, "Proveedor aeroespacial (nave de Wilkie Ave)", "Taller", new Vector2(216f, 24f), new Vector2(44f, 52f), 90f, 8.5f,
            tilt, band, false, "PRECISION MACHINING · AEROSPACE", new Color(0.2f, 0.3f, 0.45f), 1.8f, -18f);
        Transform b = a.transform; a.notHome = true; // fachada 52 (x ±26), fondo 44 (z ±22)
        for (int k = 0; k < 4; k++) DPiece(b, "torno", new Vector3(-16f + k * 6f, 0f, 4f), 0f, $"Fresadora CNC del proveedor {k + 1}", new Vector2(1.6f, 1f));
        for (int k = 0; k < 3; k++) Prop(b, "Estanteria_de_palés", new Vector3(-12f + k * 12f, 2.5f, 18f), new Vector3(9f, 5f, 1.2f), M("Rack_naranja", 0.9f, 0.45f, 0.1f), true);
        Prop(b, "Almacen_de_barras_de_aluminio", new Vector3(16f, 0.6f, 4f), new Vector3(6f, 1.2f, 2f), _metal, true);
        Prop(b, "Maquina_de_medir_por_coordenadas_(CMM)", new Vector3(16f, 1f, -8f), new Vector3(2f, 2f, 2f), M("Granito_CMM", 0.25f, 0.25f, 0.27f), true);
        Prop(b, "Viga_del_puente_grua", new Vector3(0f, 7.6f, 6f), new Vector3(50f, 0.6f, 0.6f), M("Grua_amarilla", 0.9f, 0.75f, 0.1f), false);
        Prop(b, "Oficina_de_la_nave", new Vector3(5f, 1.5f, -16f), new Vector3(10f, 3f, 6f), M("Oficina_tabique", 0.9f, 0.9f, 0.88f), true);
        foreach (float x in new[] { 10f, 18f }) Prop(b, "Puerta_de_muelle", new Vector3(x, 2.2f, 22.1f), new Vector3(3.6f, 4.4f, 0.06f), M("Puerta_muelle", 0.7f, 0.72f, 0.74f), false);
        l.Add(a);
        var c = Shell(city, "Century Business Center (naves nuevas, 2007)", "Taller", new Vector2(-100f, -90f), new Vector2(90f, 60f), 0f, 8.5f,
            tilt, band, false, "CENTURY BUSINESS CENTER", new Color(0.35f, 0.35f, 0.38f), 1.8f, -30f);
        Transform cb = c.transform; c.notHome = true;
        for (int k = 0; k < 4; k++) Prop(cb, "Tabique_entre_naves", new Vector3(-27f + k * 18f, 4.25f, 5f), new Vector3(0.25f, 8.5f, 48f), M("Tabique_nave", 0.82f, 0.8f, 0.76f), true);
        for (int k = 0; k < 4; k++) Prop(cb, "Estanteria_de_palés", new Vector3(-36f + k * 18f, 2.5f, 20f), new Vector3(12f, 5f, 1.2f), M("Rack_naranja", 0.9f, 0.45f, 0.1f), true);
        l.Add(c);
        return l;
    }

    // ── 13 · LOS APARCAMIENTOS DE SPACEX (norte y sur) y la acera con palmeras de la fachada ──
    static void SpaceXLots(Transform city)
    {
        ParkingLot(Node(city, "Aparcamiento norte de SpaceX", D(-10f, 36f)), new Vector2(52f, 26f), 2, 2.6f, 14);
        ParkingLot(Node(city, "Aparcamiento sur de SpaceX (Rocket Road)", D(0f, -68f)), new Vector2(96f, 44f), 2, 2.6f, 24);
        Transform v = Node(city, "Valla de malla de la planta de Northrop", Vector3.zero);
        Material mesh = M("Malla_metalica", 0.6f, 0.62f, 0.62f);
        Prop(v, "Valla_sur", D(0f, -90.5f, 1.2f), new Vector3(96f, 2.4f, 0.05f), mesh, true);
        Prop(v, "Valla_oeste", DRaw(-48.5f, Spread(0f, -68f).y, 1.2f), new Vector3(0.05f, 2.4f, 45f), mesh, true);
        foreach (float x in new[] { -24f, -16f, -8f, 8f, 16f, 24f }) Prop(v, "Arriate_de_la_fachada", DRaw(x, -MuskFactorySize.y * 0.5f * MuskK - 3f, 0.15f), new Vector3(3f, 0.3f, 1.2f), M("Arriate_ladrillo", 0.6f, 0.35f, 0.28f), true);
    }
    // ── 14 · EL GIMNASIO DEL BARRIO (entrenamiento) y la CLÍNICA DE URGENCIAS (recuperación), en Wilkie Ave [S]: la regla 16 pide en cada
    //    pueblo trabajo, ocio, entrenamiento y recuperación (docs/manual-de-diseno.md). Gimnasios de cadena y «urgent care» de 2008 en LA.
    static AreaFunctionSite Gym(Transform city)
    {
        var site = Shell(city, "Gimnasio del barrio (Wilkie Ave)", "Centro de entrenamiento", new Vector2(216f, -20f), new Vector2(44f, 24f), 90f, 6f,
            new Color(0.88f, 0.88f, 0.9f), new Color(0.85f, 0.4f, 0.1f), true, "FITNESS · OPEN 24 HOURS", new Color(0.85f, 0.4f, 0.1f), 1.8f, -6f);
        site.notHome = true;
        Transform b = site.transform; // fachada 24 (x ±12), fondo 44 (z ±22)
        for (int k = 0; k < 6; k++) Prop(b, "Cinta_de_correr", new Vector3(-9f + k * 3.4f, 0.7f, -17f), new Vector3(0.9f, 1.4f, 2f), _dark, true);
        for (int k = 0; k < 4; k++) Prop(b, "Bicicleta_estatica", new Vector3(-8f + k * 3.4f, 0.6f, -11.5f), new Vector3(0.6f, 1.2f, 1.3f), _dark, true);
        for (int k = 0; k < 2; k++) DPiece(b, "muneco", new Vector3(-6f + k * 9f, 0f, -3f), 0f, $"Rack de sentadillas {k + 1}");
        Prop(b, "Banco_de_press_con_barra", new Vector3(3f, 0.25f, 4f), new Vector3(0.5f, 0.5f, 1.4f), _dark, true);
        Prop(b, "Mancuernero_(2_a_45_kg)", new Vector3(-10.5f, 0.5f, 6f), new Vector3(0.7f, 1f, 6f), _metal, true);
        Prop(b, "Espejo_de_pared", new Vector3(-11.8f, 2f, 6f), new Vector3(0.04f, 2.5f, 10f), _glass, false);
        Prop(b, "Maquina_de_poleas", new Vector3(8f, 1.2f, 10f), new Vector3(3f, 2.4f, 1.2f), _metal, true);
        Prop(b, "Suelo_de_caucho", new Vector3(0f, 0.11f, 4f), new Vector3(22f, 0.02f, 16f), M("Caucho_negro", 0.15f, 0.15f, 0.16f), false);
        DPiece(b, "esterilla", new Vector3(5f, 0f, 16f), 0f, "Sala de estiramientos y yoga");
        Prop(b, "Mostrador_de_recepcion", new Vector3(8f, 0.55f, -20f), new Vector3(3.5f, 1.1f, 0.8f), M("Mostrador_laminado", 0.55f, 0.45f, 0.35f), true);
        Prop(b, "Fuente_de_agua", new Vector3(11.5f, 0.6f, 0f), new Vector3(0.5f, 1.2f, 0.4f), _metal, true);
        Prop(b, "Taquillas_del_vestuario", new Vector3(-6f, 1f, 21.4f), new Vector3(10f, 2f, 0.5f), M("Taquilla_gris", 0.55f, 0.57f, 0.6f), true);
        DPiece(b, "lavabo", new Vector3(4f, 0f, 20.5f), 0f, "Lavabo del vestuario", null, station: false);
        DPiece(b, "inodoro", new Vector3(9f, 0f, 20.5f), 0f, "Aseo del gimnasio", null, station: false);
        return site;
    }

    static AreaFunctionSite Clinic(Transform city)
    {
        var site = Shell(city, "Clínica de urgencias del barrio (urgent care)", "Hospital", new Vector2(216f, -46f), new Vector2(44f, 24f), 90f, 5f,
            new Color(0.95f, 0.95f, 0.94f), new Color(0.15f, 0.45f, 0.7f), true, "URGENT CARE · WALK-INS WELCOME", new Color(0.15f, 0.45f, 0.7f), 1.8f, 6f);
        site.notHome = true;
        Transform b = site.transform;
        Prop(b, "Mostrador_de_admision", new Vector3(6f, 0.55f, -16f), new Vector3(4f, 1.1f, 0.8f), M("Mostrador_laminado", 0.55f, 0.45f, 0.35f), true);
        for (int k = 0; k < 2; k++) DPiece(b, "banco", new Vector3(-6f, 0f, -18f + k * 4f), 0f, "Banco de apoyo de la sala de espera", null, MicroBody.OfSpecies("Ant"), null, station: false);
        for (int r = 0; r < 3; r++)
        {
            float z = -6f + r * 9f;
            CivicBuilder.Partition(b, "consulta_" + r, new Vector2(-11.85f, z - 4f), new Vector2(2f, z - 4f), 2.8f * ShellK, new Color(0.9f, 0.93f, 0.95f), 10f, 0.9f * ShellK, "supuesto ×3: puerta de consulta");
            DPiece(b, "camilla", new Vector3(-7f, 0f, z), 0f, $"Camilla de la consulta {r + 1}");
            Prop(b, "Carro_de_curas", new Vector3(-2f, 0.5f, z + 2f), new Vector3(0.6f, 1f, 0.5f), _metal, true);
            Prop(b, "Tensiometro_de_pared", new Vector3(-11.7f, 1.4f, z + 1f), new Vector3(0.1f, 0.4f, 0.3f), _dark, false);
        }
        Prop(b, "Sala_de_rayos_X_(equipo_portatil)", new Vector3(7f, 1f, 8f), new Vector3(1.4f, 2f, 1.2f), M("Rayos_X_crema", 0.88f, 0.86f, 0.8f), true);
        Prop(b, "Nevera_de_vacunas", new Vector3(10.5f, 0.9f, 16f), new Vector3(0.8f, 1.8f, 0.8f), _glass, true);
        DPiece(b, "armario", new Vector3(8f, 0f, 20f), 0f, "Botiquín y farmacia de la clínica", new Vector2(1.6f, 0.6f));
        DPiece(b, "lavabo", new Vector3(3f, 0f, 20.5f), 0f, "Lavabo de la clínica", null, station: false);
        DPiece(b, "inodoro", new Vector3(-9f, 0f, 20.5f), 0f, "Aseo de la clínica", null, station: false);
        return site;
    }
}
