using UnityEngine;

/// <summary>
/// ÁRBOL CARNÍVORO DE HIELO (docs/ice-sanctuary-ecology.md) — la flora clave del Santuario 1: vive de agua+frío y
/// **atrae a los hambrientos** dejando un rastro de olor a comida (`TraceChannel.ScentFood` = su "néctar"). Quien se
/// acerca cae en **SOPOR** (su miel soporífera): sube sueño/estrés y lo va **debilitando** (enfermedad) hasta morir
/// cerca. Al morir una presa cerca, el árbol **la absorbe y CRECE** (ramas más largas). Las **abejas de las nieves**
/// (`SnowBee`) anidan cerca y hacen miel de esos nutrientes. Barato: rastro O(1) + un OverlapSphere periódico.
/// </summary>
public class IceCarnivorousTree : MonoBehaviour
{
    [Tooltip("Radio del SEÑUELO (deja rastro de olor a comida para atraer hambrientos).")]
    [Min(1f)] public float lureRadius = 14f;
    [Tooltip("Radio LETAL: dentro, el sopor debilita hasta morir.")]
    [Min(0.5f)] public float killRadius = 3.5f;
    [Min(0f)] public float lureStrength = 3f;
    [Tooltip("Cuánto crece (escala) por cada presa que muere cerca.")]
    [Min(0f)] public float growthPerKill = 0.15f;
    [Tooltip("Escala máxima: sin tope, crecía sin control.")]
    [Min(1f)] public float maxScale = 3f;

    public int Absorbed => _absorbed;

    const float PreyKg = 5f;
    readonly System.Collections.Generic.HashSet<Anima> _torpor = new System.Collections.Generic.HashSet<Anima>();
    float _next;
    int _absorbed;
    // Cada cadáver hace crecer el árbol UNA vez. Antes se volvía a contar en cada sondeo (0,5 s) mientras siguiera en
    // el radio, y con cuerpos de cubos cada collider de parte lo contaba otra vez → crecimiento sin límite.
    readonly System.Collections.Generic.HashSet<Anima> _eaten = new System.Collections.Generic.HashSet<Anima>();

    void Update()
    {
        if (_next > Time.time) return;
        _next = Time.time + 0.5f;
        float minuteSecs = TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 60f;
        float step = 0.5f / Mathf.Max(1f, minuteSecs);

        // Señuelo: rastro de "comida" (atrae al Wander que deriva por olor).
        // (2026-11-05) En su celda el señuelo no sale y el sopor solo alcanza lo de dentro (HunterCell): si se rompe, todo su alcance, dentro de la sala.
        HunterCell cell = HunterCell.Instance;
        bool held = cell != null && cell.Holds(this);
        if (!held) TraceField.Leave(transform.position, TraceChannel.ScentFood, lureStrength);

        // (2026-11-05) Recorre a los seres, no un OverlapSphere: en la residencia, con 70 m de radio, los 32 huecos se llenaban de cubos de las casas y nunca veía a nadie.
        foreach (Anima a in SceneCache.All<Anima>(2f))
        {
            if (a == null || a.GetComponent<IceCarnivorousTree>() != null) continue;
            if (a.GetComponent<IceInsectLife>() != null) continue;   // sus abejas y sus arañas: viven de él, no son presa
            if (held && !HunterLab.Instance.Contains(a.transform.position)) continue;
            float d = Vector3.Distance(transform.position, a.transform.position);
            if (d > lureRadius) continue;
            if (d <= killRadius)
            {
                // LA MIEL LETAL (docs/ice-sanctuary-ecology.md §2): quien llega con hambre come, y duerme un sueño del que no despierta. Muere cerca: el árbol lo absorbe.
                Animal beast = a.GetComponent<Animal>();
                if (_torpor.Contains(a) || beast == null || beast.hungry > PredatorRound.HungryAt) _torpor.Add(a);
                if (_torpor.Contains(a)) { a.asleep = true; a.sleepiness = 1f; a.sickness = Mathf.Clamp01(a.sickness + 1.5f * step); }
                else a.sleepiness = Mathf.Clamp01(a.sleepiness + 2f * step);
                if (a.death && _eaten.Add(a))
                {
                    _absorbed++;
                    if (held) cell.Prey(PreyKg);
                    else transform.localScale = Vector3.one * Mathf.Min(maxScale, transform.localScale.x + growthPerKill);
                }
            }
            else
            {
                // A media distancia: un sopor suave (los va atrayendo y amodorrando).
                a.sleepiness = Mathf.Clamp01(a.sleepiness + 0.3f * step * (1f - d / lureRadius));
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 0.9f, 0.3f); Gizmos.DrawWireSphere(transform.position, lureRadius);
        Gizmos.color = new Color(0.9f, 0.3f, 0.5f, 0.4f); Gizmos.DrawWireSphere(transform.position, killRadius);
    }
}
