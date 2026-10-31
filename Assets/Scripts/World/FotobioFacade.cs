using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MEDIDAS Y NÚMEROS DE LA FACHADA DE FOTOBIORREACTORES del Árbol de los Campos (Berón 2026-10-31; docs/fotobiorreactores.md).
/// Las comparten el constructor (SampleSceneBuilder.Fotobio.cs), la fachada en juego (<see cref="FotobioFacade"/>) y la prueba.
/// - ESPECIE: Arthrospira platensis (espirulina), la del compartimento IVa de MELiSSA (ESA): pH 9,5–10,5 (casi nada más
///   crece ahí), filamentos que se cosechan con malla, comestible.
/// - TRONCO: arpas de TUBOS horizontales de vidrio (Ø 7 cm, cada 14 cm) en módulos de 3 × 4 m, separados 1,4 m de la pared de
///   acrílico (por delante de los nervios), solo en el vano 0 de cada cara (el de la esquina, el más lejos del carril de nado,
///   que va frente a la puerta y la boca de la rama), entre 19,2 y 56 m. Berón: el tronco es transparente para VER NADAR entre
///   pisos; los nadadores suben por la columna a 6 m del eje frente a cada puerta (<see cref="ArbolCampos.SwimPath"/>), que
///   desde fuera cae en los vanos 1–4 de cada cara: el vano 0 no la tapa ni de frente ni desde el hielo (antes había arpas
///   también en el vano 2, que de frente tapaba la columna de la cara de enfrente; 2026-10-31, en Unity). De 0 a 19,2 m, de
///   56 m arriba y en los vanos 1–5 el tronco queda entero transparente.
/// - RAMAS: una QUILLA de paneles planos (5 cm de cultivo) colgada bajo el tramo largo de cada rama, entre los cordones de
///   abajo: no tapa los lados del tubo (se ve nadar por la rama) y queda por encima del árbol más alto (≥ 59,5 m).
/// </summary>
public static class FotobioSpec
{
    public const string RootName = "Fachada_fotobiorreactores", Title = "Fotobiorreactores de espirulina";

    // ── LA ESPECIE Y EL CULTIVO (docs/fotobiorreactores.md §1) ──
    /// <summary>Productividad de diseño por m² de superficie iluminada (g de biomasa seca / m² / día), con luz plena (sol o LED).
    /// Literatura de tubos al aire libre: 25–35 g/m²/d (Tredici y Chini Zittelli 1998; TURP, 35 g/m²/d en verano) y hasta 47,7
    /// por m² de instalación; una fachada vertical rinde menos (BIQ, Hamburgo: ~4 % de conversión, la mitad de lo previsto). Diseño
    /// conservador: 20.</summary>
    public const float DesignProductivity = 20f;
    /// <summary>Rango razonable (para la prueba): de la fachada BIQ a lo mejor publicado en tubos.</summary>
    public const float ProductivityMin = 10f, ProductivityMax = 48f;
    /// <summary>g de O₂ por g de biomasa seca (con nitrato como fuente de N): estequiometría de Cornet (CH1,65O0,53N0,17, grado de
    /// reducción 5,4 → 1,36 mol O₂/C-mol, ~1,6 g/g sin ceniza); MELiSSA mide un cociente fotosintético 1,2 ± 0,1. Diseño: 1,5.</summary>
    public const float O2PerGram = 1.5f;
    public const float O2PerGramMin = 1.2f, O2PerGramMax = 1.8f;
    /// <summary>O₂ que respira una persona al día (kg): NASA RP-1324 / BVAD, 0,84 kg.</summary>
    public const float O2PerPerson = 0.84f;
    /// <summary>Densidad del cultivo (g/L): se cosecha a <see cref="HarvestTo"/>; por encima de <see cref="SelfShade"/> se hace sombra
    /// a sí mismo y rinde menos.</summary>
    public const float HarvestTo = 0.7f, SelfShade = 1.4f, StartDensity = 0.9f;
    /// <summary>Temperatura del cultivo (°C): óptimo 30–35 (Tomaselli 1987: máximo a 35); &lt; 15–20 °C apenas crece y no aguanta
    /// luz fuerte. Se templa con el calor de la central (calefacción de distrito); de noche se deja bajar a ~22 °C (menos
    /// respiración).</summary>
    public const float TempDay = 33f, TempNight = 22f;
    /// <summary>pH de trabajo (medio de Zarrouk, 16,8 g/L de bicarbonato): 9,5–10,5 frena a Chlorella y a casi todo.</summary>
    public const float PhLow = 9.5f, PhHigh = 10.5f;

