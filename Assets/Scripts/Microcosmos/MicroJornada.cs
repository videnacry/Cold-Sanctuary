using UnityEngine;

/// <summary>
/// LA JORNADA DE UN INSECTO-HUMANO de una era civilizada (docs/microcosmos-civilizaciones.md §2; Berón 2026-11-01: «al igual
/// que en el santuario se han creado jornadas para los personajes, en el microcosmos de eras civilizadas se crearía jornadas
/// para los insectos-humanos»). Con el reloj del juego y el mismo horario del santuario (<see cref="WorkRoster"/>):
/// 8–11 trabajo · 11–12 comida · 12–14 trabajo · 14–16 práctica · después, casa. Trabaja en SU edificio con las funciones del
/// registro que le tocan por su papel (el cocinero cocina, el médico pasa visita), come en el comedor de la era, practica
/// en el dojo o la sala de yoga (si la hay) y descansa en casa. Va andando a su puesto (MicroMove); si en 60 s no llega,
/// aparece junto a él (ninguna espera sin tope). Se para mientras la historia le necesita quieto (<see cref="hold"/>).
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

    /// <summary>Qué está haciendo ahora («Cocinando: …»), para el rótulo y las pruebas.</summary>
    public string Doing { get; private set; } = "";
    public AreaFunctionStation Station => _st;

    void Start()
    {
        _label = GetComponent<WorldLabel>(); _name = _label != null ? _label.text : name;
        _salt = Mathf.Abs(name.GetHashCode());
    }

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

    void Update()
    {
        if (hold) return;
        if (follow != null) { MicroMove.Step(transform, follow.position - follow.forward * 1.2f, speed * 1.2f, 1.5f); return; }
        string block = BlockAt(Hour);
        if (block != _block || _st == null) Choose(block);
        if (_st == null) return;
        Vector3 to = _st.transform.position + (transform.position - _st.transform.position).normalized * 1.2f;
        bool there = MicroMove.Step(transform, to, speed, 1.6f);
        if (!there && Time.time - _since > 60f) { transform.position = to; there = true; }   // sin quedarse dando vueltas para siempre
        if (!there) { Say(_block == "casa" ? "A casa" : "Va a " + (_site != null ? _site.title : "su puesto")); return; }
        Vector3 look = _st.transform.position - transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 4f);
        if (_fn != null)
        {
            CubeInsect ci = GetComponent<CubeInsect>();
            if (ci != null) AreaFunctionStation.Gesture(ci, _fn.kind);
            Say(AreaFunctions.Verb(_fn.kind));
        }
    }

    void Choose(string block)
    {
        if (_st != null && _st.occupant == this) _st.occupant = null;
        _block = block; _since = Time.time; _fn = null; _st = null;
        _site = block == "trabajo" ? work : block == "comida" ? (dine ?? work) : block == "practica" ? (practice ?? work) : (home ?? work);
        if (_site == null) return;
        string r = block == "trabajo" ? role : "todos";
        _fn = _site.Pick(r, _salt + (block == "trabajo" ? 0 : 1));
        _st = _fn != null ? _site.StationFor(_fn, this) : null;
        if (_st == null) { AreaFunctionStation[] all = _site.Stations; if (all.Length > 0) _st = all[_salt % all.Length]; }
        if (_st != null) { if (_st.occupant == null) _st.occupant = this; if (_fn != null) _st.uses++; }
        Doing = _fn != null ? $"{AreaFunctions.Verb(_fn.kind)}: {_fn.title}" : "";
    }

    void Say(string what) { if (_label != null) _label.text = $"{_name} · {what}"; }

    void OnDisable() { if (_st != null && _st.occupant == this) _st.occupant = null; }

    /// <summary>Para las pruebas: elige ya lo del bloque <paramref name="block"/>.</summary>
    public void ChooseNow(string block) { if (_label == null) Start(); Choose(block); }
}
