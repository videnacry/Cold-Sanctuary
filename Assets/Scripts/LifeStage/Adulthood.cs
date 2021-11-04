﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Adulthood : LifeStage
{
    public Adulthood(short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend) : base(pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend) { }

    public override SubEvent GrowScale()
    {
        return (Animal script, float duration) =>
        { };
    }
}