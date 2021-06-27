using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Adolescence : LifeStage
{
    public override IEnumerator Live (Animal script)
    {
        stageDays = RemainingStageDays();
        script.transform.localScale = sizePotential;
        Vector3 growFraction = (script.Adolescence.sizePotential - sizePotential) / stageDays;
        int interval = Random.Range(40, 60);
        while ((stageDays - livedDays) > 0)
        {
            script.size += growFraction;
            script.transform.localScale += growFraction;
            livedDays++;
            yield return new WaitForSeconds(interval);
        }
        script.moving = false;
        script.StartCoroutine(script.Adulthood.Live(script));
    }

}