using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TEST DEL VALIDADOR DE LAS REGLAS DE EDIFICACIÓN (docs/microcosmos-reglas-edificacion.md, <see cref="MicroBuildRules"/>), sin
/// cargar escenas: monta casos pequeños y mira que el validador vea lo que debe ver.
/// - el paso sale del agente: 4 × 0,4 + 2 × 0,2 = 2,0 m (= FurnitureSpacing.Standard);
/// - dos muebles a 1 m: violación; a 2,5 m: no; las sillas hijas de su mesa no cuentan; lo que se pisa (tatami) no cuenta;
/// - un «Muro_…» o una «Cubierta» en un edificio: violación; los muros del borde del piso («Muros_del_piso») y la barrera
///   histórica (MicroPiece «historico») no;
/// - dentro de un edificio, un panel alto y fino es un muro, una losa por encima de las cabezas es un techo y un bloque alto es
///   un edificio macizo;
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
        TestProbe.Check("el paso es de 2,0 m (dos agentes de 0,4 m de radio + 0,2 m a cada lado) y es el estándar del santuario",
            Mathf.Approximately(MicroBuildRules.Passage, 2f) && Mathf.Approximately(MicroBuildRules.Passage, FurnitureSpacing.Standard), $"{MicroBuildRules.Passage}");
        TestProbe.Check("era avanzada desde −3500 (Uruk sí, la aldea de −9500 no, 1964 sí)", MicroBuildRules.IsTownEra(-3500) && !MicroBuildRules.IsTownEra(-9500) && MicroBuildRules.IsTownEra(1964));

        var root = new GameObject("PruebaReglas").transform; root.position = new Vector3(0f, -500f, 9000f);
        try
        {
            // 1 · El paso.
            Transform a = Piece(root, "Mesa A", new Vector3(0f, 0f, 0f), new Vector3(1.4f, 0.75f, 0.8f), "mesa", "comer");
            Transform b = Piece(root, "Mesa B", new Vector3(2.4f, 0f, 0f), new Vector3(1.4f, 0.75f, 0.8f), "mesa", "comer");   // 1,0 m libre
            TestProbe.Check("dos muebles a 1 m: violación de paso", Has(MicroBuildRules.Validate(root, null, false, false), "paso"));
            b.localPosition = new Vector3(3.9f, 0f, 0f);   // 2,5 m libres
            TestProbe.Check("a 2,5 m: sin violación", !Has(MicroBuildRules.Validate(root, null, false, false), "paso"));
            Piece(a, "Silla", new Vector3(0f, 0f, -0.6f), new Vector3(0.42f, 0.9f, 0.42f), "silla", "sentarse");   // hija de su mesa
            Piece(root, "Tatami", new Vector3(1.9f, 0f, 1.5f), new Vector3(6f, 0.06f, 2f), "tatami", "", walkable: true);
            TestProbe.Check("la silla de su mesa y el tatami (se pisa) no cuentan para el paso", !Has(MicroBuildRules.Validate(root, null, false, false), "paso"));

            // 2 · Muros y techos.
            var site = new GameObject("Edificio").transform; site.SetParent(root, false); site.localPosition = new Vector3(20f, 0f, 0f);
            var afs = site.gameObject.AddComponent<AreaFunctionSite>(); afs.building = "Casa"; afs.title = "Casa de prueba";
            Transform wall = Cube(site, "Muro_fondo", new Vector3(0f, 1.6f, 4f), new Vector3(8f, 3.2f, 0.3f));
            TestProbe.Check("un «Muro_…» en un edificio: violación (por nombre y por forma)", Count(MicroBuildRules.Validate(site, null, false, false), "muro") >= 2);
            wall.name = "Panel";
            TestProbe.Check("un panel de 3,2 m alto y fino, aunque no se llame muro: violación", Has(MicroBuildRules.Validate(site, null, false, false), "muro"));
            Object.DestroyImmediate(wall.gameObject);
            Transform roof = Cube(site, "Losa", new Vector3(0f, 3.4f, 0f), new Vector3(8f, 0.2f, 8f));
            TestProbe.Check("una losa a 3,3 m sobre el suelo del edificio: techo", Has(MicroBuildRules.Validate(site, null, false, false), "techo"));
            Object.DestroyImmediate(roof.gameObject);
            Transform block = Cube(site, "Casa_maciza", new Vector3(0f, 2f, 0f), new Vector3(5f, 4f, 5f));
            TestProbe.Check("un bloque de 5 × 5 × 4 m: edificio macizo", Has(MicroBuildRules.Validate(site, null, false, false), "macizo"));
            Object.DestroyImmediate(block.gameObject);
            Transform border = new GameObject("Muros_del_piso").transform; border.SetParent(root, false);
            Cube(border, "Muro_N", new Vector3(0f, 40f, 60f), new Vector3(120f, 80f, 4f));
            Transform hist = new GameObject("Muro del gueto").transform; hist.SetParent(root, false);
            var hp = hist.gameObject.AddComponent<MicroPiece>(); hp.kind = MicroBuildRules.Historic; hp.walkable = true;
            Cube(hist, "Muro_del_gueto_N", new Vector3(-30f, 1.75f, 0f), new Vector3(16f, 3.5f, 0.5f));
            TestProbe.Check("los muros del borde del piso y la barrera histórica no cuentan", !Has(MicroBuildRules.Validate(root, null, false, false), "muro"));

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

    static bool Has(List<string> l, string what) => Count(l, what) > 0;
    static int Count(List<string> l, string what) { int n = 0; foreach (string s in l) if (s.Contains(what)) n++; return n; }
}
