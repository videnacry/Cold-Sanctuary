using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test de los SENTIDOS/ÓRGANOS (docs/consciousness-mechanics.md §3, testing-checklist §47), por `TestProbe`: la
/// observación extrae canales de información según los ÓRGANOS del ser. Un ojo capta `Color`; un ojo ciego no; una
/// lengua sinestésica "saborea la luz" (capta `Color` por el gusto). Determinista (`Perceive` usa órganos + distancia).
/// </summary>
public class SensesTest : MonoBehaviour, ITestUnit
{
    public int Group => 14;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Sentidos/órganos (observar = canales según el receptor)");
        yield return null;

        GameObject target = new GameObject("Percept_Target_TEST");
        target.transform.position = new Vector3(0f, 0f, 1f);
        target.AddComponent<Anima>();

        GameObject withEye = MakeObserver("ConOjo_TEST");   SenseOrgan.Eye(withEye);
        GameObject blind   = MakeObserver("Ciego_TEST");    SenseOrgan.BlindEye(blind);
        GameObject tongue  = MakeObserver("LenguaLuz_TEST"); SenseOrgan.LightTastingTongue(tongue);
        yield return null;   // deja correr Awake (ObserveSpell cachea su Anima)

        Anima ta = target.GetComponent<Anima>();
        Dictionary<PerceptChannel, float> pEye   = withEye.GetComponent<ObserveSpell>().Perceive(ta);
        Dictionary<PerceptChannel, float> pBlind = blind.GetComponent<ObserveSpell>().Perceive(ta);
        Dictionary<PerceptChannel, float> pTong  = tongue.GetComponent<ObserveSpell>().Perceive(ta);

        TestProbe.Greater("el OJO capta Color", Get(pEye, PerceptChannel.Color), 0f);
        TestProbe.Greater("el ojo capta la presencia (general)", Get(pEye, PerceptChannel.Presence), 0f);
        TestProbe.Near("el ojo CIEGO NO capta Color", Get(pBlind, PerceptChannel.Color), 0f, 0.0001f);
        TestProbe.Greater("la LENGUA-LUZ saborea la luz (capta Color por el gusto)", Get(pTong, PerceptChannel.Color), 0f);
        TestProbe.Greater("la lengua-luz también capta Sabor", Get(pTong, PerceptChannel.Flavor), 0f);

        Object.Destroy(target); Object.Destroy(withEye); Object.Destroy(blind); Object.Destroy(tongue);
        TestProbe.End();
    }

    static float Get(Dictionary<PerceptChannel, float> d, PerceptChannel c) => d != null && d.TryGetValue(c, out float v) ? v : 0f;

    static GameObject MakeObserver(string name)
    {
        GameObject go = new GameObject(name);
        go.AddComponent<Anima>();
        go.AddComponent<ObserveSpell>();
        return go;
    }
}
