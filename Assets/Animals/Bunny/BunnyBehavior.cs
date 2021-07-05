using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BunnyBehavior : Animal
{
    #region Family
    /// <summary>
    /// Properties wich determine how is going te be the created family of an instance
    /// </summary>
    public override char ParentalCare { get; set; } = Family.maternal;
    public override float ParentsRate { get; set; } = 0.14f;
    public override byte FamilySize { get; set; } = 4;

    public ActionsPrep actsPrep = new ActionsPrep
    (
        new ActionPrep("IdleBunny", 0, 1, -2),
        new ActionPrep("RunBunny", 8, 4),
        new ActionPrep("RunBunny", 22, 10, 3)
    );
    public override ActionsPrep ActsPrep { get => actsPrep; set => actsPrep = value; } 
    #endregion


    #region Physiognomy
    /// <summary>
    /// Field with property wich contains the base value for new instances
    /// </summary>
    public float baseMass = 7;
    public override float BaseMass { get => baseMass; set => baseMass = value; }


    public Vector3 baseScale = new Vector3(0.8f, 0.8f, 0.8f);
    public override Vector3 BaseScale { get => baseScale; set => baseScale = value; }
    #endregion



    public Vector3 homeOrigin;
    public override Vector3 HomeOrigin { get => homeOrigin; set => homeOrigin = value; }

    public float homeRadius = 20;
    public override float HomeRadius { get => homeRadius; set => homeRadius = value; }




    // Stages
    public Childhood childhood = new Childhood(50, 50, 80);
    public override Childhood ChildStage { get => childhood; set => childhood = value; }

    public byte[] childPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] ChildPreps { get => childPreparations; set => childPreparations = value; }

    public byte[] childEvents = { 
        LifeStage.Events.LoopGrow,
        LifeStage.Events.Fatten,
        LifeStage.Events.Wander,
        LifeStage.Events.Rest,
        LifeStage.Events.HomeBound 
    };
    public override byte[] ChildEvents { get => childEvents; set => childEvents = value; }
    
    
    public Adolescence adolescence = new Adolescence(680, 20, 40); 
    public override Adolescence TeenStage { get => adolescence; set => adolescence = value; }

    public byte[] teenPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] TeenPreps { get => teenPreparations; set => teenPreparations = value; }

    public byte[] teenEvents = {
        LifeStage.Events.LoopGrow,
        LifeStage.Events.Fatten,
        LifeStage.Events.Wander,
        LifeStage.Events.Rest
    };
    public override byte[] TeenEvents { get => teenEvents; set => teenEvents = value; }


    public Adulthood adulthood = new Adulthood(2190, 0, 20);
    public override Adulthood AdultStage { get => adulthood; set => adulthood = value; }

    public byte[] adultPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] AdultPreps { get => adultPreparations; set => adultPreparations = value; }

    public byte[] adultEvents = {
        LifeStage.Events.LoopGrow,
        LifeStage.Events.Fatten,
        LifeStage.Events.Wander,
        LifeStage.Events.Rest
    };
    public override byte[] AdultEvents { get => adultEvents; set => adultEvents = value; }






    public static HashSet<GameObject> population = new HashSet<GameObject>();
    public override HashSet<GameObject> Population { get => population; set => population = value; }
    public bool hunting;
    public override AnimationsName animationsName { get; } = new AnimationsName("Bunny");



    public GameObject mom, player;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
    }


   

    public IEnumerator Shooted(GameObject bullet)
    {
        int wait = 5;
        Vector3 bulletPosition;
        do
        {
            bulletPosition = bullet.transform.position;
            wait--;
            float zDistance = Vector3.Distance(new Vector3(0, 0, bulletPosition.z), new Vector3(0, 0, this.transform.position.z));
            if (zDistance < 1)
            {
                float xDistance = Vector3.Distance(new Vector3(bulletPosition.x, 0), new Vector3(this.transform.position.x, 0));
                if (xDistance < 1)
                {
                    float yDistance = Vector3.Distance(new Vector3(0, bulletPosition.y), new Vector3(0, this.transform.position.y));
                    if (yDistance < 1)
                    {
                        this.exhaustion += 2;
                        StopCoroutine("Hunt");
                        StopCoroutine("Escape");
                        hunting = false;
                        StartCoroutine("Sleep");
                        Debug.Log(this.gameObject);
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        } while (wait > 0);
    }
}
