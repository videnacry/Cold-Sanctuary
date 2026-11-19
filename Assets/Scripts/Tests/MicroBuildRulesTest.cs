using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TEST DEL VALIDADOR DE LAS REGLAS DE EDIFICACIÓN (docs/microcosmos-reglas-edificacion.md, <see cref="MicroBuildRules"/>), sin
/// cargar escenas: monta casos pequeños y mira que el validador vea lo que debe ver.
/// (2026-11-18, docs/plan-microcosmos-realismo.md §3: los insectos-humanos a la mitad y vuelven los muros con vista de corte)
/// - la escala sale de UNA constante, s = 0,5: agente de 0,5 m (medio ancho recogido), paso 4 × 0,5 + 2 × 0,2 = 2,4 m, calle de 4 m,
///   sitio de trabajo 0,6 m, puerta mínima 1,2 × 1,25 m; el santuario sigue con sus 2 m (FurnitureSpacing.Standard);
/// - dos muebles a 1 m: violación; a 2,2 m también (< 2,4), salvo en un microcosmos montado antes del encogimiento (MicroFloor con
///   folkScale = 1: 2 m); a 2,5 m: no; las sillas hijas de su mesa no cuentan; lo que se pisa (tatami) no cuenta;
/// - MUROS Y TECHOS PERMITIDOS: un edificio con un «Muro_…» (o un panel alto y fino, o una losa por encima de las cabezas) pide su
///   vista de corte (CutawayView) y, si tiene muros, su puerta (MicroDoor); con las dos, ninguna falta; una puerta de 0,9 m es falta
///   (P4) y MicroDoor.Place la ensancha a 1,2 m («supuesto»); el borde del piso y la barrera histórica no son edificio;
/// - los consejos (P5 hito, P6 transporte, C4 escaleras por debajo de 12 m) salen en Advise y no son faltas;
/// - un puesto de un solo cubo: violación;
/// - las necesidades básicas: faltan las seis en vacío y ninguna con sus seis muebles;
/// - las eras avanzadas empiezan en −3500 (Uruk).
/// </summary>
public class MicroBuildRulesTest : MonoBehaviour, ITestUnit
{
    public int Group => 63;   // tras las eras civilizadas (62); no carga escenas
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<MicroBuildRulesTest>() == null) tr.gameObject.AddComponent<MicroBuildRulesTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Reglas de edificación de los microcosmos: el validador");
        // 0 · La escala (E1–E4, P4): todo sale de FolkScale.
        float s = MicroBuildRules.FolkScale;
        TestProbe.Check("una sola constante: s = 0,5 (estatura tipo 0,875 m)", Mathf.Approximately(s, 0.5f) && Mathf.Approximately(MicroBuildRules.FolkStature, 1.75f * s), $"s = {s}, {MicroBuildRules.FolkStature:0.000} m");
        TestProbe.Check("el agente mide medio ancho recogido (0,5 m) y el paso, dos cuerpos recogidos + 0,2 m a cada lado (2,4 m)",
            Mathf.Approximately(MicroBuildRules.AgentRadius, 0.5f) && Mathf.Approximately(MicroBuildRules.Passage, 4f * MicroBuildRules.AgentRadius + 2f * MicroBuildRules.SideMargin) && Mathf.Approximately(MicroBuildRules.Passage, 2.4f),
            $"agente {MicroBuildRules.AgentRadius}, paso {MicroBuildRules.Passage}");
        TestProbe.Check("calle de 4 m, sitio de trabajo 0,6 m, velocidad 1,5 m/s; el santuario sigue con sus 2 m",
            Mathf.Approximately(MicroBuildRules.Street, 4f) && Mathf.Approximately(MicroBuildRules.WorkSpot, 0.6f) && Mathf.Approximately(MicroBuildRules.FolkSpeed, 1.5f) && Mathf.Approximately(FurnitureSpacing.Standard, 2f),
            $"calle {MicroBuildRules.Street}, sitio {MicroBuildRules.WorkSpot}, {MicroBuildRules.FolkSpeed} m/s, santuario {FurnitureSpacing.Standard}");
        TestProbe.Check("puerta mínima 1,2 × 1,25 m (el cuerpo recogido andando + 0,2; el alto con antenas + 0,2)",
            Mathf.Approximately(MicroBuildRules.DoorWidth, 1.2f) && Mathf.Abs(MicroBuildRules.DoorHeight - 1.25f) < 0.01f, $"{MicroBuildRules.DoorWidth} × {MicroBuildRules.DoorHeight}");
        TestProbe.Check("era avanzada desde −3500 (Uruk sí, la aldea de −9500 no, 1964 sí)", MicroBuildRules.IsTownEra(-3500) && !MicroBuildRules.IsTownEra(-9500) && MicroBuildRules.IsTownEra(1964));

        var root = new GameObject("PruebaReglas").transform; root.position = new Vector3(0f, -500f, 9000f);
        root.gameObject.AddComponent<MicroFloor>().folkScale = MicroBuildRules.FolkScale;   // montado con el cuerpo encogido: vale el paso nuevo (sin MicroFloor, el de antes)
        try
        {
            // 1 · El paso.
            Transform a = Piece(root, "Mesa A", new Vector3(0f, 0f, 0f), new Vector3(1.4f, 0.75f, 0.8f), "mesa", "comer");
            Transform b = Piece(root, "Mesa B", new Vector3(2.4f, 0f, 0f), new Vector3(1.4f, 0.75f, 0.8f), "mesa", "comer");   // 1,0 m libre
            TestProbe.Check("dos muebles a 1 m: violación de paso", Has(MicroBuildRules.Validate(root, null, false, false), "paso"));
            b.localPosition = new Vector3(3.6f, 0f, 0f);   // 2,2 m libres: con el cuerpo encogido no basta (2,4 m)
            TestProbe.Check("a 2,2 m: violación (el paso nuevo es de 2,4 m)", Has(MicroBuildRules.Validate(root, null, false, false), "paso"));
            var bare = new GameObject("Escena_sin_MicroFloor").transform; bare.position = root.position + new Vector3(0f, 0f, -300f);
            TestProbe.Check("sin MicroFloor (MobWorld, casas de ciclo de vida): el paso de antes, 2 m", Mathf.Approximately(MicroBuildRules.PassageFor(bare), MicroBuildRules.LegacyPassage));
            Object.DestroyImmediate(bare.gameObject);
            var legacy = new GameObject("Microcosmos_antiguo").transform; legacy.SetParent(root, false);
            MicroFloor old = legacy.gameObject.AddComponent<MicroFloor>();   // folkScale = 1 (por defecto): montado antes del encogimiento
            a.SetParent(legacy, true); b.SetParent(legacy, true);
            TestProbe.Check("a 2,2 m en un microcosmos montado antes del encogimiento (folkScale 1): sin violación, vale su paso de 2 m",
                Mathf.Approximately(MicroBuildRules.PassageFor(legacy), MicroBuildRules.LegacyPassage) && !Has(MicroBuildRules.Validate(legacy, null, false, false), "paso"));
            old.folkScale = MicroBuildRules.FolkScale;
            TestProbe.Check("y en cuanto se reconstruye con el cuerpo encogido, sí", Mathf.Approximately(MicroBuildRules.PassageFor(legacy), MicroBuildRules.Passage) && Has(MicroBuildRules.Validate(legacy, null, false, false), "paso"));
            a.SetParent(root, true); b.SetParent(root, true); Object.DestroyImmediate(legacy.gameObject);
            b.localPosition = new Vector3(3.9f, 0f, 0f);   // 2,5 m libres
            TestProbe.Check("a 2,5 m: sin violación", !Has(MicroBuildRules.Validate(root, null, false, false), "paso"));
            Piece(a, "Silla", new Vector3(0f, 0f, -0.6f), new Vector3(0.42f, 0.9f, 0.42f), "silla", "sentarse");   // hija de su mesa
            Piece(root, "Tatami", new Vector3(1.9f, 0f, 1.5f), new Vector3(6f, 0.06f, 2f), "tatami", "", walkable: true);
            TestProbe.Check("la silla de su mesa y el tatami (se pisa) no cuentan para el paso", !Has(MicroBuildRules.Validate(root, null, false, false), "paso"));

            // 2 · Muros y techos PERMITIDOS, con vista de corte y puerta (P2–P4).
            var site = new GameObject("Edificio").transform; site.SetParent(root, false); site.localPosition = new Vector3(20f, 0f, 0f);
            var afs = site.gameObject.AddComponent<AreaFunctionSite>(); afs.building = "Casa"; afs.title = "Casa de prueba";
            Transform wall = Cube(site, "Muro_fondo", new Vector3(0f, 1.6f, 4f), new Vector3(8f, 3.2f, 0.3f));
            List<string> w1 = MicroBuildRules.Validate(site, null, false, false);
            TestProbe.Check("un «Muro_…» en un edificio ya no es falta por sí mismo, pero pide su vista de corte y su puerta",
                !Has(w1, "muro/techo") && Has(w1, "vista de corte") && Has(w1, "sin puerta"), string.Join(" · ", w1));
            wall.name = "Panel";
            TestProbe.Check("un panel de 3,2 m alto y fino, aunque no se llame muro, cuenta como muro (pide corte y puerta)", Has(MicroBuildRules.Validate(site, null, false, false), "sin puerta"));
            wall.name = "Muro_fondo";
            Transform roof = Cube(site, "Losa", new Vector3(0f, 3.4f, 0f), new Vector3(8f, 0.2f, 8f));
            TestProbe.Check("una losa a 3,3 m sobre el suelo del edificio es su techo", Has(MicroBuildRules.Validate(site, null, false, false), "techo («Losa»)"));
            CutawayView cv = CutawayView.Ensure(site);
            TestProbe.Check("la vista de corte reúne el muro y el techo", cv != null && cv.walls.Count == 1 && cv.roofs.Count == 1, cv != null ? $"{cv.walls.Count} muros, {cv.roofs.Count} techos" : "sin componente");
            MicroDoor narrow = MicroDoor.Place(site, "Puerta", new Vector3(0f, 0f, -4f), 0f, 0.9f, 2.0f);
            narrow.width = 0.9f; narrow.widened = false;   // tal cual la real (sin ensanchar)
            List<string> w2 = MicroBuildRules.Validate(site, null, false, false);
            TestProbe.Check("con vista de corte y una puerta real de 0,9 m: falta de puerta estrecha (P4)", !Has(w2, "vista de corte") && !Has(w2, "sin puerta") && Has(w2, "puerta «Puerta»"), string.Join(" · ", w2));
            Object.DestroyImmediate(narrow.gameObject);
            MicroDoor door = MicroDoor.Place(site, "Puerta", new Vector3(0f, 0f, -4f), 0f, 0.9f, 2.0f);
            TestProbe.Check("MicroDoor.Place ensancha la de 0,9 m a 1,2 m y lo anota como supuesto", door.widened && Mathf.Approximately(door.width, MicroBuildRules.DoorWidth) && door.source.Contains("supuesto") && door.Fits(MicroBuildRules.TuckedWalkWidth, 1.05f));
            List<string> w3 = MicroBuildRules.Validate(site, null, false, false);
            TestProbe.Check("con muros, techo, vista de corte y puerta ensanchada: ninguna falta de edificio", !Has(w3, "muro") && !Has(w3, "techo") && !Has(w3, "corte") && !Has(w3, "puerta"), string.Join(" · ", w3));
            Object.DestroyImmediate(cv); Object.DestroyImmediate(door.gameObject); Object.DestroyImmediate(roof.gameObject); Object.DestroyImmediate(wall.gameObject);
            Transform border = new GameObject("Muros_del_piso").transform; border.SetParent(root, false);
            Cube(border, "Muro_N", new Vector3(0f, 40f, 60f), new Vector3(120f, 80f, 4f));
            Transform hist = new GameObject("Muro del gueto").transform; hist.SetParent(root, false);
            var hp = hist.gameObject.AddComponent<MicroPiece>(); hp.kind = MicroBuildRules.Historic; hp.walkable = true;
            Cube(hist, "Muro_del_gueto_N", new Vector3(-30f, 1.75f, 0f), new Vector3(16f, 3.5f, 0.5f));
            List<string> w4 = MicroBuildRules.Validate(root, null, false, false);
            TestProbe.Check("los muros del borde del piso y la barrera histórica no son edificio (ni corte ni puerta)", !Has(w4, "corte") && !Has(w4, "puerta"), string.Join(" · ", w4));

            // P5, P6, C4 · Los consejos: no son faltas.
            var micro = new GameObject("Microcosmos_consejos").transform; micro.SetParent(root, false); micro.localPosition = new Vector3(0f, 0f, 200f);
            micro.gameObject.AddComponent<MicroFloor>().folkScale = MicroBuildRules.FolkScale;
            var adv = new List<string>();
            List<string> w5 = MicroBuildRules.Validate(micro, null, false, false, adv);
            TestProbe.Check("un microcosmos sin hito ni transporte de época: consejo (P5, P6), no falta", Has(adv, "hito") && Has(adv, "transporte") && !Has(w5, "hito") && !Has(w5, "transporte"), string.Join(" · ", adv));
            Piece(micro, "Falcon 1", new Vector3(0f, 0f, 0f), new Vector3(1.7f, 21f, 1.7f), MicroBuildRules.Landmark, "");
            Piece(micro, "Carro", new Vector3(10f, 0f, 0f), new Vector3(2f, 1.5f, 4f), MicroBuildRules.Vehicle, "");
            TestProbe.Check("con su hito y su transporte, sin esos consejos", !Has(MicroBuildRules.Advise(micro), "hito") && !Has(MicroBuildRules.Advise(micro), "transporte"));
            var tower = new GameObject("Casa baja").transform; tower.SetParent(micro, false); tower.localPosition = new Vector3(30f, 0f, 0f);
            var tsite = tower.gameObject.AddComponent<AreaFunctionSite>(); tsite.title = "Casa baja";
            Cube(tower, "Muro_este", new Vector3(3f, 3f, 0f), new Vector3(0.3f, 6f, 6f));
            Cube(tower, "Escalera", new Vector3(0f, 1f, 0f), new Vector3(1f, 2f, 3f));
            TestProbe.Check("una escalera en un edificio de 6 m: consejo C4 (trepan; sin escaleras por debajo de 12 m)", Has(MicroBuildRules.Advise(micro), "C4"));
            Object.DestroyImmediate(micro.gameObject);

            // 3 · Un puesto de un solo cubo.
            Transform lone = Cube(site, "Puesto_fogon", new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 0.9f, 0.8f));
            lone.gameObject.AddComponent<AreaFunctionStation>().station = "fogon";
            TestProbe.Check("un puesto de un solo cubo: violación (mueble sin detalle)", Has(MicroBuildRules.Validate(site, null, false, false), "un solo cubo"));
            Object.DestroyImmediate(lone.gameObject);

            // 4 · Necesidades básicas.
            var empty = new GameObject("Vacio").transform; empty.SetParent(root, false); empty.localPosition = new Vector3(0f, 0f, 50f);
            TestProbe.Check("en vacío faltan las seis necesidades básicas", Count(MicroBuildRules.Validate(empty, null, true, false), "necesidad") == MicroBuildRules.Needs.Length);
            int k = 0;
            foreach (string need in MicroBuildRules.Needs) Piece(empty, need, new Vector3(k++ * 4f, 0f, 0f), new Vector3(1f, 0.8f, 1f), need, need);
            TestProbe.Check("con sus seis muebles, ninguna falta", Count(MicroBuildRules.Validate(empty, null, true, false), "necesidad") == 0);

            // 6 · Hogares (Household.Check): cada persona con hogar; el hogar con una cama por persona (un niño duerme con su
            //     madre), retrete, lavabo, mesa con sillas y despensa.
            var town = new GameObject("Pueblo").transform; town.SetParent(root, false); town.localPosition = new Vector3(0f, 0f, 100f);
            GameObject she = Person(town, "Vecina", true), he = Person(town, "Vecino", false), loner = Person(town, "Solitario", false);
            TestProbe.Check("sin hogar: «… sin hogar» por cada uno", Count(Household.Check(town), "sin hogar") == 3);
            Transform house = new GameObject("Casa").transform; house.SetParent(town, false);
            Transform bed1 = Piece(house, "Cama", new Vector3(0f, 0f, 0f), new Vector3(1f, 0.5f, 2f), "cama", "dormir");
            Transform table = Piece(house, "Mesa", new Vector3(4f, 0f, 0f), new Vector3(1.4f, 0.75f, 0.8f), "mesa", "comer");
            Transform chair = Piece(table, "Silla", new Vector3(0f, 0f, -0.7f), new Vector3(0.42f, 0.9f, 0.42f), "silla", "sentarse");
            Household h = new GameObject("Hogar").AddComponent<Household>(); h.transform.SetParent(house, false);
            h.family = "Vecina y Vecino"; h.beds.Add(bed1); h.table = table; h.seats.Add(chair);
            h.Join(she); h.Join(he);
            List<string> homeProblems = Household.Check(town);
            TestProbe.Check("la pareja con una sola cama, sin retrete, lavabo ni despensa: lo ve", Has(homeProblems, "1 cama(s) para 2") && Has(homeProblems, "sin retrete") && Has(homeProblems, "sin lavabo") && Has(homeProblems, "sin despensa"), string.Join(" · ", homeProblems));
            h.beds.Add(Piece(house, "Cama 2", new Vector3(-4f, 0f, 0f), new Vector3(1f, 0.5f, 2f), "cama", "dormir"));
            h.toilet = Piece(house, "Retrete", new Vector3(8f, 0f, 0f), new Vector3(0.5f, 0.45f, 0.6f), "inodoro", "aseo");
            h.washbasin = Piece(house, "Lavabo", new Vector3(12f, 0f, 0f), new Vector3(0.6f, 0.85f, 0.45f), "lavabo", "lavarse");
            h.pantry = new GameObject("Despensa").AddComponent<FoodContainer>(); h.pantry.transform.SetParent(h.transform, false);
            homeProblems = Household.Check(town);
            TestProbe.Check("completo, la pareja comparte el hogar; solo falta el del solitario", homeProblems.Count == 1 && Has(homeProblems, "Solitario") && Household.Of(she) == h && Household.Of(he) == h, string.Join(" · ", homeProblems));
        }
        finally { Destroy(root.gameObject); }
        yield return null;
        TestProbe.End();
    }

    static Transform Cube(Transform parent, string name, Vector3 local, Vector3 size)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name;
        g.transform.SetParent(parent, false); g.transform.localPosition = local; g.transform.localScale = size;
        return g.transform;
    }

    // Un mueble de varias piezas (dos cubos) con su MicroPiece.
    static Transform Piece(Transform parent, string name, Vector3 local, Vector3 size, string kind, string need, bool walkable = false)
    {
        Transform t = new GameObject(name).transform; t.SetParent(parent, false); t.localPosition = local;
        Cube(t, "Cuerpo", new Vector3(0f, size.y * 0.5f, 0f), size);
        Cube(t, "Detalle", new Vector3(0f, size.y + 0.02f, 0f), new Vector3(size.x * 0.5f, 0.04f, size.z * 0.5f));
        var mp = t.gameObject.AddComponent<MicroPiece>(); mp.kind = kind; mp.need = need; mp.walkable = walkable;
        return t;
    }

    // Una persona (el cuerpo y el ciclo de vida, sin cuerpo visible) con su linaje de insecto-humano.
    static GameObject Person(Transform parent, string name, bool female)
    {
        var g = new GameObject(name); g.transform.SetParent(parent, false);
        g.AddComponent<FolkLineage>().female = female;
        PersonLife pl = PersonLife.Equip(g, female);
        foreach (Behaviour b in g.GetComponents<Behaviour>()) if (!(b is FolkLineage)) b.enabled = false;   // quietos: solo se mira el hogar
        pl.enabled = false;
        return g;
    }

    static bool Has(List<string> l, string what) => Count(l, what) > 0;
    static int Count(List<string> l, string what) { int n = 0; foreach (string s in l) if (s.Contains(what)) n++; return n; }
}
