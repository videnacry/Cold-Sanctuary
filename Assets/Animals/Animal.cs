using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// enum Reaction → movido a ThreatResponder.cs (etapa 1, docs/anima-dissolving-animal.md)

public class Animal : Anima, ITarget, IEdible, ICarrier, IFactory   // CONCRETA (etapa 5): toda la conducta/data está en componentes+catálogos
{
    #region Family
    /// <summary>
    /// Properties wich determine how is going te be the created family of an instance
    /// </summary>
    // Estructura familiar: DATA de especie (Family.Of), fijada en Init; settable (RenderFamily la reemplaza). Etapa 5.
    public Family Group { get; set; }
    #endregion


    // Stages. Los PREPS de las 3 etapas eran idénticos en todas las especies → default concreto (etapa 5). Los EVENTS
    // varían por especie (territoriales llevan HomeBound) → siguen como data por especie por ahora.
    static readonly byte[] _stagePreps = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public byte[] ChildPreps => _stagePreps;
    public byte[] TeenPreps  => _stagePreps;
    public byte[] AdultPreps => _stagePreps;

    // Etapas + eventos: DATA del catálogo (StageProfile.Of), creadas/leídas en Init (etapa 5). Las etapas son
    // auto-property (se MUTAN: sizePotential) → cada ser las suyas; los eventos, get-only del perfil.
    StageProfile _stages = StageProfile.Of(null);
    public Childhood ChildStage { get; set; }
    public byte[] ChildEvents => _stages.childEvents;

    public Adolescence TeenStage { get; set; }
    public byte[] TeenEvents => _stages.teenEvents;

    public Adulthood AdultStage { get; set; }
    public byte[] AdultEvents => _stages.adultEvents;



    // Population
    public static HashSet<GameObject> wholePopulation = new HashSet<GameObject>();
    // Población viva de la especie: registro central por nombre (etapa 5), ya no un estático por clase.
    public HashSet<GameObject> Population => AnimalPopulations.Of(SpeciesName);


    #region Physiognomy
    /// <summary>
    /// Field with property wich contains the base value for new instances
    /// </summary>
    public char sex;
    public char lifeStage;
    // Físico de la especie (escala/masa/pesos de comida). DATA del catálogo (Physiognomy.Of), fijado en Init — ya no
    // un `defaultBody` por subclase (etapa 5). Settable (crecimiento/composición pueden reemplazarlo).
    public Physiognomy Body { get; set; }
    // Gaits de la especie: DATA del catálogo (ActionsPrep.Of), fijados en Init; settable. Etapa 5.
    public ActionsPrep ActsPrep { get; set; }
    #endregion



    // HomeOrigin = estado por-instancia (se fija en Init/FamilyGenerator). HomeRadius = data de especie (SpeciesBody). Etapa 5.
    public Vector3 HomeOrigin { get; set; }
    public float HomeRadius => _speciesBody != null ? _speciesBody.homeRadius : 100f;



    // ITarget
    public float Mass => rig != null ? rig.mass : Body.baseMass;
    public float Speed => nav != null ? nav.speed : 0f;
    public virtual char Faction => 'a';
    public bool Dead => death;
    public bool Consumed => lifeStage == LifeStage.soul;

    // IEdible — los animales son carne comestible una vez muertos
    public virtual OrganicMaterial Material => Prof.material;
    public virtual float Nutrition => 1f;
    public virtual float Toughness => Prof.toughness;
    public float Grams => rig != null ? rig.mass : 0f;
    public virtual float BiteSize => Prof.biteSize;
    // Config escalar de la especie (data del SpeciesProfile vía SpeciesBody). Nunca null (fallback Default). Etapa 5.
    protected SpeciesProfile Prof => _speciesBody != null ? _speciesBody.profile : SpeciesProfile.Default;
    // Overrides de Anima cableados al perfil de especie (antes eran overrides por clase).
    public override float HarmVsBond          => Prof.harmVsBond;
    public override float BondGrowthRate      => Prof.bondGrowthRate;
    public override float MaxFatReserves      => Prof.maxFatReserves;
    public override float FatAccumulationRate => Prof.fatAccumulationRate;

