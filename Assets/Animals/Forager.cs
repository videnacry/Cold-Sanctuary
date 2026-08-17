using UnityEngine;

/// <summary>
/// Componente de FORRAJEO (docs/anima-dissolving-animal.md, etapa 3). Encapsula la POLÍTICA de "qué/dónde comer":
/// presa (carnívoro, vía <see cref="Diet"/>), pasto y/o banco de peces (herbívoro). Los flags son **combinables**:
/// un **omnívoro** marca varios (p.ej. presa + pasto), y `SelectTarget` elige la fuente **más cercana** de las que
/// come. **Portable**: cualquier `Anima` lo lleva con su combinación (el "qué come" deja de ser la subclase
/// `Carnivore`/`Herbivore` y pasa a ser config de un componente).
///
/// De momento la PERSECUCIÓN y el COMER siguen en `Carnivore.Feed`/`Herbivore.Feed` (locomoción + ingesta); se
/// extraen en un paso posterior. Aquí solo va la **selección de objetivo**.
/// </summary>
public class Forager : MonoBehaviour
{
    [Tooltip("Come PRESA (carnívoro): consulta su Diet.")]
    public bool eatsPrey;
    [Tooltip("Come PASTO (herbívoro terrestre).")]
    public bool eatsGrass;
    [Tooltip("Come PECES/banco (herbívoro/consumidor marino).")]
    public bool eatsFish;
    [Tooltip("Tabla de presas priorizada (solo si eatsPrey).")]
    public Diet diet;

    /// <summary>El objetivo de comida más cercano entre las fuentes que come (presa/pasto/pez), o null. Un omnívoro
    /// (varios flags) elige la más cercana; el carnívoro/herbívoro puro solo tiene una fuente activa.</summary>
    public GameObject SelectTarget(Animal self)
    {
        if (self == null) return null;
        Vector3 pos = self.transform.position;
        GameObject prey  = (eatsPrey && diet != null) ? diet.SelectPrey(self) : null;   // la Diet ya prioriza
        GameObject grass = eatsGrass ? GrassPatch.Nearest(pos)?.gameObject : null;
        GameObject fish  = eatsFish  ? FishSchool.Nearest(pos)?.gameObject : null;
        return Nearest(pos, prey, grass, fish);
    }

    static GameObject Nearest(Vector3 pos, params GameObject[] gos)
    {
        GameObject best = null; float bestSq = float.MaxValue;
        foreach (GameObject go in gos)
        {
            if (go == null) continue;
            float d = (go.transform.position - pos).sqrMagnitude;
            if (d < bestSq) { bestSq = d; best = go; }
        }
        return best;
    }
}
