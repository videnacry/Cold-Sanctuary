using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA REGLA DE LA ENERGÍA (Berón 2026-11-05; docs/energia.md): cada piso declara su demanda (<see cref="FloorEnergy"/>), su creación añade paneles en
/// proporción (<see cref="TablesFor"/>) en el PARQUE ENERGÉTICO, y el validador (<see cref="Check"/>) comprueba que la oferta cubre la demanda. El reactor es la
/// carga base: en la noche polar el sol da cero. Aquí están las medidas, los modelos (sol, viento, marea) y las distancias de seguridad para los animales.
/// </summary>
public static class EnergyRule
{
    public const string Building = "Parque Energético", Area = "Energía";

    // ── EL SITIO ──
    /// <summary>Centro del casquete: la esquina noroeste del mar (el mar llega a ±600 m), a barlovento del santuario y del sitio nuclear.</summary>
    public static readonly Vector3 Center = new Vector3(-470f, 0f, 470f);
    /// <summary>Medio lado del casquete (m): 240 × 240 m.</summary>
    public const float CapHalf = 120f;
    /// <summary>Distancia mínima a todo edificio e isla: la de la lanzadera (un fallo de cohete no llega).</summary>
    public const float MinDistance = 300f;
    /// <summary>Primer piso del árbol: bajo, para que su sombra sea corta (en el casquete no crece ningún árbol, como en T14).</summary>
    public const float FirstFloorY = 12f;

    // ── LA DEMANDA ──
    /// <summary>Potencia media → punta (una base de servicios concentra el trabajo de día; supuesto de diseño).</summary>
    public const float PeakFactor = 1.6f;
    /// <summary>Potencia eléctrica del microrreactor (docs/sitio-nuclear.md): la única oferta firme.</summary>
    public const float ReactorKW = 5000f;
    /// <summary>Lo que consume el clúster de IA (docs/piso-informatica.md): 95 kW en total.</summary>
    public const float InformaticsKW = 95f;
    /// <summary>Cada casa del árbol (~1 kW de media): luz, cocina pequeña y carga.</summary>
    public const float HomeKW = 1f;
    /// <summary>(2026-11-05) Pisos sin SanctuaryArea: W/m² de oficina y de recreación (CBECS, como abajo) y de laboratorio. Lo fijo son estimaciones propias: las 14 consolas y el muro de la
    /// sala de guardia; y los ventiladores y filtros de la celda del árbol cazador, que no se pueden parar.</summary>
    public const float GuardRoomW = 16f, PlazaW = 18f, LabW = 45f, GuardRoomKW = 6f, ContainmentKW = 40f, GrowLightKW = 30f;
    /// <summary>La sala del árbol cazador: abajo no llega el sol, así que paga además sus luces de cultivo.</summary>
    public const float LabM2 = 34f * 30f;

    /// <summary>Vatios MEDIOS por m² de cada clase de piso. CBECS 2018 (tabla C22, en kWh/ft²) × 1,229: restaurante 50,7 → 62; clínica 43,8 → 54; recreación
    /// 15,0 → 18; educación y oficina 12,6–12,8 → 16; servicios 13,2 → 16. El laboratorio (45) es una estimación propia: ~3 oficinas.</summary>
    public static float WattsPerM2(SanctuaryAreaType t)
    {
        switch (t)
        {
            case SanctuaryAreaType.Kitchen: return 62f;
            case SanctuaryAreaType.Infirmary: case SanctuaryAreaType.VeterinaryClinic: case SanctuaryAreaType.SupplementsPharmacy: return 54f;
            case SanctuaryAreaType.AlchemyLab: case SanctuaryAreaType.FuelLab: case SanctuaryAreaType.CulturedMeatLab:
            case SanctuaryAreaType.AtomicPlant: case SanctuaryAreaType.Chemistry: case SanctuaryAreaType.Biology: return 45f;
            case SanctuaryAreaType.Gym: case SanctuaryAreaType.Calisthenics: case SanctuaryAreaType.YogaRoom: case SanctuaryAreaType.Ballet: case SanctuaryAreaType.MartialArts: case SanctuaryAreaType.Music: case SanctuaryAreaType.Assembly: return 18f;
            case SanctuaryAreaType.Garden: case SanctuaryAreaType.UnderwaterGarden: return 5f;   // bombas de riego; la luz es del sol
            case SanctuaryAreaType.VerticalFarm: return 5f;   // pasillos, vivero y lavado; los estantes pagan aparte su luz (FloorEnergy.Declare: VerticalFarm.LedKW)
            default: return 16f;   // oficinas, aulas, talleres, control (Informática: 16 + lo fijo)
        }
    }

