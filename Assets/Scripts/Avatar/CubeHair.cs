using UnityEngine;

/// <summary>
/// ASPECTO de cada persona (cabello, piel, ojos, barba) para el cuerpo de cubos: el CABELLO se arma con cubos sobre la
/// cabeza (corto, largo, moño, coleta, rizado, rapado) y hay gorro de cocinero para los cocineros. La CARA (ojos,
/// párpados, cejas, nariz, mandíbula) la pone <see cref="CubeFace"/>; aquí se eligen sus colores por personaje.
/// </summary>
public static class CubeHair
{
    public enum Style { Short, Long, Bun, Ponytail, Curly, Shaved, ChefHat, Fade, LongWavy, Medium, Bob, Turban, Spiky, SweptBack, Quiff, TiedBack }
    public enum Beard { None, Full, Goatee, Stubble, Long, Moustache, ThinMoustache }
    /// <summary>Dibujo de la tela de la camisa o el jersey: cuadros, rayas finas o canalé (punto grueso).</summary>
    public enum Weave { None, Checks, Stripes, Ribs }

    public struct Look
    {
        public Style style; public Color hair, skin, eyes; public bool beard;
        public float height;                 // estatura en metros (0 = la que ya tenía)
        public Color clothes; public bool hasClothes;
        // Detalles de cada persona (de sus fotos de referencia): pantalón, mangas largas, pantalón corto, calzado, gafas,
        // gorra con su letra, tipo de barba, tatuajes en los antebrazos y pendiente.
        public Color pants; public bool hasPants;
        public Color sleeves; public bool hasSleeves;   // antebrazos cubiertos (camiseta interior de manga larga)
        public bool shorts;                              // pantalón corto: espinillas al aire
        public Color shoes; public bool hasShoes;
        public Color glasses; public bool hasGlasses;
        public Color cap, capLogo; public bool hasCap;
        public Beard beardStyle;
        public bool tattoos, earring;
        public Color beanie; public bool hasBeanie;          // gorro de lana
        public bool beret;                                   // boina (con el color de beanie): plana y ladeada
        public Color lips; public bool hasLips;              // labios pintados
        // Capas de ropa del torso: CHALECO (pecho) sobre camiseta (mangas cortas de otro color), CORSÉ en la cintura, BOTAS altas.
        public Color upperSleeves, waist, boots; public bool hasUpperSleeves, hasWaist, hasBoots;
        public bool upperTattoo, wristband, chain, rifle;   // tatuaje en el brazo, muñequera, cadena de cartera, rifle a la espalda
        public bool feminine;                                // cejas finas arqueadas y pestañas/delineado
        public float jaw, cheeks;                            // forma de la cara: ancho de mandíbula (0,68–0,92) y pómulos (0–1)
        public bool sleeveless;                              // sin mangas: brazos al aire (camiseta de tirantes)
        public bool rings;                                   // anillos en los dedos
        public Color stole; public bool hasStole;            // chal/estola cruzada sobre el hombro
        public Color necklace; public bool hasNecklace, necklaceCross;   // collar (cadena con cruz o guirnalda/mala)
        // (2026-11-04: la familia y los amigos de Berón, de sus fotos; docs/diseno-personajes.md §Familia y amigos)
        public Color beardTint; public bool hasBeardTint;    // bigote de otro color que el pelo (canas)
        public Color shadesFrame, shadesLens; public bool hasShades, aviator;   // gafas de sol: envolventes o de aviador
        public Color watch, watchFace; public bool hasWatch;  // reloj de pulsera (muñeca izquierda)
        public Color band;                                   // color de la muñequera (sin él, negra)
        public Color clipA, clipB; public bool hasClip;      // pinza de flor en el pelo
        public bool bothEars, hoops;                         // pendientes en las dos orejas; de aro
        public Color bag; public bool hasBag;                // bolso cruzado
        public Color belt; public bool hasBelt;              // cinturón
        public Color skirt, skirtPrint; public bool hasSkirt;   // falda con estampado de flores
        public bool ruffles;                                 // volantes en el pecho
        public Color inner, tee; public bool jacket, jacketClosed, hasTee;   // chaqueta con cremallera sobre camisa (y camiseta)
        public Color hoodColor; public bool hood;            // capucha caída a la espalda
        public Color pattern; public Weave weave;            // cuadros, rayas o canalé de la camisa o el jersey
        public bool rolledSleeves, collar, openCollar;       // mangas remangadas; cuello de camisa (abierto)
        public Color hangingGlasses; public bool hasHangingGlasses;   // gafas colgadas de la camisa
        public bool beads;                                   // collar de cuentas de un solo color
        public Look(Style s, Color h, Color sk, Color e, bool b = false)
        {
            this = default;
            style = s; hair = h; skin = sk; eyes = e; beard = b; height = 0f; clothes = default; hasClothes = false;
            pants = sleeves = shoes = glasses = cap = capLogo = default; hasPants = hasSleeves = shorts = hasShoes = hasGlasses = hasCap = false;
            beardStyle = b ? Beard.Full : Beard.None; tattoos = earring = false;
            beanie = lips = upperSleeves = waist = boots = default; hasBeanie = hasLips = hasUpperSleeves = hasWaist = hasBoots = false;
            upperTattoo = wristband = chain = rifle = false; feminine = false; jaw = cheeks = 0f;
            sleeveless = hasStole = hasNecklace = necklaceCross = rings = false; beret = false; stole = necklace = default;
        }
        public Look Wear(Color c, float m = 0f) { clothes = c; hasClothes = true; height = m; return this; }
        public Look Pants(Color c, bool shortPants = false) { pants = c; hasPants = true; shorts = shortPants; return this; }
        public Look Sleeves(Color c) { sleeves = c; hasSleeves = true; return this; }
        public Look Shoes(Color c) { shoes = c; hasShoes = true; return this; }
        public Look Glasses(Color c) { glasses = c; hasGlasses = true; return this; }
        public Look Cap(Color c, Color logo) { cap = c; capLogo = logo; hasCap = true; return this; }
        public Look Goatee() { beard = true; beardStyle = Beard.Goatee; return this; }
        public Look Shaven() { beard = false; beardStyle = Beard.None; return this; }
        public Look Tattoos() { tattoos = true; return this; }
        public Look Earring() { earring = true; return this; }
        public Look Beanie(Color c) { beanie = c; hasBeanie = true; return this; }
        public Look Beret(Color c) { beanie = c; beret = true; return this; }
        public Look Fem() { feminine = true; return this; }
        public Look Face(float jawWidth, float cheekbones = 0f) { jaw = jawWidth; cheeks = cheekbones; return this; }
        public Look Lips(Color c) { lips = c; hasLips = true; return this; }
        public Look Stubble() { beard = true; beardStyle = Beard.Stubble; return this; }
        public Look Sleeveless() { sleeveless = true; return this; }
        public Look Rings() { rings = true; return this; }
        public Look Stole(Color c) { stole = c; hasStole = true; return this; }
        public Look Necklace(Color c, bool cross = false) { necklace = c; hasNecklace = true; necklaceCross = cross; return this; }
        public Look LongBeard() { beard = true; beardStyle = Beard.Long; return this; }   // barba larga y bigote poblados
        public Look Turban(Color cloth, Color stripe) { style = Style.Turban; cap = cloth; capLogo = stripe; return this; }   // turbante (no es gorra: hasCap sigue en false)
        public Look Vest(Color shirt) { upperSleeves = shirt; hasUpperSleeves = true; return this; }   // clothes = chaleco; shirt = camiseta
        public Look Corset(Color c) { waist = c; hasWaist = true; return this; }
        public Look Boots(Color c) { boots = c; hasBoots = true; return this; }
        public Look Moustache(Color tint, bool thin = false) { beard = true; beardStyle = thin ? Beard.ThinMoustache : Beard.Moustache; beardTint = tint; hasBeardTint = true; return this; }
        public Look Shades(Color frame, Color lens, bool aviatorShape = false) { shadesFrame = frame; shadesLens = lens; hasShades = true; aviator = aviatorShape; return this; }
        public Look Watch(Color strap, Color face) { watch = strap; watchFace = face; hasWatch = true; return this; }
        public Look Band(Color c) { wristband = true; band = c; return this; }
        public Look HairClip(Color a, Color b) { clipA = a; clipB = b; hasClip = true; return this; }
        public Look Earrings(bool hoop = false) { earring = true; bothEars = true; hoops = hoop; return this; }
        public Look Bag(Color c) { bag = c; hasBag = true; return this; }
        public Look Belt(Color c) { belt = c; hasBelt = true; return this; }
        /// <summary>Falda por encima de la rodilla: cadera del color de la falda, piernas al aire, flores de <paramref name="print"/>.</summary>
        public Look Skirt(Color c, Color print) { skirt = c; skirtPrint = print; hasSkirt = true; pants = c; hasPants = true; shorts = true; return this; }
        public Look Ruffles() { ruffles = true; return this; }
        /// <summary>Chaqueta con cremallera (el color de <see cref="Wear"/>: llamar después) sobre una camisa <paramref name="shirt"/>.
        /// Abierta, la camisa se ve por delante; cerrada, solo en el cuello. <paramref name="undershirt"/>: la camiseta que asoma.</summary>
        public Look Jacket(Color shirt, Color? undershirt = null, bool closed = false) { jacket = true; inner = shirt; jacketClosed = closed; sleeves = clothes; hasSleeves = true; if (undershirt.HasValue) { tee = undershirt.Value; hasTee = true; } return this; }
        public Look Hood(Color c) { hood = true; hoodColor = c; return this; }
        public Look Pattern(Color line, Weave kind) { pattern = line; weave = kind; return this; }
        public Look RolledSleeves() { rolledSleeves = true; return this; }
        public Look Collar(bool open = false) { collar = true; openCollar = open; return this; }
        public Look HangingGlasses(Color c) { hangingGlasses = c; hasHangingGlasses = true; return this; }
        public Look Beads(Color c) { necklace = c; hasNecklace = true; necklaceCross = false; beads = true; return this; }
        public Look Extras(bool upperTat = false, bool band = false, bool walletChain = false, bool backRifle = false) { upperTattoo = upperTat; wristband = band; chain = walletChain; rifle = backRifle; return this; }
    }

