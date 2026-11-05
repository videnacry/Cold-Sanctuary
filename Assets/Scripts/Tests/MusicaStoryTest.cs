using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TEST DE LOS MICROCOSMOS DE LA MÚSICA (docs/microcosmos-musica.md; grupo 86). Carga UNA A UNA (aditiva, espera acotada, y la descarga
/// antes de la siguiente: la memoria) las escenas de la música y mira en cada una:
/// - su mapa a mano hecho (flora, fauna, orden del piso y edificios) y su fila en la tabla de pisos;
/// - su fecha, su región y su piso (representativo o «pendiente de piso representativo»);
/// - el protagonista como insecto-humano y la historia: 8 pasos con objetivo, que avanzan, y su ficha con ≥ 4 fuentes, que avisa de que no
///   reproduce letras (<see cref="NoLyricsNote"/>);
/// - el pueblo entero: ≥ 12 edificios, las cuatro categorías y cada residencia con su frutal de época y lugar;
/// - «Grandes aportes de las canciones»: sus seis rincones, y la ficha dice que el sitio es inventado;
/// - las reglas de edificación y las de los microcosmos.
/// Sin la escena en Build Settings, la salta con aviso (hay que generarla con su menú).
/// </summary>
public class MusicaStoryTest : MonoBehaviour, ITestUnit
{
    public int Group => 86;   // tras los famosos (69): carga y descarga escenas, de una en una
    public bool ParallelSafe => false;
    public float loadTimeout = 180f;
    const string NoLyricsNote = "sin reproducir sus letras";

    // (escena, nombre en la tabla, protagonista, año, región, piso, ¿representativo?, frutales de su entrada)
    static readonly (string scene, string display, string hero, int year, string region, string platform, bool rep, string[] fruits)[] Cases =
    {
        ("Microcosmos_Shakira", "Barranquilla", "Shakira", 2009, "CaribeHispano", "Classroom_Area", true, new[] { "Mango", "Guanábana", "Aguacate", "Quenepa" }),
        ("Microcosmos_BadBunny", "Vega Baja", "Bad Bunny", 2016, "CaribeHispano", "Music_Area", true, new[] { "Mango", "Guanábana", "Aguacate", "Quenepa" }),
        ("Microcosmos_JohnLennon", "Montreal", "John Lennon", 1969, "NorteamericaNorte", "Music_Area", true, new[] { "Manzano", "Cerezo", "Ciruelo" }),
        ("Microcosmos_MichaelJackson", "Gary", "Michael Jackson", 1966, "NorteamericaNorte", "Music_Area", true, new[] { "Manzano", "Cerezo", "Ciruelo" }),
        ("Microcosmos_Blackpink", "Seúl", "Jennie", 2016, "Corea", "Ballet_Area", true, new[] { "Caqui", "Peral", "Azufaifo" }),
        ("Microcosmos_Wailers", "Kingston", "Bob Marley", 1978, "Jamaica", "Music_Area", true, new[] { "Ackee", "Árbol del pan", "Mango" }),
        ("Microcosmos_U2", "Belfast", "Bono", 1998, "IslasBritanicas", "Music_Area", true, new[] { "Manzano", "Ciruelo", "Peral" }),
        ("Microcosmos_PublicEnemy", "Long Island", "Chuck D", 1989, "NorteamericaNorte", "Music_Area", true, new[] { "Manzano", "Cerezo", "Ciruelo" }),
        ("Microcosmos_GrandesCanciones", "Grandes aportes de las canciones", "Cronista", 2025, "Hispanoamerica", "Music_Area", true, new[] { "Limonero", "Manzano" }),
        // ══FIN DE LOS CASOS══
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<MusicaStoryTest>() == null) tr.gameObject.AddComponent<MusicaStoryTest>();
    }

    public IEnumerator Run()
    {
        Floor();
        foreach (var c in Cases) yield return One(c.scene, c.display, c.hero, c.year, c.region, c.platform, c.rep, c.fruits);
    }

    /// <summary>EL PISO DE MÚSICA (docs/piso-musica.md): sus misiones, su uniforme y, con la escena construida, su sitio y su capacidad.</summary>
    static void Floor()
    {
        TestProbe.Begin("Piso de Música: misiones, uniforme, torre y capacidad");
        foreach (string id in new[] { "son_grabar", "son_ensayo", "son_equipo" })
        {
            TestProbe.Check($"«{id}» está en el registro, en Música", AreaFunctions.Get(id) != null && AreaFunctions.Get(id).area == "Música");
            TestProbe.Check($"«{id}» tiene sus pasos con «E»", PlayerTasks.Authored(id));
        }
        TestProbe.Check("ningún microcosmos sigue pendiente de sala de grabación", !System.Array.Exists(MicroFloors.Table, m => m.pending.Contains("grabación")));
        TestProbe.Check("Viena 1824 está en el piso de Música", MicroFloors.Of("Viena 1824") != null && MicroFloors.Of("Viena 1824").platform == "Music_Area");
        // La cuenta acústica (docs/piso-musica.md §Acústica): sin tapa o con la puerta abierta, el cristal casi no aísla.
        var cell = new Vector2(8.4f, 6.2f);
        TestProbe.Check("sala cerrada de doble cristal: ≥ 40 dB", MusicAcoustics.Room(cell, false) >= 40f, $"{MusicAcoustics.Room(cell, false):0} dB");
        TestProbe.Check("entre dos salas vecinas llega a la meta", MusicAcoustics.BetweenRooms(cell) >= MusicAcoustics.TargetBetweenRooms, $"{MusicAcoustics.BetweenRooms(cell):0} dB");
        TestProbe.Check("con la puerta abierta o sin tapa se pierde casi todo", MusicAcoustics.Room(cell, true) < 20f && MusicAcoustics.Room(cell, false, false) < 10f, $"{MusicAcoustics.Room(cell, true):0} y {MusicAcoustics.Room(cell, false, false):0} dB");
        SanctuaryArea area = null;
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (a.name == "Music_Area") area = a;
        if (area == null) { TestProbe.Check("el piso está en la escena (reconstruye SampleScene)", false); TestProbe.End(); return; }
        TestProbe.Check("es del tipo Music", area.areaType == SanctuaryAreaType.Music);
        var lines = AreaCapacity.Check(new[] { area.transform }, AreaCapacity.Members(), out _);
        foreach (string act in new[] { "concierto", "tocar en concierto", "grabar", "mezcla", "ensayar", "sonido" })
            TestProbe.Check($"capacidad: {act}", lines.Exists(l => l.area == "Música" && l.activity == act && l.ok), string.Join(" · ", lines.ConvertAll(l => l.ToString())));
        int rooms = 0, lids = 0; foreach (Transform t in area.GetComponentsInChildren<Transform>(true)) { if (t.name.StartsWith("Sala_cristal_Sonica_") && t.name.EndsWith("_fuera")) rooms++; if (t.name.StartsWith("Tapa_cristal_")) lids++; }
        TestProbe.Check("cada sala aislada lleva doble cristal y tapa", rooms >= 9 && lids == rooms, $"{rooms} salas, {lids} tapas");
        int stages = 0, studios = 0, bands = 0; foreach (Transform t in area.GetComponentsInChildren<Transform>(true)) { if (t.name == "Escenario de conciertos" || t.name == "Segundo escenario") stages++; if (t.name.StartsWith("Cabina de grabación")) studios++; if (t.name.StartsWith("Banda de ensayo")) bands++; }
        TestProbe.Check("más de un escenario y de un estudio, y varias salas de ensayo a la vez", stages >= 2 && studios >= 2 && bands * 5 >= AreaCapacity.Members(), $"{stages} escenarios, {studios} estudios, {bands} salas");
        int leaves = area.GetComponentsInChildren<SoundDoor>(true).Length;
        TestProbe.Check("cada sala cierra sus dos puertas correderas (SoundDoor)", leaves == rooms * 2, $"{leaves} hojas en {rooms} salas");
        int ramps = 0; foreach (Transform t in area.GetComponentsInChildren<Transform>(true)) if (t.name == "Rampa_del_escenario") ramps++;
        TestProbe.Check("cada escenario tiene su rampa", ramps == stages, $"{ramps}/{stages}");
        TestProbe.Check("los huecos de las salas de cristal miden los 2 m de paso", GlassWall.DoorWidth >= FurnitureSpacing.Standard);
        var spacing = FurnitureSpacing.Violations(area.transform);
        TestProbe.Check("2 m de paso, también con el cristal, dentro y fuera", spacing.Count == 0, $"{spacing.Count} pares");
        TestProbe.End();
    }

    IEnumerator One(string scene, string display, string hero, int year, string region, string platform, bool rep, string[] fruits)
    {
        TestProbe.Begin($"Música: {hero} ({scene})");
        // Lo que no necesita la escena: la tabla de pisos y el mapa a mano.
        MicroStoryMaps.Map map = MicroStoryMaps.For(display);
        TestProbe.Check("su mapa a mano está HECHO, con flora, fauna, piso y edificios",
            map != null && map.status == MicroStoryMaps.Status.Hecho && map.flora.Length > 0 && map.fauna.Length > 0 && map.floor.Length > 0 && map.buildings.Length > 0, map != null ? $"{map.PlantCount} plantas, {map.AnimalCount} bichos" : "sin mapa");
        MicroFloors.Match row = MicroFloors.Of(display);
        TestProbe.Check($"su fila en la tabla de pisos: {platform}", row != null && row.platform == platform && row.representative == rep, row != null ? $"{row.platform} «{row.pending}»" : "sin fila");
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            TestProbe.Check($"«{scene}» está en Build Settings", false, "genera «Tools/Cold Sanctuary/Build Microcosmos de música/…» (de uno en uno)");
            TestProbe.End(); yield break;
        }
        Scene s = SceneManager.GetSceneByName(scene);
        bool loadedHere = false;
        if (!s.isLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (op != null && !op.isDone && Time.realtimeSinceStartup < until) yield return null;
            s = SceneManager.GetSceneByName(scene);
            if (!TestProbe.Check($"«{scene}» se carga", s.isLoaded, $"no cargó en {loadTimeout:0} s")) { TestProbe.End(); yield break; }
            loadedHere = true;
        }
        yield return null; yield return null;

        StoryMission m = null; MicroFloor floor = null; MicroEra era = null; HistoryCard card = null; Transform heroT = null;
        var sites = new List<AreaFunctionSite>();
        foreach (GameObject g in s.GetRootGameObjects())
        {
            if (m == null) m = g.GetComponentInChildren<StoryMission>(true);
            if (floor == null) floor = g.GetComponentInChildren<MicroFloor>(true);
            if (era == null) era = g.GetComponentInChildren<MicroEra>(true);
            if (card == null) card = g.GetComponentInChildren<HistoryCard>(true);
            sites.AddRange(g.GetComponentsInChildren<AreaFunctionSite>(true));
            if (heroT == null) foreach (CubeInsect ci in g.GetComponentsInChildren<CubeInsect>(true)) if (ci.name.StartsWith(hero)) { heroT = ci.transform; break; }
        }
        Transform root = floor != null ? floor.transform : null;

        TestProbe.Check($"su fecha es {year} y su región {region}", era != null && era.year == year && era.region == region, era != null ? $"{era.year}, {era.region}" : "sin MicroEra");
        TestProbe.Check($"{hero} está, como insecto-humano", heroT != null);
        if (TestProbe.NotNull("la historia (StoryMission)", m))
        {
            TestProbe.Check("tiene 8 pasos", m.steps.Length == 8, $"{m.steps.Length}");
            string bad = "";
            for (int i = 0; i < m.steps.Length; i++) if (m.steps[i].targets == null || m.steps[i].targets.Length == 0 || System.Array.Exists(m.steps[i].targets, x => x == null)) bad += $"paso {i + 1} sin objetivo; ";
            TestProbe.Check("cada paso tiene su objetivo", bad.Length == 0, bad);
            int start = m.Step; string adv = "";
            for (int i = start; i < m.steps.Length - 1; i++)
            {
                if (!m.CompleteCurrentForTest()) { adv += $"no avanza en el paso {i + 1}; "; break; }
                yield return null;
                if (m.Step != i + 1) { adv += $"paso {m.Step} en vez de {i + 1}; "; break; }
            }
            TestProbe.Check("los pasos avanzan", adv.Length == 0, adv.Length > 0 ? adv : $"llega al paso {m.Step + 1}/{m.steps.Length}");
        }
        TestProbe.Check("la ficha con su historia y ≥ 4 fuentes", card != null && card.history.Length >= 3 && card.sources.Length >= 4, card != null ? $"{card.history.Length} párrafos, {card.sources.Length} fuentes" : "sin HistoryCard");
        TestProbe.Check($"la ficha avisa «{NoLyricsNote}»", card != null && string.Join(" ", card.history).Contains(NoLyricsNote));
        TestProbe.Check("la ficha nombra el momento altruista, bello o de superación de la historia", card != null && string.Join(" ", card.history).Contains("El momento de esta historia"));

        // Las cuatro categorías, el pueblo entero y sus frutales.
        TestProbe.Check("el pueblo entero: ≥ 12 edificios", sites.Count >= 12, $"{sites.Count}");
        var have = new HashSet<string>(); foreach (AreaFunctionSite x in sites) if (!string.IsNullOrEmpty(x.activity)) have.Add(x.activity);
        string missing = ""; foreach (string c in AreaFunctionSite.TownCategories) if (!have.Contains(c)) missing += c + "; ";
        TestProbe.Check("trabajo, ocio, entrenamiento y recuperación", missing.Length == 0, missing);
        string noFruit = ""; int houses = 0; var kinds = new HashSet<string>();
        foreach (AreaFunctionSite x in sites)
        {
            if (x.GetComponentInChildren<Household>(true) == null) continue;
            houses++;
            MicroPiece fr = null; foreach (MicroPiece p in x.GetComponentsInChildren<MicroPiece>(true)) if (p.kind == "frutal") { fr = p; break; }
            if (fr == null) noFruit += x.title + "; "; else kinds.Add(fr.name);
        }
        TestProbe.Check("cada residencia con su frutal", houses > 0 && noFruit.Length == 0, noFruit.Length > 0 ? noFruit : $"{houses} casas: {string.Join(", ", kinds)}");
        bool local = false; foreach (string k in kinds) foreach (string f in fruits) if (k.Contains(f)) local = true;
        TestProbe.Check($"los frutales son los de su época y lugar ({string.Join(", ", fruits)})", local, string.Join(", ", kinds));

        if (scene == "Microcosmos_GrandesCanciones")
        {
            TestProbe.Check("la ficha dice que el sitio es inventado", card != null && string.Join(" ", card.history).Contains("inventado"));
            foreach (string corner in new[] { "Highlander", "mondine", "Tittenhurst", "Ginsberg", "Trench Town", "Andes" })
                TestProbe.Check($"el rincón «{corner}»", sites.Exists(x => x.title.Contains(corner)));
        }

        // Las reglas.
        if (root != null)
        {
            var rules = MicroBuildRules.Validate(root, scene, true, false);
            TestProbe.Check("cumple las reglas de edificación", rules.Count == 0, string.Join(" · ", rules.GetRange(0, Mathf.Min(8, rules.Count))));
            var floorRules = MicroFloors.Validate(root, scene, false);
            TestProbe.Check("cumple las reglas de los microcosmos", floorRules.Count == 0, string.Join(" · ", floorRules));
            TestProbe.Check(rep ? $"sobre su piso representativo ({platform})" : $"marcado «pendiente de piso representativo» (sobre {platform})",
                floor.platform == platform && floor.representative == rep && (rep || floor.pendingFloor.Length > 0), $"{floor.platform}: «{floor.pendingFloor}»");
        }
        else TestProbe.Check("tiene su MicroFloor (montado con la plantilla)", false);

        if (loadedHere)
        {
            AsyncOperation un = SceneManager.UnloadSceneAsync(s);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (un != null && !un.isDone && Time.realtimeSinceStartup < until) yield return null;
        }
        TestProbe.End();
    }
}
