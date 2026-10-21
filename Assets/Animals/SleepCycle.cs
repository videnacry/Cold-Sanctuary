using UnityEngine;

/// <summary>
/// SUEÑO DÍA/NOCHE (docs/ice-sanctuary-ecology.md §4): dispara la compuerta <see cref="Anima.asleep"/> según el RELOJ
/// del día (<see cref="TimeController.IsDay"/>, que corre con <see cref="Clock"/>). Mientras duerme, el ser **no elige
/// deseos, no busca comida y no corrige su medio** (compuertas ya existentes en `Volition`/`ActiveBehaveTick`/
/// `CorrectMedium` con `if (!asleep)`). Auto-añadido en `Animal.Init`.
///
/// Reglas:
/// - **Diurno** (por defecto) duerme de noche; **nocturno** al revés (`nocturnal`).
/// - Se **DESPIERTA ante una amenaza** (`aware`): el sueño NO anula la supervivencia — `SenseThreats` sigue corriendo
///   aunque duerma, y en cuanto percibe peligro, `asleep` cae y puede huir.
/// - Mamíferos marinos (foca/ballena/orca): su sueño es real pero **no se ahogan** — `Suffocate()` corre en `Restore()`
///   pase lo que pase, así que dormir en el agua es seguro (aprox. del sueño unihemisférico, sin modelar la respiración).
/// - Peces/krill (`Swarm`) NO usan esto (no son `Anima`): de noche **derivan más lento** aparte (Swarm.nightDriftFactor).
///
/// Balance-safe: si no hay `Clock`/reloj en escena, `IsDay` es siempre true → **nunca duerme** (conducta previa intacta).
/// Un hechizo-estado que fuerce el sueño (p.ej. `Torpor`, futuro) puede poner <see cref="Suspended"/> para tomar el mando.
/// </summary>
public class SleepCycle : MonoBehaviour
{
    [Tooltip("Nocturno: duerme de DÍA en vez de por la noche.")]
    public bool nocturnal = false;

    /// <summary>Especies NOCTURNAS (data; `Animal.Init` lo copia a `nocturnal`). Grillo: los machos cantan al anochecer y
    /// de noche; hormiga león: el adulto vuela de noche (la larva acecha a cualquier hora); araña (arquetipo araña lobo):
    /// caza sobre todo de noche. Abejas, mariquitas, saltamontes, mantis y peloteros diurnos (el pelotero rodador se
    /// orienta por el sol; los nocturnos usan la Vía Láctea, doi 10.1016/j.cub.2012.12.034). [A] salvo cita.</summary>
    public static bool IsNocturnal(string species) => species == "Cricket" || species == "Antlion" || species == "Spider";
    [Tooltip("Solo maduros (adulto/juvenil) duermen el ciclo; las crías siguen despiertas aquí (su ritmo lo llevan otros).")]
    public bool onlyMature = false;

    /// <summary>Un hechizo-estado (p.ej. Torpor) puede TOMAR el control del sueño: mientras true, este ciclo no toca `asleep`.</summary>
    [HideInInspector] public bool Suspended;

    Animal _animal;

    void Awake() => _animal = GetComponent<Animal>();

    void Update() { if (!Suspended && !GoingToRest()) Evaluate(); }

    // A DORMIR A SU SITIO: al caer su hora de descanso, antes de dormirse vuelve a su nido/hogar (HomeOrigin); si en un
    // rato no llega, se acomoda donde esté (un sitio cercano). Los marinos (MarineLife) y los insectos (la colmena) tienen
    // su propia forma de descansar.
    float _restTry = -1f, _nextGo;
    bool GoingToRest()
    {
        if (_animal == null || _animal.death || _animal.asleep || _animal.aware) { _restTry = -1f; return false; }
        TimeController tc = TimeController.timeController;
        bool night = tc != null && !tc.IsDay;
        bool restTime = nocturnal ? !night : night;
        if (!restTime || GetComponent<MarineLife>() != null || GetComponent<IceInsectLife>() != null) { _restTry = -1f; return false; }
        if (_restTry < 0f) _restTry = Time.time;
        Vector3 home = _animal.HomeOrigin; Vector3 d = home - transform.position; d.y = 0f;
        if (d.magnitude < 6f || Time.time - _restTry > 90f) return false;   // ya en su nido (o se acomoda aquí)
        if (Time.time >= _nextGo && _animal.Loco != null) { _nextGo = Time.time + 3f; _animal.Loco.GoTo(home); }
        return true;
    }

    /// <summary>Decide dormir/despertar por el reloj del día. Público y determinista (el test lo llama directo).</summary>
    public void Evaluate()
    {
        if (_animal == null || _animal.death) return;
        TimeController tc = TimeController.timeController;
        bool night = tc != null && !tc.IsDay;
        bool restTime = nocturnal ? !night : night;                       // diurno duerme de noche; nocturno de día
        if (onlyMature && _animal.lifeStage == LifeStage.child) restTime = false;
        _animal.asleep = restTime && !_animal.aware;                      // la amenaza (aware) ROMPE el sueño
    }
}
