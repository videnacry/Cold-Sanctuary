using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; dentro de
// BreathingMask.cs, los puestos que pone el Blockout quedaban «Missing Script» al entrar en Play.)
/// <summary>Un PUESTO DE MÁSCARAS de un edificio-árbol: perchero de la entrada o estación interior (dentro del agua). Lleva la
/// cuenta de sus máscaras (se ven colgadas: <see cref="Masks"/> de <see cref="capacity"/>) y su ESTADO (las juntas, la válvula y
/// la carga de las botellas: baja un poco cada día; lo repone quien cuida los jardines del edificio, ver FruitGardener).</summary>
public class MaskStation : MonoBehaviour
{
    public enum Kind { Rack, Station }
    public Kind kind = Kind.Rack;
    public string building = "";
    public int capacity = 8;
    [SerializeField] int masks = 8;
    /// <summary>Estado de las máscaras del puesto (1 = revisadas hoy). Baja <see cref="DecayPerDay"/> al día.</summary>
    [Range(0f, 1f)] public float condition = 1f;
    /// (2026-11-04) Baja un 10 % al día: con la revisión por ROTACIÓN (cada puerta una vez por semana, ver FruitGardener) llega a la
    /// semana con un 30–40 %. Por debajo de <see cref="FlagBelow"/> su luz se pone roja y el panel del árbol la marca: se revisa ese día.
    public const float DecayPerDay = 0.1f;
    public const float FlagBelow = 0.3f;
    /// <summary>Días de la rotación: cada día se revisa a mano 1/7 de las puertas de cada árbol.</summary>
    public const int RotationDays = 7;
    /// <summary>¿Hay que revisarlo a mano el día <paramref name="day"/>? Le toca por la rotación (su número entre las estaciones del
    /// árbol) o su luz está en rojo.</summary>
    public bool Due(int day, int index) => condition < FlagBelow || ((index % RotationDays) + RotationDays) % RotationDays == ((day % RotationDays) + RotationDays) % RotationDays;
    /// <summary>La LUZ del puesto (el indicador que lee el panel): verde ≥ 50 %, ámbar, roja bajo <see cref="FlagBelow"/>.</summary>
    public Color Indicator => condition >= 0.5f ? new Color(0.4f, 1f, 0.6f) : condition >= FlagBelow ? new Color(1f, 0.75f, 0.2f) : new Color(1f, 0.25f, 0.2f);
    /// <summary>Minutos de juego en revisar el puesto (las juntas y el visor, la válvula y la carga de cada botella).</summary>
    public const float CheckMinutes = 1f;
    /// <summary>Donde se pone quien la coge (dentro del agua, delante del panel; o fuera, delante del perchero).</summary>
    public Transform standPoint;

    public static readonly List<MaskStation> All = new List<MaskStation>();
    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() => All.Remove(this);

    public int Masks => masks;
    public Vector3 SwimPoint => standPoint != null ? standPoint.position : transform.position;
    public bool Underwater => WaterZone.Underwater(SwimPoint);

    int _lastDay = -1;
    void Update()
    {
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        if (_lastDay < 0) _lastDay = day;
        if (day != _lastDay) { condition = Mathf.Max(0f, condition - DecayPerDay * (day - _lastDay)); _lastDay = day; Light(); Rebalance(building, day); }
    }

    public bool TryTake()
    {
        if (masks <= 0) return false;
        masks--; Refresh();
        if (masks == 0) Restock();   // (2026-11-04) el perchero de una entrada muy usada se quedaba vacío a media jornada (piso 5 del Árbol, prueba 70)
        return true;
    }

    /// <summary>Deja una máscara (si está lleno, la guarda en el cajón de reserva: hasta el doble de su capacidad).</summary>
    public void Put() { masks = Mathf.Min(capacity * 2, masks + 1); Refresh(); }

    /// <summary>Revisión de quien cuida el edificio: juntas, visor, válvula y carga de las botellas; repone hasta la capacidad.</summary>
    public void Check(string who)
    {
        float before = condition;
        condition = 1f;
        if (masks < capacity) masks = capacity;
        Refresh(); Light();
        Debug.Log($"[Mascaras] «{who}» revisa {name}: estado {before * 100f:0}% → 100%, {masks} máscaras.");
    }

    // Las máscaras colgadas (hijas llamadas «Mascara»): se ven tantas como quedan.
    void Refresh()
    {
        int i = 0;
        foreach (Transform t in transform)
        {
            if (!t.name.StartsWith("Mascara")) continue;
            t.gameObject.SetActive(i < masks);
            i++;
        }
    }

