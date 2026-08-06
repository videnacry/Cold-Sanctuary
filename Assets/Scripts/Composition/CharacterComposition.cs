using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Aporte ADITIVO de una parte a los stats (0 = sin cambio). Subconjunto relevante (ampliable). Los aportes
/// **biológicos** (de un miembro) se modulan por la **vitalidad** del cuerpo huésped (la química/base): un
/// cuerpo fuerte exprime más del mismo brazo. La `armor` (ropa) es externa → NO se escala por el huésped.
/// </summary>
[System.Serializable]
public class StatBonus
{
    public float strength, agility, endurance, bodyMass, perception, composure;
    [Tooltip("Aporte a armadura/coraza (ropa; no se escala por el huésped).")]
    public float armor;
}

/// <summary>
/// Una parte de composición (docs/stats-as-truth.md §5): adorno / prenda / miembro en un slot, con su visual y
/// su aporte de stats. Puede referenciar una `ClothingRecipe` (toma su slot y su `defenseRating`→armadura).
/// </summary>
[System.Serializable]
public class CompositionPart
{
    public string label = "Parte";
    public ClothingSlot slot = ClothingSlot.Accessory;
    [Tooltip("Malla a activar (opcional; los modelos viven fuera del repo).")]
    public GameObject visual;
    [Tooltip("Prenda opcional: si se asigna, usa su slot y aporta su defenseRating a la armadura.")]
    public ClothingRecipe clothing;
    [Tooltip("Aporte de stats (miembro = fuerza/agilidad…; adorno = 0; ropa = armor).")]
    public StatBonus bonus = new StatBonus();

    public ClothingSlot Slot => clothing != null ? clothing.slot : slot;
    public float Armor => bonus.armor + (clothing != null ? clothing.defenseRating : 0f);
}

/// <summary>
/// COMPOSICIÓN de un ser (docs/stats-as-truth.md §5, **fase 2**): partes slotables (adornos/ropa/miembros) que
/// dan **apariencia** y **stats**. Modelo: la **constitución** (la química/base del cuerpo = los campos de
/// `Anima`, que **evolución/transformación mutan libremente**) es el HUÉSPED; cada parte aporta un **delta
/// GESTIONADO** (resta el viejo, suma el nuevo cada frame) → **nunca pisa** a evolución/transform.
///  - **Modulación por huésped:** el aporte biológico se escala por la **vitalidad** del cuerpo (el mismo
///    brazo rinde distinto en un cuerpo fuerte que en uno débil).
///  - **Injerto progresivo:** el delta **converge** con el tiempo (`adaptSpeed`); al quitar la parte, se
///    desvanece. La armadura (ropa) es externa → no se modula.
/// Opt-in y aditivo: sin este componente, nada cambia. Fase 3: reconciliar con `BodyPartStats` y miembros
/// perdibles/injertables como assets; base proyectable desde `Humores`/`Chemistry`.
/// </summary>
public class CharacterComposition : MonoBehaviour
{
    public Anima anima;
    public List<CompositionPart> parts = new List<CompositionPart>();
    [Tooltip("Velocidad de adaptación del injerto (mayor = más rápido).")]
    public float adaptSpeed = 1.5f;

    // Delta actualmente aplicado a los campos de Anima (para sumarlo/quitarlo de forma gestionada).
    float _aStr, _aAgi, _aEnd, _aMass, _aPer, _aCom, _aArm;

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); }
    void Start() { foreach (CompositionPart p in parts) SetVisible(p, true); }

    public void Equip(CompositionPart part)
    {
        if (part == null) return;
        for (int i = parts.Count - 1; i >= 0; i--)
            if (parts[i] != null && parts[i].Slot == part.Slot) { SetVisible(parts[i], false); parts.RemoveAt(i); }
        parts.Add(part);
        SetVisible(part, true);
    }

    public void Unequip(CompositionPart part)
    {
        if (part == null) return;
        parts.Remove(part);
        SetVisible(part, false);
    }

    static void SetVisible(CompositionPart p, bool on)
    {
        if (p != null && p.visual != null) p.visual.SetActive(on);
    }

    void Update()
    {
        if (anima == null) return;

        // Vitalidad del HUÉSPED = su constitución (campos actuales MENOS nuestro delta) → modula lo biológico.
        float hostMight = ((anima.strength - _aStr) + (anima.bodyMass - _aMass) + (anima.endurance - _aEnd)) / 3f;
        float hostFactor = Mathf.Clamp(hostMight, 0.5f, 2f);

        // Objetivo: suma de aportes (los biológicos escalados por el huésped; la armadura no).
        float tStr = 0f, tAgi = 0f, tEnd = 0f, tMass = 0f, tPer = 0f, tCom = 0f, tArm = 0f;
        foreach (CompositionPart p in parts)
        {
            if (p == null) continue;
            StatBonus b = p.bonus;
            tStr += b.strength * hostFactor; tAgi += b.agility * hostFactor; tEnd += b.endurance * hostFactor;
            tMass += b.bodyMass * hostFactor; tPer += b.perception * hostFactor; tCom += b.composure * hostFactor;
            tArm += p.Armor;   // armadura (ropa): externa
        }

        // Injerto progresivo + aplicación gestionada (no pisa evolución/transformación).
        float k = Mathf.Clamp01(Time.deltaTime * adaptSpeed);
        Step(ref anima.strength,   ref _aStr,  tStr,  k);
        Step(ref anima.agility,    ref _aAgi,  tAgi,  k);
        Step(ref anima.endurance,  ref _aEnd,  tEnd,  k);
        Step(ref anima.bodyMass,   ref _aMass, tMass, k);
        Step(ref anima.perception, ref _aPer,  tPer,  k);
        Step(ref anima.composure,  ref _aCom,  tCom,  k);
        Step(ref anima.armadura,   ref _aArm,  tArm,  k);
    }

    // Lleva `applied` hacia `target` (injerto progresivo) y ajusta el campo por la diferencia (gestionado).
    static void Step(ref float field, ref float applied, float target, float k)
    {
        float next = Mathf.Lerp(applied, target, k);
        field += next - applied;
        applied = next;
    }
}
