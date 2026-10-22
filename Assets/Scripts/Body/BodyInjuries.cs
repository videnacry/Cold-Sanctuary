using System.Collections;
using UnityEngine;

/// <summary>
/// HERIDAS PERMANENTES de un cuerpo (secuelas curadas: amputación, ojo dañado, cojera, sordera). Bajan las aptitudes
/// que cada herida afecta de verdad y quedan como DATA para que el cuerpo de cubos las dibuje (le falta el antebrazo,
/// el ojo cerrado, la marcha coja). Se aplica una sola vez, un frame después de que la mezcla del alma
/// (`SoulComposition`) o la especie fijen las aptitudes, para no ser pisada.
///
/// Su primer uso es SHANIDAR (el anciano frágil de la historia de Ambrosio en la enfermería): el neandertal Shanidar 1
/// vivió hasta viejo con el orbital izquierdo aplastado (visión reducida), sin antebrazo ni mano derechos, con la marcha
/// alterada y con pérdida auditiva de conducción, lo que solo se explica porque su grupo lo cuidó
/// (Trinkaus y Villotte 2017, doi 10.1371/journal.pone.0186684).
/// </summary>
[RequireComponent(typeof(Anima))]
public class BodyInjuries : MonoBehaviour
{
    [Tooltip("Le falta el antebrazo y la mano derechos: no puede cargar ni agarrar con ese lado.")]
    public bool missingRightForearm;
    [Tooltip("Ojo izquierdo dañado (visión reducida de ese lado).")]
    public bool blindLeftEye;
    [Tooltip("Cojera: marcha alterada, más lenta.")]
    public bool limp;
    [Tooltip("Pérdida auditiva (sordera de conducción): oye peor las amenazas.")]
    public bool hearingLoss;

    // Cuánto quita cada herida (fracción de la aptitud). [A] aproximación de juego.
    const float EyeLoss = 0.15f, EarLoss = 0.2f, LimpLoss = 0.4f, ArmStrengthLoss = 0.3f;

    public bool Applied { get; private set; }

    /// <summary>¿Puede usar esta mano? (cargar, agarrar). El cuerpo de cubos y los hechizos de manos pueden leerlo.</summary>
    public bool CanUseRightHand => !missingRightForearm;

    IEnumerator Start()
    {
        yield return null;   // después de SoulComposition/SpeciesBody
        Apply(GetComponent<Anima>());
    }

    public void Apply(Anima a)
    {
        if (a == null || Applied) return;
        Applied = true;
        float per = 1f;
        if (blindLeftEye) per -= EyeLoss;
        if (hearingLoss) per -= EarLoss;
        a.perception *= Mathf.Max(0.1f, per);
        if (limp) a.agility *= 1f - LimpLoss;
        if (missingRightForearm) a.strength *= 1f - ArmStrengthLoss;
    }

    /// <summary>Descripción corta de las heridas (para la ficha/HUD).</summary>
    public string Describe()
    {
        var parts = new System.Collections.Generic.List<string>();
        if (missingRightForearm) parts.Add("sin antebrazo derecho");
        if (blindLeftEye) parts.Add("ve mal del ojo izquierdo");
        if (limp) parts.Add("cojea");
        if (hearingLoss) parts.Add("oye poco");
        return string.Join(", ", parts);
    }
}
