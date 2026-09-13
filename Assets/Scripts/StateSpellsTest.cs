using System.Collections;
using UnityEngine;

/// <summary>
/// Test de los HECHIZOS-ESTADO (docs/apremios-guardian-observacion.md §3, testing-checklist §45), por `TestProbe`:
/// el hub `AllostaticState` (carga externa + sedación), el MIEDO (resistido por el temple), y la ADICCIÓN (dosis →
/// deseo/tolerancia). Determinista (efecto en el mismo frame, antes del decaimiento; sin física/ITarget).
/// </summary>
public class StateSpellsTest : MonoBehaviour, ITestUnit
{
    public int Group => 12;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Hechizos-estado (hub alostatico, miedo, adiccion)");
        yield return null;

        // Hub: carga y sedación se registran.
        GameObject hubGO = new GameObject("Hub_TEST");
        AllostaticState hub = hubGO.AddComponent<AllostaticState>();
        hub.AddLoad(0.5f); hub.AddSedation(0.5f);
        TestProbe.Greater("el hub registra carga externa (miedo/duelo/mono)", hub.ExtraLoad, 0.4f);
        TestProbe.Greater("el hub registra sedacion (estupefaciente)", hub.Sedation, 0.4f);

        // Miedo: el TEMPLE resiste (menos carga en el sereno que en el frágil).
        GameObject fearGO = new GameObject("Fear_TEST");
        FearSpell fear = fearGO.AddComponent<FearSpell>();
        fear.force = 1f;
        GameObject fragil = MakeAnima("Fragil_TEST", 0.2f);
        GameObject sereno = MakeAnima("Sereno_TEST", 3f);
        fear.Frighten(fragil.GetComponent<Anima>());
        fear.Frighten(sereno.GetComponent<Anima>());
        float loadFragil = AllostaticState.Of(fragil).ExtraLoad;
        float loadSereno = AllostaticState.Of(sereno).ExtraLoad;
        TestProbe.Greater("el miedo carga al fragil", loadFragil, 0.1f);
        TestProbe.Check("el TEMPLE resiste el miedo (sereno < fragil)", loadSereno < loadFragil);

        // Adicción: una dosis sube deseo y tolerancia.
        AddictionState add = MakeAnima("Adicto_TEST", 1f).AddComponent<AddictionState>();
        add.Dose();
        TestProbe.Greater("la dosis sube el deseo (craving)", add.craving, 0f);
        TestProbe.Greater("la dosis sube la tolerancia", add.Tolerance, 0f);

        Object.Destroy(hubGO); Object.Destroy(fearGO); Object.Destroy(fragil); Object.Destroy(sereno);
        Object.Destroy(add.gameObject);
        TestProbe.End();
    }

    static GameObject MakeAnima(string name, float composure)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.AddComponent<Anima>().composure = composure;
        return go;
    }
}
