using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TAREAS DEL JUGADOR POR PASOS CON «E» (Berón 2026-11-04: «de momento para todas las tareas e iniciaciones vamos a utilizar el
/// modelo de Ambrosio, de modo que se pueden resolver pulsando "E" en los lugares indicados»; docs/tareas-del-jugador.md).
/// El modelo de Ambrosio (<see cref="AmbrosioStory.Act"/>): hay SITIOS marcados y en cada uno la E hace lo que toca allí
/// («Empuja a Shanidar», «Toma la melaza», «Dásela»). Aquí se generaliza para la jornada de Kushal: cada misión de la jornada
/// (<see cref="AreaFunctions"/>, <see cref="AreaWork.Today"/>) es una SECUENCIA de pasos; cada paso tiene su acción («Riega el
/// bancal»), su PUESTO (la clave genérica del mueble: «bancal», «fogon», «camilla»…), el objeto que se le da y el gesto de la
/// clase de la función. Este fichero es el CATÁLOGO (qué pasos tiene cada misión) y el localizador de SITIOS (dónde está
/// cada puesto en la escena); el que los ejecuta, con el marcador, el aviso «E — acción», el progreso y el cierre, es
/// <see cref="PlayerTaskRunner"/>.
/// Añadir una misión nueva = una línea T(...) aquí (si no, sale con los pasos genéricos de su clase: nunca queda sin pasos).
/// </summary>
public static class PlayerTasks
{
    /// <summary>Un paso: lo que se hace, en qué puesto, el objeto que se lleva y el gesto. <see cref="area"/> = otro área
    /// (llevar la cosecha a la cocina, reparar en el gimnasio); null = la de la misión.</summary>
    public sealed class Step
    {
        public string action, station, item, area;
        public AreaFunctions.Kind gesture = AreaFunctions.Kind.Care;
        /// <summary>Efecto propio del paso (además de dar el objeto); null = ninguno.</summary>
        public System.Action<GameObject> effect;
        public override string ToString() => $"{action} [{station}]{(string.IsNullOrEmpty(item) ? "" : $" → {item}")}";
    }

    /// <summary>Una misión hecha secuencia de pasos.</summary>
    public sealed class Plan
    {
        public string id, title, area, site;
        public readonly List<Step> steps = new List<Step>();
        /// <summary>¿Sale del catálogo escrito (true) o de los pasos genéricos de su clase (false)?</summary>
        public bool authored;
        public override string ToString() => $"{id}: {steps.Count} pasos";
    }

    // ── EL CATÁLOGO: misión → pasos «acción|puesto|objeto» ──────────────────────────────────────────────────────────
    static readonly Dictionary<string, string[]> _table = new Dictionary<string, string[]>();
    static readonly Dictionary<string, string> _stepArea = new Dictionary<string, string>();   // «id#paso» → otra área
    static void T(string id, params string[] steps) { _table[id] = steps; }
    static void Elsewhere(string id, int step, string area) { _stepArea[id + "#" + step] = area; }

    /// <summary>Todas las misiones con pasos escritos (las del catálogo de la jornada y las del día).</summary>
    public static IEnumerable<string> AuthoredIds => _table.Keys;
    public static bool Authored(string id) => id != null && _table.ContainsKey(id);

    static PlayerTasks()
    {
        // ══ HOSPITAL ══
        T("enf_iniciacion", "Prepara el kit de análisis|armario|Tablet", "Recibe al recién llegado en la camilla|camilla|Termometro", "Anota los resultados en su ficha|mesa|Tablet");
        T("enf_visita", "Coge el carro de curas|armario|Gasas", "Pasa visita en las camas|cama|Estetoscopio", "Haz las curas|cama|Gasas", "Anota la visita|mesa|Tablet");
        T("enf_animales", "Prepara las vacunas|armario|Jeringa", "Explora al animal en la camilla|camilla|Estetoscopio", "Vacúnalo y anótalo|camilla|Jeringa");
        T("enf_inventario", "Limpia y esteriliza el material|armario|Cepillo", "Cuenta el inventario|armario|Tablet", "Repón el banco de sangre|armario|Bolsa de sangre");
        T("hos_urgencias", "Haz el triaje|camilla|Pulsioximetro", "Estabiliza al paciente|cama|Manguito", "Registra el caso|mesa|Tablet");
        T("hos_quirofano", "Lávate las manos y ponte la bata|armario|Cepillo", "Prepara la mesa de operaciones|mesa_operaciones|Bandeja de instrumental", "Asiste en la cirugía|mesa_operaciones|Bisturí");
        T("hos_uci", "Revisa los monitores|cama|Tablet", "Cambia la medicación|cama|Jeringa", "Anota la evolución|mesa|Tablet");
        T("hos_veterinaria", "Saca la ficha del animal|mesa|Tablet", "Explóralo en la camilla|camilla|Estetoscopio", "Dale su comida y su medicina|camilla|Cuenco");
        // ══ COCINA ══
        T("coc_servicio", "Coge la comanda|barra|Comanda", "Cocina en el fogón|fogon|Sartén", "Emplata y lleva el plato al pase|mesa|Plato");
        T("coc_limpieza", "Friega los fogones|fogon|Estropajo", "Friega el suelo de la cocina|mesa|Fregona", "Haz el inventario de la despensa|armario|Tablet");
        T("coc_obrador", "Pesa la harina y la leche|mesa|Báscula", "Amasa y da forma|mesa|Masa", "Hornea|horno|Pala de horno");
        T("coc_sala", "Recibe a los turistas|barra|Carta", "Toma nota en la mesa|mesa|Comanda", "Sirve los platos|mesa|Bandeja");
        T("coc_atender", "Atiende en la barra|barra|Comanda", "Sirve el pedido|barra|Bandeja", "Limpia la barra|barra|Bayeta");
        T("coc_comedor", "Recoge las mesas|mesa|Bandeja", "Friega el suelo del comedor|mesa|Fregona", "Limpia los aseos|aseos|Cepillo");
        // ══ HUERTO ══
        T("hue_riego", "Coge la regadera|bancal|Regadera", "Riega el bancal|bancal|Regadera", "Levanta las malas hierbas con su raíz|bancal|Almocafre");
        T("hue_transplante", "Coge las plantas del vivero|bancal|Bandeja de cepellones", "Mira cuáles son nativas|bancal|Tablet", "Llévalas a la tundra y plántalas|bancal|Bandeja de cepellones", "Quema las bolsas de no nativas|incinerador|Bolsa cerrada");
        T("hue_cosecha", "Coge la cesta|bancal|Cesta", "Cosecha lo maduro|bancal|Cesta", "Lleva la cosecha a la cocina|mesa|Cesta"); Elsewhere("hue_cosecha", 2, "Cocina");
        T("hue_visitantes", "Coge el pienso|bancal|Saco de pienso", "Llena los comederos|comedero|Saco de pienso", "Rellena el agua|comedero|Cubo");
        T("mat_guadua", "Coge el machete|almacen|Machete", "Corta las guaduas maduras del bambusal|bancal|Machete", "Deja las cañas a curar con bórax|almacen|Guaduas");
        // ══ SALA DE YOGA ══
        T("yog_guia", "Sube a la tarima|esterilla|", "Guía el Saludo al Sol|esterilla|", "Cierra la clase con la relajación|esterilla|");
        T("yog_demo", "Ponte entre los alumnos|esterilla|", "Demuestra cada postura|esterilla|", "Respira con ellos en la relajación|esterilla|");
        T("yog_corrige_1", "Recorre las esterillas|esterilla|", "Corrige una postura|esterilla|", "Corrige otra postura|esterilla|");
        T("yog_corrige_2", "Recorre las esterillas|esterilla|", "Corrige una postura|esterilla|", "Corrige otra postura|esterilla|");
        T("yog_sala", "Coge la fregona|armario|Fregona", "Limpia y coloca las esterillas|esterilla|Paño", "Prepara el kund y las velas|esterilla|Vela");
        T("yog_aseos", "Coge los productos de limpieza|armario|Cubo", "Limpia los aseos|aseos|Cepillo", "Repón el papel y el jabón|aseos|Jabón");
        // ══ MECÁNICA ══
        T("mec_vehiculos", "Coge la caja de herramientas|banco_taller|Llave inglesa", "Revisa el vehículo|vehiculo|Llave inglesa", "Anota el mantenimiento|banco_taller|Tablet");
        T("mec_maquinas", "Coge la caja de herramientas|banco_taller|Llave inglesa", "Repara la máquina averiada|maquina|Destornillador", "Prueba que funciona|maquina|Tablet");
        T("mec_limpieza", "Barre el taller|banco_taller|Escoba", "Ordena las herramientas|banco_taller|Llave inglesa", "Haz el inventario|armario|Tablet");
        // ══ ESTUDIO TEXTIL ══
        T("tex_tejer", "Prepara la urdimbre|telar|Ovillo", "Teje en el telar|telar|Lanzadera", "Tiñe la tela|tina|Tela");
        T("tex_coser", "Coge la ropa rota|armario|Ropa", "Cose el arreglo|mesa|Aguja", "Dóblala y guárdala|armario|Ropa");
        T("tex_limpieza", "Barre el estudio|telar|Escoba", "Haz el inventario de hilos|armario|Ovillo");
        T("mat_cuerda", "Coge la fibra de cáñamo|armario|Fibra de cáñamo", "Trenza la cuerda|telar|Cuerda", "Enróllala para el almacén|mesa|Cuerda");
        // ══ CARNE CULTIVADA ══
        T("lab_biorreactor", "Mide la glucosa y el pH|biorreactor|Tablet", "Ajusta el oxígeno|biorreactor|Llave", "Anota los valores|mesa|Tablet");
        T("lab_iniciar", "Saca un vial del banco|armario|Vial", "Prepara el medio|mesa|Botella de medio", "Siembra el biorreactor|biorreactor|Vial");
        T("lab_cosechar", "Para el biorreactor|biorreactor|Tablet", "Cosecha el lote|biorreactor|Bandeja", "Da forma a la carne|mesa|Molde");
        T("lab_limpieza", "Vacía y lava el biorreactor|biorreactor|Cepillo", "Esteriliza en el autoclave|armario|Bandeja");
        T("mat_micelio", "Mezcla paja y cañamiza|mesa|Paja", "Llena los moldes|mesa|Molde", "Siembra el micelio|biorreactor|Vial");
        // ══ CONSTRUCCIÓN ══
        T("con_almacen", "Cuenta el material|almacen|Tablet", "Revisa las herramientas|almacen|Llave inglesa", "Anota lo que falta|almacen|Tablet");
        T("mat_bahareque", "Coge tierra y paja|almacen|Paja", "Pisa el barro con la paja|barro|Pala", "Llena los cubos de bahareque|almacen|Cubo de barro");
        // ══ CRÍA ══
        T("cri_tomas", "Prepara los biberones|mesa|Biberón", "Da la toma a las crías|recinto|Biberón", "Pésalas y anótalo en la pizarra|pizarra|Tiza");
        T("cri_recintos", "Coge la horca|almacen|Horca", "Saca la paja sucia|recinto|Horca", "Pon paja limpia|recinto|Paja");
        T("cri_vinculo", "Acércate a una cría con calma|recinto|", "Dale de comer|recinto|Biberón", "Asea a la cría|recinto|Cepillo");
        // ══ GIMNASIO ══
        T("gym_limpieza", "Coge el desinfectante|armario|Pulverizador", "Limpia las máquinas|maquina|Bayeta", "Friega el suelo|maquina|Fregona");
        T("gym_mantener", "Revisa cables y poleas|maquina|Llave inglesa", "Engrasa las guías|maquina|Aceitera", "Ajusta los asientos|maquina|Llave inglesa");
        // ══ PLANTA ATÓMICA, QUÍMICA, BIOLOGÍA, CENTRAL ══
        T("pla_operar", "Comprueba la consola|consola|Tablet", "Arranca y sube a potencia|consola|", "Haz la parada|consola|");
        T("pla_isotopos", "Coloca el blanco de molibdeno|consola|Cápsula", "Irradia el blanco|consola|", "Prepara el generador para el hospital|armario|Contenedor de plomo");
        T("pla_ronda", "Revisa las resinas|consola|Tablet", "Revisa la bomba y la torre|bomba|Linterna", "Mide la radiación|consola|Dosímetro");
        T("pla_ciclotron", "Prepara el ciclotrón|consola|Tablet", "Produce el F-18|consola|", "Sintetiza la FDG|mesa_laboratorio|Vial");
        T("fis_experimentos", "Monta la doble rendija|mesa_laboratorio|Láser", "Mide con el interferómetro de Michelson|mesa_laboratorio|Tablet", "Observa el péndulo de Foucault|pendulo|Tablet");
        T("qui_sintesis", "Pesa los reactivos|mesa_laboratorio|Báscula", "Haz la reacción en la campana|campana|Matraz", "Evapora y analiza el producto|mesa_laboratorio|Vial");
        T("qui_residuos", "Recoge los residuos|mesa_laboratorio|Bidón", "Sepáralos: ácidos, bases e inflamables|armario|Bidón");
        T("bio_cultivo", "Trabaja en la cabina|cabina|Pipeta", "Revisa el incubador|incubador|Placa", "Mira al microscopio|microscopio|Placa");
        T("bio_pcr", "Extrae el ADN|mesa_laboratorio|Pipeta", "Prepara la PCR en el termociclador|termociclador|Tubos", "Carga el secuenciador|secuenciador|Tubos");
        T("bio_fotobio", "Toma una muestra|fotobiorreactor|Vial", "Mide densidad y pH|mesa_laboratorio|Tablet", "Cosecha la espirulina|fotobiorreactor|Malla");
        T("nuc_turno", "Pasa el control de acceso|acceso|Credencial", "Vigila la sala de control|consola|Tablet", "Revisa la calefacción|intercambiador|Linterna");
        T("nuc_vigilancia", "Lee las estaciones del casquete|consola|Tablet", "Comprueba tu dosímetro|acceso|Dosímetro", "Revisa los filtros de la chimenea|consola|Linterna");
        T("nuc_pasivos", "Revisa la toma de agua de mar|intercambiador|Linterna", "Prueba las válvulas del tronco|consola|Tablet");
        T("nuc_simulacro", "Da el aviso del simulacro|consola|Tablet", "Comprueba válvulas y balizas|consola|", "Saca al turno por el acceso|acceso|Credencial");
        T("nuc_roca", "Lee extensómetros y convergencia|panel_roca|Tablet", "Lee bulones, microsismos y aforos|panel_roca|Tablet", "Firma el parte de la roca|panel_roca|Tablet");
        T("nuc_bajada", "Ponte el dosímetro en el andén|anden|Dosímetro", "Revisa el carro y la esclusa|carro|Linterna", "Firma la bajada en la caseta|caseta|Tablet");
        T("nuc_refugio", "Ponte el dosímetro en el andén|anden|Dosímetro", "Comprueba aire, agua y baterías|carro|Tablet", "Prueba el teléfono del refugio|caseta|Tablet");
        T("nuc_remolque", "Mira el iceberg en el radar|panel_roca|Tablet", "Da el rumbo al remolcador|panel_roca|Tablet", "Comprueba que se desvía|panel_roca|Tablet");
        T("lanz_remolque", "Mira el iceberg en el radar|consola|Tablet", "Da el rumbo al remolcador|consola|Tablet", "Comprueba que se desvía|consola|Tablet");
        T("lanz_vaciado", "Da la orden de vaciar|consola|Tablet", "Comprueba que los tanques se vacían|consola|Tablet");
        T("ene_remolque", "Mira el iceberg en el radar|consola|Tablet", "Da el rumbo al remolcador|consola|Tablet", "Comprueba que se desvía|consola|Tablet");
        T("ene_parada", "Para los aerogeneradores|consola|Tablet", "Para la turbina mareal|consola|Tablet");
        T("nuc_refuerzo", "Cierra las cavernas|panel_roca|Tablet", "Manda reforzar a distancia|panel_roca|Tablet", "Vigila que el ritmo baje|panel_roca|Tablet");
        // ══ GUARDIA ══
        T("gua_carnivoros", "Coge la linterna|almacen|Linterna", "Patrulla los puestos|recinto|Linterna", "Anota la ronda|recinto|Tablet");
        foreach (string ali in new[] { "gua_alimentador", "gua_alimentador_2" })
        {
            T(ali, "Coge las raciones del almacén|almacen|Cubo de carne", "Busca al primero de la lista, desde lejos|depredador|Prismáticos", "Evalúa y lanza su ración o márcalo|depredador|Cubo de carne", "Ve a por el siguiente|depredador|Cubo de carne");
            Elsewhere(ali, 0, "Carne Cultivada");
        }
        // ══ INFORMÁTICA Y AULA ══
        T("ifo_sysadmin", "Revisa las colas y las alertas|consola|Tablet", "Lanza las copias y los parches|consola|", "Firma el parte|consola|Tablet");
        T("ifo_refrigeracion", "Coge las herramientas|banco_taller|Llave inglesa", "Cambia los filtros y busca fugas|rack|Filtro", "Revisa bombas e intercambiadores|rack|Linterna");
        T("ifo_energia", "Revisa la acometida|cuadro|Linterna", "Prueba el SAI y las baterías|sai|Tablet");
        T("aul_clase", "Prepara la pizarra|pizarra|Tiza", "Da o sigue la clase|pizarra|Libro", "Recoge los apuntes|mesa|Libro");
        T("aul_estudio", "Coge un libro|estante|Libro", "Estudia en la mesa|mesa|Libro", "Devuelve el libro|estante|Libro");
        // ══ SALA DE ASAMBLEAS ══
        T("asa_orden", "Reúne los temas en la mesa de la comisión|comision|Tablet", "Sortea quién modera|kleroterion|", "Ponlos en el marcador|marcador|Tablet");
        T("asa_acta", "Escribe lo acordado|acta|Tablet", "Guárdalo en el archivo|acta|");
        T("asa_mediar", "Escucha a una parte en la toguna|toguna|", "Escucha a la otra|toguna|", "Apunta el acuerdo|toguna|Tablet");
        // ══ MÚSICA ══
        T("son_grabar", "Prepara los micrófonos en la cabina|cabina|Tablet", "Graba las tomas|cabina|", "Mézclalas en la mesa|mezclas|Tablet");
        T("son_ensayo", "Afina los instrumentos|ensayo|Llave inglesa", "Ensaya en la sala|ensayo|", "Prueba en el escenario|escenario|");
        T("son_equipo", "Revisa cuerdas y cables|tecnico|Llave inglesa", "Prueba micrófonos y altavoces|escenario|Tablet");
        T("aul_legal", "Saca los contratos y los permisos|estante|Carpeta", "Revisa las normas y los acuerdos en la mesa|mesa|Contrato", "Archiva lo firmado|estante|Carpeta");
        T("aul_lenguas", "Reúne los documentos por traducir|mesa|Diccionario", "Tradúcelos|mesa|Diccionario", "Da la clase de idiomas|pizarra|Tiza");
        T("aul_letras", "Prepara la lectura|estante|Libro", "Da la clase de poesía y narración|pizarra|Tiza", "Lee en voz alta en la sala de estudio|mesa|Libro");
        // ══ LOS PUESTOS NUEVOS DEL PERSONAL (2026-11-04, docs/personajes-mesocosmos.md) ══
        T("coc_cuentas", "Recoge los ingresos del restaurante|barra|Libro de cuentas", "Anota las compras|mesa|Libro de cuentas", "Cuadra el presupuesto|mesa|Tablet");
        T("coc_avisos", "Prepara los avisos del santuario|mesa|Hoja de avisos", "Dalos en el comedor|mesa|Hoja de avisos", "Haz el programa de radio|mesa|Micrófono");
        T("con_proyectos", "Revisa el plan y los plazos|almacen|Planos", "Comprueba el material|almacen|Tablet", "Asigna los equipos en la obra|obra|Planos");
        T("con_proyectos2", "Sigue las obras en curso|obra|Planos", "Coordina a los equipos|obra|Tablet", "Pasa los avisos de la obra|almacen|Tablet");
        T("yog_ashtanga", "Elige la secuencia del día|esterilla|Tablet", "Prepara las variaciones y los ajustes de cada alumno|esterilla|Tablet", "Deja la sala lista|esterilla|");
        T("hos_plantas", "Coge las plantas medicinales|armario|Plantas medicinales", "Prepara infusiones, cataplasmas y ungüentos|mesa|Mortero", "Llévaselos a los pacientes|cama|Infusión");
        T("hos_pediatria", "Prepara la consulta y las vacunas|armario|Jeringa", "Pesa y mide al niño en la camilla|camilla|Tablet", "Vacúnalo y anota su crecimiento|camilla|Jeringa");
        T("mec_alfombras", "Revisa los circuitos y los LED|banco_taller|Tablet", "Repara el tejido|banco_taller|Aguja", "Reparte las alfombras por las pistas|pista|Alfombra");
        T("lan_control", "Sigue la lanzadera en las pantallas|consola|Tablet", "Comprueba el tiempo|consola|Tablet", "Sigue los vuelos|consola|");
        // ══ LANZADERA DE COHETES ══
        T("lanz_control", "Revisa la telemetría|consola|Tablet", "Mira el viento en altura y el techo de nubes|consola|Tablet", "Lleva la cuenta atrás|consola|");
        T("lanz_seguridad", "Prueba el diluvio de agua|plataforma|Llave", "Limpia la trinchera de llamas|trinchera|Escoba", "Revisa el escudo y la cubeta|plataforma|Linterna");
        T("lanz_fabrica", "Integra la primera etapa|etapa|Llave inglesa", "Integra la segunda etapa|etapa|Llave inglesa", "Monta la carga útil|carga|Tablet");
        // ══ EL PARQUE ENERGÉTICO (2026-11-05, docs/energia.md) ══
        T("sol_mantenimiento", "Coge el cepillo del taller|banco_taller|Cepillo", "Quita la escarcha de las mesas|panel|Cepillo", "Revisa los módulos y el cableado|panel|Tablet");
        T("sol_instalacion", "Prepara los módulos en el taller|banco_taller|Panel solar", "Monta la mesa|panel|Llave inglesa", "Conéctala y prueba la cadena|panel|Tablet");
        T("sol_reparacion", "Coge los repuestos|estante|Panel solar", "Cambia el módulo roto|panel|Destornillador", "Comprueba que la baliza se apaga|panel|Tablet");
        T("ene_control", "Mira el balance de oferta y demanda|consola|Tablet", "Anota las paradas por fauna|consola|Tablet");
        T("ene_eolica", "Bloquea la turbina al pie de la torre|aerogenerador|Llave", "Revisa cámaras y radar|detector|Tablet", "Comprueba las palas y la pala negra|aerogenerador|Linterna");
        T("ene_mareal", "Sal a la pontona|pontona|Linterna", "Prueba el sonar|sonar|Tablet", "Revisa el anillo y las fugas|pontona|Linterna");
        T("ene_almacen", "Lee la temperatura de las baterías|bateria|Tablet", "Revisa la calefacción|bateria|Linterna", "Mira el silo de arena|silo|Tablet");
        T("ene_ronda_aves", "Busca aves heridas al pie de la torre|aerogenerador|Linterna", "Busca entre las mesas|panel|Linterna", "Deja al ave en la caja de transporte|caja_aves|Caja");
        T("ene_disuasion", "Revisa los drones|hangar|Tablet", "Cambia las figuras de sitio|hangar|Llave inglesa", "Anota cuántas veces han salido|consola|Tablet");
        T("hos_aves", "Ponte guantes y mascarilla|armario|Guantes", "Cura al ave en su caja|refugio|Venda", "Anota cuándo se puede soltar|refugio|Tablet");
    }

    // ── LOS PASOS DE UNA MISIÓN ─────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Los pasos de la misión <paramref name="m"/>: los escritos, los de su prefijo (obras, elaboraciones,
    /// reparaciones, frutales) o, si no, los genéricos de su clase. Nunca vacío.</summary>
    public static Plan For(WorkRoster.Mission m) => m == null ? null : For(m.id, m.area, m.title, m.site, m.also);

    public static Plan For(string id, string area, string title, string site = null, string[] also = null)
    {
        var p = new Plan { id = id, area = area, title = title ?? id, site = site };
        id = WorkRoster.LessonBase(id);   // (la lección: los pasos y el gesto de la tarea que aprende; su efecto no, porque la misión es otra)
        AreaFunctions.AreaFunction f = AreaFunctions.Get(id);
        AreaFunctions.Kind kind = f != null ? f.kind : KindOf(id);
        if (id != null && _table.TryGetValue(id, out string[] rows))
        {
            p.authored = true;
            for (int i = 0; i < rows.Length; i++)
            {
                Step s = Parse(rows[i], kind);
                if (_stepArea.TryGetValue(id + "#" + i, out string other)) s.area = other;
                p.steps.Add(s);
            }
            return p;
        }
        if (id != null && id.StartsWith("prep_"))
        {
            string what = id.Substring(5);
            p.authored = true;
            p.steps.Add(Parse("Saca los ingredientes|armario|Ingredientes", AreaFunctions.Kind.Cook));
            p.steps.Add(Parse($"Elabora {what}|mesa|Cuenco", AreaFunctions.Kind.Cook));
            p.steps.Add(Parse($"Deja {what} en su sitio (reposo, horno o cámara)|horno|Tarro", AreaFunctions.Kind.Cook));
            return p;
        }
        if (id != null && id.StartsWith("obra_material_"))
        {
            p.authored = true;
            p.steps.Add(Parse("Carga el material del día en el almacén|almacen|Guaduas", AreaFunctions.Kind.Build));
            p.steps.Add(Parse($"Llévalo a la obra «{site ?? "?"}»|obra|Guaduas", AreaFunctions.Kind.Build));
            p.steps.Add(Parse("Descárgalo junto a la estructura|obra|", AreaFunctions.Kind.Build));
            return p;
        }
        if (id != null && id.StartsWith("obra_trabajo_"))
        {
            p.authored = true;
            p.steps.Add(Parse("Coge las herramientas|almacen|Martillo", AreaFunctions.Kind.Build));
            p.steps.Add(Parse("Monta la estructura de guadua|obra|Guaduas", AreaFunctions.Kind.Build));
            p.steps.Add(Parse("Rellena con bahareque y coloca los paneles|obra|Cubo de barro", AreaFunctions.Kind.Build));
            return p;
        }
        if (id != null && id.StartsWith("rep_"))
        {
            string where = also != null && also.Length > 0 ? also[0] : id.Substring(4);
            p.authored = true;
            p.steps.Add(Parse("Coge las herramientas del almacén|almacen|Caja de herramientas", AreaFunctions.Kind.Make));
            Step fix = Parse($"Repara en {where}: {Tail(title)}|maquina|Martillo", AreaFunctions.Kind.Make); fix.area = where; p.steps.Add(fix);
            p.steps.Add(Parse("Devuelve las herramientas|almacen|", AreaFunctions.Kind.Make));
            return p;
        }
        if (id != null && FruitGardener.IsRound(id))
        {
            p.authored = true;
            p.steps.Add(Parse("Coge las tijeras de podar|bancal|Tijeras", AreaFunctions.Kind.Tend));
            p.steps.Add(Parse("Revisa los frutales de los techos|frutal|Tijeras", AreaFunctions.Kind.Tend));
            p.steps.Add(Parse("Poda y poliniza|frutal|Pincel", AreaFunctions.Kind.Tend));
            return p;
        }
        // Genéricos de su clase (las funciones de los microcosmos y las que se añadan sin pasos escritos).
        string st = f != null && f.stations.Length > 0 ? f.stations[0] : "area";
        string verb = AreaFunctions.Verb(kind).ToLowerInvariant();
        p.steps.Add(Parse($"Prepárate en tu puesto ({StationName(st)})|{st}|", kind));
        p.steps.Add(Parse($"{Capital(title ?? verb)}|{(f != null && f.stations.Length > 1 ? f.stations[1] : st)}|", kind));
        p.steps.Add(Parse($"Termina y recoge|{st}|", kind));
        return p;
    }

    static string Tail(string title) { if (string.IsNullOrEmpty(title)) return "lo roto"; int c = title.IndexOf(':'); return c >= 0 ? title.Substring(c + 1).Trim() : title; }
    static string Capital(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

    static Step Parse(string row, AreaFunctions.Kind kind)
    {
        string[] c = row.Split('|');
        Step step = new Step { action = c[0], station = c.Length > 1 && c[1] != "" ? c[1] : "area", item = c.Length > 2 && c[2] != "" ? c[2] : null, gesture = kind };
        // Lo que estos pasos hacen de verdad en el mundo: el alimentador atiende a uno de la lista; el deshierbe levanta las plantas del bancal.
        if (step.station == "depredador") step.effect = p => LessonPrompt.Show(PredatorRound.PlayerVisit(p), null, 8f);
        if (step.station == "incinerador") step.effect = p => WeedBench.IncinerateBags("Kushal");
        if (step.action.StartsWith("Levanta las malas hierbas")) step.effect = p => WeedBench.LiftNear(p.transform.position, "Kushal", 3);
        return step;
    }

    static AreaFunctions.Kind KindOf(string id)
    {
        if (id == null) return AreaFunctions.Kind.Care;
        if (id.StartsWith("prep_")) return AreaFunctions.Kind.Cook;
        if (id.StartsWith("obra_") || id.StartsWith("mat_")) return AreaFunctions.Kind.Build;
        if (id.StartsWith("rep_")) return AreaFunctions.Kind.Make;
        if (id.StartsWith("lanz_") || id.StartsWith("nuc_")) return AreaFunctions.Kind.Operate;
        if (id.StartsWith("sol_") || id == "ene_eolica" || id == "ene_mareal") return AreaFunctions.Kind.Make;
        if (id.StartsWith("ene_")) return AreaFunctions.Kind.Operate;
        if (id.StartsWith("bio_")) return AreaFunctions.Kind.Study;
        if (FruitGardener.IsRound(id)) return AreaFunctions.Kind.Tend;
        return AreaFunctions.Kind.Care;
    }

    /// <summary>Nombre legible de un puesto («banco_taller» → «banco de taller»).</summary>
    public static string StationName(string key) => string.IsNullOrEmpty(key) || key == "area" ? "el área" : key.Replace('_', ' ').Replace("fogon", "fogón").Replace("almacen", "almacén").Replace("vehiculo", "vehículo").Replace("maquina", "máquina");

    // ── LOS SITIOS: dónde está cada puesto en la escena ──────────────────────────────────────────────────────────────

    /// <summary>Un sitio marcado: dónde se pulsa la E, el nombre de lo que hay allí y si es un mueble de verdad (found) o un
    /// punto del área (cuando el área no tiene ese mueble todavía: la tarea nunca se queda sin sitio).</summary>
    public struct Spot { public Vector3 pos; public string label; public bool found; }

    /// <summary>Palabras (sin tildes, en minúsculas) con que se busca cada puesto entre los objetos del área.</summary>
    static readonly Dictionary<string, string[]> Keywords = new Dictionary<string, string[]>
    {
        ["bancal"] = new[] { "bancal", "plot", "huerto", "parcela" },
        ["fogon"] = new[] { "fogon", "plancha", "estufa", "cocina_", "hornilla" },
        ["horno"] = new[] { "horno" },
        ["mesa"] = new[] { "mesa", "emplatado", "pase" },
        ["barra"] = new[] { "barra", "mostrador" },
        ["camilla"] = new[] { "camilla" },
        ["cama"] = new[] { "cama" },
        ["armario"] = new[] { "armario", "estante", "despensa", "inventario", "taquilla" },
        ["mesa_operaciones"] = new[] { "quirofano", "mesa de operaciones", "mesaoperaciones", "mesa_operaciones" },
        ["esterilla"] = new[] { "esterilla", "tarima", "salayoga", "mat_" },
        ["banco_taller"] = new[] { "banco", "taller", "herramient" },
        ["telar"] = new[] { "telar" },
        ["tina"] = new[] { "tina", "tinte", "cuba" },
        ["biorreactor"] = new[] { "biorreactor", "reactor" },
        ["consola"] = new[] { "consola", "control", "monitor", "pantalla" },
        ["mesa_laboratorio"] = new[] { "laboratorio", "poyata", "mesa_lab", "mesa de laboratorio", "mesa" },
        ["pizarra"] = new[] { "pizarra", "pizarron" },
        ["almacen"] = new[] { "almacen", "deposito", "material" },
        ["recinto"] = new[] { "recinto", "corral", "cercado", "paja", "nido" },
        ["comedero"] = new[] { "comedero", "bebedero" },
        ["maquina"] = new[] { "maquina", "prensa", "polea" },
        ["incinerador"] = new[] { "incinerador" },
        ["aseos"] = new[] { "aseo", "retrete", "lavabo", "bano" },
        ["obra"] = new[] { "obra", "estructura" },
        ["barro"] = new[] { "barro", "bahareque", "material" },
        ["vehiculo"] = new[] { "vehiculo", "coche", "carro", "moto", "submarino" },
        ["frutal"] = new[] { "frutal", "arbol", "maceta" },
        ["campana"] = new[] { "campana" },
        ["cabina"] = new[] { "cabina" },
        ["incubador"] = new[] { "incubador" },
        ["microscopio"] = new[] { "microscopio" },
        ["termociclador"] = new[] { "termociclador", "pcr" },
        ["secuenciador"] = new[] { "secuenciador" },
        ["fotobiorreactor"] = new[] { "fotobio", "espirulina", "tubo" },
        ["pendulo"] = new[] { "pendulo", "foucault" },
        ["bomba"] = new[] { "bomba", "torre" },
        ["acceso"] = new[] { "acceso", "puerta", "torno" },
        ["intercambiador"] = new[] { "intercambiador", "calefaccion" },
        ["rack"] = new[] { "rack", "servidor", "refrigeracion" },
        ["cuadro"] = new[] { "cuadro", "acometida", "electrico" },
        ["sai"] = new[] { "sai", "bateria" },
        ["estante"] = new[] { "estante", "biblioteca", "libro" },
        ["plataforma"] = new[] { "plataforma", "diluvio", "escudo" },
        ["trinchera"] = new[] { "trinchera", "llamas" },
        ["etapa"] = new[] { "etapa", "cohete" },
        ["carga"] = new[] { "carga", "cofia" },
        ["pista"] = new[] { "alfombras_", "pista" },
        ["panel"] = new[] { "mesa_solar" },
        ["aerogenerador"] = new[] { "aerogenerador" },
        ["detector"] = new[] { "detector_aves" },
        ["pontona"] = new[] { "pontona_mareal" },
        ["sonar"] = new[] { "sonar_mareal" },
        ["bateria"] = new[] { "bateria_lfp" },
        ["silo"] = new[] { "silo_arena" },
        ["caja_aves"] = new[] { "caja_transporte_aves" },
        ["hangar"] = new[] { "hangar_drones" },
        ["refugio"] = new[] { "refugio_aves" },
    };

    // (por área y puesto: los candidatos, cacheados unos segundos: el test recorre todas las misiones de golpe)
    static readonly Dictionary<string, (List<Transform> list, float at)> _cache = new Dictionary<string, (List<Transform>, float)>();

    /// <summary>El sitio del paso <paramref name="nth"/> (0, 1, 2…) de una misión: el mueble del <paramref name="station"/> en
    /// <paramref name="area"/> (o en la obra <paramref name="site"/>); el n-ésimo si hay varios (para repartir los pasos); si el
    /// área no lo tiene, un punto del área; si el área no está en la escena, junto a <paramref name="near"/>.</summary>
    public static Spot Resolve(string area, string station, int nth, string site, Transform near)
    {
        // El alimentador va a donde esté el depredador: el pendiente enésimo de la lista compartida, y a su distancia, no a su lado.
        if (station == "depredador")
        {
            var open = PredatorRound.Today().FindAll(e => e.animal != null && e.state == PredatorRound.State.Pending);
            if (open.Count > 0)
            {
                Animal a = open[Mathf.Abs(nth) % open.Count].animal;
                return new Spot { pos = Ground(PredatorRound.StandOff(a, near != null ? near.position : Vector3.zero)), label = $"lejos de {a.name}", found = true };
            }
        }
        List<Transform> c = Candidates(area, station, site);
        if (c.Count > 0)
        {
            Transform t = c[Mathf.Abs(nth) % c.Count];
            return new Spot { pos = Beside(t, area), label = Pretty(t.name), found = true };
        }
        Vector3 p = WorkdayDirector.AreaPoint(area, "Tarea" + nth);
        if (p != Vector3.zero) return new Spot { pos = Ground(p), label = area, found = false };
        Vector3 o = near != null ? near.position : Vector3.zero;
        Vector3 fw = near != null ? near.forward : Vector3.forward; fw.y = 0f; if (fw.sqrMagnitude < 1e-4f) fw = Vector3.forward;
        return new Spot { pos = o + Quaternion.Euler(0f, nth * 50f - 50f, 0f) * fw.normalized * 3f, label = "aquí mismo", found = false };
    }

    static List<Transform> Candidates(string area, string station, string site)
    {
        string key = $"{area}|{station}|{site}";
        if (_cache.TryGetValue(key, out var hit) && Time.realtimeSinceStartup - hit.at < 10f && hit.list.TrueForAll(t => t != null)) return hit.list;
        var l = new List<Transform>();
        // 1. Por componente (los puestos que ya tienen su pieza de juego).
        if (station == "obra" && site != null)
            foreach (ConstructionProject cp in ConstructionProject.All) if (cp != null && cp.title == site) l.Add(cp.transform);
        if (station == "bancal" || station == "frutal")
        {
            Transform root = WorkdayDirector.AreaTransform(area);
            var plots = new List<GardenPlot>(); foreach (GardenPlot g in GardenPlot.All) if (g != null) plots.Add(g);
            if (root != null) plots.Sort((a, b) => (a.transform.position - root.position).sqrMagnitude.CompareTo((b.transform.position - root.position).sqrMagnitude));
            foreach (GardenPlot g in plots) l.Add(g.transform);
        }
        if (station == "biorreactor") foreach (CulturedMeatLab lab in CulturedMeatLab.All) if (lab != null) l.Add(lab.transform);
        if (station == "maquina") foreach (GymStation g in GymStation.All) if (g != null && (area == null || area == "Gimnasio" || area.StartsWith("Mec"))) l.Add(g.transform);
        if (station == "esterilla" && l.Count == 0)
        {
            YogaHall h = YogaHall.For(WorkdayDirector.HathaSession, YogaHall.Today);
            if (h != null) l.Add(h.transform);
        }
        // 2. Por nombre, entre los objetos de las áreas con ese nombre (sin personajes ni marcadores).
        if (l.Count == 0)
        {
            string[] words = Keywords.TryGetValue(station ?? "", out string[] w) ? w : new[] { Norm(station ?? "") };
            foreach (Transform root in Roots(area))
                foreach (Transform t in root.GetComponentsInChildren<Transform>())
                {
                    if (t == root || t.GetComponent<SanctuaryArea>() != null || t.GetComponent<Anima>() != null || t.GetComponentInParent<Anima>() != null || t.name.StartsWith(PlayerTaskRunner.MarkerPrefix)) continue;
                    string n = Norm(t.name);
                    foreach (string k in words) if (k.Length > 0 && n.Contains(k)) { l.Add(t); break; }
                    if (l.Count >= 24) break;
                }
            // Un mueble y sus piezas: solo el de más arriba (si no, el n-ésimo «paso» caía en la pata de la misma mesa).
            l.RemoveAll(t => l.Exists(o => o != t && t.IsChildOf(o)));
        }
        _cache[key] = (l, Time.realtimeSinceStartup);
        return l;
    }

    static IEnumerable<Transform> Roots(string area)
    {
        var l = new List<Transform>();
        if (string.IsNullOrEmpty(area)) return l;
        foreach (SanctuaryArea a in SceneCache.All<SanctuaryArea>(2f)) if (a != null && a.displayName == area) l.Add(a.transform);
        if (l.Count == 0) { Transform t = WorkdayDirector.AreaTransform(area); if (t != null) l.Add(t); }
        if (area == "Huerto") { GameObject g = GameObject.Find("Garden_Area"); if (g != null && !l.Contains(g.transform)) l.Add(g.transform); }
        if (area == "Cocina") { RestaurantKitchen rk = Object.FindAnyObjectByType<RestaurantKitchen>(); if (rk != null && !l.Contains(rk.transform)) l.Add(rk.transform); }
        if (area == EnergyRule.Area && EnergySite.Instance != null) l.Add(EnergySite.Instance.transform);   // el campo, las torres y el almacén, en el casquete
        return l;
    }

    // Junto al mueble (no encima): 1 m hacia el centro del área, en el suelo (NavMesh si hay). Un bancal: en su borde.
    static Vector3 Beside(Transform t, string area)
    {
        if (t.TryGetComponent(out GardenPlot gp))
        {
            Vector3 fw = t.forward; fw.y = 0f; fw = fw.sqrMagnitude > 1e-4f ? fw.normalized : Vector3.forward;
            return Ground(t.position + fw * (gp.size.y * 0.5f + 0.8f));
        }
        if (t.TryGetComponent(out GymStation gs)) return Ground(gs.ApproachPoint);
        Transform root = WorkdayDirector.AreaTransform(area);
        Vector3 d = root != null ? root.position - t.position : -t.forward; d.y = 0f;
        Vector3 p = t.position + (d.sqrMagnitude > 0.01f ? d.normalized : Vector3.forward) * 1f;
        return Ground(p);
    }

    static Vector3 Ground(Vector3 p)
    {
        if (UnityEngine.AI.NavMesh.SamplePosition(p, out UnityEngine.AI.NavMeshHit h, 2.5f, UnityEngine.AI.NavMesh.AllAreas)) return h.position;
        return p;
    }

    static string Pretty(string n) { int i = n.IndexOf('('); n = i > 0 ? n.Substring(0, i) : n; return n.Replace('_', ' ').Trim(); }

    /// <summary>En minúsculas y sin tildes (para buscar por nombre).</summary>
    public static string Norm(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (char ch in s.ToLowerInvariant())
            switch (ch)
            {
                case 'á': sb.Append('a'); break; case 'é': sb.Append('e'); break; case 'í': sb.Append('i'); break;
                case 'ó': sb.Append('o'); break; case 'ú': case 'ü': sb.Append('u'); break; case 'ñ': sb.Append('n'); break;
                default: sb.Append(ch); break;
            }
        return sb.ToString();
    }

    // ── EL GESTO de cada clase (dos fotogramas, como los pasos del huerto en AreaInitiator.SoilSteps) ────────────────
    static AsanaPoseSpell G(string n, params (BodyPart, Vector3)[] s) => DailyPoses.Make(n, -1f, s);

    /// <summary>El fotograma A o B del gesto de la clase <paramref name="k"/>.</summary>
    public static AsanaPoseSpell Gesture(AreaFunctions.Kind k, bool b)
    {
        switch (k)
        {
            case AreaFunctions.Kind.Cook: case AreaFunctions.Kind.Make: case AreaFunctions.Kind.Build:
                return b ? G("trabaja (abajo)", (BodyPart.Back, new Vector3(20, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-45, 0, 10)), (BodyPart.ForearmRight, new Vector3(-30, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-30, 0, 0)))
                         : G("trabaja (arriba)", (BodyPart.Back, new Vector3(15, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-75, 0, -10)), (BodyPart.ForearmRight, new Vector3(-45, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-30, 0, 0)));
            case AreaFunctions.Kind.Clean:
                return b ? G("limpia a la izquierda", (BodyPart.Back, new Vector3(25, 15, 0)), (BodyPart.ShoulderRight, new Vector3(-60, 0, 20)), (BodyPart.ShoulderLeft, new Vector3(-40, 0, 0)))
                         : G("limpia a la derecha", (BodyPart.Back, new Vector3(25, -15, 0)), (BodyPart.ShoulderRight, new Vector3(-60, 0, -30)), (BodyPart.ShoulderLeft, new Vector3(-40, 0, 0)));
            case AreaFunctions.Kind.Tend:
                return b ? G("azada clavada", (BodyPart.Back, new Vector3(35, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-40, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-35, 0, 0)))
                         : G("azada arriba", (BodyPart.Back, new Vector3(-5, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-150, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-140, 0, 0)), (BodyPart.ForearmRight, new Vector3(-20, 0, 0)));
            case AreaFunctions.Kind.Heal: case AreaFunctions.Kind.Care:
                return b ? G("atiende (cerca)", (BodyPart.Back, new Vector3(35, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-75, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-75, 0, 0)), (BodyPart.ForearmRight, new Vector3(-15, 0, 0)))
                         : G("atiende", (BodyPart.Back, new Vector3(25, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-60, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-55, 0, 0)), (BodyPart.ForearmRight, new Vector3(-35, 0, 0)));
            case AreaFunctions.Kind.Operate: case AreaFunctions.Kind.Study:
                return b ? G("teclea", (BodyPart.Back, new Vector3(10, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-50, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-45, 0, 0)), (BodyPart.ForearmRight, new Vector3(-40, 0, 0)), (BodyPart.ForearmLeft, new Vector3(-35, 0, 0)))
                         : G("mira la pantalla", (BodyPart.Back, new Vector3(5, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-45, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-50, 0, 0)), (BodyPart.ForearmRight, new Vector3(-35, 0, 0)), (BodyPart.ForearmLeft, new Vector3(-40, 0, 0)));
            case AreaFunctions.Kind.Teach:
                return b ? G("explica (brazos abiertos)", (BodyPart.ShoulderRight, new Vector3(-40, 0, -45)), (BodyPart.ShoulderLeft, new Vector3(-40, 0, 45)))
                         : G("señala", (BodyPart.ShoulderRight, new Vector3(-100, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-10, 0, 0)));
            case AreaFunctions.Kind.Serve: case AreaFunctions.Kind.Trade:
                return b ? G("ofrece", (BodyPart.ShoulderRight, new Vector3(-70, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-70, 0, 0)), (BodyPart.ForearmRight, new Vector3(-20, 0, 0)), (BodyPart.ForearmLeft, new Vector3(-20, 0, 0)))
                         : G("lleva la bandeja", (BodyPart.ShoulderRight, new Vector3(-40, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-40, 0, 0)), (BodyPart.ForearmRight, new Vector3(-50, 0, 0)), (BodyPart.ForearmLeft, new Vector3(-50, 0, 0)));
            case AreaFunctions.Kind.Guard:
                return b ? G("vigila a la izquierda", (BodyPart.Back, new Vector3(0, 20, 0)), (BodyPart.ShoulderRight, new Vector3(-150, 0, 0)), (BodyPart.ForearmRight, new Vector3(-90, 0, 0)))
                         : G("vigila a la derecha", (BodyPart.Back, new Vector3(0, -20, 0)), (BodyPart.ShoulderRight, new Vector3(-150, 0, 0)), (BodyPart.ForearmRight, new Vector3(-90, 0, 0)));
            case AreaFunctions.Kind.Practice:
                return b ? G("brazos arriba", (BodyPart.ShoulderRight, new Vector3(-170, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-170, 0, 0)))
                         : G("brazos al frente", (BodyPart.ShoulderRight, new Vector3(-90, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-90, 0, 0)));
            case AreaFunctions.Kind.Meditate: case AreaFunctions.Kind.Worship: case AreaFunctions.Kind.Rest:
                return DailyPoses.Meditate();
            default:
                return DailyPoses.Stand();
        }
    }
}
