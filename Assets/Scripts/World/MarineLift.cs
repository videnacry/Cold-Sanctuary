using UnityEngine;

/// <summary>El andén de abajo, en el Santuario Marino: pisarlo ofrece subir. No guarda referencias a la escena de arriba.</summary>
public class MarineLift : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && SiteLine.Instance != null) SiteLine.Instance.AskUp(other.gameObject);
    }
}
