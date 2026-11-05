using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DESCARTES Y EL DISCURSO DEL MÉTODO (docs/microcosmos-historias-3.md). Leiden, 8 de junio de 1637: en la imprenta de Jan Maire se acaba de
/// imprimir, sin nombre de autor y en francés, el Discurso del método con sus tres ensayos. La historia enseña el método (dudar, dividir,
/// ordenar, repasar) y las coordenadas con una rejilla en el suelo. Las frases son de juego; las citas, solo la del «pienso, luego existo».
/// </summary>
public static partial class StorySceneBuilder
{
    static int Descartes(Transform t)
    {
        Material oak = M("Roble_holandes", 0.42f, 0.28f, 0.18f), paper = M("Pliego_de_imprenta", 0.92f, 0.88f, 0.76f), ink = M("Tinta", 0.08f, 0.08f, 0.1f), brick = M("Ladrillo_de_Leiden", 0.55f, 0.28f, 0.2f);
        Material water = M("Agua_del_Rapenburg", 0.3f, 0.42f, 0.42f), tile = M("Azulejo_de_estufa", 0.75f, 0.8f, 0.85f), brass = M("Laton_del_astrolabio", 0.8f, 0.65f, 0.3f), grid = M("Rejilla_blanca", 0.9f, 0.9f, 0.86f), red = M("Punto_rojo", 0.75f, 0.15f, 0.12f);

        // 1 · LA IMPRENTA DE JAN MAIRE: la prensa de madera y los pliegos tendidos.
        Transform press = Spot(t, "Prensa de Jan Maire", new Vector3(14f, 0f, 12f), "La prensa de Jan Maire: el Discurso, sin nombre de autor", 3f);
        Prop(press, "Bancada de la prensa", new Vector3(0f, 0.6f, 0f), new Vector3(2f, 1.2f, 1.2f), oak);
        for (int k = 0; k < 2; k++) Prop(press, "Montante", new Vector3(-0.9f + k * 1.8f, 1.5f, 0f), new Vector3(0.25f, 3f, 0.25f), oak);
        Prop(press, "Cabezal", new Vector3(0f, 2.9f, 0f), new Vector3(2.2f, 0.3f, 0.4f), oak);
        Prop(press, "Pliegos tendidos", new Vector3(0f, 2.3f, -1.6f), new Vector3(3.2f, 0.02f, 0.8f), paper, false);
        var essays = Items(t, paper, new Vector3(0.6f, 0.12f, 0.45f), ("Pliegos de la Dióptrica", new Vector3(19f, 0.5f, 5f)), ("Pliegos de los Meteoros", new Vector3(21f, 0.5f, 0f)), ("Pliegos de la Geometría", new Vector3(16f, 0.5f, -4f)));

        // 2 · LA REJILLA: las coordenadas en el suelo de la plaza.
        Transform plane = Spot(t, "Rejilla de la Geometría", new Vector3(-14f, 0f, 3f), "Dos números dicen dónde está un punto", 2.6f);
        for (int k = 0; k < 7; k++) { Prop(plane, "Línea x", new Vector3(-3f + k, 0.02f, 0f), new Vector3(0.05f, 0.04f, 6f), grid, false); Prop(plane, "Línea z", new Vector3(0f, 0.02f, -3f + k), new Vector3(6f, 0.04f, 0.05f), grid, false); }
        Prop(plane, "Punto (2, 1)", new Vector3(2f, 0.15f, 1f), new Vector3(0.3f, 0.3f, 0.3f), red, false);

        // 3 · LAS CUATRO REGLAS: una mesa por regla, en la orilla del canal.
        Transform doubt = Spot(t, "Primera regla: no aceptar nada dudoso", new Vector3(-16f, 0f, 14f), "Regla 1: no aceptar nada que no sea evidente", 2f);
        Prop(doubt, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 0.7f), oak);
        Transform divide = Spot(t, "Segunda regla: dividir", new Vector3(-8f, 0f, 16f), "Regla 2: dividir cada dificultad en partes", 2f);
        Prop(divide, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 0.7f), oak);
        Transform order = Spot(t, "Tercera regla: ordenar", new Vector3(0f, 0f, 16f), "Regla 3: ir de lo simple a lo complejo", 2f);
        Prop(order, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 0.7f), oak);
        Transform review = Spot(t, "Cuarta regla: repasar", new Vector3(8f, 0f, 16f), "Regla 4: repasar para no omitir nada", 2f);
        Prop(review, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 0.7f), oak);
        var rules = new List<Transform> { doubt, divide, order, review };
        Prop(t, "Agua del Rapenburg", new Vector3(-23f, 0.02f, 8f), new Vector3(3f, 0.04f, 26f), water, false);

        // 4 · LA ESTUFA: la habitación caldeada de 1619.
        Transform stove = Spot(t, "Estufa", new Vector3(-9f, 0f, -9f), "La estufa: dudar de todo, sentado y solo", 2.4f);
        Prop(stove, "Estufa de azulejos", new Vector3(0f, 1f, 1f), new Vector3(1.6f, 2f, 1f), tile);
        Prop(stove, "Silla", new Vector3(0f, 0.3f, -0.8f), new Vector3(0.7f, 0.6f, 0.7f), oak);

        // 5 · LA UNIVERSIDAD: el astrolabio del Observatorio.
        Transform uni = Spot(t, "Mesa del Observatorio", new Vector3(11f, 0f, -9f), "El Observatorio de la Universidad de Leiden (1633)", 2.2f);
        Prop(uni, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.6f, 0.8f, 0.8f), oak);
        Prop(uni, "Cuadrante", new Vector3(0f, 1.1f, 0f), new Vector3(0.7f, 0.6f, 0.08f), brass, false);
        Prop(t, "Fachada de ladrillo", new Vector3(21f, 0.6f, 14f), new Vector3(0.4f, 1.2f, 8f), brick);
        Prop(t, "Letras de prueba", new Vector3(13f, 1.25f, 14f), new Vector3(0.8f, 0.1f, 0.5f), ink, false);

        // La gente: insectos-humanos. Descartes, una ARAÑA (teje sola una red de líneas rectas: las coordenadas), de negro con cuello blanco.
        Color black = new Color(0.1f, 0.1f, 0.12f), white = new Color(0.9f, 0.9f, 0.86f), brown = new Color(0.36f, 0.26f, 0.2f), blue = new Color(0.25f, 0.3f, 0.45f), wine = new Color(0.4f, 0.14f, 0.18f);
        GameObject rene = Modern(t, "René Descartes", new Vector3(3f, 0f, 9f), false, black, CubeInsect.Hat.Brimmed, 1.6f, "Spider");
        GameObject maire = Modern(t, "Jan Maire", new Vector3(11f, 0f, 9f), false, brown, CubeInsect.Hat.Cap, 1.7f, "Ant");
        GameObject schooten = Modern(t, "Frans van Schooten", new Vector3(-11f, 0f, 1f), false, wine, CubeInsect.Hat.None, 1.72f, "Cricket"); Routine(schooten, plane);
        GameObject helena = Modern(t, "Helena Jans", new Vector3(-6f, 0f, -7f), true, blue, CubeInsect.Hat.Bonnet, 1.6f, "Ant");
        GameObject student = Modern(t, "Estudiante de Leiden (Momo)", new Vector3(9f, 0f, -6f), false, brown, CubeInsect.Hat.None, 1.7f, "Grasshopper"); Routine(student, uni, rules[0]);
        int n = 5;

        Mission(t, "Descartes en Leiden: el Discurso del método (1637)", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con René Descartes en la orilla", rene.transform, "Descartes", "Hoy se acaba de imprimir. Sin mi nombre: quien quiera, que juzgue las razones y no al autor. Y en francés, no en latín: lo he escrito para quien use su propio juicio, no para los que solo creen a los libros viejos. Ve con Maire."),
                Collect("Lleva a la prensa los pliegos de los tres ensayos", essays, press, "Jan Maire", "La Dióptrica, los Meteoros y la Geometría. El método y sus pruebas, en un solo tomo. Él no cobra: se queda con doscientos ejemplares para repartir. Y tiene mal genio con las erratas."),
                Visit("Recorre las cuatro reglas del método", rules, "Descartes", "No aceptar nada que no sea evidente. Dividir cada dificultad en partes. Ir de lo simple a lo complejo. Y repasar, para estar seguro de no olvidar nada. Solo cuatro: no necesito más."),
                Reach("Ve a la rejilla con Frans van Schooten", plane, "Frans van Schooten", "Este es el truco de la Geometría: dos números dicen dónde está un punto. Una curva es una ecuación, y una ecuación es una curva. Yo dibujé las figuras del libro. Mira: dos a la derecha, una arriba."),
                Talk("Habla con Helena Jans", helena.transform, "Helena Jans", "Trabajaba en casa de su librero en Ámsterdam. Tenemos una hija, Francine, de dos años. Él quiere que un libro lo pueda leer hasta una mujer como yo. Con eso me basta para saber que lo dice en serio."),
                Hold("Siéntate junto a la estufa y duda de todo", stove, 12f, "Descartes", "Hace años, en una habitación caldeada, me propuse dudar de todo: los sentidos me engañan, los sueños me engañan. ¿Y si todo es falso? Pero para dudar tengo que estar pensando. Y si pienso, existo."),
                Talk("Habla con el estudiante", student.transform, "Estudiante", "Aquí, en el Observatorio y en la sala de anatomía, se mira y se mide. Los profesores no se fían del libro nuevo, que contradice a Aristóteles. A mí me gusta: me dice que puedo comprobarlo yo."),
                Talk("Vuelve con Descartes", rene.transform, "Descartes", "«Pienso, luego existo». Una sola cosa cierta, y desde ella se reconstruye todo. Ahora te toca a ti: no me creas. Compruébalo."),
            },
            "Leiden, en las Provincias Unidas, el 8 de junio de 1637, sobre el Aula del santuario a escala de hormiga. René Descartes acaba de imprimir, sin firmar, el Discurso del método. Ayuda en la imprenta y aprende sus reglas. E para hablar y recoger; para dudar, quédate quieto.",
            new[]
            {
                "René Descartes nació en La Haye (hoy Descartes), en Turena, en 1596. Estudió con los jesuitas en La Flèche. En 1618, en Breda, conoció a Isaac Beeckman, que le contagió la física con matemáticas, y la noche del 10 de noviembre de 1619, en Alemania, tuvo los tres sueños que lo decidieron a reformar todo el saber. Desde 1628 vivió sobre todo en las Provincias Unidas, un país donde se podía pensar con más libertad, cambiando a menudo de casa.",
                "El Discurso del método para conducir bien la razón y buscar la verdad en las ciencias se terminó de imprimir en Leiden, en casa de Jan Maire, el 8 de junio de 1637, sin nombre de autor y en francés, con tres ensayos: la Dióptrica, los Meteoros y la Geometría. Es la Geometría la que une el álgebra y la geometría con lo que hoy llamamos coordenadas cartesianas. Sus cuatro reglas siguen siendo una guía para resolver problemas.",
                "«Pienso, luego existo» (je pense, donc je suis) aparece en la cuarta parte: se puede dudar de todo menos de que quien duda está pensando. Con Helena Jans, sirvienta en Ámsterdam, tuvo una hija, Francine, nacida en 1635, que murió a los cinco años; él lo vivió como el mayor dolor de su vida. No consta que Helena estuviera en Leiden ese día.",
                "Sus ideas tuvieron problemas: en 1633 había dejado sin publicar El mundo al saber de la condena de Galileo, y en Utrecht y Leiden sus enemigos le prohibieron enseñar. Murió en Estocolmo en febrero de 1650, en la corte de la reina Cristina. Sus teorías tienen límites (el dualismo mente-cuerpo y la idea de que los animales son máquinas se discuten mucho hoy).",
            },
            new[]
            {
                "Descartes, R. (1637). Discours de la méthode. Leiden: Jan Maire.",
                "Hatfield, G. «René Descartes». Stanford Encyclopedia of Philosophy (plato.stanford.edu/entries/descartes).",
                "Gaukroger, S. (1995). Descartes: An Intellectual Biography. Oxford University Press.",
                "Watson, R. (2002). Cogito, Ergo Sum: The Life of René Descartes. David R. Godine.",
                "Sotheby's, «Descartes, Discours de la méthode, first edition» (privilegio del 4 de mayo y final de impresión del 8 de junio de 1637).",
            },
            "Descartes", "No basta con tener buen ingenio: lo principal es aplicarlo bien.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style LeidenBrick = new Style(C(0.58f, 0.3f, 0.22f), C(0.6f, 0.55f, 0.48f), C(0.5f, 0.28f, 0.2f), Roof.Gable, 4.2f);   // ladrillo holandés y teja
    static readonly Style LeidenPublic = new Style(C(0.72f, 0.62f, 0.5f), C(0.65f, 0.6f, 0.52f), C(0.32f, 0.34f, 0.38f), Roof.Gable, 5f);  // edificios de la ciudad, con piedra

    // LEIDEN (1637): el óvalo de la ciudad con sus fosos (Singel), los dos brazos del Rin que se unen en el Burcht, el canal Rapenburg al oeste con
    // la Academia y el Hortus, la Breestraat (la calle principal), la Pieterskerk, el Ayuntamiento y el mercado. Plano de Blaeu (1649), simplificado.
    // (Erfgoed Leiden; Lunsingh Scheurleer et al. 1975, Het Rapenburg.)
    const string PlanLeiden = @"
