using UnityEngine;

/// <summary>
/// El cerebro por defecto de un ser: su propia IA. Su relevancia = cuánto se "pertenece" a sí mismo; se
/// puede subir para seres poderosos, de modo que RESISTAN más la posesión (docs/anima-architecture.md
/// §11.5: "para dominar a alguien, la relevancia debe superar la del ser"). En este MVP no mueve nada por
/// sí solo — el pilar `Mind` ya piensa y la locomoción autónoma vive en `WorldCharacter`/rutina; aquí
/// queda el hueco para engancharla cuando se unifique el movimiento IA.
/// </summary>
public class AiBrain : MonoBehaviour, IBrain
{
    [Tooltip("Relevancia base con la que el ser se reclama a sí mismo. La posesión debe superarla para " +
             "tomar el mando. Súbela en seres poderosos (jefes) para que sean más difíciles de poseer.")]
    public float selfRelevance = 1f;

    public float Relevance => selfRelevance;
    public string BrainName => "IA";

    public void Act(AnimaController ctrl)
    {
        // El pilar Mind piensa solo; la locomoción autónoma vive en WorldCharacter/rutina.
        // Hueco para enganchar aquí el movimiento de IA cuando se unifique.
    }
}
