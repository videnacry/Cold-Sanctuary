using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BEETHOVEN Y EL ESTRENO DE LA NOVENA (docs/microcosmos-historias-3.md). Viena, 7 de mayo de 1824: Beethoven, sordo, estrena la Novena Sinfonía
/// en el Teatro de la Puerta de Carintia. El jugador habla con él ESCRIBIENDO en sus cuadernos de conversación, ayuda con las particellas y
/// está en la sala cuando Caroline Unger lo gira para que vea el aplauso que no oye. Bonn y Heiligenstadt se cuentan en la ficha.
/// </summary>
public static partial class StorySceneBuilder
{
    static int Beethoven(Transform t)
    {
        Material wood = M("Madera_de_cerezo", 0.5f, 0.3f, 0.2f), paper = M("Papel_pautado", 0.9f, 0.86f, 0.72f), black = M("Lacado_negro", 0.08f, 0.08f, 0.09f), red = M("Terciopelo_rojo", 0.55f, 0.1f, 0.12f);
        Material gold = M("Dorado_del_teatro", 0.8f, 0.65f, 0.3f), plaster = M("Yeso_claro", 0.9f, 0.86f, 0.78f), candle = M("Vela", 0.95f, 0.92f, 0.75f), brass = M("Laton_del_cuerno", 0.82f, 0.68f, 0.3f);

        // 1 · EL ESCENARIO del Teatro de la Puerta de Carintia: orquesta, coro y el palco. Sin muros ni techo.
        Transform stage = Spot(t, "Escenario de la Puerta de Carintia", new Vector3(0f, 0f, 16f), "El Teatro de la Puerta de Carintia: 7 de mayo de 1824", 4f);
        Prop(stage, "Tablado", new Vector3(0f, 0.3f, 2f), new Vector3(14f, 0.6f, 6f), wood);
        for (int k = 0; k < 8; k++) Prop(stage, "Atril", new Vector3(-5.5f + k * 1.6f, 1.2f, 1f), new Vector3(0.5f, 1.2f, 0.1f), black);
        for (int k = 0; k < 5; k++) Prop(stage, "Banco del coro", new Vector3(-4f + k * 2f, 0.9f, 4f), new Vector3(1.6f, 0.6f, 0.5f), wood);
        Prop(stage, "Telón rojo", new Vector3(0f, 3f, 5.4f), new Vector3(14f, 4.4f, 0.2f), red, false);
        for (int k = 0; k < 2; k++) Prop(stage, "Pilastra dorada", new Vector3(-7.3f + k * 14.6f, 2.5f, 3f), new Vector3(0.5f, 5f, 0.5f), gold);

        // 2 · LA MESA DE LA UNGARGASSE: el piano Graf, los cuadernos y la corneta acústica.
        Transform home = Spot(t, "Mesa de Beethoven", new Vector3(-15f, 0f, 9f), "Su mesa en la Ungargasse: el piano, la partitura y los cuadernos", 3f);
        Prop(home, "Piano", new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 1f, 1.2f), black);
        Prop(home, "Partitura", new Vector3(0.2f, 1.03f, 0f), new Vector3(1f, 0.04f, 0.7f), paper, false);
        Prop(home, "Mesa de los cuadernos", new Vector3(3.2f, 0.4f, 0f), new Vector3(1.4f, 0.8f, 0.8f), wood);
        Prop(home, "Corneta acústica", new Vector3(3.5f, 0.88f, 0f), new Vector3(0.4f, 0.1f, 0.14f), brass, false);

        // 3 · LAS TRES PARTICELLAS que hay que llevar del copista al teatro.
        Transform copy = Spot(t, "Copistería", new Vector3(16f, 0f, 8f), "La copistería: las particellas de la orquesta", 2.4f);
        Prop(copy, "Mesa de copiar", new Vector3(0f, 0.4f, 0f), new Vector3(2.6f, 0.8f, 1.1f), wood);
        var parts = Items(t, paper, new Vector3(0.6f, 0.1f, 0.5f), ("Particellas de los violines", new Vector3(20f, 0.5f, 2f)), ("Particellas de las voces", new Vector3(18f, 0.5f, -3f)), ("Particellas de la trompa", new Vector3(13f, 0.5f, -6f)));

