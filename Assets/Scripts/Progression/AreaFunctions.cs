using System.Collections.Generic;

/// <summary>
/// REGISTRO DE FUNCIONES DE ÁREA (docs/microcosmos-civilizaciones.md §2; Berón 2026-11-01: «¿tenemos una lista de funciones
/// por área? organiza el código para tener las listas siendo que seguiremos agregando más funciones ya sea por requerimiento
/// del santuario o por requerimiento del microcosmos»). Tres piezas:
/// 1. <see cref="AreaFunction"/>: una cosa que se hace (cocinar la carta, pasar visita, guiar la clase de yoga, el muñeco de
///    madera…), definida UNA vez: horas, si es obligatoria, de grupo, si dirige; su CLASE (el gesto y el verbo), los PUESTOS
///    que necesita (claves genéricas: fogón, cama, esterilla…), los papeles, la CAPACIDAD DE ERA que exige
///    (<see cref="Eras.Capability"/>) y su EFECTO en el santuario al terminarla.
/// 2. PLANTILLAS: un área o un edificio = un conjunto ordenado de funciones. Las del SANTUARIO (<see cref="Sanctuary"/>) dan,
///    en el mismo orden de siempre, el catálogo de la jornada (<see cref="AreaWork.Catalog"/> sale de aquí, sin cambio de
///    conducta: lo vigila <c>AreaFunctionRegistryTest</c>). Las de EDIFICIO (<see cref="Buildings"/>) son las que usan los
///    microcosmos de cada era (la cocina de Soyer, el cha chaan teng, el dojo de Yip Man), reutilizando las del santuario.
/// 3. SITIOS (<c>AreaFunctionSite</c>, <c>AreaFunctionStation</c>): en una escena, el edificio dice qué plantilla es y cada
///    mueble qué puesto cubre; <see cref="ForSite"/> da lo que se puede hacer allí en el año de la escena.
/// Añadir una función = una línea Def(...) (y su id en la plantilla del edificio que la use).
/// </summary>
public static class AreaFunctions
{
    public enum Kind { Cook, Serve, Clean, Dine, Heal, Teach, Practice, Meditate, Tend, Build, Make, Operate, Guard, Study, Care, Trade, Worship, Rest }

    /// <summary>Verbo del rótulo de cada clase («Soyer · Cocinando»).</summary>
    public static string Verb(Kind k)
    {
        switch (k)
        {
            case Kind.Cook: return "Cocinando"; case Kind.Serve: return "Sirviendo"; case Kind.Clean: return "Limpiando";
            case Kind.Dine: return "Comiendo"; case Kind.Heal: return "Curando"; case Kind.Teach: return "Enseñando";
            case Kind.Practice: return "Practicando"; case Kind.Meditate: return "Meditando"; case Kind.Tend: return "Cultivando";
            case Kind.Build: return "Construyendo"; case Kind.Make: return "Trabajando en el taller"; case Kind.Operate: return "Operando";
            case Kind.Guard: return "Vigilando"; case Kind.Study: return "Estudiando"; case Kind.Care: return "Cuidando";
            case Kind.Trade: return "Vendiendo"; case Kind.Worship: return "Rezando"; default: return "Descansando";
        }
    }

    public sealed class AreaFunction
    {
        public string id, area, title, group;
        public float hours = 1f;
        public bool mandatory, lead, roams;
        public string[] also = new string[0];
        public Kind kind = Kind.Care;
        /// <summary>Los PUESTOS que hacen falta (claves genéricas: «fogon», «cama», «esterilla», «tatami»…).</summary>
        public string[] stations = new string[0];
        /// <summary>Quién la hace (cocinero, médico, maestro, alumno…): para repartir la jornada del microcosmos.</summary>
        public string[] roles = new string[0];
        /// <summary>La capacidad de era que exige (sin fuego no se cocina…); null = cualquier época.</summary>
        public Eras.Capability? needs;
        /// <summary>Lo que pasa en el santuario al terminarla (quién la hizo). null = solo se registra.</summary>
        public System.Action<string> effect;
        /// <summary>¿Está en el catálogo diario del santuario?</summary>
        public bool sanctuary;

        public WorkRoster.Mission ToMission() => new WorkRoster.Mission(area, id, title, hours, mandatory) { group = group, also = also, roams = roams, lead = lead };
        public bool AllowedIn(int year) => !needs.HasValue || Eras.Allows(needs.Value, year);
        public override string ToString() => $"{area}: {title}";
    }

    static readonly Dictionary<string, AreaFunction> _byId = new Dictionary<string, AreaFunction>();
    static readonly List<AreaFunction> _sanctuaryOrder = new List<AreaFunction>();
    static readonly List<(string prefix, System.Action<string, string> run)> _prefixEffects = new List<(string, System.Action<string, string>)>();

