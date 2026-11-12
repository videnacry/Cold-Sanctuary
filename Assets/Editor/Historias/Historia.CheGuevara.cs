using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ERNESTO GUEVARA Y ALBERTO GRANADO EN SAN PABLO (docs/microcosmos-historias-3.md). La colonia de leprosos de San Pablo (Loreto, Perú), junio
/// de 1952: dos jóvenes argentinos, un estudiante de medicina y un bioquímico, al final de su viaje por Sudamérica, trabajan con los enfermos.
/// Solo el lado humanista: ningún arma, nada de su vida posterior salvo una línea de la ficha, que dice lo discutido sin juicio.
/// Las frases son de juego; los hechos, de los diarios de los dos (Notas de viaje; Con el Che por Sudamérica) y de Anderson (1997).
/// </summary>
public static partial class StorySceneBuilder
{
    static int CheGuevara(Transform t)
    {
        Material wood = M("Tablas_de_la_colonia", 0.5f, 0.38f, 0.26f), palm = M("Hoja_de_irapay", 0.6f, 0.55f, 0.3f), white = M("Blanco_de_botiquin", 0.9f, 0.9f, 0.86f), water = M("Agua_del_Amazonas", 0.5f, 0.42f, 0.28f);
        Material green = M("Hoja_selva", 0.22f, 0.45f, 0.24f), ball = M("Cuero_del_balon", 0.5f, 0.32f, 0.2f), log = M("Palo_de_balsa", 0.8f, 0.72f, 0.55f), rope = M("Soga", 0.65f, 0.55f, 0.35f);

        // 1 · EL DISPENSARIO: el hospital de la colonia, con su mesa, sin guantes.
        Transform clinic = Spot(t, "Dispensario de la colonia", new Vector3(-13f, 0f, 9f), "El dispensario: curas sin guantes", 3f);
        Prop(clinic, "Plataforma sobre pilotes", new Vector3(0f, 0.3f, 0f), new Vector3(7f, 0.6f, 5f), wood);
        for (int k = 0; k < 4; k++) Prop(clinic, "Pilote", new Vector3(-3f + (k % 2) * 6f, 1.5f, -2f + (k / 2) * 4f), new Vector3(0.3f, 3f, 0.3f), wood);
        Prop(clinic, "Mesa de curas", new Vector3(0f, 1f, 0f), new Vector3(2.2f, 0.8f, 1f), white);
        Transform lab = Spot(t, "Mesa de Granado", new Vector3(-17f, 0f, 0f), "Las muestras de Alberto Granado", 2.2f);
        Prop(lab, "Microscopio", new Vector3(0f, 0.95f, 0f), new Vector3(0.3f, 0.5f, 0.3f), ball, false);
        Prop(lab, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.8f, 0.8f, 0.9f), wood);
        var meds = Items(t, white, new Vector3(0.4f, 0.4f, 0.4f), ("Frasco de sulfonas", new Vector3(-21f, 0.2f, 6f)), ("Vendas limpias", new Vector3(-19f, 0.2f, 13f)), ("Agua hervida", new Vector3(-22f, 0.2f, -3f)));

        // 2 · LA VIDA DE LA COLONIA: la cancha, la chacra y la escuela.
        Transform pitch = Spot(t, "Cancha de la colonia", new Vector3(13f, 0f, 9f), "La cancha: Ernesto juega de portero", 3f);
        for (int k = 0; k < 2; k++) Prop(pitch, "Palo de la portería", new Vector3(-1.2f + k * 2.4f, 1f, 3f), new Vector3(0.15f, 2f, 0.15f), wood);
        Prop(pitch, "Balón de trapo y cuero", new Vector3(0f, 0.2f, 0f), new Vector3(0.4f, 0.4f, 0.4f), ball, false);
        Transform plot = Spot(t, "Chacra de yuca", new Vector3(18f, 0f, -2f), "La chacra: los enfermos cultivan su comida", 2.4f);
        Prop(plot, "Surcos de yuca", new Vector3(0f, 0.1f, 0f), new Vector3(3.6f, 0.2f, 2.6f), green, false);
        Transform school = Spot(t, "Escuela de la colonia", new Vector3(10f, 0f, -9f), "La escuela: aquí enseñaron a leer a una paciente", 2.4f);
        Prop(school, "Banco de la escuela", new Vector3(0f, 0.3f, 0f), new Vector3(2.4f, 0.6f, 0.7f), wood);
        var life = new List<Transform> { pitch, plot, school };

