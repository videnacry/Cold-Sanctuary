using System.Collections.Generic;

/// <summary>
/// Pools de contenido de la biblioteca de frases (docs/anima-architecture.md §11). Separado de la
/// mecánica (MindPhrase/PhraseLibrary) para poder AUTORAR sin tocar la lógica.
///
/// - <see cref="Vivencias"/>: biografías. Las de los COMPAÑEROS del santuario (Goluis/Panterilia/
///   Gohageneis/Irosene) salen fielmente de docs/creature-stats.md. El MISMO formato sirve para los
///   PERSONAJES HISTÓRICOS del Microcosmos (docs/mob-characters.md): se autoran con sus pensamientos
///   documentados / lo que fueron o hicieron, con su `source` propio → el modo narración los reconstruye.
/// - <see cref="Deseos"/>: deseos base genéricos (anónimos, reutilizables), de lo simple a lo complejo.
///
/// Cada vivencia usa el arco [nace, crece, reproduce] en 1ª persona (la voz interior del ser). El tono
/// elemental refleja su carácter dominante (Goluis→Tierra, Panterilia→Viento, Gohageneis→Agua,
/// Irosene→Fuego), tal como fija creature-stats.md. `reusable:false` marca las vivencias-firma (que
/// definen a alguien) para que no nazcan dos seres con ellas salvo azar.
/// </summary>
public static class PhrasePools
{
    // Atajos para no repetir la categoría en cada línea.
    static MindPhrase Vivencia(string source, ElementalTone tone, string[] pos, string[] neg,
                               bool reusable = false) =>
        new MindPhrase(tone, pos, neg, PhraseCategory.Vivencia, randomAssignable: true, reusable: reusable, source: source);

    // Personaje HISTÓRICO del Microcosmos: NO entra en el reparto aleatorio del santuario
    // (randomAssignable=false); solo se asigna por su identidad en modo narración (VivenciasOf).
    static MindPhrase Historico(string source, ElementalTone tone, string[] pos, string[] neg) =>
        new MindPhrase(tone, pos, neg, PhraseCategory.Vivencia, randomAssignable: false, reusable: false, source: source);

