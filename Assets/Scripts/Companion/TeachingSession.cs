using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ENSEÑANZA INTERACTIVA (call-and-response). El maestro ejecuta UN paso con su cuerpo, le PIDE al alumno que lo haga
/// (aviso en pantalla + el hechizo a lanzar) y **espera** a que el alumno lo realice antes de pasar al siguiente. Sustituye
/// la mímica pura (el alumno ya no solo copia: se le pide y se comprueba). Reutilizable: yoga (Magnate), limpieza y recetas
/// (Goluis) — cualquier secuencia de <see cref="AsanaPoseSpell"/> o de pasos con predicado de "hecho".
///
/// El alumno IA realiza el paso solo; el alumno JUGADOR lo lanza él (consola/teclas) y, si no lo lanza a tiempo
/// (<see cref="stepTimeout"/>), su cuerpo lo hace solo. Se avanza cuando el alumno lo está haciendo y su cuerpo COINCIDE
/// con la pose (tolerancia en grados), o tras un tope de seguridad.
/// Berón 2026-10-29: en la clase de upa-yoga «no seguían las instrucciones del cuello». Las posturas iban por las colas
/// de acciones de cada uno, detrás de lo que tuvieran encolado, y los pasos «cuello recto» se daban por hechos al instante
/// sin que nadie los hiciera. Ahora la sesión mueve ELLA MISMA el cuerpo del maestro y el del alumno (cuando lo lanza),
/// cada fotograma y con el hueso bloqueado: ni el andar, ni la emoción, ni otra postura lo pisan.
/// </summary>
public class TeachingSession : MonoBehaviour
{
    public Anima teacher, student;
    public string skillId = "yoga";
    public float matchTolerance = 30f;   // grados de margen para dar un paso por "realizado"
    public float stepTimeout = 14f;      // si el jugador no lo lanza en este tiempo, su cuerpo lo hace solo
    [Tooltip("Lo mínimo que dura un paso (s): el maestro lo demuestra aunque el alumno ya estuviera en esa postura.")]
    public float minStep = 2f;

    List<AsanaPoseSpell> _teacherSteps, _studentSteps;
    int _i = -1;
    float _stepStart;
    bool _studentIsAI, _cast, _autoThis;

    /// <summary>Cómo acabó cada paso de cada clase (para las pruebas): cuánto le faltaba a la cabeza del maestro y del alumno
    /// (grados) al darlo por hecho, si el cuerpo del alumno lo hizo solo y si fue por el tope.</summary>
    public struct StepResult { public Anima student; public int index; public string name; public float teacherOff, studentOff; public bool auto, timedOut; }
    public static readonly List<StepResult> Results = new List<StepResult>();

    /// <summary>El paso en curso (−1 antes de empezar) y cuántos hay.</summary>
    public int StepIndex => _i;
    public int StepCount => _teacherSteps != null ? _teacherSteps.Count : 0;
    /// <summary>La postura que el maestro demuestra ahora (null fuera de la clase).</summary>
    public AsanaPoseSpell CurrentStep => _teacherSteps != null && _i >= 0 && _i < _teacherSteps.Count ? _teacherSteps[_i] : null;
    /// <summary>¿El alumno ya está haciendo el paso actual (lo lanzó o se le hizo)?</summary>
    public bool StudentDoing => _cast;
    /// <summary>Pasos que el cuerpo del jugador hizo solo porque no los lanzó a tiempo.</summary>
    public int AutoSteps { get; private set; }

    /// <summary>Arranca una lección: el maestro y el alumno reciben COPIAS frescas de la secuencia (estado independiente).</summary>
    public static TeachingSession Begin(Anima teacher, Anima student, string skillId, System.Func<List<QueuedSpell>> build)
    {
        if (teacher == null || student == null || build == null) return null;
        TeachingSession ts = teacher.gameObject.AddComponent<TeachingSession>();
        ts.teacher = teacher; ts.student = student; ts.skillId = skillId;
        ts._teacherSteps = OnlyPoses(build());
        ts._studentSteps = OnlyPoses(build());
        return ts;
    }

    static List<AsanaPoseSpell> OnlyPoses(List<QueuedSpell> l)
    {
        var r = new List<AsanaPoseSpell>();
        if (l != null) foreach (var s in l) if (s is AsanaPoseSpell a) r.Add(a);
        return r;
    }

    void Start()
    {
        // Lo que el maestro tuviera encolado (su jornada, otra postura) espera: la cola lo ponía en otra postura encima.
        if (teacher != null && teacher.TryGetComponent(out ActionQueue tq)) tq.ClearOwn();
        // Sin PlayerBrain ni PlayerController = lo maneja la IA (el jugador de la iniciación no siempre tiene PlayerBrain: se le
        // trataba como IA, no se le pedía nada y su cuerpo hacía los pasos solo).
        _studentIsAI = student == null || (student.GetComponent<PlayerBrain>() == null && student.GetComponent<PlayerController>() == null);
        Next();
    }

