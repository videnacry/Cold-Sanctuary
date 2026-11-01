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
        AreaMicrocosmos.Box(t, "Terraza de Anu", new Vector3(0f, 2f, 46f), new Vector3(22f, 4f, 10f), terr, true);
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

    /// <summary>Una casa redonda natufiense/PPNA: muro circular bajo de piedra y barro (con la puerta a −z), cubierta cónica sin
    /// colisión, el hogar y una estera dentro. Sitio «Casa» del registro.</summary>
    internal static AreaFunctionSite RoundHouse(Transform t, string name, Vector3 pos, float radius = 2.6f)
    {
        Transform b = new GameObject(name).transform; b.SetParent(t, false); b.localPosition = pos;
        Material wall = AreaMicrocosmos.Mat("Civ_Piedra_barro", C(0.55f, 0.47f, 0.37f)), roof = AreaMicrocosmos.Mat("Civ_Ramas_barro", C(0.5f, 0.42f, 0.3f));
        Material floor = AreaMicrocosmos.Mat("Civ_Suelo_barro", C(0.45f, 0.38f, 0.3f));
        AreaMicrocosmos.Box(b, "Suelo", new Vector3(0f, 0.04f, 0f), new Vector3(radius * 2f, 0.08f, radius * 2f), floor, false);
        int segs = 12; float h = 1.6f;
        for (int k = 0; k < segs; k++)
        {
            float a = (k + 0.5f) / segs * Mathf.PI * 2f;
            Vector3 p = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radius;
            if (p.z < -radius * 0.85f) continue;   // la puerta (al sur, −z)
            AreaMicrocosmos.Box(b, "Muro_redondo", p + Vector3.up * h * 0.5f, new Vector3(2f * Mathf.PI * radius / segs + 0.1f, h, 0.45f), Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f), wall, true);
        }
        for (int k = 0; k < 4; k++)   // la cubierta cónica de ramas y barro (sin colisión)
        {
            float s = (radius * 2.3f) * (1f - k * 0.24f);
            AreaMicrocosmos.Box(b, "Cubierta", new Vector3(0f, h + 0.25f + k * 0.45f, 0f), new Vector3(s, 0.45f, s), Quaternion.Euler(0f, k * 22f, 0f), roof, false);
        }
        var site = b.gameObject.AddComponent<AreaFunctionSite>(); site.building = "Casa"; site.title = "Casa redonda (~9500 a. C.)";
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = site.title; wl.height = h + 3f;
        foreach (var (key, x, z, w, d, label) in new[] { ("fogon", 0.6f, 0.6f, 0.9f, 0.9f, "Hogar"), ("cama", -1.1f, 0.2f, 1f, 1.8f, "Estera") })
        {
            float sh = key == "cama" ? 0.15f : 0.35f;
            Transform p = AreaMicrocosmos.Box(b, "Puesto_" + key, new Vector3(x, sh * 0.5f + 0.08f, z), new Vector3(w, sh, d), StationMat(key, wall), true);
            p.gameObject.AddComponent<AreaFunctionStation>().station = key;
            var l = p.gameObject.AddComponent<WorldLabel>(); l.text = label; l.height = 1f;
        }
        return site;
    }

    /// <summary>El GRANERO REDONDO de la aldea (los primeros silos): donde la tribu lleva lo que recoge (<see cref="MicroLair"/>)
    /// en vez de una guarida. Sitio «Granero» del registro.</summary>
    internal static Transform RoundGranary(Transform t, Vector3 pos)
    {
        AreaFunctionSite site = RoundHouse(t, "Granero redondo", pos, 2.2f);
        site.building = "Granero"; site.title = "Granero redondo (los primeros silos)";
        site.GetComponent<WorldLabel>().text = site.title;
        foreach (AreaFunctionStation s in site.GetComponentsInChildren<AreaFunctionStation>()) s.station = "granero";
        var lair = site.gameObject.AddComponent<MicroLair>(); lair.doorOffset = new Vector3(0f, 0f, -3.4f);   // delante de la puerta
        return site.transform;
    }

    /// <summary>El edificio macizo <paramref name="name"/> que puso la historia (con su «Cuerpo»; la puerta en +z local) se hace
    /// SITIO del registro: sus puestos van delante de la puerta, en fila. null si la historia no lo tiene.</summary>
    static AreaFunctionSite Porch(Transform t, string name, string template, string title, params (string key, Vector2 at, Vector2 size, string label)[] stations)
    {
        Transform b = t.Find(name);
        if (b == null) return null;
        Transform body = b.Find("Cuerpo");
        float depth = body != null ? body.localScale.z : 5f;
        var site = b.gameObject.AddComponent<AreaFunctionSite>(); site.building = template; site.title = title;
        Material furn = AreaMicrocosmos.Mat("Civ_Mueble", new Color(0.5f, 0.38f, 0.26f));
        foreach (var s in stations)
        {
            float sh = StationHeight(s.key);
            Transform p = AreaMicrocosmos.Box(b, "Puesto_" + s.key, new Vector3(s.at.x, sh * 0.5f + 0.02f, depth * 0.5f + 2.2f + s.size.y * 0.5f + s.at.y), new Vector3(s.size.x, sh, s.size.y), StationMat(s.key, furn), true);
            p.gameObject.AddComponent<AreaFunctionStation>().station = s.key;
            var l = p.gameObject.AddComponent<WorldLabel>(); l.text = s.label; l.height = 1.2f;
        }
        return site;
    }
}
