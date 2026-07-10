using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum Reaction { Flee, Fight, HitAndRun }

public abstract class Animal : LivingEntity, ITarget, IEdible, ICarrier, IFactory
{
    #region Family
    /// <summary>
    /// Properties wich determine how is going te be the created family of an instance
    /// </summary>
    public abstract Family Group { get; set; }
    #endregion


    // Stages
    public abstract Childhood ChildStage { get; set; }
    public abstract byte[] ChildPreps { get; set; }
    public abstract byte[] ChildEvents { get; set; }

    public abstract Adolescence TeenStage { get; set; }
    public abstract byte[] TeenPreps { get; set; }
    public abstract byte[] TeenEvents { get; set; }

    public abstract Adulthood AdultStage { get; set; }
    public abstract byte[] AdultPreps { get; set; }
    public abstract byte[] AdultEvents { get; set; }



    // Population
    public static HashSet<GameObject> wholePopulation = new HashSet<GameObject>();
    public abstract HashSet<GameObject> Population { get; set; }


    #region Physiognomy
    /// <summary>
    /// Field with property wich contains the base value for new instances
    /// </summary>
    public char sex;
    public char lifeStage;
    public abstract Physiognomy Body { get; set; }
    public abstract ActionsPrep ActsPrep { get; set; }
    #endregion



    public abstract Vector3 HomeOrigin { get; set; }
    public abstract float HomeRadius { get; set; }



    // ITarget
    public float Mass => rig != null ? rig.mass : Body.baseMass;
    public float Speed => nav != null ? nav.speed : 0f;
    public virtual char Faction => 'a';
    public bool Dead => death;
    public bool Consumed => lifeStage == LifeStage.soul;

    // IEdible — los animales son carne comestible una vez muertos
    public virtual OrganicMaterial Material => OrganicMaterial.Meat;
    public virtual float Nutrition => 1f;
    public virtual float Toughness => 0.5f;
    public float Grams => rig != null ? rig.mass : 0f;
    public virtual float BiteSize => 2f;

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
    public virtual float Aggressiveness => 0f;
    public virtual bool DefendsCubs => false;
    public virtual bool CanHitAndRun => false;
    public virtual float PackFactor => 0.5f;
    // Umbral de percepción/reacción ante amenazas (usado en Escape). Baseline por calibrar;
    // debe escalar con agilidad/inteligencia cuando existan esos stats. Ver docs/living-entity.md.
    public virtual float BaseSensibility => 5f;
    // Aptitudes base por especie (1.0 = media real). Ver docs/creature-stats.md.
    public virtual float BaseAgility    => 1f;
    public virtual float BasePerception => 1f;

    // Post-natal species parameters (override per species)
    public virtual float BaseStressLevel       => 0.2f;
    public virtual float ThreatThreshold       => 0.5f;
    public virtual float VocalizationThreshold => 5f;   // hungry > N para que la cría llore
    public virtual float NestSecurityLevel     => 0.5f;
    // Post-natal stage config (override per species; null = sin sistema post-natal)
    public virtual PostNatalStage[] PostNatalStages => null;

    // ── LivingEntity hooks ───────────────────────────────────────────────────────

    protected override char LifeStageChar => lifeStage;

    protected override void RespondToHunger() => StartCoroutine(Feed());

    protected override float EvaluateThreat(GameObject source)
    {
        if (source == null || rig == null) return 0f;
        float enemyMass  = source.GetComponent<Rigidbody>()?.mass ?? 0f;
        float enemySpeed = source.GetComponent<NavMeshAgent>()?.speed ?? 0f;
        return enemyMass * (enemySpeed * 0.5f) / Mathf.Max(rig.mass, 1f);
    }

    public override void RespondToThreat(GameObject threat)
    {
        if (threat == null) return;
        StartCoroutine(Escape(false, new System.Collections.Generic.List<GameObject> { threat }));
    }

    // State — hunger/exhaustion/lp are animal-specific; stress/trauma/fatReserves/temperature/death/asleep live in LivingEntity
    public bool  busy = false;
    public float hungry, exhaustion, lp, sensibility;
    public bool  firstSolidEaten = false; // cría comió un FoodItem por primera vez
    public bool  firstNestExit   = false; // cría salió del nido una vez sola


    // Gameobject components
    public NavMeshAgent nav;
    public Rigidbody rig;
    public Animator ani;

    public abstract AnimationsName animationsName { get; }
    public GameObject bird;
    public GameObject target;



    public virtual void Init()
    {
        Population.Add(gameObject);
        wholePopulation.Add(gameObject);
        HomeOrigin = transform.position;
        nav = GetComponent<NavMeshAgent>();
        rig = GetComponent<Rigidbody>();
        ChildStage.Fatten()(this, 0);   // fija rig.mass y lp = rig.mass
        agility     = BaseAgility;
        perception  = BasePerception;
        sensibility = BaseSensibility * perception;   // más percepción → detecta amenazas antes
        ani = GetComponent<Animator>();
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
        familySize = familySize > 0 ? familySize : this.Group.familySize;
        return Family.RenderFamily(this.gameObject, familySize, this.Group.parentsRate, minParentsCount, this.Group.parentalCare, position, height, radius);
    }








