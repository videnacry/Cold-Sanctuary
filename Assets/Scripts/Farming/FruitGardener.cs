using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EL JARDINERO DE LOS FRUTALES (Berón 2026-10-30, docs/jardines-frutales.md): la misión diaria «Ronda de frutales» de la
/// jornada (<see cref="AreaWork"/>; la reparte la rotación como las demás). Quien la tiene va al punto de cuidados del
/// primer techo de su ronda (prepara el abono y mira el cabezal del goteo: filtro, válvula y programador), recorre las macetas
/// en orden, revisa cada una (<see cref="FruitPlant.Care"/>: el gotero, hojas y envés, abona y trata la plaga; el AGUA la pone
/// el goteo, Berón 2026-10-31) el tiempo de su ficha (<see cref="FruitCare.Ficha.minutos"/>, en minutos de juego) y, al acabar
/// un techo, vuela en alfombra al siguiente. Con el tiempo que le queda de su misión (3 h) hace las TAREAS DE LA SEMANA de sus
/// plantas: polinizar a pincel las que lo necesitan y podar y anotar el crecimiento (<see cref="FruitCare.Polinizar"/>,
/// <see cref="FruitCare.PodaDias"/>), mientras quepan.
///
/// ¿UNA PERSONA O VARIAS? <see cref="Rounds"/> mide la ronda entera (revisión de cada planta + andar entre macetas + vuelos
/// + preparar en cada punto) en horas de juego y la parte en rondas de ≤ <see cref="MaxRoundHours"/> h (el bloque de la
/// mañana), por techos seguidos del camino: si todo cabe en una, la hace una persona; si no, la reparte entre varias.
/// Todas las esperas tienen tope (si no llega a una maceta en <see cref="ReachTimeout"/> s, la salta y lo dice).
/// </summary>
public class FruitGardener : MonoBehaviour
{
    public const string MissionPrefix = "jar_frutales";
    /// <summary>Lo más larga que puede ser una ronda (h de juego): el bloque de la mañana (las misiones no se parten en la comida).</summary>
    public const float MaxRoundHours = 3f;
    /// <summary>Holgura (h de juego) que se guarda al repartir el tiempo de las tareas de la semana. Era 0,15 h; medido en Unity
    /// (2026-10-31) la ronda real va siempre algo por DEBAJO de su estimación (2,25/2,36, 2,81/2,88 y 2,86/2,88 h), y con rondas
    /// de 2,9 h esos 0,15 h se comían todo lo que sobra de las 3 h: ninguna polinización ni poda en todo el día.</summary>
    public const float ExtraMargin = 0.05f;
    /// <summary>Lo que se apunta a llenar cada ronda (h): deja margen bajo las 3 h de la misión.</summary>
    public const float RoundTarget = 2.9f;
    /// <summary>Segundos reales estimados de un vuelo de techo a techo (llamar la alfombra, subir, volar a ~26 m/s, posarse).</summary>
    public const float FlightSeconds = 45f;
    /// <summary>Entrar en una torre-árbol desde su techo y salir (nadando por el tronco a su primera punta y, al final, de vuelta al
    /// techo). Medido en Unity el 2026-11-01 con un jardinero por edificio y las macetas punta a punta (<see cref="TipOrder"/>): nado
    /// total de 73 s (Producción, 4 puntas) a 103 s (Ciencias, 8): ~60 s de entrar y salir y el resto, de punta a punta (<see cref="Hop"/>).
    /// (Antes 170 s, medido con las especies saltando de punta en punta: el salto entre puntas iba contado dos veces.)</summary>
    public const float TreeSwimSeconds = 60f;
    /// <summary>Minutos de juego para preparar en el punto de cuidados: el abono y medir pH y EC en la sala principal; en los
    /// demás, mirar el filtro, la válvula y el programador del goteo.</summary>
    public const float PrepMainMinutes = 5f, PrepMinutes = 2f;
    public const float ReachTimeout = 150f;

    public float walkSpeed = 2.6f;

    // ── Rondas (lógica pura sobre las plantas de la escena) ──────────────────────────────────────

    public class Leg
    {
        public string building; public List<FruitPlant> plants = new List<FruitPlant>(); public FruitCareStation station; public float hours;
        /// <summary>Tramo de TAREAS DEL ÁRBOL (Berón 2026-11-03): revisar las máscaras de oxígeno de sus puertas y cuidar el fitoplancton
        /// de su fondo abierto (docs/mascaras-de-oxigeno.md). Sin macetas.</summary>
        public ArbolCampos tree; public List<MaskStation> masks = new List<MaskStation>(); public List<Phytoplankton> beds = new List<Phytoplankton>();
        public bool TreeTasks => tree != null;
        /// <summary>Lo que se planea para las tareas de la semana de sus macetas (h de juego, media diaria; <see cref="WeeklyHoursPerDay"/>) y
        /// lo que de verdad le cupo en su ronda (<see cref="reserve"/>): la ronda deja ese tiempo libre para polinizar y podar.</summary>
        public float weekly, reserve;
    }
    public class Round { public string id, title; public List<Leg> legs = new List<Leg>(); public float hours; public IEnumerable<FruitPlant> Plants => legs.SelectMany(l => l.plants); public IEnumerable<Leg> TreeLegs => legs.Where(l => l.TreeTasks); }

    static List<Round> _rounds; static int _roundsFor = -1;

    static float SecondsPerGameHour => TimeController.timeController != null ? Mathf.Max(1f, TimeController.timeController.SecondsPerGameDay / 24f) : 150f;
    public static float RealSecondsForGameMinutes(float min) => min / 60f * SecondsPerGameHour;

