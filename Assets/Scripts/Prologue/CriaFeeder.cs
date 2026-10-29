using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TOMAS PROGRAMADAS de un recinto de la Cría (docs/cria-simulation.md): en un centro de rescate las crías huérfanas
/// comen a horas fijas (biberón, papilla de pescado), las dé el jugador o el equipo de cuidadores. Cada
/// <see cref="interval"/> segundos, a cada cría del recinto que tenga hambre (<c>hungry ≥ 0</c>) le da una toma
/// (la deja saciada una comida). Así las crías no tienen que cazar ni pescar dentro del recinto, y no mueren de hambre
/// si el jugador no viene; el BOND, en cambio, solo lo gana quien las cuida (<see cref="CriaCareTarget"/>).
/// </summary>
public class CriaFeeder : MonoBehaviour
{
    [Tooltip("Crías de este recinto. Vacío → las Animal hijas de este objeto.")]
    public List<Animal> crias = new List<Animal>();
    [Tooltip("Segundos entre tomas.")]
    public float interval = 120f;
    public string what = "biberón";

    float _next;

    void Start()
    {
        if (crias.Count == 0) crias.AddRange(GetComponentsInChildren<Animal>());
        _next = Time.time + 5f;
    }

    // A DEMANDA (JornadaTest 2026-10-29: con tomas cada 120 s las crías pasaban buena parte del rato con hambre, buscando
    // comida por el recinto sin encontrarla —atascadas o dando vueltas—). Como con las crías huérfanas de verdad, en cuanto
    // una tiene hambre le dan su toma; `interval` queda como el mínimo entre dos tomas de la MISMA cría.
    readonly Dictionary<Animal, float> _last = new Dictionary<Animal, float>();

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + 5f;
        foreach (Animal a in crias)
        {
            if (a == null || a.Dead || a.Body == null || a.hungry < 0f) continue;
            if (_last.TryGetValue(a, out float t) && Time.time - t < Mathf.Min(interval, 30f)) continue;
            _last[a] = Time.time;
            a.hungry = -a.Body.GetMealWeight(a.Mass);   // una toma: saciada una comida
            Debug.Log($"[Cría] toma de «{a.name}» ({what}).");
        }
    }
}
