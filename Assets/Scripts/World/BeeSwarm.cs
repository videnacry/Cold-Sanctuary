using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL ENJAMBRE DE PECOREADORAS de una colmena (Berón 2026-11-03: «a las abejas del mesocosmos también se les debería plantear
/// crear como esos grupos para optimizar los procesos; además serían más visibles al verlas volar en grupo de un árbol a otro;
/// deben volar de un árbol a otro durante el día»; docs/grupos-de-animales.md). Un <see cref="Swarm"/> de abejas: un padre con
/// sus abejas (modelos de <see cref="SwarmModels"/>) que DE DÍA vuela de árbol en árbol (las copas de los árboles a menos de
/// <see cref="range"/> m de su colmena; en cada una se queda <see cref="dwellSeconds"/> s pecoreando y deja el néctar en la
/// colmena al volver) y DE NOCHE vuelve a su colmena y se queda en racimo en la piquera.
/// Las abejas de dentro de la colmena (la reina, nodrizas, limpiadoras, guardianas: <see cref="IceInsectLife"/>, grupo 58) siguen
/// siendo individuos con su oficio: el enjambre es la parte de FUERA (el pecoreo), que antes hacía cada pecoreadora por su cuenta.
/// Lo crea la colmena al empezar (<see cref="EnsureFor"/>).
/// </summary>
public class BeeSwarm : Swarm
{
    public Hive hive;
    [Tooltip("Hasta dónde (m) van a pecorear desde su colmena.")]
    public float range = 80f;
    public float flySpeed = 4f;
    [Tooltip("Segundos (reales) que se quedan en la copa de cada árbol.")]
    public float dwellSeconds = 6f;
    [Tooltip("Néctar (g) que traen de cada árbol a la colmena.")]
    public float nectarPerVisit = 2f;

    readonly List<Transform> _trees = new List<Transform>();
    readonly Dictionary<Transform, Vector3> _crown = new Dictionary<Transform, Vector3>();
    Transform _tree; int _treeIdx = -1;
    float _dwellUntil; bool _carrying;
    /// <summary>Árboles distintos visitados (y cuántas visitas en total).</summary>
    public readonly HashSet<Transform> visited = new HashSet<Transform>();
    public int Visits { get; private set; }
    public bool AtHive { get; private set; }
    public Transform CurrentTree => _tree;
    public int TreeCount => _trees.Count;

    public static BeeSwarm EnsureFor(Hive h)
    {
        if (h == null) return null;
        foreach (Swarm s in All) if (s is BeeSwarm b && b.hive == h) return b;
        Vector3 at = Entrance(h);
        BeeSwarm bs = Create<BeeSwarm>(null, $"Enjambre_{h.transform.parent?.name ?? h.name}", at, SwarmKind.Bee, SwarmModels.Species.SnowBee, 18f, 24f, 1.2f);
        bs.hive = h; bs.visitsTrees = false; bs.perUnitMass = 0.02f; bs.growthPerSecond = 0.01f; bs.fleeRange = 4f;
        return bs;
    }

    static Vector3 Entrance(Hive h) => (h.entrance != null ? h.entrance.position : h.transform.position) + Vector3.down * 0.6f;

    protected override void Start()
    {
        base.Start();
        FindTrees();
    }

    void FindTrees()
    {
        _trees.Clear(); _crown.Clear();
        Vector3 home = hive != null ? hive.transform.position : transform.position;
        foreach (IceCarnivorousTree t in FindObjectsByType<IceCarnivorousTree>(FindObjectsSortMode.None))
        {
            Vector3 p = t.transform.position;
            if (new Vector2(p.x - home.x, p.z - home.z).magnitude > range) continue;
            // La copa: lo alto de sus ramas (la caja de sus dibujos), algo por debajo de la cima y por fuera del tronco.
            Bounds b = new Bounds(p, Vector3.zero); bool any = false;
            foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
            float top = any ? b.max.y : p.y + 8f;
            Vector3 c = new Vector3(p.x, p.y + (top - p.y) * 0.72f, p.z) + Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * Vector3.forward * Mathf.Min(3f, (any ? b.extents.x : 2f) * 0.5f);
            _trees.Add(t.transform); _crown[t.transform] = c;
        }
    }

    protected override void Move(float dt)
    {
        if (hive == null) { base.Move(dt); return; }
        float v = flySpeed * Mathf.Clamp(TimeScale, 1f, 4f);
        if (!IsDay || _trees.Count == 0)
        {
            // DE NOCHE: a la colmena, en racimo en la piquera (y dejan lo que traían).
            Vector3 home = Entrance(hive);
            transform.position = Vector3.MoveTowards(transform.position, home, v * dt);
            AtHive = Vector3.Distance(transform.position, home) < 0.5f;
            if (AtHive && _carrying) { hive.AddHoney(nectarPerVisit); _carrying = false; }
            _tree = null;
            return;
        }
        AtHive = false;
        if (_tree == null) NextTree();
        if (_tree == null) return;
        Vector3 crown = _crown[_tree];
        if (Time.time < _dwellUntil) { transform.position = Vector3.MoveTowards(transform.position, crown + 0.6f * new Vector3(Mathf.Sin(Time.time * 0.8f), 0f, Mathf.Cos(Time.time * 0.8f)), v * 0.3f * dt); return; }
        // Vuela en arco (sube por encima de las copas a medio camino).
        Vector3 to = crown;
        float flat = new Vector2(crown.x - transform.position.x, crown.z - transform.position.z).magnitude;
        if (flat > 6f) to.y = Mathf.Max(crown.y, transform.position.y) + Mathf.Min(6f, flat * 0.15f);
        transform.position = Vector3.MoveTowards(transform.position, to, v * dt);
        if (Vector3.Distance(transform.position, crown) < 1f)
        {
            Visits++; visited.Add(_tree); _carrying = true;
            _dwellUntil = Time.time + dwellSeconds;
            if (Visits <= 3 || Visits % 10 == 0) Debug.Log($"[Abejas] el enjambre «{name}» ({Visible} abejas) pecorea en la copa de «{_tree.name}» ({visited.Count} árboles distintos hoy).");
            _tree = null;   // al acabar de pecorear, al siguiente
            _pending = true;
        }
    }
    bool _pending;

    void NextTree()
    {
        if (_pending && Time.time < _dwellUntil) return;
        _pending = false;
        if (_trees.Count == 0) return;
        if (_trees.Count == 1) { _tree = _trees[0]; return; }
        int k;
        do { k = Random.Range(0, _trees.Count); } while (k == _treeIdx);
        _treeIdx = k; _tree = _trees[k];
    }

    /// <summary>Pruebas: fija la colmena y los árboles desde otro sitio.</summary>
    public void Refresh() => FindTrees();
}
