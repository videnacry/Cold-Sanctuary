using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo guarda en las escenas un MonoBehaviour cuyo archivo se llama como la clase; dentro de
// AreaFunctionSite.cs se perdía al guardar la escena del microcosmos.)
/// <summary>
/// UN PUESTO de un edificio (el fogón, una cama, la esterilla, el muñeco de madera…): la clave de <see cref="station"/> es la
/// que piden las funciones del registro. Es INTERACTUABLE: el jugador hace aquí las funciones del edificio (cocinar, servir,
/// pasar visita, practicar…), una tras otra cada vez que interactúa.
/// </summary>
public class AreaFunctionStation : MonoBehaviour, IInteractable
{
    [Tooltip("Clave del puesto: fogon, mesa, barra, cama, camilla, esterilla, tatami, muneco, bancal, banco_taller, torno, tunel, estante, pizarra, puesto, altar, lavadero, parapeto…")]
    public string station = "mesa";
    [System.NonSerialized] public Object occupant;   // quién lo usa ahora (la jornada no manda a dos al mismo)
    public int uses;                                    // veces que se ha hecho algo aquí (pruebas)
    int _next;

    public AreaFunctionSite Site => GetComponentInParent<AreaFunctionSite>();

    /// <summary>Las funciones del edificio que se hacen en ESTE puesto (en el año de la escena).</summary>
    public List<AreaFunctions.AreaFunction> Here()
    {
        var l = new List<AreaFunctions.AreaFunction>();
        AreaFunctionSite site = Site; if (site == null) return l;
        foreach (var f in site.Functions()) if (f.stations.Length == 0 || System.Array.IndexOf(f.stations, station) >= 0) l.Add(f);
        return l;
    }

    public string InteractLabel
    {
        get
        {
            var h = Here();
            if (h.Count == 0) return station;
            var f = h[_next % h.Count];
            return $"{f.title} ({Site?.title})";
        }
    }

    public bool CanInteract => Here().Count > 0;

    public void Interact()
    {
        var h = Here(); if (h.Count == 0) return;
        var f = h[_next++ % h.Count];
        Do(f, GameObject.FindGameObjectWithTag("Player"));
    }

    /// <summary>Hace la función <paramref name="f"/> aquí (<paramref name="who"/>: el gesto de su cuerpo).</summary>
    public void Do(AreaFunctions.AreaFunction f, GameObject who)
    {
        uses++;
        if (who != null)
        {
            CubeInsect ci = who.GetComponent<CubeInsect>();
            if (ci != null) Gesture(ci, f.kind);
            if (who.CompareTag("Player")) LessonPrompt.Show($"{AreaFunctions.Verb(f.kind)}: {f.title}{(Site != null && Site.title.Length > 0 ? $" — {Site.title}" : "")}", null, 4f);
        }
    }

    /// <summary>El gesto de cada clase de función en un cuerpo de insecto-humano.</summary>
    public static void Gesture(CubeInsect ci, AreaFunctions.Kind k)
    {
        switch (k)
        {
            case AreaFunctions.Kind.Practice: case AreaFunctions.Kind.Teach: ci.Martial(Mathf.PingPong(Time.time * 2f, 1f), 1f - Mathf.PingPong(Time.time * 2f, 1f), 0.6f, Mathf.Sin(Time.time) * 0.3f, 0.3f); break;
            case AreaFunctions.Kind.Meditate: case AreaFunctions.Kind.Worship: ci.Martial(0f, 0f, 0.45f, 0f, 0.1f); break;
            case AreaFunctions.Kind.Rest: case AreaFunctions.Kind.Dine: break;
            default: ci.Push(Mathf.PingPong(Time.time * 1.5f, 1f)); break;   // remover, servir, curar, limpiar, cultivar, trabajar en el banco
        }
    }
}
