using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DÓNDE VA CADA MISIÓN EN CADA MICROCOSMOS (datos; la biblioteca es <see cref="MissionLibrary"/>). Una «Binding» dice qué plantilla y cómo
/// encontrar su puesto en la escena, de lo más concreto a lo más general: el nombre del mueble (el rótulo con que lo construye el
/// StorySceneBuilder), la clave de su puesto (<see cref="AreaFunctionStation.station"/>), el tipo de edificio del pueblo
/// (<see cref="AreaFunctionSite.building"/>) o la clase de pieza (<see cref="MicroPiece.kind"/>, p. ej. «frutal»). Si la escena aún no tiene
/// ese mueble, <see cref="StationMissions"/> pone un marcador «(supuesto)» junto a <see cref="Binding.anchor"/> y lo dice en el registro, para
/// que el constructor de la escena lo añada de verdad.
/// </summary>
public static class MissionSites
{
    public class Binding
    {
        public string template;
        public string[] names = new string[0];
        public string[] stationKeys = new string[0];
        public string[] buildings = new string[0];
        public string[] pieceKinds = new string[0];
        /// <summary>El marcador si no hay mueble: su rótulo, junto a qué objeto y a qué distancia.</summary>
        public string fallback = "";
        public string[] anchor = new string[0];
        public Vector3 offset;
    }

    static Dictionary<string, List<Binding>> _scenes;

    public static List<Binding> ForScene(string scene)
    {
        if (_scenes == null) Build();
        return _scenes.TryGetValue(scene ?? "", out List<Binding> l) ? l : new List<Binding>();
    }

    public static IEnumerable<string> Scenes { get { if (_scenes == null) Build(); return _scenes.Keys; } }

    static Binding N(string template, params string[] names) => new Binding { template = template, names = names };

