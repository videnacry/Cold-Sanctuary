using UnityEngine;

/// <summary>
/// NASATYA (Ambrosio renacido): enorme, manso y fortísimo, recostado en una POSTURA RARA (eco de su deformidad); de vez
/// en cuando se acomoda. El «tell» de su vida pasada.
/// Y, como en la novela (docs/novela.md, «7. Avatar en la Cocina»: lo ve «moverse ligero y veloz llevando restos, y se une
/// a ayudarlo»), entre rato y rato recostado se levanta y lleva restos a la guarida, deprisa (una recolectora más,
/// <see cref="MicroForager"/>, más rápida). Cada viaje tiene tope: si no entrega a tiempo, vuelve a recostarse.
/// </summary>
public class MicroLounger : MonoBehaviour
{
    [Tooltip("Velocidad al llevar restos: ligero y veloz pese a su tamaño.")]
    public float carrySpeed = 5f;
    [Tooltip("Tope de cada viaje llevando restos (s).")]
    public float maxTrip = 40f;

    float _next, _tripEnd;
    int _deliveredAtStart;
    bool _carrying;
    Quaternion _base;
    MicroForager _forager;

    void Start()
    {
        _base = transform.rotation;
        transform.rotation = _base * Quaternion.Euler(0f, 0f, 70f);
        _next = Time.time + 8f;
        _forager = GetComponent<MicroForager>();
        if (_forager == null) _forager = gameObject.AddComponent<MicroForager>();
        _forager.speed = carrySpeed;
        _forager.enabled = false;
    }

    void Update()
    {
        if (_carrying)
        {
            if (_forager.delivered > _deliveredAtStart || Time.time > _tripEnd) Lounge();
            return;
        }
        if (Time.time < _next) return;
        // Uno de cada dos ratos se levanta a llevar restos; el otro, solo se acomoda.
        if (Random.value < 0.5f) { Carry(); return; }
        _next = Time.time + Random.Range(8f, 14f);
        transform.rotation = _base * Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(-30f, 30f), Random.Range(60f, 80f));
    }

    void Carry()
    {
        _carrying = true; _deliveredAtStart = _forager.delivered; _tripEnd = Time.time + maxTrip;
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);   // de pie para andar
        _forager.enabled = true;
    }

    void Lounge()
    {
        _carrying = false; _forager.enabled = false;
        _base = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        transform.rotation = _base * Quaternion.Euler(0f, 0f, 70f);
        _next = Time.time + Random.Range(8f, 14f);
    }
}
