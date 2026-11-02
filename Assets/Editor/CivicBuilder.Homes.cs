using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// REGLA 6 DE EDIFICACIÓN: UN HOGAR PARA CADA PERSONAJE (docs/microcosmos-reglas-edificacion.md §6; Berón 2026-11-02: «los pueblos
/// de historia de los microcosmos deben ser reconstruidos para que haya hogares para los demás personajes, además de los lugares
/// de trabajo y los servicios básicos, para que cada personaje realice su ciclo de vida correctamente»).
/// Tras el constructor de cada era o pueblo (y sus casas de relleno), <see cref="Homes"/> reparte a TODOS los insectos-humanos del
/// pueblo en FAMILIAS (una pareja comparte casa; los solos, la suya) y a cada familia le da un hogar (<see cref="Household"/>):
///   1. una casa que ya hay en el pueblo (un edificio con cama, retrete, lavabo y mesa con sillas: su casa de la historia, una de
///      relleno) si le quedan camas libres para todos;
///   2. si no, una CASA NUEVA de la era en un solar libre cerca de su trabajo (abierta, con los muebles detallados de
///      <see cref="MicroFurniture"/> y el paso de <see cref="MicroBuildRules.Passage"/>), con una cama por persona y, para una
///      pareja, dos más para los hijos que vendrán.
/// Cada hogar lleva su despensa (de la que se come en casa). La jornada (<see cref="MicroJornada.home"/>) va a esa casa.
/// </summary>
internal static partial class CivicBuilder
{
    /// <summary>Camas de más en la casa nueva de una pareja (los hijos que tendrá: FolkCouple.maxChildren).</summary>
    const int ChildBeds = 2;

    /// <summary>Da hogar a todo insecto-humano de <paramref name="t"/> que no lo tenga. Devuelve cuántas casas nuevas levantó.</summary>
    internal static int Homes(Transform t, Lots lots, string display)
    {
        var used = new HashSet<Transform>();
        foreach (Household h in t.GetComponentsInChildren<Household>(true)) foreach (Transform b in h.beds) if (b != null) used.Add(b);

        // Las FAMILIAS: cada pareja junta (ella primero), los demás solos; quien ya tiene hogar, no.
        var families = new List<List<GameObject>>();
        var seen = new HashSet<GameObject>();
        foreach (FolkLineage f in t.GetComponentsInChildren<FolkLineage>(true))
        {
            GameObject g = f.gameObject;
            if (seen.Contains(g) || HasHome(g)) continue;
            var fam = new List<GameObject> { g }; seen.Add(g);
            GameObject mate = f.partner != null ? f.partner.gameObject : g.TryGetComponent(out PersonLife pl) && pl.partner != null ? pl.partner.gameObject : null;
            if (mate != null && !seen.Contains(mate) && !HasHome(mate)) { seen.Add(mate); if (f.female) fam.Add(mate); else fam.Insert(0, mate); }
            families.Add(fam);
        }
        if (families.Count == 0) return 0;

        var dwellings = new List<AreaFunctionSite>();
        foreach (AreaFunctionSite s in t.GetComponentsInChildren<AreaFunctionSite>(true)) if (IsDwelling(s)) dwellings.Add(s);
        var (st, title) = HomeStyle(display);
        int built = 0, reused = 0;
        foreach (var fam in families)
        {
            bool couple = fam.Count > 1;
            AreaFunctionSite own = Jornada(fam[0])?.home;
            // 1 · Una casa que ya hay: la suya (la de su jornada) o la más cercana a ella, con camas libres para todos.
            AreaFunctionSite site = null;
            if (own != null && dwellings.Contains(own) && FreeBeds(own, used).Count >= fam.Count) site = own;
            if (site == null)
            {
                float bd = float.MaxValue; Vector3 from = own != null ? own.transform.position : fam[0].transform.position;
                foreach (AreaFunctionSite d in dwellings)
                {
                    if (FreeBeds(d, used).Count < fam.Count + (couple ? 1 : 0)) continue;   // una pareja, con sitio para un hijo
                    float dd = (d.transform.position - from).sqrMagnitude;
                    if (dd < bd) { bd = dd; site = d; }
                }
            }
            if (site != null) reused++;
            // 2 · Una casa nueva en un solar libre, cerca de su trabajo.
            if (site == null)
            {
                // En la región de su trabajo (el barrio de Seattle en el pueblo de Bruce Lee) o, si no, la del pueblo.
                string region0 = Region; AreaFunctionSite work0 = Jornada(fam[0])?.work;
                if (work0 != null && !string.IsNullOrEmpty(work0.region)) Region = work0.region;
                site = NewHouse(t, lots, fam, st, title, couple ? fam.Count + ChildBeds : fam.Count);
                Region = region0;
                if (site == null) { Debug.LogWarning($"[Hogares] {display}: sin solar libre para la casa de «{fam[0].name}»."); continue; }
                dwellings.Add(site); built++;
            }
            MakeHousehold(site, fam, used);
        }
        Debug.Log($"[Hogares] {display}: {families.Count} familias con hogar ({reused} en casas del pueblo, {built} casas nuevas).");
        return built;
    }

