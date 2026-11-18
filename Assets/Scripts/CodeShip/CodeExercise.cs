using System.Collections.Generic;

/// <summary>
/// UN RETO DE CÓDIGO (portado de CodeShip, src/modules/mind-duel/domain/exercise.ts; docs/plan-microcosmos-realismo.md §4.4). Datos puros,
/// sin Unity, para probarlo sin escena (<see cref="CodeShipLogicTest"/>). Dos clases, como en CodeShip:
///  · <see cref="Kind.Order"/>: poner los bloques en orden (problema de Parsons). Los bloques que no están en <see cref="solution"/> son
///    distracciones.
///  · <see cref="Kind.Choice"/>: leer el código y elegir una opción (completar, predecir, reparar o especificar).
/// Cada bloque tiene dos caras: pseudocódigo en español (la de siempre) y JavaScript (la vista «JS» de la hoja).
/// </summary>
[System.Serializable]
public class CodeBlock
{
    public string id = "", pseudo = "", js = "";
    /// <summary>Sangría al mostrarlo (0 = arriba del todo).</summary>
    public int indent;
}

public enum CodeSkill { Completar, Predecir, Reparar, Especificar }

[System.Serializable]
public class CodeExercise
{
    public enum Kind { Order, Choice }

    public string id = "";
    public Kind kind;
    /// <summary>La idea que enseña, en una frase (se ve arriba de la hoja).</summary>
    public string idea = "";
    /// <summary>Elegir: la pregunta. Ordenar: la orden («Ordena…»).</summary>
    public string question = "";
    /// <summary>Ordenar: las piezas. Elegir: el código que se lee.</summary>
    public CodeBlock[] blocks = new CodeBlock[0];
    /// <summary>Ordenar: los ids en el orden correcto.</summary>
    public string[] solution = new string[0];
    /// <summary>Elegir: las opciones y la buena.</summary>
    public string[] options = new string[0];
    public int correct;
    public CodeSkill skill = CodeSkill.Completar;
    /// <summary>De dónde salen los datos (la regla de CodeShip y la M39 de Cold Sanctuary: todo con fuente).</summary>
    public string source = "";
    /// <summary>Lo que se cuenta al acertar (qué pasó de verdad).</summary>
    public string explain = "";

    public bool HasDistractors => kind == Kind.Order && blocks.Length > solution.Length;

    public CodeBlock Block(string blockId)
    {
        foreach (CodeBlock b in blocks) if (b != null && b.id == blockId) return b;
        return null;
    }

    public bool IsCorrect(CodeAnswer a)
    {
        if (a == null) return false;
        if (kind == Kind.Order && a.kind == Kind.Order)
        {
            if (a.order == null || a.order.Length != solution.Length) return false;
            for (int i = 0; i < solution.Length; i++) if (a.order[i] != solution[i]) return false;
            return true;
        }
        return kind == Kind.Choice && a.kind == Kind.Choice && a.index == correct;
    }

    /// <summary>Cuántos bloques del principio están ya en su sitio (la pista tras un fallo: «los 3 primeros están bien»).</summary>
    public int CorrectPrefix(IList<string> order)
    {
        int n = 0;
        if (order == null) return 0;
        while (n < order.Count && n < solution.Length && order[n] == solution[n]) n++;
        return n;
    }

    /// <summary>Los ids de los bloques barajados con <paramref name="seed"/> (siempre igual con la misma semilla). Nunca empieza ya
    /// resuelto: si el azar deja la solución delante, se cambian los dos primeros.</summary>
    public string[] Shuffled(int seed)
    {
        var ids = new string[blocks.Length];
        for (int i = 0; i < blocks.Length; i++) ids[i] = blocks[i].id;
        var rng = new System.Random(unchecked(seed * 486187739 + StableHash(id)));
        for (int i = ids.Length - 1; i > 0; i--) { int j = rng.Next(i + 1); (ids[i], ids[j]) = (ids[j], ids[i]); }
        if (kind == Kind.Order && solution.Length > 1 && CorrectPrefix(ids) >= solution.Length) (ids[0], ids[1]) = (ids[1], ids[0]);
        return ids;
    }

