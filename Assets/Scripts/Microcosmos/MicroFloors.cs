using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>
/// LOS PISOS DE LOS MICROCOSMOS (docs/microcosmos-areas.md §Reglas de los microcosmos; Berón 2026-11-03: «todos deben pertenecer
/// a un piso del santuario y deben montarse encima de dicho piso, todas las estructuras de dicho piso se vuelven superficie
/// utilizable … si no existe un piso que encaje con el microcosmos que se quiere crear se puede marcar como pendiente de obtener
/// piso representativo en el mesocosmos/santuario y se le crea con alguno de los pisos existentes, entonces a futuro podríamos
/// identificar los pisos que faltan por crear»).
/// <see cref="Table"/>: cada microcosmos, el piso sobre el que se monta, si ese piso REPRESENTA su historia y, si no, el piso que
/// haría falta crear («pendiente de piso representativo»). <see cref="Pending"/> da la lista de pisos que faltan.
/// <see cref="Validate"/> comprueba en una escena las reglas de los microcosmos.
/// </summary>
public static class MicroFloors
{
    public sealed class Match
    {
        public string display, platform, why, pending;
        public bool representative => string.IsNullOrEmpty(pending);
        public Match(string display, string platform, string why, string pending = null) { this.display = display; this.platform = platform; this.why = why; this.pending = pending ?? ""; }
    }

    static Match M(string d, string p, string why, string pending = null) => new Match(d, p, why, pending);

