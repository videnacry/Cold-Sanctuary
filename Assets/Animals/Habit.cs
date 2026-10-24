using UnityEngine;

/// <summary>
/// HÁBITO NATURAL (docs/natural-interactions.md): ejecuta UNA actividad del <see cref="Ethogram"/> de su especie sobre un
/// <see cref="Affordance"/> real del entorno — ir al tallo y chupar savia, cortar hierba y llevarla al termitero, acechar
/// junto a un respiradero, llevar un guijarro al nido, asolearse en lo alto de una brizna… Lo conduce el deseo «habit» de
/// <see cref="DesireCatalog"/> (gana cuando el ser está en calma y saciado, por encima del merodeo al azar): cada tick de
/// volición renueva el permiso; si otro deseo gana (hambre, amenaza, cría), el permiso caduca y el hábito suelta su sitio.
/// Los marinos (con <see cref="MarineLife"/>) no lo usan: su etograma lo ejecuta MarineLife en el agua.
/// </summary>
[RequireComponent(typeof(Animal))]
public class Habit : MonoBehaviour
{
    enum State { None, Going, Doing, Carrying }

    [Tooltip("Radio base en el que busca dónde hacer sus cosas (× la percepción).")]
    public float searchRadius = 40f;

    /// <summary>Lo que está haciendo ahora («chupa savia del tallo»), o vacío.</summary>
    public string Doing { get; private set; } = "";
    public bool Active => _s != State.None;

    Animal _a;
    State _s;
    EthoAct _act;
    Affordance _target, _nest;
    Vector3 _dest;
    GameObject _load;
    float _until, _lease, _nextScan, _nextWalk, _cool;
    bool _hasOptions;
    static int _logged;

    void Awake() { _a = GetComponent<Animal>(); }

    void Start()
    {
        // Un pulgón chupando savia produce MELAZA: la ofrece como interactuable (las hormigas lo ordeñan).
        if (_a != null && _a.SpeciesName == "Aphid" && GetComponent<Affordance>() == null)
            Affordance.Put(gameObject, AffordanceKind.Honeydew, 2, 0f, 0f).maxStock = 1f;
    }

    bool Marine => GetComponent<MarineLife>() != null;
    float Radius => (searchRadius + 40f * _a.perception) * (GetComponentInParent<MicroSize>() != null ? 2f : 1f);
    float Lease => (TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 2f) * 3f + 2f;

    /// <summary>¿Tiene algo natural que hacer cerca? (cacheado; lo consulta la volición cada tick).</summary>
    public bool HasOptions()
    {
        if (_a == null || Marine) return false;
        if (_s != State.None) return true;
        if (Time.time < _cool) return false;
        if (Time.time >= _nextScan)
        {
            _nextScan = Time.time + 3f;
            _hasOptions = false;
            foreach (EthoAct e in Ethogram.Of(_a.SpeciesName))
                if (Find(e) != null || (e.effect == HabitEffect.Huddle && Night)) { _hasOptions = true; break; }
        }
        return _hasOptions;
    }

    /// <summary>Lo llama el deseo «habit»: renueva el permiso y, si no hace nada, empieza una actividad.</summary>
    public void Step()
    {
        _lease = Time.time + Lease;
        if (_s == State.None && Time.time >= _cool) Begin();
    }

    static bool Night => TimeController.timeController != null && !TimeController.timeController.IsDay;

    Affordance Find(EthoAct e)
    {
        if (e.effect == HabitEffect.Huddle) return null;
        Affordance best = null; float bd = Radius * Radius;
        foreach (Affordance a in Affordance.Of(e.kind))
        {
            if (a == null || a == _nest || !a.isActiveAndEnabled || !a.Available(_a) || a.gameObject == gameObject) continue;
            float d = (a.transform.position - transform.position).sqrMagnitude;
            if (d < bd) { bd = d; best = a; }
        }
        return best;
    }

