using UnityEngine;

/// <summary>
/// ZEUS en Egina (docs/microcosmos-myrmidons.md), en su versión humana: no hace magia; es un padre con muchos contactos
/// y mucha influencia que, conmovido por el sueño de su hijo, va a ver a cada insecto-humano que conoce y le pide que se
/// una a construir un reino de virtud. Va al más cercano que esté dispuesto, habla con él un rato y lo suma. Si nadie
/// está dispuesto todavía, vuelve junto a su hijo bajo la encina.
/// </summary>
public class MythRecruiter : MonoBehaviour
{
    public Transform home;
    public float talkSeconds = 3f;
    public float speed = 2.6f;

    MythRecruit _target; float _talkUntil = -1f;

    void Update()
    {
        MythMission m = MythMission.Instance;
        if (m == null || m.CardOpen) return;
        if (_target == null || !_target.CanJoin(m.JoinedFraction, m.Stage)) { _target = Pick(m); _talkUntil = -1f; }
        if (_target == null) { if (home != null) MicroMove.Step(transform, home.position, speed, 2f); return; }
        if (!MicroMove.Step(transform, _target.transform.position, speed, 1.8f)) return;
        if (_talkUntil < 0f) { _talkUntil = Time.time + talkSeconds; m.Say("Zeus", $"«{_target.displayName}, mi hijo sueña con un reino justo. Ven a construirlo con nosotros.»"); return; }
        if (Time.time < _talkUntil) return;
        m.Recruit(_target, "Zeus");
        _target = null; _talkUntil = -1f;
    }

    MythRecruit Pick(MythMission m)
    {
        MythRecruit best = null; float bd = float.MaxValue;
        foreach (MythRecruit r in m.Recruits)
        {
            if (r == null || !r.CanJoin(m.JoinedFraction, m.Stage)) continue;
            float d = (r.transform.position - transform.position).sqrMagnitude;
            if (d < bd) { bd = d; best = r; }
        }
        return best;
    }
}
