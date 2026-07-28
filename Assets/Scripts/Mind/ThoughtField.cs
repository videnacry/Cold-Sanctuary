using UnityEngine;

/// <summary>
/// Campo de pensamiento (docs/anima-architecture.md §5, "campo social/semántico"): una fuente en el
/// mundo que **empuja** a las mentes cercanas hacia un tono elemental y/o **altera sus humores**. Sirve
/// para guiar a los personajes pensantes por el entorno (un árbol sagrado, un tótem, un hechizo
/// persistente…). Colócalo en cualquier objeto.
///
/// Influye más en mentes poco inmersas en lo suyo (eso lo modula el `Mind`; aquí solo se declara la
/// fuente). Barato: los `Mind` lo consultan al pensar.
/// </summary>
public class ThoughtField : MonoBehaviour
{
    [Header("Empuje de tono")]
    public ElementalTone tone = ElementalTone.Agua;

    [Tooltip("Radio de influencia (m).")]
    [Min(0f)] public float radius = 6f;

    [Tooltip("Peso que añade a su tono en las mentes dentro del radio (su fuerza de sugestión).")]
    [Min(0f)] public float pull = 2f;

    [Header("Humor (opcional)")]
    public bool nudgesHumor = false;
    public Humor humor = Humor.Serotonina;
    [Tooltip("Cuánto mueve ese humor por segundo (+ sube, − baja) a las mentes dentro del radio.")]
    public float humorPerSecond = 0.05f;

    /// <summary>¿Cubre este campo la posición dada?</summary>
    public bool Covers(Vector3 pos) => (pos - transform.position).sqrMagnitude <= radius * radius;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.5f, 0.9f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
