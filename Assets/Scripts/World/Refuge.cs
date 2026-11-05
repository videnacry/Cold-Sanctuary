using UnityEngine;

/// <summary>El refugio del fondo: cifras de refugios reales y de soporte vital cerrado (docs/sitio-nuclear.md §12 y §16). Un mes con máquinas y luz del reactor; 7 días solo con lo guardado.
/// La comida de un mes va guardada: un huerto no da de comer en 30 días si no estaba ya en marcha, y las gallinas gastan más comida que la que dan.</summary>
public static class RefugeRule
{
    /// <summary>Plazas: el carro lleno y algo más (el turno que puede estar en el sitio al declararse el aviso).</summary>
    public const int Capacity = 12;
    /// <summary>Lo que aguanta (h): un mes con corriente, y una semana con lo guardado (mina: 96 h; Chile 2010: 17 días hasta el primer contacto).</summary>
    public const float Hours = 720f, StoredHours = 168f;
    /// <summary>Por persona: m² de suelo mínimos, litros de oxígeno por hora (el dato de minas) y litros de agua por día.</summary>
    public const float FloorM2 = 1.4f, OxygenLitresHour = 37.4f, WaterLitresDay = 2.13f;
    /// <summary>Kilos de oxígeno que respira una persona al día y kWh para sacarlos del agua (6,8 kg/día con 2,18 kW: 7,7 kWh/kg).</summary>
    public const float OxygenKgDay = 0.84f, ElectrolysisKwhKg = 7.7f;
    /// <summary>Cifras del balance por persona y día (kWh), del agua y de la comida guardada. Depurar CO₂ y calor, estimación mía.</summary>
    public const float ScrubberKwhDay = 2f, ClimateKwhDay = 3f, WaterUseLitresDay = 6f, DesalinationKwhM3 = 3f, FoodKgDay = 0.6f;
    /// <summary>Lo que hay guardado (m³ de oxígeno y litros de agua) para <see cref="Capacity"/> personas y <see cref="StoredHours"/> horas.</summary>
    public static float OxygenM3 => Capacity * StoredHours * OxygenLitresHour / 1000f;
    public static float WaterLitres => Capacity * StoredHours / 24f * WaterLitresDay;
    /// <summary>kWh por persona y día: electrólisis, depurar CO₂, clima y agua. Sin huerto.</summary>
    public static float KwhPersonDay => OxygenKgDay * ElectrolysisKwhKg + ScrubberKwhDay + ClimateKwhDay + WaterUseLitresDay / 1000f * DesalinationKwhM3;
    /// <summary>Potencia media (kW) con el refugio lleno: casi nada para los 5 MWe de la central y mucho para unas baterías.</summary>
    public static float KwFull => KwhPersonDay * Capacity / 24f;
    /// <summary>Comida guardada (kg de alimento seco) para el mes.</summary>
    public static float FoodKg => Capacity * FoodKgDay * Hours / 24f;
    /// <summary>Lo que se pierde al día sin revisar (baterías, cartuchos de CO₂, caducidades) y el mínimo para trabajar abajo.</summary>
    public const float DailyLoss = 0.02f, MinReady = 0.8f;
    /// <summary>Sondeos a la mar: dos hechos de antemano, y lo que tarda la perforadora en abrir otro por los 60 m de roca (días; estimación mía).</summary>
    public const int SeaBores = 2;
    public const float NewBoreDays = 5f;
}

/// <summary>
/// EL REFUGIO DEL FONDO (Berón 2026-11-05: «se debe crear el refugio del fondo», y «¿se puede preparar para 1 mes?»; docs/sitio-nuclear.md §12 y §16): una sala estanca junto al pie
/// del pozo de escape. Con corriente del reactor por la roca, hace su agua y su oxígeno de la mar; sin ella, lo guardado da una semana. Quien queda abajo con la línea y el pozo cerrados
/// espera aquí (<see cref="SiteLine"/>). Si no está listo, no se baja a trabajar: la jornada manda antes revisarlo (<c>nuc_refugio</c>).
/// </summary>
public class Refuge : MonoBehaviour
{
    public static Refuge Instance { get; private set; }

    public const string MissionId = "nuc_refugio";
    [Tooltip("¿Le llega corriente por la roca? Sin ella no hay electrólisis ni ósmosis.")] public bool reactorFeed = true;
    [Tooltip("Sondeos a la mar abiertos.")] public int seaBores = RefugeRule.SeaBores;
    public int Services { get; private set; }
    public float HoursLeft { get; private set; } = RefugeRule.Hours;
    public int Occupants { get; private set; }
    int _lastService = int.MinValue;

    /// <summary>¿Hace agua y aire? Hace falta corriente y al menos un sondeo.</summary>
    public bool MakesOwn => reactorFeed && seaBores > 0;
    /// <summary>Lo que aguanta desde lleno, según haya máquinas o solo lo guardado.</summary>
    public float MaxHours => MakesOwn ? RefugeRule.Hours : RefugeRule.StoredHours;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    /// <summary>Lo que le queda de listo el día <paramref name="day"/> (1 = recién revisado).</summary>
    public float Readiness(int day)
    {
        if (_lastService == int.MinValue) _lastService = day;
        return Mathf.Clamp01(1f - (day - _lastService) * RefugeRule.DailyLoss);
    }
    public bool Ready(int day) => Readiness(day) >= RefugeRule.MinReady;
    static int Today => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    /// <summary>LA REVISIÓN (<c>nuc_refugio</c>): cambia cartuchos y caducados, carga baterías, arranca las máquinas, purga los sondeos y prueba las comunicaciones.</summary>
    public void Service(string who) => Service(who, Today);
    public void Service(string who, int day)
    {
        _lastService = day; Services++; HoursLeft = MaxHours;
        Debug.Log($"[Refugio] {who} lo revisa: {RefugeRule.Capacity} plazas, {MaxHours / 24f:0} días, {RefugeRule.KwFull:0.0} kW lleno.");
    }

    public void Enter() { Occupants++; HoursLeft = Mathf.Min(HoursLeft, MaxHours); Debug.LogWarning($"[Refugio] Entra alguien: {Occupants} dentro. Quedan {HoursLeft:0} h."); }
    public void Leave() { Occupants = Mathf.Max(0, Occupants - 1); }
    /// <summary>Gasta <paramref name="hours"/> h de juego.</summary>
    public void Use(float hours) { HoursLeft = Mathf.Max(0f, HoursLeft - hours); }
}