    // ── LUZ: SOL Y LED (la noche polar dura ~4 meses) ──
    /// <summary>Biomasa por mol de fotones absorbidos (g/mol): diseño 0,6 (prudente; el máximo teórico, ~1,8).</summary>
    public const float YieldOnLight = 0.6f;
    /// <summary>Eficacia de los LED hortícolas (µmol/J): 2,7–3,0 los buenos (DLC), −10 % por la funda estanca → 2,5.</summary>
    public const float LedEfficacy = 2.5f;

    // ── GEOMETRÍA DEL TRONCO (marco de cada lado de HelixTower: x a lo largo de la cara, y altura, z hacia fuera) ──
    /// <summary>Vanos (de 6 por cara, entre nervios) con arpa: solo el 0 (junto a la esquina). El carril de nado y las puertas
    /// van por el vano 4 (doorX = 5 m, boca de la rama en 6,5 m) y la columna de nado (a 6 m del eje frente a cada puerta) se ve
    /// desde fuera por los vanos 1–4: quedan libres del 1 al 5. Con el mismo número de módulos (32), la columna es el doble de alta.</summary>
    public static readonly int[] TrunkBays = { 0 };
    public const int BaysPerFace = 6;
    public const float ModuleW = 3f, ModuleH = 4f, ManifoldH = 0.3f;
    public static float ModulePitch => ModuleH + 2f * ManifoldH;   // 4,6 m
    public const int ModulesPerColumn = 8;
    /// <summary>Lo más alto del arpa del tronco: 4 m por debajo de las tornapuntas de los pisos (60 m).</summary>
    public const float TrunkTop = 56f;
    public static float TrunkBottom => TrunkTop - ModulesPerColumn * ModulePitch;   // 19,2 m
    /// <summary>Separación del plano de tubos a la cara de fuera del acrílico (los nervios sobresalen 0,6 m).</summary>
    public const float Standoff = 1.4f;
    public const float TubeD = 0.07f, TubePitch = 0.14f;
    public static int TubesPerModule => Mathf.FloorToInt(ModuleH / TubePitch);   // 28
    public static float ModuleArea => ModuleW * ModuleH;
    /// <summary>Litros de cultivo por m² de arpa (tubos de Ø 6 cm por dentro).</summary>
    public static float TubeLitersPerM2 => TubesPerModule * Mathf.PI * 0.03f * 0.03f * ModuleW * 1000f / ModuleArea;

    // ── QUILLA BAJO LAS RAMAS (marco del lado de la rama) ──
    public const int KeelPanels = 6;
    public const float KeelPanelW = 2.4f, KeelPanelH = 2f, KeelGap = 0.12f, KeelThick = 0.05f;
    public static float KeelPanelArea => KeelPanelW * KeelPanelH;
    /// <summary>Cuánto baja la cara de arriba de la quilla bajo la cara del nivel: hueco del tubo (0,3), pared (0,3), cordón (0,45 + 0,45)
    /// y colgador (0,2).</summary>
    public const float KeelDrop = 1.7f;

    /// <summary>Centro (x) del vano <paramref name="bay"/> de una cara.</summary>
    public static float BayCenter(HelixTower h, int bay) { float w = 2f * h.OuterHalf / BaysPerFace; return -h.OuterHalf + (bay + 0.5f) * w; }
    public static float PlaneZ(HelixTower h) => h.OuterHalf + Standoff;

