/// <summary>
/// Todo lo que puede ser comido: animales muertos y comida depositada en el suelo.
/// Separado de ITarget para que el tipo de material y la nutrición no contaminen
/// la interfaz de detección/combate.
/// </summary>
public interface IEdible
{
    OrganicMaterial Material { get; }
    float Nutrition  { get; }  // hambre que quita por gramo consumido
    float Toughness  { get; }  // resistencia al mordisco (0 = blando, 3+ = hueso/cáscara dura)
    float Grams      { get; }  // masa comestible restante
    bool  Consumed   { get; }  // true cuando no queda nada

    /// <summary>
    /// Consume hasta <paramref name="biteSize"/> gramos, reducidos por Toughness.
    /// Devuelve la nutrición real obtenida.
    /// </summary>
    float Consume(float biteSize);
}
