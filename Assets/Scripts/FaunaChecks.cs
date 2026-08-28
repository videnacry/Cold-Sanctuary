using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sandbox AUTOEJECUTABLE de CONDUCTA/lógica sobre la ESCENA REAL (NavMesh horneado + fauna de `FamilyGenerator`),
/// reportando por `TestProbe` (`[TEST]` en `Editor.log`). A diferencia de `PheromoneFieldTest` (pura lógica), esto
/// necesita animales reales, así que espera a que hayan hecho `Init` y luego asevera **funciones DETERMINISTAS**
/// (no la emergencia con timing, que sería flaky): el wiring del refactor por componentes, la depredación por stats,
/// el eje de armamento ⟂ masa, la confianza-histórica en 0 al nacer, y el gateo sensorial de `Assess`.
///
/// Mutaciones puntuales (armamento/percepción) se **restauran en el mismo frame** (sin yields de por medio) → no
/// alteran la sim. Se omite (SKIP, sin FAIL) lo que la composición de fauna de la escena no permita comprobar.
/// </summary>
public class FaunaChecks : MonoBehaviour
{
    [Tooltip("Segundos de espera para que FamilyGenerator + Init de los animales asienten antes de asev­erar.")]
    public float settle = 1.5f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(settle);
        TestProbe.Begin("FaunaChecks (escena real)");

        List<Animal> alive = new List<Animal>();
        foreach (Animal a in FindObjectsOfType<Animal>())
            if (a != null && !a.death) alive.Add(a);
        if (!TestProbe.Greater("hay fauna viva", alive.Count, 0)) { TestProbe.End(); yield break; }

        // ── Wiring del refactor por componentes (#108–#126) ──────────────────────
        Animal s = alive[0];
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

        TestProbe.End();
    }
}
