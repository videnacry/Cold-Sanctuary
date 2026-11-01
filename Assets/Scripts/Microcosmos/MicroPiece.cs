using UnityEngine;

/// <summary>
/// UN MUEBLE DE MICROCOSMOS (docs/microcosmos-reglas-edificacion.md): la raíz de un mueble hecho de varias piezas (la cocina con
/// su sartén, su olla y su cuchillo; la mesa con sus sillas; la cama; el retrete…). Lo pone <c>MicroFurniture</c> y lo lee
/// <see cref="MicroBuildRules.Validate"/>: un mueble entero (con lo que lleva encima o pegado: sus hijos) guarda el paso
/// (<see cref="MicroBuildRules.Passage"/>) con cualquier otro, y su <see cref="need"/> dice qué necesidad básica cubre.
/// </summary>
public class MicroPiece : MonoBehaviour
{
    [Tooltip("Qué mueble es: cocina, mesa, silla, cama, inodoro, lavabo, estante, tatami…")]
    public string kind = "mueble";
    [Tooltip("La necesidad básica que cubre (MicroBuildRules.Needs): dormir, aseo, lavarse, comer, sentarse, cocinar; vacío si ninguna.")]
    public string need = "";
    [Tooltip("Se pisa (alfombra, tatami, bancal, esterilla): no cuenta para el paso entre muebles.")]
    public bool walkable;
}
