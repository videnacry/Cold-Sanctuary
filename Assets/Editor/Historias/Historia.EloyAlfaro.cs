using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ELOY ALFARO Y EL TREN DE QUITO (docs/microcosmos-historias-3.md). Chimbacalle, 25 de junio de 1908: el primer tren que llega de Durán a
/// Quito el día en que Alfaro cumple 66 años. La historia va de la obra (la cuadrilla, la Nariz del Diablo) y de lo que cambió con ella
/// (escuela laica, registro civil, telégrafo). La guerra civil y su muerte en 1912 quedan en una línea de la ficha.
/// Las frases son de juego; los hechos, de las fuentes del doc.
/// </summary>
public static partial class StorySceneBuilder
{
    static int EloyAlfaro(Transform t)
    {
        Material iron = M("Hierro_negro", 0.14f, 0.14f, 0.16f), wood = M("Durmiente", 0.42f, 0.3f, 0.2f), brass = M("Laton", 0.8f, 0.65f, 0.3f), gold = M("Oro_del_clavo", 0.95f, 0.78f, 0.25f);
        Material stone = M("Andesita", 0.5f, 0.5f, 0.52f), red = M("Bandera_roja", 0.75f, 0.15f, 0.15f), paper = M("Papel_del_plano", 0.9f, 0.86f, 0.74f), wire = M("Poste_telegrafo", 0.35f, 0.25f, 0.18f);

        // 1 · EL ANDÉN: la vía recién tendida, la locomotora y su campana.
        Transform platform = Spot(t, "Andén de Chimbacalle", new Vector3(0f, 0f, 16f), "El andén de Chimbacalle (25 de junio de 1908)", 3.5f);
        Prop(platform, "Andén de piedra", new Vector3(0f, 0.2f, -2.2f), new Vector3(16f, 0.4f, 2.4f), stone);
        for (int k = 0; k < 2; k++) Prop(platform, "Riel", new Vector3(0f, 0.08f, 0.4f + k * 1.1f), new Vector3(22f, 0.12f, 0.1f), iron, false);
        for (int k = 0; k < 9; k++) Prop(platform, "Durmiente", new Vector3(-10f + k * 2.5f, 0.04f, 0.95f), new Vector3(0.3f, 0.08f, 1.8f), wood, false);
        Prop(platform, "Caldera de la locomotora", new Vector3(5f, 1.5f, 0.95f), new Vector3(5f, 1.4f, 1.4f), iron);
        Prop(platform, "Cabina", new Vector3(1.8f, 1.8f, 0.95f), new Vector3(1.8f, 2f, 1.6f), iron);
        Prop(platform, "Chimenea", new Vector3(7f, 2.8f, 0.95f), new Vector3(0.5f, 1.2f, 0.5f), iron, false);
        Prop(platform, "Campana de latón", new Vector3(4.6f, 2.5f, 0.95f), new Vector3(0.4f, 0.4f, 0.4f), brass, false);
        Prop(platform, "Banderines", new Vector3(-4f, 2.6f, -3f), new Vector3(8f, 0.3f, 0.05f), red, false);

        // 2 · EL PLANO DE LA LÍNEA: de Durán a Quito, con la Nariz del Diablo.
        Transform map = Spot(t, "Plano de la línea", new Vector3(-15f, 0f, 8f), "El plano de la línea: Durán, Alausí, la Nariz del Diablo, Riobamba, Quito", 2.8f);
        Prop(map, "Mesa del plano", new Vector3(0f, 0.45f, 0f), new Vector3(2.6f, 0.9f, 1.3f), wood);
        Prop(map, "Plano", new Vector3(0f, 0.92f, 0f), new Vector3(2.3f, 0.02f, 1.1f), paper, false);
        Prop(map, "Maqueta de la Nariz del Diablo", new Vector3(3.4f, 0.8f, 0f), new Vector3(1.6f, 1.6f, 1.6f), stone);

        // 3 · EL ÚLTIMO TRAMO: tres durmientes que faltan (la cuadrilla).
        Transform gap = Spot(t, "Último tramo", new Vector3(13f, 0f, 3f), "El último tramo de vía", 2.2f);
        for (int k = 0; k < 2; k++) Prop(gap, "Riel suelto", new Vector3(0f, 0.08f, -0.55f + k * 1.1f), new Vector3(6f, 0.12f, 0.1f), iron, false);
        var ties = Items(t, wood, new Vector3(1.8f, 0.16f, 0.3f), ("Durmiente de madera", new Vector3(18f, 0.1f, -3f)), ("Durmiente de madera", new Vector3(20f, 0.1f, 1f)), ("Durmiente de madera", new Vector3(17f, 0.1f, 7f)));

        // 4 · LO QUE CAMBIÓ: la escuela laica, el registro civil y el telégrafo.
        Transform school = Spot(t, "Pupitre de la Normal", new Vector3(-11f, 0f, -8f), "La Escuela Normal: maestras laicas", 2.2f);
        Prop(school, "Pupitre", new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.8f, 0.7f), wood);
        Transform registry = Spot(t, "Registro civil", new Vector3(-17f, 0f, -3f), "El Registro Civil: nacer y casarse ante la ley", 2.2f);
        Prop(registry, "Mesa del registro", new Vector3(0f, 0.4f, 0f), new Vector3(1.6f, 0.8f, 0.8f), wood);
        Prop(registry, "Libro de actas", new Vector3(0f, 0.84f, 0f), new Vector3(0.7f, 0.08f, 0.5f), paper, false);
        Transform telegraph = Spot(t, "Telégrafo", new Vector3(16f, 0f, -9f), "El telégrafo: Quito y Guayaquil, en minutos", 2.6f);
        Prop(telegraph, "Poste del telégrafo", new Vector3(0f, 2.2f, 0f), new Vector3(0.18f, 4.4f, 0.18f), wire);
        Prop(telegraph, "Manipulador", new Vector3(1.2f, 0.45f, 0f), new Vector3(1f, 0.9f, 0.6f), brass);
        var changes = new List<Transform> { school, registry, telegraph };

