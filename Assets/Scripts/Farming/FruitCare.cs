using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FICHAS DE CUIDADO DE LOS FRUTALES (docs/jardines-frutales.md, Berón 2026-10-30): de cada planta frutal que ya existe
/// en el juego (<see cref="GardenCrop.Kind"/>), lo que necesita para estar bien en una MACETA GRANDE del techo de una
/// torre, dentro del clima controlado del santuario (latitud −77,85°: fuera, −18 °C de media y 4 meses de noche polar):
/// agua, luz, temperatura, sustrato, nutrientes, poda, plagas, cosecha y polinización, con su fuente. Es el saber de la
/// experta en frutales (Irosene) y lo que mira el jardinero en su ronda diaria (<see cref="FruitGardener"/>).
/// Los números de la SIMULACIÓN (cuánta agua y abono gasta al día, cuánto tarda la revisión) salen de esas fichas:
/// - <see cref="Ficha.aguaDia"/>: fracción del agua de la maceta que gasta en un día de juego (plátano y coco, mucha;
///   olivo, dátil y piña, poca);
/// - <see cref="Ficha.abonoDia"/>: fracción del abono que consume al día (los muy exigentes en potasio o nitrógeno, más);
/// - <see cref="Ficha.minutos"/>: minutos REALES de la revisión diaria CON RIEGO POR GOTEO (Berón 2026-10-31: el agua llega
///   sola por el gotero; la persona mira hojas y envés, comprueba el gotero, abona y trata la plaga): 1 min un árbol o una
///   palmera, 0,75 un arbusto, 0,5 una herbácea. En el juego son minutos de juego.
/// - Las TAREAS DE LA SEMANA, con el tiempo que libera el goteo (<see cref="Polinizar"/>, <see cref="PodaDias"/>): polinizar a
///   pincel las que lo necesitan (cada 2 días, mientras florecen) y podar y anotar el crecimiento (cada 14 días; la frutilla,
///   quitar estolones cada 7; la piña no se poda).
/// Una tabla, una entrada por especie: nada de duplicar datos en otros sitios (el nombre visible sale de
/// <see cref="GardenCrop"/>; la receta, de Gastronomy).
/// </summary>
public static class FruitCare
{
    public enum Forma { Arbol, Palmera, Arbusto, Herbacea, Rastrera }

    public class Ficha
    {
        public GardenCrop.Kind kind;
        public string nombre, cientifico;
        public Forma forma;
        [Tooltip("Escala del modelo de GardenCrop: variedad enana o patrón enanizante en maceta.")]
        public float escala;
        [Tooltip("Maceta (ancho × fondo × alto, m): 1 m³ para un árbol enano.")]
        public Vector3 maceta;
        public float aguaDia, abonoDia, minutos;
        public string agua, luz, temperatura, sustrato, nutrientes, poda, plagas, cosecha, polinizacion, antartida, enana, fuente;
    }

    static Ficha F(GardenCrop.Kind k, string nombre, string cient, Forma forma, float escala, Vector3 maceta, float aguaDia, float abonoDia, float minutos,
                   string agua, string luz, string temp, string sustrato, string nutr, string poda, string plagas, string cosecha, string poli, string antartida, string enana, string fuente)
        => new Ficha { kind = k, nombre = nombre, cientifico = cient, forma = forma, escala = escala, maceta = maceta, aguaDia = aguaDia, abonoDia = abonoDia, minutos = minutos,
                       agua = agua, luz = luz, temperatura = temp, sustrato = sustrato, nutrientes = nutr, poda = poda, plagas = plagas, cosecha = cosecha,
                       polinizacion = poli, antartida = antartida, enana = enana, fuente = fuente };

    static readonly Vector3 PotTree = new Vector3(1.2f, 0.7f, 1.2f), PotPalm = new Vector3(1.3f, 0.8f, 1.3f), PotShrub = new Vector3(0.9f, 0.6f, 0.9f),
                            PotHerb = new Vector3(0.8f, 0.5f, 0.8f), PotVine = new Vector3(1.4f, 0.5f, 1.4f);

