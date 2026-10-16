using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatiza por `TestProbe` los ítems del ALMA de `docs/testing-checklist.md` §20–§22 (que pedían leer los logs
/// `[Alma]` del sandbox `AlmaBlend_AUTO`/`AlmaCompartida_AUTO`): mezclas por distribución (Panterilia, oso con mente
/// humana, bonusPack3, el 1% que importa, tamaños), conversión Relativa vs Literal, tono de mente por blend, los 4
/// compañeros por composición, el alma compartida (poder y bonds), la familiarización del `BondPillar` y el karma de
/// especie (`SpeciesKarma`).
///
/// Determinista y NO destructivo: compone seres de prueba propios (lejos de la escena) y los destruye al final; no
/// toca a los compañeros reales.
/// </summary>
public class ChecklistSoulTest : MonoBehaviour, ITestUnit
{
    public int Group => 22;
    public bool ParallelSafe => false;

    readonly List<GameObject> _trash = new List<GameObject>();
    readonly Dictionary<string, string> _almaLog = new Dictionary<string, string>();

    public IEnumerator Run()
    {
        TestProbe.Begin("ChecklistSoul (§20-22 mezcla, conversión, alma compartida, bonds, karma)");
        yield return null;

        Application.logMessageReceived += CaptureAlma;
        try
        {
            Mezclas();                // §20
            Conversion();             // §20 Ambrosio_Convert
            Companeros();             // §20 / §22
            AlmaCompartida();         // §21
        }
        finally { Application.logMessageReceived -= CaptureAlma; }

        yield return BondsYKarma();   // §22

        foreach (GameObject g in _trash) if (g != null) Destroy(g);
        _trash.Clear();
        TestProbe.End();
    }

    // Guarda la última línea `[Alma] «nombre»: …` de cada ser (para leer el TONO que resolvió la mezcla).
    void CaptureAlma(string msg, string stack, LogType type)
    {
        if (msg == null || !msg.StartsWith("[Alma] «")) return;
        int end = msg.IndexOf('»');
        if (end > 8) _almaLog[msg.Substring(8, end - 8)] = msg;
    }

    string ToneOf(string name) => _almaLog.TryGetValue(name, out string m) && m.Contains("tono ")
        ? m.Substring(m.IndexOf("tono ") + 5).Split(' ')[0] : "?";

    SoulComposition Make(string name, (string a, float d, bool share)[] bodies, (string a, float d, bool share)[] minds,
                         string[] packs = null, bool withMind = false, float x = 0f)
    {
        GameObject g = new GameObject(name);
        g.transform.position = new Vector3(5000f + x, -400f, 5000f);
        _trash.Add(g);
        g.AddComponent<Anima>();
        if (withMind) g.AddComponent<Mind>().enabled = false;   // solo para ver qué aptitudes/pensamientos recibe
        SoulComposition sc = g.AddComponent<SoulComposition>();
        sc.resolveOnStart = false;
        foreach (var b in bodies) sc.bodies.Add(new BlendSlot { archetype = b.a, domain = b.d, shareDomain = b.share });
        foreach (var m in minds) sc.minds.Add(new BlendSlot { archetype = m.a, domain = m.d, shareDomain = m.share });
        if (packs != null) sc.bonusPacks.AddRange(packs);
        sc.Resolve();
        return sc;
    }

    static (string, float, bool)[] Solo(string a) => new[] { (a, 100f, false) };

    // ── §20 mezclas ──────────────────────────────────────────────────────────────
    void Mezclas()
    {
        SoulComposition human = Make("Humano_Puro_TEST", Solo("Human"), Solo("Human"));
        SoulComposition pant = Make("Panterilia_Blend_TEST",
            new[] { ("Human", 90f, false), ("Bear", 0f, true), ("Bunny", 0f, true) },
            new[] { ("Human", 90f, false), ("Lion", 5f, false), ("Fire", 0f, true), ("Agua", 0f, true) });
        Anima h = human.anima, p = pant.anima;

        float maxRel = 0f, sumDiff = 0f;
        foreach (AptitudeKind k in SoulMath.Mental)
        {
            Aptitudes ha = human.ReadStats(), pa = pant.ReadStats();
            float d = Mathf.Abs(pa.Get(k) - ha.Get(k));
            sumDiff += d; maxRel = Mathf.Max(maxRel, d / Mathf.Max(0.01f, ha.Get(k)));
        }
        TestProbe.Check("[1186] Panterilia_Blend (Human 90 + Lion 5 + resto) sale ~humana (mente ±10%)", maxRel < 0.10f, $"máx Δrel={maxRel:0.###}");
        TestProbe.Check("[1186] …con rastro felino (no idéntica a la humana pura)", sumDiff > 0.001f, $"ΣΔ={sumDiff:0.####}");
        TestProbe.Check("[1186] su tono sale del primario humano (Viento)", ToneOf("Panterilia_Blend_TEST") == "Viento",
                        $"tono={ToneOf("Panterilia_Blend_TEST")}");

        SoulComposition osoHum = Make("OsoMenteHumana_TEST", Solo("Bear"), new[] { ("Human", 90f, false), ("Bear", 10f, false) },
                                      null, withMind: true, x: 10f);
        SoulComposition oso = Make("Oso_Puro_TEST", Solo("Bear"), Solo("Bear"), null, false, 20f);
        SoulComposition osoPack = Make("Oso_bonusPack3_TEST", Solo("Bear"), Solo("Bear"), new[] { "bonusPack3" }, withMind: true, x: 30f);
        Anima oh = osoHum.anima;
        TestProbe.Check("[1187] OsoMenteHumana: físico de OSO (str 2.2 / masa 2.5)",
                        Mathf.Abs(oh.strength - 2.2f) < 0.01f && Mathf.Abs(oh.bodyMass - 2.5f) < 0.01f, $"str={oh.strength:0.##} masa={oh.bodyMass:0.##}");
        TestProbe.Check("[1187] OsoMenteHumana: grande (escala de oso 1.6)", Mathf.Abs(oh.transform.localScale.x - 1.6f) < 0.01f,
                        $"escala={oh.transform.localScale.x:0.##}");
        TestProbe.Check("[1187] OsoMenteHumana: mente HUMANA (razón/creatividad altas, > oso puro)",
                        oh.reasoning > 1.3f && oh.creativity > 1.2f && oh.reasoning > oso.anima.reasoning,
                        $"razón {oh.reasoning:0.##} vs oso {oso.anima.reasoning:0.##}; creat {oh.creativity:0.##}");

        bool allPlus = true; string worst = "";
        Aptitudes ob = oso.ReadStats(), opk = osoPack.ReadStats();
        foreach (AptitudeKind k in SoulMath.All)
            if (Mathf.Abs(opk.Get(k) - ob.Get(k) - 2.5f) > 0.001f) { allPlus = false; worst = $"{k}: {ob.Get(k):0.##}→{opk.Get(k):0.##}"; }
        TestProbe.Check("[1189] Oso_bonusPack3: mismas aptitudes de oso +2.5 a TODO", allPlus, worst);
        TestProbe.Check("[1189] …sin cambiar personalidad (mismo tono que el oso puro: Tierra)",
                        ToneOf("Oso_bonusPack3_TEST") == "Tierra" && ToneOf("Oso_Puro_TEST") == "Tierra",
                        $"pack={ToneOf("Oso_bonusPack3_TEST")} puro={ToneOf("Oso_Puro_TEST")}");

        // §20 Mente por blend: el tono emerge del blend MENTAL, no del cuerpo (los dos tienen cuerpo de oso).
        TestProbe.Check("[1197] mente Human (cuerpo oso) → tono Viento", ToneOf("OsoMenteHumana_TEST") == "Viento",
                        $"tono={ToneOf("OsoMenteHumana_TEST")}");
        TestProbe.Check("[1197] mente Bear → tono Tierra", ToneOf("Oso_bonusPack3_TEST") == "Tierra");
        Mind mh = osoHum.GetComponent<Mind>();
        TestProbe.Check("[1197] la Mente lee las aptitudes del blend (razón de la mezcla)",
                        mh != null && Mathf.Approximately(mh.aptitudes.reasoning, oh.reasoning), $"mind={mh?.aptitudes.reasoning:0.##} anima={oh.reasoning:0.##}");
        List<MindPhrase> bearThoughts = Archetypes.BaseThoughtsOf("Bear");
        if (bearThoughts.Count > 0)
        {
            Mind mb = osoPack.GetComponent<Mind>();
            TestProbe.Check("[1197] la mente dominante Bear siembra sus pensamientos de especie", mb != null && mb.thoughts.Contains(bearThoughts[0]),
                            $"thoughts={mb?.thoughts.Count}");
        }

        // §20 tamaño: oso > Panterilia > conejo > gallina.
        SoulComposition bunny = Make("Bunny_TEST", Solo("Bunny"), Solo("Bunny"), null, false, 40f);
        SoulComposition hen = Make("Gallina_TEST", Solo("Gallina"), Solo("Human"), null, false, 50f);
        float sB = oso.transform.localScale.x, sP = p.transform.localScale.x, sBu = bunny.transform.localScale.x, sG = hen.transform.localScale.x;
        TestProbe.Check("[1191] tamaño en escena: oso > Panterilia > conejo > gallina", sB > sP && sP > sBu && sBu > sG,
                        $"oso {sB:0.##} · Panterilia {sP:0.##} · conejo {sBu:0.##} · gallina {sG:0.##}");

        // §20 blend por DISTRIBUCIÓN: un arquetipo al 1% empuja la forma (Lion al 1% en la mente humana).
        SoulComposition onePct = Make("Humano_1pctLeon_TEST", Solo("Human"), new[] { ("Human", 99f, false), ("Lion", 1f, false) }, null, false, 60f);
        // creatividad esperada = 0.99·1.5 + 0.01·(0.8 reescalado al presupuesto humano 8.4/5.6) = 1.497
        TestProbe.Near("[1192] un arquetipo al 1% empuja la forma (creatividad 1.5 → 1.497)", onePct.anima.creativity, 1.497f, 0.0005f);
        TestProbe.Check("[1192] …y ya no es despreciable (≠ humano puro)", Mathf.Abs(onePct.anima.creativity - h.creativity) > 0.001f,
                        $"Δ={onePct.anima.creativity - h.creativity:0.####}");
    }

    // ── §20 conversión ───────────────────────────────────────────────────────────
    void Conversion()
    {
        SoulComposition a = Make("Ambrosio_A_TEST", Solo("Toro"), Solo("Bear"), null, false, 100f);
        SoulComposition b = Make("Ambrosio_B_TEST", Solo("Toro"), Solo("Bear"), null, false, 110f);
        Aptitudes orig = a.ReadStats();
        TestProbe.Check("[1194] arranca Toro+Bear (grande/fuerte/lento)", orig.strength > 1.8f && orig.agility < 1f && a.transform.localScale.x > 1.5f,
                        $"str {orig.strength:0.##} agi {orig.agility:0.##} escala {a.transform.localScale.x:0.##}");

        a.ConvertTo("Ant", "Ant", ConversionMode.Relative);
        b.ConvertTo("Ant", "Ant", ConversionMode.Literal);
        Aptitudes ra = a.ReadStats(), rb = b.ReadStats();
        float target = SoulMath.Budget(a.ComputeBaseStats(), SoulMath.All);
        float origRatio = orig.strength / orig.agility;

        TestProbe.Near("[1194] B/literal: MISMA forma exacta (str/agi conservado)", rb.strength / rb.agility, origRatio, 0.001f);
        TestProbe.Near("[1194] B/literal: al presupuesto de la hormiga", SoulMath.Budget(rb, SoulMath.All), target, 0.01f);
        TestProbe.Check("[1194] A/relativa: la forma la MODULA la hormiga (str/agi cambia)", Mathf.Abs(ra.strength / ra.agility - origRatio) > 0.1f,
                        $"relativa {ra.strength / ra.agility:0.##} vs original {origRatio:0.##}");
        TestProbe.Near("[1194] A/relativa: también al presupuesto de la hormiga", SoulMath.Budget(ra, SoulMath.All), target, 0.01f);
        TestProbe.Check("[1194] ambos modos → tamaño hormiga", Mathf.Abs(a.transform.localScale.x - 0.3f) < 0.01f && Mathf.Abs(b.transform.localScale.x - 0.3f) < 0.01f,
                        $"A {a.transform.localScale.x:0.##} · B {b.transform.localScale.x:0.##}");
    }

    // ── §20 / §22 compañeros por composición ─────────────────────────────────────
    void Companeros()
    {
        Anima pa = Make("Panterilia_SinClase_TEST", Solo("Panterilia"), Solo("Panterilia"), null, false, 200f).anima;
        TestProbe.Check("[1209] Panterilia_SinClase = Panterilia real (per 1.7, rea 1.6, disc 1.5, str 0.7)",
                        Eq(pa.perception, 1.7f) && Eq(pa.reasoning, 1.6f) && Eq(pa.discipline, 1.5f) && Eq(pa.strength, 0.7f),
                        $"per {pa.perception:0.##} rea {pa.reasoning:0.##} disc {pa.discipline:0.##} str {pa.strength:0.##}");
        Anima go = Make("Goluis_SinClase_TEST", Solo("Goluis"), Solo("Goluis"), null, false, 210f).anima;
        TestProbe.Check("[1198] Goluis_SinClase: str 1.5 / disc 1.3", Eq(go.strength, 1.5f) && Eq(go.discipline, 1.3f),
                        $"str {go.strength:0.##} disc {go.discipline:0.##}");
        Anima gh = Make("Gohageneis_SinClase_TEST", Solo("Gohageneis"), Solo("Gohageneis"), null, false, 220f).anima;
        TestProbe.Check("[1198] Gohageneis_SinClase: adapt 1.7 / soc 1.7", Eq(gh.adaptability, 1.7f) && Eq(gh.sociability, 1.7f),
                        $"adapt {gh.adaptability:0.##} soc {gh.sociability:0.##}");
        Anima ir = Make("Irosene_SinClase_TEST", Solo("Irosene"), Solo("Irosene"), null, false, 230f).anima;
        TestProbe.Check("[1198] Irosene_SinClase: soc 1.7 / cre 1.5", Eq(ir.sociability, 1.7f) && Eq(ir.creativity, 1.5f),
                        $"soc {ir.sociability:0.##} cre {ir.creativity:0.##}");
    }

    static bool Eq(float a, float b) => Mathf.Abs(a - b) < 0.001f;

    // ── §21 alma compartida ──────────────────────────────────────────────────────
    void AlmaCompartida()
    {
        SoulComposition melaza = Make("Melaza_TEST", Solo("Toro"), Solo("Bear"), null, false, 300f);
        SoulComposition hormiga = Make("Hormiga_TEST", Solo("Ant"), Solo("Human"), null, false, 310f);
        GameObject sg = new GameObject("AlmaCompartida_TEST");
        _trash.Add(sg);
        SharedSoul soul = sg.AddComponent<SharedSoul>();
        soul.Register(melaza);
        soul.Register(hormiga);

        Aptitudes m = melaza.ReadStats(), h = hormiga.ReadStats();
        TestProbe.Check("[1203] ambos cuerpos comparten la MISMA identidad (forma: str/agi igual)",
                        Mathf.Abs(m.strength / m.agility - h.strength / h.agility) < 0.001f,
                        $"melaza {m.strength / m.agility:0.###} · hormiga {h.strength / h.agility:0.###}");
        float bm = SoulMath.Budget(m, SoulMath.All), bh = SoulMath.Budget(h, SoulMath.All);
        float baseM = SoulMath.Budget(melaza.ComputeBaseStats(), SoulMath.All), baseH = SoulMath.Budget(hormiga.ComputeBaseStats(), SoulMath.All);
        TestProbe.Check("[1203] …cada uno a SU presupuesto/tamaño (el de su cuerpo base)",
                        Mathf.Abs(bm - baseM) < 0.01f && Mathf.Abs(bh - baseH) < 0.01f,
                        $"melaza {bm:0.##}/{baseM:0.##} · hormiga {bh:0.##}/{baseH:0.##}");

        float m0 = melaza.anima.strength, h0 = hormiga.anima.strength;
        soul.GainPower(0.5f);
        TestProbe.Check("[1204] entrena poder (+0.5) → suben los stats de LOS DOS", melaza.anima.strength > m0 && hormiga.anima.strength > h0,
                        $"melaza {m0:0.##}→{melaza.anima.strength:0.##} · hormiga {h0:0.##}→{hormiga.anima.strength:0.##}");
        float m1 = melaza.anima.strength, h1 = hormiga.anima.strength;
        soul.GainPower(-0.3f);
        TestProbe.Check("[1204] se lesiona (−0.3) → bajan ambos", melaza.anima.strength < m1 && hormiga.anima.strength < h1);

        soul.AddBond("Ruth");
        soul.AddBond("Ruth");
        TestProbe.Check("[1205] +bond (Ruth) → el alma lo guarda una vez, compartido", soul.sharedBonds.Count == 1 && soul.sharedBonds.Contains("Ruth"),
                        $"bonds={soul.sharedBonds.Count}");
        TestProbe.Check("[1205] …para todas sus reencarnaciones (2 cuerpos registrados)", soul.BodyCount == 2);
    }

    // ── §22 BondPillar + karma ───────────────────────────────────────────────────
    IEnumerator BondsYKarma()
    {
        // Ser compuesto con karma HUMANA (speciesBonds=Human), como Panterilia_SinClase.
        Anima human = Composed("Karma_Humano_TEST", "Human", new Vector3(6000f, -500f, 6000f));
        TestProbe.Near("[1211] karma Human→Malamute = +45 (perro↔humano)", SpeciesKarma.RelationOf(human, "Malamute"), 45f, 0.001f);
        TestProbe.Near("[1211] karma con especie sin relación = 0", SpeciesKarma.RelationOf(human, "Komodo"), 0f, 0.001f);
        Anima seal = Composed("Karma_Foca_TEST", "Seal", new Vector3(6100f, -500f, 6000f));
        TestProbe.Check("[1211] karma NEGATIVA (Seal→Bear)", SpeciesKarma.RelationOf(seal, "Bear") < 0f, $"{SpeciesKarma.RelationOf(seal, "Bear"):0}");

        ITarget dog = Neighbor("Perro_TEST", "Malamute", new Vector3(6002f, -500f, 6000f));
        ITarget komodo = Neighbor("Komodo_TEST", "Komodo", new Vector3(5998f, -500f, 6000f));
        ITarget bear = Neighbor("Oso_TEST", "Bear", new Vector3(6102f, -500f, 6000f));
        human.gameObject.AddComponent<BondPillar>();
        seal.gameObject.AddComponent<BondPillar>();
        Physics.SyncTransforms();
        yield return null; yield return null;

        Bond bDog = human.GetBond(dog), bKom = human.GetBond(komodo), bBear = seal.GetBond(bear);
        TestProbe.Check("[1210] BondPillar familiariza con CUALQUIER ITarget cercano (crece Anima.bonds)",
                        bDog != null && bKom != null && bDog.value > 0f && bKom.value > 0f, $"bonds={human.bonds.Count}");
        TestProbe.Check("[1211] con un perro (karma +45) el bond arranca alto", bDog != null && bDog.value >= 20f, $"bond perro={bDog?.value:0.#}");
        TestProbe.Check("[1211] con una especie sin relación arranca templado (openness), muy por debajo",
                        bKom != null && bKom.value < 10f && bDog != null && bKom.value < bDog.value, $"bond komodo={bKom?.value:0.#}");
        TestProbe.Check("[1211] la karma NEGATIVA no siembra bond (foca↔oso: solo la cercanía)", bBear == null || bBear.value < 1f,
                        $"bond oso={bBear?.value:0.##}");

        float d0 = bDog != null ? bDog.value : 0f;
        float until = Time.time + 2f;   // BondPillar escanea cada 0,5 s
        while (Time.time < until && (bDog == null || bDog.value <= d0)) yield return null;
        TestProbe.Check("[1210] la cercanía sigue haciendo crecer el bond con el tiempo", bDog != null && bDog.value > d0,
                        $"{d0:0.##}→{bDog?.value:0.##}");
    }

    Anima Composed(string name, string speciesBond, Vector3 pos)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.position = pos;
        _trash.Add(g);
        Anima a = g.AddComponent<Anima>();
        SoulComposition sc = g.AddComponent<SoulComposition>();
        sc.resolveOnStart = false; sc.applyScale = false;
        sc.speciesBonds.Add(new BlendSlot { archetype = speciesBond, domain = 100f });
        return a;
    }

    ITarget Neighbor(string name, string species, Vector3 pos)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.position = pos;
        _trash.Add(g);
        g.AddComponent<ChecklistProbeAnima>().species = species;
        return g.AddComponent<AnimaTarget>();
    }
}
