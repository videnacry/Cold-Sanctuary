using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS PISOS DE YOGA (Berón 2026-11-04; docs/yoga-pisos.md). «En la sala de yoga retira el anillo de yoga, pues no creo que desde el
/// anillo puedan ver ni interactuar con el profesor; en su lugar vamos a crear otro piso de yoga… hatha en un piso y ashtanga en
/// otro… la versión más occidental y la versión más conservadora, ambas con sus estatuas o elementos distintivos.»
///
/// DOS PISOS con DOS SALAS cada uno (no cuatro pisos): el piso de HATHA es el de la Sala de Yoga (YogaRoom_Area) y el de ASHTANGA
/// su planta propia («Planta_ashtanga», el siguiente piso de la torre del Entrenamiento, que sube a 4 niveles: 7 pisos y uno libre).
/// Cada piso (36 × 26 m de área) se parte en dos salas de 18 × 26: la OCCIDENTAL (−X: suelo de madera, esterillas de colores,
/// estante de accesorios, Buda en Hatha y Ganesha en Ashtanga, sin espejo) y la TRADICIONAL (+X: suelo de tierra, alfombras de
/// algodón, asiento de kusha y algodón para el maestro, el KUND para sumergirse antes de practicar —agua del tronco, sujeta por
/// el campo magnético como en sus puertas, sin escalones—, lámparas de aceite y su elemento: un linga como el Dhyanalinga en Hatha;
/// el busto de Adiyogi y Patanjali en Ashtanga). En cada sala, la tarima del maestro en el centro y sus 8 esterillas en UN anillo
/// a su alrededor (3,3–7,3 m), a ≥ 2 m entre sí: nadie queda detrás de otro alumno ni de una estatua (las estatuas, el estante y
/// el kund van en las bandas de los extremos, fuera de las líneas de visión). <see cref="YogaHall.BlockedSightLines"/> lo mide.
/// </summary>
public static partial class SampleSceneBuilder
{
    // Medidas de una sala (marco de la sala: su tarima en el origen; ±9 m en X, ±13 m en Z).
    const float YogaHallHalfX = 9f, YogaMatSideX = 5.5f, YogaMatEndZ = 5.1f, YogaHeadZ = 11f, YogaKundZ = -10.5f;
    static readonly float[] YogaMatRowsZ = { -5f, 0f, 5f };

    /// <summary>Amuebla los dos pisos de yoga: el de Hatha (<paramref name="area"/>) y el de Ashtanga (<paramref name="ashtanga"/>).</summary>
    static void FurnishYoga(Transform area, Transform ashtanga, Vector2 size)
    {
        var halls = new List<YogaHall>
        {
            BuildYogaHall(area, YogaHall.Style.Hatha, YogaHall.Version.Occidental, -YogaHallHalfX),
            BuildYogaHall(area, YogaHall.Style.Hatha, YogaHall.Version.Tradicional, YogaHallHalfX),
        };
        if (ashtanga != null)
        {
            halls.Add(BuildYogaHall(ashtanga, YogaHall.Style.Ashtanga, YogaHall.Version.Occidental, -YogaHallHalfX));
            halls.Add(BuildYogaHall(ashtanga, YogaHall.Style.Ashtanga, YogaHall.Version.Tradicional, YogaHallHalfX));
            // Sus aseos, como los del piso de Hatha: contra el borde +Z, esquina −X (en la sala occidental, junto a su estante).
            Aseos(ashtanga, new Vector3(-size.x * 0.5f + 3.1f, 0.1f, size.y * 0.5f - 0.8f), 180f);
        }
        else Debug.LogError("[Yoga] falta la planta de Ashtanga: solo se amuebla el piso de Hatha.");
        foreach (YogaHall h in halls)
        {
            int bad = h.BlockedSightLines(out string why);
            if (bad > 0) Debug.LogError($"[Yoga] {h.Label}: {bad} esterillas no ven al maestro: {why}");
            else Debug.Log($"[Yoga] {why}.");
        }
        Debug.Log($"[Yoga] Pisos de yoga: Hatha (piso de la Sala de Yoga) y Ashtanga (planta propia), {halls.Count} salas, sin anillo.");
    }

