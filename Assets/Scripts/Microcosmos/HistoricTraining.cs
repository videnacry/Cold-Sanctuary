using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Cómo se mueve el cuerpo en un ejercicio de época (para animarlo: dos posturas A ↔ B).</summary>
public enum DrillMotion { Levantar, Empujar, Balancear, Luchar, Correr, Lanzar, Golpear, Sostener, Saltar }

/// <summary>
/// ENTRENAR COMO EN SU ÉPOCA (Berón 2026-11-04: «¿en ninguna época de microcosmos hay gimnasio? desde la antigua Grecia se
/// entrenaba físicamente…»; docs/gimnasio-maquinas.md §16). El CATÁLOGO de los lugares de entrenamiento de la historia
/// (<see cref="Tradition"/>: el gimnasio y la palestra griegos, la palestra de las termas romanas, los patios de lucha del Nilo y de
/// Sumer, el akhāṛā y el kalari de la India, el maydān de furusiyya abasí, el zurkhaneh persa, el tekke de luchadores otomano, la
/// escuela de artes marciales china, el dojo, el juego de pelota maya, la sala de armas y la liza medievales, la sala de esgrima, el
/// Turnverein y la gimnasia del XIX, el club deportivo soviético y el gimnasio moderno), con sus EJERCICIOS Y SU EQUIPO de época
/// (<see cref="Drill"/>: halteres, mazas, piedras, lucha, carrera…; cada uno con los años en que existe).
/// Se elige por el ESTILO DE ÉPOCA Y LUGAR del pueblo (<see cref="MicroStyles.Entry.id"/>, el mismo que da el nombre del edificio en
/// <see cref="MicroStyles.CivicName"/>) y el año. Los PLANES de los insectos-humanos usan el mismo motor que el gimnasio del santuario
/// (<see cref="GymProgramming.Spec"/>: series, repeticiones, descanso y RPE por objetivo; progresión doble y descarga cada 4.ª semana).
/// La sesión en vivo y el maestro del pueblo: <see cref="HistoricSession"/> y <see cref="HistoricClass"/>, en el edificio con la
/// plantilla «Centro de entrenamiento» (o «Dojo») del registro (<see cref="AreaFunctions.Buildings"/>), que levanta la rama de los pueblos.
/// </summary>
public static class HistoricTraining
{
    /// <summary>Plantillas del registro donde se entrena con este motor (las del pueblo; docs/microcosmos-actividades.md).</summary>
    public static readonly string[] Buildings = { "Centro de entrenamiento", "Dojo" };
    public static bool IsTrainingSite(AreaFunctionSite s) => s != null && (s.activity == AreaFunctionSite.Entrenamiento || System.Array.IndexOf(Buildings, s.building) >= 0);

    public class Drill
    {
        public string id, name, equipment, muscle;
        public DrillMotion motion;
        public bool compound = true, timed;
        public float minutes;              // los de por tiempo (correr, formas, asaltos)
        public float baseLoad, step;       // carga de partida y lo que sube (0 = peso del cuerpo)
        public string unit = "kg";
        public int from = -100000, to = 3000;
        public string[] styles;            // (sin uso: cada tradición es ya de una clase de época)
        public bool Loaded => baseLoad > 0f;
        public bool In(int year, string kind = null) => year >= from && year <= to;
    }

    public class Tradition
    {
        public string id, place, trainer, why;
        public string[] styles;                                // las CLASES de los pueblos (AreaFunctionSite.training) que la usan
        public int from, to;
        public string[] warmup, cooldown;                      // ids de ejercicios por tiempo
        public List<(string name, string[] drills)> sessions = new List<(string, string[])>();
        public string[] cues;                                  // lo que corrige el maestro
        public bool Covers(string kind) => styles.Contains(kind);
    }

    static readonly Dictionary<string, Drill> _drills = new Dictionary<string, Drill>();
    public static readonly List<Tradition> All = new List<Tradition>();
    public static Drill D(string id) => _drills.TryGetValue(id, out Drill d) ? d : null;

    static void X(string id, string name, string equipment, string muscle, DrillMotion m, float load = 0f, float step = 0f, int from = -100000, int to = 3000, bool compound = true, string[] styles = null, string unit = "kg")
        => _drills[id] = new Drill { id = id, name = name, equipment = equipment, muscle = muscle, motion = m, baseLoad = load, step = step, from = from, to = to, compound = compound, styles = styles, unit = unit };
    static void T(string id, string name, string equipment, DrillMotion m, float minutes, int from = -100000, int to = 3000, string[] styles = null)
        => _drills[id] = new Drill { id = id, name = name, equipment = equipment, muscle = "general", motion = m, timed = true, minutes = minutes, compound = false, from = from, to = to, styles = styles };
    static Tradition Add(string id, string place, string trainer, string[] styles, string why, string[] warm, string[] cool, string[] cues, params (string, string[])[] sessions)
    {
        var t = new Tradition { id = id, place = place, trainer = trainer, styles = styles, why = why, warmup = warm, cooldown = cool, cues = cues };
        t.sessions.AddRange(sessions);
        All.Add(t); return t;
    }
    static string[] S(params string[] s) => s;

