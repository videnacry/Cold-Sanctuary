using UnityEngine;

/// <summary>El andén de cada casquete del Santuario Aéreo: pisarlo ofrece volver al muelle o subir en el ascensor del tronco. No guarda referencias a la escena de abajo.</summary>
public class AereoGate : MonoBehaviour
{
    [Tooltip("El ascensor del tronco de este casquete.")] public AereoLift lift;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || AereoLink.Instance == null) return;
        GameObject p = other.gameObject;
        string[] options = lift != null ? new[] { "Volver al muelle del santuario", "Ascensor del tronco", "Quedarse" } : new[] { "Volver al muelle del santuario", "Quedarse" };
        ChoiceDialog.Show($"{AereoRule.Building}: volver", options, i => { if (i == 0) AereoLink.Instance.Return(p); else if (lift != null && i == 1) lift.Ask(p, transform); }, null, transform, 5f);
    }
}
