using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// LOS ELEMENTOS DE CADA PISO, CON SU VERSIÓN DE MESOCOSMOS Y SU VERSIÓN DE MICROCOSMOS (docs/microcosmos-suelo-y-elementos.md
/// §Elementos; Berón 2026-11-04: «los elementos que componen cada piso deben estar listados y creados con su versión mesocosmos y su
/// versión microcosmos, de ese modo se puede tener una versión más plana y superficial para el mesocosmos y el del microcosmos en
/// cambio sería creado a lujo de detalle aprovechando su estructura para edificación o componente natural del territorio … un
/// lavamanos podría ser un lago … una cajonera podría ser un edificio, una caja podría ser una cueva o una fábrica, un zapato también
/// podría ser una cueva o un mercado, las hornillas podrían ser volcanes, el foco sería el sol, las lámparas podrían ser postes de luz
/// … las mesas y sillas podrían ser montañas o nevados … se podrían retirar alfombras o usar para crear desniveles y se podrían apilar
/// alfombras para crear cúmulos»).
/// - <see cref="Catalog"/>: cada ELEMENTO (por las palabras de su nombre), su versión de MESOCOSMOS (plana y sencilla: lo que es en el
///   santuario) y su versión de MICROCOSMOS (antes y después de las ciudades: la caja es cueva o fábrica).
/// - <see cref="Inventory"/>: las PIEZAS de un piso (cada mueble entero, cada losa) con su clave y su elemento. Lo mismo en el piso del
///   santuario y en su copia gigante: así el paso meso ↔ micro es uno a uno y un cambio del piso se puede rastrear.
/// - <see cref="Signature"/> y <see cref="Missing"/>: la firma de un inventario y lo que le falta a la forma de microcosmos (la prueba
///   de sincronía, también la del hospital).
/// - <see cref="Overrides"/>: lo que una historia cambia de su piso (quitar, apilar, mover, otro elemento): cuenta como la versión de
///   microcosmos, no como romper el piso.
/// - <see cref="DependentsOf"/> y el registro de dependencias (<see cref="Record"/>, <see cref="Stale"/>): qué microcosmos dependen de
///   cada piso y cuáles revisar cuando el piso cambie.
/// </summary>
public static class MicroFloorElements
{
    public enum Kind { Terreno, Agua, Edificio, Luz, Monumento, Llanura }

    public sealed class Element
    {
        public string id, meso, micro, microTown, why;
        public string[] keys;
        public Kind kind;
        /// <summary>La versión de microcosmos para una era (antes o después de las ciudades: <see cref="MicroBuildRules.IsTownEra"/>).</summary>
        public string MicroFor(int year) => MicroBuildRules.IsTownEra(year) && !string.IsNullOrEmpty(microTown) ? microTown : micro;
    }

    static Element E(string id, Kind k, string meso, string micro, string microTown, string why, params string[] keys)
        => new Element { id = id, kind = k, meso = meso, micro = micro, microTown = microTown, why = why, keys = keys };

