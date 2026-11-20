using System.Collections.Generic;
using static ChoreKit;

/// <summary>
/// LAS MISIONES DE VÍNCULO DE HAWTHORNE (2026-11-19, PB-166; Berón: «en el microcosmos todo se hará por retos, incluso los diálogos, de modo que
/// podrían haber misiones de entablar amistad con personajes o ser sus estudiantes, o combatir a su lado, o protegerles … los retos de diálogo
/// sirven para enseñar la historia real del escenario»). Cada misión es una conversación con un personaje: cada turno es un reto de diálogo
/// (<see cref="ChoreKit.Say"/>, la respuesta buena es la que encaja con lo que pasó de verdad) y en medio, los retos de código de lo que se
/// aprende con él. Al acabar suma en la marga de Vínculos. Los sitios (el personaje, por su nombre) en <see cref="MissionSites"/>.
/// Las cuatro clases: «entablar-amistad», «ser-estudiante», «combatir-al-lado» y «proteger».
/// </summary>
public static partial class MissionLibrary
{
    const string F1_HIST = "Koenigsmann, H. et al. (2009), «Falcon 1 Launch Vehicle: Demo Flights, Status, Manifest and Upgrade Path», SmallSat Conference; Spaceflight Now, «Falcon 1 flight 1», «flight 2», «flight 3» y «flight 4» (2006–2008); SpaceX, actualizaciones de los vuelos 1 (19-7-2006) y 3 (agosto de 2008)";
    const string MERLIN = "Wikipedia, «Merlin (rocket engine family)» y «Kestrel (rocket engine)»: el Merlin 1A (vuelos 1 y 2) se enfriaba por ablación; el Merlin 1C, el primero de refrigeración regenerativa, voló en el vuelo 3; queroseno RP-1 y oxígeno líquido; inyector de aguja (pintle). Sutton y Biblarz, «Rocket Propulsion Elements» (refrigeración regenerativa)";
    const string OMELEK = "Spaceflight Now, «Falcon 1 flight 2» (21-3-2007: el chapoteo del propergol en la segunda etapa la hizo oscilar y apagarse antes de tiempo); Reagan Test Site, atolón de Kwajalein (Islas Marshall), unos 9° N; NASA, «Launch window» y ganancia de velocidad por la rotación terrestre hacia el este (≈ 465 m/s en el ecuador)";
    const string SHOTWELL = "NASA, comunicado del 23-12-2008: contratos de Servicios Comerciales de Reabastecimiento (CRS), SpaceX 12 vuelos por unos 1600 millones de dólares; Wikipedia, «Gwynne Shotwell» (en SpaceX desde 2002, vicepresidenta de desarrollo de negocio)";
    const string HAWTHORNE_CITY = "Wikipedia, «Hawthorne, California» (ciudad desde 1922; Northrop Aircraft se fundó allí en 1939; los Beach Boys crecieron en Hawthorne) y «SpaceX headquarters» (la antigua planta de Northrop donde se fabricaban secciones del fuselaje del Boeing 747; SpaceX se mudó en 2007–2008)";
    const string FIRE = "NFPA 10, «Standard for Portable Fire Extinguishers» (fuego de clase B: líquidos inflamables; espuma, polvo o CO₂, nunca chorro de agua); OSHA, eTool «Portable Fire Extinguishers» (método PASS); NASA, «Safety Standard for Oxygen and Oxygen Systems» (NSS 1740.15): el oxígeno líquido hace arder con fuerza lo que normalmente no arde";
    const string FATIGUE = "Watson et al. (2015), consenso AASM/SRS: 7 horas o más de sueño por noche; Williamson y Feyer (2000), Occup. Environ. Med. 57:649 (17–19 h despierto rinden como 0,05 % de alcohol en sangre); Satellite Today (24-11-2008), «SpaceX facilities open 24 hours to deliver on contracts»; siete semanas entre los vuelos 3 y 4 (Spaceflight Now)";

