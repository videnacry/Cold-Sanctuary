using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL PUEBLO DE BRUCE LEE (docs/microcosmos-bruce-lee.md §5, docs/microcosmos-reglas-edificacion.md): era avanzada (1957–1971), así
/// que la ciudad real ocupa TODO el piso del Dojo ×10 (sin sus muebles gigantes). Dos barrios, como la historia: KOWLOON en los
/// años 50 (oeste del piso: la escuela de Yip Man, el piso de la familia Lee en Nathan Road, el colegio de St. Francis Xavier,
/// un cha chaan teng, el mercado de la calle del Templo y el templo de Tin Hau, con calles de tong lau y banianos) y SEATTLE
/// 1959–64 con Oakland 1964 y Los Ángeles 1967 (este del piso: el restaurante de Ruby Chow con el cuarto de Bruce encima, el Jun
/// Fan Gung Fu Institute de University Way, el aula de la Universidad de Washington, la escuela de Oakland y la de Chinatown de
/// Los Ángeles). Todo abierto (sin muros ni techos), con muebles detallados a 2 m y las necesidades básicas.
/// Lo habitan solo insectos-humanos; de flora, los banianos de las calles de Kowloon (Ficus microcarpa, el árbol de sombra de
/// sus calles) y, de la naturaleza, solo la bahía de la historia.
/// </summary>
internal static partial class CivicBuilder
{
    static readonly Style Terrazo = new Style(C(0.8f, 0.78f, 0.7f), C(0.72f, 0.7f, 0.64f), C(0.5f, 0.5f, 0.5f), Roof.None);
    static readonly Style TinHau = new Style(C(0.6f, 0.25f, 0.2f), C(0.66f, 0.6f, 0.5f), C(0.35f, 0.5f, 0.4f), Roof.None) { columns = true, wallH = 3.2f };
    static readonly Style Linoleum = new Style(C(0.55f, 0.45f, 0.35f), C(0.62f, 0.56f, 0.48f), C(0.3f, 0.3f, 0.3f), Roof.None);
    static readonly Style Campus = new Style(C(0.7f, 0.55f, 0.42f), C(0.6f, 0.52f, 0.44f), C(0.3f, 0.3f, 0.3f), Roof.None);

