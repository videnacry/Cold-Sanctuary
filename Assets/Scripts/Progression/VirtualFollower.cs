using UnityEngine;

/// <summary>El turista de la iniciación: sigue al jugador por la escena, a un par de metros.</summary>
public class VirtualFollower : MonoBehaviour
{
    public float distance = 2f, speed = 3f;
    Transform _p;
    void Update()
    {
        if (_p == null) { GameObject g = GameObject.FindGameObjectWithTag("Player"); if (g == null) return; _p = g.transform; }
        Vector3 to = _p.position - transform.position; to.y = 0f;
        if (to.magnitude > distance) transform.position += to.normalized * Mathf.Min(speed * Time.deltaTime, to.magnitude - distance);
        if (to.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(to);
    }
}
