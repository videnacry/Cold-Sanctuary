using System.Collections.Generic;

/// <summary>
/// RECETARIO DEL RESTAURANTE (docs/cocina-profesional.md): los platillos de la carta y las ELABORACIONES BASE que piden
/// (harina, masa madre, pan, pasta, queso, yogur, tofu, tempeh, tortillas, caldos…), con su equipo, su tiempo activo y su
/// tiempo de REPOSO (fermentar, madurar, cuajar, curar). Las materias primas salen de los HUERTOS (cultivos), del
/// LABORATORIO de carne cultivada (res, cerdo, pollo, conejo, pescado, marisco) y de la FERMENTACIÓN DE PRECISIÓN (proteínas
/// de leche y de huevo sin animales: el santuario no explota animales). Datos puros: los usa `KitchenLarder`.
/// </summary>
public static class Gastronomy
{
    public enum Source { Cultivo, Elaboracion, CarneCultivada, Fermentacion, Basico }

    public class Prep
    {
        public string name, equipment, how;
        public string[] inputs;
        public float activeMin, restHours;   // minutos de trabajo; horas de reposo/fermentación/maduración
        public int portions;                 // raciones/unidades que salen de una tanda
        public Prep(string n, string eq, float active, float rest, int portions, string how, params string[] inputs)
        { name = n; equipment = eq; activeMin = active; restHours = rest; this.portions = portions; this.how = how; this.inputs = inputs; }
    }

    public class Dish
    {
        public string name, origin, equipment, how;
        public string[] components;
        public Dish(string n, string origin, string eq, string how, params string[] comps) { name = n; this.origin = origin; equipment = eq; this.how = how; components = comps; }
    }