    // ══ BRUCE LEE: Kowloon (años 50), Seattle (1959–64), Oakland (1964) y Los Ángeles (1967) ══════════════════════
    // Fuentes (docs/microcosmos-bruce-lee.md §5): HistoryLink 3999; Polly 2018 (Bruce Lee: A Life); Hong Kong Heritage Museum,
    // «Bruce Lee: Kung Fu · Art · Life» (2013); Wikipedia «Tin Hau Temple, Yau Ma Tei», «Temple Street», «Cha chaan teng»,
    // «St. Francis Xavier's College», «Ruby Chow».
    static int BruceLeeEra(Transform t)
    {
        var lots = new Lots(t);
        Rect fl = lots.floor;
        // Kowloon al oeste del claro de la historia, Seattle / Oakland / Los Ángeles al este.
        Vector2 hk = new Vector2(Mathf.Lerp(0f, fl.xMin, 0.45f), 0f), us = new Vector2(Mathf.Lerp(0f, fl.xMax, 0.45f), 0f);
        var placedHK = new List<Transform>(); var placedUS = new List<Transform>();

        // ── KOWLOON, años 50 ──
        var school = Place(t, lots, Building(t, "Escuela de Yip Man (Lee Tat Street, Yau Ma Tei)", "Dojo", "Escuela de Wing Chun de Yip Man (Kowloon, años 50)", Vector3.zero, 0f, new Vector2(16f, 12f), Terrazo,
            S("tatami", 0f, 0f, 6f, 5f, "Suelo de entrenar (chi sao)"), S("muneco", 0f, 0f, 1f, 1f, "Muñeco de madera"), S("muneco", 0f, 0f, 1f, 1f, "Muñeco de madera"),
            S("banco", 0f, 0f, 1f, 1f, "Banco de los alumnos"), S("lavabo", 0f, 0f, 1f, 1f, "Lavabo"), S("inodoro", 0f, 0f, 1f, 1f, "Retrete del piso")), hk + new Vector2(0f, 30f), placedHK);
        var flat = Place(t, lots, Building(t, "Piso de la familia Lee (218 Nathan Road)", "Casa", "Piso de la familia Lee (Nathan Road, Kowloon)", Vector3.zero, 0f, new Vector2(14f, 12f), TongLau,
            S("cama", 0f, 0f, 1f, 2f, "Cama de Bruce"), S("cama", 0f, 0f, 1f, 2f, "Cama de los hermanos"), S("cama", 0f, 0f, 1f, 2f, "Cama de los padres"), S("fogon", 0f, 0f, 2f, 1f, "Cocina de gas con wok"),
            S("mesa", 0f, 0f, 1.4f, 1f, "Mesa de la familia"), S("estante", 0f, 0f, 1.6f, 0.5f, "Estante de libros y tebeos")), hk + new Vector2(0f, -30f), placedHK);
        var cafe = Place(t, lots, Building(t, "Cha chaan teng (casa de té de Kowloon)", "Restaurante", "Cha chaan teng", Vector3.zero, 0f, new Vector2(16f, 12f), ChaChaanTeng,
            S("fogon", 0f, 0f, 3f, 1f, "Cocina vista (wok y fogones)"), S("barra", 0f, 0f, 4f, 0.8f, "Barra (té con leche y bollos de piña)"),
            S("mesa", 0f, 0f, 1.4f, 1.4f, "Mesa con taburetes"), S("mesa", 0f, 0f, 1.4f, 1.4f, "Mesa con taburetes"), S("mesa", 0f, 0f, 1.4f, 1.4f, "Mesa con taburetes"),
            S("inodoro", 0f, 0f, 1f, 1f, "Retrete"), S("lavabo", 0f, 0f, 1f, 1f, "Lavabo")), hk + new Vector2(-40f, 0f), placedHK);
        var college = Place(t, lots, Building(t, "St. Francis Xavier's College (Tai Kok Tsui)", "Escuela", "Colegio de St. Francis Xavier (1956–59)", Vector3.zero, 0f, new Vector2(16f, 12f), Campus,
            S("pizarra", 0f, 0f, 1.4f, 0.4f, "Pizarra"), S("mesa", 0f, 0f, 1.4f, 1f, "Pupitres"), S("mesa", 0f, 0f, 1.4f, 1f, "Pupitres"), S("estante", 0f, 0f, 1.6f, 0.5f, "Biblioteca de la clase")), hk + new Vector2(-40f, 40f), placedHK);
        var market = Place(t, lots, Building(t, "Mercado de la calle del Templo (Yau Ma Tei)", "Mercado", "Mercado nocturno de Temple Street", Vector3.zero, 0f, new Vector2(18f, 8f), TongLau,
            S("puesto", 0f, 0f, 2f, 1f, "Puesto de fruta"), S("puesto", 0f, 0f, 2f, 1f, "Puesto de pescado seco"), S("puesto", 0f, 0f, 2f, 1f, "Puesto de ropa"), S("puesto", 0f, 0f, 2f, 1f, "Puesto de cuencos de arroz")), hk + new Vector2(-40f, -40f), placedHK);
        var temple = Place(t, lots, Building(t, "Templo de Tin Hau (Yau Ma Tei)", "Templo", "Templo de Tin Hau (diosa del mar)", Vector3.zero, 0f, new Vector2(12f, 10f), TinHau,
            S("altar", 0f, 0f, 1.5f, 0.8f, "Altar de Tin Hau (incienso)")), hk + new Vector2(-80f, 0f), placedHK);
        var tong1 = Place(t, lots, Building(t, "Tong lau de Mong Kok", "Casa", "Casa (tong lau)", Vector3.zero, 0f, new Vector2(12f, 10f), TongLau,
            S("cama", 0f, 0f, 1f, 2f, "Litera"), S("cama", 0f, 0f, 1f, 2f, "Cama")), hk + new Vector2(-80f, 40f), placedHK);

        // ── SEATTLE (1959–64), OAKLAND (1964) y LOS ÁNGELES (1967) ──
        var ruby = Place(t, lots, Building(t, "Restaurante de Ruby Chow (Broadway y Jefferson, Seattle)", "Restaurante", "Restaurante de Ruby Chow (Seattle, 1959)", Vector3.zero, 0f, new Vector2(18f, 12f), SeattleBrick,
            S("fogon", 0f, 0f, 3f, 1f, "Cocina (woks)"), S("fogon", 0f, 0f, 3f, 1f, "Cocina (vapor y frituras)"), S("barra", 0f, 0f, 4f, 0.8f, "Barra"),
            S("mesa", 0f, 0f, 1.4f, 1.4f, "Mesa"), S("mesa", 0f, 0f, 1.4f, 1.4f, "Mesa"), S("mesa", 0f, 0f, 1.4f, 1.4f, "Mesa"), S("inodoro", 0f, 0f, 1f, 1f, "Aseo"), S("lavabo", 0f, 0f, 1f, 1f, "Lavabo")), us + new Vector2(0f, 30f), placedUS);
        var room = Place(t, lots, Building(t, "Cuarto de Bruce (encima del restaurante)", "Casa", "Cuarto de Bruce en casa de Ruby Chow", Vector3.zero, 0f, new Vector2(10f, 8f), Linoleum,
            S("cama", 0f, 0f, 1f, 2f, "Cama de Bruce"), S("mesa", 0f, 0f, 1f, 1f, "Mesa de estudio")), us + new Vector2(0f, 60f), placedUS);
        var jfgi = Place(t, lots, Building(t, "Jun Fan Gung Fu Institute (4750 University Way)", "Dojo", "Jun Fan Gung Fu Institute (Seattle, 1963)", Vector3.zero, 0f, new Vector2(16f, 12f), Linoleum,
            S("tatami", 0f, 0f, 6f, 5f, "Suelo de entrenar"), S("muneco", 0f, 0f, 1f, 1f, "Muñeco de madera"), S("banco", 0f, 0f, 1f, 1f, "Banco"), S("inodoro", 0f, 0f, 1f, 1f, "Aseo"), S("lavabo", 0f, 0f, 1f, 1f, "Lavabo")), us + new Vector2(0f, -30f), placedUS);
        var uw = Place(t, lots, Building(t, "Universidad de Washington (aula de filosofía)", "Escuela", "Universidad de Washington (1961–64)", Vector3.zero, 0f, new Vector2(16f, 12f), Campus,
            S("pizarra", 0f, 0f, 1.4f, 0.4f, "Pizarra"), S("mesa", 0f, 0f, 1.4f, 1f, "Pupitres"), S("mesa", 0f, 0f, 1.4f, 1f, "Pupitres"), S("estante", 0f, 0f, 1.6f, 0.5f, "Libros de filosofía")), us + new Vector2(40f, 40f), placedUS);
        var oakland = Place(t, lots, Building(t, "Escuela de Oakland (con James Yimm Lee, 1964)", "Dojo", "Jun Fan Gung Fu Institute (Oakland, 1964)", Vector3.zero, 0f, new Vector2(14f, 10f), Linoleum,
            S("tatami", 0f, 0f, 5f, 4f, "Suelo de entrenar"), S("muneco", 0f, 0f, 1f, 1f, "Muñeco de madera")), us + new Vector2(40f, -40f), placedUS);
        var laSchool = Place(t, lots, Building(t, "Escuela de Chinatown (628 College Street, Los Ángeles, 1967)", "Dojo", "Jun Fan Gung Fu Institute (Los Ángeles, 1967)", Vector3.zero, 0f, new Vector2(14f, 10f), Linoleum,
            S("tatami", 0f, 0f, 5f, 4f, "Suelo de entrenar (Jeet Kune Do)"), S("muneco", 0f, 0f, 1f, 1f, "Muñeco de madera")), us + new Vector2(80f, 0f), placedUS);
        var seattleHome = Place(t, lots, Building(t, "Casa de madera de Seattle", "Casa", "Casa de Seattle", Vector3.zero, 0f, new Vector2(12f, 10f), Clapboard,
            S("cama", 0f, 0f, 1f, 2f, "Cama")), us + new Vector2(80f, 40f), placedUS);

        // Calles de cada barrio: el asfalto bajo sus solares (a ras de suelo) y, en Kowloon, banianos de sombra.
        Street(t, "Calles de Kowloon (Nathan Road, Temple Street)", placedHK, C(0.36f, 0.36f, 0.37f));
        Street(t, "Calles de Seattle (Broadway, University Way)", placedUS, C(0.33f, 0.33f, 0.35f));
        for (int k = 0; k < 6; k++)
        {
            Vector3? at = lots.Take(new Vector2(4f, 4f), hk + new Vector2(-20f + (k % 3) * 40f, k < 3 ? 15f : -15f));
            if (at.HasValue) Banyan(t, at.Value);
        }
        // El resto del piso: más bloques de tong lau y casas de madera, vacíos de gente (la ciudad sigue).
        int fill = FillTown(t, lots, hk, TongLau, "Tong lau", 8) + FillTown(t, lots, us, Clapboard, "Casa de madera", 6);

        int n = 0;
        n += Worker(t, "Alumno de Wing Chun 1", school, "alumno", cafe, school, flat);
        n += Worker(t, "Alumno de Wing Chun 2", school, "alumno", cafe, school, tong1);
        n += Worker(t, "Wong Shun-leung", school, "maestro", cafe, school, tong1);
        n += Worker(t, "Cocinero del cha chaan teng", cafe, "cocinero", cafe, school, tong1);
        n += Worker(t, "Camarera del cha chaan teng", cafe, "camarero", cafe, school, flat);
        n += Worker(t, "Maestro de St. Francis Xavier", college, "maestro", cafe, null, flat);
        n += Worker(t, "Vendedora de Temple Street", market, "comerciante", cafe, temple, tong1);
        n += Worker(t, "Ruby Chow", ruby, "camarero", ruby, null, seattleHome);
        n += Worker(t, "Cocinero de Ruby Chow", ruby, "cocinero", ruby, null, seattleHome);
        n += Worker(t, "Profesora de filosofía", uw, "sabio", ruby, null, seattleHome);
        n += Worker(t, "Alumno del Jun Fan", jfgi, "alumno", ruby, jfgi, room);
        n += Worker(t, "Alumna de Oakland", oakland, "alumno", ruby, oakland, seattleHome);
        n += Worker(t, "Alumno de Chinatown", laSchool, "alumno", ruby, laSchool, seattleHome);
        n += Couple(t, "Vecina del tong lau", "Vecino del tong lau", tong1, cafe, school);
        Debug.Log($"[Eras] Bruce Lee: Kowloon {placedHK.Count} edificios, Seattle/Oakland/L. A. {placedUS.Count}, {fill} bloques de relleno; piso útil {fl.width:0} × {fl.height:0} m.");
        return n;
    }

