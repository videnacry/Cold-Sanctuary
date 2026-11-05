using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS CALCULADORAS DE LANGLEY (docs/microcosmos-historias-3.md). Hampton (Virginia), 19–20 de febrero de 1962: Katherine Johnson comprueba a mano
/// la órbita de John Glenn; Dorothy Vaughan, que dirigió a las calculadoras negras de West Area, domina FORTRAN; Mary Jackson trabaja en el
/// túnel supersónico tras conseguir permiso para estudiar en un instituto segregado. Glenn no está aquí: está en cabo Cañaveral.
/// Las frases son de juego, y no citas; los hechos, de NASA y de Shetterly (2016).
/// </summary>
public static partial class StorySceneBuilder
{
    static int Langley(Transform t)
    {
        Material desk = M("Mesa_gris", 0.55f, 0.56f, 0.58f), steel = M("Acero_del_tunel", 0.62f, 0.64f, 0.67f), panel = M("Panel_IBM", 0.2f, 0.28f, 0.42f), tape = M("Cinta_magnetica", 0.1f, 0.1f, 0.12f);
        Material paper = M("Hoja_de_calculo", 0.93f, 0.92f, 0.86f), sign = M("Cartel_de_comedor", 0.85f, 0.82f, 0.7f), red = M("Lapiz_rojo", 0.7f, 0.15f, 0.12f), wood = M("Madera_de_mesa", 0.5f, 0.36f, 0.24f), globe = M("Globo_azul", 0.2f, 0.42f, 0.7f);

        // 1 · LA MESA DE KATHERINE: la calculadora mecánica, las hojas y el globo con las tres órbitas.
        Transform table = Spot(t, "Mesa de Katherine Johnson", new Vector3(0f, 0f, 15f), "La mesa de Katherine Johnson: la órbita de Friendship 7, a mano", 3f);
        Prop(table, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(3.2f, 0.8f, 1.2f), desk);
        Prop(table, "Calculadora mecánica Friden", new Vector3(-0.9f, 0.95f, 0f), new Vector3(0.6f, 0.3f, 0.5f), steel, false);
        Prop(table, "Hojas de cálculo", new Vector3(0.6f, 0.84f, 0f), new Vector3(1f, 0.08f, 0.7f), paper, false);
        Prop(table, "Globo terráqueo", new Vector3(3f, 1f, 0f), new Vector3(1.4f, 1.4f, 1.4f), globe);
        Prop(table, "Pie del globo", new Vector3(3f, 0.15f, 0f), new Vector3(0.8f, 0.3f, 0.8f), wood);

        // 2 · LAS TRES SALIDAS DE LA MÁQUINA: los listados que hay que comprobar.
        var sheets = Items(t, paper, new Vector3(0.7f, 0.1f, 0.5f), ("Listado del IBM: el lanzamiento", new Vector3(-9f, 0.5f, 3f)), ("Listado del IBM: las tres órbitas", new Vector3(-12f, 0.5f, -2f)), ("Listado del IBM: la reentrada", new Vector3(-6f, 0.5f, -5f)));

        // 3 · LA SALA DEL ORDENADOR.
        Transform computer = Spot(t, "Sala del ordenador", new Vector3(-16f, 0f, 9f), "La sala del ordenador IBM: FORTRAN", 3f);
        for (int k = 0; k < 3; k++) Prop(computer, "Armario del ordenador", new Vector3(-3f + k * 3f, 1f, 1.5f), new Vector3(2f, 2f, 0.9f), panel);
        for (int k = 0; k < 2; k++) Prop(computer, "Unidad de cinta", new Vector3(-1.5f + k * 3f, 0.95f, -1f), new Vector3(1.1f, 1.9f, 0.7f), tape);

        // 4 · LO QUE COSTÓ: el cartel del comedor, el pupitre del instituto y el túnel.
        Transform cafe = Spot(t, "Mesa del comedor", new Vector3(14f, 0f, 8f), "La mesa aparte del comedor (el cartel de Miriam Mann)", 2.4f);
        Prop(cafe, "Mesa aparte", new Vector3(0f, 0.4f, 0f), new Vector3(2.2f, 0.8f, 1f), wood);
        Prop(cafe, "Cartel «Colored Computers»", new Vector3(0f, 1.1f, 0f), new Vector3(0.9f, 0.5f, 0.05f), sign, false);
        Transform pupitre = Spot(t, "Pupitre de Hampton High", new Vector3(18f, 0f, 0f), "El curso nocturno en Hampton High School", 2.2f);
        Prop(pupitre, "Pupitre", new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 0.7f), wood);
        Transform tunnel = Spot(t, "Túnel supersónico", new Vector3(13f, 0f, -9f), "El túnel supersónico de 4 × 4 pies", 2.8f);
        Prop(tunnel, "Tubo del túnel", new Vector3(0f, 1.2f, 1f), new Vector3(7f, 2f, 2f), steel);
        Prop(tunnel, "Manómetros", new Vector3(-2.4f, 1.2f, -0.4f), new Vector3(1.6f, 1.6f, 0.2f), desk);
        var cost = new List<Transform> { cafe, pupitre, tunnel };

