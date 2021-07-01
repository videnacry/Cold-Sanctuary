using System;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [Serializable]
    public struct Subject
    {
        public GameObject gameObject;
        public int quantity;
    }
    public Subject[] subjects;
    public GameObject area;
    public static HashSet<GameObject> bears = new HashSet<GameObject>(), rabbits = new HashSet<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        transform.LookAt(this.area.GetComponent<Collider>().bounds.center);
        foreach (Subject animal in subjects)
        {
            Fill(animal.gameObject, this.area, animal.quantity);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Initialize game
    
    public void Fill(GameObject animal, GameObject area, int quantity)
    {
        IFactory factory = animal.GetComponent<IFactory>();
        if ( factory != null)
        {
            factory.GenerateSquareRange(animal, area, quantity);
        } else
        {
            Animal.StaticGenerateSquareRange(animal, area, quantity);
        }
    }
}
