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
    /// <summary>Las arañas de la celda. Los panales los manda el ciclo: <see cref="HunterLoop.Nests"/>.</summary>
    public const int Spiders = 2;

    public static bool FailureOn(int day)
    {
        if (day < FirstFailureDay) return false;
        unchecked { uint h = (uint)day * 2654435761u; return ((h ^ (h >> 13)) >> 8) % FailureEvery == 0; }
    }
}

/// <summary>
/// EL CICLO DE LA CELDA, un día cada vez y sin escena (docs/ice-sanctuary-ecology.md §2; docs/contencion-arbol-cazador.md §El balance). El árbol vive de agua, frío y luz. Sus nutrientes
/// salen sobre todo de los **cadáveres de sus propios insectos** (Berón): las abejas hacen los nidos con ellos y el árbol absorbe de los nidos. Más nutrientes → más ramas → más nidos.
// Se cuenta en kg de nitrógeno. Los insectos no crean nada: lo hacen circular y pierden un poco en cada vuelta. Lo nuevo entra por su SIMBIONTE, el microbio psicrófilo del diseño (Berón: «agrega
/// dicho microbio»), que fija el N₂ del aire de la sala en sus raíces, como el Frankia de los alisos.
/// </summary>
public struct HunterLoop
{
    /// <summary>~20 cm de abeja: 237 g escalando una abeja de 0,1 g y 1,5 cm. Physiognomy dice 1 g, que no cuadra con 20 cm. La araña, 175 g (Physiognomy). Días de vida: la abeja, los de una
    /// obrera de verano; la araña, 3 años. N: 2,5 % del peso fresco.</summary>
    public const float BeeKg = 0.237f, SpiderKg = 0.175f, BeeLifeDays = 40f, SpiderLifeDays = 1095f, NShare = 0.025f;
    /// <summary>Lo que vuelve de cada muerte y de cada hoja. No llega a 1.</summary>
    public const float Recycle = 0.97f;
    /// <summary>Nitrógeno del árbol de 13 m (~300 kg secos), su renovación al año y lo que absorbe de los nidos al día. Estimaciones.</summary>
    public const float TreeN0 = 0.6f, TurnoverPerYear = 1f / 6f, AbsorbPerDay = 0.1f;
    /// <summary>Lo que fija el simbionte: 11,5 kg de N por hectárea y año, lo medido en alisos de Alaska, la cifra fría que encontré (de 0,1 a más de 100 según el sitio), sobre los 81 m² de su bandeja.
    /// Crece con la raíz (<see cref="Scale"/>²) y se frena con nitrógeno de sobra en los nidos, como el abono frena los nódulos.</summary>
    public const float FixKgPerHaYear = 11.5f, RootM2 = 81f, FixStopsAtNestN = 0.3f;
    public const int BeesPerNest = 12, MinNests = 3, MaxNests = 6;
    public const float SpiderBeesPerDay = 0.15f, MaxScale = 1.5f, RationKg = 2f;

    public float treeN, nestN, bees, honey, pollination;

    public static HunterLoop Start => new HunterLoop { treeN = TreeN0, nestN = 0.3f, bees = 18f, honey = 1f, pollination = 0.5f };
    static float BeeN => BeeKg * NShare;
    public float Scale => Mathf.Pow(treeN / TreeN0, 1f / 3f);
    /// <summary>Más ramas, más nidos.</summary>
    public int Nests => Mathf.Clamp(Mathf.FloorToInt(MinNests * Scale * Scale), MinNests, MaxNests);
    public float TotalN => treeN + nestN + bees * BeeN;
    /// <summary>El ciclo ha perdido demasiado: hace falta una entrega por el portillo.</summary>
    public bool Hungry => TotalN < 0.9f * (TreeN0 + 0.3f + 18f * BeeN);
    public bool Colony => bees >= MinNests;

    /// <summary>Un cadáver que llega de fuera: una presa de su miel o una entrega por el portillo.</summary>
    public void Carcass(float kg) { nestN += kg * NShare * Recycle; }

    public void Day(int spiders, bool symbiont = true)
    {
        float cap = BeesPerNest * Nests;
        pollination = Mathf.Min(1f, bees / cap);
        float dead = Mathf.Min(bees, bees / BeeLifeDays + spiders * SpiderBeesPerDay);
        nestN += dead * BeeN * Recycle + spiders / SpiderLifeDays * SpiderKg * NShare * Recycle;
        float litter = treeN * TurnoverPerYear / 365f;
        treeN -= litter; nestN += litter * Recycle;
        // Sin abejas no hay nidos que absorber. El tope: lo que cabe en la celda.
        float top = TreeN0 * MaxScale * MaxScale * MaxScale;
        if (symbiont)
        {
            float s = Scale, fix = FixKgPerHaYear / 10000f * RootM2 / 365f * s * s * Mathf.Clamp01(1f - nestN / FixStopsAtNestN);
            treeN += Mathf.Clamp(fix, 0f, Mathf.Max(0f, top - treeN));
        }
        float up = Mathf.Min(nestN * AbsorbPerDay * pollination, Mathf.Max(0f, top - treeN));
        nestN -= up; treeN += up;
        bees -= dead;
        float born = bees <= 0f ? 0f : Mathf.Min(Mathf.Max(0f, cap - bees), cap / BeeLifeDays * 1.2f);
        born = Mathf.Min(born, nestN / BeeN);
        nestN -= born * BeeN; bees += born;
        honey = Mathf.Max(0f, honey + Nests * 0.05f * pollination - bees * 0.004f);
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
        loop.Day(HunterRule.Spiders);
        Show();
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
    void Show() { if (HunterLab.Instance != null) HunterLab.Instance.Show(loop.Scale, loop.Nests); }
    public void Ration(string who) { Rations++; loop.Carcass(HunterLoop.RationKg); Debug.Log($"[Contención] {who}: restos de un animal muerto, por el portillo."); }
    /// <summary>Una presa de su miel letal: un extra.</summary>
    public void Prey(float kg) { loop.Carcass(kg); Debug.Log($"[Contención] El árbol absorbe una presa de {kg:0.#} kg."); }
    public void Sample(string who)
    {
        Samples++; float cutN = loop.treeN * 0.03f; loop.treeN -= cutN; loop.nestN += cutN * 0.9f;   // casi todo lo podado vuelve
        Show();
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
        if (Instance.loop.Hungry) l.Add(new WorkRoster.Mission(a, RationId, "Bajar restos de un animal muerto al árbol cazador: el ciclo de sus insectos ha perdido demasiado", 1f));
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
    public List<GameObject> hives = new List<GameObject>();
    [Tooltip("La caja de la celda, en coordenadas de este objeto.")] public Bounds cell;

    void OnEnable()
    {
        Instance = this;
        if (HunterCell.Instance != null) Show(HunterCell.Instance.loop.Scale, HunterCell.Instance.loop.Nests);
    }
    void OnDisable() { if (Instance == this) Instance = null; }

    public bool Contains(Vector3 world) => cell.Contains(transform.InverseTransformPoint(world));
    /// <summary>Más nutrientes, más ramas y más nidos.</summary>
    public void Show(float scale, int nests)
    {
        if (tree == null) return;
        tree.transform.localScale = Vector3.one * scale;
        for (int i = 0; i < hives.Count; i++) if (hives[i] != null) hives[i].SetActive(i < nests);
    }
}
