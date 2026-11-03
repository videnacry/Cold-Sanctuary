using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MÁSCARAS Y FONDO ABIERTO DE LOS EDIFICIOS-ÁRBOL (Berón 2026-11-03; docs/mascaras-de-oxigeno.md y docs/edificio-arbol.md §Sin suelo):
/// - En cada puerta (<see cref="DoorFrame"/>): un PERCHERO de máscaras fuera (se coge al entrar al agua) y una ESTACIÓN dentro, en
///   el agua junto a la puerta (quien está en el agua sin máscara nada a la más cercana).
/// - Bajo el tronco, el HUECO en el hielo (el tronco no tiene suelo: se sale nadando al mar y entran los animales marinos), las
///   CORTINAS DE FITOPLANCTON colgadas en el hueco (las cuida el jardinero del edificio) y unos grupos de peces, kril y medusas
///   que viven debajo y suben al tronco.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color MaskYellow = new Color(0.98f, 0.78f, 0.12f), MaskVisor = new Color(0.7f, 0.9f, 1f, 0.5f), MaskStrap = new Color(0.12f, 0.12f, 0.14f);

    /// <summary>El perchero de fuera y la estación de dentro de una puerta (<paramref name="d"/>: +Z hacia fuera, y = 0 el umbral,
    /// z de 0 —cara de dentro— a <paramref name="wallThick"/> —cara de fuera—).</summary>
    static void MaskPoints(Transform d, Transform axisFrom, float w, float wallThick)
    {
        Vector3 ax = d.InverseTransformPoint(axisFrom.position);
        float sgn = ax.x >= 0f ? 1f : -1f;   // hacia el centro de la cara: lejos de las esquinas
        // ── PERCHERO (fuera) ──
        Transform rack = new GameObject("Perchero_mascaras").transform; rack.SetParent(d, false);
        rack.localPosition = new Vector3(sgn * (w * 0.5f + 0.9f), 0f, wallThick + 0.35f);
        KitchenProps.C(rack, new Vector3(-0.55f, 0.85f, 0f), new Vector3(0.08f, 1.7f, 0.08f), TreeSteel, 0f, "Poste");
        KitchenProps.C(rack, new Vector3(0.55f, 0.85f, 0f), new Vector3(0.08f, 1.7f, 0.08f), TreeSteel, 0f, "Poste");
        KitchenProps.C(rack, new Vector3(0f, 1.55f, 0f), new Vector3(1.2f, 0.06f, 0.06f), TreeSteel, 0f, "Barra");
        KitchenProps.C(rack, new Vector3(0f, 1.82f, 0f), new Vector3(1.2f, 0.22f, 0.04f), MaskYellow, 0f, "Cartel_mascaras");
        Label(KitchenProps.C(rack, new Vector3(0f, 2.1f, 0f), Vector3.one * 0.05f, MaskYellow, 0f, "Rotulo"), "Máscaras: cógela al entrar al agua y déjala al salir", 0.5f);
        MaskStation rs = rack.gameObject.AddComponent<MaskStation>(); rs.kind = MaskStation.Kind.Rack; rs.capacity = 8;
        for (int i = 0; i < 8; i++) HangingMask(rack, new Vector3(-0.49f + i * 0.14f, 1.38f, 0.05f));
        Transform rsp = new GameObject("Donde_se_coge").transform; rsp.SetParent(rack, false); rsp.localPosition = new Vector3(0f, 0f, 0.9f);
        rs.standPoint = rsp;
        // ── ESTACIÓN (dentro, en el agua, contra la cara de dentro) ──
        Transform st = new GameObject("Estacion_mascaras").transform; st.SetParent(d, false);
        st.localPosition = new Vector3(sgn * (w * 0.5f + 0.6f), 1.2f, -0.12f);
        KitchenProps.C(st, new Vector3(0f, 0.4f, 0f), new Vector3(0.9f, 1.4f, 0.08f), MaskYellow, 0f, "Panel_estacion");
        KitchenProps.C(st, new Vector3(0f, 1.18f, -0.03f), new Vector3(0.3f, 0.12f, 0.06f), new Color(0.4f, 1f, 0.6f), 0f, "Luz_estacion");
        MaskStation ss = st.gameObject.AddComponent<MaskStation>(); ss.kind = MaskStation.Kind.Station; ss.capacity = 6;
        for (int i = 0; i < 6; i++) HangingMask(st, new Vector3(-0.25f + (i % 3) * 0.25f, 0.65f - (i / 3) * 0.45f, -0.07f));
        Transform ssp = new GameObject("Donde_se_coge").transform; ssp.SetParent(st, false); ssp.localPosition = new Vector3(0f, 0.4f, -1.2f);
        ss.standPoint = ssp;
    }

    static void HangingMask(Transform parent, Vector3 at)
    {
        Transform m = new GameObject("Mascara").transform; m.SetParent(parent, false); m.localPosition = at;
        KitchenProps.C(m, new Vector3(0f, 0f, 0f), new Vector3(0.12f, 0.08f, 0.03f), MaskVisor, 0f, "Visor");
        KitchenProps.C(m, new Vector3(0f, 0f, 0.01f), new Vector3(0.13f, 0.1f, 0.02f), MaskYellow, 0f, "Marco");
        KitchenProps.C(m, new Vector3(0f, -0.08f, 0.02f), new Vector3(0.04f, 0.05f, 0.04f), MaskStrap, 0f, "Boquilla");
        KitchenProps.C(m, new Vector3(0.05f, -0.12f, 0.03f), new Vector3(0.03f, 0.1f, 0.03f), new Color(0.9f, 0.9f, 0.92f), 0f, "Botella_O2");
    }

    /// <summary>El HUECO en el hielo bajo el tronco: los bloques de la banquisa que lo pisan se parten en los trozos de alrededor.
    /// Cuadrado y alineado con el mundo: en un árbol que GIRA, de medio lado 0,7 × el agua (siempre dentro del agua del tronco,
    /// gire como gire); en una torre (giro de 0° o 90°), casi toda el agua.</summary>
    static void CutIceUnder(Helix hx)
    {
        GameObject floes = GameObject.Find("Banquisa_Casquetes");
        float half = hx.rotates || Mathf.Abs(Mathf.Repeat(hx.yaw, 90f)) > 0.5f ? hx.h.waterHalf * 0.7f : hx.h.waterHalf - 0.2f;
        Vector3 c = hx.root.position;
        Rect hole = new Rect(c.x - half, c.z - half, 2f * half, 2f * half);
        hx.ac.bottomHole = half;
        if (floes == null) { Debug.LogWarning($"[Árbol] «{hx.building}»: no hay banquisa (Banquisa_Casquetes) para abrir el fondo."); return; }
        int cut = 0;
        var pieces = new List<Transform>();
        foreach (Transform b in floes.transform) pieces.Add(b);
        foreach (Transform b in pieces)
        {
            if (b == null || b.name != "Casquete") continue;
            Vector3 p = b.position, s = b.localScale;
            Rect r = new Rect(p.x - s.x * 0.5f, p.z - s.z * 0.5f, s.x, s.z);
            if (!r.Overlaps(hole)) continue;
            void Piece(float x0, float x1, float z0, float z1)
            {
                if (x1 - x0 < 0.05f || z1 - z0 < 0.05f) return;
                GameObject g = Object.Instantiate(b.gameObject, floes.transform);
                g.name = "Casquete";
                g.transform.position = new Vector3((x0 + x1) * 0.5f, p.y, (z0 + z1) * 0.5f);
                g.transform.localScale = new Vector3(x1 - x0, s.y, z1 - z0);
            }
            float hx0 = Mathf.Max(r.xMin, hole.xMin), hx1 = Mathf.Min(r.xMax, hole.xMax);
            Piece(r.xMin, hole.xMin, r.yMin, r.yMax);
            Piece(hole.xMax, r.xMax, r.yMin, r.yMax);
            Piece(hx0, hx1, r.yMin, hole.yMin);
            Piece(hx0, hx1, hole.yMax, r.yMax);
            Object.DestroyImmediate(b.gameObject);
            cut++;
        }
        Debug.Log($"[Árbol] «{hx.building}»: fondo abierto al mar — hueco de {2f * half:0.#} × {2f * half:0.#} m en el hielo ({cut} bloques partidos).");
    }

    /// <summary>Debajo del tronco abierto: las CORTINAS DE FITOPLANCTON (4, colgadas en el hueco, a la luz que baja por el tronco)
    /// y unos grupos de peces, kril y medusas que viven debajo del árbol y suben por el hueco.</summary>
    static void BottomLife(Helix hx, int index)
    {
        Transform root = new GameObject($"Fondo_abierto_{hx.rootName}").transform;
        root.SetParent(hx.root.parent, false); root.position = hx.root.position;
        float r = Mathf.Min(3.5f, hx.ac.bottomHole * 0.55f);
        Color green = new Color(0.25f, 0.62f, 0.32f, 0.6f);
        int n = 0;
        foreach (Vector2 o in new[] { new Vector2(r, r), new Vector2(-r, r), new Vector2(r, -r), new Vector2(-r, -r) })
        {
            Transform bed = new GameObject($"Fitoplancton_{++n}").transform; bed.SetParent(root, false);
            bed.position = hx.root.position + new Vector3(o.x, -2.2f, o.y);
            KitchenProps.C(bed, new Vector3(0f, 1.2f, 0f), new Vector3(2.4f, 0.08f, 0.08f), TreeSteel, 0f, "Barra_cortina");
            for (int i = 0; i < 4; i++) KitchenProps.C(bed, new Vector3(-0.9f + i * 0.6f, 0.2f, 0f), new Vector3(0.45f, 2f, 0.03f), green, 0f, "Cortina_fitoplancton");
            Phytoplankton ph = bed.gameObject.AddComponent<Phytoplankton>();
            ph.building = hx.building;
            PolarLandscapeBuilder.IgnoreForNav(bed.gameObject);
        }
        Vector3 under = hx.root.position + Vector3.down * 6f;
        var groups = new List<Swarm>();
        groups.Add(Swarm.Create<Swarm>(root, $"Peces_{hx.rootName}", under + new Vector3(2f, 0f, 0f), SwarmKind.Fish, SwarmModels.Species.SilverFish, 20f, 40f, 3.5f));
        groups.Add(Swarm.Create<Swarm>(root, $"Kril_{hx.rootName}", under + new Vector3(-2f, 1.5f, 1f), SwarmKind.Krill, SwarmModels.Species.Krill, 40f, 60f, 3f));
        bool big = index % 2 == 1;
        groups.Add(Swarm.Create<Swarm>(root, $"Medusas_{hx.rootName}", under + new Vector3(0f, -3f, -2f), SwarmKind.Jellyfish,
            big ? SwarmModels.Species.Desmonema : SwarmModels.Species.Diplulmaris, big ? 3f : 8f, big ? 5f : 12f, big ? 4.5f : 3f));
        foreach (Swarm g in groups) { g.wanderRadius = 6f; g.treeVisitChance = 0.5f; }   // viven bajo el árbol: suben al tronco y bajan
    }
}