        // 3 · LA ORILLA: el río que separa a médicos y enfermos.
        Transform bank = Spot(t, "Orilla del Amazonas", new Vector3(0f, 0f, 20f), "La orilla: de un lado los médicos; del otro, los enfermos", 3f);
        Prop(bank, "Amazonas", new Vector3(0f, 0.02f, 5f), new Vector3(46f, 0.04f, 8f), water, false);

        // 4 · LA FIESTA y 5 · LA BALSA.
        Transform party = Spot(t, "Mesa de la fiesta", new Vector3(-5f, 0f, -9f), "La fiesta: 14 de junio, 24 años", 2.4f);
        Prop(party, "Mesa larga", new Vector3(0f, 0.4f, 0f), new Vector3(3.2f, 0.8f, 1f), wood);
        Transform raft = Spot(t, "Balsa Mambo-Tango", new Vector3(-12f, 0f, 17f), "La balsa Mambo-Tango, regalo de los enfermos", 2.4f);
        for (int k = 0; k < 5; k++) Prop(raft, "Tronco de balsa", new Vector3(-1.2f + k * 0.6f, 0.2f, 0f), new Vector3(0.5f, 0.4f, 4f), log, false);
        Prop(raft, "Soga de la balsa", new Vector3(0f, 0.45f, 0f), new Vector3(3.2f, 0.05f, 0.1f), rope, false);

        // La gente: insectos-humanos. Guevara, una LANGOSTA-SALTAMONTES (cruza un continente entero); Granado, un GRILLO. Ropa de viaje gastada.
        Color khaki = new Color(0.55f, 0.5f, 0.36f), blue = new Color(0.3f, 0.35f, 0.45f), whiteC = new Color(0.9f, 0.9f, 0.86f), pale = new Color(0.72f, 0.66f, 0.55f);
        GameObject ernesto = Who(t, "Ernesto Guevara", new Vector3(3f, 0f, 9f), false, 1.74f, "Grasshopper"); CubeInsect.WearShirt(ernesto, khaki);
        GameObject alberto = Who(t, "Alberto Granado", new Vector3(-14f, 0f, 3f), false, 1.62f, "Cricket"); CubeInsect.WearShirt(alberto, blue); Routine(alberto, lab, clinic);
        GameObject director = Modern(t, "Director de la colonia (Atlas)", new Vector3(-10f, 0f, 6f), false, whiteC, CubeInsect.Hat.None, 1.7f, "Earwig");
        GameObject patient = Who(t, "Paciente músico (Momo)", new Vector3(-3f, 0f, -6f), false, 1.66f, "Ant"); CubeInsect.WearShirt(patient, pale); Routine(patient, party, bank);
        GameObject patient2 = Who(t, "Paciente hortelana (Ruth)", new Vector3(16f, 0f, -4f), true, 1.56f, "Ant"); CubeInsect.WearShirt(patient2, pale); Routine(patient2, plot, school);
        int n = 5;

