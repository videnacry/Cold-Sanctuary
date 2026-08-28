using System.Collections;
using UnityEngine;

/// <summary>
/// Unidad de test del CELO (docs/testing-checklist.md §35), por `TestProbe`: hay una `TraceField` persistente, cada
/// animal recibió un `EstrusState` (auto-add en `Init`), y `Emit()` deja rastro de `Estrus` legible en la rejilla.
/// Determinista (fuerza el depósito con `Emit`, no espera al ciclo de celo).
/// </summary>
public class EstrusTest : MonoBehaviour, ITestUnit
{
    public int Group => 4;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Estrus (celo → rastro)");
        yield return null;

        TestProbe.NotNull("hay TraceField persistente en escena", TraceField.Instance);

        Animal a = null;
        foreach (Animal x in FindObjectsOfType<Animal>()) if (x != null && !x.death) { a = x; break; }
        if (a == null) { Debug.Log("[TEST] SKIP · Estrus — sin fauna en escena"); TestProbe.End(); yield break; }

        EstrusState e = a.GetComponent<EstrusState>();
        TestProbe.NotNull("el animal tiene EstrusState (auto-add en Init)", e);

        if (e != null && TraceField.Instance != null)
        {
            Vector3 p = a.transform.position;
            e.Emit();
            TestProbe.Greater("Emit deja rastro de Estrus en la rejilla", TraceField.Sniff(p, TraceChannel.Estrus), 0f);
        }

        TestProbe.End();
    }
}
