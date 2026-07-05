/// <summary>
/// All zones in Cold Sanctuary, ordered roughly by progression tier.
/// The exact tier value is set per-instance in SanctuaryArea.progressionTier.
/// </summary>
public enum SanctuaryAreaType
{
    // ── Tier 1 — entry level ─────────────────────────────────────────────────
    CubCare,              // Criado — first area for newcomers with low stats
    Cleaning,             // Limpieza — Panterilia's first domain
    Kitchen,              // Cocina — Goluis's domain

    // ── Tier 2 — developing ──────────────────────────────────────────────────
    Garden,               // Huerto — physical work, food production, strength gains
    VehicleWorkshop,      // Taller — tractor, boat, submarine maintenance
    AlchemyLab,           // Laboratorio de alquimia vegetal — first periodic table exposure

    // ── Tier 3 — intermediate ────────────────────────────────────────────────
    FuelLab,              // Laboratorio de combustible — biogas, biodiesel, electrolysis
    CulturedMeatLab,      // Laboratorio de carne cultivada — advanced chemistry + nutrition
    SupplementsPharmacy,  // Farmacia — Panterilia level 2, vitamins, minerals

    // ── Tier 4 — advanced ────────────────────────────────────────────────────
    SubmarineAccess,      // Piloting and underwater missions — unlocks deeper zones
    UnderwaterGarden,     // Jardín submarino — rare organisms, breath control
    NightWatch,           // Guardia nocturna — first monster sightings

    // ── Tier 5 — expert ──────────────────────────────────────────────────────
    MonsterSection,       // Sección de monstruos — containment, language, healing
}
