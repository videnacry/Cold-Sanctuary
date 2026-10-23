using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CONVICCIONES (Berón, 2026-10-23: «lo que mueve a la Magnate a resguardar que cada personaje se mantenga en su trabajo
/// es un pensamiento; con un hechizo podría quitárselo y poner otro»). Ideas firmes de un personaje que GOBIERNAN conductas
/// concretas: una conducta solo actúa mientras su dueño tenga la convicción que la sostiene (la guardia de la Magnate
/// pregunta por «guardar_los_puestos»). Se cambian con el hechizo <c>reemplazar</c> (o se quitan): el buscador del
/// lanzador muestra las convicciones del objetivo y, para poner, las del catálogo.
/// Distinto de los pensamientos que aparecen y se van (<see cref="Mind"/>): esto es lo que el personaje da por cierto.
/// </summary>
public class Convictions : MonoBehaviour
{
    /// <summary>Catálogo: id → lo que el personaje piensa (en primera persona).</summary>
    public static readonly Dictionary<string, string> Catalog = new Dictionary<string, string>
    {
        { "guardar_los_puestos", "Durante la jornada, cada uno en su puesto: si alguien se va, lo traigo de vuelta." },
        { "confiar_en_los_puestos", "Confío en que cada uno vuelve solo a su puesto; no hace falta traerlo." },
        { "iniciar_a_los_recien_llegados", "Nadie sale de la enfermería sin terminar su iniciación." },
        { "dejar_explorar", "Los recién llegados aprenden explorando: que vayan donde quieran." },
    };

    public List<string> held = new List<string>();

    /// <summary>Las convicciones de <paramref name="g"/> (se crean con las de su papel la primera vez).</summary>
    public static Convictions Of(GameObject g)
    {
        if (g == null) return null;
        Convictions c = g.GetComponent<Convictions>();
        if (c != null) return c;
        c = g.AddComponent<Convictions>();
        if (g.GetComponent<Magnate>() != null) { c.held.Add("guardar_los_puestos"); c.held.Add("iniciar_a_los_recien_llegados"); }
        return c;
    }

    public bool Has(string id) => held.Contains(id);

    /// <summary>Quita <paramref name="remove"/> (si no es vacío) y pone <paramref name="add"/> (si no es vacío y existe).</summary>
    public string Replace(string remove, string add)
    {
        bool had = !string.IsNullOrEmpty(remove) && held.Remove(remove);
        bool put = !string.IsNullOrEmpty(add) && Catalog.ContainsKey(add) && !held.Contains(add);
        if (put) held.Add(add);
        if (!had && !put) return $"«{name}» no tiene «{remove}» y no hay nada que poner.";
        return $"«{name}» " + (had ? $"ya no piensa «{Text(remove)}»" : "") + (had && put ? "; ahora piensa " : put ? "ahora piensa " : "") + (put ? $"«{Text(add)}»" : "") + ".";
    }

    public static string Text(string id) => id != null && Catalog.TryGetValue(id, out string t) ? t : id;
}
