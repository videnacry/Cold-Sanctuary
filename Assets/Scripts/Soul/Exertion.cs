using UnityEngine;

/// <summary>
/// COSTE FÍSICO de una acción de trabajo (vía A — docs/soul-relations-reincarnation §2b). "Estar de pie o hacer
/// fuerza" **gasta reservas** (glucosa/minerales) y **acumula fatiga/sueño**. No toca el estrés directamente:
/// lo hace <see cref="MoodDynamics"/>, que convierte reservas bajas + fatiga en cortisol → así el desgaste del
/// trabajo **desemboca solo** en mal humor/prisa (p. ej. Goluis tras un turno largo).
/// </summary>
[System.Serializable]
public class ExertionCost
{
    [Tooltip("Glucosa (energía) gastada por acción: correr, hacer fuerza, un paso intenso.")]
    public float glucose = 0.03f;
    [Tooltip("Minerales/calcio gastados: carga sostenida, estar mucho rato de pie.")]
    public float minerals = 0.01f;
    [Tooltip("Fatiga acumulada 0-1 (llega a MoodState si existe).")]
    public float fatigue = 0.02f;
    [Tooltip("Sueño acumulado 0-1: turnos largos, doble jornada.")]
    public float sleepiness = 0f;
}

/// <summary>Aplica un <see cref="ExertionCost"/> a un ser. Reservas en <c>Humores</c>, fatiga en <c>MoodState</c>,
/// sueño en el <c>Anima</c>. El desgaste se vuelve estrés luego, vía <see cref="MoodDynamics"/>.</summary>
public static class Exertion
{
    public static void Apply(Anima a, ExertionCost c)
    {
        if (a == null || c == null) return;

        Mind m = a.GetComponent<Mind>();
        if (m != null && m.humores != null)
        {
            m.humores.glucosa = Mathf.Clamp01(m.humores.glucosa - c.glucose);   // reservas gastadas por el esfuerzo
            m.humores.calcio  = Mathf.Clamp01(m.humores.calcio  - c.minerals);
        }
        if (c.sleepiness != 0f) a.sleepiness = Mathf.Clamp01(a.sleepiness + c.sleepiness);

        if (c.fatigue != 0f)
        {
            a.mentalFatigue = Mathf.Clamp01(a.mentalFatigue + c.fatigue);   // fatiga UNIVERSAL → aplica también al jugador (sin MoodState)
            MoodState ms = a.GetComponent<MoodState>();
            if (ms != null) ms.fatigue = Mathf.Clamp01(ms.fatigue + c.fatigue);   // + la fatiga de compañero, si existe
        }
    }
}