    static bool HasHome(GameObject g) => g.TryGetComponent(out PersonLife pl) && pl.home != null;
    static MicroJornada Jornada(GameObject g) => g.GetComponent<MicroJornada>();

    // Un EDIFICIO DONDE SE VIVE: con cama (no camilla), retrete, lavabo y mesa (con sus sillas o bancos).
    static bool IsDwelling(AreaFunctionSite s)
    {
        if (s == null) return false;
        bool bed = false, wc = false, wash = false, table = false;
        foreach (MicroPiece p in s.GetComponentsInChildren<MicroPiece>(true))
        {
            if (p.kind == "cama") bed = true;
            else if (p.kind == "inodoro") wc = true;
            else if (p.kind == "lavabo") wash = true;
            else if (p.kind == "mesa") table = true;
        }
        return bed && wc && wash && table;
    }

    static List<Transform> FreeBeds(AreaFunctionSite s, HashSet<Transform> used)
    {
        var l = new List<Transform>();
        foreach (MicroPiece p in s.GetComponentsInChildren<MicroPiece>(true)) if (p.kind == "cama" && !used.Contains(p.transform)) l.Add(p.transform);
        return l;
    }

    static (Style st, string title) HomeStyle(string display)
    {
        if (display != null && FillStyles.TryGetValue(display, out var fs)) return fs;
        if (Year < -2000) return (MudThatch, "Casa de la aldea");
        if (Year < 1850) return (FieldTimber, "Casa");
        return (Clapboard, "Casa");
    }

    /// <summary>Una CASA de la era para una familia (abierta, regla 2; muebles detallados, regla 3; con sus necesidades básicas,
    /// regla 4: <see cref="Building"/> con la plantilla «Casa» le añade mesa con sillas, cocina, retrete y lavabo) con
    /// <paramref name="beds"/> camas, en el solar libre más cercano a su trabajo. Pública para las escenas de ciclo de vida.</summary>
    internal static AreaFunctionSite NewHouse(Transform t, Lots lots, List<GameObject> fam, Style st, string title, int beds)
    {
        string who = fam.Count > 1 ? $"familia de {First(fam[0].name)} y {First(fam[1].name)}" : First(fam[0].name);
        var stations = new (string, Vector2, Vector2, string)[Mathf.Max(1, beds)];
        for (int i = 0; i < stations.Length; i++) stations[i] = S("cama", 0f, 0f, 1f, 2f, NeedLabel("cama", Year));
        AreaFunctionSite h = Building(t, $"{title} ({who})", "Casa", $"{title} de {who}", Vector3.zero, 0f, new Vector2(14f, 10f), st, stations);   // (14 m: dos muebles por fila, una casa compacta)
        if (lots == null) return h;
        Transform f = h.transform.Find("Suelo");
        AreaFunctionSite work = Jornada(fam[0])?.work;
        Vector3 near = work != null ? work.transform.localPosition : fam[0].transform.localPosition;
        Vector3? at = lots.Take(new Vector2(f.localScale.x + 2f, f.localScale.z + 2f), new Vector2(near.x, near.z));
        if (!at.HasValue) { Object.DestroyImmediate(h.gameObject); return null; }
        h.transform.localPosition = at.Value;
        return h;
    }

