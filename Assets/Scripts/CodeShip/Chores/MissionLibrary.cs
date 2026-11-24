using System;
using System.Collections.Generic;
using static ChoreKit;

/// <summary>
/// LA BIBLIOTECA DE MISIONES POR RETOS DE CÓDIGO (portada de CodeShip origin/integracion: src/modules/chores/application/catalog.ts y sus datos
/// — kitchen/data/recipes.ts, medical/data/protocols.ts, wellness/data/yoga.ts, exercise.ts, sleep.ts, procedures.ts, pets/data/rules.ts,
/// mind-duel/data/exercises.ts). Cada plantilla es una tarea que se hace en un puesto resolviendo sus retos en orden; cualquier microcosmos la
/// pone en un mueble suyo con <see cref="MissionSites"/>. Las clases de CodeShip se conservan (cocinar, atender-paciente, yoga, ejercicio,
/// regar, curar-planta, cosechar, alimentar-perro, jugar-perro, programar-animal, reparar-maquina, dormir, asearse) y se añaden las de los
/// puestos de una fábrica (mecanizar, aviónica, composite) y «plantar-idea» (el duelo de ideas de CodeShip, con su curva de dificultad).
/// Todos los pasos y valores llevan su fuente; lo adaptado al microcosmos se marca «licencia».
/// </summary>
public static partial class MissionLibrary
{
    static List<ChoreTemplate> _all;
    static List<CodeExercise> _ideas;

    public static List<ChoreTemplate> All { get { if (_all == null) Build(); return _all; } }
    /// <summary>El banco del duelo de ideas («plantar una idea»), de concepto 1 a 9.</summary>
    public static List<CodeExercise> IdeaBank { get { if (_ideas == null) Build(); return _ideas; } }

