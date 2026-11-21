using UnityEngine;

/// <summary>
/// EDIFICIOS CERRADOS (docs/microcosmos/plan-microcosmos-realismo.md §3.3, P1–P4; Berón 2026-11-18: vuelven los muros y los techos, con vista
/// de corte por dentro). <see cref="Enclose"/> levanta los cuatro muros de un edificio en el borde de su suelo (por dentro: el suelo
/// no crece y <see cref="CivicBuilder.Settle"/> lo sigue midiendo igual), el hueco de su PUERTA (<see cref="MicroDoor"/>, ensanchada a
/// MicroBuildRules.DoorWidth si la real es más estrecha) con su dintel, el TECHO de su estilo (plano o a dos aguas) y la VISTA DE CORTE
/// (<see cref="CutawayView"/>). El techo no lleva colisión: no hace falta (nadie anda por él) y la cámara no tropieza con él.
/// Los muebles ya guardan el paso con los muros: <see cref="CivicBuilder.Layout"/> deja MicroBuildRules.Passage hasta el borde del suelo.
/// </summary>
internal static partial class CivicBuilder
{
    /// <summary>Grueso de los muros (m).</summary>
    internal const float WallThick = 0.3f;

    /// <summary>Cierra el edificio <paramref name="b"/> (planta <paramref name="size"/> centrada en su origen, la fachada de la puerta
    /// a −z) con los muros, la puerta, el techo y la vista de corte del estilo <paramref name="st"/>. <paramref name="doorX"/>: dónde va
    /// la puerta a lo largo de la fachada. Devuelve la vista de corte.</summary>
    internal static CutawayView Enclose(Transform b, Vector2 size, Style st, float doorX = 0f)
        => Enclose(b, size, st.wallH, st.wall, st.roof, st.roofKind, st.doorW, st.doorH, st.doorSource, doorX);

    internal static CutawayView Enclose(Transform b, Vector2 size, float wallH, Color wall, Color roof, Roof kind, float doorRealW, float doorRealH, string doorSource, float doorX = 0f)
    {
        Material mw = AreaMicrocosmos.Mat("Civ_Muro_" + Hex(wall), wall);
        Material mr = AreaMicrocosmos.Mat("Civ_Techo_" + Hex(roof), roof);
        float hx = size.x * 0.5f, hz = size.y * 0.5f, th = WallThick;
        // Norte, este y oeste, enteros.
        AreaMicrocosmos.Box(b, "Muro_norte", new Vector3(0f, wallH * 0.5f, hz - th * 0.5f), new Vector3(size.x, wallH, th), mw, true);
        AreaMicrocosmos.Box(b, "Muro_este", new Vector3(hx - th * 0.5f, wallH * 0.5f, 0f), new Vector3(th, wallH, size.y), mw, true);
        AreaMicrocosmos.Box(b, "Muro_oeste", new Vector3(-hx + th * 0.5f, wallH * 0.5f, 0f), new Vector3(th, wallH, size.y), mw, true);
        // Sur: la fachada con el hueco de la puerta (ensanchada si hace falta, P4) y su dintel.
        MicroDoor door = MicroDoor.Place(b, "Puerta", new Vector3(doorX, 0f, -hz + th * 0.5f), 0f, doorRealW, doorRealH, doorSource);
        door.height = Mathf.Min(door.height, wallH - 0.2f);
        DoorLink(door, th);
        float dw = door.width, a0 = -hx, a1 = doorX - dw * 0.5f, b0 = doorX + dw * 0.5f, b1 = hx;
        if (a1 - a0 > 0.05f) AreaMicrocosmos.Box(b, "Muro_sur", new Vector3((a0 + a1) * 0.5f, wallH * 0.5f, -hz + th * 0.5f), new Vector3(a1 - a0, wallH, th), mw, true);
        if (b1 - b0 > 0.05f) AreaMicrocosmos.Box(b, "Muro_sur", new Vector3((b0 + b1) * 0.5f, wallH * 0.5f, -hz + th * 0.5f), new Vector3(b1 - b0, wallH, th), mw, true);
        if (wallH - door.height > 0.05f)
            AreaMicrocosmos.Box(b, "Dintel", new Vector3(doorX, door.height + (wallH - door.height) * 0.5f, -hz + th * 0.5f), new Vector3(dw, wallH - door.height, th), mw, true);
        // El techo del estilo (sin colisión).
        switch (kind)
        {
            case Roof.Gable:
            {
                float rise = Mathf.Min(2.5f, size.x * 0.18f), half = hx + 0.3f, len = Mathf.Sqrt(half * half + rise * rise), ang = Mathf.Atan2(rise, half) * Mathf.Rad2Deg;
                for (int s = -1; s <= 1; s += 2)
                    AreaMicrocosmos.Box(b, "Faldon", new Vector3(s * half * 0.5f, wallH + rise * 0.5f, 0f), new Vector3(len, 0.2f, size.y + 0.6f), Quaternion.Euler(0f, 0f, -s * ang), mr, false);
                break;
            }
            default:   // plano (y «None», que en un edificio cerrado es una azotea: sin techo no sería un recinto)
                AreaMicrocosmos.Box(b, "Cubierta", new Vector3(0f, wallH + 0.15f, 0f), new Vector3(size.x + 0.4f, 0.3f, size.y + 0.4f), mr, false);
                break;
        }
        return CutawayView.Ensure(b);
    }

