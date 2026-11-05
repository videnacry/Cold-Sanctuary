using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>Qué CRIATURA es el grupo — define qué come y quién lo come (su dieta propia):
/// Krill pasta el fitoplancton; Pez y Medusa pastan el krill. Los depredadores (Animal) lo muerden según su Forager
/// (eatsKrill para krill, eatsFish para pez y medusa —el pingüino de Adelia también come medusas—, eatsPrey para las abejas).
/// Bee: el enjambre de pecoreadoras de una colmena (<see cref="BeeSwarm"/>).</summary>
public enum SwarmKind { Fish, Krill, Jellyfish, Bee, Clam }

/// <summary>
/// GRUPO DE ANIMALES PEQUEÑOS COMO UN SOLO ORGANISMO (banco de peces, enjambre de kril, grupo de medusas, enjambre de abejas;
/// docs/grupos-de-animales.md, Berón 2026-11-03). Un GameObject PADRE que se mueve como grupo y lleva a sus INDIVIDUOS como hijos
/// (modelos low-poly de su especie, <see cref="SwarmModels"/>): se mueven todos juntos, cada uno con su sitio en el grupo y su
/// coleteo. Sin ciclo de vida por individuo (barato): el tamaño es <see cref="count"/> = el número de individuos (uno por
/// unidad, <see cref="Visible"/>), que CRECE con el tiempo (un individuo nuevo cada 1/<see cref="growthPerSecond"/> s de juego)
/// y pastando su alimento, y MENGUA al ser comido.
///
/// MORDISCO-POR-COLISIÓN: el depredador NO caza un individuo concreto — caza al PADRE: se acerca y CHOCA con el grupo (su trigger
/// cubre a los individuos); al contacto, si está hambriento, muerde: desaparece UN individuo (<see cref="Hurt"/> 1 = uno menos, al
/// momento) y obtiene sus nutrientes. Antes (auditoría 2026-11-03): el padre era un cubo translúcido escalado 10 × 2 × 10 (se veía
/// como una plataforma) y los individuos, cubos de 0,4 heredando esa escala (losas de 4 × 0,8 × 4 m); el trigger, de 40 m de radio;
/// y cada individuo valía 5 de la cuenta, así que un mordisco no quitaba ninguno a la vista. Ver docs/ice-sanctuary-ecology.md §2.2.
/// </summary>
public class Swarm : MonoBehaviour, ITarget, IEdible
{
    public static readonly List<Swarm> All = new List<Swarm>();
    public static HashSet<GameObject> population = new HashSet<GameObject>();   // para las Diets de depredadores

    [Header("Tamaño / crecimiento")]
    [FormerlySerializedAs("fishCount")]
    public float count = 20f;              // nº de individuos (hace de 'lp')
    [FormerlySerializedAs("maxFish")]
    public float maxCount = 40f;
    [Tooltip("Individuos nuevos por segundo de juego (escalado por la velocidad del juego): sin ciclo de vida, el grupo se rehace solo.")]
    public float growthPerSecond = 0.05f;
    [FormerlySerializedAs("perFishMass")]
    public float perUnitMass = 0.5f;       // kg por individuo (para Mass/Grams)

    [Header("Dieta propia (qué es y qué pasta)")]
    public SwarmKind kind = SwarmKind.Fish;
    [Tooltip("Especie de los individuos (su modelo). Si no se fija, la de su clase: pez plateado, kril, Diplulmaris o abeja de las nieves.")]
    public SwarmModels.Species species = SwarmModels.Species.SilverFish;
    public bool speciesSet;
    public float grazeRange = 6f;
    public float grazeRate = 0.4f;
    public float growthPerGraze = 1f;

    [Header("Movimiento")]
    public float driftSpeed = 1.5f;
    public float fleeSpeed = 5f;
    public float wanderRadius = 15f;
    public float fleeRange = 12f;
    [Range(0f, 1f)] public float nightDriftFactor = 0.3f;
    [Tooltip("Entra en los edificios-árbol (su fondo está abierto al mar): a veces deriva hacia dentro del tronco y vuelve a salir.")]
    public bool visitsTrees = true;
    [Tooltip("Probabilidad, en cada nuevo destino, de subir al tronco de un árbol cercano (si lo hay a < 60 m).")]
    [Range(0f, 1f)] public float treeVisitChance = 0.35f;

