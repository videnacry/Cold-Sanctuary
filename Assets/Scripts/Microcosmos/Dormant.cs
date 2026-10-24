using UnityEngine;

/// <summary>
/// ALETARGADO por la estación de la historia (MicroBiome.Bug.dormant): el cangrejo fantasma hiberna en su madriguera de
/// octubre a abril, el caracol turco de Escútari pasa el invierno sellado con su epifragma. Está en el mundo (se le puede
/// ver y encontrar) pero quieto: duerme sin que el reloj día/noche lo despierte, como la pupa en Metamorphosis. Una
/// amenaza cercana sí lo despierta (SleepCycle sigue con su lógica de amenaza cuando el letargo se quita).
/// </summary>
public class Dormant : MonoBehaviour
{
    [Tooltip("Por qué está quieto (para el log y la ficha): «hiberna en su madriguera», «sellado con el epifragma»…")]
    public string why;
    Anima _a; SleepCycle _sleep; Animal _an;

    void Update()
    {
        if (_a == null) { _a = GetComponent<Anima>(); _an = _a as Animal; _sleep = GetComponent<SleepCycle>(); }
        if (_a == null || _a.death) return;
        if (_sleep != null) _sleep.Suspended = true;
        _a.asleep = true;
        if (_an != null && _an.nav != null && _an.nav.isOnNavMesh) _an.nav.isStopped = true;
        CubeInsect ci = GetComponent<CubeInsect>(); if (ci != null) ci.agitation = 1f;   // en postura de animal, recogido
    }
}