    public IEnumerator Restore()
    {
        float interval = TimeController.timeController.TimeSpeedMinuteSecs / Random.Range(0.8f, 1.2f);
        while (1 == 1)
        {
            if (hungry >= 0 && !asleep && !busy)
                RespondToHunger();
            trauma = Mathf.Max(0f, trauma - 0.2f);
            stress = Mathf.Max(0f, stress - 0.05f);
            EvolveAptitudes(interval);
            CorrectMedium(interval);
            yield return new WaitForSeconds(interval);
        }
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
            if (water != null) { ActsPrep.walk.Prep(this, dt); nav.SetDestination(water.transform.position); }
        }
        else if (!prefersWater && currentMedium == Medium.Water)
        {
            ActsPrep.run.Prep(this, dt); nav.SetDestination(HomeOrigin);   // salir del agua hacia tierra
        }
    }

    public abstract IEnumerator Feed();



    public virtual IEnumerator Escape(bool team, List<GameObject> enemies)
    {
        GameObject threat = enemies[0];
        float enemyMass = threat.GetComponent<Rigidbody>().mass;
        float enemySpeed = threat.GetComponent<NavMeshAgent>().speed;
        Vector3 threatPos = threat.transform.position;

        if (enemyMass * (enemySpeed / 2) - Vector3.Distance(threatPos, transform.position) <= sensibility)
        {
            if (Random.Range(1, 3) > 1) this.ActsPrep.walk.Prep(this, (short)(this.ActsPrep.run.energyCost / 10));
            else this.ActsPrep.run.Prep(this, (short)(this.ActsPrep.run.energyCost / 10));
            yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 20);
            yield break;
        }

        aware = true;
        switch (ResolveReaction(threat))
        {
            case Reaction.Fight:     yield return StartCoroutine(Fight(threat));     break;
            case Reaction.HitAndRun: yield return StartCoroutine(HitAndRun(threat)); break;
            default:                 yield return StartCoroutine(Flee(threat));      break;
        }
        aware = false;
    }

    protected Reaction ResolveReaction(GameObject threat)
    {
        ITarget threatTarget = threat.GetComponent<ITarget>();
        if (threatTarget != null && !CanHarm(threatTarget))
            return Reaction.Flee;

        float enemyMass = threat.GetComponent<Rigidbody>()?.mass ?? 0f;
        float enemySpeed = threat.GetComponent<NavMeshAgent>()?.speed ?? 0f;
        float allyMass = 0f;
        if (Group?.members != null)
        {
            foreach (Animal ally in Group.members)
            {
                if (ally != null && !ally.death && ally != this &&
                    Vector3.Distance(ally.transform.position, transform.position) < HomeRadius)
                    allyMass += ally.rig.mass;
            }
        }
        float myPower = rig.mass + allyMass * PackFactor;
        float enemyPower = enemyMass * enemySpeed;

        bool defendingCubs = DefendsCubs && Group?.fed != null &&
            System.Array.Exists(Group.fed, cub => cub != null && !cub.death &&
                Vector3.Distance(cub.transform.position, threat.transform.position) < 20f);

        if (myPower > enemyPower * 1.5f && Aggressiveness > 0.5f)
            return Reaction.Fight;
        if (defendingCubs && CanHitAndRun)
            return Reaction.HitAndRun;
        return Reaction.Flee;
    }

    protected virtual IEnumerator Flee(GameObject threat)
    {
        Vector3 threatPos = threat.transform.position;
        while (Vector3.Distance(transform.position, threatPos) < 620)
        {
            this.ActsPrep.run.Prep(this, (short)(this.ActsPrep.run.energyCost / 10));
            int afraid = 30;
            while (afraid > 0)
            {
                afraid--;
                if (BirdBehavior.population.Count > 0 && nav != null && nav.isOnNavMesh)
                    nav.SetDestination(BirdBehavior.population.ElementAt(Random.Range(0, BirdBehavior.population.Count)).transform.position);
                yield return new WaitForSeconds(10);
            }
            threatPos = threat.transform.position;
        }
    }

    protected virtual IEnumerator Fight(GameObject threat)
    {
        ITarget threatTarget = threat.GetComponent<ITarget>();
        if (threatTarget == null) yield break;
        float interval = TimeController.timeController.TimeSpeedMinuteSecs / 30f;
        if (Group?.members != null)
        {
            foreach (Animal ally in Group.members)
            {
                if (ally != null && !ally.death && ally != this &&
                    Vector3.Distance(ally.transform.position, transform.position) < HomeRadius)
                    ally.StartCoroutine(ally.Fight(threat));
            }
        }
        while (!threatTarget.Dead &&
               Vector3.Distance(transform.position, threat.transform.position) < HomeRadius)
        {
            if (nav != null && nav.isOnNavMesh) nav.SetDestination(threat.transform.position);
            if (Vector3.Distance(transform.position, threat.transform.position) < 4f)
                threatTarget.Hurt((rig.mass - exhaustion) / 10f);
            yield return new WaitForSeconds(interval);
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
                if (nav != null && nav.isOnNavMesh) nav.SetDestination(threat.transform.position);
                if (Vector3.Distance(transform.position, threat.transform.position) < 4f)
                    threatTarget.Hurt((rig.mass - exhaustion) / 15f);
            }
            else
            {
                Vector3 retreat = transform.position + (transform.position - threat.transform.position).normalized * 10f;
                if (nav != null && nav.isOnNavMesh) nav.SetDestination(retreat);
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