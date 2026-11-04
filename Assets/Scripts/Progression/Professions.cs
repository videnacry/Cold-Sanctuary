using System.Collections.Generic;

/// <summary>
/// LAS PROFESIONES DEL PERSONAL (Berón 2026-11-04: «el profesional de yoga es Josedrilo y también es profesional de bioconstrucción;
/// Maite es profesional de cocina, Shalva es profesional de cultivo y bioconstrucción, Adaguchi es profesional de submarinismo y
/// cocina, Amahuz es profesional de cocina y química; Karen es profesional de asuntos legales y lenguas; Alicita es profesional de
/// contabilidad y project-manager»; docs/personajes-mesocosmos.md §Profesiones).
/// La profesión da a cada uno su papel en la jornada sin quitarle la rotación (regla J2, todos aprenden todo):
///  · <see cref="WorkRoster"/> le da cada día, ANTES de las vueltas, UNA misión de cada área de su profesión (la que menos ha hecho);
///  · en una misión de GRUPO que DIRIGE (la clase de yoga), el profesional del área, si está libre, entra en el grupo y la dirige;
///  · su área de salida (<see cref="SanctuaryDirector"/>) y su rótulo.
/// Las áreas son las de las misiones (<see cref="AreaFunctions"/>): «Sala de Yoga», «Construcción», «Cocina», «Huerto», «Química»,
/// «Aula», «Mecánica»…
/// </summary>
public static class Professions
{
    public struct Entry
    {
        public string who, title; public string[] areas;
        public Entry(string w, string t, params string[] a) { who = w; title = t; areas = a; }
    }

    public static readonly Entry[] All =
    {
        new Entry("Josedrilo", "maestro de yoga y bioconstrucción", "Sala de Yoga", "Construcción"),
        new Entry("Maite", "cocina", "Cocina"),
        new Entry("Shalva", "cultivo y bioconstrucción", "Huerto", "Construcción"),
        // Submarinismo: los submarinos (el de obra y la ambulancia) y las tareas bajo el agua de los árboles salen de Construcción
        // (obra y mantenimiento) y del Hospital (la ambulancia); su área de trabajo diaria es Construcción.
        new Entry("Adaguchi", "submarinismo y cocina", "Construcción", "Cocina"),
        new Entry("Amahuz", "cocina y química", "Cocina", "Química"),
        // Asuntos legales y lenguas: aún no hay área propia; su mesa está en el Aula (la sala de estudio, con mesas y biblioteca, del
        // piso del Aula), donde también da clase de lenguas (docs/piso-aula.md).
        new Entry("Karen", "asuntos legales y lenguas", "Aula"),
        // Contabilidad (las cuentas del santuario, que llevaba Nesvitor, desde el restaurante) y dirección de proyectos (las obras).
        new Entry("Alicita", "contabilidad y dirección de proyectos", "Cocina", "Construcción"),
        new Entry("Ducristin", "mecánica", "Mecánica"),
    };

    static Dictionary<string, Entry> _by;
    public static Entry? Of(string who)
    {
        if (_by == null) { _by = new Dictionary<string, Entry>(); foreach (Entry e in All) _by[e.who] = e; }
        return who != null && _by.TryGetValue(who.Replace("_Post", ""), out Entry x) ? x : (Entry?)null;
    }
    /// <summary>Las áreas de su profesión (vacío si no tiene).</summary>
    public static string[] AreasOf(string who) { Entry? e = Of(who); return e.HasValue ? e.Value.areas : new string[0]; }
    public static bool Practises(string who, string area) => System.Array.IndexOf(AreasOf(who), area) >= 0;
}
