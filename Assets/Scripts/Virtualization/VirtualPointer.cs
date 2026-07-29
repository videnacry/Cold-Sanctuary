using UnityEngine;

/// <summary>
/// Puntero de virtualización (docs/kitchen-simulation.md §3b). Es la vía de **teclado**: una mira en
/// pantalla (arranca arriba-centro) que se mueve con las **teclas de cámara I/K/J/L** (mecanografía; los
/// mismos defaults que `PlayerController`) y se confirma con la **tecla de interacción F** (la misma del
/// juego; Espacio es salto). El **ratón** y el **touch** son su **propio puntero**: NO mueven la mira de
/// teclado — su clic/toque interactúa directamente donde apuntan. Así el juego se puede jugar **solo con
/// teclado** o con ratón/touch, cada uno con su cursor. Todas las teclas son configurables (Inspector).
/// Lanza un rayo desde la cámara, resalta la <see cref="StationPart"/> apuntada y, al confirmar, emite su
/// paso a las <see cref="ProductionOrder"/>.
/// </summary>
public class VirtualPointer : MonoBehaviour
{
    public Camera cam;

    [Header("Teclado (mueve la mira) — mismas teclas que la cámara")]
    public KeyCode up = KeyCode.I;
    public KeyCode down = KeyCode.K;
    public KeyCode left = KeyCode.J;
    public KeyCode right = KeyCode.L;
    public float keyboardSpeed = 900f;   // px/s
    [Tooltip("Tecla de interacción (la del juego: F). Espacio NO — es salto.")]
    public KeyCode confirmKey = KeyCode.F;

    [Header("Ratón y touch (su PROPIO cursor; no mueven la mira de teclado)")]
    public bool mouseInteract = true;
    public bool touchInteract = true;

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

        // ── Mira de TECLADO (solo el teclado la mueve) ──────────────────────────────────────────────
        Vector2 d = Vector2.zero;
        if (Input.GetKey(up)) d.y += 1f;
        if (Input.GetKey(down)) d.y -= 1f;
        if (Input.GetKey(right)) d.x += 1f;
        if (Input.GetKey(left)) d.x -= 1f;
        if (d.sqrMagnitude > 0f) _pos += d.normalized * keyboardSpeed * Time.deltaTime;
        _pos.x = Mathf.Clamp(_pos.x, 0f, Screen.width);
        _pos.y = Mathf.Clamp(_pos.y, 0f, Screen.height);

        // Resaltado de lo apuntado por la mira de teclado.
        StationPart h = RaycastPart(_pos);
        if (h != _hover)
        {
            if (_hover != null) _hover.SetHighlighted(false);
            _hover = h;
            if (_hover != null) _hover.SetHighlighted(true);
        }
        if (Input.GetKeyDown(confirmKey) && _hover != null) Interact(_hover);

        // ── RATÓN: su propio cursor (clic izq. interactúa donde apunta) ─────────────────────────────
        if (mouseInteract && Input.mousePresent && Input.GetMouseButtonDown(0))
        {
            StationPart p = RaycastPart(Input.mousePosition);
            if (p != null) Interact(p);
        }

        // ── TOUCH: su propio cursor (el toque interactúa donde toca) ────────────────────────────────
        if (touchInteract && Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                StationPart p = RaycastPart(t.position);
                if (p != null) Interact(p);
            }
        }
    }

    StationPart RaycastPart(Vector2 screenPos)
    {
        if (cam == null) return null;
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            StationPart sp = hit.collider.GetComponent<StationPart>();
            if (sp == null) sp = hit.collider.GetComponentInParent<StationPart>();
            return sp;
        }
        return null;
    }

    void Interact(StationPart part)
    {
        Debug.Log($"[Virtual] → «{part.stationId}/{part.actionId}»" +
                  (string.IsNullOrEmpty(part.label) ? "." : $" ({part.label})."));
        foreach (ProductionOrder o in FindObjectsOfType<ProductionOrder>())
            o.Submit(part.stationId, part.actionId);
    }

    void OnGUI()
    {
        // GUI usa Y hacia abajo; la mira usa Y hacia arriba → convertir.
        float gx = _pos.x, gy = Screen.height - _pos.y;
        GUI.Label(new Rect(gx - 8f, gy - 12f, 40f, 26f), _hover != null ? "◎" : "＋");
        if (_hover != null)
        {
            string txt = string.IsNullOrEmpty(_hover.label) ? $"{_hover.stationId}: {_hover.actionId}" : _hover.label;
            GUI.Label(new Rect(gx + 14f, gy - 10f, 320f, 24f), txt);
        }
    }
}
