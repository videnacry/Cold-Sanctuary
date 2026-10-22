using UnityEngine;

/// <summary>
/// La RUTINA de un mirmidón de las historias (docs/microcosmos-historias.md): va de un puesto a otro de su escena (el
/// pozo, el taller, la cama del enfermo, la trinchera…) y se queda un rato en cada uno, como quien hace su jornada. Así
/// el pueblo de la historia vive aunque el jugador no le hable. Se para mientras la <see cref="StoryMission"/> le necesita
/// quieto (<see cref="hold"/>).
/// </summary>
public class MicroRoutine : MonoBehaviour
{
    public Transform[] stops = new Transform[0];
    public float speed = 2.2f;
    public Vector2 pause = new Vector2(6f, 14f);
    public bool hold;

    int _i;
    float _until;

    void Start() { _i = Random.Range(0, Mathf.Max(1, stops.Length)); _until = Time.time + Random.Range(0f, pause.y); }

    void Update()
    {
        if (hold || stops.Length == 0 || Time.time < _until) return;
        Transform to = stops[_i];
        if (to == null) { _i = (_i + 1) % stops.Length; return; }
        if (!MicroMove.Step(transform, to.position, speed, 2f)) return;
        _until = Time.time + Random.Range(pause.x, pause.y);
        _i = (_i + 1 + Random.Range(0, Mathf.Max(1, stops.Length - 1))) % stops.Length;
    }
}
