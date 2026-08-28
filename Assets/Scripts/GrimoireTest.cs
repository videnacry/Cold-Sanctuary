using System.Collections;
using UnityEngine;

/// <summary>
/// Unidad de test del REPERTORIO de doble vía (docs/capabilities-and-embodiment.md §2), por `TestProbe`. Verifica que
/// un hechizo se puede usar por la vía CORPORAL (anatomía) **o** por la MÁGICA (aprendido en el `Grimoire`), y que el
/// grimorio es universal pero **bloqueado/vacío** hasta `Learn`. Determinista (grimorio standalone + un `Anima` real).
/// </summary>
public class GrimoireTest : MonoBehaviour, ITestUnit
{
    public int Group => 3;              // tras los otros; muta transitoriamente un animal (añade/quita Grimoire)
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Grimoire (repertorio de doble vía)");
        yield return null;

        // Grimorio standalone: arranca vacío, Learn desbloquea.
        GameObject go = new GameObject("Grimoire_test");
        Grimoire g0 = go.AddComponent<Grimoire>();
        TestProbe.Check("grimorio arranca vacío (bloqueado)", !g0.Knows("fly"));
        g0.Learn("fly");
        TestProbe.Check("Learn desbloquea el hechizo (vía mágica)", g0.Knows("fly"));
        Destroy(go);

        // CanUse (doble vía) sobre un Anima REAL de la escena.
        Animal a = null;
        foreach (Animal x in FindObjectsOfType<Animal>()) if (x != null && !x.death) { a = x; break; }
        if (a == null) { Debug.Log("[TEST] SKIP · Grimoire CanUse — sin fauna en escena"); TestProbe.End(); yield break; }

        TestProbe.Check("sin ninguna vía → NO puede", !a.CanUse("nope", false));
        TestProbe.Check("vía CORPORAL (bodyEnabled) → puede", a.CanUse("nope", true));
        TestProbe.Check("sin grimorio → KnowsSpell false (universal, bloqueado)", !a.KnowsSpell("fly"));

        Grimoire g = a.gameObject.AddComponent<Grimoire>();
        g.Learn("fly");
        TestProbe.Check("vía MÁGICA (grimorio) → puede aunque no tenga la parte", a.CanUse("fly", false));
        Destroy(g);                    // transitorio: el animal no conserva el grimorio de prueba

        TestProbe.End();
    }
}