    /// <summary>Lo que se apaga el último (si el reactor para, las baterías lo aguantan <see cref="EssentialHours"/> h): hospital, crías, cultivos y cámaras de la cocina.</summary>
    public static bool Essential(SanctuaryAreaType t) => t == SanctuaryAreaType.Infirmary || t == SanctuaryAreaType.VeterinaryClinic || t == SanctuaryAreaType.CubCare
        || t == SanctuaryAreaType.CulturedMeatLab || t == SanctuaryAreaType.Kitchen;

    // ── LOS PANELES ──
    /// <summary>Qué parte de la energía anual de cada piso pagan sus paneles: 1 de cada 5 kWh. Es lo que cabe en el casquete dejando reserva; no es una norma.</summary>
    public const float SolarShare = 0.2f;
    /// <summary>kWh por kWp al año: Troll (72° S) está diseñado para 957; aquí, algo menos.</summary>
    public const float AnnualYield = 900f;
    public const float ModuleKWp = 0.4f;
    /// <summary>Una MESA SOLAR: 12 × 3 módulos verticales bifaciales (24 × 3 m).</summary>
    public const int TableModules = 36;
    public const float TableKWp = TableModules * ModuleKWp, TableLength = 24f, TableHeight = 3f, RowPitch = 8f, TableGap = 2f;
    /// <summary>Mesas que monta una persona en un turno de <c>sol_instalacion</c>.</summary>
    public const int TablesPerShift = 4;
    /// <summary>Lo que quita la escarcha y la nieve, como mucho, si nadie limpia.</summary>
    public const float MaxSoil = 0.3f, SoilPerDay = 0.05f;
    /// <summary>Calibrado para <see cref="AnnualYield"/>: nubes y pérdidas (sin él, el modelo de cielo claro da ~1815 kWh/kWp).</summary>
    public const float Calibration = 0.5f;
    const float Bifaciality = 0.7f, Albedo = 0.7f;

    /// <summary>Las mesas que pide una demanda media de <paramref name="kW"/>.</summary>
    public static int TablesFor(float kW) => Mathf.CeilToInt(RequiredKWp(kW) / TableKWp - 1e-4f);
    /// <summary>Los kWp que pide una demanda media de <paramref name="kW"/>: su parte de la energía del año entre lo que rinde un kWp.</summary>
    public static float RequiredKWp(float kW) => Mathf.Max(0f, kW) * 8760f * SolarShare / AnnualYield;

    /// <summary>kW que da 1 kWp con este sol: mesas verticales bifaciales que miran al norte y al sur, cielo de Meinel y el reflejo de la nieve. Cero bajo el horizonte.</summary>
    public static float SolarKWPerKWp(float elevationDeg, float azimuthDeg)
    {
        float s = Mathf.Sin(elevationDeg * Mathf.Deg2Rad);
        if (s <= 0.01f) return 0f;
        float dni = 1.353f * Mathf.Pow(0.7f, Mathf.Pow(1f / s, 0.678f));
        float c = Mathf.Cos(elevationDeg * Mathf.Deg2Rad) * Mathf.Cos(azimuthDeg * Mathf.Deg2Rad);   // + = el sol da a la cara norte
        float direct = dni * (c > 0f ? c : -c * Bifaciality);
        float ground = Albedo * dni * s * 1.1f * 0.5f * (1f + Bifaciality);
        float diffuse = 0.1f * dni * 0.5f * (1f + Bifaciality);
        return (direct + ground + diffuse) * Calibration;
    }

    /// <summary>El sol del santuario en un momento del año (doy 1–365, hora 0–24).</summary>
    public static float SolarAt(float doy, float hour)
    {
        SunPos p = SolarPosition.SunFor(SolarPosition.SanctuaryLat, doy, hour);
        return SolarKWPerKWp(p.elevation, p.azimuth);
    }

    static float[] _monthly;
    /// <summary>kWh por kWp de cada mes (0 = enero), integrando el sol real del santuario.</summary>
    public static float[] MonthlyYield()
    {
        if (_monthly != null) return _monthly;
        var m = new float[12];
        for (int d = 1; d <= 365; d++)
        {
            int month = Mathf.Min(11, Mathf.FloorToInt((d - 1) / 30.42f));
            for (float h = 0.5f; h < 24f; h += 1f) m[month] += SolarAt(d, h);
        }
        return _monthly = m;
    }
    public static float YearYield() { float s = 0f; foreach (float m in MonthlyYield()) s += m; return s; }