    /// <summary>Lo que se anda (y se nada) de una maceta a otra: en una torre-árbol (docs/edificio-arbol.md §Migración) las puntas
    /// están a distinta altura y en ramas distintas: de una a otra se va a la puerta de su columna, se nada por la rama y el tronco
    /// y se sale por la de la otra (≈ 1,3 × la recta + 10 m).</summary>
    public static float Hop(FruitPlant a, FruitPlant b)
    {
        if (a == null || b == null) return 0f;
        float d = Vector3.Distance(a.transform.position, b.transform.position);
        TreeFloor fa = a.GetComponentInParent<TreeFloor>(), fb = b.GetComponentInParent<TreeFloor>();
        // LAS RESIDENCIAS (2026-11-04: una maceta en la terraza de cada casa del árbol, desde ed774ff): de una casa a otra se va EN
        // ALFOMBRA (bajar, volar, subir), no andando la recta: un vuelo por casa (la ronda de la Residencia no acababa en su tiempo).
        Bedroom ra = a.GetComponentInParent<Bedroom>(), rb = b.GetComponentInParent<Bedroom>();
        if (ra != rb && (ra != null || rb != null)) return d + FlightSeconds * 2.6f;
        return fa != fb ? d * 1.3f + 10f : d;   // (medido en Unity 2026-11-01: de punta a punta de Servicios, ~38 s de juego entre andar a la puerta, nadar y salir; antes 2 × la recta + 60 m)
    }

    /// <summary>LAS TAREAS DE LA SEMANA, de media por día (h de juego), que la ronda RESERVA (2026-11-04): polinizar a pincel cada 2
    /// días las que lo necesitan y podar cada <see cref="FruitCare.PodaDias"/> días. Antes solo se hacían con «lo que sobrara» de las
    /// 3 h, y con las tareas del árbol no sobraba nada (0 polinizadas, 0 podadas).</summary>
    public static float WeeklyHoursPerDay(FruitPlant p)
    {
        float min = FruitCare.Polinizar(p.kind) ? FruitCare.MinPolinizar / 2f : 0f;
        int d = FruitCare.PodaDias(p.kind); if (d > 0) min += FruitCare.MinPodar / d;
        return min * 1.5f / 60f;   // ×1,5: el paso de ir y volver a la planta
    }

    /// <summary>Horas de juego de una revisión en orden de <paramref name="plants"/> (sin vuelo): su ficha + andar.</summary>
    public static float LegHours(List<FruitPlant> plants, bool mainRoom, float walk = 2.6f)
    {
        float min = mainRoom ? PrepMainMinutes : PrepMinutes, walkS = 0f;
        FruitPlant at = plants.Count > 0 ? plants[0] : null;
        foreach (FruitPlant p in plants)
        {
            min += p.Ficha != null ? p.Ficha.minutos : 2f;
            walkS += Hop(at, p) / walk; at = p;
        }
        return min / 60f + walkS / SecondsPerGameHour;
    }

