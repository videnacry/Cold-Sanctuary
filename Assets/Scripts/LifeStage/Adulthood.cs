using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Adulthood : LifeStage
{
    public Adulthood(short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend) : base(pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend) { }

    public override Substage GrowScale()
    {
        return (Animal script) =>
        {};
    }
}