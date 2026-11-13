using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// EL COMPROBADOR DE QA (2026-11-13, docs/qa-propuesta.md §6; equivale a «qa:check» de CodeShip). Lógica pura sobre texto, sin Unity (como
/// <see cref="TestImpact"/>): lee docs/qa/casos.md, registro.md y peticiones.md, el manual de diseño y el código de las pruebas, y dice qué no
/// cuadra. Lo usan tres sitios: QaCheckTest (lista rápida), el menú «Tools/Cold Sanctuary/QA/Comprobar» (QaCheckTool) y el qa-check.sh del
/// probador (<see cref="Cli"/>, compilado aparte con el dotnet del Editor, sin abrir Unity).
///
/// Cómo se escriben los docs para que los lea:
///  · Tablas markdown con cabecera. Una fila es de un caso si su primera celda es el ID (FAU-01, CV-03…; tachado ~~X~~ = retirado).
///  · casos.md: la prueba va en una columna cuya cabecera dice «Prueba», «Unidad», «Test» o «Grupo», como «JornadaTest (g50)»,
///    «JornadaTest g50», «YogaTest (grupo 15)» o «g40» solo; las reglas del manual, en una columna «Regla(s)». La tabla de bugs: filas
///    que empiezan por B-nn o HC-nn.
///  · registro.md: columnas «Estado», «Commit» y el run log como enlace a runs/….md. Los estados, los de la propuesta §3.
///  · peticiones.md: filas que empiezan por PB-nnn, con columnas «Casos», «Regla» y «Estado» (Hecha, Doc, Sin verificar, Abierta).
/// </summary>
public static class QaCheck
{
    // ── Modelo ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    public sealed class Row
    {
        public int Line; public string Section = ""; public string[] Header = new string[0]; public string[] Cells = new string[0];
        public string Text => string.Join(" | ", Cells);
        /// <summary>La celda de la primera columna cuya cabecera contiene alguna de las claves (sin tildes ni mayúsculas); null si no hay.</summary>
        public string Cell(params string[] keys)
        {
            for (int i = 0; i < Header.Length && i < Cells.Length; i++)
            {
                string h = Norm(Header[i]);
                foreach (string k in keys) if (h.Contains(Norm(k))) return Cells[i];
            }
            return null;
        }
        public bool HasColumn(params string[] keys) => Header.Any(h => keys.Any(k => Norm(h).Contains(Norm(k))));
    }

    public struct UnitRef
    {
        public string Unit; public int Group;
        public override string ToString() => (Unit ?? "") + (Group >= 0 ? " g" + Group : "");
    }

    public enum State { Probado, Parcial, Fail, Inestable, Cronico, SoloTest, SoloCodigo, SinProbar, NoAplica, Retirado, Desconocido }

    public sealed class SizeChange
    {
        public string Path; public long Now, Before; public string Where;
        /// <param name="now">Bytes ahora (−1 si ya no está).</param><param name="before">Bytes antes (−1 si no estaba).</param>
        public SizeChange(string path, long now, long before, string where) { Path = path; Now = now; Before = before; Where = where; }
    }

    public sealed class Input
    {
        public string Casos = "", Registro = "", Peticiones = "", Manual = "", Presets = "";
        public readonly Dictionary<string, string> OtherDocs = new Dictionary<string, string>();   // entrelazamiento.md, hallazgos.md…: solo IDs
        public readonly Dictionary<string, string> Sources = new Dictionary<string, string>();     // ruta → código de las pruebas
        public readonly HashSet<string> RunLogs = new HashSet<string>();                           // «runs/AAAA-…md», relativos a docs/qa
        public readonly List<SizeChange> Sizes = new List<SizeChange>();
        public readonly List<string> Tracked = new List<string>();                                 // archivos en git (para TmpCheck)
        public readonly List<string> Missing = new List<string>();                                 // docs que no se pudieron leer
        public readonly List<string> Notes = new List<string>();
    }

    public sealed class Problem
    {
        public string Code, Where, Message;
        public override string ToString() => $"[{Code}] {Where}: {Message}";
    }

