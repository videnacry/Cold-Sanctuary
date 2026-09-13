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
        // LOD: los lejanos deciden menos veces (SenseThreats es O(n²) → el mayor ahorro).
        _nextTick = Time.time + TimeController.timeController.TimeSpeedMinuteSecs / Random.Range(0.8f, 1.2f) * Lod.SlowFactor(self.transform.position);

        // Deseos volitivos: solo si está libre (mismo guard que hoy para comer). D3b2 mete la amenaza como deseo.
        if (!self.asleep && !self.busy) SelectAndDispatch(self);

        // Reflejo de amenaza: siempre (paridad con ActiveBehaveTick, que llamaba SenseThreats cada tick).
        self.SenseThreats();
    }

    [Tooltip("Peso del HÁBITO: cuánto sube el atractivo de un deseo por su confianza-por-uso (temperamento histórico).")]
    public float habitWeight = 0.5f;
    [Tooltip("Peso de la DISCIPLINA en la histéresis: cuánto margen extra debe superar un deseo nuevo para desbancar al actual (el disciplinado no flip-flopea).")]
    public float disciplineHysteresis = 0.15f;

    Desire _last;
    float _lastScore;

    void SelectAndDispatch(Animal self)
    {
        // ARENA de arbitraje (docs/consciousness-mechanics.md §4): los deseos COMPITEN; la resolución depende de los
        // STATS propios → personalidad. Se pondera por: NECESIDAD (drives) × HÁBITO (confianza-por-uso: lo practicado
        // se elige más) + BOND (ya dentro del NeedProbe social) + DISCIPLINA (histéresis: no cambiar de deseo por un
        // margen pequeño). Así dos seres con la misma necesidad eligen distinto según quiénes son.
        Desire best = null;
        float bestScore = minNeed;
        foreach (Desire d in DesireCatalog.All)
        {
            float score = d.NeedProbe(self) * (1f + self.Confidence(d.capability) / 100f * habitWeight);   // hábito
            if (score > bestScore) { bestScore = score; best = d; }
        }

        // DISCIPLINA (histéresis): un ser disciplinado se mantiene en su deseo salvo que otro lo supere por un margen.
        if (_last != null && best != _last && bestScore < _lastScore + self.discipline * disciplineHysteresis)
        {
            best = _last;
            bestScore = _last.NeedProbe(self) * (1f + self.Confidence(_last.capability) / 100f * habitWeight);
        }

        if (best != null)
        {
            best.Dispatch(self);
            self.RecordUse(best.capability, true, 1f);   // el USO construye el hábito (confianza-por-uso, más rápido de joven)
            _last = best; _lastScore = bestScore;
        }
    }
}
