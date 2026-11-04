using System.Collections;
using UnityEngine;

/// <summary>
/// TEST DE LA ALERTA DE PISO NUEVO (grupo 76; manual §11, Berón 2026-11-04). Datos puros, sin escena:
/// - Gates (Albuquerque) ya está en Informática y Scott (Prairie View) en el Aula, representados;
/// - un piso de biblioteca y aula encuentra a los pendientes que esperan una biblioteca o un aula (Menfis, Alejandría, Bagdad,
///   Crotona, Sils Maria);
/// - Informática encuentra a Bagdad («sala de cálculo»); una fábrica de cohetes, a Hawthorne; un piso sin relación (Calistenia), a
///   ninguno;
/// - las tildes y mayúsculas no cuentan.
/// </summary>
public class AlertaPisoNuevoTest : MonoBehaviour, ITestUnit
{
    public int Group => 76;
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<AlertaPisoNuevoTest>() == null) tr.gameObject.AddComponent<AlertaPisoNuevoTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Alerta de piso nuevo: los pendientes de piso representativo");
        MicroFloors.Match gates = MicroFloors.Of("Albuquerque"), scott = MicroFloors.Of("Prairie View");
        TestProbe.Check("Gates (Albuquerque) re-apuntado a Informática, representado", gates != null && gates.platform == "Informatics_Area" && gates.representative, gates != null ? $"{gates.platform}, «{gates.pending}»" : "sin fila");
        TestProbe.Check("Scott (Prairie View) re-apuntada al Aula, representada", scott != null && scott.platform == "Classroom_Area" && scott.representative, scott != null ? $"{scott.platform}, «{scott.pending}»" : "sin fila");
        TestProbe.Check("las tildes y mayúsculas no cuentan", MicroFloors.Norm("Informática ÁULA") == "informatica aula");
        string Names(System.Collections.Generic.List<MicroFloors.Match> l) => string.Join(", ", l.ConvertAll(m => m.display));
        var aula = MicroFloors.PendingFor("Classroom_Area", "Aula");
        bool Has(System.Collections.Generic.List<MicroFloors.Match> l, string d) => l.Exists(m => m.display == d);
        TestProbe.Check("el Aula (biblioteca) encuentra a Menfis, Alejandría, Bagdad, Crotona y Sils Maria",
            Has(aula, "Menfis") && Has(aula, "Alejandría") && Has(aula, "Bagdad") && Has(aula, "Crotona") && Has(aula, "Sils Maria"), Names(aula));
        TestProbe.Check("…y no a los ya representados (Scott)", !Has(aula, "Prairie View"));
        var info = MicroFloors.PendingFor("Informatics_Area", "Informática");
        TestProbe.Check("Informática encuentra a Bagdad (sala de cálculo)", Has(info, "Bagdad"), Names(info));
        var rockets = MicroFloors.PendingFor("Rocket_Area", "Fábrica de cohetes", new[] { "cohetes" });
        TestProbe.Check("una fábrica de cohetes encontraría a Hawthorne (Musk)", Has(rockets, "Hawthorne"), Names(rockets));
        var launch = MicroFloors.PendingFor("Control_de_mision", "Control de misión");
        TestProbe.Check("el control de misión de la lanzadera (piso nuevo de 2026-11-04) encuentra a Hawthorne (Musk)", Has(launch, "Hawthorne"), Names(launch));
        var none = MicroFloors.PendingFor("Calisthenics_Area", "Calistenia");
        TestProbe.Check("un piso sin relación (Calistenia) no encuentra a nadie", none.Count == 0, Names(none));
        TestProbe.End();
        yield break;
    }
}