    public float Consume(float biteSize)
    {
        float effectiveBite = biteSize / (1f + Toughness);
        effectiveBite = Mathf.Min(effectiveBite, rig.mass);
        rig.mass -= effectiveBite;
        if (rig.mass <= 0.1f)
        {
            Population.Remove(gameObject);
            wholePopulation.Remove(gameObject);
            lifeStage = LifeStage.soul;
        }
        return effectiveBite * Nutrition;
    }

    // ICarrier
    protected FoodItem carriedFood;
    public FoodItem CarriedFood => carriedFood;

    public bool PickUp(FoodItem food)
    {
        if (carriedFood != null || food == null || food.Consumed) return false;
        carriedFood = food;
        food.transform.SetParent(transform);
        food.transform.localPosition = Vector3.up;
        return true;
    }

    public FoodItem Drop(Vector3 position)
    {
        if (carriedFood == null) return null;
        FoodItem dropped = carriedFood;
        carriedFood = null;
        dropped.transform.SetParent(null);
        dropped.transform.position = position;
        dropped.droppedBy = this;
        return dropped;
    }

    // ThreatResponse species flags (override per species)

    // Aggressiveness/CanHitAndRun retirados: son config del componente ThreatResponder (etapa 5). Cada especie los fija
    // en ConfigureThreat. La defensa de crías EMERGE del vínculo (cubBond) + autoabandono vs peligro.
    /// <summary>Cada especie configura su <see cref="ThreatResponder"/> (agresividad, pegar-y-correr). Base: pacífico.</summary>
    protected virtual void ConfigureThreat(ThreatResponder t) { }
    public virtual float PackFactor => Prof.packFactor;
    // Umbral de percepción/reacción ante amenazas (usado en Escape). Baseline por calibrar;
    // debe escalar con agilidad/inteligencia cuando existan esos stats. Ver docs/living-entity.md.
    // Bases evolutivas: DATA del componente SpeciesBody (etapa 5). Fallback a los defaults si aún no está.
    public virtual float BaseSensibility => _speciesBody != null ? _speciesBody.baseSensibility : 5f;
    public virtual float BaseAgility    => _speciesBody != null ? _speciesBody.baseAgility : 1f;
    public virtual float BasePerception => _speciesBody != null ? _speciesBody.basePerception : 1f;

    /// <summary>Nombre del ARQUETIPO de especie (docs/soul-composition-blend.md). Si se define, `Init()` llena las
    /// aptitudes NO gestionadas por `Base*` (fuerza/masa/aguante/adaptabilidad + mentales) desde el arquetipo →
    /// migración fase 3 (mitad segura): los animales dejan de tener aptitudes planas (todas 1). null = sin cambio.</summary>
    protected virtual string SpeciesArchetype => null;
    // La especie para relaciones/karma sale del componente SpeciesBody (etapa 5); si aún no está, del arquetipo de la clase.
    public override string SpeciesName => _speciesBody != null && !string.IsNullOrEmpty(_speciesBody.species)
        ? _speciesBody.species : SpeciesArchetype;

    SpeciesBody _speciesBody;   // identidad de especie (stats base + pensamientos) como componente; auto-alta en Init

    // Post-natal species parameters (override per species)
    public virtual float BaseStressLevel       => Prof.baseStressLevel;
    public virtual float ThreatThreshold       => Prof.threatThreshold;   // escala Assess: fracción de MI poder a partir de la cual me alarmo (recalibrable)
    public virtual float VocalizationThreshold => Prof.vocalizationThreshold;   // hungry > N para que la cría llore
    public virtual float NestSecurityLevel     => Prof.nestSecurityLevel;
    // Post-natal stage config (override per species; null = sin sistema post-natal)
    // Secuencia post-natal de la especie: DATA (PostNatalProfile.Of), ya no un override por clase (etapa 5).
    public PostNatalStage[] PostNatalStages => PostNatalProfile.Of(SpeciesArchetype);

    // ── Anima hooks ───────────────────────────────────────────────────────

    protected override char LifeStageChar => lifeStage;

    protected override void RespondToHunger() => StartCoroutine(Feed());

    // La EVALUACIÓN de amenaza vive en ThreatResponder (etapa 1), plenamente stat-based (ya no usa rig.mass/NavMesh).
    protected override float EvaluateThreat(GameObject source)
        => _threat != null ? _threat.Assess(this, source) : 0f;

