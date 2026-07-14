using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// The list of available mob-missions, shown OVER the black screen (docs §3).
///
/// Both the VirtualizationMachine and the lotus meditation open this menu through
/// MeditationSession. Selecting a mission runs the reality shift and fades back in.
///
/// Singleton with auto-bootstrap: builds a simple runtime UI (title + a button per mission)
/// if none is wired. Keyboard-first (mecanografía): keys 1–9 pick a mission, Escape cancels —
/// consistent with ConfirmationPanel's no-mouse-required philosophy.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class MissionSelectMenu : MonoBehaviour
{
    public static MissionSelectMenu Instance { get; private set; }

    /// <summary>True while the menu is on screen.</summary>
    public bool IsOpen { get; private set; }

    [Tooltip("Container the mission buttons are added under. Auto-built if null.")]
    public RectTransform listContainer;

    Action<MobMission> _onSelected;
    Action             _onCancel;
    readonly List<MobMission> _missions = new List<MobMission>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(true);
        HideImmediate();
    }

    // Built-in UI font with a fallback across Unity versions.
    static Font _font;
    static Font UIFont => _font != null ? _font :
        (_font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                 ?? Resources.GetBuiltinResource<Font>("Arial.ttf"));

    void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape)) { Cancel(); return; }

        // Number keys 1..9 → pick the Nth listed mission
        for (int i = 0; i < _missions.Count && i < 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                { Select(_missions[i]); return; }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Show the menu. Only available missions are listed. onSelected fires with the chosen
    /// mission; onCancel fires if the player backs out (Escape).
    /// </summary>
    public void Show(IEnumerable<MobMission> missions,
                     Action<MobMission> onSelected, Action onCancel)
    {
        _onSelected = onSelected;
        _onCancel   = onCancel;

        _missions.Clear();
        foreach (var m in missions)
            if (m != null && m.isAvailable) _missions.Add(m);

        BuildButtons();
        IsOpen = true;
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }

    // ── Selection ──────────────────────────────────────────────────────────────

    void Select(MobMission mission)
    {
        var cb = _onSelected;
        Close();
        cb?.Invoke(mission);
    }

    void Cancel()
    {
        var cb = _onCancel;
        Close();
        cb?.Invoke();
    }

    void Close()
    {
        _onSelected = null;
        _onCancel   = null;
        IsOpen = false;
        HideImmediate();
    }

    void HideImmediate()
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }

    // ── Runtime UI ──────────────────────────────────────────────────────────────

    void BuildButtons()
    {
        if (listContainer == null) return;

        foreach (Transform child in listContainer) Destroy(child.gameObject);

        for (int i = 0; i < _missions.Count; i++)
        {
            MobMission m = _missions[i];
            string label = $"{i + 1}.  {m.missionName}" +
                           (string.IsNullOrEmpty(m.description) ? "" : $"\n     {m.description}");
            CreateButton(label, () => Select(m));
        }

        CreateButton("Esc.  Salir", Cancel);
    }

    void CreateButton(string label, Action onClick)
    {
        var go = new GameObject("MissionButton", typeof(RectTransform));
        go.transform.SetParent(listContainer, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.08f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick());

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 56f;

        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(go.transform, false);
        var text = textGO.AddComponent<Text>();
        text.font = UIFont;
        text.text = label;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.fontSize = 20;
        var trt = text.rectTransform;
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(16f, 4f); trt.offsetMax = new Vector2(-16f, -4f);
    }

    // ── Auto-bootstrap ──────────────────────────────────────────────────────────

    CanvasGroup _canvasGroup;

    /// <summary>Returns the menu, creating a runtime Canvas above the ScreenFader if none exists.</summary>
    public static MissionSelectMenu Get()
    {
        if (Instance != null) return Instance;

        EnsureEventSystem();

        var canvasGO = new GameObject("MissionSelectMenu (auto)");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1001; // above the black ScreenFader (1000)
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        var group = canvasGO.AddComponent<CanvasGroup>();

        // Title
        var titleGO = new GameObject("Title", typeof(RectTransform));
        titleGO.transform.SetParent(canvasGO.transform, false);
        var title = titleGO.AddComponent<Text>();
        title.font = UIFont;
        title.text = "Elige una práctica";
        title.color = Color.white;
        title.fontSize = 30;
        title.alignment = TextAnchor.UpperCenter;
        var titleRT = title.rectTransform;
        titleRT.anchorMin = new Vector2(0.5f, 1f); titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -60f);
        titleRT.sizeDelta = new Vector2(600f, 50f);

        // Vertical list
        var listGO = new GameObject("List", typeof(RectTransform));
        listGO.transform.SetParent(canvasGO.transform, false);
        var listRT = listGO.GetComponent<RectTransform>();
        listRT.anchorMin = new Vector2(0.5f, 0.5f); listRT.anchorMax = new Vector2(0.5f, 0.5f);
        listRT.pivot = new Vector2(0.5f, 0.5f);
        listRT.sizeDelta = new Vector2(560f, 400f);
        var layout = listGO.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = true; layout.childControlHeight = true;
        layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;

        Instance = canvasGO.AddComponent<MissionSelectMenu>();
        Instance.listContainer = listRT;
        Instance.HideImmediate();
        return Instance;
    }

    static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }
}
