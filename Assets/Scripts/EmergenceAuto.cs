using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sandbox AUTOEJECUTABLE del BUCLE EMERGENTE de temperamento (docs/capabilities-and-embodiment.md §4), reportando por
/// `TestProbe`. A diferencia de `FaunaChecks` (una aserción por frame), esto ejercita el **multi-paso que cambia una
/// DECISIÓN**: productor (`RecordUse` sube la confianza) → consumidor (con la agresividad innata a 0, `Decide` frente a
/// una presa claramente más débil pasa de **Huir** a **Pelear** cuando la confianza sube) → decaimiento (`DecayConfidence`).
///
/// Conducido de forma **determinista** (no espera a una caza real, que sería flaky y mataría fauna): muta confianza/
/// agresividad y **restaura en el mismo frame** (sin yields) → no altera la sim. Omite (SKIP, sin FAIL) si la escena no
/// tiene un par depredador/presa con ventaja de poder suficiente.
/// </summary>
public class EmergenceAuto : MonoBehaviour
{
    public float settle = 1.6f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(settle);
        TestProbe.Begin("EmergenceAuto (bucle de temperamento)");

        List<Animal> alive = new List<Animal>();
        foreach (Animal a in FindObjectsOfType<Animal>())
            if (a != null && !a.death) alive.Add(a);

        // Depredador más fuerte + presa más blanda (mismo criterio que FaunaChecks).
        Animal pred = null; float bestPow = -1f;
        Animal prey = null; float bestDef = float.MaxValue;
        foreach (Animal a in alive)
        {
            if (a.Forage == null) continue;
            if (a.Forage.eatsPrey)  { float p = Predation.PredatorPower(a); if (p > bestPow) { bestPow = p; pred = a; } }
            if (a.Forage.eatsGrass) { float d = Predation.Defense(a);       if (d < bestDef) { bestDef = d; prey = a; } }
        }
        ThreatResponder tr = pred != null ? pred.GetComponent<ThreatResponder>() : null;
        if (pred == null || prey == null || tr == null)
        {
            Debug.Log("[TEST] SKIP · EmergenceAuto — sin par depredador/presa con ThreatResponder → omitido");
            TestProbe.End(); yield break;
        }

        // 1) PRODUCTOR: RecordUse(éxito) sube la confianza de combate.
        float baseline = pred.Confidence(Capability.Combat);
        for (int i = 0; i < 10; i++) pred.RecordUse(Capability.Combat, true);
        TestProbe.Greater("RecordUse(éxito) sube la confianza", pred.Confidence(Capability.Combat), baseline);

        // 2) CONSUMIDOR → CONDUCTA: con agresividad innata 0, la confianza es lo que decide Pelear.
        //    Requiere que el depredador supere el margen de poder de la presa (si no, no aplica → SKIP este check).
        bool canOverpower = Predation.EffectivePower(pred) > Predation.EffectivePower(prey) * tr.fightPowerMargin;
        if (canOverpower)
        {
            float savedAggr = tr.aggressiveness;
            tr.aggressiveness = 0f;

            pred.spellConfidence[Capability.Combat] = 0f;
            Reaction timid = tr.Decide(pred, prey.gameObject, 0f, false, 0f);

            pred.spellConfidence[Capability.Combat] = 80f;
            Reaction bold = tr.Decide(pred, prey.gameObject, 0f, false, 0f);

            tr.aggressiveness = savedAggr;                       // restaurar (mismo frame, sin yields)

            TestProbe.Check("sin confianza (agr=0) NO ataca", timid != Reaction.Fight, $"reacción={timid}");
            TestProbe.Check("con confianza alta SÍ ataca (temperamento histórico)", bold == Reaction.Fight, $"reacción={bold}");
        }
        else Debug.Log("[TEST] SKIP · flip de conducta — el depredador no supera el margen de poder de la presa en la escena");

        // 3) DECAIMIENTO: la maestría se enfría (DecayConfidence).
        pred.spellConfidence[Capability.Combat] = 30f;
        pred.DecayConfidence(10f);
        TestProbe.Near("DecayConfidence baja la confianza", pred.Confidence(Capability.Combat), 20f, 0.001f);

        // Restaurar el estado original del depredador (no dejar la sim con confianza inyectada).
        pred.spellConfidence[Capability.Combat] = baseline;

        TestProbe.End();
    }
}