    /// <summary>El HOGAR de una familia en el edificio <paramref name="site"/>: sus camas, el retrete, el lavabo, la mesa con sus
    /// asientos y la despensa; la jornada de cada uno va a esa casa. Los muebles se REHACEN A LA MEDIDA de cada uno
    /// (docs/microcosmos-reglas-edificacion.md, regla 7; <see cref="MicroBody"/>): su cama para su cuerpo (de vientre, nido,
    /// percha, hamaca de seda…), un asiento a la mesa para cada uno, y el retrete y el lavamanos para todos (los de la primera
    /// familia de la casa), en el estilo de su época y lugar; luego se recolocan con el paso de 2 m.</summary>
    internal static Household MakeHousehold(AreaFunctionSite site, List<GameObject> fam, HashSet<Transform> used = null)
    {
        bool firstHome = site.GetComponentsInChildren<Household>(true).Length == 0;
        var go = new GameObject($"Hogar ({(fam.Count > 1 ? $"{First(fam[0].name)} y {First(fam[1].name)}" : First(fam[0].name))})");
        go.transform.SetParent(site.transform, false);
        var h = go.AddComponent<Household>();
        h.family = fam.Count > 1 ? $"Familia de {fam[0].name} y {fam[1].name}" : fam[0].name;
        var free = FreeBeds(site, used ?? new HashSet<Transform>());
        // Una cama por persona y las libres que queden de las que se pidieron para los hijos (en una casa nueva, todas).
        int want = Mathf.Min(free.Count, Mathf.Max(fam.Count, fam.Count > 1 ? fam.Count + ChildBeds : fam.Count));
        for (int i = 0; i < want; i++) { h.beds.Add(free[i]); used?.Add(free[i]); }

        // A LA MEDIDA: el cuerpo de cada uno (los hijos, en las camas de más, el de sus padres a la vez: MicroBody.Union).
        var bodies = fam.ConvertAll(MicroBody.Of);
        MicroBody.Fit all = MicroBody.Union(bodies);
        string region = string.IsNullOrEmpty(site.region) ? Region : site.region;
        MicroStyles.Entry style = MicroStyles.For(Year, region);
        for (int i = 0; i < h.beds.Count; i++)
        {
            MicroBody.Fit b = i < bodies.Count ? bodies[i] : all;
            MicroFurniture.Refit(h.beds[i], "cama", Year, region, b);
            string label = $"{MicroFurniture.NameOf("cama", style, b)} de {(i < fam.Count ? First(fam[i].name) : "los hijos")}";
            h.beds[i].name = label;
            if (h.beds[i].TryGetComponent(out WorldLabel wl)) wl.text = label;
        }
        if (firstHome)
            foreach (MicroPiece p in site.GetComponentsInChildren<MicroPiece>(true))
            {
                if (p == null || p.transform.parent != site.transform) continue;   // los muebles de la casa (las sillas de una mesa rehecha ya no están)
                if (p.kind == "inodoro") MicroFurniture.Refit(p.transform, "inodoro", Year, region, all);
                else if (p.kind == "lavabo") MicroFurniture.Refit(p.transform, "lavabo", Year, region, all);
                else if (p.kind == "mesa") MicroFurniture.Refit(p.transform, "mesa", Year, region, bodies[0], bodies);
            }
        Relayout(site);

        foreach (MicroPiece p in site.GetComponentsInChildren<MicroPiece>(true))
        {
            if (p.kind == "inodoro" && h.toilet == null) h.toilet = p.transform;
            else if (p.kind == "lavabo" && h.washbasin == null) h.washbasin = p.transform;
            else if (p.kind == "mesa" && h.table == null)
            {
                h.table = p.transform;
                foreach (MicroPiece c in p.GetComponentsInChildren<MicroPiece>(true)) if (c != p && (c.kind == "silla" || c.kind == "banco")) h.seats.Add(c.transform);
            }
        }
        // La despensa, junto a la mesa (sin malla: no es un mueble más para el paso de 2 m).
        var pantry = new GameObject("Despensa").transform; pantry.SetParent(go.transform, false);
        pantry.position = h.table != null ? h.table.position : site.transform.position;
        var fc = pantry.gameObject.AddComponent<FoodContainer>(); fc.dishName = "comida de casa"; fc.rations = 3 * fam.Count; fc.capacity = 20;
        h.pantry = fc;
        foreach (GameObject m in fam)
        {
            h.members.Add(m);
            if (m.TryGetComponent(out PersonLife pl)) pl.home = h;
            MicroJornada j = m.GetComponent<MicroJornada>(); if (j != null) j.home = site;
        }
        return h;
    }

