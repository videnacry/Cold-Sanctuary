using System.Collections;
using System.Collections.Generic;
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
        script.ChildStage.sizePotential = GetRandomDifferenceScale(script.Body.baseScale, script.ChildStage.minScaleSubstrahend, script.ChildStage.maxScaleSubstrahend);
        script.TeenStage.sizePotential = GetRandomDifferenceScale(script.Body.baseScale, script.TeenStage.minScaleSubstrahend, script.TeenStage.maxScaleSubstrahend);
        script.AdultStage.sizePotential = GetRandomDifferenceScale(script.Body.baseScale, script.AdultStage.minScaleSubstrahend, script.AdultStage.maxScaleSubstrahend);
        InitStage(script, timeController);
    }
    public static void InitStage (Animal script, TimeController timeController)
    {
        switch (script.lifeStage)
        {
            case adult: script.StartCoroutine(script.AdultStage.Live(script, timeController, script.AdultPreps, script.AdultEvents)); break;
            case child: script.StartCoroutine(script.ChildStage.Live(script, timeController, script.ChildPreps, script.ChildEvents)); break;
            default: script.StartCoroutine(script.TeenStage.Live(script, timeController, script.TeenPreps, script.TeenEvents)); break;
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








    public virtual IEnumerator Live(Animal script, TimeController timeController, byte[] stagePreps, byte[] stageEvents)
    {
        foreach (byte preparation in stagePreps) GetPrep(preparation)(script);
        while ((stageDays - livedDays) > 0)
        {
            byte dayThirds = 0;
            while (dayThirds < 3)
            {
                float thirdDuration = timeController.TimeSpeedMinuteSecs / Random.Range(2.5f, 3.2f);
                if (!script.busy) foreach (byte myEvent in stageEvents) GetEvent(myEvent)(script, thirdDuration);
                dayThirds++;
                yield return new WaitForSeconds(thirdDuration);
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



    public delegate void SubPrep(Animal script);
    public delegate void SubEvent(Animal script, float duration);

    #region Preps
    public SubPrep SetScale()
    {
        return (Animal script) => script.transform.localScale = sizePotential;
    }
    public SubPrep SetRemainingStageDays()
    {
        return (Animal script) =>
        {
            float minScale = script.Body.baseScale.y - ((maxScaleSubstrahend / 100f) * script.Body.baseScale.y);
            float maxScale = script.Body.baseScale.y - ((minScaleSubstrahend / 100f) * script.Body.baseScale.y);
            float scaleDifference = maxScale - minScale;
            stageDays = (short)(((sizePotential.y - minScale) * stageDays) / scaleDifference);
        };
    }
    public static class Preps
    {
        public const byte SetScale = 1;
        public const byte SetRemainingStageDays = 2;
    }
    public SubPrep GetPrep(byte idx)
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
    public abstract SubEvent GrowScale();
    public SubEvent Fatten()
    {
        return (Animal script, float duration) =>
        {
            script.rig.mass = (script.transform.localScale.magnitude * script.Body.baseMass) / script.Body.baseScale.magnitude;
            script.lp = script.rig.mass;
        };
    }
    public SubEvent Wander()
    {
        return (Animal script, float duration) =>
        {
            script.ActsPrep.walk.Prep(script, duration);
            List<GameObject> birds = new List<GameObject>(BirdBehavior.population);
            if (birds.Count == 0) return;
            script.target = birds[Random.Range(0, birds.Count)];
            if (script.nav != null && script.nav.isOnNavMesh)
                script.nav.SetDestination(new Vector3(script.target.transform.position.x, script.transform.position.y, script.target.transform.position.z));
        };
    }
    public SubEvent Rest()
    {
        return (Animal script, float duration) =>
        {
            if (Random.Range(1, 4) > 1 || script.exhaustion > 30) script.ActsPrep.idle.Prep(script, duration);
            if (script.exhaustion < 0) script.exhaustion = 0;
        };
    }
    public SubEvent Homebound()
    {
        return (Animal script, float duration) =>
        {
            if (Vector3.Distance(script.transform.position, script.HomeOrigin) > (script.HomeRadius * 2))
            {
                script.ActsPrep.run.Prep(script, duration);
                if (script.nav != null && script.nav.isOnNavMesh) script.nav.SetDestination(script.HomeOrigin);
            }
        };
    }
    public SubEvent Feed()
    {
        return (Animal script, float duration) =>
        {
            if (script.Group?.fed == null || script.Group.fed.Length == 0) return;
            if (script.hungry >= -script.Body.GetMealWeight(script)) return;

            ICarrier carrier = script as ICarrier;
            if (carrier == null) return;

            Vector3 nestPos = script.Group.fed[0].HomeOrigin;

            // 1. Ya carga comida → ir al nido y depositar
            if (carrier.CarriedFood != null)
            {
                float dist = Vector3.Distance(script.transform.position, nestPos);
                if (script.nav != null && script.nav.isOnNavMesh) script.nav.SetDestination(nestPos);
                script.ActsPrep.run.Prep(script, duration);
                if (dist < 10f)
                {
                    FoodItem dropped = carrier.Drop(nestPos + Random.insideUnitSphere * 3f);
                    if (dropped != null)
                        foreach (Animal cub in script.Group.fed)
                            if (cub != null && !cub.death)
                                cub.hungry = -script.Body.GetMealWeight(script);
                }
                return;
            }

            // 2. Hay FoodItem en suelo dentro del HomeRadius → recoger sin cazar
            FoodItem nearby = FindNearbyFood(script);
            if (nearby != null)
            {
                float distFood = Vector3.Distance(script.transform.position, nearby.transform.position);
                if (distFood < 5f)
                    carrier.PickUp(nearby);
                else
                {
                    if (script.nav != null && script.nav.isOnNavMesh) script.nav.SetDestination(nearby.transform.position);
                    script.ActsPrep.walk.Prep(script, duration);
                }
                return;
            }

            // 3. Sin comida disponible: la caza la maneja Restore() → Carnivore.Feed()
        };
    }

    static FoodItem FindNearbyFood(Animal script)
    {
        foreach (FoodItem food in FoodItem.GetAll())
        {
            if (food == null || food.Consumed) continue;
            if (Vector3.Distance(script.transform.position, food.transform.position) < script.HomeRadius)
                return food;
        }
        return null;
    }
    public static class Events
    {
        public const byte LoopGrow = 1;
        public const byte Fatten = 2;
        public const byte Wander = 3;
        public const byte Rest = 4;
        public const byte HomeBound = 5;
        public const byte Feed = 6;
    }

    public SubEvent GetEvent(byte idx)
    {
        return idx switch
        {
            Events.LoopGrow => GrowScale(),
            Events.Fatten => Fatten(),
            Events.Wander => Wander(),
            Events.Rest => Rest(),
            Events.HomeBound => Homebound(),
            Events.Feed => Feed(),
            _ => (script, duration) => { }

            ,
        };
    }
    #endregion
}