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
    public Vector3 sizePotential, baseScale;
    public short stageDays;
    public int livedDays = 0;
    public int minScaleSubstrahend, maxScaleSubstrahend;


    public static Childhood GetChildhood (Vector3 baseScale, short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend)
    {
        Childhood instance = new Childhood();
        InitInstance(instance, baseScale, pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend);
        return instance;
    }
    public static Adolescence GetAdolescence(Vector3 baseScale, short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend)
    {
        Adolescence instance = new Adolescence();
        InitInstance(instance, baseScale, pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend);
        return instance;
    }
    public static Adulthood GetAdulthood(Vector3 baseScale, short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend)
    {
        Adulthood instance = new Adulthood();
        InitInstance(instance, baseScale, pStageDays, pMinScaleSubstrahend, pMaxScaleSubstrahend);
        return instance;
    }
    public static LifeStage InitInstance (LifeStage instance, Vector3 pBaseScale, short pStageDays, int pMinScaleSubstrahend, int pMaxScaleSubstrahend)
    {
        instance.stageDays = pStageDays;
        instance.minScaleSubstrahend = pMinScaleSubstrahend;
        instance.maxScaleSubstrahend = pMaxScaleSubstrahend;
        instance.baseScale = pBaseScale;
        return instance;
    }
    public static void Init (Animal script)
    {
        script.Childhood.sizePotential = GetRandomDifferenceScale(script.Childhood.baseScale, script.Childhood.minScaleSubstrahend, script.Childhood.maxScaleSubstrahend);
        script.Adolescence.sizePotential = GetRandomDifferenceScale(script.Adolescence.baseScale, script.Adolescence.minScaleSubstrahend, script.Adolescence.maxScaleSubstrahend);
        script.Adulthood.sizePotential = GetRandomDifferenceScale(script.Adulthood.baseScale, script.Adulthood.minScaleSubstrahend, script.Adulthood.maxScaleSubstrahend);
        script.nav.enabled = true;
        switch (script.lifeStage)
        {
            case adult: script.StartCoroutine(script.Adulthood.Live(script)); break;
            case child: script.StartCoroutine(script.Childhood.Live(script)); break;
            default: script.StartCoroutine(script.Adolescence.Live(script)); break;
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
    public short RemainingStageDays()
    {
        float minScale = baseScale.y - ((maxScaleSubstrahend / 100f) * baseScale.y);
        float maxScale = baseScale.y - ((minScaleSubstrahend / 100f) * baseScale.y);
        float scaleDifference = maxScale - minScale;
        return (short)(((sizePotential.y - minScale) * stageDays) / scaleDifference);
    }
    public virtual IEnumerator Live (Animal script)
    {
        yield return new WaitForSeconds(3);
    }

}