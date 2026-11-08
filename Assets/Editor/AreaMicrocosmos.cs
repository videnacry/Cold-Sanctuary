using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// EL MICROCOSMOS DE CADA ÁREA (docs/microcosmos-areas.md): es como si de pronto el mundo fuera de insectos. El JUGADOR
/// (≈ 1,8 m) es una HORMIGA, y las hormigas son los HUMANOS del microcosmos: bípedas, con cara e intelecto humano. Todo lo
/// demás está a su escala:
/// - el ÁREA del santuario, ×100 (AreaScale; sus muebles son montañas; la historia y su pueblo, en su BARRIO: el piso × 10);
/// - la flora: el musgo y los hongos hacen de hierba y arbustos; las briznas y los helechos, de árboles;
/// - la fauna de antes del fuego, a tamaño relativo a la hormiga (de pulgón 1 m a mantis 10 m).
/// Cada área tiene su HISTORIA:
/// - Enfermería: la muerte de Ambrosio (alba / cueva).
/// - Cocina (era del fuego): las reencarnaciones. La tribu recoge restos de comida y los lleva a la guarida; la lidera
///   Medea renacida, una hormiga poderosísima, que con Nasatya (Ambrosio renacido) es blanda.
/// - Huerto: La Sembradora (Ruth renacida) enseña a la tribu a cultivar el hongo.
/// - Resto de áreas: la tribu viviendo en su área gigante.
/// EL MAPA ES SOLO EL PISO (docs/reglas-plataformas.md, regla 11; Berón 2026-10-29): el piso elegido ×10, con MUROS en su
/// borde para que nada salga; fuera no se puebla nada. La flora, la fauna, los personajes y las estructuras de la historia van
/// DENTRO (a ×10 los 2 m de paso son 20 m: cabe todo). Registro: «[Microcosmos] escena: dentro N plantas, M fauna…; fuera 0».
/// </summary>
public static class AreaMicrocosmos
{
    public const float AreaScale = 100f;
    /// <summary>La escala con que se hicieron los pueblos y las historias (×10, hasta 2026-11-02): el BARRIO de la historia (lo que
    /// la historia y su pueblo llenan, <see cref="FloorBounds"/>) mide el piso real × Legacy, centrado en la plaza de la historia, y
    /// el resto del piso ×100 es el santuario gigante alrededor. K = AreaScale / Legacy escala las medidas del área gigante.</summary>
    public const float Legacy = 10f, K = AreaScale / Legacy;
    /// <summary>El BARRIO de un pueblo (era avanzada; Berón 2026-11-03: «acceder a mapas de esos pueblos, de modo que se amplía su
    /// estructura»): el piso real × 60 (el 60 % del lado del piso ×100: de 1,4 a 2,8 km), centrado en la plaza de la historia. Ahí
    /// va el pueblo o la ciudad real a escala 1:1 (su plano: calles, plazas, río, lago, colinas; CivicBuilder.TownPlans).</summary>
    public const float TownScale = 60f;
    static int _obraRemoved;
    static List<MicroFloorElements.Piece> _inventory;   // el inventario de la copia gigante del piso del último Build (antes de recortarla)
    const float StoryScale = 2.25f, AntHeight = 1.8f;

