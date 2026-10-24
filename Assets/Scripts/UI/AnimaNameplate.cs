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
    [Tooltip("Carteles de muebles como mucho a la vez (los más cercanos).")]
    [Min(1)] public int maxLabels = 40;

    GUIStyle _style;
    Anima[] _animas = System.Array.Empty<Anima>();
    WorldLabel[] _labels = System.Array.Empty<WorldLabel>();
    float _nextScan;
    readonly System.Collections.Generic.List<(float d, WorldLabel wl)> _near = new System.Collections.Generic.List<(float d, WorldLabel wl)>();

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;   // solo se pinta en Repaint (antes se recorría todo también en Layout)
        Camera cam = Camera.main;
        if (cam == null) return;
        if (_style == null) _style = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleCenter, richText = true };

        if (Time.time >= _nextScan) { _nextScan = Time.time + 0.5f; _animas = SceneCache.All<Anima>(); _labels = SceneCache.All<WorldLabel>(2f); }

        foreach (Anima a in _animas)
        {
            if (a == null || a.death) continue;
            if ((a.transform.position - cam.transform.position).sqrMagnitude > maxDistance * maxDistance) continue;   // lejos: ni se busca su mente
            Mind m = a.GetComponent<Mind>();
            string label = m != null && !string.IsNullOrEmpty(m.identity) ? m.identity : a.name;
            Draw(cam, a.transform.position + Vector3.up * headHeight, label, "E8E8E8");
        }
        foreach (WorldLabel wl in _labels)   // muebles/ingredientes/estaciones (cocina por teclado)
        {
            if (wl == null || string.IsNullOrEmpty(wl.text)) continue;
            Vector3 w = wl.transform.position + Vector3.up * wl.height;
            float d = (w - cam.transform.position).sqrMagnitude;
            if (d > maxDistance * maxDistance) continue;
            if (Vector3.Dot(w - cam.transform.position, cam.transform.forward) <= 0f) continue;   // detrás de la cámara
            _near.Add((d, wl));
        }
        // Solo los MÁS CERCANOS: en la cocina o el hospital hay cientos de carteles a la vista y cada GUI.Label con texto
        // enriquecido cuesta (era de lo más caro de cada fotograma).
        if (_near.Count > maxLabels) _near.Sort((a, b) => a.d.CompareTo(b.d));
        for (int i = 0; i < _near.Count && i < maxLabels; i++) Draw(cam, _near[i].wl.transform.position + Vector3.up * _near[i].wl.height, _near[i].wl.text, "C7E8FF");
        _near.Clear();
    }

    void Draw(Camera cam, Vector3 world, string label, string hex)
    {
        if ((world - cam.transform.position).sqrMagnitude > maxDistance * maxDistance) return;
        Vector3 sp = cam.WorldToScreenPoint(world);
        if (sp.z <= 0f) return;                                  // detrás de la cámara
        GUI.Label(new Rect(sp.x - 70f, Screen.height - sp.y - 10f, 140f, 20f), $"<color=#{hex}>{label}</color>", _style);
    }
}
