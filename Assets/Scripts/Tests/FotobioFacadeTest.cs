using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LAS FACHADAS DE FOTOBIORREACTORES (Berón 2026-10-31 y 2026-11-08; docs/fotobiorreactores.md):
/// 0. DOS FACHADAS: la del Árbol de los Campos (tronco y quillas) y la del tronco de la torre de Ciencias (solo tronco, 480 m²).
/// Para CADA fachada:
/// 1. EXISTE: cuelga de su árbol con sus módulos de tubos (tronco), sus quillas (solo el Árbol), sus LED, bombas, cuadro, etiqueta y su
///    demanda de energía.
/// 2. LIBRE: ningún módulo cerca de una puerta, una boca de rama o el carril de nado; el tronco queda transparente abajo
///    y arriba, cubierto poco en cada cara, y NO TAPA A LOS QUE NADAN (la columna de nado de <see cref="ArbolCampos.SwimPath"/>
///    se ve desde el hielo y desde enfrente de cada cara); las quillas, bajo su tubo y por encima del árbol más alto.
/// 5. ESPACIO: 2 m con las plataformas de arriba y de abajo de cada quilla, con las alfombras, y nada fuera del árbol en el
///    círculo que barre la fachada (al girar, el Árbol; la torre no gira, y el círculo es más que su caja).
/// Y de todas juntas:
/// 3. NÚMEROS: productividad, O₂ por gramo, m² por persona, productividad por litro, luz de los LED y su potencia dentro de lo
///    investigado; CON SALUD 0,85 EL O₂ CUBRE A LAS 32 PERSONAS (26,9 kg/día); cada tronco aguanta su fachada.
/// 4. CUIDADO: sin cuidar, el cultivo empeora; el cuidado diario (<see cref="FotobioFacade.Care"/>) sube la salud, cosecha y
///    repone el medio; cuidadas cada día, el O₂ de las fachadas pasa de lo que respira la gente; cada fachada tiene su misión
///    obligatoria de ≤ 1 h en Biología.
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

    static Rect DoorRect(HelixTower h, float x, float y) => new Rect(x - h.doorWidth * 0.5f, y, h.doorWidth, h.doorHeight);

    /// <summary>Las medidas del tronco de la fachada: las del Árbol para la primera; para las de torre, las que guardó el constructor.</summary>
    static HelixTower TrunkOf(FotobioFacade fb) => fb.index == 0 || fb.trunk == null || fb.trunk.levels <= 0 ? TreeSpec.Helix : fb.trunk;

    public IEnumerator Run()
    {
        TestProbe.Begin("Fachadas de fotobiorreactores (Árbol y Ciencias): existen, libres de puertas y nado, O₂ para todos, cuidado diario y espacio");
        List<FotobioFacade> all = FotobioFacade.InScene();
        if (all.Count == 0) { TestProbe.Check("hay una fachada de fotobiorreactores en la escena", false); TestProbe.End(); yield break; }

        // ── 0. LAS DOS FACHADAS ──
        FotobioFacade second = all.FirstOrDefault(f => f.index == 1);
        ArbolCampos secondTree = second != null && second.transform.parent != null ? second.transform.parent.GetComponent<ArbolCampos>() : null;
        TestProbe.Check($"dos fachadas: la del Árbol y la del tronco de la torre de {FotobioSpec.SecondBuilding}",
            all.Count == 2 && all[0].index == 0 && secondTree != null && secondTree.building == FotobioSpec.SecondBuilding,
            string.Join(" · ", all.Select(f => $"{f.index}: {f.name} en {(f.transform.parent != null ? f.transform.parent.name : "—")} ({f.areaM2:0} m²)")));
        if (second != null)
            TestProbe.Check("la de Ciencias: 4 columnas de 10 módulos en el tronco (480 m²), sin quillas", second.trunkModules == 4 * FotobioSpec.ModulesPerColumn && second.keelPanels == 0 && Mathf.Abs(second.areaM2 - FotobioSpec.TrunkArea) < 1f,
                $"{second.trunkModules} módulos, {second.keelPanels} paneles, {second.areaM2:0} m²");

        foreach (FotobioFacade f in all) yield return Facade(f);

        // ── 3. NÚMEROS (todas juntas) ──
        float area = all.Sum(f => f.areaM2), liters = all.Sum(f => f.liters);
        float bio = FotobioSpec.BiomassKgDay(area), o2 = FotobioSpec.O2KgDay(area), persons = o2 / FotobioSpec.O2PerPerson;
        TestProbe.Near("superficie de cultivo de las dos fachadas (m²)", area, FotobioSpec.TotalArea(), 1f);
        TestProbe.Check("productividad de diseño dentro de lo publicado (10–48 g/m²/día)", FotobioSpec.DesignProductivity >= FotobioSpec.ProductivityMin && FotobioSpec.DesignProductivity <= FotobioSpec.ProductivityMax, $"{FotobioSpec.DesignProductivity} g/m²/día");
        TestProbe.Check("O₂ por gramo de biomasa dentro de la estequiometría (1,2–1,8 g/g)", FotobioSpec.O2PerGram >= FotobioSpec.O2PerGramMin && FotobioSpec.O2PerGram <= FotobioSpec.O2PerGramMax, $"{FotobioSpec.O2PerGram} g/g");
        float m2pp = area / Mathf.Max(0.01f, persons);
        TestProbe.Check("m² de cultivo por persona dentro de lo esperado (15–60 m²)", m2pp >= 15f && m2pp <= 60f, $"{m2pp:0.0} m² por persona ({o2:0.0} kg O₂/día para {persons:0.0} personas)");
        float vol = bio * 1000f / Mathf.Max(1f, liters);
        TestProbe.Check("productividad por litro como la de los tubos reales (0,3–2,5 g/L/día)", vol >= 0.3f && vol <= 2.5f, $"{vol:0.00} g/L/día en {liters / 1000f:0.0} m³");
        float kwAll = all.Sum(f => FotobioSpec.LedKW(f.areaM2)), kwMax = all.Max(f => FotobioSpec.LedKW(f.areaM2));
        float ppfd = FotobioSpec.LedPPFD(area);
        TestProbe.Check("los LED de la noche polar dan una luz razonable (150–600 µmol/m²/s), < 100 kW cada fachada y < 5 % del reactor entre todas", ppfd >= 150f && ppfd <= 600f && kwMax < 100f && kwAll < 0.05f * EnergyRule.ReactorKW,
            $"{ppfd:0} µmol/m²/s; {string.Join(" + ", all.Select(f => $"{FotobioSpec.LedKW(f.areaM2):0}"))} = {kwAll:0} kW en la noche polar; de media {all.Sum(f => FotobioSpec.LedMeanKW(f.areaM2)):0} kW (LED {FotobioSpec.LedMeanFraction() * 100f:0} % del año)");
        int people = FotobioFacade.People;
        TestProbe.Check($"son las personas de la lista ({PopulationPlan.PlannedPeople}: el personal, la Magnate y el recién llegado)", people >= PopulationPlan.PlannedPeople, $"{people}");
        TestProbe.Check("con salud de 0,85 el O₂ cubre a toda la gente del santuario", o2 * 0.85f >= people * FotobioSpec.O2PerPerson, $"{o2 * 0.85f:0.0} kg/día para {people} personas ({people * FotobioSpec.O2PerPerson:0.0} kg)");
        foreach (FotobioFacade f in all)
        {
            HelixTower h = TrunkOf(f);
            if (f.index == 0)
            {
                StructuralLimits.RootReport rep = FotobioSpec.Structure(h, out float mn, out float km);
                TestProbe.Check("el tronco y las ramas del Árbol aguantan su fachada (StructuralLimits.RootCheck + su peso)", !rep.Fail, $"fachada {mn:0.00} MN, {km:0.0} MN·m por rama → {rep}");
            }
            else
            {
                StructuralLimits.RootReport rep = FotobioSpec.TowerStructure(h, out float mn);
                TestProbe.Check($"el tronco de {f.building} aguanta su fachada", !rep.Fail, $"fachada {mn:0.00} MN → {rep}");
            }
        }

        // ── 4. CUIDADO ──
        var saved = all.Select(f => (f, f.density, f.nutrients, f.contamination, f.health, f.lastCareDay, f.timesCared, f.lastCaredBy, f.harvestedKgTotal, f.o2KgTotal)).ToList();
        foreach (FotobioFacade f in all) f.enabled = false;   // (que Update no las mueva mientras se mide)
        FotobioFacade fb = all[0];
        fb.density = 1.0f; fb.nutrients = 0.5f; fb.contamination = 0.15f; fb.health = 0.8f;
        for (int i = 0; i < 30; i++) fb.Advance(0.1f);   // tres días sin cuidar
        float hNo = fb.health, wNo = fb.Wellbeing;
        TestProbe.Check("tres días sin cuidar: se espesa, se queda sin medio y la salud baja", hNo < 0.8f && fb.density > FotobioSpec.SelfShade && fb.nutrients < 0.35f, $"salud 80 % → {hNo * 100f:0} %, densidad {fb.density:0.00} g/L, nutrientes {fb.nutrients * 100f:0} %, contaminación {fb.contamination * 100f:0} %");
        var (b, a) = fb.Care("Prueba");
        TestProbe.Check("el cuidado diario sube la salud, cosecha y repone el medio de Zarrouk", a > b && Mathf.Abs(fb.density - FotobioSpec.HarvestTo) < 0.01f && fb.nutrients > 0.99f && fb.Wellbeing > wNo, $"salud {b * 100f:0} % → {a * 100f:0} %, densidad {fb.density:0.00} g/L, bienestar {wNo * 100f:0} % → {fb.Wellbeing * 100f:0} %");
        float hAfter = fb.health;
        foreach (FotobioFacade f in all.Skip(1)) { f.density = 1.0f; f.nutrients = 0.5f; f.contamination = 0.15f; f.health = 0.8f; f.Care("Prueba"); }
        for (int i = 0; i < 3; i++) foreach (FotobioFacade f in all) { for (int j = 0; j < 10; j++) f.Advance(0.1f); f.Care("Prueba"); }
        float o2Cared = all.Sum(f => f.o2KgDay);
        TestProbe.Check("cuidadas cada día, la salud sigue subiendo y el O₂ de las fachadas pasa de lo que respira la gente", fb.health > hAfter && o2Cared >= people * FotobioSpec.O2PerPerson,
            $"salud {string.Join(" / ", all.Select(f => $"{f.health * 100f:0} %"))}, O₂ {string.Join(" + ", all.Select(f => $"{f.o2KgDay:0.0}"))} = {o2Cared:0.0} kg/día ({o2Cared / FotobioSpec.O2PerPerson:0.0} personas; hay {people}, {people * FotobioSpec.O2PerPerson:0.0} kg)");
        foreach (var s in saved)
        {
            s.f.density = s.density; s.f.nutrients = s.nutrients; s.f.contamination = s.contamination; s.f.health = s.health; s.f.lastCareDay = s.lastCareDay;
            s.f.timesCared = s.timesCared; s.f.lastCaredBy = s.lastCaredBy; s.f.harvestedKgTotal = s.harvestedKgTotal; s.f.o2KgTotal = s.o2KgTotal; s.f.enabled = true;
        }
        yield return null;
        var today = AreaWork.Today(0, ConstructionProject.All);
        foreach (FotobioFacade f in all)
        {
            var ms = today.Where(m => m.id == f.Id).ToList();
            TestProbe.Check($"la jornada tiene el cuidado de la fachada {(f.index == 0 ? "del Árbol" : $"de {f.building}")} («{f.Id}»): obligatorio, ≤ 1 h, en Biología",
                ms.Count == 1 && ms[0].mandatory && ms[0].hours <= FruitGardener.MaxTreeTaskHours + 1e-4f && ms[0].area == FotobioFacade.MissionArea, ms.Count == 1 ? ms[0].ToString() : $"{ms.Count} misiones");
        }
        bool lab = FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None).Any(s => s.displayName == FotobioFacade.MissionArea);
        TestProbe.Check("existe el laboratorio de Biología (la sala de cuidados del cultivo)", lab);
        TestProbe.End();
    }

    // ── 1, 2 y 5: UNA FACHADA ──
    IEnumerator Facade(FotobioFacade fb)
    {
        Transform root = fb.transform, tree = root.parent;
        HelixTower h = TrunkOf(fb);
        ArbolCampos arbol = tree != null ? tree.GetComponent<ArbolCampos>() : null;
        string who = fb.index == 0 ? "del Árbol" : $"de {fb.building}";
        bool rotates = tree != null && tree.GetComponent<TreeRotator>() != null;

        // ── 1. EXISTE ──
        TestProbe.Check($"[{who}] cuelga de su árbol", arbol != null && arbol.building == fb.building, tree != null ? tree.name : "sin padre");
        int wantModules = 4 * FotobioSpec.TrunkBays.Length * FotobioSpec.ModulesPerColumn, wantPanels = fb.index == 0 ? 4 * FotobioSpec.KeelPanels : 0;
        TestProbe.Check($"[{who}] módulos de tubos en el tronco{(wantPanels > 0 ? " y paneles en las 4 ramas" : "")}", fb.trunkModules == wantModules && fb.keelPanels == wantPanels, $"{fb.trunkModules}/{wantModules} módulos, {fb.keelPanels}/{wantPanels} paneles");
        TestProbe.Near($"[{who}] superficie de cultivo (m²)", fb.areaM2, fb.index == 0 ? FotobioSpec.Area() : FotobioSpec.TrunkArea, 1f);
        string[] parts = { "Fotobio_cultivo", "Fotobio_marco", "Fotobio_LED", "Fotobio_bombas", "Cuadro_fotobio" };
        var missing = parts.Where(p => root.Find(p) == null).ToList();
        TestProbe.Check($"[{who}] tubos de cultivo, colectores y marco, LED, bombas y cuadro", missing.Count == 0, missing.Count == 0 ? "todo" : "faltan " + string.Join(", ", missing));
        yield return null; yield return new WaitForSeconds(1.2f);   // (la etiqueta se refresca cada segundo)
        WorldLabel wl = root.Find("Cuadro_fotobio") != null ? root.Find("Cuadro_fotobio").GetComponent<WorldLabel>() : null;
        TestProbe.Check($"[{who}] el cuadro enseña densidad, salud y O₂", wl != null && wl.text.Contains("densidad") && wl.text.Contains("salud") && wl.text.Contains("O₂"), wl != null ? wl.text : "sin etiqueta");
        FloorEnergy fe = root.GetComponent<FloorEnergy>();
        TestProbe.Check($"[{who}] sus LED están en el balance de energía (FloorEnergy, potencia media)", fe != null && fe.KW >= FotobioSpec.LedMeanKW(fb.areaM2) - 0.5f, fe != null ? $"{fe.KW:0} kW de media" : "sin FloorEnergy");

        // ── 2. LIBRE DE PUERTAS, BOCAS, CARRIL DE NADO; EL TRONCO SE VE ──
        List<Rect> mods = FotobioSpec.TrunkModules(h);
        // Las puertas del tronco de cada cara (la del suelo, las de los pisos y las de sus anillos: las de ArbolCampos.levels que están en la pared del
        // tronco; en las torres cada piso pone la suya donde su umbral queda libre) y las bocas de las ramas, con su carril de nado de arriba abajo.
        var gaps = new List<Rect>[4]; var lane = new List<Vector2>[4];
        for (int s = 0; s < 4; s++) { gaps[s] = new List<Rect>(); lane[s] = new List<Vector2>(); }
        if (arbol != null)
            foreach (TreeLevel L in arbol.levels)
            {
                if (L.side < 0 || L.side > 3 || L.kind == TreeLevel.Kind.Top) continue;
                Vector3 d = Quaternion.Inverse(HelixTower.SideRot(L.side)) * L.door;
                if (Mathf.Abs(d.z - (h.waterHalf + h.wall * 0.5f)) > 1.5f) continue;   // (la puerta de una punta está en su columna, no en el tronco)
                gaps[L.side].Add(DoorRect(h, d.x, Mathf.Max(0f, d.y - 0.5f)));
                lane[L.side].Add(new Vector2(d.x - h.doorWidth * 0.5f - 1f, d.x + h.doorWidth * 0.5f + 1f));
            }
        for (int lv = 1; lv <= h.levels; lv++)
            for (int s = 0; s < 4; s++)
                if (!HelixTower.HasFloor(lv, s))
                {
                    gaps[s].Add(new Rect(h.mouthX - h.tubeHalf, h.LevelY(lv) - 0.3f - h.tubeWall, 2f * h.tubeHalf, h.TubeInnerH + 2f * h.tubeWall));
                    lane[s].Add(new Vector2(h.mouthX - h.tubeHalf - 1f, h.mouthX + h.tubeHalf + 1f));
                }
        float worstDoor = float.MaxValue, laneGap = float.MaxValue;
        for (int s = 0; s < 4; s++)
            foreach (Rect m in mods)
            {
                foreach (Rect g in gaps[s]) worstDoor = Mathf.Min(worstDoor, RectGap(m, g));
                foreach (Vector2 l in lane[s]) laneGap = Mathf.Min(laneGap, m.xMax <= l.x ? l.x - m.xMax : m.xMin >= l.y ? m.xMin - l.y : -1f);
            }
        int nGaps = gaps.Sum(g => g.Count);
        TestProbe.Check($"[{who}] ningún módulo a menos de 2 m de una puerta o de la boca de una rama", worstDoor >= 2f && nGaps > 0, $"el más cerca a {worstDoor:0.0} m ({nGaps} huecos)");
        TestProbe.Check($"[{who}] el carril de nado (frente a las puertas y bocas, de arriba abajo) queda libre en cada cara", laneGap >= 2f, $"el módulo más cerca a {laneGap:0.0} m del carril");
        float faceArea = 2f * h.OuterHalf * h.RootHeight, covered = mods.Sum(m => m.width * m.height);
        TestProbe.Check($"[{who}] cada cara del tronco queda casi toda transparente (≤ 15 % cubierta)", covered / faceArea <= 0.15f, $"{covered:0} de {faceArea:0} m² = {covered / faceArea * 100f:0.0} %");
        // Medido en la malla: lo del tronco entre TrunkBottom y TrunkTop (en planta, dentro del cuadrado del tronco + 4 m); lo de las ramas, aparte.
        Mesh cm = root.Find("Fotobio_cultivo") != null ? root.Find("Fotobio_cultivo").GetComponent<MeshFilter>().sharedMesh : null;
        float tMin = float.MaxValue, tMax = float.MinValue, kMin = float.MaxValue;
        if (cm != null)
            foreach (Vector3 v in cm.vertices)
            {
                if (Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.z)) < h.OuterHalf + 4f) { tMin = Mathf.Min(tMin, v.y); tMax = Mathf.Max(tMax, v.y); }
                else kMin = Mathf.Min(kMin, v.y);
            }
        TestProbe.Check($"[{who}] el tronco se ve entero de 0 a {FotobioSpec.TrunkBottom:0.0} m y de {FotobioSpec.TrunkTop:0} m arriba (se ve nadar desde el hielo y desde los pisos)", cm != null && tMin >= FotobioSpec.TrunkBottom - 0.01f && tMax <= FotobioSpec.TrunkTop + 0.01f, $"tubos del tronco de {tMin:0.0} a {tMax:0.0} m");
        // SE VE NADAR (Berón: el tronco es transparente para ver a la gente nadar entre pisos): la columna de nado (a 6 m del eje, hacia la puerta o la
        // boca de cada nivel) en la altura de las arpas, mirada desde 16 rumbos a 30 m del eje, a la altura de los ojos en el hielo y a su misma altura:
        // de frente a cada cara, ningún rayo la cruza con un arpa; en total, como mucho uno de cada 10.
        var lanes = new List<Vector3>();
        if (arbol != null)
            foreach (TreeLevel lv in arbol.levels)
            {
                Vector3 f = lv.via.Length > 0 ? lv.via[0] : lv.inside; f.y = 0f;
                if (Mathf.Max(Mathf.Abs(f.x), Mathf.Abs(f.z)) >= h.waterHalf || f.sqrMagnitude < 0.01f) continue;
                Vector3 d = TreeSpec.Dir(TreeSpec.Angle(f)) * 6f;
                if (lanes.All(o => (o - d).sqrMagnitude > 0.25f)) lanes.Add(d);
            }
        int rays = 0, blocked = 0, faceRays = 0, faceBlocked = 0; string firstBlock = "";
        float pz = FotobioSpec.PlaneZ(h);
        bool Blocked(Vector3 eye, Vector3 p)
        {
            for (int s = 0; s < 4; s++)
            {
                Quaternion inv = Quaternion.Inverse(HelixTower.SideRot(s));
                Vector3 a = inv * eye, b = inv * p;
                if (a.z <= pz || b.z >= pz) continue;
                float t = (a.z - pz) / (a.z - b.z); Vector3 x = Vector3.Lerp(a, b, t);
                foreach (Rect m in mods) if (x.x >= m.xMin && x.x <= m.xMax && x.y >= m.yMin && x.y <= m.yMax) return true;
            }
            return false;
        }
        foreach (Vector3 ln in lanes)
            for (float y = FotobioSpec.TrunkBottom; y <= FotobioSpec.TrunkTop; y += 1.5f)
                for (int k = 0; k < 16; k++)
                    foreach (float ey in new[] { 1.7f, y })
                    {
                        Vector3 eye = TreeSpec.Dir(k * 22.5f) * 30f + Vector3.up * ey, p = ln + Vector3.up * y;
                        bool face = k % 4 == 0, bl = Blocked(eye, p);
                        rays++; if (bl) { blocked++; if (firstBlock == "") firstBlock = $"rumbo {k * 22.5f:0}°, ojos a {ey:0} m, nadador a {y:0} m"; }
                        if (face) { faceRays++; if (bl) faceBlocked++; }
                    }
        TestProbe.Check($"[{who}] se ve nadar: las arpas no tapan la columna de nado (de frente a cada cara, nunca; en total ≤ 10 %)",
            lanes.Count >= 4 && faceBlocked == 0 && blocked <= rays * 0.1f,
            $"{lanes.Count} columnas; de frente {faceBlocked}/{faceRays} tapados; en total {blocked}/{rays} ({(rays > 0 ? blocked * 100f / rays : 0f):0.0} %)" + (firstBlock != "" ? $"; p. ej. {firstBlock}" : ""));
        if (fb.keelPanels > 0)
        {
            float minTop = TreeSpec.TallestTree + TreeSpec.TreeClearance;
            TestProbe.Check($"[{who}] las quillas de las ramas quedan por encima del árbol más alto del santuario", kMin >= minTop - 0.01f, $"lo más bajo a {kMin:0.0} m (mínimo {minTop:0.0})");
            bool underTube = true; string keelInfo = "";
            for (int lv = 1; lv <= h.levels; lv++)
            {
                var k = FotobioSpec.Keel(h, lv);
                float tubeBottom = h.LevelY(lv) - 0.3f - h.tubeWall;
                underTube &= k.y1 < tubeBottom - 0.8f && k.x0 > h.mouthX + h.tubeHalf && k.x1 < h.ColumnS.x - h.ColOuter;
                keelInfo += $"nivel {lv}: {k.y0:0.0}–{k.y1:0.0} m bajo el tubo ({tubeBottom:0.0}); ";
            }
            TestProbe.Check($"[{who}] cada quilla cuelga bajo su tubo, entre la boca y la columna (los lados del tubo se ven)", underTube, keelInfo);
            KeelSpace(h, tree, root);
        }

        // ── 5. ESPACIO ──
        // Alfombras: ≥ 2 m de cualquier pieza de la fachada.
        float padGap = float.MaxValue;
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
            foreach (CarpetPad p in FindObjectsByType<CarpetPad>(FindObjectsSortMode.None))
                if (p.TryGetComponent(out BoxCollider bc)) padGap = Mathf.Min(padGap, BoundsGap(r.bounds, bc.bounds));
        TestProbe.Check($"[{who}] la fachada a ≥ 2 m de las zonas de alfombras", padGap >= 2f, $"la más cerca a {padGap:0.0} m");
        // El círculo que barre la fachada del tronco al girar (en la torre, que no gira, el círculo es más que su caja): nada fuera del árbol dentro.
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
            if (r.name.StartsWith("Lote_")) continue;   // (el lote estático de Play: su caja abarca la zona entera; sus piezas se miran una a una)
            if (bb.max.y < FotobioSpec.TrunkBottom - 2f || bb.min.y > FotobioSpec.TrunkTop + 2f) continue;
            float dx = Mathf.Max(bb.min.x - axis.x, 0f, axis.x - bb.max.x), dz = Mathf.Max(bb.min.z - axis.z, 0f, axis.z - bb.max.z);
            if (new Vector2(dx, dz).magnitude < sweep + 2f) hits.Add(r.name);
        }
        TestProbe.Check($"[{who}] nada fuera del árbol a menos de 2 m del círculo que {(rotates ? "barre la fachada al girar" : "rodea la fachada")}", hits.Count == 0, hits.Count == 0 ? $"radio {sweep:0.0} m" : $"{hits.Count}: {string.Join(", ", hits.Take(6))}");
    }

    // Quillas (solo el Árbol): 2 m de aire con cualquier plataforma que tengan encima o debajo en planta, y nada del árbol a menos de 1 m.
    static void KeelSpace(HelixTower h, Transform tree, Transform root)
    {
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
        TestProbe.Check("[del Árbol] las quillas guardan 2 m con las plataformas de encima y de debajo", worstV >= 2f || worstV == float.MaxValue, worstV == float.MaxValue ? "ninguna plataforma en su planta" : $"{worstWhat}: {worstV:0.0} m");
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
        TestProbe.Check("[del Árbol] nada del árbol (copas de los cultivos, losas, tornapuntas) a menos de 1 m de una quilla", near.Count == 0, near.Count == 0 ? "libres" : $"{near.Count}: {string.Join(", ", near.Distinct().Take(6))}");
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
