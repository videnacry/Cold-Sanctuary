using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// El pilar MENTE de un ánima (docs/anima-architecture.md §3, §6, §10) — MVP.
///
/// Cada `thinkInterval` tiene un "momento de pensamiento": elige una FRASE según su TONO elemental
/// (derivado de sus aptitudes + humores), la expresa hasta donde su PODER MENTAL alcanza
/// (nace/crece/reproduce… o el silencio), y eso consume/produce humores. Se puede pegar a CUALQUIER ser:
/// si el objeto tiene un `IAptitudes` (Anima/PlayerStats/…), lee de ahí; si no, usa el campo.
///
/// Barato: frases compartidas (`PhraseLibrary`), solo pesos por ser, decisión por temporizador.
/// </summary>
public class Mind : MonoBehaviour
{
    [Header("Aptitudes (se siembran del IAptitudes del objeto si lo hay). 1.0 = media.")]
    public Aptitudes aptitudes = Aptitudes.Default;

    [Header("Humores (bioquímica)")]
    public Humores humores = new Humores();

    [Header("Identidad y pensamientos (docs anima §11)")]
    [Tooltip("Fuente autoral de este ser (Magnate, Goluis, Ötzi…). Vacío = anónimo/genérico.")]
    public string identity = "";

    [Tooltip("Pensamientos PRIVADOS: nunca van al pool público ni se redistribuyen (Magnate, históricos). " +
             "Se marca/desmarca a conveniencia por personaje.")]
    public bool thoughtsLocked = false;

    [Tooltip("Vivencias/pensamientos que este ser tiene AHORA. Inclinan su tono y son las frases que expresa. " +
             "Los siembra PhraseDistribution según el modo (estricta/libre).")]
    public List<MindPhrase> thoughts = new List<MindPhrase>();

    [Header("Ritmo")]
    [Min(0.5f)] public float thinkInterval = 4f;

    [Tooltip("Piso de confianza en la selección de una frase-CAPACIDAD (D3a): 0.25 = una capacidad aún no dominada " +
             "se elige a 1/4 de su peso (para poder probarse y ganar confianza); dominada, a peso pleno.")]
    [Range(0f, 1f)] public float minConfidenceFactor = 0.25f;

    float _nextThink;
    Anima _anima;                    // para leer Confidence(spell) (D3a) — el bond-hacia-el-hechizo
    CharacterComposition _body;      // para el gate por receptor/anatomía (E2)

    void Awake()
    {
        IAptitudes src = GetComponent<IAptitudes>();
        if (src != null) aptitudes = Aptitudes.From(src);
        _anima = GetComponent<Anima>();
        _body = GetComponent<CharacterComposition>();
    }

    /// <summary>Añade pensamientos base (p.ej. innatos de la especie) que aún no tenga. No duplica.</summary>
    public void SeedThoughts(System.Collections.Generic.IEnumerable<MindPhrase> phrases)
    {
        if (phrases == null) return;
        foreach (MindPhrase p in phrases)
            if (p != null && !thoughts.Contains(p)) thoughts.Add(p);
    }

    // Campos de pensamiento cercanos (refrescados por intervalos; pueden aparecer/desaparecer en runtime).
    ThoughtField[] _fields = System.Array.Empty<ThoughtField>();
    float _nextFieldRefresh;

    void Update()
    {
        humores.Regen(Time.deltaTime);
        RefreshFieldsIfDue();
        ApplyFieldHumors(Time.deltaTime);
        if (Time.time < _nextThink) return;
        _nextThink = Time.time + thinkInterval;
        Think();
    }

    void RefreshFieldsIfDue()
    {
        if (Time.time < _nextFieldRefresh) return;
        _nextFieldRefresh = Time.time + 1f;
        _fields = FindObjectsOfType<ThoughtField>();
    }

    /// <summary>Los campos que cubren a este ser mueven sus humores (guiarlo por el entorno).</summary>
    void ApplyFieldHumors(float dt)
    {
        foreach (ThoughtField f in _fields)
            if (f != null && f.nudgesHumor && f.Covers(transform.position))
                humores.Produce(f.humor, f.humorPerSecond * dt);
    }

    void Think()
    {
        ElementalTone tone = PickTone();
        // Prefiere expresar SUS propias vivencias de ese tono (gateadas por aptitud); si no, la biblioteca.
        List<MindPhrase> owned = ThoughtsForTone(tone);
        List<MindPhrase> options = owned.Count > 0 ? owned : PhraseLibrary.ForTone(tone);
        if (options.Count == 0) return;
        MindPhrase phrase = PickWeighted(options);

        bool positive = humores.Positividad >= 0f;
        string[] parts = positive ? phrase.positive : phrase.negative;

        int depth = Depth();                       // 0..4; 4 = silencio (muerte de la frase)

        if (depth <= 0)                            // no llega ni a formular el pensamiento
        {
            humores.Consume(Humor.Glucosa, 0.01f);
            return;
        }
        if (depth >= 4)                            // el silencio: mente que llegó al final de la frase
        {
            Debug.Log($"[Mente] «{name}» ({tone}, silencio): …");
            return;
        }

        int spoken = Mathf.Min(depth, parts.Length);
        string msg = string.Join(" ", parts, 0, spoken);
        Debug.Log($"[Mente] «{name}» ({tone}{(positive ? "+" : "−")}): \"{msg}\"");

        // Expresarse gasta energía; el tono del ánimo deja un poso químico.
        humores.Consume(Humor.Glucosa, 0.02f * spoken);
        humores.Produce(positive ? Humor.Serotonina : Humor.Cortisol, 0.02f);

        ApplyLifecycle(phrase);   // una-vez / decae-por-uso (solo para pensamientos propios)
    }