    /// <param name="storyOverride">Otra historia en vez de la del área (p. ej. las simulaciones de compasión): construye en
    /// el Transform dado, sobre el suelo del área, y devuelve cuántos personajes vistió.</param>
    internal static void Build(string areaGo, string display, SampleSceneBuilder.MissionKind kind, Color tint, Vector3 o, System.Func<Transform, int> storyOverride = null, MicrocosmSpec spec = null)
    {
        var root = new GameObject($"Microcosmos_{display}");
        _inventory = null;
        string sceneLabel = spec != null ? spec.scene : root.name;
        // (2026-11-04) TIEMPOS por fase: «[Tiempo] escena · fase: N s» (el probador ve dónde se va el tiempo de cada construcción).
        var sw = System.Diagnostics.Stopwatch.StartNew(); var swAll = System.Diagnostics.Stopwatch.StartNew();
        void Phase(string n) { Debug.Log($"[Tiempo] {sceneLabel} · {n}: {sw.Elapsed.TotalSeconds:0.0} s (total {swAll.Elapsed.TotalSeconds:0} s)"); sw.Restart(); }
        // Su FECHA (Eras): las conductas consultan Eras.Allows (sin ordeño de pulgones antes del Neolítico, etc.).
        var era = Eras.OfMicrocosmos(display);
        if (era.HasValue) { MicroEra me = root.AddComponent<MicroEra>(); me.year = era.Value.year; me.place = era.Value.place; me.region = MicroStyles.RegionOf(display).ToString(); }
        if (spec != null) MicrocosmTemplate.Stamp(root, spec);   // la plantilla: fecha, hora y plataforma
        int dressed = 0;

        // 1) El PISO del área, ×10 (sin conducta): el microcosmos ES ese piso visto desde el suelo, con sus muebles convertidos
        //    en montañas, y NADA MÁS (regla 11, Berón 2026-10-29): ni sus anillos, ni lo que asoma fuera de su borde, ni bosque
        //    alrededor. Los otros pisos del edificio son hermanos del área y no se copian.
        GameObject src = GameObject.Find(areaGo), giant = null;
        // (El control de misión está en el Santuario Aéreo: su microcosmos, Hawthorne, necesita esa escena abierta. Se cierra en Finish.)
        if (src == null && System.IO.File.Exists(SampleSceneBuilder.AereoScenePath))
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(SampleSceneBuilder.AereoScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
            src = GameObject.Find(areaGo);
        }
        // (2026-11-08) La Central Nuclear y la Planta Atómica están en el Santuario Marino: sus microcosmos (Chernóbil; Newton, Curie, Einstein) abren esa escena. Se cierra en Finish.
        if (src == null && System.IO.File.Exists(SampleSceneBuilder.MarineScenePath))
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(SampleSceneBuilder.MarineScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
            src = GameObject.Find(areaGo);
        }
        // Sus POSICIONES RESERVADAS (regla 8, PlatformSlots): la historia, los personajes, la fauna y la flora del microcosmos
        // van a las plazas que el piso dejó libres al construirse. Están en locales del área: en el área ×10 (clonada en el
        // origen de la raíz, sin girar), plaza × AreaScale.
        PlatformSlots slots = src != null ? src.GetComponent<PlatformSlots>() : null;
        if (src != null && slots == null) Debug.LogWarning($"[Microcosmos] {display}: «{areaGo}» no tiene posiciones reservadas (PlatformSlots; reconstruye la escena principal): el piso sale de su suelo «Mat».");
        if (src != null)
        {
            giant = Object.Instantiate(src);
            giant.name = $"{display}_gigante";
            CopyAttached(areaGo, src.transform, giant.transform);
            // LA FORMA FINALIZADA DEL PISO (Berón 2026-11-03: «los microcosmos se crean siempre sobre la forma finalizada»;
            // FloorForms): se enciende su «Acabado» (Dojo, Ballet) y se quitan las piezas de obra (andamio, material del día, conos,
            // la forma en obra preparada) ANTES de nada más: lo que se agranda, se despeja para el pueblo y se puebla es el piso
            // terminado. (Antes solo lo hacía la historia de Bruce Lee, DESPUÉS de despejar el barrio, y los conos se quedaban.)
            _obraRemoved = FloorForms.ShowFinished(giant.transform);
            // Sus PLANTAS PROPIAS (otros pisos que cuelgan del área: el de Ashtanga de la Sala de Yoga, las del hospital) no son el
            // mapa (regla 11): fuera, antes de que se pierda su TreeFloor.
            foreach (TreeFloor tf in giant.GetComponentsInChildren<TreeFloor>(true)) if (tf != null && tf.gameObject != giant) Object.DestroyImmediate(tf.gameObject);
            foreach (MonoBehaviour mb in giant.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(mb);
            foreach (string n in new[] { "Estructura_Elevada" }) { Transform t = giant.transform.Find(n); if (t != null) Object.DestroyImmediate(t.gameObject); }
            foreach (Transform t in giant.GetComponentsInChildren<Transform>(true)) if (t != null && t.name.EndsWith("_VirtualizationMachine")) Object.DestroyImmediate(t.gameObject);
            // Los ANILLOS son otras plataformas (el 80 % vuela fuera del piso): no son el mapa.
            foreach (Transform t in giant.GetComponentsInChildren<Transform>(true)) if (t != null && t != giant.transform && t.name.StartsWith("Anillo")) Object.DestroyImmediate(t.gameObject);
            // (2026-11-04) EL INVENTARIO DEL PISO (MicroFloorElements), antes de recortarlo con su borde: cada pieza tendrá su versión de
            // microcosmos (lo recortado se apunta «fuera») y el microcosmos apunta con qué piso del mesocosmos se montó.
            _inventory = MicroFloorElements.Inventory(giant.transform);
            giant.transform.SetParent(root.transform, false);
            giant.transform.localPosition = Vector3.zero; giant.transform.localRotation = Quaternion.identity;
            giant.transform.localScale = Vector3.one * AreaScale;
            foreach (Transform c in giant.transform)
                if (c.name == "Mat" || c.name.StartsWith("Suelo")) c.localPosition = new Vector3(c.localPosition.x, -0.02f, c.localPosition.z);   // su suelo, bajo el resto
            // (×100) La cara de arriba del suelo del área, a ras de 0: a ×100 su losa (de centímetros) mide metros y su cara de arriba
            // quedaba a 13 m; el pueblo y la historia (en y = 0) se quedaban enterrados bajo ella. Se baja el área gigante entera.
            {
                float slabTop = float.MinValue;
                foreach (Transform c in giant.transform)
                    if ((c.name == "Mat" || c.name.StartsWith("Suelo")) && SubtreeBounds(c, out Bounds sb, false)) slabTop = Mathf.Max(slabTop, sb.max.y);
                if (slabTop > 0.05f) giant.transform.localPosition -= Vector3.up * slabTop;
            }
            // Una copia de las plazas en el área gigante (con su escala ×10): las pruebas miran si se usaron.
            if (slots != null)
            {
                PlatformSlots copy = giant.AddComponent<PlatformSlots>();
                copy.story.AddRange(slots.story); copy.characters.AddRange(slots.characters); copy.fauna.AddRange(slots.fauna); copy.flora.AddRange(slots.flora);
                copy.floorSize = slots.floorSize; copy.storyClearance = slots.storyClearance; copy.freeCells = slots.freeCells;
            }
        }
        else Debug.LogWarning($"[Microcosmos] No encuentro {areaGo} (abre la escena principal): sin área gigante, piso liso de 120 × 120 m.");

        // EL RECTÁNGULO DEL PISO (×10): el mapa entero. Lo que del área quede ENTERO fuera de él se quita.
        Bounds rect = giant != null ? FloorRect(giant, slots) : new Bounds(Vector3.zero, new Vector3(120f, 0f, 120f));
        int pruned = giant != null ? Prune(giant.transform, rect) : 0;
        // (2026-11-03) EL PISO NO SE ROMPE (Berón: «en los requisitos de microcosmos debe estar que el piso no debe romperse, las ciudades
        // insecto deben adecuarse al piso y no al revés»): desde aquí ninguna pieza del piso (su forma finalizada, dentro de sus muros)
        // se quita ni se mueve. Se cuentan, y MicroFloors.Validate comprueba que siguen todas.
        int floorPieces0 = giant != null ? giant.GetComponentsInChildren<Renderer>(true).Length : 0;
        Phase("piso gigante e inventario");

        // 1b) ERA AVANZADA (regla 5 de docs/microcosmos-reglas-edificacion.md, MicroBuildRules.IsTownEra): el pueblo o la ciudad
        //     real de la historia ocupa TODO SU BARRIO (×100, 2026-11-02: el barrio es el piso × Legacy alrededor de la plaza de la
        //     historia; el resto del piso es el santuario gigante). Se quitan los muebles gigantes que pisan el barrio (queda su
        //     suelo) y la historia recibe el barrio entero (FloorBounds); se hace en cuanto se sabe dónde va la historia.
        int year0 = spec != null ? spec.year : (era.HasValue ? era.Value.year : Eras.Present);
        bool townEra = giant != null && storyOverride != null && MicroBuildRules.IsTownEra(year0);
        _floorRoot = root.transform; _floorRect = rect;
        // (2026-11-04) LA FORMA DE MICROCOSMOS DEL PISO (MicroFloorDress; Berón: «que tal si usamos las sillas y mesas y demás plataformas
        // para recrear edificios, montañas, mares, acantilados, lagos»): cada pieza, en su sitio, reinterpretada por el catálogo (la mesa,
        // montaña; la silla, nevado; el lavabo, lago; la cajonera, edificio; las hornillas, volcanes; el foco, el sol…) en la era de la historia.
        var (elMapped, elUnmapped) = giant != null && _inventory != null ? MicroFloorDress.Dress(root.transform, giant.transform, _inventory, display, year0, display) : (0, 0);
        Phase("forma de microcosmos (vestir el piso)");

        // 2) Dónde va la HISTORIA: el hueco más libre del SUELO del área (entre mesas, junto a la barra…), no su centro.
        Bounds foot = new Bounds(rect.center, new Vector3(rect.size.x, 1f, rect.size.z));
        var floors = new List<Renderer>(); _blocked.Clear();
        if (giant != null) { Bounds all = Classify(giant, floors, _blocked); foot = new Bounds(new Vector3(rect.center.x, all.center.y, rect.center.z), new Vector3(rect.size.x, all.size.y, rect.size.z)); }
        Vector3 anchor = new Vector3(rect.center.x, 0f, rect.center.z); float free = Mathf.Min(rect.size.x, rect.size.z) * 0.5f;
        // Con posiciones reservadas, la HISTORIA va a su plaza principal (el punto del piso más alejado de los muebles, calculado
        // al construir el piso, ya con sus alfombras); sin ellas, se busca aquí.
        if (giant != null && slots != null && slots.story.Count > 0) { anchor = slots.story[0] * AreaScale; anchor.y = 0f; free = slots.storyClearance * AreaScale; }
        else if (giant != null) anchor = FreeSpot(foot, floors, _blocked, out free);
        // HOSPITAL: la historia va DENTRO, por su pasillo central (30 m de ancho a ×10), empezando junto a la entrada; el hueco
        // «más libre» caía en el borde y la historia quedaba fuera del hospital (Berón, 2026-10-23). Si una plaza de historia cae
        // en el pasillo, cerca de la entrada, va a esa plaza.
        Transform aisle = giant != null && kind == SampleSceneBuilder.MissionKind.Heal ? FindDeep(giant.transform, "Pasillo_central") : null;
        if (aisle != null && aisle.GetComponent<Renderer>() != null)
        {
            Bounds ab = aisle.GetComponent<Renderer>().bounds;
            anchor = new Vector3(ab.center.x, 0f, ab.min.z + 30f); free = ab.size.x;
            float bestD = 15f;
            if (slots != null)
                foreach (Vector3 s in slots.story)
                {
                    Vector3 p = s * AreaScale; p.y = 0f;
                    float d = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(anchor.x, anchor.z));
                    if (InsideXZ(ab, p, -2f) && d < bestD) { bestD = d; anchor = p; }
                }
        }
        anchor = ClampXZ(anchor, Inner(rect, WallInset));
        // EL BARRIO DE LA HISTORIA (×100): el piso real × Legacy (lo que medía el piso entero a ×10), centrado en la plaza de la
        // historia y dentro de los muros. Ahí van la historia, su pueblo (FloorBounds) y la flora densa; el resto del piso ×100
        // es el santuario gigante (sus muebles, montañas de 75 m), con flora y fauna más espaciadas.
        Bounds district = District(rect, anchor, slots, townEra);
        _floorRect = district;
        if (townEra) Debug.Log($"[Microcosmos] {sceneLabel}: era avanzada ({Eras.Format(year0)}): el pueblo se adapta a su barrio ({district.size.x:0} × {district.size.z:0} m) sin tocar los muebles del piso: los rodea o se sube encima.");
        anchor.y = FloorAt(anchor, floors, 0f);
        var story = new GameObject("Historia").transform; story.SetParent(root.transform, false); story.localPosition = anchor;

        // Entrada y salida, dentro del hueco de la historia (el portal muestra su diálogo al acercarse).
        var spawn = new GameObject("MobSpawnPoint"); spawn.transform.SetParent(story, false);
        spawn.transform.localPosition = new Vector3(0f, 1f, -14f); spawn.transform.localRotation = Quaternion.LookRotation(Vector3.forward);
        spawn.AddComponent<MobSpawnPoint>();
        var portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "WorldExitPortal"; portal.transform.SetParent(story, false);
        portal.transform.localPosition = new Vector3(0f, 1.5f, -20f); portal.transform.localScale = new Vector3(2f, 3f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = Mat("MobWorldExitPortal", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<WorldExitPortal>().label = "Salir del microcosmos";
        GalaxyPortal.Dress(portal.transform);   // un agujero negro en una pequeña galaxia (Berón 2026-10-27)

        Transform cast = new GameObject("Elenco").transform; cast.SetParent(story, false);
        if (storyOverride != null) dressed = storyOverride(cast);
        else if (kind == SampleSceneBuilder.MissionKind.Heal) dressed = InfirmaryStory(cast);
        else if (kind == SampleSceneBuilder.MissionKind.KitchenChannel) dressed = KitchenStory(cast);
        else if (kind == SampleSceneBuilder.MissionKind.GardenChannel) dressed = GardenStory(cast);
        else dressed = TribeStory(cast, display, 8);
        // Las historias propias (Patanjali, Planta Atómica, Hospital…) traen su elenco pero ninguna recolectora: sin ellas los
        // mirmidones de ese microcosmos no viven del entorno (semillas; melaza desde el Neolítico, Eras.Dairying). Se añade
        // una cuadrilla pequeña con su guarida, a un lado del claro de la historia (dentro de él: la flora no la tapa).
        // ERA CON ESTRUCTURAS (Berón 2026-11-01: «en todos los mapas se ha creado hormigas-humanas recolectoras con una guarida y eso
        // es erróneo, cada era tiene sus estructuras humanas»): la cuadrilla con guarida solo en las eras SIN estructuras (antes de
        // las aldeas, Eras.HasStructures); en las civilizadas la gente vive de sus edificios (CivicBuilder, docs/microcosmos-civilizaciones.md).
        int year = spec != null ? spec.year : (era.HasValue ? era.Value.year : Eras.Present);
        if ((spec == null || spec.foragers) && !Eras.HasStructures(year) && cast.GetComponentInChildren<MicroForager>(true) == null) dressed += ForagingBand(cast, new Vector3(0f, 0f, 26f), 6);

        // 2b) La historia, DENTRO DEL PISO (regla 11): sus estructuras (cueva, río, sendero, guarida…) no pueden quedar fuera de
        //     los muros. Si se sale por un lado, se corre entera hacia dentro; si es más grande que el piso, pieza a pieza.
        string fit = FitStory(story, cast, Inner(rect, WallInset));
        Phase("historia (y su pueblo)");
        if (fit.Length > 0) Debug.Log($"[Microcosmos] {sceneLabel}: la historia {fit}.");

        // 3) Solo se aparta el mueble que pisa la historia (una mesa, una silla), no el área.
        int moved = giant != null ? TouchingUnder(story, giant.transform) : 0;   // (ya no se apartan: se cuentan)
        // La historia de la enfermería (el sandbox de Ambrosio) borra piezas al montarse: sin quitar sus suelos de la lista,
        // el microcosmos del Hospital fallaba (MissingReference en EdgeDist) y se quedaba la escena vieja, sin el hospital.
        floors.RemoveAll(f => f == null);

        // 4) El SUELO DEL ÁREA es el más bajo bajo su centro (no el más alto: entreplantas y pisos de arriba también son «suelos»;
        //    con el más alto, la flora del suelo real se descartaba y el microcosmos se quedaba pelado, Berón 2026-10-28).
        float groundY = giant != null ? FloorLowest(foot.center, floors, 0f) - 0.02f : 0f;
        MicroBiome.Biome bio = MicroBiome.For(display);
        Color soil = bio != null ? bio.ground : new Color(0.45f, 0.36f, 0.25f);
        // Bajo el piso, una losa de su tierra (arena de Egipto, barro helado de Flandes, mantillo de la taiga…) del tamaño EXACTO
        // del piso y sus muros: tapa los huecos del suelo del área, y nada más (antes era un disco de bosque de cientos de metros).
        Box(root.transform, "Suelo_piso", new Vector3(rect.center.x, groundY - 0.05f, rect.center.z), new Vector3(rect.size.x + WallThick * 2f, 0.1f, rect.size.z + WallThick * 2f), Mat("Micro_Tierra_" + (bio != null ? display : "generica"), soil), true);
        // Los MUROS en el borde del piso: nada sale (ni el jugador, ni la fauna, ni las recolectoras; tapa invisible arriba).
        float wallH = Walls(root.transform, rect, groundY, giant, soil, tint, display);

        // 5) La FLORA a escala de hormiga — musgo = hierba, briznas y helechos = árboles — SOLO dentro del piso: en sus plazas de
        //    flora (×10, a 20 m) y, entre ellas, en una rejilla densa por todo el suelo libre (entre los muebles gigantes), menos
        //    en el claro de la historia. Cada planta es un interactuable (Affordance) que algún insecto usa; y su fauna, también
        //    dentro: primero en sus plazas de fauna y luego en suelo libre.
        Bounds storyCore = new Bounds(story.position, Vector3.zero);   // lo que ocupa la historia (guarida, cueva, bancal, río…): ahí no crece nada
        foreach (Renderer r in story.GetComponentsInChildren<Renderer>(false))
        {
            if (!r.enabled || r.GetComponentInParent<MicroFood>() != null) continue;   // los restos no cuentan; los suelos ocultos tampoco
            // EL PUEBLO (regla 5: ocupa todo el piso) no es el claro: sus edificios, sus calles y lo que está lejos del claro no lo
            // agrandan (si no, el claro era el piso entero y no crecía nada); su huella se aparta pieza a pieza (TownFootprints).
            if (InTown(r.transform, story)) continue;
            storyCore.Encapsulate(r.bounds);
        }
        int townFeet = TownFootprints(story, groundY);
        if (townFeet > 0) Debug.Log($"[Microcosmos] {sceneLabel}: el pueblo ocupa {townFeet} huellas (edificios, calles, árboles); la flora y la fauna crecen entre ellas.");
        bool useSlots = giant != null && slots != null;
        // (×100) Las plazas de FLORA marcan el suelo libre del área: siguen siendo las suyas (×100, por todo el piso; las pruebas
        // las miran: ExperienciaJugadorTest). Las de FAUNA, alrededor de la historia como a ×10 (en su barrio), no esparcidas por kilómetros.
        Vector3 Near(Vector3 s) => slots.story.Count > 0 ? story.localPosition + (s - slots.story[0]) * Legacy : s * AreaScale;
        List<Vector3> floraAt = useSlots ? slots.flora.ConvertAll(s => s * AreaScale) : null, faunaAt = useSlots ? slots.fauna.ConvertAll(Near) : null;
        PlatformSlots giantSlots = giant != null ? giant.GetComponent<PlatformSlots>() : null;
        // EL PUEBLO (era avanzada, regla 5; Berón 2026-11-01: «en el pueblo de Bruce Lee las flores gigantes tapan el pueblo; en
        // Berna el musgo son cubos verdes enormes»): los insectos-humanos viven en un pueblo humano, así que su flora es la de un
        // pueblo — árboles de calle, setos, céspedes, parterres — a escala humana, escasa y fuera de calles y solares (Nature, modo
        // pueblo). La misma condición que quita los muebles gigantes (ClearForTown).
        bool townMode = storyOverride != null && MicroBuildRules.IsTownEra(year0);
        // Las caras de arriba de los muebles gigantes: a ×10 son mesetas, y también se pueblan (en un pueblo ya no hay muebles gigantes).
        // (2026-11-03, reglas de los microcosmos: «todas las estructuras de dicho piso se vuelven superficie utilizable para poblar
        // con insectos-humanos, flora, edificaciones y fauna») también en un pueblo: las de FUERA de su barrio (dentro, el pueblo
        // ya quitó los muebles), con flora y fauna del lugar a escala de pueblo y un mirador de la era en las más grandes.
        List<Bounds> tops = giant != null ? FurnitureTops(giant, groundY, Inner(rect, WallInset)) : null;
        int outposts = townMode && tops != null ? CivicBuilder.Outposts(cast, tops.FindAll(b => !CoveredTop(b)), district) : 0;
        if (outposts > 0)   // sus miradores tapan su meseta: ahí no crece nada (Covered)
            foreach (AreaFunctionSite s in cast.GetComponentsInChildren<AreaFunctionSite>(true))
                if ((s.name.StartsWith(CivicBuilder.OutpostPrefix) || s.name.Contains("barrio alto")) && SubtreeBounds(s.transform, out Bounds ob, false)) { ob.Expand(new Vector3(4f, 0f, 4f)); _blocked.Add(ob); }
        // (Ronda 28) LAS MESETAS SE ANDAN: la forma de microcosmos viste cada mueble gigante de terreno (la mesa, una montaña con su altiplano) y
        // sus mallas sólidas no se hornean (MicroFloorDress, por rendimiento), así que las caras de arriba no tenían NavMesh y los enlaces para
        // trepar no conectaban con nada (Buda: «124 enlaces, 0 con el extremo de arriba sobre el NavMesh»; el mirador, inalcanzable). Una losa
        // invisible y plana en cada cara de arriba, fuera de la copia gigante (sin sus modificadores), entra en el horneado.
        if (giant != null && tops != null)
        {
            Transform mesas = new GameObject("Mesetas_andables").transform; mesas.SetParent(root.transform, false);
            foreach (Bounds top in tops)
            {
                var slab = new GameObject("Meseta"); slab.transform.SetParent(mesas, true);
                slab.transform.position = new Vector3(top.center.x, top.max.y - 0.15f, top.center.z);   // (5 cm por debajo de la cara: la flora que crece encima no queda dentro)
                slab.AddComponent<BoxCollider>().size = new Vector3(top.size.x, 0.2f, top.size.z);
            }
        }
        // (2026-11-04) LA CAPA DE TIERRA (MicroGround, MicroSoilBuilder; Berón: «una capa de superficie … un suelo de tierra, el cual
        // tiene sus irregularidades, cúmulos y piedras»): 10 cm sobre la cara de arriba MÁS ALTA de cada suelo del piso (cubre las
        // alfombras y las losas) y una placa sobre cada mueble poblado; lisa donde está la historia y su pueblo, sin tierra sobre su
        // agua. La historia se sube a la tierra (y los edificios o calles que quedaran por debajo, sobre una alfombra): el pueblo
        // siempre se ve encima del suelo.
        MicroSoil soilLayer = null; System.Func<float, float, float> soilAt = null;
        if (giant != null && MicroSoilBuilder.Enabled)   // (apagada por defecto: MicroGround.SoilEnabled)
        {
            var flat = new List<Bounds>();
            { Bounds sc = storyCore; sc.Expand(new Vector3(8f, 0f, 8f)); flat.Add(sc); }
            if (_hasCore) { Bounds tc = _townCore; tc.Expand(new Vector3(4f, 0f, 4f)); flat.Add(tc); }
            // (las calles quedan dentro del pueblo: no se listan una a una, eran miles de cajas por vértice de tierra)
            List<Bounds> soilTops = tops?.FindAll(b => !TopHasBuilding(b, cast));
            soilLayer = MicroSoilBuilder.Lay(root.transform, rect, district, floors, groundY, soilTops, flat, StoryWater(story), soil, display.GetHashCode(), sceneLabel, out soilAt);
            story.localPosition += Vector3.up * MicroGround.SoilThickness;
            soilLayer.raised = MicroSoilBuilder.RaiseTown(story, floors, groundY);
            Physics.SyncTransforms();
        }
        Phase("pueblo en huellas, mesetas, miradores y tierra");
        int animals = Nature(root.transform, rect, district, groundY, display.GetHashCode(), storyCore, floors, bio,
                             floraAt, faunaAt, giantSlots != null ? giantSlots.floraUnderStory : null, tops, townMode, display, story.position);
        Phase("naturaleza (flora y fauna)");
        // (2026-11-04) LA FLORA ENCIMA, NUNCA DEBAJO (MicroGround; Berón: «la vegetación se está creando bajo el suelo … una de las
        // reglas debería ser que la vegetación se cree encima de cada superficie y nunca debajo»): cada planta y cada bicho, apoyados
        // en la superficie más alta de encima de su pie (un rayo desde arriba: la tierra, la alfombra, la losa del hospital); la que
        // queda dentro de un collider (una pata, un edificio) se quita.
        var surfaces = MicroSoilBuilder.GroundLayers(floors, groundY); if (tops != null) surfaces.AddRange(tops);
        // (2026-11-04) Lo del MAPA A MANO cae DESDE EL AIRE (bajo la tapa de los muros) sobre la superficie más alta de debajo.
        int dropped = _handPlaced.Count > 0 ? MicroGround.Drop(root.transform, _handPlaced, groundY + wallH - 3f, surfaces, soilAt) : 0;
        if (dropped > 0) Debug.Log($"[Mapa] {sceneLabel}: {dropped} plantas y bichos del mapa a mano caídos desde el aire sobre su superficie.");
        _handPlaced.Clear();
        var (settledPl, removedPl) = MicroGround.Settle(root.transform, surfaces, soilAt);
        Phase("asentar y caer desde el aire (Settle, Drop)");
        Debug.Log($"[Tierra] {sceneLabel}: {settledPl} plantas y bichos apoyados en la superficie de encima, {removedPl} plantas quitadas (dentro de un collider).");

        // Los PERSONAJES que andan por el área (las recolectoras de la tribu, MicroForager) empiezan en sus plazas de personaje,
        // alrededor de la historia; los de papel fijo (la líder, Nasatya, la Sembradora…) siguen junto a su estructura.
        int cast0 = 0;
        if (useSlots)
            foreach (MicroForager mf in cast.GetComponentsInChildren<MicroForager>(true))
            {
                if (cast0 >= slots.characters.Count) break;
                if (!mf.enabled) continue;   // papeles guionizados (los porteadores del mito empiezan quietos, en su sitio)
                // Alrededor de la historia a la distancia de antes (×Legacy: 30–80 m), no a ×100 (300–800 m, fuera de su barrio).
                Vector3 p = slots.story.Count > 0 ? story.localPosition + (slots.characters[cast0++] - slots.story[0]) * Legacy : slots.characters[cast0++] * AreaScale;
                p.y = FloorNear(p, floors, p.y);
                mf.transform.position = root.transform.TransformPoint(p);
            }
        if (useSlots) Debug.Log($"[Plazas] Microcosmos {display}: historia en su plaza ({anchor.x:0}, {anchor.z:0}), {cast0}/{slots.characters.Count} personajes, flora en {slots.flora.Count - (giantSlots != null ? giantSlots.floraUnderStory.Count : 0)}/{slots.flora.Count} plazas (el resto bajo la historia), fauna en {_faunaOnSlots}/{slots.fauna.Count} plazas.");

        // 6) NADA FUERA DE LOS MUROS: cada planta, bicho, resto de comida y personaje que asome por el borde se corre hacia dentro;
        //    luego se cuenta lo de dentro y lo de fuera (debe ser 0) y queda apuntado en la raíz (MicroFloor) para las pruebas.
        int nudged = 0;
        foreach (Transform t in Population(root.transform, story)) if (Contain(t, Inner(rect, 1f))) nudged++;
        MicroFloor mfl = root.AddComponent<MicroFloor>();
        mfl.center = new Vector3(rect.center.x, groundY, rect.center.z); mfl.size = new Vector2(rect.size.x, rect.size.z); mfl.wallHeight = wallH;
        mfl.districtCenter = new Vector3(district.center.x, groundY, district.center.z); mfl.districtSize = new Vector2(district.size.x, district.size.z);
        mfl.furnitureTops = tops != null ? tops.Count : 0; mfl.floraOnTops = _floraOnTops; mfl.faunaOnTops = _faunaOnTops; mfl.town = townMode;
        mfl.buildingsOnTops = outposts;
        mfl.soil = soilLayer != null; mfl.plantsSettled = settledPl; mfl.plantsRemoved = removedPl;
        mfl.elementsMapped = elMapped; mfl.elementsUnmapped = elUnmapped;
        if (_inventory != null)
        {   // LAS DEPENDENCIAS (MicroFloorElements.Record): con qué piso del mesocosmos se montó; si el piso cambia, se avisa para revisarlo.
            // (la firma, la del piso del SANTUARIO tal cual: la copia gigante lleva además las partes que el área no cuelga de sí, Attached)
            var mesoInv = src != null ? MicroFloorElements.Inventory(src.transform) : _inventory;   // (una sola vez)
            mfl.mesoInventory = MicroFloorElements.Keys(mesoInv); mfl.mesoSignature = MicroFloorElements.Signature(mfl.mesoInventory);
            MicroFloorElements.Record(sceneLabel, areaGo, mfl.mesoInventory);
            if (src != null)
            {   // LA SINCRONÍA con el piso del santuario tal como está ahora: nada suyo sin su versión de microcosmos.
                var have = new HashSet<string>(); foreach (var p in _inventory) have.Add(p.key);   // (cada pieza de la copia gigante tiene su MicroElement)
                var miss = new List<string>(); foreach (var p in mesoInv) if (!have.Contains(p.key) && !miss.Contains(p.key)) miss.Add(p.key);
                if (miss.Count > 0) Debug.LogWarning($"[Forma microcosmos] {sceneLabel}: {miss.Count} piezas de {areaGo} sin su versión de microcosmos: {string.Join(", ", miss.GetRange(0, Mathf.Min(10, miss.Count)))}.");
            }
            _inventory = null;
        }
        // EL PISO AL QUE PERTENECE (reglas de los microcosmos, docs/microcosmos-areas.md §Reglas): montado ENCIMA de ese piso del
        // santuario, en su forma finalizada; y si el piso no representa la historia, «pendiente de piso representativo».
        mfl.platform = areaGo; mfl.finishedForm = giant != null; mfl.obraRemoved = _obraRemoved;
        mfl.floorPieces = floorPieces0;
        MicroFloors.Match match = MicroFloors.Of(display);
        if (match != null) { mfl.representative = match.representative; mfl.pendingFloor = match.pending; }
        if (giant != null && FloorForms.ObraLeft(giant.transform).Count > 0) { mfl.finishedForm = false; Debug.LogWarning($"[Microcosmos] {sceneLabel}: quedan piezas de obra en el piso: {string.Join(", ", FloorForms.ObraLeft(giant.transform))}."); }
        Census(root.transform, story, mfl);
        Debug.Log($"[Microcosmos] {sceneLabel}: dentro {mfl.floraInside} plantas, {mfl.faunaInside} fauna, {mfl.charactersInside} personajes; encima de {mfl.furnitureTops} muebles {mfl.floraOnTops} plantas y {mfl.faunaOnTops} bichos; fuera {mfl.outside} (piso {rect.size.x:0} × {rect.size.z:0} m, muros de {wallH:0} m{(nudged > 0 ? $", {nudged} piezas corridas hacia dentro" : "")}{(pruned > 0 ? $", {pruned} piezas del área fuera del piso quitadas" : "")}).");
        if (mfl.outside > 0) Debug.LogWarning($"[Microcosmos] {sceneLabel}: {mfl.outside} piezas de flora, fauna o historia FUERA de los muros (regla 11).");

        // EL BLOQUEO DE HUMANOS (InsectOnly, docs/microcosmos-civilizaciones.md §3): ninguna escena de microcosmos se guarda con un
        // cuerpo humano (un personaje de historia vestido de humano se convierte en su insecto).
        InsectOnly.EnforceRoot(root.transform);
        Phase("censo, dependencias y bloqueo de humanos");

        root.transform.position = o;

        // Las misiones del área (yoga, textil, mecánica) siguen en su tablero.
        if (storyOverride == null && (kind == SampleSceneBuilder.MissionKind.Yoga || kind == SampleSceneBuilder.MissionKind.Channel))
        {
            MobMission[] missions = SampleSceneBuilder.BuildAreaMissions(root.transform, root.transform, display, tint, kind);
            foreach (MobMission m in missions) { if (m == null) continue; m.transform.position = story.TransformPoint(new Vector3(0f, 0f, 6f)); foreach (MeditationMissionBase b in m.GetComponents<MeditationMissionBase>()) b.endMode = MissionEndMode.Standalone; }
            var board = root.AddComponent<MobWorldMissionBoard>(); board.areaName = display; board.missions = missions;
        }

        var navGo = new GameObject("NavMesh_Microcosmos"); navGo.transform.SetParent(root.transform, false);
        navGo.transform.localPosition = new Vector3(rect.center.x, 0f, rect.center.z);
        var nav = navGo.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
        nav.collectObjects = Unity.AI.Navigation.CollectObjects.Volume;
        // Solo lo SÓLIDO (colliders): los decorados sin colisión (alfombras, ríos pintados, losas ocultas…) entraban como
        // escalones y partían el suelo en islas (Patanjali: la guarida y el claro de la historia, aislados del resto).
        nav.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
        Physics.SyncTransforms();   // con colliders, el horneado consulta la física: sin sincronizar, no ve lo recién creado y movido
        // El volumen: el piso y sus muros (los muros no se andan: NavMeshModifier), nada de fuera.
        nav.center = Vector3.up * 15f * K; nav.size = new Vector3(rect.size.x + WallThick * 2f, 60f * K, rect.size.z + WallThick * 2f);
        // (2026-11-05, ronda 8: el horneado es el 95 % de rehacer un microcosmos: Cocina, 445 de 473 s) El vóxel por defecto es radio/3 (0,167 m) en
        // un piso de kilómetros. 0,25 m (2 vóxeles por radio, el mínimo que aconseja Unity) cuesta ~44 %; los pasos de 2 m siguen abiertos.
        nav.overrideVoxelSize = true; nav.voxelSize = MicroNavVoxel;
        nav.BuildNavMesh();
        Phase("NavMesh");
        // (Ronda 29) ¿HAY NAVMESH EN LAS MESETAS? Justo tras hornear: cuántos vértices del NavMesh de este piso están a más de 10 m del suelo, y
        // tres caras de arriba muestreadas (en el mundo).
        if (giant != null && tops != null)
        {
            var tri = UnityEngine.AI.NavMesh.CalculateTriangulation();
            Bounds wr = new Bounds(root.transform.TransformPoint(rect.center), rect.size + Vector3.up * 1000f);
            float gy = root.transform.TransformPoint(new Vector3(0f, groundY, 0f)).y;
            int mine = 0, high = 0; foreach (Vector3 v in tri.vertices) { if (!InsideXZ(wr, v, 0f)) continue; mine++; if (v.y > gy + 10f) high++; }
            var samples = new List<string>();
            for (int i = 0; i < tops.Count && samples.Count < 3; i += Mathf.Max(1, tops.Count / 3))
            {
                Vector3 w = root.transform.TransformPoint(tops[i].center);
                samples.Add(UnityEngine.AI.NavMesh.SamplePosition(w, out UnityEngine.AI.NavMeshHit sh, 3f, UnityEngine.AI.NavMesh.AllAreas) ? $"({w.x:0},{w.y:0},{w.z:0}) → NavMesh a y {sh.position.y:0.0}" : $"({w.x:0},{w.y:0},{w.z:0}) → nada a 3 m");
            }
            Debug.Log($"[Microcosmos] {display}: NavMesh tras hornear: {mine} vértices en el piso, {high} a más de 10 m del suelo (mesetas); {tops.Count} caras de arriba; muestras: {string.Join(" · ", samples)}.");
        }
        // (2026-11-03) TREPAR: los insectos-humanos suben por las patas y los costados de los muebles del piso (Berón: «pueden trepar
        // plataformas verticales como las paredes y patas de sillas, mesas, etc.»). Un NavMeshLink del suelo a cada cara de arriba (la IA
        // lo recorre trepando) y un Climbable en cada mueble alto (el jugador trepa con PlayerClimber).
        int climbs = 0;
        if (giant != null && tops != null)
        {
            Transform lr = new GameObject("Trepadas").transform; lr.SetParent(root.transform, false);
            Bounds inside = Inner(rect, 3f);
            var lost = new List<string>();
            int snapped = 0;
            var upEnds = new List<(Bounds top, Vector3 up)>();   // (ronda 29) el extremo de arriba de cada meseta, para coser lo que haya encima
            foreach (Bounds top in tops)
            {
                // El pie de la trepada, por el lado que quede dentro de los muros: antes solo se probaban los dos lados en z, y una meseta junto a
                // una pared en z se quedaba sin enlace (probablemente el mirador de Microcosmos_Buda, a 224 m: «alcanzable False»; el aviso de abajo lo dirá).
                Vector3 c = top.center, up = default, down = default; bool found = false;
                foreach (Vector3 dir in new[] { Vector3.back, Vector3.forward, Vector3.left, Vector3.right })
                {
                    float half = Mathf.Abs(dir.x) > 0f ? top.extents.x : top.extents.z;
                    up = new Vector3(c.x, top.max.y, c.z) + dir * (half - 1.5f); down = new Vector3(c.x, groundY, c.z) + dir * (half + 2f);
                    if (InsideXZ(inside, down, 0f)) { found = true; break; }
                }
                if (!found) { lost.Add($"({c.x:0}, {top.max.y:0}, {c.z:0})"); continue; }
                // Los extremos, SOBRE el NavMesh ya horneado: la meseta del mirador de Buda se anda a y 237 y la cara de arriba de su mueble
                // da 233; con el extremo 4 m por debajo el enlace no conectaba («alcanzable False», ronda 25).
                // (Ronda 26) Solo si hay NavMesh a mano: en el Editor, recién horneado, no lo había en ninguna de las 124 caras y se quitaron todos
                // los enlaces. Sin él, el extremo de la cara del mueble, como antes.
                // (Ronda 29) Las caras se midieron con la raíz AÚN EN EL ORIGEN y el NavMesh se horneó ya en su sitio (root.position = o): se busca
                // en el mundo y se vuelve a las coordenadas de la raíz. Antes se buscaba en las del origen y nunca había nada («0 con el extremo…»).
                if (UnityEngine.AI.NavMesh.SamplePosition(root.transform.TransformPoint(up), out UnityEngine.AI.NavMeshHit hu, 6f, UnityEngine.AI.NavMesh.AllAreas)) { up = root.transform.InverseTransformPoint(hu.position); snapped++; }
                if (UnityEngine.AI.NavMesh.SamplePosition(root.transform.TransformPoint(down), out UnityEngine.AI.NavMeshHit hd, 3f, UnityEngine.AI.NavMesh.AllAreas)) down = root.transform.InverseTransformPoint(hd.position);
                var g = new GameObject("Trepar"); g.transform.SetParent(lr, false);
                var link = g.AddComponent<Unity.AI.Navigation.NavMeshLink>();
                // Los extremos de un NavMeshLink van en el espacio local de su objeto, que es el de la raíz (el enlace cuelga de ella sin moverse), y
                // up/down ya están en ese espacio (se midieron con la raíz en el origen). (Ronda 29: el InverseTransformPoint de la ronda 27 restaba la
                // posición de la raíz otra vez y desplazaba cada enlace (−7300, …) en Buda: fallo mío.)
                link.startPoint = down; link.endPoint = up; link.width = 2f; link.bidirectional = true; link.costModifier = 4f;
                climbs++; upEnds.Add((top, up));
            }
            if (lost.Count > 0) Debug.LogWarning($"[Microcosmos] {display}: {lost.Count} de {tops.Count} caras de arriba sin enlace para trepar (ningún lado con el pie dentro de los muros): {string.Join(", ", lost)}.");
            // (Ronda 29) CADA MIRADOR Y CASA DEL BARRIO ALTO, ALCANZABLE: CalculatePath desde la plaza (el centro del piso, en el suelo) hasta su
            // NavMesh. El mirador de Buda (y 233) seguía sin alcanzarse con los 124 enlaces conectados: su suelo es otra isla encima de su meseta.
            // Si no se llega, se cose con un enlace desde el extremo de arriba de la trepada de esa meseta hasta el NavMesh del edificio, y se mide otra vez.
            Vector3 plazaW = root.transform.TransformPoint(new Vector3(rect.center.x, groundY, rect.center.z));
            if (UnityEngine.AI.NavMesh.SamplePosition(plazaW, out UnityEngine.AI.NavMeshHit ph, 30f, UnityEngine.AI.NavMesh.AllAreas))
                foreach (AreaFunctionSite site in cast.GetComponentsInChildren<AreaFunctionSite>(true))
                {
                    if (!site.name.StartsWith(CivicBuilder.OutpostPrefix) && !site.name.Contains("barrio alto")) continue;
                    Vector3 siteW = site.transform.position;
                    if (!UnityEngine.AI.NavMesh.SamplePosition(siteW, out UnityEngine.AI.NavMeshHit sh, 6f, UnityEngine.AI.NavMesh.AllAreas)) { Debug.LogWarning($"[Microcosmos] {display}: «{site.name}» en {siteW}: sin NavMesh a 6 m."); continue; }
                    var path = new UnityEngine.AI.NavMeshPath();
                    UnityEngine.AI.NavMesh.CalculatePath(ph.position, sh.position, UnityEngine.AI.NavMesh.AllAreas, path);
                    string before = $"{path.status}";
                    if (path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    {
                        Vector3 sl = root.transform.InverseTransformPoint(siteW);
                        (Bounds top, Vector3 up) best = default; float bd = float.MaxValue; bool any = false;
                        foreach (var ue in upEnds)
                        {
                            if (!InsideXZ(ue.top, sl, 0.5f)) continue;
                            float dd = Mathf.Abs(ue.top.max.y - sl.y);
                            if (dd < bd) { bd = dd; best = ue; any = true; }
                        }
                        if (any)
                        {
                            var sg = new GameObject("Subir_al_edificio"); sg.transform.SetParent(lr, false);
                            var sl2 = sg.AddComponent<Unity.AI.Navigation.NavMeshLink>();
                            sl2.startPoint = best.up; sl2.endPoint = root.transform.InverseTransformPoint(sh.position); sl2.width = 2f; sl2.bidirectional = true; sl2.costModifier = 2f;
                            sl2.UpdateLink();
                            UnityEngine.AI.NavMesh.CalculatePath(ph.position, sh.position, UnityEngine.AI.NavMesh.AllAreas, path);
                        }
                    }
                    // (Ronda 29) SI SIGUE SIN LLEGARSE: a la meseta libre más alta a la que sí se llega (y que le cabe); si no hay ninguna, el horario
                    // libre no lo usa (AreaFunctionSite.unreachable). El mirador de Buda a y 233 quedaba «PathPartial» aun cosido.
                    string relocated = "";
                    if (path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    {
                        var taken = new List<Vector3>();
                        foreach (AreaFunctionSite o2 in cast.GetComponentsInChildren<AreaFunctionSite>(true)) if (o2 != site) taken.Add(root.transform.InverseTransformPoint(o2.transform.position));
                        foreach (var ue in upEnds.OrderByDescending(x => x.top.max.y))
                        {
                            if (ue.top.size.x < 16f || ue.top.size.z < 14f) continue;
                            if (taken.Any(p => InsideXZ(ue.top, p, 0f) && Mathf.Abs(p.y - ue.top.max.y) < 3f)) continue;
                            Vector3 cw = root.transform.TransformPoint(new Vector3(ue.top.center.x, ue.top.max.y, ue.top.center.z));
                            if (!UnityEngine.AI.NavMesh.SamplePosition(cw, out UnityEngine.AI.NavMeshHit ch, 3f, UnityEngine.AI.NavMesh.AllAreas)) continue;
                            var p2 = new UnityEngine.AI.NavMeshPath();
                            if (!UnityEngine.AI.NavMesh.CalculatePath(ph.position, ch.position, UnityEngine.AI.NavMesh.AllAreas, p2) || p2.status != UnityEngine.AI.NavMeshPathStatus.PathComplete) continue;
                            Vector3 from = site.transform.position; site.transform.position = cw; path = p2;
                            relocated = $" → recolocado de y {from.y:0} a la meseta de y {cw.y:0}: {p2.status}";
                            break;
                        }
                        if (path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete) { site.unreachable = true; relocated = " → ninguna meseta libre alcanzable: el horario libre no lo usa"; }
                    }
                    Debug.Log($"[Microcosmos] {display}: de la plaza a «{site.name}» (NavMesh a y {sh.position.y:0.0}): {before}" + relocated + (before == $"{path.status}" && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete ? "" : $" → con el enlace al edificio: {path.status}") + ".");
                }
            Debug.Log($"[Microcosmos] {display}: {climbs} enlaces para trepar, {snapped} con el extremo de arriba sobre el NavMesh (el resto, en la cara del mueble).");
            foreach (Collider c in giant.GetComponentsInChildren<Collider>())
            {
                Bounds b = c.bounds;
                if (IsFloor(b) || b.size.y < 2f || c.GetComponent<Climbable>() != null) continue;
                c.gameObject.AddComponent<Climbable>().topY = b.max.y;
            }
        }
        mfl.climbLinks = climbs;
        Phase("enlaces para trepar");
        Debug.Log($"[Microcosmos] {display}: piso ×{AreaScale} con muros (se apartan {moved} muebles bajo la historia, hueco libre {free:0} m entre {_blocked.Count} muebles), {dressed} mirmidones e insectos de la historia y {animals} animales a escala de hormiga.");
    }

    // ── El PISO: su rectángulo, lo que sobra fuera, los muros y lo que queda dentro (regla 11) ───────────────────────────

    const float WallThick = 4f;    // grosor de los muros (m, ya a escala de hormiga)
    const float WallInset = 6f;    // las estructuras de la historia y la flora, a esta distancia del muro como mínimo
    const float WallMinHeight = 80f;

    /// <summary>El rectángulo del PISO del área gigante (en el espacio de la raíz, aún en el origen): el tamaño con que se
    /// calcularon sus plazas (PlatformSlots.floorSize, centrado en el origen del área) ×10; sin plazas, su suelo «Mat»; y si no,
    /// el plano más grande del área.</summary>
    static Bounds FloorRect(GameObject giant, PlatformSlots slots)
    {
        Vector3 c0 = giant.transform.position;
        if (slots != null && slots.floorSize.x > 1f && slots.floorSize.y > 1f)
            return new Bounds(new Vector3(c0.x, 0f, c0.z), new Vector3(slots.floorSize.x * AreaScale, 0f, slots.floorSize.y * AreaScale));
        Transform mat = giant.transform.Find("Mat");
        Renderer mr = mat != null ? mat.GetComponent<Renderer>() : null;
        if (mr != null) return new Bounds(new Vector3(mr.bounds.center.x, 0f, mr.bounds.center.z), new Vector3(mr.bounds.size.x, 0f, mr.bounds.size.z));
        Bounds best = new Bounds(new Vector3(c0.x, 0f, c0.z), new Vector3(120f, 0f, 120f)); float area = 0f;
        foreach (Renderer r in giant.GetComponentsInChildren<Renderer>())
        {
            Bounds b = r.bounds;
            if (!IsFloor(b) || b.size.x * b.size.z <= area) continue;
            area = b.size.x * b.size.z; best = new Bounds(new Vector3(b.center.x, 0f, b.center.z), new Vector3(b.size.x, 0f, b.size.z));
        }
        return best;
    }

    /// <summary>El rectángulo encogido <paramref name="inset"/> m por cada lado (nunca menos de 10 m de lado).</summary>
    /// <summary>¿Es <paramref name="t"/> parte del PUEBLO de la historia y no de su claro? Lo es si está bajo un edificio
    /// (AreaFunctionSite), bajo unas «Calles de …», o si la pieza de la historia a la que pertenece (el hijo del «Elenco») queda a
    /// más de 30 m del centro del claro.</summary>
    static bool InTown(Transform t, Transform story)
    {
        if (t.GetComponentInParent<AreaFunctionSite>(true) != null) return true;
        Transform top = null;
        for (Transform p = t; p != null && p != story; p = p.parent)
        {
            if (p.name.StartsWith("Calles de") || p.name.StartsWith("Calles ") || p.name.StartsWith("Paisaje de")) return true;
            if (p.parent != null && p.parent.name == "Elenco") top = p;
        }
        if (top == null) return false;
        Vector3 d = top.position - story.position; d.y = 0f;
        return d.magnitude > 30f;
    }

    /// <summary>Las huellas del PUEBLO (cada edificio, cada tramo de calle, cada pieza lejos del claro) se añaden a lo que
    /// tapa el suelo (<see cref="_blocked"/>, 1 m de alto): ni la flora ni la fauna nacen encima. Devuelve cuántas.</summary>
    static int TownFootprints(Transform story, float groundY)
    {
        int n = 0;
        var seen = new HashSet<Transform>();
        _townFeet.Clear(); _streets.Clear(); _townCore = default; _hasCore = false;
        foreach (Renderer r in story.GetComponentsInChildren<Renderer>(false))
        {
            if (!r.enabled || r.GetComponentInParent<CubeInsect>(true) != null || !InTown(r.transform, story)) continue;   // la gente anda: no es huella
            AreaFunctionSite site = r.GetComponentInParent<AreaFunctionSite>(true);
            if (site != null)
            {
                if (!seen.Add(site.transform)) continue;
                bool any = false; Bounds b = default;
                foreach (Renderer rr in site.GetComponentsInChildren<Renderer>(false)) { if (rr.GetComponentInParent<CubeInsect>(true) != null) continue; if (!any) { b = rr.bounds; any = true; } else b.Encapsulate(rr.bounds); }
                if (!any) continue;
                var foot = new Bounds(new Vector3(b.center.x, groundY + 0.5f, b.center.z), new Vector3(b.size.x + 2f, 1f, b.size.z + 2f));
                _blocked.Add(foot); _townFeet.Add(foot); n++; if (!_hasCore) { _townCore = foot; _hasCore = true; } else _townCore.Encapsulate(foot);
            }
            else
            {
                Bounds b = r.bounds;
                var foot = new Bounds(new Vector3(b.center.x, groundY + 0.5f, b.center.z), new Vector3(b.size.x, 1f, b.size.z));
                _blocked.Add(foot); _townFeet.Add(foot); n++;
                if (IsStreet(r.transform, story)) _streets.Add(foot);
            }
        }
        return n;
    }

    /// <summary>(2026-11-04) El AGUA de la historia (ríos, lagos, el mar del plano, el agua pintada de una orilla): sobre ella no va
    /// la capa de tierra. Las piezas planas cuyo nombre o material dice agua.</summary>
    static List<Bounds> StoryWater(Transform story)
    {
        var l = new List<Bounds>();
        foreach (Renderer r in story.GetComponentsInChildren<Renderer>(true))
        {
            Bounds b = r.bounds;
            if (b.size.y > 3f || b.size.x * b.size.z < 4f) continue;
            string n = r.name.ToLowerInvariant();
            bool water = n.Contains("agua") || n.Contains("lago") || n.Contains("río") || n.Contains("rio_") || n.Contains("water") || n.Contains("mar_");
            if (!water) foreach (Material m in r.sharedMaterials) if (m != null && (m.name.StartsWith("Relieve_agua") || m.name.StartsWith("Relieve_mar") || m.name.Contains("Agua"))) { water = true; break; }
            if (water) l.Add(b);
        }
        return l;
    }

    /// <summary>¿Hay un edificio (un mirador del barrio alto) sobre la cara de arriba <paramref name="top"/>? Ahí no va la placa de tierra.</summary>
    static bool TopHasBuilding(Bounds top, Transform cast)
    {
        foreach (AreaFunctionSite s in cast.GetComponentsInChildren<AreaFunctionSite>(true))
        {
            Vector3 p = s.transform.position;
            if (p.x > top.min.x && p.x < top.max.x && p.z > top.min.z && p.z < top.max.z && Mathf.Abs(p.y - top.max.y) < 3f) return true;
        }
        return false;
    }

    // Las huellas del pueblo y, de ellas, las calles (el último TownFootprints): la flora del pueblo crece junto a ellas, sin pisarlas,
    // y los animales del pueblo andan por las calles.
    static readonly List<Bounds> _townFeet = new List<Bounds>(), _streets = new List<Bounds>();
    static Bounds _townCore; static bool _hasCore;   // lo que ocupan los EDIFICIOS del pueblo (la rejilla fina de su flora)

    // ¿Es una calle (un tramo de «Calles de …», o el asfalto de un barrio: CivicBuilder.Street, Towns.Town)? Se pisa: es plana.
    static bool IsStreet(Transform t, Transform story)
    {
        for (Transform p = t; p != null && p != story; p = p.parent) if (p.name.StartsWith("Calle")) return true;
        return false;
    }

    // El piso del microcosmos que se está construyendo (para FloorBounds mientras corre su historia).
    static Transform _floorRoot; static Bounds _floorRect;

    /// <summary>
    /// EL RECTÁNGULO ÚTIL DEL PISO del microcosmos al que pertenece <paramref name="anyInMicrocosm"/> (cualquier transform bajo
    /// su raíz: el «Elenco» que recibe la historia, por ejemplo), EN EL MUNDO: el piso ×10 dentro de sus muros menos la franja de
    /// <see cref="WallInset"/> m (lo que la historia puede ocupar sin que FitStory la corra). size.y = 0; center.y = la de la raíz
    /// (el suelo). Sin área gigante (MobWorld_Mesopotamia, pruebas): 120 × 120 m centrados en la raíz, menos la franja.
    /// Vale mientras se construye la historia (la raíz aún en el origen) y después (lee MicroFloor).
    /// </summary>
    public static Bounds FloorBounds(Transform anyInMicrocosm)
    {
        Transform root = anyInMicrocosm != null ? anyInMicrocosm.root : null;
        Bounds rect;   // en locales de la raíz (no gira ni escala)
        if (root != null && root == _floorRoot) rect = _floorRect;
        else
        {
            MicroFloor mf = root != null ? root.GetComponent<MicroFloor>() : null;
            Transform giant = null;
            if (root != null) foreach (Transform c in root) if (c.name.EndsWith("_gigante")) { giant = c; break; }
            if (mf != null && mf.districtSize.x > 1f) rect = new Bounds(new Vector3(mf.districtCenter.x, 0f, mf.districtCenter.z), new Vector3(mf.districtSize.x, 0f, mf.districtSize.y));
            else if (mf != null) rect = new Bounds(new Vector3(mf.center.x, 0f, mf.center.z), new Vector3(mf.size.x, 0f, mf.size.y));
            else if (giant != null) { rect = FloorRect(giant.gameObject, giant.GetComponent<PlatformSlots>()); rect.center -= root.position; }
            else rect = new Bounds(Vector3.zero, new Vector3(120f, 0f, 120f));
        }
        Bounds inner = Inner(rect, WallInset);
        Vector3 c0 = new Vector3(inner.center.x, 0f, inner.center.z);
        if (root != null) c0 += root.position;
        return new Bounds(c0, new Vector3(inner.size.x, 0f, inner.size.z));
    }

    /// <summary><see cref="FloorBounds"/> en los LOCALES XZ de <paramref name="t"/> (sin giro ni escala: el «Elenco» de una
    /// historia): el rectángulo que la historia puede llenar, con (0, 0) en el propio origen de <paramref name="t"/>.</summary>
    public static Rect FloorRectLocal(Transform t)
    {
        Bounds b = FloorBounds(t);
        Vector3 mn = t.InverseTransformPoint(b.min), mx = t.InverseTransformPoint(b.max);
        return Rect.MinMaxRect(Mathf.Min(mn.x, mx.x), Mathf.Min(mn.z, mx.z), Mathf.Max(mn.x, mx.x), Mathf.Max(mn.z, mx.z));
    }

    /// <summary>(2026-11-03) LOS MUEBLES DEL PISO, que el pueblo rodea o sube (nunca se quitan): la caja en el mundo de cada pieza del
    /// piso gigante que no es suelo (losas bajas), del microcosmos al que pertenece <paramref name="anyInMicrocosm"/>. Para los solares
    /// (CivicBuilder.Lots) y el plano del pueblo (sus calles, ríos y colinas no pasan a través de un mueble).</summary>
    internal static List<Bounds> FloorFurniture(Transform anyInMicrocosm)
    {
        var l = new List<Bounds>();
        Transform root = anyInMicrocosm != null ? anyInMicrocosm.root : null;
        if (root == null) return l;
        foreach (Transform c in root)
        {
            if (!c.name.EndsWith("_gigante")) continue;
            foreach (Renderer r in c.GetComponentsInChildren<Renderer>())
            {
                Bounds b = r.bounds;
                if (IsFloor(b) || b.size.y < 0.1f) continue;
                l.Add(b);
            }
        }
        return l;
    }

    // ¿Algo del piso gigante cubre la cara de arriba <paramref name="top"/> (otra balda encima, la lámpara sobre la mesa)? Entonces no
    // cabe un edificio encima.
    static bool CoveredTop(Bounds top)
    {
        foreach (Bounds b in _blocked)
            if (b.min.y > top.max.y + 0.5f && b.min.y < top.max.y + 12f && b.max.x > top.min.x && b.min.x < top.max.x && b.max.z > top.min.z && b.min.z < top.max.z) return true;
        return false;
    }

    /// <summary>EL BARRIO DE LA HISTORIA (×100): un rectángulo del tamaño del piso real × <see cref="Legacy"/> (lo que medía el
    /// piso entero a ×10: el pueblo, la historia y sus plazas caben como antes), centrado en <paramref name="anchor"/> y metido
    /// dentro de los muros (menos la franja de <see cref="WallInset"/>). Sin plazas (sin área gigante), el piso entero.</summary>
    static Bounds District(Bounds rect, Vector3 anchor, PlatformSlots slots, bool town = false)
    {
        Bounds inner = Inner(rect, WallInset);
        // (2026-11-03) EN UN PUEBLO (era avanzada) el barrio es el piso real × TownScale (el 60 % del lado del piso ×100: 1,4–2,8
        // km), donde cabe el pueblo o la ciudad real de la historia a su tamaño de verdad (1 m = 1 m: los insectos-humanos miden
        // como una persona), con su río, su lago y sus colinas (CivicBuilder.TownPlans); alrededor, el santuario gigante.
        float scale = town ? TownScale : Legacy;
        Vector2 size = slots != null && slots.floorSize.x > 1f ? slots.floorSize * scale : new Vector2(rect.size.x, rect.size.z) * (scale / AreaScale);
        size = new Vector2(Mathf.Min(size.x, inner.size.x), Mathf.Min(size.y, inner.size.z));
        float cx = Mathf.Clamp(anchor.x, inner.min.x + size.x * 0.5f, inner.max.x - size.x * 0.5f);
        float cz = Mathf.Clamp(anchor.z, inner.min.z + size.y * 0.5f, inner.max.z - size.y * 0.5f);
        // Inner (FloorBounds) le quita la franja del muro: se le devuelve para que el barrio útil mida lo pedido.
        return new Bounds(new Vector3(cx, 0f, cz), new Vector3(size.x + 2f * WallInset, 0f, size.y + 2f * WallInset));
    }

    static Bounds Inner(Bounds rect, float inset)
    {
        float ix = Mathf.Min(inset, Mathf.Max(0f, rect.extents.x - 5f)), iz = Mathf.Min(inset, Mathf.Max(0f, rect.extents.z - 5f));
        return new Bounds(rect.center, new Vector3(rect.size.x - ix * 2f, rect.size.y, rect.size.z - iz * 2f));
    }

    static Vector3 ClampXZ(Vector3 p, Bounds r) => new Vector3(Mathf.Clamp(p.x, r.min.x, r.max.x), p.y, Mathf.Clamp(p.z, r.min.z, r.max.z));

    // Las cajas de los renderers (activos) de un subárbol. false si no tiene ninguno.
    static bool SubtreeBounds(Transform t, out Bounds b, bool onlyEnabled = true)
    {
        b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>(!onlyEnabled))
        {
            if (onlyEnabled && !r.enabled) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        return any;
    }

    // Cuánto hay que correr <paramref name="b"/> (en XZ) para que quede dentro de <paramref name="r"/> (si cabe).
    static Vector3 Overflow(Bounds b, Bounds r)
    {
        float dx = b.min.x < r.min.x ? r.min.x - b.min.x : b.max.x > r.max.x ? r.max.x - b.max.x : 0f;
        float dz = b.min.z < r.min.z ? r.min.z - b.min.z : b.max.z > r.max.z ? r.max.z - b.max.z : 0f;
        return new Vector3(dx, 0f, dz);
    }

    /// <summary>Quita del área gigante lo que queda ENTERO fuera del piso (voladizos, rótulos, piezas de fuera); lo que cruza su
    /// borde se mira por partes (sus hijos). Devuelve cuántas piezas quitó.</summary>
    static int Prune(Transform t, Bounds rect)
    {
        int n = 0;
        var kids = new List<Transform>(); foreach (Transform c in t) kids.Add(c);
        foreach (Transform c in kids)
        {
            if (c == null || !SubtreeBounds(c, out Bounds b, false)) continue;   // sin renderers (vacíos, puntos): se quedan
            const float tol = 0.5f;
            bool outside = b.max.x <= rect.min.x + tol || b.min.x >= rect.max.x - tol || b.max.z <= rect.min.z + tol || b.min.z >= rect.max.z - tol;
            if (outside) { Object.DestroyImmediate(c.gameObject); n++; continue; }
            bool crosses = b.min.x < rect.min.x - tol || b.max.x > rect.max.x + tol || b.min.z < rect.min.z - tol || b.max.z > rect.max.z + tol;
            if (crosses) n += Prune(c, rect);
        }
        return n;
    }

    /// <summary>Mete la historia en el piso: si se sale, la corre entera hacia dentro; si es más grande que el piso por algún
    /// lado, la centra en ese lado y recoloca pieza a pieza lo que siga fuera. Devuelve qué hizo («» si ya cabía).</summary>
    static string FitStory(Transform story, Transform cast, Bounds inner)
    {
        // Dos medidas: el CLARO de la historia (sin el pueblo) decide cuánto se corre; el CONJUNTO (con el pueblo) decide si hay
        // que recolocar pieza a pieza lo que se salga. (2026-11-01) Con el pueblo de la regla 5 en la medida del corrimiento, el
        // conjunto medía el piso entero y la historia de Bruce Lee se corría 4 m fuera de su plaza reservada (prueba 60).
        Bounds b = default, core = default; bool any = false, anyCore = false;
        foreach (Renderer r in story.GetComponentsInChildren<Renderer>(false))
        {
            if (!r.enabled || r.GetComponentInParent<MicroFood>() != null) continue;   // los restos se contienen uno a uno
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            if (InTown(r.transform, story)) continue;
            if (!anyCore) { core = r.bounds; anyCore = true; } else core.Encapsulate(r.bounds);
        }
        foreach (MobSpawnPoint sp in story.GetComponentsInChildren<MobSpawnPoint>(true))
        {
            if (!any) { b = new Bounds(sp.transform.position, Vector3.one); any = true; } else b.Encapsulate(sp.transform.position);
            if (!anyCore) { core = new Bounds(sp.transform.position, Vector3.one); anyCore = true; } else core.Encapsulate(sp.transform.position);
        }
        if (!any) return "";
        if (!anyCore) core = b;
        bool fitsX = b.size.x <= inner.size.x, fitsZ = b.size.z <= inner.size.z;
        bool coreX = core.size.x <= inner.size.x, coreZ = core.size.z <= inner.size.z;
        Vector3 d = Overflow(core, inner);
        if (!coreX) d.x = inner.center.x - core.center.x;
        if (!coreZ) d.z = inner.center.z - core.center.z;
        story.position += d;
        b.center += d;
        string what = d.sqrMagnitude > 0.01f ? $"se corre {d.magnitude:0} m hacia dentro del piso" : "";
        if (fitsX && fitsZ && Overflow(b, inner).sqrMagnitude < 0.01f) return what;
        int moved = Relayout(cast, inner);
        if (fitsX && fitsZ) return (what.Length > 0 ? what + "; " : "") + $"se recolocan dentro {moved} piezas del pueblo que se salían";
        return $"NO CABE entera ({b.size.x:0} × {b.size.z:0} m en un piso útil de {inner.size.x:0} × {inner.size.z:0} m): se centra el claro y se recolocan {moved} piezas dentro";
    }

    // Recoloca dentro de <paramref name="inner"/> cada pieza de la historia que se salga (y entra en las que no caben enteras).
    static int Relayout(Transform t, Bounds inner)
    {
        int n = 0;
        var kids = new List<Transform>(); foreach (Transform c in t) kids.Add(c);
        foreach (Transform c in kids)
        {
            if (!SubtreeBounds(c, out Bounds b)) { Vector3 p = ClampXZ(c.position, inner); if (p != c.position) { c.position = p; n++; } continue; }
            if (b.size.x <= inner.size.x && b.size.z <= inner.size.z) { Vector3 d = Overflow(b, inner); if (d.sqrMagnitude > 1e-4f) { c.position += d; n++; } }
            else n += Relayout(c, inner);
        }
        return n;
    }


    /// <summary>Las CARAS DE ARRIBA de los muebles gigantes (camas, encimeras, mesas, máquinas, estantes): cajas a nivel, de
    /// ≥ <paramref name="minArea"/> m² (ya ×10) y entre 1 y 60 m sobre el suelo, dentro de <paramref name="inside"/>; no los
    /// techos ni las losas grandes (más del 30 % del piso). Cada una, como una caja plana a la altura de su cara de arriba.</summary>
    static List<Bounds> FurnitureTops(GameObject giant, float groundY, Bounds inside, float minArea = 6f * K * K)
    {
        var l = new List<Bounds>();
        float big = inside.size.x * inside.size.z * 0.3f;
        foreach (Renderer r in giant.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled) continue;
            Bounds b = r.bounds;
            float h = b.max.y - groundY, a = b.size.x * b.size.z;
            if (IsFloor(b) || h < 1f * K || h > 60f * K || b.size.x < 2f * K || b.size.z < 2f * K || a < minArea || a > big) continue;
            if (Vector3.Dot(r.transform.up, Vector3.up) < 0.98f) continue;   // solo lo que está a nivel
            MeshFilter mf = r.GetComponent<MeshFilter>();
            bool box = r.GetComponent<BoxCollider>() != null || (mf != null && mf.sharedMesh != null && mf.sharedMesh.name.StartsWith("Cube"));
            if (!box || !InsideXZ(inside, b.center, 0f)) continue;   // una caja: su cara de arriba es plana y cubre su huella
            l.Add(new Bounds(new Vector3(b.center.x, b.max.y, b.center.z), new Vector3(b.size.x, 0f, b.size.z)));
        }
        return l;
    }
    /// <summary>Lo que vive en el microcosmos y no puede salir del piso: cada planta y cada bicho (sus nodos), y en la historia
    /// los restos de comida y los personajes (mirmidones, ánimas, agentes).</summary>
    static List<Transform> Population(Transform root, Transform story)
    {
        var l = new List<Transform>();
        foreach (string n in new[] { "Flora_a_escala", "Fauna_a_escala" }) { Transform g = root.Find(n); if (g != null) foreach (Transform c in g) l.Add(c); }
        foreach (Transform t in Characters(story)) l.Add(t);
        foreach (MicroFood f in story.GetComponentsInChildren<MicroFood>(true)) if (!l.Contains(f.transform)) l.Add(f.transform);
        return l;
    }

    // Los personajes de la historia: todo lo que anda (MicroForager, Anima, NavMeshAgent), sin repetir los anidados.
    static List<Transform> Characters(Transform story)
    {
        var l = new List<Transform>();
        foreach (Transform t in story.GetComponentsInChildren<Transform>(true))
            if (t.GetComponent<MicroForager>() != null || t.GetComponent<Anima>() != null || t.GetComponent<UnityEngine.AI.NavMeshAgent>() != null)
            {
                bool nested = false;
                foreach (Transform o in l) if (t.IsChildOf(o)) { nested = true; break; }
                if (!nested) l.Add(t);
            }
        return l;
    }

    // Corre <paramref name="t"/> hacia dentro si alguna de sus piezas asoma fuera de <paramref name="inner"/>. true si lo movió.
    static bool Contain(Transform t, Bounds inner)
    {
        if (t == null) return false;
        if (!SubtreeBounds(t, out Bounds b)) { Vector3 p = ClampXZ(t.position, inner); if (p == t.position) return false; t.position = p; return true; }
        if (b.size.x > inner.size.x || b.size.z > inner.size.z) return false;
        Vector3 d = Overflow(b, inner);
        if (d.sqrMagnitude < 1e-4f) return false;
        t.position += d;
        return true;
    }

    /// <summary>Cuenta en <paramref name="f"/> las plantas, bichos y personajes dentro del piso, y las piezas (renderers de
    /// flora, fauna e historia) que quedan fuera de los muros.</summary>
    static void Census(Transform root, Transform story, MicroFloor f)
    {
        Bounds rect = new Bounds(f.center, new Vector3(f.size.x, 0f, f.size.y));
        bool In(Bounds b) => b.min.x >= rect.min.x - 0.5f && b.max.x <= rect.max.x + 0.5f && b.min.z >= rect.min.z - 0.5f && b.max.z <= rect.max.z + 0.5f;
        int Count(Transform g) { int n = 0; if (g != null) foreach (Transform c in g) if (!SubtreeBounds(c, out Bounds b) ? In(new Bounds(c.position, Vector3.zero)) : In(b)) n++; return n; }
        Transform flora = root.Find("Flora_a_escala"), fauna = root.Find("Fauna_a_escala");
        f.floraInside = Count(flora); f.faunaInside = Count(fauna);
        f.charactersInside = 0;
        foreach (Transform t in Characters(story)) if (SubtreeBounds(t, out Bounds b) ? In(b) : In(new Bounds(t.position, Vector3.zero))) f.charactersInside++;
        f.outside = 0;
        foreach (Transform g in new[] { flora, fauna, story })
            if (g != null) foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) if (r.enabled && !In(r.bounds)) f.outside++;
    }

    /// <summary>Los MUROS en el borde del piso (por fuera de él: su cara de dentro es el borde), sin sombra (si no, al alba
    /// dejarían el piso a oscuras), con colliders y fuera del NavMesh, y una TAPA invisible arriba (nada vuela fuera). Son de
    /// la tierra de su era (adobe, barro, mantillo…) con un remate del color del área y contrafuertes por fuera. Altura: la del
    /// mueble más alto del área + 10 m, y al menos <see cref="WallMinHeight"/>. Devuelve la altura.</summary>
    static float Walls(Transform root, Bounds rect, float groundY, GameObject giant, Color soil, Color tint, string display)
    {
        float top = groundY + WallMinHeight;
        if (giant != null) foreach (Renderer r in giant.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y + 10f);
        { Transform hist = root.Find("Historia"); if (hist != null) foreach (Renderer r in hist.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y + 10f); }   // (los montes del plano del pueblo)
        float h = Mathf.Min(top - groundY, 400f), t = WallThick, hx = rect.extents.x, hz = rect.extents.z;
        Transform walls = new GameObject("Muros_del_piso").transform; walls.SetParent(root, false);
        walls.localPosition = new Vector3(rect.center.x, groundY, rect.center.z);
        Color body = Color.Lerp(soil, tint, 0.3f) * 0.85f; body.a = 1f;
        Color cap = Color.Lerp(tint, Color.white, 0.25f); cap.a = 1f;
        Color pier = body * 0.8f; pier.a = 1f;
        Material wm = Mat("Micro_Muro_" + display, body), cm = Mat("Micro_Muro_remate_" + display, cap), pm = Mat("Micro_Muro_contrafuerte_" + display, pier);
        void Piece(string name, Vector3 at, Vector3 size, Material m, bool solid)
        {
            Transform w = Box(walls, name, at, size, m, solid);
            w.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (solid) { var mod = w.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>(); mod.overrideArea = true; mod.area = 1; }   // no se anda por encima
        }
        // N (+z) y S (−z) de punta a punta (tapan las esquinas); E (+x) y O (−x) entre ellos.
        var sides = new (string name, Vector3 at, Vector3 size, Vector3 outward)[]
        {
            ("Muro_N", new Vector3(0f, h * 0.5f, hz + t * 0.5f), new Vector3(hx * 2f + t * 2f, h, t), Vector3.forward),
            ("Muro_S", new Vector3(0f, h * 0.5f, -hz - t * 0.5f), new Vector3(hx * 2f + t * 2f, h, t), Vector3.back),
            ("Muro_E", new Vector3(hx + t * 0.5f, h * 0.5f, 0f), new Vector3(t, h, hz * 2f), Vector3.right),
            ("Muro_O", new Vector3(-hx - t * 0.5f, h * 0.5f, 0f), new Vector3(t, h, hz * 2f), Vector3.left),
        };
        foreach (var (name, at, size, outward) in sides)
        {
            Piece(name, at, size, wm, true);
            Piece(name + "_remate", new Vector3(at.x, h + 1f, at.z) + outward * 0.5f, new Vector3(size.x + 1f, 2f, size.z + 1f), cm, false);
            // Contrafuertes cada ~40 m, por FUERA (no le quitan suelo al piso).
            bool alongX = size.x > size.z; float len = alongX ? size.x : size.z;
            int k = Mathf.Max(2, Mathf.RoundToInt(len / (40f * K)));
            for (int i = 0; i <= k; i++)
            {
                float s = -len * 0.5f + len * i / k;
                Vector3 p = at + outward * (t * 0.5f + 1.5f) + (alongX ? new Vector3(s, 0f, 0f) : new Vector3(0f, 0f, s));
                Piece(name + "_contrafuerte", new Vector3(p.x, h * 0.45f, p.z), new Vector3(alongX ? 3f : 3f, h * 0.9f, 3f), pm, false);
            }
        }
        // La TAPA: solo collider (no se ve, no da sombra): nada que vuele o salte sale por arriba.
        var lid = new GameObject("Tapa_invisible"); lid.transform.SetParent(walls, false); lid.transform.localPosition = new Vector3(0f, h + 1f, 0f);
        BoxCollider bc = lid.AddComponent<BoxCollider>(); bc.size = new Vector3(hx * 2f + t * 2f, 2f, hz * 2f + t * 2f);
        return h;
    }

    // ── Dónde cabe la historia dentro del área ───────────────────────────────────────────────────────────────────

    // Partes del área que el constructor NO cuelga de su GameObject (y que el área ×10 necesita para reconocerse). La
    // COCINA de verdad (puestos, neveras, barra de pase) vive en Restaurante_AUTO/Sala, no bajo Kitchen_Area: sin
    // copiarla, el microcosmos de la cocina era solo el comedor, con un hueco vacío en medio (el «hueco de 127 m»).
    static readonly Dictionary<string, string[]> Attached = new Dictionary<string, string[]>
    {
        { "Kitchen_Area", new[] { "Restaurante_AUTO/Sala" } },
    };

    /// <summary>Copia en <paramref name="copy"/> (el área recién clonada, aún a escala 1) las partes de <see cref="Attached"/>,
    /// con su misma pose relativa al área.</summary>
    static void CopyAttached(string areaGo, Transform src, Transform copy)
    {
        if (!Attached.TryGetValue(areaGo, out string[] paths)) return;
        foreach (string path in paths)
        {
            string[] parts = path.Split('/');
            GameObject top = GameObject.Find(parts[0]);
            Transform part = top == null ? null : parts.Length > 1 ? top.transform.Find(string.Join("/", parts, 1, parts.Length - 1)) : top.transform;
            if (part == null) { Debug.LogWarning($"[Microcosmos] {areaGo}: no encuentro {path}; el área ×10 irá sin esa parte."); continue; }
            Transform c = Object.Instantiate(part.gameObject).transform;
            c.name = part.name;
            c.SetParent(copy, false);
            c.localPosition = src.InverseTransformPoint(part.position);
            c.localRotation = Quaternion.Inverse(src.rotation) * part.rotation;
            c.localScale = part.lossyScale;
        }
    }

    // Bloques sólidos del área gigante (lo que no es suelo); restos de comida y briznas los evitan.
    static readonly List<Bounds> _blocked = new List<Bounds>();

    static bool IsFloor(Bounds b) => b.size.y < 1.5f * K && b.max.y < 3f * K;

    /// <summary>Separa el SUELO (planos bajos) de los MUEBLES (bloques) y devuelve la huella del área.</summary>
    static Bounds Classify(GameObject giant, List<Renderer> floors, List<Bounds> blocked)
    {
        Bounds foot = default; bool any = false;
        foreach (Renderer r in giant.GetComponentsInChildren<Renderer>())
        {
            Bounds b = r.bounds;
            if (!any) { foot = b; any = true; } else foot.Encapsulate(b);
            if (IsFloor(b)) { if (b.size.x > 4f * K && b.size.z > 4f * K) floors.Add(r); }   // suelo (no un tapete suelto)
            else blocked.Add(b);
        }
        return any ? foot : new Bounds(Vector3.zero, new Vector3(100f, 1f, 100f));
    }

    static float DistXZ(Bounds b, Vector3 p)
    {
        float dx = Mathf.Max(b.min.x - p.x, 0f, p.x - b.max.x), dz = Mathf.Max(b.min.z - p.z, 0f, p.z - b.max.z);
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    /// <summary>El punto del SUELO del área más alejado de cualquier mueble y del borde del suelo (rejilla de 6 m). Solo
    /// cuentan puntos que pisan un suelo del área: la huella incluye escaleras, rótulos y piezas que sobresalen, y sin
    /// esto el «hueco más libre» caía fuera de la plataforma (en la Cocina daba 127 m libres, lejos de los muebles).</summary>
    static Vector3 FreeSpot(Bounds foot, List<Renderer> floors, List<Bounds> blocked, out float free)
    {
        Vector3 best = new Vector3(foot.center.x, 0f, foot.center.z); free = 0f;
        float inset = Mathf.Min(10f * K, Mathf.Min(foot.extents.x, foot.extents.z) * 0.5f), step = 6f * K;
        for (float x = foot.min.x + inset; x <= foot.max.x - inset; x += step)
            for (float z = foot.min.z + inset; z <= foot.max.z - inset; z += step)
            {
                var p = new Vector3(x, 0f, z);
                float d = floors.Count == 0 ? float.MaxValue : EdgeDist(p, floors);
                if (d <= free) continue;   // fuera del suelo (0) o más cerca del borde que el mejor
                foreach (Bounds b in blocked) { d = Mathf.Min(d, DistXZ(b, p)); if (d <= free) break; }
                if (d > free) { free = d; best = p; }
            }
        return best;
    }

    /// <summary>Distancia de <paramref name="p"/> al borde del suelo más grande que lo cubre (0 si no pisa ninguno).</summary>
    static float EdgeDist(Vector3 p, List<Renderer> floors)
    {
        float best = 0f;
        foreach (Renderer r in floors)
        {
            Bounds b = r.bounds;
            if (p.x < b.min.x || p.x > b.max.x || p.z < b.min.z || p.z > b.max.z) continue;
            best = Mathf.Max(best, Mathf.Min(Mathf.Min(p.x - b.min.x, b.max.x - p.x), Mathf.Min(p.z - b.min.z, b.max.z - p.z)));
        }
        return best;
    }

    /// <summary>Altura del suelo del área en ese punto (la cara de arriba del plano más alto que lo cubre).</summary>
    // El suelo bajo p cuya cara de arriba queda más cerca de <paramref name="near"/> (no el más alto: hay plantas encima).
    static float FloorNear(Vector3 p, List<Renderer> floors, float near)
    {
        float y = near, best = float.MaxValue;
        foreach (Renderer r in floors)
        {
            if (r == null) continue;
            Bounds b = r.bounds;
            if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z && Mathf.Abs(b.max.y - near) < best) { best = Mathf.Abs(b.max.y - near); y = b.max.y; }
        }
        return y;
    }

    // Como FloorNear, pero sin ningún suelo bajo p devuelve <paramref name="none"/> (el del piso), no <paramref name="near"/>: una
    // plaza a 5 m (y × 10) sin mueble debajo no deja la planta flotando.
    static float FloorNearOr(Vector3 p, List<Renderer> floors, float near, float none)
    {
        foreach (Renderer r in floors)
        {
            if (r == null) continue;
            Bounds b = r.bounds;
            if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z) return FloorNear(p, floors, near);
        }
        return none;
    }

    // El suelo más BAJO bajo p (el del área; por encima hay anillos y entreplantas).
    static float FloorLowest(Vector3 p, List<Renderer> floors, float fallback)
    {
        float y = float.MaxValue;
        foreach (Renderer r in floors)
        {
            if (r == null) continue;
            Bounds b = r.bounds;
            if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z) y = Mathf.Min(y, b.max.y);
        }
        return y == float.MaxValue ? fallback : y;
    }

