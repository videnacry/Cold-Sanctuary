/// <summary>
/// `Anima` de PRUEBA con ESPECIE configurable (solo para los tests `Checklist*Test`): permite que un ser de prueba
/// declare un `SpeciesName` (Malamute/Bear/…) para ejercitar el karma de especie (`SpeciesKarma`/`BondPillar`) sin
/// tener que instanciar un animal real.
/// </summary>
public class ChecklistProbeAnima : Anima
{
    public string species;
    public override string SpeciesName => species;
}
