using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS NORMAS Y EL CICLO DEL ÁRBOL CAZADOR (Berón 2026-11-05; docs/contencion-arbol-cazador.md, regla S16). Vive en el Santuario Marino, con sus abejas y arañas: «se supone que el árbol
/// se está diseñando para ser autosustentable y require de las abejas y arañas». Las normas vienen de la contención de plantas (BL3-P) y del manejo de animales peligrosos (siempre una
/// barrera, doble puerta, simulacros, código de escape). La barrera final es el agua: de la sala al mar solo se sale por una esclusa inundada, y un enjambre no cruza el agua.
/// </summary>
public static class HunterRule
{
    /// <summary>Días sin ronda que aguantan los filtros.</summary>
    public const int MissedRounds = 3;
    /// <summary>Un fallo de sellos cada tantos días de media, desde <see cref="FirstFailureDay"/>.</summary>
    public const int FailureEvery = 60, FirstFailureDay = 30;
    /// <summary>Quienes tienen que firmar la recaptura: nadie trabaja solo junto a él.</summary>
    public const int RecaptureCrew = 2;
    /// <summary>Días hasta que las compuertas cierran solas (a prueba de fallos: la línea puede estar cerrada y la emergencia no debe durar para siempre).</summary>
    public const int AutoSealDays = 1;
    public const int DrillEvery = 7, SampleEvery = 7;
    /// <summary>Su alcance entero, el del árbol mayor de la antigua arboleda: señuelo y sopor letal (m). Dentro de la celda no sale de ella.</summary>
    public const float LureRadius = 70f, KillRadius = 12f;
    /// <summary>Panales del árbol y arañas de la celda. Son pocos: una celda no da para más.</summary>
    public const int Hives = 3, Spiders = 2;

    public static bool FailureOn(int day)
    {
        if (day < FirstFailureDay) return false;
        unchecked { uint h = (uint)day * 2654435761u; return ((h ^ (h >> 13)) >> 8) % FailureEvery == 0; }
    }
}

/// <summary>
/// EL CICLO CERRADO DE LA CELDA, un día cada vez y sin escena: cadáveres → nutrientes → néctar y miel → polinización → crecimiento, y lo que muere vuelve al suelo.
/// Es un modelo propio, con cifras de juego. Se calcula arriba (como <see cref="CavernRock"/>) y abajo solo se ve.
/// </summary>
public struct HunterLoop
{
    public const int MaxBees = 18, MinBees = 4;
    /// <summary>Lo que se recicla de lo que se gasta. No llega a 1: siempre se pierde algo, y por eso a veces hace falta una ración.</summary>
    public const float Recycle = 0.97f;
    public const float NectarPerBee = 0.02f, HoneyPerNectar = 0.5f, HoneyPerBee = 0.004f, RationKg = 2f, LowNutrients = 1f, SpiderBeesPerDay = 0.15f, GrowthPerDay = 0.002f;

    public float nutrients, honey, bees, pollination, scale;

    public static HunterLoop Start => new HunterLoop { nutrients = 4f, honey = 1f, bees = 12f, pollination = 0.5f, scale = 1f };
    public bool Hungry => nutrients < LowNutrients;
    public bool Colony => bees >= MinBees;

    public void Day(int spiders, float maxScale)
    {
        float nectar = Mathf.Min(nutrients, bees * NectarPerBee);
        nutrients -= nectar * (1f - Recycle);
        honey += nectar * HoneyPerNectar;
        float eaten = bees * HoneyPerBee;
        float fed = honey >= eaten ? 1f : honey / Mathf.Max(0.0001f, eaten);
        honey = Mathf.Max(0f, honey - eaten);
        float born = bees > 0f && fed > 0.9f ? 1f : 0f, lost = spiders * SpiderBeesPerDay + (1f - fed) * bees * 0.2f;
        lost = Mathf.Min(lost, bees + born);
        bees = Mathf.Clamp(bees + born - lost, 0f, MaxBees);
        nutrients += lost * 0.01f * Recycle;   // las abejas muertas vuelven al suelo
        pollination = bees / MaxBees;
        scale = Mathf.Clamp(scale + GrowthPerDay * pollination * (Hungry ? 0f : 1f), 1f, maxScale);
    }
}

/// <summary>
/// LA CELDA DEL ÁRBOL CAZADOR, DESDE ARRIBA. Como <see cref="CavernRock"/>, vive en el santuario 1, junto a la línea: las misiones y la guardia funcionan sin cargar el Santuario Marino.
/// Lo de abajo (<see cref="HunterLab"/>) solo se ve. Sellada, ni su señuelo ni su sopor salen de la celda. Nadie entra: raciones por el portillo y muestras por los guantes.
/// </summary>
public class HunterCell : MonoBehaviour
{
    public enum State { Contenido, Brecha }
    public const string RoundId = "cuar_ronda", RationId = "cuar_racion", SampleId = "cuar_muestra", DrillId = "cuar_simulacro", RecaptureId = "cuar_recaptura";

    public static HunterCell Instance { get; private set; }

    public State Now { get; private set; } = State.Contenido;
    public HunterLoop loop = HunterLoop.Start;
    public int Rounds { get; private set; }
    public int Rations { get; private set; }
    public int Samples { get; private set; }
    public int Drills { get; private set; }
    public int Breaches { get; private set; }
    public int CrewSigned { get; private set; }

    int _missed, _lastDay = int.MinValue, _breachDay;
    const float MaxScale = 1.5f;

