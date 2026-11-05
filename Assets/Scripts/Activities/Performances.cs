using System.Collections.Generic;
using UnityEngine;
using P = ActivityKit.Prop;
using F = ActivityKit.Fx;
using B = BodyPart;

/// <summary>
/// EL CATÁLOGO DE ACTUACIONES (docs/actividades-y-uniformes.md §3): qué hace cada misión de la jornada, cada actividad del horario
/// libre y cada clase de función del microcosmos cuando el personaje ya está en su sitio. Por orden de lo común que es en la jornada:
///  1. Las misiones del catálogo diario del santuario con PASOS PROPIOS (<see cref="ForMission"/>): pasar visita, regar, limpiar…
///  2. Las de prefijo (obras, elaboraciones del obrador).
///  3. LA COLA LARGA: lo que no tiene pasos propios hace el BUCLE GENÉRICO de su clase (<see cref="ForKind"/>: herramienta en la mano,
///     postura de trabajo y su efecto periódico), marcado como genérico (la lista de pendientes del doc).
/// Los gestos (<see cref="M"/>) son dos fotogramas A ↔ B por hueso (ejes del cuerpo de cubos: X negativo en el hombro = brazo adelante,
/// en el antebrazo = doblar el codo; X positivo en la espalda = inclinarse).
/// </summary>
public static class Performances
{
    // ── Los gestos ──────────────────────────────────────────────────────────────────────────────────────────────
    public static class M
    {
        static (B, Vector3, Vector3) J(B b, float ax, float bx, float az = 0f, float bz = 0f, float ay = 0f, float by = 0f) => (b, new Vector3(ax, ay, az), new Vector3(bx, by, bz));

