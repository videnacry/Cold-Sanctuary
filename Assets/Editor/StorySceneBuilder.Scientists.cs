using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// MICROCOSMOS DE CIENTÍFICOS E HISTÓRICOS (docs/microcosmos-cientificos.md; Berón 2026-11-01). Doce historias, cada una en
/// el PUEBLO donde ocurrió (CivicBuilder.Towns: el pueblo llena el piso entero) y sobre el área del santuario que mejor la
/// cuenta: Pitágoras (Gimnasio), Heródoto (Huerto), al-Juarismi y Tesla (Construcción), Newton y Einstein (Planta Atómica),
/// Jenner (Biología), Bolívar (Cocina), Nietzsche (Sala de Yoga), los Curie y Patterson (Química) y Ananenko y su escuadrón
/// (Central Nuclear). En el claro de la historia (radio ~24 m; entrada y portal en z −14…−20) están los sitios de la misión;
/// la gente son insectos-humanos (InsectCast: el insecto de cada uno, con su porqué) y el elenco de la enfermería renace con
/// su nombre. Al terminar, la HistoryCard cuenta lo que pasó de verdad, con fuentes.
/// </summary>
public static partial class StorySceneBuilder
{
    static readonly string[] ScientistScenes =
    {
        "Microcosmos_Pitagoras", "Microcosmos_Herodoto", "Microcosmos_AlJuarismi", "Microcosmos_Newton", "Microcosmos_Jenner", "Microcosmos_Bolivar",
        "Microcosmos_Nietzsche", "Microcosmos_Curie", "Microcosmos_Tesla", "Microcosmos_Einstein", "Microcosmos_Patterson", "Microcosmos_Chernobil",
    };

    /// <summary>Genera SOLO los doce microcosmos de científicos e históricos (con la escena principal abierta: sus pisos son los
    /// que se agrandan).</summary>
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias: científicos e históricos (12)")]
    public static void BuildScientists() { foreach (string sc in ScientistScenes) { try { BuildOne(sc); } catch (System.Exception e) { Debug.LogWarning($"[Historias] {sc}: {e.Message}"); } } }

    // Un personaje histórico: un insecto-humano de AreaMicrocosmos.Folk (su insecto de InsectCast o <paramref name="species"/>,
    // erguido a su estatura, con cara, pelo, linaje y rótulo) con su SEXO real, de cara a la entrada (−z).
    static GameObject Who(Transform t, string name, Vector3 pos, bool female, float h = 1.75f, string species = null)
    {
        GameObject g = AreaMicrocosmos.Folk(t, name, pos, h, 1f, species, null, female);
        g.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        return g;
    }
    static GameObject Modern(Transform t, string name, Vector3 pos, bool female, Color coat, CubeInsect.Hat hat = CubeInsect.Hat.None, float h = 1.75f, string species = null)
    { GameObject g = Who(t, name, pos, female, h, species); CubeInsect.Wear1940s(g, coat, hat); return g; }
    static GameObject Ancient(Transform t, string name, Vector3 pos, bool female, Color robe, bool beard = false, float h = 1.75f, string species = null)
    { GameObject g = Who(t, name, pos, female, h, species); CubeInsect.WearGreek(g, robe, false, beard); return g; }

    static Material M(string name, float r, float g, float b) => AreaMicrocosmos.Mat("Hist_" + name, new Color(r, g, b));
    static Transform Prop(Transform parent, string name, Vector3 pos, Vector3 size, Material m, bool solid = true) => AreaMicrocosmos.Box(parent, name, pos, size, m, solid);
    static List<Transform> Items(Transform t, Material m, Vector3 size, params (string label, Vector3 pos)[] items)
    {
        var l = new List<Transform>();
        foreach (var (label, pos) in items) { Transform b = AreaMicrocosmos.Box(t, label, pos, size, m, false); Label(b, label, 0.6f); l.Add(b); }
        return l;
    }

    // ══ PITÁGORAS · Crotona, ~530 a. C. · Gimnasio ════════════════════════════════════════════════════════════════

    static int Pythagoras(Transform t)
    {
        Material bronze = M("Bronce", 0.7f, 0.5f, 0.25f), wood = M("Madera", 0.55f, 0.4f, 0.25f), sand = M("Arena", 0.85f, 0.75f, 0.55f), rope = M("Cuerda", 0.75f, 0.65f, 0.45f);
        Transform court = Spot(t, "Patio del homakoeion", new Vector3(0f, 0f, 14f), "Patio de la escuela (los cinco años de silencio)", 2.5f);
        Prop(court, "Banco de piedra", new Vector3(-3f, 0.25f, 2f), new Vector3(3f, 0.5f, 0.6f), M("Piedra", 0.85f, 0.8f, 0.68f));
        Transform forge = Spot(t, "Herrería", new Vector3(-16f, 0f, 6f), "La herrería (la leyenda de los martillos)", 2.5f);
        Prop(forge, "Yunque", new Vector3(0f, 0.4f, 0f), new Vector3(0.9f, 0.8f, 0.5f), M("Hierro", 0.3f, 0.3f, 0.32f));
        var hammers = Items(t, bronze, new Vector3(0.5f, 0.15f, 0.2f), ("Martillo de 12", new Vector3(-18f, 0.1f, 3f)), ("Martillo de 9", new Vector3(-16.5f, 0.1f, 3f)), ("Martillo de 8", new Vector3(-15f, 0.1f, 3f)), ("Martillo de 6", new Vector3(-13.5f, 0.1f, 3f)));
        Transform mono = Spot(t, "Monocordio", new Vector3(14f, 0f, 8f), "Monocordio (una cuerda y un puente móvil)", 2f);
        Prop(mono, "Caja", new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.2f, 0.4f), wood);
        Prop(mono, "Cuerda", new Vector3(0f, 0.57f, 0f), new Vector3(2.3f, 0.02f, 0.02f), bronze, false);
        Prop(mono, "Puente (2/3)", new Vector3(0.38f, 0.6f, 0f), new Vector3(0.05f, 0.08f, 0.3f), wood, false);
        Prop(mono, "Mesa", new Vector3(0f, 0.17f, 0f), new Vector3(2.6f, 0.34f, 0.8f), wood);
        // La playa: un triángulo de lados 3, 4 y 5 (aquí ×2 m) con una cuerda de 12 nudos: el ángulo recto.
        Transform beach = Spot(t, "Playa del Jónico", new Vector3(12f, 0f, -6f), "Playa: la cuerda de 12 nudos", 2f);
        Prop(beach, "Arena", new Vector3(3f, 0.02f, 3f), new Vector3(12f, 0.04f, 10f), sand, false);
        var stakes = Items(t, wood, new Vector3(0.15f, 0.9f, 0.15f), ("Estaca A (esquina recta)", new Vector3(10f, 0.45f, -8f)), ("Estaca B (a 3 tramos)", new Vector3(16f, 0.45f, -8f)), ("Estaca C (a 4 tramos)", new Vector3(10f, 0.45f, 0f)));
        Prop(t, "Cuerda AB", new Vector3(13f, 0.05f, -8f), new Vector3(6f, 0.04f, 0.04f), rope, false);
        Prop(t, "Cuerda AC", new Vector3(10f, 0.05f, -4f), new Vector3(0.04f, 0.04f, 8f), rope, false);
        Transform palestra = Spot(t, "Arena de Milón", new Vector3(-12f, 0f, 18f), "Milón y el ternero (la palestra)", 2.5f);
        Prop(palestra, "Arena de lucha", new Vector3(0f, 0.02f, 0f), new Vector3(6f, 0.04f, 6f), sand, false);

        Color white = new Color(0.95f, 0.93f, 0.88f);
        GameObject pyth = Ancient(t, "Pitágoras", new Vector3(1.5f, 0f, 13f), false, white, beard: true);
        GameObject theano = Ancient(t, "Teano (Medea)", new Vector3(-2f, 0f, 12f), true, new Color(0.6f, 0.35f, 0.4f));
        GameObject milo = Ancient(t, "Milón de Crotona (Atlas)", new Vector3(-12f, 0f, 16f), false, new Color(0.7f, 0.55f, 0.35f), beard: true, h: 1.9f);
        GameObject smith = Ancient(t, "Herrero (Momo)", new Vector3(-15f, 0f, 8f), false, new Color(0.45f, 0.35f, 0.3f));
        GameObject pupil = Ancient(t, "Acusmático (Sakshi)", new Vector3(3f, 0f, 16f), true, white); Routine(pupil, court, mono, beach);
        int n = 5;

