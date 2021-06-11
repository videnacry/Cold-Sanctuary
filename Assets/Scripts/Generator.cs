using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Generator : MonoBehaviour
{
    [Serializable]
    public struct Subject
    {
        public GameObject gameObject;
        public int quantity;
    }
    public Subject[] subjects;
    public GameObject center;
    public float area, respawnHeight = 3;
    public static HashSet<GameObject> bears = new HashSet<GameObject>(), rabbits = new HashSet<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        transform.LookAt(center.transform.position);
        foreach (Subject animal in subjects)
        {
            Fill(animal.gameObject, animal.quantity, this.area, this.respawnHeight);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Initialize game
    
    public void Fill(GameObject animal, int quantity, float range, float respawnHeight = 5)
    {
        IFactory factory = animal.GetComponent<IFactory>();
        if ( factory != null)
        {
            factory.GenerateSquareRange(animal, quantity, range, respawnHeight);
        } else
        {
            this.GenerateSquareRange(animal, quantity, range, respawnHeight);
        }
    }

    public GameObject[] GenerateSquareRange(GameObject animal, int quantity, float range, float respawnHeight)
    {
        GameObject[] creatures = new GameObject[quantity];
        for (int idx = 0; quantity > idx; idx++)
        {
            GameObject creature = Instantiate(animal, new Vector3(UnityEngine.Random.Range(0, range), respawnHeight, UnityEngine.Random.Range(0, range)), transform.rotation);
            Vector3 scale = creature.transform.localScale;
            creature.transform.localScale = new Vector3(scale.x - UnityEngine.Random.Range(0.1f, 0.4f), scale.y - UnityEngine.Random.Range(0.1f, 0.4f), scale.z - UnityEngine.Random.Range(0.1f, 0.4f));
            creatures[idx] = creature;
            Respawn.birds.Add(creature);
        }
        return creatures;
    }
}
