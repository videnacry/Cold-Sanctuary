using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AnimationsName 
{
    public readonly string idle;
    public readonly string run;
    public AnimationsName(string pName)
    {
        this.idle = "Idle" + pName;
        this.run = "Run" + pName;
    }
    public AnimationsName(string pIdle, string pRun)
    {
        this.idle = pIdle;
        this.run = pRun;
    }
}
