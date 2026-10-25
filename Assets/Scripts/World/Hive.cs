using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// COLMENA de las abejas de las nieves: el panal (Nido_Abejas) como lo usan las abejas de verdad (docs/abejas-y-panal.md).
/// Cada CELDILLA tiene un estado y un color que lo deja ver:
///   vacía (oscura) → huevo (la reina pone uno por celdilla) → larva (las nodrizas la alimentan) → cría operculada (tapada
///   de cera parda) → nace una abeja y la celdilla queda sucia hasta que una limpiadora la deja lista.
///   néctar (ámbar brillante, lo dejan las pecoreadoras) → las procesadoras lo abanican hasta que pierde agua → miel
///   operculada (tapa de cera clara). Polen (naranja), junto a la cría: comida de las larvas.
/// La disposición es la natural: miel arriba (en arco), polen alrededor de la cría y la cría abajo/centro.
/// Un "día de abeja" dura <see cref="secondsPerDay"/> s de juego (los tiempos reales: huevo 3 días, larva 6, pupa 12).
/// </summary>
public class Hive : MonoBehaviour
{
    public enum Cell { Empty, Dirty, Egg, Larva, CappedBrood, Nectar, Honey, Pollen }

    [Tooltip("Segundos reales por día de abeja; 0 = el día de juego del reloj (Clock: 1 h por defecto).")]
    public float secondsPerDay = 0f;
    /// <summary>Segundos reales de un día de abeja (el del reloj del juego salvo que se fije otro).</summary>
    public float DaySeconds => secondsPerDay > 0f ? secondsPerDay : (TimeController.timeController != null ? TimeController.timeController.SecondsPerGameDay : 3600f);
    [Tooltip("Máximo de abejas de la colonia (las que nacen de más no se crean).")]
    public int maxBees = 12;

    public readonly List<Transform> cells = new List<Transform>();
    public readonly List<Cell> state = new List<Cell>();
    readonly List<float> _since = new List<float>();     // días en el estado actual
    public float honeyStock { get; private set; }         // reserva (celdillas de miel) en gramos
    public int born { get; private set; }
    public Transform entrance;                            // la piquera
    public Vector3 combCenter => transform.position + Vector3.down * 0.35f;

    static readonly Dictionary<Cell, Color> Colors = new Dictionary<Cell, Color>
    {
        { Cell.Empty, new Color(0.45f, 0.3f, 0.12f) }, { Cell.Dirty, new Color(0.3f, 0.22f, 0.12f) },
        { Cell.Egg, new Color(0.96f, 0.95f, 0.9f) }, { Cell.Larva, new Color(0.98f, 0.96f, 0.85f) },
        { Cell.CappedBrood, new Color(0.7f, 0.52f, 0.3f) }, { Cell.Nectar, new Color(0.95f, 0.65f, 0.15f) },
        { Cell.Honey, new Color(0.98f, 0.9f, 0.62f) }, { Cell.Pollen, new Color(0.95f, 0.55f, 0.1f) },
    };

