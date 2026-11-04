using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// EL SUELO DE LOS MICROCOSMOS: LA FLORA ENCIMA, NUNCA DEBAJO, Y LA CAPA DE TIERRA (docs/microcosmos-suelo-y-elementos.md; Berón
/// 2026-11-04: «en el microcosmos de Ambrosio he visto que la vegetación se está creando bajo el suelo … una de las reglas debería
/// ser que la vegetación se cree encima de cada superficie y nunca debajo … colocar una capa de superficie que tenga una altura de
/// 10 cm … un suelo de tierra, el cual tiene sus irregularidades, cúmulos y piedras»).
/// - POR QUÉ SALÍA BAJO EL SUELO: el suelo «Mat/Suelo» del área gigante se baja 2 cm del piso real (2 m a ×100) y el suelo del
///   microcosmos (groundY) es el MÁS BAJO; la flora se ponía a la altura del suelo «más cercano a groundY» (FloorNear), que es ese suelo
///   bajado, aunque encima hubiera otra losa (el pasillo y las salas del hospital, una alfombra): la planta quedaba debajo.
/// - LA REGLA: cada planta se apoya en la superficie MÁS ALTA que hay encima de su pie (<see cref="TopSurface"/>: un rayo desde
///   arriba, acotado a <see cref="Probe"/> m, contra los colliders del microcosmos y las caras de arriba de sus suelos) y ninguna
///   está dentro de un collider. <see cref="ValidateFlora"/> lo comprueba (regla 7 de <see cref="MicroFloors.Validate"/>).
/// - LA CAPA DE TIERRA (<see cref="MicroSoil"/>): <see cref="SoilThickness"/> (10 cm reales: el microcosmos mide en metros de
///   insecto-humano) sobre cada suelo que usa el microcosmos, siguiendo la cara de arriba más alta (así cubre alfombras y losas), con
///   ruido, cúmulos y piedras, y su collider. La historia y su pueblo se apoyan en ella.
/// </summary>
public static class MicroGround
{
    /// <summary>Grosor de la capa de tierra (m del microcosmos: los insectos-humanos miden como una persona, así que son 10 cm de
    /// verdad). Bajo los cúmulos es más gruesa; nunca menos de la mitad.</summary>
    public const float SoilThickness = 0.10f;
    /// <summary>(2026-11-04) ¿Se pone la CAPA DE TIERRA? APAGADA por defecto (Berón: «el suelo de tierra se puede desactivar si es tan
    /// pesado»): la Cocina colgaba Unity al hornear el NavMesh. Con false no se construye, no se sube la historia y no se exige (regla 8).</summary>
    public static bool SoilEnabled = false;
    /// <summary>Nombre del grupo de la capa de tierra bajo la raíz del microcosmos.</summary>
    public const string SoilName = "Suelo_de_tierra";
    /// <summary>Hasta cuántos metros por encima del pie de una planta se busca la superficie que la cubre (una alfombra o una losa
    /// del piso ×100 miden 1–6 m; un mueble de verdad, decenas: esos ya los esquiva quien siembra).</summary>
    public const float Probe = 6f;
    /// <summary>Tolerancia (m): un pie a menos de esto bajo su superficie está apoyado.</summary>
    public const float Tolerance = 0.05f;

    static readonly AffordanceKind[] PlantKinds = { AffordanceKind.Grass, AffordanceKind.PlantSap, AffordanceKind.Leaf, AffordanceKind.Fungus, AffordanceKind.Flower, AffordanceKind.Berry, AffordanceKind.Willow, AffordanceKind.Lichen };

    /// <summary>¿Es <paramref name="t"/> la raíz de una planta? Los hijos de «Flora_a_escala», los frutales y plantas de los
    /// edificios (MicroPiece «frutal»/«planta») y lo que lleva un Affordance vegetal sin otra planta por encima.</summary>
    public static bool IsPlantRoot(Transform t)
    {
        if (t == null) return false;
        if (t.parent != null && t.parent.name == "Flora_a_escala") return true;
        MicroPiece mp = t.GetComponent<MicroPiece>();
        if (mp != null && (mp.kind == "frutal" || mp.kind == "planta")) return true;
        foreach (Affordance a in t.GetComponents<Affordance>())
            if (System.Array.IndexOf(PlantKinds, a.kind) >= 0)
            {
                for (Transform p = t.parent; p != null; p = p.parent) if (IsPlantRoot(p)) return false;
                return true;
            }
        return false;
    }

