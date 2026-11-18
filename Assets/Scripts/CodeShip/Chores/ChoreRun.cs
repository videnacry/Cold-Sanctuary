using System;
using System.Collections.Generic;
using System.Text;

/// <summary>Los grupos de la lista de tareas de CodeShip (chore.ts CHORE_CATEGORY), más el taller de los puestos de trabajo y las ideas.</summary>
public enum ChoreCategory { Comida, Plantas, Animales, Taller, Salud, Ideas, Ciencia, Seguridad }

/// <summary>
/// UNA PLANTILLA DE MISIÓN POR RETOS DE CÓDIGO (portada de CodeShip, src/modules/chores/domain/chore.ts y application/catalog.ts): una tarea que
/// se hace en un puesto resolviendo sus retos EN ORDEN (cocinar = un reto por paso de la receta; yoga = un reto por postura; curar = triaje
/// ABCDE y el protocolo…). Datos puros: cualquier microcosmos la instancia en un puesto (<see cref="MissionSites"/>, <see cref="StationMissions"/>).
/// <see cref="duel"/> = «plantar una idea»: no lleva retos fijos, sino un duelo de ideas (<see cref="IdeaDuel"/>) con la curva de dificultad.
/// </summary>
[Serializable]
public class ChoreTemplate
{
    /// <summary>Id estable («cocinar-huevos-revueltos»).</summary>
    public string id = "";
    /// <summary>La clase de CodeShip (ChoreKind: «cocinar», «yoga», «atender-paciente», «alimentar-perro»…) o las nuevas de los puestos de la nave.</summary>
    public string kind = "";
    public ChoreCategory category;
    public string title = "", scenario = "";
    /// <summary>De dónde salen los pasos y los valores (regla de CodeShip §1 y M39 de Cold Sanctuary).</summary>
    public string source = "";
    /// <summary>«real» (así se hace), «licencia» (adaptado al microcosmos) o «supuesto».</summary>
    public string label = "real";
    public CodeExercise[] exercises = new CodeExercise[0];
    /// <summary>Fallos que aguanta (CodeShip: 2).</summary>
    public int maxMistakes = 2;
    /// <summary>Duelo de ideas: su dificultad de partida (0..1); sube con cada idea plantada.</summary>
    public bool duel;
    public float duelLevel;

    public string Validate()
    {
        var bad = new List<string>();
        if (string.IsNullOrEmpty(id)) bad.Add("sin id");
        if (string.IsNullOrEmpty(kind)) bad.Add("sin clase");
        if (string.IsNullOrEmpty(title)) bad.Add("sin título");
        if (string.IsNullOrEmpty(source)) bad.Add("sin fuente");
        if (!duel && exercises.Length == 0) bad.Add("sin retos");
        var ids = new HashSet<string>();
        foreach (CodeExercise e in exercises)
        {
            if (e == null) { bad.Add("un reto vacío"); continue; }
            if (!ids.Add(e.id)) bad.Add($"reto repetido «{e.id}»");
            string v = e.Validate();
            if (v.Length > 0) bad.Add($"{e.id}: {v}");
        }
        return string.Join("; ", bad);
    }
}

/// <summary>
/// HACER UNA TAREA (CodeShip ChoreRun): los retos en el orden dado (nunca barajados). Cada acierto es un paso hecho; demasiados fallos y hay
/// que reintentarla. Un fallo queda apuntado para su paso: dar otra vez la misma respuesta mala no es un fallo nuevo.
/// </summary>
public class ChoreRun
{
    public enum State { Doing, Done, Failed }

    public readonly ChoreTemplate template;
    protected readonly CodeAttempts attempts = new CodeAttempts();
    protected int step, mistakes;
    protected State status = State.Doing;
    readonly int _maxMistakes;

    public ChoreRun(ChoreTemplate t)
    {
        template = t;
        _maxMistakes = t != null ? Math.Max(1, t.maxMistakes) : 2;
        if (t != null && !t.duel && t.exercises.Length == 0) throw new ArgumentException($"La tarea {t.id} no tiene retos");
    }

    protected ChoreRun(ChoreTemplate t, int maxMistakes) { template = t; _maxMistakes = Math.Max(1, maxMistakes); }

    public State Status => status;
    public int Step => step;
    public virtual int Steps => template.exercises.Length;
    public int Mistakes => mistakes;
    public virtual int MaxMistakes => _maxMistakes;
    public int MistakesLeft => MaxMistakes - mistakes;
    public virtual CodeExercise Current => status == State.Doing && step < template.exercises.Length ? template.exercises[step] : null;
    /// <summary>Los fallos del reto de ahora (la hoja marca las opciones ya falladas).</summary>
    public CodeAttempts Attempts { get { attempts.On(Current); return attempts; } }

    /// <summary>¿Ya se dio, mal, esta respuesta al reto de ahora?</summary>
    public bool Tried(CodeAnswer a) { attempts.On(Current); return attempts.Repeated(a); }

