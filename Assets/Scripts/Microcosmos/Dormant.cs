using UnityEngine;

/// <summary>
/// ALETARGADO por la estación de la historia (MicroBiome.Bug.dormant): el cangrejo fantasma hiberna en su madriguera de
/// octubre a abril, el caracol turco de Escútari pasa el invierno sellado con su epifragma. Está en el mundo (se le puede
/// ver y encontrar) pero quieto: duerme sin que el reloj día/noche lo despierte, como la pupa en Metamorphosis. Una
/// amenaza percibida (`Anima.aware`) SÍ lo despierta: mientras dura, el letargo suelta el sueño, el agente y el reloj, y
/// SleepCycle y la conducta responden como siempre; al pasar el peligro vuelve a aletargarse.
/// </summary>
public class Dormant : MonoBehaviour
{
    [Tooltip("Por qué está quieto (para el log y la ficha): «hiberna en su madriguera», «sellado con el epifragma»…")]
    public string why;
    /// <summary>Aletargado ahora (no lo ha despertado una amenaza). El cuerpo de cubos lo dibuja recogido (el caracol, dentro de la concha).</summary>
    public bool Resting { get; private set; }
    Anima _a; SleepCycle _sleep; Animal _an; CubeInsect _ci;

    public static bool IsResting(Component c) => c != null && c.TryGetComponent(out Dormant d) && d.enabled && d.Resting;

    void Update()
    {
        if (_a == null) { _a = GetComponent<Anima>(); _an = _a as Animal; _sleep = GetComponent<SleepCycle>(); _ci = GetComponent<CubeInsect>(); }
        if (_a == null || _a.death) { Release(); return; }
        if (_a.aware) { Release(); return; }   // algo lo amenaza: despierta y reacciona (huye, se acoraza, se defiende)
        Resting = true;
        if (_sleep != null) _sleep.Suspended = true;
        _a.asleep = true;
        if (_an != null && _an.nav != null && _an.nav.isOnNavMesh) _an.nav.isStopped = true;
        if (_ci != null) _ci.agitation = 1f;   // en postura de animal, recogido
    }

    // Devuelve el control a SleepCycle, al agente y a la postura (por la mente), una sola vez.
    void Release()
    {
        if (!Resting) return;
        Resting = false;
        if (_sleep != null) _sleep.Suspended = false;
        if (_a != null) _a.asleep = false;
        if (_an != null && _an.nav != null && _an.nav.isOnNavMesh) _an.nav.isStopped = false;
        if (_ci != null) _ci.agitation = -1f;
    }

    void OnDisable() => Release();
}