muralla Foso y murallas (Singel) | - | -760 -220, -760 620, -200 900, 700 900, 1050 400, 1000 -240, 300 -520, -400 -480, -760 -220
rio Oude Rijn | 30 | -1300 240, -400 300, 150 330
rio Nieuwe Rijn | 30 | 1300 380, 600 400, 150 330
rio Rapenburg | 24 | -60 -440, -30 -100, -60 300
rio Oude Vest | 16 | -400 720, 500 720
calle Breestraat | 12 | -60 120, 640 200
calle Calle Pieterskerk | 8 | -30 -120, 480 -80
calle Calle Rapenburg oeste | 8 | -110 -420, -110 300
calle Calle Rapenburg este | 8 | 10 -420, 10 300
calle Hooigracht | 8 | 300 -300, 700 280
calle Haarlemmerstraat | 10 | -300 520, 900 560
calle Calle del Burcht | 6 | 150 330, 200 520
colina Burcht (la mota) | 60 22 | 170 400
campo Hortus botanicus | 110 90 | -210 -170
campo Prados del Rin | 500 180 | -1000 -880
hito 0 | 100 40
hito 1 | -120 -40
hito 2 | -220 180
hito 3 | 240 -30
hito 4 | 360 170
hito 5 | 280 -140
hito 6 | -240 -190
hito 7 | -320 40
Plaza | 500 240
Pileta | 560 90
Mercado | 620 300
Parque | -500 -340
Estudio | 420 -240
Entrenamiento | -320 -320
Investigacion | -130 90
Sanacion | -180 340
Policia | 320 280
Hotel | 150 170
Bomberos | 700 110
Guarderia | 760 420
Monumento | 240 480
Taller | 700 -140
Carpinteria | 40 620
Obra | 840 180
Estacion | -560 430
Mecanica | 880 500
Edificio | 420 40
relleno | 60 | 300 200";

    static int LeidenTown(Transform t)
    {
        var s = Town(t, "Leiden", new[]
        {
            new Lot("Imprenta de Jan Maire", "Taller", "Imprenta de Jan Maire", 16f, 12f, LeidenBrick,
                S("banco_taller", -4f, 2f, 2.4f, 1f, "Prensa"), S("mesa", 3f, 2f, 2f, 1f, "Caja de tipos"), S("estante", 0f, -4f, 1.6f, 0.6f, "Pliegos secándose")),
            new Lot("Academiegebouw", "Escuela", "Universidad de Leiden (Academiegebouw)", 18f, 12f, LeidenPublic,
                S("pizarra", 0f, 4f, 2.4f, 0.4f, "Pizarra"), S("mesa", -3f, 0f, 2.4f, 1f, "Bancos de los estudiantes"), S("estante", 5f, -3f, 0.8f, 2f, "Libros de la Biblioteca")),
            new Lot("Casa de René Descartes", "Casa", "Casa alquilada de René Descartes", 12f, 10f, LeidenBrick,
                S("cama", -2f, 2f, 1.4f, 2f, "Cama-armario de Descartes"), S("mesa", 3f, 2f, 1.6f, 1f, "Mesa de escribir"), S("estante", 3f, -3f, 0.8f, 1.6f, "Lentes pulidas y papeles")),
            new Lot("Casa de Van Schooten", "Casa", "Casa de Frans van Schooten", 12f, 10f, LeidenBrick,
                S("cama", -2f, 2f, 1.4f, 2f, "Cama-armario"), S("mesa", 3f, 2f, 1.6f, 1f, "Mesa de dibujar figuras")),
            new Lot("Posada de la Breestraat", "Restaurante", "Posada de la Breestraat (arenque y cerveza)", 14f, 10f, LeidenBrick,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Cocina de la posada"), S("mesa", 2f, -1f, 2f, 1f, "Mesa larga"), S("barra", 3f, 3f, 2f, 0.8f, "Barriles")),
            new Lot("Pieterskerk", "Templo", "Pieterskerk", 18f, 12f, LeidenPublic,
                S("altar", 0f, 4f, 2.4f, 1f, "Púlpito"), S("banco", -3f, -2f, 2f, 0.6f, "Banco"), S("banco", 3f, -2f, 2f, 0.6f, "Banco")),
            new Lot("Hortus botanicus", "Huerto", "Hortus botanicus (el jardín de la Universidad, 1590)", 16f, 12f, LeidenPublic,
                S("bancal", -4f, 0f, 4f, 6f, "Plantas medicinales"), S("bancal", 2f, 0f, 4f, 6f, "Plantas de las Indias"), S("pozo", 6f, 3f, 1.2f, 1.2f, "Pozo")),
            new Lot("Theatrum anatomicum", "Laboratorio", "Teatro anatómico de Leiden (1594)", 14f, 12f, LeidenPublic,
                S("mesa_laboratorio", 0f, 2f, 2.4f, 1f, "Mesa del teatro anatómico"), S("banco", 0f, -3f, 4f, 1.6f, "Gradas"), S("estante", 5f, -3f, 0.8f, 2f, "Colección de curiosidades")),
        }, new[]
        {
            new Lot("Casa de Leiden", "Casa", "Casa estrecha de ladrillo", 10f, 9f, LeidenBrick),
            new Lot("Taller de paños de Leiden", "Taller", "Taller de paños", 12f, 10f, LeidenBrick, S("telar", 0f, 0f, 2f, 1f, "Telar de paño"), S("estante", 0f, 0f, 0.8f, 1.6f, "Lana")),
            new Lot("Librería de Leiden", "Mercado", "Librería", 10f, 8f, LeidenBrick, S("puesto", 0f, 0f, 2f, 1f, "Mostrador de libros"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Tienda de la ciudad", "Mercado", "Tienda de quesos y arenques", 10f, 8f, LeidenBrick, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Quesos")),
        }, C(0.5f, 0.45f, 0.4f));
        int n = 0;
        n += W(t, "Cajista de la imprenta", s[0], "", s[4], null, null);
        n += W(t, "Profesor de matemáticas", s[1], "sabio", s[4], null, s[3]);
        n += W(t, "Estudiante de medicina", s[1], "alumno", s[4], s[1], null);
        n += W(t, "Cocinera de la posada", s[4], "cocinero", s[4], null, null);
        n += W(t, "Jardinero del Hortus", s[6], "hortelano", s[4], null, null);
        n += W(t, "Disector del teatro anatómico", s[7], "sabio", s[4], null, null);
        n += Townsfolk(t, s, 8, "Vecina de Leiden", "Vecino de Leiden", s[4], s[5]);
        return n;
    }
}

