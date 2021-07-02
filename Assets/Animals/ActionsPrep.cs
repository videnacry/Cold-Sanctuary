using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class ActionsPrep
{
    public ActionPrep idle;
    public ActionPrep walk;
    public ActionPrep run;
    public ActionsPrep(ActionPrep pIdle, ActionPrep pWalk, ActionPrep pRun)
    {
        this.idle = pIdle;
        this.walk = pWalk;
        this.run = pRun;
    }
}
