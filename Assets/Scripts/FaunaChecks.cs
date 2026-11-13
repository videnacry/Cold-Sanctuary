using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sandbox AUTOEJECUTABLE de CONDUCTA/lógica sobre la ESCENA REAL (NavMesh horneado + fauna de `FamilyGenerator`),
/// reportando por `TestProbe` (`[TEST]` en `Editor.log`). A diferencia de `TraceFieldTest` (pura lógica), esto
/// necesita animales reales, así que espera a que hayan hecho `Init` y luego asevera **funciones DETERMINISTAS**
/// (no la emergencia con timing, que sería flaky): el wiring del refactor por componentes, la depredación por stats,
/// el eje de armamento ⟂ masa, la confianza-histórica en 0 al nacer, y el gateo sensorial de `Assess`.
///
/// Mutaciones puntuales (armamento/percepción) se **restauran en el mismo frame** (sin yields de por medio) → no
/// alteran la sim. Se omite (SKIP, sin FAIL) lo que la composición de fauna de la escena no permita comprobar.
/// </summary>
public class FaunaChecks : MonoBehaviour, ITestUnit
{
    public int Group => 1;              // sobre la fauna real (tras la rejilla)
    public bool ParallelSafe => false;  // muta/restaura campos de la faura → serie

    public IEnumerator Run()
    {
        TestProbe.Begin("FaunaChecks (escena real)");
        yield return null;

        List<Animal> alive = new List<Animal>();
        foreach (Animal a in FindObjectsOfType<Animal>())
            if (a != null && !a.death) alive.Add(a);
        if (!TestProbe.Greater("hay fauna viva", alive.Count, 0)) { TestProbe.End(); yield break; }

        // ── Wiring del refactor por componentes (#108–#126) ──────────────────────
        // Un herbívoro: los carnívoros ya pueden haber cazado antes de la prueba (con el día de 1 h el tiempo corre 24×).
        Animal s = alive.Find(x => x.Forage != null && !x.Forage.eatsPrey) ?? alive[0];
        TestProbe.NotNull("ThreatResponder presente", s.GetComponent<ThreatResponder>());
        TestProbe.NotNull("Locomotion presente", s.GetComponent<Locomotion>());
        TestProbe.NotNull("Forager presente", s.GetComponent<Forager>());
        TestProbe.NotNull("SpeciesBody presente", s.GetComponent<SpeciesBody>());
        TestProbe.NotNull("AiBrain presente", s.GetComponent<AiBrain>());
        TestProbe.NotNull("AnimaController presente", s.GetComponent<AnimaController>());
        TestProbe.Greater("SpeciesBody sembró fuerza (no default 0)", s.strength, 0f);

        // ── Eje de armamento ⟂ masa (#130) ──────────────────────────────────────
        float p0 = Predation.PredatorPower(s);
        s.armament += 5f;
        float p1 = Predation.PredatorPower(s);
        s.armament -= 5f;                                   // restaurar (mismo frame)
        TestProbe.Greater("armamento sube el poder depredador", p1, p0, "arma ⟂ masa");

        // ── Confianza histórica: 0 al nacer (#131/#132) ─────────────────────────
        TestProbe.Check("confianza de combate arranca en 0", Mathf.Approximately(s.Confidence(Capability.Combat), 0f),
                        $"conf={s.Confidence(Capability.Combat):0.##}");

        // ── Depredación por stats: elegir el depredador más fuerte y la presa más blanda ──
        Animal pred = null; float bestPow = -1f;
        Animal prey = null; float bestDef = float.MaxValue;
        foreach (Animal a in alive)
        {
            if (a.Forage == null) continue;
            if (a.Forage.eatsPrey)  { float p = Predation.PredatorPower(a); if (p > bestPow) { bestPow = p; pred = a; } }
            if (a.Forage.eatsGrass) { float d = Predation.Defense(a);       if (d < bestDef) { bestDef = d; prey = a; } }
        }

        if (pred != null && prey != null)
        {
            TestProbe.Check("el depredador PUEDE con la presa (stats)", Predation.CanHunt(pred, prey),
                            $"{pred.SpeciesName} → {prey.SpeciesName}");
            TestProbe.Check("la presa NO puede cazar al depredador", !Predation.CanHunt(prey, pred),
                            $"{prey.SpeciesName} ↛ {pred.SpeciesName}");

            // Assess gateado por sentidos (#129): menos percepción → menos amenaza percibida.
            ThreatResponder tr = prey.GetComponent<ThreatResponder>();
            if (tr != null)
            {
                float full = tr.Assess(prey, pred.gameObject);
                float savedPerc = prey.perception;
                prey.perception = 0.1f;                      // ciego-ish
                float dim = tr.Assess(prey, pred.gameObject);
                prey.perception = savedPerc;                 // restaurar (mismo frame)
                TestProbe.Check("Assess gateado por sentidos (menos percepción → menos amenaza)", dim < full,
                                $"full={full:0.##} dim={dim:0.##}");
            }
        }
        else Debug.Log("[TEST] SKIP · par depredador/presa — la escena no tiene ambos → omitido (sin FAIL)");

        // ── (2026-11-11) UN VIAJE SIN CAMINO NO HA LLEGADO ──────────────────────────────────────────────────────────────
        // Vivo (a41fefc): 333 «[Fauna] atascado … va=False/True … empezados=136 · camino False … destino a 0,0 m»: el agente se quedaba sin
        // camino justo tras empezar el paseo y Going lo daba por LLEGADO (sin camino, lo que queda = 0): empezaba otro, y otro, sin moverse.
        Animal w = alive.Find(x => x.nav != null && x.nav.enabled && x.nav.isOnNavMesh && x.GetComponent<MarineLife>() == null && !x.busy);
        Vector3 goal = Vector3.zero; bool found = false;
        if (w != null) for (int i = 0; i < 12 && !found; i++) { Vector3 p = w.transform.position + Quaternion.Euler(0f, i * 30f, 0f) * Vector3.forward * 6f; found = NavReach.Reachable(w, p, out goal); }
        if (w != null && found)
        {
            bool going = DesireCatalog.GoingWithoutPathForTest(w, goal);
            TestProbe.Check("un viaje que se queda sin camino a 6 m de su meta no cuenta como llegado", going, w.name);
            TestProbe.Check("…y vuelve a pedir su camino", w.nav.pathPending || w.nav.hasPath || Vector3.Distance(w.nav.destination, goal) < 0.6f, DesireCatalog.NavInfo(w));
            w.nav.ResetPath();
        }
        // (2026-11-11) UN SOLO DUEÑO: cuando gana otro deseo, el viaje del anterior se acaba (y el hábito suelta al animal); el hábito y el
        // ciclo de vida no tocan a quien lleva un viaje de otro deseo. NavTrace (vivo57): «Habit.Halt» ×31, «Habit.Move» ×7, «LifeStage.Feed» ×3
        // y «Forager: se para a pescar» cambiaban el camino de un paseo en marcha; el paseo se quedaba sin camino y esperaba.
        if (w != null && found)
        {
            DesireCatalog.StartTripForTest(w, "explore", goal);
            TestProbe.Check("un paseo recién empezado es un viaje en marcha", DesireCatalog.LiveTrip(w, out string k0) && k0 == "explore");
            TestProbe.Check("el hábito cede ante un viaje de otro deseo", Habit.MustYield(w));
            // (2026-11-13, vivo77) La pausa de OTRA conducta (Forager comiendo, PostNatalManager, un hábito) no le quita la velocidad a un viaje en
            // marcha: «Idle» ponía nav.speed = 0 y el animal quedaba con camino y «pedida 0,0» (osos, pingüinos, crías).
            w.Loco?.Idle(1f);
            TestProbe.Check("la pausa de otra conducta no deja sin velocidad a un viaje en marcha", w.nav.speed > 0.05f && DesireCatalog.LiveTrip(w, out _), $"velocidad pedida {w.nav.speed:0.0}");
            DesireCatalog.Handover(w, "eat");
            TestProbe.Check("si gana otro deseo, el paseo deja de ser suyo (sin viaje en marcha)", !DesireCatalog.LiveTrip(w, out _));
            w.nav.ResetPath();
            // (2026-11-13, ronda 88) EL VIAJE SE VIGILA AUNQUE EL DESEO NO SE DESPACHE: con el agente apagado, el vigilante lo da por acabado.
            DesireCatalog.StartTripForTest(w, "explore", goal);
            Vector3 wp = w.transform.position; w.nav.enabled = false;
            DesireCatalog.SuperviseTripsNow();
            TestProbe.Check("el vigilante de viajes acaba el viaje de un animal sin agente (sin esperar a su deseo)", !DesireCatalog.LiveTrip(w, out _));
            w.nav.enabled = true; if (w.nav.isOnNavMesh) w.nav.Warp(wp);
        }
        else Debug.Log("[TEST] SKIP · viaje sin camino — ningún animal sobre el NavMesh con un sitio alcanzable a 6 m");

        // ── (2026-11-13) QUIETO CON CAMINO (vivo75: 17 «atascado > 20 s» con camino, velocidad pedida 2–5 y velocidad 0,00) ────────────────────
        // A los 1,5 s quieto con camino se reinicia el agente (1); si vuelve a quedarse quieto, se deja el viaje (2). Andar o no querer andar, nada.
        string fz = DesireCatalog.FrozenStepsForTest((true, 0f, 0f), (true, 0f, 1f), (true, 0f, 1.6f), (true, 0f, 2f), (true, 0f, 3.6f));
        TestProbe.Check("quieto con camino: a los 1,5 s se reinicia el agente y, si sigue quieto, se deja el viaje", fz == "0,0,1,0,2", fz);
        string fm = DesireCatalog.FrozenStepsForTest((true, 0f, 0f), (true, 1f, 1f), (true, 0f, 2f), (true, 0f, 3f), (false, 0f, 9f));
        TestProbe.Check("…andando (o sin querer andar) no cuenta como quieto", fm == "0,0,0,0,0", fm);
        // (2026-11-13, ronda 78) CASA LEJOS, POR TRAMOS: el punto a 35 m a lo largo del camino (0,0,0) → (10,0,0) → (10,0,30) es (10,0,25); a 50 m no hay.
        bool along = DesireCatalog.PointAlong(new[] { Vector3.zero, new Vector3(10f, 0f, 0f), new Vector3(10f, 0f, 30f) }, DesireCatalog.HomeLeg, out Vector3 legAt);
        bool beyond = DesireCatalog.PointAlong(new[] { Vector3.zero, new Vector3(10f, 0f, 0f), new Vector3(10f, 0f, 30f) }, 50f, out _);
        TestProbe.Check("el tramo de vuelta a casa cae a 35 m por el camino (y no más allá de su final)", along && Vector3.Distance(legAt, new Vector3(10f, 0f, 25f)) < 0.01f && !beyond, $"{legAt}");
        // (2026-11-13, ronda 78) EXPLORAR UN SITIO PEQUEÑO NO ES DAR VUELTAS: metas distintas y alcanzadas en la ventana sí son explorar; un salto
        // fallido o una meta repetida (a < 1 m de otra) siguen siendo «dar vueltas», y sin ningún salto acabado tampoco se le disculpa.
        Vector3 g1 = new Vector3(0f, 0f, 0f), g2 = new Vector3(4f, 0f, 0f), g3 = new Vector3(4f, 0f, 4f);
        TestProbe.Check("explorar: tres metas distintas y alcanzadas en 20 s no es dar vueltas",
            DesireCatalog.FreshExploring(new[] { (90f, g1, true), (95f, g2, true), (99f, g3, true) }, 100f, 20f));
        TestProbe.Check("…un salto fallido en la ventana sí cuenta",
            !DesireCatalog.FreshExploring(new[] { (90f, g1, true), (95f, g2, false), (99f, g3, true) }, 100f, 20f));
        TestProbe.Check("…una meta repetida (ir y volver al mismo sitio) sí cuenta",
            !DesireCatalog.FreshExploring(new[] { (90f, g1, true), (95f, g2, true), (99f, g1 + new Vector3(0.5f, 0f, 0f), true) }, 100f, 20f));
        TestProbe.Check("…sin ningún salto acabado en la ventana, no se le disculpa",
            !DesireCatalog.FreshExploring(new (float, Vector3, bool)[0], 100f, 20f));
        TestProbe.Check("…lo de antes de la ventana no cuenta (un fallo y la misma meta hace 30 s)",
            DesireCatalog.FreshExploring(new[] { (60f, g2, false), (70f, g3, true), (90f, g1, true), (99f, g3, true) }, 100f, 20f));
        // Un paseo sin ningún salto posible se para y mira (descanso, como la cría del recinto) y el siguiente salto sale hacia otro lado.
        Animal sw = alive.Find(x => x.nav != null && x.nav.enabled && x.nav.isOnNavMesh && x.GetComponent<MarineLife>() == null && !x.busy);
        if (sw != null)
        {
            bool rests = DesireCatalog.StrollWithoutHopForTest(sw, out float turned);
            TestProbe.Check("un paseo sin salto posible descansa a propósito (no está atascado)", rests, sw.name);
            TestProbe.Check("…y prueba otra dirección (gira ≥ 90°)", turned >= 89f, $"{turned:0} °");
        }

        // ── (2026-11-11, ronda 72) EL PASEO DE UN ADULTO VA A UN SITIO DE VERDAD: a ≥ 3 m y con camino completo ──
        Animal ex = alive.Find(x => x != null && x.lifeStage == LifeStage.adult && x.nav != null && x.nav.isOnNavMesh && x.GetComponent<MarineLife>() == null && !DesireCatalog.Confined(x));
        if (ex != null)
        {
            int ok = 0, tries = 0; float worst = float.MaxValue;
            for (int k = 0; k < 10; k++) { if (!DesireCatalog.ExploreGoalForTest(ex, out Vector3 g)) continue; tries++; float d = Vector3.Distance(new Vector3(g.x, 0f, g.z), new Vector3(ex.transform.position.x, 0f, ex.transform.position.z)); worst = Mathf.Min(worst, d); if (d >= 3f - 1e-3f) ok++; }
            TestProbe.Check("el paseo de un adulto elige sitios a ≥ 3 m (no a su lado)", tries == 0 || ok == tries, $"{ex.name}: {ok}/{tries}, el más cerca a {(tries > 0 ? worst.ToString("0.0") : "—")} m");
        }

        // ── (2026-11-11) LAS CRÍAS DEL RECINTO VAN A UN SITIO DE VERDAD ──
        // Vivo61: Polluelo_1/2/3 y Osezno_2 con la meta del paseo en su propio sitio, 1 800 s sin moverse.
        var penned = alive.FindAll(x => x != null && DesireCatalog.Confined(x) && x.nav != null && x.nav.isOnNavMesh);
        int withGoal = 0; string near = "";
        foreach (Animal c in penned)
        {
            if (!DesireCatalog.PenGoal(c, out Vector3 g)) continue;
            withGoal++;
            float d = Vector3.Distance(new Vector3(g.x, 0f, g.z), new Vector3(c.transform.position.x, 0f, c.transform.position.z));
            if (d < 1.5f) near += $"{c.name} {d:0.0} m; ";
        }
        if (penned.Count > 0) TestProbe.Check("el paseo de una cría del recinto va a un sitio a ≥ 1,5 m de ella", near.Length == 0, $"{withGoal}/{penned.Count} con sitio{(near.Length > 0 ? "; demasiado cerca: " + near : "")}");
        else Debug.Log("[TEST] SKIP · paseo del recinto — no hay crías en un recinto sobre el NavMesh");

        // ── (2026-11-13, ronda 82) LAS CRÍAS DEL RECINTO ANDAN SIN NAVMESH (PenWalker): el recinto gira con el árbol ──
        // El sitio: dentro del suelo con 0,6 m a los muros y fuera de lo que abulta (la guarida, las rocas).
        var den = new List<Rect> { Rect.MinMaxRect(-3f, -2.2f, -0.6f, -1f) };
        Vector2 half = new Vector2(3.5f, 3f);
        TestProbe.Check("recinto: el centro del suelo vale", PenWalker.GoalOk(Vector2.zero, half, den, 0.25f));
        TestProbe.Check("recinto: pegado al muro no vale (0,6 m de margen)", !PenWalker.GoalOk(new Vector2(3.2f, 0f), half, den, 0.25f));
        TestProbe.Check("recinto: dentro de la guarida no vale", !PenWalker.GoalOk(new Vector2(-1.8f, -1.6f), half, den, 0.25f));
        TestProbe.Check("recinto: un tramo que atraviesa la guarida se descarta", PenWalker.Crosses(new Vector2(-2f, 0f), new Vector2(-2f, -2.5f), den, 0.25f) && !PenWalker.Crosses(new Vector2(0f, 0f), new Vector2(2f, 1f), den, 0.25f));
        int walkers = 0, picked = 0; string bad = "";
        foreach (Animal c in alive.FindAll(x => x != null && x.GetComponentInParent<CriaFeeder>() != null))
        {
            PenWalker pw = PenWalker.For(c);
            if (pw == null) continue;
            walkers++;
            if (!pw.PickGoal()) continue;
            picked++;
            Vector3 g = pw.GoalLocalForTest, here = c.transform.localPosition;
            if (!PenWalker.GoalOk(new Vector2(g.x, g.z), pw.HalfForTest, pw.BlocksForTest, 0.2f) || new Vector2(g.x - here.x, g.z - here.z).magnitude < 0.8f) bad += $"{c.name} ({g.x:0.0},{g.z:0.0}); ";
            if (c.nav != null && c.nav.enabled) bad += $"{c.name} con el agente encendido; ";
            if (PenWalker.Pausing(c)) bad += $"{c.name} «en pausa» mientras anda; ";   // (ronda 83) andando no es descanso…
            pw.Stop();
            if (!PenWalker.Pausing(c)) bad += $"{c.name} parado sin contar como pausa; ";   // …y parado entre dos paseos, sí
        }
        if (walkers > 0) TestProbe.Check("las crías del recinto andan sin NavMesh, a un sitio de su suelo (dentro, lejos de muros y de la guarida)", picked > 0 && bad.Length == 0, $"{picked}/{walkers} con sitio{(bad.Length > 0 ? "; mal: " + bad : "")}");
        else Debug.Log("[TEST] SKIP · andador del recinto — no hay crías en un recinto de la Cría");

        TestProbe.End();
    }
}