    /// <summary>Todas las funciones registradas.</summary>
    public static IEnumerable<AreaFunction> All => _byId.Values;
    public static AreaFunction Get(string id) => id != null && _byId.TryGetValue(id, out AreaFunction f) ? f : null;

    // ── Definición ───────────────────────────────────────────────────────────────────────────────────────────────
    static AreaFunction Def(string area, string id, string title, float h, bool mandatory, Kind kind, params string[] stations)
    {
        var f = new AreaFunction { area = area, id = id, title = title, hours = h, mandatory = mandatory, kind = kind, stations = stations };
        _byId[id] = f;
        return f;
    }
    // La función forma parte del catálogo diario del santuario (en este orden).
    static AreaFunction S(AreaFunction f) { f.sanctuary = true; _sanctuaryOrder.Add(f); return f; }
    static AreaFunction Lead(AreaFunction f) { f.lead = true; return f; }
    static AreaFunction Also(AreaFunction f, params string[] areas) { f.also = areas; return f; }
    static AreaFunction Group(AreaFunction f, string g) { f.group = g; return f; }
    static AreaFunction Roles(AreaFunction f, params string[] r) { f.roles = r; return f; }
    static AreaFunction Needs(AreaFunction f, Eras.Capability c) { f.needs = c; return f; }
    static AreaFunction Effect(AreaFunction f, System.Action<string> e) { f.effect = e; return f; }

    const string H = AreaWork.Hospital, Guard = AreaWork.Guard;