    public static List<MindPhrase> Vivencias() => new List<MindPhrase>
    {
        // ── Goluis (fuerza; Tierra) ─────────────────────────────────────────────────────────────────
        Vivencia("Goluis", ElementalTone.Tierra,
            new[] { "Mis manos conocen el peso.", "Cargo sin quejarme, un día y otro.", "Lo que sostengo, perdura." },
            new[] { "Otra vez la misma carga.", "El cuerpo aguanta, pero ¿para qué?", "Solo soy la fuerza que usan." }),
        Vivencia("Goluis", ElementalTone.Fuego,   // el pasado en la banda: temple y desconfianza
            new[] { "Aprendí a leer a la gente.", "Sé cuándo alguien miente.", "Cuido a los míos aunque me cueste." },
            new[] { "No me fío de nadie.", "Todos quieren algo de mí.", "Mi pasado me mira de reojo." }),
        Vivencia("Goluis", ElementalTone.Tierra,  // ser padre / su hija
            new[] { "Hay alguien por quien vale la pena.", "Haría lo que fuera por ella.", "Me sostengo por ella." },
            new[] { "Le fallé antes.", "¿Y si no soy suficiente?", "Cargo una culpa que no suelto." }),

        // ── Panterilia (percepción; Viento) ─────────────────────────────────────────────────────────
        Vivencia("Panterilia", ElementalTone.Viento,   // estudiar sin parar
            new[] { "Siempre hay algo más que aprender.", "Encadeno una idea con otra.", "Enseño lo que descubro." },
            new[] { "No paro de pensar.", "Mil ideas y ninguna quieta.", "Me pierdo en lo que imagino." }),
        Vivencia("Panterilia", ElementalTone.Viento,   // atención al detalle
            new[] { "Noto lo que otros pasan por alto.", "Cada detalle cuenta una historia.", "Ordeno el caos con la vista." },
            new[] { "Veo fallos por todas partes.", "El detalle me abruma.", "No dejo pasar ni una." }),
        Vivencia("Panterilia", ElementalTone.Agua,     // se bloquea bajo estrés
            new[] { "Respiro y el miedo afloja.", "Fluyo hasta que vuelvo a mí.", "Dejo que pase la ola." },
            new[] { "Bajo presión me quedo en blanco.", "El cuerpo no responde.", "Me disuelvo cuando más falta hago." }),
        Vivencia("Panterilia", ElementalTone.Viento,   // exageración / influida por ideas de terceros
            new[] { "Imagino mundos enteros.", "Las ideas ajenas me encienden.", "Cuento historias más grandes que la vida." },
            new[] { "Agrando todo en mi mente.", "Creo lo que otros sueñan.", "No distingo lo real de lo temido." }),

        // ── Gohageneis (versatilidad; Agua + Fuego) ───────────────────────────────────────────────────
        Vivencia("Gohageneis", ElementalTone.Agua,     // nómada por muchos países
            new[] { "Me adapto a cualquier lugar.", "Cada sitio me enseña algo.", "Llevo mi casa donde voy." },
            new[] { "Nunca echo raíces.", "¿Dónde es mi hogar?", "Me arrastra el siguiente destino." }),
        Vivencia("Gohageneis", ElementalTone.Agua,     // encadenar oficios variados
            new[] { "Aprendo cualquier oficio rápido.", "Me amoldo a lo que haga falta.", "Nada se me resiste mucho." },
            new[] { "Nada dura conmigo.", "Empiezo mil cosas.", "¿Qué soy, al final?" }),
        Vivencia("Gohageneis", ElementalTone.Fuego,    // fiesta / baile / celebrar
            new[] { "La música me enciende.", "Contagio ganas de bailar.", "Celebro estar vivo." },
            new[] { "Huyo hacia la fiesta.", "El ruido tapa el vacío.", "Mañana no recuerdo hoy." }),

        // ── Irosene (pasión / sociabilidad; Fuego + Agua) ───────────────────────────────────────────────
        Vivencia("Irosene", ElementalTone.Fuego,       // motivadora hiperexpresiva
            new[] { "Puedo encender a cualquiera.", "Tu ánimo también es mío.", "Levanto a quien cae." },
            new[] { "Doy tanto que me vacío.", "¿Y quién me anima a mí?", "Sonrío aunque me duela." }),
        Vivencia("Irosene", ElementalTone.Fuego,       // dulces / envolver dulces de niña
            new[] { "Endulzo la vida de los demás.", "Mis manos recuerdan el oficio.", "Un dulce y todo mejora." },
            new[] { "Endulzo para no llorar.", "Me deshago por agradar.", "Nadie prueba lo mío por mí." }),
        Vivencia("Irosene", ElementalTone.Agua,        // sobrevivir a hogares violentos
            new[] { "Sobreviví a lo que me rompió.", "Me rehíce de la nada.", "El agua vence a la roca con calma." },
            new[] { "Aún cargo esos golpes.", "El miedo vive en un rincón.", "Me disuelvo para no romperme." }),
        Vivencia("Irosene", ElementalTone.Agua,        // renacer con el santuario (cáncer / movilidad)
            new[] { "Mi cuerpo volvió a moverse.", "Corro, escalo, buceo: otra vez viva.", "Nunca es tarde para renacer." },
            new[] { "Mi cuerpo me falló una vez.", "Temo que vuelva a fallar.", "El tiempo aprieta." }),

        // ═══ Personajes históricos del Microcosmos (bloqueados; docs/mob-quests-early.md §2) ═══════════════
        // ── Ötzi, el Hombre de Hielo (~3300 a.C., Edad del Cobre; mecánica "buscar la raíz"; Tierra) ──────
        Historico("Ötzi", ElementalTone.Tierra,        // hombre común: pastor y trabajador del cobre
            new[] { "Trabajo el cobre con mis manos.", "Guío el rebaño por el monte.", "Soy un hombre común, y basta." },
            new[] { "Nadie recordará mi nombre.", "Solo soy uno más en el monte.", "La montaña no perdona al débil." }),
        Historico("Ötzi", ElementalTone.Fuego,         // huía de algo, siempre alerta
            new[] { "Estoy atento a cada sombra.", "Confío en mi instinto.", "Sé cuándo el peligro ronda." },
            new[] { "Algo me persigue.", "No puedo bajar la guardia.", "Estuve alerta… no lo bastante." }),
        Historico("Ötzi", ElementalTone.Fuego,         // la flecha por la espalda: la emboscada (la raíz)
            new[] { "Cargué el primer metal, hacha y orgullo.", "Creí que el filo me protegía.", "Miré de frente lo que pude." },
            new[] { "Me dispararon por la espalda.", "Una disputa, un hacha, y la muerte.", "El primer metal fue la primera arma." }),
        Historico("Ötzi", ElementalTone.Agua,          // 5000 años congelado; comprender su muerte lo libera
            new[] { "Comprender mi muerte me libera.", "Recupero la historia que me robaron.", "La verdad, aun dura, da paz." },
            new[] { "Llevo cinco mil años sin descanso.", "Mi muerte quedó sin resolver.", "El hielo me guardó y me olvidó." }),

        // ── Nasatya (guardián ficticio; encarna a un Ashvin — veloz, generoso, rescata a los débiles;
        //    con Kushal, "los recolectores estrella"). Era temprana; docs/founding-trio-stories.md ──────────
        Historico("Nasatya", ElementalTone.Fuego,      // huérfano; el Señor del Fuego le sembró calidez con piedras
            new[] { "Unas piedras encendieron mi alma huérfana.", "Aprendí a jugar, a lanzar, a reír.", "Llevo el calor que me dieron." },
            new[] { "Perdí a mis padres muy pronto.", "El frío de dentro no se apaga solo.", "Cargo una orfandad callada." }),
        Historico("Nasatya", ElementalTone.Viento,     // recolector estrella: velocidad, energía, generosidad
            new[] { "Soy rápido y fuerte; traigo comida para todos.", "Doy lo mío antes de guardarlo.", "Cuidar a los míos es mi orgullo." },
            new[] { "Cargo con el hambre de todos.", "Si fallo, alguien no come.", "Nadie corre por mí." }),
        Historico("Nasatya", ElementalTone.Agua,       // la idea de la coneja: asentar a los débiles, no abandonarlos
            new[] { "La coneja oculta a sus crías y vuelve por ellas.", "Quedémonos por quien ya no puede seguir.", "Nadie más será abandonado." },
            new[] { "Dejamos atrás a los nuestros cada viaje.", "La culpa de tantos adioses me pesa.", "El miedo vuelve cruel a la tribu." }),
        Historico("Nasatya", ElementalTone.Fuego,      // el señuelo: se queda atrás para salvar a la Sembradora
            new[] { "Distraigo a las fieras: que ella viva.", "Doy mi carrera por su salvación.", "Que Kushal la lleve a salvo." },
            new[] { "Sé que quizá no vuelva.", "Me buscarán y no me hallarán.", "Me voy cargando una culpa que no es mía." }),

        // ── El Guardián / Señor del Fuego (Paleolítico, Cocina/FuelLab; arquetipo Canalizar; Fuego) ────────
        Historico("Guardián del Fuego", ElementalTone.Fuego,   // velar la última llama
            new[] { "Velo la última llama viva.", "Mientras yo vele, la tribu vive.", "Reparto el fuego, hogar por hogar." },
            new[] { "Si me duermo, todo se apaga.", "El fuego se muere en mis manos.", "Cargo yo solo con la noche." }),
        Historico("Guardián del Fuego", ElementalTone.Fuego,   // el fuego hace crecer a la tribu
            new[] { "El fuego cocina y sana.", "Su calor aleja a las fieras.", "Con la llama, crecimos." },
            new[] { "El fuego pide y pide leña.", "Sin él volvemos al frío.", "Dependemos de una sola brasa." }),
        Historico("Guardián del Fuego", ElementalTone.Fuego,   // el giro: el mismo don destruye; la intención decide
            new[] { "Aprendí a contener las llamas.", "El don se cuida, no se teme.", "La intención decide, no el fuego." },
            new[] { "El fuego que nutre también arrasa.", "Hice del hogar un arma.", "Lo que da vida, quema." }),

        // ── La Sembradora (Neolítico, Huerto; arquetipo Curar/atender; Tierra/Agua) ────────────────────────
        Historico("La Sembradora", ElementalTone.Tierra,       // domesticar el grano
            new[] { "Guardé la semilla y volvió multiplicada.", "La tierra responde a quien la cuida.", "De un puñado, un campo." },
            new[] { "La primera cosecha se malogró.", "El suelo no da si no sé pedirle.", "Sembré y no brotó nada." }),
        Historico("La Sembradora", ElementalTone.Agua,         // atender/nutrir el brote
            new[] { "Riego y los brotes despiertan.", "Cuidar el brote es cuidar a todos.", "La paciencia da fruto." },
            new[] { "Un descuido y el campo enferma.", "La sequía se lo lleva todo.", "Doy y doy sin descanso." }),
        Historico("La Sembradora", ElementalTone.Tierra,       // el giro: el excedente trae jerarquía y guerra
            new[] { "Aprendí a cultivar también generosidad.", "Lo que sembramos moldea al pueblo.", "Reparto la abundancia." },
            new[] { "El grano guardado despertó la codicia.", "Cercas, dueños, primeras disputas.", "La abundancia trajo el hambre de más." }),

        // ── El Alfarero (Neolítico, Cocina/Textil; arquetipo Canalizar/moldear; Tierra/Fuego) ──────────────
        Historico("El Alfarero", ElementalTone.Tierra,         // moldear el barro con fuego
            new[] { "Doy forma al barro con mis manos.", "El fuego endurece lo que moldeo.", "De la tierra, una vasija." },
            new[] { "El barro se me quiebra.", "El horno la partió.", "Mis manos no dan la forma." }),
        Historico("El Alfarero", ElementalTone.Tierra,         // la vasija sostiene a otros; frágil pero perdura
            new[] { "Mi vasija guarda el agua de otros.", "Creo cosas que sostienen a la tribu.", "Frágil como soy, mi obra perdura." },
            new[] { "Soy tan frágil como el barro.", "Sobrevivo en cosas, no en memoria.", "Nadie recordará al alfarero." }),

        // ── El Primer Herrero (Edad de los Metales, Forja; hilo B; Fuego/Tierra) — docs/forge-simulation.md ─
        Historico("El Primer Herrero", ElementalTone.Fuego,    // dominar la forja (bronce = cobre+estaño; luego hierro)
            new[] { "Fundo cobre y estaño en bronce.", "Del fuego saco herramientas nuevas.", "Mis arados hacen crecer la aldea." },
            new[] { "El metal exige más y más fuego.", "Me quemo por dar de comer a otros.", "La fragua nunca descansa." }),
        Historico("El Primer Herrero", ElementalTone.Fuego,    // el giro: la misma forja hace arado y espada
            new[] { "El que forja, elige qué forjar.", "Elijo el arado antes que la espada.", "Doy filo para labrar, no para herir." },
            new[] { "La misma forja hace la espada.", "Forjé el arma que nos dividió.", "El metal afiló la codicia." }),
        Historico("El Primer Herrero", ElementalTone.Tierra,   // herencia del fuego: la pirita esconde hierro
            new[] { "Heredé la chispa del que velaba el fuego.", "La piedra que escupe chispa guarda hierro.", "Del rescoldo a la fragua." },
            new[] { "El don del fuego ahora hiere de lejos.", "Cada filo pesa en mi conciencia.", "Doblé el metal y también a los hombres." }),

        // ── Sargón de Acad (Metales; hilo E, Corona y Espada; primer imperio; villano-capaz; Fuego/Tierra) ──
        Historico("Sargón de Acad", ElementalTone.Tierra,      // unir ciudades en el primer imperio
            new[] { "Uní ciudades dispersas en un imperio.", "Traje caminos, orden y ley.", "Mi nombre durará grabado en arcilla." },
            new[] { "Uní por la fuerza.", "El que no se somete, cae.", "El poder siempre pide más poder." }),
        Historico("Sargón de Acad", ElementalTone.Fuego,       // el nodo oscuro: dominio confundido con protección
            new[] { "Protejo a los míos con mano firme.", "Sin un fuerte, todo se desmorona.", "Doy paz, aunque sea a la fuerza." },
            new[] { "Someto para gobernar.", "Confundo el miedo con el respeto.", "Nadie recuerda a quién pisé para subir." }),

        // ── El Tallador (hilo B · Barro y Metal; ancla: primer útil de piedra → levantar refugio; Tierra) ──
        //    Base de la historia del área de CONSTRUCCIÓN. docs/construction-simulation.md
        Historico("El Tallador", ElementalTone.Tierra,         // tallar la piedra: de un canto, una herramienta
            new[] { "Doy forma a la piedra con paciencia.", "De un canto saco una herramienta.", "Mis manos moldean lo duro." },
            new[] { "La piedra se astilla mal.", "Golpeo y golpeo sin lograr filo.", "Lo duro no cede a la prisa." }),
        Historico("El Tallador", ElementalTone.Tierra,         // levantar el refugio: piedra sobre piedra, un hogar
            new[] { "Levanto muros contra el viento.", "Doy techo a los míos.", "Piedra sobre piedra, un hogar." },
            new[] { "El muro se me viene abajo.", "El techo no aguanta la lluvia.", "Cargo piedras hasta el agotamiento." }),
        Historico("El Tallador", ElementalTone.Tierra,         // el giro: el muro une, pero también separa
            new[] { "El muro nos guarece a todos.", "Construir es cuidar.", "Dentro cabemos los que somos." },
            new[] { "El muro que abriga también divide.", "Tras la cerca nace lo 'mío'.", "Levanté la primera frontera." }),

        // ── La Recolectora (hilo C, ancla; era PRE-FUEGO: plantas que alimentan y curan; Agua/Tierra) ──────
        //    Raíz de la Enfermería/medicina; en el Neolítico se vuelve La Sembradora. docs/area-progression §Pre-fuego
        Historico("La Recolectora", ElementalTone.Agua,        // conocer las plantas que alimentan y curan
            new[] { "Sé qué planta alimenta y cuál cura.", "La tierra guarda remedios.", "Llevo conmigo la que me salva." },
            new[] { "Confundí una planta y enfermé.", "El bosque también esconde venenos.", "Sin saber, cada hoja es un riesgo." }),
        Historico("La Recolectora", ElementalTone.Agua,        // aprender de los animales (zoofarmacognosia)
            new[] { "Vi al enfermo comer una hoja y sanar.", "Los animales me enseñan sus remedios.", "Observo y aprendo del que sufre." },
            new[] { "No siempre entiendo lo que veo.", "A veces la cura llega tarde.", "La naturaleza no da manual." }),
        Historico("La Recolectora", ElementalTone.Agua,        // cuidar al que cae (raíz de la Enfermería/compasión)
            new[] { "Cuido al herido hasta que sana.", "Nadie del grupo queda sin ayuda.", "Sanar es acompañar." },
            new[] { "A veces no basta con cuidar.", "Pierdo a quien no pude salvar.", "Cargo el dolor de los que se van." }),

        // ── El Perro de Oberkassel (real, ~14.200 años; PRIMER PERRO-compañero; POV de un ANIMA-perro; Agua) ──
        //    Mito fundacional de la cría/santuario: cachorro salvado del moquillo por amor, no por utilidad.
        //    docs/cria-simulation.md · founding: cuidar al débil e "inútil" funda el vínculo.
        Historico("El Perro de Oberkassel", ElementalTone.Agua,   // la enfermedad: casi muere, lo mantienen vivo
            new[] { "Sobreviví a la fiebre que casi me lleva.", "Unas manos me mantuvieron caliente.", "Volví del borde gracias a ellos." },
            new[] { "Ardía y temblaba, sin poder moverme.", "Enfermo no servía de nada, solo estorbaba.", "Temí que me dejaran atrás." }),
        Historico("El Perro de Oberkassel", ElementalTone.Agua,   // el vínculo (imprint): su manada son ellos
            new[] { "Ellos son mi manada ahora.", "Guardo su sueño y ellos el mío.", "Doy mi lealtad a quien me salvó." },
            new[] { "Temo el día en que ya no estén.", "Sin ellos no sé quién soy.", "Cargo el miedo de volver a estar solo." }),
        Historico("El Perro de Oberkassel", ElementalTone.Agua,   // el adiós: muere joven, enterrado con los suyos
            new[] { "Descanso junto a los que amé.", "Nos enterraron juntos, como manada.", "El cariño no cabe en una vida corta." },
            new[] { "Me fui demasiado pronto.", "No pude devolverles todo.", "Mi tiempo fue breve." }),

        // ── Animales-protagonista (reales; POV animal). Lista curada en docs/animal-heroes.md ──────────────
        // Togo (1925, carrera del suero a Nome) — heroísmo + reivindicación (hizo el tramo más duro; Balto se llevó la fama).
        Historico("Togo", ElementalTone.Viento,
            new[] { "Corrí en la ventisca por los que enferman.", "Guié al trineo por el hielo que cruje.", "Mis patas no se rindieron." },
            new[] { "El frío mordía cada paso.", "El hielo se abría bajo nosotros.", "Agotado, pero no paré." }),
        Historico("Togo", ElementalTone.Tierra,                   // la fama ajena → reivindicación
            new[] { "Hice el tramo más largo y duro.", "Sé lo que corrí, aunque otro brille.", "Con el tiempo contaron mi verdad." },
            new[] { "Otro perro se llevó la gloria.", "Corrí más y me olvidaron.", "Nadie recordó mi nombre al principio." }),

        // Hachikō (Japón, 1920s-30s) — lealtad y duelo (esperó ~9 años a su dueño fallecido en la estación).
        Historico("Hachikō", ElementalTone.Tierra,
            new[] { "Cada tarde voy a la estación a esperarlo.", "Sé que volverá; yo estaré.", "Mi lealtad no mide el tiempo." },
            new[] { "Ya no baja del tren.", "Espero y espero, y no llega.", "El andén se vacía sin él." }),
        Historico("Hachikō", ElementalTone.Agua,                  // los años / el legado
            new[] { "Mi espera enseñó lo que es el amor leal.", "La gente me trae calor mientras aguardo.", "Al fin descanso; quizá ya lo encuentro." },
            new[] { "Me hago viejo en el mismo andén.", "Nadie me explica por qué no vuelve.", "El invierno pesa en los huesos." }),

        // Cher Ami (paloma mensajera, WWI) — heroísmo/sacrificio (entregó el mensaje que salvó al "Batallón Perdido", herida).
        Historico("Cher Ami", ElementalTone.Viento,
            new[] { "Llevé el mensaje entre el fuego.", "Volé aunque me hirieran.", "Doscientos viven porque no caí." },
            new[] { "Las balas me alcanzaron.", "Perdí una pata y un ojo.", "Volé sangrando hasta entregar." }),
    };