    /// <summary>
    /// EL CATÁLOGO, de lo más concreto a lo más general (manda la primera fila cuyo nombre encaja: «Lampara_de_mesa» es una lámpara, no
    /// una mesa). Las filas de Berón primero; el resto, decididas aquí (docs/microcosmos-suelo-y-elementos.md §Tabla).
    /// </summary>
    public static readonly Element[] Catalog =
    {
        // ── Las de Berón ──
        E("bombilla", Kind.Luz, "bombilla o foco del techo", "el SOL (una esfera que brilla y da luz)", null, "Berón: «el foco sería el sol»", "bombilla", "foco", "bulb", "plafon", "luz_techo", "luminaria"),
        E("lampara", Kind.Luz, "lámpara (de pie, de mesa, flexo)", "POSTE DE LUZ gigante con su farol, y faroles a escala del pueblo alrededor", null, "Berón: «las lámparas podrían ser postes de luz»", "lampara", "farol", "flexo", "lamp", "aplique", "lampara_quirurgica"),
        E("hornilla", Kind.Terreno, "fogón u hornillas", "VOLCANES (uno por fuego, con su cráter de lava y su fumarola)", null, "Berón: «las hornillas podrían ser volcanes»", "hornilla", "quemador", "fogon", "cocina", "stove", "burner", "vitro", "brasero", "placa", "parrilla", "horno"),
        E("lavabo", Kind.Agua, "lavabo o fregadero", "LAGO en su cuenca (con grifo abierto: cascada, fuente y río)", null, "Berón: «un lavamanos podría ser un lago, y si está abierto sería una pileta o un río»", "lavabo", "lavamanos", "fregadero", "pila", "sink", "lavadero", "pileta"),
        E("cajonera", Kind.Edificio, "cajonera, cómoda, armario o taquilla", "EDIFICIO de la era (cada cajón, una planta: ventanas, puerta, azotea)", null, "Berón: «una cajonera podría ser un edificio … se tendrán que crear edificios para eras más actuales, entonces podrían usar las cajoneras y armarios»", "cajonera", "comoda", "armario", "ropero", "wardrobe", "taquilla", "locker", "archivador", "estanteria", "estante", "vitrina", "nevera", "frigorifico", "refrigerador", "aparador", "botiquin", "mueble"),
        E("caja", Kind.Terreno, "caja, baúl o cajón", "CUEVA (boca oscura y derrubios)", "FÁBRICA (chimeneas, tejado en diente de sierra, portón)", "Berón: «una caja podría ser una cueva o una fábrica»", "caja", "box", "baul", "cofre", "contenedor", "cajon"),
        E("zapato", Kind.Terreno, "zapato o bota", "CUEVA (la boca del zapato)", "MERCADO (puestos con toldo alrededor)", "Berón: «un zapato también podría ser una cueva o un mercado»", "zapato", "zapatilla", "bota", "shoe", "sandalia"),
        E("silla", Kind.Terreno, "silla, taburete, banco o sofá", "NEVADO (cumbre con nieve en el respaldo, laderas de roca en las patas)", null, "Berón: «las mesas y sillas podrían ser montañas o nevados»", "silla", "chair", "taburete", "stool", "banqueta", "sofa", "butaca", "sillon", "banco"),
        E("mesa", Kind.Terreno, "mesa, escritorio, mostrador o encimera", "MONTAÑA de meseta (las patas, contrafuertes de roca; el tablero, altiplano poblado)", null, "Berón: «las mesas y sillas podrían ser montañas»", "mesa", "table", "escritorio", "desk", "mostrador", "encimera", "barra", "pupitre", "tarima", "plataforma"),
        E("alfombra", Kind.Terreno, "alfombra voladora, esterilla o tatami", "DESNIVEL (una terraza con sus rampas); apiladas, un CÚMULO; o se retira", null, "Berón: «se podrían retirar alfombras o usar para crear desniveles y se podrían apilar alfombras para crear cúmulos»", "alfombra", "tapete", "esterilla", "tatami", "carpet", "rug", "colchoneta"),
        E("estatua", Kind.Monumento, "estatua (figura humana)", "ESTATUA DE INSECTO-HUMANO en la misma pose y papel", null, "Berón: «todas las estatuas humanas deben tener su versión para el microcosmos donde modelan figuras de insectos-humanos»", "estatua", "statue", "busto", "escultura"),
        // ── Decididas aquí ──
        E("cama", Kind.Terreno, "cama, camilla o colchón", "COLINAS de pradera (lomas suaves sobre el colchón)", null, "el colchón blando y ondulado: lomas", "cama", "camilla", "bed", "litera", "colchon", "cuna", "incubadora", "hamaca"),
        E("cojin", Kind.Terreno, "cojín, almohada o puf", "DUNAS", null, "lo blando y redondeado: dunas de arena", "cojin", "almohada", "puf", "pillow", "cushion"),
        E("banera", Kind.Agua, "bañera, ducha, pecera o piscina", "MAR INTERIOR (agua honda y su playa)", null, "un recipiente de agua grande: un mar", "banera", "ducha", "pecera", "acuario", "piscina", "tanque_agua", "tina", "estanque"),
        E("olla", Kind.Agua, "olla, sartén, cuenco o taza", "LAGO DE CRÁTER (borde de roca y agua dentro)", null, "un recipiente hondo y redondo: un cráter", "olla", "sarten", "cazo", "cuenco", "bol", "taza", "vaso", "plato", "pot", "bowl"),
        E("botella", Kind.Edificio, "botella, jarra, bombona o tanque", "TORRE o FARO (ventanas en banda y luz arriba)", null, "lo alto y estrecho: una torre", "botella", "jarra", "jarron", "bombona", "oxigeno", "termo", "bottle", "frasco", "probeta", "matraz", "silo", "deposito"),
        E("maquina", Kind.Edificio, "máquina, monitor, ordenador o aparato", "FÁBRICA o CENTRAL (tuberías, chimeneas, luces)", "FÁBRICA o CENTRAL (tuberías, chimeneas, luces)", "un aparato: la industria", "maquina", "monitor", "pantalla", "ordenador", "computer", "reactor", "generador", "turbina", "respirador", "ventilador", "analizador", "microscopio", "autoclave", "ecografo", "rayos", "centrifuga", "bomba", "motor", "telar", "torno", "prensa", "impresora", "consola", "altavoz", "amplificador", "piano", "tambor"),
        E("poste", Kind.Luz, "gotero, perchero, soporte o antena", "ANTENA o TORRE DE RADIO (celosía con luz roja arriba)", null, "lo fino y alto: una antena", "gotero", "suero", "perchero", "soporte", "antena", "mastil"),
        E("carro", Kind.Edificio, "carro, silla de ruedas o carrito", "ESTACIÓN (andén con marquesina)", null, "lo que rueda: una estación", "carro", "carrito", "silla_de_ruedas", "wheelchair", "trolley", "carretilla", "cochecito"),
        E("jaula", Kind.Edificio, "perrera, jaula o corral", "FORTALEZA (almenas en su borde)", null, "lo que encierra: una muralla", "perrera", "jaula", "corral", "cage", "kennel", "box_animal"),
        E("planta", Kind.Terreno, "planta en maceta, árbol o jardinera", "BOSQUE (árboles a escala del pueblo)", null, "una planta: un bosque", "planta", "maceta", "tiesto", "arbol", "arbusto", "jardinera", "bonsai", "frutal", "huerto", "bancal"),
        E("espejo", Kind.Terreno, "espejo", "GLACIAR (hielo y grietas)", null, "lo liso que refleja: hielo", "espejo", "mirror"),
        E("libro", Kind.Terreno, "libros, papeles o carpetas", "MESA DE ESTRATOS (capas de roca de colores)", null, "las hojas apiladas: estratos", "libro", "papel", "carpeta", "cuaderno", "book", "revista"),
        E("pared", Kind.Terreno, "pared, panel, pizarra, cuadro o puerta", "ACANTILADO (pared de roca facetada)", null, "lo vertical y plano: un acantilado", "pared", "muro", "panel", "pizarra", "cuadro", "puerta", "tabique", "mampara", "biombo", "cortina", "cristal", "ventana", "barandilla", "tapa_cristal"),
        E("columna", Kind.Terreno, "columna, pilar o poste de estructura", "TORRE DE ROCA (aguja de piedra)", null, "un pilar: una aguja de roca", "columna", "pilar", "viga", "poste", "pata"),
        E("escalera", Kind.Terreno, "escalera o rampa", "TERRAZAS (bancales escalonados)", null, "los peldaños: terrazas", "escalera", "rampa", "peldano", "escalon", "tarima", "escenario"),
        E("suelo", Kind.Llanura, "suelo, losa, zona o pasillo", "LLANURA (la tierra del microcosmos, con sus cúmulos y piedras)", null, "el suelo es el terreno", "suelo", "mat", "losa", "zona", "pasillo", "floor", "marca", "linea", "base", "pavimento"),
        E("roca", Kind.Terreno, "cualquier otra pieza", "AFLORAMIENTO de roca (peñascos al pie de la pieza)", null, "lo que no tiene fila: un peñasco, y se apunta para darle la suya", "__otro__"),
    };

