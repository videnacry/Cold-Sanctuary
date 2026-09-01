using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// ENJAMBRE/BANCO como ORGANISMO: una sola entidad que representa a muchas criaturas pequeñas (peces, krill,
/// y a futuro bandadas/enjambres de otros santuarios). Deriva por su medio, huye de depredadores cercanos, y su
/// tamaño (<see cref="count"/>, que hace de "lp") crece — comiendo su productor (fitoplancton) y con un pequeño
/// autoregenerado — y mengua al ser comido; se recupera desde 0. Es ITarget + IEdible.
///
/// MORDISCO-POR-COLISIÓN: el depredador NO caza un individuo — se acerca y CHOCA con el enjambre (su trigger cubre
/// a los hijos); al contacto, si está hambriento, muerde: desaparece una cría y obtiene sus nutrientes.
/// El mismo enjambre PASTA a su productor (fitoplancton) por cercanía para crecer. Barato: 1 entidad por enjambre
/// (no miles de criaturas). Antes se llamaba `FishSchool`. Ver docs/ice-sanctuary-ecology.md §2.2.
/// </summary>
public class Swarm : MonoBehaviour, ITarget, IEdible
{
    public static readonly List<Swarm> All = new List<Swarm>();
    public static HashSet<GameObject> population = new HashSet<GameObject>();   // para las Diets de depredadores

    [Header("Tamaño / crecimiento")]
    [FormerlySerializedAs("fishCount")]
    public float count = 20f;              // nº de criaturas ~ tamaño del enjambre (hace de 'lp')
    [FormerlySerializedAs("maxFish")]
    public float maxCount = 60f;
    public float growthPerSecond = 0.05f;  // autoregenerado base (escalado por la velocidad del juego)
    [FormerlySerializedAs("perFishMass")]
    public float perUnitMass = 0.5f;       // kg por cría (para Mass/Grams)

    [Header("Alimentación (pasta a su productor: fitoplancton)")]
    [Tooltip("Distancia a la que el enjambre pasta el fitoplancton más cercano.")]
    public float grazeRange = 6f;
    [Tooltip("Biomasa/seg que arranca del fitoplancton y convierte en crecimiento (escalado por la velocidad del juego).")]
    public float grazeRate = 0.4f;
    [Tooltip("Cuánto crece el enjambre por unidad de biomasa pastada.")]
    public float growthPerGraze = 1f;

    [Header("Movimiento")]
    public float driftSpeed = 1.5f;        // deriva tranquila
    public float fleeSpeed = 5f;
    public float wanderRadius = 15f;
    public float fleeRange = 12f;          // distancia a la que detecta depredadores

    [Header("Crías visibles (el enjambre como ORGANISMO — docs/ice-sanctuary-ecology.md §2.2)")]
    [Tooltip("Radio en el que se reparten las crías (y tamaño del collider del enjambre).")]
    public float schoolSpread = 4f;
    [Tooltip("Tope de crías VISIBLES (rendimiento): el enjambre no muestra miles.")]
    [FormerlySerializedAs("maxVisibleFish")]
    public int maxVisible = 12;
    [Tooltip("Cada cría visible representa N del count (crecer multiplica las crías; menguar las quita).")]
    [FormerlySerializedAs("fishPerChild")]
    [Min(1f)] public float countPerChild = 5f;
    [Tooltip("Prefab de la cría (opcional; si null, se crean primitivas).")]
    [FormerlySerializedAs("fishPrefab")]
    public GameObject unitPrefab;
    [Tooltip("Nombre de la cría (Fish, Krill…).")]
    public string unitName = "Fish";
    [Tooltip("Segundos mínimos entre mordiscos al enjambre (mordisco-por-colisión).")]
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
        // El enjambre es UN cuerpo con un trigger que CUBRE el área de las crías → los depredadores chocan con el
        // enjambre (mordisco-por-colisión), no con una cría concreta. Las crías van parentadas → se mueven con él.
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = schoolSpread;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        byte timeScale = TimeController.timeController != null ? TimeController.timeController.TimeSpeed : (byte)1;

        // Crece: autoregenerado base + lo que PASTA del fitoplancton cercano (fitoplancton→enjambre).
        count = Mathf.Min(maxCount, count + growthPerSecond * timeScale * dt + GrazePhytoplankton(dt, timeScale));

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

