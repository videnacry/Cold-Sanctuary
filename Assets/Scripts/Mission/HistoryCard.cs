using UnityEngine;

/// <summary>
/// TARJETA DE HISTORIA de las simulaciones históricas y míticas del microcosmos (compasión, mirmidones…): una INTRO al
/// entrar (qué pasa y qué hacer) y, al cumplir la misión, LO QUE PASÓ DE VERDAD con sus FUENTES. Mientras está abierta,
/// la misión no lee teclas (<see cref="IsOpen"/>). Intro/E cierra la intro; Intro/Esc, la tarjeta final.
/// </summary>
public class HistoryCard : MonoBehaviour
{
    public string title = "";
    [TextArea] public string intro = "";
    [TextArea(4, 20)] public string[] history = new string[0];
    public string[] sources = new string[0];
    /// <summary>Línea extra en la tarjeta final (p. ej. los nombres del frasco de Irena).</summary>
    public System.Func<string> extra;

    bool _intro = true, _final;
    int _closedFrame = -1;   // la tecla que la cierra no la lee también la misión en el mismo frame
    public bool IsOpen => _intro || _final || Time.frameCount == _closedFrame;

    void Start() { if (string.IsNullOrEmpty(intro)) _intro = false; }

    public void ShowFinal() { _final = true; }

    void Update()
    {
        if (_intro) { if (GameKeys.GetKeyDown(KeyCode.Return) || GameKeys.GetKeyDown(KeyCode.E)) { _intro = false; _closedFrame = Time.frameCount; } return; }
        if (_final && (GameKeys.GetKeyDown(KeyCode.Return) || GameKeys.GetKeyDown(KeyCode.Escape))) { _final = false; _closedFrame = Time.frameCount; }
    }

    void OnGUI()
    {
        if (!_intro && !_final) return;
        GUIStyle wrap = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 15 };
        Rect r = new Rect(Screen.width * 0.15f, Screen.height * 0.1f, Screen.width * 0.7f, Screen.height * 0.8f);
        GUI.Box(r, "");
        GUILayout.BeginArea(new Rect(r.x + 20f, r.y + 16f, r.width - 40f, r.height - 32f));
        GUILayout.Label($"<b>{title}</b>", new GUIStyle(wrap) { fontSize = 20, richText = true });
        if (_intro) GUILayout.Label(intro, wrap);
        else
        {
            foreach (string h in history) GUILayout.Label(h, wrap);
            string x = extra?.Invoke();
            if (!string.IsNullOrEmpty(x)) GUILayout.Label(x, wrap);
            GUILayout.Space(8f);
            GUILayout.Label("Fuentes:", wrap);
            foreach (string s in sources) GUILayout.Label("· " + s, new GUIStyle(wrap) { fontSize = 12 });
        }
        GUILayout.FlexibleSpace();
        GUILayout.Label(_intro ? "Intro / E: empezar" : "Intro: cerrar", wrap);
        GUILayout.EndArea();
    }
}
