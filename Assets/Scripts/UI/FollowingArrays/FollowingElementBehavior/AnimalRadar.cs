using UnityEngine;

public class AnimalRadar : TrackerBehavior
{
    Animal animalReference;

    public override void Init(GameObject elementReference)
    {
        animalReference = elementReference.GetComponent<Animal>();
        base.Init(elementReference);
    }

    protected override bool IsTargetAlive() => animalReference.lifeStage != LifeStage.soul;
}