    [Header("Individuos (el grupo como ORGANISMO)")]
    [Tooltip("Radio en el que se reparten los individuos (y tamaño del trigger del grupo).")]
    public float schoolSpread = 4f;
    [Tooltip("Tope de individuos visibles (rendimiento).")]
    [FormerlySerializedAs("maxVisibleFish")]
    public int maxVisible = 60;
    [FormerlySerializedAs("fishPerChild")]
    [Min(1f)] public float countPerChild = 1f;
    [FormerlySerializedAs("fishPrefab")]
    public GameObject unitPrefab;
    public string unitName = "Fish";
    [Min(0.05f)] public float biteCooldown = 0.5f;

    public static int Bites { get; private set; }
    /// <summary>Anima a todos los individuos aunque estén lejos de la cámara (pruebas, fotos).</summary>
    public static bool AnimateAll;

    protected Vector3 _origin;
    protected Vector3 _target;
    Vector3 _via; bool _hasVia;
    protected readonly List<Transform> _children = new List<Transform>();
    readonly List<Vector3> _home = new List<Vector3>();
    readonly List<float> _phase = new List<float>();
    float _nextReconcile, _nextBite, _nextPredatorScan, _nextNear;
    Transform _predator;
    Vector3 _lastPos, _vel;
    bool _started;
    /// <summary>Velocidad (m/s) suavizada del grupo.</summary>
    public Vector3 Velocity => _vel;
    /// <summary>Individuos que hay ahora (hijos visibles).</summary>
    public int Visible => _children.Count;
    /// <summary>Individuos que debe haber según la cuenta.</summary>
    public int Target => Mathf.Clamp(Mathf.FloorToInt(count / Mathf.Max(1f, countPerChild) + 1e-4f), 0, maxVisible);
    /// <summary>¿Está ahora dentro del tronco de un edificio-árbol?</summary>
    public ArbolCampos InTree { get; private set; }
    public Vector3 Origin => _origin;

    protected virtual void OnEnable()  { All.Add(this);    population.Add(gameObject); }
    protected virtual void OnDisable() { All.Remove(this); population.Remove(gameObject); }

    protected virtual void Start()
    {
        // AUDITORÍA 2026-11-03: los grupos viejos de la escena eran un CUBO escalado (10 × 2 × 10) con su malla: se veían como una
        // plataforma, sus hijos heredaban la escala y el trigger salía de 40 m. El padre es un punto: sin malla y a escala 1.
        if (TryGetComponent(out MeshRenderer mr)) mr.enabled = false;
        transform.localScale = Vector3.one;
        if (!speciesSet)
        {
            species = kind == SwarmKind.Krill ? SwarmModels.Species.Krill : kind == SwarmKind.Jellyfish ? SwarmModels.Species.Diplulmaris
                    : kind == SwarmKind.Bee ? SwarmModels.Species.SnowBee : SwarmModels.Species.SilverFish;
            if (kind == SwarmKind.Krill) { maxCount = Mathf.Max(maxCount, 60f); count = Mathf.Max(count, 30f); }
        }
        countPerChild = 1f;                                   // UN individuo por unidad: un mordisco = uno menos a la vista
        maxVisible = Mathf.Max(maxVisible, Mathf.CeilToInt(maxCount));
        maxCount = Mathf.Min(maxCount, maxVisible);
        _origin = transform.position; _lastPos = _origin;
        _target = PickWanderTarget();
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = schoolSpread;
        if (!TryGetComponent(out Rigidbody rb)) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;   // un trigger que se mueve: cuerpo cinemático
        _started = true;
        ReconcileChildren();
    }

    protected byte TimeScale => TimeController.timeController != null ? TimeController.timeController.TimeSpeed : (byte)1;
    protected bool IsDay => TimeController.timeController == null || TimeController.timeController.IsDay;

    protected virtual void Update()
    {
        float dt = Time.deltaTime;
        byte timeScale = TimeScale;
        // Crece: un individuo nuevo cada cierto tiempo (sin ciclo de vida) + lo que PASTA de su alimento (fitoplancton→kril→pez).
        count = Mathf.Min(maxCount, count + growthPerSecond * timeScale * dt + GrazeFood(dt, timeScale));
        if (Time.time >= _nextReconcile) { _nextReconcile = Time.time + 0.5f; ReconcileChildren(); }
        if (Time.time >= _nextNear && count >= 1f) { _nextNear = Time.time + 0.25f; BiteByProximity(); }
        Move(dt);
        if (dt > 0f)
        {
            Vector3 v = (transform.position - _lastPos) / dt; _lastPos = transform.position;
            if (v.sqrMagnitude < 400f) _vel = Vector3.Lerp(_vel, v, 1f - Mathf.Exp(-3f * dt));
        }
        Face(dt);
        AnimateIndividuals();
    }

