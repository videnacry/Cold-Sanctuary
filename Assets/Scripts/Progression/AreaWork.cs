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
    public const string Hospital = "Hospital";   // la antigua Enfermería (docs/hospital.md)

    static WorkRoster.Mission M(string area, string id, string title, float h, bool mandatory = false) => new WorkRoster.Mission(area, id, title, h, mandatory);
    static WorkRoster.Mission Also(WorkRoster.Mission m, params string[] areas) { m.also = areas; return m; }   // la tarea hace ir también a esas áreas
    static WorkRoster.Mission G(string area, string id, string title, string group) => new WorkRoster.Mission(area, id, title, 1f, true) { group = group };   // misión de grupo (a la vez)
    static WorkRoster.Mission Lead(WorkRoster.Mission m) { m.lead = true; return m; }   // dirige: no a un recién llegado (WorkRoster.IsNewcomer)

    /// <summary>Misiones fijas de cada área (sin las de obras, que dependen de qué obras haya abiertas).</summary>
    public static readonly WorkRoster.Mission[] Catalog =
    {
        Lead(M(Hospital, "enf_iniciacion", "Iniciación de recién llegados y turistas (análisis, vacuna, primera virtualización)", 1f, true)),
        M(Hospital, "enf_visita", "Pasar visita y curas en las salas de camas", 2f, true),
        Also(M(Hospital, "enf_animales", "Análisis y vacunas de animales", 1f), "Cría"),
        M(Hospital, "enf_inventario", "Limpieza, esterilización e inventario del hospital", 1f),
        M(Hospital, "hos_urgencias", "Guardia de urgencias (triaje y reanimación)", 2f, true),
        Lead(M(Hospital, "hos_quirofano", "Quirófano: cirugía programada (cirujano + anestesista)", 2f)),
        M(Hospital, "hos_uci", "Cuidados intensivos (UCI)", 2f),
        M(Hospital, "hos_veterinaria", "Hospital veterinario (planta alta): consulta y hospitalización de animales", 2f),
        M("Cocina", "coc_servicio", "Servicio del restaurante (cocinar la carta)", 2f, true),
        M("Cocina", "coc_limpieza", "Limpieza e inventario de la cocina", 1f, true),
        M("Cocina", "coc_obrador", "Elaboraciones del obrador (pan, quesos, fermentos)", 2f),
        M("Cocina", "coc_sala", "Atender a los turistas en sala", 1f),
        M("Huerto", "hue_riego", "Riego y deshierbe de los huertos", 2f, true),
        Also(M("Huerto", "hue_cosecha", "Cosecha para la cocina", 1f), "Cocina"),
        M("Huerto", "hue_visitantes", "Alimentar a los visitantes del huerto", 1f),
        Lead(G("Sala de Yoga", "yog_guia", "Clase de yoga: guiar desde la plataforma central (objetivo 1)", "clase_yoga")),
        Lead(G("Sala de Yoga", "yog_demo", "Clase de yoga: demostrar entre los alumnos (objetivo 2)", "clase_yoga")),
        G("Sala de Yoga", "yog_corrige_1", "Clase de yoga: corregir posturas a los alumnos", "clase_yoga"),
        G("Sala de Yoga", "yog_corrige_2", "Clase de yoga: corregir posturas a los alumnos", "clase_yoga"),
        M("Cocina", "coc_atender", "Atender a los comensales en la barra (servir los pedidos)", 2f, true),
        M("Sala de Yoga", "yog_sala", "Preparar y limpiar la sala de yoga", 1f),
        M("Mecánica", "mec_vehiculos", "Mantenimiento de vehículos", 2f),
        new WorkRoster.Mission("Mecánica", "mec_maquinas", "Reparar maquinaria de las áreas", 1f) { roams = true },
        M("Estudio Textil", "tex_tejer", "Tejer y teñir", 2f),
        M("Estudio Textil", "tex_coser", "Coser y reparar ropa", 1f),
        M("Carne Cultivada", "lab_biorreactor", "Supervisar los biorreactores (glucosa, pH, oxígeno)", 1f, true),
        M("Carne Cultivada", "lab_iniciar", "Iniciar un lote: vial del banco, medio y siembra en el biorreactor", 1f, true),
        Also(M("Carne Cultivada", "lab_cosechar", "Cosechar el lote y dar forma a la carne (cocina y comedero)", 1f, true), "Cocina", "Cría"),
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
        Also(M("Planta Atómica", "pla_isotopos", "Irradiar molibdeno para el generador de Tc-99m del hospital", 1f), Hospital),
        M("Planta Atómica", "pla_ronda", "Ronda de la planta: resinas, bomba, torre y protección radiológica", 1f),
        Also(M("Planta Atómica", "pla_ciclotron", "Producir F-18 en el ciclotrón y sintetizar FDG para el PET del hospital", 1f), Hospital),
        M("Planta Atómica", "fis_experimentos", "Experimentos de física: doble rendija, Michelson, Millikan y el péndulo de Foucault", 1f),
        M("Química", "qui_sintesis", "Síntesis en la campana: pesar, reaccionar, evaporar y analizar el producto", 1f),
        M("Química", "qui_residuos", "Recoger y separar los residuos químicos (ácidos, bases e inflamables)", 0.5f),
        M("Biología", "bio_cultivo", "Cultivo celular: cabina, incubador, microscopio y biorreactor piloto (medios para la carne cultivada)", 1f),
        M("Biología", "bio_pcr", "Del ADN a la secuencia: extracción, pre-PCR, termociclador y secuenciador", 1f),
        M("Central Nuclear", "nuc_turno", "Turno de la central: acceso, sala de control, conversión de potencia y calefacción", 1f),
        M(Guard, "gua_carnivoros", "Patrulla y raciones a los carnívoros", 2f, true),
    };

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
