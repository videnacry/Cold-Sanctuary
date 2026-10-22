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
    static WorkRoster.Mission G(string area, string id, string title, string group) => new WorkRoster.Mission(area, id, title, 1f, true) { group = group };   // misión de grupo (a la vez)

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
        G("Sala de Yoga", "yog_guia", "Clase de yoga: guiar desde la plataforma central (objetivo 1)", "clase_yoga"),
        G("Sala de Yoga", "yog_demo", "Clase de yoga: demostrar entre los alumnos (objetivo 2)", "clase_yoga"),
        G("Sala de Yoga", "yog_corrige_1", "Clase de yoga: corregir posturas a los alumnos", "clase_yoga"),
        G("Sala de Yoga", "yog_corrige_2", "Clase de yoga: corregir posturas a los alumnos", "clase_yoga"),
        M("Cocina", "coc_atender", "Atender a los comensales en la barra (servir los pedidos)", 2f, true),
        M("Sala de Yoga", "yog_sala", "Preparar y limpiar la sala de yoga", 1f),
        M("Mecánica", "mec_vehiculos", "Mantenimiento de vehículos", 2f),
        M("Mecánica", "mec_maquinas", "Reparar maquinaria de las áreas", 1f),
        M("Estudio Textil", "tex_tejer", "Tejer y teñir", 2f),
        M("Estudio Textil", "tex_coser", "Coser y reparar ropa", 1f),
        M("Carne Cultivada", "lab_biorreactor", "Supervisar los biorreactores (glucosa, pH, oxígeno)", 1f, true),
        M("Carne Cultivada", "lab_iniciar", "Iniciar un lote: vial del banco, medio y siembra en el biorreactor", 1f, true),
        M("Carne Cultivada", "lab_cosechar", "Cosechar el lote y dar forma a la carne (cocina y comedero)", 1f, true),
        M("Carne Cultivada", "lab_limpieza", "Limpieza y esterilización del laboratorio", 1f),
        M("Cocina", "coc_comedor", "Limpieza del comedor y de los aseos", 1f, true),
        M("Sala de Yoga", "yog_aseos", "Limpieza de los aseos de la sala de yoga", 0.5f),
        M("Estudio Textil", "tex_limpieza", "Limpieza e inventario del estudio textil", 0.5f),
        M("Mecánica", "mec_limpieza", "Limpieza e inventario del taller", 0.5f),
        M("Construcción", "con_almacen", "Inventario del almacén de material y herramientas", 0.5f),
        M("Cría", "cri_tomas", "Tomas y pesaje de las crías (anotar en la pizarra)", 1f, true),
        M("Cría", "cri_recintos", "Limpiar los recintos y cambiar la paja", 1f, true),
        M("Cría", "cri_vinculo", "Cuidar una cría hasta ganar su vínculo (leer, calmar, alimentar, asear)", 2f),
        M("Gimnasio", "gym_limpieza", "Limpieza del gimnasio", 0.5f),
        M("Gimnasio", "gym_mantener", "Mantenimiento del gimnasio", 1f),
        M("Planta Atómica", "pla_operar", "Operar el reactor: arranque, subir a potencia y parada (consola)", 1f),
        M("Planta Atómica", "pla_isotopos", "Irradiar molibdeno para el generador de Tc-99m del hospital", 1f),
        M("Planta Atómica", "pla_ronda", "Ronda de la planta: resinas, bomba, torre y protección radiológica", 1f),
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
            l.Add(M("Construcción", "obra_material_" + p.title, $"Llevar el material del día a la obra «{p.title}»", 1f, true));
            l.Add(M("Construcción", "obra_trabajo_" + p.title, $"Trabajar la obra «{p.title}» (estructura de guadua, bahareque y paneles)", 2f, true));
        }
        // MATERIAL: lo que el almacén necesita reponer (todo sale del bambusal, los huertos y el laboratorio).
        MaterialStore store = MaterialStore.Instance;
        if (store != null)
            foreach (string need in store.Needs(open))
                switch (need)
                {
                    case "guadua": l.Add(M("Huerto", "mat_guadua", "Cortar guadua madura en el bambusal (se cura y trata con bórax)", 2f, true)); break;
                    case "bahareque": l.Add(M("Construcción", "mat_bahareque", "Preparar barro con paja para el bahareque", 1f, true)); break;
                    case "micelio": l.Add(M("Carne Cultivada", "mat_micelio", "Sembrar paneles de micelio (paja y cañamiza en moldes)", 1f, true)); break;
                    case "cuerda": l.Add(M("Estudio Textil", "mat_cuerda", "Trenzar cuerda de fibra de cáñamo", 1f, true)); break;
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
