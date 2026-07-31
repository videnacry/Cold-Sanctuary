using UnityEngine;

/// <summary>
/// Ficha de ALMA del mundo insecto (docs/microcosmos-insects.md §13). Marca a un ser con su
/// **nombre de alma**, el **hilo-arquetipo** que encarna en esta vida (A..G, ver mob-epochs-matrix.md),
/// su **rol en la vida 1** (alba/cueva) y el **nombre/cuerpo con que reencarna en la vida 2** (la Cocina,
/// era del fuego), más el **tell**: el indicio de vida pasada que deja **notar** el vínculo sin afirmarlo
/// (postura, manía, cuerpo…). Es solo **datos + un log** de arranque; no conduce comportamiento (de eso se
/// ocupan los <see cref="IBrain"/>). Sirve de canon en escena y de semilla para el sistema de indicios.
/// </summary>
public class SoulRecord : MonoBehaviour
{
    [Tooltip("Nombre del alma (vida 1, insecto). Ej: Ambrosio, Hespero, Medea…")]
    public string soulName;
    [Tooltip("Hilo-arquetipo que encarna en ESTA vida: A..G (o '-' para el centro sagrado sin hilo).")]
    public string hilo;
    [TextArea] public string vida1Role;   // rol en el alba/cueva
    [Tooltip("Reencarnación en la Cocina (era del fuego). Vacío = no reencarna a una figura conocida.")]
    public string vida2Name;
    [TextArea] public string tell;         // indicio de vida pasada (postura, manía, cuerpo…)

    void Start()
    {
        Debug.Log($"[Alma] {soulName} · hilo {hilo} · vida1: {vida1Role}" +
                  (string.IsNullOrEmpty(vida2Name) ? "" : $" → vida2: {vida2Name}"));
    }
}
