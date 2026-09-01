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

    [Header("Peces hijos (el banco como ORGANISMO — docs/ice-sanctuary-ecology.md §2.2)")]
    [Tooltip("Radio en el que se reparten los peces hijos (y tamaño del collider del banco).")]
    public float schoolSpread = 4f;
    [Tooltip("Tope de peces hijos VISIBLES (rendimiento): el banco no muestra miles.")]
    public int maxVisibleFish = 12;
    [Tooltip("Cada pez hijo visible representa N del fishCount (crecer multiplica los hijos; menguar los quita).")]
    [Min(1f)] public float fishPerChild = 5f;
    [Tooltip("Prefab del pez hijo (opcional; si null, se crean primitivas).")]
    public GameObject fishPrefab;
    [Tooltip("Segundos mínimos entre mordiscos al banco (mordisco-por-colisión).")]
    [Min(0.05f)] public float biteCooldown = 0.5f;

    Vector3 _origin;
    Vector3 _target;
    readonly List<Transform> _children = new List<Transform>();
    float _nextReconcile;
    float _nextBite;

    void OnEnable()  { All.Add(this);    population.Add(gameObject); }
    void OnDisable() { All.Remove(this); population.Remove(gameObject); }

    void Start()
    {
        _origin = transform.position;
        _target = PickWanderTarget();
        // El banco es UN cuerpo con un trigger que CUBRE el área de los hijos → los depredadores chocan con el banco
        // (mordisco-por-colisión, siguiente pieza), no con un pez concreto. Los hijos van parentados → se mueven con él.
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = schoolSpread;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Crece con el tiempo (escalado por la velocidad del juego); se autoregenera desde 0.
        byte timeScale = TimeController.timeController != null ? TimeController.timeController.TimeSpeed : (byte)1;
        fishCount = Mathf.Min(maxFish, fishCount + growthPerSecond * timeScale * dt);

        if (Time.time >= _nextReconcile) { _nextReconcile = Time.time + 0.5f; ReconcileChildren(); }

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

    // MORDISCO-POR-COLISIÓN (docs/ice-sanctuary-ecology.md §2.2): el depredador NO caza un pez concreto — se acerca y
    // CHOCA con el banco (su trigger cubre a los hijos); al contacto, si está hambriento, muerde: desaparece un pez
    // (Hurt→Reconcile quita un hijo) y obtiene los nutrientes de un pez. Throttled por biteCooldown.
    void OnTriggerStay(Collider other)
    {
        if (Time.time < _nextBite || fishCount <= 0f) return;
        Animal a = other.GetComponentInParent<Animal>();
        if (a == null || a.death || a.Forage == null || !a.Forage.eatsFish || a.hungry < 0f) return;
        _nextBite = Time.time + biteCooldown;
        Hurt(1f);                                                    // un pez menos (Reconcile lo quita visualmente)
        float nutrition = perFishMass * Nutrition;
        a.hungry -= nutrition;
        a.GetComponent<Metabolism>()?.AbsorbFood(nutrition, Material);   // opt-in (como Forager.Eat)
    }

    // Ajusta el nº de peces hijos VISIBLES al tamaño del banco: crecer los multiplica, menguar/ser comido los quita.
    // Barato: capado a maxVisibleFish (no miles). Van parentados → se mueven con el banco (movimiento coherente).
    void ReconcileChildren()
    {
        int target = Mathf.Clamp(Mathf.RoundToInt(fishCount / Mathf.Max(1f, fishPerChild)), 0, maxVisibleFish);
        while (_children.Count > target)
        {
            Transform t = _children[_children.Count - 1];
            _children.RemoveAt(_children.Count - 1);
            if (t != null) Destroy(t.gameObject);
        }
        while (_children.Count < target) SpawnChild();
    }

    void SpawnChild()
    {
        GameObject fish = fishPrefab != null ? Instantiate(fishPrefab) : GameObject.CreatePrimitive(PrimitiveType.Cube);
        fish.name = "Fish";
        fish.transform.SetParent(transform, false);
        Vector2 r = Random.insideUnitCircle * schoolSpread;
        fish.transform.localPosition = new Vector3(r.x, Random.Range(-1f, 1f), r.y);
        fish.transform.localScale = Vector3.one * 0.4f;
        Collider c = fish.GetComponent<Collider>();
        if (c != null) Destroy(c);   // los hijos no colisionan; el banco (padre) lleva el trigger
        _children.Add(fish.transform);
    }

    Transform NearestPredator()
    {
        Transform best = null;
        float bestD = fleeRange;
        foreach (GameObject go in Animal.wholePopulation)
        {
            Animal a = go != null ? go.GetComponent<Animal>() : null;   // depredador del banco = come presa (eatsPrey), ya no el tipo Carnivore
            if (a == null || a.Forage == null || !a.Forage.eatsPrey) continue;
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
