using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Misión de CUIDADO: llevar a los <see cref="WeakOne"/> (débiles) a un **refugio** (la cueva) — antesala de
/// las misiones de cría (docs/area-progression.md "Apertura"). Cuenta los débiles que llegan cerca del
/// refugio (por distancia, sin depender de rigidbodies); al reunir a <see cref="needed"/>, completa y dispara
/// <see cref="onComplete"/> (que el prólogo engancha a su siguiente beat y a un aviso del Mesocosmos).
/// </summary>
public class CarryToRefuge : MonoBehaviour
{
    [Tooltip("Radio del refugio: un débil dentro cuenta como puesto a salvo.")]
    public float radius = 3f;
    [Min(1)] public int needed = 3;

    public UnityEvent onComplete;

    int _safe;
    float _nextPoll;
    bool _done;

    public int Safe => _safe;
    public bool Done => _done;

    void Update()
    {
        if (_done || Time.time < _nextPoll) return;
        _nextPoll = Time.time + 0.5f;

        foreach (WeakOne w in FindObjectsOfType<WeakOne>())
        {
            if (w == null || w.safe) continue;
            if ((w.transform.position - transform.position).sqrMagnitude <= radius * radius)
            {
                w.safe = true;
                _safe++;
                Debug.Log($"[Cuidado] «{w.name}» a salvo en el refugio ({_safe}/{needed}).");
                if (_safe >= needed)
                {
                    _done = true;
                    Debug.Log("[Cuidado] Todos los débiles a salvo en la cueva.");
                    onComplete?.Invoke();
                    return;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.7f, 0.6f, 0.4f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
