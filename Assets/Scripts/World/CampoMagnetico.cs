using UnityEngine;

/// <summary>CAMPO MAGNÉTICO de una puerta de la raíz: la lámina de luz que sujeta el agua (como el del tanque de
/// virtualización). Riela despacio; los cuerpos la cruzan, el agua no.</summary>
public class CampoMagnetico : MonoBehaviour
{
    Renderer _r; MaterialPropertyBlock _mpb; Color _base; float _phase;
    void Start()
    {
        _r = GetComponent<Renderer>(); _mpb = new MaterialPropertyBlock();
        _base = _r != null && _r.sharedMaterial != null ? _r.sharedMaterial.color : new Color(0.55f, 0.85f, 1f, 0.35f);
        _phase = transform.position.y * 0.37f;
    }
    void Update()
    {
        if (_r == null) return;
        float k = 0.75f + 0.25f * Mathf.Sin(Time.time * 2.3f + _phase) * Mathf.Sin(Time.time * 0.7f + _phase * 2f);
        Color c = _base; c.a = _base.a * k;
        _r.GetPropertyBlock(_mpb); _mpb.SetColor("_BaseColor", c); _mpb.SetColor("_Color", c); _r.SetPropertyBlock(_mpb);
    }
}
