using UnityEngine;

/// <summary>La GUARIDA de la tribu: adonde las hormigas llevan la comida (o el hongo cultivado).</summary>
public class MicroLair : MonoBehaviour
{
    public int stock;

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
