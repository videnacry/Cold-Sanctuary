using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hechizo de ABASTECIMIENTO / "trasplante" (docs/magic-metabolism §16). El mago se llena de quarks/energía
/// (comiendo/desintegrando) y luego los **introduce en otro personaje**: rellena sus reservas (elementos +
/// energía + quarks crudos). Generaliza "alimentar" a **cualquier `Anima`** — quimeras, aliados, otro jugador,
/// uno mismo. Rol de **HEALER/soporte** en la guerra: restablece las fuerzas (materia/energía para actuar y
/// lanzar) de los compañeros. Se paga de LAS reservas del lanzador (por recurso: solo transfiere lo que puede
/// pagar). Opt-in; sin reservas no hace nada.
/// </summary>
public class SupplySpell : MonoBehaviour
{
    public Anima caster;
    [Tooltip("Energía (J) a transferir al objetivo (de la reserva del lanzador).")]
    public float energyToGive = 0f;
    [Tooltip("Materia cruda (g de quarks) a transferir al objetivo.")]
    public float quarkGramsToGive = 0f;
    [Tooltip("Elementos concretos a transferir (símbolo→gramos), de las pools del lanzador.")]
    public List<ElementCost> elementsToGive = new List<ElementCost>();

    void Awake() { if (caster == null) caster = GetComponent<Anima>(); }

    /// <summary>Transfiere del lanzador al objetivo lo que pueda pagar (por recurso). Devuelve si transfirió algo.</summary>
    public bool SupplyTo(Anima target)
    {
        if (target == null || caster == null) return false;
        MagicReserves myRes = caster.GetComponent<MagicReserves>();
        MagicReserves tgRes = target.GetComponent<MagicReserves>();
        QuarkReserve myQ = caster.GetComponent<QuarkReserve>();
        QuarkReserve tgQ = target.GetComponent<QuarkReserve>();
        bool any = false;

        // Energía: se paga del lanzador y se deposita en el objetivo (lo que no cabe en su tope, se pierde).
        if (energyToGive > 0f && myRes != null && tgRes != null && myRes.PayEnergy(energyToGive))
        {
            tgRes.StoreEnergy(energyToGive);
            any = true;
        }

        // Elementos: por cada uno, si el lanzador lo tiene, se lo pasa al objetivo (hasta el tope del objetivo).
        if (elementsToGive != null && myRes != null && tgRes != null)
            foreach (ElementCost c in elementsToGive)
                if (c != null && c.amount > 0f && myRes.Get(c.symbol) >= c.amount)
                {
                    myRes.Add(c.symbol, -c.amount);
                    tgRes.Store(c.symbol, c.amount);
                    any = true;
                }

        // Quarks (materia cruda): trasplante directo de sustrato.
        if (quarkGramsToGive > 0f && myQ != null && tgQ != null && myQ.GramsAvailable >= quarkGramsToGive)
        {
            myQ.quarks -= quarkGramsToGive * QuarkReserve.QuarksPerGram;
            tgQ.AddGrams(quarkGramsToGive);
            any = true;
        }

        int nEl = elementsToGive != null ? elementsToGive.Count : 0;
        if (any) Debug.Log($"[Abastecer] «{caster.name}» → «{target.name}»: {energyToGive:0} J, {quarkGramsToGive:0} g quarks, {nEl} elemento(s).");
        else Debug.Log($"[Abastecer] «{caster.name}»: sin recursos suficientes para abastecer a «{target.name}».");
        return any;
    }
}
