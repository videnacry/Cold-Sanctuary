using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Driver de PRUEBA (OnGUI) del bucle de magia para el sandbox `Magia_AUTO` (docs/testing-checklist.md §15b/c/f).
/// No necesita un `Anima` concreto: los componentes de magia son null-safe en `anima`. Botones: aprender el 1er
/// hechizo (desbloquea las pools), comer (rellena reservas vía `Metabolism`), consumir/transformar quarks, lanzar
/// fuego (químico / masa-energía). Muestra las reservas por elemento + energía + quarks. Solo para el sandbox.
/// </summary>
public class MagicSandboxDriver : MonoBehaviour
{
    public MagicReserves reserves;
    public Grimoire grimoire;
    public QuarkReserve quarks;
    public FireSpell fire;
    public ElementalSpell elemental;
    public Metabolism metabolism;
    public Anima anima;
    public SupplySpell supply;
    public Anima supplyTarget;
    public MagicReserves targetReserves;

    void Awake()
    {
        if (reserves == null) reserves = GetComponent<MagicReserves>();
        if (grimoire == null) grimoire = GetComponent<Grimoire>();
        if (quarks == null) quarks = GetComponent<QuarkReserve>();
        if (fire == null) fire = GetComponent<FireSpell>();
        if (elemental == null) elemental = GetComponent<ElementalSpell>();
        if (metabolism == null) metabolism = GetComponent<Metabolism>();
        if (anima == null) anima = GetComponent<Anima>();
        if (supply == null) supply = GetComponent<SupplySpell>();
    }

    void OnGUI()
    {
        int x = 10, y = 300;
        GUI.Box(new Rect(x, y, 372, 486), "Magia_AUTO (prueba — docs testing-checklist §19)");
        y += 26;
        bool unlocked = reserves != null && reserves.unlocked;

        if (grimoire != null)
        {
            GUI.Label(new Rect(x + 8, y, 356, 20), $"Pools: {(unlocked ? "DESBLOQUEADAS" : "bloqueadas (aprende el 1er hechizo)")}");
            y += 22;
            if (!unlocked && GUI.Button(new Rect(x + 8, y, 220, 24), "Aprender 1er hechizo (awaken)"))
                grimoire.Learn(grimoire.awakenSpellId);
            y += 28;
        }

        if (metabolism != null && GUI.Button(new Rect(x + 8, y, 165, 24), "Comer carne (×50)"))
            metabolism.AbsorbFood(50f, OrganicMaterial.Meat);
        if (metabolism != null && GUI.Button(new Rect(x + 180, y, 165, 24), "Comer fruta (×50)"))
            metabolism.AbsorbFood(50f, OrganicMaterial.Fruit);
        y += 28;

        if (quarks != null && GUI.Button(new Rect(x + 8, y, 165, 24), "Consumir quarks (+1 g)"))
            quarks.AddGrams(1.0);
        if (quarks != null && reserves != null && GUI.Button(new Rect(x + 180, y, 165, 24), "Quarks→C (10 g)"))
            quarks.MakeElement(reserves, "C", 10f);
        y += 28;
        if (quarks != null && reserves != null && GUI.Button(new Rect(x + 8, y, 337, 24), "Restituir energía (aniquilar 0,001 g → J)"))
            quarks.Restitute(reserves, 0.001);
        y += 30;

        if (fire != null && GUI.Button(new Rect(x + 8, y, 100, 24), "Chispa"))
        { fire.SetTier(FireTier.Spark); fire.Cast(); }
        if (fire != null && GUI.Button(new Rect(x + 112, y, 110, 24), "Lanzallamas"))
        { fire.SetTier(FireTier.Flamethrower); fire.Cast(); }
        if (fire != null && GUI.Button(new Rect(x + 226, y, 130, 24), "Aliento de dragón"))
        { fire.SetTier(FireTier.DragonBreath); fire.Cast(); }
        y += 30;

        if (elemental != null && GUI.Button(new Rect(x + 8, y, 108, 24), "Agua"))
        { elemental.SetElement(SpellElement.Water); elemental.Cast(); }
        if (elemental != null && GUI.Button(new Rect(x + 120, y, 108, 24), "Tierra"))
        { elemental.SetElement(SpellElement.Earth); elemental.Cast(); }
        if (elemental != null && GUI.Button(new Rect(x + 232, y, 108, 24), "Viento"))
        { elemental.SetElement(SpellElement.Wind); elemental.Cast(); }
        y += 28;

        if (reserves != null && GUI.Button(new Rect(x + 8, y, 337, 24), "Cargar reservas de prueba (H/C/O/Si +100)"))
        { reserves.Store("H", 100f); reserves.Store("C", 100f); reserves.Store("O", 100f); reserves.Store("Si", 100f); }
        y += 28;

        // Los topes DEPENDEN de los stats (stats-as-truth): subir stats → suben capPerElement/energyCap.
        if (anima != null && GUI.Button(new Rect(x + 8, y, 337, 24), "Subir stats de prueba (+0.5 razón/memoria/masa/aguante)"))
        { anima.reasoning += 0.5f; anima.memory += 0.5f; anima.bodyMass += 0.5f; anima.endurance += 0.5f; anima.strength += 0.5f; }
        y += 28;

        // Abastecer/trasplante a otro personaje (rol healer): energía + quarks + algo de C.
        if (supply != null && supplyTarget != null && GUI.Button(new Rect(x + 8, y, 337, 24), "Abastecer objetivo (1e6 J + 5 g quarks + 50 C)"))
        {
            supply.energyToGive = 1_000_000f;
            supply.quarkGramsToGive = 5f;
            supply.elementsToGive = new List<ElementCost> { new ElementCost { symbol = "C", amount = 50f } };
            supply.SupplyTo(supplyTarget);
        }
        y += 30;

        if (reserves != null)
        {
            StringBuilder sb = new StringBuilder("Reservas: ");
            if (reserves.reserves != null)
                foreach (ElementAmount e in reserves.reserves)
                    if (e != null) sb.Append($"{e.symbol}={e.amount:0.#}  ");
            GUI.Label(new Rect(x + 8, y, 356, 20), sb.ToString());
            y += 22;
            string q = quarks != null ? $"{quarks.GramsAvailable:0.###} g" : "-";
            GUI.Label(new Rect(x + 8, y, 356, 20), $"Energía: {reserves.energy:0} J    Quarks: {q}");
            y += 20;
            GUI.Label(new Rect(x + 8, y, 356, 20),
                $"Topes (de stats): {reserves.EffectiveCapPerElement:0.#} g/elem   {reserves.EffectiveEnergyCap:0} J");
            y += 22;
        }
        if (anima != null)   // con Anima real: comer sube stats (Constitution, gradual) y el exceso → grasa
        {
            GUI.Label(new Rect(x + 8, y, 356, 20),
                $"Stats: fuerza {anima.strength:0.00}  masa {anima.bodyMass:0.00}  grasa {anima.fatReserves:0.00}");
            y += 20;
        }
        if (targetReserves != null)
            GUI.Label(new Rect(x + 8, y, 356, 20), $"Objetivo → energía {targetReserves.energy:0} J, C {targetReserves.Get("C"):0.#}");
    }
}
