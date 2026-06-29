using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to each UI button in the posture interface.
/// Notifies AsanaDetector when the player taps this position.
/// Supports both click (mobile/desktop) and keyboard hotkey.
/// </summary>
public class BodyPositionButton : MonoBehaviour
{
    [Header("Position this button represents")]
    public BodyPosition bodyPosition;

    [Header("References")]
    public AsanaDetector detector;
    public Button uiButton;
    public TMP_Text label;

    void Start()
    {
        if (label != null)
            label.text = $"{bodyPosition.description} [{bodyPosition.hotkey}]";

        if (uiButton != null)
            uiButton.onClick.AddListener(OnPressed);
    }

    void Update()
    {
        if (Input.GetKeyDown(bodyPosition.hotkey))
            OnPressed();
    }

    private void OnPressed()
    {
        if (detector != null)
            detector.SelectPosition(bodyPosition.bodyPart);
    }
}
