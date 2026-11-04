using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PRUEBA 68 · EL SUELO Y LOS ELEMENTOS DE LOS MICROCOSMOS, SIN CARGAR ESCENAS (docs/microcosmos-suelo-y-elementos.md; Berón
/// 2026-11-04). Casos pequeños para cada pedido:
/// 1. la FLORA ENCIMA: una planta bajo una losa se ve enterrada; Settle la sube; una planta dentro de un collider se ve y se quita;
/// 2. la CAPA DE TIERRA: 10 cm (MicroGround.SoilThickness);
/// 3. el CATÁLOGO de elementos (lavabo → lago; cajonera → edificio; caja → cueva / fábrica; zapato → cueva / mercado; hornillas →
///    volcanes; foco → sol; lámpara → poste de luz; mesa → montaña; silla → nevado; «pilar» no es un lavabo; claves sin número);
/// 4. la SINCRONÍA (el hospital y cualquier piso): el inventario, su firma y lo que le falta a la forma de microcosmos; quién depende
///    del hospital;
/// 5. las ALFOMBRAS: 7 cm de canto con núcleo de circuitos y costura de LED;
/// 6. la ESCALA cívica: plato de una comida, olla de 2 L por comensal, estatua y fuente monumentales, todo asiento con sitio (MicroSeat),
///    el asiento libre más cercano;
/// 7. las ESTATUAS: una figura humana en una estatua es violación; la de insecto-humano no.
/// </summary>
public class MicroSueloTest : MonoBehaviour, ITestUnit
{
    public int Group => 68;   // no carga escenas (el 64 es YogaFloorsTest)
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<MicroSueloTest>() == null) tr.gameObject.AddComponent<MicroSueloTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Microcosmos: suelo de tierra, flora encima, elementos del piso, alfombras, escala cívica y estatuas (casos)");
        var root = new GameObject("PruebaSuelo").transform; root.position = new Vector3(0f, -800f, 12000f);
        try
        {
            // 1 · La flora encima, nunca debajo.
            Transform soil = new GameObject(MicroGround.SoilName).transform; soil.SetParent(root, false);
            Cube(soil, "Mosaico", new Vector3(0f, -0.05f, 0f), new Vector3(60f, 0.1f, 60f));
            Transform slab = Cube(root, "Losa_del_hospital", new Vector3(10f, 1f, 0f), new Vector3(10f, 2f, 10f));   // una losa de 2 m (a ×100: 2 cm)
            Transform flora = new GameObject("Flora_a_escala").transform; flora.SetParent(root, false);
            Transform buried = new GameObject("Brizna").transform; buried.SetParent(flora, false); buried.localPosition = new Vector3(10f, 0f, 0f);
            Transform ok = new GameObject("Musgo").transform; ok.SetParent(flora, false); ok.localPosition = new Vector3(-10f, 0f, 0f);
            Physics.SyncTransforms();
            var bad = MicroGround.ValidateFlora(root);
            TestProbe.Check("1 · una planta con el pie bajo una losa: enterrada (y la del suelo, no)", bad.Count == 1 && bad[0].Contains("Brizna"), string.Join(" · ", bad));
            var (settled, removed) = MicroGround.Settle(root);
            TestProbe.Check("1 · Settle la apoya encima de la losa", Mathf.Abs(buried.position.y - (root.position.y + 2f)) < 0.05f && removed == 0, $"pie a {buried.position.y - root.position.y:0.00} m; {settled} apoyadas, {removed} quitadas");
            TestProbe.Check("1 · después, ninguna planta enterrada", MicroGround.ValidateFlora(root).Count == 0, string.Join(" · ", MicroGround.ValidateFlora(root)));
            Transform post = Cube(root, "Pata_de_mesa", new Vector3(-20f, 5f, 0f), new Vector3(2f, 10f, 2f));   // una pata de 10 m de alto: más que el Probe
            Transform inside = new GameObject("Helecho").transform; inside.SetParent(flora, false); inside.localPosition = new Vector3(-20f, 0f, 0f);
            Physics.SyncTransforms();
            TestProbe.Check("1 · una planta dentro de una pata: violación", Has(MicroGround.ValidateFlora(root), "dentro"), string.Join(" · ", MicroGround.ValidateFlora(root)));
            MicroGround.Settle(root);
            yield return null;   // (Destroy, al final del fotograma)
            TestProbe.Check("1 · Settle la quita (no se queda dentro del mueble)", inside == null, inside != null ? $"sigue en {inside.position}" : "quitada");
            // 2 · La capa de tierra.
            TestProbe.Near("2 · la capa de tierra mide 10 cm (en metros del microcosmos)", MicroGround.SoilThickness, 0.10f, 0.001f);

            // 3 · El catálogo.
            TestProbe.Check("3 · lavabo → lago (y río si está abierto)", Id("Lavabo_2") == "lavabo" && MicroFloorElements.ById("lavabo").micro.Contains("LAGO"), Id("Lavabo_2"));
            TestProbe.Check("3 · cajonera y armario → edificio", Id("Cajonera") == "cajonera" && Id("Armario_ropero") == "cajonera" && MicroFloorElements.ById("cajonera").kind == MicroFloorElements.Kind.Edificio);
            var caja = MicroFloorElements.ById("caja"); var zap = MicroFloorElements.ById("zapato");
            TestProbe.Check("3 · caja → cueva antes de las ciudades y fábrica después; zapato → cueva o mercado", Id("Caja_de_carton") == "caja" && caja.MicroFor(-9000).Contains("CUEVA") && caja.MicroFor(1900).Contains("FÁBRICA") && zap.MicroFor(-9000).Contains("CUEVA") && zap.MicroFor(1819).Contains("MERCADO"));
            TestProbe.Check("3 · hornillas → volcanes; foco y bombilla → sol; lámpara de mesa → poste de luz (no mesa)", Id("Hornillas") == "hornilla" && Id("Foco") == "bombilla" && Id("Bombilla_techo") == "bombilla" && Id("Lampara_de_mesa") == "lampara", $"{Id("Hornillas")} {Id("Foco")} {Id("Lampara_de_mesa")}");
            TestProbe.Check("3 · mesa → montaña; silla → nevado; silla de ruedas → estación (no nevado)", Id("Mesa_comedor") == "mesa" && Id("Silla (3)") == "silla" && Id("Silla_de_ruedas") == "carro", $"{Id("Silla_de_ruedas")}");
            TestProbe.Check("3 · «Pilar» no es una pila de lavar; alfombra y estatua tienen su fila", Id("Pilar_2") == "columna" && Id("Alfombra_1") == "alfombra" && Id("Estatua_Patanjali") == "estatua", $"{Id("Pilar_2")}");
            TestProbe.Check("3 · la clave de una pieza no lleva su número de copia", MicroFloorElements.Key("Cama_12") == "cama" && MicroFloorElements.Key("Silla (3)") == "silla" && MicroFloorElements.Key("Lámpara 2") == "lampara");

            // 4 · El inventario, la firma y la sincronía (lo mismo para el hospital que para cualquier piso).
            Transform meso = new GameObject("Piso_de_prueba").transform; meso.SetParent(root, false); meso.localPosition = new Vector3(0f, 0f, 200f);
            Transform eq = new GameObject("Equipamiento").transform; eq.SetParent(meso, false);
            Transform bed = new GameObject("Cama_1").transform; bed.SetParent(eq, false); bed.gameObject.AddComponent<BoxCollider>(); Cube(bed, "Colchon", Vector3.up * 0.5f, new Vector3(1f, 0.3f, 2f));
            Cube(eq, "Monitor_3", new Vector3(3f, 1f, 0f), Vector3.one * 0.5f);
            Cube(eq, "Lavabo", new Vector3(-3f, 0.8f, 0f), new Vector3(0.6f, 0.3f, 0.5f));
            var inv = MicroFloorElements.Inventory(meso);
            TestProbe.Check("4 · el inventario: cada mueble entero una vez (la cama con su colchón es una pieza)", inv.Count == 3, string.Join(", ", MicroFloorElements.Keys(inv)));
            string sig0 = MicroFloorElements.Signature(MicroFloorElements.Keys(inv));
            Transform form = new GameObject(FloorForms.MicroName).transform; form.SetParent(root, false);
            foreach (var p in inv) if (p.key != "lavabo") { var me = new GameObject(p.key).AddComponent<MicroElement>(); me.transform.SetParent(form, false); me.key = p.key; }
            var miss = MicroFloorElements.Missing(meso, form);
            TestProbe.Check("4 · la sincronía ve lo que le falta a la forma de microcosmos (el lavabo)", miss.Count == 1 && miss[0] == "lavabo", string.Join(", ", miss));
            Cube(eq, "Gotero", new Vector3(5f, 1f, 0f), new Vector3(0.1f, 2f, 0.1f));
            string sig1 = MicroFloorElements.Signature(MicroFloorElements.Keys(MicroFloorElements.Inventory(meso)));
            TestProbe.Check("4 · si el piso gana una pieza, cambia su firma (el microcosmos se marca para revisar)", sig0 != sig1, $"{sig0} → {sig1}");
            var deps = MicroFloorElements.DependentsOf("Infirmary_Area");
            TestProbe.Check("4 · los microcosmos que dependen del hospital están listados", deps.Contains("Ambrosio (nivel 1)") && deps.Contains("Escútari") && deps.Count >= 4, string.Join(", ", deps));

            // 5 · Las alfombras más gruesas.
            Transform zone = new GameObject("Pista_de_prueba").transform; zone.SetParent(root, false); zone.localPosition = new Vector3(0f, 0f, 400f);
            FlyingCarpet fc = CarpetPad.BuildCarpet(zone, null, Vector3.zero, 0); fc.enabled = false;   // (sin pista: quieta)
            Bounds cb = BoundsOf(fc.transform);
            TestProbe.Check("5 · la alfombra tiene canto (≥ 6 cm) con su núcleo de circuitos y su costura de LED", cb.size.y >= 0.06f && fc.transform.Find("Nucleo_circuitos") != null && fc.transform.Find("Costura_LED") != null && fc.transform.Find("Circuito") != null, $"{cb.size.y * 100f:0.0} cm");
            TestProbe.Check("5 · quien va encima, sobre su grosor", FlyingCarpet.Thickness >= 0.06f && Mathf.Abs(cb.max.y - fc.transform.position.y - FlyingCarpet.Thickness) < 0.02f, $"grosor {FlyingCarpet.Thickness}");

            // 6 · La escala del mobiliario cívico.
            TestProbe.Check("6 · el plato da una comida (≥ 0,26 m) y la olla, 2 L por comensal", MicroCivicScale.PlateFor() >= 0.26f && MicroCivicScale.PotLitres(MicroCivicScale.PotFor(8)) >= 16f, $"plato {MicroCivicScale.PlateFor():0.00} m, olla de 8: {MicroCivicScale.PotLitres(MicroCivicScale.PotFor(8)):0} L");
            TestProbe.Check("6 · estatua y fuente monumentales (más del doble del ciudadano)", MicroCivicScale.StatueTotal() >= 2f * MicroCivicScale.Citizen && MicroCivicScale.FountainSide() >= 2f * MicroCivicScale.Citizen);
            Transform site = new GameObject("Plaza_de_prueba").transform; site.SetParent(root, false); site.localPosition = new Vector3(0f, 0f, 600f);
            Transform chair = Piece(site, "Banco", "banco");
            TestProbe.Check("6 · un banco sin sitio para sentarse: violación", Has(MicroBuildRules.ValidateCivicScale(site), "sin sitio"));
            MicroSeat ms = chair.gameObject.AddComponent<MicroSeat>(); ms.points = new[] { new Vector3(-0.7f, 0f, 0f), Vector3.zero, new Vector3(0.7f, 0f, 0f) };
            TestProbe.Check("6 · con sus tres sitios (MicroSeat), no", !Has(MicroBuildRules.ValidateCivicScale(site), "sin sitio"));
            MicroSeat near = MicroSeat.Nearest(chair.position + Vector3.right * 3f, 10f, out int slot);
            TestProbe.Check("6 · el asiento libre más cercano es el banco (su sitio de la derecha)", near == ms && slot == 2, $"{(near != null ? near.name : "ninguno")} sitio {slot}");
            ms.Take(slot, gameObject);
            MicroSeat near2 = MicroSeat.Nearest(chair.position + Vector3.right * 3f, 10f, out int slot2);
            TestProbe.Check("6 · un sitio ocupado no se ofrece", near2 == ms && slot2 == 1, $"sitio {slot2}");
            ms.Leave(slot, gameObject);

            // 7 · Las estatuas: insecto-humano, no humana.
            Transform statue = Piece(site, "Estatua del Libertador", "estatua");
            Cube(statue, "Figura", new Vector3(0f, 1.8f, 0f), new Vector3(0.6f, 1.4f, 0.4f));
            Cube(statue, "Peana", new Vector3(0f, 0.6f, 0f), new Vector3(1.6f, 1.2f, 1.6f));
            TestProbe.Check("7 · una estatua con figura humana: violación", Has(MicroBuildRules.ValidateCivicScale(site), "figura humana"));
            Destroy(statue.Find("Figura").gameObject);
            Transform ins = new GameObject("Figura_insecto_humano").transform; ins.SetParent(statue, false);
            Cube(ins, "Torso", new Vector3(0f, 2.6f, 0f), new Vector3(0.5f, 2.2f, 0.4f));
            yield return null;   // (Destroy al final del fotograma)
            var sv = MicroBuildRules.ValidateCivicScale(site);
            TestProbe.Check("7 · la de insecto-humano monumental (≥ 3,1 m), no", !Has(sv, "figura humana") && !Has(sv, "monumental"), string.Join(" · ", sv));
        }
        finally { Destroy(root.gameObject); }
        TestProbe.End();
    }

    static string Id(string name) { var e = MicroFloorElements.Match(name); return e != null ? e.id : "(ninguno)"; }
    static bool Has(List<string> l, string what) { foreach (string s in l) if (s.Contains(what)) return true; return false; }

    static Transform Cube(Transform parent, string name, Vector3 local, Vector3 size)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name; g.transform.SetParent(parent, false); g.transform.localPosition = local; g.transform.localScale = size;
        return g.transform;
    }

    static Transform Piece(Transform parent, string name, string kind)
    {
        var t = new GameObject(name).transform; t.SetParent(parent, false);
        var mp = t.gameObject.AddComponent<MicroPiece>(); mp.kind = kind;
        Cube(t, "Asiento", new Vector3(0f, 0.45f, 0f), new Vector3(2f, 0.08f, 0.5f));
        return t;
    }

    static Bounds BoundsOf(Transform t)
    {
        Bounds b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return b;
    }
}

