/// <summary>
/// Recursos globales de un santuario (Mesocosmos / Macrocosmos, ver
/// docs/world-topology-and-planes.md §4 y §7). Son distintos de las monedas (CoinWallet) y de los
/// items (ItemData): representan los AGREGADOS del santuario que alimentan construcción, mejoras y
/// guerra. Cada área produce el suyo (AreaProducer); construcción/mejoras/guerra lo gastan.
/// </summary>
public enum SanctuaryResource
{
    Food,       // Alimento — sube el tope de tropas generables (Macrocosmos).
    Materials,  // Materiales — construcción, reparación y estructuras de guerra.
    Research    // Investigación — mejoras y desbloqueos.
}
