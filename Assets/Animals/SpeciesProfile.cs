using System.Collections.Generic;

/// <summary>
/// Config ESCALAR por especie (docs/anima-dissolving-animal.md, etapa 5): los valores que antes eran overrides en
/// cada clase de especie (PackFactor, bond, reservas, IEdible, umbrales post-natales…) pasan a DATA. La lee `Animal`
/// vía <see cref="SpeciesBody.profile"/>. Un paso más para vaciar las clases de especie (rumbo a la composición).
/// </summary>
public class SpeciesProfile
{
    public float packFactor, harmVsBond, bondGrowthRate, biteSize, toughness, baseStressLevel,
                 vocalizationThreshold, nestSecurityLevel, maxFatReserves, fatAccumulationRate, threatThreshold;
    public OrganicMaterial material;

    public SpeciesProfile(float packFactor, float harmVsBond, float bondGrowthRate, float biteSize, float toughness,
        float baseStressLevel, float vocalizationThreshold, float nestSecurityLevel, float maxFatReserves,
        float fatAccumulationRate, float threatThreshold, OrganicMaterial material)
    {
        this.packFactor = packFactor; this.harmVsBond = harmVsBond; this.bondGrowthRate = bondGrowthRate;
        this.biteSize = biteSize; this.toughness = toughness; this.baseStressLevel = baseStressLevel;
        this.vocalizationThreshold = vocalizationThreshold; this.nestSecurityLevel = nestSecurityLevel;
        this.maxFatReserves = maxFatReserves; this.fatAccumulationRate = fatAccumulationRate;
        this.threatThreshold = threatThreshold; this.material = material;
    }

    // Defaults = los de Animal/Anima (para especies fuera del catálogo / seres compuestos genéricos).
    public static readonly SpeciesProfile Default =
        new SpeciesProfile(0.5f, 0.5f, 1f, 2f, 0.5f, 0.2f, 5f, 0.5f, 20f, 0.5f, 0.5f, OrganicMaterial.Meat);

    static Dictionary<string, SpeciesProfile> _catalog;

    /// <summary>El perfil escalar de una especie (instancia compartida de solo-lectura). Desconocida → Default.</summary>
    public static SpeciesProfile Of(string species)
    {
        if (_catalog == null) BuildCatalog();
        return species != null && _catalog.TryGetValue(species, out SpeciesProfile p) ? p : Default;
    }

    static void BuildCatalog()
    {
        // (packFactor, harmVsBond, bondGrowthRate, biteSize, toughness, baseStressLevel, vocalizationThreshold,
        //  nestSecurityLevel, maxFatReserves, fatAccumulationRate, threatThreshold, material)
        _catalog = new Dictionary<string, SpeciesProfile>
        {
            { "Bear",     new SpeciesProfile(0.3f, 0.7f, 0.4f, 15.0f, 2.0f, 0.1f, 5.0f, 0.9f, 100.0f, 2.0f, 0.8f, OrganicMaterial.Meat) },
            { "Bunny",    new SpeciesProfile(0.0f, 0.2f, 1.5f, 2.0f, 0.1f, 0.85f, 6.0f, 0.3f, 5.0f, 0.2f, 0.5f, OrganicMaterial.Meat) },
            { "Deer",     new SpeciesProfile(0.0f, 0.1f, 1.8f, 2.0f, 0.4f, 0.6f, 9.0f, 0.1f, 10.0f, 0.4f, 0.5f, OrganicMaterial.Meat) },
            { "Fox",      new SpeciesProfile(0.2f, 0.6f, 0.8f, 2.5f, 0.4f, 0.5f, 4.0f, 0.6f, 12.0f, 0.8f, 0.5f, OrganicMaterial.Meat) },
            { "Malamute", new SpeciesProfile(0.9f, 0.3f, 2.2f, 3.0f, 0.6f, 0.2f, 2.0f, 0.8f, 10.0f, 0.4f, 0.5f, OrganicMaterial.Meat) },
            { "Seal",     new SpeciesProfile(0.5f, 0.1f, 2.0f, 2.0f, 0.5f, 0.4f, 5.0f, 0.6f, 80.0f, 1.5f, 0.5f, OrganicMaterial.Fish) },
            { "Whale",    new SpeciesProfile(0.0f, 0.1f, 2.5f, 2.0f, 2.5f, 0.35f, 1.5f, 0.5f, 120.0f, 1.8f, 0.5f, OrganicMaterial.Fish) },
            { "Wolf",     new SpeciesProfile(0.8f, 0.8f, 0.5f, 5.0f, 0.8f, 0.3f, 3.0f, 0.7f, 15.0f, 0.6f, 0.5f, OrganicMaterial.Meat) },
            // Fauna de hielo
            { "Penguin",  new SpeciesProfile(0.4f, 0.1f, 2.0f, 2.0f, 0.4f, 0.45f, 5.0f, 0.5f, 40.0f, 1.2f, 0.5f, OrganicMaterial.Fish) },  // colonia; presa
            { "Orca",     new SpeciesProfile(0.8f, 0.4f, 1.2f, 12.0f, 1.5f, 0.15f, 3.0f, 0.6f, 150.0f, 1.8f, 0.4f, OrganicMaterial.Meat) }, // apex; caza en pod
            // Insectos — cadena trófica del Microcosmos (Nivel 1)
            { "Ant",     new SpeciesProfile(0.9f, 0.4f, 1.5f, 0.8f, 0.5f, 0.3f, 3.0f, 0.7f,  2.0f, 0.5f, 0.4f, OrganicMaterial.Meat) },  // colonia; defiende
            { "Aphid",   new SpeciesProfile(0.0f, 0.1f, 2.5f, 0.1f, 0.1f, 0.7f, 8.0f, 0.2f,  0.5f, 0.3f, 0.6f, OrganicMaterial.Meat) },  // presa total; sin daño
            { "Ladybug", new SpeciesProfile(0.1f, 0.7f, 0.6f, 1.5f, 0.7f, 0.4f, 5.0f, 0.5f,  1.5f, 0.6f, 0.4f, OrganicMaterial.Meat) },  // élitros = toughness alta
            { "Spider",  new SpeciesProfile(0.0f, 0.9f, 0.2f, 4.0f, 1.2f, 0.2f, 4.0f, 0.5f,  3.0f, 0.8f, 0.3f, OrganicMaterial.Meat) },  // veneno; solitaria; paciente
            { "Cricket", new SpeciesProfile(0.2f, 0.4f, 1.0f, 1.0f, 0.4f, 0.5f, 5.0f, 0.4f,  2.0f, 0.6f, 0.5f, OrganicMaterial.Meat) },  // omnívoro; huye primero
        };
    }
}
