using UnityEngine;

/// <summary>
/// Motor de VOLICIÓN (docs/volition-selection-engine.md): elige QUÉ hacer puntuando los <see cref="Desire"/> del
/// <see cref="DesireCatalog"/> por su necesidad, y despacha el ganador — reemplazando la prioridad FIJA de
/// `Animal.ActiveBehaveTick`. Lo conduce el `AiBrain` (solo si su `useVolition` está activo); la posesión lo suprime
/// aguas arriba (el `PlayerBrain` toma el mando). La amenaza se atiende como REFLEJO cada tick (paridad con hoy);
/// pasará a deseo con piso de seguridad en D3b2.
///
/// **D3b1 (paridad):** con el flag activo reproduce la conducta actual (comer con hambre + sensar amenaza). Mismo
/// throttle que `ActiveBehaveTick`. No cambia nada mientras el flag esté OFF (default).
/// </summary>
public class Volition : MonoBehaviour
{
    [Tooltip("Necesidad mínima para actuar un deseo (por debajo, no hace nada).")]
    public float minNeed = 0.0001f;

    float _nextTick;

    /// <summary>Un tick de decisión (throttled como ActiveBehaveTick): sensa amenaza (reflejo) y, si está libre,
    /// elige y despacha el deseo de mayor necesidad.</summary>
    public void Tick(Animal self)
    {
        if (self == null || self.death || Time.time < _nextTick) return;
        _nextTick = Time.time + TimeController.timeController.TimeSpeedMinuteSecs / Random.Range(0.8f, 1.2f);

        // Deseos volitivos: solo si está libre (mismo guard que hoy para comer). D3b2 mete la amenaza como deseo.
        if (!self.asleep && !self.busy) SelectAndDispatch(self);

        // Reflejo de amenaza: siempre (paridad con ActiveBehaveTick, que llamaba SenseThreats cada tick).
        self.SenseThreats();
    }

    void SelectAndDispatch(Animal self)
    {
        Desire best = null;
        float bestScore = minNeed;
        foreach (Desire d in DesireCatalog.All)
        {
            // D3b3: × EffectiveWeight (confianza D3a) + gate por receptor (E2) cuando los deseos sean frases.
            float score = d.NeedProbe(self);
            if (score > bestScore) { bestScore = score; best = d; }
        }
        best?.Dispatch(self);
    }
}