    // ── Piezas del pueblo (las reglas de edificación) ───────────────────────────────────────────────────────────────

    /// <summary>Lleva el edificio <paramref name="site"/> (recién hecho, en el origen) al solar libre más cercano a
    /// <paramref name="near"/> de su tamaño (el de su «Suelo») y lo apunta en <paramref name="placed"/>.</summary>
    internal static AreaFunctionSite Place(Transform t, Lots lots, AreaFunctionSite site, Vector2 near, List<Transform> placed = null)
    {
        Transform floor = site.transform.Find("Suelo");
        Vector2 size = floor != null ? new Vector2(floor.localScale.x, floor.localScale.z) : new Vector2(12f, 10f);
        if (Mathf.Abs(Mathf.DeltaAngle(site.transform.localEulerAngles.y, 90f)) < 45f || Mathf.Abs(Mathf.DeltaAngle(site.transform.localEulerAngles.y, 270f)) < 45f) size = new Vector2(size.y, size.x);
        Vector3? at = lots.Take(size + new Vector2(2f, 2f), near);   // con la columnata (si la tiene) dentro
        if (at.HasValue) site.transform.localPosition = at.Value;
        else { site.transform.localPosition = new Vector3(near.x, 0f, near.y); Debug.LogWarning($"[Eras] «{site.title}»: no hay solar libre de {size.x:0} × {size.y:0} m; se queda en ({near.x:0}, {near.y:0})."); }
        placed?.Add(site.transform);
        return site;
    }