    /// <summary>true = acertó. Una respuesta mala repetida devuelve false y no cuenta nada.</summary>
    public virtual bool Submit(CodeAnswer a)
    {
        CodeExercise ex = Current;
        if (ex == null) return false;
        if (Tried(a)) return false;
        bool ok = ex.IsCorrect(a);
        attempts.Record(a, ok);
        if (ok) Advance();
        else if (++mistakes >= MaxMistakes) status = State.Failed;
        return ok;
    }

    protected virtual void Advance()
    {
        step++;
        if (step >= Steps) status = State.Done;
    }
}

/// <summary>
/// LA CURVA DE DIFICULTAD (CodeShip difficulty.ts duelProfileFor): de 0 (fácil) a 1 (lo más difícil), cuántas ideas hay que acertar, cuántos
/// fallos se aguantan, cuántos bloques como mucho, si hay bloques de distracción (desde 0,35) y hasta qué concepto del índice de estudio.
/// </summary>
public struct DifficultyProfile
{
    public float difficulty;
    public int persuasionGoal, patience, maxBlocks, maxConcept;
    public bool distractors;

    public static DifficultyProfile For(float d)
    {
        d = Math.Min(1f, Math.Max(0f, d));
        return new DifficultyProfile
        {
            difficulty = d,
            persuasionGoal = 3 + (int)Math.Round(d * 5, MidpointRounding.AwayFromZero),
            patience = 5 - (int)Math.Round(d * 3, MidpointRounding.AwayFromZero),
            maxBlocks = 4 + (int)Math.Round(d * 6, MidpointRounding.AwayFromZero),
            distractors = d >= 0.35f - 1e-6f,
            maxConcept = 1 + Math.Min(9, (int)Math.Floor(d * 10f + 1e-4f)),
        };
    }

    /// <summary>En CodeShip la dificultad sale de la masa y la riqueza del asteroide; aquí, de la del puesto y de las ideas ya plantadas.</summary>
    public static float Level(float baseLevel, int planted) => Math.Min(1f, baseLevel + 0.15f * planted);
}

/// <summary>
/// PLANTAR UNA IDEA (CodeShip mind-duel duel.ts): convencer a alguien acertando <see cref="DifficultyProfile.persuasionGoal"/> ideas antes de
/// agotar su paciencia. Los retos salen del banco, filtrados por la curva (concepto, bloques, distracciones), en un orden fijado por la semilla.
/// </summary>
public class IdeaDuel : ChoreRun
{
    public readonly DifficultyProfile profile;
    public readonly List<CodeExercise> Pool = new List<CodeExercise>();
    readonly int _goal;

    public IdeaDuel(IList<CodeExercise> bank, DifficultyProfile p, int seed, ChoreTemplate t = null) : base(t ?? new ChoreTemplate { id = "duelo", kind = "plantar-idea", title = "Plantar una idea", source = "CodeShip", duel = true }, p.patience)
    {
        profile = p;
        foreach (CodeExercise e in bank)
            if (e.concept <= p.maxConcept && ChoreKit.BlockCount(e) <= p.maxBlocks && (p.distractors || !e.HasDistractors)) Pool.Add(e);
        if (Pool.Count == 0) foreach (CodeExercise e in bank) if (e.concept <= 1) Pool.Add(e);   // nunca un duelo vacío
        // Orden fijado por la semilla (el duelo de un asteroide baraja; la tarea no).
        var rng = new Random(seed);
        for (int i = Pool.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); (Pool[i], Pool[j]) = (Pool[j], Pool[i]); }
        _goal = Math.Max(1, Math.Min(p.persuasionGoal, Pool.Count));
    }

    public override int Steps => _goal;
    public override CodeExercise Current => status == State.Doing && step < Pool.Count ? Pool[step] : null;
}

/// <summary>
/// LOS AYUDANTES QUE CONVIERTEN DATOS EN RETOS (CodeShip catalog.ts): ordenar, elegir (con la buena en un sitio estable que solo depende del
/// id), y la cadena de pasos (<see cref="StepChain"/>: un reto por paso de un procedimiento real; «{bien|mal|mal}» pide el valor).
/// </summary>
public static class ChoreKit
{
    public static uint Hash(string s)
    {
        unchecked
        {
            uint h = 2166136261;
            foreach (char c in s ?? "") h = (h ^ c) * 16777619;
            return h;
        }
    }

