using System.Collections;
using UnityEngine;

/// <summary>
/// Sandbox AUTOEJECUTABLE de la rejilla de feromonas (`PheromoneField`), reportando por `TestProbe` (PASS/FAIL en
/// `Editor.log`, grep `[TEST]`). Verifica la PRIMITIVA en pura lógica —sin NavMesh ni Animals— porque la rejilla es un
/// campo de datos: depósito, lectura, gradiente ("seguir el rastro"), decaimiento temporal y el modo `volumetric`
/// (mar/aire = la profundidad `y` separa celdas). Se auto-crea su propio `PheromoneField` y lo destruye al terminar.
///
/// Los checks de CONDUCTA (un animal deriva al rastro / huye en dirección contraria) necesitan la escena con NavMesh
/// horneado + fauna → van en un sandbox/misión aparte que corre SOBRE la escena real. Ver testing-checklist §27/§29.
/// </summary>
public class PheromoneFieldTest : MonoBehaviour, ITestUnit
{
    public int Group => 0;              // pura lógica; crea/destruye su propio PheromoneField → aislado, va primero
    public bool ParallelSafe => false;  // toca el singleton PheromoneField → serie

    public IEnumerator Run()
    {
        TestProbe.Begin("PheromoneField");

        GameObject go = new GameObject("PF_test");
        go.transform.SetParent(transform);
        PheromoneField f = go.AddComponent<PheromoneField>();
        f.cellSize = 4f; f.decayPerSecond = 0.3f; f.pruneInterval = 0.5f; f.epsilon = 0.05f;
        yield return null;   // dejar correr Awake (fija Instance)

        const TraceChannel CH = TraceChannel.ScentFood;
        Vector3 a = new Vector3(0f, 0f, 0f);

        // Depósito + lectura.
        PheromoneField.Leave(a, CH, 10f);
        TestProbe.Greater("deposito legible", PheromoneField.Sniff(a, CH), 0f);
        TestProbe.Check("lejos = 0", PheromoneField.Sniff(new Vector3(1000f, 0f, 1000f), CH) == 0f);

        // Gradiente: desde una celda vecina, Trail apunta HACIA el depósito.
        Vector3 near = a + new Vector3(4f, 0f, 0f);
        Vector3 g = PheromoneField.Trail(near, CH);
        TestProbe.Check("gradiente hacia el deposito", g.sqrMagnitude > 0f && Vector3.Dot(g.normalized, (a - near).normalized) > 0.5f, $"g={g}");

        // Decaimiento con el tiempo (perezoso, al leer).
        float before = PheromoneField.Sniff(a, CH);
        yield return new WaitForSeconds(1.5f);
        float after = PheromoneField.Sniff(a, CH);
        TestProbe.Check("decae con el tiempo", after < before, $"{before:0.##} -> {after:0.##}");

        // volumetric (mar/aire): misma x,z pero distinta profundidad y = celdas DISTINTAS.
        f.volumetric = true;
        Vector3 lo = new Vector3(500f, 0f, 500f);
        Vector3 hi = new Vector3(500f, 80f, 500f);   // 20 celdas más arriba
        PheromoneField.Leave(lo, CH, 5f);
        PheromoneField.Leave(hi, CH, 10f);
        TestProbe.Near("3D: la profundidad separa celdas (bajo)", PheromoneField.Sniff(lo, CH), 5f, 0.5f);
        TestProbe.Near("3D: la profundidad separa celdas (alto)", PheromoneField.Sniff(hi, CH), 10f, 0.5f);

        TestProbe.End();
        Destroy(go);
    }
}