    static Color C(float r, float g, float b) => new Color(r, g, b);
    static readonly Color SkinLight = C(0.93f, 0.8f, 0.7f), SkinMid = C(0.84f, 0.66f, 0.52f), SkinTan = C(0.72f, 0.54f, 0.4f), SkinDark = C(0.5f, 0.36f, 0.26f);

    /// <summary>El aspecto de una persona por su nombre (los del santuario, ver docs/diseno-personajes.md); el resto,
    /// uno variado por su nombre.</summary>
    public static Look For(string name)
    {
        string n = (name ?? "").ToLowerInvariant();
        // Kushal: nombre de origen nepalí/indio (inferido; la ficha no da nacionalidad). 1,75 m (DEVLOG).
        // (de su hoja de diseño) media melena negra, gorro de lana rojo, gafas redondas, barba de unos días; chaleco morado con
        // borrego sobre camiseta azul marino, vaqueros verde oscuro rotos con cadena, muñequera, zapatillas grises de bota,
        // tatuaje en el brazo y un rifle colgado a la espalda. Piel clara rosada.
        if (n.Contains("player") || n.Contains("kushal")) return new Look(Style.Medium, C(0.07f, 0.06f, 0.07f), C(0.84f, 0.66f, 0.6f), C(0.22f, 0.16f, 0.12f)).Face(0.74f, 0.3f).Stubble().Beanie(C(0.55f, 0.1f, 0.14f)).Glasses(C(0.3f, 0.3f, 0.32f)).Wear(C(0.36f, 0.12f, 0.45f)).Vest(C(0.1f, 0.1f, 0.33f)).Pants(C(0.08f, 0.28f, 0.22f)).Shoes(C(0.62f, 0.64f, 0.68f)).Extras(upperTat: true, band: true, walletChain: true, backRifle: true);
        // La Magnate: sin ficha de aspecto; maga de células infinitas → canas plateadas, túnica violeta de maestra.
        // (de su hoja de diseño) bob azul noche con flequillo recto y mechones largos a los lados, ojos marrones, labios
        // morados; blusa blanca de cuello alto, bolero morado de mangas abullonadas, corsé granate con ribete dorado,
        // pantalón bombacho celeste y botas altas azul marino con cordones. Piel clara amarillenta.
        if (n.Contains("magnate")) return new Look(Style.Bob, C(0.13f, 0.2f, 0.3f), C(0.95f, 0.87f, 0.68f), C(0.32f, 0.2f, 0.1f)).Fem().Face(0.66f, 0.6f).Lips(C(0.35f, 0.22f, 0.45f)).Wear(C(0.95f, 0.95f, 0.95f), 1.66f).Vest(C(0.5f, 0.15f, 0.42f)).Sleeves(C(0.5f, 0.15f, 0.42f)).Corset(C(0.4f, 0.16f, 0.13f)).Pants(C(0.72f, 0.85f, 0.9f)).Boots(C(0.15f, 0.17f, 0.3f));
        // Linduarda: ecuatoriana, médica, "a la moda, deportista, gimnasio a diario" → coleta alta, bata blanca, atlética.
        // (de su foto) coleta alta negra, gafas redondas doradas, pijama sanitario azul celeste sobre camiseta blanca de manga larga.
        if (n.Contains("linduarda")) return new Look(Style.Ponytail, C(0.08f, 0.06f, 0.06f), C(0.86f, 0.68f, 0.55f), C(0.25f, 0.16f, 0.1f)).Fem().Face(0.72f, 0.7f).Lips(C(0.7f, 0.38f, 0.4f)).Wear(C(0.52f, 0.72f, 0.9f), 1.6f).Pants(C(0.52f, 0.72f, 0.9f)).Sleeves(C(0.95f, 0.95f, 0.93f)).Glasses(C(0.85f, 0.7f, 0.4f)).Shoes(C(0.95f, 0.95f, 0.95f));
        // Lizmibet: 70+ años, ecuatoriana, médica, gran amiga de Irosene. (de su foto) pelo corto rizado y voluminoso castaño
        // cobrizo, gafas finas plateadas, piel clara, vestido blanco campesino de manga larga hasta los tobillos, sandalias blancas.
        if (n.Contains("lizmibet")) return new Look(Style.Curly, C(0.6f, 0.32f, 0.2f), C(0.93f, 0.77f, 0.67f), C(0.3f, 0.2f, 0.12f)).Fem().Face(0.84f, 0.3f).Lips(C(0.78f, 0.5f, 0.52f)).Glasses(C(0.72f, 0.72f, 0.75f)).Wear(C(0.97f, 0.97f, 0.95f), 1.55f).Pants(C(0.97f, 0.97f, 0.95f)).Sleeves(C(0.97f, 0.97f, 0.95f)).Shoes(C(0.95f, 0.95f, 0.95f));
        // Su esposo, Tofelite «TOTE», JEFE DE CONSTRUCCIÓN. (de su foto, tote.jpg, 2026-11-04; antes, de la foto con Lizmibet: polo
        // gris y bermudas) piel morena, pelo corto oscuro, GAFAS DE SOL ENVOLVENTES oscuras, camisa rosa pálido de RAYAS FINAS con
        // las mangas REMANGADAS y el cuello ABIERTO, pantalón gris de vestir con CINTURÓN oscuro, zapatos marrones de cordones y
        // RELOJ DORADO en la muñeca izquierda. Robusto (sus stats).
        if (n.Contains("jefeconstruccion") || n.Contains("jefe_construccion")) return new Look(Style.Short, C(0.12f, 0.1f, 0.1f), C(0.78f, 0.58f, 0.45f), C(0.2f, 0.14f, 0.1f)).Face(0.88f, 0.4f).Shades(C(0.1f, 0.1f, 0.11f), C(0.16f, 0.12f, 0.12f)).Wear(C(0.93f, 0.82f, 0.86f), 1.68f).Pattern(C(0.78f, 0.52f, 0.62f), Weave.Stripes).RolledSleeves().Collar(true).Pants(C(0.56f, 0.55f, 0.53f)).Belt(C(0.16f, 0.13f, 0.18f)).Shoes(C(0.4f, 0.28f, 0.18f)).Watch(C(0.85f, 0.72f, 0.38f), C(0.95f, 0.9f, 0.75f));
        // Sadhguru: da las clases en el centro de yoga. (de su foto) turbante crema con franja granate, barba larga blanca
        // y bigote poblado, piel morena, camiseta sin mangas verde agua, dhoti blanco y chal blanco con ribete turquesa,
        // guirnalda de flores al cuello, descalzo.
        if (n.Contains("sadhguru")) return new Look(Style.Short, C(0.92f, 0.92f, 0.9f), C(0.56f, 0.39f, 0.28f), C(0.15f, 0.1f, 0.08f)).Turban(C(0.95f, 0.93f, 0.87f), C(0.5f, 0.15f, 0.25f)).LongBeard().Face(0.86f, 0.5f).Wear(C(0.5f, 0.72f, 0.62f), 1.72f).Sleeveless().Pants(C(0.96f, 0.96f, 0.94f)).Stole(C(0.97f, 0.97f, 0.96f)).Necklace(C(0.9f, 0.82f, 0.5f)).Shoes(C(0.56f, 0.39f, 0.28f));
        // Andrea: italiano, uno de los DUEÑOS del santuario (docs/character-andrea.md). (de su foto) joven, pelo castaño oscuro
        // ondulado-rizado con volumen arriba, sonrisa amplia. Siempre ELEGANTE: camisa negra de cuello, pantalón de vestir gris
        // marengo y zapatos de piel; le gustan los anillos, las cadenas (plata, con cruz) y los pendientes; pulsera.
        if (n.Contains("andrea")) return new Look(Style.Curly, C(0.2f, 0.14f, 0.1f), C(0.9f, 0.72f, 0.6f), C(0.3f, 0.2f, 0.12f)).Face(0.82f, 0.45f).Earring().Rings().Wear(C(0.07f, 0.07f, 0.08f), 1.78f).Sleeves(C(0.07f, 0.07f, 0.08f)).Pants(C(0.22f, 0.23f, 0.26f)).Necklace(C(0.8f, 0.8f, 0.83f), true).Shoes(C(0.32f, 0.2f, 0.13f)).Extras(band: true);
        // LOS SEIS DEL MESOCOSMOS (docs/personajes-mesocosmos.md, Berón 2026-11-03). Rasgos públicos y reconocibles, sin caricatura.
        // Elon Musk (cohetes y coches eléctricos; en la Mecánica): 1,88 m, pelo castaño oscuro corto peinado hacia atrás, piel clara,
        // camiseta negra, vaqueros oscuros y zapatillas negras.
        if (n.Contains("elonmusk") || n.Contains("elon musk")) return new Look(Style.Short, C(0.24f, 0.17f, 0.12f), C(0.93f, 0.79f, 0.69f), C(0.35f, 0.45f, 0.42f)).Face(0.86f, 0.4f).Wear(C(0.08f, 0.08f, 0.09f), 1.88f).Pants(C(0.14f, 0.16f, 0.22f)).Shoes(C(0.07f, 0.07f, 0.07f));
        // Goku (personaje de ficción de Dragon Ball; entrenador de artes marciales): pelo negro en grandes puntas, gi naranja con
        // camiseta azul debajo, cinturón y muñequeras azules y botas azules con ribete rojo. Fornido (sus stats).
        if (n.Contains("goku")) return new Look(Style.Spiky, C(0.05f, 0.05f, 0.06f), C(0.95f, 0.8f, 0.68f), C(0.08f, 0.08f, 0.1f)).Face(0.84f, 0.5f).Wear(C(0.95f, 0.47f, 0.12f), 1.75f).Vest(C(0.12f, 0.25f, 0.62f)).Corset(C(0.12f, 0.25f, 0.62f)).Pants(C(0.95f, 0.47f, 0.12f)).Boots(C(0.12f, 0.25f, 0.62f)).Extras(band: true);
        // Bill Gates (el software y la filantropía de la salud pública; en el Hospital, registros y vacunación): pelo castaño claro
        // con canas peinado de lado, gafas finas, jersey azul de cuello en pico sobre camisa clara, pantalón caqui.
        if (n.Contains("billgates") || n.Contains("bill gates")) return new Look(Style.Short, C(0.6f, 0.5f, 0.4f), C(0.95f, 0.8f, 0.72f), C(0.3f, 0.38f, 0.48f)).Face(0.8f, 0.3f).Glasses(C(0.55f, 0.55f, 0.58f)).Wear(C(0.22f, 0.32f, 0.55f), 1.77f).Sleeves(C(0.22f, 0.32f, 0.55f)).Pants(C(0.72f, 0.65f, 0.5f)).Shoes(C(0.3f, 0.2f, 0.14f));
        // Warren Buffett (inversor; las cuentas del santuario, desde el restaurante): canoso, gafas de montura gruesa oscura, traje
        // gris con camisa blanca, zapatos negros. Mayor (sus stats).
        if (n.Contains("warrenbuffett") || n.Contains("warren buffett")) return new Look(Style.Short, C(0.88f, 0.88f, 0.86f), C(0.94f, 0.79f, 0.72f), C(0.35f, 0.3f, 0.25f)).Face(0.88f, 0.35f).Glasses(C(0.12f, 0.1f, 0.1f)).Wear(C(0.42f, 0.43f, 0.46f), 1.78f).Vest(C(0.96f, 0.96f, 0.95f)).Sleeves(C(0.42f, 0.43f, 0.46f)).Pants(C(0.38f, 0.39f, 0.42f)).Shoes(C(0.06f, 0.06f, 0.06f));
        // MacKenzie Scott (novelista y filántropa; en el Huerto, alimentar a los visitantes): media melena castaña, blusa azul
        // claro, pantalón oscuro.
        if (n.Contains("mackenziescott") || n.Contains("mackenzie scott")) return new Look(Style.Medium, C(0.4f, 0.27f, 0.17f), C(0.95f, 0.8f, 0.7f), C(0.3f, 0.22f, 0.14f)).Fem().Face(0.74f, 0.55f).Lips(C(0.78f, 0.5f, 0.5f)).Wear(C(0.62f, 0.75f, 0.88f), 1.7f).Sleeves(C(0.62f, 0.75f, 0.88f)).Pants(C(0.15f, 0.17f, 0.22f)).Shoes(C(0.2f, 0.15f, 0.12f));
        // Ibrahim Traoré (capitán, presidente de transición de Burkina Faso; coordina la Guardia): piel oscura, pelo negro muy corto
        // y barba corta, boina roja, uniforme de campaña verde oliva y botas.
        if (n.Contains("ibrahimtraore") || n.Contains("ibrahim traoré") || n.Contains("ibrahim traore")) return new Look(Style.Shaved, C(0.05f, 0.04f, 0.04f), C(0.36f, 0.24f, 0.17f), C(0.12f, 0.08f, 0.06f)).Face(0.86f, 0.55f).Stubble().Beret(C(0.72f, 0.1f, 0.12f)).Wear(C(0.33f, 0.36f, 0.24f), 1.8f).Sleeves(C(0.33f, 0.36f, 0.24f)).Pants(C(0.3f, 0.33f, 0.22f)).Boots(C(0.12f, 0.1f, 0.08f));
        // FAMILIA Y AMIGOS DE BERÓN (2026-11-04, de sus fotos; docs/diseno-personajes.md §Familia y amigos). Personas reales:
        // solo rasgos visibles (pelo, piel, ropa, complementos), con respeto y sin caricatura. Estaturas estimadas.
        // Nesvitor (Néstor): piel curtida morena, pelo canoso y negro peinado hacia atrás, BIGOTE espeso canoso; forro polar negro
        // con cremallera ABIERTO sobre camisa blanca de CUADROS finos azules y camiseta amarilla; las gafas colgadas de la camisa.
        if (n.Contains("nesvitor")) return new Look(Style.SweptBack, C(0.3f, 0.29f, 0.28f), C(0.7f, 0.5f, 0.38f), C(0.2f, 0.14f, 0.1f)).Face(0.84f, 0.5f).Moustache(C(0.6f, 0.58f, 0.56f)).Wear(C(0.1f, 0.1f, 0.11f), 1.68f).Jacket(C(0.95f, 0.95f, 0.97f), C(0.98f, 0.84f, 0.2f)).Pattern(C(0.55f, 0.66f, 0.88f), Weave.Checks).HangingGlasses(C(0.72f, 0.66f, 0.5f)).Pants(C(0.22f, 0.22f, 0.25f)).Shoes(C(0.12f, 0.1f, 0.09f));
        // Amykhuz (antes «Amahuz»; Berón 2026-11-04; pareja de Karen, confirmado): delgado, piel clara morena, pelo castaño oscuro, BIGOTE fino; polo azul marino de manga corta
        // (cuello cerrado), pantalón oscuro y BOLSO negro cruzado.
        if (n.Contains("amykhuz") || n.Contains("amahuz")) return new Look(Style.Short, C(0.2f, 0.13f, 0.09f), C(0.86f, 0.68f, 0.55f), C(0.22f, 0.15f, 0.1f)).Face(0.78f, 0.45f).Moustache(C(0.24f, 0.17f, 0.12f), true).Wear(C(0.1f, 0.12f, 0.22f), 1.76f).Collar().Pants(C(0.08f, 0.08f, 0.1f)).Bag(C(0.06f, 0.06f, 0.07f)).Shoes(C(0.07f, 0.07f, 0.08f));
        // Karen (pareja de Amykhuz): piel morena cálida, melena larga castaña oscura con una PINZA DE FLOR rosa y amarilla,
        // pendientes pequeños; blusa blanca sin mangas con VOLANTES, FALDA berenjena con flores rosas y lilas, RELOJ negro.
        if (n.Contains("karen")) return new Look(Style.Long, C(0.22f, 0.14f, 0.09f), C(0.74f, 0.53f, 0.4f), C(0.2f, 0.13f, 0.08f)).Fem().Face(0.78f, 0.55f).Lips(C(0.72f, 0.42f, 0.42f)).HairClip(C(0.95f, 0.55f, 0.72f), C(0.98f, 0.85f, 0.3f)).Earrings().Wear(C(0.97f, 0.96f, 0.95f), 1.58f).Sleeveless().Ruffles().Skirt(C(0.24f, 0.12f, 0.18f), C(0.88f, 0.58f, 0.82f)).Watch(C(0.08f, 0.08f, 0.09f), C(0.2f, 0.2f, 0.22f)).Shoes(C(0.16f, 0.1f, 0.1f));
        // Alicita (Alicia): piel clara morena, melena larga lisa negra, ARETES de aro pequeños; chaqueta negra con cremallera
        // (cerrada) y el cuello verde fosforito de la camiseta asomando. Sonriente.
        if (n.Contains("alicita")) return new Look(Style.Long, C(0.06f, 0.05f, 0.06f), C(0.88f, 0.7f, 0.58f), C(0.18f, 0.12f, 0.08f)).Fem().Face(0.8f, 0.55f).Lips(C(0.75f, 0.45f, 0.45f)).Earrings(true).Wear(C(0.12f, 0.13f, 0.15f), 1.58f).Jacket(C(0.62f, 0.95f, 0.15f), null, true).Pants(C(0.12f, 0.12f, 0.15f)).Shoes(C(0.1f, 0.1f, 0.11f));
        // Ducristin («semigod»): piel morena, pelo corto castaño oscuro LEVANTADO por delante (tupé), GAFAS DE AVIADOR de cristal
        // degradado; camisa azul clara de CUADROS con cuello abierto y MUÑEQUERA roja.
        if (n.Contains("ducristin")) return new Look(Style.Quiff, C(0.2f, 0.13f, 0.09f), C(0.78f, 0.58f, 0.45f), C(0.2f, 0.13f, 0.08f)).Face(0.86f, 0.5f).Shades(C(0.75f, 0.75f, 0.78f), C(0.36f, 0.22f, 0.26f), true).Wear(C(0.6f, 0.78f, 0.95f), 1.7f).Sleeves(C(0.6f, 0.78f, 0.95f)).Pattern(C(0.42f, 0.6f, 0.88f), Weave.Checks).Collar(true).Band(C(0.85f, 0.15f, 0.2f)).Pants(C(0.25f, 0.27f, 0.32f)).Shoes(C(0.15f, 0.12f, 0.1f));
        // Josedrilo (cumbaya.jpg, a la IZQUIERDA; supuesto): melena larga castaña oscura RECOGIDA atrás; chaqueta de cuero negra
        // y pantalón granate.
        if (n.Contains("josedrilo")) return new Look(Style.TiedBack, C(0.22f, 0.15f, 0.1f), C(0.88f, 0.72f, 0.6f), C(0.22f, 0.15f, 0.1f)).Face(0.8f, 0.45f).Wear(C(0.1f, 0.09f, 0.09f), 1.75f).Jacket(C(0.2f, 0.2f, 0.22f)).Pants(C(0.45f, 0.2f, 0.2f)).Shoes(C(0.1f, 0.08f, 0.07f));
        // Maite (cumbaya.jpg, en el CENTRO; supuesto): melena larga ondulada castaña, piel clara; parka con CAPUCHA verde oliva
        // parduzco sobre una camiseta gris.
        if (n.Contains("maite")) return new Look(Style.LongWavy, C(0.4f, 0.26f, 0.16f), C(0.95f, 0.8f, 0.7f), C(0.25f, 0.17f, 0.1f)).Fem().Face(0.76f, 0.55f).Lips(C(0.78f, 0.5f, 0.5f)).Wear(C(0.36f, 0.33f, 0.24f), 1.65f).Jacket(C(0.5f, 0.5f, 0.52f)).Hood(C(0.36f, 0.33f, 0.24f)).Pants(C(0.15f, 0.15f, 0.18f)).Shoes(C(0.2f, 0.15f, 0.12f));
        // Shalva (cumbaya.jpg, a la DERECHA; supuesto): pelo corto castaño; jersey grueso de CANALÉ verde oliva y vaqueros
        // azul claro.
        if (n.Contains("shalva")) return new Look(Style.Short, C(0.32f, 0.22f, 0.14f), C(0.9f, 0.74f, 0.62f), C(0.25f, 0.18f, 0.12f)).Face(0.84f, 0.45f).Wear(C(0.3f, 0.33f, 0.15f), 1.72f).Sleeves(C(0.3f, 0.33f, 0.15f)).Pattern(C(0.2f, 0.22f, 0.09f), Weave.Ribs).Pants(C(0.62f, 0.7f, 0.8f)).Shoes(C(0.2f, 0.18f, 0.16f));
        // Adaguchi (confirmado por Berón el 2026-11-04): piel clara morena, pelo corto ondulado
        // castaño oscuro con volumen arriba, un PENDIENTE pequeño; sudadera negra SIN MANGAS con capucha y COLLAR de cuentas oscuras.
        if (n.Contains("adaguchi")) return new Look(Style.Fade, C(0.16f, 0.1f, 0.07f), C(0.88f, 0.7f, 0.58f), C(0.2f, 0.13f, 0.08f)).Face(0.84f, 0.5f).Earring().Wear(C(0.08f, 0.08f, 0.09f), 1.65f).Sleeveless().Hood(C(0.08f, 0.08f, 0.09f)).Beads(C(0.25f, 0.2f, 0.26f)).Pants(C(0.12f, 0.12f, 0.14f)).Shoes(C(0.1f, 0.1f, 0.1f));
        // Gallarle (2026-11-04, pareja de Ducristin): PENDIENTE DE FOTO. Aspecto neutro provisional, sin rasgos inventados ni género:
        // pelo corto castaño, piel media, camiseta gris azulada y pantalón oscuro, 1,70 m. Se cambia cuando Berón mande la foto.
        if (n.Contains("gallarle")) return new Look(Style.Short, C(0.3f, 0.22f, 0.15f), C(0.84f, 0.66f, 0.52f), C(0.25f, 0.18f, 0.12f)).Face(0.8f, 0.45f).Wear(C(0.45f, 0.5f, 0.55f), 1.7f).Pants(C(0.18f, 0.18f, 0.22f)).Shoes(C(0.15f, 0.13f, 0.12f));
        // Julian (pareja de Alicita; búlgaro) y Fausto (pareja de Irosene; mecánico), 2026-11-04: PENDIENTES DE FOTO. Aspecto neutro
        // provisional como el de Gallarle (sin rasgos inventados ni género); Fausto con ropa de taller.
        if (n.Contains("julian")) return new Look(Style.Short, C(0.28f, 0.2f, 0.14f), C(0.9f, 0.76f, 0.66f), C(0.3f, 0.24f, 0.18f)).Face(0.8f, 0.45f).Wear(C(0.4f, 0.45f, 0.52f), 1.75f).Pants(C(0.2f, 0.2f, 0.24f)).Shoes(C(0.15f, 0.13f, 0.12f));
        if (n.Contains("fausto")) return new Look(Style.Short, C(0.25f, 0.2f, 0.16f), C(0.82f, 0.64f, 0.5f), C(0.25f, 0.18f, 0.12f)).Face(0.84f, 0.45f).Wear(C(0.35f, 0.38f, 0.42f), 1.72f).Sleeves(C(0.35f, 0.38f, 0.42f)).Pants(C(0.3f, 0.32f, 0.36f)).Shoes(C(0.12f, 0.1f, 0.09f));
        // LOS SEIS NUEVOS (Berón 2026-11-04): David y Adonis (informática), Julito y Ale (biotecnología), Ivelyna (pediatra) y Luciana
        // (piloto). PENDIENTES DE FOTO: aspecto neutro provisional, como Gallarle, sin rasgos inventados. El rasgo femenino (Fem) solo
        // donde el nombre lo sugiere (Ivelyna, Luciana), PENDIENTE DE CONFIRMAR con Berón; Ale (Alejandro o Alejandra), sin él. Por nombre
        // EXACTO (David, Ale… son nombres que pueden salir en los microcosmos).
        if (n == "david" || n == "adonis" || n == "julito" || n == "ale" || n == "ivelyna" || n == "luciana") return Placeholder(n);
        // BRUCE LEE (docs/microcosmos-bruce-lee.md): figura histórica del microcosmos del Dojo. Sin parecido de cine: un hombre
        // delgado y atlético de ~1,72 m, pelo negro corto, ropa sencilla de entrenamiento negra (chaqueta de cuello chino, pantalón
        // y zapatillas de tela). La complexión delgada y fibrosa sale de sus stats (BodyShape), que pone su builder.
        if (n.Contains("bruce lee")) return new Look(Style.Short, C(0.05f, 0.05f, 0.06f), C(0.86f, 0.7f, 0.56f), C(0.15f, 0.1f, 0.07f)).Face(0.8f, 0.6f).Wear(C(0.1f, 0.1f, 0.11f), 1.72f).Sleeves(C(0.1f, 0.1f, 0.11f)).Pants(C(0.08f, 0.08f, 0.09f)).Shoes(C(0.12f, 0.12f, 0.12f));
        if (n.Contains("maestra")) return new Look(Style.Bun, C(0.7f, 0.7f, 0.72f), SkinLight, C(0.3f, 0.25f, 0.2f));
        // Goluis: 25 años, el de la fuerza (corpulento), jefe de cocina → cabeza rapada, barba, chaqueta de cocinero.
        // (de su foto) degradado con la parte de arriba en punta, perilla, pendiente, tatuajes en los brazos, polo claro.
        if (n.Contains("goluis")) return new Look(Style.Fade, C(0.1f, 0.08f, 0.07f), C(0.74f, 0.55f, 0.42f), C(0.18f, 0.12f, 0.08f)).Face(0.92f, 0.3f).Goatee().Earring().Tattoos().Wear(C(0.93f, 0.9f, 0.86f), 1.75f).Pants(C(0.2f, 0.22f, 0.28f));
        // Panterilia: 42 años, la de la percepción (delgada), alma con un 5 % de león → melena leonada, ojos ámbar.
        if (n.Contains("panterilia")) return new Look(Style.Long, C(0.66f, 0.46f, 0.22f), SkinMid, C(0.78f, 0.58f, 0.18f)).Fem().Face(0.76f, 0.5f).Wear(C(0.45f, 0.5f, 0.32f), 1.68f);
        // Gohageneis: 28 años, "el Papi", nómada desde Venezuela (Colombia, Perú, Ecuador, España, EE. UU.), ágil.
        // (de su foto) pelo corto bajo una gorra azul con la «V» amarilla y roja, sin barba, atlético, bañador rojo.
        if (n.Contains("gohageneis")) return new Look(Style.Fade, C(0.07f, 0.06f, 0.06f), C(0.66f, 0.46f, 0.33f), C(0.2f, 0.13f, 0.08f)).Face(0.84f, 0.5f).Cap(C(0.15f, 0.3f, 0.75f), C(0.98f, 0.8f, 0.15f)).Wear(C(0.95f, 0.95f, 0.93f), 1.76f).Pants(C(0.85f, 0.12f, 0.12f), true);
        // Irosene: 70+ años, ecuatoriana, madre de 9; hoy corre, escala y bucea; destaca en el textil → canas cortas, tejido andino.
        // (de su foto) melena larga ondulada rubia con canas y flequillo, piel clara, ojos azul grisáceo.
        if (n.Contains("irosene")) return new Look(Style.LongWavy, C(0.86f, 0.8f, 0.64f), C(0.93f, 0.76f, 0.66f), C(0.45f, 0.58f, 0.62f)).Fem().Face(0.9f, 0.4f).Lips(C(0.82f, 0.55f, 0.55f)).Wear(C(0.85f, 0.32f, 0.3f), 1.55f);
        if (n.Contains("cocinero")) return new Look(Style.ChefHat, C(0.25f, 0.18f, 0.12f), Pick(n, SkinLight, SkinMid, SkinTan, SkinDark), C(0.25f, 0.18f, 0.12f)).Wear(C(0.95f, 0.95f, 0.93f));
        Style[] st = { Style.Short, Style.Long, Style.Bun, Style.Ponytail, Style.Curly };
        Color[] hc = { C(0.15f, 0.1f, 0.08f), C(0.4f, 0.26f, 0.15f), C(0.85f, 0.7f, 0.4f), C(0.6f, 0.2f, 0.1f), C(0.55f, 0.55f, 0.58f) };
        int hsh = Mathf.Abs(n.GetHashCode());
        return new Look(st[hsh % st.Length], hc[(hsh / 7) % hc.Length], Pick(n, SkinLight, SkinMid, SkinTan, SkinDark), C(0.25f, 0.18f, 0.12f));
    }

