using UnityEngine;

/// <summary>
/// EL MAESTRO QUE ENSEÑA CON EL CUERPO (docs/microcosmos-bruce-lee.md): hace en bucle una forma de <see cref="JeetKuneDo"/>
/// (la guardia, los golpes en cadena, el muñeco, el juego de pies, el golpe de una pulgada…). Si está en una historia por
/// pasos (<see cref="StoryMission"/>), cada paso elige su forma (<see cref="formByStep"/>) y su sitio (<see cref="spotByStep"/>):
/// el maestro va a la estación de ese paso (Hong Kong, la bahía, Seattle, Long Beach, Los Ángeles…) y mira al jugador.
/// Necesita un cuerpo de cubos humano (<see cref="CubeHumanoid"/>, con <see cref="CreatureRig"/>) y un <see cref="Anima"/>.
/// </summary>
public class MartialForms : MonoBehaviour
{
    [Tooltip("La forma sin historia (o si el paso no tiene una): wingchun, cadena, muneco, guardia, pasos, pulgada, notas, agua.")]
    public string form = "guardia";
    [Tooltip("La forma de cada paso de la StoryMission (vacío = la de siempre).")]
    public string[] formByStep = new string[0];
    [Tooltip("Dónde está el maestro en cada paso (null = donde esté).")]
    public Transform[] spotByStep = new Transform[0];
    [Tooltip("Cuánto se aparta del punto de su estación (m), hacia el jugador, para no estar dentro del mueble.")]
    public float standOff = 1.6f;

    Anima _anima;
    StoryMission _mission;
    string _current;
    (System.Func<AsanaPoseSpell> pose, float seconds)[] _frames;
    int _frame;
    AsanaPoseSpell _spell;
    int _step = -1;

    /// <summary>La forma que está haciendo ahora.</summary>
    public string Current => _current;

    void Start()
    {
        _anima = GetComponent<Anima>();
        _mission = GetComponentInParent<StoryMission>();
        if (_mission == null) _mission = StoryMission.Instance;
    }

    void Update()
    {
        if (_anima == null) return;
        int step = _mission != null ? _mission.Step : -1;
        if (step != _step) { _step = step; OnStep(step); }
        Play(_step >= 0 && _step < formByStep.Length && !string.IsNullOrEmpty(formByStep[_step]) ? formByStep[_step] : form);
        if (_frames == null || _frames.Length == 0) return;
        if (_spell == null) { _spell = _frames[_frame].pose(); _spell.duration = _frames[_frame].seconds; _spell.reachFraction = Mathf.Min(0.6f, 0.25f / Mathf.Max(0.05f, _spell.duration) + 0.2f); }
        if (_spell.Tick(_anima)) { _frame = (_frame + 1) % _frames.Length; _spell = null; }
    }

    /// <summary>Cambia de forma (si ya es esa, no hace nada).</summary>
    public void Play(string key)
    {
        if (key == _current) return;
        _current = key; _frames = JeetKuneDo.Form(key); _frame = 0; _spell = null;
    }

    // Al cambiar de paso: a la estación de ese paso, mirando al jugador (o al centro de la estación).
    void OnStep(int step)
    {
        if (step < 0 || step >= spotByStep.Length || spotByStep[step] == null) return;
        Vector3 spot = spotByStep[step].position;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        Vector3 toward = p != null ? p.transform.position - spot : -spotByStep[step].forward;
        toward.y = 0f;
        if (toward.sqrMagnitude < 0.01f) toward = Vector3.back;
        transform.position = spot + toward.normalized * standOff;
        transform.rotation = Quaternion.LookRotation(toward.normalized, Vector3.up);   // de cara al jugador: le enseña
    }

    /// <summary>Para las pruebas: el sitio al que va en el paso <paramref name="step"/> (o null).</summary>
    public Transform SpotOf(int step) => step >= 0 && step < spotByStep.Length ? spotByStep[step] : null;
}