    /// <summary>Rectángulos (marco del lado: x, y) de los módulos de una cara: 1 vano × 8 módulos.</summary>
    public static List<Rect> TrunkModules(HelixTower h)
    {
        var l = new List<Rect>();
        foreach (int bay in TrunkBays)
            for (int m = 0; m < ModulesPerColumn; m++)
                l.Add(new Rect(BayCenter(h, bay) - ModuleW * 0.5f, TrunkBottom + m * ModulePitch, ModuleW, ModulePitch));
        return l;
    }

    /// <summary>La quilla de la rama de un campo de punta (marco del lado): x de x0 a x1 (a lo largo del tramo largo), a z, de y0 a y1.</summary>
    public static (float x0, float x1, float z, float y0, float y1) Keel(HelixTower h, int level)
    {
        Vector3 col = h.ColumnS;
        float xB0 = h.mouthX + (h.tubeHalf - h.tubeWall), xB1 = col.x - h.ColOuter;
        float len = KeelPanels * KeelPanelW + (KeelPanels - 1) * KeelGap, xm = (xB0 + xB1) * 0.5f;
        float top = h.LevelY(level) - KeelDrop;
        return (xm - len * 0.5f, xm + len * 0.5f, col.z, top - KeelPanelH, top);
    }

    /// <summary>Superficie iluminada de la fachada (m²): el tronco (4 caras) y las quillas de las <paramref name="branches"/> ramas.</summary>
    public static float Area(int branches = 4) => 4 * TrunkBays.Length * ModulesPerColumn * ModuleArea + branches * KeelPanels * KeelPanelArea;
    /// <summary>Litros de cultivo.</summary>
    public static float Liters(int branches = 4) => 4 * TrunkBays.Length * ModulesPerColumn * ModuleArea * TubeLitersPerM2 + branches * KeelPanels * KeelPanelArea * KeelThick * 1000f;
    /// <summary>Biomasa (kg/día) y O₂ (kg/día) a pleno rendimiento.</summary>
    public static float BiomassKgDay(float area) => area * DesignProductivity / 1000f;
    public static float O2KgDay(float area) => BiomassKgDay(area) * O2PerGram;
    /// <summary>Potencia de los LED (kW) para dar la luz de diseño sin sol (noche polar).</summary>
    public static float LedKW(float area) => BiomassKgDay(area) * 1000f / YieldOnLight / (LedEfficacy * 1e-6f) / 86400f / 1000f;
    /// <summary>PPFD media en la superficie del cultivo con los LED (µmol/m²/s).</summary>
    public static float LedPPFD(float area) => BiomassKgDay(area) * 1000f / YieldOnLight / area / 86400f * 1e6f;

    // ── PESO (el tronco y las ramas lo cargan; StructuralLimits.RootCheck + esto) ──
    /// <summary>Peso (t) de un módulo del tronco: cultivo, vidrio borosilicato (2,5 mm, 2230 kg/m³), colectores y marco, LED y ménsulas.</summary>
    public static float ModuleTonnes => (TubeLitersPerM2 * ModuleArea + TubesPerModule * ModuleW * Mathf.PI * TubeD * 0.0025f * 2230f + 150f + 20f + 40f) / 1000f;
    /// <summary>Peso (t) de un panel de la quilla: cultivo, dos caras de acrílico de 8 mm, marco y colgadores.</summary>
    public static float KeelPanelTonnes => (KeelPanelArea * KeelThick * 1000f + 2f * KeelPanelArea * 0.008f * 1190f + 60f) / 1000f;
    /// <summary>Bombas, desgasificadores y tuberías de subida (t).</summary>
    public const float PlantTonnes = 3f;
    public static float TrunkTonnes => 4 * TrunkBays.Length * ModulesPerColumn * ModuleTonnes + PlantTonnes;
    public static float KeelTonnes => KeelPanels * KeelPanelTonnes;

