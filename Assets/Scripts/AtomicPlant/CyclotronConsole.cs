using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Consola del ciclotrón: arrancar el haz, parar y mandar el F-18, o ver la física. Pide el dosímetro.</summary>
public class CyclotronConsole : MonoBehaviour, IInteractable
{
    public Cyclotron cyclotron;
    public FdgSynthesis synthesis;

    public string InteractLabel => cyclotron != null && cyclotron.beamOn
        ? $"Ciclotrón: {cyclotron.targetBq / 1e9f:0.0} GBq de F-18 en el blanco ({cyclotron.irradiatedMin:0} min)"
        : "Consola del ciclotrón (producir F-18 para el PET)";
    public bool CanInteract => cyclotron != null && !ChoiceDialog.IsOpen;

    public void Interact()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && p.GetComponent<PersonalDosimeter>() == null) { PlantNotice.Say("Zona controlada: coge antes el dosímetro en la taquilla de la entrada."); return; }
        if (cyclotron.beamOn)
            ChoiceDialog.Show("Ciclotrón: haz encendido",
                new[] { "Parar el haz y mandar el F-18 a la celda de síntesis", "Seguir irradiando" },
                i => { if (i == 0) Deliver(); }, null, transform, 3f);
        else
            ChoiceDialog.Show("Ciclotrón: ¿qué hacemos?",
                new[] { $"Arrancar el haz ({cyclotron.beamMicroA:0} µA sobre agua enriquecida en ¹⁸O)", "Cómo funciona un ciclotrón" },
                i => { if (i == 0) cyclotron.StartBeam(); else Explain(); }, null, transform, 3f);
    }

    void Deliver()
    {
        float a = cyclotron.StopAndDeliver();
        if (synthesis == null) synthesis = FindFirstObjectByType<FdgSynthesis>();
        if (synthesis != null) synthesis.Receive(a);
    }

    static void Explain() => PlantNotice.Say(
        $"Un imán de {Cyclotron.FieldTesla:0.0} T curva los protones en círculo; dos electrodos huecos («Des») a {Cyclotron.DeeKV:0} kV los empujan " +
        $"cada vez que cruzan el hueco. Como f = qB/2πm no depende de la velocidad, una RF fija de {Cyclotron.FrequencyHz / 1e6f:0.0} MHz basta: " +
        "cuanto más rápidos, más grande el círculo, pero tardan lo mismo por vuelta. El reactor hace isótopos con neutrones; el ciclotrón, con protones.");
}
