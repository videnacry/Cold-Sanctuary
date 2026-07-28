using System.Collections.Generic;
using UnityEngine;

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

/// <summary>Ciclo de vida de un pensamiento en la mente que lo tiene (docs anima §11).</summary>
public enum ThoughtLifecycle
{
    Persistent,     // siempre disponible
    OnceThenGone,   // existe hasta usarse por primera vez, luego desaparece
    DecaysPerUse    // pierde peso con cada uso (se apaga poco a poco)
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

    /// <summary>
    /// Quién vivió esta frase (para vivencias). null = anónima/genérica. Un compañero del santuario
    /// (Goluis, Irosene…) o un personaje histórico del Microcosmos (Ötzi, Hipatia…). Sirve para dos cosas:
    /// (1) el "efecto Irosene" — si TODAS las vivencias de una fuente caen en un mismo ser, desarrolla su
    /// papel; (2) el modo narración del Microcosmos — construir a un mob histórico con SU biografía real.
    /// </summary>
    public readonly string source;

    /// <summary>Peso base de selección (valor escalable): 1 = normal; &lt;1 pocos por debajo; &gt;1 por encima.</summary>
    public readonly float weight;
    /// <summary>Ciclo de vida en la mente: persistente / una vez / decae por uso.</summary>
    public readonly ThoughtLifecycle lifecycle;
    /// <summary>Si el pensamiento requiere una aptitud mínima para poder pensarse/mostrarse.</summary>
    public readonly bool gated;
    public readonly AptitudeKind gateAptitude;
    public readonly float gateMin;

    public MindPhrase(ElementalTone t, string[] pos, string[] neg,
                      PhraseCategory cat = PhraseCategory.Elemental,
                      bool randomAssignable = true, bool reusable = true, string source = null,
                      float weight = 1f, ThoughtLifecycle lifecycle = ThoughtLifecycle.Persistent,
                      bool gated = false, AptitudeKind gateAptitude = AptitudeKind.Reasoning, float gateMin = 0f)
    {
        tone = t; positive = pos; negative = neg;
        category = cat; this.randomAssignable = randomAssignable; this.reusable = reusable;
        this.source = source;
        this.weight = weight; this.lifecycle = lifecycle;
        this.gated = gated; this.gateAptitude = gateAptitude; this.gateMin = gateMin;
    }
}

/// <summary>
/// Biblioteca COMPARTIDA de frases (flyweight, docs/anima-architecture.md §6, §11). Reúne las pools de
/// todas las categorías: elementales (idle), vivencias (biografías), deseos base… Asanas y hechizos se
/// enlazarán desde sus sistemas. Consulta por tono/categoría/fuente y reparto de vivencias al crear seres.
/// </summary>
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

    /// <summary>Todas las frases de una categoría (Vivencia/Asana/Hechizo/Elemental/Deseo).</summary>
    public static List<MindPhrase> ForCategory(PhraseCategory c)
    {
        var list = new List<MindPhrase>();
        foreach (MindPhrase p in All) if (p.category == c) list.Add(p);
        return list;
    }

    /// <summary>
    /// La biografía (vivencias) de una fuente concreta — un compañero o un mob histórico del Microcosmos.
    /// Con esto el modo narración construye a un personaje con SUS pensamientos documentados.
    /// </summary>
    public static List<MindPhrase> VivenciasOf(string source)
    {
        var list = new List<MindPhrase>();
        foreach (MindPhrase p in All)
            if (p.category == PhraseCategory.Vivencia && p.source == source) list.Add(p);
        return list;
    }

    /// <summary>
    /// Reparte <paramref name="count"/> vivencias al azar al crear un ser (modo espontáneo). Respeta
    /// <c>randomAssignable</c>; y si <paramref name="taken"/> se pasa, excluye las <c>reusable=false</c> ya
    /// entregadas en la partida (para que no nazcan dos con la misma vivencia). Añade lo repartido a
    /// <paramref name="taken"/>. Si por azar caen todas las de una fuente en un ser, "desarrolla su papel".
    /// </summary>
    public static List<MindPhrase> DealVivencias(int count, HashSet<MindPhrase> taken = null)
    {
        var pool = new List<MindPhrase>();
        foreach (MindPhrase p in All)
            if (p.category == PhraseCategory.Vivencia && p.randomAssignable &&
                (p.reusable || taken == null || !taken.Contains(p)))
                pool.Add(p);

        var dealt = new List<MindPhrase>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            MindPhrase pick = pool[idx];
            pool.RemoveAt(idx);                       // no repetir dentro del mismo ser
            dealt.Add(pick);
            if (!pick.reusable && taken != null) taken.Add(pick);
        }
        return dealt;
    }

    static List<MindPhrase> Build()
    {
        var all = new List<MindPhrase>();
        all.AddRange(BuildElemental());               // pensamientos idle por tono
        all.AddRange(PhrasePools.Vivencias());         // biografías (santuario + histórico)
        all.AddRange(PhrasePools.Deseos());            // deseos base
        return all;
    }

    static List<MindPhrase> BuildElemental() => new List<MindPhrase>
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