    static HistoricTraining()
    {
        // ── Ejercicios (con los años en que existen) ─────────────────────────────────────────────
        T("carrera_suave", "Carrera suave", null, DrillMotion.Correr, 5f);
        T("estirar", "Estiramientos y masaje con aceite", null, DrillMotion.Sostener, 5f);
        X("carrera_estadio", "Carreras de un estadio (≈190 m)", null, "piernas", DrillMotion.Correr);
        X("lucha_pale", "Lucha de pie (palé)", null, "todo el cuerpo", DrillMotion.Luchar);
        X("pugilato", "Pugilato con correas (himantes)", "himantes de cuero", "hombros y brazos", DrillMotion.Golpear);
        X("halteres", "Saltos y alzadas con halteres", "halteres de piedra o plomo", "piernas y hombros", DrillMotion.Saltar, 2f, 0.5f, from: -708);
        X("disco", "Lanzamiento de disco", "disco de bronce", "tronco y hombros", DrillMotion.Lanzar, 2f, 0.5f);
        X("jabalina", "Lanzamiento de jabalina (con amentum)", "jabalina", "hombros", DrillMotion.Lanzar);
        X("korykos", "Golpes al korykos (saco colgado)", "korykos (saco de arena)", "hombros y brazos", DrillMotion.Golpear, from: -500, compound: false);
        X("ternero", "Cargar el ternero (como Milón de Crotona)", "un ternero (crece cada día)", "todo el cuerpo", DrillMotion.Levantar, 30f, 5f, from: -540);
        X("arco", "Tensar y tirar con arco", "arco y flechas", "espalda y hombros", DrillMotion.Levantar);
        X("palus", "Golpes al palus (poste) con espada de madera", "espada y escudo de mimbre de doble peso", "hombros y tronco", DrillMotion.Golpear, from: -300);
        X("trigon", "Pelota (trigón y harpastum)", "pelota", "hombros y piernas", DrillMotion.Lanzar, compound: false);
        T("natatio", "Nado en la natatio", null, DrillMotion.Correr, 5f, -100, 476);
        X("lucha_cinturon", "Lucha de cinturón", "cinturón", "todo el cuerpo", DrillMotion.Luchar);
        X("honda", "Tiro con honda", "honda y piedras", "hombros", DrillMotion.Lanzar, compound: false);
        X("tahtib", "Esgrima de palos (tahtib)", "palo largo", "hombros y piernas", DrillMotion.Golpear, from: -2000);
        X("malla", "Lucha (malla-yuddha)", null, "todo el cuerpo", DrillMotion.Luchar, from: -1500);
        X("gada", "Volteos de la gada (maza)", "gada", "hombros y espalda", DrillMotion.Balancear, 8f, 2f);
        X("piedra_india", "Alzar la piedra", "piedra", "todo el cuerpo", DrillMotion.Levantar, 20f, 5f);
        X("dand", "Dand (flexiones indias)", null, "pecho, hombros y espalda", DrillMotion.Empujar, from: 1500);
        X("baithak", "Baithak (sentadillas indias)", null, "piernas", DrillMotion.Saltar, from: 1500);
        X("mudgar", "Volteos de mudgar (mazas)", "par de mudgar", "hombros", DrillMotion.Balancear, 5f, 1f, from: 1500);
        X("mallakhamb", "Mallakhamb (posturas en el poste)", "poste de madera", "todo el cuerpo", DrillMotion.Sostener, from: 1135);
        X("kushti", "Kushti en el foso de tierra", "foso de tierra", "todo el cuerpo", DrillMotion.Luchar, from: 1500);
        T("meippayattu", "Meippayattu (secuencias del kalari)", null, DrillMotion.Saltar, 5f, 1100, 3000, S("india_moderna"));
        X("kaal", "Kaal eduppu (patadas del kalari)", null, "piernas y cadera", DrillMotion.Golpear, from: 1100);
        X("kettukari", "Kettukari (bastón largo del kalari)", "bastón de bambú", "hombros", DrillMotion.Golpear, from: 1100);
        X("lanza_birjas", "Lanza contra el blanco (birjas)", "lanza", "hombros y tronco", DrillMotion.Empujar);
        X("espada_poste", "Espada contra el poste", "espada de práctica", "hombros", DrillMotion.Golpear);
        X("chawgan", "Volteos del mazo de chawgan (polo, a pie)", "mazo de chawgan", "hombros", DrillMotion.Balancear, compound: false);
        X("koshti", "Lucha (koshti)", null, "todo el cuerpo", DrillMotion.Luchar);
        X("meel", "Volteos de meel (mazas)", "par de meel", "hombros", DrillMotion.Balancear, 5f, 1f);
        X("sang", "Sang (escudos de madera, tumbado)", "par de sang", "pecho y hombros", DrillMotion.Empujar, 10f, 2f);
        X("shena", "Shena (flexiones sobre la tabla)", "tabla de shena", "pecho y cadera", DrillMotion.Empujar);
        X("kabbadeh", "Volteos de kabbadeh (arco de hierro)", "kabbadeh", "hombros", DrillMotion.Balancear, 8f, 2f, compound: false);
        T("charkh", "Charkh (giros)", null, DrillMotion.Correr, 3f);
        X("gures", "Yağlı güreş (lucha con aceite)", "kispet de cuero y aceite", "todo el cuerpo", DrillMotion.Luchar);
        X("okculuk", "Tiro con arco en el okmeydanı", "arco compuesto", "espalda y hombros", DrillMotion.Levantar);
        X("cirit", "Lanzamiento de cirit (jabalina)", "cirit", "hombros", DrillMotion.Lanzar, compound: false);
        T("pesrev", "Peşrev (calentamiento ritual de los luchadores)", null, DrillMotion.Saltar, 5f);
        T("mabu", "Mabu (postura del jinete)", null, DrillMotion.Sostener, 3f);
        X("shisuo", "Volteos de shisuo (candado de piedra)", "shisuo", "hombros y agarre", DrillMotion.Balancear, 10f, 2.5f, from: 702);
        T("taolu", "Taolu (formas)", null, DrillMotion.Golpear, 5f);
        X("gun", "Bastón (gun)", "bastón largo", "hombros y tronco", DrillMotion.Golpear);
        X("patadas", "Patadas", null, "piernas y cadera", DrillMotion.Golpear, compound: false);
        X("muk_yan", "Muk yan jong (muñeco de madera)", "muñeco de madera", "brazos", DrillMotion.Golpear, from: 1850);
        X("chi_sao", "Chi sao (manos pegajosas)", null, "brazos", DrillMotion.Luchar, from: 1850);
        X("pesas_bl", "Pesas: curl y press", "barra y mancuernas", "brazos y hombros", DrillMotion.Levantar, 15f, 2.5f, from: 1900);
        X("suburi", "Suburi (cortes con bokken)", "bokken", "hombros y espalda", DrillMotion.Balancear);
        X("shiko", "Shiko (pisotón del sumo)", null, "piernas y cadera", DrillMotion.Saltar);
        X("ishi_sashi", "Ishi sashi (candados de piedra)", "ishi sashi", "antebrazos", DrillMotion.Balancear, 5f, 1f, compound: false);
        X("randori", "Randori (práctica libre de lucha)", null, "todo el cuerpo", DrillMotion.Luchar);
        X("pelota_cadera", "Golpes de cadera a la pelota de hule", "pelota de hule y yugo", "cadera y tronco", DrillMotion.Golpear);
        X("atlatl", "Lanzamiento con átlatl", "átlatl y dardos", "hombros", DrillMotion.Lanzar, compound: false);
        X("pell", "Golpes al pell (poste)", "espada y escudo de práctica", "hombros", DrillMotion.Golpear);
        X("ringen", "Lucha (ringen)", null, "todo el cuerpo", DrillMotion.Luchar);
        X("quintana", "Lanza contra la quintana", "lanza", "hombros y tronco", DrillMotion.Empujar, from: 1100);
        X("arco_largo", "Tensar el arco largo", "arco largo", "espalda y hombros", DrillMotion.Levantar);
        X("piedra_lanzar", "Lanzar la piedra", "piedra", "todo el cuerpo", DrillMotion.Lanzar, 6f, 1f);
        X("estocadas", "Estocadas a fondo", "espada o florete", "piernas y hombros", DrillMotion.Empujar);
        X("paradas", "Paradas y respuestas", "espada o florete", "brazos", DrillMotion.Golpear, compound: false);
        T("asalto", "Asalto de esgrima", "espada o florete", DrillMotion.Luchar, 3f);
        X("lucha_cornish", "Lucha (estilo de Cornualles o Cumberland)", null, "todo el cuerpo", DrillMotion.Luchar);
        X("palo_largo", "Palo largo (quarterstaff)", "palo", "hombros", DrillMotion.Golpear);
        X("lanza_llanera", "Lanza llanera (a pie)", "lanza", "hombros y tronco", DrillMotion.Empujar);
        X("machete", "Esgrima de machete", "machete de práctica", "brazos", DrillMotion.Golpear, compound: false);
        X("reck", "Dominadas en la barra fija (Reck)", "barra fija", "espalda y brazos", DrillMotion.Levantar, from: 1811);
        X("barren", "Fondos en paralelas (Barren)", "paralelas", "pecho y tríceps", DrillMotion.Empujar, from: 1811);
        X("anillas", "Anillas", "anillas", "hombros y brazos", DrillMotion.Sostener, from: 1840, compound: false);
        X("potro", "Saltos al potro", "potro", "piernas y hombros", DrillMotion.Saltar, from: 1811);
        X("cuerda", "Trepa de cuerda", "cuerda", "espalda y brazos", DrillMotion.Levantar, from: 1811);
        X("mazas_indias", "Mazas indias (Indian clubs)", "par de mazas indias", "hombros", DrillMotion.Balancear, 1f, 0.5f, from: 1850, compound: false);
        X("mancuernas", "Mancuernas: alzadas y press", "mancuernas", "hombros y brazos", DrillMotion.Levantar, 3f, 1f, from: 1711);
        T("calistenia", "Ejercicios libres (gimnasia sueca de Ling)", null, DrillMotion.Saltar, 5f, 1813);
        X("balon_medicinal", "Balón medicinal", "balón medicinal", "tronco", DrillMotion.Lanzar, 3f, 1f, from: 1889, compound: false);
        X("steinstossen", "Steinstossen (lanzar la piedra)", "piedra de 20 kg", "todo el cuerpo", DrillMotion.Lanzar, 20f, 2.5f, from: 1805);
        X("schwingen", "Schwingen (lucha suiza en el serrín)", "calzón de lona", "todo el cuerpo", DrillMotion.Luchar);
        X("girya", "Girya (pesa rusa): arrancadas y dos tiempos", "girya", "todo el cuerpo", DrillMotion.Balancear, 16f, 8f, from: 1704);
        X("dominadas_gto", "Dominadas (norma GTO)", "barra fija", "espalda y brazos", DrillMotion.Levantar);
        X("flexiones", "Flexiones", null, "pecho y brazos", DrillMotion.Empujar, compound: false);
        X("sambo", "Sambo", "kurtka (chaqueta)", "todo el cuerpo", DrillMotion.Luchar, from: 1938);
        X("banca", "Press de banca con barra", "barra y banco", "pecho y brazos", DrillMotion.Empujar, 40f, 2.5f, from: 1930);
        X("sentadilla_barra", "Sentadilla con barra", "barra y soportes", "piernas", DrillMotion.Saltar, 45f, 5f, from: 1920);
        X("muerto", "Peso muerto", "barra", "espalda y piernas", DrillMotion.Levantar, 60f, 5f, from: 1920);
        X("remo_barra", "Remo con barra", "barra", "espalda", DrillMotion.Levantar, 35f, 2.5f, from: 1920);
        X("maquina", "Máquina de placas", "máquina de placas", "según la máquina", DrillMotion.Empujar, 30f, 2.5f, from: 1970, compound: false);
        X("comba", "Comba", "cuerda de saltar", "piernas", DrillMotion.Saltar, compound: false);
        X("lutte", "Lutte traditionnelle (lucha)", null, "todo el cuerpo", DrillMotion.Luchar);

        // Ejercicios que añaden las clases de los pueblos (claude/edif-pueblos: MicroStyles.TrainingKind).
        X("salto_longitud", "Salto de longitud", null, "piernas", DrillMotion.Saltar, compound: false);
        X("mazo_lanzar", "Lanzar el mazo (sledgehammer)", "mazo de herrero", "todo el cuerpo", DrillMotion.Lanzar, 6f, 1f);
        X("garrote", "Juego de garrote (esgrima de garrote larense)", "garrote", "hombros y piernas", DrillMotion.Golpear);
        X("coleo_carrera", "Carreras de la manga de coleo (a pie)", null, "piernas", DrillMotion.Correr);
        T("marcha_hebert", "Marcha y carrera por el parcours", null, DrillMotion.Correr, 5f, 1905);
        X("cuadrupedia", "Cuadrupedia", null, "todo el cuerpo", DrillMotion.Empujar, from: 1905);
        X("trepar_hebert", "Trepar y suspenderse", "barra de suspensión", "espalda y brazos", DrillMotion.Levantar, from: 1905);
        X("saltar_plinto", "Saltar el plinto", "plinto", "piernas", DrillMotion.Saltar, from: 1905);
        X("levantar_tronco", "Levantar y llevar (un tronco, un compañero)", "tronco", "todo el cuerpo", DrillMotion.Levantar, 15f, 2.5f, from: 1905);
        X("lanzar_balon", "Lanzar el balón", "balón", "hombros y tronco", DrillMotion.Lanzar, compound: false);
        X("pases", "Pases con el pie", "balón", "piernas", DrillMotion.Golpear, compound: false);
        X("regate", "Conducción y regate", "balón", "piernas", DrillMotion.Correr, compound: false);
        X("tiro_puerta", "Tiros a puerta", "balón", "piernas y cadera", DrillMotion.Golpear, compound: false);
        X("velocidad_pista", "Series de velocidad en la pista", null, "piernas", DrillMotion.Correr);
        X("saco_boxeo", "Saco de boxeo", "saco", "hombros y brazos", DrillMotion.Golpear, compound: false);

        // ── Lugares de entrenamiento: uno por CLASE ESTABLE de los pueblos (AreaFunctionSite.training) ─────────────
        Add("heroicos", "Juegos heroicos (Ilíada XXIII)", "El más anciano de los héroes (dirige los juegos)", S("juegos_heroicos"),
            "Antes del gimnasio (s. VI a. C.): los juegos fúnebres de Patroclo: carrera, lucha, pugilato, peso, jabalina y arco.",
            S("carrera_suave"), S("estirar"), S("pies bajo los hombros al agarrar", "brazo largo en el lanzamiento"),
            ("Juegos", S("carrera_estadio", "lucha_pale", "pugilato", "disco", "jabalina", "arco")));
        Add("griego", "Gimnasio y palestra", "Paidotribes (maestro de la palestra)", S("gimnasio_griego"),
            "Xisto, skamma, halteres, kórykos, disco y jabalina; en Crotona, Milón y su ternero.",
            S("carrera_suave"), S("estirar"), S("pies bajo los hombros al agarrar", "balancea los halteres antes de saltar", "brazo largo en el lanzamiento"),
            ("Pentatlón", S("carrera_estadio", "halteres", "disco", "jabalina", "lucha_pale")),
            ("Palestra", S("lucha_pale", "pugilato", "korykos", "ternero", "carrera_estadio")));
        Add("romano", "Palestra de las termas", "Magister de la palestra", S("palestra_romana"),
            "Pesas (halteres), pelota, lucha y el palus de los legionarios; después, el baño.",
            S("carrera_suave"), S("natatio"), S("golpe desde el hombro, no desde el codo", "escudo arriba"),
            ("Termas", S("halteres", "trigon", "lucha_pale", "korykos")),
            ("Instrucción", S("palus", "lucha_pale", "halteres", "trigon")));
        Add("nilo", "Patio de lucha", "Maestro de lucha", S("patio_lucha_egipcio"),
            "Lucha (los 400 pares de Beni Hasan), esgrima de palos (tahtib) y sacos de arena: lo que se enseñaba a los soldados.",
            S("carrera_suave"), S("estirar"), S("cadera baja al agarrar", "palo en guardia alta"),
            ("Lucha y palo", S("lucha_cinturon", "tahtib", "piedra_india", "carrera_estadio")));
        Add("sumer", "Patio de lucha", "Maestro de lucha", S("patio_lucha"),
            "Lucha de cinturón (bronces de Khafajah, ~2600 a. C.), piedras de levantar y carrera.",
            S("carrera_suave"), S("estirar"), S("mano firme en el cinturón", "pies separados"),
            ("Lucha y piedras", S("lucha_cinturon", "piedra_india", "carrera_estadio", "honda")));
        Add("akhara", "Akhāṛā (patio de lucha)", "Guru (maestro del akhāṛā)", S("akhara"),
            "Lucha en el foso de tierra, dand y baithak, mazas (gada y mudgar), la nal de piedra; en la antigüedad, lucha y maza; en Tamil Nadu, además, el kalari.",
            S("carrera_suave"), S("estirar"), S("espalda larga en el dand", "la maza pasa cerca de la espalda", "talones en el suelo en el baithak"),
            ("Akhāṛā", S("dand", "baithak", "gada", "mudgar", "kushti", "malla", "piedra_india")),
            ("Fuerza", S("piedra_india", "gada", "malla", "carrera_estadio", "mudgar")),
            ("Kalari", S("meippayattu", "kaal", "kettukari", "dand")));
        Add("furusiyya", "Maydān de furusiyya", "Ustādh (maestro de furusiyya)", S("furusiyya"),
            "Arco, lanza, espada y lucha (los caballos, en la pista del maydān).",
            S("carrera_suave"), S("estirar"), S("tensa con la espalda, no con el brazo", "lanza alineada con el hombro"),
            ("Armas", S("arco", "lanza_birjas", "espada_poste", "koshti")),
            ("Lucha y carrera", S("koshti", "carrera_estadio", "arco", "lanza_birjas")));
        Add("zurkhaneh", "Zurkhaneh (casa de fuerza)", "Morshed (marca el ritmo) y miandar", S("zurkhaneh"),
            "Meel, sang, shena, kabbadeh, giros y lucha al ritmo del tambor (safávida en adelante).",
            S("charkh"), S("estirar"), S("al compás del zarb", "meel cerca del cuerpo"),
            ("Varzesh-e pahlavani", S("shena", "meel", "sang", "kabbadeh", "koshti")));
        Add("otomano", "Tekke de luchadores", "Üstat (maestro del tekke)", S("tekke_guresh"),
            "Lucha con aceite (yağlı güreş), mazas y tiro con arco.",
            S("pesrev"), S("estirar"), S("mano en el kispet", "arco tensado hasta la oreja"),
            ("Güreş", S("gures", "meel", "okculuk")),
            ("Okçuluk", S("okculuk", "gures", "meel", "carrera_estadio")));
        Add("wuguan", "Escuela de artes marciales (wuguan)", "Shifu", S("wuguan"),
            "Postura del jinete, formas, saco, candados de piedra y armas; en el Hong Kong de Ip Man, el muñeco de madera y el chi sao.",
            S("mabu"), S("estirar"), S("rodillas hacia fuera en el mabu", "el codo cae hacia dentro", "golpe por la línea central"),
            ("Básicos", S("taolu", "shisuo", "patadas", "gun")),
            ("Muñeco y saco", S("muk_yan", "chi_sao", "saco_boxeo", "shisuo", "pesas_bl")));
        Add("dojo", "Dojo", "Sensei", S("dojo"),
            "Suburi, kata, shiko, makiwara, randori, kyūdō y los candados de piedra (hojo undō).",
            S("carrera_suave"), S("estirar"), S("corta con las caderas", "kiai al final del corte"),
            ("Keiko", S("suburi", "shiko", "ishi_sashi", "randori", "arco")));
        Add("maya", "Juego de pelota", "Maestro del juego de pelota", S("juego_pelota"),
            "Golpes de cadera a la pelota de hule con el yugo, carrera y átlatl.",
            S("carrera_suave"), S("estirar"), S("la cadera, no las manos", "rodilla baja al recibir"),
            ("Pelota", S("pelota_cadera", "carrera_estadio", "atlatl")));
        Add("medieval", "Sala de armas", "Maestro de armas", S("sala_armas"),
            "El poste (pell), la quintana de la liza, la lucha y la piedra.",
            S("carrera_suave"), S("estirar"), S("golpe con el peso del cuerpo", "escudo pegado"),
            ("Armas", S("pell", "quintana", "ringen", "piedra_lanzar")));
        Add("esgrima", "Sala de esgrima", "Maestro de esgrima", S("sala_esgrima"),
            "Estocadas, paradas, asaltos y el poste.",
            S("carrera_suave"), S("estirar"), S("rodilla sobre el pie en el fondo", "la punta en línea", "vuelve a la guardia"),
            ("Esgrima", S("estocadas", "paradas", "asalto", "pell")));
        Add("prado", "Prado de los juegos (Cotswold Olimpicks, 1612–)", "El que organiza los juegos", S("prado_juegos"),
            "En el campo inglés: lucha, garrote (quarterstaff y singlestick), carreras, saltos y el mazo.",
            S("carrera_suave"), S("estirar"), S("agarre corto en el garrote", "cadera abajo en la lucha"),
            ("Juegos del prado", S("lucha_cornish", "palo_largo", "carrera_estadio", "salto_longitud", "mazo_lanzar")));
        Add("llanos", "Patio del garrote y manga de coleo", "Maestro de garrote", S("garrote_coleo"),
            "El juego de garrote, la lanza llanera, el machete y las carreras de la manga.",
            S("carrera_suave"), S("estirar"), S("garrote corto, pies ligeros", "la lanza sale con la cadera"),
            ("Garrote", S("garrote", "lanza_llanera", "machete", "coleo_carrera")));
        Add("turnplatz", "Turnplatz y ruedo de schwingen", "Turnwart (maestro de gimnasia)", S("turnplatz_schwingen"),
            "Schwingen en el serrín, la piedra de Unspunnen, la barra fija y las paralelas del Turnverein.",
            S("calistenia"), S("estirar"), S("agarre del calzón, cadera pegada", "cuerpo recto en la barra"),
            ("Schwingen y piedra", S("schwingen", "steinstossen", "reck", "barren")));
        Add("turnen", "Gimnasio (Turnhalle)", "Turnwart (maestro de gimnasia)", S("turnhalle"),
            "El Turnverein de Jahn (1811) y la gimnasia sueca de Ling: barra fija, paralelas, anillas, potro y mazas indias.",
            S("calistenia"), S("estirar"), S("cuerpo recto en la barra", "codos a 90° en las paralelas", "la maza dibuja círculos detrás"),
            ("Aparatos", S("reck", "barren", "potro", "anillas", "cuerda")),
            ("Mazas", S("mazas_indias", "reck", "barren", "potro")));
        Add("hebert", "Parcours del método natural (Hébert)", "Moniteur (monitor)", S("hebert"),
            "El método natural de Georges Hébert: marchar, correr, saltar, trepar, cuadrupedia, levantar y lanzar.",
            S("marcha_hebert"), S("estirar"), S("aterriza suave, rodillas flexionadas", "espalda recta al levantar"),
            ("Parcours", S("cuadrupedia", "trepar_hebert", "saltar_plinto", "levantar_tronco", "lanzar_balon")));
        Add("tregua", "Partido de la tregua", "El capitán", S("futbol_tregua"),
            "La Navidad de 1914 en la tierra de nadie: un balón (o una lata) y correr.",
            S("carrera_suave"), S("estirar"), S("pase con el interior del pie", "levanta la cabeza"),
            ("Partido", S("pases", "regate", "tiro_puerta", "velocidad_pista")));
        Add("sovietico", "Club deportivo", "Trener (entrenador)", S("club_deportivo"),
            "La girya, la barra fija y las normas del GTO, el saco y la pista.",
            S("carrera_suave"), S("estirar"), S("la girya sube con la cadera", "barbilla sobre la barra"),
            ("GTO", S("girya", "dominadas_gto", "flexiones", "saco_boxeo", "velocidad_pista")));
        Add("estadio", "Estadio", "Entrenador", S("estadio"),
            "Fútbol y atletismo; y la lutte traditionnelle.",
            S("carrera_suave"), S("estirar"), S("pase con el interior del pie", "brazos activos al correr"),
            ("Campo y pista", S("pases", "regate", "velocidad_pista", "tiro_puerta", "lutte")));
        // (2026-11-05) Los juegos de la tanda 3 (docs/microcosmos-entrenamiento-eras.md).
        X("wino_golpes", "Golpes con el wiño a la pali", "wiño y pali", "hombros y tronco", DrillMotion.Golpear, compound: false);
        X("palin_partido", "Partido de palín por la cancha", "wiño y pali", "piernas", DrillMotion.Correr);
        X("carrera_cerro", "Carrera de la ladera al cerro", null, "piernas", DrillMotion.Correr);
        X("honda_waraka", "Tiro con honda (waraka)", "honda de lana y piedras", "hombros", DrillMotion.Lanzar);
        X("relevo_chasqui", "Relevos de chasqui con el mensaje", "quipu y pututu", "piernas", DrillMotion.Correr);
        X("tewa_pases", "Pases con el palo y su red", "palo de nogal y pelota de piel", "hombros y tronco", DrillMotion.Lanzar, compound: false);
        X("tewa_tiro", "Tiro a la meta", "palo de nogal y pelota de piel", "hombros y tronco", DrillMotion.Lanzar, compound: false);
        X("tewa_carrera", "Carreras por el campo", null, "piernas", DrillMotion.Correr);
        X("tabla_saque", "Saque con la tabla de pelota nacional", "tabla y pelota de caucho", "hombros y tronco", DrillMotion.Golpear, compound: false);
        X("tabla_volea", "Volea con la tabla", "tabla y pelota de caucho", "hombros", DrillMotion.Golpear, compound: false);
        Add("palin", "Paliwe (cancha de palín)", "Lonko de la cancha", S("palin"),
            "El palín mapuche: dos equipos llevan la pali con el wiño de un extremo a otro de la cancha; también se juega entre comunidades.",
            S("carrera_suave"), S("estirar"), S("wiño bajo, a dos manos", "levanta la vista antes de golpear"),
            ("Palín", S("wino_golpes", "palin_partido", "carrera_estadio")));
        Add("warachikuy", "Campo del warachikuy", "Orejón maestro de los jóvenes", S("warachikuy"),
            "Las pruebas de los jóvenes incas: la carrera al cerro, la honda y los relevos de los chasquis.",
            S("carrera_suave"), S("estirar"), S("pasos cortos en la cuesta", "el relevo se da corriendo"),
            ("Pruebas", S("carrera_cerro", "honda_waraka", "relevo_chasqui")));
        Add("tewaarathon", "Campo de tewaarathon", "Corredor mayor", S("tewaarathon"),
            "El juego del Creador (lacrosse): palo de nogal con red, pelota de piel de ciervo y mucho correr.",
            S("carrera_suave"), S("estirar"), S("muñecas sueltas al recibir", "la cabeza alta al correr"),
            ("Juego", S("tewa_pases", "tewa_tiro", "tewa_carrera")));
        Add("pelota_nacional", "Cancha de pelota nacional", "Capitán de la cuadrilla", S("pelota_nacional"),
            "La pelota de tabla de Ecuador: se saca y se devuelve con una tabla pesada, en una cancha larga de tierra.",
            S("carrera_suave"), S("estirar"), S("brazada larga, con la cadera", "la vista en la pelota"),
            ("Partido", S("tabla_saque", "tabla_volea", "carrera_estadio")));
        Add("moderno", "Gimnasio", "Entrenador", S("gimnasio_moderno"),
            "Barra y discos, saco, barra fija y balón; desde 1970, máquinas de placas.",
            S("carrera_suave"), S("estirar"), S("escápulas atrás en la banca", "espalda neutra en el peso muerto"),
            ("Torso", S("banca", "remo_barra", "dominadas_gto", "saco_boxeo", "maquina")),
            ("Pierna", S("sentadilla_barra", "muerto", "comba", "maquina", "lanzar_balon")));
    }

