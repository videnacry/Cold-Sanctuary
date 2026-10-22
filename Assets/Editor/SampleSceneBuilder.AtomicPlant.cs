using UnityEngine;

/// <summary>
/// PLANTA ATÓMICA (docs/atomic-plant.md): un reactor de investigación de piscina tipo TRIGA, elevado como todo el
/// santuario. Todo lo que hay es INTERACTUABLE (sin decorado): la taquilla de dosímetros y el pórtico de contaminación
/// en la entrada; en la nave del reactor, la piscina (puente para ver el núcleo y su brillo de Cherenkov), el conejo
/// neumático, la purificación del agua, la bomba y el intercambiador; en la sala de control, la consola; la celda
/// caliente; la torre de refrigeración; y el rincón de enseñanza (cámara de niebla y contador Geiger). La piscina baja
/// como un pilar de hormigón hasta el suelo: el núcleo está a ~7 m bajo el agua, como en los TRIGA reales (7,3–8,2 m).
/// En el sur, en el lóbulo libre del casquete Sur (el huerto de rotación subió sobre el Huerto).
/// </summary>
public static partial class SampleSceneBuilder
{
    public static readonly Vector2 AtomicPlantSize = new Vector2(24f, 18f);
    static readonly Vector3 AtomicPlantCenter = new Vector3(15f, 0f, -101f);

    static readonly Color ApConcrete = new Color(0.72f, 0.72f, 0.7f), ApSteel = new Color(0.62f, 0.66f, 0.7f), ApWater = new Color(0.25f, 0.55f, 0.85f, 0.45f),
        ApLead = new Color(0.38f, 0.4f, 0.44f), ApGlass = new Color(0.75f, 0.85f, 0.75f, 0.35f), ApYellow = new Color(0.95f, 0.8f, 0.2f);

    /// <summary>Crea, eleva y equipa la planta. Devuelve su área (ya en su plataforma).</summary>
    static SanctuaryArea BuildAtomicPlant(Transform areasGroup)
    {
        SanctuaryArea a = BuildAreaBox(areasGroup, "AtomicPlant_Area", "Planta Atómica", SanctuaryAreaType.AtomicPlant, 3,
            AtomicPlantCenter, AtomicPlantSize, new Color(0.8f, 0.82f, 0.78f));
        Elevate(a.transform, AtomicPlantSize, ElevatedHeight);
        FurnishAtomicPlant(a.transform, ElevatedHeight);
        return a;
    }

