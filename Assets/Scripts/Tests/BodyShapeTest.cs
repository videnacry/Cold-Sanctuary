using System.Collections;
using UnityEngine;

/// <summary>
/// FORMA DEL CUERPO DESDE LAS STATS (docs/body-shape-from-stats.md): alto+fuerte+ágil → más alto y piernas más gruesas;
/// fuerte sin agilidad → brazos más gruesos que piernas; pesado y débil → tronco más ancho (grasa) que el atlético.
/// </summary>
public class BodyShapeTest : MonoBehaviour, ITestUnit
{
    public int Group => 26;
    public bool ParallelSafe => false;

    static BodyShape Make(string n, float str, float mass, float agi, float end)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        g.name = n;
        g.transform.position = new Vector3(0f, -200f, 0f);   // lejos de todo
        Anima a = g.AddComponent<Anima>();
        a.strength = str; a.bodyMass = mass; a.agility = agi; a.endurance = end;
        CubeHumanoid.Dress(g, Color.gray, new Color(0.86f, 0.7f, 0.58f));
        BodyShape s = g.GetComponent<BodyShape>() ?? g.AddComponent<BodyShape>();
        s.Apply(true);
        return s;
    }

    static float Girth(BodyShape s, BodyPart p)
    {
        foreach (CubeJoint j in s.GetComponentsInChildren<CubeJoint>()) if (j.part == p) return j.Girth.x;
        return 1f;
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Forma del cuerpo (stats físicas → estatura, músculo, grasa)");
        yield return null;
        BodyShape avg = Make("Medio_TEST", 1f, 1f, 1f, 1f);
        BodyShape athlete = Make("AltoFuerteAgil_TEST", 1.6f, 1.6f, 1.6f, 1f);
        BodyShape lifter = Make("FuerteLento_TEST", 1.8f, 1.6f, 0.8f, 1f);
        BodyShape heavy = Make("PesadoDebil_TEST", 0.8f, 1.8f, 0.8f, 0.8f);

        TestProbe.Greater("alto, fuerte y ágil → más alto que el medio", athlete.Height, avg.Height + 0.03f, $"{athlete.Height:0.00} vs {avg.Height:0.00}");
        TestProbe.Greater("ágil → piernas más gruesas que el medio", Girth(athlete, BodyPart.ThighLeft), Girth(avg, BodyPart.ThighLeft) + 0.02f);
        TestProbe.Greater("fuerte sin agilidad → brazos más gruesos que piernas", Girth(lifter, BodyPart.ShoulderLeft) - Girth(lifter, BodyPart.ThighLeft), 0f,
                          $"brazo {Girth(lifter, BodyPart.ShoulderLeft):0.00} vs muslo {Girth(lifter, BodyPart.ThighLeft):0.00}");
        TestProbe.Greater("pesado y débil → más grasa que el atleta", heavy.Fat, athlete.Fat + 0.2f, $"{heavy.Fat:0.00} vs {athlete.Fat:0.00}");
        TestProbe.Greater("la grasa va al tronco más que a los antebrazos", Girth(heavy, BodyPart.Chest) - Girth(heavy, BodyPart.ForearmLeft), 0.05f);

        foreach (BodyShape s in new[] { avg, athlete, lifter, heavy }) Destroy(s.gameObject);
        TestProbe.End();
    }
}
