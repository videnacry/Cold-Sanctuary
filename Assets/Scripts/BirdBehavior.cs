
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBehavior : MonoBehaviour, IFactory
{

    public int min, max;
    public float speed, tripTime;
    float velocity=0, lapse,altura;
    int giro=0;
    bool moving = false;
    public static HashSet<GameObject> population = new HashSet<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!moving)
        {
            this.lapse = Random.Range(30, this.tripTime);
            this.velocity = Random.Range(speed, speed / 3)/60;
            this.giro = Random.Range(0, 4);
            altura = Altura(this.giro);
            Fly();
        }
    }

    void Fly()
    {
        StartCoroutine("Move");
    }

    int Altura(int rotation)
    {
        switch (rotation)
        {
            case 1:
                return Random.Range(this.min, this.min + (this.max * 1));
            case 2:
                return Random.Range(this.min, this.min + (this.max * 2));
            case 3:
                return Random.Range(this.min, this.min + (this.max * 3));
            default:
                return Random.Range(this.min, this.min + (this.max * 4));
        }
    }

    //Coroutines

    IEnumerator Move()
    {
        moving = true;
        while (this.lapse>0)
        {
            transform.Rotate(Vector3.up * this.giro);
            transform.Translate(Vector3.forward * this.velocity);
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, this.altura, transform.position.z), velocity / 2);
            lapse--;
            yield return new WaitForSeconds(.1f);
        }
        moving = false;
    }

    public GameObject[] GenerateSquareRange(GameObject animal, GameObject area, int quantity)
    {
        Bounds bounds = area.GetComponent<Collider>().bounds;
        Vector3 minPos = bounds.min;
        Vector3 maxPos = bounds.max;
        GameObject[] birds = new GameObject[quantity];
        for (int idx = 0; quantity > idx; idx++)
        {
            Vector3 pos = new Vector3(Random.Range(minPos.x, maxPos.x), maxPos.y, Random.Range(minPos.z, maxPos.z));
            GameObject bird = Instantiate(gameObject, pos, transform.rotation);
            Vector3 scale = bird.transform.localScale;
            bird.transform.localScale = new Vector3(scale.x - Random.Range(0.1f, 0.4f), scale.y - Random.Range(0.1f, 0.4f), scale.z - Random.Range(0.1f, 0.4f));
            birds[idx] = bird;
            Respawn.birds.Add(bird);
            BirdBehavior.population.Add(bird);
        }
        return birds;
    }

}
