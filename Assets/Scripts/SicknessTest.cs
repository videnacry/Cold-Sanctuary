using System.Collections;
using UnityEngine;

/// <summary>
/// Unidad de test de la ENFERMEDAD (docs/testing-checklist.md §38), por `TestProbe`, determinista: `MakeSick` sube
/// `Anima.sickness`, `Predation.PredatorPower`/`Defense` bajan (presa fácil), y se deposita el canal `Sickness`. `Heal`
/// (enfermería) lo revierte. Muta/restaura la salud de un animal en el mismo bloque → sin efecto en la sim.
/// </summary>
public class SicknessTest : MonoBehaviour, ITestUnit
{
    public int Group => 6;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Sickness (enfermedad → presa fácil)");
        yield return null;

        Animal a = null;
        foreach (Animal x in FindObjectsOfType<Animal>())
            if (x != null && !x.death && x.GetComponent<SicknessState>() != null) { a = x; break; }
        if (a == null) { Debug.Log("[TEST] SKIP · Sickness — sin fauna con SicknessState"); TestProbe.End(); yield break; }

        SicknessState s = a.GetComponent<SicknessState>();
        float pow0 = Predation.PredatorPower(a);
        float def0 = Predation.Defense(a);

        s.MakeSick(0.5f);
        TestProbe.Check("MakeSick → enfermo", s.IsSick, $"sickness={a.sickness:0.##}");
        TestProbe.Check("enfermo baja el poder depredador", Predation.PredatorPower(a) < pow0);
        TestProbe.Check("enfermo baja la defensa (presa fácil)", Predation.Defense(a) < def0);
        if (TraceField.Instance != null)
        {
            TraceField.Leave(a.transform.position, TraceChannel.Sickness, 6f);
            TestProbe.Greater("deja rastro Sickness", TraceField.Sniff(a.transform.position, TraceChannel.Sickness), 0f);
        }

        s.Heal(1f);   // cura (enfermería): revertir
        TestProbe.Check("Heal → sano de nuevo", !s.IsSick, $"sickness={a.sickness:0.##}");

        TestProbe.End();
    }
}