    /// <summary>Mueve el grupo entero (el padre). Peces, kril y medusas: huyen del depredador más cercano o derivan; a veces suben
    /// al tronco de un edificio-árbol por su fondo abierto.</summary>
    protected virtual void Move(float dt)
    {
        if (kind == SwarmKind.Clam) return;   // los bivalvos están en el fondo: no van a ninguna parte
        // (2026-11-03) Se apartan del SUBMARINO DE OBRA (como de un barco): si pasa a menos de 6 m de su casco, se alejan de él.
        ConstructionSubmarine sub = ConstructionSubmarine.Nearest(transform.position, schoolSpread + 6f);
        if (sub != null)
        {
            Vector3 away = transform.position - sub.HullCenter; if (away.sqrMagnitude < 0.01f) away = Vector3.right;
            transform.position += away.normalized * (kind == SwarmKind.Jellyfish ? 2.5f : fleeSpeed) * dt;
            return;
        }
        if (Time.time >= _nextPredatorScan) { _nextPredatorScan = Time.time + 0.25f; _predator = NearestPredator(); }   // (cada 0,25 s: buscarlo en cada fotograma, en cada grupo, pesaba)
        Transform predator = _predator;
        if (predator != null && kind != SwarmKind.Jellyfish)   // (las medusas no huyen: derivan)
        {
            Vector3 away = transform.position - predator.position; away.y = 0f;
            if (away.sqrMagnitude > 0.001f) transform.position += away.normalized * fleeSpeed * dt;
            return;
        }
        float rest = IsDay ? 1f : nightDriftFactor;
        Vector3 goal = _hasVia ? _via : _target;
        if (Vector3.Distance(transform.position, goal) < 1f)
        {
            if (_hasVia) _hasVia = false;
            else _target = PickWanderTarget();
        }
        transform.position = Vector3.MoveTowards(transform.position, _hasVia ? _via : _target, driftSpeed * rest * dt);
        if (Time.frameCount % 15 == 0) InTree = TreeAround(transform.position, true);
    }

    /// <summary>Lleva el grupo a un destino (pruebas, hechizos): deriva hacia él.</summary>
    public void GoTo(Vector3 p) { _target = p; _hasVia = false; }

    // De cara a donde va (los individuos miran hacia delante del padre).
    void Face(float dt)
    {
        Vector3 v = kind == SwarmKind.Jellyfish ? Vector3.zero : _vel;
        Vector3 flat = new Vector3(v.x, 0f, v.z);
        if (flat.sqrMagnitude < 0.01f) return;
        float pitch = Mathf.Clamp(-Mathf.Atan2(v.y, flat.magnitude) * Mathf.Rad2Deg, -35f, 35f);
        Quaternion want = Quaternion.LookRotation(flat) * Quaternion.Euler(pitch, 0f, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, want, 1f - Mathf.Exp(-2.5f * dt));
    }