    static void Vinculos()
    {
        // ── ENTABLAR AMISTAD: la ingeniera de propulsión (Medea) y su motor ─────────────────────────────────────────────────────────────
        Add("amistad-ingeniera-propulsion", "entablar-amistad", ChoreCategory.Vinculos, "Hacerte amigo de la ingeniera de propulsión",
            "Agosto de 2008, en la nave de Hawthorne. Medea lleva días mirando las curvas del vuelo 3. Para ganarte su confianza, escúchala y demuéstrale que entiendes lo que ha pasado.",
            F1_HIST + "; " + MERLIN, new[]
        {
            Say("amistad-medea-1", "Ingeniera de propulsión", "¿Eres nuevo? Aquí llevamos semanas durmiendo poco. ¿Sabes por qué estamos todos así?",
                "Por el vuelo 3: el 2 de agosto la primera etapa chocó con la segunda justo después de separarse",
                new[] { "Porque el cohete explotó en la plataforma de Cabo Cañaveral", "Porque la NASA nos ha cancelado un contrato" },
                "El vuelo 3 despegó de Omelek el 2 de agosto de 2008. Tras el apagado del motor, el empuje que le quedaba empujó la primera etapa contra la segunda."),
            Say("amistad-medea-2", "Ingeniera de propulsión", "Mi motor, el Merlin 1C, voló por primera vez en ese vuelo. ¿Sabes qué tenía de nuevo?",
                "Se enfría con su propio queroseno, que corre por canales en la pared antes de quemarse",
                new[] { "Funciona con hidrógeno líquido, como el del transbordador", "Es un motor de combustible sólido, como un cohete de feria" },
                "Es la refrigeración regenerativa: el queroseno RP-1 se calienta enfriando la cámara y la tobera, y luego se quema con el oxígeno líquido. Los vuelos 1 y 2 usaron el Merlin 1A, que se enfriaba quemando poco a poco una capa de su pared (ablación)."),
            Order("amistad-medea-camino", 1, "El camino del combustible es una secuencia: cada tramo, después del anterior",
                "la turbobomba sube la presión del queroseno", "el queroseno corre por los canales de la pared de la cámara y la tobera", "llega al inyector ya caliente", "se mezcla con el oxígeno líquido y arde en la cámara"),
            Say("amistad-medea-3", "Ingeniera de propulsión", "El primer Falcon 1, en marzo de 2006, voló poco más de medio minuto. ¿Sabes qué lo tumbó?",
                "Una tuerca de aluminio, corroída por el aire salado de la isla, dejó escapar combustible y prendió un incendio",
                new[] { "Le cayó un rayo durante el despegue", "Se quedó sin combustible porque lo cargaron a medias" },
                "La investigación de SpaceX culpó a una tuerca de aluminio de una línea de combustible, corroída por el aire marino de Omelek. Desde entonces se cambiaron esas piezas por otras de acero inoxidable."),
            Say("amistad-medea-4", "Ingeniera de propulsión", "Tres vuelos y tres fallos. Hay quien dice que deberíamos dejarlo. ¿Tú qué piensas?",
                "Cada fallo fue distinto y lo entendisteis: el siguiente cohete ya lleva el arreglo",
                new[] { "Que tienen razón: si ha fallado tres veces, fallará siempre", "Que da igual, que lo importante es que no se entere nadie" },
                "Eso pensaba el equipo: el vuelo 1 fue una fuga, el 2 un chapoteo del propergol y el 3 el empuje residual. Siete semanas después, el vuelo 4 llegó a órbita."),
            Say("amistad-medea-5", "Ingeniera de propulsión", "Gracias por escucharme. ¿Bajamos a comer a la taquería de Crenshaw cuando acabe el turno?",
                "Claro, y me cuentas cómo llegaste a SpaceX",
                new[] { "No, comer con alguien es perder el tiempo", "Solo si me haces tú el trabajo de mañana" },
                "Una amistad se hace compartiendo tiempo y escuchando. En 2008 la nave tenía cocina propia, pero el barrio de Crenshaw Boulevard estaba a un paseo."),
        }, "licencia");

        // ── SER ESTUDIANTE: el director de lanzamiento (Héspero) enseña la cuenta atrás ─────────────────────────────────────────────────
        Add("estudiante-director-lanzamiento", "ser-estudiante", ChoreCategory.Vinculos, "Ser estudiante del director de lanzamiento",
            "Septiembre de 2008. Héspero dirigirá la cuenta atrás del vuelo 4 desde la sala de control. Acepta enseñarte si le demuestras que sabes escuchar y razonar.",
            OMELEK + "; " + F1_HIST, new[]
        {
            Say("estudiante-hespero-1", "Director de lanzamiento", "Antes de enseñarte nada: ¿desde dónde lanzamos el Falcon 1?",
                "Desde la isla de Omelek, en el atolón de Kwajalein, en las Islas Marshall",
                new[] { "Desde Cabo Cañaveral, en Florida", "Desde la pista del aeropuerto de Hawthorne" },
                "Omelek es una isla diminuta del polígono de pruebas Reagan, en medio del Pacífico. Hasta allí se llevaba cada cohete por mar o en avión de carga."),
            Say("estudiante-hespero-2", "Director de lanzamiento", "¿Y por qué tan lejos, a solo nueve grados del ecuador?",
                "Cerca del ecuador la Tierra gira más deprisa y le regala al cohete velocidad hacia el este",
                new[] { "Porque allí la gravedad desaparece", "Porque allí no hay aire y el cohete no roza" },
                "En el ecuador el suelo ya va a unos 465 m/s hacia el este. Un cohete pequeño como el Falcon 1 aprovecha cada metro por segundo."),
            Say("estudiante-hespero-3", "Director de lanzamiento", "En el vuelo 2, en marzo de 2007, llegamos al espacio, pero la segunda etapa se apagó antes de tiempo. ¿Recuerdas por qué?",
                "El propergol chapoteaba en el tanque, la etapa empezó a oscilar y el motor se quedó sin alimentación",
                new[] { "Chocó con un satélite que pasaba", "Se rompió la cofia y entró aire" },
                "El chapoteo (slosh) del oxígeno líquido hizo bambolearse la segunda etapa. Para el vuelo 3 se añadieron deflectores en el tanque y se cambió el control."),
            OrderWithDistractor("estudiante-hespero-cuenta", 1, "Una cuenta atrás es un programa: cada paso, solo cuando el anterior está bien",
                "soltar los anclajes antes de encender el motor",
                "carga el queroseno", "carga el oxígeno líquido", "pregunta «¿go o no go?» a cada puesto", "el ordenador de vuelo toma la cuenta final", "enciende el Merlin con el cohete sujeto", "si el empuje es bueno, suelta los anclajes"),
            Choice("estudiante-hespero-aborto", 4, CodeSkill.Predecir, "Una condición decide qué camino se toma", "El motor da el 92 % del empuje. ¿Qué hace el programa?",
                new[] { Block("a", "si empuje >= 100 {", "if (empuje >= 100) {"), Block("b", "soltar(anclajes)", "anclajes.soltar();", 1), Block("c", "} si no {", "} else {"), Block("d", "apagar(merlin)", "merlin.apagar();", 1), Block("e", "esperar(nuevoIntento)", "await nuevoIntento();", 1), Block("f", "}", "}") },
                "Apaga el Merlin y espera otro intento, con el cohete aún sujeto", new[] { "Suelta los anclajes y despega", "Se queda encendido para siempre" }),
            Say("estudiante-hespero-4", "Director de lanzamiento", "Bien. El 28 de septiembre el vuelo 4 llegó a órbita. ¿Sabes qué llevaba?",
                "Una carga de prueba de 165 kg, RatSat: nadie quería arriesgar un satélite de verdad",
                new[] { "El primer astronauta de SpaceX", "Un telescopio de la NASA de varias toneladas" },
                "Fue el primer cohete de combustible líquido hecho con dinero privado que llegó a órbita. En julio de 2009 el vuelo 5 puso en órbita el satélite malasio RazakSAT."),
        }, "licencia");

        // ── COMBATIR A SU LADO: un conato de incendio de queroseno con la bombera de la estación 162 ─────────────────────────────────
        var fight = new List<CodeExercise>
        {
            Say("combatir-bombera-1", "Bombera de la estación 162", "¡Se ha derramado queroseno junto al banco de pruebas y ha prendido! ¿Qué clase de fuego es?",
                "Clase B: un líquido inflamable",
                new[] { "Clase A: madera y papel", "Clase C: un gas" },
                "En la norma NFPA de EE. UU., la clase B son los líquidos inflamables (gasolina, queroseno, aceites)."),
            Say("combatir-bombera-2", "Bombera de la estación 162", "Tengo la manguera aquí mismo. ¿Le echo el chorro de agua?",
                "No: el agua salpica el queroseno ardiendo y lo extiende. Mejor el extintor de espuma o de polvo",
                new[] { "Sí, el agua apaga cualquier fuego", "Sí, y cuanto más fuerte el chorro, mejor" },
                "El queroseno flota sobre el agua: un chorro lo esparce encendido. La espuma lo cubre y lo ahoga; el polvo corta la reacción."),
            Order("combatir-pass", 1, "Un procedimiento de emergencia es una secuencia corta que se ensaya",
                "tira de la anilla", "apunta a la base de las llamas", "aprieta la maneta", "barre de lado a lado hasta apagarlo"),
            Say("combatir-bombera-3", "Bombera de la estación 162", "Al lado está el tanque de oxígeno líquido del banco. ¿Por qué me preocupa tanto?",
                "Porque con oxígeno puro casi todo arde mucho más fuerte, hasta la ropa o el asfalto",
                new[] { "Porque el oxígeno líquido apaga el fuego y nos quitaría el trabajo", "No preocupa: el oxígeno no tiene nada que ver con el fuego" },
                "El fuego necesita combustible, calor y oxígeno (el triángulo del fuego). Una atmósfera rica en oxígeno hace arder lo que normalmente no arde: por eso los bancos de pruebas separan el oxígeno del combustible."),
            Say("combatir-bombera-4", "Bombera de la estación 162", "Apagado. Buen trabajo, compañero. ¿Sabes que el primer Falcon 1 se perdió por un fuego parecido?",
                "Sí: en 2006 una fuga de combustible por una tuerca corroída prendió junto al motor",
                new[] { "No puede ser: los cohetes no se incendian", "Sí: lo apagaron con agua y siguió volando" },
                "El vuelo 1 (24-3-2006) perdió el motor por un incendio a los pocos segundos del despegue. Desde entonces, vigilar las fugas es parte de cada cuenta atrás."),
        };
        Add("combatir-incendio-bombera", "combatir-al-lado", ChoreCategory.Vinculos, "Combatir un conato de incendio junto a la bombera",
            "Un banco de pruebas de Hawthorne (supuesto: los Merlin se probaban en McGregor, Texas). Se ha derramado queroseno y ha prendido. La bombera de la estación 162 del Condado está aquí: lucha a su lado.",
            FIRE + "; " + F1_HIST, fight.ToArray(), "licencia");

        // ── PROTEGER: al técnico de la nave (Atlas), que no duerme ──────────────────────────────────────────────────────────────────
        Add("proteger-tecnico-cansancio", "proteger", ChoreCategory.Vinculos, "Proteger al técnico de la nave del agotamiento",
            "Septiembre de 2008. La nave trabaja día y noche para el vuelo 4. Atlas lleva casi veinte horas despierto y quiere seguir apretando tornillos de la interetapa.",
            FATIGUE + "; " + HAWTHORNE_CITY, new[]
        {
            Say("proteger-atlas-1", "Técnico de la nave", "Solo me quedan unos tornillos. Llevo diecinueve horas despierto, pero estoy bien, ¿no?",
                "No del todo: tras 17–19 horas despierto se rinde como con alcohol en la sangre, y esos tornillos son de la interetapa",
                new[] { "Sí: el cansancio no afecta a las manos", "Sí: si tomas otro café es como haber dormido" },
                "Un estudio de 2000 midió que 17–19 horas sin dormir equivalen a 0,05 % de alcohol en sangre. En una pieza de vuelo, un error así cuesta un cohete."),
            Choice("proteger-atlas-turno", 4, CodeSkill.Predecir, "Una condición protege a quien no se da cuenta", "Atlas lleva 19 horas despierto. ¿Qué dice el programa?",
                new[] { Block("a", "horasDespierto = 19", "const horasDespierto = 19;"), Block("b", "si horasDespierto > 16 {", "if (horasDespierto > 16) {"), Block("c", "mostrar «relevo y a dormir»", "mostrar('relevo y a dormir');", 1), Block("d", "} si no {", "} else {"), Block("e", "mostrar «sigue»", "mostrar('sigue');", 1), Block("f", "}", "}") },
                "relevo y a dormir", new[] { "sigue", "no muestra nada" }),
            Say("proteger-atlas-2", "Técnico de la nave", "¿Y quién acaba los tornillos? La nave ya está abierta día y noche.",
                "El relevo del turno de noche: por eso hay turnos, para que nadie trabaje agotado",
                new[] { "Nadie: el vuelo puede esperar un año", "Tú mismo, mañana, sin dormir otra vez" },
                "En noviembre de 2008 la prensa contaba que las instalaciones de SpaceX abrían 24 horas para cumplir sus contratos, con turnos de día, tarde y noche."),
            Say("proteger-atlas-3", "Técnico de la nave", "Vale, me voy a casa. ¿Sabes qué se fabricaba antes en esta nave?",
                "Secciones del fuselaje del Boeing 747: era una planta de Northrop",
                new[] { "Coches de carreras", "Nada: es una nave nueva, de 2008" },
                "Hawthorne es ciudad de aviones: Northrop Aircraft nació allí en 1939, y su antigua planta junto al aeropuerto es hoy la sede de SpaceX."),
            Say("proteger-atlas-4", "Técnico de la nave", "¿Cuántas horas debería dormir para volver mañana entero?",
                "Siete o más",
                new[] { "Con tres basta si eres fuerte", "Doce, o no sirve de nada" },
                "El consenso de la Academia Americana de Medicina del Sueño (2015): un adulto necesita 7 horas o más por noche."),
        }, "licencia");
    }
}
