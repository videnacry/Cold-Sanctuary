using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PISTA DE ATERRIZAJE DE ALFOMBRAS VOLADORAS (docs/transporte-alfombras.md). Hay UNA por cada grupo de plataformas
/// unidas por puentes (el restaurante y la enfermería, la sala de yoga y el laboratorio…, y la arboleda residencial):
/// es como una sala de yoga con esterillas más grandes de lo normal, y esas esterillas son ALFOMBRAS que vuelan.
/// Ir de un grupo a otro en alfombra es mucho más rápido que bajar la escalera y cruzar tierra firme.
/// </summary>
public class CarpetPad : MonoBehaviour
{
    public static readonly List<CarpetPad> All = new List<CarpetPad>();

    [Tooltip("Nombre que se ve en el diálogo de destinos (las áreas del grupo).")]
    public string displayName = "Pista";
    [Tooltip("Centros de las áreas del grupo (para saber a qué pista va cada destino).")]
    public Vector3[] members = new Vector3[0];
    [Tooltip("Grupo de plataformas unidas por puentes: entre pistas del mismo grupo se va andando, no en alfombra.")]
    public int group;
    [Tooltip("Pista BAJO un edificio (salida de abajo): hacia dónde salen las alfombras antes de subir (mundo; cero = cielo abierto).")]
    public Vector3 exitDir;
    [Tooltip("Huecos de aterrizaje (dónde se posa cada alfombra), en coordenadas locales.")]
    public Vector3[] slots = new Vector3[0];

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    /// <summary>Un hueco libre para que aterrice una alfombra (mundo).</summary>
    public Vector3 FreeSlot(FlyingCarpet except = null)
    {
        foreach (Vector3 s in slots)
        {
            Vector3 w = transform.TransformPoint(s);
            bool taken = false;
            foreach (FlyingCarpet c in FlyingCarpet.All)
                if (c != except && !c.Flying && (c.transform.position - w).sqrMagnitude < 1.5f) { taken = true; break; }
            if (!taken) return w;
        }
        return transform.position + Vector3.up * 0.1f;
    }

    /// <summary>Una alfombra posada aquí y libre (la más cercana a <paramref name="near"/>).</summary>
    public FlyingCarpet IdleCarpet(Vector3 near)
    {
        FlyingCarpet best = null; float bd = float.MaxValue;
        foreach (FlyingCarpet c in FlyingCarpet.All)
        {
            if (c.Flying || c.pad != this) continue;
            float d = (c.transform.position - near).sqrMagnitude;
            if (d < bd) { bd = d; best = c; }
        }
        return best;
    }

    /// <summary>La pista del grupo de áreas más cercano a <paramref name="p"/>.</summary>
    public static CarpetPad For(Vector3 p)
    {
        CarpetPad best = null; float bd = float.MaxValue;
        foreach (CarpetPad pad in All)
        {
            float d = Flat(pad.transform.position - p);
            foreach (Vector3 m in pad.members) d = Mathf.Min(d, Flat(m - p));
            if (d < bd) { bd = d; best = pad; }
        }
        return best;
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude; }

    /// <summary>
    /// Ruta de un trabajador (IA) hacia <paramref name="to"/>: si el destino está en OTRO grupo de plataformas, primero
    /// va a la pista (cambia <paramref name="to"/>) y allí se sube a una alfombra. true = vuela ahora (no hay que moverlo).
    /// </summary>
    public static bool Route(GameObject g, ref Vector3 to)
    {
        if (FlyingCarpet.Riding(g)) return true;
        CarpetPad a = For(g.transform.position), b = For(to);
        if (a == null || b == null || a == b || a.group == b.group) return false;   // mismo grupo: andando por los puentes
        if ((g.transform.position - a.transform.position).sqrMagnitude > 36f) { to = a.transform.position; return false; }   // andar (y subir) a la pista: puede estar encima de un área
        FlyingCarpet c = a.IdleCarpet(g.transform.position);
        if (c == null) return false;
        c.Fly(g, b);
        return true;
    }
}