        // 5 · LA RADIO: el 20 de febrero, la cuenta atrás.
        Transform radio = Spot(t, "Radio del pasillo", new Vector3(-5f, 0f, 8f), "La radio: 20 de febrero de 1962, 9:47", 2f);
        Prop(radio, "Radio de válvulas", new Vector3(0f, 0.55f, 0f), new Vector3(0.9f, 1.1f, 0.6f), wood);
        Prop(radio, "Dial", new Vector3(0f, 0.8f, -0.32f), new Vector3(0.5f, 0.1f, 0.02f), red, false);

        // La gente: insectos-humanos. Johnson, un ESCARABAJO PELOTERO (se orienta por la Vía Láctea); Vaughan, una HORMIGA (la colonia reparte el
        // cálculo en pasos); Jackson, una TIJERETA (cuida de las suyas).
        Color mint = new Color(0.55f, 0.72f, 0.65f), navy = new Color(0.16f, 0.2f, 0.34f), rose = new Color(0.72f, 0.42f, 0.45f), grey = new Color(0.45f, 0.46f, 0.48f);
        GameObject katherine = Modern(t, "Katherine Johnson", new Vector3(2f, 0f, 11f), true, mint, CubeInsect.Hat.None, 1.65f, "DungBeetle");
        GameObject dorothy = Modern(t, "Dorothy Vaughan", new Vector3(-13f, 0f, 6f), true, navy, CubeInsect.Hat.None, 1.62f, "Ant"); Routine(dorothy, computer);
        GameObject mary = Modern(t, "Mary Jackson", new Vector3(10f, 0f, -6f), true, rose, CubeInsect.Hat.None, 1.68f, "Earwig"); Routine(mary, tunnel, pupitre);
        GameObject engineer = Modern(t, "Ingeniero de la División de Vuelo (Atlas)", new Vector3(-3f, 0f, 12f), false, grey, CubeInsect.Hat.None, 1.78f, "Grasshopper");
        GameObject young = Modern(t, "Calculadora joven (Ruth)", new Vector3(-9f, 0f, -3f), true, rose, CubeInsect.Hat.None, 1.6f, "Ant"); Routine(young, computer, table);
        int n = 5;

