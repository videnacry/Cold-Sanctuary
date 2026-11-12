using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GALVARINO VUELVE CON SU GENTE (docs/microcosmos-historias-3.md). Un lof mapuche de las tierras de Arauco, noviembre de 1557: Galvarino,
/// a quien los españoles castigaron cortándole las manos tras la batalla de Lagunillas, vuelve con los suyos. Se cuenta con dignidad y SIN
/// IMÁGENES DE DAÑO NI COMBATE: la historia va de la machi que lo cura, de la gente que lo cuida y de la palabra que da en el koyag.
/// Lo que pasó después (Millarapue) se narra, no se juega. Las frases son de juego; los hechos, de La Araucana y las crónicas.
/// </summary>
public static partial class StorySceneBuilder
{
    static int Galvarino(Transform t)
    {
        Material wood = M("Roble_pellin", 0.42f, 0.3f, 0.2f), straw = M("Ratonera_de_la_ruka", 0.72f, 0.62f, 0.38f), wool = M("Lana_mapuche", 0.5f, 0.12f, 0.12f), black = M("Lana_negra", 0.1f, 0.1f, 0.12f);
        Material leaf = M("Hoja_de_canelo", 0.22f, 0.42f, 0.25f), stone = M("Piedra_de_Nahuelbuta", 0.52f, 0.5f, 0.46f), clay = M("Greda_del_metawe", 0.6f, 0.42f, 0.3f), water = M("Agua_del_estero", 0.3f, 0.5f, 0.55f);

        // 1 · LA RUKA DE LA MACHI y su REWE: el tronco escalonado junto al que cura, con ramas de canelo (foye) y el kultrun.
        Transform machiSpot = Spot(t, "Rewe de la machi", new Vector3(-12f, 0f, 10f), "El rewe de la machi (el canelo y el kultrun)", 3.2f);
        Prop(machiSpot, "Rewe (tronco escalonado)", new Vector3(0f, 1.3f, 0f), new Vector3(0.5f, 2.6f, 0.5f), wood);
        for (int k = 0; k < 3; k++) Prop(machiSpot, "Rama de canelo", new Vector3(0.2f, 2.2f + k * 0.2f, -0.2f + k * 0.2f), new Vector3(0.9f, 0.1f, 0.25f), leaf, false);
        Prop(machiSpot, "Kultrun (tambor)", new Vector3(1.3f, 0.3f, 0f), new Vector3(0.6f, 0.3f, 0.6f), wood);
        Prop(machiSpot, "Estera de curación", new Vector3(-1.8f, 0.03f, 0f), new Vector3(1.2f, 0.06f, 2.2f), straw, false);

        // 2 · EL LAWEN: las tres plantas que trae el jugador.
        Transform pot = Spot(t, "Olla del lawen", new Vector3(-16f, 0f, 3f), "La olla del lawen (el remedio)", 2f);
        Prop(pot, "Fuego", new Vector3(0f, 0.1f, 0f), new Vector3(1f, 0.2f, 1f), stone, false);
        Prop(pot, "Metawe (olla de greda)", new Vector3(0f, 0.45f, 0f), new Vector3(0.7f, 0.6f, 0.7f), clay);
        var herbs = Items(t, leaf, new Vector3(0.5f, 0.4f, 0.5f), ("Hojas de canelo (foye)", new Vector3(-20f, 0.2f, 12f)), ("Hojas de matico", new Vector3(-22f, 0.2f, 5f)), ("Corteza de maqui", new Vector3(-19f, 0.2f, -3f)));

        // 3 · LA RUKA DE GALVARINO y lo que hace su gente: el fuego, el telar y la mesa común.
        Transform ruka = Spot(t, "Ruka de Galvarino", new Vector3(12f, 0f, 12f), "La ruka de Galvarino", 3.4f);
        Prop(ruka, "Kütral (el fuego de la ruka)", new Vector3(0f, 0.1f, 0f), new Vector3(1.2f, 0.2f, 1.2f), stone, false);
        for (int k = 0; k < 2; k++) Prop(ruka, "Horcón", new Vector3(-3f + k * 6f, 1.4f, 3f), new Vector3(0.3f, 2.8f, 0.3f), wood);
        Prop(ruka, "Cumbrera de ratonera", new Vector3(0f, 2.9f, 3f), new Vector3(7f, 0.25f, 1.6f), straw, false);
        Transform loom = Spot(t, "Telar de la ruka", new Vector3(17f, 0f, 5f), "El witral: su hermana le teje el makuñ", 2.4f);
        Prop(loom, "Witral (telar vertical)", new Vector3(0f, 1f, 0f), new Vector3(2f, 2f, 0.2f), wood);
        Prop(loom, "Makuñ a medio tejer", new Vector3(0f, 0.9f, -0.14f), new Vector3(1.5f, 1.2f, 0.04f), wool, false);
        Transform meal = Spot(t, "Comida común", new Vector3(19f, 0f, -3f), "La comida común: se come junto, nadie sobra", 2.4f);
        Prop(meal, "Fuente de madera (rali) con catutos", new Vector3(0f, 0.15f, 0f), new Vector3(1.4f, 0.3f, 0.9f), wood);
        Transform field = Spot(t, "Chacra del lof", new Vector3(13f, 0f, -9f), "La chacra: kinwa, papas y porotos", 2.4f);
        Prop(field, "Surcos", new Vector3(0f, 0.1f, 0f), new Vector3(3.6f, 0.2f, 2.4f), M("Tierra_del_lof", 0.3f, 0.22f, 0.16f), false);
        var care = new List<Transform> { loom, meal, field };

        // 4 · EL KOYAG: el campo de reunión de los lonkos, con el estero al lado.
        Transform koyag = Spot(t, "Lepün del koyag", new Vector3(-4f, 0f, 19f), "El koyag: los lonkos escuchan en círculo", 4f);
        for (int k = 0; k < 8; k++) { float a = k * Mathf.PI / 4f; Prop(koyag, "Asiento de madera (wanco)", new Vector3(Mathf.Cos(a) * 4.2f, 0.2f, Mathf.Sin(a) * 4.2f), new Vector3(0.6f, 0.4f, 0.6f), wood); }
        Prop(koyag, "Canelo del lepün", new Vector3(0f, 1.6f, 0f), new Vector3(0.4f, 3.2f, 0.4f), wood);
        Prop(t, "Estero", new Vector3(-23f, 0.02f, 16f), new Vector3(3f, 0.04f, 16f), water, false);

        // La gente: insectos-humanos. Galvarino, un GRILLO (puede perder una pata y sigue saltando y cantando): lleva mangas largas de lana
        // que cubren sus brazos; no se muestra ninguna herida.
        Color red = new Color(0.5f, 0.12f, 0.12f), blackC = new Color(0.1f, 0.1f, 0.12f), blue = new Color(0.2f, 0.25f, 0.45f), brown = new Color(0.42f, 0.32f, 0.24f);
        GameObject hero = Ancient(t, "Galvarino", new Vector3(9f, 0f, 9f), false, red, false, 1.68f, "Cricket");
        GameObject machi = Ancient(t, "Machi del lof (Ruth)", new Vector3(-10f, 0f, 7f), true, blackC, false, 1.58f, "Earwig"); Routine(machi, machiSpot, pot);
        GameObject sister = Ancient(t, "Hermana de Galvarino (Medea)", new Vector3(15f, 0f, 7f), true, blue, false, 1.56f, "Ant"); Routine(sister, loom, meal);
        GameObject caupolican = Ancient(t, "Caupolicán", new Vector3(-7f, 0f, 16f), false, blackC, false, 1.8f, "DungBeetle");
        GameObject elder = Ancient(t, "Kimche del lof (Atlas)", new Vector3(3f, 0f, 3f), false, brown, true, 1.6f, "Cicada");
        GameObject boy = Ancient(t, "Niño del lof (Momo)", new Vector3(10f, 0f, -5f), false, brown, false, 1.3f, "Cricket"); Routine(boy, field, meal);
        int n = 6;

        Mission(t, "Galvarino vuelve con su gente (1557)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con la machi junto al rewe", machi.transform, "Machi", "Ha vuelto Galvarino. Los españoles lo castigaron y lo soltaron para que su cuerpo fuera un aviso. Pero aquí su cuerpo no es un aviso: es de los nuestros. Ayúdame a preparar el lawen."),
                Collect("Trae canelo, matico y maqui a la olla del lawen", herbs, pot, "Machi", "El canelo es el árbol de nuestra gente. El matico cierra. El maqui da fuerza. Y hace falta lo que no está en la olla: que no esté solo."),
                Talk("Visita a Galvarino en su ruka", hero.transform, "Galvarino", "Aún no sé atarme el trarilonko ni encender el fuego. Me da vergüenza pedirlo. Y luego pienso que nadie aquí lo hizo nunca solo."),
                Visit("Ayuda con lo que hace su gente: el telar, la comida y la chacra", care, "Hermana de Galvarino", "Ahora le tejemos, le partimos el catuto y sembramos su parte de la chacra. Él les cuenta a los niños lo que vio. En un lof nadie sobra."),
                Talk("Habla con el niño del lof", boy.transform, "Niño", "Galvarino me enseña a mirar el río para saber cuándo lloverá. ¡No hacen falta manos para mirar!"),
                Reach("Ve al koyag, donde se reúnen los lonkos", koyag, "Caupolicán", "Se dice que algunos quieren rendirse. Antes de decidir, que hable quien más ha perdido. Así se hace aquí: se escucha a todos."),
                Hold("Escucha a Galvarino en el círculo", koyag, 12f, "Galvarino", "No vengo a enseñaros lo que me hicieron. Vengo a deciros que sigo siendo Galvarino. Esta tierra sigue aquí, y mis palabras, y vosotros. Cuidad de ella y cuidaos entre vosotros."),
                Talk("Habla con el kimche, que guarda lo que pasó", elder.transform, "Kimche", "Volvió a estar con los suyos en la batalla de Millarapue y allí acabó su camino. Hasta un soldado español, el poeta Ercilla, escribió su nombre con respeto. Nosotros no lo recordamos por lo que perdió, sino por lo que no quiso dejar."),
            },
            "Un lof mapuche de las tierras de Arauco, en la primavera de 1557, sobre el Hospital del santuario a escala de hormiga. Galvarino ha vuelto con su gente tras la batalla de Lagunillas. Ayuda a la machi, conoce a quienes lo cuidan y escucha lo que dice en el koyag, la reunión de los lonkos. E para hablar, recoger y ayudar; para escuchar, quédate quieto.",
            new[]
            {
                "Desde 1541 los españoles entraron en el Wallmapu, el país mapuche, al sur del Bío-Bío. Los mapuche no tenían rey: se organizaban en lof, y en tiempos de peligro los lonkos se reunían y elegían a un toki. Resistieron más de trescientos años; pocos pueblos de América conservaron así su tierra.",
                "En 1557 llegó el nuevo gobernador, García Hurtado de Mendoza. En la batalla de Lagunillas, en septiembre o noviembre de ese año (las fuentes difieren), los españoles capturaron a varios mapuche; como castigo les cortaron las manos, y a Galvarino también, y lo soltaron para que sirviera de aviso. Según el poema La Araucana, de Alonso de Ercilla, soldado presente en la campaña, Galvarino volvió con los suyos y pidió al toki Caupolicán y a los lonkos que siguieran defendiendo su tierra.",
                "Volvió a estar con ellos en la batalla de Millarapue (30 de noviembre de 1557). Lo capturaron de nuevo y lo ejecutaron; las crónicas difieren en cómo. Ercilla cuenta que intentó salvarle la vida y que Galvarino no quiso. Lo que sabemos de él viene casi todo de autores españoles, y su nombre es una forma española; para el pueblo mapuche es un símbolo de dignidad, y hoy una comuna de La Araucanía lleva su nombre.",
                "Esta historia no muestra heridas ni combates. Las escenas del lof, la machi y el koyag son una recreación respetuosa de cómo vivía su gente, no una crónica de esos días. El pueblo mapuche sigue vivo y sus reclamos sobre el Wallmapu siguen abiertos.",
            },
            new[]
            {
                "Ercilla, A. de (1569–1589). La Araucana, cantos XXII, XXIII y XXVI.",
                "Góngora Marmolejo, A. de (1575). Historia de todas las cosas que han acaecido en el Reino de Chile.",
                "Bengoa, J. (2003). Historia de los antiguos mapuches del sur. Catalonia.",
                "Museo Chileno de Arte Precolombino, «Pueblos originarios: Mapuche» (precolombino.cl).",
                "Biografías y Vidas, «Galvarino» (biografiasyvidas.com).",
            },
            "Kimche", "Mari mari, peñi. Lo que no se pudo cortar sigue caminando por esta tierra.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style RukaStraw = new Style(C(0.6f, 0.5f, 0.34f), C(0.42f, 0.32f, 0.24f), C(0.72f, 0.62f, 0.38f), Roof.Gable, 2.8f);   // ruka: coligüe y ratonera, suelo de tierra

