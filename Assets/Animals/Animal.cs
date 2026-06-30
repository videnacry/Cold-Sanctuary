using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum Reaction { Flee, Fight, HitAndRun }

public abstract class Animal : MonoBehaviour, IAnimal, ITarget, IEdible, ICarrier, IFactory
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
    public float Mass => rig != null ? rig.mass : Body.mass;
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

    // Bond system (Fase 4)
    public virtual float HarmVsBond => 0.5f;
    public virtual float BondGrowthRate => 1f;
    public List<Bond> bonds = new List<Bond>();

    /// <summary>
    /// BondGrowthRate real: base * etapa de vida * (1 - trauma).
    /// Crías vinculan 3x más rápido; el trauma frena el vínculo.
    /// </summary>
    public float EffectiveBondGrowthRate
    {
        get
        {
            float lifeMultiplier = lifeStage == LifeStage.child ? 3f
                                 : lifeStage == LifeStage.teen  ? 1.5f
                                 : 0.5f;
            return BondGrowthRate * lifeMultiplier * (1f - trauma / 100f);
        }
    }

    public Bond GetBond(ITarget target)
    {
        foreach (Bond b in bonds)
            if (b.target == target) return b;
        return null;
    }

    public void GrowBond(ITarget target, BondType type, float amount)
    {
        Bond b = GetBond(target);
        if (b == null) { b = new Bond(target, type); bonds.Add(b); }
        b.value = Mathf.Clamp(b.value + amount * EffectiveBondGrowthRate, 0f, 100f);
    }

    /// <summary> False si el vínculo es suficientemente alto para bloquear el daño. </summary>
    public bool CanHarm(ITarget target)
    {
        if (target == null) return true;
        Bond b = GetBond(target);
        if (b == null) return true;
        return b.value < HarmVsBond * 100f; // bond=100 siempre bloquea (100 < 100 = false)
    }

    // Post-natal species parameters (override per species)
    public virtual float BaseStressLevel       => 0.2f;
    public virtual float ThreatThreshold       => 0.5f;
    public virtual float VocalizationThreshold => 5f;   // hungry > N para que la cría llore
    public virtual float NestSecurityLevel     => 0.5f;
    public virtual float MaxFatReserves        => 20f;
    public virtual float FatAccumulationRate   => 0.5f;

    // Post-natal stage config (override per species; null = sin sistema post-natal)
    public virtual PostNatalStage[] PostNatalStages => null;

    // State
    public bool asleep = false, death = false, busy = false;
    public float hungry, exhaustion, lp, sensibility;
    public float trauma      = 0f;   // 0–100; frena bond, crece con daño
    public float stress      = 0f;   // 0–1; estrés ambiental/ansiedad
    public float fatReserves = 0f;   // reservas de grasa; universal, maxFatReserves por especie
    public float temperature = 38f;  // temperatura corporal (°C)
    public bool firstSolidEaten = false; // cría comió un FoodItem por primera vez
    public bool firstNestExit   = false; // cría salió del nido una vez sola


    // Gameobject components
    public bool aware { get; set; } = false;
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
        ChildStage.Fatten()(this, 0);
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
    public virtual Animal[] RenderFamily(Vector3 position, float height, int minParentsCount = 0, int familySize = 0)
    {
        familySize = familySize > 0 ? familySize : this.Group.familySize;
        return Family.RenderFamily(this.gameObject, familySize, this.Group.parentsRate, minParentsCount, this.Group.parentalCare, position, height);
    }








    public IEnumerator Restore()
    {
        float interval = TimeController.timeController.TimeSpeedMinuteSecs / Random.Range(0.8f, 1.2f);
        while (1 == 1)
        {
            if (hungry >= 0 && !asleep && !this.busy)
                StartCoroutine("Feed");
            trauma = Mathf.Max(0f, trauma - 0.2f);
            stress = Mathf.Max(0f, stress - 0.05f);
            yield return new WaitForSeconds(interval);
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
                nav.SetDestination(BirdBehavior.population.ElementAt(Random.Range(0, BirdBehavior.population.Count - 1)).transform.position);
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
            nav.SetDestination(threat.transform.position);
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
                nav.SetDestination(threat.transform.position);
                if (Vector3.Distance(transform.position, threat.transform.position) < 4f)
                    threatTarget.Hurt((rig.mass - exhaustion) / 15f);
            }
            else
            {
                Vector3 retreat = transform.position + (transform.position - threat.transform.position).normalized * 10f;
                nav.SetDestination(retreat);
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