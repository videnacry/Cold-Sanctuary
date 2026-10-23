using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ISÓTOPOS PARA EL HOSPITAL (no hay otro a miles de km): lo que la planta le da. Mo-99 (T½ 65,94 h) se desintegra en
/// Tc-99m (T½ 6,01 h), el isótopo de la mitad de las pruebas de medicina nuclear; el «generador» de Tc-99m es una
/// columna de Mo-99 de la que se «ordeña» el tecnecio cada día. El hospital lee este almacén.
/// </summary>
public static class MedicalIsotopes
{
    /// <summary>Generadores de Tc-99m entregados: (actividad de Mo-99 en Bq, momento de la entrega).</summary>
    public static readonly List<(float bq, float at)> Generators = new List<(float, float)>();

    public const float Mo99HalfLifeHours = 65.94f;

    /// <summary>Lotes de FDG (F-18) entregados por el ciclotrón: (actividad en Bq, momento). Caducan en horas.</summary>
    public static readonly List<(float bq, float at)> Fdg = new List<(float, float)>();

    /// <summary>Actividad de Mo-99 que queda ahora (cae a la mitad cada 65,94 h de juego comprimido).</summary>
    public static float Mo99Now(float hoursPerSecond)
    {
        float sum = 0f;
        foreach (var g in Generators) sum += g.bq * Mathf.Pow(0.5f, (Time.time - g.at) * hoursPerSecond / Mo99HalfLifeHours);
        return sum;
    }
}

/// <summary>
/// EL «CONEJO» (sistema de transferencia neumática): una cápsula que el aire empuja por un tubo hasta el núcleo y de
/// vuelta. F → eliges muestra y la mandas; F otra vez → la recuperas y va a la celda caliente. Mientras está dentro,
/// la ACTIVIDAD crece hacia la saturación: A = N·σ·φ·(1 − e^(−λt)). La física de la activación con números reales:
/// - Blanco de MOLIBDENO (1 g): Mo-98 (24,39 % del Mo natural, σ = 0,13 b) + n → Mo-99, para el hospital.
/// - PELO DE FOCA (análisis por activación neutrónica): cada elemento da un isótopo con su rayo gamma propio; según
///   cuánto se irradia y cuánto se espera, asoman unos u otros (lo corto primero; el mercurio, tras días).
/// El tiempo de irradiación se COMPRIME: 1 s real = 10 min (un día ≈ 2,4 min) (<see cref="MinutesPerSecond"/>).
/// </summary>
public class PneumaticTransfer : MonoBehaviour, IInteractable
{
    public enum Sample { None, Molibdeno, PeloFoca }

    public const float MinutesPerSecond = 10f;
    public HotCell hotCell;

    public Sample inCore = Sample.None;
    public float irradiatedSaturation;   // Mo-99: fracción de saturación integrada con la potencia real
    public float irradiatedHours;        // horas (comprimidas) dentro del núcleo
    public float meanFlux;               // flujo medio (n/cm²·s) durante la irradiación

    float _fluxSum;

    public string InteractLabel => inCore == Sample.None ? "Mandar una muestra al núcleo (conejo neumático)" : $"Recuperar la cápsula ({Name(inCore)}, {irradiatedHours:0.#} h dentro)";
    public bool CanInteract => !ChoiceDialog.IsOpen;

    public static string Name(Sample s) => s == Sample.Molibdeno ? "blanco de molibdeno" : s == Sample.PeloFoca ? "pelo de foca" : "nada";

    public void Interact()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && p.GetComponent<PersonalDosimeter>() == null) { PlantNotice.Say("Zona controlada: coge antes el dosímetro en la taquilla de la entrada."); return; }
        if (inCore != Sample.None) { Retrieve(); return; }
        ChoiceDialog.Show("Conejo neumático: ¿qué muestra?",
            new[] { "Blanco de molibdeno (1 g) → Mo-99 para el hospital", "Pelo de foca → análisis por activación (¿qué elementos lleva?)" },
            i => Send(i == 0 ? Sample.Molibdeno : Sample.PeloFoca), null, transform, 3f);
    }

    void Send(Sample s)
    {
        inCore = s; irradiatedSaturation = 0f; irradiatedHours = 0f; _fluxSum = 0f; meanFlux = 0f;
        float power = ResearchReactor.Instance != null ? ResearchReactor.Instance.power : 0f;
        PlantNotice.Say(power < 1e3f ? $"Cápsula en el núcleo. Ojo: el reactor está a {ResearchReactor.FormatPower(power)}; sin neutrones no se activa nada."
                                     : $"Cápsula en el núcleo a {ResearchReactor.FormatPower(power)}. Cada segundo real son {MinutesPerSecond:0} min de irradiación.");
    }

    void Update()
    {
        if (inCore == Sample.None) return;
        float hours = Time.deltaTime * MinutesPerSecond / 60f;
        float flux = ResearchReactor.Instance != null ? ResearchReactor.Instance.Flux : 0f;
        float lam = Mathf.Log(2f) / MedicalIsotopes.Mo99HalfLifeHours;
        // dS/dt = (φ/φref)·λ − λ·S  →  S tiende a la saturación a ese flujo (φref = 1e13, 1 MW).
        irradiatedSaturation += (flux / 1e13f - irradiatedSaturation) * lam * hours;
        irradiatedHours += hours;
        _fluxSum += flux * hours; meanFlux = irradiatedHours > 0f ? _fluxSum / irradiatedHours : 0f;
    }

    void Retrieve()
    {
        if (hotCell == null) hotCell = FindFirstObjectByType<HotCell>();
        if (hotCell != null) hotCell.Receive(inCore, irradiatedSaturation, irradiatedHours, meanFlux);
        PlantNotice.Say($"La cápsula vuelve por el tubo a la celda caliente ({Name(inCore)}, {irradiatedHours:0.#} h a un flujo medio de {meanFlux:0.0e0} n/cm²·s).");
        inCore = Sample.None;
    }
}
