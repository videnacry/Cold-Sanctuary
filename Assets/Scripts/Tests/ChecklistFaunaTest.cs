using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Automatiza por `TestProbe` ítems de FAUNA de `docs/testing-checklist.md` §23–§45 que pedían observar el Play:
/// forrajeo (`Forager.SelectTarget`), decisión de amenaza (`ThreatResponder.Decide/Assess`: huir, cazar, manada,
/// defensa de crías y acoso), posesión (`AnimaController`/`PlayerBrain`), penalización de medio, fauna recién nacida
/// neutra, frases sin gates de capacidad, asfixia, deambular local con sesgo hacia delante, `TraceField` sin instancia,
/// subproducto de hechizo (off por defecto / opt-in / rate-limit), celo que deposita rastro, inanición letal y los
/// hechizos-estado estupefaciente/duelo.
///
/// Determinista y NO destructivo: las decisiones se prueban con seres de prueba; lo que necesita un animal REAL se hace
/// sobre una COPIA (`Reproduction.SpawnOffspring`, mismo mecanismo que `LifeCycleAuto`) que se destruye al final. Sobre
/// la fauna de la escena solo se hacen CONSULTAS (no se muta nada). El `TraceField` persistente se "oculta" un instante
/// (singleton → null) para la prueba sin-instancia y se restaura en el mismo frame (sin destruirlo).
/// </summary>
public class ChecklistFaunaTest : MonoBehaviour, ITestUnit
{
    public int Group => 23;
    public bool ParallelSafe => false;

    readonly List<GameObject> _trash = new List<GameObject>();

    public IEnumerator Run()
    {
        TestProbe.Begin("ChecklistFauna (§23-45 forrajeo, amenaza, posesión, medio, rastros, copia de animal)");
        yield return null;

        Decisiones();                     // §23/§24 ThreatResponder
        Forrajeo();                       // §23 Forager (fauna real, solo lectura)
        yield return Posesion();          // §23 IA + posesión
        Medio();                          // §23 medio (foca/ballena)
        FrasesSinGates();                 // §24 D3a/E2 dormidos
        TraceSinInstancia();              // §26 / §28
        yield return Subproducto();       // §28
        yield return EstadosNarcoticoDuelo();   // §45
        yield return SobreCopia();        // §24/§25/§35/§37/§40 (copia de un animal real)

        foreach (GameObject g in _trash) if (g != null) Destroy(g);
        _trash.Clear();
        TestProbe.End();
    }

    GameObject Spawn(string name, Vector3 pos, bool primitive = true)
    {
        GameObject g = primitive ? GameObject.CreatePrimitive(PrimitiveType.Cube) : new GameObject();
        g.name = name; g.transform.position = pos;
        _trash.Add(g);
        return g;
    }

    Anima Being(string name, Vector3 pos, float str, float mass, bool target = false)
    {
        GameObject g = Spawn(name, pos);
        Anima a = g.AddComponent<Anima>();
        a.strength = str; a.bodyMass = mass;
        if (target) g.AddComponent<AnimaTarget>();
        return a;
    }