        /// <summary>Trabajar en la encimera (amasar, ordenar): los dos brazos adelante, empujando.</summary>
        public static readonly ActivityKit.Arms Reach = new ActivityKit.Arms(1.2f, J(B.Back, 8, 14), J(B.ShoulderLeft, -45, -58, 8, 8), J(B.ShoulderRight, -45, -58, -8, -8), J(B.ForearmLeft, -40, -15), J(B.ForearmRight, -40, -15));
        /// <summary>Remover la olla: el brazo derecho en círculo; el izquierdo sujeta.</summary>
        public static readonly ActivityKit.Arms Stir = new ActivityKit.Arms(1.0f, J(B.ShoulderRight, -58, -48, -14, 14), J(B.ForearmRight, -38, -24), J(B.ShoulderLeft, -32, -32), J(B.ForearmLeft, -60, -60));
        /// <summary>Picar con el cuchillo: el antebrazo sube y baja.</summary>
        public static readonly ActivityKit.Arms Chop = new ActivityKit.Arms(0.6f, J(B.ShoulderRight, -42, -30), J(B.ForearmRight, -80, -22), J(B.ShoulderLeft, -36, -36), J(B.ForearmLeft, -42, -42), J(B.Neck, 15, 15));
        /// <summary>Agachado sobre el bancal (deshierbar, cosechar, plantar).</summary>
        public static readonly ActivityKit.Arms Stoop = new ActivityKit.Arms(1.6f, J(B.Back, 35, 48), J(B.Chest, 14, 20), J(B.ShoulderLeft, -60, -82), J(B.ShoulderRight, -78, -56), J(B.ForearmLeft, -12, -6), J(B.ForearmRight, -6, -14));
        /// <summary>Regar: el brazo adelante y la muñeca que inclina la regadera.</summary>
        public static readonly ActivityKit.Arms Water = new ActivityKit.Arms(2.4f, J(B.ShoulderRight, -52, -64), J(B.ForearmRight, -14, -6), J(B.HandRight, 0, 0, 0, -38), J(B.ShoulderLeft, -8, -8));
        /// <summary>Martillar: el brazo arriba y abajo.</summary>
        public static readonly ActivityKit.Arms Hammer = new ActivityKit.Arms(0.7f, J(B.ShoulderRight, -150, -55), J(B.ForearmRight, -40, -10), J(B.ShoulderLeft, -42, -42), J(B.ForearmLeft, -30, -30), J(B.Back, 5, 15));
        /// <summary>Barrer / fregar el suelo: los dos brazos de lado a lado.</summary>
        public static readonly ActivityKit.Arms Sweep = new ActivityKit.Arms(1.3f, J(B.Back, 12, 12), J(B.ShoulderRight, -35, -35, 25, -25), J(B.ShoulderLeft, -50, -50, 20, -20), J(B.ForearmRight, -20, -20), J(B.ForearmLeft, -40, -40));
        /// <summary>Limpiar una superficie con la bayeta: círculos con la mano derecha.</summary>
        public static readonly ActivityKit.Arms Wipe = new ActivityKit.Arms(0.9f, J(B.Back, 15, 15), J(B.ShoulderRight, -60, -54, -25, 20), J(B.ForearmRight, -25, -15), J(B.ShoulderLeft, -22, -22));
        /// <summary>Anotar en la tablilla: la izquierda la sujeta, la derecha escribe.</summary>
        public static readonly ActivityKit.Arms Write = new ActivityKit.Arms(0.8f, J(B.ShoulderLeft, -35, -35, 15, 15), J(B.ForearmLeft, -85, -85), J(B.ShoulderRight, -30, -33, -15, -5), J(B.ForearmRight, -95, -88), J(B.Neck, 16, 20));
        /// <summary>Leer: el libro con las dos manos y la cabeza baja; de vez en cuando, pasa la página.</summary>
        public static readonly ActivityKit.Arms Read = new ActivityKit.Arms(3.0f, J(B.ShoulderLeft, -35, -35, 10, 10), J(B.ShoulderRight, -35, -40, -10, -18), J(B.ForearmLeft, -80, -82), J(B.ForearmRight, -80, -92), J(B.Neck, 18, 22));
        /// <summary>Teclear en una consola: las manos alternan.</summary>
        public static readonly ActivityKit.Arms Type = new ActivityKit.Arms(0.4f, J(B.ShoulderLeft, -45, -48), J(B.ShoulderRight, -48, -45), J(B.ForearmLeft, -40, -50), J(B.ForearmRight, -50, -40), J(B.Neck, 10, 10));
        /// <summary>Revisar algo (un monitor, una máquina, una planta): inclinarse y señalar.</summary>
        public static readonly ActivityKit.Arms Inspect = new ActivityKit.Arms(2.2f, J(B.Back, 10, 20), J(B.Neck, 15, 25), J(B.ShoulderRight, -80, -70, 0, -15), J(B.ForearmRight, -10, -10), J(B.ShoulderLeft, -10, -10));
        /// <summary>Llevar una caja o una bandeja a la altura del pecho.</summary>
        public static readonly ActivityKit.Arms Carry = new ActivityKit.Arms(2.0f, J(B.ShoulderLeft, -25, -32), J(B.ShoulderRight, -25, -32), J(B.ForearmLeft, -80, -72), J(B.ForearmRight, -80, -72));
        /// <summary>Explicar en la pizarra: el brazo derecho alto, señalando.</summary>
        public static readonly ActivityKit.Arms Teach = new ActivityKit.Arms(1.8f, J(B.ShoulderRight, -120, -100, 0, -25), J(B.ForearmRight, -15, -30), J(B.ShoulderLeft, -20, -20), J(B.ForearmLeft, -60, -60));
        /// <summary>Atender a un paciente en la cama: inclinado sobre él, las dos manos.</summary>
        public static readonly ActivityKit.Arms Treat = new ActivityKit.Arms(1.5f, J(B.Back, 25, 30), J(B.ShoulderLeft, -55, -65), J(B.ShoulderRight, -65, -55), J(B.ForearmLeft, -30, -20), J(B.ForearmRight, -20, -30));
        /// <summary>Poner una inyección o una vía.</summary>
        public static readonly ActivityKit.Arms Inject = new ActivityKit.Arms(1.2f, J(B.Back, 10, 10), J(B.ShoulderRight, -80, -75), J(B.ForearmRight, -45, -35), J(B.ShoulderLeft, -60, -60, 15, 15), J(B.ForearmLeft, -30, -30));
        /// <summary>Coser: la aguja sale y tira del hilo.</summary>
        public static readonly ActivityKit.Arms Sew = new ActivityKit.Arms(0.9f, J(B.ShoulderLeft, -40, -40), J(B.ShoulderRight, -40, -34, 0, -20), J(B.ForearmLeft, -70, -70), J(B.ForearmRight, -80, -55), J(B.Neck, 20, 20));
        /// <summary>Dar el biberón o la ración a un animal (agachado).</summary>
        public static readonly ActivityKit.Arms Feed = new ActivityKit.Arms(1.4f, J(B.Back, 30, 34), J(B.ShoulderRight, -70, -60), J(B.ForearmRight, -20, -10), J(B.ShoulderLeft, -50, -55), J(B.ForearmLeft, -40, -30));
        /// <summary>Vigilar: la linterna adelante y la cabeza que mira a un lado y a otro.</summary>
        public static readonly ActivityKit.Arms Patrol = new ActivityKit.Arms(4.0f, J(B.Neck, 0, 0, 0, 0, -30, 30), J(B.ShoulderRight, -30, -30), J(B.ForearmRight, -60, -60));
        /// <summary>Levantar del suelo hasta el pecho (paja, sacos, cestas).</summary>
        public static readonly ActivityKit.Arms Lift = new ActivityKit.Arms(2.0f, J(B.Back, 40, 5), J(B.ShoulderLeft, -80, -30), J(B.ShoulderRight, -80, -30), J(B.ForearmLeft, -10, -60), J(B.ForearmRight, -10, -60));
        /// <summary>Pipetear: la derecha aprieta el émbolo, la izquierda sujeta el matraz.</summary>
        public static readonly ActivityKit.Arms Pipette = new ActivityKit.Arms(1.1f, J(B.ShoulderRight, -55, -55), J(B.ForearmRight, -70, -55), J(B.ShoulderLeft, -45, -45), J(B.ForearmLeft, -50, -50), J(B.Neck, 15, 15));
        /// <summary>Charlar con una taza: la derecha gesticula, la izquierda bebe de vez en cuando.</summary>
        public static readonly ActivityKit.Arms Talk = new ActivityKit.Arms(2.2f, J(B.ShoulderRight, -30, -45, 0, -15), J(B.ForearmRight, -60, -90), J(B.ShoulderLeft, -35, -35), J(B.ForearmLeft, -90, -110, 0, -20));
        /// <summary>Jugar con una cría: agachado, las manos que la acarician.</summary>
        public static readonly ActivityKit.Arms Play = new ActivityKit.Arms(1.2f, J(B.Back, 30, 34), J(B.ShoulderLeft, -70, -80, 15, 5), J(B.ShoulderRight, -70, -80, -15, -5), J(B.ForearmLeft, -20, -20), J(B.ForearmRight, -20, -20));
        /// <summary>Golpes de entrenamiento (puño izquierdo, puño derecho).</summary>
        public static readonly ActivityKit.Arms Spar = new ActivityKit.Arms(0.8f, J(B.ShoulderLeft, -85, -30), J(B.ShoulderRight, -30, -85), J(B.ForearmLeft, -5, -100), J(B.ForearmRight, -100, -5));
        /// <summary>Rezar / meditar con las manos juntas.</summary>
        public static readonly ActivityKit.Arms Pray = new ActivityKit.Arms(4.0f, J(B.ShoulderLeft, -40, -42, 20, 20), J(B.ShoulderRight, -40, -42, -20, -20), J(B.ForearmLeft, -90, -90, -40, -40), J(B.ForearmRight, -90, -90, 40, 40), J(B.Neck, 10, 14));
        /// <summary>Echar de una jarra (servir agua, el medio de cultivo).</summary>
        public static readonly ActivityKit.Arms Pour = new ActivityKit.Arms(2.0f, J(B.ShoulderRight, -60, -70), J(B.ForearmRight, -30, -20), J(B.HandRight, 0, 0, 0, -50), J(B.ShoulderLeft, -30, -30), J(B.ForearmLeft, -50, -50));
        /// <summary>Quieto con los brazos relajados (descansar).</summary>
        public static readonly ActivityKit.Arms Rest = new ActivityKit.Arms(4.0f, J(B.Neck, 0, 6));
    }

    // ── Construir ───────────────────────────────────────────────────────────────────────────────────────────────
    static ActivityPerformer.Step S(string label, ActivityKit.Arms arms, P right, P left, float seconds, F fx, float every = 1.6f,
        ActivityPerformer.InsectArm insect = ActivityPerformer.InsectArm.Push, ActivityPerformer.Base pose = ActivityPerformer.Base.Stand, bool atFace = false, System.Action<GameObject> act = null)
        => new ActivityPerformer.Step { label = label, arms = arms, right = right, left = left, seconds = seconds, fx = fx, fxEvery = every, insect = insect, pose = pose, fxAtFace = atFace, act = act };

