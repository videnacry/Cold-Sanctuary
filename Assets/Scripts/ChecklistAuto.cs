using System.Collections;
using UnityEngine;

/// <summary>
/// Cubre por CÓDIGO bloques de `docs/testing-checklist.md` que la guía pedía comprobar **a mano en el
/// inspector**, para que queden con evidencia `[TEST]` en el log como el resto. No simula pulsaciones:
/// pone directamente el estado que la tecla o el inspector provocarían y asevera el efecto.
///
/// Sigue el contrato del resto de unidades: **determinista y NO destructivo** (crea lo suyo, lo destruye,
/// y restaura lo global que toca). Grupo 20 → corre DESPUÉS de los 15 grupos existentes, para no alterar
/// el estado que ellos miden.
///
/// Lo que NO está aquí, a propósito, porque no se puede falsear con honestidad:
///  · Trepar (`Espacio`) y fuego (`G`): el juego los lee con `Input.GetKey` **sostenido**; forzar el
///    estado interno no probaría lo que el ítem pide (que la tecla mantenida funcione). Prueba FÍSICA.
///  · La rama de daño de «Dura» (§3): exige **acercarse andando** con WASD; teletransportar el Transform
///    ya falló dos veces en ciclos anteriores. Prueba FÍSICA.
///  · Misiones WASD (§33), observación recorriendo el mapa (§39), consola `~`, las 4 esferas de
///    Mesopotamia (§6) y la misión de yoga (§4): las conduce el jugador.
/// Ya estaban cubiertos por otras unidades (no se duplican aquí): armamento⟂masa y `Assess` gateado por
/// sentidos (`FaunaChecks`), miedo/temple/adicción (`StateSpellsTest`), `Heal` (`SicknessTest`).
/// </summary>
public class ChecklistAuto : MonoBehaviour, ITestUnit
{
    public int Group => 20;
    public bool ParallelSafe => false;   // toca la velocidad de tiempo, que es global

    public IEnumerator Run()
    {
        TestProbe.Begin("ChecklistAuto (bloques que pedían inspector)");
        yield return ApremioTomaElMando();
        yield return VelocidadDeTiempoSubeRecursos();
        TestProbe.End();
    }

    /// <summary>
    /// `testing-recent-features.md` §3 — con el apremio por encima de `takeoverArousal` (~0.85) el CUERPO
    /// toma el mando: sube `AiBrain.selfRelevance`, aparece `physicalBoost` (subidón) y `mindStatus
    /// CalculationFog` (visión de túnel). Y al bajar el apremio, lo suelta.
    /// </summary>
    IEnumerator ApremioTomaElMando()
    {
        GameObject go = new GameObject("Apremio_TEST");
        Anima anima = go.AddComponent<Anima>();
        AiBrain ai = go.AddComponent<AiBrain>();          // antes del ASR: su Awake captura la relevancia base
        float baseRelevance = ai.selfRelevance;
        AcuteStressResponse asr = go.AddComponent<AcuteStressResponse>();

        anima.stress = 0.95f;                              // arousal = max(stress, hambre, alerta)
        yield return null; yield return null; yield return null;   // que Update corra con dt > 0

        TestProbe.Check("apremio alto → el CUERPO toma el mando (InTakeover)", asr.InTakeover,
                        $"stress={anima.stress:F2} umbral={asr.takeoverArousal:F2}");
        TestProbe.Greater("apremio alto → selfRelevance sube sobre la base", ai.selfRelevance, baseRelevance,
                          $"base={baseRelevance:F2} ahora={ai.selfRelevance:F2}");
        TestProbe.Greater("apremio alto → physicalBoost > 0 (subidón drena glucosa)", anima.physicalBoost, 0f);
        TestProbe.Greater("apremio alto → niebla cognitiva > 0 (visión de túnel)", anima.mindStatusCalculationFog, 0f);

        anima.stress = 0f;
        yield return null; yield return null;
        TestProbe.Check("apremio bajo → suelta el mando", !asr.InTakeover);
        TestProbe.Near("apremio bajo → la niebla vuelve a 0", anima.mindStatusCalculationFog, 0f, 0.01f);

        Object.Destroy(go);
    }

    /// <summary>
    /// §2 — "al subir la velocidad de tiempo, los recursos suben más rápido". `AreaProducer` tickea en
    /// tiempo de JUEGO (`TimeController.TimeSpeedMinuteSecs`), así que la escala sale sola. Se asevera el
    /// invariante del reloj **y** se mide el recurso realmente ganado a velocidad baja vs alta.
    /// </summary>
    IEnumerator VelocidadDeTiempoSubeRecursos()
    {
        TimeController tc = TimeController.timeController;
        if (!TestProbe.NotNull("hay TimeController", tc)) yield break;
        float originalDay = tc.RealMinutesPerDay;   // la duración del día configurada (Clock)

        tc.SetTimeSpeed(1);
        float secsAt1 = tc.TimeSpeedMinuteSecs;
        tc.SetTimeSpeed(10);
        float secsAt10 = tc.TimeSpeedMinuteSecs;
        TestProbe.Check("más velocidad = menos segundos reales por minuto de juego",
                        secsAt10 < secsAt1, $"x1 → {secsAt1:F1}s/min · x10 → {secsAt10:F1}s/min");

        // Medición real: el mismo productor, un segundo de reloj real a cada velocidad.
        GameObject go = new GameObject("Producer_TEST");
        AreaProducer prod = go.AddComponent<AreaProducer>();
        prod.resource = SanctuaryResource.Food;
        prod.perGameMinute = 600f;      // alto, para que un segundo real se note
        prod.perWorkerBonus = 0f;
        prod.assignedWorkers = 0;

        tc.SetTimeSpeed(1);
        yield return null;
        float a0 = SanctuaryResources.Instance.Get(SanctuaryResource.Food);
        yield return new WaitForSeconds(1f);
        float slow = SanctuaryResources.Instance.Get(SanctuaryResource.Food) - a0;

        tc.SetTimeSpeed(20);
        yield return null;
        float b0 = SanctuaryResources.Instance.Get(SanctuaryResource.Food);
        yield return new WaitForSeconds(1f);
        float fast = SanctuaryResources.Instance.Get(SanctuaryResource.Food) - b0;

        TestProbe.Greater("a velocidad x1 el área YA produce", slow, 0f, $"+{slow:F2} Food/s");
        TestProbe.Greater("a x20 produce MÁS que a x1", fast, slow, $"x1 +{slow:F2} vs x20 +{fast:F2} Food/s");

        Object.Destroy(go);
        tc.SetDayLength(originalDay);   // restaurar: otras unidades/el juego leen la velocidad global
    }
}
