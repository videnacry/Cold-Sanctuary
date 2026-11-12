using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL HUERTO VERTICAL (Berón 2026-11-08; docs/produccion-maquinaria.md §2026-11-08): un piso de ESTANTES CON LED en la torre de Producción
/// que completa la verdura que los bancales del Árbol de los Campos no llegan a dar. Regla P19: la maquinaria sale de la demanda, no de un número:
/// <see cref="RacksFor"/> pide los estantes que faltan para <see cref="DemandKgDay"/> (personas × 0,8 kg) con lo que ya dan los bancales.
/// - RENDIMIENTO: <see cref="YieldKgM2Year"/> = 48,3 kg de peso fresco por m² de bandeja y año, la MEDIANA de la literatura para lechuga en granjas
///   verticales (Pennisi et al. 2025, Agronomy for Sustainable Development 45:57: media 59,7 ± 5,8, mediana 48,3, n = 54; lo que declaran las
///   empresas, 105). Se toma la mediana: prudente.
/// - ENERGÍA: 0,08–0,13 kg por kWh en la misma revisión (7,7–12,5 kWh/kg); diseño <see cref="KWhPerKg"/> = 10 kWh por kg (luz, clima y bombas).
/// - ESTANTE: NFT de 4 bandejas de 3,0 × 1,2 m (14,4 m² de cultivo), la de arriba a 1,9 m (la alcanza la persona más alta sin subirse a nada).
/// </summary>
public class VerticalFarm : MonoBehaviour
{
    public const string AreaName = "Huerto vertical", AreaObject = "VerticalFarm_Area", RackPrefix = "Estanteria_LED";
    /// <summary>Verdura por persona y día (kg): la de la línea del Huerto de <see cref="ProductionAudit"/>.</summary>
    public const float VegKgPerPersonDay = 0.8f;
    /// <summary>Lo que da un bancal al aire libre del Árbol (kg/m²/año), el de la auditoría de 2026-11-05 [A].</summary>
    public const float FieldKgM2Year = 3f;
    /// <summary>Mediana de la literatura, lechuga en granja vertical (kg de peso fresco / m² de bandeja / año; Pennisi et al. 2025).</summary>
    public const float YieldKgM2Year = 48.3f;
    /// <summary>Electricidad por kg (kWh): 1 / 0,1 kg/kWh (Pennisi et al. 2025: 0,08–0,13).</summary>
    public const float KWhPerKg = 10f;
    public const int Tiers = 4;
    public const float TierW = 3f, TierD = 1.2f, TopTierY = 1.9f;
    /// <summary>Reserva sobre la demanda: una bandeja que se pierde (pythium, una bomba parada) no deja a nadie sin verdura. Supuesto de diseño.</summary>
    public const float Reserve = 1.1f;
    /// <summary>Estantes que caben en un piso de la torre de Producción con su vivero, su lavado y su cámara (2 m entre todo): si la demanda pide más,
    /// otro piso.</summary>
    public const int RacksPerFloor = 12;

    public static float RackM2 => Tiers * TierW * TierD;                       // 14,4 m²
    public static float RackKgDay => RackM2 * YieldKgM2Year / 365f;             // ~1,9 kg/día
    public static float RackKW => RackKgDay * KWhPerKg / 24f;                   // ~0,8 kW de media

    /// <summary>La verdura que pide el santuario al día: sus personas (<see cref="PopulationPlan.People"/>) × 0,8 kg.</summary>
    public static float DemandKgDay => PopulationPlan.People * VegKgPerPersonDay;

    /// <summary>Lo que dan los bancales de la escena (kg/día): su superficie × 3 kg/m²/año.</summary>
    public static float FieldsKgDay()
    {
        float m2 = 0f;
        foreach (GardenPlot p in Object.FindObjectsByType<GardenPlot>(FindObjectsInactive.Include, FindObjectsSortMode.None)) m2 += p.size.x * p.size.y;
        return m2 * FieldKgM2Year / 365f;
    }

    /// <summary>LA REGLA P19: los estantes que hacen falta para <paramref name="demandKgDay"/> con lo que ya dan los bancales (y la reserva).</summary>
    public static int RacksFor(float demandKgDay, float fieldsKgDay) => Mathf.Max(0, Mathf.CeilToInt((demandKgDay * Reserve - fieldsKgDay) / RackKgDay - 1e-4f));
    public static int FloorsFor(int racks) => Mathf.Max(1, Mathf.CeilToInt(racks / (float)RacksPerFloor));

    /// <summary>Estantes que hay en la escena (los de todos los huertos verticales).</summary>
    public static int RacksInScene()
    {
        int n = 0;
        foreach (VerticalFarm f in Object.FindObjectsByType<VerticalFarm>(FindObjectsInactive.Include, FindObjectsSortMode.None)) n += f.Racks;
        return n;
    }
    /// <summary>Lo que dan los estantes de la escena (kg/día).</summary>
    public static float CapacityKgDay() => RacksInScene() * RackKgDay;

    public static VerticalFarm Instance { get; private set; }
    void OnEnable() { if (Instance == null) Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    [Tooltip("Estantes de este piso (los cuenta el constructor; Racks los vuelve a contar en la escena).")]
    public int racksBuilt;
    public float harvestedKg;
    public int lastHarvestDay = -1, lastSowDay = -1, lastCheckDay = -1;
    public string lastBy;

    /// <summary>Estantes de este piso: los hijos «Estanteria_LED…».</summary>
    public int Racks
    {
        get
        {
            int n = 0;
            foreach (Transform t in GetComponentsInChildren<Transform>(true)) if (t.name.StartsWith(RackPrefix)) n++;
            return n;
        }
    }
    /// <summary>Potencia media de los LED, la climatización y las bombas de este piso (kW).</summary>
    public float LedKW => Racks * RackKW;

    static int Today => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    // Los efectos de sus misiones (AreaFunctions: huv_siembra, huv_cosecha, huv_solucion).
    public void Sow(string who) { lastSowDay = Today; lastBy = who; Debug.Log($"[Huerto vertical] «{who}» siembra las bandejas de germinación y trasplanta las plántulas a los canales."); }
    public void Harvest(string who)
    {
        float kg = Racks * RackKgDay;
        harvestedKg += kg; lastHarvestDay = Today; lastBy = who;
        Debug.Log($"[Huerto vertical] «{who}» cosecha {kg:0.0} kg de verdura de {Racks} estantes y la lleva a la cámara para la cocina.");
    }
    public void CheckSolution(string who) { lastCheckDay = Today; lastBy = who; Debug.Log($"[Huerto vertical] «{who}» mide la solución nutritiva (EC 1,2–1,8 mS/cm, pH 5,5–6,5) y limpia los canales."); }
}