    /// <summary>El asfalto (o el empedrado) bajo los solares de un barrio, a ras de suelo: la calle que los une.</summary>
    internal static void Street(Transform t, string name, List<Transform> placed, Color c)
    {
        if (placed.Count == 0) return;
        Bounds b = new Bounds(placed[0].localPosition, Vector3.zero);
        foreach (Transform p in placed)
        {
            Transform f = p.Find("Suelo"); Vector3 s = f != null ? f.localScale : new Vector3(12f, 0f, 10f);
            b.Encapsulate(new Bounds(p.localPosition, new Vector3(s.x, 0f, s.z)));
        }
        b.Expand(new Vector3(MicroBuildRules.Street * 2f, 0f, MicroBuildRules.Street * 2f));
        AreaMicrocosmos.Box(t, name, new Vector3(b.center.x, 0.01f, b.center.z), new Vector3(b.size.x, 0.02f, b.size.z), AreaMicrocosmos.Mat("Civ_Calle_" + Hex(c), c), false);
    }

    /// <summary>Un BANIANO de calle (Ficus microcarpa): tronco, raíces aéreas y copa alta (a 6 m: no tapa la vista a pie).</summary>
    internal static void Banyan(Transform t, Vector3 at)
    {
        Transform b = new GameObject("Baniano (Ficus microcarpa)").transform; b.SetParent(t, false); b.localPosition = at;
        Material bark = AreaMicrocosmos.Mat("Civ_Corteza_baniano", C(0.45f, 0.4f, 0.34f)), leaf = AreaMicrocosmos.Mat("Civ_Hoja_baniano", C(0.22f, 0.42f, 0.2f));
        AreaMicrocosmos.Box(b, "Tronco", new Vector3(0f, 3.5f, 0f), new Vector3(1f, 7f, 1f), bark, true);
        for (int k = 0; k < 6; k++) AreaMicrocosmos.Box(b, "Raiz_aerea", Quaternion.Euler(0f, k * 60f, 0f) * new Vector3(0f, 3f, 1.4f), new Vector3(0.08f, 6f, 0.08f), bark, false);
        for (int k = 0; k < 3; k++) AreaMicrocosmos.Box(b, "Copa", new Vector3(0f, 7f + k * 0.9f, 0f), new Vector3(9f - k * 2.5f, 1f, 9f - k * 2.5f), Quaternion.Euler(0f, k * 20f, 0f), leaf, false);
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = "Baniano"; wl.height = 9.5f;
    }

    /// <summary>
    /// El RELLENO DEL PUEBLO (regla 5): hasta <paramref name="count"/> casas más de la era («Casa» del registro, con sus
    /// necesidades básicas) en los solares libres más cercanos a <paramref name="near"/>. Están vacías de gente (la ciudad
    /// sigue más allá de la historia). Devuelve cuántas puso.
    /// </summary>
    internal static int FillTown(Transform t, Lots lots, Vector2 near, Style st, string title, int count)
    {
        int n = 0;
        for (int k = 0; k < count; k++)
        {
            AreaFunctionSite h = Building(t, $"{title} {k + 1}", "Casa", title, Vector3.zero, 0f, new Vector2(12f, 10f), st, S("cama", 0f, 0f, 1f, 2f, NeedLabel("cama", Year)));
            Transform f = h.transform.Find("Suelo");
            Vector3? at = lots.Take(new Vector2(f.localScale.x + 2f, f.localScale.z + 2f), near);
            if (!at.HasValue) { Object.DestroyImmediate(h.gameObject); break; }
            h.transform.localPosition = at.Value; n++;
        }
        return n;
    }
}
