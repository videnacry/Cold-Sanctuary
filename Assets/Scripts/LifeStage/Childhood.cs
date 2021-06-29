using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Childhood : LifeStage
{
    public delegate void Substage(Animal script);
    public Substage SetScale()
    {
        return (Animal script) => script.transform.localScale = sizePotential;
    }
    public Substage SetStageDays()
    {
        return (Animal script) => stageDays = RemainingStageDays();
    }
    public Substage LoopGrow()
    {
        return (Animal script) =>
        {
            Vector3 growFraction = (script.TeenStage.sizePotential - sizePotential) / stageDays;
            script.size += growFraction;
            script.transform.localScale += growFraction;
        };
    }
    public static class Preparations
    {
        public const byte SetScale = 1;
        public const byte SetStageDays = 2;
    }
    public static class Events
    {
        public const byte LoopGrow = 1;
    }
    public Substage GetPreparation(byte idx)
    {
        return idx switch
        {
            Preparations.SetScale => SetScale(),
            Preparations.SetStageDays => SetStageDays(),
            _ => (script) => { }

            ,
        };
    }
    public Substage GetEvent(byte idx)
    {
        return idx switch
        {
            Events.LoopGrow => LoopGrow(),
            _ => (script) => { }

            ,
        };
    }
    public override IEnumerator Live(Animal script, TimeController timeController)
    {
        foreach (byte preparation in script.ChildPreparations) GetPreparation(preparation)(script);
        while ((stageDays - livedDays) > 0)
        {
            foreach (byte myEvent in script.ChildEvents) GetEvent(myEvent)(script);
            livedDays++;
            yield return new WaitForSeconds(timeController.TimeSpeedMinuteSecs / Random.Range(1.0f, 2.0f));
        }
        script.adult = true;
        script.StartCoroutine(script.TeenStage.Live(script, timeController));
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