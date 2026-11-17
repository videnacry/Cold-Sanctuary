using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// LA PRUEBA EN LA PÁGINA (2026-11-16; Berón: «validar cada microcosmo existente, que los personajes, flora, fauna y demás se muestren bien, en
/// los lugares correctos y con comportamientos correctos … una página por microcosmo existente y futuro»). Con <c>?prueba=1</c> en la URL de
/// una página de la web (<see cref="WebLigera.Prueba"/>), corre EN EL JUGADOR, tras cargar la escena, y escribe el resultado:
///  · en la consola del navegador, una línea <c>[PRUEBA-JSON] {…}</c> y otra <c>[PRUEBA-TITULO] OK</c> o <c>FALLOS n</c>; la plantilla
///    WebLigera las pasa a <c>&lt;pre id="resultado"&gt;</c> y a <c>document.title</c> (así las lee tools/web/probar-web.ps1, Edge sin ventana).
/// Lo que mira, con las mismas reglas que las pruebas del editor (no unas nuevas):
///  · PRESENCIA: la gente (insectos-humanos del pueblo, como CivilizedErasTest: ≥ 3 con jornada, ≥ 2 edificios, nadie con cuerpo humano,
///    estatura 1,3–2,3 m), la flora y la fauna frente a lo que espera su MAPA A MANO (MicroStoryMaps: al menos la mitad de sus plantas y bichos;
///    sin mapa hecho, que haya). En el santuario: animales presentes y no más del tope de la web, gente presente.
///  · SITIO (la regla de la flora de MicroGround, extendida a bichos y gente): nadie enterrado bajo la superficie de encima, nadie DENTRO de un
///    collider, nadie fuera del piso del microcosmos, nadie cayendo (baja más de 2 m mientras se mira).
///  · SIN MAGENTA: ningún material que saliera con el sombreador de error o no soportado (WebLigera.FixBrokenShaders los arregla y los cuenta).
///  · COMPORTAMIENTO: en <see cref="Watch"/> s, una parte de la gente y de los animales se mueve o gira; y los CONTROLES TÁCTILES responden
///    (el joystick de andar, empujado sin dedo, mueve a Kushal).
/// Todas las esperas, acotadas.
/// </summary>
public class WebPrueba : MonoBehaviour
{
    public const float Settle = 6f, Watch = 12f, TouchSecs = 2f, MaxTotal = 120f;
    const int Sample = 300;

    [System.Serializable] public class Check { public string nombre; public bool ok; public string detalle; }
    [System.Serializable] public class Resultado
    {
        public string escena, nombre, version; public bool ok; public int fallos;
        public int gente, flora, fauna, floraEsperada, faunaEsperada; public float segundos;
        public List<Check> checks = new List<Check>();
    }

    Resultado _r;
    bool _done;

    public void Run(string microScene) { if (!_done) StartCoroutine(Go(microScene)); }

    void Add(string nombre, bool ok, string detalle = "") => _r.checks.Add(new Check { nombre = nombre, ok = ok, detalle = detalle ?? "" });