    // UN LOF DE ARAUCO (1557): rukas sueltas entre el bosque y los esteros, sin calles: senderos. El golfo de Arauco al oeste, el río
    // Carampangue al norte y la cordillera de Nahuelbuta al este. Plano APROXIMADO e inventado en su detalle: no hay plano del lof, y se
    // conoce el paisaje (Bengoa 2003; OpenStreetMap, provincia de Arauco). Lagunillas queda fuera, al norte.
    const string PlanLagunillas = @"
mar Golfo de Arauco | 700 2800 | -1750 0
rio Río Carampangue | 26 | 1400 1000, 500 860, -400 980, -1400 1080
rio Estero del lof | 8 | 900 -500, 300 -80, -130 180, -500 620, -700 950
monte Cordillera de Nahuelbuta | 800 900 | 1900 200
colina Loma del canelo | 220 60 | 520 520
colina Loma del lof | 200 45 | -520 -520
campo Chacras del lof (kinwa, papas y porotos) | 300 200 | 260 -420
campo Bosque de robles | 420 300 | 900 -850
calle Sendero de las rukas | 4 | -280 40, 300 90
calle Sendero del estero | 4 | -130 180, 100 -220
calle Sendero de la costa | 4 | -280 40, -1100 -100
calle Sendero de Nahuelbuta | 4 | 300 90, 1000 300
calle Sendero de la chacra | 4 | 100 -220, 300 -420
calle Sendero del paliwe | 4 | -100 -60, -480 -300
hito 0 | 140 120
hito 1 | -150 120
hito 2 | 40 240
hito 3 | -60 210
hito 4 | 230 40
hito 5 | 260 -240
hito 6 | 260 180
hito 7 | -240 -120
Plaza | -360 180
Parque | -380 480
Mercado | 60 -320
Pileta | -40 -180
Entrenamiento | -480 -300 | actual: canchas de palín de las comunidades de Arauco; posición aproximada
Baile | -300 330
Estudio | 370 300
Guarderia | 110 -120
Carpinteria | 430 -40
Taller | -130 -340
Obra | 520 180
Estacion | -560 830
relleno | 34 | 100 60";

