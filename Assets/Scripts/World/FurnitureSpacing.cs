using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ESPACIO MÍNIMO ENTRE MUEBLES (Berón 2026-10-22: que no se apilen y los personajes puedan pasar). Cada tipo de mueble
/// tiene el pasillo libre que pide a su alrededor; dos muebles del mismo piso más juntos que eso son una VIOLACIÓN
/// (los personajes no pasan o pasan por encima). Hay cosas que sí pueden ir juntas (ovillos, botellas, placas: son piezas
/// de un mueble, no muebles), así que solo se miden los tipos de la tabla.
///
/// Pasillos de referencia: entre máquinas de gimnasio ~0,6–0,9 m (guías de diseño de gimnasios, p. ej. ACSM); entre
/// mesas de restaurante ~0,9 m para que pase el camarero; entre camas de hospital ≥ 0,9 m (acceso por los dos lados).
/// </summary>
public static class FurnitureSpacing
{
    /// <summary>Prefijo del nombre del mueble → pasillo libre mínimo (m) a su alrededor.</summary>
    public static readonly (string prefix, float gap)[] Rules =
    {
        ("Maquina_", 0.9f),     // gimnasio
        ("Mesa_", 0.9f),        // comedor (la mesa con sus sillas)
        ("Cama ", 0.9f),        // camas de hospital
        ("Puesto_", 0.9f),      // puestos de cocina
        ("Telar", 0.8f),
        ("Aseos", 0.9f),
    };

    static float GapFor(string name)
    {
        foreach (var r in Rules) if (name.StartsWith(r.prefix)) return r.gap;
        return -1f;
    }

    /// <summary>Pares de muebles de <paramref name="root"/> más juntos que su pasillo mínimo (mismo piso).</summary>
    public static List<(Transform a, Transform b, float gap, float need)> Violations(Transform root)
    {
        var items = new List<(Transform t, Bounds b, float need)>();
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            float need = GapFor(t.name);
            if (need < 0f || t.parent == null || GapFor(t.parent.name) >= 0f) continue;   // solo el mueble, no sus piezas
            Renderer[] rs = t.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) continue;
            Bounds b = rs[0].bounds; foreach (Renderer r in rs) if (r.enabled) b.Encapsulate(r.bounds);
            items.Add((t, b, need));
        }
        var bad = new List<(Transform, Transform, float, float)>();
        for (int i = 0; i < items.Count; i++)
            for (int j = i + 1; j < items.Count; j++)
            {
                var (ta, ba, na) = items[i]; var (tb, bb, nb) = items[j];
                if (Mathf.Abs(ba.min.y - bb.min.y) > 1.5f) continue;   // otro piso (entreplanta)
                float dx = Mathf.Max(0f, Mathf.Max(ba.min.x - bb.max.x, bb.min.x - ba.max.x));
                float dz = Mathf.Max(0f, Mathf.Max(ba.min.z - bb.max.z, bb.min.z - ba.max.z));
                float gap = Mathf.Sqrt(dx * dx + dz * dz), need = Mathf.Max(na, nb);
                if (gap < need) bad.Add((ta, tb, gap, need));
            }
        return bad;
    }

    /// <summary>Revisa todas las áreas humanas y avisa por consola de cada par demasiado junto.</summary>
    public static int Report()
    {
        int total = 0;
        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
        {
            var bad = Violations(a.transform);
            total += bad.Count;
            foreach (var (x, y, gap, need) in bad)
                Debug.LogWarning($"[Espacio] {a.displayName}: «{x.name}» y «{y.name}» a {gap:0.00} m (piden {need:0.0} m de paso).");
        }
        Debug.Log($"[Espacio] {total} pares de muebles sin su pasillo mínimo.");
        return total;
    }
}