        // 5 · EL CLAVO DE ORO.
        Transform spike = Spot(t, "El clavo de oro", new Vector3(-6f, 0f, 11f), "El último clavo, de oro (América Alfaro)", 2f);
        Prop(spike, "Clavo de oro", new Vector3(0f, 0.2f, 0f), new Vector3(0.1f, 0.4f, 0.1f), gold);

        // La gente: insectos-humanos. Alfaro, una HORMIGA (abre un sendero y por él pasa toda la colonia), de frac oscuro.
        Color black = new Color(0.1f, 0.1f, 0.12f), navy = new Color(0.14f, 0.18f, 0.32f), grey = new Color(0.4f, 0.4f, 0.42f), cream = new Color(0.86f, 0.82f, 0.72f);
        GameObject alfaro = Modern(t, "Eloy Alfaro", new Vector3(3f, 0f, 8f), false, black, CubeInsect.Hat.Brimmed, 1.62f, "Ant");
        GameObject harman = Modern(t, "Archer Harman", new Vector3(-13f, 0f, 11f), false, grey, CubeInsect.Hat.Brimmed, 1.78f, "Grasshopper");
        GameObject america = Modern(t, "América Alfaro", new Vector3(-8f, 0f, 8f), true, cream, CubeInsect.Hat.Bonnet, 1.6f, "Ant");
        GameObject foreman = Who(t, "Capataz jamaicano (Atlas)", new Vector3(15f, 0f, 6f), false, 1.8f, "Termite"); CubeInsect.WearShirt(foreman, new Color(0.45f, 0.38f, 0.3f)); CubeInsect.WearHat(foreman, CubeInsect.Hat.Brimmed); Routine(foreman, gap);
        GameObject teacher = Modern(t, "Maestra normalista (Ruth)", new Vector3(-9f, 0f, -6f), true, navy, CubeInsect.Hat.None, 1.6f, "Earwig"); Routine(teacher, school, registry);
        GameObject clerk = Modern(t, "Telegrafista (Momo)", new Vector3(14f, 0f, -11f), false, grey, CubeInsect.Hat.Cap, 1.68f, "Cricket");
        int n = 6;