    static float FloorAt(Vector3 p, List<Renderer> floors, float fallback)
    {
        float y = float.MinValue;
        foreach (Renderer r in floors)
        {
            if (r == null) continue;
            Bounds b = r.bounds;
            if (p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z) y = Mathf.Max(y, b.max.y);
        }
        return y == float.MinValue ? fallback : y;
    }

    /// <summary>Aparta SOLO los muebles que pisa la historia (su guarida, su gente, el portal); el resto del área queda.</summary>
    static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    static int TouchingUnder(Transform story, Transform giant)
    {
        // Solo se aparta el mueble que CHOCA con una pieza sólida de la historia (no todo lo que cae dentro de la caja que la
        // envuelve): las historias grandes (el bosque y la cueva de Ambrosio) se MEZCLAN con el área, que sigue reconociéndose
        // (Berón, 2026-10-23: el microcosmos reutiliza el área y la puebla por encima). Los suelos de la historia no cuentan.
        var solids = new List<Bounds>();
        foreach (Renderer r in story.GetComponentsInChildren<Renderer>(true))
        {
            if (r.GetComponentInParent<MicroFood>() != null) continue;   // los restos se recolocan luego
            Bounds rb = r.bounds;
            if (rb.size.y < 0.5f && Mathf.Max(rb.size.x, rb.size.z) > 6f) continue;   // suelo / agua de la historia
            rb.Expand(new Vector3(1f, 0f, 1f)); solids.Add(rb);
        }
        foreach (MobSpawnPoint sp in story.GetComponentsInChildren<MobSpawnPoint>(true)) solids.Add(new Bounds(sp.transform.position, new Vector3(6f, 4f, 6f)));   // se llega a un sitio libre
        if (solids.Count == 0) return 0;
        // (2026-11-03) El piso NO se rompe: el mueble que toca la historia se queda (la historia se apoya en él o lo rodea); solo se cuenta.
        int touching = 0;
        foreach (Renderer r in giant.GetComponentsInChildren<Renderer>())
        {
            Bounds b = r.bounds;
            if (IsFloor(b)) continue;
            foreach (Bounds s in solids) if (b.max.x >= s.min.x && b.min.x <= s.max.x && b.max.z >= s.min.z && b.min.z <= s.max.z && b.min.y <= s.max.y) { touching++; break; }
        }
        _blocked.Clear();
        foreach (Renderer r in giant.GetComponentsInChildren<Renderer>()) if (!IsFloor(r.bounds)) _blocked.Add(r.bounds);
        // Restos de comida que hayan caído dentro de un mueble: al suelo libre más cercano de su zona.
        foreach (MicroFood f in story.GetComponentsInChildren<MicroFood>(true))
            for (int k = 0; k < 12 && Blocked(f.transform.position); k++)
            {
                Vector2 r = Random.insideUnitCircle * f.spread * 0.5f;
                f.transform.position = new Vector3(story.position.x + r.x, f.transform.position.y, story.position.z + r.y);
            }
        return touching;
    }