    // ── Peso escalable + ciclo de vida de los pensamientos propios (docs anima §11) ──────────────
    System.Collections.Generic.Dictionary<MindPhrase, int> _uses;

    int UsesOf(MindPhrase p) => (_uses != null && _uses.TryGetValue(p, out int n)) ? n : 0;

    /// <summary>Peso efectivo: el base de la frase, reducido por usos si su ciclo es DecaysPerUse.</summary>
    float EffectiveWeight(MindPhrase p)
    {
        float w = Mathf.Max(0.0001f, p.weight);
        if (p.lifecycle == ThoughtLifecycle.DecaysPerUse) w /= (1 + UsesOf(p));
        // D3a: una frase-CAPACIDAD (hechizo/asana con clave) se pondera por la CONFIANZA en ella (el bond-hacia-el-
        // hechizo): lo dominado se elige más; lo nuevo conserva un piso (`minConfidenceFactor`) para poder probarse
        // y aprenderse. Sin clave → sin cambio (las frases actuales quedan igual). Ver capabilities-and-embodiment.md §5.
        if (!string.IsNullOrEmpty(p.capability) && _anima != null)
            w *= Mathf.Lerp(minConfidenceFactor, 1f, _anima.Confidence(p.capability) / 100f);
        return w;
    }

    /// <summary>Pick ponderado por EffectiveWeight (los pensamientos de más peso salen más).</summary>
    MindPhrase PickWeighted(List<MindPhrase> options)
    {
        float total = 0f;
        foreach (MindPhrase p in options) total += EffectiveWeight(p);
        if (total <= 0f) return options[Random.Range(0, options.Count)];
        float r = Random.value * total;
        foreach (MindPhrase p in options)
            if ((r -= EffectiveWeight(p)) < 0f) return p;
        return options[options.Count - 1];
    }

    /// <summary>Tras usar un pensamiento propio: si es OnceThenGone se va; si DecaysPerUse pierde peso.</summary>
    void ApplyLifecycle(MindPhrase p)
    {
        if (!thoughts.Contains(p)) return;   // solo los propios tienen ciclo de vida (la biblioteca es fija)
        switch (p.lifecycle)
        {
            case ThoughtLifecycle.OnceThenGone:
                thoughts.Remove(p);
                break;
            case ThoughtLifecycle.DecaysPerUse:
                if (_uses == null) _uses = new System.Collections.Generic.Dictionary<MindPhrase, int>();
                _uses[p] = UsesOf(p) + 1;
                break;
        }
    }

    /// <summary>Tono elemental por pesos de aptitudes + humores + campos de pensamiento (docs §5).</summary>
    ElementalTone PickTone()
    {
        // Pesos base por aptitudes + humores (personalidad).  Índice = (int)ElementalTone.
        float[] w = new float[4];
        w[(int)ElementalTone.Tierra] = aptitudes.strength + aptitudes.endurance + aptitudes.bodyMass;
        w[(int)ElementalTone.Agua]   = aptitudes.composure + aptitudes.adaptability + humores.serotonina;
        w[(int)ElementalTone.Fuego]  = aptitudes.creativity + aptitudes.sociability + humores.adrenalina;
        w[(int)ElementalTone.Viento] = aptitudes.agility + aptitudes.reasoning + aptitudes.perception;

        // Sus vivencias/pensamientos propios inclinan su tono (lo que ha vivido, lo tiñe).
        foreach (MindPhrase t in thoughts)
            if (t != null) w[(int)t.tone] += 1f;

        // Empuje de los campos de pensamiento que cubren a este ser (guía por el entorno, docs §5).
        foreach (ThoughtField f in _fields)
            if (f != null && f.Covers(transform.position))
                w[(int)f.tone] += f.pull;

        float total = w[0] + w[1] + w[2] + w[3];
        if (total <= 0f) return ElementalTone.Tierra;

        float r = Random.value * total;
        for (int i = 0; i < 4; i++)
            if ((r -= w[i]) < 0f) return (ElementalTone)i;
        return ElementalTone.Viento;
    }

    /// <summary>
    /// Poder mental → hasta qué parte de la frase llega. 0 = ni nace; 1–3 = nace/crece/reproduce;
    /// 4 = silencio. Sale de razón/memoria/disciplina, modulado por la energía (humores). Ajustable.
    /// </summary>
    int Depth()
    {
        float power = (aptitudes.reasoning + aptitudes.memory + aptitudes.discipline) / 3f;
        power *= Mathf.Lerp(0.5f, 1f, humores.Energia);
        return Mathf.Clamp(Mathf.FloorToInt(power * 3f), 0, 4);
    }

    /// <summary>Las vivencias/pensamientos propios de este ser, del tono dado y que pasan su gate de aptitud.</summary>
    List<MindPhrase> ThoughtsForTone(ElementalTone tone)
    {
        List<MindPhrase> list = new List<MindPhrase>();
        foreach (MindPhrase t in thoughts)
            if (t != null && t.tone == tone && PassesGate(t)) list.Add(t);
        return list;
    }

    /// <summary>¿Este ser puede pensar/lanzar esta frase? Gate por APTITUD (mínimo) y por RECEPTOR/anatomía (E2):
    /// si la frase fija `gateCapability`, requiere que alguna `CompositionPart` lo conceda (ojo→Ver, colmillo→Morder).
    /// Sin gates → siempre sí. Ver docs/capabilities-and-embodiment.md §2.</summary>
    bool PassesGate(MindPhrase p)
    {
        if (p.gated && aptitudes.Get(p.gateAptitude) < p.gateMin) return false;
        if (!string.IsNullOrEmpty(p.gateCapability) && (_body == null || !_body.Grants(p.gateCapability))) return false;
        return true;
    }
}
