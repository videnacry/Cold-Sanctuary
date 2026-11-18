using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PRUEBA DE LA BIBLIOTECA DE MISIONES POR RETOS DE CÓDIGO (grupo 120; portada de CodeShip src/modules/chores; Berón 2026-11-18: «las misiones
/// se crean como las de codeship … cocinar, curar, plantar ideas, trabajar el campo, entrenar físicamente, hacer yoga, cuidar animales»).
/// Lógica pura, más una parte en la escena si Microcosmos_ElonMusk está cargado:
///  · la biblioteca: cada plantilla bien formada, con fuente, y todas las clases que pidió Berón;
///  · una tarea (ChoreRun): los retos en orden, un acierto avanza, dos fallos la hacen fallar, repetir un fallo no cuenta;
///  · la cadena de pasos (stepChain): un reto por paso; «{bien|mal|mal}» pide el valor; la buena en un sitio estable;
///  · la curva de dificultad (duelProfileFor) y el duelo de ideas («plantar ideas»): meta, paciencia, bloques y conceptos crecen con ella;
///  · Musk: cada puesto y cada necesidad de la nave y del pueblo tiene su misión; en la escena, cada una encuentra su sitio.
/// </summary>
public class ChoreLibraryTest : MonoBehaviour, ITestUnit
{
    public int Group => 120;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<ChoreLibraryTest>() == null) tr.gameObject.AddComponent<ChoreLibraryTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("CodeShip: biblioteca de misiones por retos de código");
        Library();
        Runs();
        Chains();
        Duel();
        Musk();
        Motions();
        yield return null;
        Scene();
        TestProbe.End();
    }

    static void Library()
    {
        string bad = "";
        foreach (ChoreTemplate t in MissionLibrary.All)
        {
            string e = t.Validate();
            if (e.Length > 0) bad += $"{t.id}: {e}; ";
        }
        TestProbe.Check("[CS-50] todas las plantillas están bien formadas (retos válidos, fuente, título)", bad.Length == 0 && MissionLibrary.All.Count >= 20, bad.Length > 0 ? bad : $"{MissionLibrary.All.Count} plantillas");
        var ids = new HashSet<string>(); string dup = "";
        foreach (ChoreTemplate t in MissionLibrary.All) if (!ids.Add(t.id)) dup += t.id + " ";
        TestProbe.Check("[CS-51] los ids no se repiten", dup.Length == 0, dup);
        string[] kinds = { "cocinar", "atender-paciente", "plantar-idea", "regar", "cosechar", "curar-planta", "ejercicio", "yoga", "alimentar-perro", "jugar-perro", "programar-animal", "mecanizar", "avionica", "composite", "reparar-maquina", "dormir", "asearse" };
        string missing = "";
        foreach (string k in kinds) if (!MissionLibrary.All.Exists(t => t.kind == k)) missing += k + " ";
        TestProbe.Check("[CS-52] están todas las clases que pidió Berón y las de los puestos de la nave", missing.Length == 0, missing.Length > 0 ? "faltan: " + missing : string.Join(", ", kinds));
        bool everyCat = true;
        foreach (ChoreCategory c in System.Enum.GetValues(typeof(ChoreCategory))) if (!MissionLibrary.All.Exists(t => t.category == c)) everyCat = false;
        TestProbe.Check("[CS-53] cada grupo de la lista de tareas tiene alguna misión", everyCat);
    }

    static CodeBlock B(string id) => new CodeBlock { id = id, pseudo = id, js = id + "();" };

    static void Runs()
    {
        var a = new CodeExercise { id = "r1", kind = CodeExercise.Kind.Choice, question = "¿?", options = new[] { "x", "y", "z" }, correct = 1 };
        var b = new CodeExercise { id = "r2", kind = CodeExercise.Kind.Order, blocks = new[] { B("p"), B("q") }, solution = new[] { "p", "q" } };
        var t = new ChoreTemplate { id = "prueba", kind = "cocinar", category = ChoreCategory.Comida, title = "Prueba", source = "s", exercises = new[] { a, b } };
        var run = new ChoreRun(t);
        TestProbe.Check("[CS-54] una tarea empieza en su primer reto, con 2 fallos de margen (CodeShip)", run.Current == a && run.Steps == 2 && run.MistakesLeft == 2 && run.Status == ChoreRun.State.Doing);
        run.Submit(CodeAnswer.Choice(0)); run.Submit(CodeAnswer.Choice(0));
        TestProbe.Check("[CS-55] repetir la misma respuesta mala cuenta un fallo, no dos", run.Mistakes == 1 && run.Status == ChoreRun.State.Doing && run.Tried(CodeAnswer.Choice(0)));
        TestProbe.Check("[CS-56] un acierto pasa al siguiente reto", run.Submit(CodeAnswer.Choice(1)) && run.Current == b && run.Step == 1);
        TestProbe.Check("[CS-57] el último acierto la termina", run.Submit(CodeAnswer.Order("p", "q")) && run.Status == ChoreRun.State.Done && run.Current == null);
        var run2 = new ChoreRun(t);
        run2.Submit(CodeAnswer.Choice(0)); run2.Submit(CodeAnswer.Choice(2));
        TestProbe.Check("[CS-58] dos fallos distintos la hacen fallar (hay que reintentar)", run2.Status == ChoreRun.State.Failed && run2.Current == null);
    }

    static void Chains()
    {
        CodeExercise[] ex = ChoreKit.StepChain("cadena", 1, i => $"paso {i + 1}", new[] { "lava las manos", "cocina hasta {71|50|100} °C", "emplata" }, new[] { "tira todo a la basura", "riega la base" });
        TestProbe.Check("[CS-60] un reto por paso", ex.Length == 3);
        CodeExercise v = ex[1];
        TestProbe.Check("[CS-61] «{bien|mal|mal}» pide el valor, con su unidad", v.kind == CodeExercise.Kind.Choice && v.options[v.correct] == "71 °C" && System.Array.IndexOf(v.options, "50 °C") >= 0 && v.question.Contains("valor"), v.options.Length > 0 ? string.Join(" / ", v.options) : "");
        CodeExercise n = ex[2];
        TestProbe.Check("[CS-62] los demás preguntan qué viene ahora, con los dos pasos de antes como código", n.options[n.correct] == "emplata" && n.blocks.Length == 2 && n.blocks[1].pseudo.Contains("cocina hasta 71"), n.blocks.Length > 0 ? n.blocks[n.blocks.Length - 1].pseudo : "");
        bool ok = true;
        foreach (CodeExercise e in ex) if (e.Validate().Length > 0) ok = false;
        TestProbe.Check("[CS-63] todos los retos de la cadena validan", ok);
        CodeExercise c1 = ChoreKit.Choice("estable", 1, CodeSkill.Predecir, "i", "¿?", new CodeBlock[0], "bien", new[] { "mal1", "mal2" });
        CodeExercise c2 = ChoreKit.Choice("estable", 1, CodeSkill.Predecir, "i", "¿?", new CodeBlock[0], "bien", new[] { "mal1", "mal2" });
        TestProbe.Check("[CS-64] la opción buena va en un sitio que solo depende del id (estable)", c1.correct == c2.correct && c1.options[c1.correct] == "bien");
        TestProbe.Check("[CS-65] «lava las hojas» → lavaLasHojas();", ChoreKit.ToCall("lava las hojas") == "lavaLasHojas();" && ChoreKit.ToCall("¡Cocina ya el arroz!") == "cocinaYaElArroz();", ChoreKit.ToCall("¡Cocina ya el arroz!"));
    }

    static void Duel()
    {
        DifficultyProfile e = DifficultyProfile.For(0f), h = DifficultyProfile.For(1f);
        TestProbe.Check("[CS-70] la curva: fácil 3 ideas, paciencia 5, 4 bloques, sin distracciones, concepto 1", e.persuasionGoal == 3 && e.patience == 5 && e.maxBlocks == 4 && !e.distractors && e.maxConcept == 1);
        TestProbe.Check("[CS-71] la curva: difícil 8 ideas, paciencia 2, 10 bloques, con distracciones, concepto 10", h.persuasionGoal == 8 && h.patience == 2 && h.maxBlocks == 10 && h.distractors && h.maxConcept == 10);
        TestProbe.Check("[CS-72] las distracciones llegan a partir de 0,35", !DifficultyProfile.For(0.34f).distractors && DifficultyProfile.For(0.35f).distractors);
        var duel = new IdeaDuel(MissionLibrary.IdeaBank, DifficultyProfile.For(0f), 7);
        TestProbe.Check("[CS-73] el duelo de ideas usa solo retos de su nivel", duel.Current != null && duel.Pool.TrueForAll(x => x.concept <= 1 && ChoreKit.BlockCount(x) <= 4 && !x.HasDistractors), $"{duel.Pool.Count} retos");
        int guard = 0;
        while (duel.Status == ChoreRun.State.Doing && guard++ < 20) duel.Submit(duel.Current.kind == CodeExercise.Kind.Order ? CodeAnswer.Order(duel.Current.solution) : CodeAnswer.Choice(duel.Current.correct));
        TestProbe.Check("[CS-74] acertar las ideas de la meta planta la idea", duel.Status == ChoreRun.State.Done && duel.Step >= System.Math.Min(3, duel.Pool.Count));
        var lose = new IdeaDuel(MissionLibrary.IdeaBank, DifficultyProfile.For(1f), 3);
        guard = 0;
        while (lose.Status == ChoreRun.State.Doing && guard++ < 40)
        {
            CodeExercise c = lose.Current;
            if (c.kind == CodeExercise.Kind.Choice) lose.Submit(CodeAnswer.Choice((c.correct + 1 + guard % (c.options.Length - 1)) % c.options.Length));
            else lose.Submit(CodeAnswer.Order(c.Shuffled(guard)));
        }
        TestProbe.Check("[CS-75] agotar la paciencia pierde el duelo", lose.Status == ChoreRun.State.Failed && lose.Mistakes >= 2, $"{lose.Mistakes} fallos");
    }

    static void Musk()
    {
        List<MissionSites.Binding> b = MissionSites.ForScene("Microcosmos_ElonMusk");
        string[] need = { "cocinar", "atender-paciente", "regar", "ejercicio", "yoga", "alimentar-perro", "mecanizar", "avionica", "composite", "plantar-idea", "dormir", "asearse" };
        string missing = "";
        foreach (string k in need) if (!b.Exists(x => MissionLibrary.Find(x.template) != null && MissionLibrary.Find(x.template).kind == k)) missing += k + " ";
        TestProbe.Check("[CS-80] en Hawthorne cada puesto y cada necesidad tiene su misión", missing.Length == 0, missing.Length > 0 ? "faltan: " + missing : $"{b.Count} misiones");
        string unknown = "";
        foreach (MissionSites.Binding x in b) if (MissionLibrary.Find(x.template) == null) unknown += x.template + " ";
        TestProbe.Check("[CS-81] cada misión de Hawthorne es una plantilla de la biblioteca", unknown.Length == 0, unknown);

        // Los sitios del lote 2 de Hawthorne (docs/microcosmos-musk-historia.md §4, por su nombre exacto) tienen su misión.
        string[] sites = { "Rincón de gimnasio", "Soldadura por fricción (FSW): tanque del Falcon 9", "Laboratorio de materiales", "Sala de datos de pruebas (propulsión)",
                           "Vestuario de la sala limpia", "Dispensador de gel hidroalcohólico", "Cámara frigorífica de la cocina", "Tarjeta de inspección sanitaria (A)",
                           "Furgoneta del proveedor de comida (supuesto)", "Lonchera (camión de comida)", "Garita de seguridad (1 Rocket Road)", "Hidrante", "Bomberos",
                           "Cruce de Crenshaw Boulevard y Rocket Road (semáforos)", "Aparcamiento de SpaceX" };
        string unbound = "";
        foreach (string s in sites) if (!b.Exists(x => System.Array.IndexOf(x.names, s) >= 0)) unbound += $"«{s}» ";
        TestProbe.Check("[CS-82] cada sitio nuevo de Hawthorne (lote 2) tiene su misión, por su nombre exacto", unbound.Length == 0, unbound.Length > 0 ? "sin misión: " + unbound : $"{sites.Length} sitios");
        string[] lot2 = { "soldar-fsw", "ensayo-materiales", "analizar-datos", "sala-limpia", "seguridad-alimentaria", "recepcion", "control-acceso", "incendios", "seguridad-vial", "aeronautica" };
        string lacking = "";
        foreach (string k in lot2) if (!MissionLibrary.All.Exists(t => t.kind == k)) lacking += k + " ";
        TestProbe.Check("[CS-83] las clases nuevas: soldar, ensayos, datos, sala limpia, comida segura, recibir, garita, incendios, cruzar y la pista", lacking.Length == 0, lacking);
    }

    // CodeShip reglas.md §4.2: cada paso de un reto se anima en su sitio, con las animaciones que ya tiene el juego.
    static void Motions()
    {
        StepAnimator.Motion M(string kind, string id = "", int step = 0, int steps = 5) => StepAnimator.MotionFor(kind, id, step, steps);
        bool G(StepAnimator.Motion m, AreaFunctions.Kind k) => m.type == StepAnimator.Motion.Type.Gesture && m.gesture == k;
        TestProbe.Check("[CS-90] cocinar: el gesto de cocinar en el fogón", G(M("cocinar"), AreaFunctions.Kind.Cook));
        TestProbe.Check("[CS-91] el torno y el composite, el gesto del banco; la aviónica, teclear en la consola", G(M("mecanizar"), AreaFunctions.Kind.Make) && G(M("composite"), AreaFunctions.Kind.Make) && G(M("avionica"), AreaFunctions.Kind.Operate));
        StepAnimator.Motion y = M("yoga", "yoga-saludo-al-sol", 6, 12), u = M("yoga", "yoga-upa-cuello", 2, 6);
        TestProbe.Check("[CS-92] el yoga, postura a postura: el reto n pone la postura n de su secuencia", y.type == StepAnimator.Motion.Type.Yoga && y.sequence == "saludo-al-sol" && y.posture == 6 && u.sequence == "upa-cuello" && u.posture == 2, $"{y} / {u}");
        TestProbe.Check("[CS-93] jugar con el perro: un paseo con él", M("jugar-perro").type == StepAnimator.Motion.Type.Walk);
        TestProbe.Check("[CS-94] dormir: tumbarse en el último paso, no antes", M("dormir", "", 5, 6).type == StepAnimator.Motion.Type.Lie && M("dormir", "", 2, 6).type != StepAnimator.Motion.Type.Lie);
        TestProbe.Check("[CS-96] la FSW, el gesto del banco; la garita y el cruce, mirar a un lado y a otro; el extintor, barrer de lado a lado",
            G(M("soldar-fsw"), AreaFunctions.Kind.Make) && G(M("control-acceso"), AreaFunctions.Kind.Guard) && G(M("seguridad-vial"), AreaFunctions.Kind.Guard) && G(M("incendios"), AreaFunctions.Kind.Clean));
        string none = "";
        foreach (ChoreTemplate t in MissionLibrary.All) if (!StepAnimator.Handles(t.kind)) none += t.kind + " ";
        TestProbe.Check("[CS-95] cada clase de misión de la biblioteca tiene su animación", none.Length == 0, none);
    }

    static void Scene()
    {
        Scene s = SceneManager.GetSceneByName("Microcosmos_ElonMusk");
        if (!s.isLoaded) { Debug.Log("[TEST] Microcosmos_ElonMusk no está cargado: se salta la parte de los sitios en la escena."); return; }
        StationMissions sm = StationMissions.Ensure();
        sm.BindScene(s);
        List<StationMission> ms = sm.In(s);
        int found = ms.FindAll(m => m.target != null && !m.fallback).Count, fb = ms.FindAll(m => m.fallback).Count;
        TestProbe.Check("[CS-85] en la escena, cada misión encuentra su sitio (los que faltan, con un marcador «supuesto»)", ms.Count == MissionSites.ForScene(s.name).Count && ms.TrueForAll(m => m.target != null), $"{found} en su mueble, {fb} con marcador: {string.Join(", ", ms.FindAll(m => m.fallback).ConvertAll(m => m.template.title))}");
        TestProbe.Check("[CS-86] los puestos de la nave están en su mueble real (cocina, aviónica, composite, mecanizado)",
            new[] { "mecanizar", "avionica", "composite", "cocinar" }.Length == 4 && System.Array.TrueForAll(new[] { "mecanizar", "avionica", "composite", "cocinar" }, k => ms.Exists(m => m.template.kind == k && !m.fallback)));
        string[] real = { "ejercicio", "yoga", "atender-paciente", "alimentar-perro", "soldar-fsw", "ensayo-materiales", "sala-limpia", "seguridad-alimentaria", "control-acceso", "seguridad-vial" };
        string marker = "";
        foreach (string k in real) if (!ms.Exists(m => m.template.kind == k && !m.fallback)) marker += k + " ";
        TestProbe.Check("[CS-88] con los sitios del lote 2, gimnasio, yoga, botiquín, perro y los puestos nuevos ya no son marcadores «supuesto»", marker.Length == 0, marker);
        StationMission any = ms.Find(m => m.Available);
        if (any != null)
        {
            ChoreRun run = sm.Begin(any, openSheet: false);
            int guard = 0;
            while (run.Status == ChoreRun.State.Doing && guard++ < 40) run.Submit(run.Current.kind == CodeExercise.Kind.Order ? CodeAnswer.Order(run.Current.solution) : CodeAnswer.Choice(run.Current.correct));
            sm.End(any, run);
            TestProbe.Check("[CS-87] resolver sus retos cumple la misión del puesto y la quita de pendientes", any.timesDone == 1 && !any.Available, any.template.title);
        }
    }
}