    // Cada individuo: su sitio en el grupo con un vaivén propio, y su gesto (coleteo del pez, pulso de la medusa, aleteo de la abeja).
    void AnimateIndividuals()
    {
        float t = Time.time;
        // (2026-11-04) Lejos de la cámara (> 150 m) no se anima a cada individuo: son muchos y no se ven (la escena va más ligera).
        Camera cam = Camera.main;
        if (!AnimateAll && cam != null && (cam.transform.position - transform.position).sqrMagnitude > 150f * 150f) return;
        float sc = SwarmModels.DisplayScale(species);
        for (int i = 0; i < _children.Count; i++)
        {
            Transform c = _children[i];
            if (c == null) continue;
            float ph = _phase[i];
            Vector3 h = _home[i];
            switch (kind)
            {
                case SwarmKind.Jellyfish:
                {
                    float pulse = Mathf.Sin(t * 1.6f + ph);
                    c.localPosition = h + new Vector3(0f, 0.15f * Mathf.Sin(t * 0.8f + ph), 0f);
                    c.localScale = new Vector3(sc * (1f + 0.12f * pulse), sc * (1f - 0.1f * pulse), sc * (1f + 0.12f * pulse));
                    break;
                }
                case SwarmKind.Bee:
                {
                    c.localPosition = h + new Vector3(0.4f * Mathf.Sin(t * 2.1f + ph), 0.3f * Mathf.Sin(t * 3.3f + ph * 1.7f), 0.4f * Mathf.Cos(t * 1.7f + ph));
                    c.localRotation = Quaternion.Euler(0f, 25f * Mathf.Sin(t * 1.3f + ph), 0f);
                    if (c.childCount >= 4)
                    {
                        float flap = 55f * Mathf.Sin(t * 40f + ph);
                        c.GetChild(2).localRotation = Quaternion.Euler(0f, 0f, flap);
                        c.GetChild(3).localRotation = Quaternion.Euler(0f, 0f, -flap);
                    }
                    break;
                }
                default:
                {
                    float speed = Mathf.Max(0.3f, _vel.magnitude);
                    c.localPosition = h + new Vector3(0.25f * Mathf.Sin(t * 0.7f + ph), 0.12f * Mathf.Sin(t * 0.9f + ph * 1.3f), 0.3f * Mathf.Sin(t * 0.5f + ph * 0.7f));
                    float beat = (kind == SwarmKind.Krill ? 6f : 18f) * Mathf.Sin(t * (4f + 3f * speed) + ph);
                    c.localRotation = Quaternion.Euler(0f, beat, 0f);
                    break;
                }
            }
        }
    }

    float GrazeFood(float dt, byte timeScale)
    {
        if (count >= maxCount || kind == SwarmKind.Bee || kind == SwarmKind.Clam) return 0f;
        float bite = grazeRate * timeScale * dt;
        Vector3 pos = transform.position;
        if (kind == SwarmKind.Krill)
        {
            Phytoplankton food = Phytoplankton.Nearest(pos);
            if (food == null || food.Consumed) return 0f;
            if ((food.transform.position - pos).sqrMagnitude > grazeRange * grazeRange) return 0f;
            return food.Consume(bite) * growthPerGraze;
        }
        Swarm krill = Nearest(pos, SwarmKind.Krill);   // pez y medusa → kril
        if (krill == null || krill == this || krill.Consumed) return 0f;
        if ((krill.transform.position - pos).sqrMagnitude > grazeRange * grazeRange) return 0f;
        return krill.Consume(bite) * growthPerGraze;
    }

    /// <summary>El edificio-árbol en cuyo tronco está <paramref name="p"/> (null si no). <paramref name="water"/>: en su agua.</summary>
    public static ArbolCampos TreeAround(Vector3 p, bool water)
    {
        foreach (ArbolCampos t in ArbolCampos.All)
        {
            if (t == null) continue;
            Vector3 l = t.transform.InverseTransformPoint(p);
            if (Mathf.Max(Mathf.Abs(l.x), Mathf.Abs(l.z)) > t.waterHalf || l.y > t.rootHeight || l.y < -2f) continue;
            if (!water || t.InRootWater(p)) return t;
        }
        return null;
    }

    protected virtual Vector3 PickWanderTarget()
    {
        // ¿Sube al tronco de un edificio-árbol? (su fondo está abierto al mar: Berón 2026-11-03). Entra por debajo del hueco.
        ArbolCampos inT = TreeAround(transform.position, false);
        if (visitsTrees && kind != SwarmKind.Bee && inT == null && Random.value < treeVisitChance)
        {
            ArbolCampos near = null; float bd = 60f;
            foreach (ArbolCampos t in ArbolCampos.All)
            {
                if (t == null) continue;
                Vector3 b = t.transform.position; float d = new Vector2(b.x - _origin.x, b.z - _origin.z).magnitude;
                if (d < bd) { bd = d; near = t; }
            }
            if (near != null)
            {
                _via = near.transform.position + Vector3.down * 5f; _hasVia = true;   // por debajo del hueco del fondo
                return near.transform.TransformPoint(new Vector3(Random.Range(-5f, 5f), Random.Range(3f, 18f), Random.Range(-5f, 5f)));
            }
        }
        // Dentro de un tronco: sale por el fondo, al mar.
        if (inT != null) { _via = inT.transform.position + Vector3.down * 5f; _hasVia = true; }
        // En el agua: el grupo no deriva por DEBAJO de las placas de hielo (las focas que lo seguían acababan dentro de la placa).
        bool wet = !MarineLife.IceAbove(_origin);
        for (int i = 0; i < 6; i++)
        {
            Vector2 r = Random.insideUnitCircle * wanderRadius;
            Vector3 t = _origin + new Vector3(r.x, 0f, r.y);
            if (!wet || !MarineLife.IceAbove(t)) return t;
        }
        return _origin;
    }