    /// <summary>Las rondas del día (recalculadas si cambian las plantas): techos en el orden del camino más corto desde la
    /// sala principal, recorridos maceta a maceta y partidos en rondas equilibradas de ≤ <see cref="RoundTarget"/> h.</summary>
    public static List<Round> Rounds()
    {
        FruitPlant.All.RemoveAll(p => p == null);
        MaskStation.All.RemoveAll(m => m == null);
        int key = (FruitPlant.All.Count * 1000 + MaskStation.All.Count) * 7 + ((Today % 7) + 7) % 7;   // (la rotación de las máscaras cambia cada día)
        if (_rounds != null && _roundsFor == key) return _rounds;
        _roundsFor = key;
        _rounds = new List<Round>();
        var byRoof = FruitPlant.All.Where(p => !p.HomeTended).GroupBy(p => p.building).ToDictionary(g => g.Key, g => TipOrder(g.OrderBy(p => p.order).ToList()));   // (2026-11-04) las macetas de las casas del árbol las cuida su residente (FruitPlant.HomeTended)
        // LOS EDIFICIOS-ÁRBOL y sus tareas (Berón 2026-11-03): revisar las máscaras de oxígeno y cuidar el fitoplancton. Las hace el
        // jardinero de ese edificio (si no cabe en su ronda, otro jardinero del mismo edificio); el Árbol de los Campos, sin
        // frutales, entra en el camino solo por sus tareas.
        var trees = new Dictionary<string, ArbolCampos>();
        foreach (ArbolCampos t in ArbolCampos.All) if (t != null && t.building != LaunchHeightRule.Building && !trees.ContainsKey(t.building) && TreeStations(t).Count > 0) trees[t.building] = t;   // (la lanzadera, lejos: sus máscaras las revisa su equipo de seguridad)
        var buildings = byRoof.Keys.Union(trees.Keys).ToList();
        if (buildings.Count == 0) return _rounds;
        var stations = Object.FindObjectsByType<FruitCareStation>(FindObjectsSortMode.None);
        FruitCareStation StationOf(string b) => stations.FirstOrDefault(s => s.building == b);
        Vector3 Center(string b)
        {
            if (!byRoof.ContainsKey(b)) return trees[b].transform.position;
            Vector3 c = Vector3.zero; foreach (FruitPlant p in byRoof[b]) c += p.transform.position; return c / byRoof[b].Count;
        }
        // Camino: desde el techo de la sala principal, siempre al edificio más cercano.
        string start = stations.FirstOrDefault(s => s.mainRoom && byRoof.ContainsKey(s.building))?.building ?? buildings.OrderBy(k => k).First();
        var order = new List<string> { start };
        while (order.Count < buildings.Count)
        {
            Vector3 here = Center(order[order.Count - 1]);
            order.Add(buildings.Where(k => !order.Contains(k)).OrderBy(k => Vector3.Distance(here, Center(k))).First());
        }
        // UN JARDINERO POR EDIFICIO (2026-11-01, con todos los edificios hechos árboles): entrar en una torre-árbol y salir cuesta
        // ~1,15 h (TreeSwimSeconds) y saltar de punta a punta, otro tanto por maceta; el llenado seguido de antes partía los
        // edificios entre rondas (Servicios 16 + 3, Entrenamiento 18 + 1…) y cada trozo pagaba otra vez el vuelo y el nado: 7
        // rondas, 17,1 h. Ahora cada edificio es UNA unidad (vuelo, nado, preparar en su punto y sus macetas en orden) y solo se
        // parte si él solo no cabe en una ronda; las unidades seguidas del camino se juntan en una ronda mientras quepan.
        // (2026-11-03) Detrás de las macetas de un edificio-árbol, la unidad de sus TAREAS DEL ÁRBOL (máscaras y fitoplancton): va
        // en la misma ronda si cabe; si no, es otra ronda (otro jardinero del mismo edificio, ese día).
        float flightH = FlightSeconds / SecondsPerGameHour;
        float Swim(string b) => byRoof[b].Any(p => p.GetComponentInParent<TreeFloor>() != null) ? TreeSwimSeconds / SecondsPerGameHour : 0f;   // (las puntas de una torre-árbol)
        float Open(string b, bool first) { FruitCareStation st = StationOf(b); return flightH + Swim(b) + (st != null && st.mainRoom && first ? PrepMainMinutes : PrepMinutes) / 60f; }
        var units = new List<Leg>();
        foreach (string b in order)
        {
            Leg leg = null; FruitPlant at = null; int pieces = 0;
            if (byRoof.ContainsKey(b))
                foreach (FruitPlant p in byRoof[b])
                {
                    float work = (p.Ficha != null ? p.Ficha.minutos : 2f) / 60f, plantH = work + (leg != null ? Hop(at, p) / 2.6f / SecondsPerGameHour : 0f);
                    if (leg == null || (leg.plants.Count > 0 && leg.hours + plantH > RoundTarget))   // (solo si el edificio no cabe entero en una ronda)
                    { leg = new Leg { building = b, station = StationOf(b), hours = Open(b, pieces++ == 0) }; units.Add(leg); plantH = work; }
                    leg.plants.Add(p); leg.hours += plantH; leg.weekly += WeeklyHoursPerDay(p); at = p;
                }
            if (trees.TryGetValue(b, out ArbolCampos tree))
            {
                // LAS TAREAS DEL ÁRBOL, EN TRAMOS QUE QUEPAN EN UNA RONDA (2026-11-04: con las ~200 revisiones de máscaras, un árbol solo
                // pedía 4,6–5,5 h y la ronda pasaba de 3 h). De arriba abajo, tantas puertas como quepan (con su vuelo hasta el techo,
                // entrar y salir); el fitoplancton del fondo, en el último tramo (o en uno suyo si no cabe).
                var (masks, beds) = TreeTaskList(tree);
                var chunk = new List<MaskStation>();
                float budget = RoundTarget - flightH;
                foreach (MaskStation m in masks)
                {
                    chunk.Add(m);
                    if (chunk.Count > 1 && TreeTaskHours(tree, chunk, new List<Phytoplankton>()) > budget)
                    {
                        chunk.RemoveAt(chunk.Count - 1);
                        units.Add(new Leg { building = b, tree = tree, masks = chunk, beds = new List<Phytoplankton>(), hours = TreeTaskHours(tree, chunk, new List<Phytoplankton>()) });
                        chunk = new List<MaskStation> { m };
                    }
                }
                if (beds.Count > 0 && chunk.Count > 0 && TreeTaskHours(tree, chunk, beds) > budget)
                {
                    units.Add(new Leg { building = b, tree = tree, masks = chunk, beds = new List<Phytoplankton>(), hours = TreeTaskHours(tree, chunk, new List<Phytoplankton>()) });
                    chunk = new List<MaskStation>();
                }
                if (chunk.Count > 0 || beds.Count > 0)
                    units.Add(new Leg { building = b, tree = tree, masks = chunk, beds = beds, hours = TreeTaskHours(tree, chunk, beds) });
            }
        }
        // EL REPARTO (2026-11-04): cada tramo cabe ya en una ronda; se llenan las rondas por orden del camino y, si un tramo no cabe en
        // la ronda en curso, se prueba en las anteriores (con su vuelo) antes de abrir otra: menos jardineros, ninguna ronda > 2,9 h.
        foreach (Leg u in units)
        {
            Round fit = null; float fitH = 0f;
            foreach (Round r in _rounds)
            {
                bool afterOwn = r.legs.Count > 0 && r.legs[r.legs.Count - 1].building == u.building;
                float h = u.hours + u.weekly + (u.TreeTasks && !afterOwn ? flightH : 0f);
                if (r.hours + h <= RoundTarget) { fit = r; fitH = h; if (afterOwn) break; }
            }
            // Las macetas de un edificio son UN tramo (no se parten si caben solas en una ronda): si el edificio llena la ronda él
            // solo, se le reserva para la semana lo que quede hasta las 2,9 h.
            if (fit == null)
            {
                fit = new Round(); _rounds.Add(fit);
                float baseH = u.hours + (u.TreeTasks ? flightH : 0f);
                u.reserve = Mathf.Clamp(RoundTarget - baseH, 0f, u.weekly);
                fitH = baseH + u.reserve;
            }
            else u.reserve = u.weekly;
            fit.legs.Add(u); fit.hours += fitH;
        }
        for (int i = 0; i < _rounds.Count; i++)
        {
            Round r = _rounds[i];
            r.id = $"{MissionPrefix}_{i + 1}";
            string roofs = string.Join(", ", r.legs.Where(l => l.plants.Count > 0).Select(l => l.building).Distinct());
            string treeT = string.Join(", ", r.TreeLegs.Select(l => l.building).Distinct());
            int n = r.Plants.Count();
            r.title = n > 0
                ? $"Ronda de frutales {i + 1}/{_rounds.Count}: revisar, abonar, polinizar y podar las macetas (con goteo) del techo de {roofs} ({n} plantas)" + (treeT.Length > 0 ? $"; revisar las máscaras de oxígeno y cuidar el fitoplancton de {treeT}" : "")
                : $"Ronda {i + 1}/{_rounds.Count} del jardinero: revisar las máscaras de oxígeno y cuidar el fitoplancton de {treeT}";
        }
        Debug.Log($"[Frutales] {FruitPlant.All.Count} macetas en {byRoof.Count} techos y tareas de {trees.Count} árboles (máscaras y fitoplancton) · ronda completa {_rounds.Sum(r => r.hours):0.0} h de juego → {_rounds.Count} " +
                  $"{(_rounds.Count == 1 ? "persona" : "personas")} al día (≤ {MaxRoundHours:0} h cada una): " + string.Join(" · ", _rounds.Select(r => $"{r.id} {r.hours:0.0} h [{string.Join("+", r.legs.Select(l => l.TreeTasks ? $"{l.building}: máscaras de {l.masks.Count} puertas y {l.beds.Count} cortinas de fitoplancton {l.hours:0.0} h" : $"{l.building} {l.plants.Count}"))}]")));
        return _rounds;
    }

