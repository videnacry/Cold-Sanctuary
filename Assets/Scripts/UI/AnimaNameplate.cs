using UnityEngine;

/// <summary>
/// ETIQUETAS DE NOMBRE sobre las animas (docs/typed-spells-and-queues.md): dibuja el nombre encima de cada
/// <see cref="Anima"/> a la vista, para que el jugador sepa a quién/qué dirigirse al **escribir hechizos**
/// (p. ej. <c>walkTo({destiny:"Marilia"})</c> desde la <see cref="SpellConsole"/>). Un solo componente en escena
/// (como <see cref="AnimaThoughtHUD"/>). Prototipo OnGUI con proyección mundo→pantalla; la versión world-space
/// (TextMesh por anima) es el paso siguiente.
/// </summary>
public class AnimaNameplate : MonoBehaviour
{
    [Tooltip("Distancia máxima a la cámara para mostrar el nombre.")]
    [Min(1f)] public float maxDistance = 35f;
    [Tooltip("Altura sobre el pivote del anima donde se ancla la etiqueta (m).")]
    public float headHeight = 1.2f;

    GUIStyle _style;
    Anima[] _animas = System.Array.Empty<Anima>();
    float _nextScan;

    void OnGUI()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        if (_style == null) _style = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleCenter, richText = true };

        if (Time.time >= _nextScan) { _nextScan = Time.time + 0.5f; _animas = FindObjectsOfType<Anima>(); }

        foreach (Anima a in _animas)
        {
            if (a == null || a.death) continue;
            Vector3 world = a.transform.position + Vector3.up * headHeight;
            if ((world - cam.transform.position).sqrMagnitude > maxDistance * maxDistance) continue;
            Vector3 sp = cam.WorldToScreenPoint(world);
            if (sp.z <= 0f) continue;                                  // detrás de la cámara

            Mind m = a.GetComponent<Mind>();
            string label = m != null && !string.IsNullOrEmpty(m.identity) ? m.identity : a.name;
            float screenY = Screen.height - sp.y;
            GUI.Label(new Rect(sp.x - 70f, screenY - 10f, 140f, 20f), $"<color=#E8E8E8>{label}</color>", _style);
        }
    }
}
