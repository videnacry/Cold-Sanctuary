using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UN EDIFICIO QUE HACE FUNCIONES (docs/microcosmos-civilizaciones.md §2): dice qué plantilla del registro es
/// (<see cref="AreaFunctions.Buildings"/>: «Cocina», «Restaurante», «Hospital», «Dojo», «Sala de Yoga», «Huerto», «Taller»…)
/// y sus muebles (<see cref="AreaFunctionStation"/>) qué puesto cubre cada uno. Lo que se puede hacer en él sale del registro
/// filtrado por el año de la escena (<see cref="Eras.Now"/>). Lo usan los insectos-humanos en su jornada
/// (<see cref="MicroJornada"/>) y el jugador (interactuando con un puesto).
/// </summary>
public class AreaFunctionSite : MonoBehaviour
{
    [Tooltip("La plantilla del registro (AreaFunctions.Buildings).")]
    public string building = "Casa";
    [Tooltip("Su nombre de verdad («Restaurante de Ruby Chow, Seattle»).")]
    public string title = "";
    [Tooltip("La región o cultura de este edificio (MicroStyles.Region): con el año, el estilo de sus muebles.")]
    public string region = "";

    // ── LAS ETIQUETAS DE ACTIVIDAD (2026-11-03, docs/microcosmos-actividades.md; Berón: «que deben crearse junto a su interacción
    //    con los humanos y humanos-insecto … para poder pertenecer a sus áreas de trabajo, descanso, recuperación u ocio, de modo
    //    que formen parte de sus actividades semanales»). Las pone el constructor (CivicBuilder.Tag) y las LEE el horario semanal:
    //    qué se hace aquí, para cuántos y para qué cuerpos.
    // (2026-11-04) Berón: «¿en ninguna época de microcosmos hay gimnasio? … estructuras de ocio/entrenamiento/trabajo/recuperación»:
    // el ENTRENAMIENTO es su propia categoría (antes los centros de entrenamiento y los dojos contaban como ocio).
    public const string Trabajo = "trabajo", Descanso = "descanso", Recuperacion = "recuperacion", Ocio = "ocio", Entrenamiento = "entrenamiento";
    public static readonly string[] Activities = { Trabajo, Descanso, Recuperacion, Ocio, Entrenamiento };
    /// <summary>(2026-11-04, regla 16 de los microcosmos) Las cuatro categorías que TODO pueblo de historia debe tener (al menos un
    /// edificio de cada una; MicroFloors.Validate). El descanso lo dan las casas.</summary>
    public static readonly string[] TownCategories = { Trabajo, Ocio, Entrenamiento, Recuperacion };
    [Tooltip("(2026-11-04) Si es de entrenamiento: su CLASE ESTABLE de época y lugar (gimnasio_griego, akhara, zurkhaneh, wuguan, dojo, turnhalle, juego_pelota…; MicroStyles.TrainingKind). La leen los planes de entrenamiento de los insectos-humanos (docs/gimnasio-maquinas.md).")]
    public string training = "";
    [Tooltip("(2026-11-04) De dónde sale su POSICIÓN: vacío = del plano histórico o de la historia; «mapa actual: …» = de un mapa de hoy (el plano histórico no lo tenía; se recrea con lo nativo de la época); «FALTA …» = puesto sin posición (lo marca el validador).")]
    public string mapSource = "";
    [Tooltip("Su categoría de actividad principal (para quien lo visita): trabajo, descanso, recuperacion, ocio o entrenamiento.")]
    public string activity = "";
    [Tooltip("¿Tiene además PUESTOS DE TRABAJO (quien lo atiende trabaja aquí)?")]
    public bool hasWork;
    [Tooltip("Cuántas personas lo usan a la vez (camas, asientos, puestos, sitio en lo que se pisa).")]
    public int capacity;
    [Tooltip("La interacción que ofrece, en una frase (lo que hacen aquí humanos e insectos-humanos).")]
    public string interaction = "";
    [Tooltip("Los cuerpos para los que está adaptado (MicroBody.Plan): «Humano, Centauro…».")]
    public string bodies = "";

