using System.Collections.Generic;
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
        M("Alejandría", "AtomicPlant_Area", "Eratóstenes mide la Tierra (primer tramo de la escalera de la física)", "Observatorio y biblioteca (astronomía y geografía)"),
        M("Bruce Lee", "MartialArts_Area", "del Wing Chun al Jeet Kune Do: el Dojo"),
        M("Irena Sendler", "Infirmary_Area", "la trabajadora social que entraba al gueto con el pase de control de epidemias"),
        M("Le Chambon-sur-Lignon", "Kitchen_Area", "la hospitalidad del plateau: dar de comer y esconder"),
        M("Planta Atómica", "AtomicPlant_Area", "Newton, Curie y Einstein: la planta atómica"),
        M("Egina", "Construction_Area", "los mirmidones levantan la ciudad de Egina"),
        M("Crotona", "Gym_Area", "Pitágoras y la palestra de Milón", "Aula de matemáticas y música (el monocordio)"),
        M("Menfis", "Garden_Area", "Heródoto y la crecida del Nilo: los campos", "Archivo de historia y geografía (biblioteca)"),
        M("Bagdad", "Construction_Area", "al-Juarismi: medir tierras y repartir herencias", "Casa de la Sabiduría (biblioteca y sala de cálculo)"),
        M("Woolsthorpe", "AtomicPlant_Area", "Newton, la gravedad y la luz: la planta atómica (física)"),
        M("Berkeley", "Biology_Area", "Jenner y la primera vacuna: el laboratorio de biología"),
        M("Angostura", "Kitchen_Area", "Bolívar en el Congreso de Angostura", "Sala de asambleas (parlamento)"),
        M("Sils Maria", "YogaRoom_Area", "Nietzsche camina junto al lago: el cuerpo y la mente", "Sendero de montaña y biblioteca de filosofía"),
        M("París 1898", "Chemistry_Area", "Marie y Pierre Curie aíslan el radio: el laboratorio de química"),
        M("Colorado Springs", "Construction_Area", "Tesla y la electricidad de alta tensión", "Taller de electricidad (alta tensión)"),
        M("Berna", "AtomicPlant_Area", "Einstein y el año milagroso: la planta atómica"),
        M("Pasadena", "Chemistry_Area", "Patterson y la edad de la Tierra: el laboratorio de química"),
        M("Chernóbil", "NuclearPower_Area", "Ananenko y su escuadrón en el bloque 4: la central nuclear"),
        M("Palenque", "Garden_Area", "Pakal, el rey que renace como el maíz (la milpa)", "Templo de los antepasados (memoria, rito y duelo)"),
        // Los famosos (docs/microcosmos-famosos.md)
        M("Velliangiri", "YogaRoom_Area", "Sadhguru y el Dhyanalinga: meditar en silencio, la Sala de Yoga"),
        M("Hawthorne", "VehicleWorkshop_Area", "Musk y el Falcon 1: motores y montaje, el taller de Mecánica", "Fábrica de cohetes y control de misión"),
        M("Aldea de la montaña", "MartialArts_Area", "Goku (homenaje, ficción): entrenar en la aldea de la montaña, el Dojo"),
    };

    /// <summary>La fila del microcosmos <paramref name="display"/> (o null).</summary>
    public static Match Of(string display)
    {
        foreach (Match m in Table) if (m.display == display) return m;
        return null;
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
    /// 6. si su piso no lo representa, está MARCADO «pendiente de piso representativo» (con el piso que falta).
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
        }
        // 6 · Pendiente de piso representativo, marcado.
        if (!f.representative && string.IsNullOrEmpty(f.pendingFloor)) bad.Add("su piso no lo representa y no dice qué piso falta (pendiente de piso representativo)");
        if (log)
        {
            string pend = f.representative ? "" : $"; PENDIENTE de piso representativo: «{f.pendingFloor}» (hoy sobre {f.platform})";
            if (bad.Count == 0) Debug.Log($"[Pisos] {label}: OK (sobre {f.platform}, forma finalizada, {f.furnitureTops} muebles poblados{pend}).");
            else Debug.LogWarning($"[Pisos] {label}: {bad.Count} reglas de los microcosmos sin cumplir — " + string.Join(" · ", bad));
        }
        return bad;
    }
}
