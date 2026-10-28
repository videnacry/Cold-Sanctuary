using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CONDUCTA DE LAS ESPECIES DE LAS ERAS (docs/era-species-behavior.md §5) sin esperar a la noche ni a un depredador: el
/// test monta cada ESCENARIO a mano (pone la hora, mete al depredador, fuerza el celo) en una losa aparte, y comprueba
/// por `TestProbe` lo que en 2 min de Play casi nunca coincide:
/// - Defensa pasiva: una cochinilla ante una araña elige acorazarse (`Reaction.Hold`), la bola sube su `armadura` y su
///   `Predation.Defense`, y al desenrollarse vuelve a la de antes.
/// - Señal de cortejo: el macho de luciérnaga en celo destella y la hembra en celo lo encuentra (`NearestCaller`) y
///   contesta; la cigarra y el grillo en celo suenan.
/// - Dieta por fase: la luciérnaga adulta no caza; la larva sí (si su `StageProfile` es de metamorfosis completa).
/// - Cría: la cría del escorpión sube al lomo de la madre (`BroodRide`) y la madre se encariña con ella.
/// - Noche: la luciérnaga es nocturna y a mediodía duerme.
/// Lo destruye todo y restaura la hora al terminar.
/// </summary>
public class EraSpeciesTest : MonoBehaviour, ITestUnit
{
    public int Group => 29;
    public bool ParallelSafe => false;
    [Tooltip("Prefabs de las especies que usa el test (los asigna el builder): Woodlouse, Spider, Firefly, Cicada, Cricket, Scorpion.")]
    public GameObject[] prefabs;

    readonly List<GameObject> _spawned = new List<GameObject>();
    static readonly Vector3 Origin = new Vector3(5000f, 0.3f, -5000f);   // lejos de todo (ni agua, ni áreas, ni NavMesh ajeno)

    GameObject PrefabOf(string species)
    {
        if (prefabs != null) foreach (GameObject p in prefabs) if (p != null && p.name == species) return p;
        return null;
    }

    Animal Spawn(string species, Vector3 offset, char stage, char sex)
    {
        GameObject p = PrefabOf(species);
        if (p == null) return null;
        GameObject go = Instantiate(p, Origin + offset, Quaternion.identity);
        go.name = $"{species}_TEST";
        _spawned.Add(go);
        Animal a = go.GetComponent<Animal>();
        if (a != null) { a.lifeStage = stage; a.sex = sex; }   // antes de su Start/Init
        return a;
    }

    static void ForceEstrus(Animal a)
    {
        EstrusState e = a != null ? a.GetComponent<EstrusState>() : null;
        if (e != null) e.estrusFraction = 1f;   // todo el ciclo en celo: el siguiente Update lo pone InEstrus
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Especies de las eras (defensa pasiva, cortejo, dieta por fase, cría a lomos, noche)");
        yield return null;
        if (PrefabOf("Woodlouse") == null) { Debug.Log("[TEST] SKIP · especies de las eras — faltan los prefabs (los asigna el builder)."); TestProbe.End(); yield break; }

        GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);   // suelo propio: que nadie se caiga al vacío
        slab.name = "Losa_EraSpecies_TEST";
        slab.transform.position = Origin - new Vector3(0f, 0.55f, 0f);
        slab.transform.localScale = new Vector3(40f, 1f, 40f);
        _spawned.Add(slab);

        TimeController tc = TimeController.timeController;
        float savedHour = tc != null ? tc.Hour : 12f;
        int savedDay = tc != null ? tc.Day : 0;   // SetHour adelanta el día al «volver atrás»: se restauran los dos con SetClock

        // ── 1. DEFENSA PASIVA: cochinilla ante una araña ──
        Animal louse = Spawn("Woodlouse", Vector3.zero, LifeStage.adult, Sex.female);
        Animal spider = Spawn("Spider", new Vector3(1.5f, 0f, 0f), LifeStage.adult, Sex.female);
        yield return null; yield return null; yield return null;   // Init (auto-add de componentes)
        PassiveDefence pd = louse != null ? louse.GetComponent<PassiveDefence>() : null;
        TestProbe.NotNull("la cochinilla tiene PassiveDefence (auto-add en Init)", pd);
        ThreatResponder tr = louse != null ? louse.GetComponent<ThreatResponder>() : null;
        if (pd != null && tr != null && spider != null)
        {
            Reaction r = tr.Decide(louse, spider.gameObject, 0f, false, 0f);
            TestProbe.Check("ante la araña (más ágil) se hace BOLA en vez de huir", r == Reaction.Hold, r.ToString());
            float arm0 = louse.armadura, def0 = Predation.Defense(louse);
            pd.Begin();
            TestProbe.Check("hecha bola: Holding", pd.Holding);
            TestProbe.Greater("la bola sube su armadura", louse.armadura, arm0 + 0.5f);
            TestProbe.Greater("y su defensa ante depredadores (Predation.Defense)", Predation.Defense(louse), def0);
            TestProbe.Greater("se queda acorazada un rato que sale de sus stats", pd.HoldSeconds(louse), 5f);
            pd.End();
            TestProbe.Near("al desenrollarse vuelve a su armadura", louse.armadura, arm0, 0.001f);
        }

