using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DESHIERBE CON CUIDADO (Berón 2026-11-05: «el deshierbe se podría realizar con cuidado? podrían servir para poblar áreas del mundo?»;
/// docs/deshierbe-cuidadoso.md). Las malas hierbas de los bancales son plantas de verdad (<see cref="GardenWeed"/>). Se levantan con su cepellón, vivas.
///   · NATIVAS de la tundra: al vivero húmedo y, en la misión de trasplante, a la isla de tierra. Sobreviven según su ficha, y las supervivientes
///     dan comida como cualquier otra planta de la tundra.
///   · NO NATIVAS: raíz y tierra en bolsa cerrada y al INCINERADOR del huerto, nunca al suelo ni al compost (Berón: «hazlo real»: es lo que se hace en la Antártida). Es la regla que protege a los sitios polares (Tratado Antártico, Anexo II,
///     art. 4; Ley de Medio Ambiente de Svalbard, s. 26) y es de lo poco que no se puede deshacer: *Poa annua* dejó banco de semillas en el suelo.
/// </summary>
public static class WildPlants
{
    public sealed class Species
    {
        public string key, name, scientific;
        public bool native;
        /// <summary>Parte de los trasplantes que agarra (solo las nativas).</summary>
        public float survival;
        public AffordanceKind food;
        public bool edible;
        public Color color;
    }

    static Species N(string k, string n, string s, float surv, AffordanceKind f, Color c) => new Species { key = k, name = n, scientific = s, native = true, survival = surv, food = f, edible = true, color = c };
    static Species X(string k, string n, string s, Color c) => new Species { key = k, name = n, scientific = s, color = c };

    /// <summary>Nativas de la tundra (las siembran el viento y las aves desde la isla) y foráneas (llegan con las semillas y las botas).</summary>
    public static readonly Species[] All =
    {
        N("Carex", "Juncia", "Carex bigelowii", 0.85f, AffordanceKind.Grass, new Color(0.45f, 0.6f, 0.34f)),
        N("Eriophorum", "Algodón ártico", "Eriophorum vaginatum", 0.85f, AffordanceKind.Grass, new Color(0.58f, 0.66f, 0.42f)),
        N("Salix", "Sauce ártico", "Salix arctica", 0.6f, AffordanceKind.Willow, new Color(0.5f, 0.56f, 0.34f)),
        N("Empetrum", "Camarina", "Empetrum nigrum", 0.6f, AffordanceKind.Berry, new Color(0.3f, 0.42f, 0.28f)),
        X("Poa", "Espiguilla", "Poa annua", new Color(0.55f, 0.72f, 0.36f)),
        X("Stellaria", "Pamplina", "Stellaria media", new Color(0.5f, 0.68f, 0.4f)),
        X("Taraxacum", "Diente de león", "Taraxacum officinale", new Color(0.62f, 0.66f, 0.3f)),
        X("Rumex", "Acederilla", "Rumex acetosella", new Color(0.6f, 0.42f, 0.3f)),
    };
}

/// <summary>Una mala hierba de un bancal: una planta que se puede levantar entera. La pone <see cref="WeedPatch"/>.</summary>
public class GardenWeed : MonoBehaviour
{
    public static readonly List<GardenWeed> All = new List<GardenWeed>();
    public WildPlants.Species species;
    public GardenPlot plot;
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);
}

/// <summary>Hace salir malas hierbas en un bancal, poco a poco y hasta un tope. <see cref="GardenPlot"/> lo añade al generarse.</summary>
public class WeedPatch : MonoBehaviour
{
    public const int Cap = 5;
    public float everySeconds = 240f;
    GardenPlot _plot;
    System.Random _rnd;
    float _next;

    void Start()
    {
        _plot = GetComponent<GardenPlot>();
        _rnd = new System.Random(_plot != null ? _plot.seed * 31 + 7 : 7);
        for (int i = 0; i < 3; i++) Sprout();
        _next = Time.time + everySeconds;
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + everySeconds;
        int n = 0; foreach (GardenWeed w in GardenWeed.All) if (w != null && w.plot == _plot) n++;
        if (n < Cap) Sprout();
    }

    public void Sprout()
    {
        if (_plot == null) return;
        WildPlants.Species s = WildPlants.All[_rnd.Next(WildPlants.All.Length)];
        Vector2 half = _plot.size * 0.5f;
        Vector3 local = new Vector3(((float)_rnd.NextDouble() - 0.5f) * 2f * half.x * 0.9f, 0.1f, ((float)_rnd.NextDouble() - 0.5f) * 2f * half.y * 0.9f);
        var g = new GameObject("Mala_hierba_" + s.key); g.transform.SetParent(transform, false); g.transform.localPosition = local;
        GardenWeed w = g.AddComponent<GardenWeed>(); w.species = s; w.plot = _plot;
        for (int i = 0; i < 5; i++)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(b.GetComponent<Collider>());
            b.name = "Brizna"; b.transform.SetParent(g.transform, false);
            b.transform.localRotation = Quaternion.Euler((float)_rnd.NextDouble() * 30f - 15f, i * 72f, (float)_rnd.NextDouble() * 30f - 15f);
            b.transform.localPosition = b.transform.localRotation * new Vector3(0f, 0.07f, 0f);
            b.transform.localScale = new Vector3(0.02f, 0.15f, 0.012f);
            b.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(s.color);
        }
    }
}