    IEnumerator Go(string micro)
    {
        _done = true;
        float t0 = Time.realtimeSinceStartup;
        _r = new Resultado { escena = micro ?? "SampleScene", nombre = micro != null ? WebLigera.DisplayOf(micro) : "Santuario", version = Application.version };
        Debug.Log($"[Web] Prueba de «{_r.nombre}» ({_r.escena}): empieza.");
        bool inside = micro == null || (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld);
        Add("la escena se abre y Kushal está dentro", inside, micro == null ? "santuario" : (inside ? micro : $"no se entró en {micro}"));
        yield return Wait(Settle);
        // Las tarjetas de historia, cerradas (como lo haría el jugador: E).
        GameKeys.TapVirtual(KeyCode.E);
        yield return Wait(0.5f);

        Scene s = micro != null ? SceneManager.GetSceneByName(micro) : SceneManager.GetActiveScene();
        WebLigera.FixBrokenShaders("prueba");
        if (s.IsValid() && s.isLoaded) { try { Presence(s, micro); } catch (System.Exception e) { Add("presencia y sitio (sin errores)", false, e.Message); } }
        else Add("la escena está cargada", false, _r.escena);

        // COMPORTAMIENTO: quién se mueve en Watch segundos.
        var movers = s.IsValid() && s.isLoaded ? Movers(s, micro != null) : new List<Transform>();
        var p0 = movers.Select(m => m != null ? (m.position, m.rotation) : (Vector3.zero, Quaternion.identity)).ToList();
        yield return Wait(Watch);
        int moved = 0, alive = 0, fell = 0;
        for (int i = 0; i < movers.Count; i++)
        {
            Transform m = movers[i]; if (m == null) continue; alive++;
            float d = Vector3.Distance(m.position, p0[i].Item1), a = Quaternion.Angle(m.rotation, p0[i].Item2);
            if (d > 0.25f || a > 15f) moved++;
            // Cae: bajó más de 2 m Y no tiene suelo debajo (un pájaro que se posa o un pez que baja no cuentan).
            if (p0[i].Item1.y - m.position.y > 2f && !Physics.Raycast(m.position + Vector3.up * 0.5f, Vector3.down, 1000f, ~0, QueryTriggerInteraction.Ignore)) fell++;   // (1000 m: un ave que vuela alto tiene suelo debajo)
        }
        Add($"se mueven o giran en {Watch:0} s (≥ 25 %)", alive == 0 ? false : moved >= Mathf.CeilToInt(alive * 0.25f), $"{moved} de {alive}");
        Add("nadie cae (baja > 2 m mientras se mira)", fell == 0, $"{fell} cayendo");

        // CONTROLES TÁCTILES: el joystick de andar, sin dedo, mueve a Kushal.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        bool hasTouch = FindAnyObjectByType<TouchControls>() != null;
        Add("controles táctiles en pantalla", hasTouch);
        if (player != null && hasTouch)
        {
            Vector3 a0 = player.transform.position;
            TouchControls.TestMove = new Vector2(0f, 1f);
            yield return Wait(TouchSecs);
            TouchControls.TestMove = null;
            float d = Vector3.Distance(player.transform.position, a0);
            Add($"el joystick mueve a Kushal (en {TouchSecs:0} s)", d > 0.3f, $"{d:0.00} m");
        }
        else Add("el joystick mueve a Kushal", false, player == null ? "sin jugador" : "sin controles");

        WebLigera.FixBrokenShaders("prueba, al final");
        Add("ningún material magenta (sombreador de error o no soportado)", WebLigera.MagentaTotal == 0, WebLigera.MagentaTotal == 0 ? "" : $"{WebLigera.MagentaTotal}: {string.Join("; ", WebLigera.MagentaList)}");

        _r.fallos = _r.checks.Count(c => !c.ok);
        _r.ok = _r.fallos == 0;
        _r.segundos = Time.realtimeSinceStartup - t0;
        string json = JsonUtility.ToJson(_r);
        Debug.Log("[PRUEBA-JSON] " + json);
        Debug.Log("[PRUEBA-TITULO] " + (_r.ok ? "OK" : $"FALLOS {_r.fallos}"));
        foreach (Check c in _r.checks) if (!c.ok) Debug.LogWarning($"[Web] Prueba «{_r.nombre}»: FALLA «{c.nombre}» — {c.detalle}");
    }

    static IEnumerator Wait(float secs)
    {
        float until = Time.realtimeSinceStartup + Mathf.Min(secs, MaxTotal);
        while (Time.realtimeSinceStartup < until) yield return null;
    }