    const ActivityPerformer.InsectArm Push = ActivityPerformer.InsectArm.Push, Strike = ActivityPerformer.InsectArm.Strike, Fine = ActivityPerformer.InsectArm.Fine, Raise = ActivityPerformer.InsectArm.Raise, Still = ActivityPerformer.InsectArm.Still;
    const ActivityPerformer.Base Sit = ActivityPerformer.Base.Sit, Med = ActivityPerformer.Base.Meditate;

    static readonly Dictionary<string, ActivityPerformer.Performance> _missions = new Dictionary<string, ActivityPerformer.Performance>();
    static readonly Dictionary<AreaFunctions.Kind, ActivityPerformer.Performance> _kinds = new Dictionary<AreaFunctions.Kind, ActivityPerformer.Performance>();
    static readonly Dictionary<string, ActivityPerformer.Performance> _leisure = new Dictionary<string, ActivityPerformer.Performance>();
    static readonly Dictionary<string, ActivityPerformer.Performance> _training = new Dictionary<string, ActivityPerformer.Performance>();

    static void Mi(string id, string title, params ActivityPerformer.Step[] steps) => _missions[id] = new ActivityPerformer.Performance(id, title, steps);

    // Pasos que se repiten en varias misiones.
    static ActivityPerformer.Step Sweeping(float s = 9f) => S("Barrer el suelo", M.Sweep, P.Broom, P.None, s, F.Dust, 1.3f);
    static ActivityPerformer.Step Mopping(float s = 9f) => S("Fregar el suelo", M.Sweep, P.Mop, P.None, s, F.Water, 1.2f);
    static ActivityPerformer.Step Wiping(string what = "Limpiar las superficies", float s = 8f) => S(what, M.Wipe, P.Spray, P.Cloth, s, F.Bubbles, 1.1f, atFace: true);
    static ActivityPerformer.Step Noting(string what = "Anotar en la tablilla", float s = 6f) => S(what, M.Write, P.Pen, P.Clipboard, s, F.Paper, 2.5f, Fine);
    static ActivityPerformer.Step Console(string what = "Revisar la consola", float s = 8f) => S(what, M.Type, P.None, P.None, s, F.Glow, 1.4f, Fine, atFace: true);
    static ActivityPerformer.Step Checking(string what, P tool = P.Tablet, F fx = F.Glow, float s = 7f) => S(what, M.Inspect, P.None, tool, s, fx, 2f, Raise, atFace: true);