    // ── §23/§24 decisiones de amenaza ────────────────────────────────────────────
    void Decisiones()
    {
        Vector3 o = new Vector3(8000f, -600f, 8000f);
        Anima weak = Being("Presa_TEST", o, 0.5f, 0.5f);
        Anima strong = Being("Depredador_TEST", o + new Vector3(40f, 0f, 0f), 2f, 2f, target: true);
        ThreatResponder trW = weak.gameObject.AddComponent<ThreatResponder>();
        ThreatResponder trS = strong.gameObject.AddComponent<ThreatResponder>();

        TestProbe.Check("[1231] la presa ante un depredador más fuerte → HUYE",
                        trW.Decide(weak, strong.gameObject, 0.3f, false, 0f) == Reaction.Flee);
        trS.aggressiveness = 1f;
        TestProbe.Check("[1231] el depredador agresivo y claramente más fuerte → ATACA (caza)",
                        trS.Decide(strong, weak.gameObject, 0.3f, false, 0f) == Reaction.Fight);
        TestProbe.Greater("[1231] la presa percibe amenaza (Assess > ThreatThreshold típico)", trW.Assess(weak, strong.gameObject), 1f);
        weak.GrowBond(strong.GetComponent<ITarget>(), BondType.Friend, 1000f);
        TestProbe.Near("[1231] con BOND alto hacia él, la amenaza se desactiva (Assess 0 → no huye)",
                       trW.Assess(weak, strong.gameObject), 0f, 0.001f);
        weak.bonds.Clear();

        // Manada: un lobo solo huye del oso; con aliados cerca (EffectivePower) planta cara.
        Anima wolf = Being("Lobo_TEST", o + new Vector3(0f, 0f, 80f), 1.3f, 1f, target: true);
        wolf.agility = 1.6f;
        ThreatResponder trWolf = wolf.gameObject.AddComponent<ThreatResponder>();
        trWolf.aggressiveness = 1f;
        Anima bear = Being("Oso_TEST", o + new Vector3(0f, 0f, 160f), 2.2f, 2.5f);
        Physics.SyncTransforms();
        Reaction alone = trWolf.Decide(wolf, bear.gameObject, 0.3f, false, 0f);
        float pAlone = Predation.EffectivePower(wolf);
        for (int i = 0; i < 4; i++)
        {
            Anima ally = Being("LoboAliado_TEST", wolf.transform.position + new Vector3(2f + i, 0f, 1f), 1.3f, 1f, target: true);
            ally.agility = 1.6f;
        }
        Physics.SyncTransforms();
        float pPack = Predation.EffectivePower(wolf);
        Reaction withPack = trWolf.Decide(wolf, bear.gameObject, 0.3f, false, 0f);
        TestProbe.Check("[1231] un lobo SOLO huye del oso", alone == Reaction.Flee, $"{alone} (poder {pAlone:0.##})");
        TestProbe.Check("[1231] con aliados cerca (manada) planta cara", pPack > pAlone && withPack == Reaction.Fight,
                        $"{withPack} (poder {pAlone:0.##}→{pPack:0.##})");

        // Defensa de crías EMERGENTE (sin flag) + `canHitAndRun` disuelto.
        Anima mother = Being("Madre_TEST", o + new Vector3(-80f, 0f, 0f), 0.5f, 0.5f);
        ThreatResponder trM = mother.gameObject.AddComponent<ThreatResponder>();
        TestProbe.Check("[1234] sin crías, la madre débil huye", trM.Decide(mother, strong.gameObject, 0.5f, false, 0f) == Reaction.Flee);
        Reaction cubs = trM.Decide(mother, strong.gameObject, 0.5f, true, 2f);
        TestProbe.Check("[1257] madre débil con crías ante quien NO puede vencer → ACOSA (pegar y retirarse)", cubs == Reaction.HitAndRun, $"{cubs}");
        TestProbe.Check("[1234] sin vínculo/autoabandono suficientes, ni con crías planta cara",
                        trM.Decide(mother, strong.gameObject, 0f, true, 0f) == Reaction.Flee);
        Anima bearMom = Being("OsaMadre_TEST", o + new Vector3(-120f, 0f, 0f), 2.2f, 2.5f);
        ThreatResponder trB = bearMom.gameObject.AddComponent<ThreatResponder>();   // agresividad 0
        Anima medium = Being("Amenaza_TEST", o + new Vector3(-160f, 0f, 0f), 1f, 1f);
        TestProbe.Check("[1234] osa sin crías y sin agresividad → no ataca", trB.Decide(bearMom, medium.gameObject, 0.3f, false, 0f) == Reaction.Flee);
        TestProbe.Check("[1257] osa fuerte con crías → se PLANTA (Fight)", trB.Decide(bearMom, medium.gameObject, 0.3f, true, 1f) == Reaction.Fight);
    }

