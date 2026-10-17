using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>PATRULLA: visitar en orden los puestos de vigía (<c>PuestoVigia_*</c>); al llegar a cada uno, el siguiente.
/// Recompensa: XP de Stats y un poco de observación. HUD con la flecha al siguiente puesto.</summary>
public class PatrolMission : MonoBehaviour
{
    public float reachRadius = 4f;
    readonly List<Transform> _posts = new List<Transform>();
    int _next = -1;
    Transform _player;

    public bool Running => _next >= 0;
    public int Visited => Mathf.Max(0, _next);
    public int Total => _posts.Count;

    public void Begin()
    {
        _posts.Clear();
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (t.name.StartsWith("PuestoVigia_")) _posts.Add(t);
        _posts.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        _player = p != null ? p.transform : null;
        _next = _posts.Count > 0 ? 0 : -1;
        Debug.Log(_posts.Count > 0 ? $"[Guardia] Patrulla: recorre {_posts.Count} puestos de vigía." : "[Guardia] No hay puestos de vigía en esta escena.");
    }

    void Update()
    {
        if (_next < 0 || _player == null || _next >= _posts.Count) return;
        Vector3 d = _posts[_next].position - _player.position; d.y = 0f;
        if (d.magnitude <= reachRadius) Reach();
    }

    /// <summary>Marca el puesto actual como visitado (lo usa Update al llegar, y los tests).</summary>
    public void Reach()
    {
        if (_next < 0 || _next >= _posts.Count) return;
        Debug.Log($"[Guardia] Puesto {_next + 1}/{_posts.Count} vigilado ({_posts[_next].name.Replace("PuestoVigia_", "")}).");
        _next++;
        if (_next >= _posts.Count)
        {
            Debug.Log("[Guardia] ✅ Patrulla completa: el santuario está en calma.");
            if (_player != null) { CharacterLevel cl = _player.GetComponent<CharacterLevel>(); if (cl != null) cl.GainStatsXp(15f); }
            _next = -1;
        }
    }

    void OnGUI()
    {
        if (_next < 0 || _player == null || _next >= _posts.Count) return;
        string arrow = HudGate.Arrow(_player.position, _posts[_next].position);
        float dist = Vector3.Distance(_player.position, _posts[_next].position);
        GUI.Box(new Rect(Screen.width * 0.5f - 230f, 40f, 460f, 28f), $"Patrulla {_next}/{_posts.Count} · siguiente: {_posts[_next].name.Replace("PuestoVigia_", "")} {arrow} {dist:0} m");
    }
}
