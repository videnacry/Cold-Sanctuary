using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ALMACÉN DE MATERIAL de Construcción (docs/construccion-sostenible.md). El santuario es AUTOSUSTENTABLE: todo lo que
/// se construye sale de PLANTAS (y de los animales sin hacerles daño), cultivado y transformado aquí mismo:
///  · GUADUA (bambú estructural) del BAMBUSAL: se corta la caña madura (3–5 años, se repone sola: el rizoma rebrota), se
///    CURA (avinagrado en pie y secado) y se TRATA con bórax (inmersión) antes de usarla: unos días hasta estar lista.
///  · BAHAREQUE: barro (la tierra del sitio) con paja de trigo/arroz, sobre esterilla de guadua, para paredes.
///  · PANELES DE MICELIO: paja y cañamiza inoculadas con micelio de hongo en moldes; crecen ~5 días en el laboratorio y se
///    secan con calor (aislamiento ligero, sin plástico).
///  · CUERDA DE CÁÑAMO para atar uniones; ACEITE DE LINAZA (del lino) y CERA de las abejas (la que sobra en el panal,
///    sin dañar la colmena) para proteger la madera.
/// Cada día de obra gasta un LOTE: 4 guaduas tratadas, 2 de barro con paja, 2 paneles de micelio y 1 cuerda.
/// </summary>
public class MaterialStore : MonoBehaviour
{
    public static MaterialStore Instance { get; private set; }

    public const string Guadua = "guadua tratada", Barro = "barro con paja", Micelio = "panel de micelio", Cuerda = "cuerda de cáñamo", Acabado = "aceite de linaza y cera";
    public static readonly (string item, int n)[] DayKit = { (Guadua, 4), (Barro, 2), (Micelio, 2), (Cuerda, 1) };

    [Header("Bambusal")]
    public int groveMature = 30;           // cañas maduras (3–5 años) listas para cortar
    public float groveRegrowPerDay = 1.5f; // rebrote del rizoma (corta sostenible ≈ un tercio de las maduras al año)
    [Header("Tiempos (días de juego)")]
    public float cureDays = 3f;            // avinagrado + inmersión en bórax + secado
    public float myceliumDays = 5f;        // crecimiento en el molde y secado

    public readonly Dictionary<string, int> stock = new Dictionary<string, int>();
    readonly List<(string item, int n, float readyDay)> _processing = new List<(string, int, float)>();
    float _regrow;
    WorldLabel _label;

    static float Day => TimeController.timeController != null ? TimeController.timeController.Day + TimeController.timeController.Hour / 24f : Time.time / 3600f;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    void Start()
    {
        // Existencias iniciales: lo que dejó listo la cuadrilla antes de que empiece el juego (unos días de obra).
        Add(Guadua, 12); Add(Barro, 6); Add(Micelio, 6); Add(Cuerda, 3); Add(Acabado, 2);
        _label = GetComponent<WorldLabel>();
        Refresh();
    }

    float _lastDay = -1f;
    void Update()
    {
        float d = Day;
        if (_lastDay < 0f) _lastDay = d;
        _regrow += (d - _lastDay) * groveRegrowPerDay; _lastDay = d;
        if (_regrow >= 1f) { int k = Mathf.FloorToInt(_regrow); groveMature += k; _regrow -= k; }
        for (int i = _processing.Count - 1; i >= 0; i--)
            if (d >= _processing[i].readyDay) { Add(_processing[i].item, _processing[i].n); Debug.Log($"[Material] Listo: {_processing[i].n} × {_processing[i].item}."); _processing.RemoveAt(i); Refresh(); }
    }

    public int Have(string item) => stock.TryGetValue(item, out int v) ? v : 0;
    public void Add(string item, int n) { stock[item] = Have(item) + n; Refresh(); }
    public int Processing(string item) { int n = 0; foreach (var p in _processing) if (p.item == item) n += p.n; return n; }

    public bool HasDayKit() { foreach (var k in DayKit) if (Have(k.item) < k.n) return false; return true; }
    public bool TakeDayKit(string who, string project)
    {
        if (!HasDayKit()) { Debug.Log($"[Material] Falta material para un día de obra en «{project}»: {Summary()}."); return false; }
        foreach (var k in DayKit) stock[k.item] = Have(k.item) - k.n;
        Debug.Log($"[Material] {who} lleva a «{project}» 4 guaduas, 2 de bahareque, 2 paneles de micelio y una cuerda.");
        Refresh();
        return true;
    }

    // ── Las misiones que reponen el almacén ──
    public void HarvestGuadua(string who, int n = 6)
    {
        n = Mathf.Min(n, groveMature);
        if (n <= 0) { Debug.Log("[Material] El bambusal no tiene cañas maduras: hay que esperar a que rebrote."); return; }
        groveMature -= n;
        _processing.Add((Guadua, n, Day + cureDays));
        Debug.Log($"[Material] {who} corta {n} guaduas maduras en el bambusal (sobre el primer nudo, el rizoma rebrota); se curan y tratan con bórax {cureDays:0} días.");
        Refresh();
    }
    public void MakeBahareque(string who, int n = 3) { Add(Barro, n); Debug.Log($"[Material] {who} prepara {n} de barro con paja (tierra del sitio + paja de trigo y arroz)."); }
    public void GrowMycelium(string who, int n = 4) { _processing.Add((Micelio, n, Day + myceliumDays)); Debug.Log($"[Material] {who} siembra micelio en {n} moldes (paja y cañamiza); en {myceliumDays:0} días se secan y son paneles."); Refresh(); }
    public void TwistRope(string who, int n = 2) { Add(Cuerda, n); Debug.Log($"[Material] {who} trenza {n} cuerdas de fibra de cáñamo."); }
    public void MakeFinish(string who) { Add(Acabado, 1); Debug.Log($"[Material] {who} mezcla aceite de linaza con cera de abeja sobrante para proteger la guadua."); }

    /// <summary>¿Qué conviene reponer? (lo usa el catálogo de misiones del día)</summary>
    public List<string> Needs(int openProjects)
    {
        var l = new List<string>();
        int days = Mathf.Max(1, openProjects) * 2;   // reserva: dos días de obra por cada obra abierta
        if (Have(Guadua) + Processing(Guadua) < 4 * days) l.Add("guadua");
        if (Have(Barro) < 2 * days) l.Add("bahareque");
        if (Have(Micelio) + Processing(Micelio) < 2 * days) l.Add("micelio");
        if (Have(Cuerda) < days) l.Add("cuerda");
        return l;
    }

    public string Summary()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var kv in stock) sb.Append($"{kv.Key} {kv.Value} · ");
        sb.Append($"bambusal {groveMature} cañas maduras");
        if (_processing.Count > 0) sb.Append($" · en proceso {_processing.Count} tanda(s)");
        return sb.ToString();
    }

    void Refresh() { if (_label != null) _label.text = "Almacén de material\n" + Summary(); }
}