    /// <summary>Las plantas del microcosmos <paramref name="root"/> (sin repetir las anidadas).</summary>
    public static List<Transform> Plants(Transform root)
    {
        var l = new List<Transform>();
        if (root == null) return l;
        Transform flora = root.Find("Flora_a_escala");
        if (flora != null) foreach (Transform c in flora) l.Add(c);
        Transform hist = root.Find("Historia");
        if (hist != null)
        {
            var seen = new HashSet<Transform>();   // (rendimiento: antes List.Contains sobre miles de plantas por cada Affordance)
            foreach (MicroPiece mp in hist.GetComponentsInChildren<MicroPiece>(true)) if ((mp.kind == "frutal" || mp.kind == "planta") && seen.Add(mp.transform)) l.Add(mp.transform);
            foreach (Affordance a in hist.GetComponentsInChildren<Affordance>(true))
                if (!seen.Contains(a.transform) && IsPlantRoot(a.transform) && seen.Add(a.transform)) l.Add(a.transform);
        }
        return l;
    }

    /// <summary>¿No cuenta como superficie ni como obstáculo? Las plantas (una planta no se apoya en otra), los bichos y la gente,
    /// los disparadores y la tapa invisible de los muros.</summary>
    public static bool Ignored(Collider c, Transform self = null)
    {
        if (c == null || c.isTrigger) return true;
        Transform t = c.transform;
        if (self != null && t.IsChildOf(self)) return true;
        if (t.name == "Tapa_invisible") return true;
        for (Transform p = t; p != null; p = p.parent)
        {
            if (p.name == "Flora_a_escala" || p.name == "Fauna_a_escala") return true;
            if (p.GetComponent<UnityEngine.AI.NavMeshAgent>() != null || p.GetComponent<CubeInsect>() != null || p.GetComponent<MicroSize>() != null) return true;
            MicroPiece mp = p.GetComponent<MicroPiece>();
            if (mp != null && (mp.kind == "frutal" || mp.kind == "planta")) return true;
        }
        return false;
    }