    // ── Dónde se hace cada ejercicio: las claves de los puestos de los pueblos (CivicBuilder.TrainingKit) ─────────
    public static readonly string[] FloorKeys = { "tatami", "pista", "foso_lucha", "pista_carreras", "campo", "pista_esgrima" };
    /// <summary>Las claves de puesto (<see cref="AreaFunctionStation.station"/>) donde se hace <paramref name="d"/>, en orden de preferencia.</summary>
    public static string[] KeysOf(Drill d)
    {
        switch (d.id)
        {
            case "halteres": return S("halteres", "pesas", "piedras");
            case "piedra_india": case "steinstossen": case "ternero": case "levantar_tronco": return S("piedras", "pesas", "halteres");
            case "gada": case "mudgar": case "meel": case "kabbadeh": case "mazas_indias": case "shisuo": case "ishi_sashi": case "girya": case "sang": return S("mazas", "piedras", "pesas");
            case "mancuernas": case "pesas_bl": case "banca": case "sentadilla_barra": case "muerto": case "remo_barra": case "maquina": return S("pesas", "halteres");
            case "korykos": case "saco_boxeo": return S("saco", "muneco", "makiwara");
            case "palus": case "pell": case "espada_poste": case "muk_yan": return S("muneco", "makiwara", "saco");
            case "reck": case "dominadas_gto": case "trepar_hebert": case "cuerda": return S("barra_fija");
            case "barren": return S("paralelas");
            case "potro": case "saltar_plinto": return S("potro");
            case "anillas": return S("anillas");
            case "arco": case "okculuk": case "arco_largo": return S("diana");
            case "trigon": case "pelota_cadera": case "pases": case "regate": case "tiro_puerta": case "lanzar_balon": return S("balon", "campo", "pista");
            case "estocadas": case "paradas": case "asalto": case "garrote": case "machete": case "palo_largo": case "tahtib": case "gun": case "kettukari": case "suburi": case "lanza_birjas": case "lanza_llanera": case "quintana":
                return S("pista_esgrima", "armero", "tatami", "foso_lucha", "pista", "campo");
        }
        switch (d.motion)
        {
            case DrillMotion.Luchar: return S("foso_lucha", "tatami");
            case DrillMotion.Correr: return S("pista_carreras", "campo", "pista");
            case DrillMotion.Lanzar: return S("campo", "pista_carreras", "pista", "armero");
            default: return S("tatami", "foso_lucha", "pista", "campo", "pista_carreras");
        }
    }