    // ── TAREAS DEL ÁRBOL: máscaras de oxígeno y fitoplancton (Berón 2026-11-03; docs/mascaras-de-oxigeno.md) ──
    /// <summary>Minutos de juego por puerta: revisar las máscaras del perchero de fuera y de la estación de dentro (juntas y visor,
    /// la válvula y la carga de cada botella; reponer las que falten).</summary>
    public static float MaskDoorMinutes => MaskStation.CheckMinutes * 2f;
    /// <summary>Velocidad (m/s reales) nadando por el tronco con la corriente: la de los viajes (2,5 + 1,4).</summary>
    public const float TaskSwimSpeed = 3.9f;

    /// <summary>Todas las estaciones (dentro del agua; cada una va con el perchero de su puerta) de un árbol, de abajo arriba.</summary>
    public static List<MaskStation> TreeStations(ArbolCampos t)
        => t.GetComponentsInChildren<MaskStation>().Where(m => m.kind == MaskStation.Kind.Station).OrderBy(m => m.transform.position.y).ThenBy(m => m.name).ToList();

    /// <summary>Minutos de juego en leer el PANEL DE MÁSCARAS de la puerta del suelo (la luz y la carga de cada estación del árbol).</summary>
    public const float BoardMinutes = 3f;
    static int Today => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    /// <summary>Lo que toca HOY en un árbol (2026-11-04: revisión BARATA, docs/mascaras-de-oxigeno.md §El jardinero): las estaciones
    /// que le tocan por la ROTACIÓN semanal (1/7 de las puertas cada día) y las que el panel marca en rojo (<see cref="MaskStation.FlagBelow"/>),
    /// de abajo arriba, y sus cortinas de fitoplancton.</summary>
    public static (List<MaskStation> masks, List<Phytoplankton> beds) TreeTaskList(ArbolCampos t)
    {
        int day = Today;
        var all = TreeStations(t);
        var masks = new List<MaskStation>();
        for (int i = 0; i < all.Count; i++) if (all[i].Due(day, i)) masks.Add(all[i]);
        masks.Reverse();   // (de ARRIBA abajo: entra por el techo y baja una sola vez, 2026-11-04)
        var beds = Phytoplankton.All.Where(p => p != null && p.building == t.building).ToList();
        return (masks, beds);
    }

    /// <summary>Horas de juego de las tareas del árbol: leer el panel en la puerta del suelo, entrar, subir nadando a las estaciones de
    /// hoy (≈ 1,3 × la recta + 6 m entre una y otra) y volver al fondo, cuidar las cortinas de fitoplancton y salir por el suelo.</summary>
    public static float TreeTaskHours(ArbolCampos t, List<MaskStation> masks, List<Phytoplankton> beds)
    {
        // (2026-11-04) Entra por el TECHO (el panel de máscaras está en su puerta) y BAJA una sola vez por las estaciones de hoy hasta el
        // fondo: subir desde el suelo y volver contaba la altura del tronco dos veces (1,06 h medidas en el árbol más pequeño, prueba 55).
        Vector3 start = t.Top != null ? t.World(t.Top.inside) : t.transform.position + Vector3.up * t.rootHeight, bottom = t.Ground != null ? t.World(t.Ground.inside) : t.transform.position;
        float swimM = 0f; Vector3 at = start;
        foreach (MaskStation m in masks) { swimM += Vector3.Distance(at, m.SwimPoint) * 1.3f + 6f; at = m.SwimPoint; }
        swimM += Vector3.Distance(at, bottom) * 1.3f;
        at = bottom;
        foreach (Phytoplankton b in beds) { swimM += Vector3.Distance(at, b.transform.position) + 2f; at = b.transform.position; }
        float min = BoardMinutes + masks.Count * MaskDoorMinutes + beds.Count * Phytoplankton.CareMinutes;
        return min / 60f + (swimM / TaskSwimSpeed + TreeSwimSeconds * 0.5f) / SecondsPerGameHour;
    }

    public static Round RoundFor(string missionId) => missionId == null ? null : Rounds().FirstOrDefault(r => r.id == missionId);

    /// <summary>Las macetas de un edificio PUNTA A PUNTA (2026-11-01): en una torre-árbol cada cambio de punta es ir a la puerta de
    /// su columna, nadar por la rama y el tronco y salir por la otra (~1 min, <see cref="Hop"/>); en el orden de reparto las
    /// especies saltaban de una punta a otra. Ahora todas las de una punta seguidas, empezando por la más alta (se entra por el
    /// techo) y siguiendo por la más cercana. En un techo plano no cambia nada.</summary>
    static List<FruitPlant> TipOrder(List<FruitPlant> plants)
    {
        var tips = plants.GroupBy(p => p.GetComponentInParent<TreeFloor>()).Select(g => g.ToList()).ToList();
        if (tips.Count <= 1) return plants;
        Vector3 C(List<FruitPlant> t) { Vector3 c = Vector3.zero; foreach (FruitPlant p in t) c += p.transform.position; return c / t.Count; }
        var left = new List<List<FruitPlant>>(tips);
        var outList = new List<FruitPlant>();
        List<FruitPlant> cur = left.OrderByDescending(t => C(t).y).First();
        while (cur != null)
        {
            outList.AddRange(cur); left.Remove(cur);
            Vector3 here = C(cur);
            cur = left.Count > 0 ? left.OrderBy(t => Vector3.Distance(here, C(t))).First() : null;
        }
        return outList;
    }
    public static bool IsRound(string missionId) => missionId != null && missionId.StartsWith(MissionPrefix);