    static Color Pick(string n, params Color[] c) => c[Mathf.Abs(n.GetHashCode() / 3) % c.Length];

    // Aspecto provisional de los pendientes de foto: pelo corto o media melena castaña, piel media, ropa lisa de un color por persona.
    static int Sum(string n) { int h = 0; foreach (char ch in n) h += ch; return h; }
    static Look Placeholder(string n)
    {
        Color[] wear = { C(0.4f, 0.45f, 0.52f), C(0.45f, 0.4f, 0.35f), C(0.35f, 0.45f, 0.4f), C(0.5f, 0.45f, 0.55f), C(0.55f, 0.5f, 0.42f), C(0.38f, 0.42f, 0.5f) };
        bool fem = n == "ivelyna" || n == "luciana";   // (por el nombre; pendiente de confirmar)
        Look l = new Look(fem ? Style.Medium : Style.Short, C(0.28f, 0.2f, 0.14f), C(0.86f, 0.7f, 0.58f), C(0.25f, 0.18f, 0.12f)).Face(fem ? 0.76f : 0.82f, 0.45f)
            .Wear(wear[Sum(n) % wear.Length], fem ? 1.63f : 1.74f).Pants(C(0.2f, 0.2f, 0.24f)).Shoes(C(0.15f, 0.13f, 0.12f));
        return fem ? l.Fem() : l;
    }