    /// <summary>El lugar de entrenamiento de la CLASE <paramref name="kind"/> (<see cref="AreaFunctionSite.training"/>; null = ninguno).</summary>
    public static Tradition For(string kind, int year = 0)
    {
        if (string.IsNullOrEmpty(kind)) return null;
        foreach (Tradition t in All) if (t.styles.Contains(kind)) return t;
        return null;
    }

    /// <summary>La clase de entrenamiento de un edificio: la suya (<see cref="AreaFunctionSite.training"/>) o, si no la tiene, la de su
    /// estilo de época y lugar (<see cref="MicroStyles.TrainingKind"/>).</summary>
    public static string KindOf(AreaFunctionSite site, int year)
    {
        if (site == null) return null;
        if (!string.IsNullOrEmpty(site.training)) return site.training;
        MicroStyles.Entry e = MicroStyles.For(year, site.region);
        return e != null ? MicroStyles.TrainingKind(e, year) : null;
    }

    /// <summary>El de un edificio del pueblo (su clase y el año de la escena).</summary>
    public static Tradition ForSite(AreaFunctionSite site, int year) => For(KindOf(site, year), year);

    /// <summary>El de un microcosmos por su nombre (su fecha, su región y la clase de entrenamiento de su estilo); null si no tiene fecha.</summary>
    public static Tradition ForMicrocosmos(string display, out int year, out string kind)
    {
        year = 0; kind = null;
        var when = Eras.OfMicrocosmos(display);
        if (when == null) return null;
        year = when.Value.year;
        MicroStyles.Entry e = MicroStyles.For(year, MicroStyles.RegionOf(display));
        kind = e != null ? MicroStyles.TrainingKind(e, year) : null;
        return For(kind, year);
    }