    public override void RespondToThreat(GameObject threat)
    {
        if (threat == null) return;
        StartCoroutine(Escape(false, new System.Collections.Generic.List<GameObject> { threat }));
    }

    // State — hunger/exhaustion/lp are animal-specific; stress/trauma/fatReserves/temperature/death/asleep live in Anima
    public bool  busy = false;
    public bool  fighting = false;   // guard propio de Fight() — ver comentario en Fight()
    public float hungry, exhaustion, lp, sensibility;
    public bool  firstSolidEaten = false; // cría comió un FoodItem por primera vez
    public bool  firstNestExit   = false; // cría salió del nido una vez sola


    // Gameobject components
    public NavMeshAgent nav;
    public Rigidbody rig;
    public Animator ani;

    [HideInInspector] public WalkSpell Walk;   // opt-in: si está, provee la velocidad del NavMesh (locomoción-hechizo)
    [HideInInspector] public bool Running;     // ¿la acción actual es correr? (channeling del hechizo) — lo fija ActionPrep
    ThreatResponder _threat;                   // política luchar/huir por stats (etapa 1); auto-alta en Init
    [HideInInspector] public Locomotion Loco;  // mover NavMesh + gait (etapa 2); auto-alta en Init
    [HideInInspector] public Forager Forage;   // política "qué/dónde comer" (etapa 3); auto-alta + config en Init

    // Derivado del nombre de especie (IdleWolf/WalkWolf/RunWolf…): ya no un override por clase (etapa 5).
    public AnimationsName animationsName => new AnimationsName(SpeciesArchetype);
    public GameObject bird;
    public GameObject target;



