using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class ActionPrep
{
    public string aniName;
    public float navSpeed;
    public float aniSpeed;
    public float energyCost;
    public ActionPrep(string pAniName, float pNavSpeed, float pAniSpeed = 1, float pEnergyCost = 0.5f)
    {
        this.aniName = pAniName;
        this.navSpeed = pNavSpeed;
        this.aniSpeed = pAniSpeed;
        this.energyCost = pEnergyCost/TimeController.timeController.TimeSpeedMinuteSecs/4;
    }
    public void Prep(Animal pScript, float pAnimationTime)
    {
        pScript.ani.Play(this.aniName);
        pScript.ani.speed = this.aniSpeed;
        pScript.nav.speed = this.navSpeed;
        float exhaustion = this.energyCost * pAnimationTime;
        pScript.exhaustion += exhaustion;
        pScript.hungry += (exhaustion > 0) ? pScript.Body.GetMealWeight(pScript) * exhaustion : 0;
    }
}
