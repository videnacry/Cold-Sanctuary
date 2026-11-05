using System.Collections.Generic;

/// <summary>
/// CATÁLOGO DE MISIONES DE TRABAJO por área (docs/jornada-y-rotacion.md; base: docs/area-missions-spec.md, los
/// «simulacros» de cada área). Cada día el <c>WorkdayDirector</c> pide la lista del día: las OBLIGATORIAS (el área debe
/// estar cubierta todos los días) y las demás. Construcción añade, por cada obra abierta, llevar el material del día y
/// trabajarlo, y una reparación del día en alguna área humana. Las claves de área son las de <see cref="SanctuaryArea.displayName"/>.
/// </summary>
public static class AreaWork
{
    public const string Guard = "Guardia";   // no es un SanctuaryArea: la patrulla, los puestos de vigía y el alimentador
    public const string Containment = "Laboratorio de contención";   // un sitio, no un SanctuaryArea (docs/contencion-arbol-cazador.md)
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
        foreach (ConstructionProject p in projects)
        {
            if (p == null || p.Finished) continue;
            open++;
            var carry = M("Construcción", "obra_material_" + p.title, $"Llevar el material del día a la obra «{p.title}»", 1f, true); carry.site = p.title;   // del almacén a la obra
            l.Add(carry);
            // El EQUIPO del día: una misión por persona, hechas a la vez (mismo grupo); el día de obra cuenta cuando trabajan todas.
            for (int k = 0; k < UnityEngine.Mathf.Max(1, p.crewNeeded); k++)
            {
                var work = M("Construcción", "obra_trabajo_" + p.title + (k == 0 ? "" : $"#{k + 1}"), $"Trabajar la obra «{p.title}» (estructura de guadua, bahareque y paneles)", 2f, true);
                work.site = p.title; work.group = "obra_" + p.title; l.Add(work);
            }
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
