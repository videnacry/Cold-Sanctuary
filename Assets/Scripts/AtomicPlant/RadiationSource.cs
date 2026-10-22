using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UNA FUENTE DE RADIACIÓN en la planta atómica (docs/atomic-plant.md §Protección radiológica): la superficie de la
/// piscina (el N-16 del agua que pasa por el núcleo, proporcional a la potencia), la celda caliente con las muestras
/// irradiadas, la fuente de arranque. La tasa de dosis cae con el CUADRADO DE LA DISTANCIA y la reduce el BLINDAJE:
/// distancia, tiempo y blindaje, las tres reglas de la protección radiológica.
/// </summary>
public class RadiationSource : MonoBehaviour
{
    public static readonly List<RadiationSource> All = new List<RadiationSource>();

    [Tooltip("Tasa de dosis a 1 m sin blindaje (µSv/h).")]
    public float microSvPerHourAt1m = 1f;
    [Tooltip("Si es la piscina: la tasa escala con la potencia del reactor (valor dado a 1 MW).")]
    public bool scalesWithReactorPower;
    [Tooltip("Fracción que deja pasar el blindaje (1 = nada; plomo de la celda caliente ≈ 0,001).")]
    [Range(0f, 1f)] public float shielding = 1f;

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public float RateAt(Vector3 p)
    {
        float s = microSvPerHourAt1m * shielding;
        if (scalesWithReactorPower) s *= ResearchReactor.Instance != null ? ResearchReactor.Instance.power / 1e6f : 0f;
        float r = Mathf.Max(0.5f, Vector3.Distance(p, transform.position));
        return s / (r * r);
    }

    /// <summary>Fondo natural (~0,1 µSv/h) + todas las fuentes.</summary>
    public static float TotalRateAt(Vector3 p)
    {
        float sum = 0.1f;
        foreach (RadiationSource s in All) if (s != null) sum += s.RateAt(p);
        return sum;
    }
}
