using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AnimationsName 
{
    public readonly string idle;
    public readonly string walk;
    public readonly string run;
    public AnimationsName(string pName)
    {
        this.idle = "Idle" + pName;
        this.walk = "Walk" + pName;
        this.run = "Run" + pName;
    }
    public AnimationsName(string pIdle, string pWalk, string pRun)
    {
        this.idle = pIdle;
        this.walk = pWalk;
        this.run = pRun;
    }
}
