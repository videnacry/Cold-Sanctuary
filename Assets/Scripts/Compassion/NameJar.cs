using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS FRASCOS BAJO EL MANZANO: Irena Sendler escribía en papel de seda el nombre real de cada niño que sacaba del gueto
/// y su nueva identidad, y enterraba las listas en frascos bajo un manzano del jardín de una compañera, para poder
/// devolverlos a sus familias al acabar la guerra. Aquí cada niño puesto a salvo deja su nombre en el frasco.
/// </summary>
public class NameJar : MonoBehaviour
{
    public readonly List<string> names = new List<string>();
    public static NameJar Instance { get; private set; }
    void OnEnable() => Instance = this;
    void OnDisable() { if (Instance == this) Instance = null; }

    public static void Record(string name)
    {
        if (Instance == null || string.IsNullOrEmpty(name) || Instance.names.Contains(name)) return;
        Instance.names.Add(name);
        Debug.Log($"[Compasión] Frasco bajo el manzano: «{name}» ({Instance.names.Count}).");
    }
}
