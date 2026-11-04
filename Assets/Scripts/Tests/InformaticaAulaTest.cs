using System.Collections;
using UnityEngine;

/// <summary>
/// TEST DE LOS PISOS NUEVOS (grupo 74; docs/piso-informatica.md, docs/piso-aula.md).
/// 1. EL MODELO DEL CLÚSTER DE IA (<see cref="AiCluster"/>, sin escena): ~90 kW de TI con 64 GPU; PUE de refrigeración líquida
///    (1,03–1,20); casi sin agua (WUE &lt; 0,05 L/kWh, frente a ~1,8 de la media con torres); el calor va primero a la red de
///    distrito; la suciedad sube y, sin mantenimiento, las GPU bajan potencia; la misión de refrigeración lo arregla; si cae la red,
///    el SAI aguanta ~40 min y la misión de energía la repone; las misiones de Informática y del Aula están en el registro.
/// 2. LOS PISOS EN LA ESCENA (si está construida): Informática en la torre de Ciencias, encima de Biología, con sus 8 racks de
///    GPU, CDU, intercambiadores (distrito y mar), depósito de reposición, SAI, baterías, extinción y puestos; el Aula en la del
///    Entrenamiento, encima del Dojo, con 4 filas de pupitres y 4 mesas de estudio, plazas para todos.
/// </summary>
public class InformaticaAulaTest : MonoBehaviour, ITestUnit
{
    public int Group => 74;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<InformaticaAulaTest>() == null) tr.gameObject.AddComponent<InformaticaAulaTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Informática: el modelo del clúster de IA (energía, agua, calor, mantenimiento)");
        var go = new GameObject("Prueba_AiCluster"); go.SetActive(false);
        AiCluster ai = go.AddComponent<AiCluster>();
        ai.enabled = false;   // sin Update: el test lo avanza con Step
        go.SetActive(true);
        ai.utilization = 1f; ai.health = 1f; ai.fouling = 0f; ai.foulingPerDay = 0.08f;
        ai.Step(1f);
        TestProbe.Check("64 GPU (2 racks × 4 servidores × 8)", ai.Gpus == 64, $"{ai.Gpus}");
        TestProbe.Check("~88 kW de TI a plena carga (8 × 10,2 + 6)", ai.ItKW > 80f && ai.ItKW < 95f, $"{ai.ItKW:0.0} kW");
        TestProbe.Check("PUE de refrigeración líquida (1,03–1,20)", ai.Pue > 1.03f && ai.Pue < 1.2f, $"{ai.Pue:0.000}");
        TestProbe.Check("el calor del agua va primero a la red de distrito (hasta lo que pide)", ai.HeatToDistrictKW > 0f && ai.HeatToDistrictKW <= ai.districtDemandKW + 0.01f, $"distrito {ai.HeatToDistrictKW:0} kW, mar {ai.HeatToSeaKW:0} kW");
        ai.Step(23f);
        TestProbe.Check("casi sin agua: WUE < 0,05 L/kWh (circuito cerrado; la media con torres ronda 1,8)", ai.Wue < 0.05f, $"{ai.Wue:0.0000} L/kWh, {ai.MakeupLiters:0.00} L en un día");
        TestProbe.Check("el calor aprovechado se cuenta (kWh)", ai.RecoveredKWh > 0f, $"{ai.RecoveredKWh:0} kWh");
        for (int d = 0; d < 12; d++) ai.Step(24f);
        TestProbe.Check("sin mantenimiento, la suciedad calienta las GPU y bajan potencia", ai.Throttled, $"{ai.GpuTempC:0} °C, suciedad {ai.fouling:0.00}");
        float throttledKW = ai.ItKW;
        ai.CoolingService("Prueba");
        ai.Step(0.1f);
        TestProbe.Check("la misión de refrigeración limpia y las GPU vuelven a su temperatura", !ai.Throttled && ai.ItKW > throttledKW, $"{ai.GpuTempC:0} °C, {ai.ItKW:0} kW");
        ai.CutGrid();
        ai.Step(0.5f);
        TestProbe.Check("sin red, el SAI aguanta (media hora después sigue en marcha)", !ai.GridUp && ai.BatteryKWh > 0f && ai.ItKW > 0f, $"batería {ai.BatteryKWh:0.0} kWh");
        ai.Step(1f);
        TestProbe.Check("sin red ni batería, se apaga ordenadamente", ai.BatteryKWh <= 0f && ai.ItKW == 0f, $"batería {ai.BatteryKWh:0.0} kWh, TI {ai.ItKW:0} kW");
        ai.PowerCheck("Prueba");
        ai.Step(0.1f);
        TestProbe.Check("la misión de energía repone la acometida y el SAI", ai.GridUp && ai.ItKW > 0f);
        ai.health = 0.2f; ai.Sysadmin("Prueba");
        TestProbe.Check("la misión de sistemas deja el clúster al día", ai.health >= 0.99f);
        foreach (string id in new[] { "ifo_sysadmin", "ifo_refrigeracion", "ifo_energia" })
            TestProbe.Check($"«{id}» está en el registro con su efecto", AreaFunctions.Get(id) != null && AreaFunctions.Get(id).effect != null && AreaFunctions.Get(id).area == "Informática");
        foreach (string id in new[] { "aul_clase", "aul_estudio" })
            TestProbe.Check($"«{id}» está en el registro (Aula)", AreaFunctions.Get(id) != null && AreaFunctions.Get(id).area == "Aula");
        Destroy(go);
        TestProbe.End();
        yield return null;

        // ── Los pisos en la escena ──
        TestProbe.Begin("Informática y Aula: los pisos en su torre, con su equipo");
        GameObject info = GameObject.Find("Informatics_Area"), aula = GameObject.Find("Classroom_Area");
        if (info == null || aula == null)
        {
            TestProbe.Check("los pisos están en la escena (reconstruye SampleScene)", false, $"Informática {(info != null)}, Aula {(aula != null)}");
            TestProbe.End(); yield break;
        }
        bool Above(string lower, GameObject upper)
        {
            GameObject lo = GameObject.Find(lower);
            if (lo == null) return false;
            TreeFloor a = lo.GetComponent<TreeFloor>(), b = upper.GetComponent<TreeFloor>();
            if (a != null && b != null) return b.index > a.index;
            return upper.transform.position.y > lo.transform.position.y + 1f;
        }
        TestProbe.Check("Informática, en la torre de Ciencias encima de Biología", Above("Biology_Area", info));
        TestProbe.Check("el Aula, en la torre del Entrenamiento encima del Dojo", Above("MartialArts_Area", aula));
        int Count(GameObject root, string prefix) { int n = 0; foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name.StartsWith(prefix)) n++; return n; }
        TestProbe.Check("8 racks de GPU (dos filas de 4)", Count(info, "Rack_GPU_") == 8, $"{Count(info, "Rack_GPU_")}");
        TestProbe.Check("una CDU por fila", Count(info, "CDU") >= 2);
        TestProbe.Check("intercambiadores: a la red de distrito y al agua de mar", Count(info, "Intercambiador_distrito") >= 1 && Count(info, "Intercambiador_mar") >= 1);
        TestProbe.Check("depósito de reposición (agua del tronco), bombas, acometida, 2 SAI y 3 armarios de baterías",
            Count(info, "Deposito_reposicion") >= 1 && Count(info, "Bomba") >= 2 && Count(info, "Cuadro_general") >= 1 && Count(info, "Bloque_SAI") == 2 && Count(info, "Bloque_Armario de baterías") == 3);
        TestProbe.Check("extinción por gas inerte", Count(info, "Botella_gas_inerte") >= 4);
        TestProbe.Check("el modelo del clúster (AiCluster) y su pantalla de estado", info.GetComponent<AiCluster>() != null && info.GetComponent<AiCluster>().monitor != null);
        int members = WorkdayDirector.Staff.Length, groups = Mathf.CeilToInt(members / 6f), floors = Mathf.Max(1, Mathf.CeilToInt(members / 24f));
        TestProbe.Check($"plazas para los {members} miembros (lista del personal): ≥ {groups} filas de pupitres de 6 y ≥ {groups} mesas de estudio de 6", Count(aula, "Fila de pupitres") >= groups && Count(aula, "Mesa de estudio") >= groups, $"{Count(aula, "Fila de pupitres")} filas, {Count(aula, "Mesa de estudio")} mesas");
        TestProbe.Check($"{floors} piso(s) de Aula (24 plazas por piso; si no caben, otro piso)", Count(aula, "Planta_aula_") == floors - 1, $"{Count(aula, "Planta_aula_") + 1} pisos");
        TestProbe.Check("la tarima del profesor (pizarra y pantalla) y la biblioteca", Count(aula, "Pizarra") >= 1 && Count(aula, "Estanteria") >= 6);
        TestProbe.End();
    }
}
