using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Carnivore : Animal
{
    /// <summary>
    /// Stops walking, go to hunt the near rabbit
    /// </summary>
    /// <returns>yield</returns>
    public override IEnumerator Feed()
    {
        List<GameObject> wolves = new List<GameObject>
        {
            this.gameObject
        };
        if (BunnyBehavior.population.Count == 0)
        {
            yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 5);
        }
        else if (this.lifeStage == LifeStage.teen || this.lifeStage == LifeStage.adult)
        {
            this.busy = true;
            GameObject prey = BunnyBehavior.population.First();
            Vector3 location = prey.transform.position;
            float distance = Vector3.Distance(transform.position, location);
            foreach (GameObject rabbit in BunnyBehavior.population)
            {
                if (distance > Vector3.Distance(transform.position, rabbit.transform.position))
                {
                    distance = Vector3.Distance(transform.position, rabbit.transform.position);
                    location = rabbit.transform.position;
                    prey = rabbit;
                }
            }
            Animal victim = prey.GetComponent<Animal>();
            StartCoroutine(victim.Escape(false, wolves));
            float cansancio = 0;
            do
            {
                float interval = TimeController.timeController.TimeSpeedMinuteSecs / 60;
                location = prey.transform.position;
                nav.SetDestination(location);
                distance = Vector3.Distance(location, transform.position);
                if (victim.death)
                {
                    if (distance < 6)
                    {
                        this.ActsPrep.idle.Prep(this, TimeController.timeController.TimeSpeedMinuteSecs / 30);
                        if (hungry < -this.Body.GetMealMaxWeight(this)) break;
                        if (victim.lifeStage == LifeStage.soul) break;
                        victim.Hurt(0.2f);
                        hungry -= 0.2f;
                    }
                    else
                    {
                        this.ActsPrep.run.Prep(this, TimeController.timeController.TimeSpeedMinuteSecs / 30);
                    }
                    yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 30);
                }
                else if (distance < 300 || victim.aware)
                {
                    this.ActsPrep.run.Prep(this, interval);
                    cansancio += 0.01f;
                    if (distance < 8)
                    {
                        victim.Hurt(0.8f);
                    }
                    yield return new WaitForSeconds(interval);
                }
                else
                {
                    this.ActsPrep.walk.Prep(this, TimeController.timeController.TimeSpeedMinuteSecs / 20);
                    yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 20);
                }
            } while (distance < 700 && cansancio < 1);
            exhaustion += cansancio;
            this.busy = false;
        }
    }
}