    void Next()
    {
        _i++;
        if (_teacherSteps == null || _i >= _teacherSteps.Count) { Finish(); return; }
        _stepStart = Time.time;
        _cast = _studentIsAI; _autoThis = false;   // la IA lo hace sola, a la vez que el maestro; el jugador, cuando lo lance
        // Se le pide al alumno el PASO concreto, con su hechizo listo para lanzar (la consola lo propone: Intro lo lanza).
        string spell = $"asana({{name:\"{_teacherSteps[_i].asanaName}\"}})";
        // El rótulo en pantalla es solo para el JUGADOR: antes cualquier clase entre otros (la Magnate con un rezagado) lo mostraba.
        if (!_studentIsAI) LessonPrompt.Show(_teacherSteps[_i].asanaName, spell);
        if (!_studentIsAI) SpellConsole.Propose(spell, $"{TeacherName} te pide: {_teacherSteps[_i].asanaName}");
    }

    /// <summary>El alumno lanza el paso que se le pide (hechizo `asana`): su cuerpo lo empieza a hacer.</summary>
    public bool CastCurrentStep(Anima who)
    {
        if (who != student || _studentSteps == null || _i < 0 || _i >= _studentSteps.Count) return false;
        _cast = true;
        return true;
    }

    // Quién enseña (su nombre en la jornada: la Magnate, el de turno de la sala de yoga…).
    string TeacherName { get { Mind m = teacher != null ? teacher.GetComponent<Mind>() : null; return m != null && !string.IsNullOrEmpty(m.identity) ? m.identity : (teacher != null ? teacher.name : "El maestro"); } }

    public static TeachingSession For(Anima student)
    {
        foreach (TeachingSession ts in FindObjectsByType<TeachingSession>(FindObjectsSortMode.None)) if (ts.student == student) return ts;
        return null;
    }

    void Update()
    {
        if (_i < 0 || _teacherSteps == null || _i >= _teacherSteps.Count) return;
        if (student == null || teacher == null) { Finish(); return; }
        float t = Time.time - _stepStart;
        AsanaPoseSpell step = _teacherSteps[_i];
        // El maestro DEMUESTRA el paso cada fotograma (Tick la sigue aplicando ya llegada, con los huesos bloqueados).
        step.Tick(teacher);
        if (!_cast && t > stepTimeout)
        {
            _cast = true; AutoSteps++; _autoThis = true;
            Debug.Log($"[Clase] «{student.name}» no lanzó «{step.asanaName}» a tiempo: su cuerpo lo hace con {TeacherName}.");
        }
        if (_cast && _i < _studentSteps.Count) _studentSteps[_i].Tick(student);
        // Los dos: el alumno lo hizo y el maestro lo terminó de demostrar (si no, a veces pasaba al siguiente con el maestro a medias).
        bool done = (_cast && t >= minStep && PoseMatch(student, step, matchTolerance) && PoseMatch(teacher, step, matchTolerance))
                    || t > stepTimeout + step.duration + 4f;   // tope de seguridad (no se atasca nunca)
        if (!done) return;
        Results.Add(new StepResult { student = student, index = _i, name = step.asanaName, teacherOff = HeadOff(teacher, step), studentOff = HeadOff(student, step), auto = _autoThis, timedOut = t > stepTimeout + step.duration + 4f });
        Debug.Log($"[Clase] paso {_i + 1}/{_teacherSteps.Count} «{step.asanaName}» hecho en {t:0.0} s ({(t > stepTimeout + step.duration + 4f ? "tope" : "lo hizo")}): cabeza del maestro a {HeadOff(teacher, step):0}°, del alumno a {HeadOff(student, step):0}°{(_studentIsAI ? " (alumno IA)" : "")}.");
        Next();   // el maestro ESPERA hasta que el alumno realiza el paso
    }

    void Finish()
    {
        if (student != null)
        {
            if (!student.TryGetComponent(out Grimoire g)) g = student.gameObject.AddComponent<Grimoire>();
            g.Learn(skillId);   // aprende la secuencia al completarla (ya no es un regalo instantáneo)
        }
        // Fin de la clase: los dos se levantan (si la última postura era sentada se quedaban así andando, empujando…).
        if (student != null) DailyPoses.Stand().ApplyFinal(student);
        if (teacher != null) DailyPoses.Stand().ApplyFinal(teacher);
        if (!_studentIsAI) LessonPrompt.Hide();
        _i = _teacherSteps != null ? _teacherSteps.Count : 0;
        Destroy(this);
    }

    // Cuánto le falta a la cabeza (grados) para la postura del paso: para el registro de la clase.
    static float HeadOff(Anima a, AsanaPoseSpell pose)
    {
        CreatureRig rig = a != null ? a.GetComponent<CreatureRig>() : null;
        Transform h = rig != null ? rig.Get(BodyPart.Head) : null;
        if (h == null || pose == null) return -1f;
        foreach (var p in pose.parts) if (p.Item1 == BodyPart.Head) return Quaternion.Angle(h.localRotation, Quaternion.Euler(p.Item2));
        return 0f;
    }

    /// <summary>¿El cuerpo de <paramref name="a"/> coincide con la pose (en los huesos que fija) dentro de la tolerancia?</summary>
    public static bool PoseMatch(Anima a, AsanaPoseSpell pose, float tol)
    {
        if (a == null || pose == null) return true;
        CreatureRig rig = a.GetComponent<CreatureRig>();
        if (rig == null) return true;
        foreach (var p in pose.parts)
        {
            Transform b = rig.Get(p.Item1);
            if (b == null) continue;
            if (Quaternion.Angle(b.localRotation, Quaternion.Euler(p.Item2)) > tol) return false;
        }
        return true;
    }
}
