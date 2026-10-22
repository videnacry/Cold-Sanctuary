using UnityEngine;

/// <summary>
/// EL HOSPITAL (antes Enfermería; docs/hospital.md). No hay otro hospital a miles de km, así que tiene lo mínimo de un
/// hospital de primer nivel de la OMS para tratar todos los casos sin derivar: urgencias con reanimación, quirófano
/// (cirujano + anestesista, esterilización, lavado quirúrgico), UCI, paritorio, aislamiento, imagen, laboratorio y banco
/// de sangre, farmacia, oxígeno propio y telemedicina. Dos plantas sobre la misma base:
/// - PLANTA BAJA (personas): servicios a pie de suelo con pasillos de ≥ 3 m, y las 30 CAMAS DE PLANTA en una galería en U
///   a 3 m (plataforma hija, sin barandillas; por debajo se pasa de pie). Aquí sigue la iniciación (silla de exámenes,
///   analizador clínico, mesa del médico y el tanque de virtualización).
/// - PLANTA ALTA, a 10 m (animales): hospital veterinario con 30 PLAZAS (6 cuadras, 16 boxes, 8 jaulas), consulta y
///   quirófano veterinarios. Mismo número de plazas para animales que camas para personas.
/// 8 PUESTOS MÉDICOS (HospitalStation.doctorPost): urgencias, 2 consultas, UCI, cirujano, anestesista, veterinaria y
/// cirugía veterinaria; la jornada coloca a cada médico en uno libre.
/// </summary>
public static partial class SampleSceneBuilder
{
    public static readonly Vector2 HospitalSize = new Vector2(32f, 26f);
    public const float HospitalBedDeck = 3f;
    // Cama de 1,07 m de ancho (barandas; gotero y monitor van detrás del cabecero) + 0,9 m de pasillo entre camas
    // (FurnitureSpacing: acceso por los dos lados) → 2 m de eje a eje.
    const float BedPitch = 2f;   // galería de camas: a 3 m se pasa de pie por debajo (a 2,2 m se chocaba)

    static Transform Station(Transform t, HospitalService service, string label, float height, bool doctorPost = false, Vector3? stand = null)
    {
        HospitalStation hs = t.gameObject.AddComponent<HospitalStation>();
        hs.service = service; hs.doctorPost = doctorPost;
        if (stand.HasValue) hs.standLocal = stand.Value;
        Label(t, doctorPost ? label + " · puesto médico" : label, height);
        return t;
    }