    /// <summary>
    /// LA SUPERFICIE MÁS ALTA bajo el punto <paramref name="p"/> (x, z) entre <paramref name="p"/>.y − 2 m y <paramref name="p"/>.y +
    /// <paramref name="above"/>: un rayo desde arriba contra los colliders de <paramref name="root"/> (no los de fuera: mientras se
    /// construye, el microcosmos está en el origen, encima del santuario) y, si se dan, las caras de arriba de <paramref name="surfaces"/>
    /// (suelos sin collider: alfombras pintadas). false si no hay ninguna.
    /// </summary>
    public static bool TopSurface(Transform root, Vector3 p, float above, out float y, out Collider hit, Transform self = null, IList<Bounds> surfaces = null)
    {
        y = float.MinValue; hit = null;
        Vector3 from = new Vector3(p.x, p.y + above, p.z);
        float len = above + 2f;
        foreach (RaycastHit h in Physics.RaycastAll(from, Vector3.down, len, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.normal.y < 0.3f || Ignored(h.collider, self)) continue;
            if (root != null && !h.transform.IsChildOf(root)) continue;
            // Solo lo que TAPA el pie (una capa apoyada a su altura o por debajo: la tierra, una alfombra, una losa), no una balda
            // que cuelga por encima (debajo de ella la planta no está enterrada).
            if (!IsSoil(h.collider) && h.collider.bounds.min.y > p.y + 0.5f) continue;
            if (h.point.y > y) { y = h.point.y; hit = h.collider; }
        }
        if (surfaces != null)
            foreach (Bounds b in surfaces)
                if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z && b.max.y <= from.y && b.max.y >= p.y - 2f && b.min.y <= p.y + 0.5f && b.max.y > y) { y = b.max.y; hit = null; }
        return y > float.MinValue;
    }

    /// <summary>¿Es <paramref name="c"/> un mosaico o una placa de la capa de tierra?</summary>
    public static bool IsSoil(Collider c) => c != null && c.transform.parent != null && c.transform.parent.name == SoilName;

    /// <summary>¿Está el pie de <paramref name="plant"/> DENTRO de un collider (un mueble, un edificio, una roca)? Una esfera pequeña
    /// a 0,25 m sobre su pie (no toca la tierra en la que se apoya). Devuelve el collider o null.</summary>
    public static Collider Inside(Transform root, Transform plant)
    {
        Vector3 c = plant.position + Vector3.up * 0.25f;
        foreach (Collider k in Physics.OverlapSphere(c, 0.12f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (Ignored(k, plant)) continue;
            if (root != null && !k.transform.IsChildOf(root)) continue;
            if (IsSoil(k)) continue;   // la tierra (un cúmulo) no la contiene: se apoya
            return k;
        }
        return null;
    }

    /// <summary>
    /// LA REGLA DE LA FLORA (regla 7 de los microcosmos): ninguna planta tiene su pie por debajo de la superficie sobre la que está
    /// (con <see cref="Tolerance"/>) y ninguna está dentro de un collider. Devuelve las violaciones (como mucho <paramref name="max"/>
    /// con detalle; luego el total).
    /// </summary>
    public static List<string> ValidateFlora(Transform root, int max = 12, IList<Bounds> surfaces = null)
    {
        var bad = new List<string>();
        if (root == null) return bad;
        Physics.SyncTransforms();
        int buried = 0, inside = 0;
        foreach (Transform pl in Plants(root))
        {
            if (pl == null || !pl.gameObject.activeInHierarchy) continue;
            Vector3 b = pl.position;
            if (TopSurface(root, b, Probe, out float top, out Collider on, pl, surfaces) && top > b.y + Tolerance)
            {
                if (buried++ < max) bad.Add($"planta «{pl.name}» enterrada: su pie a {b.y:0.00} m y la superficie de encima ({(on != null ? on.name : "suelo")}) a {top:0.00} m");
                continue;
            }
            Collider k = Inside(root, pl);
            if (k != null && inside++ < max) bad.Add($"planta «{pl.name}» dentro de «{k.name}»");
        }
        if (buried > max) bad.Add($"… {buried} plantas enterradas en total");
        if (inside > max) bad.Add($"… {inside} plantas dentro de un collider en total");
        return bad;
    }


    // ── EN LOTE (2026-11-04, rendimiento): un RaycastCommand y un OverlapSphereCommand por pieza, todos a la vez (antes, un
    //    Physics.RaycastAll y un OverlapSphere por planta: 10–20 mil consultas una a una), y lo que no cuenta, en caché por collider. ──
    const int MaxHits = 16;

    // Lo que no cuenta (las plantas, los bichos, la gente, la tapa), sin mirar quién pregunta: en caché por collider.
    static bool IgnoredCached(Collider c, Dictionary<Collider, bool> cache)
    {
        if (c == null) return true;
        if (!cache.TryGetValue(c, out bool v)) cache[c] = v = Ignored(c);
        return v;
    }

    /// <summary>Un rayo hacia abajo por pieza (desde <paramref name="from"/>(i), <paramref name="length"/>(i) m) en lote; por pieza, el
    /// punto más alto que <paramref name="accept"/> admite (float.MinValue si ninguno). Si un rayo llena sus <see cref="MaxHits"/>
    /// impactos, esa pieza se mira aparte con RaycastAll (no se pierde la cara de arriba).</summary>
    static float[] BatchTops(Transform root, IList<Transform> items, System.Func<int, Vector3> from, System.Func<int, float> length, System.Func<int, Collider, bool> accept)
    {
        int n = items.Count; var tops = new float[n];
        for (int i = 0; i < n; i++) tops[i] = float.MinValue;
        if (n == 0) return tops;
        var cmds = new NativeArray<RaycastCommand>(n, Allocator.TempJob);
        var hits = new NativeArray<RaycastHit>(n * MaxHits, Allocator.TempJob);
        try
        {
            var qp = new QueryParameters(~0, false, QueryTriggerInteraction.Ignore, false);
            for (int i = 0; i < n; i++) cmds[i] = new RaycastCommand(items[i] != null ? from(i) : Vector3.zero, Vector3.down, qp, items[i] != null ? length(i) : 0f);
            RaycastCommand.ScheduleBatch(cmds, hits, 32, MaxHits).Complete();
            for (int i = 0; i < n; i++)
            {
                if (items[i] == null) continue;
                int count = 0;
                for (int k = 0; k < MaxHits; k++)
                {
                    RaycastHit h = hits[i * MaxHits + k];
                    Collider c = h.collider;
                    if (c == null) break;
                    count++;
                    if (h.normal.y < 0.3f || !h.transform.IsChildOf(root) || !accept(i, c)) continue;
                    if (h.point.y > tops[i]) tops[i] = h.point.y;
                }
                if (count < MaxHits) continue;
                tops[i] = float.MinValue;   // lleno: se mira aparte
                foreach (RaycastHit h in Physics.RaycastAll(from(i), Vector3.down, length(i), ~0, QueryTriggerInteraction.Ignore))
                    if (h.normal.y >= 0.3f && h.transform.IsChildOf(root) && accept(i, h.collider) && h.point.y > tops[i]) tops[i] = h.point.y;
            }
        }
        finally { cmds.Dispose(); hits.Dispose(); }
        return tops;
    }

    // ¿Cuál pieza queda DENTRO de un collider (una esfera de 0,12 m a 0,25 m sobre su pie), en lote?
    static bool[] BatchInside(Transform root, IList<Transform> items, Dictionary<Collider, bool> cache)
    {
        int n = items.Count; var inside = new bool[n];
        if (n == 0) return inside;
        var cmds = new NativeArray<OverlapSphereCommand>(n, Allocator.TempJob);
        var hits = new NativeArray<ColliderHit>(n * MaxHits, Allocator.TempJob);
        try
        {
            var qp = new QueryParameters(~0, false, QueryTriggerInteraction.Ignore, false);
            for (int i = 0; i < n; i++) cmds[i] = new OverlapSphereCommand(items[i] != null ? items[i].position + Vector3.up * 0.25f : Vector3.down * 1e6f, items[i] != null ? 0.12f : 0.001f, qp);
            OverlapSphereCommand.ScheduleBatch(cmds, hits, 32, MaxHits).Complete();
            for (int i = 0; i < n; i++)
            {
                if (items[i] == null) continue;
                for (int k = 0; k < MaxHits; k++)
                {
                    Collider c = hits[i * MaxHits + k].collider;
                    if (c == null) break;
                    if (IgnoredCached(c, cache) || c.transform.IsChildOf(items[i]) || !c.transform.IsChildOf(root) || IsSoil(c)) continue;
                    inside[i] = true; break;
                }
            }
        }
        finally { cmds.Dispose(); hits.Dispose(); }
        return inside;
    }

    /// <summary>
    /// (2026-11-04) CAER DESDE EL AIRE (Berón: «un truco que conozco es crear todos los objetos en el aire para que caigan sobre la
    /// superficie que tenga debajo»): cada pieza de <paramref name="items"/> se suelta desde <paramref name="fromY"/> (bajo la tapa de los
    /// muros) y queda sobre la superficie MÁS ALTA de debajo de su (x, z) entre los colliders de <paramref name="root"/> (sin plantas,
    /// bichos ni gente), las caras de <paramref name="surfaces"/> y la tierra que se ve (<paramref name="soilAt"/>). Para lo que se
    /// coloca a mano (el mapa de cada historia): ahí donde cae es donde el autor lo quiso, también sobre un mueble. Devuelve cuántas.
    /// </summary>
    public static int Drop(Transform root, IList<Transform> items, float fromY, IList<Bounds> surfaces = null, System.Func<float, float, float> soilAt = null)
    {
        if (root == null || items == null) return 0;
        Physics.SyncTransforms();
        var cache = new Dictionary<Collider, bool>();
        var list = new List<Transform>(items);
        float[] tops = BatchTops(root, list, i => new Vector3(list[i].position.x, fromY, list[i].position.z), i => fromY - list[i].position.y + 50f,
                                 (i, c) => !IgnoredCached(c, cache) && !c.transform.IsChildOf(list[i]));
        int n = 0;
        for (int i = 0; i < list.Count; i++)
        {
            Transform it = list[i]; if (it == null) continue;
            Vector3 p = it.position; float top = tops[i];
            if (surfaces != null) foreach (Bounds b in surfaces) if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z && b.max.y < fromY && b.max.y > top) top = b.max.y;
            float s = soilAt != null ? soilAt(p.x, p.z) : float.NaN;
            if (!float.IsNaN(s) && s > top) top = s;
            if (top == float.MinValue) continue;
            it.position = new Vector3(p.x, top, p.z);
            n++;
        }
        return n;
    }

    /// <summary>
    /// APOYA cada planta (y cada bicho de «Fauna_a_escala») en la superficie más alta de encima de su pie y QUITA las que quedan
    /// dentro de un collider. Lo usa el constructor del microcosmos después de sembrar (también vale en juego). Devuelve
    /// (apoyadas, quitadas). Las consultas van en lote (<see cref="BatchTops"/>, <see cref="BatchInside"/>).
    /// </summary>
    public static (int settled, int removed) Settle(Transform root, IList<Bounds> surfaces = null, System.Func<float, float, float> soilAt = null)
    {
        int settled = 0, removed = 0;
        if (root == null) return (0, 0);
        Physics.SyncTransforms();
        var plants = Plants(root);
        Transform fauna = root.Find("Fauna_a_escala");
        if (fauna != null) foreach (Transform c in fauna) plants.Add(c);
        var cache = new Dictionary<Collider, bool>();
        var bases = new Vector3[plants.Count];
        for (int i = 0; i < plants.Count; i++) bases[i] = plants[i] != null ? plants[i].position : Vector3.zero;
        // Como TopSurface: solo lo que TAPA el pie (la tierra, una capa apoyada a su altura o por debajo), no una balda por encima.
        float[] tops = BatchTops(root, plants, i => bases[i] + Vector3.up * Probe, i => Probe + 2f,
                                 (i, c) => !IgnoredCached(c, cache) && !c.transform.IsChildOf(plants[i]) && (IsSoil(c) || c.bounds.min.y <= bases[i].y + 0.5f));
        for (int i = 0; i < plants.Count; i++)
        {
            Transform pl = plants[i]; if (pl == null) continue;
            Vector3 b = bases[i]; float top = tops[i]; bool found = top > float.MinValue;
            if (surfaces != null)
                foreach (Bounds sb in surfaces)
                    if (b.x >= sb.min.x && b.x <= sb.max.x && b.z >= sb.min.z && b.z <= sb.max.z && sb.max.y <= b.y + Probe && sb.max.y >= b.y - 2f && sb.min.y <= b.y + 0.5f && sb.max.y > top) { top = sb.max.y; found = true; }
            // (2026-11-04) La tierra que SE VE (con su ruido y sus cúmulos): su collider es más liso y va por debajo (MicroSoilBuilder).
            float s = soilAt != null ? soilAt(b.x, b.z) : float.NaN;
            if (!float.IsNaN(s) && s >= b.y - 2f && s <= b.y + Probe) { top = found ? Mathf.Max(top, s) : s; found = true; }
            if (found && Mathf.Abs(top - b.y) > 0.005f) { pl.position = new Vector3(b.x, top, b.z); settled++; }
        }
        Physics.SyncTransforms();
        var flora = new List<Transform>(); foreach (Transform pl in plants) if (pl != null && (pl.parent == null || pl.parent.name != "Fauna_a_escala")) flora.Add(pl);
        bool[] inside = BatchInside(root, flora, cache);
        for (int i = 0; i < flora.Count; i++)
        {
            if (!inside[i] || flora[i] == null) continue;
            if (Application.isPlaying) Object.Destroy(flora[i].gameObject); else Object.DestroyImmediate(flora[i].gameObject);
            removed++;
        }
        return (settled, removed);
    }
}

/// <summary>
/// LA CAPA DE TIERRA de un microcosmos (<see cref="MicroGround"/>): la pone <c>MicroSoilBuilder</c> bajo la raíz («Suelo_de_tierra»),
/// en mosaicos (una malla combinada por mosaico, con sus piedras dentro, y su MeshCollider) y en placas sobre las caras de arriba de
/// los muebles. Lo leen las pruebas.
/// </summary>
public class MicroSoil : MonoBehaviour
{
    [Tooltip("Grosor de la capa (m del microcosmos).")]
    public float thickness = MicroGround.SoilThickness;
    [Tooltip("Mosaicos de tierra sobre el suelo del piso y placas sobre los muebles (una malla combinada cada uno).")]
    public int tiles, plates;
    [Tooltip("Colliders LISOS de la tierra (sin ruido ni piedras: los hornea el NavMesh).")]
    public int colliders;
    [Tooltip("Cúmulos y piedras repartidos.")]
    public int mounds, stones;
    [Tooltip("Edificios y tramos de calle del pueblo levantados para quedar sobre la tierra.")]
    public int raised;
}
