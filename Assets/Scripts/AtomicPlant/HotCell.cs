using UnityEngine;

/// <summary>
/// LA CELDA CALIENTE: caja de plomo con ventana de vidrio plomado y telemanipuladores. Aquí llega la cápsula del
/// <see cref="PneumaticTransfer"/>. F →
/// - Molibdeno: se carga la columna del GENERADOR de Tc-99m y se manda al hospital (<see cref="MedicalIsotopes"/>).
/// - Pelo de foca: se mide con el detector de GERMANIO: cada isótopo tiene su rayo gamma (su «huella»). Lo que se ve
///   depende de cuánto se irradió (saturación, 1 − e^(−λt)) y de cuánto se esperó (desintegración, e^(−λt)).
/// El plomo deja pasar ~1/1000 (por eso la tasa fuera es baja); manipular la muestra puede contaminar las manos.
/// </summary>
public class HotCell : MonoBehaviour, IInteractable
{
    PneumaticTransfer.Sample _sample = PneumaticTransfer.Sample.None;
    float _saturation, _irrHours, _flux, _arrived;

    // Saturación de Mo-99 en 1 g de Mo natural a 1e13 n/cm²·s: N(Mo-98)·σ·φ = 1,53e21 · 0,13e-24 · 1e13 ≈ 2,0e9 Bq.
    const float Mo99SaturationBqAt1e13 = 1.99e9f;

    // ANÁLISIS POR ACTIVACIÓN: (isótopo producido, rayo gamma keV, semivida en horas, peso relativo en el pelo).
    // Semividas y energías: tablas de nucleidos (NNDC). El peso es solo orientativo (cuánta señal da cada elemento).
    static readonly (string nuclide, string element, float keV, float halfLifeH, float weight)[] Lines =
    {
        ("Cl-38", "cloro",    1642.7f, 0.6207f, 1.0f),
        ("Na-24", "sodio",    1368.6f, 14.96f,  0.8f),
        ("K-42",  "potasio",  1524.6f, 12.36f,  0.4f),
        ("Hg-203","mercurio",  279.2f, 1118.6f, 0.6f),
        ("Se-75", "selenio",   264.7f, 2874.7f, 0.5f),
        ("Zn-65", "zinc",     1115.5f, 5853.6f, 0.5f),
    };

    public string InteractLabel => _sample == PneumaticTransfer.Sample.None ? "Celda caliente (vacía)" :
        _sample == PneumaticTransfer.Sample.Molibdeno ? "Celda caliente: cargar el generador de Tc-99m" : "Celda caliente: medir el pelo en el detector de germanio";
    public bool CanInteract => true;

    public void Receive(PneumaticTransfer.Sample s, float saturation, float irrHours, float flux)
    {
        _sample = s; _saturation = saturation; _irrHours = irrHours; _flux = flux; _arrived = Time.time;
    }

    float HoursWaited => (Time.time - _arrived) * PneumaticTransfer.MinutesPerSecond / 60f;

    public void Interact()
    {
        if (_sample == PneumaticTransfer.Sample.None) { PlantNotice.Say("La celda está vacía: manda una muestra al núcleo con el conejo neumático."); return; }
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        PersonalDosimeter d = p != null ? p.GetComponent<PersonalDosimeter>() : null;
        if (d != null && Random.value < 0.2f) d.contaminated = true;

        if (_sample == PneumaticTransfer.Sample.Molibdeno)
        {
            float bq = Mo99SaturationBqAt1e13 * _saturation * Mathf.Pow(0.5f, HoursWaited / MedicalIsotopes.Mo99HalfLifeHours);
            MedicalIsotopes.Generators.Add((bq, Time.time));
            float doses = bq / 7.4e8f;   // una gammagrafía ósea usa ~740 MBq de Tc-99m
            PlantNotice.Say($"Generador cargado: {bq / 1e6f:0.0} MBq de Mo-99 → al hospital. Da para ~{doses:0.0} gammagrafías (740 MBq cada una). " +
                            (doses < 1f ? "Poco: los reactores irradian días enteros a plena potencia." : "El tecnecio se «ordeña» cada día de la columna."));
        }
        else
        {
            var seen = new System.Text.StringBuilder();
            foreach (var l in Lines)
            {
                float lam = Mathf.Log(2f) / l.halfLifeH;
                float signal = l.weight * (_flux / 1e13f) * (1f - Mathf.Exp(-lam * _irrHours)) * Mathf.Exp(-lam * HoursWaited);
                if (signal > 0.02f) seen.Append($"{l.nuclide} ({l.keV:0.0} keV, {l.element}) ");
            }
            PlantNotice.Say(seen.Length == 0
                ? $"Espectro casi vacío: {_irrHours:0.#} h a {_flux:0.0e0} n/cm²·s no bastan. Más potencia o más tiempo."
                : $"Picos: {seen}— Lo de vida corta se ve enseguida; para ver el mercurio hay que irradiar días y esperar a que se vaya el sodio.");
        }
        _sample = PneumaticTransfer.Sample.None;
    }
}