    // ── PRESENCIA Y SITIO ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
    void Presence(Scene s, string micro)
    {
        GameObject[] roots = s.GetRootGameObjects();
        Transform root = null;   // la raíz del microcosmos (la que lleva su flora): MicroGround mira solo sus colliders
        foreach (GameObject g in roots) if (g.transform.Find("Flora_a_escala") != null) { root = g.transform; break; }
        Transform rayRoot = root != null && root.name.StartsWith("Microcosmos_") ? root : null;   // (en el MobWorld, la ciudad va en otra raíz)

        var people = new List<CubeInsect>(); var jornadas = new List<MicroJornada>(); var sites = new List<AreaFunctionSite>();
        var animals = new List<Animal>(); int humans = 0;
        foreach (GameObject g in roots)
        {
            // LA GENTE: los del pueblo (folk) y los MIRMIDONES del elenco (insectos-humanos sin Animal: los de los microcosmos de área y de las
            // historias de la tribu, AreaMicrocosmos.DressMyrmidon). Los bichos llevan Animal.
            foreach (CubeInsect ci in g.GetComponentsInChildren<CubeInsect>(true)) if (IsPerson(ci)) people.Add(ci);
            jornadas.AddRange(g.GetComponentsInChildren<MicroJornada>(true));
            sites.AddRange(g.GetComponentsInChildren<AreaFunctionSite>(true));
            foreach (Animal a in g.GetComponentsInChildren<Animal>(true)) if (a != null && !a.CompareTag("Player")) animals.Add(a);
            foreach (Transform t in g.GetComponentsInChildren<Transform>(true)) if (t.name == CubeHumanoid.BodyName && t.parent != null && InsectOnly.IsHuman(t.parent.gameObject)) humans++;
        }
        List<Transform> plants = MicroGround.Plants(root);
        _r.gente = micro != null ? people.Count : CountSanctuaryPeople(roots);
        _r.flora = plants.Count; _r.fauna = animals.Count;

        if (micro != null)
        {
            // Lo que espera (las reglas de CivilizedErasTest y el mapa a mano de MicroStoryMaps).
            Add("nadie con cuerpo humano (todo insectos-humanos)", humans == 0, $"{humans}");
            bool town = sites.Count > 0;
            if (town)
            {
                Add("al menos 2 edificios de la era", sites.Count >= 2, $"{sites.Count}");
                Add("gente con jornada (≥ 3)", jornadas.Count >= 3, $"{jornadas.Count} con jornada, {people.Count} del pueblo");
                string tall = "";
                foreach (CubeInsect ci in people.Where(x => x.folk))   // (la estatura, la de los del pueblo: los mirmidones van en postura de centauro)
                {
                    if (ci.TryGetComponent(out BodyAging grow) && grow.growth < BodyAging.Adult) continue;
                    CapsuleCollider cc = ci.GetComponent<CapsuleCollider>(); if (cc != null && cc.height < 1.5f) continue;
                    Transform ins = ci.transform.Find("Insecto"); if (ins == null) continue;
                    bool any = false; Bounds b = default;
                    foreach (Renderer r in ins.GetComponentsInChildren<Renderer>()) { if (!r.enabled) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
                    if (any && (b.size.y < 1.3f || b.size.y > 2.3f) && !ci.name.StartsWith("Herido")) tall += $"{ci.name} {b.size.y:0.0} m; ";
                }
                Add("todos de estatura parecida (1,3–2,3 m)", tall.Length == 0, tall);
            }
            else Add("hay gente en la escena", people.Count > 0 || jornadas.Count > 0, $"{people.Count}");
            MicroStoryMaps.Map map = MicroStoryMaps.For(_r.nombre);
            bool hand = map != null && map.status == MicroStoryMaps.Status.Hecho;
            _r.floraEsperada = hand ? map.PlantCount : 0; _r.faunaEsperada = hand ? map.AnimalCount : 0;
            if (hand)
            {
                Add($"flora del mapa a mano (≥ la mitad de {map.PlantCount})", plants.Count >= map.PlantCount / 2, $"{plants.Count}");
                Add($"fauna del mapa a mano (≥ la mitad de {map.AnimalCount})", animals.Count >= map.AnimalCount / 2, $"{animals.Count}");
            }
            else
            {
                Add("hay flora", plants.Count > 0, $"{plants.Count} (sin mapa a mano hecho: {(map != null ? map.status.ToString() : "sin mapa")})");
                Add("hay fauna (o el mapa no la pide)", animals.Count > 0 || (map != null && map.status == MicroStoryMaps.Status.Imaginado), $"{animals.Count}");
            }
        }
        else
        {
            Add("hay gente en el santuario", _r.gente > 0, $"{_r.gente}");
            int plan = animals.Count(a => a.SpeciesName != null && PopulationPlan.Groups.ContainsKey(a.SpeciesName));   // (el tope de la web cuenta los del plan, como WebLigera.TrimFauna)
            Add($"hay animales, y los del plan no pasan del tope de la web ({WebLigera.AnimalCap})", animals.Count > 0 && plan <= WebLigera.AnimalCap, $"{plan} del plan de {animals.Count}");
        }

        // SITIO: la regla de la flora de MicroGround (enterrada / dentro de un collider), también para bichos y gente; y dentro del piso.
        Bounds? floor = null;
        if (root != null) { Transform f = root.Find("Suelo_piso"); if (f != null && f.TryGetComponent(out Renderer fr)) floor = fr.bounds; }
        Place("flora", plants, rayRoot, floor, 0.05f, Kind.Flora);
        Place("fauna", animals.Select(a => a.transform).ToList(), rayRoot, floor, 0.3f, Kind.Fauna);
        Place("gente", people.Select(p => p.transform).ToList(), rayRoot, floor, 0.3f, Kind.Gente);
    }

    static int CountSanctuaryPeople(GameObject[] roots)
    {
        int n = 0;
        foreach (GameObject g in roots) foreach (CreatureRig cr in g.GetComponentsInChildren<CreatureRig>(true)) if (ActivityKit.IsHuman(cr.gameObject) && cr.GetComponent<Animal>() == null && !cr.CompareTag("Player")) n++;   // (las personas: ActivityKit.IsHuman)
        return n;
    }

    /// <summary>¿Una persona del microcosmos? Los del pueblo (folk) y los MIRMIDONES del elenco (insectos-humanos sin Animal en ellos ni
    /// encima, fuera de la fauna); no el jugador.</summary>
    static bool IsPerson(CubeInsect ci)
    {
        if (ci == null || ci.CompareTag("Player") || ci.GetComponentInParent<PlayerController>() != null) return false;
        if (ci.folk) return true;
        if (ci.GetComponentInParent<Animal>() != null) return false;
        for (Transform p = ci.transform; p != null; p = p.parent) if (p.name == "Fauna_a_escala") return false;
        return true;
    }

    enum Kind { Flora, Fauna, Gente }

    /// <summary>¿Es un mueble o un edificio (un puesto, una pieza del piso de la historia)? La gente se sienta a la mesa y se pone al
    /// puesto: tocarlos no es estar «dentro» ni «enterrada».</summary>
    static bool Furniture(Collider k)
    {
        for (Transform p = k.transform; p != null; p = p.parent)
            if (p.GetComponent<AreaFunctionSite>() != null || p.GetComponent<AreaFunctionStation>() != null || p.GetComponent<MicroPiece>() != null) return true;
        return false;
    }

    static bool Ground(Collider k) => k != null && (MicroGround.IsSoil(k) || k.name.StartsWith("Suelo") || k.name.Contains("Terreno") || k.name.Contains("Talud") || k.name.Contains("Hielo"));

    /// <summary>¿Qué collider (que no sea él mismo ni lo que la regla de MicroGround ignora) contiene el CENTRO de su cuerpo, 5 cm por
    /// encima (la tolerancia del apoyo)? Para un bicho: posado ENCIMA de un mueble o de una hoja no cuenta; hundido en él, sí.</summary>
    static Collider BodyInside(Transform t, Transform root, bool skipFurniture)
    {
        bool any = false; Bounds b = default;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) { if (!r.enabled) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        Vector3 c = (any ? b.center : t.position) + Vector3.up * 0.05f;
        foreach (Collider k in Physics.OverlapSphere(c, 0.01f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (MicroGround.Ignored(k, t) || MicroGround.IsSoil(k)) continue;
            if (root != null && !k.transform.IsChildOf(root)) continue;
            if (skipFurniture && Furniture(k)) continue;
            return k;
        }
        return null;
    }

    void Place(string what, List<Transform> items, Transform rayRoot, Bounds? floor, float tol, Kind kind)
    {
        if (items.Count == 0) return;
        int step = Mathf.Max(1, items.Count / Sample), looked = 0, buried = 0, inside = 0, insideGround = 0, outside = 0, noGround = 0, onFurniture = 0;
        string first = "";
        Physics.SyncTransforms();
        for (int i = 0; i < items.Count; i += step)
        {
            Transform t = items[i]; if (t == null || !t.gameObject.activeInHierarchy) continue;
            looked++;
            Vector3 p = t.position;
            if (floor.HasValue)
            {
                Bounds f = floor.Value;
                if (p.x < f.min.x - 1f || p.x > f.max.x + 1f || p.z < f.min.z - 1f || p.z > f.max.z + 1f) { outside++; if (first.Length == 0) first = $"{t.name} fuera del piso"; continue; }
            }
            // Una planta «(sobre mueble)» va en la meseta de un mueble gigante a propósito (regla 5): no se le busca suelo (como en las
            // pruebas del pueblo, que tampoco la cuentan como flotante).
            bool onTop = kind == Kind.Flora && t.name.Contains("(sobre mueble)");
            if (onTop) { onFurniture++; continue; }
            if (MicroGround.TopSurface(rayRoot, p, MicroGround.Probe, out float top, out Collider on, t))
            {
                bool counts = !(kind == Kind.Gente && on != null && Furniture(on));   // (la gente, sentada a la mesa: la mesa no la «entierra»)
                if (counts && top > p.y + tol) { buried++; if (first.Length == 0) first = $"{t.name} enterrado ({p.y:0.00} m bajo {(on != null ? on.name : "suelo")} a {top:0.00} m)"; continue; }
            }
            else if (!Physics.Raycast(p + Vector3.up * 0.5f, Vector3.down, 1000f, ~0, QueryTriggerInteraction.Ignore)) { noGround++; if (first.Length == 0) first = $"{t.name} sin suelo debajo"; continue; }
            // DENTRO: la planta, con la regla de MicroGround (su pie); el bicho y la persona, por el centro de su cuerpo (BodyInside).
            Collider k = kind == Kind.Flora ? MicroGround.Inside(rayRoot, t) : BodyInside(t, rayRoot, kind == Kind.Gente);
            if (k != null)
            {
                inside++; if (Ground(k)) insideGround++;
                if (first.Length == 0) first = $"{t.name} dentro de «{k.name}»";
            }
        }
        // Tolerancia: hasta un 5 % (lo mismo que admite la pasada de Settle al asentar en el editor).
        int bad = buried + inside + outside + noGround, allow = Mathf.Max(1, looked / 20);
        Add($"{what}: en su sitio (ni enterrada, ni dentro de objetos, ni fuera del piso, ni sin suelo)", bad <= allow,
            $"{looked} miradas: {buried} enterradas, {inside} dentro ({insideGround} hundidas en el suelo, {inside - insideGround} en muebles o paredes), {outside} fuera, {noGround} sin suelo" +
            $"{(onFurniture > 0 ? $"; {onFurniture} sobre mueble (a propósito)" : "")}{(first.Length > 0 ? " · p. ej. " + first : "")}");
    }

    static List<Transform> Movers(Scene s, bool micro)
    {
        var l = new List<Transform>();
        foreach (GameObject g in s.GetRootGameObjects())
        {
            if (micro) foreach (CubeInsect ci in g.GetComponentsInChildren<CubeInsect>()) { if (IsPerson(ci) && l.Count < 80) l.Add(ci.transform); }
            else foreach (CreatureRig cr in g.GetComponentsInChildren<CreatureRig>()) if (ActivityKit.IsHuman(cr.gameObject) && !cr.CompareTag("Player") && l.Count < 80) l.Add(cr.transform);
            int a = 0;
            foreach (Animal an in g.GetComponentsInChildren<Animal>()) { if (!an.CompareTag("Player") && a++ < 80) l.Add(an.transform); }
        }
        return l;
    }
}
