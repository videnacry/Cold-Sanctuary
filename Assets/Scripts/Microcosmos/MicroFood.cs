using System.Collections.Generic;
using UnityEngine;

/// <summary>Un RESTO DE COMIDA (o una hoja, en el huerto) que las hormigas recogen y llevan a la guarida.</summary>
public class MicroFood : MonoBehaviour
{
    public bool leaf;
    public Transform carrier;
    public Vector3 home;          // centro de la zona donde reaparece
    public float spread = 40f;

    public static readonly List<MicroFood> All = new List<MicroFood>();
    void OnEnable() { if (!All.Contains(this)) All.Add(this); if (home == Vector3.zero) home = transform.position; }
    void OnDisable() { All.Remove(this); }

    /// <summary>Entregado: reaparece en otro sitio de su zona (siempre hay restos que recoger).</summary>
    public void Respawn()
    {
        carrier = null;
        transform.SetParent(null, true);
        Vector2 r = Random.insideUnitCircle * spread;
        transform.position = new Vector3(home.x + r.x, home.y, home.z + r.y);
    }
}
