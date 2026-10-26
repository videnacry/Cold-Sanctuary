using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MISIONES DE VIRTUALIZACIÓN CON ESCENA PROPIA (docs/jornada-y-rotacion.md). Todas las misiones de trabajo del catálogo
/// (AreaWork) se practican DENTRO de la máquina de virtualización, cada una en su escena «Virtual_&lt;clave&gt;»: se recorren
/// sus pasos en orden (puestos numerados, F para hacerlo) y al terminar la misión cuenta como hecha (WorkdayDirector).
/// La del HOSPITAL (antes enfermería) es iniciar a un turista: recibirlo, análisis, vacuna, normas y acompañarlo a su primera
/// virtualización (el turista te sigue por la escena).
/// Las misiones que dependen del día (elaboraciones, obras, material, reparaciones) comparten escena por familia.
/// </summary>
public static class VirtualMissions
{
    /// <summary>Clave de escena de una misión (las de familia comparten escena).</summary>
    public static string Key(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (id.StartsWith("prep_")) return "prep";
        if (id.StartsWith("obra_material_")) return "obra_material";
        if (id.StartsWith("obra_trabajo_")) return "obra_trabajo";
        if (id.StartsWith("rep_")) return "rep";
        return id;
    }

    public static string SceneName(string id) => "Virtual_" + Key(id);

    /// <summary>Todas las claves con escena (el catálogo fijo y las familias del día), con su área.</summary>
    public static IEnumerable<(string key, string area, string title)> All()
    {
        foreach (WorkRoster.Mission m in AreaWork.Catalog) yield return (m.id, m.area, m.title);
        yield return ("prep", "Cocina", "Elaboración del obrador");
        yield return ("obra_material", "Construcción", "Llevar el material del día a la obra");
        yield return ("obra_trabajo", "Construcción", "Trabajar la obra (guadua, bahareque y paneles)");
        yield return ("rep", "Construcción", "Reparación en un área");
        yield return ("mat_guadua", "Huerto", "Cortar guadua madura en el bambusal");
        yield return ("mat_bahareque", "Construcción", "Preparar barro con paja para el bahareque");
        yield return ("mat_micelio", "Carne Cultivada", "Sembrar paneles de micelio");
        yield return ("mat_cuerda", "Estudio Textil", "Trenzar cuerda de cáñamo");
    }

