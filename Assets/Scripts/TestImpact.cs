using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// MAPA DE IMPACTO DE LAS PRUEBAS (2026-11-11, scratchpad analisis-pruebas.md §3a): de los archivos cambiados (git diff) a los grupos de
/// prueba que los tocan, por los NOMBRES DE CLASE que cita cada archivo de prueba. Lógica pura (la usa la herramienta del editor
/// TestImpactTool y la prueba TestPipelineTest). Un archivo de prueba cambiado corre su propio grupo. Una clase que tocan más de la mitad
/// de los grupos, o un archivo del propio arnés (TestRunner, TestProbe, ITestUnit, TestResults, TestImpact), es un CENTRO: mejor la
/// batería entera.
/// </summary>
public static class TestImpact
{
    public sealed class TestFile { public string cls = ""; public int group; public string text = ""; }

    public sealed class Result
    {
        public readonly SortedSet<int> groups = new SortedSet<int>();
        public readonly List<string> hubs = new List<string>();        // clases o archivos que tocan > 50 % de los grupos (o el arnés)
        public readonly List<string> unmapped = new List<string>();    // clases cambiadas que no cita ninguna prueba
        public int totalGroups;
        public bool RecommendFull => hubs.Count > 0;
    }

    public static readonly string[] HarnessClasses = { "TestRunner", "TestProbe", "ITestUnit", "TestResults", "TestPresets", "TestImpact", "IFastGameTime" };

    static readonly Regex Declares = new Regex(@"\b(?:class|struct|interface|enum)\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);
    static readonly Regex GroupOf = new Regex(@"int\s+Group\s*=>\s*(\d+)", RegexOptions.Compiled);

    /// <summary>Las clases (y structs, interfaces, enums) que declara un archivo.</summary>
    public static List<string> ClassesIn(string source) => Declares.Matches(source ?? "").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();

    /// <summary>¿Es un archivo de prueba? Devuelve su grupo (o -1).</summary>
    public static int TestGroup(string source)
    {
        if (string.IsNullOrEmpty(source) || !source.Contains("ITestUnit")) return -1;
        Match m = GroupOf.Match(source);
        return m.Success && int.TryParse(m.Groups[1].Value, out int g) ? g : -1;
    }

    /// <summary>Los grupos que tocan las clases cambiadas. <paramref name="changedTests"/>: clases de prueba cambiadas (corren su grupo).</summary>
    public static Result Groups(IEnumerable<string> changedClasses, IEnumerable<string> changedTests, IList<TestFile> tests)
    {
        var r = new Result { totalGroups = tests.Select(t => t.group).Distinct().Count() };
        foreach (string tc in changedTests ?? Enumerable.Empty<string>())
            foreach (TestFile t in tests) if (t.cls == tc) r.groups.Add(t.group);
        foreach (string c in (changedClasses ?? Enumerable.Empty<string>()).Distinct())
        {
            if (HarnessClasses.Contains(c)) { r.hubs.Add($"{c} (el arnés de las pruebas)"); continue; }
            var word = new Regex($@"\b{Regex.Escape(c)}\b");
            var hit = new SortedSet<int>(tests.Where(t => t.cls != c && word.IsMatch(t.text)).Select(t => t.group));
            if (hit.Count == 0) { r.unmapped.Add(c); continue; }
            if (r.totalGroups > 0 && hit.Count * 2 > r.totalGroups) r.hubs.Add($"{c} ({hit.Count} de {r.totalGroups} grupos)");
            r.groups.UnionWith(hit);
        }
        return r;
    }
}
