using System.Collections.Generic;

/// <summary>
/// REQUISITOS de los hechizos aprendibles (docs/spell-genesis-and-sanctuary-progression). Cada id puede exigir:
///   • **prerequisitos**: otros hechizos que el aprendiz debe saber antes (p. ej. para el Saludo al Sol hay que saber
///     Yoga Namaskar, y para éste el upa-yoga de cuello);
///   • **items**: ingredientes/utensilios que deben estar presentes para practicarlo (p. ej. una receta necesita sus
///     ingredientes y su sartén/olla).
/// Enseñar = el maestro sabe el id y el aprendiz cumple los prerequisitos (ver <see cref="Grimoire.CanLearn"/> y
/// <see cref="TeachSpell"/>). Centralizado aquí; añadir un hechizo nuevo es una línea más.
/// </summary>
public class SpellReq
{
    public string[] prerequisites = System.Array.Empty<string>();
    public string[] items = System.Array.Empty<string>();
}

public static class SpellCatalog
{
    static readonly string[] None = System.Array.Empty<string>();

    static readonly Dictionary<string, SpellReq> Reqs = new Dictionary<string, SpellReq>
    {
        // Progresión de yoga (cadena de prerequisitos).
        { "upa-yoga",           new SpellReq() },
        { "yoga-namaskar",      new SpellReq { prerequisites = new[] { "upa-yoga" } } },
        { "yoga",               new SpellReq { prerequisites = new[] { "yoga-namaskar" } } },   // "yoga" = Saludo al Sol
        { "ashtanga-primaria",  new SpellReq { prerequisites = new[] { "yoga" } } },
        // Oficios de cocina.
        { "clean",              new SpellReq() },
        { "stock",              new SpellReq() },
        // Recetas (los items son sus ingredientes/utensilios; ver CookingRecipe).
        { "dish:huevos-revueltos", new SpellReq { prerequisites = new[] { "clean" }, items = new[] { "huevos", "sarten", "sal" } } },
        { "dish:ensalada",         new SpellReq { prerequisites = new[] { "clean" }, items = new[] { "verduras", "limon", "bol" } } },
        { "dish:avena",            new SpellReq { prerequisites = new[] { "clean" }, items = new[] { "avena", "leche", "olla" } } },
    };

    public static string[] Prerequisites(string id) => Reqs.TryGetValue(id, out var r) ? r.prerequisites : None;
    public static string[] Items(string id) => Reqs.TryGetValue(id, out var r) ? r.items : None;
    public static bool Has(string id) => Reqs.ContainsKey(id);
}
