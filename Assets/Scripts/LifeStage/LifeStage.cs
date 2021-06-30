using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public abstract class LifeStage
{
    // Stages
    public const char child = 'c', teen = 't', adult = 'a';


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
        script.nav.enabled = true;
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





    public abstract IEnumerator Live(Animal script, TimeController timeController);



    public delegate void Substage(Animal script);

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
    public abstract Substage GrowScale();
    public Substage Fatten()
    {
        return (Animal script) => script.rig.mass = (script.transform.localScale.magnitude * script.BaseMass) / script.BaseScale.magnitude;
    }
    public static class Preps
    {
        public const byte SetScale = 1;
        public const byte SetRemainingStageDays = 2;
    }
    public static class Events
    {
        public const byte LoopGrow = 1;
        public const byte Fatten = 2;
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
    public Substage GetEvent(byte idx)
    {
        return idx switch
        {
            Events.LoopGrow => GrowScale(),
            Events.Fatten => Fatten(),
            _ => (script) => { }

            ,
        };
    }

}