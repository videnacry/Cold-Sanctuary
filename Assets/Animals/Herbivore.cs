using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Herbivore : Animal
{
    // Land herbivores (Bunny, Deer) walk to the nearest GrassPatch before eating. Marine ones
    // (Whale, Seal) walk to the nearest FishSchool instead — see WhaleBehavior/SealBehavior.
    protected virtual bool GrazesOnLand => true;

    /// <summary>
    /// Walks to the nearest food source for this species (grass on land, fish at sea) if one
    /// exists, then eats.
    /// </summary>
    /// <returns>yield</returns>
    public override IEnumerator Feed()
    {
        if (this.lifeStage == LifeStage.adult || this.lifeStage == LifeStage.teen)
        {
            this.busy = true;

            Transform foodSource = GrazesOnLand
                ? GrassPatch.Nearest(transform.position)?.transform
                : FishSchool.Nearest(transform.position)?.transform;

            // Marine species swim over open water with no baked NavMesh under them (only the
            // ground is baked) — nav.SetDestination throws if the agent isn't on one, so guard
            // this instead of assuming every Herbivore is NavMesh-placed.
            if (foodSource != null && nav != null && nav.isOnNavMesh)
            {
                float walkInterval = TimeController.timeController.TimeSpeedMinuteSecs / 30;
                while (Vector3.Distance(transform.position, foodSource.position) > 3f)
                {
                    nav.SetDestination(foodSource.position);
                    this.ActsPrep.walk.Prep(this, walkInterval);
                    yield return new WaitForSeconds(walkInterval);
                }
            }

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
            if (!GrazesOnLand && foodSource != null)
            {
                FishSchool fs = foodSource.GetComponent<FishSchool>();
                if (fs != null) fs.Graze(feed);   // el pastoreo marino reduce el banco
            }
            this.busy = false;
        }
    }
}