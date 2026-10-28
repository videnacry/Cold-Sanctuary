using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// POSICIONES RESERVADAS DE CADA PISO (docs/reglas-plataformas.md, regla 8; Berón 2026-10-28): al final de la blockout, con
/// los muebles y las áreas de alfombras ya puestos (<see cref="PlaceCarpetAreas"/>), cada piso apunta en su
/// <see cref="PlatformSlots"/> dónde irán la HISTORIA, los PERSONAJES, la FAUNA y la FLORA de su microcosmos
/// (<c>AreaMicrocosmos</c> las usa). Rejilla de 1 m sobre el rectángulo del piso; una celda está LIBRE si queda a ≥ 1 m de
/// todo lo que se alza en ese piso (muebles, paredes, máquinas), a ≥ 1 m de las áreas de alfombras y a ≥ 0,8 m del borde.
/// - Historia: la celda libre más alejada de los muebles (como el «hueco más libre» del microcosmos) y hasta 3 huecos
///   grandes más, a ≥ 5 m entre sí.
/// - Personajes: hasta 8, a 3–8 m de la historia (si no caben 4, de 2 a 12 m), a ≥ 2 m entre sí.
/// - Fauna: hasta 12, las más pegadas a los bordes y las esquinas, a ≥ 3 m entre sí.
/// - Flora: lo libre que queda, cada 2 m (hasta 60, repartidas por todo el piso).
/// </summary>
public static partial class SampleSceneBuilder
{
    const float SlotCell = 1f, SlotFurnitureClear = 1f, SlotCarpetClear = 1f, SlotEdge = 0.8f;
    const int MaxStorySlots = 4, MaxCharacterSlots = 8, MaxFaunaSlots = 12, MaxFloraSlots = 60;

    /// <summary>Calcula las plazas de cada piso (las áreas y sus pisos de arriba; no los techos de alfombras). <paramref name="all"/>:
    /// las cajas de todo lo que se alza en la escena (las de <see cref="PlaceCarpetAreas"/>, con los acabados de las obras).</summary>
    static void PlaceReservedSlots(List<Bounds> all)
    {
        var floors = new List<Transform>();
        foreach (Transform t in _platforms.Keys)
            if (t != null && !t.name.StartsWith("Techo_alfombras_") && !t.name.StartsWith("Anden_alfombras_") && !floors.Contains(t)) floors.Add(t);
        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
            if (!floors.Contains(a.transform)) floors.Add(a.transform);
        // Las áreas de alfombras (su zona marcada): ahí se posan y se sube; nada reservado encima.
        var pads = new List<Bounds>();
        foreach (CarpetPad p in Object.FindObjectsByType<CarpetPad>(FindObjectsSortMode.None))
        {
            BoxCollider bc = p.GetComponent<BoxCollider>();
            if (bc == null) continue;
            Vector3 e = p.transform.rotation * bc.size;
            pads.Add(new Bounds(p.transform.TransformPoint(bc.center), new Vector3(Mathf.Abs(e.x), Mathf.Abs(e.y), Mathf.Abs(e.z))));
        }
        int few = 0;
        foreach (Transform f in floors)
        {
            Vector2 size = _platforms.TryGetValue(f, out Vector2 s) ? s : FloorBox(f);
            PlatformSlots ps = f.GetComponent<PlatformSlots>();
            if (ps == null) ps = f.gameObject.AddComponent<PlatformSlots>();
            ComputeSlots(f, size, all, pads, ps);
            SanctuaryArea sa = f.GetComponent<SanctuaryArea>();
            string name = sa != null ? sa.displayName : f.name;
            bool ok = ps.story.Count >= PlatformSlots.MinStory && ps.characters.Count >= PlatformSlots.MinCharacters
                   && ps.fauna.Count >= PlatformSlots.MinFauna && ps.flora.Count >= PlatformSlots.MinFlora;
            string line = $"[Plazas] {name}: historia {ps.story.Count}, personajes {ps.characters.Count}, fauna {ps.fauna.Count}, flora {ps.flora.Count}";
            if (ok || sa == null) Debug.Log(line);
            else { Debug.LogWarning(line + " — no cabe la historia de su microcosmos: amplía el piso o aparta muebles (regla 8)."); few++; }
        }
        Debug.Log($"[Plazas] {floors.Count} pisos con sus posiciones reservadas para el microcosmos ({few} áreas sin sitio suficiente).");
    }

