using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Un RECIPIENTE de cocina y su contenido. Las primitivas del restaurante cuyo nombre es de recipiente
/// (olla/bol/plancha/sartén/mesa/plato) lo reciben solas al primer `putIn`.</summary>
public class KitchenVessel : MonoBehaviour
{
    public List<string> contents = new List<string>();
    public bool beaten;

    static readonly string[] VesselWords = { "olla", "bol", "plancha", "sart", "mesa", "plato" };

    public static bool LooksLikeVessel(string name)
    {
        string n = name.ToLowerInvariant();
        foreach (string w in VesselWords) if (n.Contains(w)) return true;
        return false;
    }

    public static KitchenVessel Of(GameObject go)
    {
        KitchenVessel v = go.GetComponent<KitchenVessel>();
        if (v == null && LooksLikeVessel(go.name)) v = go.AddComponent<KitchenVessel>();
        return v;
    }

    public string Describe() => (beaten ? "batido de " : "") + string.Join(" + ", contents);
    public void Clear() { contents.Clear(); beaten = false; }
}
