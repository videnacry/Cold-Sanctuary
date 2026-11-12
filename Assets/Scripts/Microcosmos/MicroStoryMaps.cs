using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL MAPA A MANO DE CADA HISTORIA (docs/microcosmos-mapas-a-mano.md; Berón 2026-11-04: «para crear territorios más
/// accurate/representativos/reales se puede crear a mano cada mapa de cada historia del microcosmos, de modo que la flora y fauna se
/// colocan a mano y de paso se organizan los muebles y demás componentes del piso para recrear el mapa siendo que … se pueden usar como
/// edificaciones y componentes naturales»). Regla M9 del manual: TODA historia tiene su mapa hecho a mano.
/// Cada <see cref="Map"/> dice, en metros desde el origen de la «Historia» (x al este, z al norte: el mismo marco que el plano del
/// pueblo, <c>CivicBuilder.TownPlans</c>):
/// - <see cref="Map.floor"/>: cómo se ordenan los muebles y piezas del piso para recrear el lugar (quitar, apilar, mover, otro elemento
///   del catálogo <see cref="MicroFloorElements"/>), por la clave de la pieza o por su elemento;
/// - <see cref="Map.flora"/>: cada planta o rodal en su sitio (el nombre de la planta de su bioma, <c>MicroBiome</c>);
/// - <see cref="Map.fauna"/>: cada bicho o grupo en su sitio;
/// - <see cref="Map.buildings"/>: los edificios y su sitio (los levanta el plano del pueblo; aquí se listan para comprobarlo);
/// - <see cref="Map.todo"/>: lo que le falta (los mapas «esbozo»).
/// El constructor (<c>AreaMicrocosmos.Nature</c>) usa el mapa si está <see cref="Status.Hecho"/>: la flora y la fauna del mapa, CAÍDAS
/// DESDE EL AIRE sobre la superficie más alta de debajo (<see cref="MicroGround.Drop"/>). Sin mapa hecho, siembra al azar como RESPALDO
/// y lo registra («[Mapa] … SIN mapa a mano»).
/// </summary>
public static partial class MicroStoryMaps
{
    /// <summary>Hecho: mapa a mano del lugar real. Esbozo: aún sin hacer (siembra al azar de respaldo). Imaginado: escenario inventado a propósito, sin lugar
    /// real que recrear (Berón 2026-10-27: la taiga al amanecer de Ambrosio y Sakshi); su bioma se siembra al azar y no es una tarea pendiente.</summary>
    public enum Status { Hecho, Esbozo, Imaginado }

    /// <summary>Una orden sobre las piezas del piso: las que tienen la clave (o empiezan por ella) o el elemento <see cref="match"/>.</summary>
    public sealed class Piece
    {
        public string match, element, why; public MicroFloorElements.Action action; public Vector2 move;
        public Piece(string match, MicroFloorElements.Action action, string why, string element = null, Vector2 move = default) { this.match = match; this.action = action; this.why = why; this.element = element; this.move = move; }
    }

    /// <summary>Una planta (o un rodal de <see cref="count"/> en un círculo de <see cref="spread"/> m) de la planta <see cref="plant"/>
    /// de su bioma (por el principio de su nombre), en <see cref="at"/>.</summary>
    public sealed class Plant
    {
        public string plant; public Vector2 at; public int count; public float spread;
        public Plant(string plant, float x, float z, int count = 1, float spread = 0f) { this.plant = plant; at = new Vector2(x, z); this.count = Mathf.Max(1, count); this.spread = spread; }
    }

    /// <summary>Un bicho o un grupo (especie de <c>AnimalPrefabGenerator</c>), con su nombre y su largo (m), en <see cref="at"/>.</summary>
    public sealed class Animal
    {
        public string species, name; public Vector2 at; public int count; public float length, spread;
        public Animal(string species, string name, float x, float z, int count, float length, float spread = 6f) { this.species = species; this.name = name; at = new Vector2(x, z); this.count = Mathf.Max(1, count); this.length = length; this.spread = spread; }
    }

    /// <summary>Un edificio del pueblo (el tipo del plano: Plaza, Mercado, «Casa del Congreso…») y su sitio.</summary>
    public sealed class Building
    {
        public string what; public Vector2 at;
        public Building(string what, float x, float z) { this.what = what; at = new Vector2(x, z); }
    }

    public sealed class Map
    {
        public string display, place, source; public Status status;
        public Piece[] floor = new Piece[0]; public Plant[] flora = new Plant[0]; public Animal[] fauna = new Animal[0]; public Building[] buildings = new Building[0];
        public string[] todo = new string[0];
        public int PlantCount { get { int n = 0; foreach (Plant p in flora) n += p.count; return n; } }
        public int AnimalCount { get { int n = 0; foreach (Animal a in fauna) n += a.count; return n; } }
    }

    static Map Stub(string display, params string[] todo) => new Map { display = display, status = Status.Esbozo, todo = todo };
    static Map Imagined(string display, string what) => new Map { display = display, status = Status.Imaginado, place = what, source = "escenario imaginado (Berón, 2026-10-27), sin mapa real" };
    const string Base1 = "colocar a mano la flora y la fauna del lugar y la época", Base2 = "ordenar los muebles del piso para el mapa real (qué pieza es qué)";

    /// <summary>LOS MAPAS. Angostura es el ejemplo completo; el resto, esbozos con lo que les falta.</summary>
    public static readonly Map[] All =
    {
        // ══ ANGOSTURA, 15 de febrero de 1819 (Bolívar): sobre la Sala de asambleas ══════════════════════════
        // El pueblo: la cuadrícula colonial en la orilla sur del Orinoco (su «angostura», ~800 m de río), la Plaza Mayor con la catedral
        // y la Casa del Congreso, el Paseo Orinoco con su barranca de granito, la Piedra del Medio en el río y el cerro El Zamuro con su
        // fortín al sur. Febrero: estación seca; sabana de paja de Trachypogon con chaparros, morichales en los caños, araguaneyes en
        // flor (amarillos) y lajas de granito del escudo guayanés. (Plano de Angostura, 1819, Archivo General de Indias; Lynch 2006;
        // Humboldt, «Viaje», cap. XXIV.)
        new Map
        {
            display = "Angostura", place = "Angostura del Orinoco (hoy Ciudad Bolívar), febrero de 1819", status = Status.Hecho,
            source = "Plano de Angostura (1819, AGI); Lynch 2006; Humboldt, Viaje a las regiones equinocciales, cap. XXIV",
            floor = new[]
            {
                new Piece("alfombra", MicroFloorElements.Action.Apilar, "las alfombras de la cocina, apiladas: el cerro El Zamuro y su fortín, al sur del pueblo"),
                new Piece("hornilla", MicroFloorElements.Action.Como, "en Guayana no hay volcanes: los fogones son TEPUYES (mesetas de arenisca del escudo)", "mesa"),
                new Piece("silla", MicroFloorElements.Action.Como, "ni nieve en el Orinoco: las sillas son cerros de granito", "mesa"),
                new Piece("lavabo", MicroFloorElements.Action.Como, "los fregaderos son el cauce del Orinoco y sus lagunas de desborde (grifo abierto: el río)", "lavabo"),
                new Piece("cajonera", MicroFloorElements.Action.Como, "armarios y neveras: las casas de dos plantas de la ciudad colonial (estilo de 1819)", "cajonera"),
                // (2026-11-05) En la Sala de asambleas: sin pantallas ni máquinas en 1819; lo de antes (fogones, fregaderos) era de cuando iba sobre la Cocina.
                new Piece("maquina", MicroFloorElements.Action.Como, "sin máquinas en 1819: lajas de granito", "roca"),
                new Piece("pared", MicroFloorElements.Action.Como, "el marcador: el farallón de la barranca del Orinoco", "pared"),
            },
            flora = new[]
            {
                // Los araguaneyes en flor de la Plaza Mayor (en sus cuatro esquinas, fuera de la plaza) y del Paseo Orinoco.
                new Plant("Araguaney", -16f, -57f), new Plant("Araguaney", 16f, -57f), new Plant("Araguaney", -16f, -83f), new Plant("Araguaney", 16f, -83f),
                new Plant("Araguaney", -600f, 107f), new Plant("Araguaney", -480f, 107f), new Plant("Araguaney", -360f, 107f), new Plant("Araguaney", -240f, 107f),
                new Plant("Araguaney", 240f, 107f), new Plant("Araguaney", 360f, 107f), new Plant("Araguaney", 480f, 107f), new Plant("Araguaney", 600f, 107f),
                // Los morichales de los caños, al este y al oeste del pueblo, junto a la barranca.
                new Plant("Moriche", -620f, 160f, 9, 40f), new Plant("Moriche", 610f, 180f, 7, 35f),
                // La sabana de paja al sur y en las faldas del Zamuro, con sus chaparros.
                new Plant("Paja de sabana", -450f, -350f, 40, 150f), new Plant("Paja de sabana", 450f, -300f, 30, 120f), new Plant("Paja de sabana", 260f, -500f, 30, 140f),
                new Plant("Chaparro", 260f, -500f, 14, 120f), new Plant("Chaparro", -400f, -420f, 10, 80f),
                // Las lajas de granito de la barranca, con sus bromelias, y las playas del Orinoco en la seca.
                new Plant("Laja de granito", -300f, 225f, 5, 25f), new Plant("Laja de granito", 0f, 228f, 5, 25f), new Plant("Laja de granito", 300f, 226f, 5, 25f),
                new Plant("Bromelia", -80f, 230f, 6, 30f), new Plant("Bromelia", 330f, 230f, 4, 20f),
                new Plant("Arena del Orinoco", -450f, 250f, 4, 30f), new Plant("Arena del Orinoco", 420f, 250f, 4, 30f),
            },
            fauna = new[]
            {
                new Animal("Ant", "Bachaco (Atta sexdens), hormiga cortadora de hojas", -380f, -400f, 6, 1.6f, 15f),
                new Animal("Termite", "Comején (Nasutitermes), en los chaparros del Zamuro", 250f, -470f, 4, 1.2f, 20f),
                new Animal("Grasshopper", "Saltamontes (Tropidacris collaris), en la paja", -450f, -350f, 4, 2.4f, 40f),
                new Animal("Mantis", "Mantis (Stagmatoptera), en la sabana", 450f, -300f, 2, 2.4f, 20f),
                new Animal("Firefly", "Cocuyo (Pyrophorus), de noche por el Paseo", 0f, 110f, 4, 1.2f, 60f),
                new Animal("Spider", "Araña de seda dorada (Trichonephila clavipes), en el morichal", -620f, 160f, 2, 2f, 20f),
            },
            buildings = new[]
            {
                new Building("Plaza", 0f, -70f), new Building("Casa del Congreso de Angostura", 50f, 50f), new Building("Catedral de Angostura", -125f, 40f),
                new Building("Puerto del Orinoco", -60f, 210f), new Building("Mercado", 60f, -55f), new Building("Fortín de El Zamuro", 260f, -500f),
            },
            todo = new[] { "en vivo: mirar que cada rodal cae en suelo libre (no en una calle ni un solar) y los araguaneyes de la plaza fuera de su paso de 2 m" },
        },

        // ══ ESBOZOS: lo que le falta a cada historia ═══════════════════════════════════════════════════════════════════════
        Imagined("Sakshi (el río)", "la taiga al amanecer: el río del origen"),
        Imagined("Ambrosio (nivel 1)", "la taiga al amanecer: la cueva, el camino y el bosque"),
        Stub("Hospital", Base1, Base2, "el hospital como paisaje: qué camas son colinas, qué máquinas son centrales; la tribu de antes del fuego"),
        Stub("Cocina", Base1, Base2, "la era del fuego: la guarida y los restos del hogar entre las encimeras"),
        Stub("Huerto", Base1, Base2, "la primera aldea (−9500): los campos de hongos y las casas redondas sobre los bancales"),
        Stub("Sala de Yoga", Base1, Base2), Stub("Estudio Textil", Base1, Base2), Stub("Mecánica", Base1, Base2),
        Stub("Mesopotamia", Base1, Base2, "Uruk: el Éufrates, los canales y los campos de cebada; el templo blanco"),
        Stub("Patanjali", Base1, Base2, "el āśram en el bosque: el río, el pipal, las chozas"),
        Stub("Tregua 1914", Base1, "la tierra de nadie (alfombras quitadas), las trincheras de cada lado y los abetos de Navidad"),
        Stub("Escútari", Base1, Base2, "el hospital de los barracones de Selimiye junto al Bósforo"),
        MapKapilavastu(),   // (2026-11-12) hecho: MicroStoryMaps.Tanda4.cs
        Stub("Dayton", Base1, Base2, "el taller de bicicletas de los Wright en West Third Street y el río Miami"),
        Stub("Alejandría", Base1, Base2, "el puerto, el Faro, la Biblioteca y el pozo de Siena para la sombra"),
        Stub("Bruce Lee", Base1, Base2, "Hong Kong de los 50: Nathan Road, los tong lau y la escuela de Yip Man"),
        Stub("Irena Sendler", Base1, Base2, "el gueto de Varsovia: su muro, sus puertas y la ciudad de fuera"),
        Stub("Le Chambon-sur-Lignon", Base1, Base2, "el plateau del Vivarais-Lignon: pinares, granjas y la estación"),
        Stub("Planta Atómica", Base1, Base2), Stub("Egina", Base1, Base2, "la isla de Egina, su puerto y el templo de Afaya"),
        Stub("Crotona", Base1, Base2, "la ciudad griega de Calabria, la escuela pitagórica y la palestra de Milón"),
        Stub("Menfis", Base1, Base2, "el Nilo en crecida, los campos inundados y los templos de Ptah"),
        Stub("Bagdad", Base1, Base2, "la ciudad redonda de al-Mansur, el Tigris y la Casa de la Sabiduría"),
        MapWoolsthorpe(),   // (2026-11-12) hecho: MicroStoryMaps.Tanda4.cs
        MapBerkeley(),   // (2026-11-12) hecho: MicroStoryMaps.Tanda4.cs
        Stub("Sils Maria", Base1, "el lago de Silvaplana, la roca de Surlej y los senderos de la Engadina (la mesa como nevado)"),
        MapParis1898(),   // (2026-11-12) hecho: MicroStoryMaps.Cientificos.cs
        MapColoradoSprings(),   // (2026-11-12) hecho: MicroStoryMaps.Cientificos.cs
        MapBerna(),   // (2026-11-12) hecho: MicroStoryMaps.Cientificos.cs
        MapPasadena(),   // (2026-11-12) hecho: mapa a mano
        Stub("Chernóbil", Base1, Base2, "Prípiat, la central, el estanque de refrigeración (el lavabo como mar) y el bosque rojo"),
        Stub("Palenque", Base1, Base2, "el Palacio, el Templo de las Inscripciones, el río Otolum y la milpa"),
        // (2026-11-05) Los siete famosos que salieron del mesocosmos (docs/personajes-mesocosmos.md).
        Stub("Velliangiri", Base1, Base2, "los montes Velliangiri, Coimbatore, el Dhyanalinga y su bosque"),
        Stub("Hawthorne", Base1, Base2, "la fábrica de SpaceX en Hawthorne, el control de misión y el Falcon 1"),
        Stub("Aldea de la montaña", Base1, Base2, "la aldea de la montaña (homenaje, ficción): la cabaña, el bosque y el campo de entrenar"),
        Stub("Albuquerque", Base1, Base2, "Albuquerque en 1975: el garaje de MITS, el Altair y el desierto de Nuevo México"),
        Stub("Omaha", Base1, Base2, "Omaha: la tienda de la señora B, sus alfombras y las calles de Nebraska"),
        Stub("Prairie View", Base1, Base2, "Prairie View A&M: el campus, su biblioteca y la pradera de Texas"),
        Stub("Uagadugú", Base1, Base2, "los huertos antes de las lluvias en Burkina Faso: el karité, el mijo y el agua"),
    };

    /// <summary>El mapa de la historia <paramref name="display"/> (o null si ni siquiera tiene esbozo).</summary>
    public static Map For(string display)
    {
        foreach (Map m in All) if (m.display == display) return m;
        foreach (Map m in Music) if (m.display == display) return m;   // la música (MicroStoryMaps.Musica.cs)
        foreach (Map m in Historias3) if (m.display == display) return m;   // la tanda 3 (MicroStoryMaps.Historias.cs)
        return null;
    }

    /// <summary>¿Tiene <paramref name="display"/> su mapa HECHO a mano?</summary>
    public static bool Authored(string display) { Map m = For(display); return m != null && m.status == Status.Hecho; }

    /// <summary>La orden del mapa sobre una pieza del piso (por su clave o su elemento), o null.</summary>
    public static Piece FloorOp(string display, string key, string element)
    {
        Map m = For(display);
        if (m == null) return null;
        foreach (Piece p in m.floor) if (key == p.match || key.StartsWith(p.match) || element == p.match) return p;
        return null;
    }

    /// <summary>Las posiciones de un rodal (determinista: en espiral dentro de su círculo), en metros desde la «Historia».</summary>
    public static List<Vector2> Spots(Vector2 at, int count, float spread)
    {
        var l = new List<Vector2>();
        if (count <= 1 || spread <= 0f) { for (int i = 0; i < count; i++) l.Add(at); return l; }
        const float golden = 2.39996323f;
        for (int i = 0; i < count; i++) { float r = spread * Mathf.Sqrt((i + 0.5f) / count), a = i * golden; l.Add(at + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r); }
        return l;
    }
}