        Mission(t, "Pitágoras en Crotona: el número y la música", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con Pitágoras en el patio de la escuela", pyth.transform, "Pitágoras", "Llegué de Samos huyendo del tirano Polícrates. Aquí, en Crotona, vivimos juntos, compartimos lo que tenemos y estudiamos el número: creo que en él está el orden del mundo."),
                Hold("Los nuevos guardan silencio: quédate callado en el patio", court, 10f, "Teano", "Los que empiezan solo escuchan (son los «acusmáticos»). Callar enseña a oír."),
                Visit("En la herrería: escucha los cuatro martillos", hammers, "Herrero", "Los de 12 y 6 suenan a octava; los de 12 y 8, a quinta; los de 12 y 9, a cuarta. Eso dice la leyenda… aunque con martillos no pasa así: pasa con cuerdas."),
                Hold("En el monocordio: pon el puente a la mitad, a 2/3 y a 3/4 de la cuerda", mono, 10f, "Pitágoras", "Media cuerda suena una octava más aguda (2:1); dos tercios, una quinta (3:2); tres cuartos, una cuarta (4:3). La armonía es una razón entre números pequeños."),
                Visit("En la playa: tensa la cuerda de 12 nudos entre las tres estacas (3, 4 y 5 tramos)", stakes, "Acusmático", "¡Sale un ángulo recto! 3 × 3 + 4 × 4 = 5 × 5. Los babilonios ya lo usaban mil años antes."),
                Talk("Pregunta a Pitágoras por qué es así", pyth.transform, "Pitágoras", "En todo triángulo con un ángulo recto, el cuadrado del lado mayor es la suma de los cuadrados de los otros dos. Saberlo no basta: hay que demostrarlo para todos los triángulos."),
                Talk("En la palestra, habla con Milón", milo.transform, "Milón", "Cada día cargo al mismo ternero sobre los hombros. Crece un poco y yo con él: hoy es un toro. Así se entrena: poco a poco, cada día un poco más."),
                Hold("Carga con Milón: aguanta firme en la arena", palestra, 8f, "Milón", "La fuerza también es un número que crece despacio. Y un día, cuando cedió el techo de la sala, sostuve la viga hasta que salieron todos."),
            },
            "Crotona (sur de Italia), hacia el 530 a. C., sobre el gimnasio del santuario a escala de hormiga. Pitágoras ha fundado aquí una comunidad que estudia el número, la música y el cuerpo; Milón, el luchador más famoso de Grecia, entrena con ellos. Recorre la ciudad: la escuela, la herrería, el monocordio, la playa y la palestra. E para hablar y tocar; en los pasos de práctica, quédate quieto.",
            new[]
            {
                "Pitágoras de Samos (c. 570 – c. 495 a. C.) dejó Samos hacia el 530 a. C. y fundó en Crotona una comunidad con normas propias: bienes en común, una dieta con restricciones (la más famosa: no comer habas), silencio para los recién llegados y estudio del número. No dejó nada escrito; casi todo lo que sabemos viene de autores muy posteriores (Aristóteles, Porfirio, Jámblico), y es difícil separar al Pitágoras real de la leyenda.",
                "La música de las razones (2:1 la octava, 3:2 la quinta, 4:3 la cuarta) es cierta para las cuerdas: se comprueba con un monocordio. La leyenda de los martillos de la herrería (Nicómaco, s. II d. C.) es falsa como física: el tono de un martillo no es proporcional a su peso; Vincenzo Galilei, el padre de Galileo, lo demostró en el s. XVI.",
                "El «teorema de Pitágoras» se conocía mucho antes: la tablilla babilónica Plimpton 322 (~1800 a. C.) lista ternas como 3-4-5, y los Śulbasūtra de la India lo usan para construir altares. No sabemos si Pitágoras lo demostró; la primera demostración que conservamos es la de Euclides (Elementos I.47, ~300 a. C.).",
                "Milón de Crotona ganó la lucha en seis Juegos Olímpicos (desde el 540 a. C.) y fue, según la tradición, discípulo de Pitágoras. La historia del ternero que cargaba cada día hasta que fue toro (Quintiliano, Institutio I.9) es la imagen más antigua de la sobrecarga progresiva del entrenamiento. Hacia el 450 a. C. una revuelta quemó la casa de Milón, donde se reunían los pitagóricos (Polibio II.39).",
                "La viga que sostiene Milón es una leyenda antigua que recoge Estrabón (VI, 1, 12).",
            },
            new[]
            {
                "Huffman, C. (2018). «Pythagoras». Stanford Encyclopedia of Philosophy (plato.stanford.edu/entries/pythagoras).",
                "Burkert, W. (1972). Lore and Science in Ancient Pythagoreanism. Harvard University Press.",
                "Robson, E. (2001). «Neither Sherlock Holmes nor Babylon: a reassessment of Plimpton 322». Historia Mathematica 28:167–206.",
                "Nicómaco de Gerasa, Enchiridion harmonices, cap. 6 (la leyenda de los martillos); Quintiliano, Institutio oratoria I.9; Polibio, Historias II.39.",
            },
            "Pitágoras", "No me creas a mí: mide la cuerda y escucha.");
        return n;
    }

    // ══ HERÓDOTO · Menfis, ~450 a. C. · Huerto ══════════════════════════════════════════════════════════════════════

    static int Herodotus(Transform t)
    {
        Material mud = M("Limo", 0.3f, 0.25f, 0.18f), water = M("Agua_Nilo", 0.3f, 0.45f, 0.5f), papyrus = M("Papiro", 0.9f, 0.82f, 0.6f), stone = M("Caliza", 0.9f, 0.85f, 0.7f), seed = M("Grano", 0.8f, 0.68f, 0.35f);
        Transform port = Spot(t, "Muelle", new Vector3(-16f, 0f, 4f), "Muelle del Nilo", 2.5f);
        Prop(port, "Agua del Nilo", new Vector3(-4f, 0.02f, 6f), new Vector3(6f, 0.04f, 26f), water, false);
        Prop(port, "Barca de papiro", new Vector3(-3f, 0.25f, 0f), new Vector3(1.2f, 0.5f, 4f), papyrus, false);
        Transform temple = Spot(t, "Patio del templo de Ptah", new Vector3(10f, 0f, 16f), "Patio del templo de Ptah", 3f);
        for (int k = 0; k < 4; k++) Prop(temple, "Columna papiriforme", new Vector3(-4.5f + k * 3f, 2f, 3f), new Vector3(0.6f, 4f, 0.6f), stone);
        Transform fields = Spot(t, "Campos de la crecida", new Vector3(-4f, 0f, 20f), "Campos cubiertos de limo (el don del río)", 2f);
        Prop(fields, "Limo nuevo", new Vector3(0f, 0.02f, 0f), new Vector3(10f, 0.04f, 6f), mud, false);
        var sacks = Items(t, seed, new Vector3(0.6f, 0.5f, 0.6f), ("Saco de trigo", new Vector3(-12f, 0.25f, -4f)), ("Saco de trigo", new Vector3(-10f, 0.25f, -6f)), ("Saco de lino", new Vector3(-8f, 0.25f, -4f)));
        Transform pyramids = Spot(t, "Mirador de las pirámides", new Vector3(18f, 0f, -2f), "Las pirámides de Guiza, a lo lejos", 3f);
        for (int k = 0; k < 3; k++) Prop(pyramids, "Pirámide (lejana)", new Vector3(2f + k * 2.5f, 0.6f - k * 0.1f, 4f), new Vector3(1.6f - k * 0.3f, 1.2f - k * 0.2f, 1.6f - k * 0.3f), stone);

        Color linen = new Color(0.95f, 0.93f, 0.86f), chiton = new Color(0.55f, 0.6f, 0.75f);
        GameObject hero = Ancient(t, "Heródoto", new Vector3(-12f, 0f, 2f), false, chiton, beard: true);
        GameObject priest = Ancient(t, "Sacerdote de Ptah (Shanidar)", new Vector3(10f, 0f, 13f), false, linen, h: 1.65f);
        GameObject boat = Ancient(t, "Barquero (Héspero)", new Vector3(-15f, 0f, 8f), false, new Color(0.7f, 0.6f, 0.45f));
        GameObject farmer = Ancient(t, "Campesina (Ruth)", new Vector3(-2f, 0f, 18f), true, linen);
        GameObject guide = Ancient(t, "Intérprete (Momo)", new Vector3(16f, 0f, 0f), false, new Color(0.6f, 0.5f, 0.35f)); Routine(guide, temple, pyramids, port);
        var informants = new List<Transform> { priest.transform, boat.transform, farmer.transform };
        int n = 5;

        Mission(t, "Heródoto en Egipto: «el don del río»", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("En el muelle, habla con Heródoto", hero.transform, "Heródoto", "Soy de Halicarnaso. Escribo esta investigación para que el tiempo no borre lo que hicieron los hombres. He venido a Egipto a ver y a preguntar."),
                Talk("Pregunta a los sacerdotes de Ptah por los primeros hombres", priest.transform, "Sacerdote", "El faraón Psamético crió a dos niños sin que nadie les hablara, para ver qué lengua decían primero. Dijeron «bekos»: pan en frigio. Desde entonces aceptamos que los frigios son más antiguos."),
                Visit("Historíē es investigar: pregunta al sacerdote, al barquero y a la campesina por qué crece el Nilo", informants, "Heródoto", "Unos dicen que lo empujan los vientos, otros que viene del Océano, otros que es nieve que se funde. Ninguna me convence. Anoto las tres y digo cuál creo: así el lector puede juzgar."),
                Hold("En los campos: mira el limo nuevo que ha dejado la crecida", fields, 8f, "Campesina", "Cada verano el río sube, lo cubre todo y se va dejando esta tierra negra. Por eso decimos que Egipto es un don del río. A mi hijo se lo llevó una crecida. El griego me ha preguntado cómo se llamaba y lo ha escrito en su rollo. Nadie había escrito nunca el nombre de un campesino."),
                Collect("Lleva el grano a los campos para la siembra", sacks, fields, "Campesina", "Aquí no hace falta arar como en Grecia: se siembra en el barro y se deja que los animales pisen la semilla."),
                Reach("Ve al mirador de las pirámides con el intérprete", pyramids, "Intérprete", "Esa es la del rey Keops. Me han dicho que la hicieron cien mil hombres en turnos de tres meses, durante veinte años."),
                Talk("Pregunta a Heródoto si se lo cree", hero.transform, "Heródoto", "Cuento lo que me dicen; no estoy obligado a creerlo todo, pero sí a contarlo. Lo que vi con mis ojos lo digo; lo que oí, lo digo como oído."),
            },
            "Menfis (Egipto), hacia el 450 a. C., en verano, cuando empieza la crecida del Nilo, sobre el huerto del santuario a escala de hormiga. Heródoto, el primer historiador, viaja preguntando: a los sacerdotes, a los barqueros, a los campesinos. Investiga con él por qué el Nilo crece y fertiliza los campos. E para hablar y recoger; para mirar, quédate quieto.",
            new[]
            {
                "Heródoto de Halicarnaso (c. 484 – c. 425 a. C.) escribió las «Historias» (historíē = investigación), en nueve libros, sobre las guerras entre griegos y persas y los pueblos que las rodeaban. Cicerón lo llamó «padre de la historia»; otros, «padre de las mentiras», porque recoge también lo que no comprobó. Él mismo separa lo que vio, lo que oyó y lo que cree.",
                "El libro II es Egipto, que visitó hacia los años 450. Allí dice que Egipto es «don del río» (II.5) y discute tres explicaciones de la crecida del Nilo (II.20–27): los vientos etesios, el Océano y la nieve que se funde; rechaza las tres y propone otra (el sol). La causa real son las lluvias del monzón de verano en las montañas de Etiopía, que alimentan el Nilo Azul.",
                "Cuenta el experimento del faraón Psamético para saber cuál era la lengua más antigua (II.2) y que en Egipto se sembraba en el barro y se dejaba a los cerdos pisar la semilla (II.14). Sobre las pirámides (II.124) dice que trabajaron 100 000 hombres por turnos de tres meses durante 20 años. La arqueología (el poblado de los trabajadores de Guiza) muestra cuadrillas de trabajadores pagados y alimentados, de unos miles a la vez, no esclavos.",
                "Su método, aunque imperfecto, es el comienzo de la historia y la geografía: viajar, preguntar a varias fuentes, comparar, decir qué parece verdad y por qué.",
                "La campesina y el nombre de su hijo son una recreación: no constan. Heródoto dijo que escribía para que no se perdiera la memoria de lo que hicieron las personas, y esta escena lo lleva a quien nadie anotaba.",
            },
            new[]
            {
                "Heródoto, Historias, libro II (trad. C. Schrader, Gredos, 1977).",
                "Lloyd, A. B. (1975–1988). Herodotus Book II: Introduction and Commentary. Brill.",
                "Lehner, M. y Hawass, Z. (2017). Giza and the Pyramids. University of Chicago Press.",
                "Said, R. (1993). The River Nile: Geology, Hydrology and Utilization. Pergamon.",
            },
            "Heródoto", "Pregunta a más de uno, y di de quién lo has oído.");
        return n;
    }

    // ══ AL-JUARISMI · Bagdad, ~820 · Construcción ═══════════════════════════════════════════════════════════════════

    static int AlKhwarizmi(Transform t)
    {
        Material brick = M("Ladrillo", 0.78f, 0.6f, 0.42f), paper = M("Papel", 0.93f, 0.9f, 0.8f), water = M("Agua_canal", 0.3f, 0.5f, 0.6f), wood = M("Madera", 0.55f, 0.4f, 0.25f), coin = M("Oro", 0.9f, 0.75f, 0.3f);
        Transform hikma = Spot(t, "Sala de la Casa de la Sabiduría", new Vector3(0f, 0f, 16f), "Casa de la Sabiduría (Bayt al-Ḥikma)", 3f);
        Prop(hikma, "Mesa baja", new Vector3(0f, 0.2f, 0f), new Vector3(2f, 0.4f, 1f), wood);
        Transform square = Spot(t, "El cuadrado de al-jabr", new Vector3(12f, 0f, 14f), "x² + 10x = 39 (completar el cuadrado)", 2f);
        Prop(square, "Cuadrado x²", new Vector3(0f, 0.03f, 0f), new Vector3(3f, 0.06f, 3f), paper, false);
        foreach (var (x, z) in new[] { (2.1f, 0f), (-2.1f, 0f), (0f, 2.1f), (0f, -2.1f) }) Prop(square, "Rectángulo 2,5x", new Vector3(x, 0.03f, z), new Vector3(x == 0f ? 3f : 1.2f, 0.06f, z == 0f ? 3f : 1.2f), brick, false);
        Transform court = Spot(t, "Tribunal del cadí", new Vector3(-14f, 0f, 12f), "Tribunal del cadí: una herencia", 2.5f);
        var dinars = Items(t, coin, new Vector3(0.3f, 0.1f, 0.3f), ("20 dinares (hija)", new Vector3(-16f, 0.05f, 8f)), ("40 dinares (hijo mayor)", new Vector3(-14f, 0.05f, 8f)), ("40 dinares (hijo menor)", new Vector3(-12f, 0.05f, 8f)));
        Transform souk = Spot(t, "Cambista del zoco", new Vector3(14f, 0f, -4f), "El cambista: los números indios y el cero", 2.5f);
        Prop(souk, "Balanza", new Vector3(0f, 0.5f, 0f), new Vector3(1.2f, 1f, 0.4f), coin);
        Transform canal = Spot(t, "Canal en obras", new Vector3(-14f, 0f, -6f), "El canal (medir la tierra)", 2.5f);
        Prop(canal, "Agua del canal", new Vector3(-3f, 0.02f, 0f), new Vector3(2f, 0.04f, 14f), water, false);
        var stakes = Items(t, wood, new Vector3(0.15f, 0.9f, 0.15f), ("Estaca 1", new Vector3(-12f, 0.45f, -10f)), ("Estaca 2", new Vector3(-12f, 0.45f, -4f)), ("Estaca 3", new Vector3(-12f, 0.45f, 2f)));

        Color robe = new Color(0.3f, 0.35f, 0.5f), white = new Color(0.94f, 0.92f, 0.86f);
        GameObject kh = Ancient(t, "Muḥammad ibn Mūsā al-Juarismi", new Vector3(1.5f, 0f, 14f), false, robe, beard: true);
        CubeInsect.WearHat(kh, CubeInsect.Hat.Turban);
        GameObject qadi = Ancient(t, "El cadí (Shanidar)", new Vector3(-14f, 0f, 14f), false, white, beard: true); CubeInsect.WearHat(qadi, CubeInsect.Hat.Turban);
        GameObject heir = Ancient(t, "La heredera (Sakshi)", new Vector3(-17f, 0f, 11f), true, new Color(0.6f, 0.3f, 0.35f));
        GameObject changer = Ancient(t, "El cambista (Atlas)", new Vector3(14f, 0f, -2f), false, new Color(0.55f, 0.45f, 0.3f)); CubeInsect.WearHat(changer, CubeInsect.Hat.Turban);
        GameObject digger = Ancient(t, "Capataz del canal (Momo)", new Vector3(-16f, 0f, -8f), false, new Color(0.6f, 0.5f, 0.38f)); Routine(digger, canal, souk);
        int n = 5;

        Mission(t, "al-Juarismi: el álgebra y los números indios", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con al-Juarismi en la Casa de la Sabiduría", kh.transform, "al-Juarismi", "El califa al-Mamún me ha pedido un libro breve de cálculo para lo que la gente necesita a diario: herencias, testamentos, repartos, comercio, medir la tierra y cavar canales."),
                Talk("En el tribunal: escucha al cadí", qadi.transform, "El cadí", "Un hombre deja 100 dinares a sus dos hijos y su hija. Por la ley, cada hijo recibe el doble que la hija. ¿Cuánto es cada parte?"),
                Collect("Reparte la herencia: la hija es «la cosa» (x); cada hijo, 2x. x + 2x + 2x = 100", dinars, court, "La heredera", "5x = 100, así que x = 20: veinte para mí y cuarenta para cada hermano. ¡Y suma cien! Mis hermanos decían que no me tocaba nada. Él me lo ha enseñado gratis, en la arena, para que nadie me engañe nunca."),
                Hold("Al-jabr y al-muqābala: completa el cuadrado de x² + 10x = 39", square, 12f, "al-Juarismi", "Parte las 10x en cuatro rectángulos de 2,5x alrededor del cuadrado. Faltan cuatro esquinas de 2,5 × 2,5: añade 25 a los dos lados (eso es al-jabr, «restaurar»). Queda un cuadrado de 64: su lado es 8, y x = 8 − 5 = 3."),
                Talk("En el zoco: habla con el cambista", changer.transform, "El cambista", "Con los números de la India cuento en un momento: diez cifras y un círculo, ṣifr, para el vacío. Con él, el 105 no se confunde con el 15."),
                Visit("Mide el canal con el capataz: comprueba las tres estacas", stakes, "Capataz", "Cada tramo, seis codos de largo; con tu cuenta sé cuánta tierra sacar y cuántos jornales pagar."),
                Talk("Vuelve con al-Juarismi", kh.transform, "al-Juarismi", "No inventé los números ni las ecuaciones: los reuní, los ordené y los expliqué paso a paso, para que cualquiera pueda seguir la receta."),
            },
            "Bagdad, la Ciudad Redonda del califato abasí, hacia el año 820, sobre la construcción del santuario a escala de hormiga. En la Casa de la Sabiduría se traducen libros griegos, persas e indios; al-Juarismi escribe allí el libro que dará nombre al álgebra. Resuelve con él una herencia, completa un cuadrado, cuenta con el cero y mide un canal. E para hablar y recoger; para pensar, quédate quieto.",
            new[]
            {
                "Muḥammad ibn Mūsā al-Juarismi (c. 780 – c. 850), originario de Corasmia (Asia central), trabajó en Bagdad bajo el califa al-Mamún, en la Casa de la Sabiduría, donde se traducían y estudiaban obras griegas, persas e indias.",
                "Hacia el 820 escribió «al-Kitāb al-mukhtaṣar fī ḥisāb al-jabr wa-l-muqābala» (el libro breve del cálculo por restauración y comparación). De «al-jabr» viene la palabra álgebra. Clasifica las ecuaciones de primer y segundo grado en seis tipos y las resuelve con recetas en palabras (sin símbolos) y con figuras geométricas, como el cuadrado completado de x² + 10x = 39. Más de la mitad del libro trata de herencias y testamentos.",
                "Otro libro suyo explicaba el cálculo con los numerales indios (con el cero, que Brahmagupta ya usaba en 628). Su traducción latina empezaba «Dixit Algoritmi…» («dijo al-Juarismi»): de su nombre viene la palabra algoritmo. Escribió también tablas astronómicas (zīj) y una geografía.",
                "Bagdad, fundada en 762 por al-Mansur como una ciudad redonda con cuatro puertas, tenía hacia el 794 uno de los primeros molinos de papel del mundo islámico: el papel abarató los libros y ayudó al auge de las traducciones.",
                "Es una recreación que enseñara gratis a una heredera: no consta. Su libro sí se escribió, según su prólogo, para los repartos y herencias de la gente.",
            },
            new[]
            {
                "Rashed, R. (2009). Al-Khwārizmī: The Beginnings of Algebra. Saqi Books.",
                "O'Connor, J. J. y Robertson, E. F. «Abu Ja'far Muhammad ibn Musa Al-Khwarizmi». MacTutor History of Mathematics (Univ. de St Andrews).",
                "Gutas, D. (1998). Greek Thought, Arabic Culture. Routledge.",
                "Bloom, J. M. (2001). Paper Before Print. Yale University Press.",
            },
            "al-Juarismi", "Una receta clara vale más que mil cuentas sueltas.");
        return n;
    }

    // ══ NEWTON · Woolsthorpe, 1666 · Planta Atómica ═════════════════════════════════════════════════════════════════

    static int Newton(Transform t)
    {
        Material bark = M("Corteza", 0.36f, 0.27f, 0.2f), leaf = M("Hoja", 0.25f, 0.45f, 0.22f), apple = M("Manzana", 0.75f, 0.15f, 0.1f), glass = M("Vidrio", 0.8f, 0.9f, 0.95f), stone = M("Caliza", 0.85f, 0.8f, 0.66f), dark = M("Penumbra", 0.12f, 0.12f, 0.14f);
        Transform room = Spot(t, "Cuarto de Newton", new Vector3(-12f, 0f, 14f), "El cuarto de Newton (la rendija en la contraventana)", 2.5f);
        Prop(room, "Contraventana con rendija", new Vector3(0f, 1f, 2f), new Vector3(1.2f, 1.2f, 0.1f), dark);
        Prop(room, "Primer prisma", new Vector3(0f, 1f, 0.6f), new Vector3(0.2f, 0.25f, 0.2f), glass, false);
        Prop(room, "Segundo prisma", new Vector3(0f, 1f, -1.4f), new Vector3(0.2f, 0.25f, 0.2f), glass, false);
        string[] spectrum = { "Rojo", "Naranja", "Amarillo", "Verde", "Azul", "Añil", "Violeta" };
        Color[] cols = { new Color(0.9f, 0.1f, 0.1f), new Color(0.95f, 0.5f, 0.1f), new Color(0.95f, 0.9f, 0.1f), new Color(0.2f, 0.7f, 0.2f), new Color(0.2f, 0.35f, 0.9f), new Color(0.3f, 0.2f, 0.6f), new Color(0.55f, 0.2f, 0.7f) };
        for (int k = 0; k < 7; k++) Prop(room, "Franja " + spectrum[k], new Vector3(-0.6f + k * 0.2f, 1f, -3f), new Vector3(0.18f, 0.6f, 0.04f), AreaMicrocosmos.Mat("Hist_Espectro_" + spectrum[k], cols[k]), false);
        var prisms = Items(t, glass, new Vector3(0.25f, 0.3f, 0.25f), ("Prisma (feria de Stourbridge)", new Vector3(6f, 0.15f, -6f)), ("Prisma (feria de Stourbridge)", new Vector3(9f, 0.15f, -4f)));
        Transform orchard = Tree(t, "Manzano del huerto", new Vector3(12f, 0f, 14f), bark, leaf, 5f);
        Prop(t, "Manzana caída", new Vector3(13f, 0.2f, 12f), Vector3.one * 0.35f, apple, false);
        var dials = Items(t, stone, new Vector3(0.6f, 0.6f, 0.08f), ("Reloj de sol tallado en el muro", new Vector3(-18f, 1.2f, 4f)), ("Reloj de sol del establo", new Vector3(-18f, 1.2f, 0f)), ("Reloj de sol de la esquina", new Vector3(-16f, 1.2f, -4f)));
        Transform moonView = Spot(t, "Prado al anochecer", new Vector3(16f, 0f, 0f), "El prado: la Luna sobre Woolsthorpe", 2f);

        Color coat = new Color(0.3f, 0.25f, 0.22f);
        GameObject isaac = Modern(t, "Isaac Newton", new Vector3(-10f, 0f, 12f), false, coat, h: 1.7f);
        GameObject hannah = Modern(t, "Hannah Ayscough (Ruth)", new Vector3(-4f, 0f, 8f), true, new Color(0.4f, 0.35f, 0.3f), CubeInsect.Hat.Bonnet);
        GameObject shepherd = Modern(t, "Pastor (Momo)", new Vector3(10f, 0f, -8f), false, new Color(0.45f, 0.38f, 0.25f), CubeInsect.Hat.Brimmed); Routine(shepherd, moonView, orchard);
        GameObject servant = Modern(t, "Criado de la granja (Atlas)", new Vector3(-6f, 0f, 18f), false, new Color(0.5f, 0.42f, 0.32f)); Routine(servant, orchard, room);
        int n = 4;

        Mission(t, "Newton en Woolsthorpe: el año de la peste", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con Isaac Newton en la granja de su madre", isaac.transform, "Newton", "La peste ha cerrado Cambridge y he vuelto a casa. Aquí nadie me molesta: puedo pensar todo el día en la luz, en el movimiento y en el cálculo."),
                Talk("Habla con su madre, Hannah", hannah.transform, "Hannah", "Quise que llevara la granja, pero se olvida de las ovejas por leer. De niño tallaba relojes de sol en las paredes."),
                Visit("Lee la hora en los relojes de sol que talló de niño", dials, "Newton", "La sombra del palo gira 15 grados cada hora: la Tierra gira y el Sol parece moverse."),
                Collect("Lleva los dos prismas al cuarto de Newton", prisms, room, "Newton", "Los compré en la feria de Stourbridge. Ahora, cierra la contraventana."),
                Hold("El experimento crucial: mira cómo el primer prisma abre la luz en colores y el segundo no cambia el rojo", room, 12f, "Newton", "La luz blanca no es pura: es la mezcla de todos los colores. Cada color se desvía un poco distinto, y un color solo ya no se divide."),
                Reach("Ve al huerto de manzanos", orchard, "Criado", "Ha caído otra manzana. Siempre caen hacia abajo, nunca de lado. Y esta mañana el señor ha dejado sus prismas y ha llevado leña y pan a la puerta de la viuda del molinero, que tiene la peste en casa. No quiere que se sepa."),
                Hold("Quédate bajo el manzano y mira hacia la Luna", orchard, 10f, "Newton", "¿Y si la misma fuerza que tira de la manzana llega hasta la Luna, debilitada por la distancia? La Luna está a 60 radios de la Tierra: debería caer 60 × 60 = 3600 veces menos. Hago la cuenta… y casi cuadra."),
            },
            "Woolsthorpe (Lincolnshire, Inglaterra), 1666, sobre la planta atómica del santuario a escala de hormiga: el siguiente tramo de la escalera de la física tras Eratóstenes. La peste ha cerrado la universidad y el joven Isaac Newton está en la granja de su madre. Ayúdale con los prismas y piensa con él bajo el manzano. E para hablar y recoger; para mirar, quédate quieto.",
            new[]
            {
                "Isaac Newton nació en Woolsthorpe el día de Navidad de 1642 (calendario juliano). En 1665 la peste cerró la Universidad de Cambridge y pasó buena parte de 1665–1667 en casa. Él mismo contó después que en esos años empezó el cálculo («fluxiones»), la teoría de los colores y la idea de la gravitación universal: se los llama sus «años milagrosos».",
                "Con prismas comprados en una feria hizo pasar un rayo de sol por una rendija: el primer prisma abría la luz en un espectro de colores, y un segundo prisma no cambiaba el color de un rayo ya separado. Concluyó que la luz blanca es una mezcla de colores (Philosophical Transactions, 1672).",
                "La historia de la manzana la contó el propio Newton, ya anciano, a William Stukeley en 1726 (Memoirs of Sir Isaac Newton's Life, 1752): no le cayó en la cabeza, la vio caer y se preguntó si la misma fuerza alcanzaba la Luna. Comparó la caída de la Luna (a unos 60 radios terrestres) con la de una manzana y vio que encajaba con una fuerza que disminuye con el cuadrado de la distancia. Lo publicó en los «Principia» (1687).",
                "Newton era solitario, desconfiado y obsesivo con su trabajo (por eso su insecto es una araña): también dedicó años a la alquimia y a la teología.",
                "Es una recreación que Newton llevara leña y pan a una viuda: no consta, y él era más bien huraño. Se ha añadido para mostrar una peste que también fue vecindad.",
            },
            new[]
            {
                "Westfall, R. S. (1980). Never at Rest: A Biography of Isaac Newton. Cambridge University Press.",
                "Stukeley, W. (1752). Memoirs of Sir Isaac Newton's Life. Royal Society, MS/142 (digitalizado por la Royal Society).",
                "Newton, I. (1672). «A Letter … containing his New Theory about Light and Colors». Philosophical Transactions 6:3075–3087.",
                "National Trust, «Woolsthorpe Manor» (nationaltrust.org.uk).",
            },
            "Newton", "Si he visto más lejos, es porque miré mucho tiempo la misma cosa.");
        return n;
    }

    // ══ EDWARD JENNER · Berkeley, 1796 · Biología ═══════════════════════════════════════════════════════════════════

    static int Jenner(Transform t)
    {
        Material straw = M("Paja", 0.8f, 0.7f, 0.4f), wood = M("Madera", 0.55f, 0.4f, 0.25f), glass = M("Vidrio", 0.8f, 0.9f, 0.95f), steel = M("Acero", 0.7f, 0.72f, 0.75f);
        Transform byre = Spot(t, "Establo", new Vector3(-14f, 0f, 14f), "El establo de la granja (Blossom)", 2.5f);
        Prop(byre, "Paja", new Vector3(0f, 0.05f, 0f), new Vector3(6f, 0.1f, 5f), straw, false);
        Prop(byre, "Taburete de ordeñar", new Vector3(1.5f, 0.2f, -1f), new Vector3(0.4f, 0.4f, 0.4f), wood);
        Who(t, "Blossom (la vaca)", new Vector3(-14f, 0f, 15f), true, 1.2f, "Aphid");   // en el microcosmos, las vacas son pulgones
        Transform study = Spot(t, "Consulta de The Chantry", new Vector3(10f, 0f, 14f), "The Chantry: la consulta de Jenner", 2.5f);
        Prop(study, "Silla de curas", new Vector3(0f, 0.25f, 0f), new Vector3(0.6f, 0.5f, 0.6f), wood);
        var tools = Items(t, steel, new Vector3(0.3f, 0.05f, 0.1f), ("Lanceta", new Vector3(-6f, 0.05f, 4f)), ("Frasco con linfa de viruela vacuna", new Vector3(-8f, 0.05f, 6f)));
        Transform wait = Spot(t, "Jardín de The Chantry", new Vector3(14f, 0f, -4f), "Seis semanas de espera (1 de julio de 1796)", 2f);
        Transform notes = Spot(t, "Mesa de notas", new Vector3(4f, 0f, 20f), "Los casos de Jenner (la «Inquiry» de 1798)", 2f);
        Prop(notes, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.8f, 0.7f), wood);
        Prop(notes, "Cuaderno", new Vector3(0f, 0.85f, 0f), new Vector3(0.4f, 0.06f, 0.3f), M("Papel", 0.93f, 0.9f, 0.8f), false);

        Color coat = new Color(0.25f, 0.3f, 0.45f);
        GameObject edward = Modern(t, "Edward Jenner", new Vector3(8f, 0f, 12f), false, coat, CubeInsect.Hat.Brimmed);
        GameObject sarah = Modern(t, "Sarah Nelmes (Medea)", new Vector3(-12f, 0f, 12f), true, new Color(0.6f, 0.5f, 0.45f), CubeInsect.Hat.Bonnet);
        GameObject james = Modern(t, "James Phipps (Ambrosio)", new Vector3(12f, 0f, 10f), false, new Color(0.5f, 0.42f, 0.32f), CubeInsect.Hat.Cap, 1.25f);
        GameObject gardener = Modern(t, "El jardinero Phipps (Atlas)", new Vector3(16f, 0f, -2f), false, new Color(0.4f, 0.35f, 0.25f), CubeInsect.Hat.Brimmed); Routine(gardener, wait, study);
        int n = 5;

        Mission(t, "Edward Jenner y la primera vacuna", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con el doctor Jenner en The Chantry", edward.transform, "Jenner", "Las ordeñadoras dicen que quien ha pasado la viruela de las vacas no coge la viruela. La viruela mata a uno de cada tres enfermos. Quiero comprobar si es verdad."),
                Talk("Ve al establo y habla con Sarah Nelmes, la ordeñadora", sarah.transform, "Sarah", "Me pinché con una espina ordeñando a Blossom, y ella tenía la viruela vacuna en las ubres. Ahora tengo estas ampollas en la mano."),
                Hold("Mira a Blossom y las ampollas de la viruela vacuna", byre, 8f, "Sarah", "Es una enfermedad leve: unas ampollas, un poco de fiebre, y se pasa."),
                Collect("Lleva la lanceta y la linfa de la mano de Sarah a la consulta", tools, study, "Jenner", "Con esta lanceta tomo un poco del líquido de la ampolla."),
                Talk("14 de mayo: habla con James, el hijo del jardinero, de ocho años", james.transform, "James", "El doctor me ha hecho dos rasguñitos en el brazo. Me ha salido una ampolla y he tenido algo de fiebre un día. Ya estoy bien."),
                Hold("1 de julio, seis semanas después: espera en el jardín mientras Jenner le inocula viruela de verdad", wait, 12f, "Jenner", "No ha enfermado. Ni ahora ni en las pruebas siguientes: está protegido."),
                Reach("Ve a la mesa de notas", notes, "Jenner", "Lo llamaré variolae vaccinae, viruela de la vaca. Pero un caso no basta: repetiré la prueba con más personas antes de publicarlo. Y no la patentaré: que sea de todos."),
            },
            "Berkeley (Gloucestershire, Inglaterra), primavera de 1796, sobre el laboratorio de biología del santuario a escala de hormiga (las vacas de la granja son pulgones, como en todo el microcosmos). El médico rural Edward Jenner pone a prueba lo que cuentan las ordeñadoras. Acompáñale del establo a la consulta. E para hablar y recoger; para esperar, quédate quieto.",
            new[]
            {
                "Edward Jenner (1749–1823) era médico rural en Berkeley y naturalista (entró en la Royal Society en 1789 por su estudio del cuco). La viruela mataba entonces a unas 400 000 personas al año en Europa. Ya se practicaba la variolización (inocular pus de viruela para pasar una forma leve), traída a Inglaterra por Lady Mary Wortley Montagu en 1721; era útil pero peligrosa.",
                "El 14 de mayo de 1796 Jenner tomó líquido de las lesiones de viruela vacuna (cowpox) de la mano de la ordeñadora Sarah Nelmes, que se había contagiado de la vaca Blossom, y lo inoculó a James Phipps, de 8 años, hijo de su jardinero. El 1 de julio le inoculó viruela y el niño no enfermó. En 1798 publicó «An Inquiry into the Causes and Effects of the Variolae Vaccinae» con más casos. De «vacca» (vaca) viene la palabra vacuna.",
                "No fue el primero en intentarlo (el granjero Benjamin Jesty vacunó a su familia en 1774), pero sí quien lo estudió, lo publicó y lo difundió. Hoy un experimento así con un niño sería inaceptable: la ética médica exige consentimiento y comités de revisión.",
                "La vacunación se extendió por el mundo (la expedición Balmis, 1803–1806, la llevó a América y Asia con niños que portaban la vacuna de brazo a brazo). La OMS declaró erradicada la viruela el 8 de mayo de 1980: la única enfermedad humana erradicada. La piel de Blossom se conserva en la biblioteca de St George's (Londres).",
                "Jenner no patentó la vacuna, la dio a conocer y vacunó gratis a los pobres en una cabaña de su jardín, el «templo de Vaccinia».",
            },
            new[]
            {
                "Jenner, E. (1798). An Inquiry into the Causes and Effects of the Variolae Vaccinae. Londres: Sampson Low.",
                "Riedel, S. (2005). «Edward Jenner and the history of smallpox and vaccination». Proc (Bayl Univ Med Cent) 18:21–25. PMID 16200144.",
                "Jenner Museum, Berkeley (jennermuseum.com).",
                "Organización Mundial de la Salud (1980). The global eradication of smallpox: final report of the Global Commission.",
            },
            "Jenner", "Lo que sabían las ordeñadoras era verdad: solo había que comprobarlo con cuidado.");
        return n;
    }

    // ══ SIMÓN BOLÍVAR · Angostura, 1819 · Cocina ════════════════════════════════════════════════════════════════════

    static int Bolivar(Transform t)
    {
        Material water = M("Agua_Orinoco", 0.4f, 0.35f, 0.25f), granite = M("Granito", 0.45f, 0.42f, 0.4f), paper = M("Papel", 0.93f, 0.9f, 0.8f), wood = M("Madera", 0.55f, 0.4f, 0.25f), casabe = M("Casabe", 0.92f, 0.86f, 0.7f);
        Transform hall = Spot(t, "Sala del Congreso", new Vector3(0f, 0f, 16f), "Sala del Congreso de Angostura (15 de febrero de 1819)", 3f);
        Prop(hall, "Tribuna", new Vector3(0f, 0.5f, 2f), new Vector3(1.4f, 1f, 0.8f), wood);
        Transform press = Spot(t, "Prensa del Correo del Orinoco", new Vector3(-14f, 0f, 12f), "Imprenta del Correo del Orinoco", 2.5f);
        Prop(press, "Prensa", new Vector3(0f, 0.7f, 0f), new Vector3(1.2f, 1.4f, 0.8f), wood);
        var sheets = Items(t, paper, new Vector3(0.5f, 0.04f, 0.35f), ("Pliego del Correo del Orinoco", new Vector3(-12f, 0.6f, 8f)), ("Pliego del Correo del Orinoco", new Vector3(-15f, 0.6f, 8f)));
        Transform river = Spot(t, "Orilla del Orinoco", new Vector3(16f, 0f, 6f), "El Orinoco en su angostura", 2.5f);
        Prop(river, "Agua del Orinoco", new Vector3(4f, 0.02f, 0f), new Vector3(6f, 0.04f, 30f), water, false);
        Prop(river, "Piedra del Medio", new Vector3(5f, 0.6f, 2f), new Vector3(2.4f, 1.2f, 2f), granite);
        Transform kitchen = Spot(t, "Fogón de la tropa", new Vector3(-12f, 0f, -6f), "Cocina de la tropa: el budare del casabe", 2.5f);
        Prop(kitchen, "Budare", new Vector3(0f, 0.45f, 0f), new Vector3(1.2f, 0.9f, 1.2f), granite);
        var tortas = Items(t, casabe, new Vector3(0.5f, 0.05f, 0.5f), ("Torta de casabe", new Vector3(-6f, 0.05f, -8f)), ("Torta de casabe", new Vector3(-4f, 0.05f, -6f)), ("Torta de casabe", new Vector3(-8f, 0.05f, -10f)));

        Color blue = new Color(0.15f, 0.2f, 0.45f), white = new Color(0.93f, 0.92f, 0.88f);
        GameObject sb = Modern(t, "Simón Bolívar", new Vector3(2f, 0f, 14f), false, blue, CubeInsect.Hat.None, 1.68f);
        GameObject printer = Modern(t, "Impresor Andrés Roderick (Héspero)", new Vector3(-13f, 0f, 14f), false, white);
        GameObject boatman = Modern(t, "Bonguero (Momo)", new Vector3(14f, 0f, 4f), false, new Color(0.6f, 0.5f, 0.35f), CubeInsect.Hat.Brimmed);
        GameObject cook = Modern(t, "Cocinera de la tropa (Ruth)", new Vector3(-10f, 0f, -8f), true, new Color(0.7f, 0.4f, 0.3f));
        GameObject legion = Modern(t, "Voluntario británico (Atlas)", new Vector3(6f, 0f, -6f), false, new Color(0.6f, 0.15f, 0.15f), CubeInsect.Hat.Cap); Routine(legion, kitchen, hall, river);
        int n = 5;

        Mission(t, "Simón Bolívar en Angostura (1819)", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("En la plaza, habla con Simón Bolívar", sb.transform, "Bolívar", "Caracas está en manos realistas. Desde Angostura, sobre el Orinoco, gobernamos lo que hemos liberado: el río nos trae armas y voluntarios y nos lleva a los llanos."),
                Talk("En la imprenta, habla con el impresor", printer.transform, "Impresor", "Desde junio de 1818 imprimimos el Correo del Orinoco: la guerra también se hace con noticias."),
                Collect("Lleva los pliegos recién impresos a la sala del Congreso", sheets, hall, "Impresor", "Que los diputados lean lo que se dice de ellos en Londres y en Filadelfia."),
                Hold("15 de febrero de 1819: escucha el discurso de Bolívar en el Congreso", hall, 12f, "Bolívar", "Moral y luces son los polos de una república; moral y luces son nuestras primeras necesidades. Y os imploro la confirmación de la libertad absoluta de los esclavos."),
                Talk("En la orilla, pregunta al bonguero por la Piedra del Medio", boatman.transform, "Bonguero", "Esa piedra del medio del río es nuestra regla: cuando el Orinoco crece, se la traga; cuando baja, asoma. Aquí el río se estrecha: por eso se llama Angostura."),
                Collect("La tropa sale hacia los Andes: lleva el casabe al fogón para el viaje", tortas, kitchen, "Cocinera", "El casabe se hace con yuca amarga: se ralla, se exprime el jugo venenoso y se tuesta en el budare. Aguanta semanas sin estropearse."),
                Talk("Despídete de Bolívar antes de la campaña", sb.transform, "Bolívar", "En mayo saldremos a cruzar los llanos inundados y los Andes por el páramo de Pisba. Si llegamos, liberaremos la Nueva Granada."),
            },
            "Angostura del Orinoco (hoy Ciudad Bolívar, Venezuela), febrero de 1819, sobre la Sala de asambleas del santuario a escala de hormiga. Simón Bolívar ha reunido aquí un congreso para fundar una república en plena guerra de independencia. Ayuda en la imprenta, escucha el discurso y prepara la comida de la tropa. E para hablar y recoger; para escuchar, quédate quieto.",
            new[]
            {
                "Simón Bolívar (Caracas, 1783 – Santa Marta, 1830) dirigió la independencia de Venezuela, Colombia, Ecuador, Perú y Bolivia frente al Imperio español. Tras varias derrotas, en 1817 los patriotas tomaron Angostura, en el Orinoco, y la hicieron su capital provisional; en junio de 1818 fundaron el periódico «Correo del Orinoco».",
                "El 15 de febrero de 1819 se instaló el Congreso de Angostura. En su discurso, Bolívar dijo que «moral y luces son nuestras primeras necesidades», propuso una república con un senado hereditario y un poder «moral», y pidió confirmar la libertad de los esclavos. El 17 de diciembre de 1819 el Congreso creó la República de Colombia (la «Gran Colombia»).",
                "Entre mayo y agosto de 1819 su ejército cruzó los llanos inundados y los Andes por el páramo de Pisba, donde murieron muchos soldados de frío, y venció en Boyacá el 7 de agosto. Combatieron con él llaneros, esclavos liberados y voluntarios británicos e irlandeses (la Legión Británica).",
                "Su figura tiene luces y sombras: defendió la abolición de la esclavitud y la educación, pero también un gobierno muy centralizado y un presidente vitalicio (Constitución de Bolivia, 1826). Angostura pasó a llamarse Ciudad Bolívar en 1846. El casabe, el pan de yuca de los pueblos indígenas del Orinoco, era comida de viaje de la tropa.",
            },
            new[]
            {
                "Lynch, J. (2006). Simón Bolívar: A Life. Yale University Press.",
                "Bolívar, S. (1819). Discurso pronunciado ante el Congreso de Angostura (en: Doctrina del Libertador, Biblioteca Ayacucho, 1976).",
                "Correo del Orinoco (1818–1822), edición facsimilar. Academia Nacional de la Historia, Caracas.",
                "Humboldt, A. von (1814–1825). Viaje a las regiones equinocciales del Nuevo Continente (Angostura, el Orinoco y la Piedra del Medio).",
            },
            "Bolívar", "Un pueblo ignorante es instrumento ciego de su propia destrucción.");
        return n;
    }

    // ══ NIETZSCHE · Sils Maria, 1881 · Sala de Yoga ══════════════════════════════════════════════════════════════════

    static int Nietzsche(Transform t)
    {
        Material lake = M("Lago", 0.35f, 0.55f, 0.65f), granite = M("Granito", 0.55f, 0.55f, 0.52f), wood = M("Madera", 0.55f, 0.4f, 0.25f), paper = M("Papel", 0.93f, 0.9f, 0.8f);
        Transform room = Spot(t, "Cuarto de la casa Durisch", new Vector3(-12f, 0f, 14f), "El cuarto de Nietzsche (casa Durisch)", 2.5f);
        Prop(room, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 0.7f), wood);
        var notebooks = Items(t, paper, new Vector3(0.3f, 0.06f, 0.22f), ("Cuaderno de paseo", new Vector3(-8f, 0.05f, 8f)), ("Cuaderno de paseo", new Vector3(-6f, 0.05f, 12f)));
        var path = Items(t, granite, new Vector3(0.6f, 0.3f, 0.6f), ("Mojón del sendero (Sils)", new Vector3(-2f, 0.15f, 2f)), ("Mojón del sendero (orilla)", new Vector3(6f, 0.15f, 6f)), ("Mojón del sendero (Surlej)", new Vector3(12f, 0.15f, 12f)));
        Transform shore = Spot(t, "Orilla del lago de Silvaplana", new Vector3(16f, 0f, 6f), "Lago de Silvaplana", 2f);
        Prop(shore, "Agua del lago", new Vector3(4f, 0.02f, 4f), new Vector3(6f, 0.04f, 26f), lake, false);
        Transform rock = Spot(t, "La roca de Surlej", new Vector3(14f, 0f, 16f), "La roca de Surlej (el eterno retorno)", 4f);
        for (int k = 0; k < 3; k++) Prop(rock, "Roca piramidal", new Vector3(0f, 0.8f + k * 1.1f, 1.5f), new Vector3(3.6f - k * 1.1f, 1.6f, 3f - k * 0.9f), granite);
        Transform chaste = Spot(t, "Península de Chasté", new Vector3(-16f, 0f, -4f), "Península de Chasté (respirar junto al lago)", 2f);

        Color coat = new Color(0.25f, 0.24f, 0.22f);
        GameObject fn = Modern(t, "Friedrich Nietzsche", new Vector3(-10f, 0f, 12f), false, coat, CubeInsect.Hat.Brimmed, 1.71f);
        GameObject durisch = Modern(t, "Gian Durisch (Shanidar)", new Vector3(-14f, 0f, 10f), false, new Color(0.4f, 0.35f, 0.3f), CubeInsect.Hat.Cap, 1.65f);
        GameObject walker = Modern(t, "Veraneante (Sakshi)", new Vector3(4f, 0f, 4f), true, new Color(0.6f, 0.65f, 0.75f), CubeInsect.Hat.Bonnet); Routine(walker, shore, chaste);
        GameObject herder = Modern(t, "Pastor del Fex (Momo)", new Vector3(-6f, 0f, -6f), false, new Color(0.45f, 0.38f, 0.25f)); Routine(herder, chaste, shore);
        int n = 4;

        Mission(t, "Nietzsche en Sils Maria: caminar y el eterno retorno", StoryMission.Marga.Yoga, 60f,
            new[]
            {
                Talk("Habla con Nietzsche en su cuarto de la casa Durisch", fn.transform, "Nietzsche", "Dejé mi cátedra de Basilea: las jaquecas y los ojos ya no me dejaban leer. Aquí, a 1800 metros, el aire me deja pensar. Escribo caminando: seis, ocho horas al día."),
                Talk("Habla con Gian Durisch, que le alquila el cuarto", durisch.transform, "Durisch", "Paga poco y pide poco: silencio y su cuarto a oscuras por los ojos. Sale con el cuaderno temprano y vuelve al anochecer. Medio ciego, con esas jaquecas, y ayer se paró a llevarle la leña a mi mujer hasta casa. A mi niña le guarda higos. Dirán lo que quieran de sus libros: es el hombre más amable que ha pasado por aquí."),
                Collect("Llévale sus cuadernos de paseo", notebooks, room, "Nietzsche", "No me fío de ningún pensamiento que no haya nacido al aire libre y en movimiento."),
                Visit("Camina con él por el sendero de la orilla, mojón a mojón", path, "Nietzsche", "Paso, paso, respiración. El cuerpo piensa: la cabeza solo pone en limpio lo que el cuerpo ya sabía."),
                Hold("Junto a la roca de Surlej, quédate quieto con él", rock, 12f, "Nietzsche", "¿Y si un demonio te dijera que esta vida, tal como la vives, la vivirás otra vez, e innumerables veces, sin nada nuevo? ¿Lo maldecirías… o le dirías: nunca oí nada más divino? Lo anoto: «a 6000 pies más allá del hombre y del tiempo»."),
                Talk("Pregúntale qué quiere decir", fn.transform, "Nietzsche", "Amor fati: amar lo que es necesario. No querer que nada sea distinto, ni hacia delante ni hacia atrás."),
                Hold("En la península de Chasté, respira despacio junto al lago", chaste, 10f, "Veraneante", "Aquí todo el mundo viene a curarse algo. Él dice que camina para curarse de pensar sentado."),
            },
            "Sils Maria (Alta Engadina, Suiza), principios de agosto de 1881, sobre la sala de yoga del santuario a escala de hormiga. El filósofo Friedrich Nietzsche pasa aquí su primer verano: camina horas junto a los lagos y escribe en cuadernos. Camina con él hasta la roca donde tuvo su idea más famosa. E para hablar y recoger; en los pasos de práctica, quédate quieto.",
            new[]
            {
                "Friedrich Nietzsche (1844–1900) fue catedrático de filología en Basilea a los 24 años. En 1879 dejó la cátedra por su mala salud (jaquecas, vista muy débil, problemas de estómago) y vivió como un viajero solitario entre Italia y Suiza. Desde 1881 pasó casi todos los veranos en Sils Maria, en un cuarto que alquilaba en la casa de Gian Durisch (hoy la Nietzsche-Haus).",
                "A principios de agosto de 1881, paseando por la orilla del lago de Silvaplana, junto a una gran roca piramidal cerca de Surlej, tuvo la idea del «eterno retorno»: la anotó en un papel con la frase «6000 pies más allá del hombre y del tiempo». La publicó en «La gaya ciencia» (1882, §341, «El peso más pesado») y la desarrolló en «Así habló Zaratustra» (1883–1885).",
                "Daba mucha importancia a caminar: «Solo los pensamientos que se tienen caminando tienen valor» (El ocaso de los ídolos, 1889). En enero de 1889 sufrió un colapso mental en Turín y no se recuperó.",
                "Su hermana Elisabeth, que controló sus escritos tras su muerte, los manipuló y los acercó al nacionalismo y al antisemitismo, que él había despreciado. Por eso conviene leerlo en ediciones críticas (Colli y Montinari).",
                "Son una recreación la leña y los higos: no constan. Quienes lo trataron en Sils lo recuerdan como un huésped amable y discreto, y en Turín, en 1889, cuentan que abrazó a un caballo maltratado (el relato es dudoso).",
            },
            new[]
            {
                "Nietzsche, F. Ecce homo (1888), «Así habló Zaratustra» §1 (el relato de la roca de Surlej).",
                "Nietzsche, F. La gaya ciencia (1882), §341; Fragmentos póstumos, agosto de 1881, 11[141] (KSA 9).",
                "Safranski, R. (2000). Nietzsche: biografía de su pensamiento. Tusquets.",
                "Nietzsche-Haus Sils Maria (nietzschehaus.ch); Sils Tourismus, «Nietzsche-Stein am Silvaplanersee».",
            },
            "Nietzsche", "Llega a ser el que eres.");
        return n;
    }

    // ══ MARIE Y PIERRE CURIE · París, 1898 · Química ═══════════════════════════════════════════════════════════════

    static int Curie(Transform t)
    {
        Material brown = M("Pechblenda", 0.25f, 0.22f, 0.2f), iron = M("Hierro", 0.3f, 0.3f, 0.32f), glow = M("Brillo_radio", 0.55f, 0.9f, 0.85f), wood = M("Madera", 0.55f, 0.4f, 0.25f), brass = M("Laton", 0.8f, 0.65f, 0.3f);
        Transform meter = Spot(t, "Electrómetro", new Vector3(-12f, 0f, 14f), "Electrómetro de cuarzo piezoeléctrico", 2.5f);
        Prop(meter, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.6f, 0.8f, 0.8f), wood);
        Prop(meter, "Cámara de ionización", new Vector3(-0.4f, 0.95f, 0f), new Vector3(0.3f, 0.3f, 0.3f), brass, false);
        Prop(meter, "Cuarzo y pesas", new Vector3(0.4f, 1f, 0f), new Vector3(0.1f, 0.4f, 0.1f), M("Cuarzo", 0.9f, 0.9f, 0.95f), false);
        var sacks = Items(t, brown, new Vector3(0.6f, 0.5f, 0.6f), ("Saco de residuos de pechblenda (Joachimsthal)", new Vector3(10f, 0.25f, -6f)), ("Saco de residuos de pechblenda", new Vector3(13f, 0.25f, -4f)), ("Saco de residuos de pechblenda", new Vector3(16f, 0.25f, -6f)));
        Transform cauldron = Spot(t, "Patio del cobertizo", new Vector3(12f, 0f, 10f), "El caldero (20 kg por tanda)", 2.5f);
        Prop(cauldron, "Caldero", new Vector3(0f, 0.45f, 0f), new Vector3(1.2f, 0.9f, 1.2f), iron);
        Prop(cauldron, "Barra de hierro", new Vector3(0.3f, 1.2f, 0f), new Vector3(0.06f, 1.4f, 0.06f), iron, false);
        Transform shelf = Spot(t, "Estante de los tubos", new Vector3(0f, 0f, 20f), "De noche: los tubos que brillan", 2.5f);
        for (int k = 0; k < 5; k++) Prop(shelf, "Tubo de sal de radio", new Vector3(-1f + k * 0.5f, 1.1f, 0f), new Vector3(0.08f, 0.3f, 0.08f), glow, false);
        Prop(shelf, "Balda", new Vector3(0f, 0.95f, 0f), new Vector3(3f, 0.06f, 0.4f), wood);

        Color dark = new Color(0.15f, 0.15f, 0.17f);
        GameObject marie = Modern(t, "Marie Curie", new Vector3(-10f, 0f, 12f), true, dark, CubeInsect.Hat.None, 1.6f);
        GameObject pierre = Modern(t, "Pierre Curie", new Vector3(-14f, 0f, 12f), false, dark, CubeInsect.Hat.None, 1.78f);
        GameObject bemont = Modern(t, "Gustave Bémont (Héspero)", new Vector3(10f, 0f, 12f), false, new Color(0.3f, 0.3f, 0.35f)); Routine(bemont, cauldron, meter);
        GameObject carter = Modern(t, "Carretero (Atlas)", new Vector3(14f, 0f, -8f), false, new Color(0.45f, 0.4f, 0.3f), CubeInsect.Hat.Cap);
        int n = 4;

        Mission(t, "Marie y Pierre Curie: el polonio y el radio", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("En el cobertizo de la rue Lhomond, habla con Marie Curie", marie.transform, "Marie", "Becquerel vio que el uranio emite unos rayos que velan las placas. Yo los mido: hacen conductor el aire. Cuanto más uranio, más corriente, sin importar cómo esté combinado. Es una propiedad del átomo. La llamo radiactividad."),
                Hold("Mide con el electrómetro de cuarzo de Pierre", meter, 10f, "Pierre", "El cuarzo, al estirarlo con pesas, da una carga que compensa la del aire ionizado. Así medimos corrientes diminutas con precisión."),
                Talk("Pregunta a Marie por la pechblenda", marie.transform, "Marie", "La pechblenda es más activa que el uranio puro que contiene. Tiene que haber dentro otro elemento, desconocido y mucho más activo."),
                Collect("Ayuda al carretero a llevar los sacos de residuos de pechblenda al patio", sacks, cauldron, "Carretero", "Vienen de las minas de Joachimsthal, en Bohemia. Toneladas de esto, ¿y para sacar qué?"),
                Hold("Remueve el caldero con la barra de hierro", cauldron, 12f, "Marie", "Disuelvo, filtro, precipito y vuelvo a cristalizar, una y otra vez. Cada vez la fracción que guardo es más activa. Es un trabajo agotador."),
                Talk("Habla con Pierre", pierre.transform, "Pierre", "En julio encontramos el primero y Marie lo llamó polonio, por su Polonia. Y el 26 de diciembre anunciamos el segundo, con Bémont: el radio."),
                Hold("De noche, mira los tubos que brillan en el estante", shelf, 8f, "Marie", "Una de nuestras alegrías era entrar de noche y ver, por todas partes, las siluetas débilmente luminosas de los frascos. Y no patentaremos el método: el radio es de todos."),
            },
            "París, diciembre de 1898, sobre el laboratorio de química del santuario a escala de hormiga. En un cobertizo de la Escuela de Física y Química, con goteras y una estufa, Marie y Pierre Curie buscan un elemento desconocido dentro de la pechblenda. Mide, carga y remueve con ellos. E para hablar y recoger; en los pasos de trabajo, quédate quieto.",
            new[]
            {
                "Maria Skłodowska (Varsovia, 1867 – 1934) llegó a París en 1891 y se casó con Pierre Curie en 1895. Para su tesis estudió los «rayos de Becquerel» (1896) con el electrómetro de cuarzo piezoeléctrico de Pierre y su hermano Jacques, y propuso que la radiación era una propiedad del átomo: acuñó la palabra radiactividad.",
                "En 1898 anunciaron el polonio (julio) y el radio (26 de diciembre, con Gustave Bémont), separados de residuos de pechblenda de Joachimsthal. Trabajaron en un cobertizo de la ESPCI, en la rue Lhomond. En 1902 Marie obtuvo una décima de gramo de cloruro de radio puro a partir de toneladas de mineral y midió su masa atómica.",
                "Recibió el Nobel de Física en 1903 (con Pierre y Becquerel) y el de Química en 1911: la primera persona con dos premios Nobel y la única en dos ciencias distintas. Pierre murió atropellado por un carro en 1906.",
                "No sabían lo peligrosa que era la radiación: llevaban muestras en los bolsillos. Marie murió en 1934 de anemia aplásica, probablemente por la radiación. Sus cuadernos siguen siendo radiactivos y se guardan en cajas de plomo en la Biblioteca Nacional de Francia. Por eso hoy se trabaja con dosímetro, blindaje y distancia.",
                "Marie y Pierre Curie no patentaron su método de aislar el radio y lo compartieron con quien se lo pidió.",
            },
            new[]
            {
                "Curie, P. y Curie, M. (1898). «Sur une substance nouvelle radio-active, contenue dans la pechblende». Comptes rendus 127:175–178.",
                "Curie, P., Curie, M. y Bémont, G. (1898). «Sur une nouvelle substance fortement radio-active, contenue dans la pechblende». Comptes rendus 127:1215–1217.",
                "Curie, M. (1923). Pierre Curie. Macmillan (el relato del cobertizo y los frascos luminosos).",
                "Quinn, S. (1995). Marie Curie: A Life. Simon & Schuster; Musée Curie (musee.curie.fr).",
            },
            "Marie", "En la vida no hay que temer nada, solo comprender.");
        return n;
    }

    // ══ NIKOLA TESLA · Colorado Springs, 1899 · Construcción ═══════════════════════════════════════════════════════

    static int Tesla(Transform t)
    {
        Material copper = M("Cobre", 0.75f, 0.45f, 0.25f), wood = M("Madera", 0.55f, 0.4f, 0.25f), bulb = M("Bombilla", 1f, 0.95f, 0.7f), spark = M("Chispa", 0.75f, 0.8f, 1f), steel = M("Acero", 0.6f, 0.62f, 0.65f);
        Transform coil = Spot(t, "Transmisor amplificador", new Vector3(0f, 0f, 18f), "El transmisor amplificador (la bobina gigante)", 6f);
        for (int k = 0; k < 8; k++) Prop(coil, "Espira", new Vector3(0f, 0.2f + k * 0.3f, 0f), new Vector3(5f - k * 0.1f, 0.05f, 5f - k * 0.1f), copper, false);
        Prop(coil, "Mástil", new Vector3(0f, 4f, 0f), new Vector3(0.15f, 8f, 0.15f), wood);
        Prop(coil, "Bola de cobre", new Vector3(0f, 8.2f, 0f), Vector3.one * 0.8f, copper, false);
        for (int k = 0; k < 4; k++) Prop(coil, "Descarga", new Vector3(Mathf.Cos(k * 1.57f) * 1.8f, 8f, Mathf.Sin(k * 1.57f) * 1.8f), new Vector3(0.05f, 0.05f, 3f), spark, false);
        var lamps = Items(t, bulb, new Vector3(0.2f, 0.3f, 0.2f), ("Lámpara clavada en el suelo (sin cables)", new Vector3(10f, 0.15f, -4f)), ("Lámpara clavada en el suelo", new Vector3(14f, 0.15f, 2f)), ("Lámpara clavada en el suelo", new Vector3(16f, 0.15f, -8f)));
        Transform plant = Spot(t, "Dinamo de la central", new Vector3(-14f, 0f, 6f), "La central de El Paso Electric (el generador quemado)", 2.5f);
        Prop(plant, "Dinamo", new Vector3(0f, 0.6f, 0f), new Vector3(1.6f, 1.2f, 1.2f), steel);
        Transform hill = Spot(t, "Loma de la tormenta", new Vector3(-14f, 0f, -6f), "La tormenta del 3 de julio (ondas estacionarias)", 2f);
        Transform memory = Spot(t, "Banco del recuerdo", new Vector3(-4f, 0f, 2f), "Budapest, 1882: el campo magnético giratorio", 1.6f);
        Prop(memory, "Banco", new Vector3(0f, 0.25f, 0f), new Vector3(1.6f, 0.5f, 0.5f), wood);

        Color suit = new Color(0.18f, 0.18f, 0.2f);
        GameObject nt = Modern(t, "Nikola Tesla", new Vector3(2f, 0f, 12f), false, suit, CubeInsect.Hat.Brimmed, 1.88f);
        GameObject czito = Modern(t, "Kolman Czito (Atlas)", new Vector3(-3f, 0f, 14f), false, new Color(0.45f, 0.4f, 0.3f), CubeInsect.Hat.Cap);
        GameObject engineer = Modern(t, "Maquinista de la central (Momo)", new Vector3(-12f, 0f, 8f), false, new Color(0.35f, 0.35f, 0.4f), CubeInsect.Hat.Cap);
        GameObject reporter = Modern(t, "Periodista de Colorado (Medea)", new Vector3(8f, 0f, 0f), true, new Color(0.55f, 0.45f, 0.5f), CubeInsect.Hat.Bonnet); Routine(reporter, coil, hill);
        int n = 4;

        Mission(t, "Nikola Tesla en Colorado Springs", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con Tesla en su estación experimental", nt.transform, "Tesla", "Aquí, a casi 2000 metros, el aire es seco y las tormentas son enormes. He venido a estudiar la electricidad de la Tierra y a enviar energía sin cables."),
                Hold("Siéntate en el banco y escucha cómo se le ocurrió el motor de corriente alterna", memory, 10f, "Tesla", "Budapest, 1882: paseaba al atardecer recitando el Fausto de Goethe y lo vi de golpe: un campo magnético que gira, con dos corrientes alternas desfasadas. Lo dibujé en la arena con un bastón."),
                Talk("Pregunta a Czito por la bobina", czito.transform, "Czito", "La bobina primaria da pulsos; la secundaria resuena a la misma frecuencia y sube la tensión a millones de voltios. Todo es resonancia: como empujar un columpio en el momento justo."),
                Visit("Comprueba las tres lámparas clavadas en el campo: se encienden sin cables", lamps, "Tesla", "La energía llega por el campo y por el suelo, resonando. A unos metros, mis lámparas brillan sin tocar ningún hilo."),
                Hold("Lejos de la bobina, mira el rayo artificial", coil, 12f, "Periodista", "¡Descargas de decenas de metros y un trueno que se oye en la ciudad! Los caballos echan chispas por las herraduras."),
                Talk("En la central, habla con el maquinista", engineer.transform, "Maquinista", "Con su prueba ha quemado nuestro generador y ha dejado a oscuras Colorado Springs. Ha pagado él la reparación. Dicen que una vez rompió su contrato con Westinghouse y renunció a una fortuna para salvar a quien creyó en él."),
                Hold("3 de julio: en la loma, mide la tormenta con Tesla", hill, 10f, "Tesla", "La tormenta se aleja, y mis aparatos marcan subidas y bajadas regulares: ondas estacionarias. Creo que la Tierra entera resuena, como un instrumento."),
            },
            "Colorado Springs (Colorado, EE. UU.), julio de 1899, sobre la construcción del santuario a escala de hormiga (su área lleva la electricidad). Nikola Tesla, el inventor del motor y el sistema de corriente alterna, ha montado una estación al pie de las Montañas Rocosas para hacer rayos y enviar energía sin cables. E para hablar y tocar; para mirar, quédate quieto y lejos de la bobina.",
            new[]
            {
                "Nikola Tesla (Smiljan, 1856 – Nueva York, 1943), serbio nacido en el Imperio austríaco, contó que en 1882, paseando por un parque de Budapest, imaginó el campo magnético giratorio. En Estados Unidos patentó el motor de inducción de corriente alterna (1888), que Westinghouse llevó a la Exposición de Chicago de 1893 y a la central de las cataratas del Niágara (1895–1896): es la base de la red eléctrica actual.",
                "De mayo de 1899 a enero de 1900 trabajó en Colorado Springs con un «transmisor amplificador»: una bobina resonante enorme con un mástil y una bola de cobre. Produjo descargas de decenas de metros (sus notas hablan de más de 40 m; probablemente menos), encendió lámparas a distancia y, al sobrecargar el generador de la compañía local, dejó la ciudad a oscuras.",
                "El 3 de julio de 1899, midiendo una tormenta que se alejaba, creyó detectar ondas estacionarias en la Tierra y concluyó que se podría transmitir energía por todo el planeta. Lo intentó en la torre de Wardenclyffe (Nueva York), que quedó sin terminar por falta de dinero. Algunas de sus afirmaciones (señales de Marte) no se confirmaron.",
                "Murió pobre en un hotel de Nueva York. La unidad de inducción magnética del Sistema Internacional, el tesla (T), lleva su nombre desde 1960.",
                "Que rompiera su contrato con Westinghouse lo cuentan sus biógrafos, y otros historiadores lo discuten: se da como relato, no como hecho seguro.",
            },
            new[]
            {
                "Tesla, N. (1978). Colorado Springs Notes, 1899–1900. Nolit / Museo Nikola Tesla, Belgrado.",
                "Tesla, N. (1919). «My Inventions». Electrical Experimenter (el relato de Budapest, 1882).",
                "Carlson, W. B. (2013). Tesla: Inventor of the Electrical Age. Princeton University Press.",
                "Patente de EE. UU. 381 968 (1888), «Electro-Magnetic Motor».",
            },
            "Tesla", "El presente es de ellos; el futuro, por el que tanto he trabajado, es mío.");
        return n;
    }

    // ══ ALBERT EINSTEIN · Berna, 1905 · Planta Atómica ═════════════════════════════════════════════════════════════

    static int Einstein(Transform t)
    {
        Material stone = M("Arenisca", 0.75f, 0.68f, 0.52f), gold = M("Oro", 0.9f, 0.75f, 0.3f), wood = M("Madera", 0.55f, 0.4f, 0.25f), paper = M("Papel", 0.93f, 0.9f, 0.8f), rail = M("Rail", 0.4f, 0.4f, 0.42f);
        Transform tower = Spot(t, "Torre del Reloj", new Vector3(0f, 0f, 20f), "Zytglogge, la Torre del Reloj de Berna", 9f);
        Prop(tower, "Torre", new Vector3(0f, 3.5f, 2f), new Vector3(2.4f, 7f, 2.4f), stone);
        Prop(tower, "Esfera del reloj", new Vector3(0f, 5.5f, 0.75f), new Vector3(1.4f, 1.4f, 0.1f), gold, false);
        Prop(t, "Vía del tranvía", new Vector3(0f, 0.02f, 6f), new Vector3(0.8f, 0.04f, 22f), rail, false);
        Transform office = Spot(t, "Pupitre de la Oficina de Patentes", new Vector3(14f, 0f, 8f), "Oficina de Patentes: técnico experto de 3.ª clase", 2.5f);
        Prop(office, "Pupitre", new Vector3(0f, 0.5f, 0f), new Vector3(1.4f, 1f, 0.7f), wood);
        var patents = Items(t, paper, new Vector3(0.4f, 0.04f, 0.3f), ("Solicitud: reloj eléctrico sincronizado", new Vector3(10f, 0.05f, 2f)), ("Solicitud: señal de tiempo por telégrafo", new Vector3(12f, 0.05f, 0f)), ("Solicitud: máquina de coser (rechazable)", new Vector3(15f, 0.05f, 2f)));
        Transform table = Spot(t, "Mesa de la Academia Olimpia", new Vector3(-14f, 0f, 10f), "La Academia Olimpia (Hume, Mach, Poincaré)", 2.5f);
        Prop(table, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.8f, 1.4f), wood);
        Transform home = Spot(t, "Kramgasse 49", new Vector3(-14f, 0f, -4f), "Kramgasse 49: los cuatro artículos de 1905", 2.5f);
        Prop(home, "Mesa de trabajo", new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.8f, 0.7f), wood);

        Color suit = new Color(0.3f, 0.28f, 0.26f);
        GameObject ae = Modern(t, "Albert Einstein", new Vector3(-2f, 0f, 14f), false, suit, CubeInsect.Hat.None, 1.72f);
        GameObject mileva = Modern(t, "Mileva Marić", new Vector3(-16f, 0f, -2f), true, new Color(0.35f, 0.3f, 0.4f), CubeInsect.Hat.None, 1.6f);
        GameObject besso = Modern(t, "Michele Besso (Héspero)", new Vector3(12f, 0f, 10f), false, new Color(0.35f, 0.33f, 0.3f));
        GameObject solovine = Modern(t, "Maurice Solovine (Momo)", new Vector3(-12f, 0f, 12f), false, new Color(0.4f, 0.35f, 0.3f));
        GameObject habicht = Modern(t, "Conrad Habicht (Atlas)", new Vector3(-16f, 0f, 12f), false, new Color(0.3f, 0.35f, 0.3f)); Routine(habicht, table, tower);
        int n = 5;

        Mission(t, "Albert Einstein en Berna: el año milagroso", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Bajo la Torre del Reloj, habla con Einstein", ae.transform, "Einstein", "Trabajo ocho horas en la Oficina de Patentes y el resto del tiempo pienso. Me pregunto qué vería si viajara montado en un rayo de luz."),
                Hold("Mira el reloj de la torre desde el tranvía que se aleja", tower, 10f, "Einstein", "Si me alejara de la torre a la velocidad de la luz, la luz de las agujas no me alcanzaría: para mí, el reloj parecería parado. ¿Qué significa entonces «al mismo tiempo»?"),
                Visit("En la oficina, revisa tres solicitudes de patente", patents, "Besso", "Aquí llegan muchos aparatos para sincronizar relojes de estaciones lejanas con señales eléctricas. Saber qué hora es «a la vez» en dos sitios es un problema práctico."),
                Talk("Habla con su amigo Michele Besso", besso.transform, "Besso", "Ayer le di vueltas al problema con él y se fue a casa abatido. Hoy ha venido y me ha dicho: «Gracias, lo he resuelto». El tiempo no es absoluto."),
                Hold("En la mesa de la Academia Olimpia, discute con Solovine y Habicht", table, 10f, "Solovine", "Leemos a Hume, a Mach, a Poincaré, y discutimos hasta las tantas con salchicha, queso y té. Lo llamamos, en broma, la Academia Olimpia. Yo vine por un anuncio, a pagarle clases de física. Pronto dejó de cobrarme: dijo que prefería charlar."),
                Talk("En casa, habla con Mileva", mileva.transform, "Mileva", "Estudiamos física juntos en Zúrich. Ahora tenemos a Hans Albert, de un año, y él escribe por las noches en esta mesa."),
                Reach("Vuelve a la mesa de la Kramgasse", home, "Einstein", "Cuatro artículos este año: la luz en cuantos, el movimiento browniano, la relatividad especial y, en septiembre, una nota corta: la masa es energía, E = mc²."),
            },
            "Berna (Suiza), mayo de 1905, sobre la planta atómica del santuario a escala de hormiga: el tramo de la escalera de la física que lleva al átomo. Un examinador de patentes de 26 años, Albert Einstein, está a punto de cambiar la idea de tiempo. Piensa con él bajo la Torre del Reloj, revisa patentes y discute con sus amigos. E para hablar y recoger; para pensar, quédate quieto.",
            new[]
            {
                "Albert Einstein (Ulm, 1879 – Princeton, 1955) no encontró plaza en la universidad tras licenciarse y en 1902 entró en la Oficina Federal de la Propiedad Intelectual de Berna como «técnico experto de 3.ª clase». Vivía con Mileva Marić y su hijo Hans Albert en la Kramgasse 49, a pocos pasos de la Torre del Reloj (hoy la Einsteinhaus).",
                "En 1905, su «año milagroso», publicó en «Annalen der Physik» cuatro artículos: la luz formada por cuantos de energía (efecto fotoeléctrico, por el que recibió el Nobel de 1921), el movimiento browniano (la prueba de que existen los átomos), la relatividad especial («Sobre la electrodinámica de los cuerpos en movimiento», junio) y la equivalencia entre masa y energía (septiembre), E = mc².",
                "Contó (Kioto, 1922) que la clave le vino tras una conversación con su amigo Michele Besso: la simultaneidad depende del observador y el tiempo no es absoluto. El historiador Peter Galison señala que en la oficina de patentes pasaban muchos aparatos para sincronizar relojes con señales eléctricas: un problema práctico de la época.",
                "Con sus amigos Maurice Solovine y Conrad Habicht leía y discutía filosofía y física en la «Academia Olimpia». La historia de que miró la Torre del Reloj desde el tranvía es una anécdota muy repetida, sin documento que la confirme.",
                "Según contó Maurice Solovine, llegó por un anuncio de clases y Einstein pronto dejó de cobrarle. Se da como recuerdo de Solovine.",
            },
            new[]
            {
                "Einstein, A. (1905). «Zur Elektrodynamik bewegter Körper». Annalen der Physik 17:891–921.",
                "Isaacson, W. (2007). Einstein: His Life and Universe. Simon & Schuster.",
                "Galison, P. (2003). Einstein's Clocks, Poincaré's Maps. W. W. Norton.",
                "Einsteinhaus Bern, Albert Einstein Gesellschaft (einstein-bern.ch).",
            },
            "Einstein", "Lo importante es no dejar de hacerse preguntas.");
        return n;
    }

    // ══ CLAIR C. PATTERSON · Pasadena y Argonne, 1953 · Química ═══════════════════════════════════════════════════════

    static int Patterson(Transform t)
    {
        Material steel = M("Acero", 0.7f, 0.72f, 0.75f), rock = M("Meteorito", 0.3f, 0.28f, 0.27f), red = M("Surtidor", 0.75f, 0.15f, 0.12f), white = M("Blanco", 0.95f, 0.95f, 0.95f), dust = M("Polvo", 0.6f, 0.55f, 0.45f), ice = M("Hielo", 0.85f, 0.92f, 1f);
        Transform clean = Spot(t, "Banco limpio", new Vector3(-12f, 0f, 14f), "Laboratorio limpio (aire filtrado)", 2.5f);
        Prop(clean, "Campana de aire filtrado", new Vector3(0f, 0.6f, 0f), new Vector3(1.6f, 1.2f, 0.8f), white);
        var dirt = Items(t, dust, new Vector3(0.4f, 0.1f, 0.4f), ("Polvo del suelo (plomo)", new Vector3(-16f, 0.05f, 10f)), ("Vidrio sin lavar con ácido", new Vector3(-14f, 0.05f, 8f)), ("Pintura vieja de la pared (plomo)", new Vector3(-10f, 0.05f, 8f)));
        var meteor = Items(t, rock, new Vector3(0.5f, 0.4f, 0.5f), ("Troilita del meteorito del Cañón del Diablo", new Vector3(-4f, 0.2f, 4f)));
        Transform spectro = Spot(t, "Espectrómetro de masas", new Vector3(12f, 0f, 16f), "Argonne: el espectrómetro de masas", 2.5f);
        Prop(spectro, "Imán y tubo de vuelo", new Vector3(0f, 0.8f, 0f), new Vector3(2.4f, 1.6f, 0.9f), steel);
        Transform station = Spot(t, "Surtidor «Ethyl»", new Vector3(14f, 0f, -4f), "Gasolina con plomo (tetraetilo)", 2.5f);
        Prop(station, "Surtidor", new Vector3(0f, 0.8f, 0f), new Vector3(0.6f, 1.6f, 0.5f), red);
        var samples = Items(t, ice, new Vector3(0.3f, 0.4f, 0.3f), ("Agua de mar de la superficie", new Vector3(4f, 0.2f, -6f)), ("Agua de mar profunda", new Vector3(6f, 0.2f, -8f)), ("Hielo de Groenlandia (1753)", new Vector3(8f, 0.2f, -6f)));

        Color coat = new Color(0.92f, 0.92f, 0.9f);
        GameObject cp = Modern(t, "Clair Cameron Patterson", new Vector3(-10f, 0f, 12f), false, coat, CubeInsect.Hat.None, 1.78f);
        GameObject lorraine = Modern(t, "Lorraine Patterson (Ruth)", new Vector3(-4f, 0f, 16f), true, new Color(0.5f, 0.55f, 0.65f));
        GameObject tech = Modern(t, "Técnico de Argonne (Héspero)", new Vector3(12f, 0f, 13f), false, new Color(0.4f, 0.4f, 0.45f));
        GameObject kehoe = Modern(t, "Robert Kehoe (Medea)", new Vector3(16f, 0f, -2f), false, new Color(0.25f, 0.25f, 0.3f), CubeInsect.Hat.Brimmed);
        GameObject attendant = Modern(t, "Gasolinero (Momo)", new Vector3(12f, 0f, -6f), false, new Color(0.3f, 0.4f, 0.55f), CubeInsect.Hat.Cap); Routine(attendant, station);
        int n = 5;

        Mission(t, "Clair Patterson: la edad de la Tierra y el plomo", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("En el laboratorio, habla con Clair Patterson", cp.transform, "Patterson", "Harrison Brown me pidió medir el plomo de rocas antiguas para calcular la edad de la Tierra. Pero el plomo me sale en todas partes: en el polvo, en el vidrio, en mi ropa. El laboratorio entero está contaminado."),
                Visit("Limpia las tres fuentes de contaminación", dirt, "Patterson", "Aire filtrado, todo lavado con ácido, botes de teflón, ropa limpia y nada de polvo. Es el primer laboratorio ultralimpio: sin él, mis números no valen nada."),
                Collect("Lleva la muestra del meteorito al banco limpio", meteor, clean, "Patterson", "Los meteoritos se formaron a la vez que la Tierra. El plomo de este, del Cañón del Diablo (Arizona), casi no tiene uranio: es el plomo primordial."),
                Hold("En Argonne, mide los isótopos del plomo con el espectrómetro", spectro, 12f, "Técnico", "Plomo 206, 207 y 204… La cuenta del uranio que se desintegra da 4550 millones de años."),
                Talk("Habla con Lorraine, su mujer", lorraine.transform, "Lorraine", "Volvió a casa de su madre en Iowa tan nervioso que creyó que le daba un infarto. Es la primera edad fiable de la Tierra."),
                Visit("Mide el plomo en el mar y en el hielo", samples, "Patterson", "En la superficie del mar hay mucho más plomo que en el fondo, y en el hielo de hace dos siglos casi nada. Ese plomo de más lo ha puesto la gente."),
                Talk("En la gasolinera, habla con Robert Kehoe, el científico de la industria", kehoe.transform, "Kehoe", "El plomo en la sangre de la gente es natural, y la gasolina «Ethyl» es segura."),
                Talk("Responde con Patterson", cp.transform, "Patterson", "No es natural: es cien veces más de lo que había antes de la industria. Viene del tetraetilo de plomo de la gasolina. Me quitarán contratos por decirlo, pero lo diré."),
            },
            "Pasadena (California) y el laboratorio de Argonne (Illinois), 1953, sobre el laboratorio de química del santuario a escala de hormiga. El geoquímico Clair Patterson quiere saber la edad de la Tierra y descubre por el camino que el planeta entero está contaminado de plomo. Limpia, mide y discute con él. E para hablar, limpiar y recoger; para medir, quédate quieto.",
            new[]
            {
                "Clair Cameron Patterson (1922–1995) aprendió en Chicago, con Harrison Brown, a medir isótopos de plomo. La contaminación por plomo del ambiente era tan grande que tuvo que crear el primer laboratorio ultralimpio, en Caltech (Pasadena).",
                "En 1953, con el espectrómetro de masas del Laboratorio Nacional de Argonne, midió el plomo del meteorito del Cañón del Diablo y obtuvo la edad de la Tierra: publicada en 1956, 4550 ± 70 millones de años. Es el valor que se sigue aceptando.",
                "Después demostró que el plomo de la superficie del mar y del hielo de Groenlandia había aumentado muchísimo desde la Revolución Industrial, sobre todo por el tetraetilo de plomo de la gasolina (usado desde 1923). En 1965 lo publicó y se enfrentó a la industria y a su científico más conocido, Robert Kehoe, que sostenía que esos niveles eran naturales.",
                "Su trabajo llevó a retirar el plomo de la gasolina: en EE. UU., desde 1975 (catalizadores) hasta su prohibición total en 1996; en el mundo, en 2021. El plomo en la sangre de los estadounidenses bajó más de un 80 %. El plomo daña el cerebro en desarrollo: no hay un nivel seguro.",
            },
            new[]
            {
                "Patterson, C. (1956). «Age of meteorites and the earth». Geochimica et Cosmochimica Acta 10:230–237.",
                "Patterson, C. (1965). «Contaminated and natural lead environments of man». Archives of Environmental Health 11:344–360.",
                "Needleman, H. L. (2000). «Clair Patterson and Robert Kehoe: two views of lead toxicity». Environmental Research 84:20–35.",
                "Caltech Archives, Oral History with Clair C. Patterson (1995); PNUMA (2021), fin de la gasolina con plomo.",
            },
            "Patterson", "Cuando sabes algo que puede hacer daño a todos, callarte también es una decisión.");
        return n;
    }

    // ══ CHERNÓBIL · Ananenko, Bespalov y Baranov, mayo de 1986 · Central Nuclear ═══════════════════════════════════

    static int Chernobyl(Transform t)
    {
        Material concrete = M("Hormigon", 0.55f, 0.55f, 0.53f), black = M("Ruina", 0.15f, 0.15f, 0.15f), water = M("Agua_sotano", 0.25f, 0.35f, 0.38f), steel = M("Acero", 0.55f, 0.57f, 0.6f), rubber = M("Neopreno", 0.12f, 0.12f, 0.14f), hose = M("Manguera", 0.75f, 0.2f, 0.12f);
        Transform ruin = Spot(t, "El bloque 4", new Vector3(0f, 0f, 22f), "El bloque 4 (26 de abril de 1986, 01:23)", 10f);
        Prop(ruin, "Muro roto", new Vector3(-3f, 3f, 2f), new Vector3(4f, 6f, 1f), black);
        Prop(ruin, "Muro roto", new Vector3(3f, 2f, 2f), new Vector3(4f, 4f, 1f), black);
        // (2026-11-01) La ruina del bloque 4 ES la historia (como el muro del gueto de Varsovia): sus muros rotos son la excepción
        // histórica de la regla 2 (sin muros), no un edificio cerrado. Sin esto, «Chernóbil: cumple las reglas» fallaba (grupo 62).
        { var rp = ruin.gameObject.AddComponent<MicroPiece>(); rp.kind = MicroBuildRules.Historic; rp.walkable = true; }
        Transform meet = Spot(t, "Punto de reunión", new Vector3(-12f, 0f, 10f), "Punto de reunión del escuadrón", 2.5f);
        var gear = Items(t, rubber, new Vector3(0.5f, 0.15f, 0.4f), ("Traje de buzo", new Vector3(-16f, 0.08f, 4f)), ("Linterna", new Vector3(-14f, 0.08f, 2f)), ("Dosímetro", new Vector3(-10f, 0.08f, 4f)));
        Transform corridor = Spot(t, "Pasillo inundado", new Vector3(10f, 0f, 10f), "Bajo el reactor: los pasillos inundados", 2.5f);
        Prop(corridor, "Agua", new Vector3(0f, 0.15f, 0f), new Vector3(4f, 0.3f, 12f), water, false);
        Prop(corridor, "Tubería", new Vector3(1.8f, 1f, 0f), new Vector3(0.2f, 0.2f, 12f), steel);
        var valves = Items(t, steel, new Vector3(0.5f, 0.5f, 0.15f), ("Válvula de compuerta 1", new Vector3(11.8f, 1f, 6f)), ("Válvula de compuerta 2", new Vector3(11.8f, 1f, 14f)));
        for (int k = 0; k < 3; k++) Prop(t, "Manguera de los bomberos", new Vector3(-4f + k * 1.2f, 0.05f, 6f), new Vector3(0.15f, 0.1f, 10f), hose, false);
        Transform square = Spot(t, "Plaza de Prípiat", new Vector3(-14f, 0f, -6f), "Prípiat, evacuada el 27 de abril", 3f);
        Prop(square, "Noria (sin estrenar)", new Vector3(-3f, 3f, 0f), new Vector3(0.3f, 6f, 6f), M("Amarillo", 0.9f, 0.75f, 0.2f), false);

        Color overall = new Color(0.85f, 0.85f, 0.82f), fire = new Color(0.25f, 0.25f, 0.28f);
        GameObject ana = Modern(t, "Alekséi Ananenko", new Vector3(-10f, 0f, 12f), false, overall, CubeInsect.Hat.Cap);
        GameObject bes = Modern(t, "Valeri Bespálov", new Vector3(-13f, 0f, 13f), false, overall, CubeInsect.Hat.Cap);
        GameObject bar = Modern(t, "Borís Baránov", new Vector3(-15f, 0f, 11f), false, overall, CubeInsect.Hat.Cap);
        GameObject firefighter = Modern(t, "Bombero de la central (Atlas)", new Vector3(-4f, 0f, 4f), false, fire, CubeInsect.Hat.Spike);
        GameObject dosim = Modern(t, "Dosimetrista (Linduarda)", new Vector3(-6f, 0f, 14f), true, overall, CubeInsect.Hat.Cap);
        GameObject resident = Modern(t, "Vecina de Prípiat (Ruth)", new Vector3(-12f, 0f, -8f), true, new Color(0.55f, 0.4f, 0.45f)); Routine(resident, square);
        int n = 6;

        Mission(t, "Chernóbil, 1986: el escuadrón que bajó al agua", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con Alekséi Ananenko junto al bloque 4", ana.transform, "Ananenko", "El reactor 4 explotó el 26 de abril. Debajo hay agua de los bomberos en la piscina de burbujeo. Si el núcleo fundido llega a ella, el vapor puede romper aún más. Hay que abrir las válvulas para vaciarla. Yo sé dónde están."),
                Talk("Habla con Borís Baránov, el jefe de turno, y con Valeri Bespálov", bar.transform, "Baránov", "Somos tres: Alekséi, que conoce las válvulas; Valeri, ingeniero; y yo, que estaba de turno. Nadie nos obliga. Alguien tiene que hacerlo."),
                Collect("Prepara el equipo: traje, linterna y dosímetro, al punto de reunión", gear, meet, "Dosimetrista", "Os mediré la dosis antes y después. Cuanto menos tiempo estéis abajo, mejor."),
                Talk("Habla con el bombero", firefighter.transform, "Bombero", "Hemos estado bombeando el agua de los pasillos: ya no hay que nadar, el agua os llegará por las rodillas. Las mangueras siguen ahí."),
                Visit("Baja al pasillo inundado y abre las dos válvulas", valves, "Ananenko", "Se nos apagó una linterna y seguimos la tubería a tientas hasta la válvula. Ahora, la otra."),
                Hold("Escucha: el agua de la piscina se va por las tuberías", corridor, 8f, "Bespálov", "¡Se oye correr el agua! Ya está. Vámonos de aquí."),
                Talk("Pregunta a la dosimetrista qué les pasará", dosim.transform, "Dosimetrista", "Han recibido una dosis alta, pero los tres vivirán: no murieron días después, como se cuenta a veces. Borís murió en 2005, del corazón; Alekséi y Valeri siguen vivos y en 2018 recibieron la Orden al Valor de Ucrania."),
                Reach("Ve a la plaza vacía de Prípiat", square, "Vecina", "Nos dijeron que nos íbamos tres días. La noria iba a estrenarse el 1 de mayo. Nunca volvimos."),
            },
            "Prípiat y la central nuclear de Chernóbil (Unión Soviética, hoy Ucrania), principios de mayo de 1986, sobre la central nuclear del santuario a escala de hormiga. Tras la explosión del reactor 4, tres trabajadores de la central se ofrecen para bajar al sótano inundado y abrir unas válvulas. Ayúdales a prepararse y baja con ellos. E para hablar y recoger; para escuchar, quédate quieto.",
            new[]
            {
                "El 26 de abril de 1986, a la 01:23, el reactor RBMK n.º 4 de Chernóbil explotó durante una prueba de seguridad mal planteada en un reactor con defectos de diseño. La ciudad de Prípiat (unos 49 000 habitantes) fue evacuada el 27 de abril. La radiación se extendió por Europa.",
                "Bajo el reactor estaba la piscina de burbujeo, llena de agua de los bomberos. Se temía que el núcleo fundido (corio) la alcanzara y provocara una explosión de vapor. A principios de mayo (las fuentes dan días distintos), los ingenieros Alekséi Ananenko y Valeri Bespálov y el jefe de turno Borís Baránov entraron en los sótanos inundados con trajes de buzo y abrieron las válvulas para vaciarla. Los bomberos ya habían bombeado parte del agua de los pasillos.",
                "Mucha gente cree que murieron poco después: no es así. Baránov murió en 2005 de un problema de corazón; Ananenko y Bespálov seguían vivos en 2018, cuando el presidente de Ucrania les concedió la Orden al Valor. También es exagerada la idea de que, sin ellos, media Europa habría quedado arrasada.",
                "Unas 600 000 personas («liquidadores») trabajaron en limpiar y cubrir el reactor. Según la ONU (UNSCEAR), 28 trabajadores murieron de síndrome de radiación aguda en los meses siguientes, y miles de niños y jóvenes de la zona tuvieron cáncer de tiroides por el yodo radiactivo (casi todos se curaron). El reactor está cubierto por el Nuevo Confinamiento Seguro desde 2016.",
            },
            new[]
            {
                "UNSCEAR (2011). Sources and Effects of Ionizing Radiation, Annex D: Health effects due to radiation from the Chernobyl accident. Naciones Unidas.",
                "IAEA (1992). INSAG-7: The Chernobyl Accident: Updating of INSAG-1.",
                "Leatherbarrow, A. (2016). Chernobyl 01:23:40 (incluye la correspondencia con Alekséi Ananenko).",
                "Plokhy, S. (2018). Chernobyl: The History of a Nuclear Catastrophe. Basic Books.",
            },
            "Ananenko", "No éramos héroes: éramos los que sabían dónde estaban las válvulas.");
        return n;
    }
}