        Mission(t, "Ernesto Guevara y Alberto Granado en San Pablo (1952)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con el director de la colonia", director.transform, "Director", "Han llegado dos argentinos con una carta del doctor Pesce, de Lima. Un bioquímico y un casi médico, sin un centavo. Aquí tenemos a seiscientos enfermos en la orilla sur, y al personal en la norte. Ayúdalos a instalarse."),
                Talk("Habla con Alberto Granado", alberto.transform, "Alberto Granado", "Yo vengo de trabajar en un leprosario en Córdoba. Por eso salimos en mi moto, La Poderosa, hasta que murió. Ahora, las curas. Tráeme lo que falta."),
                Collect("Trae las sulfonas, las vendas y el agua hervida", meds, clinic, "Ernesto Guevara", "Sin guantes, por favor. La lepra no se pega por dar la mano: lo que se pega es el miedo. Dales la mano. Es la mitad de la cura."),
                Visit("Recorre la vida de la colonia: la cancha, la chacra y la escuela", life, "Paciente hortelana", "Nos juegan al fútbol, comen con nosotros, y mira: el doctor argentino me enseña a leer. ¡Doctor, le digo, aunque me dice que aún le faltan exámenes! Hace mucho que nadie nos miraba a la cara."),
                Talk("Habla con el paciente músico", patient.transform, "Paciente músico", "El cantor es ciego y el acordeonista no tiene dedos en la mano derecha: se ata unas varillas a la muñeca. Vamos a darles una serenata desde el río. No tenemos otra cosa."),
                Hold("La noche de su cumpleaños: escucha el brindis en la mesa", party, 10f, "Ernesto Guevara", "Veinticuatro años. Este viaje me ha enseñado que somos un solo pueblo, de México al estrecho de Magallanes. Brindo por eso, y por esta gente."),
                Reach("Ve a la orilla: Ernesto cruza a nado hasta los enfermos", bank, "Alberto Granado", "Le he dicho que no. Con su asma, de noche, en este río. Y se ha tirado igual: quiere pasar la noche de su lado. Ahí va."),
                Talk("Vuelve con Granado junto a la balsa de los enfermos", raft, "Alberto Granado", "Los enfermos nos han hecho una balsa, la Mambo-Tango, y han venido a despedirnos cantando desde las barcas. Cuando alguien te trata como a una persona, te quedas en él para siempre."),
            },
            "La colonia de San Pablo, a orillas del Amazonas (Loreto, Perú), en junio de 1952, sobre el Hospital del santuario a escala de hormiga. Dos jóvenes argentinos llegan al final de su viaje a trabajar con enfermos de lepra. E para hablar, recoger y ayudar; para escuchar, quédate quieto.",
            new[]
            {
                "A finales de 1951, el estudiante de medicina Ernesto Guevara (Rosario, 1928) y su amigo el bioquímico Alberto Granado (1922–2011), que trabajaba con enfermos de lepra en Córdoba, salieron de Argentina en una vieja moto Norton, La Poderosa II, para recorrer Sudamérica. La moto se rompió en Chile y siguieron como pudieron. En Lima, el médico Hugo Pesce, especialista en lepra, los acogió y les dio cartas.",
                "La colonia de San Pablo, en la selva del Perú, junto al Amazonas, se fundó en 1925 y por los años 40 era una colonia agrícola. Los médicos y el personal vivían en una orilla y los enfermos en la otra. Guevara y Granado llegaron en junio de 1952 y se quedaron unas dos semanas: curaron, hicieron pruebas, jugaron al fútbol y dieron la mano sin guantes, algo que los enfermos no olvidaron.",
                "El 14 de junio Guevara cumplió 24 años; en la fiesta brindó por una América unida, de México al estrecho de Magallanes. Sus diarios cuentan que cruzó a nado el río hasta la orilla de los enfermos, pese a su asma. Antes de irse, los enfermos les regalaron una balsa, la Mambo-Tango, y les dieron una serenata desde el río. Bajaron por el Amazonas hacia Leticia. El libro de esos diarios se publicó en 1993 (Notas de viaje) y la película en 2004.",
                "Años después, Ernesto Guevara fue dirigente de la revolución cubana, conocido como «el Che», y murió en Bolivia en 1967. Su vida posterior, armada y política, incluidas las ejecuciones que ordenó en Cuba, es muy discutida: unos lo admiran y otros lo condenan. Esta historia se limita a aquellas semanas de 1952. La lepra (enfermedad de Hansen) tiene cura desde los años 80 y no se contagia por dar la mano. El director y los pacientes de esta escena son personajes inventados; las frases son de juego.",
            },
            new[]
            {
                "Guevara, E. (1993). Notas de viaje (Diarios de motocicleta). Centro de Estudios Che Guevara.",
                "Granado, A. (1978). Con el Che por Sudamérica. Letras Cubanas.",
                "Anderson, J. L. (1997). Che Guevara: A Revolutionary Life. Grove Press.",
                "International Leprosy Association, History of Leprosy, «San Pablo» (leprosyhistory.org).",
                "Revista Cubana de Medicina Militar (2021), 50 (1), sobre la labor de Ernesto Guevara en los leprosarios.",
            },
            "Alberto Granado", "Viajamos para ver mundo. Lo que vimos fue a la gente.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style StiltPalm = new Style(C(0.5f, 0.4f, 0.28f), C(0.45f, 0.35f, 0.25f), C(0.6f, 0.55f, 0.3f), Roof.Gable, 3f);   // madera sobre pilotes y hoja de irapay

    // SAN PABLO (1952): el Amazonas de oeste a este, ANCHO ACORTADO a 280 m (mide más de un kilómetro), los enfermos en la orilla sur y los médicos
    // en la norte; un caño y la selva alrededor. Plano aproximado: las notas de viaje dan el reparto y no las distancias (San Pablo de Loreto, OpenStreetMap).
    const string PlanSanPablo = @"
