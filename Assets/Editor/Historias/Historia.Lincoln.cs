using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LINCOLN EN GETTYSBURG (docs/microcosmos-historias-3.md, con la decisión: Berón preguntó si fue tan bueno y, si no, el mejor presidente de EE. UU.;
/// las encuestas de historiadores lo ponen 1.º o 2.º, y es él). Gettysburg, 19 de noviembre de 1863: se dedica el cementerio nacional, cuatro meses
/// después de la batalla. La escena es civil: un pueblo que entierra a sus muertos, un hombre negro que los recoge, una mujer que guarda la
/// puerta, y dos minutos de discurso. Ninguna batalla: solo lo que queda después.
/// </summary>
public static partial class StorySceneBuilder
{
    static int Lincoln(Transform t)
    {
        Material pine = M("Tablas_de_pino", 0.6f, 0.46f, 0.3f), stone = M("Granito_de_Gettysburg", 0.56f, 0.54f, 0.52f), brick = M("Ladrillo_de_Pensilvania", 0.6f, 0.32f, 0.24f), bunting = M("Colgadura_azul", 0.15f, 0.2f, 0.45f);
        Material grass = M("Hierba_de_noviembre", 0.55f, 0.52f, 0.32f), paper = M("Hoja_del_discurso", 0.92f, 0.88f, 0.76f), flower = M("Corona_de_laurel", 0.25f, 0.42f, 0.22f), earth = M("Tierra_removida", 0.36f, 0.28f, 0.2f);

        // 1 · EL ESTRADO: la tribuna de pino con colgaduras, delante del semicírculo de tumbas.
        Transform stage = Spot(t, "Estrado del cementerio", new Vector3(0f, 0f, 15f), "El estrado: 19 de noviembre de 1863", 3.5f);
        Prop(stage, "Tablado de pino", new Vector3(0f, 0.5f, 0f), new Vector3(9f, 1f, 4f), pine);
        Prop(stage, "Colgadura", new Vector3(0f, 0.8f, -2.05f), new Vector3(9f, 0.5f, 0.05f), bunting, false);
        Prop(stage, "Atril", new Vector3(0f, 1.6f, -1f), new Vector3(0.8f, 1.2f, 0.4f), pine);

        // 2 · EL SEMICÍRCULO DE TUMBAS: por estados, sin rangos (diseño de William Saunders).
        Transform graves = Spot(t, "Semicírculo de tumbas", new Vector3(0f, 0f, 21f), "Las tumbas, por estados y sin rangos", 3f);
        for (int r = 0; r < 2; r++) for (int k = 0; k < 9; k++) { float a = Mathf.PI * (k / 8f); float rad = 6f + r * 2f; Prop(graves, "Hilera de tumbas", new Vector3(Mathf.Cos(a) * rad, 0.15f, Mathf.Sin(a) * rad * 0.5f), new Vector3(1.2f, 0.3f, 0.5f), stone); }
        var wreaths = Items(t, flower, new Vector3(0.5f, 0.15f, 0.5f), ("Corona de laurel", new Vector3(-12f, 0.08f, 3f)), ("Corona de laurel", new Vector3(-16f, 0.08f, -2f)), ("Corona de laurel", new Vector3(-9f, 0.08f, -5f)));

        // 3 · LA CASA DE WILLS (el discurso a medio pasar a limpio).
        Transform wills = Spot(t, "Mesa de la casa Wills", new Vector3(14f, 0f, 9f), "La casa de David Wills: Lincoln termina el discurso", 2.4f);
        Prop(wills, "Mesa del dormitorio", new Vector3(0f, 0.4f, 0f), new Vector3(1.6f, 0.8f, 0.8f), pine);
        Prop(wills, "Hojas del discurso", new Vector3(0f, 0.84f, 0f), new Vector3(0.5f, 0.02f, 0.7f), paper, false);

        // 4 · LO QUE QUEDA: la puerta del cementerio Evergreen, la carretilla de Biggs y la proclama.
        Transform gatehouse = Spot(t, "Puerta del cementerio Evergreen", new Vector3(-14f, 0f, 11f), "La puerta del cementerio Evergreen", 3f);
        for (int k = 0; k < 2; k++) Prop(gatehouse, "Pilar de la puerta", new Vector3(-1.6f + k * 3.2f, 1.6f, 0f), new Vector3(1.2f, 3.2f, 1.2f), brick);
        Prop(gatehouse, "Arco", new Vector3(0f, 2.9f, 0f), new Vector3(3.2f, 0.6f, 1f), brick);
        Transform cart = Spot(t, "Carro de Basil Biggs", new Vector3(10f, 0f, -9f), "El carro de Basil Biggs: traer a cada soldado a su tumba", 2.4f);
        Prop(cart, "Carro de tablas", new Vector3(0f, 0.6f, 0f), new Vector3(2.8f, 0.7f, 1.4f), pine);
        Prop(cart, "Tierra removida", new Vector3(3f, 0.15f, 0f), new Vector3(2f, 0.3f, 1.4f), earth, false);
        Transform proc = Spot(t, "Mesa de la proclama", new Vector3(16f, 0f, -2f), "La Proclamación de Emancipación (1 de enero de 1863)", 2.2f);
        Prop(proc, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.6f, 0.8f, 0.8f), pine);
        Prop(proc, "Pliego de la proclama", new Vector3(0f, 0.84f, 0f), new Vector3(0.7f, 0.02f, 0.5f), paper, false);
        Transform hospital = Spot(t, "Seminario hospital", new Vector3(-6f, 0f, -10f), "El seminario: aún hay heridos", 2.4f);
        Prop(hospital, "Catre", new Vector3(0f, 0.3f, 0f), new Vector3(2f, 0.6f, 0.9f), pine);
        var after = new List<Transform> { gatehouse, cart, proc, hospital };
        Prop(t, "Campo de noviembre", new Vector3(19f, 0.02f, 14f), new Vector3(8f, 0.04f, 16f), grass, false);

        // La gente: insectos-humanos. Lincoln, una MANTIS (alta, delgada y paciente), con levita y chistera.
        Color black = new Color(0.08f, 0.08f, 0.1f), grey = new Color(0.36f, 0.36f, 0.38f), brown = new Color(0.36f, 0.26f, 0.2f), rose = new Color(0.45f, 0.3f, 0.32f);
        GameObject abe = Modern(t, "Abraham Lincoln", new Vector3(3f, 0f, 8f), false, black, CubeInsect.Hat.Brimmed, 1.93f, "Mantis");
        GameObject wills2 = Modern(t, "David Wills", new Vector3(12f, 0f, 6f), false, grey, CubeInsect.Hat.None, 1.72f, "Ant"); Routine(wills2, wills, stage);
        GameObject elizabeth = Modern(t, "Elizabeth Thorn", new Vector3(-12f, 0f, 8f), true, rose, CubeInsect.Hat.Bonnet, 1.6f, "Earwig"); Routine(elizabeth, gatehouse);
        GameObject basil = Modern(t, "Basil Biggs", new Vector3(8f, 0f, -6f), false, brown, CubeInsect.Hat.Brimmed, 1.75f, "DungBeetle"); Routine(basil, cart);
        GameObject everett = Modern(t, "Edward Everett", new Vector3(-3f, 0f, 9f), false, grey, CubeInsect.Hat.None, 1.7f, "Cicada");
        GameObject girl = Who(t, "Niña de Gettysburg (Momo)", new Vector3(5f, 0f, 3f), true, 1.3f, "Ant"); CubeInsect.WearShirt(girl, rose);
        int n = 6;

        Mission(t, "Lincoln en Gettysburg (1863)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con David Wills en su casa de la plaza", wills2.transform, "David Wills", "Soy el abogado del pueblo. En julio hubo aquí cincuenta mil bajas en tres días. Yo pedí un cementerio para los soldados de la Unión, y al presidente unas pocas palabras. Solo unas pocas: el orador es el señor Everett."),
                Reach("Sube a la mesa donde Lincoln termina el discurso", wills, "David Wills", "Durmió aquí anoche. Trajo el discurso a medias, y lo ha acabado aquí. Dos hojas. Tiene un hijo enfermo en Washington y aun así ha venido."),
                Collect("Lleva tres coronas a las tumbas", wreaths, graves, "Niña de Gettysburg", "Las tumbas están en semicírculo, por estados y no por rangos: un general no estaría más alto que un soldado. Es lo que quiso el que las diseñó."),
                Visit("Mira lo que queda: la puerta, el carro, la proclama y el seminario", after, "Basil Biggs", "Soy negro y libre, y huí con mi familia cuando llegó el ejército de Lee: a los negros libres se los llevaban al Sur. He vuelto, y trabajo con mis hombres trayendo a estos soldados a su tumba, uno a uno. El presidente firmó en enero la emancipación. Por eso estoy aquí."),
                Talk("Habla con Elizabeth Thorn, en la puerta del cementerio", elizabeth.transform, "Elizabeth Thorn", "Mi marido está en el ejército y yo guardo el cementerio. Tras la batalla enterré a más de noventa soldados con mis manos, embarazada de seis meses, y con mi padre. Nadie viene a dar un discurso por eso. Por eso escucho."),
                Hold("Escucha al orador, el señor Everett", stage, 8f, "Edward Everett", "Dos horas de discurso, de memoria, como en la antigua Atenas, con la batalla contada día por día..."),
                Hold("Escucha a Lincoln: dos minutos", stage, 12f, "Abraham Lincoln", "Hace ochenta y siete años nuestros padres crearon en este continente una nación nueva, concebida en libertad y dedicada a la idea de que todos los hombres son creados iguales... y que el gobierno del pueblo, por el pueblo y para el pueblo no desaparezca de la tierra."),
                Talk("Habla con Lincoln al bajar", abe.transform, "Abraham Lincoln", "No ha gustado mucho, creo. Lo importante no es lo que digamos aquí, sino lo que ellos hicieron. Queda la tarea: que esa frase, «todos los hombres son creados iguales», sea verdad para todos."),
            },
            "Gettysburg (Pensilvania), el 19 de noviembre de 1863, sobre la Sala de asambleas del santuario a escala de hormiga. Cuatro meses después de la batalla, el pueblo entierra a sus muertos y el presidente Abraham Lincoln viene a decir unas pocas palabras. E para hablar y recoger; para escuchar, quédate quieto.",
            new[]
            {
                "Abraham Lincoln nació en Kentucky en 1809, en una cabaña de troncos, casi sin escuela: fue autodidacta. Fue abogado en Illinois y presidente de Estados Unidos de 1861 a 1865. Al ganar las elecciones, los estados del Sur, esclavistas, se separaron, y empezó la guerra civil, que costó unos 750 000 muertos.",
                "El 1 de enero de 1863 firmó la Proclamación de Emancipación, que declaraba libres a los esclavos de los estados rebeldes. Abrió el ejército a los soldados negros. En julio, en Gettysburg, hubo la mayor batalla de la guerra. En noviembre, el abogado David Wills organizó el cementerio y le pidió «unas pocas palabras». El 19 de noviembre, tras dos horas de Edward Everett, Lincoln habló unos dos minutos, en unas 272 palabras. Everett le escribió después que le habría gustado acercarse a la idea central tanto en dos horas como él en dos minutos.",
                "Elizabeth Thorn, cuidadora del cementerio Evergreen mientras su marido estaba en el ejército, enterró a unos 100 soldados, embarazada de seis meses. Basil Biggs, afroamericano libre y veterinario, fue subcontratado para exhumar y volver a enterrar a más de 3500 soldados de la Unión. La Decimotercera Enmienda, que abolió la esclavitud en todo el país, pasó por el Congreso en enero de 1865; Lincoln fue asesinado en abril.",
                "¿Fue tan bueno? La encuesta C-SPAN a historiadores lo pone el primero en todas sus ediciones (2000, 2009, 2017 y 2021), y la del Siena College de 2022 lo pone segundo, tras Franklin D. Roosevelt. Tuvo también sus sombras: suspendió el habeas corpus en la guerra, en 1862 autorizó ejecutar a 38 hombres dakota (conmutó la pena a más de 260), y durante años propuso enviar a los afroamericanos fuera del país, idea que abandonó. Las frases son de juego, salvo las del discurso, abreviado.",
            },
            new[]
            {
                "Lincoln, A. (1863). Discurso de Gettysburg (copia Bliss), Biblioteca del Congreso.",
                "National Park Service, «David Wills House» (nps.gov/gett).",
                "Wills, G. (1992). Lincoln at Gettysburg. Simon & Schuster.",
                "McPherson, J. (1988). Battle Cry of Freedom. Oxford University Press; Foner, E. (2010). The Fiery Trial. Norton.",
                "C-SPAN (2021), Presidential Historians Survey (press release); Siena College Research Institute (2022), US Presidents Study (sri.siena.edu/us-presidents-study).",
                "Pennsylvania Quest for Freedom, «Basil Biggs»; Gettysburg Daily, «Elizabeth Thorn».",
            },
            "Abraham Lincoln", "Aún nos queda la tarea que ellos dejaron sin acabar.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style GettysburgBrick = new Style(C(0.62f, 0.34f, 0.26f), C(0.5f, 0.42f, 0.32f), C(0.32f, 0.3f, 0.3f), Roof.Gable, 3.8f);   // ladrillo de Pensilvania
    static readonly Style GettysburgWood = new Style(C(0.9f, 0.88f, 0.82f), C(0.5f, 0.4f, 0.3f), C(0.34f, 0.32f, 0.3f), Roof.Gable, 3.4f);     // casa de tablas

    // GETTYSBURG (1863): el Diamond (plaza) de donde salen York, Chambersburg, Baltimore y Carlisle; el Seminario al oeste, el Pennsylvania College al
    // norte y la estación en Carlisle Street; Cemetery Hill al sur con el Evergreen y el cementerio nacional; Culp's Hill, Rock Creek y los montes South
    // al oeste. La plaza queda al NORTE del claro, que es Cemetery Hill: la historia ocurre en el cementerio. Plano ACORTADO. (NPS; plano de Warren, 1868.)
    const string PlanGettysburg = @"
calle Baltimore Street | 12 | 40 -1000, 20 120, 0 780
calle Carlisle Street | 10 | 0 780, 20 1300
calle York Street | 10 | 0 780, 1300 960
calle Chambersburg Street | 10 | 0 780, -1300 960
calle Taneytown Road | 8 | -40 60, -160 -1200
calle Emmitsburg Road | 8 | -40 200, -700 -1200
calle Steinwehr Avenue | 8 | -280 260, 20 200
calle Middle Street | 8 | -500 650, 500 650
calle Calle del seminario | 8 | -500 780, -500 980
rio Rock Creek | 12 | 860 1300, 620 500, 700 -300, 640 -1300
colina Cemetery Hill | 260 30 | 60 120
colina Culp's Hill | 220 60 | 620 -60
colina Seminary Ridge | 200 28 | -660 760
monte Round Tops | 300 180 | -440 -1320 | seco
campo Campos de trigo de Rose (rastrojo) | 300 200 | -700 -640
campo Huertos de manzanos del condado de Adams | 300 200 | 1000 -760
campo Prados de McPherson | 280 180 | -1100 900
hito 0 | 60 760
hito 1 | -40 120
hito 2 | 60 -20
hito 3 | -560 840
hito 4 | 60 960
hito 5 | -300 -520
hito 6 | 110 1120
hito 7 | 200 730
hito 8 | -130 600
hito 9 | -180 1120
Plaza | 0 800
Pileta | -80 760
Mercado | 200 860
Parque | -180 300
Piscina | 860 500
Sanacion | -700 700
Estudio | 340 680
Bomberos | 400 860
Policia | -200 760
Taller | -360 900
Carpinteria | 380 500
Mecanica | 420 960
Guarderia | -280 480
Museo | 280 300
Monumento | 40 680
Obra | 640 860
Tecnologia | 150 940
relleno | 46 | 60 600";

    static int GettysburgTown(Transform t)
    {
        var s = Town(t, "Gettysburg", new[]
        {
            new Lot("Casa de David Wills", "Casa", "Casa de David Wills (en el Diamond)", 14f, 12f, GettysburgBrick,
                S("cama", -3f, 2f, 1.6f, 2f, "Cama del presidente"), S("mesa", 3f, 2f, 1.6f, 1f, "Mesa donde terminó el discurso"), S("estante", 3f, -3f, 0.8f, 1.6f, "Libros de leyes")),
            new Lot("Casa de la puerta del Evergreen", "Casa", "Casa de la puerta del cementerio Evergreen (los Thorn)", 12f, 10f, GettysburgBrick,
                S("cama", -2f, 2f, 1.4f, 2f, "Cama de Elizabeth Thorn"), S("cama", 3f, 2f, 1.2f, 2f, "Cama de los niños"), S("armario", 3f, -3f, 1.2f, 0.6f, "Palas y registros")),
            new Lot("Cementerio Nacional de los Soldados", "Parque", "Cementerio Nacional de los Soldados", 22f, 18f, GettysburgWood,
                S("banco", -5f, 0f, 4f, 1.6f, "Banco del cementerio"), S("banco", 5f, 0f, 4f, 1.6f, "Banco del cementerio"), S("frutal", 0f, 4f, 1.6f, 1.6f, "Manzano (Malus domestica)"), S("fuente", 0f, -4f, 4.4f, 4.4f, "Fuente del cementerio")),
            new Lot("Seminario Luterano", "Hospital", "Seminario Luterano, hospital de la batalla", 16f, 12f, GettysburgBrick,
                S("camilla", -4f, 2f, 1f, 2f, "Catre de heridos"), S("cama", 0f, 2f, 1.2f, 2f, "Cama"), S("armario", 4f, 2f, 1.2f, 0.6f, "Vendas y láudano"), S("lavabo", 0f, -3f, 1f, 1f, "Palangana")),
            new Lot("Estación de Gettysburg", "Estación", "Estación de Gettysburg (llegó aquí el tren del presidente)", 16f, 10f, GettysburgBrick,
                S("anden", -2f, 2f, 8f, 3f, "Andén"), S("puesto", 4f, -2f, 2f, 1f, "Taquilla y telégrafo")),
            new Lot("Granja de Basil Biggs", "Huerto", "Granja de Basil Biggs", 16f, 12f, GettysburgWood,
                S("bancal", -4f, 0f, 4f, 6f, "Maíz y heno"), S("bancal", 2f, 0f, 4f, 6f, "Hortalizas"), S("pozo", 6f, 3f, 1.2f, 1.2f, "Pozo")),
            new Lot("Pennsylvania College", "Escuela", "Pennsylvania College", 18f, 12f, GettysburgBrick,
                S("pizarra", 0f, 4f, 2.4f, 0.4f, "Pizarra"), S("mesa", -3f, -1f, 2.4f, 1f, "Pupitres"), S("estante", 5f, 3f, 0.8f, 2f, "Biblioteca")),
            new Lot("Posada McClellan", "Restaurante", "Posada del Diamond (comidas y camas)", 14f, 10f, GettysburgBrick,
                S("cocina", -3f, 2f, 2f, 0.8f, "Cocina de leña"), S("mesa", 2f, -1f, 2f, 1f, "Mesa larga"), S("barra", 3f, 3f, 2f, 0.8f, "Mostrador")),
            new Lot("Iglesia de San Pablo", "Templo", "Iglesia metodista africana de San Pablo", 14f, 10f, GettysburgWood,
                S("altar", 0f, 3f, 2f, 1f, "Púlpito"), S("banco", -3f, -2f, 2f, 0.6f, "Banco"), S("banco", 3f, -2f, 2f, 0.6f, "Banco")),
            new Lot("Patio de gimnasia del College", "Centro de entrenamiento", "Patio de gimnasia del College (barra, paralelas y mazas)", 16f, 14f, GettysburgWood, TrainingKit("turnhalle")),
        }, new[]
        {
            new Lot("Casa de Gettysburg", "Casa", "Casa de ladrillo o tablas de Gettysburg", 10f, 9f, GettysburgWood),
            new Lot("Tienda del Diamond", "Mercado", "Tienda del Diamond", 10f, 8f, GettysburgBrick, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller del carretero", "Taller", "Taller de carros de Gettysburg", 12f, 10f, GettysburgWood, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del carretero"), S("armario", 0f, 0f, 1.2f, 0.6f, "Herramientas")),
            new Lot("Huerto de manzanos", "Huerto", "Huerto de manzanos del condado de Adams", 12f, 10f, GettysburgWood, S("bancal", 0f, 0f, 4f, 6f, "Manzanos jóvenes"), S("pozo", 0f, 0f, 1f, 1f, "Pozo")),
        }, C(0.45f, 0.4f, 0.34f));
        int n = 0;
        n += W(t, "Enfermera voluntaria del seminario", s[3], "", s[7], s[8], null);
        n += W(t, "Médico del seminario", s[3], "sabio", s[7], null, null);
        n += W(t, "Jefe de estación", s[4], "", s[7], null, null);
        n += W(t, "Trabajador de Biggs", s[5], "hortelano", s[7], s[8], null);
        n += W(t, "Profesor del College", s[6], "sabio", s[7], null, null);
        n += W(t, "Estudiante del College", s[6], "alumno", s[7], s[9], null);
        n += W(t, "Cocinera de la posada", s[7], "cocinero", s[7], null, null);
        n += W(t, "Pastor de San Pablo", s[8], "", s[7], null, null);
        n += Townsfolk(t, s, 10, "Vecina de Gettysburg", "Vecino de Gettysburg", s[7], s[8]);
        return n;
    }
}

