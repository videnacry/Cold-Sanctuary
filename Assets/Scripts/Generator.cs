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
            Animal.StaticGenerateSquareRange(animal, quantity, range, respawnHeight);
        }
    }
}
