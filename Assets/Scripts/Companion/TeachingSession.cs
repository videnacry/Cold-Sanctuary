using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ENSEÑANZA INTERACTIVA (call-and-response). El maestro ejecuta UN paso con su cuerpo, le PIDE al alumno que lo haga
/// (aviso en pantalla + el hechizo a lanzar) y **espera** a que el alumno lo realice antes de pasar al siguiente. Sustituye
/// la mímica pura (el alumno ya no solo copia: se le pide y se comprueba). Reutilizable: yoga (Magnate), limpieza y recetas
/// (Goluis) — cualquier secuencia de <see cref="AsanaPoseSpell"/> o de pasos con predicado de "hecho".
///
/// El alumno IA realiza el paso solo (se le encola); el alumno JUGADOR lo lanza él (consola/teclas). Se avanza cuando el
/// cuerpo del alumno COINCIDE con la pose del maestro (tolerancia en grados) o cuando vence un tiempo de seguridad.
/// </summary>
public class TeachingSession : MonoBehaviour
{
    public Anima teacher, student;
    public string skillId = "yoga";
    public float matchTolerance = 30f;   // grados de margen para dar un paso por "realizado"
    public float stepTimeout = 14f;      // piso de seguridad si el alumno no llega

    List<AsanaPoseSpell> _teacherSteps, _studentSteps;
    int _i = -1;
    float _stepStart;
    bool _studentIsAI;
    ActionQueue _tq, _sq;

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
        if (!teacher.TryGetComponent(out _tq)) _tq = teacher.gameObject.AddComponent<ActionQueue>();
        if (!student.TryGetComponent(out _sq)) _sq = student.gameObject.AddComponent<ActionQueue>();
        _studentIsAI = student.GetComponent<PlayerBrain>() == null;   // sin PlayerBrain = lo maneja la IA
        Next();
    }

    void Next()
    {
        _i++;
        if (_teacherSteps == null || _i >= _teacherSteps.Count) { Finish(); return; }
        _stepStart = Time.time;
        _tq.EnqueueOwn(_teacherSteps[_i]);                                   // el maestro demuestra el paso
        if (_studentIsAI && _i < _studentSteps.Count) _sq.EnqueueOwn(_studentSteps[_i]);   // la IA lo hace sola
        // Se le pide al alumno el PASO concreto, con su hechizo listo para lanzar (la consola lo propone: Intro lo lanza).
        string spell = $"asana({{name:\"{_teacherSteps[_i].asanaName}\"}})";
        // El rótulo en pantalla es solo para el JUGADOR: antes cualquier clase entre otros (la Magnate con un rezagado) lo mostraba.
        if (!_studentIsAI) LessonPrompt.Show(_teacherSteps[_i].asanaName, spell);
        if (!_studentIsAI) SpellConsole.Propose(spell, $"La Magnate te pide: {_teacherSteps[_i].asanaName}");
    }

    /// <summary>El alumno lanza el paso que se le pide (hechizo `asana`): se le encola su copia del paso actual.</summary>
    public bool CastCurrentStep(Anima who)
    {
        if (who != student || _studentSteps == null || _i < 0 || _i >= _studentSteps.Count) return false;
        ActionQueue q = who.GetComponent<ActionQueue>();
        if (q == null) q = who.gameObject.AddComponent<ActionQueue>();
        q.EnqueuePlayer(_studentSteps[_i]);
        return true;
    }

    public static TeachingSession For(Anima student)
    {
        foreach (TeachingSession ts in FindObjectsByType<TeachingSession>(FindObjectsSortMode.None)) if (ts.student == student) return ts;
        return null;
    }

    void Update()
    {
        if (_i < 0 || _teacherSteps == null || _i >= _teacherSteps.Count) return;
        if (student == null || teacher == null) { Finish(); return; }
        bool done = PoseMatch(student, _teacherSteps[_i], matchTolerance) || Time.time - _stepStart > stepTimeout;
        if (done) Next();   // el maestro ESPERA hasta que el alumno realiza el paso (o vence el piso de seguridad)
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
        Destroy(this);
    }

    // ¿El cuerpo del alumno coincide con la pose objetivo (en los huesos que la pose fija) dentro de la tolerancia?
    static bool PoseMatch(Anima a, AsanaPoseSpell pose, float tol)
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
