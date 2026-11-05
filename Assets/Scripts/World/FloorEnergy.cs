using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA DEMANDA DE UN PISO (Berón 2026-11-05; docs/energia.md): cada piso declara la energía que necesita, y de ahí salen sus paneles
/// (<see cref="EnergyRule.TablesFor"/>). Se pone al construir (<c>EnsureFloorEnergy</c>) o, si el piso nace en juego, al validar
/// (<see cref="EnergyRule.DeclareMissing"/>). Un piso con un consumo propio (el clúster de IA) lo suma en <see cref="fixedKW"/>.
/// </summary>
public class FloorEnergy : MonoBehaviour
{
    public static readonly List<FloorEnergy> All = new List<FloorEnergy>();

    public string floor = "";
    public float areaM2, wattsPerM2 = 16f, fixedKW;
    [Tooltip("Se sostiene con las baterías si para el reactor.")]
    public bool essential;

    /// <summary>La potencia media que pide (kW).</summary>
    public float KW => fixedKW + areaM2 * wattsPerM2 / 1000f;

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    /// <summary>(Editor) Fuera de Play no corre OnEnable: rehace la lista con lo que hay en la escena.</summary>
    public static void Refresh() { All.Clear(); All.AddRange(FindObjectsByType<FloorEnergy>(FindObjectsInactive.Include, FindObjectsSortMode.None)); }

    /// <summary>Declara el área <paramref name="a"/>: su superficie por la de su clase, y lo fijo de Informática.</summary>
    public static FloorEnergy Declare(SanctuaryArea a)
    {
        FloorEnergy f = a.GetComponent<FloorEnergy>();
        if (f == null) f = a.gameObject.AddComponent<FloorEnergy>();
        f.floor = a.displayName;
        Vector2 s = SizeOf(a.transform);
        f.areaM2 = s.x * s.y;
        f.wattsPerM2 = EnergyRule.WattsPerM2(a.areaType);
        f.fixedKW = a.areaType == SanctuaryAreaType.Informatics ? EnergyRule.InformaticsKW : 0f;
        f.essential = EnergyRule.Essential(a.areaType);
        return f;
    }

    // El tamaño del piso: el de sus plazas o el de su suelo «Mat».
    static Vector2 SizeOf(Transform area)
    {
        PlatformSlots ps = area.GetComponent<PlatformSlots>();
        if (ps != null && ps.floorSize.x > 1f) return ps.floorSize;
        Transform mat = area.Find("Mat");
        if (mat != null && mat.TryGetComponent(out Renderer r)) return new Vector2(r.bounds.size.x, r.bounds.size.z);
        return new Vector2(18f, 15f);
    }
}
