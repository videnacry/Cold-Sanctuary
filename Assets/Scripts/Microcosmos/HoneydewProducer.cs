using UnityEngine;

/// <summary>
/// El PULGÓN produce **melaza** (honeydew) cada cierto tiempo — el "líquido codiciado" por el que las
/// hormigas lo cuidan y ordeñan (**mirmecofilia**, docs/microcosmos-insects.md §2). Es el análogo de la
/// **cría/ganadería** a escala insecto. Barato: solo un contador con log.
/// </summary>
public class HoneydewProducer : MonoBehaviour
{
    [Min(0.2f)] public float interval = 3f;
    public int honeydew;

    float _next;

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + interval;
        honeydew++;
        Debug.Log($"[Micro] «{name}» (pulgón) produce melaza ({honeydew}). Las hormigas la ansían.");
    }
}
