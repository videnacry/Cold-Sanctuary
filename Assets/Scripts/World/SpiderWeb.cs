using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TELA DE ARAÑA ORBICULAR de la araña de las nieves, tendida entre la rama de un panal y el aire por donde pasan las
/// abejas: radios desde el centro y una espiral de hilo pegajoso, como las de las arañas tejedoras (Araneidae). La
/// araña espera en el centro; la abeja que la cruza queda PEGADA unos segundos (se agita) y la araña va a por ella.
/// Hilo blanco translúcido de cubos finos; el plano de la tela es el XY local (normal = +Z local).
/// </summary>
public class SpiderWeb : MonoBehaviour
{
    public float radius = 0.9f;
    public static readonly List<SpiderWeb> All = new List<SpiderWeb>();
    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    /// <summary>¿El punto <paramref name="p"/> está dentro de la tela (cerca de su plano y dentro del círculo)?</summary>
    public bool Catches(Vector3 p)
    {
        Vector3 l = transform.InverseTransformPoint(p);
        return Mathf.Abs(l.z) < 0.12f && new Vector2(l.x, l.y).magnitude < radius;
    }

    public static SpiderWeb Build(Vector3 center, Vector3 facing, float radius, Transform parent)
    {
        Transform t = new GameObject("Telarana").transform;
        t.SetParent(parent, true);
        t.position = center;
        t.rotation = Quaternion.LookRotation(facing.sqrMagnitude > 0.001f ? facing.normalized : Vector3.forward, Vector3.up);
        SpiderWeb w = t.gameObject.AddComponent<SpiderWeb>(); w.radius = radius;
        Color silk = new Color(0.95f, 0.97f, 1f, 0.55f);
        const int spokes = 14;
        for (int i = 0; i < spokes; i++)   // radios
        {
            float a = i * 360f / spokes;
            Transform r = KitchenProps.C(t, Quaternion.Euler(0f, 0f, a) * Vector3.up * radius * 0.5f, new Vector3(0.008f, radius, 0.008f), silk, 0f, "Radio");
            r.localRotation = Quaternion.Euler(0f, 0f, a);
            Object.DestroyImmediate(r.GetComponent<Collider>());
        }
        for (int ring = 1; ring <= 6; ring++)   // espiral pegajosa (anillos de tramos rectos entre radios)
        {
            float rr = radius * ring / 6.4f;
            for (int i = 0; i < spokes; i++)
            {
                float a0 = i * 360f / spokes * Mathf.Deg2Rad, a1 = (i + 1) * 360f / spokes * Mathf.Deg2Rad;
                Vector3 p0 = new Vector3(-Mathf.Sin(a0), Mathf.Cos(a0)) * rr, p1 = new Vector3(-Mathf.Sin(a1), Mathf.Cos(a1)) * rr;
                Transform s = KitchenProps.C(t, (p0 + p1) * 0.5f, new Vector3((p1 - p0).magnitude, 0.006f, 0.006f), silk, 0f, "Espiral");
                s.localRotation = Quaternion.FromToRotation(Vector3.right, p1 - p0);
                Object.DestroyImmediate(s.GetComponent<Collider>());
            }
        }
        // Hilos de anclaje hacia arriba (a la rama) y hacia los lados.
        for (int k = 0; k < 3; k++)
        {
            float a = (k * 120f + 30f) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a) + 0.6f, 0f).normalized;
            Transform h = KitchenProps.C(t, dir * radius * 1.3f, new Vector3(0.006f, radius * 0.7f, 0.006f), silk, 0f, "Anclaje");
            h.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
            Object.DestroyImmediate(h.GetComponent<Collider>());
        }
        return w;
    }
}
