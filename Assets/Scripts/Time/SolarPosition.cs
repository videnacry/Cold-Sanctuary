using UnityEngine;

public enum SunPhase { Day, CivilTwilight, NauticalTwilight, AstronomicalTwilight, Night }

public struct SunPos
{
    public float elevation;        // grados sobre el horizonte (sin refracción)
    public float azimuth;          // grados desde el norte, en el sentido de las agujas del reloj (90 = este)
    public float declination;      // grados
    public float sunrise, sunset;  // horas en la misma base que la hora pedida; NaN si no hay orto u ocaso
    public bool midnightSun, polarNight;
    public SunPhase phase;
}

/// <summary>
/// POSICIÓN DEL SOL por lugar, fecha y hora (ecuaciones de la NOAA; docs/luz-microcosmos.md). La comparten el santuario
/// (polar: <see cref="SunCycle"/>) y la plantilla de los microcosmos, para que cada lugar y hora tenga su luz real
/// (Berón 2026-10-27). Convenio de la escena: +Z = norte, +X = este.
/// </summary>
public static class SolarPosition
{
    /// <summary>El santuario: la isla de Ross (latitud de McMurdo). A 78° S hay sol de medianoche, noche polar y, entre
    /// medias, días y noches; en el polo exacto la hora no cambiaría la luz.</summary>
    public const float SanctuaryLat = -77.85f, SanctuaryLon = 166.67f;
    /// <summary>Día del año con el que empieza la partida: 21 sep, equinoccio de primavera austral.</summary>
    public const float SanctuaryStartDoy = 264f;

    /// <param name="lat">Latitud en grados (+N, −S).</param>
    /// <param name="dayOfYear">Día del año 1–365 (admite decimales).</param>
    /// <param name="hour">Hora 0–24; por defecto, hora SOLAR MEDIA LOCAL.</param>
    /// <param name="lon">Longitud (+E); solo cuenta si se da <paramref name="utcOffset"/>.</param>
    /// <param name="utcOffset">Huso de <paramref name="hour"/> (−5 = EST). Null: hora solar local.</param>
    public static SunPos SunFor(float lat, float dayOfYear, float hour, float lon = 0f, float? utcOffset = null)
    {
        double tz = utcOffset ?? lon / 15.0;
        double g = 2.0 * Mathf.PI / 365.0 * (dayOfYear - 1 + (hour - tz - 12.0) / 24.0);
        double eqt = 229.18 * (0.000075 + 0.001868 * System.Math.Cos(g) - 0.032077 * System.Math.Sin(g) - 0.014615 * System.Math.Cos(2 * g) - 0.040849 * System.Math.Sin(2 * g));
        double decl = 0.006918 - 0.399912 * System.Math.Cos(g) + 0.070257 * System.Math.Sin(g) - 0.006758 * System.Math.Cos(2 * g) + 0.000907 * System.Math.Sin(2 * g)
                      - 0.002697 * System.Math.Cos(3 * g) + 0.00148 * System.Math.Sin(3 * g);
        double tst = hour * 60.0 + eqt + 4.0 * lon - 60.0 * tz;
        double ha = (tst / 4.0 - 180.0) * Mathf.Deg2Rad, phi = lat * Mathf.Deg2Rad;
        double cosZ = System.Math.Sin(phi) * System.Math.Sin(decl) + System.Math.Cos(phi) * System.Math.Cos(decl) * System.Math.Cos(ha);
        cosZ = System.Math.Max(-1.0, System.Math.Min(1.0, cosZ));
        var s = new SunPos();
        s.declination = (float)(decl * Mathf.Rad2Deg);
        s.elevation = (float)(90.0 - System.Math.Acos(cosZ) * Mathf.Rad2Deg);
        double az = System.Math.Atan2(System.Math.Sin(ha), System.Math.Cos(ha) * System.Math.Sin(phi) - System.Math.Tan(decl) * System.Math.Cos(phi)) * Mathf.Rad2Deg + 180.0;
        s.azimuth = (float)((az % 360.0 + 360.0) % 360.0);
        double x = System.Math.Cos(90.833 * Mathf.Deg2Rad) / (System.Math.Cos(phi) * System.Math.Cos(decl)) - System.Math.Tan(phi) * System.Math.Tan(decl);
        s.sunrise = s.sunset = float.NaN;
        if (x <= -1.0) s.midnightSun = true;
        else if (x >= 1.0) s.polarNight = true;
        else
        {
            double h0 = System.Math.Acos(x) * Mathf.Rad2Deg;
            s.sunrise = (float)((720.0 - 4.0 * (lon + h0) - eqt) / 60.0 + tz);
            s.sunset = (float)((720.0 - 4.0 * (lon - h0) - eqt) / 60.0 + tz);
        }
        float el = s.elevation;
        s.phase = el >= -0.833f ? SunPhase.Day : el >= -6f ? SunPhase.CivilTwilight : el >= -12f ? SunPhase.NauticalTwilight : el >= -18f ? SunPhase.AstronomicalTwilight : SunPhase.Night;
        return s;
    }

    /// <summary>Nivel de luz 0–1 por la elevación del sol (día, crepúsculos, noche).</summary>
    public static float Level(float el)
    {
        if (el >= 0f) return 0.35f + 0.65f * Mathf.Sqrt(Mathf.Sin(el * Mathf.Deg2Rad));
        if (el >= -6f) return 0.12f + 0.23f * (el + 6f) / 6f;
        if (el >= -18f) return 0.04f + 0.08f * (el + 18f) / 12f;
        return 0.04f;
    }

