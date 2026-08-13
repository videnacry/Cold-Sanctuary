using UnityEngine;

/// <summary>Una ranura de MEZCLA (docs/soul-composition-blend.md): un arquetipo (cuerpo o mente) con su
/// **dominio (%)**. Si `shareDomain`, no fija un % sino que **se reparte lo que quede sin reclamar** con las
/// demás marcadas.</summary>
[System.Serializable]
public class BlendSlot
{
    [Tooltip("Nombre del arquetipo (ver Archetypes): Human/Bear/Wolf/Bunny/Lion/… (cuerpo) o Human/Bear/Rock/Fire/… (mente).")]
    public string archetype = "Human";
    [Range(0f, 100f)]
    [Tooltip("Dominio en %. Ignorado si shareDomain está marcado.")]
    public float domain = 100f;
    [Tooltip("Se reparte a partes iguales lo que quede sin reclamar (100 − suma de los explícitos).")]
    public bool shareDomain = false;
}