        Mission(t, "Eloy Alfaro y el tren de Quito (1908)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con Eloy Alfaro en el andén", alfaro.transform, "Eloy Alfaro", "Hoy cumplo sesenta y seis años y no quiero otro regalo: que un tren llegue a Quito. Decían que la cordillera no se podía subir. Ve a ver cómo se hizo."),
                Reach("Ve al plano de la línea con Archer Harman", map, "Archer Harman", "De Durán, a nivel del mar, a más de tres mil metros. Lo peor fue la Nariz del Diablo: una pared de roca que el tren sube en zigzag, adelante y atrás."),
                Collect("Lleva tres durmientes al último tramo", ties, gap, "Capataz jamaicano", "Vinimos miles desde Jamaica a poner esta vía, y a nuestro lado la gente de la sierra y de la costa. Muchos no vuelven: la fiebre y la roca se los llevaron. Pon bien el durmiente: por aquí pasa su trabajo."),
                Talk("Habla con el capataz de la cuadrilla", foreman.transform, "Capataz", "Cuando pase el tren, todos mirarán la locomotora. Mira tú los rieles: ahí estamos nosotros."),
                Visit("Mira lo que cambió: la escuela, el registro y el telégrafo", changes, "Maestra normalista", "Ahora una muchacha puede estudiar para maestra y un matrimonio se apunta ante la ley. Y lo que pasa en Guayaquil se sabe en Quito en minutos. El tren lleva lo mismo: gente, cartas, ideas, sacos de cacao y de papas."),
                Talk("Habla con el telegrafista", clerk.transform, "Telegrafista", "¡Ya pasó por Latacunga! Dicen que en la sierra salen los campesinos a ver la máquina. Muchos no habían visto nunca una."),
                Reach("Ve con América Alfaro al último clavo", spike, "América Alfaro", "Mi padre quiso que el último clavo fuera de oro y que lo pusiera yo. Pero este clavo es de todos los que trabajaron aquí."),
                Hold("Espera en el andén a que llegue el tren", platform, 10f, "Eloy Alfaro", "¡Ahí viene! De Durán a Quito sin bajarse: la costa y la sierra, por fin, un solo país. Ahora, a ponerlo a trabajar."),
            },
            "Chimbacalle, al sur de Quito, el 25 de junio de 1908, sobre el taller de Mecánica del santuario a escala de hormiga (pendiente de su piso: una estación de ferrocarril). Hoy llega por primera vez un tren de Durán a Quito, el día en que el presidente Eloy Alfaro cumple 66 años. Conoce a quienes lo hicieron posible. E para hablar y recoger; para esperar el tren, quédate quieto.",
            new[]
            {
                "José Eloy Alfaro Delgado nació en Montecristi (Manabí) el 25 de junio de 1842. Tras décadas de lucha política, llegó al poder con la Revolución Liberal de 1895 y fue presidente de Ecuador en 1895–1901 y 1906–1911. Su gobierno separó la Iglesia del Estado, creó el registro y el matrimonio civil, impulsó la enseñanza laica y las escuelas normales (como la Manuela Cañizares, para mujeres, de 1901) y abrió empleos públicos a las mujeres.",
                "El ferrocarril era su gran obra: en 1897 firmó el contrato con el empresario estadounidense Archer Harman, y su hermano John fue el ingeniero jefe. La línea sube desde Durán, frente a Guayaquil, hasta más de 3600 m, y cruza la Nariz del Diablo, una pared de roca que se salva con un zigzag. Miles de trabajadores llegaron de Jamaica, junto a miles de ecuatorianos, y muchos murieron por las fiebres y los accidentes.",
                "El primer tren llegó a Chimbacalle el 25 de junio de 1908, el cumpleaños de Alfaro. Su hija América puso el último clavo, de oro (las fuentes difieren en el día). La línea unió la costa y la sierra, que hasta entonces se separaban por varios días de mula.",
                "Alfaro es una figura admirada y también discutida: gobernó en años de guerra civil y con poderes extraordinarios en varios momentos. Fue asesinado por una turba en Quito el 28 de enero de 1912. Esta historia se limita a aquel día de 1908 y a quienes construyeron la vía.",
            },
            new[]
            {
                "Clark, A. K. (1998). The Redemptive Work: Railway and Nation in Ecuador, 1895–1930. Scholarly Resources.",
                "Ayala Mora, E. (1994). Historia de la Revolución Liberal ecuatoriana. Corporación Editora Nacional.",
                "Alfaro, E. Historia del Ferrocarril de Guayaquil a Quito (citado en El Telégrafo, «Hace 105 años el tren viajó de Durán a Quito», 2013).",
                "Vicepresidencia de Ecuador (2018), discurso por los 110 años de la llegada del ferrocarril a Quito.",
            },
            "Eloy Alfaro", "Un país se une con rieles y con escuelas. Lo demás viene después.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style QuitoAdobe = new Style(C(0.92f, 0.9f, 0.82f), C(0.55f, 0.42f, 0.32f), C(0.62f, 0.32f, 0.22f), Roof.Gable, 3.6f);   // Quito: adobe encalado y teja
    static readonly Style RailShed = new Style(C(0.55f, 0.45f, 0.38f), C(0.42f, 0.4f, 0.38f), C(0.4f, 0.4f, 0.42f), Roof.Gable, 5f);        // los talleres: ladrillo y chapa

    // CHIMBACALLE (1908): la estación al sur del Panecillo, la vía que llega del sur por Turubamba y sigue al noreste, el río Machángara
    // al este, el camino a Quito por Maldonado, el Pichincha al oeste. Plano aproximado y ACORTADO al norte (el centro de Quito queda tras
    // el Panecillo, fuera). (Atlas histórico de Quito; OpenStreetMap, «Estación de Chimbacalle».)
    const string PlanChimbacalle = @"
