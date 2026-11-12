using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EL REFUGIO CRECE HASTA QUE SE PUEDE VIVIR ALLÍ (Berón 2026-11-05, #128: «el refugio sería ideal irlo mejorando al punto que sea posible vivir alli»; docs/sitio-nuclear.md
/// §21 fase 2 y §26). Crece por ETAPAS, cada una una obra (<see cref="ConstructionProject"/>) que el equipo de construcción del Marino va haciendo, una detrás de otra.
/// Las tareas de obra son opcionales (se hacen con horas libres y nunca piden un clon: <see cref="AreasEnObra"/>). Cada etapa terminada sube lo que el refugio aguanta
/// (<see cref="CapacityAfter"/>): plazas de emergencia, gente que puede vivir allí por turnos, días de autonomía o residentes fijos. La autonomía de un mes con el refugio
/// lleno (#125, 720 h) se mantiene en cada etapa. Lógica pura salvo <see cref="RefugeBuilder"/>, que abre las obras en la escena.
/// </summary>
public static class RefugeGrowth
{
    public const string Prefix = "Refugio: ";

    public struct Stage
    {
        public string title, what; public int days, crew;
        public Stage(string title, int days, int crew, string what) { this.title = Prefix + title; this.days = days; this.crew = crew; this.what = what; }
    }

    /// <summary>LAS ETAPAS, en orden (días de obra y equipo: como las salas de Ballet y Dojo, estimación mía).</summary>
    public static readonly Stage[] Stages =
    {
        new Stage("dormitorios", 6, 2, "12 literas en tres dormitorios con aseo: se puede dormir allí por turnos"),
        new Stage("comedor y despensa", 5, 2, "cocina, comedor y despensa para dos meses (432 kg de comida seca)"),
        new Stage("agua y aire para 24", 6, 2, "segunda planta de ósmosis y segundo electrolizador: agua y oxígeno para 24"),
        new Stage("invernadero", 8, 3, "276 m² de cultivo con luz (unos 23 m² por persona: Lunar Palace 1): comida fresca para 12 sin gastar la despensa"),
        new Stage("enfermería y taller", 5, 2, "enfermería y taller de reparación: se puede vivir allí sin subir"),
    };
    public static bool IsStage(string title) => title != null && title.StartsWith(Prefix);

    /// <summary>Lo que aguanta el refugio con <c>n</c> etapas terminadas.</summary>
    public struct Capacity
    {
        public int seats;      // plazas de emergencia (aire y agua para tantos durante un mes)
        public int livable;    // quienes pueden vivir allí por turnos de semanas (camas)
        public float days;     // días de comida para los que viven allí (sin contar el invernadero: con él, sin límite)
        public int permanent;  // quienes pueden vivir allí sin subir (con invernadero, enfermería y taller)
        public bool habitable => permanent > 0;
        public override string ToString() => $"{seats} plazas · {livable} por turnos · {(days >= Forever ? "sin límite" : $"{days:0} días")} · {permanent} fijos";
    }

    /// <summary>«Sin límite»: un año, para los números.</summary>
    public const float Forever = 365f;

    public static float FoodKg(int done) => RefugeRule.FoodKg + (done >= 2 ? 216f : 0f);
    public static int AirPeople(int done) => RefugeRule.Capacity + (done >= 3 ? 12 : 0);
    public static int Beds(int done) => done >= 1 ? 12 : 0;
    public static int Greenhouse(int done) => done >= 4 ? 12 : 0;

    public static Capacity CapacityAfter(int done)
    {
        done = Mathf.Clamp(done, 0, Stages.Length);
        int beds = Beds(done);
        float days = beds == 0 ? AutonomyHours(RefugeRule.Capacity, done, true) / 24f : Mathf.Min(Forever, AutonomyHours(beds, done, true) / 24f);
        return new Capacity { seats = AirPeople(done), livable = beds, days = days, permanent = done >= Stages.Length && Greenhouse(done) >= beds ? beds : 0 };
    }

    /// <summary>LA AUTONOMÍA (h) de <paramref name="people"/> personas con <paramref name="done"/> etapas: con corriente y sondeos, el agua y el aire se hacen para quienes
    /// quepan en la planta (<see cref="AirPeople"/>) y manda la comida (la despensa, menos lo que da el invernadero); sin corriente, lo guardado: una semana para 12.</summary>
    public static float AutonomyHours(int people, int done, bool power)
    {
        if (people <= 0) return Forever * 24f;
        if (!power) return RefugeRule.StoredHours * Mathf.Min(1f, RefugeRule.Capacity / (float)people);
        if (people > AirPeople(done)) return RefugeRule.StoredHours * AirPeople(done) / people;   // (más de los que la planta mantiene: lo guardado, repartido)
        float perDay = Mathf.Max(0, people - Greenhouse(done)) * RefugeRule.FoodKgDay;
        return perDay <= 0f ? Forever * 24f : Mathf.Min(Forever * 24f, FoodKg(done) / perDay * 24f);
    }
}

/// <summary>
/// LAS OBRAS DEL REFUGIO EN LA ESCENA: abre la obra de la siguiente etapa (una a la vez, en orden) y, cuando acaba, la siguiente. Las etapas hechas se leen de lo guardado de
/// cada obra (<c>cs_obra_&lt;título&gt;</c>). Va en el mismo objeto que <see cref="Refuge"/> (lo pone él). Mira cada 5 s; nada espera sin tope.
/// </summary>
public class RefugeBuilder : MonoBehaviour
{
    public int Done { get; private set; }
    /// <summary>Las pruebas lo encienden para abrir las obras con ellas en marcha.</summary>
    public static bool InTests;
    ConstructionProject _open;

    IEnumerator Start()
    {
        while (enabled)
        {
            Tick();
            yield return new WaitForSeconds(5f);
        }
    }

    void Tick()
    {
        if ((ColdSanctuary.TestRunner.Running || ColdSanctuary.TestRunner.Requested) && !InTests) return;   // (con las pruebas no se abre ninguna obra: las de las áreas las miden ellas)
        if (_open != null && !_open.Finished) return;
        if (_open != null) { Done++; Debug.Log($"[Refugio] Terminada la etapa «{_open.title}»: ahora {RefugeGrowth.CapacityAfter(Done)}."); Destroy(_open.gameObject); _open = null; }
        while (Done < RefugeGrowth.Stages.Length && SavedFinished(RefugeGrowth.Stages[Done])) Done++;
        if (Done >= RefugeGrowth.Stages.Length) return;
        RefugeGrowth.Stage s = RefugeGrowth.Stages[Done];
        var go = new GameObject($"Obra_{s.title}");
        go.transform.SetParent(transform, false);
        _open = go.AddComponent<ConstructionProject>();
        _open.title = s.title; _open.daysRequired = s.days; _open.crewNeeded = s.crew; _open.reform = true;   // (reform: sus tareas de obra son opcionales, con horas libres)
        Debug.Log($"[Refugio] Obra abierta: «{s.title}» ({s.days} días, equipo de {s.crew}): {s.what}.");
    }

    static bool SavedFinished(RefugeGrowth.Stage s)
    {
        string[] parts = PlayerPrefs.GetString("cs_obra_" + s.title, "").Split('|');
        return parts.Length > 0 && int.TryParse(parts[0], out int d) && d >= s.days;
    }
}