    // Deseos base: genéricos, anónimos y REUTILIZABLES (los comparte cualquier ser). De lo simple a lo complejo.
    static MindPhrase Deseo(ElementalTone tone, string[] pos, string[] neg) =>
        new MindPhrase(tone, pos, neg, PhraseCategory.Deseo, randomAssignable: false, reusable: true, source: null);

    public static List<MindPhrase> Deseos() => new List<MindPhrase>
    {
        Deseo(ElementalTone.Tierra,   // comer
            new[] { "Tengo hambre.", "Busco algo que comer.", "Sacio y descanso." },
            new[] { "El hambre aprieta.", "No hay qué comer.", "Vacío por dentro." }),
        Deseo(ElementalTone.Viento,   // dormir
            new[] { "Necesito descansar.", "Cierro los ojos.", "Sueño y me voy lejos." },
            new[] { "No puedo dormir.", "La mente no calla.", "El sueño no llega." }),
        Deseo(ElementalTone.Tierra,   // trabajar
            new[] { "Hay trabajo que hacer.", "Me pongo manos a la obra.", "El deber me sostiene." },
            new[] { "Otra vez a la carga.", "El trabajo no acaba nunca.", "Solo sirvo para esto." }),
        Deseo(ElementalTone.Agua,     // cuidar
            new[] { "Alguien me necesita.", "Cuidar me da sentido.", "Doy lo que tengo." },
            new[] { "Cargo con todos.", "¿Quién me cuida a mí?", "Me olvido de mí cuidando." }),
        Deseo(ElementalTone.Agua,     // acompañar
            new[] { "No quiero estar solo.", "Busco a los míos.", "Juntos pesa menos." },
            new[] { "Me siento aparte.", "Nadie me busca.", "Sobro donde estoy." }),
        Deseo(ElementalTone.Fuego,    // poseer / dominar (el deseo complejo)
            new[] { "Quiero más.", "Esto puede ser mío.", "Extiendo mi voluntad." },
            new[] { "Nada me basta.", "Lo quiero todo.", "Me pierdo en el querer." }),
    };

