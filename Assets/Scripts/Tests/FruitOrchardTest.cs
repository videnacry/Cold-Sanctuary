using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LOS FRUTALES DE LOS TECHOS (Berón 2026-10-30; docs/jardines-frutales.md):
/// 1. LLENOS Y CON SU PASO: cada techo de alfombras con frutales tiene UNA DE CADA especie de <see cref="FruitCare.All"/>
///    (o más), ninguna maceta a menos de 2 m de otra o de un equipo (<see cref="FurnitureSpacing.Violations"/>, 0 pares) y
///    todas a ≥ 2 m de las áreas de alfombras.
/// 2. SALA DE CUIDADOS: la de la experta en Servicios con sus equipos; un punto de cuidados en cada techo.
/// 3. ESTADO VISIBLE: cada maceta con sus tres barras (agua, abono, salud) y su etiqueta.
/// 4. BIENESTAR: sin cuidar, el agua y la salud bajan; tras la revisión (<see cref="FruitPlant.Care"/>) el bienestar sube.
/// 5. RONDAS: cada maceta en una sola ronda, cada ronda ≤ 3 h, y la jornada las tiene como misiones obligatorias.
/// 6. EL JARDINERO: un cuerpo de prueba hace la ronda que cruza más techos (a ×4): revisa todas sus macetas, vuela de un
///    techo al siguiente y termina; el bienestar medio de esas plantas sube. Se mide su tiempo frente al estimado.
/// Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class FruitOrchardTest : MonoBehaviour, ITestUnit
{
    public int Group => 55;   // tras el nado (54)
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<FruitOrchardTest>() == null) tr.gameObject.AddComponent<FruitOrchardTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Frutales de los techos: una de cada especie, 2 m de paso, sala de cuidados, bienestar y ronda del jardinero");
        FruitPlant.All.RemoveAll(p => p == null);
        var plants = FruitPlant.All.ToList();
        var byRoof = plants.GroupBy(p => p.building).ToDictionary(g => g.Key, g => g.ToList());
        TestProbe.Check("hay frutales en al menos 4 techos de alfombras", byRoof.Count >= 4, $"{byRoof.Count} techos, {plants.Count} macetas: {string.Join(", ", byRoof.Select(kv => $"{kv.Key} {kv.Value.Count}"))}");
        if (plants.Count == 0) { TestProbe.End(); yield break; }

        // ── 1. LLENOS Y CON SU PASO ──
        int kinds = FruitCare.All.Length;
        foreach (var kv in byRoof)
        {
            Transform roof = kv.Value[0].transform.parent != null ? kv.Value[0].transform.parent.parent : null;
            var have = new HashSet<GardenCrop.Kind>(kv.Value.Select(p => p.kind));
            var lack = FruitCare.All.Where(f => !have.Contains(f.kind)).Select(f => f.nombre).ToList();
            TestProbe.Check($"techo de {kv.Key}: una de cada una de las {kinds} frutales", lack.Count == 0, lack.Count == 0 ? $"{kv.Value.Count} macetas ({kv.Value.Count / (float)kinds:0.0} por especie)" : "faltan " + string.Join(", ", lack));
            if (roof == null) continue;
            var bad = FurnitureSpacing.Violations(roof);
            TestProbe.Check($"techo de {kv.Key}: 2 m de paso entre macetas, equipos y alfombras", bad.Count == 0, bad.Count == 0 ? "0 pares" : $"{bad.Count} pares: {FurnitureSpacing.Worst(bad)}");
            var pads = roof.GetComponentsInChildren<CarpetPad>().Select(c => { BoxCollider bc = c.GetComponent<BoxCollider>(); Vector3 e = c.transform.rotation * bc.size; return new Bounds(c.transform.TransformPoint(bc.center), new Vector3(Mathf.Abs(e.x), Mathf.Abs(e.y), Mathf.Abs(e.z))); }).ToList();
            float worst = float.MaxValue;
            foreach (FruitPlant p in kv.Value) { Bounds b = Box(p.transform); foreach (Bounds q in pads) worst = Mathf.Min(worst, Gap(b, q)); }
            bool tipRoof = roof.GetComponent<TreeFloor>() != null;   // las puntas de una torre-árbol no tienen alfombras (se llega nadando)
            TestProbe.Check($"techo de {kv.Key}: las macetas a ≥ 2 m de las áreas de alfombras", (tipRoof || pads.Count > 0) && worst >= 1.95f, $"la más cerca a {worst:0.00} m de {pads.Count} áreas");
            // RIEGO POR GOTEO (Berón 2026-10-31): cabezal en su punto, tubería por el techo y un gotero en cada maceta.
            int pipes = FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name == "Riego_goteo_" + kv.Key).Sum(net => net.GetComponentsInChildren<Renderer>().Count(r => r.name == "Tuberia_goteo"));   // (en una torre-árbol, uno por punta)
            int drips = kv.Value.Count(p => p.drip && p.transform.Find("Gotero") != null);
            bool headOk = FindObjectsByType<FruitCareStation>(FindObjectsSortMode.None).Any(s => s.building == kv.Key && s.transform.Find("Cabezal_riego") != null);
            TestProbe.Check($"techo de {kv.Key}: riego por goteo (cabezal, tubería y un gotero por maceta)", headOk && pipes >= kv.Value.Count && drips == kv.Value.Count, $"cabezal {(headOk ? "sí" : "no")}, {pipes} tramos, {drips}/{kv.Value.Count} goteros");
        }

        // ── 2. SALA DE CUIDADOS ──
        var stations = FindObjectsByType<FruitCareStation>(FindObjectsSortMode.None);
        FruitCareStation mainRoom = stations.FirstOrDefault(s => s.mainRoom);
        string[] needed = { "Banco_trasplante", "Deposito_fertirriego", "Armario_abonos", "Mesa_diagnostico", "Estante_herramientas", "Compostador", "Caja_abejorros", "Pulverizador_mochila", "Carro_cosecha", "Escalera_tijera", "Cabezal_riego" };
        var missing = mainRoom == null ? needed.ToList() : needed.Where(n => mainRoom.transform.Find(n) == null).ToList();
        TestProbe.Check("la sala de cuidados de la experta (Servicios) tiene todo su equipo", mainRoom != null && missing.Count == 0, mainRoom == null ? "no hay sala" : missing.Count == 0 ? $"{needed.Length} equipos en {mainRoom.building}" : "faltan " + string.Join(", ", missing));
        var noStation = byRoof.Keys.Where(b => !stations.Any(s => s.building == b)).ToList();
        TestProbe.Check("cada techo con frutales tiene su punto de cuidados", noStation.Count == 0, noStation.Count == 0 ? $"{stations.Length} puntos" : "sin punto: " + string.Join(", ", noStation));

        // ── 3. ESTADO VISIBLE ──
        yield return null;   // (las barras se montan en Start)
        int shown = plants.Count(p => p.transform.Find("Ficha_estado") is Transform f && f.Find("Agua") && f.Find("Abono") && f.Find("Salud") && p.GetComponent<WorldLabel>() is WorldLabel wl && wl.text.Contains("agua"));
        TestProbe.Check("cada maceta enseña su agua, abono y salud (barras y etiqueta)", shown == plants.Count, $"{shown}/{plants.Count}");

        // ── 4. BIENESTAR ──
        FruitPlant probe = plants[0];
        float w0 = probe.water, h0 = probe.health;
        probe.water = 0.3f; probe.nutrients = 0.25f; probe.pests = 0.5f; probe.health = 0.5f;
        float n0 = probe.nutrients;
        probe.Tick(1f);   // un día sin revisar
        TestProbe.Check($"sin revisar un día, el goteo le da el agua al {probe.Name}, pero pierde abono y salud", probe.drip && probe.water >= 0.6f && probe.nutrients < n0 && probe.health < 0.5f, $"agua {probe.water:0.00}, abono {probe.nutrients:0.00}, salud {probe.health:0.00}");
        var (before, after) = probe.Care("prueba");
        TestProbe.Check($"tras la revisión diaria, el bienestar del {probe.Name} sube", after > before + 0.15f && probe.nutrients > 0.3f, $"{before * 100f:0}% → {after * 100f:0}%");
        float hc = probe.health; probe.Tick(1f);
        TestProbe.Check("cuidada, al día siguiente su salud sigue subiendo", probe.health > hc, $"salud {hc:0.00} → {probe.health:0.00}");

        // ── 5. RONDAS ──
        var rounds = FruitGardener.Rounds();
        var inRounds = rounds.SelectMany(r => r.Plants).ToList();
        TestProbe.Check("cada maceta está en una sola ronda", inRounds.Count == plants.Count && inRounds.Distinct().Count() == plants.Count, $"{inRounds.Count} en rondas de {plants.Count}");
        TestProbe.Check("con el goteo, la ronda la hacen unas 3 personas al día", rounds.Count <= 3, $"{rounds.Count} rondas, total {rounds.Sum(r => r.hours):0.0} h de juego");
        TestProbe.Check($"ninguna ronda pasa de {FruitGardener.MaxRoundHours:0} h (cabe en el bloque de la mañana)", rounds.All(r => r.hours <= FruitGardener.MaxRoundHours + 0.01f),
            $"{rounds.Count} rondas: " + string.Join(" · ", rounds.Select(r => $"{r.hours:0.0} h")) + $"; total {rounds.Sum(r => r.hours):0.0} h → {rounds.Count} persona(s)");
        var today = AreaWork.Today(0, ConstructionProject.All).Where(m => FruitGardener.IsRound(m.id)).ToList();
        TestProbe.Check("la jornada tiene cada ronda como misión obligatoria que recorre los edificios", today.Count == rounds.Count && today.All(m => m.mandatory && m.roams && Mathf.Approximately(m.hours, FruitGardener.MaxRoundHours)),
            string.Join("; ", today.Select(m => $"{m.id} {m.hours:0.#} h")));

        // ── 6. EL JARDINERO ──
        FruitGardener.Round round = rounds.OrderByDescending(r => r.legs.Select(l => l.building).Distinct().Count()).ThenBy(r => r.hours).First();
        var rp = round.Plants.ToList();
        foreach (FruitPlant p in rp) { p.water = 0.3f; p.nutrients = 0.3f; p.pests = 0.4f; p.health = 0.55f; }
        float wellBefore = rp.Average(p => p.Wellbeing);
        GameObject body = WorkdayDirector.SpawnBodyCopy("Irosene", "Jardinera (prueba)");
        Vector3? start = FruitGardener.StartPoint(round.id);
        if (start.HasValue) WorkdayDirector.Teleport(body.transform, start.Value);
        FruitGardener g = body.AddComponent<FruitGardener>();
        float oldScale = Time.timeScale; Time.timeScale = 4f;
        g.Assign(round.id);
        float estS = round.hours * TimeController.timeController.SecondsPerGameDay / 24f;
        float cap = Mathf.Min(900f, estS / Time.timeScale * 2.5f + 90f), t0 = Time.realtimeSinceStartup;
        while (!g.Finished && Time.realtimeSinceStartup - t0 < cap) yield return null;   // con tope
        Time.timeScale = oldScale;
        int roofsIn = round.legs.Select(l => l.building).Distinct().Count();
        int caredHere = g.Cared;   // (no lastCaredBy: si la jornada corre, el jardinero de verdad de esa ronda también las cuida)
        TestProbe.Check($"el jardinero termina «{round.id}» ({rp.Count} macetas, {roofsIn} techos)", g.Finished, g.Finished ? g.Doing : $"a los {cap:0} s reales va por «{g.Doing}» ({g.Cared}/{rp.Count})");
        TestProbe.Check("revisa todas las macetas de su ronda (salta ≤ 5 %)", caredHere >= Mathf.CeilToInt(rp.Count * 0.95f), $"{caredHere}/{rp.Count} cuidadas, {g.Skipped} saltadas");
        TestProbe.Check("al acabar un techo pasa al siguiente (en alfombra)", g.roofsDone.Count == roofsIn, $"techos: {string.Join(" → ", g.roofsDone)}");
        float wellAfter = rp.Average(p => p.Wellbeing);
        TestProbe.Check("el bienestar medio de sus plantas sube tras la ronda", wellAfter > wellBefore + 0.15f, $"{wellBefore * 100f:0}% → {wellAfter * 100f:0}%");
        float gameH = g.RoundSeconds / (TimeController.timeController.SecondsPerGameDay / 24f);
        TestProbe.Check("la ronda medida cabe en las horas de su misión", g.Finished && gameH <= FruitGardener.MaxRoundHours + 0.25f, $"medida {gameH:0.00} h de juego (revisión estimada {round.hours:0.00} h; el resto, tareas de la semana)");
        TestProbe.Check("con lo que sobra de las 3 h, poliniza y poda (tareas de la semana)", g.Pollinated + g.Pruned > 0, $"{g.Pollinated} polinizadas, {g.Pruned} podadas");
        Debug.Log($"[Frutales] Medida: «{round.id}» {rp.Count} macetas en {roofsIn} techos: {g.RoundSeconds:0} s de juego = {gameH:0.00} h (estimada {round.hours:0.00} h); total del día {rounds.Sum(r => r.hours):0.0} h → {rounds.Count} persona(s).");
        Destroy(body);
        probe.water = Mathf.Max(probe.water, w0); probe.health = Mathf.Max(probe.health, h0);
        TestProbe.End();
    }

    static Bounds Box(Transform t)
    {
        Bounds b = new Bounds(t.position, Vector3.zero); bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return b;
    }

    static float Gap(Bounds a, Bounds b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x));
        float dz = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
