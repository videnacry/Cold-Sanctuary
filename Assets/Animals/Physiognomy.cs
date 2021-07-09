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
    public float GetMealMinWeight (Animal pScript)
    {
        return pScript.rig.mass * this.mealMinBodyWeight;
    }
    public float GetMealMaxWeight(Animal pScript)
    {
        return pScript.rig.mass * this.mealMaxBodyWeight;
    }
    public float GetMealWeight(Animal pScript)
    {
        return pScript.rig.mass * this.mealBodyWeight;
    }
}