using UnityEngine;

/// <summary>
/// SUPERFICIE AMORTIGUADORA VIVA: un COLCHÓN DE MUSGO esponjoso (musgo de turbera / Sphagnum modificado, que en la
/// naturaleza forma cojines de decenas de cm) plantado bajo las construcciones elevadas y las pasarelas. Absorbe el golpe
/// de quien se caiga (<see cref="FallImpact"/>) sin estorbar a nadie: es una PLANTA más —los animales pasan por encima,
/// los insectos viven en él y retiene la humedad—, a diferencia de una red (que atrapa aves y animales).
/// </summary>
public class SoftGround : MonoBehaviour
{
    [Range(0f, 1f)] public float softness = 0.85f;

    /// <summary>Colchón de musgo de <paramref name="size"/> (x, z) a ras de suelo en <paramref name="center"/>.</summary>
    public static SoftGround Plant(Transform parent, Vector3 center, Vector2 size, int seed)
    {
        var rnd = new System.Random(seed);
        Transform t = new GameObject("Colchon_Musgo").transform;
        t.SetParent(parent, true);
        t.position = center;
        Color moss = new Color(0.62f, 0.76f, 0.56f), mossLight = new Color(0.74f, 0.84f, 0.62f);
        // Base mullida (con collider: es donde se aterriza) y cojines encima.
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = "Musgo"; b.transform.SetParent(t, false);
        b.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        b.transform.localScale = new Vector3(size.x, 0.24f, size.y);
        b.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(moss);
        int bumps = Mathf.Clamp(Mathf.RoundToInt(size.x * size.y / 3f), 4, 60);
        for (int i = 0; i < bumps; i++)
        {
            float x = ((float)rnd.NextDouble() - 0.5f) * size.x * 0.9f, z = ((float)rnd.NextDouble() - 0.5f) * size.y * 0.9f;
            float s = 0.5f + (float)rnd.NextDouble() * 0.9f;
            KitchenProps.C(t, new Vector3(x, 0.26f, z), new Vector3(s, 0.12f + (float)rnd.NextDouble() * 0.1f, s * 0.9f), i % 2 == 0 ? mossLight : moss, (float)rnd.NextDouble() * 90f, "Cojin_Musgo");
        }
        SoftGround sg = t.gameObject.AddComponent<SoftGround>();
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = "Colchón de musgo"; wl.height = 0.8f;
        return sg;
    }
}