    /// <summary>Las frutales del juego, de las más grandes a las más pequeñas (así se plantan: primero lo que más sitio
    /// pide). No entran las de especia (canela, laurel, clavo, pimienta) ni las hortalizas de fruto (tomate, pimiento…),
    /// que son del Huerto.</summary>
    public static readonly Ficha[] All =
    {
        F(GardenCrop.Kind.Mango, "mango", "Mangifera indica", Forma.Arbol, 0.45f, PotTree, 0.45f, 0.12f, 1f,
          "húmedo en crecimiento, más seco antes de florecer; 2–4 L/día en maceta", "pleno sol, mucha luz (DLI alto); no es de fotoperiodo",
          "24–30 °C; daño por debajo de ~1 °C; las noches frescas (< 15 °C) o la sequía inducen la floración", "bien drenado, pH 5,5–7",
          "N moderado (el exceso impide florecer), K, B y Zn", "despunte justo después de cosechar para mantenerlo bajo",
          "antracnosis, oídio, cochinilla, trips", "injertado: 3–4 años; 3–5 meses de flor a fruto", "casi autofértil; moscas y abejas ayudan: pincel en invernadero",
          "posible con esfuerzo: luz LED intensa en la noche polar y polinizar a mano", "cultivares compactos 'Pickering', 'Cogshall', 'Nam Doc Mai': 2–2,5 m en maceta",
          "UF/IFAS EDIS MG216 (journals.flvc.org/edis/article/view/116063)"),
        F(GardenCrop.Kind.Aguacate, "aguacate", "Persea americana", Forma.Arbol, 0.45f, PotTree, 0.4f, 0.12f, 1f,
          "húmedo pero con drenaje perfecto (muy sensible al encharcamiento); 2–4 L/día", "pleno sol",
          "20–28 °C; raza mexicana aguanta −4 °C, antillana ~0 °C; noches de ~15 °C ayudan a florecer", "grueso y aireado, pH 5,5–6,5",
          "N, Zn y Fe; sensible a la sal y al boro", "mantenerlo bajo; pinzar las puntas", "Phytophthora cinnamomi (podredumbre de raíz), trips, ácaros",
          "injertado: 3–4 años; 6–12 meses de flor a fruto", "flores tipo A/B: un cultivar A y uno B, o pincel", "posible con esfuerzo (polinización y drenaje)",
          "'Wurtz' (Little Cado), tipo A, ~3–4 m", "conocimiento hortícola general (UC ANR, fichas del aguacate)"),
        F(GardenCrop.Kind.Guanabana, "guanábana", "Annona muricata", Forma.Arbol, 0.5f, PotTree, 0.4f, 0.1f, 1f,
          "humedad pareja, sin encharcar", "pleno sol a sombra ligera", "25–30 °C; daño por debajo de ~5 °C, muere por debajo de ~3 °C",
          "bien drenado, pH 5,5–6,5", "NPK equilibrado", "formar bajo", "cochinilla algodonosa, cochinilla; el barrenador de la semilla es plaga de campo",
          "de semilla: 3–5 años; 4–6 meses de flor a fruto", "flores protóginas: en invernadero cuajan poco y deformes: polinizar a mano",
          "posible con esfuerzo (polinización a mano y poda fuerte)", "en el campo 7,5–9 m; en maceta ~2–3 m con poda (estimado)", "Morton 1987, Fruits of Warm Climates (growables.org)"),
        F(GardenCrop.Kind.Cacao, "cacao", "Theobroma cacao", Forma.Arbol, 0.5f, PotTree, 0.45f, 0.12f, 1f,
          "siempre húmedo; humedad del aire 70–80 %", "SEMISOMBRA (~50 %)", "18–32 °C; daño por debajo de ~10 °C", "rico en materia orgánica, pH 6–7",
          "mucho N y K; Zn", "poda de formación hasta la horqueta (jorquette)", "cochinilla, trips, mazorca negra (Phytophthora), míridos",
          "de semilla: 3–5 años; 5–6 meses de polinización a mazorca madura", "cauliflora; en el campo la polinizan mosquitos Forcipomyia que no hay en el invernadero: a mano (cuaja solo un 0,5–5 %)",
          "posible con esfuerzo (humedad alta, polinización a mano)", "~2–3 m en maceta (estimado)", "IntechOpen cap. 66049; UC ANR"),
        F(GardenCrop.Kind.Almendro, "almendro", "Prunus dulcis", Forma.Arbol, 0.45f, PotTree, 0.25f, 0.08f, 1f,
          "dejar secar entre riegos; aguanta la sequía", "pleno sol", "15–30 °C creciendo; necesita FRÍO invernal: ~350–500 h (Nonpareil ~22–23 porciones de frío)",
          "profundo y drenado, pH 6–8", "N, Zn, B", "vaso abierto; poda fuerte en invierno", "araña roja, anarsia, monilia, cribado",
          "3–4 años; ~7 meses de flor a cosecha", "casi todos autoincompatibles; 'Independence' es autofértil pero cuaja un 63 % menos sin abejas: abejas o pincel",
          "difícil: pide un invierno frío (zona de frío estacional del invernadero) y polinizadores", "enano genético 'Garden Prince', 2–3 m",
          "UC Davis (entnem.ucdavis.edu, almendro autofértil); UC ANR porciones de frío"),
        F(GardenCrop.Kind.Olivo, "olivo", "Olea europaea", Forma.Arbol, 0.5f, PotTree, 0.2f, 0.06f, 1f,
          "dejar secar entre riegos", "pleno sol", "15–30 °C; aguanta ~−7 °C; para florecer necesita un periodo fresco que oscile (~2 meses a 2–15 °C; a 7 °C constantes no florece)",
          "muy drenado, pH 6,5–8,5", "poco: N y B", "aclarar la copa", "cochinilla de la tizne, repilo, verticilosis",
          "3–4 años; ~5–6 meses de flor a fruto", "por viento; 'Arbequina' es autofértil: sacudir las flores o un ventilador",
          "difícil: pide frío estacional (Arbequina es la menos exigente)", "'Arbequina', 1,5–2,5 m", "ASHS 2015 (olivos); Frontiers in Plant Science 2017, 10.3389/fpls.2017.01830"),
        F(GardenCrop.Kind.Naranja, "naranja", "Citrus sinensis", Forma.Arbol, 0.5f, PotTree, 0.35f, 0.1f, 1f,
          "dejar secar los primeros cm y regar a fondo; nunca encharcado; 1–3 L/día", "pleno sol (DLI ~20–30)",
          "22–30 °C; daño a −2/−3 °C; sin frío obligatorio, pero un fresco (< 20 °C) mejora la floración y el color (con calor constante la piel queda verde)",
          "drenado, pH 6–7", "mucho N y quelatos de Fe, Zn y Mn", "ligera; quitar chupones", "cochinilla, cochinilla algodonosa, araña roja, mosca blanca, Phytophthora",
          "injertado: ~3 años; 7–12 meses de flor a fruto", "autofértil", "posible (pide un fresco para colorear)", "patrón Flying Dragon (la mitad de tamaño); 'Washington Navel'",
          "garden.org; UF/IFAS EDIS (patrón trifoliado)"),
        F(GardenCrop.Kind.Limon, "limón", "Citrus limon", Forma.Arbol, 0.55f, PotTree, 0.35f, 0.1f, 1f,
          "dejar secar los primeros cm y regar a fondo; nunca encharcado; 1–3 L/día", "pleno sol (DLI ~20–30); no es de fotoperiodo",
          "20–32 °C; hojas dañadas a ~−2 °C, fruto a −1 °C; sin frío: florece casi todo el año", "drenado, pH 6–7",
          "mucho N y quelatos de Fe, Zn y Mn (en maceta, siempre quelatados)", "ligera; quitar chupones", "cochinilla, cochinilla algodonosa, araña roja, mosca blanca, Phytophthora",
          "injertado: 2–3 años; 6–9 meses de flor a fruto", "autofértil", "realista", "'Improved Meyer' sobre Flying Dragon: 1,5–2,5 m en maceta",
          "garden.org/learn/articles/view/1294; UF/IFAS EDIS (Flying Dragon)"),
        F(GardenCrop.Kind.Lima, "lima", "Citrus × latifolia", Forma.Arbol, 0.55f, PotTree, 0.35f, 0.1f, 1f,
          "como el limón", "pleno sol", "22–32 °C; la más friolera de los cítricos (daño ~0 °C)", "drenado, pH 6–7",
          "como el limón; la clorosis férrica es frecuente", "ligera", "como el limón", "2–3 años; 3–5 meses de flor a fruto (el cítrico más rápido)",
          "partenocárpica y sin semillas: no necesita polinizar", "realista", "'Bearss' sobre Flying Dragon", "garden.org; UF/IFAS EDIS"),
        F(GardenCrop.Kind.Coco, "coco (enano)", "Cocos nucifera", Forma.Palmera, 0.45f, PotPalm, 0.55f, 0.12f, 1f,
          "húmedo; mucha agua (3–5 L/día)", "pleno sol, luz muy alta", "27–32 °C; sufre por debajo de 10–13 °C", "arenoso y drenado, pH 5,5–8",
          "mucho K y Cl; Mg, B", "solo quitar las hojas secas", "araña roja, cochinilla, cochinilla algodonosa",
          "los enanos dan a los 3–4 años (Malayo enano ~36 meses); ~12 meses de polinización a coco maduro", "los enanos se autopolinizan casi siempre",
          "DIFÍCIL: aun enana llega a 5–10 m con hojas de 3–5 m; aquí, joven, y se renueva", "Malayo enano; no hay patrón enanizante", "Wikipedia: Dwarf coconut"),
        F(GardenCrop.Kind.Datil, "dátil", "Phoenix dactylifera", Forma.Palmera, 0.45f, PotPalm, 0.2f, 0.08f, 1f,
          "superficie seca y riego profundo; tolera la sal", "pleno sol", "necesita MUCHO calor para madurar (> ~3000 grados-día sobre 18 °C, sin verificar) y aire seco al madurar; aguanta −5 °C",
          "arenoso, pH 7–8,5", "K y N", "quitar las hojas viejas y aclarar los racimos", "cochinilla, ácaros, Fusarium, pudrición del fruto con humedad",
          "de hijuelo: 4–8 años; 6–7 meses de polinización a fruto maduro", "DIOICA: polinización a mano con polen de un macho (o comprado)",
          "DIFÍCIL: árbol de 15–25 m; en maceta no fructifica a un tamaño sensato: aquí es joven y decorativa", "—", "Wikipedia: Date palm; Bee Culture, manual de polinización"),
        F(GardenCrop.Kind.Cafe, "café", "Coffea arabica", Forma.Arbusto, 0.7f, PotShrub, 0.35f, 0.1f, 0.75f,
          "humedad pareja; un estrés hídrico y luego riego hace florecer", "SEMISOMBRA o sol filtrado", "óptimo ~21 °C; se para por debajo de 10 °C y por encima de 32 °C",
          "orgánico, pH 5–6,5", "N, K, Zn, B", "despuntar a 1,5–2 m", "cochinilla, cochinilla algodonosa, ácaros, roya",
          "3–4 años de semilla; ~2500 grados-día (base 10 °C) de flor a cosecha: ~4 meses a 25/20 °C", "autofértil (arábica es autógama)",
          "realista", "'Caturra', 'Catuaí': 1,5–2 m", "Cenicafé (biblioteca.cenicafe.org); MDPI Plants 2025"),
        F(GardenCrop.Kind.Babaco, "babaco", "Vasconcellea × heilbornii", Forma.Arbusto, 1f, PotShrub, 0.4f, 0.12f, 0.75f,
          "húmedo, pero muy propenso a la pudrición de raíz", "luz viva; tolera semisombra", "subtropical fresco: mínima 10 °C de noche y 12 °C de día; a 18 °C madura parejo; el frío marca la piel",
          "ligero y drenado, pH 6–6,5", "mucho K y Ca", "un solo tallo; cortarlo tras la cosecha", "Fusarium, Phytophthora, ácaros",
          "da fruto ~10 meses tras plantar y produce 6 meses (~40 frutos de 1–1,5 kg)", "partenocárpico: no necesita polinizar",
          "realista: planta ideal de maceta", "tallo único de 1,5–2,5 m", "Kempler y Kabaluk 1996, HortScience 31:785"),
        F(GardenCrop.Kind.Banana, "banana (enana)", "Musa AAA 'Dwarf Cavendish'", Forma.Arbusto, 0.7f, new Vector3(1.0f, 0.7f, 1.0f), 0.7f, 0.18f, 0.75f,
          "siempre húmedo; muy sedienta (5–10 L/día)", "pleno sol, luz alta", "26–30 °C; deja de crecer por debajo de 14 °C, daño por frío < 10–13 °C",
          "rico y drenado, pH 5,5–7", "MUY exigente en K (K:N ≈ 3:1) y Mg", "una madre y un hijo; quitar hojas secas", "araña roja, trips, pulgón (vector del bunchy top), mal de Panamá",
          "9–12 meses de plantar a flor; 3–4 meses de flor a racimo", "partenocárpica: no necesita polinizar", "realista", "'Dwarf Cavendish' 2–3 m; 'Super Dwarf' ~1,2 m",
          "conocimiento hortícola general"),
        F(GardenCrop.Kind.Naranjilla, "naranjilla", "Solanum quitoense", Forma.Arbusto, 0.9f, PotShrub, 0.4f, 0.1f, 0.75f,
          "húmedo; aire húmedo", "SEMISOMBRA (no soporta pleno sol)", "óptimo 17–19 °C; por encima de 29–30 °C sufre; no aguanta helada",
          "orgánico y drenado, pH 5–6,5", "NPK equilibrado", "ligera", "nematodo agallador (Meloidogyne): injertar en S. torvum o sustrato estéril",
          "~1 año de semilla a fruto; 4–5 meses de flor a fruto", "casi autofértil; las abejas ayudan", "realista (fresca y a semisombra)",
          "arbusto de 1,5–2 m (hay formas sin espinas)", "Morton 1987 (growables.com); PROSEA"),
        F(GardenCrop.Kind.Mortino, "mortiño", "Vaccinium floribundum", Forma.Arbusto, 1f, PotShrub, 0.3f, 0.06f, 0.75f,
          "húmedo, nunca encharcado", "pleno sol a sombra ligera", "planta de páramo (2800–4000 m): fresco, 8–16 °C; aguanta heladas leves; sin dato de horas de frío",
          "ÁCIDO, pH 4,5–5,5 (sustrato de ericáceas)", "poco: N amoniacal y Fe", "mínima", "poco estudiado en invernadero: botritis y pulgón como el arándano",
          "lento: 2–3 años de esqueje o in vitro", "polinización por vibración como el arándano: pincel o vibrar las flores (mejor 3 clones)",
          "realista", "arbusto de 0,5–1,5 m, bueno en maceta", "RHS (rhs.org.uk/plants/18683); tesis ESPOL"),
        F(GardenCrop.Kind.Pina, "piña", "Ananas comosus", Forma.Herbacea, 1.3f, PotHerb, 0.2f, 0.08f, 0.5f,
          "dejar secar entre riegos (planta CAM)", "pleno sol", "20–30 °C; daño por debajo de ~10 °C; las noches frescas inducen la floración",
          "arenoso y drenado, pH 4,5–5,5", "N y K; Fe", "ninguna; quitar hijuelos para multiplicar", "cochinilla (virus del marchitamiento), pudrición del cogollo (Phytophthora)",
          "de corona a fruto ~18–24 meses; inducida con etileno a los 9–12 meses, 5–7 meses más", "autoincompatible: el fruto se forma sin polinizar",
          "realista", "~1 m en maceta de 20–30 L", "growables.org (inducción floral de la piña); Embrapa"),
        F(GardenCrop.Kind.Frutilla, "frutilla", "Fragaria × ananassa", Forma.Herbacea, 1.6f, PotHerb, 0.35f, 0.1f, 0.5f,
          "húmedo y parejo; goteo", "pleno sol; las de día neutro ('Albion', 'Seascape') florecen siempre",
          "15–25 °C; las de día corto piden 200–400 h de frío; las de día neutro, poco", "turba, pH 5,5–6,5", "K, Ca, B", "quitar estolones y hojas viejas",
          "botritis, oídio, araña roja, trips", "~3 meses de plantar a fruto; ~30 días de flor a fruto", "autofértil, pero sin abejas o pincel la fresa sale deforme (EDEN ISS fracasó por eso)",
          "realista (la Food Growth Chamber del Polo Sur las cosecha)", "día neutro, en torres o canalones", "space.com (EDEN ISS); Univ. de Arizona (South Pole Food Growth Chamber)"),
        F(GardenCrop.Kind.Sandia, "sandía", "Citrullus lanatus", Forma.Rastrera, 0.6f, PotVine, 0.45f, 0.12f, 0.5f,
          "húmedo hasta que cuaja, luego menos", "pleno sol (DLI > 20)", "21–32 °C; daño por frío < 10 °C", "franco-arenoso, pH 6–6,8",
          "K y Ca al fructificar", "guiar en vertical; 1–2 frutos por planta", "oídio, araña roja, pulgón, mosca blanca",
          "anual: 80–100 días de semilla a fruto; 30–40 de flor a madura", "monoica: abejas o polinización a mano",
          "realista en espaldera (anual: se resiembra)", "tipo 'Sugar Baby' en bolsa de ≥ 40 L", "conocimiento hortícola general"),
    };