    static void FurnishAtomicPlant(Transform area, float h)
    {
        Transform r = new GameObject("Planta").transform; r.SetParent(area, false); r.localPosition = new Vector3(0f, 0.1f, 0f);

        // ── EL REACTOR: piscina de hormigón que baja hasta el suelo (pilar), con el agua, el núcleo y su luz ──
        Transform pool = KitchenProps.Root(r, "Piscina_reactor", new Vector3(-6.5f, 0f, -2.5f));
        float depth = h + 0.1f, inner = 2.4f, wall = 1f, top = 1f;   // parapeto de 1 m sobre el suelo de la planta
        float cy = (top - depth) * 0.5f, ch = top + depth;
        Solid(pool, new Vector3(0f, cy, -(inner + wall) * 0.5f), new Vector3(inner + 2f * wall, ch, wall), ApConcrete, "Hormigon");
        Solid(pool, new Vector3(0f, cy, (inner + wall) * 0.5f), new Vector3(inner + 2f * wall, ch, wall), ApConcrete, "Hormigon");
        Solid(pool, new Vector3(-(inner + wall) * 0.5f, cy, 0f), new Vector3(wall, ch, inner), ApConcrete, "Hormigon");
        Solid(pool, new Vector3((inner + wall) * 0.5f, cy, 0f), new Vector3(wall, ch, inner), ApConcrete, "Hormigon");
        Solid(pool, new Vector3(0f, -depth, 0f), new Vector3(inner, 0.3f, inner), ApConcrete, "Fondo");
        KitchenProps.C(pool, new Vector3(0f, (top - 0.2f - depth) * 0.5f, 0f), new Vector3(inner - 0.02f, top - 0.2f + depth, inner - 0.02f), ApWater, 0f, "Agua");
        Transform lid = Solid(pool, new Vector3(0f, top + 1f, 0f), new Vector3(inner, 2f, inner), ApWater, "Barrera_agua");
        lid.GetComponent<Renderer>().enabled = false;   // nadie cae a la piscina; se mira desde el puente
        // El NÚCLEO: ~90 elementos de UZrH en una rejilla, rodeado del reflector de grafito; y las 4 barras que bajan a él.
        float coreY = -depth + 0.9f;
        KitchenProps.C(pool, new Vector3(0f, coreY, 0f), new Vector3(1.1f, 0.8f, 1.1f), new Color(0.3f, 0.3f, 0.32f), 0f, "Reflector_grafito");
        KitchenProps.C(pool, new Vector3(0f, coreY, 0f), new Vector3(0.6f, 0.82f, 0.6f), new Color(0.45f, 0.75f, 1f), 0f, "Nucleo_UZrH");
        for (int i = 0; i < 4; i++)
            KitchenProps.C(pool, new Vector3(-0.21f + i * 0.14f, (coreY + top) * 0.5f + 0.4f, 0.32f), new Vector3(0.05f, top - coreY, 0.05f), ApSteel, 0f, "Barra_control");
        var lightGo = new GameObject("Cherenkov"); lightGo.transform.SetParent(pool, false); lightGo.transform.localPosition = new Vector3(0f, coreY + 1.2f, 0f);
        Light cher = lightGo.AddComponent<Light>(); cher.type = LightType.Point; cher.color = new Color(0.35f, 0.6f, 1f); cher.range = 9f; cher.intensity = 0f;
        ResearchReactor reactor = pool.gameObject.AddComponent<ResearchReactor>(); reactor.cherenkov = cher;
        // N-16 del agua que sale del núcleo: la tasa en la superficie escala con la potencia.
        var surf = new GameObject("Fuente_superficie"); surf.transform.SetParent(pool, false); surf.transform.localPosition = new Vector3(0f, top, 0f);
        RadiationSource rs = surf.AddComponent<RadiationSource>(); rs.microSvPerHourAt1m = 15f; rs.scalesWithReactorPower = true;
        // PUENTE sobre el borde sur de la piscina (por donde se asoma uno).
        Transform bridge = KitchenProps.Root(r, "Puente_piscina", pool.localPosition + new Vector3(0f, 0f, inner * 0.5f + wall + 0.5f));
        KitchenProps.C(bridge, new Vector3(0f, 0.15f, 0f), new Vector3(2.4f, 0.3f, 0.9f), ApSteel, 0f, "Pasarela");
        KitchenProps.C(bridge, new Vector3(0f, 0.3f, 0.46f), new Vector3(2.4f, 0.06f, 0.06f), ApYellow, 0f, "Borde_amarillo");
        KitchenProps.Fit(bridge, solid: false);
        bridge.gameObject.AddComponent<PlantEquipment>().kind = PlantEquipment.Kind.PoolBridge;
        Label(bridge, "Puente de la piscina (mirar al núcleo)", 1.6f);

        // ── NAVE DEL REACTOR: conejo neumático, purificación, bomba e intercambiador ──
        Transform rabbit = Equip(r, "Conejo_neumatico", new Vector3(-2.6f, 0f, -6.8f), 0f, (t) =>
        {
            KitchenProps.C(t, new Vector3(0f, 0.55f, 0f), new Vector3(0.9f, 1.1f, 0.6f), ApSteel, 0f, "Estacion_envio");
            KitchenProps.C(t, new Vector3(0f, 1.15f, 0f), new Vector3(0.3f, 0.1f, 0.3f), ApYellow, 0f, "Boca_capsula");
            KitchenProps.C(t, new Vector3(-1.9f, 1.2f, 0f), new Vector3(3f, 0.08f, 0.08f), ApSteel, 0f, "Tubo_al_nucleo");
        }, "Conejo neumático (muestras al núcleo)");
        PneumaticTransfer pt = rabbit.gameObject.AddComponent<PneumaticTransfer>();

        Equip(r, "Purificacion", new Vector3(-10.4f, 0f, -6.8f), 0f, (t) =>
        {
            for (int i = 0; i < 2; i++) KitchenProps.C(t, new Vector3(-0.45f + i * 0.9f, 0.7f, 0f), new Vector3(0.5f, 1.4f, 0.5f), new Color(0.85f, 0.85f, 0.88f), 0f, "Columna_resinas");
            KitchenProps.C(t, new Vector3(0f, 1.5f, 0.3f), new Vector3(0.4f, 0.25f, 0.05f), new Color(0.2f, 0.25f, 0.2f), 0f, "Conductimetro");
        }, "Purificación del agua (resinas)").gameObject.AddComponent<PlantEquipment>().kind = PlantEquipment.Kind.Purification;

        Equip(r, "Bomba_e_intercambiador", new Vector3(-10.4f, 0f, 2f), 90f, (t) =>
        {
            KitchenProps.C(t, new Vector3(-0.7f, 0.4f, 0f), new Vector3(0.7f, 0.8f, 0.7f), new Color(0.3f, 0.45f, 0.7f), 0f, "Bomba");
            KitchenProps.C(t, new Vector3(0.5f, 0.6f, 0f), new Vector3(1.4f, 0.7f, 0.7f), ApSteel, 0f, "Intercambiador_placas");
        }, "Bomba del primario e intercambiador").gameObject.AddComponent<PlantEquipment>().kind = PlantEquipment.Kind.PrimaryPump;

        // ── SALA DE CONTROL: la consola mira a la piscina ──
        Transform console = Equip(r, "Consola_reactor", new Vector3(4.5f, 0f, -6f), 0f, (t) =>
        {
            KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(3.2f, 0.9f, 0.8f), new Color(0.82f, 0.82f, 0.78f), 0f, "Mesa_consola");
            KitchenProps.C(t, new Vector3(0f, 1.15f, -0.25f), new Vector3(3f, 0.6f, 0.1f), new Color(0.15f, 0.18f, 0.2f), 0f, "Pantallas");
            KitchenProps.C(t, new Vector3(1.2f, 0.93f, 0.2f), new Vector3(0.18f, 0.06f, 0.18f), new Color(0.9f, 0.15f, 0.1f), 0f, "Boton_SCRAM");
            for (int i = 0; i < 4; i++) KitchenProps.C(t, new Vector3(-1.1f + i * 0.35f, 0.93f, 0.2f), new Vector3(0.12f, 0.05f, 0.12f), ApSteel, 0f, "Mando_barra");
        }, "Consola del reactor");
        console.gameObject.AddComponent<ReactorConsole>().reactor = reactor;

