using UnityEngine;

/// <summary>
/// FICHA DE SALUD de un anyma (docs/sanctuary1-level-and-live-tests §4): si está **vacunado** y si se le ha hecho el
/// **análisis** clínico. <see cref="Linduarda"/> lanza su status global y, a quien tenga alguno en `false`, le nace
/// ir a vacunarlo / tomarle muestras. **Kushal empieza con ambos en false** → Linduarda lo atiende al empezar el juego.
/// Por defecto true (el resto del santuario ya está al día).
/// </summary>
public class HealthRecord : MonoBehaviour
{
    public bool vaccinated = true;
    public bool analyzed = true;

    public bool NeedsCare => !vaccinated || !analyzed;
}