        // ── 2. CORTEJO: destellos de la luciérnaga ──
        Animal male = Spawn("Firefly", new Vector3(-6f, 0f, 4f), LifeStage.adult, Sex.male);
        Animal female = Spawn("Firefly", new Vector3(-3f, 0f, 4f), LifeStage.adult, Sex.female);
        yield return null; yield return null; yield return null;
        ForceEstrus(male); ForceEstrus(female);
        yield return null; yield return null;
        MatingSignal ms = male != null ? male.GetComponent<MatingSignal>() : null;
        TestProbe.NotNull("la luciérnaga tiene MatingSignal (auto-add en Init)", ms);
        if (ms != null && female != null)
        {
            bool flashed = false, answered = false, found = false;
            float t0 = Time.time;
            while (Time.time - t0 < 3f)
            {
                ms.Display();   // lo que hace el hábito «Signal» cada frame
                Light ml = male.GetComponentInChildren<Light>();
                if (ml != null && ml.enabled) flashed = true;
                if (MatingSignal.NearestCaller(female, 40f) == ms) found = true;
                Light fl = female.GetComponentInChildren<Light>();
                if (fl != null && fl.enabled) answered = true;
                yield return null;
            }
            TestProbe.Check("el macho en celo DESTELLA", flashed);
            TestProbe.Check("la hembra en celo lo encuentra (NearestCaller)", found);
            TestProbe.Check("y le CONTESTA con su destello", answered);
            male.sex = Sex.female;
            TestProbe.Check("una hembra no llama (Display = false)", !ms.Display());
            male.sex = Sex.male;
        }

        // ── 3. CANTO: cigarra y grillo ──
        foreach (string sp in new[] { "Cicada", "Cricket" })
        {
            Animal singer = Spawn(sp, new Vector3(sp == "Cicada" ? 6f : 9f, 0f, 4f), LifeStage.adult, Sex.male);
            if (singer == null) continue;
            yield return null; yield return null; yield return null;
            ForceEstrus(singer);
            yield return null; yield return null;
            MatingSignal song = singer.GetComponent<MatingSignal>();
            if (song == null) { TestProbe.Check($"{sp}: tiene MatingSignal", false); continue; }
            song.Display(); yield return null; song.Display(); yield return null;
            AudioSource au = singer.GetComponent<AudioSource>();
            TestProbe.Check($"{sp} en celo CANTA (AudioSource sonando)", au != null && au.isPlaying);
        }

        // ── 4. DIETA POR FASE: solo la larva de luciérnaga caza ──
        if (male != null && male.Forage != null)
        {
            TestProbe.Check("la luciérnaga ADULTA no caza", !male.Forage.HuntsNow(male));
            if (StageProfile.Of("Firefly").development == StageProfile.Development.Complete)
            {
                char st = male.lifeStage;
                male.lifeStage = LifeStage.child;   // misma criatura, fase de larva (se restaura en el mismo frame)
                TestProbe.Check("la LARVA de luciérnaga sí caza (caracoles y babosas)", male.Forage.HuntsNow(male));
                male.lifeStage = st;
            }
            else Debug.Log("[TEST] SKIP · larva de luciérnaga — StageProfile.Firefly aún no es de metamorfosis completa.");
        }

        // ── 5. CRÍA A LOMOS: escorpión ──
        Animal mom = Spawn("Scorpion", new Vector3(0f, 0f, -6f), LifeStage.adult, Sex.female);
        yield return null; yield return null; yield return null;
        Reproduction rep = mom != null ? mom.GetComponent<Reproduction>() : null;
        if (rep != null)
        {
            Animal cub = rep.SpawnOffspring();
            if (cub != null) _spawned.Add(cub.gameObject);
            yield return null; yield return null; yield return null; yield return null;
            BroodRide ride = cub != null ? cub.GetComponent<BroodRide>() : null;
            TestProbe.Check("la cría del escorpión lleva BroodRide hacia su madre", ride != null && ride.enabled && ride.mother == mom);
            TestProbe.Check("va subida al lomo (por encima del pivote de la madre)", cub != null && cub.transform.position.y > mom.transform.position.y + 0.001f,
                            cub != null ? $"Δy {cub.transform.position.y - mom.transform.position.y:0.000}" : "sin cría");
            TestProbe.Check("y no anda por su cuenta (busy)", cub != null && cub.busy);
            Bond b = cub != null ? mom.GetBond(cub) : null;
            TestProbe.Check("la madre se encariña con lo que pare (vínculo)", b != null && b.value > 0f);
        }
        else TestProbe.Check("el escorpión tiene Reproduction", false);

        // ── 6. NOCHE: la luciérnaga duerme de día ──
        SleepCycle sc = female != null ? female.GetComponent<SleepCycle>() : null;
        TestProbe.Check("la luciérnaga es NOCTURNA", SleepCycle.IsNocturnal("Firefly"));
        if (sc != null && tc != null)
        {
            female.aware = false;
            tc.SetClock(savedDay, 12f); sc.Evaluate();
            TestProbe.Check("a mediodía la luciérnaga DUERME", female.asleep);
            tc.SetClock(savedDay, 23f); sc.Evaluate();
            TestProbe.Check("a las 23:00 está despierta", !female.asleep, $"dormida {female.asleep}, muerta {female.death}, consciente {female.aware}, hora {tc.Hour:0.0}, día {tc.IsDay}");
            tc.SetClock(savedDay, savedHour);
        }

        foreach (GameObject g in _spawned) if (g != null) Destroy(g);
        _spawned.Clear();
        TestProbe.End();
    }
}
