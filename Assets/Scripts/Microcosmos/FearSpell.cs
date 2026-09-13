using UnityEngine;

/// <summary>
/// Hechizo de MIEDO (docs/sanctuary-second-lap-and-fear.md, apremios §3): inyecta PAVOR — una carga alostática
/// (`AllostaticState.AddLoad`) en el/los objetivo(s), **resistida por su temple** (composure), que sube su estrés. Con
/// `range>0` es AoE (afecta a todo ánima en el radio); con `range=0`, solo al objetivo. Su <see cref="EffectTiming"/>:
/// `Instant` = un susto puntual; `Periodic` = terror que REINCIDE cada `tickInterval` durante `duration` (pesadilla).
/// `force` = magnitud del miedo. No baja el apremio: es sufrimiento puro (el eje oscuro de la Magnate).
/// </summary>
public class FearSpell : SpellBase
{
    [Tooltip("Cuánto del miedo se convierte en estrés inmediato (además de la carga alostática).")]
    [Range(0f, 1f)] public float stressSpike = 0.3f;

    float _until;

    void Awake() { if (timing == EffectTiming.Instant && duration > 0f) timing = EffectTiming.Periodic; }

    public override bool CanCast(Anima caster, ITarget target) => target != null && !target.Dead;

    public override void Cast(Anima caster, ITarget target)
    {
        _caster = caster;
        Apply(caster, target);
        if (timing != EffectTiming.Instant && duration > 0f) _until = Time.time + duration;   // reincide en Update
    }

    Anima _caster;
    ITarget _target;

    void Apply(Anima caster, ITarget target)
    {
        _target = target;
        if (range > 0f)
        {
            Vector3 c = caster != null ? caster.transform.position : transform.position;
            foreach (Collider col in Physics.OverlapSphere(c, range))
            {
                Anima a = col.GetComponentInParent<Anima>();
                if (a != null && a != caster) Frighten(a);
            }
        }
        else
        {
            Anima a = target != null ? target.transform.GetComponentInParent<Anima>() : null;
            if (a != null) Frighten(a);
        }
    }

    internal void Frighten(Anima a)
    {
        float resist = 1f / (1f + Mathf.Max(0f, a.composure));   // el TEMPLE resiste el miedo
        float load = force * resist;
        AllostaticState.Of(a).AddLoad(load);
        a.stress = Mathf.Min(1f, a.stress + load * stressSpike);
    }

    void Update()
    {
        if (timing == EffectTiming.Periodic && Time.time < _until)
        {
            // reincide a intervalos (usa el _repeatTimer implícito por tick simple)
            if (Time.time >= _nextTick) { _nextTick = Time.time + tickInterval; Apply(_caster, _target); }
        }
    }
    float _nextTick;
}
