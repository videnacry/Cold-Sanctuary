using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// VIDA DEL SANTUARIO: construcciones elevadas con colchón de musgo (las caídas no matan), dormitorios para cada persona,
/// energía solar (nada de gas: cocinas de inducción), insectos de hielo a tamaño modificado y la energía que se recupera
/// del propio cuerpo (metabolismo).
/// </summary>
public class SanctuaryLifeTest : MonoBehaviour, ITestUnit
{
    public int Group => 28;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Vida del santuario (elevado, musgo, dormitorios, solar, insectos de hielo, energía)");
        yield return null;

        // Caídas: 7 m sobre hielo hacen daño; sobre el colchón de musgo, casi nada.
        GameObject probe = new GameObject("Caida_TEST");
        probe.AddComponent<CharacterController>();
        FallImpact fi = probe.AddComponent<FallImpact>();
        float hard = fi.Land(7f, 0f), soft = fi.Land(7f, 0.85f);
        TestProbe.Greater("una caída de 7 m sobre hielo hace daño", hard, 30f, $"{hard:0}");
        TestProbe.Check("sobre el colchón de musgo el golpe se amortigua", soft < hard * 0.2f, $"{soft:0} vs {hard:0}");
        TestProbe.Check("saltar un escalón no hace daño", fi.Land(1f, 0f) == 0f);
        Destroy(probe);
        TestProbe.Greater("hay colchones de musgo bajo lo construido", FindObjectsByType<SoftGround>(FindObjectsSortMode.None).Length, 6f);

        // Áreas elevadas.
        GameObject kitchen = GameObject.Find("Kitchen_Area");
        TestProbe.Check("la cocina está elevada (12 m)", kitchen != null && kitchen.transform.position.y >= 11f, kitchen != null ? $"{kitchen.transform.position.y:0.0} m" : "sin cocina");
        GameObject garden = GameObject.Find("Garden_Area");
        TestProbe.Check("el huerto también está elevado (los cultivos no dan sombra al bosque)", garden != null && garden.transform.position.y >= 10f, garden != null ? $"{garden.transform.position.y:0.0} m" : "sin huerto");

        // Dormitorios.
        TestProbe.Greater("hay un dormitorio por persona (12)", Bedroom.All.Count, 11f);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Bedroom mine = player != null ? Bedroom.Of(player.GetComponent<Anima>()) : null;
        TestProbe.Check("Kushal tiene su habitación", mine != null && mine.bed != null, mine != null ? mine.name : "ninguna");

