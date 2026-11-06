using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Lo que gira en la sala: el barrido del radar y el tambor del sismógrafo.</summary>
public class GuardSpin : MonoBehaviour
{
    public Vector3 degreesPerSecond = new Vector3(0f, 90f, 0f);
    void Update() => transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
}
