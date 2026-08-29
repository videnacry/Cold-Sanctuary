using System.Collections.Generic;

/// <summary>Perfil de un arquetipo (cuerpo o mente): aptitudes base + altura (cuerpo) + tono (mente) +
/// **pensamientos base de la especie** (mente): las frases innatas con que arranca (un oso "piensa como oso").</summary>
public class ArchetypeProfile
{
    public Aptitudes aptitudes;
    public float height = 1f;                 // altura relativa (1 = humano)
    public ElementalTone tone = ElementalTone.Tierra;
    public List<MindPhrase> basePhrases;      // pensamientos base de la especie (solo mentes); null = ninguno
    public float landAffinity = 1f, waterAffinity = 0.4f, airAffinity = 0f;   // afinidad por medio (cuerpo)
}

/// <summary>
/// Catálogo de ARQUETIPOS en CÓDIGO (docs/soul-composition-blend.md). El repo solo versiona `.cs`, así que los
/// perfiles viven aquí (no en ScriptableObjects). **Cuerpos** aportan lo físico (agility/perception/strength/
/// bodyMass/endurance) + altura; **mentes** lo mental (composure/reasoning/memory/creativity/sociability/
/// discipline) + tono; **bonusPacks** son vectores aditivos de stats (potencia por nivel; NO tocan tono/thoughts).
/// Valores tentativos (a calibrar con balance).
/// </summary>
public static class Archetypes
{
    static Dictionary<string, ArchetypeProfile> _bodies;
    static Dictionary<string, ArchetypeProfile> _minds;
    static Dictionary<string, Aptitudes> _packs;

    public static ArchetypeProfile BodyOf(string name)
    {
        Init();
        return name != null && _bodies.TryGetValue(name, out ArchetypeProfile p) ? p : _bodies["Human"];
    }

    public static ArchetypeProfile MindOf(string name)
    {
        Init();
        return name != null && _minds.TryGetValue(name, out ArchetypeProfile p) ? p : _minds["Human"];
    }

    public static bool TryPack(string name, out Aptitudes a)
    {
        Init();
        if (name != null && _packs.TryGetValue(name, out a)) return true;
        a = new Aptitudes();
        return false;
    }

    static Dictionary<string, Dictionary<string, float>> _relations;

    /// <summary>Relación kármica base de una especie hacia otra (0 neutro, + agrado, − desagrado). docs soul-relations §2.</summary>
    public static float RelationValue(string mySpecies, string otherSpecies)
    {
        Init();
        if (string.IsNullOrEmpty(mySpecies) || string.IsNullOrEmpty(otherSpecies)) return 0f;
        return _relations.TryGetValue(mySpecies, out Dictionary<string, float> map) && map.TryGetValue(otherSpecies, out float v) ? v : 0f;
    }

    /// <summary>Disposición NETA de una especie (suma de sus relaciones): + = amistosa en general, − = recelosa.
    /// Base del `openness` al conocer una especie nueva.</summary>
    public static float NetDisposition(string species)
    {
        Init();
        if (string.IsNullOrEmpty(species) || !_relations.TryGetValue(species, out Dictionary<string, float> map)) return 0f;
        float s = 0f;
        foreach (float v in map.Values) s += v;
        return s;
    }

