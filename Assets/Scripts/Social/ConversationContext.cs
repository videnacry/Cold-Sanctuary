using UnityEngine;

/// <summary>
/// EL CONTEXTO DE LAS CHARLAS de una escena (docs/conversaciones.md §Temas): de qué hablan quienes viven aquí además de su
/// trabajo, su familia y sus necesidades. En un microcosmos la era y el lugar salen solos de <see cref="MicroEra"/>; este componente
/// añade la HISTORIA del microcosmos (frases cortas que alguien cuenta y el otro comenta) o, fuera de un microcosmos, el lugar y la
/// época. Opcional: sin él las charlas usan el santuario de hoy.
/// </summary>
public class ConversationContext : MonoBehaviour
{
    public static ConversationContext Current { get; private set; }

    [Tooltip("El lugar («la sala de charlas», «Kapilavastu»). Vacío: el de MicroEra o «el santuario».")]
    public string place = "";
    [Tooltip("La época contada («hoy, en el santuario helado», «s. V a. C.»). Vacío: la de MicroEra.")]
    public string era = "";
    [Tooltip("La historia del sitio: frases cortas que alguien cuenta (una por charla).")]
    [TextArea] public string[] story = new string[0];

    void OnEnable() { Current = this; }
    void OnDisable() { if (Current == this) Current = null; }
}
