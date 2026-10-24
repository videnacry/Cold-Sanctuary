using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Envuelto en namespace (única excepción a "sin namespaces" del proyecto): el nombre "TestRunner" choca
// (CS0434) con el namespace UnityEditor.TestRunner del Unity Test Framework — `global::` no lo resuelve
// (limitación conocida del compilador con esta colisión puntual). El resto de la clase, sin cambios.
namespace ColdSanctuary
{
/// <summary>
/// Orquestador de tests (docs/testing-checklist.md §32) — el "array-de-arrays" del usuario: reúne las
/// <see cref="ITestUnit"/> de sus hijos/sí mismo y las corre **grupo a grupo EN SERIE** (menor `Group` primero);
/// dentro de un grupo, EN PARALELO solo si TODAS son `ParallelSafe` (si no, en serie). Serie por defecto porque la
/// mayoría de los tests mutan estado compartido (la rejilla es singleton; la fauna se muta y restaura) → correrlos
/// solapados los haría flaky. Emite un `[TEST] TOTAL` al final (resumen de resúmenes) — el compañero lee un solo
/// veredicto ordenado en `Editor.log`. Añadir un test = meter una unidad en su grupo.
/// </summary>
public class TestRunner : MonoBehaviour
{
    [Tooltip("Espera inicial para que FamilyGenerator + Init de la fauna asienten antes de correr las unidades.")]
    public float startDelay = 1.6f;

    /// <summary>¿Están corriendo las pruebas? (la puerta de la enfermería y la jornada no interfieren con ellas)</summary>
    public static bool Running { get; private set; }

    /// <summary>Las pruebas solo corren si se piden (Temp/run-tests, lo crea «Tools/Cold Sanctuary/Jugar con las pruebas» o el
    /// comando «test» del buzón del editor). Antes corrían en CADA Play: varios minutos moviendo fauna, personajes y la consola
    /// de hechizos mientras se intentaba jugar (Berón 2026-10-24: «no he podido mover al personaje en un buen tiempo»).</summary>
    public const string RequestFile = "Temp/run-tests";
    public static bool Requested => Application.isEditor && System.IO.File.Exists(RequestFile);

    void Awake()
    {
        if (!Requested) gameObject.SetActive(false);   // sin pruebas: ni el orquestador ni sus unidades hacen nada
    }

    IEnumerator Start()
    {
        Running = true;
        try { System.IO.File.Delete(RequestFile); } catch (System.Exception) { }   // una vez: el siguiente Play es para jugar
        yield return new WaitForSeconds(startDelay);
        TestProbe.ResetTotals();

        // Reunir unidades (en este GO y sus hijos) y ordenarlas por grupo.
        List<ITestUnit> units = new List<ITestUnit>();
        foreach (MonoBehaviour mb in GetComponentsInChildren<MonoBehaviour>())
            if (mb is ITestUnit u) units.Add(u);
        units.Sort((a, b) => a.Group.CompareTo(b.Group));

        Debug.Log($"[TEST] ══════ TestRunner: {units.Count} unidad(es) ══════");

        int i = 0;
        while (i < units.Count)
        {
            int g = units[i].Group;
            List<ITestUnit> group = new List<ITestUnit>();
            while (i < units.Count && units[i].Group == g) { group.Add(units[i]); i++; }

            bool allParallel = group.Count > 1;
            foreach (ITestUnit u in group) if (!u.ParallelSafe) { allParallel = false; break; }

            if (allParallel)
            {
                List<Coroutine> running = new List<Coroutine>();
                foreach (ITestUnit u in group) running.Add(StartCoroutine(u.Run()));
                foreach (Coroutine c in running) yield return c;   // barrera: esperar a todo el grupo
            }
            else
            {
                foreach (ITestUnit u in group) yield return StartCoroutine(u.Run());   // en serie
            }
        }

        TestProbe.Total();
        Running = false;
    }
}
}