    // ── EL VIENTO ──
    /// <summary>La turbina de Ross Island (Enercon E-33): 330 kW, rotor de 33 m, buje a 37 m.</summary>
    public const float TurbineKW = 330f, RotorD = 33f, HubH = 37f;
    public const int Turbines = 3;
    public const float CutIn = 3f, Rated = 12f, CutOut = 25f, MeanWind = 7.9f;
    /// <summary>Un ave a esta distancia para la turbina: 20 s de vuelo a 10 m/s (IdentiFlight ve ~1 km). Sin aves, rearme a los <see cref="ResumeSeconds"/> s.</summary>
    public const float BirdStopRadius = 200f, ResumeSeconds = 30f;
    /// <summary>A ≥ esta distancia de las islas: el doble de los 100 m de la guía más estricta (petreles gigantes). Ninguna norma da esta cifra para turbinas.</summary>
    public const float IslandBuffer = 200f;
    /// <summary>Cribado del lanzamiento de hielo (Seifert/DEWI): 1,5 × (buje + rotor).</summary>
    public static float IceThrowRadius => 1.5f * (HubH + RotorD);
    /// <summary>El refugio de aves, a ≥ el doble del radio de parada de las turbinas: el alimento y los animales concentrados atraen a otras aves (docs/energia.md §7). Elección de diseño.</summary>
    public const float RehabMinDistance = 2f * BirdStopRadius;
    /// <summary>Turnos de cura antes de soltarla.</summary>
    public const int RehabDays = 3;
    /// <summary>HABITUACIÓN: cuánto se acostumbran las aves a las figuras y a los drones con cada salida, cuánto olvidan al día y por debajo de qué fuerza ya no salen. Modelo del juego.</summary>
    public const float HabituationPerScare = 0.1f, HabituationDecay = 0.02f, MinDeterrent = 0.25f, HabituationReset = 0.4f;
    /// <summary>Alguien a esta distancia del pie de la torre la para (la revisión se hace parada).</summary>
    public const float TowerLockout = 30f;
    public static float TurbineTop => HubH + RotorD * 0.5f;

    /// <summary>Viento a la altura del buje (m/s) en la hora <paramref name="t"/> de juego. MODELO, no medida: media de Ross Island y tres oscilaciones lentas, sin estaciones.</summary>
    public static float WindSpeed(float t)
    {
        float g = 1f + 0.45f * Mathf.Sin(t * 0.073f) + 0.3f * Mathf.Sin(t * 0.211f + 1.3f) + 0.2f * Mathf.Sin(t * 0.47f + 0.4f);
        return Mathf.Max(0f, MeanWind * g);
    }

    /// <summary>Potencia de una turbina (kW) con viento <paramref name="v"/>: cúbica hasta la nominal; para por encima de <see cref="CutOut"/>.</summary>
    public static float WindPowerKW(float v)
    {
        if (v < CutIn || v > CutOut) return 0f;
        if (v >= Rated) return TurbineKW;
        float a = (v * v * v - CutIn * CutIn * CutIn) / (Rated * Rated * Rated - CutIn * CutIn * CutIn);
        return TurbineKW * a;
    }

    /// <summary>Energía media de una turbina (kW) a lo largo de un año de horas del modelo.</summary>
    public static float WindMeanKW()
    {
        float s = 0f; const int n = 8760;
        for (int i = 0; i < n; i++) s += WindPowerKW(WindSpeed(i));
        return s / n;
    }

    // ── LA MAREA ──
    /// <summary>La piloto: 2 rotores de 10 m, a 1 m/s de pico (McMurdo: máxima bajo el arranque de la mayoría de las turbinas; ERDC/CRREL TR-16-17). Por eso es un piloto.</summary>
    public const float TidalRotorD = 10f, PeakCurrent = 1f, TidalCp = 0.35f, TipSpeedRatio = 4.5f;
    public const int TidalUnits = 2;
    /// <summary>Tope de la punta: por debajo de los 5,6 m/s (IC 95 %: 4,4–6,6) del trauma grave en focas (Onoufriou et al., 2019).</summary>
    public const float MaxTipSpeed = 5f;
    /// <summary>Un mamífero a esta distancia para el rotor: el radio final de SeaGen.</summary>
    public const float MarineStopRadius = 50f;

    /// <summary>Corriente (m/s) a la hora <paramref name="t"/>: marea diurna (~25 h) con una modulación quincenal.</summary>
    public static float TidalCurrent(float t) => PeakCurrent * Mathf.Abs(Mathf.Sin(t * Mathf.PI / 25f)) * (0.65f + 0.35f * Mathf.Cos(t * 2f * Mathf.PI / 330f));

    /// <summary>Velocidad de la punta con esa corriente, con el regulador: nunca pasa de <see cref="MaxTipSpeed"/>.</summary>
    public static float TipSpeed(float current) => Mathf.Min(MaxTipSpeed, current * TipSpeedRatio);

    /// <summary>Potencia de un rotor (kW): ½ ρ A v³ Cp.</summary>
    public static float TidalPowerKW(float v)
    {
        float area = Mathf.PI * TidalRotorD * TidalRotorD * 0.25f;
        return 0.5f * 1027f * area * v * v * v * TidalCp / 1000f;
    }

