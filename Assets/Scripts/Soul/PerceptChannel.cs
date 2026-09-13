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
    Color,        // vista — banda VISIBLE (los dicrómatas como el perro la captan con MENOS matices)
    ColorUV,      // ultravioleta: abejas/aves/renos lo ven; marcas UV (flores) solo detectables por quien tiene esta banda
    ColorInfra,   // infrarrojo/calor-como-color: serpientes; solapa con termocepción
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

/// <summary>Nivel del canal (base científica estímulo↔percepto↔affordance): el ESTÍMULO crudo que transduce el receptor
/// (Color/Sonido/Olor…), el PERCEPTO derivado por el cerebro (Distancia/Identidad — el estímulo no basta, cf. la sala
/// con niebla) y la AFFORDANCE (Gibson: lo que el entorno OFRECE hacer — Amenaza/Emoción son de este tipo).</summary>
public enum PerceptKind { Stimulus, Derived, Affordance }

public static class Percept
{
    /// <summary>¿Es un canal GENERAL (lo da casi cualquier sentido)?</summary>
    public static bool IsGeneral(PerceptChannel c)
        => c == PerceptChannel.Presence || c == PerceptChannel.Distance
        || c == PerceptChannel.Direction || c == PerceptChannel.Movement;

    /// <summary>Nivel del canal: estímulo crudo / percepto derivado / affordance (docs/consciousness-mechanics §3).</summary>
    public static PerceptKind Kind(PerceptChannel c)
    {
        switch (c)
        {
            case PerceptChannel.Color: case PerceptChannel.ColorUV: case PerceptChannel.ColorInfra:
            case PerceptChannel.Odor: case PerceptChannel.Flavor:
            case PerceptChannel.Sound: case PerceptChannel.Texture: case PerceptChannel.Temperature:
            case PerceptChannel.Bioelectric:
                return PerceptKind.Stimulus;                      // lo que el RECEPTOR transduce
            case PerceptChannel.Threat: case PerceptChannel.Emotion:
                return PerceptKind.Affordance;                   // lo que OFRECE (Gibson): peligro/estado ajeno
            default:
                return PerceptKind.Derived;                      // Presence/Distance/Direction/Movement/Shape/Identity
        }
    }
}
