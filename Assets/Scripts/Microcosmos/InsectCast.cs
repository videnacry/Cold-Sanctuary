using UnityEngine;

/// <summary>
/// EL INSECTO DE CADA PERSONAJE (docs/microcosmos-civilizaciones.md §5). Ley de microcosmos-eras-and-observation.md §1: el
/// animal se elige por su ETOLOGÍA (lo que hace en la naturaleza se parece a lo que la persona hizo), no al revés. En el
/// microcosmos nadie entra con cuerpo humano (<see cref="InsectOnly"/>): esto dice en qué insecto se convierte cada uno.
/// </summary>
public static class InsectCast
{
    /// <summary>(patrón en el nombre, especie, por qué). Se busca en orden: los nombres compuestos («Yip Man (Shanidar)»)
    /// caen en el personaje histórico antes que en el alma renacida.</summary>
    public static readonly (string match, string species, string why)[] Table =
    {
        ("bruce lee",     "Mantis",      "el estilo de la mantis es kung fu real; espera inmóvil y golpea primero (intercepta), delgada y explosiva; nativa de Hong Kong (Hierodula patellifera)"),
        ("yip man",       "Mantis",      "el maestro de Bruce Lee: el mismo linaje, una mantis vieja"),
        ("nightingale",   "Firefly",     "la dama de la lámpara: la ronda de noche con su luz"),
        ("siddh",         "Cicada",      "años bajo tierra y una muda: deja la piel vieja (el despertar)"),
        ("buda",          "Cicada",      "años bajo tierra y una muda: deja la piel vieja (el despertar)"),
        ("patanjali",     "Snail",       "quietud (sthira-sukham āsanam); la concha enrollada recuerda a Ādiśeṣa"),
        ("erat",          "DungBeetle",  "el pelotero se orienta por el sol y la Vía Láctea: el que midió la Tierra con el sol"),
        ("wright",        "Grasshopper", "salta y planea con alas: salto, ala y control"),
        ("sendler",       "Earwig",      "la madre tijereta guarda y cuida a sus crías: escondió a niños"),
        ("chambon",       "SnowBee",     "la colmena entera protege: el pueblo escondió a miles"),
        ("soyer",         "Cockroach",   "el cocinero de campaña: vive en las cocinas y no le asusta nada"),
        ("sutherland",    "Woodlouse",   "el inspector sanitario: la cochinilla limpia lo que se pudre"),
        ("trocm",         "SnowBee",     "Le Chambon: la colmena entera protege (el pastor y su familia)"),
        ("theis",         "SnowBee",     "Le Chambon: la colmena entera protege"),
        ("piotrowska",    "Earwig",      "escondió niños en su casa: la madre tijereta"),
        ("newton",        "Spider",      "solitario y obsesivo, teje solo su obra (microcosmos-eras-and-observation.md §1: aislado → araña)"),
        ("curie",         "Firefly",     "la luz del radio en la oscuridad del cobertizo"),
        ("einstein",      "Cricket",     "el grillo marca la temperatura con su canto (ley de Dolbear): la física en un insecto"),
        // Científicos e históricos (docs/microcosmos-cientificos.md)
        ("tesla",         "SnowBee",     "el abejorro siente el campo eléctrico de las flores (Clarke et al. 2013, Science 340:66): la electricidad en un insecto"),
        ("nietzsche",     "Scorpionfly", "la mosca escorpión de la nieve (Boreus) vive sola en el frío de las alturas: «a 6000 pies más allá del hombre y del tiempo»"),
        ("bolívar",       "Grasshopper", "la langosta del desierto cruza montañas y hasta el océano en enjambre: la travesía de los Andes"),
        ("bolivar",       "Grasshopper", "la langosta del desierto cruza montañas y hasta el océano en enjambre: la travesía de los Andes"),
        ("pitágoras",     "Cicada",      "las cigarras periódicas salen cada 13 o 17 años, números primos; la cigarra canta: número y música"),
        ("pitagoras",     "Cicada",      "las cigarras periódicas salen cada 13 o 17 años, números primos; la cigarra canta: número y música"),
        ("ananenko",      "Termite",     "las termitas soldado cierran la brecha del termitero aunque les cueste la vida: el escuadrón que entró en el agua"),
        ("bespalov",      "Termite",     "el escuadrón de Chernóbil: termitas soldado que cierran la brecha"),
        ("baranov",       "Termite",     "el escuadrón de Chernóbil: termitas soldado que cierran la brecha"),
        ("jenner",        "Ant",         "las hormigas se inmunizan unas a otras con dosis pequeñas de esporas (Konrad et al. 2012, PLoS Biol 10:e1001300): una vacuna social"),
        ("patterson",     "Woodlouse",   "la cochinilla acumula plomo y otros metales: es el bioindicador de la contaminación por plomo"),
        ("heródoto",      "DungBeetle",  "el escarabajo sagrado de Egipto; Heródoto contó del fénix que lleva a su padre en una bola de mirra (II.73)"),
        ("herodoto",      "DungBeetle",  "el escarabajo sagrado de Egipto; Heródoto contó del fénix que lleva a su padre en una bola de mirra (II.73)"),
        ("juarismi",      "SnowBee",     "la abeja calcula: su danza codifica el ángulo con el sol y la distancia, y construye celdas hexagonales"),
        ("khwarizmi",     "SnowBee",     "la abeja calcula: su danza codifica el ángulo con el sol y la distancia, y construye celdas hexagonales"),
        // Los famosos (docs/microcosmos-famosos.md): sus microcosmos piden la especie explícita; aquí, el porqué.
        ("elon musk",     "Grasshopper", "el salto es un lanzamiento: guarda energía en las patas y la suelta de golpe (Bennet-Clark 1975)"),
        ("goku",          "DungBeetle",  "el insecto más fuerte por su peso (Onthophagus taurus): entrena empujando cada día su carga"),
        ("bill gates",    "Ant",         "la colonia resuelve problemas repartiéndolos en pasos sencillos (algoritmos de colonia de hormigas, Dorigo 1992)"),
        ("paul allen",    "Spider",      "teje solo, de noche, la red que lo sostiene todo (el simulador del 8080)"),
        ("buffett",       "Ant",         "la hormiga segadora guarda grano en los años buenos para los malos: paciencia"),
        ("blumkin",       "SnowBee",     "la obrera que no para: la señora B trabajó hasta pasados los 100 años"),
        ("mackenzie",     "SnowBee",     "la abeja da sin pedir nada: poliniza las flores que visita y sigue su vuelo"),
        ("traoré",        "Termite",     "las Macrotermes de la sabana cultivan su propio hongo: la colonia que se alimenta a sí misma"),
        ("traore",        "Termite",     "las Macrotermes de la sabana cultivan su propio hongo: la colonia que se alimenta a sí misma"),
        ("kushal",        "Ant",         "canon: el mirmidón (Egina); el jugador entra como hormiga"),
        ("player",        "Ant",         "canon: el mirmidón (Egina); el jugador entra como hormiga"),
        ("magnate",       "Aphid",       "se divide en clones: el pulgón se clona (partenogénesis)"),
        ("linduarda",     "Firefly",     "médica: la luz en la noche"),
        ("lizmibet",      "Ladybug",     "médica: la mariquita protege el huerto"),
        ("jefeconstruccion", "Termite",  "las termitas construyen montículos"),
        ("jefe_construccion", "Termite", "las termitas construyen montículos"),
        ("sadhguru",      "Cicada",      "la muda, el canto que medita"),
        ("andrea",        "Ladybug",     "elegante, de colores"),
        ("goluis",        "DungBeetle",  "fuerza (y cocina)"),
        ("panterilia",    "Antlion",     "percepción: el acecho del león"),
        ("gohageneis",    "Grasshopper", "nómada: la langosta migratoria"),
        ("irosene",       "Spider",      "tejedora: el textil"),
        ("atlas",         "DungBeetle",  "el insecto más fuerte por su peso (Onthophagus taurus)"),
        ("momo",          "Cricket",     "el bromista que canta"),
        ("héspero",       "Firefly",     "vigía de estrellas y señor del fuego: la luz"),
        ("hespero",       "Firefly",     "vigía de estrellas y señor del fuego: la luz"),
        ("ambrosio",      "Aphid",       "canon: el pulgón (Nasatya)"),
        ("medea",         "Ant",         "la tirana de las feromonas"),
        ("ruth",          "Ant",         "la sembradora: las hormigas que cultivan hongo"),
        ("sakshi",        "Mantis",      "la observadora que espera inmóvil"),
        ("shanidar",      "Woodlouse",   "el anciano: un linaje antiquísimo"),
    };

