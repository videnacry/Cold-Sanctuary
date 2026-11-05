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

    [Tooltip("Cada cuánto (s) se vuelve a mirar quién está cerca. Entre medias solo se pintan los ya elegidos.")]
    [Min(0.05f)] public float refresh = 0.25f;

    // Los carteles casi no se mueven y son 1 549: van en una rejilla de celdas de maxDistance, rehecha de vez en cuando, y de cada
    // vista solo se miran las 9 celdas de alrededor de la cámara. Antes se recorrían todos en cada repintado.
    struct Plate { public Transform t; public float height; public string text; public float d; }

    GUIStyle _style;
    readonly System.Collections.Generic.Dictionary<(int, int), System.Collections.Generic.List<Plate>> _grid = new System.Collections.Generic.Dictionary<(int, int), System.Collections.Generic.List<Plate>>();
    readonly System.Collections.Generic.List<Plate> _shown = new System.Collections.Generic.List<Plate>();
    readonly System.Collections.Generic.List<Plate> _near = new System.Collections.Generic.List<Plate>();
    float _nextLook, _nextGrid;
    float _cell = 35f;

    void BuildGrid()
    {
        _grid.Clear();
        _cell = maxDistance;
        foreach (WorldLabel wl in SceneCache.All<WorldLabel>(5f))
        {
            if (wl == null || string.IsNullOrEmpty(wl.text)) continue;
            Vector3 p = wl.transform.position;
            var key = (Mathf.FloorToInt(p.x / _cell), Mathf.FloorToInt(p.z / _cell));
            if (!_grid.TryGetValue(key, out var list)) _grid[key] = list = new System.Collections.Generic.List<Plate>();
            list.Add(new Plate { t = wl.transform, height = wl.height, text = $"<color=#C7E8FF>{wl.text}</color>" });
        }
    }

    void Look(Camera cam)
    {
        Vector3 at = cam.transform.position, fw = cam.transform.forward;
        float max = maxDistance * maxDistance;
        _shown.Clear();
        foreach (Anima a in SceneCache.All<Anima>())
        {
            if (a == null || a.death) continue;
            if ((a.transform.position - at).sqrMagnitude > max) continue;   // lejos: ni se busca su mente
            Mind m = a.GetComponent<Mind>();
            string label = m != null && !string.IsNullOrEmpty(m.identity) ? m.identity : a.name;
            _shown.Add(new Plate { t = a.transform, height = headHeight, text = $"<color=#E8E8E8>{label}</color>" });
        }
        _near.Clear();
        int cx = Mathf.FloorToInt(at.x / _cell), cz = Mathf.FloorToInt(at.z / _cell);
        for (int x = cx - 1; x <= cx + 1; x++)
            for (int z = cz - 1; z <= cz + 1; z++)
            {
                if (!_grid.TryGetValue((x, z), out var list)) continue;
                foreach (Plate p in list)
                {
                    if (p.t == null) continue;
                    Vector3 w = p.t.position + Vector3.up * p.height;
                    float d = (w - at).sqrMagnitude;
                    if (d > max || Vector3.Dot(w - at, fw) <= 0f) continue;
                    Plate q = p; q.d = d; _near.Add(q);
                }
            }
        // Solo los MÁS CERCANOS: en la cocina o el hospital hay cientos a la vista y cada GUI.Label con texto enriquecido cuesta.
        if (_near.Count > maxLabels) _near.Sort((a, b) => a.d.CompareTo(b.d));
        for (int i = 0; i < _near.Count && i < maxLabels; i++) _shown.Add(_near[i]);
    }

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;   // solo se pinta en Repaint (antes se recorría todo también en Layout)
        Camera cam = Camera.main;
        if (cam == null) return;
        if (_style == null) _style = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.MiddleCenter, richText = true };
        float now = Time.unscaledTime;
        if (now >= _nextGrid) { _nextGrid = now + 5f; BuildGrid(); }
        if (now >= _nextLook) { _nextLook = now + refresh; Look(cam); }
        foreach (Plate p in _shown) if (p.t != null) Draw(cam, p.t.position + Vector3.up * p.height, p.text);
    }

    void Draw(Camera cam, Vector3 world, string text)
    {
        if ((world - cam.transform.position).sqrMagnitude > maxDistance * maxDistance) return;
        Vector3 sp = cam.WorldToScreenPoint(world);
        if (sp.z <= 0f) return;                                  // detrás de la cámara
        GUI.Label(new Rect(sp.x - 70f, Screen.height - sp.y - 10f, 140f, 20f), text, _style);
    }
}
