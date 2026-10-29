using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SUBTÍTULOS DE LAS HISTORIAS (OnGUI): lo que dicen los personajes de un microcosmos, en una caja abajo, encima del
/// objetivo (LessonPrompt). Antes las historias hablaban por PlaneMessenger, que solo existe en algunas escenas: en el
/// microcosmos de Ambrosio no se veía ni su petición de ayuda ni su muerte. Se apilan hasta tres frases; cada una dura lo
/// suyo. Se auto-crea y persiste entre escenas.
/// </summary>
public class StorySubtitle : MonoBehaviour
{
    static StorySubtitle _inst;
    struct Line { public string who, text; public float until; }
    static readonly List<Line> _lines = new List<Line>();

    public static void Show(string who, string text, float secs = 7f)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (_inst == null) { var go = new GameObject("StorySubtitle_HUD"); _inst = go.AddComponent<StorySubtitle>(); DontDestroyOnLoad(go); }
        _lines.Add(new Line { who = who, text = text, until = Time.time + Mathf.Max(3f, secs) });
        while (_lines.Count > 3) _lines.RemoveAt(0);
    }

    public static void Clear() => _lines.Clear();

    /// <summary>La caja (toda la pila) de la última vez que se dibujó, y en qué frame: <see cref="KushalVoice"/> se queda encima.</summary>
    public static Rect LastRect { get; private set; }
    public static int LastDrawFrame { get; private set; } = -100;

    void OnGUI()
    {
        _lines.RemoveAll(l => Time.time > l.until);
        if (_lines.Count == 0) return;
        var style = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true, richText = true, alignment = TextAnchor.MiddleLeft };
        float w = Mathf.Min(Mathf.Max(620f, Screen.width * 0.55f), Screen.width - 40f), y = Screen.height - 230f;
        float bottom = y - 6f;
        for (int i = _lines.Count - 1; i >= 0; i--)
        {
            string t = string.IsNullOrEmpty(_lines[i].who) ? _lines[i].text : $"<b>{_lines[i].who}:</b> {_lines[i].text}";
            float h = style.CalcHeight(new GUIContent(t), w - 24f) + 14f;
            y -= h + 6f;
            var box = new Rect((Screen.width - w) * 0.5f, y, w, h);
            GUI.Box(box, GUIContent.none); GUI.Box(box, GUIContent.none);   // dos veces: más opaca
            GUI.Label(new Rect(box.x + 12f, box.y + 7f, box.width - 24f, h - 14f), t, style);
        }
        LastRect = new Rect((Screen.width - w) * 0.5f, y, w, bottom - y); LastDrawFrame = Time.frameCount;
    }
}
