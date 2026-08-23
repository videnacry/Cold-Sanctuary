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
    [Tooltip("Aporte a ARMA ofensiva (colmillo/veneno/garra/aguijón): sube Anima.armament ⟂ masa (Predation). " +
             "Biológico → SÍ se modula por la vitalidad del huésped. Así 'crear el colmillo' es una parte del cuerpo.")]
    public float armament;
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
    [Tooltip("Tejido VIVO/biológico (miembro, ropa-viva): DOBLE SENTIDO — el huésped lo modula y se adapta " +
             "progresivamente. Si es false (rígido: metal/coraza inerte): aporte plano, sin modular.")]
    public bool living = true;
    [Tooltip("Capacidades/RECEPTORES que esta parte habilita (E2, docs/capabilities-and-embodiment.md §2): " +
             "ojo→\"see\", oído→\"hear\", colmillo→\"bite\", aguijón→\"sting\". Un hechizo gateado por capacidad " +
             "requiere que ALGUNA parte lo conceda → así el sentido/arma sale de la anatomía, no de un escalar.")]
    public string[] grants;

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
    float _aStr, _aAgi, _aEnd, _aMass, _aPer, _aCom, _aArm, _aWpn;

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

    /// <summary>Hechizo: **despierta** materia inerte a tejido VIVO (o al revés). La prenda pasa a doble
    /// sentido (modulada por el huésped + injerto progresivo). Encaja con "toda `Anima` es despertable".</summary>
    public void Animate(CompositionPart part, bool alive = true)
    {
        if (part != null) part.living = alive;
    }

    static void SetVisible(CompositionPart p, bool on)
    {
        if (p != null && p.visual != null) p.visual.SetActive(on);
    }

    /// <summary>¿ALGUNA parte equipada concede esta capacidad/RECEPTOR? (E2). Es la "forma de verificar tener
    /// receptor visual/auditivo/etc." para gatear los hechizos sensoriales/físicos por anatomía — lo lee
    /// `Mind.PassesGate` cuando una frase fija `gateCapability`. Ver docs/capabilities-and-embodiment.md §2.</summary>
    public bool Grants(string capability)
    {
        if (string.IsNullOrEmpty(capability)) return false;
        foreach (CompositionPart p in parts)
            if (p != null && p.grants != null)
                foreach (string g in p.grants)
                    if (g == capability) return true;
        return false;
    }

    void Update()
    {
        if (anima == null) return;

        // Vitalidad del HUÉSPED = su constitución (campos actuales MENOS nuestro delta) → modula lo biológico.
        float hostMight = ((anima.strength - _aStr) + (anima.bodyMass - _aMass) + (anima.endurance - _aEnd)) / 3f;
        float hostFactor = Mathf.Clamp(hostMight, 0.5f, 2f);
        // El AURA mágica energiza los componentes del individuo — incluso los inertes (docs/stats-as-truth §9).
        float aura = 1f + Mathf.Max(0f, anima.magicAura);

        // Objetivo: suma de aportes (los biológicos escalados por el huésped; todo escalado por el aura).
        float tStr = 0f, tAgi = 0f, tEnd = 0f, tMass = 0f, tPer = 0f, tCom = 0f, tArm = 0f, tWpn = 0f;
        foreach (CompositionPart p in parts)
        {
            if (p == null) continue;
            StatBonus b = p.bonus;
            float f = (p.living ? hostFactor : 1f) * aura;   // vivo = modulado por huésped; todo × aura (aun inerte)
            tStr += b.strength * f; tAgi += b.agility * f; tEnd += b.endurance * f;
            tMass += b.bodyMass * f; tPer += b.perception * f; tCom += b.composure * f;
            tArm += p.Armor * aura;   // el aura refuerza incluso la coraza inerte
            tWpn += b.armament * f;   // el arma (colmillo/veneno) es biológica → modulada por el huésped
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
        Step(ref anima.armament,   ref _aWpn,  tWpn,  k);
    }

    // Lleva `applied` hacia `target` (injerto progresivo) y ajusta el campo por la diferencia (gestionado).
    static void Step(ref float field, ref float applied, float target, float k)
    {
        float next = Mathf.Lerp(applied, target, k);
        field += next - applied;
        applied = next;
    }
}
