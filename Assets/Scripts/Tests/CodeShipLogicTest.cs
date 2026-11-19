using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA LÓGICA PORTADA DE CODESHIP (grupo 119; docs/plan-microcosmos-realismo.md §4, Berón 2026-11-18: «no sé qué hacer ni
/// cómo jugar»). Solo lógica pura (no carga escenas ni mueve a nadie), así que va en la lista rápida:
///  · retos de código: validar el catálogo, acertar y fallar un reto de ordenar y uno de elegir, los intentos repetidos no cuentan
///    (el fallo de CodeShip: tres toques sobre la misma opción errónea terminaban la misión), el barajado nunca da la solución;
///  · la lista de tareas: el orden (historia → tarea de la jornada → misiones; dentro, lo que tiene sitio y lo más cerca primero);
///  · el viaje: el punto al que se va (el NavMesh junto al objetivo; si no hay, el borde de su caja; si no, el centro), la llegada y
///    el tope de tiempo;
///  · si la escena tiene una historia con reto (Microcosmos_ElonMusk), que el paso de reto está y que acertarlo cumple el paso.
/// </summary>
public class CodeShipLogicTest : MonoBehaviour, ITestUnit
{
    public int Group => 119;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<CodeShipLogicTest>() == null) tr.gameObject.AddComponent<CodeShipLogicTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("CodeShip: retos de código, lista de tareas y viaje");
        Challenges();
        Tasks();
        Travel();
        yield return null;
        StoryHook();
        TestProbe.End();
    }

    static void Challenges()
    {
        // ── El catálogo: cada reto bien formado ──
        string bad = "";
        foreach (CodeExercise ex in CodeChallenges.All) { string e = ex.Validate(); if (e.Length > 0) bad += $"{ex.id}: {e}; "; }
        TestProbe.Check("[CS-01] todos los retos del catálogo están bien formados", bad.Length == 0 && CodeChallenges.All.Count >= 2, bad.Length > 0 ? bad : $"{CodeChallenges.All.Count} retos");
        TestProbe.Check("[CS-02] Microcosmos_ElonMusk tiene un reto de ordenar y uno de elegir", CodeChallenges.ForScene("Microcosmos_ElonMusk").Exists(s => s.exercise.kind == CodeExercise.Kind.Order) && CodeChallenges.ForScene("Microcosmos_ElonMusk").Exists(s => s.exercise.kind == CodeExercise.Kind.Choice));

        // ── Ordenar (problema de Parsons) ──
        var order = new CodeExercise
        {
            id = "prueba-orden", kind = CodeExercise.Kind.Order, idea = "prueba",
            blocks = new[] { B("a"), B("b"), B("c"), B("x") }, solution = new[] { "a", "b", "c" },
        };
        TestProbe.Check("[CS-03] un reto de ordenar de prueba está bien formado (con un bloque de distracción)", order.Validate().Length == 0 && order.HasDistractors, order.Validate());
        TestProbe.Check("[CS-04] el orden correcto acierta", order.IsCorrect(CodeAnswer.Order("a", "b", "c")));
        TestProbe.Check("[CS-05] otro orden, uno incompleto o uno con la distracción fallan",
            !order.IsCorrect(CodeAnswer.Order("b", "a", "c")) && !order.IsCorrect(CodeAnswer.Order("a", "b")) && !order.IsCorrect(CodeAnswer.Order("a", "b", "x")) && !order.IsCorrect(CodeAnswer.Choice(0)));
        TestProbe.Check("[CS-06] cuántos bloques del principio están bien (la pista)", order.CorrectPrefix(new[] { "a", "c", "b" }) == 1 && order.CorrectPrefix(new[] { "a", "b", "c" }) == 3 && order.CorrectPrefix(new string[0]) == 0);
        bool neverSolved = true;
        for (int seed = 0; seed < 50; seed++) { string[] sh = order.Shuffled(seed); if (string.Join(",", sh).StartsWith("a,b,c") || sh.Length != 4) neverSolved = false; }
        TestProbe.Check("[CS-07] el barajado trae todos los bloques y nunca empieza ya resuelto", neverSolved);
        TestProbe.Check("[CS-08] el barajado es el mismo con la misma semilla", string.Join(",", order.Shuffled(7)) == string.Join(",", order.Shuffled(7)));
        var badOrder = new CodeExercise { id = "mal", kind = CodeExercise.Kind.Order, blocks = new[] { B("a") }, solution = new[] { "a", "z" } };
        TestProbe.Check("[CS-09] un reto cuya solución usa un bloque que no existe no valida", badOrder.Validate().Length > 0);

        // ── Elegir ──
        var choice = new CodeExercise { id = "prueba-eleccion", kind = CodeExercise.Kind.Choice, question = "¿?", blocks = new[] { B("l1") }, options = new[] { "no", "sí", "tampoco" }, correct = 1 };
        TestProbe.Check("[CS-10] un reto de elegir acierta solo con su opción", choice.Validate().Length == 0 && choice.IsCorrect(CodeAnswer.Choice(1)) && !choice.IsCorrect(CodeAnswer.Choice(0)) && !choice.IsCorrect(CodeAnswer.Order("l1")));
        var badChoice = new CodeExercise { id = "mal2", kind = CodeExercise.Kind.Choice, question = "¿?", options = new[] { "a" }, correct = 3 };
        TestProbe.Check("[CS-11] un reto de elegir con la respuesta fuera de rango no valida", badChoice.Validate().Length > 0);

        // ── Los intentos (CodeShip attempts.ts): repetir un fallo no cuenta ni deshace nada ──
        var at = new CodeAttempts();
        at.On(choice);
        at.Record(CodeAnswer.Choice(0), false);
        at.Record(CodeAnswer.Choice(0), false);
        at.Record(CodeAnswer.Choice(0), false);
        TestProbe.Check("[CS-12] tres toques sobre la misma opción errónea cuentan como un fallo", at.Count == 1 && at.Repeated(CodeAnswer.Choice(0)) && at.WrongChoices.Count == 1);
        at.Record(CodeAnswer.Choice(1), true);
        TestProbe.Check("[CS-13] un acierto no se apunta como fallo", at.Count == 1 && !at.Repeated(CodeAnswer.Choice(1)));
        at.On(order);
        TestProbe.Check("[CS-14] cambiar de reto olvida los fallos del anterior", at.Count == 0);
        at.Record(CodeAnswer.Order("b", "a", "c"), false);
        TestProbe.Check("[CS-15] un orden fallado queda apuntado por sus bloques", at.Repeated(CodeAnswer.Order("b", "a", "c")) && !at.Repeated(CodeAnswer.Order("a", "b", "c")));
    }

    static CodeBlock B(string id) => new CodeBlock { id = id, pseudo = id, js = id + "();" };

    static void Tasks()
    {
        var l = new List<TaskEntry>
        {
            new TaskEntry { id = "m1", group = TaskEntry.Group.Mision, title = "Misión sin sitio" },
            new TaskEntry { id = "j1", group = TaskEntry.Group.Jornada, title = "Tarea", hasPlace = true, distance = 5f },
            new TaskEntry { id = "h-lejos", group = TaskEntry.Group.Historia, title = "Visita B", hasPlace = true, distance = 30f },
            new TaskEntry { id = "h-cerca", group = TaskEntry.Group.Historia, title = "Visita A", hasPlace = true, distance = 4f },
            new TaskEntry { id = "h-luego", group = TaskEntry.Group.Historia, title = "Después", hasPlace = true, distance = 1f, later = true },
            new TaskEntry { id = "h-sin", group = TaskEntry.Group.Historia, title = "Sin sitio", distance = 0f },
        };
        List<TaskEntry> s = TaskOrdering.Sort(l);
        string ids = string.Join(",", s.ConvertAll(t => t.id));
        TestProbe.Check("[CS-20] la lista: historia primero, lo de ahora antes que lo de después, con sitio y lo más cerca antes; luego la jornada y las misiones",
            ids == "h-cerca,h-lejos,h-sin,h-luego,j1,m1", ids);
        TestProbe.Check("[CS-21] ordenar dos veces da lo mismo (estable)", string.Join(",", TaskOrdering.Sort(s).ConvertAll(t => t.id)) == ids);
        TestProbe.Check("[CS-22] solo lo de ahora con sitio tiene «Ir»", s.TrueForAll(t => t.CanGo == (t.hasPlace && !t.later)));
        TestProbe.Check("[CS-23] la distancia se escribe para el jugador", TaskEntry.DistanceText(3.4f) == "3 m" && TaskEntry.DistanceText(1500f) == "1,5 km" && TaskEntry.DistanceText(0.4f) == "aquí");
    }

    static void Travel()
    {
        Vector3 from = new Vector3(10f, 0f, 0f), table = Vector3.zero;
        // 1) Con NavMesh: el punto del NavMesh más cercano al objetivo (la mesa lo tapa: el punto queda en su borde).
        TravelMath.Sampler nav = (Vector3 p, float max, out Vector3 hit) => { hit = new Vector3(1.2f, 0f, 0f); return true; };
        Vector3 g1 = TravelMath.ResolveGoal(table, from, nav, new Bounds(table, new Vector3(2f, 1f, 1f)));
        TestProbe.Check("[CS-30] con NavMesh se va al punto andable junto al objetivo", (g1 - new Vector3(1.2f, 0f, 0f)).sqrMagnitude < 1e-4f, $"{g1}");
        // 2) Sin NavMesh, con caja sólida: al borde de la caja del lado del jugador, con margen.
        TravelMath.Sampler none = (Vector3 p, float max, out Vector3 hit) => { hit = p; return false; };
        Vector3 g2 = TravelMath.ResolveGoal(table, from, none, new Bounds(table, new Vector3(2f, 1f, 1f)), 0.6f);
        TestProbe.Check("[CS-31] sin NavMesh, al borde de su caja del lado de quien va (más el margen)", Mathf.Abs(g2.x - 1.6f) < 0.01f && Mathf.Abs(g2.z) < 0.01f, $"{g2}");
        // 3) Sin nada: el centro.
        Vector3 g3 = TravelMath.ResolveGoal(new Vector3(3f, 0f, 4f), from, none, null);
        TestProbe.Check("[CS-32] sin NavMesh ni caja, al propio objetivo", g3 == new Vector3(3f, 0f, 4f), $"{g3}");
        // 4) Si el punto del NavMesh queda lejos del objetivo (otro piso, otra sala), no vale.
        TravelMath.Sampler far = (Vector3 p, float max, out Vector3 hit) => { hit = p + new Vector3(0f, 0f, 9f); return true; };
        Vector3 g4 = TravelMath.ResolveGoal(table, from, far, null);
        TestProbe.Check("[CS-33] un punto del NavMesh a más del radio de búsqueda no se usa", g4 == table, $"{g4}");

        TestProbe.Check("[CS-34] llega cerca del punto o del objetivo, en planta y en el mismo piso",
            TravelMath.Arrived(new Vector3(1.5f, 1f, 0f), new Vector3(1.2f, 0f, 0f), table, 2f) && TravelMath.Arrived(new Vector3(0f, 0f, 1.9f), new Vector3(9f, 0f, 9f), table, 2f)
            && !TravelMath.Arrived(from, new Vector3(1.2f, 0f, 0f), table, 2f) && !TravelMath.Arrived(new Vector3(1.2f, 8f, 0f), new Vector3(1.2f, 0f, 0f), table, 2f));
        float t10 = TravelMath.Timeout(10f, 2f), t1k = TravelMath.Timeout(1000f, 2f), t0 = TravelMath.Timeout(0f, 2f);
        TestProbe.Check("[CS-35] el viaje tiene tope de tiempo: crece con la distancia, con mínimo y máximo", t10 > t0 && t0 >= 15f && t1k <= TravelMath.MaxSeconds && t10 >= 10f / 2f, $"{t0:0} / {t10:0} / {t1k:0} s");
    }

    static void StoryHook()
    {
        StoryMission m = StoryMission.Instance;
        if (m == null || m.steps == null) { Debug.Log("[TEST] Esta escena no tiene historia: se salta la parte del reto en la historia."); return; }
        int code = System.Array.FindIndex(m.steps, s => s.kind == StoryMission.Kind.Code);
        var specs = CodeChallenges.ForScene(m.gameObject.scene.name);
        if (specs.Count == 0) { Debug.Log($"[TEST] «{m.gameObject.scene.name}» no tiene retos de código: se salta."); return; }
        TestProbe.Check("[CS-40] los retos de la escena están en su historia, cada uno con su sitio", code >= 0 && System.Array.TrueForAll(m.steps, s => s.kind != StoryMission.Kind.Code || (s.targets.Length > 0 && s.targets[0] != null && CodeChallenges.Find(s.challenge) != null)), $"paso de reto {code + 1}");
        int guard = 0;
        while (!m.Finished && m.CurrentStep.kind != StoryMission.Kind.Code && guard++ < 20) m.CompleteCurrentForTest();
        if (m.Finished) { TestProbe.Check("[CS-41] se llega al paso del reto", false, "la historia acabó antes"); return; }
        int at = m.Step;
        CodeExercise ex = CodeChallenges.Find(m.CurrentStep.challenge);
        TestProbe.Check("[CS-41] una respuesta mala no cumple el paso", !m.AnswerChallenge(ex.kind == CodeExercise.Kind.Order ? CodeAnswer.Order(ex.Shuffled(1)) : CodeAnswer.Choice((ex.correct + 1) % ex.options.Length)) && m.Step == at);
        TestProbe.Check("[CS-42] acertar el reto cumple el paso de la historia", m.AnswerChallenge(ex.kind == CodeExercise.Kind.Order ? CodeAnswer.Order(ex.solution) : CodeAnswer.Choice(ex.correct)) && m.Step == at + 1, $"paso {m.Step + 1}");
    }
}
