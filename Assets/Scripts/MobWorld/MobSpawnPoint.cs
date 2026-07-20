using UnityEngine;

/// <summary>
/// Marks where the player appears inside a mob-world scene. MobWorldLoader teleports the player
/// here on entry. Place one per mob-world scene (near the entrance/yoga-portal).
/// </summary>
public class MobSpawnPoint : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.4f, 0.9f, 0.6f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
}
