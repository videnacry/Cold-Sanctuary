using UnityEngine;

/// <summary>
/// NASATYA (Ambrosio renacido): enorme, manso y fortísimo, recostado en una POSTURA RARA (eco de su deformidad); de vez
/// en cuando se acomoda. El «tell» de su vida pasada.
/// </summary>
public class MicroLounger : MonoBehaviour
{
    float _next;
    Quaternion _base;

    void Start() { _base = transform.rotation; transform.rotation = _base * Quaternion.Euler(0f, 0f, 70f); _next = Time.time + 8f; }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + Random.Range(8f, 14f);
        transform.rotation = _base * Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(-30f, 30f), Random.Range(60f, 80f));
    }
}
