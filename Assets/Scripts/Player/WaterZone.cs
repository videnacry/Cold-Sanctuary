using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VOLUMEN DE AGUA NADABLE: un Collider (trigger) que abarca el agua bajo su superficie (el mar bajo la banquisa, la
/// columna de agua de la raíz del árbol…). La SUPERFICIE es lo más alto del volumen. Quien nada (<see cref="SwimBody"/>,
/// el jugador, los que andan con <see cref="PathMover"/>) pregunta por la posición —<see cref="SurfaceAt"/>— en vez de
/// fiarse del borde del trigger: así un teletransporte o una puerta que lo saque del agua no lo deja «nadando fantasma»,
/// y cualquiera (no solo el jugador) sabe si está en el agua, a qué altura queda la superficie y cuánto cubre.
/// El tinte bajo el agua lo pone <see cref="SwimBody"/> según la altura de la CÁMARA (no al tocar el trigger).
/// </summary>
[RequireComponent(typeof(Collider))]
public class WaterZone : MonoBehaviour
{
    public Color underwaterFogColor = new Color(0.05f, 0.2f, 0.35f);

    static readonly List<WaterZone> _all = new List<WaterZone>();
    /// <summary>Los volúmenes de agua activos.</summary>
    public static IReadOnlyList<WaterZone> All => _all;

    Collider _col;
    public Collider Volume => _col != null ? _col : (_col = GetComponent<Collider>());
    /// <summary>Altura de la superficie del agua (lo más alto del volumen).</summary>
    public float Surface => Volume.bounds.max.y;

    void OnEnable() { if (!_all.Contains(this)) _all.Add(this); }
    void OnDisable() => _all.Remove(this);

    /// <summary>¿El punto está dentro del volumen? (sirve para cajas giradas y columnas; ClosestPoint da el propio punto si está dentro)</summary>
    public bool Contains(Vector3 p)
    {
        Collider c = Volume;
        if (c == null || !c.enabled || !c.bounds.Contains(p)) return false;
        if (c is BoxCollider || c is SphereCollider || c is CapsuleCollider || (c is MeshCollider mc && mc.convex))
            return (c.ClosestPoint(p) - p).sqrMagnitude < 1e-6f;
        return true;   // malla cóncava: basta la caja que la envuelve
    }

    /// <summary>El agua que hay en la vertical de <paramref name="p"/>: la zona cuyo volumen contiene ese punto (o, si el punto
    /// queda por ENCIMA de la superficie, el de la superficie justo debajo). Devuelve null si ahí no hay agua.</summary>
    public static WaterZone At(Vector3 p)
    {
        WaterZone best = null; float bestSurface = float.NegativeInfinity;
        for (int i = 0; i < _all.Count; i++)
        {
            WaterZone z = _all[i];
            if (z == null || !z.isActiveAndEnabled) continue;
            Bounds b = z.Volume.bounds;
            if (p.y < b.min.y) continue;
            Vector3 q = p; if (q.y > b.max.y - 0.01f) q.y = b.max.y - 0.01f;
            if (!z.Contains(q)) continue;
            if (b.max.y > bestSurface) { best = z; bestSurface = b.max.y; }   // volúmenes solapados: el de la superficie más alta
        }
        return best;
    }

    /// <summary>¿Hay agua en la vertical de <paramref name="p"/>? <paramref name="surface"/> = su superficie.</summary>
    public static bool SurfaceAt(Vector3 p, out float surface)
    {
        WaterZone z = At(p);
        surface = z != null ? z.Surface : float.NegativeInfinity;
        return z != null;
    }

    /// <summary>¿El punto está bajo el agua (dentro de un volumen, por debajo de su superficie)?</summary>
    public static bool Underwater(Vector3 p) => SurfaceAt(p, out float s) && p.y < s;
}
