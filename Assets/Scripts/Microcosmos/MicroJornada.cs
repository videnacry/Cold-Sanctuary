using UnityEngine;

/// <summary>
/// LA JORNADA DE UN INSECTO-HUMANO de una era civilizada (docs/microcosmos-civilizaciones.md §2; Berón 2026-11-01: «al igual
/// que en el santuario se han creado jornadas para los personajes, en el microcosmos de eras civilizadas se crearía jornadas
/// para los insectos-humanos»). Con el reloj del juego y el mismo horario del santuario (<see cref="WorkRoster"/>):
/// 8–11 trabajo · 11–12 comida · 12–14 trabajo · 14–16 práctica · después, casa. Trabaja en SU edificio con las funciones del
/// registro que le tocan por su papel (el cocinero cocina, el médico pasa visita), come en el comedor de la era, practica
/// en el dojo o la sala de yoga (si la hay) y descansa en casa. Va andando a su puesto (MicroMove); si en 60 s no llega,
/// aparece junto a él (ninguna espera sin tope). Se para mientras la historia le necesita quieto (<see cref="hold"/>).
/// (2026-11-02) Es la CAPA DE HORARIO sobre el ciclo de vida de una persona (<see cref="PersonLife"/>), como WorkdayDirector para
/// el personal del santuario: el cuerpo (<see cref="NeedsDriver"/>) pide dormir, el baño, comer o lavarse y lo atiende en su
/// casa (<see cref="Household"/>); mientras, la jornada no lo mueve. En el bloque de la comida come en el comedor de la era.
/// </summary>
public class MicroJornada : MonoBehaviour
{
    public AreaFunctionSite work, dine, practice, home;
    [Tooltip("Su papel (cocinero, camarero, medico, enfermera, maestro, alumno, mecanico, sabio, comerciante, soldado…).")]
    public string role = "";
    public float speed = 2.4f;
    public bool hold;
    [Tooltip("Un niño sigue a su madre (FolkCouple).")]
    public Transform follow;

    AreaFunctionSite _site; AreaFunctions.AreaFunction _fn; AreaFunctionStation _st;
    string _block; float _since; int _salt;
    WorldLabel _label; string _name;
    Anima _a; ActionQueue _q; BodyMetabolism _body;
    bool _resume; float _arrivedAt = -1f; int _ateDay = int.MinValue;

    /// <summary>Qué está haciendo ahora («Cocinando: …»), para el rótulo y las pruebas.</summary>
    public string Doing { get; private set; } = "";
    public AreaFunctionStation Station => _st;
    /// <summary>Su nombre en el rótulo (sin lo que está haciendo).</summary>
    public string DisplayName => _name ?? (TryGetComponent(out WorldLabel wl) ? wl.text : name);

    void Start()
    {
        _label = GetComponent<WorldLabel>(); _name = _label != null ? _label.text : name;
        _salt = Mathf.Abs(name.GetHashCode());
        _a = GetComponent<Anima>(); _q = GetComponent<ActionQueue>(); _body = GetComponent<BodyMetabolism>();
    }

    /// <summary>¿En su turno (trabajo o práctica, no siguiendo a su madre)? Entonces el hambre que no aprieta espera a la comida.</summary>
    public bool OnShift { get { string b = BlockAt(Hour); return follow == null && work != null && (b == "trabajo" || b == "practica"); } }
    /// <summary>¿Es la hora de estar en casa (después de la práctica, la noche)?</summary>
    public bool AtHomeBlock => BlockAt(Hour) == "casa";

    /// <summary>La hora del día (el reloj del juego; sin él, un día cada 6 min reales).</summary>
    public static float Hour => TimeController.timeController != null ? TimeController.timeController.Hour : (Time.time / 15f) % 24f;

    /// <summary>El bloque del horario a la hora <paramref name="h"/>: trabajo, comida, práctica o casa.</summary>
    public static string BlockAt(float h)
    {
        float s = WorkRoster.StartHour;
        if (h >= s && h < s + WorkRoster.FirstBlock) return "trabajo";
        if (h >= s + 3f && h < s + 4f) return "comida";
        if (h >= s + 4f && h < s + 6f) return "trabajo";
        if (h >= s + 6f && h < s + 8f) return "practica";
        return "casa";
    }

    /// <summary>Cuántas veces alguien no llegó andando a su puesto en 60 s y se le puso allí (pruebas, registro).</summary>
    public static int Fallbacks;

