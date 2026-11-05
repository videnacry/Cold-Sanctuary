using System.Collections.Generic;

/// <summary>El nivel de verdad de una frase (regla M39 del manual).</summary>
public enum Veracity
{
    SinMarcar,
    /// <summary>Palabras reales, con su documento (una ley, un discurso, un libro).</summary>
    Documentada,
    /// <summary>Se dijo según testigos o crónicas: no hay documento exacto.</summary>
    Atestiguada,
    /// <summary>Palabras de juego de una persona real, sobre hechos de su ficha.</summary>
    Recreada,
    /// <summary>La dice un personaje inventado o una voz de juego.</summary>
    Inventada,
}

/// <summary>
/// LA VERACIDAD DE LAS FRASES (Berón 2026-11-05: «las frases se pueden conservar pero como regla general se deben tener la property que ofrezca fuentes para saber si tiene
/// nivel de veracidad o es inventado»; regla M39, docs/veracidad-frases.md). Cada paso de una <see cref="StoryMission"/> lleva su <see cref="Veracity"/> y su fuente.
/// <see cref="Fill"/> las pone: si la frase está en <see cref="Quoted"/>, lo que diga esa tabla; si la dice una persona real (<see cref="Real"/>), Recreada, con las fuentes de
/// la ficha; si no, Inventada. Se aplica al construir y, en las escenas ya construidas, al arrancar (no hace falta reconstruirlas).
/// El jugador lo ve en la nota de cada frase y, al final, en la ficha, con la tecla V (<see cref="HistoryCard"/>).
/// </summary>
public static class LineTruth
{
    /// <summary>Las frases con base propia, por su comienzo: lo que no esté aquí no es una cita.</summary>
    public static readonly (string start, Veracity v, string source)[] Quoted =
    {
        ("Prometo por mi conciencia y honor", Veracity.Documentada, "Real Decreto 707/1979, fórmula de promesa del cargo"),
        ("Votos emitidos, 350", Veracity.Documentada, "Diario de Sesiones del Congreso, 1-6-2018 (resumen del recuento)"),
        ("Hace ochenta y siete años", Veracity.Documentada, "Lincoln, Discurso de Gettysburg (copia Bliss), abreviado y traducido"),
        ("«Pienso, luego existo»", Veracity.Documentada, "Descartes, Discurso del método (1637), parte IV"),
        ("Moral y luces son los polos", Veracity.Documentada, "Bolívar, Discurso de Angostura (15-2-1819)"),
        ("Yogaś citta-vṛtti-nirodhaḥ", Veracity.Documentada, "Patanjali, Yoga Sūtra I.2"),
        ("Sthira-sukham", Veracity.Documentada, "Patanjali, Yoga Sūtra II.46"),
        ("Dhāraṇā es atar", Veracity.Documentada, "Patanjali, Yoga Sūtra III.1"),
        ("Cuando la atención fluye", Veracity.Documentada, "Patanjali, Yoga Sūtra III.2–3"),
        ("¿Y si un demonio", Veracity.Documentada, "Nietzsche, La gaya ciencia, § 341 (resumido)"),
        ("Una de nuestras alegrías", Veracity.Documentada, "Marie Curie, Pierre Curie (1923)"),
        ("Vacía tu mente", Veracity.Documentada, "Bruce Lee, The Pierre Berton Show, 9-12-1971"),
        ("Henriette tiene dieciocho", Veracity.Atestiguada, "Thayer, Life of Beethoven (Unger lo llamó tirano de las voces)"),
        ("Llegué de Rusia sin saber", Veracity.Atestiguada, "Buffett, carta de 1983; obituario de Rose Blumkin, The New York Times (1998)"),
        ("Mucha gente pensaba", Veracity.Recreada, "Space.com (29-9-2008): lo que dijo al equipo, resumido"),
    };

