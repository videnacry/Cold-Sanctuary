using System.Collections;
using UnityEngine;

/// <summary>
/// Añadir al GameObject de la MADRE. Gestiona la sub-máquina de etapas post-natales:
/// ejecuta entryActions al entrar a cada stage, corre el ciclo de decisión de la madre
/// y el ciclo autónomo de las crías cuando la madre se aleja.
/// Inicializado desde Animal.Init() si PostNatalStages != null.
/// Ver docs/behavior-system.md §Fase 6.
/// </summary>
public class PostNatalManager : MonoBehaviour
{
    Animal mother;
    PostNatalStage[] stages;
    int stageIdx = 0;
    float daysInStage = 0f;
    float nurseTimer = 0f;
    float visitTimer = 0f;

    // Distancia a la que se considera que la madre está en el nido
    const float MotherProximity     = 12f;
    // Fracción de madre.Body.GetMealWeight que determina hambre suficiente para compartir
    const float FedThresholdFactor  = 0.5f;
    // Temperatura corporal objetivo: con madre cerca / sola
    const float TempWithMother      = 38f;
    const float TempAlone           = 35f;
    const float TempCritical        = 33f;
    // Estrés que activa inmovilidad (venado/conejo)
    const float ImmobilityStress    = 0.65f;
    // Estrés mínimo para que la cría llore (complementa VocalizationThreshold)
    const float VocalizationStress  = 0.3f;

    PostNatalStage CurrentStage =>
        stages != null && stageIdx < stages.Length ? stages[stageIdx] : null;

    // ──────────────────────────────────────────────
    // Inicialización (llamada desde Animal.Init())
    // ──────────────────────────────────────────────

    public void Initialize(Animal owner)
    {
        mother = owner;
        stages = owner.PostNatalStages;
        if (stages == null || stages.Length == 0) { enabled = false; return; }
        StartCoroutine(RunPostNatal());
    }

    // ──────────────────────────────────────────────
    // Máquina de estados principal
    // ──────────────────────────────────────────────

    IEnumerator RunPostNatal()
    {
        while (stageIdx < stages.Length && !mother.death)
        {
            PostNatalStage stage = stages[stageIdx];
            daysInStage = 0f;
            nurseTimer  = 0f;
            visitTimer  = 0f;

            if (stage.entryActions != null && stage.entryActions.Length > 0)
                yield return StartCoroutine(ExecuteEntryActions(stage));

            yield return StartCoroutine(RunStageLoop(stage));
            stageIdx++;
        }
        // Todas las etapas completadas → separación definitiva
    }

    // ──────────────────────────────────────────────
    // Loop de un stage
    // ──────────────────────────────────────────────

    IEnumerator RunStageLoop(PostNatalStage stage)
    {
        float tick = TimeController.timeController.TimeSpeedMinuteSecs / 10f;

        while (!stage.CanTransition(mother, PrimaryCub, daysInStage) && !mother.death)
        {
            float dayFraction = tick / TimeController.timeController.TimeSpeedMinuteSecs;
            daysInStage += dayFraction;
            nurseTimer  += tick;
            visitTimer  += tick;

            AccumulateFat(tick);
            UpdateTemperatures(tick);

            yield return StartCoroutine(MotherDecisionTick(stage, tick));

            Animal[] cubs = GetCubs();
            if (cubs != null)
                foreach (Animal cub in cubs)
                    if (cub != null && !cub.death)
                        yield return StartCoroutine(CubSoloCycle(cub, stage, tick));

            yield return new WaitForSeconds(tick);
        }
    }

    // ──────────────────────────────────────────────
    // entryActions — secuenciales, una sola vez
    // ──────────────────────────────────────────────

    IEnumerator ExecuteEntryActions(PostNatalStage stage)
    {
        foreach (MotherAction action in stage.entryActions)
            yield return StartCoroutine(DoMotherAction(action, stage));
    }

    // ──────────────────────────────────────────────
    // Ciclo de decisión de la madre (prioridad)
    // ──────────────────────────────────────────────