        Mission(t, "Las calculadoras de Langley: Johnson, Vaughan y Jackson (1962)", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con el ingeniero de vuelo", engineer.transform, "Ingeniero", "Las órbitas de Glenn las ha calculado la máquina IBM. Pero el astronauta no se fía del todo de una caja que puede fallar. Pide que lo compruebe a mano la que él llama «la chica». Ya sabes quién es."),
                Talk("Habla con Katherine Johnson", katherine.transform, "Katherine Johnson", "Son las mismas ecuaciones, cada una con su lápiz. Si él sale, yo quiero que vuelva. Tráeme los listados de la máquina, por favor: lo comprobaré todo."),
                Collect("Lleva los tres listados del IBM a su mesa", sheets, table, "Katherine Johnson", "Gracias. Lanzamiento, órbitas, reentrada. Un dígito mal al principio es un océano de error al final."),
                Talk("Ve a la sala del ordenador con Dorothy Vaughan", dorothy.transform, "Dorothy Vaughan", "Yo dirigí a las calculadoras de West Area, las mujeres negras a las que aquí tuvieron años aparte. Cuando vi llegar la máquina, supe que nos quitaría el trabajo; así que aprendí FORTRAN antes que nadie y se lo enseñé a mis chicas."),
                Visit("Mira lo que costó llegar aquí: el comedor, el instituto y el túnel", cost, "Mary Jackson", "Miriam Mann quitaba cada día el cartel de la mesa aparte, hasta que dejaron de ponerlo. Para ser ingeniera tuve que pedir permiso a la ciudad para ir a clase en un instituto solo para blancos. Y aquí estoy: el aire, a dos veces la velocidad del sonido."),
                Hold("Siéntate a la mesa de Katherine: día y medio de números", table, 12f, "Katherine Johnson", "Número a número, en la Friden. Y sale. Los míos y los de la máquina coinciden. Díselo: ya puede subir."),
                Hold("20 de febrero, 9:47: escucha la radio", radio, 10f, "Calculadora joven", "«Godspeed, John Glenn». ¡Ya está arriba! Tres vueltas a la Tierra en menos de cinco horas. Y en esta sala nadie respira."),
                Talk("Vuelve con Katherine Johnson", katherine.transform, "Katherine Johnson", "Ha amerizado. Lo que yo hacía era mi trabajo. Pero mira a tu alrededor: esta sala la abrieron mujeres a las que nadie miraba. Haz muchas preguntas. Ellas las hicieron."),
            },
            "Langley (Hampton, Virginia), los días 19 y 20 de febrero de 1962, sobre el piso de Informática del santuario a escala de hormiga. Mañana John Glenn será el primer estadounidense en orbitar la Tierra, y una mujer va a comprobar a mano lo que ha calculado el ordenador. Conoce a las tres mujeres que abrieron camino. E para hablar y recoger; para calcular o escuchar, quédate quieto.",
            new[]
            {
                "Desde 1935, el laboratorio Langley de la NACA (desde 1958, la NASA) contrató a mujeres como «calculadoras»: hacían a mano los cálculos de los ingenieros. En 1943, con la guerra, empezó a contratar a mujeres negras; por las leyes de segregación de Virginia las sentaron aparte, en West Area, con comedor y baños aparte. Según Margot Lee Shetterly, Miriam Mann retiraba el cartel «Colored Computers» de su mesa hasta que dejó de aparecer.",
                "Dorothy Vaughan llegó en 1943 y en 1949 pasó a dirigir West Area: fue la primera supervisora negra de la NACA. En 1958, al desaparecer esa sección, se fue a la nueva División de Análisis y Computación, donde fue experta en FORTRAN. Se retiró en 1971. Mary Jackson entró en West Area en 1951 y pasó a trabajar con el ingeniero Kazimierz Czarnecki en el túnel supersónico; para hacer las clases necesarias tuvo que obtener permiso especial de la ciudad de Hampton para ir a Hampton High, segregada. En 1958 fue la primera ingeniera negra de la NASA; en 1979 dejó la ingeniería por el programa de igualdad de las mujeres de Langley.",
                "Katherine Johnson llegó en 1953 y a las dos semanas Vaughan la asignó a la División de Vuelo. Calculó la trayectoria de Alan Shepard en 1961. Para el vuelo de Friendship 7, Glenn pidió que ella comprobara las cuentas del ordenador IBM: tardó un día y medio. Glenn despegó el 20 de febrero de 1962 a las 9:47 (hora del este). Johnson trabajó también en Apolo y recibió la Medalla Presidencial de la Libertad en 2015. Murió en 2020, a los 101 años. Su historia se hizo conocida con el libro Hidden Figures (2016).",
                "Las fuentes difieren sobre cuándo pidió Glenn esa comprobación (no fue en el último minuto, como en la película). Las frases son de juego, no citas, salvo el «Godspeed, John Glenn» de Scott Carpenter. No hay en esta historia ningún gran ordenador en el mismo edificio que su mesa: se han reunido para contarla.",
            },
            new[]
            {
                "NASA, «Katherine Johnson Biography» (nasa.gov/centers-and-facilities/langley/katherine-johnson-biography).",
                "NASA, «Dorothy Vaughan» (nasa.gov/people/dorothy-vaughan) y «Mary W. Jackson Biography» (nasa.gov/people/mary-w-jackson-biography).",
                "Shetterly, M. L. (2016). Hidden Figures. William Morrow.",
                "Space.com (2017), «Hidden Figures: When Did John Glenn Ask for the Girl to Check the Numbers?».",
                "Skopinski, T. H. y Johnson, K. G. (1960). Determination of Azimuth Angle at Burnout for Placing a Satellite Over a Selected Earth Position. NASA TN D-233.",
            },
            "Katherine Johnson", "Cuenta bien, comprueba otra vez y no dejes de preguntar.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style NasaBrick = new Style(C(0.7f, 0.48f, 0.38f), C(0.7f, 0.7f, 0.68f), C(0.5f, 0.5f, 0.5f), Roof.Flat, 4f);     // los edificios de Langley: ladrillo y terrazo
    static readonly Style HamptonClap = new Style(C(0.9f, 0.88f, 0.82f), C(0.55f, 0.45f, 0.34f), C(0.36f, 0.36f, 0.38f), Roof.Gable, 3.4f);   // casa de tablas de Hampton

