using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Respawn : MonoBehaviour
{
    public GameObject[] beasts, aves;
    public int[] quantity;
    public GameObject center;
    public int amount;
    public static List<GameObject> birds = new List<GameObject>();
    Queue<GameObject> momsBear = new Queue<GameObject>();
    Queue<GameObject> momsWolf = new Queue<GameObject>();
    List<WolfBehavior> wolves = new List<WolfBehavior>();
    List<WolfBehavior> wolfChildren = new List<WolfBehavior>();
    WolfBehavior alpha = new WolfBehavior();
    public static HashSet<GameObject> bears = new HashSet<GameObject>(), rabbits = new HashSet<GameObject>();
    GameObject momBear;
    GameObject momWolf;

    bool start = true,hard=false;


    // Start is called before the first frame update
    void Start()
    {
        transform.LookAt(center.transform.position);
        Fill(beasts,false);
        Fill(aves, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            StartCoroutine("Emerge");
        }
    }

    // Initialize game
    
    void Fill(GameObject[] animals,bool ave)
    {
        if (!ave)
        {
            int i = 0;
            int childBear = 0;
            int childWolf = 0;
            int total = amount, birdAmount = amount/2;
            while (animals.Length > i + 1)
            {
                float percent = (total / (animals.Length - i)) / 3f;
                amount = (int)(percent * 4);
                total -= amount;
                while (amount > 0)
                {
                    if (childBear == 0)
                    {
                        try
                        {
                            momBear = momsBear.Dequeue();
                            childBear = (Random.Range(1, 2));
                        }
                        catch { }
                    }
                    if(childWolf == 0)
                    {
                        try
                        {
                            momWolf = momsWolf.Dequeue();
                            childWolf = (Random.Range(1, 3));
                        }
                        catch { }
                    }
                    amount--;
                    GameObject animal=Instantiate(animals[i], new Vector3(Random.Range(330, 910), 10, Random.Range(222, 980)), transform.rotation);
                    int size = Random.Range(4, 10);
                    if (animal.transform.localScale.x > 1)
                    {
                        if (8 < size)
                        {
                            Vector3 scale = animal.transform.localScale;
                            if (i > 1 && i < 6)
                            {
                                WolfBehavior wolf = animal.GetComponent<WolfBehavior>();
                                wolf.gender = ((Random.Range(0, 4) >= 2) ? true : false);
                                float floatSize = ((size + 4) / 10);
                                wolf.adult = false;
                                animal.transform.localScale = new Vector3(scale.x - floatSize, scale.y - floatSize, scale.z - floatSize);
                                wolfChildren.Add(wolf);
                                try
                                {
                                    wolf.mom = momWolf;
                                    animal.transform.position = wolf.mom.transform.position + (Vector3.right * Random.Range(2,4));
                                    WolfBehavior mom = wolf.mom.GetComponent<WolfBehavior>();
                                    wolf.hungry = mom.hungry;
                                    childWolf--;
                                }
                                catch
                                {
                                    wolf.hungry = Random.Range(0, 4);
                                }
                            }
                            else if (i > 5)
                            {
                                BearBehaviour bear = animal.GetComponent<BearBehaviour>();
                                bear.gender = ((Random.Range(0, 4) >= 2) ? true : false);
                                float floatSize = ((size + 8) / 10);
                                bear.adult = false;
                                animal.transform.localScale = new Vector3(scale.x - floatSize, scale.y - floatSize, scale.z - floatSize);
                                bear.sizePotential = new Vector3(scale.x - Random.Range(0.1f, 0.3f), scale.y - Random.Range(0.1f, 0.3f), scale.z - Random.Range(0.1f, 0.3f));
                                try
                                {
                                    bear.mom = momBear;
                                    animal.transform.position = bear.mom.transform.position + (Vector3.right * Random.Range(2,4));
                                    BearBehaviour mom = momBear.GetComponent<BearBehaviour>();
                                    bear.hungry = mom.hungry;
                                    childBear--;
                                    bear.alone = false;
                                    mom.alone = false;
                                    mom.children[childBear] = bear;
                                }
                                catch
                                {
                                    bear.hungry = Random.Range(0, 4);
                                }
                            }

                        }
                        else
                        {
                            if (i > 1 && i < 6)
                            {
                                WolfBehavior wolf = animal.GetComponent<WolfBehavior>();
                                wolf.gender = ((Random.Range(0, 4) >= 2) ? true : false);
                                wolf.hungry = Random.Range(0, 5);
                                Vector3 scale = animal.transform.localScale;                                
                                animal.transform.localScale = new Vector3(scale.x + Random.Range(-0.1f, 0.3f), scale.y + Random.Range(-0.1f, 0.3f), scale.z + Random.Range(-0.1f, 0.3f));
                                /*
                                if (wolf.gender)
                                {
                                    wolf.rig.mass = (10 * wolf.size.x) + (10 * wolf.size.y) + (10 * wolf.size.z);
                                }
                                else
                                {
                                    wolf.rig.mass = (14 * wolf.size.x) + (14 * wolf.size.y) + (14 * wolf.size.z);
                                }
                                */
                                try
                                {
                                    if (wolf.rig.mass > alpha.rig.mass)
                                    {
                                        wolves.Add(alpha);
                                        alpha = wolf;
                                    }
                                    else
                                    {
                                        wolves.Add(wolf);
                                    }
                                }
                                catch
                                {
                                    alpha = wolf;
                                }
                            }
                            else if (i > 5)
                            {
                                BearBehaviour bear = animal.GetComponent<BearBehaviour>();
                                bear.gender = true;
                                Vector3 scale = animal.transform.localScale;
                                animal.transform.localScale = new Vector3(scale.x - Random.Range(0.3f, 0.7f), scale.y - Random.Range(0.3f, 0.7f), scale.z - Random.Range(0.3f, 0.7f));
                                momsBear.Enqueue(animal);
                                bears.Add(animal);
                                bear.hungry = Random.Range(0, 5);
                                bear.Size();
                                bear.rig.mass = (30 * bear.size.x) + (30 * bear.size.y) + (30 * bear.size.z);
                            }
                        }
                    }
                    else
                    {
                        rabbits.Add(animal);
                    }
                }
                i++;
            }
            alpha.group = wolfChildren.ToArray();
            foreach (WolfBehavior wolf in alpha.group)
            {
                wolf.alone = false;
            }
            while (total > 0)
            {
                total--;
                GameObject animal = Instantiate(animals[i], new Vector3(Random.Range(330, 910), 9, Random.Range(222, 980)), transform.rotation);
                bears.Add(animal);
                Vector3 scale = animal.transform.localScale;
                animal.transform.localScale = new Vector3(scale.x + Random.Range(-0.1f, 0.4f), scale.y + Random.Range(-0.1f, 0.4f), scale.z + Random.Range(-0.1f, 0.4f));
                BearBehaviour bear = animal.GetComponent<BearBehaviour>();
                bear.Size();
                bear.rig.mass = (80 * bear.size.x) + (80 * bear.size.y) + (80 * bear.size.z);
                bear.hungry = Random.Range(0, 5);
            }
            amount = birdAmount;
        }
        else
        {
            int i = 0;
            int total = amount;
            while (animals.Length > i + 1)
            {
                float percent = (total / (animals.Length - i)) / 3f;
                amount = (int)(percent * 4);
                total -= amount;
                while (amount > 0)
                {
                    amount--;
                    GameObject bird=Instantiate(animals[i], new Vector3(Random.Range(330, 910), 250, Random.Range(222, 980)), transform.rotation);
                    Vector3 scale = bird.transform.localScale;
                    bird.transform.localScale = new Vector3(scale.x - Random.Range(0.1f, 0.4f), scale.y - Random.Range(0.1f, 0.4f), scale.z - Random.Range(0.1f, 0.4f));
                    Respawn.birds.Add(bird);
                    BirdBehavior.population.Add(bird);
                }
                i++;
            }
            while (total > 0)
            {
                total--;
                GameObject bird=Instantiate(animals[i], new Vector3(Random.Range(330, 910), 250, Random.Range(222, 980)), transform.rotation);
                Vector3 scale = bird.transform.localScale;
                bird.transform.localScale = new Vector3(scale.x - Random.Range(0.1f, 0.4f), scale.y - Random.Range(0.1f, 0.4f), scale.z - Random.Range(0.1f, 0.4f));
                Respawn.birds.Add(bird);
                BirdBehavior.population.Add(bird);
            }
        }
    }

    // Respawn animals

    IEnumerator Emerge()
    {
        start = false;
        yield return new WaitForSeconds(Random.Range((hard)?180:60, (hard)?120:360));
        amount = Random.Range(1, 3);
        int creature = Random.Range(1, beasts.Length);
        amount *= quantity[creature];
        while (amount > 0)  
        {
            Instantiate(beasts[creature], transform.position, transform.rotation);
            amount--;
            yield return new WaitForSeconds(Random.Range(1, 10));
        }
        start = true;
    }
}
