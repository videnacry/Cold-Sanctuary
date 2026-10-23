using UnityEngine;

/// <summary>
/// EL HOSPITAL (antes Enfermería; docs/hospital.md). No hay otro hospital a miles de km, así que tiene lo mínimo de un
/// hospital de primer nivel de la OMS para tratar todos los casos sin derivar: urgencias con reanimación, quirófano
/// (cirujano + anestesista, esterilización, lavado quirúrgico), UCI, paritorio, aislamiento, imagen, laboratorio y banco
/// de sangre, farmacia, oxígeno propio y telemedicina. Tres plantas sobre la misma base (docs/hospital-plan.md):
/// - PLANTA BAJA (personas): ambulatorio y servicios centrales con 2 m libres entre objetos y salas de norma; PLANTA 1:
///   las 30 CAMAS (2 m entre camas) y el aislamiento con esclusa en su subplataforma. Abajo sigue la iniciación (silla de exámenes,
///   analizador clínico, mesa del médico y el tanque de virtualización).
/// - PLANTA 2 (animales): hospital veterinario con 30 PLAZAS (6 cuadras, 16 boxes, 8 jaulas), consulta y
///   quirófano veterinarios. Mismo número de plazas para animales que camas para personas.
/// 8 PUESTOS MÉDICOS (HospitalStation.doctorPost): urgencias, 2 consultas, UCI, cirujano, anestesista, veterinaria y
/// cirugía veterinaria; la jornada coloca a cada médico en uno libre.
/// </summary>
public static partial class SampleSceneBuilder
{
    public static readonly Vector2 HospitalSize = new Vector2(32f, 26f);
    public const float HospitalBedDeck = MezzanineHeight;
    // Cama de ~1,12 m de ancho medida (barandas y ruedas; gotero y monitor van detrás del cabecero) + 0,9 m de pasillo
    // entre camas (FurnitureSpacing: acceso por los dos lados) + margen → 2,1 m de eje a eje.

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


        // DISTRIBUCIÓN REAL (docs/hospital-plan.md, Berón 2026-10-23): 2 m LIBRES entre objetos para pasar entre ellos;
        // salas con su superficie de norma (paritorio 32 m², UCI 18,6 m² por cama, quirófano ~37 m²), marcadas en el suelo.
        // PLANTA BAJA = ambulatorio y servicios centrales; PLANTA 1 = hospitalización (30 camas) y AISLAMIENTO en una
        // subplataforma con esclusa; PLANTA 2 = hospital veterinario.
        Color[] bl = { new Color(0.62f, 0.8f, 0.92f), new Color(0.75f, 0.9f, 0.8f), new Color(0.95f, 0.85f, 0.7f), new Color(0.9f, 0.78f, 0.85f) };
        void Zone(Transform parent, string name, Vector3 c, Vector2 size, Color col) => KitchenProps.C(parent, c + Vector3.up * 0.025f, new Vector3(size.x, 0.01f, size.y), col, 0f, name);
        Color zoneC = new Color(0.8f, 0.9f, 0.88f), zoneW = new Color(0.9f, 0.86f, 0.8f);