    /// <summary>La categoría de actividad de cada plantilla del registro (lo que el horario semanal busca).</summary>
    public static string ActivityOf(string building)
    {
        switch (building)
        {
            case "Casa": case "Orfanato": case "Hotel": return Descanso;
            case "Hospital": case "Sala de Yoga": case "Baños": case "Templo": return Recuperacion;
            case "Restaurante": case "Parque": case "Plaza": case "Museo": case "Sala de baile": case "Mirador": case "Fuente pública": return Ocio;
            case "Centro de entrenamiento": case "Dojo": return Entrenamiento;
            default: return Trabajo;
        }
    }

    /// <summary>La interacción de cada plantilla, en una frase (para humanos e insectos-humanos).</summary>
    public static string InteractionOf(string building)
    {
        switch (building)
        {
            case "Casa": return "dormir, comer en familia, asearse, cuidar de los hijos y coger fruta del frutal";
            case "Orfanato": return "cuidar y esconder a los niños, darles de comer y acostarlos";
            case "Hotel": return "llegar de viaje, dormir en una habitación y comer";
            case "Hospital": return "pasar visita, curar, vacunar y descansar en cama";
            case "Sala de Yoga": return "practicar posturas y meditar";
            case "Baños": return "bañarse, nadar o sudar en el baño de vapor";
            case "Templo": return "rezar, ofrendar y meditar";
            case "Restaurante": return "comer, servir y charlar a la mesa";
            case "Parque": return "pasear a la sombra, sentarse junto a la fuente y coger fruta";
            case "Plaza": return "reunirse, coger agua en la fuente y contemplar el monumento";
            case "Museo": return "ver la colección y cuidarla";
            case "Sala de baile": return "bailar y aprender los pasos";
            case "Mirador": return "asomarse desde la meseta del mueble gigante";
            case "Centro de entrenamiento": case "Dojo": return "entrenar el cuerpo con un maestro";
            case "Fuente pública": return "coger agua y lavar";
            case "Cocina": return "cocinar la comida del día";
            case "Taller": return "reparar y fabricar";
            case "Escuela": return "enseñar, estudiar y escribir";
            case "Mercado": return "vender y comprar";
            case "Laboratorio": return "experimentar, medir y anotar";
            case "Comisaría": return "atender avisos y hacer la ronda";
            case "Bomberos": return "guardia con la bomba y ejercicios de incendio";
            case "Guardería": return "cuidar y jugar con los pequeños";
            case "Estación": return "esperar y subir al transporte, vender billetes";
            case "Carpintería": return "serrar, cepillar y ensamblar madera";
            case "Obra": return "levantar la obra y preparar el material";
            case "Centro de tecnología": return "operar los aparatos de la época";
            case "Cuartel": return "guardia e instrucción";
            case "Huerto": return "sembrar, regar y cosechar";
            default: return "trabajar en sus puestos";
        }
    }

    public static readonly List<AreaFunctionSite> All = new List<AreaFunctionSite>();
    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    /// <summary>Las funciones que se hacen aquí en el año de la escena.</summary>
    public List<AreaFunctions.AreaFunction> Functions(int? year = null) => AreaFunctions.ForSite(building, year ?? Eras.Now);

    public AreaFunctionStation[] Stations => GetComponentsInChildren<AreaFunctionStation>(true);

    /// <summary>Un puesto libre que sirva para <paramref name="f"/> (o null).</summary>
    public AreaFunctionStation StationFor(AreaFunctions.AreaFunction f, Object who = null)
    {
        AreaFunctionStation any = null;
        foreach (AreaFunctionStation s in Stations)
        {
            if (f != null && f.stations.Length > 0 && System.Array.IndexOf(f.stations, s.station) < 0) continue;
            if (s.occupant == null || s.occupant == who) return s;
            if (any == null) any = s;
        }
        return any;
    }

    /// <summary>Una función de aquí para el papel <paramref name="role"/> que tenga puesto (la que toca según la hora).</summary>
    public AreaFunctions.AreaFunction Pick(string role, int salt)
    {
        var fs = Functions();
        var ok = new List<AreaFunctions.AreaFunction>();
        foreach (var f in fs)
        {
            if (StationFor(f) == null) continue;
            if (string.IsNullOrEmpty(role) || f.roles.Length == 0 || System.Array.IndexOf(f.roles, role) >= 0 || System.Array.IndexOf(f.roles, "todos") >= 0) ok.Add(f);
        }
        if (ok.Count == 0) foreach (var f in fs) if (StationFor(f) != null) ok.Add(f);
        return ok.Count > 0 ? ok[Mathf.Abs(salt) % ok.Count] : null;
    }
}