    static Performances()
    {
        // ══ 1. EL CATÁLOGO DIARIO DEL SANTUARIO (las más comunes primero: las obligatorias de cada día) ══
        Mi("enf_visita", "Pasar visita y curas",
            S("Auscultar al paciente", M.Treat, P.Stethoscope, P.None, 7f, F.Heart, 3f, Fine, atFace: true),
            S("Tomar la temperatura", M.Inspect, P.Thermometer, P.None, 5f, F.None),
            S("Cambiar el vendaje", M.Treat, P.Bandage, P.Bandage, 7f, F.Heart, 3.5f, Fine, atFace: true),
            Noting("Anotar en la historia clínica"));
        Mi("hos_urgencias", "Guardia de urgencias",
            Noting("Triaje: anotar la gravedad", 5f),
            S("Explorar al paciente", M.Treat, P.Stethoscope, P.None, 7f, F.Heart, 3f, Fine, atFace: true),
            S("Poner una vía", M.Inject, P.Syringe, P.None, 6f, F.None, 2f, Fine),
            Checking("Mirar el monitor", P.None, F.Glow, 5f));
        Mi("hos_uci", "Cuidados intensivos",
            Checking("Vigilar las constantes en el monitor", P.Tablet),
            S("Ajustar la medicación", M.Inject, P.Syringe, P.None, 6f, F.None, 2f, Fine),
            S("Cambiar de postura al paciente", M.Treat, P.None, P.None, 7f, F.Heart, 3f, atFace: true),
            Noting());
        Mi("hos_quirofano", "Quirófano",
            S("Lavarse las manos y los antebrazos", M.Wipe, P.Sponge, P.None, 6f, F.Water, 1f),
            S("Operar", M.Treat, P.Scalpel, P.None, 10f, F.None, 2f, Fine, atFace: true),
            S("Suturar", M.Sew, P.Needle, P.None, 8f, F.Thread, 1.5f, Fine, atFace: true),
            Checking("Revisar la anestesia", P.None, F.Glow, 5f));
        Mi("hos_veterinaria", "Hospital veterinario",
            S("Explorar al animal", M.Feed, P.Stethoscope, P.None, 7f, F.Heart, 3f, atFace: true),
            S("Vacunar o medicar", M.Inject, P.Syringe, P.None, 6f, F.None, 2f, Fine),
            S("Darle de comer", M.Feed, P.Bottle, P.None, 6f, F.Heart, 3f),
            Noting());
        _missions["enf_animales"] = _missions["hos_veterinaria"];
        Mi("enf_inventario", "Limpieza, esterilización e inventario",
            Wiping("Desinfectar la camilla"),
            S("Reponer el material", M.Carry, P.Box, P.None, 7f, F.None),
            Noting("Contar el inventario"));
        Mi("enf_iniciacion", "Iniciación de recién llegados",   // (la hace el iniciador en orquesta; esto, mientras espera)
            S("Preparar la vacuna", M.Pipette, P.Syringe, P.Flask, 6f, F.Bubbles, 2f, Fine),
            Noting("Preparar la ficha del recién llegado"));
        // La cocina: si la brigada del restaurante la mueve (RestaurantService), eso manda; esto es lo que hace sin servicio.
        Mi("coc_servicio", "Cocinar la carta",
            S("Picar las verduras", M.Chop, P.Knife, P.None, 7f, F.Leaves, 1.2f, Strike, atFace: true),
            S("Remover la olla", M.Stir, P.Ladle, P.None, 8f, F.Steam, 1f, Push, atFace: true),
            S("Saltear en la sartén", M.Stir, P.Pan, P.None, 6f, F.Steam, 0.8f, Push, atFace: true),
            S("Emplatar", M.Reach, P.Spoon, P.Plate, 5f, F.None));
        Mi("coc_obrador", "Elaboraciones del obrador",
            S("Amasar", M.Reach, P.None, P.None, 9f, F.Dust, 1.2f, Push, atFace: true),
            S("Batir", M.Stir, P.Whisk, P.None, 6f, F.Bubbles, 1f),
            S("Llevar la bandeja al horno", M.Carry, P.Tray, P.None, 5f, F.Steam, 2f));
        Mi("coc_limpieza", "Limpieza e inventario de la cocina",
            S("Fregar los cacharros", M.Wipe, P.Sponge, P.None, 9f, F.Bubbles, 0.9f, atFace: true),
            Mopping(),
            Noting("Apuntar lo que falta en la despensa"));
        Mi("coc_sala", "Atender a los turistas en sala",
            Noting("Tomar nota del pedido", 5f),
            S("Llevar los platos", M.Carry, P.Tray, P.None, 7f, F.Steam, 2.5f),
            S("Servir el agua", M.Pour, P.Pitcher, P.None, 5f, F.Water, 1.2f, atFace: true));
        _missions["coc_atender"] = _missions["coc_sala"];
        Mi("coc_comedor", "Limpieza del comedor y de los aseos",
            Wiping("Limpiar las mesas"),
            Sweeping(),
            Mopping());
        Mi("hue_riego", "Riego y deshierbe",
            S("Regar los bancales", M.Water, P.WateringCan, P.None, 9f, F.Water, 0.7f, atFace: true),
            S("Levantar las malas hierbas con su raíz", M.Stoop, P.None, P.None, 9f, F.Soil, 1.2f, atFace: true, act: g => WeedBench.LiftNear(g.transform.position, g.name)),
            S("Cavar con la azada", M.Hammer, P.Hoe, P.None, 7f, F.Soil, 0.9f, Strike, atFace: true));
        Mi("hue_cosecha", "Cosecha para la cocina",
            S("Cortar la cosecha", M.Stoop, P.Shears, P.None, 9f, F.Leaves, 1.1f, Fine, atFace: true),
            S("Llenar la cesta", M.Lift, P.Basket, P.None, 7f, F.Leaves, 2f),
            S("Llevar la cesta", M.Carry, P.Basket, P.None, 5f, F.None));
        Mi("hue_visitantes", "Alimentar a los visitantes del huerto",
            S("Repartir la fruta", M.Lift, P.Basket, P.None, 7f, F.Leaves, 2f),
            S("Echar agua en el bebedero", M.Pour, P.Pitcher, P.None, 6f, F.Water, 1f, atFace: true));
        Mi("yog_sala", "Preparar y limpiar las salas de yoga",
            Sweeping(),
            S("Colocar las esterillas", M.Lift, P.None, P.None, 7f, F.Dust, 2.5f),
            Wiping("Limpiar los accesorios"));
        Mi("yog_aseos", "Limpieza de los aseos de yoga", Mopping(), Wiping("Limpiar los lavabos"));
        Mi("mec_vehiculos", "Mantenimiento de vehículos",
            Checking("Revisar el motor con la linterna", P.Flashlight, F.None, 6f),
            S("Apretar los tornillos", M.Reach, P.Wrench, P.None, 8f, F.Sparks, 1.4f, Strike, atFace: true),
            S("Martillar la chapa", M.Hammer, P.Hammer, P.None, 6f, F.Sparks, 0.7f, Strike, atFace: true),
            Noting("Anotar la revisión"));
        _missions["mec_maquinas"] = _missions["mec_vehiculos"];
        Mi("tex_tejer", "Tejer y teñir",
            S("Tejer en el telar", M.Reach, P.Yarn, P.None, 10f, F.Thread, 1.2f, Fine, atFace: true),
            S("Teñir en la cuba", M.Stir, P.Ladle, P.None, 7f, F.Bubbles, 1f, atFace: true));
        Mi("tex_coser", "Coser y reparar ropa",
            S("Coser", M.Sew, P.Needle, P.Yarn, 10f, F.Thread, 1.1f, Fine),
            S("Cortar la tela", M.Chop, P.Shears, P.None, 5f, F.Thread, 1.5f, Fine, atFace: true));
        Mi("lab_biorreactor", "Supervisar los biorreactores",
            Console("Leer la glucosa, el pH y el oxígeno"),
            S("Tomar una muestra", M.Pipette, P.Pipette, P.Flask, 7f, F.Bubbles, 1.5f, Fine),
            Noting("Anotar las lecturas"));
        Mi("lab_iniciar", "Iniciar un lote",
            S("Sacar el vial del banco", M.Reach, P.Flask, P.None, 5f, F.Steam, 1.5f, Fine, atFace: true),
            S("Sembrar el medio", M.Pipette, P.Pipette, P.Flask, 8f, F.Bubbles, 1.2f, Fine),
            Console("Programar el biorreactor", 5f));
        Mi("lab_cosechar", "Cosechar el lote",
            S("Sacar la carne del biorreactor", M.Reach, P.Tray, P.None, 6f, F.Steam, 1.5f, atFace: true),
            S("Darle forma", M.Chop, P.Knife, P.None, 7f, F.None, 1f, Strike),
            S("Llevar las bandejas", M.Carry, P.Tray, P.None, 5f, F.None));
        Mi("lab_limpieza", "Limpieza y esterilización del laboratorio", Wiping("Esterilizar las cabinas"), Mopping());
        Mi("con_almacen", "Inventario del almacén",
            S("Ordenar las cajas", M.Carry, P.Box, P.None, 8f, F.Dust, 2f),
            Noting("Contar el material y las herramientas"));
        Mi("cri_tomas", "Tomas y pesaje de las crías",
            S("Dar el biberón", M.Feed, P.Bottle, P.None, 9f, F.Heart, 2.5f),
            S("Pesar a la cría", M.Lift, P.None, P.None, 6f, F.Heart, 3f),
            S("Anotar en la pizarra", M.Teach, P.Chalk, P.None, 5f, F.Chalk, 1.2f, Raise));
        Mi("cri_recintos", "Limpiar los recintos y cambiar la paja",
            S("Rastrillar la paja vieja", M.Sweep, P.Broom, P.None, 8f, F.Dust, 1.1f),
            S("Echar paja nueva", M.Lift, P.None, P.None, 7f, F.Leaves, 1.4f),
            S("Llenar el bebedero", M.Pour, P.Bucket, P.None, 5f, F.Water, 1f, atFace: true));
        Mi("cri_vinculo", "Cuidar una cría hasta ganar su vínculo",
            S("Calmarla y acariciarla", M.Play, P.None, P.None, 9f, F.Heart, 2f),
            S("Darle de comer", M.Feed, P.Bottle, P.None, 7f, F.Heart, 2.5f),
            S("Leerle en voz baja", M.Read, P.Book, P.None, 7f, F.Paper, 3f, Fine),
            S("Asearla con el cepillo", M.Wipe, P.Brush, P.None, 7f, F.Dust, 1.4f));
        Mi("gym_limpieza", "Limpieza del gimnasio", Wiping("Desinfectar las máquinas"), Mopping());
        Mi("mec_limpieza", "Limpieza e inventario del taller", Wiping("Desengrasar el banco"), Sweeping(), Noting("Contar las herramientas"));
        Mi("tex_limpieza", "Limpieza e inventario del estudio textil", Sweeping(), S("Doblar las telas", M.Lift, P.None, P.None, 6f, F.Thread, 2f), Noting("Contar los hilos y las telas"));
        Mi("gym_mantener", "Mantenimiento del gimnasio",
            Checking("Revisar los cables y las poleas", P.Flashlight, F.None, 6f),
            S("Ajustar las máquinas", M.Reach, P.Wrench, P.None, 8f, F.Sparks, 1.5f, Strike, atFace: true),
            S("Engrasar las guías", M.Wipe, P.Cloth, P.None, 6f, F.None));
        Mi("pla_operar", "Operar el reactor",
            Console("Subir a potencia en la consola", 9f),
            Checking("Vigilar los indicadores", P.Tablet),
            Noting("Registrar la maniobra"));
        _missions["pla_isotopos"] = _missions["pla_operar"]; _missions["pla_ciclotron"] = _missions["pla_operar"]; _missions["nuc_turno"] = _missions["pla_operar"];
        Mi("pla_ronda", "Ronda de la planta",
            S("Recorrer con la linterna", M.Patrol, P.Flashlight, P.None, 8f, F.None, 2f, Raise),
            Checking("Medir la radiación con el dosímetro", P.Tablet, F.Glow, 6f),
            Noting("Firmar la ronda"));
        Mi("fis_experimentos", "Experimentos de física",
            Checking("Alinear el láser", P.None, F.Glow, 7f),
            S("Ajustar el montaje", M.Reach, P.Screwdriver, P.None, 7f, F.None, 2f, Fine, atFace: true),
            Noting("Apuntar las franjas"));
        Mi("qui_sintesis", "Síntesis en la campana",
            S("Pesar el reactivo", M.Pipette, P.Spoon, P.Flask, 6f, F.Dust, 2f, Fine, atFace: true),
            S("Remover la reacción", M.Stir, P.None, P.Flask, 8f, F.Steam, 1f, Push, atFace: true),
            S("Pipetear el producto", M.Pipette, P.Pipette, P.Flask, 6f, F.Bubbles, 1.3f, Fine),
            Noting("Anotar el rendimiento"));
        Mi("qui_residuos", "Recoger y separar los residuos químicos",
            S("Llevar los bidones", M.Carry, P.Box, P.None, 7f, F.None),
            S("Separar ácidos, bases e inflamables", M.Lift, P.Flask, P.None, 6f, F.Bubbles, 2f),
            Noting("Etiquetar"));
        Mi("bio_cultivo", "Cultivo celular",
            S("Trabajar en la cabina", M.Pipette, P.Pipette, P.Flask, 9f, F.Bubbles, 1.3f, Fine, atFace: true),
            Checking("Mirar al microscopio", P.None, F.Glow, 7f),
            Noting());
        Mi("bio_pcr", "Del ADN a la secuencia",
            S("Extraer el ADN", M.Pipette, P.Pipette, P.Flask, 8f, F.Bubbles, 1.3f, Fine, atFace: true),
            Console("Programar el termociclador", 6f),
            Checking("Leer la secuencia", P.Tablet));
        Mi("ifo_sysadmin", "Administrar el clúster de IA",
            Console("Revisar colas y alertas", 9f),
            Checking("Mirar los paneles", P.Tablet),
            Console("Aplicar los parches", 6f));
        Mi("ifo_refrigeracion", "Mantener la refrigeración líquida",
            Checking("Buscar fugas con la linterna", P.Flashlight, F.None, 6f),
            S("Cambiar los filtros", M.Reach, P.Wrench, P.None, 8f, F.Water, 1.2f, Strike, atFace: true),
            Noting("Anotar la presión"));
        Mi("ifo_energia", "Revisar el SAI y las baterías",
            Checking("Medir las baterías", P.Tablet, F.Sparks, 6f),
            Noting("Registrar la carga"));
        Mi("asa_orden", "Preparar el orden del día",
            Noting("Reunir los temas de la semana"),
            S("Sortear quién modera", M.Reach, P.None, P.None, 6f, F.None, 1.6f),
            Console("Poner el orden del día en el marcador", 6f));
        Mi("asa_acta", "Levantar el acta",
            Noting("Escribir lo acordado"),
            Checking("Leerlo en voz alta", P.Tablet),
            Noting("Guardarlo en el archivo"));
        Mi("asa_mediar", "Mediar en un desacuerdo",
            S("Escuchar a una parte", M.Reach, P.None, P.None, 8f, F.None, 1.6f),
            S("Escuchar a la otra", M.Reach, P.None, P.None, 8f, F.None, 1.6f),
            Noting("Apuntar el acuerdo"));
        Mi("son_grabar", "Grabar y mezclar",
            Console("Ajustar la mesa de mezclas", 9f),
            Checking("Escuchar la toma", P.Tablet),
            Console("Mezclar las pistas", 6f));
        Mi("son_ensayo", "Ensayar el concierto",
            S("Afinar el instrumento", M.Reach, P.Wrench, P.None, 8f, F.None, 1.6f),
            S("Tocar la pieza", M.Reach, P.None, P.None, 10f, F.None, 1.6f),
            Noting("Anotar los cambios"));
        Mi("son_equipo", "Revisar instrumentos y sonido",
            S("Cambiar cuerdas y cables", M.Reach, P.Wrench, P.None, 8f, F.None, 1.6f),
            Checking("Probar los micrófonos", P.Tablet, F.None, 6f));
        Mi("sol_mantenimiento", "Mantener los paneles solares",
            S("Quitar la escarcha", M.Sweep, P.Brush, P.None, 9f, F.Dust, 1.2f),
            Checking("Revisar los módulos", P.Tablet, F.Glow, 6f),
            Noting("Anotar las mesas revisadas"));
        Mi("sol_instalacion", "Instalar paneles solares",
            S("Subir el módulo a la mesa", M.Reach, P.Box, P.None, 8f, F.Dust, 1.5f, Raise, atFace: true),
            S("Apretar las fijaciones", M.Reach, P.Wrench, P.None, 8f, F.Sparks, 1.4f, Strike, atFace: true),
            Checking("Probar la cadena", P.Tablet, F.Sparks, 6f));
        Mi("sol_reparacion", "Reparar paneles solares",
            Checking("Buscar la avería", P.Flashlight, F.None, 6f),
            S("Cambiar el módulo", M.Reach, P.Screwdriver, P.None, 8f, F.Sparks, 1.2f, Strike, atFace: true),
            Noting("Anotar la reparación"));
        Mi("ene_control", "Llevar la energía",
            Console("Repartir las cargas", 9f),
            Noting("Anotar las paradas por fauna"));
        Mi("ene_eolica", "Revisar los aerogeneradores",
            Checking("Revisar cámaras y radar", P.Tablet, F.Glow, 7f),
            Checking("Mirar las palas", P.Flashlight, F.None, 7f),
            Noting("Anotar la revisión"));
        Mi("ene_mareal", "Revisar la turbina mareal",
            Checking("Probar el sonar", P.Tablet, F.Glow, 7f),
            Checking("Buscar fugas", P.Flashlight, F.Water, 7f));
        Mi("instruir", "Aprender a iniciar a otros",
            S("Explicar la tarea", M.Teach, P.None, P.Book, 9f, F.None, 2f, Raise),
            Noting("Anotar lo aprendido"));
        Mi("ene_ronda_aves", "Ronda de aves",
            Checking("Buscar aves heridas", P.Flashlight, F.None, 8f),
            S("Recoger con cuidado", M.Reach, P.Box, P.None, 6f, F.None, 2f, Fine),
            Noting("Anotar lo encontrado"));
        Mi("ene_disuasion", "Cambiar las figuras de sitio",
            S("Mover la figura", M.Reach, P.Wrench, P.None, 8f, F.Sparks, 1.4f, Strike, atFace: true),
            Checking("Revisar los drones", P.Tablet, F.Glow, 7f));
        Mi("hos_aves", "Curar aves",
            S("Curar el ala", M.Reach, P.Bandage, P.None, 8f, F.None, 2f, Fine),
            Noting("Anotar la evolución"));
        Mi("ene_almacen", "Revisar el almacén",
            Checking("Medir las baterías", P.Tablet, F.Sparks, 6f),
            Noting("Registrar la temperatura"));
        Mi("aul_clase", "Dar o recibir una clase",
            S("Explicar en la pizarra", M.Teach, P.Chalk, P.Book, 9f, F.Chalk, 1.2f, Raise, atFace: true),
            S("Tomar apuntes", M.Write, P.Pen, P.Book, 7f, F.Paper, 2.5f, Fine));
        Mi("aul_estudio", "Estudiar en la biblioteca",
            S("Leer", M.Read, P.Book, P.None, 10f, F.Paper, 3f, Fine),
            S("Tomar apuntes", M.Write, P.Pen, P.Book, 7f, F.Paper, 2.5f, Fine));
        Mi("gua_sala", "Turno en la sala de guardia",
            S("Mirar las pantallas", M.Patrol, P.Clipboard, P.None, 8f, F.None, 2f, Raise),
            Noting("Firmar el parte del turno"));
        Mi("gua_carnivoros", "Patrulla",   // (la lleva CarnivoreKeeper; esto, en sus pausas)
            S("Vigilar", M.Patrol, P.Flashlight, P.None, 8f, F.None, 2f, Raise),
            Noting("Anotar la ronda"));
        foreach (string ali in new[] { "gua_alimentador", "gua_alimentador_2" })
            Mi(ali, "Alimentador",   // (la lleva PredatorFeeder; esto, en sus pausas)
                S("Mirar la lista compartida", M.Patrol, P.Clipboard, P.None, 6f, F.None, 2f, Raise),
                S("Preparar la ración", M.Chop, P.Knife, P.None, 6f, F.None, 1f, Strike));
        Mi("hue_transplante", "Trasplante a la tundra",
            S("Abrir el hueco", M.Hammer, P.Hoe, P.None, 8f, F.Soil, 0.9f, Strike, atFace: true),
            S("Plantar el cepellón", M.Stoop, P.None, P.None, 8f, F.Soil, 1.2f, atFace: true, act: g => WeedBench.TransplantAll(g.name)),
            S("Regar", M.Water, P.WateringCan, P.None, 6f, F.Water, 0.8f, atFace: true, act: g => WeedBench.IncinerateBags(g.name)));
        // Las del día (material para las obras).
        Mi("mat_guadua", "Cortar guadua",
            S("Cortar la caña con el machete", M.Hammer, P.Machete, P.None, 8f, F.Leaves, 0.7f, Strike, atFace: true),
            S("Cargar las cañas", M.Carry, P.Rope, P.None, 6f, F.None));
        Mi("mat_bahareque", "Preparar barro con paja",
            S("Mezclar el barro", M.Stir, P.Hoe, P.None, 9f, F.Soil, 0.9f, Push, atFace: true),
            S("Echar la paja", M.Lift, P.None, P.None, 6f, F.Leaves, 1.3f),
            S("Llevar el cubo", M.Carry, P.Bucket, P.None, 5f, F.None));
        Mi("mat_micelio", "Sembrar paneles de micelio",
            S("Llenar los moldes", M.Reach, P.None, P.None, 8f, F.Dust, 1.2f, Push, atFace: true),
            S("Sembrar el micelio", M.Pipette, P.Spoon, P.Flask, 6f, F.Dust, 1.6f, Fine));
        Mi("mat_cuerda", "Trenzar cuerda de cáñamo", S("Trenzar", M.Sew, P.Rope, P.None, 10f, F.Thread, 1.1f, Fine));
        // Las de prefijo: una por obra y por elaboración.
        Mi("obra_material_", "Material para la obra",
            S("Cargar los sacos y las cañas", M.Lift, P.None, P.None, 7f, F.Dust, 1.5f),
            S("Llevar el material", M.Carry, P.Box, P.None, 7f, F.Dust, 2.5f),
            Noting("Apuntar lo que llega"));
        Mi("obra_trabajo_", "Trabajar en la obra",
            S("Clavar y atar las cañas", M.Hammer, P.Hammer, P.None, 9f, F.Dust, 0.7f, Strike, atFace: true),
            S("Revocar con la llana", M.Wipe, P.Trowel, P.Bucket, 8f, F.Soil, 1f, atFace: true),
            Checking("Comprobar el nivel", P.None, F.None, 5f));
        Mi("prep_", "Elaboración del obrador",
            S("Mezclar los ingredientes", M.Stir, P.Whisk, P.None, 8f, F.Bubbles, 1f, atFace: true),
            S("Amasar o moldear", M.Reach, P.None, P.None, 8f, F.Dust, 1.2f, atFace: true),
            S("Llevar al reposo", M.Carry, P.Tray, P.None, 5f, F.None));
        // ══ ENTRENAMIENTOS de las 14 h que no llevan su propio sistema (el gimnasio y el yoga sí lo llevan) ══
        // (2026-11-05) El entrenamiento de las 8:00: en el gimnasio y en yoga no hay clase a esa hora, así que entrena cada uno en su sitio.
        _training["Gimnasio"] = new ActivityPerformer.Performance("ent_gimnasio", "Entrenar por su cuenta",
            S("Sentadillas y flexiones", M.Lift, P.None, P.None, 8f, F.Sweat, 2f), S("Estiramientos", M.Reach, P.None, P.None, 6f, F.None));
        _training["Sala de Yoga"] = new ActivityPerformer.Performance("ent_yoga", "Saludos al sol",
            S("Saludo al sol", M.Reach, P.None, P.None, 8f, F.None, 2f, Raise), S("Respirar", M.Teach, P.None, P.None, 6f, F.None, 2f, Raise));
        _training["Sala de Ballet"] = new ActivityPerformer.Performance("ent_ballet", "Barra de ballet",
            S("Pliés en la barra", M.Reach, P.None, P.None, 8f, F.Sweat, 3f), S("Port de bras", M.Teach, P.None, P.None, 6f, F.None, 2f, Raise));
        _training["Calistenia"] = new ActivityPerformer.Performance("ent_calistenia", "Calistenia",
            S("Dominadas y fondos", M.Lift, P.None, P.None, 8f, F.Sweat, 2f), S("Estiramientos", M.Reach, P.None, P.None, 6f, F.None));
        _training["Dojo (artes marciales)"] = new ActivityPerformer.Performance("ent_dojo", "Artes marciales",
            S("Golpes al saco", M.Spar, P.None, P.None, 9f, F.Sweat, 2f, Strike), S("Formas", M.Teach, P.None, P.None, 6f, F.None, 2f, Raise));

        // ══ 2. EL HORARIO LIBRE (las actividades que eran solo una postura) ══
        _leisure["lectura"] = new ActivityPerformer.Performance("ocio_lectura", "Leer en la arboleda",
            S("Leer", M.Read, P.Book, P.None, 14f, F.Paper, 4f, Fine, Sit), S("Beber un té", M.Talk, P.None, P.Cup, 6f, F.Steam, 2f, Fine, Sit));
        _leisure["textil"] = new ActivityPerformer.Performance("ocio_textil", "Manualidades",
            S("Bordar", M.Sew, P.Needle, P.Yarn, 12f, F.Thread, 1.3f, Fine, Sit), S("Tejer con agujas", M.Sew, P.Needle, P.Needle, 10f, F.Thread, 1.5f, Fine, Sit));
        _leisure["tertulia"] = new ActivityPerformer.Performance("ocio_tertulia", "Tertulia",
            S("Charlar", M.Talk, P.None, P.Cup, 10f, F.None, 2f, Fine, Sit), S("Beber", M.Talk, P.None, P.Cup, 4f, F.Steam, 1.5f, Fine, Sit));
        _leisure["huerto"] = new ActivityPerformer.Performance("ocio_huerto", "Cuidar el huerto",
            S("Regar", M.Water, P.WateringCan, P.None, 9f, F.Water, 0.8f, atFace: true), S("Mirar las plantas", M.Stoop, P.None, P.None, 7f, F.Leaves, 2f));
        _leisure["crias"] = new ActivityPerformer.Performance("ocio_crias", "Jugar con las crías",
            S("Acariciar", M.Play, P.None, P.None, 9f, F.Heart, 1.8f), S("Jugar con la pelota", M.Lift, P.None, P.None, 6f, F.Heart, 2.5f));
        // (micro) las que no tienen su gesto propio
        _leisure["escuela"] = new ActivityPerformer.Performance("ocio_escuela", "Leer y estudiar", S("Leer", M.Read, P.Book, P.None, 12f, F.Paper, 4f, Fine, Sit));
        _leisure["mercado"] = new ActivityPerformer.Performance("ocio_mercado", "Pasear por el mercado", S("Mirar los puestos", M.Inspect, P.Basket, P.None, 9f, F.None, 2f, Raise));
        _leisure["cuidar"] = new ActivityPerformer.Performance("ocio_cuidar", "Cuidar de la familia", S("Cuidar", M.Play, P.None, P.None, 10f, F.Heart, 2.5f));

        // ══ 3. LA COLA LARGA: el bucle genérico de cada clase de función (microcosmos y lo que no tiene pasos propios) ══
        void K(AreaFunctions.Kind k, params ActivityPerformer.Step[] st) => _kinds[k] = new ActivityPerformer.Performance("clase_" + k, AreaFunctions.Verb(k), st) { generic = true };
        K(AreaFunctions.Kind.Cook, S("Remover la olla", M.Stir, P.Ladle, P.None, 8f, F.Steam, 1f, Push, atFace: true), S("Picar", M.Chop, P.Knife, P.None, 6f, F.Leaves, 1.2f, Strike, atFace: true));
        K(AreaFunctions.Kind.Serve, S("Llevar los platos", M.Carry, P.Tray, P.None, 7f, F.Steam, 2.5f), S("Limpiar la mesa", M.Wipe, P.Cloth, P.None, 6f, F.Bubbles, 1.2f, atFace: true));
        K(AreaFunctions.Kind.Clean, Sweeping(), S("Fregar", M.Wipe, P.Cloth, P.Bucket, 7f, F.Bubbles, 1f, atFace: true));
        K(AreaFunctions.Kind.Dine, S("Comer", M.Talk, P.Spoon, P.Cup, 10f, F.Crumbs, 2f, Fine, Sit));   // (la comida de verdad la hace MealRitual)
        K(AreaFunctions.Kind.Heal, S("Atender al enfermo", M.Treat, P.Bandage, P.None, 8f, F.Heart, 2.5f, Fine, atFace: true), S("Preparar el remedio", M.Pipette, P.Spoon, P.Flask, 6f, F.Steam, 1.5f, Fine), Noting("Anotar"));
        K(AreaFunctions.Kind.Teach, S("Explicar", M.Teach, P.Chalk, P.Book, 9f, F.Chalk, 1.3f, Raise, atFace: true), S("Corregir", M.Inspect, P.None, P.Book, 6f, F.None, 2f, Raise));
        K(AreaFunctions.Kind.Practice, S("Practicar", M.Spar, P.None, P.None, 9f, F.Sweat, 2f, Strike), S("Repetir la forma", M.Teach, P.None, P.None, 6f, F.None, 2f, Raise));
        K(AreaFunctions.Kind.Meditate, S("Meditar", M.Pray, P.None, P.None, 20f, F.None, 3f, Still, Med));
        K(AreaFunctions.Kind.Tend, S("Cavar", M.Hammer, P.Hoe, P.None, 8f, F.Soil, 0.9f, Strike, atFace: true), S("Regar", M.Water, P.WateringCan, P.None, 7f, F.Water, 0.8f, atFace: true), S("Deshierbar", M.Stoop, P.None, P.None, 7f, F.Leaves, 1.2f, atFace: true));
        K(AreaFunctions.Kind.Build, S("Martillar", M.Hammer, P.Hammer, P.None, 8f, F.Dust, 0.7f, Strike, atFace: true), S("Acarrear", M.Carry, P.Box, P.None, 6f, F.Dust, 2.5f));
        K(AreaFunctions.Kind.Make, S("Trabajar la pieza", M.Reach, P.Wrench, P.None, 8f, F.Sparks, 1.3f, Strike, atFace: true), S("Golpear", M.Hammer, P.Hammer, P.None, 6f, F.Sparks, 0.7f, Strike, atFace: true));
        K(AreaFunctions.Kind.Operate, Console("Manejar los mandos"), Checking("Vigilar los indicadores", P.Tablet));
        K(AreaFunctions.Kind.Guard, S("Vigilar", M.Patrol, P.Flashlight, P.None, 10f, F.None, 2f, Raise));
        K(AreaFunctions.Kind.Study, S("Leer", M.Read, P.Book, P.None, 10f, F.Paper, 3f, Fine), S("Escribir", M.Write, P.Pen, P.Book, 7f, F.Paper, 2.5f, Fine));
        K(AreaFunctions.Kind.Care, S("Cuidar", M.Play, P.None, P.None, 9f, F.Heart, 2f), S("Dar de comer", M.Feed, P.Spoon, P.None, 6f, F.Heart, 2.5f));
        K(AreaFunctions.Kind.Trade, S("Despachar", M.Carry, P.Basket, P.None, 8f, F.None), S("Cobrar y charlar", M.Talk, P.None, P.None, 6f, F.None, 2f, Fine));
        K(AreaFunctions.Kind.Worship, S("Rezar", M.Pray, P.None, P.None, 14f, F.None, 3f, Still, Med));
        K(AreaFunctions.Kind.Rest, S("Descansar", M.Rest, P.None, P.None, 20f, F.None, 3f, Still));   // (de pie junto a su cama o su banco: sin silla no se sienta en el aire)
    }