    static AreaFunctions()
    {
        // ══ EL SANTUARIO: el catálogo diario de la jornada (mismo orden, títulos y horas que el AreaWork.Catalog de antes) ══
        Roles(S(Lead(Def(H, "enf_iniciacion", "Iniciación de recién llegados y turistas (análisis, vacuna, primera virtualización)", 1f, true, Kind.Heal, "camilla"))), "medico");
        Roles(S(Def(H, "enf_visita", "Pasar visita y curas en las salas de camas", 2f, true, Kind.Heal, "cama")), "medico", "enfermera");
        S(Also(Def(H, "enf_animales", "Análisis y vacunas de animales", 1f, false, Kind.Heal, "camilla"), "Cría"));
        // (2026-11-03) El inventario incluye el BANCO DE SANGRE: retirar lo caducado, reponer el submarino-ambulancia y llamar a un donante O− si falta.
        Roles(S(Effect(Def(H, "enf_inventario", "Limpieza, esterilización e inventario del hospital", 1f, false, Kind.Clean, "armario"), who => BloodBank.Instance?.DailyRound(who))), "enfermera");
        Roles(S(Def(H, "hos_urgencias", "Guardia de urgencias (triaje y reanimación)", 2f, true, Kind.Heal, "cama")), "medico", "enfermera");
        Roles(S(Lead(Def(H, "hos_quirofano", "Quirófano: cirugía programada (cirujano + anestesista)", 2f, false, Kind.Heal, "mesa_operaciones"))), "medico");
        S(Def(H, "hos_uci", "Cuidados intensivos (UCI)", 2f, false, Kind.Heal, "cama"));
        S(Def(H, "hos_veterinaria", "Hospital veterinario (planta alta): consulta y hospitalización de animales", 2f, false, Kind.Heal, "camilla"));
        Roles(Needs(S(Def("Cocina", "coc_servicio", "Servicio del restaurante (cocinar la carta)", 2f, true, Kind.Cook, "fogon")), Eras.Capability.Fire), "cocinero");
        Roles(S(Def("Cocina", "coc_limpieza", "Limpieza e inventario de la cocina", 1f, true, Kind.Clean, "fogon")), "cocinero", "camarero");
        Roles(Needs(S(Def("Cocina", "coc_obrador", "Elaboraciones del obrador (pan, quesos, fermentos)", 2f, false, Kind.Cook, "horno")), Eras.Capability.Agriculture), "cocinero");
        Roles(S(Def("Cocina", "coc_sala", "Atender a los turistas en sala", 1f, false, Kind.Serve, "mesa")), "camarero");
        Roles(S(Def("Huerto", "hue_riego", "Riego y deshierbe de los huertos", 2f, true, Kind.Tend, "bancal")), "hortelano");
        Roles(S(Also(Def("Huerto", "hue_cosecha", "Cosecha para la cocina", 1f, false, Kind.Tend, "bancal"), "Cocina")), "hortelano");
        S(Def("Huerto", "hue_visitantes", "Alimentar a los visitantes del huerto", 1f, false, Kind.Serve, "bancal"));
        Roles(S(Lead(Group(Def("Sala de Yoga", "yog_guia", "Clase de Hatha: guiar desde la tarima (objetivo 1)", 1f, true, Kind.Teach, "esterilla"), "clase_yoga"))), "maestro");
        Roles(S(Lead(Group(Def("Sala de Yoga", "yog_demo", "Clase de Hatha: demostrar entre los alumnos (objetivo 2)", 1f, true, Kind.Teach, "esterilla"), "clase_yoga"))), "maestro");
        Roles(S(Group(Def("Sala de Yoga", "yog_corrige_1", "Clase de Hatha: corregir posturas a los alumnos", 1f, true, Kind.Teach, "esterilla"), "clase_yoga")), "maestro");
        Roles(S(Group(Def("Sala de Yoga", "yog_corrige_2", "Clase de Hatha: corregir posturas a los alumnos", 1f, true, Kind.Teach, "esterilla"), "clase_yoga")), "maestro");
        Roles(S(Def("Cocina", "coc_atender", "Atender a los comensales en la barra (servir los pedidos)", 2f, true, Kind.Serve, "barra")), "camarero");
        S(Def("Sala de Yoga", "yog_sala", "Preparar y limpiar las salas de los pisos de yoga (Hatha y Ashtanga; el kund de las tradicionales)", 1f, false, Kind.Clean, "esterilla"));
        Roles(Needs(S(Def("Mecánica", "mec_vehiculos", "Mantenimiento de vehículos", 2f, false, Kind.Make, "banco_taller")), Eras.Capability.Iron), "mecanico");
        { var f = Roles(Needs(S(Def("Mecánica", "mec_maquinas", "Reparar maquinaria de las áreas", 1f, false, Kind.Make, "banco_taller")), Eras.Capability.Iron), "mecanico"); f.roams = true; }
        Roles(S(Def("Estudio Textil", "tex_tejer", "Tejer y teñir", 2f, false, Kind.Make, "telar")), "tejedor");
        Roles(S(Def("Estudio Textil", "tex_coser", "Coser y reparar ropa", 1f, false, Kind.Make, "telar")), "tejedor");
        S(Def("Carne Cultivada", "lab_biorreactor", "Supervisar los biorreactores (glucosa, pH, oxígeno)", 1f, true, Kind.Operate, "biorreactor"));
        S(Effect(Def("Carne Cultivada", "lab_iniciar", "Iniciar un lote: vial del banco, medio y siembra en el biorreactor", 1f, true, Kind.Operate, "biorreactor"), who => Lab()?.OrderStart(who)));
        S(Effect(Also(Def("Carne Cultivada", "lab_cosechar", "Cosechar el lote y dar forma a la carne (cocina y comedero)", 1f, true, Kind.Operate, "biorreactor"), "Cocina", "Cría"), who => Lab()?.OrderHarvest(who)));
        S(Def("Carne Cultivada", "lab_limpieza", "Limpieza y esterilización del laboratorio", 1f, false, Kind.Clean, "biorreactor"));
        Roles(S(Def("Cocina", "coc_comedor", "Limpieza del comedor y de los aseos", 1f, true, Kind.Clean, "mesa")), "camarero");
        S(Def("Sala de Yoga", "yog_aseos", "Limpieza de los aseos de los pisos de yoga", 0.5f, false, Kind.Clean));
        S(Def("Estudio Textil", "tex_limpieza", "Limpieza e inventario del estudio textil", 0.5f, false, Kind.Clean, "telar"));
        S(Def("Mecánica", "mec_limpieza", "Limpieza e inventario del taller", 0.5f, false, Kind.Clean, "banco_taller"));
        S(Def("Construcción", "con_almacen", "Inventario del almacén de material y herramientas", 0.5f, false, Kind.Build));
        S(Def("Cría", "cri_tomas", "Tomas y pesaje de las crías (anotar en la pizarra)", 1f, true, Kind.Care));
        S(Def("Cría", "cri_recintos", "Limpiar los recintos y cambiar la paja", 1f, true, Kind.Clean));
        S(Def("Cría", "cri_vinculo", "Cuidar una cría hasta ganar su vínculo (leer, calmar, alimentar, asear)", 2f, false, Kind.Care));
        S(Def("Gimnasio", "gym_limpieza", "Limpieza del gimnasio", 0.5f, false, Kind.Clean));
        S(Def("Gimnasio", "gym_mantener", "Mantenimiento del gimnasio", 1f, false, Kind.Make));
        Needs(S(Def("Planta Atómica", "pla_operar", "Operar el reactor: arranque, subir a potencia y parada (consola)", 1f, false, Kind.Operate, "consola")), Eras.Capability.Nuclear);
        Needs(S(Also(Def("Planta Atómica", "pla_isotopos", "Irradiar molibdeno para el generador de Tc-99m del hospital", 1f, false, Kind.Operate, "consola"), H)), Eras.Capability.Nuclear);
        Needs(S(Def("Planta Atómica", "pla_ronda", "Ronda de la planta: resinas, bomba, torre y protección radiológica", 1f, false, Kind.Guard, "consola")), Eras.Capability.Nuclear);
        Needs(S(Also(Def("Planta Atómica", "pla_ciclotron", "Producir F-18 en el ciclotrón y sintetizar FDG para el PET del hospital", 1f, false, Kind.Operate, "consola"), H)), Eras.Capability.Nuclear);
        S(Def("Planta Atómica", "fis_experimentos", "Experimentos de física: doble rendija, Michelson, Millikan y el péndulo de Foucault", 1f, false, Kind.Study, "mesa_laboratorio"));
        S(Def("Química", "qui_sintesis", "Síntesis en la campana: pesar, reaccionar, evaporar y analizar el producto", 1f, false, Kind.Study, "mesa_laboratorio"));
        S(Def("Química", "qui_residuos", "Recoger y separar los residuos químicos (ácidos, bases e inflamables)", 0.5f, false, Kind.Clean, "mesa_laboratorio"));
        S(Def("Biología", "bio_cultivo", "Cultivo celular: cabina, incubador, microscopio y biorreactor piloto (medios para la carne cultivada)", 1f, false, Kind.Study, "mesa_laboratorio"));
        S(Def("Biología", "bio_pcr", "Del ADN a la secuencia: extracción, pre-PCR, termociclador y secuenciador", 1f, false, Kind.Study, "mesa_laboratorio"));
        Needs(S(Def("Central Nuclear", "nuc_turno", "Turno de la central: acceso, sala de control, conversión de potencia y calefacción", 1f, false, Kind.Operate, "consola")), Eras.Capability.Nuclear);
        Roles(S(Def(Guard, "gua_carnivoros", "Patrulla y raciones a los carnívoros", 2f, true, Kind.Guard)), "guardia");
        // (2026-11-04) LOS PISOS NUEVOS (docs/piso-informatica.md, docs/piso-aula.md): el clúster de IA de Informática y el Aula.
        Roles(S(Effect(Def("Informática", "ifo_sysadmin", "Administrar el clúster de IA: colas, copias, parches y alertas (puesto de monitorización)", 1f, false, Kind.Operate, "consola"), who => AiCluster.Instance?.Sysadmin(who))), "informatico");
        Roles(S(Effect(Def("Informática", "ifo_refrigeracion", "Mantener la refrigeración líquida: filtros, fugas, bombas e intercambiadores", 1f, false, Kind.Make, "banco_taller"), who => AiCluster.Instance?.CoolingService(who))), "informatico");
        Roles(S(Effect(Def("Informática", "ifo_energia", "Revisar la acometida, el SAI y las baterías del clúster", 0.5f, false, Kind.Operate, "consola"), who => AiCluster.Instance?.PowerCheck(who))), "informatico");
        Roles(S(Def("Aula", "aul_clase", "Dar o recibir una clase en el aula (un tema de las áreas del santuario)", 1f, false, Kind.Teach, "pizarra")), "maestro", "alumno");
        S(Def("Aula", "aul_estudio", "Estudiar en la sala de estudio (biblioteca)", 1f, false, Kind.Study, "mesa"));
        // (2026-11-04) LAS PROFESIONES NUEVAS (Berón; docs/personajes-mesocosmos.md §Profesiones): asuntos legales y lenguas (Karen, en
        // una mesa de la sala de estudio del Aula), la contabilidad (Alicita, desde el restaurante) y la dirección de las obras (Alicita).
        S(Def("Aula", "aul_legal", "Asuntos legales: contratos, permisos, normas y acuerdos del santuario (en una mesa de la sala de estudio)", 1f, false, Kind.Study, "mesa"));
        Roles(S(Def("Aula", "aul_lenguas", "Lenguas: traducir documentos y dar clase de idiomas", 1f, false, Kind.Teach, "pizarra")), "maestro", "alumno");
        S(Def("Cocina", "coc_cuentas", "Las cuentas del santuario: ingresos del restaurante, compras y presupuesto", 0.5f, false, Kind.Trade, "mesa"));
        S(Def("Construcción", "con_proyectos", "Dirigir los proyectos de obra: plan, plazos, material y equipos", 1f, false, Kind.Build));
        // (2026-11-04, más profesiones) La COMUNICACIÓN (Andrea: los avisos y la radio, en el comedor), las LETRAS (Irosene: poesía,
        // narración y lecturas en el Aula) y la CURANDERA (Maite: plantas medicinales en la farmacia del hospital).
        S(Def("Cocina", "coc_avisos", "Comunicación: preparar y dar los avisos del santuario y el programa de radio en el comedor", 0.5f, false, Kind.Trade, "mesa"));
        Roles(S(Def("Aula", "aul_letras", "Letras: clase de poesía y narración, y lecturas en la sala de estudio", 1f, false, Kind.Teach, "pizarra")), "maestro", "alumno");
        Roles(S(Def("Hospital", "hos_plantas", "Curandera: remedios de plantas medicinales (infusiones, cataplasmas y ungüentos) para los pacientes", 1f, false, Kind.Heal, "armario")), "enfermera");

        // ══ FUNCIONES DEL DÍA (no son del catálogo fijo: las añade AreaWork.Today), con su efecto en el santuario ══
        Get("coc_servicio").effect = who => KitchenLarder.Instance?.ServeService(12, who);
        Effect(Also(Def("Huerto", "mat_guadua", "Cortar guadua madura en el bambusal (se cura y trata con bórax)", 2f, true, Kind.Tend), "Construcción"), who => MaterialStore.Instance?.HarvestGuadua(who));
        Effect(Def("Construcción", "mat_bahareque", "Preparar barro con paja para el bahareque", 1f, true, Kind.Build), who => MaterialStore.Instance?.MakeBahareque(who));
        Effect(Also(Def("Carne Cultivada", "mat_micelio", "Sembrar paneles de micelio (paja y cañamiza en moldes)", 1f, true, Kind.Tend), "Construcción"), who => MaterialStore.Instance?.GrowMycelium(who));
        Effect(Also(Def("Estudio Textil", "mat_cuerda", "Trenzar cuerda de fibra de cáñamo", 1f, true, Kind.Make), "Construcción"), who => MaterialStore.Instance?.TwistRope(who));
        // Las de prefijo (una por obra o por elaboración).
        _prefixEffects.Add(("obra_material_", (id, who) => Obra(id.Substring(14), who, material: true)));
        _prefixEffects.Add(("obra_trabajo_", (id, who) => Obra(id.Substring(13), who, material: false)));
        _prefixEffects.Add(("prep_", (id, who) => KitchenLarder.Instance?.StartPrep(id.Substring(5), who)));

        // ══ FUNCIONES DE LOS MICROCOSMOS (eras civilizadas): las que el santuario no tiene ══
        Roles(Needs(Def("Cocina", "coc_fogon", "Cocinar en el fogón (la comida del día)", 1f, false, Kind.Cook, "fogon"), Eras.Capability.Fire), "cocinero");
        Roles(Def("Restaurante", "res_comer", "Comer en el comedor", 1f, false, Kind.Dine, "mesa"), "todos");
        Roles(Def("Restaurante", "res_servir", "Servir las mesas", 1f, false, Kind.Serve, "mesa"), "camarero");
        Roles(Def("Hospital", "hos_lavanderia", "Lavar y hervir la ropa de cama", 1f, false, Kind.Clean, "lavadero"), "enfermera");
        Roles(Needs(Def("Hospital", "hos_vacunar", "Vacunar", 0.5f, false, Kind.Heal, "camilla"), Eras.Capability.Vaccination), "medico");
        Roles(Def("Hospital", "hos_registro", "Anotar los datos de los enfermos", 1f, false, Kind.Study, "mesa"), "medico");
        Roles(Def("Sala de Yoga", "yog_practica", "Practicar las posturas", 1f, false, Kind.Practice, "esterilla"), "alumno");
        Roles(Def("Sala de Yoga", "yog_meditar", "Meditar", 1f, false, Kind.Meditate, "esterilla"), "todos");
        Roles(Def("Dojo", "dojo_guardia", "Guardia y juego de pies", 1f, false, Kind.Practice, "tatami"), "alumno");
        Roles(Def("Dojo", "dojo_muneco", "Trabajar el muñeco de madera", 1f, false, Kind.Practice, "muneco"), "alumno");
        Roles(Def("Dojo", "dojo_combate", "Combate libre y manos pegajosas (chi sao)", 1f, false, Kind.Practice, "tatami"), "alumno");
        Roles(Lead(Def("Dojo", "dojo_ensenar", "Dar la clase", 1f, false, Kind.Teach, "tatami")), "maestro");
        Roles(Needs(Def("Huerto", "hue_sembrar", "Sembrar", 1f, false, Kind.Tend, "bancal"), Eras.Capability.Agriculture), "hortelano");
        Roles(Def("Taller", "tal_reparar", "Reparar (bicicletas, herramientas)", 2f, false, Kind.Make, "banco_taller"), "mecanico");
        Roles(Needs(Def("Taller", "tal_fabricar", "Fabricar piezas en el torno", 2f, false, Kind.Make, "torno"), Eras.Capability.Iron), "mecanico");
        Roles(Def("Taller", "tal_ensayo", "Ensayar en el túnel de viento", 1f, false, Kind.Study, "tunel"), "inventor");
        Roles(Def("Escuela", "esc_estudiar", "Estudiar", 2f, false, Kind.Study, "estante"), "alumno", "sabio");
        Roles(Lead(Def("Escuela", "esc_ensenar", "Enseñar", 1f, false, Kind.Teach, "pizarra")), "sabio", "maestro");
        Roles(Needs(Def("Escuela", "esc_escribir", "Escribir y copiar", 1f, false, Kind.Study, "mesa"), Eras.Capability.Writing), "sabio");
        Roles(Def("Mercado", "mer_vender", "Vender en el puesto", 2f, false, Kind.Trade, "puesto"), "comerciante");
        Roles(Def("Mercado", "mer_comprar", "Hacer la compra", 0.5f, false, Kind.Trade, "puesto"), "todos");
        Roles(Def("Templo", "tem_rezar", "Rezar / ofrendar", 0.5f, false, Kind.Worship, "altar"), "todos");
        Roles(Def("Casa", "cas_descansar", "Descansar en casa", 1f, false, Kind.Rest, "cama"), "todos");
        Roles(Def("Casa", "cas_cuidar", "Cuidar de los hijos", 1f, false, Kind.Care, "cama"), "todos");
        Roles(Def("Lavandería", "lav_lavar", "Lavar la ropa", 1f, false, Kind.Clean, "lavadero"), "todos");
        Roles(Def("Trinchera", "tri_vigilar", "Guardia en el parapeto", 2f, false, Kind.Guard, "parapeto"), "soldado");
        Roles(Def("Laboratorio", "lab_medir", "Medir y anotar los resultados del experimento", 1f, false, Kind.Study, "mesa_laboratorio"), "sabio");
        Roles(Def("Oficina de patentes", "ofi_patentes", "Examinar solicitudes de patente", 2f, false, Kind.Study, "mesa"), "sabio");
        Roles(Def("Taller de papeles", "ofi_documentos", "Hacer documentos falsos que salvan vidas (cédulas, cartillas de racionamiento)", 2f, false, Kind.Make, "mesa"), "sabio");
        Roles(Needs(Def("Alfarería", "alf_cuencos", "Moldear cuencos de borde biselado (las raciones)", 2f, false, Kind.Make, "torno_alfarero"), Eras.Capability.Pottery), "alfarero");
        Roles(Needs(Def("Granero", "gra_raciones", "Repartir las raciones de cebada", 1f, false, Kind.Serve, "granero"), Eras.Capability.Agriculture), "comerciante", "todos");
        Roles(Needs(Def("Granero", "gra_moler", "Moler el grano en el molino de mano", 1f, false, Kind.Cook, "granero"), Eras.Capability.Agriculture), "todos");
        Roles(Def("Orfanato", "orf_cuidar", "Cuidar a los niños escondidos", 2f, false, Kind.Care, "cama"), "todos");
        Roles(Def("Trinchera", "tri_refugio", "Descansar en el refugio", 1f, false, Kind.Rest, "cama"), "soldado");

        // ══ LOS EDIFICIOS DEL PUEBLO (2026-11-03, docs/microcosmos-reglas-edificacion.md §8; CivicBuilder.Civic): cada uno con su
        //    interacción para humanos e insectos-humanos y su categoría de actividad (trabajo, descanso, recuperación u ocio) ══
        Roles(Def("Comisaría", "pol_atender", "Atender denuncias y avisos en el mostrador", 2f, false, Kind.Serve, "mesa"), "guardia", "todos");
        Roles(Def("Comisaría", "pol_ronda", "Salir de ronda por las calles", 2f, false, Kind.Guard, "armero", "armario"), "guardia");
        Roles(Def("Bomberos", "bom_guardia", "Guardia junto a la bomba (listos para salir)", 2f, false, Kind.Guard, "carro"), "bombero", "todos");
        Roles(Def("Bomberos", "bom_ejercicio", "Ejercicio: tender la manguera y llenar la bomba", 1f, false, Kind.Practice, "carro", "fuente", "pozo"), "bombero");
        Roles(Def("Museo", "mus_visitar", "Visitar la colección", 1f, false, Kind.Study, "vitrina", "estatua"), "todos");
        Roles(Def("Museo", "mus_conservar", "Limpiar y catalogar las piezas", 1f, false, Kind.Clean, "vitrina"), "conservador", "sabio");
        Roles(Def("Guardería", "gua_cuidar", "Cuidar a los pequeños (siesta y comida)", 2f, false, Kind.Care, "cuna"), "cuidador", "todos");
        Roles(Def("Guardería", "gua_jugar", "Jugar con los niños", 1f, false, Kind.Care, "esterilla", "cuna"), "todos");
        Roles(Def("Sala de baile", "bai_bailar", "Bailar", 1f, false, Kind.Practice, "pista"), "todos");
        Roles(Lead(Def("Sala de baile", "bai_ensenar", "Enseñar los pasos del baile", 1f, false, Kind.Teach, "pista")), "maestro");
        Roles(Def("Estación", "est_viajar", "Esperar y subir al transporte", 0.5f, false, Kind.Rest, "anden"), "todos");
        Roles(Def("Estación", "est_taquilla", "Vender billetes y despachar la carga", 2f, false, Kind.Trade, "puesto"), "comerciante");
        Roles(Def("Hotel", "hot_recepcion", "Recibir a los viajeros", 2f, false, Kind.Serve, "barra"), "camarero");
        Roles(Def("Hotel", "hot_dormir", "Dormir en una habitación", 1f, false, Kind.Rest, "cama"), "todos");
        Roles(Def("Carpintería", "car_madera", "Serrar, cepillar y ensamblar madera", 2f, false, Kind.Make, "caballete", "banco_taller"), "carpintero", "mecanico");
        Roles(Def("Obra", "obr_levantar", "Levantar la obra (piedra, ladrillo o bloque)", 2f, false, Kind.Build, "material"), "albanil", "todos");
        Roles(Def("Obra", "obr_preparar", "Preparar la madera de la obra", 1f, false, Kind.Build, "caballete"), "carpintero", "albanil");
        Roles(Def("Centro de tecnología", "tec_operar", "Operar los aparatos (centralita, calculadora o instrumentos)", 2f, false, Kind.Operate, "consola"), "tecnico", "sabio");
        Roles(Def("Cuartel", "mil_guardia", "Guardia en el cuartel", 2f, false, Kind.Guard, "armero"), "soldado", "guardia");
        Roles(Def("Cuartel", "mil_instruccion", "Instrucción en el patio", 1f, false, Kind.Practice, "tatami"), "soldado");
        Roles(Def("Baños", "ban_banarse", "Bañarse y nadar (o sudar en el baño de vapor)", 1f, false, Kind.Rest, "piscina", "temazcal"), "todos");
        Roles(Def("Baños", "ban_vigilar", "Vigilar y limpiar los baños", 1f, false, Kind.Clean, "piscina", "temazcal"), "todos");
        Roles(Def("Parque", "par_pasear", "Pasear y descansar a la sombra", 1f, false, Kind.Rest, "fuente", "frutal"), "todos");
        Roles(Def("Parque", "par_cosechar", "Coger fruta de los frutales", 0.5f, false, Kind.Tend, "frutal"), "hortelano", "todos");
        Roles(Def("Plaza", "pla_reunirse", "Reunirse en la plaza (charlar, coger agua)", 1f, false, Kind.Rest, "fuente"), "todos");
        Roles(Def("Plaza", "pla_contemplar", "Contemplar el monumento", 0.5f, false, Kind.Meditate, "estatua"), "todos");
        Roles(Def("Centro de entrenamiento", "ent_entrenar", "Entrenar (lucha, gimnasia o juego de pelota)", 1f, false, Kind.Practice, "tatami", "pista"), "alumno", "todos");
        Roles(Lead(Def("Centro de entrenamiento", "ent_ensenar", "Dirigir el entrenamiento", 1f, false, Kind.Teach, "tatami", "pista")), "maestro");
        Roles(Def("Fuente pública", "fue_agua", "Coger agua y lavar", 0.5f, false, Kind.Clean, "fuente", "lavadero"), "todos");
        Roles(Def("Mirador", "mir_mirar", "Asomarse desde la meseta (el santuario gigante alrededor)", 0.5f, false, Kind.Meditate, "estatua", "frutal"), "todos");
    }

