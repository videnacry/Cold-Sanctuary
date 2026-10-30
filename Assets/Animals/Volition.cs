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
        // Ni mientras REACCIONA a una amenaza (aware: huye, pelea): esa corrutina lleva el cuerpo. Antes el deseo de turno
        // («volver a casa») re-apuntaba el agente cada decisión y la huida lo volvía a apartar: el pingüino iba y venía
        // («da vueltas en círculo (homebound)»).
        if (!self.asleep && !self.busy && !self.aware) SelectAndDispatch(self);

        // Reflejo de amenaza: siempre (paridad con ActiveBehaveTick, que llamaba SenseThreats cada tick).
        self.SenseThreats();
    }

    [Tooltip("Peso del HÁBITO: cuánto sube el atractivo de un deseo por su confianza-por-uso (temperamento histórico).")]
    public float habitWeight = 0.5f;
    [Tooltip("Peso de la DISCIPLINA en la histéresis: cuánto margen extra debe superar un deseo nuevo para desbancar al actual (el disciplinado no flip-flopea).")]
    public float disciplineHysteresis = 0.15f;

    Desire _last;
    float _lastScore;

    /// <summary>El último deseo que ganó (diagnóstico del censo).</summary>
    public string LastDesire => _last != null ? _last.key : "";

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
        // Solo si el deseo actual SIGUE vivo (necesidad > mínimo): antes un deseo ya satisfecho («eat» tras comer) se quedaba
        // por histéresis para siempre y el animal saciado constaba como «quiere comer y no se mueve».
        if (_last != null && best != _last && _last.NeedProbe(self) > minNeed && bestScore < _lastScore + self.discipline * disciplineHysteresis)
        {
            best = _last;
            bestScore = _last.NeedProbe(self) * (1f + self.Confidence(_last.capability) / 100f * habitWeight);
        }

        // Nada lo mueve ahora (saciado, en calma): deja de «querer» lo último (LastDesire vacío = descansa).
        if (best == null) { _last = null; _lastScore = 0f; return; }

        if (best != null)
        {
            // POR LA COLA (docs/typed-spells-and-queues §4): en vez de actuar directo, el ser "lanza" el deseo como un
            // hechizo en su ActionQueue (carril opcional) → usa la misma cola que el jugador. El carril `own` (reflejo
            // del cuerpo, AcuteStressResponse) tiene prioridad, así que un apremio corporal pausa esto solo. Si no hay
            // ActionQueue (o useQueue=false), cae al despacho directo de siempre (paridad).
            // Con el CUERPO AL MANDO (AcuteStressResponse: el carril `own` lleva su reflejo), el carril opcional queda
            // bloqueado; pero ese reflejo es justo «la IA conduce»: el deseo se despacha DIRECTO. Antes se quedaba en
            // cola sin ejecutarse nunca → un animal con hambre/alerta ni comía ni hacía nada (fauna congelada, 2026-10-23).
            ActionQueue q = useQueue ? self.GetComponent<ActionQueue>() : null;
            if (q != null && q.Own.Count > 0) best.Dispatch(self);
            else if (q != null) { if (q.Player.Count == 0) q.EnqueuePlayer(best.Conjure(self)); }   // deseo → SUPRAHECHIZO en la cola
            else best.Dispatch(self);
            self.RecordUse(best.capability, true, 1f);   // el USO construye el hábito (confianza-por-uso, más rápido de joven)
            _last = best; _lastScore = bestScore;
        }
    }

    [Tooltip("Despachar los deseos POR LA ActionQueue (el ser 'lanza' sus acciones como el jugador). Si no hay cola, directo.")]
    public bool useQueue = true;
}
