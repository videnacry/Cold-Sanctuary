using UnityEngine;

/// <summary>
/// Hechizo de PRUEBA (solo para los tests `Checklist*Test`): expone los ganchos `protected` de <see cref="SpellBase"/>
/// —el `powerBonus` unificado (carga/canalizar/forcejeo/decaimiento) y el subproducto/rastro— para asertarlos de forma
/// determinista, sin teclas ni física. No hace nada por sí mismo (sin `Update`); lo conduce el test.
/// </summary>
public class ChecklistProbeSpell : SpellBase
{
    /// <summary>Último bonus inyectado al SOLTAR la carga (hook `OnChargeReleased`), y cuántas veces se soltó.</summary>
    public float lastReleased;
    public int releases;

    public override bool CanCast(Anima caster, ITarget target) => true;
    public override void Cast(Anima caster, ITarget target) { }

    protected override void OnChargeReleased(float injected) { lastReleased = injected; releases++; }

    /// <summary>Un paso del bonus con intención explícita (como lo conduciría un brain).</summary>
    public void Tick(Anima c, float dt, bool charging, bool channeling) => TickPowerBonus(c, dt, charging, channeling);

    /// <summary>Un intento FALLIDO → sube el forcejeo.</summary>
    public void Fail(Anima c) => ReportResult(c, false);

    /// <summary>Vuelta inmediata a la base.</summary>
    public void ResetBonus() => ResetPowerBonus();

    /// <summary>Deposita el subproducto (con su rate-limit) como si el hechizo ACTUARA.</summary>
    public void Act() => LeaveByproduct();
}