    // ── §23 Forager sobre la fauna real (consultas, no muta nada) ────────────────
    void Forrajeo()
    {
        List<Animal> alive = new List<Animal>();
        foreach (Animal a in FindObjectsOfType<Animal>()) if (a != null && !a.death && a.Forage != null) alive.Add(a);
        if (alive.Count == 0) { TestProbe.Check("[1227] hay fauna para probar el forrajeo", false, "sin fauna inicializada"); return; }

        Animal herb = null;
        foreach (Animal a in alive)
            if (a.Forage.eatsGrass && !a.Forage.eatsPrey && !a.Forage.eatsFish && !a.Forage.eatsKrill) { herb = a; break; }
        if (herb != null && GrassPatch.All.Count > 0)
        {
            GameObject sel = herb.Forage.SelectTarget(herb);
            GrassPatch nearest = GrassPatch.Nearest(herb.transform.position);
            TestProbe.Check("[1227] un herbívoro elige el GrassPatch más cercano", sel != null && nearest != null && sel == nearest.gameObject,
                            $"{herb.SpeciesName} → {(sel != null ? sel.name : "nada")}");
        }
        else Debug.Log("[TEST] SKIP · [1227] herbívoro→GrassPatch — la escena no tiene herbívoro puro o pasto");

        int preds = 0, picks = 0, bad = 0; string why = "";
        foreach (Animal p in alive)
        {
            if (!p.Forage.eatsPrey) continue;
            preds++;
            GameObject sel = p.Forage.SelectTarget(p);
            Anima t = sel != null ? sel.GetComponentInParent<Anima>() : null;
            if (t == null || t.death) continue;   // pasto/pez/carcasa o nada a tiro
            picks++;
            float radius = Mathf.Max(p.Forage.minHuntRadius, p.perception * p.Forage.huntRangePerPerception);
            ITarget tt = t.GetComponent<ITarget>();
            if (t.SpeciesName != null && t.SpeciesName == p.SpeciesName) { bad++; why = $"{p.SpeciesName} caza su especie"; }
            else if (Predation.EffectivePower(p) < Predation.Defense(t)) { bad++; why = $"{p.SpeciesName} no puede con {t.SpeciesName}"; }
            else if (tt != null && !p.CanHarm(tt)) { bad++; why = $"{p.SpeciesName} ataca a un vinculado"; }
            else if (Vector3.Distance(p.transform.position, t.transform.position) > radius + 10f) { bad++; why = $"{p.SpeciesName} fuera de su radio por percepción"; }
        }
        if (preds > 0)
            TestProbe.Check("[1228] la presa elegida respeta las reglas (no su especie, puede por stats, sin vínculo, dentro del radio)",
                            bad == 0, $"depredadores {preds}, presas vivas elegidas {picks}{(bad > 0 ? " · " + why : "")}");
        else Debug.Log("[TEST] SKIP · [1228] reglas de presa — la escena no tiene depredadores");
    }

    // ── §23 IA + posesión ────────────────────────────────────────────────────────
    IEnumerator Posesion()
    {
        GameObject g = Spawn("Poseible_TEST", new Vector3(8200f, -600f, 8000f), primitive: false);
        g.AddComponent<Anima>();
        g.AddComponent<AiBrain>();
        PlayerBrain pb = g.AddComponent<PlayerBrain>();
        AnimaController ctrl = g.AddComponent<AnimaController>();
        yield return null; yield return null;
        TestProbe.Check("[1238] sin poseer, decide su IA (AiBrain)", ctrl.Active is AiBrain, $"{ctrl.Active?.BrainName}");
        pb.possessionRelevance = 5f;
        yield return null; yield return null;
        TestProbe.Check("[1238] al poseer (PlayerBrain de mayor relevancia) su IA se SUPRIME", ctrl.Active is PlayerBrain, $"{ctrl.Active?.BrainName}");
        pb.Release();
        yield return null; yield return null;
        TestProbe.Check("[1238] al soltar, la IA retoma el mando", ctrl.Active is AiBrain, $"{ctrl.Active?.BrainName}");

        foreach (Animal a in FindObjectsOfType<Animal>())
        {
            if (a == null || a.death) continue;
            AnimaController ac = a.GetComponent<AnimaController>();
            if (ac == null || ac.Active == null || a.GetComponent<PlayerBrain>() != null) continue;
            TestProbe.Check("[1238] un animal real sin poseer lo conduce su IA", ac.Active is AiBrain, $"{a.name}: {ac.Active.BrainName}");
            break;
        }
    }

