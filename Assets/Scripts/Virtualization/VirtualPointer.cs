using UnityEngine;

/// <summary>
/// Mira de virtualización (docs/kitchen-simulation.md §3b). Es una **retícula FIJA en el centro de la
/// cámara** — nunca se mueve en pantalla: lo que se mueve es la **cámara/cabeza** (con
/// <see cref="HeadLook"/>, con restricciones de giro), y apuntas girando la cabeza. Se **confirma con F**
/// (la tecla de interacción del juego; Espacio es salto). El **ratón** y el **touch** conservan su **propio
/// cursor**: su clic/toque interactúa donde apuntan (no afectan la retícula central). Resalta la
/// <see cref="StationPart"/> apuntada; al confirmar emite su paso a las <see cref="ProductionOrder"/> —o,
/// si la parte es temporizada (<see cref="StationPart.timed"/>), lanza su reto de mecanografía.
/// </summary>
public class VirtualPointer : MonoBehaviour
{
    public Camera cam;
    [Tooltip("Tecla de interacción (la del juego: F). Espacio NO — es salto.")]
    public KeyCode confirmKey = KeyCode.F;
    [Header("Ratón y touch (su propio cursor)")]
    public bool mouseInteract = true;
    public bool touchInteract = true;
    public float rayDistance = 100f;

    StationPart _hover;

    void Awake() { if (cam == null) cam = Camera.main; }

    void Update()
    {
        // Retícula CENTRAL: siempre el centro de la cámara.
        StationPart center = RaycastPart(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        if (center != _hover)
        {
            if (_hover != null) _hover.SetHighlighted(false);
            _hover = center;
            if (_hover != null) _hover.SetHighlighted(true);
        }

        if (TypingChallenge.Active) return;   // mientras se teclea, no se interactúa ni se apunta

        // Teclado: confirmar sobre lo que esté en el centro.
        if (Input.GetKeyDown(confirmKey) && _hover != null) Interact(_hover);

        // Ratón: su propio cursor (clic izq. interactúa donde apunta).
        if (mouseInteract && Input.mousePresent && Input.GetMouseButtonDown(0))
        {
            StationPart p = RaycastPart(Input.mousePosition);
            if (p != null) Interact(p);
        }

        // Touch: su propio cursor (el toque interactúa donde toca).
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
        // Parte temporizada → reto de mecanografía; parte simple → emite el paso ya.
        if (part.timed != null)
        {
            if (!part.timed.Running) part.timed.Begin(part.stationId, part.actionId);
            return;
        }
        Debug.Log($"[Virtual] → «{part.stationId}/{part.actionId}»" +
                  (string.IsNullOrEmpty(part.label) ? "." : $" ({part.label})."));
        foreach (VirtualTask t in FindObjectsOfType<VirtualTask>())
            t.Submit(part.stationId, part.actionId);
    }

    void OnGUI()
    {
        // Retícula fija en el centro de la pantalla.
        float cx = Screen.width * 0.5f, cy = Screen.height * 0.5f;
        GUI.Label(new Rect(cx - 8f, cy - 12f, 40f, 26f), _hover != null ? "◎" : "＋");
        if (_hover != null && !TypingChallenge.Active)
        {
            string txt = string.IsNullOrEmpty(_hover.label) ? $"{_hover.stationId}: {_hover.actionId}" : _hover.label;
            GUI.Label(new Rect(cx + 14f, cy - 10f, 320f, 24f), txt);
        }
    }
}
