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
}