calle Vía del ferrocarril Durán–Quito | 6 | -1100 -900, -420 -520, 0 90, 620 420
calle Calle Maldonado (camino a Quito) | 12 | 100 -1000, 60 -100, 40 180, 150 1100
calle Calle de la estación | 10 | -300 -60, 520 -60
calle Calle de Chimbacalle | 8 | -700 -320, 700 -320
calle Calle de los talleres | 8 | -500 240, 450 240
calle Camino de Turubamba | 8 | -420 -520, -900 -1200
calle Calle del Machángara | 6 | 300 -700, 520 700
rio Río Machángara | 14 | 700 -1200, 620 -400, 560 240, 700 1200
monte Pichincha | 700 1500 | -1700 500
colina El Panecillo | 240 160 | -120 760
colina Lomas de Puengasí | 240 120 | 1250 -300
campo Haciendas de Turubamba | 420 260 | -760 -880
campo Chacras de maíz y papas | 260 180 | 400 -860
hito 0 | 40 90
hito 1 | -160 90
hito 2 | 200 300
hito 3 | 160 -40
hito 4 | -60 -140
hito 5 | -320 -190
hito 6 | 260 -60
hito 7 | 420 -500
Plaza | 120 -120
Pileta | 170 -210
Parque | 300 520
Mercado | 340 -240
Estacion | -420 -480
Mecanica | -420 150
Carpinteria | -300 -400
Taller | 220 -420
Estudio | 330 200
Entrenamiento | -40 420 | actual: canchas de pelota nacional del sur de Quito; posición aproximada
Sanacion | 360 60 | actual: un hospital del sur de Quito, acercado; posición aproximada
Piscina | 460 -120 | actual: baños de la orilla del Machángara; posición aproximada
Baile | -120 -420
Bomberos | -150 300
Policia | 10 -300
Hotel | -200 -40
Guarderia | 240 -520
Tecnologia | -40 -60
Investigacion | 120 860 | actual: Observatorio Astronómico de Quito (1873), acercado; posición aproximada
Obra | -520 -240
relleno | 55 | 100 -300";

    static int ChimbacalleTown(Transform t)
    {
        var s = Town(t, "Chimbacalle", new[]
        {
            new Lot("Estación de Chimbacalle", "Estación", "Estación de Chimbacalle (Ferrocarril de Guayaquil a Quito)", 18f, 10f, RailShed,
                S("anden", -3f, 2f, 8f, 3f, "Andén"), S("puesto", 3f, -2f, 2f, 1f, "Taquilla"), S("estante", 6f, -3f, 0.8f, 1.6f, "Horarios y paquetes")),
            new Lot("Talleres del ferrocarril", "Taller", "Talleres del ferrocarril", 18f, 12f, RailShed,
                S("banco_taller", -5f, 3f, 2.4f, 1f, "Banco del ajustador"), S("torno", 0f, 3f, 1.6f, 1f, "Torno de ruedas"), S("horno", 5f, 3f, 1.6f, 1.6f, "Fragua"), S("armario", 0f, -4f, 1.6f, 0.6f, "Llaves y calibres")),
            new Lot("Escuela Normal Manuela Cañizares", "Escuela", "Escuela Normal para maestras (fundada en 1901)", 16f, 12f, QuitoAdobe,
                S("pizarra", 0f, 4f, 2.4f, 0.4f, "Pizarra"), S("mesa", 0f, 0f, 2.4f, 1f, "Pupitres"), S("estante", -5f, -3f, 0.8f, 1.6f, "Libros de pedagogía")),
            new Lot("Fonda de la estación", "Restaurante", "Fonda de la estación (locro de papas y hornado)", 14f, 10f, QuitoAdobe,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Olla de locro"), S("mesa", 2f, -1f, 2f, 1f, "Mesa larga"), S("barra", 3f, 3f, 2f, 0.8f, "Mostrador de hornado")),
            new Lot("Casa de los Alfaro en Quito", "Casa", "Casa de la familia Alfaro (de visita en Chimbacalle)", 12f, 10f, QuitoAdobe,
                S("cama", -2f, 2f, 1.4f, 2f, "Cama de Eloy Alfaro"), S("cama", 2f, 2f, 1.2f, 2f, "Cama de América"), S("estante", 4f, -3f, 0.8f, 1.6f, "Libros y cartas")),
            new Lot("Casa de la cuadrilla", "Casa", "Casa de la cuadrilla (trabajadores de Jamaica y de la sierra)", 14f, 10f, QuitoAdobe,
                S("cama", -4f, 2f, 1f, 2f, "Catre"), S("cama", 0f, 2f, 1f, 2f, "Catre"), S("cama", 4f, 2f, 1f, 2f, "Catre")),
            new Lot("Registro Civil", "Taller de papeles", "Oficina del Registro Civil", 12f, 10f, QuitoAdobe,
                S("mesa", 0f, 2f, 2f, 1f, "Mesa del registrador"), S("armario", 3f, -3f, 1.2f, 0.6f, "Libros de actas")),
            new Lot("Chacra de la Villaflora", "Huerto", "Chacra de maíz, papas y habas", 14f, 12f, QuitoAdobe,
                S("bancal", -3f, 0f, 4f, 6f, "Maíz, habas y papas"), S("pozo", 4f, 2f, 1.2f, 1.2f, "Acequia")),
        }, new[]
        {
            new Lot("Casa de Chimbacalle", "Casa", "Casa de adobe de Chimbacalle", 10f, 9f, QuitoAdobe),
            new Lot("Tienda del barrio", "Mercado", "Tienda del barrio (panela, sal, velas)", 10f, 8f, QuitoAdobe, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller de zapatero", "Taller", "Taller de zapatero", 10f, 8f, QuitoAdobe, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del zapatero"), S("armario", 0f, 0f, 1.2f, 0.6f, "Hormas y cuero")),
            new Lot("Chacra del barrio", "Huerto", "Chacra del barrio", 12f, 10f, QuitoAdobe, S("bancal", 0f, 0f, 4f, 6f, "Maíz"), S("pozo", 0f, 0f, 1f, 1f, "Acequia")),
        }, C(0.5f, 0.46f, 0.42f));
        int n = 0;
        n += W(t, "Jefe de estación", s[0], "", s[3], null, null);
        n += W(t, "Ajustador de los talleres", s[1], "mecanico", s[3], null, s[5]);
        n += W(t, "Maquinista de Durán", s[1], "mecanico", s[3], null, s[5]);
        n += W(t, "Directora de la Normal", s[2], "sabio", s[3], null, null);
        n += W(t, "Alumna normalista", s[2], "alumno", s[3], s[2], null);
        n += W(t, "Cocinera de la fonda", s[3], "cocinero", s[3], null, null);
        n += W(t, "Registrador civil", s[6], "", s[3], null, null);
        n += W(t, "Chacarera de la Villaflora", s[7], "hortelano", s[3], null, null);
        n += Townsfolk(t, s, 8, "Vecina de Chimbacalle", "Vecino de Chimbacalle", s[3], null);
        return n;
    }
}

