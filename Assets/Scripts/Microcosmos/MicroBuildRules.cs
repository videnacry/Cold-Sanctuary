using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS REGLAS DE EDIFICACIÓN DE LOS MICROCOSMOS Y MESOCOSMOS (docs/microcosmos-reglas-edificacion.md; Berón 2026-11-01: «en el
/// microcosmos de Bruce Lee la cocina y los muebles parecían cubos»), revisadas el 2026-11-18 (docs/plan-microcosmos-realismo.md
/// §3: insectos-humanos a la MITAD, s = <see cref="FolkScale"/>, con las patas recogidas, y vuelven los muros con vista de corte).
/// Los números salen del cuerpo encogido (<see cref="FolkScale"/>; para volver a afinarlo a 0,4 basta con cambiar esa constante):
/// 1. PASO: entre dos muebles cualesquiera, <see cref="Passage"/> (2,4 m con s = 0,5) libres: dos cuerpos recogidos se cruzan andando.
///    Los microcosmos montados ANTES del encogimiento (<see cref="MicroFloor.folkScale"/> = 1) se miden con el paso de entonces
///    (<see cref="LegacyPassage"/>, 2 m) hasta que se reconstruyan, uno a uno (fase 4 del plan).
/// 2. MUROS Y TECHOS PERMITIDOS (P1–P4, sustituye a la regla «sin muros ni techos» del 2026-11-01): cada lugar real se recrea a
///    1:1 con sus fachadas, pero todo edificio con muros o techo lleva su VISTA DE CORTE (<see cref="CutawayView"/>: por dentro, el
///    techo y los muros entre la cámara y el jugador se desvanecen) y al menos una PUERTA (<see cref="MicroDoor"/>) de
///    ≥ <see cref="DoorWidth"/> × <see cref="DoorHeight"/> m (las reales más estrechas se ensanchan).
/// 3. MUEBLES DETALLADOS (<see cref="MicroPiece"/>, varias piezas) con sus utensilios (sartén, olla, cuchillo…): nada de cubos lisos.
/// 4. NECESIDADES BÁSICAS: camas, retretes, lavabos, mesas, sillas (o apoyos de abdomen) y cocina (<see cref="Needs"/>).
/// 5. ERAS AVANZADAS (<see cref="IsTownEra"/>): el pueblo o la ciudad real ocupa TODO el piso (sin los muebles gigantes del área).
/// <see cref="Validate"/> comprueba 1–4 en una escena o en un subárbol y devuelve (y registra) las violaciones. Los CONSEJOS (P5 hito,
/// P6 transporte de época, C4 sin escaleras por debajo de 12 m) aún no son faltas: los da <see cref="Advise"/> y Validate los registra.
/// </summary>
public static class MicroBuildRules
{
    // ── LA ESCALA DEL CUERPO (E1, plan §3.1): UNA sola constante; todo lo demás sale de ella ─────────────────────────────────
    /// <summary>E1 · FACTOR s de los insectos-humanos (Berón 2026-11-18: «la mitad de grandes»): su estatura, su largo, sus patas,
    /// sus brazos y sus antenas × s. El MUNDO (arquitectura, calles, árboles, vehículos, objetos de mano) se queda a 1:1, así que
    /// desde el cuerpo se ve 1/s veces mayor. Para afinarlo (p. ej. a 0,4), cambiar SOLO esto.</summary>
    public const float FolkScale = 0.5f;
    /// <summary>La estatura humana de referencia (m): la que se pasa a <c>AreaMicrocosmos.Folk</c>, antes de encoger.</summary>
    public const float HumanStature = 1.75f;
    /// <summary>La estatura de un insecto-humano tipo (1,75 × s = 0,875 m).</summary>
    public const float FolkStature = HumanStature * FolkScale;
    /// <summary>La estatura de un insecto-humano cuya persona mediría <paramref name="humanHeight"/> m.</summary>
    public static float Stature(float humanHeight) => humanHeight * FolkScale;
    /// <summary>ANCHO ANDANDO con las patas RECOGIDAS (<c>CubeInsect</c>, postura «recogida»): 2 × s (1,0 m con s = 0,5).</summary>
    public const float TuckedWalkWidth = 2f * FolkScale;
    /// <summary>Radio del NavMeshAgent de un insecto-humano: medio ancho recogido (0,5 m con s = 0,5).</summary>
    public const float AgentRadius = 0.5f * TuckedWalkWidth;
    /// <summary>Radio de la cápsula física del insecto-humano (0,35 m a escala humana, × s).</summary>
    public const float CapsuleRadius = 0.35f * FolkScale;
    /// <summary>Velocidad del agente (3 m/s a escala humana, × s): la amplitud del paso de <c>CubeInsect</c> va con speed/1,5;
    /// si no bajara, el paso se saturaría y las patas patinarían.</summary>
    public const float FolkSpeed = 3f * FolkScale;
    /// <summary>Holgura a cada lado del par que se cruza (la evitación del agente y el giro del cuerpo).</summary>
    public const float SideMargin = 0.2f;
    /// <summary>PASO LIBRE entre dos muebles (E4): dos cuerpos recogidos andando (4 × 0,5 = 2,0 m) + 0,2 m a cada lado = 2,4 m.
    /// (El santuario sigue con el suyo, <see cref="FurnitureSpacing.Standard"/> = 2 m: allí andan personas.)</summary>
    public const float Passage = 4f * AgentRadius + 2f * SideMargin;
    /// <summary>El paso con que se montaron los microcosmos ANTES del encogimiento (2 agentes de 0,4 m + 0,2 + 0,2 = 2,0 m): vale
    /// para los que aún tienen <see cref="MicroFloor.folkScale"/> = 1 (<see cref="PassageFor"/>).</summary>
    public const float LegacyPassage = 2f;
    /// <summary>Las calles de un pueblo (E4: se quedan en 4 m; caben dos parejas o un carro real de 2–2,5 m).</summary>
    public const float Street = 4f;
    /// <summary>Delante de cada puesto de trabajo, el sitio donde se pone el trabajador (fondo libre; 0,9 m → 0,6 m con s = 0,5).</summary>
    public const float WorkSpot = 1.2f * FolkScale;
    /// <summary>P4 · ANCHO MÍNIMO DE PUERTA: el cuerpo recogido andando + 0,2 m (1,2 m con s = 0,5). Las reales más estrechas se ensanchan.</summary>
    public const float DoorWidth = TuckedWalkWidth + 0.2f;
    /// <summary>P4 · ALTO MÍNIMO DE PUERTA: el alto con las antenas (~2,1 × s) + 0,2 m (1,25 m con s = 0,5). El real siempre basta.</summary>
    public const float DoorHeight = 2.1f * FolkScale + 0.2f;
    /// <summary>C4 · Por debajo de esta altura (m) no hay escaleras ni ascensores para moverse: trepan las paredes.</summary>
    public const float LiftFrom = 12f;
    /// <summary>Alto a partir del cual una pieza de edificio es un MURO (pide vista de corte y puerta).</summary>
    public const float WallHeight = 1.2f;
    /// <summary>Alto desde el que una losa por encima de las cabezas es un TECHO (pide vista de corte).</summary>
    public const float RoofHeight = 2.2f;

    /// <summary>MicroPiece.kind del HITO reconocible de cada microcosmos (P5: el Falcon 1, la kallanka, la universidad de Leiden).</summary>
    public const string Landmark = "hito";
    /// <summary>MicroPiece.kind del TRANSPORTE DE ÉPOCA que se usa (P6: se sube, se mueve, lleva carga).</summary>
    public const string Vehicle = "vehiculo";
    /// <summary>MicroPiece.kind de una escalera que es MONUMENTO (C4: escalinatas, pirámides; no camino obligado).</summary>
    public const string Monument = "monumento";

    /// <summary>El paso que vale en <paramref name="scope"/>: el nuevo (<see cref="Passage"/>), o el de antes del encogimiento si su
    /// microcosmos se montó con los cuerpos a escala humana (<see cref="MicroFloor.folkScale"/> ≥ 0,99).</summary>
    public static float PassageFor(Transform scope)
    {
        if (scope == null) return Passage;
        MicroFloor mf = scope.GetComponentInParent<MicroFloor>(true);
        if (mf == null) mf = scope.GetComponentInChildren<MicroFloor>(true);
        return mf != null && mf.folkScale >= 0.99f ? LegacyPassage : Passage;
    }

    /// <summary>Desde qué año un microcosmos es de ERA AVANZADA (las primeras ciudades: Uruk, ~3500 a. C.): el pueblo o la ciudad
    /// real ocupa el piso entero. Antes, las aldeas (Huerto, ~9500 a. C.) conviven con los muebles gigantes del área.</summary>
    public const int TownFrom = -3500;
    public static bool IsTownEra(int year) => year >= TownFrom;

    /// <summary>Las necesidades básicas que todo pueblo cubre (MicroPiece.need).</summary>
    public static readonly string[] Needs = { "dormir", "aseo", "lavarse", "comer", "sentarse", "cocinar" };

    /// <summary>Nombres de piezas que son muros o techos de edificio (los del borde del piso, «Muros_del_piso», no cuentan).</summary>
    public static readonly string[] WallRoofNames = { "Muro_", "Muro ", "Cubierta", "Faldon", "Dintel", "Techo", "Tejado", "Chapitel" };
    /// <summary>Los de <see cref="WallRoofNames"/> que son TECHO (el resto, muro).</summary>
    public static readonly string[] RoofNames = { "Cubierta", "Faldon", "Techo", "Tejado", "Chapitel" };

    /// <summary>MicroPiece.kind de una BARRERA HISTÓRICA que es la propia historia (el muro del gueto de Varsovia, cuyas salidas
    /// son los pasos de la misión): no es un edificio (no pide vista de corte ni puerta) ni cuenta para el paso.</summary>
    public const string Historic = "historico";

    /// <summary>¿Es <paramref name="r"/> un MURO de edificio (por nombre o por forma: alto, largo y fino)?</summary>
    public static bool IsWall(Renderer r, float groundY)
    {
        if (r == null) return false;
        string n = r.name; foreach (string w in WallRoofNames) if (n.StartsWith(w)) return System.Array.IndexOf(RoofNames, w) < 0;
        Bounds b = r.bounds; float thin = Mathf.Min(b.size.x, b.size.z), len = Mathf.Max(b.size.x, b.size.z);
        return b.size.y >= WallHeight && len >= 2.5f && thin <= 0.6f && b.min.y - groundY < WallHeight;
    }

    /// <summary>¿Es <paramref name="r"/> un TECHO de edificio (por nombre o por forma: una losa por encima de las cabezas)?</summary>
    public static bool IsRoof(Renderer r, float groundY)
    {
        if (r == null) return false;
        string n = r.name; foreach (string w in RoofNames) if (n.StartsWith(w)) return true;
        Bounds b = r.bounds;
        return b.min.y - groundY >= RoofHeight && b.size.x * b.size.z >= 4f && b.size.y < 1f;
    }

    /// <summary>Los renderers de <paramref name="site"/> que son de su edificio (no de un mueble, un puesto, la gente, el cristal,
    /// el borde del piso ni la barrera histórica).</summary>
    public static IEnumerable<Renderer> ShellRenderers(Transform site, Transform scope = null)
    {
        foreach (Renderer r in site.GetComponentsInChildren<Renderer>(true))
        {
            if (r.GetComponentInParent<MicroPiece>(true) != null || r.GetComponentInParent<AreaFunctionStation>(true) != null) continue;
            if (r.GetComponentInParent<FolkLineage>(true) != null || r.GetComponentInParent<CubeInsect>(true) != null) continue;   // la gente
            if (r.GetComponentInParent<GlassWall>(true) != null || r.GetComponentInParent<MicroDoor>(true) != null) continue;
            if (Excluded(r.transform, scope ?? site)) continue;
            yield return r;
        }
    }

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
        // PAREDES DE CRISTAL (2026-11-03): cada paño guarda el paso con los muebles de dentro y de fuera de su sala.
        foreach (GlassWall g in GlassWall.Panes(scope))
            if (Box(g.transform, out Bounds b)) l.Add((g.transform, b));
        return l;
    }

    /// <summary>
    /// Comprueba las reglas 1–4 en <paramref name="scope"/> (la raíz de un microcosmos, su «Historia», o un edificio) y devuelve
    /// las violaciones. Con <paramref name="needs"/>, exige además las necesidades básicas. Se registra en la consola:
    /// «[Reglas] etiqueta: OK (n muebles, necesidades: …)» o un aviso con las primeras violaciones.
    /// </summary>
    public static List<string> Validate(Transform scope, string label = null, bool needs = true, bool log = true, List<string> advice = null)
    {
        var bad = new List<string>();
        if (scope == null) { bad.Add("sin raíz"); return bad; }
        label = label ?? scope.name;
        float passage = PassageFor(scope);

        // 2 · MUROS Y TECHOS PERMITIDOS, con vista de corte y puerta (P2–P4, 2026-11-18). Cada edificio (AreaFunctionSite) con algún
        //     muro (por nombre o forma: alto, largo y fino) o techo (losa por encima de las cabezas) lleva su CutawayView y al menos
        //     una MicroDoor; cada puerta, de al menos DoorWidth × DoorHeight. (El borde del piso, la barrera histórica y el cristal
        //     no son edificio.)
        foreach (AreaFunctionSite site in scope.GetComponentsInChildren<AreaFunctionSite>(true))
        {
            float gy = site.transform.position.y;
            string wall = null, roof = null;
            foreach (Renderer r in ShellRenderers(site.transform, scope))
            {
                MicroPiece hp = r.GetComponentInParent<MicroPiece>(true);
                if (hp != null && hp.kind == Historic) continue;
                if (wall == null && IsWall(r, gy)) wall = r.name;
                else if (roof == null && IsRoof(r, gy)) roof = r.name;
                if (wall != null && roof != null) break;
            }
            if (wall == null && roof == null) continue;
            string what = wall != null && roof != null ? $"muros («{wall}») y techo («{roof}»)" : wall != null ? $"muros («{wall}»)" : $"techo («{roof}»)";
            if (site.GetComponentInChildren<CutawayView>(true) == null && site.GetComponentInParent<CutawayView>(true) == null)
                bad.Add($"«{site.title}» con {what} sin vista de corte (CutawayView, P2)");
            if (wall != null && site.GetComponentInChildren<MicroDoor>(true) == null)
                bad.Add($"«{site.title}» con muros y sin puerta (MicroDoor, P3)");
        }
        foreach (MicroDoor d in scope.GetComponentsInChildren<MicroDoor>(true))
            if (d.width < DoorWidth - 0.01f || d.height < DoorHeight - 0.01f)
                bad.Add($"puerta «{d.name}» de {d.width:0.00} × {d.height:0.00} m (< {DoorWidth:0.00} × {DoorHeight:0.00}: el cuerpo recogido no pasa, P4) en {Path(d.transform, scope)}");

        // 3 · Muebles detallados: un puesto que es UN cubo (con su malla en el propio puesto y sin MicroPiece) es un cubo liso.
        foreach (AreaFunctionStation s in scope.GetComponentsInChildren<AreaFunctionStation>(true))
            if (s.GetComponentInParent<MicroPiece>(true) == null && s.GetComponent<Renderer>() != null && s.transform.childCount == 0)
                bad.Add($"puesto «{s.station}» de un solo cubo en {Path(s.transform, scope)}");

        // 1 · El paso entre muebles (2,4 m; 2 m en los microcosmos aún sin encoger).
        var pieces = Pieces(scope);
        for (int i = 0; i < pieces.Count; i++)
            for (int j = i + 1; j < pieces.Count; j++)
            {
                if (pieces[i].t.IsChildOf(pieces[j].t) || pieces[j].t.IsChildOf(pieces[i].t)) continue;
                if (pieces[i].t.GetComponent<GlassWall>() != null && pieces[j].t.GetComponent<GlassWall>() != null) continue;   // dos paños de cristal pueden ir pegados
                float g = GapXZ(pieces[i].b, pieces[j].b);
                if (g < passage - 0.01f) bad.Add($"paso de {g:0.00} m (< {passage:0.0}) entre «{pieces[i].t.name}» y «{pieces[j].t.name}»");
            }

        // 8–12 · Los edificios (2026-11-03): su actividad e interacción, adaptados a humanos e insectos-humanos, su estilo del
        //        catálogo, el frutal de cada residencia y las plantas en terraza sin nada debajo.
        bad.AddRange(ValidateSites(scope));

        // 4 · Necesidades básicas.
        var have = new HashSet<string>();
        foreach (MicroPiece p in scope.GetComponentsInChildren<MicroPiece>(true)) if (!string.IsNullOrEmpty(p.need)) have.Add(p.need);
        if (needs) foreach (string n in Needs) if (!have.Contains(n)) bad.Add($"falta la necesidad básica «{n}»");

        // P5, P6, C4 · Consejos (aún no son faltas).
        List<string> tips = Advise(scope);
        if (advice != null) advice.AddRange(tips);

        if (log)
        {
            if (bad.Count == 0) Debug.Log($"[Reglas] {label}: OK ({pieces.Count} muebles a ≥ {passage:0.0} m, edificios con vista de corte y puerta, necesidades: {string.Join(", ", have)}).");
            else Debug.LogWarning($"[Reglas] {label}: {bad.Count} violaciones de docs/microcosmos-reglas-edificacion.md — " + string.Join(" · ", bad.GetRange(0, Mathf.Min(12, bad.Count))) + (bad.Count > 12 ? " …" : ""));
            if (tips.Count > 0) Debug.Log($"[Reglas] {label}: consejos (P5, P6, C4) — " + string.Join(" · ", tips.GetRange(0, Mathf.Min(8, tips.Count))));
        }
        return bad;
    }

    /// <summary>
    /// LOS CONSEJOS de docs/plan-microcosmos-realismo.md §3 que aún no son faltas (2026-11-18; pasarán a reglas cuando el piloto,
    /// Microcosmos_ElonMusk, las cumpla):
    /// - P5 · un HITO reconocible (MicroPiece «<see cref="Landmark"/>») en cada microcosmos (raíz con <see cref="MicroFloor"/>);
    /// - P6 · un TRANSPORTE DE ÉPOCA que se use (MicroPiece «<see cref="Vehicle"/>»);
    /// - C4 · ninguna escalera para moverse en un edificio de menos de <see cref="LiftFrom"/> m (trepan); las escaleras reales que
    ///   son monumento llevan MicroPiece «<see cref="Monument"/>».
    /// </summary>
    public static List<string> Advise(Transform scope)
    {
        var tips = new List<string>();
        if (scope == null) return tips;
        if (scope.GetComponent<MicroFloor>() != null || scope.GetComponentInChildren<MicroFloor>(true) != null)
        {
            bool landmark = false, vehicle = false;
            foreach (MicroPiece p in scope.GetComponentsInChildren<MicroPiece>(true)) { if (p.kind == Landmark) landmark = true; if (p.kind == Vehicle) vehicle = true; }
            if (!landmark) tips.Add($"sin hito reconocible (MicroPiece «{Landmark}», P5)");
            if (!vehicle) tips.Add($"sin transporte de época que se use (MicroPiece «{Vehicle}», P6)");
        }
        foreach (AreaFunctionSite site in scope.GetComponentsInChildren<AreaFunctionSite>(true))
        {
            float top = 0f, gy = site.transform.position.y; bool any = false;
            foreach (Renderer r in ShellRenderers(site.transform, scope)) { top = Mathf.Max(top, r.bounds.max.y - gy); any = true; }
            if (!any || top >= LiftFrom) continue;
            foreach (Transform t in site.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("Escalera") && !t.name.StartsWith("Ascensor")) continue;
                MicroPiece mp = t.GetComponentInParent<MicroPiece>(true);
                if (mp != null && mp.kind == Monument) continue;
                tips.Add($"«{t.name}» en «{site.title}» de {top:0} m (< {LiftFrom:0} m: sin escaleras ni ascensores, trepan las paredes, C4)");
                break;
            }
        }
        return tips;
    }

    /// <summary>
    /// LAS REGLAS DE LOS EDIFICIOS (8–12, 2026-11-03; docs/microcosmos-reglas-edificacion.md):
    /// 8. cada edificio (AreaFunctionSite) lleva su ACTIVIDAD (trabajo, descanso, recuperacion u ocio), su INTERACCIÓN y su capacidad;
    /// 9. está ADAPTADO A PERSONAS Y A INSECTOS-HUMANOS: al menos un mueble a la medida de cada uno (MicroPiece.fitted);
    /// 10. su ESTILO sale del CATÁLOGO: su región es una de MicroStyles.Region y cada mueble lleva un estilo (MicroPiece.style) del catálogo;
    /// 11. toda RESIDENCIA (con un hogar, Household) tiene su FRUTAL;
    /// 12. una planta en terraza (MicroPiece «planta») no tiene ningún mueble debajo (no es un techo).
    /// </summary>
    public static List<string> ValidateSites(Transform scope)
    {
        var bad = new List<string>();
        var ids = new HashSet<string>(); foreach (MicroStyles.Entry e in MicroStyles.All) ids.Add(e.id);
        foreach (AreaFunctionSite site in scope.GetComponentsInChildren<AreaFunctionSite>(true))
        {
            if (System.Array.IndexOf(AreaFunctionSite.Activities, site.activity) < 0) bad.Add($"«{site.title}» sin etiqueta de actividad (trabajo, descanso, recuperacion, ocio o entrenamiento)");
            if (string.IsNullOrEmpty(site.interaction) || site.capacity <= 0) bad.Add($"«{site.title}» sin su interacción o su capacidad");
            bool human = false, insect = false, fruit = false;
            foreach (MicroPiece mp in site.GetComponentsInChildren<MicroPiece>(true))
            {
                if (mp.fitted) { if (string.IsNullOrEmpty(mp.fitFor)) human = true; else insect = true; }
                if (mp.kind == "frutal") fruit = true;
                if (!string.IsNullOrEmpty(mp.style) && !ids.Contains(mp.style)) bad.Add($"«{mp.name}» en «{site.title}»: estilo «{mp.style}» fuera del catálogo");
            }
            if (!human) bad.Add($"«{site.title}» no adaptado a personas (ningún mueble a la medida de un humano)");
            if (!insect) bad.Add($"«{site.title}» no adaptado a insectos-humanos (ningún mueble a la medida de su cuerpo)");
            if (string.IsNullOrEmpty(site.region) || !System.Enum.TryParse(site.region, out MicroStyles.Region _)) bad.Add($"«{site.title}» sin región del catálogo de estilos («{site.region}»)");
            if (site.GetComponentInChildren<Household>(true) != null && !fruit) bad.Add($"residencia «{site.title}» sin frutal");
        }
        // 12 · Las plantas en terraza no cubren nada.
        foreach (MicroPiece tier in scope.GetComponentsInChildren<MicroPiece>(true))
        {
            if (tier.kind != "planta" || !Box(tier.transform, out Bounds tb)) continue;
            AreaFunctionSite site = tier.GetComponentInParent<AreaFunctionSite>(true);
            if (site == null) continue;
            foreach (MicroPiece mp in site.GetComponentsInChildren<MicroPiece>(true))
            {
                if (mp == tier || mp.walkable || mp.transform.IsChildOf(tier.transform) || !Box(mp.transform, out Bounds b)) continue;
                if (b.min.y < tb.max.y - 1.0f && b.max.x > tb.min.x + 0.05f && b.min.x < tb.max.x - 0.05f && b.max.z > tb.min.z + 0.05f && b.min.z < tb.max.z - 0.05f)
                { bad.Add($"la planta en terraza de «{site.title}» tapa «{mp.name}»"); break; }
            }
        }
        bad.AddRange(ValidateCivicScale(scope));
        return bad;
    }

    /// <summary>
    /// (2026-11-04) LA ESCALA DEL MOBILIARIO CÍVICO (reglas 13–15, docs/microcosmos-suelo-y-elementos.md §Escala; <see cref="MicroCivicScale"/>):
    /// 13. TODO ASIENTO ES UTILIZABLE: cada silla y cada banco (MicroPiece «silla»/«banco») lleva su <see cref="MicroSeat"/> con sitios;
    /// 14. los PLATOS de las mesas dan una comida (≥ <see cref="MicroCivicScale.PlateFor"/>) y las OLLAS de las cocinas, 2 L por comensal;
    /// 15. las ESTATUAS son monumentales y modelan INSECTOS-HUMANOS (nada de figura humana), y las fuentes miden lo de una plaza.
    /// </summary>
    public static List<string> ValidateCivicScale(Transform scope)
    {
        var bad = new List<string>();
        foreach (MicroPiece mp in scope.GetComponentsInChildren<MicroPiece>(true))
        {
            if (mp.kind == "silla" || mp.kind == "banco")
            {
                MicroSeat s = mp.GetComponent<MicroSeat>();
                if (s == null || s.points == null || s.points.Length == 0) bad.Add($"asiento «{mp.name}» sin sitio para sentarse (MicroSeat)");
            }
            else if (mp.kind == "estatua")
            {
                if (Box(mp.transform, out Bounds b) && b.size.y < MicroCivicScale.StatueTotal() * 0.85f) bad.Add($"estatua «{mp.name}» de {b.size.y:0.0} m (< {MicroCivicScale.StatueTotal():0.0}: no es monumental)");
                bool insect = false, human = false;
                foreach (Transform c in mp.GetComponentsInChildren<Transform>(true)) { if (c.name == "Figura_insecto_humano") insect = true; if (c.name == "Figura" || c.name == "Brazo" && c.parent == mp.transform) human = true; }
                if (human && !insect) bad.Add($"estatua «{mp.name}» con figura humana (debe modelar un insecto-humano)");
            }
            else if (mp.kind == "fuente" && Box(mp.transform, out Bounds fb) && Mathf.Max(fb.size.x, fb.size.z) < MicroCivicScale.FountainSide() * 0.85f)
                bad.Add($"fuente «{mp.name}» de {Mathf.Max(fb.size.x, fb.size.z):0.0} m de lado (< {MicroCivicScale.FountainSide():0.0})");
        }
        foreach (Transform t in scope.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Plato" && t.parent != null && t.parent.name == "Mesa" && t.localScale.x * 0.24f < MicroCivicScale.PlateFor() * 0.95f)
            { bad.Add($"plato de {t.localScale.x * 0.24f:0.00} m en «{t.parent.parent?.name}» (< {MicroCivicScale.PlateFor():0.00}: no da una comida)"); break; }
        }
        foreach (Transform t in scope.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Olla" && t.GetComponentInParent<MicroPiece>(true) != null && Box(t, out Bounds ob) && MicroCivicScale.PotLitres(Mathf.Max(ob.size.x, ob.size.z) * 0.8f) < 2f * 6f)
            { bad.Add($"olla de {MicroCivicScale.PotLitres(Mathf.Max(ob.size.x, ob.size.z) * 0.8f):0} L en «{t.parent?.name}» (< 12 L: no da de comer a una casa)"); break; }
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