    /// <summary>Una SALA de un piso de yoga, centrada en x = <paramref name="cx"/> del piso.</summary>
    static YogaHall BuildYogaHall(Transform floor, YogaHall.Style style, YogaHall.Version ver, float cx)
    {
        bool trad = ver == YogaHall.Version.Tradicional, ash = style == YogaHall.Style.Ashtanga;
        Transform h = new GameObject($"Sala_{style}_{(trad ? "tradicional" : "occidental")}").transform;
        h.SetParent(floor, false); h.localPosition = new Vector3(cx, 0.1f, 0f);
        YogaHall hall = h.gameObject.AddComponent<YogaHall>();
        hall.style = style; hall.version = ver;
        hall.roomTempC = ash ? (trad ? 28f : 26f) : (trad ? 25f : 22f);   // valores de diseño: el Ashtanga, más cálido (se suda)
        WorldLabel hl = h.gameObject.AddComponent<WorldLabel>();
        hl.text = $"{style} · {(trad ? "tradicional" : "occidental")}"; hl.height = 3.4f;

        // EL SUELO: madera clara (occidental) o tierra apisonada (tradicional). Un «Suelo»: no estorba a las puertas ni a la vista.
        Color wood = new Color(0.8f, 0.65f, 0.45f), earth = new Color(0.5f, 0.38f, 0.26f);
        KitchenProps.C(h, new Vector3(0f, 0.003f, 0f), new Vector3(2f * YogaHallHalfX - 0.4f, 0.006f, 25.6f), trad ? earth : wood, 0f, trad ? "Suelo_tierra" : "Suelo_madera");
        if (!trad) for (float x = -YogaHallHalfX + 1.2f; x < YogaHallHalfX - 0.5f; x += 1.2f)
                KitchenProps.C(h, new Vector3(x, 0.0065f, 0f), new Vector3(0.03f, 0.002f, 25.6f), new Color(0.64f, 0.52f, 0.36f), 0f, "Suelo_junta");

        // LAS ESTERILLAS (alfombras-esterilla): 8 en un solo anillo alrededor de la tarima, de cara a ella. Su red es la de la sala
        // (no vuelan a otros pisos: en el edificio-árbol se nada).
        string building = BuildingOf(floor, "Sala de Yoga"); int fi = FloorIndex(floor);
        string net = $"{building} (esterillas de {hall.Label})", surf = $"{building}#{fi}", label = FloorName(floor, fi, building);
        var spots = new List<Vector3>();
        foreach (float z in YogaMatRowsZ) { spots.Add(new Vector3(-YogaMatSideX, 0f, z)); spots.Add(new Vector3(YogaMatSideX, 0f, z)); }
        spots.Add(new Vector3(0f, 0f, YogaMatEndZ)); spots.Add(new Vector3(0f, 0f, -YogaMatEndZ));
        int k = 0;
        foreach (Vector3 p in spots)
        {
            Vector3 toC = -p; toC.y = 0f;
            Quaternion face = Quaternion.LookRotation(Mathf.Abs(p.x) > 0.1f ? new Vector3(toC.x, 0f, 0f) : new Vector3(0f, 0f, toC.z));
            CarpetPad pad = CarpetPad.BuildZone(h, h.TransformPoint(p + Vector3.up * 0.01f), h.rotation * face, CarpetPad.Kind.Edificio, net, fi, surf, label, 1, 3f, 3.6f);
            pad.name = $"Esterilla_{++k}";
            if (trad) DurrieLook(pad.transform);
        }

        // LA TARIMA DEL MAESTRO, en el centro (se sube a ella: con collider).
        Transform tar = KitchenProps.C(h, new Vector3(0f, 0.2f, 0f), new Vector3(2.6f, 0.4f, 2.6f), trad ? new Color(0.58f, 0.5f, 0.4f) : new Color(0.6f, 0.5f, 0.42f), 0f, "Tarima del maestro");
        tar.gameObject.AddComponent<BoxCollider>();
        hall.tarima = tar;
        if (trad)
        {
            // Su asiento (Bhagavad Gita 6.11: hierba kusha y, encima, tela; sin la piel de ciervo: es un santuario de animales).
            KitchenProps.C(h, new Vector3(0f, 0.415f, 0f), new Vector3(0.95f, 0.03f, 0.95f), new Color(0.74f, 0.7f, 0.42f), 0f, "Asiento_kusha");
            KitchenProps.C(h, new Vector3(0f, 0.435f, 0f), new Vector3(0.8f, 0.012f, 0.8f), new Color(0.95f, 0.93f, 0.86f), 0f, "Tela_algodon");
        }
        else KitchenProps.C(h, new Vector3(0f, 0.41f, 0f), new Vector3(0.8f, 0.02f, 2f), new Color(0.55f, 0.35f, 0.6f), 0f, "Esterilla del maestro");
        KitchenProps.C(h, new Vector3(1f, 0.5f, 0.9f), new Vector3(0.25f, 0.2f, 0.25f), new Color(0.85f, 0.7f, 0.35f), 0f, "Cuenco tibetano");

        // LO DISTINTIVO, en la banda de cabecera (+Z, a 11 m: fuera de las líneas de visión), de cara a la tarima.
        Transform head = new GameObject("Cabecera").transform; head.SetParent(h, false);
        if (trad)
        {
            if (!ash) Furniture(YogaStatues.Linga(head, new Vector3(0f, 0f, YogaHeadZ - 0.4f)));
            else
            {
                Furniture(YogaStatues.PatanjaliStatue(head, new Vector3(-3f, 0f, YogaHeadZ)));
                Furniture(YogaStatues.AdiyogiBust(head, new Vector3(1.5f, 0f, YogaHeadZ)));
            }
            // Lámparas de aceite en arco delante (a ras del suelo: no tapan nada) y su luz cálida.
            for (int a = -3; a <= 3; a++)
            {
                float ang = a * 20f * Mathf.Deg2Rad, r = 2.2f;
                YogaStatues.OilLamp(head, new Vector3((ash ? -0.75f : 0f) + Mathf.Sin(ang) * r * 1.4f, 0f, YogaHeadZ - 0.4f - Mathf.Cos(ang) * r * 0.8f));
            }
            Light l = new GameObject("Luz_lamparas").AddComponent<Light>(); l.transform.SetParent(head, false);
            l.transform.localPosition = new Vector3(0f, 1.2f, YogaHeadZ - 1.8f);
            l.type = LightType.Point; l.color = new Color(1f, 0.7f, 0.4f); l.range = 7f; l.intensity = 1.4f; l.shadows = LightShadows.None;
            BuildKund(h, cx);
        }
        else
        {
            Furniture(ash ? YogaStatues.Ganesha(head, new Vector3(0f, 0f, YogaHeadZ)) : YogaStatues.BuddhaStatue(head, new Vector3(0f, 0f, YogaHeadZ)));
            PropsShelf(head, new Vector3(4f, 0f, YogaHeadZ), ash);
        }
        foreach (Transform s in head)
            if (s.name.StartsWith("Estatua") || s.name.StartsWith("Busto") || s.name.StartsWith("Linga")) s.localRotation = Quaternion.Euler(0f, 180f, 0f);   // de cara a la tarima
        return hall;
    }

