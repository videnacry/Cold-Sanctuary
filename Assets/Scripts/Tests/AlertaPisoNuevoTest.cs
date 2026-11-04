using System.Collections;
using UnityEngine;

/// <summary>
/// TEST DE LA ALERTA DE PISO NUEVO (grupo 76; manual §11, Berón 2026-11-04). Datos puros, sin escena:
/// - Gates (Albuquerque) ya está en Informática y Scott (Prairie View) en el Aula, representados;
/// - los que la alerta señaló (Menfis, Alejandría, Bagdad, Crotona, Sils Maria → Aula; Hawthorne → control de misión) ya están re-apuntados;
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
        // (2026-11-04, 2.ª vuelta) Los que la alerta señaló ya están re-apuntados: al Aula (biblioteca y aula) y al control de misión.
        foreach (string d in new[] { "Menfis", "Alejandría", "Bagdad", "Crotona", "Sils Maria" })
        { MicroFloors.Match m = MicroFloors.Of(d); TestProbe.Check($"{d} re-apuntado al Aula, representado", m != null && m.platform == "Classroom_Area" && m.representative, m != null ? m.platform : "sin fila"); }
        MicroFloors.Match musk = MicroFloors.Of("Hawthorne");
        TestProbe.Check("Hawthorne (Musk) re-apuntado al control de misión de la lanzadera, representado", musk != null && musk.platform == "Control_de_mision" && musk.representative, musk != null ? musk.platform : "sin fila");
        var aula = MicroFloors.PendingFor("Classroom_Area", "Aula");
        TestProbe.Check("el Aula ya no deja pendientes que encajen (todos re-apuntados)", aula.Count == 0, Names(aula));
        var info = MicroFloors.PendingFor("Informatics_Area", "Informática");
        TestProbe.Check("Informática tampoco (Bagdad está en el Aula)", info.Count == 0, Names(info));
        // El buscador, con una fila pendiente que sigue en la tabla: un campo de deportes encuentra la Tregua de 1914.
        var sport = MicroFloors.PendingFor("Campo_deportes", "Campo de deportes", new[] { "deportes", "fútbol" });
        TestProbe.Check("un campo de deportes encontraría la Tregua de 1914 (sigue pendiente)", sport.Exists(m => m.display == "Tregua 1914"), Names(sport));
        var none = MicroFloors.PendingFor("Calisthenics_Area", "Calistenia");
        TestProbe.Check("un piso sin relación (Calistenia) no encuentra a nadie", none.Count == 0, Names(none));
        TestProbe.End();
        yield break;
    }
}