        // 4 · LOS TRES MÚSICOS que hay que encontrar.
        Transform sontag = Spot(t, "Sala de Henriette Sontag", new Vector3(-13f, 0f, -6f), "Henriette Sontag ensaya las notas altas", 2f);
        Prop(sontag, "Pianoforte de ensayo", new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.7f), wood);
        Transform unger = Spot(t, "Sala de Caroline Unger", new Vector3(-6f, 0f, -11f), "Caroline Unger: «escribe para cantar»", 2f);
        Prop(unger, "Silla de ensayo", new Vector3(0f, 0.4f, 0f), new Vector3(0.7f, 0.8f, 0.7f), red);
        Transform umlauf = Spot(t, "Atril de Umlauf", new Vector3(9f, 0f, -11f), "Michael Umlauf, que dirigirá de verdad", 2f);
        Prop(umlauf, "Atril del director", new Vector3(0f, 0.6f, 0f), new Vector3(0.7f, 1.2f, 0.2f), black);
        var artists = new List<Transform> { sontag, unger, umlauf };

        // 5 · LA MESA DE LA CENA: lo que sobró (la taquilla fue un desastre).
        Transform ledger = Spot(t, "Mesa de las cuentas", new Vector3(14f, 0f, 16f), "Las cuentas del concierto", 2f);
        Prop(ledger, "Mesa de las cuentas", new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.8f, 0.8f), wood);
        Prop(ledger, "Vela", new Vector3(0.4f, 0.95f, 0f), new Vector3(0.08f, 0.3f, 0.08f), candle, false);
        Prop(t, "Barandilla del patio de butacas", new Vector3(0f, 0.5f, 8f), new Vector3(10f, 1f, 0.2f), plaster);

        // La gente: insectos-humanos. Beethoven, un GRILLO (oye por las patas: la música, por la vibración), de levita verde oscuro.
        Color green = new Color(0.16f, 0.26f, 0.2f), black2 = new Color(0.1f, 0.1f, 0.12f), rose = new Color(0.75f, 0.5f, 0.55f), blue = new Color(0.3f, 0.36f, 0.55f), brown = new Color(0.32f, 0.24f, 0.18f);
        GameObject lv = Modern(t, "Ludwig van Beethoven", new Vector3(-11f, 0f, 6f), false, green, CubeInsect.Hat.None, 1.65f, "Cricket");
        GameObject schindler = Modern(t, "Anton Schindler", new Vector3(3f, 0f, 9f), false, black2, CubeInsect.Hat.None, 1.72f, "Spider");
        GameObject caroline = Modern(t, "Caroline Unger", new Vector3(-5f, 0f, -8f), true, rose, CubeInsect.Hat.None, 1.6f, "SnowBee");
        GameObject henriette = Modern(t, "Henriette Sontag", new Vector3(-11f, 0f, -3f), true, blue, CubeInsect.Hat.None, 1.58f, "Cicada");
        GameObject michael = Modern(t, "Michael Umlauf", new Vector3(10f, 0f, -8f), false, black2, CubeInsect.Hat.None, 1.7f, "Mantis");
        GameObject copyist = Modern(t, "Copista (Atlas)", new Vector3(14f, 0f, 5f), false, brown, CubeInsect.Hat.None, 1.66f, "Ant"); Routine(copyist, copy);
        int n = 6;

        Mission(t, "Beethoven en Viena: el estreno de la Novena (1824)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con Anton Schindler", schindler.transform, "Schindler", "El maestro ya no oye nada. Para hablarle hay que escribir en su cuaderno: él contesta en voz alta. Hoy tiene el peor humor de su vida y la mejor música. Ve a verlo: toma, lápiz."),
                Talk("Escríbele a Beethoven en su cuaderno", lv.transform, "Beethoven", "¿Qué pone? ¿Que si estoy listo? Treinta músicos y mecenas de Viena me firmaron una carta para que no me fuera a estrenar a Berlín. Si esta ciudad la quiere, que la tenga. Que traigan las partes de una vez."),
                Collect("Lleva las tres particellas al teatro", parts, stage, "Copista", "Las hemos copiado a mano, de noche, con la tinta húmeda. Hay errores, seguro. Y los solistas se quejan: las notas altas son imposibles."),
                Visit("Busca a Sontag, a Unger y a Umlauf", artists, "Caroline Unger", "Henriette tiene dieciocho años y yo veinte. Le dije a la cara que es un tirano de las voces. Se rió, y no cambió ni una nota. Umlauf ha dicho a la orquesta que le mire a él, y no al maestro."),
                Talk("Habla con Michael Umlauf", michael.transform, "Umlauf", "Él estará a mi lado marcando el compás. Lo llevará dentro, adelantado o atrasado, y yo llevaré la orquesta. Nadie va a decirle nada. Es lo menos que podemos hacer."),
                Hold("Escucha la Novena desde el patio de butacas", stage, 14f, "Schindler", "Es la primera vez que una sinfonía lleva coro. Oye eso: «Freude…». Y él, ahí, de espaldas a la sala, sigue la partitura en silencio."),
                Reach("Ve al escenario: la música ha terminado", stage, "Caroline Unger", "Ha acabado y él sigue pasando páginas. No oye la sala. La sala está en pie, y agita sombreros y pañuelos porque saben que no oye. Déjame… lo giro con cuidado."),
                Talk("Ve a ver las cuentas con Schindler", schindler.transform, "Schindler", "Quedó casi nada, una vez pagados el teatro y los copistas. Se ha puesto furioso con todos. Pero mira lo que has oído: lo escribió sin oír una nota."),
            },
            "Viena, el 7 de mayo de 1824, sobre la Sala de Ballet del santuario a escala de hormiga (pendiente de su piso: una sala de música y conciertos). Ludwig van Beethoven, completamente sordo, va a estrenar su última sinfonía. Ayuda a que el concierto llegue a tiempo y escúchalo. E para hablar y recoger; para escuchar, quédate quieto.",
            new[]
            {
                "Ludwig van Beethoven nació en Bonn, en diciembre de 1770 (lo bautizaron el 17), en una familia de músicos de la corte. Con 21 años se fue a Viena para estudiar con Haydn y ya no volvió. Desde hacia 1798 empezó a perder el oído. En octubre de 1802, en Heiligenstadt, a las afueras de Viena, escribió a sus hermanos una carta que nunca envió, el Testamento de Heiligenstadt: confiesa su desesperación y su decisión de seguir viviendo por su arte.",
                "Hacia 1818 ya necesitaba cuadernos de conversación para que le hablasen por escrito; se conservan unos 140, con solo la mitad de cada conversación. Dependía de ayudantes como Anton Schindler, que luego falsificó algunas entradas: lo que él contó debe leerse con cautela. Compuso sus últimas grandes obras sin oírlas.",
                "En febrero de 1824, un grupo de músicos y mecenas de Viena le pidió por carta que estrenara allí su nueva sinfonía. El 7 de mayo, en el Teatro de la Puerta de Carintia, se estrenó la Novena Sinfonía, la primera gran sinfonía con coro y solistas, con la Oda a la alegría de Schiller. Dirigió Michael Umlauf, con Beethoven a su lado; cantaron Henriette Sontag (18 años) y Caroline Unger (20). Según varios testimonios, que difieren en el momento, Unger lo giró para que viera el aplauso que no oía. Las ganancias fueron muy pequeñas.",
                "Beethoven murió en Viena el 26 de marzo de 1827. La melodía de la Oda a la alegría es hoy el himno de la Unión Europea, y el manuscrito de la Novena es Memoria del Mundo de la UNESCO. Sobre «golpear con la mandíbula el piano para oírlo», los testimonios son dudosos. Las frases son de juego.",
            },
            new[]
            {
                "Thayer, A. W. (ed. Forbes, 1967). Thayer's Life of Beethoven. Princeton University Press.",
                "Albrecht, T. (2024). Beethoven's Ninth Symphony: Rehearsing and Performing Its 1824 Premiere. Boydell & Brewer.",
                "Solomon, M. (1998). Beethoven. Schirmer.",
                "History Today, «Beethoven's Heiligenstadt Testament» (historytoday.com).",
                "WETA, «May 7, 1824: 200th Anniversary Premiere of Beethoven's Symphony No. 9» (weta.org).",
            },
            "Beethoven", "No oigo el mundo. Lo que ha sonado hoy lo llevaba dentro.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style ViennaPlaster = new Style(C(0.9f, 0.84f, 0.66f), C(0.55f, 0.42f, 0.3f), C(0.55f, 0.32f, 0.25f), Roof.Gable, 4f);   // revoco amarillo vienés y teja
    static readonly Style ViennaStone = new Style(C(0.84f, 0.8f, 0.72f), C(0.62f, 0.56f, 0.48f), C(0.4f, 0.42f, 0.42f), Roof.Gable, 5f);     // piedra de los edificios mayores

    // VIENA (1824): la ciudad dentro de sus murallas (el Ring aún no existe), el glacis alrededor, la Puerta de Carintia al sur con el teatro, la
    // Kärntner Strasse hasta San Esteban, el canal del Danubio al noreste, el Prater al este, la Ungargasse en el suburbio de Landstrasse al
    // sureste. Plano de Vasquez (1830) simplificado y ACORTADO (ojo: Heiligenstadt queda fuera, al norte; en la ficha). (Wien Geschichte Wiki.)
    const string PlanViena = @"
