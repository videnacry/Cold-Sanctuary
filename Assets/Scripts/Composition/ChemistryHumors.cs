using UnityEngine;

/// <summary>
/// PUENTE COMPUESTOS → HUMORES (docs/consciousness-mechanics.md §2.2) — cierra el hueco de la cadena química: la
/// **composición** de un ser (elementos→compuestos en `Constitution`) **genera/influye sus HUMORES** (`Mind.humores`),
/// a CORTO plazo (empuje por tick hacia un objetivo) y a LARGO plazo (el objetivo lo fija la composición, que cambia
/// despacio al comer/gastar). Así la bioquímica pesa en el ánimo — y de ahí en la ARENA (el estrés/energía que sale de
/// aquí compiten con los pensamientos por la conducta). Opt-in y graceful (sin `Constitution` o `Mind`, no hace nada).
///
/// Mapeo (recetas aproximadas, tuneables):
///   ATP            → Glucosa   (energía disponible)
///   Minerales      → Calcio
///   Proteína+neurona → Serotonina (triptófano; base del ánimo — LENTO/largo plazo)
///   Carencia (ATP+proteína bajos) → Cortisol (estrés metabólico — corto plazo)
/// </summary>
public class ChemistryHumors : MonoBehaviour
{
    [Min(0.1f)] public float tick = 0.5f;
    [Tooltip("Velocidad de acercamiento del humor a su objetivo químico por tick (corto plazo).")]
    [Range(0f, 1f)] public float rate = 0.2f;

    Constitution _con;
    Mind _mind;
    float _next;

    void Awake()
    {
        _con = GetComponent<Constitution>();
        _mind = GetComponent<Mind>();
    }

    void Update()
    {
        if (_con == null || _mind == null || _mind.humores == null || Time.time < _next) return;
        _next = Time.time + tick;
        Humores h = _mind.humores;

        float energy  = Mathf.Clamp01(_con.ATP);
        float ca      = Mathf.Clamp01(_con.Minerals);
        float sero    = Mathf.Clamp01(_con.Protein * 0.6f + _con.Neuron * 0.4f);
        float deficit = Mathf.Clamp01(1f - (_con.ATP + _con.Protein) * 0.5f);

        Nudge(h, Humor.Glucosa,    energy, rate);          // energía disponible
        Nudge(h, Humor.Calcio,     ca,     rate);
        Nudge(h, Humor.Serotonina, sero,   rate * 0.4f);   // base del ánimo: LENTA (largo plazo)
        h.Produce(Humor.Cortisol, deficit * rate * 0.3f);  // la carencia química empuja el cortisol (corto plazo)
    }

    // Acerca un humor a su objetivo usando el API público (Produce con el delta).
    static void Nudge(Humores h, Humor humor, float target, float k)
        => h.Produce(humor, (target - h.Get(humor)) * k);
}
