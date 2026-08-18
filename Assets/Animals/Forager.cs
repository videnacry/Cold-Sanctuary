using System.Collections;
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

    /// <summary>Da un mordisco a `food` y aplica la INGESTA a `self`: nutrición (baja el hambre + `Metabolism` opt-in),
    /// **bond** con quien dejó la comida (compartir alimenta el vínculo), y marca la 1ª sólida de una cría. Devuelve
    /// la nutrición obtenida. Multi-consumo: varios pueden morder el mismo `food` (pool compartido de `IEdible`).</summary>
    public float Eat(Animal self, IEdible food, GameObject foodObject, float biteSize)
    {
        if (self == null || food == null) return 0f;
        float nutrition = food.Consume(biteSize);
        self.hungry -= nutrition;
        self.GetComponent<Metabolism>()?.AbsorbFood(nutrition, food.Material);   // opt-in: construye stats / grasa
        FoodItem dropped = foodObject != null ? foodObject.GetComponent<FoodItem>() : null;
        if (dropped != null && dropped.droppedBy != null)
            self.GrowBond(dropped.droppedBy, BondType.Friend, nutrition);        // quien te trae comida te cae bien
        if (self.lifeStage == LifeStage.child && dropped != null)
            self.firstSolidEaten = true;
        return nutrition;
    }

    /// <summary>PASTAR/pescar (herbívoro/consumidor de banco): va a la fuente más cercana (por `Locomotion`) y come,
    /// bajando el hambre; si es un banco de peces, lo reduce (`Graze`). Movido desde `Herbivore.Feed` (etapa 3).</summary>
    public IEnumerator Graze(Animal self)
    {
        if (self == null || (self.lifeStage != LifeStage.adult && self.lifeStage != LifeStage.teen)) yield break;
        self.busy = true;

        GameObject food = SelectTarget(self);
        Transform foodSource = food != null ? food.transform : null;

        // Los marinos nadan sobre mar abierto sin NavMesh horneado debajo → guardar (Loco.Walk ya lo respeta,
        // pero sin el guard el while no avanzaría nunca).
        if (foodSource != null && self.nav != null && self.nav.isOnNavMesh)
        {
            float walkInterval = TimeController.timeController.TimeSpeedMinuteSecs / 30;
            while (Vector3.Distance(self.transform.position, foodSource.position) > 3f)
            {
                self.Loco.Walk(foodSource.position, walkInterval);
                yield return new WaitForSeconds(walkInterval);
            }
        }

        float interval = TimeController.timeController.TimeSpeedMinuteSecs / Random.Range(7, 12);
        float feed = self.Body.GetMealWeight(self) * 0.6f;
        if (self.Group.fed.Length > 0) { interval *= 1.2f; feed *= 1.5f; }   // con crías, come más despacio y más
        self.Loco.Idle(interval);
        yield return new WaitForSeconds(interval);
        self.hungry -= feed;
        yield return new WaitForSeconds(interval);
        self.hungry -= feed;
        if (eatsFish && foodSource != null)
        {
            FishSchool fs = foodSource.GetComponent<FishSchool>();
            if (fs != null) fs.Graze(feed);   // el pastoreo marino reduce el banco
        }
        self.busy = false;
    }
}