muralla Murallas de Viena | - | -520 40, -440 600, 60 860, 620 640, 760 200, 560 -120, 120 -220, -360 -160, -520 40
rio Canal del Danubio | 40 | -1000 1100, 200 960, 900 500, 1300 100
rio Río Wien | 16 | -1300 -520, -300 -300, 520 -280, 900 250
calle Kärntner Strasse | 10 | 80 -80, 120 420
calle Graben y Kohlmarkt | 10 | 120 420, -140 500
calle Calle de San Esteban | 8 | 120 420, 460 460
calle Ungargasse | 8 | 680 -120, 1100 -800
calle Landstrasser Hauptstrasse | 10 | 820 180, 1300 -600
calle Camino del glacis | 8 | -700 -320, 900 -220
calle Wiedner Hauptstrasse | 10 | 80 -220, -100 -1200
calle Mariahilfer Strasse | 10 | -520 40, -1400 -240
calle Calle de Josefstadt | 8 | -500 380, -1300 500
campo Glacis (prados y paseos) | 500 90 | 80 -300
campo Prater | 420 460 | 1250 760
campo Viñas de los suburbios | 340 220 | -1000 -1000
hito 0 | 60 90
hito 1 | 950 -480
hito 2 | 300 120
hito 3 | -200 -560
hito 4 | 240 220
hito 5 | 180 460
hito 6 | -200 300
hito 7 | 420 300
Plaza | 40 400
Pileta | -100 420
Mercado | -180 150
Parque | 480 -300
Baile | 240 560
Estudio | -300 520
Entrenamiento | -350 180
Sanacion | -760 520
Piscina | 860 600
Museo | -60 620
Investigacion | 380 620
Hotel | 140 240
Bomberos | -320 60
Policia | 520 360
Taller | -500 -480
Carpinteria | -860 -300
Mecanica | 640 -560
Estacion | 560 -640
Guarderia | -620 -640
Edificio | -60 240
Obra | -860 200
relleno | 60 | 200 300";

    static int VienaTown(Transform t)
    {
        var s = Town(t, "Viena 1824", new[]
        {
            new Lot("Teatro de la Puerta de Carintia", "Sala de baile", "Teatro de la Puerta de Carintia (Kärntnertortheater)", 20f, 14f, ViennaStone,
                S("pista", -4f, 2f, 6f, 6f, "Escenario"), S("mesa", 5f, 3f, 1.6f, 1f, "Foso de la orquesta"), S("banco", 0f, -5f, 4f, 1.6f, "Butacas")),
            new Lot("Casa de la Ungargasse", "Casa", "Casa de Beethoven (Ungargasse 5, invierno de 1823–1824)", 14f, 10f, ViennaPlaster,
                S("cama", -3f, 2f, 1.4f, 2f, "Cama de Beethoven"), S("consola", 3f, 2f, 1.6f, 0.8f, "Piano Graf"), S("estante", 3f, -3f, 0.8f, 2f, "Partituras y cuadernos de conversación")),
            new Lot("Copistería", "Taller de papeles", "Copistería de música", 12f, 10f, ViennaPlaster,
                S("mesa", -2f, 2f, 2f, 1f, "Mesa de los copistas"), S("armario", 3f, -3f, 1.2f, 0.6f, "Papel pautado y tinta")),
            new Lot("Taller de Streicher", "Taller", "Taller de pianos de Nannette Streicher", 16f, 12f, ViennaPlaster,
                S("banco_taller", -4f, 2f, 2.4f, 1f, "Banco del piano"), S("torno", 2f, 2f, 1.6f, 1f, "Torno de patas"), S("armario", 0f, -4f, 1.6f, 0.6f, "Cuerdas y martillos")),
            new Lot("Café de la Kärntner Strasse", "Restaurante", "Café y mesón de la ciudad", 14f, 10f, ViennaPlaster,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Cocina (goulash, café)"), S("mesa", 2f, -1f, 2f, 1f, "Mesa con periódicos"), S("barra", 3f, 3f, 2f, 0.8f, "Barra del café")),
            new Lot("Catedral de San Esteban", "Templo", "Catedral de San Esteban", 18f, 12f, ViennaStone,
                S("altar", 0f, 4f, 2.4f, 1f, "Altar mayor"), S("banco", -3f, -2f, 2f, 0.6f, "Banco"), S("banco", 3f, -2f, 2f, 0.6f, "Banco")),
            new Lot("Sociedad de Amigos de la Música", "Escuela", "Gesellschaft der Musikfreunde (conservatorio, 1817)", 16f, 12f, ViennaStone,
                S("pizarra", 0f, 4f, 2.4f, 0.4f, "Pizarra pautada"), S("consola", -4f, 0f, 1.6f, 0.8f, "Pianoforte de clase"), S("estante", 5f, -3f, 0.8f, 2f, "Archivo de partituras")),
            new Lot("Casa de Sontag y Unger", "Casa", "Casa de las cantantes", 14f, 10f, ViennaPlaster,
                S("cama", -3f, 2f, 1.2f, 2f, "Cama de Henriette Sontag"), S("cama", 3f, 2f, 1.2f, 2f, "Cama de Caroline Unger")),
        }, new[]
        {
            new Lot("Casa de Viena", "Casa", "Casa de vecinos de Viena", 11f, 9f, ViennaPlaster),
            new Lot("Tienda de la ciudad", "Mercado", "Tienda de la ciudad", 10f, 8f, ViennaPlaster, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller de violines", "Taller", "Taller del violero", 10f, 8f, ViennaPlaster, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del violero"), S("armario", 0f, 0f, 1.2f, 0.6f, "Maderas y barnices")),
            new Lot("Huerto del suburbio", "Huerto", "Huerto y viña del suburbio", 12f, 10f, ViennaPlaster, S("bancal", 0f, 0f, 4f, 6f, "Hortalizas"), S("pozo", 0f, 0f, 1f, 1f, "Pozo")),
        }, C(0.5f, 0.47f, 0.42f));
        int n = 0;
        n += W(t, "Violinista de la orquesta", s[0], "", s[4], null, null);
        n += W(t, "Cantante del coro", s[0], "", s[4], null, null);
        n += W(t, "Copista de música", s[2], "", s[4], null, null);
        n += W(t, "Nannette Streicher", s[3], "", s[4], null, null);
        n += W(t, "Camarera del café", s[4], "cocinero", s[4], null, null);
        n += W(t, "Maestra del conservatorio", s[6], "sabio", s[4], null, null);
        n += W(t, "Alumno del conservatorio", s[6], "alumno", s[4], s[6], null);
        n += Townsfolk(t, s, 8, "Vecina de Viena", "Vecino de Viena", s[4], s[5]);
        return n;
    }
}

