using System.Collections;
using UnityEngine;

/// <summary>
/// Test de la OBSERVACIÓN/ecuanimidad (docs/apremios-guardian-observacion.md §4, testing-checklist §44), por `TestProbe`:
/// un ser PROMEDIO no recibe amortiguación (factor ~1 → el estrés existente no cambia); un ser SERENO (temple alto) y/o
/// ENTRENADO (`ObservationSkill`) tiene observación > 0 y SUFRE MENOS por la misma carga (factor < 1). Determinista.
/// </summary>
public class ObservationTest : MonoBehaviour, ITestUnit
{
    public int Group => 11;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Observacion (amortigua el sufrimiento, no el apremio)");
        yield return null;

        GameObject promedio = MakeAnima("Promedio_TEST", 1f);
        GameObject sereno   = MakeAnima("Sereno_TEST", 2f);   // temple/razon/disciplina altos
        sereno.AddComponent<ObservationSkill>().trained = 1f; // + entrenado observando

        Anima pa = promedio.GetComponent<Anima>();
        Anima sa = sereno.GetComponent<Anima>();

        TestProbe.Near("un ser PROMEDIO no observa (nivel ~0)", Observation.LevelOf(pa), 0f, 0.001f);
        TestProbe.Greater("un ser sereno+entrenado SÍ observa (nivel > 0)", Observation.LevelOf(sa), 0.2f);
        TestProbe.Near("el promedio SUFRE toda la carga (factor ~1 → no cambia el balance)", Observation.SufferingFactor(pa), 1f, 0.001f);
        TestProbe.Check("el sereno SUFRE MENOS por la misma carga (factor < promedio)",
            Observation.SufferingFactor(sa) < Observation.SufferingFactor(pa));

        Object.Destroy(promedio); Object.Destroy(sereno);
        TestProbe.End();
    }

    static GameObject MakeAnima(string name, float temple)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        SimpleAnima a = go.AddComponent<SimpleAnima>();
        a.composure = temple; a.reasoning = temple; a.discipline = temple;
        return go;
    }
}
