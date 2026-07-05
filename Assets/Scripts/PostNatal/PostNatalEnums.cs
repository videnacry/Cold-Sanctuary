public enum NestType             { Burrow, OpenField, Beach, SnowDen, OpenWater }
public enum FatherRole           { Absent, Provider, ActiveCaregiver }
public enum MotherPresencePattern { Continuous, FrequentVisits, MinimalVisits, ProgrammedAbandonment }
public enum WeaningType          { Gradual, Abrupt }
public enum FeedingMethod        { Nurse, Regurgitate, FoodItem, None }

/// <summary>
/// Acciones universales de cuidado. Las mismas acciones aparecen como entryActions
/// (secuenciales al nacer) o como loopBehaviors (ponderados durante el stage).
/// </summary>
public enum MotherAction
{
    Clean,          // limpiar/acicalar — universal; en Stage0 activa reflejos
    Stimulate,      // estimulación física — empuje de hocico; Stage0
    GuideTeat,      // guiar al pezón — Stage0
    FirstMilk,      // primera toma (calostro) — Stage0; da anticuerpos extra
    Nurse,          // amamantar — loop general
    Regurgitate,    // regurgitar comida semidigerida — carnívoros etapas tardías
    Rest,           // descansar junto a la cría
    Guard,          // vigilar perímetro sin alejarse
    MarkHidingSpot, // actualizar HomeOrigin de la cría (venado, bear)
}
