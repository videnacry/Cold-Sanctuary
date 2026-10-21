using UnityEngine;

/// <summary>
/// PATRULLA genérica (sin uniformes ni símbolos): recorre sus puntos y, si VE a un refugiado expuesto (fuera de un
/// escondite y de su zona, sin papeles), lo detiene: vuelve a su punto de partida. Nunca hay violencia en pantalla; el
/// coste es el tiempo perdido. Ve en un cono frontal hasta <see cref="sight"/> m, y no a través de paredes.
/// </summary>
public class Patrol : MonoBehaviour
{
    public Vector3[] route;
    public float speed = 2.2f;
    public float sight = 9f;
    [Range(10f, 180f)] public float fov = 100f;
    public float pause = 2f;
    [Tooltip("Si se asigna, la ruta está en coordenadas locales de este Transform (el pueblo).")]
    public Transform space;

    int _i; float _waitUntil;

    void Start()
    {
        if (space == null || route == null) return;
        for (int i = 0; i < route.Length; i++) route[i] = space.TransformPoint(route[i]);
    }

    void Update()
    {
        if (route != null && route.Length > 0 && Time.time >= _waitUntil && MicroMove.Step(transform, route[_i], speed))
        { _i = (_i + 1) % route.Length; _waitUntil = Time.time + pause; }
        Watch();
    }

    void Watch()
    {
        Vector3 eye = transform.position + Vector3.up * 1.6f;
        foreach (Refugee r in CompassionMission.Refugees)
        {
            if (r == null || !r.Exposed) continue;
            Vector3 to = r.transform.position + Vector3.up * 1f - eye;
            if (to.magnitude > sight || Vector3.Angle(transform.forward, new Vector3(to.x, 0f, to.z)) > fov * 0.5f) continue;
            if (Physics.Raycast(eye, to.normalized, out RaycastHit hit, to.magnitude, ~0, QueryTriggerInteraction.Ignore)
                && !hit.transform.IsChildOf(r.transform)) continue;
            r.Caught();
            CompassionMission.Instance?.Notify($"Una patrulla ha visto a {r.displayName}: vuelve a su punto de partida.");
        }
    }
}
