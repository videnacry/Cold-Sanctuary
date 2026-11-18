using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PILOTO DEL REALISMO (docs/plan-microcosmos-realismo.md, fase 1; Berón 2026-11-18): MICROCOSMOS_ELONMUSK, la fábrica de SpaceX en
/// 1 Rocket Road, Hawthorne (California), entre agosto y el 28 de septiembre de 2008. Es el primer microcosmos con las reglas nuevas:
/// - el lugar real a 1:1 (E2, P1), con MUROS y TECHO, su PUERTA y la VISTA DE CORTE (P2–P4: CivicBuilder.Enclose, CutawayView,
///   MicroDoor), un tabique de aseos con la puerta real ensanchada a 1,2 m (P4, «supuesto»);
/// - un HITO reconocible (P5): el rótulo SPACEX de la fachada y la primera etapa del Falcon 1 del vuelo 5 en su cuna;
/// - TRANSPORTES DE ÉPOCA QUE SE USAN (P6, <see cref="MicroVehicle"/>): el Tesla Roadster de 2008 en el aparcamiento y una carretilla
///   elevadora con un palé dentro de la nave;
/// - muebles REALES de altura humana: mesas mixtas (sillas para personas y, para los insectos-humanos, apoyo de abdomen sobre una
///   tarima, C1 y E7) y TARIMAS DE PUESTO delante de las consolas del control de misión y de la consola de datos del vuelo 3 (E7);
/// - las NECESIDADES BÁSICAS dentro de la nave: cocina y comedor, aseos con lavamanos y una zona de descanso (dormir, aseo, lavarse,
///   comer, sentarse y cocinar), para que trabajen, coman, duerman y vayan al retrete;
/// - la historia de siempre (los mismos pasos y los mismos nombres: el reto de código de CodeShip busca «Consola de datos del vuelo 3»
///   y los pasos «Mira con el equipo los datos del vuelo 3» y «Revisa las tres cosas», CodeChallenges).
///
/// FUENTES (regla M39; cada medida dice si es real o supuesta):
/// - SpaceX se mudó en 2007 a la antigua planta de Northrop en Hawthorne, que llegó a pasar del millón de pies cuadrados (TechCrunch,
///   «SpaceX headquarters in Hawthorne, California»); la nave de 1 Rocket Road tiene unos 510 000 ft² ≈ 47 000 m² (Los Angeles Business
///   Journal). Aquí se levanta SOLO UN TRAMO de la nave (44 × 30 m, supuesto) por rendimiento (PC de pruebas con HD 620).
/// - Planta abierta de cubículos, Musk incluido; solo RR. HH. y algunos directivos con despacho (Satellite Today, 24-11-2008, «SpaceX
///   facilities open 24 hours to deliver on contracts»); el control de misión detrás de grandes cristales, a la vista desde la nave
///   (Wired, «The Rocket Factory: SpaceX Builds Them From Top to Bottom»); los pósteres de Marte junto al cubículo de Musk (Vance, A.,
///   «Elon Musk: Tesla, SpaceX, and the Quest for a Fantastic Future», 2015).
/// - Falcon 1 con Merlin 1C: 22,25 m de alto y 1,7 m de diámetro; Merlin 1C de 2,92 m (Wikipedia, «Falcon 1» y «SpaceX Merlin», con
///   sus fuentes de SpaceX); vuelo 4 el 28-9-2008 a las 16:15 PDT desde Omelek (Satellite Today, 29-9-2008; Space.com, 29-9-2008;
///   Spaceflight Now, «Falcon 1 flight 4»).
/// - Tesla Roadster (2008): 3,946 × 1,85 (con retrovisores) × 1,127 m (Edmunds; dimensions.com «Tesla Roadster 2008»). Musk recibió el
///   primer Roadster de serie (P1) en febrero de 2008 (Tesla Motors, nota de prensa del 1-2-2008).
/// - Carretilla elevadora: medidas de una contrapesada de 2,5 t de la época (≈ 2,3 × 1,07 × 2,1 m): SUPUESTO (no hay foto de cuál usaban).
/// - Altura libre de la nave (9 m), la cocina con mesas, la zona de descanso de los turnos de noche, la puerta de la fachada (1,8 m) y la
///   de los aseos (0,9 m, ensanchada): SUPUESTOS de juego, marcados como tales.
/// </summary>
public static partial class StorySceneBuilder
{
    // El tramo de la nave: su centro en el claro (la entrada y el portal, en z −14…−20, quedan libres delante de su puerta).
    // (piloto 3) Más el desplazamiento a un sitio libre de los muebles gigantes del piso (MuskSpot): todo lo de la nave, el aparcamiento
    // y la calle se corre con él.
    static Vector3 MuskShift;
    static Vector3 MuskFactoryAt => new Vector3(0f, 0f, 10f) + MuskShift;
    static readonly Vector2 MuskFactorySize = new Vector2(44f, 30f);

