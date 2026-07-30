using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Acción TEMPORIZADA que se acelera con MECANOGRAFÍA (docs/kitchen-simulation.md §4b). Al lanzarse (desde
/// una <see cref="StationPart"/> temporizada, p. ej. el fogón con los huevos), corre un tiempo base y
/// muestra un banco de **palabras** (verbo/ingrediente/**compuesto/nutriente**: cook, eggs, protein, B2…).
/// Teclear cada palabra correcta **recorta tiempo**; al agotarse el tiempo (o teclearlas todas) la acción
/// se completa y emite su paso a las <see cref="ProductionOrder"/>. Localizable (idioma elegible) → sirve
/// para practicar mecanografía e idiomas. Mientras corre, <see cref="Active"/> congela cámara/mira.
/// </summary>
public class TypingChallenge : MonoBehaviour
{
    [Tooltip("Tiempo base de la acción (s). La coherencia de tiempos vive aquí.")]
    public float baseTime = 8f;
    [Tooltip("Segundos que recorta cada palabra tecleada correctamente.")]
    public float reductionPerWord = 1.2f;
    [Tooltip("Idioma del banco (para practicar; en/fr/… — MVP informativo).")]
    public string language = "en";
    [Tooltip("Banco de palabras (verbo/ingrediente/compuesto/nutriente).")]
    public string[] words = { "cook", "eggs", "protein", "healthy", "b2" };

    static int _activeCount;
    /// <summary>¿Hay algún reto de mecanografía en curso? (para congelar cámara/mira).</summary>
    public static bool Active => _activeCount > 0;

    public bool Running { get; private set; }

    float _remaining;
    string _buffer = "";
    string _station, _action;
    readonly List<string> _pending = new List<string>();

    /// <summary>Lanza el reto para el paso (stationId, actionId) que emitirá al completarse.</summary>
    public void Begin(string station, string action)
    {
        if (Running) return;
        _station = station; _action = action;
        _remaining = baseTime; _buffer = "";
        _pending.Clear();
        foreach (string w in words) if (!string.IsNullOrEmpty(w)) _pending.Add(w.ToLower());
        Running = true;
        _activeCount++;
        Debug.Log($"[Typing] «{station}/{action}» arranca ({baseTime:0.0}s, {language}). Teclea: {string.Join(", ", _pending)}");
    }

    void Update()
    {
        if (!Running) return;
        _remaining -= Time.deltaTime;

        foreach (char c in Input.inputString)
        {
            if (c == '\b') { if (_buffer.Length > 0) _buffer = _buffer.Substring(0, _buffer.Length - 1); }
            else if (c == '\n' || c == '\r' || c == ' ') SubmitWord();
            else _buffer += char.ToLower(c);
        }
        if (_buffer.Length > 0 && _pending.Contains(_buffer)) SubmitWord();   // auto al calzar

        if (_remaining <= 0f) Complete();
    }

    void SubmitWord()
    {
        string w = _buffer.Trim();
        _buffer = "";
        if (string.IsNullOrEmpty(w)) return;
        if (_pending.Remove(w))
        {
            _remaining -= reductionPerWord;
            Debug.Log($"[Typing] «{w}» ✓ (−{reductionPerWord:0.0}s → {Mathf.Max(0f, _remaining):0.0}s).");
            if (_pending.Count == 0) Complete();
        }
    }

    void Complete()
    {
        if (!Running) return;
        Running = false;
        _activeCount = Mathf.Max(0, _activeCount - 1);
        Debug.Log($"[Typing] Acción «{_station}/{_action}» completada.");
        foreach (VirtualTask t in FindObjectsOfType<VirtualTask>())
            t.Submit(_station, _action);
    }

    void OnGUI()
    {
        if (!Running) return;
        var r = new Rect(Screen.width * 0.5f - 170f, Screen.height * 0.34f, 340f, 120f);
        GUI.Box(r, "Cocinando…");
        GUI.Label(new Rect(r.x + 12f, r.y + 26f, r.width - 24f, 22f), $"Tiempo: {Mathf.Max(0f, _remaining):0.0}s");
        GUI.Label(new Rect(r.x + 12f, r.y + 50f, r.width - 24f, 22f), "Teclea: " + string.Join("  ", _pending));
        GUI.Label(new Rect(r.x + 12f, r.y + 74f, r.width - 24f, 22f), "> " + _buffer);
    }
}