    /// <summary>Las sesiones de la tradición en ese año y estilo (cada una con sus ejercicios de época; las de menos de 3, fuera).</summary>
    public static List<(string name, List<Drill> drills)> Sessions(Tradition t, int year, string style)
    {
        var l = new List<(string, List<Drill>)>();
        foreach (var s in t.sessions)
        {
            List<Drill> d = s.drills.Select(D).Where(x => x != null && x.In(year, style)).ToList();
            if (d.Count >= 3) l.Add((s.name, d));
        }
        return l;
    }

    // ── Planes de los insectos-humanos (el motor del gimnasio del santuario) ────────────────────
    public class Progress { public float load; public int reps, sets; }

    public class Plan
    {
        public string who; public Tradition tradition; public int year; public string style;
        public TrainingGoal goal; public int[] days; public int week;
        public List<(string name, List<Drill> drills)> sessions;
        public readonly Dictionary<string, Progress> progress = new Dictionary<string, Progress>();
        public readonly HashSet<string> missed = new HashSet<string>();
        public bool Trains(int day) => days.Contains(((day % 7) + 7) % 7);
        public override string ToString() => $"{who}: {tradition.place} · {goal} · {GymProgramming.Days(days)} · {string.Join(" / ", sessions.Select(s => s.name))}";
    }

