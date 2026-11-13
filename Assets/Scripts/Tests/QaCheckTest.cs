using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// (2026-11-13, docs/qa-propuesta.md §6) EL COMPROBADOR DE QA, en la lista rápida (0 s, sin escena): primero sus reglas sobre textos de
/// ejemplo (<see cref="QaCheckFixtures"/>), luego los docs de verdad (docs/qa/ del repositorio que dice Temp/qa-root.txt, lo escribe
/// tools/sync-to-unity.sh). Falla si el catálogo, el registro, las peticiones y las pruebas no cuadran.
/// </summary>
public class QaCheckTest : MonoBehaviour, ITestUnit
{
    public int Group => 118;
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<QaCheckTest>() == null) tr.gameObject.AddComponent<QaCheckTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Comprobador de QA: reglas sobre textos de ejemplo y docs/qa de verdad");
        QaCheckFixtures.Run((name, ok, detail) => TestProbe.Check(name, ok, detail));

        string project = System.IO.Path.GetDirectoryName(Application.dataPath);
        string root = QaCheck.FindRoot(project);
        if (TestProbe.Check("encuentro el repositorio con docs/qa (Temp/qa-root.txt, COLD_SANCTUARY_REPO o el propio proyecto)", root != null, project))
        {
            QaCheck.Report rep = QaCheck.Run(QaCheck.Load(root));
            TestProbe.Check("docs/qa cuadra con las pruebas: 0 problemas (lo mismo que «Tools/Cold Sanctuary/QA/Comprobar»)", rep.Problems.Count == 0,
                rep.Summary + (rep.Problems.Count > 0 ? " · " + string.Join(" · ", rep.Problems.Take(12).Select(p => p.ToString())) : ""));
        }
        TestProbe.End();
        yield break;
    }
}

/// <summary>Las reglas del comprobador sobre textos pequeños. Pura (sin Unity): la corre QaCheckTest y también el qa-check.sh compilado.</summary>
public static class QaCheckFixtures
{
    // Los IDs de ejemplo van troceados («[» + «CV-01]») para que el comprobador, al leer este archivo, no los tome por checks de verdad.
    const string Lb = "[";

    public const string Casos =
        "# Casos\n\n## JOR · Jornada\n\n" +
        "| ID | Qué tiene que pasar | Prueba | Reglas | Origen |\n|---|---|---|---|---|\n" +
        "| JOR-01 | el reparto es igual | RepartoSantuariosTest (g113) | J1 | PB-152 |\n" +
        "| ~~JOR-02~~ | retirado | — | | |\n\n" +
        "## CV · Convivencia\n\n" +
        "| ID | Procesos | Qué tiene que pasar | Prueba | Reglas |\n|---|---|---|---|---|\n" +
        "| CV-01 | tiempo × puertas | nadie se salta una puerta | JornadaTest g50 | — |\n\n" +
        "## OP · Operación\n\n" +
        "| ID | Regla | Prueba |\n|---|---|---|\n" +
        "| OP-01 | un solo probador | — |\n\n" +
        "## Bugs y huecos\n\n" +
        "| ID | Qué | Casos | Dueño |\n|---|---|---|---|\n" +
        "| B-01 | fauna atascada | CV-01 | probador |\n" +
        "| HC-01 | bonusPacks \\| decide Berón | — | Berón |\n";

    public const string Registro =
        "# Registro\n\n" +
        "| ID | Estado | Fecha | Commit | Run log |\n|---|---|---|---|---|\n" +
        "| JOR-01 | Probado (suite g113) | 11-13 | 1bdfe1c | [r84](runs/2026-11-13-1200-r84-rapida.md) |\n" +
        "| JOR-02 | Retirado | | | |\n" +
        "| CV-01 | Crónico | 11-13 | 1bdfe1c | B-01 |\n" +
        "| OP-01 | No aplica | | | |\n";

