/// <summary>
/// Una etapa del ciclo post-natal. Cada especie declara un array de estas
/// como propiedad virtual en su clase Animal. Ver docs/behavior-system.md §Fase 6.
/// </summary>
[System.Serializable]
public class PostNatalStage
{
    public string label                         = "Stage";
    public float  durationDays                  = 7f;
    public NestType             nestType        = NestType.Burrow;
    public FatherRole           fatherRole      = FatherRole.Absent;
    public MotherPresencePattern presencePattern = MotherPresencePattern.Continuous;
    public WeaningType          weaningType     = WeaningType.Gradual;
    public FeedingMethod        feedingMethod   = FeedingMethod.Nurse;

    /// <summary>
    /// Acciones ejecutadas en orden al entrar al stage (una sola vez).
    /// Ejemplo Stage0: Clean → Stimulate → GuideTeat → FirstMilk
    /// </summary>
    public MotherAction[] entryActions = new MotherAction[0];

    /// <summary>
    /// Condiciones que deben cumplirse TODAS para avanzar al siguiente stage.
    /// Si el array está vacío, el stage avanza solo por tiempo (durationDays).
    /// </summary>
    public TransitionCondition[] transitions = new TransitionCondition[0];

    public bool CanTransition(Animal mother, Animal cub, float daysInStage)
    {
        if (transitions == null || transitions.Length == 0)
            return daysInStage >= durationDays;
        foreach (TransitionCondition t in transitions)
            if (!t.Evaluate(mother, cub, daysInStage)) return false;
        return true;
    }
}
