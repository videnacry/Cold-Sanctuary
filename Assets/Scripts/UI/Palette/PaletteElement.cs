using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// MonoBehaviour del botón individual. Un prefab, múltiples contextos.
// Las referencias visuales se asignan en el prefab; la data llega en Configure().
public class PaletteElement : MonoBehaviour
{
    [SerializeField] Image      iconImage;
    [SerializeField] TMP_Text   labelText;
    [SerializeField] GameObject selectedHighlight;
    [SerializeField] GameObject lockedOverlay;

    public PaletteElementData data       { get; private set; }
    public bool               isSelected { get; private set; }
    public bool               isLocked   { get; private set; }

    public event Action<PaletteElementData> OnActivated;

    public void Configure(PaletteElementData d, int playerBond = 0)
    {
        data     = d;
        isLocked = playerBond < d.bondMin;

        if (iconImage != null) { iconImage.sprite = d.icon; iconImage.color = d.tint; }
        if (labelText != null) labelText.text = d.label;
        if (selectedHighlight != null) selectedHighlight.SetActive(false);
        if (lockedOverlay     != null) lockedOverlay.SetActive(isLocked);

        // los bloques de texto del NPC no son interactuables
        if (d.type == PaletteElementType.DialogueText)
        {
            Button btn = GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }
    }

    public void SetSelected(bool val)
    {
        isSelected = val;
        if (selectedHighlight != null) selectedHighlight.SetActive(val);
    }

    void Update()
    {
        if (data == null || isLocked || data.shortcut == KeyCode.None) return;
        if (Input.GetKeyDown(data.shortcut)) Activate();
    }

    // Llamado por el Button.onClick del prefab O desde Update al detectar la tecla.
    public void Activate()
    {
        if (isLocked || data == null) return;
        if (data.type == PaletteElementType.DialogueText) return;
        OnActivated?.Invoke(data);
    }
}