    void Start() { KeepDry(); Refresh(); Light(); }

    // (2026-11-04) Un PERCHERO no puede quedar dentro del agua (el del piso 5 del Árbol de los Campos salía «mojado», prueba 70): si lo
    // está, pasa al otro lado de su puerta; y si también ahí, se aparta de la pared hasta salir del agua (con tope).
    void KeepDry()
    {
        if (kind != Kind.Rack) return;
        bool Wet() => WaterZone.Underwater(SwimPoint + Vector3.up) || WaterZone.Underwater(transform.position + Vector3.up);
        if (!Wet()) return;
        Vector3 lp = transform.localPosition, was = lp;
        transform.localPosition = new Vector3(-lp.x, lp.y, lp.z);
        for (int i = 0; i < 6 && Wet(); i++) transform.localPosition += new Vector3(0f, 0f, 1f);
        if (Wet()) transform.localPosition = was;
        Debug.Log($"[Mascaras] {name} estaba dentro del agua: {(Wet() ? "sigue mojado" : "movido a " + transform.localPosition)}.");
    }

    /// <summary>El puesto más cercano a <paramref name="p"/> (de una clase, o de cualquiera si null), a menos de
    /// <paramref name="max"/> m; <paramref name="underwater"/>: solo los que están dentro del agua (y solo los que no, si false
    /// y es un perchero). <paramref name="withMasks"/>: solo los que tienen alguna.</summary>
    public static MaskStation Nearest(Vector3 p, Kind? kind, float max, bool underwater, bool withMasks = false)
    {
        MaskStation best = null; float bd = max * max;
        foreach (MaskStation m in All)
        {
            if (m == null || !m.isActiveAndEnabled) continue;
            if (kind.HasValue && m.kind != kind.Value) continue;
            if (withMasks && m.masks <= 0) continue;
            if (underwater && m.kind == Kind.Rack) continue;
            float d = (m.SwimPoint - p).sqrMagnitude;
            if (d < bd) { bd = d; best = m; }
        }
        return best;
    }

    // EL CARRO DE MÁSCARAS (2026-11-04): un puesto que se queda sin ninguna recibe la mitad de las del puesto más lleno de su edificio
    // (hasta su capacidad). Así una entrada muy usada no se queda vacía entre los repartos de cada noche.
    void Restock()
    {
        MaskStation rich = null;
        foreach (MaskStation m in Of(building)) if (m != this && m.masks > 1 && (rich == null || m.masks > rich.masks)) rich = m;
        if (rich == null) return;
        int n = Mathf.Min(capacity, rich.masks / 2);
        rich.masks -= n; masks += n;
        rich.Refresh(); Refresh();
        Debug.Log($"[Mascaras] {name} se quedó sin máscaras: el carro trae {n} de {rich.name} ({building}).");
    }

    // REPARTO DE CADA NOCHE (2026-11-04): las máscaras van de la entrada por la que se entra a la puerta por la que se sale, así que
    // unos percheros se vacían y otros se llenan (el del piso 5 del Árbol de los Campos quedaba vacío en la prueba 70). Al cambiar el
    // día, en cada edificio, los puestos con más de su capacidad pasan las que les sobran a los que tienen menos (el carro de máscaras).
    static readonly Dictionary<string, int> _rebalancedDay = new Dictionary<string, int>();
    static void Rebalance(string building, int day)
    {
        if (building == null || (_rebalancedDay.TryGetValue(building, out int d) && d == day)) return;
        _rebalancedDay[building] = day;
        var list = new List<MaskStation>(); foreach (MaskStation m in Of(building)) list.Add(m);
        int moved = 0;
        foreach (MaskStation poor in list)
            foreach (MaskStation rich in list)
            {
                if (poor.masks >= poor.capacity) break;
                while (rich != poor && rich.masks > rich.capacity && poor.masks < poor.capacity) { rich.masks--; poor.masks++; moved++; }
            }
        if (moved > 0) { foreach (MaskStation m in list) m.Refresh(); Debug.Log($"[Mascaras] Reparto de la noche en {building}: {moved} máscaras de los percheros llenos a los vacíos."); }
    }

    // La luz de la estación (verde, ámbar o roja según su estado): el indicador que lee el panel del árbol.
    void Light()
    {
        Transform l = transform.Find("Luz_estacion");
        if (l != null && l.TryGetComponent(out Renderer r)) r.sharedMaterial = KitchenProps.Mat(Indicator);
    }

    public static IEnumerable<MaskStation> Of(string building)
    {
        foreach (MaskStation m in All) if (m != null && m.building == building) yield return m;
    }
}
