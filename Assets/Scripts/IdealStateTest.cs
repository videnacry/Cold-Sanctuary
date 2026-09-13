using System.Collections;
using UnityEngine;

/// <summary>
/// Test del ESTADO IDEAL (docs/consciousness-mechanics.md, testing-checklist §46), por `TestProbe`: la composición
/// elemental (`ElementsStatus`) y el nivel de actividad (`ActivityLevel`, ACWR). Funciones puras/deterministas.
/// </summary>
public class IdealStateTest : MonoBehaviour, ITestUnit
{
    public int Group => 13;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Estado ideal (elementos + actividad)");
        yield return null;

        // ── ElementsStatus: ideal ∝ masa; clasificación por ratio ──
        TestProbe.Near("O ideal ≈ 65% de la masa (1 kg → 650 g)", ElementsStatus.IdealGrams("O", 1f), 650f, 1f);
        TestProbe.Check("cantidad ideal → nivel Ideal", ElementsStatus.Classify(650f, 650f) == ElementLevel.Ideal);
        TestProbe.Check("mitad del ideal → Deficiente", ElementsStatus.Classify(200f, 650f) == ElementLevel.Deficiente);
        TestProbe.Check("triple del ideal → Exceso", ElementsStatus.Classify(2000f, 650f) == ElementLevel.Exceso);
        Color ideal = ElementsStatus.ColorOf(ElementLevel.Ideal);
        TestProbe.Check("el nivel Ideal es VERDE (g>r y g>b)", ideal.g > ideal.r && ideal.g > ideal.b);
        TestProbe.Check("formato con unidad (0.0005 g → mg)", ElementsStatus.Format(0.0005f).Contains("mg"));

        // ── ActivityLevel: ACWR (aguda 7d / crónica 28d), zona bendecida 0.8–1.3 ──
        GameObject go = new GameObject("Activity_TEST");
        ActivityLevel act = go.AddComponent<ActivityLevel>();
        for (int d = 0; d < 40; d++) { act.AddLoad(10f); act.RollDay(); }   // 40 días de carga estable
        TestProbe.Check("carga sostenida → zona BENDECIDA (ACWR 0.8–1.3)", act.Zone == ActivityZone.Bendecido);
        for (int d = 0; d < 4; d++) { act.AddLoad(40f); act.RollDay(); }     // pico brusco
        TestProbe.Check("pico brusco → SOBRECARGA (ACWR > 1.3)", act.Zone == ActivityZone.Sobrecarga);
        for (int d = 0; d < 20; d++) { act.RollDay(); }                       // parón largo (carga 0)
        TestProbe.Check("parón largo → DESENTRENADO (ACWR < 0.8)", act.Zone == ActivityZone.Desentrenado);

        Object.Destroy(go);
        TestProbe.End();
    }
}
