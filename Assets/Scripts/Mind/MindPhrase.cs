using System.Collections.Generic;

/// <summary>
/// Clasificación de una frase (docs/anima-architecture.md §6/§10). TODO es una frase: una vivencia, una
/// asana, un hechizo, un pensamiento elemental idle, un deseo base.
/// </summary>
public enum PhraseCategory
{
    Vivencia,    // experiencia de vida — modela las aptitudes base al crear un personaje
    Asana,       // postura de yoga (habilidad corporal)
    Hechizo,     // conjuro (habilidad mágica)
    Elemental,   // pensamiento idle con tono elemental
    Deseo        // deseo base: trabajar / cuidar / acompañar / comer / dormir…
}

/// <summary>
/// Una "frase" del pensamiento (docs/anima-architecture.md §6, §10.2): pertenece a una CATEGORÍA y a un
/// tono elemental, tiene forma positiva y negativa, y partes con ciclo de vida — [nace, crece, se
/// reproduce]. La 4ª etapa (muere/silencio) NO es texto: es el silencio al que solo llega una mente muy
/// poderosa.
///
/// Biblioteca COMPARTIDA (flyweight): todas las ánimas referencian las mismas frases; el "alma" concreta
/// de cada ser sale de sus PESOS (tono/aptitudes/humores), no de contenido único por ser.
/// </summary>
public class MindPhrase
{
    public readonly ElementalTone tone;
    public readonly PhraseCategory category;
    public readonly string[] positive;   // [nace, crece, reproduce]
    public readonly string[] negative;

    /// <summary>Si puede asignarse al azar al crear un personaje (para vivencias).</summary>
    public readonly bool randomAssignable;
    /// <summary>Si puede asignarse a más de un ser (evita que nazcan con las mismas vivencias si es false).</summary>
    public readonly bool reusable;

    public MindPhrase(ElementalTone t, string[] pos, string[] neg,
                      PhraseCategory cat = PhraseCategory.Elemental,
                      bool randomAssignable = true, bool reusable = true)
    {
        tone = t; positive = pos; negative = neg;
        category = cat; this.randomAssignable = randomAssignable; this.reusable = reusable;
    }
}

public static class PhraseLibrary
{
    static List<MindPhrase> _all;

    public static List<MindPhrase> All
    {
        get { if (_all == null) _all = Build(); return _all; }
    }

    public static List<MindPhrase> ForTone(ElementalTone t)
    {
        var list = new List<MindPhrase>();
        foreach (MindPhrase p in All) if (p.tone == t) list.Add(p);
        return list;
    }

    static List<MindPhrase> Build() => new List<MindPhrase>
    {
        new MindPhrase(ElementalTone.Tierra,
            new[] { "Siento el peso de la tierra.", "Todo descansa y perdura.", "Soy raíz; nada me mueve." },
            new[] { "La tierra pesa demasiado.", "Estoy atrapado, inmóvil.", "Me hundo y no vuelvo." }),
        new MindPhrase(ElementalTone.Agua,
            new[] { "Fluyo sin resistir.", "Me adapto a cada forma.", "Soy calma que todo abraza." },
            new[] { "Me arrastra la corriente.", "No hallo forma ni orilla.", "Me disuelvo en la nada." }),
        new MindPhrase(ElementalTone.Fuego,
            new[] { "Algo arde y despierta.", "Quiero crear, moverme, jugar.", "Soy chispa que enciende a otros." },
            new[] { "La rabia me consume.", "Todo me irrita.", "Me quemo y quemo alrededor." }),
        new MindPhrase(ElementalTone.Viento,
            new[] { "Un pensamiento pasa ligero.", "Voy de un lugar a otro sin atarme.", "Soy voz que lleva lo que oye." },
            new[] { "No detengo mi mente.", "Me disperso en mil ideas.", "Nada permanece en mí." }),
    };
}
