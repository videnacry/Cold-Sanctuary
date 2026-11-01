// using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Childhood : LifeStage
{
    public Childhood (short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend) : base(pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend) { }



    public override SubEvent GrowScale()
    {
        return (Animal script, float duration) =>
        {
            // En proporción a los días de vida pasados (tickDays): al acabar la etapa llega justo al tamaño de la siguiente.
            // (Antes crecía 1/stageDays en cada tercio de «día»: el triple de lo debido.)
            Vector3 growFraction = (script.TeenStage.sizePotential - sizePotential) / Mathf.Max(1, (int)stageDays) * tickDays;
            script.transform.localScale += growFraction;
        };
    }
}