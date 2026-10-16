using UnityEngine;

/// <summary>
/// GOLUIS, el JEFE de la cocina (docs/kitchen-simulation.md). Como Linduarda con la salud y la Magnate con el yoga, su
/// **pensamiento custom** es INICIAR a los recién llegados en la cocina: a quien la Magnate ya presentó, le lanza «sígueme»
/// (<see cref="FollowSpell"/>), lo trae a la cocina y le ENSEÑA —por <see cref="TeachSpell"/>, respetando requisitos
/// (primero limpiar, luego las recetas)— lo que él sabe (es omnisciente de cocina: `Grimoire.knowsAll`). "Conocer = poder
/// enseñar". Al terminar, marca `kitchenTrained`. Emergente, no scriptado. (La enseñanza paso-a-paso interactiva de la
/// limpieza/recetas puede migrar luego a <see cref="TeachingSession"/>, como el yoga.)
/// </summary>
[RequireComponent(typeof(Anima))]
public class KitchenBoss : MonoBehaviour
{
    [Min(0.3f)] public float reach = 2.5f;
    [Min(1f)] public float surveyInterval = 10f;
    [Min(0.2f)] public float teachEvery = 2f;
    [Tooltip("Qué enseña, en orden (requisitos en SpellCatalog: las recetas necesitan 'clean').")]
    public string[] teaches = { "clean", "dish:huevos-revueltos", "dish:ensalada", "dish:avena" };

    Anima _a;
    Grimoire _g;
    FollowSpell _follow;
    TeachSpell _teach;
    SanctuaryOnboarding _student;
    float _next, _teachNext;
    int _i;

    void Awake()
    {
        _a = GetComponent<Anima>();
        _g = GetComponent<Grimoire>() ?? gameObject.AddComponent<Grimoire>();
        _g.knowsAll = true;   // el jefe sabe todo lo de la cocina → puede enseñarlo
        _follow = GetComponent<FollowSpell>() ?? gameObject.AddComponent<FollowSpell>();
        _teach = GetComponent<TeachSpell>() ?? gameObject.AddComponent<TeachSpell>();
        _next = Time.time + 2f;
    }

    void Update()
    {
        if (_a == null || _a.death) return;
        if (_student == null && Time.time >= _next) { _next = Time.time + surveyInterval; _student = FindNewcomer(); if (_student != null) Greet(); }
        if (_student != null) Guide();
    }

    // Recién llegado ya PRESENTADO por la Magnate y aún sin formar en cocina.
    SanctuaryOnboarding FindNewcomer()
    {
        SanctuaryOnboarding best = null; float bestSq = float.MaxValue;
        foreach (SanctuaryOnboarding o in FindObjectsOfType<SanctuaryOnboarding>())
        {
            if (o == null || !o.presented || o.kitchenTrained) continue;
            float sq = (o.transform.position - transform.position).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = o; }
        }
        return best;
    }

    void Greet()
    {
        _i = 0; _teachNext = Time.time + teachEvery;
        ITarget it = _student.GetComponentInParent<ITarget>();
        if (it != null) _follow.Cast(_a, it);   // «sígueme a la cocina»
        Debug.Log($"[Goluis] recibe a «{_student.name}» en la cocina.");
    }

    void Guide()
    {
        Anima t = _student.GetComponentInParent<Anima>();
        if (t == null) { _student = null; return; }
        // Goluis se queda en la cocina; el alumno lo sigue. Enseña cuando ya está cerca.
        if ((t.transform.position - transform.position).magnitude > reach) return;

        if (_i < teaches.Length)
        {
            if (Time.time < _teachNext) return;
            _teachNext = Time.time + teachEvery;
            _teach.skillId = teaches[_i];
            ITarget it = _student.GetComponentInParent<ITarget>();
            if (it != null && _teach.CanCast(_a, it)) _teach.Cast(_a, it);   // enseña (valida requisitos dentro)
            _i++;
            return;
        }

        FollowSpell.Stop(t);
        _student.kitchenTrained = true;
        Debug.Log($"[Goluis] «{t.name}» ya sabe limpiar y las recetas base.");
        _student = null;
    }
}
