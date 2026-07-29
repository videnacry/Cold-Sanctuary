/// <summary>
/// Un "cerebro" que puede conducir a un `Anima` (docs/anima-architecture.md §11.5). El
/// <see cref="AnimaController"/> elige cada frame el cerebro de MAYOR relevancia y le cede el mando.
/// Implementaciones: la IA propia del ser (<see cref="AiBrain"/>) o el input del jugador vía posesión
/// (<see cref="PlayerBrain"/>). "El jugador es solo un input" → un cerebro más, intercambiable.
/// </summary>
public interface IBrain
{
    /// <summary>Cuánto reclama conducir a este ser AHORA. El mayor gana; la posesión sube la del jugador.</summary>
    float Relevance { get; }

    /// <summary>Nombre para logs/depuración.</summary>
    string BrainName { get; }

    /// <summary>Conducir al ser este frame (mover/actuar). Solo se invoca al cerebro activo.</summary>
    void Act(AnimaController ctrl);
}