    IEnumerator MotherDecisionTick(PostNatalStage stage, float duration)
    {
        // 1. Amenaza activa → Escape/ResolveReaction ya lo maneja Animal.Escape()
        if (mother.aware) yield break;

        Animal cub = PrimaryCub;
        if (cub == null) yield break;

        // 2. ¿La ventana de presencia lo permite?
        if (!IsPresenceWindowOpen(stage, duration))
        {
            // La madre se aleja del nido si corresponde
            if (Vector3.Distance(mother.transform.position, cub.HomeOrigin) < MotherProximity * 2f)
            {
                Vector3 away = mother.HomeOrigin + Random.insideUnitSphere * mother.HomeRadius * 0.7f;
                away.y = mother.transform.position.y;
                mother.nav.SetDestination(away);
                mother.ActsPrep.walk.Prep(mother, duration);
            }
            yield break;
        }

        // Volver al nido si está lejos
        float distToNest = Vector3.Distance(mother.transform.position, cub.HomeOrigin);
        if (distToNest > MotherProximity)
        {
            mother.nav.SetDestination(cub.HomeOrigin);
            mother.ActsPrep.run.Prep(mother, duration);
            yield break;
        }

        // 3. ¿Cría pide? (vocaliza o stress alto)
        if (cub.stress > VocalizationStress || cub.hungry > cub.VocalizationThreshold)
        {
            yield return StartCoroutine(DoMotherAction(MotherAction.Nurse, stage));
            yield break;
        }

        // 4. ¿Tiempo desde última toma?
        float nurseInterval = 3f * TimeController.timeController.TimeSpeedMinuteSecs;
        if (nurseTimer > nurseInterval && mother.exhaustion < 70f)
        {
            yield return StartCoroutine(DoMotherAction(MotherAction.Nurse, stage));
            yield break;
        }

        // 5. ¿Madre hambrienta? → caza o consume reservas
        if (mother.hungry >= 0f)
        {
            if (mother.fatReserves > 0f)
            {
                float consumed = duration * 0.5f;
                mother.fatReserves = Mathf.Max(0f, mother.fatReserves - consumed);
                mother.hungry      = Mathf.Max(-mother.Body.GetMealWeight(mother), mother.hungry - consumed);
            }
            // Si no tiene reservas, Restore() ya lanza Carnivore.Feed() cuando hungry >= 0
            yield break;
        }

        // 6. Descansar y acicalar
        yield return StartCoroutine(DoMotherAction(MotherAction.Clean, stage));
        mother.ActsPrep.idle.Prep(mother, duration);
    }

    // ──────────────────────────────────────────────
    // Ejecutar una acción de madre
    // ──────────────────────────────────────────────

    IEnumerator DoMotherAction(MotherAction action, PostNatalStage stage)
    {
        float d = TimeController.timeController.TimeSpeedMinuteSecs / 20f;
        Animal[] cubs = GetCubs();

        switch (action)
        {
            case MotherAction.Clean:
            case MotherAction.Stimulate:
                mother.ActsPrep.idle.Prep(mother, d);
                ForEachCub(cubs, cub =>
                {
                    cub.stress = Mathf.Max(0f, cub.stress - 0.2f);
                    mother.GrowBond(cub, BondType.Imprint, d);
                });
                break;

            case MotherAction.GuideTeat:
                mother.ActsPrep.idle.Prep(mother, d);
                break;

            case MotherAction.FirstMilk:
                mother.ActsPrep.idle.Prep(mother, d);
                nurseTimer = 0f;
                ForEachCub(cubs, cub =>
                {
                    cub.hungry = -mother.Body.GetMealWeight(mother);
                    cub.stress = 0f;
                    // calostro: bono de anticuerpos → trauma base empieza bajo
                    cub.trauma = Mathf.Max(0f, cub.trauma - 10f);
                    mother.GrowBond(cub, BondType.Imprint, d * 3f);
                });
                mother.exhaustion += d;
                break;

            case MotherAction.Nurse:
                mother.ActsPrep.idle.Prep(mother, d);
                nurseTimer = 0f;
                float nutrition = stage.feedingMethod == FeedingMethod.Regurgitate ? d : d * 2f;
                ForEachCub(cubs, cub =>
                {
                    cub.hungry = Mathf.Max(cub.hungry - nutrition, -mother.Body.GetMealWeight(mother));
                    cub.stress = Mathf.Max(0f, cub.stress - 0.3f);
                    mother.GrowBond(cub, BondType.Imprint, nutrition);
                });
                mother.exhaustion += d * 0.5f;
                break;

            case MotherAction.Regurgitate:
                mother.ActsPrep.idle.Prep(mother, d);
                Animal first = PrimaryCub;
                if (first != null)
                {
                    FoodItem dropped = (mother as ICarrier)?.Drop(first.HomeOrigin + Vector3.right * 0.5f);
                    if (dropped != null)
                    {
                        nurseTimer = 0f;
                        mother.GrowBond(first, BondType.Imprint, d);
                    }
                }
                break;

            case MotherAction.Rest:
                mother.ActsPrep.idle.Prep(mother, d * 3f);
                mother.exhaustion = Mathf.Max(0f, mother.exhaustion - d * 2f);
                break;

            case MotherAction.Guard:
                mother.ActsPrep.idle.Prep(mother, d);
                break;

            case MotherAction.MarkHidingSpot:
                ForEachCub(cubs, cub => cub.HomeOrigin = cub.transform.position);
                break;
        }

        yield return new WaitForSeconds(d);
    }

