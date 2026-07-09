using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fixed-size pool of Hologram cards, sized to the largest list the menu will ever need to
/// show at once. Today that's the periodic table catalog (Magia) — nothing else in the menu
/// shows more items simultaneously, so bounding the pool to that count is safe. If a future
/// submenu needs more, raise poolSize; don't build a second pool.
/// See docs/ui-holographic-menu.md.
/// </summary>
public class HologramPool : MonoBehaviour
{
    public static HologramPool Instance { get; private set; }

    public RectTransform canvasRoot;
    public int poolSize = 118;
    public Vector2 cardSize = new Vector2(72f, 72f);

    readonly List<Hologram> _all = new List<Hologram>();
    readonly Queue<Hologram> _free = new Queue<Hologram>();

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < poolSize; i++)
        {
            Hologram h = CreateHologramInstance($"Hologram_{i}");
            _all.Add(h);
            _free.Enqueue(h);
        }
    }

    public Hologram Acquire()
    {
        if (_free.Count == 0)
        {
            Debug.LogWarning("[HologramPool] Pool agotado — se pidieron mas hologramas que poolSize.");
            return null;
        }
        Hologram h = _free.Dequeue();
        return h;
    }

    public void Release(Hologram h)
    {
        if (h == null) return;
        h.Release();
        _free.Enqueue(h);
    }

    public void ReleaseAll(List<Hologram> holograms)
    {
        foreach (Hologram h in holograms) Release(h);
        holograms.Clear();
    }

    // Flat card: translucent background + optional icon + text fallback. No 3D mesh, no
    // world-space Canvas — this is a plain screen-space UI element so it can be small,
    // corner-pinned and mobile-friendly (see docs/ui-holographic-menu.md).
    Hologram CreateHologramInstance(string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(canvasRoot, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = cardSize;

        Image background = go.GetComponent<Image>();
        background.color = new Color(0.3f, 0.85f, 0.95f, 0.22f);

        // Un Button agregado por AddComponent nunca recibe targetGraphic (solo el menu
        // "Crear > UI > Button" del editor lo autoasigna) — sin esto, el tinte de foco/hover no
        // se ve, dejando a un jugador que navega solo con teclado sin forma de saber donde esta.
        Button button = go.GetComponent<Button>();
        button.targetGraphic = background;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.4f, 1.9f, 2f, 1.6f); // foco por teclado — bien por encima de blanco para que resalte contra el fondo translucido
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(0.7f, 1f, 1f, 1.8f);
        button.colors = colors;

        GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(go.transform, false);
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.15f, 0.15f);
        iconRect.anchorMax = new Vector2(0.85f, 0.85f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        Image icon = iconGO.GetComponent<Image>();
        icon.preserveAspect = true;
        iconGO.SetActive(false);

        GameObject labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(go.transform, false);
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.05f, 0.05f);
        labelRect.anchorMax = new Vector2(0.95f, 0.95f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelGO.GetComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.enableAutoSizing = true;
        label.fontSizeMin = 7f;
        label.fontSizeMax = 16f;

        Hologram h = go.AddComponent<Hologram>();
        h.rect = rect;
        h.background = background;
        h.icon = icon;
        h.label = label;
        h.button = go.GetComponent<Button>();
        h.Release();
        return h;
    }
}
