using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum HologramCorner { TopLeft, TopRight, BottomLeft, BottomRight }

/// <summary>
/// Orchestrates the whole holographic menu: a corner "Menú" hologram opens Magia / Yoga /
/// Sistema, each with their own content and layout. Purpose-built rather than a generic tree
/// interpreter — the 3-level structure is fixed and known, see docs/ui-holographic-menu.md
/// for the full design rationale (including the open question of where the "full asana/spell"
/// shortcut holograms should sit long-term).
/// </summary>
public class HologramMenuController : MonoBehaviour
{
    [Header("Refs")]
    public HologramPool pool;
    public AsanaDetector asanaDetector;
    public List<BodyPosition> bodyPositions = new List<BodyPosition>();

    [Header("Layout")]
    public HologramCorner menuCorner = HologramCorner.BottomRight;
    public HologramCorner closeCorner = HologramCorner.TopRight;
    public HologramCorner shortcutCorner = HologramCorner.TopLeft;
    public Vector2 screenMargin = new Vector2(24f, 24f);

    [Header("Sistema")]
    public Slider volumeSlider;
    const string CameraModePrefKey = "CameraMode";
    const string VolumePrefKey = "MasterVolume";

    // Todo el juego debe ser jugable solo con mouse O solo con teclado — este toggle abre/
    // cierra el menu sin necesitar clic, y cada Open* deja seleccionado (EventSystem) el primer
    // holograma para que las flechas y Enter/Espacio (Submit, ver InputManager) naveguen y
    // activen sin tocar el mouse. La navegacion entre hologramas usa el Navigation.Mode.Automatic
    // por defecto de Button — funciona bien porque cada tarjeta tiene una posicion real en
    // pantalla (ver "Layout helpers" abajo), asi que no hace falta cablear vecinos a mano.
    [Header("Teclado")]
    public KeyCode menuToggleKey = KeyCode.M;

    Hologram _menuHologram;
    readonly List<Hologram> _active = new List<Hologram>();
    bool _menuOpen;
    Vector2 CardSize => pool.cardSize;

