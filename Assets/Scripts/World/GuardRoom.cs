using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA SALA DE GUARDIA (Berón 2026-11-05; docs/sala-de-guardia.md): el nivel 4 de la torre de Servicios, junto a la Sala de asambleas. Aquí se observa todo el santuario.
/// <see cref="GuardWatch"/> lee el estado real con accesos estáticos (como <see cref="MarineBase"/>.Instance): si un sistema no está en la escena, su pantalla lo dice y no falla.
/// LO SIMULADO: el santuario no tiene terremotos ni maremotos; <see cref="GuardWatchRule"/> da un parte diario determinista, sin efecto en el mundo, y la pantalla lo dice.
/// Tampoco hay simulación de fuego: esa pantalla lee la emergencia nuclear.
/// </summary>
public static class GuardWatchRule
{
    /// <summary>Un sismo de M ≥ 7 solo ocurre cada tantos días de media.</summary>
    public const int BigQuakeEvery = 60;
    public const float TsunamiWatchMagnitude = 7f;

    static uint Hash(int day) { unchecked { uint h = (uint)day * 2654435761u; return h ^ (h >> 13); } }

    /// <summary>El mayor sismo regional del día: casi siempre leve; cada <see cref="BigQuakeEvery"/> días de media, fuerte.</summary>
    public static float Magnitude(int day)
    {
        uint h = Hash(day);
        return h % BigQuakeEvery == 0 ? 7f + (h >> 8) % 10u * 0.1f : 2f + (h >> 8) % 35u * 0.1f;
    }

    public enum Sea { Normal, Vigilancia, Aviso }

    /// <summary>Como en los centros reales: el sismo abre la vigilancia, y solo el mareógrafo y la boya confirman el aviso.</summary>
    public static Sea SeaState(int day)
    {
        if (Magnitude(day) < TsunamiWatchMagnitude) return Sea.Normal;
        return (Hash(day) >> 20) % 3u == 0u ? Sea.Aviso : Sea.Vigilancia;
    }
}

public static class GuardWatch
{
    public enum Feed { Icebergs, Radar, Roca, Sismografo, Mar, Depredadores, Emergencias }
    public static readonly Feed[] Feeds = (Feed[])System.Enum.GetValues(typeof(Feed));

    /// <summary>Partes firmados desde que arrancó la escena.</summary>
    public static int Shifts { get; private set; }

    static int Day => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    public static string Title(Feed f)
    {
        switch (f)
        {
            case Feed.Icebergs: return "Icebergs";
            case Feed.Radar: return "Radar";
            case Feed.Roca: return "Roca de la caverna";
            case Feed.Sismografo: return "Sismógrafo";
            case Feed.Mar: return "Mareógrafo y boya";
            case Feed.Depredadores: return "Depredadores";
            default: return "Emergencias";
        }
    }

    /// <summary>¿Esa pantalla está en alerta? (se pone roja).</summary>
    public static bool Alert(Feed f)
    {
        switch (f)
        {
            case Feed.Icebergs: case Feed.Radar:
                foreach (IcebergRule.Site s in IcebergRule.Sites) if (Iceberg.Threatening(s) != null) return true;
                return false;
            case Feed.Roca: return CavernRock.Instance != null && CavernRock.Instance.Now != CavernRock.Level.Normal;
            case Feed.Sismografo: return GuardWatchRule.Magnitude(Day) >= GuardWatchRule.TsunamiWatchMagnitude;
            case Feed.Mar: return GuardWatchRule.SeaState(Day) != GuardWatchRule.Sea.Normal;
            case Feed.Emergencias: return NuclearSite.Evacuating;
            default: return false;
        }
    }

    public static string Read(Feed f)
    {
        switch (f)
        {
            case Feed.Icebergs:
            {
                int threats = 0;
                foreach (IcebergRule.Site s in IcebergRule.Sites) if (Iceberg.Threatening(s) != null) threats++;
                return $"{Iceberg.All.Count} seguidos · {threats} sitios amenazados";
            }
            case Feed.Radar:
            {
                float best = float.MaxValue;
                foreach (Iceberg b in Iceberg.All)
                    if (b != null) foreach (IcebergRule.Site s in IcebergRule.Sites) best = Mathf.Min(best, b.DistanceTo(s));
                return best < float.MaxValue ? $"{Iceberg.All.Count} ecos · el más cercano a un sitio, a {best:0} m" : "Sin ecos";
            }
            case Feed.Roca:
            {
                CavernRock r = CavernRock.Instance;
                return r == null ? "Sin datos: la caverna no está en esta escena" : $"{r.Now} · {r.RateMmDay:0.000} mm/día · {r.InflowLpm:0} l/min · {r.MicroEvents} microsismos";
            }
            case Feed.Sismografo: return $"M {GuardWatchRule.Magnitude(Day):0.0} hoy (simulado)";
            case Feed.Mar: return $"{GuardWatchRule.SeaState(Day)} (simulado)";
            case Feed.Depredadores:
            {
                int total = PredatorRound.Today().Count;
                return total == 0 ? "Sin depredadores en la lista" : $"{PredatorRound.Left} de {total} por revisar";
            }
            default: return NuclearSite.Evacuating ? "EVACUACIÓN DEL SITIO NUCLEAR" : "Sin emergencias · no hay sensor de fuego";
        }
    }

    /// <summary>El efecto de la misión: firmar el parte del turno.</summary>
    public static void Shift(string who)
    {
        Shifts++;
        var parts = new List<string>();
        foreach (Feed f in Feeds) parts.Add($"{Title(f)}: {Read(f)}{(Alert(f) ? " (ALERTA)" : "")}");
        Debug.Log($"[Guardia] Parte de {who}: {string.Join(" · ", parts)}.");
    }
}

/// <summary>El piso de la sala: lo da a conocer a la jornada, como la estación de la línea de las cavernas.</summary>
public class GuardRoom : MonoBehaviour
{
    public const string FloorName = "Guardia_AUTO";
    void OnEnable() => WorkdayDirector.RegisterPlace(AreaWork.GuardRoom, transform);
}

/// <summary>Una pantalla del muro: su texto se renueva cada <see cref="Every"/> s y se pone roja en alerta.</summary>
public class GuardScreen : MonoBehaviour
{
    const float Every = 2f;
    public GuardWatch.Feed feed;
    public Renderer panel;
    public Material normal, alert;
    WorldLabel _label;
    float _next;

    void Start() => _label = GetComponent<WorldLabel>();

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + Every;
        if (_label != null) _label.text = $"{GuardWatch.Title(feed)}\n{GuardWatch.Read(feed)}";
        if (panel != null && normal != null && alert != null) panel.sharedMaterial = GuardWatch.Alert(feed) ? alert : normal;
    }
}

/// <summary>Lo que gira en la sala: el barrido del radar y el tambor del sismógrafo.</summary>
public class GuardSpin : MonoBehaviour
{
    public Vector3 degreesPerSecond = new Vector3(0f, 90f, 0f);
    void Update() => transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
}