    public class Prescription
    {
        public Drill drill; public int sets, reps; public float load, restSec, rpe, minutes; public string part = "principal";
        public bool Timed => drill.timed;
        public string LoadText => Timed ? $"{minutes:0} min" : drill.Loaded ? $"{load:0.##} {drill.unit}" : "peso del cuerpo";
        public override string ToString() => Timed ? $"{part}: {drill.name} {minutes:0} min" : $"{drill.name} {sets}×{reps} · {LoadText}{(drill.equipment != null ? $" ({drill.equipment})" : "")} · descanso {restSec:0} s";
    }

    public class Routine { public string who, name; public int day, week; public bool deload, recovery; public List<Prescription> items = new List<Prescription>(); public IEnumerable<Prescription> Main => items.Where(i => !i.Timed); }

    static readonly Dictionary<string, Plan> _plans = new Dictionary<string, Plan>();
    public static void ResetPlans() => _plans.Clear();

    static int Hash(string s) { unchecked { int h = 23; foreach (char c in s ?? "") h = h * 31 + c; return h & 0x7fffffff; } }

    /// <summary>El plan de <paramref name="who"/> en esa tradición, año y estilo. El papel da el objetivo (soldados y guardias,
    /// fuerza; el resto, por su nombre: hipertrofia, resistencia o fuerza); 3 o 4 días por semana.</summary>
    public static Plan PlanFor(string who, Tradition t, int year, string style, string role = "", int day = 0)
    {
        if (t == null || string.IsNullOrEmpty(who)) return null;
        string key = $"{who}|{t.id}|{year}";
        if (_plans.TryGetValue(key, out Plan p)) return p;
        int h = Hash(who);
        TrainingGoal g = role == "soldado" || role == "guardia" || role == "guerrero" ? TrainingGoal.Fuerza : new[] { TrainingGoal.Hipertrofia, TrainingGoal.Resistencia, TrainingGoal.Fuerza }[h % 3];
        int[][] patterns = { new[] { 0, 2, 4 }, new[] { 1, 3, 5 }, new[] { 0, 2, 4, 6 }, new[] { 1, 3, 5, 6 } };
        p = new Plan { who = who, tradition = t, year = year, style = style, goal = g, days = patterns[(h / 3) % patterns.Length], week = Mathf.Max(0, day / 7), sessions = Sessions(t, year, style) };
        foreach (var s in p.sessions) foreach (Drill d in s.drills) Init(p, d);
        _plans[key] = p;
        return p;
    }

