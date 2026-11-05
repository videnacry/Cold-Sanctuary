using UnityEngine;

/// <summary>
/// EL CATÁLOGO DE ESTILOS, AMPLIADO A LOS EDIFICIOS DEL PUEBLO Y A LOS FRUTALES (docs/microcosmos-reglas-edificacion.md §8–11;
/// Berón 2026-11-03: «entran actividades del santuario como centros de estudio, centros de entrenamiento, de baile, plazas,
/// estaciones de transporte, parques, museos, centros de investigación, guarderías, estatuas, calles, edificios de varios pisos,
/// centros de policía, bomberos, mercados, mecánicas, talleres, milicia, piscinas, hoteles, carpintería, piletas, construcción,
/// centros de tecnología … con su estilo de la época-lugar»; «las residencias del microcosmos deben tener plantas frutales»).
/// - <see cref="CivicName"/>: el NOMBRE DE ÉPOCA Y LUGAR de cada tipo de edificio (la palestra de Crotona, el hammam de Bagdad,
///   el parque de bomberos de Berna, el temazcal de Palenque…), o null si en esa época y lugar no existía (no hay comisaría en
///   la Crotona de −530 ni estación de tren en Woolsthorpe en 1666): ese edificio no se levanta.
/// - <see cref="Fruits"/>: los FRUTALES de cada estilo (los que de verdad se cultivaban allí y entonces), para las casas.
/// Como el resto del catálogo, épocas y lugares parecidos reutilizan la misma entrada (por su <see cref="Entry.id"/>).
/// </summary>
public static partial class MicroStyles
{
    /// <summary>Los tipos de edificio del pueblo que pidió Berón (además de casas, cocinas, talleres de la historia…).</summary>
    public enum Civic
    {
        Estudio, Entrenamiento, Baile, Plaza, Estacion, Parque, Museo, Investigacion, Guarderia, Monumento, Policia, Bomberos,
        Mercado, Mecanica, Taller, Milicia, Piscina, Hotel, Carpinteria, Pileta, Obra, Tecnologia, Edificio,
        /// (2026-11-04) Donde se cura (recuperación): asclepeion, bimaristán, puesto de socorro, hospital…
        Sanacion,
    }

    static bool Is(Entry e, params string[] ids) { foreach (string i in ids) if (e.id == i) return true; return false; }
    static bool Europe(Entry e) => Is(e, "europa_medieval", "alpino", "estuardo", "georgiano", "europa_romantica", "belle_epoque", "entreguerras", "francia_rural");
    static bool Modern(Entry e) => Is(e, "norteamerica_moderna", "sovietico", "occidental_moderna", "hongkong_tonglau", "norteamerica_interior", "india_moderna", "sahel_moderno", "caribe_hispano", "jamaica_moderna", "corea_moderna", "britanico_moderno", "norteamerica_norte", "sudafrica_moderna");
    static bool UsaModern(Entry e) => Is(e, "norteamerica_victoriana", "norteamerica_moderna", "norteamerica_interior", "norteamerica_norte");

