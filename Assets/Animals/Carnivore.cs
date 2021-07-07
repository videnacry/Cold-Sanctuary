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
        this.busy = true;
        if (this.lifeStage == LifeStage.teen || this.lifeStage == LifeStage.adult)
        {
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
                yield return new WaitForSeconds(1);
            }
            Animal victim = prey.GetComponent<Animal>();
            StartCoroutine(victim.Escape(false, wolves));
            float cansancio = 0;
            do
            {
                location = prey.transform.position;
                distance = Vector3.Distance(location, transform.position);
                if (distance < 300 || victim.aware)
                {
                    this.ActsPrep.run.Prep(this, TimeController.timeController.TimeSpeedMinuteSecs / 60);
                    cansancio += 0.01f;
                    if (distance < 6)
                    {
                        if (hungry < -50) break;
                        if (victim.lifeStage == LifeStage.soul) break;
                        hungry -= 0.2f;
                        victim.Hurt(0.4f);
                    }
                    nav.SetDestination(location);
                    yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 60);
                }
                else
                {
                    this.ActsPrep.walk.Prep(this, TimeController.timeController.TimeSpeedMinuteSecs / 20);
                    nav.SetDestination(location);
                    yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 20);
                }
            } while (distance < 700 && cansancio < 1);
            exhaustion += cansancio;
        }
        this.busy = false;
    }
}