using System.Collections;
using UnityEngine;

/// <summary>
/// Unidad de test del CICLO DE VIDA (docs/testing-checklist.md §36), por `TestProbe`. El ciclo completo (nacer→crecer→
/// reproducir→morir) no se puede esperar en un test (crecer/vejez dura miles de días), así que asevera el MECANISMO
/// nuevo de forma determinista: el **parto** produce una **cría** (etapa `child`) de la **misma especie** que el
/// progenitor — reusando `Reproduction.SpawnOffspring`. La cría de prueba se destruye al terminar.
///
/// (Nacer/crecer/morir por vejez/ser comido ya existían; reproducir era el hueco que cierra este arco.)
/// </summary>
public class LifeCycleAuto : MonoBehaviour, ITestUnit
{
    public int Group => 5;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("LifeCycle (parto → cría)");
        yield return null;

        Animal parent = null;
        foreach (Animal x in FindObjectsOfType<Animal>())
            if (x != null && !x.death && x.GetComponent<Reproduction>() != null) { parent = x; break; }
        if (parent == null) { Debug.Log("[TEST] SKIP · LifeCycle — sin fauna con Reproduction"); TestProbe.End(); yield break; }

        Reproduction repro = parent.GetComponent<Reproduction>();
        Animal baby = repro.SpawnOffspring();
        TestProbe.NotNull("el parto produce una cría", baby);
        if (baby != null)
        {
            TestProbe.Check("la cría arranca en etapa CHILD", baby.lifeStage == LifeStage.child, $"stage={baby.lifeStage}");
            TestProbe.Check("la cría es de la misma especie", baby.SpeciesName == parent.SpeciesName,
                            $"{baby.SpeciesName} vs {parent.SpeciesName}");
            Destroy(baby.gameObject);   // limpieza: no dejar fauna extra tras el test
        }

        TestProbe.End();
    }
}