    /// <summary>Las especies que conviven en las eras civilizadas (para los que no tienen una propia: variado por su nombre).</summary>
    public static readonly string[] Folk = { "Ant", "Ladybug", "Grasshopper", "Cricket", "Termite", "SnowBee", "Firefly", "Cicada", "Earwig", "Spider", "DungBeetle", "Mantis", "Centipede" };

    /// <summary>La especie de <paramref name="name"/> (o una de <see cref="Folk"/> por su nombre).</summary>
    public static string SpeciesFor(string name)
    {
        string n = (name ?? "").ToLowerInvariant();
        foreach (var t in Table) if (n.Contains(t.match)) return t.species;
        return Folk[Mathf.Abs(n.GetHashCode()) % Folk.Length];
    }

    /// <summary>Por qué es ese insecto («» si va por sorteo).</summary>
    public static string Why(string name)
    {
        string n = (name ?? "").ToLowerInvariant();
        foreach (var t in Table) if (n.Contains(t.match)) return t.why;
        return "";
    }

    /// <summary>Color del cuerpo de cada especie (el de la galería).</summary>
    public static Color ColorOf(string species)
    {
        switch (species)
        {
            case "Ant": return new Color(0.4f, 0.25f, 0.18f);
            case "Aphid": return new Color(0.62f, 0.75f, 0.4f);
            case "Ladybug": return new Color(0.8f, 0.15f, 0.12f);
            case "DungBeetle": return new Color(0.2f, 0.18f, 0.16f);
            case "Grasshopper": return new Color(0.45f, 0.62f, 0.25f);
            case "Cricket": return new Color(0.3f, 0.22f, 0.15f);
            case "Mantis": return new Color(0.4f, 0.68f, 0.3f);
            case "Antlion": return new Color(0.55f, 0.48f, 0.38f);
            case "Termite": return new Color(0.9f, 0.85f, 0.72f);
            case "SnowBee": return new Color(0.85f, 0.7f, 0.25f);
            case "Spider": return new Color(0.35f, 0.3f, 0.28f);
            case "Firefly": return new Color(0.25f, 0.22f, 0.2f);
            case "Cicada": return new Color(0.3f, 0.35f, 0.25f);
            case "Earwig": return new Color(0.4f, 0.22f, 0.12f);
            case "Snail": return new Color(0.6f, 0.5f, 0.38f);
            case "Woodlouse": return new Color(0.45f, 0.45f, 0.48f);
            case "Cockroach": return new Color(0.45f, 0.25f, 0.12f);
            case "Centipede": return new Color(0.55f, 0.22f, 0.1f);
            default: return new Color(0.45f, 0.35f, 0.25f);
        }
    }
}
