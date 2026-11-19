using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS AVISOS (CodeShip misiones.md «aviso dorado arriba (toca para abrir el registro)» y ux-acciones.md §1.8: forma y color). Uno a la vez,
/// arriba en el centro, debajo de la barra del viaje: «✓ Misión hecha…», «✗ Demasiados fallos…», «★ Nuevo objetivo…», «Has llegado…». Tocarlo
/// abre lo suyo (la lista de Tareas por defecto). Se ve unos segundos y deja paso al siguiente; los últimos quedan en la ventana de
/// misiones (<see cref="Recent"/>). Vigila también la historia: cuando cambia de paso, avisa del objetivo nuevo.
/// </summary>
public class HudNotices : MonoBehaviour
{
    public enum Kind { Info, Good, Bad, Goal }

    class Notice { public string text; public Kind kind; public System.Action tap; public float seconds; }

    static HudNotices _inst;
    static readonly Queue<Notice> _queue = new Queue<Notice>();
    /// <summary>Los últimos avisos (el más nuevo al final), para la ventana de misiones y las pruebas.</summary>
    public static readonly List<string> Recent = new List<string>();
    Notice _now; float _until;
    StoryMission _story; int _storyStep = -1;
    GUIStyle _st;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (_inst != null) return;
        var go = new GameObject("HudNotices");
        DontDestroyOnLoad(go);
        _inst = go.AddComponent<HudNotices>();
    }

    /// <summary>Pone un aviso en la cola (si es el mismo que el último, no lo repite). <paramref name="tap"/>: lo que abre al tocarlo.</summary>
    public static void Push(string text, Kind kind = Kind.Info, System.Action tap = null)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (Recent.Count > 0 && Recent[Recent.Count - 1] == text) return;
        Recent.Add(text); if (Recent.Count > 8) Recent.RemoveAt(0);
        if (_queue.Count >= 4) _queue.Dequeue();
        _queue.Enqueue(new Notice { text = text, kind = kind, tap = tap, seconds = kind == Kind.Goal ? 6f : 4f });
        Debug.Log($"[Aviso] {text}");
    }

    void Update()
    {
        // La historia cambia de paso: el objetivo nuevo.
        StoryMission sm = StoryMission.Instance;
        if (sm != _story) { _story = sm; _storyStep = sm != null ? sm.Step : -1; }
        else if (sm != null && sm.Step != _storyStep)
        {
            _storyStep = sm.Step;
            if (sm.Finished) Push($"✓ Historia terminada: {sm.title}", Kind.Good);
            else if (sm.CurrentStep != null) Push($"★ Nuevo objetivo: {sm.CurrentStep.goal}", Kind.Goal, () => HudModals.Toggle(HudModals.Modal.Tareas));
        }
        if (_now != null && Time.unscaledTime < _until) return;
        _now = _queue.Count > 0 ? _queue.Dequeue() : null;
        if (_now != null) _until = Time.unscaledTime + _now.seconds;
    }

    void OnGUI()
    {
        CsTheme.Use();   // (2026-11-19) el tema de CodeShip para los menús (UI/CsTheme.cs)
        if (_now == null || ColdSanctuary.TestRunner.Running || CodeChallengeSheet.IsOpen) return;
        if (_st == null) _st = new GUIStyle(GUI.skin.button) { richText = true, wordWrap = true, fontSize = WebLigera.Active ? 15 : 13, alignment = TextAnchor.MiddleCenter };
        float u = WebLigera.Active ? Mathf.Max(44f, Mathf.Min(Screen.width, Screen.height) * 0.07f) : 30f;
        float w = Mathf.Min(520f, Screen.width - 16f);
        float y = AutoTravel.BarTop + (AutoTravel.Instance != null && AutoTravel.Instance.BarShowing ? u + 20f : 0f);
        var r = new Rect((Screen.width - w) * 0.5f, y, w, u + 8f);
        Color c = GUI.backgroundColor;
        GUI.backgroundColor = _now.kind == Kind.Good ? new Color(0.35f, 0.8f, 0.45f) : _now.kind == Kind.Bad ? new Color(0.95f, 0.4f, 0.35f) : _now.kind == Kind.Goal ? new Color(1f, 0.8f, 0.3f) : new Color(0.6f, 0.7f, 0.85f);
        if (GUI.Button(r, _now.text, _st))
        {
            System.Action a = _now.tap ?? (() => { if (HudModals.Available(HudModals.Modal.Tareas)) HudModals.Toggle(HudModals.Modal.Tareas); });
            _now = null;
            a();
        }
        GUI.backgroundColor = c;
    }
}