    // Sin tamaño apuntado (escenas de prueba): el de su suelo («Mat»), o 12 × 12.
    static Vector2 FloorBox(Transform f)
    {
        Transform mat = f.Find("Mat");
        return mat != null ? new Vector2(Mathf.Abs(mat.localScale.x), Mathf.Abs(mat.localScale.z)) : new Vector2(12f, 12f);
    }

    // La cara de arriba del SUELO del piso: el plano grande más alto cerca de su origen (el «Mat» va algo por encima).
    static float FloorTop(Transform f)
    {
        float y = float.MinValue;
        foreach (Renderer r in f.GetComponentsInChildren<Renderer>())
        {
            Bounds b = r.bounds;
            if (b.size.y > 0.5f || Mathf.Max(b.size.x, b.size.z) < 3f) continue;
            if (b.max.y < f.position.y - 0.4f || b.max.y > f.position.y + 0.6f) continue;
            y = Mathf.Max(y, b.max.y);
        }
        return y == float.MinValue ? f.position.y : y;
    }

    static float DistXZ(Bounds b, Vector3 p) => FurnitureSpacing.DistXZ(b, p);

    static void ComputeSlots(Transform f, Vector2 size, List<Bounds> all, List<Bounds> pads, PlatformSlots ps)
    {
        ps.story.Clear(); ps.characters.Clear(); ps.fauna.Clear(); ps.flora.Clear();
        ps.floorSize = size; ps.storyClearance = 0f;
        float fy = FloorTop(f), hx = size.x * 0.5f, hz = size.y * 0.5f;
        // Lo que se alza EN ESTE PISO (no el suelo, ni lo de los pisos de arriba o de abajo), cerca de su huella.
        Bounds foot = new Bounds(new Vector3(f.position.x, fy + 1f, f.position.z), new Vector3(size.x, 2f, size.y));
        if (Mathf.Abs(Mathf.DeltaAngle(f.eulerAngles.y, 0f)) > 1f) foot.Expand(new Vector3(size.y, 0f, size.x));
        foot.Expand(new Vector3(4f, 0f, 4f));
        var near = new List<Bounds>();
        // (los suelos grandes y planos ya no están en «all»; las bases bajas de las máquinas, de 8 cm, sí cuentan)
        foreach (Bounds b in all)
            if (b.max.y > fy + 0.03f && b.min.y < fy + 2.2f && b.max.x > foot.min.x && b.min.x < foot.max.x && b.max.z > foot.min.z && b.min.z < foot.max.z) near.Add(b);
        // Y cada MUEBLE entero de este piso (la caja que mide FurnitureSpacing, la que mira la prueba): la pluma de una grúa
        // por encima de 2 m también lo aparta.
        foreach (var (_, b) in FurnitureSpacing.Items(f)) if (Mathf.Abs(b.min.y - fy) < 1.5f) near.Add(b);
        var nearPads = new List<Bounds>();
        foreach (Bounds b in pads)
            if (Mathf.Abs(b.min.y - fy) < 1.5f && b.max.x > foot.min.x && b.min.x < foot.max.x && b.max.z > foot.min.z && b.min.z < foot.max.z) nearPads.Add(b);

        // Las celdas libres (locales) y su holgura (distancia al mueble o al borde más cercano).
        var cells = new List<(Vector3 local, Vector3 world, float clear, float edge)>();
        float y = f.InverseTransformPoint(new Vector3(f.position.x, fy, f.position.z)).y;
        for (float x = -hx + SlotEdge; x <= hx - SlotEdge + 0.01f; x += SlotCell)
            for (float z = -hz + SlotEdge; z <= hz - SlotEdge + 0.01f; z += SlotCell)
            {
                Vector3 l = new Vector3(x, y, z), w = f.TransformPoint(l);
                float edge = Mathf.Min(hx - Mathf.Abs(x), hz - Mathf.Abs(z)), clear = edge;
                foreach (Bounds b in near) { clear = Mathf.Min(clear, DistXZ(b, w)); if (clear < SlotFurnitureClear) break; }
                if (clear < SlotFurnitureClear) continue;
                bool onPad = false;
                foreach (Bounds b in nearPads) if (DistXZ(b, w) < SlotCarpetClear) { onPad = true; break; }
                if (onPad) continue;
                cells.Add((l, w, clear, edge));
            }
        ps.freeCells = cells.Count;
        if (cells.Count == 0) return;
        bool Far(Vector3 p, List<Vector3> from, float d) { foreach (Vector3 q in from) if ((p - q).sqrMagnitude < d * d) return false; return true; }
        bool Free(Vector3 p) => Far(p, ps.story, 1f) && Far(p, ps.characters, 1f) && Far(p, ps.fauna, 1f) && Far(p, ps.flora, 1f);
        float D2(Vector3 a, Vector3 b) => new Vector2(a.x - b.x, a.z - b.z).magnitude;

        var byClear = new List<(Vector3 local, Vector3 world, float clear, float edge)>(cells);
        byClear.Sort((a, b) => b.clear.CompareTo(a.clear));
        var byEdge = new List<(Vector3 local, Vector3 world, float clear, float edge)>(cells);
        float Corner(Vector3 l) => (hx - Mathf.Abs(l.x)) + (hz - Mathf.Abs(l.z));
        byEdge.Sort((a, b) => { int k = a.edge.CompareTo(b.edge); return k != 0 ? k : Corner(a.local).CompareTo(Corner(b.local)); });

        // HISTORIA: la celda con más holgura (el hueco más libre).
        Vector3 story = byClear[0].local; ps.storyClearance = byClear[0].clear;
        ps.story.Add(story);

        // PERSONAJES: alrededor de la historia, a 3–8 m (si no caben, de 2 a 12 m), con holgura primero.
        void Characters(int upTo)
        {
            foreach (var (r0, r1) in new[] { (3f, 8f), (2f, 12f) })
            {
                foreach (var c in byClear)
                {
                    if (ps.characters.Count >= upTo) return;
                    float d = D2(c.local, story);
                    if (d >= r0 && d <= r1 && Far(c.local, ps.characters, 2f) && Free(c.local)) ps.characters.Add(c.local);
                }
                if (ps.characters.Count >= PlatformSlots.MinCharacters) return;
            }
        }
        // FAUNA: junto a los bordes y las esquinas (las esquinas primero: dos bordes cerca).
        void Fauna(int upTo)
        {
            foreach (var c in byEdge)
            {
                if (ps.fauna.Count >= upTo) return;
                if (Far(c.local, ps.fauna, 2.5f) && Free(c.local)) ps.fauna.Add(c.local);
            }
        }
        // FLORA: lo libre que queda, cada 2 m (no encima de la historia: su claro), repartida por todo el piso.
        void Flora(int upTo, float gap = 2f, float claro = 2.5f)
        {
            var more = new List<Vector3>();
            foreach (var c in cells)
                if (D2(c.local, story) >= claro && Far(c.local, ps.flora, gap) && Far(c.local, more, gap) && Free(c.local)) more.Add(c.local);
            int room = upTo - ps.flora.Count;
            if (more.Count <= room) ps.flora.AddRange(more);
            else { float step = more.Count / (float)room; for (int i = 0; i < room; i++) ps.flora.Add(more[Mathf.Min(more.Count - 1, Mathf.FloorToInt(i * step))]); }
        }

        // Primero el MÍNIMO de cada tipo (en un piso justo, que quepan todos), luego hasta el máximo.
        Characters(PlatformSlots.MinCharacters); Fauna(PlatformSlots.MinFauna); Flora(PlatformSlots.MinFlora);
        if (ps.flora.Count < PlatformSlots.MinFlora) Flora(PlatformSlots.MinFlora, 1.5f, 2f);   // piso justo: más juntas (a ×10, 15 m) y hasta el claro de la historia (20 m)
        foreach (var c in byClear)   // otros huecos grandes para estructuras de la historia, separados
        {
            if (ps.story.Count >= MaxStorySlots) break;
            if (c.clear >= 2f && Far(c.local, ps.story, 5f) && Free(c.local)) ps.story.Add(c.local);
        }
        Characters(MaxCharacterSlots); Fauna(MaxFaunaSlots); Flora(MaxFloraSlots);
    }
}
