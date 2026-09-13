using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catálogo de SENTIDOS (docs/consciousness-mechanics.md §3) — "observar" no es solo la vista: es PERCIBIR por cualquier
/// receptor que el ser tenga. Base científica (el ser humano ya tiene &gt;5; los animales, más): exterocepción
/// (vista/oído/olfato/gusto/tacto/termocepción/ecolocalización/magneto/electrorrecepción) + interocepción/propiocepción/
/// vestibular/nocicepción. Cada sentido tiene un ALCANCE distinto (el olfato llega lejos; el tacto exige contacto). Un
/// ser observa a otro si tiene AL MENOS un sentido cuyo alcance lo cubre → un topo "observa" por olfato/tacto; un
/// murciélago por ecolocalización; Sakshi debilitada sigue observando por varios sentidos.
///
/// Qué sentidos tiene un ser = su CONFIGURACIÓN (los receptores que le concede su anatomía/composición — `grants`); por
/// defecto, los cinco básicos. Así "observar por config" encaja con el modelo "todo es Anima, la config restringe".
/// </summary>
public static class Senses
{
    // sentido → alcance de percepción (m). Ajustable.
    static readonly Dictionary<string, float> Range = new Dictionary<string, float>
    {
        { "vista",       12f },
        { "oido",        15f },
        { "olfato",      20f },   // el que más lejos llega
        { "gusto",        1f },   // contacto (probar)
        { "tacto",        1.5f }, // contacto/cercanía
        { "termo",        8f },   // termocepción: calor de un cuerpo
        { "eco",         10f },   // ecolocalización
        { "magneto",     30f },   // campo geomagnético (orientación) — muy amplio, poco específico
        { "electro",      6f },   // electrorrecepción (presa cercana)
        { "propio",       0f },   // propiocepción: el propio cuerpo (no percibe a OTROS)
        { "vestibular",   0f },   // equilibrio (propio)
        { "intero",       0f },   // interocepción: estado interno propio (hambre/latido)
        { "nocicepcion",  0f },   // dolor (propio)
    };

    /// <summary>Alcance de un sentido (0 = interno, no percibe a otros). Desconocido → 0.</summary>
    public static float RangeOf(string sense)
        => sense != null && Range.TryGetValue(sense, out float r) ? r : 0f;

    /// <summary>El mayor alcance EXTEROCEPTIVO entre un conjunto de sentidos (para saber hasta dónde "observa" a otros).</summary>
    public static float BestReach(IEnumerable<string> senses)
    {
        float best = 0f;
        if (senses != null) foreach (string s in senses) best = Mathf.Max(best, RangeOf(s));
        return best;
    }

    /// <summary>Los cinco básicos (por defecto, si el ser no declara sus receptores).</summary>
    public static readonly string[] Basic = { "vista", "oido", "olfato", "gusto", "tacto" };

    // ── Base científica: la MODALIDAD por TRANSDUCCIÓN (qué energía física convierte el receptor) ──────────────────
    // La biología clasifica los sentidos por su transducción: FOTOrrecepción (luz), MECANOrrecepción (presión/vibración
    // → tacto/oído/propio/vestibular/ecolocalización), QUIMIOrrecepción (químicos → olfato/gusto), TERMOrrecepción,
    // NOCIcepción (daño), ELECTROrrecepción, MAGNETOrrecepción, e INTEROcepción (estado interno). Fundamenta los órganos
    // (y las QUIMERAS/sinestesia: una "lengua que saborea la luz" = transducción QUÍMICA que deriva un percepto de Color
    // — sustitución sensorial, real). El percepto (PerceptChannel) es lo DERIVADO; la transducción es el CÓMO.
    public enum Transduction { Photo, Mechano, Chemo, Thermo, Nociception, Electro, Magneto, Interoception }

    static readonly Dictionary<string, Transduction> Mode = new Dictionary<string, Transduction>
    {
        { "vista", Transduction.Photo },
        { "oido", Transduction.Mechano }, { "tacto", Transduction.Mechano }, { "eco", Transduction.Mechano },
        { "propio", Transduction.Mechano }, { "vestibular", Transduction.Mechano },
        { "olfato", Transduction.Chemo }, { "gusto", Transduction.Chemo },
        { "termo", Transduction.Thermo }, { "nocicepcion", Transduction.Nociception },
        { "electro", Transduction.Electro }, { "magneto", Transduction.Magneto },
        { "intero", Transduction.Interoception },
    };

    /// <summary>La transducción (clase de receptor real) de un sentido. Desconocido → Mechano (el más común).</summary>
    public static Transduction TransductionOf(string sense)
        => sense != null && Mode.TryGetValue(sense, out Transduction t) ? t : Transduction.Mechano;
}