    /// <summary>LA TABLA DE PISOS: microcosmos → piso del santuario. Con «pending», el piso no representa la historia: se monta en
    /// el que se dice mientras se crea el que falta.</summary>
    public static readonly Match[] Table =
    {
        M("Sakshi (el río)", "YogaRoom_Area", "el río del origen de Sakshi: la práctica empieza en la Sala de Yoga"),
        M("Ambrosio (nivel 1)", "Infirmary_Area", "la muerte de Ambrosio: el hospital"),
        M("Hospital", "Infirmary_Area", "la muerte de Ambrosio: el hospital"),
        M("Cocina", "Kitchen_Area", "la era del fuego: nace la cocina"),
        M("Huerto", "Garden_Area", "los primeros cultivos (La Sembradora)"),
        M("Sala de Yoga", "YogaRoom_Area", "la tribu en su área"), M("Estudio Textil", "TextileStudio_Area", "la tribu en su área"), M("Mecánica", "VehicleWorkshop_Area", "la tribu en su área"),
        M("Mesopotamia", "Kitchen_Area", "Uruk: las raciones de cebada y el pan del templo (la cocina apunta a ella)"),
        M("Patanjali", "YogaRoom_Area", "los ocho miembros del yoga: la Sala de Yoga"),
        M("Tregua 1914", "Gym_Area", "la tregua de Navidad y el partido de fútbol en tierra de nadie", "Campo de deportes de equipo (fútbol)"),
        M("Escútari", "Infirmary_Area", "Florence Nightingale: el hospital"),
        M("Kapilavastu", "Infirmary_Area", "los cuatro encuentros (enfermedad, vejez, muerte): el hospital"),
        M("Dayton", "VehicleWorkshop_Area", "los Wright, de la bicicleta al avión: el taller de mecánica"),
        // (2026-11-04) Re-apuntados al Aula por la alerta de piso nuevo (esperaban una biblioteca o un aula).
        M("Alejandría", "Classroom_Area", "Eratóstenes mide la Tierra: la biblioteca de Alejandría (el Aula y su biblioteca)"),
        M("Bruce Lee", "MartialArts_Area", "del Wing Chun al Jeet Kune Do: el Dojo"),
        M("Irena Sendler", "Infirmary_Area", "la trabajadora social que entraba al gueto con el pase de control de epidemias"),
        M("Le Chambon-sur-Lignon", "Kitchen_Area", "la hospitalidad del plateau: dar de comer y esconder"),
        M("Planta Atómica", "AtomicPlant_Area", "Newton, Curie y Einstein: la planta atómica"),
        M("Egina", "Construction_Area", "los mirmidones levantan la ciudad de Egina"),
        M("Crotona", "Classroom_Area", "Pitágoras: la escuela del número y la música (el aula)"),
        M("Menfis", "Classroom_Area", "Heródoto, el primer historiador: el archivo y la biblioteca (el Aula)"),
        M("Bagdad", "Classroom_Area", "al-Juarismi en la Casa de la Sabiduría: biblioteca y sala de cálculo (el Aula)"),
        M("Woolsthorpe", "AtomicPlant_Area", "Newton, la gravedad y la luz: la planta atómica (física)"),
        M("Berkeley", "Biology_Area", "Jenner y la primera vacuna: el laboratorio de biología"),
        M("Angostura", "Kitchen_Area", "Bolívar en el Congreso de Angostura", "Sala de asambleas (parlamento)"),
        M("Sils Maria", "Classroom_Area", "Nietzsche: la biblioteca de filosofía (el Aula)"),
        M("París 1898", "Chemistry_Area", "Marie y Pierre Curie aíslan el radio: el laboratorio de química"),
        M("Colorado Springs", "Construction_Area", "Tesla y la electricidad de alta tensión", "Taller de electricidad (alta tensión)"),
        M("Berna", "AtomicPlant_Area", "Einstein y el año milagroso: la planta atómica"),
        M("Pasadena", "Chemistry_Area", "Patterson y la edad de la Tierra: el laboratorio de química"),
        M("Chernóbil", "NuclearPower_Area", "Ananenko y su escuadrón en el bloque 4: la central nuclear"),
        M("Palenque", "Garden_Area", "Pakal, el rey que renace como el maíz (la milpa)", "Templo de los antepasados (memoria, rito y duelo)"),
        // Las historias de la tanda 3 (docs/microcosmos-historias-3.md)
        M("Chimbacalle", "VehicleWorkshop_Area", "Alfaro y el tren Guayaquil–Quito: locomotoras y mecánica", "Estación de ferrocarril y transporte"),
        M("Lagunillas", "Infirmary_Area", "Galvarino: curarse, volver con los suyos y seguir; la machi y el lawen, el hospital"),
        M("Cajamarca", "TextileStudio_Area", "cómo vivían los incas: el cumbi, los quipus y las colcas, el Estudio Textil", "Almacén y contaduría (qollqas y quipus)"),
        M("Onondaga", "Classroom_Area", "la Gran Ley de la Paz: el consejo de las Cinco Naciones", "Sala de asambleas (parlamento)"),
        M("Alejandría de Cleopatra", "Kitchen_Area", "Cleopatra abre los graneros en los años sin crecida: dar de comer a una ciudad, la Cocina"),
        M("Langley", "Informatics_Area", "las calculadoras humanas de Langley y el IBM 7090: el piso de Informática"),
        M("Viena 1824", "Music_Area", "Beethoven y el estreno de la Novena: el concierto, el piso de Música"),
        M("Leiden", "Classroom_Area", "Descartes, el método y la geometría: el Aula"),
        M("San Pablo", "Infirmary_Area", "Guevara y Granado en la colonia de leprosos: el hospital"),
        M("Madrid 2018", "Classroom_Area", "la moción de censura de 2018: cómo funciona un parlamento", "Sala de asambleas (parlamento)"),
        M("Gettysburg", "Classroom_Area", "Lincoln en Gettysburg: el discurso y la emancipación", "Sala de asambleas (parlamento)"),
        M("Feria de las Hazañas", "Classroom_Area", "grandes hazañas de presidentes y primeros ministros: aprender de lo que hicieron", "Sala de asambleas (parlamento)"),
        // Los famosos (docs/microcosmos-famosos.md)
        M("Velliangiri", "YogaRoom_Area", "Sadhguru y el Dhyanalinga: meditar en silencio, la Sala de Yoga"),
        M("Hawthorne", "Control_de_mision", "Musk y el Falcon 1: el control de misión de la lanzadera (re-apuntado por la alerta de piso nuevo)"),
        M("Aldea de la montaña", "MartialArts_Area", "Goku (homenaje, ficción): entrenar en la aldea de la montaña, el Dojo"),
        // (2026-11-04) Re-apuntados por la ALERTA DE PISO NUEVO (AlertFloors): Gates a Informática y Scott al Aula (docs/piso-informatica.md, docs/piso-aula.md).
        M("Albuquerque", "Informatics_Area", "Gates y Allen: el Altair y su BASIC; el piso de Informática (ordenadores y programación)"),
        M("Omaha", "TextileStudio_Area", "Buffett y la señora B: la tienda que empezó vendiendo alfombras y telas, el Estudio Textil", "Mercado y oficina de comercio (economía)"),
        M("Prairie View", "Classroom_Area", "Scott y Prairie View A&M: la universidad, su biblioteca y sus estudiantes; el Aula"),
        M("Uagadugú", "Garden_Area", "los productores y sus huertos: labrar y sembrar antes de las lluvias, el Huerto"),
        // La música (docs/microcosmos-musica.md): sobre su piso, Música (docs/piso-musica.md, 2026-11-05); BLACKPINK, sobre la Sala de Ballet.
        M("Grandes aportes de las canciones", "Music_Area", "seis canciones que cambiaron algo: el escenario del piso de Música"),
        M("Long Island", "Music_Area", "Public Enemy nace en una radio y un estudio: la sala de grabación del piso de Música"),
        M("Belfast", "Music_Area", "U2 en el Waterfront Hall: un concierto por el referéndum de paz; el escenario del piso de Música"),
        M("Kingston", "Music_Area", "el One Love Peace Concert: la música en el escenario; el escenario del piso de Música"),
        M("Seúl", "Ballet_Area", "BLACKPINK: años de ensayo ante el espejo; la Sala de Ballet es una sala de ensayo"),
        M("Gary", "Music_Area", "los Jackson: ensayar voces y pasos en casa y subir al escenario del instituto; el escenario del piso de Música"),
        M("Montreal", "Music_Area", "Lennon y Ono graban con todos en la habitación del hotel: el escenario del piso de Música"),
        M("Vega Baja", "Music_Area", "Benito graba en su cuarto: la sala de grabación del piso de Música"),
        M("Barranquilla", "Classroom_Area", "Shakira y el colegio Pies Descalzos de La Playa: la escuela; el Aula"),
    };

