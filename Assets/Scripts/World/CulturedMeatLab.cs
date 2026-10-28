using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LABORATORIO DE CARNE CULTIVADA (área `CulturedMeatLab`, docs/carne-cultivada.md): produce carne para los depredadores
/// del santuario sin matar a ningún animal. Simula el proceso real, por lotes y con el tiempo comprimido:
///  1. BIOPSIA: unos gramos de músculo de un animal vivo (en la enfermería, con anestesia local) → se aíslan las células
///     satélite (madre del músculo) y precursoras de grasa en la campana de flujo laminar.
///  2. BANCO DE CÉLULAS: se congelan en nitrógeno líquido (−196 °C); cada lote empieza descongelando un vial.
///  3. MEDIO DE CULTIVO: aminoácidos, glucosa, vitaminas, sales y factores de crecimiento (sin suero animal), filtrado
///     estéril a 0,2 µm.
///  4. PROLIFERACIÓN: en biorreactores de tanque agitado sobre microportadores, a 37 °C, pH 7,2–7,4 y O₂ disuelto
///     controlado; las células se duplican cada ~24 h. Se renueva el medio cuando baja la glucosa y se corrige el pH.
///  5. DIFERENCIACIÓN: menos factores de crecimiento, sobre un ANDAMIO comestible (alginato/proteína vegetal) y con
///     estiramiento mecánico → las células se fusionan en fibras musculares; la grasa madura aparte.
///  6. COSECHA: se separa, se lava, se forma en piezas y va a la cámara fría → al COMEDERO de los depredadores.
/// La energía es la del santuario (solar). Las raciones son <see cref="FoodItem"/> de carne que los carnívoros comen.
/// </summary>
public class CulturedMeatLab : MonoBehaviour
{
    public enum Stage { Biopsia, Banco, Medio, Proliferacion, Diferenciacion, Cosecha }

    [Header("Tiempo comprimido (segundos de juego por fase)")]
    public float biopsySeconds = 20f, bankSeconds = 10f, mediumSeconds = 15f, secondsPerDoubling = 12f, differentiationSeconds = 30f, harvestSeconds = 10f;
    [Tooltip("Multiplica la velocidad (pruebas).")]
    public float speed = 1f;

    [Header("Lote")]
    public double seedCells = 1e9;          // un vial del banco
    public double cellsPerGram = 2.5e8;     // músculo: ≈ 2–3·10⁸ células por gramo
    public float batchGrams = 2000f;        // 2 kg por lote → 4 raciones de 500 g
    public int maxPortionsWaiting = 6;
    [Tooltip("En el santuario el laboratorio NO va solo: iniciar cada lote y cosecharlo son MISIONES de la jornada (alguien tiene que hacerlas). Proliferar y diferenciar sí van solos en el biorreactor.")]
    public bool requiresStaff = true;
    bool _startOrdered, _harvestOrdered;
    public bool WaitingForStaff => requiresStaff && ((stage == Stage.Biopsia && !_startOrdered) || (stage == Stage.Cosecha && !_harvestOrdered));
    /// <summary>Misión «Iniciar un lote»: biopsia del banco, medio y siembra.</summary>
    public void OrderStart(string who) { _startOrdered = true; Debug.Log($"[Carne cultivada] {who} inicia el lote de {Line} (vial del banco, medio y siembra en el biorreactor)."); }
    /// <summary>Misión «Cosechar el lote»: cosecha y formado (si aún no está listo, queda encargado).</summary>
    public void OrderHarvest(string who) { _harvestOrdered = true; Debug.Log($"[Carne cultivada] {who} cosecha y da forma al lote de {Line}."); }
    [Tooltip("Raciones de cada lote que van a la cocina del restaurante (el resto, al comedero de los depredadores).")]
    public int kitchenPortions = 4;

    /// <summary>Líneas celulares del banco: cada lote cultiva la siguiente. Los mamíferos y aves desde células satélite
    /// del músculo; los peces y mariscos desde células de músculo de pez (crecen a 27 °C, no a 37 °C).</summary>
    public static readonly string[] Lines = { "res", "pollo", "salmón", "cerdo", "atún", "conejo", "bacalao", "calamar", "gamba" };
    public string Line => Lines[batches % Lines.Length];

    [Header("Destino")]
    public Transform feeder;                // comedero de los depredadores
    public WorldLabel console;              // pantalla de la consola (estado del lote)

