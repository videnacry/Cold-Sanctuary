using UnityEngine;

/// <summary>El andén de cada casquete del Santuario Aéreo: pisarlo ofrece volver al muelle. No guarda referencias a la escena de abajo.</summary>
public class AereoGate : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || AereoLink.Instance == null) return;
        GameObject p = other.gameObject;
        ChoiceDialog.Show($"{AereoRule.Building}: volver", new[] { "Volver al muelle del santuario", "Quedarse" }, i => { if (i == 0) AereoLink.Instance.Return(p); }, null, transform, 5f);
    }
}