    /// <summary>EL TRONCO AGUANTA LA FACHADA: la comprobación del árbol (<see cref="StructuralLimits.RootCheck"/>) más el peso de la
    /// fachada en el tronco (axil) y de cada quilla en el voladizo de su rama (a media rama). Devuelve el informe con la fachada.</summary>
    public static StructuralLimits.RootReport Structure(HelixTower h, out float facadeMN, out float keelMomentMN)
    {
        StructuralLimits.RootReport r = StructuralLimits.RootCheck(h, 4, 4, TreeSpec.RootHeight, TreeSpec.Ribs, TreeSpec.HoopSpacing, TreeSpec.HoopArea, TreeSpec.StrutDrop);
        const float g = 9.81f;
        facadeMN = (TrunkTonnes + 4f * KeelTonnes) * g / 1000f;
        var k = Keel(h, 1);
        float arm = new Vector2((k.x0 + k.x1) * 0.5f, k.z - h.OuterHalf).magnitude;   // de la cara del tronco al centro de la quilla
        keelMomentMN = KeelTonnes * g / 1000f * arm;
        r.axialMN += facadeMN;
        r.tipMoment += keelMomentMN;
        return r;
    }
}

/// <summary>
/// LA FACHADA DE FOTOBIORREACTORES EN JUEGO (docs/fotobiorreactores.md): sigue el cultivo de espirulina de toda la fachada
/// (un solo lote: los módulos están en serie por sus colectores) y lo enseña en su cuadro y en el color de los tubos.
/// - DENSIDAD (g/L): sube con lo que crece; la COSECHA diaria la baja a 0,7 g/L. Por encima de 1,4 g/L se hace sombra y rinde menos.
/// - NUTRIENTES (medio de Zarrouk, 0–1): se gastan al crecer; el cuidado diario los repone (bicarbonato, nitrato, fosfato, hierro).
/// - CONTAMINACIÓN (0–1): Chlorella, rotíferos, amebas; sube cada día (más si faltan nutrientes y baja el pH); el cuidado la mira al
///   microscopio, sube el pH a 10 y filtra.
/// - SALUD (0–1): va hacia lo que permiten las tres cosas; con mala salud los tubos amarillean (clorosis).
/// - LUZ: el sol (por su altura, <see cref="SolarPosition"/>) y los LED, que se encienden cuando el sol no da (la noche polar,
///   ~4 meses) y rellenan hasta la luz de diseño.
/// - O₂ (kg/día) = superficie × productividad × 1,5 g O₂/g; se compara con lo que respira la gente del santuario.
/// EL CUIDADO DIARIO es una misión obligatoria de 1 h de Biología («bio_fotobio»): quien la tiene va al laboratorio de biología
/// (muestra, microscopio, medio) y al terminar su turno se aplica <see cref="Care"/>. Sin esperas: todo se mira en Update.
/// </summary>
public class FotobioFacade : MonoBehaviour
{
    public const string MissionId = "bio_fotobio", MissionArea = "Biología";
    public const float MissionHours = 1f;

    public static FotobioFacade Instance { get; private set; }

    [Header("Construida")]
    public float areaM2, liters;
    public int trunkModules, keelPanels;
    [Header("Cultivo")]
    public float density = FotobioSpec.StartDensity;
    [Range(0f, 1f)] public float nutrients = 0.8f, contamination = 0.1f, health = 0.85f;
    public int lastCareDay = -1;
    public string lastCaredBy;
    public int timesCared;
    [Header("Ahora")]
    public float productivity, o2KgDay, ledKW, sunFactor;
    public bool ledOn;
    public float harvestedKgTotal, o2KgTotal;

