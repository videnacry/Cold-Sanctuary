using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Comida que el jugador (u otro sistema) deposita en el suelo.
/// Implementa ITarget para que SelectPrey la detecte y IEdible para que
/// Carnivore.Feed() la consuma. Registra en byMaterial para que las dietas
/// referencien su población por tipo de material.
///
/// En Unity: arrastrar al GameObject de comida; configurar material, grams,
/// nutrition y toughness en el Inspector. Añadir a la dieta de la especie:
///   new PreyEntry(FoodItem.GetPopulation(OrganicMaterial.Meat), preference, 0, range)
/// </summary>
public class FoodItem : MonoBehaviour, ITarget, IEdible
{
    public OrganicMaterial material = OrganicMaterial.Meat;
    public float grams     = 50f;
    public float nutrition = 1f;
    public float toughness = 0f;

    /// <summary> Quién dejó esta comida; si no es null, el animal que la coma crecerá bond. </summary>
    public ITarget droppedBy;

    // Registro estático por material
    static Dictionary<OrganicMaterial, HashSet<GameObject>> byMaterial
        = new Dictionary<OrganicMaterial, HashSet<GameObject>>();

    public static HashSet<GameObject> GetPopulation(OrganicMaterial mat)
    {
        if (!byMaterial.ContainsKey(mat))
            byMaterial[mat] = new HashSet<GameObject>();
        return byMaterial[mat];
    }

    void Start()  => GetPopulation(material).Add(gameObject);
    void OnDestroy() => GetPopulation(material).Remove(gameObject);

    // ITarget — la comida siempre está "muerta" (disponible), nunca es amenaza
    public float Mass    => grams;
    public float Speed   => 0f;
    public char  Faction => 'f';
    public bool  Dead    => true;
    bool ITarget.Consumed => Consumed;
    public void  Hurt(float damage) { }  // la comida no recibe daño de combate

    // IEdible
    public OrganicMaterial Material  => material;
    public float Nutrition => nutrition;
    public float Toughness => toughness;
    public float Grams     => grams;
    public bool  Consumed  => grams <= 0f;

    public float Consume(float biteSize)
    {
        float effectiveBite = biteSize / (1f + toughness);
        effectiveBite = Mathf.Min(effectiveBite, grams);
        grams -= effectiveBite;
        if (grams <= 0f) Destroy(gameObject);
        return effectiveBite * nutrition;
    }
}
