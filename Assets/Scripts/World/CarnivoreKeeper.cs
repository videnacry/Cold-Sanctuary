using UnityEngine;

/// <summary>
/// GUARDIA DEL SANTUARIO: patrulla los puestos de vigía y vigila a los CARNÍVOROS. Los animales se comportan según su
/// naturaleza (un oso hambriento CAZA); el trabajo del guardia es evitarlo: cuando un carnívoro de tierra empieza a tener
/// hambre, el guardia va al COMEDERO de los depredadores, recoge una ración de carne cultivada y se la lleva, dejándola
/// delante de él. Saciado, el carnívoro no caza. Si el comedero está vacío, sigue patrullando y vuelve a mirar.
/// </summary>
public class CarnivoreKeeper : MonoBehaviour
{
    public float speed = 3.2f;
    public float checkEvery = 4f;

    enum Duty { Patrol, ToFeeder, ToAnimal }
    Duty _duty = Duty.Patrol;
    Animal _target;
    FoodItem _ration;
    Transform _feeder;
    Transform[] _posts;
    int _post;
    float _nextCheck;
    public int fed { get; private set; }
    readonly System.Collections.Generic.Dictionary<Animal, float> _lastFed = new System.Collections.Generic.Dictionary<Animal, float>();
    [Tooltip("Segundos antes de volver a llevarle ración al mismo animal (así reparte entre todos los carnívoros).")]
    public float refeedAfter = 90f;
    public string Doing => _duty == Duty.Patrol ? "patrulla" : _duty == Duty.ToFeeder ? $"va al comedero a por carne para «{(_target != null ? _target.name : "?")}»" : $"lleva carne a «{(_target != null ? _target.name : "?")}»";

    void Start()
    {
        GameObject f = GameObject.Find("Comedero_depredadores");
        _feeder = f != null ? f.transform : null;
        var posts = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name.StartsWith("PuestoVigia_")) posts.Add(t);
        posts.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        _posts = posts.ToArray();
        if (TryGetComponent(out IdleBehavior idle)) idle.enabled = false;   // su trabajo es patrullar, no merodear
    }

    void Update()
    {
        if (Time.time >= _nextCheck && _duty == Duty.Patrol) { _nextCheck = Time.time + checkEvery; _target = HungryCarnivore(); if (_target != null) _duty = Duty.ToFeeder; }
        switch (_duty)
        {
            case Duty.Patrol:
                if (_posts == null || _posts.Length == 0) return;
                if (PathMover.Of(this).StepToward(_posts[_post].position, speed * 0.6f) < 1.5f) _post = (_post + 1) % _posts.Length;
                break;
            case Duty.ToFeeder:
                if (_target == null || _target.death || _feeder == null) { _duty = Duty.Patrol; return; }
                if (PathMover.Of(this).StepToward(_feeder.position, speed) < 1.6f)
                {
                    _ration = _feeder.GetComponentInChildren<FoodItem>();
                    if (_ration == null) { Debug.Log("[Guardia] El comedero está vacío: sigue la patrulla (el laboratorio prepara otra tanda)."); _duty = Duty.Patrol; _nextCheck = Time.time + 20f; return; }
                    _ration.transform.SetParent(transform, false);
                    _ration.transform.localPosition = new Vector3(0.35f, 1f, 0.3f);
                    _duty = Duty.ToAnimal;
                }
                break;
            case Duty.ToAnimal:
                if (_target == null || _target.death || _ration == null) { DropRation(transform.position + transform.forward); _duty = Duty.Patrol; return; }
                if (PathMover.Of(this).StepToward(_target.transform.position, speed) < 3f)
                {
                    Vector3 front = _target.transform.position + (transform.position - _target.transform.position).normalized * 1.2f;
                    DropRation(front);
                    fed++; _lastFed[_target] = Time.time;
                    Debug.Log($"[Guardia] «{name}» da una ración de carne cultivada a «{_target.name}» para que no cace.");
                    _target = null; _duty = Duty.Patrol;
                }
                break;
        }
    }

    void DropRation(Vector3 at)
    {
        if (_ration == null) return;
        _ration.transform.SetParent(null, true);
        if (Physics.Raycast(at + Vector3.up * 3f, Vector3.down, out RaycastHit h, 10f, ~0, QueryTriggerInteraction.Ignore)) at = h.point + Vector3.up * 0.08f;
        _ration.transform.position = at;
        _ration = null;
    }

    // El carnívoro de tierra (caza presas, no es cría) que tenga más hambre; los marinos (orcas) pescan en la laguna.
    Animal HungryCarnivore()
    {
        Animal best = null; float worst = -0.05f;
        foreach (Animal a in FindObjectsByType<Animal>(FindObjectsSortMode.None))
        {
            if (a == null || a.death || a.lifeStage == LifeStage.child || a.Forage == null || !a.Forage.eatsPrey) continue;
            if (a.GetComponent<MarineLife>() != null || a.GetComponent<IceInsectLife>() != null) continue;
            if (Mathf.Abs(a.transform.position.x) > 250f || Mathf.Abs(a.transform.position.z) > 250f) continue;   // escenarios de prueba
            if (_lastFed.TryGetValue(a, out float t) && Time.time - t < refeedAfter) continue;   // ya comió hace poco: turno de otro
            if (a.hungry > worst) { worst = a.hungry; best = a; }
        }
        return best;
    }
}
