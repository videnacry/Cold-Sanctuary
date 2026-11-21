using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MICROCOSMOS_ELONMUSKOMELEK: LA ISLA DE OMELEK (atolón de Kwajalein, Islas Marshall), la mañana del 29 de septiembre de 2008 (hora local;
/// el 28 en California): el cuarto Falcon 1 ya está vertical en su plataforma y el equipo de lanzamiento hace las últimas revisiones,
/// cierra el hangar y EVACÚA la isla en lancha antes del lanzamiento de las 11:15 (23:15 UTC). Es el clímax de la historia de Hawthorne
/// (docs/microcosmos/microcosmos-musk-historia.md, escena 3) y el segundo escenario hecho con las reglas del plan de realismo: insectos-humanos a la
/// mitad, lugar real a 1:1, edificios con muros, puerta y vista de corte, hito y transporte de época.
///
/// FUENTES: Spaceflight Now, «Falcon 1 flight 4» y la crónica de 2009 («austere launch pad with propellant tanks, a processing hangar and
/// workspace for engineers»); Spaceflight Now 2005 (Kwajalein Range Services vertió el hormigón de la plataforma y reformó unos pocos
/// edificios pequeños; el hangar «parecido al de Vandenberg», de tipo carpa); Federal Register E8-1068 (23-1-2008: mejoras propuestas,
/// almacenamiento y carga de propelentes); Wikipedia, «Omelek Island» (siete acres, ~2,8 ha; el helipuerto, los muelles, la vieja plataforma
/// de la punta norte y los dos bloques del lado este son anteriores a SpaceX); Wikipedia, «RatSat» (el equipo siguió el lanzamiento desde
/// la sala de control de Kwajalein: la isla se evacúa); Koenigsmann et al., SmallSat 2009 (el vehículo: 22,25 m × 1,7 m, Merlin 1C y Kestrel).
/// SUPUESTOS de juego: la forma exacta de la isla (un huso N–S de ~320 × 90 m), la posición de cada pieza, las medidas del hangar (36 × 16 m)
/// y de los contenedores de trabajo, la lancha (8 m), la granja de propelentes y el reparto de cocoteros y naupaka.
/// Para los retos de CodeShip (los hace el trabajador de la interfaz), los SITIOS se llaman: «Consola de la plataforma (Omelek)», «Plataforma
/// de lanzamiento», «Tanque de oxígeno líquido (LOX)», «Tanque de queroseno RP-1», «Botellas de helio», «Antena de telemetría» y
/// «Muelle de Omelek»; los pasos, como dice la tabla de docs/microcosmos/microcosmos-musk-historia.md.
/// </summary>
public static partial class StorySceneBuilder
{
    static Vector3 OmelekShift;

    static int Omelek(Transform t)
    {
        CivicBuilder.Year = 2008;
        CivicBuilder.Region = MicroStyles.RegionOf("Omelek").ToString();
        MicroFurniture.DefaultBody = MicroBody.OfSpecies("Ant");
        OmelekShift = FreeSpot(t, Rect.MinMaxRect(-50f, -35f, 50f, 295f), "Omelek");
        Vector3 P(float x, float z) => OmelekShift + new Vector3(x, 0f, z);

        Material sand = M("Arena_de_coral", 0.9f, 0.86f, 0.74f), beach = M("Playa_blanca", 0.96f, 0.93f, 0.84f), lagoon = M("Laguna_turquesa", 0.25f, 0.65f, 0.7f);
        Material ocean = M("Oceano", 0.1f, 0.32f, 0.5f), concrete = M("Hormigon_isla", 0.72f, 0.71f, 0.68f), steel = M("Acero", 0.7f, 0.72f, 0.75f);
        Material white = M("Fuselaje_blanco", 0.94f, 0.94f, 0.95f), black = M("Negro_interetapa", 0.06f, 0.06f, 0.07f), dark = M("Pantalla", 0.08f, 0.1f, 0.14f);
        Material yellow = M("Amarillo_seguridad", 0.95f, 0.78f, 0.1f), rust = M("Oxido", 0.5f, 0.3f, 0.2f), wood = M("Madera_muelle", 0.55f, 0.42f, 0.3f);
        Material navy = M("Azul_SpaceX", 0.07f, 0.11f, 0.2f), shrub = M("Naupaka", 0.3f, 0.5f, 0.28f), alu = M("Aluminio_lancha", 0.75f, 0.77f, 0.8f);

        // ── EL MAR Y LA ISLA: el océano al este, la laguna al oeste; la isla de arena de coral con su playa (un huso N–S, supuesto) ──
        Prop(t, "Oceano", P(250f, 130f) + new Vector3(0f, 0.01f, 0f), new Vector3(400f, 0.02f, 900f), ocean, false);
        Prop(t, "Laguna", P(-250f, 130f) + new Vector3(0f, 0.012f, 0f), new Vector3(400f, 0.02f, 900f), lagoon, false);
        Prop(t, "Arrecife_del_canal", P(0f, -130f) + new Vector3(0f, 0.011f, 0f), new Vector3(100f, 0.02f, 160f), lagoon, false);
        Prop(t, "Arrecife_del_canal", P(0f, 390f) + new Vector3(0f, 0.011f, 0f), new Vector3(100f, 0.02f, 160f), lagoon, false);
        Prop(t, "Playa", P(0f, 130f) + new Vector3(0f, 0.04f, 0f), new Vector3(96f, 0.06f, 328f), beach, false);
        Prop(t, "Arena_de_coral", P(0f, 130f) + new Vector3(0f, 0.07f, 0f), new Vector3(84f, 0.08f, 312f), sand, false);
        Prop(t, "Camino_de_hormigon", P(0f, 130f) + new Vector3(0f, 0.115f, 0f), new Vector3(5f, 0.02f, 290f), concrete, false);

        // ── 1 · LOS CONTENEDORES DE TRABAJO Y DESCANSO del equipo (supuesto: dos contenedores unidos, 24 × 14 m), con su consola de la
        //    plataforma, la cocina, los catres, el aseo y el lavamanos; muros, techo, puerta de contenedor (0,9 m ensanchada) y vista de corte ──
        Transform quarters = new GameObject("Contenedores de trabajo de SpaceX (Omelek)").transform; quarters.SetParent(t, false); quarters.localPosition = P(-15f, 25f);
        Prop(quarters, "Suelo_del_contenedor", new Vector3(0f, 0.15f, 0f), new Vector3(24f, 0.1f, 14f), M("Suelo_vinilico", 0.62f, 0.64f, 0.66f), false);
        CivicBuilder.Enclose(quarters, new Vector2(24f, 14f), 3f, new Color(0.85f, 0.86f, 0.84f), new Color(0.6f, 0.62f, 0.64f), CivicBuilder.Roof.Flat, 0.9f, 2.0f, "supuesto: puerta de contenedor de oficina");
        Transform console = Spot(quarters, "Consola de la plataforma (Omelek)", new Vector3(-7f, 0f, 4.5f), "La consola de la plataforma: telemetría con Hawthorne", 2.4f);
        Transform pc = Piece(console, "consola", Vector3.zero, 0f, "Consola de la plataforma (sistemas de tierra)", new Vector2(1.6f, 0.7f));
        WorkPlatform(quarters, pc, 0.76f, 1.6f, steel);
        Transform pc2 = Piece(quarters, "consola", new Vector3(-2.5f, 0f, 4.5f), 0f, "Consola de propelentes (carga de LOX y RP-1)", new Vector2(1.6f, 0.7f));
        WorkPlatform(quarters, pc2, 0.76f, 1.6f, steel);
        var both = new List<MicroBody.Fit> { MicroBody.OfSpecies("Ant"), MicroBody.Human() };
        Piece(quarters, "mesa", new Vector3(6.5f, 0f, 3.5f), 0f, "Mesa del equipo: de pie con apoyos de abdomen y sillas", null, null, both);
        Piece(quarters, "cocina", new Vector3(-8f, 0f, -4.5f), 0f, "Cocina del contenedor (supuesto)");
        foreach (float x in new[] { -3f, 0.9f, 4.8f, 8.7f }) Piece(quarters, "cama", new Vector3(x, 0f, -5f), 0f, "Catre del equipo de lanzamiento (supuesto)", null, station: false);
        Piece(quarters, "inodoro", new Vector3(10.5f, 0f, 1f), 0f, "Aseo químico (supuesto)", null, station: false);
        Piece(quarters, "lavabo", new Vector3(-5f, 0f, 0f), 0f, "Lavamanos", null, station: false);
        var qsite = quarters.gameObject.AddComponent<AreaFunctionSite>(); qsite.building = "Centro de tecnología"; qsite.title = "Contenedores de trabajo de SpaceX (Omelek)"; qsite.region = CivicBuilder.Region;
        CivicBuilder.Tag(qsite);
        CutawayView.Ensure(quarters);
        Transform fruit = CivicBuilder.FruitTree(quarters, quarters.name.GetHashCode());   // (regla 11: es donde viven; el frutal, fuera)
        fruit.localPosition = new Vector3(-16f, 0f, -2f);

        // ── 2 · EL HANGAR DE PROCESADO (de tipo carpa, «parecido al de Vandenberg»; 36 × 16 m, supuesto): vacío del cohete, que ya está en la
        //    plataforma; los raíles del transportador, los bancos de herramientas y la mesa del descanso. Puerta grande del transportador (8 m) ──
        Transform hangar = new GameObject("Hangar de procesado del Falcon 1 (Omelek)").transform; hangar.SetParent(t, false); hangar.localPosition = P(-5f, 140f);
        Prop(hangar, "Losa_del_hangar", new Vector3(0f, 0.15f, 0f), new Vector3(36f, 0.1f, 16f), concrete, false);
        CivicBuilder.Enclose(hangar, new Vector2(36f, 16f), 6f, new Color(0.92f, 0.92f, 0.9f), new Color(0.88f, 0.88f, 0.86f), CivicBuilder.Roof.Gable, 8f, 5.5f, "supuesto: la puerta del transportador");
        foreach (float z in new[] { -0.8f, 0.8f }) Prop(hangar, "Rail_del_transportador", new Vector3(-4f, 0.22f, z), new Vector3(26f, 0.08f, 0.15f), steel, false);
        Prop(hangar, "Carro_del_transportador (vacío)", new Vector3(-12f, 0.6f, 0f), new Vector3(4f, 0.7f, 2.4f), yellow, true);
        Piece(hangar, "banco_taller", new Vector3(-12f, 0f, 5.5f), 0f, "Banco de herramientas del hangar", new Vector2(2f, 0.75f));
        Piece(hangar, "banco_taller", new Vector3(-5f, 0f, 5.5f), 0f, "Banco de la segunda etapa (Kestrel)", new Vector2(2f, 0.75f));
        Piece(hangar, "armario", new Vector3(2f, 0f, 6f), 0f, "Repuestos y llaves", new Vector2(1.6f, 0.6f));
        Piece(hangar, "mesa", new Vector3(12f, 0f, 4.5f), 0f, "Mesa del descanso del hangar", null, null, both);
        for (int k = 0; k < 2; k++) Prop(hangar, "Contenedor_de_repuestos", new Vector3(-16.2f, 1.2f, -5.5f + k * 2.6f), new Vector3(2.4f, 2.4f, 2.4f), M("Contenedor_azul", 0.2f, 0.35f, 0.6f), true);
        var hsite = hangar.gameObject.AddComponent<AreaFunctionSite>(); hsite.building = "Taller"; hsite.title = "Hangar de procesado del Falcon 1 (Omelek)"; hsite.region = CivicBuilder.Region; hsite.notHome = true;
        CivicBuilder.Tag(hsite);
        CutawayView.Ensure(hangar);
        Transform hangarSpot = Spot(t, "Hangar de procesado", P(-5f, 129f), "El hangar de procesado (ya vacío)", 2.5f);

        // ── 3 · LA GRANJA DE PROPELENTES (supuesto de forma: E8-1068 habla de almacenamiento y carga de propelentes): el oxígeno líquido en un
        //    termo horizontal, el queroseno RP-1 y las botellas de helio, con su murete de contención ──
        Transform farm = new GameObject("Granja de propelentes").transform; farm.SetParent(t, false); farm.localPosition = P(25f, 165f);
        Prop(farm, "Losa", new Vector3(0f, 0.15f, 0f), new Vector3(20f, 0.1f, 22f), concrete, false);
        foreach (var (x, z, w, d) in new[] { (0f, -11f, 20f, 0.3f), (0f, 11f, 20f, 0.3f), (-10f, 0f, 0.3f, 22f), (10f, 0f, 0.3f, 22f) }) Prop(farm, "Murete_de_contencion", new Vector3(x, 0.5f, z), new Vector3(w, 0.8f, d), concrete, true);
        Prop(farm, "Termo_de_LOX", new Vector3(0f, 2f, -5f), new Vector3(12f, 3f, 3f), white, true);
        for (int k = -1; k <= 1; k += 2) Prop(farm, "Patas_del_termo", new Vector3(k * 4f, 0.5f, -5f), new Vector3(0.4f, 1f, 2.4f), steel, false);
        Prop(farm, "Tanque_de_RP-1", new Vector3(0f, 1.6f, 4f), new Vector3(8f, 2.5f, 2.5f), M("Tanque_gris", 0.55f, 0.57f, 0.6f), true);
        for (int k = 0; k < 6; k++) Prop(farm, "Botella_de_helio", new Vector3(6.5f, 0.9f, 1f + k * 0.6f), new Vector3(0.35f, 1.6f, 0.35f), M("Verde_helio", 0.3f, 0.45f, 0.35f), false);
        Prop(farm, "Tuberia_a_la_plataforma", new Vector3(-4f, 0.4f, 25f), new Vector3(0.25f, 0.25f, 30f), steel, false);
        var fuel = new List<Transform>
        {
            Spot(farm, "Tanque de oxígeno líquido (LOX)", new Vector3(0f, 0f, -8f), "Oxígeno líquido (−183 °C)", 3.6f),
            Spot(farm, "Tanque de queroseno RP-1", new Vector3(-6f, 0f, 4f), "Queroseno RP-1", 3f),
            Spot(farm, "Botellas de helio", new Vector3(7.5f, 0f, -1f), "Helio a presión", 2.2f),
        };

        // ── 4 · LA PLATAFORMA DE LANZAMIENTO con el Falcon 1 VERTICAL (22,25 m × 1,7 m): el HITO de la isla (P5) ──
        Transform pad = new GameObject("Plataforma de lanzamiento de Omelek (Falcon 1 vertical)").transform; pad.SetParent(t, false); pad.localPosition = P(0f, 235f);
        Prop(pad, "Losa_de_la_plataforma", new Vector3(0f, 0.25f, 0f), new Vector3(30f, 0.3f, 30f), concrete, true);
        Prop(pad, "Foso_de_llamas", new Vector3(0f, 0.38f, 6f), new Vector3(4f, 0.04f, 10f), M("Foso_oscuro", 0.2f, 0.2f, 0.2f), false);
        Prop(pad, "Montura_de_lanzamiento", new Vector3(0f, 1.2f, 0f), new Vector3(3.4f, 1.6f, 3.4f), steel, true);
        float baseY = 2.0f;
        Prop(pad, "Primera_etapa (Merlin 1C)", new Vector3(0f, baseY + 7.5f, 0f), new Vector3(1.7f, 15f, 1.7f), white, false);
        Prop(pad, "Tobera_del_Merlin", new Vector3(0f, baseY - 0.3f, 0f), new Vector3(1.1f, 0.6f, 1.1f), M("Cobre_tobera", 0.72f, 0.42f, 0.25f), false);
        Prop(pad, "Interetapa_negra", new Vector3(0f, baseY + 15.6f, 0f), new Vector3(1.72f, 1.2f, 1.72f), black, false);
        Prop(pad, "Segunda_etapa (Kestrel)", new Vector3(0f, baseY + 18f, 0f), new Vector3(1.6f, 3.6f, 1.6f), white, false);
        Prop(pad, "Cofia", new Vector3(0f, baseY + 20.9f, 0f), new Vector3(1.5f, 2.2f, 1.5f), white, false);
        Prop(pad, "Punta_de_la_cofia", new Vector3(0f, baseY + 22.1f, 0f), new Vector3(0.8f, 0.3f, 0.8f), white, false);
        Prop(pad, "Letras_SpaceX", new Vector3(0f, baseY + 9f, -0.86f), new Vector3(0.6f, 3.5f, 0.01f), navy, false);
        Prop(pad, "Erector (abatido para el lanzamiento)", new Vector3(-3.4f, 1.6f, -6f), new Vector3(1.0f, 1.0f, 18f), yellow, true);
        Prop(pad, "Pivote_del_erector", new Vector3(-3.4f, 1.0f, 2.6f), new Vector3(1.4f, 1.4f, 1.4f), steel, true);
        Prop(pad, "Torre_umbilical", new Vector3(3.2f, 4.5f, 0f), new Vector3(0.6f, 9f, 0.6f), steel, true);
        Prop(pad, "Brazo_umbilical", new Vector3(2.0f, 8.5f, 0f), new Vector3(2.2f, 0.2f, 0.2f), steel, false);
        foreach (var (x, z) in new[] { (-12f, -12f), (12f, -12f), (-12f, 12f), (12f, 12f) }) Prop(pad, "Mastil_de_pararrayos", new Vector3(x, 12f, z), new Vector3(0.3f, 24f, 0.3f), steel, true);
        Hito(pad, "hito");
        Transform padSpot = Spot(t, "Plataforma de lanzamiento", P(0f, 216f), "El Falcon 1 del vuelo 4, vertical en la plataforma", 3f);

        // ── 5 · LO QUE YA ESTABA EN LA ISLA: el helipuerto, la vieja plataforma de la punta norte y los dos bloques del lado este ──
        Prop(t, "Helipuerto", P(18f, 20f) + new Vector3(0f, 0.14f, 0f), new Vector3(20f, 0.06f, 20f), concrete, false);
        Prop(t, "H_del_helipuerto", P(18f, 20f) + new Vector3(0f, 0.18f, 0f), new Vector3(1.2f, 0.01f, 6f), yellow, false);
        Prop(t, "Vieja_plataforma_norte", P(0f, 282f) + new Vector3(0f, 0.2f, 0f), new Vector3(14f, 0.3f, 10f), M("Hormigon_viejo", 0.6f, 0.58f, 0.55f), true);
        Prop(t, "Soporte_oxidado", P(0f, 282f) + new Vector3(0f, 1.5f, 0f), new Vector3(1.2f, 2.6f, 1.2f), rust, true);
        foreach (float z in new[] { 100f, 116f }) Prop(t, "Bloque_del_este (anterior a SpaceX)", P(34f, z) + new Vector3(0f, 3f, 0f), new Vector3(8f, 6f, 10f), M("Bloque_hormigon", 0.68f, 0.66f, 0.6f), true);
        // La antena de telemetría y el generador.
        Prop(t, "Pedestal_de_la_antena", P(30f, 62f) + new Vector3(0f, 1.5f, 0f), new Vector3(1.2f, 3f, 1.2f), steel, true);
        Prop(t, "Plato_de_la_antena", P(30f, 62f) + new Vector3(0f, 3.8f, 0f), new Vector3(3.6f, 3.6f, 0.4f), white, false);
        Transform ant = Spot(t, "Antena de telemetría", P(30f, 58f), "La antena de telemetría", 4.5f);
        Prop(t, "Contenedor_del_generador", P(-32f, 62f) + new Vector3(0f, 1.3f, 0f), new Vector3(6f, 2.6f, 2.4f), M("Contenedor_verde", 0.25f, 0.4f, 0.3f), true);

        // ── 6 · EL MUELLE en la laguna y la LANCHA del equipo (P6: se sube con F y se conduce sobre el agua; 8 m, supuesto) ──
        Prop(t, "Muelle", P(-62f, 40f) + new Vector3(0f, 0.5f, 0f), new Vector3(34f, 0.25f, 4f), wood, true);
        for (int k = 0; k < 8; k++) foreach (float s in new[] { -1.8f, 1.8f }) Prop(t, "Pilote", P(-48f - k * 4.2f, 40f + s) + new Vector3(0f, 0.3f, 0f), new Vector3(0.3f, 0.9f, 0.3f), wood, false);
        Transform pier = Spot(t, "Muelle de Omelek", P(-60f, 40f), "El muelle: la lancha a Kwajalein", 2.5f);
        Transform boat = new GameObject("Lancha del equipo (a Kwajalein)").transform; boat.SetParent(t, false); boat.localPosition = P(-70f, 46f); boat.localRotation = Quaternion.Euler(0f, 270f, 0f);
        Prop(boat, "Casco", new Vector3(0f, 0.45f, 0f), new Vector3(2.6f, 0.9f, 8f), alu, false);
        Prop(boat, "Cabina", new Vector3(0f, 1.4f, -0.8f), new Vector3(2.2f, 1.1f, 2.6f), white, false);
        Prop(boat, "Parabrisas", new Vector3(0f, 1.6f, 0.52f), new Vector3(2f, 0.6f, 0.02f), dark, false);
        Prop(boat, "Motor_fueraborda", new Vector3(0f, 0.7f, -4.2f), new Vector3(0.6f, 1.0f, 0.5f), M("Negro_mate", 0.07f, 0.07f, 0.08f), false);
        Transform seat = new GameObject("Asiento_del_patron").transform; seat.SetParent(boat, false); seat.localPosition = new Vector3(0f, 0.9f, -0.6f);
        var v = boat.gameObject.AddComponent<MicroVehicle>(); v.title = "Lancha del equipo"; v.maxSpeed = 7f; v.accel = 2f; v.turn = 35f; v.seat = seat;
        v.source = "supuesto: lancha de aluminio de ~8 m (el equipo cruzaba la laguna entre Omelek y Kwajalein)";
        Hito(boat, "vehiculo");
        Label(boat, "Lancha del equipo: F para subir", 2.6f);

        // ── 7 · LA VEGETACIÓN DE LA ISLA: cocoteros en la orilla y naupaka (Scaevola taccada) en el borde de la playa (supuesto de reparto) ──
        for (int k = 0; k < 18; k++)
        {
            float z = -15f + k * 17f; if (z > 205f && z < 265f) continue;   // la plataforma, despejada
            Palm(t, P(k % 2 == 0 ? -38f : 38f, z), "Cocotero (Cocos nucifera)");
            Prop(t, "Naupaka", P(k % 2 == 0 ? 39f : -39f, z + 6f) + new Vector3(0f, 0.6f, 0f), new Vector3(3f, 1.2f, 2.4f), shrub, false);
        }

        // ── LA GENTE: el equipo de lanzamiento (Musk no estaba en la isla: lo vivió en Hawthorne). Insectos-humanos a la mitad ──
        Color polo = new Color(0.2f, 0.25f, 0.35f);
        GameObject lead = Who(t, "Jefa de plataforma (Calisto)", P(-5f, 127f), true); CubeInsect.WearShirt(lead, polo); Routine(lead, hangarSpot, padSpot);
        GameObject prop = Who(t, "Ingeniero de propulsión en la isla (Atlas)", P(18f, 158f), false); CubeInsect.WearShirt(prop, new Color(0.3f, 0.32f, 0.36f)); Routine(prop, fuel[0], fuel[1]);
        GameObject tele = Who(t, "Técnico de telemetría (Hiperión)", P(-22f, 21f), false); CubeInsect.WearShirt(tele, polo); Routine(tele, console, ant);
        GameObject pilot = Who(t, "Patrón de la lancha (Deucalión)", P(-50f, 42f), false); CubeInsect.WearShirt(pilot, new Color(0.85f, 0.6f, 0.15f));
        int n = 4;

        Mission(t, "Omelek, 29 de septiembre de 2008: el Falcon 1 se queda solo en la isla", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("6:30: habla con la jefa de plataforma junto al hangar", lead.transform, "Jefa de plataforma", "Hoy no hay ensayo. El cohete está vertical desde anoche; revisamos todo una vez más y a las nueve no puede quedar nadie en la isla."),
                Reach("Ve a la plataforma: el Falcon 1 ya está vertical", padSpot, "Jefa de plataforma", "Veintidós metros. Hace siete semanas era solo piezas en Hawthorne."),
                Visit("Revisa la granja de propelentes: el oxígeno líquido, el queroseno y el helio", fuel, "Ingeniero de propulsión", "El oxígeno se carga al final: a −183 °C, se evapora y hay que reponerlo hasta el último minuto."),
                Hold("Comprueba la telemetría con Hawthorne en la consola de la plataforma", console, 10f, "Técnico de telemetría", "Hawthorne nos ve. Si esta consola dice verde, el cohete está listo para que lo dejemos solo."),
                Talk("Habla con el técnico de telemetría", tele.transform, "Técnico de telemetría", "Lo controlamos todo desde Kwajalein, a treinta kilómetros. Aquí solo se queda el cohete."),
                Reach("Recoge: todos al muelle", pier, "Patrón de la lancha", "Contad cabezas. Nadie se queda en Omelek."),
                Hold("En la lancha, rumbo a Kwajalein: escucha la cuenta atrás por radio", boat, 12f, "Patrón de la lancha", "Once y quince. Despegue… separación… encendido de la segunda etapa… ¡En órbita!"),
                Talk("La jefa de plataforma, en la lancha", lead.transform, "Jefa de plataforma", "Cuatro intentos. Lo hemos hecho entre todos."),
            },
            "La isla de Omelek, en el atolón de Kwajalein (Islas Marshall), la mañana del 29 de septiembre de 2008 (el 28 en California), sobre el control de misión del santuario a escala de hormiga. El cuarto Falcon 1 ya está vertical en su plataforma. Haz las últimas revisiones con el equipo, cierra el hangar y evacúa la isla en la lancha antes del lanzamiento. E para hablar y revisar; para mirar, quédate quieto. La lancha: F para subir y bajar, W/A/S/D para conducir.",
            new[]
            {
                "Omelek es una isla de unas 2,8 hectáreas (siete acres) del atolón de Kwajalein, en las Islas Marshall, dentro del Reagan Test Site del Ejército de EE. UU. Desde 2006 SpaceX lanzó desde allí sus cinco Falcon 1, porque está cerca del ecuador y el cohete sube sobre el océano.",
                "La base era austera: una plataforma de hormigón, los tanques de propelentes, un hangar de procesado de tipo carpa y espacio de trabajo para los ingenieros. El helipuerto, los muelles, una vieja plataforma en la punta norte y dos bloques en el lado este ya estaban antes de SpaceX.",
                "El día del lanzamiento la isla se evacúa: el equipo sigue la cuenta atrás desde la sala de control de Kwajalein. El 28 de septiembre de 2008 a las 23:15 UTC (11:15 en las Marshall, 16:15 en California) el cuarto Falcon 1 llegó a órbita con 165 kg de lastre (RatSat): el primer cohete de combustible líquido desarrollado con dinero privado en conseguirlo.",
                "Supuestos de juego: la forma exacta de la isla, dónde va cada pieza, las medidas del hangar y de los contenedores, la granja de propelentes, la lancha y el reparto de cocoteros. Elon Musk no estaba en Omelek: lo vivió en Hawthorne (Microcosmos_ElonMusk).",
            },
            new[]
            {
                "Spaceflight Now, «Falcon 1 flight 4» (spaceflightnow.com/falcon/004) y crónica del vuelo 5 (2009): «austere launch pad with propellant tanks, a processing hangar and workspace».",
                "Spaceflight Now (2005), la base de Kwajalein: hormigón de la plataforma y edificios reformados; el hangar «parecido al de Vandenberg».",
                "Federal Register E8-1068 (23-1-2008), Finding of No Significant Impact: mejoras propuestas en Omelek (propelentes, hangar, plataforma de Falcon 9).",
                "Wikipedia, «Omelek Island» y «RatSat».",
                "Koenigsmann, H. et al. (2009). «Falcon 1 Launch Vehicle: Demo Flights, Status, Manifest and Upgrade Path». SmallSat Conference.",
                "Satellite Today (29-9-2008), «SpaceX Finally Launches Falcon 1, Rocket Soars To Orbit On Fourth Try».",
            },
            "Jefa de plataforma", "El cohete ya no es nuestro: es del cielo.");
        return n;
    }
}
