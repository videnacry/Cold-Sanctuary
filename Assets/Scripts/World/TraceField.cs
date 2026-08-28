using System.Collections.Generic;
using UnityEngine;

/// <summary>Canales de rastro/subproducto (docs/environmental-navigation.md §4.2). Índice en el `float[]` de cada celda.</summary>
public enum TraceChannel
{
    ScentFood,    // olor de comida/cadáver
    ScentSelf,    // marca propia / olor del ser (territorio, rastro de paso)
    Threat,       // peligro
    Estrus,       // celo
    Sickness,     // enfermedad
    Waste,        // heces/digestión
    Disturbance,  // dar vueltas / presencia nerviosa
}

/// <summary>
/// REJILLA DE RASTROS — un campo de **STIGMERGY** (coordinación indirecta dejando rastros que estimulan la siguiente
/// acción; Grassé 1959, feromonas de hormigas). docs/environmental-navigation.md §4.2/§4.3: campo de depósito para los subproductos
/// **continuos/de rastro** (caminar, saturar una zona), en vez de un GameObject por traza. El mundo se discretiza en
/// celdas; cada celda guarda **cuánta señal hay por canal**. Depositar = O(1); leer/gradiente = O(9 celdas); el
/// decaimiento es **PEREZOSO** (se aplica al TOCAR la celda, poniéndola al día por el tiempo transcurrido) + una poda
/// de baja frecuencia que borra las celdas agotadas. Sin GameObjects/colliders/OverlapSphere → coste **plano**
/// (independiente de la longitud del rastro). Es lo canónico para feromonas de hormigas (encaja con el Microcosmos).
///
/// **Aislado y dormido:** nada deposita/lee todavía; se enchufa cuando `SpellBase.byproduct` deposite (N7) y los
/// impulsos lean el gradiente (N1). Sin instancia en escena, la fachada estática es no-op (seguro).
///
/// Uso: `TraceField.Leave(pos, canal, cantidad)` al emitir; `TraceField.Trail(pos, canal)` → un vector-impulso
/// hacia el olor más fuerte; `TraceField.Sniff(pos, canal)` → intensidad en el punto.
///
/// **Tierra vs mar/aire:** por defecto es 2D (ignora la altura). Con `volumetric = true` incluye la profundidad `y`
/// (gradiente vertical) para nadar/volar; como el diccionario es disperso, 3D no infla memoria y el gradiente solo
/// pasa de 8 a 26 vecinas. Un campo 2D de tierra + un campo 3D de mar = dos instancias con distinto `volumetric`.
/// </summary>
public class TraceField : MonoBehaviour
{
    public static TraceField Instance { get; private set; }

    [Tooltip("Tamaño de celda (m). Fino (2-3) = rastros nítidos, más celdas; grande (5+) = más barato y tosco.")]
    [Min(0.25f)] public float cellSize = 4f;
    [Tooltip("Medio 3D (mar/aire): incluye la profundidad 'y' en la celda → gradiente vertical. False (por defecto, " +
             "tierra): ignora 'y' (rejilla 2D). El diccionario es disperso → 3D no infla memoria; el gradiente mira 26 " +
             "vecinas en vez de 8 (despreciable). Para tener campo 2D en tierra y 3D en el mar, usa dos instancias.")]
    public bool volumetric = false;
    [Tooltip("Fracción que queda por SEGUNDO (decaimiento). 0.95 → un rastro casi se va en ~1-2 min; menor = más efímero.")]
    [Range(0.01f, 0.9999f)] public float decayPerSecond = 0.95f;
    [Tooltip("Tope de intensidad por celda/canal (evita que re-pisar sature sin límite).")]
    [Min(0f)] public float cap = 100f;
    [Tooltip("Por debajo de esto, un canal se considera 0; una celda con todos los canales < esto se PODA.")]
    [Min(0f)] public float epsilon = 0.05f;
    [Tooltip("Cada cuántos segundos se barre para podar celdas agotadas (barato: solo celdas activas).")]
    [Min(0.5f)] public float pruneInterval = 5f;

    class Cell { public readonly float[] c; public float lastUpdate; public Cell(int n) { c = new float[n]; } }

    static readonly int ChannelCount = System.Enum.GetValues(typeof(TraceChannel)).Length;

