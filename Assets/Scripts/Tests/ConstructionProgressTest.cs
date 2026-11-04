using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA OBRA VISIBLE (Berón 2026-11-04: «el progreso de las obras podría ser visual?»; docs/construction-simulation.md):
/// simula N días de obra en una obra de prueba con su «Acabado» (suelo, jaula, lámpara, banco, saco, espejo, maceta):
/// 1. PLAN: las piezas se reparten por días en orden de etapas (cimientos y suelo → estructura → instalaciones → muebles y
///    máquinas → acabados y decoración) y cada día tiene alguna.
/// 2. TURNO: durante «Trabajar la obra» salen las piezas del día (una parte a media jornada), el martillo está en la mano,
///    el material de los palés mengua y el cartel dice la etapa y el porcentaje; el andamio se va junto a lo que se construye.
/// 3. DÍAS: cada día se ven MÁS piezas (nunca menos), las de días futuros siguen sin salir y el último día = el acabado entero.
/// 4. NADA USABLE antes de terminar: la maqueta no tiene colisiones ni conductas y el «Acabado» de verdad sigue apagado.
/// 5. GUARDAR: otra obra igual restaurada en el día k enseña exactamente las mismas piezas que la simulada en el día k.
/// 6. ESCENA: las obras sin terminar del santuario tienen su plan (piezas para todos los días) y nada usable a la vista.
/// </summary>
public class ConstructionProgressTest : MonoBehaviour, ITestUnit
{
    public int Group => 98;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<ConstructionProgressTest>() == null) tr.gameObject.AddComponent<ConstructionProgressTest>();
    }

    const int Days = 5;

    // Una obra de prueba con su acabado ya diseñado, lejos de todo.
    static ConstructionProject MakeSite(string name, Vector3 at)
    {
        GameObject area = new GameObject(name);
        area.SetActive(false);
        area.transform.position = at;
        Transform done = new GameObject(FloorForms.FinishedName).transform; done.SetParent(area.transform, false); done.localPosition = Vector3.up * 0.1f;
        Color c = new Color(0.7f, 0.6f, 0.45f);
        KitchenProps.C(done, new Vector3(0f, 0.025f, 0f), new Vector3(8f, 0.05f, 8f), c, 0f, "Suelo_tatami");
        Transform cage = KitchenProps.Root(done, "Jaula", new Vector3(2f, 0.05f, 2f));
        KitchenProps.C(cage, new Vector3(0f, 0.04f, 0f), new Vector3(3f, 0.08f, 3f), c, 0f, "Lona");
        for (int i = 0; i < 4; i++) KitchenProps.C(cage, new Vector3(i % 2 == 0 ? -1.5f : 1.5f, 0.9f, i < 2 ? -1.5f : 1.5f), new Vector3(0.12f, 1.8f, 0.12f), c, 0f, "Poste_guadua");
        for (int i = 0; i < 2; i++) KitchenProps.C(cage, new Vector3(0f, 0.9f, i == 0 ? -1.5f : 1.5f), new Vector3(3f, 1.6f, 0.04f), c, 0f, "Red_canamo");
        KitchenProps.C(done, new Vector3(0f, 2.8f, 0f), new Vector3(0.4f, 0.1f, 0.4f), Color.white, 0f, "Lampara_techo");
        Transform bench = KitchenProps.C(done, new Vector3(-2.5f, 0.25f, -2f), new Vector3(1.6f, 0.5f, 0.5f), c, 0f, "Banco");
        bench.gameObject.AddComponent<BoxCollider>();
        bench.gameObject.AddComponent<WorldLabel>().text = "Banco (prueba)";
        KitchenProps.C(done, new Vector3(-2.5f, 1.2f, 2f), new Vector3(0.4f, 1.2f, 0.4f), Color.red, 0f, "Saco_boxeo");
        KitchenProps.C(done, new Vector3(-3.8f, 1.2f, 0f), new Vector3(0.05f, 1.6f, 2f), Color.cyan, 0f, "Espejo_pared");
        KitchenProps.C(done, new Vector3(3.5f, 0.3f, -3.5f), new Vector3(0.4f, 0.6f, 0.4f), Color.green, 0f, "Maceta_planta");
        done.gameObject.SetActive(false);
        Transform mat = KitchenProps.Root(area.transform, "Material_del_dia", new Vector3(3f, 0.1f, -3f));
        KitchenProps.C(mat, new Vector3(0f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), c, 0f, "Pale");
        for (int i = 0; i < 8; i++) KitchenProps.C(mat, new Vector3(-0.4f + (i % 4) * 0.25f, 0.18f + (i / 4) * 0.1f, 0f), new Vector3(0.22f, 0.09f, 1f), c, 0f, "Barro_con_paja");
        Transform scaf = KitchenProps.Root(area.transform, "Andamio_obra", new Vector3(-3.5f, 0.1f, -3.5f));
        KitchenProps.C(scaf, new Vector3(0f, 1.5f, 0f), new Vector3(0.05f, 3f, 0.05f), Color.gray, 0f, "Tubo");
        ConstructionProject cp = area.AddComponent<ConstructionProject>();
        cp.title = name; cp.daysRequired = Days; cp.crewNeeded = 2; cp.persist = false;
        area.SetActive(true);
        return cp;
    }

    // ¿Hay algo USABLE en la maqueta (colisiones o conductas)?
    static string Usable(ConstructionProject cp)
    {
        if (cp.Preview == null) return "";
        var l = new List<string>();
        foreach (Collider c in cp.Preview.GetComponentsInChildren<Collider>(true)) l.Add("colisión en " + c.name);
        foreach (MonoBehaviour m in cp.Preview.GetComponentsInChildren<MonoBehaviour>(true)) l.Add(m.GetType().Name + " en " + m.name);
        return string.Join(", ", l.Take(6));
    }

    static List<string> Shown(ConstructionProject cp) => cp.Pieces.Where(p => p.shown > 0f && p.group != null && p.group.gameObject.activeInHierarchy).Select(p => p.name).ToList();

    public IEnumerator Run()
    {
        TestProbe.Begin("Obra visible: etapas por días, turno animado, material que mengua, andamio, nada usable y guardar");
        ConstructionProject cp = MakeSite("Obra_visible_TEST", new Vector3(4000f, 0f, 4000f));
        GameObject worker = GameObject.CreatePrimitive(PrimitiveType.Capsule); worker.name = "Obrera_TEST";
        Destroy(worker.GetComponent<Collider>());
        worker.transform.position = cp.transform.position + new Vector3(0f, 1f, -2f);
        yield return null;   // Start: plan de obra
        Transform realDone = cp.transform.Find(FloorForms.FinishedName);

        // ── 1. PLAN ──
        var pieces = cp.Pieces.ToList();
        TestProbe.Check("el plan tiene piezas para todos los días de obra", pieces.Count >= Days && Enumerable.Range(1, Days).All(d => pieces.Any(p => p.day == d)),
            string.Join(" · ", Enumerable.Range(1, Days).Select(d => $"día {d}: {string.Join("+", pieces.Where(p => p.day == d).Select(p => p.name))}")));
        bool ordered = true; for (int i = 1; i < pieces.Count; i++) if (pieces[i].day < pieces[i - 1].day || pieces[i].stage < pieces[i - 1].stage) ordered = false;
        TestProbe.Check("orden de obra: cimientos y suelo → estructura → instalaciones → muebles y máquinas → acabados y decoración", ordered,
            string.Join(", ", pieces.Select(p => $"{p.name}={ConstructionProject.StageNames[p.stage]}")));
        TestProbe.Check("el suelo va el primer día y la decoración el último", pieces.First(p => p.name == "Suelo_tatami").day == 1 && pieces.First(p => p.name == "Maceta_planta").day == Days);
        TestProbe.Check("día 0: no se ve ninguna pieza y el acabado de verdad está apagado", Shown(cp).Count == 0 && realDone != null && !realDone.gameObject.activeSelf);

        // ── 2–4. N DÍAS DE OBRA ──
        int prev = 0; bool mono = true, noFuture = true, noUse = true, partial = true, shrink = true, label = true, scaffold = true, hammer = true;
        string why = "";
        List<string> atDay3 = null;
        for (int d = 1; d <= Days; d++)
        {
            if (!cp.materialOnSite) { cp.DeliverMaterial("Repartidor_TEST"); if (!cp.materialOnSite) cp.Restore(cp.daysDone, true); }   // (sin material en el almacén de la escena: llega igual)
            float h = TimeController.timeController != null ? TimeController.timeController.Hour : 9f;
            cp.BeginShift("Obrera_TEST", worker, h, 2f);
            cp.BeginShift("Obrero2_TEST", null, h, 2f);
            yield return null;
            if (GameObject.Find("Martillo_obra") == null) { hammer = false; why += $"día {d}: sin martillo; "; }
            // A media jornada: algo del día ya salió (pero no todo), y el material mengua.
            cp.AdvanceDay(0.5f);
            int mid = Shown(cp).Count;
            if (mid <= prev && pieces.Count(p => p.day == d) > 0) { partial = false; why += $"día {d}: a media jornada {mid} ≤ {prev}; "; }
            if (cp.MaterialLeft >= cp.MaterialFull) { shrink = false; why += $"día {d}: material {cp.MaterialLeft}/{cp.MaterialFull}; "; }
            string lbl = cp.GetComponent<WorldLabel>().text;
            if (!lbl.Contains(cp.CurrentStageName) || !lbl.Contains("%")) { label = false; why += $"día {d}: cartel «{lbl}»; "; }
            cp.SnapScaffold();
            Vector3 wp = cp.WorkPoint, sp = cp.ScaffoldPosition;
            float sd = Vector2.Distance(new Vector2(wp.x, wp.z), new Vector2(sp.x, sp.z));
            if (sd > 2.5f) { scaffold = false; why += $"día {d}: andamio a {sd:0.0} m de lo que se construye; "; }
            if (Usable(cp).Length > 0 || realDone.gameObject.activeSelf) { noUse = false; why += $"día {d}: usable {Usable(cp)}; "; }
            cp.AdvanceDay(1f);
            cp.WorkDay("Obrera_TEST");
            bool lastBefore = cp.daysDone == d - 1;   // falta el segundo del equipo
            cp.WorkDay("Obrero2_TEST");
            if (!lastBefore || cp.daysDone != d) { mono = false; why += $"día {d}: días hechos {cp.daysDone} (equipo de 2); "; }
            yield return null;
            if (d < Days)
            {
                var shown = Shown(cp);
                if (shown.Count < prev) { mono = false; why += $"día {d}: {shown.Count} < {prev}; "; }
                int expect = pieces.Count(p => p.day <= d);
                if (shown.Count != expect || pieces.Where(p => p.day > d).Any(p => shown.Contains(p.name))) { noFuture = false; why += $"día {d}: se ven {shown.Count}, tocan {expect}; "; }
                if (Usable(cp).Length > 0 || realDone.gameObject.activeSelf) { noUse = false; why += $"día {d} (fin): usable {Usable(cp)}; "; }
                prev = shown.Count;
                if (d == 3) atDay3 = shown;
            }
            if (GameObject.Find("Martillo_obra") != null) { hammer = false; why += $"día {d}: el martillo sigue tras el turno; "; }
        }
        TestProbe.Check("en el turno salen piezas del día (a media jornada ya hay más que el día anterior)", partial, why);
        TestProbe.Check("el martillo aparece en la mano durante el turno y se guarda al acabarlo", hammer, why);
        TestProbe.Check("los palés del material del día menguan con el trabajo", shrink, why);
        TestProbe.Check("el cartel dice la etapa y el porcentaje", label, why);
        TestProbe.Check("el andamio se va junto a la parte que se construye", scaffold, why);
        TestProbe.Check("cada día se ven más piezas (nunca menos) y el día cuenta con el equipo entero", mono, why);
        TestProbe.Check("las piezas de días futuros no salen antes de tiempo", noFuture, why);
        TestProbe.Check("NADA USABLE antes de terminar: la maqueta sin colisiones ni conductas, el acabado de verdad apagado", noUse, why);
        bool fin = cp.Finished && realDone.gameObject.activeSelf && (cp.Preview == null || !cp.Preview.gameObject.activeSelf);
        Collider bench = realDone.Find("Banco") != null ? realDone.Find("Banco").GetComponent<BoxCollider>() : null;
        TestProbe.Check("último día = el acabado entero, de verdad (con sus colisiones), sin maqueta ni andamio", fin && bench != null && bench.enabled && !cp.transform.Find("Andamio_obra").gameObject.activeSelf,
            $"terminada {cp.Finished}, acabado {realDone.gameObject.activeSelf}, maqueta {(cp.Preview != null && cp.Preview.gameObject.activeSelf)}");
        TestProbe.Check("todas las piezas del plan cuentan como colocadas al terminar", cp.Pieces.All(p => p.Placed));

        // ── 5. GUARDAR / CARGAR ──
        ConstructionProject again = MakeSite("Obra_visible_TEST_2", new Vector3(4100f, 0f, 4000f));
        yield return null;
        again.Restore(3, false);
        var re = Shown(again);
        TestProbe.Check("cargar la obra en el día 3 enseña exactamente las piezas de esos días", atDay3 != null && re.Count == atDay3.Count && re.All(atDay3.Contains),
            $"cargada: {string.Join(", ", re)} · simulada: {(atDay3 != null ? string.Join(", ", atDay3) : "—")}");
        TestProbe.Check("cargada a medias: el material del día no está y el acabado sigue apagado", !again.materialOnSite && !again.transform.Find(FloorForms.FinishedName).gameObject.activeSelf && again.MaterialLeft == again.MaterialFull);

        // ── 6. LAS OBRAS DE LA ESCENA ──
        foreach (ConstructionProject p in ConstructionProject.All.Where(p => p != null && p != cp && p != again && !p.Finished).ToList())
        {
            var ps = p.Pieces;
            Transform ad = p.transform.Find(FloorForms.FinishedName);
            bool plan = ad == null || ps.Count == 0 || ps.Count < p.daysRequired || Enumerable.Range(1, p.daysRequired).All(d => ps.Any(x => x.day == d));
            TestProbe.Check($"«{p.title}»: plan de obra por días ({ps.Count} piezas, {p.daysRequired} días) y nada usable a la vista", plan && Usable(p).Length == 0 && (ad == null || !ad.gameObject.activeSelf),
                $"etapa {p.CurrentStageName}, {p.Percent:0} %; {Usable(p)}");
        }

        Destroy(cp.gameObject); Destroy(again.gameObject); Destroy(worker);
        yield return null;
        TestProbe.End();
    }
}