    public static ChoreTemplate Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (ChoreTemplate t in All) if (t.id == id) return t;
        return null;
    }

    // ── Fuentes ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    const string CS_RECIPES = "Cold Sanctuary, CookingRecipe.cs (las mismas recetas que usa CodeShip, kitchen/data/recipes.ts)";
    const string USDA = "USDA FSIS, «Safe Minimum Internal Temperature Chart»: platos de huevo y carne picada 71 °C (160 °F); piezas enteras de vacuno 63 °C (145 °F) con 3 min de reposo";
    const string WHO_BEC = "OMS/CICR, «Basic Emergency Care» (triaje ABCDE: vía aérea, respiración, circulación, estado neurológico, exposición)";
    const string RED_CROSS = "Cruz Roja Americana, «First Aid/CPR/AED Participant's Manual»: golpes y torceduras, reposo, frío 20 min con un paño, compresión y elevación";
    const string AHA = "American Heart Association, «Monitoring Your Blood Pressure at Home»: 5 min sentado en reposo, manguito a la altura del corazón; normal, sistólica < 120 mmHg";
    const string AASM = "Watson et al. (2015), consenso AASM/SRS: un adulto necesita 7 horas o más de sueño por noche";
    const string WHO_HANDS = "OMS, «How to Handwash?» (2009): mojar, jabón, palmas, dorsos, entre los dedos, pulgares, aclarar y secar; 40–60 s";
    const string YOGA = "Cold Sanctuary, docs/sistemas/yoga-asanas-spec.md (la hoja de asanas que también usa CodeShip, wellness/data/yoga.ts)";
    const string SPRINT = "English et al. (2020), «High intensity training during spaceflight: the NASA Sprint Study», npj Microgravity 6:21; Tanaka et al. (2001), J. Am. Coll. Cardiol. 37:153 (FCmáx = 208 − 0,7 × edad)";
    const string WSAVA = "WSAVA, «Global Nutrition Guidelines» (2011): energía en reposo RER = 70 × kg^0,75; adulto castrado ≈ 1,6 × RER";
    const string ASPCA_PLAY = "ASPCA / Bekoff (1995), «Play signals as punctuation»: la reverencia de juego (pecho abajo, cola arriba) invita a jugar";
    const string CS_GARDEN = "CodeShip, garden/domain/plant.ts (regar por debajo del 40 % de agua; cuidar suma 0,4 de salud) y catalog.ts plantChores";
    const string SANDVIK = "Sandvik Coromant, fórmulas de torneado: n = Vc × 1000 / (π × D); aluminio con plaquita de metal duro, Vc ≈ 300 m/min";
    const string IPC = "IPC J-STD-001 e IPC-A-610 (soldadura e inspección de conjuntos electrónicos); NASA-STD-8739.3; punta del soldador para estaño-plomo ≈ 315–370 °C";
    const string HEXCEL = "Hexcel, ficha técnica HexPly 8552 (preimpregnado de carbono aeroespacial): curado en autoclave 110 min a 180 °C y 7 bar, con bolsa de vacío";
    const string ASME = "ASME PCC-1, «Guidelines for Pressure Boundary Bolted Flange Joint Assembly»: apretar en estrella/cruz y en varias pasadas hasta el par";
    const string OIL = "Manual del propietario de cualquier coche de 2008 (cambio de aceite y filtro); el nivel, entre las dos marcas de la varilla";
    const string CS_IDEAS = "CodeShip, mind-duel/data/exercises.ts (banco de ideas por conceptos del índice de estudio), con el tema de la fábrica de 2008";

    static void Build()
    {
        _all = new List<ChoreTemplate>();
        _ideas = new List<CodeExercise>();
        Kitchen();
        Health();
        Wellness();
        Animals();
        Field();
        Workshop();
        Ideas();
        Hawthorne();   // (2026-11-18) los sitios del lote 2 de Hawthorne: MissionLibrary.Hawthorne.cs
        Vinculos();    // (2026-11-19, PB-166) amistad, estudiante, combatir al lado y proteger, con retos de diálogo: MissionLibrary.Vinculos.cs
    }

    static ChoreTemplate Add(string id, string kind, ChoreCategory cat, string title, string scenario, string source, CodeExercise[] ex, string label = "real")
    {
        var t = new ChoreTemplate { id = id, kind = kind, category = cat, title = title, scenario = scenario, source = source, exercises = ex, label = label };
        foreach (CodeExercise e in ex) if (string.IsNullOrEmpty(e.source)) e.source = source;
        _all.Add(t);
        return t;
    }

    // ══ COMIDA: cada paso de la receta, un reto (CodeShip cookingChore) ══════════════════════════════════════════════════════════════
    static readonly (string id, string name, string scenario, string source, string[] steps)[] Recipes =
    {
        ("huevos-revueltos", "Huevos revueltos", "2 huevos → 1 porción para el turno de noche", CS_RECIPES + "; " + USDA,
            new[] { "ve a la nevera", "toma los huevos", "calienta la plancha", "revuelve los huevos", "cocina hasta {71|50|100} °C", "especia con sal y pimienta", "emplata" }),
        ("ensalada", "Ensalada", "Lechuga, tomate y rábano → 2 porciones", CS_RECIPES,
            new[] { "toma lechuga, tomate, rábano, limón, sal y aceite", "lava los vegetales", "corta los vegetales", "mezcla todo en el bol", "aliña con sal, limón y aceite", "emplata" }),
        ("tortilla-papa", "Tortilla de papa", "2 papas y 3 huevos → 2 porciones", "Wikipedia, «Tortilla de patatas» (receta tradicional); " + USDA,
            new[] { "pela las papas", "córtalas en láminas finas", "fríelas a fuego medio hasta que estén tiernas", "bate los huevos con sal", "mezcla las papas con el huevo", "cuaja un lado en la sartén", "dale la vuelta con un plato", "cuaja el otro lado hasta {71|40|150} °C" }),
        ("chapati", "Chapati (pan plano de trigo)", "Harina integral, agua y sal → 2 porciones", "Wikipedia, «Chapati»",
            new[] { "mezcla la harina integral con agua y sal", "amasa hasta que la masa esté lisa", "deja reposar la masa", "divídela en bolitas", "estira cada bolita en un disco", "cuécelo en la plancha caliente por los dos lados", "sirve caliente" }),
        ("tacos-carne-asada", "Tacos de carne asada", "La taquería de Crenshaw: carne de vacuno a la plancha en tortilla de maíz", "Wikipedia, «Carne asada» y «Taco»; " + USDA,
            new[] { "marina la carne con lima, ajo y sal", "calienta la plancha", "asa la carne hasta {63|40|100} °C por dentro", "deja reposar la carne {3|0|30} minutos", "córtala en tiras finas", "calienta las tortillas de maíz en la plancha", "sirve con cebolla, cilantro y salsa" }),
    };

    static void Kitchen()
    {
        var pool = new List<string>();
        foreach (var r in Recipes) foreach (string s in r.steps) pool.Add(PlainStep(s));
        foreach (var r in Recipes)
        {
            string name = r.name;
            Add("cocinar-" + r.id, "cocinar", ChoreCategory.Comida, "Cocinar " + name.ToLowerInvariant(), r.scenario, r.source,
                StepChain("cocinar-" + r.id, 1, i => $"{name}: paso {i + 1}", r.steps, pool));
        }
    }

    // ══ SALUD: triaje ABCDE y el protocolo, un reto por paso (CodeShip patientChore); el chequeo, dormir y asearse (procedureChore) ═══
    static readonly string[] Abcde = { "vía aérea", "respiración", "circulación", "estado neurológico", "exposición" };

    static void Health()
    {
        var pool = new List<string> { "dale de comer algo dulce", "frota la herida con alcohol puro", "dale una aspirina sin preguntar" };
        Patient("herida", "Primeros auxilios: un corte con chapa de aluminio", "Un técnico se ha cortado la mano con el borde de una sección de tanque", WHO_BEC,
            new[] { "ponte guantes", "presiona para parar el sangrado", "irriga la herida con suero salino", "revisa si hay cuerpos extraños", "cubre con un apósito limpio" }, pool);
        Patient("golpe", "Primeros auxilios: un golpe en el tobillo", "Una técnica ha bajado mal de la tarima de su puesto", RED_CROSS,
            new[] { "deja la zona en reposo", "aplica frío envuelto en un paño durante {20|2|90} minutos", "comprime con una venda sin apretar", "eleva el tobillo por encima del corazón", "si hay deformidad o el dolor no cede, al hospital" }, pool);
        Patient("estres-agudo", "Atender a alguien con estrés agudo", "Tras tres lanzamientos fallidos, alguien del equipo tiembla y no puede respirar con calma", "Cold Sanctuary, docs/areas/hospital.md (resumen de juego, como en CodeShip medical/data/protocols.ts)",
            new[] { "lleva a la persona a un lugar tranquilo", "baja la luz", "revisa lesiones ocultas", "ofrece agua", "deja que descanse" }, pool, "licencia");
        Add("medico-tension", "medico", ChoreCategory.Salud, "Tomar la tensión con el tensiómetro del botiquín", "Revisión del turno: tensión en reposo, apuntada", AHA,
            StepChain("medico-tension", 1, i => $"Tensión: paso {i + 1}", new[] { "siéntate cinco minutos en reposo", "pon el manguito en el brazo a la altura del corazón", "toma la tensión: normal, por debajo de {120|180|60} mmHg de sistólica", "apunta el resultado con la hora" }, new[] { "tómala justo después de subir corriendo", "habla mientras se infla el manguito" }));
    }

    static void Patient(string id, string title, string scenario, string source, string[] steps, List<string> pool, string label = "real")
    {
        var ex = new List<CodeExercise>
        {
            Choice($"atender-{id}-triaje", 1, CodeSkill.Predecir, "Primero, lo que mata antes", "Triaje ABCDE: ¿qué revisas primero?", new CodeBlock[0], Abcde[0], new[] { Abcde[2], "la herida que más se ve" }),
        };
        ex.AddRange(StepChain($"atender-{id}", 1, i => $"Paso {i + 1}", steps, pool));
        Add("atender-" + id, "atender-paciente", ChoreCategory.Salud, title, scenario, source + "; " + WHO_BEC, ex.ToArray(), label);
    }

    // ══ YOGA, EJERCICIO, DORMIR Y ASEARSE ════════════════════════════════════════════════════════════════════════════════════════════
    static readonly string[] Breaths = { "inhala", "exhala", "retén" };

    /// <summary>Un reto por postura: elegir la que viene, con su respiración (CodeShip yogaChore).</summary>
    static CodeExercise[] Yoga(string id, string name, (string name, string breath, string cue)[] ps)
    {
        var o = new CodeExercise[ps.Length];
        for (int i = 0; i < ps.Length; i++)
        {
            var prev = new List<CodeBlock>();
            for (int n = Math.Max(0, i - 2); n < i; n++) prev.Add(Block(n.ToString(), $"{n + 1}. {ps[n].name} · {ps[n].breath}", $"postura({n + 1}, \"{ps[n].name}\", \"{ps[n].breath}\");"));
            string other = Array.Find(Breaths, b => b != ps[i].breath);
            var q = Array.Find(ps, p => p.name != ps[i].name && (i == 0 || p.name != ps[i - 1].name));
            if (q.name == null) q = ps[(i + 1) % ps.Length];
            o[i] = Choice($"yoga-{id}-{i + 1}", 1, CodeSkill.Predecir, ps[i].cue, i == 0 ? $"¿Con qué postura empieza {name}?" : $"Postura {i + 1} de {ps.Length}: ¿qué viene ahora?",
                prev.ToArray(), $"{ps[i].name} · {ps[i].breath}", new[] { $"{ps[i].name} · {other}", $"{q.name} · {q.breath}" });
        }
        return o;
    }

    static void Wellness()
    {
        Add("yoga-upa-cuello", "yoga", ChoreCategory.Salud, "Upa-yoga de cuello y hombros", "Horas mirando pantallas y soldando inclinado: seis ejercicios, una postura por reto", YOGA, Yoga("upa-cuello", "el upa-yoga de cuello", new[]
        {
            ("Flexión", "exhala", "barbilla al pecho"), ("Extensión", "inhala", "cabeza atrás"), ("Rotación", "inhala", "cara a la derecha y vuelve al frente"),
            ("Inclinación lateral", "exhala", "oreja al hombro derecho, luego al izquierdo"), ("Círculo completo", "inhala", "barbilla, oreja, atrás, oreja, barbilla"),
            ("Hombros hacia delante", "exhala", "arriba-atrás inhalando, abajo-delante exhalando"),
        }));
        Add("yoga-saludo-al-sol", "yoga", ChoreCategory.Salud, "Saludo al Sol", "Doce asanas, una respiración por transición", YOGA, Yoga("saludo-al-sol", "el Saludo al Sol", new[]
        {
            ("Pranamasana", "exhala", "palmas juntas frente al esternón"), ("Hasta Uttanasana", "inhala", "brazos arriba a la anchura de los hombros"),
            ("Uttanasana", "exhala", "bisagra de cadera, manos junto a los pies"), ("Ashwa Sanchalanasana", "inhala", "pie derecho atrás, rodilla al suelo"),
            ("Dandasana", "retén", "plancha: talón a coronilla en línea"), ("Ashtanga Namaskara", "exhala", "rodillas, pecho y barbilla al suelo"),
            ("Bhujangasana", "inhala", "cobra: pecho arriba"), ("Adho Mukha Svanasana", "exhala", "perro boca abajo, caderas al cielo"),
            ("Ashwa Sanchalanasana", "inhala", "zancada con la otra pierna"), ("Uttanasana", "exhala", "vuelve a plegarte"),
            ("Hasta Uttanasana", "inhala", "sube con los brazos arriba"), ("Pranamasana", "exhala", "palmas al pecho"),
        }));

        // Ejercicio: el protocolo Sprint de la NASA en máquinas de tierra (CodeShip workoutChore; en la Tierra no hacen falta arneses).
        const int age = 30;
        float hrMax = 208f - 0.7f * age; int target = (int)Math.Ceiling(0.9 * hrMax);
        Add("ejercicio-intervalos-4x4", "ejercicio", ChoreCategory.Salud, "Intervalos 4 × 4 min en la cinta", "Protocolo Sprint de la NASA: cuatro intervalos por encima del 90 % del pulso máximo", SPRINT, new[]
        {
            Choice("ejercicio-4x4-pulso", 2, CodeSkill.Predecir, "Cada intervalo, por encima del 90 % del pulso máximo", $"Tienes {age} años: ¿a qué pulso tienes que llegar?",
                new[] { Block("a", $"fcMax = 208 − 0,7 × {age}", $"const fcMax = 208 - 0.7 * {age};"), Block("b", "objetivo = redondearArriba(0,9 × fcMax)", "const objetivo = Math.ceil(0.9 * fcMax);") },
                $"{target} lpm", new[] { $"{Math.Round(hrMax)} lpm", $"{Math.Ceiling(0.75 * hrMax)} lpm" }),
            Choice("ejercicio-4x4-minutos", 3, CodeSkill.Predecir, "Los intervalos se repiten en un bucle", "¿Cuántos minutos vas a fondo?",
                new[] { Block("a", "repetir(4) {", "for (let i = 0; i < 4; i++) {"), Block("b", "a fondo 240 s", "aFondo(240);", 1), Block("c", "suave hasta recuperar", "suave();", 1), Block("d", "}", "}") },
                "16 min", new[] { "4 min", "32 min" }),
            Choice("ejercicio-4x4-limite", 2, CodeSkill.Predecir, "La cinta va de 2,4 a 19,3 km/h (como la T2 de la ISS, una Woodway)", "Si se le pide 22 km/h, ¿qué da?",
                new[] { Block("a", "pedida = 22 km/h", "const pedida = 22;"), Block("b", "real = limitar(pedida, 2,4, 19,3)", "const real = Math.max(2.4, Math.min(19.3, pedida));") },
                "19,3 km/h", new[] { "22 km/h", "2,4 km/h" }),
        });
        Add("ejercicio-fuerza-4x8", "ejercicio", ChoreCategory.Salud, "Fuerza 4 × 8 con la barra", "Protocolo Sprint: series y repeticiones, la cuarta casi al máximo", SPRINT, new[]
        {
            Choice("ejercicio-fuerza-reps", 3, CodeSkill.Predecir, "Series y repeticiones son un bucle dentro de otro", "¿Cuántas repeticiones haces en total?",
                new[] { Block("a", "repetir(4) {", "for (let s = 0; s < 4; s++) {"), Block("b", "repetir(8) { sentadilla() }", "for (let r = 0; r < 8; r++) sentadilla();", 1), Block("c", "}", "}") },
                "32", new[] { "12", "8" }),
            Order("ejercicio-fuerza-pasos", 1, "Una sentadilla segura", "calienta con la barra vacía", "pon los discos y los cierres", "baja con la espalda recta", "sube empujando con los talones", "deja la barra en su soporte"),
        });
        Add("ejercicio-bici-30", "ejercicio", ChoreCategory.Salud, "30 min continuos en la bicicleta", "Al 75 % del VO₂ pico, tres días a la semana", SPRINT, new[]
        {
            Choice("ejercicio-bici-semana", 3, CodeSkill.Predecir, "Tres días a la semana, alternando con los intervalos", "¿Cuántos minutos suma a la semana?",
                new[] { Block("a", "repetir(3) {", "for (let dia = 0; dia < 3; dia++) {"), Block("b", "pedalear 30 min", "pedalear(30);", 1), Block("c", "}", "}") },
                "90 min", new[] { "30 min", "180 min" }),
        });

        Add("dormir-catre", "dormir", ChoreCategory.Salud, "Dormir en el catre del turno de noche", "Siete semanas entre el vuelo 3 y el 4: se duerme en la nave, por turnos", AASM,
            StepChain("dormir-catre", 1, i => $"Dormir: paso {i + 1}", new[] { "pasa tu turno al técnico que entra", "lávate los dientes", "apaga la pantalla del portátil", "pon la alarma para dormir al menos {7|4|12} horas", "tápate con la manta", "apaga la luz y duerme" },
                new[] { "tómate un café doble", "deja la pantalla encendida junto a la cara" }), "licencia");
        Add("asearse-manos", "asearse", ChoreCategory.Salud, "Lavarse las manos (antes de comer o de tocar la aviónica)", "Grasa, polvo de carbono y virutas: las manos, como dice la OMS", WHO_HANDS,
            StepChain("asearse-manos", 1, i => $"Lavarse: paso {i + 1}", new[] { "mójate las manos con agua", "pon jabón suficiente para cubrirlas", "frota las palmas entre sí", "frota el dorso de cada mano con la otra palma", "entrelaza los dedos y frota", "frota los pulgares girándolos", "frota todo durante {40|10|5} segundos como mínimo", "aclara con agua", "sécate con una toalla de un solo uso" },
                new[] { "sécate en la ropa", "sopla las manos para secarlas" }));
    }

    // ══ ANIMALES: el perro del pueblo (en el microcosmos, una cochinilla hormiguera) ═════════════════════════════════════════════════
    static void Animals()
    {
        const float kg = 20f, factor = 1.6f, kibble = 3.5f;
        double rer = 70 * Math.Pow(kg, 0.75), daily = rer * factor; int meal = (int)Math.Round(daily / 2), grams = (int)Math.Round(meal / kibble);
        Add("perro-alimentar", "alimentar-perro", ChoreCategory.Animales, "Dar de comer al perro del barrio", $"Un perro mestizo de {kg:0} kg gimotea junto a su cuenco", WSAVA, new[]
        {
            Choice("perro-alimentar-racion", 4, CodeSkill.Predecir, "¡Comida, comida!", $"Es un perro de {kg:0} kg, adulto y castrado. ¿Cuántas kcal lleva su comida (la mitad del día)?",
                new[] { Block("a", $"guardar 70 × {kg:0}^0,75 en reposo", $"const reposo = 70 * {kg:0} ** 0.75; // ≈ {Math.Round(rer)}"), Block("b", "guardar reposo × 1,6 en diario", "const diario = reposo * 1.6;"), Block("c", "guardar diario ÷ 2 en comida", "const comida = diario / 2;") },
                $"{meal} kcal", new[] { $"{Math.Round(rer / 2)} kcal", $"{Math.Round(daily)} kcal", $"{meal * 3} kcal" }),
            Choice("perro-alimentar-gramos", 4, CodeSkill.Predecir, "Como de todo, pero poquito", "Su pienso da 3,5 kcal por gramo. ¿Cuánto sirves?",
                new[] { Block("a", "gramos = comida ÷ 3,5", "const gramos = Math.round(comida / 3.5);"), Block("b", "servir(gramos)", "servir('pienso', gramos);") },
                $"{grams} g", new[] { $"{meal} g", $"{grams * 3} g", "un cuenco lleno hasta arriba" }),
            Order("perro-alimentar-pasos", 1, "Siéntate… ¡bien!", "pedirle que se siente", "servir la ración", "cambiar el agua"),
        }, "licencia");
        Add("perro-jugar", "jugar-perro", ChoreCategory.Animales, "Pasear y jugar con el perro del barrio", "Te trae su pelota y baja el pecho", ASPCA_PLAY, new[]
        {
            Choice("perro-jugar-reverencia", 5, CodeSkill.Predecir, "¿Jugamos?", "Baja el pecho y levanta la cola (reverencia de juego). ¿Qué hace el programa?",
                new[] { Block("a", "si perro.reverencia entonces", "if (perro.reverencia) {"), Block("b", "lanzar la pelota", "lanzar('pelota');", 1), Block("c", "si no", "} else {"), Block("d", "acariciarlo despacio", "acariciar(perro);", 1), Block("e", "fin", "}") },
                "Lanza la pelota", new[] { "Lo acaricia despacio", "Lo ignora" }),
            Order("perro-jugar-pasos", 1, "Un paseo por la calle de las casas", "ponerle la correa", "pasear por la calle de las casas", "jugar a la pelota en el parque", "darle agua al volver"),
        }, "licencia");
        Add("perro-programar-bordillo", "programar-animal", ChoreCategory.Animales, "Enseñar al perro: esperar en el bordillo", "Crenshaw Boulevard tiene seis carriles: que espere la orden antes de cruzar", "CodeShip, pets/data/rules.ts (programar la mente de un animal con eventos); AKC, «How to Teach Your Dog to Wait»", new[]
        {
            new CodeExercise { id = "perro-bordillo-evento", kind = CodeExercise.Kind.Order, concept = 9, idea = "Huele a calle y a coches", question = "Ordena la regla. Sobra un bloque.",
                blocks = new[] { Block("a", "cuando llegue al bordillo", "on('bordillo', () => {"), Block("b", "sentarme", "sentarme();", 1), Block("c", "esperar la orden «cruza»", "await orden('cruza');", 1), Block("d", "fin", "});"), Block("x", "cruzar corriendo", "cruzar();", 1) },
                solution = new[] { "a", "b", "c", "d" }, skill = CodeSkill.Especificar },
            Choice("perro-bordillo-predecir", 5, CodeSkill.Predecir, "¿Cruzo o espero?", "El semáforo está en rojo para él. ¿Qué hará?",
                new[] { Block("a", "si semaforo == \"verde\" y orden == \"cruza\" entonces", "if (semaforo === 'verde' && orden === 'cruza') {"), Block("b", "cruzar()", "cruzar();", 1), Block("c", "si no", "} else {"), Block("d", "esperar sentado", "esperar();", 1), Block("e", "fin", "}") },
                "Espera sentado", new[] { "Cruza", "Se va a casa" }),
        }, "licencia");
    }

    // ══ CAMPO: los huertos y frutales del pueblo (CodeShip plantChores) ══════════════════════════════════════════════════════════════
    static void Field()
    {
        Add("campo-regar", "regar", ChoreCategory.Plantas, "Regar los frutales del pueblo", "El sustrato se está secando con el sol de septiembre en Los Ángeles", CS_GARDEN, new[]
        {
            Choice("campo-regar-umbral", 4, CodeSkill.Completar, "Tengo sed", "¿Qué valor falta para regar cuando le quede menos del 40 % de agua?",
                new[] { Block("a", "si planta.agua < ___ entonces", "if (planta.agua < ___) {"), Block("b", "regar(planta)", "regar(planta);", 1), Block("c", "fin", "}") },
                "0,4", new[] { "40", "\"poca\"" }),
            Order("campo-regar-pasos", 1, "Que el agua llegue a las raíces", "llenar la regadera", "regar la base del tronco", "comprobar que drena"),
        }, "licencia");
        Add("campo-curar", "curar-planta", ChoreCategory.Plantas, "Curar un frutal dañado", "El viento de Santa Ana ha roto ramas y hojas", CS_GARDEN, new[]
        {
            Order("campo-curar-pasos", 1, "Mis hojas rotas me quitan fuerza", "revisar las hojas", "cortar las ramas dañadas", "asegurar el tutor", "regar la base"),
            Choice("campo-curar-salud", 2, CodeSkill.Predecir, "¿Cuánto me recupero?", "¿Cuánto vale salud al final?",
                new[] { Block("a", "guardar 0,3 en salud", "let salud = 0.3;"), Block("b", "sumar 0,4 a salud", "salud = salud + 0.4;") },
                "0,7", new[] { "0,3", "0,4", "0,34" }),
        }, "licencia");
        Add("campo-cosechar", "cosechar", ChoreCategory.Plantas, "Cosechar la fruta madura", "Los limoneros y naranjos de las casas están cargados", CS_GARDEN, new[]
        {
            Order("campo-cosechar-pasos", 1, "Ya estoy madura", "comprobar que está madura", "cortar con tijeras limpias", "guardar en la despensa"),
            Choice("campo-cosechar-porciones", 5, CodeSkill.Predecir, "¿Cuánto doy?", "¿Cuántas porciones dará?",
                new[] { Block("a", "guardar 0,6 en salud", "let salud = 0.6;"), Block("b", "guardar verdadero en dioFruto", "let dioFruto = true;"), Block("c", "si no dioFruto entonces porciones = 0", "if (!dioFruto) porciones = 0;"),
                        Block("d", "si no, si salud >= 0,5 entonces porciones = 2", "else if (salud >= 0.5) porciones = 2;"), Block("e", "si no porciones = 1", "else porciones = 1;") },
                "2", new[] { "0", "1" }),
        }, "licencia");
    }

    // ══ TALLER: los puestos de una fábrica de cohetes de 2008 y el taller del pueblo ════════════════════════════════════════════════
    static void Workshop()
    {
        int rpm = (int)Math.Round(300 * 1000 / (Math.PI * 50));
        var ex = new List<CodeExercise>(StepChain("mecanizar-torno", 1, i => $"Torno: paso {i + 1}",
            new[] { "sujeta la barra de aluminio en el plato del torno", "monta la herramienta y mide su corrector", "carga el programa de la pieza", "simula el programa en vacío", "cierra la puerta y arranca el husillo", "mide la pieza con el micrómetro" },
            new[] { "abre la puerta con el husillo en marcha", "quita las virutas con la mano" }));
        ex.Add(Choice("mecanizar-torno-rpm", 2, CodeSkill.Predecir, "La velocidad de corte manda", "Aluminio, Vc = 300 m/min, diámetro 50 mm. ¿A cuántas rpm gira el husillo?",
            new[] { Block("a", "Vc = 300  // m/min", "const Vc = 300;"), Block("b", "D = 50    // mm", "const D = 50;"), Block("c", "n = Vc × 1000 ÷ (π × D)", "const n = Math.round(Vc * 1000 / (Math.PI * D));") },
            $"{rpm} rpm", new[] { "6 rpm", $"{rpm * 10} rpm", "300 rpm" }));
        Add("mecanizar-torno", "mecanizar", ChoreCategory.Taller, "Mecanizar una pieza en el torno CNC", "Un soporte de aluminio para la línea de combustible del Merlin", SANDVIK, ex.ToArray());

        var av = new List<CodeExercise>(StepChain("avionica", 1, i => $"Aviónica: paso {i + 1}",
            new[] { "ponte la pulsera antiestática", "calienta el soldador a {340|150|600} °C", "suelda el conector al mazo de cables", "inspecciona cada soldadura con la lupa", "prueba la continuidad con el multímetro", "enciende la caja y mira la señal en el osciloscopio" },
            new[] { "toca las placas sin pulsera", "enfría la soldadura soplando" }));
        av.Add(Choice("avionica-tramas", 6, CodeSkill.Predecir, "La telemetría es un bucle", "La caja manda 10 tramas por segundo durante una prueba de 5 s. ¿Cuántas tramas llegan?",
            new[] { Block("a", "repetir(5) {   // segundos", "for (let s = 0; s < 5; s++) {"), Block("b", "repetir(10) { enviarTrama() }", "for (let i = 0; i < 10; i++) enviarTrama();", 1), Block("c", "}", "}") },
            "50", new[] { "15", "10", "5" }));
        Add("avionica-caja", "avionica", ChoreCategory.Taller, "Montar y probar una caja de aviónica", "Las cajas de vuelo del Falcon 1: cableado, soldadura y prueba", IPC, av.ToArray());

        Add("composite-cofia", "composite", ChoreCategory.Taller, "Laminar media cofia de fibra de carbono", "La cofia del Falcon 1: preimpregnado, bolsa de vacío y autoclave", HEXCEL,
            StepChain("composite-cofia", 1, i => $"Composite: paso {i + 1}",
                new[] { "limpia el molde y pon desmoldeante", "corta las telas de carbono preimpregnado", "colócalas capa a capa con la orientación del plano", "pon el tejido pelable y el aireador", "cierra la bolsa de vacío y comprueba que no pierde", "cura en el autoclave a {180|60|400} °C", "desmolda y recorta la pieza" },
                new[] { "cura la pieza al sol", "pisa las telas para que se peguen" }));

        Add("reparar-brida-merlin", "reparar-maquina", ChoreCategory.Taller, "Apretar la brida del Merlin 1C", "Banco del Merlin: una brida de la línea de combustible tras la prueba de encendido", ASME, new[]
        {
            Order("brida-pasos", 1, "Algo se soltó dentro de mí", "limpia las roscas", "pon los pernos a mano", "aprieta en cruz", "aprieta al par en dos pasadas", "marca cada perno apretado"),
            Choice("brida-orden", 7, CodeSkill.Especificar, "Que la junta asiente pareja", "¿En qué orden se aprietan los 4 pernos de la brida?", new CodeBlock[0],
                "en cruz: 1, 3, 2, 4", new[] { "en círculo: 1, 2, 3, 4", "todos a la vez al máximo" }),
        });
        Add("reparar-coche-aceite", "reparar-maquina", ChoreCategory.Taller, "Cambiar el aceite de un coche", "El taller de coches de Crenshaw", OIL,
            StepChain("coche-aceite", 1, i => $"Taller: paso {i + 1}",
                new[] { "calienta el motor un par de minutos", "quita el tapón del cárter y vacía el aceite", "cambia el filtro de aceite", "pon el tapón con su arandela nueva", "llena con el aceite que dice el manual", "comprueba el nivel con la varilla" },
                new[] { "tira el aceite usado por el desagüe", "llena hasta arriba sin mirar la varilla" }));
    }

    // ══ IDEAS: el duelo de ideas de CodeShip, con el banco retematizado a la fábrica de 2008 ═════════════════════════════════════════
    static void Ideas()
    {
        CodeExercise O(string id, int concept, string idea, string[] sol, params (string id, string p, string js)[] bs)
        {
            var blocks = new List<CodeBlock>();
            foreach (var b in bs) blocks.Add(Block(b.id, b.p, b.js));
            return new CodeExercise { id = id, kind = CodeExercise.Kind.Order, concept = concept, idea = idea, question = sol.Length < bs.Length ? "Ordena la idea. Sobra un bloque." : "Ordena la idea.", blocks = blocks.ToArray(), solution = sol, source = CS_IDEAS };
        }
        CodeExercise C(string id, int concept, CodeSkill skill, string idea, string q, string[] options, int correct, params (string id, string p, string js)[] bs)
        {
            var blocks = new List<CodeBlock>();
            foreach (var b in bs) blocks.Add(Block(b.id, b.p, b.js));
            return new CodeExercise { id = id, kind = CodeExercise.Kind.Choice, concept = concept, skill = skill, idea = idea, question = q, options = options, correct = correct, blocks = blocks.ToArray(), source = CS_IDEAS };
        }
        _ideas.AddRange(new[]
        {
            O("idea-consola", 1, "Primero se mira, luego se decide", new[] { "a", "b", "c" }, ("a", "encender la consola", "encender(consola);"), ("b", "cargar la telemetría del vuelo 3", "cargar(telemetria);"), ("c", "mostrar la curva de empuje", "mostrar(curvaEmpuje);")),
            O("idea-tanque", 1, "Un tanque se prueba antes de volar", new[] { "a", "b", "c" }, ("a", "presurizar el tanque con helio", "presurizar(tanque);"), ("b", "escuchar si hay fugas", "escucharFugas(tanque);"), ("c", "apuntar la presión", "apuntar(tanque.presion);")),
            O("idea-intentar", 1, "Fallar, aprender, volver a intentar", new[] { "a", "b", "c", "d" }, ("a", "leer los datos del fallo", "leer(datos);"), ("b", "encontrar la causa", "causa = analizar(datos);"), ("c", "cambiar la secuencia", "cambiar(secuencia, causa);"), ("d", "lanzar otra vez", "lanzar();")),
            C("idea-intentos", 2, CodeSkill.Predecir, "Cuento los intentos", "¿Cuánto vale intentos al final?", new[] { "3", "4", "1", "34" }, 1, ("a", "guardar 3 en intentos", "let intentos = 3;"), ("b", "guardar intentos + 1 en intentos", "intentos = intentos + 1;")),
            C("idea-nombre", 2, CodeSkill.Completar, "Un cohete tiene nombre", "¿Qué falta para guardar el nombre del cohete?", new[] { "\"Falcon 1\"", "mostrar", "nombreCohete", "= =" }, 0, ("a", "guardar ___ en nombreCohete", "let nombreCohete = ___;"), ("b", "mostrar nombreCohete", "mostrar(nombreCohete);")),
            C("idea-reparar", 2, CodeSkill.Reparar, "Algo no encaja", "¿Qué línea falla?", new[] { "Se usa empuje antes de guardarlo", "4 no es un número", "Sobra mostrar", "Está bien" }, 0, ("a", "mostrar empuje", "mostrar(empuje);"), ("b", "guardar 4 en empuje", "let empuje = 4;")),
            C("idea-orbita", 3, CodeSkill.Completar, "¿Estamos en órbita o no?", "¿Qué tipo de dato encaja en enOrbita?", new[] { "verdadero (booleano)", "\"4\" (texto)", "4 (número)", "nada" }, 0, ("a", "guardar ___ en enOrbita", "let enOrbita = ___;")),
            C("idea-texto", 3, CodeSkill.Predecir, "Junto el nombre con el número", "¿Qué muestra?", new[] { "Falcon1", "Falcon + 1", "2", "Error" }, 0, ("a", "guardar \"Falcon\" en nombre", "let nombre = \"Falcon\";"), ("b", "guardar 1 en numero", "let numero = 1;"), ("c", "mostrar nombre + numero", "mostrar(nombre + numero);")),
            C("idea-precisa", 3, CodeSkill.Especificar, "¿Qué quieres exactamente?", "¿Qué petición es lo bastante precisa para que el equipo la cumpla?", new[] { "Arreglad el cohete", "Alargad la espera entre el apagado del Merlin y la separación", "Hacedlo mejor", "Lo que veáis" }, 1),
            C("idea-si", 5, CodeSkill.Predecir, "Si aún empuja, se espera", "empuje vale 2. ¿Qué hace el programa?", new[] { "Espera", "Separa", "Las dos cosas" }, 0, ("a", "si empuje > 0 entonces", "if (empuje > 0) {"), ("b", "esperar()", "esperar();"), ("c", "si no", "} else {"), ("d", "separar()", "separar();"), ("e", "fin", "}")),
            C("idea-bucle", 6, CodeSkill.Predecir, "Tres veces hasta la cuarta", "¿Cuántas veces se llama a lanzar()?", new[] { "4", "3", "1" }, 0, ("a", "repetir(4) {", "for (let i = 0; i < 4; i++) {"), ("b", "lanzar()", "lanzar();"), ("c", "}", "}")),
            O("idea-evento", 9, "Cuando se apaga, espero", new[] { "a", "b", "c", "d" }, ("a", "cuando se apague el Merlin", "on('apagado', () => {"), ("b", "esperar a que caiga el empuje", "await sinEmpuje();"), ("c", "separar las etapas", "separar();"), ("d", "fin", "});"), ("x", "separar enseguida", "separar(); // ya")),
        });

        Add("idea-reutilizar", "plantar-idea", ChoreCategory.Ideas, "Plantar una idea: no rendirse tras el tercer fallo", "Convence al equipo, idea a idea, antes de que se le agote la paciencia", CS_IDEAS, new CodeExercise[0], "licencia").duel = true;
        Add("idea-secuencia", "plantar-idea", ChoreCategory.Ideas, "Plantar una idea: el arreglo es de tiempo, no de piezas", "En la pizarra, ante los ingenieros: más ideas que acertar y menos paciencia", CS_IDEAS, new CodeExercise[0], "licencia").duel = true;
        Find("idea-secuencia").duelLevel = 0.45f;
    }
}
