using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Adolescence : LifeStage
{
    public Adolescence (short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend) : base(pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend) { }


    public override Substage GrowScale()
    {
        return (Animal script) =>
        {
            Vector3 growFraction = (script.AdultStage.sizePotential - sizePotential) / stageDays;
            script.transform.localScale += growFraction;
        };
    }
}