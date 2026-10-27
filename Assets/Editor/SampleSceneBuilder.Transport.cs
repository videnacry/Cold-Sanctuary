using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TRANSPORTE EN ALFOMBRAS VOLADORAS (docs/alfombras-plan.md, Berón 2026-10-27): sin escaleras (salvo las plataformas
/// hijas), sin bordes ni puentes. Cada EDIFICIO tiene:
/// - en cada piso (y en el suelo de hielo, a su pie) un ÁREA DE ALFOMBRAS DE EDIFICIO, que lleva a sus otros pisos;
/// - encima de su piso más alto, un PISO DE ALFOMBRAS (techo) con dos áreas: las de edificio y las del SANTUARIO, que
///   llevan al techo de los demás edificios y a la plataforma central de la residencia.
/// Las áreas se ponen al final (<see cref="PlaceCarpetAreas"/>), con los muebles ya puestos, donde no estorban; las del
/// suelo, cuando ya está el hielo (<see cref="PlaceGroundCarpetAreas"/>).
/// </summary>
public static partial class SampleSceneBuilder
{
    // Los edificios: su planta baja (la base de sus pisos) y su nombre.
    static readonly List<(Transform t, string name)> _buildings = new List<(Transform, string)>();
    // Piso de arriba → el de abajo (StackOn, el huerto de rotación, el techo de alfombras).
    static readonly Dictionary<Transform, Transform> _upperOf = new Dictionary<Transform, Transform>();
    // Huecos del borde (ya no hay: sin puentes) y llegadas de los puentes; se conservan para quien los consulta.
    static readonly Dictionary<Transform, List<(int side, float a, float b)>> _openings = new Dictionary<Transform, List<(int, float, float)>>();
    static readonly List<(Transform area, Vector2 half, int side, float center)> _landings = new List<(Transform, Vector2, int, float)>();

    const float ZoneW = 7.6f, ZoneD = 4.4f;

    /// <summary>Apunta los edificios (sus plantas bajas). Ya no se tienden puentes.</summary>
    static void BuildBridgesAndPads(Transform parent, SanctuaryArea[] areas, Vector2[] sizes)
    {
        _openings.Clear(); _landings.Clear(); _buildings.Clear();
        foreach (SanctuaryArea a in areas) _buildings.Add((a.transform, a.displayName));
        Debug.Log($"[Transporte] {areas.Length} edificios, sin puentes ni escaleras: se va en alfombra voladora.");
    }

    static Transform BaseOf(Transform t) { while (t != null && _upperOf.TryGetValue(t, out Transform low)) t = low; return t; }

    /// <summary>Sus pisos (de abajo arriba), los elevados que acaban en su planta baja.</summary>
    static List<Transform> FloorsOf(Transform b)
    {
        var l = new List<Transform>();
        foreach (Transform t in _platforms.Keys) if (t != null && BaseOf(t) == b) l.Add(t);
        if (!l.Contains(b)) l.Add(b);
        l.Sort((x, y) => x.position.y.CompareTo(y.position.y));
        return l;
    }

    static string FloorName(Transform f, int i, string building)
    {
        SanctuaryArea a = f.GetComponent<SanctuaryArea>();
        if (i == 1) return "Planta baja";
        if (a != null && a.displayName != building) return $"Planta {i - 1} · {a.displayName}";
        string n = f.name.Replace("_Area", "").Replace("Planta_", "").Replace("Huerto_", "Huerto ").Replace('_', ' ');
        return $"Planta {i - 1} · {n}";
    }

    /// <summary>Por cada edificio: el PISO DE ALFOMBRAS encima del más alto (del mismo tamaño) con sus dos áreas, y un área
    /// de alfombras de edificio en cada piso, donde no hay muebles. Los huertos sueltos llevan sus dos áreas en un andén al
    /// lado (encima taparían el sol a los cultivos).</summary>
    static void PlaceCarpetAreas()
    {
        ResolveInnerUpperFloors();
        Transform areasRoot = GameObject.Find("Areas_Placed")?.transform;
        var buildings = new List<(Transform t, string name)>(_buildings);
        foreach (Transform t in _platforms.Keys)   // huertos de gremio elevados: cada uno, un edificio de un piso
            if (t != null && t.name.StartsWith("Huerto_") && !_upperOf.ContainsKey(t) && !buildings.Exists(b => b.t == t))
                buildings.Add((t, GardenPlot.Title(t.GetComponent<GardenPlot>().guild)));
        var all = new List<Bounds>();
        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!r.enabled || r.GetComponent<TMPro.TMP_Text>() != null) continue;
            Bounds b = r.bounds;
            if (b.size.x > 60f || b.size.z > 60f) continue;
            if (b.size.y < 0.3f && (b.size.x > 3f || b.size.z > 3f)) continue;   // suelos, alfombras de suelo, losas
            all.Add(b);
        }
        int zones = 0;
        foreach (var (bt, bname) in buildings)
        {
            if (bt == null) continue;
            List<Transform> floors = FloorsOf(bt);
            Transform top = floors[floors.Count - 1];
            Vector2 ts = _platforms.TryGetValue(top, out Vector2 sz) ? sz : new Vector2(12f, 12f);
            bool plot = bt.GetComponent<GardenPlot>() != null;
            Transform roof = new GameObject(plot ? $"Anden_alfombras_{bname}" : $"Techo_alfombras_{bname}").transform;
            roof.SetParent(areasRoot != null ? areasRoot : bt.parent, true);
            Vector2 rs;
            if (plot)
            {
                rs = new Vector2(Mathf.Max(ZoneW + 2f, 10f), 2f * ZoneD + 3f);
                roof.SetPositionAndRotation(bt.TransformPoint(new Vector3(0f, -ElevatedHeight, -ts.y * 0.5f - rs.y * 0.5f - 0.2f)), bt.rotation);
                Elevate(roof, rs, ElevatedHeight);
                _upperOf[roof] = bt;   // de su edificio (el huerto), al mismo nivel
            }
            else
            {
                rs = ts;
                roof.SetPositionAndRotation(top.position, top.rotation);
                Elevate(roof, rs, UpperFloorGap, groundBelow: false);
                _upperOf[roof] = top;
                int ri = _roofs.FindIndex(r => r.top == top);
                if (ri >= 0) _roofs[ri] = (roof, rs, _roofs[ri].name); else _roofs.Add((roof, rs, bname));   // sus paneles, en el techo
            }
            Solid(roof, new Vector3(0f, -0.05f, 0f), new Vector3(rs.x, 0.14f, rs.y), new Color(0.86f, 0.82f, 0.74f), "Suelo_techo");
            floors = FloorsOf(bt);   // ya con el techo (o el andén)
            for (int i = 0; i < floors.Count; i++)
            {
                Transform f = floors[i];
                bool isRoof = f == roof;
                Vector2 fs = _platforms.TryGetValue(f, out Vector2 fz) ? fz : ts;
                string surf = $"{bname}#{i + 1}", fname = isRoof ? (plot ? "Andén de alfombras" : "Techo (alfombras)") : FloorName(f, i + 1, bname);
                if (isRoof)
                {
                    float dz = Mathf.Min(ZoneD * 0.5f + 0.6f, fs.y * 0.5f - ZoneD * 0.5f - 0.1f);
                    CarpetPad.BuildZone(f, f.TransformPoint(new Vector3(0f, 0.08f, dz)), f.rotation, CarpetPad.Kind.Edificio, bname, i + 1, surf, fname, 2);
                    CarpetPad.BuildZone(f, f.TransformPoint(new Vector3(0f, 0.08f, -dz)), f.rotation, CarpetPad.Kind.Santuario, bname, i + 1, surf, bname, 2);
                    zones += 2;
                    continue;
                }
                Vector3 at = FreeSpot(f, fs, all, out int hits);
                CarpetPad pad = CarpetPad.BuildZone(f, at, f.rotation, CarpetPad.Kind.Edificio, bname, i + 1, surf, fname, 2);
                all.Add(new Bounds(at + Vector3.up * 1.2f, new Vector3(ZoneW, 2.2f, ZoneD)));
                if (hits > 0) Debug.LogWarning($"[Transporte] Alfombras de «{bname} · {fname}»: no hay hueco del todo libre ({hits} choques).");
                zones++;
            }
        }
        _carpetBuildings.Clear(); _carpetBuildings.AddRange(buildings);
        Debug.Log($"[Transporte] {zones} áreas de alfombras en {buildings.Count} edificios (cada piso, y en el techo las de edificio y santuario).");
    }

    static readonly List<(Transform t, string name)> _carpetBuildings = new List<(Transform, string)>();

    // El sitio del piso <paramref name="f"/> donde cabe un área de alfombras con menos muebles encima (mejor cerca de un borde).
    static Vector3 FreeSpot(Transform f, Vector2 size, List<Bounds> all, out int bestHits)
    {
        float fy = f.position.y, hx = size.x * 0.5f, hz = size.y * 0.5f;
        var near = new List<Bounds>();
        Bounds foot = new Bounds(f.position + Vector3.up * 1.3f, new Vector3(size.x + 1f, 2.3f, size.y + 1f));
        if (Mathf.Abs(f.eulerAngles.y) > 1f) foot.Expand(new Vector3(size.y, 0f, size.x));
        foreach (Bounds b in all) if (b.Intersects(foot)) near.Add(b);
        Vector3 best = f.TransformPoint(new Vector3(0f, 0.08f, hz - ZoneD * 0.5f - 0.3f)); bestHits = int.MaxValue; float bestEdge = float.MaxValue;
        float mx = Mathf.Max(0f, hx - ZoneW * 0.5f - 0.3f), mz = Mathf.Max(0f, hz - ZoneD * 0.5f - 0.3f);
        for (float z = -mz; z <= mz + 0.01f; z += 0.5f)
                for (float x = -mx; x <= mx + 0.01f; x += 0.5f)
                {
                    Vector3 w = f.TransformPoint(new Vector3(x, 0.08f, z));
                    Vector3 ext = f.rotation * new Vector3(ZoneW + 0.6f, 0f, ZoneD + 0.6f); ext = new Vector3(Mathf.Abs(ext.x), 2.2f, Mathf.Abs(ext.z));
                    Bounds zb = new Bounds(w + Vector3.up * (0.15f + 1.1f), ext);
                    int n = 0; foreach (Bounds b in near) if (b.Intersects(zb)) n++;
                    float edge = Mathf.Min(hz - Mathf.Abs(z), hx - Mathf.Abs(x));   // cuanto más pegado a un borde, menos estorba el paso
                    if (n < bestHits || (n == bestHits && edge < bestEdge)) { bestHits = n; bestEdge = edge; best = w; }
                }
        // A ras del SUELO de verdad del piso (el suelo del área va algo por encima de su origen: si no, quedaba hundida).
        Physics.SyncTransforms();
        foreach (RaycastHit h in Physics.RaycastAll(best + Vector3.up * 3f, Vector3.down, 4.5f, ~0, QueryTriggerInteraction.Ignore))
            if (h.point.y > best.y - 0.5f && h.point.y < best.y + 2.5f && h.collider.GetComponentInParent<CarpetPad>() == null) { best.y = Mathf.Max(best.y, h.point.y + 0.04f); }
        return best;
    }

    /// <summary>Al pie de cada edificio, en el hielo, un área de alfombras de edificio (la «planta 0»): sin escalera, desde el
    /// suelo se sube en alfombra. Busca el lado (+Z, −Z, +X, −X) con hielo firme debajo.</summary>
    static void PlaceGroundCarpetAreas()
    {
        Physics.SyncTransforms();   // el hielo recién construido (y movido) aún no estaba en la física del editor
        int placed = 0;
        foreach (var (bt, bname) in _carpetBuildings)
        {
            if (bt == null) continue;
            Vector2 s = _platforms.TryGetValue(bt, out Vector2 sz) ? sz : new Vector2(12f, 12f);
            // El hielo firme más cercano al pie (8 direcciones, cada vez más lejos): si el edificio da al agua, en la orilla.
            Vector3? spot = null;
            for (float extra = 2.5f; extra <= 40f && spot == null; extra += 3f)
                for (int a = 0; a < 8 && spot == null; a++)
                {
                    Vector3 dir = Quaternion.Euler(0f, a * 45f, 0f) * Vector3.forward;
                    float out_ = Mathf.Abs(dir.z) * s.y * 0.5f + Mathf.Abs(dir.x) * s.x * 0.5f + ZoneD * 0.5f + extra;
                    Vector3 p = bt.TransformPoint(dir * out_); p.y = bt.position.y - 1.5f;
                    if (!Physics.Raycast(p, Vector3.down, out RaycastHit hit, 40f, ~0, QueryTriggerInteraction.Ignore)) continue;
                    if (hit.point.y < -0.3f || Water(hit.collider.transform)) continue;   // agua o fondo del mar
                    spot = hit.point;
                }
            if (spot == null)
            {
                Vector3 q = bt.position; q.y -= 1.5f;
                string seen = Physics.Raycast(q, Vector3.down, out RaycastHit h0, 40f, ~0, QueryTriggerInteraction.Ignore) ? $"{h0.collider.name} a {h0.point.y:0.0} m" : "nada";
                Debug.LogWarning($"[Transporte] Sin hielo firme cerca de «{bname}» para sus alfombras del suelo (debajo: {seen}).");
                continue;
            }
            CarpetPad.BuildZone(bt, spot.Value + Vector3.up * 0.05f, bt.rotation, CarpetPad.Kind.Edificio, bname, 0, $"{bname}#0", "Suelo (hielo)", 2);
            placed++;
        }
        Debug.Log($"[Transporte] {placed} áreas de alfombras en el hielo, al pie de cada edificio.");
    }

    static bool Water(Transform t)
    {
        if (t != null)   // solo el propio objeto: sus padres («SeaAndFog», la banquisa…) daban falsos positivos
        {
            string n = t.name.ToLowerInvariant();
            if (n.Contains("agua") || n.Contains("water") || n.Contains("sea") || n.Contains("mar_") || n.Contains("ocean")) return true;
        }
        return false;
    }

    // Ya no se tienden puentes: no hay llegadas que despejar.
    static void ClearBridgeLandings() => _landings.Clear();
}
