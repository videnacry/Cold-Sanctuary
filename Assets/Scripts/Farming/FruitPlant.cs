using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UNA FRUTAL EN SU MACETA del techo de una torre (docs/jardines-frutales.md). Lleva su ESTADO de cuidado, que se ve en
/// tres barras sobre el borde de la maceta (azul: agua · ámbar: abono · verde: salud) y en su etiqueta:
/// - el AGUA la repone el GOTERO (<see cref="drip"/>, al 85 %; Berón 2026-10-31) y el ABONO baja cada día de juego según su ficha (<see cref="FruitCare.Ficha.aguaDia"/>, <c>abonoDia</c>);
/// - la PLAGA sube poco a poco (la revisión diaria la descubre y la trata);
/// - la SALUD sube si tiene agua, abono y pocas plagas, y baja con sed, hambre o plaga.
/// El jardinero de la ronda (<see cref="FruitGardener"/>) la revisa con <see cref="Care"/>: riega o fertirriega hasta
/// llenar (con goteo, solo si el gotero se atascó), repone el abono si hace falta y limpia la plaga; con el tiempo que sobra,
/// poliniza a pincel (<see cref="Pollinate"/>: sube el cuajado) y poda y anota su crecimiento (<see cref="Prune"/>). <see cref="Wellbeing"/> resume las cuatro cosas (0–1).
/// La maceta (raíz con BoxCollider, sin renderer) es un MUEBLE para <see cref="FurnitureSpacing"/>: guarda sus 2 m.
/// </summary>
public class FruitPlant : MonoBehaviour
{
    public GardenCrop.Kind kind;
    [Tooltip("Torre en cuyo techo está.")]
    public string building;
    [Tooltip("Orden en que se plantó en su techo (la ronda la sigue).")]
    public int order;
    [Range(0f, 1f)] public float water = 0.55f, nutrients = 0.5f, pests = 0.2f, health = 0.7f;
    [Tooltip("Día de juego de la última revisión (−1 = nunca).")]
    public int lastCareDay = -1;
    public string lastCaredBy;
    public int timesCared;
    [Tooltip("RIEGO POR GOTEO (Berón 2026-10-31): el agua le llega sola desde el cabezal de su techo; la ronda ya no riega, revisa y abona.")]
    public bool drip;
    [Tooltip("Cuajado de la flor (0–1): sube al polinizar a mano; baja si nadie poliniza las que lo necesitan.")]
    [Range(0f, 1f)] public float fruitSet = 0.5f;
    public int lastPollinatedDay = -99, lastPrunedDay = -99;
    public int timesPollinated, timesPruned;

    public static readonly List<FruitPlant> All = new List<FruitPlant>();
    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public FruitCare.Ficha Ficha => FruitCare.For(kind);
    public string Name => Ficha != null ? Ficha.nombre : kind.ToString();
    /// <summary>Bienestar (0–1): salud, agua, abono y sin plaga.</summary>
    public float Wellbeing => Mathf.Clamp01(0.4f * health + 0.25f * water + 0.2f * nutrients + 0.15f * (1f - pests));
    public bool Thirsty => water < 0.3f;

    /// <summary>Medio lado de la maceta (m), en su marco.</summary>
    public Vector3 PotSize => Ficha != null ? Ficha.maceta : new Vector3(1f, 0.6f, 1f);

    /// <summary>Dónde se pone de pie quien la cuida: junto a la maceta, del lado de <paramref name="from"/>.</summary>
    public Vector3 StandPoint(Vector3 from)
    {
        Vector3 d = from - transform.position; d.y = 0f;
        if (d.sqrMagnitude < 0.01f) d = -transform.forward;
        return transform.position + d.normalized * (Mathf.Max(PotSize.x, PotSize.z) * 0.5f + 0.7f) + Vector3.up;
    }

    static float SecondsPerDay => TimeController.timeController != null ? Mathf.Max(1f, TimeController.timeController.SecondsPerGameDay) : 3600f;
    static int Today => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    Transform _bars, _w, _n, _h;
    WorldLabel _label;
    float _nextUi;

    void Start()
    {
        BuildBars();
        _label = GetComponent<WorldLabel>();
        if (_label == null) { _label = gameObject.AddComponent<WorldLabel>(); _label.height = 1.6f; }
        Refresh();
    }

    void Update()
    {
        Tick(Time.deltaTime / SecondsPerDay);
        if (Time.time >= _nextUi) { _nextUi = Time.time + 1f; Refresh(); }
    }

    /// <summary>Avanza <paramref name="days"/> días de juego: gasta agua y abono, sube la plaga y la salud sigue al estado.</summary>
    public void Tick(float days)
    {
        if (days <= 0f) return;
        FruitCare.Ficha f = Ficha;
        float aw = f != null ? f.aguaDia : 0.35f, an = f != null ? f.abonoDia : 0.1f;
        water = Mathf.Clamp01(water - aw * days);
        if (drip) water = Mathf.MoveTowards(water, DripLevel, (aw + 2f) * days);   // el gotero repone lo que gasta (y algo más)
        if (FruitCare.Polinizar(kind)) fruitSet = Mathf.Clamp01(fruitSet - 0.08f * days);
        nutrients = Mathf.Clamp01(nutrients - an * days);
        pests = Mathf.Clamp01(pests + 0.12f * days);
        bool stressed = water < 0.25f || nutrients < 0.2f || pests > 0.6f;
        float target = stressed ? 0.2f : Mathf.Clamp01(0.55f + 0.45f * Mathf.Min(water, nutrients * 1.5f, 1f - pests));
        health = Mathf.MoveTowards(health, target, (stressed ? 0.35f : 0.25f) * days);
    }

