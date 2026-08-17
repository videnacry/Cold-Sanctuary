using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Carnivore : Animal
{
    /// <summary>
    /// Tabla de presas priorizada de la especie (preferencia, dificultad, rango).
    /// Ver docs/behavior-system.md (Componente A).
    /// </summary>
    public abstract Diet Diet { get; set; }

    // Config del Forager (etapa 3): un carnívoro caza PRESA según su Diet. (Un omnívoro marcaría además eatsGrass.)
    protected override void ConfigureForager(Forager f) { f.eatsPrey = true; f.diet = Diet; }

    /// <summary>
    /// Stops walking, picks the best prey from its Diet (priority + hunger + range) and hunts it.
    /// </summary>
    /// <returns>yield</returns>
    public override IEnumerator Feed()
    {
        List<GameObject> hunters = new List<GameObject>
        {
            this.gameObject
        };
        if (this.lifeStage == LifeStage.teen || this.lifeStage == LifeStage.adult)
        {
            GameObject prey = Forage.SelectTarget(this);
            if (prey == null)
            {
                yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 5);
                yield break;
            }
            this.busy = true;
            ITarget victim = prey.GetComponent<ITarget>();
            if (victim == null) { this.busy = false; yield break; }
            Anima victimEntity = prey.GetComponent<Anima>();
            victimEntity?.RespondToThreat(this.gameObject);
            Vector3 location = prey.transform.position;
            float distance = Vector3.Distance(transform.position, location);
            float cansancio = 0;
            do
            {
                float interval = TimeController.timeController.TimeSpeedMinuteSecs / 60;
                location = prey.transform.position;
                Loco.GoTo(location);
                distance = Vector3.Distance(location, transform.position);
                if (victim.Dead)
                {
                    if (distance < 6)
                    {
                        IEdible food = prey.GetComponent<IEdible>();
                        if (food == null || food.Consumed) break;
                        if (hungry < -this.Body.GetMealMaxWeight(this)) break;
                        Loco.Idle(TimeController.timeController.TimeSpeedMinuteSecs / 30);
                        float nutrition = food.Consume(BiteSize);
                        hungry -= nutrition;
                        // Absorción por metabolismo: lo útil construye stats (Constitution); el exceso → grasa
                        // (opt-in: solo si el ser tiene Metabolism). docs/stats-as-truth §9.
                        GetComponent<Metabolism>()?.AbsorbFood(nutrition, food.Material);
                        FoodItem dropped = prey.GetComponent<FoodItem>();
                        if (dropped?.droppedBy != null)
                            GrowBond(dropped.droppedBy, BondType.Friend, nutrition);
                        if (lifeStage == LifeStage.child && dropped != null)
                            firstSolidEaten = true;
                    }
                    else
                    {
                        Loco.SetGait(true, TimeController.timeController.TimeSpeedMinuteSecs / 30);
                    }
                    yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 30);
                }
                else
                {
                    Anima victimLiving = prey.GetComponent<Anima>();
                    bool preyAware = victimLiving != null && victimLiving.aware;
                    if (distance < 300 || preyAware)
                    {
                        Loco.SetGait(true, interval);
                        cansancio += 0.01f;
                        if (distance < 8)
                            victim.Hurt(0.8f);
                        yield return new WaitForSeconds(interval);
                    }
                    else
                    {
                        Loco.SetGait(false, TimeController.timeController.TimeSpeedMinuteSecs / 20);
                        yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 20);
                    }
                }
            } while (distance < 700 && cansancio < 1);
            exhaustion += cansancio;

            // Si la presa quedó como FoodItem sin consumir, llevarla a las crías
            FoodItem remains = prey?.GetComponent<FoodItem>();
            if (victim.Dead && remains != null && !remains.Consumed && Group?.fed?.Length > 0)
                (this as ICarrier)?.PickUp(remains);

            this.busy = false;
        }
    }
}