    // ══ ELON MUSK · la fábrica de SpaceX en Hawthorne (California), 28 de septiembre de 2008 · Control de misión ═══════════════════════
    //    El cuarto lanzamiento del Falcon 1 (desde Omelek, atolón de Kwajalein) se vivió en la fábrica: el equipo de Hawthorne lo
    //    siguió en las pantallas y Musk les habló al llegar a órbita.
    static int ElonMusk(Transform t)
    {
        Material white = M("Fuselaje_blanco", 0.94f, 0.94f, 0.95f), steel = M("Acero", 0.7f, 0.72f, 0.75f), dark = M("Pantalla", 0.08f, 0.1f, 0.14f);
        Material copper = M("Cobre_tobera", 0.72f, 0.42f, 0.25f), blue = M("Gráfica_azul", 0.25f, 0.55f, 0.85f), epoxy = M("Suelo_epoxi", 0.82f, 0.83f, 0.85f);
        Material navy = M("Azul_SpaceX", 0.07f, 0.11f, 0.2f), asphalt = M("Asfalto", 0.2f, 0.2f, 0.21f), paint = M("Pintura_blanca", 0.92f, 0.92f, 0.9f);
        Material mars = M("Poster_Marte", 0.75f, 0.35f, 0.2f), wood = M("Palé", 0.7f, 0.56f, 0.38f), yellow = M("Amarillo_carretilla", 0.92f, 0.72f, 0.1f);

        MuskShift = MuskSpot(t);   // (piloto 3) lejos de los muebles gigantes del piso ×100 (la foto «fabrica» salía tapada)
        // El estilo y el cuerpo de los muebles (Civilize los vuelve a poner después, para el pueblo).
        CivicBuilder.Year = 2008;
        CivicBuilder.Region = MicroStyles.RegionOf("Hawthorne").ToString();
        MicroFurniture.DefaultBody = MicroBody.OfSpecies("Ant");
        var both = new List<MicroBody.Fit> { MicroBody.OfSpecies("Ant"), MicroBody.Human() };   // mesas mixtas (regla 9)

        // ── 0 · LA NAVE: suelo de epoxi, muros de chapa blanca, cubierta, puerta de la fachada y vista de corte ──
        Transform fac = new GameObject("Fábrica de SpaceX (1 Rocket Road)").transform; fac.SetParent(t, false); fac.localPosition = MuskFactoryAt;
        // (El suelo NO se llama «Suelo»: CivicBuilder.Settle corre a un solar libre los edificios con «Suelo», y este va en el claro a propósito.)
        Prop(fac, "Suelo_de_epoxi", new Vector3(0f, 0.04f, 0f), new Vector3(MuskFactorySize.x, 0.08f, MuskFactorySize.y), epoxy, false);
        Color shell = new Color(0.9f, 0.91f, 0.92f), roof = new Color(0.62f, 0.64f, 0.66f), tile = new Color(0.86f, 0.86f, 0.84f);
        CivicBuilder.Enclose(fac, MuskFactorySize, 9f, shell, roof, CivicBuilder.Roof.Flat, 1.8f, 2.4f, "supuesto: doble puerta acristalada de la fachada de Rocket Road");
        // Los ASEOS, tras un tabique con su puerta real de 0,9 m ensanchada a 1,2 m (P4, «supuesto»).
        CivicBuilder.Partition(fac, "aseos_oeste", new Vector2(17f, -14.7f), new Vector2(17f, -4f), 2.6f, tile, 5.35f, 0.9f, "supuesto: puerta de aseo de 0,9 m");
        CivicBuilder.Partition(fac, "aseos_norte", new Vector2(17f, -4f), new Vector2(21.7f, -4f), 2.6f, tile, -1f, 0f, "");

        // P5 · EL HITO DE LA LLEGADA: el rótulo SPACEX sobre la puerta de la fachada (de cara a la entrada del microcosmos).
        Transform sign = new GameObject("Rótulo de SpaceX (fachada de 1 Rocket Road)").transform; sign.SetParent(fac, false);
        sign.localPosition = new Vector3(0f, 0f, -MuskFactorySize.y * 0.5f - 0.25f);
        Prop(sign, "Panel_del_rotulo", new Vector3(0f, 7.2f, 0f), new Vector3(12f, 1.8f, 0.15f), navy, false);
        Text3D(sign, "SPACEX", new Vector3(0f, 7.2f, -0.1f), 0.25f, Color.white);
        Hito(sign, "hito");

        // ── 1 · LA NAVE DE MONTAJE (noroeste): la primera etapa del Falcon 1 del vuelo 5 en su cuna, a su medida real ──
        //    (22,25 m el cohete entero con el Merlin 1C y 1,7 m de diámetro; la primera etapa, ~16 m: supuesto de proporción)
        Transform stage = new GameObject("Falcon 1 (vuelo 5) en su cuna").transform; stage.SetParent(fac, false); stage.localPosition = new Vector3(-11f, 0f, 9f);
        Prop(stage, "Primera etapa del Falcon 1 (16 m × 1,7 m)", new Vector3(0f, 1.25f, 0f), new Vector3(16f, 1.7f, 1.7f), white);
        Prop(stage, "Interetapa negra (fibra de carbono)", new Vector3(7.6f, 1.25f, 0f), new Vector3(0.9f, 1.72f, 1.72f), M("Negro_interetapa", 0.06f, 0.06f, 0.07f), false);
        Prop(stage, "Letras SpaceX de la etapa", new Vector3(1.5f, 1.25f, -0.86f), new Vector3(3.2f, 0.4f, 0.01f), navy, false);
        Prop(stage, "Bandera de EE. UU. de la etapa", new Vector3(-2.5f, 1.4f, -0.86f), new Vector3(0.9f, 0.5f, 0.01f), M("Bandera_azul", 0.15f, 0.2f, 0.45f), false);
        Prop(stage, "Tobera del Merlin 1C", new Vector3(-8.6f, 1.25f, 0f), new Vector3(1.2f, 1.1f, 1.1f), copper, false);
        for (int k = 0; k < 3; k++) Prop(stage, "Cuna", new Vector3(-6f + k * 6f, 0.2f, 0f), new Vector3(0.6f, 0.4f, 2.2f), steel);
        Prop(stage, "Motor Kestrel (segunda etapa) en su soporte", new Vector3(-3f, 0.6f, 3.2f), new Vector3(0.6f, 1.2f, 0.6f), copper);
        Hito(stage, "hito");
        Transform floorSpot = Spot(fac, "Nave de montaje", new Vector3(-11f, 0f, 5.6f), "La nave de montaje: el Falcon 1 que vendrá después del vuelo 4", 3.5f);

        // Las herramientas, el banco del Merlin 1C (el motor de 2,92 m a su lado) y el torno de control numérico (oeste, en medio).
        Piece(fac, "armario", new Vector3(-17f, 0f, 4.5f), 0f, "Herramientas y llaves dinamométricas", new Vector2(1.6f, 0.6f));
        Transform merlin = Piece(fac, "banco_taller", new Vector3(-16f, 0f, -1f), 0f, "Banco del Merlin 1C", new Vector2(2.4f, 1f));
        Prop(merlin, "Motor Merlin 1C (2,92 m)", new Vector3(0f, 1.15f, 0f), new Vector3(2.92f, 0.9f, 0.9f), steel, false);
        Prop(merlin, "Campana del Merlin 1C", new Vector3(-1.2f, 1.15f, 0f), new Vector3(0.5f, 1.0f, 1.0f), copper, false);
        Piece(fac, "torno", new Vector3(-9f, 0f, -1f), 0f, "Torno de control numérico", new Vector2(1.6f, 1f));

        // P6 · LA CARRETILLA ELEVADORA (supuesto) con un palé con la cúpula de un tanque de LOX: se sube con F y se conduce.
        Transform fork = Forklift(fac, new Vector3(-9f, 0f, 3.5f), 90f, yellow, dark, steel, wood, white);

        // ── 2 · LA ZONA DE DESCANSO Y LA COCINA (suroeste) ──
        Piece(fac, "cama", new Vector3(-18f, 0f, -5.5f), 0f, "Catre de los turnos de noche (supuesto)", null, station: false);
        Piece(fac, "cama", new Vector3(-14f, 0f, -5.5f), 0f, "Catre de los turnos de noche (supuesto)", null, station: false);
        Piece(fac, "cocina", new Vector3(-18f, 0f, -12f), 0f, "Cocina de la nave (supuesto)");
        Piece(fac, "mesa", new Vector3(-10f, 0f, -10.5f), 0f, "Comedor de la nave: mesa alta con apoyos de abdomen y sillas (supuesto)", null, null, both, station: false);   // (mixta, regla 9: no es puesto; la jornada come en la mesa alta)

        // ── 3 · EL CONTROL DE MISIÓN (noreste): tras grandes cristales, a la vista desde la nave (Wired) ──
        GlassWall.Box(fac, "sala_de_control_SpaceX", new Vector3(12f, 0f, 8f), new Vector2(16f, 12f), 0f, 2);
        Transform control = Spot(fac, "Control de misión", new Vector3(12f, 0f, 8f), "Control de misión: la señal de Omelek", 3.5f);
        for (int k = 0; k < 3; k++) Prop(control, "Pantalla grande", new Vector3(-4.5f + k * 4.5f, 1.9f, 5.4f), new Vector3(2.4f, 1.2f, 0.1f), dark, false);
        Prop(control, "Imagen de Omelek (Kwajalein)", new Vector3(0f, 1.9f, 5.33f), new Vector3(2.1f, 0.9f, 0.02f), blue, false);
        foreach (float x in new[] { 9f, 15f })
        {
            Transform c = Piece(fac, "consola", new Vector3(x, 0f, 6.5f), 0f, x < 12f ? "Consola de lanzamiento" : "Consola de telemetría", new Vector2(1.6f, 0.7f));
            WorkPlatform(fac, c, 0.76f, 1.6f, steel);
        }

        // ── 4 · LA PLANTA DE CUBÍCULOS (sureste): el cubículo de Musk entre los ingenieros, con sus pósteres de Marte ──
        // (2026-11-18, piloto 2) Los escritorios son PUESTOS de 2008 (tres LCD) con su tarima, no mesas de comer.
        Transform cube = Piece(fac, "consola", new Vector3(6f, 0f, -8f), 0f, "Cubículo de Elon Musk (con los pósteres de Marte)", new Vector2(1.6f, 0.7f));
        WorkPlatform(fac, cube, 0.76f, 1.6f, steel);
        Prop(fac, "Mampara del cubículo", new Vector3(6f, 0.7f, -11.2f), new Vector3(2.2f, 1.4f, 0.06f), M("Mampara_gris", 0.55f, 0.57f, 0.6f), false);
        for (int k = -1; k <= 1; k += 2) Prop(fac, "Póster de Marte", new Vector3(6f + k * 0.55f, 1.0f, -11.16f), new Vector3(0.6f, 0.8f, 0.01f), mars, false);

        // 2 · LA CONSOLA DEL VUELO 3 (2 de agosto de 2008): la gráfica del empuje que quedó tras el apagado. (Mismo nombre: el reto de
        //     código de CodeShip la busca por él.)
        Transform data = Spot(fac, "Consola de datos del vuelo 3", new Vector3(13f, 0f, -8f), "Los datos del vuelo 3 (2 de agosto de 2008)", 2.4f);
        Transform dc = Piece(data, "consola", Vector3.zero, 0f, "Consola de datos del vuelo 3 (telemetría)", new Vector2(1.6f, 0.7f));
        Prop(dc, "Curva del empuje residual", new Vector3(0f, 1.0f, 0.145f), new Vector3(0.38f, 0.03f, 0.01f), M("Curva_roja", 0.85f, 0.2f, 0.15f), false);
        WorkPlatform(fac, dc, 0.76f, 1.6f, steel);

        // 3 · LAS TRES COSAS QUE MIRAR antes del vuelo 4 (entre los cubículos y el cristal del control).
        var fixes = Items(fac, steel, new Vector3(0.7f, 0.9f, 0.7f),
            ("Motor Merlin 1C (empuje residual tras el apagado)", new Vector3(-16f, 0.45f, -3f)),
            ("Interetapa (la separación)", new Vector3(-4f, 0.45f, 6f)),
            ("Secuencia de vuelo (la espera antes de separar)", new Vector3(15.5f, 0.45f, -10.5f)));

        // ── 5 · LOS ASEOS (sureste, tras su tabique): retrete y lavamanos a la medida del insecto-humano ──
        Piece(fac, "inodoro", new Vector3(19.8f, 0f, -12.5f), 0f, "Aseo", null, station: false);
        Piece(fac, "lavabo", new Vector3(19.8f, 0f, -6.5f), 0f, "Lavamanos", null, station: false);

        // La nave es UN edificio (taller), con su puerta, su vista de corte y sus etiquetas (Civilize las rehace con lo demás).
        var site = fac.gameObject.AddComponent<AreaFunctionSite>(); site.building = "Taller"; site.title = "Fábrica de SpaceX (1 Rocket Road)"; site.region = CivicBuilder.Region;
        site.notHome = true;   // no es una vivienda: el reparto de hogares no le rehace los muebles (su hogar del turno de noche lo pone HawthorneTown)
        int extraFolk = MuskDetail(t, fac);   // (piloto 2) el detalle: grúa, estanterías, bancos, cubículos, marcas, fachada, muelle, aparcamiento y palmeras
        CivicBuilder.Tag(site);
        CutawayView.Ensure(fac);   // (otra vez, ya con los tabiques de los aseos: la vista de corte los recoge también)
        Label(fac, "Fábrica de SpaceX (1 Rocket Road, Hawthorne)", 10f);

        // ── 6 · EL APARCAMIENTO (al oeste, fuera): el Tesla Roadster de 2008 (se sube con F y se conduce) ──
        Transform lot = Spot(t, "Aparcamiento de SpaceX", new Vector3(-30f, 0f, 6f) + MuskShift, "Aparcamiento de Rocket Road", 2.5f);
        Prop(lot, "Asfalto", new Vector3(0f, 0.02f, 0f), new Vector3(12f, 0.04f, 26f), asphalt, false);
        for (int k = 0; k <= 9; k++) for (int s = -1; s <= 1; s += 2) Prop(lot, "Raya de aparcamiento", new Vector3(s * 3.2f, 0.045f, -12f + k * 2.7f), new Vector3(5.2f, 0.01f, 0.12f), paint, false);
        ParkedCars(lot);
        Roadster(t, new Vector3(-33.2f, 0f, 6.15f) + MuskShift, 90f, M("Rojo_Roadster", 0.7f, 0.08f, 0.08f), dark);

        // ── La gente: insectos-humanos. Musk, un SALTAMONTES (el salto es un lanzamiento: guarda energía en las patas y la suelta de
        //    golpe), con la camiseta negra de su versión humana. (Who recibe la estatura de la persona: el cuerpo, a la mitad.)
        Color black = new Color(0.08f, 0.08f, 0.09f), polo = new Color(0.2f, 0.25f, 0.35f);
        Vector3 F(float x, float z) => MuskFactoryAt + new Vector3(x, 0f, z);
        GameObject musk = Who(t, "Elon Musk", F(-4f, 5f), false, 1.88f, "Grasshopper"); CubeInsect.WearShirt(musk, black);
        GameObject prop = Who(t, "Ingeniera de propulsión (Medea)", F(13f, -5.5f), true); CubeInsect.WearShirt(prop, polo);
        GameObject launch = Who(t, "Director de lanzamiento (Héspero)", F(12f, 3.8f), false); CubeInsect.WearShirt(launch, polo);
        GameObject tech = Who(t, "Técnico de la nave (Atlas)", F(-12f, 5f), false); CubeInsect.WearShirt(tech, new Color(0.3f, 0.32f, 0.36f)); Routine(tech, floorSpot, data);
        GameObject welder = Who(t, "Soldadora de tanques (Ruth)", F(-4f, 12.5f), true); CubeInsect.WearShirt(welder, new Color(0.45f, 0.4f, 0.32f)); Routine(welder, floorSpot);
        int n = 5 + extraFolk;

        Mission(t, "Elon Musk y el Falcon 1: a la cuarta, órbita (2008)", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Agosto de 2008: habla con Elon Musk junto al cohete", musk.transform, "Elon Musk", "Tres lanzamientos, tres fallos. Pero tenemos piezas para un cuarto cohete y un equipo que no se ha rendido. Vamos a entender qué pasó y a intentarlo otra vez."),
                Hold("Mira con el equipo los datos del vuelo 3", data, 10f, "Ingeniera de propulsión", "Al apagarse, el motor nuevo siguió empujando un poco más de lo previsto, y la primera etapa alcanzó a la segunda después de separarse. El fallo no está en una pieza rota: está en el tiempo."),
                Visit("Revisa las tres cosas: el motor, la interetapa y la secuencia", fixes, "Ingeniera de propulsión", "Basta con esperar más entre el apagado y la separación. Un cambio pequeño en la secuencia."),
                Talk("Habla con el técnico de la nave", tech.transform, "Técnico", "Siete semanas entre el vuelo 3 y el 4. Hemos dormido poco, pero el cohete ya está en la isla."),
                Reach("28 de septiembre, 16:15: ve al control de misión", control, "Director de lanzamiento", "Omelek, en el atolón de Kwajalein, en medio del Pacífico. Desde aquí lo vemos todo por las pantallas."),
                Hold("La cuenta atrás y el ascenso: quédate mirando las pantallas", control, 12f, "Director de lanzamiento", "Despegue… separación de etapas limpia… encendido de la segunda etapa… apagado. ¡Estamos en órbita!"),
                Talk("Habla con la soldadora de tanques", welder.transform, "Soldadora", "Hay gente llorando por toda la nave. Yo soldé ese tanque."),
                Talk("Musk habla a todo el equipo", musk.transform, "Elon Musk", "Mucha gente pensaba que no podíamos hacerlo. Lo habéis hecho vosotros, todos. A la cuarta va la vencida."),
            },
            "La fábrica de SpaceX en 1 Rocket Road, Hawthorne (California), entre agosto y el 28 de septiembre de 2008, sobre el control de misión del santuario a escala de hormiga. Tras tres fallos, la empresa tiene un último Falcon 1. Ayuda al equipo a entender el fallo del vuelo 3 y vive con ellos el cuarto lanzamiento. E para hablar y revisar; para mirar, quédate quieto. En el aparcamiento está el Tesla Roadster y en la nave una carretilla: F para subir y bajar, W/A/S/D para conducir.",
            new[]
            {
                "Elon Musk (Pretoria, 1971) fundó SpaceX en 2002 para abaratar el acceso al espacio. Su primer cohete, el Falcon 1, era pequeño (22 m con el motor Merlin 1C y 1,7 m de diámetro) y usaba un motor propio, el Merlin. Se lanzaba desde la isla de Omelek, en el atolón de Kwajalein (Islas Marshall).",
                "En 2007 SpaceX se mudó a la antigua planta de Northrop en Hawthorne, junto al aeropuerto municipal: una nave enorme (unos 47 000 m²) con oficinas de planta abierta, donde también Musk trabajaba en un cubículo, y un control de misión tras grandes cristales, a la vista de la fábrica. Aquí se recrea un tramo de la nave.",
                "Los tres primeros vuelos fallaron: en 2006 un incendio por una fuga apagó el motor poco después del despegue; en 2007 la segunda etapa se apagó antes de tiempo; y el 2 de agosto de 2008 el empuje residual del motor tras el apagado hizo que la primera etapa chocara con la segunda después de separarse. La solución fue alargar la espera entre el apagado y la separación.",
                "El 28 de septiembre de 2008 (23:15 UTC; las 16:15 en California), siete semanas después, el cuarto Falcon 1 llegó a órbita con una carga de prueba de 165 kg (RatSat): fue el primer cohete de combustible líquido desarrollado con dinero privado en alcanzar la órbita terrestre. En la sede de Hawthorne, unos 500 empleados lo siguieron por las pantallas; Musk les dio las gracias y dijo que era «uno de los mejores días de mi vida».",
                "Supuestos de juego: la altura de la nave (9 m), la cocina con mesas, la zona de descanso de los turnos de noche, la carretilla elevadora y el color del Roadster; la puerta de los aseos se ha ensanchado a 1,2 m para que pasen los insectos-humanos. En diciembre de 2008 la NASA contrató a SpaceX para llevar carga a la Estación Espacial. Musk es también una figura pública muy discutida por otros asuntos; esta historia se limita a 2008 y al trabajo de su equipo.",
            },
            new[]
            {
                "Space.com (29-9-2008), «SpaceX Successfully Launches Falcon 1 Rocket Into Orbit».",
                "Spaceflight Now, «Falcon 1 flight 4» (spaceflightnow.com/falcon/004).",
                "Satellite Today (29-9-2008), «SpaceX Successful in Fourth Falcon 1 Mission»; (24-11-2008), «SpaceX facilities open 24 hours to deliver on contracts».",
                "Aviation Week (2008), «Falcon 1 Makes Orbit On Fourth Attempt».",
                "Koenigsmann, H. et al. (2009). «Falcon 1 Launch Vehicle: Demo Flights, Status, Manifest and Upgrade Path». SmallSat Conference (digitalcommons.usu.edu/smallsat/2009).",
                "TechCrunch, «SpaceX headquarters in Hawthorne, California» (la mudanza de 2007 a la planta de Northrop); Los Angeles Business Journal (1 Rocket Road, ~510 000 ft²).",
                "Wired, «The Rocket Factory: SpaceX Builds Them From Top to Bottom» (el control de misión tras cristales, la planta abierta).",
                "Vance, A. (2015). Elon Musk: Tesla, SpaceX, and the Quest for a Fantastic Future. Ecco.",
                "Edmunds y dimensions.com, «Tesla Roadster 2008» (3,946 × 1,85 × 1,127 m).",
            },
            "Elon Musk", "Que falle no es lo grave. Lo grave sería dejar de intentarlo.");
        return n;
    }

    // Un mueble del registro (MicroFurniture) en la nave, a su estilo de 2008 y con su puesto (AreaFunctionStation) si lo es.
    static Transform Piece(Transform parent, string key, Vector3 at, float yaw, string label, Vector2? size = null, MicroBody.Fit? body = null, IList<MicroBody.Fit> users = null, bool station = true)
    {
        Transform p = MicroFurniture.Build(parent, key, at, yaw, label, CivicBuilder.Year, size, CivicBuilder.Region, body, users);
        if (station && !CivicBuilder.NotStation(key) && p.GetComponent<AreaFunctionStation>() == null) p.gameObject.AddComponent<AreaFunctionStation>().station = key;
        Label(p, label, 1.6f);
        return p;
    }

    // Marca la raíz como mueble de su clase (P5 «hito», P6 «vehiculo»): la cuentan las reglas (MicroBuildRules.Advise) y el paso.
    static void Hito(Transform root, string kind)
    {
        var mp = root.GetComponent<MicroPiece>() ?? root.gameObject.AddComponent<MicroPiece>();
        mp.kind = kind; mp.style = MicroStyles.For(CivicBuilder.Year, CivicBuilder.Region).id;
    }

    /// <summary>(E7) LA TARIMA DE PUESTO delante de un mueble real de altura humana (<paramref name="desk"/> m de tablero): sube al
    /// insecto-humano hasta que el tablero le queda a las manos (MicroBody.StandingTable). No es un mueble (se pisa): no cuenta para el
    /// paso; la jornada lo pone encima (su sitio de trabajo, a MicroBuildRules.WorkSpot del frente, cae sobre ella).</summary>
    static Transform WorkPlatform(Transform parent, Transform piece, float desk, float width, Material m)
    {
        MicroBody.Fit f = MicroFurniture.DefaultBody ?? MicroBody.OfSpecies("Ant");
        float h = Mathf.Max(0.05f, desk - MicroBody.StandingTable(f)), depth = f.length + 0.3f;
        Vector3 front = parent.InverseTransformPoint(piece.TransformPoint(new Vector3(0f, 0f, -0.35f - depth * 0.5f)));
        Transform p = AreaMicrocosmos.Box(parent, "Tarima_de_puesto", new Vector3(front.x, h * 0.5f, front.z), new Vector3(width, h, depth), Quaternion.Euler(0f, piece.eulerAngles.y - parent.eulerAngles.y, 0f), m, true);
        return p;
    }

    static Transform Text3D(Transform parent, string text, Vector3 local, float size, Color c)
    {
        var go = new GameObject("Letras_" + text); go.transform.SetParent(parent, false); go.transform.localPosition = local;
        go.transform.localRotation = Quaternion.identity;
        TextMesh tm = go.AddComponent<TextMesh>(); tm.text = text; tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center;
        tm.characterSize = size; tm.fontSize = 60; tm.color = c;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (tm.font != null) go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        return go.transform;
    }

    // P6 · EL TESLA ROADSTER DE 2008 (3,946 × 1,85 × 1,127 m; Edmunds, dimensions.com), con su asiento y que se conduce (MicroVehicle).
    static Transform Roadster(Transform t, Vector3 at, float yaw, Material body, Material glass)
    {
        Transform car = new GameObject("Tesla Roadster (2008)").transform; car.SetParent(t, false); car.localPosition = at; car.localRotation = Quaternion.Euler(0f, yaw, 0f);
        Material tyre = M("Neumatico", 0.08f, 0.08f, 0.08f);
        Prop(car, "Carroceria", new Vector3(0f, 0.45f, 0f), new Vector3(1.75f, 0.5f, 3.946f), body);
        Prop(car, "Capo_delantero", new Vector3(0f, 0.72f, 1.2f), new Vector3(1.6f, 0.08f, 1.2f), body, false);
        Prop(car, "Parabrisas", new Vector3(0f, 0.92f, 0.25f), new Vector3(1.4f, 0.35f, 0.08f), glass, false);
        Prop(car, "Arco_de_seguridad", new Vector3(0f, 1.0f, -0.55f), new Vector3(1.4f, 0.25f, 0.4f), body, false);   // (1,127 m de alto con la capota)
        Prop(car, "Retrovisores", new Vector3(0f, 0.85f, 0.4f), new Vector3(1.85f, 0.08f, 0.1f), glass, false);
        foreach (var (x, z) in new[] { (-0.78f, 1.2f), (0.78f, 1.2f), (-0.78f, -1.15f), (0.78f, -1.15f) })
            Prop(car, "Rueda", new Vector3(x, 0.3f, z), new Vector3(0.25f, 0.6f, 0.6f), tyre, false);
        Transform seat = new GameObject("Asiento_del_conductor").transform; seat.SetParent(car, false); seat.localPosition = new Vector3(-0.35f, 0.45f, -0.2f);
        var v = car.gameObject.AddComponent<MicroVehicle>();
        v.title = "Tesla Roadster (2008)"; v.maxSpeed = 6f; v.accel = 3f; v.turn = 60f; v.seat = seat;
        v.source = "Edmunds y dimensions.com, «Tesla Roadster 2008»; Tesla Motors (1-2-2008): el P1, el primer Roadster de serie, para Musk.";
        Hito(car, "vehiculo");
        Label(car, "Tesla Roadster (2008): F para subir", 1.6f);
        return car;
    }

    // P6 · UNA CARRETILLA ELEVADORA contrapesada de la época (≈ 2,3 × 1,07 × 2,1 m: SUPUESTO), con un palé y la cúpula de un tanque.
    static Transform Forklift(Transform parent, Vector3 at, float yaw, Material paint, Material dark, Material steel, Material wood, Material white)
    {
        Transform fl = new GameObject("Carretilla elevadora (2008)").transform; fl.SetParent(parent, false); fl.localPosition = at; fl.localRotation = Quaternion.Euler(0f, yaw, 0f);
        Prop(fl, "Chasis", new Vector3(0f, 0.55f, -0.25f), new Vector3(1.07f, 0.8f, 1.7f), paint);
        Prop(fl, "Contrapeso", new Vector3(0f, 0.6f, -1.0f), new Vector3(1.0f, 0.7f, 0.3f), dark, false);
        Prop(fl, "Asiento", new Vector3(0f, 1.05f, -0.4f), new Vector3(0.45f, 0.15f, 0.45f), dark, false);
        foreach (var (x, z) in new[] { (-0.5f, 0.45f), (0.5f, 0.45f), (-0.5f, -0.95f), (0.5f, -0.95f) })
            Prop(fl, "Poste_del_techo_protector", new Vector3(x, 1.5f, z), new Vector3(0.06f, 1.1f, 0.06f), dark, false);
        Prop(fl, "Techo_protector", new Vector3(0f, 2.06f, -0.25f), new Vector3(1.07f, 0.05f, 1.5f), dark, false);
        Prop(fl, "Mastil", new Vector3(0f, 1.05f, 0.72f), new Vector3(0.8f, 2.1f, 0.12f), steel, false);
        Transform cargo = new GameObject("Horquillas").transform; cargo.SetParent(fl, false); cargo.localPosition = new Vector3(0f, 0.12f, 1.35f);
        for (int s = -1; s <= 1; s += 2) Prop(fl, "Horquilla", new Vector3(s * 0.3f, 0.06f, 1.3f), new Vector3(0.12f, 0.05f, 1.07f), steel, false);
        // La carga: un palé con la cúpula de un tanque de oxígeno líquido (LOX) del Falcon 1, en las horquillas.
        Transform pallet = new GameObject("Palé con la cúpula de un tanque de LOX").transform; pallet.SetParent(cargo, false);
        Prop(pallet, "Palé", new Vector3(0f, 0.07f, 0f), new Vector3(1.0f, 0.14f, 1.0f), wood, false);
        Prop(pallet, "Cúpula del tanque de LOX", new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.6f, 0.9f), white, false);
        Transform seat = new GameObject("Asiento_del_conductor").transform; seat.SetParent(fl, false); seat.localPosition = new Vector3(0f, 0.9f, -0.4f);
        var v = fl.gameObject.AddComponent<MicroVehicle>();
        v.title = "Carretilla elevadora"; v.maxSpeed = 3f; v.accel = 1.5f; v.turn = 80f; v.seat = seat; v.cargoPoint = cargo;
        v.source = "supuesto: carretilla contrapesada de 2,5 t de la época (≈ 2,3 × 1,07 × 2,1 m)";
        Hito(fl, "vehiculo");
        Label(fl, "Carretilla elevadora: F para subir", 2.4f);
        return fl;
    }
}