    // HAMPTON Y LANGLEY (1962): el centro de investigación junto a la base aérea, el río Back al norte, el arroyo Newmarket al oeste, el bulevar
    // Mercury y la carretera a Hampton, el río Hampton al sureste con el Hampton Institute y la bahía de Chesapeake al este. Plano aproximado y
    // ACORTADO: Newport News y las casas de Johnson y Vaughan, más cerca. (OpenStreetMap; NASA, Langley Historic Sites; Shetterly 2016.)
    const string PlanLangley = @"
mar Bahía de Chesapeake | 500 2800 | 1650 0
rio Río Back | 60 | -1300 980, -300 820, 700 900, 1300 980
rio Arroyo Newmarket | 18 | -900 950, -780 300, -860 -500, -700 -1200
rio Río Hampton | 40 | 1250 -1250, 900 -760, 760 -300
calle Bulevar Mercury | 26 | -1400 -520, 1400 -520
calle Avenida Armistead | 14 | 480 -1200, 480 500
calle Calle de Langley | 12 | -600 0, 600 0
calle Avenida Sweeney | 10 | 0 -520, 0 560
calle Calle Victoria | 12 | -1000 -860, 800 -860
calle Calle Wythe Creek | 10 | -560 -520, -420 640
calle Calle de las casas de Hampton | 8 | -300 -700, 700 -700
calle Calle de Newport News | 8 | -1300 -120, -880 -120
campo Pista de la base aérea | 900 80 | 560 480
campo Marismas del río Back | 400 110 | 200 730
hito 0 | 60 100
hito 1 | -150 120
hito 2 | 220 180
hito 3 | 180 -100
hito 4 | 330 -640
hito 5 | 860 -520
hito 6 | -180 -650
hito 7 | -1000 -240
hito 8 | 110 -700
hito 9 | -320 -620
Plaza | 560 -760
Mercado | 150 -440
Parque | -420 300
Piscina | 620 -660
Entrenamiento | -300 -400 | actual: gimnasio de Hampton (Hampton Roads); posición aproximada
Sanacion | 720 -880
Bomberos | 240 -420
Policia | 400 -430
Guarderia | -80 -800
Museo | 880 -900
Hotel | -120 -440
Tecnologia | -320 120
Investigacion | 320 40
Mecanica | -500 -440
Carpinteria | -620 -780
Estudio | 150 -960
Baile | 480 -960
Estacion | -1100 -600
relleno | 55 | 200 -700";

