using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL SANTUARIO MARINO, la escena (docs/sitio-nuclear.md §20): solo sitios e instrumentos a la vista. La lógica vive arriba (<see cref="CavernRock"/>, <see cref="Refuge"/>, <see cref="SiteLine"/>),
/// para que las misiones funcionen sin cargar esta escena. Nada de arriba apunta aquí: todo cruza por <see cref="Instance"/>, que solo existe mientras la escena está cargada.
/// Mientras Kushal está abajo, oscurece la niebla, y al subir la devuelve.
/// </summary>
public class MarineBase : MonoBehaviour
{
    public static MarineBase Instance { get; private set; }

    [Tooltip("Donde para el carro abajo.")] public Transform arrival;
    [Tooltip("Dónde se espera en el refugio.")] public Transform refugeSpot;
    public List<Transform> extensometers = new List<Transform>(), convergence = new List<Transform>(), loadCells = new List<Transform>(),
        geophones = new List<Transform>(), piezometers = new List<Transform>(), flowMeters = new List<Transform>();
    [Tooltip("La holgura medida al construir (m).")] public float clearanceM;

    Color _fog, _ambient;
    bool _entered;

    void OnEnable() { Instance = this; }
    void OnDisable() { Leave(); if (Instance == this) Instance = null; }

    public void Enter()
    {
        if (_entered) return;
        _entered = true; _fog = RenderSettings.fogColor; _ambient = RenderSettings.ambientLight;
        RenderSettings.fogColor = new Color(0.03f, 0.05f, 0.07f);
        RenderSettings.ambientLight = new Color(0.45f, 0.47f, 0.5f);
    }

    public void Leave()
    {
        if (!_entered) return;
        _entered = false; RenderSettings.fogColor = _fog; RenderSettings.ambientLight = _ambient;
    }
}