    /// <summary>La fila del microcosmos <paramref name="display"/> (o null).</summary>
    public static Match Of(string display)
    {
        foreach (Match m in Table) if (m.display == display) return m;
        return null;
    }

    // ══ ALERTA DE PISO NUEVO (Berón 2026-11-04; manual §11, regla «alerta de piso nuevo») ═════════════════════════════════════════
    // Cada vez que se crea un piso, se mira si encaja con algún microcosmos «pendiente de piso representativo»: así no se queda
    // ninguno montado en un piso que no lo representa cuando ya existe el suyo.

    /// <summary>Palabras que describen cada piso del santuario (además de su nombre), para buscar sus pendientes.</summary>
    public static readonly Dictionary<string, string[]> FloorKeywords = new Dictionary<string, string[]>
    {
        ["Informatics_Area"] = new[] { "informática", "ordenador", "programación", "computación", "servidor", "cálculo" },
        ["Classroom_Area"] = new[] { "aula", "biblioteca", "universidad", "universitaria", "estudio", "clase", "archivo" },
        ["Music_Area"] = new[] { "música", "grabación", "concierto", "escenario" },
    };

    /// <summary>Minúsculas y sin tildes (para comparar «Informática» con «informatica»).</summary>
    public static string Norm(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var sb = new StringBuilder();
        foreach (char c in s.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        return sb.ToString();
    }

    static readonly HashSet<string> Stop = new HashSet<string> { "planta", "sala", "piso", "area", "del", "los", "las" };

    /// <summary>Los microcosmos PENDIENTES (no representados) cuyo piso que falta encaja con un piso de nombre
    /// <paramref name="display"/> y palabras <paramref name="keywords"/> (las de <see cref="FloorKeywords"/> si es null).</summary>
    public static List<Match> PendingFor(string platform, string display, IEnumerable<string> keywords = null)
    {
        var words = new List<string>();
        if (keywords == null && platform != null && FloorKeywords.TryGetValue(platform, out string[] kw)) keywords = kw;
        if (keywords != null) foreach (string k in keywords) { string n = Norm(k); if (n.Length >= 4) words.Add(n); }
        foreach (string w in Norm(display).Split(' ', '(', ')', ',', '·')) if (w.Length >= 5 && !Stop.Contains(w)) words.Add(w);
        var hits = new List<Match>();
        foreach (Match m in Table)
        {
            if (m.representative || m.platform == platform) continue;
            string p = Norm(m.pending);
            foreach (string w in words) if (p.Contains(w)) { hits.Add(m); break; }
        }
        return hits;
    }

    /// <summary>LA ALERTA: para cada piso (<paramref name="floors"/>: GameObject del área y su nombre), dice en la consola si es un
    /// piso NUEVO (ningún microcosmos lo usa) y qué microcosmos pendientes encajan con él. Devuelve cuántos encajes encontró.</summary>
    public static int AlertFloors(IEnumerable<(string platform, string display)> floors)
    {
        int n = 0;
        foreach (var (platform, display) in floors)
        {
            bool used = false;
            foreach (Match m in Table) if (m.platform == platform) { used = true; break; }
            var hits = PendingFor(platform, display);
            n += hits.Count;
            if (hits.Count == 0 && used) continue;
            string list = hits.Count == 0 ? "ninguno encaja" : string.Join(" · ", hits.ConvertAll(h => $"{h.display} (hoy sobre {h.platform}; falta «{h.pending}»)"));
            Debug.LogWarning($"[Pisos] ALERTA de piso {(used ? "" : "NUEVO ")}«{display}» ({platform}): pendientes de piso representativo que encajan: {list}." +
                             (hits.Count > 0 ? " Re-apúntalos en MicroFloors.Table si este piso los representa (manual §11)." : ""));
        }
        return n;
    }

    /// <summary>LOS PISOS QUE FALTAN POR CREAR en el santuario: cada uno con los microcosmos que lo esperan.</summary>
    public static Dictionary<string, List<string>> Pending()
    {
        var d = new Dictionary<string, List<string>>();
        foreach (Match m in Table)
        {
            if (m.representative) continue;
            if (!d.TryGetValue(m.pending, out var l)) d[m.pending] = l = new List<string>();
            l.Add($"{m.display} (hoy sobre {m.platform})");
        }
        return d;
    }

    /// <summary>
    /// LAS REGLAS DE LOS MICROCOSMOS en la raíz <paramref name="root"/> de una escena de microcosmos:
    /// 1. pertenece a un piso del santuario (MicroFloor.platform) que está en la tabla (o lo dice: sin fila, aviso);
    /// 2. está MONTADO ENCIMA de ese piso: lleva la copia gigante del piso («…_gigante») y su historia está a ras del suelo del piso;
    /// 3. sobre su FORMA FINALIZADA: nada de obra (andamio, material, conos) y el «Acabado» encendido;
    /// 4. las ESTRUCTURAS del piso (caras de arriba de los muebles) son superficie poblada: flora y fauna encima;
    /// 5. en una era avanzada, el PUEBLO ENTERO: el barrio del pueblo (×60) y al menos 12 edificios;
    /// 6. si su piso no lo representa, está MARCADO «pendiente de piso representativo» (con el piso que falta);
    /// 7. (2026-11-04) la FLORA ENCIMA de cada superficie, nunca debajo ni dentro de un collider (<see cref="MicroGround.ValidateFlora"/>);
    /// 8. la CAPA DE TIERRA (<see cref="MicroSoil"/>) sobre los suelos del piso;
    /// 9. montado desde la FORMA DE MICROCOSMOS del piso («Forma_microcosmos», <see cref="MicroFloorElements"/>), con el inventario del
    ///    piso del mesocosmos apuntado (<see cref="MicroFloor.mesoSignature"/>).
    /// </summary>
    public static List<string> Validate(Transform root, string label = null, bool log = true)
    {
        var bad = new List<string>();
        if (root == null) { bad.Add("sin raíz"); return bad; }
        label = label ?? root.name;
        MicroFloor f = root.GetComponent<MicroFloor>();
        if (f == null) { bad.Add("sin MicroFloor (no se montó con la plantilla)"); if (log) Debug.LogWarning($"[Pisos] {label}: {bad[0]}"); return bad; }
        // 1 · Su piso.
        if (string.IsNullOrEmpty(f.platform)) bad.Add("no dice a qué piso del santuario pertenece (MicroFloor.platform)");
        // 2 · Montado ENCIMA del piso.
        Transform giant = null;
        foreach (Transform c in root) if (c.name.EndsWith("_gigante")) { giant = c; break; }
        if (giant == null) bad.Add($"no lleva su piso gigante («…_gigante» de {f.platform}): no está montado encima de un piso");
        Transform story = root.Find("Historia");
        // Encima: sobre su suelo o sobre una de sus estructuras (una entreplanta, la meseta de un mueble), nunca debajo ni sobre los muros.
        if (story != null && (story.localPosition.y < f.center.y - 2.5f || story.localPosition.y > f.center.y + Mathf.Max(5f, f.wallHeight))) bad.Add($"la historia no está encima del piso ({story.localPosition.y - f.center.y:0.0} m sobre su suelo)");
        // 3 · Forma finalizada.
        if (!f.finishedForm) bad.Add("no se montó sobre la forma finalizada del piso");
        if (giant != null) { var obra = FloorForms.ObraLeft(giant); if (obra.Count > 0) bad.Add("piezas de obra en el piso: " + string.Join(", ", obra.GetRange(0, Mathf.Min(4, obra.Count)))); }
        // 3b · El piso NO se rompe: siguen todas sus piezas (ni se quitan muebles para el pueblo ni para la historia).
        if (giant != null && f.floorPieces > 0)
        {
            int now = giant.GetComponentsInChildren<Renderer>(true).Length;
            if (now != f.floorPieces) bad.Add($"el piso se rompió: {f.floorPieces} piezas al montarlo y {now} ahora (el pueblo se adapta al piso, no al revés)");
        }
        if (f.buildingsOnTops > 0 && f.climbLinks == 0) bad.Add("hay edificios sobre los muebles del piso y ningún enlace para trepar (NavMeshLink)");
        // 4 · Las estructuras del piso, pobladas.
        if (f.furnitureTops > 0 && (f.floraOnTops == 0 || f.faunaOnTops == 0)) bad.Add($"{f.furnitureTops} muebles con cara de arriba y {f.floraOnTops} plantas / {f.faunaOnTops} bichos encima (deben poblarse)");
        // 5 · El pueblo entero.
        if (f.town)
        {
            int sites = root.GetComponentsInChildren<AreaFunctionSite>(true).Length;
            float want = 0.4f * Mathf.Min(f.size.x, f.size.y);
            if (Mathf.Min(f.districtSize.x, f.districtSize.y) < want) bad.Add($"el pueblo ocupa un barrio de {f.districtSize.x:0} × {f.districtSize.y:0} m (< 40 % del piso de {f.size.x:0} m): no es el pueblo entero");
            if (sites < 12) bad.Add($"el pueblo tiene {sites} edificios (< 12): reconstruye la ciudad entera de la historia");
            bad.AddRange(ValidateCategories(root));   // regla 16 (2026-11-04)
        }
        // 6 · Pendiente de piso representativo, marcado.
        if (!f.representative && string.IsNullOrEmpty(f.pendingFloor)) bad.Add("su piso no lo representa y no dice qué piso falta (pendiente de piso representativo)");
        // 7 · (2026-11-04) LA FLORA ENCIMA, NUNCA DEBAJO (MicroGround): ninguna planta con el pie bajo la superficie sobre la que está
        //     ni dentro de un collider.
        bad.AddRange(MicroGround.ValidateFlora(root, 4));
        // 8 · (2026-11-04) LA CAPA DE TIERRA sobre los suelos del piso (MicroSoil).
        if (MicroGround.SoilEnabled && giant != null && root.GetComponentInChildren<MicroSoil>(true) == null) bad.Add("sin capa de tierra (MicroSoil) sobre los suelos del piso");
        // 9 · (2026-11-04) LA FORMA DE MICROCOSMOS: el piso se monta desde su forma de microcosmos (MicroFloorElements) y apunta el
        //     inventario del piso del mesocosmos con que se montó (para saber qué revisar cuando el piso cambie).
        if (giant != null && root.Find(FloorForms.MicroName) == null) bad.Add($"sin «{FloorForms.MicroName}» (los elementos del piso sin su versión de microcosmos)");
        if (giant != null && string.IsNullOrEmpty(f.mesoSignature)) bad.Add("no apunta el inventario del piso del mesocosmos con que se montó (mesoSignature)");
        if (log)
        {
            string pend = f.representative ? "" : $"; PENDIENTE de piso representativo: «{f.pendingFloor}» (hoy sobre {f.platform})";
            if (bad.Count == 0) Debug.Log($"[Pisos] {label}: OK (sobre {f.platform}, forma finalizada, {f.furnitureTops} muebles poblados{pend}).");
            else Debug.LogWarning($"[Pisos] {label}: {bad.Count} reglas de los microcosmos sin cumplir — " + string.Join(" · ", bad));
        }
        return bad;
    }
    /// <summary>Prefijo de <see cref="AreaFunctionSite.mapSource"/> para una posición tomada de un mapa de hoy.</summary>
    public const string CurrentMap = "mapa actual:";
    /// <summary>Prefijo de <see cref="AreaFunctionSite.mapSource"/> para un edificio que el constructor tuvo que poner sin posición del mapa.</summary>
    public const string MissingMap = "FALTA";

    /// <summary>
    /// REGLA 16 (2026-11-04; Berón: «se deben llenar con mapas actuales para cumplir con las estructuras de ocio/entrenamiento/trabajo/
    /// recuperación, se usarían mapas actuales solo para buscar posiciones pero se investigaría la época/lugar para recrear
    /// edificaciones, muebles y maquinaria nativos»): un pueblo de historia tiene al menos un edificio de cada categoría
    /// (<see cref="AreaFunctionSite.TownCategories"/>: trabajo, ocio, entrenamiento y recuperación), y cada posición sacada de un mapa
    /// de hoy lo dice con su fuente («mapa actual: …»); ninguno sin posición («FALTA …»). Devuelve las violaciones.
    /// </summary>
    public static List<string> ValidateCategories(Transform root)
    {
        var bad = new List<string>();
        var have = new HashSet<string>();
        foreach (AreaFunctionSite s in root.GetComponentsInChildren<AreaFunctionSite>(true))
        {
            if (!string.IsNullOrEmpty(s.activity)) have.Add(s.activity);
            if (s.mapSource.StartsWith(MissingMap)) bad.Add($"«{s.title}» sin posición del mapa ({s.mapSource})");
            else if (s.mapSource.StartsWith(CurrentMap) && s.mapSource.Length < CurrentMap.Length + 8) bad.Add($"«{s.title}» sale de un mapa actual sin decir cuál");
            if (s.activity == AreaFunctionSite.Entrenamiento && string.IsNullOrEmpty(s.training)) bad.Add($"«{s.title}» de entrenamiento sin su clase (training)");
        }
        foreach (string c in AreaFunctionSite.TownCategories) if (!have.Contains(c)) bad.Add($"el pueblo no tiene ningún edificio de {c} (regla 16: si el plano histórico no lo trae, su posición sale de un mapa actual)");
        return bad;
    }

}