    /// <summary>Los pasos de la misión, en orden.</summary>
    public static string[] Steps(string id)
    {
        switch (Key(id))
        {
            case "enf_iniciacion": return ArrivalExam.MissionSteps();   // el mismo examen de llegada que hace el de turno (NewcomerInitiator)
            case "enf_visita": return new[] { "Revisar la historia de cada paciente", "Tomar constantes", "Curar las heridas", "Dar la medicación" };
            case "enf_animales": return new[] { "Sujetar al animal con calma", "Tomar la muestra", "Analizarla", "Vacunar" };
            case "enf_inventario": return new[] { "Desinfectar camillas y superficies", "Esterilizar el instrumental en el autoclave", "Contar el material", "Reponer el botiquín" };
            case "hos_urgencias": return new[] { "Triaje: ¿quién va primero?", "Vía aérea y respiración", "Circulación: vía y sueros", "Estabilizar y decidir: planta, UCI o quirófano" };
            case "hos_quirofano": return new[] { "Lista de verificación de cirugía segura (OMS)", "Lavado quirúrgico", "Anestesia", "Operar", "Pasar a cuidados intensivos" };
            case "hos_uci": return new[] { "Revisar el ventilador", "Tomar constantes", "Ajustar la medicación", "Anotar en la historia" };
            case "hos_veterinaria": return new[] { "Pesar al animal (dosis por kilo)", "Explorarlo en la mesa", "Hacerle una ecografía", "Instalarlo en su box" };
            case "coc_servicio": return new[] { "Leer las comandas", "Cocinar el primer plato", "Cocinar el segundo plato", "Emplatar", "Pasar al pase" };
            case "coc_limpieza": return new[] { "Fregar los fuegos", "Limpiar las neveras", "Contar el stock", "Apuntar lo que falta" };
            case "coc_obrador": case "prep": return new[] { "Pesar los ingredientes", "Amasar o mezclar", "Fermentar o madurar", "Guardar en la despensa" };
            case "coc_sala": return new[] { "Recibir a los turistas", "Llevarles a su mesa", "Tomar nota", "Servir" };
            case "coc_atender": return new[] { "Tomar el pedido en la barra", "Recoger el plato del pase", "Servirlo" };
            case "coc_comedor": return new[] { "Recoger las mesas", "Limpiar el comedor", "Limpiar los aseos" };
            case "hue_riego": return new[] { "Abrir el riego", "Regar las filas", "Quitar las malas hierbas" };
            case "hue_cosecha": return new[] { "Cosechar lo maduro", "Llenar las cestas", "Llevarlas a la cocina" };
            case "hue_visitantes": return new[] { "Ver qué animal tiene hambre", "Darle de comer", "Proteger el cultivo" };
            case "yog_guia": return new[] { "Subir a la plataforma central", "Guiar el saludo al sol", "Guiar las posturas", "Cerrar con la meditación" };
            case "yog_demo": return new[] { "Colocarse entre los alumnos", "Demostrar cada postura", "Sostenerla mientras la copian" };
            case "yog_corrige_1": case "yog_corrige_2": return new[] { "Observar a los alumnos", "Corregir la postura de uno", "Corregir la de otro", "Corregir la del último" };
            case "yog_sala": return new[] { "Colocar las esterillas", "Limpiar el suelo", "Ventilar la sala" };
            case "yog_aseos": case "tex_limpieza": case "mec_limpieza": case "gym_limpieza": return new[] { "Barrer", "Fregar", "Reponer lo que falte" };
            case "mec_vehiculos": return new[] { "Revisar el vehículo", "Cambiar el aceite", "Ajustar los frenos", "Probarlo" };
            case "mec_maquinas": return new[] { "Diagnosticar la máquina", "Desmontar la pieza", "Repararla", "Montarla y probar" };
            case "tex_tejer": return new[] { "Preparar el telar", "Tejer", "Teñir", "Tender a secar" };
            case "tex_coser": return new[] { "Revisar la ropa rota", "Cortar el remiendo", "Coser" };
            case "lab_biorreactor": return new[] { "Medir la glucosa", "Medir el pH", "Medir el oxígeno", "Ajustar el biorreactor" };
            case "lab_iniciar": return new[] { "Sacar un vial del banco", "Preparar el medio", "Sembrar el biorreactor" };
            case "lab_cosechar": return new[] { "Cosechar el lote", "Dar forma a la carne", "Llevarla a la cocina y al comedero" };
            case "lab_limpieza": return new[] { "Limpiar las superficies", "Esterilizar el material", "Ordenar el laboratorio" };
            case "con_almacen": return new[] { "Contar el material", "Contar las herramientas", "Apuntar lo que falta" };
            case "gym_mantener": return new[] { "Revisar las máquinas", "Engrasar y ajustar", "Probarlas" };
            case "pla_operar": return new[] { "Coger el dosímetro", "Comprobar la piscina desde el puente", "Arrancar: sacar las barras hasta crítico", "Subir a potencia y vigilar el periodo", "Parar: meter las barras" };
            case "pla_isotopos": return new[] { "Coger el dosímetro", "Mandar el molibdeno al núcleo", "Recuperar la cápsula", "Cargar el generador en la celda caliente", "Pasar por el pórtico de contaminación" };
            case "pla_ronda": return new[] { "Leer la conductividad y cambiar resinas", "Revisar la bomba y el intercambiador", "Revisar la torre de refrigeración", "Medir la tasa de dosis con el Geiger" };
            case "fis_experimentos": return new[] { "Contar fotones tras la doble rendija", "Alinear el interferómetro de Michelson", "Medir la carga de una gota de aceite", "Ver girar el plano del péndulo de Foucault" };
            case "qui_sintesis": return new[] { "Ponerse gafas y bata junto a la ducha", "Montar la reacción en la campana", "Evaporar el disolvente en el rotavapor", "Analizar el producto en el HPLC" };
            case "qui_residuos": return new[] { "Recoger los bidones de residuos", "Guardar los ácidos en su armario", "Guardar las bases en su armario", "Guardar los inflamables" };
            case "bio_cultivo": return new[] { "Sembrar células en la cabina de seguridad", "Llevarlas al incubador de CO2", "Mirar el cultivo al microscopio", "Probar un medio sin suero en el biorreactor piloto" };
            case "bio_pcr": return new[] { "Extraer el ADN de la muestra", "Preparar la mezcla en la estación pre-PCR", "Amplificar en el termociclador", "Secuenciar" };
            case "nuc_turno": return new[] { "Pasar el control de acceso", "Relevo en la sala de control", "Revisar la conversión de potencia", "Revisar el intercambiador de la calefacción" };
            case "pla_ciclotron": return new[] { "Coger el dosímetro", "Arrancar el haz del ciclotrón", "Parar el haz y mandar el F-18", "Sintetizar la FDG en la celda" };
            case "gua_carnivoros": return new[] { "Recoger las raciones", "Patrullar el perímetro", "Dar las raciones a los carnívoros" };
            case "obra_material": return new[] { "Cargar el material en el almacén", "Llevarlo a la obra", "Descargarlo a pie de obra" };
            case "obra_trabajo": return new[] { "Montar la estructura de guadua", "Rellenar con bahareque", "Colocar los paneles" };
            case "rep": return new[] { "Encontrar la avería", "Traer las herramientas", "Repararla", "Comprobarla" };
            case "mat_guadua": return new[] { "Elegir la guadua madura", "Cortarla", "Curarla y tratarla con bórax" };
            case "mat_bahareque": return new[] { "Cavar el barro", "Mezclarlo con paja", "Amasarlo" };
            case "mat_micelio": return new[] { "Preparar la paja y la cañamiza", "Llenar los moldes", "Sembrar el micelio" };
            case "mat_cuerda": return new[] { "Separar la fibra de cáñamo", "Hilarla", "Trenzar la cuerda" };
            default: return new[] { "Preparar", "Hacer la tarea", "Recoger" };
        }
    }


