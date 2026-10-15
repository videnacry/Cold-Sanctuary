using UnityEngine;

/// <summary>
/// Adaptador <see cref="ITarget"/> para un <see cref="Anima"/> que no lo es por sí mismo (compañeros, la Magnate,
/// seres por composición…). Solo los <c>Animal</c> y el <c>PlayerTarget</c> implementaban ITarget, así que los hechizos
/// dirigidos de la consola (bond, thought, teach, analysis, vaccinate) no encontraban a casi ningún humano. La
/// <see cref="SpellConsole"/> lo añade al vuelo al resolver el destino; los hechizos sacan el Anima del GameObject.
/// </summary>
[DisallowMultipleComponent]
public class AnimaTarget : MonoBehaviour, ITarget
{
    Anima _a;
    Anima A => _a != null ? _a : (_a = GetComponent<Anima>());

    public float Mass     => A != null ? A.BodyMass : 1f;
    public float Speed    => A != null ? A.agility : 1f;
    public char  Faction  => 'h';
    public bool  Dead     => A == null || A.death;
    public bool  Consumed => false;
    public void  Hurt(float damage) { }   // no-violencia: un hechizo de consola no hiere a un compañero
}
