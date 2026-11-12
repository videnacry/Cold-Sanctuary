using UnityEngine;

/// <summary>Una parada del <see cref="AereoLift"/>: pisarla ofrece las demás. El andén no lleva esta pieza: su puerta es <see cref="AereoGate"/>.</summary>
public class AereoLiftStop : MonoBehaviour
{
    public AereoLift lift;

    void OnTriggerEnter(Collider other)
    {
        if (lift == null || !other.CompareTag("Player") || lift.Busy || lift.ArrivedAt == transform || FlyingCarpet.Riding(other.gameObject)) return;
        lift.Ask(other.gameObject, transform);
    }

    void OnTriggerExit(Collider other)
    {
        if (lift != null && other.CompareTag("Player")) lift.ClearArrival(transform);
    }
}