    /// <summary>El nombre del edificio <paramref name="c"/> en el estilo <paramref name="e"/> y el año <paramref name="year"/>; null
    /// si en esa época y lugar no existía.</summary>
    public static string CivicName(Civic c, Entry e, int year)
    {
        bool pre = Is(e, "prehistoria");
        switch (c)
        {
            case Civic.Plaza:
                return Is(e, "egeo") ? "Ágora" : Is(e, "roma") ? "Foro" : Is(e, "egipto") ? "Plaza del templo" : Is(e, "islam_abasi") ? "Maydān (plaza de la mezquita)" :
                       Is(e, "otomano") ? "Meydan (plaza de la fuente)" : Is(e, "hispanoamerica") ? "Plaza Mayor" : Is(e, "maya_clasico") ? "Gran Plaza" :
                       Is(e, "china_tradicional", "hongkong_tonglau") ? "Plaza del templo" : Is(e, "sovietico") ? "Plaza central" : Is(e, "frente_1914") ? null :
                       UsaModern(e) ? "Plaza (town square)" : Is(e, "sahel_moderno") ? "Place (plaza del barrio)" : Is(e, "india_moderna") ? "Plaza del templo del pueblo" : pre ? "Claro de reunión" : "Plaza mayor";
            case Civic.Monumento:
                return Is(e, "egeo") ? "Estatua votiva (kouros)" : Is(e, "egipto") ? "Coloso del faraón" : Is(e, "roma") ? "Estatua del foro" : Is(e, "islam_abasi", "otomano") ? null :
                       Is(e, "maya_clasico") ? "Estela del gobernante" : Is(e, "mesopotamia") ? "Estela de ofrenda" : Is(e, "sur_asia") ? "Piedra votiva" :
                       Is(e, "china_tradicional", "hongkong_tonglau") ? "León guardián (shishi)" : Is(e, "frente_1914") || pre ? null : Is(e, "hispanoamerica") ? "Cruz de la plaza" : "Monumento";
            case Civic.Pileta:
                return Is(e, "egeo") ? "Fuente (krene)" : Is(e, "roma") ? "Fuente pública" : Is(e, "islam_abasi") ? "Sabil (fuente pública)" : Is(e, "otomano") ? "Çeşme (fuente)" :
                       Is(e, "egipto", "mesopotamia", "sur_asia") ? "Pozo y abrevadero" : Is(e, "maya_clasico") ? "Pila del acueducto" : Is(e, "hispanoamerica") ? "Pila de la plaza" :
                       Is(e, "hongkong_tonglau") ? "Grifo público (standpipe)" : Is(e, "frente_1914") ? null : pre ? "Fuente del arroyo" : "Fuente y lavadero";
            case Civic.Parque:
                return Is(e, "egeo") ? "Arboleda sagrada (alsos)" : Is(e, "roma") ? "Jardín (hortus)" : Is(e, "egipto") ? "Jardín del templo" : Is(e, "islam_abasi") ? "Bustān (jardín)" :
                       Is(e, "otomano") ? "Bahçe (jardín de té)" : Is(e, "sur_asia") ? "Arboleda (ārāma)" : Is(e, "mesopotamia") ? "Huerto de palmeras" : Is(e, "maya_clasico") ? "Arboleda de cacao y ceibas" :
                       Is(e, "hispanoamerica") ? "Alameda" : Is(e, "china_tradicional", "hongkong_tonglau") ? "Jardín" : Is(e, "sovietico") ? "Parque de la cultura" : Is(e, "frente_1914") ? "Tierra de nadie: el encuentro de Navidad" :
                       UsaModern(e) ? "Parque" : Is(e, "sahel_moderno") ? "Parque urbano (bosque de la ciudad)" : pre ? null : "Jardín público";
            case Civic.Mercado:
                return Is(e, "egeo") ? "Mercado del ágora" : Is(e, "islam_abasi", "otomano") ? "Zoco" : Is(e, "maya_clasico") ? "Mercado de la plaza" : Is(e, "frente_1914") || pre ? null : "Mercado";
            case Civic.Taller:
                return Is(e, "maya_clasico") ? "Taller de jade y obsidiana" : Is(e, "frente_1914") ? null : pre ? "Taller de piedra tallada" : "Taller de artesanos";
            case Civic.Carpinteria:
                return Is(e, "frente_1914") || pre ? null : Is(e, "maya_clasico") ? "Taller de madera (cedro y chicozapote)" : "Carpintería";
            case Civic.Obra:
                return Is(e, "frente_1914") || pre ? null : Is(e, "maya_clasico") ? "Obra: templo en construcción (cal y piedra)" : Modern(e) ? "Obra (bloque nuevo)" : "Obra: casa en construcción";
            case Civic.Estudio:
                if (year < -3200 || pre) return null;   // sin escritura no hay escuela de escribas
                return Is(e, "egeo") ? "Escuela (didaskaleion)" : Is(e, "egipto") ? "Casa de la Vida (escribas)" : Is(e, "islam_abasi") ? "Kuttāb (escuela)" : Is(e, "otomano") ? "Mektep (escuela)" :
                       Is(e, "maya_clasico") ? "Casa de los escribas (aj tz'ib)" : Is(e, "sur_asia") ? "Escuela del maestro (gurukula)" : Is(e, "china_tradicional") ? "Escuela (sishu)" :
                       Is(e, "frente_1914") ? null : "Escuela";
            case Civic.Entrenamiento:
                // (2026-11-04) EL LUGAR DE ENTRENAR DE CADA ÉPOCA Y LUGAR (docs/microcosmos-entrenamiento-eras.md, con fuentes): su clase
                // estable es TrainingKind (la leen los planes de entrenamiento de los insectos-humanos).
                switch (TrainingKind(e, year))
                {
                    case "juegos_heroicos": return "Dromos de los juegos (carrera, lucha y pugilato)";
                    case "gimnasio_griego": return "Gimnasio (palestra y xisto)";
                    case "palestra_romana": return "Palestra de las termas";
                    case "patio_lucha_egipcio": return "Patio de lucha (como en Beni Hasan)";
                    case "patio_lucha": return "Patio de lucha (lucha de cinturón)";
                    case "akhara": return "Akhāṛā (patio de lucha)";
                    case "furusiyya": return "Maydān de furusiyya (caballos, arco y lucha)";
                    case "tekke_guresh": return "Tekke de luchadores (yağlı güreş) y campo de tiro";
                    case "juego_pelota": return "Juego de pelota";
                    case "wuguan": return "Escuela de artes marciales (wuguan)";
                    case "dojo": return "Dojo";
                    case "sala_armas": return "Sala de armas";
                    case "sala_esgrima": return "Sala de esgrima";
                    case "prado_juegos": return "Prado de juegos (lucha, garrote y carreras)";
                    case "garrote_coleo": return "Patio del juego de garrote y manga de coleo";
                    case "turnplatz_schwingen": return "Turnplatz y ruedo de Schwingen";
                    case "turnhalle": return Is(e, "norteamerica_victoriana") ? "Gimnasio de la YMCA" : "Gimnasio (sala de gimnasia)";
                    case "hebert": return "Terreno de deporte (método natural de Hébert)";
                    case "futbol_tregua": return "Partido de la tregua (fútbol en la tierra de nadie)";
                    case "club_deportivo": return "Club deportivo (normas GTO)";
                    case "gimnasio_moderno": return "Gimnasio";
                    case "estadio": return "Estadio y terreno de deporte";
                }
                return null;
            case Civic.Sanacion:
                // (2026-11-04) Donde se cura (categoría recuperación): plantilla «Hospital».
                return Is(e, "egeo") ? "Asclepeion (santuario de curación)" : Is(e, "roma") ? "Valetudinarium" : Is(e, "egipto") ? "Casa de la Vida (médicos del templo)" :
                       Is(e, "mesopotamia") ? "Casa del asû (el sanador)" : Is(e, "islam_abasi") ? "Bimaristán" : Is(e, "otomano") ? "Darüşşifa" : Is(e, "sur_asia") ? "Casa del vaidya (āyurveda)" :
                       Is(e, "maya_clasico") ? "Casa del curandero (aj men)" : Is(e, "frente_1914") ? "Puesto de socorro del regimiento" : pre ? null :
                       Is(e, "china_tradicional") ? "Botica de hierbas" : year >= 1850 ? "Hospital" : Is(e, "hispanoamerica") ? "Hospital de caridad" : "Casa del cirujano-barbero";
            case Civic.Baile:
                return Is(e, "egeo") ? "Orchestra (lugar de la danza)" : Is(e, "egipto") ? "Patio de danzas del templo" : Is(e, "sur_asia") ? "Pabellón de danza" : Is(e, "islam_abasi", "otomano") ? "Salón de música (majlis)" :
                       Is(e, "hispanoamerica") ? "Sala de joropo" : Is(e, "maya_clasico") ? "Plataforma de danza" : Is(e, "hongkong_tonglau", "china_tradicional") ? "Salón de baile" :
                       Is(e, "sovietico") ? "Palacio de Cultura (sala de baile)" : Is(e, "frente_1914") ? null : pre || Is(e, "mesopotamia") ? "Plaza de danza" : "Salón de baile";
            case Civic.Estacion:
                if (year >= 1830 && !Is(e, "frente_1914", "maya_clasico", "otomano", "hispanoamerica")) return Is(e, "hongkong_tonglau") ? "Terminal del ferry y estación de Kowloon" : "Estación de ferrocarril";
                return Is(e, "egeo", "roma", "egipto") ? "Puerto (embarcadero)" : Is(e, "islam_abasi", "otomano") ? "Caravasar (posta de caravanas)" : Is(e, "sur_asia", "mesopotamia") ? "Embarcadero del río" :
                       Is(e, "hispanoamerica") ? "Puerto fluvial (bongos y curiaras)" : Is(e, "china_tradicional") ? "Embarcadero" : Europe(e) && year >= 1600 ? "Posta de diligencias" :
                       Is(e, "europa_medieval") ? "Posta de caballos" : null;
            case Civic.Museo:
                if (Is(e, "egeo", "egipto")) return "Tesoro del templo (ofrendas)";
                if (Is(e, "otomano") && year >= 1846) return "Colección de antigüedades (Santa Irene)";
                return year >= 1683 && !Is(e, "frente_1914", "hispanoamerica") ? (Is(e, "sovietico") ? "Museo de historia local" : "Museo") : null;
            case Civic.Investigacion:
                return Is(e, "islam_abasi") ? "Observatorio (Shammasiyya)" : Is(e, "egeo") ? "Observatorio (gnomon y esfera)" : Is(e, "egipto") ? "Casa de la Vida (astronomía del templo)" :
                       Is(e, "maya_clasico") ? "Observatorio (la torre del Palacio)" : Is(e, "china_tradicional") ? "Observatorio" : Is(e, "sovietico") ? "Instituto de investigación" :
                       year >= 1600 && !Is(e, "frente_1914", "hispanoamerica", "otomano") ? (year >= 1900 ? "Laboratorio de investigación" : "Gabinete científico") : null;
            case Civic.Guarderia:
                if (Is(e, "frente_1914")) return null;
                return year >= 1840 ? (Is(e, "sovietico") ? "Guardería (detski sad)" : "Guardería (crèche)") : "Casa de cuidado de los niños";
            case Civic.Policia:
                return Is(e, "roma") ? "Vigiles" : Is(e, "egipto") ? "Puesto de los medjay" : Is(e, "egeo") ? "Guardia del ágora" : Is(e, "islam_abasi") ? "Shurta (guardia del zoco)" :
                       Is(e, "otomano") ? "Karakol de zaptiye" : Is(e, "china_tradicional") ? "Yamen (oficina de policía)" : Is(e, "sovietico") ? "Militsiya" :
                       Is(e, "hispanoamerica") ? "Cabildo y guardia" : Is(e, "estuardo", "georgiano") ? "Casa de la ronda (alguaciles)" : Europe(e) && year < 1500 ? "Casa del alguacil" :
                       year >= 1829 && !Is(e, "frente_1914", "maya_clasico") ? "Comisaría de policía" : null;
            case Civic.Bomberos:
                return Is(e, "roma") ? "Cuartel de los vigiles (incendios)" : Is(e, "otomano") ? "Tulumbacılar (bomberos de la bomba)" : Is(e, "georgiano") ? "Bomba de incendios de la parroquia" :
                       Is(e, "alpino") ? "Bomba de incendios del pueblo" : Is(e, "francia_rural", "sahel_moderno") ? "Sapeurs-pompiers" : UsaModern(e) ? "Firehouse (parque de bomberos)" :
                       year >= 1800 && !Is(e, "frente_1914", "hispanoamerica", "maya_clasico") ? "Parque de bomberos" : null;
            case Civic.Mecanica:
                if (year < -3500 || pre || Is(e, "maya_clasico", "frente_1914")) return null;   // sin rueda no hay carretero (los mayas no usaban ruedas de carga)
                return year >= 1900 ? "Taller mecánico (garaje)" : year >= 1850 ? "Taller mecánico" : "Taller del carretero (ruedas y ejes)";
            case Civic.Milicia:
                return Is(e, "egeo") ? "Cuartel de los hoplitas" : Is(e, "roma") ? "Castra" : Is(e, "maya_clasico") ? "Casa de los guerreros (bate')" : Is(e, "islam_abasi") ? "Cuartel del jund" :
                       UsaModern(e) ? "Armería de la Guardia Nacional" : Is(e, "frente_1914") ? null : pre || Is(e, "mesopotamia") ? "Casa de los guerreros" : "Cuartel de la milicia";
            case Civic.Piscina:
                return Is(e, "egeo") ? "Baños (balaneion)" : Is(e, "roma") ? "Termas" : Is(e, "islam_abasi") ? "Hammam" : Is(e, "otomano") ? "Hamam" : Is(e, "egipto") ? "Lago sagrado del templo" :
                       Is(e, "sur_asia") ? "Estanque de baños (puṣkariṇī)" : Is(e, "maya_clasico") ? "Temazcal (baño de vapor)" : Is(e, "alpino") ? "Baños del lago" :
                       Is(e, "sovietico") ? "Piscina «Lazurny»" : Is(e, "china_tradicional") ? "Baños públicos" : year >= 1850 && !Is(e, "frente_1914", "hispanoamerica") ? "Piscina y baños públicos" : null;
            case Civic.Hotel:
                return Is(e, "egeo") ? "Katagogion (posada)" : Is(e, "roma") ? "Mansio" : Is(e, "islam_abasi") ? "Funduq (posada de mercaderes)" : Is(e, "otomano") ? "Han" :
                       Is(e, "sur_asia") ? "Dharmaśālā (casa de peregrinos)" : Is(e, "china_tradicional") ? "Posada" : Is(e, "frente_1914", "maya_clasico", "egipto", "mesopotamia") || pre ? null :
                       year >= 1820 ? "Hotel" : "Posada";
            case Civic.Tecnologia:
                if (Is(e, "islam_abasi")) return "Taller de astrolabios e instrumentos";
                if (Is(e, "egeo")) return "Taller de mecanismos (engranajes y relojes de agua)";
                if (year >= 1950) return Is(e, "sovietico") ? "Centro de cálculo (ordenador de la central)" : "Centro de cálculo (ordenadores)";
                return year >= 1880 && !Is(e, "frente_1914") ? "Central telefónica" : null;
            case Civic.Edificio:
                return Is(e, "hongkong_tonglau") ? "Tong lau (varias plantas)" : Is(e, "belle_epoque", "entreguerras", "europa_romantica") ? "Inmueble de varias plantas" :
                       Is(e, "sovietico") ? "Bloque de viviendas (khrushchyovka)" : Is(e, "roma") ? "Insula" : Is(e, "otomano") ? "Konak de dos plantas" : Is(e, "maya_clasico") ? "Palacio en terrazas" :
                       Is(e, "islam_abasi") ? "Casa de dos plantas con terraza" : UsaModern(e) ? "Edificio de apartamentos" :
                       Is(e, "alpino", "estuardo", "georgiano") ? "Casa de dos plantas" : null;
        }
        return null;
    }

