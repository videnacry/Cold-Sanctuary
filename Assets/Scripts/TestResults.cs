using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// RESULTADOS DE LA BATERÍA, para máquinas (2026-11-11, pruebas rápidas; scratchpad analisis-pruebas.md §3a): una fila por unidad —
/// grupo, unidad, PASS, FAIL, segundos reales, estado (OK, FALLOS, TIMEOUT, EXCEPCIÓN) y los textos de sus FAIL— en
/// <see cref="Path"/> (TSV con cabecera). Se reescribe tras cada unidad, así que una batería cortada también deja su archivo. Con él, el
/// probador vuelve a correr solo lo que falló, y <c>PruebasRapidas</c> rehace la lista rápida.
/// </summary>
public static class TestResults
{
    public const string Path = "Logs/test-results.tsv";
    // «casos» (2026-11-13, sistema de QA): los IDs de caso de los checks de la unidad, «CV-03 OK, CV-07 FAIL». Va al final para no mover las otras.
    public const string Header = "grupo\tunidad\tpass\tfail\tsegundos\testado\tfallos\tcasos";

    public class Row
    {
        public int group; public string unit = ""; public int pass, fail; public float seconds; public string status = ""; public List<string> fails = new List<string>(); public string cases = "";
        public string ToTsv() => $"{group}\t{Clean(unit)}\t{pass}\t{fail}\t{seconds.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}\t{status}\t{string.Join(" | ", fails.Select(Clean))}\t{Clean(cases)}";
    }

    static string Clean(string s) => (s ?? "").Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');

    public static readonly List<Row> Rows = new List<Row>();
    public static void Reset() => Rows.Clear();

    public static void Add(Row r)
    {
        Rows.Add(r);
        Write();
    }

    public static void Write()
    {
        try
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            var lines = new List<string> { Header };
            foreach (Row r in Rows) lines.Add(r.ToTsv());
            System.IO.File.WriteAllLines(Path, lines);
        }
        catch (System.Exception e) { Debug.LogWarning($"[TEST] No se pudo escribir {Path}: {e.Message}"); }
    }

    /// <summary>Lee un archivo de resultados (para rehacer la lista rápida o volver a correr lo que falló).</summary>
    public static List<Row> Read(string path)
    {
        var l = new List<Row>();
        if (!System.IO.File.Exists(path)) return l;
        foreach (string line in System.IO.File.ReadAllLines(path).Skip(1)) { Row r = Parse(line); if (r != null) l.Add(r); }
        return l;
    }

    public static Row Parse(string line)
    {
        string[] c = line.Split('\t');
        if (c.Length < 6 || !int.TryParse(c[0], out int g)) return null;
        var r = new Row { group = g, unit = c[1], status = c[5] };
        int.TryParse(c[2], out r.pass); int.TryParse(c[3], out r.fail);
        float.TryParse(c[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out r.seconds);
        if (c.Length > 6 && c[6].Length > 0) r.fails.AddRange(c[6].Split(new[] { " | " }, System.StringSplitOptions.None));
        if (c.Length > 7) r.cases = c[7].Trim();
        return r;
    }
}

/// <summary>
/// LISTAS DE PRUEBAS (2026-11-11): «rapida» son las unidades baratas (menos de 60 s cada una en la última batería completa), como datos
/// en C# (<see cref="TestPresetsData"/>: el repositorio solo guarda .cs). Se rehace con «Tools/Cold Sanctuary/Pruebas/Rehacer la lista
/// rápida» (TestImpactTool.RefreshRapida) a partir de Logs/test-results.tsv. <see cref="Parse"/> lee el mismo formato en texto.
/// </summary>
public static class TestPresets
{
    public const string Rapida = "rapida";

    /// <summary>Las clases de unidad de una lista (vacía si no existe).</summary>
    public static HashSet<string> Units(string preset)
    {
        if (preset == Rapida) { var s = new HashSet<string>(); foreach (var e in TestPresetsData.Rapida) s.Add(e.unit); return s; }
        TextAsset t = Resources.Load<TextAsset>("TestPresets/" + preset);
        return t != null ? Parse(t.text) : new HashSet<string>();
    }

    public static HashSet<string> Parse(string text)
    {
        var s = new HashSet<string>();
        foreach (string raw in (text ?? "").Split('\n'))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            s.Add(line.Split('\t')[0].Trim());
        }
        return s;
    }
}

/// <summary>
/// (2026-11-11) TIEMPO DE JUEGO MÁS RÁPIDO, a petición de la unidad: las que solo esperan a que el juego avance (la jornada, la experiencia
/// del jugador, el horario libre, lo nuclear, cohetes, submarinos) pueden subir Time.maximumDeltaTime (hasta 1 s) mientras corren. A 3 FPS,
/// con el tope de 0,333 de siempre, un timeScale de 6 rendía como 1. El TestRunner lo pone antes de la unidad, lo registra y lo deja como estaba.
/// </summary>
public interface IFastGameTime
{
    /// <summary>El Time.maximumDeltaTime que quiere mientras corre (se acota a 1 s).</summary>
    float MaxDeltaTime { get; }
}