    static Progress Init(Plan p, Drill d)
    {
        if (p.progress.TryGetValue(d.id, out Progress pr)) return pr;
        GymProgramming.GoalSpec s = GymProgramming.Spec(p.goal, d.compound);
        float k = p.goal == TrainingGoal.Fuerza ? 1.25f : p.goal == TrainingGoal.Resistencia ? 0.7f : 1f;
        pr = new Progress { reps = s.repLo, sets = d.compound ? s.sets : s.setsAccessory, load = d.Loaded ? Mathf.Max(d.step > 0f ? d.step : 0.5f, Mathf.Round(d.baseLoad * k / Mathf.Max(0.5f, d.step)) * Mathf.Max(0.5f, d.step)) : 0f };
        p.progress[d.id] = pr;
        return pr;
    }

    /// <summary>La rutina de un día: el calentamiento de la tradición, los ejercicios de la sesión que toca y la vuelta a la calma
    /// (la descarga cada 4.ª semana, como en el santuario). Los días sin entrenamiento en su plan: calentar y estirar.</summary>
    public static Routine Prescribe(Plan p, int day)
    {
        SyncWeek(p, day);
        var r = new Routine { who = p.who, day = day, week = p.week, deload = GymProgramming.IsDeload(p.week) };
        foreach (string w in p.tradition.warmup) { Drill d = D(w); if (d != null) r.items.Add(new Prescription { drill = d, minutes = d.minutes, part = "calentamiento" }); }
        int wd = ((day % 7) + 7) % 7, idx = System.Array.IndexOf(p.days, wd);
        if (idx < 0 || p.sessions.Count == 0) { r.recovery = true; r.name = "Recuperación"; }
        else
        {
            var s = p.sessions[((day / 7) * p.days.Length + idx) % p.sessions.Count];
            r.name = s.name + (r.deload ? " (descarga)" : "");
            foreach (Drill d in s.drills)
            {
                Progress pr = Init(p, d); GymProgramming.GoalSpec sp = GymProgramming.Spec(p.goal, d.compound);
                var x = new Prescription { drill = d, sets = pr.sets, reps = pr.reps, load = pr.load, restSec = sp.restSec, rpe = sp.rpe, minutes = d.minutes };
                if (r.deload) { x.sets = Mathf.Max(1, Mathf.CeilToInt(x.sets * 0.5f)); if (d.Loaded) x.load = Mathf.Max(d.step, x.load - d.step); x.rpe -= 2f; }
                r.items.Add(x);
            }
        }
        foreach (string c in p.tradition.cooldown) { Drill d = D(c); if (d != null) r.items.Add(new Prescription { drill = d, minutes = d.minutes, part = "vuelta a la calma" }); }
        return r;
    }

