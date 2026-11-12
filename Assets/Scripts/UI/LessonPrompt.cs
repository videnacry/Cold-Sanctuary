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

    /// <summary>La caja de la última vez que se dibujó, y en qué frame: <see cref="KushalVoice"/> se queda encima.</summary>
    public static Rect LastRect { get; private set; }
    public static int LastDrawFrame { get; private set; } = -100;

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
        // La caja crece con el texto (los diálogos largos del examen se cortaban en una caja de alto fijo) y, si hace falta,
        // también a lo ancho, hasta casi toda la pantalla.
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, wordWrap = true };
        float w = Mathf.Min(Mathf.Max(560f, Screen.width * 0.5f), Screen.width - 40f);
        float th = Mathf.Max(28f, style.CalcHeight(new GUIContent(_text), w - 20f));
        float sh = string.IsNullOrEmpty(_spell) ? 0f : 28f;
        float h = th + sh + 16f;
        var box = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 84f, w, h);   // encima de la barra de hechizos
        GUI.Box(box, GUIContent.none);
        LastRect = box; LastDrawFrame = Time.frameCount;
        GUI.Label(new Rect(box.x + 10, box.y + 8, box.width - 20, th), _text, style);
        if (!string.IsNullOrEmpty(_spell))
        {
            var sstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(box.x + 10, box.y + 8 + th, box.width - 20, sh), $"Escribe (o Intro + Intro): {_spell}", sstyle);
        }
    }
}