    // Pensamientos BASE de especie: innatos, ligados a una especie por `source` (= su arquetipo). NO al azar
    // (los siembra el arquetipo de mente, no el reparto). Tono acorde al arquetipo de mente de cada especie.
    static MindPhrase Especie(string species, ElementalTone tone, string[] pos, string[] neg) =>
        new MindPhrase(tone, pos, neg, PhraseCategory.Especie, randomAssignable: false, reusable: true, source: species);

    public static List<MindPhrase> Especie() => new List<MindPhrase>
    {
        Especie("Human", ElementalTone.Viento,
            new[] { "Me pregunto qué habrá más allá.", "Puedo imaginarlo y construirlo.", "Se lo contaré a los demás." },
            new[] { "Nada tiene sentido.", "Doy vueltas sin avanzar.", "Me pierdo en mis propias ideas." }),
        Especie("Bear", ElementalTone.Tierra,
            new[] { "El bosque es mío.", "Camino sin prisa.", "Como, duermo, resisto." },
            new[] { "Alguien invade mi territorio.", "El hambre aprieta.", "Gruño y todo se aparta." }),
        Especie("Wolf", ElementalTone.Viento,
            new[] { "La manada es una sola voz.", "Corro con los míos.", "Rastreo y guío." },
            new[] { "Me han dejado atrás.", "Solo no soy nada.", "Aúllo y nadie responde." }),
        Especie("Bunny", ElementalTone.Agua,
            new[] { "Escucho el menor ruido.", "Me escondo y espero.", "Si huyo, vivo." },
            new[] { "Algo acecha.", "No hay dónde esconderse.", "El miedo me paraliza." }),
        Especie("Fox", ElementalTone.Fuego,
            new[] { "Hay un camino astuto.", "Observo antes de actuar.", "Consigo lo que quiero sin pelear." },
            new[] { "Me he pasado de listo.", "Nadie se fía de mí.", "Solo con mis trucos." }),
        Especie("Deer", ElementalTone.Agua,
            new[] { "Pasto atento.", "Alzo la cabeza al menor crujido.", "El grupo me da calma." },
            new[] { "Algo se mueve entre los árboles.", "Estoy expuesto.", "Corro sin mirar atrás." }),
        Especie("Seal", ElementalTone.Agua,
            new[] { "El agua es juego.", "Me deslizo y giro.", "Salgo a tomar el sol." },
            new[] { "Una sombra bajo el agua.", "El hielo se aleja.", "Torpe fuera del mar." }),
        Especie("Whale", ElementalTone.Agua,
            new[] { "Canto largo y hondo.", "El mar recuerda.", "Guío al banco por la corriente." },
            new[] { "Un silencio inmenso.", "Me he perdido del banco.", "El océano pesa." }),
        Especie("Penguin", ElementalTone.Agua,
            new[] { "Bajo al agua y vuelo.", "La colonia me abriga.", "Persigo el banco plateado." },
            new[] { "Una sombra veloz me ronda.", "Torpe sobre el hielo.", "He perdido a los míos." }),
        Especie("Orca", ElementalTone.Agua,
            new[] { "El pod caza como uno.", "Conozco cada corriente.", "Nada escapa a mi oído." },
            new[] { "Lejos de mi familia soy nada.", "El mar calla demasiado.", "He perdido el rastro." }),
        Especie("Lion", ElementalTone.Fuego,
            new[] { "Soy fuerza que descansa al sol.", "La manada me sigue.", "Cuando me alzo, todos miran." },
            new[] { "Alguien reta mi lugar.", "El hambre de la manada es mía.", "Rujo para que se aparten." }),
        Especie("Malamute", ElementalTone.Tierra,
            new[] { "Tiro con ganas.", "Mi humano es mi manada.", "El frío no me para." },
            new[] { "Me han dejado atado.", "Echo de menos a los míos.", "Aúllo a la puerta." }),
    };
}
