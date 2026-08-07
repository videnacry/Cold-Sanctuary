using System.Collections.Generic;
using UnityEngine;

/// <summary>Nivel de desintegración: define cuánta ENERGÍA se libera y su etiqueta de UI.</summary>
public enum DecompositionLevel
{
    Compound,   // compuesto → átomos (romper enlaces): catabolismo, poca energía (química).
    Atom,       // átomo → núcleo + electrones (ionización).
    Nucleus,    // núcleo → nucleones: FISIÓN, mucha energía (nuclear).
    Mass        // materia → energía: ANIQUILACIÓN (E=mc²), la máxima.
}

/// <summary>
/// TRABAJO de descomposición (docs/magic-metabolism-progression.md §14): en las cocinas de los santuarios
/// (solo la de S1 es cocina real; S2..S4 = tienda/laboratorio/planta nuclear) el obrero DESCOMPONE un lote de
/// materia → **elementos** + **energía** (julios; cada nivel suelta muchísima más: química→nuclear→masa-energía).
/// Como es un TRABAJO, lo obtenido se REPARTE:
///   • la mayor parte → **reservas de la economía** del santuario (`SanctuaryResources`: Elements/Energy),
///   • una parte (`workerCut`) → **paga** al obrero (sus pools/energía en `MagicReserves`).
/// La ENERGÍA solo la CAPTURA el obrero si ha aprendido la física de ese nivel (`Grimoire` conoce
/// `energyPhysicsId`); si no, "solo ve materia→materia" y su parte de energía va entera a la economía (se
/// revela al rotar por Mecánica/Forja). Opt-in; lo dispara el fin de la misión/minijuego de la cocina.
/// *Falta:* el minijuego de selección (elementos/quarks) que rellene `yield`/`energyJoules` antes de `Complete()`.
/// </summary>
public class DecompositionJob : MonoBehaviour
{
    [Header("Obrero (quien hace el trabajo y cobra la paga)")]
    public Anima worker;
    public MagicReserves workerReserves;
    public Grimoire workerGrimoire;

    [Header("Lote a descomponer")]
    public DecompositionLevel level = DecompositionLevel.Compound;
    [Tooltip("Elementos que contiene el lote (símbolo → gramos).")]
    public List<ElementAmount> yield = new List<ElementAmount>();
    [Tooltip("Energía (julios) que libera descomponer el lote a este nivel.")]
    public float energyJoules = 0f;

    [Header("Reparto (trabajo = economía + paga)")]
    [Range(0f, 1f)]
    [Tooltip("Fracción para el obrero; el resto → economía del santuario.")]
    public float workerCut = 0.2f;
    [Tooltip("Física que hay que haber aprendido para CAPTURAR la energía de este nivel (Grimoire). Vacío = sin gate.")]
    public string energyPhysicsId = "";

    void Awake()
    {
        if (worker == null) worker = GetComponent<Anima>();
        if (workerReserves == null && worker != null) workerReserves = worker.GetComponent<MagicReserves>();
        if (workerGrimoire == null && worker != null) workerGrimoire = worker.GetComponent<Grimoire>();
    }

    /// <summary>¿El obrero puede leer/capturar la energía de este nivel? (física aprendida).</summary>
    public bool EnergyRevealed =>
        string.IsNullOrEmpty(energyPhysicsId) || (workerGrimoire != null && workerGrimoire.Knows(energyPhysicsId));

    /// <summary>Ejecuta el reparto: paga al obrero su parte (si tiene pools) y suma el resto a la economía.
    /// Llamar al COMPLETAR la misión/minijuego de descomposición.</summary>
    public void Complete()
    {
        SanctuaryResources econ = SanctuaryResources.HasInstance || Application.isPlaying ? SanctuaryResources.Instance : null;
        bool canPayWorker = workerReserves != null && workerReserves.unlocked && workerCut > 0f;
        float cut = canPayWorker ? Mathf.Clamp01(workerCut) : 0f;

        // --- MATERIA (elementos): el obrero cobra su parte en sus pools; lo que no cupo → economía ---
        float totalMatter = 0f, workerMatter = 0f;
        foreach (ElementAmount e in yield)
        {
            if (e == null || e.amount <= 0f) continue;
            totalMatter += e.amount;
            float mine = e.amount * cut;
            if (mine > 0f) workerMatter += mine - workerReserves.Store(e.symbol, mine);   // Store devuelve el sobrante
        }
        float econMatter = totalMatter - workerMatter;
        if (econMatter > 0f && econ != null) econ.Add(SanctuaryResource.Elements, econMatter);

        // --- ENERGÍA (julios): solo la capta el obrero si aprendió la física; si no, va entera a la economía ---
        float energyCut = (canPayWorker && EnergyRevealed) ? cut : 0f;
        float mineE = energyJoules * energyCut;
        float workerEnergy = mineE > 0f ? mineE - workerReserves.StoreEnergy(mineE) : 0f;
        float econEnergy = energyJoules - workerEnergy;
        if (econEnergy > 0f && econ != null) econ.Add(SanctuaryResource.Energy, econEnergy);

        Debug.Log($"[Descomposición] {LevelLabel(level)} en «{name}»: {totalMatter:0.##} g materia, {energyJoules:0} J. " +
                  $"Obrero {workerMatter:0.##} g + {workerEnergy:0} J" +
                  (!EnergyRevealed && energyJoules > 0f ? " (energía NO revelada: falta física)" : "") +
                  $"; economía {econMatter:0.##} g + {econEnergy:0} J.");
    }

    static string LevelLabel(DecompositionLevel l)
    {
        switch (l)
        {
            case DecompositionLevel.Compound: return "catabolismo (compuesto→átomos)";
            case DecompositionLevel.Atom:     return "ionización (átomo→núcleo+e⁻)";
            case DecompositionLevel.Nucleus:  return "fisión (núcleo→nucleones)";
            case DecompositionLevel.Mass:     return "aniquilación (materia→energía, E=mc²)";
            default:                          return "descomposición";
        }
    }
}
