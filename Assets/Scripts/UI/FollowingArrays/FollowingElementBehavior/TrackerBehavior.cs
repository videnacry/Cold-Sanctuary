using System.Collections;
using UnityEngine;

// Base para cualquier botón flotante que rastrea un GameObject en la escena.
// Apunta continuamente al objetivo y se destruye cuando este desaparece.
// Subclases: AnimalRadar, MineralTracker, FoodTracker, ThreatTracker...
// También puede ser resultado de un hechizo de detección.
public abstract class TrackerBehavior : FollowingElementBehavior
{
    public float trackDistance = 3f;

    protected GameObject target;

    public override void Init(GameObject elementReference)
    {
        target = elementReference;
        StartCoroutine(Track());
    }

    IEnumerator Track()
    {
        while (target != null && IsTargetAlive())
        {
            transform.localPosition = Vector3.zero;
            transform.LookAt(target.transform);
            transform.Translate(Vector3.forward * trackDistance);
            yield return new WaitForSeconds(0.06f);
        }
        Destroy(gameObject);
    }

    // Cada subclase define cuándo el objetivo deja de estar "vivo"
    protected abstract bool IsTargetAlive();
}