    // MORDISCO-POR-COLISIÓN: el depredador choca con el grupo; si tiene hambre y es su dieta, desaparece UN individuo.
    void OnTriggerStay(Collider other) => TryBite(other.GetComponentInParent<Animal>());

    /// <summary>Un mordisco de <paramref name="a"/> si tiene hambre, come lo de este grupo y no acaba de morder (true si mordió). Lo
    /// llaman el choque (trigger) y, por si el animal no tiene un collider que lo dispare (la ballena, 2026-11-04: «0 mordiscos»), la
    /// cercanía: cada 0,25 s, los animales con el centro dentro del grupo.</summary>
    public bool TryBite(Animal a)
    {
        if (Time.time < _nextBite || count < 1f) return false;
        if (a == null || a.death || a.Forage == null || a.hungry < 0f) return false;
        if (!Eats(a)) return false;
        _nextBite = Time.time + biteCooldown;
        Hurt(1f);
        float nutrition = perUnitMass * Nutrition;
        a.hungry -= nutrition;
        a.GetComponent<Metabolism>()?.AbsorbFood(nutrition, Material);
        return true;
    }

    // Mordisco por CERCANÍA: un animal con su centro dentro del grupo (radio del grupo + 1 m) muerde aunque no tenga collider.
    void BiteByProximity()
    {
        float r = schoolSpread + 1f, r2 = r * r;
        Vector3 c = transform.position;
        foreach (GameObject go in Animal.wholePopulation)
        {
            if (go == null || (go.transform.position - c).sqrMagnitude > r2) continue;
            if (go.TryGetComponent(out Animal a) && TryBite(a)) return;
        }
    }

    /// <summary>¿Este animal come a los de este grupo? (kril: eatsKrill; pez: eatsFish; medusa: el pingüino —el de Adelia las
    /// come— y quien come peces; abejas: la araña de las nieves y quien caza presas al vuelo).</summary>
    public bool Eats(Animal a)
    {
        if (a == null || a.Forage == null) return false;
        switch (kind)
        {
            case SwarmKind.Krill: return a.Forage.eatsKrill;
            case SwarmKind.Clam: return a.Forage.eatsClams;
            case SwarmKind.Jellyfish: return a.SpeciesName == "Penguin" || a.Forage.eatsFish;
            case SwarmKind.Bee: return a.Forage.eatsPrey && a.SpeciesName != "Bear";
            default: return a.Forage.eatsFish;
        }
    }

    /// <summary>Ajusta los individuos a la cuenta: crecer añade, menguar o ser comido quita (uno por unidad).</summary>
    public void ReconcileChildren()
    {
        if (!_started) return;
        int target = Target;
        while (_children.Count > target)
        {
            int k = Random.Range(0, _children.Count);
            Transform t = _children[k];
            _children.RemoveAt(k); _home.RemoveAt(k); _phase.RemoveAt(k);
            if (t != null) Destroy(t.gameObject);
        }
        while (_children.Count < target) SpawnChild();
    }

    void SpawnChild()
    {
        GameObject unit = unitPrefab != null ? Instantiate(unitPrefab) : SwarmModels.Make(species);
        unit.name = unitName;
        unit.transform.SetParent(transform, false);
        Vector3 home = HomeSlot();
        unit.transform.localPosition = home;
        foreach (Collider c in unit.GetComponentsInChildren<Collider>()) Destroy(c);   // los individuos no colisionan; el grupo lleva el trigger
        _children.Add(unit.transform); _home.Add(home); _phase.Add(Random.value * 100f);
    }

    // El sitio de cada individuo: un banco es un elipsoide alargado; las medusas, sueltas en un volumen mayor; el kril, una nube
    // densa y plana; las abejas, un corro.
    Vector3 HomeSlot()
    {
        Vector3 r = Random.insideUnitSphere;
        switch (kind)
        {
            case SwarmKind.Jellyfish: return new Vector3(r.x * schoolSpread, r.y * schoolSpread * 0.6f, r.z * schoolSpread);
            case SwarmKind.Krill: return new Vector3(r.x * schoolSpread, r.y * schoolSpread * 0.35f, r.z * schoolSpread);
            case SwarmKind.Bee: return new Vector3(r.x * schoolSpread, r.y * schoolSpread * 0.5f, r.z * schoolSpread);
            default: return new Vector3(r.x * schoolSpread * 0.6f, r.y * schoolSpread * 0.3f, r.z * schoolSpread);
        }
    }

