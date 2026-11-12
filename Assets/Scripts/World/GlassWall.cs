using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PARED DE CRISTAL (Berón 2026-11-03: «para salas que deban estar aisladas se pueden utilizar paredes de cristal de modo que no
/// bloquean visibilidad y se debe mantener las reglas de espaciado, en este caso a dichas paredes»; docs/hospital.md §Paredes de
/// cristal). Es la ÚNICA pared permitida en las plataformas (la regla es «sin muros ni techos»: tapan la vista y el paso): el
/// cristal aísla el aire (aislamiento con presión negativa, quirófano con presión positiva, UCI, sala fría de pingüinos) sin
/// tapar la vista. Y GUARDA LOS 2 m como un mueble: <see cref="FurnitureSpacing"/> la mide (prefijo «Pared_cristal») contra todos
/// los muebles de dentro y de fuera de su sala; dos paños de cristal pueden ir pegados (una sala junto a otra comparte mampara).
/// En los microcosmos, <see cref="MicroBuildRules"/> la deja pasar por la regla 2 y la mide en la regla 1.
/// </summary>
public class GlassWall : MonoBehaviour
{
    public string room;
    public const string Prefix = "Pared_cristal";
    public const float Height = 2.8f, Thick = 0.08f, DoorWidth = 2f, DoorHeight = 2.5f;   // (2026-11-05: 2 m, los del paso de S7; antes 1,8)
    /// <summary>Holgura extra de la sala sobre sus 2 m: cubre el perfil de aluminio (sobresale 0,024 m) con margen.</summary>
    public const float ProfileClear = 0.05f;
    public static readonly Color GlassTint = new Color(0.78f, 0.92f, 1f, 0.22f), Frame = new Color(0.72f, 0.75f, 0.78f);

    /// <summary>
    /// UNA SALA DE CRISTAL alrededor de <paramref name="furniture"/> (en el marco de <paramref name="parent"/>): su caja + 2 m a
    /// cada lado (el paso de la regla, dentro de la sala), cuatro paños de cristal con perfil de aluminio y una PUERTA corredera
    /// abierta (hueco de 2 m) en el lado <paramref name="doorSide"/> (0 = +Z, 1 = +X, 2 = −Z, 3 = −X), centrada (o desplazada
    /// <paramref name="doorOffset"/> m). Devuelve la raíz «Sala_cristal_…» (no es un mueble: lo son sus paños).
    /// </summary>
    public static Transform Room(Transform parent, string name, IList<Transform> furniture, int doorSide, float margin = 2f, float doorOffset = 0f, Vector2? minSize = null)
    {
        Bounds b = default; bool any = false;
        foreach (Transform f in furniture)
        {
            if (f == null) continue;
            foreach (Renderer r in f.GetComponentsInChildren<Renderer>())
            {
                Bounds lb = Wrap(parent.worldToLocalMatrix * r.transform.localToWorldMatrix, r.localBounds);
                if (!any) { b = lb; any = true; } else b.Encapsulate(lb);
            }
        }
        if (!any) return null;
        // + ProfileClear: el perfil y el zócalo de aluminio sobresalen 0,024 m del cristal hacia dentro; sin esto el paso
        // medido (FurnitureSpacing, con el perfil) quedaba en 1,97–1,98 m (2026-11-03).
        float m = margin + ProfileClear;
        Vector2 size = new Vector2(b.size.x + 2f * m + 2f * Thick, b.size.z + 2f * m + 2f * Thick);
        if (minSize.HasValue) size = Vector2.Max(size, minSize.Value);
        Vector3 c = new Vector3(b.center.x, 0f, b.center.z);
        float floorY = b.min.y;
        return Box(parent, name, c, size, floorY, doorSide, doorOffset);
    }

    /// <summary>Una sala de cristal de <paramref name="size"/> (x, z) centrada en <paramref name="c"/> (marco de <paramref name="parent"/>).</summary>
    public static Transform Box(Transform parent, string name, Vector3 c, Vector2 size, float floorY, int doorSide, float doorOffset = 0f, int skipSide = -1)
    {
        Transform room = new GameObject("Sala_cristal_" + name).transform;
        room.SetParent(parent, false); room.localPosition = new Vector3(c.x, floorY, c.z);
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        for (int s = 0; s < 4; s++)
        {
            bool alongX = s == 0 || s == 2;
            float len = alongX ? size.x : size.y;
            Vector3 mid = s == 0 ? new Vector3(0f, 0f, hz - Thick * 0.5f) : s == 1 ? new Vector3(hx - Thick * 0.5f, 0f, 0f) : s == 2 ? new Vector3(0f, 0f, -hz + Thick * 0.5f) : new Vector3(-hx + Thick * 0.5f, 0f, 0f);
            Vector3 along = alongX ? Vector3.right : Vector3.forward;
            if (s == skipSide) continue;   // (una antesala pegada a sus salas: ese lado lo cierran ellas)
            if (s != doorSide) { Pane(room, name, mid, along, len, $"{Prefix}_{name}_{s}"); continue; }
            // El hueco de la puerta (corredera abierta, aparcada por dentro junto al hueco).
            float d0 = Mathf.Clamp(doorOffset, -len * 0.5f + DoorWidth * 0.5f + 0.2f, len * 0.5f - DoorWidth * 0.5f - 0.2f);
            float a0 = -len * 0.5f, a1 = d0 - DoorWidth * 0.5f, b0 = d0 + DoorWidth * 0.5f, b1 = len * 0.5f;
            if (a1 - a0 > 0.05f) Pane(room, name, mid + along * ((a0 + a1) * 0.5f), along, a1 - a0, $"{Prefix}_{name}_{s}a");
            if (b1 - b0 > 0.05f) Pane(room, name, mid + along * ((b0 + b1) * 0.5f), along, b1 - b0, $"{Prefix}_{name}_{s}b");
            // dintel de aluminio sobre el hueco (no baja: el hueco queda libre a 2,4 m) y el rótulo de la puerta
            Transform lintel = KitchenProps.C(room, mid + along * d0 + Vector3.up * (Height - 0.15f), Abs(along * DoorWidth) + new Vector3(Thick, 0.3f, Thick) - Abs(along * Thick), Frame, 0f, "Dintel_cristal");
            lintel.gameObject.AddComponent<GlassWall>().room = name;
        }
        return room;
    }

    static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    // Un paño: el cristal (con su caja de colisión: no se atraviesa) y su perfil de aluminio arriba y abajo.
    static void Pane(Transform room, string name, Vector3 mid, Vector3 along, float len, string paneName)
    {
        Vector3 size = along * len + new Vector3(along.x == 0f ? Thick : 0f, 0f, along.z == 0f ? Thick : 0f);
        size = Abs(size); size.y = Height;
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = paneName;
        g.transform.SetParent(room, false);
        g.transform.localPosition = mid + Vector3.up * (Height * 0.5f);
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(GlassTint);
        g.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        g.AddComponent<GlassWall>().room = name;
        Vector3 pf = along.x != 0f ? new Vector3(1f, 0.04f / Height, 1.6f) : new Vector3(1.6f, 0.04f / Height, 1f), pz = along.x != 0f ? new Vector3(1f, 0.06f / Height, 1.6f) : new Vector3(1.6f, 0.06f / Height, 1f);
        KitchenProps.C(g.transform, new Vector3(0f, 0.5f - 0.02f / Height, 0f), pf, Frame, 0f, "Perfil");
        KitchenProps.C(g.transform, new Vector3(0f, -0.5f + 0.03f / Height, 0f), pz, Frame, 0f, "Zocalo");
    }

    static Bounds Wrap(Matrix4x4 m, Bounds b)
    {
        Bounds o = new Bounds(m.MultiplyPoint3x4(b.center), Vector3.zero);
        for (int i = 0; i < 8; i++)
            o.Encapsulate(m.MultiplyPoint3x4(b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f))));
        return o;
    }

    /// <summary>Lo que hay DENTRO de la sala de <paramref name="room"/> (en su marco): para comprobar que guarda los 2 m.</summary>
    public static List<GlassWall> Panes(Transform scope)
    {
        var l = new List<GlassWall>();
        foreach (GlassWall g in scope.GetComponentsInChildren<GlassWall>(true)) if (g.name.StartsWith(Prefix)) l.Add(g);
        return l;
    }
}