    /// <summary>«lava las hojas» → «lavaLasHojas();» (sin tildes ni signos, cuatro palabras como mucho).</summary>
    public static string ToCall(string step)
    {
        string n = (step ?? "").Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in n)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
            sb.Append(c < 128 && char.IsLetterOrDigit(c) ? c : ' ');
        }
        string[] w = sb.ToString().Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var o = new StringBuilder();
        for (int i = 0; i < Math.Min(4, w.Length); i++)
            o.Append(i == 0 ? w[i].ToLowerInvariant() : char.ToUpperInvariant(w[i][0]) + w[i].Substring(1).ToLowerInvariant());
        return o + "();";
    }

    public static CodeBlock Block(string id, string pseudo, string js = null, int indent = 0) => new CodeBlock { id = id, pseudo = pseudo, js = js ?? ToCall(pseudo), indent = indent };

    public static int BlockCount(CodeExercise e) => e.kind == CodeExercise.Kind.Order ? e.blocks.Length : e.blocks.Length;

    /// <summary>Ordenar los pasos, tal cual (los bloques son a, b, c…; la solución, ese orden).</summary>
    public static CodeExercise Order(string id, int concept, string idea, params string[] steps)
    {
        var blocks = new CodeBlock[steps.Length];
        var sol = new string[steps.Length];
        for (int i = 0; i < steps.Length; i++) { blocks[i] = Block(((char)('a' + i)).ToString(), steps[i]); sol[i] = blocks[i].id; }
        return new CodeExercise { id = id, kind = CodeExercise.Kind.Order, concept = concept, idea = idea, question = "Ordena los pasos.", blocks = blocks, solution = sol, skill = CodeSkill.Especificar };
    }

    /// <summary>Ordenar con un bloque de distracción (no va en la solución).</summary>
    public static CodeExercise OrderWithDistractor(string id, int concept, string idea, string distractor, params string[] steps)
    {
        CodeExercise e = Order(id, concept, idea, steps);
        var l = new List<CodeBlock>(e.blocks) { Block("x", distractor) };
        e.blocks = l.ToArray();
        e.question = "Ordena los pasos. Sobra un bloque.";
        return e;
    }

    /// <summary>Elegir, con la buena en un sitio estable que depende del id.</summary>
    public static CodeExercise Choice(string id, int concept, CodeSkill skill, string idea, string question, CodeBlock[] code, string right, IList<string> wrong)
    {
        var opts = new List<string>();
        foreach (string w in wrong) if (w != right && !opts.Contains(w)) opts.Add(w);
        int at = (int)(Hash(id) % (uint)(opts.Count + 1));
        opts.Insert(at, right);
        return new CodeExercise { id = id, kind = CodeExercise.Kind.Choice, concept = concept, skill = skill, idea = idea, question = question, blocks = code ?? new CodeBlock[0], options = opts.ToArray(), correct = at };
    }

    static readonly System.Text.RegularExpressions.Regex Param = new System.Text.RegularExpressions.Regex(@"\{([^}]+)\}");

    /// <summary>«cocina hasta {71|50|100} °C» → «cocina hasta 71 °C».</summary>
    public static string PlainStep(string step) => Param.Replace(step, m => m.Groups[1].Value.Split('|')[0]);

    /// <summary>
    /// Un procedimiento en el que cada paso es un reto (CodeShip stepChain): elegir el paso que viene (con los dos anteriores como código), o
    /// rellenar el valor real si el paso lo lleva («{bien|mal|mal}»). Los pasos de <paramref name="pool"/> sirven de respuestas malas creíbles.
    /// </summary>
    public static CodeExercise[] StepChain(string idBase, int concept, Func<int, string> idea, IList<string> steps, IList<string> pool)
    {
        var plain = new List<string>();
        foreach (string s in steps) plain.Add(PlainStep(s));
        var outp = new CodeExercise[steps.Count];
        for (int i = 0; i < steps.Count; i++)
        {
            var prev = new List<CodeBlock>();
            for (int n = Math.Max(0, i - 2); n < i; n++) prev.Add(Block(n.ToString(), $"{n + 1}. {plain[n]}", ToCall(plain[n])));
            string id = $"{idBase}-{i + 1}";
            var m = Param.Match(steps[i]);
            if (m.Success)
            {
                string[] parts = m.Groups[1].Value.Split('|');
                string after = steps[i].Substring(m.Index + m.Length).Trim();
                string unit = after.Length > 0 ? after.Split(' ')[0] : "";
                Func<string, string> u = v => unit.Length > 0 ? $"{v} {unit}" : v;
                string gap = Param.Replace(steps[i], "___");
                prev.Add(Block("x", $"{i + 1}. {gap}", ToCall(gap.Replace("___", "")).Replace("();", "(___);")));
                var wrong = new List<string>();
                for (int k = 1; k < parts.Length; k++) wrong.Add(u(parts[k]));
                outp[i] = Choice(id, 2, CodeSkill.Completar, idea(i), $"Paso {i + 1} de {steps.Count}: ¿qué valor falta?", prev.ToArray(), u(parts[0]), wrong);
                continue;
            }
            string later = i + 1 < plain.Count ? plain[i + 1] : plain.Find(x => plain.IndexOf(x) < i - 2);
            string foreign = null;
            if (pool != null) foreach (string x in pool) if (!plain.Contains(x) && x != later) { foreign = x; break; }
            foreign = foreign ?? "tira todo a la basura";
            var bad = new List<string>();
            foreach (string x in new[] { later, foreign }) if (!string.IsNullOrEmpty(x) && x != plain[i]) bad.Add(x);
            if (bad.Count < 2) bad.Add("espera sin hacer nada");
            string q = i == 0 ? $"¿Con qué paso empieza? ({steps.Count} pasos)" : $"Paso {i + 1} de {steps.Count}: ¿qué viene ahora?";
            outp[i] = Choice(id, concept, CodeSkill.Predecir, idea(i), q, prev.ToArray(), plain[i], bad);
        }
        return outp;
    }
}