        // ── CELDA CALIENTE (plomo y vidrio plomado; telemanipuladores) ──
        Transform hot = Equip(r, "Celda_caliente", new Vector3(10f, 0f, -2f), -90f, (t) =>
        {
            KitchenProps.C(t, new Vector3(0f, 0.9f, 0f), new Vector3(2f, 1.8f, 1.6f), ApLead, 0f, "Plomo");
            KitchenProps.C(t, new Vector3(0f, 1.25f, 0.81f), new Vector3(0.9f, 0.5f, 0.04f), ApGlass, 0f, "Vidrio_plomado");
            for (int s = -1; s <= 1; s += 2) KitchenProps.C(t, new Vector3(s * 0.6f, 1.35f, 0.95f), new Vector3(0.08f, 0.08f, 0.35f), ApSteel, 0f, "Telemanipulador");
        }, "Celda caliente (isótopos)");
        pt.hotCell = hot.gameObject.AddComponent<HotCell>();
        var hs = new GameObject("Fuente_celda"); hs.transform.SetParent(hot, false); hs.transform.localPosition = Vector3.up;
        RadiationSource hrs = hs.AddComponent<RadiationSource>(); hrs.microSvPerHourAt1m = 50000f; hrs.shielding = 0.0001f;

        // ── ENTRADA (+Z): taquilla de dosímetros y pórtico de contaminación, a los lados del paso de la escalera ──
        Equip(r, "Taquilla_dosimetros", new Vector3(-3.4f, 0f, 7.6f), 180f, (t) =>
        {
            KitchenProps.C(t, new Vector3(0f, 0.9f, 0f), new Vector3(1.4f, 1.8f, 0.35f), new Color(0.7f, 0.75f, 0.8f), 0f, "Taquilla");
            for (int i = 0; i < 6; i++) KitchenProps.C(t, new Vector3(-0.5f + (i % 3) * 0.5f, 1.2f - (i / 3) * 0.4f, -0.19f), new Vector3(0.12f, 0.18f, 0.03f), ApYellow, 0f, "Dosimetro");
        }, "Dosímetros (cógelo al entrar)").gameObject.AddComponent<DosimeterRack>();

