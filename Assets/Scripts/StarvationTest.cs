using System.Collections;
using UnityEngine;

/// <summary>
/// Unidad de test de la INANICIÓN (docs/testing-checklist.md §40), por `TestProbe`, determinista y NO destructiva:
/// fuerza un tick de inanición sin grasa → la masa baja (el cuerpo se consume → debilita) y el hambre prolongada
/// **enferma** (dispara `SicknessState`). Restaura hambre/grasa/masa/salud en el mismo bloque → sin efecto en la sim.
/// </summary>
public class StarvationTest : MonoBehaviour, ITestUnit
{
    public int Group => 7;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Starvation (inanición)");
        yield return null;

        Animal a = null;
        foreach (Animal x in FindObjectsOfType<Animal>())
            if (x != null && !x.death && x.rig != null && x.Body != null) { a = x; break; }
        if (a == null) { Debug.Log("[TEST] SKIP · Starvation — sin fauna lista (rig/Body)"); TestProbe.End(); yield break; }

        float sHungry = a.hungry, sFat = a.fatReserves, sMass = a.rig.mass, sSick = a.sickness;
        float mealMax = a.Body.GetMealMaxWeight(a.rig.mass);

        a.hungry = mealMax * (a.starvationMeals + 1f);   // hambre de nivel inanición
        a.fatReserves = 0f;                               // sin despensa → se consume el cuerpo
        float mass0 = a.rig.mass;
        a.Starve();

        TestProbe.Check("sin grasa, la inanición consume MASA", a.rig.mass < mass0, $"{mass0:0.##} -> {a.rig.mass:0.##}");
        TestProbe.Greater("la inanición prolongada ENFERMA (dispara SicknessState)", a.sickness, 0f);

        a.hungry = sHungry; a.fatReserves = sFat; a.rig.mass = sMass; a.sickness = sSick;   // restaurar (no dejarlo flaco/enfermo)

        TestProbe.End();
    }
}
