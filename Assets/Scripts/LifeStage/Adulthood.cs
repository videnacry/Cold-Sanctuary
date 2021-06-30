using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Adulthood : LifeStage
{
    public Adulthood(short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend) : base(pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend) { }

    public override IEnumerator Live(Animal script, TimeController timeController)
    {
        foreach (byte preparation in script.AdultPreps) GetPrep(preparation)(script);
        while ((stageDays - livedDays) > 0)
        {
            foreach (byte myEvent in script.AdultEvents) GetEvent(myEvent)(script);
            livedDays++;
            yield return new WaitForSeconds(timeController.TimeSpeedMinuteSecs / Random.Range(1.0f, 2.0f));
        }
        script.lifeStage = LifeStage.adult;
    }

    public override Substage GrowScale()
    {
        return (Animal script) =>
        {};
    }
}