    /// <summary>Un mueble entero (su caja de colisión envuelve las piezas): guarda los 2 m con los demás y el tanque no se le pone encima.</summary>
    static Transform Furniture(Transform t) { KitchenProps.Fit(t); return t; }

    /// <summary>Alfombra de algodón (durrie) en vez de la de colores: crudo con franjas índigo.</summary>
    static void DurrieLook(Transform pad)
    {
        foreach (Renderer r in pad.GetComponentsInChildren<Renderer>())
        {
            if (r.name == "Tejido") r.sharedMaterial = KitchenProps.Mat(new Color(0.93f, 0.9f, 0.82f));
            else if (r.name == "Dibujo") r.sharedMaterial = KitchenProps.Mat(new Color(0.2f, 0.27f, 0.55f));
        }
    }

    /// <summary>ESTANTE DE ACCESORIOS (occidental): bloques de corcho, cinturones, bolsters y mantas (Hatha); en Ashtanga, las
    /// alfombras de algodón de Mysore enrolladas y toallas.</summary>
    static void PropsShelf(Transform parent, Vector3 pos, bool ashtanga)
    {
        Color oak = new Color(0.62f, 0.48f, 0.32f);
        Transform r = KitchenProps.Root(parent, "Estante_accesorios", pos, 180f);
        KitchenProps.C(r, new Vector3(0f, 0.6f, 0.25f), new Vector3(2.4f, 1.2f, 0.06f), oak, 0f, "Fondo");
        for (int i = 0; i < 3; i++) KitchenProps.C(r, new Vector3(0f, 0.05f + i * 0.5f, 0f), new Vector3(2.4f, 0.04f, 0.56f), oak, 0f, "Balda");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(r, new Vector3(s * 1.18f, 0.6f, 0f), new Vector3(0.04f, 1.2f, 0.56f), oak, 0f, "Lateral");
        if (!ashtanga)
        {
            for (int b = 0; b < 8; b++) KitchenProps.C(r, new Vector3(-1f + b * 0.26f, 0.15f, 0f), new Vector3(0.23f, 0.15f, 0.1f), new Color(0.78f, 0.62f, 0.42f), 0f, "Bloque_corcho");
            for (int b = 0; b < 3; b++) KitchenProps.C(r, new Vector3(-0.7f + b * 0.7f, 0.66f, 0f), new Vector3(0.6f, 0.2f, 0.22f), new Color(0.25f, 0.35f, 0.55f), 0f, "Bolster");
            for (int b = 0; b < 4; b++) KitchenProps.C(r, new Vector3(-0.9f + b * 0.6f, 1.12f, 0f), new Vector3(0.5f, 0.1f, 0.4f), new Color(0.75f, 0.3f, 0.25f), 0f, "Manta");
            for (int b = 0; b < 4; b++) KitchenProps.C(r, new Vector3(0.6f + b * 0.12f, 0.6f, -0.2f), new Vector3(0.04f, 0.5f, 0.03f), new Color(0.2f, 0.2f, 0.25f), 0f, "Cinturon");
        }
        else
        {
            for (int b = 0; b < 7; b++) KitchenProps.C(r, new Vector3(-1f + b * 0.33f, 0.2f, 0f), new Vector3(0.14f, 0.14f, 0.5f), b % 2 == 0 ? new Color(0.93f, 0.9f, 0.82f) : new Color(0.2f, 0.27f, 0.55f), 0f, "Alfombra_Mysore");
            for (int b = 0; b < 5; b++) KitchenProps.C(r, new Vector3(-0.9f + b * 0.45f, 0.66f, 0f), new Vector3(0.4f, 0.12f, 0.35f), Color.white, 0f, "Toalla");
        }
        WorldLabel wl = r.gameObject.AddComponent<WorldLabel>(); wl.text = ashtanga ? "Alfombras de Mysore y toallas" : "Accesorios (bloques, cinturones, bolsters, mantas)"; wl.height = 1.7f;
        KitchenProps.Fit(r);
    }

