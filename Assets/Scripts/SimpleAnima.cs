using UnityEngine;

/// <summary>
/// Un `Anima` CONCRETO mínimo — un ser "despertable" sin comportamiento propio (docs/anima-architecture.md:
/// todo es un `Anima`, incluso lo inanimado). Implementa los hooks abstractos como no-ops. Sirve para cablear
/// sistemas que solo necesitan los **drives/aptitudes** de la base (magia, metabolismo, composición) sin
/// arrastrar la maquinaria de `Animal`. No añade `Awake`/`Start` → **seguro de instanciar en un sandbox**.
/// </summary>
public class SimpleAnima : Anima
{
    protected override void RespondToHunger() { }
    protected override float EvaluateThreat(GameObject source) => 0f;
    public override void RespondToThreat(GameObject threat) { }
}