    void Begin()
    {
        EthoAct[] acts = Ethogram.Of(_a.SpeciesName);
        float total = 0f;
        var found = new Affordance[acts.Length];
        for (int i = 0; i < acts.Length; i++)
        {
            found[i] = Find(acts[i]);
            if (found[i] != null || (acts[i].effect == HabitEffect.Huddle && Night)) total += acts[i].weight;
        }
        if (total <= 0f) { _cool = Time.time + 5f; return; }
        float r = Random.value * total;
        for (int i = 0; i < acts.Length; i++)
        {
            bool ok = found[i] != null || (acts[i].effect == HabitEffect.Huddle && Night);
            if (!ok) continue;
            r -= acts[i].weight;
            if (r > 0f && i < acts.Length - 1) continue;
            _act = acts[i]; _target = found[i];
            if (_act.effect == HabitEffect.Huddle) { if (!Colony(out _dest)) { _cool = Time.time + 5f; return; } }
            else _dest = _target.transform.position;
            _s = State.Going; Doing = _act.verb; _nextWalk = 0f;
            return;
        }
    }

    // Centro de los de su especie cercanos (para apiñarse).
    bool Colony(out Vector3 c)
    {
        c = Vector3.zero; int n = 0;
        foreach (GameObject g in _a.Population)
            if (g != null && g != gameObject && Vector3.Distance(g.transform.position, transform.position) < 25f) { c += g.transform.position; n++; }
        if (n < 2) return false;
        c /= n; return true;
    }

    void Update()
    {
        if (_s == State.None) return;
        if (_a == null || _a.death || _a.asleep || _a.busy || Time.time > _lease) { Stop(); return; }
        switch (_s)
        {
            case State.Going:
                if (_target != null && !_target.Available(_a)) { Stop(); return; }
                if (_target != null) _dest = _target.transform.position;
                if (!Move(_dest)) return;
                if (_target != null && !_target.Occupy(_a)) { Stop(); return; }
                _s = State.Doing; _until = Time.time + _act.seconds;
                Halt();
                if (_logged++ < 40 || _logged % 50 == 0) Debug.Log($"[Hábito] «{name}» ({_a.SpeciesName}) {_act.verb}.");
                break;
            case State.Doing:
                Apply(Time.deltaTime);
                if (Time.time >= _until) Finish();
                break;
            case State.Carrying:
                if (!Move(_a.HomeOrigin)) return;
                Deliver();
                Stop();
                break;
        }
    }

    float Arrive
    {
        get
        {
            float s = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
            var nav = _a.nav;
            return Mathf.Max(1.5f, nav != null ? nav.radius * s * 2.5f : 1.5f);
        }
    }

    // Un paso hacia el destino: por NavMesh (gait de andar) si lo hay; si no, en línea recta. true = ha llegado.
    bool Move(Vector3 to)
    {
        Vector3 d = to - transform.position; d.y = 0f;
        if (d.magnitude <= Arrive) return true;
        if (_a.nav != null && _a.nav.enabled && _a.nav.isOnNavMesh)
        {
            if (Time.time >= _nextWalk) { _nextWalk = Time.time + 1f; _a.Loco?.Walk(to, 1f); }
            return false;
        }
        float v = _a.ActsPrep != null && _a.ActsPrep.walk != null ? _a.ActsPrep.walk.navSpeed : 2f;
        return MicroMove.Step(transform, to, v, Arrive);
    }

    void Halt() { if (_a.nav != null && _a.nav.enabled && _a.nav.isOnNavMesh) _a.nav.ResetPath(); _a.Loco?.Idle(1f); }

    float MealWeight => _a.Body != null && _a.rig != null ? _a.Body.GetMealWeight(_a.rig.mass) : 0.1f;

    void Apply(float dt)
    {
        switch (_act.effect)
        {
            case HabitEffect.Feed:
            {
                float got = _target != null ? _target.Take(dt / Mathf.Max(1f, _act.seconds), _a) : 0f;
                _a.hungry -= MealWeight * 0.6f * got;
                if (_a.SpeciesName == "Aphid") GetComponent<Affordance>()?.Add(got * 0.5f);   // savia → melaza
                if (_target != null && _target.kind == AffordanceKind.Honeydew && _target.Stock <= 0.01f) _until = Time.time;   // ordeñado
                break;
            }
            case HabitEffect.Rest: case HabitEffect.Bask:
                _a.exhaustion = Mathf.Max(0f, _a.exhaustion - dt * 0.5f);
                _a.sleepiness = Mathf.Max(0f, _a.sleepiness - dt * 0.002f);
                break;
            case HabitEffect.Huddle:
                _a.exhaustion = Mathf.Max(0f, _a.exhaustion - dt * 0.5f);
                _a.stress = Mathf.Max(0f, _a.stress - dt * 0.01f);
                if (Time.frameCount % 30 == 0 && Colony(out Vector3 c) && Vector3.Distance(c, transform.position) > Arrive * 2f) Move(c);   // la piña rota: los de fuera se meten
                break;
            case HabitEffect.Shelter: case HabitEffect.Ambush:
                _a.stress = Mathf.Max(0f, _a.stress - dt * 0.01f);
                break;
            case HabitEffect.Watch:
                _a.alertness = Mathf.Max(_a.alertness, 0.3f);   // vigila sin asustarse (centinela, patrulla)
                break;
            case HabitEffect.StillHunt:
                StillHunt();
                break;
            case HabitEffect.Signal:
                // Llama a pareja (destellos/canto) solo si el ser está en celo; si no, es un rato de descanso en el posadero.
                MatingSignal sig = GetComponent<MatingSignal>();
                if (sig == null || !sig.Display()) { _a.exhaustion = Mathf.Max(0f, _a.exhaustion - dt * 0.5f); break; }
                _a.exhaustion += dt * 0.02f;   // cantar/destellar gasta
                break;
        }
    }

