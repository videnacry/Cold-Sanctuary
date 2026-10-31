using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TORRES TEMÁTICAS (Berón 2026-10-29; docs/plano-edificios.md): POCOS edificios con MUCHOS pisos (las megacolumnas ya están
/// dimensionadas para 30, <see cref="StructuralLimits"/>), cada uno con un tema:
///  · SERVICIOS (personas y animales): Restaurante · Hospital (3 plantas) · Cría · Carne Cultivada;
///  · ENTRENAMIENTO: Gimnasio · Cardio y zona libre · Calistenia · Sala de Yoga · Sala de Ballet · Dojo;
///  · CIENCIAS: Física (3 plantas: TRIGA, ciclotrón, sala de experimentos) · Química (2) · Biología (2);
///  · PRODUCCIÓN (huertos, jardines y construcción): Huerto · Rotación · Construcción · Mecánica · Estudio Textil.
/// La Central Nuclear sigue APARTE: un reactor de producción necesita su propio sitio de exclusión.
/// Todos los pisos de una torre miden lo mismo (la planta del área más grande de la torre): así las 8 megacolumnas de las
/// puntas del octógono son continuas de abajo arriba. Cada área amuebla su piso centrada en él. Un área con PLANTAS
/// PROPIAS (hospital, física, química, biología) las monta como hijas; la siguiente área va encima de la MÁS ALTA de ellas
/// (no 11 m sobre su planta baja). Los pisos de áreas distintas son HERMANOS (no hijos): el microcosmos que clona un área
/// copia solo sus plantas.
/// </summary>
public static partial class SampleSceneBuilder
{
    public const string TorreServicios = "Servicios", TorreEntrenamiento = "Entrenamiento", TorreCiencias = "Ciencias", TorreProduccion = "Producción";

    // Planta de cada torre (la de su área más grande) y su sitio, en un lóbulo de hielo (PolarLandscapeBuilder.Islands) con las
    // 8 columnas sobre hielo firme y ≥ 16 m entre la punta de un anillo y la torre vecina (reglas-plataformas, regla 4).
    public static readonly Vector2 ServiciosSize = new Vector2(40f, 38f);        // el restaurante (cocina abierta + comedor en anillo)
    public static readonly Vector2 CienciasSize = new Vector2(32f, 20f);         // biología
    public static readonly Vector2 ProduccionSize = new Vector2(26f, 20f);       // el huerto (y la rotación de encima: 24 × 18 + 2 m de suelo)
    // LAS TORRES-ÁRBOL (Berón 2026-10-31; docs/edificio-arbol.md §Migración): cada torre es un árbol en hélice (su tronco, en hielo
    // firme), y entre las piezas de dos árboles (pisos con sus anillos, puntas, ramas, troncos) hay ≥ 16 m en 3D, con el Árbol de
    // los Campos girando (se barre su círculo). En sus sitios de antes, a 70–100 m unas de otras, no cabían (cada árbol alcanza
    // 51–71 m): se movieron (búsqueda en scratchpad/mig) y la Producción sube un tramo (primer piso a 96 m, por encima de los
    // demás). Los giros (múltiplos de 90°) orientan el molinete para que no choque con el vecino.
    public static readonly Vector3 ServiciosCenter = new Vector3(50f, 0f, 92f);      // lóbulo Norte:Enfermeria (donde estuvo el hospital)
    public static readonly Vector3 EntrenamientoCenter = new Vector3(10f, 0f, 0f);   // (aún torre apilada; árbol: (-77, 52), lóbulo Norte:Bosque_Comestible)
    public static readonly Vector3 CienciasCenter = new Vector3(92f, 0f, -108f);     // (aún torre apilada; árbol: (92, -107), lóbulo Sur:Biologia)
    public static readonly Vector3 ProduccionCenter = new Vector3(18f, 0f, -95f);    // (aún torre apilada; árbol: (114, 0), lóbulo Este:Paso)
    public const float ServiciosYaw = 90f, EntrenamientoYaw = 0f, CienciasYaw = 0f, ProduccionYaw = 90f;
    public const float ProduccionFirstY = 96f;   // por encima de Servicios y Ciencias (a 112 y 108 m de ella)
    // (El Entrenamiento usa GymSize, 36 × 26.)

    // La torre que se está levantando: su sitio, su planta y su piso más alto hasta ahora (null = aún sin planta baja).
    static Vector3 _towerCenter;
    static Vector2 _towerSize;
    static Transform _towerTop;

    /// <summary>Empieza una torre: los siguientes <see cref="TowerFloor"/> van uno encima de otro en este sitio.</summary>
    static void BeginTower(Vector3 center, Vector2 size) { _towerCenter = center; _towerSize = size; _towerTop = null; _towerHelix = null; }

    // EL MODELO DEL ÁRBOL (Berón 2026-10-31; docs/edificio-arbol.md §Migración): la torre que se está levantando es un edificio en
    // hélice; sus pisos van, en orden, a los pisos pegados al tronco (nivel 1: lados a y c; nivel 2: b y d…).
    static Helix _towerHelix;

    /// <summary>Empieza una TORRE-ÁRBOL: tronco cuadrado de agua en <paramref name="center"/>, girada <paramref name="yaw"/>, con
    /// <paramref name="levels"/> niveles de hélice (2 pisos y 2 puntas cada uno) desde el primer piso del Árbol de los Campos. Los
    /// siguientes <see cref="TowerFloor"/> van a sus pisos, en orden; las puntas llevan frutales en maceta (<see cref="PlaceRooftopOrchards"/>).</summary>
    static Helix BeginTreeTower(Transform parent, string building, Vector3 center, Vector2 size, float yaw, int levels, float firstY = 0f, float tipGap = 10f)
    {
        Helix hx = BeginHelix(parent, building, TowerRoot(building), center, yaw, TowerHelix(size, levels, firstY, tipGap), rotates: false);
        hx.pickDoors = true;
        hx.tipContent = (k, tip) => { tip.name = $"Terraza_punta_{k}"; _orchardTips.Add((tip, building)); return "frutales"; };
        _towerHelix = hx; _towerCenter = center; _towerSize = size; _towerTop = null;
        return hx;
    }

    /// <summary>El nombre de la raíz de la torre-árbol de un edificio («Torre_Servicios»…).</summary>
    public static string TowerRoot(string building) => "Torre_" + building;

    /// <summary>Pone <paramref name="floor"/> como el SIGUIENTE PISO de la torre: la planta baja a <see cref="ElevatedHeight"/>
    /// sobre su sitio, y los demás 11 m sobre el piso más alto que ya tenga (StackOn: el mismo octógono, columnas
    /// continuas). Devuelve la planta del piso (la de la torre).</summary>
    static Vector2 TowerFloor(Transform floor)
    {
        if (_towerHelix != null)
        {
            int k = 1;
            while (k <= _towerHelix.Platforms && (TreeSpec.Platform(k).tip || _towerHelix.decks[k] != null)) k++;
            if (k > _towerHelix.Platforms) { Debug.LogError($"[Torres] «{_towerHelix.building}» no tiene más pisos para «{floor.name}»: súbele un nivel a su hélice."); return _towerSize; }
            HelixFloor(_towerHelix, k, floor, _towerSize);
            _towerTop = floor;
            return _towerSize;
        }
        if (_towerTop == null)
        {
            floor.SetPositionAndRotation(_towerCenter, Quaternion.identity);
            Elevate(floor, _towerSize, ElevatedHeight);
        }
        else StackOn(floor, _towerTop, _towerSize);
        _towerTop = floor;
        return _towerSize;
    }

    /// <summary>La planta del piso <paramref name="floor"/> (la de su torre), o <paramref name="fallback"/> si no está elevado.</summary>
    static Vector2 FloorSize(Transform floor, Vector2 fallback) => floor != null && _platforms.TryGetValue(floor, out Vector2 s) ? s : fallback;

    /// <summary>Nombre del EDIFICIO del piso <paramref name="floor"/> (el de su torre si lo tiene; si no, el de su planta baja):
    /// es la red de sus alfombras de edificio.</summary>
    static string BuildingOf(Transform floor, string fallback)
    {
        Transform b = BaseOf(floor);
        if (b != null && BuildingNames.TryGetValue(b, out string n)) return n;
        SanctuaryArea a = b != null ? b.GetComponent<SanctuaryArea>() : null;
        return a != null ? a.displayName : fallback;
    }

    /// <summary>Número de piso (1 = planta baja) de <paramref name="floor"/> en su edificio, como lo cuentan las alfombras.</summary>
    static int FloorIndex(Transform floor)
    {
        List<Transform> fl = FloorsOf(BaseOf(floor));
        return Mathf.Max(1, fl.IndexOf(floor) + 1);
    }

    /// <summary>Un PISO PROPIO de un área (hija suya, p. ej. la planta veterinaria del hospital), como siguiente piso de la torre.
    /// Va encima de <paramref name="below"/>. Si ya existe (lo dejó preparado <see cref="BuildAreaStructures"/>, que levanta
    /// las torres piso a piso), lo devuelve tal cual.</summary>
    static Transform InnerFloor(Transform area, string name, Transform below)
    {
        Transform t = area.Find(name);
        if (t != null && _platforms.ContainsKey(t)) return t;
        t = new GameObject(name).transform; t.SetParent(area, false);
        if (_towerTop != null && _towerTop == below) TowerFloor(t);
        else StackOn(t, below, FloorSize(below, new Vector2(24f, 18f)));
        return t;
    }
}
