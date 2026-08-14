using UnityEngine;

/// <summary>
/// CAMPO SOCIAL — efecto EMERGENTE y GLOBAL (docs/soul-relations-reincarnation §2b). Cada anima **contagia su
/// ánimo** a las animas cercanas **con las que tiene bond**, según **sus propios stats+humores** (no dirigido al
/// jugador — el jugador es una anima más). Generaliza `ThoughtField`. Así la "actitud" no se hardcodea: la
/// "fiesta" de Gohageneis **emerge** de su alta `sociability` + humores altos (positividad/energía) + sus bonds →
/// sube el ánimo (serotonina) y la energía (adrenalina) de sus vecinos de **alto bond y bajo threat**. Un vecino
/// que le **teme** (threat) o con bond bajo, no se contagia. Opt-in; requiere `Mind` (para los humores).
/// </summary>
public class SocialField : MonoBehaviour
{
    public Anima anima;
    public float radius = 6f;
    [Tooltip("Fuerza del contagio/seg (escala por bond × sociabilidad × mi positividad+energía).")]
    public float influence = 0.05f;
    [Tooltip("Bond mínimo (0–100) para contagiar a un vecino.")]
    public float minBond = 10f;
    [Min(0.1f)] public float scanInterval = 0.5f;

    float _next;
    Mind _mind;

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); _mind = GetComponent<Mind>(); }

    void Update()
    {
        if (anima == null || _mind == null || Time.time < _next) return;
        float dt = scanInterval;
        _next = Time.time + scanInterval;

        Humores mine = _mind.humores;
        float drive = Mathf.Max(0f, mine.Positividad) + mine.Energia;   // cuánta "fiesta"/ánimo irradio
        float soc = Mathf.Max(0f, anima.sociability);
        if (drive <= 0f || soc <= 0f) return;

        Collider[] cols = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider c in cols)
        {
            if (c == null || c.gameObject == gameObject) continue;
            ITarget t = c.GetComponent<ITarget>();
            if (t == null) continue;

            Bond b = anima.GetBond(t);
            float bond = b != null ? b.value : 0f;
            if (bond < minBond) continue;                       // solo a quien aprecio

            Anima other = c.GetComponent<Anima>();
            if (other != null && Predation.Fears(other, anima)) continue;   // respeta el miedo (threat)

            Mind om = c.GetComponent<Mind>();
            if (om == null) continue;

            float amt = influence * drive * soc * (bond / 100f) * dt;
            om.humores.serotonina = Mathf.Clamp01(om.humores.serotonina + amt);        // sube el ánimo (positividad)
            om.humores.adrenalina = Mathf.Clamp01(om.humores.adrenalina + amt * 0.5f);  // sube la energía (a jugar)
        }
    }
}
