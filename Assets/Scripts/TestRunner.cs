using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Tooltip("Solo estos grupos (vacío = todos). Las escenas de prueba de un área (Assets/Scenes/Pruebas, docs/escenas-de-prueba.md) " +
             "corren solo las pruebas de su piso: las demás unidades, aunque se registren solas al pedir las pruebas, no corren.")]
    public int[] onlyGroups = new int[0];

    [Tooltip("Lista de unidades (vacío = todas): «rapida» = las baratas (Resources/TestPresets/rapida.txt). También por Temp/runner-preset.txt (una vez).")]
    public string preset = "";
    /// <summary>Lista de una sola vez: si existe, su texto es la lista (p. ej. «rapida») y se borra al leerla.</summary>
    public const string PresetFile = "Temp/runner-preset.txt";

    [Tooltip("Tope por unidad (s reales): pasado, «[TEST] TIMEOUT unidad X» y sigue la batería.")]
    public float unitTimeout = 1500f;

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
        // Quien la jornada apagó al cargar la escena (pasa el día en otro santuario: Lizmibet y Linduarda, casa en el santuario 2) se quedaba
        // apagado toda la batería: la jornada lo aplica antes de que esto empiece, y solo vuelve a mirar al cambiar de día. Con las pruebas,
        // nadie apagado (RemoteStaff): «Lizmibet está en el santuario», sus series del gimnasio y su entrenamiento en la jornada, 0.
        RemoteStaff.RestoreAll();
        try { System.IO.File.Delete(RequestFile); } catch (System.Exception) { }   // una vez: el siguiente Play es para jugar
        yield return new WaitForSeconds(startDelay);
        TestProbe.ResetTotals();

        // Reunir unidades (en este GO y sus hijos) y ordenarlas por grupo.
        try { if (System.IO.File.Exists(PresetFile)) { preset = System.IO.File.ReadAllText(PresetFile).Trim(); System.IO.File.Delete(PresetFile); } } catch (System.Exception) { }
        HashSet<string> only = string.IsNullOrEmpty(preset) ? null : TestPresets.Units(preset);
        if (only != null) Debug.Log($"[TEST] Lista «{preset}»: {only.Count} unidades.");
        List<ITestUnit> units = new List<ITestUnit>();
        foreach (MonoBehaviour mb in GetComponentsInChildren<MonoBehaviour>())
            if (mb is ITestUnit u && (onlyGroups == null || onlyGroups.Length == 0 || System.Array.IndexOf(onlyGroups, u.Group) >= 0) && (only == null || only.Contains(mb.GetType().Name))) units.Add(u);
        TestResults.Reset();
        units.Sort((a, b) => a.Group.CompareTo(b.Group));

        Debug.Log($"[TEST] ══════ TestRunner: {units.Count} unidad(es) ══════");

        StartCoroutine(Alimentador());
        int i = 0;
        while (i < units.Count)
        {
            int g = units[i].Group;
            _groupNow = g; CurrentGroup = g;
            List<ITestUnit> group = new List<ITestUnit>();
            while (i < units.Count && units[i].Group == g) { group.Add(units[i]); i++; }

            _scale = Time.timeScale; _maxDelta = Time.maximumDeltaTime;
            bool allParallel = group.Count > 1;
            foreach (ITestUnit u in group) if (!u.ParallelSafe) { allParallel = false; break; }

            if (allParallel)
            {
                TestProbe.Context(g, "paralelo");
                int p0 = TestProbe.GrandPass, f0 = TestProbe.GrandFail, n0 = TestProbe.Fails.Count; float t0 = Time.realtimeSinceStartup;
                List<Coroutine> running = new List<Coroutine>();
                foreach (ITestUnit u in group) running.Add(StartCoroutine(Guard(u, null)));
                foreach (Coroutine c in running) yield return c;   // barrera: esperar a todo el grupo
                var row = new TestResults.Row { group = g, unit = "paralelo: " + string.Join(",", group.Select(x => x.GetType().Name)), pass = TestProbe.GrandPass - p0, fail = TestProbe.GrandFail - f0, seconds = Time.realtimeSinceStartup - t0 };
                row.fails.AddRange(TestProbe.Fails.Skip(n0)); row.status = row.fail > 0 ? "FALLOS" : "OK"; TestResults.Add(row);
            }
            else
            {
                foreach (ITestUnit u in group) yield return RunUnit(u);   // en serie, cada una con su tope y su fila de resultados
            }
            Sanear(g);
            Alimentar();
        }

        TestProbe.Context(0, "");
        TestProbe.Total();
        TestResults.Write();
        Debug.Log($"[TEST] Resultados por unidad → {TestResults.Path} ({TestResults.Rows.Count} filas).");
        Running = false;
    }

    // UNA UNIDAD, con su contexto (cada FAIL dice grupo y unidad), su tope de tiempo, su tiempo de juego rápido si lo pide, sus excepciones
    // contadas como FAIL (antes una excepción cortaba en silencio el final de la unidad) y su fila en el archivo de resultados.
    IEnumerator RunUnit(ITestUnit u)
    {
        string name = u.GetType().Name;
        TestProbe.Context(u.Group, name);
        int p0 = TestProbe.GrandPass, f0 = TestProbe.GrandFail, n0 = TestProbe.Fails.Count; float t0 = Time.realtimeSinceStartup;
        float maxDeltaWas = Time.maximumDeltaTime;
        if (u is IFastGameTime fast && fast.MaxDeltaTime > Time.maximumDeltaTime)
        {
            Time.maximumDeltaTime = Mathf.Min(1f, fast.MaxDeltaTime);
            Debug.Log($"[TEST] {name}: tiempo de juego rápido, maximumDeltaTime {maxDeltaWas:0.###} → {Time.maximumDeltaTime:0.###} (×{Time.maximumDeltaTime / Mathf.Max(0.001f, maxDeltaWas):0.0} con pocos FPS).");
        }
        var state = new UnitState();
        Coroutine c = StartCoroutine(Guard(u, state));
        while (!state.done && Time.realtimeSinceStartup - t0 < unitTimeout) yield return null;
        string status = null;
        if (!state.done)
        {
            StopCoroutine(c);
            if (u is MonoBehaviour mb && mb != null) mb.StopAllCoroutines();
            Debug.LogError($"[TEST] TIMEOUT unidad {name} (grupo {u.Group}) a los {unitTimeout:0} s: se para y sigue la batería.");
            TestProbe.Check($"la unidad acaba en menos de {unitTimeout:0} s", false, "TIMEOUT");
            TestProbe.End();
            status = "TIMEOUT";
        }
        else if (state.error != null) status = "EXCEPCION";
        Time.maximumDeltaTime = maxDeltaWas;
        var row = new TestResults.Row { group = u.Group, unit = name, pass = TestProbe.GrandPass - p0, fail = TestProbe.GrandFail - f0, seconds = Time.realtimeSinceStartup - t0 };
        row.fails.AddRange(TestProbe.Fails.Skip(n0));
        row.status = status ?? (row.fail > 0 ? "FALLOS" : "OK");
        TestResults.Add(row);
        TestProbe.Context(0, "");
    }

    sealed class UnitState { public bool done; public System.Exception error; }

    // Recorre la corrutina de la unidad a mano: una excepción se registra como FAIL (con su unidad) en vez de cortar la unidad en silencio.
    IEnumerator Guard(ITestUnit u, UnitState state)
    {
        IEnumerator e = null;
        try { e = u.Run(); } catch (System.Exception ex) { Fail(u, ex, state); }
        while (e != null)
        {
            object cur;
            try { if (!e.MoveNext()) break; cur = e.Current; }
            catch (System.Exception ex) { Fail(u, ex, state); break; }
            yield return cur;
        }
        if (state != null) state.done = true;
    }

    static void Fail(ITestUnit u, System.Exception ex, UnitState state)
    {
        if (state != null) state.error = ex;
        Debug.LogException(ex);
        TestProbe.Check($"la unidad {u.GetType().Name} corre sin excepciones", false, $"{ex.GetType().Name}: {ex.Message}");
        TestProbe.End();
    }

    // EL ALIMENTADOR: durante la batería WorkdayDirector.Update no corre (Running), así que nadie tiene la misión de alimentar a los depredadores de tierra y
    // pasan horas de juego sin comer: los adultos morían de hambre a media batería y las pruebas 99 y 105 no encontraban la población. Entre grupos se hace su trabajo.
    // Entre grupos no bastaba: un grupo largo dejaba a los osos con hambre 45 y sin masa («Servicio del restaurante en vivo»). Cada 5 s reales; no en la jornada (50), que
    // lleva a su alimentador de verdad, ni en la prueba del alimentador (105), que necesita a los animales con hambre.
    int _groupNow;
    /// <summary>El grupo que corre ahora (0 si las unidades las lanza otro arnés, como SoloGrupos).</summary>
    public static int CurrentGroup { get; private set; }
    static float _fedAt = -999f;
    /// <summary>El alimentador de la batería, cada 5 s reales, lo llame quien lo llame (esta corrutina o, con otro arnés que pone Running sin pasar por
    /// Start, WorkdayDirector.Update: con SoloGrupos los osos adultos morían de hambre, 43–46, en el primer grupo largo de cada tanda).</summary>
    public static void FeedBetween()
    {
        if (!Running || CurrentGroup == 50 || CurrentGroup == 105 || Time.realtimeSinceStartup - _fedAt < 5f) return;
        _fedAt = Time.realtimeSinceStartup; Alimentar();
    }
    IEnumerator Alimentador()
    {
        while (Running)
        {
            FeedBetween();
            yield return new WaitForSecondsRealtime(5f);
        }
    }

    static void Alimentar()
    {
        foreach (GameObject go in Animal.wholePopulation)
        {
            if (go == null || !go.TryGetComponent(out Animal a) || a.death || a.lifeStage == LifeStage.child && !(a.Forage != null && a.Forage.eatsPrey)) continue;
            if (a.lifeStage == LifeStage.child || PredatorRound.Served(a)) a.hungry = Mathf.Min(a.hungry, 0f);
        }
    }

    // LO QUE UN GRUPO DEJA MAL (una excepción en una corrutina se la salta su final): se dice y se deja como estaba, para que no lo pague el grupo siguiente.
    float _scale, _maxDelta;
    void Sanear(int group)
    {
        if (GameKeys.DialogOpen) { Debug.LogError($"[TEST] FUGA del grupo {group}: se quedó abierto {GameKeys.OpenDialogs}."); GameKeys.CloseDialogs(); }   // (ronda 28: «Esc la cierra» fallaba con un diálogo de otro grupo abierto)
        if (!Mathf.Approximately(Time.timeScale, _scale)) { Debug.LogError($"[TEST] FUGA del grupo {group}: Time.timeScale {Time.timeScale} (era {_scale})."); Time.timeScale = _scale; }
        if (!Mathf.Approximately(Time.maximumDeltaTime, _maxDelta)) { Debug.LogError($"[TEST] FUGA del grupo {group}: maximumDeltaTime {Time.maximumDeltaTime} (era {_maxDelta})."); Time.maximumDeltaTime = _maxDelta; }
        WorkdayDirector wd = FindAnyObjectByType<WorkdayDirector>(FindObjectsInactive.Include);
        if (wd != null && !wd.enabled) { Debug.LogError($"[TEST] FUGA del grupo {group}: la jornada se quedó apagada."); wd.enabled = true; }
    }
}
}
