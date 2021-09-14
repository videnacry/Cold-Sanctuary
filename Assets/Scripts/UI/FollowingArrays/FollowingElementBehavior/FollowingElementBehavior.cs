using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FollowingElementBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    // --------------------------------VIRTUAL METHODS-------------------------------------------
    public virtual void Init(GameObject elementReference)
    {
        transform.Translate(new Vector3(UnityEngine.Random.Range(1, 5), UnityEngine.Random.Range(1, 5), UnityEngine.Random.Range(1, 5)));
    }
}
