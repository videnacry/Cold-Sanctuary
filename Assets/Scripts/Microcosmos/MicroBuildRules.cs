using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS REGLAS DE EDIFICACIÓN DE LOS MICROCOSMOS Y MESOCOSMOS (docs/microcosmos-reglas-edificacion.md; Berón 2026-11-01: «en el
/// microcosmos de Bruce Lee la cocina y los muebles parecían cubos»). Cinco reglas, con números a la escala de los
/// insectos-humanos (el agente de navegación de <c>AreaMicrocosmos.Folk</c>: radio 0,4 m, cápsula de 0,35 m):
/// 1. PASO: entre dos muebles cualesquiera, <see cref="Passage"/> (2 m) libres: dos personajes se cruzan sin chocar.
/// 2. SIN MUROS NI TECHOS en los edificios (tapan la vista y el paso), como en el santuario.
/// 3. MUEBLES DETALLADOS (<see cref="MicroPiece"/>, varias piezas) con sus utensilios (sartén, olla, cuchillo…): nada de cubos lisos.
/// 4. NECESIDADES BÁSICAS: camas, retretes, lavabos, mesas, sillas y cocina (<see cref="Needs"/>).
/// 5. ERAS AVANZADAS (<see cref="IsTownEra"/>): el pueblo o la ciudad real ocupa TODO el piso (sin los muebles gigantes del área).
/// <see cref="Validate"/> comprueba 1–4 en una escena o en un subárbol y devuelve (y registra) las violaciones.
/// </summary>
public static class MicroBuildRules
{
    /// <summary>Radio del NavMeshAgent de un insecto-humano (AreaMicrocosmos.Folk: 0,4 m × bulk 1).</summary>
    public const float AgentRadius = 0.4f;
    /// <summary>Holgura a cada lado del par que se cruza (la evitación del agente y el giro del cuerpo).</summary>
    public const float SideMargin = 0.2f;
    /// <summary>PASO LIBRE entre dos muebles: dos agentes hombro con hombro (4 × 0,4 = 1,6 m) + 0,2 m a cada lado = 2,0 m.
    /// Es el mismo estándar del santuario (<see cref="FurnitureSpacing.Standard"/>).</summary>
    public const float Passage = 4f * AgentRadius + 2f * SideMargin;
    /// <summary>Las calles de un pueblo (dos pasos: se cruzan dos parejas, o un carro y una persona).</summary>
    public const float Street = 2f * Passage;
    /// <summary>Delante de cada puesto de trabajo, el sitio donde se pone el trabajador (fondo libre).</summary>
    public const float WorkSpot = 0.9f;
    /// <summary>Alto a partir del cual una pieza tapa la vista de un insecto-humano (ojos a ~1,6 m de 1,7 m de alto): un muro.</summary>
    public const float WallHeight = 1.2f;
    /// <summary>Alto desde el que una losa por encima de las cabezas es un techo.</summary>
    public const float RoofHeight = 2.2f;

    /// <summary>Desde qué año un microcosmos es de ERA AVANZADA (las primeras ciudades: Uruk, ~3500 a. C.): el pueblo o la ciudad
    /// real ocupa el piso entero. Antes, las aldeas (Huerto, ~9500 a. C.) conviven con los muebles gigantes del área.</summary>
    public const int TownFrom = -3500;
    public static bool IsTownEra(int year) => year >= TownFrom;

    /// <summary>Las necesidades básicas que todo pueblo cubre (MicroPiece.need).</summary>
    public static readonly string[] Needs = { "dormir", "aseo", "lavarse", "comer", "sentarse", "cocinar" };

    /// <summary>Nombres de piezas que son muros o techos de edificio (los del borde del piso, «Muros_del_piso», no cuentan).</summary>
    public static readonly string[] WallRoofNames = { "Muro_", "Muro ", "Cubierta", "Faldon", "Dintel", "Techo", "Tejado", "Chapitel" };

    /// <summary>MicroPiece.kind de una BARRERA HISTÓRICA que es la propia historia (el muro del gueto de Varsovia, cuyas salidas
    /// son los pasos de la misión): la única excepción a la regla 2; no cuenta como muro ni para el paso.</summary>
    public const string Historic = "historico";

    /// <summary>Lo que se mide: los muebles (<see cref="MicroPiece"/> de más fuera, no pisables) y los puestos
    /// (<see cref="AreaFunctionStation"/>) que no son parte de un mueble.</summary>
    public static List<(Transform t, Bounds b)> Pieces(Transform scope)
    {
        var l = new List<(Transform, Bounds)>();
        foreach (MicroPiece p in scope.GetComponentsInChildren<MicroPiece>(true))
        {
            if (p.walkable || HasPieceAbove(p.transform, scope)) continue;
            if (Box(p.transform, out Bounds b)) l.Add((p.transform, b));
        }
        foreach (AreaFunctionStation s in scope.GetComponentsInChildren<AreaFunctionStation>(true))
        {
            if (s.GetComponentInParent<MicroPiece>(true) != null) continue;
            if (Box(s.transform, out Bounds b) && b.size.y > 0.2f) l.Add((s.transform, b));
        }
        return l;
    }

    /// <summary>
    /// Comprueba las reglas 1–4 en <paramref name="scope"/> (la raíz de un microcosmos, su «Historia», o un edificio) y devuelve
    /// las violaciones. Con <paramref name="needs"/>, exige además las necesidades básicas. Se registra en la consola:
    /// «[Reglas] etiqueta: OK (n muebles, necesidades: …)» o un aviso con las primeras violaciones.
    /// </summary>
    public static List<string> Validate(Transform scope, string label = null, bool needs = true, bool log = true)
    {
        var bad = new List<string>();
        if (scope == null) { bad.Add("sin raíz"); return bad; }
        label = label ?? scope.name;

        // 2 · Sin muros ni techos: por nombre en todo el subárbol (menos los muros del borde del piso y el área gigante) y por
        //     forma dentro de cada edificio (AreaFunctionSite): una pieza alta y larga y fina es un muro; una losa por encima de
        //     las cabezas, un techo; un bloque alto y ancho, un edificio macizo (tapa la vista igual).
        foreach (Transform t in scope.GetComponentsInChildren<Transform>(true))
        {
            if (Excluded(t, scope)) continue;
            MicroPiece hp = t.GetComponentInParent<MicroPiece>(true);
            if (hp != null && hp.kind == Historic) continue;   // la barrera que ES la historia (el muro del gueto de Varsovia)
            foreach (string n in WallRoofNames) if (t.name.StartsWith(n)) { bad.Add($"muro/techo «{t.name}» en {Path(t, scope)}"); break; }
        }
        foreach (AreaFunctionSite site in scope.GetComponentsInChildren<AreaFunctionSite>(true))
            foreach (Renderer r in site.GetComponentsInChildren<Renderer>(true))
            {
                if (r.GetComponentInParent<MicroPiece>(true) != null || r.GetComponentInParent<AreaFunctionStation>(true) != null || Excluded(r.transform, scope)) continue;
                if (r.GetComponentInParent<FolkLineage>(true) != null || r.GetComponentInParent<CubeInsect>(true) != null) continue;   // la gente
                Bounds b = r.bounds; float thin = Mathf.Min(b.size.x, b.size.z), len = Mathf.Max(b.size.x, b.size.z);
                if (b.size.y >= WallHeight && len >= 2.5f && thin <= 0.6f) bad.Add($"muro «{r.name}» ({b.size.y:0.0} m de alto) en {site.title}");
                else if (b.min.y - site.transform.position.y >= RoofHeight && b.size.x * b.size.z >= 4f && b.size.y < 1f) bad.Add($"techo «{r.name}» en {site.title}");
                else if (b.size.y >= WallHeight && thin > 0.6f && b.size.x * b.size.z >= 6f) bad.Add($"bloque macizo «{r.name}» ({b.size.x:0} × {b.size.z:0} × {b.size.y:0} m) en {site.title}");
            }

        // 3 · Muebles detallados: un puesto que es UN cubo (con su malla en el propio puesto y sin MicroPiece) es un cubo liso.
        foreach (AreaFunctionStation s in scope.GetComponentsInChildren<AreaFunctionStation>(true))
            if (s.GetComponentInParent<MicroPiece>(true) == null && s.GetComponent<Renderer>() != null && s.transform.childCount == 0)
                bad.Add($"puesto «{s.station}» de un solo cubo en {Path(s.transform, scope)}");

        // 1 · Paso de 2 m entre muebles.
        var pieces = Pieces(scope);
        for (int i = 0; i < pieces.Count; i++)
            for (int j = i + 1; j < pieces.Count; j++)
            {
                if (pieces[i].t.IsChildOf(pieces[j].t) || pieces[j].t.IsChildOf(pieces[i].t)) continue;
                float g = GapXZ(pieces[i].b, pieces[j].b);
                if (g < Passage - 0.01f) bad.Add($"paso de {g:0.00} m (< {Passage:0.0}) entre «{pieces[i].t.name}» y «{pieces[j].t.name}»");
            }

        // 4 · Necesidades básicas.
        var have = new HashSet<string>();
        foreach (MicroPiece p in scope.GetComponentsInChildren<MicroPiece>(true)) if (!string.IsNullOrEmpty(p.need)) have.Add(p.need);
        if (needs) foreach (string n in Needs) if (!have.Contains(n)) bad.Add($"falta la necesidad básica «{n}»");

        if (log)
        {
            if (bad.Count == 0) Debug.Log($"[Reglas] {label}: OK ({pieces.Count} muebles a ≥ {Passage:0.0} m, sin muros ni techos, necesidades: {string.Join(", ", have)}).");
            else Debug.LogWarning($"[Reglas] {label}: {bad.Count} violaciones de docs/microcosmos-reglas-edificacion.md — " + string.Join(" · ", bad.GetRange(0, Mathf.Min(12, bad.Count))) + (bad.Count > 12 ? " …" : ""));
        }
        return bad;
    }

    /// <summary>Distancia en XZ entre dos cajas (0 si se tocan o se solapan).</summary>
    public static float GapXZ(Bounds a, Bounds b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x));
        float dz = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    /// <summary>¿Cubre <paramref name="scope"/> la necesidad <paramref name="need"/>?</summary>
    public static bool Covers(Transform scope, string need)
    {
        foreach (MicroPiece p in scope.GetComponentsInChildren<MicroPiece>(true)) if (p.need == need) return true;
        return false;
    }

    // Los muros del borde del piso (regla 11 de la plantilla) y el área gigante del santuario no son edificios.
    static bool Excluded(Transform t, Transform scope)
    {
        for (Transform p = t; p != null && p != scope.parent; p = p.parent)
            if (p.name == "Muros_del_piso" || p.name.EndsWith("_gigante")) return true;
        return false;
    }

    static bool HasPieceAbove(Transform t, Transform scope)
    {
        for (Transform p = t.parent; p != null && p != scope.parent; p = p.parent) if (p.GetComponent<MicroPiece>() != null) return true;
        return false;
    }

    static bool Box(Transform t, out Bounds b)
    {
        b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>(true))
        {
            if (r.GetComponentInParent<CubeInsect>(true) != null) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        return any;
    }

    static string Path(Transform t, Transform scope)
    {
        AreaFunctionSite s = t.GetComponentInParent<AreaFunctionSite>(true);
        return s != null ? s.title : (t.parent != null ? t.parent.name : scope.name);
    }
}
