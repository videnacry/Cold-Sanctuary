using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA LANZADERA DE COHETES (Berón 2026-11-04; docs/lanzadera-de-cohetes.md): un CASQUETE ARTIFICIAL lejos del santuario (≥ 300 m de
/// cualquier edificio o isla: la distancia de edificio habitado para la explosión de un cohete de la clase Electron) con un
/// EDIFICIO-ÁRBOL REFORZADO que sostiene la plataforma por encima de todo lo demás (<see cref="LaunchHeightRule"/>): su corona lleva el
/// cohete, el deflector y la trinchera de llamas, el diluvio de agua, la cubeta de contención y el escudo contra cascotes. Pisos:
/// «Fábrica de cohetes» (con la plaza del submarino de obra) y «Control de misión» (con sus puestos); el piso libre de arriba,
/// garaje de la lanzadera.
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>Centro del casquete artificial: al oeste, en mar abierto (el mar llega a ±600 m).</summary>
    public static readonly Vector3 LaunchSiteCenter = new Vector3(-560f, 0f, 0f);
    const float LaunchCapHalf = 55f, LaunchPadRise = 12f;
    static Helix _launchHx;

    /// <summary>Empieza la lanzadera: el casquete, el árbol reforzado (con la altura que pide la regla) y sus dos pisos amueblados.
    /// La corona se pone después de terminar los árboles (<see cref="FinishLaunchSite"/>).</summary>
    static void BuildLaunchSite(Transform parent)
    {
        Transform root = new GameObject("Lanzadera_AUTO").transform; root.SetParent(parent, false);
        Vector3 site = LaunchSiteCenter;
        // ── Distancia al santuario ──
        float nearest = NearestToSite(site, LaunchHeightRule.MinDistance + LaunchCapHalf, null, out string near);
        if (nearest < LaunchHeightRule.MinDistance) Debug.LogError($"[Lanzadera] Está a {nearest:0} m de {near}: la regla pide ≥ {LaunchHeightRule.MinDistance:0} m.");
        else Debug.Log($"[Lanzadera] A {nearest:0} m de lo más cercano del santuario ({near}); distancia de edificio habitado {LaunchHeightRule.InhabitedBuildingDistance:0} m (+50 % por cascotes: {LaunchHeightRule.MinDistance:0} m).");

        // ── La altura (la regla) ──
        float need = LaunchHeightRule.RequiredPadHeight(out string why);
        // Árbol de 2 niveles: techo = primer piso + 22 + 23 m (HelixTower.RootHeight); la corona, 12 m sobre el techo.
        float firstY = Mathf.Max(TreeSpec.Helix.firstY, Mathf.Ceil(need - LaunchPadRise - 45f));
        HelixTower h = TowerHelix(new Vector2(32f, 32f), 2, firstY, 10f);
        Helix hx = BeginHelix(root, LaunchHeightRule.Building, "Lanzadera_cohetes", site, 0f, h, rotates: false);
        hx.launch = true; hx.pickDoors = true;
        hx.tipContent = (k, tip) => { tip.name = $"Mirador_punta_{k}"; return "mirador"; };
        hx.santuarioLabel = $"{LaunchHeightRule.Building}: Fábrica de cohetes, Control de misión y la plataforma de lanzamiento";
        _launchHx = hx;
        Debug.Log($"[Lanzadera] La regla pide la plataforma a {why}: primer piso a {firstY:0} m, techo a {h.RootHeight:0} m, plataforma a {h.RootHeight + LaunchPadRise:0} m.");

        // ── El casquete artificial: 110 × 110 m alrededor del hueco del tronco (el submarino entra por debajo) ──
        BuildArtificialCap(root, hx, site, LaunchCapHalf);

        // ── Pisos: Fábrica de cohetes (P1) y Control de misión (P2) ──
        Transform fab = HelixFloor(hx, 1, null, new Vector2(h.floorSize, h.floorSize), "Fabrica_de_cohetes");
        FurnishRocketFactory(fab, h.floorSize);
        Transform ctl = HelixFloor(hx, 2, null, new Vector2(h.floorSize, h.floorSize), "Control_de_mision");
        FurnishMissionControl(ctl, h.floorSize);
        Transform gar = HelixFloor(hx, 5, null, new Vector2(h.floorSize, h.floorSize), "Garaje_lanzadera");
        SubmarineBay(gar, "Garaje de la lanzadera: el submarino de obra trae las etapas y la carga útil por la vía marina");
    }

    /// <summary>Lo más cerca de <paramref name="site"/>: los troncos de los edificios-árbol y el hielo de las islas (hasta <paramref name="maxR"/>).</summary>
    static float NearestToSite(Vector3 site, float maxR, Helix skip, out string near)
    {
        float nearest = float.MaxValue; near = "nada";
        foreach (Helix o in _helixes)
        {
            if (o == skip) continue;
            float d = Vector2.Distance(new Vector2(o.site.x, o.site.z), new Vector2(site.x, site.z));
            if (d < nearest) { nearest = d; near = o.building; }
        }
        for (float a = 0f; a < 360f; a += 5f)
            for (float r = 0f; r < maxR; r += 5f)
            {
                Vector3 p = site + TreeSpec.Dir(a) * r;
                if (PolarLandscapeBuilder.NaturalIce(p.x, p.z)) { if (r < nearest) { nearest = r; near = "una isla del santuario"; } break; }
            }
        return nearest;
    }

    /// <summary>Un CASQUETE ARTIFICIAL de 2·<paramref name="H"/> m de lado alrededor del hueco del tronco (el submarino entra por debajo), con su pretil.</summary>
    static void BuildArtificialCap(Transform root, Helix hx, Vector3 site, float H)
    {
        Transform cap = new GameObject("Casquete_artificial").transform; cap.SetParent(root, false); cap.position = site;
        Color capC = new Color(0.82f, 0.85f, 0.88f);
        float hole = hx.h.waterHalf - 0.2f;
        foreach (var (x0, x1, z0, z1) in new[] { (-H, -hole, -H, H), (hole, H, -H, H), (-hole, hole, -H, -hole), (-hole, hole, hole, H) })
            Solid(cap, new Vector3((x0 + x1) * 0.5f, -0.75f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, 1.5f, z1 - z0), capC, "Casquete_artificial");
        for (int s = 0; s < 4; s++)   // borde de defensa (contra el oleaje y para contener vertidos)
            KitchenProps.C(cap, Quaternion.Euler(0f, s * 90f, 0f) * new Vector3(0f, 0.6f, H - 0.5f), new Vector3(2f * H, 1.2f, 1f), new Color(0.55f, 0.57f, 0.6f), s * 90f, "Pretil_casquete");
        // Con el mar a su hondura real, un casquete no se queda solo en su sitio: 4 tendones al lecho, como una plataforma de patas tensadas (docs/sitio-nuclear.md §9).
        float tendon = -SeaRule.FloorY;
        for (int a = -1; a <= 1; a += 2)
            for (int b = -1; b <= 1; b += 2)
                PolarLandscapeBuilder.IgnoreForNav(KitchenProps.C(cap, new Vector3(a * (H - 6f), -tendon * 0.5f - 1f, b * (H - 6f)), new Vector3(0.8f, tendon, 0.8f), new Color(0.3f, 0.32f, 0.35f), 0f, "Tendon_casquete").gameObject);
        // LA DETECCIÓN (docs/sitio-nuclear.md §14): radar en un mástil y sonar de quillas bajo el borde, en cada casquete. Lo que no se ve no se remolca.
        Vector3 mast = new Vector3(H - 8f, 0f, 0f);
        KitchenProps.C(cap, mast + new Vector3(0f, 5f, 0f), new Vector3(0.3f, 10f, 0.3f), TreeSteel, 0f, "Mastil_radar");
        KitchenProps.C(cap, mast + new Vector3(0f, 10.2f, 0f), new Vector3(2.4f, 0.4f, 0.4f), new Color(0.25f, 0.27f, 0.3f), 0f, "Radar_hielo");
        KitchenProps.C(cap, mast + new Vector3(0f, -6f, 0f), new Vector3(1f, 2f, 1f), new Color(0.95f, 0.8f, 0.2f), 0f, "Sonar_quillas");
        hx.ac.bottomHole = hole;
    }

    // La FÁBRICA DE COHETES: dos etapas en sus cunas, la carga útil, el puente grúa y la plaza del submarino.
    static void FurnishRocketFactory(Transform f, float S)
    {
        Color white = new Color(0.86f, 0.87f, 0.88f), steel = TreeSteel, dark = new Color(0.2f, 0.21f, 0.23f), yel = new Color(0.95f, 0.75f, 0.1f);
        for (int i = 0; i < 2; i++)
        {
            Transform st = new GameObject(i == 0 ? "Etapa_1" : "Etapa_2").transform; st.SetParent(f, false); st.localPosition = new Vector3(-7f + i * 6f, 0f, 6f);
            float len = i == 0 ? 12f : 2.4f;
            Transform body = KitchenProps.C(st, new Vector3(0f, 1.3f, 0f), new Vector3(1.2f, 1.2f, len), white, 0f, "Fuselaje");
            KitchenProps.C(st, new Vector3(0f, 0.35f, -len * 0.3f), new Vector3(1.6f, 0.7f, 0.4f), steel, 0f, "Cuna");
            KitchenProps.C(st, new Vector3(0f, 0.35f, len * 0.3f), new Vector3(1.6f, 0.7f, 0.4f), steel, 0f, "Cuna");
            if (i == 0) KitchenProps.C(st, new Vector3(0f, 1.3f, -len * 0.5f - 0.5f), new Vector3(1.1f, 1.1f, 1f), dark, 0f, "Motores");
        }
        KitchenProps.C(f, new Vector3(5f, 1.2f, 6f), new Vector3(1.2f, 2.4f, 1.2f), new Color(0.9f, 0.9f, 0.92f), 0f, "Carga_util_cofia");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(f, new Vector3(s * (S * 0.5f - 1.5f), 4f, 4f), new Vector3(0.4f, 8f, 0.4f), steel, 0f, "Columna_grua");
        KitchenProps.C(f, new Vector3(0f, 8f, 4f), new Vector3(S - 3f, 0.5f, 0.6f), yel, 0f, "Grua_puente");   // (no «Puente_…»: SanctuaryLifeTest busca puentes entre plataformas por ese prefijo)
        Label(KitchenProps.C(f, new Vector3(0f, 9f, 4f), Vector3.one * 0.1f, yel, 0f, "Cartel_fabrica"), $"{LaunchSite.FactoryFloor}: etapas, carga útil e integración (ver el microcosmos de Musk)", 1f);
        SubmarineBay(f, null, new Vector3(0f, 0.02f, -6f));
    }

    // CONTROL DE MISIÓN: tres filas de puestos con sus pantallas, la pantalla grande y el puesto del director de vuelo.
    static void FurnishMissionControl(Transform f, float S)
    {
        Color desk = new Color(0.32f, 0.34f, 0.38f), screen = new Color(0.12f, 0.35f, 0.45f), chair = new Color(0.18f, 0.18f, 0.2f);
        // (2026-11-04) Su «Mat» (el suelo del área, del tamaño del piso): con él, el tanque de virtualización del área busca su sitio con 2 m
        // libres en todo el piso de 32 m; sin él lo buscaba en 10 × 10 m, entre los puestos (prueba 28: 0,00–0,91 m).
        KitchenProps.C(f, new Vector3(0f, 0.01f, 0f), new Vector3(S - 1f, 0.02f, S - 1f), new Color(0.3f, 0.32f, 0.35f), 0f, "Mat");
        for (int row = 0; row < 3; row++)
            for (int i = 0; i < 4; i++)
            {
                Transform p = new GameObject("Puesto_control").transform; p.SetParent(f, false);
                p.localPosition = new Vector3(-7.5f + i * 5f, 0f, -2f + row * 4f);
                KitchenProps.C(p, new Vector3(0f, 0.38f, 0f), new Vector3(2.2f, 0.76f, 0.9f), desk, 0f, "Mesa_control");
                KitchenProps.C(p, new Vector3(0f, 1.05f, 0.25f), new Vector3(1.8f, 0.6f, 0.06f), screen, 0f, "Pantallas");
                KitchenProps.C(p, new Vector3(0f, 0.45f, -0.9f), new Vector3(0.6f, 0.9f, 0.6f), chair, 0f, "Silla");
            }
        KitchenProps.C(f, new Vector3(0f, 3.5f, 11f), new Vector3(16f, 5f, 0.3f), screen, 0f, "Pantalla_grande");
        Label(KitchenProps.C(f, new Vector3(0f, 6.6f, 11f), Vector3.one * 0.1f, screen, 0f, "Cartel_control"), $"{LaunchSite.ControlArea}: telemetría, meteorología y la cuenta atrás", 1f);
        SanctuaryArea a = f.gameObject.AddComponent<SanctuaryArea>();
        a.areaType = SanctuaryAreaType.LaunchControl; a.displayName = LaunchSite.ControlArea;
        a.description = "Control de misión de la lanzadera de cohetes (docs/lanzadera-de-cohetes.md)";
    }

    // Una plaza pintada para el submarino de obra (14 × 5 m + 1 m alrededor).
    static void SubmarineBay(Transform deck, string sign, Vector3? at = null)
    {
        Color paint = new Color(0.98f, 0.82f, 0.1f);
        Transform g = new GameObject("Plaza_submarino").transform; g.SetParent(deck, false); g.localPosition = at ?? new Vector3(0f, 0.02f, 2f);
        float L = TreeSpec.VehicleLength + 2f, B = TreeSpec.VehicleBeam + 2f;
        foreach (float s in new[] { -1f, 1f })
        {
            KitchenProps.C(g, new Vector3(s * B * 0.5f, 0f, 0f), new Vector3(0.25f, 0.02f, L), paint, 0f, "Linea_plaza");
            KitchenProps.C(g, new Vector3(0f, 0f, s * L * 0.5f), new Vector3(B, 0.02f, 0.25f), paint, 0f, "Linea_plaza");
        }
        if (sign != null) Label(KitchenProps.C(g, new Vector3(B * 0.5f + 1.5f, 2.5f, 0f), Vector3.one * 0.1f, paint, 0f, "Cartel_garaje"), sign, 0.6f);
        PolarLandscapeBuilder.IgnoreForNav(g.gameObject);
    }

    /// <summary>Con el árbol terminado: la CORONA DE LANZAMIENTO sobre su techo (pilares reforzados, plataforma, cohete, deflector y
    /// trinchera de llamas, diluvio, cubeta de contención y escudo contra cascotes) y la primera validación de la regla.</summary>
    static void FinishLaunchSite()
    {
        Helix hx = _launchHx; _launchHx = null;
        if (hx == null || hx.ac == null || hx.ac.top == null) return;
        Transform tree = hx.root;
        float roofY = tree.position.y + hx.RootHeight, padY = roofY + LaunchPadRise;
        Transform site = new GameObject("Corona_lanzamiento").transform; site.SetParent(tree, false);
        LaunchSite ls = site.gameObject.AddComponent<LaunchSite>();
        ls.roofY = roofY;
        Color conc = new Color(0.6f, 0.6f, 0.58f), steel = TreeSteel, red = new Color(0.7f, 0.18f, 0.12f), water = new Color(0.25f, 0.5f, 0.7f, 0.5f);
        // Pilares (4, de hormigón armado y acero: 3 × 3 m) del techo a la plataforma.
        foreach (Vector2 c in new[] { new Vector2(-9f, -9f), new Vector2(9f, -9f), new Vector2(-9f, 9f), new Vector2(9f, 9f) })
        {
            Transform p = Solid(tree, Vector3.zero, new Vector3(3f, LaunchPadRise, 3f), conc, "Pilar_corona");
            Vector3 w = tree.TransformPoint(new Vector3(c.x, 0f, c.y)); p.position = new Vector3(w.x, roofY + LaunchPadRise * 0.5f, w.z);
            ls.pillars.Add(p);
        }
        Transform deck = new GameObject("Plataforma").transform; deck.SetParent(site, false); deck.position = new Vector3(tree.position.x, padY, tree.position.z); deck.rotation = tree.rotation;
        ls.deck = deck;
        // Losa de 24 × 24 × 2 m con el hueco de la llama (4 × 4 m) en el centro.
        float S = 12f, o = 2f;
        foreach (var (x0, x1, z0, z1) in new[] { (-S, -o, -S, S), (o, S, -S, S), (-o, o, -S, -o), (-o, o, o, S) })
            Solid(deck, new Vector3((x0 + x1) * 0.5f, -1f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, 2f, z1 - z0), conc, "Losa_plataforma");
        // DEFLECTOR (cuña de acero refrigerada) bajo el hueco y TRINCHERA DE LLAMAS: el chorro baja, la cuña lo tuerce y sale por un
        // conducto de 4 × 4 m hacia el lado del mar abierto (oeste), lejos del santuario.
        Transform defl = KitchenProps.C(deck, new Vector3(0f, -4.5f, 0f), new Vector3(4f, 0.6f, 5.6f), steel, Quaternion.Euler(45f, 270f, 0f), "Deflector_llamas");
        KitchenProps.C(deck, new Vector3(-8f, -5.5f, 0f), new Vector3(12f, 0.4f, 4.4f), conc, 0f, "Trinchera_llamas_suelo");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(deck, new Vector3(-6f, -4f, s * 2.2f), new Vector3(16f, 3.4f, 0.4f), conc, 0f, "Trinchera_llamas_pared");
        KitchenProps.C(deck, new Vector3(-6f, -2.2f, 0f), new Vector3(16f, 0.4f, 4.8f), conc, 0f, "Trinchera_llamas_techo");
        // DILUVIO DE AGUA (supresión de sonido y refrigeración): depósito elevado, bajante y aros de rociadores alrededor del hueco.
        KitchenProps.C(deck, new Vector3(9f, 6f, 9f), new Vector3(5f, 8f, 5f), new Color(0.75f, 0.77f, 0.8f), 0f, "Deposito_diluvio");
        KitchenProps.C(deck, new Vector3(9f, 1f, 6f), new Vector3(0.6f, 2f, 0.6f), steel, 0f, "Bajante_diluvio");
        for (int i = 0; i < 8; i++) { float ang = i * 45f * Mathf.Deg2Rad; KitchenProps.C(deck, new Vector3(Mathf.Cos(ang) * 3f, 0.2f, Mathf.Sin(ang) * 3f), new Vector3(0.3f, 0.3f, 0.3f), steel, 0f, "Rociador_diluvio"); }
        KitchenProps.C(deck, new Vector3(0f, -0.05f, 0f), new Vector3(4f, 0.05f, 4f), water, 0f, "Cortina_agua");
        // CUBETA DE CONTENCIÓN (recoge combustible y agua sin que caigan al mar: va a la planta de tratamiento) y ESCUDO CONTRA CASCOTES
        // del lado del santuario (este), 10 m de alto.
        for (int s = 0; s < 4; s++) KitchenProps.C(deck, Quaternion.Euler(0f, s * 90f, 0f) * new Vector3(0f, 0.75f, S - 0.25f), new Vector3(2f * S, 1.5f, 0.5f), conc, s * 90f, "Cubeta_contencion");
        Solid(deck, new Vector3(S + 0.75f, 5f, 0f), new Vector3(1.5f, 10f, 2f * S), conc, "Escudo_cascotes");
        // ESCUDO: losa de hormigón con contrafuertes, zunchos de acero y remate (ronda 7: era una pared lisa).
        for (int i = -5; i <= 5; i++) KitchenProps.C(deck, new Vector3(S - 0.2f, 5f, i * 2.3f), new Vector3(0.5f, 10f, 0.5f), steel, 0f, "Contrafuerte_escudo");
        foreach (float y in new[] { 3.2f, 6.6f }) KitchenProps.C(deck, new Vector3(S - 0.1f, y, 0f), new Vector3(0.3f, 0.35f, 2f * S), steel, 0f, "Zuncho_escudo");
        KitchenProps.C(deck, new Vector3(S + 0.75f, 10.15f, 0f), new Vector3(2.1f, 0.3f, 2f * S + 0.4f), steel, 0f, "Remate_escudo");
        // EL COHETE (clase Electron: 18 m de alto, 1,2 m de diámetro, ~13 t) en su mesa de lanzamiento, y la torre umbilical.
        Transform rk = new GameObject("Cohete").transform; rk.SetParent(deck, false); rk.localPosition = new Vector3(0f, 1.2f, 0f);
        RocketModel.Build(rk);
        // Mesa: cuatro vigas del borde del hueco a la base del cohete, con su pinza.
        for (int s = 0; s < 4; s++)
        {
            Quaternion q = Quaternion.Euler(0f, s * 90f, 0f);
            KitchenProps.C(deck, q * new Vector3(0f, 0.35f, 1.55f), new Vector3(0.5f, 0.5f, 1.9f), steel, s * 90f, "Viga_mesa");
            KitchenProps.C(rk, q * new Vector3(0f, -0.1f, 0.72f), new Vector3(0.35f, 1.2f, 0.3f), red, s * 90f, "Pinza_mesa");
        }
        // TORRE UMBILICAL: celosía de cuatro patas con riostras y plataformas, a franjas rojas y blancas (ronda 7: era una barra roja).
        Transform um = new GameObject("Torre_umbilical").transform; um.SetParent(deck, false); um.localPosition = new Vector3(4.5f, 0f, -4.5f);
        foreach (Vector2 c in new[] { new Vector2(-0.8f, -0.8f), new Vector2(0.8f, -0.8f), new Vector2(-0.8f, 0.8f), new Vector2(0.8f, 0.8f) })
            KitchenProps.C(um, new Vector3(c.x, 11f, c.y), new Vector3(0.22f, 22f, 0.22f), steel, 0f, "Pata_torre");
        for (int k = 0; k < 11; k++)
        {
            float y = 1f + k * 2f;
            Color band = k % 2 == 0 ? red : new Color(0.85f, 0.85f, 0.83f);
            for (int s = 0; s < 4; s++)
            {
                Quaternion q = Quaternion.Euler(0f, s * 90f, 0f);
                KitchenProps.C(um, q * new Vector3(0f, y, 0.8f), new Vector3(1.6f, 0.14f, 0.14f), band, s * 90f, "Riostra_torre");
                KitchenProps.C(um, q * new Vector3(0f, y + 1f, 0.8f), new Vector3(2.5f, 0.09f, 0.09f), steel, Quaternion.Euler(0f, s * 90f, k % 2 == 0 ? 51f : -51f), "Diagonal_torre");
            }
            if (k % 3 == 1) KitchenProps.C(um, new Vector3(0f, y + 0.08f, 0f), new Vector3(1.7f, 0.06f, 1.7f), steel, 0f, "Plataforma_torre");
        }
        KitchenProps.C(um, new Vector3(0f, 22.2f, 0f), new Vector3(1.9f, 0.4f, 1.9f), red, 0f, "Remate_torre");
        foreach (float y in new[] { 13f, 8f }) KitchenProps.C(um, new Vector3(-2f, y, 2f), new Vector3(0.3f, 0.3f, 5f), steel, -45f, "Brazo_umbilical");
        Label(KitchenProps.C(deck, new Vector3(0f, 24f, 0f), Vector3.one * 0.1f, red, 0f, "Cartel_plataforma"), "Plataforma de lanzamiento (regla de altura: docs/lanzadera-de-cohetes.md)", 2f);
        // LOS PROPULSORES Y LOS PIROTÉCNICOS (docs/sitio-nuclear.md §6): al aire, en el casquete y lejos del árbol y entre sí. Bajo tierra no:
        // el oxígeno y los vapores se acumulan y una explosión confinada es peor. La fábrica se queda arriba: allí las etapas están vacías.
        Vector3 cc = tree.position;
        Mesh sphere = RocketModel.Lathe("Deposito_esfera", new[] { (0f, 0f), (2.6f, 0.7f), (3.8f, 2.4f), (4.2f, 4.5f), (3.8f, 6.6f), (2.6f, 8.3f), (0f, 9f) }, 12);
        foreach (var (n, x, z, c) in new[] { ("Deposito_LOX", -42f, 42f, new Color(0.82f, 0.86f, 0.9f)), ("Deposito_RP1", -42f, -42f, new Color(0.7f, 0.62f, 0.45f)) })
        {
            Transform d = new GameObject(n).transform; d.SetParent(site, false); d.position = cc + new Vector3(x, 0.3f, z);
            d.gameObject.AddComponent<MeshFilter>().sharedMesh = sphere;
            d.gameObject.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(c);
            for (int s = 0; s < 4; s++) KitchenProps.C(d, Quaternion.Euler(0f, s * 90f, 0f) * new Vector3(0f, 0.5f, 6f), new Vector3(13f, 1f, 0.5f), conc, s * 90f, "Cubeta_deposito");
            Label(d, n == "Deposito_LOX" ? "Oxígeno líquido, al aire y aparte del RP-1" : "Queroseno RP-1, al aire y aparte del oxígeno", 10.5f);
        }
        Transform mag = new GameObject("Polvorin_pirotecnicos").transform; mag.SetParent(site, false); mag.position = cc + new Vector3(42f, 0f, 42f);
        KitchenProps.C(mag, new Vector3(0f, 1.3f, 0f), new Vector3(6f, 2.6f, 4f), conc, 0f, "Bunker_polvorin");
        KitchenProps.C(mag, new Vector3(0f, 1f, -3.5f), new Vector3(7f, 2f, 0.8f), conc, 0f, "Muro_polvorin");
        Label(mag, "Polvorín: las cargas de separación de etapas, lejos de todo", 3.5f);
        // Alfombras a la plataforma (no hay escaleras).
        CarpetPad.BuildZone(deck, deck.TransformPoint(new Vector3(-7f, 0.03f, -7f)), deck.rotation, CarpetPad.Kind.Edificio, hx.building, hx.Platforms + 1, $"{hx.building}#plataforma", "Plataforma de lanzamiento", 2);
        foreach (Transform t in deck.GetComponentsInChildren<Transform>(true)) if (t.name.StartsWith("Trinchera") || t.name.StartsWith("Deflector") || t.name.StartsWith("Cortina")) PolarLandscapeBuilder.IgnoreForNav(t.gameObject);
        ValidateLaunchHeight("al terminar la lanzadera");
    }

    /// <summary>LA REGLA (docs/lanzadera-de-cohetes.md): con todo lo elevado ya puesto, la plataforma ≥ máx(lo más alto, aves) + margen.
    /// Si no, FALLA (error en el registro) y la sube. Se llama al terminar la lanzadera y al final del Blockout.</summary>
    static void ValidateLaunchHeight(string when)
    {
        LaunchSite ls = Object.FindAnyObjectByType<LaunchSite>();
        if (ls == null) return;
        float need = LaunchHeightRule.RequiredPadHeight(out string why);
        if (ls.PadY + 0.01f >= need) { Debug.Log($"[Lanzadera] Regla de altura ({when}): la plataforma, a {ls.PadY:0} m, cumple {why}."); return; }
        Debug.LogError($"[Lanzadera] Regla de altura ({when}): la plataforma está a {ls.PadY:0} m y la regla pide {why}. Se SUBE la corona.");
        ls.RaiseTo(need);
    }
}
