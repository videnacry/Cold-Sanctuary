using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA ROCA DE LAS CAVERNAS: holgura, umbrales y plazos (Berón 2026-11-05: «¿la tierra podría aplastar las instalaciones? ¿crearlas con mucho margen
/// para dar tiempo a actuar?»; docs/sitio-nuclear.md §7). El juego no tiene geología: se SUPONE roca cristalina dura y sana, como la de Forsmark, y no
/// sal ni arcilla. En esa roca el «aplastamiento» casi no existe (los extensómetros del SFR: ≤ 0,30 mm en 9 años); lo que sí pasa es que caiga una
/// cuña o suba el agua. Por eso la holgura se declara y se mide, pero lo que da tiempo es LEER: en WIPP, en sal, la convergencia avisó un mes antes.
/// </summary>
public static class CavernRule
{
    /// <summary>Holgura mínima (m) entre el forro de una caverna y el equipo principal, por los lados y por arriba.</summary>
    public const float ClearanceM = 3f;
    /// <summary>Dimensiones (m) en planta. Más luz NO es más seguro en roca: el techo sin apoyo es mayor (Gjøvik: 61 m; aquí, 16 como mucho).</summary>
    public const float ReactorLength = 20f, ReactorSpan = 12f, WasteLength = 24f, WasteSpan = 16f;
    /// <summary>De centro a centro de las dos cavernas (m): deja un pilar de roca de <see cref="PillarM"/> m.</summary>
    public const float WasteOffsetZ = 32f;
    /// <summary>Pilar de roca entre cavernas (m). Regla práctica, de memoria: al menos la luz mayor.</summary>
    public const float PillarM = WasteOffsetZ - (ReactorSpan + WasteSpan) * 0.5f;

    /// <summary>Ritmo de fondo (mm/día): el mayor del SFR, 0,30 mm en 1997–2006 con 0,02 mm de precisión.</summary>
    public const float BaseRateMmDay = 0.30f / (9f * 365f);
    /// <summary>Agua de fondo (l/min) de las dos cavernas: en el SFR, cada caverna da de 0 a 9 l/min.</summary>
    public const float BaseInflowLpm = 12f;
    /// <summary>UMBRALES DEL JUEGO, no de una norma: cada proyecto real fija los suyos. AVISO: varias veces el fondo. ALARMA: lo que en esta roca ya no es
    /// un ajuste (Gjøvik: 7–8 mm con 61 m de luz, al excavar). El ritmo cuenta más que el total: en WIPP, lo que avisó fue la aceleración.</summary>
    public const float AvisoRateMmDay = 0.1f, AlarmaRateMmDay = 1f, AvisoTotalMm = 1f, AlarmaTotalMm = 5f, AvisoInflow = 2f, AlarmaInflow = 5f;
    /// <summary>Días de ALARMA sin reforzar hasta declarar la emergencia del sitio.</summary>
    public const int EscalateDays = 3;
    /// <summary>Un episodio cada tantos días, de media, y nunca los primeros días.</summary>
    public const int EpisodeEvery = 90, FirstEpisodeDay = 30;

    public static CavernRock.Level Grade(float mmSinceZero, float rateMmDay, float inflowRatio)
    {
        if (mmSinceZero >= AlarmaTotalMm || rateMmDay >= AlarmaRateMmDay || inflowRatio >= AlarmaInflow) return CavernRock.Level.Alarma;
        if (mmSinceZero >= AvisoTotalMm || rateMmDay >= AvisoRateMmDay || inflowRatio >= AvisoInflow) return CavernRock.Level.Aviso;
        return CavernRock.Level.Normal;
    }

    /// <summary>Años hasta que un cierre de <paramref name="mmPerYear"/> se come <paramref name="marginM"/> de holgura.</summary>
    public static float YearsToClose(float marginM, float mmPerYear) => mmPerYear > 0f ? marginM * 1000f / mmPerYear : float.PositiveInfinity;

    static uint Hash(int day) => unchecked((uint)day * 2654435761u);
    /// <summary>¿Empieza un episodio el día <paramref name="day"/>? Fijo para cada día: la partida se puede repetir.</summary>
    public static bool EpisodeOn(int day) => day >= FirstEpisodeDay && (Hash(day) >> 16) % EpisodeEvery == 0;
    public static CavernRock.Cause EpisodeCause(int day) => ((Hash(day) >> 24) & 1) == 0 ? CavernRock.Cause.Cuna : CavernRock.Cause.Agua;
}

/// <summary>
/// LA VIGILANCIA DE LA ROCA (docs/sitio-nuclear.md §7): extensómetros, puntos de convergencia, células de carga en los bulones, geófonos, piezómetros y
/// aforos, más el asiento y la inclinación del cajón del TRIGA. Se lee a diario en el panel (<c>nuc_roca</c>); en AVISO, alguien baja por la línea, inspecciona
/// y refuerza (<c>nuc_bajada</c>); en ALARMA nadie baja, se refuerza desde arriba (<c>nuc_refuerzo</c>) y, si sigue, emergencia del sitio.
/// </summary>
public class CavernRock : MonoBehaviour
{
    public enum Level { Normal, Aviso, Alarma }
    public enum Cause { Ninguna, Cuna, Agua }

    public static CavernRock Instance { get; private set; }

    public List<Transform> extensometers = new List<Transform>(), convergence = new List<Transform>(), loadCells = new List<Transform>(),
        geophones = new List<Transform>(), piezometers = new List<Transform>(), flowMeters = new List<Transform>(), caissonGauges = new List<Transform>();
    [Tooltip("Holgura medida al construir (m): la menor de las dos cavernas.")]
    public float clearanceM;

    public float TotalMm { get; private set; }
    public float RateMmDay { get; private set; } = CavernRule.BaseRateMmDay;
    public float InflowLpm { get; private set; } = CavernRule.BaseInflowLpm;
    public Cause Episode { get; private set; }
    public int Readings { get; private set; }
    public int Inspections { get; private set; }
    public int Reinforcements { get; private set; }
    public int MicroEvents { get; private set; }
    public string LastReader { get; private set; } = "";

    float _zeroMm;      // el cero tras cada refuerzo: lo gastado no vuelve, pero cada refuerzo mide desde ahí
    int _lastDay = -1, _alarmSince = -1;

    public float MmSinceZero => TotalMm - _zeroMm;
    public Level Now => CavernRule.Grade(MmSinceZero, RateMmDay, InflowLpm / CavernRule.BaseInflowLpm);
    /// <summary>En ALARMA nadie baja a las cavernas (WIPP: el techo cayó 20 min después de que saliera la cuadrilla).</summary>
    public bool Closed => Now == Level.Alarma;
    public float MarginLeftM => clearanceM - TotalMm * 0.001f;
    /// <summary>Días hasta gastar la holgura a este ritmo. Es el plazo si todo fuera cierre lento; una cuña cae antes: por eso mandan los umbrales.</summary>
    public float DaysToAct => RateMmDay > 0f ? MarginLeftM * 1000f / RateMmDay : float.PositiveInfinity;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    static int Today => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    void Advance(int day)
    {
        if (_lastDay < 0) { _lastDay = day; return; }
        int steps = Mathf.Clamp(day - _lastDay, 0, 60);
        for (int i = 0; i < steps; i++)
        {
            int d = day - steps + 1 + i;
            if (Episode == Cause.Ninguna && CavernRule.EpisodeOn(d)) Begin(CavernRule.EpisodeCause(d));
            if (Episode == Cause.Cuna) { RateMmDay = Mathf.Min(RateMmDay * 2f, 20f); MicroEvents += 3; }
            else if (Episode == Cause.Agua) InflowLpm = Mathf.Min(InflowLpm * 1.5f, CavernRule.BaseInflowLpm * 10f);
            TotalMm += RateMmDay;
        }
        _lastDay = Mathf.Max(_lastDay, day);
    }

    /// <summary>Empieza un episodio: una cuña que se mueve o agua que sube (también la usa la prueba).</summary>
    public void Begin(Cause cause)
    {
        Episode = cause;
        if (cause == Cause.Cuna) { RateMmDay = Mathf.Max(RateMmDay, 0.2f); MicroEvents += 3; }
        else if (cause == Cause.Agua) InflowLpm = Mathf.Max(InflowLpm, CavernRule.BaseInflowLpm * 2.2f);
        Debug.LogWarning($"[Roca] Algo se mueve en las cavernas: {cause}.");
    }

    /// <summary>LEER LA ROCA en el panel (<c>nuc_roca</c>): lo que no se lee no avisa. Devuelve el nivel.</summary>
    public Level Read(string who) => Read(who, Today);
    public Level Read(string who, int day)
    {
        Advance(day);
        Readings++; LastReader = who;
        Level lv = Now;
        Debug.Log($"[Roca] {who} lee el día {day}: {MmSinceZero:0.00} mm desde el cero, {RateMmDay:0.000} mm/día, {InflowLpm:0} l/min, {MicroEvents} microsismos → {lv}. Holgura: {MarginLeftM:0.00} m.");
        if (lv != Level.Alarma) { _alarmSince = -1; return lv; }
        if (_alarmSince < 0) _alarmSince = day;
        else if (day - _alarmSince >= CavernRule.EscalateDays && !NuclearSite.Evacuating) NuclearSite.Instance?.DeclareEmergency($"la roca de las cavernas lleva {day - _alarmSince} días en alarma sin reforzar");
        return lv;
    }

    /// <summary>LA BAJADA (<c>nuc_bajada</c>): inspección a la vista; en AVISO, sanear y reforzar allí mismo.</summary>
    public void Inspect(string who) => Inspect(who, Today);
    public void Inspect(string who, int day)
    {
        Advance(day);
        Inspections++;
        Debug.Log($"[Roca] {who} baja por la línea e inspecciona las cavernas ({Now}).");
        if (Now == Level.Aviso) Reinforce(who);
    }

    /// <summary>REFORZAR (bulones, gunita, inyección; en ALARMA, a distancia): el movimiento para; lo gastado de la holgura no vuelve.</summary>
    public void Reinforce(string who)
    {
        if (Episode == Cause.Ninguna && Now == Level.Normal) return;
        _zeroMm = TotalMm; RateMmDay = CavernRule.BaseRateMmDay; InflowLpm = CavernRule.BaseInflowLpm * 1.1f; Episode = Cause.Ninguna; _alarmSince = -1;
        Reinforcements++;
        Debug.Log($"[Roca] {who} refuerza: la roca se para. Quedan {MarginLeftM:0.00} m de holgura.");
    }
}
