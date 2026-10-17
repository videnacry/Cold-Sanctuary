using UnityEngine;

/// <summary>
/// AVISO DE LECCIÓN (OnGUI): muestra abajo-centro lo que el maestro pide AHORA ("párate recto", "levanta las manos con la
/// inhalación"…) y, si el alumno es el jugador, el **hechizo a escribir** para realizarlo. Lo usa <see cref="TeachingSession"/>.
/// Andamiaje de UI (como los demás HUDs OnGUI); se auto-crea y persiste.
/// </summary>
public class LessonPrompt : MonoBehaviour
{
    static LessonPrompt _inst;
    static string _text, _spell;
    static float _until;

    public static void Show(string instruction, string spell, float secs = 12f)
    {
        Ensure();
        _text = instruction; _spell = spell; _until = Time.time + secs;
    }

    public static void Hide() { _text = null; }

    static void Ensure()
    {
        if (_inst != null) return;
        GameObject go = new GameObject("LessonPrompt_HUD");
        _inst = go.AddComponent<LessonPrompt>();
        Object.DontDestroyOnLoad(go);
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(_text) || Time.time > _until) return;
        float w = 560f, h = string.IsNullOrEmpty(_spell) ? 48f : 72f;
        var box = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 84f, w, h);   // encima de la barra de hechizos
        GUI.Box(box, GUIContent.none);
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, wordWrap = true };
        GUI.Label(new Rect(box.x + 10, box.y + 6, box.width - 20, 28f), _text, style);
        if (!string.IsNullOrEmpty(_spell))
        {
            var sstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(box.x + 10, box.y + 36, box.width - 20, 28f), $"Escribe (o Intro + Intro): {_spell}", sstyle);
        }
    }
}