    /// <summary>Lo que está mal en los datos del reto (vacío = bien formado): ids repetidos o vacíos, una solución con bloques que no
    /// existen, una respuesta fuera de las opciones…</summary>
    public string Validate()
    {
        var bad = new List<string>();
        if (string.IsNullOrEmpty(id)) bad.Add("sin id");
        var seen = new HashSet<string>();
        foreach (CodeBlock b in blocks)
        {
            if (b == null || string.IsNullOrEmpty(b.id)) { bad.Add("un bloque sin id"); continue; }
            if (!seen.Add(b.id)) bad.Add($"bloque repetido «{b.id}»");
            if (string.IsNullOrEmpty(b.pseudo)) bad.Add($"«{b.id}» sin pseudocódigo");
        }
        if (kind == Kind.Order)
        {
            if (solution.Length < 2) bad.Add("ordenar necesita al menos 2 bloques en la solución");
            var used = new HashSet<string>();
            foreach (string s in solution)
            {
                if (!seen.Contains(s)) bad.Add($"la solución usa «{s}», que no es un bloque");
                if (!used.Add(s)) bad.Add($"la solución repite «{s}»");
            }
        }
        else
        {
            if (string.IsNullOrEmpty(question)) bad.Add("elegir sin pregunta");
            if (options.Length < 2) bad.Add("elegir necesita al menos 2 opciones");
            if (correct < 0 || correct >= options.Length) bad.Add($"la respuesta {correct} está fuera de las {options.Length} opciones");
        }
        return string.Join("; ", bad);
    }

    static int StableHash(string s)
    {
        unchecked
        {
            int h = (int)2166136261;
            if (s != null) foreach (char c in s) h = (h ^ c) * 16777619;
            return h;
        }
    }
}

/// <summary>Una respuesta: un orden de bloques o una opción.</summary>
public class CodeAnswer
{
    public CodeExercise.Kind kind;
    public string[] order = new string[0];
    public int index = -1;

    public static CodeAnswer Order(params string[] ids) => new CodeAnswer { kind = CodeExercise.Kind.Order, order = ids ?? new string[0] };
    public static CodeAnswer Choice(int i) => new CodeAnswer { kind = CodeExercise.Kind.Choice, index = i };
    /// <summary>La misma respuesta da la misma clave.</summary>
    public string Key => kind == CodeExercise.Kind.Choice ? $"c:{index}" : "o:" + string.Join(",", order);
}

/// <summary>
/// LOS FALLOS YA DADOS al reto que está en pantalla (CodeShip attempts.ts). Dar otra vez la misma respuesta mala no es un fallo nuevo: no
/// cuesta nada y no deshace el primero. (En CodeShip, tres toques sobre la misma opción errónea marcaban, desmarcaban y terminaban la misión.)
/// </summary>
public class CodeAttempts
{
    string _exercise;
    readonly HashSet<string> _wrong = new HashSet<string>();
    public readonly List<int> WrongChoices = new List<int>();
    public readonly List<string[]> WrongOrders = new List<string[]>();

    /// <summary>El reto que está ahora en pantalla; si es otro, olvida los fallos.</summary>
    public void On(CodeExercise ex)
    {
        string id = ex != null ? ex.id : null;
        if (id == _exercise) return;
        _exercise = id;
        _wrong.Clear(); WrongChoices.Clear(); WrongOrders.Clear();
    }

    public bool Repeated(CodeAnswer a) => a != null && _wrong.Contains(a.Key);

    public void Record(CodeAnswer a, bool ok)
    {
        if (a == null || ok || Repeated(a)) return;
        _wrong.Add(a.Key);
        if (a.kind == CodeExercise.Kind.Choice) WrongChoices.Add(a.index);
        else WrongOrders.Add((string[])a.order.Clone());
    }

    public int Count => _wrong.Count;
}