public static partial class MicroBiome
{
    // Gettysburg, noviembre: otoño tardío de los Apalaches. Robles y nogales desnudos, rastrojo de trigo, manzanos del condado de Adams (ya
    // cosechados), rocas de diabasa en las lomas; pocos bichos: grillos de otoño, arañas. Caballos, mulas y perros. (NPS, Gettysburg Vegetation.)
    static Biome GettysburgBiome() => new Biome { title = "Gettysburg, Pensilvania (noviembre de 1863)", ground = C(0.45f, 0.4f, 0.3f), plants = new[] {
            new Plant(Kind.Shrub, 7, "Roble blanco (Quercus alba), sin hojas", 2000, C(0.5f, 0.42f, 0.3f), C(0.45f, 0.38f, 0.3f)),
            new Plant(Kind.Shrub, 5, "Nogal negro (Juglans nigra)", 1800, C(0.5f, 0.42f, 0.28f), C(0.4f, 0.32f, 0.25f)),
            new Plant(Kind.Grass, 14, "Rastrojo de trigo", 25, C(0.72f, 0.65f, 0.42f), C(0.75f, 0.68f, 0.45f)),
            new Plant(Kind.Grass, 10, "Hierba de noviembre", 15, C(0.55f, 0.52f, 0.32f), C(0.6f, 0.55f, 0.35f)),
            new Plant(Kind.Stone, 8, "Diabasa de Gettysburg", 0, C(0.42f, 0.42f, 0.42f), Stone) },
          town = new[] {
            new Plant(Kind.Shrub, 6, "Manzano del condado de Adams (Malus domestica)", 500, C(0.45f, 0.42f, 0.3f), C(0.75f, 0.15f, 0.12f)),
            new Plant(Kind.Shrub, 4, "Arce rojo (Acer rubrum), de la calle", 1500, C(0.65f, 0.3f, 0.2f), C(0.45f, 0.38f, 0.3f)),
            new Plant(Kind.Grass, 8, "Césped del patio", 8, C(0.5f, 0.55f, 0.32f), C(0.5f, 0.52f, 0.32f)) },
          bugs = new[] {
            new Bug("Cricket", 3, 20, C(0.22f, 0.16f, 0.1f), "Grillo de otoño (Gryllus pennsylvanicus)"),
            new Bug("Spider", 2, 10, C(0.4f, 0.32f, 0.25f), "Araña del campo") },
          townFauna = new[] {
            new Pet("Aphid", 3, 2f, C(0.45f, 0.35f, 0.28f), "Caballo del carro (en el microcosmos, un pulgón de tiro)"),
            new Pet("Woodlouse", 3, 0.7f, C(0.7f, 0.6f, 0.45f), "Perro del pueblo (en el microcosmos, una cochinilla hormiguera)") } };
}
