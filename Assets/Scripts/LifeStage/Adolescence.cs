using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Adolescence : LifeStage
{
    public Adolescence (short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend) : base(pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend) { }

    public override IEnumerator Live(Animal script, TimeController timeController)
    {
        foreach (byte preparation in script.TeenPreps) GetPrep(preparation)(script);
        while ((stageDays - livedDays) > 0)
        {
            foreach (byte myEvent in script.TeenEvents) GetEvent(myEvent)(script);
            livedDays++;
            yield return new WaitForSeconds(timeController.TimeSpeedMinuteSecs / Random.Range(1.0f, 2.0f));
        }
        script.lifeStage = LifeStage.teen;
        script.StartCoroutine(script.AdultStage.Live(script, timeController));
    }


    public override Substage GrowScale()
    {
        return (Animal script) =>
        {
            Vector3 growFraction = (script.AdultStage.sizePotential - sizePotential) / stageDays;
            script.transform.localScale += growFraction;
        };
    }
}