    Transform NearestPredator()
    {
        Transform best = null;
        float bestD = fleeRange;
        foreach (GameObject go in Animal.wholePopulation)
        {
            Animal a = go != null ? go.GetComponent<Animal>() : null;
            if (a == null || a.Forage == null || !a.Forage.eatsPrey) continue;
            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d < bestD) { bestD = d; best = go.transform; }
        }
        return best;
    }

    /// <summary>El grupo ACUÁTICO más cercano (un banco o enjambre en el mar: «marcador de agua» de la fauna). Las abejas no.</summary>
    public static Swarm Nearest(Vector3 position)
    {
        Swarm nearest = null;
        float bestDistSqr = float.MaxValue;
        foreach (Swarm school in All)
        {
            if (school == null || school.kind == SwarmKind.Bee) continue;
            float distSqr = (school.transform.position - position).sqrMagnitude;
            if (distSqr < bestDistSqr) { bestDistSqr = distSqr; nearest = school; }
        }
        return nearest;
    }

    /// <summary>El grupo no-agotado más cercano de una CLASE dada (para dietas: un pez busca el banco de krill).</summary>
    public static Swarm Nearest(Vector3 position, SwarmKind kind)
    {
        Swarm nearest = null;
        float bestDistSqr = float.MaxValue;
        foreach (Swarm school in All)
        {
            if (school == null || school.kind != kind || school.Consumed) continue;
            float distSqr = (school.transform.position - position).sqrMagnitude;
            if (distSqr < bestDistSqr) { bestDistSqr = distSqr; nearest = school; }
        }
        return nearest;
    }

    // ── ITarget ────────────────────────────────────────────────────────────────
    public float Mass     => count * perUnitMass;
    public float Speed    => driftSpeed;
    public char  Faction  => 'f';
    public bool  Dead     => count <= 0.5f;
    public bool  Consumed => count < 1f;
    /// <summary>Un mordisco: tantos individuos menos (1 = uno), y desaparecen AL MOMENTO.</summary>
    public void  Hurt(float damage) { count = Mathf.Max(0f, count - damage); Bites++; ReconcileChildren(); }

    // ── IEdible ────────────────────────────────────────────────────────────────
    public OrganicMaterial Material => OrganicMaterial.Fish;
    public float Nutrition => 1f;
    public float Toughness => 0.1f;
    public float Grams     => count * perUnitMass;

    public float Consume(float biteSize)
    {
        float caught = Mathf.Min(biteSize, count);
        count -= caught;
        if (caught >= 1f) ReconcileChildren();
        return caught * perUnitMass * Nutrition;   // no despawnea: se rehace con el tiempo
    }

    public void Graze(float amount) => Hurt(amount);

    public static string UnitName(SwarmModels.Species sp) => sp == SwarmModels.Species.Clam ? "Almeja" : sp == SwarmModels.Species.SnowBee ? "Abeja" : sp == SwarmModels.Species.Krill ? "Kril" : sp == SwarmModels.Species.SilverFish ? "Pez" : "Medusa";

    /// <summary>Crea un grupo nuevo (en juego o desde un constructor): el padre es un punto vacío con su Swarm.</summary>
    public static T Create<T>(Transform parent, string name, Vector3 pos, SwarmKind kind, SwarmModels.Species sp, float count, float max, float spread) where T : Swarm
    {
        GameObject g = new GameObject(name);
        if (parent != null) g.transform.SetParent(parent, true);
        g.transform.position = pos;
        T s = g.AddComponent<T>();
        s.kind = kind; s.species = sp; s.speciesSet = true; s.count = count; s.maxCount = max; s.maxVisible = Mathf.CeilToInt(max);
        s.schoolSpread = spread; s.unitName = UnitName(sp);
        if (kind == SwarmKind.Krill) s.perUnitMass = 0.05f;
        if (kind == SwarmKind.Clam) { s.perUnitMass = 0.5f; s.driftSpeed = 0f; s.growthPerSecond = 0.05f; }
        if (kind == SwarmKind.Jellyfish) { s.perUnitMass = sp == SwarmModels.Species.Desmonema ? 4f : 0.1f; s.driftSpeed = 0.5f; s.growthPerSecond = 0.01f; }
        return s;
    }
}
