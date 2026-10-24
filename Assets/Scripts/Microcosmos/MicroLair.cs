using UnityEngine;

/// <summary>La GUARIDA de la tribu: adonde las hormigas llevan la comida (o el hongo cultivado).</summary>
public class MicroLair : MonoBehaviour
{
    public int stock;
    [Tooltip("Dónde se entrega, en local: delante de la boca. El centro de la guarida queda dentro del montículo (no se llega).")]
    public Vector3 doorOffset;

    public Vector3 Door => transform.TransformPoint(doorOffset);

    public static MicroLair Nearest(Vector3 p)
    {
        MicroLair best = null; float bd = float.MaxValue;
        foreach (MicroLair l in FindObjectsByType<MicroLair>(FindObjectsSortMode.None))
        {
            float d = (l.transform.position - p).sqrMagnitude;
            if (d < bd) { bd = d; best = l; }
        }
        return best;
    }
}
