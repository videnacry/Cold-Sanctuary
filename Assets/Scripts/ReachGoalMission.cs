using UnityEngine;

/// <summary>
/// Primera misión JUGABLE (WASD) que además es un TEST del juego real (docs/testing-checklist.md §33). El jugador
/// camina hasta un marcador ("el refugio"); al llegar, reporta por `TestProbe` — el **mismo canal `[TEST]`** que los
/// sandboxes de laboratorio, así el compañero ve laboratorio y juego real juntos (grep `[TEST]`). Comprueba también que
/// el jugador **se movió de verdad** (recorrido > umbral) → prueba que hubo input WASD, no un spawn sobre la meta.
///
/// Visión (usuario): las misiones-test de hoy son el AVANCE del juego de mañana. No es andamiaje desechable: es una
/// misión mínima ("llega al refugio") que luego se encadena con las reales. No es `ITestUnit` (la conduce el jugador,
/// no el `TestRunner`); emite sus propias líneas `[TEST]` al completarse.
/// </summary>
public class ReachGoalMission : MonoBehaviour
{
    [Tooltip("Distancia a la meta para considerarla alcanzada.")]
    public float goalRadius = 3f;
    [Tooltip("Recorrido mínimo del jugador para dar por bueno que hubo WASD (no spawn sobre la meta).")]
    public float movedThreshold = 4f;
    [Tooltip("Desplazamiento de la meta respecto al inicio del jugador (m, en +Z).")]
    public float goalAhead = 12f;

    Transform _player;
    Vector3 _start;
    bool _hasPlayer, _done;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) { Debug.Log("[TEST] SKIP · Misión WASD (llegar al refugio) — no hay Player en escena"); return; }
        _player = p.transform;
        _start = _player.position;
        _hasPlayer = true;

        // Meta = este GO, colocado adelante del jugador; marcador visible sin collider (que no estorbe).
        transform.position = _start + new Vector3(0f, 0f, goalAhead);
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "RefugeMarker";
        marker.transform.SetParent(transform, false);
        marker.transform.localScale = new Vector3(1.5f, 3f, 1.5f);
        Collider col = marker.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }

    void Update()
    {
        if (_done || !_hasPlayer || _player == null) return;
        if (Vector3.Distance(_player.position, transform.position) > goalRadius) return;

        _done = true;
        float traveled = Vector3.Distance(_start, _player.position);
        TestProbe.Begin("Misión WASD: llegar al refugio");
        TestProbe.Check("el jugador se movió (hubo WASD)", traveled > movedThreshold, $"recorrido={traveled:0.#} m");
        TestProbe.Check("llegó al refugio", true);
        TestProbe.End();
    }

    void OnGUI()
    {
        if (!_hasPlayer) return;
        string msg = _done
            ? "✓ Misión completada: llegaste al refugio."
            : $"Misión (WASD): ve al marcador del refugio  ·  {Vector3.Distance(_player.position, transform.position):0} m";
        GUI.Label(new Rect(10f, 34f, 480f, 22f), msg);
    }
}
