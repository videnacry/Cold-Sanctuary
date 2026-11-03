using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BANCAL DE TIERRA DE UN JARDÍN FRUTAL (Berón 2026-11-03, docs/jardines-frutales.md §Jardines en la cima): en el techo de
/// alfombras de cada edificio-árbol, los frutales van en BANCALES DE TIERRA (cubierta ajardinada intensiva: lámina antirraíces,
/// capa de drenaje, filtro y 0,8 m de sustrato ligero) y no en macetas sueltas. Cada bancal es un GREMIO: frutales que
/// comparten pH y riego, con su cortejo de plantas companeras (flores de néctar para los insectos útiles, aromáticas, una
/// leguminosa que fija nitrógeno, tagetes contra los nematodos), sus INSECTOS ÚTILES (abejorros y abejas albañiles que
/// polinizan; sírfidos, crisopas, mariquitas y ácaros depredadores contra pulgón, cochinilla, trips y araña roja) y sus
/// ANIMALES (lombrices del compost, una caja nido de carbonero que come orugas).
///
/// Lo que el bancal hace en la simulación (<see cref="FruitPlant.Tick"/>; [A] aproximación de juego, el sentido es el de las
/// fuentes del doc): la tierra guarda más agua que una maceta (<see cref="waterFactor"/>), el compost, el acolchado y la
/// leguminosa sueltan nutrientes (<see cref="nutrientFactor"/>), los enemigos naturales frenan la plaga (<see cref="pestFactor"/>)
/// y los polinizadores que viven allí sostienen el cuajado (<see cref="pollinationFactor"/>).
/// El bancal (raíz con BoxCollider, sin renderer) es UN MUEBLE para <see cref="FurnitureSpacing"/>: guarda sus 2 m con todo.
/// </summary>
public class GardenBed : MonoBehaviour
{
    public string building;
    public string guild;
    [Tooltip("Largo × ancho del bancal (m) y alto de su sustrato.")]
    public Vector2 size = new Vector2(4f, 1.4f);
    public float depth = 0.8f;
    [Tooltip("Cuánto gasta el agua una planta en tierra frente a una maceta (1 = igual).")]
    public float waterFactor = 0.6f;
    [Tooltip("Cuánto sube la plaga con los insectos útiles del bancal (1 = como en maceta).")]
    public float pestFactor = 0.5f;
    [Tooltip("Cuánto se gasta el abono con compost, acolchado y leguminosa (1 = como en maceta).")]
    public float nutrientFactor = 0.7f;
    [Tooltip("Cuánto baja el cuajado sin pincel con abejorros y flores (1 = como en maceta).")]
    public float pollinationFactor = 0.5f;
    public string[] companions = new string[0], beneficials = new string[0], animals = new string[0];

    public static readonly List<GardenBed> All = new List<GardenBed>();
    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public List<FruitPlant> Plants => new List<FruitPlant>(GetComponentsInChildren<FruitPlant>());
    public Guild Kind => Find(guild);

    /// <summary>Volumen de sustrato (m³).</summary>
    public float SoilVolume => size.x * size.y * depth;
    /// <summary>Peso con el sustrato saturado (t): sustrato de cubierta intensiva ~1,2 t/m³ saturado (FLL 2018: 1,0–1,4).</summary>
    public float SaturatedTonnes => SoilVolume * 1.2f + size.x * size.y * 0.1f;   // + drenaje, filtro y bordes (~0,1 t/m²)

    // ── LOS GREMIOS (docs/jardines-frutales.md §Jardines en la cima, con sus fuentes) ──────────────────────────────

    public class Guild
    {
        public string id, nombre, ph, acolchado;
        public GardenCrop.Kind[] frutales;
        /// <summary>Compañeras que ya existen en el juego (GardenCrop): se plantan con su forma.</summary>
        public GardenCrop.Kind[] companeras;
        /// <summary>Flores de néctar (alyssum, borraja, caléndula, facelia…): franjas de flor en los bordes del bancal.</summary>
        public string[] flores;
        public string[] insectos, animales;
        public float phMin, phMax;
    }