    public static readonly List<Hive> All = new List<Hive>();
    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Awake()
    {
        cells.Clear(); state.Clear(); _since.Clear();
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.name == "Celdilla") { cells.Add(t); state.Add(Cell.Empty); _since.Add(0f); }
            else if (t.name == "Piquera") entrance = t;
        }
        CombineCells();   // en juego, todas las celdillas en una malla (rendimiento)
        // Disposición inicial natural: arco de miel arriba, polen debajo, el resto vacío para que la reina ponga.
        float top = float.MinValue, bottom = float.MaxValue;
        foreach (Transform c in cells) { top = Mathf.Max(top, c.position.y); bottom = Mathf.Min(bottom, c.position.y); }
        for (int i = 0; i < cells.Count; i++)
        {
            float f = Mathf.InverseLerp(bottom, top, cells[i].position.y);
            Set(i, f > 0.8f ? Cell.Honey : f > 0.65f ? Cell.Pollen : Cell.Empty);
        }
        honeyStock = Count(Cell.Honey) * 2f;
    }

    void Update()
    {
        MoveAt6();
        float days = Time.deltaTime / Mathf.Max(0.1f, DaySeconds);
        for (int i = 0; i < cells.Count; i++)
        {
            _since[i] += days;
            switch (state[i])
            {
                case Cell.Egg: if (_since[i] >= 3f) Set(i, Cell.Larva); break;              // eclosiona a los 3 días
                case Cell.CappedBrood: if (_since[i] >= 12f) Emerge(i); break;            // pupa: sale a los 21 días del huevo
            }
        }
    }



    // MUDANZA DIARIA (abejas de hielo): cada día a las 6:00 la colonia cambia de árbol y se lleva su panal consigo: lo
    // coloca bajo el posadero (plataforma de rama) libre de otro árbol cercano. Las abejas lo siguen (vuelven a su panal).
    int _movedDay = int.MinValue;
    public int moves { get; private set; }

    void MoveAt6()
    {
        TimeController tc = TimeController.timeController;
        if (tc == null || ColdSanctuary.TestRunner.Running) return;
        if (_movedDay == int.MinValue) { _movedDay = tc.Hour >= 6f ? tc.Day : tc.Day - 1; return; }   // el día del arranque no cuenta
        if (tc.Day == _movedDay || tc.Hour < 6f) return;
        _movedDay = tc.Day;
        Transform myTree = transform.parent;
        Transform best = null; float bd = float.MaxValue;
        foreach (Transform t in SceneCache.All<Transform>(30f))
        {
            if (t == null || t.name != "Rama_Plataforma" || t.parent == myTree || t.parent == null) continue;
            bool taken = false; foreach (Hive h in All) if (h != this && h.transform.parent == t.parent && (h.transform.position - t.position).sqrMagnitude < 4f) { taken = true; break; }
            if (taken) continue;
            float d = (t.position - transform.position).sqrMagnitude;
            if (d > 4f && d < bd) { bd = d; best = t; }
        }
        if (best == null) return;
        transform.SetParent(best.parent, true);
        transform.position = best.position + new Vector3(0.5f, -0.05f, 0.2f);
        moves++;
        Debug.Log($"[Hielo] Las 6:00: la colonia de «{name}» se muda con su panal a «{best.parent.name}» ({Mathf.Sqrt(bd):0} m).");
    }
    /// <summary>¿El punto está dentro de la cavidad del panal?</summary>
    public bool Inside(Vector3 p)
    {
        Vector3 l = transform.InverseTransformPoint(p);
        return Mathf.Abs(l.x) < 0.48f && l.y < 0.1f && l.y > -0.9f && Mathf.Abs(l.z) < 0.53f;
    }

    /// <summary>Punto de paso para ir de dentro a fuera (o al revés): por la PIQUERA, la única abertura.</summary>
    public Vector3 Via(Vector3 from, Vector3 target)
    {
        bool tIn = Inside(target), fIn = Inside(from);
        if (tIn == fIn || entrance == null) return target;
        Vector3 below = entrance.position + Vector3.down * 0.25f, above = entrance.position + Vector3.up * 0.18f;
        if (!fIn) return (from - below).sqrMagnitude > 0.02f && (from - entrance.position).sqrMagnitude > 0.01f ? below : above;   // entrar
        return (from - above).sqrMagnitude > 0.02f && (from - entrance.position).sqrMagnitude > 0.01f ? above : below;             // salir
    }
    public int Count(Cell c) { int n = 0; foreach (Cell s in state) if (s == c) n++; return n; }

    public void Set(int i, Cell s)
    {
        state[i] = s; _since[i] = 0f;
        if (_comb != null) { _dirty = true; return; }
        Renderer r = cells[i].GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = KitchenProps.Mat(Colors[s]);
    }

    // RENDIMIENTO: cada panal tiene ~1 200 celdillas y el bosque llegaba a 32 000 renderers solo de celdillas. En juego se
    // dibujan como UNA malla con una submalla (y un color) por estado, que se rehace cuando cambia alguna (como mucho dos
    // veces por segundo). Las celdillas siguen ahí (posición, nombre) para las abejas.
    MeshFilter _comb;
    bool _dirty;
    float _nextBuild;

    void CombineCells()
    {
        if (cells.Count == 0) return;
        var go = new GameObject("Panal_combinado");
        go.transform.SetParent(transform, false);
        _comb = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        _comb.sharedMesh = new Mesh { name = "Panal", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        foreach (Transform c in cells) { Renderer r = c.GetComponent<Renderer>(); if (r != null) r.enabled = false; }
        _dirty = true;
    }

    void OnDestroy() { if (_comb != null && _comb.sharedMesh != null) Destroy(_comb.sharedMesh); }   // la malla del panal se crea en juego: no dejarla huérfana

    void LateUpdate()
    {
        if (_comb == null || !_dirty || Time.time < _nextBuild) return;
        _dirty = false; _nextBuild = Time.time + 0.5f;
        var groups = new Dictionary<Cell, List<CombineInstance>>();
        Matrix4x4 toLocal = _comb.transform.worldToLocalMatrix;
        for (int i = 0; i < cells.Count; i++)
        {
            MeshFilter mf = cells[i] != null ? cells[i].GetComponent<MeshFilter>() : null;
            if (mf == null || mf.sharedMesh == null) continue;
            if (!groups.TryGetValue(state[i], out var l)) groups[state[i]] = l = new List<CombineInstance>();
            l.Add(new CombineInstance { mesh = mf.sharedMesh, transform = toLocal * cells[i].localToWorldMatrix });
        }
        var parts = new List<CombineInstance>(); var mats = new List<Material>(); var temp = new List<Mesh>();
        foreach (var kv in groups)
        {
            var m = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            m.CombineMeshes(kv.Value.ToArray(), true, true);
            temp.Add(m); parts.Add(new CombineInstance { mesh = m, transform = Matrix4x4.identity }); mats.Add(KitchenProps.Mat(Colors[kv.Key]));
        }
        _comb.sharedMesh.Clear();
        _comb.sharedMesh.CombineMeshes(parts.ToArray(), false, false);
        _comb.GetComponent<MeshRenderer>().sharedMaterials = mats.ToArray();
        foreach (Mesh m in temp) Destroy(m);
    }

    public float Since(int i) => _since[i];

    /// <summary>La celdilla más cercana a <paramref name="p"/> en el estado pedido (-1 si no hay). <paramref name="preferTop"/>:
    /// la miel y el néctar van arriba; la cría, abajo.</summary>
    public int Find(Cell s, Vector3 p, bool? preferTop = null)
    {
        int best = -1; float bs = float.MaxValue;
        for (int i = 0; i < cells.Count; i++)
        {
            if (state[i] != s) continue;
            float score = (cells[i].position - p).sqrMagnitude;
            if (preferTop == true) score -= cells[i].position.y * 4f;
            if (preferTop == false) score += cells[i].position.y * 4f;
            if (score < bs) { bs = score; best = i; }
        }
        return best;
    }

    /// <summary>Punto donde se posa una abeja para trabajar la celdilla (por fuera de la lámina).</summary>
    public Vector3 Perch(int i)
    {
        Transform c = cells[i];
        float side = Vector3.Dot(c.position - transform.position, c.right) >= 0f ? 1f : -1f;
        return c.position + c.right * side * 0.12f;
    }

    /// <summary>Las abejas comen miel (más en frío/de noche: tiritan para calentar el racimo).</summary>
    public void Eat(float grams)
    {
        honeyStock = Mathf.Max(0f, honeyStock - grams);
        if (honeyStock < Count(Cell.Honey) * 2f - 2f) { int h = Find(Cell.Honey, combCenter); if (h >= 0) Set(h, Cell.Empty); }
    }

    public void AddHoney(float grams) { honeyStock += grams; }

    void Emerge(int i)
    {
        Set(i, Cell.Dirty);   // queda la muda: una limpiadora la deja lista para otra puesta
        born++;
        IceInsectLife mother = null; int bees = 0;
        foreach (IceInsectLife b in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None))
            if (b.hive == this && b.kind == IceInsectLife.Kind.Bee) { bees++; if (mother == null || b.isQueen) mother = b; }
        if (mother == null || bees >= maxBees) { Debug.Log($"[Colmena] nace una abeja en «{name}» (la colonia está completa: no se añade)."); return; }
        GameObject child = Instantiate(mother.gameObject, Perch(i), Quaternion.identity, mother.transform.parent);
        child.name = mother.name.Replace("(Clone)", "") + "_nacida";
        IceInsectLife life = child.GetComponent<IceInsectLife>();
        if (life != null) { life.isQueen = false; life.ageDays = 0f; }
        Debug.Log($"[Colmena] nace una obrera en «{name}» (días 0: empieza limpiando celdillas).");
    }
}