    // El MUEBLE entero al que pertenece un cubo (la mesa, no su pata): el hijo del área si es pequeño, o el de su grupo.
    static Transform Piece(Transform t, Transform giant, Bounds foot)
    {
        Transform top = t;
        while (top.parent != null && top.parent != giant) top = top.parent;
        if (top == t || Footprint(top) < 0.15f * foot.size.x * foot.size.z) return top;
        Transform piece = t;
        while (piece.parent != null && piece.parent != top) piece = piece.parent;
        return piece;
    }

    static float Footprint(Transform t)
    {
        Bounds b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return any ? b.size.x * b.size.z : 0f;
    }

    static bool Blocked(Vector3 p)
    {
        foreach (Bounds b in _blocked) if (p.x > b.min.x && p.x < b.max.x && p.z > b.min.z && p.z < b.max.z && p.y < b.max.y) return true;
        return false;
    }

    // ── HISTORIAS ─────────────────────────────────────────────────────────────────────────────────────────────────

    // ENFERMERÍA: la muerte de Ambrosio. Los builders del alba, a escala de hormiga, SIN los duplicados del mapa de
    // juego (otro Ambrosio, otra Kushal-hormiga: Kushal eres tú).
    static int InfirmaryStory(Transform root)
    {
        var story = new GameObject("Historia_Ambrosio").transform; story.SetParent(root, false);
        SampleSceneBuilder.BuildMicrocosmosSandbox(story);
        SampleSceneBuilder.BuildNivel1Sandbox(story);
        foreach (string dup in new[] { "Ambrosio_Aphid", "HoneydewDrop_Sample", "Kushal_Ant" })
            foreach (Transform t in story.GetComponentsInChildren<Transform>(true)) if (t != null && t.name == dup) { Object.DestroyImmediate(t.gameObject); break; }
        story.localScale = Vector3.one * StoryScale;
        // Los dos sandboxes se montan con su propio desplazamiento en el mundo (a cientos de metros): se traen junto al hueco de
        // la historia, a lo largo del pasillo central (+z, hacia el fondo): primero la cueva y luego el bosque, mezclados con
        // las camas y el equipo de los lados en vez de taparlo entero. La entrada y el portal quedan detrás (−z).
        float[] at = { 25f, 90f }; int k = 0;
        foreach (Transform top in story)
        {
            Renderer[] rs = top.GetComponentsInChildren<Renderer>(true); if (rs.Length == 0) continue;
            Bounds bb = rs[0].bounds; foreach (Renderer r in rs) bb.Encapsulate(r.bounds);
            Vector3 want = story.position + new Vector3(0f, 0f, at[Mathf.Min(k++, at.Length - 1)]);
            // Su suelo (el del bosque, el de la cueva) quedaba ~3 m BAJO el suelo del hospital: el elenco andaba enterrado. Se
            // sube a ras del suelo del hospital y ese suelo propio se oculta: se pisa el del hospital, con el bosque encima.
            float floorTop = float.MinValue; var ownFloors = new List<Renderer>();
            foreach (Renderer r in rs) { Bounds b = r.bounds; if (b.size.y < 0.5f && Mathf.Max(b.size.x, b.size.z) > 6f) { ownFloors.Add(r); floorTop = Mathf.Max(floorTop, b.max.y); } }
            float dy = floorTop > float.MinValue ? story.position.y - floorTop : 0f;
            top.position += new Vector3(want.x - bb.center.x, dy, want.z - bb.center.z);
            foreach (Renderer r in ownFloors) { r.enabled = false; Collider c = r.GetComponent<Collider>(); if (c != null) c.enabled = false; }
        }
        int n = 0;
        foreach (MeshFilter mfa in story.GetComponentsInChildren<MeshFilter>(true))   // todas las cápsulas: el elenco y los depredadores
        {
            MeshFilter mf = mfa; Component a = mfa;
            if (mf == null || mf.sharedMesh == null || !mf.sharedMesh.name.Contains("Capsule")) continue;
            Renderer r = a.GetComponent<Renderer>();
            Color c = r != null && r.sharedMaterial != null ? r.sharedMaterial.color : new Color(0.5f, 0.4f, 0.3f);
            bool aphid = a.name.Contains("Ambrosio");
            // Los depredadores siguen siendo INSECTOS (a escala de la hormiga); las hormigas, MIRMIDONES (hormigas humanas).
            if (a.name.Contains("Mariquita")) { CubeInsect.Dress(a.gameObject, "Ladybug", c, 2.4f); n++; continue; }
            if (a.name.Contains("Escarabajo")) { CubeInsect.Dress(a.gameObject, "DungBeetle", c, 5f); n++; continue; }
            if (a.name.Contains("Ara")) { CubeInsect.Dress(a.gameObject, "Spider", c, 4f); n++; continue; }
            CubeInsect.DressMyrmidon(a.gameObject, c, aphid ? AntHeight * 1.2f : AntHeight, aphid ? 1.6f : 1f, aphid);
            n++;
        }
        return n;
    }

