using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Una pantalla del muro: su texto se renueva cada <see cref="Every"/> s y se pone roja en alerta.</summary>
public class GuardScreen : MonoBehaviour
{
    const float Every = 2f;
    public GuardWatch.Feed feed;
    public Renderer panel;
    public Material normal, alert;
    WorldLabel _label;
    float _next;

    void Start() => _label = GetComponent<WorldLabel>();

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + Every;
        if (_label != null) _label.text = $"{GuardWatch.Title(feed)}\n{GuardWatch.Read(feed)}";
        if (panel != null && normal != null && alert != null) panel.sharedMaterial = GuardWatch.Alert(feed) ? alert : normal;
    }
}