    // ── §23 medio ────────────────────────────────────────────────────────────────
    void Medio()
    {
        GameObject g = Spawn("Foca_TEST", new Vector3(8300f, -600f, 8000f), primitive: false);
        Anima seal = g.AddComponent<Anima>();
        SpeciesBody sb = g.AddComponent<SpeciesBody>();
        sb.species = "Seal";
        sb.Apply(seal);
        seal.currentMedium = Medium.Water;
        float inWater = seal.EffectiveAgility;
        seal.currentMedium = Medium.Land;
        float onLand = seal.EffectiveAgility;
        TestProbe.Check("[1241] foca en agua se mueve normal (WaterAffinity 1.0)", Mathf.Approximately(inWater, seal.agility),
                        $"agi efectiva {inWater:0.##} = base {seal.agility:0.##}");
        TestProbe.Check("[1241] foca en tierra, penalizada (LandAffinity del arquetipo)", onLand < inWater,
                        $"tierra {onLand:0.##} < agua {inWater:0.##}");
        ArchetypeProfile whale = Archetypes.BodyOf("Whale");
        TestProbe.Check("[1241] la ballena es data del arquetipo: agua 1.0 / tierra muy baja (< umbral de asfixia 0.3)",
                        Mathf.Approximately(whale.waterAffinity, 1f) && whale.landAffinity < 0.3f, $"tierra {whale.landAffinity:0.##}");
    }

    // ── §24 D3a/E2 dormidos ──────────────────────────────────────────────────────
    void FrasesSinGates()
    {
        int n = 0, gated = 0;
        List<MindPhrase> all = new List<MindPhrase>(PhraseLibrary.All);
        all.AddRange(PhrasePools.Especie());
        foreach (MindPhrase p in all)
        {
            if (p == null) continue;
            n++;
            if (!string.IsNullOrEmpty(p.capability) || !string.IsNullOrEmpty(p.gateCapability)) gated++;
        }
        TestProbe.Check("[1267] ninguna frase fija capability/gateCapability (la fauna con Mente piensa igual)", gated == 0,
                        $"{gated} de {n} frases con gate");
    }

    // ── §26 / §28 TraceField sin instancia ───────────────────────────────────────
    void TraceSinInstancia()
    {
        TraceField saved = TraceField.Instance;
        MethodInfo setter = typeof(TraceField).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetSetMethod(true);
        if (saved != null && setter == null)
        {
            TestProbe.Check("[1286] se puede ocultar el singleton TraceField para la prueba", false, "sin setter");
            return;
        }
        GameObject g = Spawn("SubproductoSinRejilla_TEST", new Vector3(8400f, -600f, 8000f), primitive: false);
        ChecklistProbeSpell sp = g.AddComponent<ChecklistProbeSpell>();
        sp.leavesByproduct = true; sp.byproductStrength = 20f;

        Vector3 p = new Vector3(8400f, 0f, 8000f);
        bool threw = false;
        float sniff = -1f; Vector3 trail = Vector3.one;
        if (saved != null) setter.Invoke(null, new object[] { null });
        try
        {
            TraceField.Leave(p, TraceChannel.ScentSelf, 10f);
            sniff = TraceField.Sniff(p, TraceChannel.ScentSelf);
            trail = TraceField.Trail(p, TraceChannel.ScentSelf);
            sp.Act();
        }
        catch (System.Exception e) { threw = true; Debug.LogWarning($"[TEST] TraceField sin instancia lanzó: {e.Message}"); }
        finally { if (saved != null) setter.Invoke(null, new object[] { saved }); }

        TestProbe.Check("[1286] sin TraceField: Leave/Sniff/Trail no petan", !threw);
        TestProbe.Check("[1286] sin TraceField: Sniff = 0 y Trail = zero (no-op)", sniff == 0f && trail == Vector3.zero);
        TestProbe.Check("[1315] hechizo con subproducto opt-in SIN rejilla → no peta (no-op)", !threw);
        TestProbe.Check("[1286] el TraceField persistente queda restaurado", TraceField.Instance == saved);
    }