    /// <summary>
    /// Dónde se hace cada paso EN EL ÁREA REAL: nombre de un objeto del área (o «@entrada», «@maquina»); null = un mueble
    /// del área elegido solo. Así la virtualización reutiliza la enfermería, la cocina… de verdad.
    /// </summary>
    public static string[] Anchors(string id)
    {
        switch (Key(id))
        {
            case "enf_iniciacion": return ArrivalExam.MissionAnchors();
            case "enf_visita": return new[] { "Cama 1", "Cama 2", "Cama 3", "Botiquin" };
            case "enf_animales": return new[] { "Cuadra 1", "Mesa veterinaria", "Laboratorio", "Mesa veterinaria" };
            case "enf_inventario": return new[] { "Camilla", "Autoclave", "Botiquin", "Botiquin" };
            case "hos_urgencias": return new[] { "Triaje", "Carro_de_parada", "Carro_de_parada", "Cama 1" };
            case "hos_quirofano": return new[] { "Quirofano", "Lavado_quirurgico", "Anestesia", "Quirofano", "UCI 1" };
            case "hos_uci": return new[] { "UCI 1", "UCI 2", "UCI 3", "Control_UCI" };
            case "hos_veterinaria": return new[] { "Bascula", "Mesa veterinaria", "Ecografo veterinario", "Box 1" };
            case "pla_operar": return new[] { "Taquilla_dosimetros", "Puente_piscina", "Consola_reactor", "Consola_reactor", "Consola_reactor" };
            case "pla_isotopos": return new[] { "Taquilla_dosimetros", "Conejo_neumatico", "Conejo_neumatico", "Celda_caliente", "Portico_contaminacion" };
            case "pla_ronda": return new[] { "Purificacion", "Bomba_e_intercambiador", "Torre_refrigeracion", "Contador_Geiger" };
            case "fis_experimentos": return new[] { "Bloque_Doble rendija de fotón único", "Bloque_Interferómetro de Michelson 1", "Bloque_Gota de aceite de Millikan 1", "Bloque_Péndulo de Foucault" };
            case "qui_sintesis": return new[] { "Bloque_Ducha de seguridad 1", "Bloque_Campana de gases 1", "Bloque_Rotavapor 1", "Bloque_HPLC 1" };
            case "qui_residuos": return new[] { "Bloque_Residuos químicos", "Bloque_Armario de ácidos", "Bloque_Armario de bases", "Bloque_Armarios de inflamables" };
            case "bio_cultivo": return new[] { "Bloque_Cabina de seguridad biológica 1", "Bloque_Incubador de CO2 1", "Bloque_Microscopio invertido 1", "Bloque_Biorreactor piloto (50 L)" };
            case "bio_pcr": return new[] { "Bloque_Mesa de extracción de ADN 1", "Bloque_Estación pre-PCR 1", "Bloque_Termocicladores 1", "Bloque_Secuenciadores (MiSeq y MinION)" };
            case "nuc_turno": return new[] { "Bloque_Puesto de acceso", "Bloque_Sala de control", "Bloque_Conversión de potencia", "Bloque_Intercambiador de la calefacción de distrito" };
            case "pla_ciclotron": return new[] { "Taquilla_dosimetros", "Consola_ciclotron", "Consola_ciclotron", "Celda_sintesis_FDG" };
            default: return null;
        }
    }
    public static Color Tint(string area)
    {
        switch (area)
        {
            case AreaWork.Hospital: return new Color(0.85f, 0.95f, 0.9f);
            case "Cocina": return new Color(0.75f, 0.45f, 0.3f);
            case "Huerto": return new Color(0.5f, 0.7f, 0.4f);
            case "Sala de Yoga": return new Color(0.75f, 0.65f, 0.85f);
            case "Mecánica": return new Color(0.55f, 0.55f, 0.6f);
            case "Estudio Textil": return new Color(0.85f, 0.6f, 0.65f);
            case "Carne Cultivada": return new Color(0.9f, 0.75f, 0.75f);
            case "Gimnasio": return new Color(0.5f, 0.55f, 0.65f);
            case "Construcción": return new Color(0.7f, 0.6f, 0.45f);
            case "Planta Atómica": return new Color(0.8f, 0.82f, 0.78f);
            case "Química": return new Color(0.78f, 0.85f, 0.9f);
            case "Biología": return new Color(0.78f, 0.9f, 0.78f);
            case "Central Nuclear": return new Color(0.75f, 0.78f, 0.8f);
            default: return new Color(0.6f, 0.65f, 0.7f);
        }
    }
}
