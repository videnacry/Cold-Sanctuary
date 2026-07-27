/// <summary>
/// Contrato de las 12 aptitudes que **todo ser vivo** expone (docs/creature-stats.md): permite leer las
/// aptitudes de forma UNIFORME sin importar la clase concreta (animal `Anima`, companion
/// `CompanionBase`, jugador `PlayerStats`), mientras no exista `NPCBase` que las consolide.
///
/// 1.0 = media. Los getters son PascalCase para no chocar con los campos camelCase existentes.
/// </summary>
public interface IAptitudes
{
    float Agility { get; }
    float Perception { get; }
    float Strength { get; }
    float BodyMass { get; }
    float Adaptability { get; }
    float Composure { get; }
    float Endurance { get; }
    float Reasoning { get; }
    float Memory { get; }
    float Creativity { get; }
    float Sociability { get; }
    float Discipline { get; }
}
