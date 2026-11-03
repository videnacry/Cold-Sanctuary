using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS PUEBLOS DE LOS CIENTÍFICOS E HISTÓRICOS (docs/microcosmos-cientificos.md; Berón 2026-11-01: «los microcosmos en eras
/// avanzadas deben ocupar todo el piso para recrear un pueblo o ciudad, poblada solo por insectos-humanos civilizados»).
/// Cada historia trae su PUEBLO REAL: los edificios donde ocurrió (investigados, con fuentes en el doc) cerca del claro de la
/// historia, y el resto del piso lleno de calles y solares de la era (casas, talleres, tiendas…) con su gente y sus familias.
/// Sigue las reglas de edificación (docs/microcosmos-reglas-edificacion.md): edificios abiertos de <see cref="Building"/>,
/// calles de <see cref="MicroBuildRules.Street"/>, el claro de la historia (radio ~26 m, entrada y portal en z −14…−20) libre.
/// El piso viene de <see cref="AreaMicrocosmos.FloorRectLocal"/> (el área gigante ya sin muebles: era avanzada).
/// </summary>
internal static partial class CivicBuilder
{
    // ── Estilos de las eras nuevas (colores de sus materiales reales: el suelo del edificio los lleva) ──
    static readonly Style GreekStone = new Style(C(0.86f, 0.8f, 0.66f), C(0.72f, 0.62f, 0.48f), C(0.7f, 0.35f, 0.25f), Roof.Gable, 3.4f);      // Crotona: piedra y teja
    static readonly Style NileMud = new Style(C(0.6f, 0.5f, 0.38f), C(0.55f, 0.45f, 0.33f), C(0.7f, 0.62f, 0.45f), Roof.Flat, 3f);            // Menfis: adobe del limo del Nilo
    static readonly Style AbbasidBrick = new Style(C(0.82f, 0.7f, 0.5f), C(0.75f, 0.62f, 0.45f), C(0.7f, 0.6f, 0.45f), Roof.Flat, 4f);       // Bagdad: ladrillo cocido y yeso
    static readonly Style Limewash = new Style(C(0.9f, 0.88f, 0.8f), C(0.5f, 0.42f, 0.32f), C(0.75f, 0.62f, 0.4f), Roof.Gable, 3.2f);        // Berkeley: piedra encalada
    static readonly Style Thatch = new Style(C(0.82f, 0.76f, 0.6f), C(0.45f, 0.38f, 0.3f), C(0.7f, 0.6f, 0.38f), Roof.Gable, 3f);           // cabañas de paja inglesas
    static readonly Style Colonial = new Style(C(0.93f, 0.88f, 0.75f), C(0.7f, 0.42f, 0.3f), C(0.65f, 0.3f, 0.2f), Roof.Gable, 4f);         // Angostura: casa colonial, piso de ladrillo
    static readonly Style Engadine = new Style(C(0.92f, 0.9f, 0.85f), C(0.55f, 0.45f, 0.35f), C(0.4f, 0.4f, 0.42f), Roof.Gable, 4f);        // Sils: casa engadinesa encalada, suelo de madera
    static readonly Style Haussmann = new Style(C(0.85f, 0.8f, 0.68f), C(0.5f, 0.38f, 0.28f), C(0.35f, 0.38f, 0.42f), Roof.Flat, 4f);       // París: piedra de París, parqué
    static readonly Style FrontierWood = new Style(C(0.65f, 0.5f, 0.35f), C(0.5f, 0.4f, 0.3f), C(0.4f, 0.35f, 0.3f), Roof.Gable, 3.6f);     // Colorado: tablas de pino
    static readonly Style Arcades = new Style(C(0.78f, 0.72f, 0.58f), C(0.6f, 0.55f, 0.45f), C(0.5f, 0.32f, 0.25f), Roof.Gable, 4f);        // Berna: arenisca de los soportales
    static readonly Style Stucco = new Style(C(0.95f, 0.9f, 0.8f), C(0.72f, 0.7f, 0.66f), C(0.7f, 0.4f, 0.3f), Roof.Flat, 3.4f);            // Pasadena: estuco y terrazo
    static readonly Style Panel = new Style(C(0.8f, 0.8f, 0.78f), C(0.55f, 0.55f, 0.55f), C(0.5f, 0.5f, 0.5f), Roof.Flat, 3.2f);            // Prípiat: bloques de hormigón prefabricado
    static readonly Style Reactor = new Style(C(0.6f, 0.6f, 0.58f), C(0.35f, 0.38f, 0.36f), C(0.45f, 0.45f, 0.45f), Roof.Flat, 4f);         // la central: hormigón y acero

    // Un solar del pueblo: lo que se construye en él.
    internal sealed class Lot
    {
        public string name, template, title; public Vector2 size; public Style style;
        public (string key, Vector2 at, Vector2 size, string label)[] stations;
        public Lot(string name, string template, string title, float w, float d, Style st, params (string key, Vector2 at, Vector2 size, string label)[] stations)
        { this.name = name; this.template = template; this.title = title; size = new Vector2(w, d); style = st; this.stations = stations; }
        /// <summary>(2026-11-03) Un edificio de VARIAS PLANTAS en terraza (CivicBuilder.Storeys): los puestos de cada planta.</summary>
        public (string key, Vector2 at, Vector2 size, string label)[][] floors;
    }

    const float ClearingRadius = 26f;   // el claro de la historia (docs/microcosmos-reglas-edificacion.md, receta 2)
    // Tope de edificios por pueblo (rendimiento). Las manzanas NO se agrandan para repartir el piso entre pocos edificios (con
    // 40, en el piso de 388 × 368 m de la Cocina salían manzanas de 59 m con casas de 10 m: un llano vacío con casitas sueltas);
    // tienen el tamaño de su edificio más grande + paso + calle, así que el pueblo es DENSO. Con 120 caben enteras las
    // ciudades (París, Berna, Pasadena, Bagdad) y los pueblos medianos; solo en los pisos enormes (Angostura sobre la Cocina,
    // Prípiat sobre la Central) quedan libres las manzanas más lejanas del claro: las afueras, campo de la era (MicroBiome).
    const int MaxLots = 120;
    /// <summary>Manzanas (las más lejanas del claro) que el relleno deja libres para las CASAS NUEVAS de las familias (regla 6,
    /// <see cref="Homes"/>): con los solares de relleno amueblados (2–3 muebles), sin esta reserva no quedaba solar libre y
    /// hasta 8 vecinos se quedaban sin hogar (PC de Berón, 2026-11-03: París, Menfis, Woolsthorpe, Berkeley, Berna, Pasadena).</summary>
    const int HomeReserve = 8;

    /// <summary>
    /// EL PUEBLO EN EL PISO ENTERO: reparte el piso (fuera del claro de la historia) en una cuadrícula de manzanas separadas por
    /// calles de <see cref="MicroBuildRules.Street"/>. Los edificios de la historia (<paramref name="named"/>) van a los solares
    /// más cercanos al claro, en su orden; el resto del piso se llena con los de <paramref name="fill"/> (rotando), hasta
    /// <see cref="MaxLots"/>. Devuelve los sitios en el mismo orden (los nombrados primero; null si no cupo).
    /// </summary>
    static List<AreaFunctionSite> Town(Transform t, string town, Lot[] named, Lot[] fill, Color street)
    {
        if (_plan != null) return PlannedTown(t, town, named, fill);
        Rect r = AreaMicrocosmos.FloorRectLocal(t);
        // Tamaño de la manzana: la del edificio más grande + 2 m de paso a cada lado + calle (ver MaxLots).
        float biggest = 14f;
        foreach (Lot l in named) biggest = Mathf.Max(biggest, Mathf.Max(l.size.x, l.size.y) + 2f * MicroBuildRules.Passage);
        float cell = biggest + MicroBuildRules.Street;

        // Las manzanas que caben enteras en el piso y fuera del claro (su esquina más cercana, a más de ClearingRadius).
        var cells = new List<Vector2>();
        int nx = Mathf.Max(1, Mathf.FloorToInt(r.width / cell)), nz = Mathf.Max(1, Mathf.FloorToInt(r.height / cell));
        float x0 = r.center.x - nx * cell * 0.5f + cell * 0.5f, z0 = r.center.y - nz * cell * 0.5f + cell * 0.5f;
        for (int i = 0; i < nx; i++)
            for (int k = 0; k < nz; k++)
            {
                var c = new Vector2(x0 + i * cell, z0 + k * cell);
                float half = (cell - MicroBuildRules.Street) * 0.5f;
                Vector2 near = new Vector2(Mathf.Clamp(0f, c.x - half, c.x + half), Mathf.Clamp(0f, c.y - half, c.y + half));
                if (near.magnitude < ClearingRadius) continue;
                cells.Add(c);
            }
        cells.Sort((a, b) => a.sqrMagnitude.CompareTo(b.sqrMagnitude));

        // Las CALLES: franjas de suelo del color de la era entre las manzanas (solo dibujo: se pisan).
        Material mStreet = AreaMicrocosmos.Mat("Civ_Calle_" + Hex(street), street);
        Transform streets = new GameObject($"Calles de {town}").transform; streets.SetParent(t, false);
        for (int i = 0; i <= nx; i++)
        {
            float x = x0 - cell * 0.5f + i * cell;
            AreaMicrocosmos.Box(streets, "Calle", new Vector3(x, 0.02f, r.center.y), new Vector3(MicroBuildRules.Street, 0.04f, nz * cell), mStreet, false);
        }
        for (int k = 0; k <= nz; k++)
        {
            float z = z0 - cell * 0.5f + k * cell;
            AreaMicrocosmos.Box(streets, "Calle", new Vector3(r.center.x, 0.02f, z), new Vector3(nx * cell, 0.04f, MicroBuildRules.Street), mStreet, false);
        }

        var sites = new List<AreaFunctionSite>();
        int used = 0;
        foreach (Lot l in named)
        {
            if (used >= cells.Count) { Debug.LogWarning($"[Pueblos] {town}: no cabe «{l.name}» (piso de {r.width:0} × {r.height:0} m)."); sites.Add(null); continue; }
            Vector2 c = cells[used++];
            sites.Add(Building(t, l.name, l.template, l.title, new Vector3(c.x, 0f, c.y), 0f, l.size, l.style, l.stations));
        }
        int f = 0; _home = null;
        int fillUpTo = Mathf.Min(MaxLots, Mathf.Max(used, cells.Count - HomeReserve));
        while (used < fillUpTo && fill.Length > 0)
        {
            Lot l = fill[f % fill.Length];
            Vector2 c = cells[used++];
            string n = fill.Length > 1 || f > 0 ? $"{l.name} {f / fill.Length + 1}" : l.name;
            // (2026-11-01) Un solar de relleno sin puestos propios (las casas del pueblo: «Casa de Berna», «Casa de Pasadena»…) recibe
            // el de su necesidad básica, la cama, como las casas de FillTown: sin ninguno, CivilizedErasTest daba «sin puestos» y sus
            // vecinos no encontraban puesto en su edificio (grupo 62: Einstein, Patterson, Newton, Jenner, Bolívar, Tesla…).
            var st = l.stations != null && l.stations.Length > 0 ? l.stations : new[] { S("cama", 0f, 0f, 1f, 2f, NeedLabel("cama", Year)) };
            AreaFunctionSite site = Building(t, n, l.template, l.title, new Vector3(c.x, 0f, c.y), 0f, l.size, l.style, st);
            if (_home == null && l.template == "Casa") _home = site;
            sites.Add(site);
            f++;
        }
        int outskirts = cells.Count - used;
        Debug.Log($"[Pueblos] {town}: {named.Length} edificios de la historia y {f} solares del pueblo en un piso de {r.width:0} × {r.height:0} m (manzanas de {cell:0} m, calles de {MicroBuildRules.Street:0} m)"
                  + (outskirts > 0 ? $"; {outskirts} manzanas de las afueras quedan libres (casas nuevas de las familias, campo)." : "."));
        return sites;
    }

    /// <summary>
    /// EL PUEBLO SOBRE SU PLANO REAL (2026-11-03, CivicBuilder.Plans): los edificios de la historia (<paramref name="named"/>) en su
    /// sitio del mapa (los «hito» del plano; sin hito, junto al claro), los edificios del pueblo (PlanCivic) y el relleno
    /// (<paramref name="fill"/>, rotando) a lo largo de sus calles, de frente a ellas. Devuelve los sitios en el orden de siempre (los
    /// de la historia primero, null si no cupo; luego los del relleno), para que su gente (W, Townsfolk) se reparta igual.
    /// </summary>
    static List<AreaFunctionSite> PlannedTown(Transform t, string town, Lot[] named, Lot[] fill)
    {
        var lots = new Lots(t);
        if (named.Length > 0) _planStyle = named[0].style;
        var sites = new List<AreaFunctionSite>();
        for (int i = 0; i < named.Length; i++)
        {
            Lot l = named[i];
            AreaFunctionSite s = l.floors != null ? Storeys(t, l.name, l.template, l.title, l.style, l.floors) : Building(t, l.name, l.template, l.title, Vector3.zero, 0f, l.size, l.style, l.stations);
            Vector2 near = _plan.hitos.TryGetValue(i, out Vector2 h) ? PlanPoint(h) : new Vector2(0f, 40f);
            if (!PlaceOn(lots, s, near)) { Debug.LogWarning($"[Pueblos] {town}: no cabe «{l.name}»."); Object.DestroyImmediate(s.gameObject); sites.Add(null); continue; }
            sites.Add(s);
        }
        PlanCivic(t, lots, _plan);
        _home = null;
        var filled = PlanFill(t, lots, _plan, fill, Mathf.Min(_plan.fill, PlanMaxSites - Count(t)));
        foreach (AreaFunctionSite s in filled) { if (_home == null && s.building == "Casa") _home = s; sites.Add(s); }
        Debug.Log($"[Pueblos] {town}: {named.Length} edificios de la historia en su sitio del plano y {filled.Count} solares del pueblo sobre sus calles.");
        return sites;
    }

    /// <summary>La gente del pueblo de relleno: una pareja (que tendrá hijos) en cada <paramref name="every"/> casas, hasta
    /// <paramref name="max"/> parejas; trabajan en el edificio del solar, comen en <paramref name="dine"/>.</summary>
    static int Townsfolk(Transform t, List<AreaFunctionSite> sites, int from, string she, string he, AreaFunctionSite dine, AreaFunctionSite practice, int every = 3, int max = 6)
    {
        int n = 0, couples = 0;
        for (int i = from; i < sites.Count && couples < max; i += every)
        {
            AreaFunctionSite s = sites[i];
            if (s == null) continue;
            couples++;
            n += C2(t, $"{she} {couples}", $"{he} {couples}", s, dine ?? s, practice);
        }
        return n;
    }

    static T At<T>(List<T> l, int i) where T : class => i < l.Count ? l[i] : null;

    // Un trabajador de la historia, con su casa en la primera casa del pueblo (el primer solar de relleno, <paramref name="home"/>)
    // si no tiene otra. Sin sitio (el piso no daba para su edificio) no se pone.
    static int W(Transform t, string name, AreaFunctionSite work, string role, AreaFunctionSite dine, AreaFunctionSite practice, AreaFunctionSite home)
        => work == null ? 0 : Worker(t, name, work, role, dine ?? work, practice, home ?? _home ?? work);
    static int C2(Transform t, string she, string he, AreaFunctionSite home, AreaFunctionSite dine, AreaFunctionSite practice)
        => home == null ? 0 : Couple(t, she, he, home, dine ?? home, practice);
    static AreaFunctionSite _home;   // la primera casa del pueblo que se está construyendo (Town la fija)

    // ══ CROTONA (~530 a. C.): la comunidad pitagórica (el homakoeion, la «sala de oír juntos»), la palestra de Milón, el
    //    templo de Hera Lacinia en el cabo, la casa de Milón donde se reunían (incendiada ~450 a. C., Polibio II.39), una
    //    herrería y el ágora. Casas griegas de patio (piedra y teja). (Burkert 1972; Huffman, SEP «Pythagoras»; Spadea 1997 sobre Crotona.)
    static int PythagorasTown(Transform t)
    {
        var s = Town(t, "Crotona", new[]
        {
            new Lot("Homakoeion (la escuela pitagórica)", "Escuela", "Comunidad pitagórica de Crotona", 14f, 10f, GreekStone,
                S("pizarra", -3f, 2f, 1.6f, 0.4f, "Tablilla de arena (ábaco de figuras)"), S("mesa", 3f, 2f, 1.6f, 1f, "Mesa común (comida sin carne)"), S("estante", 0f, -2f, 0.8f, 2f, "Monocordios y liras")),
            new Lot("Palestra de Milón", "Dojo", "Palestra de Crotona", 14f, 10f, GreekStone,
                S("tatami", 0f, 0f, 6f, 5f, "Arena de lucha (skamma)")),
            new Lot("Templo de Hera Lacinia", "Templo", "Templo de Hera Lacinia (cabo Lacinio)", 12f, 10f, GreekStone,
                S("altar", 0f, 0f, 2f, 1f, "Altar de Hera"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa de ofrendas"), S("estante", 0f, 0f, 0.8f, 2f, "Exvotos")),
            new Lot("Casa de Milón", "Casa", "Casa de Milón (aquí se reunían)", 12f, 10f, GreekStone,
                S("cama", -3f, 2f, 1.2f, 2f, "Lecho"), S("fogon", 3f, 2f, 1.6f, 0.8f, "Hogar"), S("mesa", 0f, -2f, 1.6f, 1f, "Mesa")),
            new Lot("Herrería del ágora", "Taller", "Herrería (los martillos de la leyenda)", 10f, 8f, GreekStone,
                S("banco_taller", 0f, 0f, 2f, 0.9f, "Yunque y martillos")),
        }, new[]
        {
            new Lot("Casa griega de patio", "Casa", "Casa de Crotona", 10f, 9f, GreekStone),
            new Lot("Puesto del ágora", "Mercado", "Ágora de Crotona", 10f, 8f, GreekStone, S("puesto", 0f, 0f, 2f, 1f, "Puesto (aceite, higos, miel)"), S("puesto", 0f, 0f, 2f, 1f, "Puesto de vasijas"), S("estante", 0f, 0f, 0.8f, 2f, "Ánforas de aceite")),
            new Lot("Taller de cerámica", "Alfarería", "Taller de cerámica", 10f, 8f, GreekStone, S("torno_alfarero", 0f, 0f, 1f, 1f, "Torno de alfarero"), S("horno", 0f, 0f, 1.6f, 1.6f, "Horno de cerámica"), S("estante", 0f, 0f, 0.8f, 2f, "Vasijas secándose")),
            new Lot("Huerto de olivos", "Huerto", "Olivar", 12f, 10f, GreekStone, S("bancal", 0f, 0f, 4f, 6f, "Bancal de legumbres"), S("pozo", 0f, 0f, 1f, 1f, "Pozo")),
        }, C(0.72f, 0.62f, 0.48f));
        int n = 0;
        n += W(t, "Teano", s[0], "maestro", s[0], s[1], s[3]);
        n += W(t, "Acusmático de la escuela", s[0], "alumno", s[0], s[1], s[3]);
        n += W(t, "Matemático de la escuela", s[0], "alumno", s[0], s[1], s[3]);
        n += W(t, "Herrero de Crotona", s[4], "mecanico", s[0], null, s[3]);
        n += W(t, "Sacerdotisa de Hera", s[2], "", s[0], null, s[3]);
        n += Townsfolk(t, s, 5, "Vecina de Crotona", "Vecino de Crotona", s[0], s[1]);
        return n;
    }

    // ══ MENFIS (~450 a. C.): el templo de Ptah (Hefesto para Heródoto, II.99), el puerto del Nilo, los campos de la crecida con
    //    su shaduf, el mercado y las casas de adobe del limo. (Heródoto II; Lloyd 1975–88; Jeffreys, The Survey of Memphis, EES.)
    static int HerodotusTown(Transform t)
    {
        var s = Town(t, "Menfis", new[]
        {
            new Lot("Templo de Ptah", "Templo", "Templo de Ptah (Hefesto) en Menfis", 16f, 12f, NileMud,
                S("altar", 0f, 3f, 2f, 1f, "Altar de Ptah"), S("estante", -4f, -2f, 0.8f, 2.4f, "Archivo de papiros de los sacerdotes")),
            new Lot("Puerto de Menfis", "Mercado", "Puerto del Nilo", 14f, 10f, NileMud,
                S("puesto", -3f, 0f, 2f, 1f, "Barca de papiro con grano"), S("puesto", 3f, 0f, 2f, 1f, "Ánforas de vino de Grecia")),
            new Lot("Campos de la crecida", "Huerto", "Campos inundados por el Nilo", 16f, 12f, NileMud,
                S("bancal", -4f, 0f, 4f, 8f, "Campo de trigo emmer"), S("bancal", 4f, 0f, 4f, 8f, "Campo de lino"), S("pozo", 0f, 5f, 1.2f, 1.2f, "Shaduf (cigoñal)")),
            new Lot("Panadería y cervecería", "Cocina", "Panadería y cervecería", 12f, 9f, NileMud,
                S("horno", -3f, 0f, 1.6f, 1.6f, "Horno de pan"), S("fogon", 3f, 0f, 1.6f, 0.8f, "Tinajas de cerveza")),
            new Lot("Casa del escriba", "Casa", "Casa de un escriba de Menfis", 12f, 10f, NileMud,
                S("mesa", 0f, 0f, 1.6f, 1f, "Mesa de escribir (paleta y papiro)")),
        }, new[]
        {
            new Lot("Casa de adobe", "Casa", "Casa de Menfis", 10f, 9f, NileMud),
            new Lot("Taller de lino", "Taller", "Taller de lino", 10f, 8f, NileMud, S("telar", 0f, 0f, 2f, 1f, "Telar horizontal"), S("telar", 0f, 0f, 2f, 1f, "Telar del aprendiz"), S("lavadero", 0f, 0f, 1.6f, 1f, "Balsa de enriar el lino")),
            new Lot("Alfarería del barrio", "Alfarería", "Alfarería", 10f, 8f, NileMud, S("torno_alfarero", 0f, 0f, 1f, 1f, "Torno de alfarero"), S("horno", 0f, 0f, 1.6f, 1.6f, "Horno de cerámica"), S("estante", 0f, 0f, 0.8f, 2f, "Jarras de cerveza secándose")),
            new Lot("Granero", "Granero", "Granero del templo", 10f, 9f, NileMud, S("granero", 0f, 0f, 2f, 2f, "Silo de grano"), S("granero", 0f, 0f, 2f, 2f, "Silo de cebada"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa del escriba del granero")),
        }, C(0.42f, 0.35f, 0.27f));
        int n = 0;
        n += W(t, "Sacerdote de Ptah", s[0], "sabio", s[3], s[0], s[4]);
        n += W(t, "Barquero del Nilo", s[1], "", s[3], null, s[4]);
        n += W(t, "Campesino de la crecida", s[2], "hortelano", s[3], null, s[4]);
        n += W(t, "Panadera de Menfis", s[3], "cocinero", s[3], null, s[4]);
        n += W(t, "Escriba de Menfis", s[4], "sabio", s[3], null, s[4]);
        n += Townsfolk(t, s, 5, "Vecina de Menfis", "Vecino de Menfis", s[3], s[0]);
        return n;
    }

    // ══ BAGDAD (~820): la Ciudad Redonda de al-Mansur (762) y el barrio de al-Karj: la Casa de la Sabiduría (Bayt al-Ḥikma, la
    //    biblioteca y los traductores), el tribunal del cadí (las herencias), el zoco de al-Karj, el molino de papel (~794), el
    //    hospital (bīmāristān) y el canal. (Lassner 1970, The Topography of Baghdad; Gutas 1998; Bloom 2001, Paper Before Print.)
    static int AlKhwarizmiTown(Transform t)
    {
        var s = Town(t, "Bagdad", new[]
        {
            new Lot("Casa de la Sabiduría (Bayt al-Ḥikma)", "Escuela", "Casa de la Sabiduría", 16f, 12f, AbbasidBrick,
                S("estante", -5f, 3f, 0.8f, 3f, "Libros traducidos (griego, persa, sánscrito)"), S("mesa", 0f, 2f, 2f, 1f, "Mesa de los traductores"), S("pizarra", 4f, -2f, 1.6f, 0.4f, "Tablero de polvo (cálculo)")),
            new Lot("Tribunal del cadí", "Escuela", "Tribunal del cadí (herencias)", 12f, 10f, AbbasidBrick,
                S("mesa", 0f, 0f, 2f, 1f, "Mesa del cadí")),
            new Lot("Zoco de al-Karj", "Mercado", "Zoco de al-Karj", 16f, 10f, AbbasidBrick,
                S("puesto", -4f, 0f, 2f, 1f, "Especias"), S("puesto", 0f, 0f, 2f, 1f, "Telas"), S("puesto", 4f, 0f, 2f, 1f, "Cambista (balanza)")),
            new Lot("Molino de papel", "Taller de papeles", "Molino de papel de Bagdad", 12f, 10f, AbbasidBrick,
                S("lavadero", -3f, 0f, 2f, 1f, "Tina de pasta de trapo"), S("mesa", 3f, 0f, 2f, 1f, "Mesa de secado")),
            new Lot("Casa de comidas", "Restaurante", "Casa de comidas del zoco", 12f, 10f, AbbasidBrick,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Tannur (horno de pan)"), S("mesa", 2f, -1f, 1.6f, 1.6f, "Alfombra y mesa baja")),
            new Lot("Bimaristán", "Hospital", "Bimaristán (hospital)", 14f, 10f, AbbasidBrick,
                S("cama", -3f, 2f, 1.2f, 2f, "Cama"), S("cama", 3f, 2f, 1.2f, 2f, "Cama"), S("armario", 0f, -2f, 1.2f, 0.6f, "Botica")),
        }, new[]
        {
            new Lot("Casa de patio", "Casa", "Casa de Bagdad", 10f, 10f, AbbasidBrick),
            new Lot("Taller del zoco", "Taller", "Taller del zoco", 10f, 8f, AbbasidBrick, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del calderero"), S("horno", 0f, 0f, 1.6f, 1.6f, "Fragua"), S("estante", 0f, 0f, 0.8f, 2f, "Calderos y bandejas de cobre")),
            new Lot("Mezquita de barrio", "Templo", "Mezquita del barrio", 12f, 10f, AbbasidBrick, S("altar", 0f, 0f, 1.6f, 0.6f, "Mihrab"), S("lavadero", 0f, 0f, 1.6f, 1f, "Fuente de las abluciones"), S("estante", 0f, 0f, 0.8f, 2f, "Atril y libros")),
            new Lot("Huerto de palmeras", "Huerto", "Huerto de palmeras", 12f, 10f, AbbasidBrick, S("bancal", 0f, 0f, 4f, 6f, "Bancal regado por el canal"), S("pozo", 0f, 0f, 1f, 1f, "Noria del canal")),
        }, C(0.72f, 0.62f, 0.45f));
        int n = 0;
        n += W(t, "Traductor de la Casa de la Sabiduría", s[0], "sabio", s[4], s[0], null);
        n += W(t, "Copista de la Casa de la Sabiduría", s[0], "sabio", s[4], s[0], null);
        n += W(t, "Cadí de al-Karj", s[1], "sabio", s[4], null, null);
        n += W(t, "Mercader de especias", s[2], "", s[4], null, null);
        n += W(t, "Papelero", s[3], "mecanico", s[4], null, null);
        n += W(t, "Cocinero del zoco", s[4], "cocinero", s[4], null, null);
        n += W(t, "Médico del bimaristán", s[5], "medico", s[4], null, null);
        n += Townsfolk(t, s, 6, "Vecina de Bagdad", "Vecino de Bagdad", s[4], s[0]);
        return n;
    }

    // ══ WOOLSTHORPE (1666): la casa solariega de piedra caliza (Woolsthorpe Manor: cocina, salón y el cuarto de Newton con su
    //    ventana), el granero, el huerto de manzanos, el redil de las ovejas, la iglesia de Colsterworth (donde se bautizó) y las
    //    cabañas de paja de la aldea. (National Trust, «Woolsthorpe Manor»; Westfall 1980.)
    static int NewtonTown(Transform t)
    {
        var s = Town(t, "Woolsthorpe", new[]
        {
            new Lot("Woolsthorpe Manor", "Casa", "Woolsthorpe Manor (la casa de Newton)", 14f, 10f, StoneManor,
                S("mesa", -3f, 2f, 1.6f, 1f, "Mesa de estudio (prismas y cuadernos)"), S("estante", 3f, 2f, 0.8f, 2f, "Libros (Descartes, Galileo)")),
            new Lot("Granero de Woolsthorpe", "Granero", "Granero", 12f, 10f, StoneManor,
                S("granero", 0f, 0f, 2f, 2f, "Grano y heno")),
            new Lot("Redil de las ovejas", "Huerto", "Redil y prado", 14f, 10f, Thatch,
                S("bancal", 0f, 0f, 5f, 5f, "Prado del redil")),
            new Lot("Iglesia de Colsterworth", "Templo", "Iglesia de San Juan Bautista (Colsterworth)", 12f, 10f, StoneManor,
                S("altar", 0f, 2f, 2f, 1f, "Altar"), S("mesa", 0f, -2f, 2f, 1f, "Bancos")),
            new Lot("Posada de Colsterworth", "Restaurante", "Posada del camino (Great North Road)", 12f, 10f, Thatch,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Chimenea con olla"), S("mesa", 2f, -1f, 1.6f, 1.6f, "Mesa con bancos")),
        }, new[]
        {
            new Lot("Cabaña de la aldea", "Casa", "Cabaña de Woolsthorpe", 10f, 9f, Thatch),
            new Lot("Herrería", "Taller", "Herrería de la aldea", 10f, 8f, Thatch, S("banco_taller", 0f, 0f, 2f, 0.9f, "Yunque y fragua"), S("horno", 0f, 0f, 1.6f, 1.6f, "Fragua"), S("armario", 0f, 0f, 1f, 0.6f, "Herraduras y clavos")),
            new Lot("Huerto de manzanos", "Huerto", "Huerto de la aldea", 12f, 10f, Thatch, S("bancal", 0f, 0f, 4f, 6f, "Bancal de coles"), S("pozo", 0f, 0f, 1f, 1f, "Pozo")),
        }, C(0.52f, 0.46f, 0.36f));
        int n = 0;
        n += W(t, "Hannah Ayscough", s[0], "", s[0], null, s[0]);       // la madre de Newton, que llevaba la granja
        n += W(t, "Pastor de Woolsthorpe", s[2], "hortelano", s[4], null, s[0]);
        n += W(t, "Posadera de Colsterworth", s[4], "cocinero", s[4], null, s[0]);
        n += W(t, "Párroco de Colsterworth", s[3], "", s[4], null, s[0]);
        n += Townsfolk(t, s, 5, "Vecina de Woolsthorpe", "Vecino de Woolsthorpe", s[4], s[3]);
        return n;
    }

    // ══ BERKELEY (1796): The Chantry (la casa de Jenner, junto a la iglesia), la granja lechera (las vacas, aquí pulgones), la
    //    casa de los Phipps, la iglesia de Santa María, el castillo de Berkeley y la calle mayor con su posada. (Jenner Museum;
    //    Jenner 1798; Riedel 2005.)
    static int JennerTown(Transform t)
    {
        var s = Town(t, "Berkeley", new[]
        {
            new Lot("The Chantry", "Hospital", "The Chantry (casa y consulta de Jenner)", 14f, 10f, Limewash,
                S("camilla", -3f, 2f, 1f, 2f, "Silla de vacunar"), S("mesa", 3f, 2f, 1.6f, 1f, "Mesa (lancetas y cuadernos)"), S("estante", 0f, -2f, 0.8f, 2f, "Frascos de linfa")),
            new Lot("Granja lechera", "Granero", "Granja lechera de Berkeley", 14f, 10f, Thatch,
                S("granero", -3f, 0f, 2f, 2f, "Establo (las vacas)"), S("lavadero", 3f, 0f, 1.6f, 1f, "Cubo y taburete de ordeñar")),
            new Lot("Casa de los Phipps", "Casa", "Casa de los Phipps (el jardinero)", 12f, 10f, Thatch,
                S("cama", -2f, 2f, 1.2f, 2f, "Cama de James")),
            new Lot("Iglesia de Santa María", "Templo", "Iglesia de Santa María de Berkeley", 12f, 10f, Limewash,
                S("altar", 0f, 2f, 2f, 1f, "Altar"), S("banco", 0f, 0f, 2f, 0.6f, "Banco de la nave"), S("banco", 0f, 0f, 2f, 0.6f, "Banco de la nave"), S("estante", 0f, 0f, 0.8f, 2f, "Libros de oración")),
            new Lot("Posada de la calle mayor", "Restaurante", "Posada de Berkeley", 12f, 10f, Limewash,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Chimenea con olla"), S("mesa", 2f, -1f, 1.6f, 1.6f, "Mesa con bancos")),
        }, new[]
        {
            new Lot("Cabaña de Berkeley", "Casa", "Cabaña de Berkeley", 10f, 9f, Thatch),
            new Lot("Quesería", "Cocina", "Quesería", 10f, 8f, Limewash, S("fogon", 0f, 0f, 1.6f, 0.8f, "Caldero de cuajar"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa de prensar el queso"), S("estante", 0f, 0f, 0.8f, 2f, "Quesos curándose")),
            new Lot("Prado de pastos", "Huerto", "Prado", 12f, 10f, Thatch, S("bancal", 0f, 0f, 4f, 6f, "Pasto"), S("pozo", 0f, 0f, 1f, 1f, "Abrevadero")),
            new Lot("Taller del herrador", "Taller", "Herrador", 10f, 8f, Thatch, S("banco_taller", 0f, 0f, 2f, 0.9f, "Yunque"), S("horno", 0f, 0f, 1.6f, 1.6f, "Fragua"), S("armario", 0f, 0f, 1f, 0.6f, "Herraduras y clavos")),
        }, C(0.5f, 0.44f, 0.36f));
        int n = 0;
        n += W(t, "Catherine Jenner", s[0], "", s[4], null, s[0]);
        n += W(t, "Ordeñadora de la granja", s[1], "", s[4], null, s[2]);
        n += W(t, "Granjero de Berkeley", s[1], "hortelano", s[4], null, s[2]);
        n += W(t, "Posadero de Berkeley", s[4], "cocinero", s[4], null, s[2]);
        n += Townsfolk(t, s, 5, "Vecina de Berkeley", "Vecino de Berkeley", s[4], s[3]);
        return n;
    }

    // ══ ANGOSTURA (1819): la Casa del Congreso (la casa del Correo, en la plaza), la imprenta del Correo del Orinoco, la plaza
    //    mayor y la catedral, el puerto del Orinoco con la Piedra del Medio, la cocina de la tropa (casabe) y el cuartel. Casas
    //    coloniales de bahareque encalado, patio y tejas. (Lynch 2006; Casa del Congreso de Angostura, hoy museo.)
    static int BolivarTown(Transform t)
    {
        var s = Town(t, "Angostura", new[]
        {
            new Lot("Casa del Congreso de Angostura", "Escuela", "Casa del Congreso (15 de febrero de 1819)", 16f, 12f, Colonial,
                S("mesa", 0f, 3f, 3f, 1f, "Mesa de la presidencia"), S("mesa", -4f, -2f, 2f, 1f, "Escaños de los diputados"), S("mesa", 4f, -2f, 2f, 1f, "Escaños de los diputados")),
            new Lot("Imprenta del Correo del Orinoco", "Taller de papeles", "Imprenta del Correo del Orinoco", 12f, 10f, Colonial,
                S("banco_taller", -2f, 0f, 2f, 1f, "Prensa de imprenta"), S("estante", 3f, 0f, 0.8f, 2f, "Cajas de tipos")),
            new Lot("Cocina de la tropa", "Cocina", "Cocina de la tropa (casabe y carne)", 14f, 10f, Colonial,
                S("fogon", -3f, 2f, 2f, 1f, "Budare del casabe"), S("fogon", 3f, 2f, 1.6f, 0.8f, "Fogón de la carne"), S("mesa", 0f, -2f, 2f, 1f, "Mesa de la tropa")),
            new Lot("Puerto del Orinoco", "Mercado", "Puerto de Angostura", 14f, 10f, Colonial,
                S("puesto", -3f, 0f, 2f, 1f, "Curiara con cueros y tabaco"), S("puesto", 3f, 0f, 2f, 1f, "Pesca del Orinoco")),
            new Lot("Catedral de Angostura", "Templo", "Catedral de Nuestra Señora de las Nieves", 14f, 10f, Colonial,
                S("altar", 0f, 2f, 2f, 1f, "Altar")),
            new Lot("Cuartel y hospital de sangre", "Hospital", "Hospital de sangre", 14f, 10f, Colonial,
                S("cama", -3f, 2f, 1.2f, 2f, "Catre"), S("cama", 3f, 2f, 1.2f, 2f, "Catre")),
        }, new[]
        {
            new Lot("Casa colonial", "Casa", "Casa de Angostura", 10f, 10f, Colonial),
            new Lot("Pulpería", "Mercado", "Pulpería", 10f, 8f, Colonial, S("puesto", 0f, 0f, 2f, 1f, "Mostrador de la pulpería"), S("barra", 0f, 0f, 2f, 0.8f, "Barra (guarapo y café)"), S("estante", 0f, 0f, 0.8f, 2f, "Sacos, velas y aguardiente")),
            new Lot("Conuco", "Huerto", "Conuco (yuca y maíz)", 12f, 10f, Colonial, S("bancal", 0f, 0f, 4f, 6f, "Yuca amarga"), S("fogon", 0f, 0f, 1.6f, 0.8f, "Budare del casabe")),
        }, C(0.6f, 0.45f, 0.35f));
        int n = 0;
        n += W(t, "Impresor del Correo del Orinoco", s[1], "mecanico", s[2], null, null);
        n += W(t, "Cocinera de la tropa", s[2], "cocinero", s[2], null, null);
        n += W(t, "Soldado llanero", s[5], "soldado", s[2], null, null);
        n += W(t, "Voluntario de la Legión Británica", s[5], "soldado", s[2], null, null);
        n += W(t, "Bonguero del Orinoco", s[3], "", s[2], null, null);
        n += W(t, "Diputado de Angostura", s[0], "sabio", s[2], null, null);
        n += Townsfolk(t, s, 6, "Vecina de Angostura", "Vecino de Angostura", s[2], s[4]);
        return n;
    }

    // ══ SILS MARIA (1881): la casa de Gian Durisch (el cuarto que alquilaba Nietzsche; hoy Nietzsche-Haus), la fonda del pueblo,
    //    la iglesia, la escuela, la tienda de Durisch y las casas engadinesas (muros gruesos encalados, sgraffito). (Nietzsche-Haus
    //    Sils Maria; Safranski 2000.)
    static int NietzscheTown(Transform t)
    {
        var s = Town(t, "Sils Maria", new[]
        {
            new Lot("Casa Durisch (Nietzsche-Haus)", "Casa", "Casa Durisch: el cuarto de Nietzsche", 12f, 10f, Engadine,
                S("mesa", -3f, 2f, 1.4f, 0.8f, "Mesa de escribir (los cuadernos)"), S("estante", 3f, 2f, 0.8f, 1.6f, "Libros y medicinas")),
            new Lot("Tienda de Durisch", "Mercado", "Tienda de ultramarinos de Durisch", 10f, 8f, Engadine,
                S("puesto", 0f, 0f, 2f, 1f, "Mostrador")),
            new Lot("Fonda de Sils", "Restaurante", "Fonda de Sils (la comida del mediodía)", 12f, 10f, Engadine,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Cocina de leña"), S("mesa", 2f, -1f, 1.6f, 1.6f, "Mesa redonda de los huéspedes")),
            new Lot("Prado de la orilla", "Sala de Yoga", "Prado junto al lago (caminar y respirar)", 14f, 10f, Engadine,
                S("esterilla", -3f, 0f, 1f, 2f, "Banco del prado"), S("esterilla", 3f, 0f, 1f, 2f, "Banco del prado")),
            new Lot("Iglesia de Sils Maria", "Templo", "Iglesia de Sils Maria", 12f, 10f, Engadine,
                S("altar", 0f, 2f, 2f, 1f, "Altar"), S("banco", 0f, 0f, 2f, 0.6f, "Banco de la nave"), S("banco", 0f, 0f, 2f, 0.6f, "Banco de la nave")),
        }, new[]
        {
            new Lot("Casa engadinesa", "Casa", "Casa de Sils Maria", 10f, 10f, Engadine),
            new Lot("Establo de montaña", "Granero", "Establo y pajar", 10f, 9f, Engadine, S("granero", 0f, 0f, 2f, 2f, "Heno"), S("lavadero", 0f, 0f, 1.6f, 1f, "Abrevadero"), S("armario", 0f, 0f, 1f, 0.6f, "Arreos y cencerros")),
            new Lot("Hotel de los veraneantes", "Restaurante", "Hotel de Sils", 12f, 10f, Engadine, S("mesa", 0f, 0f, 1.6f, 1.6f, "Comedor del hotel"), S("mesa", 0f, 0f, 1.6f, 1.6f, "Mesa de los huéspedes"), S("barra", 0f, 0f, 2f, 0.8f, "Recepción")),
        }, C(0.55f, 0.52f, 0.48f));
        int n = 0;
        n += W(t, "Gian Durisch", s[1], "", s[2], null, s[0]);
        n += W(t, "Cocinera de la fonda", s[2], "cocinero", s[2], null, s[0]);
        n += W(t, "Pastor del Fex", s[3], "hortelano", s[2], null, s[0]);
        n += W(t, "Veraneante de Sils", s[3], "alumno", s[2], s[3], s[0]);
        n += Townsfolk(t, s, 5, "Vecina de Sils", "Vecino de Sils", s[2], s[3]);
        return n;
    }

    // ══ PARÍS, 1898: la Escuela de Física y Química (ESPCI) de la rue Lhomond con el cobertizo (hangar) donde los Curie aislaron
    //    el polonio y el radio, la casa de la rue de la Glacière, la Academia de Ciencias (la nota del 26 de diciembre), un café del
    //    Barrio Latino, la panadería y los inmuebles de piedra de París. (Curie 1923; Quinn 1995; Musée Curie.)
    static int CurieTown(Transform t)
    {
        var s = Town(t, "París", new[]
        {
            new Lot("Cobertizo de la rue Lhomond", "Laboratorio", "El cobertizo de los Curie (ESPCI)", 14f, 10f, LabShed,
                S("mesa_laboratorio", -4f, 2.5f, 2.4f, 1f, "Mesa del electrómetro y el cuarzo piezoeléctrico"), S("fogon", 2f, 2.5f, 1.6f, 1.2f, "Caldero de pechblenda (al aire libre)"),
                S("mesa_laboratorio", 0f, -2f, 2.4f, 1f, "Cristalización fraccionada"), S("estante", 5.5f, -1f, 0.8f, 2f, "Tubos que brillan (sales de radio)")),
            new Lot("Casa de la rue de la Glacière", "Casa", "Casa de los Curie (rue de la Glacière, 24)", 12f, 10f, Haussmann,
                S("cama", -3f, 2f, 1.2f, 2f, "Cuna de Irène")),
            new Lot("Academia de Ciencias", "Escuela", "Academia de Ciencias (Instituto de Francia)", 14f, 10f, Haussmann,
                S("mesa", 0f, 2f, 3f, 1f, "Mesa de las Comptes rendus"), S("estante", -4f, -2f, 0.8f, 2f, "Biblioteca")),
            new Lot("Café del Barrio Latino", "Restaurante", "Café de la rue Mouffetard", 12f, 10f, Haussmann,
                S("barra", -3f, 2f, 3f, 0.8f, "Barra"), S("mesa", 2f, -1f, 1.4f, 1.4f, "Velador")),
            new Lot("Panadería", "Cocina", "Panadería del barrio", 10f, 8f, Haussmann,
                S("horno", 0f, 0f, 1.6f, 1.6f, "Horno de pan")),
        }, new[]
        {
            new Lot("Inmueble de París", "Casa", "Inmueble de piedra", 10f, 10f, Haussmann),
            new Lot("Tienda del barrio", "Mercado", "Tienda", 10f, 8f, Haussmann, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 2f, "Estantería de la tienda"), S("armario", 0f, 0f, 1f, 0.6f, "Cajones de la mercería")),
            new Lot("Taller del barrio", "Taller", "Taller de un artesano", 10f, 8f, Haussmann, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("torno", 0f, 0f, 1.5f, 0.5f, "Torno de pedal"), S("estante", 0f, 0f, 0.8f, 2f, "Piezas y herramientas")),
        }, C(0.42f, 0.42f, 0.45f));
        int n = 0;
        n += W(t, "Gustave Bémont", s[0], "sabio", s[3], null, s[1]);    // jefe de trabajos de la ESPCI: firmó la nota del radio
        n += W(t, "Eugène Curie", s[1], "", s[1], null, s[1]);           // el padre de Pierre, médico: cuidaba a Irène
        n += W(t, "Secretario de la Academia", s[2], "sabio", s[3], null, s[1]);
        n += W(t, "Camarera del café", s[3], "camarero", s[3], null, s[1]);
        n += W(t, "Panadero del barrio", s[4], "cocinero", s[3], null, s[1]);
        n += Townsfolk(t, s, 5, "Vecina de París", "Vecino de París", s[3], null);
        return n;
    }

    // ══ COLORADO SPRINGS (1899): la estación experimental de Tesla en Knob Hill (granero de tablas con techo corredizo y un mástil
    //    de 43 m con su bola de cobre: aquí, sin techo), la central de la El Paso Electric Company (la que quemó), el Alta Vista Hotel
    //    donde vivía, la estación del ferrocarril, la calle de la frontera con sus tiendas de tablas. (Tesla 1978, Colorado Springs
    //    Notes; Carlson 2013.)
    static int TeslaTown(Transform t)
    {
        var s = Town(t, "Colorado Springs", new[]
        {
            new Lot("Estación experimental de Knob Hill", "Laboratorio", "Estación experimental de Tesla (1899)", 16f, 16f, FrontierWood,
                S("mesa_laboratorio", -5f, 5f, 2.4f, 1f, "Mesa de los receptores"), S("banco_taller", 5f, 5f, 2.4f, 0.9f, "Banco de los condensadores"), S("estante", 0f, -6f, 0.8f, 2f, "Cuadernos (Colorado Springs Notes)")),
            new Lot("Central de la El Paso Electric Company", "Taller", "Central eléctrica de Colorado Springs", 14f, 10f, FrontierWood,
                S("torno", -3f, 0f, 2f, 1.2f, "Dinamo"), S("banco_taller", 3f, 0f, 2f, 0.9f, "Cuadro de mandos")),
            new Lot("Alta Vista Hotel", "Restaurante", "Alta Vista Hotel", 14f, 10f, FrontierWood,
                S("fogon", -4f, 2f, 1.6f, 0.8f, "Cocina del hotel"), S("mesa", 2f, -1f, 1.6f, 1.6f, "Comedor"), S("cama", 4f, 3f, 1.2f, 2f, "Cuarto de Tesla")),
            new Lot("Estación del ferrocarril", "Mercado", "Estación de Denver & Rio Grande", 14f, 10f, FrontierWood,
                S("puesto", 0f, 0f, 2f, 1f, "Taquilla")),
            new Lot("Taller de Czito", "Taller", "Taller de los ayudantes", 12f, 10f, FrontierWood,
                S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco de bobinar")),
        }, new[]
        {
            new Lot("Casa de tablas", "Casa", "Casa de Colorado Springs", 10f, 10f, FrontierWood),
            new Lot("Almacén de la frontera", "Mercado", "Almacén general", 10f, 8f, FrontierWood, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 2f, "Latas, harina y café"), S("armario", 0f, 0f, 1f, 0.6f, "Herramientas de minero")),
            new Lot("Herrería y caballeriza", "Taller", "Herrería", 10f, 8f, FrontierWood, S("banco_taller", 0f, 0f, 2f, 0.9f, "Yunque"), S("horno", 0f, 0f, 1.6f, 1.6f, "Fragua"), S("lavadero", 0f, 0f, 1.6f, 1f, "Abrevadero de los caballos")),
        }, C(0.55f, 0.45f, 0.35f));
        int n = 0;
        n += W(t, "Kolman Czito", s[4], "mecanico", s[2], null, null);   // el ayudante de Tesla en Colorado Springs
        n += W(t, "Fritz Löwenstein", s[0], "mecanico", s[2], null, null);
        n += W(t, "Maquinista de la central", s[1], "mecanico", s[2], null, null);
        n += W(t, "Cocinero del Alta Vista", s[2], "cocinero", s[2], null, null);
        n += W(t, "Jefe de estación", s[3], "", s[2], null, null);
        n += Townsfolk(t, s, 5, "Vecina de Colorado Springs", "Vecino de Colorado Springs", s[2], null);
        return n;
    }

    // ══ BERNA (1905): la casa de la Kramgasse 49 (el piso de los Einstein), la Torre del Reloj (Zytglogge), la Oficina Federal de
    //    la Propiedad Intelectual (en el edificio de Correos y Telégrafos), el café de la Academia Olimpia, la tienda y las casas de
    //    soportales de arenisca. (Einstein-Haus Bern; Isaacson 2007; Galison 2003.)
    static int EinsteinTown(Transform t)
    {
        var s = Town(t, "Berna", new[]
        {
            new Lot("Kramgasse 49", "Casa", "Casa de los Einstein (Kramgasse 49)", 12f, 10f, Arcades,
                S("mesa", -3f, 2f, 1.6f, 1f, "Mesa de trabajo (los artículos de 1905)"), S("cama", 3f, 2f, 1.2f, 2f, "Cuna de Hans Albert")),
            new Lot("Oficina de Patentes", "Oficina de patentes", "Oficina Federal de la Propiedad Intelectual", 16f, 10f, Sandstone,
                S("mesa", -5f, 2f, 1.8f, 1f, "Pupitre de examinador"), S("mesa", 0f, 2f, 1.8f, 1f, "Pupitre de examinador"), S("mesa", 5f, 2f, 1.8f, 1f, "Pupitre de examinador"), S("estante", 0f, -3f, 0.8f, 3f, "Solicitudes de patente")),
            new Lot("Café de la Academia Olimpia", "Restaurante", "Café (la Academia Olimpia)", 12f, 10f, Arcades,
                S("barra", -3f, 2f, 3f, 0.8f, "Barra"), S("mesa", 2f, -1f, 1.6f, 1.6f, "Mesa de Solovine y Habicht")),
            new Lot("Panadería de la Kramgasse", "Cocina", "Panadería", 10f, 8f, Arcades,
                S("horno", 0f, 0f, 1.6f, 1.6f, "Horno")),
            new Lot("Relojería", "Taller", "Relojería (relojes sincronizados)", 10f, 8f, Arcades,
                S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del relojero")),
        }, new[]
        {
            new Lot("Casa de soportales", "Casa", "Casa de Berna", 10f, 10f, Arcades),
            new Lot("Tienda de soportales", "Mercado", "Tienda de la Kramgasse", 10f, 8f, Arcades, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 2f, "Estantería de la tienda"), S("armario", 0f, 0f, 1f, 0.6f, "Relojes de pared")),
            new Lot("Escuela de Berna", "Escuela", "Escuela", 12f, 10f, Arcades, S("pizarra", 0f, 0f, 1.6f, 0.4f, "Pizarra"), S("mesa", 0f, 0f, 1.6f, 1f, "Pupitres"), S("mesa", 0f, 0f, 1.6f, 1f, "Pupitres de los pequeños")),
        }, C(0.55f, 0.52f, 0.46f));
        int n = 0;
        n += W(t, "Michele Besso", s[1], "sabio", s[2], null, null);
        n += W(t, "Examinador de patentes", s[1], "sabio", s[2], null, null);
        n += W(t, "Maurice Solovine", s[2], "alumno", s[2], s[2], null);
        n += W(t, "Conrad Habicht", s[2], "alumno", s[2], s[2], null);
        n += W(t, "Relojero de Berna", s[4], "mecanico", s[2], null, null);
        n += W(t, "Panadera de la Kramgasse", s[3], "cocinero", s[2], null, null);
        n += Townsfolk(t, s, 5, "Vecina de Berna", "Vecino de Berna", s[2], null);
        return n;
    }

    // ══ PASADENA (1953): el laboratorio limpio de Patterson en Caltech (el primero de su clase: aire filtrado, todo lavado con
    //    ácido), el edificio del espectrómetro de masas (en Argonne, Illinois: aquí, la sala de la máquina), la gasolinera con
    //    gasolina «Ethyl» (tetraetilo de plomo), el diner, la escuela y las casas de estuco. (Caltech Archives, entrevista a
    //    Patterson 1995; Needleman 2000.)
    static int PattersonTown(Transform t)
    {
        var s = Town(t, "Pasadena", new[]
        {
            new Lot("Laboratorio limpio de Caltech", "Laboratorio", "Laboratorio limpio de Patterson (Caltech)", 14f, 10f, Stucco,
                S("mesa_laboratorio", -4f, 2f, 2.4f, 1f, "Campana de aire filtrado"), S("lavadero", 3f, 2f, 1.6f, 1f, "Baño de ácido para el vidrio"), S("estante", 0f, -2f, 0.8f, 2f, "Frascos de teflón")),
            new Lot("Sala del espectrómetro (Argonne)", "Laboratorio", "Espectrómetro de masas (Argonne, Illinois)", 14f, 10f, Panel,
                S("mesa_laboratorio", 0f, 2f, 3f, 1f, "Espectrómetro de masas"), S("mesa", 0f, -2f, 1.6f, 1f, "Registros de isótopos")),
            new Lot("Gasolinera «Ethyl»", "Taller", "Gasolinera (gasolina con plomo)", 14f, 10f, Stucco,
                S("puesto", -3f, 0f, 1f, 1f, "Surtidor «Ethyl»"), S("banco_taller", 3f, 0f, 2f, 0.9f, "Taller de coches")),
            new Lot("Diner de Colorado Boulevard", "Restaurante", "Diner", 12f, 10f, Stucco,
                S("barra", -3f, 2f, 3f, 0.8f, "Barra con taburetes"), S("fogon", 3f, 2f, 1.6f, 0.8f, "Plancha"), S("mesa", 0f, -2f, 1.4f, 1.4f, "Reservado")),
            new Lot("Escuela de Pasadena", "Escuela", "Escuela primaria", 12f, 10f, Stucco,
                S("pizarra", 0f, 2f, 1.6f, 0.4f, "Pizarra"), S("mesa", 0f, -2f, 2f, 1f, "Pupitres")),
        }, new[]
        {
            new Lot("Casa de estuco", "Casa", "Casa de Pasadena", 10f, 10f, Stucco),
            new Lot("Tienda de la avenida", "Mercado", "Tienda", 10f, 8f, Stucco, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 2f, "Estantería del colmado"), S("armario", 0f, 0f, 1f, 0.6f, "Nevera de refrescos")),
            new Lot("Taller mecánico", "Taller", "Taller mecánico", 10f, 8f, Stucco, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("torno", 0f, 0f, 1.5f, 0.5f, "Torno"), S("estante", 0f, 0f, 0.8f, 2f, "Recambios")),
        }, C(0.32f, 0.32f, 0.33f));
        int n = 0;
        n += W(t, "Lorraine Patterson", s[3], "", s[3], null, null);
        n += W(t, "Tsaihwa Chow", s[0], "sabio", s[3], null, null);   // geoquímico del laboratorio de Patterson
        n += W(t, "Técnico de Argonne", s[1], "sabio", s[3], null, null);
        n += W(t, "Gasolinero", s[2], "mecanico", s[3], null, null);
        n += W(t, "Cocinera del diner", s[3], "cocinero", s[3], null, null);
        n += W(t, "Maestra de Pasadena", s[4], "maestro", s[3], null, null);
        n += Townsfolk(t, s, 5, "Vecina de Pasadena", "Vecino de Pasadena", s[3], null);
        return n;
    }

    // ══ PRÍPIAT Y LA CENTRAL (mayo de 1986): la sala del bloque 4 (las válvulas de la piscina de burbujeo), el parque de bomberos
    //    de la central, la sede de la comisión (Hotel Polissia), el Palacio de Cultura Energetik, el parque de atracciones (la noria
    //    que iba a abrir el 1 de mayo), la cantina de los liquidadores y los bloques de viviendas evacuados. (Plokhy 2018;
    //    Leatherbarrow 2016; IAEA INSAG-7.)
    static int ChernobylTown(Transform t)
    {
        var s = Town(t, "Prípiat", new[]
        {
            new Lot("Sótano del bloque 4", "Laboratorio", "Bloque 4: pasillos inundados y válvulas", 16f, 12f, Reactor,
                S("banco_taller", -4f, 3f, 2f, 0.9f, "Válvula de compuerta"), S("banco_taller", 4f, 3f, 2f, 0.9f, "Válvula de compuerta"), S("lavadero", 0f, -3f, 3f, 2f, "Agua de la piscina de burbujeo")),
            new Lot("Parque de bomberos de la central", "Taller", "Parque de bomberos n.º 2", 14f, 10f, Panel,
                S("banco_taller", -3f, 0f, 2f, 1f, "Bombas y mangueras"), S("armario", 3f, 0f, 1.2f, 0.6f, "Trajes y dosímetros")),
            new Lot("Hotel Polissia (la comisión)", "Escuela", "Hotel Polissia: la comisión del gobierno", 14f, 10f, Panel,
                S("mesa", 0f, 2f, 3f, 1f, "Mesa de la comisión"), S("estante", -4f, -2f, 0.8f, 2f, "Planos de la central")),
            new Lot("Cantina de los liquidadores", "Restaurante", "Cantina", 12f, 10f, Panel,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Cocina"), S("mesa", 2f, -1f, 1.6f, 1.6f, "Mesa")),
            new Lot("Puesto de dosimetría", "Hospital", "Puesto de dosimetría y descontaminación", 12f, 10f, Panel,
                S("lavadero", -3f, 0f, 1.6f, 1f, "Ducha de descontaminación"), S("camilla", 3f, 0f, 1f, 2f, "Camilla")),
            new Lot("Parque de atracciones", "Mercado", "Parque de atracciones (la noria, sin estrenar)", 14f, 12f, Panel,
                S("puesto", 0f, 0f, 2f, 1f, "Taquilla cerrada")),
        }, new[]
        {
            new Lot("Bloque de viviendas", "Casa", "Bloque de Prípiat (evacuado)", 12f, 10f, Panel),
            new Lot("Tienda de Prípiat", "Mercado", "Tienda (cerrada)", 10f, 8f, Panel, S("puesto", 0f, 0f, 2f, 1f, "Mostrador vacío"), S("estante", 0f, 0f, 0.8f, 2f, "Estantería vacía"), S("armario", 0f, 0f, 1f, 0.6f, "Cajas sin abrir")),
            new Lot("Escuela de Prípiat", "Escuela", "Escuela (vacía)", 12f, 10f, Panel, S("pizarra", 0f, 0f, 1.6f, 0.4f, "Pizarra"), S("mesa", 0f, 0f, 1.6f, 1f, "Pupitres (vacíos)"), S("mesa", 0f, 0f, 1.6f, 1f, "Pupitres de los pequeños")),
        }, C(0.4f, 0.4f, 0.4f));
        int n = 0;
        n += W(t, "Bombero de la central", s[1], "", s[3], null, null);
        n += W(t, "Bombero de Prípiat", s[1], "", s[3], null, null);
        n += W(t, "Dosimetrista", s[4], "medico", s[3], null, null);
        n += W(t, "Miembro de la comisión", s[2], "sabio", s[3], null, null);
        n += W(t, "Cocinera de la cantina", s[3], "cocinero", s[3], null, null);
        // Una pareja de operadores que se quedó a trabajar (la ciudad está evacuada: el resto de casas, vacías).
        n += C2(t, "Operadora de la central", "Operador de la central", At(s, 6) ?? s[3], s[3], null);
        return n;
    }
}