        Equip(r, "Portico_contaminacion", new Vector3(3.6f, 0f, 7.4f), 180f, (t) =>
        {
            KitchenProps.C(t, new Vector3(0f, 1f, 0f), new Vector3(1f, 2f, 0.6f), new Color(0.85f, 0.85f, 0.8f), 0f, "Monitor");
            for (int s = -1; s <= 1; s += 2) KitchenProps.C(t, new Vector3(s * 0.25f, 1.1f, -0.31f), new Vector3(0.3f, 0.4f, 0.02f), new Color(0.3f, 0.35f, 0.4f), 0f, "Detector_mano");
            KitchenProps.C(t, new Vector3(0f, 0.05f, -0.6f), new Vector3(0.8f, 0.1f, 0.6f), new Color(0.3f, 0.35f, 0.4f), 0f, "Detector_pies");
            KitchenProps.C(t, new Vector3(0.9f, 0.85f, 0f), new Vector3(0.5f, 0.15f, 0.4f), Color.white, 0f, "Lavabo");
        }, "Pórtico de contaminación (al salir)").gameObject.AddComponent<ContaminationMonitor>();

        // ── ENSEÑANZA: cámara de niebla y contador Geiger (lejos de la esquina de la máquina de virtualización) ──
        Equip(r, "Camara_niebla", new Vector3(3.6f, 0f, 2.6f), 0f, (t) =>
        {
            KitchenProps.C(t, new Vector3(0f, 0.4f, 0f), new Vector3(1f, 0.8f, 0.7f), KitchenProps.Wood, 0f, "Mesa");
            KitchenProps.C(t, new Vector3(0f, 0.95f, 0f), new Vector3(0.5f, 0.3f, 0.5f), ApGlass, 0f, "Cubeta_vapor");
        }, "Cámara de niebla").gameObject.AddComponent<PlantEquipment>().kind = PlantEquipment.Kind.CloudChamber;

        Equip(r, "Contador_Geiger", new Vector3(7.2f, 0f, 2.6f), 0f, (t) =>
        {
            KitchenProps.C(t, new Vector3(0f, 0.4f, 0f), new Vector3(0.8f, 0.8f, 0.6f), KitchenProps.Wood, 0f, "Mesa");
            KitchenProps.C(t, new Vector3(0f, 0.9f, 0f), new Vector3(0.3f, 0.18f, 0.2f), ApYellow, 0f, "Geiger");
            KitchenProps.C(t, new Vector3(0.25f, 0.88f, 0f), new Vector3(0.25f, 0.05f, 0.05f), new Color(0.2f, 0.2f, 0.2f), 0f, "Sonda");
        }, "Contador Geiger").gameObject.AddComponent<PlantEquipment>().kind = PlantEquipment.Kind.GeigerCounter;

        // ── TORRE DE REFRIGERACIÓN del secundario (ventilador forzado), en la esquina noroeste ──
        Equip(r, "Torre_refrigeracion", new Vector3(-10f, 0f, 7f), 0f, (t) =>
        {
            KitchenProps.C(t, new Vector3(0f, 1.2f, 0f), new Vector3(1.8f, 2.4f, 1.8f), new Color(0.6f, 0.68f, 0.72f), 0f, "Celda_torre");
            KitchenProps.C(t, new Vector3(0f, 2.5f, 0f), new Vector3(1.2f, 0.2f, 1.2f), ApSteel, 0f, "Ventilador");
        }, "Torre de refrigeración (secundario)").gameObject.AddComponent<PlantEquipment>().kind = PlantEquipment.Kind.CoolingTower;

        Aseos(r, new Vector3(0f, 0f, -8.2f), 0f, 1);   // al fondo, entre el conejo y la consola
    }

    /// <summary>Un equipo de la planta: raíz + piezas + collider sólido que lo envuelve (sin apilarse: cada uno en su
    /// sitio, con ≥ 1,5 m de paso alrededor) + su cartel.</summary>
    static Transform Equip(Transform parent, string name, Vector3 local, float yaw, System.Action<Transform> build, string label)
    {
        Transform t = KitchenProps.Root(parent, name, local, yaw);
        build(t);
        KitchenProps.Fit(t, solid: true);
        Label(t, label, 2.6f);
        return t;
    }
}