    static void Init()
    {
        if (_bodies != null) return;
        _relations = new Dictionary<string, Dictionary<string, float>>
        {   // base evolutiva/kármica (por generaciones). Simétrica donde tiene sentido; el resto = 0 (neutro).
            { "Seal",     new Dictionary<string, float> { { "Bear", -40f }, { "Whale", 20f } } },   // depredada por osos
            { "Bunny",    new Dictionary<string, float> { { "Wolf", -50f }, { "Fox", -40f } } },
            { "Deer",     new Dictionary<string, float> { { "Wolf", -45f }, { "Bear", -30f } } },
            { "Human",    new Dictionary<string, float> { { "Malamute", 45f } } },                   // perro↔humano
            { "Malamute", new Dictionary<string, float> { { "Human", 45f }, { "Wolf", 15f } } },      // perro (pariente del lobo)
            { "Wolf",     new Dictionary<string, float> { { "Wolf", 15f } } },                        // lealtad de manada
            { "Whale",    new Dictionary<string, float> { { "Seal", 20f } } },
            { "Bear",     new Dictionary<string, float>() },                                          // solitario, neutro
            { "Fox",      new Dictionary<string, float>() },
            { "Panterilia", new Dictionary<string, float> { { "Malamute", 45f } } },                  // humana
        };
        _bodies = new Dictionary<string, ArchetypeProfile>
        {                          // altura, agi, per, str, mass, end
            { "Human",   MakeBody(1.00f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f) },
            { "Bear",    MakeBody(1.60f, 0.7f, 0.9f, 2.2f, 2.5f, 1.6f) },
            { "Wolf",    MakeBody(1.00f, 1.6f, 1.4f, 1.3f, 1.0f, 1.5f) },
            { "Bunny",   MakeBody(0.45f, 1.8f, 1.3f, 0.4f, 0.4f, 0.9f) },
            { "Lion",    MakeBody(1.20f, 1.5f, 1.2f, 1.8f, 1.6f, 1.3f) },
            { "Toro",    MakeBody(1.70f, 0.8f, 0.8f, 2.0f, 2.2f, 1.4f) },
            { "Gallina",  MakeBody(0.35f, 1.2f, 1.1f, 0.3f, 0.3f, 0.7f) },
            { "Mono",     MakeBody(0.90f, 1.7f, 1.3f, 0.9f, 0.8f, 1.2f) },
            { "Ant",      MakeBody(0.30f, 1.6f, 1.2f, 0.5f, 0.4f, 1.3f) },   // hormiga: pequeña, ágil, fuerte para su tamaño
            { "Aphid",   MakeBody(0.20f, 0.3f, 0.6f, 0.2f, 0.8f, 0.5f) },   // pulgón: lento, gordo, sin defensa
            { "Ladybug", MakeBody(0.20f, 0.9f, 1.0f, 0.6f, 0.5f, 0.8f) },   // mariquita: compacta, buena percepción, élitros duros
            { "Spider",  MakeBody(0.40f, 1.4f, 1.5f, 1.0f, 0.7f, 0.9f) },   // araña lobo: rápida, 8 ojos = percepción excepcional
            { "Cricket", MakeBody(0.35f, 1.3f, 1.1f, 0.7f, 0.6f, 1.0f) },   // grillo: saltador ágil, buenos sentidos
            { "Panterilia",  MakeBody(1.00f, 0.95f, 1.7f,  0.7f, 0.8f, 0.9f, 1.4f) },   // companions (fase 5): sus Base* reales
            { "Goluis",      MakeBody(1.05f, 0.9f,  1.1f,  1.5f, 1.3f, 1.4f, 0.6f) },
            { "Gohageneis",  MakeBody(1.00f, 1.2f,  1.05f, 1.1f, 1.1f, 1.3f, 1.7f) },
            { "Irosene",     MakeBody(1.00f, 1.2f,  1.3f,  1.2f, 1.3f, 1.3f, 1.6f) },
            { "Fox",      MakeBody(1.00f, 1.4f, 1.5f, 1.1f, 0.9f, 1.3f) },
            { "Deer",     MakeBody(1.50f, 1.4f, 1.5f, 1.0f, 1.4f, 1.5f) },
            { "Seal",     MakeBody(1.20f, 1.1f, 1.2f, 1.3f, 1.6f, 1.4f) },
            { "Whale",    MakeBody(3.00f, 0.6f, 1.0f, 2.8f, 3.0f, 2.0f) },
            { "Malamute", MakeBody(1.10f, 1.3f, 1.3f, 1.4f, 1.2f, 1.6f) },
        };
        _minds = new Dictionary<string, ArchetypeProfile>
        {                          // tono, com, rea, mem, cre, soc, dis
            { "Human", MakeMind(ElementalTone.Viento, 1.2f, 1.6f, 1.4f, 1.5f, 1.4f, 1.3f) },
            { "Bear",  MakeMind(ElementalTone.Tierra, 1.2f, 0.7f, 0.8f, 0.5f, 0.6f, 0.7f) },
            { "Lion",  MakeMind(ElementalTone.Fuego,  1.0f, 0.9f, 0.9f, 0.8f, 1.1f, 0.9f) },
            { "Rock",  MakeMind(ElementalTone.Tierra, 2.0f, 0.6f, 1.2f, 0.3f, 0.4f, 1.5f) },
            { "Fire",  MakeMind(ElementalTone.Fuego,  0.6f, 1.1f, 0.9f, 1.7f, 1.5f, 0.6f) },
            { "Agua",     MakeMind(ElementalTone.Agua,   1.4f, 1.2f, 1.3f, 1.0f, 1.2f, 1.0f) },
            { "Mono",     MakeMind(ElementalTone.Viento, 0.7f, 1.2f, 1.0f, 1.6f, 1.5f, 0.6f) },
            { "Panterilia",  MakeMind(ElementalTone.Viento, 0.7f, 1.6f, 1.4f, 1.4f, 1.1f, 1.5f) },   // analítica/imaginativa (fase 5)
            { "Goluis",      MakeMind(ElementalTone.Tierra, 1.5f, 0.7f, 0.8f, 0.7f, 0.7f, 1.3f) },   // práctico, reservado, disciplinado
            { "Gohageneis",  MakeMind(ElementalTone.Fuego,  1.2f, 1.0f, 1.0f, 1.2f, 1.7f, 0.6f) },   // social, versátil, poco disciplinado
            { "Irosene",     MakeMind(ElementalTone.Fuego,  1.4f, 1.1f, 1.3f, 1.5f, 1.7f, 1.1f) },   // hiperexpresiva, creativa, motivadora
            { "Wolf",     MakeMind(ElementalTone.Viento, 1.0f, 1.0f, 1.0f, 0.7f, 1.4f, 1.2f) },   // manada: social, disciplinado
            { "Fox",      MakeMind(ElementalTone.Fuego,  0.8f, 1.3f, 1.1f, 1.4f, 1.0f, 0.7f) },   // astuto, creativo
            { "Bunny",    MakeMind(ElementalTone.Agua,   0.6f, 0.8f, 1.0f, 0.6f, 1.0f, 0.6f) },   // tímido
            { "Deer",     MakeMind(ElementalTone.Agua,   1.0f, 0.8f, 1.0f, 0.6f, 1.1f, 0.9f) },
            { "Seal",     MakeMind(ElementalTone.Agua,   1.1f, 0.9f, 1.0f, 0.9f, 1.2f, 0.8f) },   // juguetón
            { "Whale",    MakeMind(ElementalTone.Agua,   1.8f, 1.4f, 1.6f, 1.2f, 1.3f, 1.3f) },   // sabio, calmado
            { "Malamute", MakeMind(ElementalTone.Tierra, 1.1f, 1.0f, 1.1f, 0.8f, 1.5f, 1.4f) },   // leal, disciplinado
            { "Ant",     MakeMind(ElementalTone.Tierra, 1.0f, 0.6f, 1.0f, 0.5f, 1.6f, 1.5f) },   // colonia: disciplina máx, muy social, memoria de rastros
            { "Aphid",   MakeMind(ElementalTone.Agua,   1.2f, 0.4f, 0.7f, 0.4f, 0.8f, 0.5f) },   // pasivo, escasa inteligencia individual
            { "Ladybug", MakeMind(ElementalTone.Fuego,  0.8f, 0.8f, 0.7f, 0.6f, 0.5f, 0.7f) },   // semi-solitaria, cazadora oportunista
            { "Spider",  MakeMind(ElementalTone.Tierra, 1.8f, 1.0f, 0.9f, 0.7f, 0.3f, 1.2f) },   // depredadora paciente; composure = calma de emboscada
            { "Cricket", MakeMind(ElementalTone.Viento, 0.7f, 0.9f, 0.8f, 1.3f, 0.9f, 0.6f) },   // curioso, creativo, canta
        };
        _packs = new Dictionary<string, Aptitudes>
        {   // aditivo (todas las aptitudes). Valores por nivel (placeholder; salen del balance del boss del santuario).
            { "bonusPack1", Flat(0.5f) },
            { "bonusPack2", Flat(1.2f) },
            { "bonusPack3", Flat(2.5f) },
            { "bonusPack4", Flat(4.5f) },
        };

        // Afinidad por medio de los cuerpos ACUÁTICOS (el resto usa el default terrestre 1/0.4/0).
        _bodies["Whale"].waterAffinity = 1.0f; _bodies["Whale"].landAffinity = 0.1f;   // varada e indefensa en tierra
        _bodies["Seal"].waterAffinity  = 1.0f; _bodies["Seal"].landAffinity  = 0.6f;   // anfibia

        // Adjunta los pensamientos BASE de especie a su arquetipo de mente (por `source` = nombre de especie).
        foreach (MindPhrase p in PhrasePools.Especie())
            if (p.source != null && _minds.TryGetValue(p.source, out ArchetypeProfile prof))
            {
                if (prof.basePhrases == null) prof.basePhrases = new List<MindPhrase>();
                prof.basePhrases.Add(p);
            }
    }

