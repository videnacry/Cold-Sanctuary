using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OBRAS CON SU ACABADO YA DISEÑADO (Berón 2026-10-28; docs/obras-dojo-ballet.md): el Dojo (MMA, kickboxing y karate) y
/// la Sala de Ballet siguen EN OBRAS, pero lo que quedará al terminar ya está montado dentro, en un «Acabado» oculto que
/// <see cref="ConstructionProject"/> muestra el día que se completa. Cada obra pide su EQUIPO: el día cuenta cuando han
/// trabajado todas sus personas (una lleva el material, dos trabajan).
/// Coordenadas locales del piso de 18 × 15 m: origen en el centro, −Z = el frente (las alfombras, en (−5,2, −5,3)).
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color OHemp = new Color(0.78f, 0.72f, 0.56f), OCork = new Color(0.66f, 0.52f, 0.36f), OGuadua = new Color(0.78f, 0.74f, 0.42f),
        OMycel = new Color(0.93f, 0.9f, 0.82f), ORed = new Color(0.72f, 0.2f, 0.18f), OBlue = new Color(0.22f, 0.36f, 0.66f), OMirror = new Color(0.78f, 0.86f, 0.92f);

    /// <summary>Obra con equipo y acabado: el andamio y el material del día, donde no estorban al acabado (§3.4).</summary>
    static void PrepareSite(Transform area, Vector2 size, string title, int days, int crew, System.Action<Transform> finish)
    {
        MakeConstructionSite(area, size, title, days);
        Transform scaf = area.Find("Andamio_obra"), mat = area.Find("Material_del_dia");
        if (scaf != null) scaf.localPosition = new Vector3(size.x * 0.5f - 1.4f, 0.1f, -size.y * 0.5f + 1f);   // fuera de la zona de alfombras
        if (mat != null) mat.localPosition = new Vector3(0.9f, 0.1f, -5f);
        ConstructionProject cp = area.GetComponent<ConstructionProject>(); cp.crewNeeded = crew;
        Transform done = new GameObject("Acabado").transform; done.SetParent(area, false); done.localPosition = Vector3.up * 0.1f;
        finish(done);
        done.gameObject.SetActive(false);   // aparece al terminar la obra
    }

    /// <summary>DOJO de entrenamiento (§1.2): jaula de MMA de 18 ft, ring de kickboxing de 5,2 m entre cuerdas, tatami de
    /// karate de 8 × 8, sacos, makiwaras, equipo, botiquín y vestuario. Los tamaños de competición no caben juntos en 270 m².</summary>
    static void FinishedDojo(Transform r)
    {
        KitchenProps.C(r, new Vector3(4.3f, 0.025f, 0f), new Vector3(9.4f, 0.05f, 14.8f), OCork, 0f, "Base_corcho");   // suelo de impacto bajo jaula y ring
        // JAULA DE MMA: octógono de 5,5 m entre caras, a ras de suelo, con red de cáñamo sobre postes de guadua doble.
        Transform cage = KitchenProps.Root(r, "Jaula_MMA", new Vector3(5.8f, 0.05f, -4.6f));
        float apo = 2.75f, side = 2f * apo * Mathf.Tan(Mathf.PI / 8f);
        for (int k = 0; k < 2; k++) KitchenProps.C(cage, new Vector3(0f, 0.04f, 0f), new Vector3(4.2f, 0.08f, 4.2f), OHemp, 22.5f + k * 45f, "Lona");   // dos cuadrados inscritos (sus puntas, en los postes): la lona no sobresale de la red
        for (int i = 0; i < 8; i++)
        {
            float a = i * 45f, ar = a * Mathf.Deg2Rad, pr = (a + 22.5f) * Mathf.Deg2Rad, pd = apo / Mathf.Cos(Mathf.PI / 8f);
            if (i != 4) KitchenProps.C(cage, new Vector3(Mathf.Sin(ar) * apo, 0.95f, Mathf.Cos(ar) * apo), new Vector3(side, 1.8f, 0.04f), new Color(0.6f, 0.56f, 0.44f, 1f), a, "Red_canamo");   // la cara −X es la puerta
            KitchenProps.C(cage, new Vector3(Mathf.Sin(pr) * pd, 0.95f, Mathf.Cos(pr) * pd), new Vector3(0.14f, 1.9f, 0.14f), OGuadua, 0f, "Poste_guadua");
        }
        cage.gameObject.AddComponent<WorldLabel>().text = "Jaula de MMA (octógono de 18 ft)";
        // RING DE KICKBOXING: 6,2 m con delantal, 5,2 m entre cuerdas, 4 cuerdas de cáñamo.
        Transform ring = KitchenProps.Root(r, "Ring_kickboxing", new Vector3(5.5f, 0.05f, 3.6f));
        KitchenProps.C(ring, new Vector3(0f, 0.15f, 0f), new Vector3(6.2f, 0.3f, 6.2f), OHemp, 0f, "Lona_ring");
        for (int sx = -1; sx <= 1; sx += 2) for (int sz = -1; sz <= 1; sz += 2)
            KitchenProps.C(ring, new Vector3(sx * 2.6f, 1.0f, sz * 2.6f), new Vector3(0.16f, 1.4f, 0.16f), sx * sz > 0 ? ORed : OBlue, 0f, "Esquina");
        for (int k = 0; k < 4; k++)
        {
            float y = 0.6f + k * 0.3f;
            KitchenProps.C(ring, new Vector3(0f, y, 2.6f), new Vector3(5.2f, 0.04f, 0.04f), OHemp, 0f, "Cuerda");
            KitchenProps.C(ring, new Vector3(0f, y, -2.6f), new Vector3(5.2f, 0.04f, 0.04f), OHemp, 0f, "Cuerda");
            KitchenProps.C(ring, new Vector3(2.6f, y, 0f), new Vector3(0.04f, 0.04f, 5.2f), OHemp, 0f, "Cuerda");
            KitchenProps.C(ring, new Vector3(-2.6f, y, 0f), new Vector3(0.04f, 0.04f, 5.2f), OHemp, 0f, "Cuerda");
        }
        ring.gameObject.AddComponent<WorldLabel>().text = "Ring de kickboxing (5,2 m entre cuerdas)";
        // TATAMI DE KARATE: 8 × 8, cuadro de 6 × 6 y franja de 1 m de otro color (también tatami abierto: grappling, kata).
        Transform tat = KitchenProps.Root(r, "Tatami_karate", new Vector3(-4.6f, 0.05f, 3.1f));
        KitchenProps.C(tat, new Vector3(0f, 0.03f, 0f), new Vector3(8f, 0.06f, 8f), ORed, 0f, "Franja");
        KitchenProps.C(tat, new Vector3(0f, 0.035f, 0f), new Vector3(6f, 0.06f, 6f), OBlue, 0f, "Area_combate");
        tat.gameObject.AddComponent<WorldLabel>().text = "Tatami de karate (8 × 8)";
        // SACOS PESADOS colgados de un pórtico de guadua a 3 m.
        Transform bags = KitchenProps.Root(r, "Sacos_pesados", new Vector3(-6.5f, 0.05f, -5.2f));
        KitchenProps.C(bags, new Vector3(0f, 3f, 0f), new Vector3(0.12f, 0.12f, 4.2f), OGuadua, 0f, "Portico");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(bags, new Vector3(0f, 1.5f, s * 2.1f), new Vector3(0.12f, 3f, 0.12f), OGuadua, 0f, "Pata");
        for (int i = -1; i <= 1; i++)
        {
            KitchenProps.C(bags, new Vector3(0f, 1.55f, i * 1.6f), new Vector3(0.35f, 1.2f, 0.35f), new Color(0.42f, 0.32f, 0.22f), 0f, "Saco");
            KitchenProps.C(bags, new Vector3(0f, 2.55f, i * 1.6f), new Vector3(0.02f, 0.9f, 0.02f), OHemp, 0f, "Cadena");
        }
        // MAKIWARAS y estantería de equipo junto al pasillo central; el botiquín pegado al vestuario, en la esquina suroeste (con los
        // sacos: 2 m libres entre los muebles, docs/obras-dojo-ballet.md).
        for (int i = 0; i < 2; i++) KitchenProps.C(r, new Vector3(0.3f + i * 1.2f, 0.6f, 4.3f), new Vector3(0.2f, 1.2f, 0.1f), OGuadua, 0f, "Makiwara");
        Transform shelf = KitchenProps.C(r, new Vector3(-0.3f, 0.9f, 2f), new Vector3(0.4f, 1.8f, 4f), OGuadua, 0f, "Estanteria_equipo");
        shelf.gameObject.AddComponent<WorldLabel>().text = "Guantes, espinilleras, paos, cascos y protectores";
        Transform aid = KitchenProps.C(r, new Vector3(-2.2f, 0.4f, -4.9f), new Vector3(2.6f, 0.8f, 1.2f), new Color(0.9f, 0.92f, 0.9f), 0f, "Primeros_auxilios");
        aid.gameObject.AddComponent<WorldLabel>().text = "Primeros auxilios";
        Transform lock1 = KitchenProps.C(r, new Vector3(-2.2f, 1f, -6.5f), new Vector3(2.6f, 2f, 2f), OMycel, 0f, "Vestuario");
        lock1.gameObject.AddComponent<WorldLabel>().text = "Vestuario";
        // PAREDES ACOLCHADAS (micelio forrado de lona de cáñamo) donde tatami y ring quedan a menos de 1 m de la pared.
        KitchenProps.C(r, new Vector3(-8.9f, 1f, 3.1f), new Vector3(0.12f, 2f, 8f), OHemp, 0f, "Acolchado");
        KitchenProps.C(r, new Vector3(-4.6f, 1f, 7.4f), new Vector3(8f, 2f, 0.12f), OHemp, 0f, "Acolchado");
        KitchenProps.C(r, new Vector3(8.9f, 1f, 3.6f), new Vector3(0.12f, 2f, 6.2f), OHemp, 0f, "Acolchado");
    }

    /// <summary>SALA DE BALLET (§2.2): suelo flotante de 18 × 8,6 m (16–23 alumnos), pared de espejo al fondo, barras dobles
    /// a 0,85 y 1,10 m, piano, sonido, resina, fuente, vestuario y banco de calzado.</summary>
    static void FinishedBallet(Transform r)
    {
        KitchenProps.C(r, new Vector3(0f, 0.05f, 3.2f), new Vector3(18f, 0.1f, 8.6f), new Color(0.82f, 0.74f, 0.6f), 0f, "Suelo_flotante");   // corcho, guadua y linóleo natural
        Transform mirror = KitchenProps.C(r, new Vector3(0f, 1.35f, 7.45f), new Vector3(15.6f, 2.4f, 0.04f), OMirror, 0f, "Pared_espejo");
        mirror.gameObject.AddComponent<WorldLabel>().text = "Espejo (13 paneles con lámina de seguridad)";
        foreach (float h in new[] { 0.85f, 1.1f })
        {
            KitchenProps.C(r, new Vector3(0f, h, 7.3f), new Vector3(15.6f, 0.045f, 0.045f), OGuadua, 0f, "Barra");
            for (int s = -1; s <= 1; s += 2) KitchenProps.C(r, new Vector3(s * 8.8f, h, 3f), new Vector3(0.045f, 0.045f, 7.2f), OGuadua, 0f, "Barra");
        }
        for (float x = -7.8f; x <= 7.81f; x += 1.3f) KitchenProps.C(r, new Vector3(x, 0.55f, 7.4f), new Vector3(0.04f, 1.1f, 0.2f), OGuadua, 0f, "Soporte_barra");
        for (int i = 0; i < 4; i++) KitchenProps.C(r, new Vector3(3f, 0.5f + i * 0.08f, -5.5f), new Vector3(2.4f, 0.06f, 0.4f), OGuadua, 0f, "Barra_portatil");
        Transform piano = KitchenProps.C(r, new Vector3(-6f, 0.65f, -2.3f), new Vector3(1.5f, 1.3f, 0.6f), new Color(0.15f, 0.12f, 0.1f), 0f, "Piano_vertical");
        piano.gameObject.AddComponent<WorldLabel>().text = "Piano";
        KitchenProps.C(r, new Vector3(-2.8f, 0.4f, -2.3f), new Vector3(0.8f, 0.8f, 0.4f), new Color(0.2f, 0.2f, 0.22f), 0f, "Equipo_sonido");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(r, new Vector3(s * 8.5f, 2.5f, 7f), new Vector3(0.3f, 0.5f, 0.3f), new Color(0.2f, 0.2f, 0.22f), 0f, "Altavoz");
        KitchenProps.C(r, new Vector3(0.5f, 0.15f, -2.3f), new Vector3(0.6f, 0.3f, 0.6f), OGuadua, 0f, "Caja_resina");
        KitchenProps.C(r, new Vector3(3.5f, 0.5f, -2.3f), new Vector3(0.6f, 1f, 0.6f), OMycel, 0f, "Fuente");
        Transform room = KitchenProps.C(r, new Vector3(7.2f, 1f, -5.9f), new Vector3(3.6f, 2f, 3.2f), OMycel, 0f, "Vestuario_botiquin");
        room.gameObject.AddComponent<WorldLabel>().text = "Vestuario y botiquín";
        KitchenProps.C(r, new Vector3(-1f, 0.22f, -7f), new Vector3(3.6f, 0.44f, 0.5f), OGuadua, 0f, "Banco_calzado");
    }

    /// <summary>Dónde poner el TANQUE de virtualización de un área (local): junto al borde, empezando por la esquina +X +Z,
    /// el primer punto que deja 2 m libres hasta cualquier mueble (FurnitureSpacing); si no hay, el más holgado.</summary>
    static Vector3 TankSpot(Transform area, Vector2 sz)
    {
        const float radius = 1.6f, inset = 1.9f;   // medio tanque (con su puerta) y lo que se aparta del borde
        var items = FurnitureSpacing.Items(area);
        float hx = sz.x * 0.5f - inset, hz = sz.y * 0.5f - inset;
        Vector3 best = new Vector3(hx, 1f, hz); float bestFree = float.MinValue;
        var ring = new List<Vector3>();
        for (float x = hx; x >= -hx; x -= 1f) ring.Add(new Vector3(x, 1f, hz));    // fondo (+Z), de +X a −X
        for (float z = hz; z >= -hz; z -= 1f) ring.Add(new Vector3(hx, 1f, z));    // lado +X
        for (float z = hz; z >= -hz; z -= 1f) ring.Add(new Vector3(-hx, 1f, z));   // lado −X
        for (float x = hx; x >= -hx; x -= 1f) ring.Add(new Vector3(x, 1f, -hz));   // frente (−Z)
        foreach (Vector3 l in ring)
        {
            Vector3 w = area.TransformPoint(l);
            float free = float.MaxValue;
            foreach (var (_, b) in items) free = Mathf.Min(free, FurnitureSpacing.DistXZ(b, w) - radius);
            foreach (CarpetPad cp in area.GetComponentsInChildren<CarpetPad>()) free = Mathf.Min(free, Vector3.Distance(new Vector3(cp.transform.position.x, w.y, cp.transform.position.z), w) - radius - 4f);
            if (free >= FurnitureSpacing.Standard) return l;
            if (free > bestFree) { bestFree = free; best = l; }
        }
        Debug.LogWarning($"[Tanque] {area.name}: no hay sitio con 2 m libres; va donde más holgura hay ({bestFree:0.0} m).");
        return best;
    }
}