    /// <summary>EL PASO POR LA PUERTA para la navegación: un NavMeshLink de un lado al otro del hueco, de su ancho. El NavMesh se encoge
    /// el radio del agente junto a cada muro, y en un hueco de 1,2 m (P4) con el agente de 0,5 m y el vóxel de 0,25 m apenas queda suelo:
    /// con el enlace, los insectos-humanos cruzan igual (como los enlaces de trepar de AreaMicrocosmos).</summary>
    internal static void DoorLink(MicroDoor door, float wallThick)
    {
        if (door == null) return;
        var g = new GameObject("Paso_de_puerta"); g.transform.SetParent(door.transform, false);
        var link = g.AddComponent<Unity.AI.Navigation.NavMeshLink>();
        float reach = wallThick * 0.5f + MicroBuildRules.AgentRadius + 0.6f;
        link.startPoint = new Vector3(0f, 0f, -reach); link.endPoint = new Vector3(0f, 0f, reach);
        link.width = Mathf.Max(0.5f, door.width - 0.2f); link.bidirectional = true;
    }

    /// <summary>Rehace los muros, el techo y la puerta de un edificio cerrado cuando su suelo cambia de tamaño (los hogares se
    /// recolocan a la medida de su familia: <see cref="Relayout"/>), con las mismas medidas y colores que tenía.</summary>
    internal static void Reenclose(Transform b, Vector2 size)
    {
        Transform north = b.Find("Muro_norte"); if (north == null) return;
        float wallH = north.localScale.y;
        Renderer nr = north.GetComponent<Renderer>(); Color wall = nr != null && nr.sharedMaterial != null ? nr.sharedMaterial.color : Color.white;
        Transform roofT = b.Find("Cubierta") ?? b.Find("Faldon"); Renderer rr = roofT != null ? roofT.GetComponent<Renderer>() : null;
        Color roof = rr != null && rr.sharedMaterial != null ? rr.sharedMaterial.color : Color.gray;
        Roof kind = b.Find("Faldon") != null ? Roof.Gable : Roof.Flat;
        MicroDoor old = null; foreach (Transform c in b) if (c.name == "Puerta" && c.TryGetComponent(out MicroDoor d)) old = d;
        float rw = old != null && old.realWidth > 0f ? old.realWidth : MicroBuildRules.DoorWidth, rh = old != null ? old.height : 2.1f;
        string src = old != null ? old.source.Replace(" (supuesto: ensanchada)", "") : "supuesto";
        var kill = new System.Collections.Generic.List<GameObject>();
        foreach (Transform c in b)
            if (c.name == "Muro_norte" || c.name == "Muro_este" || c.name == "Muro_oeste" || c.name == "Muro_sur" || c.name == "Dintel" || c.name == "Cubierta" || c.name == "Faldon" || c == (old != null ? old.transform : null))
                kill.Add(c.gameObject);
        foreach (GameObject g in kill) Object.DestroyImmediate(g);
        Enclose(b, size, wallH, wall, roof, kind, rw, rh, src);
    }

    /// <summary>Un TABIQUE interior (m, en el marco de <paramref name="b"/>): de <paramref name="from"/> a <paramref name="to"/> en planta,
    /// con un hueco de puerta (<see cref="MicroDoor"/>, ensanchada si hace falta) a <paramref name="doorAt"/> m de <paramref name="from"/>
    /// (negativo: sin puerta). Se llama «Muro_…»: la vista de corte lo recoge y el validador lo cuenta como muro.</summary>
    internal static MicroDoor Partition(Transform b, string name, Vector2 from, Vector2 to, float h, Color wall, float doorAt, float doorRealW, string doorSource)
    {
        Material mw = AreaMicrocosmos.Mat("Civ_Muro_" + Hex(wall), wall);
        Vector2 d = to - from; float len = d.magnitude; if (len < 0.1f) return null;
        Vector2 u = d / len; float yaw = Mathf.Atan2(u.x, u.y) * Mathf.Rad2Deg;   // el tabique va a lo largo de su z local
        Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
        void Seg(float s0, float s1)
        {
            if (s1 - s0 < 0.05f) return;
            Vector2 c = from + u * ((s0 + s1) * 0.5f);
            AreaMicrocosmos.Box(b, "Muro_" + name, new Vector3(c.x, h * 0.5f, c.y), new Vector3(0.12f, h, s1 - s0), rot, mw, true);
        }
        if (doorAt < 0f) { Seg(0f, len); return null; }
        float w = MicroDoor.BuildWidth(doorRealW);
        Seg(0f, doorAt - w * 0.5f); Seg(doorAt + w * 0.5f, len);
        Vector2 dc = from + u * doorAt;
        MicroDoor door = MicroDoor.Place(b, "Puerta_" + name, new Vector3(dc.x, 0f, dc.y), yaw + 90f, doorRealW, Mathf.Min(2.1f, h), doorSource);
        door.height = Mathf.Max(door.height, MicroBuildRules.DoorHeight);
        DoorLink(door, 0.12f);
        return door;
    }
}
