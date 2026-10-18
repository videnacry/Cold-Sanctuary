using System.Collections;
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
        TestProbe.Check("la cocina está elevada (≥ 5 m)", kitchen != null && kitchen.transform.position.y >= 5f, kitchen != null ? $"{kitchen.transform.position.y:0.0} m" : "sin cocina");
        GameObject garden = GameObject.Find("Garden_Area");
        TestProbe.Check("el huerto sigue a ras de suelo", garden != null && garden.transform.position.y < 1f);

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
            float oldSpeed = lab.speed; lab.speed = 400f;
            bool sawProliferation = false;
            float t1 = Time.time;
            while (lab.portionsMade == 0 && Time.time - t1 < 8f) { sawProliferation |= lab.stage == CulturedMeatLab.Stage.Proliferacion; yield return null; }
            lab.speed = oldSpeed;
            TestProbe.Check("el lote pasa por la proliferación en biorreactor", sawProliferation);
            TestProbe.Greater("…y cosecha raciones de carne para los depredadores", lab.portionsMade, 0f, $"{lab.portionsMade} raciones");
        }

        // Colmena: la reina pone huevos en las celdillas y las obreras tienen oficios distintos según su edad.
        Hive hv = null;
        foreach (Hive h in Hive.All) { foreach (IceInsectLife b in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None)) if (b.hive == h) { hv = h; break; } if (hv != null) break; }
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
            foreach (string line in CulturedMeatLab.Lines) larder.Add(CulturedMeatLab.MeatItem(line), 10f);
            for (int h = 0; h < 80; h++) larder.Tick(1f);   // 80 horas de juego
            int ready = 0; foreach (Gastronomy.Dish d in Gastronomy.Menu) if (larder.CanServe(d)) ready++;
            TestProbe.Greater("el obrador elabora sus bases (yogur, quesos, masas, tofu…)", larder.prepsDone, 15f, $"{larder.prepsDone} elaboraciones; yogur {larder.Have("yogur"):0}");
            TestProbe.Greater("tras 80 h de mise en place casi toda la carta está lista", ready, Gastronomy.Menu.Length * 0.75f, larder.Report());
            TestProbe.Check("la lasaña sale (pasta fresca, ragú de carne cultivada, bechamel, parmesano)", larder.Serve("Lasaña"), string.Join(", ", larder.Missing(Gastronomy.DishOf("Lasaña"))));
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
