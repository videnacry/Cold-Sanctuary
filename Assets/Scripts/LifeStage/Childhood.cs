using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Childhood : LifeStage
{
    public Childhood (short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend) : base(pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend) { }



    public override Substage GrowScale()
    {
        return (Animal script) =>
        {
            Vector3 growFraction = (script.TeenStage.sizePotential - sizePotential) / stageDays;
            script.transform.localScale += growFraction;
        };
    }
}