public static partial class SampleSceneBuilder
{
    /// <summary>EL GARAJE DEL SUBMARINO DE OBRA (Berón 2026-11-03; docs/submarino-de-obra.md): el primer piso libre de la torre de
    /// Producción (la de Construcción) pasa a ser «Garaje de submarinos», con su plaza pintada y el submarino aparcado mirando a la
    /// puerta. Si la torre no tiene piso libre, la plaza va en el piso de Construcción.</summary>
    static void SubmarineGarage(Helix hx)
    {
        Transform deck = null;
        for (int k = 1; k <= hx.Platforms; k++) if (hx.decks[k] != null && hx.decks[k].name.StartsWith("Piso_libre_")) { deck = hx.decks[k]; break; }
        if (deck != null) deck.name = "Garaje_submarinos";
        else
        {
            for (int k = 1; k <= hx.Platforms && deck == null; k++)
                if (hx.decks[k] != null && hx.decks[k].GetComponentInChildren<SanctuaryArea>() is SanctuaryArea a && a.name.StartsWith("Construction")) deck = hx.decks[k];
            if (deck == null) { Debug.LogWarning($"[Submarino] «{hx.building}» no tiene piso libre ni de Construcción para el garaje."); return; }
        }
        float half = hx.h.floorSize * 0.5f;
        Color paint = new Color(0.98f, 0.82f, 0.1f);
        Transform g = new GameObject("Plaza_submarino").transform; g.SetParent(deck, false); g.localPosition = new Vector3(0f, 0.02f, 2f);
        float L = TreeSpec.VehicleLength + 2f, B = TreeSpec.VehicleBeam + 2f;
        foreach (float s in new[] { -1f, 1f })
        {
            KitchenProps.C(g, new Vector3(s * B * 0.5f, 0f, 0f), new Vector3(0.25f, 0.02f, L), paint, 0f, "Linea_plaza");
            KitchenProps.C(g, new Vector3(0f, 0f, s * L * 0.5f), new Vector3(B, 0.02f, 0.25f), paint, 0f, "Linea_plaza");
        }
        Label(KitchenProps.C(g, new Vector3(B * 0.5f + 1.5f, 2.5f, 0f), Vector3.one * 0.1f, paint, 0f, "Cartel_garaje"), "Garaje del submarino de obra: maquinaria y material a cualquier piso por la vía marina", 0.6f);
        GameObject sub = new GameObject("Submarino_obra");
        sub.transform.SetParent(deck, false);
        sub.transform.localPosition = new Vector3(0f, 0.05f, 2f);
        sub.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);   // mirando a la puerta (el lado −Z del piso, contra el tronco)
        sub.AddComponent<ConstructionSubmarine>();
        PolarLandscapeBuilder.IgnoreForNav(g.gameObject);
        Debug.Log($"[Submarino] Garaje en «{deck.name}» de {hx.building} (piso de {2f * half:0} m), con el submarino de obra aparcado.");
    }
}