public static partial class MicroBiome
{
    // Leiden, junio: el verde de los pólderes. Olmos y tilos a lo largo de los canales, sauces podados, carrizo y lirio amarillo en las orillas,
    // prado de vacas. Abejorros, mariquitas, hormigas. Vacas, caballos de sirga, gatos. (Weeda et al. 1985, Nederlandse oecologische flora.)
    static Biome LeidenBiome() => new Biome { title = "Leiden, Holanda (junio de 1637)", ground = C(0.38f, 0.36f, 0.28f), plants = new[] {
            new Plant(Kind.Shrub, 6, "Sauce podado (Salix alba)", 700, C(0.42f, 0.55f, 0.38f), C(0.5f, 0.45f, 0.38f)),
            new Plant(Kind.Grass, 12, "Carrizo (Phragmites australis)", 250, C(0.45f, 0.58f, 0.32f), C(0.55f, 0.45f, 0.4f)),
            new Plant(Kind.Flower, 5, "Lirio amarillo (Iris pseudacorus)", 90, C(0.35f, 0.55f, 0.32f), C(0.95f, 0.82f, 0.15f)),
            new Plant(Kind.Grass, 12, "Prado del pólder", 25, C(0.4f, 0.62f, 0.3f), C(0.55f, 0.6f, 0.3f)),
            new Plant(Kind.Stone, 6, "Adoquín de ladrillo", 0, C(0.55f, 0.32f, 0.25f), Stone) },
          town = new[] {
            new Plant(Kind.Shrub, 6, "Olmo holandés (Ulmus minor), del canal", 1800, C(0.3f, 0.5f, 0.28f), C(0.5f, 0.42f, 0.32f)),
            new Plant(Kind.Shrub, 5, "Tilo (Tilia × europaea), del canal", 1800, C(0.3f, 0.5f, 0.28f), C(0.75f, 0.7f, 0.4f)),
            new Plant(Kind.Flower, 3, "Rosa de jardín (Rosa gallica)", 100, C(0.28f, 0.45f, 0.25f), C(0.8f, 0.3f, 0.4f)) },
          bugs = new[] {
            new Bug("SnowBee", 4, 18, C(0.8f, 0.6f, 0.2f), "Abejorro (Bombus terrestris)"),
            new Bug("Ladybug", 3, 7, C(0.8f, 0.2f, 0.12f), "Mariquita de siete puntos"),
            new Bug("Ant", 4, 5, C(0.25f, 0.18f, 0.14f), "Hormiga negra (Lasius niger)") },
          townFauna = new[] {
            new Pet("Aphid", 3, 2f, C(0.45f, 0.35f, 0.28f), "Caballo de la barca de sirga (en el microcosmos, un pulgón de tiro)"),
            new Pet("Aphid", 3, 2f, C(0.9f, 0.88f, 0.84f), "Vaca del pólder (en el microcosmos, un pulgón)"),
            new Pet("Woodlouse", 3, 0.5f, C(0.6f, 0.55f, 0.5f), "Gato de la imprenta (en el microcosmos, una cochinilla hormiguera)") } };
}
