using System.Collections;
using UnityEngine;

/// <summary>
/// Sandbox AUTOEJECUTABLE de la rejilla de feromonas (`TraceField`), reportando por `TestProbe` (PASS/FAIL en
/// `Editor.log`, grep `[TEST]`). Verifica la PRIMITIVA en pura lógica —sin NavMesh ni Animals— porque la rejilla es un
/// campo de datos: depósito, lectura, gradiente ("seguir el rastro"), decaimiento temporal y el modo `volumetric`
/// (mar/aire = la profundidad `y` separa celdas). Se auto-crea su propio `TraceField` y lo destruye al terminar.
///
/// Los checks de CONDUCTA (un animal deriva al rastro / huye en dirección contraria) necesitan la escena con NavMesh
/// horneado + fauna → van en un sandbox/misión aparte que corre SOBRE la escena real. Ver testing-checklist §27/§29.
/// </summary>
public class TraceFieldTest : MonoBehaviour, ITestUnit
{
    public int Group => 0;              // pura lógica; crea/destruye su propio TraceField → aislado, va primero
    public bool ParallelSafe => false;  // toca el singleton TraceField → serie

    public IEnumerator Run()
    {
        TestProbe.Begin("TraceField");

        GameObject go = new GameObject("PF_test");
        go.transform.SetParent(transform);
        TraceField f = go.AddComponent<TraceField>();
        f.cellSize = 4f; f.decayPerSecond = 0.3f; f.pruneInterval = 0.5f; f.epsilon = 0.05f;
        yield return null;   // deja correr Awake. Usa métodos de INSTANCIA (f.*) → aislado del TraceField persistente.

        const TraceChannel CH = TraceChannel.ScentFood;
        Vector3 a = new Vector3(0f, 0f, 0f);

        // Depósito + lectura.
        f.Deposit(a, CH, 10f);
        TestProbe.Greater("deposito legible", f.Read(a, CH), 0f);
        TestProbe.Check("lejos = 0", f.Read(new Vector3(1000f, 0f, 1000f), CH) == 0f);

        // Gradiente: desde una celda vecina, apunta HACIA el depósito.
        Vector3 near = a + new Vector3(4f, 0f, 0f);
        Vector3 g = f.Gradient(near, CH);
        TestProbe.Check("gradiente hacia el deposito", g.sqrMagnitude > 0f && Vector3.Dot(g.normalized, (a - near).normalized) > 0.5f, $"g={g}");

        // Decaimiento con el tiempo (perezoso, al leer).
        float before = f.Read(a, CH);
        yield return new WaitForSeconds(1.5f);
        float after = f.Read(a, CH);
        TestProbe.Check("decae con el tiempo", after < before, $"{before:0.##} -> {after:0.##}");

        // volumetric (mar/aire): misma x,z pero distinta profundidad y = celdas DISTINTAS.
        f.volumetric = true;
        Vector3 lo = new Vector3(500f, 0f, 500f);
        Vector3 hi = new Vector3(500f, 80f, 500f);   // 20 celdas más arriba
        f.Deposit(lo, CH, 5f);
        f.Deposit(hi, CH, 10f);
        TestProbe.Near("3D: la profundidad separa celdas (bajo)", f.Read(lo, CH), 5f, 0.5f);
        TestProbe.Near("3D: la profundidad separa celdas (alto)", f.Read(hi, CH), 10f, 0.5f);

        TestProbe.End();
        Destroy(go);
    }
}