    /// <summary>
    /// (2026-11-04) LA CLASE ESTABLE DEL LUGAR DE ENTRENAR de la época y el lugar <paramref name="e"/> en <paramref name="year"/>
    /// (null si no había ninguno: la prehistoria). Es la etiqueta que leen los planes de entrenamiento de los insectos-humanos
    /// (AreaFunctionSite.training; docs/gimnasio-maquinas.md y docs/microcosmos-entrenamiento-eras.md, con sus fuentes):
    /// juegos_heroicos, gimnasio_griego, palestra_romana, patio_lucha_egipcio, patio_lucha, akhara, furusiyya, tekke_guresh,
    /// juego_pelota, wuguan, dojo, sala_armas, sala_esgrima, prado_juegos, garrote_coleo, turnplatz_schwingen, turnhalle, hebert,
    /// futbol_tregua, club_deportivo, gimnasio_moderno, estadio.
    /// </summary>
    public static string TrainingKind(Entry e, int year)
    {
        switch (e.id)
        {
            case "prehistoria": return null;
            case "egeo": return year < -700 ? "juegos_heroicos" : "gimnasio_griego";   // los juegos de la Ilíada (XXIII) antes del gimnasio (s. VI a. C.)
            case "roma": return "palestra_romana";
            case "egipto": return "patio_lucha_egipcio";
            case "mesopotamia": return "patio_lucha";
            case "sur_asia": case "india_moderna": return "akhara";
            case "islam_abasi": return "furusiyya";   // la zurkhaneh como institución es posterior (safávida); en la Bagdad abasí, la furusiyya y la lucha
            case "otomano": return "tekke_guresh";
            case "maya_clasico": return "juego_pelota";
            case "china_tradicional": case "hongkong_tonglau": return "wuguan";
            case "japon_edo_meiji": return "dojo";
            case "europa_medieval": return "sala_armas";
            case "estuardo": case "georgiano": return "prado_juegos";   // en el campo inglés: la lucha, el garrote y las carreras de los Cotswold Olimpicks (1612–)
            case "hispanoamerica": return "garrote_coleo";
            case "alpino": return "turnplatz_schwingen";
            case "europa_romantica": case "belle_epoque": case "entreguerras": case "norteamerica_victoriana": return "turnhalle";
            case "francia_rural": return year >= 1910 ? "hebert" : "turnhalle";
            case "frente_1914": return "futbol_tregua";
            case "sovietico": return "club_deportivo";
            case "sahel_moderno": case "caribe_hispano": case "jamaica_moderna": case "sudafrica_moderna": return "estadio";
            default: return "gimnasio_moderno";
        }
    }

