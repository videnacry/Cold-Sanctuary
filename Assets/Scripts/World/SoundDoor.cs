using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA PUERTA CORREDERA DE UNA SALA DE SONIDO (Berón 2026-11-05, «hazlo real»; docs/piso-musica.md §Acústica): una hoja de cristal cierra el hueco de cada una de las dos
/// pieles de la sala, y se abre sola cuando alguien está a <see cref="OpenRadius"/> m. Cerrada es lo que da los dB de <see cref="MusicAcoustics"/>. La hoja no lleva colisión:
/// el NavMesh pasa por el hueco y nadie puede quedarse encerrado ni atascado si la puerta llega tarde. La búsqueda es acotada: cada 0,25 s, sobre una lista común que se rehace cada 4 s.
/// </summary>
public class SoundDoor : MonoBehaviour
{
    public const float OpenRadius = 1.8f, SlideSeconds = 0.6f;
    public static readonly List<SoundDoor> All = new List<SoundDoor>();

    [Tooltip("Lo que se desliza al abrir (local).")]
    public Vector3 slide;

    Vector3 _closed;
    float _t, _next;
    bool _want;
    static readonly List<Transform> Bodies = new List<Transform>();
    static float _bodiesAt = -100f;

    public bool Closed => _t <= 0.01f;

    void Awake() { _closed = transform.localPosition; }
    void OnEnable() { if (!All.Contains(this)) All.Add(this); _next = Random.value * 0.25f; }
    void OnDisable() { All.Remove(this); }

    void Update()
    {
        if (Time.time >= _next) { _next = Time.time + 0.25f; _want = Someone(); }
        float target = _want ? 1f : 0f;
        if (Mathf.Approximately(_t, target)) return;
        _t = Mathf.MoveTowards(_t, target, Time.deltaTime / SlideSeconds);
        transform.localPosition = _closed + slide * _t;
    }

    bool Someone()
    {
        if (Time.time - _bodiesAt > 4f)
        {
            _bodiesAt = Time.time;
            Bodies.Clear();
            foreach (Anima a in SceneCache.All<Anima>(4f)) if (a != null) Bodies.Add(a.transform);
        }
        Vector3 at = transform.parent != null ? transform.parent.TransformPoint(_closed) : _closed;
        foreach (Transform b in Bodies)
            if (b != null && (b.position - at).sqrMagnitude < OpenRadius * OpenRadius) return true;
        return false;
    }
}