    public virtual void Init()
    {
        if (Group == null) Group = Family.Of(SpeciesArchetype);   // estructura familiar de la especie (data); RenderFamily la reemplaza
        Population.Add(gameObject);
        wholePopulation.Add(gameObject);
        HomeOrigin = transform.position;
        nav = GetComponent<NavMeshAgent>();
        _threat = GetComponent<ThreatResponder>();                       // etapa 1: la política luchar/huir es un componente
        if (_threat == null) _threat = gameObject.AddComponent<ThreatResponder>();
        ConfigureThreat(_threat);                                        // cada especie fija agresividad/pegar-y-correr
        Loco = GetComponent<Locomotion>();                               // etapa 2: mover NavMesh + gait como componente
        if (Loco == null) Loco = gameObject.AddComponent<Locomotion>();
        Forage = GetComponent<Forager>();                                // etapa 3: política "qué/dónde comer"
        if (Forage == null) Forage = gameObject.AddComponent<Forager>();
        Forage.ConfigureForSpecies(SpeciesArchetype);                    // flags de comida por especie (data)
        ActsPrep = ActionsPrep.Of(SpeciesArchetype);   // gaits de la especie (data); ANTES de la config de WalkSpell, que los lee
        Walk = GetComponent<WalkSpell>();     // OPT-IN: locomoción-hechizo (velocidad stat-driven) SOBRE el NavMesh
        if (Walk != null)
        {
            Walk.selfDriven = false;          // el NavMesh navega; el hechizo solo PROVEE la velocidad (con su lógica)
            if (ActsPrep != null && ActsPrep.walk != null) Walk.baseSpeed = ActsPrep.walk.navSpeed;
            if (ActsPrep != null && ActsPrep.walk != null && ActsPrep.run != null)
                Walk.maxPowerWithChanneling = Mathf.Max(0f, ActsPrep.run.navSpeed - ActsPrep.walk.navSpeed);
        }
        Body = Physiognomy.Of(SpeciesArchetype);   // físico de la especie desde el catálogo (data); ANTES de Fatten, que lo usa
        _stages = StageProfile.Of(SpeciesArchetype);   // ciclo de vida de la especie (data); las etapas se crean ANTES de Fatten (usa ChildStage)
        ChildStage = new Childhood(_stages.childDays, _stages.childMin, _stages.childMax);
        TeenStage  = new Adolescence(_stages.teenDays, _stages.teenMin, _stages.teenMax);
        AdultStage = new Adulthood(_stages.adultDays, _stages.adultMin, _stages.adultMax);
        rig = GetComponent<Rigidbody>();
        ChildStage.Fatten()(this, 0);   // fija rig.mass y lp = rig.mass
        // Etapa 5: la identidad de especie (arquetipo → stats base + pensamientos + medio + bases evolutivas) es un componente.
        _speciesBody = GetComponent<SpeciesBody>();
        if (_speciesBody == null) _speciesBody = gameObject.AddComponent<SpeciesBody>();
        if (string.IsNullOrEmpty(_speciesBody.species)) _speciesBody.species = SpeciesArchetype;   // por defecto, el de la clase
        _speciesBody.Apply(this);                      // aptitudes + medio + fija agility/perception/sensibility base
        RecomputeAutoabandono();                       // autoabandono deriva de entrega↔autoconservación (stats/bonds)
        Mind mind = GetComponent<Mind>();
        if (mind != null) HumorProfile.Apply(this, mind.humores);   // humores base por personalidad (si tiene Mente)
        ani = GetComponent<Animator>();
        // Etapa 4: la IA ACTIVA (forrajeo/amenaza) la conduce un brain; la posesión (PlayerBrain) la suprime. El AiBrain
        // se añade antes del AnimaController y se refrescan los brains (por si el controller ya venía del prefab).
        if (GetComponent<AiBrain>() == null) gameObject.AddComponent<AiBrain>();
        AnimaController ac = GetComponent<AnimaController>();
        if (ac == null) ac = gameObject.AddComponent<AnimaController>();
        ac.RefreshBrains();
        StartCoroutine(Restore());
        LifeStage.Init(this, TimeController.timeController);
        PostNatalManager pnm = GetComponent<PostNatalManager>();
        if (pnm != null) pnm.Initialize(this);
    }
    public static GameObject[] StaticGenerateSquareRange(GameObject animal, GameObject area, int quantity)
    {
        Bounds bounds = area.GetComponent<Collider>().bounds;
        Vector3 minPos = bounds.min;
        Vector3 maxPos = bounds.max;
        GameObject[] creatures = new GameObject[quantity];
        for (int idx = 0; quantity > idx; idx++)
        {
            Vector3 pos = new Vector3(Random.Range(minPos.x, maxPos.x), maxPos.y + 1, Random.Range(minPos.z, maxPos.z));
            GameObject creature = Instantiate(animal, pos, animal.transform.rotation);
            Vector3 scale = creature.transform.localScale;
            creature.transform.localScale = new Vector3(scale.x - Random.Range(0.1f, 0.4f), scale.y - Random.Range(0.1f, 0.4f), scale.z - Random.Range(0.1f, 0.4f));
            creatures[idx] = creature;
        }
        return creatures;
    }
    public virtual GameObject[] GenerateSquareRange(GameObject animal, GameObject area, int quantity)
    {
        return Animal.StaticGenerateSquareRange(animal, area, quantity);
    }
    public virtual Animal[] RenderFamily(Vector3 position, float height, int minParentsCount = 0, int familySize = 0, float radius = 0)
    {
        // `this` puede ser el prefab ASSET (FamilyGenerator llama sobre el template, nunca instanciado/Init'd) →
        // Group puede seguir sin fijar (null, o con familySize=0 si quedó de un estado previo). No confiar en
        // que esté listo solo por no ser null: si el tamaño resuelto no es válido, recurrir a Family.Of fresco.
        Family group = Group != null && Group.familySize > 0 ? Group : Family.Of(SpeciesArchetype);
        familySize = familySize > 0 ? familySize : group.familySize;
        return Family.RenderFamily(this.gameObject, familySize, group.parentsRate, minParentsCount, group.parentalCare, position, height, radius);
    }








    // PASIVO (siempre, lo conduzca quien lo conduzca): metabolismo/evolución/medio/velocidad + decaimiento de trauma/estrés.
    // Las decisiones ACTIVAS (forrajeo/amenaza) se movieron a ActiveBehaveTick, que conduce el brain (etapa 4).
    public IEnumerator Restore()
    {
        float interval = TimeController.timeController.TimeSpeedMinuteSecs / Random.Range(0.8f, 1.2f);
        while (1 == 1)
        {
            trauma = Mathf.Max(0f, trauma - 0.2f);
            stress = Mathf.Max(0f, stress - 0.05f);
            EvolveAptitudes(interval);
            CorrectMedium(interval);
            FeedWalkSpeed(interval);
            yield return new WaitForSeconds(interval);
        }
    }