    // ── Materias primas ─────────────────────────────────────────────────────────────────────────
    // Lo que llega sin elaborar: del huerto (cultivo), del laboratorio (carne cultivada) o de la fermentación de precisión.
    public static readonly Dictionary<string, (Source src, string from)> Raw = new Dictionary<string, (Source, string)>
    {
        // Huertos (GardenCrop.Kind entre paréntesis cuando ya existe)
        { "trigo", (Source.Cultivo, "Trigo") }, { "trigo duro", (Source.Cultivo, "Trigo") }, { "maíz", (Source.Cultivo, "Maiz") },
        { "maíz morado", (Source.Cultivo, "MaizMorado") }, { "choclo", (Source.Cultivo, "Maiz") }, { "arroz", (Source.Cultivo, "Arroz") },
        { "soja", (Source.Cultivo, "Soja") }, { "habas", (Source.Cultivo, "Haba") }, { "chochos", (Source.Cultivo, "Tarwi") },
        { "fréjol", (Source.Cultivo, "Frijol") }, { "arveja", (Source.Cultivo, "Arveja") }, { "lenteja", (Source.Cultivo, "Lenteja") },
        { "garbanzo", (Source.Cultivo, "Garbanzo") }, { "maní", (Source.Cultivo, "Mani") }, { "zapallo", (Source.Cultivo, "Calabaza") },
        { "melloco", (Source.Cultivo, "Melloco") }, { "papa", (Source.Cultivo, "Papa") }, { "tomate", (Source.Cultivo, "Tomate") },
        { "cebolla", (Source.Cultivo, "Cebolla") }, { "ajo", (Source.Cultivo, "Ajo") }, { "cebollín", (Source.Cultivo, "Cebollin") },
        { "pimiento", (Source.Cultivo, "Pimiento") }, { "chile guajillo", (Source.Cultivo, "Chile") }, { "achiote", (Source.Cultivo, "Achiote") },
        { "azafrán", (Source.Cultivo, "Azafran") }, { "romero", (Source.Cultivo, "Romero") }, { "albahaca", (Source.Cultivo, "Albahaca") },
        { "cilantro", (Source.Cultivo, "Cilantro") }, { "perejil", (Source.Cultivo, "Perejil") }, { "jengibre", (Source.Cultivo, "Jengibre") },
        { "judía verde", (Source.Cultivo, "Frijol") }, { "garrofón", (Source.Cultivo, "Frijol") }, { "plátano maduro", (Source.Cultivo, "Banana") },
        { "piña", (Source.Cultivo, "Pina") }, { "naranjilla", (Source.Cultivo, "Naranjilla") }, { "mortiño", (Source.Cultivo, "Mortino") },
        { "babaco", (Source.Cultivo, "Babaco") }, { "frutilla", (Source.Cultivo, "Frutilla") }, { "hierbaluisa", (Source.Cultivo, "Hierbaluisa") },
        { "cedrón", (Source.Cultivo, "Cedron") }, { "canela", (Source.Cultivo, "Canela") }, { "clavo", (Source.Cultivo, "Clavo") },
        { "caña de azúcar", (Source.Cultivo, "Cana") }, { "café", (Source.Cultivo, "Cafe") }, { "cacao", (Source.Cultivo, "Cacao") },
        { "aceitunas", (Source.Cultivo, "Olivo") }, { "limón", (Source.Cultivo, "Limon") }, { "alga nori", (Source.Cultivo, "Alga") },
        { "alga kombu", (Source.Cultivo, "Alga") }, { "shiitake", (Source.Cultivo, "Shiitake") }, { "sésamo", (Source.Cultivo, "Sesamo") },
        { "wasabi", (Source.Cultivo, "Wasabi") }, { "espinaca", (Source.Cultivo, "Espinaca") }, { "pepino", (Source.Cultivo, "Pepino") },
        { "aguacate", (Source.Cultivo, "Aguacate") },
        // Laboratorio de carne cultivada (CulturedMeatLab)
        { "res cultivada", (Source.CarneCultivada, "res") }, { "cerdo cultivado", (Source.CarneCultivada, "cerdo") },
        { "pollo cultivado", (Source.CarneCultivada, "pollo") }, { "conejo cultivado", (Source.CarneCultivada, "conejo") },
        { "salmón cultivado", (Source.CarneCultivada, "salmón") }, { "atún cultivado", (Source.CarneCultivada, "atún") },
        { "bacalao cultivado", (Source.CarneCultivada, "bacalao") }, { "calamar cultivado", (Source.CarneCultivada, "calamar y su tinta") },
        { "gamba cultivada", (Source.CarneCultivada, "gamba") },
        // Fermentación de precisión (levaduras que producen proteínas de leche y de huevo, sin animales)
        { "proteína de leche", (Source.Fermentacion, "caseína y suero de fermentación de precisión") },
        { "proteína de huevo", (Source.Fermentacion, "ovoalbúmina de fermentación de precisión") },
        { "levadura", (Source.Fermentacion, "Saccharomyces cultivada en el obrador") },
        { "cultivos lácticos", (Source.Fermentacion, "bacterias lácticas (S. thermophilus, L. bulgaricus…)") },
        { "cuajo microbiano", (Source.Fermentacion, "quimosina de fermentación (sin cuajo animal)") },
        { "koji", (Source.Fermentacion, "Aspergillus oryzae sobre arroz") }, { "rhizopus", (Source.Fermentacion, "esporas para tempeh") },
        // Básicos
        { "agua", (Source.Basico, "agua de deshielo filtrada") }, { "sal", (Source.Basico, "sal marina evaporada al sol") },
        { "cal", (Source.Basico, "hidróxido de calcio para nixtamalizar") }, { "nigari", (Source.Basico, "cloruro de magnesio del agua de mar") },
    };

