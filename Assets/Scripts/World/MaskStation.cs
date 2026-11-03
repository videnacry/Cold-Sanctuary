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
    public const float DecayPerDay = 0.25f;
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
        if (day != _lastDay) { condition = Mathf.Max(0f, condition - DecayPerDay * (day - _lastDay)); _lastDay = day; }
    }

    public bool TryTake()
    {
        if (masks <= 0) return false;
        masks--; Refresh();
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
        Refresh();
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

    void Start() { Refresh(); }

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

    public static IEnumerable<MaskStation> Of(string building)
    {
        foreach (MaskStation m in All) if (m != null && m.building == building) yield return m;
    }
}
