using System;
using System.Collections.Generic;
using UnityEngine;

public class FamilyGenerator : MonoBehaviour
{
    [Serializable]
    public class Family
    {
        public GameObject gameObjectMale;
        public GameObject gameObjectFemale;
        public int quantity;
        public Vector3 position;
        public int renderHeight;
    }
    [Serializable]
    public class FamilyPaternalCounted: Family
    {
        public int parentsCount;
        public GameObject[] Render()
        {
            return Animal.StaticRenderPaternalFamilyCount(this.gameObjectMale, this.quantity, this.parentsCount, this.position, this.renderHeight);
        }
    }
    [Serializable]
    public class FamilyPaternalRate : Family
    {
        public float parentsRate;
        public GameObject[] Render()
        {
            return Animal.StaticRenderPaternalFamilyRate(this.gameObjectMale, this.quantity, this.parentsRate, this.position, this.renderHeight);
        }
    }
    public FamilyPaternalCounted[] familiesPaternalsCounted;
    public FamilyPaternalRate[] familiesPaternalsRate;



    // Start is called before the first frame update
    void Start()
    {
        foreach (FamilyPaternalCounted animal in familiesPaternalsCounted)
        {
            animal.Render();
        }
        foreach (FamilyPaternalRate animal in familiesPaternalsRate)
        {
            animal.Render();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Initialize game
}
