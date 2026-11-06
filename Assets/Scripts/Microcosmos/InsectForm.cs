using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Recuerda qué se ocultó del cuerpo humano para devolvérselo (<see cref="InsectOnly.Release"/>).</summary>
public class InsectForm : MonoBehaviour
{
    public string species;
    [SerializeField] List<Renderer> _hidden = new List<Renderer>();

    /// <summary>Oculta todo lo visible (menos los rótulos y lo que ya sea insecto) y devuelve su estatura.</summary>
    public float HideHuman()
    {
        bool any = false; Bounds b = default;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r is ParticleSystemRenderer || r.GetComponent<TextMesh>() != null) continue;
            if (r.transform.IsChildOf(transform) && r.GetComponentInParent<CubeInsect>() != null && r.transform.parent != null && IsUnderInsect(r.transform)) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            r.enabled = false; _hidden.Add(r);
        }
        float h = any ? b.size.y : 1.75f;
        return Mathf.Clamp(h, 1.4f, 2.1f);
    }

    bool IsUnderInsect(Transform t) { for (Transform p = t; p != null && p != transform; p = p.parent) if (p.name == "Insecto") return true; return false; }

    public void Restore()
    {
        Transform ins = transform.Find("Insecto");
        if (ins != null) { if (Application.isPlaying) Destroy(ins.gameObject); else DestroyImmediate(ins.gameObject); }
        CubeInsect ci = GetComponent<CubeInsect>();
        if (ci != null) { if (Application.isPlaying) Destroy(ci); else DestroyImmediate(ci); }
        foreach (Renderer r in _hidden) if (r != null) r.enabled = true;
        _hidden.Clear();
        if (Application.isPlaying) Destroy(this); else DestroyImmediate(this);
    }
}