    /// <summary>Las misiones de la jornada: una por ronda, obligatorias (las plantas en maceta no aguantan un día sin
    /// revisar), que recorren varios edificios (<c>roams</c>), de 3 h: la revisión y, con lo que sobra, las tareas de la semana.</summary>
    public static List<WorkRoster.Mission> Missions()
    {
        var l = new List<WorkRoster.Mission>();
        foreach (Round r in Rounds())
            l.Add(new WorkRoster.Mission("Huerto", r.id, r.title, MaxRoundHours, true) { roams = true });   // 3 h: lo que sobra de la revisión, para polinizar y podar
        return l;
    }

    /// <summary>Dónde empieza la ronda (el punto de cuidados de su primer techo), para colocar a quien la tiene al arrancar.</summary>
    public static Vector3? StartPoint(string missionId)
    {
        Round r = RoundFor(missionId);
        if (r == null || r.legs.Count == 0) return null;
        Leg l = r.legs[0];
        if (l.TreeTasks && l.tree.Top != null) return l.tree.World(l.tree.Top.outside) + Vector3.up;   // (una ronda que empieza por las tareas de un árbol: en su techo)
        return l.station != null ? l.station.StandPoint : l.plants.Count > 0 ? l.plants[0].StandPoint(l.plants[0].transform.position + Vector3.forward) : (Vector3?)null;
    }

    // ── La ronda en el juego ─────────────────────────────────────────────────────────────────────

    enum Phase { ToStation, Prep, ToPlant, Caring, ToExtra, Extra, ToTree, TreeSwim, TreeWork, TreeExit, Done }
    Phase _phase = Phase.Done;
    Round _round;
    int _leg, _plant;
    float _until, _targetSince, _startedAt, _boardUntil;
    Transform _can;

    public string MissionId { get; private set; }
    public bool Finished => _phase == Phase.Done && _round != null;
    public int Cared { get; private set; }
    public int Skipped { get; private set; }
    public int Pollinated { get; private set; }
    public int Pruned { get; private set; }
    public int MasksChecked { get; private set; }
    public int PhytoCared { get; private set; }
    public float RoundSeconds { get; private set; }
    public readonly List<string> roofsDone = new List<string>();
    public string Doing { get; private set; }

    /// <summary>Le da la ronda <paramref name="missionId"/> (si ya la tiene, sigue donde iba).</summary>
    public void Assign(string missionId) => Assign(missionId, RoundFor(missionId));

    /// <summary>Solo las tareas del árbol <paramref name="t"/> (máscaras y fitoplancton), como una ronda de un tramo (pruebas).</summary>
    public void AssignTreeTasks(ArbolCampos t)
    {
        var (masks, beds) = TreeTaskList(t);
        Round r = new Round { id = $"{MissionPrefix}_arbol_{t.building}", title = $"Revisar las máscaras de oxígeno y cuidar el fitoplancton de {t.building}" };
        r.legs.Add(new Leg { building = t.building, tree = t, masks = masks, beds = beds, hours = TreeTaskHours(t, masks, beds) });
        r.hours = r.legs[0].hours;
        MissionId = null;
        Assign(r.id, r);
    }

    void Assign(string missionId, Round round)
    {
        if (MissionId == missionId && _round != null) return;
        MissionId = missionId; _round = round;
        _leg = 0; _plant = 0; Cared = 0; Skipped = 0; Pollinated = 0; Pruned = 0; MasksChecked = 0; PhytoCared = 0; _wps.Clear(); _extras.Clear(); roofsDone.Clear(); RoundSeconds = 0f;
        _startedAt = Time.time;
        if (_round == null || _round.legs.Count == 0) { _phase = Phase.Done; Doing = "Sin frutales que revisar"; return; }
        Begin(_round.legs[0].TreeTasks ? Phase.ToTree : Phase.ToStation);
        Debug.Log($"[Frutales] «{name}» empieza «{_round.title}» (estimado {_round.hours:0.0} h de juego).");
    }


    // ── LAS TAREAS DEL ÁRBOL EN EL JUEGO (Berón 2026-11-03; docs/mascaras-de-oxigeno.md) ──
    // Va al techo de su árbol, coge una máscara del perchero y entra; baja nadando de puerta en puerta (por el camino de los viajes:
    // el tronco, las ramas y las columnas) y en cada una revisa las máscaras de la estación de dentro y del perchero de fuera; al
    // final baja al fondo abierto a cuidar las cortinas de fitoplancton y sale por la puerta del suelo (dejando la máscara).
    int _task;
    TreeLevel _lastLevel;
    readonly List<System.Func<Vector3>> _wps = new List<System.Func<Vector3>>();
    int _wp;