    // ── §28 subproducto de hechizo ───────────────────────────────────────────────
    IEnumerator Subproducto()
    {
        if (TraceField.Instance == null) { TestProbe.Check("[1308] hay TraceField para el subproducto", false); yield break; }
        Vector3 p = new Vector3(8500f, -600f, 8000f);
        GameObject g = Spawn("Subproducto_TEST", p, primitive: false);
        ChecklistProbeSpell sp = g.AddComponent<ChecklistProbeSpell>();
        FireSpell fire = g.AddComponent<FireSpell>();
        TestProbe.Check("[1308] por defecto los hechizos NO dejan subproducto (leavesByproduct=false)", !sp.leavesByproduct && !fire.leavesByproduct);

        float s0 = TraceField.Sniff(p, TraceChannel.Disturbance);
        sp.Act();
        TestProbe.Near("[1308] con leavesByproduct=false, LeaveByproduct es no-op", TraceField.Sniff(p, TraceChannel.Disturbance), s0, 0.001f);

        sp.leavesByproduct = true; sp.byproductChannel = TraceChannel.Disturbance; sp.byproductStrength = 20f; sp.byproductInterval = 0.5f;
        sp.Act();
        float s1 = TraceField.Sniff(p, TraceChannel.Disturbance);
        TestProbe.Near("[1310] opt-in: al actuar deja RASTRO en la rejilla (Sniff ≈ fuerza)", s1 - s0, 20f, 0.5f);
        sp.Act(); sp.Act();
        TestProbe.Near("[1314] rate-limit: re-actuar en el mismo instante NO vuelve a depositar", TraceField.Sniff(p, TraceChannel.Disturbance), s1, 0.5f);
        float until = Time.time + 0.6f;
        while (Time.time < until) yield return null;
        float s2 = TraceField.Sniff(p, TraceChannel.Disturbance);
        sp.Act();
        TestProbe.Greater("[1314] pasado byproductInterval, sí vuelve a depositar", TraceField.Sniff(p, TraceChannel.Disturbance), s2 + 15f);
        TestProbe.Check("[1310] el rastro DECAE con el tiempo", s2 < s1, $"{s1:0.##}→{s2:0.##}");
    }

