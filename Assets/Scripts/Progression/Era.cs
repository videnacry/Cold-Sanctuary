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

    /// <summary>Desde cuándo hay ESTRUCTURAS HUMANAS (aldeas de casas redondas del Natufiense / PPNA, ~9500 a. C.): antes, la tribu vive en
    /// una guarida o refugio natural; después, cada era tiene sus edificios (docs/microcosmos-civilizaciones.md §8).</summary>
    public const int StructuresFrom = -9500;
    public static bool HasStructures(int year) => year >= StructuresFrom;

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
            case "Bruce Lee":             return (1964, "Hong Kong, Seattle, Long Beach y Los Ángeles");
            case "Irena Sendler":         return (1942, "Varsovia");
            case "Le Chambon-sur-Lignon": return (1942, "Le Chambon-sur-Lignon, Francia");
            // Los microcosmos de científicos e históricos (docs/microcosmos-cientificos.md): cada uno, el pueblo donde ocurrió.
            case "Crotona":               return (-530, "Crotona, Magna Grecia (Pitágoras)");
            case "Menfis":                return (-450, "Menfis y el Nilo, Egipto (Heródoto)");
            case "Bagdad":                return (820, "Bagdad, la Ciudad Redonda (al-Juarismi)");
            case "Woolsthorpe":           return (1666, "Woolsthorpe, Lincolnshire (Newton, el año de la peste)");
            case "Berkeley":              return (1796, "Berkeley, Gloucestershire (Edward Jenner)");
            case "Angostura":             return (1819, "Angostura del Orinoco (Simón Bolívar)");
            case "Sils Maria":            return (1881, "Sils Maria, Alta Engadina (Nietzsche)");
            case "París 1898":            return (1898, "París, rue Lhomond (Marie y Pierre Curie)");
            case "Colorado Springs":      return (1899, "Colorado Springs (Nikola Tesla)");
            case "Berna":                 return (1905, "Berna (Albert Einstein, el año milagroso)");
            case "Pasadena":              return (1953, "Pasadena y Argonne (Clair C. Patterson)");
            case "Chernóbil":             return (1986, "Prípiat y la central de Chernóbil (Alekséi Ananenko y su escuadrón)");
            case "Palenque":              return (683, "Lakamha' (Palenque), Chiapas: la muerte y el renacer de K'inich Janaab Pakal");
            case "Velliangiri":           return (1999, "Isha Yoga Center, al pie de los montes Velliangiri (Coimbatore): Sadhguru y el Dhyanalinga");
            case "Hawthorne":             return (2008, "La fábrica de SpaceX en Hawthorne, California (Elon Musk y el cuarto Falcon 1)");
            case "Aldea de la montaña":   return (1930, "Una aldea de montaña de la China tradicional (homenaje a Goku, ficción)");
            case "Albuquerque":           return (1975, "Albuquerque, Nuevo México (Bill Gates, Paul Allen y el BASIC del Altair)");
            case "Omaha":                 return (1983, "Omaha, Nebraska (Warren Buffett compra la Nebraska Furniture Mart a Rose Blumkin)");
            case "Prairie View":          return (2020, "Prairie View A&M University, Texas (la donación de MacKenzie Scott)");
            case "Uagadugú":              return (2024, "Uagadugú, Burkina Faso (la entrega de tractores a los productores)");
            // La tanda 3 (docs/microcosmos-historias-3.md)
            case "Chimbacalle":           return (1908, "Chimbacalle, Quito (Eloy Alfaro y la llegada del tren de Guayaquil)");
            case "Lagunillas":            return (1557, "Tierras de Arauco, Chile (Galvarino vuelve con su gente)");
            case "Cajamarca":             return (1533, "Cajamarca, Perú (Atahualpa y el rescate)");
            case "Onondaga":              return (1450, "El lago Onondaga, Nueva York (la Gran Ley de la Paz; fecha tradicional discutida)");
            case "Alejandría de Cleopatra": return (-42, "Alejandría, Egipto (Cleopatra y los años sin crecida del Nilo)");
            case "Langley":               return (1962, "Langley, Hampton, Virginia (Johnson, Vaughan, Jackson y el vuelo de Glenn)");
            case "Viena 1824":            return (1824, "Viena (Beethoven y el estreno de la Novena)");
            case "Leiden":                return (1637, "Leiden, Provincias Unidas (Descartes publica el Discurso del método)");
            case "San Pablo":             return (1952, "La colonia de San Pablo, Loreto, Perú (Ernesto Guevara y Alberto Granado)");
            case "Madrid 2018":           return (2018, "El Congreso de los Diputados, Madrid (la moción de censura de 2018)");
            case "Gettysburg":            return (1863, "Gettysburg, Pensilvania (Lincoln y el cementerio nacional)");
            case "Feria de las Hazañas":  return (2026, "Una feria inventada (grandes hazañas de presidentes y primeros ministros)");
            case "Planta Atómica":        return (1905, "Woolsthorpe, París y Berna (Newton, Curie, Einstein)");
            default: return null;
        }
    }
}
