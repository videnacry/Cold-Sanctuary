using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS ERAS QUE FALTABAN (docs/microcosmos-civilizaciones.md §8–9): Planta Atómica (1666/1898/1905), Varsovia 1942 (Irena
/// Sendler), Le Chambon-sur-Lignon 1942, Uruk (~3500 a. C., Mesopotamia) y la primera aldea (~9500 a. C., el Huerto). Donde la
/// historia ya trae sus edificios (los pueblos de las simulaciones de compasión, hechos macizos), se les añade el SITIO del
/// registro y sus puestos delante de la puerta (<see cref="Porch"/>); donde no, se construyen (<see cref="Building"/>).
/// </summary>
internal static partial class CivicBuilder
{
    static readonly Style StoneManor = new Style(C(0.82f, 0.76f, 0.6f), C(0.55f, 0.5f, 0.42f), C(0.35f, 0.3f, 0.28f), Roof.Gable, 4.2f);   // piedra caliza de Lincolnshire
    static readonly Style Sandstone = new Style(C(0.75f, 0.68f, 0.52f), C(0.5f, 0.45f, 0.4f), C(0.4f, 0.28f, 0.22f), Roof.Gable, 4.5f);    // la arenisca de Berna
    static readonly Style LabShed = new Style(C(0.6f, 0.55f, 0.48f), C(0.42f, 0.38f, 0.33f), C(0.7f, 0.78f, 0.82f), Roof.Gable, 3.6f);
    static readonly Style MudBrick = new Style(C(0.76f, 0.62f, 0.44f), C(0.68f, 0.58f, 0.42f), C(0.7f, 0.6f, 0.45f), Roof.Flat, 3.2f);       // adobe de Uruk, azotea de cañas y barro
    static readonly Style Whitewash = new Style(C(0.95f, 0.94f, 0.9f), C(0.8f, 0.74f, 0.62f), C(0.9f, 0.88f, 0.82f), Roof.Flat, 4.5f);      // el Templo Blanco (adobe encalado)

    // ══ PLANTA ATÓMICA: Woolsthorpe Manor (la casa de piedra de Newton, Lincolnshire; allí pasó 1665–66, el «año milagroso»),
    //    el laboratorio de la ESPCI en la rue Lhomond (el cobertizo de los Curie ya está en la historia: aquí, su laboratorio de
    //    mesas) y la Oficina de Patentes de Berna (Einstein, «técnico experto de 3.ª clase», 1902–09; el edificio de Correos y
    //    Telégrafos de la Speichergasse). (National Trust, «Woolsthorpe Manor»; Curie 1904, tesis; Isaacson 2007, Einstein.)
    static int AtomicPlant(Transform t)
    {
        var manor = Building(t, "Woolsthorpe Manor (la casa de Newton)", "Casa", "Woolsthorpe Manor", new Vector3(-31f, 0f, 4f), 0f, new Vector2(12f, 9f), StoneManor,
            S("cama", -3.5f, 2.5f, 1.2f, 2f, "Cama"), S("fogon", 3.5f, 3f, 1.6f, 0.8f, "Chimenea de la cocina"), S("mesa", 0f, -1f, 2f, 1f, "Mesa de estudio (el prisma)"));
        var lab = Building(t, "Laboratorio de la ESPCI (rue Lhomond)", "Laboratorio", "Laboratorio de los Curie (ESPCI, París)", new Vector3(0f, 0f, 38f), 0f, new Vector2(14f, 9f), LabShed,
            S("mesa_laboratorio", -4f, 2.5f, 2.6f, 1f, "Mesa de análisis (pechblenda)"), S("mesa_laboratorio", 2f, 2.5f, 2.6f, 1f, "Mesa del electrómetro"), S("estante", 5.5f, -1f, 0.8f, 2.4f, "Estante de muestras"));
        var office = Building(t, "Oficina de Patentes de Berna (1905)", "Oficina de patentes", "Oficina Federal de la Propiedad Intelectual (Berna)", new Vector3(31f, 0f, 4f), 0f, new Vector2(14f, 9f), Sandstone,
            S("mesa", -4f, 2f, 1.8f, 1f, "Pupitre de examinador"), S("mesa", 0f, 2f, 1.8f, 1f, "Pupitre de examinador"), S("mesa", 4f, 2f, 1.8f, 1f, "Pupitre de examinador"));
        int n = 0;
        n += Worker(t, "Hannah Ayscough", manor, "", manor, null, manor);        // la madre de Newton, que llevaba la granja
        n += Worker(t, "André Debierne", lab, "sabio", lab, null, manor);         // colaborador de los Curie (descubrió el actinio, 1899)
        n += Worker(t, "Michele Besso", office, "sabio", office, null, manor);    // el amigo de Einstein en la oficina de patentes
        n += Worker(t, "Examinador de patentes", office, "sabio", office, null, manor);
        n += Couple(t, "Vecina de Woolsthorpe", "Vecino de Woolsthorpe", manor, manor, null);
        return n;
    }

    // ══ VARSOVIA, 1942 (Irena Sendler): los edificios ya los pone la historia (macizos). Se hacen sitios del registro: la
    //    oficina de Żegota (documentos falsos), el convento de las Franciscanas y el orfanato del Padre Boduen (cuidar a los niños
    //    escondidos), la familia de acogida (una casa). (Mieszkowska 2011, Irena Sendler: Mother of the Children of the Holocaust.)
    static int Warsaw1942(Transform t)
    {
        var office = Porch(t, "Oficina de Żegota (documentos)", "Taller de papeles", "Oficina de Żegota", S("mesa", 0f, 0f, 2f, 1f, "Mesa de los documentos"));
        var conv = Porch(t, "Convento de las Franciscanas", "Orfanato", "Convento de las Franciscanas de la Familia de María", S("cama", -2f, 0f, 1.2f, 2f, "Catre de los niños"), S("fogon", 2f, 0f, 1.2f, 0.8f, "Cocina del convento"));
        var orph = Porch(t, "Orfanato del Padre Boduen", "Orfanato", "Orfanato del Padre Boduen", S("cama", -2f, 0f, 1.2f, 2f, "Cuna"), S("fogon", 2f, 0f, 1.2f, 0.8f, "Cocina"));
        var fam = Porch(t, "Familia de acogida", "Casa", "Casa de la familia de acogida", S("cama", -1.8f, 0f, 1.2f, 2f, "Cama"), S("fogon", 1.8f, 0f, 1.2f, 0.8f, "Cocina"));
        if (office == null || conv == null || orph == null || fam == null) { Debug.LogWarning("[Eras] Varsovia: faltan edificios de la historia; sin sitios."); return 0; }
        int n = 0;
        n += Worker(t, "Hermana franciscana", conv, "", conv, null, conv);
        n += Worker(t, "Cuidadora del orfanato", orph, "", orph, null, orph);
        n += Worker(t, "Falsificador de Żegota", office, "sabio", fam, null, fam);
        n += Couple(t, "Madre de acogida de Varsovia", "Padre de acogida de Varsovia", fam, fam, null);
        return n;
    }

    // ══ LE CHAMBON-SUR-LIGNON, 1942: el templo protestante, el presbiterio de los Trocmé, la Escuela Nueva Cevenol (fundada en
    //    1938 por André Trocmé y Édouard Theis), la pensión, la granja del plateau y la panadería. Los pone la historia; aquí,
    //    sus sitios del registro. (Hallie 1979, Lest Innocent Blood Be Shed; Yad Vashem, «Le Chambon-sur-Lignon».)
    static int LeChambon(Transform t)
    {
        var temple = Porch(t, "Templo protestante", "Templo", "Templo protestante de Le Chambon", S("altar", 0f, 0f, 1.6f, 0.8f, "Mesa de la comunión (en el atrio)"));
        var pres = Porch(t, "Presbiterio de los Trocmé", "Casa", "Presbiterio de los Trocmé", S("fogon", -1.6f, 0f, 1.2f, 0.8f, "Cocina"), S("cama", 1.6f, 0f, 1.2f, 2f, "Banco del zaguán"));
        var school = Porch(t, "Escuela Nueva Cevenol", "Escuela", "Escuela Nueva Cevenol", S("pizarra", -2f, 0f, 0.4f, 1.8f, "Pizarra al aire libre"), S("mesa", 1.5f, 0f, 2f, 1f, "Pupitres"));
        var inn = Porch(t, "Pensión «Maison des Roches»", "Restaurante", "Pensión «Maison des Roches»", S("fogon", -2.5f, 0f, 1.2f, 0.8f, "Cocina"), S("mesa", 1f, 0f, 1.6f, 1.2f, "Mesa común"));
        var farm = Porch(t, "Granja del plateau", "Huerto", "Granja del plateau", S("bancal", -2f, 0f, 2.5f, 3f, "Patatas"), S("bancal", 2f, 0f, 2.5f, 3f, "Coles"));
        var bakery = Porch(t, "Panadería", "Cocina", "Panadería de Le Chambon", S("horno", 0f, 0f, 1.6f, 1.2f, "Horno de pan"));
        if (temple == null || pres == null || school == null || inn == null || farm == null || bakery == null) { Debug.LogWarning("[Eras] Le Chambon: faltan edificios de la historia; sin sitios."); return 0; }
        int n = 0;
        n += Worker(t, "Maestra de la Cevenol", school, "maestro", inn, null, pres);
        n += Worker(t, "Panadero de Le Chambon", bakery, "cocinero", inn, null, farm);
        n += Worker(t, "Cocinera de la pensión", inn, "cocinero", inn, null, inn);
        n += Worker(t, "Campesina del plateau", farm, "hortelano", farm, temple, farm);
        n += Couple(t, "Campesina de Le Mazet", "Campesino de Le Mazet", farm, inn, temple);
        return n;
    }

    // ══ URUK (~3500 a. C.): casas de ADOBE alrededor de un patio, el TEMPLO BLANCO (adobe encalado) sobre la terraza de Anu, la
    //    alfarería de los cuencos de borde biselado (hechos en molde por miles: las raciones de cebada) y el granero. La escritura
    //    (−3200) aún no: sin escuela de escribas. (Liverani 2006, Uruk: The First City; Wikipedia «Uruk», «White Temple and
    //    ziggurat of Uruk», «Bevelled rim bowl».)
    static int Uruk(Transform t)
    {
        Material terr = AreaMicrocosmos.Mat("Civ_Terraza_Anu", C(0.72f, 0.6f, 0.42f));
        AreaMicrocosmos.Box(t, "Terraza de Anu", new Vector3(0f, 0.15f, 46f), new Vector3(22f, 0.3f, 10f), terr, false);   // su planta (regla 2: un bloque de 4 m tapaba la vista)
        var temple = Building(t, "Templo Blanco (al pie de la terraza de Anu)", "Templo", "Templo Blanco de Uruk", new Vector3(0f, 0f, 36f), 0f, new Vector2(14f, 8f), Whitewash,
            S("altar", 0f, 2f, 1.6f, 1f, "Altar (mesa de ofrendas)"));
        var house1 = Building(t, "Casa de adobe con patio", "Casa", "Casa de Uruk", new Vector3(-31f, 0f, 4f), 0f, new Vector2(12f, 10f), MudBrick,
            S("cama", -3.5f, 3f, 1.2f, 2f, "Estera de dormir"), S("fogon", 3.5f, 3f, 1.4f, 1f, "Horno de pan (tannur)"));
        var pottery = Building(t, "Alfarería (cuencos de borde biselado)", "Alfarería", "Alfarería de Uruk", new Vector3(31f, 0f, 4f), 0f, new Vector2(12f, 9f), MudBrick,
            S("torno_alfarero", -3f, 2f, 1.4f, 1.4f, "Moldes de cuencos"), S("torno_alfarero", 2f, 2f, 1.4f, 1.4f, "Torno lento"), S("fogon", 4.5f, -2.5f, 1.6f, 1.6f, "Horno de cerámica"));
        var granary = Building(t, "Granero del templo", "Granero", "Granero de cebada", new Vector3(31f, 0f, -26f), 0f, new Vector2(10f, 8f), MudBrick,
            S("granero", -2.5f, 1.5f, 2f, 2f, "Silo de cebada"), S("granero", 2.5f, 1.5f, 2f, 2f, "Molino de mano"));
        var house2 = Building(t, "Casa del barrio de los alfareros", "Casa", "Casa de Uruk", new Vector3(-31f, 0f, -26f), 0f, new Vector2(10f, 9f), MudBrick,
            S("cama", -2.5f, 2.5f, 1.2f, 2f, "Estera de dormir"), S("fogon", 2.5f, 2.5f, 1.4f, 1f, "Hogar"));
        int n = 0;
        n += Worker(t, "Alfarero de Uruk", pottery, "alfarero", granary, null, house2);
        n += Worker(t, "Alfarera de Uruk", pottery, "alfarero", granary, null, house2);
        n += Worker(t, "Sacerdotisa del Eanna", temple, "", granary, null, house1);
        n += Worker(t, "Guardián del granero", granary, "comerciante", granary, null, house1);
        n += Couple(t, "Madre de Uruk", "Padre de Uruk", house1, granary, temple);
        return n;
    }

    // ══ LA PRIMERA ALDEA (~9500 a. C., el Huerto): casas REDONDAS de piedra y barro, semienterradas, de ~5 m de diámetro, con el
    //    hogar dentro y cubierta cónica de ramas y barro (Ain Mallaha / Eynan, Natufiense; Jericó y Netiv Hagdud, PPNA) y un
    //    granero redondo (los primeros silos, Dhra', ~9300 a. C.). (Kuijt y Finlayson 2009, PNAS 106:10966; Bar-Yosef 1998.)
    static int FirstVillage(Transform t)
    {
        var h1 = RoundHouse(t, "Casa redonda", new Vector3(-30f, 0f, 16f));
        var h2 = RoundHouse(t, "Casa redonda", new Vector3(-30f, 0f, -14f));
        var h3 = RoundHouse(t, "Casa redonda", new Vector3(28f, 0f, -22f));
        int n = 0;
        n += Couple(t, "Madre de la aldea", "Padre de la aldea", h1, h1, null);
        n += Couple(t, "Hija de la aldea", "Yerno de la aldea", h2, h2, null);
        n += Worker(t, "Abuela de la aldea", h3, "", h3, null, h3);
        return n;
    }
    /// <summary>Una casa redonda natufiense/PPNA, ABIERTA (reglas de edificación: sin muro ni cubierta): su suelo de barro
    /// semienterrado, el anillo de piedras del cimiento a ras de suelo (como se excava hoy), los postes que sostenían la cubierta
    /// y dentro el hogar y la estera. Sitio «Casa» del registro.</summary>
    internal static AreaFunctionSite RoundHouse(Transform t, string name, Vector3 pos, float radius = 2.6f)
    {
        Transform b = new GameObject(name).transform; b.SetParent(t, false); b.localPosition = pos;
        Material wall = AreaMicrocosmos.Mat("Civ_Piedra_barro", C(0.55f, 0.47f, 0.37f)), post = AreaMicrocosmos.Mat("Civ_Ramas_barro", C(0.5f, 0.42f, 0.3f));
        Material floor = AreaMicrocosmos.Mat("Civ_Suelo_barro", C(0.45f, 0.38f, 0.3f));
        // El radio crece lo que pidan el hogar y la estera con 2 m de paso entre ellos y al anillo.
        radius = Mathf.Max(radius, 3.6f);
        if (name.StartsWith("Casa")) radius = Mathf.Max(radius, 5f);   // (2026-11-03) con el banco para personas dentro del anillo
        AreaMicrocosmos.Box(b, "Suelo", new Vector3(0f, 0.04f, 0f), new Vector3(radius * 2f, 0.08f, radius * 2f), floor, false);
        int segs = 14;
        for (int k = 0; k < segs; k++)
        {
            float a = (k + 0.5f) / segs * Mathf.PI * 2f;
            Vector3 p = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radius;
            AreaMicrocosmos.Box(b, "Piedra_del_cimiento", p + Vector3.up * 0.09f, new Vector3(2f * Mathf.PI * radius / segs * 0.8f, 0.18f, 0.4f), Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f), wall, false);
            if (k % 3 == 0) AreaMicrocosmos.Box(b, "Poste", p * 0.92f + Vector3.up * 1.2f, new Vector3(0.18f, 2.4f, 0.18f), post, true);
        }
        var site = b.gameObject.AddComponent<AreaFunctionSite>(); site.building = "Casa"; site.title = "Casa redonda (~9500 a. C.)";
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = site.title; wl.height = 4f;
        int y0 = Year; Year = -9500;
        foreach (var (key, x, label) in new[] { ("fogon", 2.3f, "Hogar"), ("cama", -2.3f, "Estera") })   // (a ±2,3 m: a ±1,6 quedaba 1,3 m de paso, < 2 m de las reglas)
        {
            Transform p = MicroFurniture.Build(b, key, new Vector3(x, 0.08f, 0f), 90f, label, -9500, null, Region);
            p.gameObject.AddComponent<AreaFunctionStation>().station = key;
            var l = p.gameObject.AddComponent<WorldLabel>(); l.text = label; l.height = 1f;
        }
        // (2026-11-03) Reglas 9 y 11: un asiento para personas delante de la puerta y su FRUTAL detrás (la higuera de Gilgal).
        MicroFurniture.Build(b, "banco", new Vector3(0f, 0.08f, 3.0f), 0f, "Banco (para personas)", -9500, null, Region, MicroBody.Human());
        if (name.StartsWith("Casa")) { Transform fr = FruitTree(b, b.GetSiblingIndex()); fr.localPosition = new Vector3(0f, 0.08f, -(radius + 1.5f)); }
        site.region = Region; Tag(site, "dormir junto al hogar, moler y guardar el grano");
        Year = y0;
        return site;
    }

    /// <summary>El GRANERO REDONDO de la aldea (los primeros silos): donde la tribu lleva lo que recoge (<see cref="MicroLair"/>)
    /// en vez de una guarida. Sitio «Granero» del registro.</summary>
    internal static Transform RoundGranary(Transform t, Vector3 pos)
    {
        AreaFunctionSite site = RoundHouse(t, "Granero redondo", pos, 2.2f);
        site.building = "Granero"; site.title = "Granero redondo (los primeros silos)"; site.activity = ""; site.interaction = ""; Tag(site);
        site.GetComponent<WorldLabel>().text = site.title;
        foreach (AreaFunctionStation s in site.GetComponentsInChildren<AreaFunctionStation>()) s.station = "granero";
        var lair = site.gameObject.AddComponent<MicroLair>(); lair.doorOffset = new Vector3(0f, 0f, -3.4f);   // delante de la puerta
        return site.transform;
    }
    /// <summary>El edificio <paramref name="name"/> que puso la historia (con su «Cuerpo», hoy la losa de su planta abierta: la
    /// puerta en +z local) se hace SITIO del registro: sus muebles detallados (<see cref="MicroFurniture"/>) van sobre su planta
    /// y delante, en filas con el paso de 2 m (<see cref="Layout"/>). Una Casa o un orfanato traen sus necesidades básicas.
    /// null si la historia no lo tiene.</summary>
    static AreaFunctionSite Porch(Transform t, string name, string template, string title, params (string key, Vector2 at, Vector2 size, string label)[] stations)
    {
        Transform b = t.Find(name);
        if (b == null) return null;
        Transform body = b.Find("Cuerpo");
        Vector2 foot = body != null ? new Vector2(body.localScale.x, body.localScale.z) : new Vector2(5f, 5f);
        var site = b.gameObject.AddComponent<AreaFunctionSite>(); site.building = template; site.title = title;
        var pieces = Furnish(b, template, stations);
        // Los muebles en las filas MÁS COMPACTAS que dejan el paso de 2 m (la anchura de fila que da la planta más pequeña), centrados
        // en la planta; si no caben, la planta (su losa, sus bordes y sus postes) crece hasta cubrirlos. (2026-11-01) Antes las filas
        // eran del ancho de la planta (5–8 m: un mueble por fila) y lo que no cabía salía hacia delante hasta 10 m, encima del
        // edificio de enfrente: «Mesa de los documentos» a 0,88 m de la «Cama» de la familia de acogida (Varsovia).
        Vector2 need = CompactLayout(b, pieces, foot);
        if (need.x > foot.x + 0.01f || need.y > foot.y + 0.01f) Grow(b, Vector2.Max(foot, need));
        HidingPlace hp = b.GetComponent<HidingPlace>();
        if (hp != null) hp.radius = Mathf.Max(hp.radius, Mathf.Max(need.x, need.y) * 0.5f + 0.5f);
        site.region = Region; Tag(site);
        return site;
    }

    /// <summary>Prueba anchuras de fila desde la del mueble más ancho hasta la de todos en fila y se queda con la que da la planta de
    /// menor área (con la de <paramref name="foot"/> como mínimo). Deja los muebles colocados así y devuelve lo que ocupan (con el
    /// paso de 2 m al borde).</summary>
    static Vector2 CompactLayout(Transform b, List<Transform> pieces, Vector2 foot)
    {
        float P = MicroBuildRules.Passage, widest = 0f, all = 0f;
        foreach (Transform p in pieces) { float w = MicroFurniture.LocalBounds(p).size.x; widest = Mathf.Max(widest, w); all += w + P; }
        float bestW = widest + 2f * P, bestArea = float.MaxValue;
        for (float w = widest + 2f * P; w <= all + 2f * P + 0.01f; w += 0.25f)
        {
            Vector2 plot = Vector2.Max(foot, Layout(b, pieces, new Vector2(w, 0f), 0f));
            if (plot.x * plot.y < bestArea - 1e-3f) { bestArea = plot.x * plot.y; bestW = w; }
        }
        return Layout(b, pieces, new Vector2(bestW, 0f), 0f);
    }

    /// <summary>La planta abierta de un edificio de la historia (CompassionSceneBuilder.Building: «Cuerpo», «Borde_del_suelo» y
    /// «Poste») crece hasta <paramref name="to"/>, centrada.</summary>
    static void Grow(Transform b, Vector2 to)
    {
        foreach (Transform c in b)
        {
            Vector3 s = c.localScale, p = c.localPosition;
            if (c.name == "Cuerpo") c.localScale = new Vector3(to.x, s.y, to.y);
            else if (c.name == "Borde_del_suelo") { c.localPosition = new Vector3(Mathf.Sign(p.x) * (to.x * 0.5f - 0.15f), p.y, p.z); c.localScale = new Vector3(s.x, s.y, to.y); }
            else if (c.name == "Poste") c.localPosition = new Vector3(Mathf.Sign(p.x) * (to.x * 0.5f - 0.15f), p.y, Mathf.Sign(p.z) * (to.y * 0.5f - 0.15f));
        }
    }
}
