using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class TimeTest : MonoBehaviour
{
    public byte timeSpeed = 30;
    private void Start()
    {
        Test.testers.Add(new Test.Tester(this, gameObject));
    }
    private void Update()
    {
        TimeController.timeController.SetTimeSpeed(timeSpeed);
    }
}
