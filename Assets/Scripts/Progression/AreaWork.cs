using System.Collections.Generic;

/// <summary>
/// CATÁLOGO DE MISIONES DE TRABAJO por área (docs/jornada-y-rotacion.md; base: docs/area-missions-spec.md, los
/// «simulacros» de cada área). Cada día el <c>WorkdayDirector</c> pide la lista del día: las OBLIGATORIAS (el área debe
/// estar cubierta todos los días) y las demás. Construcción añade, por cada obra abierta, llevar el material del día y
/// trabajarlo, y una reparación del día en alguna área humana. Las claves de área son las de <see cref="SanctuaryArea.displayName"/>.
/// </summary>
public static class AreaWork
{
    public const string Guard = "Guardia";   // no es un SanctuaryArea: la torre y los puestos de vigía

    static WorkRoster.Mission M(string area, string id, string title, float h, bool mandatory = false) => new WorkRoster.Mission(area, id, title, h, mandatory);

    /// <summary>Misiones fijas de cada área (sin las de obras, que dependen de qué obras haya abiertas).</summary>
    public static readonly WorkRoster.Mission[] Catalog =
    {
        M("Enfermería", "enf_iniciacion", "Iniciación de recién llegados y turistas (análisis, vacuna, primera virtualización)", 1f, true),
        M("Enfermería", "enf_visita", "Pasar visita y curas", 2f, true),
        M("Enfermería", "enf_animales", "Análisis y vacunas de animales", 1f),
        M("Enfermería", "enf_inventario", "Limpieza e inventario de la enfermería", 1f),
        M("Cocina", "coc_servicio", "Servicio del restaurante (cocinar la carta)", 2f, true),
        M("Cocina", "coc_limpieza", "Limpieza e inventario de la cocina", 1f, true),
        M("Cocina", "coc_obrador", "Elaboraciones del obrador (pan, quesos, fermentos)", 2f),
        M("Cocina", "coc_sala", "Atender a los turistas en sala", 1f),
        M("Huerto", "hue_riego", "Riego y deshierbe de los huertos", 2f, true),
        M("Huerto", "hue_cosecha", "Cosecha para la cocina", 1f),
        M("Huerto", "hue_visitantes", "Alimentar a los visitantes del huerto", 1f),
        M("Sala de Yoga", "yog_clase", "Clase de yoga para turistas", 1f, true),
        M("Sala de Yoga", "yog_sala", "Preparar y limpiar la sala de yoga", 1f),
        M("Mecánica", "mec_vehiculos", "Mantenimiento de vehículos", 2f),
        M("Mecánica", "mec_maquinas", "Reparar maquinaria de las áreas", 1f),
        M("Estudio Textil", "tex_tejer", "Tejer y teñir", 2f),
        M("Estudio Textil", "tex_coser", "Coser y reparar ropa", 1f),
        M("Carne Cultivada", "lab_biorreactor", "Supervisar los biorreactores", 1f, true),
        M("Gimnasio", "gym_mantener", "Mantenimiento del gimnasio", 1f),
        M(Guard, "gua_recienllegados", "Vigilar a los recién llegados (no salen sin iniciación)", 2f, true),
        M(Guard, "gua_carnivoros", "Patrulla y raciones a los carnívoros", 2f, true),
    };

    static readonly (string area, string what)[] Repairs =
    {
        ("Enfermería", "tubería que gotea"), ("Cocina", "enchufe del obrador"), ("Sala de Yoga", "ventana que no cierra"),
        ("Estudio Textil", "puerta descolgada"), ("Mecánica", "tejado con goteras"), ("Gimnasio", "foco fundido"),
    };

    /// <summary>Las misiones del día <paramref name="day"/>: las fijas, las de cada obra abierta y una reparación.</summary>
    public static List<WorkRoster.Mission> Today(int day, IEnumerable<ConstructionProject> projects)
    {
        var l = new List<WorkRoster.Mission>();
        foreach (var m in Catalog) l.Add(m.Copy());
        foreach (ConstructionProject p in projects)
        {
            if (p == null || p.Finished) continue;
            l.Add(M("Construcción", "obra_material_" + p.title, $"Llevar el material del día a la obra «{p.title}»", 1f, true));
            l.Add(M("Construcción", "obra_trabajo_" + p.title, $"Trabajar la obra «{p.title}»", 2f, true));
        }
        var r = Repairs[((day % Repairs.Length) + Repairs.Length) % Repairs.Length];
        l.Add(M("Construcción", "rep_" + r.area, $"Reparación en {r.area}: {r.what}", 1f));
        return l;
    }

    /// <summary>Misiones de un área (para el menú «Virtualización» de su máquina).</summary>
    public static List<WorkRoster.Mission> OfArea(string area, IEnumerable<ConstructionProject> projects, int day)
    {
        var l = new List<WorkRoster.Mission>();
        foreach (var m in Today(day, projects)) if (m.area == area) l.Add(m);
        return l;
    }
}