        // ── PLANTA BAJA, OESTE: entrada (triaje), consultas e iniciación, observación de urgencias y UCI ──
        Transform triage = InfirmaryProps.DoctorDesk(r, "Triaje", new Vector3(-4.5f, 0f, 10.5f), 180f);
        Label(triage, "Recepción y triaje", 1.5f);
        Aseos(r, new Vector3(-13.5f, 0f, 11.4f), 180f);
        // Consultas (una fila, 2 m entre muebles): silla de exámenes de la iniciación, mesa del médico, camilla.
        Transform exam = KitchenProps.Root(r, "Silla_examen", new Vector3(-12.6f, 0f, 5.5f), 90f);
        Color seatC = new Color(0.35f, 0.55f, 0.65f);
        KitchenProps.C(exam, new Vector3(0f, 0.45f, 0f), new Vector3(0.6f, 0.1f, 0.6f), seatC, 0f, "Asiento_examen");
        KitchenProps.C(exam, new Vector3(0f, 0.85f, -0.27f), new Vector3(0.6f, 0.75f, 0.08f), seatC, 0f, "Respaldo");
        KitchenProps.C(exam, new Vector3(0.36f, 0.68f, 0.05f), new Vector3(0.1f, 0.06f, 0.6f), seatC, 0f, "Reposabrazos");
        KitchenProps.C(exam, new Vector3(0f, 0.2f, 0f), new Vector3(0.08f, 0.4f, 0.08f), KitchenProps.Steel, 0f, "Pie");
        KitchenProps.C(exam, new Vector3(0.75f, 0.45f, 0.3f), new Vector3(0.4f, 0.9f, 0.4f), KitchenProps.Steel, 0f, "Carrito_examen");   // tensiómetro, linterna, tubos y vacunas
        Label(exam, "Silla de exámenes (iniciación)", 1.4f);
        Station(InfirmaryProps.DoctorDesk(r, "Mesa del medico", new Vector3(-8.4f, 0f, 5.5f), 180f), HospitalService.Consulta, "Consulta 1 (mesa del médico)", 1.5f, true);
        Label(InfirmaryProps.ExamTable(r, "Camilla", new Vector3(-4.4f, 0f, 5.5f), 90f), "Camilla de exploración", 1.3f);
        Label(InfirmaryProps.MedicineCabinet(r, "Botiquin", new Vector3(-15.4f, 0f, 8f), -90f), "Botiquín", 2.2f);
        KitchenProps.Sink(r, "Lavamanos", new Vector3(-15.4f, 0f, 2.5f), -90f);
        Station(InfirmaryProps.DoctorDesk(r, "Consulta 2", new Vector3(-12.6f, 0f, 0.5f), 180f), HospitalService.Consulta, "Consulta 2", 1.5f, true);
        Label(InfirmaryProps.ExamTable(r, "Camilla 2", new Vector3(-8.4f, 0f, 0.5f), 90f), "Camilla", 1.3f);
        Station(InfirmaryProps.Telemedicine(r, "Telemedicina", new Vector3(-4.4f, 0f, 0.5f), 180f), HospitalService.Telemedicina, "Telemedicina (especialistas a distancia)", 2.4f);
        // Observación de urgencias: 3 camas a 3,1 m de eje (2 m libres entre ellas) y el carro de parada.
        for (int i = 0; i < 3; i++) Label(InfirmaryProps.HospitalBed(r, $"Cama {i + 1}", new Vector3(-13.6f + i * 3.1f, 0f, -4.4f), 0f, bl[i % 3]), $"Cama {i + 1} (observación)", 1.4f);
        Station(InfirmaryProps.CrashCart(r, "Carro_de_parada", new Vector3(-4.2f, 0f, -4.4f), -90f), HospitalService.Urgencias, "Urgencias: carro de parada y desfibrilador", 1.6f, true, new Vector3(0f, 0f, -0.9f));
        // UCI: 3 boxes de 4,3 × 4,3 m (≈ 18,6 m² por cama, la norma) con su ventilador, y el control.
        for (int i = 0; i < 3; i++)
        {
            Vector3 c = new Vector3(-13.9f + i * 4.3f, 0f, -10.6f);
            Zone(r, "Box_UCI", c, new Vector2(4.2f, 4.2f), zoneC);
            Transform icu = InfirmaryProps.HospitalBed(r, $"UCI {i + 1}", c, 0f, bl[(i + 2) % 4]);
            InfirmaryProps.Ventilator(icu, "Ventilador", new Vector3(-0.85f, 0f, 0.3f), 90f);
            Label(icu, $"UCI {i + 1}", 1.6f);
        }
        Station(InfirmaryProps.DoctorDesk(r, "Control_UCI", new Vector3(-2.6f, 0f, -10.6f), 90f), HospitalService.UCI, "Control de la UCI", 1.5f, true);