    void TreeUpdate(Leg leg)
    {
        ArbolCampos t = leg.tree;
        if (t == null) { EndLeg(); return; }
        switch (_phase)
        {
            case Phase.ToTree:
            {
                // (2026-11-04) Por la PUERTA DEL SUELO: primero lee el panel de máscaras (la luz y la carga de cada estación), luego
                // entra a las estaciones de hoy (la rotación y las que están en rojo) y al fitoplancton del fondo, que está ahí mismo.
                TreeLevel gl = t.Top;   // (2026-11-04: por el TECHO, y baja una sola vez)
                if (gl == null) { EndLeg(); return; }
                Doing = $"Va al techo de {leg.building} a revisar las máscaras de oxígeno y el fitoplancton";
                if (t.InRootWater(transform.position)) { _lastLevel = t.LevelAt(transform.position) ?? gl; StartTreeTasks(t, leg, false); return; }
                Vector3 to = t.World(gl.outside) + Vector3.up;
                if (_boardUntil <= 0f)
                {
                    if (!Reach(to)) { if (_timedOut) { Debug.LogWarning($"[Frutales] «{name}» no llega al techo de {leg.building}: deja sus máscaras para otro día."); EndLeg(); } return; }
                    _boardUntil = Time.time + RealSecondsForGameMinutes(BoardMinutes);
                    Debug.Log($"[Frutales] «{name}» lee el panel de máscaras de {leg.building}: hoy revisa {leg.masks.Count} de {TreeStations(t).Count} estaciones (rotación semanal y las que están en rojo).");
                }
                Doing = $"Lee el panel de máscaras de {leg.building}: la luz y la carga de cada estación";
                if (Time.time < _boardUntil) return;
                _boardUntil = 0f;
                MaskStation rack = MaskStation.Nearest(transform.position, MaskStation.Kind.Rack, 7f, false, true) ?? MaskStation.Nearest(t.World(gl.inside), MaskStation.Kind.Station, 9f, true, true);
                if (rack != null) BreathingMask.Of(transform).Equip(rack);
                _lastLevel = gl;
                StartTreeTasks(t, leg, true);
                break;
            }
            case Phase.TreeSwim:
            {
                if (_wp >= _wps.Count) { ArriveTask(t, leg); return; }
                Vector3 target = _wps[_wp]();
                Vector3 d = target - transform.position;
                SwimBody sb = SwimBody.Of(this);
                float step = TaskSwimSpeed * Time.deltaTime;
                if (WaterZone.Underwater(target + Vector3.up * 0.5f)) sb.SwimTo(target, TaskSwimSpeed);
                else { sb.Begin(); transform.position += d.normalized * Mathf.Min(step, d.magnitude); }
                if ((target - transform.position).magnitude < 0.6f) { _wp++; _targetSince = Time.time; }
                else if (Time.time - _targetSince > ReachTimeout)
                {
                    Debug.LogWarning($"[Frutales] «{name}» no llega nadando a su tarea {_task + 1} en {leg.building} en {ReachTimeout:0} s: la salta.");
                    Skipped++; NextTreeTask(t, leg);
                }
                break;
            }
            case Phase.TreeWork:
                if (Time.time < _until) return;
                Hold(false);
                if (_task < leg.masks.Count)
                {
                    MaskStation st = leg.masks[_task];
                    if (st != null) foreach (MaskStation m in st.transform.parent.GetComponentsInChildren<MaskStation>()) m.Check(name);
                    MasksChecked++;
                }
                else { Phytoplankton b = leg.beds[_task - leg.masks.Count]; if (b != null) b.Care(name); PhytoCared++; }
                NextTreeTask(t, leg);
                break;
            case Phase.TreeExit:
                Doing = $"Sale del tronco de {leg.building} por la puerta del suelo";
                if (!TreeTrip.Busy(gameObject) || Time.time - _targetSince > ReachTimeout)
                {
                    if (BreathingMask.Has(gameObject)) BreathingMask.Of(transform).Return();
                    Debug.Log($"[Frutales] «{name}» termina las tareas de {leg.building}: máscaras de {MasksChecked} puertas revisadas y {PhytoCared} cortinas de fitoplancton cuidadas.");
                    EndLeg();
                }
                break;
            default:
                Begin(Phase.ToTree);
                break;
        }
    }

    void StartTreeTasks(ArbolCampos t, Leg leg, bool fromOutside)
    {
        _task = -1;
        if (fromOutside && t.Top != null) { TreeLevel gl = t.Top; _wps.Clear(); _wps.Add(() => t.World(gl.door) + Vector3.up); _wps.Add(() => t.World(gl.inside) + Vector3.up); }
        Debug.Log($"[Frutales] «{name}» entra en {leg.building} a revisar las máscaras de {leg.masks.Count} puertas y {leg.beds.Count} cortinas de fitoplancton.");
        NextTreeTask(t, leg, keepEntry: fromOutside);
    }

    TreeLevel LevelOf(ArbolCampos t, MaskStation m)
    {
        if (m == null) return null;
        Transform door = m.transform.parent;
        foreach (TreeLevel l in t.levels) if (l.field != null && l.field.parent == door) return l;
        return null;
    }

    // La siguiente tarea: el camino a nado (puntos del árbol, que puede girar) hasta su estación o su cortina.
    void NextTreeTask(ArbolCampos t, Leg leg, bool keepEntry = false)
    {
        Hold(false);
        _task++;
        if (!keepEntry) _wps.Clear();
        _wp = 0;
        int total = leg.masks.Count + leg.beds.Count;
        TreeLevel ground = t.Ground;
        if (_task >= total)
        {
            // Sale por la puerta del suelo (un viaje de TreeTrip: deja la máscara en el perchero).
            if (ground == null) { EndLeg(); return; }
            Vector3 here = t.transform.InverseTransformPoint(transform.position - Vector3.up * PathMover.Of(this).PivotHeight);
            var path = new List<Vector3> { here, ground.inside, ground.door, ground.outside };
            SwimBody.Of(this).End();
            TreeTrip.Start(gameObject, t, path, ground, true);
            _phase = Phase.TreeExit; _targetSince = Time.time;
            return;
        }
        float ph = PathMover.Of(this).PivotHeight;
        TreeLevel to = _task < leg.masks.Count ? LevelOf(t, leg.masks[_task]) : ground;
        if (to != null && _lastLevel != null && to != _lastLevel)
        {
            List<Vector3> p = t.SwimPath(_lastLevel, to);
            for (int i = 2; i < p.Count - 2; i++) { Vector3 l = p[i]; _wps.Add(() => t.World(l) + Vector3.up * ph); }
        }
        if (to != null) _lastLevel = to;
        if (_task < leg.masks.Count) { MaskStation m = leg.masks[_task]; _wps.Add(() => m != null ? m.SwimPoint + Vector3.down * 0.7f : transform.position); }
        else
        {
            Phytoplankton b = leg.beds[_task - leg.masks.Count];
            if (_task == leg.masks.Count) _wps.Add(() => t.transform.position + Vector3.up * 0.5f);   // al centro del fondo, sobre el hueco
            _wps.Add(() => b != null ? b.transform.position + new Vector3(0f, 0.2f, -1.2f) : transform.position);
        }
        _phase = Phase.TreeSwim; _targetSince = Time.time;
    }

