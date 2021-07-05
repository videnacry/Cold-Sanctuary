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
    public static HashSet<GameObject> bears = new HashSet<GameObject>(), rabbits = new HashSet<GameObject>();

    bool start = true,hard=false;


    // Start is called before the first frame update
    void Start()
    {
        transform.LookAt(center.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (start) StartCoroutine(Emerge());
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