    static void BuildHospital(Transform root)
    {
        GameObject area = GameObject.Find("Infirmary_Area");
        if (area == null) return;
        float hx = HospitalSize.x * 0.5f, hz = HospitalSize.y * 0.5f;
        Transform r = new GameObject("Equipamiento").transform;
        r.SetParent(area.transform, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Suelo_Hospital"; floor.transform.SetParent(r, false);
        floor.transform.localPosition = new Vector3(0f, 0.005f, 0f); floor.transform.localScale = new Vector3(HospitalSize.x - 0.4f, 0.04f, HospitalSize.y - 0.4f);
        floor.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Hospital_Suelo_MAT", new Color(0.86f, 0.94f, 0.92f));
        // Pasillo central (entrada → fondo), libre de muebles: 3 m de ancho.
        KitchenProps.C(r, new Vector3(0f, 0.03f, 1.9f), new Vector3(3f, 0.01f, HospitalSize.y - 7.6f), new Color(0.72f, 0.86f, 0.84f), 0f, "Pasillo_central");

        // ── GALERÍA DE CAMAS a 3 m (en U, sin barandillas): 10 al oeste, 12 al fondo, 8 al este = 30 camas de planta ──
        // Sin postes en el borde que da al exterior de la plataforma (por ahí llegan los puentes).
        float g = HospitalBedDeck, run = StairRun(g);
        Color[] bl = { new Color(0.62f, 0.8f, 0.92f), new Color(0.75f, 0.9f, 0.8f), new Color(0.95f, 0.85f, 0.7f), new Color(0.9f, 0.78f, 0.85f) };
        Transform west = Deck(r, "Galeria_oeste", new Vector3(-hx + 2.2f, 0f, -3f), new Vector2(4.4f, 20f), g, new Vector2Int(-1, -1));
        Transform back = Deck(r, "Galeria_fondo", new Vector3(0f, 0f, -hz + 2.2f), new Vector2(HospitalSize.x - 8.8f, 4.4f), g, new Vector2Int(0, -1));
        Transform east = Deck(r, "Galeria_este", new Vector3(hx - 2.2f, 0f, -4.5f), new Vector2(4.4f, 17f), g, new Vector2Int(1, -1));
        int bed = 4;   // las camas 1-3 son las de observación de urgencias (abajo)
        // Cabecero hacia el borde exterior; hacia dentro queda un pasillo de ~1,5 m por el que llegan las escaleras.
        for (int i = 0; i < 10; i++, bed++) Label(InfirmaryProps.HospitalBed(r, $"Cama {bed}", new Vector3(-hx + 2f, g, -12.4f + i * BedPitch), 90f, bl[i % 4]), $"Cama {bed}", 1.4f);
        for (int i = 0; i < 12; i++, bed++) Label(InfirmaryProps.HospitalBed(r, $"Cama {bed}", new Vector3(-11f + i * BedPitch, g, -hz + 2.2f), 0f, bl[i % 4]), $"Cama {bed}", 1.4f);
        for (int i = 0; i < 8; i++, bed++) Label(InfirmaryProps.HospitalBed(r, $"Cama {bed}", new Vector3(hx - 2f, g, -12.4f + i * BedPitch), -90f, bl[i % 4]), $"Cama {bed}", 1.4f);
        InclinedStair(r, "Escalera_galeria_oeste", new Vector3(-hx + 3.6f, 0f, 7f + run), 180f, g);
        InclinedStair(r, "Escalera_galeria_este", new Vector3(hx - 3.6f, 0f, 4f + run), 180f, g);
        Label(west, "Sala de camas (oeste)", 1.8f); Label(east, "Sala de camas (este)", 1.8f); Label(back, "Sala de camas (fondo)", 1.8f);

        // ── PLANTA BAJA, OESTE: entrada, urgencias, consultas (iniciación) y UCI ──
        Transform triage = InfirmaryProps.DoctorDesk(r, "Triaje", new Vector3(-5.6f, 0f, 7.4f), 180f);
        Label(triage, "Recepción y triaje", 1.5f);
        Aseos(r, new Vector3(-5.4f, 0f, hz - 0.9f), 180f);   // junto a la entrada; a −9,6 baja la escalera de la planta alta
        Station(InfirmaryProps.IsolationRoom(r, "Aislamiento", new Vector3(-10.2f, 0f, 7.6f), 90f, bl[1]), HospitalService.Aislamiento, "Aislamiento (infecciosos)", 2.6f, false, new Vector3(0f, 0f, 2.2f));
        Color[] blankets = { new Color(0.62f, 0.8f, 0.92f), new Color(0.75f, 0.9f, 0.8f), new Color(0.95f, 0.85f, 0.7f) };
        for (int i = 0; i < 3; i++) Label(InfirmaryProps.HospitalBed(r, $"Cama {i + 1}", new Vector3(-10f + i * 2.4f, 0f, 4.2f), 0f, blankets[i]), $"Cama {i + 1} (observación)", 1.4f);
        Station(InfirmaryProps.CrashCart(r, "Carro_de_parada", new Vector3(-3.1f, 0f, 4.6f), -90f), HospitalService.Urgencias, "Urgencias: carro de parada y desfibrilador", 1.6f, true, new Vector3(0f, 0f, -0.9f));
        // Iniciación (los nombres los usan NewcomerInitiator y la misión enf_iniciacion): silla de exámenes, mesa del médico, camilla.
        Transform exam = KitchenProps.Root(r, "Silla_examen", new Vector3(-9.8f, 0f, 0.6f), 90f);
        Color seatC = new Color(0.35f, 0.55f, 0.65f);
        KitchenProps.C(exam, new Vector3(0f, 0.45f, 0f), new Vector3(0.6f, 0.1f, 0.6f), seatC, 0f, "Asiento_examen");
        KitchenProps.C(exam, new Vector3(0f, 0.85f, -0.27f), new Vector3(0.6f, 0.75f, 0.08f), seatC, 0f, "Respaldo");
        KitchenProps.C(exam, new Vector3(0.36f, 0.68f, 0.05f), new Vector3(0.1f, 0.06f, 0.6f), seatC, 0f, "Reposabrazos");
        KitchenProps.C(exam, new Vector3(0f, 0.2f, 0f), new Vector3(0.08f, 0.4f, 0.08f), KitchenProps.Steel, 0f, "Pie");
        KitchenProps.C(exam, new Vector3(0.75f, 0.45f, 0.3f), new Vector3(0.4f, 0.9f, 0.4f), KitchenProps.Steel, 0f, "Carrito_examen");   // tensiómetro, linterna, tubos y vacunas
        Label(exam, "Silla de exámenes (iniciación)", 1.4f);
        Station(InfirmaryProps.DoctorDesk(r, "Mesa del medico", new Vector3(-6.6f, 0f, 0.6f), 180f), HospitalService.Consulta, "Consulta 1 (mesa del médico)", 1.5f, true);
        Label(InfirmaryProps.ExamTable(r, "Camilla", new Vector3(-3.4f, 0f, 0.4f), 90f), "Camilla de exploración", 1.3f);
        Label(InfirmaryProps.MedicineCabinet(r, "Botiquin", new Vector3(-11.1f, 0f, 2.4f), -90f), "Botiquín", 2.2f);
        KitchenProps.Sink(r, "Lavamanos", new Vector3(-11.1f, 0f, -1.2f), -90f);
        Station(InfirmaryProps.DoctorDesk(r, "Consulta 2", new Vector3(-9f, 0f, -3f), 180f), HospitalService.Consulta, "Consulta 2", 1.5f, true);
        Label(InfirmaryProps.ExamTable(r, "Camilla 2", new Vector3(-6.4f, 0f, -3.2f), 90f), "Camilla", 1.3f);
        Station(InfirmaryProps.Telemedicine(r, "Telemedicina", new Vector3(-3.4f, 0f, -3f), 180f), HospitalService.Telemedicina, "Telemedicina (especialistas a distancia)", 2.4f);
        for (int i = 0; i < 3; i++)
        {
            Transform icu = InfirmaryProps.HospitalBed(r, $"UCI {i + 1}", new Vector3(-10.6f + i * 2.2f, 0f, -7f), 0f, bl[(i + 2) % 4]);
            InfirmaryProps.Ventilator(icu, "Ventilador", new Vector3(-0.85f, 0f, 0.3f), 90f);
            Label(icu, $"UCI {i + 1}", 1.6f);
        }
        Station(InfirmaryProps.DoctorDesk(r, "Control_UCI", new Vector3(-3.3f, 0f, -7.2f), 90f), HospitalService.UCI, "Control de la UCI", 1.5f, true);

        // ── PLANTA BAJA, ESTE: espera, farmacia, imagen, paritorio, laboratorio + banco de sangre, quirófano ──
        Transform wait = KitchenProps.Root(r, "Sala_de_espera", new Vector3(6.5f, 0f, hz - 1.4f));
        for (int i = 0; i < 10; i++)
        {
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.45f, -(i / 5) * 1.2f), new Vector3(0.5f, 0.08f, 0.5f), new Color(0.45f, 0.62f, 0.72f), 0f, "Silla");
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.75f, -(i / 5) * 1.2f + 0.24f), new Vector3(0.5f, 0.55f, 0.06f), new Color(0.45f, 0.62f, 0.72f), 0f, "Respaldo");
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.22f, -(i / 5) * 1.2f), new Vector3(0.06f, 0.44f, 0.06f), KitchenProps.Steel, 0f, "Pata");
        }
        Label(wait, "Sala de espera", 1.5f);
        Station(InfirmaryProps.PharmacyShelf(r, "Farmacia", new Vector3(5.6f, 0f, 7.8f), 0f), HospitalService.Farmacia, "Farmacia", 2.3f, false, new Vector3(0f, 0f, -1.1f));
        Station(InfirmaryProps.XRay(r, "Rayos X", new Vector3(3.2f, 0f, 2.8f), 0f), HospitalService.Imagen, "Rayos X", 2.6f, false, new Vector3(0.9f, 0f, 1f));
        Station(InfirmaryProps.Ultrasound(r, "Ecografo", new Vector3(5.4f, 0f, 3.8f), 0f), HospitalService.Imagen, "Ecógrafo", 1.6f);
        Label(InfirmaryProps.ExamTable(r, "Camilla eco", new Vector3(6.6f, 0f, 3.8f), 0f), "Camilla de ecografía", 1.3f);
        Station(InfirmaryProps.DeliveryBed(r, "Paritorio", new Vector3(9.4f, 0f, 3.8f), 180f), HospitalService.Paritorio, "Paritorio (cuna térmica)", 1.9f, false, new Vector3(0f, 0f, 1.2f));
        Label(InfirmaryProps.LabBench(r, "Laboratorio", new Vector3(3.9f, 0f, -1.2f), 180f), "Laboratorio (microscopio, centrífuga)", 1.4f);
        Transform an = InfirmaryProps.BloodAnalyzer(r, "Analizador clinico", new Vector3(7.2f, 0f, -1.2f), 180f);
        an.gameObject.AddComponent<ClinicalStation>();
        Label(an, "Analizador clínico (F)", 1.8f);
        Station(InfirmaryProps.BloodBankFridge(r, "Banco_de_sangre", new Vector3(9.6f, 0f, -1.2f), 180f), HospitalService.BancoDeSangre, "Banco de sangre", 2.2f, false, new Vector3(0f, 0f, -0.9f));
        // QUIRÓFANO: mesa, lámpara, anestesia a la cabecera, instrumental, lavado quirúrgico y esterilización al lado.
        Transform or = InfirmaryProps.OperatingTable(r, "Quirofano", new Vector3(6.4f, 0f, -6.2f), 0f);
        Station(or, HospitalService.Quirofano, "Quirófano: mesa quirúrgica", 1.6f, true, new Vector3(0.9f, 0f, 0.2f));
        InfirmaryProps.SurgicalLight(r, "Lampara_quirurgica", new Vector3(5.4f, 0f, -6.2f), 90f);
        Station(InfirmaryProps.AnesthesiaMachine(r, "Anestesia", new Vector3(6.4f, 0f, -8.1f), 0f), HospitalService.Anestesia, "Máquina de anestesia", 2f, true, new Vector3(0.9f, 0f, 0f));
        Label(InfirmaryProps.InstrumentTable(r, "Instrumental", new Vector3(8.3f, 0f, -5f), 0f), "Instrumental", 1.3f);
        Station(InfirmaryProps.ScrubSink(r, "Lavado_quirurgico", new Vector3(3f, 0f, -4.6f), 90f), HospitalService.Esterilizacion, "Lavado quirúrgico", 1.9f, false, new Vector3(0f, 0f, -0.9f));
        Station(InfirmaryProps.Autoclave(r, "Autoclave", new Vector3(10.6f, 0f, -4.4f), -90f), HospitalService.Esterilizacion, "Esterilización (autoclave)", 2f, false, new Vector3(0f, 0f, -1f));
        Station(InfirmaryProps.OxygenPlant(r, "Planta_oxigeno", new Vector3(10.4f, 0f, -7.6f), -90f), HospitalService.Oxigeno, "Planta de oxígeno", 2.6f, false, new Vector3(0f, 0f, -1.1f));

        // ── PLANTA ALTA (a 10 m): HOSPITAL VETERINARIO con 30 plazas ──
        Transform up = new GameObject("Planta_veterinaria").transform; up.SetParent(area.transform, false);
        Vector2 us = StackOn(up, area.transform, HospitalSize);
        float ux = us.x * 0.5f, uz = us.y * 0.5f;
        Transform uf = Solid(up, new Vector3(0f, 0.05f, 0f), new Vector3(us.x, 0.1f, us.y), new Color(0.82f, 0.9f, 0.86f), "Suelo_veterinaria");
        Label(uf, "Hospital veterinario (planta alta)", 2.2f);
        Transform v = new GameObject("Equipamiento_veterinario").transform; v.SetParent(up, false); v.localPosition = new Vector3(0f, 0.1f, 0f);
        int place = 1;
        for (int i = 0; i < 6; i++, place++) Label(InfirmaryProps.LargeAnimalStall(v, $"Cuadra {i + 1}", new Vector3(-10.5f + i * 4.2f, 0f, -uz + 1.8f), 0f), $"Plaza {place}: cuadra (oso, foca, lobo)", 1.8f);
        for (int i = 0; i < 8; i++, place++) Label(InfirmaryProps.Kennel(v, $"Box {i + 1}", new Vector3(-ux + 1.4f, 0f, -6f + i * 1.7f), 90f), $"Plaza {place}: box", 1.6f);
        for (int i = 0; i < 8; i++, place++) Label(InfirmaryProps.Kennel(v, $"Box {i + 9}", new Vector3(ux - 1.4f, 0f, -6f + i * 1.7f), -90f), $"Plaza {place}: box", 1.6f);
        for (int i = 0; i < 2; i++, place += 4) Label(InfirmaryProps.Cages(v, $"Jaulas {i + 1}", new Vector3(-1.2f + i * 2.4f, 0f, -5.4f), 0f), $"Plazas {place}-{place + 3}: jaulas (pingüino, zorro, aves)", 1.6f);
        Station(InfirmaryProps.VetTable(v, "Mesa veterinaria", new Vector3(-5f, 0f, 2.6f), 90f), HospitalService.Veterinaria, "Consulta veterinaria", 1.4f, true, new Vector3(0f, 0f, -1.1f));
        Station(InfirmaryProps.Ultrasound(v, "Ecografo veterinario", new Vector3(-3.2f, 0f, 2.8f), 0f), HospitalService.Imagen, "Ecógrafo", 1.6f);
        Label(InfirmaryProps.FloorScale(v, "Bascula", new Vector3(-5f, 0f, 6.4f), 0f), "Báscula (dosis por kilo)", 1.5f);
        Station(InfirmaryProps.VetTable(v, "Quirofano veterinario", new Vector3(5.4f, 0f, 2.6f), 90f), HospitalService.CirugiaVeterinaria, "Quirófano veterinario", 1.4f, true, new Vector3(0f, 0f, -1.1f));
        InfirmaryProps.SurgicalLight(v, "Lampara_quirurgica_vet", new Vector3(4.2f, 0f, 2.6f), 90f);
        Station(InfirmaryProps.AnesthesiaMachine(v, "Anestesia veterinaria", new Vector3(7.4f, 0f, 2.6f), -90f), HospitalService.Anestesia, "Anestesia veterinaria", 2f, false, new Vector3(0f, 0f, -0.9f));
        Label(InfirmaryProps.MedicineCabinet(v, "Botiquin veterinario", new Vector3(5.4f, 0f, 6.8f), 180f), "Farmacia veterinaria", 2.2f);
        Debug.Log($"[Hospital] 2 plantas: {bed - 1} camas para personas (3 de observación + 30 de planta en la galería a {g} m) y {place - 1} plazas para animales; {Object.FindObjectsByType<HospitalStation>(FindObjectsSortMode.None).Length} puestos interactivos.");

        // Linduarda en su mesa; Lizmibet pasando visita en la galería; el paciente de prueba (lobo) arriba, en una cuadra.
        GameObject l = GameObject.Find("Linduarda");
        if (l != null) l.transform.position = area.transform.TransformPoint(new Vector3(-6.6f, 1f, 1.9f));
        GameObject lz = GameObject.Find("Lizmibet");
        if (lz != null) lz.transform.position = area.transform.TransformPoint(new Vector3(-hx + 3.7f, 0.1f + g + 1f, -3.5f));
        GameObject p = GameObject.Find("Lobo_paciente");
        if (p != null) p.transform.position = v.TransformPoint(new Vector3(-10.5f, 0.15f, -uz + 1.8f));
        // Kushal (el jugador) EMPIEZA AQUÍ, junto a las camas de observación: es el recién llegado.
        GameObject kushal = GameObject.Find("Player");
        if (kushal != null) kushal.transform.position = area.transform.TransformPoint(new Vector3(-7.6f, 1.3f, 2.4f));
        // La doctora inicia a los recién llegados en el TANQUE del hospital (no en cualquier máquina).
        VirtualizationMachine vm = area.GetComponentInChildren<VirtualizationMachine>();
        if (l != null && vm != null && l.TryGetComponent(out Linduarda lin)) lin.virtualizationMachine = vm;
    }
}
