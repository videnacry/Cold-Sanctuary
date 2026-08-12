using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hechizo de **TRANSFORMACIÓN por combate de stats** (docs/stats-as-truth.md §4). Transforma a un `Anima`
/// (o a sí mismo) en una <see cref="TransformPreset"/>. **3 niveles** según la potencia del lanzador vs el
/// coste:
///   1) potencia ≥ **coste** → **cuerpo Y stats** (real);
///   2) potencia ≥ **resistencia** (pero &lt; coste) → **solo visual** (farol: conserva sus stats);
///   3) potencia &lt; resistencia → **falla**.
/// **Coste** = poder *inyectado* (subir la forma por encima del objetivo) + *resistencia* (stats del objetivo
/// vs los del lanzador). **Bidireccional**: bajar stats = debilitar (barato). A sí mismo: resistencia 0 →
/// siempre al menos visual. La **duración** la fija la energía del hechizo; al expirar, **revierte**.
/// "Huelen los stats": el visual-only es un **farol** (los demás siguen leyendo los stats reales); el real los
/// cambia. Se cablea en `Anima`s reales (jugador/companions); no hay sandbox (Anima es abstracta).
/// </summary>
public class TransformationSpell : MonoBehaviour
{
    public Anima caster;
    [Tooltip("Potencia extra del hechizo (energía/nivel).")]
    public float spellPower = 1f;
    [Tooltip("Duración (s) de la transformación (la fija la energía del hechizo).")]
    public float duration = 20f;

    [Tooltip("Coste del hechizo en elementos (se paga de MagicReserves del lanzador si la tiene). Vacío = gratis.")]
    public List<ElementCost> cost = new List<ElementCost>();
    [Tooltip("Coste en ENERGÍA (julios): la activación/canalización del hechizo, además de la materia. 0 = gratis.")]
    public float energyCost = 0f;

    public enum Result { Failed, VisualOnly, Full }

    void Awake() { if (caster == null) caster = GetComponent<Anima>(); }

    // Potencia de transformar = control (compostura) + perseverancia (disciplina) + creatividad.
    static float TransformPower(Anima a) => a == null ? 1f : (a.composure + a.discipline + a.creativity) / 3f;

    public Result Cast(Anima target, TransformPreset form)
    {
        if (target == null || form == null || caster == null) return Result.Failed;
        // Coste de magia: si el lanzador tiene reservas, debe poder pagar (agotado un elemento → no hay hechizo).
        MagicReserves mr = caster.GetComponent<MagicReserves>();
        if (mr != null && !mr.Pay(cost, energyCost)) { Debug.Log($"[Transform] «{caster.name}» sin reservas (materia/energía) para el hechizo."); return Result.Failed; }

        bool self = target == caster;
        StatProfile now = StatProfile.Capture(target);
        Vector3 savedScale = target.transform.localScale;

        float resistance = self ? 0f : Mathf.Max(0f, now.Might - StatProfile.Capture(caster).Might);
        float injected = Mathf.Max(0f, form.profile.Might - now.Might);
        float totalCost = resistance + injected;
        float power = TransformPower(caster) + spellPower;

        if (power < resistance)
        {
            Debug.Log($"[Transform] «{caster.name}» no puede transformar a «{target.name}» en {form.formName}: " +
                      $"potencia {power:F1} < resistencia {resistance:F1}.");
            return Result.Failed;
        }

        ApplyVisual(target, form);
        Result r;
        if (power >= totalCost) { form.profile.ApplyTo(target); r = Result.Full; }   // cuerpo Y stats
        else r = Result.VisualOnly;                                            // farol: conserva sus stats

        Debug.Log($"[Transform] «{target.name}» → {form.formName} " +
                  $"({(r == Result.Full ? "REAL (cuerpo+stats)" : "farol (solo visual; conserva stats)")}); " +
                  $"potencia {power:F1} vs coste {totalCost:F1} (resist {resistance:F1} + inyectado {injected:F1}). " +
                  $"Revierte en {duration}s.");
        StartCoroutine(Revert(target, now, savedScale, r == Result.Full, duration, form));
        return r;
    }

    void ApplyVisual(Anima target, TransformPreset form)
    {
        target.transform.localScale = Vector3.Scale(target.transform.localScale, form.visualScale);
        if (form.formModel != null) form.formModel.SetActive(true);
    }

    IEnumerator Revert(Anima target, StatProfile saved, Vector3 savedScale, bool wasFull, float t, TransformPreset form)
    {
        yield return new WaitForSeconds(t);
        if (target == null) yield break;
        target.transform.localScale = savedScale;
        if (wasFull) saved.ApplyTo(target);
        if (form != null && form.formModel != null) form.formModel.SetActive(false);
        Debug.Log($"[Transform] «{target.name}» revierte" + (form != null ? $" de {form.formName}." : "."));
    }
}
