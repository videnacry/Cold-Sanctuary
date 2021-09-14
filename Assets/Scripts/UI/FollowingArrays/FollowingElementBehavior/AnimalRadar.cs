using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimalRadar : FollowingElementBehavior
{
    public Animal animalReference;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("PointAnimal");
    }

    // Update is called once per frame
    void Update()
    {
    }

    // --------------------------------VIRTUAL METHODS-------------------------------------------
    public override void Init(GameObject elementReference)
    {
        animalReference = elementReference.GetComponent<Animal>();
    }

    public IEnumerator PointAnimal()
    {
        while (animalReference.lifeStage != LifeStage.soul)
        {
            transform.localPosition = new Vector3(0, 0, 0);
            transform.LookAt(animalReference.transform);
            transform.Translate(Vector3.forward * 3);
            yield return new WaitForSeconds(0.06f);
        }
    }
}