    /// <summary>EL KUND (sala tradicional): un estanque de 6 × 3 m en la banda del lado del tronco (−Z), a ras del suelo: su agua
    /// (0,9 m) la trae del tronco un canal por el suelo y la SUJETA EL CAMPO MAGNÉTICO entre cuatro bobinas de cobre, como en las
    /// puertas del árbol (docs/edificio-arbol.md): se entra andando, sin escalones. Antes de la práctica, cada alumno se sienta en
    /// el agua <see cref="YogaHall.ImmersionSeconds"/> s (como en los Theerthakund de Isha, antes del Dhyanalinga).</summary>
    static void BuildKund(Transform hallT, float cx)
    {
        Transform k = KitchenProps.Root(hallT, "Kund", new Vector3(0f, 0f, YogaKundZ));
        hallT.GetComponent<YogaHall>().kund = k;
        Color stone = new Color(0.6f, 0.56f, 0.5f);
        KitchenProps.C(k, new Vector3(0f, 0.004f, 0f), new Vector3(6.6f, 0.008f, 3.6f), stone, 0f, "Suelo_borde_kund");
        Transform water = KitchenProps.C(k, new Vector3(0f, 0.45f, 0f), new Vector3(6f, 0.9f, 3f), TreeWaterColor, 0f, "Agua_kund");
        Renderer wr = water.GetComponent<Renderer>(); wr.sharedMaterial = TreeWaterMat; wr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        water.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;   // se anda por el suelo de debajo
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
                KitchenProps.C(k, new Vector3(sx * 3.1f, 0.5f, sz * 1.6f), new Vector3(0.18f, 1f, 0.18f), TreeCopper, 0f, "Bobina_campo");
        // El CANAL del tronco: sale de la cara del tronco (a 18 m del centro del piso, por −Z) en x = 3 del piso, dentro de la cara,
        // y llega al kund por el suelo (plano: no estorba a la puerta ni a nadie).
        WorldLabel wl = k.gameObject.AddComponent<WorldLabel>(); wl.text = "Kund: sumergirse antes de practicar (agua del tronco)"; wl.height = 2f;
        KitchenProps.Fit(k, solid: false);   // un mueble para los 2 m y el tanque, pero se entra andando (caja de disparo)
        // (El canal cuelga de la sala, no del kund: no agranda su caja.) Marco de la sala: la cara del tronco está en z = −18 del piso.
        float x0 = 3f - cx, zFace = -18f, xEdge = -3.3f;
        Transform c1 = KitchenProps.C(hallT, new Vector3(x0, 0.01f, (zFace + YogaKundZ) * 0.5f), new Vector3(0.6f, 0.02f, YogaKundZ - zFace + 0.3f), TreeWaterColor, 0f, "Suelo_canal_kund");
        Transform c2 = KitchenProps.C(hallT, new Vector3((x0 + xEdge) * 0.5f, 0.01f, YogaKundZ), new Vector3(Mathf.Abs(xEdge - x0) + 0.3f, 0.02f, 0.6f), TreeWaterColor, 0f, "Suelo_canal_kund");
        foreach (Transform c in new[] { c1, c2 }) { Renderer r = c.GetComponent<Renderer>(); r.sharedMaterial = TreeWaterMat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; }
    }
}