    // ── Elaboraciones base ──────────────────────────────────────────────────────────────────────
    public static readonly Prep[] Preps =
    {
        new Prep("harina de trigo", "molino de piedra", 20, 0, 20, "moler el trigo y tamizar", "trigo"),
        new Prep("sémola de trigo duro", "molino de piedra", 25, 0, 20, "moler grueso el trigo duro (pasta)", "trigo duro"),
        new Prep("harina de maíz morado", "molino de piedra", 20, 0, 15, "moler el maíz morado seco", "maíz morado"),
        new Prep("azúcar / panela", "trapiche y paila", 40, 2, 20, "exprimir la caña, hervir el guarapo hasta cristalizar", "caña de azúcar"),
        new Prep("aceite de oliva", "almazara", 40, 0, 20, "moler la aceituna, batir y decantar en frío", "aceitunas"),
        new Prep("masa madre", "cámara de fermentación", 10, 120, 30, "harina y agua, alimentada cada día 5 días hasta que dobla", "harina de trigo", "agua"),
        new Prep("leche (sin animales)", "marmita", 15, 0, 20, "disolver proteína de leche de fermentación con grasa vegetal y agua; pasteurizar 72 °C 15 s", "proteína de leche", "aceite de oliva", "agua"),
        new Prep("leche de soja", "marmita", 30, 8, 20, "remojar la soja 8 h, triturar con agua, hervir 10 min y colar (okara aparte)", "soja", "agua"),
        new Prep("nata y mantequilla", "batidora planetaria", 20, 0, 15, "separar la nata de la leche y batirla hasta separar la mantequilla", "leche (sin animales)"),
        new Prep("yogur", "yogurtera (incubadora)", 15, 6, 20, "calentar la leche a 85 °C, enfriar a 43 °C, añadir cultivos lácticos e incubar 4–8 h; enfriar", "leche (sin animales)", "cultivos lácticos"),
        new Prep("huevo (sin animales)", "batidora planetaria", 5, 0, 20, "proteína de huevo de fermentación + agua: bate, cuaja y monta como el huevo", "proteína de huevo", "agua"),
        new Prep("queso fresco / quesillo", "cuba de quesería", 45, 2, 12, "leche a 32 °C + cuajo microbiano; cuajada 40 min; cortar, desuerar y salar", "leche (sin animales)", "cuajo microbiano", "sal"),
        new Prep("mozzarella", "cuba de quesería", 60, 4, 12, "cuajada acidificada; hilar en agua a 80 °C y formar bolas", "leche (sin animales)", "cultivos lácticos", "cuajo microbiano"),
        new Prep("mascarpone", "marmita", 20, 12, 10, "nata a 85 °C con zumo de limón; escurrir 12 h en frío", "nata y mantequilla", "limón"),
        new Prep("queso crema", "cuba de quesería", 20, 14, 10, "leche y nata con cultivos lácticos 12–16 h; escurrir", "leche (sin animales)", "nata y mantequilla", "cultivos lácticos"),
        new Prep("parmesano (madurado)", "cava de maduración", 90, 8760, 30, "cuajada cocida a 55 °C, prensada, en salmuera 20 días y madurada 12 meses a 12 °C", "leche (sin animales)", "cuajo microbiano", "sal"),
        new Prep("pan de masa madre", "horno mixto", 40, 16, 16, "amasar, fermentación en bloque 4 h, formar, retardo en frío 12 h, hornear 230 °C 35 min", "harina de trigo", "masa madre", "agua", "sal"),
        new Prep("guaguas de pan", "horno mixto", 50, 3, 20, "masa enriquecida con azúcar y mantequilla; formar las guaguas, fermentar 2 h y hornear", "harina de trigo", "levadura", "azúcar / panela", "nata y mantequilla"),
        new Prep("masa de pizza", "amasadora espiral", 20, 48, 20, "harina, agua (65 %), sal y poca levadura; maduración en frío 24–72 h", "harina de trigo", "agua", "sal", "levadura"),
        new Prep("pasta fresca", "laminadora de pasta", 40, 0.5f, 20, "sémola y huevo, amasar, reposar 30 min, laminar y cortar", "sémola de trigo duro", "huevo (sin animales)"),
        new Prep("pasta seca (espaguetis, fideos)", "extrusora y secadero", 30, 36, 40, "sémola y agua extruidas por molde de bronce; secar 24–48 h a baja temperatura", "sémola de trigo duro", "agua"),
        new Prep("fideos de ramen", "laminadora de pasta", 40, 24, 20, "harina con agua alcalina (kansui), laminar, cortar y reposar 1 día", "harina de trigo", "agua", "sal"),
        new Prep("bizcocho", "horno mixto", 30, 0, 12, "montar huevo con azúcar, incorporar la harina y hornear 180 °C", "huevo (sin animales)", "azúcar / panela", "harina de trigo"),
        new Prep("bizcochos de soletilla", "horno mixto", 30, 0, 30, "claras montadas con azúcar, yemas y harina; mangas y horno 190 °C", "huevo (sin animales)", "azúcar / panela", "harina de trigo"),
        new Prep("tofu", "prensa de tofu", 30, 0.5f, 10, "leche de soja a 75 °C + nigari; cuajar, cortar y prensar 20 min", "leche de soja", "nigari"),
        new Prep("tempeh", "incubadora de tempeh", 40, 36, 10, "soja descascarillada cocida, inocular rhizopus y fermentar 30 °C 24–48 h", "soja", "rhizopus"),
        new Prep("seitán", "marmita", 40, 1, 10, "lavar la masa de harina hasta dejar el gluten y cocerlo en caldo", "harina de trigo", "agua"),
        new Prep("salsa de soja (shoyu)", "barricas de fermentación", 30, 4380, 50, "soja y trigo con koji y salmuera fermentados 6 meses; prensar", "soja", "trigo", "koji", "sal"),
        new Prep("miso", "barricas de fermentación", 30, 4380, 40, "soja cocida con koji de arroz y sal, fermentado 6–12 meses", "soja", "koji", "sal"),
        new Prep("vinagre de arroz", "barricas de fermentación", 20, 720, 40, "sake de arroz (koji + levadura) y después fermentación acética 1 mes", "arroz", "koji", "levadura"),
        new Prep("arroz de sushi (shari)", "arrocera", 30, 0, 20, "lavar el arroz, cocer y aliñar con vinagre de arroz, azúcar y sal", "arroz", "vinagre de arroz", "azúcar / panela", "sal"),
        new Prep("masa de maíz nixtamalizado", "nixtamalizadora y molino", 45, 10, 30, "cocer el maíz con cal 40 min, reposar 8–12 h, lavar y moler", "maíz", "cal", "agua"),
        new Prep("tortillas", "comal", 30, 0, 40, "bolear la masa, prensar y cocer en el comal", "masa de maíz nixtamalizado"),
        new Prep("caldo de huesos (tonkotsu)", "marmita a presión", 30, 10, 30, "hervir los huesos y la grasa de cerdo cultivado 8–12 h hasta emulsionar", "cerdo cultivado", "agua", "cebolla", "ajo", "jengibre"),
        new Prep("dashi", "marmita", 20, 1, 30, "kombu en frío y shiitake; calentar sin hervir", "alga kombu", "shiitake", "agua"),
        new Prep("fumet de pescado", "marmita", 30, 1, 30, "espinas y cabezas de pescado cultivado con sofrito, 30 min", "bacalao cultivado", "cebolla", "agua"),
        new Prep("sofrito", "fogón de inducción", 30, 0, 20, "cebolla, ajo, pimiento y tomate pochados lentamente en aceite", "cebolla", "ajo", "pimiento", "tomate", "aceite de oliva"),
        new Prep("salsa de tomate", "marmita", 30, 1, 20, "tomate, ajo y albahaca reducidos", "tomate", "ajo", "albahaca", "aceite de oliva"),
        new Prep("ragú boloñés", "marmita", 40, 3, 20, "sofrito con res cultivada picada, tomate y vino; 3 h a fuego lento", "res cultivada", "sofrito", "salsa de tomate"),
        new Prep("bechamel", "fogón de inducción", 15, 0, 20, "mantequilla y harina en roux; leche hasta espesar", "nata y mantequilla", "harina de trigo", "leche (sin animales)"),
        new Prep("café espresso", "cafetera espresso", 5, 0, 10, "tostar el café, moler y extraer a 9 bar", "café"),
        new Prep("leche condensada y evaporada", "marmita", 60, 0, 12, "reducir la leche (evaporada) y con azúcar (condensada)", "leche (sin animales)", "azúcar / panela"),
        new Prep("encurtido de jengibre (gari)", "barricas de fermentación", 15, 72, 30, "jengibre fino en vinagre de arroz y azúcar 3 días", "jengibre", "vinagre de arroz"),
        new Prep("adobo al pastor", "batidora planetaria", 20, 12, 20, "chiles, achiote, piña y especias; marinar el cerdo cultivado 12 h", "cerdo cultivado", "achiote", "chile guajillo", "piña"),
        new Prep("salsa de enchilada", "marmita", 30, 0, 20, "chiles guajillo tostados e hidratados, tomate, cebolla y ajo licuados y fritos", "chile guajillo", "tomate", "cebolla", "ajo"),
    };

