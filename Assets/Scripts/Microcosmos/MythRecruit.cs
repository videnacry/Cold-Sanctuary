using UnityEngine;

/// <summary>
/// Un insecto-humano de EGINA que aún no se ha unido al reino de Éaco (docs/microcosmos-myrmidons.md). Zeus (un
/// <see cref="MythRecruiter"/>) o el jugador (E) le hablan; si se cumple su condición (algunos esperan a ver que la mitad
/// ya trabaja, o que se alzan las columnas), SE UNE y se pone a su tarea: cargar piedra de la cantera al templo
/// (<see cref="MicroForager"/>), o ir a su puesto (el hogar común, las terrazas, la mesa de las leyes, la stoa).
/// </summary>
public class MythRecruit : MonoBehaviour
{
    public enum Role { Carrier, Hearth, Terraces, Scribe, Elder }

    public string displayName = "";
    public Role role = Role.Carrier;
    [Tooltip("Su puesto si no carga piedra.")]
    public Transform workSpot;
    [Range(0f, 1f), Tooltip("Se une solo si ya se ha unido esta fracción del pueblo.")]
    public float joinsAtFraction;
    [Tooltip("Se une solo cuando el templo llega a esta etapa (0 = desde el principio).")]
    public int joinsAtStage;
    [TextArea] public string refusal = "Aún no lo veo claro.";
    [TextArea] public string acceptance = "Cuenta conmigo.";

    public bool Joined { get; private set; }

    public bool CanJoin(float joinedFraction, int stage) => !Joined && joinedFraction >= joinsAtFraction && stage >= joinsAtStage;

    public void Join()
    {
        if (Joined) return;
        Joined = true;
        if (role == Role.Carrier) { MicroForager f = GetComponent<MicroForager>(); if (f != null) f.enabled = true; }
    }

    void Update()
    {
        if (!Joined || role == Role.Carrier || workSpot == null) return;
        MicroMove.Step(transform, workSpot.position, 2.5f, 1.5f);
    }
}
