using System;

/// <summary>
/// How an avatar moves — maps to the access plane it unlocks (docs §4, Eje A).
/// </summary>
public enum AvatarLocomotion
{
    Ground, // gusano — suelo (y rampas suaves); no trepa
    Climb,  // araña — suelo + paredes + techo
    Flight  // mosco — aire
}

/// <summary>
/// One avatar-robot the player can pilot inside the Microcosmos (docs §4).
/// Progresión: gusano → araña → mosco → … (y al llegar al loto, el avatar pasa a ser elección
/// estética). Se desbloquean con stats/hechizos; aquí sólo se registra `unlocked`.
/// Serializable para configurarlos en el Inspector del AvatarController.
/// </summary>
[Serializable]
public class RobotAvatar
{
    public string name = "Gusano";

    [Tooltip("Desbloqueado por progresión (stats/hechizo). Si false, no es seleccionable.")]
    public bool unlocked = true;

    public AvatarLocomotion locomotion = AvatarLocomotion.Ground;

    public float moveSpeed = 4f;
    public float turnSpeed = 120f;

    [Tooltip("Escala uniforme del cuerpo del avatar (gusano pequeño, etc.).")]
    public float bodyScale = 1f;
}
