
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFactory
{
    public GameObject[] GenerateSquareRange(GameObject gameObject, int quantity, float range, float respawnHeight);
}