    static CulturedMeatLab Lab() => CulturedMeatLab.All.Count > 0 ? CulturedMeatLab.All[0] : null;

    static void Obra(string title, string who, bool material)
    {
        int hash = title.LastIndexOf('#'); if (hash > 0) title = title.Substring(0, hash);   // «…#2»: otra persona del equipo
        foreach (ConstructionProject p in ConstructionProject.All)
            if (p.title == title) { if (material) p.DeliverMaterial(who); else p.WorkDay(who); }
    }

    // ── El SANTUARIO ─────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>El catálogo fijo de la jornada (lo que antes era la lista de AreaWork), en su orden.</summary>
    public static WorkRoster.Mission[] SanctuaryMissions()
    {
        var l = new WorkRoster.Mission[_sanctuaryOrder.Count];
        for (int i = 0; i < l.Length; i++) l[i] = _sanctuaryOrder[i].ToMission();
        return l;
    }

    /// <summary>Plantillas del santuario: cada área (su nombre visible) con sus funciones del catálogo, en orden.</summary>
    public static Dictionary<string, List<string>> Sanctuary
    {
        get
        {
            var d = new Dictionary<string, List<string>>();
            foreach (AreaFunction f in _sanctuaryOrder) { if (!d.TryGetValue(f.area, out var l)) d[f.area] = l = new List<string>(); l.Add(f.id); }
            return d;
        }
    }

