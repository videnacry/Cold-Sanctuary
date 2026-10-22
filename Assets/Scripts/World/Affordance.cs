using System.Collections.Generic;
using UnityEngine;

/// <summary>Qué ofrece un elemento del entorno a un ser vivo (docs/natural-interactions.md).</summary>
public enum AffordanceKind
{
    // Comida vegetal (se gasta y rebrota)
    Grass, PlantSap, Seed, Leaf, Fungus, Flower, Berry, Willow, Lichen, Dung,
    // Lugares (se ocupan, no se gastan)
    Perch, Shelter, SandPit, BreathingHole, HaulOut, Den, RubbingGravel,
    // Materiales que se acarrean (se gastan)
    Pebble,
    // Producto de otro ser (la melaza de un pulgón)
    Honeydew,
}

/// <summary>
/// INTERACTUABLE DEL ENTORNO: algo del mundo que un ser USA según la etología real de su especie (el <see cref="Ethogram"/>).
/// Sustituye al decorado: cada brizna, helecho, hongo, piedra, respiradero o montón de guijarros del juego dice qué ofrece
/// y a cuántos a la vez. Los consumibles (pasto, savia, semillas, guijarros…) llevan un <see cref="Stock"/> que se gasta y
/// REBROTA solo (cálculo perezoso, sin Update); los lugares (posadero, refugio, respiradero…) se OCUPAN.
/// Un mismo objeto puede llevar varios (una brizna: pasto para el saltamontes, savia para el pulgón, posadero para la mantis).
/// </summary>
public class Affordance : MonoBehaviour
{
    public AffordanceKind kind;
    [Tooltip("Cuántos seres lo usan a la vez.")]
    [Min(1)] public int capacity = 1;
    [Tooltip("Existencias (consumibles). Los lugares no la usan.")]
    public float stock = 1f, maxStock = 1f;
    [Tooltip("Rebrote por segundo de juego real (0 = no rebrota).")]
    public float regrowPerSecond = 0.002f;
    [Tooltip("Último ser que lo usó y cuándo (p. ej. la foca que respira por su respiradero: el oso que acecha lo ve).")]
    public Anima lastUser;
    public float lastUseTime = -999f;

    readonly List<Anima> _users = new List<Anima>();
    float _t;
    AffordanceKind _reg;   // la lista en la que está dado de alta (si cambia «kind» tras añadirlo, se re-registra)

    static readonly Dictionary<AffordanceKind, List<Affordance>> _byKind = new Dictionary<AffordanceKind, List<Affordance>>();

    public static List<Affordance> Of(AffordanceKind k)
    {
        if (!_byKind.TryGetValue(k, out List<Affordance> l)) _byKind[k] = l = new List<Affordance>();
        return l;
    }

    void OnEnable() { _reg = kind; Of(kind).Add(this); _t = Time.time; }
    void OnDisable() { Of(_reg).Remove(this); _users.Clear(); }

    public static bool IsConsumable(AffordanceKind k) => k <= AffordanceKind.Dung || k == AffordanceKind.Pebble || k == AffordanceKind.Honeydew;

    /// <summary>Existencias actuales, con el rebrote acumulado desde la última consulta.</summary>
    public float Stock
    {
        get
        {
            float now = Time.time;
            if (regrowPerSecond > 0f) stock = Mathf.Min(maxStock, stock + (now - _t) * regrowPerSecond * maxStock);
            _t = now;
            return stock;
        }
    }

    public bool Free(Anima who) { _users.RemoveAll(u => u == null || u.death); return _users.Contains(who) || _users.Count < capacity; }
    public bool Available(Anima who) => Free(who) && (!IsConsumable(kind) || Stock > 0.05f);

    public bool Occupy(Anima who) { if (!Free(who)) return false; if (!_users.Contains(who)) _users.Add(who); return true; }
    public void Release(Anima who) => _users.Remove(who);

    /// <summary>Gasta <paramref name="amount"/> (o lo que quede) y apunta quién lo usó. Devuelve lo obtenido.</summary>
    public float Take(float amount, Anima who = null)
    {
        Touch(who);
        if (!IsConsumable(kind)) return amount;
        float got = Mathf.Min(Stock, amount);
        stock -= got;
        return got;
    }

    public void Add(float amount) { stock = Mathf.Min(maxStock, Stock + amount); }

    public void Touch(Anima who) { lastUser = who; lastUseTime = Time.time; }

    /// <summary>El más cercano de su clase que <paramref name="who"/> puede usar ahora, dentro de <paramref name="radius"/>.</summary>
    public static Affordance Nearest(AffordanceKind k, Vector3 pos, float radius, Anima who = null)
    {
        Affordance best = null; float bd = radius * radius;
        foreach (Affordance a in Of(k))
        {
            if (a == null || !a.isActiveAndEnabled || !a.Available(who)) continue;
            float d = (a.transform.position - pos).sqrMagnitude;
            if (d < bd) { bd = d; best = a; }
        }
        return best;
    }

    /// <summary>Atajo para los constructores: añade un interactuable a <paramref name="g"/>.</summary>
    public static Affordance Put(GameObject g, AffordanceKind k, int capacity = 1, float stock = 1f, float regrowPerSecond = 0.002f)
    {
        var a = g.AddComponent<Affordance>();
        a.kind = k; a.capacity = capacity; a.stock = a.maxStock = stock; a.regrowPerSecond = regrowPerSecond;
        if (Application.isPlaying && a.isActiveAndEnabled && a._reg != k) { Of(a._reg).Remove(a); a._reg = k; Of(k).Add(a); }   // en juego, OnEnable ya lo dio de alta con el kind por defecto
        return a;
    }
}
