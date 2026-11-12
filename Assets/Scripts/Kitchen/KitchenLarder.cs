using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DESPENSA Y OBRADOR del restaurante (docs/cocina-profesional.md). Lleva el stock de materias primas y de ELABORACIONES
/// (harina, masa madre, quesos, yogur, tofu, caldos…) y planifica la producción como una brigada real (mise en place):
/// para servir un platillo primero tiene que haber sus elaboraciones; si falta una, se pone en marcha… y si esa a su vez
/// pide otra (el queso pide leche; la leche, proteína de fermentación), se encadena. Las elaboraciones con REPOSO
/// (fermentar, madurar, cuajar) tardan su tiempo en HORAS DE JUEGO (TimeController): el yogur incuba 6 h, la masa de pizza
/// madura 48 h, el parmesano 12 meses (la bodega arranca con piezas ya curadas, como cualquier restaurante).
/// Las materias primas llegan solas: los cultivos que hay plantados, la carne cultivada que cosecha el laboratorio
/// (`CulturedMeatLab`) y los insumos de fermentación y básicos.
/// </summary>
public class KitchenLarder : MonoBehaviour
{
    public static KitchenLarder Instance { get; private set; }

    [Tooltip("Cada cuántas horas de juego llega género de los huertos y del obrador de fermentación.")]
    public float restockHours = 6f;
    [Tooltip("Raciones de cada platillo que el obrador intenta tener preparadas (mise en place).")]
    public int prepAhead = 2;
    [Tooltip("Tandas de elaboración en marcha a la vez (la brigada no es infinita).")]
    public int maxConcurrent = 12;

    public readonly Dictionary<string, float> stock = new Dictionary<string, float>();
    public readonly List<Batch> running = new List<Batch>();
    public int prepsDone { get; private set; }
    public int dishesServed { get; private set; }

    public class Batch { public Gastronomy.Prep prep; public float hoursLeft; }

    float _restock;
    HashSet<string> _cropsPlanted;
    int _cropsSeen = -1;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    void Start()
    {
        // Bodega inicial: elaboraciones de largo curado que un restaurante siempre tiene en reserva.
        Add("parmesano (madurado)", 12); Add("salsa de soja (shoyu)", 20); Add("miso", 12); Add("vinagre de arroz", 15);
        Add("masa madre", 6);
        Restock();
    }

    static float GameHours(float realDelta) =>
        TimeController.timeController.TimeSpeedMinuteSecs > 0.0001f ? realDelta / TimeController.timeController.TimeSpeedMinuteSecs / 60f : 0f;

    void Update() => Tick(GameHours(Time.deltaTime));

    /// <summary>Avanza el obrador <paramref name="hours"/> horas de juego (los tests lo llaman directamente).</summary>
    public void Tick(float hours)
    {
        _restock += hours;
        if (_restock >= restockHours) { _restock = 0f; Restock(); }

        for (int i = running.Count - 1; i >= 0; i--)
        {
            Batch b = running[i];
            b.hoursLeft -= hours;
            if (b.hoursLeft > 0f) continue;
            running.RemoveAt(i);
            Add(b.prep.name, b.prep.portions);
            prepsDone++;
            Debug.Log($"[Cocina] Elaboración lista: {b.prep.name} ×{b.prep.portions} ({b.prep.equipment}{(b.prep.restHours > 0 ? $", {Hours(b.prep.restHours)} de reposo" : "")}).");
        }
        if (autoPlan) Plan();
    }

    [Tooltip("Si está activo, el obrador arranca solo sus tandas. En el santuario lo apaga la jornada: cada elaboración es una MISIÓN de alguien (WorkdayDirector).")]
    public bool autoPlan = true;
    List<Gastronomy.Prep> _collect;

    // Mise en place: por cada platillo de la carta, asegura sus elaboraciones (y las de éstas).
    void Plan()
    {
        foreach (Gastronomy.Dish d in Gastronomy.Menu)
            foreach (string c in d.components) Ensure(c, prepAhead, 0);
    }

    /// <summary>Las elaboraciones que HACEN FALTA y se pueden empezar ya (hay sus ingredientes): de aquí salen las misiones
    /// de cocina del día. No arranca nada.</summary>
    public List<Gastronomy.Prep> Needed(int max)
    {
        _collect = new List<Gastronomy.Prep>();
        foreach (Gastronomy.Dish d in Gastronomy.Menu)
            foreach (string c in d.components) Ensure(c, prepAhead, 0);
        var l = _collect; _collect = null;
        return l.Count > max ? l.GetRange(0, max) : l;
    }

    /// <summary>Alguien hace la elaboración (misión de cocina): gasta sus ingredientes y empieza su tanda (con su reposo).</summary>
    public bool StartPrep(string name, string who)
    {
        Gastronomy.Prep p = Gastronomy.PrepOf(name);
        if (p == null) return false;
        foreach (string inp in p.inputs) if (Have(inp) < 1f) return false;
        foreach (string inp in p.inputs) Take(inp, 1f);
        running.Add(new Batch { prep = p, hoursLeft = p.restHours });   // el trabajo activo ya lo hizo la persona
        Debug.Log($"[Cocina] {who} elabora «{p.name}» en {p.equipment}{(p.restHours > 0 ? $" (ahora reposa {Hours(p.restHours)})" : "")}.");
        return true;
    }

