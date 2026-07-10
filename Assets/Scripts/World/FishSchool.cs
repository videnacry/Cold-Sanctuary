using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Banco de peces como ORGANISMO: una sola entidad que representa a muchos peces. Deriva por el agua,
/// huye de depredadores cercanos, y su tamaño (fishCount, que hace de "lp") crece con el tiempo y mengua
/// al ser comido; se autoregenera desde 0. Es ITarget + IEdible → los carnívoros (oso polar, zorro) lo
/// pescan vía Diet, y los herbívoros marinos (ballena/foca) lo pastan vía Herbivore.Feed. Barato: 1
/// entidad por banco (no miles de peces). Ver docs/refuge-and-adult-behavior.md.
/// </summary>
public class FishSchool : MonoBehaviour, ITarget, IEdible
{
    public static readonly List<FishSchool> All = new List<FishSchool>();
    public static HashSet<GameObject> population = new HashSet<GameObject>();   // para las Diets de depredadores

    [Header("Tamaño / crecimiento")]
    public float fishCount = 20f;          // nº de peces ~ tamaño del banco (hace de 'lp')
    public float maxFish = 60f;
    public float growthPerSecond = 0.05f;  // crece con el tiempo (escalado por la velocidad del juego)
    public float perFishMass = 0.5f;       // kg por pez (para Mass/Grams)

    [Header("Movimiento")]
    public float driftSpeed = 1.5f;        // deriva tranquila
    public float fleeSpeed = 5f;
    public float wanderRadius = 15f;
    public float fleeRange = 12f;          // distancia a la que detecta depredadores

    Vector3 _origin;
    Vector3 _target;

    void OnEnable()  { All.Add(this);    population.Add(gameObject); }
    void OnDisable() { All.Remove(this); population.Remove(gameObject); }

    void Start()
    {
        _origin = transform.position;
        _target = PickWanderTarget();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Crece con el tiempo (escalado por la velocidad del juego); se autoregenera desde 0.
        byte timeScale = TimeController.timeController != null ? TimeController.timeController.TimeSpeed : (byte)1;
        fishCount = Mathf.Min(maxFish, fishCount + growthPerSecond * timeScale * dt);

        // Huye del depredador más cercano; si no hay, deriva tranquilo.
        Transform predator = NearestPredator();
        if (predator != null)
        {
            Vector3 away = transform.position - predator.position; away.y = 0f;
            if (away.sqrMagnitude > 0.001f) transform.position += away.normalized * fleeSpeed * dt;
        }
        else
        {
            if (Vector3.Distance(transform.position, _target) < 1f) _target = PickWanderTarget();
            transform.position = Vector3.MoveTowards(transform.position, _target, driftSpeed * dt);
        }
    }

    Vector3 PickWanderTarget()
    {
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        return _origin + new Vector3(r.x, 0f, r.y);
    }

    Transform NearestPredator()
    {
        Transform best = null;
        float bestD = fleeRange;
        foreach (GameObject go in Animal.wholePopulation)
        {
            if (go == null || go.GetComponent<Carnivore>() == null) continue;
            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d < bestD) { bestD = d; best = go.transform; }
        }
        return best;
    }

    public static FishSchool Nearest(Vector3 position)
    {
        FishSchool nearest = null;
        float bestDistSqr = float.MaxValue;
        foreach (FishSchool school in All)
        {
            float distSqr = (school.transform.position - position).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                nearest = school;
            }
        }
        return nearest;
    }

    // ── ITarget ────────────────────────────────────────────────────────────────
    public float Mass     => fishCount * perFishMass;
    public float Speed    => driftSpeed;
    public char  Faction  => 'f';
    public bool  Dead     => fishCount <= 0.5f;
    public bool  Consumed => fishCount <= 0f;
    public void  Hurt(float damage) => fishCount = Mathf.Max(0f, fishCount - damage);

    // ── IEdible ────────────────────────────────────────────────────────────────
    public OrganicMaterial Material => OrganicMaterial.Fish;
    public float Nutrition => 1f;
    public float Toughness => 0.1f;
    public float Grams     => fishCount * perFishMass;

    public float Consume(float biteSize)
    {
        float caught = Mathf.Min(biteSize, fishCount);
        fishCount -= caught;
        return caught * perFishMass * Nutrition;   // no despawnea: se autoregenera con el tiempo
    }

    /// <summary>Depleción por pastoreo de herbívoros marinos (ballena/foca).</summary>
    public void Graze(float amount) => Hurt(amount);
}
