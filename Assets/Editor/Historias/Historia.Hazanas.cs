using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA FERIA DE LAS HAZAÑAS (docs/microcosmos-historias-3.md). Un lugar INVENTADO que reúne, en pabellones, una hazaña documentada de varios líderes:
/// George Washington (dejar el poder, 1797), Franklin D. Roosevelt (la Seguridad Social, 1935), José Figueres (abolir el ejército, 1948), Lee Kuan Yew
/// (de la pobreza a la riqueza y la educación; el «líder asiático» que pidió Berón; fue PRIMER MINISTRO, no presidente), Seretse Khama (Botsuana, 1966),
/// Nelson Mandela (reconciliar y dejar el cargo, 1994–1999) y Ellen Johnson Sirleaf (primera mujer elegida presidenta en África, 2006). Cada pabellón,
/// su hazaña y su sombra en una línea. Sirleaf vive: su pabellón lo explica una encargada inventada, sin ponerle palabras.
/// </summary>
public static partial class StorySceneBuilder
{
    static int Hazanas(Transform t)
    {
        Material stone = M("Piedra_de_la_Feria", 0.8f, 0.77f, 0.7f), wood = M("Madera_del_pabellon", 0.55f, 0.4f, 0.28f), gold = M("Placa_dorada", 0.85f, 0.7f, 0.3f), paper = M("Cartilla_de_la_Feria", 0.92f, 0.9f, 0.82f);
        Material red = M("Pabellon_rojo", 0.7f, 0.2f, 0.18f), blue = M("Pabellon_azul", 0.2f, 0.32f, 0.6f), green = M("Pabellon_verde", 0.2f, 0.5f, 0.32f), sky = M("Pabellon_celeste", 0.5f, 0.72f, 0.85f);

        // Cada pabellón: una tarima, su panel y un objeto de su hazaña.
        Transform Pavilion(string name, Vector3 at, string label, Material m, string thing, Vector3 size, Material thingMat)
        {
            Transform p = Spot(t, name, at, label, 3f);
            Prop(p, "Tarima", new Vector3(0f, 0.1f, 0f), new Vector3(5f, 0.2f, 4f), m, false);
            Prop(p, "Panel de la hazaña", new Vector3(0f, 1.4f, 1.6f), new Vector3(3f, 2.4f, 0.15f), stone);
            Prop(p, "Placa", new Vector3(0f, 1.6f, 1.5f), new Vector3(1.6f, 0.6f, 0.02f), gold, false);
            Prop(p, thing, new Vector3(0f, size.y * 0.5f + 0.2f, 0f), size, thingMat);
            return p;
        }

        Transform singapore = Pavilion("Pabellón de Singapur", new Vector3(0f, 0f, 16f), "Singapur: de la pobreza a la riqueza y la educación", red, "Bloque de viviendas públicas (maqueta)", new Vector3(1.6f, 2f, 1f), stone);
        Transform usa1 = Pavilion("Pabellón de Mount Vernon", new Vector3(-16f, 0f, 11f), "George Washington: dejar el poder", blue, "Mesa con la carta de despedida", new Vector3(1.6f, 0.8f, 0.8f), wood);
        Transform usa2 = Pavilion("Pabellón del New Deal", new Vector3(-19f, 0f, 1f), "Franklin D. Roosevelt: la Seguridad Social", blue, "Fichero de la Seguridad Social", new Vector3(1.6f, 1.4f, 0.6f), wood);
        Transform costa = Pavilion("Pabellón de Costa Rica", new Vector3(-13f, 0f, -9f), "José Figueres: un país sin ejército", sky, "Llave del cuartel Bellavista", new Vector3(0.5f, 1f, 0.2f), gold);
        Transform botswana = Pavilion("Pabellón de Botsuana", new Vector3(13f, 0f, -9f), "Seretse Khama: las escuelas del diamante", sky, "Pupitre y diamante en bruto", new Vector3(1.2f, 0.8f, 0.7f), wood);
        Transform southAfrica = Pavilion("Pabellón de Sudáfrica", new Vector3(19f, 0f, 1f), "Nelson Mandela: reconciliar y dejar el cargo", green, "Mesa de la Comisión de la Verdad", new Vector3(2f, 0.8f, 0.9f), wood);
        Transform liberia = Pavilion("Pabellón de Liberia", new Vector3(16f, 0f, 11f), "Ellen Johnson Sirleaf: la primera presidenta elegida de África", red, "Urna", new Vector3(0.8f, 0.8f, 0.8f), wood);
        var north = new List<Transform> { usa1, usa2, costa };
        Transform plaza = Spot(t, "Plaza de las Hazañas", new Vector3(0f, 0f, 5f), "La plaza: ¿qué tienen en común?", 2.6f);
        Prop(plaza, "Banco redondo", new Vector3(0f, 0.25f, 0f), new Vector3(3f, 0.5f, 3f), stone);
        var stamps = Items(t, paper, new Vector3(0.5f, 0.1f, 0.4f), ("Cartilla: vivienda", new Vector3(5f, 0.6f, 15f)), ("Cartilla: escuelas", new Vector3(-5f, 0.6f, 14f)), ("Cartilla: una ciudad limpia y sin corrupción", new Vector3(-7f, 0.6f, 19f)));

        // La gente: insectos-humanos, cada uno por su hazaña. Lee, HORMIGA tejedora de Singapur; Washington, MANTIS; Roosevelt, ARAÑA (la red de
        // seguridad); Figueres, TIJERETA (guarda a sus crías); Khama, TERMITA (levanta con lo que hay); Mandela, CIGARRA (años bajo tierra).
        Color white = new Color(0.92f, 0.9f, 0.84f), navy = new Color(0.14f, 0.18f, 0.32f), grey = new Color(0.42f, 0.42f, 0.44f), brown = new Color(0.36f, 0.28f, 0.22f), madiba = new Color(0.72f, 0.42f, 0.18f);
        GameObject guide = Modern(t, "Guía de la Feria (Sakshi)", new Vector3(3f, 0f, 3f), true, new Color(0.5f, 0.72f, 0.85f), CubeInsect.Hat.None, 1.65f, "Ladybug");
        GameObject lee = Modern(t, "Lee Kuan Yew", new Vector3(3f, 0f, 13f), false, white, CubeInsect.Hat.None, 1.78f, "Ant");
        GameObject washington = Modern(t, "George Washington", new Vector3(-13f, 0f, 8f), false, navy, CubeInsect.Hat.None, 1.88f, "Mantis");
        GameObject roosevelt = Modern(t, "Franklin D. Roosevelt", new Vector3(-16f, 0f, -1f), false, grey, CubeInsect.Hat.None, 1.88f, "Spider");
        GameObject figueres = Modern(t, "José Figueres", new Vector3(-10f, 0f, -7f), false, brown, CubeInsect.Hat.Brimmed, 1.6f, "Earwig");
        GameObject khama = Modern(t, "Seretse Khama", new Vector3(10f, 0f, -7f), false, navy, CubeInsect.Hat.None, 1.8f, "Termite");
        GameObject mandela = Who(t, "Nelson Mandela", new Vector3(16f, 0f, -1f), false, 1.83f, "Cicada"); CubeInsect.WearShirt(mandela, madiba);
        GameObject clerk = Modern(t, "Encargada del pabellón de Liberia (Ruth)", new Vector3(13f, 0f, 8f), true, grey, CubeInsect.Hat.None, 1.62f, "SnowBee"); Routine(clerk, liberia);
        int n = 8;

        Mission(t, "La Feria de las Hazañas de presidentes", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con la guía de la Feria", guide.transform, "Guía de la Feria", "Esta feria no existe: la hemos inventado para que quepan juntos, en un solo sitio, siete líderes de siete épocas. No venimos a decidir quién fue mejor. Cada pabellón enseña una hazaña que está en los libros, y también su sombra."),
                Talk("Empieza por Lee Kuan Yew, en el pabellón de Singapur", lee.transform, "Lee Kuan Yew", "No fui presidente: fui primer ministro. En 1965 nos echaron de Malasia: una isla sin agua, sin recursos, de unos 500 dólares por persona. Solo teníamos a la gente. Así que todo fue para ellos."),
                Collect("Reúne las tres cartillas de Singapur y llévalas a la maqueta", stamps, singapore, "Lee Kuan Yew", "Vivienda para casi todos, escuelas en inglés y en la lengua de cada uno, y funcionarios bien pagados y vigilados. Mis críticos dicen que fui muy duro con la oposición y con la prensa, y tienen razón en que lo hice. Juzgad vosotros."),
                Visit("Visita a los que renunciaron a algo: Washington, Roosevelt y Figueres", north, "Guía de la Feria", "Washington pudo seguir en el poder y se fue a casa tras dos mandatos. Roosevelt creó la Seguridad Social en 1935, en plena Depresión. Y Figueres, tras una guerra civil, disolvió el ejército de Costa Rica en 1948 y su cuartel se hizo museo."),
                Talk("Habla con Seretse Khama", khama.transform, "Seretse Khama", "En 1966 Botsuana tenía doce kilómetros de carretera asfaltada y un puñado de graduados. Un año después aparecieron los diamantes. Podían habernos arruinado, como a otros. Los usamos para escuelas, y seguimos siendo una democracia."),
                Talk("Habla con Nelson Mandela", mandela.transform, "Nelson Mandela", "Pasé 27 años en la cárcel. Al salir pude pedir venganza y pedí una Comisión de la Verdad y la Reconciliación. Y tras cinco años, me fui. Saber irse también es una hazaña."),
                Talk("Habla con la encargada del pabellón de Liberia", clerk.transform, "Encargada del pabellón", "Tras catorce años de guerra civil, las liberianas votaron en 2005 y Ellen Johnson Sirleaf fue la primera mujer elegida presidenta de un país africano. En 2011 recibió el Nobel de la Paz, compartido. En 2018 entregó el poder en el primer relevo pacífico de un presidente electo en décadas. También fue criticada, por nombrar a familiares en cargos públicos."),
                Hold("Siéntate en la plaza y piensa qué tienen en común", plaza, 10f, "Guía de la Feria", "Ninguno fue perfecto. Pero en todos hay un momento en que pudieron quedarse con algo —el poder, la venganza, el dinero— y lo dejaron para los demás. Eso es lo que venimos a mirar."),
            },
            "La Feria de las Hazañas, un lugar INVENTADO, sobre la Sala de asambleas del santuario a escala de hormiga. Siete pabellones cuentan una hazaña documentada de siete líderes de distintos países, con sus sombras. E para hablar y recoger; para pensar, quédate quieto.",
            new[]
            {
                "Lee Kuan Yew (1923–2015) fue PRIMER MINISTRO de Singapur de 1959 a 1990, no presidente: la presidencia de Singapur era ceremonial. Tras la independencia de 1965, con unos 500 dólares de renta por persona, la ciudad-Estado pasó a unos 14 000 en 1990, con vivienda pública para la mayoría, educación bilingüe y un alfabetismo del 50 % al 80 % y más. Hay otros candidatos a esa hazaña en Asia, como Park Chung-hee en Corea del Sur o Deng Xiaoping en China; Lee encaja mejor por haber sido a la vez riqueza y educación desde la pobreza. Sus críticos señalan las detenciones sin juicio, la presión a la prensa y las demandas a opositores.",
                "George Washington dejó la presidencia en 1797, tras dos mandatos, cuando pudo seguir: fijó una costumbre que Roosevelt rompió y la Enmienda 22 hizo ley. Fue también propietario de esclavos. Franklin D. Roosevelt firmó la Ley de Seguridad Social el 14 de agosto de 1935; su gobierno internó a más de 100 000 japoneses y japoneses-americanos en 1942. La junta de José Figueres abolió el ejército de Costa Rica el 1 de diciembre de 1948, con un gesto en el cuartel Bellavista, hoy Museo Nacional; los motivos se debaten (también le quitaba armas a sus rivales).",
                "Seretse Khama fue el primer presidente de Botsuana (1966–1980): al independizarse era el tercer país más pobre del mundo, con 12 km de carretera asfaltada y 22 titulados universitarios. Los diamantes de Orapa (1967) se invirtieron en escuelas y salud, con democracia, y su economía fue la que más creció del mundo entre 1960 y 1980. Nelson Mandela (1918–2013) salió de la cárcel en 1990, fue presidente de Sudáfrica en 1994 y no se presentó en 1999; compartió el Nobel de la Paz con F. W. de Klerk en 1993. Ellen Johnson Sirleaf fue presidenta de Liberia de 2006 a 2018.",
                "Esta feria es inventada y no es una clasificación. Las frases son de juego, resumen de lo que hicieron; de Sirleaf, que vive, habla una encargada. Se han dejado fuera a otros muchos; la lista se puede ampliar en el mismo formato.",
            },
            new[]
            {
                "Britannica, «Lee Kuan Yew» (britannica.com); Lee, K. Y. (2000). From Third World to First. HarperCollins.",
                "George Washington's Mount Vernon, «Farewell Address»; Social Security Administration, «Social Security Act of 1935» (ssa.gov/history).",
                "Americas Quarterly, «Why Did Costa Rica Really Abolish Its Military?» (americasquarterly.org); Nueva Sociedad, «Costa Rica: siete décadas sin fuerzas armadas».",
                "Acemoglu, D., Johnson, S. y Robinson, J. (2003). «An African Success Story: Botswana». En Rodrik (ed.), In Search of Prosperity. Princeton University Press.",
                "Mandela, N. (1994). Long Walk to Freedom. Little, Brown.",
                "Nobel Prize, «Ellen Johnson Sirleaf: Facts» (nobelprize.org); Truth and Reconciliation Commission of Liberia (2009).",
            },
            "Guía de la Feria", "Mira qué dejaron para los que venían detrás. Y mira también lo que no.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style FairPavilion = new Style(C(0.88f, 0.86f, 0.8f), C(0.72f, 0.7f, 0.64f), C(0.75f, 0.62f, 0.35f), Roof.Flat, 4f);    // pabellón de feria
    static readonly Style FairRed = new Style(C(0.75f, 0.3f, 0.25f), C(0.72f, 0.7f, 0.64f), C(0.6f, 0.2f, 0.18f), Roof.Flat, 4f);
    static readonly Style FairBlue = new Style(C(0.35f, 0.45f, 0.7f), C(0.72f, 0.7f, 0.64f), C(0.25f, 0.32f, 0.55f), Roof.Flat, 4f);
    static readonly Style FairGreen = new Style(C(0.35f, 0.6f, 0.42f), C(0.72f, 0.7f, 0.64f), C(0.25f, 0.45f, 0.3f), Roof.Flat, 4f);
    static readonly Style FairSky = new Style(C(0.6f, 0.78f, 0.86f), C(0.72f, 0.7f, 0.64f), C(0.4f, 0.6f, 0.7f), Roof.Flat, 4f);

    // LA FERIA DE LAS HAZAÑAS: INVENTADA, como la aldea de Goku. Dos avenidas en cruz, un anillo de pabellones y un jardín con su laguna, y un barrio
    // de la Feria (la gente que trabaja en ella).
    const string PlanFeria = @"
calle Avenida de la Feria | 16 | 0 -1000, 0 1000
calle Avenida de los Países | 16 | -1000 0, 1000 0
calle Anillo norte | 10 | -500 300, 0 500, 500 300
calle Anillo sur | 10 | -500 -300, 0 -500, 500 -300
calle Anillo este | 10 | 500 300, 700 0, 500 -300
calle Anillo oeste | 10 | -500 300, -700 0, -500 -300
calle Calle del barrio de la Feria | 8 | -900 -760, 900 -760
calle Calle de la estación | 8 | 800 400, 800 -760
lago Laguna de la Feria | 150 80 | -950 700
colina Mirador de la Feria | 180 34 | 950 720
campo Jardín de las Naciones | 300 180 | -900 -400
hito 0 | 0 500
hito 1 | -400 330
hito 2 | -620 60
hito 3 | -460 -320
hito 4 | 460 -320
hito 5 | 620 60
hito 6 | 400 330
hito 7 | 150 120
hito 8 | -150 120
hito 9 | 0 -640
Plaza | 40 -130
Pileta | -40 -60
Mercado | 280 -130
Parque | -900 -400
Entrenamiento | 300 -560
Piscina | 620 -560
Sanacion | -240 -560
Baile | -240 -130
Museo | 620 620
Tecnologia | -620 620
Estudio | -520 -600
Guarderia | -780 -600
Hotel | 200 760
Estacion | 800 -760
Taller | 540 -700
Carpinteria | 280 -880
Obra | -300 880
relleno | 40 | 0 -760";

    static int FeriaTown(Transform t)
    {
        var s = Town(t, "Feria de las Hazañas", new[]
        {
            new Lot("Pabellón de Singapur", "Museo", "Pabellón de Singapur (Lee Kuan Yew)", 18f, 12f, FairRed, S("vitrina", -4f, 2f, 1.6f, 0.6f, "Maqueta de un bloque del HDB"), S("vitrina", 3f, 2f, 1.6f, 0.6f, "Libros de la escuela bilingüe"), S("estatua", 0f, -3f, 1.4f, 1.4f, "Hormiga tejedora")),
            new Lot("Pabellón de Mount Vernon", "Museo", "Pabellón de Mount Vernon (George Washington)", 16f, 12f, FairBlue, S("vitrina", -3f, 2f, 1.6f, 0.6f, "Carta de despedida"), S("vitrina", 3f, 2f, 1.6f, 0.6f, "Espada que dejó")),
            new Lot("Pabellón del New Deal", "Museo", "Pabellón del New Deal (Franklin D. Roosevelt)", 16f, 12f, FairBlue, S("vitrina", -3f, 2f, 1.6f, 0.6f, "Ley de Seguridad Social"), S("vitrina", 3f, 2f, 1.6f, 0.6f, "Radio de las charlas junto al fuego")),
            new Lot("Pabellón de Costa Rica", "Museo", "Pabellón de Costa Rica (José Figueres)", 16f, 12f, FairSky, S("vitrina", -3f, 2f, 1.6f, 0.6f, "La llave del cuartel"), S("vitrina", 3f, 2f, 1.6f, 0.6f, "Libreta escolar")),
            new Lot("Pabellón de Botsuana", "Museo", "Pabellón de Botsuana (Seretse Khama)", 16f, 12f, FairSky, S("vitrina", -3f, 2f, 1.6f, 0.6f, "Diamante de Orapa"), S("vitrina", 3f, 2f, 1.6f, 0.6f, "Pupitre de 1966")),
            new Lot("Pabellón de Sudáfrica", "Museo", "Pabellón de Sudáfrica (Nelson Mandela)", 16f, 12f, FairGreen, S("vitrina", -3f, 2f, 1.6f, 0.6f, "Actas de la Comisión de la Verdad"), S("vitrina", 3f, 2f, 1.6f, 0.6f, "Papeleta de 1994")),
            new Lot("Pabellón de Liberia", "Museo", "Pabellón de Liberia (Ellen Johnson Sirleaf)", 16f, 12f, FairRed, S("vitrina", -3f, 2f, 1.6f, 0.6f, "Urna de 2005"), S("vitrina", 3f, 2f, 1.6f, 0.6f, "Diploma del Nobel de la Paz")),
            new Lot("Restaurante de los Países", "Restaurante", "Restaurante de los Países", 18f, 12f, FairPavilion, S("cocina", -4f, 3f, 2.4f, 0.8f, "Cocina (hainanese chicken rice, seswaa, gallo pinto)"), S("mesa", 0f, -1f, 2.4f, 1f, "Mesas largas"), S("barra", 4f, 3f, 2.4f, 0.8f, "Barra")),
            new Lot("Oficina de la Feria", "Taller de papeles", "Oficina de la Feria", 14f, 10f, FairPavilion, S("mesa", 0f, 2f, 2.4f, 1f, "Mesa de la organización"), S("armario", 4f, -3f, 1.2f, 0.6f, "Archivo de fuentes")),
            new Lot("Residencia de la Feria", "Casa", "Residencia del personal de la Feria", 16f, 12f, FairPavilion, S("cama", -5f, 3f, 1f, 2f, "Cama"), S("cama", -2f, 3f, 1f, 2f, "Cama"), S("cama", 1f, 3f, 1f, 2f, "Cama"), S("cama", 4f, 3f, 1f, 2f, "Cama")),
        }, new[]
        {
            new Lot("Casa de la Feria", "Casa", "Casa del barrio de la Feria", 10f, 9f, FairPavilion),
            new Lot("Puesto de artesanía", "Mercado", "Puesto de artesanía del mundo", 10f, 8f, FairPavilion, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Huerto de la Feria", "Huerto", "Huerto de la Feria", 12f, 10f, FairPavilion, S("bancal", 0f, 0f, 4f, 6f, "Hortalizas del mundo"), S("pozo", 0f, 0f, 1f, 1f, "Riego")),
            new Lot("Taller de montaje", "Taller", "Taller de montaje de pabellones", 12f, 10f, FairPavilion, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("armario", 0f, 0f, 1.2f, 0.6f, "Herramientas")),
        }, C(0.55f, 0.53f, 0.5f));
        int n = 0;
        n += W(t, "Guía de Singapur", s[0], "sabio", s[7], null, s[9]);
        n += W(t, "Guía de Costa Rica", s[3], "sabio", s[7], null, s[9]);
        n += W(t, "Guía de Botsuana", s[4], "sabio", s[7], null, s[9]);
        n += W(t, "Guía de Sudáfrica", s[5], "sabio", s[7], null, s[9]);
        n += W(t, "Cocinera de los Países", s[7], "cocinero", s[7], null, null);
        n += W(t, "Bibliotecaria de la Feria", s[8], "sabio", s[7], null, null);
        n += Townsfolk(t, s, 10, "Vecina de la Feria", "Vecino de la Feria", s[7], null);
        return n;
    }
}

public static partial class MicroBiome
{
    // La Feria (inventada): jardín de plantas de los países de cada pabellón: el árbol de la lluvia de Singapur, el cornizuelo y el corteza de
    // Costa Rica, la acacia de Botsuana, el protea de Sudáfrica, la palmera aceitera de Liberia, el cerezo de Washington, el roble de Roosevelt.
    static Biome FeriaBiome() => new Biome { title = "La Feria de las Hazañas (inventada): jardín de los países", ground = C(0.5f, 0.46f, 0.36f), plants = new[] {
            new Plant(Kind.Grass, 12, "Césped de la Feria, segado", 6, C(0.42f, 0.6f, 0.3f), C(0.5f, 0.58f, 0.3f)),
            new Plant(Kind.Flower, 5, "Protea (Protea cynaroides), de Sudáfrica", 120, C(0.35f, 0.5f, 0.36f), C(0.9f, 0.5f, 0.55f)),
            new Plant(Kind.Flower, 4, "Orquídea de Singapur (Papilionanthe Miss Joaquim)", 80, C(0.3f, 0.5f, 0.28f), C(0.7f, 0.4f, 0.8f)),
            new Plant(Kind.Stone, 6, "Adoquín de la Feria", 0, C(0.7f, 0.68f, 0.62f), Stone) },
          town = new[] {
            new Plant(Kind.Shrub, 5, "Árbol de la lluvia (Samanea saman), de Singapur", 1800, C(0.3f, 0.5f, 0.28f), C(0.85f, 0.5f, 0.6f)),
            new Plant(Kind.Shrub, 4, "Acacia del Kalahari (Vachellia erioloba), de Botsuana", 900, C(0.4f, 0.5f, 0.3f), C(0.9f, 0.8f, 0.3f)),
            new Plant(Kind.Shrub, 4, "Corteza amarilla (Handroanthus ochraceus), de Costa Rica", 1200, C(0.32f, 0.5f, 0.28f), C(0.95f, 0.8f, 0.15f)),
            new Plant(Kind.Shrub, 3, "Roble blanco (Quercus alba), de Estados Unidos", 1800, C(0.28f, 0.48f, 0.26f), C(0.45f, 0.38f, 0.3f)) },
          bugs = new[] {
            new Bug("Ant", 4, 9, C(0.75f, 0.45f, 0.15f), "Hormiga tejedora (Oecophylla smaragdina)"),
            new Bug("SnowBee", 3, 14, C(0.8f, 0.6f, 0.2f), "Abeja del jardín"),
            new Bug("Ladybug", 3, 7, C(0.8f, 0.2f, 0.12f), "Mariquita del jardín") },
          townFauna = new[] { new Pet("Woodlouse", 3, 0.7f, C(0.7f, 0.6f, 0.45f), "Perro de la Feria (en el microcosmos, una cochinilla hormiguera)") } };
}