    // ACECHO EN EL RESPIRADERO (oso polar): espera inmóvil; si una foca sale a respirar por ESE agujero, la caza.
    void StillHunt()
    {
        if (_target == null || _target.lastUser == null || Time.time - _target.lastUseTime > 2f) return;
        if (!(_target.lastUser is Animal seal) || seal.death || seal.SpeciesName == _a.SpeciesName) return;
        if (Vector3.Distance(seal.transform.position, _target.transform.position) > 4f) return;
        if (_a.hungry < -0.5f && Random.value < 0.5f) return;   // saciado, a veces la deja ir
        seal.Hurt(9999f);
        _a.hungry = Mathf.Min(_a.hungry, -0.9f);
        Debug.Log($"[Hábito] «{name}» caza a «{seal.name}» al salir a respirar por su respiradero (still-hunting).");
        _until = Time.time;
    }

    void Finish()
    {
        if (_act.effect == HabitEffect.Carry && _target != null)
        {
            float got = _target.Take(_act.kind == AffordanceKind.Pebble ? 1f : 0.25f, _a);
            _target.Release(_a);
            if (got > 0f) { Pick(); _s = State.Carrying; return; }
        }
        Stop();
    }

    // Lo que carga, sobre la cabeza/lomo (guijarro, semilla, brizna, bola de estiércol), a escala de su cuerpo.
    void Pick()
    {
        _load = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _load.name = $"Carga_{_act.kind}";
        Object.Destroy(_load.GetComponent<Collider>());
        _load.transform.SetParent(transform, false);
        float s = 0.35f;
        _load.transform.localPosition = Vector3.up * 1.1f; _load.transform.localScale = Vector3.one * s;
        Renderer src = _target != null ? _target.GetComponentInChildren<Renderer>() : null;
        if (src != null) _load.GetComponent<Renderer>().sharedMaterial = src.sharedMaterial;
    }

    void Deliver()
    {
        if (_act.kind == AffordanceKind.Pebble)
        {
            // El NIDO crece de guijarros en casa (y es robable: otro pingüino lo ve como un montón de piedras).
            if (_nest == null)
            {
                var g = new GameObject($"Nido_guijarros_{name}");
                g.transform.position = _a.HomeOrigin + new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
                _nest = Affordance.Put(g, AffordanceKind.Pebble, 1, 0f, 0f);
                _nest.maxStock = 30f;
            }
            _nest.Add(1f);
            if (_load != null && _nest.transform.childCount < 14)
            {
                _load.transform.SetParent(_nest.transform, true);
                _load.transform.position = _nest.transform.position + new Vector3(Random.Range(-0.4f, 0.4f), 0.05f + _nest.transform.childCount * 0.02f, Random.Range(-0.4f, 0.4f));
                _load = null;
            }
        }
        else
            _a.hungry -= MealWeight * 0.6f;   // lo que lleva a casa se come allí (o alimenta a la colonia)
        if (_logged++ < 40 || _logged % 50 == 0) Debug.Log($"[Hábito] «{name}» deja en casa lo que traía ({_act.kind}).");
    }

    void Stop()
    {
        if (_target != null) _target.Release(_a);
        if (_load != null) Object.Destroy(_load);
        _target = null; _load = null; _s = State.None; Doing = "";
        _cool = Time.time + Random.Range(2f, 6f);
    }

    void OnDisable() { if (_s != State.None) Stop(); }
}