/// <summary>El VIVERO: lo que se ha levantado y espera sitio, con la cuenta de lo que acaba en cada lado. Lo llevan la jornada y la E.</summary>
public static class WeedBench
{
    /// <summary>Plazas húmedas del vivero. Lleno, las nativas se dejan donde están: mejor en el bancal que secas en una bandeja.</summary>
    public const int NurseryCap = 16;
    public static readonly List<WildPlants.Species> Nursery = new List<WildPlants.Species>();
    /// <summary>Bolsas cerradas con la planta y la tierra de su raíz, esperando al incinerador.</summary>
    public static int Bags { get; private set; }
    public static int Incinerated { get; private set; }
    public static int Transplanted { get; private set; }
    public static int Failed { get; private set; }
    static int _tries;

    /// <summary>Levanta una planta con su cepellón. true si se ha levantado.</summary>
    public static bool Lift(GardenWeed w, string who)
    {
        if (w == null || w.species == null) return false;
        WildPlants.Species s = w.species;
        if (s.native && Nursery.Count >= NurseryCap) return false;
        if (s.native) Nursery.Add(s); else Bags++;
        Object.Destroy(w.gameObject);
        Debug.Log(s.native
            ? $"[Deshierbe] {who} levanta {s.name} ({s.scientific}) con su cepellón: al vivero húmedo."
            : $"[Deshierbe] {who} levanta {s.name} ({s.scientific}), no nativa: a la bolsa cerrada, con la tierra de su raíz, para el incinerador.");
        return true;
    }

    /// <summary>Levanta las malas hierbas del bancal más cercano (hasta <paramref name="most"/>). Devuelve cuántas.</summary>
    public static int LiftNear(Vector3 from, string who, int most = 2)
    {
        GardenPlot plot = null; float bd = float.MaxValue;
        foreach (GardenWeed w in GardenWeed.All)
        {
            if (w == null || w.plot == null) continue;
            float d = (w.plot.transform.position - from).sqrMagnitude;
            if (d < bd) { bd = d; plot = w.plot; }
        }
        int n = 0;
        foreach (GardenWeed w in GardenWeed.All.ToArray())
            if (w != null && w.plot == plot && n < most && Lift(w, who)) n++;
        return n;
    }

    /// <summary>Quema las bolsas de plantas no nativas. Devuelve cuántas.</summary>
    public static int IncinerateBags(string who)
    {
        int n = Bags; Incinerated += n; Bags = 0;
        if (n > 0) Debug.Log($"[Deshierbe] {who} incinera {n} bolsas de plantas no nativas con su tierra.");
        return n;
    }

    /// <summary>Planta en la tundra lo que hay en el vivero. Devuelve cuántas se han plantado.</summary>
    public static int TransplantAll(string who)
    {
        Territorio ter = Object.FindAnyObjectByType<Territorio>();
        PolarUnderstory flora = Object.FindAnyObjectByType<PolarUnderstory>();
        if (ter == null || ter.landPoints.Length == 0) return 0;
        int done = 0;
        foreach (WildPlants.Species s in Nursery.ToArray())
        {
            _tries++;
            if (!FindTundra(ter, _tries, out Vector3 at)) continue;
            Nursery.Remove(s);
            // ¿Agarra? Determinista: una de cada tantas según la ficha.
            if ((_tries * 0.61803f) % 1f > s.survival) { Failed++; Debug.Log($"[Deshierbe] {who}: {s.name} no agarra en la tundra."); continue; }
            Plant(s, at, flora != null ? flora.transform : ter.transform);
            Transplanted++; done++;
        }
        if (done > 0) Debug.Log($"[Deshierbe] {who} trasplanta {done} plantas nativas a la isla de tierra (en total {Transplanted}; no agarraron {Failed}).");
        return done;
    }

    // Un sitio de tundra de verdad: suelo «Tierra» (no orilla), no bajo una roca ni un edificio.
    static bool FindTundra(Territorio ter, int seed, out Vector3 at)
    {
        for (int i = 0; i < 12; i++)
        {
            Vector3 c = ter.landPoints[(seed + i) % ter.landPoints.Length];
            float a = (seed * 2.4f + i * 1.7f), r = 6f + (seed * 7 + i * 5) % 22;
            Vector3 p = c + new Vector3(Mathf.Cos(a) * r, 8f, Mathf.Sin(a) * r);
            if (Physics.Raycast(p, Vector3.down, out RaycastHit h, 12f, ~0, QueryTriggerInteraction.Ignore) && h.collider.name == Territorio.Land) { at = h.point; return true; }
        }
        at = Vector3.zero; return false;
    }

    static void Plant(WildPlants.Species s, Vector3 at, Transform parent)
    {
        var g = new GameObject("Trasplante_" + s.key); g.transform.SetParent(parent, false); g.transform.position = at;
        for (int i = 0; i < 6; i++)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(b.GetComponent<Collider>());
            b.name = "Brizna"; b.transform.SetParent(g.transform, false);
            b.transform.localRotation = Quaternion.Euler(15f, i * 60f, 0f);
            b.transform.localPosition = b.transform.localRotation * new Vector3(0f, 0.1f, 0f);
            b.transform.localScale = new Vector3(0.025f, 0.2f, 0.015f);
            b.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(s.color);
        }
        if (s.edible) Affordance.Put(g, s.food, 2, 2f, 0.0005f);
    }

    /// <summary>Deja las cuentas a cero (pruebas).</summary>
    public static void ResetCounts() { Nursery.Clear(); Bags = Incinerated = Transplanted = Failed = 0; _tries = 0; }
}
