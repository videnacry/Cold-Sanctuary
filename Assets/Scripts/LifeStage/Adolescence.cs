using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Adolescence : LifeStage
{
    public override IEnumerator Live (Animal script, TimeController timeController)
    {
        stageDays = RemainingStageDays();
        script.transform.localScale = sizePotential;
        Vector3 growFraction = (script.Adulthood.sizePotential - sizePotential) / stageDays;
        while ((stageDays - livedDays) > 0)
        {
            script.size += growFraction;
            script.transform.localScale += growFraction;
            livedDays++;
            yield return new WaitForSeconds(timeController.TimeSpeedMinuteSecs / Random.Range(1.0f, 2.0f));
        }
        script.moving = false;
        script.StartCoroutine(script.Adulthood.Live(script, timeController));
    }

}