public static partial class MicroBiome
{
    // Viena, mayo: castaños en flor en el Prater, tilos de paseo, sauces y álamos del Danubio, lilas en los jardines y viñas en los suburbios.
    // Abejorros, abejas, escarabajo de mayo, hormigas. Caballos de los coches y perros. (Fischer 2008, Exkursionsflora von Österreich.)
    static Biome VienaBiome() => new Biome { title = "Viena (mayo de 1824)", ground = C(0.45f, 0.4f, 0.32f), plants = new[] {
            new Plant(Kind.Shrub, 6, "Sauce del Danubio (Salix alba)", 1800, C(0.45f, 0.55f, 0.4f), C(0.55f, 0.5f, 0.4f)),
            new Plant(Kind.Shrub, 6, "Álamo (Populus nigra)", 2200, C(0.35f, 0.52f, 0.3f), C(0.5f, 0.45f, 0.35f)),
            new Plant(Kind.Grass, 12, "Prado del glacis", 25, C(0.42f, 0.62f, 0.3f), C(0.55f, 0.6f, 0.3f)),
            new Plant(Kind.Flower, 5, "Diente de león (Taraxacum)", 20, C(0.35f, 0.55f, 0.28f), C(0.95f, 0.8f, 0.15f)),
            new Plant(Kind.Stone, 8, "Empedrado", 0, C(0.6f, 0.58f, 0.55f), Stone) },
          town = new[] {
            new Plant(Kind.Shrub, 6, "Castaño de Indias (Aesculus hippocastanum), en flor", 1800, C(0.28f, 0.48f, 0.26f), C(0.95f, 0.92f, 0.85f)),
            new Plant(Kind.Shrub, 5, "Tilo (Tilia platyphyllos), de paseo", 1800, C(0.3f, 0.5f, 0.28f), C(0.75f, 0.7f, 0.4f)),
            new Plant(Kind.Shrub, 3, "Lila (Syringa vulgaris)", 300, C(0.3f, 0.48f, 0.28f), C(0.7f, 0.5f, 0.8f)),
            new Plant(Kind.Grass, 8, "Vid de los suburbios (Vitis vinifera)", 150, C(0.35f, 0.5f, 0.28f), C(0.5f, 0.55f, 0.3f)) },
          bugs = new[] {
            new Bug("DungBeetle", 3, 28, C(0.42f, 0.28f, 0.18f), "Escarabajo de mayo (Melolontha melolontha)"),
            new Bug("SnowBee", 4, 15, C(0.8f, 0.6f, 0.2f), "Abeja de los castaños (Apis mellifera)"),
            new Bug("Ant", 4, 5, C(0.25f, 0.18f, 0.14f), "Hormiga negra (Lasius niger)") },
          townFauna = new[] {
            new Pet("Aphid", 3, 2f, C(0.45f, 0.35f, 0.28f), "Caballo del coche (en el microcosmos, un pulgón de tiro)"),
            new Pet("Woodlouse", 3, 0.7f, C(0.6f, 0.5f, 0.4f), "Perro de la ciudad (en el microcosmos, una cochinilla hormiguera)") } };
}