    // COCINA (era del fuego): la tribu de las reencarnaciones recoge restos de comida y los lleva a la guarida.
    static int KitchenStory(Transform root)
    {
        Transform lair = Lair(root, Vector3.zero, "Guarida de la tribu");
        Food(root, false, 26, 42f);
        int n = 0;
        // Medea renacida: LA TIRANA DE LAS FEROMONAS, la líder (enorme y poderosa).
        GameObject leader = Ant(root, "La Tirana de las Feromonas", new Vector3(0f, 0f, 5f), new Color(0.45f, 0.12f, 0.12f), 2.3f, 1.25f); n++;
        // Ambrosio renacido: NASATYA, la «almohada gigante», mansa y fortísima, recostada en una postura rara.
        GameObject nasatya = Ant(root, "Nasatya", new Vector3(8f, 0f, 4f), new Color(0.75f, 0.68f, 0.55f), 2.4f, 2f); n++;
        nasatya.AddComponent<MicroLounger>();
        var ml = leader.AddComponent<MicroLeader>(); ml.lair = lair; ml.nasatya = nasatya.transform;
        // Héspero renacido: EL SEÑOR DEL FUEGO, junto a la brasa de la tribu; Sakshi: EL CHAMÁN; Momo: EL BUFÓN.
        Transform ember = Box(root, "Brasa", new Vector3(-6f, 0.4f, 3f), new Vector3(1.2f, 0.8f, 1.2f), Mat("Micro_Brasa", new Color(1f, 0.45f, 0.15f)), true);
        Ant(root, "El Señor del Fuego", new Vector3(-7.5f, 0f, 3f), new Color(0.3f, 0.2f, 0.18f), 2f, 1.1f); n++;
        Ant(root, "El Chamán", new Vector3(-5f, 0f, 5.5f), new Color(0.35f, 0.4f, 0.45f), 1.8f, 0.95f); n++;
        Ant(root, "El Bufón", new Vector3(4f, 0f, 8f), new Color(0.75f, 0.5f, 0.2f), 1.7f, 0.95f).AddComponent<MicroForager>(); n++;
        for (int i = 0; i < 12; i++)
        {
            float a = i * 30f * Mathf.Deg2Rad;
            Ant(root, $"Recolectora_{i + 1}", new Vector3(Mathf.Cos(a) * 12f, 0f, Mathf.Sin(a) * 12f), new Color(0.4f, 0.25f, 0.18f), 1.8f, 1f).AddComponent<MicroForager>();
            n++;
        }
        // CÓMO NACIÓ LA COCINA (docs/novela.md): la historia jugable. Sus piezas van en el claro de la historia (la plaza de
        // historia del piso, ×10; FitStory la mete entera dentro de los muros): el HOGAR (piedras alrededor de la brasa y la
        // piedra caliente donde se tuesta lo primero) y los RESTOS DEL HOGAR que Kushal y Nasatya llevan al fuego (no son
        // MicroFood: las recolectoras no se los llevan a la guarida).
        Material stoneMat = Mat("Micro_Piedra_hogar", new Color(0.5f, 0.48f, 0.45f));
        for (int k = 0; k < 8; k++)
        {
            float a = k * 45f * Mathf.Deg2Rad;
            Box(root, "Piedra_del_circulo", ember.localPosition + new Vector3(Mathf.Cos(a) * 1.5f, -0.2f, Mathf.Sin(a) * 1.5f), new Vector3(0.6f, 0.4f, 0.6f), stoneMat, false);
        }
        Box(root, "Piedra_del_hogar", ember.localPosition + new Vector3(1.8f, -0.2f, -1f), new Vector3(2f, 0.4f, 1.6f), stoneMat, false);
        Transform scraps = new GameObject("Restos_del_hogar").transform; scraps.SetParent(root, false);
        Material crumb = Mat("Micro_Miga", new Color(0.9f, 0.75f, 0.45f)), fruit = Mat("Micro_Fruta", new Color(0.85f, 0.3f, 0.25f));
        for (int i = 0; i < KitchenOriginStory.ScrapCount; i++)
        {
            float a = (i * 36f + 18f) * Mathf.Deg2Rad, r = 15f + (i % 3) * 3.5f;   // entre las recolectoras (12 m) y los restos de la tribu
            Box(scraps, $"Resto_del_hogar_{i + 1}", new Vector3(Mathf.Cos(a) * r, 0.4f, Mathf.Sin(a) * r), Vector3.one * 0.8f, Quaternion.Euler(0f, i * 23f, 0f), i % 3 == 0 ? fruit : crumb, false);
        }
        root.gameObject.AddComponent<KitchenOriginStory>();
        return n;
    }

    // HUERTO: La Sembradora (Ruth renacida) cuida el bancal de hongos y enseña a la tribu a cultivar.
    static int GardenStory(Transform root)
    {
        // ~9500 a. C.: ya hay ALDEA (Eras.HasStructures): la tribu lleva lo recogido al GRANERO REDONDO, no a una guarida, y vive en
        // casas redondas (CivicBuilder.FirstVillage, docs/microcosmos-civilizaciones.md §8).
        CivicBuilder.RoundGranary(root, new Vector3(-14f, 0f, 0f));
        Transform bed = new GameObject("Bancal_de_hongos").transform; bed.SetParent(root, false); bed.localPosition = new Vector3(10f, 0f, 2f);
        Box(bed, "Lecho", new Vector3(0f, 0.15f, 0f), new Vector3(10f, 0.3f, 7f), Mat("Micro_Lecho", new Color(0.35f, 0.28f, 0.2f)), true);
        Material st = Mat("Micro_Pie_Seta", new Color(0.9f, 0.86f, 0.75f)), cp = Mat("Micro_Hongo_cultivo", new Color(0.92f, 0.9f, 0.8f));
        for (int i = 0; i < 12; i++)
        {
            Transform m = new GameObject($"Hongo_{i + 1}").transform; m.SetParent(bed, false);
            m.localPosition = new Vector3(-4f + (i % 4) * 2.6f, 0.3f, -2.5f + (i / 4) * 2.5f);
            Box(m, "Pie", new Vector3(0f, 0.3f, 0f), new Vector3(0.2f, 0.6f, 0.2f), st, false);
            Box(m, "Sombrero", new Vector3(0f, 0.65f, 0f), new Vector3(0.8f, 0.2f, 0.8f), cp, false);
        }
        Food(root, true, 20, 40f);   // hojas para abonar el hongo
        Food(root, false, 12, 40f);  // y comida silvestre que aún se recoge
        int n = 0;
        var teacher = Ant(root, "La Sembradora", new Vector3(12f, 0f, -3f), new Color(0.35f, 0.45f, 0.25f), 1.8f, 1f).AddComponent<MicroTeacher>(); n++;
        teacher.bed = bed;
        for (int i = 0; i < 10; i++)
        {
            float a = i * 36f * Mathf.Deg2Rad;
            Ant(root, $"Mirmidón_{i + 1}", new Vector3(-14f + Mathf.Cos(a) * 8f, 0f, Mathf.Sin(a) * 8f), new Color(0.4f, 0.25f, 0.18f), 1.8f, 1f).AddComponent<MicroForager>();
            n++;
        }
        n += CivicBuilder.Civilize(root, "Huerto");   // las casas redondas de la primera aldea y sus familias
        return n;
    }

    // CUADRILLA DE RECOLECTORAS para los microcosmos de historia: su guarida y unas mirmidonas que recogen lo que da el entorno.
    internal static int ForagingBand(Transform root, Vector3 at, int count)
    {
        Lair(root, at, "Guarida de las recolectoras");
        // Restos junto a la guarida (como TribeStory): donde el área gigante tapa el suelo (el Hospital ×10 ocupa casi todo
        // y sus paredes cierran tres lados) la flora queda a >130 m y las recolectoras no tenían nada al alcance.
        Transform scraps = new GameObject("Restos_de_la_guarida").transform; scraps.SetParent(root, false); scraps.localPosition = at;
        Food(scraps, false, 12, 24f);   // en anillo de 14–24 m alrededor de la guarida
        for (int i = 0; i < count; i++)
        {
            float a = i * (360f / count) * Mathf.Deg2Rad;
            Ant(root, $"Recolectora_{i + 1}", at + new Vector3(Mathf.Cos(a) * 8f, 0f, Mathf.Sin(a) * 8f), new Color(0.4f, 0.25f, 0.18f), 1.8f, 1f).AddComponent<MicroForager>();
        }
        return count;
    }

    // RESTO DE ÁREAS: la tribu en su área gigante, con su guarida y sus recolectoras.
    static int TribeStory(Transform root, string display, int count)
    {
        Lair(root, Vector3.zero, $"Guarida ({display})");
        Food(root, false, 16, 40f);
        for (int i = 0; i < count; i++)
        {
            float a = i * (360f / count) * Mathf.Deg2Rad;
            Ant(root, $"Mirmidón_{i + 1}", new Vector3(Mathf.Cos(a) * 10f, 0f, Mathf.Sin(a) * 10f), new Color(0.4f, 0.25f, 0.18f), 1.8f, 1f).AddComponent<MicroForager>();
        }
        return count;
    }

    // ── Piezas ────────────────────────────────────────────────────────────────────────────────────────────────────

    // Una HORMIGA HUMANIZADA: bípeda, con cara; NavMeshAgent para rodear los muebles gigantes.
    internal static GameObject Ant(Transform root, string name, Vector3 pos, Color c, float height, float bulk)
    {
        var g = new GameObject(name);
        g.transform.SetParent(root, false); g.transform.localPosition = pos;
        g.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        var col = g.AddComponent<CapsuleCollider>(); col.height = height; col.radius = 0.35f * bulk; col.center = Vector3.up * height * 0.5f;
        var ag = g.AddComponent<UnityEngine.AI.NavMeshAgent>(); ag.radius = 0.4f * bulk; ag.height = height; ag.speed = 3f; ag.angularSpeed = 360f;
        CubeInsect.DressMyrmidon(g, c, height, bulk);
        WorldLabel wl = g.AddComponent<WorldLabel>(); wl.text = name; wl.height = height + 0.4f;
        return g;
    }

    // Un INSECTO-HUMANO de era civilizada (docs/microcosmos-civilizaciones.md §5–6): su insecto (InsectCast, o <paramref name="species"/>),
    // erguido a su estatura, con cara humana y pelo; todas las especies de tamaño parecido. Lleva su linaje (FolkLineage).
    internal static GameObject Folk(Transform root, string name, Vector3 pos, float height, float bulk = 1f, string species = null, Color? body = null, bool? female = null)
    {
        var g = new GameObject(name);
        g.transform.SetParent(root, false); g.transform.localPosition = pos;
        g.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        var col = g.AddComponent<CapsuleCollider>(); col.height = height; col.radius = 0.35f * bulk; col.center = Vector3.up * height * 0.5f;
        var ag = g.AddComponent<UnityEngine.AI.NavMeshAgent>(); ag.radius = 0.4f * bulk; ag.height = height; ag.speed = 3f; ag.angularSpeed = 360f;
        string sp = species ?? InsectCast.SpeciesFor(name);
        // El sexo: el pedido; si no, el del nombre (FemaleByName: Hannah, Teano, «Vecina»…); si el nombre no lo dice, al azar estable.
        bool isFemale = female ?? FemaleByName(name) ?? (Mathf.Abs(name.GetHashCode()) % 2) == 0;
        CubeInsect.DressFolk(g, sp, height, name, isFemale);
        if (body.HasValue) foreach (Renderer r in g.transform.Find("Insecto").GetComponentsInChildren<Renderer>(true)) if (r.sharedMaterial != null && r.sharedMaterial.color == InsectCast.ColorOf(sp)) r.sharedMaterial = KitchenProps.Mat(body.Value);
        Transform ins = g.transform.Find("Insecto");
        if (ins != null && bulk != 1f) ins.localScale = new Vector3(ins.localScale.x * bulk, ins.localScale.y, ins.localScale.z * Mathf.Sqrt(bulk));
        var lin = g.AddComponent<FolkLineage>(); lin.species = sp; lin.female = isFemale;
        PersonLife.Equip(g, isFemale);   // el MISMO ciclo de vida que un humano: necesidades, edad, pareja, hijos y vejez (docs/ciclo-de-vida.md §Humanos)
        WorldLabel wl = g.AddComponent<WorldLabel>(); wl.text = name; wl.height = height + 0.4f;
        return g;
    }

