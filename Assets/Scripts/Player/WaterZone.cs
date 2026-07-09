using UnityEngine;

/// <summary>
/// Attach to a trigger Collider spanning a swimmable water volume (e.g. Sea_Placeholder).
/// Tells PlayerController when to switch to swim movement, and tints the fog blue while
/// submerged as a cheap "underwater" cue — no post-processing stack set up yet for anything
/// fancier.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WaterZone : MonoBehaviour
{
    public Color underwaterFogColor = new Color(0.05f, 0.2f, 0.35f);

    Color _savedFogColor;

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.SetSwimming(true);
        _savedFogColor = RenderSettings.fogColor;
        RenderSettings.fogColor = underwaterFogColor;
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.SetSwimming(false);
        RenderSettings.fogColor = _savedFogColor;
    }
}
