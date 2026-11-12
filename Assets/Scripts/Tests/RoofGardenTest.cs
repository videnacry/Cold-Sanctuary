using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LOS JARDINES EN LA CIMA (Berón 2026-11-03; docs/jardines-frutales.md §Jardines en la cima):
/// 1. cada edificio-árbol tiene en su techo de alfombras al menos un BANCAL DE TIERRA con frutales;
/// 2. cada bancal guarda sus 2 m con todo lo del techo (en el marco del techo: el del Árbol de los Campos gira), está a ≥ 2 m de
///    las áreas de alfombras y fuera del umbral de la puerta del techo;
/// 3. cada bancal es un GREMIO: sus frutales comparten pH; lleva compañeras, flores de néctar, hotel de insectos, caja de abejorros,
///    caja nido, vermicompostador, el corte de sus capas y su goteo (un gotero por frutal);
/// 4. TIERRA FRENTE A MACETA: la misma especie, en las mismas condiciones y 3 días sin revisar, está mejor en el bancal (menos
///    plaga, más abono) — lo que dicen las fuentes del doc;
/// 5. la RONDA: los frutales de la cima están en la ronda de su edificio y van PRIMERO (se llega al techo en alfombra); ninguna
///    ronda pasa de 3 h;
/// 6. un jardinero de prueba cuida los bancales de una cima (a ×4) y su bienestar sube;
/// 7. el peso del sustrato saturado es despreciable para el tronco (&lt; 1 % de lo que ya carga).
/// </summary>
public class RoofGardenTest : MonoBehaviour, ITestUnit
{
    public int Group => 80;   // tras todo lo demás (los frutales, 55; los árboles, 53 y 57; los grupos 60–72 de otros hilos)
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<RoofGardenTest>() == null) tr.gameObject.AddComponent<RoofGardenTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Jardines en la cima de los edificios-árbol: bancales de tierra por gremios, 2 m, insectos útiles y ronda");
        GardenBed.All.RemoveAll(b => b == null);
        var trees = ArbolCampos.All.Where(a => a != null && a.top != null).ToList();
        TestProbe.Check("hay edificios-árbol con su techo", trees.Count >= 4, $"{trees.Count} árboles");
        foreach (ArbolCampos ac in trees)
        {
            var beds = ac.top.GetComponentsInChildren<GardenBed>().ToList();
            int fruits = beds.Sum(b => b.Plants.Count);
            TestProbe.Check($"cima de {ac.building}: bancales de tierra con frutales", beds.Count >= 1 && fruits >= 2, $"{beds.Count} bancales, {fruits} frutales ({string.Join(", ", beds.Select(b => b.guild))})");
            if (beds.Count == 0) continue;

            // ── 2. EL PASO ──
            var items = new List<(Transform t, Bounds b, float need, bool bed)>();
            foreach (Transform t in ac.top.GetComponentsInChildren<Transform>())
            {
                if (t == ac.top) continue;
                if (t.GetComponent<GardenBed>() != null) { items.Add((t, LocalBox(ac.top, t), FurnitureSpacing.Standard, true)); continue; }
                if (t.GetComponent<CarpetPad>() is CarpetPad cp && cp.GetComponent<BoxCollider>() is BoxCollider bc)
                { items.Add((t, Wrap(ac.top.worldToLocalMatrix * t.localToWorldMatrix, new Bounds(bc.center, bc.size)), 2f, false)); continue; }
                if (t.parent == ac.top && t.GetComponent<GardenBed>() == null && !t.name.StartsWith("Jardin_cima") && !t.name.StartsWith("Losa") && t.GetComponentInChildren<Renderer>() != null)
                    items.Add((t, LocalBox(ac.top, t), FurnitureSpacing.Standard, false));
            }
            float worst = float.MaxValue; string worstPair = "";
            foreach (var a in items.Where(i => i.bed))
                foreach (var o in items)
                {
                    if (o.t == a.t) continue;
                    if (o.b.min.y > a.b.max.y + 0.5f || o.b.max.y < 0.05f) continue;   // (lo que va por encima de las cabezas: paneles a 3 m)
                    float gap = Gap(a.b, o.b) - o.need;
                    if (gap < worst) { worst = gap; worstPair = $"«{a.t.name}»–«{o.t.name}» {Gap(a.b, o.b):0.00}/{o.need:0.0} m"; }
                }
            TestProbe.Check($"cima de {ac.building}: los bancales guardan 2 m con todo (y con las alfombras)", worst >= -0.01f, worst == float.MaxValue ? "nada al lado" : $"el más justo: {worstPair}");
            TreeLevel top = ac.Top;
            if (top != null)
            {
                Vector3 door = ac.top.InverseTransformPoint(ac.World(top.door)), outside = ac.top.InverseTransformPoint(ac.World(top.outside));
                bool clear = true;
                for (float f = 0f; f <= 1.01f; f += 0.1f)
                {
                    Vector3 p = Vector3.Lerp(door, outside, f);
                    foreach (GardenBed b in beds) if (LocalBox(ac.top, b.transform).Contains(new Vector3(p.x, 0.4f, p.z))) clear = false;
                }
                TestProbe.Check($"cima de {ac.building}: el umbral de la puerta del techo queda libre", clear, $"puerta ({door.x:0.0}, {door.z:0.0}) → fuera ({outside.x:0.0}, {outside.z:0.0})");
            }

            // ── 3. EL GREMIO ──
            foreach (GardenBed b in beds)
            {
                bool ph = b.PhCompatible(out string why);
                string[] parts = { "Hotel_insectos", "Caja_abejorros", "Caja_nido_carbonero", "Vermicompostador", "Corte_capas", "Cabezal_riego" };
                var lack = parts.Where(p => b.transform.Find(p) == null).ToList();
                int flowers = b.GetComponentsInChildren<Renderer>().Count(r => r.name == "Flor_nectar");
                int crops = b.GetComponentsInChildren<GardenCrop>().Length - b.Plants.Count;
                int drips = b.Plants.Count(p => p.transform.Find("Gotero") != null && p.drip);
                TestProbe.Check($"{b.name} ({ac.building}): gremio con pH común, compañeras, flores, insectos útiles, animales y goteo",
                    ph && lack.Count == 0 && flowers >= 6 && crops >= 2 && drips == b.Plants.Count && b.beneficials.Length >= 2 && b.animals.Length >= 1,
                    $"{why}; {crops} compañeras, {flores(flowers)}; {(lack.Count == 0 ? "con hotel, abejorros, caja nido, lombrices, corte y cabezal" : "faltan " + string.Join(", ", lack))}; {drips}/{b.Plants.Count} goteros");
            }
        }

        // ── 4. TIERRA FRENTE A MACETA ──
        GardenBed anyBed = GardenBed.All.FirstOrDefault(b => b != null && b.Plants.Count > 0);
        if (anyBed != null)
        {
            FruitPlant inBed = anyBed.Plants[0];
            var go = new GameObject("Maceta_de_control"); go.SetActive(false);
            FruitPlant pot = go.AddComponent<FruitPlant>(); pot.kind = inBed.kind; pot.drip = true;
            var saved = (inBed.water, inBed.nutrients, inBed.pests, inBed.health, inBed.fruitSet);
            foreach (FruitPlant p in new[] { inBed, pot }) { p.water = 0.6f; p.nutrients = 0.6f; p.pests = 0.2f; p.health = 0.7f; p.fruitSet = 0.6f; }
            for (int d = 0; d < 3; d++) { inBed.Tick(1f); pot.Tick(1f); }
            TestProbe.Check($"tierra frente a maceta: el {inBed.Name} del bancal está mejor tras 3 días sin revisar", inBed.Wellbeing > pot.Wellbeing + 0.03f && inBed.pests < pot.pests && inBed.nutrients > pot.nutrients,
                $"bancal: bienestar {inBed.Wellbeing * 100f:0} %, plaga {inBed.pests:0.00}, abono {inBed.nutrients:0.00} · maceta: {pot.Wellbeing * 100f:0} %, plaga {pot.pests:0.00}, abono {pot.nutrients:0.00}");
            (inBed.water, inBed.nutrients, inBed.pests, inBed.health, inBed.fruitSet) = saved;
            Destroy(go);
        }

        // ── 5. LA RONDA ──
        var rounds = FruitGardener.Rounds();
        var bedPlants = FruitPlant.All.Where(p => p != null && p.bed != null).ToList();
        var inRounds = new HashSet<FruitPlant>(rounds.SelectMany(r => r.Plants));
        TestProbe.Check("los frutales de los bancales están en las rondas del jardinero", bedPlants.Count > 0 && bedPlants.All(inRounds.Contains), $"{bedPlants.Count(inRounds.Contains)}/{bedPlants.Count}");
        var legs = rounds.SelectMany(r => r.legs).Where(l => l.plants.Any(p => p.bed != null)).ToList();
        int firstRoof = legs.Count(l => l.plants[0].bed != null);
        TestProbe.Check("en cada edificio, la cima va primero (se llega al techo en alfombra)", firstRoof == legs.Count, $"{firstRoof}/{legs.Count} tramos empiezan por los bancales");
        TestProbe.Check("ninguna ronda pasa de su tope con los bancales", rounds.All(r => r.hours <= FruitGardener.MaxRoundHours + 0.01f), string.Join(" · ", rounds.Select(r => $"{r.id} {r.hours:0.00} h")));

        // ── 6. UN JARDINERO EN LA CIMA ──
        GardenBed bed0 = GardenBed.All.Where(b => b != null && b.Plants.Count > 0).OrderBy(b => b.Plants.Count).FirstOrDefault();
        if (bed0 != null)
        {
            var targets = bed0.Plants;
            foreach (FruitPlant p in targets) { p.nutrients = 0.3f; p.pests = 0.5f; p.health = 0.55f; }
            float before = targets.Average(p => p.Wellbeing);
            GameObject body = WorkdayDirector.SpawnBodyCopy("Shalva", "Jardinero de la cima (prueba)");
            WorkdayDirector.Teleport(body.transform, targets[0].StandPoint(targets[0].transform.position + bed0.transform.forward * 3f));
            float old = Time.timeScale; Time.timeScale = 4f;
            int cared = 0; float t0 = Time.realtimeSinceStartup;
            foreach (FruitPlant p in targets)
            {
                Vector3 to = p.StandPoint(body.transform.position);
                float t1 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - t1 < 40f)
                {
                    PathMover.Of(body.transform).StepToward(to, 2.6f);
                    Vector3 flat = to - body.transform.position; flat.y = 0f;
                    if (flat.magnitude < 1.2f) break;
                    yield return null;
                }
                Vector3 f2 = to - body.transform.position; f2.y = 0f;
                if (f2.magnitude < 1.5f) { p.Care(body.name); cared++; }
            }
            Time.timeScale = old;
            float after = targets.Average(p => p.Wellbeing);
            TestProbe.Check($"un jardinero llega andando a los frutales de «{bed0.name}» ({bed0.building}) y los cuida", cared == targets.Count && after > before + 0.1f,
                $"{cared}/{targets.Count} cuidados en {Time.realtimeSinceStartup - t0:0} s; bienestar {before * 100f:0} % → {after * 100f:0} %");
            Destroy(body);
        }

        // ── 7. EL PESO ──
        float tonnes = GardenBed.All.Where(b => b != null).Sum(b => b.SaturatedTonnes);
        float perTree = trees.Count > 0 ? tonnes / trees.Count : 0f;
        TestProbe.Check("el sustrato saturado pesa poco para el tronco (< 1 % de los ~174 MN que ya carga)", perTree * 9.81f / 1000f < 1.74f, $"{tonnes:0} t en total, {perTree:0} t por árbol = {perTree * 9.81f / 1000f:0.00} MN");
        TestProbe.End();
        yield break;
    }

    static string flores(int n) => $"{n} flores de néctar";


    static Bounds LocalBox(Transform frame, Transform t)
    {
        Bounds b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r.GetComponent<TMPro.TMP_Text>() != null) continue;
            Bounds rb = Wrap(frame.worldToLocalMatrix * r.transform.localToWorldMatrix, r.localBounds);
            if (!any) { b = rb; any = true; } else b.Encapsulate(rb);
        }
        return b;
    }

    static Bounds Wrap(Matrix4x4 m, Bounds b)
    {
        Bounds o = new Bounds(m.MultiplyPoint3x4(b.center), Vector3.zero);
        for (int i = 0; i < 8; i++)
            o.Encapsulate(m.MultiplyPoint3x4(b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f))));
        return o;
    }

    static float Gap(Bounds a, Bounds b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x));
        float dz = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