    public Stage stage { get; private set; } = Stage.Biopsia;
    public double cells { get; private set; }
    public float glucose { get; private set; } = 1f;     // fracción de la glucosa del medio fresco (4,5 g/L)
    public float pH { get; private set; } = 7.35f;
    public float dissolvedO2 { get; private set; } = 0.4f; // fracción de saturación del aire
    public int batches { get; private set; }
    public int portionsMade { get; private set; }
    float _t;

    public static readonly List<CulturedMeatLab> All = new List<CulturedMeatLab>();
    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    double TargetCells => batchGrams * cellsPerGram;

    void Update()
    {
        float dt = Time.deltaTime * speed;
        _t += dt;
        switch (stage)
        {
            case Stage.Biopsia: if (_t >= biopsySeconds && (!requiresStaff || _startOrdered)) { _startOrdered = false; Next(Stage.Banco); } break;   // alguien tiene que iniciar el lote
            case Stage.Banco: if (_t >= bankSeconds) { cells = seedCells; Next(Stage.Medio); } break;
            case Stage.Medio: if (_t >= mediumSeconds) { glucose = 1f; pH = 7.35f; Next(Stage.Proliferacion); } break;
            case Stage.Proliferacion:
                cells *= System.Math.Pow(2.0, dt / secondsPerDoubling);
                // Las células comen glucosa y sueltan lactato (baja el pH); el control renueva medio y añade base.
                glucose -= dt / secondsPerDoubling * 0.35f;
                pH -= dt / secondsPerDoubling * 0.08f;
                dissolvedO2 = Mathf.Lerp(dissolvedO2, 0.4f, dt);   // el agitador y la aireación lo mantienen en consigna
                if (glucose < 0.3f) { glucose = 1f; Debug.Log($"[Carne cultivada] renovación de medio (perfusión): {cells:0.0e0} células."); }
                if (pH < 7.2f) pH = 7.35f;   // dosificación de bicarbonato
                if (cells >= TargetCells) { cells = TargetCells; Next(Stage.Diferenciacion); }
                break;
            case Stage.Diferenciacion: if (_t >= differentiationSeconds) Next(Stage.Cosecha); break;
            case Stage.Cosecha:
                if (_t >= harvestSeconds && (!requiresStaff || _harvestOrdered)) { _harvestOrdered = false; Harvest(); Next(Stage.Biopsia); }   // y cosecharlo
                break;
        }
        if (console != null) console.text = Status();
    }

    void Next(Stage s)
    {
        stage = s; _t = 0f;
        Debug.Log($"[Carne cultivada] fase: {Name(s)}.");
    }

