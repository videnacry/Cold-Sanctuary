using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mensajes que cruzan entre planos (docs/area-progression.md "Apertura"): p. ej. el **Mesocosmos** avisa al
/// jugador mientras está en el **Microcosmos** ("cuando termines, ve a la sala de meditación para volver").
/// Refuerza que se puede **entrar/salir del Microcosmos** (por la `VirtualizationMachine`/`YogaPortal`)
/// incluso en mitad de misión, y mantiene al jugador orientado. Muy simple: cola de avisos con caducidad.
/// </summary>
public class PlaneMessenger : MonoBehaviour
{
    public static PlaneMessenger Instance;

    struct Msg { public string from; public string text; public float until; }
    readonly List<Msg> _active = new List<Msg>();

    void Awake() { Instance = this; }

    /// <summary>Envía un aviso de <paramref name="from"/> (p. ej. "Mesocosmos") por unos segundos.</summary>
    public static void Send(string from, string text, float seconds = 6f)
    {
        Debug.Log($"[Mensaje·{from}] {text}");
        if (Instance != null)
            Instance._active.Add(new Msg { from = from, text = text, until = Time.time + seconds });
    }

    void Update()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
            if (Time.time > _active[i].until) _active.RemoveAt(i);
    }

    void OnGUI()
    {
        float y = 40f;
        foreach (Msg m in _active)
        {
            GUI.Label(new Rect(20f, y, 620f, 22f), $"✉ {m.from}: {m.text}");
            y += 22f;
        }
    }
}