public static partial class MicroBiome
{
    // Quito (2850 m), junio: el verano seco de la sierra. Eucaliptos recién traídos (desde 1865), capulíes, pencos, sigses, chilca y retama;
    // páramo de paja en el Pichincha. (Jørgensen y León-Yánez 1999, Catalogue of the Vascular Plants of Ecuador.)
    static Biome ChimbacalleBiome() => new Biome { title = "Quito, sierra andina (junio de 1908, el verano seco)", ground = C(0.5f, 0.42f, 0.32f), plants = new[] {
            new Plant(Kind.Grass, 14, "Paja de páramo (Calamagrostis intermedia)", 80, C(0.72f, 0.66f, 0.42f), C(0.8f, 0.72f, 0.5f)),
            new Plant(Kind.Shrub, 6, "Chilca (Baccharis latifolia)", 250, C(0.3f, 0.48f, 0.28f), C(0.85f, 0.85f, 0.7f)),
            new Plant(Kind.Grass, 8, "Sigse (Cortaderia nitida)", 220, C(0.5f, 0.58f, 0.36f), C(0.9f, 0.86f, 0.72f)),
            new Plant(Kind.Shrub, 5, "Penco (Agave americana)", 180, C(0.45f, 0.6f, 0.5f), C(0.5f, 0.55f, 0.4f)),
            new Plant(Kind.Flower, 4, "Retama (Spartium junceum)", 200, C(0.35f, 0.5f, 0.3f), C(0.95f, 0.8f, 0.15f)),
            new Plant(Kind.Stone, 8, "Andesita del Pichincha", 0, C(0.5f, 0.5f, 0.52f), Stone) },
          town = new[] {
            new Plant(Kind.Shrub, 8, "Eucalipto (Eucalyptus globulus), plantado", 1800, C(0.42f, 0.52f, 0.45f), C(0.6f, 0.52f, 0.45f)),
            new Plant(Kind.Shrub, 4, "Capulí (Prunus serotina), de patio", 700, C(0.25f, 0.45f, 0.22f), C(0.25f, 0.08f, 0.12f)),
            new Plant(Kind.Grass, 10, "Hierba del camino", 15, C(0.5f, 0.58f, 0.3f), C(0.55f, 0.55f, 0.3f)) },
          bugs = new[] {
            new Bug("SnowBee", 3, 22, C(0.15f, 0.14f, 0.14f), "Abejorro del páramo (Bombus funebris)"),
            new Bug("Ant", 5, 7, C(0.3f, 0.2f, 0.15f), "Hormiga de los eucaliptos"),
            new Bug("Grasshopper", 3, 30, C(0.55f, 0.5f, 0.35f), "Saltamontes de la paja (Trimerotropis)") },
          townFauna = new[] {
            new Pet("Aphid", 3, 1.6f, C(0.55f, 0.5f, 0.45f), "Burro de carga (en el microcosmos, un pulgón de carga)"),
            new Pet("Woodlouse", 3, 0.7f, C(0.7f, 0.6f, 0.45f), "Perro del barrio (en el microcosmos, una cochinilla hormiguera)") } };
}