    // Nombres propios y palabras de oficio o parentesco que dicen el sexo (el resto, al azar estable por el hash del nombre).
    static readonly HashSet<string> FemaleNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "Hannah", "Teano", "Theano", "Marie", "Irène", "Irene", "Irena", "Florence", "Ruby", "Linda", "Katharine", "Mary", "Hipatia", "Hypatia", "Ada",
        "Rosalind", "Lise", "Emmy", "Sophie", "Jane", "Rachel", "Wangari", "Hildegard", "Medea", "Ruth", "Sakshi", "Lizmibet", "Andrea", "Kitty",
        "Elena", "Ana", "María", "Maria", "Magda", "Madame", "Lady", "Doña", "Mrs.", "Miss", "Sor", "Santa", "Reina", "Princesa", "Yasodharā", "Yasodhara", "Māyā", "Maya",
        "Madre", "Hermana", "Hija", "Abuela", "Esposa", "Vecina", "Cocinera", "Lavandera", "Alumna", "Camarera", "Enfermera", "Maestra", "Campesina", "Panadera",
        "Sacerdotisa", "Alfarera", "Discípula", "Tejedora", "Profesora", "Vendedora", "Cuidadora", "Sabia", "Monja", "Niña", "Muchacha", "Mujer",
    };
    static readonly HashSet<string> MaleNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "Isaac", "Albert", "Pierre", "Michele", "André", "Andre", "Bruce", "Yip", "Wong", "Taky", "James", "Ed", "Bob", "Dan", "Wilbur", "Orville", "Siddhārtha", "Siddhartha",
        "Patanjali", "Eratóstenes", "Pitágoras", "Ambrosio", "Héspero", "Atlas", "Momo", "Shanidar", "Kushal", "Oscar", "André", "Padre", "Hermano", "Hijo", "Abuelo",
        "Esposo", "Vecino", "Cocinero", "Alumno", "Camarero", "Maestro", "Campesino", "Panadero", "Sacerdote", "Alfarero", "Discípulo", "Yerno", "Sargento", "Cirujano",
        "Centinela", "Guardián", "Examinador", "Falsificador", "Bibliotecario", "Copista", "Mecánico", "Aprendiz", "Salvavidas", "Monje", "Niño", "Muchacho", "Hombre", "Rey", "Príncipe", "Fray", "Don", "Mr.", "Sir",
    };

    /// <summary>¿Es mujer <paramref name="name"/>? Por su nombre propio o su oficio («Hannah Ayscough», «Teano», «Vecina del
    /// tong lau», «Cocinero de Ruby Chow»); null si el nombre no lo dice. Manda la primera palabra que lo diga.</summary>
    internal static bool? FemaleByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        foreach (string w in name.Split(' ', '(', ')', ',', '«', '»', '-'))
        {
            if (w.Length == 0) continue;
            if (FemaleNames.Contains(w)) return true;
            if (MaleNames.Contains(w)) return false;
        }
        return null;
    }

    static Transform Lair(Transform root, Vector3 pos, string label)
    {
        Transform l = new GameObject("Guarida").transform; l.SetParent(root, false); l.localPosition = pos;
        Material earth = Mat("Micro_Guarida", new Color(0.4f, 0.3f, 0.2f)), dark = Mat("Micro_Boca_guarida", new Color(0.08f, 0.06f, 0.05f));
        for (int k = 0; k < 4; k++)
        {
            Transform m = Box(l, "Monticulo", new Vector3(0f, 0.6f + k * 1.1f, -3f), new Vector3(12f - k * 2.6f, 1.2f, 9f - k * 2f), earth, true);
            var mod = m.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>(); mod.overrideArea = true; mod.area = 1;   // no se anda por encima: su cima era una isla del NavMesh
        }
        Box(l, "Entrada", new Vector3(0f, 1.2f, 1.55f), new Vector3(2.4f, 2.4f, 0.1f), dark, false);
        l.gameObject.AddComponent<MicroLair>().doorOffset = new Vector3(0f, 0f, 3.2f);   // delante de la boca: el centro queda dentro del montículo
        WorldLabel wl = l.gameObject.AddComponent<WorldLabel>(); wl.text = label; wl.height = 6f;
        return l;
    }

    // Restos de comida (o hojas): migas, granos y trozos de fruta de ~1 m, a escala de hormiga.
    static void Food(Transform root, bool leaves, int n, float spread)
    {
        Material crumb = Mat("Micro_Miga", new Color(0.9f, 0.75f, 0.45f)), fruit = Mat("Micro_Fruta", new Color(0.85f, 0.3f, 0.25f)), leaf = Mat("Micro_Hoja", new Color(0.35f, 0.6f, 0.25f));
        var rnd = new System.Random(n * 31 + (leaves ? 7 : 0));
        for (int i = 0; i < n; i++)
        {
            float a = (float)rnd.NextDouble() * Mathf.PI * 2f, r = 14f + (float)rnd.NextDouble() * (spread - 14f);
            Vector3 p = new Vector3(Mathf.Cos(a) * r, 0.4f, Mathf.Sin(a) * r);
            Transform f = Box(root, leaves ? "Hoja" : "Resto_comida", p, leaves ? new Vector3(1.6f, 0.08f, 1f) : Vector3.one * (0.6f + (float)rnd.NextDouble() * 0.5f),
                              Quaternion.Euler(0f, (float)rnd.NextDouble() * 180f, 0f), leaves ? leaf : (i % 3 == 0 ? fruit : crumb), false);
            var mf = f.gameObject.AddComponent<MicroFood>(); mf.leaf = leaves; mf.spread = spread * 0.8f;
        }
    }

    // FLORA y FAUNA a escala de hormiga por toda la superficie (docs/natural-interactions.md). Nada es solo decorado:
    // cada planta, piedra o mancha de arena lleva el Affordance de lo que los insectos hacen con ella de verdad.
    //   · Brizna (pasto): hierba que comen saltamontes y grillos y que cortan las termitas; savia para el pulgón; posadero
    //     para asolearse (saltamontes) o tender la tela (araña); y espiga de SEMILLAS que cosechan los mirmidones.
    //   · Helecho: savia, hojas que la tribu lleva al jardín de hongos, posadero de la mantis y refugio bajo sus frondas.
    //   · Hongo: comida de las termitas cultivadoras y refugio bajo el sombrero.
    //   · Flor: polen/néctar (mariquita) y el sitio donde la mantis acecha a los que vienen a la flor.
    //   · Musgo y piedra: refugio húmedo (grillo en su madriguera, mariquitas agregadas).
    //   · Arena fina: donde la hormiga león cava su embudo. · Boñiga: lo que rueda el pelotero.
    /// <param name="rect">El PISO ×10 (el interior de los muros): todo crece y nace DENTRO (regla 11); fuera, nada.</param>
    /// <param name="floraSlots">Las plazas de flora del área (PlatformSlots, ya ×10, en el espacio de la raíz): una planta en cada
    /// una. <paramref name="floraUnderStory"/> recibe las que tapa la historia. Entre ellas, la rejilla densa del suelo libre.</param>
    /// <param name="faunaSlots">Las plazas de fauna del área (ídem): los bichos van primero a ellas y luego a suelo libre.</param>
    static int Nature(Transform root, Bounds rect, Bounds district, float groundY, int seed, Bounds story, List<Renderer> floors, MicroBiome.Biome biome = null,
                      List<Vector3> floraSlots = null, List<Vector3> faunaSlots = null, List<int> floraUnderStory = null, List<Bounds> tops = null,
                      bool town = false, string display = null, Vector3 storyOrigin = default)
    {
        var rnd = new System.Random(seed);
        // (2026-11-04) EL MAPA A MANO de la historia (MicroStoryMaps; regla M9): si está hecho, su flora y su fauna van a su sitio (y caen
        // desde el aire: MicroGround.Drop); la siembra al azar de abajo es solo el RESPALDO de las historias sin mapa, y se dice.
        MicroStoryMaps.Map map = display != null ? MicroStoryMaps.For(display) : null;
        bool hand = map != null && map.status == MicroStoryMaps.Status.Hecho;
        _handPlaced.Clear();
        if (display != null) Debug.Log(hand ? $"[Mapa] {display}: mapa a mano ({map.PlantCount} plantas, {map.AnimalCount} bichos, {map.floor.Length} órdenes sobre el piso; {map.place})." : $"[Mapa] {display}: SIN mapa a mano ({(map != null ? "esbozo: " + string.Join("; ", map.todo) : "ni esbozo")}): siembra al azar como respaldo.");
        float biomeWeight = 0f; if (biome != null) foreach (MicroBiome.Plant pl in biome.plants) biomeWeight += pl.weight;
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        Vector3 center = new Vector3(rect.center.x, groundY, rect.center.z);
        Bounds inner = Inner(rect, WallInset);
        var flora = new GameObject("Flora_a_escala").transform; flora.SetParent(root, false); flora.localPosition = center;
        // EL BARRIO DE LA HISTORIA (×100) se puebla denso (como antes todo el piso); el resto del piso gigante, con la rejilla
        // ×K: la misma flora, sin multiplicar por cien el número de plantas (ni de objetos de la escena).
        Bounds dense = Inner(district, WallInset); dense.Expand(new Vector3(40f, 0f, 40f));
        // (2026-11-03) En un pueblo de barrio grande (×60), la rejilla fina solo donde están sus edificios (más 40 m): el resto del
        // barrio (huertas, prados, el río, las colinas) con la rejilla gruesa. Si no, 1–8 millones de celdas de 3 m.
        if (town && _hasCore) { dense = _townCore; dense.Expand(new Vector3(80f, 0f, 80f)); }
        { Vector3 mn = Vector3.Max(dense.min, inner.min), mx = Vector3.Min(dense.max, inner.max); dense.SetMinMax(new Vector3(mn.x, 0f, mn.z), new Vector3(Mathf.Max(mn.x, mx.x), 0f, Mathf.Max(mn.z, mx.z))); }
        IEnumerable<(Bounds box, float step)> Grids(float c) { yield return (dense, c); if (K > 1f) yield return (inner, c * K); }
        Material grass = Mat("Micro_Hierba", new Color(0.62f, 0.68f, 0.3f)), grassDry = Mat("Micro_Hierba_seca", new Color(0.78f, 0.72f, 0.42f));
        Material fern = Mat("Micro_Helecho", new Color(0.25f, 0.5f, 0.25f)), fernStem = Mat("Micro_Helecho_tallo", new Color(0.3f, 0.4f, 0.2f));
        Material stem = Mat("Micro_Pie_Seta", new Color(0.9f, 0.86f, 0.75f)), cap = Mat("Micro_Sombrero", new Color(0.7f, 0.35f, 0.2f));
        Material moss = Mat("Micro_Musgo", new Color(0.3f, 0.45f, 0.2f)), mossTip = Mat("Micro_Musgo_punta", new Color(0.45f, 0.6f, 0.25f));
        Material rock = Mat("Micro_Grano", new Color(0.55f, 0.52f, 0.48f)), seedMat = Mat("Micro_Espiga", new Color(0.82f, 0.7f, 0.42f));
        Material petal = Mat("Micro_Petalo", new Color(0.97f, 0.95f, 0.9f)), petal2 = Mat("Micro_Petalo_lila", new Color(0.8f, 0.6f, 0.9f)), pollen = Mat("Micro_Polen", new Color(0.98f, 0.8f, 0.2f));
        Material sand = Mat("Micro_Arena", new Color(0.82f, 0.72f, 0.5f)), dung = Mat("Micro_Estiercol", new Color(0.35f, 0.25f, 0.15f));

        // Lo que se alza SOBRE el suelo del piso (muebles, paredes, máquinas): no los techos ni lo que cuelga arriba, que tapaban
        // todo el piso y no dejaban crecer nada.
        var standing = new List<Bounds>();
        foreach (Bounds b in _blocked) if (b.min.y < groundY + 5f * K && b.max.y > groundY + 0.05f) standing.Add(b);
        const float storyClear = 20f;
        // SUELO LIBRE en w: dentro de los muros (con margen), fuera del claro de la historia, a ≥ 3 m de todo mueble y sobre el
        // suelo del área a su altura (no en una entreplanta). fy: la altura de ese suelo.
        bool FreeFloor(Vector3 w, out float fy)
        {
            fy = groundY;
            if (!InsideXZ(inner, w, 0f)) return false;
            if (new Vector2(w.x - story.center.x, w.z - story.center.z).magnitude < storyClear || InsideXZ(story, w, 4f)) return false;   // el claro de la historia
            foreach (Bounds b in standing) if (w.x > b.min.x - 3f && w.x < b.max.x + 3f && w.z > b.min.z - 3f && w.z < b.max.z + 3f) return false;
            fy = floors.Count > 0 ? FloorNear(w, floors, groundY) : groundY;
            return Mathf.Abs(fy - groundY) <= 1.5f;
        }

        var sandPits = new List<Vector3>(); var dungs = new List<Vector3>();
        int placed0 = 0;

        // ── MODO PUEBLO (eras avanzadas: el pueblo humano de los insectos-humanos, regla 5) ─────────────────────────────────
        // A ESCALA DE PUEBLO (1 cm real = 1 cm: un tilo de 15 m, un seto de 0,8 m, un césped de 5 cm), ESCASA (una de cada ~3
        // celdas de 3 m) y FUERA de las calles y los solares: el tronco o la mata a ≥ 1 m de la huella de cada edificio (que ya
        // lleva 1 m de margen: 2 m de paso, MicroBuildRules.Passage) y del borde de cada calle; las copas bajas, igual; entre
        // plantas, sus radios + 0,8 m. Junto al pueblo (a < 15 m de una huella) crece lo PLANTADO (Biome.town: árboles de calle,
        // setos, céspedes, parterres; ni piedras ni arena: el adoquín es la calle); en las afueras, lo del lugar (Biome.plants),
        // donde el lugar lo tenía. Antes la flora de hormiga (×25: flores de 37 m, matas de 40 m) tapaba el pueblo entero.
        var townPts = new Dictionary<(int, int), List<(Vector3 p, float r)>>();   // (2026-11-03) por celdas de 12 m: con el barrio ×60 eran miles de plantas a comparar una a una
        (int, int) Cell(Vector3 w) => (Mathf.FloorToInt(w.x / 12f), Mathf.FloorToInt(w.z / 12f));
        bool Crowded(Vector3 w, float r)
        {
            var (cx, cz) = Cell(w);
            for (int i = cx - 1; i <= cx + 1; i++) for (int k = cz - 1; k <= cz + 1; k++)
                if (townPts.TryGetValue((i, k), out var l)) foreach (var q in l) if (new Vector2(q.p.x - w.x, q.p.z - w.z).magnitude < q.r + r + 0.8f) return true;
            return false;
        }
        void AddPt(Vector3 w, float r) { var c = Cell(w); if (!townPts.TryGetValue(c, out var l)) townPts[c] = l = new List<(Vector3, float)>(); l.Add((w, r)); }
        const float TownCell = 3f, TownKeep = 0.38f, NearTownDist = 15f;
        MicroBiome.Plant[] fallbackTown = {
            new MicroBiome.Plant(MicroBiome.Kind.Grass, 10, "Césped", 6, new Color(0.4f, 0.6f, 0.28f), new Color(0.45f, 0.6f, 0.3f)),
            new MicroBiome.Plant(MicroBiome.Kind.Shrub, 4, "Árbol de sombra", 1000, new Color(0.25f, 0.45f, 0.22f), new Color(0.3f, 0.45f, 0.25f)),
            new MicroBiome.Plant(MicroBiome.Kind.Shrub, 4, "Seto", 90, new Color(0.22f, 0.4f, 0.2f), new Color(0.25f, 0.42f, 0.22f)) };
        int townTrees = 0, townSmall = 0;
        bool NearTown(Vector3 w, float d)
        {
            foreach (Bounds b in _townFeet) if (GapXZ(b, w) < d) return true;
            return false;
        }
        bool FreeTown(Vector3 w, float r, out float fy)
        {
            fy = groundY;
            if (!InsideXZ(inner, w, -r)) return false;
            if (new Vector2(w.x - story.center.x, w.z - story.center.z).magnitude < storyClear + r || InsideXZ(story, w, 4f + r)) return false;
            foreach (Bounds b in standing) if (GapXZ(b, w) < r + 1f) return false;
            fy = floors.Count > 0 ? FloorNearOr(w, floors, groundY, groundY) : groundY;
            return Mathf.Abs(fy - groundY) <= 1.5f;
        }
        bool TownSow(Vector3 p, bool always)
        {
            Vector3 w = center + p;
            if (!always && R(0f, 1f) > TownKeep) return false;   // (antes de mirar el pueblo: es lo barato)
            bool near = NearTown(w, NearTownDist);
            MicroBiome.Plant[] pool = biome == null ? fallbackTown : near && biome.town != null ? biome.town : biome.plants;
            float sum = 0f; foreach (MicroBiome.Plant q in pool) sum += q.weight;
            float pick = R(0f, sum); MicroBiome.Plant pl = pool[pool.Length - 1];
            foreach (MicroBiome.Plant q in pool) { pick -= q.weight; if (pick <= 0f) { pl = q; break; } }
            bool loose = pl.kind == MicroBiome.Kind.Stone || pl.kind == MicroBiome.Kind.Sand || pl.kind == MicroBiome.Kind.Dung;
            if (near && loose) return false;   // en el pueblo, el adoquín y el ladrillo son la calle: no hay piedras sueltas
            float h = Mathf.Max(0.03f, pl.cm / 100f) * R(0.85f, 1.15f);
            TownForm form = FormOf(pl, h);
            TownRadii(form, h, out float blockR, out float spaceR);
            if (!FreeTown(w, blockR, out float fy)) return false;
            if (Crowded(w, spaceR)) return false;
            AddPt(w, spaceR);
            TownPlant(pl, new Vector3(p.x, fy - center.y, p.z), h, form, spaceR);
            if (form == TownForm.Tree || form == TownForm.Palm) townTrees++; else townSmall++;
            return true;
        }

        // Una planta del PUEBLO: su forma a escala humana (árbol con tronco y copa por encima de las cabezas, palmera, mata o seto,
        // macolla, césped, parterre, musgo o hiedra a ras de suelo, seta, piedra…), con su nombre y sus usos (Affordance).
        void TownPlant(MicroBiome.Plant pl, Vector3 p, float h, TownForm form, float r)
        {
            Material a = Mat("Bio_" + pl.name + "_1", pl.main), b = Mat("Bio_" + pl.name + "_2", pl.accent);
            Material bark = Mat("Bio_corteza", new Color(0.36f, 0.28f, 0.21f));
            Transform t = Plant(pl.name, p);
            switch (form)
            {
                case TownForm.Tree:
                {
                    float tw = Mathf.Clamp(h * 0.035f, 0.2f, 0.6f), ch = h * 0.55f;
                    Box(t, "Tronco", Vector3.up * (h - ch * 0.5f) * 0.5f, new Vector3(tw, h - ch * 0.5f, tw), Quaternion.identity, bark, true);
                    Box(t, "Copa", Vector3.up * (h - ch * 0.5f), new Vector3(r * 2f, ch, r * 2f), Quaternion.identity, a, false);
                    Box(t, "Copa", Vector3.up * (h - ch * 0.4f), new Vector3(r * 1.5f, ch * 0.85f, r * 1.5f), Quaternion.Euler(0f, 45f, 0f), a, false);
                    for (int k = 0; k < 6; k++) Box(t, "Flor_o_fruto", new Vector3(R(-0.8f, 0.8f) * r, h - ch * R(0.1f, 0.8f), R(-0.8f, 0.8f) * r), Vector3.one * Mathf.Clamp(h * 0.03f, 0.15f, 0.4f), Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Leaf, 3, 3f, 0.002f);
                    Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                }
                case TownForm.Palm:
                    Box(t, "Estipe", Vector3.up * h * 0.47f, new Vector3(0.4f, h * 0.94f, 0.4f), Quaternion.identity, bark, true);
                    for (int k = 0; k < 8; k++) Box(t, "Hoja", Vector3.up * h + Quaternion.Euler(0f, k * 45f, 0f) * new Vector3(0f, -0.4f, r * 0.5f), new Vector3(0.6f, 0.08f, r), Quaternion.Euler(25f, k * 45f, 0f), a, false);
                    for (int k = 0; k < 3; k++) Box(t, "Racimo", Vector3.up * (h - 0.8f) + Quaternion.Euler(0f, k * 120f, 0f) * new Vector3(0f, 0f, 0.4f), Vector3.one * 0.35f, Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Leaf, 3, 3f, 0.002f);
                    Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
                    break;
                case TownForm.Bush:   // mata, seto o rosal: un cojín de hojas con sus flores o bayas
                {
                    bool hedge = pl.name.StartsWith("Seto");
                    Vector3 size = hedge ? new Vector3(r * 2f * 1.8f, h, r * 2f * 0.6f) : new Vector3(r * 2f, h, r * 2f);
                    Box(t, hedge ? "Seto" : "Mata", Vector3.up * h * 0.5f, size, Quaternion.identity, a, h > 0.5f);
                    for (int k = 0; k < 5; k++) Box(t, "Flor_o_baya", new Vector3(R(-0.4f, 0.4f) * size.x, h * R(0.55f, 1f), R(-0.4f, 0.4f) * size.z), Vector3.one * Mathf.Clamp(h * 0.12f, 0.06f, 0.2f), Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Leaf, 3, 3f, 0.002f);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    if (pl.kind == MicroBiome.Kind.Flower) Affordance.Put(t.gameObject, AffordanceKind.Flower, 2, 1f, 0.004f);
                    break;
                }
                case TownForm.Clump:   // gramínea alta o helecho bajo: unas hojas en macolla
                    for (int k = 0; k < 5; k++) Box(t, "Hoja", new Vector3(0f, h * 0.5f, 0f) + Quaternion.Euler(0f, k * 72f, 0f) * new Vector3(0f, 0f, 0.08f), new Vector3(0.05f, h, 0.18f), Quaternion.Euler(R(-15f, 15f), k * 72f, 0f), a, false);
                    Box(t, "Espiga", Vector3.up * h, new Vector3(0.06f, Mathf.Max(0.05f, h * 0.15f), 0.12f), Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Grass, 2, 1f, 0.004f);
                    Affordance.Put(t.gameObject, AffordanceKind.PlantSap, 3, 1f, 0.01f);
                    break;
                case TownForm.Lawn:
                    Box(t, "Cesped", Vector3.up * Mathf.Max(0.02f, h * 0.5f) * 0.5f, new Vector3(r * 2f, Mathf.Max(0.02f, h * 0.5f), r * 2f * R(0.7f, 1f)), Quaternion.identity, a, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Grass, 2, 1f, 0.004f);
                    break;
                case TownForm.Bed:   // parterre o jardinera: tierra con sus flores
                    Box(t, "Jardinera", Vector3.up * 0.12f, new Vector3(1.4f, 0.24f, 0.9f), Quaternion.identity, Mat("Bio_jardinera", new Color(0.45f, 0.33f, 0.24f)), false);
                    Box(t, "Hojas", Vector3.up * (0.24f + h * 0.4f), new Vector3(1.2f, h * 0.8f, 0.7f), Quaternion.identity, a, false);
                    for (int k = 0; k < 7; k++) Box(t, "Flor", new Vector3(R(-0.55f, 0.55f), 0.24f + h * 0.85f, R(-0.3f, 0.3f)), Vector3.one * 0.09f, Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Flower, 2, 1f, 0.004f);
                    break;
                case TownForm.Patch:   // musgo, hiedra o liquen a ras de suelo
                    Box(t, "Manto", Vector3.up * 0.025f, new Vector3(r * 2f, 0.05f, r * 2f * 0.8f), Quaternion.identity, a, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                case TownForm.Mushroom:
                    Box(t, "Pie", Vector3.up * h * 0.5f, new Vector3(0.04f, h, 0.04f), Quaternion.identity, a, false);
                    Box(t, "Sombrero", Vector3.up * h, new Vector3(h * 0.8f, h * 0.2f, h * 0.8f), Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Fungus, 3, 2f, 0.003f);
                    break;
                case TownForm.Rock:
                    Box(t, "Roca", Vector3.up * r * 0.4f, new Vector3(r * 1.6f, r * 0.8f, r * 1.4f), Quaternion.identity, a, true);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                case TownForm.Sand:
                    Box(t, "Arena", Vector3.up * 0.015f, new Vector3(r * 2f, 0.03f, r * 2f), Quaternion.identity, a, false);
                    Affordance.Put(t.gameObject, AffordanceKind.SandPit, 1); sandPits.Add(p);
                    break;
                case TownForm.Dung:
                    Box(t, "Bulto", Vector3.up * 0.08f, new Vector3(0.4f, 0.16f, 0.35f), Quaternion.identity, a, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Dung, 2, 3f, 0.001f); dungs.Add(p);
                    break;
                case TownForm.Frost:
                    Box(t, "Escarcha", Vector3.up * 0.01f, new Vector3(r * 2f, 0.02f, r * 2f), Quaternion.identity, a, false);
                    break;
            }
        }


        // La flora DEL ÁREA, en sus POSICIONES RESERVADAS (PlatformSlots.flora, ya ×10): una planta en cada una, a ras del suelo
        // del área (el más cercano a la altura de la plaza), menos las que tapa la historia (su claro).
        int onSlots = 0;
        var slotPts = new List<Vector3>();
        if (floraSlots != null && !hand)
            for (int i = 0; i < floraSlots.Count; i++)
            {
                Vector3 w = floraSlots[i];
                if (new Vector2(w.x - story.center.x, w.z - story.center.z).magnitude < storyClear || InsideXZ(story, w, 4f) || !InsideXZ(rect, w, -1f)) { floraUnderStory?.Add(i); continue; }
                // En el pueblo, la plaza que cae en una calle o un solar es del pueblo (como la que tapa la historia).
                if (town && !FreeTown(w, 0.6f, out _)) { floraUnderStory?.Add(i); continue; }
                // A ras del suelo bajo la plaza; si no hay suelo del área debajo (el pueblo quitó los muebles gigantes), el del piso.
                // (2026-11-01) Antes quedaba a la altura de la plaza (y × 10 = 5 m): las piedras «Adoquín de arenisca» flotaban a
                // 3,5–5 m sobre las calles de Berna (los cubos beis de Einstein_Berna_calle.png) y la hierba de Bruce Lee, a 5 m.
                float fy = FloorNearOr(w, floors, w.y, groundY);
                slotPts.Add(w);
                if (Sow(new Vector3(w.x - center.x, fy - center.y, w.z - center.z), true)) { placed0++; onSlots++; }
            }

        // EL PUEBLO: una rejilla fina (3 m) por todo el suelo libre, escasa (TownSow decide si toca planta y cuál).
        //     (×100) Fina en el BARRIO de la historia; por el resto del piso gigante, cada TownCell × K.
        //     (2026-11-03) Si el pueblo pasa de 500 m de lado (barrio ×60), la rejilla fina se abre en proporción: ~28 000 celdas como mucho.
        float fine = TownCell * Mathf.Max(1f, Mathf.Sqrt(dense.size.x * dense.size.z) / 500f);
        if (town && !hand)
            foreach (var (box, step) in Grids(fine))
                for (float x = box.min.x + step * 0.5f; x <= box.max.x; x += step)
                    for (float z = box.min.z + step * 0.5f; z <= box.max.z; z += step)
                    {
                        if (step > fine && InsideXZ(dense, new Vector3(x, 0f, z), 0f)) continue;
                        if (TownSow(new Vector3(x + R(-1f, 1f) - center.x, 0f, z + R(-1f, 1f) - center.z), false)) placed0++;
                    }

        // Y ENTRE LAS PLAZAS, DENSO: a ×10 los 2 m de paso del piso son 20 m, así que cada hueco libre se rellena con una rejilla
        // de 6 m con jitter (una planta por celda, sin amontonarse), por todo el suelo libre dentro de los muros.
        //     (×100) La rejilla de 6 m, en el BARRIO de la historia; por el resto del piso gigante (kilómetros), cada 6 m × K: el
        //     santuario gigante también tiene su musgo y sus briznas, sin multiplicar por cien las plantas.
        const float cell = 6f;
        if (!town && !hand)
        foreach (var (box, step) in Grids(cell))
        for (float x = box.min.x + step * 0.5f; x <= box.max.x; x += step)
            for (float z = box.min.z + step * 0.5f; z <= box.max.z; z += step)
            {
                if (step > cell && InsideXZ(dense, new Vector3(x, 0f, z), 0f)) continue;
                Vector3 w = new Vector3(x + R(-step * 0.4f, step * 0.4f), groundY, z + R(-step * 0.4f, step * 0.4f));
                if (!FreeFloor(w, out float fy)) continue;
                bool nearSlot = false;
                foreach (Vector3 s in slotPts) if (new Vector2(w.x - s.x, w.z - s.z).sqrMagnitude < 9f) { nearSlot = true; break; }
                if (nearSlot) continue;
                if (Sow(new Vector3(w.x - center.x, fy - center.y, w.z - center.z), false)) placed0++;
            }

        // Y ENCIMA DE LOS MUEBLES GIGANTES (Berón 2026-10-29): a ×10 la cama, la encimera, la mesa o la máquina son mesetas de
        // decenas de metros; su cara de arriba (FurnitureTops) también se puebla, con la misma rejilla, donde nada la tapa.
        var topPts = new List<Vector3>();
        int onTops = 0;
        if (tops != null)
            foreach (Bounds top in tops)
            {
                int here = 0;
                float ex = Mathf.Max(0f, top.extents.x - 1.5f), ez = Mathf.Max(0f, top.extents.z - 1.5f);
                float tc = InsideXZ(dense, top.center, 0f) ? cell : cell * K;   // (×100: denso solo en el barrio)
                for (float x = top.center.x - ex; x <= top.center.x + ex + 0.01f; x += tc)
                    for (float z = top.center.z - ez; z <= top.center.z + ez + 0.01f; z += tc)
                    {
                        Vector3 w = new Vector3(x, top.max.y, z);
                        if (!InsideXZ(inner, w, 0f) || InsideXZ(story, w, 2f) || Covered(w)) continue;
                        topPts.Add(w);
                        if (town ? TownTopSow(w - center) : Sow(w - center, here == 0)) { placed0++; onTops++; here++; }
                    }
            }
        _floraOnTops = onTops;
        // (2026-11-04) LA FLORA DEL MAPA A MANO: cada planta o rodal en su sitio (metros desde la «Historia»), con la planta de su bioma;
        // luego cae desde el aire sobre su superficie (MicroGround.Drop, en AreaMicrocosmos.Build).
        if (hand)
        {
            int handFlora = 0, missing = 0, handNoRoom = 0;
            const float TownHandMaxH = 20f;   // (la prueba: ≤ 25 m; el moriche, 20 m, es de los que más)
            foreach (MicroStoryMaps.Plant hp in map.flora)
            {
                MicroBiome.Plant? found = FindPlant(biome, hp.plant);
                if (found == null) { missing++; Debug.LogWarning($"[Mapa] {display}: la planta «{hp.plant}» no está en su bioma: se salta."); continue; }
                MicroBiome.Plant pl = found.Value;
                foreach (Vector2 s in MicroStoryMaps.Spots(hp.at, hp.count, hp.spread))
                {
                    Vector3 w = ClampXZ(storyOrigin + new Vector3(s.x, 0f, s.y), inner);
                    Vector3 p = new Vector3(w.x - center.x, groundY - center.y, w.z - center.z);
                    int before = flora.childCount;
                    if (town)
                    {
                        // (2026-11-05, ronda 8, Microcosmos_Bolivar: 182/183 flotando, 85 de más de 25 m, 2 en una calle) En un PUEBLO no se cae
                        // desde el aire: el Drop se quedaba con la cara más alta de debajo, y aquí eso son las capas del piso ×100 (9,5 m
                        // sobre el suelo del pueblo) o una meseta. Va al suelo libre más cercano a su sitio del mapa, como la flora del
                        // pueblo (FreeTown, fuera de calles y solares), o a la meseta si el mapa la pone encima («(sobre mueble)», regla 5).
                        float h = Mathf.Clamp(pl.cm / 100f, 0.03f, TownHandMaxH);
                        TownForm form = FormOf(pl, h);
                        TownRadii(form, h, out float blockR, out float spaceR);
                        Bounds? mesa = null;
                        if (tops != null) foreach (Bounds top in tops) if (InsideXZ(top, w, -1f) && !Covered(new Vector3(w.x, top.max.y, w.z))) { mesa = top; break; }
                        if (mesa.HasValue)
                        {
                            AddPt(w, spaceR);
                            pl.name = found.Value.name + " (sobre mueble)";
                            TownPlant(pl, new Vector3(p.x, mesa.Value.max.y - center.y, p.z), Mathf.Min(h, 15f), form, spaceR);
                            pl.name = found.Value.name;
                        }
                        else if (HandSpot(w, blockR, out Vector3 free, out float fy))
                        {
                            AddPt(free, spaceR);
                            TownPlant(pl, new Vector3(free.x - center.x, fy - center.y, free.z - center.z), h, form, spaceR);
                        }
                        else { handNoRoom++; continue; }
                    }
                    else
                    {
                        NativePlant(pl, p);
                        for (int c = before; c < flora.childCount; c++) _handPlaced.Add(flora.GetChild(c));
                    }
                    placed0++; handFlora++;
                }
            }
            Debug.Log($"[Mapa] {display}: {handFlora} plantas del mapa a mano en su sitio{(handNoRoom > 0 ? $"; {handNoRoom} sin suelo libre cerca" : "")}{(missing > 0 ? $"; {missing} rodales sin su planta en el bioma" : "")}.");
        }
        // El suelo libre del pueblo más cercano a w (en espiral, hasta ~70 m), fuera de calles y solares.
        bool HandSpot(Vector3 w, float r, out Vector3 free, out float fy)
        {
            const float golden = 2.39996323f;
            for (int k = 0; k <= 24; k++)
            {
                float d = 3f * k, a = k * golden;
                free = ClampXZ(w + new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d), inner);
                if (!FreeTown(free, r, out fy)) continue;
                bool taken = false;
                foreach (Bounds b in _streets) if (GapXZ(b, free) < r + 1f) { taken = true; break; }
                if (!taken) foreach (Bounds b in _townFeet) if (GapXZ(b, free) < r + 1f) { taken = true; break; }
                if (!taken) return true;
            }
            free = w; fy = groundY;
            return false;
        }
        // Una planta DEL LUGAR, a escala de pueblo (sus copas, a escala humana: lo mismo que en las afueras), sobre la cara de arriba
        // de un mueble gigante fuera del pueblo (2026-11-03). Lleva «(sobre mueble)» en el nombre: las pruebas del pueblo no la
        // cuentan como flotante (está apoyada en su meseta).
        bool TownTopSow(Vector3 p)
        {
            MicroBiome.Plant[] pool = biome != null ? biome.plants : fallbackTown;
            float sum = 0f; foreach (MicroBiome.Plant q in pool) if (q.kind != MicroBiome.Kind.Stone && q.kind != MicroBiome.Kind.Sand && q.kind != MicroBiome.Kind.Dung) sum += q.weight;
            if (sum <= 0f) return false;
            float pick = R(0f, sum); MicroBiome.Plant pl = pool[0];
            foreach (MicroBiome.Plant q in pool) { if (q.kind == MicroBiome.Kind.Stone || q.kind == MicroBiome.Kind.Sand || q.kind == MicroBiome.Kind.Dung) continue; pl = q; pick -= q.weight; if (pick <= 0f) break; }
            float h = Mathf.Min(Mathf.Max(0.03f, pl.cm / 100f) * R(0.85f, 1.15f), 15f);
            TownForm form = FormOf(pl, h);
            TownRadii(form, h, out float _, out float spaceR);
            Vector3 w = center + p;
            if (Crowded(w, spaceR)) return false;
            AddPt(w, spaceR);
            pl.name += " (sobre mueble)";
            TownPlant(pl, p, h, form, spaceR);
            return true;
        }
        // ¿Algo se alza justo encima de w (una lámpara sobre la mesa, el estante de arriba)? Ahí no cabe una planta.
        bool Covered(Vector3 w)
        {
            foreach (Bounds b in _blocked)
                if (b.min.y >= w.y - 0.1f && b.min.y < w.y + 4f && b.max.y > w.y + 0.1f && w.x > b.min.x - 1f && w.x < b.max.x + 1f && w.z > b.min.z - 1f && w.z < b.max.z + 1f) return true;
            return false;
        }

        // Una planta (o piedra, arena, boñiga) en p; false = suelo desnudo. <paramref name="always"/>: una plaza de flora siempre
        // lleva algo.
        bool Sow(Vector3 p, bool always)
        {
            if (town) return TownSow(p, always);   // el pueblo: a escala humana, escasa, fuera de calles y solares
            if (biome != null)   // FLORA NATIVA de su era y región (MicroBiome); un 12 % de suelo desnudo para pasar
            {
                if (R(0f, 1f) < 0.12f && !always) return false;
                float pick = R(0f, biomeWeight);
                foreach (MicroBiome.Plant pl in biome.plants) { pick -= pl.weight; if (pick <= 0f) { NativePlant(pl, p); break; } }
                return true;
            }
            float roll = R(0f, always ? 89f : 100f);
            if (roll < 27f) Grass(p);
            else if (roll < 40f) Moss(p);
            else if (roll < 50f) Fungus(p);
            else if (roll < 59f) Fern(p);
            else if (roll < 67f) Flower(p);
            else if (roll < 81f) Stone(p);
            else if (roll < 86f) { Transform a = Plant("Arena_fina", p); Box(a, "Arena", Vector3.up * 0.03f, new Vector3(9f, 0.06f, 9f), Quaternion.identity, sand, false); Affordance.Put(a.gameObject, AffordanceKind.SandPit, 1); sandPits.Add(p); }
            else if (roll < 89f) { Transform d = Plant("Boñiga", p); Box(d, "Estiercol", Vector3.up * 0.9f, new Vector3(4f, 1.8f, 3.5f), Quaternion.identity, dung, true); Affordance.Put(d.gameObject, AffordanceKind.Dung, 2, 3f, 0.001f); dungs.Add(p); }
            else return false;   // suelo desnudo: también hace falta sitio para pasar
            return true;
        }

        // Una planta NATIVA: su forma según el tipo (gramínea, musgo, seta, helecho, flor, mata, piedra, arena, boñiga,
        // escarcha), su porte real (1 cm ≈ 0,25 m) y sus colores; el nodo lleva su nombre común y científico.
        void NativePlant(MicroBiome.Plant pl, Vector3 p)
        {
            Material a = Mat("Bio_" + pl.name + "_1", pl.main), b = Mat("Bio_" + pl.name + "_2", pl.accent);
            float hh = Mathf.Clamp(pl.cm * 0.25f, 2f, 40f) * R(0.8f, 1.2f);
            Transform t = Plant(pl.name, p);
            switch (pl.kind)
            {
                case MicroBiome.Kind.Grass:
                {
                    float lean = R(-18f, 18f);
                    Box(t, "Tallo", Vector3.up * hh * 0.3f, new Vector3(0.25f, hh * 0.6f, 1.2f), Quaternion.Euler(lean * 0.4f, 0f, 0f), a, true);
                    Box(t, "Punta", new Vector3(0f, hh * 0.78f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.25f), new Vector3(0.2f, hh * 0.4f, 0.8f), Quaternion.Euler(lean, 0f, 0f), a, false);
                    Box(t, "Espiga", new Vector3(0f, hh * 0.95f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.35f), new Vector3(0.7f, Mathf.Max(1.5f, hh * 0.12f), 1.2f), Quaternion.Euler(lean, 0f, 0f), b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Grass, 2, 1f, 0.004f);
                    Affordance.Put(t.gameObject, AffordanceKind.PlantSap, 3, 1f, 0.01f);
                    Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
                    Transform seeds = Plant("Semillas", p + new Vector3(1.6f, 0f, 0f));
                    Box(seeds, "Semilla", Vector3.up * 0.25f, new Vector3(0.6f, 0.4f, 0.8f), Quaternion.identity, b, false);
                    Affordance.Put(seeds.gameObject, AffordanceKind.Seed, 2, 3f, 0.003f);
                    break;
                }
                case MicroBiome.Kind.Moss:
                    Box(t, "Cojin", Vector3.up * Mathf.Min(hh, 3f) * 0.25f, new Vector3(4f, Mathf.Min(hh, 3f) * 0.5f, 4f), Quaternion.identity, a, false);
                    for (int k = 0; k < 3; k++) Box(t, "Punta", new Vector3(R(-1.4f, 1.4f), Mathf.Min(hh, 3f) * 0.55f, R(-1.4f, 1.4f)), new Vector3(0.3f, 0.5f, 0.3f), Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                case MicroBiome.Kind.Fungus:
                    Box(t, "Pie", Vector3.up * hh * 0.5f, new Vector3(hh * 0.15f, hh, hh * 0.15f), Quaternion.identity, a, true);
                    Box(t, "Sombrero", Vector3.up * hh, new Vector3(hh * 0.8f, hh * 0.18f, hh * 0.8f), Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Fungus, 3, 2f, 0.003f);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                case MicroBiome.Kind.Fern:
                    Box(t, "Tallo", Vector3.up * hh * 0.5f, new Vector3(0.6f, hh, 0.6f), Quaternion.identity, b, true);
                    for (int k = 0; k < 7; k++) Box(t, "Fronda", Vector3.up * hh + Quaternion.Euler(0f, k * 51f, 0f) * new Vector3(0f, -1.5f, hh * 0.25f), new Vector3(2.4f, 0.2f, hh * 0.5f), Quaternion.Euler(-20f, k * 51f, 0f), a, false);
                    Affordance.Put(t.gameObject, AffordanceKind.PlantSap, 4, 1f, 0.01f);
                    Affordance.Put(t.gameObject, AffordanceKind.Leaf, 3, 3f, 0.002f);
                    Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                case MicroBiome.Kind.Flower:
                    Box(t, "Tallo", Vector3.up * hh * 0.5f, new Vector3(0.4f, hh, 0.4f), Quaternion.identity, a, true);
                    for (int k = 0; k < 5; k++) Box(t, "Petalo", Vector3.up * hh + Quaternion.Euler(0f, k * 72f, 0f) * new Vector3(0f, 0f, 2.2f), new Vector3(2f, 0.15f, 3.2f), Quaternion.Euler(-12f, k * 72f, 0f), b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Flower, 2, 1f, 0.004f);
                    Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
                    break;
                case MicroBiome.Kind.Shrub:   // mata leñosa: un cojín de hojas con sus flores o bayas
                    Box(t, "Mata", Vector3.up * hh * 0.45f, new Vector3(hh * 0.8f, hh * 0.9f, hh * 0.8f), Quaternion.identity, a, true);
                    for (int k = 0; k < 5; k++) Box(t, "Flor_o_baya", new Vector3(R(-0.35f, 0.35f) * hh, hh * R(0.6f, 0.92f), R(-0.35f, 0.35f) * hh), Vector3.one * Mathf.Max(0.6f, hh * 0.07f), Quaternion.identity, b, false);
                    Affordance.Put(t.gameObject, AffordanceKind.Leaf, 3, 3f, 0.002f);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
                    break;
                case MicroBiome.Kind.Stone:
                {
                    float sz = R(1.5f, 6f);
                    Box(t, "Roca", Vector3.up * sz * 0.4f, new Vector3(sz, sz * 0.8f, sz * 1.1f), Quaternion.Euler(R(0f, 25f), 0f, R(0f, 15f)), a, true);
                    Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
                    break;
                }
                case MicroBiome.Kind.Sand:
                    Box(t, "Arena", Vector3.up * 0.03f, new Vector3(9f, 0.06f, 9f), Quaternion.identity, a, false);
                    Affordance.Put(t.gameObject, AffordanceKind.SandPit, 1); sandPits.Add(p);
                    break;
                case MicroBiome.Kind.Dung:
                    Box(t, "Bulto", Vector3.up * 0.9f, new Vector3(4f, 1.8f, 3.5f), Quaternion.identity, a, true);
                    Affordance.Put(t.gameObject, AffordanceKind.Dung, 2, 3f, 0.001f); dungs.Add(p);
                    break;
                case MicroBiome.Kind.Snow:
                    Box(t, "Escarcha", Vector3.up * 0.04f, new Vector3(R(8f, 14f), 0.08f, R(8f, 14f)), Quaternion.identity, a, false);
                    break;
            }
        }

        // Cada planta: un nodo SIN escala en su pie (lleva los Affordance) y sus piezas como hijos (sin deformarse).
        Transform Plant(string name, Vector3 p) { var g = new GameObject(name).transform; g.SetParent(flora, false); g.localPosition = p; g.localRotation = Quaternion.Euler(0f, R(0f, 360f), 0f); return g; }

        void Grass(Vector3 p)
        {
            float hh = R(10f, 25f), lean = R(-18f, 18f);
            Transform b = Plant("Brizna", p);
            Box(b, "Tallo", Vector3.up * hh * 0.3f, new Vector3(0.25f, hh * 0.6f, 1.2f), Quaternion.Euler(lean * 0.4f, 0f, 0f), rnd.Next(3) == 0 ? grassDry : grass, true);
            Box(b, "Punta", new Vector3(0f, hh * 0.78f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.25f), new Vector3(0.2f, hh * 0.4f, 0.8f), Quaternion.Euler(lean, 0f, 0f), grass, false);
            Affordance.Put(b.gameObject, AffordanceKind.Grass, 2, 1f, 0.004f);
            Affordance.Put(b.gameObject, AffordanceKind.PlantSap, 3, 1f, 0.01f);
            Affordance.Put(b.gameObject, AffordanceKind.Perch, 1);
            if (rnd.Next(2) == 0)
            {
                // Espiga madura: las semillas caen al pie y los mirmidones las cosechan.
                Box(b, "Espiga", new Vector3(0f, hh * 0.95f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.35f), new Vector3(0.7f, 2.5f, 1.2f), Quaternion.Euler(lean, 0f, 0f), seedMat, false);
                Transform seeds = Plant("Semillas", p + new Vector3(1.6f, 0f, 0f));
                Box(seeds, "Semilla", Vector3.up * 0.25f, new Vector3(0.6f, 0.4f, 0.8f), Quaternion.identity, seedMat, false);
                Box(seeds, "Semilla", new Vector3(0.7f, 0.2f, 0.3f), new Vector3(0.5f, 0.35f, 0.7f), Quaternion.Euler(0f, 40f, 0f), seedMat, false);
                Affordance.Put(seeds.gameObject, AffordanceKind.Seed, 2, 3f, 0.003f);
            }
        }
        void Moss(Vector3 p)
        {
            Transform m = Plant("Musgo", p);
            Box(m, "Cojin", Vector3.up * 0.5f, new Vector3(3f, 1f, 3f), Quaternion.identity, moss, false);
            for (int k = 0; k < 3; k++) Box(m, "Musgo_punta", new Vector3(R(-1f, 1f), 1.15f, R(-1f, 1f)), new Vector3(0.25f, 0.5f, 0.25f), Quaternion.identity, mossTip, false);
            Affordance.Put(m.gameObject, AffordanceKind.Shelter, 2);
        }
        void Fungus(Vector3 p)
        {
            float hh = R(2f, 6f);
            Transform f = Plant("Hongo", p);
            Box(f, "Pie", Vector3.up * hh * 0.5f, new Vector3(hh * 0.15f, hh, hh * 0.15f), Quaternion.identity, stem, true);
            Box(f, "Sombrero", Vector3.up * hh, new Vector3(hh * 0.8f, hh * 0.18f, hh * 0.8f), Quaternion.identity, cap, false);
            Affordance.Put(f.gameObject, AffordanceKind.Fungus, 3, 2f, 0.003f);
            Affordance.Put(f.gameObject, AffordanceKind.Shelter, 2);
        }
        void Fern(Vector3 p)
        {
            float hh = R(12f, 22f);
            Transform t = Plant("Helecho", p);
            Box(t, "Tallo", Vector3.up * hh * 0.5f, new Vector3(0.6f, hh, 0.6f), Quaternion.identity, fernStem, true);
            for (int k = 0; k < 7; k++) Box(t, "Fronda", Vector3.up * hh + Quaternion.Euler(0f, k * 51f, 0f) * new Vector3(0f, -1.5f, 5f), new Vector3(2.4f, 0.2f, 11f), Quaternion.Euler(-20f, k * 51f, 0f), fern, false);
            Affordance.Put(t.gameObject, AffordanceKind.PlantSap, 4, 1f, 0.01f);
            Affordance.Put(t.gameObject, AffordanceKind.Leaf, 3, 3f, 0.002f);
            Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
            Affordance.Put(t.gameObject, AffordanceKind.Shelter, 2);
        }
        void Flower(Vector3 p)
        {
            float hh = R(8f, 16f);
            Transform t = Plant("Flor", p);
            Box(t, "Tallo", Vector3.up * hh * 0.5f, new Vector3(0.4f, hh, 0.4f), Quaternion.identity, fernStem, true);
            Material pm = rnd.Next(2) == 0 ? petal : petal2;
            for (int k = 0; k < 5; k++) Box(t, "Petalo", Vector3.up * hh + Quaternion.Euler(0f, k * 72f, 0f) * new Vector3(0f, 0f, 2.2f), new Vector3(2f, 0.15f, 3.2f), Quaternion.Euler(-12f, k * 72f, 0f), pm, false);
            Box(t, "Centro", Vector3.up * (hh + 0.2f), new Vector3(1.8f, 0.5f, 1.8f), Quaternion.identity, pollen, false);
            Affordance.Put(t.gameObject, AffordanceKind.Flower, 2, 1f, 0.004f);
            Affordance.Put(t.gameObject, AffordanceKind.Perch, 1);
        }
        void Stone(Vector3 p)
        {
            float sz = R(1.5f, 6f);
            Transform g = Plant("Piedra", p);
            Box(g, "Roca", Vector3.up * sz * 0.4f, new Vector3(sz, sz * 0.8f, sz * 1.1f), Quaternion.Euler(R(0f, 25f), 0f, R(0f, 15f)), rock, true);
            Affordance.Put(g.gameObject, AffordanceKind.Shelter, 2);
        }

        // FAUNA: cada bicho cuelga de un padre MicroSize que lo mantiene a su tamaño relativo a la hormiga (el prefab
        // fija su propia escala al nacer; por eso antes se veían diminutos).
        var fauna = new GameObject("Fauna_a_escala").transform; fauna.SetParent(root, false); fauna.localPosition = center;
        (string sp, int n, float len)[] generic = { ("Antlion", 5, 3f), ("DungBeetle", 5, 5f), ("Termite", 10, 1.6f), ("Mantis", 3, 10f), ("Cricket", 5, 5f), ("Grasshopper", 5, 7f), ("Aphid", 8, 1f), ("Ladybug", 5, 2.4f) };
        // Con bioma, su FAUNA NATIVA (activa en la estación de la historia): cada especie real con el cuerpo de su grupo.
        var list = new List<(string sp, int n, float len, Color tint, string name, string dormant, bool street, bool handAt, Vector2 at, float spread)>();
        // En el PUEBLO, los bichos del lugar no pasan de la talla de un animal grande del pueblo (2,4 m, un caballo): a escala de
        // hormiga una mantis medía 12 m y paseaba entre las casas de Kowloon como un monstruo, al lado de Bruce Lee (1,7 m).
        if (hand) foreach (MicroStoryMaps.Animal ha in map.fauna) list.Add((ha.species, ha.count, town ? Mathf.Min(ha.length, 2.4f) : ha.length, SpeciesColor(ha.species), ha.name, null, false, true, ha.at, ha.spread));   // EL MAPA A MANO
        else if (biome != null) foreach (MicroBiome.Bug bg in biome.bugs) list.Add((bg.species, bg.count, town ? Mathf.Min(bg.Length, 2.4f) : bg.Length, bg.tint, bg.name, bg.dormant, false, false, default, 0f));
        else foreach (var (gs, gn, gl) in generic) list.Add((gs, gn, gl, SpeciesColor(gs), gs, null, false, false, default, 0f));
        // LOS ANIMALES DEL PUEBLO (Biome.townFauna: los caballos de los carros, los asnos, los gatos…), a su talla real y por las
        // CALLES (Berón 2026-11-01: «al-Juarismi, Curie, Einstein y Patterson no tienen fauna»).
        if (town && !hand && biome != null && biome.townFauna != null)
            foreach (MicroBiome.Pet pt in biome.townFauna) list.Add((pt.species, pt.count, pt.length, pt.tint, pt.name, null, true, false, default, 0f));
        // (2026-11-03) Las MESETAS de los muebles gigantes fuera del pueblo también tienen su fauna (reglas de los microcosmos): en un pueblo
        // sin bichos del lugar (París, Berna, Bagdad, Pasadena: solo los animales de la calle), unos pocos del santuario gigante, allí arriba.
        if ((town || hand) && tops != null && tops.Count > 0)
        {
            list.Add(("Ladybug", 2, 2.4f, SpeciesColor("Ladybug"), "Mariquita de las mesetas", null, false, false, default, 0f));
            list.Add(("Ant", 2, 1.8f, SpeciesColor("Ant"), "Hormiga de las mesetas", null, false, false, default, 0f));
        }
        int placed = 0, faunaNext = 0, faunaTop = 0, onStreets = 0;
        foreach (var (sp, n, len, tint, label, dormant, street, handAt, hat, hspread) in list)
        {
            GameObject prefab = AnimalPrefabGenerator.LoadOrCreate(sp);   // lo genera si falta (especies nuevas)
            List<Vector2> handSpots = handAt ? MicroStoryMaps.Spots(hat, n, hspread) : null;   // (el mapa a mano: cada uno en su sitio)
            for (int i = 0; i < n; i++)
            {
                // Cada uno nace junto a lo que usa: la hormiga león en la arena fina, el pelotero junto a una boñiga; el resto, en la
                // siguiente plaza de fauna del piso y, tomadas todas, en suelo libre DENTRO de los muros (nunca fuera: regla 11).
                Vector3 p;
                if (handAt) { Vector3 w = storyOrigin + new Vector3(handSpots[i].x, 0f, handSpots[i].y); p = new Vector3(w.x - center.x, (town ? FloorNearOr(w, floors, groundY, groundY) : groundY) - center.y, w.z - center.z); }
                else if (street && _streets.Count > 0)
                {
                    // Por una calle (a lo largo de un tramo al azar, por su centro: a ≥ 2 m de los solares).
                    Bounds sb = _streets[rnd.Next(_streets.Count)];
                    Vector3 w = new Vector3(R(sb.min.x + 0.8f, sb.max.x - 0.8f), groundY, R(sb.min.z + 0.8f, sb.max.z - 0.8f));
                    if (!InsideXZ(inner, w, 0f)) w = ClampXZ(w, inner);
                    p = new Vector3(w.x - center.x, FloorNearOr(w, floors, groundY, groundY) - center.y, w.z - center.z); onStreets++;
                }
                else if (sp == "Antlion" && sandPits.Count > 0) p = sandPits[rnd.Next(sandPits.Count)];
                else if (topPts.Count > 0 && sp != "DungBeetle" && (i % 3 == 2 || town && i == 0 && faunaTop < 4 || label.EndsWith("de las mesetas"))) {   // (en un pueblo, el primero de cada especie también, hasta 4: las mesetas fuera del pueblo tienen su fauna)
                    p = topPts[rnd.Next(topPts.Count)] - center; faunaTop++; }   // uno de cada tres, encima de un mueble
                else if (sp == "DungBeetle" && dungs.Count > 0) p = dungs[rnd.Next(dungs.Count)] + new Vector3(town ? 0.6f : 5f, 0f, 0f);
                else if (faunaSlots != null && faunaNext < faunaSlots.Count) { Vector3 w = faunaSlots[faunaNext++]; p = new Vector3(w.x - center.x, FloorNearOr(w, floors, w.y, groundY) - center.y, w.z - center.z); }
                else
                {
                    Vector3 w = new Vector3(R(inner.min.x, inner.max.x), groundY, R(inner.min.z, inner.max.z)); float fy = groundY;
                    for (int k = 0; k < 30 && !FreeFloor(w, out fy); k++) w = new Vector3(R(inner.min.x, inner.max.x), groundY, R(inner.min.z, inner.max.z));
                    p = new Vector3(w.x - center.x, fy - center.y, w.z - center.z);
                }
                Vector3 pw = ClampXZ(center + p, inner); p = pw - center;   // dentro de los muros, siempre
                if (prefab == null) continue;
                var holder = new GameObject($"{label}_a_escala").transform; holder.SetParent(fauna, false); holder.localPosition = p;
                holder.gameObject.AddComponent<MicroSize>().length = len;
                GameObject a = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                a.transform.SetParent(holder, false); a.transform.localPosition = Vector3.zero;
                // FORMA REAL (docs/insect-morphology.md): el cuerpo de cubos articulado de su especie sustituye al modelo;
                // la conducta, el ciclo de vida y la biología siguen siendo los del prefab.
                foreach (Renderer x in a.GetComponentsInChildren<Renderer>(true)) x.enabled = false;
                // POBLACIÓN MEZCLADA: la mayoría ADULTOS (erguidos cuando están lúcidos) y algunas crías (larva, ninfa o
                // joven según su desarrollo). Antes el prefab nacía sin etapa → todos jóvenes, nadie erguido.
                Animal an = a.GetComponent<Animal>();
                if (an != null) an.lifeStage = i < Mathf.Max(1, Mathf.CeilToInt(n * 0.7f)) ? LifeStage.adult : LifeStage.child;
                CubeInsect.Dress(a, sp, tint, 1f, an != null ? Metamorphosis.PhaseOf(an) : Metamorphosis.Phase.Adult, an == null || an.sex != Sex.male);
                if (dormant != null) a.AddComponent<Dormant>().why = dormant;   // aletargado en esta estación: está, pero quieto
                Bounds bb = default; bool any = false;
                foreach (Renderer x in a.GetComponentsInChildren<Renderer>()) { if (!x.enabled) continue; if (!any) { bb = x.bounds; any = true; } else bb.Encapsulate(x.bounds); }
                // A su largo de cuerpo (el de la raíz «Insecto», sin la caja de patas y alas), como lo mantiene MicroSize.
                Transform ins = a.transform.Find("Insecto");
                if (ins != null && ins.lossyScale.z > 1e-6f) holder.localScale = Vector3.one * (len / ins.lossyScale.z);
                else if (any) { float now = Mathf.Max(bb.size.x, bb.size.z); if (now > 1e-4f) holder.localScale = Vector3.one * (len / now); }
                if (handAt && !town) _handPlaced.Add(holder);   // (en un pueblo, en su suelo: ver la flora del mapa a mano)
                placed++;
            }
        }
        _faunaOnSlots = faunaNext; _faunaOnTops = faunaTop;
        Debug.Log($"[Microcosmos] Flora interactuable: {placed0} plantas, piedras, arenas y boñigas dentro del piso ({inner.size.x:0} × {inner.size.z:0} m útiles{(floraSlots != null ? $"; {onSlots} en las plazas de flora del área" : "")}; {onTops} plantas y {faunaTop} bichos encima de {(tops != null ? tops.Count : 0)} muebles)" + (biome != null ? $" · bioma: {biome.title}, {biome.plants.Length} plantas y {biome.bugs.Length} bichos nativos." : ".")
                  + (town ? $" · PUEBLO: flora a escala de pueblo, {townTrees} árboles y {townSmall} matas, céspedes y parterres fuera de calles y solares; {onStreets} animales del pueblo por {_streets.Count} calles." : ""));
        return placed;
    }

    // Las formas de la flora del pueblo (escala humana): ver Nature, modo pueblo.
    enum TownForm { Tree, Palm, Bush, Clump, Lawn, Bed, Patch, Mushroom, Rock, Sand, Dung, Frost }

    static TownForm FormOf(MicroBiome.Plant pl, float h)
    {
        switch (pl.kind)
        {
            case MicroBiome.Kind.Shrub:
            case MicroBiome.Kind.Fern:
                if (h > 3f) return pl.name.StartsWith("Palmera") || pl.name.StartsWith("Moriche") ? TownForm.Palm : TownForm.Tree;
                return h > 0.6f || pl.kind == MicroBiome.Kind.Shrub ? TownForm.Bush : TownForm.Clump;
            case MicroBiome.Kind.Flower: return h > 3f ? TownForm.Tree : h > 0.6f ? TownForm.Bush : TownForm.Bed;
            case MicroBiome.Kind.Grass: return h <= 0.35f ? TownForm.Lawn : TownForm.Clump;
            case MicroBiome.Kind.Moss: return TownForm.Patch;
            case MicroBiome.Kind.Fungus: return TownForm.Mushroom;
            case MicroBiome.Kind.Stone: return TownForm.Rock;
            case MicroBiome.Kind.Sand: return TownForm.Sand;
            case MicroBiome.Kind.Dung: return TownForm.Dung;
            default: return TownForm.Frost;
        }
    }

    /// <summary>El radio de su huella en el suelo (lo que no puede pisar calle ni solar: el tronco, o la copa si cuelga por debajo
    /// de 2,5 m) y el de su copa o mata (la distancia a la planta vecina).</summary>
    static void TownRadii(TownForm f, float h, out float block, out float space)
    {
        switch (f)
        {
            case TownForm.Tree: space = Mathf.Clamp(h * 0.28f, 1.2f, 4.5f); block = h - h * 0.55f >= 2.5f ? 0.6f : space; break;
            case TownForm.Palm: space = Mathf.Clamp(h * 0.18f, 1.2f, 3.5f); block = 0.5f; break;
            case TownForm.Bush: space = Mathf.Clamp(h * 0.45f, 0.25f, 1.4f); block = space; break;
            case TownForm.Clump: space = block = 0.35f; break;
            case TownForm.Lawn: space = block = 1.6f; break;
            case TownForm.Bed: space = block = 0.75f; break;
            case TownForm.Patch: space = block = 0.8f; break;
            case TownForm.Mushroom: space = block = 0.15f; break;
            case TownForm.Rock: space = block = 0.4f; break;
            case TownForm.Sand: space = block = 1.2f; break;
            case TownForm.Dung: space = block = 0.25f; break;
            default: space = block = 1.5f; break;
        }
    }

    /// <summary>Distancia en XZ de un punto a una caja (0 si cae dentro).</summary>
    static float GapXZ(Bounds b, Vector3 p)
    {
        float dx = Mathf.Max(0f, Mathf.Max(b.min.x - p.x, p.x - b.max.x)), dz = Mathf.Max(0f, Mathf.Max(b.min.z - p.z, p.z - b.max.z));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    static int _faunaOnSlots;   // cuántos bichos fueron a las plazas de fauna (el último Nature)
    // (2026-11-04) Lo que el último Nature puso desde el MAPA A MANO (MicroStoryMaps): cae desde el aire en Build (MicroGround.Drop).
    static readonly List<Transform> _handPlaced = new List<Transform>();
    /// <summary>Lado del vóxel del NavMesh de un microcosmos (m). Más grande, más rápido y menos fino en los pasos estrechos: no bajar de 0,25 sin mirar la regla de los 2 m.</summary>
    const float MicroNavVoxel = 0.25f;
    // La planta del bioma cuyo nombre empieza por <paramref name="name"/> (sus plantas y las del pueblo), o null.
    static MicroBiome.Plant? FindPlant(MicroBiome.Biome b, string name)
    {
        if (b == null || string.IsNullOrEmpty(name)) return null;
        foreach (MicroBiome.Plant[] arr in new[] { b.plants, b.town }) if (arr != null) foreach (MicroBiome.Plant p in arr) if (p.name.StartsWith(name)) return p;
        return null;
    }
    static int _floraOnTops, _faunaOnTops;   // plantas y bichos encima de los muebles gigantes (el último Nature)

    static bool InsideXZ(Bounds b, Vector3 p, float margin) => p.x > b.min.x - margin && p.x < b.max.x + margin && p.z > b.min.z - margin && p.z < b.max.z + margin;

    static Color SpeciesColor(string sp)
    {
        switch (sp)
        {
            case "Ladybug": return new Color(0.85f, 0.12f, 0.1f);
            case "DungBeetle": return new Color(0.15f, 0.12f, 0.1f);
            case "Grasshopper": return new Color(0.45f, 0.62f, 0.25f);
            case "Cricket": return new Color(0.22f, 0.16f, 0.1f);
            case "Mantis": return new Color(0.4f, 0.68f, 0.3f);
            case "Antlion": return new Color(0.62f, 0.52f, 0.38f);
            case "Termite": return new Color(0.9f, 0.85f, 0.72f);
            case "Aphid": return new Color(0.55f, 0.78f, 0.35f);
            default: return new Color(0.3f, 0.2f, 0.15f);
        }
    }

    internal static Transform Box(Transform parent, string name, Vector3 local, Vector3 size, Material m, bool solid) => Box(parent, name, local, size, Quaternion.identity, m, solid);

    internal static Transform Box(Transform parent, string name, Vector3 local, Vector3 size, Quaternion rot, Material m, bool solid)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localRotation = rot; g.transform.localScale = size;
        if (!solid) Object.DestroyImmediate(g.GetComponent<Collider>());
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g.transform;
    }

    static readonly Dictionary<string, Material> _mats = new Dictionary<string, Material>();
    internal static Material Mat(string name, Color c)
    {
        if (_mats.TryGetValue(name, out Material cached) && cached != null) return cached;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh) { name = name, color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return _mats[name] = m;
    }
}
