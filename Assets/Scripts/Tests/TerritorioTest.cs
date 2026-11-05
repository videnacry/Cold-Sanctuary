using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// PRUEBA DEL TERRITORIO POLAR (Berón 2026-11-05; docs/territorio-polar.md, docs/reglas-animales.md):
/// 1. CATÁLOGOS: cada especie nueva está en todas sus tablas (una que falta cae en silencio a un valor por defecto), con su plan de cuerpo.
/// 2. ESPECIES: viven en el santuario, a su tamaño real, en su sitio (la isla, la laguna, la colonia).
/// 3. CICLO: las tablas dicen lo que su ficha (gestación, huevo, lactancia, guarida); el ciclo entero corre en su escena (grupo 58).
/// 4. ISLA: tierra e hielo en torno a mitad y mitad; de un extremo a otro de la isla se anda, y se llega desde el resto a nado o en alfombra.
/// 5. FLORA: en la isla crecen plantas, y todas sobre el suelo: ni enterradas ni en el aire.
/// 6. CUEVAS: se entra andando, tienen techo y un adulto grande cabe.
/// 7. HÁBITATS: barreras, acantilados, colonias, playa de morsas y bivalvos.
/// Solo en SampleScene (sin Territorio_AUTO, SKIP).
/// </summary>
public class TerritorioTest : MonoBehaviour, ITestUnit
{
    public int Group => 99;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<TerritorioTest>() == null) tr.gameObject.AddComponent<TerritorioTest>();
    }

    // Especie, largo real (m), tolerancia y dónde vive: 0 tierra, 1 laguna, 2 hielo.
    static readonly (string sp, float size, int where)[] New =
    {
        ("Walrus", 3.0f, 1), ("EmperorPenguin", 1.0f, 2), ("Penguin", 0.6f, 2), ("Reindeer", 2.0f, 0), ("Wolf", 1.4f, 0), ("ArcticHare", 0.62f, 0), ("ArcticFox", 0.85f, 0), ("MuskOx", 2.3f, 0),
    };

    static float BodyLength(Animal a)
    {
        bool any = false; Bounds b = default; Matrix4x4 m0 = a.transform.worldToLocalMatrix;
        foreach (Renderer r in a.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r.GetComponent<WorldLabel>() != null || !r.TryGetComponent(out MeshFilter mf) || mf.sharedMesh == null) continue;
            Matrix4x4 m = m0 * r.localToWorldMatrix; Bounds lb = mf.sharedMesh.bounds;
            for (int c = 0; c < 8; c++)
            {
                Vector3 p = m.MultiplyPoint3x4(lb.center + Vector3.Scale(lb.extents, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1)));
                if (!any) { b = new Bounds(p, Vector3.zero); any = true; } else b.Encapsulate(p);
            }
        }
        return any ? Mathf.Max(b.size.x, b.size.y, b.size.z) * a.transform.lossyScale.x : 0f;
    }

    static bool WalkPath(Vector3 from, Vector3 to, out string why)
    {
        why = "";
        if (!NavMesh.SamplePosition(from, out NavMeshHit a, 8f, NavMesh.AllAreas)) { why = $"sin NavMesh en {from}"; return false; }
        if (!NavMesh.SamplePosition(to, out NavMeshHit b, 8f, NavMesh.AllAreas)) { why = $"sin NavMesh en {to}"; return false; }
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) { why = $"camino {path.status} de {a.position} a {b.position}"; return false; }
        return true;
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Territorio polar: especies, ciclo, isla de tierra, flora, cuevas y hábitats");
        Territorio ter = FindAnyObjectByType<Territorio>();
        if (ter == null) { Debug.Log("[TEST] SKIP · Territorio — esta escena no es el santuario"); TestProbe.End(); yield break; }

        // ── 1. CATÁLOGOS ──
        foreach (var (sp, _, _) in New)
        {
            bool ok = SpeciesProfile.Of(sp) != SpeciesProfile.Default && Physiognomy.Of(sp) != null && StageProfile.Of(sp) != StageProfile.Of("no-existe") && LifeHistory.Has(sp)
                && PopulationPlan.PerHuman.ContainsKey(sp) && SpeciesScaleFix.Has(sp) && ActionsPrep.Of(sp) != null && Reproduction.Calendar(sp).gestationMonths != Reproduction.Calendar("no-existe").gestationMonths;
            TestProbe.Check($"{sp}: en todas sus tablas", ok);
        }
        TestProbe.Check("cada especie nueva tiene su plan de cuerpo (morsa: pinnípedo; emperador: ave; reno: cuadrúpedo)",
            CubeCreature.SpecFor("Walrus").plan == CubeCreature.Plan.Pinniped && CubeCreature.SpecFor("EmperorPenguin").plan == CubeCreature.Plan.Bird && CubeCreature.SpecFor("Reindeer").antlers && CubeCreature.SpecFor("MuskOx").horns);
        TestProbe.Check("cada una tiene su escena de ciclo de vida", New.All(n => CicloVidaPanel.Species.Any(s => s.species == n.sp)));

        // ── 2. ESPECIES ──
        var animals = FindObjectsByType<Animal>(FindObjectsSortMode.None).Where(a => a != null && !a.death && Mathf.Abs(a.transform.position.x) < 250f && Mathf.Abs(a.transform.position.z) < 250f).ToList();
        yield return new WaitForSeconds(3f);   // SpeciesScaleFix corrige a cada segundo
        foreach (var (sp, size, where) in New)
        {
            Animal[] all = animals.Where(a => a != null && a.SpeciesName == sp).ToArray();
            if (!TestProbe.Check($"{sp}: vive en el santuario", all.Length >= 2, $"{all.Length} individuos")) continue;
            Animal adult = all.FirstOrDefault(a => a.lifeStage == LifeStage.adult) ?? all[0];
            float want = size * (adult.lifeStage == LifeStage.child ? 0.4f : adult.lifeStage == LifeStage.teen ? 0.7f : 1f);
            TestProbe.Near($"{sp}: tamaño real ({adult.lifeStage})", BodyLength(adult), want, want * 0.2f);
            // (basta uno: hay lobos de prueba en el bosque oeste y un pingüino puede estar nadando)
            bool Home(Animal a)
            {
                Vector3 p = a.transform.position;
                if (where == 1) return Vector2.Distance(new Vector2(p.x, p.z), new Vector2(MarineLife.Lagoon.x, MarineLife.Lagoon.z)) < 60f;
                bool hit = Physics.Raycast(p + Vector3.up * 3f, Vector3.down, out RaycastHit h, 8f, ~0, QueryTriggerInteraction.Ignore);
                return hit && (where == 0) == Territorio.IsLand(h.collider);
            }
            TestProbe.Check($"{sp}: en su hábitat", all.Any(Home), $"primero en ({all[0].transform.position.x:0}, {all[0].transform.position.z:0})");
        }

        // ── 3. CICLO: los datos de su ficha ──
        LifeHistory emp = LifeHistory.Of("EmperorPenguin"), ade = LifeHistory.Of("Penguin"), wal = LifeHistory.Of("Walrus");
        TestProbe.Check("emperador: un huevo sobre las patas y el macho lo incuba ~2 meses", emp.eggs && emp.eggOnFeet && emp.incubator == LifeHistory.Incubator.Male && Mathf.Abs(emp.incubationMonths - 2.1f) < 0.2f);
        TestProbe.Check("Adelia: nido de guijarros, ~34 días", ade.eggs && ade.eggNest && !ade.eggOnFeet && Mathf.Abs(ade.incubationMonths - 1.15f) < 0.15f);
        TestProbe.Check("morsa: ~15 meses de gestación y una cría cada ~2,6 años", Mathf.Abs(Reproduction.Calendar("Walrus").gestationMonths - 15f) < 1f && Reproduction.Calendar("Walrus").intervalMonths >= 24f && wal.care == LifeHistory.Care.Lactation);
        TestProbe.Check("la morsa come bivalvos y hay de dónde", animals.Where(a => a != null && a.SpeciesName == "Walrus").All(a => a.Forage != null && a.Forage.eatsClams) && Swarm.Nearest(MarineLife.Lagoon, SwarmKind.Clam) != null);
        TestProbe.Check("reno, buey y liebre tienen su planta en la isla", Affordance.Of(AffordanceKind.Lichen).Count > 0 && Affordance.Of(AffordanceKind.Willow).Count > 0 && Affordance.Of(AffordanceKind.Grass).Count > 0
            && Ethogram.Eats("Reindeer", AffordanceKind.Lichen) && Ethogram.Eats("MuskOx", AffordanceKind.Grass) && Ethogram.Eats("ArcticHare", AffordanceKind.Willow));
        TestProbe.Check("guaridas: la del lobo y la del zorro están en la isla", Den.All.Any(d => d.species == "Wolf") && Den.All.Any(d => d.species == "ArcticFox"));

        // ── 4. ISLA ──
        float share = ter.landArea / Mathf.Max(1f, ter.landArea + ter.iceArea);
        TestProbe.Check("en torno a mitad de tierra y mitad de hielo", share > 0.4f && share < 0.6f, $"{share:P0} tierra ({ter.landArea:0} m² y {ter.iceArea:0} m²)");
        TestProbe.Check("la isla tiene sus lóbulos apuntados", ter.landPoints.Length >= 6, $"{ter.landPoints.Length} puntos");
        for (int i = 1; i < ter.landPoints.Length; i++)
            TestProbe.Check($"la isla se anda: tramo {i}", WalkPath(ter.landPoints[0], ter.landPoints[i], out string why), why);
        // Del hub a la isla: por los enlaces de nado de siempre.
        string why2 = "sin puntos";
        bool fromHub = ter.landPoints.Length > 0 && WalkPath(new Vector3(5f, 0f, -10f), ter.landPoints[0], out why2);
        TestProbe.Check("del hub a la isla se llega (a nado por los enlaces)", fromHub, why2);

        // ── 5. FLORA ──
        PolarUnderstory flora = FindAnyObjectByType<PolarUnderstory>();
        TestProbe.Check("hay tundra en la isla", flora != null && flora.TundraPlants > 200, $"{(flora != null ? flora.TundraPlants : 0)} plantas");
        int spots = 0, bad = 0; string worst = "";
        if (flora != null)
        {
            foreach (Transform t in flora.transform)
            {
                if (!t.name.StartsWith("Planta_")) continue;
                spots++;
                if (!Physics.Raycast(t.position + Vector3.up * 4f, Vector3.down, out RaycastHit h, 6f, ~0, QueryTriggerInteraction.Ignore)) { bad++; worst = $"{t.name}@{t.position}: sin suelo"; continue; }
                if (Mathf.Abs(h.point.y - t.position.y) > 0.12f) { bad++; worst = $"{t.name}@{t.position}: suelo a {h.point.y:0.00}"; }
            }
            bool meshesOk = flora.GetComponentsInChildren<MeshRenderer>().All(r => r.bounds.min.y > -0.1f);
            TestProbe.Check("ninguna mata bajo el suelo", meshesOk);
        }
        TestProbe.Check("cada planta comestible está sobre su suelo", spots > 50 && bad == 0, $"{spots} puntos, {bad} mal {worst}");

        // ── 6. CUEVAS ──
        TestProbe.Check("hay cuevas de hielo y de roca", ter.caves.Length >= 3 && ter.caves.Any(c => c != null && c.name.Contains("roca")) && ter.caves.Any(c => c != null && c.name.Contains("hielo")), $"{ter.caves.Length}");
        foreach (Transform c in ter.caves)
        {
            if (c == null) continue;
            Transform boca = c.Find("Boca"), fondo = c.Find("Fondo");
            if (!TestProbe.Check($"{c.name}: tiene boca y fondo", boca != null && fondo != null)) continue;
            TestProbe.Check($"{c.name}: se entra andando", WalkPath(boca.position, fondo.position, out string why3), why3);
            TestProbe.Check($"{c.name}: el fondo tiene techo a más de 2,5 m", Physics.Raycast(fondo.position + Vector3.up * 0.5f, Vector3.up, out RaycastHit roof, 6f, ~0, QueryTriggerInteraction.Ignore) && roof.distance + 0.5f > 2.5f);
            TestProbe.Check($"{c.name}: cabe un oso (nada a 1,2 m del eje)", !Physics.Raycast(boca.position + Vector3.up + c.forward * 2f, -c.right, 1.2f) && !Physics.Raycast(boca.position + Vector3.up + c.forward * 2f, c.right, 1.2f));
        }

        // ── 7. HÁBITATS ──
        TestProbe.Check("barreras y acantilados", ter.cliffs >= 20, $"{ter.cliffs} bloques");
        TestProbe.Check("colonia del emperador y playa de morsas", GameObject.Find("Colonia_emperador") != null && GameObject.Find("Playa_morsas") != null);
        TestProbe.Check("charcas para beber en la isla", WaterSource.All.Count(w => w != null && w.transform.position.z < -100f) >= 3);
        TestProbe.End();
    }
}
