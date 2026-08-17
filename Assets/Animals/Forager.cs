using UnityEngine;

/// <summary>De qué se alimenta un ser (config del <see cref="Forager"/>).</summary>
public enum FoodMode { Prey, Grass, Fish }

/// <summary>
/// Componente de FORRAJEO (docs/anima-dissolving-animal.md, etapa 3). Encapsula la POLÍTICA de "qué/dónde comer":
/// carnívoros → su <see cref="Diet"/> (presa por prioridad + hambre + rango); herbívoros → el pasto o banco de
/// peces más cercano. **Portable**: cualquier `Anima` puede llevarlo con su `mode` (deja de ser una subclase
/// `Carnivore`/`Herbivore`, pasa a ser config de un componente).
///
/// De momento la PERSECUCIÓN y el COMER siguen en `Carnivore.Feed`/`Herbivore.Feed` (locomoción + ingesta,
/// acoplados a la máquina de `Animal`); se extraen en un paso posterior. Aquí solo va la **selección de objetivo**.
/// </summary>
public class Forager : MonoBehaviour
{
    [Tooltip("Qué come: presa (carnívoro), pasto o peces (herbívoro terrestre/marino).")]
    public FoodMode mode = FoodMode.Grass;
    [Tooltip("Tabla de presas priorizada (solo para mode = Prey).")]
    public Diet diet;

    /// <summary>El objetivo de comida ahora mismo (GameObject de la presa / pasto / banco), o null si no hay.</summary>
    public GameObject SelectTarget(Animal self)
    {
        if (self == null) return null;
        switch (mode)
        {
            case FoodMode.Prey:  return diet != null ? diet.SelectPrey(self) : null;
            case FoodMode.Grass: return GrassPatch.Nearest(self.transform.position)?.gameObject;
            case FoodMode.Fish:  return FishSchool.Nearest(self.transform.position)?.gameObject;
        }
        return null;
    }
}
