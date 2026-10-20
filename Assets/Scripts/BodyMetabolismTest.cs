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

        // El cuerpo PIDE: con hambre mete «comer» en su carril propio; con sueño, «dormir».
        GameObject n = new GameObject("Necesitado_TEST");
        Anima na = n.AddComponent<Anima>();
        BodyMetabolism nb = n.AddComponent<BodyMetabolism>(); nb.enabled = false;
        ActionQueue nq = n.AddComponent<ActionQueue>(); nq.enabled = false;   // que no lo ejecute: solo miramos qué encola
        NeedsDriver nd = n.AddComponent<NeedsDriver>(); nd.checkEvery = 0f;
        nb.hunger = 0.9f;
        yield return null; yield return null;
        bool eat = false; foreach (QueuedSpell s in nq.Own) if (s is EatSpell || s is DineSpell) eat = true;   // comer = ir al restaurante
        TestProbe.Check("con hambre el cuerpo encola COMER (carril propio)", eat);
        nq.ClearOwnForTest(); nb.hunger = 0f; na.sleepiness = 0.95f;
        yield return null; yield return null;
        bool sleep = false; foreach (QueuedSpell s in nq.Own) if (s is SleepSpell) sleep = true;
        TestProbe.Check("con sueño el cuerpo encola DORMIR", sleep);

        // EDAD como efecto del cuerpo: crece si come, el estrés desgasta más, las células de mago casi no envejecen.
        BodyAging fedKid = MakeAging("NinoComido_TEST", growth: 0.1f, hunger: 0f, stress: 0f, mage: false);
        BodyAging starvedKid = MakeAging("NinoHambriento_TEST", growth: 0.1f, hunger: 1f, stress: 0f, mage: false);
        fedKid.Step(24f * 200f); starvedKid.Step(24f * 200f);
        TestProbe.Check("un niño bien comido crece más que uno hambriento", fedKid.growth > starvedKid.growth,
                        $"comido {fedKid.growth:0.00} vs hambriento {starvedKid.growth:0.00}");
        BodyAging calm = MakeAging("Sereno_TEST", growth: 1f, hunger: 0f, stress: 0f, mage: false);
        BodyAging tense = MakeAging("Estresado_TEST", growth: 1f, hunger: 0f, stress: 1f, mage: false);
        BodyAging mage = MakeAging("Maga_TEST2", growth: 1f, hunger: 0f, stress: 1f, mage: true);
        calm.Step(24f * 500f); tense.Step(24f * 500f); mage.Step(24f * 500f);
        TestProbe.Check("el estrés desgasta más (envejece antes)", tense.wear > calm.wear, $"estrés {tense.wear:0.00} vs sereno {calm.wear:0.00}");
        TestProbe.Near("células de mago: casi no envejece", mage.wear, 0.2f, 0.01f);
        calm.wear = 0.99f; calm.Step(24f * 100f);
        TestProbe.Check("desgaste 1 → muere de vieja", calm.GetComponent<Anima>().death);

        foreach (BodyAging ag in new[] { fedKid, starvedKid, calm, tense, mage }) Object.Destroy(ag.gameObject);
        Object.Destroy(h); Object.Destroy(m); Object.Destroy(n);
        TestProbe.End();
    }

    static BodyAging MakeAging(string name, float growth, float hunger, float stress, bool mage)
    {
        GameObject g = new GameObject(name);
        Anima a = g.AddComponent<Anima>(); a.stress = stress;
        if (mage) g.AddComponent<MageCells>().fraction = 1f;
        BodyMetabolism b = g.AddComponent<BodyMetabolism>(); b.enabled = false; b.hunger = hunger;
        BodyAging ag = g.AddComponent<BodyAging>(); ag.enabled = false; ag.growth = growth;
        return ag;
    }
}