    /// <summary>Ejecuta el EFECTO de terminar la función <paramref name="id"/> (true si había uno). Sustituye al switch de
    /// <c>WorkdayDirector.Apply</c>: una función nueva trae su efecto en su Def.</summary>
    public static bool Run(string id, string who)
    {
        if (string.IsNullOrEmpty(id)) return false;
        foreach (var (prefix, run) in _prefixEffects) if (id.StartsWith(prefix)) { run(id, who); return true; }
        AreaFunction f = Get(id);
        if (f == null || f.effect == null) return false;
        f.effect(who);
        return true;
    }

    // ── Los EDIFICIOS de los microcosmos ─────────────────────────────────────────────────────────────────────────

    /// <summary>Plantillas de edificio (cualquier era): qué funciones se hacen en cada tipo. Reutilizan las del santuario
    /// (pasar visita, urgencias, el servicio de la carta, la clase de yoga, el riego…) y añaden las suyas.</summary>
    public static readonly Dictionary<string, string[]> Buildings = new Dictionary<string, string[]>
    {
        ["Cocina"] = new[] { "coc_servicio", "coc_fogon", "coc_obrador", "coc_limpieza" },
        ["Restaurante"] = new[] { "coc_servicio", "coc_atender", "res_servir", "res_comer", "coc_comedor" },
        ["Hospital"] = new[] { "enf_visita", "hos_urgencias", "hos_registro", "hos_lavanderia", "hos_vacunar", "enf_inventario" },
        ["Sala de Yoga"] = new[] { "yog_guia", "yog_practica", "yog_meditar", "yog_sala" },
        ["Dojo"] = new[] { "dojo_ensenar", "dojo_guardia", "dojo_muneco", "dojo_combate" },
        ["Huerto"] = new[] { "hue_riego", "hue_sembrar", "hue_cosecha" },
        ["Taller"] = new[] { "tal_reparar", "tal_fabricar", "tal_ensayo", "mec_limpieza" },
        ["Escuela"] = new[] { "esc_ensenar", "esc_estudiar", "esc_escribir" },
        ["Mercado"] = new[] { "mer_vender", "mer_comprar" },
        ["Templo"] = new[] { "tem_rezar", "yog_meditar" },
        ["Casa"] = new[] { "coc_fogon", "cas_descansar", "cas_cuidar" },
        ["Lavandería"] = new[] { "lav_lavar", "hos_lavanderia" },
        ["Trinchera"] = new[] { "tri_vigilar", "tri_refugio", "coc_fogon" },
        ["Laboratorio"] = new[] { "fis_experimentos", "lab_medir", "esc_estudiar" },
        ["Oficina de patentes"] = new[] { "ofi_patentes", "esc_escribir" },
        ["Taller de papeles"] = new[] { "ofi_documentos" },
        ["Alfarería"] = new[] { "alf_cuencos" },
        ["Granero"] = new[] { "gra_raciones", "gra_moler" },
        ["Orfanato"] = new[] { "orf_cuidar", "coc_fogon", "cas_descansar" },
        // (2026-11-03) Los edificios del pueblo (CivicBuilder.Civic; su nombre de época y lugar, MicroStyles.CivicName).
        ["Comisaría"] = new[] { "pol_atender", "pol_ronda" },
        ["Bomberos"] = new[] { "bom_guardia", "bom_ejercicio" },
        ["Museo"] = new[] { "mus_visitar", "mus_conservar" },
        ["Guardería"] = new[] { "gua_cuidar", "gua_jugar", "coc_fogon" },
        ["Sala de baile"] = new[] { "bai_bailar", "bai_ensenar" },
        ["Estación"] = new[] { "est_viajar", "est_taquilla" },
        ["Hotel"] = new[] { "hot_recepcion", "hot_dormir", "res_comer" },
        ["Carpintería"] = new[] { "car_madera" },
        ["Obra"] = new[] { "obr_levantar", "obr_preparar" },
        ["Centro de tecnología"] = new[] { "tec_operar" },
        ["Cuartel"] = new[] { "mil_guardia", "mil_instruccion" },
        ["Baños"] = new[] { "ban_banarse", "ban_vigilar" },
        ["Parque"] = new[] { "par_pasear", "par_cosechar" },
        ["Plaza"] = new[] { "pla_reunirse", "pla_contemplar" },
        ["Centro de entrenamiento"] = new[] { "ent_entrenar", "ent_ensenar" },
        ["Fuente pública"] = new[] { "fue_agua" },
        ["Mirador"] = new[] { "mir_mirar" },
    };

    /// <summary>Las funciones que se pueden hacer en un edificio <paramref name="building"/> en el año <paramref name="year"/>
    /// (las que exigen algo que aún no existía —vacunas antes de 1796, sembrar antes del Neolítico— no salen).</summary>
    public static List<AreaFunction> ForSite(string building, int year)
    {
        var l = new List<AreaFunction>();
        if (building == null) return l;
        if (Buildings.TryGetValue(building, out string[] ids))
            foreach (string id in ids) { AreaFunction f = Get(id); if (f != null && f.AllowedIn(year)) l.Add(f); }
        else if (Sanctuary.TryGetValue(building, out List<string> sids))
            foreach (string id in sids) { AreaFunction f = Get(id); if (f != null && f.AllowedIn(year)) l.Add(f); }
        return l;
    }
}
