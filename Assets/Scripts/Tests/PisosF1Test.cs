using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA FASE F1 DEL REDISEÑO DE PISOS (grupo 143; PB-170, docs/areas/rediseno-pisos-arboles.md §6 ter):
/// 1. PISOS LLENOS (<see cref="FurnitureSpacing.FillOf"/>; CodeShip reglas 2.9 y 2.10): un piso de 40 × 40 m medio vacío o con un punto a
///    más de 6 m de todo avisa; la franja de 2,5 m del borde (para tirarse) no cuenta como hueco.
/// 2. UN PISO POR CENTRO (J16 reescrita): los edificios amplios apagados; el ocio de los centros de entrenamiento va por sus plazas y el
///    de los sitios para todos (J36), para todos.
/// 3. En la escena (si está cargada): cada torre-árbol con losas de 40 m.
/// </summary>
public class PisosF1Test : MonoBehaviour, ITestUnit
{
    public int Group => 143;
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<PisosF1Test>() == null) tr.gameObject.AddComponent<PisosF1Test>();
    }

    public IEnumerator Run()
    {
        const float S = FloorOrganization.FloorSide;
        TestProbe.Begin("Rediseño de pisos F1: pisos de 40 × 40 m llenos y sin huecos");
        var trunk = new List<Rect> { new Rect(-S * 0.5f, -S * 0.5f - 22f, 22f, 22f) };
        FurnitureSpacing.Fill empty = FurnitureSpacing.FillOf(S, trunk);
        TestProbe.Check("un piso vacío (solo el tronco al lado) está medio vacío y con huecos", empty.HalfEmpty && empty.Gaps, empty.ToString());
        var grid = new List<Rect>(trunk);
        for (float x = -16f; x <= 16f; x += 8f) for (float z = -16f; z <= 16f; z += 8f) grid.Add(new Rect(x - 1f, z - 1f, 2f, 2f));
        FurnitureSpacing.Fill full = FurnitureSpacing.FillOf(S, grid);
        TestProbe.Check("muebles de 2 m cada 8 m: lleno y ningún punto a más de 6 m", full.Ok && full.worstGap < 4.5f, full.ToString());
        FurnitureSpacing.Fill centre = FurnitureSpacing.FillOf(S, new List<Rect>(trunk) { new Rect(-10f, -10f, 20f, 20f) });
        TestProbe.Check("una sala de 20 × 20 m en el centro deja el piso medio vacío y las esquinas a > 6 m", centre.HalfEmpty && centre.Gaps, centre.ToString());
        // Muebles de 2 m cada 7 m hasta ±15 m: dentro, nada a más de 3,6 m; la esquina del borde (19,5; 19,5) queda a 6,4 m, pero es la franja para tirarse.
        var inner = new List<Rect>(trunk);
        for (float x = -14f; x <= 14f; x += 7f) for (float z = -14f; z <= 14f; z += 7f) inner.Add(new Rect(x - 1f, z - 1f, 2f, 2f));
        FurnitureSpacing.Fill edge = FurnitureSpacing.FillOf(S, inner);
        TestProbe.Check("la franja de 2,5 m del borde, libre para tirarse, no cuenta como hueco", !edge.Gaps, edge.ToString());
        TestProbe.Check("los cuartos del piso se miden aparte (para juntar componentes por cuartos)", centre.quarters != null && centre.quarters.Length == 4 && centre.quarters.All(q => q > 0f && q < 1f));
        TestProbe.End();
        yield return null;

        TestProbe.Begin("Rediseño de pisos F1: un piso por centro y el ocio por plazas (W9)");
        int m = AreaCapacity.DefaultMembers;
        TestProbe.Check("los edificios amplios están apagados: un piso por centro mientras la suma de plazas llegue",
            !TrainingCentres.Amplios && (TrainingCentres.SeatsOnePerActivity() < m || TrainingCentres.All.All(s => TrainingCentres.FloorsFor(s.name, m) == 1)), $"{TrainingCentres.SeatsOnePerActivity()} plazas, {m} miembros");
        TestProbe.Check("el ocio de un centro de entrenamiento va por las plazas de su piso (yoga Hatha: 16)",
            LeisureDirector.CapacityOf("yoga", 8, 30) == 16 && LeisureDirector.CapacityOf("artes", 6, 30) == 25 && LeisureDirector.CapacityOf("baile", 6, 30) == 20,
            $"yoga {LeisureDirector.CapacityOf("yoga", 8, 30)}, artes {LeisureDirector.CapacityOf("artes", 6, 30)}, baile {LeisureDirector.CapacityOf("baile", 6, 30)}");
        TestProbe.Check("los sitios para todos (J36: asamblea, tertulia, concierto) siguen siendo para todos", LeisureDirector.CapacityOf("asamblea", 8, 30) == 30 && LeisureDirector.CapacityOf("tertulia", 8, 30) == 30);
        TestProbe.Check("ninguna actividad está a la vez «para todos» y en un centro de entrenamiento", !LeisureDirector.AtCentre.Keys.Any(LeisureDirector.AllMembers.Contains));
        TestProbe.End();
        yield return null;

        var towers = ArbolCampos.All.Where(t => t != null && t.GetComponent<TreeRotator>() == null && t.transform.name.StartsWith("Torre_")).ToList();
        if (towers.Count == 0) yield break;   // sin la escena del santuario
        TestProbe.Begin("Rediseño de pisos F1: losas de 40 m en las torres-árbol");
        foreach (ArbolCampos t in towers)
        {
            var slabs = t.GetComponentsInChildren<Renderer>().Where(r => r.name == "Losa_arbol").ToList();
            if (slabs.Count == 0) continue;
            float small = slabs.Min(r => Mathf.Min(r.bounds.size.x, r.bounds.size.z));
            if (t.building == EnergyRule.Building) continue;   // (el parque de energía, en su casquete artificial, no es una torre del rediseño)
            TestProbe.Check($"{t.building}: {slabs.Count} losas de ≥ {S:0} m", small >= S - 0.5f, $"la más pequeña, {small:0.0} m");
        }
        TestProbe.End();
    }
}
