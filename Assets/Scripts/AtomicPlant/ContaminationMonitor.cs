using UnityEngine;

/// <summary>
/// PÓRTICO DE CONTAMINACIÓN de manos y pies (salida de la zona controlada): F → te mide. Si una gota de agua de la
/// piscina o una muestra te dejó contaminado, te lavas las manos en el lavabo del pórtico y vuelves a medir. Distinguir
/// IRRADIAR (recibir radiación: no te vuelve radiactivo) de CONTAMINAR (llevar encima material radiactivo).
/// </summary>
public class ContaminationMonitor : MonoBehaviour, IInteractable
{
    public string InteractLabel => "Pórtico de contaminación (manos y pies)";
    public bool CanInteract => true;

    public void Interact()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        PersonalDosimeter d = p != null ? p.GetComponent<PersonalDosimeter>() : null;
        if (d == null) { PlantNotice.Say("Pórtico: limpio. (Sin dosímetro no deberías haber entrado a la zona controlada.)"); return; }
        if (d.contaminated)
        {
            d.contaminated = false;
            PlantNotice.Say("Pórtico: ALARMA en las manos. Te lavas con agua y jabón en el lavabo del pórtico y vuelves a medir: limpio.");
        }
        else PlantNotice.Say("Pórtico: limpio. Recibir radiación (irradiar) no te vuelve radiactivo; mancharte de material radiactivo (contaminar), sí.");
    }
}
