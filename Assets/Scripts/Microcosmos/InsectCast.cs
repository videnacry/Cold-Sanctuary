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