    float _nextBehave;
    /// <summary>Decisiones ACTIVAS de la IA animal (forrajeo + amenaza), conducidas por el brain (`AiBrain`) — la
    /// POSESIÓN las SUPRIME (si el jugador conduce, `AnimaController` llama a `PlayerBrain`, no a este). Throttled
    /// al mismo ritmo que antes tenía `Restore`. Etapa 4 (docs/anima-dissolving-animal.md).</summary>
    public void ActiveBehaveTick()
    {
        if (death || Time.time < _nextBehave) return;
        _nextBehave = Time.time + TimeController.timeController.TimeSpeedMinuteSecs / Random.Range(0.8f, 1.2f);
        if (hungry >= 0 && !asleep && !busy) RespondToHunger();
        SenseThreats();
    }



    // Evolución lenta de aptitudes por actividad (ver docs/creature-stats.md §Evolución de aptitudes).
    // Correr/perseguir/huir sube la agilidad; estar alerta (aware) sube la percepción; el reposo las decae.
    void EvolveAptitudes(float dt)
    {
        float runSpeed = ActsPrep?.run != null ? ActsPrep.run.navSpeed : 0f;
        float intensity = (nav != null && nav.isOnNavMesh && runSpeed > 0.01f)
            ? Mathf.Clamp01(nav.velocity.magnitude / runSpeed) : 0f;
        agility     = AptitudeEvolution.Step(agility,    BaseAgility,    intensity,       dt);
        perception  = AptitudeEvolution.Step(perception, BasePerception, aware ? 1f : 0f, dt);
        sensibility = BaseSensibility * perception;   // la sensibilidad sigue a la percepción evolucionada
    }

    // OPT-IN: si el animal lleva un WalkSpell, su velocidad de NavMesh sale del hechizo (correr = channeling → sube
    // gradual a la punta; andar → decae; con su gasto). El NavMesh sigue navegando (pathfinding). Ver docs.
    void FeedWalkSpeed(float dt)
    {
        if (Walk == null || nav == null || !nav.isOnNavMesh) return;
        nav.speed = Walk.StepSpeed(false, Running, nav.hasPath, dt);   // charging=false; channeling=Running; moving=tiene ruta
    }

    // Comportamiento de medio: los acuáticos buscan agua si quedan en tierra; los terrestres salen
    // del agua hacia tierra. Solo cuando no cazan/huyen (busy) — así un oso que persigue focas sí
    // entra al agua. Ver docs/refuge-and-adult-behavior.md.
    void CorrectMedium(float dt)
    {
        if (busy || asleep || nav == null || !nav.isOnNavMesh) return;
        bool prefersWater = WaterAffinity > LandAffinity;
        if (prefersWater && currentMedium != Medium.Water)
        {
            FishSchool water = FishSchool.Nearest(transform.position);   // marcadores de agua
            if (water != null) Loco.Walk(water.transform.position, dt);
        }
        else if (!prefersWater && currentMedium == Medium.Water)
        {
            Loco.Run(HomeOrigin, dt);   // salir del agua hacia tierra
        }
    }

