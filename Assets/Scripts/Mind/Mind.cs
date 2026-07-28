using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// El pilar MENTE de un ánima (docs/anima-architecture.md §3, §6, §10) — MVP.
///
/// Cada `thinkInterval` tiene un "momento de pensamiento": elige una FRASE según su TONO elemental
/// (derivado de sus aptitudes + humores), la expresa hasta donde su PODER MENTAL alcanza
/// (nace/crece/reproduce… o el silencio), y eso consume/produce humores. Se puede pegar a CUALQUIER ser:
/// si el objeto tiene un `IAptitudes` (LivingEntity/PlayerStats/…), lee de ahí; si no, usa el campo.
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

    float _nextThink;

    void Awake()
    {
        IAptitudes src = GetComponent<IAptitudes>();
        if (src != null) aptitudes = Aptitudes.From(src);
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
        // Prefiere expresar SUS propias vivencias de ese tono; si no tiene, tira de la biblioteca compartida.
        List<MindPhrase> options = ThoughtsForTone(tone);
        if (options.Count == 0) options = PhraseLibrary.ForTone(tone);
        if (options.Count == 0) return;
        MindPhrase phrase = options[Random.Range(0, options.Count)];

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

    /// <summary>Las vivencias/pensamientos propios de este ser que son de un tono dado.</summary>
    List<MindPhrase> ThoughtsForTone(ElementalTone tone)
    {
        List<MindPhrase> list = new List<MindPhrase>();
        foreach (MindPhrase t in thoughts)
            if (t != null && t.tone == tone) list.Add(t);
        return list;
    }
}
