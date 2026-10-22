using UnityEngine;

/// <summary>
/// TAQUILLA DE DOSÍMETROS (entrada de la planta): F → coges tu dosímetro personal (o lo devuelves y lees tu dosis).
/// Es la primera regla de la planta: nadie entra a la zona controlada sin dosímetro.
/// </summary>
public class DosimeterRack : MonoBehaviour, IInteractable
{
    public string InteractLabel => Player != null && PersonalDosimeter.Of(Player) != null ? "Devolver el dosímetro (leer tu dosis)" : "Coger el dosímetro personal";
    public bool CanInteract => true;

    static Transform Player { get { GameObject p = GameObject.FindGameObjectWithTag("Player"); return p != null ? p.transform : null; } }

    public void Interact()
    {
        Transform p = Player; if (p == null) return;
        PersonalDosimeter d = PersonalDosimeter.Of(p);
        if (d == null)
        {
            p.gameObject.AddComponent<PersonalDosimeter>();
            PlantNotice.Say("Coges tu dosímetro. Distancia, tiempo y blindaje: así se protege uno de la radiación.");
            return;
        }
        if (d.contaminated) { PlantNotice.Say("Antes de devolverlo, pasa por el pórtico de contaminación y lávate las manos."); return; }
        PlantNotice.Say($"Dosis de hoy: {d.accumulatedMicroSv:0.00} µSv (una radiografía de tórax ≈ 20 µSv; límite anual de un trabajador: 20 mSv).");
        Destroy(d);
    }
}
