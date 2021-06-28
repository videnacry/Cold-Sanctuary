using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Test : MonoBehaviour
{
    [Serializable]
    public class Tester
    {
        public MonoBehaviour script;
        public GameObject gameObject;
        public Tester (MonoBehaviour pScript, GameObject pGameObject)
        {
            script = pScript;
            gameObject = pGameObject;
        }
    }
    public static List<Tester> testers = new List<Tester>();
    public List<Tester> testerInstances;
    private void Start ()
    {
        Test.testers.Add(new Tester(this, gameObject));
        testerInstances = Test.testers;
    }
}
