using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA FACHADA DE FOTOBIORREACTORES DEL ÁRBOL (Berón 2026-10-31; docs/fotobiorreactores.md):
/// 1. EXISTE: la fachada cuelga del árbol con sus módulos de tubos (tronco), sus quillas (ramas), sus LED, bombas, cuadro y etiqueta.
/// 2. LIBRE: ningún módulo cerca de una puerta, una boca de rama o el carril de nado; el tronco queda transparente abajo
///    (0–37,6 m) y arriba (56–92 m) y cubierto poco en cada cara; las quillas, bajo su tubo y por encima del árbol más alto.
/// 3. NÚMEROS: productividad, O₂ por gramo, m² por persona, productividad por litro, luz de los LED y su potencia dentro de lo
///    investigado; el O₂ cubre a la gente del santuario; el tronco aguanta el peso.
/// 4. CUIDADO: sin cuidar, el cultivo empeora; el cuidado diario (<see cref="FotobioFacade.Care"/>) sube la salud, cosecha y
///    repone el medio; la jornada tiene la misión obligatoria de 1 h en Biología.
/// 5. ESPACIO: 2 m con las plataformas de arriba y de abajo de cada quilla, con las alfombras, y nada fuera del árbol en el
///    círculo que barre la fachada al girar.
/// Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class FotobioFacadeTest : MonoBehaviour, ITestUnit
{
    public int Group => 56;   // tras los frutales (55)
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<FotobioFacadeTest>() == null) tr.gameObject.AddComponent<FotobioFacadeTest>();
    }

    static Rect DoorRect(HelixTower h, float y) => new Rect(h.doorX - h.doorWidth * 0.5f, y, h.doorWidth, h.doorHeight);

    public IEnumerator Run()
    {
        TestProbe.Begin("Fachada de fotobiorreactores del Árbol: existe, libre de puertas y nado, O₂ investigado, cuidado diario y espacio");
        FotobioFacade fb = FotobioFacade.Instance != null ? FotobioFacade.Instance : FindAnyObjectByType<FotobioFacade>();
        if (fb == null) { TestProbe.Check("hay una fachada de fotobiorreactores en la escena", false); TestProbe.End(); yield break; }
        Transform root = fb.transform, tree = root.parent;
        HelixTower h = TreeSpec.Helix;

        // ── 1. EXISTE ──
        TestProbe.Check("cuelga del Árbol de los Campos (gira con él)", tree != null && tree.name == TreeSpec.RootName, tree != null ? tree.name : "sin padre");
        int wantModules = 4 * FotobioSpec.TrunkBays.Length * FotobioSpec.ModulesPerColumn, wantPanels = 4 * FotobioSpec.KeelPanels;
        TestProbe.Check("módulos de tubos en el tronco y paneles en las 4 ramas", fb.trunkModules == wantModules && fb.keelPanels == wantPanels, $"{fb.trunkModules}/{wantModules} módulos, {fb.keelPanels}/{wantPanels} paneles");
        TestProbe.Near("superficie de cultivo (m²)", fb.areaM2, FotobioSpec.Area(), 1f);
        string[] parts = { "Fotobio_cultivo", "Fotobio_marco", "Fotobio_LED", "Fotobio_bombas", "Cuadro_fotobio" };
        var missing = parts.Where(p => root.Find(p) == null).ToList();
        TestProbe.Check("tubos de cultivo, colectores y marco, LED, bombas y cuadro", missing.Count == 0, missing.Count == 0 ? "todo" : "faltan " + string.Join(", ", missing));
        yield return null; yield return new WaitForSeconds(1.2f);   // (la etiqueta se refresca cada segundo)
        WorldLabel wl = root.Find("Cuadro_fotobio") != null ? root.Find("Cuadro_fotobio").GetComponent<WorldLabel>() : null;
        TestProbe.Check("el cuadro enseña densidad, salud y O₂", wl != null && wl.text.Contains("densidad") && wl.text.Contains("salud") && wl.text.Contains("O₂"), wl != null ? wl.text : "sin etiqueta");

        // ── 2. LIBRE DE PUERTAS, BOCAS, CARRIL DE NADO; EL TRONCO SE VE ──
        List<Rect> mods = FotobioSpec.TrunkModules(h);
        float worstDoor = float.MaxValue;
        for (int s = 0; s < 4; s++)
        {
            var gaps = new List<Rect>();
            if (s == TreeSpec.GroundSide) gaps.Add(DoorRect(h, 0f));
            for (int lv = 1; lv <= h.levels; lv++)
                gaps.Add(HelixTower.HasFloor(lv, s) ? DoorRect(h, h.LevelY(lv)) : new Rect(h.mouthX - h.tubeHalf, h.LevelY(lv) - 0.3f - h.tubeWall, 2f * h.tubeHalf, h.TubeInnerH + 2f * h.tubeWall));
            foreach (Rect m in mods) foreach (Rect g in gaps) worstDoor = Mathf.Min(worstDoor, RectGap(m, g));
        }
        TestProbe.Check("ningún módulo a menos de 2 m de una puerta o de la boca de una rama", worstDoor >= 2f, $"el más cerca a {worstDoor:0.0} m");
        float laneX0 = h.doorX - h.doorWidth * 0.5f - 1f, laneX1 = h.mouthX + h.tubeHalf + 1f;
        float laneGap = mods.Min(m => m.xMax <= laneX0 ? laneX0 - m.xMax : m.xMin >= laneX1 ? m.xMin - laneX1 : -1f);
        TestProbe.Check("el carril de nado (frente a las puertas y bocas, de arriba abajo) queda libre en cada cara", laneGap >= 2f, $"carril x {laneX0:0.0}…{laneX1:0.0} m; el módulo más cerca a {laneGap:0.0} m");
        float faceArea = 2f * h.OuterHalf * TreeSpec.RootHeight, covered = mods.Sum(m => m.width * m.height);
        TestProbe.Check("cada cara del tronco queda casi toda transparente (≤ 15 % cubierta)", covered / faceArea <= 0.15f, $"{covered:0} de {faceArea:0} m² = {covered / faceArea * 100f:0.0} %");
        // Medido en la malla: lo del tronco entre 37,6 y 56 m; lo de las ramas, por encima del árbol más alto.
        Mesh cm = root.Find("Fotobio_cultivo") != null ? root.Find("Fotobio_cultivo").GetComponent<MeshFilter>().sharedMesh : null;
        float tMin = float.MaxValue, tMax = float.MinValue, kMin = float.MaxValue;
        if (cm != null)
            foreach (Vector3 v in cm.vertices)
            {
                if (new Vector2(v.x, v.z).magnitude < h.OuterHalf + 4f) { tMin = Mathf.Min(tMin, v.y); tMax = Mathf.Max(tMax, v.y); }
                else kMin = Mathf.Min(kMin, v.y);
            }
        TestProbe.Check("el tronco se ve entero de 0 a 37 m y de 56 m arriba (se ve nadar desde el hielo y desde los pisos)", cm != null && tMin >= FotobioSpec.TrunkBottom - 0.01f && tMax <= FotobioSpec.TrunkTop + 0.01f, $"tubos del tronco de {tMin:0.0} a {tMax:0.0} m");
        float minTop = TreeSpec.TallestTree + TreeSpec.TreeClearance;
        TestProbe.Check("las quillas de las ramas quedan por encima del árbol más alto del santuario", kMin >= minTop - 0.01f, $"lo más bajo a {kMin:0.0} m (mínimo {minTop:0.0})");
        bool underTube = true; string keelInfo = "";
        for (int lv = 1; lv <= h.levels; lv++)
        {
            var k = FotobioSpec.Keel(h, lv);
            float tubeBottom = h.LevelY(lv) - 0.3f - h.tubeWall;
            underTube &= k.y1 < tubeBottom - 0.8f && k.x0 > h.mouthX + h.tubeHalf && k.x1 < h.ColumnS.x - h.ColOuter;
            keelInfo += $"nivel {lv}: {k.y0:0.0}–{k.y1:0.0} m bajo el tubo ({tubeBottom:0.0}); ";
        }
        TestProbe.Check("cada quilla cuelga bajo su tubo, entre la boca y la columna (los lados del tubo se ven)", underTube, keelInfo);

        // ── 3. NÚMEROS ──
        float area = fb.areaM2, bio = FotobioSpec.BiomassKgDay(area), o2 = FotobioSpec.O2KgDay(area), persons = o2 / FotobioSpec.O2PerPerson;
        TestProbe.Check("productividad de diseño dentro de lo publicado (10–48 g/m²/día)", FotobioSpec.DesignProductivity >= FotobioSpec.ProductivityMin && FotobioSpec.DesignProductivity <= FotobioSpec.ProductivityMax, $"{FotobioSpec.DesignProductivity} g/m²/día");
        TestProbe.Check("O₂ por gramo de biomasa dentro de la estequiometría (1,2–1,8 g/g)", FotobioSpec.O2PerGram >= FotobioSpec.O2PerGramMin && FotobioSpec.O2PerGram <= FotobioSpec.O2PerGramMax, $"{FotobioSpec.O2PerGram} g/g");
        float m2pp = area / Mathf.Max(0.01f, persons);
        TestProbe.Check("m² de cultivo por persona dentro de lo esperado (15–60 m²)", m2pp >= 15f && m2pp <= 60f, $"{m2pp:0.0} m² por persona ({o2:0.0} kg O₂/día para {persons:0.0} personas)");
        float vol = bio * 1000f / Mathf.Max(1f, fb.liters);
        TestProbe.Check("productividad por litro como la de los tubos reales (0,3–2,5 g/L/día)", vol >= 0.3f && vol <= 2.5f, $"{vol:0.00} g/L/día en {fb.liters / 1000f:0.0} m³");
        float ppfd = FotobioSpec.LedPPFD(area), kw = FotobioSpec.LedKW(area);
        TestProbe.Check("los LED de la noche polar dan una luz razonable (150–600 µmol/m²/s) con < 10 % de la central (1 MWe)", ppfd >= 150f && ppfd <= 600f && kw < 100f, $"{ppfd:0} µmol/m²/s, {kw:0} kW");
        int people = FotobioFacade.People;
        TestProbe.Check("con salud de 0,85 el O₂ cubre a toda la gente del santuario", o2 * 0.85f >= people * FotobioSpec.O2PerPerson, $"{o2 * 0.85f:0.0} kg/día para {people} personas ({people * FotobioSpec.O2PerPerson:0.0} kg)");
        StructuralLimits.RootReport rep = FotobioSpec.Structure(h, out float mn, out float km);
        TestProbe.Check("el tronco y las ramas aguantan la fachada (StructuralLimits.RootCheck + su peso)", !rep.Fail, $"fachada {mn:0.00} MN, {km:0.0} MN·m por rama → {rep}");

        // ── 4. CUIDADO ──
        float d0 = fb.density, n0 = fb.nutrients, c0 = fb.contamination, h0 = fb.health; int cd0 = fb.lastCareDay, tc0 = fb.timesCared; string by0 = fb.lastCaredBy; float hv0 = fb.harvestedKgTotal, o0 = fb.o2KgTotal;
        fb.enabled = false;   // (que Update no lo mueva mientras se mide)
        fb.density = 1.0f; fb.nutrients = 0.5f; fb.contamination = 0.15f; fb.health = 0.8f;
        for (int i = 0; i < 30; i++) fb.Advance(0.1f);   // tres días sin cuidar
        float hNo = fb.health, wNo = fb.Wellbeing;
        TestProbe.Check("tres días sin cuidar: se espesa, se queda sin medio y la salud baja", hNo < 0.8f && fb.density > FotobioSpec.SelfShade && fb.nutrients < 0.35f, $"salud 80 % → {hNo * 100f:0} %, densidad {fb.density:0.00} g/L, nutrientes {fb.nutrients * 100f:0} %, contaminación {fb.contamination * 100f:0} %");
        var (b, a) = fb.Care("Prueba");
        TestProbe.Check("el cuidado diario sube la salud, cosecha y repone el medio de Zarrouk", a > b && Mathf.Abs(fb.density - FotobioSpec.HarvestTo) < 0.01f && fb.nutrients > 0.99f && fb.Wellbeing > wNo, $"salud {b * 100f:0} % → {a * 100f:0} %, densidad {fb.density:0.00} g/L, bienestar {wNo * 100f:0} % → {fb.Wellbeing * 100f:0} %");
        float hAfter = fb.health;
        for (int i = 0; i < 3; i++) { for (int j = 0; j < 10; j++) fb.Advance(0.1f); fb.Care("Prueba"); }
        TestProbe.Check("cuidado cada día, la salud sigue subiendo y el O₂ pasa de lo que respira la gente", fb.health > hAfter && fb.o2KgDay >= people * FotobioSpec.O2PerPerson, $"salud {fb.health * 100f:0} %, O₂ {fb.o2KgDay:0.0} kg/día ({fb.PeopleCovered:0.0} personas; hay {people})");
        fb.density = d0; fb.nutrients = n0; fb.contamination = c0; fb.health = h0; fb.lastCareDay = cd0; fb.timesCared = tc0; fb.lastCaredBy = by0; fb.harvestedKgTotal = hv0; fb.o2KgTotal = o0;
        fb.enabled = true;
        var ms = AreaWork.Today(0, ConstructionProject.All).Where(m => m.id == FotobioFacade.MissionId).ToList();
        TestProbe.Check("la jornada tiene el cuidado de los fotobiorreactores: obligatorio, 1 h, en Biología", ms.Count == 1 && ms[0].mandatory && Mathf.Approximately(ms[0].hours, 1f) && ms[0].area == FotobioFacade.MissionArea, ms.Count == 1 ? ms[0].ToString() : $"{ms.Count} misiones");
        bool lab = FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None).Any(s => s.displayName == FotobioFacade.MissionArea);
        TestProbe.Check("existe el laboratorio de Biología (la sala de cuidados del cultivo)", lab);

        // ── 5. ESPACIO ──
        // Quillas: 2 m de aire con cualquier plataforma (piso o campo de punta) que tengan encima o debajo en planta.
        float worstV = float.MaxValue; string worstWhat = "";
        for (int k = 1; k <= TreeSpec.Platforms; k++)
        {
            var (lv, side, tip) = TreeSpec.Platform(k);
            if (!tip) continue;
            var kl = FotobioSpec.Keel(h, lv);
            Vector3 a0 = HelixTower.FromSide(side, new Vector3(kl.x0, 0f, kl.z - 0.1f)), a1 = HelixTower.FromSide(side, new Vector3(kl.x1, 0f, kl.z + 0.1f));
            Rect keel = Rect.MinMaxRect(Mathf.Min(a0.x, a1.x), Mathf.Min(a0.z, a1.z), Mathf.Max(a0.x, a1.x), Mathf.Max(a0.z, a1.z));
            for (int j = 1; j <= TreeSpec.Platforms; j++)
            {
                var (lj, sj, tj) = TreeSpec.Platform(j);
                Rect pr = tj ? HelixTower.PlanRect(sj, h.TipCenterS, h.tipSize) : HelixTower.PlanRect(sj, h.FloorCenterS, h.floorSize);
                if (HelixTower.Overlap(keel, pr) <= 0f) continue;
                float py = TreeSpec.PlatformY(j), gap = py >= kl.y1 ? py - 0.3f - TreeSpec.StrutDrop - kl.y1 : kl.y0 - (py + 2f);   // encima: su losa y tornapuntas; debajo: 2 m de paso sobre el suelo
                if (gap < worstV) { worstV = gap; worstWhat = $"quilla del piso {k} y plataforma {j}"; }
            }
        }
        TestProbe.Check("las quillas guardan 2 m con las plataformas de encima y de debajo", worstV >= 2f || worstV == float.MaxValue, worstV == float.MaxValue ? "ninguna plataforma en su planta" : $"{worstWhat}: {worstV:0.0} m");
        // Ni las copas de los cultivos ni otra pieza del árbol (salvo la celosía de su rama, de la que cuelga) a menos de 1 m de una quilla.
        var near = new List<string>();
        if (tree != null)
            for (int k = 1; k <= TreeSpec.Platforms; k++)
            {
                var (lv, side, tip) = TreeSpec.Platform(k);
                if (!tip) continue;
                var kl = FotobioSpec.Keel(h, lv);
                Quaternion inv = Quaternion.Inverse(HelixTower.SideRot(side));
                Bounds kb = new Bounds(); kb.SetMinMax(new Vector3(kl.x0 - 1f, kl.y0 - 1f, kl.z - 1f), new Vector3(kl.x1 + 1f, kl.y1 + 0.2f, kl.z + 1f));   // marco del lado (arriba, solo hasta los colgadores)
                Bounds wb = new Bounds(tree.TransformPoint(HelixTower.FromSide(side, kb.center)), Vector3.one * (kb.size.magnitude + 1f));
                foreach (Renderer r in tree.GetComponentsInChildren<Renderer>())
                {
                    if (r.transform.IsChildOf(root) || !r.bounds.Intersects(wb)) continue;
                    if (r.name.Contains("rama") || r.name.Contains("Rama")) continue;   // su tubo y su celosía
                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    if (mf == null || mf.sharedMesh == null) continue;
                    Vector3[] vs = mf.sharedMesh.vertices;
                    for (int i = 0; i < vs.Length; i += Mathf.Max(1, vs.Length / 400))
                    {
                        Vector3 p = inv * tree.InverseTransformPoint(r.transform.TransformPoint(vs[i]));
                        if (kb.Contains(p)) { near.Add($"{r.name} (piso {k})"); break; }
                    }
                }
            }
        TestProbe.Check("nada del árbol (copas de los cultivos, losas, tornapuntas) a menos de 1 m de una quilla", near.Count == 0, near.Count == 0 ? "libres" : $"{near.Count}: {string.Join(", ", near.Distinct().Take(6))}");
        // Alfombras: ≥ 2 m de cualquier pieza de la fachada.
        float padGap = float.MaxValue;
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
            foreach (CarpetPad p in FindObjectsByType<CarpetPad>(FindObjectsSortMode.None))
                if (p.TryGetComponent(out BoxCollider bc)) padGap = Mathf.Min(padGap, BoundsGap(r.bounds, bc.bounds));
        TestProbe.Check("la fachada a ≥ 2 m de las zonas de alfombras", padGap >= 2f, $"la más cerca a {padGap:0.0} m");
        // El círculo que barre la fachada del tronco al girar: nada fuera del árbol dentro (árboles de hielo, torres…).
        float sweep = 0f;
        foreach (Rect m in mods) sweep = Mathf.Max(sweep, new Vector2(Mathf.Max(Mathf.Abs(m.xMin), Mathf.Abs(m.xMax)) + 0.15f, FotobioSpec.PlaneZ(h) + 0.1f).magnitude);
        Vector3 axis = tree != null ? tree.position : TreeSpec.Site;
        var hits = new List<string>();
        foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (tree != null && r.transform.IsChildOf(tree)) continue;
            Bounds bb = r.bounds;
            if (bb.size.x > 80f || bb.size.z > 80f) continue;   // el hielo y el mar
            if (r.GetComponentInParent<Rigidbody>() != null || r.GetComponentInParent<Anima>() != null) continue;   // lo que se mueve
            if (bb.max.y < FotobioSpec.TrunkBottom - 2f || bb.min.y > FotobioSpec.TrunkTop + 2f) continue;
            float dx = Mathf.Max(bb.min.x - axis.x, 0f, axis.x - bb.max.x), dz = Mathf.Max(bb.min.z - axis.z, 0f, axis.z - bb.max.z);
            if (new Vector2(dx, dz).magnitude < sweep + 2f) hits.Add(r.name);
        }
        TestProbe.Check("nada fuera del árbol a menos de 2 m del círculo que barre la fachada al girar", hits.Count == 0, hits.Count == 0 ? $"radio {sweep:0.0} m" : $"{hits.Count}: {string.Join(", ", hits.Take(6))}");
        TestProbe.End();
    }

    static float RectGap(Rect a, Rect b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.xMin - b.xMax, b.xMin - a.xMax)), dy = Mathf.Max(0f, Mathf.Max(a.yMin - b.yMax, b.yMin - a.yMax));
        return new Vector2(dx, dy).magnitude;
    }
    static float BoundsGap(Bounds a, Bounds b)
    {
        Vector3 d = Vector3.Max(Vector3.zero, Vector3.Max(a.min - b.max, b.min - a.max));
        return d.magnitude;
    }
}
