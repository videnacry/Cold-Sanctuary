using UnityEngine;

/// <summary>
/// ERAS (Berón, 2026-10-23: «necesitaríamos algo similar al tiempo para crear comportamientos/capacidades en base a las
/// fechas; cada microcosmos tendría su propia fecha»). Igual que el reloj recrea el día y la jornada, esto recrea la
/// ÉPOCA: cada microcosmos lleva su fecha (<see cref="MicroEra"/>) y cada capacidad cultural tiene un año de inicio con su
/// fuente. Las conductas preguntan <see cref="Eras.Allows"/> antes de hacer algo que no existía en esa época (los
/// mirmidones no ordeñan pulgones antes de la ganadería lechera). El santuario es el presente.
/// Años: negativos = antes de Cristo (−7000 = 7000 a. C.). Ver docs/eras.md.
/// </summary>
public static class Eras
{
    public const int Present = 2026;

    public enum Capability
    {
        Fire,          // fuego dominado y habitual
        Pottery,       // cerámica
        Agriculture,   // cultivo (siembra y cosecha de cereales)
        Herding,       // pastoreo de ganado
        Dairying,      // ordeño (en el microcosmos: ordeñar pulgones)
        Writing,       // escritura
        Bronze,        // metalurgia del bronce
        Iron,          // del hierro
        Vaccination,   // vacunas
        Electricity,   // red eléctrica
        Antibiotics,   // antibióticos
        Nuclear,       // reactor nuclear
    }

    /// <summary>(capacidad, año de inicio, fuente). Fechas de los primeros indicios sólidos; [?] = discutido.</summary>
    public static readonly (Capability cap, int year, string source)[] Table =
    {
        (Capability.Fire,        -400000, "uso habitual del fuego: Qesem (Israel), Shahack-Gross et al. 2014, J Archaeol Sci 44:12 [indicios de ~1 Ma en Wonderwerk]"),
        (Capability.Pottery,     -18000,  "cerámica de la cueva de Xianrendong (China), Wu et al. 2012, Science 336:1696"),
        (Capability.Agriculture, -9500,   "cereales domesticados en el Creciente Fértil, Zeder 2011, Curr Anthropol 52:S221"),
        (Capability.Herding,     -8500,   "cabras y ovejas domésticas en el Creciente Fértil, Zeder 2008, PNAS 105:11597"),
        (Capability.Dairying,    -7000,   "grasas de leche en cerámica de Anatolia, VII milenio a. C., Evershed et al. 2008, Nature 455:528"),
        (Capability.Writing,     -3200,   "cuneiforme de Uruk (Mesopotamia)"),
        (Capability.Bronze,      -3300,   "Edad del Bronce en el Próximo Oriente"),
        (Capability.Iron,        -1200,   "Edad del Hierro en el Mediterráneo oriental"),
        (Capability.Vaccination, 1796,    "vacuna de la viruela, Jenner 1798 (experimento de 1796)"),
        (Capability.Electricity, 1882,    "central de Pearl Street (Nueva York), Edison 1882"),
        (Capability.Antibiotics, 1942,    "penicilina en uso clínico: primeros pacientes 1941–42"),
        (Capability.Nuclear,     1942,    "Chicago Pile-1, 2 de diciembre de 1942"),
    };

    /// <summary>El año en que se está ahora: el del microcosmos cargado o el presente (santuario).</summary>
    public static int Now => MicroEra.Current != null ? MicroEra.Current.year : Present;

    public static int StartOf(Capability c) { foreach (var t in Table) if (t.cap == c) return t.year; return int.MinValue; }

    /// <summary>¿Existe ya esta capacidad en el año <paramref name="year"/> (por defecto, el de ahora)?</summary>
    public static bool Allows(Capability c, int? year = null) => (year ?? Now) >= StartOf(c);

    /// <summary>«7000 a. C.» / «1942».</summary>
    public static string Format(int year) => year < 0 ? $"{-year:#,0} a. C." : year.ToString();

    /// <summary>La fecha y el lugar de cada microcosmos (por su nombre). null = sin fecha propia (el presente).</summary>
    public static (int year, string place)? OfMicrocosmos(string display)
    {
        switch (display)
        {
            case "Sakshi (el río)":       return (-1100000, "el río del origen de Sakshi, antes de Ambrosio");
            case "Ambrosio (nivel 1)":
            case "Hospital":              return (-1000000, "taiga, antes del fuego (Ambrosio)");
            case "Mesopotamia":           return (-3500, "Uruk, en el amanecer de las ciudades");
            case "Cocina":                return (-400000, "la era del fuego (las reencarnaciones)");
            case "Huerto":                return (-9500, "los primeros cultivos (La Sembradora)");
            case "Sala de Yoga": case "Estudio Textil": case "Mecánica": return (-40000, "una tribu del Paleolítico");
            case "Kapilavastu":           return (-530, "Kapilavastu, Terai del Nepal");
            case "Patanjali":             return (-150, "llanura del Ganges");
            case "Egina":                 return (-1250, "Egina, la Grecia heroica del mito");
            case "Alejandría":            return (-240, "Alejandría y Siene, Egipto");
            case "Escútari":              return (1854, "Escútari, Estambul");
            case "Dayton":                return (1903, "Dayton y Kill Devil Hills, EE. UU.");
            case "Tregua 1914":           return (1914, "Flandes");
            case "Irena Sendler":         return (1942, "Varsovia");
            case "Le Chambon-sur-Lignon": return (1942, "Le Chambon-sur-Lignon, Francia");
            case "Planta Atómica":        return (1905, "Woolsthorpe, París y Berna (Newton, Curie, Einstein)");
            default: return null;
        }
    }
}

/// <summary>La FECHA de un microcosmos: va en su raíz; mientras su escena está cargada, <see cref="Eras.Now"/> es su año.</summary>
public class MicroEra : MonoBehaviour
{
    public static MicroEra Current { get; private set; }
    public int year = Eras.Present;
    public string place = "";
    [Tooltip("La fecha que se cuenta (día, estación o época), de la plantilla de microcosmos.")]
    public string date = "";
    [Tooltip("La hora del día (0–24): de ella sale la posición del sol.")]
    public float hour = 10f;
    [Tooltip("La plataforma del santuario que este microcosmos agranda (su área).")]
    public string platform = "";

    // Al entrar: dónde y cuándo estás (fecha, hora y la plataforma del santuario de la que es este microcosmos).
    void Start()
    {
        if (string.IsNullOrEmpty(date)) return;
        int h = Mathf.FloorToInt(hour), m = Mathf.RoundToInt((hour - h) * 60f) % 60;
        LessonPrompt.Show($"{place} · {date} · {h:00}:{m:00} · sobre la plataforma: {platform}", null, 8f);
    }

    void OnEnable() { Current = this; }
    void OnDisable() { if (Current == this) Current = null; }
}
