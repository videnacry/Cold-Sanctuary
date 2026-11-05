using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>
/// CELDA DE SÍNTESIS DE FDG (fluorodesoxiglucosa): el F-18 llega del blanco y un módulo automático lo une a la glucosa
/// (~25 min, rendimiento radioquímico ~60 %). Cada PET de adulto usa ~370 MBq. Lo producido se apunta en
/// <see cref="MedicalIsotopes.Fdg"/> para el hospital: el F-18 caduca rápido (T½ 110 min), por eso se hace al lado.
/// </summary>
public class FdgSynthesis : MonoBehaviour, IInteractable
{
    public const float SynthesisMin = 25f, RadiochemicalYield = 0.6f, PetDoseBq = 3.7e8f;
    float _bq, _arrived;

    public string InteractLabel => _bq > 0f ? "Celda de síntesis: hacer FDG con el F-18" : "Celda de síntesis de FDG (vacía)";
    public bool CanInteract => true;

    public void Receive(float bq) { _bq = bq; _arrived = Time.time; }

    public void Interact()
    {
        if (_bq <= 0f) { PlantNotice.Say("La celda está vacía: produce F-18 en la consola del ciclotrón y para el haz para mandarlo aquí."); return; }
        float waitedMin = (Time.time - _arrived) * PneumaticTransfer.MinutesPerSecond + SynthesisMin;
        float fdg = _bq * RadiochemicalYield * Mathf.Pow(0.5f, waitedMin / Cyclotron.F18HalfLifeMin);
        MedicalIsotopes.Fdg.Add((fdg, Time.time));
        PlantNotice.Say($"FDG lista: {fdg / 1e9f:0.0} GBq tras {waitedMin:0} min (síntesis {SynthesisMin:0} min, rendimiento {RadiochemicalYield:P0}). " +
                        $"Da para ~{fdg / PetDoseBq:0} PET de 370 MBq si llega pronto: cada 110 min queda la mitad.");
        _bq = 0f;
    }
}