    // PASTOREO DEL PRODUCTOR: el enjambre (krill/pez) arranca biomasa del fitoplancton más cercano dentro de rango y
    // la convierte en crecimiento. Es el eslabón fitoplancton→enjambre (paralelo al mordisco depredador→enjambre).
    float GrazePhytoplankton(float dt, byte timeScale)
    {
        if (count >= maxCount) return 0f;
        Phytoplankton food = Phytoplankton.Nearest(transform.position);
        if (food == null || food.Consumed) return 0f;
        if ((food.transform.position - transform.position).sqrMagnitude > grazeRange * grazeRange) return 0f;
        float bite = grazeRate * timeScale * dt;
        float eaten = food.Consume(bite);          // reduce la biomasa del productor (se autoregenera con la luz)
        return eaten * growthPerGraze;
    }

    Vector3 PickWanderTarget()
    {
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        return _origin + new Vector3(r.x, 0f, r.y);
    }

    // MORDISCO-POR-COLISIÓN (docs/ice-sanctuary-ecology.md §2.2): el depredador NO caza una cría concreta — se acerca y
    // CHOCA con el enjambre (su trigger cubre a las crías); al contacto, si está hambriento, muerde: desaparece una cría
    // (Hurt→Reconcile la quita) y obtiene los nutrientes de una cría. Throttled por biteCooldown.
    void OnTriggerStay(Collider other)
    {
        if (Time.time < _nextBite || count <= 0f) return;
        Animal a = other.GetComponentInParent<Animal>();
        if (a == null || a.death || a.Forage == null || !a.Forage.eatsFish || a.hungry < 0f) return;
        _nextBite = Time.time + biteCooldown;
        Hurt(1f);                                                    // una cría menos (Reconcile la quita visualmente)
        float nutrition = perUnitMass * Nutrition;
        a.hungry -= nutrition;
        a.GetComponent<Metabolism>()?.AbsorbFood(nutrition, Material);   // opt-in (como Forager.Eat)
    }

    // Ajusta el nº de crías VISIBLES al tamaño del enjambre: crecer las multiplica, menguar/ser comido las quita.
    // Barato: capado a maxVisible (no miles). Van parentadas → se mueven con el enjambre (movimiento coherente).
    void ReconcileChildren()
    {
        int target = Mathf.Clamp(Mathf.RoundToInt(count / Mathf.Max(1f, countPerChild)), 0, maxVisible);
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
        GameObject unit = unitPrefab != null ? Instantiate(unitPrefab) : GameObject.CreatePrimitive(PrimitiveType.Cube);
        unit.name = unitName;
        unit.transform.SetParent(transform, false);
        Vector2 r = Random.insideUnitCircle * schoolSpread;
        unit.transform.localPosition = new Vector3(r.x, Random.Range(-1f, 1f), r.y);
        unit.transform.localScale = Vector3.one * 0.4f;
        Collider c = unit.GetComponent<Collider>();
        if (c != null) Destroy(c);   // las crías no colisionan; el enjambre (padre) lleva el trigger
        _children.Add(unit.transform);
    }

    Transform NearestPredator()
    {
        Transform best = null;
        float bestD = fleeRange;
        foreach (GameObject go in Animal.wholePopulation)
        {
            Animal a = go != null ? go.GetComponent<Animal>() : null;   // depredador del enjambre = come presa (eatsPrey)
            if (a == null || a.Forage == null || !a.Forage.eatsPrey) continue;
            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d < bestD) { bestD = d; best = go.transform; }
        }
        return best;
    }

    public static Swarm Nearest(Vector3 position)
    {
        Swarm nearest = null;
        float bestDistSqr = float.MaxValue;
        foreach (Swarm school in All)
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
    public float Mass     => count * perUnitMass;
    public float Speed    => driftSpeed;
    public char  Faction  => 'f';
    public bool  Dead     => count <= 0.5f;
    public bool  Consumed => count <= 0f;
    public void  Hurt(float damage) => count = Mathf.Max(0f, count - damage);

    // ── IEdible ────────────────────────────────────────────────────────────────
    public OrganicMaterial Material => OrganicMaterial.Fish;
    public float Nutrition => 1f;
    public float Toughness => 0.1f;
    public float Grams     => count * perUnitMass;

    public float Consume(float biteSize)
    {
        float caught = Mathf.Min(biteSize, count);
        count -= caught;
        return caught * perUnitMass * Nutrition;   // no despawnea: se autoregenera con el tiempo
    }

    /// <summary>Depleción por pastoreo de herbívoros marinos (ballena/foca).</summary>
    public void Graze(float amount) => Hurt(amount);
}
