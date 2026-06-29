using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Añadir al GameObject del jugador para que los animales puedan tratarlo como presa o amenaza.
/// Los animales que quieran cazar al jugador deben incluir PlayerTarget.population en su Diet.
/// Ver docs/behavior-system.md (Componente C).
/// </summary>
public class PlayerTarget : MonoBehaviour, ITarget, ICarrier
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

    // ICarrier — teclas E (recoger) y Q (soltar) se wirean desde PlayerCtrl
    FoodItem _carriedFood;
    public FoodItem CarriedFood => _carriedFood;

    public bool PickUp(FoodItem food)
    {
        if (_carriedFood != null || food == null || food.Consumed) return false;
        _carriedFood = food;
        food.transform.SetParent(transform);
        food.transform.localPosition = Vector3.forward + Vector3.up;
        return true;
    }

    public FoodItem Drop(Vector3 position)
    {
        if (_carriedFood == null) return null;
        FoodItem dropped = _carriedFood;
        _carriedFood = null;
        dropped.transform.SetParent(null);
        dropped.transform.position = position;
        dropped.droppedBy = this;
        return dropped;
    }

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
