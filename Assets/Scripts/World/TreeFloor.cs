using UnityEngine;

/// <summary>Un piso del árbol (marca): su número, su giro y su inclinación. Los muebles de un piso que gira se miden en
/// su propio marco (<see cref="FurnitureSpacing"/>).</summary>
public class TreeFloor : MonoBehaviour
{
    public int index;
    public float tilt, yaw;
}