    public static void SyncWeek(Plan p, int day) { int target = Mathf.Max(0, day / 7), guard = 0; while (p.week < target && guard++ < 520) AdvanceWeek(p); }

    /// <summary>Progresión doble (como el santuario): +1 repetición (+2 en resistencia) hasta arriba del rango; arriba, +1 paso de
    /// carga (un halter más pesado, la girya siguiente, un ternero más grande) y vuelta abajo; con el peso del cuerpo, +1 rep.</summary>
    public static void AdvanceWeek(Plan p)
    {
        bool wasDeload = GymProgramming.IsDeload(p.week);
        p.week++;
        if (!wasDeload)
            foreach (var kv in p.progress)
            {
                if (p.missed.Contains(kv.Key)) continue;
                Drill d = D(kv.Key); Progress pr = kv.Value; GymProgramming.GoalSpec s = GymProgramming.Spec(p.goal, d.compound);
                if (!d.Loaded) { pr.reps = Mathf.Min(30, pr.reps + 1); continue; }
                if (pr.reps < s.repHi) pr.reps = Mathf.Min(s.repHi, pr.reps + (p.goal == TrainingGoal.Resistencia ? 2 : 1));
                else { pr.load += d.step; pr.reps = s.repLo; }
            }
        p.missed.Clear();
    }

    public static float Tonnage(Plan p) => p.progress.Values.Sum(x => x.sets * x.reps * Mathf.Max(1f, x.load));
}
