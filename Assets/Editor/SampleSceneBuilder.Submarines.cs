using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS SUBMARINOS DEL HOSPITAL (Berón 2026-11-03; docs/submarinos-medicos.md):
/// - el SUBMARINO-AMBULANCIA anfibio (<see cref="HospitalSubmarine"/>): 14 × 5 × 5 m (el vehículo de diseño de los árboles), con orugas para el hielo y hélices para el
///   mar; cabe por las puertas de los edificios-árbol y sube nadando por el tronco a la puerta del piso del hospital. Aparcado
///   junto a la puerta del suelo del árbol del hospital (a un lado del paso).
/// - el LEVIATÁN (<see cref="WhaleHospitalSubmarine"/>): 72 × 18 × 12 m, hospital de animales gigantes con su pozo médico de
///   32 × 10 m y suelo elevable; atracado en superficie junto a una orilla («a ras del suelo»), con su pasarela al hielo.
/// Son vehículos: sin colisiones (se mueven ellos) y fuera del NavMesh.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color SubWhite = new Color(0.93f, 0.94f, 0.95f), SubOrange = new Color(0.95f, 0.5f, 0.15f), SubBlue = new Color(0.18f, 0.42f, 0.78f),
                          SubDark = new Color(0.22f, 0.25f, 0.3f), SubHull = new Color(0.28f, 0.34f, 0.42f);

    static void BuildMedicalSubmarines(Transform root)
    {
        Transform amb = BuildAmbulanceSub(root);
        Transform lev = BuildLeviathan(root);
        foreach (Transform t in new[] { amb, lev })
            if (t != null) PolarLandscapeBuilder.IgnoreForNav(t.gameObject);
    }

    // ── EL SUBMARINO-AMBULANCIA ──────────────────────────────────────────────────────────────────────────────

    static Transform BuildAmbulanceSub(Transform root)
    {
        // Su sitio: junto a la puerta del suelo del árbol del hospital (el de los puestos de urgencias), a un lado del paso.
        ArbolCampos ac = null; TreeLevel ground = null;
        foreach (Helix hx in _helixes)
        {
            if (hx.ac == null) continue;
            foreach (HospitalStation s in hx.root.GetComponentsInChildren<HospitalStation>()) if (s.service == HospitalService.Urgencias) { ac = hx.ac; break; }
            if (ac != null) break;
        }
        if (ac == null) { Debug.LogWarning("[Submarino] No encuentro el árbol del hospital (sin puestos de urgencias en un árbol): no se construye."); return null; }
        ground = ac.Ground;
        Vector3 gOut = ac.World(ground.outside), gDoor = ac.World(ground.door);
        Vector3 outDir = gOut - gDoor; outDir.y = 0f; outDir.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, outDir);
        float L = HospitalSubmarine.Length, B = HospitalSubmarine.Beam, H = HospitalSubmarine.Height;
        Vector3 park = gOut + outDir * (L * 0.5f + 3f) + right * (B + 6f);
        park.y = gDoor.y + H * 0.5f + 0.2f;
        Transform t = new GameObject("Submarino_ambulancia").transform;
        t.SetParent(root, false);
        t.SetPositionAndRotation(park, Quaternion.LookRotation(-outDir));
        HospitalSubmarine hs = t.gameObject.AddComponent<HospitalSubmarine>();
        hs.parkPos = park; hs.parkRot = t.rotation; hs.building = ac.building;

        // CASCO: blanco con franja naranja; la proa redondeada (+Z), la popa con la rampa (−Z).
        float hz = L * 0.5f;
        KitchenProps.C(t, new Vector3(0f, -1.25f, -0.5f), new Vector3(B - 0.4f, 1.6f, L - 3f), SubWhite, 0f, "Casco_bajo");
        KitchenProps.C(t, new Vector3(0f, 1.95f, -0.5f), new Vector3(B - 0.4f, 0.5f, L - 3f), SubWhite, 0f, "Casco_techo");
        for (int s = -1; s <= 1; s += 2)
        {
            // Costados: cristal de la bodega (se ve dentro) y casco opaco delante (cabina) y detrás.
            KitchenProps.C(t, new Vector3(s * (B * 0.5f - 0.25f), 0.35f, -2f), new Vector3(0.08f, 2.7f, HospitalSubmarine.Bay.z), VirtualizationTank.GlassColor, 0f, "Ventanal_bodega");
            KitchenProps.C(t, new Vector3(s * (B * 0.5f - 0.25f), 0.35f, 4.5f), new Vector3(0.12f, 2.7f, 4.6f), SubWhite, 0f, "Costado");
            KitchenProps.C(t, new Vector3(s * (B * 0.5f - 0.24f), 0.95f, -0.5f), new Vector3(0.1f, 0.3f, L - 3f), SubOrange, 0f, "Franja");
            // Orugas (hielo) a cada lado.
            KitchenProps.C(t, new Vector3(s * (B * 0.5f - 0.35f), -2.05f, -0.5f), new Vector3(0.7f, 0.9f, L - 2.2f), SubDark, 0f, "Oruga");
            for (int k = 0; k < 6; k++) KitchenProps.C(t, new Vector3(s * (B * 0.5f - 0.35f), -2.05f, -6.8f + k * 2.5f), new Vector3(0.75f, 0.7f, 0.7f), KitchenProps.Steel, 0f, "Rueda_oruga");
        }
        // Proa: tres escalones redondeando (+Z), con su ventanal de cabina.
        KitchenProps.C(t, new Vector3(0f, -0.2f, hz - 1.1f), new Vector3(B - 1.2f, 3.6f, 1.6f), SubWhite, 0f, "Proa");
        KitchenProps.C(t, new Vector3(0f, -0.2f, hz - 0.3f), new Vector3(B - 2.4f, 2.6f, 0.6f), SubWhite, 0f, "Proa");
        KitchenProps.C(t, new Vector3(0f, 0.6f, hz - 1.05f), new Vector3(B - 1.6f, 1.1f, 0.1f), VirtualizationTank.GlassColor, 0f, "Ventanal_cabina");
        // Vela (torreta) con el faro azul y la estrella de la vida.
        KitchenProps.C(t, new Vector3(0f, 2.75f, 3.2f), new Vector3(1.6f, 1.1f, 3f), SubWhite, 0f, "Vela");
        KitchenProps.C(t, new Vector3(0f, 3.4f, 3.6f), new Vector3(0.5f, 0.2f, 0.5f), SubBlue, 0f, "Faro_azul");
        for (int k = 0; k < 3; k++) KitchenProps.C(t, new Vector3(0.81f, 2.75f, 3.2f), new Vector3(0.02f, 0.9f, 0.25f), SubBlue, new Vector3(k * 60f, 0f, 0f), "Estrella_de_la_vida");
        // Popa: rampa (abatible), hélices en sus toberas y timones.
        KitchenProps.C(t, new Vector3(0f, -0.6f, -hz + 0.15f), new Vector3(B - 1f, 3.4f, 0.2f), SubOrange, new Vector3(-12f, 0f, 0f), "Rampa_popa");
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(t, new Vector3(s * 1.6f, -1.6f, -hz - 0.2f), new Vector3(1.2f, 1.2f, 0.6f), SubDark, 0f, "Tobera");
            KitchenProps.C(t, new Vector3(s * 1.6f, -1.6f, -hz - 0.2f), new Vector3(0.15f, 1f, 0.12f), KitchenProps.Steel, new Vector3(0f, 0f, 45f), "Helice");
        }
        // CABINA: piloto y dos asientos del equipo.
        Transform cab = new GameObject("Cabina").transform; cab.SetParent(t, false); cab.localPosition = new Vector3(0f, -0.8f, 4.2f);
        for (int i = 0; i < 3; i++)
        {
            Transform seat = KitchenProps.Root(cab, i == 0 ? "Puesto_piloto" : $"Asiento_{i}", new Vector3(-1.3f + i * 1.3f, 0f, i == 0 ? 1.4f : 0f));
            KitchenProps.C(seat, new Vector3(0f, 0.25f, 0f), new Vector3(0.55f, 0.1f, 0.55f), SubBlue, 0f, "Asiento");
            KitchenProps.C(seat, new Vector3(0f, 0.6f, -0.25f), new Vector3(0.55f, 0.7f, 0.08f), SubBlue, 0f, "Respaldo");
        }
        KitchenProps.C(cab, new Vector3(0f, 0.7f, 2.1f), new Vector3(2.6f, 0.5f, 0.4f), SubDark, 0f, "Consola_navegacion");
        // BODEGA (7,5 × 3,8 × 4 m): camilla, trineo de la grúa de 8 t, oxígeno, monitor, desfibrilador, ventilador, sangre y fluidos.
        Transform bay = new GameObject("Bodega").transform; bay.SetParent(t, false); bay.localPosition = new Vector3(0f, -1.85f, -2f);
        Vector3 BY = HospitalSubmarine.Bay;
        KitchenProps.C(bay, new Vector3(0f, -0.05f, 0f), new Vector3(BY.x, 0.1f, BY.z), new Color(0.75f, 0.85f, 0.85f), 0f, "Suelo_bodega");
        KitchenProps.C(bay, new Vector3(0f, 0.12f, 0.2f), new Vector3(2.6f, 0.15f, 5.6f), KitchenProps.Steel, 0f, "Trineo_carga");
        KitchenProps.C(bay, new Vector3(0f, BY.y - 0.15f, 0f), new Vector3(0.25f, 0.2f, BY.z), KitchenProps.DarkSteel, 0f, "Viga_grua_8t");
        KitchenProps.C(bay, new Vector3(0f, BY.y - 0.6f, 0.8f), new Vector3(0.6f, 0.5f, 0.6f), SubOrange, 0f, "Polipasto");
        KitchenProps.C(bay, new Vector3(-1.4f, 0.55f, 2.6f), new Vector3(0.6f, 0.15f, 2.0f), KitchenProps.White, 0f, "Camilla");
        Transform wall = new GameObject("Equipos").transform; wall.SetParent(bay, false); wall.localPosition = new Vector3(1.55f, 0f, 0f);
        KitchenProps.C(wall, new Vector3(0f, 0.6f, -2.8f), new Vector3(0.5f, 1.2f, 0.5f), KitchenProps.White, 0f, "Concentrador_O2");
        for (int i = 0; i < 2; i++) KitchenProps.C(wall, new Vector3(0.1f, 0.75f, -2.2f + i * 0.35f), new Vector3(0.22f, 1.5f, 0.22f), new Color(0.95f, 0.95f, 0.95f), 0f, "Bombona_O2");
        KitchenProps.C(wall, new Vector3(0f, 1.5f, -1.2f), new Vector3(0.3f, 0.4f, 0.6f), SubDark, 0f, "Monitor_constantes");
        KitchenProps.C(wall, new Vector3(0f, 1.1f, -0.3f), new Vector3(0.3f, 0.3f, 0.4f), new Color(0.95f, 0.78f, 0.2f), 0f, "Desfibrilador");
        KitchenProps.C(wall, new Vector3(0f, 0.7f, 0.5f), new Vector3(0.45f, 1.4f, 0.4f), KitchenProps.White, 0f, "Ventilador");
        KitchenProps.C(wall, new Vector3(0f, 0.6f, 1.4f), new Vector3(0.5f, 1.2f, 0.55f), KitchenProps.White, 0f, "Nevera_sangre_O_neg");
        for (int i = 0; i < 4; i++) KitchenProps.C(wall, new Vector3(-0.26f, 0.5f + i * 0.2f, 1.4f), new Vector3(0.02f, 0.14f, 0.1f), new Color(0.62f, 0.1f, 0.17f), 0f, "Bolsa_sangre");
        KitchenProps.C(wall, new Vector3(0f, 1.6f, 2.3f), new Vector3(0.3f, 0.5f, 0.9f), new Color(0.85f, 0.92f, 0.98f), 0f, "Fluidos_y_bombas");
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>();
        wl.text = "Submarino-ambulancia del hospital (anfibio, 14 × 5 × 5 m): bodega para un oso o un elefante, O₂, monitor, desfibrilador, ventilador, sangre O−";
        wl.height = 4.2f;
        Debug.Log($"[Submarino] Ambulancia anfibia aparcada junto a la puerta del suelo de «{ac.building}» en ({park.x:0}, {park.z:0}); {L:0} × {B:0} × {H:0} m, puertas de los árboles {TreeSpec.DoorWidth:0.#} × {TreeSpec.DoorHeight:0.#} m.");
        return t;
    }

    // ── EL LEVIATÁN ─────────────────────────────────────────────────────────────────────────────────────────

    static Transform BuildLeviathan(Transform root)
    {
        float L = WhaleHospitalSubmarine.Length, B = WhaleHospitalSubmarine.Beam, H = WhaleHospitalSubmarine.Height;
        if (!FindBerth(L, B, out Vector3 berth, out float yaw, out float shoreSide)) { Debug.LogWarning("[Leviatán] No encuentro un atraque en mar abierto junto a una orilla: no se construye."); return null; }
        float deckTop = 0.3f;
        berth.y = deckTop - H * 0.5f;
        Transform t = new GameObject("Leviatan_hospital_ballenas").transform;
        t.SetParent(root, false);
        t.SetPositionAndRotation(berth, Quaternion.Euler(0f, yaw, 0f));
        WhaleHospitalSubmarine ws = t.gameObject.AddComponent<WhaleHospitalSubmarine>();
        ws.berthPos = berth; ws.berthRot = t.rotation;
        float hz = L * 0.5f, W = WhaleHospitalSubmarine.Well.x, WL = WhaleHospitalSubmarine.Well.y;
        float wellZ0 = -hz, wellZ1 = -hz + WL, side = (B - W) * 0.5f;   // el pozo, a popa (abierto por la compuerta)
        // CASCO alrededor del pozo: dos cascos laterales, el bloque de proa y el fondo.
        for (int s = -1; s <= 1; s += 2)
            KitchenProps.C(t, new Vector3(s * (W * 0.5f + side * 0.5f), 0f, 0f), new Vector3(side, H, L - 4f), SubHull, 0f, "Casco_lateral");
        KitchenProps.C(t, new Vector3(0f, 0f, (wellZ1 + hz - 4f) * 0.5f + 0f), new Vector3(W, H, hz - 4f - wellZ1), SubHull, 0f, "Casco_proa");
        KitchenProps.C(t, new Vector3(0f, -H * 0.5f + 0.3f, (wellZ0 + wellZ1) * 0.5f), new Vector3(W, 0.6f, WL), SubHull, 0f, "Fondo_pozo");
        // Proa redondeada en escalones.
        for (int k = 0; k < 3; k++) KitchenProps.C(t, new Vector3(0f, -k * 0.6f, hz - 3f + k * 1.2f), new Vector3(B - 4f - k * 4f, H - k * 2f, 1.4f), SubHull, 0f, "Proa");
        // Cubierta (a ras del hielo en el atraque) con franjas naranjas y la vela con el puente.
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(t, new Vector3(s * (W * 0.5f + side * 0.5f), H * 0.5f + 0.02f, 0f), new Vector3(side, 0.04f, L - 4f), new Color(0.55f, 0.6f, 0.62f), 0f, "Cubierta");
            KitchenProps.C(t, new Vector3(s * (B * 0.5f + 0.01f), H * 0.5f - 0.8f, 0f), new Vector3(0.05f, 0.6f, L - 6f), SubOrange, 0f, "Franja");
            // Ventanales de cristal al pozo (desde los pasillos de los cascos se ve a la ballena).
            KitchenProps.C(t, new Vector3(s * (W * 0.5f + 0.02f), -1f, (wellZ0 + wellZ1) * 0.5f), new Vector3(0.06f, 4f, WL - 4f), VirtualizationTank.GlassColor, 0f, "Ventanal_pozo");
        }
        Transform bridge = new GameObject("Puente").transform; bridge.SetParent(t, false); bridge.localPosition = new Vector3(0f, H * 0.5f, hz - 18f);
        KitchenProps.C(bridge, new Vector3(0f, 3f, 0f), new Vector3(7f, 6f, 10f), SubHull, 0f, "Vela");
        KitchenProps.C(bridge, new Vector3(0f, 4.6f, 5.02f), new Vector3(6f, 1.2f, 0.05f), VirtualizationTank.GlassColor, 0f, "Ventanal_puente");
        KitchenProps.C(bridge, new Vector3(0f, 6.3f, 0f), new Vector3(1f, 0.4f, 1f), SubBlue, 0f, "Faro_azul");
        for (int i = 1; i <= 4; i++)
        {
            Transform seat = KitchenProps.Root(bridge, $"Asiento_{i}", new Vector3(-2.4f + (i - 1) * 1.6f, 3.4f, 2.6f));
            KitchenProps.C(seat, new Vector3(0f, 0.25f, 0f), new Vector3(0.6f, 0.1f, 0.6f), SubBlue, 0f, "Asiento");
        }
        // EL POZO MÉDICO: agua (a nivel del mar, abierta a popa), suelo elevable, pasarela con barandilla, pórtico grúa, rociadores.
        Transform well = new GameObject("Pozo").transform; well.SetParent(t, false); well.localPosition = new Vector3(0f, 0f, (wellZ0 + wellZ1) * 0.5f);
        float waterLocal = WhaleHospitalSubmarine.WaterY - berth.y;   // el agua del pozo, a nivel del mar (en el atraque)
        KitchenProps.C(well, new Vector3(0f, (waterLocal + (-H * 0.5f + 0.6f)) * 0.5f, 0f), new Vector3(W - 0.2f, waterLocal + H * 0.5f - 0.6f, WL - 0.4f), new Color(0.2f, 0.5f, 0.75f, 0.45f), 0f, "Agua_pozo");
        Transform floorT = KitchenProps.Root(well, "Suelo_elevable", new Vector3(0f, -H * 0.5f + 1.2f, 0f));
        KitchenProps.C(floorT, new Vector3(0f, -0.2f, 0f), new Vector3(W - 0.6f, 0.4f, WL - 1f), new Color(0.85f, 0.85f, 0.8f), 0f, "Plataforma");
        for (int k = 0; k < 6; k++) KitchenProps.C(floorT, new Vector3(0f, 0.05f, -WL * 0.5f + 3f + k * (WL - 6f) / 5f), new Vector3(W - 1.2f, 0.12f, 0.6f), new Color(0.3f, 0.55f, 0.65f), 0f, "Colchon_espuma");
        for (int s = -1; s <= 1; s += 2)
            for (int k = 0; k < 4; k++) KitchenProps.C(well, new Vector3(s * (W * 0.5f - 0.25f), -1.5f, -WL * 0.5f + 4f + k * (WL - 8f) / 3f), new Vector3(0.4f, H - 3f, 0.4f), KitchenProps.DarkSteel, 0f, "Guia_suelo");
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(well, new Vector3(s * (W * 0.5f - 0.6f), H * 0.5f + 0.1f, 0f), new Vector3(1.2f, 0.12f, WL - 2f), KitchenProps.Steel, 0f, "Pasarela_pozo");
            KitchenProps.C(well, new Vector3(s * (W * 0.5f - 1.15f), H * 0.5f + 0.65f, 0f), new Vector3(0.05f, 1f, WL - 2f), KitchenProps.Steel, 0f, "Barandilla");
        }
        for (int k = 0; k < 2; k++)
        {
            float z = -WL * 0.25f + k * WL * 0.5f;
            KitchenProps.C(well, new Vector3(0f, H * 0.5f + 5f, z), new Vector3(W, 0.4f, 0.4f), SubOrange, 0f, "Portico_grua");
            for (int s = -1; s <= 1; s += 2) KitchenProps.C(well, new Vector3(s * W * 0.5f, H * 0.5f + 2.5f, z), new Vector3(0.4f, 5f, 0.4f), SubOrange, 0f, "Pata_portico");
        }
        Transform spray = new GameObject("Rociadores").transform; spray.SetParent(well, false);
        for (int s = -1; s <= 1; s += 2)
            for (int k = 0; k < 10; k++) KitchenProps.C(spray, new Vector3(s * (W * 0.5f - 1.3f), H * 0.5f - 0.5f, -WL * 0.5f + 2f + k * (WL - 4f) / 9f), new Vector3(0.08f, 0.3f, 0.08f), new Color(0.6f, 0.85f, 1f, 0.6f), 0f, "Chorro");
        spray.gameObject.SetActive(false);
        // Compuerta de popa (abatible por abajo).
        Transform gate = KitchenProps.Root(t, "Compuerta_popa", new Vector3(0f, -H * 0.5f + 0.6f, -hz));
        KitchenProps.C(gate, new Vector3(0f, (H - 0.6f) * 0.5f, 0f), new Vector3(W, H - 0.6f, 0.4f), SubOrange, 0f, "Compuerta");
        // A bordo: laboratorio, farmacia de cetáceos, ecógrafo portátil, jeringas de pértiga, pontones con eslingas (las grandes ballenas, junto al casco).
        Transform eq = new GameObject("Equipo_medico").transform; eq.SetParent(t, false); eq.localPosition = new Vector3(0f, H * 0.5f, wellZ1 + 4f);
        KitchenProps.C(eq, new Vector3(-3f, 1f, 0f), new Vector3(3f, 2f, 2f), KitchenProps.White, 0f, "Laboratorio_a_bordo");
        KitchenProps.C(eq, new Vector3(1f, 1f, 0f), new Vector3(2f, 2f, 1.2f), KitchenProps.White, 0f, "Farmacia_cetaceos");
        KitchenProps.C(eq, new Vector3(3.5f, 0.6f, 0f), new Vector3(0.8f, 1.2f, 0.8f), SubDark, 0f, "Ecografo_portatil");
        KitchenProps.C(eq, new Vector3(3.5f, 1.5f, 1.2f), new Vector3(0.1f, 3f, 0.1f), KitchenProps.Steel, 0f, "Jeringa_pertiga");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(t, new Vector3(s * (B * 0.5f - 2f), H * 0.5f + 0.8f, hz - 9f), new Vector3(2.5f, 1.6f, 8f), SubOrange, 0f, "Ponton_eslinga");
        // PASARELA al hielo, por el costado de la orilla.
        Transform gw = new GameObject("Pasarela").transform; gw.SetParent(t, false); gw.localPosition = new Vector3(shoreSide * (B * 0.5f), H * 0.5f, hz - 26f);
        float reach = 7f;
        KitchenProps.C(gw, new Vector3(shoreSide * reach * 0.5f, 0.05f, 0f), new Vector3(reach, 0.1f, 2f), KitchenProps.Steel, 0f, "Tablero");
        Transform end = new GameObject("Fin").transform; end.SetParent(gw, false); end.localPosition = new Vector3(shoreSide * (reach + 0.5f), 0f, 0f);
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>();
        wl.text = "Leviatán: hospital submarino de animales gigantes (72 × 18 m; pozo médico de 32 × 10 m con suelo elevable; orcas, belugas, ballenas de hasta 16 m)";
        wl.height = H * 0.5f + 8f;
        Debug.Log($"[Leviatán] Atracado en ({berth.x:0}, {berth.z:0}), rumbo {yaw:0}°, cubierta a {deckTop:0.0} m (a ras del hielo); pozo de {WL:0} × {W:0} m.");
        return t;
    }

    /// <summary>Un atraque en mar abierto (todo el casco y 3 m alrededor sin hielo) con la orilla a 2–8 m de un costado (la pasarela),
    /// a ≥ 45 m de los troncos de los árboles, lo más cerca posible de la laguna de las orcas.</summary>
    static bool FindBerth(float L, float B, out Vector3 pos, out float yaw, out float shoreSide)
    {
        pos = Vector3.zero; yaw = 0f; shoreSide = 1f;
        float best = float.MaxValue, lim = PolarLandscapeBuilder.Half - L * 0.5f - 6f;
        var trunks = new List<Vector3>(); foreach (Helix hx in _helixes) if (hx.root != null) trunks.Add(hx.root.position);
        for (float x = -lim; x <= lim; x += 8f)
            for (float z = -lim; z <= lim; z += 8f)
            {
                Vector3 c = new Vector3(x, 0f, z);
                float score = Vector3.Distance(c, PolarLandscapeBuilder.Lagoon);
                if (score >= best) continue;
                bool nearTrunk = false; foreach (Vector3 tr in trunks) if (Vector3.Distance(new Vector3(tr.x, 0f, tr.z), c) < 45f + L * 0.5f) nearTrunk = true;
                if (nearTrunk) continue;
                for (int a = 0; a < 180; a += 30)
                {
                    Quaternion q = Quaternion.Euler(0f, a, 0f);
                    bool clear = true;
                    for (float lz = -L * 0.5f - 3f; lz <= L * 0.5f + 3f && clear; lz += 4f)
                        for (float lx = -B * 0.5f - 3f; lx <= B * 0.5f + 3f && clear; lx += 3f)
                        {
                            Vector3 p = c + q * new Vector3(lx, 0f, lz);
                            if (PolarLandscapeBuilder.NaturalIce(p.x, p.z) || Mathf.Abs(p.x) > PolarLandscapeBuilder.Half - 2f || Mathf.Abs(p.z) > PolarLandscapeBuilder.Half - 2f) clear = false;
                        }
                    if (!clear) continue;
                    for (int s = -1; s <= 1 && clear; s += 2)
                    {
                        Vector3 g = c + q * new Vector3(s * (B * 0.5f + 7.5f), 0f, L * 0.5f - 26f);
                        if (!PolarLandscapeBuilder.NaturalIce(g.x, g.z)) continue;
                        if (!PhysicsClear(c, q, L, B)) { clear = false; break; }
                        best = score; pos = c; yaw = a; shoreSide = s;
                        break;
                    }
                }
            }
        return best < float.MaxValue;
    }

    // Nada sólido a ras del agua donde va el casco (placas de hielo sueltas, muelles…).
    static bool PhysicsClear(Vector3 c, Quaternion q, float L, float B)
    {
        for (float lz = -L * 0.5f; lz <= L * 0.5f; lz += 6f)
            for (float lx = -B * 0.5f; lx <= B * 0.5f; lx += 6f)
            {
                Vector3 p = c + q * new Vector3(lx, 0f, lz);
                if (Physics.Raycast(new Vector3(p.x, 8f, p.z), Vector3.down, out RaycastHit hit, 9f, ~0, QueryTriggerInteraction.Ignore) && hit.point.y > -0.4f) return false;
            }
        return true;
    }
}