    // Territorialidad/cautela: percibe depredadores cercanos y huye proactivamente (revive
    // EvaluateThreat + ThreatThreshold). Evita que las presas se queden tranquilas junto a depredadores;
    // también hace que un cánido solo huya de un oso. Solo cuando no caza/huye/duerme.
    // Rendimiento: escanea wholePopulation; a gran escala requeriría partición espacial.
    void SenseThreats()
    {
        if (busy || aware || asleep || rig == null) return;
        float range = HomeRadius * (0.5f + perception * 0.5f);   // la percepción amplía la alerta
        GameObject threat = null;
        float nearest = range;
        foreach (GameObject go in wholePopulation)
        {
            if (go == null || go == gameObject) continue;
            Animal predator = go.GetComponent<Animal>();   // "depredador" = come presa (Forager.eatsPrey), ya no el tipo Carnivore
            if (predator == null || predator.death || predator.Forage == null || !predator.Forage.eatsPrey) continue;
            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d <= nearest && EvaluateThreat(go) > ThreatThreshold) { nearest = d; threat = go; }
        }
        if (threat != null) RespondToThreat(threat);
    }

    // Forrajear: cazar si come presa, si no pastar/pescar. Reemplaza los Feed de Carnivore/Herbivore (concreto, etapa 5).
    public IEnumerator Feed() => Forage.eatsPrey ? Forage.Hunt(this) : Forage.Graze(this);



    public virtual IEnumerator Escape(bool team, List<GameObject> enemies)
    {
        // aware se marca ANTES de cualquier rama (incluida la de salida temprana) y SIEMPRE
        // se limpia en el finally. Antes solo se marcaba en la rama "de verdad peligroso", así
        // que la rama de salida temprana (yield break) nunca dejaba aware=true — y como
        // SenseThreats() solo se frena con "aware", relanzaba un Escape() nuevo en cada tick de
        // Restore() mientras el depredador siguiera cerca, apilando corrutinas sin límite (bug
        // que colgó el proceso/la máquina — ver DEVLOG).
        aware = true;
        try
        {
            GameObject threat = enemies[0];
            Vector3 threatPos = threat.transform.position;
            float scare = EvaluateThreat(threat);                          // stat-based (Assess): fracción de mi poder × bond + aura
            float dist  = Vector3.Distance(threatPos, transform.position);

            // Poco peligroso / lejos (relativo a mi sensibilidad de detección) → solo NERVIOS (andar/correr random),
            // no reacción plena. `alertReach` convierte el miedo en metros: más peligroso → reacciono de más lejos.
            if (scare * _threat.alertReach - dist <= sensibility)
            {
                Loco.SetGait(Random.Range(1, 3) <= 1, (short)(this.ActsPrep.run.energyCost / 10));
                yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 20);
                yield break;
            }

            switch (ResolveReaction(threat))
            {
                case Reaction.Fight:     yield return StartCoroutine(Fight(threat));     break;
                case Reaction.HitAndRun: yield return StartCoroutine(HitAndRun(threat)); break;
                default:                 yield return StartCoroutine(Flee(threat));      break;
            }
        }
        finally
        {
            aware = false;
        }
    }

    // La POLÍTICA de decisión vive en ThreatResponder (etapa 1). Aquí solo se computa el contexto de CRÍAS
    // (acoplado a Family/Group, que aún vive en Animal) y se delega.
    protected Reaction ResolveReaction(GameObject threat)
    {
        RecomputeAutoabandono();   // fresco con los bonds actuales
        // Defensa de crías EMERGENTE (no un flag DefendsCubs): si hay crías propias cerca de la amenaza, la decisión
        // sale sola del vínculo con ellas (cubBond) + autoabandono vs el peligro. docs/anima-dissolving-animal.md.
        bool defendingCubs = Group?.fed != null &&
            System.Array.Exists(Group.fed, cub => cub != null && !cub.death &&
                Vector3.Distance(cub.transform.position, threat.transform.position) < 20f);
        float cubBond = defendingCubs ? CubBondFactor() : 0f;
        return _threat.Decide(this, threat, autoabandono, defendingCubs, cubBond);
    }

    // Vínculo medio con las crías defendidas (0..1); si aún no hay bond, afinidad de cría por defecto (0.4).
    float CubBondFactor()
    {
        if (Group?.fed == null) return 0f;
        float sum = 0f; int n = 0;
        foreach (Animal cub in Group.fed)
            if (cub != null && !cub.death) { Bond b = GetBond(cub); sum += b != null ? b.value : 40f; n++; }
        return n > 0 ? Mathf.Clamp01(sum / n / 100f) : 0f;
    }

    protected virtual IEnumerator Flee(GameObject threat)
    {
        Vector3 threatPos = threat.transform.position;
        while (Vector3.Distance(transform.position, threatPos) < 620)
        {
            Loco.SetGait(true, (short)(this.ActsPrep.run.energyCost / 10));
            int afraid = 30;
            while (afraid > 0)
            {
                afraid--;
                if (BirdBehavior.population.Count > 0)
                    Loco.GoTo(BirdBehavior.population.ElementAt(Random.Range(0, BirdBehavior.population.Count)).transform.position);
                yield return new WaitForSeconds(10);
            }
            threatPos = threat.transform.position;
        }
    }

    // Reclutaba aliados con "ally.StartCoroutine(ally.Fight(threat))" en cada entrada, sin
    // comprobar si ese aliado YA estaba peleando — cada aliado reclutado volvía a reclutar al
    // resto de la manada (incluido quien lo reclutó a él), y como esto se repetía en cada tick
    // de Restore()/SenseThreats() mientras el combate seguía activo, generaba una explosión
    // combinatoria de corrutinas (una manada de N lobos podía multiplicar llamadas a
    // StartCoroutine sin límite) — esto, sumado al bug de Escape(), fue lo que colgó la máquina.
    // Fix: guard propio "fighting" (try/finally) — no reentrar si ya se está peleando, y solo
    // reclutar aliados que no estén ya peleando.
    protected virtual IEnumerator Fight(GameObject threat)
    {
        if (fighting) yield break;
        ITarget threatTarget = threat.GetComponent<ITarget>();
        if (threatTarget == null) yield break;
        fighting = true;
        try
        {
            float interval = TimeController.timeController.TimeSpeedMinuteSecs / 30f;
            if (Group?.members != null)
            {
                foreach (Animal ally in Group.members)
                {
                    if (ally != null && !ally.death && !ally.fighting && ally != this &&
                        Vector3.Distance(ally.transform.position, transform.position) < HomeRadius)
                        ally.StartCoroutine(ally.Fight(threat));
                }
            }
            while (!threatTarget.Dead &&
                   Vector3.Distance(transform.position, threat.transform.position) < HomeRadius)
            {
                Loco.GoTo(threat.transform.position);
                if (Vector3.Distance(transform.position, threat.transform.position) < 4f)
                    threatTarget.Hurt((rig.mass - exhaustion) / 10f);
                yield return new WaitForSeconds(interval);
            }
        }
        finally
        {
            fighting = false;
        }
    }

    // Ataca solo por la espalda; retrocede cuando la amenaza encara al animal.
    protected virtual IEnumerator HitAndRun(GameObject threat)
    {
        ITarget threatTarget = threat.GetComponent<ITarget>();
        if (threatTarget == null) yield break;
        float interval = TimeController.timeController.TimeSpeedMinuteSecs / 30f;
        while (!threatTarget.Dead &&
               Vector3.Distance(transform.position, threat.transform.position) < HomeRadius)
        {
            Vector3 dirToMe = (transform.position - threat.transform.position).normalized;
            if (Vector3.Dot(threat.transform.forward, dirToMe) > 0)
            {
                Loco.GoTo(threat.transform.position);
                if (Vector3.Distance(transform.position, threat.transform.position) < 4f)
                    threatTarget.Hurt((rig.mass - exhaustion) / 15f);
            }
            else
            {
                Vector3 retreat = transform.position + (transform.position - threat.transform.position).normalized * 10f;
                Loco.GoTo(retreat);
            }
            yield return new WaitForSeconds(interval);
        }
    }





    /// <summary>
    /// Inflict damage, remove gameObject from population and wholePopulation fields, set lifestage to soul and rig.mass to 0
    /// </summary>
    /// <param name="damage"></param>
    public virtual void Hurt(float damage)
    {
        lp -= damage;
        exhaustion += damage;
        trauma = Mathf.Clamp(trauma + (damage / Mathf.Max(rig.mass, 1f)) * 30f, 0f, 100f);
        if (!death && lp < rig.mass * 0.7f)
        {
            transform.Rotate(Vector3.forward, 90);
            death = true;
            StopAllCoroutines();
            ani.enabled = false;
            rig.isKinematic = true;
            nav.enabled = false;
        }
    }



    // Gizmos
    public float gizmoSphereRadio = 5;
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, gizmoSphereRadio);
    }



    // Collision
    public void OnCollisionEnter(Collision collision)
    {
        nav.enabled = true;
        GetComponent<BoxCollider>().enabled = false;
    }
}