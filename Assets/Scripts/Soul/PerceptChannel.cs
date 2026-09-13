/// <summary>
/// CANALES DE INFORMACIÓN de la percepción (docs/consciousness-mechanics.md §3) — QUÉ se extrae al observar, con
/// independencia de POR QUÉ órgano. Hay canales **GENERALES** (casi cualquier sentido los da: que algo ESTÁ ahí, a qué
/// DISTANCIA, en qué DIRECCIÓN, si se MUEVE) y **ESPECÍFICOS** (color=vista, olor=olfato, sabor=gusto, sonido=oído,
/// textura=tacto, temperatura=termo, campo=electro…). La clave del diseño: un ÓRGANO declara qué canales genera y con
/// qué calidad (`SenseOrgan`), así que una "lengua que saborea la luz" = un órgano de gusto que además provee `Color`;
/// un "ojo ciego" = un ojo que provee `Color` con calidad 0. El canal no está atado al órgano.
/// </summary>
public enum PerceptChannel
{
    // Generales (los da casi cualquier sentido)
    Presence,     // que algo existe ahí
    Distance,     // a qué distancia
    Direction,    // en qué dirección
    Movement,     // si se mueve / hacia dónde
    // Específicos (según el receptor)
    Color,        // vista
    Shape,        // vista/tacto
    Odor,         // olfato
    Flavor,       // gusto
    Sound,        // oído
    Texture,      // tacto
    Temperature,  // termocepción
    Bioelectric,  // electrorrecepción
    // De alto nivel (interpretación)
    Identity,     // quién/qué especie es
    Emotion,      // su estado emocional (legibilidad)
    Threat,       // cuán peligroso es
}

public static class Percept
{
    /// <summary>¿Es un canal GENERAL (lo da casi cualquier sentido)?</summary>
    public static bool IsGeneral(PerceptChannel c)
        => c == PerceptChannel.Presence || c == PerceptChannel.Distance
        || c == PerceptChannel.Direction || c == PerceptChannel.Movement;
}
