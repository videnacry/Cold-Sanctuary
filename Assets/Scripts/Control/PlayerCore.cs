using UnityEngine;

/// <summary>
/// El "alma"/input PERSISTENTE del jugador (docs/anima-architecture.md §11.5). No es un cuerpo: es donde
/// viven los mandos y el CAMBIO DE CUERPO (body-swap). Al cambiar de cuerpo:
///   1. LIBERA el cuerpo anterior (su IA retoma el mando),
///   2. SECUESTRA (posee) el nuevo con el <see cref="PossessionSpell"/>,
///   3. REALINEA la cámara al nuevo cuerpo.
/// El cuerpo poseído se mueve solo vía su <see cref="PlayerBrain"/> (que lee el input). Así "controlar =
/// enchufar el input en otro cuerpo", y el jugador es solo un input que salta de ánima en ánima.
/// </summary>
public class PlayerCore : MonoBehaviour
{
    [Tooltip("Hechizo con el que el jugador posee cuerpos. Si está vacío, lo busca en este objeto.")]
    public PossessionSpell spell;

    [Tooltip("Tecla para saltar al cuerpo poseíble más cercano (distinto del actual).")]
    public KeyCode swapKey = KeyCode.Tab;

    [Tooltip("Al empezar, posee automáticamente el cuerpo más cercano dentro del alcance.")]
    public bool possessNearestOnStart = true;

    [Header("Cámara (opcional)")]
    public Camera cam;
    public Vector3 camOffset = new Vector3(0f, 4f, -6f);
    public bool followCamera = true;

    void Awake()
    {
        if (spell == null) spell = GetComponent<PossessionSpell>();
        if (cam == null) cam = Camera.main;
    }

    void Start()
    {
        if (possessNearestOnStart && spell != null && spell.Current == null)
            spell.PossessNearest();
    }

    void Update()
    {
        if (Input.GetKeyDown(swapKey)) SwapToNearest();
        if (followCamera) FollowBody();
    }

    /// <summary>Salta al Anima poseíble más cercano DISTINTO del cuerpo actual (medido desde el cuerpo actual).</summary>
    public void SwapToNearest()
    {
        if (spell == null) return;
        AnimaController current = spell.Current;
        Vector3 origin = current != null ? current.transform.position : transform.position;

        AnimaController best = null;
        float bestDist = spell.range;
        foreach (AnimaController c in FindObjectsOfType<AnimaController>())
        {
            if (c == current || c.gameObject == gameObject) continue;
            float d = Vector3.Distance(origin, c.transform.position);
            if (d <= bestDist) { bestDist = d; best = c; }
        }

        if (best != null)
        {
            spell.Possess(best);   // Possess() ya libera el cuerpo anterior antes de secuestrar el nuevo
            Debug.Log($"[Jugador] Cambio de cuerpo → «{best.name}» (cámara realineada). El AnimaController " +
                      $"decide si el jugador domina (posesión > selfRelevance del ser).");
        }
        else Debug.Log("[Jugador] No hay otro cuerpo poseíble cerca al que saltar.");
    }

    void FollowBody()
    {
        AnimaController body = spell != null ? spell.Current : null;
        if (body == null || cam == null) return;
        Vector3 target = body.transform.position + body.transform.TransformDirection(camOffset);
        cam.transform.position = Vector3.Lerp(cam.transform.position, target, 0.15f);
        cam.transform.LookAt(body.transform.position + Vector3.up);
    }
}