    static int LagunillasTown(Transform t)
    {
        var s = Town(t, "Lagunillas", new[]
        {
            new Lot("Ruka de Galvarino", "Casa", "Ruka de Galvarino y su familia", 14f, 12f, RukaStraw,
                S("cama", -4f, 2f, 1.2f, 2f, "Lecho de esteras de Galvarino"), S("cama", 0f, 2f, 1.2f, 2f, "Lecho de su hermana"), S("fogon", 4f, 2f, 1.6f, 0.8f, "Kütral (el fuego de la ruka)")),
            new Lot("Ruka de la machi", "Hospital", "Ruka de la machi (curar con lawen y kultrun)", 14f, 12f, RukaStraw,
                S("camilla", -3f, 2f, 1f, 2f, "Estera de curación"), S("armario", 3f, 2f, 1.2f, 0.6f, "Lawen: canelo, matico, maqui y boldo"), S("altar", 0f, -3f, 1.6f, 1f, "Rewe de la machi"), S("lavabo", 4f, -3f, 1f, 1f, "Metawe de agua limpia")),
            new Lot("Ruka del lonko", "Casa", "Ruka del lonko", 14f, 12f, RukaStraw,
                S("cama", -2f, 2f, 1.4f, 2f, "Lecho del lonko"), S("estante", 3f, 2f, 1.2f, 0.6f, "Chaiwe (cestos) y makuñ")),
            new Lot("Lepün del lonko", "Plaza", "Campo del koyag (reunión de los lonkos)", 22f, 18f, RukaStraw,
                S("banco", -5f, 0f, 4f, 1.6f, "Tronco para sentarse"), S("banco", 5f, 0f, 4f, 1.6f, "Tronco para sentarse"), S("fuente", 0f, 4f, 4.4f, 4.4f, "Vertiente del lepün"), S("estatua", 0f, -4f, 2.2f, 2.2f, "Chemamull (figura de madera)")),
            new Lot("Ruka de la comida", "Cocina", "Ruka de la comida (catutos, muday y charki)", 14f, 10f, RukaStraw,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Metawe sobre el fuego"), S("mesa", 2f, -1f, 2f, 1f, "Rali (fuentes de madera) sobre la estera"), S("granero", 4f, 3f, 2f, 2f, "Piedra de moler (kudi)")),
            new Lot("Chacra del lof", "Huerto", "Chacra del lof (kinwa, papas y porotos)", 16f, 12f, RukaStraw,
                S("bancal", -4f, 0f, 4f, 6f, "Kinwa y papas"), S("bancal", 2f, 0f, 4f, 6f, "Porotos y zapallo"), S("pozo", 6f, 3f, 1.2f, 1.2f, "Acequia del estero")),
            new Lot("Ruka de las tejedoras", "Taller", "Ruka de las tejedoras (witral y lana de chilihueque)", 14f, 10f, RukaStraw,
                S("telar", -3f, 2f, 2f, 1f, "Witral (telar vertical)"), S("telar", 3f, 2f, 2f, 1f, "Witral"), S("estante", 0f, -3f, 1.2f, 0.6f, "Lana teñida")),
            new Lot("Corral del chilihueque", "Granero", "Corral del chilihueque y troje del lof", 14f, 10f, RukaStraw,
                S("granero", -3f, 2f, 2f, 2f, "Troje de kinwa y papas"), S("estante", 3f, 2f, 1.2f, 0.6f, "Chaiwe de semillas")),
        }, new[]
        {
            new Lot("Ruka del lof", "Casa", "Ruka del lof", 12f, 10f, RukaStraw),
            new Lot("Taller de cestos", "Taller", "Taller de cestos de quilineja", 10f, 8f, RukaStraw, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco de la cestera"), S("estante", 0f, 0f, 0.8f, 1.6f, "Llepü (cestos planos)")),
            new Lot("Alfarería del lof", "Alfarería", "Alfarería (metawe y ketru metawe)", 10f, 8f, RukaStraw, S("torno_alfarero", 0f, 0f, 1f, 1f, "Moldear a mano la greda")),
            new Lot("Chacra familiar", "Huerto", "Chacra familiar", 12f, 10f, RukaStraw, S("bancal", 0f, 0f, 4f, 6f, "Papas y porotos"), S("pozo", 0f, 0f, 1f, 1f, "Vertiente")),
        }, C(0.42f, 0.34f, 0.26f));
        int n = 0;
        n += W(t, "Ayudante de la machi", s[1], "", s[4], s[1], s[2]);
        n += W(t, "Lonko del lof", s[3], "", s[4], null, s[2]);
        n += W(t, "Cocinera del lof", s[4], "cocinero", s[4], null, s[2]);
        n += W(t, "Hortelana de la chacra", s[5], "hortelano", s[4], null, null);
        n += W(t, "Tejedora del witral", s[6], "", s[4], null, null);
        n += W(t, "Pastor de chilihueques", s[7], "", s[4], null, null);
        n += Townsfolk(t, s, 8, "Vecina del lof", "Vecino del lof", s[4], s[1]);
        return n;
    }
}

public static partial class MicroBiome
{
    // Arauco, primavera austral: el bosque templado del sur de Chile. Canelo (foye, el árbol sagrado), roble pellín, arrayán, peumo, quila,
    // helechos y copihue; ciervo volante, moscardón nativo, cigarra chilena. Chilihueque y perros en el lof. (Rodríguez et al. 2018, Flora de
    // Chile; Peña 1986, Introducción a los insectos de Chile.)
    static Biome LagunillasBiome() => new Biome { title = "Arauco, bosque templado del sur (primavera de 1557)", ground = C(0.3f, 0.24f, 0.18f), plants = new[] {
            new Plant(Kind.Shrub, 7, "Roble pellín (Nothofagus obliqua)", 2200, C(0.3f, 0.48f, 0.26f), C(0.42f, 0.32f, 0.24f)),
            new Plant(Kind.Shrub, 6, "Arrayán (Luma apiculata)", 900, C(0.24f, 0.42f, 0.24f), C(0.7f, 0.42f, 0.28f)),
            new Plant(Kind.Shrub, 6, "Quila (Chusquea quila)", 400, C(0.42f, 0.56f, 0.3f), C(0.55f, 0.6f, 0.3f)),
            new Plant(Kind.Flower, 4, "Copihue (Lapageria rosea)", 180, C(0.24f, 0.42f, 0.24f), C(0.8f, 0.12f, 0.2f)),
            new Plant(Kind.Fern, 8, "Helecho costilla de vaca (Blechnum chilense)", 90, C(0.28f, 0.5f, 0.28f), C(0.35f, 0.3f, 0.2f)),
            new Plant(Kind.Grass, 12, "Pasto del claro", 40, C(0.42f, 0.58f, 0.3f), C(0.55f, 0.55f, 0.3f)),
            new Plant(Kind.Stone, 8, "Piedra de Nahuelbuta", 0, C(0.52f, 0.5f, 0.46f), Stone) },
          town = new[] {
            new Plant(Kind.Shrub, 6, "Canelo (Drimys winteri), el foye del rewe", 900, C(0.22f, 0.42f, 0.25f), C(0.92f, 0.92f, 0.85f)),
            new Plant(Kind.Shrub, 4, "Maqui (Aristotelia chilensis), en el borde de la chacra", 300, C(0.25f, 0.45f, 0.22f), C(0.15f, 0.1f, 0.2f)),
            new Plant(Kind.Shrub, 3, "Matico (Buddleja globosa)", 250, C(0.35f, 0.5f, 0.3f), C(0.95f, 0.65f, 0.15f)),
            new Plant(Kind.Grass, 8, "Kinwa (Chenopodium quinoa), de la chacra", 120, C(0.45f, 0.55f, 0.28f), C(0.7f, 0.5f, 0.3f)) },
          bugs = new[] {
            new Bug("DungBeetle", 3, 60, C(0.42f, 0.3f, 0.2f), "Ciervo volante (Chiasognathus grantii)"),
            new Bug("SnowBee", 4, 25, C(0.75f, 0.42f, 0.15f), "Moscardón nativo (Bombus dahlbomii)"),
            new Bug("Cicada", 3, 25, C(0.25f, 0.25f, 0.2f), "Cigarra chilena (Tettigades)"),
            new Bug("Ant", 5, 8, C(0.2f, 0.15f, 0.12f), "Hormiga negra (Camponotus morosus)") },
          townFauna = new[] {
            new Pet("Aphid", 3, 1.8f, C(0.85f, 0.8f, 0.7f), "Chilihueque (en el microcosmos, un pulgón de lana)"),
            new Pet("Woodlouse", 2, 0.7f, C(0.5f, 0.4f, 0.3f), "Perro del lof (en el microcosmos, una cochinilla hormiguera)") } };
}