    /// <summary>La fila de los elementos sin fila (peñascos).</summary>
    public static Element Other => Catalog[Catalog.Length - 1];

    /// <summary>El elemento por su id (o null).</summary>
    public static Element ById(string id) { foreach (Element e in Catalog) if (e.id == id) return e; return null; }

    /// <summary>El nombre normalizado: minúsculas, sin tildes, con «_» en vez de espacios y guiones.</summary>
    public static string Norm(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        var sb = new StringBuilder(name.Length);
        foreach (char ch in name.ToLowerInvariant())
        {
            switch (ch)
            {
                case 'á': case 'à': case 'ä': case 'â': sb.Append('a'); break;
                case 'é': case 'è': case 'ë': case 'ê': sb.Append('e'); break;
                case 'í': case 'ì': case 'ï': case 'î': sb.Append('i'); break;
                case 'ó': case 'ò': case 'ö': case 'ô': sb.Append('o'); break;
                case 'ú': case 'ù': case 'ü': case 'û': sb.Append('u'); break;
                case 'ñ': sb.Append('n'); break;
                case ' ': case '-': case '.': sb.Append('_'); break;
                default: sb.Append(ch); break;
            }
        }
        return sb.ToString();
    }

    /// <summary>LA CLAVE de una pieza: su nombre normalizado sin el número de copia («Cama_12», «Silla (3)» → «cama», «silla»).</summary>
    public static string Key(string name)
    {
        string n = Norm(name).Trim('_');
        int p = n.IndexOf('(');
        if (p > 0) n = n.Substring(0, p).TrimEnd('_');
        int end = n.Length;
        while (end > 0 && (char.IsDigit(n[end - 1]) || n[end - 1] == '_')) end--;
        return end > 0 ? n.Substring(0, end) : n;
    }

