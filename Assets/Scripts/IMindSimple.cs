/// <summary>
/// Minimal mental-state subset for companions and NPCs.
/// Companions expose these so proximity-effect systems can read their internal state
/// without needing to know the concrete type.
/// The player implements the full IMind, which covers different (and more) stats.
/// </summary>
public interface IMindSimple
{
    float fatigue { get; set; }
    float stress  { get; set; }
    float mood    { get; set; }
}
