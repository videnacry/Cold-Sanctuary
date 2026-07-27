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

    [Header("Ritmo")]
    [Min(0.5f)] public float thinkInterval = 4f;

    float _nextThink;

    void Awake()
    {
        IAptitudes src = GetComponent<IAptitudes>();
        if (src != null) aptitudes = Aptitudes.From(src);
    }

    void Update()
    {
        humores.Regen(Time.deltaTime);
        if (Time.time < _nextThink) return;
        _nextThink = Time.time + thinkInterval;
        Think();
    }

    void Think()
    {
        ElementalTone tone = PickTone();
        var options = PhraseLibrary.ForTone(tone);
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

    /// <summary>Tono elemental por pesos de aptitudes + humores (docs §5) → personalidad.</summary>
    ElementalTone PickTone()
    {
        float tierra = aptitudes.strength + aptitudes.endurance + aptitudes.bodyMass;
        float agua   = aptitudes.composure + aptitudes.adaptability + humores.serotonina;
        float fuego  = aptitudes.creativity + aptitudes.sociability + humores.adrenalina;
        float viento = aptitudes.agility + aptitudes.reasoning + aptitudes.perception;

        float total = tierra + agua + fuego + viento;
        if (total <= 0f) return ElementalTone.Tierra;

        float r = Random.value * total;
        if ((r -= tierra) < 0f) return ElementalTone.Tierra;
        if ((r -= agua)   < 0f) return ElementalTone.Agua;
        if ((r -= fuego)  < 0f) return ElementalTone.Fuego;
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
}
