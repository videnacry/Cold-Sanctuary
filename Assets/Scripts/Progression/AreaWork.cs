using System.Collections.Generic;
using System.Linq;

/// <summary>
/// CATÁLOGO DE MISIONES DE TRABAJO por área (docs/jornada-y-rotacion.md; base: docs/area-missions-spec.md, los
/// «simulacros» de cada área). Cada día el <c>WorkdayDirector</c> pide la lista del día: las OBLIGATORIAS (el área debe
/// estar cubierta todos los días) y las demás. Construcción añade, por cada obra abierta, llevar el material del día y
/// trabajarlo, y una reparación del día en alguna área humana. Las claves de área son las de <see cref="SanctuaryArea.displayName"/>.
/// </summary>
public static class AreaWork
{
    public const string Guard = "Guardia";   // no es un SanctuaryArea: la patrulla, los puestos de vigía y el alimentador
    public const string GuardRoom = "Sala de guardia";   // un sitio de la jornada, no un SanctuaryArea (docs/sala-de-guardia.md)
    public const string Hospital = "Hospital";   // la antigua Enfermería (docs/hospital.md)

    static WorkRoster.Mission M(string area, string id, string title, float h, bool mandatory = false) => new WorkRoster.Mission(area, id, title, h, mandatory);
    static WorkRoster.Mission Also(WorkRoster.Mission m, params string[] areas) { m.also = areas; return m; }   // la tarea hace ir también a esas áreas

    /// <summary>Misiones fijas de cada área (sin las de obras, que dependen de qué obras haya abiertas).</summary>
    /// <remarks>Sale del REGISTRO DE FUNCIONES (<see cref="AreaFunctions"/>, docs/microcosmos-civilizaciones.md §2): las mismas
    /// misiones y en el mismo orden de siempre, ahora definidas una vez y reutilizables por los edificios de los microcosmos.</remarks>
    public static readonly WorkRoster.Mission[] Catalog = AreaFunctions.SanctuaryMissions();

    static readonly (string area, string what)[] Repairs =
    {
        (Hospital, "tubería que gotea"), ("Cocina", "enchufe del obrador"), ("Sala de Yoga", "ventana que no cierra"),
        ("Estudio Textil", "puerta descolgada"), ("Mecánica", "tejado con goteras"), ("Gimnasio", "foco fundido"),
    };

    /// <summary>Las misiones de una obra abierta: llevar el material del día (1 h) y el EQUIPO (una misión de 2 h por turno, del mismo grupo; el día de
    /// obra cuenta cuando se han hecho todos los turnos). <paramref name="reform"/> (un área no básica en obra, <see cref="AreasEnObra"/>): opcionales,
    /// las hace quien tenga tiempo y nunca piden un clon; las obras de siempre (Dojo, Ballet) son obligatorias.</summary>
    public static List<WorkRoster.Mission> ObraMissions(string title, int crew, bool reform)
    {
        var l = new List<WorkRoster.Mission>();
        var carry = M("Construcción", "obra_material_" + title, $"Llevar el material del día a la obra «{title}»", 1f, !reform); carry.site = title;   // del almacén a la obra
        l.Add(carry);
        for (int k = 0; k < UnityEngine.Mathf.Max(1, crew); k++)
        {
            var work = M("Construcción", "obra_trabajo_" + title + (k == 0 ? "" : $"#{k + 1}"), $"Trabajar la obra «{title}» (estructura de guadua, bahareque y paneles)", 2f, !reform);
            work.site = title; work.group = "obra_" + title; l.Add(work);
        }
        foreach (var m in l) m.optional = reform;
        return l;
    }

    /// <summary>Las misiones del día <paramref name="day"/>: las fijas, las de cada obra abierta y una reparación.</summary>
    public static List<WorkRoster.Mission> Today(int day, IEnumerable<ConstructionProject> projects)
    {
        var l = new List<WorkRoster.Mission>();
        foreach (var m in Catalog) l.Add(m.Copy());
        // FRUTALES DE LOS TECHOS (docs/jardines-frutales.md): la ronda diaria, partida en tantas personas como haga falta.
        l.AddRange(FruitGardener.Missions());
        // FOTOBIORREACTORES DEL ÁRBOL (docs/fotobiorreactores.md): el cuidado diario del cultivo de espirulina (1 h, en Biología).
        l.AddRange(FotobioFacade.Missions());
        // LANZADERA DE COHETES (docs/lanzadera-de-cohetes.md): control de misión, seguridad de la plataforma y la fábrica.
        l.AddRange(LaunchSite.Missions());
        // SITIO NUCLEAR (docs/sitio-nuclear.md): vigilancia, revisión de lo pasivo y el simulacro semanal.
        l.AddRange(NuclearSite.Missions(day));
        // PARQUE ENERGÉTICO (docs/energia.md): los paneles (mantener, instalar, reparar), las turbinas y el almacén.
        l.AddRange(EnergySite.Missions(day));
        l.AddRange(HunterCell.Missions(day));   // el árbol cazador (docs/contencion-arbol-cazador.md)
        // COCINA: cada elaboración que hace falta (harina, queso, pasta, masa madre…) es una misión, con su tiempo activo;
        // el reposo (fermentar, madurar) corre solo después.
        KitchenLarder larder = KitchenLarder.Instance;
        if (larder != null)
            foreach (Gastronomy.Prep p in larder.Needed(10))
                l.Add(M("Cocina", "prep_" + p.name, $"Elaborar {p.name} ({p.equipment})", UnityEngine.Mathf.Clamp(UnityEngine.Mathf.Ceil(p.activeMin / 30f) * 0.5f, 0.5f, 2f)));
        int open = 0;
        var projectList = projects.Where(p => p != null).ToList();
        // (Berón 2026-11-09) LAS ÁREAS EN OBRA (AreasEnObra): no dan tareas de área (salvo el O₂ y el material de obra); sus horas son las de su obra.
        var enObra = new HashSet<string>(projectList.Where(p => p.reform && !p.Finished).Select(p => WorkRoster.AreaGroup(p.title)));
        if (enObra.Count > 0) l = AreasEnObra.Filter(l, enObra);
        foreach (ConstructionProject p in projectList)
        {
            if (p.Finished) continue;
            open++;
            l.AddRange(ObraMissions(p.title, p.crewNeeded, p.reform));
        }
        // MATERIAL: lo que el almacén necesita reponer (todo sale del bambusal, los huertos y el laboratorio).
        MaterialStore store = MaterialStore.Instance;
        if (store != null)
            foreach (string need in store.Needs(open))
                switch (need)
                {
                    case "guadua": l.Add(Also(M("Huerto", "mat_guadua", "Cortar guadua madura en el bambusal (se cura y trata con bórax)", 2f, true), "Construcción")); break;
                    case "bahareque": l.Add(M("Construcción", "mat_bahareque", "Preparar barro con paja para el bahareque", 1f, true)); break;
                    case "micelio": l.Add(Also(M("Carne Cultivada", "mat_micelio", "Sembrar paneles de micelio (paja y cañamiza en moldes)", 1f, true), "Construcción")); break;
                    case "cuerda": l.Add(Also(M("Estudio Textil", "mat_cuerda", "Trenzar cuerda de fibra de cáñamo", 1f, true), "Construcción")); break;
                }
        var r = Repairs[((day % Repairs.Length) + Repairs.Length) % Repairs.Length];
        l.Add(Also(M("Construcción", "rep_" + r.area, $"Reparación en {r.area}: {r.what}", 1f), r.area));   // herramientas del almacén → el área
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
