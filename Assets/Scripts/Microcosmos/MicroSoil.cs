using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>
/// LA CAPA DE TIERRA de un microcosmos (<see cref="MicroGround"/>): la pone <c>MicroSoilBuilder</c> bajo la raíz («Suelo_de_tierra»),
/// en mosaicos (una malla combinada por mosaico, con sus piedras dentro, y su MeshCollider) y en placas sobre las caras de arriba de
/// los muebles. Lo leen las pruebas.
/// </summary>
public class MicroSoil : MonoBehaviour
{
    [Tooltip("Grosor de la capa (m del microcosmos).")]
    public float thickness = MicroGround.SoilThickness;
    [Tooltip("Mosaicos de tierra sobre el suelo del piso y placas sobre los muebles (una malla combinada cada uno).")]
    public int tiles, plates;
    [Tooltip("Colliders LISOS de la tierra (sin ruido ni piedras: los hornea el NavMesh).")]
    public int colliders;
    [Tooltip("Cúmulos y piedras repartidos.")]
    public int mounds, stones;
    [Tooltip("Edificios y tramos de calle del pueblo levantados para quedar sobre la tierra.")]
    public int raised;
}
