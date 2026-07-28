using UnityEngine;

/// <summary>
/// Cerebro de IA que hace que el ser SIGA a un objetivo (docs/anima-architecture.md §11.5). Es el que
/// permite "liberar la mente de Kushal para que siga a su compañero por la cocina", y el primer ladrillo
/// de las acciones entre personajes (acompañar, ir juntos a un área). Sin objetivo no reclama el mando;
/// con objetivo pide una relevancia algo mayor que el idle para conducir mientras haya a quién seguir.
/// </summary>
public class FollowBrain : MonoBehaviour, IBrain
{
    [Tooltip("A quién seguir. Vacío = no sigue a nadie (no reclama el control).")]
    public Transform target;

    [Tooltip("Relevancia al tener objetivo. Ponla por encima del AiBrain idle (1) para que tome el mando.")]
    public float relevance = 1.5f;

    public float moveSpeed = 3.5f;
    public float turnSpeed = 360f;
    [Tooltip("Distancia a la que deja de acercarse (para no empujar al que sigue).")]
    public float stopDistance = 2f;

    public float Relevance => target != null ? relevance : 0f;
    public string BrainName => "IA (seguir)";

    CharacterController _cc;
    void Awake() => _cc = GetComponent<CharacterController>();

    public void Act(AnimaController ctrl)
    {
        if (target == null) return;
        Vector3 to = target.position - transform.position; to.y = 0f;
        float dist = to.magnitude;
        if (dist <= stopDistance) return;

        Vector3 dir = to / dist;
        if (_cc != null && _cc.enabled) _cc.SimpleMove(dir * moveSpeed);
        else transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
    }
}