    static Color Hex(int rgb) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);

    /// <summary>Color de la luz por la elevación: noche azul, crepúsculo rosado, sol bajo dorado, alto blanco cálido.</summary>
    public static Color Tint(float el)
    {
        Color night = Hex(0x738CD9), dusk = Hex(0xFF8C66), gold = Hex(0xFF9E59), low = Hex(0xFFD9B2), high = Hex(0xFFF7EB);
        if (el < -6f) return night;
        if (el < 0f) return Color.Lerp(night, dusk, (el + 6f) / 6f);
        if (el < 10f) return Color.Lerp(gold, low, el / 10f);
        if (el < 40f) return Color.Lerp(low, high, (el - 10f) / 30f);
        return high;
    }

    /// <summary>Pone la luz direccional como ese sol. Bajo el horizonte pasa a ser la LUNA: tenue, fría y en el lado
    /// opuesto del cielo (la noche polar se puede jugar). Devuelve el nivel de luz.</summary>
    public static float Apply(Light light, SunPos s)
    {
        float L = Level(s.elevation);
        if (light == null) return L;
        bool up = s.elevation > -0.833f;
        // La luz VIAJA desde el sol: apunta al lado contrario de su acimut.
        light.transform.rotation = up ? Quaternion.Euler(Mathf.Max(s.elevation, 2f), s.azimuth + 180f, 0f) : Quaternion.Euler(25f, s.azimuth, 0f);
        light.color = Tint(s.elevation);
        light.intensity = up ? 1.2f * L : Mathf.Max(0.3f, 1.2f * L);   // la luna alumbra lo justo para jugar
        return L;
    }

    /// <summary>Día del año en el santuario: el año de juego dura <see cref="TimeController.DaysPerYear"/> días, así que la
    /// estación avanza ~13 días por día de juego (también dentro del día).</summary>
    public static float SanctuaryDayOfYear(TimeController t)
    {
        float days = t.Day + t.Hour / 24f;
        return Mathf.Repeat(SanctuaryStartDoy - 1f + days * 365.2422f / Mathf.Max(1f, t.DaysPerYear), 365f) + 1f;
    }

    /// <summary>El sol del santuario ahora mismo.</summary>
    public static SunPos Sanctuary(TimeController t) => SunFor(SanctuaryLat, SanctuaryDayOfYear(t), t.Hour);

    /// <summary>Lugar y día del año de cada microcosmos (docs/luz-microcosmos.md §2; * = fecha representativa).</summary>
    public static bool GeoOf(string display, out float lat, out float lon, out float doy)
    {
        lat = lon = doy = 0f;
        if (string.IsNullOrEmpty(display)) return false;
        (string key, float la, float lo, float d)[] table =
        {
            ("Sakshi", 60f, 60f, 121f), ("Ambrosio", 60f, 60f, 182f), ("Enfermer", 60f, 60f, 182f), ("Hospital", 60f, 60f, 182f),
            ("Cocina", 32.10f, 34.97f, 288f), ("Huerto", 31.87f, 35.44f, 91f), ("Textil", 48.88f, 16.65f, 288f),
            ("Mecánica", 48.88f, 16.65f, 182f), ("Mesopotamia", 31.32f, 45.64f, 91f), ("Patanjali", 25.32f, 83.01f, 15f),
            ("Kapilavastu", 27.57f, 83.05f, 288f), ("Egina", 37.75f, 23.53f, 152f), ("Alejandría", 31.20f, 29.92f, 172f),
            ("Escútari", 41.00f, 29.02f, 319f), ("Dayton", 36.02f, -75.67f, 351f), ("Tregua", 50.74f, 2.88f, 359f),
            ("Chambon", 45.06f, 4.30f, 288f), ("Sendler", 52.25f, 21.00f, 288f), ("Atómica", 52.81f, -0.63f, 253f),
            ("Bruce Lee", 22.28f, 114.17f, 258f),   // Hong Kong, mediados de septiembre (empieza en la escuela de Yip Man)
        };
        foreach (var e in table)
            if (display.IndexOf(e.key, System.StringComparison.OrdinalIgnoreCase) >= 0) { lat = e.la; lon = e.lo; doy = e.d; return true; }
        return false;
    }
}

/// <summary>
/// EL SOL POLAR DEL SANTUARIO: mueve la luz direccional de la escena con la hora y la estación del reloj de juego
/// (<see cref="SolarPosition.Sanctuary"/>): sol de medianoche rasante y dorado, noches polares con luna, y días y noches
/// en las transiciones. <see cref="TimeController.IsDay"/> (6–18 h) sigue siendo el horario humano.
/// </summary>
public class SunCycle : MonoBehaviour
{
    Light _light;
    public SunPos Now { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Attach()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "SampleScene") return;
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional && l.GetComponent<SunCycle>() == null) { l.gameObject.AddComponent<SunCycle>(); return; }
    }

    void Awake() { _light = GetComponent<Light>(); }

    void Update()
    {
        TimeController t = TimeController.timeController; if (t == null || _light == null) return;
        Now = SolarPosition.Sanctuary(t);
        float L = SolarPosition.Apply(_light, Now);
        // El ambiente solo si su escena manda (en un microcosmos cargado encima manda la luz de allí); la nieve refleja.
        if (gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene()) RenderSettings.ambientIntensity = 0.3f + 0.7f * L;
    }
}