    public static readonly Guild[] Guilds =
    {
        new Guild
        {
            id = "citricos", nombre = "Cítricos con su cortejo", ph = "pH 6–7", phMin = 6f, phMax = 7f, acolchado = "compost y hoja de consuelda",
            frutales = new[] { GardenCrop.Kind.Limon, GardenCrop.Kind.Lima, GardenCrop.Kind.Naranja },
            companeras = new[] { GardenCrop.Kind.Romero, GardenCrop.Kind.Cilantro, GardenCrop.Kind.Haba },
            flores = new[] { "alyssum (Lobularia maritima)", "caléndula", "consuelda" },
            insectos = new[] { "Cryptolaemus montrouzieri (cochinilla algodonosa)", "Aphytis melinus (piojo rojo)", "sírfidos y crisopas (pulgón)", "abejorros" },
            animales = new[] { "lombrices del compost", "caja nido de carbonero" },
        },
        new Guild
        {
            id = "andino", nombre = "Andino de semisombra (la banana da la sombra)", ph = "pH 5,5–6,5", phMin = 5.5f, phMax = 6.5f, acolchado = "hoja de banana picada",
            frutales = new[] { GardenCrop.Kind.Banana, GardenCrop.Kind.Cafe, GardenCrop.Kind.Naranjilla },
            companeras = new[] { GardenCrop.Kind.Huacatay, GardenCrop.Kind.Haba, GardenCrop.Kind.Cedron },
            flores = new[] { "tagetes (contra nematodos)", "capuchina (planta trampa del pulgón)" },
            insectos = new[] { "Aphidius colemani (pulgón)", "Orius (trips)", "mariquitas", "abejorros" },
            animales = new[] { "lombrices del compost", "hongos micorrícicos (inóculo)" },
        },
        new Guild
        {
            id = "acido", nombre = "Bayas de suelo ácido", ph = "pH 4,5–5,5", phMin = 4.5f, phMax = 5.5f, acolchado = "corteza de pino y acícula",
            frutales = new[] { GardenCrop.Kind.Mortino, GardenCrop.Kind.Frutilla, GardenCrop.Kind.Pina },
            companeras = new[] { GardenCrop.Kind.Cebollin, GardenCrop.Kind.Albahaca },
            flores = new[] { "borraja (abejas para la frutilla)", "alyssum" },
            insectos = new[] { "Phytoseiulus persimilis (araña roja)", "Amblyseius swirskii (trips y mosca blanca)", "abejorros (polinización por vibración)" },
            animales = new[] { "lombrices del compost" },
        },
        new Guild
        {
            id = "mediterraneo", nombre = "Mediterráneo seco", ph = "pH 6,5–8", phMin = 6.5f, phMax = 8f, acolchado = "grava y paja",
            frutales = new[] { GardenCrop.Kind.Olivo, GardenCrop.Kind.Almendro },
            companeras = new[] { GardenCrop.Kind.Romero, GardenCrop.Kind.Arveja, GardenCrop.Kind.Perejil },
            flores = new[] { "facelia", "milenrama", "tomillo" },
            insectos = new[] { "abeja albañil Osmia (almendro)", "abejorros", "crisopas", "mariquitas" },
            animales = new[] { "bloque nido de Osmia", "caja nido de carbonero" },
        },
        new Guild
        {
            id = "tropico", nombre = "Trópico húmedo", ph = "pH 6–7", phMin = 6f, phMax = 7f, acolchado = "hojarasca y cáscara de cacao",
            frutales = new[] { GardenCrop.Kind.Cacao, GardenCrop.Kind.Babaco, GardenCrop.Kind.Guanabana },
            companeras = new[] { GardenCrop.Kind.Jengibre, GardenCrop.Kind.Curcuma, GardenCrop.Kind.Frijol },
            flores = new[] { "montón de pseudotallo de banana (cría de mosquitas Forcipomyia)" },
            insectos = new[] { "mosquitas Forcipomyia (polinizan el cacao)", "Cryptolaemus (cochinilla)", "abejorros" },
            animales = new[] { "lombrices del compost", "hongos micorrícicos" },
        },
    };

    public static Guild Find(string id) { foreach (Guild g in Guilds) if (g.id == id) return g; return null; }

    /// <summary>pH de cultivo de cada frutal (de su ficha, FruitCare): el mínimo y el máximo.</summary>
    public static (float lo, float hi) PhOf(GardenCrop.Kind k)
    {
        switch (k)
        {
            case GardenCrop.Kind.Mortino: case GardenCrop.Kind.Pina: return (4.5f, 5.5f);
            case GardenCrop.Kind.Frutilla: return (5.5f, 6.5f);
            case GardenCrop.Kind.Cafe: case GardenCrop.Kind.Naranjilla: return (5f, 6.5f);
            case GardenCrop.Kind.Banana: return (5.5f, 7f);
            case GardenCrop.Kind.Olivo: return (6.5f, 8.5f);
            case GardenCrop.Kind.Almendro: return (6f, 8f);
            case GardenCrop.Kind.Datil: return (7f, 8.5f);
            case GardenCrop.Kind.Cacao: case GardenCrop.Kind.Naranja: case GardenCrop.Kind.Limon: case GardenCrop.Kind.Lima: return (6f, 7f);
            case GardenCrop.Kind.Babaco: return (6f, 6.5f);
            case GardenCrop.Kind.Guanabana: return (5.5f, 6.5f);
            default: return (5.5f, 7f);
        }
    }

    /// <summary>¿Comparten pH todas las frutales del bancal? (la razón de agruparlas en gremios: la tierra del bancal tiene UN pH;
    /// la frutilla, algo menos ácida que el mortiño, se tolera a pH 5,5 con acolchado ácido).</summary>
    public bool PhCompatible(out string why)
    {
        float lo = 0f, hi = 14f;
        foreach (FruitPlant p in Plants) { var (a, b) = PhOf(p.kind); lo = Mathf.Max(lo, a); hi = Mathf.Min(hi, b); }
        why = $"pH común {lo:0.0}–{hi:0.0}";
        return hi >= lo - 0.01f;
    }
}
