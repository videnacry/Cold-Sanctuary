using System.Collections;
using UnityEngine;

/// <summary>
/// Unidad de test del YOGA (docs/testing-checklist.md; stats-as-truth §Yoga), por `TestProbe`. El yoga es central: cada
/// asana es un <see cref="PostureSpell"/> que rota un hueso del <see cref="CreatureRig"/>; el Saludo al Sol
/// (<see cref="SunSalutation"/>) encadena 12. Reusado por superhechizos (buscarComida encadena posturas). Este test crea
/// un cuerpo de cubos (bípedo) con su rig, le encola una postura por la <see cref="ActionQueue"/> y asevera que el hueso
/// REALMENTE rota (la postura se aplica), y que la secuencia del Saludo al Sol trae sus 12 asanas. Se limpia al terminar.
/// </summary>
public class YogaTest : MonoBehaviour, ITestUnit
{
    public int Group => 15;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Yoga (postura rota el rig + Saludo al Sol)");
        yield return null;

        // Estructura: 12 posturas, cada una por respiraciones (1 fase para llegar + 2 de mantenimiento = 3 fases).
        var seq = SunSalutation.Sequence(0.2f);
        TestProbe.Check("el Saludo al Sol son 12 posturas", SunSalutation.PostureCount == 12, $"posturas={SunSalutation.PostureCount}");
        TestProbe.Check("la secuencia son 36 fases de respiración (12×3)", seq.Count == 36, $"count={seq.Count}");

        // Cuerpo de cubos bípedo + su rig (CreatureRig con huesos por BodyPart).
        GameObject subject = new GameObject("YogaTest_Subject");
        subject.transform.position = new Vector3(-30f, 0f, -30f);   // lejos de la fauna real
        subject.AddComponent<Anima>();
        CubeBody.Build(subject, "human");                           // plan bípedo → tiene Chest/Hips/Shoulders/etc.
        CreatureRig rig = subject.GetComponent<CreatureRig>();
        Transform back = rig != null ? rig.Get(BodyPart.Chest) : null;
        TestProbe.NotNull("el cuerpo tiene hueso Chest en el rig", back);

        if (back != null)
        {
            Quaternion before = back.localRotation;
            ActionQueue q = subject.AddComponent<ActionQueue>();
            q.EnqueuePlayer(new PostureSpell { part = BodyPart.Chest, localEuler = new Vector3(40f, 0f, 0f), hold = 0.3f, lerp = 12f });

            float t = 0f;
            while (t < 1.2f) { t += Time.deltaTime; yield return null; }   // deja que la ActionQueue haga tick y el slerp avance

            float moved = Quaternion.Angle(before, back.localRotation);
            TestProbe.Check("la postura ROTA el hueso Chest (>5°)", moved > 5f, $"Δ={moved:0.0}°");
        }

        if (subject != null) Destroy(subject);   // limpieza
        TestProbe.End();
    }
}