    static void Build()
    {
        _scenes = new Dictionary<string, List<Binding>>();

        // ══ ELON MUSK · Hawthorne, 2008: la nave de SpaceX (StorySceneBuilder.Musk, .MuskDetail, .MuskDetail3) y el pueblo (CivicBuilder.Famosos)
        string[] kitchen = { "Cocina de la nave (supuesto)" };
        string[] catre = { "Catre de los turnos de noche (supuesto)" };
        var aidKit = new Binding { names = new[] { "Botiquín de primeros auxilios", "Botiquín" }, stationKeys = new[] { "camilla" }, buildings = new[] { "Hospital", "Clínica", "Enfermería" }, fallback = "Botiquín de primeros auxilios de la nave (supuesto)", anchor = new[] { "Lavamanos", "Aseo" }, offset = new Vector3(-1.6f, 0f, 0f) };
        var garden = new Binding { pieceKinds = new[] { "frutal" }, stationKeys = new[] { "bancal" }, buildings = new[] { "Huerto", "Parque" }, fallback = "Limonero del patio (supuesto)", anchor = new[] { "Casa de Hawthorne", "Aparcamiento de SpaceX" }, offset = new Vector3(3f, 0f, 3f) };
        var gym = new Binding { names = new[] { "Rincón de gimnasio" }, buildings = new[] { "Centro de entrenamiento", "Gimnasio" }, stationKeys = new[] { "muneco" }, fallback = "Barra con discos y cinta del turno de noche (supuesto)", anchor = catre, offset = new Vector3(0f, 0f, 3.2f) };
        var mat = new Binding { names = new[] { "Esterilla de yoga" }, stationKeys = new[] { "esterilla", "tatami" }, buildings = new[] { "Sala de Yoga" }, fallback = "Esterilla de yoga junto a los catres (supuesto)", anchor = catre, offset = new Vector3(3f, 0f, 1.6f) };
        var dog = new Binding { names = new[] { "Cochinilla hormiguera", "cochinilla hormiguera", "Perro" }, fallback = "Cochinilla hormiguera del barrio (el perro del pueblo, supuesto)", anchor = new[] { "Casa de Hawthorne", "Aparcamiento de SpaceX" }, offset = new Vector3(-2.5f, 0f, 2f) };
        Binding Like(Binding b, string template) => new Binding { template = template, names = b.names, stationKeys = b.stationKeys, buildings = b.buildings, pieceKinds = b.pieceKinds, fallback = b.fallback, anchor = b.anchor, offset = b.offset };

        _scenes["Microcosmos_ElonMusk"] = new List<Binding>
        {
            // La nave: comer, dormir, asearse, curar.
            new Binding { template = "cocinar-huevos-revueltos", names = kitchen, stationKeys = new[] { "cocina", "fogon" } },
            new Binding { template = "cocinar-tortilla-papa", names = kitchen, stationKeys = new[] { "cocina", "fogon" } },
            new Binding { template = "cocinar-chapati", names = kitchen, stationKeys = new[] { "cocina", "fogon" } },
            N("dormir-catre", catre),
            N("asearse-manos", "Lavamanos"),
            Like(aidKit, "atender-herida"), Like(aidKit, "atender-golpe"), Like(aidKit, "atender-estres-agudo"), Like(aidKit, "medico-tension"),
            // Los puestos de trabajo de la nave.
            N("mecanizar-torno", "Torno de control numérico", "Banco de mecanizado: válvulas e inyectores del Merlin"),
            N("avionica-caja", "Banco de aviónica"),
            N("composite-cofia", "Banco de composite (la cofia)"),
            N("reparar-brida-merlin", "Banco del Merlin 1C"),
            N("idea-reutilizar", "Cubículo de Elon Musk (con los pósteres de Marte)"),
            N("idea-secuencia", "Pizarra: la secuencia del vuelo 4"),
            // Entrenar y yoga (la nave no tenía gimnasio: lo del turno de noche, supuesto, hasta que el pueblo tenga el suyo).
            Like(gym, "ejercicio-intervalos-4x4"), Like(gym, "ejercicio-fuerza-4x8"), Like(gym, "ejercicio-bici-30"),
            Like(mat, "yoga-upa-cuello"), Like(mat, "yoga-saludo-al-sol"),
            // El pueblo: la taquería, el taller de coches, los frutales y el perro.
            new Binding { template = "cocinar-tacos-carne-asada", names = new[] { "Plancha de los tacos" }, buildings = new[] { "Restaurante" }, stationKeys = new[] { "fogon" } },
            new Binding { template = "reparar-coche-aceite", names = new[] { "Banco del mecánico" }, buildings = new[] { "Taller" }, stationKeys = new[] { "banco_taller" } },
            Like(garden, "campo-regar"), Like(garden, "campo-curar"), Like(garden, "campo-cosechar"),
            Like(dog, "perro-alimentar"), Like(dog, "perro-jugar"), Like(dog, "perro-programar-bordillo"),
            // (2026-11-18) Lote 2 de Hawthorne (docs/microcosmos-musk-historia.md §4, nombres exactos): trabajadores, científicos, bacterias, comida y
            // seguridad urbana.
            N("fsw-tanque-falcon9", "Soldadura por fricción (FSW): tanque del Falcon 9"),
            N("ensayo-traccion", "Laboratorio de materiales"),
            N("microscopio-soldadura", "Laboratorio de materiales"),
            N("datos-prueba-merlin", "Sala de datos de pruebas (propulsión)"),
            N("sala-limpia-vestirse", "Vestuario de la sala limpia", "Sala limpia de aviónica"),
            N("gel-manos", "Dispensador de gel hidroalcohólico"),
            N("camara-frigorifica", "Cámara frigorífica de la cocina"),
            N("inspeccion-sanitaria", "Tarjeta de inspección sanitaria (A)", "Lavamanos de la cocina (higiene)"),
            N("recibir-proveedor", "Furgoneta del proveedor de comida (supuesto)", "Entrega de comida en el muelle"),
            N("cocinar-tacos-carne-asada", "Lonchera (camión de comida)"),
            N("garita-tarjeta", "Garita de seguridad (1 Rocket Road)"),
            N("incendios-extintor", "Bomberos", "Hidrante"),
            N("incendios-hidrante", "Hidrante"),
            N("cruzar-crenshaw", "Cruce de Crenshaw Boulevard y Rocket Road (semáforos)"),
            N("pista-hawthorne", "Aparcamiento de SpaceX"),   // el aeropuerto es fondo (no se pisa): su pista se lee desde el aparcamiento
        };
    }
}
