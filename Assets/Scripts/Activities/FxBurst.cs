using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Un efecto de <see cref="ActivityKit.Burst"/>: mueve sus cubitos y se destruye al acabar su vida (sin esperas sin tope).</summary>
public class FxBurst : MonoBehaviour
{
    readonly List<(Transform t, Vector3 v)> _bits = new List<(Transform, Vector3)>();
    float _g, _die;

    public void Init(Color c, Vector3 v, float g, float size, float life, int n, float spread)
    {
        ActivityKit.Alive++;
        _g = g; _die = Time.time + life;
        for (int i = 0; i < n; i++)
        {
            Transform t = KitchenProps.C(transform, Vector3.zero, Vector3.one * size, c, Random.Range(0f, 360f), "bit");
            Vector3 jitter = new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.1f, 0.3f), Random.Range(-0.4f, 0.4f)) * spread;
            _bits.Add((t, v + jitter));
        }
    }

    void Update()
    {
        if (Time.time >= _die) { Destroy(gameObject); return; }
        float dt = Time.deltaTime;
        for (int i = 0; i < _bits.Count; i++)
        {
            var b = _bits[i]; if (b.t == null) continue;
            b.v.y -= _g * dt;
            b.t.position += b.v * dt;
            _bits[i] = b;
        }
    }

    void OnDestroy() { ActivityKit.Alive = Mathf.Max(0, ActivityKit.Alive - 1); }
}
