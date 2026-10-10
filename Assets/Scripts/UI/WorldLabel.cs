using UnityEngine;

/// <summary>Etiqueta de nombre para un objeto NO-anima (mueble/ingrediente/utensilio/estación). La dibuja
/// <see cref="AnimaNameplate"/> junto a los nombres de las animas, para que el jugador vea a qué dirigir sus
/// hechizos por teclado (p. ej. walkTo({destiny:"Nevera"})). docs/typed-spells-and-queues.md.</summary>
public class WorldLabel : MonoBehaviour
{
    public string text = "";
    public float height = 0.8f;   // altura sobre el objeto donde se ancla la etiqueta
}
