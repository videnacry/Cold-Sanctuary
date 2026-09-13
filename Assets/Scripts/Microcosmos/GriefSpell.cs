using UnityEngine;

/// <summary>
/// DUELO (docs/apremios-guardian-observacion.md §3, microcosmos-level1 §Beats) — el hechizo-estado de la pérdida: cuando
/// muere un vínculo (lo dispara el `Level1Director`/quien corresponda con `Cast(self, elMuerto)`), el ser CARGA tristeza
/// a intervalos (`EffectTiming.Periodic`) durante `duration`: sube su carga alostática y su estrés (baja la valencia →
/// la emoción lo lee). No se "cura" solo con el tiempo; el acompañamiento/observación lo amortigua (Guardián). `force` =
/// peso del duelo. Es el cierre emocional del tableau de la muerte de Ambrosio.
/// </summary>
public class GriefSpell : SpellBase
{
    Anima _self;
    float _until;
    float _nextTick;

    void Awake()
    {
        _self = GetComponent<Anima>();
        if (timing == EffectTiming.Instant) timing = EffectTiming.Periodic;
        if (duration <= 0f) duration = 12f;
    }

    public override bool CanCast(Anima caster, ITarget target) => (caster ?? _self) != null;

    public override void Cast(Anima caster, ITarget target)
    {
        if (caster != null) _self = caster;
        _until = Time.time + duration;
        _nextTick = 0f;   // dispara ya en el próximo Update
    }

    void Update()
    {
        if (_self == null || _self.death || Time.time >= _until) return;
        if (Time.time < _nextTick) return;
        _nextTick = Time.time + tickInterval;
        AllostaticState.Of(_self).AddLoad(force);                       // el duelo pesa (carga externa)
        _self.stress = Mathf.Min(1f, _self.stress + force * 0.2f);
    }
}