    void Start()
    {
        _menuHologram = pool.Acquire();
        _menuHologram.SetAnchoredPosition(GetCornerBase(menuCorner));
        _menuHologram.SetContent("Menú", null, OpenRoot);
        EventSystem.current?.SetSelectedGameObject(_menuHologram.gameObject);

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
            AudioListener.volume = volumeSlider.value;
            volumeSlider.onValueChanged.AddListener(SetVolume);
            volumeSlider.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(menuToggleKey))
        {
            if (_menuOpen) CloseAll();
            else OpenRoot();
        }
        else if (_menuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAll();
        }
    }

    // Deja seleccionado el primer holograma activo del submenu recien construido — sin esto,
    // el EventSystem no tiene nada seleccionado tras abrir por teclado y las flechas no
    // arrancan la navegacion hasta el primer clic de mouse.
    void FocusFirstActive()
    {
        if (_active.Count > 0)
            EventSystem.current?.SetSelectedGameObject(_active[0].gameObject);
    }

    // ── Root ──────────────────────────────────────────────────────────────────

    void OpenRoot()
    {
        if (_menuOpen) return;
        _menuOpen = true;
        _menuHologram.gameObject.SetActive(false);

        AddCenteredVertical(0, 3, "Magia", OpenMagia);
        AddCenteredVertical(1, 3, "Yoga", OpenYoga);
        AddCenteredVertical(2, 3, "Sistema", OpenSistema);
        AddCorner(closeCorner, "X", CloseAll);
        FocusFirstActive();
    }

    void CloseAll()
    {
        Time.timeScale = 1f;
        if (volumeSlider != null) volumeSlider.gameObject.SetActive(false);
        pool.ReleaseAll(_active);
        _menuOpen = false;
        _menuHologram.gameObject.SetActive(true);
        EventSystem.current?.SetSelectedGameObject(_menuHologram.gameObject);
    }

    void ClearSubmenu()
    {
        Time.timeScale = 1f;
        if (volumeSlider != null) volumeSlider.gameObject.SetActive(false);
        pool.ReleaseAll(_active);
    }

    // ── Magia (tabla periódica = invocación; Hechizos = ejecución directa) ─────

    void OpenMagia()
    {
        ClearSubmenu();
        List<string> discovered = new List<string>(PeriodicTableManager.Instance.AllDiscovered());
        ArrangePeriodicGrid(discovered);
        AddCorner(shortcutCorner, "Hechizos", OpenHechizosShortcuts);
        AddCorner(closeCorner, "X", CloseAll);
        FocusFirstActive();
    }

    // Hechizos: un holograma por hechizo completo desbloqueado, para lanzarlo directo sin
    // pasar elemento por elemento. Sin datos todavia (no existe un "Spell" ScriptableObject
    // ni una nocion de hechizo desbloqueado en el codigo) — placeholder de layout, ver TODO
    // en docs/ui-holographic-menu.md.
    void OpenHechizosShortcuts()
    {
        ClearSubmenu();
        AddCorner(shortcutCorner, "Tabla", OpenMagia);
        AddCorner(closeCorner, "X", CloseAll);
        FocusFirstActive();
    }

    // ── Yoga ──────────────────────────────────────────────────────────────────

    void OpenYoga()
    {
        ClearSubmenu();
        ArrangeGrid(bodyPositions.Count, 4, i =>
        {
            BodyPosition pos = bodyPositions[i];
            return ($"{pos.description}\n[{pos.hotkey}]", (Action)(() => asanaDetector.SelectPosition(pos.bodyPart)));
        });
        AddCorner(shortcutCorner, "Asanas", OpenAsanasShortcuts);
        AddCorner(closeCorner, "X", CloseAll);
        FocusFirstActive();
    }

    // Asanas: un holograma por asana completa desbloqueada (AsanaDetector.unlockedAsanas),
    // para ejecutarla directo. unlockedAsanas esta vacio hasta que se creen los assets Asana
    // — ver DEVLOG "2. Interfaz de Posturas / Asanas" y el TODO en docs/ui-holographic-menu.md.
    void OpenAsanasShortcuts()
    {
        ClearSubmenu();
        if (asanaDetector != null)
        {
            List<Asana> unlocked = new List<Asana>();
            foreach (Asana asana in asanaDetector.unlockedAsanas)
                if (asana.ShortcutUnlocked) unlocked.Add(asana);

            ArrangeGrid(unlocked.Count, 4, i =>
                (unlocked[i].displayName, (Action)(() => { /* AsanaDetector.ExecuteShortcut() aun no existe */ })));
        }
        AddCorner(shortcutCorner, "Posturas", OpenYoga);
        AddCorner(closeCorner, "X", CloseAll);
        FocusFirstActive();
    }

    // ── Sistema ───────────────────────────────────────────────────────────────

    void OpenSistema()
    {
        ClearSubmenu();
        AddCenteredVertical(0, 2, "Cámara 1ª/3ª", ToggleCameraPerspective);
        AddCorner(closeCorner, "X", CloseAll);

        if (volumeSlider != null)
        {
            volumeSlider.gameObject.SetActive(true);
            RectTransform sliderRect = volumeSlider.GetComponent<RectTransform>();
            Vector2 pos = CenteredVerticalPosition(1, 2);
            sliderRect.anchorMin = Vector2.zero;
            sliderRect.anchorMax = Vector2.zero;
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = pos;
        }

        Time.timeScale = 0f;
        FocusFirstActive();
    }

    void ToggleCameraPerspective()
    {
        if (CameraManager.Instance == null) return;
        CameraMode next = CameraManager.Instance.preferredMode == CameraMode.ThirdPerson
            ? CameraMode.FirstPerson
            : CameraMode.ThirdPerson;
        CameraManager.Instance.SwitchTo(next);
        PlayerPrefs.SetInt(CameraModePrefKey, (int)next);
        PlayerPrefs.Save();
    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumePrefKey, value);
        PlayerPrefs.Save();
    }

    // ── Layout helpers ───────────────────────────────────────────────────────
    // Every Hologram uses anchorMin=anchorMax=(0,0) (bottom-left of the canvas), so
    // anchoredPosition is a plain pixel offset from the canvas' bottom-left corner —
    // corner/vertical/grid placement is just arithmetic on that one coordinate space.

    Vector2 GetCornerBase(HologramCorner corner)
    {
        float w = pool.canvasRoot.rect.width;
        float h = pool.canvasRoot.rect.height;
        switch (corner)
        {
            case HologramCorner.BottomRight: return new Vector2(w - screenMargin.x, screenMargin.y);
            case HologramCorner.BottomLeft:  return new Vector2(screenMargin.x, screenMargin.y);
            case HologramCorner.TopRight:    return new Vector2(w - screenMargin.x, h - screenMargin.y);
            default:                         return new Vector2(screenMargin.x, h - screenMargin.y);
        }
    }

    // Vertical list centered on screen as a group (not corner-anchored) — used for the root
    // Menú→Magia/Yoga/Sistema list and Sistema's own controls, so they read easily instead of
    // hugging the edge. The "Menú" launcher and the close/shortcut buttons stay in their
    // corners regardless (that part of the original corner-anchored design is unchanged).
    Vector2 CenteredVerticalPosition(int index, int count)
    {
        Vector2 origin = new Vector2(pool.canvasRoot.rect.width * 0.5f, pool.canvasRoot.rect.height * 0.5f);
        float spacing = CardSize.y + 12f;
        float startY = origin.y + (count - 1) * spacing * 0.5f;
        return new Vector2(origin.x, startY - index * spacing);
    }

    void AddCenteredVertical(int index, int count, string text, Action onClick)
    {
        Hologram h = pool.Acquire();
        if (h == null) return;
        h.SetAnchoredPosition(CenteredVerticalPosition(index, count));
        h.SetContent(text, null, onClick);
        _active.Add(h);
    }

    void AddCorner(HologramCorner corner, string text, Action onClick)
    {
        Hologram h = pool.Acquire();
        if (h == null) return;
        h.SetAnchoredPosition(GetCornerBase(corner));
        h.SetContent(text, null, onClick);
        _active.Add(h);
    }

    // Simple centered grid — used for Yoga's body positions and the Hechizos/Asanas shortcut
    // lists. contentFor(i) returns the (label, onClick) for slot i.
    void ArrangeGrid(int count, int columns, Func<int, (string label, Action onClick)> contentFor)
    {
        Vector2 origin = new Vector2(pool.canvasRoot.rect.width * 0.5f, pool.canvasRoot.rect.height * 0.5f);
        for (int i = 0; i < count; i++)
        {
            Hologram h = pool.Acquire();
            if (h == null) return;
            int col = i % columns;
            int row = i / columns;
            Vector2 pos = origin + new Vector2(
                (col - (columns - 1) / 2f) * (CardSize.x + 10f),
                -row * (CardSize.y + 10f) + CardSize.y);
            h.SetAnchoredPosition(pos);
            (string label, Action onClick) content = contentFor(i);
            h.SetContent(content.label, null, content.onClick);
            _active.Add(h);
        }
    }

    // Approximate periodic table shape: one column per ElementGroup, elements stacked within
    // their column in discovery order. Not chemically exact (real periods/lanthanides would
    // need atomic-number-aware row math) — good enough for "looks like the periodic table"
    // at a glance. See TODO in docs/ui-holographic-menu.md.
    void ArrangePeriodicGrid(List<string> symbols)
    {
        var byGroup = new Dictionary<PeriodicTableManager.ElementGroup, List<string>>();
        foreach (string symbol in symbols)
        {
            ElementData data = PeriodicTableManager.Instance.GetData(symbol);
            if (data == null) continue;
            if (!byGroup.TryGetValue(data.group, out List<string> list))
            {
                list = new List<string>();
                byGroup[data.group] = list;
            }
            list.Add(symbol);
        }

        Vector2 origin = new Vector2(pool.canvasRoot.rect.width * 0.5f, pool.canvasRoot.rect.height * 0.5f);
        int col = 0;
        int totalColumns = Mathf.Max(byGroup.Count, 1);
        foreach (KeyValuePair<PeriodicTableManager.ElementGroup, List<string>> kv in byGroup)
        {
            int row = 0;
            foreach (string symbol in kv.Value)
            {
                Hologram h = pool.Acquire();
                if (h == null) return;
                Vector2 pos = origin + new Vector2(
                    (col - (totalColumns - 1) / 2f) * (CardSize.x + 6f),
                    -row * (CardSize.y + 6f) + CardSize.y);
                h.SetAnchoredPosition(pos);
                ElementData data = PeriodicTableManager.Instance.GetData(symbol);
                h.SetContent(symbol, null, () => { }); // solo lectura -- ver DEVLOG "1. Interfaz de Encantamientos"
                _active.Add(h);
                row++;
            }
            col++;
        }
    }
}
