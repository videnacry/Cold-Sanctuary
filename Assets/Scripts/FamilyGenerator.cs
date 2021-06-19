using System;
using UnityEngine;

public class FamilyGenerator : MonoBehaviour
{
    [Serializable]
    public class Family
    {
        public GameObject gameObjectMale;
        public GameObject gameObjectFemale;
        [Header("If minParentsCount == 0 then parentsRate of the script in gameObjectMale is going to be used instead")]
        public int minParentsCount = 0;
        [Header("If quantity == 0 then familySize of the script in gameObjectMale is going to be used instead")]
        public int quantity = 0;
        public Vector3 position;
        public int renderHeight;
    }
    public Family[] families;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Family animal in families)
        {
            animal.gameObjectMale.GetComponent<Animal>().RenderFamily(animal.position, animal.renderHeight, animal.minParentsCount, animal.quantity);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Initialize game
}