    static int LangleyTown(Transform t)
    {
        var s = Town(t, "Langley", new[]
        {
            new Lot("Sección de cálculo", "Centro de tecnología", "Edificio de West Area: mesas de las calculadoras", 18f, 12f, NasaBrick,
                S("consola", -5f, 3f, 1.6f, 0.8f, "Calculadora Friden"), S("consola", 0f, 3f, 1.6f, 0.8f, "Calculadora Monroe"), S("consola", 5f, 3f, 1.6f, 0.8f, "Calculadora Marchant"), S("estante", 0f, -4f, 1.6f, 0.6f, "Carpetas de cálculos")),
            new Lot("Sala del ordenador IBM", "Centro de tecnología", "División de Análisis y Computación: el ordenador IBM", 18f, 12f, NasaBrick,
                S("consola", -4f, 2f, 2f, 0.8f, "Consola del IBM"), S("consola", 4f, 2f, 2f, 0.8f, "Lectora de tarjetas perforadas"), S("armario", 0f, -4f, 1.6f, 0.6f, "Cintas magnéticas"), S("mesa", -4f, -4f, 2f, 1f, "Mesa de programar en FORTRAN")),
            new Lot("Túnel supersónico", "Laboratorio", "Túnel supersónico de 4 × 4 pies", 20f, 12f, NasaBrick,
                S("mesa_laboratorio", -4f, 2f, 2.4f, 1f, "Sección de pruebas del túnel"), S("consola", 4f, 2f, 1.6f, 0.8f, "Panel de manómetros"), S("estante", 0f, -4f, 1.2f, 0.6f, "Maquetas de alas")),
            new Lot("Cafetería de Langley", "Restaurante", "Cafetería de Langley", 16f, 12f, NasaBrick,
                S("cocina", -4f, 3f, 2.4f, 0.8f, "Cocina de la cafetería"), S("mesa", 0f, -1f, 2.4f, 1f, "Mesas largas"), S("barra", 4f, 3f, 2.4f, 0.8f, "Barra de servir")),
            new Lot("Hampton High School", "Escuela", "Hampton High School (los cursos nocturnos)", 16f, 12f, NasaBrick,
                S("pizarra", 0f, 4f, 2.4f, 0.4f, "Pizarra de matemáticas"), S("mesa", 0f, 0f, 2.4f, 1f, "Pupitres"), S("estante", -5f, -3f, 0.8f, 1.6f, "Libros de física")),
            new Lot("Hampton Institute", "Escuela", "Hampton Institute (universidad negra, desde 1868)", 18f, 12f, HamptonClap,
                S("pizarra", 0f, 4f, 2.4f, 0.4f, "Pizarra"), S("mesa", -3f, -1f, 2.4f, 1f, "Mesa de estudio"), S("estante", 5f, 3f, 0.8f, 2f, "Biblioteca Huntington")),
            new Lot("Casa de Mary Jackson", "Casa", "Casa de Mary y Levi Jackson", 12f, 10f, HamptonClap,
                S("cama", -2f, 2f, 1.6f, 2f, "Cama de matrimonio"), S("cama", 3f, 2f, 1.2f, 2f, "Cama de los hijos"), S("mesa", 0f, -3f, 1.6f, 1f, "Mesa de estudiar")),
            new Lot("Casa de Dorothy Vaughan", "Casa", "Casa de Dorothy Vaughan (Newport News)", 12f, 10f, HamptonClap,
                S("cama", -2f, 2f, 1.4f, 2f, "Cama de Dorothy"), S("cama", 3f, 2f, 1.2f, 2f, "Cama de los hijos"), S("estante", 3f, -3f, 0.8f, 1.6f, "Manual de FORTRAN")),
            new Lot("Casa de Katherine Johnson", "Casa", "Casa de Katherine Johnson", 12f, 10f, HamptonClap,
                S("cama", -2f, 2f, 1.4f, 2f, "Cama de Katherine"), S("cama", 3f, 2f, 1.2f, 2f, "Cama de las hijas"), S("estante", 3f, -3f, 0.8f, 1.6f, "Libros de geometría")),
            new Lot("Iglesia Carver Memorial", "Templo", "Iglesia Presbiteriana Carver Memorial", 14f, 10f, HamptonClap,
                S("altar", 0f, 3f, 2f, 1f, "Púlpito"), S("banco", -3f, -2f, 2f, 0.6f, "Banco"), S("banco", 3f, -2f, 2f, 0.6f, "Banco")),
        }, new[]
        {
            new Lot("Casa de Hampton", "Casa", "Casa de tablas de Hampton", 10f, 9f, HamptonClap),
            new Lot("Tienda de Hampton", "Mercado", "Tienda de la esquina", 10f, 8f, HamptonClap, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller de barcas", "Taller", "Taller de barcas del río Hampton", 12f, 10f, HamptonClap, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del calafate"), S("armario", 0f, 0f, 1.2f, 0.6f, "Herramientas")),
            new Lot("Pescadería de ostras", "Mercado", "Pescadería (ostras y cangrejo azul)", 10f, 8f, HamptonClap, S("puesto", 0f, 0f, 2f, 1f, "Puesto de ostras"), S("estante", 0f, 0f, 0.8f, 1.6f, "Cestas")),
        }, C(0.42f, 0.42f, 0.42f));
        int n = 0;
        n += W(t, "Calculadora de West Area", s[0], "sabio", s[3], null, null);
        n += W(t, "Programadora de FORTRAN", s[1], "sabio", s[3], null, null);
        n += W(t, "Técnico del túnel", s[2], "", s[3], null, null);
        n += W(t, "Cocinero de la cafetería", s[3], "cocinero", s[3], null, null);
        n += W(t, "Profesor de Hampton High", s[4], "sabio", s[3], null, null);
        n += W(t, "Estudiante del Hampton Institute", s[5], "alumno", s[3], s[5], null);
        n += W(t, "Pastora de Carver Memorial", s[9], "", s[3], null, null);
        n += Townsfolk(t, s, 10, "Vecina de Hampton", "Vecino de Hampton", s[3], null);
        return n;
    }
}

