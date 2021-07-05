using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public abstract class LifeStage
{
    // Stages
    public const char child = 'c', teen = 't', adult = 'a', soul = 's';


    // Instance sizes
    public Vector3 sizePotential;
    public short stageDays;
    public int livedDays = 0;
    public int minScaleSubstrahend, maxScaleSubstrahend;

    public LifeStage (short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend)
    {
        stageDays = pStageDays;
        minScaleSubstrahend = pMinScaleSubstrahend;
        maxScaleSubstrahend = pMaxScaleSubstrahend;
    }
    public static void Init (Animal script, TimeController timeController)
    {
        script.ChildStage.sizePotential = GetRandomDifferenceScale(script.BaseScale, script.ChildStage.minScaleSubstrahend, script.ChildStage.maxScaleSubstrahend);
        script.TeenStage.sizePotential = GetRandomDifferenceScale(script.BaseScale, script.TeenStage.minScaleSubstrahend, script.TeenStage.maxScaleSubstrahend);
        script.AdultStage.sizePotential = GetRandomDifferenceScale(script.BaseScale, script.AdultStage.minScaleSubstrahend, script.AdultStage.maxScaleSubstrahend);
        InitStage(script, timeController);
    }
    public static void InitStage (Animal script, TimeController timeController)
    {
        switch (script.lifeStage)
        {
            case adult: script.StartCoroutine(script.AdultStage.Live(script, timeController)); break;
            case child: script.StartCoroutine(script.ChildStage.Live(script, timeController)); break;
            default: script.StartCoroutine(script.TeenStage.Live(script, timeController)); break;
        }
    }
    public static Vector3 GetRandomDifferenceScale(Vector3 baseScale, int pMinScaleSubstrahend, int pMaxScaleSubstrahend)
    {
        System.Random random = new System.Random();
        int scaleMargin = ((pMaxScaleSubstrahend  - pMinScaleSubstrahend) * 20) / 100;
        int scaleBase = random.Next(pMinScaleSubstrahend + scaleMargin, pMaxScaleSubstrahend - scaleMargin);
        int minScale = scaleBase - scaleMargin;
        int maxScale = scaleBase + scaleMargin;
        float scaleX = baseScale.x - ((random.Next(minScale, maxScale) * baseScale.x) / 100);
        float scaleY = baseScale.y - ((random.Next(minScale, maxScale) * baseScale.y) / 100);
        float scaleZ = baseScale.z - ((random.Next(minScale, maxScale) * baseScale.z) / 100);
        return new Vector3(scaleX, scaleY, scaleZ);
    }








    public virtual IEnumerator Live(Animal script, TimeController timeController)
    {
        foreach (byte preparation in script.ChildPreps) GetPrep(preparation)(script);
        while ((stageDays - livedDays) > 0)
        {
            byte dayThirds = 0;
            while (dayThirds < 3)
            {
                if (!script.busy) foreach (byte myEvent in script.ChildEvents) GetEvent(myEvent)(script);
                dayThirds++;
                yield return new WaitForSeconds(timeController.TimeSpeedMinuteSecs / Random.Range(2.5f, 3.2f));
            }
            livedDays++;
        }
        script.lifeStage = GetNextStage(script.lifeStage);
        LifeStage.InitStage(script, timeController);
    }
    public static char GetNextStage (char lifeStage)
    {
        return lifeStage == child ? teen : lifeStage == teen ? adult : soul;
    }



    public delegate void Substage(Animal script);

    #region Preps
    public Substage SetScale()
    {
        return (Animal script) => script.transform.localScale = sizePotential;
    }
    public Substage SetRemainingStageDays()
    {
        return (Animal script) =>
        {
            float minScale = script.BaseScale.y - ((maxScaleSubstrahend / 100f) * script.BaseScale.y);
            float maxScale = script.BaseScale.y - ((minScaleSubstrahend / 100f) * script.BaseScale.y);
            float scaleDifference = maxScale - minScale;
            stageDays = (short)(((sizePotential.y - minScale) * stageDays) / scaleDifference);
        };
    }
    public static class Preps
    {
        public const byte SetScale = 1;
        public const byte SetRemainingStageDays = 2;
    }
    public Substage GetPrep(byte idx)
    {
        return idx switch
        {
            Preps.SetScale => SetScale(),
            Preps.SetRemainingStageDays => SetRemainingStageDays(),
            _ => (script) => { }

            ,
        };
    }
    #endregion



    #region Events
    public abstract Substage GrowScale();
    public Substage Fatten()
    {
        return (Animal script) =>
        {
            script.rig.mass = (script.transform.localScale.magnitude * script.BaseMass) / script.BaseScale.magnitude;
            script.lp = script.rig.mass;
        };
    }
    public Substage Wander()
    {
        return (Animal script) =>
        {
            script.ActsPrep.walk.Prep(script);
            script.target = BirdBehavior.population.ElementAt(Random.Range(0, BirdBehavior.population.Count - 1));
            script.nav.SetDestination(new Vector3(script.target.transform.position.x, script.transform.position.y, script.target.transform.position.z));
        };
    }
    public Substage Rest()
    {
        return (Animal script) =>
        {
            if (Random.Range(1, 4) > 1 || script.exhaustion > 3) script.ActsPrep.idle.Prep(script);
            if (script.exhaustion < 0) script.exhaustion = 0;
        };
    }
    public Substage Homebound()
    {
        return (Animal script) =>
        {
            if (Vector3.Distance(script.transform.position, script.HomeOrigin) > (script.HomeRadius * 2))
            {
                script.ActsPrep.run.Prep(script);
                script.nav.SetDestination(script.HomeOrigin);
            }
        };
    }
    public static class Events
    {
        public const byte LoopGrow = 1;
        public const byte Fatten = 2;
        public const byte Wander = 3;
        public const byte Rest = 4;
        public const byte HomeBound = 5;
    }

    public Substage GetEvent(byte idx)
    {
        return idx switch
        {
            Events.LoopGrow => GrowScale(),
            Events.Fatten => Fatten(),
            Events.Wander => Wander(),
            Events.Rest => Rest(),
            Events.HomeBound => Homebound(),
            _ => (script) => { }

            ,
        };
    }
    #endregion
}