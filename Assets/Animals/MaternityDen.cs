using UnityEngine;

/// <summary>
/// GUARIDA DE MATERNIDAD (docs/ciclo-de-vida.md): la hembra preñada de una especie que pare en guarida
/// (<see cref="LifeHistory.den"/>: oso polar, malamute) entra en ella <see cref="LifeHistory.denBeforeMonths"/> antes del
/// parto, pare DENTRO (<see cref="Reproduction"/> la deja parir a su lado), amamanta allí a las crías y sale con ellas
/// <see cref="LifeHistory.denAfterMonths"/> después. La osa AYUNA todo ese tiempo (vive de su grasa); la perra sale un
/// momento a comer si tiene mucha hambre y vuelve. Lo que hace lo pide el deseo «den» de <see cref="DesireCatalog"/>; aquí
/// solo se lleva el calendario y el sitio (una <see cref="Den"/> cercana o, si no hay, la que cava donde esté).
/// </summary>
[RequireComponent(typeof(Animal))]
public class MaternityDen : MonoBehaviour
{
    public static event System.Action<Animal> Entered, Emerged;

    Animal _a;
    Reproduction _r;
    LifeHistory _life;
    public Vector3 site;
    public bool hasSite, entered, emerged;
    public Den den;
    float _bornAt = -1f, _next;
    /// <summary>¿Quiere estar en la guarida ahora (por parir o con crías recién nacidas)?</summary>
    public bool Wants { get; private set; }
    /// <summary>¿Está dentro de su guarida?</summary>
    public bool InDen => Wants && hasSite && Vector3.Distance(transform.position, site) <= 1.6f;
    public LifeHistory Life => _life;

    void Start()
    {
        _a = GetComponent<Animal>();
        _r = GetComponent<Reproduction>();
        _life = LifeHistory.Of(_a.LifeSpecies);
        Reproduction.Born += OnBorn;
    }

    void OnDestroy() { Reproduction.Born -= OnBorn; if (den != null && den.occupant == _a) den.occupant = null; }

    void OnBorn(Animal mother, Animal baby) { if (mother == _a) _bornAt = Time.time; }

    void Update()
    {
        if (_a == null || _life == null || !_life.den || Time.time < _next) return;
        _next = Time.time + 0.25f;
        if (_a.death || _a.sex != Sex.female || _a.lifeStage != LifeStage.adult || DesireCatalog.Confined(_a)) { Wants = false; return; }
        TimeController tc = TimeController.timeController;
        bool due = _r != null && _r.Pregnant && _r.BirthAt - Time.time <= (tc != null ? tc.SecondsForMonths(_life.denBeforeMonths) : 30f);
        bool nursing = _bornAt >= 0f && Time.time - _bornAt < (tc != null ? tc.SecondsForMonths(_life.denAfterMonths) : 60f);
        Wants = due || nursing;
        if (Wants && !hasSite) PickSite();
        if (Wants && !entered && InDen)
        {
            entered = true;
            Debug.Log($"[Guarida] «{name}» entra en {(den != null ? "su guarida" : "la guarida que se ha hecho")} para parir.");
            Entered?.Invoke(_a);
        }
        if (!Wants && entered && !emerged && _bornAt >= 0f)
        {
            emerged = true;
            Debug.Log($"[Guarida] «{name}» sale de la guarida con sus crías.");
            Emerged?.Invoke(_a);
        }
        if (!Wants && emerged && _r != null && _r.Pregnant)   // otra temporada: todo de nuevo
        {
            entered = emerged = hasSite = false; _bornAt = -1f;
            if (den != null && den.occupant == _a) den.occupant = null;
            den = null;
        }
    }

    void PickSite()
    {
        den = Den.Nearest(_a, 80f);
        if (den != null) { den.occupant = _a; site = den.Inside; }
        else
        {
            site = transform.position;   // la cava (osa: en un ventisquero) o hace el nido donde está
            Debug.Log($"[Guarida] «{name}» no tiene guarida cerca: se la hace en ({site.x:0},{site.z:0}).");
        }
        hasSite = true;
    }
}
