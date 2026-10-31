using UnityEngine;

/// <summary>
/// EL HOSPITAL (antes Enfermería; docs/hospital.md). No hay otro hospital a miles de km, así que tiene lo mínimo de un
/// hospital de primer nivel de la OMS para tratar todos los casos sin derivar: urgencias con reanimación, quirófano
/// (cirujano + anestesista, esterilización, lavado quirúrgico), UCI, paritorio, aislamiento, imagen, laboratorio y banco
/// de sangre, farmacia, oxígeno propio y telemedicina. Tres plantas sobre la misma base (docs/hospital-plan.md v2), con
/// 4 PUESTOS por servicio (cuatro médicos a la vez en la misma tarea) y 2 m libres entre objetos:
/// - PLANTA BAJA: urgencias y ambulatorio (la iniciación sigue aquí: silla de exámenes, analizador clínico, mesa del
///   médico y el tanque) + ANILLO de observación y maternidad.
/// - PLANTA 1: 4 quirófanos, UCI de 6 y 4 aislamientos con esclusa + ANILLO de recuperación y hospitalización.
/// - PLANTA 2: hospital veterinario (docs/vet-hospital-plan.md) + ANILLO de grandes animales (osos, focas).
/// Mismo número de plazas para animales que camas para personas (48).
/// </summary>
public static partial class SampleSceneBuilder
{
    public static readonly Vector2 HospitalSize = new Vector2(32f, 26f);

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
        // TORRE DE SERVICIOS (docs/plano-edificios.md): cada planta del hospital mide lo que la torre (la del restaurante,
        // F); la distribución sigue siendo la de 32 × 26 (HospitalSize), centrada, y los ANILLOS van al borde de la planta.
        Vector2 F = FloorSize(area.transform, HospitalSize);
        float layZ = HospitalSize.y * 0.5f;   // (las filas de perreras, desde el borde −Z de la distribución)
        Transform r = new GameObject("Equipamiento").transform;
        r.SetParent(area.transform, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Suelo_Hospital"; floor.transform.SetParent(r, false);
        floor.transform.localPosition = new Vector3(0f, 0.005f, 0f); floor.transform.localScale = new Vector3(F.x - 0.4f, 0.04f, F.y - 0.4f);
        floor.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Hospital_Suelo_MAT", new Color(0.86f, 0.94f, 0.92f));
        // Pasillo central (entrada → fondo), libre de muebles: 3 m de ancho.
        KitchenProps.C(r, new Vector3(0f, 0.03f, 1.9f), new Vector3(3f, 0.01f, HospitalSize.y - 7.6f), new Color(0.72f, 0.86f, 0.84f), 0f, "Pasillo_central");


        // DISTRIBUCIÓN REAL v2 (docs/hospital-plan.md, Berón 2026-10-23): 2 m LIBRES entre objetos; CAPACIDAD ×4 (cuatro
        // médicos pueden hacer la misma tarea a la vez); salas de norma marcadas en el suelo; cada planta con su ANILLO de
        // camas (plataforma hija cuadrada a 4 m, 20 % dentro del borde y 80 % volando), cada uno con las camas de su servicio.
        // PLANTA BAJA = urgencias y ambulatorio (+ anillo de observación y maternidad); PLANTA 1 = bloque quirúrgico,
        // UCI y aislamiento (+ anillo de recuperación y hospitalización); PLANTA 2 = hospital veterinario.
        Color[] bl = { new Color(0.62f, 0.8f, 0.92f), new Color(0.75f, 0.9f, 0.8f), new Color(0.95f, 0.85f, 0.7f), new Color(0.9f, 0.78f, 0.85f) };
        void Zone(Transform parent, string name, Vector3 c, Vector2 size, Color col) => KitchenProps.C(parent, c + Vector3.up * 0.025f, new Vector3(size.x, 0.01f, size.y), col, 0f, name);
        Color zoneC = new Color(0.8f, 0.9f, 0.88f), zoneW = new Color(0.9f, 0.86f, 0.8f), zoneP = new Color(0.95f, 0.88f, 0.9f), zoneQ = new Color(0.78f, 0.9f, 0.86f);
        int bed = 1;
        // Camas por el ANILLO, atravesadas (2 m libres entre camas = 3,1 m de eje; 4 m de paso a cada lado), sin las esquinas
        // ni el hueco de su área de alfombras (centro del lado +Z; antes, el de las escaleras).
        var ringNext = new System.Collections.Generic.Dictionary<Transform, float>();   // por dónde va cada anillo (las categorías siguen una tras otra)
        int RingBeds(Transform ring, Vector2 floor, int count, string category)
        {
            int placed = 0;
            float s = ringNext.TryGetValue(ring, out float sv) ? sv : 1.55f;
            for (; s < RingLength(floor) && placed < count; s += 3.2f)
            {
                var (p, yaw, corner) = RingPoint(floor, s, cornerClear: 2.6f);
                if (corner || (Mathf.Abs(p.x) < 4.6f && Mathf.Abs(p.z) > floor.y * 0.5f - 2f)) continue;   // (el centro del lado +Z es su área de alfombras)
                Label(InfirmaryProps.HospitalBed(ring, $"Cama {bed}", p, yaw, bl[bed % 4]), $"Cama {bed} ({category})", 1.4f);
                bed++; placed++;
            }
            ringNext[ring] = s;
            return placed;
        }
        // SIN ESCALERAS (Berón 2026-10-28): a cada anillo se sube en ALFOMBRA desde el área de alfombras de su planta; la del
        // anillo va en el centro de su lado +Z (las camas y los boxes dejan ese hueco). Lo que ocupaban las escaleras queda libre.
        string hospitalName = BuildingOf(area.transform, area.GetComponent<SanctuaryArea>() is SanctuaryArea hsa ? hsa.displayName : AreaWork.Hospital);   // la red de alfombras: la de su torre

        // ══ PLANTA BAJA (32 × 26): URGENCIAS Y AMBULATORIO ═════════════════════════════════════════════════════════════
        // Oeste: 4 triajes (entrada), 4 consultas (una con la silla de iniciación), 2 carros de parada, laboratorio.
        Station(InfirmaryProps.DoctorDesk(r, "Triaje", new Vector3(-14f, 0f, 8.6f), 180f), HospitalService.Urgencias, "Recepción y triaje 1", 1.5f, true);
        Station(InfirmaryProps.DoctorDesk(r, "Triaje 2", new Vector3(-6.4f, 0f, 9f), 180f), HospitalService.Urgencias, "Triaje 2", 1.5f, true);
        Station(InfirmaryProps.DoctorDesk(r, "Triaje 3", new Vector3(-2.3f, 0f, 5.6f), 90f), HospitalService.Urgencias, "Triaje 3", 1.5f, true);
        Station(InfirmaryProps.DoctorDesk(r, "Triaje 4", new Vector3(14.6f, 0f, 8.4f), 180f), HospitalService.Urgencias, "Triaje 4", 1.5f, true);
        Aseos(r, new Vector3(-14f, 0f, 12.3f), 180f);
        Transform exam = KitchenProps.Root(r, "Silla_examen", new Vector3(-13.6f, 0f, 5.6f), 90f);
        Color seatC = new Color(0.35f, 0.55f, 0.65f);
        // Asiento a 0,45 m (docs/medidas-muebles.md). Su frente es +z local (el respaldo está en −z): hacia ahí mira el
        // paciente sentado (ArrivalExam.PatientSpot lee la altura del «Asiento_examen»).
        KitchenProps.C(exam, new Vector3(0f, 0.4f, 0f), new Vector3(0.6f, 0.1f, 0.6f), seatC, 0f, "Asiento_examen");
        KitchenProps.C(exam, new Vector3(0f, 0.8f, -0.27f), new Vector3(0.6f, 0.75f, 0.08f), seatC, 0f, "Respaldo");
        KitchenProps.C(exam, new Vector3(0.36f, 0.63f, 0.05f), new Vector3(0.1f, 0.06f, 0.6f), seatC, 0f, "Reposabrazos");
        KitchenProps.C(exam, new Vector3(0f, 0.175f, 0f), new Vector3(0.08f, 0.35f, 0.08f), KitchenProps.Steel, 0f, "Pie");
        KitchenProps.C(exam, new Vector3(0.75f, 0.45f, 0.3f), new Vector3(0.4f, 0.9f, 0.4f), KitchenProps.Steel, 0f, "Carrito_examen");   // tensiómetro, linterna, tubos y vacunas
        Label(exam, "Silla de exámenes (iniciación)", 1.4f);
        Station(InfirmaryProps.DoctorDesk(r, "Mesa del medico", new Vector3(-10.1f, 0f, 5.6f), 180f), HospitalService.Consulta, "Consulta 1 (mesa del médico)", 1.5f, true);
        Label(InfirmaryProps.ExamTable(r, "Camilla", new Vector3(-6.3f, 0f, 5.6f), 90f), "Camilla de exploración", 1.3f);
        Station(InfirmaryProps.DoctorDesk(r, "Consulta 2", new Vector3(-13.6f, 0f, 1.4f), 180f), HospitalService.Consulta, "Consulta 2", 1.5f, true);
        Label(InfirmaryProps.ExamTable(r, "Camilla 2", new Vector3(-9.8f, 0f, 1.4f), 90f), "Camilla", 1.3f);
        Station(InfirmaryProps.DoctorDesk(r, "Consulta 3", new Vector3(-5.6f, 0f, 1.4f), 180f), HospitalService.Consulta, "Consulta 3", 1.5f, true);
        Station(InfirmaryProps.DoctorDesk(r, "Consulta 4", new Vector3(-13.6f, 0f, -2.6f), 180f), HospitalService.Consulta, "Consulta 4", 1.5f, true);
        Label(InfirmaryProps.ExamTable(r, "Camilla 3", new Vector3(-9.8f, 0f, -2.6f), 90f), "Camilla", 1.3f);
        Station(InfirmaryProps.Telemedicine(r, "Telemedicina", new Vector3(-5.6f, 0f, -2.6f), 180f), HospitalService.Telemedicina, "Telemedicina (especialistas a distancia)", 2.4f);
        Label(InfirmaryProps.MedicineCabinet(r, "Botiquin", new Vector3(-15.4f, 0f, -6.4f), -90f), "Botiquín", 2.2f);
        KitchenProps.Sink(r, "Lavamanos", new Vector3(-6.4f, 0f, 12.3f), 180f);   // contra la pared norte (junto a la consulta no quedaban 2 m)
        // BIOCONTROL (docs/iniciacion-medica-investigacion.md): el lavabotas, en la fila de la entrada (entre los triajes 1 y 2,
        // con 2 m libres a cada mueble). Bandeja con desinfectante (Virkon S), cepillos y aspirador para las semillas del
        // velcro y los bolsillos: lo que protege a la flora y la fauna nativas.
        Transform boots = KitchenProps.Root(r, "Lavabotas", new Vector3(-10.2f, 0f, 9.0f), 180f);
        KitchenProps.C(boots, new Vector3(0f, 0.05f, 0f), new Vector3(0.9f, 0.1f, 0.6f), new Color(0.3f, 0.45f, 0.6f), 0f, "Bandeja_desinfectante");
        KitchenProps.C(boots, new Vector3(0f, 0.09f, 0f), new Vector3(0.8f, 0.02f, 0.5f), new Color(0.75f, 0.55f, 0.85f), 0f, "Virkon");
        KitchenProps.C(boots, new Vector3(-0.55f, 0.45f, 0.15f), new Vector3(0.3f, 0.9f, 0.3f), KitchenProps.Steel, 0f, "Aspirador");
        KitchenProps.C(boots, new Vector3(0.5f, 0.6f, 0.25f), new Vector3(0.05f, 1.2f, 0.05f), KitchenProps.Steel, 0f, "Barra_apoyo");
        KitchenProps.Fit(boots, solid: false);
        Label(boots, "Lavabotas y biocontrol (semillas, tierra)", 1.5f);
        Station(InfirmaryProps.CrashCart(r, "Carro_de_parada", new Vector3(-12f, 0f, -6.6f), -90f), HospitalService.Urgencias, "Urgencias: carro de parada y desfibrilador", 1.6f, true, new Vector3(0f, 0f, -0.9f));
        Station(InfirmaryProps.CrashCart(r, "Carro_de_parada 2", new Vector3(-6.4f, 0f, -6.6f), -90f), HospitalService.Urgencias, "Carro de parada 2", 1.6f, true, new Vector3(0f, 0f, -0.9f));
        Label(InfirmaryProps.LabBench(r, "Laboratorio", new Vector3(-13.4f, 0f, -10.6f), 180f), "Laboratorio (microscopio, centrífuga)", 1.4f);
        Transform an = InfirmaryProps.BloodAnalyzer(r, "Analizador clinico", new Vector3(-9.25f, 0f, -10.6f), 180f);
        an.gameObject.AddComponent<ClinicalStation>();
        Label(an, "Analizador clínico (F)", 1.8f);
        Station(InfirmaryProps.BloodBankFridge(r, "Banco_de_sangre", new Vector3(-6f, 0f, -10.6f), 180f), HospitalService.BancoDeSangre, "Banco de sangre", 2.2f, false, new Vector3(0f, 0f, -0.9f));
        // Este: espera y farmacia (entrada), imagen (2 rayos X, 2 ecos) y 4 PARITORIOS de 32 m².
        Transform wait = KitchenProps.Root(r, "Sala_de_espera", new Vector3(5.6f, 0f, 11.2f));
        for (int i = 0; i < 10; i++)
        {
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.41f, -(i / 5) * 1.2f), new Vector3(0.5f, 0.08f, 0.5f), new Color(0.45f, 0.62f, 0.72f), 0f, "Silla");   // asiento a 0,45 m
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.71f, -(i / 5) * 1.2f + 0.24f), new Vector3(0.5f, 0.55f, 0.06f), new Color(0.45f, 0.62f, 0.72f), 0f, "Respaldo");
            KitchenProps.C(wait, new Vector3(-1.8f + (i % 5) * 0.9f, 0.185f, -(i / 5) * 1.2f), new Vector3(0.06f, 0.37f, 0.06f), KitchenProps.Steel, 0f, "Pata");
        }
        Label(wait, "Sala de espera", 1.5f);
        Station(InfirmaryProps.PharmacyShelf(r, "Farmacia", new Vector3(10f, 0f, 12.2f), 0f), HospitalService.Farmacia, "Farmacia", 2.3f, false, new Vector3(0f, 0f, -1.1f));
        for (int i = 0; i < 2; i++)
        {
            Zone(r, "Sala_rayos_X", new Vector3(4.8f + i * 6f, 0f, 4.6f), new Vector2(5f, 4.4f), zoneW);
            Station(InfirmaryProps.XRay(r, i == 0 ? "Rayos X" : "Rayos X 2", new Vector3(4.8f + i * 6f, 0f, 4.6f), 0f), HospitalService.Imagen, $"Rayos X {i + 1} (sala blindada)", 2.6f, false, new Vector3(0.9f, 0f, 1f));
            Zone(r, "Sala_ecografia", new Vector3(4.8f + i * 6f, 0f, 0.2f), new Vector2(5f, 3.4f), zoneC);
            Station(InfirmaryProps.Ultrasound(r, i == 0 ? "Ecografo" : "Ecografo 2", new Vector3(4f + i * 6f, 0f, 0.4f), 0f), HospitalService.Imagen, $"Ecógrafo {i + 1}", 1.6f);
            Label(InfirmaryProps.ExamTable(r, i == 0 ? "Camilla eco" : "Camilla eco 2", new Vector3(5.4f + i * 6f, 0f, 0.4f), 0f), "Camilla de ecografía", 1.3f);
        }
        for (int i = 0; i < 4; i++)   // 4 salas de 5,6 × 5,7 m (32 m², norma LDR) en dos filas
        {
            Vector3 c = new Vector3(5.8f + (i % 2) * 6.1f, 0f, -5.3f - (i / 2) * 6.1f);
            Zone(r, "Sala_paritorio", c, new Vector2(5.6f, 5.7f), zoneP);
            Station(InfirmaryProps.DeliveryBed(r, i == 0 ? "Paritorio" : $"Paritorio {i + 1}", c, 180f), HospitalService.Paritorio, $"Paritorio {i + 1} (32 m², cuna térmica)", 1.9f, false, new Vector3(0f, 0f, 1.2f));
        }
        // ANILLO BAJO: observación de urgencias y maternidad (la madre se recupera en el piso de su paritorio).
        Transform ring0 = RingDeck(r, "Anillo_camas_baja", F);
        RingCarpets(ring0, area.transform, F, hospitalName, "Anillo de urgencias y maternidad");
        int obs = RingBeds(ring0, F, 6, "observación de urgencias");
        int mat = RingBeds(ring0, F, 12, "maternidad");
        Label(ring0, "Anillo de camas: urgencias y maternidad", 2.2f);

        // ══ PLANTA 1 (32 × 23): BLOQUE QUIRÚRGICO, UCI Y AISLAMIENTO ══════════════════════════════════════════════════
        // (Las plantas 1 y 2 son pisos de la torre: ya las dejó puestas BuildAreaStructures, encima de urgencias.)
        Transform p1 = InnerFloor(area.transform, "Planta_hospitalizacion", area.transform);
        Vector2 s1 = FloorSize(p1, F);
        Transform f1 = Solid(p1, new Vector3(0f, 0.05f, 0f), new Vector3(s1.x, 0.1f, s1.y), new Color(0.84f, 0.92f, 0.9f), "Suelo_hospitalizacion");
        Label(f1, "Bloque quirúrgico, UCI y aislamiento (planta 1)", 2.2f);
        Transform w1 = new GameObject("Planta1_equipamiento").transform; w1.SetParent(p1, false); w1.localPosition = new Vector3(0f, 0.1f, 0f);
        // 4 QUIRÓFANOS de 6 × 6 m (oeste) con su lavado; esterilización y oxígeno al lado.
        for (int i = 0; i < 4; i++)
        {
            Vector3 c = new Vector3(-12.2f + (i % 2) * 6.4f, 0f, -7.6f + (i / 2) * 6.4f);
            string sfx = i == 0 ? "" : $" {i + 1}";
            Zone(w1, "Sala_quirofano", c, new Vector2(6f, 6f), zoneQ);
            Station(InfirmaryProps.OperatingTable(w1, "Quirofano" + sfx, c, 0f), HospitalService.Quirofano, $"Quirófano {i + 1}", 1.6f, true, new Vector3(0.9f, 0f, 0.2f));
            InfirmaryProps.SurgicalLight(w1, "Lampara_quirurgica" + sfx, c + new Vector3(-1.4f, 0f, 0f), 90f);
            Station(InfirmaryProps.AnesthesiaMachine(w1, "Anestesia" + sfx, c + new Vector3(0f, 0f, -2.2f), 0f), HospitalService.Anestesia, $"Anestesia {i + 1}", 2f, true, new Vector3(0.9f, 0f, 0f));
            Label(InfirmaryProps.InstrumentTable(w1, "Instrumental" + sfx, c + new Vector3(2f, 0f, 1.6f), 0f), "Instrumental", 1.3f);
        }
        Station(InfirmaryProps.ScrubSink(w1, "Lavado_quirurgico", new Vector3(-12.2f, 0f, 5.4f), 0f), HospitalService.Esterilizacion, "Lavado quirúrgico", 1.9f, false, new Vector3(0f, 0f, -0.9f));
        Station(InfirmaryProps.ScrubSink(w1, "Lavado_quirurgico 2", new Vector3(-5.8f, 0f, 5.4f), 0f), HospitalService.Esterilizacion, "Lavado quirúrgico 2", 1.9f, false, new Vector3(0f, 0f, -0.9f));
        Station(InfirmaryProps.Autoclave(w1, "Autoclave", new Vector3(-14.6f, 0f, 8.6f), 90f), HospitalService.Esterilizacion, "Esterilización (autoclave)", 2f, false, new Vector3(0f, 0f, -1f));
        Station(InfirmaryProps.OxygenPlant(w1, "Planta_oxigeno", new Vector3(-5.8f, 0f, 8.8f), 180f), HospitalService.Oxigeno, "Planta de oxígeno", 2.6f, false, new Vector3(0f, 0f, -1.1f));
        // UCI de 6 boxes de 4,3 × 4,3 m (18,6 m² por cama) al sureste, con su control.
        for (int i = 0; i < 6; i++)
        {
            Vector3 c = new Vector3(5.6f + (i % 3) * 4.4f, 0f, -8.8f + (i / 3) * 4.8f);   // 2 m libres entre camas (4,8 m de eje: la cama con su gotero mide 2,7)
            Zone(w1, "Box_UCI", c, new Vector2(4.2f, 4.2f), zoneC);
            Transform icu = InfirmaryProps.HospitalBed(w1, $"UCI {i + 1}", c, 0f, bl[(i + 2) % 4]);
            InfirmaryProps.Ventilator(icu, "Ventilador", new Vector3(-0.85f, 0f, 0.3f), 90f);
            Label(icu, $"UCI {i + 1}", 1.6f);
        }
        Station(InfirmaryProps.DoctorDesk(w1, "Control_UCI", new Vector3(14f, 0f, -0.4f), 180f), HospitalService.UCI, "Control de la UCI", 1.5f, true);
        // AISLAMIENTO: 4 habitaciones con esclusa en su ZONA (noreste), a ras de la planta (sin escalón ni escalera: Berón
        // 2026-10-28), con una sola entrada por la franja de la esclusa.
        Transform iso = new GameObject("Zona_aislamiento").transform; iso.SetParent(w1, false); iso.localPosition = new Vector3(9.4f, 0f, 6.8f);
        KitchenProps.C(iso, new Vector3(0f, 0.01f, 0f), new Vector3(12f, 0.02f, 7f), new Color(0.88f, 0.93f, 0.95f), 0f, "Suelo_aislamiento");
        KitchenProps.C(iso, new Vector3(0f, 0.02f, -2.6f), new Vector3(12f, 0.02f, 1.8f), new Color(0.95f, 0.92f, 0.75f), 0f, "Esclusa_antesala");   // franja de la esclusa (EPI)
        for (int i = 0; i < 4; i++)
            Station(InfirmaryProps.IsolationRoom(iso, i == 0 ? "Aislamiento" : $"Aislamiento {i + 1}", new Vector3(-4.5f + i * 3f, 0f, 1f), 180f, bl[i % 4]), HospitalService.Aislamiento, $"Aislamiento {i + 1}: presión negativa y esclusa", 2.6f, false, new Vector3(0f, 0f, 2.2f));
        Label(iso, "Aislamiento (entrada solo por la esclusa)", 2.4f);
        // ANILLO ALTO: recuperación postquirúrgica (junto a los quirófanos) y hospitalización general.
        Transform ring1 = RingDeck(w1, "Anillo_camas_planta1", s1);
        RingCarpets(ring1, p1, s1, hospitalName, "Anillo de recuperación y hospitalización");
        int rec = RingBeds(ring1, s1, 10, "recuperación postquirúrgica");
        int gen = RingBeds(ring1, s1, 20, "hospitalización");
        Label(ring1, "Anillo de camas: recuperación y hospitalización", 2.2f);
        int humanBeds = bed - 1;
        // ══ PLANTA 2: HOSPITAL VETERINARIO (docs/vet-hospital-plan.md) ═══════════════════════════════════════════════
        // 4 alas con 4 puestos iguales por tarea (4 veterinarios a la vez ante una avalancha): clínica (4 mesas de
        // tratamiento, 4 quirófanos, rayos X + eco, laboratorio, farmacia, báscula), perros/lobos/zorros (perreras en
        // batería y jaulas), servicio (cocina de dietas con congelador, sala fría de pingüinos) y, en su ANILLO, los grandes
        // animales: boxes de osos de 4 × 3 m en batería con su jaula de contención y corrales de foca con piscina salada.
        Transform up = InnerFloor(area.transform, "Planta_veterinaria", p1);
        Vector2 us = FloorSize(up, s1);
        float uz = us.y * 0.5f;   // (el anillo, al borde de la planta)
        Transform uf = Solid(up, new Vector3(0f, 0.05f, 0f), new Vector3(us.x, 0.1f, us.y), new Color(0.82f, 0.9f, 0.86f), "Suelo_veterinaria");
        Label(uf, "Hospital veterinario (planta 2)", 2.2f);
        Transform v = new GameObject("Equipamiento_veterinario").transform; v.SetParent(up, false); v.localPosition = new Vector3(0f, 0.1f, 0f);
        Color vz = new Color(0.86f, 0.9f, 0.8f);
        int place = 1;
        // ALA PERROS / LOBOS / ZORROS (oeste): 16 perreras en dos filas enfrentadas (batería, pasillo de 2 m) y 8 jaulas.
        for (int i = 0; i < 8; i++, place++) Label(InfirmaryProps.Kennel(v, $"Box {i + 1}", new Vector3(-14.6f + i * 1.45f, 0f, -layZ + 2.4f), 0f), $"Plaza {place}: perrera", 1.6f);
        for (int i = 0; i < 8; i++, place++) Label(InfirmaryProps.Kennel(v, $"Box {i + 9}", new Vector3(-14.6f + i * 1.45f, 0f, -layZ + 6.2f), 180f), $"Plaza {place}: perrera", 1.6f);
        for (int i = 0; i < 2; i++, place += 4) Label(InfirmaryProps.Cages(v, $"Jaulas {i + 1}", new Vector3(-13.6f + i * 3.4f, 0f, 1.8f), 0f), $"Plazas {place}-{place + 3}: jaulas (zorro, aves)", 1.6f);
        Zone(v, "Pediluvio", new Vector3(-4.2f, 0f, -layZ + 4.3f), new Vector2(1.2f, 1.6f), new Color(0.55f, 0.75f, 0.85f));
        // ALA CLÍNICA (centro): 4 mesas de tratamiento en fila (una con la báscula), y 4 quirófanos de 5 × 5 m.
        for (int i = 0; i < 4; i++)
            Station(InfirmaryProps.VetTable(v, i == 0 ? "Mesa veterinaria" : $"Mesa veterinaria {i + 1}", new Vector3(-13.4f + i * 3.6f, 0f, 6.6f), 90f), HospitalService.Veterinaria, $"Tratamiento veterinario {i + 1}", 1.4f, true, new Vector3(0f, 0f, -1.1f));
        Label(InfirmaryProps.FloorScale(v, "Bascula", new Vector3(-13.4f, 0f, 9.6f), 0f), "Báscula (dosis por kilo)", 1.5f);
        for (int i = 0; i < 4; i++)
        {
            Vector3 c = new Vector3(0.6f + (i % 2) * 6f, 0f, -6.6f + (i / 2) * 5.4f);   // 6 m de eje en X: 2 m entre la anestesia de un quirófano y la lámpara del siguiente
            string sfx = i == 0 ? "" : $" {i + 1}";
            Zone(v, "Sala_quirofano_vet", c, new Vector2(5f, 5f), zoneQ);
            Station(InfirmaryProps.VetTable(v, "Quirofano veterinario" + sfx, c, 90f), HospitalService.CirugiaVeterinaria, $"Quirófano veterinario {i + 1}", 1.4f, true, new Vector3(0f, 0f, -1.1f));
            InfirmaryProps.SurgicalLight(v, "Lampara_quirurgica_vet" + sfx, c + new Vector3(-1.4f, 0f, 0f), 90f);
            Station(InfirmaryProps.AnesthesiaMachine(v, "Anestesia veterinaria" + sfx, c + new Vector3(1.6f, 0f, 0f), -90f), HospitalService.Anestesia, $"Anestesia veterinaria {i + 1}", 2f, false, new Vector3(0f, 0f, -0.9f));
        }
        // ALA DE DIAGNÓSTICO Y SERVICIO (este): rayos X blindado (4 × 4 m), eco, laboratorio, farmacia, cocina de dietas
        // (con congelador a −18 °C para el pescado) y la sala fría de los pingüinos (0–10 °C).
        Zone(v, "Sala_rayos_X_vet", new Vector3(12.6f, 0f, 6.4f), new Vector2(4.4f, 4.4f), zoneW);
        Station(InfirmaryProps.XRay(v, "Rayos X veterinario", new Vector3(12.6f, 0f, 6.4f), 0f), HospitalService.Imagen, "Rayos X veterinario (sala blindada)", 2.6f, false, new Vector3(0.9f, 0f, 1f));
        Station(InfirmaryProps.Ultrasound(v, "Ecografo veterinario", new Vector3(7.6f, 0f, 7f), 0f), HospitalService.Imagen, "Ecógrafo veterinario", 1.6f);
        Label(InfirmaryProps.LabBench(v, "Laboratorio veterinario", new Vector3(13.9f, 0f, 1.4f), -90f), "Laboratorio veterinario", 1.4f);
        Label(InfirmaryProps.MedicineCabinet(v, "Botiquin veterinario", new Vector3(15.4f, 0f, -2.6f), -90f), "Farmacia veterinaria", 2.2f);
        Transform diet = KitchenProps.Root(v, "Cocina_de_dietas", new Vector3(12.4f, 0f, -6.6f));
        KitchenProps.C(diet, new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.9f, 0.7f), KitchenProps.Steel, 0f, "Mesa_dietas");
        KitchenProps.C(diet, new Vector3(2.2f, 0.95f, 0f), new Vector3(1f, 1.9f, 0.8f), new Color(0.85f, 0.9f, 0.95f), 0f, "Congelador_pescado");
        KitchenProps.Fit(diet, solid: true);
        Label(diet, "Cocina de dietas (pescado, carne, fruta) y congelador −18 °C", 2.2f);
        Zone(v, "Sala_fria_pinguinos", new Vector3(8.3f, 0f, 2.4f), new Vector2(6.6f, 4.4f), new Color(0.8f, 0.9f, 1f));
        for (int i = 0; i < 3; i++, place += 4) Label(InfirmaryProps.Cages(v, $"Jaulas pinguinos {i + 1}", new Vector3(6.2f + i * 2.2f, 0f, 2.4f), 0f), $"Plazas {place}-{place + 3}: sala fría de pingüinos (0–10 °C)", 1.6f);
        // ANILLO DE GRANDES ANIMALES: 6 boxes de osos de 4 × 3 m en batería (puertas de guillotina entre ellos) + jaula de
        // contención, y 2 corrales de foca con piscina de agua salada; pasillo del cuidador de 4 m a cada lado.
        Transform ringV = RingDeck(v, "Anillo_grandes_animales", us);
        RingCarpets(ringV, up, us, hospitalName, "Anillo de grandes animales");
        int stall = 0;
        float lastS = 0f;   // 6 boxes de oso y 4 de ungulados (reno, ciervo) o lobos: 48 plazas, como camas de personas
        for (float s = 2f; stall < 10; s += 4f)
        {
            var (sp0, yaw, corner) = RingPoint(us, s);
            if (corner || (Mathf.Abs(sp0.x) < 4.6f && Mathf.Abs(sp0.z) > uz - 2f)) continue;
            Label(InfirmaryProps.LargeAnimalStall(ringV, $"Cuadra {stall + 1}", sp0, yaw), stall < 6 ? $"Plaza {place++}: box de oso 4 × 3 m (guillotinas)" : $"Plaza {place++}: box de reno, ciervo o lobo", 1.8f);
            lastS = s;
            stall++;
        }
        // La jaula, en el siguiente hueco libre (fuera de las esquinas y del área de alfombras, como los boxes).
        float js = lastS + 6f;   // con 2 m libres al último box
        for (int g = 0; g < 40; g++, js += 1f) { var (q, _, qc) = RingPoint(us, js); if (!qc && !(Mathf.Abs(q.x) < 4.6f && Mathf.Abs(q.z) > uz - 2f)) break; }
        var (jp, jyaw, _) = RingPoint(us, js);
        Transform squeeze = KitchenProps.Root(ringV, "Jaula_contencion", jp, jyaw);
        for (int k = -1; k <= 1; k += 2) KitchenProps.C(squeeze, new Vector3(k * 0.9f, 1.1f, 0f), new Vector3(0.08f, 2.2f, 2.4f), KitchenProps.Steel, 0f, "Barrotes");
        KitchenProps.C(squeeze, new Vector3(0f, 2.2f, 0f), new Vector3(1.9f, 0.08f, 2.4f), KitchenProps.Steel, 0f, "Techo_barrotes");
        KitchenProps.Fit(squeeze, solid: false);
        Label(squeeze, "Jaula de contención (inyectar sin anestesiar)", 2.4f);
        float half = RingLength(us) * 0.5f;
        float penS = half + 4f;
        for (int i = 0; i < 2; i++, penS += 7f)
        {
            for (int g = 0; g < 80 && RingPoint(us, penS).corner; g++) penS += 1f;   // (en un edificio-árbol el lado del tronco no tiene anillo)
            var (sp, syaw, _) = RingPoint(us, penS);
            Transform pen = KitchenProps.Root(ringV, $"Corral_foca {i + 1}", sp, syaw);
            KitchenProps.C(pen, new Vector3(0f, 0.05f, -1.4f), new Vector3(4f, 0.1f, 2.2f), new Color(0.75f, 0.75f, 0.72f), 0f, "Zona_seca");
            KitchenProps.C(pen, new Vector3(0f, 0.4f, 1.3f), new Vector3(4f, 0.8f, 3f), new Color(0.3f, 0.55f, 0.8f, 0.6f), 0f, "Piscina_salada");
            KitchenProps.Fit(pen, solid: false);
            Label(pen, $"Plaza {place++}: corral de foca con piscina salada (≥ 0,76 m)", 2f);
        }
        Label(ringV, "Anillo de grandes animales: osos y focas", 2.4f);
        Debug.Log($"[Hospital] 3 plantas: {humanBeds} camas para personas en dos anillos ({obs} de urgencias y {mat} de maternidad abajo; {rec} de recuperación y {gen} de hospitalización en la planta 1), 4 puestos por servicio, 4 aislamientos con esclusa y {place - 1} plazas para animales; {Object.FindObjectsByType<HospitalStation>(FindObjectsSortMode.None).Length} puestos interactivos.");

        // Linduarda en su mesa; Lizmibet pasando visita en el anillo de la planta 1; el paciente de prueba (lobo) arriba, en una perrera.
        GameObject l = GameObject.Find("Linduarda");
        if (l != null) l.transform.position = area.transform.TransformPoint(new Vector3(-9.8f, 1f, 4.4f));
        GameObject lz = GameObject.Find("Lizmibet");
        if (lz != null) lz.transform.position = ring1.TransformPoint(RingPoint(s1, 40f).p + new Vector3(0f, 1f, 2.5f));   // pasando visita en el anillo
        GameObject p = GameObject.Find("Lobo_paciente");
        if (p != null) p.transform.position = v.TransformPoint(new Vector3(-14.6f, 0.15f, -layZ + 2.4f));   // en la perrera 1
        // Kushal (el jugador) EMPIEZA AQUÍ, junto a las camas de observación: es el recién llegado.
        GameObject kushal = GameObject.Find("Player");
        if (kushal != null) kushal.transform.position = area.transform.TransformPoint(new Vector3(-11.8f, 1.3f, 3.6f));
        // La doctora inicia a los recién llegados en el TANQUE del hospital (no en cualquier máquina).
        VirtualizationMachine vm = area.GetComponentInChildren<VirtualizationMachine>();
        if (l != null && vm != null && l.TryGetComponent(out Linduarda lin)) lin.virtualizationMachine = vm;
    }
}