    static Dictionary<GardenCrop.Kind, Ficha> _by;
    /// <summary>La ficha de una frutal, o null si <paramref name="k"/> no es frutal.</summary>
    public static Ficha For(GardenCrop.Kind k)
    {
        if (_by == null) { _by = new Dictionary<GardenCrop.Kind, Ficha>(); foreach (Ficha f in All) _by[f.kind] = f; }
        return _by.TryGetValue(k, out Ficha x) ? x : null;
    }
    public static bool IsFruit(GardenCrop.Kind k) => For(k) != null;

    /// <summary>¿Necesita polinización a mano (pincel, sacudir, vibrar)? Sin abejas en el invernadero no cuaja o cuaja deforme
    /// (EDEN ISS perdió así sus fresas): mango, aguacate (flores A/B), guanábana (protógina), cacao (cuaja 0,5–5 %), almendro
    /// (autoincompatible), olivo (sacudir), dátil (dioica), frutilla, sandía (monoica) y mortiño (por vibración).</summary>
    public static bool Polinizar(GardenCrop.Kind k) =>
        k == GardenCrop.Kind.Mango || k == GardenCrop.Kind.Aguacate || k == GardenCrop.Kind.Guanabana || k == GardenCrop.Kind.Cacao || k == GardenCrop.Kind.Almendro
        || k == GardenCrop.Kind.Olivo || k == GardenCrop.Kind.Datil || k == GardenCrop.Kind.Frutilla || k == GardenCrop.Kind.Sandia || k == GardenCrop.Kind.Mortino;

    /// <summary>Cada cuántos días se poda (y se anota su crecimiento en el cuaderno de investigación); 0 = no se poda.</summary>
    public static int PodaDias(GardenCrop.Kind k) => k == GardenCrop.Kind.Pina ? 0 : k == GardenCrop.Kind.Frutilla ? 7 : 14;

    /// <summary>Minutos (de juego) de cada tarea de la semana.</summary>
    public const float MinPolinizar = 1f, MinPodar = 3f;
}