        // ── PLANTA BAJA, ESTE: espera, farmacia, imagen (rayos X y eco en salas separadas), laboratorio y banco de sangre,
        //    paritorio (sala de 32 m²) y bloque quirúrgico (quirófano ~37 m² + esterilización) ──
        Transform wait = KitchenProps.Root(r, "Sala_de_espera", new Vector3(5.2f, 0f, 11f));
        for (int i = 0; i < 10; i++)
        {
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.45f, -(i / 5) * 1.2f), new Vector3(0.5f, 0.08f, 0.5f), new Color(0.45f, 0.62f, 0.72f), 0f, "Silla");
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.75f, -(i / 5) * 1.2f + 0.24f), new Vector3(0.5f, 0.55f, 0.06f), new Color(0.45f, 0.62f, 0.72f), 0f, "Respaldo");
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.22f, -(i / 5) * 1.2f), new Vector3(0.06f, 0.44f, 0.06f), KitchenProps.Steel, 0f, "Pata");
        }
        Label(wait, "Sala de espera", 1.5f);
        Station(InfirmaryProps.PharmacyShelf(r, "Farmacia", new Vector3(10f, 0f, 11.9f), 0f), HospitalService.Farmacia, "Farmacia", 2.3f, false, new Vector3(0f, 0f, -1.1f));
        Zone(r, "Sala_rayos_X", new Vector3(4.6f, 0f, 5f), new Vector2(5f, 5f), zoneW);
        Station(InfirmaryProps.XRay(r, "Rayos X", new Vector3(4.6f, 0f, 5f), 0f), HospitalService.Imagen, "Rayos X (sala blindada)", 2.6f, false, new Vector3(0.9f, 0f, 1f));
        Zone(r, "Sala_ecografia", new Vector3(11.2f, 0f, 5f), new Vector2(4.2f, 3.4f), zoneC);
        Station(InfirmaryProps.Ultrasound(r, "Ecografo", new Vector3(10.4f, 0f, 5.2f), 0f), HospitalService.Imagen, "Ecógrafo", 1.6f);
        Label(InfirmaryProps.ExamTable(r, "Camilla eco", new Vector3(11.8f, 0f, 5.2f), 0f), "Camilla de ecografía", 1.3f);
        Label(InfirmaryProps.LabBench(r, "Laboratorio", new Vector3(3.6f, 0f, -0.6f), 180f), "Laboratorio (microscopio, centrífuga)", 1.4f);
        Transform an = InfirmaryProps.BloodAnalyzer(r, "Analizador clinico", new Vector3(7.2f, 0f, -0.6f), 180f);
        an.gameObject.AddComponent<ClinicalStation>();
        Label(an, "Analizador clínico (F)", 1.8f);
        Station(InfirmaryProps.BloodBankFridge(r, "Banco_de_sangre", new Vector3(3.6f, 0f, -4.4f), 180f), HospitalService.BancoDeSangre, "Banco de sangre", 2.2f, false, new Vector3(0f, 0f, -0.9f));
        Zone(r, "Sala_paritorio", new Vector3(13.2f, 0f, -1.4f), new Vector2(5.6f, 5.7f), new Color(0.95f, 0.88f, 0.9f));   // 32 m² (norma LDR: 31,6 m², lado ≥ 3,96 m)
        Station(InfirmaryProps.DeliveryBed(r, "Paritorio", new Vector3(13.2f, 0f, -1.4f), 180f), HospitalService.Paritorio, "Paritorio (sala de 32 m², cuna térmica)", 1.9f, false, new Vector3(0f, 0f, 1.2f));
        // QUIRÓFANO (6 × 6 m) con lavado quirúrgico a su entrada; ESTERILIZACIÓN y oxígeno al lado.
        Zone(r, "Sala_quirofano", new Vector3(7f, 0f, -10f), new Vector2(6f, 6f), new Color(0.78f, 0.9f, 0.86f));
        Transform or = InfirmaryProps.OperatingTable(r, "Quirofano", new Vector3(7f, 0f, -10f), 0f);
        Station(or, HospitalService.Quirofano, "Quirófano: mesa quirúrgica", 1.6f, true, new Vector3(0.9f, 0f, 0.2f));
        InfirmaryProps.SurgicalLight(r, "Lampara_quirurgica", new Vector3(5.6f, 0f, -10f), 90f);
        Station(InfirmaryProps.AnesthesiaMachine(r, "Anestesia", new Vector3(7f, 0f, -12.4f), 0f), HospitalService.Anestesia, "Máquina de anestesia", 2f, true, new Vector3(0.9f, 0f, 0f));
        Label(InfirmaryProps.InstrumentTable(r, "Instrumental", new Vector3(9.3f, 0f, -8.4f), 0f), "Instrumental", 1.3f);
        Station(InfirmaryProps.ScrubSink(r, "Lavado_quirurgico", new Vector3(3.2f, 0f, -8.6f), 90f), HospitalService.Esterilizacion, "Lavado quirúrgico", 1.9f, false, new Vector3(0f, 0f, -0.9f));
        Station(InfirmaryProps.Autoclave(r, "Autoclave", new Vector3(14.4f, 0f, -7.6f), -90f), HospitalService.Esterilizacion, "Esterilización (autoclave)", 2f, false, new Vector3(0f, 0f, -1f));
        Station(InfirmaryProps.OxygenPlant(r, "Planta_oxigeno", new Vector3(14.4f, 0f, -11.6f), -90f), HospitalService.Oxigeno, "Planta de oxígeno", 2.6f, false, new Vector3(0f, 0f, -1.1f));

        // ── PLANTA 1: HOSPITALIZACIÓN. 30 camas a 3,1 m de eje (2 m libres entre camas y 2 m de paso a los pies) y el
        //    AISLAMIENTO en una SUBPLATAFORMA propia, con esclusa (antesala) y una sola entrada por su escalera ──
        Transform p1 = new GameObject("Planta_hospitalizacion").transform; p1.SetParent(area.transform, false);
        Vector2 s1 = StackOn(p1, area.transform, HospitalSize);
        float x1 = s1.x * 0.5f, z1 = s1.y * 0.5f;
        Transform f1 = Solid(p1, new Vector3(0f, 0.05f, 0f), new Vector3(s1.x, 0.1f, s1.y), new Color(0.84f, 0.92f, 0.9f), "Suelo_hospitalizacion");
        Label(f1, "Hospitalización (planta 1)", 2.2f);
        Transform w1 = new GameObject("Sala_de_camas").transform; w1.SetParent(p1, false); w1.localPosition = new Vector3(0f, 0.1f, 0f);
        int bed = 4;   // las camas 1-3 son las de observación de urgencias (abajo)
        float[] westZ = { -8.4f, -5.3f, -2.2f, 0.9f, 4f, 7.1f, 10.2f }, eastZ = { -8.4f, -5.3f, -2.2f, 0.9f };   // al este, delante, va el aislamiento
        for (int i = 0; i < westZ.Length; i++, bed++) Label(InfirmaryProps.HospitalBed(w1, $"Cama {bed}", new Vector3(-x1 + 1.4f, 0f, westZ[i]), 90f, bl[i % 4]), $"Cama {bed}", 1.4f);
        for (int i = 0; i < eastZ.Length; i++, bed++) Label(InfirmaryProps.HospitalBed(w1, $"Cama {bed}", new Vector3(x1 - 1.4f, 0f, eastZ[i]), -90f, bl[(i + 1) % 4]), $"Cama {bed}", 1.4f);
        for (int i = 0; i < 7; i++, bed++) Label(InfirmaryProps.HospitalBed(w1, $"Cama {bed}", new Vector3(-9.3f + i * 3.1f, 0f, -z1 + 1.4f), 0f, bl[(i + 2) % 4]), $"Cama {bed}", 1.4f);
        for (int i = 0; i < 6; i++, bed++) Label(InfirmaryProps.HospitalBed(w1, $"Cama {bed}", new Vector3(-7.75f + i * 3.1f, 0f, -2.4f), 180f, bl[(i + 3) % 4]), $"Cama {bed}", 1.4f);   // isla central: dos filas con 2 m de paso entre cabeceros
        for (int i = 0; i < 6; i++, bed++) Label(InfirmaryProps.HospitalBed(w1, $"Cama {bed}", new Vector3(-7.75f + i * 3.1f, 0f, 2.4f), 0f, bl[i % 4]), $"Cama {bed}", 1.4f);
        // Aislamiento: subplataforma a 1 m en la esquina delantera este, con su escalera que llega a la ESCLUSA.
        const float isoH = 1f;
        Transform iso = Deck(p1, "Subplataforma_aislamiento", new Vector3(x1 - 4.2f, 0.1f, z1 - 4f), new Vector2(7f, 6f), isoH);
        // La escalera sube por el oeste y llega a la ESCLUSA (franja de 2 m con el EPI); su segunda puerta da a la habitación.
        KitchenProps.C(iso, new Vector3(-2.5f, 0.02f, 0f), new Vector3(2f, 0.02f, 6f), new Color(0.95f, 0.92f, 0.75f), 0f, "Esclusa_antesala");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(iso, new Vector3(-1.5f, 1.15f, s * 0.75f), new Vector3(0.08f, 2.3f, 0.08f), new Color(0.6f, 0.65f, 0.7f), 0f, "Puerta_interior");
        Station(InfirmaryProps.IsolationRoom(iso, "Aislamiento", new Vector3(1f, 0f, 0f), -90f, bl[1]), HospitalService.Aislamiento, "Aislamiento (infecciosos): presión negativa y esclusa", 2.6f, false, new Vector3(0f, 0f, 2.2f));
        InclinedStair(p1, "Escalera_aislamiento", new Vector3(x1 - 7.7f - StairRun(isoH), 0.1f, z1 - 4f), 90f, isoH);
        Label(iso, "Aislamiento (entrada solo por la esclusa)", 2.4f);
        int humanBeds = bed - 1;
        // ── PLANTA 2 (encima de la hospitalización): HOSPITAL VETERINARIO con 30 plazas ──
        Transform up = new GameObject("Planta_veterinaria").transform; up.SetParent(area.transform, false);
        Vector2 us = StackOn(up, p1, s1);
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
        Debug.Log($"[Hospital] 3 plantas: {humanBeds} camas para personas (3 de observación abajo + 30 de hospitalización en la planta 1, con 2 m entre camas), aislamiento con esclusa en su subplataforma y {place - 1} plazas para animales; {Object.FindObjectsByType<HospitalStation>(FindObjectsSortMode.None).Length} puestos interactivos.");

        // Linduarda en su mesa; Lizmibet pasando visita en la planta 1; el paciente de prueba (lobo) arriba, en una cuadra.
        GameObject l = GameObject.Find("Linduarda");
        if (l != null) l.transform.position = area.transform.TransformPoint(new Vector3(-8.4f, 1f, 4.2f));
        GameObject lz = GameObject.Find("Lizmibet");
        if (lz != null) lz.transform.position = w1.TransformPoint(new Vector3(-x1 + 4.2f, 1f, -3.75f));   // en el pasillo entre camas
        GameObject p = GameObject.Find("Lobo_paciente");
        if (p != null) p.transform.position = v.TransformPoint(new Vector3(-10.5f, 0.15f, -uz + 1.8f));
        // Kushal (el jugador) EMPIEZA AQUÍ, junto a las camas de observación: es el recién llegado.
        GameObject kushal = GameObject.Find("Player");
        if (kushal != null) kushal.transform.position = area.transform.TransformPoint(new Vector3(-10.4f, 1.3f, 2.8f));
        // La doctora inicia a los recién llegados en el TANQUE del hospital (no en cualquier máquina).
        VirtualizationMachine vm = area.GetComponentInChildren<VirtualizationMachine>();
        if (l != null && vm != null && l.TryGetComponent(out Linduarda lin)) lin.virtualizationMachine = vm;
    }
}