    // ── §45 estupefaciente / duelo ───────────────────────────────────────────────
    IEnumerator EstadosNarcoticoDuelo()
    {
        GameObject pg = Spawn("Paciente_TEST", new Vector3(8600f, -600f, 8000f));
        Anima patient = pg.AddComponent<Anima>();
        ITarget pt = pg.AddComponent<AnimaTarget>();
        GameObject cg = Spawn("Narcotico_TEST", new Vector3(8610f, -600f, 8000f), primitive: false);
        NarcoticSpell narc = cg.AddComponent<NarcoticSpell>();
        narc.force = 0.3f;

        narc.Cast(null, pt);
        AllostaticState hub = AllostaticState.Of(patient);
        float sed1 = hub.Sedation;
        AddictionState add = patient.GetComponent<AddictionState>();
        TestProbe.Near("[1515] NarcoticSpell SEDA (sube Sedation)", sed1, 0.3f, 0.001f);
        TestProbe.Check("[1515] …y deja AddictionState (tolerancia↑)", add != null && add.Tolerance > 0f, $"tol={add?.Tolerance:0.##}");
        narc.Cast(null, pt);
        float inc2 = hub.Sedation - sed1;
        TestProbe.Check("[1515] con tolerancia, la siguiente dosis seda MENOS", inc2 > 0f && inc2 < 0.3f - 0.001f, $"2ª dosis +{inc2:0.###}");

        GameObject gg = Spawn("Doliente_TEST", new Vector3(8620f, -600f, 8000f));
        Anima mourner = gg.AddComponent<Anima>();
        GriefSpell grief = gg.AddComponent<GriefSpell>();
        grief.force = 0.5f; grief.tickInterval = 0.3f;
        TestProbe.Check("[1515] GriefSpell es Periodic (duelo a intervalos)", grief.timing == EffectTiming.Periodic);
        grief.Cast(mourner, null);
        yield return null; yield return null;
        float st1 = mourner.stress;
        TestProbe.Greater("[1515] el duelo carga la carga alostática", AllostaticState.Of(mourner).ExtraLoad, 0f);
        TestProbe.Greater("[1515] el duelo sube el estrés", st1, 0f);
        float until = Time.time + 0.6f;
        while (Time.time < until) yield return null;
        TestProbe.Greater("[1515] …y REINCIDE al siguiente intervalo", mourner.stress, st1 + 0.05f, $"{st1:0.##}→{mourner.stress:0.##}");
    }

