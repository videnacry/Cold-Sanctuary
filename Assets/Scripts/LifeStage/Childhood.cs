using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Childhood : LifeStage
{
    public Childhood (short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend) : base(pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend) { }


    public override IEnumerator Live(Animal script, TimeController timeController)
    {
        foreach (byte preparation in script.ChildPreps) GetPrep(preparation)(script);
        while ((stageDays - livedDays) > 0)
        {
            foreach (byte myEvent in script.ChildEvents) GetEvent(myEvent)(script);
            livedDays++;
            yield return new WaitForSeconds(timeController.TimeSpeedMinuteSecs / Random.Range(1.0f, 2.0f));
        }
        script.lifeStage = LifeStage.teen;
        script.StartCoroutine(script.TeenStage.Live(script, timeController));
    }



    public override Substage GrowScale()
    {
        return (Animal script) =>
        {
            Vector3 growFraction = (script.TeenStage.sizePotential - sizePotential) / stageDays;
            script.transform.localScale += growFraction;
        };
    }
    /*--------------------------------Normal-----------------------------------------
    public override IEnumerator Live (Animal script, TimeController timeController)
    {
        stageDays = RemainingStageDays();
        script.transform.localScale = sizePotential;
        Vector3 growFraction = (script.Adolescence.sizePotential - sizePotential) / stageDays;
        while ((stageDays - livedDays) > 0)
        {
            if (script.parents.Count > 0)
            {
                GameObject nearParent = script.parents.First();
                float nearParentDistance = Vector3.Distance(script.transform.position, nearParent.transform.position);
                foreach (GameObject parent in script.parents)
                {
                    float parentDistance = Vector3.Distance(script.transform.position, parent.transform.position);
                    if (nearParentDistance > parentDistance)
                    {
                        nearParent = parent;
                        nearParentDistance = parentDistance;
                    }
                }
                script.ani.Play(script.animationsName.idle);
                if (nearParentDistance > 10)
                {
                    script.ani.Play(script.animationsName.run);
                    script.bird = nearParent;
                    script.nav.SetDestination(nearParent.transform.position);
                    script.moving = true;
                    script.asleep = false;
                    script.nav.speed = 5;
                    script.ani.speed = 5;
                }
            }
            script.size += growFraction;
            script.transform.localScale += growFraction;
            livedDays++;
            yield return new WaitForSeconds(timeController.TimeSpeedMinuteSecs / Random.Range(1.0f, 2.0f));
        }
        script.adult = true;
        script.StartCoroutine(script.Adolescence.Live(script, timeController));
    }
    */
}