using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CADA PASO DE UN RETO SE VE EN SU SITIO (CodeShip reglas.md §4.2: «cada paso de un reto tiene animación en su entorno real»; Berón
/// 2026-11-18). Al acertar un reto de una misión de puesto (<see cref="StationMissions"/>), la hoja se recoge unos segundos y el cuerpo hace
/// ese paso ante el mueble, con las animaciones que ya tiene el juego:
///  · los gestos de las funciones de área (<see cref="PlayerTasks.Gesture"/>: cocinar, fabricar en el banco, teclear en la consola, curar,
///    cultivar, limpiar, explicar, entrenar…), en el cuerpo del jugador, y el de un insecto-humano que trabaje allí
///    (<see cref="AreaFunctionStation.Gesture"/>);
///  · el yoga, postura a postura: el Saludo al Sol (<see cref="SunSalutation.Sequence"/>) y el upa-yoga de cuello (<see cref="UpaYoga.NeckSequence"/>);
///  · el paseo del perro: unos pasos en círculo junto a él (<see cref="PathMover"/>) y el perro detrás;
///  · dormir: al último paso, tumbado (<see cref="DailyPoses.Lie"/>).
/// Todo con tope: <see cref="StepSeconds"/> por paso; al acabar, de pie.
/// </summary>
public class StepAnimator : MonoBehaviour
{
    public const float StepSeconds = 2.5f;

    /// <summary>Qué hace el cuerpo en un paso (lógica pura: <see cref="MotionFor"/>, probada en ChoreLibraryTest).</summary>
    public struct Motion
    {
        public enum Type { Gesture, Yoga, Walk, Lie }
        public Type type;
        public AreaFunctions.Kind gesture;
        /// <summary>Yoga: «saludo-al-sol» o «upa-cuello», y la postura.</summary>
        public string sequence;
        public int posture;
        public override string ToString() => type == Type.Gesture ? $"gesto {gesture}" : type == Type.Yoga ? $"yoga {sequence} {posture + 1}" : type.ToString();
    }

    static readonly string[] Known = { "cocinar", "mecanizar", "composite", "reparar-maquina", "avionica", "atender-paciente", "medico", "regar", "curar-planta", "cosechar", "alimentar-perro", "programar-animal", "jugar-perro", "ejercicio", "asearse", "plantar-idea", "dormir", "yoga", "soldar-fsw", "ensayo-materiales", "analizar-datos", "sala-limpia", "seguridad-alimentaria", "recepcion", "control-acceso", "incendios", "seguridad-vial", "aeronautica" };
    /// <summary>¿Tiene esta clase de misión su animación propia? (las que no, el gesto de banco por defecto)</summary>
    public static bool Handles(string kind) => System.Array.IndexOf(Known, kind) >= 0;

    /// <summary>El movimiento del paso <paramref name="step"/> (de <paramref name="steps"/>) de una misión de la clase <paramref name="kind"/>.</summary>
    public static Motion MotionFor(string kind, string templateId, int step, int steps)
    {
        Motion G(AreaFunctions.Kind k) => new Motion { type = Motion.Type.Gesture, gesture = k };
        switch (kind)
        {
            case "cocinar": return G(AreaFunctions.Kind.Cook);
            case "mecanizar": case "composite": return G(AreaFunctions.Kind.Make);
            case "reparar-maquina": return G(AreaFunctions.Kind.Build);
            case "avionica": return G(AreaFunctions.Kind.Operate);
            case "atender-paciente": case "medico": return G(AreaFunctions.Kind.Heal);
            case "regar": case "curar-planta": case "cosechar": return G(AreaFunctions.Kind.Tend);
            case "alimentar-perro": case "programar-animal": return G(AreaFunctions.Kind.Care);
            case "jugar-perro": return new Motion { type = Motion.Type.Walk };
            case "ejercicio": return G(AreaFunctions.Kind.Practice);
            case "asearse": return G(AreaFunctions.Kind.Clean);
            case "plantar-idea": case "aeronautica": return G(AreaFunctions.Kind.Teach);
            case "soldar-fsw": return G(AreaFunctions.Kind.Make);
            case "ensayo-materiales": case "analizar-datos": return G(AreaFunctions.Kind.Operate);
            case "sala-limpia": case "seguridad-alimentaria": case "incendios": return G(AreaFunctions.Kind.Clean);   // el extintor: barrer de lado a lado
            case "recepcion": return G(AreaFunctions.Kind.Serve);
            case "control-acceso": case "seguridad-vial": return G(AreaFunctions.Kind.Guard);   // mirar a un lado y a otro
            case "dormir": return step >= steps - 1 ? new Motion { type = Motion.Type.Lie } : G(AreaFunctions.Kind.Clean);
            case "yoga": return new Motion { type = Motion.Type.Yoga, sequence = templateId != null && templateId.Contains("saludo") ? "saludo-al-sol" : "upa-cuello", posture = step };
            default: return G(AreaFunctions.Kind.Make);
        }
    }

    // ── Reproducirlo ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
    static StepAnimator _inst;
    public static StepAnimator Ensure()
    {
        if (_inst != null) return _inst;
        var go = new GameObject("StepAnimator_AUTO");
        DontDestroyOnLoad(go);
        return _inst = go.AddComponent<StepAnimator>();
    }

    /// <summary>¿Se está viendo un paso ahora? (la hoja se recoge mientras)</summary>
    public static bool Playing => _inst != null && Time.time < _inst._until;
    /// <summary>Lo que se ve ahora («Paso 3 de 7: cocina hasta 71 °C»), para la barra de la hoja recogida.</summary>
    public static string Caption => _inst != null ? _inst._caption : "";
    /// <summary>El último movimiento reproducido (pruebas).</summary>
    public static Motion Last { get; private set; }

    Motion _m;
    float _until, _nextFrame;
    bool _frameB;
    string _caption = "";
    Transform _target, _player, _walker;
    Anima _anima;
    CubeInsect _folk;
    Vector3 _walkCenter; float _walkAngle;

    /// <summary>Reproduce el paso <paramref name="step"/> de la misión <paramref name="m"/> (lo llama la hoja al acertar).</summary>
    public static void Play(StationMission m, int step, int steps, string caption)
    {
        if (m == null || m.template == null) return;
        StepAnimator a = Ensure();
        a._m = MotionFor(m.template.kind, m.template.id, step, steps);
        Last = a._m;
        a._caption = $"Paso {step + 1} de {steps}: {caption}";
        a._target = m.target;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        a._player = p != null ? p.transform : null;
        a._anima = p != null ? p.GetComponent<Anima>() : null;
        a._folk = FolkAt(m.target, a._player);
        a._until = Time.time + StepSeconds;
        a._nextFrame = 0f;
        if (a._player != null && a._target != null)
        {
            Vector3 d = a._target.position - a._player.position; d.y = 0f;
            if (d.sqrMagnitude > 0.01f) a._player.rotation = Quaternion.LookRotation(d);   // ante la máquina
        }
        if (a._m.type == Motion.Type.Walk && a._target != null)
        {
            a._walkCenter = a._target.position; a._walkAngle += 90f;
            a._walker = a._target;   // el perro (o su marcador) va detrás
        }
        else a._walker = null;
        if (a._m.type == Motion.Type.Yoga) a.PoseYoga();
        if (a._m.type == Motion.Type.Lie && a._anima != null) DailyPoses.Lie().ApplyFinal(a._anima);
        Debug.Log($"[Misiones] {m.template.title} · {a._caption} ({a._m}).");
    }

    /// <summary>El insecto-humano que trabaja en ese mueble (o el más cercano a él, a menos de 3 m), si lo hay.</summary>
    static CubeInsect FolkAt(Transform target, Transform player)
    {
        if (target == null) return null;
        CubeInsect best = null; float bd = 9f;
        foreach (MicroJornada j in FindObjectsByType<MicroJornada>(FindObjectsSortMode.None))
        {
            if (j == null || (player != null && j.transform == player)) continue;
            if (j.Station != null && j.Station.transform == target) return j.GetComponent<CubeInsect>();
            float d = (j.transform.position - target.position).sqrMagnitude;
            if (d < bd && j.TryGetComponent(out CubeInsect ci)) { bd = d; best = ci; }
        }
        return best;
    }

    void PoseYoga()
    {
        if (_anima == null) return;
        List<QueuedSpell> seq = _m.sequence == "saludo-al-sol" ? SunSalutation.Sequence(1f) : UpaYoga.NeckSequence(1f);
        AsanaPoseSpell pose = null;
        if (_m.sequence == "saludo-al-sol") { int i = Mathf.Clamp(_m.posture * 3, 0, seq.Count - 1); pose = seq[i] as AsanaPoseSpell; }
        else
        {
            string[] keys = { "barbilla al pecho", "cabeza atrás", "cara a la derecha", "oreja al hombro derecho", "rueda hacia atrás", "hombros arriba y atrás" };
            string key = keys[Mathf.Clamp(_m.posture, 0, keys.Length - 1)];
            foreach (QueuedSpell s in seq) if (s is AsanaPoseSpell a && a.asanaName.Contains(key)) { pose = a; break; }
        }
        if (pose != null) pose.ApplyFinal(_anima);
    }

    void Update()
    {
        if (_until <= 0f) return;
        if (Time.time >= _until)
        {
            _until = 0f;
            if (_anima != null && _m.type != Motion.Type.Lie) DailyPoses.Stand().ApplyFinal(_anima);   // al acabar, de pie
            return;
        }
        switch (_m.type)
        {
            case Motion.Type.Gesture:
                if (_anima != null && Time.time >= _nextFrame) { _nextFrame = Time.time + 0.6f; _frameB = !_frameB; PlayerTasks.Gesture(_m.gesture, _frameB).ApplyFinal(_anima); }
                break;
            case Motion.Type.Walk:
                if (_player != null)
                {
                    float a = (_walkAngle + (1f - (_until - Time.time) / StepSeconds) * 90f) * Mathf.Deg2Rad;
                    Vector3 to = _walkCenter + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 2.5f;
                    PathMover.Of(_player).StepToward(to, 1.4f);
                    if (_walker != null && _walker != _player)
                    {
                        Vector3 behind = _player.position - _player.forward * 1.2f; behind.y = _walker.position.y;
                        _walker.position = Vector3.Lerp(_walker.position, behind, Time.deltaTime * 3f);
                    }
                }
                break;
        }
        if (_folk != null && _m.type == Motion.Type.Gesture) AreaFunctionStation.Gesture(_folk, _m.gesture);
    }
}
