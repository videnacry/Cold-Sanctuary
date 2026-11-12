using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EL ARNÉS DE LAS PRUEBAS RÁPIDAS (2026-11-11): la línea de un FAIL lleva grupo y unidad; la fila de resultados va y vuelve igual; la
/// lista rápida se lee; el mapa de impacto lleva de las clases cambiadas a los grupos y avisa de un centro. Todo sin escena, en milisegundos.
/// </summary>
public class TestPipelineTest : MonoBehaviour, ITestUnit
{
    public int Group => 17;
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<TestPipelineTest>() == null) tr.gameObject.AddComponent<TestPipelineTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Arnés de las pruebas: FAIL con su unidad, resultados, lista rápida y mapa de impacto");

        int g0 = TestProbe.ContextGroup; string u0 = TestProbe.ContextUnit;
        TestProbe.Context(50, "JornadaTest");
        string line = TestProbe.FailLine("algo", "detalle");
        TestProbe.Context(g0, u0);
        TestProbe.Check("la línea de un FAIL lleva grupo y unidad (y sigue empezando por «[TEST] FAIL ·»)", line == "[TEST] FAIL · g50 JornadaTest · algo — detalle", line);

        var row = new TestResults.Row { group = 50, unit = "JornadaTest", pass = 44, fail = 2, seconds = 515.25f, status = "FALLOS" };
        row.fails.Add("uno — a"); row.fails.Add("dos\tcon tabulador");
        TestResults.Row back = TestResults.Parse(row.ToTsv());
        TestProbe.Check("la fila de resultados va y vuelve igual (TSV)", back != null && back.group == 50 && back.unit == "JornadaTest" && back.pass == 44 && back.fail == 2
            && Mathf.Abs(back.seconds - 515.3f) < 0.06f && back.status == "FALLOS" && back.fails.Count == 2 && back.fails[1] == "dos con tabulador", row.ToTsv());

        HashSet<string> parsed = TestPresets.Parse("# comentario\nAereoTest\t110\t28\n\n  TraceFieldTest\t1\t0  \n");
        TestProbe.Check("una lista se lee: una clase por línea, sin comentarios", parsed.SetEquals(new[] { "AereoTest", "TraceFieldTest" }), string.Join(",", parsed));
        HashSet<string> rapida = TestPresets.Units(TestPresets.Rapida);
        TestProbe.Greater("la lista rápida existe y tiene sus unidades (TestPresetsData)", rapida.Count, 20f, $"{rapida.Count} unidades");

        var tests = new List<TestImpact.TestFile>
        {
            new TestImpact.TestFile { cls = "ATest", group = 1, text = "class ATest : ITestUnit { Foo.Do(); }" },
            new TestImpact.TestFile { cls = "BTest", group = 2, text = "class BTest { Bar x; Foo y; }" },
            new TestImpact.TestFile { cls = "CTest", group = 3, text = "class CTest { Baz z; FooBar w; }" },
        };
        TestImpact.Result rFoo = TestImpact.Groups(new[] { "Foo" }, null, tests), rBaz = TestImpact.Groups(new[] { "Baz" }, new[] { "ATest" }, tests);
        TestProbe.Check("mapa de impacto: una clase lleva a los grupos que la citan como palabra entera, y una que toca más de la mitad es un centro",
            rFoo.groups.SetEquals(new[] { 1, 2 }) && rFoo.RecommendFull && rBaz.groups.SetEquals(new[] { 1, 3 }) && !rBaz.RecommendFull,
            $"Foo → {string.Join(" ", rFoo.groups)} (centro {rFoo.RecommendFull}); Baz + ATest → {string.Join(" ", rBaz.groups)}");
        TestImpact.Result rHarness = TestImpact.Groups(new[] { "TestProbe" }, null, tests);
        TestProbe.Check("cambiar el arnés pide la batería entera", rHarness.RecommendFull, string.Join("; ", rHarness.hubs));
        TestProbe.Check("las clases que declara un archivo y el grupo de un archivo de prueba", TestImpact.ClassesIn("public class X : MonoBehaviour {} struct Y {} enum Z {}").SequenceEqual(new[] { "X", "Y", "Z" })
            && TestImpact.TestGroup("class T : MonoBehaviour, ITestUnit { public int Group => 42; }") == 42 && TestImpact.TestGroup("class N {}") == -1);

        yield return null;
        TestProbe.End();
    }
}
