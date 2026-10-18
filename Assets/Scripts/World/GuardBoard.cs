using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TABLERO DE GUARDIA (F en lo alto de la torre): lista las misiones de vigía. «Patrullar» ya se juega: recorrer los
/// puestos de vigía del santuario (marcados con un poste y farol) con un indicador de dirección. Las demás se listan con
/// lo que piden; la Magnate ya hace de oficio «enseñar al rezagado».
/// </summary>
public class GuardBoard : MonoBehaviour, IInteractable
{
    public string InteractLabel => "Ver misiones de guardia";
    public bool CanInteract => true;

    bool _open;
    PatrolMission _patrol;

    static readonly string[] Missions =
    {
        "1  Patrullar: recorre los puestos de vigía del santuario.",
        "2  Alimentar animales: lleva comida de la barra de servicio a los animales del bosque (próximamente).",
        "3  Llevar heridos a la enfermería: acompaña al que esté enfermo hasta Linduarda (próximamente).",
        "4  Enseñar al rezagado: la Magnate lo hace sola; acompáñala cuando enseñe yoga.",
    };

    public void Interact() { _open = !_open; }

    void Update()
    {
        if (!_open) return;
        if (GameKeys.GetKeyDown(KeyCode.Escape)) _open = false;
        if (GameKeys.GetKeyDown(KeyCode.Alpha1)) { StartPatrol(); _open = false; }
    }

    /// <summary>Arranca la patrulla (también desde la consola de hechizos o los tests).</summary>
    public PatrolMission StartPatrol()
    {
        if (_patrol == null) { if (!TryGetComponent(out _patrol)) _patrol = gameObject.AddComponent<PatrolMission>(); }
        _patrol.Begin();
        return _patrol;
    }

    void OnGUI()
    {
        if (!_open) return;
        GUI.Box(new Rect(Screen.width * 0.5f - 300f, 80f, 600f, 170f), "<b>Misiones de guardia</b>  (número para elegir · Esc cierra)");
        var st = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
        for (int i = 0; i < Missions.Length; i++) GUI.Label(new Rect(Screen.width * 0.5f - 285f, 110f + i * 32f, 570f, 30f), Missions[i], st);
    }
}
