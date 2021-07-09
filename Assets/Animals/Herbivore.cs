using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Herbivore : Animal
{
    /// <summary>
    /// Stops walking, go to hunt the near rabbit
    /// </summary>
    /// <returns>yield</returns>
    public override IEnumerator Feed()
    {
        if (this.lifeStage == LifeStage.adult || this.lifeStage == LifeStage.teen)
        {
            this.busy = true;
            float interval = TimeController.timeController.TimeSpeedMinuteSecs / Random.Range(7, 12);
            float feed = this.Body.GetMealWeight(this) * 0.6f;
            if (this.Group.fed.Length > 0)
            {
                interval *= 1.2f;
                feed *= 1.5f;
            }
            this.ActsPrep.idle.Prep(this, interval);
            yield return new WaitForSeconds(interval);
            this.hungry -= feed;
            yield return new WaitForSeconds(interval);
            this.hungry -= feed;
            this.busy = false;
        }
    }
}