using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One reusable, poolable flat UI card. HologramPool owns a fixed set of these and
/// HologramMenuController reconfigures them (label/icon, screen position, click behavior) as
/// the player navigates — no per-item Instantiate/Destroy. See docs/ui-holographic-menu.md.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class Hologram : MonoBehaviour
{
    public RectTransform rect;
    public Image background;
    public TMP_Text label;
    public Image icon;
    public Button button;

    Action _onClick;

    void Awake()
    {
        // Resolve via GetComponent rather than trusting the public fields to already be set —
        // HologramPool.CreateHologramInstance() calls AddComponent<Hologram>() (which runs
        // Awake() synchronously, before it gets a chance to assign rect/button back) and only
        // wires the fields on the line after that call returns.
        if (rect == null) rect = GetComponent<RectTransform>();
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(() => _onClick?.Invoke());
    }

    /// <summary>Symbol/label text always shown as a fallback; icon (if given) is preferred.</summary>
    public void SetContent(string text, Sprite iconSprite, Action onClick)
    {
        gameObject.SetActive(true);
        label.text = text;
        bool hasIcon = iconSprite != null;
        icon.sprite = iconSprite;
        icon.gameObject.SetActive(hasIcon);
        label.gameObject.SetActive(!hasIcon);
        _onClick = onClick;
    }

    public void SetAnchoredPosition(Vector2 pos) => rect.anchoredPosition = pos;

    public void Release()
    {
        _onClick = null;
        gameObject.SetActive(false);
    }
}