    // ──────────────────────────────────────────────
    // Ciclo autónomo de la cría (cuando la madre no está)
    // ──────────────────────────────────────────────

    IEnumerator CubSoloCycle(Animal cub, PostNatalStage stage, float duration)
    {
        bool motherNear = Vector3.Distance(mother.transform.position, cub.HomeOrigin) < MotherProximity;
        if (motherNear) yield break;

        // Temperatura crítica → buscar calor / volver al nido
        if (cub.temperature < TempCritical)
        {
            cub.nav.SetDestination(cub.HomeOrigin);
            cub.ActsPrep.run.Prep(cub, duration);
            yield break;
        }

        // Inmovilidad: stress alto + nido inseguro (venado, conejo)
        if (cub.stress > ImmobilityStress && cub.NestSecurityLevel < 0.35f)
        {
            cub.ActsPrep.idle.Prep(cub, duration);
            yield break;
        }

        // Hambre alta + agotamiento → letargo
        if (cub.hungry > cub.VocalizationThreshold * 1.5f && cub.exhaustion > 60f)
        {
            cub.ActsPrep.idle.Prep(cub, duration * 2f);
            cub.stress = Mathf.Min(1f, cub.stress + 0.03f * duration);
            yield break;
        }

        // Hambre media → vocalizar
        if (cub.hungry > cub.VocalizationThreshold)
        {
            cub.stress = Mathf.Min(1f, cub.stress + 0.08f * duration);
            cub.ActsPrep.idle.Prep(cub, duration);
            // aquí iría animación/sonido de llanto
            yield break;
        }

        // Temperatura baja → buscar calor (sin ser crítica)
        if (cub.temperature < TempAlone + 0.5f)
        {
            cub.nav.SetDestination(cub.HomeOrigin);
            cub.ActsPrep.walk.Prep(cub, duration);
            yield break;
        }

        // Hambre baja + energía → exploración pequeña
        cub.stress = Mathf.Max(0f, cub.stress - 0.05f * duration);
        Vector3 wander = cub.HomeOrigin + Random.insideUnitSphere * 4f;
        wander.y = cub.transform.position.y;
        cub.nav.SetDestination(wander);
        cub.ActsPrep.walk.Prep(cub, duration);
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    bool IsPresenceWindowOpen(PostNatalStage stage, float dt)
    {
        switch (stage.presencePattern)
        {
            case MotherPresencePattern.Continuous:
                return true;

            case MotherPresencePattern.FrequentVisits:
                // Ciclo: presente N segundos, ausente N segundos
                float halfPeriod = TimeController.timeController.TimeSpeedMinuteSecs * 3f;
                return (visitTimer % (halfPeriod * 2f)) < halfPeriod;

            case MotherPresencePattern.MinimalVisits:
                // Solo visita cuando el estrés ambiental es bajo (noche/silencio)
                // La reducción de stress nocturna la maneja TimeController en el futuro.
                // Por ahora: ventana breve cada ~8 "horas" de simulación.
                float visitPeriod = TimeController.timeController.TimeSpeedMinuteSecs * 8f;
                float visitWindow = TimeController.timeController.TimeSpeedMinuteSecs * 0.5f;
                return (visitTimer % visitPeriod) < visitWindow
                       && mother.stress < mother.BaseStressLevel * 0.7f;

            case MotherPresencePattern.ProgrammedAbandonment:
                return true; // la transición de stage maneja el abandono definitivo

            default:
                return true;
        }
    }

    void AccumulateFat(float dt)
    {
        if (mother.hungry < -mother.Body.GetMealWeight(mother) * FedThresholdFactor)
            mother.fatReserves = Mathf.Min(mother.MaxFatReserves,
                mother.fatReserves + mother.FatAccumulationRate * dt);
    }

    void UpdateTemperatures(float dt)
    {
        Animal[] cubs = GetCubs();
        if (cubs == null) return;
        foreach (Animal cub in cubs)
        {
            if (cub == null || cub.death) continue;
            bool near  = Vector3.Distance(mother.transform.position, cub.HomeOrigin) < MotherProximity;
            float target = near ? TempWithMother : TempAlone;
            cub.temperature = Mathf.MoveTowards(cub.temperature, target, dt * 0.3f);
        }
    }

    void ForEachCub(Animal[] cubs, System.Action<Animal> action)
    {
        if (cubs == null) return;
        foreach (Animal cub in cubs)
            if (cub != null && !cub.death) action(cub);
    }

    Animal PrimaryCub
    {
        get
        {
            Animal[] f = GetCubs();
            return f != null && f.Length > 0 ? f[0] : null;
        }
    }

    Animal[] GetCubs() => mother?.Group?.fed;
}
