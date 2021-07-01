
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFactory
{
    public GameObject[] GenerateSquareRange(GameObject animal, GameObject area, int quantity);
}