    readonly Dictionary<long, Cell> _cells = new Dictionary<long, Cell>();
    readonly List<long> _toRemove = new List<long>();
    float _nextPrune;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }
    void OnDestroy() { if (Instance == this) Instance = null; }

    // ── Fachada estática (no-op sin instancia → seguro) ──────────────────────────
    /// <summary>Deposita `amount` del `channel` en la celda de `pos` (emitir un subproducto continuo, p.ej. al caminar).</summary>
    public static void Leave(Vector3 pos, TraceChannel channel, float amount) => Instance?.Deposit(pos, channel, amount);
    /// <summary>Intensidad del `channel` en `pos` (0 si no hay/no hay instancia).</summary>
    public static float Sniff(Vector3 pos, TraceChannel channel) => Instance != null ? Instance.Read(pos, channel) : 0f;
    /// <summary>Vector hacia el olor MÁS FUERTE alrededor (impulso de "seguir el rastro"); Vector3.zero si nada.</summary>
    public static Vector3 Trail(Vector3 pos, TraceChannel channel) => Instance != null ? Instance.Gradient(pos, channel) : Vector3.zero;

    // ── Instancia ────────────────────────────────────────────────────────────────
    public void Deposit(Vector3 pos, TraceChannel channel, float amount)
    {
        if (amount <= 0f) return;
        long key = Key(pos);
        if (!_cells.TryGetValue(key, out Cell cell)) { cell = new Cell(ChannelCount) { lastUpdate = Time.time }; _cells[key] = cell; }
        Settle(cell);
        int i = (int)channel;
        cell.c[i] = Mathf.Min(cap, cell.c[i] + amount);
    }

    public float Read(Vector3 pos, TraceChannel channel)
    {
        if (!_cells.TryGetValue(Key(pos), out Cell cell)) return 0f;
        Settle(cell);
        float v = cell.c[(int)channel];
        return v >= epsilon ? v : 0f;
    }

    public Vector3 Gradient(Vector3 pos, TraceChannel channel)
    {
        int cx = Coord(pos.x), cy = volumetric ? Coord(pos.y) : 0, cz = Coord(pos.z);
        int ch = (int)channel;
        int yLo = volumetric ? -1 : 0, yHi = volumetric ? 1 : 0;   // sin volumen: solo el plano (dy = 0)
        Vector3 net = Vector3.zero;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = yLo; dy <= yHi; dy++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;
                    if (!_cells.TryGetValue(Pack(cx + dx, cy + dy, cz + dz), out Cell cell)) continue;
                    Settle(cell);
                    float v = cell.c[ch];
                    if (v < epsilon) continue;
                    net += new Vector3(dx, dy, dz).normalized * v;   // dirección a la vecina × su intensidad
                }
        return net;
    }

    // Pone la celda al día: aplica el decaimiento acumulado desde la última vez que se tocó (PEREZOSO).
    void Settle(Cell cell)
    {
        float dt = Time.time - cell.lastUpdate;
        if (dt <= 0f) return;
        cell.lastUpdate = Time.time;
        float f = Mathf.Pow(decayPerSecond, dt);
        for (int i = 0; i < cell.c.Length; i++) cell.c[i] *= f;
    }

    void Update()
    {
        if (Time.time < _nextPrune) return;
        _nextPrune = Time.time + pruneInterval;
        _toRemove.Clear();
        foreach (KeyValuePair<long, Cell> kv in _cells)
        {
            Settle(kv.Value);
            if (IsEmpty(kv.Value)) _toRemove.Add(kv.Key);
        }
        for (int i = 0; i < _toRemove.Count; i++) _cells.Remove(_toRemove[i]);
    }

    bool IsEmpty(Cell cell)
    {
        for (int i = 0; i < cell.c.Length; i++) if (cell.c[i] >= epsilon) return false;
        return true;
    }

    // ── Mundo → celda ────────────────────────────────────────────────────────────
    // Clave: 3 coords empaquetadas en un long (21 bits c/u, con sesgo para negativos → rango ±2^20 celdas por eje,
    // >4000 km @ 4 m). En tierra (volumetric=false) la coord y es siempre 0 → la rejilla es 2D exacta, misma ruta.
    int Coord(float world) => Mathf.FloorToInt(world / cellSize);
    long Pack(int x, int y, int z)
    {
        const long B = 1L << 20;   // sesgo: mete [-2^20, 2^20) en [0, 2^21) sin solaparse entre campos
        return (((long)x + B) << 42) | (((long)y + B) << 21) | ((long)z + B);
    }
    long Key(Vector3 pos) => Pack(Coord(pos.x), volumetric ? Coord(pos.y) : 0, Coord(pos.z));

    /// <summary>Nº de celdas activas (debug/perf).</summary>
    public int ActiveCells => _cells.Count;
}
