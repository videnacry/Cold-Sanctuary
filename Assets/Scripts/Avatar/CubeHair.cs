using UnityEngine;

/// <summary>
/// ASPECTO de cada persona (cabello, piel, ojos, barba) para el cuerpo de cubos: el CABELLO se arma con cubos sobre la
/// cabeza (corto, largo, moño, coleta, rizado, rapado) y hay gorro de cocinero para los cocineros. La CARA (ojos,
/// párpados, cejas, nariz, mandíbula) la pone <see cref="CubeFace"/>; aquí se eligen sus colores por personaje.
/// </summary>
public static class CubeHair
{
    public enum Style { Short, Long, Bun, Ponytail, Curly, Shaved, ChefHat, Fade, LongWavy, Medium, Bob }
    public enum Beard { None, Full, Goatee, Stubble }

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
        public Color lips; public bool hasLips;              // labios pintados
        // Capas de ropa del torso: CHALECO (pecho) sobre camiseta (mangas cortas de otro color), CORSÉ en la cintura, BOTAS altas.
        public Color upperSleeves, waist, boots; public bool hasUpperSleeves, hasWaist, hasBoots;
        public bool upperTattoo, wristband, chain, rifle;   // tatuaje en el brazo, muñequera, cadena de cartera, rifle a la espalda
        public bool feminine;                                // cejas finas arqueadas y pestañas/delineado
        public float jaw, cheeks;                            // forma de la cara: ancho de mandíbula (0,68–0,92) y pómulos (0–1)
        public Look(Style s, Color h, Color sk, Color e, bool b = false)
        {
            style = s; hair = h; skin = sk; eyes = e; beard = b; height = 0f; clothes = default; hasClothes = false;
            pants = sleeves = shoes = glasses = cap = capLogo = default; hasPants = hasSleeves = shorts = hasShoes = hasGlasses = hasCap = false;
            beardStyle = b ? Beard.Full : Beard.None; tattoos = earring = false;
            beanie = lips = upperSleeves = waist = boots = default; hasBeanie = hasLips = hasUpperSleeves = hasWaist = hasBoots = false;
            upperTattoo = wristband = chain = rifle = false; feminine = false; jaw = cheeks = 0f;
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
        public Look Fem() { feminine = true; return this; }
        public Look Face(float jawWidth, float cheekbones = 0f) { jaw = jawWidth; cheeks = cheekbones; return this; }
        public Look Lips(Color c) { lips = c; hasLips = true; return this; }
        public Look Stubble() { beard = true; beardStyle = Beard.Stubble; return this; }
        public Look Vest(Color shirt) { upperSleeves = shirt; hasUpperSleeves = true; return this; }   // clothes = chaleco; shirt = camiseta
        public Look Corset(Color c) { waist = c; hasWaist = true; return this; }
        public Look Boots(Color c) { boots = c; hasBoots = true; return this; }
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
                        if (look.style == Style.Bob) P(h, new Vector3(s * (size.x * 0.42f), center.y - size.y * 0.55f, center.z + size.z * 0.35f), new Vector3(0.05f, size.y * 0.75f, 0.05f), c);   // mechones largos delante
                    }
                    if (look.style == Style.Bob) P(h, new Vector3(0f, top - 0.022f, center.z + size.z * 0.5f + 0.012f), new Vector3(w * 0.98f, 0.045f, 0.03f), c);   // flequillo recto (por encima de las cejas)
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
                else if (look.style == Style.Ponytail)
                {
                    P(h, new Vector3(0f, center.y + size.y * 0.3f, back - 0.04f), new Vector3(0.05f, 0.05f, 0.06f), c * 0.8f + new Color(0, 0, 0, 1));
                    P(h, new Vector3(0f, center.y - size.y * 0.15f, back - 0.06f), new Vector3(0.06f, size.y * 0.8f, 0.05f), c);
                }
                else if (look.style == Style.Curly)
                    for (int i = 0; i < 10; i++)
                    {
                        float a = i * 36f * Mathf.Deg2Rad;
                        P(h, new Vector3(Mathf.Cos(a) * size.x * 0.42f, top + 0.03f + (i % 2) * 0.02f, center.z + Mathf.Sin(a) * size.z * 0.42f), Vector3.one * 0.075f, c);
                    }
                break;
        }
        if (look.style != Style.ChefHat && look.style != Style.Shaved) Strands(h, center, size, c, look.style);
        float front = center.z + size.z * 0.5f;
        if (look.beardStyle == Beard.Full)
        {
            P(h, new Vector3(0f, center.y - size.y * 0.42f, center.z + size.z * 0.42f), new Vector3(size.x * 0.75f, size.y * 0.25f, 0.06f), c);
            for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * size.x * 0.42f, center.y - size.y * 0.2f, center.z + size.z * 0.3f), new Vector3(0.03f, size.y * 0.35f, 0.08f), c);
        }
        else if (look.beardStyle == Beard.Stubble)   // BARBA DE UNOS DÍAS: sombra fina en mentón y mejillas
        {
            Color sh = Color.Lerp(look.skin, c, 0.22f);   // sombra suave, no una máscara
            P(h, new Vector3(0f, center.y - size.y * 0.4f, front + 0.003f), new Vector3(size.x * 0.6f, size.y * 0.14f, 0.008f), sh);
            for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * size.x * 0.33f, center.y - size.y * 0.22f, front + 0.002f), new Vector3(size.x * 0.12f, size.y * 0.2f, 0.008f), sh);
        }
        else if (look.beardStyle == Beard.Goatee)   // PERILLA: solo en la barbilla
            P(h, new Vector3(0f, center.y - size.y * 0.44f, front + 0.005f), new Vector3(size.x * 0.32f, size.y * 0.16f, 0.03f), c);

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

        if (look.earring)   // PENDIENTE
            P(h, new Vector3(-(size.x * 0.5f + 0.012f), center.y - size.y * 0.05f, center.z), new Vector3(0.018f, 0.018f, 0.018f), new Color(0.85f, 0.85f, 0.88f));
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
            int n = Mathf.Clamp(Mathf.RoundToInt((top ? s.x : Mathf.Max(s.x, s.z)) / 0.03f), 2, 9);
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
    static void P(Transform parent, Vector3 pos, Vector3 size, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "Pelo";
        Collider col = g.GetComponent<Collider>();
        if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = CubeCreature.Mat(c);
    }
}