public static partial class MicroBiome
{
    // Hampton, febrero: invierno suave de la costa de Virginia. Pino taeda, cedro rojo, acebo y marismas de espartina; camelias en los jardines;
    // pocos bichos: arañas en los edificios y mariquitas de invierno. Perros y gatos. (Weakley et al. 2012, Flora of Virginia.)
    static Biome LangleyBiome() => new Biome { title = "Hampton, costa de Virginia (febrero de 1962)", winter = true, ground = C(0.42f, 0.38f, 0.3f), plants = new[] {
            new Plant(Kind.Shrub, 7, "Pino taeda (Pinus taeda)", 2200, C(0.22f, 0.38f, 0.25f), C(0.45f, 0.35f, 0.25f)),
            new Plant(Kind.Shrub, 5, "Cedro rojo (Juniperus virginiana)", 1000, C(0.22f, 0.36f, 0.28f), C(0.4f, 0.45f, 0.6f)),
            new Plant(Kind.Grass, 14, "Espartina (Spartina alterniflora), de la marisma", 120, C(0.62f, 0.58f, 0.4f), C(0.7f, 0.6f, 0.4f)),
            new Plant(Kind.Stone, 8, "Bordillo de hormigón", 0, C(0.72f, 0.72f, 0.68f), Stone) },
          town = new[] {
            new Plant(Kind.Shrub, 5, "Acebo americano (Ilex opaca), de jardín", 700, C(0.2f, 0.38f, 0.22f), C(0.8f, 0.15f, 0.12f)),
            new Plant(Kind.Flower, 4, "Camelia (Camellia japonica), en flor", 250, C(0.2f, 0.4f, 0.22f), C(0.85f, 0.25f, 0.35f)),
            new Plant(Kind.Grass, 12, "Césped de invierno, segado", 5, C(0.5f, 0.55f, 0.36f), C(0.5f, 0.52f, 0.32f)) },
          bugs = new[] { new Bug("Spider", 2, 8, C(0.4f, 0.32f, 0.25f), "Araña casera"), new Bug("Ladybug", 3, 7, C(0.8f, 0.3f, 0.12f), "Mariquita de invierno", "aletargada en invierno") },
          townFauna = new[] {
            new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la casa (en el microcosmos, una cochinilla hormiguera)"),
            new Pet("Woodlouse", 2, 0.5f, C(0.85f, 0.83f, 0.78f), "Gato de la casa (en el microcosmos, una cochinilla hormiguera)") } };
}