    /// <summary>El servicio: emplata hasta <paramref name="n"/> raciones de la carta que estén listas (platillos compuestos).</summary>
    public int ServeService(int n, string who)
    {
        int served = 0;
        for (int round = 0; round < n && served < n; round++)
            foreach (Gastronomy.Dish d in Gastronomy.Menu) { if (served >= n) break; if (CanServe(d) && Serve(d.name)) served++; }
        Debug.Log($"[Cocina] {who}: servicio con {served} platos de la carta.");
        return served;
    }

    void Ensure(string item, float want, int depth)
    {
        if (depth > 6 || Have(item) >= want) return;
        Gastronomy.Prep p = Gastronomy.PrepOf(item);
        if (p == null) return;   // materia prima: llega con el reabastecimiento
        foreach (Batch b in running) if (b.prep == p) return;   // ya hay una tanda en marcha
        bool ready = true;
        foreach (string inp in p.inputs) if (Have(inp) < 1f) { ready = false; Ensure(inp, 1f, depth + 1); }
        if (!ready || running.Count >= maxConcurrent) return;
        if (_collect != null) { if (!_collect.Contains(p)) _collect.Add(p); return; }   // solo listar (misiones)
        foreach (string inp in p.inputs) Take(inp, 1f);
        running.Add(new Batch { prep = p, hoursLeft = p.activeMin / 60f + p.restHours });
    }

    /// <summary>Llega el género: cultivos que hay plantados en el santuario, fermentación y básicos. La carne la trae el
    /// laboratorio al cosechar.</summary>
    public void Restock()
    {
        // Lo plantado se vuelve a leer si cambió el número de matas: la despensa arranca (Start) antes que algunos huertos del
        // Árbol de los Campos (cada GardenPlot planta en su Start) y se quedaba para siempre con los cultivos de los primeros
        // (sin trigo ni tomate: el obrador hacía 9 elaboraciones y ningún plato de la carta).
        if (_cropsPlanted == null || _cropsPlanted.Count == 0 || _cropsSeen != GardenCrop.All.Count)
        {
            _cropsSeen = GardenCrop.All.Count;
            _cropsPlanted = new HashSet<string>();
            foreach (GardenCrop c in GardenCrop.All) if (c != null) _cropsPlanted.Add(c.kind.ToString());
        }
        foreach (var kv in Gastronomy.Raw)
        {
            var (src, from) = kv.Value;
            if (src == Gastronomy.Source.CarneCultivada) continue;
            if (src == Gastronomy.Source.Cultivo && _cropsPlanted.Count > 0 && !_cropsPlanted.Contains(from)) continue;
            if (Have(kv.Key) < 8f) Add(kv.Key, 4f);
        }
    }

    /// <summary>El laboratorio entrega carne cultivada de una línea celular (res, pollo, salmón…).</summary>
    public void ReceiveMeat(string item, float portions) { Add(item, portions); Debug.Log($"[Cocina] Llega del laboratorio: {item} ×{portions:0}."); }

    /// <summary>¿Se puede emplatar ya? (todas sus elaboraciones y materias primas en la despensa).</summary>
    public bool CanServe(Gastronomy.Dish d)
    {
        foreach (string c in d.components) if (Have(c) < 1f) return false;
        return true;
    }

    /// <summary>Emplata una ración: descuenta sus componentes.</summary>
    public bool Serve(string dishName)
    {
        Gastronomy.Dish d = Gastronomy.DishOf(dishName);
        if (d == null || !CanServe(d)) return false;
        foreach (string c in d.components) Take(c, 1f);
        dishesServed++;
        Debug.Log($"[Cocina] Sale «{d.name}» ({d.origin}) del {d.equipment}.");
        return true;
    }

    /// <summary>Qué le falta a un platillo (vacío si se puede servir).</summary>
    public List<string> Missing(Gastronomy.Dish d)
    {
        var l = new List<string>();
        foreach (string c in d.components) if (Have(c) < 1f) l.Add(c);
        return l;
    }

    public float Have(string item) => stock.TryGetValue(item, out float v) ? v : 0f;
    public void Add(string item, float n) { stock[item] = Have(item) + n; }
    void Take(string item, float n) { stock[item] = Mathf.Max(0f, Have(item) - n); }

    static string Hours(float h) => h >= 720f ? $"{h / 720f:0.#} meses" : h >= 48f ? $"{h / 24f:0.#} días" : h >= 1f ? $"{h:0.#} h" : $"{h * 60f:0} min";

    public string Report()
    {
        var sb = new System.Text.StringBuilder();
        int ready = 0;
        foreach (Gastronomy.Dish d in Gastronomy.Menu) if (CanServe(d)) ready++;
        sb.Append($"Carta {ready}/{Gastronomy.Menu.Length} listos · elaboraciones hechas {prepsDone} · servidos {dishesServed} · en marcha: ");
        foreach (Batch b in running) sb.Append($"{b.prep.name} ({Hours(Mathf.Max(0f, b.hoursLeft))}), ");
        return sb.ToString();
    }
}