    /// <summary>El elemento de una pieza por su nombre (la primera fila que encaja), o null si ninguna.</summary>
    public static Element Match(string name)
    {
        string n = "_" + Norm(name) + "_";
        // 1) Las claves de varias palabras («silla_de_ruedas», «lampara_quirurgica»): mandan sobre las de una.
        foreach (Element e in Catalog)
            foreach (string k in e.keys)
                if (k.IndexOf('_') > 0 && k != "__otro__" && n.Contains("_" + k + "_")) return e;
        // 2) Las de una palabra: una palabra del nombre igual a la clave (o su plural: «sillas», «lamparas», «pilares» no es «pila»).
        string[] tokens = n.Split('_');
        foreach (Element e in Catalog)
            foreach (string k in e.keys)
            {
                if (k.IndexOf('_') >= 0) continue;
                foreach (string w in tokens) if (w.Length > 0 && (w == k || w == k + "s" || w == k + "es")) return e;
            }
        return null;
    }

    // (Por prefijo: las de yoga son varias, «Planta_hatha_2»…)
    static bool IsInnerFloor(string n) => n.StartsWith("Planta_hospitalizacion") || n.StartsWith("Planta_veterinaria") || n.StartsWith("Planta_ashtanga") || n.StartsWith("Planta_hatha") || n.StartsWith("Planta_musica");

    // Lo que no es parte del piso terminado (ni en el santuario ni en su copia gigante): la obra, las formas preparadas, los anillos
    // (son otras plataformas), la estructura elevada y la máquina de virtualización.
    static bool Skip(Transform t)
    {
        string n = t.name;
        if (FloorForms.IsObra(t) || n == FloorForms.ConstructionName || n == FloorForms.MicroName) return true;
        if (n.StartsWith("Anillo") || n == "Estructura_Elevada" || n.EndsWith("_VirtualizationMachine")) return true;
        if (IsInnerFloor(n)) return true;   // (las plantas de un área en otro piso son otras plataformas, como los anillos. «Planta» casaba con la fila de la maceta y la planta entera contaba como un mueble sin versión en la copia gigante)
        return false;
    }

    /// <summary>Una pieza del piso: su transform, su clave y su elemento (null: sin fila, va como peñasco).</summary>
    public struct Piece { public Transform t; public string key; public Element element; public string path; }

    /// <summary>
    /// EL INVENTARIO de un piso: cada pieza (un mueble entero —la raíz que lleva collider o malla, o cuyo nombre es de un elemento—,
    /// una losa) sin bajar a sus partes; los contenedores vacíos se recorren. Igual en el piso del santuario y en su copia gigante
    /// (no mira componentes de conducta, que la copia no lleva).
    /// </summary>
    public static List<Piece> Inventory(Transform area)
    {
        var l = new List<Piece>();
        if (area != null) Walk(area, area, l, "");
        return l;
    }

