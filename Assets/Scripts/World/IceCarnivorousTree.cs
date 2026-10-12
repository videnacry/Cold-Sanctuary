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

    float _next;
    int _absorbed;
    static readonly Collider[] _hits = new Collider[32];

    void Update()
    {
        if (_next > Time.time) return;
        _next = Time.time + 0.5f;
        float minuteSecs = TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 60f;
        float step = 0.5f / Mathf.Max(1f, minuteSecs);

        // Señuelo: rastro de "comida" (atrae al Wander que deriva por olor).
        TraceField.Leave(transform.position, TraceChannel.ScentFood, lureStrength);

        int n = Physics.OverlapSphereNonAlloc(transform.position, lureRadius, _hits);
        for (int i = 0; i < n; i++)
        {
            Anima a = _hits[i] != null ? _hits[i].GetComponentInParent<Anima>() : null;
            if (a == null || a.GetComponent<IceCarnivorousTree>() != null) continue;
            float d = Vector3.Distance(transform.position, a.transform.position);
            if (d <= killRadius)
            {
                // SOPOR letal: lo adormece y debilita; cuando muere cerca, el árbol lo absorbe y crece.
                a.sleepiness = Mathf.Clamp01(a.sleepiness + 2f * step);
                a.sickness = Mathf.Clamp01(a.sickness + 1.5f * step);
                if (a.death) { _absorbed++; transform.localScale += Vector3.one * growthPerKill; }
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
