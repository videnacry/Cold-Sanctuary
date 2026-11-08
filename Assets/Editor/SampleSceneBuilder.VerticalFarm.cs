using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL HUERTO VERTICAL (Berón 2026-11-08; docs/produccion-maquinaria.md §2026-11-08; medidas en <see cref="VerticalFarm"/>): un piso de la torre de
/// PRODUCCIÓN con ESTANTES NFT de 4 bandejas bajo LED, para la verdura que los bancales del Árbol de los Campos no llegan a dar. Regla P19: cuántos
/// estantes (y cuántos pisos) salen de la DEMANDA (<see cref="VerticalFarm.RacksFor"/>: las personas de la lista × 0,8 kg, menos lo de los bancales,
/// con su reserva), no de un número. Con cada estante, todo lo que hace falta para trabajarlo (reglas de edificación de los pisos: muebles detallados,
/// herramientas, 2 m de paso, necesidades básicas): el VIVERO de germinación, el DOSIFICADOR de la solución nutritiva, el LAVADO y embolsado, la CÁMARA
/// de verdura, el BANCO DE HERRAMIENTAS, el COMPOSTADOR de raíces, el descanso con su fuente y los aseos.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color VfSteel = new Color(0.74f, 0.77f, 0.8f), VfTray = new Color(0.92f, 0.93f, 0.9f), VfLed = new Color(0.95f, 0.45f, 0.85f),
        VfLeaf = new Color(0.36f, 0.68f, 0.3f), VfLeafDark = new Color(0.2f, 0.5f, 0.22f), VfWater = new Color(0.3f, 0.55f, 0.75f), VfWood = new Color(0.6f, 0.47f, 0.33f);

    /// <summary>Los estantes que pide hoy la demanda (con lo que dan los bancales ya construidos del Árbol) y los pisos que ocupan.</summary>
    static (int racks, int floors) VerticalFarmPlan()
    {
        float fields = VerticalFarm.FieldsKgDay(), demand = VerticalFarm.DemandKgDay;
        int racks = VerticalFarm.RacksFor(demand, fields);
        return (racks, racks > 0 ? VerticalFarm.FloorsFor(racks) : 0);
    }

    /// <summary>EL HUERTO VERTICAL como SIGUIENTE PISO (o pisos) de la torre que se está levantando (Producción). Devuelve su área.</summary>
    static SanctuaryArea BuildVerticalFarm(Transform areasGroup)
    {
        float fields = VerticalFarm.FieldsKgDay(), demand = VerticalFarm.DemandKgDay;
        var (racks, floors) = VerticalFarmPlan();
        SanctuaryArea a = BuildAreaBox(areasGroup, VerticalFarm.AreaObject, VerticalFarm.AreaName, SanctuaryAreaType.VerticalFarm, 2, ProduccionCenter, ProduccionSize, new Color(0.55f, 0.75f, 0.5f));
        a.description = "Estantes de cultivo con LED: la verdura que no dan los bancales del Árbol (docs/produccion-maquinaria.md)";
        VerticalFarm vf = a.gameObject.AddComponent<VerticalFarm>();
        Vector2 s = TowerFloor(a.transform);
        int left = racks;
        for (int k = 0; k < floors; k++)
        {
            Transform floor = a.transform;
            if (k > 0) { floor = new GameObject($"Planta_huerto_vertical_{k + 1}").transform; floor.SetParent(a.transform, false); TowerFloor(floor); }
            int here = Mathf.Min(left, VerticalFarm.RacksPerFloor); left -= here;
            FurnishVerticalFarm(floor, s, here, k == 0);
        }
        vf.racksBuilt = vf.Racks;
        WorldLabel wl = a.gameObject.AddComponent<WorldLabel>(); wl.height = 5f;
        wl.text = $"Huerto vertical: {vf.racksBuilt} estantes LED ({vf.racksBuilt * VerticalFarm.RackM2:0} m² de bandeja), {vf.racksBuilt * VerticalFarm.RackKgDay:0.0} kg de verdura al día";
        float cap = vf.racksBuilt * VerticalFarm.RackKgDay;
        string msg = $"[Huerto vertical] {PopulationPlan.People} personas piden {demand:0.0} kg de verdura al día; los bancales dan {fields:0.0}; con la reserva de {(VerticalFarm.Reserve - 1f) * 100f:0} % faltan " +
                     $"{Mathf.Max(0f, demand * VerticalFarm.Reserve - fields):0.0} → {racks} estantes de {VerticalFarm.RackM2:0.0} m² ({VerticalFarm.YieldKgM2Year} kg/m²/año, Pennisi et al. 2025) en {floors} piso(s): " +
                     $"{cap:0.0} kg/día, total {fields + cap:0.0} kg/día; LED, clima y bombas {vf.LedKW:0.0} kW de media ({VerticalFarm.KWhPerKg} kWh/kg).";
        if (vf.racksBuilt < racks) Debug.LogWarning(msg + $" Solo cupieron {vf.racksBuilt}: sube RacksPerFloor o pon otro piso.");
        else Debug.Log(msg);
        return a;
    }

    /// <summary>Un piso del huerto vertical: <paramref name="racks"/> estantes y, en el primero, el vivero, el dosificador, el lavado, la cámara, las
    /// herramientas, el compost, el descanso y los aseos. Los estantes van primero: si algo no cabe, que no sean ellos.</summary>
    static void FurnishVerticalFarm(Transform floor, Vector2 s, int racks, bool first)
    {
        Transform g = new GameObject("Planta_cultivo_vertical").transform; g.SetParent(floor, false); g.localPosition = Vector3.up * 0.1f;
        var blocks = new List<LabBlock>();
        blocks.AddRange(Times(racks, "Estantería LED", VerticalFarm.TierW + 0.2f, VerticalFarm.TierD + 0.2f, t => { NftRack(t); AreaSeat.Add(t.gameObject, "cultivo vertical", AreaSeat.Category.Trabajo); }));
        if (first)
        {
            blocks.Add(new LabBlock("Vivero de germinación", 2.4f, 1.2f, t => { GerminationBench(t); AreaSeat.Add(t.gameObject, "siembra", AreaSeat.Category.Trabajo, 2); }));
            blocks.Add(new LabBlock("Dosificador de solución nutritiva", 2.4f, 1.2f, t => { NutrientDoser(t); AreaSeat.Add(t.gameObject, "solución nutritiva", AreaSeat.Category.Trabajo, 2); }));
            blocks.Add(new LabBlock("Lavado y embolsado", 2.4f, 1.0f, t => { WashStation(t); AreaSeat.Add(t.gameObject, "cosecha", AreaSeat.Category.Trabajo, 2); }));
            blocks.Add(new LabBlock("Cámara de verdura", 2.2f, 2.2f, t => VegColdRoom(t)));
            blocks.Add(new LabBlock("Banco de herramientas", 2.0f, 0.9f, t => { FarmToolBench(t); AreaSeat.Add(t.gameObject, "mantenimiento", AreaSeat.Category.Trabajo, 2); }));
            blocks.Add(new LabBlock("Compostador de raíces", 1.2f, 1.0f, t => Composter(t)));
            blocks.Add(new LabBlock("Descanso y fuente de agua", 1.8f, 0.8f, t => RestCorner(t)));
        }
        LogLeft(VerticalFarm.AreaName, PackFloor(g, s, blocks, CarpetKeepOut(s)));
        if (first) Aseos(floor, new Vector3(-s.x * 0.5f + 0.2f + 1.9f, 0.1f, s.y * 0.5f - 0.9f), 180f, 1);
    }

    // ── LAS PIEZAS (de cubos, con lo que las hace reconocibles) ──────────────────────────────────────────────────────────────────

    /// <summary>Un estante NFT de 4 bandejas de 3,0 × 1,2 m con su barra LED encima de cada una, las lechugas, la tubería de riego y el canal de
    /// retorno. La bandeja de arriba, a 1,9 m (<see cref="VerticalFarm.TopTierY"/>).</summary>
    static void NftRack(Transform t)
    {
        Transform r = KitchenProps.Root(t, VerticalFarm.RackPrefix, Vector3.zero);
        float W = VerticalFarm.TierW, D = VerticalFarm.TierD, top = VerticalFarm.TopTierY, step = (top - 0.4f) / (VerticalFarm.Tiers - 1);
        foreach (float x in new[] { -W * 0.5f, W * 0.5f })
            foreach (float z in new[] { -D * 0.5f, D * 0.5f })
                KitchenProps.C(r, new Vector3(x, (top + 0.45f) * 0.5f, z), new Vector3(0.05f, top + 0.45f, 0.05f), VfSteel, 0f, "Montante");
        for (int i = 0; i < VerticalFarm.Tiers; i++)
        {
            float y = 0.4f + i * step;
            KitchenProps.C(r, new Vector3(0f, y, 0f), new Vector3(W, 0.06f, D), VfTray, 0f, "Bandeja_NFT");
            for (int row = 0; row < 4; row++)   // las lechugas, en sus tacos a 25 cm
                KitchenProps.C(r, new Vector3(0f, y + 0.09f, -D * 0.5f + 0.15f + row * 0.3f), new Vector3(W - 0.15f, 0.12f, 0.16f), row % 2 == 0 ? VfLeaf : VfLeafDark, 0f, "Lechugas");
            KitchenProps.C(r, new Vector3(0f, y + 0.4f, 0f), new Vector3(W - 0.1f, 0.03f, 0.12f), VfLed, 0f, "Barra_LED");
        }
        KitchenProps.C(r, new Vector3(-W * 0.5f - 0.07f, (top + 0.4f) * 0.5f, 0f), new Vector3(0.04f, top, 0.04f), VfWater, 0f, "Tuberia_riego");
        KitchenProps.C(r, new Vector3(W * 0.5f + 0.07f, 0.2f, 0f), new Vector3(0.08f, 0.08f, D), VfWater, 0f, "Canal_retorno");
        KitchenProps.C(r, new Vector3(0f, top + 0.47f, 0f), new Vector3(0.3f, 0.06f, 0.2f), LabDark, 0f, "Driver_LED");
    }

    static void GerminationBench(Transform t)
    {
        Bench(t, 2.4f, 1.2f, LabBench);
        for (int i = 0; i < 4; i++) KitchenProps.C(t, new Vector3(-0.9f + i * 0.6f, 1.0f, 0f), new Vector3(0.5f, 0.06f, 0.3f), VfLeaf, 0f, "Bandeja_semillero");
        for (int i = 0; i < 2; i++) KitchenProps.C(t, new Vector3(-0.6f + i * 1.2f, 1.12f, 0f), new Vector3(1.1f, 0.18f, 0.4f), LabGlass, 0f, "Cupula_humedad");
        KitchenProps.C(t, new Vector3(0f, 1.6f, 0f), new Vector3(2.2f, 0.04f, 0.15f), VfLed, 0f, "Barra_LED_vivero");
        KitchenProps.C(t, new Vector3(1.0f, 1.0f, 0.4f), new Vector3(0.2f, 0.12f, 0.12f), new Color(0.85f, 0.75f, 0.4f), 0f, "Sobres_de_semillas");
        KitchenProps.C(t, new Vector3(-1.0f, 1.0f, 0.4f), new Vector3(0.3f, 0.1f, 0.2f), new Color(0.45f, 0.35f, 0.25f), 0f, "Tacos_de_lana_de_roca");
    }

    static void NutrientDoser(Transform t)
    {
        Vessel(t, 0.35f, 1.1f, new Color(0.85f, 0.85f, 0.75f), "Deposito_A");
        Transform b = KitchenProps.Root(t, "Deposito_B", new Vector3(0.8f, 0f, 0f)); Vessel(b, 0.35f, 1.1f, new Color(0.75f, 0.82f, 0.85f), "Deposito_B");
        Transform ret = KitchenProps.Root(t, "Deposito_retorno", new Vector3(-0.8f, 0f, 0f)); Vessel(ret, 0.4f, 0.8f, VfWater, "Deposito_retorno");
        KitchenProps.C(t, new Vector3(0f, 1.5f, -0.5f), new Vector3(1.2f, 0.6f, 0.12f), LabWhite, 0f, "Controlador_EC_pH");
        KitchenProps.C(t, new Vector3(0f, 1.55f, -0.57f), new Vector3(0.5f, 0.25f, 0.02f), LedGreen, 0f, "Pantalla_EC_pH");
        KitchenProps.C(t, new Vector3(0.4f, 0.2f, -0.45f), new Vector3(0.35f, 0.3f, 0.25f), LabBlue, 0f, "Bomba_dosificadora");
        KitchenProps.C(t, new Vector3(-0.4f, 0.2f, -0.45f), new Vector3(0.35f, 0.3f, 0.25f), LabSteel, 0f, "Filtro_y_UV");
    }

    static void WashStation(Transform t)
    {
        KitchenProps.C(t, new Vector3(-0.6f, 0.45f, 0f), new Vector3(1.1f, 0.9f, 0.9f), VfSteel, 0f, "Pila_de_lavado");
        KitchenProps.C(t, new Vector3(-0.6f, 0.88f, 0f), new Vector3(0.95f, 0.05f, 0.75f), VfWater, 0f, "Agua_de_lavado");
        KitchenProps.C(t, new Vector3(-0.6f, 1.25f, -0.4f), new Vector3(0.05f, 0.5f, 0.05f), LabSteel, 0f, "Grifo");
        Bench(KitchenProps.Root(t, "Mesa_embolsado", new Vector3(0.6f, 0f, 0f)), 1.1f, 0.9f, LabBench);
        KitchenProps.C(t, new Vector3(0.4f, 0.98f, 0f), new Vector3(0.3f, 0.1f, 0.3f), LabWhite, 0f, "Bascula");
        KitchenProps.C(t, new Vector3(0.85f, 1.0f, 0.1f), new Vector3(0.3f, 0.15f, 0.4f), new Color(0.9f, 0.9f, 0.95f, 0.6f), 0f, "Bolsas_compostables");
        KitchenProps.C(t, new Vector3(0.6f, 1.05f, -0.3f), new Vector3(0.35f, 0.25f, 0.35f), LabSteel, 0f, "Centrifugadora_de_ensalada");
    }

    static void VegColdRoom(Transform t)
    {
        KitchenProps.C(t, new Vector3(0f, 1.1f, 0f), new Vector3(2.2f, 2.2f, 2.2f), new Color(0.88f, 0.92f, 0.95f), 0f, "Camara_verdura");
        KitchenProps.C(t, new Vector3(0f, 1.0f, -1.12f), new Vector3(0.9f, 1.9f, 0.04f), LabSteel, 0f, "Puerta_camara");
        KitchenProps.C(t, new Vector3(0.7f, 1.6f, -1.13f), new Vector3(0.2f, 0.12f, 0.02f), LedGreen, 0f, "Termometro_4C");
        for (int i = 0; i < 3; i++) KitchenProps.C(t, new Vector3(-0.7f + i * 0.7f, 0.15f, -1.4f), new Vector3(0.55f, 0.3f, 0.35f), VfLeaf, 0f, "Caja_de_cosecha");
    }

    static void FarmToolBench(Transform t)
    {
        Bench(t, 2.0f, 0.9f, LabBench);
        KitchenProps.C(t, new Vector3(-0.7f, 0.97f, 0f), new Vector3(0.25f, 0.03f, 0.08f), LabSteel, 15f, "Cuchillo_de_cosecha");
        KitchenProps.C(t, new Vector3(-0.35f, 0.97f, 0f), new Vector3(0.2f, 0.04f, 0.1f), LabRed, -10f, "Tijeras_de_poda");
        KitchenProps.C(t, new Vector3(0f, 1.0f, 0f), new Vector3(0.08f, 0.18f, 0.05f), LabYellow, 0f, "Medidor_EC_pH");
        KitchenProps.C(t, new Vector3(0.35f, 1.0f, 0f), new Vector3(0.3f, 0.08f, 0.08f), VfWood, 0f, "Cepillo_de_canales");
        KitchenProps.C(t, new Vector3(0.75f, 1.05f, 0f), new Vector3(0.25f, 0.2f, 0.25f), LabBlue, 0f, "Cubo");
        KitchenProps.C(t, new Vector3(0f, 1.6f, 0.42f), new Vector3(1.8f, 0.9f, 0.04f), VfWood, 0f, "Panel_de_herramientas");
        Transform cart = KitchenProps.Root(t, "Carro_de_bandejas", new Vector3(0f, 0f, -0.75f));
        KitchenProps.C(cart, new Vector3(0f, 0.5f, 0f), new Vector3(0.9f, 0.04f, 0.5f), VfSteel, 0f, "Carro");
        KitchenProps.C(cart, new Vector3(0f, 0.08f, 0f), new Vector3(0.9f, 0.04f, 0.5f), VfSteel, 0f, "Carro_abajo");
    }

    static void Composter(Transform t)
    {
        KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1.0f, 0.9f, 0.8f), new Color(0.3f, 0.45f, 0.3f), 0f, "Compostador");
        KitchenProps.C(t, new Vector3(0f, 0.93f, 0f), new Vector3(1.05f, 0.06f, 0.85f), new Color(0.25f, 0.35f, 0.25f), 0f, "Tapa");
        Label(t, "Raíces y hojas viejas: abono para los bancales del Árbol", 1.4f);
    }

    static void RestCorner(Transform t)
    {
        KitchenProps.C(t, new Vector3(-0.3f, 0.45f, 0f), new Vector3(1.2f, 0.06f, 0.45f), VfWood, 0f, "Banco");
        KitchenProps.C(t, new Vector3(-0.3f, 0.22f, 0f), new Vector3(1.1f, 0.44f, 0.4f), LabSteel, 0f, "Patas");
        KitchenProps.C(t, new Vector3(0.7f, 0.5f, 0f), new Vector3(0.35f, 1.0f, 0.35f), LabWhite, 0f, "Fuente_de_agua");
        KitchenProps.C(t, new Vector3(0.7f, 1.02f, 0f), new Vector3(0.2f, 0.04f, 0.2f), VfWater, 0f, "Caño");   // (sin AreaSeat: no es una actividad, es la pausa)
    }
}
