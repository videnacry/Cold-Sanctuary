using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Childhood : LifeStage
{
    // Types
    public const char den = 'd', nest = 'n';


    public override IEnumerator Live (Animal script)
    {
        stageDays = RemainingStageDays();
        script.transform.localScale = sizePotential;
        int interval = Random.Range(12, 16);
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
            yield return new WaitForSeconds(interval);
        }
        script.adult = true;
        script.StartCoroutine(script.Adolescence.Live(script));
    }
    
}