    void ArriveTask(ArbolCampos t, Leg leg)
    {
        bool mask = _task < leg.masks.Count;
        float min = mask ? MaskDoorMinutes : Phytoplankton.CareMinutes;
        if (mask) { TreeLevel lv = LevelOf(t, leg.masks[_task]); Doing = $"Revisa las máscaras de oxígeno de la puerta de {lv?.name ?? "su nivel"} ({_task + 1}/{leg.masks.Count}, {leg.building}): juntas, visor, válvula y carga"; }
        else Doing = $"Cuida el fitoplancton del fondo de {leg.building}: limpia las cortinas, mide y abona (nitrato, fosfato, hierro)";
        _until = Time.time + RealSecondsForGameMinutes(min);
        _phase = Phase.TreeWork; Hold(!mask);
    }

    void Begin(Phase p) { _phase = p; _targetSince = Time.time; PathMover.Of(this).Restart(); }

    void OnEnable() { if (TryGetComponent(out IdleBehavior idle)) idle.enabled = false; }
    void OnDisable() { Hold(false); }

    Leg CurLeg => _round != null && _leg < _round.legs.Count ? _round.legs[_leg] : null;

    void Update()
    {
        if (_round == null || _phase == Phase.Done) return;
        // Las NECESIDADES van antes (baño, comer, dormir), como el guardia.
        if (TryGetComponent(out ActionQueue q) && q.Own.Count > 0 && (q.Own[0] is ToiletSpell || q.Own[0] is DineSpell || q.Own[0] is SleepSpell || q.Own[0] is EatSpell)) return;
        // TOPE DE LA RONDA (2026-11-04): su misión dura 3 h; si se alarga (una maceta que no se alcanza, tareas de la semana lentas), a las
        // 3,2 h la da por terminada y lo dice: lo que queda, al día siguiente.
        if (Time.time - _startedAt > (MaxRoundHours + 0.2f) * SecondsPerGameHour && !(CurLeg != null && CurLeg.TreeTasks)) { if (CurLeg != null && !roofsDone.Contains(CurLeg.building)) roofsDone.Add(CurLeg.building); Debug.LogWarning($"[Frutales] «{name}»: «{MissionId}» pasa de {MaxRoundHours + 0.2f:0.0} h: la da por terminada (va por «{Doing}»)."); Finish(); return; }
        Leg leg = CurLeg;
        if (leg == null) { Finish(); return; }
        if (leg.TreeTasks) { TreeUpdate(leg); return; }
        switch (_phase)
        {
            case Phase.ToStation:
                if (leg.station == null) { Begin(Phase.ToPlant); return; }
                Doing = $"Va al punto de cuidados del techo de {leg.building}";
                if (Reach(leg.station.StandPoint)) { _until = Time.time + RealSecondsForGameMinutes(leg.station.mainRoom && _leg == 0 ? PrepMainMinutes : PrepMinutes); _phase = Phase.Prep; }
                break;
            case Phase.Prep:
                Doing = leg.station != null && leg.station.mainRoom ? "Prepara el abono, mide pH y EC y mira el cabezal del goteo en la sala de cuidados" : $"Mira el filtro, la válvula y el programador del goteo del techo de {leg.building}";
                if (Time.time >= _until) Begin(Phase.ToPlant);
                break;
            case Phase.ToPlant:
            {
                FruitPlant p = _plant < leg.plants.Count ? leg.plants[_plant] : null;
                if (p == null) { NextPlant(false); return; }
                Doing = $"Va al {p.Name} ({_plant + 1}/{leg.plants.Count}, techo de {leg.building})";
                if (Reach(p.StandPoint(transform.position)))
                {
                    Vector3 look = p.transform.position - transform.position; look.y = 0f;
                    if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(look);
                    _until = Time.time + RealSecondsForGameMinutes(p.Ficha != null ? p.Ficha.minutos : 2f);
                    _phase = Phase.Caring; Hold(true);
                }
                break;
            }
            case Phase.Caring:
            {
                FruitPlant p = _plant < leg.plants.Count ? leg.plants[_plant] : null;
                if (p == null) { NextPlant(false); return; }
                Doing = $"Revisa el {p.Name}: el gotero, hojas y envés, abono y plaga";
                if (Time.time >= _until)
                {
                    var (b, a) = p.Care(name);
                    Cared++;
                    if (Cared <= 3 || Cared % 10 == 0) Debug.Log($"[Frutales] «{name}» cuida el {p.Name} del techo de {leg.building}: bienestar {b * 100f:0}% → {a * 100f:0}%.");
                    NextPlant(true);
                }
                break;
            }
            case Phase.ToExtra:
            {
                if (_extras.Count == 0 || Time.time > _extraUntil + RealSecondsForGameMinutes(5f)) { _extras.Clear(); Hold(false); EndLeg(); return; }   // (2026-11-04) las tareas de la semana no pasan de su tiempo: lo que no cabe, otro día
                FruitPlant p = _extras[0];
                if (p == null) { _extras.RemoveAt(0); return; }
                Doing = $"Va al {p.Name} a {ExtraWhat(p)} (techo de {leg.building})";
                if (Reach(p.StandPoint(transform.position)))
                {
                    Vector3 look = p.transform.position - transform.position; look.y = 0f;
                    if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(look);
                    _until = Time.time + RealSecondsForGameMinutes(ExtraMinutes(p)); _phase = Phase.Extra; Hold(true);
                }
                break;
            }
            case Phase.Extra:
            {
                FruitPlant p = _extras.Count > 0 ? _extras[0] : null;
                if (p == null) { Begin(Phase.ToExtra); return; }
                Doing = $"{char.ToUpper(ExtraWhat(p)[0])}{ExtraWhat(p).Substring(1)} el {p.Name}";
                if (Time.time >= _until)
                {
                    int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
                    if (p.NeedsPollination(day)) { p.Pollinate(name); Pollinated++; }
                    if (p.NeedsPruning(day)) { p.Prune(name); Pruned++; }
                    _extras.RemoveAt(0); Hold(false);
                    NextExtra();
                }
                break;
            }
        }
    }

