using System.Collections.Generic;
using static ChoreKit;

/// <summary>
/// LAS MISIONES DE LOS SITIOS NUEVOS DE HAWTHORNE (lote 2; docs/microcosmos/microcosmos-musk-historia.md §4; Berón 2026-11-18: «quienes crean las naves son
/// los trabajadores, los científicos … la producción de alimentos o la seguridad urbana … bacterias»). Como las demás, procedimientos reales con
/// su fuente, un reto por paso (<see cref="ChoreKit.StepChain"/>) o retos de ordenar y elegir. Los sitios, por su nombre exacto, en
/// <see cref="MissionSites"/>.
/// </summary>
public static partial class MissionLibrary
{
    const string TWI = "TWI (The Welding Institute), «What is Friction Stir Welding?»: un pasador giratorio calienta por fricción y remueve el metal en estado sólido, sin fundirlo; The Fabricator (sept. y nov. de 2008): SpaceX suelda los tanques de aluminio-litio del Falcon 9 por FSW con cabezal de pasador retráctil";
    const string ASTM_E8 = "ASTM E8/E8M, «Standard Test Methods for Tension Testing of Metallic Materials» (resistencia = fuerza máxima ÷ sección inicial); aleación aluminio-litio 2195: resistencia a la tracción ≈ 590 MPa (NASA, depósito externo superligero)";
    const string ASTM_E3 = "ASTM E3, «Standard Guide for Preparation of Metallographic Specimens» (cortar, embutir, desbastar, pulir) y ASTM E407 (ataque químico: reactivo de Keller para el aluminio)";
    const string TEST_DATA = "Práctica de ensayo de motores (lectura de presión de cámara frente al tiempo); los Merlin se probaban en McGregor, Texas. Valores del reto: de juego";
    const string IEST = "IEST-RP-CC003, «Garment System Considerations for Cleanrooms»: vestirse de arriba abajo, con alfombra adhesiva y banco de paso; ISO 14644-1 (clase ISO 8)";
    const string WHO_RUB = "OMS, «How to Handrub?» (2009): gel en la palma ahuecada, palmas, dorsos, entre los dedos, pulgares y yemas; 20–30 s hasta que se seque";
    const string FDA = "FDA Food Code (2009): frío a 5 °C (41 °F) o menos (3-501.16), fecha y 7 días como mucho para lo listo para comer (3-501.17), recibir lo frío a 5 °C o menos y rechazar latas hinchadas (3-202.11, 3-202.15)";
    const string LACOUNTY = "Departamento de Salud Pública del Condado de Los Ángeles: la letra de inspección de restaurantes (desde 1998): A = 90–100 puntos, B = 80–89, C = 70–79";
    const string OSHA_EXT = "OSHA, eTool «Portable Fire Extinguishers»: el método PASS (tira de la anilla, apunta a la base, aprieta la maneta, barre de lado a lado)";
    const string CVC_HYDRANT = "Código de Vehículos de California §22514: no se aparca a menos de 15 pies (4,6 m) de una boca de incendios";
    const string CVC_PED = "Código de Vehículos de California §21456: con la mano roja intermitente no se empieza a cruzar; quien ya está cruzando termina";
    const string GATE = "Prácticas de control de acceso (ASIS International, «Physical Security Principles»): tarjeta, cara con la foto, registro de visitas. En la garita de SpaceX en 2008: supuesto";
    const string FAA = "FAA, Aeronautical Information Manual 2-3-3: el número de la pista es su rumbo magnético entre diez, redondeado; aeropuerto de Hawthorne (Jack Northrop Field), pista 07/25, 1489 × 30 m";