    // ── EL ALMACÉN ──
    /// <summary>6 contenedores LFP de 5 MWh, calefactados (no se carga por debajo de 0 °C) y a 3 m unos de otros.</summary>
    public const int BatteryContainers = 6;
    public const float ContainerMWh = 5f, BatteryMWh = BatteryContainers * ContainerMWh, ContainerGap = 3f, SandMWh = 30f, BatteryEfficiency = 0.9f;
    /// <summary>Lo esencial aguanta sin reactor ni viento estas horas: las del agua pasiva de T14.</summary>
    public const float EssentialHours = 72f;

    // ── EL BALANCE Y EL VALIDADOR ──
    public sealed class Report
    {
        public float demandKW, peakKW, essentialKW, requiredKWp, installedKWp;
        public int floors, tablesRequired, tablesInstalled, tablesFree;
        public readonly List<string> problems = new List<string>();
        public bool Ok => problems.Count == 0;
        public override string ToString() =>
            $"{floors} pisos, {demandKW:0} kW de media y {peakKW:0} de punta ({essentialKW:0} esenciales); {installedKWp:0} de {requiredKWp:0} kWp ({tablesInstalled} de {tablesRequired} mesas, {tablesFree} libres)" +
            (Ok ? ": cumple" : ": " + string.Join("; ", problems));
    }

    /// <summary>La demanda media total (kW).</summary>
    public static float DemandKW() { float s = 0f; foreach (FloorEnergy f in FloorEnergy.All) if (f != null) s += f.KW; return s; }
    public static float EssentialKW() { float s = 0f; foreach (FloorEnergy f in FloorEnergy.All) if (f != null && f.essential) s += f.KW; return s; }

    /// <summary>Los paneles fuera del casquete (los techos y el hub) también cuentan.</summary>
    public static float RooftopKWp() { float s = 0f; foreach (SolarArray a in Object.FindObjectsByType<SolarArray>(FindObjectsSortMode.None)) if (a != null) s += a.peakWatts / 1000f; return s; }

    /// <summary>Todo piso sin declarar se declara por su clase: así un piso nuevo entra solo en el balance. Devuelve cuántos.</summary>
    public static int DeclareMissing()
    {
        int n = 0;
        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
            if (a != null && a.GetComponent<FloorEnergy>() == null) { FloorEnergy.Declare(a); n++; }
        return n;
    }

    /// <summary>LA REGLA: ¿cubre la oferta a la demanda? Con <paramref name="installedKWp"/> y sin el sitio (al construir) se puede validar antes de jugar.</summary>
    public static Report Check(float installedKWp, int tablesInstalled, int tablesFree)
    {
        var r = new Report { floors = FloorEnergy.All.Count, demandKW = DemandKW(), essentialKW = EssentialKW(), installedKWp = installedKWp, tablesInstalled = tablesInstalled, tablesFree = tablesFree };
        r.peakKW = r.demandKW * PeakFactor;
        r.requiredKWp = RequiredKWp(r.demandKW);
        r.tablesRequired = TablesFor(Mathf.Max(0f, r.demandKW - RooftopKWp() * AnnualYield / (8760f * SolarShare)));
        if (r.floors == 0) r.problems.Add("ningún piso ha declarado su demanda");
        if (ReactorKW < r.peakKW) r.problems.Add($"la punta ({r.peakKW:0} kW) pasa del reactor ({ReactorKW:0} kW)");
        if (installedKWp + 0.01f < r.requiredKWp) r.problems.Add($"faltan paneles: {installedKWp:0} de {r.requiredKWp:0} kWp");
        if (r.tablesRequired > tablesInstalled + tablesFree) r.problems.Add($"el casquete no da para {r.tablesRequired} mesas ({tablesInstalled + tablesFree} plazas)");
        float hours = r.essentialKW > 0f ? BatteryMWh * BatteryEfficiency * 1000f / r.essentialKW : 999f;
        if (hours < EssentialHours) r.problems.Add($"las baterías aguantan lo esencial {hours:0} h (pide {EssentialHours:0} h)");
        // Mes a mes: lo firme más la media del viento y del sol de ese mes. En la noche polar, el sol da cero y el reactor lo cubre.
        float wind = Turbines * WindMeanKW();
        float[] y = MonthlyYield();
        for (int m = 0; m < 12; m++)
        {
            float solar = installedKWp * y[m] / 730f;
            if (ReactorKW + wind + solar < r.demandKW) r.problems.Add($"el mes {m + 1} no llega: {ReactorKW + wind + solar:0} de {r.demandKW:0} kW");
        }
        return r;
    }
}
