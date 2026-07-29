using UnityEngine;

/// <summary>
/// Puntero de virtualización INPUT-AGNÓSTICO (docs/kitchen-simulation.md §3b). Mantiene una posición en
/// pantalla (arranca arriba-centro) y la actualiza con CUALQUIERA de tres entradas, a la vez, para que el
/// juego se pueda jugar **solo con teclado**, **con ratón** o **con touch** — esa apertura es el objetivo:
///   • TECLADO: las teclas de cámara mueven el puntero (delta).
///   • RATÓN: si el ratón se mueve, fija el puntero en su posición.
///   • TOUCH: si hay un toque, fija el puntero ahí; el inicio del toque confirma.
/// Lanza un rayo desde la cámara por el puntero, detecta la <see cref="StationPart"/> apuntada y, al
/// **confirmar** (tecla / clic / toque), emite su paso a todas las <see cref="ProductionOrder"/>.
/// Dibuja una mira por OnGUI. Añádelo a un GameObject de la escena (uno por jugador).
/// </summary>
public class VirtualPointer : MonoBehaviour
{
    public Camera cam;

    [Header("Teclado — mover con las teclas de cámara")]
    public KeyCode up = KeyCode.UpArrow;
    public KeyCode down = KeyCode.DownArrow;
    public KeyCode left = KeyCode.LeftArrow;
    public KeyCode right = KeyCode.RightArrow;
    public float keyboardSpeed = 900f;   // px/s
    public KeyCode confirmKey = KeyCode.Space;

    [Header("Alcance del rayo")]
    public float rayDistance = 100f;

    Vector2 _pos;
    bool _init;
    StationPart _hover;

    void Awake() { if (cam == null) cam = Camera.main; }
    void OnEnable() { ResetToTopCenter(); }

    void ResetToTopCenter()
    {
        _pos = new Vector2(Screen.width * 0.5f, Screen.height * 0.82f);
        _init = true;
    }

    void Update()
    {
        if (!_init) ResetToTopCenter();
        bool confirm = false;

        // TECLADO (delta con las teclas de cámara).
        Vector2 d = Vector2.zero;
        if (Input.GetKey(up)) d.y += 1f;
        if (Input.GetKey(down)) d.y -= 1f;
        if (Input.GetKey(right)) d.x += 1f;
        if (Input.GetKey(left)) d.x -= 1f;
        if (d.sqrMagnitude > 0f) _pos += d.normalized * keyboardSpeed * Time.deltaTime;
        if (Input.GetKeyDown(confirmKey)) confirm = true;

        // RATÓN (si se mueve, manda; si se clica, confirma).
        if (Input.mousePresent)
        {
            if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0f)
                _pos = Input.mousePosition;
            if (Input.GetMouseButtonDown(0)) confirm = true;
        }

        // TOUCH (si hay toque, manda; el inicio confirma).
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            _pos = t.position;
            if (t.phase == TouchPhase.Began) confirm = true;
        }

        _pos.x = Mathf.Clamp(_pos.x, 0f, Screen.width);
        _pos.y = Mathf.Clamp(_pos.y, 0f, Screen.height);

        // Rayo → parte apuntada.
        _hover = null;
        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(_pos);
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
            {
                _hover = hit.collider.GetComponent<StationPart>();
                if (_hover == null) _hover = hit.collider.GetComponentInParent<StationPart>();
            }
        }

        if (confirm && _hover != null) Interact(_hover);
    }

    void Interact(StationPart part)
    {
        Debug.Log($"[Virtual] Puntero → «{part.stationId}/{part.actionId}»" +
                  (string.IsNullOrEmpty(part.label) ? "." : $" ({part.label})."));
        foreach (ProductionOrder o in FindObjectsOfType<ProductionOrder>())
            o.Submit(part.stationId, part.actionId);
    }

    void OnGUI()
    {
        // GUI usa Y hacia abajo; el puntero usa Y hacia arriba → convertir.
        float gx = _pos.x, gy = Screen.height - _pos.y;
        GUI.Label(new Rect(gx - 8f, gy - 12f, 40f, 26f), _hover != null ? "◎" : "＋");
        if (_hover != null)
        {
            string txt = string.IsNullOrEmpty(_hover.label) ? $"{_hover.stationId}: {_hover.actionId}" : _hover.label;
            GUI.Label(new Rect(gx + 14f, gy - 10f, 320f, 24f), txt);
        }
    }
}