    public const string Peticiones =
        "# Peticiones\n\n" +
        "| PB | Fecha | Cita | Regla | Casos | Estado |\n|---|---|---|---|---|---|\n" +
        "| PB-152 | 11-10 | todo mate | J1 | JOR-01 | Hecha |\n" +
        "| PB-140 | 11-08 | bonusPacks | — | HC-01 | Abierta |\n" +
        "| PB-020 | 11-01 | un doc | — | — | Doc |\n";

    public const string Manual =
        "# Manual\n\n| # | Regla | Por qué | Se comprueba en | Fuente |\n|---|---|---|---|---|\n" +
        "| J1 | **Reparto igual** | justo | `RepartoSantuariosTest` (grupo 113) | x |\n";

    public const string Reparto =
        "public class RepartoSantuariosTest : MonoBehaviour, ITestUnit\n{\n    public int Group => 113;\n" +
        "    void A() { TestProbe.Check(\"[PB-152] todo mate\", true); TestProbe.Check($\"[JOR-01] reparto igual {1}\", true); }\n}\n";
    public const string Jornada =
        "public class JornadaTest : MonoBehaviour, ITestUnit\n{\n    public int Group => 50;\n" +
        "    void A() { TestProbe.Check(\"[CV-01] nadie se salta una puerta\", true); }\n}\n" +
        "public class OtraTest : MonoBehaviour, ITestUnit\n{\n    public int Group => 51;\n}\n";
    public const string Presets = "    public static readonly (string unit, int group, float seconds)[] Rapida =\n    {\n        (\"JornadaTest\", 50, 0f),\n    };\n";

    /// <summary>Un caso de ejemplo que cuadra entero; cada prueba cambia una sola cosa.</summary>
    public static QaCheck.Input Good()
    {
        var i = new QaCheck.Input { Casos = Casos, Registro = Registro, Peticiones = Peticiones, Manual = Manual, Presets = Presets };
        i.Sources["Assets/Scripts/Tests/RepartoSantuariosTest.cs"] = Reparto;
        i.Sources["Assets/Scripts/Tests/JornadaTest.cs"] = Jornada;
        i.RunLogs.Add("runs/2026-11-13-1200-r84-rapida.md");
        i.Sizes.Add(new QaCheck.SizeChange("docs/qa/casos.md", 300, 280, "último commit"));
        return i;
    }

    static QaCheck.Input With(System.Func<QaCheck.Input, QaCheck.Input> change) => change(Good());
    static QaCheck.Input Swap(string where, string from, string to)
    {
        QaCheck.Input i = Good();
        switch (where)
        {
            case "casos": i.Casos = i.Casos.Replace(from, to); break;
            case "registro": i.Registro = i.Registro.Replace(from, to); break;
            case "peticiones": i.Peticiones = i.Peticiones.Replace(from, to); break;
            case "jornada": i.Sources["Assets/Scripts/Tests/JornadaTest.cs"] = Jornada.Replace(from, to); break;
            case "presets": i.Presets = i.Presets.Replace(from, to); break;
        }
        return i;
    }

