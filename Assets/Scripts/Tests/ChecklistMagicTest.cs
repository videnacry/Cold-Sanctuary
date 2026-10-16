using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatiza por `TestProbe` los ítems de MAGIA de `docs/testing-checklist.md` §19a–§19g (que pedían el HUD del
/// sandbox `Magia_AUTO`): comer antes/después de desbloquear, `Grimoire.Learn("awaken-reserves")`, apetito/antojo/
/// selectividad, costes reales de fuego/agua/tierra/viento, `Pay` todo-o-nada, aura tras lanzar, abastecer, el reparto
/// de `DecompositionJob`, las fases del `DecompositionMinigame`, los topes que escalan con stats, el `powerBonus`
/// unificado de `SpellBase` y el sustrato de quarks.
///
/// Determinista y NO destructivo: todo ocurre en GameObjects de prueba que se destruyen al final; lo único global que
/// toca es el ledger `SanctuaryResources`, y deshace exactamente lo que sumó.
/// </summary>
public class ChecklistMagicTest : MonoBehaviour, ITestUnit
{
    public int Group => 21;
    public bool ParallelSafe => false;   // toca el ledger global de recursos (y lo restaura)

    readonly List<GameObject> _trash = new List<GameObject>();

    public IEnumerator Run()
    {
        TestProbe.Begin("ChecklistMagic (§19 comer→reservas, hechizos, descomposición, quarks)");
        yield return null;

        yield return ComerYDesbloquear();       // §19a / §19b
        ApetitoYAntojo();                       // §19a (Appetite/Craving/Selectivity)
        yield return FrioSubeElGasto();         // §19a (frío)
        Fuego();                                // §19c fuego químico / masa-energía + aura
        Elementales();                          // §19c agua/tierra/viento
        PagoTodoONada();                        // §19c Pay(costs, energy)
        Abastecer();                            // §19c SupplySpell
        Descomposicion();                       // §19d
        Minijuego();                            // §19e
        TopesPorStats();                        // §19f-bis
        PowerBonus();                           // §19g
        Quarks();                               // §19f

        foreach (GameObject g in _trash) if (g != null) Destroy(g);
        _trash.Clear();
        TestProbe.End();
    }

    GameObject New(string name)
    {
        GameObject g = new GameObject(name);
        g.transform.position = new Vector3(4000f, -300f, 4000f);   // lejos de todo (sin vecinos ni rejillas)
        _trash.Add(g);
        return g;
    }

    /// <summary>Un mago de prueba: Anima + Constitution + MagicReserves + Metabolism (manual) + Grimoire.</summary>
    GameObject MakeMage(string name, out Anima a, out MagicReserves mr, out Metabolism met, out Grimoire gr)
    {
        GameObject g = New(name);
        a = g.AddComponent<Anima>();
        g.AddComponent<Constitution>();
        mr = g.AddComponent<MagicReserves>();
        met = g.AddComponent<Metabolism>();
        met.enabled = false;                 // sin quema por frame: absorciones deterministas
        gr = g.AddComponent<Grimoire>();
        return g;
    }

    // ── §19a / §19b ──────────────────────────────────────────────────────────────
    IEnumerator ComerYDesbloquear()
    {
        MakeMage("Mago_Comer_TEST", out Anima a, out MagicReserves mr, out Metabolism met, out Grimoire gr);
        Constitution con = a.GetComponent<Constitution>();

        // ANTES de desbloquear: comer NO llena pools; lo útil → Constitution; el exceso → grasa.
        float fat0 = a.fatReserves, n0 = con.El("N"), str0 = a.strength;
        met.AbsorbFood(50f, OrganicMaterial.Meat);
        TestProbe.Check("[1108] comer ANTES de desbloquear no llena las pools de magia",
                        !mr.unlocked && mr.Get("N") == 0f && mr.Get("H") == 0f, $"N={mr.Get("N")} H={mr.Get("H")}");
        TestProbe.Greater("[1108] el exceso de comer (bloqueado) va a GRASA", a.fatReserves, fat0, $"grasa {fat0:0.###}→{a.fatReserves:0.###}");
        TestProbe.Greater("[1108] lo útil alimenta la Constitution (N sube)", con.El("N"), n0);

        for (int i = 0; i < 10; i++) yield return null;   // la Constitution converge poco a poco (Update)
        TestProbe.Greater("[1112] al comer, la fuerza sube poco a poco (Constitution)", a.strength, str0,
                          $"str {str0:0.#####}→{a.strength:0.#####}");
        TestProbe.Check("[1112] …y sube GRADUAL (no salta de golpe)", a.strength - str0 < 0.05f, $"Δ={a.strength - str0:0.#####}");

        // Aprender el 1er hechizo → desbloquea, siembra H/C/N/O, dispara OnLearned.
        string learned = null;
        gr.OnLearned += id => learned = id;
        bool ok = gr.Learn("awaken-reserves");
        TestProbe.Check("[1117] Learn(\"awaken-reserves\") → MagicReserves.unlocked", ok && mr.unlocked);
        bool seeded = true;
        foreach (string s in new[] { "H", "C", "N", "O" })
        {
            bool has = false;
            foreach (ElementAmount r in mr.reserves) if (r != null && r.symbol == s) has = true;
            seeded &= has;
        }
        TestProbe.Check("[1117] el despertar siembra las pools H/C/N/O", seeded, $"pools={mr.reserves.Count}");
        TestProbe.Check("[1117] Learn dispara OnLearned con su id", learned == "awaken-reserves", $"id={learned}");
        TestProbe.Check("[1117] Learn es idempotente (2ª vez → false)", !gr.Learn("awaken-reserves"));

        // DESPUÉS: el exceso llena las pools (carne→N/H, fruta→C) hasta el tope, y solo lo que sobra → grasa.
        float cap = mr.EffectiveCapPerElement;
        float fat1 = a.fatReserves;
        met.AbsorbFood(10f, OrganicMaterial.Meat);
        TestProbe.Check("[1110] tras desbloquear, comer CARNE llena N y H", mr.Get("N") > 0f && mr.Get("H") > 0f,
                        $"N={mr.Get("N"):0.##} H={mr.Get("H"):0.##}");
        TestProbe.Near("[1110] …y si cabe en las pools, NO engorda", a.fatReserves, fat1, 1e-4f);
        met.AbsorbFood(10f, OrganicMaterial.Fruit);
        TestProbe.Greater("[1110] comer FRUTA llena C", mr.Get("C"), 0f);
        float fat2 = a.fatReserves;
        met.AbsorbFood(5000f, OrganicMaterial.Meat);
        TestProbe.Near("[1110] las pools se llenan hasta capPerElement (N)", mr.Get("N"), cap, 0.01f, $"tope={cap:0.#}");
        TestProbe.Greater("[1110] lo que ya no cabe → grasa", a.fatReserves, fat2);
    }

    // ── §19a Appetite / Craving / Selectivity ────────────────────────────────────
    void ApetitoYAntojo()
    {
        MakeMage("Mago_Apetito_TEST", out Anima a, out MagicReserves _, out Metabolism met, out Grimoire _);
        TestProbe.Near("[1114] saciado (pools llenos) → apetito 0", met.Appetite, 0f, 0.001f);
        TestProbe.Near("[1114] saciado → selectivo (Selectivity 1)", met.Selectivity, 1f, 0.001f);
        foreach (NutrientNeed n in met.needs) if (n.nutrient == "protein") n.pool = 0f;
        TestProbe.Greater("[1114] falta proteína → sube el apetito", met.Appetite, 0f);
        TestProbe.Check("[1114] el antojo es lo que más falta (protein)", met.Craving == "protein", $"craving={met.Craving}");
        foreach (NutrientNeed n in met.needs) n.pool = 0f;
        TestProbe.Check("[1114] hambriento del todo → come cualquier cosa (Selectivity ~0)", met.Selectivity < 0.05f,
                        $"sel={met.Selectivity:0.##}");
        a.fatReserves = 1f;   // leptina: con reservas de grasa el apetito se atenúa
        TestProbe.Check("[1114] con grasa (leptina) el apetito se atenúa", met.Appetite < 0.05f, $"apetito={met.Appetite:0.##}");
    }

    // ── §19a frío sube el gasto ──────────────────────────────────────────────────
    IEnumerator FrioSubeElGasto()
    {
        GameObject gw = New("Templado_TEST"); Anima aw = gw.AddComponent<Anima>(); Metabolism mw = gw.AddComponent<Metabolism>();
        GameObject gc = New("Frio_TEST");     Anima ac = gc.AddComponent<Anima>(); Metabolism mc = gc.AddComponent<Metabolism>();
        aw.temperature = 38f; ac.temperature = 30f;
        for (int i = 0; i < 15; i++) yield return null;
        float fatW = 1f, fatC = 1f;
        foreach (NutrientNeed n in mw.needs) if (n.nutrient == "fat") fatW = n.pool;
        foreach (NutrientNeed n in mc.needs) if (n.nutrient == "fat") fatC = n.pool;
        TestProbe.Check("[1113] el FRÍO (temp<37) quema más grasa (más antojo de grasa)", fatC < fatW,
                        $"pool grasa frío={fatC:0.####} templado={fatW:0.####}");
        mw.enabled = false; mc.enabled = false;
    }

    // ── §19c fuego ───────────────────────────────────────────────────────────────
    void Fuego()
    {
        MakeMage("Mago_Fuego_TEST", out Anima a, out MagicReserves mr, out Metabolism _, out Grimoire gr);
        gr.Learn("awaken-reserves");
        a.gameObject.AddComponent<MagicAura>();
        FireSpell fire = a.gameObject.AddComponent<FireSpell>();   // spellKey None → solo por Cast()

        fire.SetTier(FireTier.Spark);
        TestProbe.Near("[1122] chispa cuesta ~0,018 g de combustible", fire.FuelGrams, 0.0178f, 0.001f);
        fire.SetTier(FireTier.Flamethrower);
        TestProbe.Near("[1122] lanzallamas cuesta ~22 g de combustible", fire.FuelGrams, 22.2f, 0.2f);
        TestProbe.Check("[1122] SIN reservas → no sale", !fire.Cast());

        mr.Add("C", 100f); mr.Add("H", 100f); mr.energy = 1e6f;
        float c0 = mr.Get("C"), h0 = mr.Get("H"), e0 = mr.energy, ign = fire.EnergyCost, fuel = fire.FuelGrams;
        bool ok = fire.Cast();
        TestProbe.Check("[1122] con reservas, el lanzallamas sale", ok);
        TestProbe.Near("[1122] químico: cobra C (85,7% del combustible)", c0 - mr.Get("C"), fuel * 0.857f, 0.01f);
        TestProbe.Near("[1122] químico: cobra H (14,3% del combustible)", h0 - mr.Get("H"), fuel * 0.143f, 0.01f);
        TestProbe.Near("[1122] químico: cobra solo la IGNICIÓN en julios", e0 - mr.energy, ign, 1f, $"ignición={ign:0}");
        TestProbe.Check("[1129] lanzar un hechizo vuelve el aura DESTRUCTIVA (temido)", a.magicAura < 0f, $"aura={a.magicAura:0.##}");

        // Masa-energía (aliento de dragón): cobra ~µg + TODA la energía del fuego (del pool).
        fire.SetTier(FireTier.DragonBreath);
        mr.energy = 1000f;
        TestProbe.Check("[1124] masa-energía SIN energía en el pool → no sale", !fire.Cast());
        mr.energy = 2e8f;
        float cBefore = mr.Get("C"), eBefore = mr.energy, need = fire.EnergyCost, grams = fire.AnnihilatedGrams;
        ok = fire.Cast();
        TestProbe.Check("[1124] masa-energía con el pool lleno → sale", ok);
        TestProbe.Near("[1124] masa-energía cobra TODA la energía del fuego (1e8 J)", eBefore - mr.energy, need, 100f, $"exige={need:0}");
        TestProbe.Check("[1124] masa-energía solo aniquila ~µg de materia", grams > 0f && grams < 1e-5f && cBefore - mr.Get("C") < 1e-3f,
                        $"aniquila={grams:0.###e0} g");
    }

    // ── §19c agua / tierra / viento ──────────────────────────────────────────────
    void Elementales()
    {
        MakeMage("Mago_Elem_TEST", out Anima a, out MagicReserves mr, out Metabolism _, out Grimoire _);
        mr.Add("H", 500f); mr.Add("O", 10000f); mr.Add("Si", 5000f); mr.energy = 10000f;
        ElementalSpell sp = a.gameObject.AddComponent<ElementalSpell>();

        sp.SetElement(SpellElement.Water);
        float h0 = mr.Get("H"), o0 = mr.Get("O"), e0 = mr.energy;
        TestProbe.Check("[1126] agua sale con reservas", sp.Cast());
        TestProbe.Near("[1126] agua gasta ~11% H de 1 kg", h0 - mr.Get("H"), 111.9f, 0.5f);
        TestProbe.Near("[1126] agua gasta ~89% O de 1 kg", o0 - mr.Get("O"), 888.1f, 0.5f);
        TestProbe.Near("[1126] agua cuesta ~200 J", e0 - mr.energy, 200f, 1f);

        sp.SetElement(SpellElement.Earth);
        float si0 = mr.Get("Si"); o0 = mr.Get("O"); e0 = mr.energy;
        TestProbe.Check("[1126] tierra sale con reservas", sp.Cast());
        TestProbe.Near("[1126] tierra gasta MUCHA materia: Si de 5 kg", si0 - mr.Get("Si"), 2337f, 1f);
        TestProbe.Near("[1126] tierra gasta MUCHA materia: O de 5 kg", o0 - mr.Get("O"), 2663f, 1f);
        TestProbe.Near("[1126] tierra cuesta poca energía (~250 J)", e0 - mr.energy, 250f, 1f);

        sp.SetElement(SpellElement.Wind);
        float h1 = mr.Get("H"); o0 = mr.Get("O"); e0 = mr.energy;
        TestProbe.Check("[1126] viento sale", sp.Cast());
        TestProbe.Check("[1126] viento: materia 0 (aire gratis)", mr.Get("H") == h1 && mr.Get("O") == o0);
        TestProbe.Near("[1126] viento: ~450 J (pesa en energía)", e0 - mr.energy, 450f, 1f);
    }

    // ── §19c Pay todo-o-nada ─────────────────────────────────────────────────────
    void PagoTodoONada()
    {
        GameObject g = New("Pago_TEST");
        MagicReserves mr = g.AddComponent<MagicReserves>();
        mr.Add("C", 10f); mr.energy = 100f;
        var costs = new List<ElementCost> { new ElementCost { symbol = "C", amount = 5f }, new ElementCost { symbol = "Fe", amount = 1f } };
        TestProbe.Check("[1128] Pay sin un elemento → false y NO cobra nada",
                        !mr.Pay(costs, 10f) && mr.Get("C") == 10f && mr.energy == 100f);
        var cOnly = new List<ElementCost> { new ElementCost { symbol = "C", amount = 5f } };
        TestProbe.Check("[1128] Pay sin energía suficiente → false y NO cobra materia",
                        !mr.Pay(cOnly, 1000f) && mr.Get("C") == 10f && mr.energy == 100f);
        TestProbe.Check("[1128] Pay con todo → cobra materia Y energía",
                        mr.Pay(cOnly, 40f) && Mathf.Approximately(mr.Get("C"), 5f) && Mathf.Approximately(mr.energy, 60f));
    }

    // ── §19c SupplySpell ─────────────────────────────────────────────────────────
    void Abastecer()
    {
        GameObject gc = New("Healer_TEST");
        Anima caster = gc.AddComponent<Anima>();
        MagicReserves cr = gc.AddComponent<MagicReserves>();
        QuarkReserve cq = gc.AddComponent<QuarkReserve>();
        SupplySpell sup = gc.AddComponent<SupplySpell>();
        GameObject gt = New("Abastecido_TEST");
        Anima target = gt.AddComponent<Anima>();
        MagicReserves tr = gt.AddComponent<MagicReserves>();
        QuarkReserve tq = gt.AddComponent<QuarkReserve>();

        cr.energy = 1000f; cr.Add("C", 50f); cq.AddGrams(10);
        sup.energyToGive = 300f; sup.quarkGramsToGive = 2f;
        sup.elementsToGive = new List<ElementCost> { new ElementCost { symbol = "C", amount = 20f } };
        bool ok = sup.SupplyTo(target);
        TestProbe.Check("[1130] SupplyTo transfiere", ok);
        TestProbe.Check("[1130] energía: baja la del lanzador y sube la del objetivo",
                        Mathf.Approximately(cr.energy, 700f) && Mathf.Approximately(tr.energy, 300f), $"lanzador {cr.energy} / objetivo {tr.energy}");
        TestProbe.Check("[1130] C: pasa del lanzador al objetivo",
                        Mathf.Approximately(cr.Get("C"), 30f) && Mathf.Approximately(tr.Get("C"), 20f));
        TestProbe.Check("[1130] quarks: pasan 2 g", System.Math.Abs(cq.GramsAvailable - 8.0) < 1e-6 && System.Math.Abs(tq.GramsAvailable - 2.0) < 1e-6,
                        $"lanzador {cq.GramsAvailable:0.###} g / objetivo {tq.GramsAvailable:0.###} g");

        GameObject gp = New("HealerPobre_TEST");
        gp.AddComponent<Anima>(); gp.AddComponent<MagicReserves>(); gp.AddComponent<QuarkReserve>();
        SupplySpell poor = gp.AddComponent<SupplySpell>();
        poor.energyToGive = 300f; poor.quarkGramsToGive = 1f;
        poor.elementsToGive = new List<ElementCost> { new ElementCost { symbol = "C", amount = 5f } };
        TestProbe.Check("[1130] sin recursos → no transfiere (\"sin recursos\")", !poor.SupplyTo(target));
    }

    // ── §19d DecompositionJob ────────────────────────────────────────────────────
    void Descomposicion()
    {
        SanctuaryResources econ = SanctuaryResources.Instance;
        float el0 = econ.Get(SanctuaryResource.Elements), en0 = econ.Get(SanctuaryResource.Energy);

        // Sin obrero: TODO va a la economía.
        DecompositionJob j1 = New("Descomp_SinObrero_TEST").AddComponent<DecompositionJob>();
        j1.yield = Yield(10f, 5f); j1.energyJoules = 1000f; j1.workerCut = 0.2f;
        j1.Complete();
        TestProbe.Near("[1135] sin obrero: toda la MATERIA → SanctuaryResources.Elements",
                       econ.Get(SanctuaryResource.Elements) - el0, 15f, Tol(el0, 0.01f));
        TestProbe.Near("[1135] sin obrero: toda la ENERGÍA → SanctuaryResources.Energy",
                       econ.Get(SanctuaryResource.Energy) - en0, 1000f, Tol(en0, 0.5f));

        // Con obrero (reservas desbloqueadas): workerCut a sus pools/energía; el resto a la economía.
        MakeMage("Obrero_TEST", out Anima w, out MagicReserves wr, out Metabolism _, out Grimoire wg);
        wg.Learn("awaken-reserves");
        DecompositionJob j2 = New("Descomp_ConObrero_TEST").AddComponent<DecompositionJob>();
        j2.worker = w; j2.workerReserves = wr; j2.workerGrimoire = wg;
        j2.yield = Yield(10f, 5f); j2.energyJoules = 1000f; j2.workerCut = 0.2f;
        float el1 = econ.Get(SanctuaryResource.Elements), en1 = econ.Get(SanctuaryResource.Energy);
        j2.Complete();
        TestProbe.Check("[1137] con obrero: cobra su parte en sus pools (C 2 g + H 1 g)",
                        Mathf.Approximately(wr.Get("C"), 2f) && Mathf.Approximately(wr.Get("H"), 1f), $"C={wr.Get("C")} H={wr.Get("H")}");
        TestProbe.Near("[1137] con obrero: cobra su parte de energía (200 J)", wr.energy, 200f, 0.5f);
        TestProbe.Near("[1137] con obrero: el resto de materia → economía (12 g)", econ.Get(SanctuaryResource.Elements) - el1, 12f, Tol(el1, 0.01f));
        TestProbe.Near("[1137] con obrero: el resto de energía → economía (800 J)", econ.Get(SanctuaryResource.Energy) - en1, 800f, Tol(en1, 0.5f));

        // energyPhysicsId: si el grimorio NO lo conoce, su parte de energía va entera a la economía.
        DecompositionJob j3 = New("Descomp_Fision_TEST").AddComponent<DecompositionJob>();
        j3.worker = w; j3.workerReserves = wr; j3.workerGrimoire = wg;
        j3.yield = Yield(10f, 0f); j3.energyJoules = 1000f; j3.workerCut = 0.2f; j3.energyPhysicsId = "fision-TEST";
        float wE = wr.energy, en2 = econ.Get(SanctuaryResource.Energy);
        TestProbe.Check("[1138] por defecto (sin energyPhysicsId) la energía se capta", j2.EnergyRevealed);
        TestProbe.Check("[1138] con energyPhysicsId desconocido → energía NO revelada", !j3.EnergyRevealed);
        j3.Complete();
        TestProbe.Near("[1138] no revelada: el obrero no capta energía", wr.energy, wE, 0.01f);
        TestProbe.Near("[1138] no revelada: TODA la energía → economía", econ.Get(SanctuaryResource.Energy) - en2, 1000f, Tol(en2, 0.5f));
        wg.Learn("fision-TEST");
        TestProbe.Check("[1138] tras aprender la física → energía revelada", j3.EnergyRevealed);

        // Restaurar el ledger global (deshacer exactamente lo sumado).
        econ.Add(SanctuaryResource.Elements, el0 - econ.Get(SanctuaryResource.Elements));
        econ.Add(SanctuaryResource.Energy, en0 - econ.Get(SanctuaryResource.Energy));
    }

    /// <summary>Tolerancia que respeta la precisión float del ledger (si el total ya es grande, el delta pierde decimales).</summary>
    static float Tol(float baseValue, float min) => min + Mathf.Abs(baseValue) * 4e-7f;

    static List<ElementAmount> Yield(float c, float h)
    {
        var l = new List<ElementAmount> { new ElementAmount { symbol = "C", amount = c } };
        if (h > 0f) l.Add(new ElementAmount { symbol = "H", amount = h });
        return l;
    }

    // ── §19e DecompositionMinigame ───────────────────────────────────────────────
    void Minijuego()
    {
        SanctuaryResources econ = SanctuaryResources.Instance;
        float el0 = econ.Get(SanctuaryResource.Elements), en0 = econ.Get(SanctuaryResource.Energy);

        GameObject g = New("Minijuego_TEST");
        DecompositionJob job = g.AddComponent<DecompositionJob>();   // sin obrero → todo a la economía
        DecompositionMinigame mg = g.AddComponent<DecompositionMinigame>();
        mg.enabled = false;                  // sin temporizador ni OnGUI: lo conducimos a mano
        mg.job = job;
        mg.perItemSeconds = 999f;
        mg.batch = new List<DecompSample>
        {
            Sample("H2O", 1000f, ("H", 2f), ("O", 16f)),   // identificada + rota perfecta + bien clasificada → cuenta entera
            Sample("CO2",  500f, ("C", 12f), ("O", 32f)),  // mal identificada → NO cuenta
            Sample("CH4",  400f, ("C", 12f), ("H", 4f)),   // identificada + rota a medias + 1 mal clasificada → solo C×0,5
        };

        mg.Begin();
        TestProbe.Check("[1142] Iniciar jornada → fase 1 IDENTIFICAR", mg.phase == DecompositionMinigame.Phase.Identify);
        mg.SubmitName("H2O"); mg.SubmitName("NaCl"); mg.SubmitName("CH4");
        TestProbe.Check("[1142] tras identificar todas → fase 2 ROMPER", mg.phase == DecompositionMinigame.Phase.Break);
        mg.SubmitBreak(1f); mg.SubmitBreak(1f); mg.SubmitBreak(0.5f);
        TestProbe.Check("[1142] tras romper todas → fase 3 CLASIFICAR", mg.phase == DecompositionMinigame.Phase.Classify);
        mg.Classify("H"); mg.Classify("O");     // H2O bien
        mg.Classify("C"); mg.Classify("O");     // CO2 (no identificada: da igual)
        mg.Classify("C"); mg.Classify("O");     // CH4: C bien, H mal (lo mete en O)
        TestProbe.Check("[1142] tras clasificar todas → jornada terminada", mg.phase == DecompositionMinigame.Phase.Finished);

        // yield = H2O (H2+O16) + CH4 solo C×0,5 = 2 + 16 + 6 = 24 g; energía = 1000 + 400×0,5 = 1200 J.
        TestProbe.Near("[1145] solo cuentan identificadas+rotas+bien clasificadas (×calidad)",
                       econ.Get(SanctuaryResource.Elements) - el0, 24f, Tol(el0, 0.01f));
        TestProbe.Near("[1145] la energía escala con la calidad de ruptura", econ.Get(SanctuaryResource.Energy) - en0, 1200f, Tol(en0, 0.5f));
        TestProbe.Check("[1145] al terminar llama a Complete() (el job recibió el yield)", job.yield != null && job.yield.Count == 3,
                        $"elementos={job.yield?.Count}");

        econ.Add(SanctuaryResource.Elements, el0 - econ.Get(SanctuaryResource.Elements));
        econ.Add(SanctuaryResource.Energy, en0 - econ.Get(SanctuaryResource.Energy));
    }

    static DecompSample Sample(string name, float joules, params (string s, float g)[] comps)
    {
        DecompSample d = new DecompSample { trueName = name, energyJoules = joules };
        foreach (var c in comps) d.components.Add(new ElementAmount { symbol = c.s, amount = c.g });
        return d;
    }

    // ── §19f-bis topes por stats ─────────────────────────────────────────────────
    void TopesPorStats()
    {
        GameObject g = New("Topes_TEST");
        Anima a = g.AddComponent<Anima>();
        MagicReserves mr = g.AddComponent<MagicReserves>();
        TestProbe.Near("[1150] aptitudes base → tope ~100 g/elemento", mr.EffectiveCapPerElement, 100f, 1f);
        TestProbe.Near("[1150] aptitudes base → tope ~1e6 J", mr.EffectiveEnergyCap, 1e6f, 1e4f);
        float cap0 = mr.EffectiveCapPerElement, e0 = mr.EffectiveEnergyCap;
        a.strength = 2f; a.endurance = 2f; a.bodyMass = 2f;
        TestProbe.Greater("[1150] subir resistencia/fuerza/masa sube EffectiveCapPerElement", mr.EffectiveCapPerElement, cap0 * 1.5f,
                          $"{cap0:0}→{mr.EffectiveCapPerElement:0}");
        TestProbe.Near("[1150] …y no toca el tope de energía", mr.EffectiveEnergyCap, e0, 1f);
        a.reasoning = 2f; a.memory = 2f;
        TestProbe.Greater("[1150] subir razón/memoria sube EffectiveEnergyCap", mr.EffectiveEnergyCap, e0 * 1.5f,
                          $"{e0:0}→{mr.EffectiveEnergyCap:0}");
    }

    // ── §19g powerBonus unificado ────────────────────────────────────────────────
    void PowerBonus()
    {
        GameObject g = New("PowerBonus_TEST");
        Anima a = g.AddComponent<Anima>();
        ChecklistProbeSpell sp = g.AddComponent<ChecklistProbeSpell>();   // decay 0.5/s, rampas 1/s, topes 3

        sp.Tick(a, 0.5f, true, false); sp.Tick(a, 0.5f, true, false);
        TestProbe.Near("[1160] cargar (LShift) acumula la carga", sp.ChargeAccum, 1f, 0.001f);
        TestProbe.Check("[1160] mientras carga NO aplica el bonus (no dispara)", sp.IsCharging && sp.PowerBonus == 0f && sp.releases == 0);
        sp.Tick(a, 0.0001f, false, false);
        TestProbe.Check("[1160] soltar la carga → burst (OnChargeReleased) con lo acumulado", sp.releases == 1 && Mathf.Approximately(sp.lastReleased, 1f),
                        $"inyectado={sp.lastReleased:0.##}");
        TestProbe.Near("[1160] …y el bonus pasa a valer la carga", sp.PowerBonus, 1f, 0.001f);

        sp.Tick(a, 1f, false, false);
        TestProbe.Near("[1165] sin sostener nada, el bonus DECAE (0,5/s)", sp.PowerBonus, 0.5f, 0.001f);
        sp.Tick(a, 5f, false, false);
        TestProbe.Near("[1165] …hasta 0 (vuelta al hechizo base)", sp.PowerBonus, 0f, 0.001f);

        sp.Tick(a, 1f, false, true);
        TestProbe.Near("[1162] canalizar (RShift) sin carga → el bonus sube gradual", sp.PowerBonus, 1f, 0.001f);
        sp.Tick(a, 10f, false, true);
        TestProbe.Near("[1162] …hasta su tope maxPowerWithChanneling", sp.PowerBonus, 3f, 0.001f);
        sp.Tick(a, 1f, false, true);
        TestProbe.Near("[1162] canalizando, el bonus NO decae por debajo del suelo", sp.PowerBonus, 3f, 0.001f);

        sp.ResetBonus();
        for (int i = 0; i < 4; i++) sp.Fail(a);
        TestProbe.Near("[1158] cada intento fallido sube el FORCEJEO (+0,05)", sp.PowerBonus, 0.2f, 0.001f);
        for (int i = 0; i < 500; i++) sp.Fail(a);
        TestProbe.Near("[1158] …hasta su tope maxPowerWithForcejeo", sp.PowerBonus, 3f, 0.001f);

        // Los TOPES escalan por stats: carga/forcejeo con las FÍSICAS, canalizar con las MENTALES.
        sp.ResetBonus();
        a.strength = 2f; a.endurance = 2f; a.bodyMass = 2f;
        sp.Tick(a, 100f, true, false);
        TestProbe.Near("[1169] tope de CARGA × aptitudes físicas (×2)", sp.ChargeAccum, 6f, 0.001f);
        sp.Tick(a, 0.0001f, false, false);
        sp.ResetBonus();
        sp.Tick(a, 100f, false, true);
        TestProbe.Near("[1169] tope de CANALIZAR NO cambia con las físicas", sp.PowerBonus, 3f, 0.001f);
        a.reasoning = 2f; a.memory = 2f; a.creativity = 2f;
        sp.Tick(a, 100f, false, true);
        TestProbe.Near("[1169] tope de CANALIZAR × aptitudes mentales (×2)", sp.PowerBonus, 6f, 0.001f);
    }

    // ── §19f QuarkReserve ────────────────────────────────────────────────────────
    void Quarks()
    {
        GameObject g = New("Quarks_TEST");
        QuarkReserve q = g.AddComponent<QuarkReserve>();
        MagicReserves mr = g.AddComponent<MagicReserves>();   // sin Anima → tope = capPerElement
        mr.capPerElement = 1.5f;

        q.AddGrams(2);
        TestProbe.Check("[1172] AddGrams(2) → GramsAvailable 2", System.Math.Abs(q.GramsAvailable - 2.0) < 1e-9, $"{q.GramsAvailable:0.###}");
        q.AddQuarks(QuarkReserve.QuarksPerGram);
        TestProbe.Check("[1172] AddQuarks(1,807e24) = +1 g", System.Math.Abs(q.GramsAvailable - 3.0) < 1e-9, $"{q.GramsAvailable:0.###}");
        double atoms = q.AtomsAvailable("C");
        TestProbe.Check("[1172] AtomsAvailable(\"C\") da un número plausible (1e22–1e25)", atoms > 1e22 && atoms < 1e25, $"{atoms:0.###e0}");

        float made = q.MakeElement(mr, "Fe", 1f);
        TestProbe.Check("[1174] MakeElement crea el elemento en su pool", Mathf.Approximately(made, 1f) && Mathf.Approximately(mr.Get("Fe"), 1f));
        TestProbe.Check("[1174] …y gasta los quarks de lo creado", System.Math.Abs(q.GramsAvailable - 2.0) < 1e-6, $"{q.GramsAvailable:0.###} g");
        made = q.MakeElement(mr, "Fe", 1f);
        TestProbe.Near("[1174] MakeElement respeta el tope (solo crea lo que cabe)", made, 0.5f, 1e-4f);
        TestProbe.Check("[1174] …y solo gasta quarks por lo creado", System.Math.Abs(q.GramsAvailable - 1.5) < 1e-6, $"{q.GramsAvailable:0.###} g");

        float e0 = mr.energy;
        double j = q.Restitute(mr, 1e-9);
        TestProbe.Check("[1174] Restitute(1 ng) → ~89.875 J (E=mc²)", System.Math.Abs(j - 89875.5) < 50.0, $"{j:0} J");
        TestProbe.Near("[1174] …y sube el pool de energía", mr.energy - e0, (float)j, 1f);
    }
}