    /// <summary>Pone el pelo sobre la articulación de la cabeza (su cubo: <paramref name="size"/> centrado en <paramref name="center"/>).</summary>
    public static void Build(Transform head, Vector3 center, Vector3 size, Look look)
    {
        Transform h = new GameObject("Cabello").transform;
        h.SetParent(head, false);
        float top = center.y + size.y * 0.5f, back = center.z - size.z * 0.5f, w = size.x + 0.02f, d = size.z + 0.02f;
        Color c = look.hair;
        switch (look.style)
        {
            case Style.Shaved:
                P(h, new Vector3(0f, top + 0.004f, center.z - 0.01f), new Vector3(w - 0.02f, 0.01f, d - 0.04f), c);
                break;
            case Style.ChefHat:
                P(h, new Vector3(0f, top + 0.03f, center.z), new Vector3(w, 0.06f, d), Color.white);
                P(h, new Vector3(0f, top + 0.16f, center.z), new Vector3(w * 0.9f, 0.2f, d * 0.9f), Color.white);
                P(h, new Vector3(0f, top + 0.27f, center.z), new Vector3(w * 1.05f, 0.06f, d * 1.05f), Color.white);
                break;
            case Style.Fade:
                // DEGRADADO: laterales y nuca casi rapados (más oscuros, pegados) y arriba pelo corto en punta.
                Color side = Color.Lerp(c, look.skin, 0.45f);
                for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * (size.x * 0.5f + 0.004f), center.y + size.y * 0.22f, center.z - size.z * 0.1f), new Vector3(0.01f, size.y * 0.35f, size.z * 0.75f), side);
                P(h, new Vector3(0f, center.y + size.y * 0.22f, back - 0.004f), new Vector3(w - 0.02f, size.y * 0.35f, 0.01f), side);
                P(h, new Vector3(0f, top + 0.02f, center.z - 0.005f), new Vector3(w - 0.03f, 0.04f, d - 0.03f), c);
                for (int i = 0; i < 9; i++)   // puntas (la parte de arriba peinada hacia arriba)
                    P(h, new Vector3(((i % 3) - 1) * size.x * 0.28f, top + 0.05f + (i % 2) * 0.012f, center.z + ((i / 3) - 1) * size.z * 0.28f), new Vector3(0.05f, 0.04f, 0.05f), c);
                break;
            default:
                // Casquete + nuca (base de todos los peinados) y flequillo corto sobre la frente.
                P(h, new Vector3(0f, top + 0.025f, center.z - 0.005f), new Vector3(w, 0.05f, d), c);
                P(h, new Vector3(0f, center.y + size.y * 0.15f, back - 0.012f), new Vector3(w, size.y * 0.75f, 0.03f), c);
                for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * (size.x * 0.5f + 0.012f), center.y + size.y * 0.25f, center.z - size.z * 0.15f), new Vector3(0.03f, size.y * 0.5f, size.z * 0.6f), c);
                if (look.style == Style.Ponytail || look.style == Style.Bun || look.style == Style.SweptBack || look.style == Style.Quiff || look.style == Style.TiedBack)   // RECOGIDO o PEINADO HACIA ATRÁS: la frente despejada (sin flequillo)
                    P(h, new Vector3(0f, top - 0.004f, center.z + size.z * 0.5f - 0.03f), new Vector3(w * 0.88f, 0.022f, 0.05f), c);
                else if (look.style != Style.Bob)
                    P(h, new Vector3(0f, top - 0.02f, center.z + size.z * 0.5f - 0.005f), new Vector3(w * 0.85f, 0.05f, 0.025f), c);
                if (look.style == Style.Medium || look.style == Style.Bob)
                {
                    // MEDIA MELENA (por los hombros) o BOB (a la mandíbula, con mechones largos delante): cortinas a los lados y
                    // detrás; el bob con FLEQUILLO RECTO grueso sobre las cejas.
                    float len = look.style == Style.Medium ? 0.85f : 0.6f;
                    P(h, new Vector3(0f, center.y - size.y * (len * 0.5f - 0.25f), back - 0.02f), new Vector3(w * 1.05f, size.y * len, 0.04f), c);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        P(h, new Vector3(s * (size.x * 0.5f + 0.02f), center.y - size.y * (len * 0.5f - 0.3f), center.z - size.z * 0.05f), new Vector3(0.04f, size.y * len, size.z * 0.7f), c);
                        if (look.style == Style.Bob)   // mechones largos que enmarcan la cara POR FUERA (junto a las sienes), hasta la clavícula
                            P(h, new Vector3(s * (size.x * 0.5f + 0.028f), center.y - size.y * 0.38f, center.z + size.z * 0.3f), new Vector3(0.035f, size.y * 1.0f, 0.07f), c);
                    }
                    if (look.style == Style.Bob)   // FLEQUILLO de mechones sueltos e irregulares (no un bloque) que llega a las cejas
                    {
                        float[] lens = { 0.07f, 0.085f, 0.075f, 0.09f, 0.072f, 0.086f, 0.068f };
                        for (int i = 0; i < lens.Length; i++)
                        {
                            float x = (i - 3) / 3f * w * 0.42f;
                            P(h, new Vector3(x, top - 0.004f - lens[i] * 0.5f, center.z + size.z * 0.5f + 0.012f), new Vector3(w * 0.125f, lens[i], 0.022f), c);
                        }
                    }
                }
                if (look.style == Style.Long || look.style == Style.LongWavy)
                {
                    P(h, new Vector3(0f, center.y - size.y * 0.55f, back - 0.02f), new Vector3(w * 1.05f, size.y * 1.1f, 0.04f), c);
                    for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * (size.x * 0.5f + 0.015f), center.y - size.y * 0.3f, center.z - size.z * 0.1f), new Vector3(0.035f, size.y * 0.9f, size.z * 0.5f), c);
                }
                if (look.style == Style.LongWavy)
                {
                    // ONDAS: mechones que se abren y cierran hacia abajo, y FLEQUILLO largo sobre la frente.
                    for (int k = 0; k < 4; k++)
                        for (int s = -1; s <= 1; s += 2)
                            P(h, new Vector3(s * (size.x * 0.5f + 0.03f + (k % 2) * 0.025f), center.y - size.y * (0.05f + k * 0.28f), center.z - size.z * 0.15f), new Vector3(0.05f, size.y * 0.3f, size.z * 0.55f), c);
                    P(h, new Vector3(0f, top - 0.05f, center.z + size.z * 0.5f + 0.008f), new Vector3(w * 0.95f, 0.08f, 0.03f), c);
                }
                else if (look.style == Style.Bun)
                    P(h, new Vector3(0f, top - 0.01f, back - 0.05f), new Vector3(0.12f, 0.1f, 0.1f), c);
                else if (look.style == Style.SweptBack)
                {
                    // PEINADO HACIA ATRÁS: una capa algo abultada sobre la coronilla que baja hacia la nuca, y las sienes pegadas.
                    P(h, new Vector3(0f, top + 0.045f, center.z - 0.02f), new Vector3(w * 0.9f, 0.03f, d * 0.92f), c);
                    Transform swept = P(h, new Vector3(0f, top + 0.02f, back + 0.01f), new Vector3(w * 0.92f, 0.04f, 0.08f), c);
                    swept.localRotation = Quaternion.Euler(-30f, 0f, 0f);
                }
                else if (look.style == Style.Quiff)
                {
                    // TUPÉ: el pelo de delante levantado hacia arriba y atrás, más alto en la frente que en la coronilla.
                    Transform q = P(h, new Vector3(0f, top + 0.06f, center.z + size.z * 0.3f), new Vector3(w * 0.82f, 0.08f, 0.12f), c);
                    q.localRotation = Quaternion.Euler(-28f, 0f, 0f);
                    P(h, new Vector3(0f, top + 0.04f, center.z - size.z * 0.05f), new Vector3(w * 0.85f, 0.04f, d * 0.6f), c);
                }
                else if (look.style == Style.TiedBack)
                {
                    // MELENA RECOGIDA ATRÁS: cola baja en la nuca, por debajo de los hombros; la goma, más oscura.
                    P(h, new Vector3(0f, center.y - size.y * 0.05f, back - 0.03f), new Vector3(0.06f, 0.05f, 0.05f), c * 0.75f + new Color(0, 0, 0, 1));
                    Transform tail = P(h, new Vector3(0f, center.y - size.y * 0.55f, back - 0.045f), new Vector3(0.07f, size.y * 0.95f, 0.045f), c);
                    tail.localRotation = Quaternion.Euler(-6f, 0f, 0f);
                }
                else if (look.style == Style.Ponytail)
                {
                    // COLETA ALTA: el moño de la goma en la coronilla, atrás, y la cola cayendo por la nuca.
                    P(h, new Vector3(0f, top - 0.01f, back + 0.01f), new Vector3(0.07f, 0.06f, 0.07f), c * 0.8f + new Color(0, 0, 0, 1));
                    Transform tail = P(h, new Vector3(0f, center.y + size.y * 0.05f, back - 0.07f), new Vector3(0.06f, size.y * 0.85f, 0.05f), c);
                    tail.localRotation = Quaternion.Euler(-12f, 0f, 0f);
                }
                else if (look.style == Style.Curly)
                {
                    // RIZOS: rizos redondos (las piezas se suavizan hasta casi esferas) en dos coronas sobre la cabeza, más
                    // los de las sienes y la nuca, y unos sueltos que caen sobre la frente. Volumen, no un casco.
                    var rnd = new System.Random((int)(c.r * 1000) + (int)(c.g * 100));
                    for (int ring = 0; ring < 3; ring++)
                    {
                        int n = ring == 0 ? 6 : ring == 1 ? 12 : 16;
                        float rr = ring == 0 ? 0.18f : ring == 1 ? 0.38f : 0.5f, y = ring == 0 ? top + 0.05f : ring == 1 ? top + 0.025f : top - 0.035f;
                        for (int i = 0; i < n; i++)
                        {
                            float a = (i + ring * 0.5f) / n * Mathf.PI * 2f;
                            float sz = 0.055f + (float)rnd.NextDouble() * 0.025f;
                            P(h, new Vector3(Mathf.Cos(a) * size.x * rr, y + (float)rnd.NextDouble() * 0.015f, center.z - 0.01f + Mathf.Sin(a) * size.z * rr), Vector3.one * sz, Color.Lerp(c, Color.black, (float)rnd.NextDouble() * 0.25f));
                        }
                    }
                    for (int s = -1; s <= 1; s += 2)
                        for (int k = 0; k < 3; k++)   // sienes y detrás de las orejas
                            P(h, new Vector3(s * (size.x * 0.5f + 0.015f), center.y + size.y * (0.25f - k * 0.12f), center.z - size.z * (0.1f + k * 0.12f)), Vector3.one * 0.05f, c);
                    for (int k = -2; k <= 2; k++) P(h, new Vector3(k * size.x * 0.2f, center.y + size.y * 0.05f, back - 0.02f), Vector3.one * 0.06f, c);   // nuca
                    for (int k = -1; k <= 1; k++) P(h, new Vector3(k * size.x * 0.22f + 0.01f, top - 0.04f, center.z + size.z * 0.5f + 0.005f), new Vector3(0.045f, 0.055f, 0.04f), c);   // rizos sobre la frente
                }
                break;
            case Style.Turban:
                // TURBANTE: vueltas de tela envolviendo la cabeza (bandas superpuestas algo inclinadas), con una franja de color
                // que asoma y el remate cruzado delante. El pelo canoso asoma a los lados, sobre las orejas.
                Color cloth = look.cap.a > 0f ? look.cap : new Color(0.95f, 0.93f, 0.88f), stripe = look.capLogo.a > 0f ? look.capLogo : new Color(0.5f, 0.15f, 0.25f);
                for (int k = 0; k < 4; k++)
                {
                    float y = top - 0.04f + k * 0.035f, grow = 1.12f - k * 0.06f;
                    GameObject band = P(h, new Vector3(0f, y, center.z - 0.01f), new Vector3(w * grow, 0.05f, d * grow), cloth).gameObject;
                    band.transform.localRotation = Quaternion.Euler((k % 2 == 0 ? 8f : -6f), 0f, (k % 2 == 0 ? -4f : 5f));
                }
                P(h, new Vector3(0f, top + 0.1f, center.z - 0.01f), new Vector3(w * 0.8f, 0.05f, d * 0.8f), cloth);   // coronilla
                Transform st = P(h, new Vector3(size.x * 0.1f, top + 0.02f, center.z - 0.005f), new Vector3(w * 1.08f, 0.018f, d * 1.08f), stripe);
                st.localRotation = Quaternion.Euler(-10f, 0f, 12f);
                Transform knot = P(h, new Vector3(0f, top + 0.03f, center.z + d * 0.55f), new Vector3(w * 0.5f, 0.06f, 0.04f), cloth);
                knot.localRotation = Quaternion.Euler(0f, 0f, 35f);
                for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * (size.x * 0.5f + 0.01f), center.y + size.y * 0.12f, center.z - size.z * 0.12f), new Vector3(0.03f, size.y * 0.3f, size.z * 0.45f), c);   // canas sobre las orejas
                break;
            case Style.Spiky:
                // PUNTAS GRANDES (Goku): casquete y nuca, y mechones gruesos en punta que salen hacia arriba, a los lados y hacia
                // atrás en abanico; delante, tres mechones que caen sobre la frente.
                P(h, new Vector3(0f, top + 0.025f, center.z - 0.005f), new Vector3(w, 0.05f, d), c);
                P(h, new Vector3(0f, center.y + size.y * 0.2f, back - 0.012f), new Vector3(w, size.y * 0.6f, 0.03f), c);
                for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * (size.x * 0.5f + 0.012f), center.y + size.y * 0.3f, center.z - size.z * 0.15f), new Vector3(0.03f, size.y * 0.4f, size.z * 0.6f), c);
                for (int i = 0; i < 9; i++)
                {
                    float ang = -80f + i * 20f;   // abanico de −80° a +80° (de un lado al otro, por encima)
                    float len = 0.3f + (i % 2) * 0.08f + (i == 4 ? 0.06f : 0f);   // (2026-11-03: más largas; a 0,16–0,25 m quedaban como un mechón)
                    Transform spike = P(h, new Vector3(Mathf.Sin(ang * Mathf.Deg2Rad) * (size.x * 0.5f + len * 0.35f), top + 0.04f + Mathf.Cos(ang * Mathf.Deg2Rad) * len * 0.45f, center.z - size.z * 0.12f - (i % 3) * 0.04f), new Vector3(0.09f, len, 0.09f), c);
                    spike.localRotation = Quaternion.Euler(-18f - (i % 3) * 8f, 0f, -ang * 0.9f);
                }
                for (int k = -1; k <= 1; k++)   // mechones de la frente
                {
                    Transform f = P(h, new Vector3(k * size.x * 0.22f, top - 0.04f, center.z + size.z * 0.5f + 0.01f), new Vector3(0.05f, 0.1f, 0.03f), c);
                    f.localRotation = Quaternion.Euler(0f, 0f, k * 18f);
                }
                break;
        }
        if (look.style != Style.ChefHat && look.style != Style.Shaved && look.style != Style.Turban) Strands(h, center, size, c, look.style);
        float front = center.z + size.z * 0.5f;
        if (look.beardStyle == Beard.Full)
        {
            P(h, new Vector3(0f, center.y - size.y * 0.42f, center.z + size.z * 0.42f), new Vector3(size.x * 0.75f, size.y * 0.25f, 0.06f), c);
            for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * size.x * 0.42f, center.y - size.y * 0.2f, center.z + size.z * 0.3f), new Vector3(0.03f, size.y * 0.35f, 0.08f), c);
        }
        else if (look.beardStyle == Beard.Long)   // BARBA LARGA Y POBLADA con bigote espeso: cae hasta el pecho, ancha y ahuecada
        {
            var rb = new System.Random(7);
            for (int k = 0; k < 6; k++)
            {
                float y = center.y - size.y * (0.3f + k * 0.13f), wid = size.x * (0.95f - k * 0.08f);
                P(h, new Vector3(0f, y, front - 0.01f + k * 0.012f), new Vector3(wid, size.y * 0.18f, 0.07f + k * 0.008f), Color.Lerp(c, Color.white, (float)rb.NextDouble() * 0.15f));
                for (int s = -1; s <= 1; s += 2)   // mechones laterales sueltos
                    P(h, new Vector3(s * wid * 0.45f, y - 0.01f, front + k * 0.01f), new Vector3(0.035f, size.y * 0.2f, 0.05f), Color.Lerp(c, Color.gray, 0.12f));
            }
            for (int s = -1; s <= 1; s += 2)
            {
                P(h, new Vector3(s * size.x * 0.46f, center.y - size.y * 0.1f, center.z + size.z * 0.15f), new Vector3(0.04f, size.y * 0.5f, size.z * 0.6f), c);   // patillas unidas a la barba
                Transform mo = P(h, new Vector3(s * size.x * 0.14f, center.y - size.y * 0.2f, front + 0.02f), new Vector3(size.x * 0.32f, 0.03f, 0.03f), c);    // bigote
                mo.localRotation = Quaternion.Euler(0f, 0f, s * -18f);
            }
        }
        else if (look.beardStyle == Beard.Stubble)   // BARBA DE UNOS DÍAS: sombra fina en mentón y mejillas
        {
            Color sh = Color.Lerp(look.skin, c, 0.22f);   // sombra suave, no una máscara
            P(h, new Vector3(0f, center.y - size.y * 0.4f, front + 0.003f), new Vector3(size.x * 0.6f, size.y * 0.14f, 0.008f), sh);
            for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * size.x * 0.33f, center.y - size.y * 0.22f, front + 0.002f), new Vector3(size.x * 0.12f, size.y * 0.2f, 0.008f), sh);
        }
        else if (look.beardStyle == Beard.Goatee)   // PERILLA: solo en la barbilla
            P(h, new Vector3(0f, center.y - size.y * 0.44f, front + 0.005f), new Vector3(size.x * 0.32f, size.y * 0.16f, 0.03f), c);
        else if (look.beardStyle == Beard.Moustache || look.beardStyle == Beard.ThinMoustache)
        {
            // BIGOTE entre la nariz y la boca: espeso (con las puntas caídas a los lados de la boca) o fino (una sombra).
            Color mc = look.hasBeardTint ? look.beardTint : c;
            bool thick = look.beardStyle == Beard.Moustache;
            float mw = size.x * (thick ? 0.48f : 0.34f);
            if (!thick) mc = Color.Lerp(look.skin, mc, 0.55f);
            P(h, new Vector3(0f, center.y - size.y * 0.195f, front + (thick ? 0.014f : 0.004f)), new Vector3(mw, thick ? 0.035f : 0.012f, thick ? 0.03f : 0.006f), mc);
            if (thick)
                for (int s = -1; s <= 1; s += 2)
                    P(h, new Vector3(s * mw * 0.47f, center.y - size.y * 0.255f, front + 0.012f), new Vector3(0.024f, 0.04f, 0.024f), mc);
        }

        if (look.hasGlasses)   // GAFAS redondas (en cubos: dos aros cuadrados con su puente y patillas)
        {
            float ey = center.y + size.y * 0.12f, ex = size.x * 0.24f, r = Mathf.Min(size.x, size.y) * 0.16f, z = front + 0.02f;
            for (int s = -1; s <= 1; s += 2)
            {
                P(h, new Vector3(s * ex, ey + r, z), new Vector3(r * 2f, 0.013f, 0.012f), look.glasses);
                P(h, new Vector3(s * ex, ey - r, z), new Vector3(r * 2f, 0.013f, 0.012f), look.glasses);
                P(h, new Vector3(s * (ex + r), ey, z), new Vector3(0.013f, r * 2f, 0.012f), look.glasses);
                P(h, new Vector3(s * (ex - r), ey, z), new Vector3(0.013f, r * 2f, 0.012f), look.glasses);
                P(h, new Vector3(s * (size.x * 0.5f + 0.005f), ey, center.z + size.z * 0.15f), new Vector3(0.006f, 0.008f, size.z * 0.7f), look.glasses);
            }
            P(h, new Vector3(0f, ey + r * 0.3f, z), new Vector3(ex * 2f - r * 2f, 0.008f, 0.008f), look.glasses);
        }

        if (look.hasCap)   // GORRA de béisbol: copa, visera hacia delante y su letra
        {
            P(h, new Vector3(0f, top + 0.05f, center.z - 0.005f), new Vector3(w + 0.01f, 0.1f, d + 0.01f), look.cap);
            P(h, new Vector3(0f, top + 0.01f, front + 0.06f), new Vector3(w * 0.85f, 0.015f, 0.12f), look.cap);
            P(h, new Vector3(-0.015f, top + 0.06f, front + 0.008f), new Vector3(0.012f, 0.05f, 0.005f), look.capLogo);   // «V»
            P(h, new Vector3(0.015f, top + 0.06f, front + 0.008f), new Vector3(0.012f, 0.05f, 0.005f), look.capLogo);
            P(h, new Vector3(0f, top + 0.035f, front + 0.009f), new Vector3(0.014f, 0.012f, 0.005f), new Color(0.85f, 0.15f, 0.15f));
        }

        if (look.hasBeanie)   // GORRO DE LANA con el borde doblado
        {
            P(h, new Vector3(0f, top + 0.06f, center.z - 0.005f), new Vector3(w + 0.015f, 0.12f, d + 0.015f), look.beanie);
            P(h, new Vector3(0f, top + 0.005f, center.z - 0.005f), new Vector3(w + 0.03f, 0.045f, d + 0.03f), look.beanie * 0.85f + new Color(0, 0, 0, 1));
            for (int k = -2; k <= 2; k++) P(h, new Vector3(k * w * 0.18f, top + 0.06f, center.z + d * 0.5f + 0.01f), new Vector3(0.008f, 0.1f, 0.005f), look.beanie * 0.7f + new Color(0, 0, 0, 1));   // canalé
        }

        if (look.beret)   // BOINA: un disco plano de fieltro, ladeado hacia la derecha, con su cinta de cuero
        {
            Transform disc = P(h, new Vector3(-size.x * 0.12f, top + 0.045f, center.z - 0.01f), new Vector3(w * 1.12f, 0.05f, d * 1.05f), look.beanie);
            disc.localRotation = Quaternion.Euler(0f, 0f, 9f);
            P(h, new Vector3(0f, top + 0.004f, center.z - 0.005f), new Vector3(w + 0.02f, 0.025f, d + 0.02f), new Color(0.12f, 0.08f, 0.06f));
            P(h, new Vector3(size.x * 0.22f, top + 0.03f, front + 0.008f), new Vector3(0.035f, 0.04f, 0.006f), new Color(0.85f, 0.72f, 0.3f));   // insignia
        }

        if (look.hasShades)   // GAFAS DE SOL: cristales rellenos (envolventes: una banda que rodea las sienes; de aviador: gota con montura fina)
        {
            float ey = center.y + size.y * 0.12f, ex = size.x * 0.24f, r = Mathf.Min(size.x, size.y) * 0.16f, z = front + 0.02f;
            if (look.aviator)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    P(h, new Vector3(s * ex, ey - r * 0.15f, z), new Vector3(r * 2.3f, r * 1.8f, 0.01f), look.shadesLens);
                    P(h, new Vector3(s * (ex - r * 0.15f), ey - r * 1.15f, z), new Vector3(r * 1.7f, r * 0.5f, 0.01f), Color.Lerp(look.shadesLens, Color.white, 0.12f));   // degradado: más claro abajo
                    P(h, new Vector3(s * ex, ey + r * 0.8f, z + 0.004f), new Vector3(r * 2.4f, 0.01f, 0.01f), look.shadesFrame);
                    P(h, new Vector3(s * (size.x * 0.5f + 0.005f), ey + r * 0.6f, center.z + size.z * 0.15f), new Vector3(0.006f, 0.008f, size.z * 0.7f), look.shadesFrame);
                }
                P(h, new Vector3(0f, ey + r * 0.8f, z + 0.004f), new Vector3(ex * 2f - r * 2.2f, 0.008f, 0.008f), look.shadesFrame);   // doble puente
                P(h, new Vector3(0f, ey + r * 0.35f, z + 0.004f), new Vector3(ex * 2f - r * 2.2f, 0.006f, 0.006f), look.shadesFrame);
            }
            else
            {
                P(h, new Vector3(0f, ey, z), new Vector3(size.x * 0.92f, r * 1.7f, 0.014f), look.shadesLens);
                P(h, new Vector3(0f, ey + r * 0.9f, z + 0.002f), new Vector3(size.x * 0.94f, 0.014f, 0.016f), look.shadesFrame);
                for (int s = -1; s <= 1; s += 2)
                {
                    P(h, new Vector3(s * (size.x * 0.5f + 0.006f), ey, front - 0.02f), new Vector3(0.012f, r * 1.4f, 0.07f), look.shadesLens);   // envuelve la sien
                    P(h, new Vector3(s * (size.x * 0.5f + 0.006f), ey + r * 0.5f, center.z + size.z * 0.1f), new Vector3(0.008f, 0.012f, size.z * 0.6f), look.shadesFrame);
                }
            }
        }

        if (look.hasClip)   // PINZA DE FLOR sobre la oreja izquierda: cinco pétalos alrededor de un botón
        {
            Vector3 fc = new Vector3(size.x * 0.5f + 0.035f, top - 0.06f, center.z + size.z * 0.05f);
            for (int i = 0; i < 5; i++)
            {
                float a = i / 5f * Mathf.PI * 2f;
                P(h, fc + new Vector3(0f, Mathf.Cos(a) * 0.022f, Mathf.Sin(a) * 0.022f), new Vector3(0.012f, 0.022f, 0.022f), i % 2 == 0 ? look.clipA : Color.Lerp(look.clipA, Color.white, 0.3f));
            }
            P(h, fc + new Vector3(0.004f, 0f, 0f), new Vector3(0.014f, 0.016f, 0.016f), look.clipB);
        }

        if (look.earring)   // PENDIENTE (uno, o en las dos orejas; pequeño o de aro)
            for (int s = -1; s <= (look.bothEars ? 1 : -1); s += 2)
            {
                float ex = s * (size.x * 0.5f + 0.012f), ey = center.y - size.y * 0.05f;
                Color metal = new Color(0.85f, 0.85f, 0.88f);
                if (!look.hoops) { P(h, new Vector3(ex, ey, center.z), new Vector3(0.018f, 0.018f, 0.018f), metal); continue; }
                P(h, new Vector3(ex, ey - 0.012f, center.z - 0.016f), new Vector3(0.008f, 0.03f, 0.008f), metal);   // aro: cuatro lados
                P(h, new Vector3(ex, ey - 0.012f, center.z + 0.016f), new Vector3(0.008f, 0.03f, 0.008f), metal);
                P(h, new Vector3(ex, ey - 0.03f, center.z), new Vector3(0.008f, 0.008f, 0.04f), metal);
                P(h, new Vector3(ex, ey + 0.004f, center.z), new Vector3(0.008f, 0.008f, 0.02f), metal);
            }
    }


    // MECHONES: sobre cada masa de pelo, tiras finas de tonos algo más claros y más oscuros que siguen su caída (vertical
    // en melenas, de delante atrás arriba). Rompen el bloque liso y dan textura de cabello; las puntas quedan desiguales.
    static void Strands(Transform h, Vector3 center, Vector3 size, Color c, Style style)
    {
        var pieces = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in h) if (t.name == "Pelo") pieces.Add(t);
        var rnd = new System.Random(pieces.Count * 31 + (int)(c.r * 100) + (int)style * 7);
        Color light = Color.Lerp(c, Color.white, 0.18f), dark = Color.Lerp(c, Color.black, 0.25f);
        foreach (Transform p in pieces)
        {
            Vector3 s = p.localScale, pos = p.localPosition;
            Vector3 outward = pos - center; outward.y = Mathf.Max(0f, outward.y);
            bool top = s.y < 0.07f && s.x > 0.1f;   // casquete / copa: mechones de delante hacia atrás
            int n = Mathf.Clamp(Mathf.RoundToInt((top ? s.x : Mathf.Max(s.x, s.z)) / 0.022f), 3, 12);   // mechones finos
            for (int i = 0; i < n; i++)
            {
                float u = (i + 0.5f) / n - 0.5f;
                Color col = rnd.NextDouble() < 0.5 ? light : dark;
                float jitter = (float)rnd.NextDouble() * 0.02f;
                if (top)
                    P(h, pos + new Vector3(u * s.x, s.y * 0.5f + 0.004f, (float)rnd.NextDouble() * 0.02f - 0.01f), new Vector3(0.012f, 0.008f, s.z * (0.7f + jitter * 10f)), col);
                else
                {
                    // Cara exterior de la masa (la que mira hacia fuera de la cabeza).
                    bool alongX = s.x >= s.z;
                    Vector3 face = alongX ? new Vector3(0f, 0f, Mathf.Sign(outward.z == 0f ? -1f : outward.z) * (s.z * 0.5f + 0.003f))
                                          : new Vector3(Mathf.Sign(outward.x == 0f ? 1f : outward.x) * (s.x * 0.5f + 0.003f), 0f, 0f);
                    Vector3 along = alongX ? new Vector3(u * s.x, 0f, 0f) : new Vector3(0f, 0f, u * s.z);
                    float len = s.y * (0.75f + (float)rnd.NextDouble() * 0.35f);   // puntas desiguales
                    P(h, pos + face + along + new Vector3(0f, (s.y - len) * -0.5f, 0f),
                      alongX ? new Vector3(0.012f, len, 0.006f) : new Vector3(0.006f, len, 0.012f), col);
                }
            }
        }
    }
    static Transform P(Transform parent, Vector3 pos, Vector3 size, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "Pelo";
        Collider col = g.GetComponent<Collider>();
        if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = CubeCreature.Mat(c);
        return g.transform;
    }
}