    public static void Run(System.Action<string, bool, string> check)
    {
        // ── El lector de tablas ──
        List<QaCheck.Row> rows = QaCheck.Rows(Casos);
        check("[QA] el lector de tablas saca las filas de cada tabla (con su cabecera y su sección)", rows.Count == 6
            && rows[0].Cell("prueba") == "RepartoSantuariosTest (g113)" && rows[0].Section.StartsWith("JOR"), string.Join(" / ", rows.Select(r => r.Cells[0])));
        QaCheck.Row hc = rows.FirstOrDefault(r => r.Cells[0] == "HC-01");
        check("[QA] una barra escapada (\\|) no parte la celda", hc != null && hc.Cells.Length == 4 && hc.Cells[1] == "bonusPacks | decide Berón", hc == null ? "sin HC-01" : string.Join(" ¦ ", hc.Cells));
        check("[QA] el ID se lee aunque vaya tachado o en negrita, y lo tachado cuenta como retirado",
            QaCheck.IdOf("~~JOR-02~~") == "JOR-02" && QaCheck.IdOf("**CV-01**") == "CV-01" && QaCheck.Struck("~~JOR-02~~") && !QaCheck.Struck("JOR-01"), QaCheck.IdOf("~~JOR-02~~"));
        List<QaCheck.UnitRef> refs = QaCheck.UnitRefs("FaunaChecks (g1), JornadaTest g50, YogaTest (grupo 15); g40 + vivo");
        check("[QA] las unidades se citan como «Unidad (gN)», «Unidad gN», «Unidad (grupo N)» o «gN» solo",
            refs.Count == 4 && refs[0].Unit == "FaunaChecks" && refs[0].Group == 1 && refs[1].Unit == "JornadaTest" && refs[1].Group == 50
            && refs[2].Unit == "YogaTest" && refs[2].Group == 15 && refs[3].Unit == null && refs[3].Group == 40, string.Join(", ", refs.Select(r => r.ToString())));
        List<QaCheck.Row> split = QaCheck.Rows("| ID | Área | Unidad | Grupo | Reglas |\n|---|---|---|---|---|\n| FAU-01 | FAU | TerritorioTest | 99 | — |\n| JOR-03 | JOR | JornadaTest, HorarioLibreTest | 50, 65 | J1 |\n| FAU-02 | FAU | — | — | — |\n");
        List<QaCheck.UnitRef> s0 = QaCheck.CaseUnits(split[0]), s1 = QaCheck.CaseUnits(split[1]), s2 = QaCheck.CaseUnits(split[2]);
        check("[QA] con columnas «Unidad» y «Grupo» separadas, cada unidad va con su grupo (en orden)",
            s0.Count == 1 && s0[0].Unit == "TerritorioTest" && s0[0].Group == 99 && s1.Count == 2 && s1[1].Unit == "HorarioLibreTest" && s1[1].Group == 65 && s2.Count == 0,
            string.Join(", ", s0.Concat(s1).Concat(s2).Select(u => u.ToString())));
        Dictionary<string, int> units = QaCheck.TestUnits(new Dictionary<string, string> { { "a.cs", Jornada } });
        check("[QA] las unidades de prueba salen del código con su grupo (varias por archivo)", units.Count == 2 && units["JornadaTest"] == 50 && units["OtraTest"] == 51,
            string.Join(", ", units.Select(k => k.Key + "=" + k.Value)));
        check("[QA] el ID de un check sale del principio de su texto («[CV-03] …», también en $\"…\")",
            QaCheck.CaseIdOf(Lb + "CV-03] la fauna con viaje") == "CV-03" && QaCheck.CaseIdOf(Lb + "PB-152] mate") == "PB-152" && QaCheck.CaseIdOf("sin ID [CV-03]") == null
            && QaCheck.CheckIds(Reparto).SequenceEqual(new[] { "PB-152", "JOR-01" }), string.Join(",", QaCheck.CheckIds(Reparto)));
        check("[QA] la columna de casos de test-results.tsv: un ID por caso, FAIL si alguno de sus checks falló",
            QaCheck.CaseSummary(new[] { ("CV-03", true), ("CV-07", false), ("CV-03", false), ("CV-07", false), ("JOR-01", true) }) == "CV-03 FAIL, CV-07 FAIL, JOR-01 OK",
            QaCheck.CaseSummary(new[] { ("CV-03", true), ("CV-07", false), ("CV-03", false), ("CV-07", false), ("JOR-01", true) }));
        check("[QA] los estados del registro se reconocen (con o sin tilde, con su paréntesis)",
            QaCheck.StateOf("Probado (suite g113)") == QaCheck.State.Probado && QaCheck.StateOf("Crónico") == QaCheck.State.Cronico && QaCheck.StateOf("cronico (B-01)") == QaCheck.State.Cronico
            && QaCheck.StateOf("Solo código") == QaCheck.State.SoloCodigo && QaCheck.StateOf("Sin probar") == QaCheck.State.SinProbar && QaCheck.StateOf("Probadísimo") == QaCheck.State.Desconocido, "");

        // ── El ejemplo bueno cuadra ──
        QaCheck.Report good = QaCheck.Run(Good());
        check("[QA] el ejemplo que cuadra no tiene problemas", good.Problems.Count == 0, good.Summary + " · " + string.Join(" · ", good.Problems.Select(p => p.ToString())));

        // ── Cada regla, rompiendo una sola cosa ──
        Expect(check, "§6.1 una unidad citada que no existe", Swap("casos", "JornadaTest g50", "JornadaTestX g50"), "unidad");
        Expect(check, "§6.1 una unidad citada con otro grupo", Swap("casos", "RepartoSantuariosTest (g113)", "RepartoSantuariosTest (g112)"), "grupo");
        Expect(check, "§6.1 un grupo citado solo que no existe", Swap("casos", "JornadaTest g50", "g77"), "grupo");
        Expect(check, "§6.1 un caso sin prueba ni grupo (salvo los OP)", Swap("casos", "RepartoSantuariosTest (g113)", "—"), "caso-sin-prueba");
        Expect(check, "§6.1 «Probado (suite gN)» en un caso que no cita ese grupo", Swap("registro", "Probado (suite g113)", "Probado (suite g50)"), "grupo");
        QaCheck.Report noTestYet = QaCheck.Run(With(i => { i.Casos = i.Casos.Replace("RepartoSantuariosTest (g113)", "—"); i.Registro = i.Registro.Replace("| Probado (suite g113) |", "| Probado (retro) |"); return i; }));
        check("[QA] un caso visto en vivo o en retro, o aún sin probar, puede no tener prueba todavía (sale como aviso)",
            !noTestYet.Has("caso-sin-prueba") && noTestYet.Warnings.Any(w => w.Contains("sin unidad ni grupo")), string.Join(" · ", noTestYet.Problems.Select(p => p.ToString())));
        QaCheck.Report prose = QaCheck.Run(With(i => { i.Peticiones = "Las nuevas siguen desde PB-156.\n\n" + i.Peticiones; return i; }));
        check("[QA] un ID futuro en la prosa («desde PB-156») no es un problema", prose.Problems.Count == 0, string.Join(" · ", prose.Problems.Select(p => p.ToString())));
        Expect(check, "IDs: un ID desconocido en una tabla", With(i => { i.Peticiones = "Las nuevas siguen desde PB-156.\n\n" + i.Peticiones.Replace("| JOR-01 | Hecha |", "| JOR-01 | Hecha | PB-157 |"); return i; }), "id-desconocido");
        Expect(check, "§6.2 un caso sin fila en el registro", Swap("registro", "| CV-01 | Crónico | 11-13 | 1bdfe1c | B-01 |\n", ""), "sin-registro");
        Expect(check, "§6.2 una fila del registro con un ID que no está en el catálogo", Swap("registro", "| OP-01 |", "| OP-09 |"), "registro-desconocido");
        Expect(check, "§6.2 dos filas del registro para un caso", Swap("registro", "| OP-01 | No aplica | | | |\n", "| OP-01 | No aplica | | | |\n| OP-01 | Sin probar | | | |\n"), "duplicado");
        Expect(check, "§6.2 un estado que no existe", Swap("registro", "Probado (suite g113)", "Probadísimo"), "estado-desconocido");
        Expect(check, "§6.3 «Probado» sin run log", Swap("registro", "[r84](runs/2026-11-13-1200-r84-rapida.md)", "—"), "probado-sin-run");
        Expect(check, "§6.3 «Probado» con un run log que no existe", Swap("registro", "r84-rapida.md", "r85-rapida.md"), "probado-sin-run");
        Expect(check, "§6.3 «Probado» sin commit", Swap("registro", "| 11-13 | 1bdfe1c | [r84]", "| 11-13 | | [r84]"), "probado-sin-commit");
        Expect(check, "§6.4 un caso CV sin ningún check con su [CV-nn]", Swap("jornada", Lb + "CV-01] ", ""), "cv-sin-check");
        Expect(check, "§6.4 un check que cita un ID que no está en el catálogo", Swap("jornada", Lb + "CV-01] ", Lb + "CV-07] "), "check-desconocido");
        Expect(check, "§6.5 un B-nn citado que no está en la tabla de bugs", Swap("registro", "| B-01 |", "| B-02 |"), "bug-desconocido");
        Expect(check, "§6.5 un caso en «Crónico» sin su B-nn", With(i => { i.Registro = i.Registro.Replace("| B-01 |", "| |"); i.Casos = i.Casos.Replace("| fauna atascada | CV-01 |", "| fauna atascada | — |"); return i; }), "cronico-sin-bug");
        Expect(check, "§6.5 un check en «Crónico» (tabla «Inestables y crónicos») sin su B-nn",
            With(i => { i.Registro += "\n## Inestables y crónicos\n\n| Check | Grupo | Estado | B/HC |\n|---|---|---|---|\n| fauna: ningún animal atascado | 50 | Crónico | — |\n"; return i; }), "cronico-sin-bug");
        Expect(check, "§6.6 una PB sin casos ni «Doc»", Swap("peticiones", "| — | Doc |", "| — | Hecha |"), "pb-sin-casos");
        Expect(check, "§6.6 una PB «Hecha» con un caso en «Solo código»", Swap("registro", "| Probado (suite g113) |", "| Solo código |"), "pb-estado");
        Expect(check, "§6.6 una PB «Hecha» con un HC abierto", Swap("peticiones", "| HC-01 | Abierta |", "| HC-01 | Hecha |"), "pb-estado");
        Expect(check, "§6.6 una PB con un estado que no existe", Swap("peticiones", "| JOR-01 | Hecha |", "| JOR-01 | Lista |"), "pb-estado");
        Expect(check, "§6.7 una regla del manual que no existe (en un caso)", Swap("casos", "| J1 |", "| J99 |"), "regla-desconocida");
        Expect(check, "§6.7 una regla del manual que no existe (en una petición)", Swap("peticiones", "| J1 |", "| W99 |"), "regla-desconocida");
        Expect(check, "IDs: una petición que cita un caso que no existe", Swap("peticiones", "| JOR-01 | Hecha |", "| JOR-01, JOR-07 | Hecha |"), "id-desconocido");
        Expect(check, "§6.8 una unidad de la lista rápida que ya no existe", Swap("presets", "(\"JornadaTest\", 50, 0f)", "(\"NadaTest\", 3, 0f)"), "preset");
        Expect(check, "§6.9 un doc de QA que encoge más de la mitad (82878fc)", With(i => { i.Sizes.Add(new QaCheck.SizeChange("docs/checklist.md", 0, 231000, "último commit")); return i; }), "encoge");
        Expect(check, "§6.9 un doc de QA borrado", With(i => { i.Sizes.Add(new QaCheck.SizeChange("docs/qa/registro.md", -1, 5000, "sin commitear")); return i; }), "encoge");
        Expect(check, "§6.10 un archivo de TmpCheck en git", With(i => { i.Tracked.Add("Assets/Editor/TmpCheck/FotoProbe.cs"); return i; }), "tmpcheck");
        QaCheck.Report half = QaCheck.Run(With(i => { i.Sizes.Add(new QaCheck.SizeChange("docs/qa/casos.md", 160, 300, "último commit")); i.Sizes.Add(new QaCheck.SizeChange("docs/auditoria-peticiones.md", -1, 66000, "último commit")); return i; }));
        check("[QA] encoger menos de la mitad no es problema, y la auditoría vieja se puede retirar (la sustituye peticiones.md)", half.Problems.Count == 0, string.Join(" · ", half.Problems.Select(p => p.ToString())));
    }

    static void Expect(System.Action<string, bool, string> check, string what, QaCheck.Input input, string code)
    {
        QaCheck.Report r = QaCheck.Run(input);
        bool ok = r.Problems.Any(p => p.Code == code);
        check($"[QA] lo para: {what} («{code}»)", ok, string.Join(" · ", r.Problems.Select(p => p.ToString())));
    }
}
