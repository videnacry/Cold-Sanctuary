using UnityEngine;

/// <summary>
/// El cerebro por defecto de un ser: su propia IA. Su relevancia = cuánto se "pertenece" a sí mismo; se
/// puede subir para seres poderosos, de modo que RESISTAN más la posesión (docs/anima-architecture.md
/// §11.5: "para dominar a alguien, la relevancia debe superar la del ser"). En este MVP no mueve nada por
/// sí solo — el pilar `Mind` ya piensa y la locomoción autónoma vive en `WorldCharacter`/rutina; aquí
/// queda el hueco para engancharla cuando se unifique el movimiento IA.
/// </summary>
public class AiBrain : MonoBehaviour, IBrain
{
    [Tooltip("Relevancia base con la que el ser se reclama a sí mismo. La posesión debe superarla para " +
             "tomar el mando. Súbela en seres poderosos (jefes) para que sean más difíciles de poseer.")]
    public float selfRelevance = 1f;

    [Tooltip("Destino de locomoción de la IA (opcional). Si hay WalkSpell + destino, la IA camina hacia él POR " +
             "el hechizo (misma locomoción universal que el jugador). Vacío = no se mueve (lo mueve otra rutina).")]
    public Transform moveTarget;
    [Tooltip("Distancia a la que se considera 'llegado' y deja de andar.")]
    [Min(0.05f)] public float arriveRadius = 0.5f;

    public float Relevance => selfRelevance;
    public string BrainName => "IA";

    WalkSpell _walk;

    void Awake() => _walk = GetComponent<WalkSpell>();

    public void Act(AnimaController ctrl)
    {
        // Locomoción IA UNIFICADA: el mismo hechizo de andar que usa el jugador, conducido hacia el destino.
        if (_walk != null && moveTarget != null)
        {
            Vector3 to = moveTarget.position - transform.position; to.y = 0f;
            if (to.sqrMagnitude > arriveRadius * arriveRadius) _walk.Drive(to);   // camina hacia el destino
        }
        // (El pilar Mind sigue pensando por su cuenta; aquí solo va la locomoción.)
    }
}