    readonly List<Renderer> _culture = new List<Renderer>(), _leds = new List<Renderer>();
    WorldLabel _label;
    float _nextUi, _nextDuty;
    bool _present; int _dutyDay = int.MinValue, _caredDay = int.MinValue;
    SanctuaryArea _lab;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    void Start()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            if (r.name.StartsWith("Fotobio_cultivo")) _culture.Add(r);
            else if (r.name.StartsWith("Fotobio_LED")) _leds.Add(r);
        }
        Transform panel = transform.Find("Cuadro_fotobio");
        _label = panel != null ? panel.GetComponent<WorldLabel>() : null;
        Refresh(true);
    }

    // ── NÚMEROS ──
    /// <summary>Personas del santuario (el personal, la Magnate y el recién llegado; <see cref="PopulationPlan.Humans"/>).</summary>
    public static int People => Mathf.Max(1, PopulationPlan.Humans);
    public float PeopleCovered => o2KgDay / FotobioSpec.O2PerPerson;
    float NutF => Mathf.Clamp01(nutrients / 0.35f);
    float DenF => density < 0.3f ? density / 0.3f : density > FotobioSpec.SelfShade ? Mathf.Max(0.25f, 1f - (density - FotobioSpec.SelfShade) * 0.8f) : 1f;
    public float Ph => Mathf.Lerp(8.6f, 10.2f, Mathf.Clamp01(nutrients));
    public float Temperature => TimeController.timeController != null && TimeController.timeController.IsDay ? FotobioSpec.TempDay : FotobioSpec.TempNight;
    public float Wellbeing => Mathf.Clamp01(0.5f * health + 0.2f * nutrients + 0.15f * (1f - contamination) + 0.15f * DenF);

    /// <summary>Luz de ahora (0–1): el sol en una fachada vertical que gira, y los LED cuando no basta.</summary>
    public float Light()
    {
        TimeController tc = TimeController.timeController;
        float el = tc != null ? SolarPosition.Sanctuary(tc).elevation : 20f;
        sunFactor = Mathf.Clamp01(Mathf.Sin(Mathf.Max(0f, el) * Mathf.Deg2Rad) / Mathf.Sin(25f * Mathf.Deg2Rad)) * 0.9f;
        ledOn = sunFactor < 0.75f;
        ledKW = ledOn ? FotobioSpec.LedKW(areaM2) * (1f - sunFactor) : 0f;
        return ledOn ? 1f : Mathf.Clamp01(sunFactor / 0.75f);   // (con sol de sobra, el sol basta)
    }

    /// <summary>Avanza el cultivo <paramref name="days"/> días de juego (lo llama Update; las pruebas, directamente).</summary>
    public void Advance(float days)
    {
        if (days <= 0f || areaM2 <= 0f) return;
        float light = Light();
        productivity = FotobioSpec.DesignProductivity * light * health * NutF * DenF;
        float grams = productivity * areaM2 * days;
        density += grams / Mathf.Max(1f, liters);
        nutrients = Mathf.Clamp01(nutrients - 0.3f * days * productivity / FotobioSpec.DesignProductivity);
        contamination = Mathf.Clamp01(contamination + days * (0.05f + (nutrients < 0.3f ? 0.06f : 0f)));
        float target = 1f - 0.8f * contamination - (nutrients < 0.35f ? (0.35f - nutrients) * 2f : 0f) - (density > 1.8f ? (density - 1.8f) * 0.5f : 0f);
        target = Mathf.Clamp01(target);
        health = Mathf.MoveTowards(health, target, (target < health ? 0.35f : 0.2f) * days);
        o2KgDay = productivity * areaM2 * FotobioSpec.O2PerGram / 1000f;
        o2KgTotal += o2KgDay * days;
    }

    /// <summary>EL CUIDADO DIARIO: muestra (densidad, pH, microscopio), cosecha hasta 0,7 g/L (malla de 30–50 µm, lavado y prensado),
    /// medio de Zarrouk nuevo y limpieza de contaminantes (pH a 10, filtrado). Devuelve la salud antes y después.</summary>
    public (float before, float after) Care(string by)
    {
        float b = health;
        if (density > FotobioSpec.HarvestTo + 0.05f) { float kg = (density - FotobioSpec.HarvestTo) * liters / 1000f; harvestedKgTotal += kg; density = FotobioSpec.HarvestTo; }
        nutrients = 1f;
        contamination *= 0.25f;
        health = Mathf.Clamp01(health + 0.15f);
        lastCareDay = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        lastCaredBy = by; timesCared++;
        Refresh(true);
        Debug.Log($"[Fotobio] «{by}» cuida los fotobiorreactores: salud {b * 100f:0}% → {health * 100f:0}%, densidad {density:0.00} g/L, contaminación {contamination * 100f:0}%.");
        return (b, health);
    }

    /// <summary>La misión diaria (la añade <see cref="AreaWork.Today"/> si hay fachada).</summary>
    public static List<WorkRoster.Mission> Missions()
    {
        var l = new List<WorkRoster.Mission>();
        if (Instance != null)
            l.Add(new WorkRoster.Mission(MissionArea, MissionId, "Fotobiorreactores del Árbol: muestra (densidad, pH y microscopio), medio de Zarrouk, cosecha de espirulina y lavado", MissionHours, true));
        return l;
    }

    void Update()
    {
        TimeController tc = TimeController.timeController;
        float spd = tc != null ? Mathf.Max(1f, tc.SecondsPerGameDay) : 3600f;
        Advance(Time.deltaTime / spd);
        if (Time.time >= _nextDuty) { _nextDuty = Time.time + 1f; Duty(tc); }
        if (Time.time >= _nextUi) { _nextUi = Time.time + 1f; Refresh(false); }
    }

    // ¿Quién tiene hoy el cuidado? Si está en el laboratorio durante su turno, al acabarlo se aplica el cuidado.
    void Duty(TimeController tc)
    {
        WorkdayDirector wd = WorkdayDirector.Instance;
        if (tc == null || wd == null || wd.today == null || wd.plannedDay != tc.Day) return;
        WorkRoster.Assignment a = null;
        foreach (WorkRoster.Assignment x in wd.today.work) if (x.mission.id == MissionId) { a = x; break; }
        if (a == null) return;
        if (_dutyDay != tc.Day) { _dutyDay = tc.Day; _present = false; }
        float h = tc.Hour, end = a.start + a.mission.hours;
        if (h >= a.start && h < end && !_present)
        {
            if (a.worker.clone) _present = true;   // un clon de la Magnate: lo hace donde le toca
            else
            {
                GameObject body = WorkdayDirector.Body(a.worker.name);
                if (_lab == null) foreach (SanctuaryArea s in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None)) if (s.displayName == MissionArea) { _lab = s; break; }
                if (body != null && (_lab == null || Vector3.Distance(body.transform.position, _lab.transform.position) < 30f)) _present = true;
            }
        }
        if (h >= end && _caredDay != tc.Day)
        {
            _caredDay = tc.Day;
            if (_present) Care(a.worker.name);
            else Debug.LogWarning($"[Fotobio] «{a.worker.name}» no llegó al laboratorio de biología en su turno: hoy los fotobiorreactores se quedan sin cuidar.");
        }
    }

    Color _lastTint = Color.clear; bool _lastLed;
    void Refresh(bool force)
    {
        Color healthy = new Color(0.10f, 0.40f, 0.32f), sick = new Color(0.62f, 0.62f, 0.28f);
        Color tint = Color.Lerp(sick, healthy, Mathf.Round(health * 10f) / 10f);   // 11 tonos: pocos materiales
        if (force || tint != _lastTint) { _lastTint = tint; Material m = KitchenProps.Mat(tint); foreach (Renderer r in _culture) if (r != null) r.sharedMaterial = m; }
        Light();
        if (force || ledOn != _lastLed) { _lastLed = ledOn; Material m = KitchenProps.Mat(ledOn ? new Color(0.98f, 0.35f, 0.8f) : new Color(0.22f, 0.22f, 0.25f)); foreach (Renderer r in _leds) if (r != null) r.sharedMaterial = m; }
        if (_label != null)
            _label.text = $"{FotobioSpec.Title} · {areaM2:0} m² · densidad {density:0.00} g/L · pH {Ph:0.0} · {Temperature:0} °C · salud {health * 100f:0}%" +
                          $"{(contamination > 0.4f ? " · ¡contaminación!" : "")} · O₂ {o2KgDay:0.0} kg/día ({PeopleCovered:0.0} de {People} personas)" +
                          $" · {(ledOn ? $"LED {ledKW:0} kW" : "sol")}{(lastCareDay >= 0 ? $" · cuidado: {lastCaredBy}, día {lastCareDay}" : " · sin cuidar")}";
    }
}
