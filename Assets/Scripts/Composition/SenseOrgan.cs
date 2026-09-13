using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ÓRGANO SENSORIAL (docs/consciousness-mechanics.md §3) — la clase de body-part que faltaba. Un órgano DECLARA qué
/// <see cref="PerceptChannel"/> genera/procesa y con qué CALIDAD (0–1), y por qué <see cref="Senses">sentido</see>
/// (que fija su ALCANCE). Todo es CONFIGURACIÓN, no el tipo:
///   • ojo normal   = sense "vista",  {Color 1, Shape .9, Distance .9, Direction 1, Movement 1, Presence 1}
///   • ojo ciego    = igual pero Color/Shape = 0
///   • ojo borroso  = Color/Shape ~0.3
///   • lengua que SABOREA LA LUZ = sense "gusto" pero provee `Color` (sinestésico) — raro, pero POSIBLE
///   • oído que "escucha la luz" = sense "oido" que provee `Color`
/// La observación (`ObserveSpell`) AGREGA los canales de todos los órganos del ser (máxima calidad por canal). Como
/// componente, es "vivo" (parte del cuerpo); la vía MÁGICA (grimorio) provee los mismos canales pero escalados por la
/// habilidad del hechicero. `rangeMultiplier` ajusta el alcance del sentido para este órgano concreto.
/// </summary>
public class SenseOrgan : MonoBehaviour
{
    [System.Serializable]
    public struct ChannelQuality
    {
        public PerceptChannel channel;
        [Range(0f, 1f)] public float quality;
    }

    [Tooltip("Nombre del órgano (Ojo/Oído/Nariz/Lengua/Piel/Antena…).")]
    public string organName = "Ojo";
    [Tooltip("Sentido (Senses) — fija el ALCANCE base.")]
    public string sense = "vista";
    [Tooltip("Multiplicador de alcance de ESTE órgano (águila = >1; miope = <1).")]
    [Min(0f)] public float rangeMultiplier = 1f;
    [Tooltip("INTENSIDAD/PROFUNDIDAD global del órgano (0..1+): escala TODOS sus canales (idea del usuario — hay una base " +
             "universal y la intensidad decide cuánto detalle se obtiene). Ojo borroso = acuity baja; ojo agudo = alta.")]
    [Min(0f)] public float acuity = 1f;
    [Tooltip("La LISTA COMPLETA de canales, cada uno con su BASE 0..1 (0 = este órgano no capta ese canal). La calidad " +
             "efectiva = base × acuity. Un ojo ciego = todo 0; un perro dicrómata = Color bajo, sin ColorUV; una abeja = ColorUV>0.")]
    public ChannelQuality[] provides;

    /// <summary>Alcance efectivo del órgano (sentido × multiplicador).</summary>
    public float Reach => Senses.RangeOf(sense) * rangeMultiplier;

    /// <summary>Calidad efectiva [0,1] con la que capta un canal = base del canal × <see cref="acuity"/> (0 = no lo capta).</summary>
    public float QualityOf(PerceptChannel c)
    {
        if (provides == null) return 0f;
        foreach (ChannelQuality q in provides) if (q.channel == c) return Mathf.Clamp01(q.quality * acuity);
        return 0f;
    }

    // ── Fábrica: crea un órgano en un GO con sus canales (para builders/config por código) ──
    public static SenseOrgan Add(GameObject go, string organName, string sense, float rangeMul,
                                 params (PerceptChannel ch, float q)[] channels)
    {
        SenseOrgan o = go.AddComponent<SenseOrgan>();
        o.organName = organName; o.sense = sense; o.rangeMultiplier = rangeMul;
        List<ChannelQuality> list = new List<ChannelQuality>();
        if (channels != null) foreach ((PerceptChannel ch, float q) in channels) list.Add(new ChannelQuality { channel = ch, quality = q });
        o.provides = list.ToArray();
        return o;
    }

    // Presets comunes (los "generales" Presence/Distance/Direction/Movement casi siempre presentes).
    public static SenseOrgan Eye(GameObject go, float acuity = 1f) => Add(go, "Ojo", "vista", 1f,
        (PerceptChannel.Presence, 1f), (PerceptChannel.Distance, 0.9f * acuity), (PerceptChannel.Direction, 1f),
        (PerceptChannel.Movement, 1f), (PerceptChannel.Color, acuity), (PerceptChannel.Shape, 0.9f * acuity), (PerceptChannel.Identity, 0.6f * acuity));
    public static SenseOrgan BlindEye(GameObject go) => Add(go, "Ojo ciego", "vista", 1f,
        (PerceptChannel.Presence, 0f));   // no aporta nada útil
    public static SenseOrgan Nose(GameObject go) => Add(go, "Nariz", "olfato", 1f,
        (PerceptChannel.Presence, 1f), (PerceptChannel.Distance, 0.5f), (PerceptChannel.Direction, 0.6f),
        (PerceptChannel.Odor, 1f), (PerceptChannel.Identity, 0.7f), (PerceptChannel.Emotion, 0.4f));
    public static SenseOrgan Ear(GameObject go) => Add(go, "Oído", "oido", 1f,
        (PerceptChannel.Presence, 1f), (PerceptChannel.Distance, 0.7f), (PerceptChannel.Direction, 0.9f),
        (PerceptChannel.Movement, 0.8f), (PerceptChannel.Sound, 1f));
    public static SenseOrgan Tongue(GameObject go) => Add(go, "Lengua", "gusto", 1f,
        (PerceptChannel.Presence, 1f), (PerceptChannel.Flavor, 1f), (PerceptChannel.Identity, 0.5f));
    public static SenseOrgan Skin(GameObject go) => Add(go, "Piel", "tacto", 1f,
        (PerceptChannel.Presence, 1f), (PerceptChannel.Texture, 1f), (PerceptChannel.Temperature, 0.7f), (PerceptChannel.Shape, 0.5f));
    /// <summary>Una lengua SINESTÉSICA que "saborea la luz" (provee Color por el gusto) — rara pero posible.</summary>
    public static SenseOrgan LightTastingTongue(GameObject go) => Add(go, "Lengua-luz", "gusto", 3f,
        (PerceptChannel.Presence, 1f), (PerceptChannel.Flavor, 1f), (PerceptChannel.Color, 0.6f), (PerceptChannel.Distance, 0.4f));

    /// <summary>Ojo DICRÓMATA (perro): ve, pero con MENOS matices de color y sin UV.</summary>
    public static SenseOrgan DogEye(GameObject go) => Add(go, "Ojo dicromata", "vista", 1f,
        (PerceptChannel.Presence, 1f), (PerceptChannel.Distance, 0.9f), (PerceptChannel.Direction, 1f),
        (PerceptChannel.Movement, 1.2f), (PerceptChannel.Color, 0.4f), (PerceptChannel.Shape, 0.8f));   // buen movimiento, poco color
    /// <summary>Ojo con banda ULTRAVIOLETA (abeja/ave): ve marcas UV invisibles para el humano.</summary>
    public static SenseOrgan UVEye(GameObject go) => Add(go, "Ojo UV", "vista", 1f,
        (PerceptChannel.Presence, 1f), (PerceptChannel.Distance, 0.7f), (PerceptChannel.Direction, 1f),
        (PerceptChannel.Movement, 1f), (PerceptChannel.Color, 0.8f), (PerceptChannel.ColorUV, 1f), (PerceptChannel.Shape, 0.7f));
}
