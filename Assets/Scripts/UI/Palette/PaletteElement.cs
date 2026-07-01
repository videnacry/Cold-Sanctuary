using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ElementState { UI, Materializing, Physical, Dematerializing }

// MonoBehaviour del botón individual. Un prefab, múltiples contextos.
// En estado UI orbita al jugador como botón. En estado Physical es un bloque
// sólido en el mundo con collider y apariencia/dimensiones distintas.
public class PaletteElement : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Image      iconImage;
    [SerializeField] TMP_Text   labelText;
    [SerializeField] GameObject selectedHighlight;
    [SerializeField] GameObject lockedOverlay;

    [Header("Materialización")]
    [SerializeField] Vector3  blockScale        = new Vector3(1f, 0.25f, 1f);
    [SerializeField] Material blockMaterial;     // null = mantener material del prefab
    [SerializeField] float    materializeSpeed  = 2f;

    public PaletteElementData data       { get; private set; }
    public ElementState       state      { get; private set; } = ElementState.UI;
    public bool               isSelected { get; private set; }
    public bool               isLocked   { get; private set; }

    public event Action<PaletteElementData> OnActivated;

    Transform  _originalParent;
    Vector3    _originalLocalPos;
    Vector3    _originalLocalScale;
    Material   _originalMaterial;

    // ──────────────────────────────────────────────
    // Configuración como botón
    // ──────────────────────────────────────────────

    public void Configure(PaletteElementData d, int playerBond = 0)
    {
        data     = d;
        isLocked = playerBond < d.bondMin;

        if (iconImage != null) { iconImage.sprite = d.icon; iconImage.color = d.tint; }
        if (labelText != null) labelText.text = d.label;
        if (selectedHighlight != null) selectedHighlight.SetActive(false);
        if (lockedOverlay     != null) lockedOverlay.SetActive(isLocked);

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
        if (state != ElementState.UI) return;
        if (data == null || isLocked || data.shortcut == KeyCode.None) return;
        if (Input.GetKeyDown(data.shortcut)) Activate();
    }

    public void Activate()
    {
        if (state != ElementState.UI || isLocked || data == null) return;
        if (data.type == PaletteElementType.DialogueText) return;
        OnActivated?.Invoke(data);
    }

    // ──────────────────────────────────────────────
    // Materialización — UI → bloque físico en el mundo
    // ──────────────────────────────────────────────

    public void Materialize(Vector3 worldPos)
    {
        if (state != ElementState.UI) return;
        state = ElementState.Materializing;

        _originalParent     = transform.parent;
        _originalLocalPos   = transform.localPosition;
        _originalLocalScale = transform.localScale;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null) _originalMaterial = rend.material;

        transform.SetParent(null); // desanclar del jugador → objeto de mundo
        StartCoroutine(MaterializeRoutine(worldPos));
    }

    IEnumerator MaterializeRoutine(Vector3 targetPos)
    {
        Vector3 startPos   = transform.position;
        Vector3 startScale = transform.localScale;

        for (float t = 0f; t < 1f; t += Time.deltaTime * materializeSpeed)
        {
            float s = Mathf.SmoothStep(0f, 1f, t);
            transform.position   = Vector3.Lerp(startPos, targetPos, s);
            transform.localScale = Vector3.Lerp(startScale, blockScale, s);
            yield return null;
        }
        transform.position   = targetPos;
        transform.localScale = blockScale;

        if (blockMaterial != null)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null) rend.material = blockMaterial;
        }

        // BoxCollider con size (1,1,1) en espacio local; blockScale lo dimensiona en mundo
        BoxCollider col = gameObject.AddComponent<BoxCollider>();
        col.size = Vector3.one;

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true; // el bloque queda estático; no le afecta la gravedad

        state = ElementState.Physical;
    }

    // ──────────────────────────────────────────────
    // Desmaterialización — bloque físico → botón UI
    // ──────────────────────────────────────────────

    public void Dematerialize()
    {
        if (state != ElementState.Physical) return;
        state = ElementState.Dematerializing;
        StartCoroutine(DematerializeRoutine());
    }

    IEnumerator DematerializeRoutine()
    {
        Destroy(GetComponent<BoxCollider>());
        Destroy(GetComponent<Rigidbody>());

        if (_originalMaterial != null)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null) rend.material = _originalMaterial;
        }

        // Re-anclar al padre original; Unity preserva la posición de mundo (worldPositionStays=true)
        if (_originalParent != null)
            transform.SetParent(_originalParent);
        else
        {
            Destroy(gameObject); // padre destruido, descartar
            yield break;
        }

        Vector3 startLocalPos   = transform.localPosition;
        Vector3 startLocalScale = transform.localScale;

        for (float t = 0f; t < 1f; t += Time.deltaTime * materializeSpeed)
        {
            float s = Mathf.SmoothStep(0f, 1f, t);
            transform.localPosition = Vector3.Lerp(startLocalPos, _originalLocalPos, s);
            transform.localScale    = Vector3.Lerp(startLocalScale, _originalLocalScale, s);
            yield return null;
        }
        transform.localPosition = _originalLocalPos;
        transform.localScale    = _originalLocalScale;

        state = ElementState.UI;
    }
}