    /// <summary>LA REVISIÓN DIARIA: riega (o fertirriega) hasta llenar, repone el abono si está por debajo de la mitad,
    /// trata la plaga y la planta mejora al momento (raíces regadas, sin plaga). Devuelve el bienestar antes y después.</summary>
    public (float before, float after) Care(string who)
    {
        float before = Wellbeing;
        if (!drip || water < 0.6f) water = drip ? DripLevel : 1f;   // con goteo, solo si el gotero se atascó (lo desatasca)
        if (nutrients < 0.6f) nutrients = Mathf.Min(1f, nutrients + 0.5f);
        pests = Mathf.Max(0f, pests - 0.8f);
        health = Mathf.Min(1f, health + 0.1f);
        lastCareDay = Today; lastCaredBy = who; timesCared++;
        Refresh();
        return (before, Wellbeing);
    }

    /// <summary>Hasta dónde mantiene el gotero la humedad de la maceta (sin encharcar: 85 %).</summary>
    public const float DripLevel = 0.85f;

    /// <summary>¿Le toca hoy polinizar a pincel? (las que lo necesitan, cada 2 días)</summary>
    public bool NeedsPollination(int day) => FruitCare.Polinizar(kind) && day - lastPollinatedDay >= 2;
    /// <summary>¿Le toca hoy la poda (y anotar su crecimiento)?</summary>
    public bool NeedsPruning(int day) { int d = FruitCare.PodaDias(kind); return d > 0 && day - lastPrunedDay >= d; }

    /// <summary>Polinizar a pincel (o sacudir, o vibrar): el cuajado sube.</summary>
    public void Pollinate(string who) { fruitSet = Mathf.Min(1f, fruitSet + 0.35f); lastPollinatedDay = Today; timesPollinated++; lastCaredBy = who; Refresh(); }
    /// <summary>Podar (chupones, ramas cruzadas, estolones) y anotar el crecimiento en el cuaderno: la salud sube un poco.</summary>
    public void Prune(string who) { health = Mathf.Min(1f, health + 0.05f); pests = Mathf.Max(0f, pests - 0.1f); lastPrunedDay = Today; timesPruned++; lastCaredBy = who; Refresh(); }

    // Tres barras sobre un poste en una esquina de la maceta (dentro de su huella: no cambian la caja del mueble).
    void BuildBars()
    {
        if (_bars != null) return;
        Vector3 pot = PotSize;
        _bars = new GameObject("Ficha_estado").transform;
        _bars.SetParent(transform, false);
        _bars.localPosition = new Vector3(pot.x * 0.5f - 0.08f, pot.y, -pot.z * 0.5f + 0.08f);
        Bar("Poste", new Vector3(0f, 0.3f, 0f), new Vector3(0.04f, 0.6f, 0.04f), new Color(0.45f, 0.36f, 0.28f));
        _w = Bar("Agua", new Vector3(0f, 0.66f, 0f), new Vector3(0.06f, 0.06f, 0.06f), new Color(0.3f, 0.6f, 0.95f));
        _n = Bar("Abono", new Vector3(0f, 0.74f, 0f), new Vector3(0.06f, 0.06f, 0.06f), new Color(0.9f, 0.65f, 0.25f));
        _h = Bar("Salud", new Vector3(0f, 0.82f, 0f), new Vector3(0.06f, 0.06f, 0.06f), new Color(0.35f, 0.85f, 0.4f));
    }

    Transform Bar(string n, Vector3 p, Vector3 s, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(g.GetComponent<Collider>());
        g.name = n; g.transform.SetParent(_bars, false); g.transform.localPosition = p; g.transform.localScale = s;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
        return g.transform;
    }

    // Las barras crecen hacia DENTRO de la maceta (hacia −X local), hasta 0,5 m llenas.
    void SetBar(Transform b, float v)
    {
        if (b == null) return;
        float len = Mathf.Max(0.02f, 0.5f * Mathf.Clamp01(v));
        b.localScale = new Vector3(len, 0.06f, 0.06f);
        b.localPosition = new Vector3(-len * 0.5f, b.localPosition.y, 0f);
    }

    void Refresh()
    {
        SetBar(_w, water); SetBar(_n, nutrients); SetBar(_h, health);
        if (_label != null) _label.text = $"{Name} · agua {water * 100f:0}% · abono {nutrients * 100f:0}% · salud {health * 100f:0}%{(drip ? " · goteo" : "")}{(FruitCare.Polinizar(kind) ? $" · cuajado {fruitSet * 100f:0}%" : "")}{(pests > 0.5f ? " · ¡plaga!" : "")}";
    }

    /// <summary>Las frutales de un techo (por su torre), en el orden de plantación.</summary>
    public static List<FruitPlant> OnRoof(string building)
    {
        var l = new List<FruitPlant>();
        foreach (FruitPlant p in All) if (p != null && p.building == building) l.Add(p);
        l.Sort((a, b) => a.order.CompareTo(b.order));
        return l;
    }
}
