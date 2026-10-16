using System.Collections;
using UnityEngine;

/// <summary>
/// Unidad de test del METABOLISMO DEL CUERPO (docs/body-metabolism): un humano sin comer coge hambre, el sueño sube
/// despierto y baja dormido (y pesa más de madrugada), comer lo sacia, y las CÉLULAS DE MAGO (fracción 1, como la
/// Magnate) anulan hambre y sueño. Determinista: avanza con BodyMetabolism.Step, sin esperar al reloj.
/// </summary>
public class BodyMetabolismTest : MonoBehaviour, ITestUnit
{
    public int Group => 16;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Cuerpo (metabolismo: hambre, sueño, células)");
        yield return null;

        GameObject h = new GameObject("Humano_TEST");
        Anima a = h.AddComponent<Anima>();
        BodyMetabolism body = h.AddComponent<BodyMetabolism>();
        body.enabled = false;   // solo pasos manuales
        body.Step(7f, 12f);
        TestProbe.Greater("7 h sin comer → hambre", body.hunger, 0.8f, $"hambre={body.hunger:0.00}");
        body.Feed(1f);
        TestProbe.Near("comer sacia", body.hunger, 0f, 0.01f);

        a.sleepiness = 0f; body.Step(2f, 12f); float day = a.sleepiness;
        a.sleepiness = 0f; body.Step(2f, 3f);  float night = a.sleepiness;
        TestProbe.Check("de madrugada el sueño sube más que a mediodía", night > day, $"madrugada {night:0.00} vs mediodía {day:0.00}");
        a.asleep = true; a.sleepiness = 0.8f; body.Step(4f, 3f);
        TestProbe.Check("dormido el sueño baja", a.sleepiness < 0.8f, $"{a.sleepiness:0.00}");
        a.asleep = false;

        GameObject m = new GameObject("Maga_TEST");
        Anima ma = m.AddComponent<Anima>();
        m.AddComponent<MageCells>().fraction = 1f;
        BodyMetabolism mb = m.AddComponent<BodyMetabolism>();
        mb.enabled = false;
        mb.Step(24f, 3f);
        TestProbe.Near("células de mago (fracción 1): sin hambre", mb.hunger, 0f, 0.001f);
        TestProbe.Near("células de mago (fracción 1): sin sueño", ma.sleepiness, 0f, 0.001f);

        Object.Destroy(h); Object.Destroy(m);
        TestProbe.End();
    }
}