    static void Hawthorne()
    {
        // ── LOS TRABAJADORES: la soldadura por fricción de los tanques del Falcon 9 ──
        var fsw = new List<CodeExercise>(StepChain("fsw", 1, i => $"FSW: paso {i + 1}",
            new[] { "sujeta las dos chapas de aluminio-litio en el utillaje", "comprueba que las chapas casan sin hueco", "baja el pasador giratorio hasta que el hombro toque la chapa", "deja que la fricción caliente el metal sin fundirlo", "avanza el cabezal a lo largo de la junta", "retrae el pasador al final para no dejar agujero", "inspecciona la soldadura con ultrasonidos" },
            new[] { "funde el borde con el soplete", "enfría la junta con agua mientras avanza" }));
        fsw.Add(Choice("fsw-solido", 3, CodeSkill.Especificar, "Sólido, no líquido", "¿Qué le pasa al metal en la soldadura por fricción?", new CodeBlock[0],
            "Se ablanda y se remueve sin llegar a fundirse", new[] { "Se funde y se mezcla como en la soldadura de arco", "Se pega con un adhesivo caliente" }));
        Add("fsw-tanque-falcon9", "soldar-fsw", ChoreCategory.Taller, "Soldar por fricción un anillo del tanque del Falcon 9", "La primera soldadura larga de un barril de aluminio-litio", TWI, fsw.ToArray());

        // ── LOS CIENTÍFICOS: el laboratorio de materiales y la sala de datos ──
        Add("ensayo-traccion", "ensayo-materiales", ChoreCategory.Ciencia, "Ensayo de tracción de una probeta de la soldadura", "¿Aguanta la junta FSW lo que el metal de al lado?", ASTM_E8, new[]
        {
            Order("traccion-pasos", 1, "Tirar hasta romper, midiendo", "mide la anchura y el espesor de la probeta", "sujétala en las mordazas", "pon el extensómetro", "tira a velocidad constante", "registra la fuerza y el alargamiento hasta que rompa"),
            Choice("traccion-resistencia", 2, CodeSkill.Predecir, "Resistencia = fuerza ÷ sección", "¿Qué resistencia sale?",
                new[] { Block("a", "fuerzaMax = 29 500  // N", "const fuerzaMax = 29500;"), Block("b", "seccion = 50       // mm²", "const seccion = 50;"), Block("c", "resistencia = fuerzaMax ÷ seccion", "const resistencia = fuerzaMax / seccion; // MPa") },
                "590 MPa", new[] { "1475 MPa", "29 MPa", "5,9 MPa" }),
        });
        Add("microscopio-soldadura", "ensayo-materiales", ChoreCategory.Ciencia, "Mirar el grano de la soldadura al microscopio", "La zona removida por el pasador tiene el grano más fino", ASTM_E3,
            StepChain("microscopio", 1, i => $"Metalografía: paso {i + 1}",
                new[] { "corta una muestra a través de la soldadura", "embútela en resina", "desbasta con lija cada vez más fina", "pule con pasta de diamante", "ataca la superficie con reactivo de Keller", "mira el grano al microscopio" },
                new[] { "mira la muestra sin pulir", "lija con el grano más grueso al final" }));
        Add("datos-prueba-merlin", "analizar-datos", ChoreCategory.Ciencia, "Leer la prueba de encendido del Merlin", "Las gráficas que llegan de McGregor, Texas", TEST_DATA, new[]
        {
            Order("datos-pasos", 1, "Los datos cuentan lo que pasó", "carga los datos de la prueba", "quita las muestras de antes del encendido", "dibuja la presión de cámara frente al tiempo", "marca el apagado", "compara con la prueba anterior", "apunta lo raro en el informe"),
            Choice("datos-media", 3, CodeSkill.Predecir, "Una media es un bucle que suma", "¿Qué valor muestra?",
                new[] { Block("a", "suma = 0", "let suma = 0;"), Block("b", "para cada p en [48, 50, 52] {", "for (const p of [48, 50, 52]) {"), Block("c", "suma = suma + p", "suma += p;", 1), Block("d", "}", "}"), Block("e", "mostrar suma ÷ 3", "mostrar(suma / 3);") },
                "50", new[] { "150", "52", "16" }),
        }, "licencia");

        // ── LAS BACTERIAS: la sala limpia y el gel ──
        var gown = new List<CodeExercise>(StepChain("sala-limpia", 1, i => $"Vestirse: paso {i + 1}",
            new[] { "lávate las manos", "pisa la alfombra adhesiva", "ponte el gorro", "ponte la mascarilla", "ponte el buzo sin que toque el suelo", "ponte las calzas al cruzar el banco", "ponte los guantes y frótalos con alcohol" },
            new[] { "entra con la ropa de la calle", "ponte las calzas antes del gorro, sentado en el suelo" }));
        gown.Add(Choice("sala-limpia-orden", 1, CodeSkill.Especificar, "De arriba abajo", "¿Por qué uno se viste de la cabeza a los pies?", new CodeBlock[0],
            "Para que lo de arriba no suelte partículas sobre lo ya puesto", new[] { "Porque es más rápido", "Da igual el orden" }));
        Add("sala-limpia-vestirse", "sala-limpia", ChoreCategory.Salud, "Vestirse para la sala limpia de aviónica", "Polvo, pelo y bacterias fuera de las placas de vuelo (ISO 8)", IEST, gown.ToArray());
        Add("gel-manos", "asearse", ChoreCategory.Salud, "Desinfectarse las manos con gel", "En la entrada, el comedor y el vestuario", WHO_RUB,
            StepChain("gel", 1, i => $"Gel: paso {i + 1}",
                new[] { "pon gel en la palma ahuecada", "frota las palmas entre sí", "frota el dorso de cada mano", "frota entre los dedos", "frota los pulgares girándolos", "frota las yemas contra la palma", "frota hasta que se seque, unos {20|5|90} segundos" },
                new[] { "aclara el gel con agua", "sécate con la ropa" }));

        // ── LA COMIDA: la cámara, la letra de inspección, el proveedor ──
        Add("camara-frigorifica", "seguridad-alimentaria", ChoreCategory.Comida, "Revisar la cámara frigorífica", "Lo que come el turno de noche, a salvo", FDA,
            StepChain("camara", 1, i => $"Cámara: paso {i + 1}",
                new[] { "mira el termómetro: a {5|12|20} °C como mucho", "lo crudo abajo y lo cocinado arriba", "etiqueta lo abierto con la fecha", "tira lo listo para comer de más de {7|30|2} días", "lo más viejo delante", "cierra bien la puerta" },
                new[] { "deja la puerta abierta mientras ordenas", "pon la carne cruda encima de la ensalada" }));
        Add("inspeccion-sanitaria", "seguridad-alimentaria", ChoreCategory.Comida, "La inspección sanitaria de la cocina", "La tarjeta de la puerta: la letra que se ganó la cocina", LACOUNTY, new[]
        {
            Choice("inspeccion-letra", 5, CodeSkill.Predecir, "La letra sale de los puntos", "La cocina saca 92 puntos. ¿Qué letra cuelga en la puerta?",
                new[] { Block("a", "si puntos >= 90 entonces letra = \"A\"", "if (puntos >= 90) letra = 'A';"), Block("b", "si no, si puntos >= 80 entonces letra = \"B\"", "else if (puntos >= 80) letra = 'B';"), Block("c", "si no, si puntos >= 70 entonces letra = \"C\"", "else if (puntos >= 70) letra = 'C';") },
                "A", new[] { "B", "C" }),
            Order("inspeccion-pasos", 1, "Antes de que llegue el inspector", "lávate las manos en el lavamanos de la cocina", "comprueba la temperatura de la cámara", "limpia y desinfecta las superficies", "tapa y etiqueta lo guardado", "cuelga la tarjeta a la vista"),
        });
        Add("recibir-proveedor", "recepcion", ChoreCategory.Comida, "Recibir la furgoneta del proveedor", "Las cajas de comida en el muelle de carga", FDA,
            StepChain("proveedor", 1, i => $"Recibir: paso {i + 1}",
                new[] { "comprueba que la furgoneta es frigorífica", "mide lo frío: a {5|15|25} °C como mucho", "rechaza las latas hinchadas o abolladas", "comprueba fechas y etiquetas", "guarda lo frío primero", "firma el albarán" },
                new[] { "deja las cajas al sol en el muelle", "acepta la carne tibia si es barata" }));

        // ── LA SEGURIDAD URBANA: la garita, los incendios y el cruce ──
        Add("garita-tarjeta", "control-acceso", ChoreCategory.Seguridad, "Control de acceso en la garita", "1 Rocket Road: quién entra a la fábrica", GATE, new[]
        {
            Order("garita-pasos", 1, "Nadie entra sin ser quien dice", "saluda y pide la tarjeta", "pásala por el lector", "compara la foto con la cara", "a una visita, pídele el documento y llama a quien visita", "dale el pase de visitante y apunta la hora", "levanta la barrera"),
            Choice("garita-predecir", 5, CodeSkill.Predecir, "Si algo no cuadra, se pregunta", "La tarjeta es válida pero la foto no es de quien la lleva. ¿Qué hace el programa?",
                new[] { Block("a", "si tarjeta.valida y foto == cara entonces", "if (tarjeta.valida && foto === cara) {"), Block("b", "abrir la barrera", "abrir();", 1), Block("c", "si no", "} else {"), Block("d", "llamar al responsable", "llamar(responsable);", 1), Block("e", "fin", "}") },
                "Llama al responsable", new[] { "Abre la barrera", "No hace nada" }),
        }, "licencia");
        Add("incendios-extintor", "incendios", ChoreCategory.Seguridad, "Apagar un conato con el extintor", "Simulacro con los bomberos de la estación 162 del Condado (Crenshaw)", OSHA_EXT,
            StepChain("extintor", 1, i => $"Extintor: paso {i + 1}",
                new[] { "da la alarma y llama al 911", "coge el extintor", "tira de la anilla", "apunta a la base del fuego", "aprieta la maneta", "barre de lado a lado", "si no se apaga, sal y cierra la puerta" },
                new[] { "apunta a las llamas de arriba", "echa agua a un fuego eléctrico" }));
        Add("incendios-hidrante", "incendios", ChoreCategory.Seguridad, "El hidrante, siempre libre", "Los bomberos conectan aquí la manguera", CVC_HYDRANT, new[]
        {
            Choice("hidrante-distancia", 4, CodeSkill.Completar, "Un coche aparcado retrasa a los bomberos", "¿Qué distancia falta?",
                new[] { Block("a", "si distanciaAlHidrante < ___ entonces", "if (distanciaAlHidrante < ___) {"), Block("b", "no aparcar aquí", "buscarOtroSitio();", 1), Block("c", "fin", "}") },
                "15 pies (4,6 m)", new[] { "1 pie (0,3 m)", "100 pies (30 m)" }),
            Order("hidrante-pasos", 1, "Que se vea y se alcance", "mira si hay un hidrante en la acera", "deja 15 pies libres a cada lado", "no lo tapes con cajas ni contenedores", "avisa si gotea o está roto"),
        });
        Add("cruzar-crenshaw", "seguridad-vial", ChoreCategory.Seguridad, "Cruzar Crenshaw Boulevard", "Seis carriles entre Rocket Road y la calle 120", CVC_PED, new[]
        {
            Order("cruce-pasos", 1, "Seis carriles: sin prisa y por el paso", "espera en la acera", "pulsa el botón del semáforo", "espera al muñeco blanco", "mira a izquierda y derecha", "cruza por el paso sin correr"),
            Choice("cruce-mano", 5, CodeSkill.Predecir, "La mano roja intermitente", "Aún no has empezado a cruzar y la mano roja parpadea. ¿Qué haces?",
                new[] { Block("a", "si señal == \"mano intermitente\" y no cruzando entonces", "if (senal === 'manoIntermitente' && !cruzando) {"), Block("b", "esperar al siguiente verde", "esperar();", 1), Block("c", "si no, si señal == \"muñeco\" entonces", "} else if (senal === 'muneco') {"), Block("d", "cruzar", "cruzar();", 1), Block("e", "fin", "}") },
                "Esperas al siguiente verde", new[] { "Cruzas corriendo", "Cruzas despacio" }),
        });

        // ── El aeropuerto, de fondo: se lee desde el aparcamiento (su pista se ve, pero no se pisa) ──
        Add("pista-hawthorne", "aeronautica", ChoreCategory.Ideas, "Leer la pista del aeropuerto de Hawthorne", "Desde el aparcamiento se ve la pista 07/25 de Jack Northrop Field", FAA, new[]
        {
            Choice("pista-rumbo", 2, CodeSkill.Predecir, "El número de la pista es su rumbo", "¿Hacia qué rumbo despega un avión por la pista 07?",
                new[] { Block("a", "numero = 7", "const numero = 7;"), Block("b", "rumbo = numero × 10  // grados", "const rumbo = numero * 10;") },
                "70°", new[] { "7°", "250°" }),
            Choice("pista-contraria", 2, CodeSkill.Predecir, "Cada pista tiene dos cabeceras", "Si un extremo es la 07, ¿cuál es el otro?",
                new[] { Block("a", "otro = (7 + 18) si es ≤ 36", "const otro = 7 + 18;") },
                "25", new[] { "07", "36" }),
        });
    }
}
