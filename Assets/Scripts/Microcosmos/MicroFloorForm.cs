using System.Collections.Generic;
using System.Text;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>
/// LA FORMA DE MICROCOSMOS DE UN PISO DEL SANTUARIO («Forma_microcosmos», oculta bajo el área; FloorForms.microcosm): la lista de sus
/// elementos con su versión de MESOCOSMOS (lo que es en el santuario) y su versión de MICROCOSMOS (lo que será en los microcosmos que
/// se montan sobre él), su firma, y los microcosmos que dependen de él. La prepara SampleSceneBuilder.EnsureFloorForms.
/// </summary>
public class MicroFloorForm : MonoBehaviour
{
    [Tooltip("Cada pieza del piso: «clave · elemento · versión de mesocosmos → versión de microcosmos · ruta».")]
    public string[] elements = new string[0];
    [Tooltip("Las claves del inventario con su número y su firma (MicroFloorElements.Signature).")]
    public string[] keys = new string[0];
    public string signature = "";
    [Tooltip("Los microcosmos que se montan sobre este piso (MicroFloors.Table): los que hay que revisar si el piso cambia.")]
    public string[] dependents = new string[0];
    [Tooltip("Piezas sin fila en el catálogo (van como peñascos): para darles la suya.")]
    public string[] unmapped = new string[0];

    /// <summary>Rellena la forma con el inventario actual de <paramref name="area"/>.</summary>
    public void Fill(Transform area)
    {
        var inv = MicroFloorElements.Inventory(area);
        var l = new List<string>(); var un = new List<string>();
        foreach (MicroFloorElements.Piece p in inv)
        {
            MicroFloorElements.Element e = p.element ?? MicroFloorElements.Other;
            l.Add($"{p.key} · {e.id} · {e.meso} → {e.micro}{(string.IsNullOrEmpty(e.microTown) || e.microTown == e.micro ? "" : $" / desde las ciudades: {e.microTown}")} · {p.path}");
            if (p.element == null && !un.Contains(p.key)) un.Add(p.key);
        }
        elements = l.ToArray(); unmapped = un.ToArray();
        keys = MicroFloorElements.Keys(inv); signature = MicroFloorElements.Signature(keys);
        dependents = MicroFloorElements.DependentsOf(area.name).ToArray();
    }
}
