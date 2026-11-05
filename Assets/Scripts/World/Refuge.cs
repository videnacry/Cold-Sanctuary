using UnityEngine;

/// <summary>El refugio del fondo: cifras de refugios reales (docs/sitio-nuclear.md §12). Mina de EE. UU.: 96 h, ≥ 1,4 m² por persona y 2,1 l de agua al día;
/// rescate de submarinos: primera ayuda en 96 h, y se estudian escenarios de 172 h. Aquí, 7 días: nadie llega rápido a 566 m.</summary>
public static class RefugeRule
{
    /// <summary>Plazas: el carro lleno y algo más (el turno que puede estar en el sitio al declararse el aviso).</summary>
    public const int Capacity = 12;
    public const float Hours = 168f;
    /// <summary>Por persona: m² de suelo, litros de oxígeno por hora y litros de agua por día.</summary>
    public const float FloorM2 = 1.4f, OxygenLitresHour = 37.4f, WaterLitresDay = 2.13f;
    /// <summary>Lo que hace falta guardar (m³ de oxígeno y litros de agua) para <see cref="Capacity"/> personas y <see cref="Hours"/> horas.</summary>
    public static float OxygenM3 => Capacity * Hours * OxygenLitresHour / 1000f;
    public static float WaterLitres => Capacity * Hours / 24f * WaterLitresDay;
    /// <summary>Lo que se pierde al día sin revisar (baterías, cartuchos de CO₂, caducidades) y el mínimo para trabajar abajo.</summary>
    public const float DailyLoss = 0.02f, MinReady = 0.8f;
}

/// <summary>
/// EL REFUGIO DEL FONDO (Berón 2026-11-05: «se debe crear el refugio del fondo»; docs/sitio-nuclear.md §12): una sala estanca junto al pie del pozo de escape, con aire,
/// agua y comunicaciones propias. Quien queda abajo con la línea y el pozo cerrados espera aquí (<see cref="SiteLine"/>). Si no está listo, no se baja a trabajar
/// (como en las minas): la jornada manda antes revisarlo (<c>nuc_refugio</c>).
/// </summary>
public class Refuge : MonoBehaviour
{
    public static Refuge Instance { get; private set; }

    public const string MissionId = "nuc_refugio";
    public int Services { get; private set; }
    public float HoursLeft { get; private set; } = RefugeRule.Hours;
    public int Occupants { get; private set; }
    int _lastService = int.MinValue;

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

    /// <summary>LA REVISIÓN (<c>nuc_refugio</c>): cambia cartuchos y caducados, carga baterías y prueba las comunicaciones.</summary>
    public void Service(string who) => Service(who, Today);
    public void Service(string who, int day)
    {
        _lastService = day; Services++; HoursLeft = RefugeRule.Hours;
        Debug.Log($"[Refugio] {who} lo revisa: {RefugeRule.Capacity} plazas, {RefugeRule.Hours:0} h, {RefugeRule.OxygenM3:0} m³ de oxígeno y {RefugeRule.WaterLitres:0} l de agua.");
    }

    public void Enter() { Occupants++; Debug.LogWarning($"[Refugio] Entra alguien: {Occupants} dentro. Quedan {HoursLeft:0} h."); }
    public void Leave() { Occupants = Mathf.Max(0, Occupants - 1); }
    /// <summary>Gasta <paramref name="hours"/> h de juego de aire y agua.</summary>
    public void Use(float hours) { HoursLeft = Mathf.Max(0f, HoursLeft - hours); }
}