    static void Walk(Transform t, Transform area, List<Piece> l, string path)
    {
        foreach (Transform c in t)
        {
            if (c == null || Skip(c)) continue;
            string p = path.Length == 0 ? c.name : path + "/" + c.name;
            Element e = Match(c.name);
            bool own = c.GetComponent<Renderer>() != null || c.GetComponent<Collider>() != null;
            if ((e != null || own) && c.GetComponentInChildren<Renderer>(true) != null) { l.Add(new Piece { t = c, key = Key(c.name), element = e, path = p }); continue; }
            Walk(c, area, l, p);
        }
    }

    /// <summary>Las claves de un inventario con cuántas hay («cama×48»), ordenadas.</summary>
    public static string[] Keys(List<Piece> inv)
    {
        var d = new SortedDictionary<string, int>(System.StringComparer.Ordinal);
        foreach (Piece p in inv) { d.TryGetValue(p.key, out int n); d[p.key] = n + 1; }
        var l = new List<string>(); foreach (var kv in d) l.Add($"{kv.Key}×{kv.Value}");
        return l.ToArray();
    }

    /// <summary>LA FIRMA de un inventario (FNV-1a de sus claves): cambia si el piso gana, pierde o cambia piezas.</summary>
    public static string Signature(string[] keys)
    {
        uint h = 2166136261;
        foreach (string k in keys) { foreach (char ch in k) { h ^= ch; h *= 16777619; } h ^= '|'; h *= 16777619; }
        return h.ToString("x8");
    }

    /// <summary>
    /// LA SINCRONÍA: las claves del piso del santuario (<paramref name="meso"/>) que no tienen su versión en la forma de microcosmos
    /// (<paramref name="microForm"/>, sus <see cref="MicroElement"/>). Vacío = todo lo del piso está en el microcosmos.
    /// </summary>
    public static List<string> Missing(Transform meso, Transform microForm)
    {
        var have = new HashSet<string>();
        if (microForm != null) foreach (MicroElement me in microForm.GetComponentsInChildren<MicroElement>(true)) have.Add(me.key);
        var miss = new List<string>();
        foreach (Piece p in Inventory(meso)) if (!have.Contains(p.key) && !miss.Contains(p.key)) miss.Add(p.key);
        return miss;
    }

    /// <summary>Lo que cambió entre dos inventarios (claves con su número): lo nuevo (+) y lo que ya no está (−).</summary>
    public static List<string> Diff(string[] before, string[] now)
    {
        var a = new HashSet<string>(before ?? new string[0]); var b = new HashSet<string>(now ?? new string[0]);
        var l = new List<string>();
        foreach (string k in b) if (!a.Contains(k)) l.Add("+" + k);
        foreach (string k in a) if (!b.Contains(k)) l.Add("−" + k);
        return l;
    }

    // ── LO QUE UNA HISTORIA CAMBIA DE SU PISO (su versión de microcosmos) ─────────────────────────────────────────────────

    public enum Action { Quitar, Apilar, Mover, Como }

    public sealed class Override
    {
        public string display, piece, element, why; public Action action; public Vector2 move;
        public Override(string display, string piece, Action action, string why, string element = null, Vector2 move = default)
        { this.display = display; this.piece = piece; this.action = action; this.why = why; this.element = element; this.move = move; }
    }

    /// <summary>
    /// LOS CAMBIOS DE CADA HISTORIA sobre su piso: <see cref="Action.Quitar"/> (la pieza se apaga: sigue contada, el piso no se rompe),
    /// <see cref="Action.Apilar"/> (las alfombras de la clave hacen un cúmulo), <see cref="Action.Mover"/> (en metros del piso real) y
    /// <see cref="Action.Como"/> (otro elemento del catálogo). La pieza se busca por su clave (<see cref="Key"/>) o por el principio.
    /// </summary>
    public static readonly Override[] Overrides =
    {
        new Override("Sils Maria", "mesa", Action.Como, "Nietzsche camina junto al lago: la mesa, un nevado de la Engadina", "silla"),
        new Override("Tregua 1914", "alfombra", Action.Quitar, "la tierra de nadie: sin alfombras, la llanura embarrada de Flandes"),
        new Override("Chernóbil", "lavabo", Action.Como, "el estanque de refrigeración de la central", "banera"),
    };

