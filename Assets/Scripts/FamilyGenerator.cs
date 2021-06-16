using System;
using System.Collections.Generic;
using UnityEngine;

public class FamilyGenerator : MonoBehaviour
{
    [Serializable]
    public struct Subject
    {
        public GameObject gameObjectMale;
        public GameObject gameObjectFemale;
        public int quantity;
        public Vector3 position;
        public int positionHeight;
        public int parentsCount;
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
            Fill(animal.gameObjectMale, animal.quantity, animal.parentsCount, animal.position, animal.positionHeight);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Initialize game
    
    public void Fill(GameObject animal, int quantity, int parentsCount, Vector3 position, float respawnHeight = 5)
    {
        IFactory factory = animal.GetComponent<IFactory>();
        if ( factory != null)
        {
            Animal.StaticRenderPaternalFamilyCount(animal, quantity, parentsCount, position, respawnHeight);
        } else
        {
            Animal.StaticRenderPaternalFamilyCount(animal, quantity, parentsCount, position, respawnHeight);
        }
    }
}
