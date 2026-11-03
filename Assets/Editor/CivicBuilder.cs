using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS EDIFICIOS REALES DE CADA ERA en los microcosmos civilizados (docs/microcosmos-civilizaciones.md §8; Berón 2026-11-01:
/// «se deben crear edificaciones reales, debes investigar planos e imágenes para poder recrear las estructuras de la era e
/// historia, y deben ser interactuables, por ejemplo restaurantes o cocinas o dojos o salas de yoga o hospitales o jardines»).
/// Cada edificio es un <see cref="AreaFunctionSite"/> con una plantilla del REGISTRO (<see cref="AreaFunctions.Buildings"/>) y
/// sus muebles son <see cref="AreaFunctionStation"/> (interactuables): se reutilizan las funciones del santuario (el servicio de
/// la carta, pasar visita, la clase de yoga, el riego…) a la escala de los insectos-humanos (~1,7 m de pie: puertas de 2,4 m,
/// salas de 6–20 m). Los habita gente de la era con su JORNADA (<see cref="MicroJornada"/>) y familias de especies mezcladas que
/// tienen hijos (<see cref="FolkCouple"/>, <see cref="InsectGenealogy"/>).
/// Los edificios van alrededor del claro de la historia (sus estaciones están a menos de ~24 m del centro): a ±31 m en x y a
/// +36 / −26 m en z, sin tapar la entrada ni el portal (0, −14…−20).
/// </summary>
internal static partial class CivicBuilder
{
    internal enum Roof { None, Flat, Gable }

    internal sealed class Style
    {
        public Color wall, floor, roof; public Roof roofKind = Roof.Flat; public float wallH = 3.4f; public bool open; public bool columns;
        public Style(Color wall, Color floor, Color roof, Roof kind, float h = 3.4f) { this.wall = wall; this.floor = floor; this.roof = roof; roofKind = kind; wallH = h; }
    }

    // ── Estilos de cada era (colores de sus materiales reales) ──
    static Color C(float r, float g, float b) => new Color(r, g, b);
    static readonly Style TongLau = new Style(C(0.86f, 0.82f, 0.7f), C(0.55f, 0.5f, 0.45f), C(0.6f, 0.58f, 0.55f), Roof.Flat, 3.4f);          // Hong Kong: hormigón enlucido, azotea
    static readonly Style ChaChaanTeng = new Style(C(0.62f, 0.78f, 0.7f), C(0.85f, 0.85f, 0.8f), C(0.55f, 0.55f, 0.52f), Roof.Flat, 3.4f);  // azulejo verde claro y terrazo
    static readonly Style SeattleBrick = new Style(C(0.6f, 0.3f, 0.22f), C(0.45f, 0.32f, 0.22f), C(0.25f, 0.24f, 0.24f), Roof.Gable, 3.8f);
    static readonly Style Selimiye = new Style(C(0.92f, 0.84f, 0.6f), C(0.6f, 0.55f, 0.48f), C(0.7f, 0.35f, 0.25f), Roof.Gable, 4.2f);      // enlucido amarillo y teja
    static readonly Style FieldTimber = new Style(C(0.55f, 0.42f, 0.28f), C(0.45f, 0.38f, 0.3f), C(0.35f, 0.3f, 0.25f), Roof.Gable, 3f);
    static readonly Style DaytonBrick = new Style(C(0.62f, 0.32f, 0.24f), C(0.5f, 0.38f, 0.28f), C(0.3f, 0.28f, 0.27f), Roof.Flat, 6.5f);  // dos plantas, dos escaparates
    static readonly Style Clapboard = new Style(C(0.93f, 0.92f, 0.88f), C(0.5f, 0.4f, 0.3f), C(0.3f, 0.3f, 0.32f), Roof.Gable, 3.6f);     // casa de tablas blancas
    static readonly Style BurntBrick = new Style(C(0.66f, 0.38f, 0.26f), C(0.6f, 0.5f, 0.38f), C(0.55f, 0.45f, 0.3f), Roof.Flat, 3.2f);    // ladrillo cocido (Tilaurakot)
    static readonly Style Limestone = new Style(C(0.92f, 0.9f, 0.82f), C(0.8f, 0.76f, 0.66f), C(0.85f, 0.82f, 0.74f), Roof.Flat, 4.5f) { columns = true };
    static readonly Style MudThatch = new Style(C(0.62f, 0.5f, 0.36f), C(0.55f, 0.45f, 0.32f), C(0.75f, 0.65f, 0.38f), Roof.Gable, 2.8f);   // adobe y paja
    static readonly Style Pavilion = new Style(C(0.5f, 0.38f, 0.25f), C(0.7f, 0.6f, 0.45f), C(0.75f, 0.65f, 0.38f), Roof.Gable, 3f) { open = true };
    static readonly Style Sandbag = new Style(C(0.62f, 0.55f, 0.4f), C(0.33f, 0.27f, 0.2f), C(0.4f, 0.33f, 0.25f), Roof.Flat, 2.2f);       // refugio de trinchera

    /// <summary>Añade a la historia los edificios de su era (por su nombre de microcosmos) y aplica las REGLAS DE EDIFICACIÓN
    /// (docs/microcosmos-reglas-edificacion.md): lo que falte de las necesidades básicas va a unos aseos de la era en un solar libre,
    /// y la escena se valida (<see cref="MicroBuildRules.Validate"/>). Devuelve cuántos insectos-humanos puso.</summary>
    internal static int Civilize(Transform t, string display)
    {
        MicroEra me = t.root.GetComponentInChildren<MicroEra>(true);
        var era = Eras.OfMicrocosmos(display);
        Year = me != null ? me.year : era.HasValue ? era.Value.year : Eras.Present;
        // La REGIÓN (MicroStyles): con el año, el estilo de época y lugar de cada mueble; el cuerpo por defecto, el de un
        // insecto-humano (en un microcosmos solo hay insectos: InsectOnly). Los hogares ajustan luego cada mueble a su dueño.
        Region = me != null && !string.IsNullOrEmpty(me.region) ? me.region : MicroStyles.RegionOf(display).ToString();
        MicroFurniture.DefaultBody = MicroBody.OfSpecies("Ant", 1.75f);
        // EL PLANO DEL PUEBLO (2026-11-03, CivicBuilder.Plans): en una era avanzada, su paisaje y sus calles reales primero (los
        // solares los esquivan); luego los edificios de la historia en su sitio, los del pueblo y las casas a lo largo de las calles.
        _plan = null; _civicDone = _fillDone = false; _quarterAt = null;
        _planStyle = FillStyles.TryGetValue(display, out var fs0) ? fs0.st : null;
        if (MicroBuildRules.IsTownEra(Year))
        {
            Rect fl = AreaMicrocosmos.FloorRectLocal(t);
            _plan = PlanOf(display, fl);
            if (_plan != null) BuildPlan(t, _plan, fl, _planStyle != null ? Color.Lerp(_planStyle.floor, Color.gray, 0.4f) : C(0.5f, 0.48f, 0.44f));
        }
        int n = CivilizeEra(t, display);
        if (t.GetComponentInChildren<AreaFunctionSite>(true) == null) return n;   // una historia sin pueblo
        int moved = Settle(t);
        var lots = new Lots(t);
        int civic = PlanCivic(t, lots, _plan);
        // Regla 5: en las eras avanzadas el pueblo sigue por el piso (casas de la era, vacías de gente): con plano, a lo largo de sus
        // calles; sin él, alrededor del claro.
        int fill = 0;
        if (MicroBuildRules.IsTownEra(Year) && FillStyles.TryGetValue(display, out var fs))
        {
            if (_plan != null && !_fillDone) fill = PlanFill(t, lots, _plan, new[] { new Lot(fs.title, "Casa", fs.title, 12f, 10f, fs.st) }, _plan.fill).Count;
            else if (_plan == null) fill = FillTown(t, lots, Vector2.zero, fs.st, fs.title, lots.floor.width < 250f ? 4 : 10);   // (en un piso de 120 m, como el de Uruk en MobWorld, no caben 10 con sus frutales);
        }
        int homes = Homes(t, lots, display);   // regla 6: un hogar para cada personaje (las parejas, juntas; CivicBuilder.Homes)
        EnsureNeeds(t, lots, NeedsStyle);
        // (2026-11-03) Los hogares rehacen los muebles a la medida de cada familia y la casa crece (Relayout): si al crecer se acerca a
        // menos de una calle de otro edificio, se corre a un solar libre (la Higuera de una casa nueva del Huerto quedaba a 0,16 m
        // del banco de la casa redonda de al lado).
        moved += Settle(t);
        foreach (AreaFunctionSite s in t.GetComponentsInChildren<AreaFunctionSite>(true)) { s.bodies = ""; Tag(s); }   // las etiquetas, ya con los muebles rehechos a la medida de cada hogar
        if (moved > 0 || fill > 0 || homes > 0 || civic > 0) Debug.Log($"[Eras] {display}: {moved} edificios corridos a un solar libre (paso de {MicroBuildRules.Street:0} m entre edificios), {civic} edificios del pueblo, {fill} casas de relleno, {homes} casas nuevas para familias; {Count(t)} edificios en total.");
        MicroBuildRules.Validate(t, $"{display} ({Eras.Format(Year)})");
        return n;
    }

    /// <summary>Las casas con que se llena el resto del piso en cada era avanzada (regla 5): su material y su nombre.</summary>
    static readonly Dictionary<string, (Style st, string title)> FillStyles = new Dictionary<string, (Style, string)>
    {
        ["Escútari"] = (Selimiye, "Casa de Üsküdar"),
        ["Dayton"] = (Clapboard, "Casa de West Dayton"),
        ["Kapilavastu"] = (BurntBrick, "Casa de ladrillo de Kapilavastu"),
        ["Alejandría"] = (Limestone, "Casa del barrio Brucheion"),
        ["Patanjali"] = (MudThatch, "Cabaña de la aldea"),
        ["Planta Atómica"] = (StoneManor, "Casa de Woolsthorpe-by-Colsterworth"),
        ["Irena Sendler"] = (Sandstone, "Casa de vecinos de Varsovia"),
        ["Le Chambon-sur-Lignon"] = (StoneManor, "Casa de piedra del plateau"),
        ["Mesopotamia"] = (MudBrick, "Casa de adobe de Uruk"),
        ["Tregua 1914"] = (FieldTimber, "Granja de Ploegsteert (evacuada)"),
        ["Palenque"] = (MayaPlaster, "Casa de Lakamha'"),
        ["Egina"] = (GreekStone, "Casa de Egina"),
    };

    /// <summary>Cada EDIFICIO ABIERTO (los de <see cref="Building"/>, con su «Suelo») que choca con otro o con la historia (a menos
    /// de una calle) o se sale del piso se corre al solar libre más cercano, con su gente (las jornadas que trabajan allí).
    /// Devuelve cuántos movió.</summary>
    static int Settle(Transform t)
    {
        var sites = new List<AreaFunctionSite>();
        foreach (Transform c in t) { var s = c.GetComponent<AreaFunctionSite>(); if (s != null && c.Find("Suelo") != null) sites.Add(s); }
        var skip = new HashSet<Transform>(); foreach (var s in sites) skip.Add(s.transform);
        var lots = new Lots(t, skip);
        int moved = 0;
        foreach (AreaFunctionSite s in sites)
        {
            Transform f = s.transform.Find("Suelo");
            Vector2 size = new Vector2(f.localScale.x, f.localScale.z) + new Vector2(2f, 2f);
            float yaw = s.transform.localEulerAngles.y; if (Mathf.Abs(Mathf.DeltaAngle(yaw, 90f)) < 45f || Mathf.Abs(Mathf.DeltaAngle(yaw, 270f)) < 45f) size = new Vector2(size.y, size.x);
            Vector2 c0 = new Vector2(s.transform.localPosition.x, s.transform.localPosition.z);
            if (lots.Free(c0, size)) { lots.Mark(new Rect(c0 - size * 0.5f, size)); continue; }
            Vector3? at = lots.Take(size, c0);
            if (!at.HasValue) { lots.Mark(new Rect(c0 - size * 0.5f, size)); Debug.LogWarning($"[Eras] «{s.title}»: sin solar libre para correrlo."); continue; }
            Vector3 d = at.Value - s.transform.localPosition; d.y = 0f;
            s.transform.localPosition += d; moved++;
            foreach (MicroJornada j in t.GetComponentsInChildren<MicroJornada>(true)) if (j.work == s) j.transform.localPosition += d;
        }
        return moved;
    }

    static Style NeedsStyle => new Style(C(0.7f, 0.64f, 0.52f), C(0.62f, 0.57f, 0.48f), C(0.5f, 0.45f, 0.4f), Roof.None);

    /// <summary>
    /// LOS SOLARES LIBRES del piso de una historia (en los locales de <c>t</c>): el rectángulo útil
    /// (<see cref="AreaMicrocosmos.FloorRectLocal"/>) menos el claro de la historia (un círculo de <see cref="Clearing"/> m en
    /// (0, 0), con la entrada y el portal) y lo que ya hay construido (con una calle de <see cref="MicroBuildRules.Street"/>
    /// alrededor). <see cref="Take"/> da el centro del solar libre más cercano a un punto y lo marca ocupado.
    /// </summary>
    internal sealed class Lots
    {
        public const float Clearing = 24f;
        public readonly Rect floor;
        readonly List<Rect> _taken = new List<Rect>();
        readonly Transform _t;

        public Lots(Transform t, HashSet<Transform> skip = null)
        {
            _t = t; floor = AreaMicrocosmos.FloorRectLocal(t);
            // Lo construido: una caja por hijo de t (un edificio, una estación de la historia), sin la gente.
            foreach (Transform c in t)
            {
                if (skip != null && skip.Contains(c)) continue;
                // (2026-11-03) El PLANO del pueblo (TownPlans): sus calles y su paisaje (río, lago, colinas, campos) se pisan o se
                // ven, pero no se construye encima: cada tramo, su caja (no la del conjunto, que es el pueblo entero).
                if (c.name.StartsWith(PlanLandscape) || c.name.EndsWith(PlanStreets))
                {
                    foreach (Renderer r in c.GetComponentsInChildren<Renderer>(true))
                    {
                        Bounds rb = r.bounds;
                        Vector3 a = t.InverseTransformPoint(rb.min), z = t.InverseTransformPoint(rb.max);
                        Mark(Rect.MinMaxRect(Mathf.Min(a.x, z.x), Mathf.Min(a.z, z.z), Mathf.Max(a.x, z.x), Mathf.Max(a.z, z.z)));
                    }
                    continue;
                }
                bool any = false; Bounds b = default;
                foreach (Renderer r in c.GetComponentsInChildren<Renderer>(true))
                {
                    if (r.GetComponentInParent<CubeInsect>(true) != null) continue;
                    if (r.bounds.size.y < 0.1f) continue;   // lo que se pisa (calles, asfalto, agua pintada): se construye al lado o encima
                    if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
                }
                if (!any) continue;
                Vector3 mn = t.InverseTransformPoint(b.min), mx = t.InverseTransformPoint(b.max);
                Mark(Rect.MinMaxRect(Mathf.Min(mn.x, mx.x), Mathf.Min(mn.z, mx.z), Mathf.Max(mn.x, mx.x), Mathf.Max(mn.z, mx.z)));
            }
            // (2026-11-03) LOS MUEBLES DEL PISO (que ya no se quitan: el piso no se rompe): lo que apoya en el suelo (patas, costados,
            // encimeras, camas) es un solar ocupado; el pueblo lo rodea (o se sube encima: CivicBuilder.Outposts). Lo que queda muy por
            // encima (el tablero de una mesa) no estorba al suelo de debajo.
            foreach (Bounds fb in AreaMicrocosmos.FloorFurniture(t))
            {
                if (fb.min.y - t.position.y > 5f) continue;
                Vector3 a = t.InverseTransformPoint(fb.min), z = t.InverseTransformPoint(fb.max);
                Mark(Rect.MinMaxRect(Mathf.Min(a.x, z.x), Mathf.Min(a.z, z.z), Mathf.Max(a.x, z.x), Mathf.Max(a.z, z.z)));
            }
        }

        // (2026-11-03) Un índice por celdas de 40 m: con el barrio ×60 hay miles de solares y tramos de calle que mirar.
        readonly Dictionary<(int, int), List<int>> _grid = new Dictionary<(int, int), List<int>>();
        const float GridCell = 40f;
        static int G(float v) => Mathf.FloorToInt(v / GridCell);

        public void Mark(Rect r)
        {
            int id = _taken.Count; _taken.Add(r);
            for (int i = G(r.xMin); i <= G(r.xMax); i++) for (int k = G(r.yMin); k <= G(r.yMax); k++)
            { if (!_grid.TryGetValue((i, k), out var l)) _grid[(i, k)] = l = new List<int>(); l.Add(id); }
        }

        /// <summary>¿Cabe un solar de <paramref name="size"/> centrado en <paramref name="c"/> (dentro del piso, fuera del claro, a
        /// una calle de todo lo construido)?</summary>
        public bool Free(Vector2 c, Vector2 size)
        {
            Rect r = new Rect(c - size * 0.5f, size);
            if (r.xMin < floor.xMin || r.xMax > floor.xMax || r.yMin < floor.yMin || r.yMax > floor.yMax) return false;
            float dx = Mathf.Max(0f, Mathf.Max(r.xMin, -r.xMax)), dz = Mathf.Max(0f, Mathf.Max(r.yMin, -r.yMax));
            if (dx * dx + dz * dz < Clearing * Clearing) return false;
            float s = MicroBuildRules.Street;
            Rect g = new Rect(r.x - s, r.y - s, r.width + 2f * s, r.height + 2f * s);
            for (int i = G(g.xMin); i <= G(g.xMax); i++) for (int k = G(g.yMin); k <= G(g.yMax); k++)
                if (_grid.TryGetValue((i, k), out var l)) foreach (int id in l) if (g.Overlaps(_taken[id])) return false;
            return true;
        }

        /// <summary>El solar libre de <paramref name="size"/> más cercano a <paramref name="near"/> (por defecto, el claro), en
        /// una rejilla de 2 m; lo marca ocupado. null si no cabe.</summary>
        public Vector3? Take(Vector2 size, Vector2? near = null)
        {
            Vector2 n = near ?? Vector2.zero; Vector2? best = null; float bd = float.MaxValue;
            // (2026-11-03) En ANILLOS alrededor de «near» (rejilla de 2 m): con el barrio ×60 (hasta 2,8 km) recorrer el piso entero
            // eran millones de solares por casa. El primer anillo con sitio da el más cercano (se mira uno más por si la diagonal
            // del anterior queda más lejos).
            float x0 = floor.xMin + size.x * 0.5f, x1 = floor.xMax - size.x * 0.5f, z0 = floor.yMin + size.y * 0.5f, z1 = floor.yMax - size.y * 0.5f;
            if (x0 > x1 || z0 > z1) return null;
            Vector2 c0 = new Vector2(Mathf.Clamp(n.x, x0, x1), Mathf.Clamp(n.y, z0, z1));
            int maxR = Mathf.Min(Mathf.CeilToInt(Mathf.Max(floor.width, floor.height) / 2f) + 1, 350), foundAt = -1;   // hasta 700 m de «near»
            for (int ring = 0; ring <= maxR; ring++)
            {
                if (foundAt >= 0 && ring > foundAt + 1) break;
                for (int i = -ring; i <= ring; i++)
                    for (int k = -ring; k <= ring; k += (Mathf.Abs(i) == ring || ring == 0) ? 1 : 2 * ring)   // solo el borde del anillo
                    {
                        Vector2 c = new Vector2(c0.x + i * 2f, c0.y + k * 2f);
                        if (c.x < x0 || c.x > x1 || c.y < z0 || c.y > z1) continue;
                        float d = (c - n).sqrMagnitude;
                        if (d < bd && Free(c, size)) { bd = d; best = c; if (foundAt < 0) foundAt = ring; }
                    }
            }
            if (!best.HasValue) return null;
            Mark(new Rect(best.Value - size * 0.5f, size));
            return new Vector3(best.Value.x, 0f, best.Value.y);
        }
    }

    static int CivilizeEra(Transform t, string display)
    {
        switch (display)
        {
            case "Bruce Lee": return BruceLeeEra(t);
            case "Escútari": return Scutari(t);
            case "Dayton": return Dayton(t);
            case "Kapilavastu": return Kapilavastu(t);
            case "Alejandría": return Alexandria(t);
            case "Patanjali": return Ashram(t);
            case "Tregua 1914": return Trenches1914(t);
            case "Planta Atómica": return AtomicPlant(t);
            case "Irena Sendler": return Warsaw1942(t);
            case "Le Chambon-sur-Lignon": return LeChambon(t);
            case "Mesopotamia": return Uruk(t);
            case "Huerto": return FirstVillage(t);
            case "Crotona": return PythagorasTown(t);
            case "Menfis": return HerodotusTown(t);
            case "Bagdad": return AlKhwarizmiTown(t);
            case "Woolsthorpe": return NewtonTown(t);
            case "Berkeley": return JennerTown(t);
            case "Angostura": return BolivarTown(t);
            case "Sils Maria": return NietzscheTown(t);
            case "París 1898": return CurieTown(t);
            case "Colorado Springs": return TeslaTown(t);
            case "Berna": return EinsteinTown(t);
            case "Pasadena": return PattersonTown(t);
            case "Chernóbil": return ChernobylTown(t);
            case "Palenque": return PalenqueTown(t);
            case "Egina": return EginaTown(t);
            default: return 0;
        }
    }

    // ══ ESCÚTARI (1854–55): el cuartel de Selimiye (finales de la década de 1820): cuadrilátero con patio y torres en las
    //    esquinas, alas de tres plantas, salas-pasillo con camas a los dos lados; Nightingale vivía en una torre (hoy museo).
    //    La cocina de campaña de Alexis Soyer (1855) y la lavandería. (Wikipedia «Selimiye Barracks»; Royal Collection Trust:
    //    «The Military Hospital, Scutari» y «Miss Nightingale's Tower», J. Robertson 1855; Soyer 1857.)
    static int Scutari(Transform t)
    {
        var ward = Building(t, "Ala del cuartel de Selimiye (sala de camas)", "Hospital", "Hospital del cuartel de Selimiye", new Vector3(-33f, 0f, 8f), 0f, new Vector2(12f, 22f), Selimiye,
            S("cama", -4f, 7f, 1.2f, 2.2f, "Cama"), S("cama", 4f, 7f, 1.2f, 2.2f, "Cama"), S("cama", -4f, 2f, 1.2f, 2.2f, "Cama"), S("cama", 4f, 2f, 1.2f, 2.2f, "Cama"),
            S("cama", -4f, -3f, 1.2f, 2.2f, "Cama"), S("cama", 4f, -3f, 1.2f, 2.2f, "Cama"), S("mesa", 0f, 9.5f, 1.6f, 0.9f, "Mesa de los registros"),
            S("camilla", 0f, -7f, 1f, 2f, "Camilla de curas"), S("armario", 5f, -8.5f, 1.2f, 0.6f, "Armario de vendas"), S("lavadero", -5f, -8.5f, 1.4f, 0.8f, "Barreño"));
        Tower(t, "Torre de la esquina (la de Nightingale)", new Vector3(-33f, 0f, -8f), 6f, 13f, Selimiye);
        var kitchen = Building(t, "Cocina de campaña de Soyer (1855)", "Cocina", "Cocina de Alexis Soyer", new Vector3(32f, 0f, 8f), 0f, new Vector2(12f, 9f), FieldTimber,
            S("fogon", -3f, 2.5f, 1.4f, 1.4f, "Fogón de campaña de Soyer"), S("fogon", 1f, 2.5f, 1.4f, 1.4f, "Fogón de campaña de Soyer"), S("horno", 4.5f, 2.5f, 1.4f, 1.4f, "Horno"));
        var laundry = Building(t, "Lavandería del hospital", "Lavandería", "Lavandería (ropa de cama hervida)", new Vector3(32f, 0f, -24f), 0f, new Vector2(10f, 8f), FieldTimber,
            S("lavadero", -2.5f, 2f, 2f, 1f, "Caldera de lavar"), S("lavadero", 2.5f, 2f, 2f, 1f, "Pila"));
        var home = Building(t, "Barracones del personal", "Casa", "Barracones", new Vector3(0f, 0f, 38f), 0f, new Vector2(14f, 8f), Selimiye,
            S("cama", -4f, 2f, 1.2f, 2f, "Catre"), S("cama", 0f, 2f, 1.2f, 2f, "Catre"), S("cama", 4f, 2f, 1.2f, 2f, "Catre"), S("fogon", 5.5f, -2f, 1f, 0.8f, "Estufa"));
        int n = 0;
        n += Worker(t, "Enfermera Mary Clarke", ward, "enfermera", kitchen, null, home);
        n += Worker(t, "Enfermera de Escútari", ward, "enfermera", kitchen, null, home);
        n += Worker(t, "Cirujano del ejército", ward, "medico", kitchen, null, home);
        n += Worker(t, "Ayudante de Soyer", kitchen, "cocinero", kitchen, null, home);
        n += Worker(t, "Lavandera de Escútari", laundry, "", kitchen, null, home);
        n += Worker(t, "Lavandera de la ribera", laundry, "", kitchen, null, home);
        n += Couple(t, "Esposa del sargento", "Sargento convaleciente", home, kitchen, null);
        return n;
    }

    // ══ DAYTON (1892–1903): la Wright Cycle Company (1127 W. Third St.), edificio de ladrillo de DOS plantas y dos escaparates
    //    (en Greenfield Village desde 1937): tienda delante, taller al fondo (torno, motor), allí el túnel de viento de 1901. La casa
    //    de tablas de 7 Hawthorn St. y el cobertizo de Kitty Hawk (1902). (The Henry Ford, «Wright Cycle Shop… circa 1910»; NPS,
    //    Wright Cycle Company Historic Furnishings Report.)
    static int Dayton(Transform t)
    {
        var shop = Building(t, "Wright Cycle Company (1127 West Third Street)", "Taller", "Wright Cycle Company", new Vector3(-31f, 0f, 4f), 0f, new Vector2(9f, 18f), DaytonBrick,
            S("banco_taller", -2.5f, 5f, 2.4f, 0.9f, "Banco de reparar bicicletas"), S("banco_taller", 2.5f, 5f, 2.4f, 0.9f, "Banco de montar"),
            S("torno", 2.5f, 1f, 1.6f, 0.8f, "Torno"), S("tunel", -2f, 0f, 0.6f, 1.8f, "Túnel de viento (1901)"), S("mesa", 0f, -6f, 3f, 0.8f, "Mostrador de la tienda"));
        Upper(t, shop.transform, new Vector2(9f, 18f), DaytonBrick, "Imprenta (planta alta)");
        var home = Building(t, "Casa de los Wright (7 Hawthorn Street)", "Casa", "Casa de los Wright", new Vector3(31f, 0f, 4f), 0f, new Vector2(10f, 10f), Clapboard,
            S("cama", -3f, 3f, 1.2f, 2f, "Cama"), S("cama", 3f, 3f, 1.2f, 2f, "Cama"), S("fogon", 0f, -3.5f, 1.4f, 0.8f, "Cocina de leña"));
        var shed = Building(t, "Cobertizo de Kitty Hawk (1902)", "Taller", "Campamento de Kill Devil Hills", new Vector3(31f, 0f, -26f), 0f, new Vector2(14f, 8f), FieldTimber,
            S("banco_taller", -4f, 2f, 2.4f, 0.9f, "Banco del planeador"), S("banco_taller", 3f, 2f, 2.4f, 0.9f, "Banco del motor"));
        int n = 0;
        n += Worker(t, "Katharine Wright", home, "", home, null, home);
        n += Worker(t, "Aprendiz del taller", shop, "mecanico", home, null, home);
        n += Worker(t, "Mecánico de Dayton", shop, "mecanico", home, null, home);
        n += Worker(t, "Salvavidas de Kill Devil Hills", shed, "mecanico", home, null, home);
        n += Couple(t, "Vecina de West Third Street", "Vecino de West Third Street", home, home, null);
        return n;
    }

    // ══ KAPILAVASTU (s. V a. C.): Tilaurakot, ciudad en cuadrícula con casas y tiendas de LADRILLO, puertas a los puntos
    //    cardinales, muralla de ladrillo cocido sobre una empalizada de madera, estanques revestidos de ladrillo (Durham Univ.,
    //    proyecto Tilaurakot-Kapilavastu desde 2014; Wikipedia «Tilaurakot»).
    static int Kapilavastu(Transform t)
    {
        Wall(t, "Muralla de ladrillo cocido (puerta norte)", new Vector3(0f, 0f, 46f), 40f, 4f, BurntBrick, gate: 5f);
        var h1 = Building(t, "Casa de ladrillo (calle este-oeste)", "Casa", "Casa de Kapilavastu", new Vector3(-31f, 0f, 0f), 0f, new Vector2(10f, 8f), BurntBrick,
            S("cama", -2.5f, 2f, 1.2f, 2f, "Lecho"), S("fogon", 3f, 2.5f, 1.2f, 0.8f, "Hogar de barro"));
        var h2 = Building(t, "Casa de ladrillo (calle norte-sur)", "Casa", "Casa de Kapilavastu", new Vector3(-31f, 0f, 16f), 0f, new Vector2(10f, 8f), BurntBrick,
            S("cama", -2.5f, 2f, 1.2f, 2f, "Lecho"), S("fogon", 3f, 2.5f, 1.2f, 0.8f, "Hogar de barro"));
        var market = Building(t, "Calle de tiendas", "Mercado", "Mercado de Kapilavastu", new Vector3(31f, 0f, 4f), 0f, new Vector2(14f, 8f), BurntBrick,
            S("puesto", -4.5f, 0f, 2f, 1.2f, "Puesto de grano"), S("puesto", 0f, 0f, 2f, 1.2f, "Puesto de telas"), S("puesto", 4.5f, 0f, 2f, 1.2f, "Puesto de cerámica"));
        var garden = Building(t, "Huerto junto al estanque", "Huerto", "Huerto y estanque revestido de ladrillo", new Vector3(31f, 0f, -26f), 0f, new Vector2(14f, 10f), Pavilion,
            S("bancal", -4f, 1f, 3f, 5f, "Bancal de arroz"), S("bancal", 0.5f, 1f, 3f, 5f, "Bancal de legumbres"), S("lavadero", 5f, 0f, 2.5f, 6f, "Estanque de ladrillo"));
        int n = 0;
        n += Worker(t, "Alfarero de Kapilavastu", market, "comerciante", h1, null, h1);
        n += Worker(t, "Tejedora de Kapilavastu", market, "comerciante", h2, null, h2);
        n += Worker(t, "Campesino del Terai", garden, "hortelano", h1, null, h1);
        n += Couple(t, "Madre de Kapilavastu", "Padre de Kapilavastu", h2, h2, null);
        return n;
    }

    // ══ ALEJANDRÍA (~240 a. C.): el Museion y la Biblioteca (salas con columnas y armarios de rollos: sin planos; según
    //    Estrabón XVII.1.8, el Museion tenía paseo, exedra con asientos y una gran sala común para comer los sabios) y una casa
    //    griega con patio.
    static int Alexandria(Transform t)
    {
        var mus = Building(t, "Museion (sala de los rollos)", "Escuela", "Museion de Alejandría", new Vector3(-32f, 0f, 4f), 0f, new Vector2(16f, 12f), Limestone,
            S("estante", -6f, 4.8f, 3f, 0.8f, "Armario de rollos"), S("estante", -1.5f, 4.8f, 3f, 0.8f, "Armario de rollos"), S("estante", 3f, 4.8f, 3f, 0.8f, "Armario de rollos"),
            S("pizarra", 6.5f, 2f, 0.4f, 2f, "Tablilla de cera"), S("mesa", 0f, -1f, 3f, 1.2f, "Mesa de copia"));
        var dining = Building(t, "Sala común del Museion", "Restaurante", "Comedor de los sabios (Museion)", new Vector3(0f, 0f, 38f), 0f, new Vector2(14f, 9f), Limestone,
            S("fogon", -5f, 3f, 1.6f, 1f, "Hogar"), S("barra", 0f, 3.2f, 3f, 0.8f, "Aparador"), S("mesa", -3f, -1.5f, 2f, 1.2f, "Lecho de banquete"), S("mesa", 3f, -1.5f, 2f, 1.2f, "Lecho de banquete"));
        var home = Building(t, "Casa griega con patio", "Casa", "Casa de Alejandría", new Vector3(32f, 0f, 4f), 0f, new Vector2(12f, 12f), Limestone,
            S("cama", -4f, 4f, 1.2f, 2f, "Lecho"), S("fogon", 4f, 4f, 1.4f, 0.8f, "Hogar"), S("altar", 0f, 0f, 0.8f, 0.8f, "Altar del patio"));
        int n = 0;
        n += Worker(t, "Bibliotecario del Museion", mus, "sabio", dining, null, home);
        n += Worker(t, "Copista del Museion", mus, "sabio", dining, null, home);
        n += Worker(t, "Cocinero del Museion", dining, "cocinero", dining, null, home);
        n += Couple(t, "Madre alejandrina", "Padre alejandrino", home, dining, null);
        return n;
    }

    // ══ PATANJALI (s. II a. C.): el āśrama junto al río: cabañas de adobe y paja, el patio de práctica bajo cubierta, la cocina de
    //    leña y el huerto (Olivelle 1993, The Āśrama System; cuevas de meditación como las de Barabar, s. III a. C.).
    static int Ashram(Transform t)
    {
        var hut = Building(t, "Cabaña del āśrama", "Casa", "Cabaña del āśrama", new Vector3(-32f, 0f, 2f), 0f, new Vector2(9f, 7f), MudThatch,
            S("cama", -2f, 1.5f, 1.2f, 2f, "Estera de dormir"), S("cama", 2f, 1.5f, 1.2f, 2f, "Estera de dormir"));
        var yoga = Building(t, "Patio de práctica (pabellón)", "Sala de Yoga", "Patio de práctica del āśrama", new Vector3(32f, 0f, 4f), 0f, new Vector2(14f, 10f), Pavilion,
            S("esterilla", -4f, 2f, 1f, 2f, "Esterilla"), S("esterilla", 0f, 2f, 1f, 2f, "Esterilla"), S("esterilla", 4f, 2f, 1f, 2f, "Esterilla"), S("esterilla", 0f, -2.5f, 1f, 2f, "Esterilla del maestro"));
        var kitchen = Building(t, "Cocina de leña", "Cocina", "Cocina del āśrama", new Vector3(32f, 0f, -26f), 0f, new Vector2(9f, 7f), MudThatch,
            S("fogon", 0f, 1.5f, 1.6f, 1.2f, "Fogón de leña (chulha)"));
        var garden = Building(t, "Huerto del āśrama", "Huerto", "Huerto del āśrama", new Vector3(0f, 0f, 38f), 0f, new Vector2(16f, 9f), Pavilion,
            S("bancal", -5f, 0f, 3f, 5f, "Bancal"), S("bancal", 0f, 0f, 3f, 5f, "Bancal"), S("bancal", 5f, 0f, 3f, 5f, "Bancal"));
        int n = 0;
        n += Worker(t, "Discípula del āśrama", yoga, "alumno", kitchen, yoga, hut);
        n += Worker(t, "Discípulo del āśrama", garden, "hortelano", kitchen, yoga, hut);
        n += Worker(t, "Cocinera del āśrama", kitchen, "cocinero", kitchen, yoga, hut);
        n += Couple(t, "Madre del pueblo", "Padre del pueblo", hut, kitchen, yoga);
        return n;
    }

    // ══ TREGUA DE 1914: en el frente no hay edificios; hay refugios (dugouts) de tablas y sacos y la cocina de campaña
    //    (Imperial War Museums, «The Christmas Truce»).
    static int Trenches1914(Transform t)
    {
        var bri = Building(t, "Refugio británico (dugout)", "Trinchera", "Refugio británico", new Vector3(-30f, 0f, -12f), 0f, new Vector2(10f, 6f), Sandbag,
            S("parapeto", 0f, -2.2f, 6f, 0.6f, "Aspillera"), S("cama", -3f, 1.5f, 1.2f, 2f, "Litera"), S("fogon", 3f, 1.5f, 1f, 0.8f, "Brasero"));
        var ger = Building(t, "Refugio alemán (Unterstand)", "Trinchera", "Refugio alemán", new Vector3(-30f, 0f, 34f), 180f, new Vector2(10f, 6f), Sandbag,
            S("parapeto", 0f, -2.2f, 6f, 0.6f, "Aspillera"), S("cama", -3f, 1.5f, 1.2f, 2f, "Litera"), S("fogon", 3f, 1.5f, 1f, 0.8f, "Estufa"));
        var cook = Building(t, "Cocina de campaña", "Trinchera", "Cocina de campaña británica", new Vector3(30f, 0f, -12f), 0f, new Vector2(8f, 6f), FieldTimber,
            S("fogon", 0f, 1f, 2f, 1.2f, "Marmita de campaña"), S("cama", -2.5f, -1.2f, 1.2f, 2f, "Catre"));
        int n = 0;
        n += Worker(t, "Cocinero del batallón", cook, "", cook, null, bri, "Ant", new Color(0.55f, 0.22f, 0.12f));
        n += Worker(t, "Centinela británico", bri, "soldado", cook, null, bri, "Ant", new Color(0.55f, 0.22f, 0.12f));
        n += Worker(t, "Centinela alemán", ger, "soldado", ger, null, ger, "Ant", new Color(0.12f, 0.11f, 0.1f));
        return n;
    }

    // ── Piezas ──────────────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Un puesto: clave del registro, posición local (x, z) dentro del edificio, tamaño (ancho, fondo) y rótulo. Desde las
    /// reglas de edificación (docs/microcosmos-reglas-edificacion.md) la posición solo da el ORDEN (los muebles se colocan con el
    /// paso de 2 m) y el tamaño solo cuenta en lo que se pisa (tatami, esterilla, bancal).</summary>
    static (string key, Vector2 at, Vector2 size, string label) S(string key, float x, float z, float w, float d, string label) => (key, new Vector2(x, z), new Vector2(w, d), label);

    /// <summary>El año de la escena que se está civilizando (elige la versión de cada mueble: hogar o cocina de gas, letrina o retrete).</summary>
    internal static int Year = 1950;

    /// <summary>La región o cultura (MicroStyles.Region) de lo que se está construyendo: la del microcosmos (MicroEra.region o
    /// MicroStyles.RegionOf); un barrio de otro lugar (Seattle en el pueblo de Bruce Lee) la cambia mientras se levanta.</summary>
    internal static string Region = "";

    /// <summary>
    /// Un EDIFICIO ABIERTO (reglas de edificación 1–4): el suelo con el material real de la era (y un borde más oscuro), su
    /// rótulo y sus MUEBLES DETALLADOS (<see cref="MicroFurniture"/>), sin muros ni techo. Los puestos (<paramref name="stations"/>)
    /// son interactuables (<see cref="AreaFunctionStation"/> en la raíz de cada mueble) y se colocan en filas con
    /// <see cref="MicroBuildRules.Passage"/> entre ellos y al borde del suelo; el suelo crece si no caben en <paramref name="size"/>.
    /// Una Casa (o un orfanato) trae además sus necesidades básicas: cama, retrete, lavabo, mesa con sillas y cocina.
    /// Es un <see cref="AreaFunctionSite"/> de la plantilla <paramref name="template"/>.
    /// </summary>
    internal static AreaFunctionSite Building(Transform parent, string name, string template, string title, Vector3 pos, float yaw, Vector2 size, Style st,
                                              params (string key, Vector2 at, Vector2 size, string label)[] stations)
    {
        Transform b = new GameObject(name).transform; b.SetParent(parent, false); b.localPosition = pos;
        var pieces = Furnish(b, template, stations);
        Vector2 used = Layout(b, pieces, size, 0f);
        Floor(b, used, st);
        if (st.columns) for (int k = 0; k < 6; k++) AreaMicrocosmos.Box(b, "Columna", new Vector3(-used.x * 0.5f + 0.4f + k * (used.x - 0.8f) / 5f, st.wallH * 0.5f, -used.y * 0.5f - 0.6f), new Vector3(0.4f, st.wallH, 0.4f), AreaMicrocosmos.Mat("Civ_Muro_" + Hex(st.wall), st.wall), true);
        b.localRotation = Quaternion.Euler(0f, yaw, 0f);
        var site = b.gameObject.AddComponent<AreaFunctionSite>(); site.building = template; site.title = title; site.region = Region;
        Tag(site);
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = title; wl.height = 3.2f;
        return site;
    }

    /// <summary>Los muebles de un edificio (puestos + necesidades de una Casa), aún sin colocar.</summary>
    static List<Transform> Furnish(Transform b, string template, (string key, Vector2 at, Vector2 size, string label)[] stations)
    {
        var l = new List<Transform>();
        var have = new HashSet<string>();
        foreach (var s in stations)
        {
            Transform p = MicroFurniture.Build(b, s.key, Vector3.zero, 0f, s.label, Year, s.size, Region);
            if (!NotStation(s.key)) p.gameObject.AddComponent<AreaFunctionStation>().station = s.key;   // retrete, lavabo, banco y silla: muebles, no puestos
            var lab = p.gameObject.AddComponent<WorldLabel>(); lab.text = s.label; lab.height = 1.6f;
            l.Add(p); have.Add(s.key);
        }
        if (template == "Casa" || template == "Orfanato")
        {
            foreach (string need in new[] { "cama", "mesa", "fogon", "inodoro", "lavabo" })
            {
                if (have.Contains(need)) continue;
                l.Add(MicroFurniture.Build(b, need, Vector3.zero, 0f, NeedLabel(need, Year), Year, null, Region));
            }
            // REGLA 11 (Berón 2026-11-03: «las residencias del microcosmos deben tener plantas frutales»): su FRUTAL, de los que se
            // cultivaban en su época y lugar (MicroStyles.Fruits: el manzano de Woolsthorpe, el cacao de Palenque, el mango del Terai…).
            if (!have.Contains("frutal")) l.Add(FruitTree(b, b.name.GetHashCode()));
        }
        Adapt(b, l);
        return l;
    }

    /// <summary>El FRUTAL de una casa (MicroFurniture «frutal»), rotulado con su especie.</summary>
    internal static Transform FruitTree(Transform b, int seed)
    {
        MicroStyles.Fruit fr = MicroFurniture.FruitFor(MicroStyles.For(Year, Region), seed);
        Transform p = MicroFurniture.Build(b, "frutal", Vector3.zero, 0f, fr.name, Year, null, Region);
        var wl = p.gameObject.AddComponent<WorldLabel>(); wl.text = fr.name; wl.height = fr.height + 0.6f;
        return p;
    }

    /// <summary>
    /// REGLA 9 (2026-11-03): todo edificio está ADAPTADO A HUMANOS Y A INSECTOS-HUMANOS. Sus muebles se hacen para el cuerpo por
    /// defecto del microcosmos (MicroFurniture.DefaultBody: un insecto-humano centauro); si entre <paramref name="pieces"/> no hay
    /// ninguno a la medida de una persona, se le añade un banco para personas (y al revés, uno de abdomen para insectos-humanos).
    /// </summary>
    static void Adapt(Transform b, List<Transform> pieces)
    {
        bool human = false, insect = false;
        foreach (Transform p in pieces)
            foreach (MicroPiece mp in p.GetComponentsInChildren<MicroPiece>(true))
            {
                if (!mp.fitted) continue;
                if (string.IsNullOrEmpty(mp.fitFor)) human = true; else insect = true;
            }
        if (!human)
        {
            // Con mesa, uno de sus asientos se hace para una persona (la mesa no crece); sin ella, un banco para personas.
            Transform table = pieces.Find(p => p.TryGetComponent(out MicroPiece mp) && mp.kind == "mesa");
            MicroBody.Fit insectBody = MicroFurniture.DefaultBody ?? MicroBody.OfSpecies("Ant", 1.75f);
            if (table != null && insectBody.Insect) MicroFurniture.Refit(table, "mesa", Year, Region, insectBody, new List<MicroBody.Fit> { insectBody, MicroBody.Human() });
            else pieces.Add(MicroFurniture.Build(b, "banco", Vector3.zero, 0f, "Banco (para personas)", Year, null, Region, MicroBody.Human()));
        }
        if (!insect) pieces.Add(MicroFurniture.Build(b, "banco", Vector3.zero, 0f, "Asiento de abdomen (para insectos-humanos)", Year, null, Region, MicroBody.OfSpecies("Ant", 1.75f)));
    }

    /// <summary>
    /// LAS ETIQUETAS DE ACTIVIDAD de un edificio (regla 8, docs/microcosmos-actividades.md): su categoría (trabajo, descanso,
    /// recuperación u ocio: <see cref="AreaFunctionSite.ActivityOf"/>), si tiene puestos de trabajo, su capacidad (camas, asientos,
    /// puestos, sitio en lo que se pisa), la interacción que ofrece y los cuerpos para los que está adaptado. Es lo que el horario
    /// semanal lee para mandar a cada uno a trabajar, descansar, recuperarse o divertirse.
    /// </summary>
    internal static void Tag(AreaFunctionSite site, string interaction = null)
    {
        if (site == null) return;
        if (string.IsNullOrEmpty(site.activity)) site.activity = AreaFunctionSite.ActivityOf(site.building);
        if (string.IsNullOrEmpty(site.interaction)) site.interaction = interaction ?? AreaFunctionSite.InteractionOf(site.building);
        if (string.IsNullOrEmpty(site.region)) site.region = Region;
        int cap = 0; var bodies = new SortedSet<string>();
        foreach (MicroPiece mp in site.GetComponentsInChildren<MicroPiece>(true))
        {
            if (mp.transform.parent != null && mp.transform.parent.GetComponentInParent<MicroPiece>(true) != null) { if (mp.kind == "silla" || mp.kind == "banco") cap++; continue; }   // las sillas de una mesa: un comensal cada una
            if (mp.fitted) bodies.Add(string.IsNullOrEmpty(mp.fitFor) ? "Humano" : mp.fitFor);
            switch (mp.kind)
            {
                case "cama": case "camilla": case "cuna": case "silla": cap += 1; break;
                case "banco": cap += 2; break;
                case "tatami": case "esterilla": case "bancal": case "pista": case "piscina": case "anden": cap += mp.walkable ? 4 : 1; break;
                case "frutal": case "inodoro": case "lavabo": break;
                default: if (mp.GetComponent<AreaFunctionStation>() != null) cap += 1; break;
            }
        }
        site.capacity = Mathf.Max(1, cap);
        site.bodies = string.Join(", ", bodies);
        site.hasWork = false;
        foreach (var f in site.Functions(Year)) if (f.kind != AreaFunctions.Kind.Rest && f.kind != AreaFunctions.Kind.Dine && !(f.roles.Length == 1 && f.roles[0] == "todos")) { site.hasWork = true; break; }
    }

    /// <summary>El nombre de cada mueble de necesidad básica en el ESTILO de su época y lugar (<see cref="Region"/>, MicroStyles) y
    /// para el cuerpo por defecto: «Charpai (de vientre)», «Kliné», «Letrina a la turca (de abdomen)», «Aguamanil»…</summary>
    internal static string NeedLabel(string key, int year) => MicroFurniture.NameOf(key, MicroStyles.For(year, Region));
    /// <summary>
    /// Coloca <paramref name="pieces"/> (hijos de <paramref name="b"/>) en filas: <see cref="MicroBuildRules.Passage"/> entre
    /// ellos y al borde. Las filas son de ancho <paramref name="minSize"/>.x (o el del mueble más ancho) y se centran; el conjunto
    /// se centra en z y se corre <paramref name="zOffset"/>. Devuelve el tamaño del suelo (≥ <paramref name="minSize"/>).
    /// </summary>
    internal static Vector2 Layout(Transform b, List<Transform> pieces, Vector2 minSize, float zOffset)
    {
        float P = MicroBuildRules.Passage;
        var boxes = new List<Bounds>();
        float widest = 0f;
        foreach (Transform p in pieces) { Bounds lb = MicroFurniture.LocalBounds(p); boxes.Add(lb); widest = Mathf.Max(widest, lb.size.x); }
        float rowMax = Mathf.Max(minSize.x - 2f * P, widest);
        var rows = new List<List<int>>(); var row = new List<int>(); float x = 0f;
        for (int i = 0; i < pieces.Count; i++)
        {
            float w = boxes[i].size.x;
            if (row.Count > 0 && x + w > rowMax + 0.01f) { rows.Add(row); row = new List<int>(); x = 0f; }
            row.Add(i); x += w + P;
        }
        if (row.Count > 0) rows.Add(row);
        float usedD = 0f, usedW = 0f;
        var depth = new List<float>();
        foreach (var r in rows)
        {
            float d = 0f, w = -P; foreach (int i in r) { d = Mathf.Max(d, boxes[i].size.z); w += boxes[i].size.x + P; }
            depth.Add(d); usedD += d; usedW = Mathf.Max(usedW, w);
        }
        usedD += P * Mathf.Max(0, rows.Count - 1);
        float z = usedD * 0.5f;   // de atrás (+z) hacia delante (−z)
        for (int k = 0; k < rows.Count; k++)
        {
            float w = -P; foreach (int i in rows[k]) w += boxes[i].size.x + P;
            float cx = -w * 0.5f;
            foreach (int i in rows[k])
            {
                Bounds lb = boxes[i];
                pieces[i].localPosition = new Vector3(cx + lb.size.x * 0.5f - lb.center.x, 0f, z - depth[k] * 0.5f - lb.center.z + zOffset);
                cx += lb.size.x + P;
            }
            z -= depth[k] + P;
        }
        return new Vector2(Mathf.Max(minSize.x, usedW + 2f * P), Mathf.Max(minSize.y, usedD + 2f * P));
    }

    // El SUELO de un edificio abierto (la losa con el material de la era y un borde un poco más oscuro, plano: se pisa).
    static void Floor(Transform b, Vector2 size, Style st)
    {
        Material floor = AreaMicrocosmos.Mat("Civ_Suelo_" + Hex(st.floor), st.floor);
        Color edge = Color.Lerp(st.wall, Color.black, 0.25f); edge.a = 1f;
        Material trim = AreaMicrocosmos.Mat("Civ_Borde_" + Hex(edge), edge);
        AreaMicrocosmos.Box(b, "Suelo", new Vector3(0f, 0.04f, 0f), new Vector3(size.x, 0.08f, size.y), floor, false);
        for (int k = -1; k <= 1; k += 2)
        {
            AreaMicrocosmos.Box(b, "Borde_del_suelo", new Vector3(0f, 0.09f, k * (size.y * 0.5f - 0.15f)), new Vector3(size.x, 0.02f, 0.3f), trim, false);
            AreaMicrocosmos.Box(b, "Borde_del_suelo", new Vector3(k * (size.x * 0.5f - 0.15f), 0.09f, 0f), new Vector3(0.3f, 0.02f, size.y), trim, false);
        }
    }

    // La planta alta (la imprenta de los Wright): una losa y un rótulo, sin entrar.
    static void Upper(Transform t, Transform b, Vector2 size, Style st, string label)
    {
        Transform u = new GameObject(label).transform; u.SetParent(b, false); u.localPosition = new Vector3(0f, st.wallH * 0.55f, 0f);
        var wl = u.gameObject.AddComponent<WorldLabel>(); wl.text = label; wl.height = 1f;
    }

    // Una torre de esquina (Selimiye), ABIERTA (regla 2): su planta y cuatro pilares en las esquinas, sin muros ni chapitel.
    static void Tower(Transform t, string name, Vector3 pos, float side, float h, Style st)
    {
        Transform b = new GameObject(name).transform; b.SetParent(t, false); b.localPosition = pos;
        Material wall = AreaMicrocosmos.Mat("Civ_Muro_" + Hex(st.wall), st.wall);
        Floor(b, new Vector2(side, side), st);
        foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) }) AreaMicrocosmos.Box(b, "Pilar", new Vector3(x * (side * 0.5f - 0.3f), h * 0.5f, z * (side * 0.5f - 0.3f)), new Vector3(0.5f, h, 0.5f), wall, true);
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = name; wl.height = h + 2f;
    }

    // Un tramo de muralla con su puerta (Tilaurakot), como hoy: el cimiento bajo de ladrillo (0,5 m, no tapa la vista) y los
    // postes de la puerta.
    static void Wall(Transform t, string name, Vector3 pos, float len, float h, Style st, float gate)
    {
        Transform b = new GameObject(name).transform; b.SetParent(t, false); b.localPosition = pos;
        Material wall = AreaMicrocosmos.Mat("Civ_Muro_" + Hex(st.wall), st.wall);
        float side = (len - gate) * 0.5f;
        AreaMicrocosmos.Box(b, "Cimiento_de_la_muralla", new Vector3(-(gate + side) * 0.5f, 0.25f, 0f), new Vector3(side, 0.5f, 1.6f), wall, true);
        AreaMicrocosmos.Box(b, "Cimiento_de_la_muralla", new Vector3((gate + side) * 0.5f, 0.25f, 0f), new Vector3(side, 0.5f, 1.6f), wall, true);
        foreach (float x in new[] { -gate * 0.5f - 0.3f, gate * 0.5f + 0.3f }) AreaMicrocosmos.Box(b, "Poste_de_la_puerta", new Vector3(x, h * 0.5f, 0f), new Vector3(0.5f, h, 0.5f), wall, true);
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = name; wl.height = h + 2f;
    }

    /// <summary>
    /// Regla 4 a escala del pueblo: si en <paramref name="t"/> falta alguna necesidad básica (dormir, aseo, lavarse, comer,
    /// sentarse, cocinar), unos ASEOS Y COMEDOR DE LA ERA abiertos (letrina o retrete, palangana o lavabo, mesa con sillas,
    /// cama, cocina: lo que falte) en <paramref name="at"/>. Devuelve el sitio o null si no faltaba nada.
    /// </summary>
    internal static AreaFunctionSite EnsureNeeds(Transform t, Lots lots, Style st)
    {
        var missing = new List<(string key, Vector2 at, Vector2 size, string label)>();
        foreach (var (need, key) in new[] { ("aseo", "inodoro"), ("lavarse", "lavabo"), ("dormir", "cama"), ("comer", "mesa"), ("sentarse", "mesa"), ("cocinar", "fogon") })
            if (!MicroBuildRules.Covers(t, need) && !missing.Exists(m => m.key == key)) missing.Add(S(key, 0f, 0f, 1f, 1f, NeedLabel(key, Year)));
        if (missing.Count == 0) return null;
        Vector3? lot = lots.Take(new Vector2(18f, 14f));
        if (!lot.HasValue) { Debug.LogWarning($"[Eras] Faltan necesidades básicas ({missing.Count}) y no queda un solar libre de 18 × 14 m para sus aseos."); return null; }
        Vector3 at = lot.Value;
        // Un sitio «Casa» del registro (descansar y cocinar se hacen aquí); su primer puesto es la mesa o la cama si las hay.
        missing.Sort((a, b2) => Rank(a.key).CompareTo(Rank(b2.key)));
        Transform b = new GameObject(Year < 1880 ? "Letrinas y pozo del barrio" : "Aseos y comedor del barrio").transform; b.SetParent(t, false); b.localPosition = at;
        var pieces = new List<Transform>();
        foreach (var m in missing)
        {
            Transform p = MicroFurniture.Build(b, m.key, Vector3.zero, 0f, m.label, Year, null, Region);
            if (m.key == "cama" || m.key == "fogon") p.gameObject.AddComponent<AreaFunctionStation>().station = m.key;   // las funciones de la Casa
            pieces.Add(p);
        }
        // Un sitio necesita un puesto con funciones: si solo faltaban retrete y lavabo, una cama de descanso.
        bool anyStation = false; foreach (Transform p in pieces) if (p.GetComponent<AreaFunctionStation>() != null) anyStation = true;
        if (!anyStation) { Transform p = MicroFurniture.Build(b, "cama", Vector3.zero, 0f, NeedLabel("cama", Year), Year, null, Region); p.gameObject.AddComponent<AreaFunctionStation>().station = "cama"; pieces.Insert(0, p); }
        Adapt(b, pieces);
        Vector2 used = Layout(b, pieces, new Vector2(14f, 8f), 0f);
        Floor(b, used, st);
        var site = b.gameObject.AddComponent<AreaFunctionSite>(); site.building = "Casa"; site.title = b.name; site.region = Region;
        Tag(site);
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = b.name; wl.height = 3f;
        return site;
    }

    /// <summary>Muebles de las necesidades básicas que no son puestos del registro (no tienen función propia).</summary>
    internal static bool NotStation(string key) => key == "inodoro" || key == "lavabo" || key == "banco" || key == "silla";

    static int Rank(string key) => key == "cama" ? 0 : key == "fogon" ? 1 : key == "mesa" ? 2 : 3;


    // Un insecto-humano de la era con su jornada (trabajo, comida, práctica, casa).
    static int Worker(Transform t, string name, AreaFunctionSite work, string role, AreaFunctionSite dine, AreaFunctionSite practice, AreaFunctionSite home, string species = null, Color? body = null, bool? female = null)
    {
        // Delante de su primer puesto, en el sitio de trabajo (MicroBuildRules.WorkSpot, dentro del paso de 2 m: no pisa otro mueble).
        AreaFunctionStation[] sts = work.Stations;
        Vector3 at;
        if (sts.Length > 0)
        {
            BoxCollider bc = sts[0].GetComponent<BoxCollider>();
            float front = bc != null ? bc.center.z - bc.size.z * 0.5f : -0.6f;
            at = t.InverseTransformPoint(sts[0].transform.TransformPoint(new Vector3(bc != null ? bc.center.x : 0f, 0f, front - MicroBuildRules.WorkSpot)));
        }
        else at = t.InverseTransformPoint(work.transform.TransformPoint(new Vector3(0f, 0f, -2f)));
        at.y = 0f;
        GameObject g = AreaMicrocosmos.Folk(t, name, at, Random.Range(1.6f, 1.85f), 1f, species, body, female);
        CubeInsect.WearShirt(g, Color.HSVToRGB(Mathf.Abs(name.GetHashCode() % 100) / 100f, 0.35f, 0.75f));
        var j = g.AddComponent<MicroJornada>(); j.work = work; j.role = role; j.dine = dine; j.practice = practice; j.home = home;
        return 1;
    }

    // Una PAREJA de especies distintas (con abuelos de otras especies), que tendrá hijos: el árbol que se mezcla.
    static int Couple(Transform t, string she, string he, AreaFunctionSite home, AreaFunctionSite dine, AreaFunctionSite practice)
    {
        Worker(t, she, home, "", dine, practice, home, female: true); Worker(t, he, home, "", dine, practice, home, female: false);
        FolkLineage a = null, b = null;
        foreach (FolkLineage l in t.GetComponentsInChildren<FolkLineage>(true)) { if (l.name == she) a = l; if (l.name == he) b = l; }
        if (a == null || b == null) return 2;
        var rnd = new System.Random(she.GetHashCode());
        string R() => InsectCast.Folk[rnd.Next(InsectCast.Folk.Length)];
        foreach (FolkLineage l in new[] { a, b })
        {
            l.ancestors = new List<InsectGenealogy.Ancestor> { new InsectGenealogy.Ancestor("madre", R(), 1), new InsectGenealogy.Ancestor("padre", R(), 1),
                new InsectGenealogy.Ancestor("abuela", R(), 2), new InsectGenealogy.Ancestor("abuelo", R(), 2), new InsectGenealogy.Ancestor("bisabuelo", R(), 3) };
        }
        a.female = true; b.female = false; a.partner = b; b.partner = a;
        if (a.TryGetComponent(out PersonLife pa) && b.TryGetComponent(out PersonLife pb)) { pa.partner = pb; pb.partner = pa; pa.courtship = pb.courtship = 1f; pa.female = true; pb.female = false; }
        a.gameObject.AddComponent<FolkCouple>();
        return 2;
    }

    static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
}
