using System.Collections;
using UnityEngine;

/// <summary>
/// Unidad de test del SUEÑO día/noche (docs/testing-checklist.md §41), por `TestProbe`: cada animal recibió un
/// `SleepCycle` (auto-add en `Init`) y el reloj (`TimeController`) dispara la compuerta `asleep`. Determinista: fuerza
/// la hora con `SetHour` y llama a `Evaluate()` (no espera al ciclo real). Restaura la hora al terminar.
/// </summary>
public class SleepTest : MonoBehaviour, ITestUnit
{
    public int Group => 8;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Sueño (día/noche → asleep; la amenaza despierta)");
        yield return null;

        Animal a = null;
        foreach (Animal x in FindObjectsOfType<Animal>()) if (x != null && !x.death) { a = x; break; }
        if (a == null) { Debug.Log("[TEST] SKIP · Sueño — sin fauna en escena"); TestProbe.End(); yield break; }

        SleepCycle s = a.GetComponent<SleepCycle>();
        TestProbe.NotNull("el animal tiene SleepCycle (auto-add en Init)", s);
        TimeController tc = TimeController.timeController;
        if (s == null || tc == null) { TestProbe.End(); yield break; }

        // Guardar estado para restaurar (no descuadrar la escena tras el test).
        float savedHour = tc.Hour;
        bool savedAware = a.aware;
        bool savedNocturnal = s.nocturnal;
        s.nocturnal = false;   // forzar DIURNO para el test

        tc.SetHour(2f);        // madrugada = noche
        a.aware = false;
        s.Evaluate();
        TestProbe.Check("de NOCHE (02:00) un diurno se DUERME", a.asleep);

        tc.SetHour(12f);       // mediodía = día
        s.Evaluate();
        TestProbe.Check("de DÍA (12:00) un diurno está DESPIERTO", !a.asleep);

        tc.SetHour(2f);        // noche otra vez, pero con amenaza presente
        a.aware = true;
        s.Evaluate();
        TestProbe.Check("una amenaza (aware) ROMPE el sueño aunque sea de noche", !a.asleep);

        // Restaurar el estado previo.
        a.aware = savedAware;
        s.nocturnal = savedNocturnal;
        tc.SetHour(savedHour);
        s.Evaluate();

        TestProbe.End();
    }
}
