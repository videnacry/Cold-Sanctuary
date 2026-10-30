using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UNA FRUTAL EN SU MACETA del techo de una torre (docs/jardines-frutales.md). Lleva su ESTADO de cuidado, que se ve en
/// tres barras sobre el borde de la maceta (azul: agua · ámbar: abono · verde: salud) y en su etiqueta:
/// - el AGUA y el ABONO bajan cada día de juego según su ficha (<see cref="FruitCare.Ficha.aguaDia"/>, <c>abonoDia</c>);
/// - la PLAGA sube poco a poco (la revisión diaria la descubre y la trata);
/// - la SALUD sube si tiene agua, abono y pocas plagas, y baja con sed, hambre o plaga.
/// El jardinero de la ronda (<see cref="FruitGardener"/>) la revisa con <see cref="Care"/>: riega o fertirriega hasta
/// llenar, repone el abono si hace falta y limpia la plaga. <see cref="Wellbeing"/> resume las cuatro cosas (0–1).
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
        water = 1f;
        if (nutrients < 0.6f) nutrients = Mathf.Min(1f, nutrients + 0.5f);
        pests = Mathf.Max(0f, pests - 0.8f);
        health = Mathf.Min(1f, health + 0.1f);
        lastCareDay = Today; lastCaredBy = who; timesCared++;
        Refresh();
        return (before, Wellbeing);
    }

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
        if (_label != null) _label.text = $"{Name} · agua {water * 100f:0}% · abono {nutrients * 100f:0}% · salud {health * 100f:0}%{(pests > 0.5f ? " · ¡plaga!" : "")}";
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
