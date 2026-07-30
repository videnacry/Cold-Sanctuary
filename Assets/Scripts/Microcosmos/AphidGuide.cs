using UnityEngine;

/// <summary>
/// La MASCOTA-GUÍA de la 1ª misión del Microcosmos (docs/microcosmos-insects.md §4). El pulgón **no quiere
/// dejar a su familia** (a la que la colonia abandonó por débil): **guía** hacia ella (fase 0) y, al llegar,
/// **rescata** a los <see cref="WeakOne"/> caídos cerca (les inserta un `FollowBrain` hacia el refugio) y
/// luego encabeza el camino al **refugio/hormiguero** (fase 1). Cierra con `CarryToRefuge` (el nido cuenta
/// a los rescatados). Movimiento propio por `Translate` (no necesita `AnimaController`).
/// </summary>
public class AphidGuide : MonoBehaviour
{
    [Tooltip("Dónde está la familia caída.")]
    public Transform familyPoint;
    [Tooltip("El refugio/hormiguero a donde llevarlos.")]
    public Transform refuge;
    public float speed = 2.5f;
    public float arrive = 1.5f;
    public float rescueRadius = 3f;

    int _phase;      // 0 = ir a la familia; 1 = ir al refugio
    bool _rescued;

    void Update()
    {
        Transform goal = _phase == 0 ? familyPoint : refuge;
        if (goal == null) return;

        Vector3 to = goal.position - transform.position; to.y = 0f;
        float d = to.magnitude;
        if (d > arrive)
        {
            transform.Translate(to.normalized * speed * Time.deltaTime, Space.World);
            return;
        }

        if (_phase == 0)
        {
            if (!_rescued) { RescueNearby(); _rescued = true; }
            Debug.Log("[Micro] El pulgón guía a la familia rescatada hacia el refugio.");
            _phase = 1;
        }
    }

    void RescueNearby()
    {
        foreach (WeakOne w in FindObjectsOfType<WeakOne>())
        {
            if (w.gameObject == gameObject) continue;
            if ((w.transform.position - transform.position).sqrMagnitude > rescueRadius * rescueRadius) continue;

            FollowBrain fb = w.GetComponent<FollowBrain>();
            if (fb == null) fb = w.gameObject.AddComponent<FollowBrain>();
            fb.target = refuge; fb.relevance = 3f; fb.stopDistance = 1.5f;
            AnimaController ac = w.GetComponent<AnimaController>();
            if (ac != null) ac.RefreshBrains();
            Debug.Log($"[Micro] «{w.name}» rescatado: ahora sigue al refugio.");
        }
    }
}
