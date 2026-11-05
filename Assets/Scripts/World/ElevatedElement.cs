using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Un ELEMENTO ELEVADO (una torre, un árbol que crece, un anillo, lo que se añada): la regla de la lanzadera lo cuenta. Al
/// aparecer en juego, vuelve a validar la altura de la plataforma (y la sube si hace falta).</summary>
public class ElevatedElement : MonoBehaviour
{
    public string label = "";
    [Tooltip("Lo más alto (y); si es 0, se mide con sus dibujos.")]
    public float top;

    public float Top
    {
        get
        {
            if (top > 0f) return top;
            float y = transform.position.y; bool any = false;
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer || r.bounds.size.y > 1000f) continue;
                y = any ? Mathf.Max(y, r.bounds.max.y) : r.bounds.max.y; any = true;
            }
            return y;
        }
    }

    void OnEnable() { if (Application.isPlaying && LaunchSite.Instance != null) LaunchSite.Instance.Validate($"apareció «{name}»"); }
}
