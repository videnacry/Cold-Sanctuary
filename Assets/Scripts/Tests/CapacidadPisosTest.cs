using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TEST DE LA REGLA DE CAPACIDAD (grupo 75; manual C8, Berón 2026-11-04). Primero el VALIDADOR con un área de prueba (dos puestos
/// de trabajo pasan, uno no; un aula de estudio por debajo de los miembros falla). Después, en la escena (si está construida):
/// cada actividad de trabajo con ≥ 2 puestos, y el ENTRENAMIENTO y el ESTUDIO para TODOS los miembros a la vez: el gimnasio (con su
/// piso de cardio), las dos salas de yoga (Hatha y Ashtanga) y el Aula (clase y estudio). El ring de almohadas aún no existe: se
/// apunta como pendiente.
/// </summary>
public class CapacidadPisosTest : MonoBehaviour, ITestUnit
{
    public int Group => 75;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<CapacidadPisosTest>() == null) tr.gameObject.AddComponent<CapacidadPisosTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Regla de capacidad: el validador");
        var root = new GameObject("Prueba_area_capacidad");
        void Seat(string act, AreaSeat.Category c, int n) { var g = new GameObject("Puesto"); g.transform.SetParent(root.transform, false); AreaSeat.Add(g, act, c, n); }
        Seat("soldar", AreaSeat.Category.Trabajo, 1); Seat("soldar", AreaSeat.Category.Trabajo, 1);
        Seat("pintar", AreaSeat.Category.Trabajo, 1);
        Seat("estudio", AreaSeat.Category.Estudio, 6); Seat("estudio", AreaSeat.Category.Estudio, 6);
        var lines = AreaCapacity.Check(new[] { root.transform }, 18, out _);
        AreaCapacity.Line L(string act) => lines.Find(l => l.activity == act);
        TestProbe.Check("dos puestos de la misma tarea: cumple", L("soldar").ok && L("soldar").have == 2);
        TestProbe.Check("un solo puesto: no cumple (varias personas a la vez)", !L("pintar").ok);
        TestProbe.Check("estudio para 12 con 18 miembros: no cumple (caben todos)", !L("estudio").ok && L("estudio").need == 18);
        // (2026-11-05) Un centro de dos pisos con uno aún en obras: da servicio el terminado y, para la regla, cuenta también el otro.
        var obra = new GameObject("Piso_en_obras"); obra.transform.SetParent(root.transform, false);
        obra.AddComponent<ConstructionProject>().persist = false;
        void Dojo(Transform parent) { var g = new GameObject("Puesto"); g.transform.SetParent(parent, false); AreaSeat.Add(g, "dojo", AreaSeat.Category.Entrenamiento, 10); }
        Dojo(root.transform); Dojo(obra.transform);
        lines = AreaCapacity.Check(new[] { root.transform }, 18, out _);
        TestProbe.Check("un centro de dos pisos con uno en obras: cumple, y solo da servicio el terminado", L("dojo").ok && L("dojo").have == 10 && L("dojo").planned == 10, L("dojo").ToString());
        Destroy(root);
        TestProbe.End();
        yield return null;

        TestProbe.Begin("Regla de capacidad: los pisos del santuario");
        var areas = new List<Transform>();
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsInactive.Include, FindObjectsSortMode.None)) areas.Add(a.transform);
        if (areas.Count == 0) { TestProbe.Check("hay áreas en la escena (reconstruye SampleScene)", false); TestProbe.End(); yield break; }
        int members = AreaCapacity.Members();
        var all = AreaCapacity.Check(areas, members, out var pending);
        foreach (AreaCapacity.Line l in all)
            TestProbe.Check($"{l.area} · {l.activity} ({l.category}): {l.have} puestos{(l.planned > 0 ? $" y {l.planned} en obras" : "")} para {l.need}", l.ok);
        TestProbe.Check("el Aula: clase y estudio para todos", all.Exists(l => l.area == "Aula" && l.activity == "clase" && l.ok) && all.Exists(l => l.area == "Aula" && l.activity == "estudio" && l.ok));
        TestProbe.Check("Informática: sistemas, refrigeración y energía, varias personas a la vez",
            all.Exists(l => l.area == "Informática" && l.activity == "sysadmin" && l.ok) && all.Exists(l => l.area == "Informática" && l.activity == "refrigeración" && l.ok) && all.Exists(l => l.area == "Informática" && l.activity == "energía" && l.ok));
        Debug.Log($"[Capacidad] pendientes: {string.Join(", ", pending)}");
        TestProbe.End();
    }
}