    // ── Carta ───────────────────────────────────────────────────────────────────────────────────
    public static readonly Dish[] Menu =
    {
        new Dish("Colada morada", "Ecuador", "marmita", "colada de harina de maíz morado con frutas y hierbas, especiada; con guaguas de pan",
                 "harina de maíz morado", "mortiño", "naranjilla", "piña", "babaco", "frutilla", "hierbaluisa", "cedrón", "canela", "clavo", "azúcar / panela", "guaguas de pan"),
        new Dish("Fanesca", "Ecuador", "marmita", "sopa de doce granos tiernos con zapallo, leche y quesillo; con bacalao, huevo y plátano maduro",
                 "choclo", "habas", "chochos", "fréjol", "arveja", "lenteja", "maní", "zapallo", "melloco", "arroz", "leche (sin animales)", "queso fresco / quesillo", "bacalao cultivado", "huevo (sin animales)", "plátano maduro"),
        new Dish("Tres leches", "Latinoamérica", "horno mixto", "bizcocho empapado en tres leches y cubierto de nata montada",
                 "bizcocho", "leche condensada y evaporada", "leche (sin animales)", "nata y mantequilla", "canela"),
        new Dish("Pastel de maíz", "Ecuador", "horno mixto", "choclo molido con mantequilla, huevo y queso, horneado",
                 "choclo", "nata y mantequilla", "huevo (sin animales)", "queso fresco / quesillo", "azúcar / panela"),
        new Dish("Humitas", "Ecuador", "vaporera", "choclo molido con quesillo y mantequilla envuelto en hojas de choclo y al vapor",
                 "choclo", "queso fresco / quesillo", "nata y mantequilla", "huevo (sin animales)"),
        new Dish("Lasaña", "Italia", "horno mixto", "capas de pasta fresca, ragú y bechamel con parmesano, gratinada",
                 "pasta fresca", "ragú boloñés", "bechamel", "parmesano (madurado)", "mozzarella"),
        new Dish("Ramen tonkotsu", "Japón", "marmita a presión", "fideos en caldo de huesos con chashu de cerdo cultivado, huevo marinado, nori y cebollín",
                 "fideos de ramen", "caldo de huesos (tonkotsu)", "cerdo cultivado", "huevo (sin animales)", "salsa de soja (shoyu)", "alga nori", "cebollín"),
        new Dish("Pizza napolitana", "Italia", "horno de pizza (eléctrico)", "masa madurada 48 h, tomate, mozzarella y albahaca; 90 s a 450 °C",
                 "masa de pizza", "salsa de tomate", "mozzarella", "albahaca", "aceite de oliva"),
        new Dish("Tacos al pastor", "México", "trompo vertical (inducción)", "cerdo cultivado adobado al trompo en tortillas con piña, cebolla y cilantro",
                 "tortillas", "adobo al pastor", "piña", "cebolla", "cilantro"),
        new Dish("Enchiladas", "México", "horno mixto", "tortillas rellenas de pollo cultivado bañadas en salsa de chile, con queso y crema",
                 "tortillas", "pollo cultivado", "salsa de enchilada", "queso fresco / quesillo", "nata y mantequilla"),
        new Dish("Paella valenciana", "España", "paellero de inducción", "arroz con pollo y conejo cultivados, judía verde, garrofón, tomate, azafrán y romero",
                 "arroz", "pollo cultivado", "conejo cultivado", "judía verde", "garrofón", "tomate", "azafrán", "romero", "aceite de oliva"),
        new Dish("Arroz negro", "España", "paellero de inducción", "arroz con sofrito, fumet y la tinta y el calamar cultivados; con alioli",
                 "arroz", "calamar cultivado", "sofrito", "fumet de pescado", "ajo", "aceite de oliva"),
        new Dish("Arroz chaufa", "Perú", "wok de inducción", "arroz salteado al wok con pollo cultivado, huevo, cebollín, jengibre y sillao",
                 "arroz", "pollo cultivado", "huevo (sin animales)", "cebollín", "jengibre", "salsa de soja (shoyu)"),
        new Dish("Tiramisú", "Italia", "abatidor", "soletillas empapadas en espresso con crema de mascarpone y cacao; reposo en frío",
                 "bizcochos de soletilla", "café espresso", "mascarpone", "huevo (sin animales)", "azúcar / panela", "cacao"),
        new Dish("Pastel de queso", "Mundo", "horno mixto", "base de galleta y crema de queso horneada a baja temperatura",
                 "queso crema", "huevo (sin animales)", "azúcar / panela", "nata y mantequilla", "harina de trigo"),
        new Dish("Sushi (nigiri y maki)", "Japón", "barra de sushi", "arroz de sushi con salmón y atún cultivados, nori, wasabi y gari",
                 "arroz de sushi (shari)", "salmón cultivado", "atún cultivado", "alga nori", "wasabi", "encurtido de jengibre (gari)", "salsa de soja (shoyu)"),
        new Dish("Fideuà", "España", "paellero de inducción", "fideos finos tostados y cocidos en fumet con gambas y calamar cultivados; alioli",
                 "pasta seca (espaguetis, fideos)", "fumet de pescado", "gamba cultivada", "calamar cultivado", "sofrito"),
        new Dish("Espaguetis al pomodoro", "Italia", "fogón de inducción", "espaguetis secos con salsa de tomate y albahaca y parmesano",
                 "pasta seca (espaguetis, fideos)", "salsa de tomate", "albahaca", "parmesano (madurado)"),
        new Dish("Bowl de tofu y tempeh", "Asia", "wok de inducción", "tofu y tempeh salteados con verduras, arroz y miso",
                 "tofu", "tempeh", "arroz", "miso", "espinaca", "sésamo"),
        new Dish("Pan de masa madre con aceite", "Mediterráneo", "horno mixto", "pan del obrador con aceite de oliva",
                 "pan de masa madre", "aceite de oliva"),
    };

    public static Prep PrepOf(string name) { foreach (Prep p in Preps) if (p.name == name) return p; return null; }
    public static Dish DishOf(string name) { foreach (Dish d in Menu) if (d.name == name) return d; return null; }

    /// <summary>Equipos que piden los platillos y elaboraciones (para montar la cocina).</summary>
    public static HashSet<string> Equipment()
    {
        var set = new HashSet<string>();
        foreach (Prep p in Preps) set.Add(p.equipment);
        foreach (Dish d in Menu) set.Add(d.equipment);
        return set;
    }
}
