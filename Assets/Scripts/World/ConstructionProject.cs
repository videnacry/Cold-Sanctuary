using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OBRA EN CURSO (docs/jornada-y-rotacion.md): un área que aún se está construyendo (ballet, artes marciales,
/// calistenia…). Cada día el equipo de Construcción hace dos misiones: LLEVAR el material del día (palés al pie de la obra)
/// y TRABAJARLO (las paredes suben y el andamio se mueve). Cuando se completan los días de obra, el área queda TERMINADA y
/// sus misiones (entrenamientos) se abren a todos. Mientras, el cartel dice cuánto falta.
/// </summary>
public class ConstructionProject : MonoBehaviour
{
    public static readonly List<ConstructionProject> All = new List<ConstructionProject>();

    public string title = "Obra";
    [Min(1)] public int daysRequired = 5;
    public int daysDone;
    public bool materialOnSite = true;   // el primer día ya hay material en la obra
    public float wallHeight = 3f;

    public bool Finished => daysDone >= daysRequired;
    public float Progress => Mathf.Clamp01(daysDone / (float)daysRequired);
    public SanctuaryArea Area => GetComponent<SanctuaryArea>();

    readonly List<Transform> _walls = new List<Transform>();
    readonly List<float> _wallBaseY = new List<float>();
    Transform _material, _scaffold;
    WorldLabel _label;

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Start() { Collect(); Refresh(); }

    void Collect()
    {
        if (_walls.Count > 0) return;
        foreach (Transform t in transform)
        {
            if (t.name.StartsWith("Wall_")) { _walls.Add(t); _wallBaseY.Add(t.localScale.y); }
            else if (t.name == "Material_del_dia") _material = t;
            else if (t.name == "Andamio_obra") _scaffold = t;
        }
        _label = GetComponent<WorldLabel>();
        if (_label == null) { _label = gameObject.AddComponent<WorldLabel>(); _label.height = 4.5f; }
    }

    /// <summary>Misión «Llevar material»: dejan en la obra los palés de un día de trabajo.</summary>
    public void DeliverMaterial(string who)
    {
        if (Finished || materialOnSite) return;
        // Del ALMACÉN de Construcción (guadua, bahareque, micelio, cuerda); sin almacén (escenas de prueba), llega sin más.
        if (MaterialStore.Instance != null && !MaterialStore.Instance.TakeDayKit(who, title)) return;
        materialOnSite = true;
        Debug.Log($"[Obra] {who} lleva el material del día a «{title}».");
        Refresh();
    }

    /// <summary>Misión «Trabajar la obra»: se gasta el material del día y la obra avanza un día. Devuelve true al terminarla.</summary>
    public bool WorkDay(string who)
    {
        if (Finished) return false;
        if (!materialOnSite) { Debug.Log($"[Obra] {who} no puede avanzar «{title}»: falta el material del día."); return false; }
        materialOnSite = false;
        daysDone++;
        Debug.Log($"[Obra] {who} trabaja en «{title}»: {daysDone}/{daysRequired} días.");
        Refresh();
        if (Finished) Debug.Log($"[Obra] ¡«{title}» terminada! Se abren sus misiones de entrenamiento.");
        return Finished;
    }

    public void Refresh()
    {
        Collect();
        // Las paredes suben con la obra (de un murete a su altura); terminada, quedan enteras.
        float k = Finished ? 1f : Mathf.Lerp(0.15f, 0.9f, Progress);
        for (int i = 0; i < _walls.Count; i++)
        {
            Transform w = _walls[i];
            Vector3 s = w.localScale; s.y = _wallBaseY[i] * k; w.localScale = s;
            Vector3 p = w.localPosition; p.y = s.y * 0.5f; w.localPosition = p;
        }
        if (_material != null) _material.gameObject.SetActive(materialOnSite && !Finished);
        if (_scaffold != null) _scaffold.gameObject.SetActive(!Finished);
        if (_label != null) _label.text = Finished ? $"{title} (abierta)" : $"{title} — EN OBRAS ({daysDone}/{daysRequired} días)";
    }
}