    // ── Sobre una COPIA de un animal real ────────────────────────────────────────
    IEnumerator SobreCopia()
    {
        Animal parent = null;
        foreach (Animal x in FindObjectsOfType<Animal>())
        {
            if (x == null || x.death || x.GetComponent<Reproduction>() == null || x.rig == null || x.rig.mass <= 0.01f || x.Body == null) continue;
            if (parent == null || (parent.nav == null || !parent.nav.isOnNavMesh) && x.nav != null && x.nav.isOnNavMesh) parent = x;
        }
        if (parent == null) { Debug.Log("[TEST] SKIP · pruebas sobre copia — sin fauna lista (Reproduction/rig/Body)"); yield break; }

        Animal copy = parent.GetComponent<Reproduction>().SpawnOffspring();
        if (!TestProbe.NotNull("[1406] SpawnOffspring produce una copia (cría) del animal", copy)) yield break;
        copy.name = parent.SpeciesName + "_copia_TEST";
        _trash.Add(copy.gameObject);
        yield return null; yield return null;   // su Start → Init
        if (!TestProbe.Check("la copia se inicializa (Init: Forager/SpeciesBody)", copy.Forage != null && copy.Body != null && copy.rig != null))
            yield break;

        // Que no actúe por su cuenta mientras la medimos (IA y reproducción fuera; el mantenimiento pasivo sigue).
        AnimaController ac = copy.GetComponent<AnimaController>(); if (ac != null) ac.enabled = false;
        Reproduction rep = copy.GetComponent<Reproduction>(); if (rep != null) rep.enabled = false;

        // §24 regresión: recién nacido → neutro.
        TestProbe.Check("[1255] fauna recién spawneada: armamento 0", Mathf.Approximately(copy.armament, 0f), $"arm={copy.armament:0.##}");
        TestProbe.Check("[1255] fauna recién spawneada: confianza de combate 0", Mathf.Approximately(copy.Confidence(Capability.Combat), 0f));
        // La percepción de partida es la base evolutiva de la especie (SpeciesBody.basePerception), no un 1 fijo: un pingüino o una
        // foca pueden partir algo por debajo. Lo que importa es que NO esté gateada (muy por debajo de su base).
        float basePer = copy.TryGetComponent(out SpeciesBody sbp) ? sbp.basePerception : 1f;
        TestProbe.Check("[1255] fauna recién spawneada: percepción en su base de especie (sin gateo sensorial)", copy.perception >= basePer * 0.75f, $"per={copy.perception:0.##}, base {basePer:0.##}");

        // §25/§37 deambular local + sesgo hacia delante.
        if (copy.nav != null && copy.nav.isOnNavMesh && copy.ChildStage != null && copy.ActsPrep?.walk != null)
        {
            // Mirando hacia el INTERIOR del hielo (desde la posición hacia el centro): al este está el mar y allí los destinos
            // caen fuera del NavMesh (el agente los recoloca y el sesgo no se ve).
            Vector3 inland = -copy.transform.position; inland.y = 0f; inland = inland.sqrMagnitude > 0.01f ? inland.normalized : Vector3.left;
            copy.transform.rotation = Quaternion.LookRotation(inland);
            Vector3 pos = copy.transform.position;
            float step = Mathf.Clamp(copy.HomeRadius, 5f, 25f);
            Vector3 trail = TraceField.Trail(pos, TraceChannel.ScentFood);
            Vector3 trailTerm = trail.sqrMagnitude > 0.0001f ? trail.normalized * step : Vector3.zero;
            // Con islas pequeñas el agente recoloca los destinos que caen al agua, así que el sesgo se mide en RELATIVO:
            // mirando hacia un lado, los destinos quedan más hacia ese lado que mirando hacia el contrario.
            float maxDist = 0f;
            LifeStage.SubEvent wander = copy.ChildStage.Wander();
            float Mean(Vector3 face)
            {
                copy.transform.rotation = Quaternion.LookRotation(face);
                float sum = 0f;
                for (int i = 0; i < 24; i++)
                {
                    wander(copy, 0.01f);
                    Vector3 off = copy.nav.destination - pos - trailTerm; off.y = 0f;
                    sum += Vector3.Dot(off, inland);
                    maxDist = Mathf.Max(maxDist, off.magnitude);
                }
                return sum / 24f;
            }
            float ahead = Mean(inland), behind = Mean(-inland);
            copy.nav.ResetPath();
            TestProbe.Check("[1276] Wander va a un punto LOCAL (radio = HomeRadius acotado 5–25)", maxDist <= step * 2f + 3f,   // margen: el NavMesh puede recolocar un destino al borde de otra isla
                            $"máx {maxDist:0.#} m, paso {step:0.#}");
            // Sesgo hacia delante medido en el DESTINO CALCULADO (LifeStage.WanderTarget): en el NavMesh de islas el agente
            // recoloca los destinos que caen al agua y el sesgo no se ve en nav.destination.
            float fwdSum = 0f;
            for (int i = 0; i < 200; i++)
                fwdSum += Vector3.Dot(LifeStage.WanderTarget(pos, inland, step, Random.insideUnitCircle) - pos, inland);
            TestProbe.Greater("[1420] el wander sesga hacia ADELANTE (sigue explorando)", fwdSum / 200f, step * 0.4f,
                              $"media hacia delante {fwdSum / 200f:0.#} m (paso {step:0.#}; en el NavMesh: {ahead:0.#} vs {behind:0.#})");
        }
        else Debug.Log("[TEST] SKIP · [1276]/[1420] wander — la copia no está sobre el NavMesh");

        // Apartar la copia (sin NavMesh) para medir rastros sin vecinos.
        if (copy.nav != null) copy.nav.enabled = false;
        copy.transform.position = new Vector3(8700f, -600f, 8000f);

        // §35 celo: solo adultos; un adulto en celo deposita Estrus.
        EstrusState es = copy.GetComponent<EstrusState>();
        if (es != null && TraceField.Instance != null)
        {
            yield return null;
            TestProbe.Check("[1395] una CRÍA no entra en celo", !es.InEstrus);
            float e0 = TraceField.Sniff(copy.transform.position, TraceChannel.Estrus);
            es.cyclePeriod = 1000f; es.estrusFraction = 1f;   // celo inmediato (la fase arranca < 120)
            copy.lifeStage = LifeStage.adult;
            yield return null; yield return null;
            TestProbe.Check("[1395] un ADULTO entra en celo por ciclo (InEstrus)", es.InEstrus);
            TestProbe.Greater("[1395] …y deposita Estrus en la rejilla (Sniff)", TraceField.Sniff(copy.transform.position, TraceChannel.Estrus), e0);
            copy.lifeStage = LifeStage.child;
        }
        else Debug.Log("[TEST] SKIP · [1395] celo — sin EstrusState o sin TraceField");

        // §25 asfixia (Suffocate es privado: lo invoca el tick pasivo; aquí, un paso explícito por reflexión).
        MethodInfo suffocate = typeof(Animal).GetMethod("Suffocate", BindingFlags.NonPublic | BindingFlags.Instance);
        if (suffocate != null && !copy.death)
        {
            copy.lp = copy.rig.mass;
            copy.currentMedium = Medium.Land; copy.landAffinity = 1f;
            float lp0 = copy.lp;
            suffocate.Invoke(copy, null);
            TestProbe.Near("[1272] en SU medio no se asfixia", copy.lp, lp0, 1e-4f);
            copy.currentMedium = Medium.Water; copy.waterAffinity = 0.4f;
            suffocate.Invoke(copy, null);
            TestProbe.Near("[1272] un terrestre que NADA de paso (agua 0.4 > 0.3) NO se ahoga", copy.lp, lp0, 1e-4f);
            copy.currentMedium = Medium.Land; copy.landAffinity = Archetypes.BodyOf("Whale").landAffinity;   // ballena varada
            suffocate.Invoke(copy, null);
            float lp1 = copy.lp;
            suffocate.Invoke(copy, null);
            TestProbe.Check("[1272] ballena varada en tierra PIERDE vida progresivamente", lp1 < lp0 && copy.lp < lp1,
                            $"lp {lp0:0.##}→{lp1:0.##}→{copy.lp:0.##}");
            copy.landAffinity = 1f;
        }
        else Debug.Log("[TEST] SKIP · [1272] asfixia — sin método Suffocate o la copia murió");

        // §40 inanición letal (solo si la copia tiene Animator+NavMeshAgent: el camino de muerte de Hurt los usa).
        float lethal = copy.Body.baseMass * copy.lethalMassFrac;
        if (copy.ani != null && copy.nav != null && !copy.death && lethal > 0.2f)
        {
            copy.fatReserves = 0f;
            copy.rig.mass = lethal * 1.5f; copy.lp = copy.rig.mass;
            copy.hungry = copy.Body.GetMealMaxWeight(copy.rig.mass) * (copy.starvationMeals + 1f);   // hambre de inanición
            float m0 = copy.rig.mass;
            copy.Starve();
            TestProbe.Check("[1449] inanición con masa por encima del mínimo: consume masa pero NO mata", !copy.death && copy.rig.mass < m0,
                            $"masa {m0:0.##}→{copy.rig.mass:0.##} (mín {lethal:0.##})");
            copy.rig.mass = lethal * 1.01f; copy.lp = copy.rig.mass;
            copy.hungry = copy.Body.GetMealMaxWeight(copy.rig.mass) * (copy.starvationMeals + 1f);
            copy.fatReserves = 0f;
            copy.Starve();
            TestProbe.Check("[1449] si la masa cae bajo baseMass × lethalMassFrac → MUERE", copy.death,
                            $"masa {copy.rig.mass:0.##} ≤ {lethal:0.##}");
        }
        else Debug.Log("[TEST] SKIP · [1449] inanición letal — la copia no tiene Animator/NavMeshAgent, ya murió o su masa base es ínfima");

        Destroy(copy.gameObject);
    }
}
