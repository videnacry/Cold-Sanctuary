using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Añadir al GameObject del jugador para que los animales puedan tratarlo como presa o amenaza.
/// Los animales que quieran cazar al jugador deben incluir PlayerTarget.population en su Diet.
/// Ver docs/behavior-system.md (Componente C).
/// </summary>
public class PlayerTarget : MonoBehaviour, ITarget
{
    public float mass = 80f;
    public float speed = 5f;
    public float lp = 100f;
    public char faction = 'h'; // humano

    public static HashSet<GameObject> population = new HashSet<GameObject>();

    bool _dead = false;

    // ITarget
    public float Mass => mass;
    public float Speed => speed;
    public char Faction => faction;
    public bool Dead => _dead;
    public bool Consumed => false;

    void Start() => population.Add(gameObject);
    void OnDestroy() => population.Remove(gameObject);

    public void Hurt(float damage)
    {
        lp -= damage;
        if (lp <= 0 && !_dead)
        {
            _dead = true;
            Debug.Log("Player killed.");
        }
    }
}