    // ── LAS TAREAS DE LA SEMANA (el tiempo que libera el goteo) ──
    // Al acabar la revisión de un techo, las plantas de ese tramo a las que hoy les toca polinizar o podar, mientras quepan en
    // su parte del tiempo que sobra de la misión (3 h menos lo que falta de revisión y vuelos, repartido por macetas).
    readonly List<FruitPlant> _extras = new List<FruitPlant>();
    float _extraUntil;
    static string ExtraWhat(FruitPlant p)
    {
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        bool po = p.NeedsPollination(day), pr = p.NeedsPruning(day);
        return po && pr ? "polinizar y podar" : po ? "polinizar a pincel" : "podar y anotar el crecimiento de";
    }
    static float ExtraMinutes(FruitPlant p)
    {
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        return (p.NeedsPollination(day) ? FruitCare.MinPolinizar : 0f) + (p.NeedsPruning(day) ? FruitCare.MinPodar : 0f);
    }

    void StartExtras(Leg leg)
    {
        _extras.Clear();
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        foreach (FruitPlant p in leg.plants) if (p != null && (p.NeedsPollination(day) || p.NeedsPruning(day))) _extras.Add(p);
        float flightH = FlightSeconds / SecondsPerGameHour, elapsed = (Time.time - _startedAt) / SecondsPerGameHour, rest = 0f; int restPlants = 0;
        for (int i = _leg + 1; i < _round.legs.Count; i++) { rest += _round.legs[i].hours + (_round.legs[i].building != _round.legs[i - 1].building ? flightH : 0f); restPlants += _round.legs[i].plants.Count; }
        float spare = Mathf.Max(0f, MaxRoundHours - ExtraMargin - elapsed - rest);
        float share = spare * leg.plants.Count / Mathf.Max(1, leg.plants.Count + restPlants);
        share = Mathf.Max(share, leg.reserve);   // (2026-11-04) lo que la ronda reservó para la semana de este tramo, aunque la revisión haya ido lenta
        _extraUntil = Time.time + share * SecondsPerGameHour;
        NextExtra();
    }

    // La siguiente tarea si cabe (andar hasta ella + su tiempo); si no, se deja para otro día y sigue la ronda.
    void NextExtra()
    {
        if (_extras.Count > 0)
        {
            FruitPlant p = _extras[0];
            float cost = p == null ? 0f : Vector3.Distance(transform.position, p.transform.position) / walkSpeed + RealSecondsForGameMinutes(ExtraMinutes(p));
            if (p != null && Time.time + cost <= _extraUntil) { Begin(Phase.ToExtra); return; }
        }
        _extras.Clear();
        EndLeg();
    }

    // Anda (o vuela) hacia el punto; true al llegar. Con tope: si no llega, lo dice y lo da por imposible.
    bool _timedOut;
    bool Reach(Vector3 to)
    {
        _timedOut = false;
        float d = PathMover.Of(this).StepToward(to, walkSpeed);
        Vector3 flat = to - transform.position; flat.y = 0f;
        if (flat.magnitude < 1.2f && Mathf.Abs(to.y - transform.position.y) < 2.5f) return true;
        if (FlyingCarpet.Riding(gameObject)) { _targetSince = Time.time; return false; }   // volando no cuenta
        if (Time.time - _targetSince > ReachTimeout)
        {
            _timedOut = true;
            Debug.LogWarning($"[Frutales] «{name}» no llega a ({to.x:0},{to.y:0},{to.z:0}) en {ReachTimeout:0} s (queda {d:0.0} m): lo salta.");
            if (_phase == Phase.ToTree) return false;   // (las tareas del árbol lo resuelven ellas)
            if (_phase == Phase.ToStation) { Begin(Phase.ToPlant); return false; }
            if (_phase == Phase.ToExtra) { if (_extras.Count > 0) _extras.RemoveAt(0); NextExtra(); return false; }
            Skipped++; NextPlant(false);
        }
        return false;
    }

    void NextPlant(bool cared)
    {
        Hold(false);
        Leg leg = CurLeg;
        _plant++;
        if (leg != null && _plant >= leg.plants.Count) { StartExtras(leg); return; }   // revisado el tramo: sus tareas de la semana
        Begin(Phase.ToPlant);
    }

    // Fin de un tramo (revisión y tareas): al siguiente, en su techo o volando al de al lado.
    void EndLeg()
    {
        Hold(false);
        Leg leg = CurLeg;
        if (leg != null && !roofsDone.Contains(leg.building)) roofsDone.Add(leg.building);
        _leg++; _plant = 0;
        if (CurLeg == null) { Finish(); return; }
        Begin(CurLeg.TreeTasks ? Phase.ToTree : leg != null && CurLeg.building == leg.building ? Phase.ToPlant : Phase.ToStation);
    }

    void Finish()
    {
        Hold(false);
        _phase = Phase.Done;
        RoundSeconds = Time.time - _startedAt;
        float h = RoundSeconds / SecondsPerGameHour;
        Doing = $"Ronda terminada: {Cared} frutales revisadas, {Pollinated} polinizadas y {Pruned} podadas en {h:0.0} h";
        Debug.Log($"[Frutales] «{name}» termina «{_round.title}»: {Cared} cuidadas, {Skipped} saltadas, {Pollinated} polinizadas, {Pruned} podadas; techos {string.Join(", ", roofsDone)}; {RoundSeconds:0} s de juego = {h:0.00} h de juego (estimado {_round.hours:0.00} h).");
    }

    // La regadera en la mano mientras revisa una maceta.
    void Hold(bool on)
    {
        if (on && _can == null)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(g.GetComponent<Collider>());
            g.name = "Regadera"; _can = g.transform; _can.SetParent(transform, false);
            _can.localPosition = new Vector3(0.35f, 0.1f, 0.45f); _can.localScale = new Vector3(0.22f, 0.25f, 0.35f);
            g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.35f, 0.62f, 0.55f));
        }
        else if (!on && _can != null) { Destroy(_can.gameObject); _can = null; }
    }
}