        // Solar e inducción.
        TestProbe.Greater("hay paneles solares (uno por edificio + campo)", FindObjectsByType<SolarArray>(FindObjectsSortMode.None).Length, 6f);
        bool flame = false;
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name == "Llama") { flame = true; break; }
        TestProbe.Check("las cocinas son de inducción (sin llama de gas)", !flame);

        // Insectos de hielo a tamaño modificado.
        int bees = 0; float maxBee = 0f; Vector3 firstScale = Vector3.zero;
        foreach (IceInsectLife b in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None))
        {
            if (b.kind != IceInsectLife.Kind.Bee) continue;
            bees++; if (bees == 1) firstScale = b.transform.lossyScale;
            Bounds bb = new Bounds(b.transform.position, Vector3.zero);
            foreach (Renderer r in b.GetComponentsInChildren<Renderer>()) if (r.enabled) bb.Encapsulate(r.bounds);
            maxBee = Mathf.Max(maxBee, Mathf.Max(bb.size.x, bb.size.z));
        }
        TestProbe.Greater("las abejas de las nieves viven (vuelan) en el bosque", bees, 0f);
        TestProbe.Check("la abeja de las nieves mide ~20 cm", bees == 0 || (maxBee > 0.1f && maxBee < 0.45f), $"{maxBee:0.00} m (escala {firstScale})");

        // Energía = metabolismo: sin grasa, se recupera gastando masa del cuerpo.
        GameObject g = new GameObject("Metabolismo_TEST");
        Anima an = g.AddComponent<Anima>(); an.fatReserves = 0f; an.bodyMass = 1f;
        CharacterLevel cl = g.AddComponent<CharacterLevel>();
        ATPRegenSpell regen = g.AddComponent<ATPRegenSpell>();
        yield return null;
        cl.currentEnergy = 0f;
        float t0 = Time.time;
        while (Time.time - t0 < 1f) yield return null;
        TestProbe.Greater("sin grasa, la energía se recupera…", cl.currentEnergy, 0.1f);
        TestProbe.Check("…pero gastando masa del propio cuerpo", an.bodyMass < 1f, $"masa {an.bodyMass:0.0000}");
        Destroy(g);

        // Tanque de virtualización: pecera con escotilla horizontal; al abrirla se enciende el campo que sujeta el agua.
        VirtualizationTank tank = FindAnyObjectByType<VirtualizationTank>();
        TestProbe.Check("hay tanque de virtualización (pecera)", tank != null);
        if (tank != null)
        {
            yield return tank.Hatch(true);
            TestProbe.Check("la escotilla horizontal se abre con el campo magnético encendido", tank.HatchOpen && tank.FieldOn);
            yield return tank.Hatch(false);
            TestProbe.Check("…y se cierra (campo apagado)", !tank.HatchOpen && !tank.FieldOn);
        }

        // Carne cultivada: el laboratorio pasa por todas las fases y deja raciones de carne en el comedero de los depredadores.
        CulturedMeatLab lab = CulturedMeatLab.All.Count > 0 ? CulturedMeatLab.All[0] : null;
        TestProbe.Check("hay laboratorio de carne cultivada", lab != null);
        if (lab != null)
        {
            float oldSpeed = lab.speed; lab.speed = 400f; bool oldStaff = lab.requiresStaff; lab.requiresStaff = false;   // (en el juego, iniciar y cosechar son misiones)
            bool sawProliferation = false;
            float t1 = Time.time;
            while (lab.portionsMade == 0 && Time.time - t1 < 8f) { sawProliferation |= lab.stage == CulturedMeatLab.Stage.Proliferacion; yield return null; }
            lab.speed = oldSpeed; lab.requiresStaff = oldStaff;
            TestProbe.Check("el lote pasa por la proliferación en biorreactor", sawProliferation);
            TestProbe.Greater("…y cosecha raciones de carne para los depredadores", lab.portionsMade, 0f, $"{lab.portionsMade} raciones");
        }

        // Colmena: la reina pone huevos en las celdillas y las obreras tienen oficios distintos según su edad.
        Hive hv = null;
        // Una colmena con abejas y SIN araña al lado (con una araña acechando, la reina huye en vez de poner: es otra prueba).
        var spiders = FindObjectsByType<SnowSpiderBehavior>(FindObjectsSortMode.None);
        foreach (Hive h in Hive.All)
        {
            if (spiders.Any(s => s != null && Vector3.Distance(s.transform.position, h.combCenter) < 5f)) continue;
            foreach (IceInsectLife b in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None)) if (b.hive == h) { hv = h; break; }
            if (hv != null) break;
        }
        if (hv == null) foreach (Hive h in Hive.All) { foreach (IceInsectLife b in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None)) if (b.hive == h) { hv = h; break; } if (hv != null) break; }   // todas tienen araña cerca: la primera
        TestProbe.Check("hay una colmena con abejas", hv != null);
        if (hv != null)
        {
            var roles = new System.Collections.Generic.HashSet<IceInsectLife.Role>();
            bool queen = false;
            foreach (IceInsectLife b in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None)) if (b.hive == hv) { roles.Add(b.role); queen |= b.isQueen; }
            TestProbe.Check("la colonia tiene reina", queen);
            float old = hv.secondsPerDay; hv.secondsPerDay = 1f;
            float t2 = Time.time;
            while (hv.Count(Hive.Cell.Egg) + hv.Count(Hive.Cell.Larva) == 0 && Time.time - t2 < 40f) yield return null;
            TestProbe.Greater("la reina pone huevos en las celdillas del panal", hv.Count(Hive.Cell.Egg) + hv.Count(Hive.Cell.Larva), 0f,
                              $"huevos {hv.Count(Hive.Cell.Egg)} larvas {hv.Count(Hive.Cell.Larva)} miel {hv.Count(Hive.Cell.Honey)} polen {hv.Count(Hive.Cell.Pollen)} vacías {hv.Count(Hive.Cell.Empty)} · {QueenInfo(hv)}");
            hv.secondsPerDay = old;
        }

        // Restaurante gourmet, estudio textil y telarañas.
        GameObject dining = GameObject.Find("Comedor_Gourmet");
        int tables = 0; if (dining != null) foreach (Transform tt in dining.GetComponentsInChildren<Transform>()) if (tt.name.StartsWith("Mesa_")) tables++;
        TestProbe.Greater("el restaurante tiene comedor con mesas vestidas", tables, 7f, $"{tables} mesas");
        int upTables = 0; if (dining != null) foreach (Transform tt in dining.GetComponentsInChildren<Transform>()) if (tt.name.StartsWith("Mesa_") && tt.position.y > dining.transform.position.y + 1.5f) upTables++;
        bool diningCarpets = dining != null && System.Array.Exists(dining.GetComponentsInChildren<CarpetPad>(), cp => cp.surface.Contains("#anillo"));
        TestProbe.Check("el comedor va en un anillo de 10 m sobre la cocina, con caja y sus alfombras (sin escalera)", upTables >= 12 && diningCarpets && GameObject.Find("Escalera_comedor_norte") == null && GameObject.Find("Caja_registradora") != null, $"{upTables} mesas arriba, alfombras del anillo {diningCarpets}");
        int pierced = 0; GameObject flora = GameObject.Find("FloraHielo_AUTO");
        var piercedWho = new System.Collections.Generic.List<string>();
        if (flora != null)
        {
            var plats = new System.Collections.Generic.List<Bounds>();
            foreach (SanctuaryArea sa in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
            {
                // Solo lo construido: las crías, pacientes y personajes que viven dentro de un área se mueven en Play y
                // estirarían su caja (una cría que se asoma al borde no es la plataforma).
                bool any = false; Bounds bb = default;
                foreach (Renderer rr in sa.GetComponentsInChildren<Renderer>())
                {
                    if (rr.GetComponentInParent<Anima>() != null || rr.GetComponentInParent<FlyingCarpet>() != null) continue;   // (las alfombras vuelan a otros edificios)
                    if (!any) { bb = rr.bounds; any = true; } else bb.Encapsulate(rr.bounds);
                }
                if (any) plats.Add(bb);
            }
            foreach (Transform tt in flora.GetComponentsInChildren<Transform>())
                if (tt.name == "Rama" && plats.Exists(pb => pb.Contains(tt.position)))
                {
                    pierced++;
                    if (piercedWho.Count < 4 && tt.parent != null && !piercedWho.Contains(tt.parent.name)) piercedWho.Add(tt.parent.name);
                }
        }
        TestProbe.Check("ninguna rama de árbol atraviesa una plataforma de área", pierced == 0, $"{pierced} ramas {string.Join(", ", piercedWho)}");
        // La Magnate mide el ÁREA entera: quien está en la entreplanta de las camas (3 m) o en la planta de arriba sigue en
        // su puesto; un paso fuera de la planta, no (Berón 2026-10-23: lo devolvía estando en la plataforma de las camas).
        GameObject hosp = GameObject.Find("Infirmary_Area"), bed = GameObject.Find("Cama 4");
        SanctuaryArea ha = hosp != null ? hosp.GetComponent<SanctuaryArea>() : null;
        Transform hmat = hosp != null ? hosp.transform.Find("Mat") : null;
        if (ha != null && bed != null && hmat != null)
        {
            Vector3 outside = hosp.transform.TransformPoint(new Vector3(hmat.localScale.x * 0.5f + 4f, 1f, 0f));
            TestProbe.Check("la Magnate cuenta la entreplanta de las camas como parte del hospital (y fuera, no)",
                ha.Contains(bed.transform.position + Vector3.up * 1.1f) && !ha.Contains(outside), $"cama en {bed.transform.position}");
        }
        // El EDIFICIO entero cuenta (Berón 2026-10-23): quien trabaja en el gimnasio puede subir a la calistenia (2 pisos más arriba); el suelo
        // de debajo del hospital, no.
        SanctuaryArea gymA = GameObject.Find("Gym_Area")?.GetComponent<SanctuaryArea>();
        SanctuaryArea dojoA = GameObject.Find("Calisthenics_Area")?.GetComponent<SanctuaryArea>();
        if (gymA != null && dojoA != null && ha != null)
            TestProbe.Check("el puesto es el edificio entero (gimnasio → calistenia de arriba sí; bajo el hospital, no)",
                gymA.InBuilding(dojoA.transform.position + Vector3.up * 1.1f) && !ha.InBuilding(new Vector3(hosp.transform.position.x, 1f, hosp.transform.position.z)),
                $"calistenia en {dojoA.transform.position}");
        // El espacio de la tarea: quien repara en la Cocina con una misión de Construcción sigue en su puesto; con el
        // inventario del almacén (solo Construcción), en la Cocina no.
        GameObject kitchenGo = GameObject.Find("Kitchen_Area");
        if (kitchenGo != null)
        {
            var dayMissions = AreaWork.Today(0, ConstructionProject.All);
            var fixKitchen = dayMissions.FirstOrDefault(m => m.id == "rep_Cocina") ?? AreaWork.Today(1, ConstructionProject.All).FirstOrDefault(m => m.id == "rep_Cocina");
            var store = dayMissions.FirstOrDefault(m => m.id == "con_almacen");
            Vector3 inKitchen = kitchenGo.transform.position + Vector3.up * 1.1f;
            if (fixKitchen != null && store != null)
                TestProbe.Check("la Magnate cuenta el espacio de la tarea (reparar en la Cocina sí, el almacén no)",
                    WorkdayDirector.InWorkspace(fixKitchen, "Construcción", inKitchen) && !WorkdayDirector.InWorkspace(store, "Construcción", inKitchen));
        }
        {
            GameObject who = GameObject.FindGameObjectWithTag("Player");
            object key = new object();
            MagnateWarden.Lock(key, true); bool locked = MagnateWarden.Blocked(who);
            MagnateWarden.Lock(key, false); bool free = !MagnateWarden.Blocked(who);
            TestProbe.Check("el alcance de la Magnate se puede bloquear (y se libera al quitar el bloqueo)", locked && free);
        }
        TestProbe.Check("el estudio textil tiene telar, rueca y tintes", GameObject.Find("Telar_de_pie") != null && GameObject.Find("Rueca") != null && GameObject.Find("Tina_Añil") != null);
        TestProbe.Greater("las arañas de las nieves tejen su tela junto a los panales", SpiderWeb.All.Count, 0f, $"{SpiderWeb.All.Count} telas");
        TestProbe.Check("la colmena está en una cavidad con piquera (entrada)", FindByPrefix("Piquera"));

        // Servicios para turistas y personal: aseos, médica, guardia.
        int wcs = 0; foreach (Transform tt in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (tt.name.StartsWith("Aseos")) wcs++;
        TestProbe.Greater("hay aseos en las áreas humanas", wcs, 4f, $"{wcs}");
        TestProbe.Check("Lizmibet (médica, amiga de Irosene) está en el santuario", FindByPrefix("Lizmibet"));
        Clock clock = FindAnyObjectByType<Clock>();
        float dayMin = TimeController.timeController.RealMinutesPerDay;
        TestProbe.Check("un día de juego dura lo configurado en el reloj (1 h por defecto)", clock != null && Mathf.Abs(dayMin - clock.realMinutesPerDay) < 0.5f, $"{dayMin:0.#} min");
        TestProbe.Check("hay área de Construcción con maquinaria y material", FindByPrefix("Construction_Area") && FindByPrefix("Miniexcavadora") && FindByPrefix("Hormigonera"));
        TestProbe.Check("el jefe de construcción, Sadhguru y Andrea están en el santuario", FindByPrefix("JefeConstruccion") && FindByPrefix("Sadhguru") && FindByPrefix("Andrea"));
        CarnivoreKeeper keeper = FindAnyObjectByType<CarnivoreKeeper>();
        TestProbe.Check("un guardia patrulla y alimenta a los carnívoros", keeper != null, keeper != null ? keeper.name : "ninguno");

        // Cocina profesional: recetario coherente, equipo instalado, cultivos plantados y obrador que encadena elaboraciones.
        var missingEq = new System.Collections.Generic.List<string>();
        foreach (string eq in Gastronomy.Equipment()) if (!FindByPrefix("Equipo_" + eq)) missingEq.Add(eq);
        TestProbe.Check("la cocina tiene todo el equipo que pide la carta", missingEq.Count == 0, missingEq.Count == 0 ? $"{Gastronomy.Equipment().Count} equipos" : "falta: " + string.Join(", ", missingEq));
        var unknown = new System.Collections.Generic.List<string>();
        foreach (Gastronomy.Dish d in Gastronomy.Menu) foreach (string c in d.components) if (!Gastronomy.Raw.ContainsKey(c) && Gastronomy.PrepOf(c) == null) unknown.Add(c);
        foreach (Gastronomy.Prep p in Gastronomy.Preps) foreach (string c in p.inputs) if (!Gastronomy.Raw.ContainsKey(c) && Gastronomy.PrepOf(c) == null) unknown.Add(c);
        TestProbe.Check("cada ingrediente sale de un cultivo, del laboratorio o de una elaboración", unknown.Count == 0, string.Join(", ", unknown));
        var planted = new System.Collections.Generic.HashSet<string>();
        foreach (GardenCrop c in GardenCrop.All) if (c != null) planted.Add(c.kind.ToString());
        var notPlanted = new System.Collections.Generic.List<string>();
        foreach (var kv in Gastronomy.Raw) if (kv.Value.src == Gastronomy.Source.Cultivo && !planted.Contains(kv.Value.from)) notPlanted.Add(kv.Key);
        TestProbe.Check("los huertos tienen los cultivos de la carta", notPlanted.Count == 0, notPlanted.Count == 0 ? $"{planted.Count} especies" : "sin plantar: " + string.Join(", ", notPlanted));
        KitchenLarder larder = KitchenLarder.Instance;
        TestProbe.Check("hay despensa y obrador en la cocina", larder != null);
        if (larder != null)
        {
            bool oldAuto = larder.autoPlan; larder.autoPlan = true;   // la prueba deja que el obrador se organice solo
            foreach (string line in CulturedMeatLab.Lines) larder.Add(CulturedMeatLab.MeatItem(line), 10f);
            for (int h = 0; h < 80; h++) larder.Tick(1f);   // 80 horas de juego
            larder.autoPlan = oldAuto;
            int ready = 0; foreach (Gastronomy.Dish d in Gastronomy.Menu) if (larder.CanServe(d)) ready++;
            TestProbe.Greater("el obrador elabora sus bases (yogur, quesos, masas, tofu…)", larder.prepsDone, 15f, $"{larder.prepsDone} elaboraciones; yogur {larder.Have("yogur"):0}");
            TestProbe.Greater("tras 80 h de mise en place casi toda la carta está lista", ready, Gastronomy.Menu.Length * 0.75f, larder.Report());
            TestProbe.Check("la lasaña sale (pasta fresca, ragú de carne cultivada, bechamel, parmesano)", larder.Serve("Lasaña"), string.Join(", ", larder.Missing(Gastronomy.DishOf("Lasaña"))));
        }

        // JORNADA Y ROTACIÓN (docs/jornada-y-rotacion.md): lógica pura del reparto.
        var ro = new WorkRoster();
        ro.areas.AddRange(new[] { "A", "B", "C" });
        WorkRoster.Worker w1 = ro.Add("W1"), w2 = ro.Add("W2");
        System.Func<System.Collections.Generic.List<WorkRoster.Mission>> todays = () => new System.Collections.Generic.List<WorkRoster.Mission>
        {
            new WorkRoster.Mission("A", "a1", "a1", 2f, true), new WorkRoster.Mission("A", "a2", "a2", 1f),
            new WorkRoster.Mission("B", "b1", "b1", 2f, true), new WorkRoster.Mission("B", "b2", "b2", 2f),
            new WorkRoster.Mission("C", "c1", "c1", 1f, true), new WorkRoster.Mission("C", "c2", "c2", 1f),
        };
        var d0 = ro.Plan(0, todays());
        var d1 = ro.Plan(1, todays());
        TestProbe.Check("el orden de áreas rota cada día (la última pasa a ser la primera)", d1.areaOrder[0] == "C" && d1.areaOrder[1] == "A", string.Join(",", d1.areaOrder));
        string firstD0 = d0.Of(w1).First().mission.area, firstD1 = d1.Of(w1).First().mission.area;
        TestProbe.Check("la primera persona empieza cada día por un área distinta", firstD0 != firstD1, $"{firstD0} → {firstD1}");
        TestProbe.Check("nadie pasa de sus 5 h de trabajo", d0.work.GroupBy(a => a.worker).All(g => g.Sum(a => a.mission.hours) <= WorkRoster.WorkHours + 1e-3f));
        var cMission = d1.Of(w1).FirstOrDefault(a => a.mission.area == "C");
        TestProbe.Check("se da primero la misión que esa persona nunca ha hecho", cMission == null || w1.Count(cMission.mission.id) == 1, cMission != null ? cMission.mission.id : "—");
        var solo = new WorkRoster(); solo.areas.Add("X"); solo.Add("Solo");
        var heavy = new System.Collections.Generic.List<WorkRoster.Mission>();
        for (int i = 0; i < 6; i++) heavy.Add(new WorkRoster.Mission("X", "m" + i, "m" + i, 2f, true));
        heavy.Add(new WorkRoster.Mission("X", "extra", "extra", 2f));
        var hp = solo.Plan(0, heavy);
        TestProbe.Check("si faltan personas, la Magnate se divide y cubre todas las obligatorias", hp.clones.Count > 0 && hp.backlog.All(m => !m.mandatory), $"{hp.clones.Count} clones, {hp.backlog.Count} pendientes");
        TestProbe.Check("las que no se cubren pasan a mañana y son las primeras", hp.backlog.Count == 1 && solo.Plan(1, new System.Collections.Generic.List<WorkRoster.Mission>()).work.Any(a => a.mission.id == "extra"));
        TestProbe.Check("el informe dice si faltan personas", WorkRoster.Balance(hp, 1).Any(s => s.StartsWith("Faltan personas")));

        // En la escena: la jornada reparte al personal real, con entrenamiento y obras.
        WorkdayDirector wd = FindAnyObjectByType<WorkdayDirector>();
        TestProbe.Check("hay director de la jornada", wd != null);
        if (wd != null)
        {
            var plan = wd.PlanDay(100);
            TestProbe.Greater("la jornada reparte misiones al personal", plan.work.Count, 5f, $"{plan.work.Count} misiones, {wd.roster.workers.Count} personas, {plan.clones.Count} clones");
            TestProbe.Check("todas las obligatorias quedan cubiertas", plan.backlog.All(m => !m.mandatory));
            TestProbe.Check("cada persona tiene entrenamiento (yoga o gimnasio)", wd.roster.workers.All(w => plan.training.ContainsKey(w)), string.Join(", ", wd.OpenTraining()));
            TestProbe.Check("hay misiones de obra para las áreas en construcción", plan.work.Any(a => a.mission.id.StartsWith("obra_trabajo_")));
            // CADA OBRA TIENE SU EQUIPO (Berón 2026-10-28): la jornada pone a trabajar a tantas personas distintas como pide.
            foreach (ConstructionProject p in ConstructionProject.All.Where(p => !p.Finished))
            {
                int crew = plan.work.Where(a => a.mission.id.StartsWith("obra_trabajo_" + p.title)).Select(a => a.worker.name).Distinct().Count();
                TestProbe.Check($"la obra «{p.title}» tiene su equipo cada día ({p.crewNeeded} personas)", crew >= p.crewNeeded, $"{crew}/{p.crewNeeded}");
            }
            ConstructionProject site = ConstructionProject.All.FirstOrDefault(p => !p.Finished);
            if (site != null)
            {
                int before = site.daysDone;
                WorkdayDirector.Apply(new WorkRoster.Mission("Construcción", "obra_material_" + site.title, "", 1f), "test");
                for (int k = 0; k < site.crewNeeded - 1; k++) WorkdayDirector.Apply(new WorkRoster.Mission("Construcción", $"obra_trabajo_{site.title}#{k + 2}", "", 2f), $"test{k}");
                bool notYet = site.crewNeeded == 1 || site.daysDone == before;   // falta una persona: aún no cuenta el día
                WorkdayDirector.Apply(new WorkRoster.Mission("Construcción", "obra_trabajo_" + site.title, "", 2f), "test");
                TestProbe.Check("llevar el material y trabajar la obra con todo el equipo la avanza un día (con uno menos, no)", notYet && site.daysDone == before + 1, $"{site.title}: {site.daysDone}/{site.daysRequired}, equipo de {site.crewNeeded}");
            }
            TestProbe.Check("la máquina ofrece misiones de virtualización del área", AreaWork.OfArea("Cocina", ConstructionProject.All, 0).Count > 0);
        }
        TestProbe.Check("la salida de la enfermería está vigilada (iniciación bloqueante)", FindAnyObjectByType<InfirmaryGate>() != null);
        TestProbe.Check("ballet y dojo están en obras; el gimnasio y la calistenia, abiertos", ConstructionProject.All.Count >= 2 && FindByPrefix("Gym_Area") && FindByPrefix("Calistenia"));
        // LÍMITE DE LA ESTRUCTURA: ningún edificio pasa de los pisos que aguantan sus columnas (StructuralLimits).
        int maxFloor = CarpetPad.All.Count > 0 ? CarpetPad.All.Max(c => c.floor) : 0;
        TestProbe.Check($"ningún edificio pasa de {StructuralLimits.MaxFloors} pisos (lo que aguantan las columnas)", maxFloor > 0 && maxFloor <= StructuralLimits.MaxFloors, $"el más alto: {maxFloor} pisos");
        // MENOS EDIFICIOS (docs/plano-edificios.md): Granja, Talleres y Entrenamiento juntan sus áreas en pisos.
        TestProbe.Check("Granja, Talleres y Entrenamiento: varias áreas por edificio", new[] { "Granja", "Talleres", "Entrenamiento" }.All(b => CarpetPad.All.Any(c => c.building == b)) && Mathf.Abs(GameObject.Find("CubCare_Area").transform.position.x - GameObject.Find("Garden_Area").transform.position.x) < 0.5f, string.Join(", ", CarpetPad.All.Select(c => c.building).Distinct()));

        // OBRA SOSTENIBLE: el material sale del bambusal, el barro con paja, el micelio y el cáñamo.
        MaterialStore ms = MaterialStore.Instance;
        TestProbe.Check("hay almacén de material de construcción", ms != null);
        if (ms != null)
        {
            int g0 = ms.Have(MaterialStore.Guadua), grove0 = ms.groveMature;
            bool kit = ms.TakeDayKit("test", "prueba");
            TestProbe.Check("un día de obra gasta guadua, bahareque, micelio y cuerda", kit && ms.Have(MaterialStore.Guadua) == g0 - 4, ms.Summary());
            ms.HarvestGuadua("test", 3);
            TestProbe.Check("cortar guadua saca cañas maduras del bambusal y las pone a curar", ms.groveMature == grove0 - 3 && ms.Processing(MaterialStore.Guadua) >= 3);
        }
        int bamboo = 0; foreach (GardenCrop c in GardenCrop.All) if (c != null && c.kind == GardenCrop.Kind.Bambu) bamboo++;
        TestProbe.Greater("hay un bambusal de guadua", bamboo, 3f, $"{bamboo} matas");

        // Laboratorio y cocina: su producción la hacen PERSONAS (misiones de la jornada).
        CulturedMeatLab lab2 = CulturedMeatLab.All.Count > 0 ? CulturedMeatLab.All[0] : null;
        if (lab2 != null)
            TestProbe.Check("el laboratorio necesita a alguien para iniciar y cosechar los lotes", lab2.requiresStaff);
        if (KitchenLarder.Instance != null) KitchenLarder.Instance.ServeService(40, "test");   // un servicio grande vacía la despensa: hay que volver a elaborar
        var todayMissions = AreaWork.Today(5, ConstructionProject.All);
        TestProbe.Check("las elaboraciones de la cocina (queso, harina, pasta…) son misiones", todayMissions.Any(m => m.id.StartsWith("prep_")), string.Join(", ", todayMissions.Where(m => m.id.StartsWith("prep_")).Select(m => m.title).Take(4)));
        TestProbe.Check("hay misiones de limpieza en las áreas", todayMissions.Count(m => m.title.StartsWith("Limpieza")) >= 5);
        TestProbe.Check("iniciar y cosechar lotes del laboratorio son misiones", todayMissions.Any(m => m.id == "lab_iniciar") && todayMissions.Any(m => m.id == "lab_cosechar"));

        // LANZADOR POR PASOS: hechizo → parámetros → Intro vacío lanza.
        SpellConsole sc = FindAnyObjectByType<SpellConsole>();
        if (sc != null)
        {
            sc.Enter("walk");
            TestProbe.Check("paso 1: «walk» + Intro elige walkTo y pide su primer parámetro", sc.PendingSpell == "walkTo" && sc.PendingParam == "destiny", $"{sc.PendingSpell} · {sc.PendingParam}");
            sc.Enter("Magnate");
            TestProbe.Check("paso 2: el valor se fija y pasa al siguiente parámetro", sc.PendingSpell == "walkTo" && sc.PendingParam == "maxSpeed", sc.PendingParam);
            sc.Enter("");
            TestProbe.Check("Intro con la entrada vacía lanza el hechizo con lo dado", sc.PendingSpell == null && sc.LastLog.Length > 0, sc.LastLog);
            sc.Enter("cancel"); sc.Enter("");
            // ATENCIÓN VISUAL: «enfocar» fija la mirada (y la cámara en 1ª persona la acerca).
            sc.Enter("enfocar"); sc.Enter("Magnate"); sc.Enter("");
            VisualAttention pva = VisualAttention.Player;
            TestProbe.Check("el hechizo «enfocar» fija la atención visual del jugador", pva != null && pva.focus != null && pva.conscious, pva != null && pva.focus != null ? pva.focus.name : "sin foco");
            TestProbe.Check("la atención acerca lo enfocado (zoom de la cámara en 1ª persona)", pva != null && pva.ZoomDegrees > 0f, pva != null ? $"{pva.ZoomDegrees:0.#}°" : "—");
        }

        // VIDA DIARIA: restaurante con sillas para todos, aseos, camas; fauna en proporción a los humanos.
        int seats = 0; GameObject dn = GameObject.Find("Comedor_Gourmet");
        if (dn != null) foreach (Transform t in dn.GetComponentsInChildren<Transform>()) if (t.name == "Asiento") seats++;
        TestProbe.Greater("el comedor tiene sillas para 30 comensales", seats, 29f, $"{seats} sillas");
        int wcSeats = 0; foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name == "Inodoro_seco") wcSeats++;
        TestProbe.Greater("hay inodoros a los que ir al baño", wcSeats, 5f, $"{wcSeats}");
        TestProbe.Check("la iniciación de turistas del hospital es una misión de virtualización con su escena", Application.CanStreamedLevelBeLoaded(VirtualMissions.SceneName("enf_iniciacion")) && VirtualMissions.Steps("enf_iniciacion").Length >= 4);
        int noScene = 0; foreach (var vm in VirtualMissions.All()) if (!Application.CanStreamedLevelBeLoaded(VirtualMissions.SceneName(vm.key))) noScene++;
        TestProbe.Check("todas las misiones de trabajo tienen su escena de virtualización", noScene == 0, $"{noScene} sin escena");
        // ALFOMBRAS (docs/alfombras-plan.md): sin puentes; cada piso con su área, y en cada techo las de edificio y santuario.
        TestProbe.Check("ya no hay puentes entre plataformas", !FindObjectsByType<Transform>(FindObjectsSortMode.None).Any(t => (t.name.StartsWith("Puente_") && t.name != "Puente_piscina") || (t.name == "Pasarela" && t.parent != null && t.parent.name != "Puente_piscina")));   // (el puente de la piscina del reactor es equipo)
        var pads = CarpetPad.All;
        int hospFloors = pads.Where(p => p.building == AreaWork.Hospital && p.kind == CarpetPad.Kind.Edificio).Select(p => p.floor).Distinct().Count();
        TestProbe.Greater("el hospital tiene alfombras en cada piso, en el hielo y en el techo (5)", hospFloors, 4f, $"{hospFloors} pisos con alfombras");
        int roofs = pads.Count(p => p.kind == CarpetPad.Kind.Santuario);
        TestProbe.Greater("cada edificio tiene en su techo las alfombras del santuario", roofs, 15f, $"{roofs} techos");
        TestProbe.Check("en el techo, las de edificio y las del santuario están en el mismo piso", pads.Where(p => p.kind == CarpetPad.Kind.Santuario && p.building != "Residencia").All(s => pads.Any(e => e.kind == CarpetPad.Kind.Edificio && e.surface == s.surface)));
        TestProbe.Check("cada casa del árbol tiene su alfombra (y la residencia, la de cada habitación)", pads.Count(p => p.kind == CarpetPad.Kind.Habitacion) >= 12 && pads.Any(p => p.kind == CarpetPad.Kind.Residencia));
        CarpetPad anyB = pads.FirstOrDefault(p => p.kind == CarpetPad.Kind.Edificio && p.floor == 1);
        TestProbe.Check("el selector de una alfombra de edificio solo ofrece sus pisos", anyB != null && anyB.Destinations().Count > 0 && anyB.Destinations().All(d => d.building == anyB.building));
        // De un piso del hospital a la cama de Kushal: piso → techo → residencia → su habitación, sin escaleras.
        CarpetPad start = pads.FirstOrDefault(p => p.building == AreaWork.Hospital && p.kind == CarpetPad.Kind.Edificio && p.floor == 2);
        Bedroom home = player != null ? Bedroom.Of(player.GetComponent<Anima>()) : null;
        if (start != null && home != null && home.bed != null)
        {
            GameObject traveler = new GameObject("Viajero_TEST");
            traveler.transform.position = start.transform.position + start.transform.right * 4.5f;
            foreach (FlyingCarpet fc in FlyingCarpet.All) fc.speed = 200f;
            int flights = FlyingCarpet.Trips; float t1 = Time.time;
            while (Time.time - t1 < 40f)
            {
                Vector3 to = home.bed.position;
                Vector3 d = to - traveler.transform.position; float dy = Mathf.Abs(d.y); d.y = 0f;
                if (d.magnitude < 3f && dy < 2f) break;
                if (!CarpetPad.Route(traveler, ref to)) traveler.transform.position = Vector3.MoveTowards(traveler.transform.position, to, 30f * Time.deltaTime);   // «anda» rápido
                yield return null;
            }
            foreach (FlyingCarpet fc in FlyingCarpet.All) fc.speed = 18f;
            Vector3 left = home.bed.position - traveler.transform.position;
            TestProbe.Check("un trabajador va de un piso del hospital a su casa del árbol en alfombras (3 vuelos)", left.magnitude < 4f && FlyingCarpet.Trips - flights >= 3, $"{FlyingCarpet.Trips - flights} vuelos, a {left.magnitude:0.0} m de la cama");
            Destroy(traveler);
        }
        var census = new System.Collections.Generic.Dictionary<string, int>();
        foreach (Animal a in FindObjectsByType<Animal>(FindObjectsSortMode.None)) if (a != null && !a.death && Mathf.Abs(a.transform.position.x) < 250f && Mathf.Abs(a.transform.position.z) < 250f) census[a.SpeciesName] = (census.TryGetValue(a.SpeciesName, out int cn) ? cn : 0) + 1;
        int bears = census.TryGetValue("Bear", out int bc) ? bc : 0, penguins = census.TryGetValue("Penguin", out int pc) ? pc : 0;
        TestProbe.Check("la fauna va en proporción a los humanos (más pingüinos que osos)", penguins > bears && bears <= PopulationPlan.Cap("Bear"), string.Join(", ", census.Select(kv => $"{kv.Key} {kv.Value}")));
        int machines = 0; foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name.StartsWith("Maquina_")) machines++;
        TestProbe.Greater("el gimnasio tiene 10 tipos de máquina × 3", machines, 29f, $"{machines} máquinas");
        // 2 m LIBRES entre muebles en TODAS las plataformas (Berón 2026-10-28; FurnitureSpacing): cada área, y la planta de la
        // cocina del restaurante (que cuelga de Restaurante_AUTO, no del área). Lo que va unido (baterías, líneas, parejas) no cuenta.
        var spaced = new System.Collections.Generic.List<(string name, Transform t)>();
        foreach (SanctuaryArea sa in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None)) spaced.Add((sa.displayName, sa.transform));
        Transform kitchenFloor = GameObject.Find("Restaurante_AUTO")?.transform.Find("Sala");
        if (kitchenFloor != null) spaced.Add(("Cocina (planta)", kitchenFloor));
        spaced.Sort((x, y) => string.CompareOrdinal(x.name, y.name));
        foreach (var (spName, spRoot) in spaced)
        {
            var bad = FurnitureSpacing.Violations(spRoot);
            TestProbe.Check($"{spName}: 2 m libres entre muebles (y ninguno sobre un área de alfombras)", bad.Count == 0, $"{bad.Count} pares: {FurnitureSpacing.Worst(bad)}");
        }
        // SIN ESCALERAS (Berón 2026-10-28): a cada ANILLO se sube en alfombra, desde los pisos de su edificio, y de él se vuela a ellos.
        int rings = 0, ringsOk = 0, stairs = 0; var noPad = new System.Collections.Generic.List<string>();
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (t.name.StartsWith("Escalera") && t.Find("Peldano") != null && t.GetComponentInParent<SanctuaryArea>() != null) stairs++;
            if (!t.name.StartsWith("Anillo_") || t.Find("Losa_anillo") == null) continue;
            rings++;
            CarpetPad pad = t.GetComponentInChildren<CarpetPad>();
            bool there = pad != null && pad.Destinations().Count > 0 && CarpetPad.All.Exists(o => o != pad && o.FliesTo(pad) && o.surface.Contains("#") && !o.surface.Contains("#anillo"));
            if (there) ringsOk++; else if (noPad.Count < 4) noPad.Add(t.name);
        }
        TestProbe.Check("cada anillo tiene su área de alfombras unida a los pisos de su edificio", rings > 0 && ringsOk == rings, $"{ringsOk}/{rings} anillos; sin alfombras: {string.Join(", ", noPad)}");
        TestProbe.Check("ninguna área tiene escaleras (se sube en alfombra)", stairs == 0, $"{stairs} escaleras");
        // POSICIONES RESERVADAS del microcosmos (regla 8, PlatformSlots; Berón 2026-10-28): cada área deja sitio en su piso para la
        // historia, los personajes, la fauna y la flora de su microcosmos, y ninguna plaza cae junto a un mueble (a < 1 m). Si se
        // cambia una plataforma (más muebles, menos suelo) y ya no caben, esto falla: ampliar el piso o recolocar.
        foreach (SanctuaryArea sa in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None).OrderBy(a => a.displayName, System.StringComparer.Ordinal))
        {
            PlatformSlots ps = sa.GetComponent<PlatformSlots>();
            bool room = ps != null && ps.story.Count >= PlatformSlots.MinStory && ps.characters.Count >= PlatformSlots.MinCharacters
                     && ps.fauna.Count >= PlatformSlots.MinFauna && ps.flora.Count >= PlatformSlots.MinFlora;
            TestProbe.Check($"{sa.displayName}: posiciones reservadas para su microcosmos (≥ {PlatformSlots.MinStory} historia, {PlatformSlots.MinCharacters} personajes, {PlatformSlots.MinFauna} fauna, {PlatformSlots.MinFlora} flora)",
                            room, ps != null ? ps.Summary() : "sin PlatformSlots (reconstruye la escena principal)");
            if (ps == null) continue;
            int close = 0; string who = "";
            var items = FurnitureSpacing.Items(sa.transform);   // los mismos muebles que miden los 2 m de paso
            foreach (PlatformSlots.Kind k in System.Enum.GetValues(typeof(PlatformSlots.Kind)))
                for (int i = 0; i < ps.Count(k); i++)
                {
                    Vector3 w = ps.World(k, i);
                    foreach (var (t, b) in items)
                        if (Mathf.Abs(b.min.y - w.y) < 1.5f && FurnitureSpacing.DistXZ(b, w) < 0.99f)   // (mismo piso)
                        { close++; if (who.Length < 160) who += $"{k} {i} junto a «{t.name}»; "; break; }
                }
            TestProbe.Check($"{sa.displayName}: ninguna posición reservada a menos de 1 m de un mueble", close == 0, $"{close} plazas: {who}");
        }
        int fullSt = 0;
        for (int i = 1; i <= 8; i++)
        {
            GameObject st = GameObject.Find($"Puesto_{i}"); if (st == null) continue;
            var eqs = new System.Collections.Generic.HashSet<string>(st.GetComponentsInChildren<Transform>().Select(x => x.name));
            if (Gastronomy.Menu.All(d => eqs.Contains("Equipo_" + d.equipment))) fullSt++;
        }
        TestProbe.Check("la cocina ocupa la planta: ocho puestos completos (todo el equipo de la carta) en anillo, cámaras y obradores dobles", fullSt == 8 && FindByPrefix("Camara_de_refrigeracion") && FindByPrefix("Panaderia_2") && FindByPrefix("Queseria_2"), $"{fullSt} puestos completos");
        TestProbe.Check("la sala de yoga tiene la plataforma en el centro, alfombras-esterilla en la sala y en su anillo, y las estatuas en él", FindByPrefix("Tarima del maestro") && CarpetPad.All.Count(c => c.building == "Sala de Yoga" && c.surface.EndsWith("#anillo")) >= 8 && FindByPrefix("Estatuas_anillo"));
        // PROGRESO (hechizo «progreso»): abre el checklist de historias y misiones, y lo vuelve a cerrar.
        SpellConsole scp = FindAnyObjectByType<SpellConsole>();
        if (scp != null)
        {
            scp.Enter("progreso()");
            bool open = ProgressPanel.Open; string said = scp.LastLog;
            scp.Enter("progreso()");
            TestProbe.Check("el hechizo «progreso» abre el checklist de historias y misiones", open && said.StartsWith("Progreso:") && !ProgressPanel.Open, said);
        }
        TestProbe.End();
    }

    static bool FindByPrefix(string prefix)
    {
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name.StartsWith(prefix)) return true;
        return false;
    }

    static string QueenInfo(Hive hv)
    {
        foreach (IceInsectLife b in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None))
            if (b.hive == hv && b.isQueen)
                return $"reina a {Vector3.Distance(b.transform.position, hv.transform.position):0.0} m, oficio {b.role} ({b.Doing}), día={(TimeController.timeController == null || TimeController.timeController.IsDay)}, muerta={b.GetComponent<Animal>()?.death}";
        return "sin reina";
    }
}
