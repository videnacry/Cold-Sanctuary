using UnityEngine;
using System.Collections.Generic;
using System;

public class HashSetHolder : MonoBehaviour
{
    public virtual HashSet<GameObject> GetHashSetHolded()
    {
        return new HashSet<GameObject>();
    }
}