    /// <summary>Un FRUTAL: su nombre (común y científico), su forma y sus colores (hoja, fruto).</summary>
    public struct Fruit
    {
        public string name; public bool palm, bush; public Color leaf, fruit; public float height;
        public Fruit(string n, float h, Color leaf, Color fruit, bool palm = false, bool bush = false) { name = n; height = h; this.leaf = leaf; this.fruit = fruit; this.palm = palm; this.bush = bush; }
    }

    static Color F(float r, float g, float b) => new Color(r, g, b);

    /// <summary>LOS FRUTALES de cada estilo de época y lugar: los que se cultivaban allí y entonces (docs/microcosmos-reglas-edificacion.md
    /// §11, con fuentes). Cada casa planta uno (o más) de su lista.</summary>
    public static Fruit[] Fruits(Entry e)
    {
        Color leaf = F(0.25f, 0.45f, 0.22f), dark = F(0.18f, 0.36f, 0.18f), grey = F(0.45f, 0.52f, 0.4f);
        switch (e.id)
        {
            case "prehistoria": return new[] { new Fruit("Higuera silvestre (Ficus carica)", 4f, leaf, F(0.45f, 0.3f, 0.4f)), new Fruit("Almendro silvestre (Prunus amygdalus)", 4f, grey, F(0.7f, 0.75f, 0.5f)) };   // Gilgal I (higos, ~9400 a. C.)
            case "mesopotamia": return new[] { new Fruit("Palmera datilera (Phoenix dactylifera)", 9f, grey, F(0.6f, 0.35f, 0.15f), palm: true), new Fruit("Granado (Punica granatum)", 3.5f, leaf, F(0.75f, 0.15f, 0.15f)) };
            case "egipto": return new[] { new Fruit("Palmera datilera (Phoenix dactylifera)", 9f, grey, F(0.6f, 0.35f, 0.15f), palm: true), new Fruit("Sicómoro (Ficus sycomorus)", 7f, leaf, F(0.8f, 0.55f, 0.35f)), new Fruit("Granado (Punica granatum)", 3.5f, leaf, F(0.75f, 0.15f, 0.15f)) };
            case "egeo": case "roma": return new[] { new Fruit("Olivo (Olea europaea)", 5f, grey, F(0.25f, 0.3f, 0.15f)), new Fruit("Higuera (Ficus carica)", 4f, leaf, F(0.45f, 0.3f, 0.4f)), new Fruit("Granado (Punica granatum)", 3.5f, leaf, F(0.75f, 0.15f, 0.15f)) };
            case "sur_asia": return new[] { new Fruit("Mango (Mangifera indica)", 8f, dark, F(0.95f, 0.65f, 0.15f)), new Fruit("Banano (Musa)", 4f, F(0.3f, 0.55f, 0.25f), F(0.9f, 0.85f, 0.3f), palm: true), new Fruit("Yambolán (Syzygium cumini)", 7f, dark, F(0.35f, 0.15f, 0.4f)) };
            case "islam_abasi": case "otomano": return new[] { new Fruit("Granado (Punica granatum)", 3.5f, leaf, F(0.75f, 0.15f, 0.15f)), new Fruit("Palmera datilera (Phoenix dactylifera)", 9f, grey, F(0.6f, 0.35f, 0.15f), palm: true), new Fruit("Higuera (Ficus carica)", 4f, leaf, F(0.45f, 0.3f, 0.4f)), new Fruit("Membrillo (Cydonia oblonga)", 4f, leaf, F(0.9f, 0.8f, 0.3f)) };
            case "china_tradicional": return new[] { new Fruit("Lichi (Litchi chinensis)", 6f, dark, F(0.8f, 0.2f, 0.25f)), new Fruit("Níspero japonés (Eriobotrya japonica)", 4f, dark, F(0.95f, 0.65f, 0.2f)), new Fruit("Longán (Dimocarpus longan)", 6f, dark, F(0.7f, 0.6f, 0.4f)) };
            case "hongkong_tonglau": return new[] { new Fruit("Papayo en maceta (Carica papaya)", 3f, F(0.3f, 0.55f, 0.25f), F(0.95f, 0.6f, 0.15f), palm: true), new Fruit("Níspero japonés (Eriobotrya japonica)", 4f, dark, F(0.95f, 0.65f, 0.2f)), new Fruit("Banano (Musa)", 3.5f, F(0.3f, 0.55f, 0.25f), F(0.9f, 0.85f, 0.3f), palm: true) };
            case "japon_edo_meiji": return new[] { new Fruit("Caqui (Diospyros kaki)", 5f, leaf, F(0.95f, 0.5f, 0.1f)), new Fruit("Ume (Prunus mume)", 4f, leaf, F(0.8f, 0.8f, 0.3f)) };
            case "europa_medieval": case "estuardo": case "georgiano": return new[] { new Fruit("Manzano (Malus domestica)", 4.5f, leaf, F(0.75f, 0.15f, 0.12f)), new Fruit("Peral (Pyrus communis)", 5f, leaf, F(0.8f, 0.75f, 0.3f)), new Fruit("Ciruelo (Prunus domestica)", 4f, leaf, F(0.4f, 0.2f, 0.45f)) };   // el manzano de Woolsthorpe
            case "alpino": return new[] { new Fruit("Grosellero (Ribes rubrum)", 1.2f, leaf, F(0.85f, 0.1f, 0.15f), bush: true), new Fruit("Serbal de los cazadores (Sorbus aucuparia)", 6f, leaf, F(0.95f, 0.35f, 0.1f)) };   // a 1800 m no maduran manzanas
            case "europa_romantica": case "belle_epoque": case "entreguerras": return new[] { new Fruit("Peral en espaldera (Pyrus communis)", 3f, leaf, F(0.8f, 0.75f, 0.3f)), new Fruit("Cerezo (Prunus avium)", 5f, leaf, F(0.7f, 0.05f, 0.1f)), new Fruit("Manzano (Malus domestica)", 4.5f, leaf, F(0.75f, 0.15f, 0.12f)) };
            case "francia_rural": return new[] { new Fruit("Manzano (Malus domestica)", 4.5f, leaf, F(0.75f, 0.15f, 0.12f)), new Fruit("Ciruelo (Prunus domestica)", 4f, leaf, F(0.4f, 0.2f, 0.45f)), new Fruit("Castaño (Castanea sativa)", 9f, dark, F(0.5f, 0.35f, 0.2f)) };   // la castaña del plateau
            case "frente_1914": return new[] { new Fruit("Manzano de la granja en ruinas (Malus domestica)", 4f, leaf, F(0.75f, 0.15f, 0.12f)) };
            case "hispanoamerica": return new[] { new Fruit("Mango (Mangifera indica)", 8f, dark, F(0.95f, 0.65f, 0.15f)), new Fruit("Guayabo (Psidium guajava)", 4f, leaf, F(0.85f, 0.85f, 0.4f)), new Fruit("Lechosa (Carica papaya)", 4f, F(0.3f, 0.55f, 0.25f), F(0.95f, 0.6f, 0.15f), palm: true) };
            case "maya_clasico": return new[] { new Fruit("Cacao (Theobroma cacao)", 4f, dark, F(0.85f, 0.5f, 0.15f)), new Fruit("Aguacate (Persea americana)", 8f, dark, F(0.2f, 0.35f, 0.15f)), new Fruit("Ramón (Brosimum alicastrum)", 9f, leaf, F(0.85f, 0.6f, 0.2f)), new Fruit("Zapote (Manilkara zapota)", 8f, dark, F(0.55f, 0.4f, 0.25f)), new Fruit("Papaya (Carica papaya)", 4f, F(0.3f, 0.55f, 0.25f), F(0.95f, 0.6f, 0.15f), palm: true) };
            // (2026-11-04) Los famosos (docs/microcosmos-famosos.md).
            case "india_moderna": return new[] { new Fruit("Mango (Mangifera indica)", 8f, dark, F(0.95f, 0.65f, 0.15f)), new Fruit("Cocotero (Cocos nucifera)", 12f, F(0.3f, 0.5f, 0.25f), F(0.55f, 0.4f, 0.2f), palm: true), new Fruit("Banano (Musa)", 4f, F(0.3f, 0.55f, 0.25f), F(0.9f, 0.85f, 0.3f), palm: true), new Fruit("Yaca (Artocarpus heterophyllus)", 9f, dark, F(0.6f, 0.65f, 0.25f)) };   // los huertos del pie de Velliangiri
            case "sahel_moderno": return new[] { new Fruit("Mango (Mangifera indica)", 8f, dark, F(0.95f, 0.65f, 0.15f)), new Fruit("Papayo (Carica papaya)", 4f, F(0.3f, 0.55f, 0.25f), F(0.95f, 0.6f, 0.15f), palm: true), new Fruit("Guayabo (Psidium guajava)", 4f, leaf, F(0.85f, 0.85f, 0.4f)), new Fruit("Karité (Vitellaria paradoxa)", 9f, dark, F(0.5f, 0.55f, 0.3f)) };   // el mango de las cours y el karité de la sabana
            case "norteamerica_interior": return new[] { new Fruit("Manzano (Malus domestica)", 4.5f, leaf, F(0.75f, 0.15f, 0.12f)), new Fruit("Melocotonero (Prunus persica)", 3.5f, leaf, F(0.95f, 0.6f, 0.4f)), new Fruit("Albaricoquero (Prunus armeniaca)", 4f, leaf, F(0.95f, 0.65f, 0.3f)), new Fruit("Pacano (Carya illinoinensis)", 12f, leaf, F(0.5f, 0.38f, 0.22f)) };   // los albaricoques de Albuquerque, los manzanos de Omaha, los pacanos de Texas
            // (2026-11-05) La música (docs/microcosmos-musica.md).
            case "caribe_hispano": return new[] { new Fruit("Mango (Mangifera indica)", 8f, dark, F(0.95f, 0.65f, 0.15f)), new Fruit("Guanábana (Annona muricata)", 5f, dark, F(0.45f, 0.6f, 0.3f)), new Fruit("Aguacate (Persea americana)", 8f, dark, F(0.2f, 0.35f, 0.15f)), new Fruit("Quenepa (Melicoccus bijugatus)", 9f, leaf, F(0.5f, 0.6f, 0.25f)) };   // el mamón de Barranquilla es la quenepa de Puerto Rico
            case "jamaica_moderna": return new[] { new Fruit("Ackee (Blighia sapida)", 9f, dark, F(0.85f, 0.25f, 0.15f)), new Fruit("Árbol del pan (Artocarpus altilis)", 10f, dark, F(0.55f, 0.65f, 0.25f)), new Fruit("Mango (Mangifera indica)", 8f, dark, F(0.95f, 0.65f, 0.15f)) };   // el mango de Second Street bajo el que ensayaban los Wailers
            case "corea_moderna": return new[] { new Fruit("Caqui (Diospyros kaki)", 5f, leaf, F(0.95f, 0.5f, 0.1f)), new Fruit("Peral de Corea (Pyrus pyrifolia)", 5f, leaf, F(0.8f, 0.7f, 0.35f)), new Fruit("Azufaifo (Ziziphus jujuba)", 5f, leaf, F(0.6f, 0.2f, 0.12f)) };
            case "britanico_moderno": return new[] { new Fruit("Manzano (Malus domestica)", 4.5f, leaf, F(0.75f, 0.15f, 0.12f)), new Fruit("Ciruelo (Prunus domestica)", 4f, leaf, F(0.4f, 0.2f, 0.45f)), new Fruit("Peral (Pyrus communis)", 5f, leaf, F(0.8f, 0.75f, 0.3f)) };
            case "norteamerica_norte": return new[] { new Fruit("Manzano (Malus domestica)", 4.5f, leaf, F(0.75f, 0.15f, 0.12f)), new Fruit("Cerezo ácido (Prunus cerasus)", 4f, leaf, F(0.7f, 0.05f, 0.1f)), new Fruit("Ciruelo (Prunus domestica)", 4f, leaf, F(0.4f, 0.2f, 0.45f)) };   // la McIntosh de Quebec, los manzanos de los patios de Gary y de Long Island
            case "sudafrica_moderna": return new[] { new Fruit("Melocotonero (Prunus persica)", 3.5f, leaf, F(0.95f, 0.6f, 0.4f)), new Fruit("Limonero (Citrus limon)", 3.5f, dark, F(0.95f, 0.9f, 0.25f)) };
            case "norteamerica_victoriana": return new[] { new Fruit("Manzano (Malus domestica)", 4.5f, leaf, F(0.75f, 0.15f, 0.12f)), new Fruit("Melocotonero (Prunus persica)", 3.5f, leaf, F(0.95f, 0.6f, 0.4f)) };
            case "norteamerica_moderna": return new[] { new Fruit("Naranjo (Citrus sinensis)", 4f, dark, F(0.95f, 0.55f, 0.1f)), new Fruit("Limonero (Citrus limon)", 3.5f, dark, F(0.95f, 0.9f, 0.25f)), new Fruit("Aguacate (Persea americana)", 7f, dark, F(0.2f, 0.35f, 0.15f)) };   // Pasadena: los naranjos del Valle de San Gabriel
            case "sovietico": return new[] { new Fruit("Manzano (Malus domestica)", 4.5f, leaf, F(0.75f, 0.15f, 0.12f)), new Fruit("Cerezo ácido (Prunus cerasus)", 4f, leaf, F(0.7f, 0.05f, 0.1f)) };   // los frutales de los patios de Prípiat
            default: return new[] { new Fruit("Limonero (Citrus limon)", 3.5f, dark, F(0.95f, 0.9f, 0.25f)), new Fruit("Manzano (Malus domestica)", 4.5f, leaf, F(0.75f, 0.15f, 0.12f)) };
        }
    }
}