/// <summary>
/// PRUEBA 67 · EL SUELO Y LOS ELEMENTOS EN LOS MICROCOSMOS CONSTRUIDOS (docs/microcosmos-suelo-y-elementos.md). Carga en aditivo, UNO A
/// UNO y con espera acotada, los microcosmos de <see cref="scenes"/> (por defecto: Bolívar, Ambrosio, el Hospital, el río de Sakshi
/// sobre la Sala de Yoga y la Cocina; los que no estén en Build Settings fallan y se siguen) y en cada uno mira:
/// (1) ninguna planta enterrada ni dentro de un collider (MicroGround.ValidateFlora);
/// (2) la capa de tierra (MicroSoil) con sus mosaicos combinados (pocos renderers) y la historia encima de ella;
/// (3) la «Forma_microcosmos» con un MicroElement por pieza del inventario con que se montó, y en sincronía con el piso del santuario
///     si está cargado (nada del piso sin su versión de microcosmos; el registro de dependencias al día);
/// (4) en los pueblos: todo asiento con sitio, platos y ollas por raciones, estatuas de insecto-humano monumentales, fuentes de plaza
///     (MicroBuildRules.ValidateCivicScale), y las medidas de la plaza de Bolívar frente al ciudadano;
/// (5) las alfombras voladoras del santuario (si está cargado), con su grosor.
/// </summary>
public class MicroSueloEscenasTest : MonoBehaviour, ITestUnit
{
    public int Group => 67;
    public bool ParallelSafe => false;
    [Tooltip("Las escenas de microcosmos que se cargan (una a una; el PC de 16 GB no aguanta todas a la vez).")]
    public string[] scenes = { "Microcosmos_Bolivar", "Microcosmos_Scene1_Ambrosio", "Microcosmos_Hospital", "Microcosmos_Scene0_SakshiRio", "Microcosmos_Cocina" };
    public float loadTimeout = 120f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<MicroSueloEscenasTest>() == null) tr.gameObject.AddComponent<MicroSueloEscenasTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Microcosmos construidos: flora encima, tierra, forma de microcosmos en sincronía, escala cívica y alfombras");
        // (5) Las alfombras del santuario.
        int carpets = 0, thin = 0;
        foreach (FlyingCarpet fc in FlyingCarpet.All)
        {
            if (fc == null) continue; carpets++;
            Bounds b = default; bool any = false;
            foreach (Renderer r in fc.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
            if (!any || b.size.y < 0.06f || fc.transform.Find("Costura_LED") == null) thin++;
        }
        if (carpets > 0) TestProbe.Check("las alfombras voladoras del santuario tienen canto de circuitos y LED (≥ 6 cm)", thin == 0, $"{thin} de {carpets} finas o sin LED: reconstruye la escena principal");

        int seen = 0;
        foreach (string name in scenes)
        {
            Scene s = SceneManager.GetSceneByName(name);
            bool loadedHere = false;
            if (!s.isLoaded)
            {
                string path = null;
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) { string p = SceneUtility.GetScenePathByBuildIndex(i); if (System.IO.Path.GetFileNameWithoutExtension(p) == name) { path = p; break; } }
                if (path == null) { TestProbe.Check($"{name}: está en Build Settings", false, "no está"); continue; }
                AsyncOperation op = SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
                float until = Time.realtimeSinceStartup + loadTimeout;
                while (op != null && !op.isDone && Time.realtimeSinceStartup < until) yield return null;
                if (op == null || !op.isDone) { TestProbe.Check($"{name}: se carga", false, $"no cargó en {loadTimeout:0} s"); continue; }
                s = SceneManager.GetSceneByName(name); loadedHere = true;
                yield return null;
            }
            MicroFloor f = null;
            foreach (GameObject g in s.GetRootGameObjects()) { f = g.GetComponentInChildren<MicroFloor>(true); if (f != null) break; }
            if (f == null) TestProbe.Check($"{name}: tiene su MicroFloor", false, "reconstruye el microcosmos");
            else { Inspect(f, name); seen++; }
            if (loadedHere)
            {
                AsyncOperation un = SceneManager.UnloadSceneAsync(s);
                float until = Time.realtimeSinceStartup + loadTimeout;
                while (un != null && !un.isDone && Time.realtimeSinceStartup < until) yield return null;
            }
        }
        TestProbe.Check("se revisó al menos un microcosmos", seen > 0, $"{seen}");
        TestProbe.End();
    }

    void Inspect(MicroFloor f, string scene)
    {
        Transform root = f.transform;
        Physics.SyncTransforms();
        // (1) La flora encima.
        var flora = MicroGround.ValidateFlora(root, 6);
        TestProbe.Check($"{scene}: ninguna planta enterrada ni dentro de un collider", flora.Count == 0, flora.Count == 0 ? $"{MicroGround.Plants(root).Count} plantas; al construirlo {f.plantsSettled} apoyadas y {f.plantsRemoved} quitadas" : string.Join(" · ", flora));
        // (2) La tierra.
        MicroSoil soil = root.GetComponentInChildren<MicroSoil>(true);
        int soilRenderers = soil != null ? soil.GetComponentsInChildren<Renderer>(true).Length : 0;
        if (MicroGround.SoilEnabled) TestProbe.Check($"{scene}: capa de tierra de 10 cm con cúmulos y piedras, en mosaicos combinados", soil != null && f.soil && soil.tiles > 0 && soilRenderers <= soil.tiles + soil.plates && soilRenderers < 1500,
            soil != null ? $"{soil.tiles} mosaicos, {soil.plates} placas, {soil.mounds} cúmulos, {soil.stones} piedras, {soilRenderers} renderers; {soil.raised} piezas del pueblo subidas" : "sin MicroSoil");
        Transform story = root.Find("Historia");
        if (story != null && soil != null)
        {
            bool over = MicroGround.TopSurface(root, new Vector3(story.position.x, story.position.y + 0.5f, story.position.z), 1f, out float top, out _, story);
            TestProbe.Check($"{scene}: la historia está sobre la tierra (no bajo ella)", !over || story.position.y >= top - 0.12f, over ? $"historia a {story.position.y:0.00}, superficie a {top:0.00}" : "sin superficie bajo el claro");
        }
        // (3) La forma de microcosmos y la sincronía.
        Transform form = root.Find(FloorForms.MicroName);
        int els = form != null ? form.GetComponentsInChildren<MicroElement>(true).Length : 0, want = 0;
        foreach (string k in f.mesoInventory) { int x = k.LastIndexOf('×'); if (x > 0 && int.TryParse(k.Substring(x + 1), out int n)) want += n; }
        TestProbe.Check($"{scene}: «Forma_microcosmos» con una versión de microcosmos por pieza del piso", form != null && els > 0 && els >= want, $"{els} elementos para {want} piezas del piso del santuario (la copia gigante puede llevar más: las partes que el área no cuelga de sí) ({f.elementsMapped} con fila, {f.elementsUnmapped} peñascos); firma {f.mesoSignature}");
        GameObject meso = GameObject.Find(f.platform);
        if (meso != null)
        {
            var miss = MicroFloorElements.Missing(meso.transform, form);
            TestProbe.Check($"{scene}: en sincronía con {f.platform} del santuario (nada del piso sin su versión de microcosmos)", miss.Count == 0, miss.Count == 0 ? "todo" : $"faltan: {string.Join(", ", miss.GetRange(0, Mathf.Min(10, miss.Count)))}: reconstruye el microcosmos");
            string now = MicroFloorElements.Signature(MicroFloorElements.Keys(MicroFloorElements.Inventory(meso.transform)));
            TestProbe.Check($"{scene}: montado con el piso de hoy (firma)", now == f.mesoSignature, $"{f.mesoSignature} al montarlo, {now} hoy");
        }
        // (4) La escala cívica en el pueblo.
        if (story != null && story.GetComponentInChildren<AreaFunctionSite>(true) != null)
        {
            var civic = MicroBuildRules.ValidateCivicScale(story);
            TestProbe.Check($"{scene}: asientos utilizables, platos y ollas por raciones, estatuas de insecto-humano y fuentes de plaza", civic.Count == 0, string.Join(" · ", civic.GetRange(0, Mathf.Min(8, civic.Count))));
            float citizen = MedianCitizen(story);
            foreach (MicroPiece mp in story.GetComponentsInChildren<MicroPiece>(true))
            {
                if (mp.kind != "estatua" && mp.kind != "fuente") continue;
                Bounds b = default; bool any = false;
                foreach (Renderer r in mp.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
                if (!any) continue;
                float size = mp.kind == "estatua" ? b.size.y : Mathf.Max(b.size.x, b.size.z);
                TestProbe.Check($"{scene}: «{mp.name}» ({mp.kind}) más grande que el ciudadano", size >= 1.9f * citizen, $"{size:0.0} m frente a un ciudadano de {citizen:0.00} m");
                break;
            }
            int seats = 0; foreach (MicroSeat ms in story.GetComponentsInChildren<MicroSeat>(true)) seats += ms.points.Length;
            TestProbe.Check($"{scene}: hay asientos públicos con sitio (bancos de la plaza, sillas de los comedores)", seats > 0, $"{seats} sitios");
        }
    }

    static float MedianCitizen(Transform story)
    {
        var l = new List<float>();
        foreach (FolkLineage fl in story.GetComponentsInChildren<FolkLineage>(true)) { var c = fl.GetComponent<CapsuleCollider>(); if (c != null) l.Add(c.height); }
        if (l.Count == 0) return MicroCivicScale.Citizen;
        l.Sort(); return l[l.Count / 2];
    }
}