    /// <summary>Tras rehacer muebles a la medida: los muebles del edificio (sus MicroPiece de primer nivel, en su orden) se
    /// vuelven a colocar en filas con el paso de 2 m (<see cref="Layout"/>) y el suelo crece si hace falta (nunca encoge). Los
    /// edificios sin «Suelo» (la casa redonda de la aldea, colocada a mano) se quedan como están.</summary>
    static void Relayout(AreaFunctionSite site)
    {
        Transform b = site.transform, floor = b.Find("Suelo");
        if (floor == null) return;
        var pieces = new List<Transform>();
        foreach (Transform c in b) if (c.GetComponent<MicroPiece>() != null) pieces.Add(c);
        if (pieces.Count == 0) return;
        Vector2 used = Layout(b, pieces, new Vector2(floor.localScale.x, floor.localScale.z), 0f);
        floor.localScale = new Vector3(used.x, floor.localScale.y, used.y);
        foreach (Transform c in b)
        {
            if (c.name != "Borde_del_suelo") continue;
            Vector3 p = c.localPosition, s = c.localScale;
            if (s.x >= s.z) { c.localScale = new Vector3(used.x, s.y, s.z); c.localPosition = new Vector3(p.x, p.y, Mathf.Sign(p.z) * (used.y * 0.5f - 0.15f)); }
            else { c.localScale = new Vector3(s.x, s.y, used.y); c.localPosition = new Vector3(Mathf.Sign(p.x) * (used.x * 0.5f - 0.15f), p.y, p.z); }
        }
    }

    /// <summary>
    /// LA CASA DE LAS ESCENAS DE CICLO DE VIDA DE UNA PERSONA (SampleSceneBuilder.CicloHumano): la misma casa que reciben las
    /// familias de los pueblos (<see cref="NewHouse"/> + <see cref="MakeHousehold"/>), en el origen de <paramref name="t"/>, con camas
    /// para la pareja y sus hijos y la despensa INAGOTABLE. Humanos: una casita de tablas de 1950; insectos-humanos: una casa de
    /// ladrillo de Kapilavastu (s. V a. C.).
    /// </summary>
    internal static AreaFunctionSite LifeCycleHouse(Transform t, List<GameObject> fam, bool insects)
    {
        Year = insects ? -500 : 1950;
        Region = (insects ? MicroStyles.Region.SurAsia : MicroStyles.Region.Norteamerica).ToString();   // charpai y chulha / la casa de 1950
        MicroFurniture.DefaultBody = insects ? MicroBody.OfSpecies("Ant", 1.75f) : MicroBody.Human(1.75f);
        Style st = insects ? BurntBrick : Clapboard;
        string title = insects ? "Casa de ladrillo de Kapilavastu" : "Casita";
        AreaFunctionSite h = NewHouse(t, null, fam, st, title, fam.Count + ChildBeds);
        Household hh = MakeHousehold(h, fam);
        hh.bottomless = true;
        hh.pantry.rations = 20; hh.pantry.dishName = insects ? "arroz con lentejas" : "guiso de la casa";
        MicroBuildRules.Validate(h.transform, $"Casa de ciclo de vida ({(insects ? "insectos-humanos" : "humanos")})", true);
        return h;
    }

    static string First(string n) { int p = n.IndexOf(' '); return p > 0 ? n.Substring(0, p) : n; }
}
