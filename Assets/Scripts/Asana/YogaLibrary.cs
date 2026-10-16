using System.Collections.Generic;

/// <summary>
/// REGISTRO de sesiones de yoga. Una sesión = un nombre + un constructor de su secuencia (lista de <see cref="QueuedSpell"/>).
/// Centraliza todas las prácticas para encolarlas, enseñarlas (Magnate) o montar una escena por sesión. Para añadir una
/// nueva (Ashtanga nivel 1–4, Shambhavi, upa-yoga de brazos…) basta con una entrada más aquí y su builder de secuencia.
/// </summary>
public class YogaSession
{
    public readonly string name;
    public readonly System.Func<List<QueuedSpell>> Build;
    public YogaSession(string name, System.Func<List<QueuedSpell>> build) { this.name = name; Build = build; }
}

public static class YogaLibrary
{
    public static readonly YogaSession[] All =
    {
        new YogaSession("Upa-yoga de cuello", () => UpaYoga.NeckSequence()),
        new YogaSession("Yoga Namaskar",      () => YogaNamaskar.Sequence()),
        new YogaSession("Saludo al Sol",      () => SunSalutation.Sequence()),
        new YogaSession("Ashtanga Primaria",  () => AshtangaPrimary.Sequence()),
        // Futuras (una línea c/u cuando se implementen): Ashtanga nivel 2–4, Shambhavi, upa-yoga de brazos…
    };

    public static YogaSession Find(string name)
    {
        foreach (var s in All) if (s.name == name) return s;
        return null;
    }
}