    /// <summary>Las personas reales que hablan en las historias, con su nombre de «speaker». Lo demás es de juego.</summary>
    public static readonly HashSet<string> Real = new HashSet<string>
    {
        // Antigüedad y tradición
        "Siddhārtha", "Channa", "Patanjali", "Pitágoras", "Teano", "Milón", "Heródoto", "Eratóstenes", "Cleopatra", "Iras", "Sosígenes", "al-Juarismi",
        "K'inich Janaab Pakal", "K'inich Kan Bahlam", "Ix Tz'akbu Ajaw", "Ix Sak K'uk'", "El Pacificador", "Ayenwatha", "Jigonsaseh", "Tadodaho",
        // De 1500 a 1900
        "Galvarino", "Caupolicán", "Atahualpa", "Felipillo", "Hernando de Soto", "Descartes", "Jan Maire", "Frans van Schooten", "Helena Jans", "Newton", "Hannah",
        "Jenner", "Sarah", "James", "Beethoven", "Schindler", "Caroline Unger", "Henriette Sontag", "Umlauf", "Bolívar", "Florence", "Alexis Soyer", "Sutherland",
        "Abraham Lincoln", "David Wills", "Elizabeth Thorn", "Basil Biggs", "Edward Everett", "Nietzsche", "Durisch", "Tesla", "Czito", "Marie", "Pierre",
        // Siglo XX
        "Wilbur", "Orville", "Charlie Taylor", "Eloy Alfaro", "América Alfaro", "Archer Harman", "Einstein", "Besso", "Mileva", "Solovine", "Patterson", "Lorraine", "Kehoe",
        "Ernesto Guevara", "Alberto Granado", "Katherine Johnson", "Dorothy Vaughan", "Mary Jackson", "Bruce Lee", "Yip Man", "Taky Kimura", "Linda Lee",
        "Ananenko", "Baránov", "Bespálov", "Rose Blumkin", "Louie Blumkin", "Ed Roberts", "Paul Allen",
        "George Washington", "Franklin D. Roosevelt", "José Figueres", "Seretse Khama", "Lee Kuan Yew", "Nelson Mandela",
        // Personas vivas o recientes
        "Sadhguru", "Elon Musk", "Bill Gates", "Warren Buffett", "MacKenzie Scott", "Ruth Simmons", "Pedro Sánchez", "Ana Pastor", "Ibrahim Traoré",
        // La música
        "John Lennon", "Yoko Ono", "Derek Taylor", "Tim Attwood", "André Perry", "Bob Marley", "Peter Tosh", "Joe Higgs", "Claudie Massop", "Bono", "John Hume", "David Trimble",
        "Chuck D", "Flavor Flav", "Hank Shocklee", "Bill Stephney", "Benito", "DJ Luian", "Lysaurie", "Shakira", "Katherine", "Mondina", "Michael", "Jermaine", "Tito", "Joe Jackson",
        "Jennie", "Jisoo", "Rosé", "Lisa", "Susana Baca", "Peter Gabriel", "Guy Carawan",
    };

    public static bool IsReal(string speaker) => !string.IsNullOrEmpty(speaker) && (Real.Contains(speaker) || speaker.StartsWith("Bruce Lee") || speaker.StartsWith("K'inich"));

    public static (Veracity v, string source) Judge(string speaker, string line, string[] sources)
    {
        if (!string.IsNullOrEmpty(line)) foreach (var q in Quoted) if (line.StartsWith(q.start)) return (q.v, q.source);
        if (IsReal(speaker)) return (Veracity.Recreada, "Frase de juego sobre hechos de la ficha: " + (sources != null && sources.Length > 0 ? sources[0] : "ver la ficha"));
        return (Veracity.Inventada, "Personaje o voz de juego: no es una cita");
    }

    public static string Label(Veracity v)
    {
        switch (v)
        {
            case Veracity.Documentada: return "documentada";
            case Veracity.Atestiguada: return "atestiguada";
            case Veracity.Recreada: return "recreada";
            case Veracity.Inventada: return "inventada";
            default: return "sin marcar";
        }
    }

    /// <summary>Pone la veracidad a toda frase de la misión que no la tenga y escribe la lista en su ficha.</summary>
    public static void Fill(StoryMission m, HistoryCard card)
    {
        if (m == null) return;
        string[] sources = card != null ? card.sources : null;
        var lines = new List<string>();
        void Row(string who, string line, Veracity v, string src) { if (!string.IsNullOrEmpty(line)) lines.Add($"[{Label(v)}] {who}: «{Short(line)}» ({src})"); }
        foreach (StoryMission.StoryStep s in m.steps)
        {
            if (string.IsNullOrEmpty(s.line)) continue;
            if (s.truth == Veracity.SinMarcar) { var j = Judge(s.speaker, s.line, sources); s.truth = j.v; s.truthSource = j.source; }
            Row(s.speaker, s.line, s.truth, s.truthSource);
        }
        if (!string.IsNullOrEmpty(m.finalLine))
        {
            if (m.finalTruth == Veracity.SinMarcar) { var j = Judge(m.finalSpeaker, m.finalLine, sources); m.finalTruth = j.v; m.finalTruthSource = j.source; }
            Row(m.finalSpeaker, m.finalLine, m.finalTruth, m.finalTruthSource);
        }
        if (card != null) card.lineTruth = lines.ToArray();
    }

    static string Short(string s) => s.Length <= 70 ? s : s.Substring(0, 67) + "…";

    /// <summary>Lo que le falta a una misión (para las pruebas): frases sin nivel o sin fuente, y personas reales marcadas como inventadas.</summary>
    public static List<string> Missing(StoryMission m)
    {
        var bad = new List<string>();
        if (m == null) return bad;
        foreach (StoryMission.StoryStep s in m.steps)
        {
            if (string.IsNullOrEmpty(s.line)) continue;
            if (s.truth == Veracity.SinMarcar || string.IsNullOrEmpty(s.truthSource)) bad.Add($"«{s.goal}» sin veracidad");
            else if (s.truth == Veracity.Inventada && IsReal(s.speaker)) bad.Add($"«{s.goal}»: {s.speaker} es real y su frase sale inventada");
        }
        return bad;
    }
}
