using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LEY "todo es un superhechizo" (docs/spells-suprahechizo-and-thoughts, stats-as-truth §Hechizos): cada acción natural
/// (caminar, trepar, comer…) es un <see cref="SuperSpell"/> = una COLA de sub-pasos (<see cref="QueuedSpell"/>). Todos
/// difieren solo en el NÚMERO de pasos. Los pasos COMPARTIDOS viven aquí como **factories**: así, cuando un paso se
/// parte mañana en varios, **todos los superhechizos que lo referencian heredan** la nueva descomposición sin tocarlos.
///
/// El paso raíz compartido es <see cref="GetUp"/> («levantarse»): hoy es 1 paso; muchos superhechizos lo encabezan
/// (caminar, trepar, comer). Se salta sin animación si el ser ya está de pie (<see cref="QueuedSpell.IsSatisfied"/>).
/// </summary>
public static class ActionSteps
{
    /// <summary>«levantarse»: ponerse de pie si estás caído. 1 paso HOY; si se parte en varios, quien lo referencie lo hereda.</summary>
    public static IEnumerable<QueuedSpell> GetUp() { yield return new GetUpStep(); }

    /// <summary>«acercarse»: caminar hasta un objetivo (reutiliza <see cref="WalkToSpell"/>).</summary>
    public static IEnumerable<QueuedSpell> ApproachTo(Transform target, float stop = 0.6f, bool run = false)
    {
        yield return new WalkToSpell { target = target, stopDistance = stop, maxSpeed = run };
    }
}

/// <summary>
/// Factories de SUPERHECHIZOS de movimiento. Cada uno compone pasos compartidos (<see cref="ActionSteps"/>): caminar =
/// levantarse + acercarse; trepar = levantarse + acercarse a la pared + ascender. Reutilizables por IA (deseos) y jugador.
/// </summary>
public static class ActionSpells
{
    /// <summary>CAMINAR = levantarse → acercarse al destino. (El andar en sí lo resuelve WalkToSpell/WalkSpell/Nav.)</summary>
    public static SuperSpell Walk(Transform target, bool run = false)
    {
        SuperSpell s = new SuperSpell { name = "caminar" };
        s.steps.AddRange(ActionSteps.GetUp());
        s.steps.AddRange(ActionSteps.ApproachTo(target, 0.6f, run));
        return s;
    }

    /// <summary>TREPAR = levantarse → acercarse a la pared trepable → ascender hasta su cima (gateado por fuerza).</summary>
    public static SuperSpell Climb(Climbable climbable, bool run = false)
    {
        SuperSpell s = new SuperSpell { name = "trepar" };
        if (climbable == null) return s;
        s.steps.AddRange(ActionSteps.GetUp());
        s.steps.AddRange(ActionSteps.ApproachTo(climbable.transform, 1.2f, run));   // llegar al pie de la pared
        s.steps.Add(new ClimbStep { climbable = climbable });
        return s;
    }
}

/// <summary>
/// «LEVANTARSE» — ponerse de pie. Se salta sin animación si el ser ya está de pie (IsSatisfied). Si está caído
/// (<see cref="Anima.prone"/>), tarda un instante en incorporarse y luego lo marca de pie. Paso raíz compartido.
/// </summary>
public class GetUpStep : QueuedSpell
{
    float _until = -1f;
    public override string Label => "levantarse";

    // Ya de pie → el superhechizo lo consume sin ejecutar (así "comer/caminar" no pierden tiempo si no te derribaron).
    public override bool IsSatisfied(Anima self) => self == null || !self.prone;

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        if (_until < 0f) _until = Time.time + 0.4f;       // breve incorporación
        if (Time.time < _until) return false;
        self.prone = false;
        return true;
    }
}

/// <summary>
/// «TREPAR» (ascender): sube al ser por una <see cref="Climbable"/> hasta su `topY`, gateado por fuerza (como
/// <see cref="PlayerClimber"/>). Se salta si ya está arriba. Mueve la Y del transform (la física fina la afina el rig).
/// </summary>
public class ClimbStep : QueuedSpell
{
    public Climbable climbable;
    public float speed = 1.5f;
    public float minStrength = 0.8f;
    public override string Label => "trepar";

    public override bool IsSatisfied(Anima self)
        => climbable == null || (self != null && self.transform.position.y >= climbable.topY - 0.05f);

    public override bool Tick(Anima self)
    {
        if (self == null || climbable == null) return true;
        if (self.strength < minStrength)
        { Debug.Log($"[Trepar] «{self.name}» no tiene fuerza para trepar ({self.strength:0.00}<{minStrength})."); return true; }
        Vector3 p = self.transform.position;
        p.y = Mathf.MoveTowards(p.y, climbable.topY, speed * Time.deltaTime);
        self.transform.position = p;
        return p.y >= climbable.topY - 0.01f;
    }
}