    static int Today => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    void OnEnable() => Instance = this;
    void OnDisable() { if (Instance == this) Instance = null; }

    /// <summary>¿Está sujeto este árbol por la celda?</summary>
    public bool Holds(IceCarnivorousTree t) => HunterLab.Instance != null && t == HunterLab.Instance.tree && Now == State.Contenido;

    void Update()
    {
        int d = Today;
        if (d == _lastDay) return;
        bool first = _lastDay == int.MinValue;
        _lastDay = d;
        if (first || ColdSanctuary.TestRunner.Running) return;
        NextDay(d);
    }

    /// <summary>Un día: el ciclo sigue, se vea o no.</summary>
    public void NextDay(int d)
    {
        loop.Day(HunterRule.Spiders, MaxScale);
        if (HunterLab.Instance != null) HunterLab.Instance.Show(loop.scale);
        if (Now == State.Brecha)
        {
            if (d - _breachDay >= HunterRule.AutoSealDays) Seal("las compuertas, solas");
            return;
        }
        _missed++;
        if (_missed > HunterRule.MissedRounds) Breach("sin ronda: filtros saturados");
        else if (HunterRule.FailureOn(d)) Breach("fallo de un sello");
    }

    public void Breach(string why)
    {
        if (Now == State.Brecha) return;
        Now = State.Brecha; Breaches++; CrewSigned = 0; _breachDay = Today;
        Debug.LogWarning($"[Contención] CÓDIGO VERDE: brecha en la celda del árbol cazador ({why}). El enjambre queda en la sala: de allí no sale, la esclusa está inundada.");
    }

    void Seal(string who)
    {
        Now = State.Contenido; _missed = 0; CrewSigned = 0;
        Debug.Log($"[Contención] Celda sellada de nuevo: {who}.");
    }

    public void Round(string who) { Rounds++; _missed = 0; Debug.Log($"[Contención] {who}: lectura remota de sellos, presión, colonia y filtros."); }
    public void Ration(string who) { Rations++; loop.nutrients += HunterLoop.RationKg; Debug.Log($"[Contención] {who}: ración por el portillo."); }
    public void Sample(string who)
    {
        Samples++; loop.scale = Mathf.Max(1f, loop.scale - 0.05f);
        if (HunterLab.Instance != null) HunterLab.Instance.Show(loop.scale);
        Debug.Log($"[Contención] {who}: muestra por los guantes y poda.");
    }
    public void Drill(string who) { Drills++; Debug.Log($"[Contención] {who}: simulacro de escape."); }
    public void Recapture(string who)
    {
        if (Now != State.Brecha) return;
        CrewSigned++;
        if (CrewSigned >= HunterRule.RecaptureCrew) Seal($"equipo de recaptura, {who} el último");
    }

    /// <summary>¿Esta misión baja por la línea? Todas menos la ronda, que es remota.</summary>
    public static bool Rides(string id) => id != null && id.StartsWith("cuar_") && id != RoundId;

    /// <summary>Las misiones: la ronda es remota, desde la sala de guardia; lo demás, bajando por la línea, como <c>nuc_bajada</c>. Durante una brecha, solo la recaptura.</summary>
    public static List<WorkRoster.Mission> Missions(int day)
    {
        var l = new List<WorkRoster.Mission>();
        if (Instance == null) return l;
        l.Add(new WorkRoster.Mission(AreaWork.GuardRoom, RoundId, "Lectura remota de la celda del árbol cazador: sellos, presión, filtros y colonia", 1f, true));
        if (SiteLine.Instance == null || SiteLine.Instance.LineClosed) return l;
        string a = SiteLine.AreaName;
        if (Instance.Now == State.Brecha)
        {
            l.Add(new WorkRoster.Mission(a, RecaptureId, "Código verde: bajar y recontener la celda del árbol cazador, desde fuera", 1f, true));
            l.Add(new WorkRoster.Mission(a, RecaptureId + "_2", "Código verde: segunda persona de la recaptura", 1f, true));
            return l;
        }
        if (Instance.loop.Hungry) l.Add(new WorkRoster.Mission(a, RationId, "Bajar una ración al árbol cazador: el ciclo se ha quedado corto de nutrientes", 1f));
        if (day % HunterRule.SampleEvery == 3) l.Add(new WorkRoster.Mission(a, SampleId, "Bajar a tomar muestras y podar por los guantes de la celda", 1f));
        if (day % HunterRule.DrillEvery == 0) l.Add(new WorkRoster.Mission(a, DrillId, "Simulacro de escape del árbol cazador", 1f));
        return l;
    }
}

/// <summary>EL LABORATORIO DE CONTENCIÓN, abajo: solo se ve. Como <see cref="MarineBase"/>, existe mientras la escena está cargada, y arriba solo se le mira por <see cref="Instance"/>.</summary>
public class HunterLab : MonoBehaviour
{
    public static HunterLab Instance { get; private set; }
    public IceCarnivorousTree tree;
    [Tooltip("La caja de la celda, en coordenadas de este objeto.")] public Bounds cell;

    void OnEnable()
    {
        Instance = this;
        if (HunterCell.Instance != null) Show(HunterCell.Instance.loop.scale);
    }
    void OnDisable() { if (Instance == this) Instance = null; }

    public bool Contains(Vector3 world) => cell.Contains(transform.InverseTransformPoint(world));
    public void Show(float scale) { if (tree != null) tree.transform.localScale = Vector3.one * scale; }
}
