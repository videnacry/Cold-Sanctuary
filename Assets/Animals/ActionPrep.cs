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
    public short energyCost;
    public ActionPrep(string pAniName, float pNavSpeed, float pAniSpeed = 1, short pEnergyCost = 1)
    {
        this.aniName = pAniName;
        this.navSpeed = pNavSpeed;
        this.aniSpeed = pAniSpeed;
        this.energyCost = pEnergyCost;
    }
    public void Prep(Animal pScript, float pAnimationTime)
    {
        pScript.ani.Play(this.aniName);
        pScript.ani.speed = this.aniSpeed;
        pScript.nav.speed = this.navSpeed;
        pScript.exhaustion += this.energyCost * pAnimationTime;
        pScript.hungry += (this.energyCost > 0)? this.energyCost * pAnimationTime : 0;
    }
}
