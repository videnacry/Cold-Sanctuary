using UnityEngine;

/// <summary>
/// Condición que debe cumplirse para avanzar al siguiente PostNatalStage.
/// Todas las condiciones del array deben ser verdaderas simultáneamente.
/// </summary>
[System.Serializable]
public class TransitionCondition
{
    public enum Kind
    {
        TimeElapsed,            // N días en el stage actual
        CubWeightAbove,         // cría alcanzó masa mínima (kg)
        CubReadinessScore,      // score compuesto peso + grasa + autonomía
        MotherFatReservesBelow, // reservas de la madre bajo umbral (abandono foca/osa)
        FirstSolidEaten,        // cría comió un FoodItem por primera vez
        FirstNestExit,          // cría salió del nido/zona una vez sola
        BondThreshold,          // bond cuidador↔cría supera valor
        ThreatLevelBelow,       // estrés ambiental suficientemente bajo para transición
    }

    public Kind kind;
    public float threshold;

    public bool Evaluate(Animal mother, Animal cub, float daysInStage)
    {
        if (cub == null || mother == null) return false;
        switch (kind)
        {
            case Kind.TimeElapsed:
                return daysInStage >= threshold;
            case Kind.CubWeightAbove:
                return cub.rig != null && cub.rig.mass >= threshold;
            case Kind.CubReadinessScore:
                return ComputeReadiness(cub) >= threshold;
            case Kind.MotherFatReservesBelow:
                return mother.fatReserves <= threshold;
            case Kind.FirstSolidEaten:
                return cub.firstSolidEaten;
            case Kind.FirstNestExit:
                return cub.firstNestExit;
            case Kind.BondThreshold:
                Bond b = mother.GetBond(cub);
                return b != null && b.value >= threshold;
            case Kind.ThreatLevelBelow:
                return mother.stress <= threshold;
            default:
                return false;
        }
    }

    // Score compuesto: (masa + grasa) normalizado contra baseMass de la especie.
    // Foca: masa > umbral Y grasa acumulada → score alto → madre puede abandonar.
    static float ComputeReadiness(Animal cub)
    {
        float baseMass = cub.Body?.baseMass ?? 1f;
        return (cub.rig.mass + cub.fatReserves) / Mathf.Max(baseMass, 0.01f);
    }
}