    // ── Consultas ───────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Lo que hace quien tiene la misión <paramref name="id"/>: sus pasos propios; si no tiene, el bucle genérico de su
    /// clase (del registro de funciones); si tampoco, el de «trabajar en el taller».</summary>
    public static ActivityPerformer.Performance ForMission(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_missions.TryGetValue(id, out ActivityPerformer.Performance p)) return p;
        foreach (string pre in new[] { "obra_material_", "obra_trabajo_", "prep_" }) if (id.StartsWith(pre)) return _missions[pre];
        AreaFunctions.AreaFunction f = AreaFunctions.Get(id);
        return ForKind(f != null ? f.kind : AreaFunctions.Kind.Make);
    }

    /// <summary>¿Tiene la misión pasos propios (no el bucle genérico)?</summary>
    public static bool HasOwnSteps(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (_missions.ContainsKey(id)) return true;
        return id.StartsWith("obra_material_") || id.StartsWith("obra_trabajo_") || id.StartsWith("prep_");
    }

    /// <summary>El bucle genérico de una clase de función (herramienta en la mano, postura de trabajo y efecto periódico).</summary>
    public static ActivityPerformer.Performance ForKind(AreaFunctions.Kind k) => _kinds.TryGetValue(k, out ActivityPerformer.Performance p) ? p : _kinds[AreaFunctions.Kind.Make];

    /// <summary>Lo que se hace en una actividad del horario libre (null = ya tiene su postura o su sistema: yoga, baile, pesas…).</summary>
    public static ActivityPerformer.Performance ForLeisure(string activityId) => activityId != null && _leisure.TryGetValue(activityId, out ActivityPerformer.Performance p) ? p : null;

    /// <summary>El entrenamiento de las 14 h en un área sin sistema propio (ballet, calistenia, dojo). null = no hay.</summary>
    public static ActivityPerformer.Performance ForTraining(string area) => area != null && _training.TryGetValue(area, out ActivityPerformer.Performance p) ? p : null;

    /// <summary>Las misiones con pasos propios (el doc y la prueba).</summary>
    public static IEnumerable<string> OwnMissions => _missions.Keys;

    // ── La brigada del restaurante (RestaurantService): un gesto con su utensilio por papel y paso ──
    static readonly Dictionary<string, ActivityPerformer.Performance> _service = new Dictionary<string, ActivityPerformer.Performance>();
    static ActivityPerformer.Performance One(string id, ActivityPerformer.Step s)
    {
        if (!_service.TryGetValue(id, out ActivityPerformer.Performance p)) _service[id] = p = new ActivityPerformer.Performance(id, s.label, s) { quiet = true };
        return p;
    }

    /// <summary>Lo que hace con las manos un miembro de la brigada del restaurante en el paso <paramref name="step"/> de su papel.</summary>
    public static ActivityPerformer.Performance ForService(RestaurantService.Role role, int step)
    {
        switch (role)
        {
            case RestaurantService.Role.Cocinero:
                return step == 0 ? One("srv_coc_0", S("Sacar el género de la nevera", M.Reach, P.None, P.None, 30f, F.Steam, 2.5f, atFace: true))
                     : step == 1 ? One("srv_coc_1", S("Cocinar en el equipo del plato", M.Stir, P.Ladle, P.None, 30f, F.Steam, 0.9f, atFace: true))
                     : One("srv_coc_2", S("Emplatar", M.Reach, P.Spoon, P.None, 30f, F.None));
            case RestaurantService.Role.Obrador: return One("srv_obr", S("Amasar y elaborar", M.Reach, P.None, P.None, 30f, F.Dust, 1.2f, atFace: true));
            case RestaurantService.Role.LimpiezaCocina: return One("srv_lim_coc", S("Fregar el puesto", M.Wipe, P.Sponge, P.None, 30f, F.Bubbles, 0.9f, atFace: true));
            case RestaurantService.Role.LimpiezaSala: return One("srv_lim_sala", S("Limpiar la mesa", M.Wipe, P.Cloth, P.Spray, 30f, F.Bubbles, 1f, atFace: true));
            default: return One("srv_sala", S("Atender", M.Write, P.Pen, P.Clipboard, 30f, F.None));
        }
    }
}