rio Río Amazonas | 280 | -1400 480, -500 520, 500 500, 1400 560
rio Caño de la colonia | 14 | -560 -1100, -300 -320, -460 380
campo Selva de la orilla sur | 500 280 | 860 -880
campo Selva del interfluvio | 420 300 | -980 -800
campo Selva de la orilla norte | 560 200 | 800 960
campo Chacras de yuca y plátano | 300 180 | 420 -200
calle Calle de la colonia | 6 | -300 60, 420 60
calle Calle del embarcadero | 6 | 0 60, 0 320
calle Calle del hospital | 5 | -240 60, -240 -220
calle Calle de las casas | 5 | -280 -220, 420 -220
calle Sendero de la chacra | 4 | 300 60, 420 -180
calle Sendero de la maloca yagua | 4 | 420 60, 1200 -40
calle Calle de los médicos | 6 | -420 840, 420 840
hito 0 | -180 110
hito 1 | -70 870
hito 2 | 200 110
hito 3 | 120 240
hito 4 | 90 -150
hito 5 | 420 -200
hito 6 | 240 -60
hito 7 | -260 -120
hito 8 | 1160 -60
hito 9 | 0 280
Plaza | 40 -20
Pileta | -80 -60
Parque | -140 220
Mercado | 340 180
Entrenamiento | 300 -340
Baile | -320 180
Estudio | 150 -300
Carpinteria | -360 -260
Taller | 420 170
Obra | 520 80
Piscina | 180 300
Guarderia | -90 -320
relleno | 32 | 0 -200";

    static int SanPabloTown(Transform t)
    {
        var s = Town(t, "San Pablo", new[]
        {
            new Lot("Hospital de la colonia", "Hospital", "Hospital de la colonia de San Pablo", 16f, 12f, StiltPalm,
                S("camilla", -4f, 2f, 1f, 2f, "Camilla de curas"), S("cama", 0f, 2f, 1.2f, 2f, "Cama del enfermo"), S("armario", 4f, 2f, 1.2f, 0.6f, "Sulfonas y vendas"), S("lavabo", 0f, -3f, 1f, 1f, "Tinaja de agua hervida")),
            new Lot("Casa del personal", "Casa", "Casa de los médicos (orilla norte)", 14f, 10f, StiltPalm,
                S("cama", -4f, 2f, 1.2f, 2f, "Hamaca del director"), S("cama", 0f, 2f, 1.2f, 2f, "Hamaca de Ernesto"), S("cama", 4f, 2f, 1.2f, 2f, "Hamaca de Alberto")),
            new Lot("Comedor de la colonia", "Restaurante", "Comedor de la colonia (juane, paiche y yuca)", 16f, 10f, StiltPalm,
                S("fogon", -4f, 2f, 1.6f, 0.8f, "Olla de pescado"), S("mesa", 1f, -1f, 2.4f, 1f, "Mesa larga"), S("barra", 4f, 3f, 2f, 0.8f, "Plátanos y fariña")),
            new Lot("Embarcadero de San Pablo", "Estación", "Embarcadero de la colonia", 14f, 10f, StiltPalm,
                S("anden", 0f, 2f, 8f, 3f, "Muelle de tablas"), S("puesto", 3f, -2f, 2f, 1f, "Almacén del vapor")),
            new Lot("Casa de la familia de pacientes", "Casa", "Casa de una familia de pacientes", 12f, 10f, StiltPalm,
                S("cama", -3f, 2f, 1.2f, 2f, "Hamaca"), S("cama", 3f, 2f, 1.2f, 2f, "Hamaca"), S("mesa", 0f, -3f, 1.6f, 1f, "Mesa de tablas")),
            new Lot("Chacra de la colonia", "Huerto", "Chacra de la colonia (yuca, plátano y maíz)", 16f, 12f, StiltPalm,
                S("bancal", -4f, 0f, 4f, 6f, "Yuca"), S("bancal", 2f, 0f, 4f, 6f, "Plátanos"), S("pozo", 6f, 3f, 1.2f, 1.2f, "Tanque de agua de lluvia")),
            new Lot("Capilla de la colonia", "Templo", "Capilla de la colonia", 12f, 10f, StiltPalm,
                S("altar", 0f, 2f, 2f, 1f, "Altar"), S("banco", 0f, -2f, 2f, 0.6f, "Banco")),
            new Lot("Laboratorio de Granado", "Laboratorio", "Laboratorio de la colonia", 12f, 10f, StiltPalm,
                S("mesa_laboratorio", 0f, 2f, 2.4f, 1f, "Mesa con microscopio"), S("estante", 4f, -3f, 0.8f, 1.6f, "Frascos y muestras")),
            new Lot("Maloca yagua", "Casa", "Maloca de los yagua (vecinos de la colonia)", 16f, 14f, StiltPalm,
                S("cama", -4f, 2f, 1.2f, 2f, "Hamaca de chambira"), S("cama", 3f, 2f, 1.2f, 2f, "Hamaca de chambira"), S("estante", 0f, -4f, 1.2f, 0.6f, "Cerbatanas de caza y cestos")),
            new Lot("Taller de balsas", "Carpintería", "Taller de balsas y canoas", 14f, 10f, StiltPalm,
                S("banco_taller", 0f, 2f, 2.4f, 1f, "Banco de la balsa"), S("armario", 4f, -3f, 1.2f, 0.6f, "Sogas y hachas")),
        }, new[]
        {
            new Lot("Casa de San Pablo", "Casa", "Casa sobre pilotes de la colonia", 10f, 9f, StiltPalm),
            new Lot("Chacra familiar", "Huerto", "Chacra familiar", 12f, 10f, StiltPalm, S("bancal", 0f, 0f, 4f, 6f, "Yuca y plátano"), S("pozo", 0f, 0f, 1f, 1f, "Tanque de agua")),
            new Lot("Taller de la colonia", "Taller", "Taller de la colonia", 10f, 8f, StiltPalm, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("armario", 0f, 0f, 1.2f, 0.6f, "Herramientas")),
            new Lot("Bodega del río", "Mercado", "Bodega del río", 10f, 8f, StiltPalm, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Latas y sal")),
        }, C(0.5f, 0.4f, 0.3f));
        int n = 0;
        n += W(t, "Enfermera de la colonia", s[0], "", s[2], null, s[1]);
        n += W(t, "Monja enfermera de San Pablo", s[0], "", s[2], s[6], s[1]);
        n += W(t, "Cocinera del comedor", s[2], "cocinero", s[2], null, s[4]);
        n += W(t, "Hortelano de la chacra", s[5], "hortelano", s[2], null, s[4]);
        n += W(t, "Ayudante de laboratorio", s[7], "sabio", s[2], null, s[1]);
        n += W(t, "Carpintero de balsas", s[9], "", s[2], null, s[4]);
        n += C2(t, "Madre yagua", "Padre yagua", s[8], s[8], null);
        n += Townsfolk(t, s, 10, "Vecina de la colonia", "Vecino de la colonia", s[2], s[6]);
        return n;
    }
}