    static readonly List<MindPhrase> _noThoughts = new List<MindPhrase>();

    /// <summary>Pensamientos BASE (innatos) de una especie — los siembra su mente al componerse. docs soul-relations §2b.</summary>
    public static List<MindPhrase> BaseThoughtsOf(string species)
    {
        Init();
        return species != null && _minds.TryGetValue(species, out ArchetypeProfile p) && p.basePhrases != null
            ? p.basePhrases : _noThoughts;
    }

    static ArchetypeProfile MakeBody(float height, float agi, float per, float str, float mass, float end, float adapt = 1f)
    {
        Aptitudes a = Aptitudes.Default;
        a.agility = agi; a.perception = per; a.strength = str; a.bodyMass = mass; a.endurance = end; a.adaptability = adapt;
        return new ArchetypeProfile { aptitudes = a, height = height };
    }

    static ArchetypeProfile MakeMind(ElementalTone tone, float com, float rea, float mem, float cre, float soc, float dis)
    {
        Aptitudes a = Aptitudes.Default;
        a.composure = com; a.reasoning = rea; a.memory = mem; a.creativity = cre; a.sociability = soc; a.discipline = dis;
        return new ArchetypeProfile { aptitudes = a, tone = tone };
    }

    static Aptitudes Flat(float v) { Aptitudes a = new Aptitudes(); a.AddAll(v); return a; }
}