    void Harvest()
    {
        batches++;
        int n = Mathf.Max(1, Mathf.RoundToInt(batchGrams / 500f));
        int waiting = 0;
        if (feeder != null) foreach (Transform c in feeder) if (c.GetComponent<FoodItem>() != null) waiting++;
        n = Mathf.Min(n, maxPortionsWaiting - waiting);
        for (int i = 0; i < n; i++)
        {
            Vector3 p = feeder != null ? feeder.position + new Vector3((i % 2) * 0.6f - 0.3f, 0.15f, (i / 2) * 0.6f - 0.3f) : transform.position;
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = "Carne_cultivada";
            Object.Destroy(g.GetComponent<Collider>());
            if (feeder != null) g.transform.SetParent(feeder, true);
            g.transform.position = p; g.transform.localScale = new Vector3(0.22f, 0.08f, 0.16f);
            g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.72f, 0.3f, 0.3f));
            FoodItem fi = g.AddComponent<FoodItem>(); fi.material = OrganicMaterial.Meat; fi.grams = 500f;
            portionsMade++;
        }
        string line = Lines[(batches - 1) % Lines.Length];
        if (KitchenLarder.Instance != null) KitchenLarder.Instance.ReceiveMeat(MeatItem(line), kitchenPortions);
        Debug.Log($"[Carne cultivada] lote {batches} ({line}) cosechado: {n} raciones de 500 g al comedero de los depredadores{(KitchenLarder.Instance != null ? $" y {kitchenPortions} a la cocina" : "")}.");
        cells = 0;
    }

    /// <summary>Nombre del producto en el recetario (Gastronomy.Raw).</summary>
    public static string MeatItem(string line) => line switch
    {
        "pollo" => "pollo cultivado", "cerdo" => "cerdo cultivado", "conejo" => "conejo cultivado", "salmón" => "salmón cultivado",
        "atún" => "atún cultivado", "bacalao" => "bacalao cultivado", "calamar" => "calamar cultivado", "gamba" => "gamba cultivada",
        _ => "res cultivada",
    };

    public string Status() => WaitingForStaff
        ? $"Carne cultivada ({Line}) · {Name(stage)} · ESPERA a que alguien {(stage == Stage.Biopsia ? "inicie el lote" : "coseche el lote")} (misión de la jornada)"
        : stage == Stage.Proliferacion
        ? $"Carne cultivada ({Line}) · {Name(stage)} · {cells:0.0e0}/{TargetCells:0.0e0} células · glucosa {glucose:P0} · pH {pH:0.00} · O₂ {dissolvedO2:P0} · 37 °C"
        : $"Carne cultivada ({Line}) · {Name(stage)} · lotes {batches} · raciones {portionsMade}";

    public static string Name(Stage s) => s switch
    {
        Stage.Biopsia => "biopsia y aislamiento de células",
        Stage.Banco => "descongelando un vial del banco (N₂ líquido)",
        Stage.Medio => "preparando y filtrando el medio",
        Stage.Proliferacion => "proliferación en biorreactor",
        Stage.Diferenciacion => "diferenciación en andamio (fibras)",
        _ => "cosecha y formado",
    };

    // ── Maquinaria (cubos) ────────────────────────────────────────────────────────────────────

    /// <summary>Monta la maquinaria del laboratorio dentro de <paramref name="area"/> (suelo local y = 0, entrada a +Z).</summary>
    public static CulturedMeatLab Build(Transform area, Vector2 size, Transform feeder)
    {
        Color steel = KitchenProps.Steel, dark = KitchenProps.DarkSteel, white = KitchenProps.White;
        Color glass = new Color(0.82f, 0.94f, 1f, 0.25f), broth = new Color(0.95f, 0.75f, 0.6f, 0.75f), screen = new Color(0.12f, 0.25f, 0.32f);
        Transform r = new GameObject("Laboratorio_CarneCultivada").transform; r.SetParent(area, false);
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        void L(Transform t, string text, float h) { WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = text; wl.height = h; }

        // Entre equipo y equipo, 2 m libres (FurnitureSpacing.Standard, Berón 2026-10-28); la mesa del microscopio va pegada a la campana.
        // 1 · Campana de flujo laminar (cabina de bioseguridad) para la biopsia, con microscopio al lado.
        Transform hood = KitchenProps.Root(r, "Campana_flujo_laminar", new Vector3(-hx + 1.2f, 0f, -hz + 1f));
        KitchenProps.C(hood, new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.8f), white, 0f, "Mueble");
        KitchenProps.C(hood, new Vector3(0f, 1.35f, 0f), new Vector3(1.6f, 0.9f, 0.8f), white, 0f, "Cabina");
        KitchenProps.C(hood, new Vector3(0f, 1.3f, 0.41f), new Vector3(1.4f, 0.6f, 0.02f), glass, 0f, "Cristal");
        KitchenProps.C(hood, new Vector3(0f, 1.95f, 0f), new Vector3(1.6f, 0.3f, 0.8f), steel, 0f, "Filtro_HEPA");
        L(hood, "Campana de flujo laminar (biopsia → células satélite)", 2.3f);
        Transform mic = KitchenProps.Root(r, "Microscopio", new Vector3(-hx + 2.4f, 0.9f, -hz + 0.9f));
        KitchenProps.C(r, new Vector3(-hx + 2.4f, 0.45f, -hz + 0.9f), new Vector3(0.7f, 0.9f, 0.6f), white, 0f, "Mesa");
        KitchenProps.C(mic, new Vector3(0f, 0.15f, 0f), new Vector3(0.18f, 0.3f, 0.22f), dark, 0f, "Cuerpo");
        KitchenProps.C(mic, new Vector3(0f, 0.32f, -0.04f), new Vector3(0.05f, 0.12f, 0.05f), dark, 0f, "Ocular");
        L(mic, "Microscopio", 0.6f);

        // 2 · Banco de células: tanque criogénico de nitrógeno líquido.
        Transform cryo = KitchenProps.Root(r, "Tanque_criogenico", new Vector3(-hx + 0.8f, 0f, 0.2f));
        KitchenProps.C(cryo, new Vector3(0f, 0.55f, 0f), new Vector3(0.7f, 1.1f, 0.7f), steel, 45f, "Deposito");
        KitchenProps.C(cryo, new Vector3(0f, 1.15f, 0f), new Vector3(0.35f, 0.1f, 0.35f), dark, 0f, "Tapa");
        KitchenProps.C(cryo, new Vector3(0f, 1.25f, 0f), new Vector3(0.6f, 0.08f, 0.6f), new Color(0.9f, 0.97f, 1f, 0.35f), 0f, "Vapor_N2");
        L(cryo, "Banco de células (N₂ líquido, −196 °C)", 1.6f);
        Transform inc = KitchenProps.Root(r, "Incubadora_CO2", new Vector3(-hx + 0.8f, 0f, 3.2f));
        KitchenProps.C(inc, new Vector3(0f, 0.6f, 0f), new Vector3(0.75f, 1.2f, 0.75f), white, 0f, "Incubadora");
        KitchenProps.C(inc, new Vector3(0.38f, 0.6f, 0f), new Vector3(0.02f, 1f, 0.6f), glass, 0f, "Puerta");
        L(inc, "Incubadora (37 °C, 5 % CO₂)", 1.5f);

        // 3 · Preparación del medio: depósito con agitador y filtro estéril; autoclave.
        Transform med = KitchenProps.Root(r, "Preparacion_medio", new Vector3(-hx + 1.2f, 0f, hz - 1.3f));
        KitchenProps.C(med, new Vector3(0f, 0.6f, 0f), new Vector3(0.9f, 1.2f, 0.9f), steel, 0f, "Deposito_mezcla");
        KitchenProps.C(med, new Vector3(0f, 1.3f, 0f), new Vector3(0.12f, 0.2f, 0.12f), dark, 0f, "Motor_agitador");
        KitchenProps.C(med, new Vector3(0.7f, 0.9f, 0f), new Vector3(0.25f, 0.5f, 0.25f), white, 0f, "Filtro_0_2um");
        KitchenProps.C(med, new Vector3(0.35f, 0.9f, 0f), new Vector3(0.5f, 0.05f, 0.05f), steel, 0f, "Tubo");
        L(med, "Medio de cultivo (sin suero) + filtro estéril 0,2 µm", 1.7f);
        Transform auto = KitchenProps.Root(r, "Autoclave", new Vector3(-hx + 5.2f, 0f, hz - 1.0f));
        KitchenProps.C(auto, new Vector3(0f, 0.5f, 0f), new Vector3(0.8f, 1f, 0.7f), steel, 0f, "Autoclave");
        KitchenProps.C(auto, new Vector3(0f, 0.55f, 0.36f), new Vector3(0.45f, 0.45f, 0.03f), dark, 0f, "Puerta_redonda");
        L(auto, "Autoclave (esteriliza)", 1.3f);

        // 4 · Biorreactores de tanque agitado: semilla (pequeño) y producción (grande), con camisa, sensores y agitador.
        Bioreactor(r, "Biorreactor_semilla", new Vector3(-3.2f, 0f, -hz + 1.4f), 0.45f, 1.2f, glass, broth, steel, dark);
        L(r.Find("Biorreactor_semilla"), "Biorreactor semilla (50 L, microportadores)", 1.9f);
        Bioreactor(r, "Biorreactor_produccion", new Vector3(1.4f, 0f, -hz + 1.6f), 0.8f, 2.1f, glass, broth, steel, dark);
        L(r.Find("Biorreactor_produccion"), "Biorreactor de producción (500 L · 37 °C · pH 7,3 · O₂)", 2.9f);

        // 5 · Biorreactor de perfusión con andamio (diferenciación en fibras, con estiramiento).
        Transform perf = KitchenProps.Root(r, "Biorreactor_andamio", new Vector3(hx - 1.5f, 0f, -0.4f));
        KitchenProps.C(perf, new Vector3(0f, 0.45f, 0f), new Vector3(1.8f, 0.9f, 1f), steel, 0f, "Base");
        KitchenProps.C(perf, new Vector3(0f, 1.1f, 0f), new Vector3(1.7f, 0.4f, 0.9f), glass, 0f, "Camara");
        for (int i = 0; i < 6; i++)   // tiras de andamio con el tejido creciendo
            KitchenProps.C(perf, new Vector3(-0.7f + i * 0.28f, 1.05f, 0f), new Vector3(0.12f, 0.06f, 0.75f), new Color(0.85f, 0.55f, 0.5f), 0f, "Andamio_tejido");
        KitchenProps.C(perf, new Vector3(-0.95f, 1.1f, 0f), new Vector3(0.1f, 0.3f, 0.8f), dark, 0f, "Estirador");
        L(perf, "Perfusión con andamio comestible (fibras musculares)", 1.8f);

        // 6 · Cosecha: centrífuga, formadora y cámara fría.
        Transform cen = KitchenProps.Root(r, "Centrifuga", new Vector3(hx - 1.0f, 0f, hz - 1.4f));
        KitchenProps.C(cen, new Vector3(0f, 0.45f, 0f), new Vector3(0.8f, 0.9f, 0.8f), white, 0f, "Centrifuga");
        KitchenProps.C(cen, new Vector3(0f, 0.92f, 0f), new Vector3(0.6f, 0.04f, 0.6f), dark, 45f, "Tapa");
        L(cen, "Centrífuga (cosecha y lavado)", 1.3f);
        Transform form = KitchenProps.Root(r, "Formadora", new Vector3(hx - 4.0f, 0f, hz - 1.3f));
        KitchenProps.C(form, new Vector3(0f, 0.45f, 0f), new Vector3(1f, 0.9f, 0.7f), steel, 0f, "Mesa");
        KitchenProps.C(form, new Vector3(0f, 1.1f, 0f), new Vector3(0.5f, 0.4f, 0.5f), dark, 0f, "Prensa");
        L(form, "Formadora de piezas", 1.6f);
        Transform cold = KitchenProps.Root(r, "Camara_fria", new Vector3(hx - 0.6f, 0f, 3.5f));
        KitchenProps.C(cold, new Vector3(0f, 1f, 0f), new Vector3(0.9f, 2f, 1.4f), white, 0f, "Camara");
        KitchenProps.C(cold, new Vector3(-0.46f, 1f, 0f), new Vector3(0.02f, 1.8f, 1.2f), steel, 0f, "Puerta");
        L(cold, "Cámara fría (4 °C)", 2.3f);

        // Consola de control con el estado del lote.
        Transform con = KitchenProps.Root(r, "Consola_lote", new Vector3(0.3f, 0f, 0.8f));
        KitchenProps.C(con, new Vector3(0f, 0.5f, 0f), new Vector3(1.2f, 1f, 0.5f), white, 0f, "Mueble");
        KitchenProps.C(con, new Vector3(0f, 1.3f, 0f), new Vector3(1.1f, 0.6f, 0.05f), screen, 0f, "Pantalla");

        CulturedMeatLab lab = r.gameObject.AddComponent<CulturedMeatLab>();
        lab.feeder = feeder;
        lab.console = con.gameObject.AddComponent<WorldLabel>(); lab.console.height = 2f; lab.console.text = "Carne cultivada";
        return lab;
    }

    static void Bioreactor(Transform parent, string name, Vector3 pos, float radius, float height, Color glass, Color broth, Color steel, Color dark)
    {
        Transform b = KitchenProps.Root(parent, name, pos);
        for (int i = 0; i < 4; i++) KitchenProps.C(b, new Vector3((i % 2 * 2 - 1) * radius * 0.7f, 0.2f, (i / 2 * 2 - 1) * radius * 0.7f), new Vector3(0.08f, 0.4f, 0.08f), dark, 0f, "Pata");
        KitchenProps.C(b, new Vector3(0f, 0.4f + height * 0.5f, 0f), new Vector3(radius * 2f, height, radius * 2f), steel, 45f, "Camisa_termica");
        KitchenProps.C(b, new Vector3(0f, 0.4f + height * 0.5f, 0f), new Vector3(radius * 1.6f, height * 0.9f, radius * 1.6f), broth, 0f, "Caldo_de_celulas");
        KitchenProps.C(b, new Vector3(0f, 0.4f + height * 0.5f, radius * 1.01f), new Vector3(radius * 0.6f, height * 0.7f, 0.02f), glass, 0f, "Mirilla");
        KitchenProps.C(b, new Vector3(0f, 0.45f + height, 0f), new Vector3(radius * 1.2f, 0.12f, radius * 1.2f), steel, 0f, "Cabeza");
        KitchenProps.C(b, new Vector3(0f, 0.65f + height, 0f), new Vector3(0.14f, 0.25f, 0.14f), dark, 0f, "Motor_agitador");
        for (int s = 0; s < 3; s++)   // sondas: pH, O₂ disuelto, temperatura
            KitchenProps.C(b, new Vector3(-radius * 0.4f + s * radius * 0.4f, 0.65f + height, radius * 0.4f), new Vector3(0.04f, 0.3f, 0.04f), new Color(0.3f, 0.5f, 0.75f), 0f, "Sonda");
    }
}
