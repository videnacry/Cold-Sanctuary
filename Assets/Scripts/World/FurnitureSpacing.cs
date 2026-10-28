using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ESPACIO ESTÁNDAR ENTRE MUEBLES: 2 m LIBRES en TODAS las plataformas (Berón 2026-10-28: «el espacio será estándar de 2 m;
/// aplícalo a todas las plataformas; lo único que no se separa es lo que puede ir pegado/unido»). Antes cada tipo pedía su
/// pasillo (0,8–0,9 m las mesas y camas, 2 m los laboratorios); ahora es uno solo, <see cref="Standard"/>, para que los
/// personajes pasen (y corran) entre dos muebles cualesquiera sin rozarlos. Dos muebles del mismo piso más juntos que eso son
/// una VIOLACIÓN (<see cref="Violations"/>, que la prueba de la vida del santuario pide a 0 en cada área).
///
/// QUÉ ES UN MUEBLE (lo que se mide):
///  · todo objeto montado como mueble: una raíz vacía con un BoxCollider que envuelve sus piezas (<see cref="KitchenProps.Fit"/>
///    lo pone a camas, camillas, mesas del médico, equipos, bloques de laboratorio, máquinas, estanterías…);
///  · y los que se nombran en <see cref="Named"/> aunque no lleven esa caja (máquinas del gimnasio, mesas del comedor, aseos,
///    puestos de cocina, telares…).
///  Solo cuenta el mueble ENTERO (el de más fuera): sus piezas no se miden entre sí. No son muebles las áreas de alfombras
///  (se miden aparte: ningún mueble puede invadir una), las propias alfombras, ni los seres vivos (animas, crías, personajes).
///
/// LO QUE VA UNIDO (no se separa; la regla es «lo que en la realidad va pegado»):
///  · las PIEZAS de un mueble (la silla del escritorio, el ventilador de la cama de la UCI, la cuna térmica del paritorio): ya
///    son el mismo mueble;
///  · las SILLAS con su mesa (son hijas de la mesa: un solo mueble) y la mesa contra la pared;
///  · las BATERÍAS (<see cref="Batteries"/>): muebles iguales puestos en fila tocándose como un solo mueble corrido — perreras y
///    jaulas en batería, boxes de animales grandes con guillotina entre ellos, tinas de tinte en fila, encimeras de cocina
///    alineadas: entre ellos no hay paso que guardar (se usan desde delante), pero el conjunto sí guarda sus 2 m con todo lo
///    demás;
///  · las LÍNEAS (<see cref="FurnitureLine"/>): el contenedor de un tramo corrido (las encimeras, fregadero y máquinas de un
///    obrador contra la pared, las cámaras frías pegadas, la cocina de leche de la Cría): sus muebles van juntos;
///  · lo que va ENCIMA de otro mueble (los ingredientes sobre la encimera): es parte de él;
///  · las PAREJAS (<see cref="Joined"/>): dos muebles distintos que se usan juntos y van pegados — el CONJUNTO de un quirófano
///    (mesa, lámpara, anestesia e instrumental del mismo número), el ecógrafo con su camilla, el botiquín
///    del dojo pegado al vestuario, la barra de sushi con su cafetera. Los de una pareja con número (Quirófano 2 · Anestesia 2) solo van
///    juntos si llevan el MISMO número: dos quirófanos vecinos sí guardan sus 2 m.
/// </summary>
public static class FurnitureSpacing
{
    /// <summary>Pasillo libre estándar (m) entre dos muebles cualesquiera del mismo piso.</summary>
    public const float Standard = 2f;

    /// <summary>Lo mínimo que se aparta un mueble de un ÁREA DE ALFOMBRAS (no la pisa: ahí se posan y se sube a ellas).</summary>
    public const float CarpetClear = 0.3f;

    /// <summary>Prefijos de nombre que son muebles aunque no lleven caja de mueble.</summary>
    public static readonly string[] Named =
    {
        "Maquina_", "Mesa_", "Cama ", "Cama_", "Puesto_", "Telar", "Aseos", "Bloque_",
        // Laboratorio de carne cultivada (equipos de cubos sin caja de mueble).
        "Campana_flujo_laminar", "Tanque_criogenico", "Incubadora_CO2", "Preparacion_medio", "Autoclave", "Biorreactor_",
        "Centrifuga", "Formadora", "Camara_fria", "Consola_lote",
        // Estudio textil (de cubos sueltos).
        "Rueca", "Ovillos", "Tina_", "Maniqui_", "Tendedero",
        // Dojo y sala de ballet acabados (docs/obras-dojo-ballet.md). El tatami no: es suelo (se pisa descalzo, como el
        // suelo flotante del ballet).
        "Jaula_MMA", "Ring_kickboxing", "Sacos_pesados", "Primeros_auxilios", "Vestuario", "Piano_vertical",
        "Equipo_sonido", "Caja_resina", "Fuente", "Banco_calzado",
    };

    /// <summary>Muebles que van EN BATERÍA: dos con el mismo prefijo pueden ir pegados (un solo mueble corrido).</summary>
    public static readonly string[] Batteries =
    {
        "Box ", "Jaulas", "Cuadra ", "Corral_foca", "Tina_", "Maniqui_",
        "Aislamiento",   // las habitaciones de aislamiento, una junto a otra (comparten mampara), como en planta
        "Trineo_", "Cajon_pliometrico_", "Barandilla_", "Muro_",   // calistenia: trineos aparcados juntos, cajones y muros de parkour en serie
    };

    /// <summary>CONJUNTOS de muebles distintos que forman un solo tramo (cualquiera va pegado a cualquier otro del mismo
    /// conjunto): el almacén de material contra la pared de la calistenia y su estructura corrida de barras.</summary>
    public static readonly string[][] Sets =
    {
        new[] { "Estante_bumpers", "Estante_kettlebells", "Cajones_pliometricos", "Balones_medicinales", "Soporte_barras" },
        new[] { "Barras_dominadas", "Escalera_horizontal", "Portico_anillas" },
    };

    /// <summary>Parejas de muebles distintos que se usan juntos y van pegados (con el mismo número, si lo llevan).</summary>
    public static readonly (string a, string b)[] Joined =
    {
        ("Quirofano", "Lampara_quirurgica"), ("Quirofano", "Anestesia"), ("Quirofano", "Instrumental"),   // el conjunto del quirófano
        ("Lampara_quirurgica", "Anestesia"), ("Lampara_quirurgica", "Instrumental"), ("Anestesia", "Instrumental"),
        ("Ecografo", "Camilla eco"),          // el ecógrafo, al lado de la camilla (la sonda llega al paciente)
        ("Equipo_barra de sushi", "Equipo_cafetera espresso"),   // una sola barra de cara a los comensales
        ("Primeros_auxilios", "Vestuario"),   // el botiquín del dojo, pegado al vestuario
    };

    static bool Starts(string n, string p) => n.StartsWith(p, System.StringComparison.Ordinal);

    static bool NamedFurniture(string n) { foreach (string p in Named) if (Starts(n, p)) return true; return false; }

    // El número del final del nombre («Quirofano veterinario 3» → «3»; sin número, «»).
    static string Num(string n)
    {
        int i = n.Length; while (i > 0 && char.IsDigit(n[i - 1])) i--;
        return n.Substring(i);
    }

    /// <summary>¿Va unido a <paramref name="b"/> (batería, conjunto o pareja)? Entonces no se mide el paso entre ellos.</summary>
    public static bool GoTogether(string a, string b)
    {
        foreach (string p in Batteries) if (Starts(a, p) && Starts(b, p)) return true;
        foreach (string[] set in Sets)
        {
            bool ia = false, ib = false;
            foreach (string p in set) { ia |= Starts(a, p); ib |= Starts(b, p); }
            if (ia && ib) return true;
        }
        foreach (var (x, y) in Joined)
            if (((Starts(a, x) && Starts(b, y)) || (Starts(a, y) && Starts(b, x))) && Num(a) == Num(b)) return true;
        return false;
    }

    // La línea de muebles (FurnitureLine) a la que pertenece, si va en una (bajo root).
    static Transform LineOf(Transform t, Transform root)
    {
        for (Transform p = t.parent; p != null && p != root; p = p.parent) if (p.GetComponent<FurnitureLine>() != null) return p;
        return null;
    }

    // Seres vivos y transporte: no son muebles (se mueven).
    static bool Alive(Transform t) =>
        t.GetComponentInParent<Anima>() != null || t.GetComponentInParent<WorldCharacter>() != null || t.GetComponentInParent<Rigidbody>() != null
        || t.GetComponentInParent<FlyingCarpet>() != null;

    /// <summary>¿Es <paramref name="t"/> un mueble entero (no una pieza)? Raíz vacía con caja de mueble, o nombre de mueble.</summary>
    public static bool IsFurniture(Transform t)
    {
        if (t.GetComponent<CarpetPad>() != null) return false;
        if (NamedFurniture(t.name)) return true;
        return t.GetComponent<BoxCollider>() != null && t.GetComponent<Renderer>() == null;
    }

    static bool Measured(Transform t, Transform root)
    {
        if (t == root || !IsFurniture(t) || Alive(t)) return false;
        for (Transform p = t.parent; p != null && p != root; p = p.parent)
            if (IsFurniture(p) || p.GetComponent<CarpetPad>() != null) return false;   // pieza de un mueble (o de un área de alfombras)
        return true;
    }

    static bool Box(Transform t, out Bounds b)
    {
        b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        return any;
    }

    static float Gap(Bounds a, Bounds b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x));
        float dz = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    /// <summary>Los muebles que se miden en <paramref name="root"/> (los mismos de <see cref="Violations"/>) y su caja: lo que
    /// las posiciones reservadas del microcosmos (<see cref="PlatformSlots"/>) deben dejar a ≥ 1 m.</summary>
    public static List<(Transform t, Bounds b)> Items(Transform root)
    {
        var l = new List<(Transform, Bounds)>();
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
            if (Measured(t, root) && Box(t, out Bounds b)) l.Add((t, b));
        return l;
    }

    /// <summary>Distancia en planta (XZ) de <paramref name="p"/> a la caja <paramref name="b"/> (0 si cae dentro).</summary>
    public static float DistXZ(Bounds b, Vector3 p)
    {
        float dx = Mathf.Max(b.min.x - p.x, 0f, p.x - b.max.x), dz = Mathf.Max(b.min.z - p.z, 0f, p.z - b.max.z);
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    /// <summary>Pares de <paramref name="root"/> sin su paso: muebles a menos de <see cref="Standard"/> m (salvo los que van
    /// unidos) y muebles a menos de <see cref="CarpetClear"/> m de un área de alfombras, en el mismo piso.</summary>
    public static List<(Transform a, Transform b, float gap, float need)> Violations(Transform root)
    {
        var items = new List<(Transform t, Bounds b, bool pad)>();
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            CarpetPad cp = t.GetComponent<CarpetPad>();
            if (cp != null)
            {
                BoxCollider bc = t.GetComponent<BoxCollider>();   // la zona marcada (BuildZone: su caja es la zona, 2 m de alto)
                if (bc != null) items.Add((t, new Bounds(t.TransformPoint(bc.center), Abs(t.rotation * bc.size)), true));
                continue;
            }
            if (Measured(t, root) && Box(t, out Bounds b)) items.Add((t, b, false));
        }
        // Lo que va ENCIMA de otro mueble (apoyado en su tablero) es parte de él: no se mide.
        items.RemoveAll(it => !it.pad && items.Exists(o => !o.pad && o.t != it.t && it.b.min.y >= o.b.max.y - 0.1f && it.b.min.y <= o.b.max.y + 0.1f
            && it.b.min.x >= o.b.min.x - 0.05f && it.b.max.x <= o.b.max.x + 0.05f && it.b.min.z >= o.b.min.z - 0.05f && it.b.max.z <= o.b.max.z + 0.05f));
        var bad = new List<(Transform, Transform, float, float)>();
        for (int i = 0; i < items.Count; i++)
            for (int j = i + 1; j < items.Count; j++)
            {
                var (ta, ba, pa) = items[i]; var (tb, bb, pb) = items[j];
                if (pa && pb) continue;   // dos áreas de alfombras (las esterillas de yoga van juntas)
                float ya = pa ? ba.center.y - ba.extents.y : ba.min.y, yb = pb ? bb.center.y - bb.extents.y : bb.min.y;
                if (Mathf.Abs(ya - yb) > 1.5f) continue;   // otro piso (entreplanta, anillo)
                if (!pa && !pb && GoTogether(ta.name, tb.name)) continue;
                if (!pa && !pb && LineOf(ta, root) is Transform la && la == LineOf(tb, root)) continue;   // el mismo tramo corrido
                float need = pa || pb ? CarpetClear : Standard, gap = Gap(ba, bb);
                if (gap < need) bad.Add((ta, tb, gap, need));
            }
        return bad;
    }

    /// <summary>Los pares peores (los más juntos) en una línea, para el detalle de una prueba.</summary>
    public static string Worst(List<(Transform a, Transform b, float gap, float need)> bad, int n = 4)
    {
        bad.Sort((x, y) => x.gap.CompareTo(y.gap));
        var s = new List<string>();
        for (int i = 0; i < bad.Count && i < n; i++) s.Add($"«{bad[i].a.name}»–«{bad[i].b.name}» {bad[i].gap:0.00}/{bad[i].need:0.0} m");
        return string.Join("; ", s);
    }

    /// <summary>Con cuántos muebles de <paramref name="scope"/> queda <paramref name="item"/> sin su pasillo (0 si no es un mueble).</summary>
    public static int CloseCount(Transform item, Transform scope)
    {
        if (!IsFurniture(item)) return 0;
        int n = 0;
        foreach (var (a, b, _, _) in Violations(scope)) if (a == item || b == item) n++;
        return n;
    }

    /// <summary>Revisa todas las áreas y avisa por consola de cada par demasiado junto.</summary>
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
        Debug.Log($"[Espacio] {total} pares de muebles sin sus {Standard:0} m de paso.");
        return total;
    }
}