    /// <summary>Los cambios de la historia <paramref name="display"/> que tocan la pieza de clave <paramref name="key"/>.</summary>
    public static Override OverrideFor(string display, string key, string element = null)
    {
        // (2026-11-04) Manda el MAPA A MANO de la historia (MicroStoryMaps); la tabla de abajo es la de las historias sin mapa.
        MicroStoryMaps.Piece mp = MicroStoryMaps.FloorOp(display, key, element);
        if (mp != null) return new Override(display, mp.match, mp.action, mp.why, mp.element, mp.move);
        foreach (Override o in Overrides) if (o.display == display && (key == o.piece || key.StartsWith(o.piece))) return o;
        return null;
    }

    // ── LAS DEPENDENCIAS: qué microcosmos se montan sobre cada piso ───────────────────────────────────────────────────────

    /// <summary>Los microcosmos que dependen del piso <paramref name="platform"/> (MicroFloors.Table).</summary>
    public static List<string> DependentsOf(string platform)
    {
        var l = new List<string>();
        foreach (MicroFloors.Match m in MicroFloors.Table) if (m.platform == platform) l.Add(m.display);
        return l;
    }

    /// <summary>El registro de dependencias: un fichero local del proyecto Unity (UserSettings; no va al repositorio) con, por escena de
    /// microcosmos, su piso, la firma y las claves del inventario con que se montó.</summary>
    public static string DepsPath => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath) ?? ".", "UserSettings", "MicroFloorDeps.txt");

    public sealed class Dep { public string scene, platform, signature; public string[] keys; }

    public static List<Dep> ReadDeps()
    {
        var l = new List<Dep>();
        try
        {
            if (!System.IO.File.Exists(DepsPath)) return l;
            foreach (string line in System.IO.File.ReadAllLines(DepsPath))
            {
                string[] p = line.Split('\t');
                if (p.Length < 4) continue;
                l.Add(new Dep { scene = p[0], platform = p[1], signature = p[2], keys = p[3].Length > 0 ? p[3].Split(',') : new string[0] });
            }
        }
        catch (System.Exception e) { Debug.LogWarning($"[Pisos] No se pudo leer {DepsPath}: {e.Message}"); }
        return l;
    }

    /// <summary>Apunta (o reemplaza) la dependencia de la escena <paramref name="scene"/>.</summary>
    public static void Record(string scene, string platform, string[] keys)
    {
        try
        {
            var l = ReadDeps(); l.RemoveAll(d => d.scene == scene);
            l.Add(new Dep { scene = scene, platform = platform, signature = Signature(keys), keys = keys });
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DepsPath));
            var lines = new List<string>(); foreach (Dep d in l) lines.Add($"{d.scene}\t{d.platform}\t{d.signature}\t{string.Join(",", d.keys)}");
            System.IO.File.WriteAllLines(DepsPath, lines);
        }
        catch (System.Exception e) { Debug.LogWarning($"[Pisos] No se pudo escribir {DepsPath}: {e.Message}"); }
    }

    /// <summary>
    /// LOS MICROCOSMOS POR REVISAR: para el piso <paramref name="area"/> del santuario (tal como está ahora), cada escena de microcosmos
    /// apuntada sobre él cuya firma ya no coincide, con lo que cambió. Vacío = todos al día.
    /// </summary>
    public static List<string> Stale(Transform area)
    {
        var l = new List<string>();
        if (area == null) return l;
        string[] now = Keys(Inventory(area)); string sig = Signature(now);
        foreach (Dep d in ReadDeps())
        {
            if (d.platform != area.name || d.signature == sig) continue;
            var diff = Diff(d.keys, now);
            l.Add($"{d.scene} (sobre {d.platform}): el piso cambió ({string.Join(", ", diff.GetRange(0, Mathf.Min(8, diff.Count)))}{(diff.Count > 8 ? " …" : "")}): reconstruir su microcosmos");
        }
        return l;
    }
}