    /// <summary>EL SITIO DE TRABAJO de un puesto: delante de su cara frontal (−z del collider), a <see cref="MicroBuildRules.WorkSpot"/>
    /// m, como lo coloca CivicBuilder.Worker. (2026-11-01) Antes era 1,2 m del CENTRO hacia donde estaba: en una encimera de cocina
    /// de 2,4 m ese punto caía dentro del mueble o pegado a su costado y no se llegaba (a los 60 s, teletransporte).</summary>
    public static Vector3 WorkSpotOf(AreaFunctionStation st, Vector3 from)
    {
        BoxCollider bc = st.GetComponent<BoxCollider>();
        if (bc == null) return st.transform.position + (from - st.transform.position).normalized * 1.2f;
        float front = bc.center.z - bc.size.z * 0.5f;
        Vector3 p = st.transform.TransformPoint(new Vector3(bc.center.x, 0f, front - MicroBuildRules.WorkSpot / Mathf.Max(1e-3f, Mathf.Abs(st.transform.lossyScale.z))));
        p.y = st.transform.position.y;
        return p;
    }

    void Update()
    {
        if (hold) return;
        if (_q == null) { _a = GetComponent<Anima>(); _q = GetComponent<ActionQueue>(); }
        if (_a != null && (_a.death || _a.asleep)) return;
        if (NeedsDriver.Attending(_q)) { _resume = true; return; }   // el cuerpo atiende una necesidad (en casa): la jornada espera
        if (LeisureDirector.Drives(gameObject)) { _resume = true; return; }   // sus horas libres: lo lleva el HORARIO LIBRE (docs/horario-libre.md)
        if (follow != null) { MicroMove.Step(transform, follow.position - follow.forward * 1.2f, speed * 1.2f, 1.5f); return; }
        string block = BlockAt(Hour);
        if (block != _block || _st == null || _resume) { _resume = false; Choose(block); }
        if (_st == null) return;
        // (2026-11-04) A LA HORA DE COMER, primero al MOSTRADOR por la bandeja y, al acabar, de vuelta a él a devolverla (MealRitual).
        AreaFunctionStation target = _block == "comida" && _counter != null && (_mealStage == 1 || _mealStage == 4) ? _counter : _st;
        Vector3 to = WorkSpotOf(target, transform.position);
        bool there = MicroMove.Step(transform, to, speed, 1.6f);
        if (!there && Time.time - _since > 60f)   // sin quedarse dando vueltas para siempre (y avisa: es un fallo de la planta)
        {
            transform.position = to; there = true; Fallbacks++;
            if (Fallbacks <= 20) Debug.LogWarning($"[Jornada micro] «{name}» no llega andando en 60 s a «{target.name}» de {(_site != null ? _site.title : "?")}: se le pone allí.");
        }
        if (!there) { _meal?.Carry(gameObject); Say(_block == "casa" ? "A casa" : _block == "comida" && _mealStage == 4 ? "Devuelve la bandeja" : "Va a " + (_site != null ? _site.title : "su puesto")); return; }
        if (_arrivedAt < 0f) _arrivedAt = Time.time;
        Vector3 look = target.transform.position - transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f && _mealStage != 3) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 4f);
        if (_block == "comida") { Lunch(); return; }
        if (_fn != null)
        {
            // (2026-11-04) HACERLO DE VERDAD (docs/actividades-y-uniformes.md §3): la actuación de su función —herramienta de su era en la
            // mano, el gesto y su efecto— y, en el trabajo, el delantal de su oficio (antes solo un gesto de brazos sin nada en las manos).
            ActivityPerformer.Keep(gameObject, Performances.ForMission(_fn.id), _st.transform.position);
            if (_block == "trabajo") UniformWearer.Want(gameObject, Uniforms.ForFunction(_fn));
            Say(AreaFunctions.Verb(_fn.kind));
        }
    }

    void Choose(string block)
    {
        if (_st != null && _st.occupant == this) _st.occupant = null;
        _block = block; _since = Time.time; _fn = null; _st = null; _arrivedAt = -1f;
        _site = block == "trabajo" ? work : block == "comida" ? (dine ?? work) : block == "practica" ? (practice ?? work) : (home ?? work);
        if (_site == null) return;
        string r = block == "trabajo" ? role : "todos";
        _fn = _site.Pick(r, _salt + (block == "trabajo" ? 0 : 1));
        _st = _fn != null ? _site.StationFor(_fn, this) : null;
        if (_st == null) { AreaFunctionStation[] all = _site.Stations; if (all.Length > 0) _st = all[_salt % all.Length]; }
        if (_st != null) { if (_st.occupant == null) _st.occupant = this; if (_fn != null) _st.uses++; }
        Doing = _fn != null ? $"{AreaFunctions.Verb(_fn.kind)}: {_fn.title}" : "";
        // Fuera del trabajo, con su ropa; la comida a medias (si la historia o una necesidad lo sacó), recogida.
        if (block != "trabajo") UniformWearer.Want(gameObject, Uniforms.Kind.None);
        _meal?.Abort(); _meal = null; _counter = null; _mealStage = 0;
        if (block == "comida" && _site != null)
        {
            foreach (AreaFunctionStation s in _site.Stations)
                if (s != null && s != _st && System.Array.IndexOf(CounterKeys, s.station) >= 0) { _counter = s; break; }
            _mealStage = Day == _ateDay ? 5 : _counter != null ? 1 : 2;
        }
    }

    // ── LA COMIDA DEL MEDIODÍA en el comedor de la era, CON SU ORQUESTA (MealRitual; docs/actividades-y-uniformes.md §2) ──
    // 1 al mostrador por la bandeja · 2 a su mesa (sin mostrador, le sirven en ella) · 3 comer a bocados (~20 min de juego) ·
    // 4 devolver la bandeja al mostrador · 5 ya comió. El mismo efecto que comer en casa (HomeNeedSpell) o en el restaurante (DineSpell),
    // pero repartido por bocados.
    static readonly string[] CounterKeys = { "barra", "mostrador", "granero", "fogon", "puesto" };
    AreaFunctionStation _counter;
    int _mealStage; float _mealUntil;
    MealRitual _meal;
    /// <summary>La comida de hoy (pruebas): null si aún no recogió su bandeja.</summary>
    public MealRitual Meal => _meal;
    /// <summary>En qué paso de la comida va (0 nada · 1 al mostrador · 2 a la mesa · 3 comiendo · 4 a devolver · 5 comió).</summary>
    public int MealStage => _mealStage;
    static int Day => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    void Lunch()
    {
        if (_body == null) _body = GetComponent<BodyMetabolism>();
        float minute = TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 1f;
        switch (_mealStage)
        {
            case 1:   // en el mostrador: la bandeja
                _meal = new MealRitual(EraDish(Eras.Now)); _meal.TakeTray(gameObject, true, 0.75f);
                _mealStage = 2; _since = Time.time;
                Say("Recoge su bandeja");
                return;
            case 2:   // en su mesa: el plato a la mesa y los cubiertos
                if (_meal == null) { _meal = new MealRitual(EraDish(Eras.Now)); _meal.TakeTray(gameObject, false, 0.75f); }   // (sin mostrador: le sirven en la mesa)
                _meal.SitDown(gameObject, _st.transform, ActivityKit.IsHuman(gameObject));
                _mealUntil = Time.time + 20f * minute; _mealStage = 3;
                return;
            case 3:
                Say("Comiendo");
                if (!_meal.Eat(gameObject, _mealUntil)) return;
                _ateDay = Day;
                if (_body != null) _body.toilet = Mathf.Clamp01(_body.toilet + 0.25f);
                PersonLife.Report(gameObject, "comer");
                Debug.Log($"[Jornada micro] «{name}» come en {(_site != null ? _site.title : "el comedor")} ({_meal.dish}, {_meal.bitesTaken} bocados).");
                _meal.PickUp(gameObject); _since = Time.time;
                if (_counter != null) { _mealStage = 4; return; }
                _meal.Return(gameObject); _mealStage = 5;   // sin mostrador: la deja recogida en la mesa
                return;
            case 4:   // en el mostrador: la devuelve
                _meal.Return(gameObject); _mealStage = 5;
                return;
            default:
                Say("Sobremesa");
                return;
        }
    }

    /// <summary>La comida de cada época (para el plato y sus nutrientes).</summary>
    public static string EraDish(int year) => year < -3000 ? "gachas de cebada" : year < 500 ? "pan, aceitunas y queso" : year < 1800 ? "potaje de legumbres" : year < 1950 ? "estofado con pan" : "arroz con verduras";

    void Say(string what) { if (_label != null) _label.text = $"{_name} · {what}"; }

    void OnDisable() { if (_st != null && _st.occupant == this) _st.occupant = null; }

    /// <summary>Para las pruebas: elige ya lo del bloque <paramref name="block"/>.</summary>
    public void ChooseNow(string block) { if (_label == null) Start(); Choose(block); }
}