public static partial class MicroBiome
{
    // San Pablo, junio: selva baja inundable del Amazonas al bajar las aguas. Lupuna, cetico, aguaje y capirona; caña brava en la orilla, heliconias
    // y bromelias. Hormiga cortadora, isula, comején, cigarra, mantis. Gallinas y perros en la colonia. (Brack Egg 1999, Diccionario de la
    // biodiversidad peruana.)
    static Biome SanPabloBiome() => new Biome { title = "San Pablo de Loreto, selva del Amazonas (junio de 1952)", ground = C(0.4f, 0.3f, 0.2f), plants = new[] {
            new Plant(Kind.Shrub, 7, "Lupuna (Ceiba pentandra)", 2400, C(0.25f, 0.48f, 0.26f), C(0.6f, 0.58f, 0.5f)),
            new Plant(Kind.Shrub, 7, "Cetico (Cecropia membranacea)", 1500, C(0.35f, 0.55f, 0.32f), C(0.65f, 0.65f, 0.58f)),
            new Plant(Kind.Shrub, 6, "Capirona (Calycophyllum spruceanum)", 2000, C(0.28f, 0.48f, 0.26f), C(0.6f, 0.4f, 0.25f)),
            new Plant(Kind.Grass, 12, "Caña brava (Gynerium sagittatum)", 400, C(0.42f, 0.6f, 0.3f), C(0.75f, 0.7f, 0.5f)),
            new Plant(Kind.Flower, 5, "Heliconia (Heliconia rostrata)", 220, C(0.28f, 0.5f, 0.26f), C(0.9f, 0.2f, 0.15f)),
            new Plant(Kind.Fern, 8, "Helecho de la selva", 90, C(0.28f, 0.52f, 0.28f), C(0.35f, 0.3f, 0.2f)) },
          town = new[] {
            new Plant(Kind.Shrub, 6, "Aguaje (Mauritia flexuosa)", 1500, C(0.3f, 0.5f, 0.25f), C(0.6f, 0.25f, 0.15f)),
            new Plant(Kind.Shrub, 5, "Plátano (Musa), de la chacra", 400, C(0.3f, 0.55f, 0.25f), C(0.9f, 0.85f, 0.3f)),
            new Plant(Kind.Shrub, 4, "Yuca (Manihot esculenta), de la chacra", 200, C(0.32f, 0.5f, 0.3f), C(0.5f, 0.35f, 0.3f)),
            new Plant(Kind.Grass, 10, "Hierba de la cancha", 10, C(0.45f, 0.62f, 0.3f), C(0.5f, 0.6f, 0.3f)) },
          bugs = new[] {
            new Bug("Ant", 6, 12, C(0.5f, 0.25f, 0.12f), "Hormiga cortadora (Atta cephalotes)"),
            new Bug("Termite", 5, 8, C(0.9f, 0.85f, 0.72f), "Comején (Nasutitermes)"),
            new Bug("Cicada", 3, 40, C(0.3f, 0.35f, 0.25f), "Cigarra de la selva (Quesada gigas)"),
            new Bug("Mantis", 2, 70, C(0.35f, 0.6f, 0.3f), "Mantis de la selva (Stagmatoptera)"),
            new Bug("Spider", 2, 30, C(0.35f, 0.28f, 0.2f), "Araña de la selva (Nephila)") },
          townFauna = new[] {
            new Pet("Ladybug", 3, 0.5f, C(0.75f, 0.45f, 0.2f), "Gallina de la colonia (en el microcosmos, una mariquita parda)"),
            new Pet("Woodlouse", 3, 0.7f, C(0.6f, 0.5f, 0.38f), "Perro de la colonia (en el microcosmos, una cochinilla hormiguera)") } };
}
