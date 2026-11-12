using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo guarda en las escenas un MonoBehaviour cuyo archivo se llama como la clase; dentro de
// InsectGenealogy.cs se perdía al guardar la escena del microcosmos.)
/// <summary>El LINAJE de un insecto-humano: su especie y sus antepasados (<see cref="InsectGenealogy"/>).</summary>
public class FolkLineage : MonoBehaviour
{
    public string species = "Ant";
    public bool female = true;
    public List<InsectGenealogy.Ancestor> ancestors = new List<InsectGenealogy.Ancestor>();
    [Tooltip("Su pareja (si tiene): con ella tiene hijos (FolkCouple).")]
    public FolkLineage partner;
    public int children;
}
