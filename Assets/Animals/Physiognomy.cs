using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Physiognomy
{
    public float mealMinBodyWeight;
    public float mealMaxBodyWeight;
    public float mealBodyWeight;
    public float baseMass;
    public Vector3 baseScale;
    
    public Physiognomy(Vector3 pBaseScale, float pBaseMass, float pMealBodyWeight, float pMealMaxBodyWeight, float pMealMinBodyWeight)
    {
        this.baseScale = pBaseScale;
        this.baseMass = pBaseMass;
        this.mealBodyWeight = pMealBodyWeight;
        this.mealMinBodyWeight = pMealMinBodyWeight;
        this.mealMaxBodyWeight = pMealMaxBodyWeight;
    }
    // Pesos de comida a partir de la MASA (kg). Desacoplado del tipo Animal (etapa 5): toma la masa, no el Animal —
    // así el físico lo puede usar cualquier ser (rumbo a la composición). El llamante pasa su rig.mass.
    public float GetMealMinWeight(float mass) => mass * this.mealMinBodyWeight;
    public float GetMealMaxWeight(float mass) => mass * this.mealMaxBodyWeight;
    public float GetMealWeight(float mass) => mass * this.mealBodyWeight;

    // ── Catálogo por especie (etapa 5): el físico deja de ser un `defaultBody` por clase y pasa a DATA. ──
    // new Physiognomy(baseScale, baseMass, mealBodyWeight, mealMaxBodyWeight, mealMinBodyWeight)
    static Dictionary<string, Physiognomy> _catalog;

    static void BuildCatalog()
    {
        _catalog = new Dictionary<string, Physiognomy>
        {
            { "Human",    new Physiognomy(new Vector3(1f, 1f, 1f),           70f, 0.09f, 0.2f,  0.05f) },
            { "Bear",     new Physiognomy(new Vector3(3.5f, 3.5f, 3.5f),    300f, 0.09f, 0.2f,  0.05f) },
            { "Wolf",     new Physiognomy(new Vector3(0.268f, 0.268f, 0.268f), 45f, 0.09f, 0.2f, 0.05f) },
            { "Bunny",    new Physiognomy(new Vector3(0.0106f, 0.0106f, 0.0106f), 7f, 0.13f, 0.07f, 0.18f) },
            { "Fox",      new Physiognomy(new Vector3(0.134f, 0.134f, 0.134f),  4f, 0.09f, 0.2f, 0.05f) },
            { "Malamute", new Physiognomy(new Vector3(0.185f, 0.185f, 0.185f), 36f, 0.09f, 0.2f, 0.05f) },
            { "Seal",     new Physiognomy(new Vector3(1.5f, 1.5f, 1.5f),      80f, 0.05f, 0.3f, 0.1f) },
            { "Deer",     new Physiognomy(new Vector3(0.219f, 0.219f, 0.219f),90f, 0.07f, 0.25f, 0.1f) },
            { "Whale",    new Physiognomy(new Vector3(1.157f, 1.157f, 1.157f),1300f, 0.04f, 0.3f, 0.12f) },
        };
    }

    /// <summary>El físico de una especie (COPIA, para que cada ser tenga el suyo). Desconocida → Human.</summary>
    public static Physiognomy Of(string species)
    {
        if (_catalog == null) BuildCatalog();
        Physiognomy p = species != null && _catalog.TryGetValue(species, out Physiognomy v) ? v : _catalog["Human"];
        return new Physiognomy(p.baseScale, p.baseMass, p.mealBodyWeight, p.mealMaxBodyWeight, p.mealMinBodyWeight);
    }
}