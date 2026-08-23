/// <summary>
/// Claves de CAPACIDAD/hechizo para la confianza-por-uso (<see cref="Anima.spellConfidence"/> / `RecordUse`).
/// Hoy son claves GRUESAS (combate); a futuro se afinan a hechizos concretos (Morder/Arañar/Ver…) cuando exista el
/// repertorio como tal. Centralizar aquí evita magic-strings dispersas. Ver docs/capabilities-and-embodiment.md §4.
/// </summary>
public static class Capability
{
    /// <summary>Uso OFENSIVO del cuerpo (cazar/pelear). Su confianza alimenta la agresividad EFECTIVA: el que hiere
    /// con éxito se vuelve más osado (temperamento histórico); el que nunca lo hace (herbívoro) no.</summary>
    public const string Combat = "combat";
}
