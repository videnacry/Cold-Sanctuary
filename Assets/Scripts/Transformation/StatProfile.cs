using UnityEngine;

/// <summary>
/// Perfil de las aptitudes de un `Anima` (docs/stats-as-truth.md §4). Se **captura** para poder revertir y se
/// **aplica** al transformar de verdad. `Might` = potencia resumida para el **combate de stats** de la
/// transformación (y base de la depredación por stats más adelante).
/// </summary>
[System.Serializable]
public class StatProfile
{
    public float agility = 1f, perception = 1f, strength = 1f, bodyMass = 1f, adaptability = 1f, composure = 1f,
                 endurance = 1f, reasoning = 1f, memory = 1f, creativity = 1f, sociability = 1f, discipline = 1f,
                 afabilidad = 1f, sensibilidad = 1f;

    /// <summary>Potencia resumida (masa/fuerza/agilidad/aguante/compostura) para el combate de stats.</summary>
    public float Might => (strength + bodyMass + agility + endurance + composure) / 5f;

    public static StatProfile Capture(Anima a) => new StatProfile
    {
        agility = a.agility, perception = a.perception, strength = a.strength, bodyMass = a.bodyMass,
        adaptability = a.adaptability, composure = a.composure, endurance = a.endurance, reasoning = a.reasoning,
        memory = a.memory, creativity = a.creativity, sociability = a.sociability, discipline = a.discipline,
        afabilidad = a.afabilidad, sensibilidad = a.sensibilidad
    };

    public void ApplyTo(Anima a)
    {
        a.agility = agility; a.perception = perception; a.strength = strength; a.bodyMass = bodyMass;
        a.adaptability = adaptability; a.composure = composure; a.endurance = endurance; a.reasoning = reasoning;
        a.memory = memory; a.creativity = creativity; a.sociability = sociability; a.discipline = discipline;
        a.afabilidad = afabilidad; a.sensibilidad = sensibilidad;
    }
}

/// <summary>
/// Una **forma** de transformación (docs/stats-as-truth.md §4): apariencia (escala/modelo — los modelos viven
/// fuera del repo) + el **perfil de stats genérico** de esa forma. Customización: una hormiga gigante = escala
/// alta + `bodyMass` alto; pequeña = lo contrario.
/// </summary>
[System.Serializable]
public class TransformPreset
{
    public string formName = "Forma";
    public Vector3 visualScale = Vector3.one;
    [Tooltip("Modelo a activar (opcional; los modelos viven fuera del repo).")]
    public GameObject formModel;
    public StatProfile profile = new StatProfile();
}
