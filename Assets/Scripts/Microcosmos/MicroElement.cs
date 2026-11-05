using System.Collections.Generic;
using System.Text;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>
/// UN ELEMENTO DEL PISO EN SU VERSIÓN DE MICROCOSMOS (bajo «Forma_microcosmos» de la raíz del microcosmos): de qué pieza del piso sale
/// (su clave y su ruta en el piso del santuario), qué elemento del catálogo es y qué es en el microcosmos. Lo pone
/// <c>MicroFloorDress</c>; lo leen la prueba de sincronía y las fotos.
/// </summary>
public class MicroElement : MonoBehaviour
{
    public string key = "", element = "", micro = "", mesoPath = "";
    [Tooltip("Lo que la historia cambió de esta pieza (quitar, apilar, mover, como otro elemento); vacío si nada.")]
    public string storyOverride = "";
    [Tooltip("La pieza quedó fuera del piso (se recortó con el borde): está inventariada pero no se viste.")]
    public bool outside;
}