    public sealed class Report
    {
        public readonly List<Problem> Problems = new List<Problem>();
        public readonly List<string> Warnings = new List<string>();
        public int Cases, RegistroRows, Requests, Bugs, CheckIds, Units;
        public string Summary => $"{Cases} casos, {RegistroRows} filas de registro, {Requests} peticiones, {Bugs} bugs y huecos, {Units} unidades, {CheckIds} checks con ID → {Problems.Count} problemas, {Warnings.Count} avisos";
        public void Add(string code, string where, string msg) => Problems.Add(new Problem { Code = code, Where = where, Message = msg });
        public bool Has(string code) => Problems.Any(p => p.Code == code);
        public string Text()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Problems.Count == 0 ? "[QA] OK · " + Summary : "[QA] PROBLEMAS · " + Summary);
            foreach (Problem p in Problems) sb.AppendLine("  ✗ " + p);
            foreach (string w in Warnings.Take(40)) sb.AppendLine("  · aviso: " + w);
            if (Warnings.Count > 40) sb.AppendLine($"  · … y {Warnings.Count - 40} avisos más");
            return sb.ToString();
        }
    }

    // ── IDs ────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    public static readonly string[] Areas = { "FAU", "JOR", "YOG", "GYM", "ARB", "MIC", "HOS", "NUC", "ENE", "PER", "UI", "REN", "CV", "PX", "OP" };
    static readonly Regex CaseIdRx = new Regex(@"^(?:" + string.Join("|", Areas) + @")-\d{2,3}$", RegexOptions.Compiled);
    static readonly Regex PbRx = new Regex(@"^PB-\d{3}$", RegexOptions.Compiled);
    static readonly Regex BugRx = new Regex(@"^(?:B|HC)-\d{2,3}$", RegexOptions.Compiled);
    static readonly Regex AnyIdRx = new Regex(@"\b(?:[A-Z]{2,3}-\d{2,3}|B-\d{2,3})\b", RegexOptions.Compiled);
    static readonly Regex BugTokRx = new Regex(@"\b(?:B|HC)-\d{2,3}\b", RegexOptions.Compiled);
    static readonly Regex RuleRx = new Regex(@"\b[A-Z]{1,2}\d{1,2}[a-z]?\b", RegexOptions.Compiled);
    static readonly Regex ManualRuleRx = new Regex(@"^[A-Z]{1,2}\d{1,2}[a-z]?$", RegexOptions.Compiled);
    static readonly Regex CommitRx = new Regex(@"\b(?=[0-9a-f]*\d)[0-9a-f]{7,40}\b", RegexOptions.Compiled);
    static readonly Regex RunLogRx = new Regex(@"runs/[^\s|)\]`>""]+\.md", RegexOptions.Compiled);
    static readonly Regex CheckPrefixRx = new Regex(@"^\s*\[([A-Z]{1,3}-\d{2,3})\]", RegexOptions.Compiled);
    // Un literal de texto que empieza por [ID] (no escapado: los textos de ejemplo de QaCheckFixtures llevan \" y no cuentan).
    static readonly Regex SourceCheckRx = new Regex(@"(?<!\\)""\s*\[([A-Z]{1,3}-\d{2,3})\]", RegexOptions.Compiled);
    static readonly Regex UnitRx = new Regex(@"(?:\b([A-Z][A-Za-z0-9_]*[a-z][A-Za-z0-9_]*)\s*\(?\s*)?\b(?:g|grupo\s*)(\d+)\b|\b([A-Z][A-Za-z0-9_]*(?:Test|Tests|Auto|Checks))\b", RegexOptions.Compiled);
    static readonly Regex ClassRx = new Regex(@"\bclass\s+([A-Za-z_]\w*)\s*:[^{]*\bITestUnit\b", RegexOptions.Compiled);
    static readonly Regex GroupRx = new Regex(@"\bint\s+Group\s*=>\s*(\d+)", RegexOptions.Compiled);
    static readonly Regex PresetRx = new Regex(@"\(\s*""([A-Za-z_]\w*)""\s*,\s*(\d+)\s*,", RegexOptions.Compiled);

    public static string Norm(string s)
    {
        string d = (s ?? "").Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(d.Length);
        foreach (char c in d) if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(char.ToLowerInvariant(c));
        return sb.ToString().Trim();
    }

    static string Short(string s) => s == null ? "" : s.Length <= 60 ? s : s.Substring(0, 57) + "…";
    static string StripMd(string s) => Regex.Replace(s ?? "", @"[*~`]|\[|\]\([^)]*\)|\]", "").Trim();
    /// <summary>El ID de una celda (sin negritas, tachados ni enlaces).</summary>
    public static string IdOf(string cell) => StripMd(cell);
    public static bool Struck(string cell) => (cell ?? "").Trim().StartsWith("~~");
    public static bool IsCaseId(string id) => id != null && CaseIdRx.IsMatch(id);

    /// <summary>El ID al principio del texto de un check («[CV-03] …»), o null.</summary>
    public static string CaseIdOf(string checkText)
    {
        Match m = CheckPrefixRx.Match(checkText ?? "");
        return m.Success ? m.Groups[1].Value : null;
    }

    /// <summary>La columna «casos» de test-results.tsv: cada ID una vez, en orden, con FAIL si alguno de sus checks falló.</summary>
    public static string CaseSummary(IEnumerable<(string id, bool ok)> hits)
    {
        var order = new List<string>(); var fail = new HashSet<string>();
        foreach (var h in hits ?? Enumerable.Empty<(string, bool)>())
        {
            if (string.IsNullOrEmpty(h.id)) continue;
            if (!order.Contains(h.id)) order.Add(h.id);
            if (!h.ok) fail.Add(h.id);
        }
        return string.Join(", ", order.Select(id => id + (fail.Contains(id) ? " FAIL" : " OK")));
    }

    public static State StateOf(string text)
    {
        string n = Norm(StripMd(text));
        if (n.StartsWith("probado")) return n == "probado" || n.StartsWith("probado (") || n.StartsWith("probado ") ? State.Probado : State.Desconocido;
        if (n.StartsWith("parcial")) return State.Parcial;
        if (n.StartsWith("fail")) return State.Fail;
        if (n.StartsWith("inestable")) return State.Inestable;
        if (n.StartsWith("cronico")) return State.Cronico;
        if (n.StartsWith("solo test")) return State.SoloTest;
        if (n.StartsWith("solo codigo")) return State.SoloCodigo;
        if (n.StartsWith("sin probar")) return State.SinProbar;
        if (n.StartsWith("no aplica")) return State.NoAplica;
        if (n.StartsWith("retirado")) return State.Retirado;
        return State.Desconocido;
    }

    // ── Lectores ───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    static string[] SplitRow(string line)
    {
        string t = line.Trim();
        if (t.StartsWith("|")) t = t.Substring(1);
        if (t.EndsWith("|") && !t.EndsWith("\\|")) t = t.Substring(0, t.Length - 1);
        var cells = new List<string>(); var cur = new StringBuilder();
        for (int i = 0; i < t.Length; i++)
        {
            if (t[i] == '\\' && i + 1 < t.Length && t[i + 1] == '|') { cur.Append('|'); i++; continue; }
            if (t[i] == '|') { cells.Add(cur.ToString().Trim()); cur.Clear(); continue; }
            cur.Append(t[i]);
        }
        cells.Add(cur.ToString().Trim());
        return cells.ToArray();
    }

    static bool IsSeparator(string line) => Regex.IsMatch(line.Trim(), @"^\|?\s*:?-{2,}:?\s*(\|\s*:?-{2,}:?\s*)*\|?$");

    /// <summary>Las filas de todas las tablas de un markdown, con su cabecera, su línea y el título de su sección (## …).</summary>
    public static List<Row> Rows(string md)
    {
        var rows = new List<Row>();
        string[] lines = (md ?? "").Replace("\r", "").Split('\n');
        string section = ""; string[] header = null;
        for (int i = 0; i < lines.Length; i++)
        {
            string l = lines[i];
            if (l.StartsWith("#")) { section = l.TrimStart('#').Trim(); header = null; continue; }
            if (!l.TrimStart().StartsWith("|")) { header = null; continue; }
            if (header == null)
            {
                if (i + 1 < lines.Length && IsSeparator(lines[i + 1])) { header = SplitRow(l); i++; }
                continue;
            }
            rows.Add(new Row { Line = i + 1, Section = section, Header = header, Cells = SplitRow(l) });
        }
        return rows;
    }

    /// <summary>Las pruebas que cita un texto: «Unidad (gN)», «Unidad gN», «Unidad (grupo N)», «gN» solo, o una clase que acaba en Test/Auto/Checks.</summary>
    public static List<UnitRef> UnitRefs(string text)
    {
        var l = new List<UnitRef>();
        foreach (Match m in UnitRx.Matches(text ?? ""))
        {
            if (m.Groups[3].Success) l.Add(new UnitRef { Unit = m.Groups[3].Value, Group = -1 });
            else l.Add(new UnitRef { Unit = m.Groups[1].Success ? m.Groups[1].Value : null, Group = int.Parse(m.Groups[2].Value) });
        }
        return l;
    }

    static readonly Regex IdentRx = new Regex(@"\b[A-Z][A-Za-z0-9_]*[a-z][A-Za-z0-9_]*\b", RegexOptions.Compiled);
    static readonly Regex NumberRx = new Regex(@"(?<![\w.,])g?(\d+)\b", RegexOptions.Compiled);

    /// <summary>
    /// Las pruebas de una fila de casos.md. Con columnas «Unidad» y «Grupo» separadas (como en el catálogo): las clases de la una y los
    /// números de la otra, emparejados en orden si hay tantos de cada; si no, cada clase con grupo −1 y cada grupo suelto. Con una sola
    /// columna (o ninguna), <see cref="UnitRefs"/>.
    /// </summary>
    public static List<UnitRef> CaseUnits(Row r)
    {
        int ui = -1, gi = -1;
        for (int i = 0; i < r.Header.Length && i < r.Cells.Length; i++)
        {
            string h = Norm(r.Header[i]);
            if (gi < 0 && h.StartsWith("grupo")) gi = i;
            else if (ui < 0 && (h.Contains("unidad") || h.Contains("prueba") || h.Contains("test"))) ui = i;
        }
        if (ui >= 0 && gi >= 0)
        {
            List<UnitRef> inUnit = UnitRefs(r.Cells[ui]);
            var names = inUnit.Where(u => u.Unit != null).Select(u => u.Unit).Concat(IdentRx.Matches(StripMd(r.Cells[ui])).Cast<Match>().Select(m => m.Value)).Distinct().ToList();
            var nums = NumberRx.Matches(StripMd(r.Cells[gi])).Cast<Match>().Select(m => int.Parse(m.Groups[1].Value)).Concat(inUnit.Where(u => u.Group >= 0).Select(u => u.Group)).Distinct().ToList();
            var l = new List<UnitRef>();
            if (names.Count == nums.Count) for (int k = 0; k < names.Count; k++) l.Add(new UnitRef { Unit = names[k], Group = nums[k] });
            else { l.AddRange(names.Select(n => new UnitRef { Unit = n, Group = -1 })); l.AddRange(nums.Select(g => new UnitRef { Unit = null, Group = g })); }
            return l;
        }
        if (ui >= 0 || gi >= 0) return UnitRefs(string.Join(" ", new[] { ui, gi }.Where(i => i >= 0).Select(i => r.Cells[i])));
        return UnitRefs(string.Join(" ", r.Cells.Skip(1)));
    }

    /// <summary>Las unidades de prueba (clase → grupo) de unos archivos: cada clase que implementa ITestUnit, con el primer «int Group =&gt; N» que la sigue.</summary>
    public static Dictionary<string, int> TestUnits(Dictionary<string, string> sources)
    {
        var d = new Dictionary<string, int>();
        foreach (var kv in sources)
            foreach (Match c in ClassRx.Matches(kv.Value ?? ""))
            {
                Match g = GroupRx.Match(kv.Value, c.Index);
                if (g.Success) d[c.Groups[1].Value] = int.Parse(g.Groups[1].Value);
            }
        return d;
    }

    /// <summary>Los IDs con que empiezan los textos de los checks de un archivo («[CV-03] …»), en orden.</summary>
    public static List<string> CheckIds(string source) => SourceCheckRx.Matches(source ?? "").Cast<Match>().Select(m => m.Groups[1].Value).ToList();

    static HashSet<string> ManualRules(string manual)
    {
        var s = new HashSet<string>();
        foreach (Row r in Rows(manual)) { string id = IdOf(r.Cells[0]); if (ManualRuleRx.IsMatch(id)) s.Add(id); }
        return s;
    }

    static IEnumerable<string> RulesIn(string cell) => cell == null ? Enumerable.Empty<string>() : RuleRx.Matches(StripMd(cell)).Cast<Match>().Select(m => m.Value);

    // ── Las reglas ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    static readonly string[] UnitColumns = { "prueba", "unidad", "test", "grupo" };
    public static readonly string[] WatchedDocs = { "docs/checklist.md", "docs/manual-de-diseno.md", "docs/auditoria-peticiones.md" };
    public const string RetiredDoc = "docs/auditoria-peticiones.md";   // la sustituye peticiones.md: se puede borrar, no vaciar
    public static bool Watched(string path) => path.StartsWith("docs/qa/") && path.EndsWith(".md") || WatchedDocs.Contains(path);

    public static Report Run(Input inp)
    {
        var rep = new Report();
        foreach (string m in inp.Missing) rep.Add("falta", m, "no está o no se puede leer");
        rep.Warnings.AddRange(inp.Notes);

        // Catálogo y tabla de bugs.
        var cases = new Dictionary<string, Row>(); var retired = new HashSet<string>(); var bugs = new Dictionary<string, Row>();
        foreach (Row r in Rows(inp.Casos))
        {
            string id = IdOf(r.Cells[0]);
            if (IsCaseId(id))
            {
                if (cases.ContainsKey(id)) { rep.Add("duplicado", $"casos.md:{r.Line}", $"{id} ya está en la línea {cases[id].Line}"); continue; }
                cases[id] = r; if (Struck(r.Cells[0])) retired.Add(id);
            }
            else if (BugRx.IsMatch(id)) bugs[id] = r;
        }
        var requests = new Dictionary<string, Row>();
        foreach (Row r in Rows(inp.Peticiones)) { string id = IdOf(r.Cells[0]); if (PbRx.IsMatch(id)) { if (requests.ContainsKey(id)) rep.Add("duplicado", $"peticiones.md:{r.Line}", $"{id} ya está en la línea {requests[id].Line}"); else requests[id] = r; } }
        HashSet<string> rules = ManualRules(inp.Manual);
        Dictionary<string, int> units = TestUnits(inp.Sources);
        var groups = new HashSet<int>(units.Values);
        rep.Cases = cases.Count; rep.Bugs = bugs.Count; rep.Requests = requests.Count; rep.Units = units.Count;

        // §6.1 y §6.7: cada caso, su prueba y sus reglas.
        var caseRefs = new Dictionary<string, List<UnitRef>>();
        foreach (var kv in cases)
        {
            string id = kv.Key; Row r = kv.Value; string where = $"casos.md:{r.Line} {id}";
            if (retired.Contains(id)) continue;
            List<UnitRef> refs = caseRefs[id] = CaseUnits(r);
            foreach (UnitRef u in refs)
            {
                if (u.Unit != null && !units.ContainsKey(u.Unit)) rep.Add("unidad", where, $"la unidad {u.Unit} no existe en Assets/Scripts");
                else if (u.Unit != null && u.Group >= 0 && units[u.Unit] != u.Group) rep.Add("grupo", where, $"{u.Unit} es del grupo {units[u.Unit]}, no del {u.Group}");
                else if (u.Unit == null && !groups.Contains(u.Group)) rep.Add("grupo", where, $"no hay ninguna unidad del grupo {u.Group}");
            }
            foreach (string rule in RulesIn(r.Cell("regla"))) if (!rules.Contains(rule)) rep.Add("regla-desconocida", where, $"la regla {rule} no está en docs/manual-de-diseno.md");
        }

        // §6.2 y §6.3: el registro.
        var state = new Dictionary<string, State>(); var regText = new Dictionary<string, string>(); var regState = new Dictionary<string, string>();
        foreach (Row r in Rows(inp.Registro))
        {
            string id = IdOf(r.Cells[0]);
            if (!IsCaseId(id))
            {
                // «Inestables y crónicos» va por checks, no por casos: un check «Crónico» también pide su B-nn.
                string st0 = r.Cell("estado");
                if (st0 != null && StateOf(st0) == State.Cronico && !Regex.IsMatch(r.Text, @"\bB-\d{2,3}\b")) rep.Add("cronico-sin-bug", $"registro.md:{r.Line}", $"«{Short(r.Cells[0])}» está en «Crónico» sin su B-nn");
                continue;
            }
            string where = $"registro.md:{r.Line} {id}";
            regText[id] = (regText.TryGetValue(id, out string t) ? t + " | " : "") + r.Text;
            if (!cases.ContainsKey(id)) { rep.Add("registro-desconocido", where, "el ID no está en el catálogo (casos.md)"); continue; }
            string stCell = r.Cell("estado");
            if (stCell == null) { if (r.Text.Contains("Crónico") || r.Text.Contains("Cronico")) state.TryAdd(id, State.Cronico); continue; }   // otras tablas (Inestables y crónicos)
            if (state.ContainsKey(id)) { rep.Add("duplicado", where, "el caso ya tiene su fila de estado en el registro"); continue; }
            State st = StateOf(stCell); state[id] = st; regState[id] = StripMd(stCell); rep.RegistroRows++;
            if (st == State.Desconocido) { rep.Add("estado-desconocido", where, $"«{stCell}» no es un estado del registro"); continue; }
            if (st == State.Probado)
            {
                string runCell = r.Cell("run", "evidencia") ?? r.Text;
                Match run = RunLogRx.Match(runCell);
                if (!run.Success) rep.Add("probado-sin-run", where, "«Probado» sin enlace a su run log (runs/….md)");
                else if (!inp.RunLogs.Contains(run.Value)) rep.Add("probado-sin-run", where, $"el run log {run.Value} no existe en docs/qa");
                string commitCell = r.HasColumn("commit") ? r.Cell("commit") : r.Text;
                if (!CommitRx.IsMatch(commitCell ?? "")) rep.Add("probado-sin-commit", where, "«Probado» sin commit");
            }
        }
        foreach (string id in cases.Keys) if (!retired.Contains(id) && !state.ContainsKey(id)) rep.Add("sin-registro", $"casos.md:{cases[id].Line} {id}", "el caso no tiene fila de estado en el registro");

        // §6.1: un caso cita su prueba. Sin unidad ni grupo solo puede estar si aún no hay prueba (Sin probar, Solo código), si se vio en vivo
        // (Probado en vivo o retro, sin suite), si no aplica, o si es un OP o un PX. «Probado (suite gN)» cita además ese grupo.
        int untested = 0;
        foreach (var kv in caseRefs)
        {
            string id = kv.Key; string where = $"casos.md:{cases[id].Line} {id}";
            State st = state.TryGetValue(id, out State s0) ? s0 : State.SinProbar;
            string stText = regState.TryGetValue(id, out string t0) ? t0 : "";
            bool suite = st == State.Probado && Norm(stText).Contains("suite");
            if (kv.Value.Count == 0)
            {
                bool mayLack = id.StartsWith("OP-") || id.StartsWith("PX-") || st == State.SinProbar || st == State.SoloCodigo || st == State.NoAplica || st == State.Retirado || (st == State.Probado && !suite);
                if (mayLack) untested++;
                else rep.Add("caso-sin-prueba", where, $"está en «{stText}» y no cita ninguna unidad ni grupo (columnas «Unidad» y «Grupo», o «JornadaTest (g50)»)");
            }
            Match sg = Regex.Match(stText, @"\bg(\d+)\b");
            if (suite && sg.Success && kv.Value.Count > 0)
            {
                int g = int.Parse(sg.Groups[1].Value);
                if (!kv.Value.Any(u => u.Group == g || (u.Unit != null && units.TryGetValue(u.Unit, out int ug) && ug == g))) rep.Add("grupo", where, $"el registro dice «{stText}» y el caso no cita el grupo {g}");
            }
        }
        if (untested > 0) rep.Warnings.Add($"{untested} casos sin unidad ni grupo (en vivo, retro, sin probar o solo código): les toca su prueba cuando se pueda");

        // §6.5: crónicos con su B-nn (en el registro o en la tabla de bugs, que cita el caso).
        foreach (var kv in state.Where(k => k.Value == State.Cronico))
        {
            bool inReg = regText.TryGetValue(kv.Key, out string t) && Regex.IsMatch(t, @"\bB-\d{2,3}\b");
            bool inTable = bugs.Any(b => b.Key.StartsWith("B-") && Regex.IsMatch(b.Value.Text, $@"\b{Regex.Escape(kv.Key)}\b"));
            if (!inReg && !inTable) rep.Add("cronico-sin-bug", $"registro.md {kv.Key}", "«Crónico» pide su B-nn (y dueño)");
        }

        // §6.4: los checks con ID.
        var cited = new HashSet<string>();
        foreach (var kv in inp.Sources)
            foreach (string cid in CheckIds(kv.Value))
            {
                rep.CheckIds++; cited.Add(cid);
                if (!cases.ContainsKey(cid) && !requests.ContainsKey(cid)) rep.Add("check-desconocido", Path.GetFileName(kv.Key), $"un check empieza por [{cid}], que no está en casos.md ni en peticiones.md");
            }
        foreach (string id in cases.Keys.Where(k => k.StartsWith("CV-") && !retired.Contains(k)))
            if (!cited.Contains(id) && !(state.TryGetValue(id, out State s) && (s == State.NoAplica || s == State.Retirado)))
                rep.Add("cv-sin-check", $"casos.md:{cases[id].Line} {id}", $"ningún TestProbe.Check empieza por «[{id}]»");

        // §6.6: peticiones y su estado calculado.
        foreach (var kv in requests)
        {
            Row r = kv.Value; string where = $"peticiones.md:{r.Line} {kv.Key}";
            string casesCell = r.Cell("caso") ?? r.Text;
            List<string> pcases = AnyIdRx.Matches(casesCell).Cast<Match>().Select(m => m.Value).Where(IsCaseId).Distinct().ToList();
            bool hasHc = Regex.IsMatch(r.Text, @"\bHC-\d{2,3}\b");
            string stCell = r.Cell("estado") ?? "";
            string written = Norm(StripMd(stCell));
            bool doc = written == "doc" || r.Cells.Any(c => StripMd(c) == "Doc");
            foreach (string rule in RulesIn(r.Cell("regla"))) if (!rules.Contains(rule)) rep.Add("regla-desconocida", where, $"la regla {rule} no está en docs/manual-de-diseno.md");
            if (written.StartsWith("retirad") || written.StartsWith("no aplica")) continue;
            var allowed = new List<string>();
            if (pcases.Count == 0)
            {
                if (doc) allowed.Add("doc");
                else if (hasHc) allowed.Add("abierta");
                else { rep.Add("pb-sin-casos", where, "sin casos, sin HC y sin la marca «Doc»"); continue; }
            }
            else
            {
                var sts = pcases.Where(state.ContainsKey).Select(c => state[c]).Where(s => s != State.NoAplica && s != State.Retirado).ToList();
                if (hasHc) allowed.Add("abierta");
                else if (sts.Count > 0 && sts.All(s => s == State.Probado)) allowed.Add("hecha");
                else if (sts.Any(s => s == State.SoloCodigo)) allowed.Add("sin verificar");
                else { allowed.Add("sin verificar"); allowed.Add("abierta"); }   // Sin probar, FAIL, Parcial…: falta verificar o falta código
                if (doc && sts.Count == 0) allowed.Add("doc");
            }
            if (!allowed.Contains(written)) rep.Add("pb-estado", where, $"pone «{stCell}» y le toca {string.Join(" o ", allowed.Select(a => "«" + a + "»"))} (casos: {(pcases.Count > 0 ? string.Join(", ", pcases.Select(c => c + "=" + (state.TryGetValue(c, out State s) ? s.ToString() : "?"))) : "ninguno")})");
        }

        // IDs citados en las tablas de los docs de QA (la prosa puede hablar de IDs futuros, «las nuevas siguen desde PB-156»): casos, PB y
        // bugs que existan.
        var docs = new List<(string name, string text)> { ("casos.md", inp.Casos), ("registro.md", inp.Registro), ("peticiones.md", inp.Peticiones) };
        docs.AddRange(inp.OtherDocs.Select(k => (k.Key, k.Value)));
        foreach (var (name, text) in docs)
        {
            var seen = new HashSet<string>();
            foreach (Row r in Rows(text))
                foreach (Match m in AnyIdRx.Matches(r.Text))
                {
                    string id = m.Value; if (!seen.Add(id)) continue;
                    string where = $"{name}:{r.Line}";
                    if (BugRx.IsMatch(id)) { if (!bugs.ContainsKey(id)) rep.Add("bug-desconocido", where, $"{id} no está en la tabla «Bugs y huecos» de casos.md"); }
                    else if (PbRx.IsMatch(id)) { if (!requests.ContainsKey(id)) rep.Add("id-desconocido", where, $"{id} no está en peticiones.md"); }
                    else if (id.StartsWith("PB-")) rep.Add("id-desconocido", where, $"{id}: una petición lleva tres cifras (PB-nnn)");
                    else if (!cases.ContainsKey(id)) rep.Add("id-desconocido", where, $"{id} no está en el catálogo (casos.md)");
                }
        }

        // §6.7 (aviso): reglas del manual que dicen «Se comprueba en (grupo N)» y no tienen caso.
        var ruleCases = new HashSet<string>(cases.Values.SelectMany(r => RulesIn(r.Cell("regla"))));
        var uncased = new List<string>();
        foreach (Row r in Rows(inp.Manual))
        {
            string id = IdOf(r.Cells[0]); string chk = r.Cell("se comprueba");
            if (ManualRuleRx.IsMatch(id) && chk != null && Regex.IsMatch(chk, @"\b(grupo\s*)?\d+\b") && !Norm(chk).Contains("solo en docs") && !ruleCases.Contains(id)) uncased.Add(id);
        }
        if (uncased.Count > 0) rep.Warnings.Add($"{uncased.Count} reglas del manual dicen «Se comprueba en» una prueba y ningún caso las cita: {string.Join(", ", uncased)}");

        // §6.8: la lista rápida.
        foreach (Match m in PresetRx.Matches(inp.Presets ?? ""))
        {
            string u = m.Groups[1].Value; int g = int.Parse(m.Groups[2].Value);
            if (!units.ContainsKey(u)) rep.Add("preset", "TestPresetsData.cs", $"la unidad {u} de la lista rápida ya no existe");
            else if (units[u] != g) rep.Add("preset", "TestPresetsData.cs", $"{u} es del grupo {units[u]}, la lista dice {g}");
        }

        // §6.9: un doc de QA que encoge más de la mitad, se vacía o desaparece (82878fc vació checklist.md y auditoria-peticiones.md).
        foreach (SizeChange s in inp.Sizes.Where(s => Watched(s.Path) && s.Before > 0))
        {
            if (s.Now < 0) { if (s.Path != RetiredDoc) rep.Add("encoge", s.Path, $"{s.Where}: el doc ya no está (tenía {s.Before} bytes)"); }
            else if (s.Now * 2 < s.Before) rep.Add("encoge", s.Path, $"{s.Where}: de {s.Before} a {s.Now} bytes (más de un 50 % menos{(s.Now == 0 ? ", vacío" : "")})");
        }

        // §6.10: TmpCheck no va al repositorio.
        foreach (string f in inp.Tracked.Where(f => f.Replace('\\', '/').Contains("/TmpCheck/") || f.Replace('\\', '/').StartsWith("TmpCheck/")))
            rep.Add("tmpcheck", f, "los archivos de TmpCheck son del probador y no van a git (OP-05)");

        return rep;
    }

    // ── Lectura del disco y de git (para el menú, la prueba y la línea de órdenes) ─────────────────────────────────────────────────────

    public const string RootFile = "Temp/qa-root.txt";

    /// <summary>El repositorio con docs/qa: el que dice Temp/qa-root.txt del proyecto (lo escribe tools/sync-to-unity.sh), el de
    /// COLD_SANCTUARY_REPO, o el propio proyecto. Null si ninguno lo tiene.</summary>
    public static string FindRoot(string project)
    {
        var c = new List<string>();
        try { string f = Path.Combine(project ?? "", RootFile); if (File.Exists(f)) c.Add(File.ReadAllText(f).Trim()); } catch { }
        string env = System.Environment.GetEnvironmentVariable("COLD_SANCTUARY_REPO");
        if (!string.IsNullOrEmpty(env)) c.Add(env);
        if (project != null) c.Add(project);
        foreach (string r in c) if (r.Length > 0 && Directory.Exists(Path.Combine(r, "docs", "qa"))) return r;
        return null;
    }

    static string ReadOr(string root, string rel, Input inp, bool required)
    {
        string p = Path.Combine(root, rel);
        try { if (File.Exists(p)) return File.ReadAllText(p, Encoding.UTF8); } catch { }
        if (required) inp.Missing.Add(rel);
        return "";
    }

    public static Input Load(string root)
    {
        var inp = new Input();
        inp.Casos = ReadOr(root, "docs/qa/casos.md", inp, true);
        inp.Registro = ReadOr(root, "docs/qa/registro.md", inp, true);
        inp.Peticiones = ReadOr(root, "docs/qa/peticiones.md", inp, true);
        inp.Manual = ReadOr(root, "docs/manual-de-diseno.md", inp, true);
        inp.Presets = ReadOr(root, "Assets/Scripts/TestPresetsData.cs", inp, false);
        foreach (string other in new[] { "entrelazamiento.md", "hallazgos.md", "contadores.md" })
        {
            string t = ReadOr(root, "docs/qa/" + other, inp, false);
            if (t.Length > 0) inp.OtherDocs[other] = t;
        }
        string runs = Path.Combine(root, "docs", "qa", "runs");
        if (Directory.Exists(runs)) foreach (string f in Directory.GetFiles(runs, "*.md")) inp.RunLogs.Add("runs/" + Path.GetFileName(f));
        string scripts = Path.Combine(root, "Assets", "Scripts");
        if (Directory.Exists(scripts))
            foreach (string f in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
            {
                string src; try { src = File.ReadAllText(f); } catch { continue; }
                if (src.Contains("ITestUnit") || src.Contains("TestProbe.")) inp.Sources[f.Substring(root.Length).TrimStart('\\', '/').Replace('\\', '/')] = src;
            }
        LoadGit(root, inp);
        return inp;
    }

    /// <summary>Tamaños de los docs vigilados: el último commit frente a su padre, y lo que hay en disco frente al último commit. Y los archivos en git.</summary>
    static void LoadGit(string root, Input inp)
    {
        string head = Git(root, "ls-tree -r -l HEAD -- docs"), parent = Git(root, "ls-tree -r -l HEAD^ -- docs"), files = Git(root, "ls-files");
        if (head == null || files == null) { inp.Notes.Add("sin git: no compruebo si un doc encoge ni si hay TmpCheck en el repositorio"); return; }
        Dictionary<string, long> h = Sizes(head), p = parent != null ? Sizes(parent) : new Dictionary<string, long>();
        foreach (string path in h.Keys.Union(p.Keys).Where(Watched))
            inp.Sizes.Add(new SizeChange(path, h.TryGetValue(path, out long a) ? a : -1, p.TryGetValue(path, out long b) ? b : -1, "último commit"));
        foreach (var kv in h.Where(k => Watched(k.Key)))
        {
            string f = Path.Combine(root, kv.Key);
            inp.Sizes.Add(new SizeChange(kv.Key, File.Exists(f) ? new FileInfo(f).Length : -1, kv.Value, "sin commitear"));
        }
        inp.Tracked.AddRange(files.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0));
    }

    static Dictionary<string, long> Sizes(string lsTree)
    {
        var d = new Dictionary<string, long>();
        foreach (string line in lsTree.Split('\n'))
        {
            int tab = line.IndexOf('\t'); if (tab < 0) continue;
            string[] meta = line.Substring(0, tab).Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (meta.Length >= 4 && long.TryParse(meta[3], out long n)) d[Unquote(line.Substring(tab + 1).Trim())] = n;
        }
        return d;
    }

    // git cita las rutas con tildes («"docs/manual-de-dise\303\261o.md"»); con core.quotePath=false no, pero por si acaso.
    static string Unquote(string p)
    {
        if (!(p.Length > 1 && p[0] == '"' && p[p.Length - 1] == '"')) return p;
        var bytes = new List<byte>(); string s = p.Substring(1, p.Length - 2);
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '\\' && i + 3 < s.Length && char.IsDigit(s[i + 1])) { bytes.Add(System.Convert.ToByte(s.Substring(i + 1, 3), 8)); i += 3; }
            else if (s[i] == '\\' && i + 1 < s.Length) { bytes.Add((byte)s[i + 1]); i++; }
            else bytes.AddRange(Encoding.UTF8.GetBytes(s[i].ToString()));
        }
        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    /// <summary>Corre git con tope de 20 s (nunca espera sin límite). Null si falla.</summary>
    static string Git(string root, string args)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git", "-c core.quotePath=false " + args)
            { WorkingDirectory = root, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true, StandardOutputEncoding = Encoding.UTF8 };
            using (var p = System.Diagnostics.Process.Start(psi))
            {
                var task = p.StandardOutput.ReadToEndAsync();
                p.StandardError.ReadToEndAsync();
                if (!p.WaitForExit(20000)) { try { p.Kill(); } catch { } return null; }
                return p.ExitCode == 0 && task.Wait(5000) ? task.Result : null;
            }
        }
        catch { return null; }
    }

    /// <summary>
    /// La línea de órdenes del qa-check.sh: «QaCheck [--root RUTA]» (por defecto, la carpeta actual). Escribe el informe y devuelve
    /// 0 si cuadra, 1 si hay problemas y 2 si no encuentra docs/qa. Para compilarlo sin Unity basta este archivo y un Main que lo llame.
    /// </summary>
    public static int Cli(string[] args, TextWriter output)
    {
        string root = Directory.GetCurrentDirectory();
        for (int i = 0; i < (args?.Length ?? 0); i++) if (args[i] == "--root" && i + 1 < args.Length) root = args[++i]; else if (!args[i].StartsWith("-")) root = args[i];
        if (!Directory.Exists(Path.Combine(root, "docs", "qa"))) { output.WriteLine($"[QA] no encuentro docs/qa en {root}"); return 2; }
        Report rep = Run(Load(root));
        output.Write(rep.Text());
        return rep.Problems.Count == 0 ? 0 : 1;
    }
}
