using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// COHESIÓN DE MANADA + ABANDONO (docs/microcosmos-level1.md §Impulsos/beats, rebanada 2): mide, de forma LEGIBLE,
/// cómo de unida o dispersa está la tribu. Los miembros = las Ánimas con <see cref="SoulRecord"/> (el elenco). Cada
/// tick calcula el centroide y la dispersión media respecto a él → <see cref="Cohesion"/> ∈ [0,1] (1 = apiñados,
/// 0 = desperdigados). Cuando la cohesión cae por debajo de <see cref="abandonThreshold"/> durante
/// <see cref="abandonHold"/> segundos, se marca <see cref="Abandoned"/> (el beat de la DESERCIÓN: la tribu se va).
///
/// Es sólo un MEDIDOR (no mueve a nadie): lo leen el <c>Level1Director</c> (rebanada 3) y la emoción. Si Sakshi se
/// separa para cuidar a Ambrosio (Tend gana a Follow), la dispersión sube → cohesión baja → llega el abandono.
/// Balance-safe: sin elenco en escena, Cohesion = 1 y Abandoned = false.
/// </summary>
public class TribeCohesion : MonoBehaviour
{
    [Tooltip("Dispersión (distancia media al centroide) que corresponde a cohesión 0. Por encima → 0.")]
    [Min(0.5f)] public float scatterAtZero = 12f;

    [Tooltip("Umbral de cohesión bajo el cual empieza a contar el abandono.")]
    [Range(0f, 1f)] public float abandonThreshold = 0.35f;

    [Tooltip("Segundos (de juego) que la cohesión debe seguir baja para declarar el abandono.")]
    [Min(0f)] public float abandonHold = 6f;

    [Tooltip("Frecuencia de recálculo (s).")]
    [Min(0.1f)] public float updateRate = 0.5f;

    /// <summary>Cohesión actual [0,1]: 1 = tribu apiñada, 0 = desperdigada.</summary>
    public float Cohesion { get; private set; } = 1f;

    /// <summary>True cuando la tribu se ha dispersado lo bastante y lo bastante tiempo (beat de deserción).</summary>
    public bool Abandoned { get; private set; }

    /// <summary>Nº de miembros considerados este tick (legible).</summary>
    public int MemberCount { get; private set; }

    readonly List<SoulRecord> _members = new List<SoulRecord>();
    float _next;
    float _lowSince = -1f;

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + updateRate;

        Recount();
        if (MemberCount < 2) { Cohesion = 1f; Abandoned = false; _lowSince = -1f; return; }

        Vector3 centroid = Vector3.zero;
        foreach (SoulRecord m in _members) centroid += m.transform.position;
        centroid /= MemberCount;

        float spread = 0f;
        foreach (SoulRecord m in _members) spread += Vector3.Distance(m.transform.position, centroid);
        spread /= MemberCount;

        Cohesion = Mathf.Clamp01(1f - spread / Mathf.Max(0.5f, scatterAtZero));

        // Histéresis temporal: la cohesión baja debe sostenerse para declarar abandono.
        if (Cohesion < abandonThreshold)
        {
            if (_lowSince < 0f) _lowSince = Time.time;
            if (Time.time - _lowSince >= abandonHold) Abandoned = true;
        }
        else { _lowSince = -1f; Abandoned = false; }
    }

    void Recount()
    {
        _members.Clear();
        foreach (SoulRecord s in FindObjectsOfType<SoulRecord>())
            if (s != null && s.isActiveAndEnabled) _members.Add(s);
        MemberCount = _members.Count;
    }
}
