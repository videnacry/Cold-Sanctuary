using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LAS ACTIVIDADES DE VERDAD (grupo 97; docs/actividades-y-uniformes.md; Berón 2026-11-04: «parece que solo van al piso que
/// les toca y no hacen nada … sería posible que los humanos se coloquen uniformes»). En cualquier escena con personal:
///  1. LA COMIDA (humano): bandeja → sentarse → cubiertos → bocados (el brazo sube y baja, la comida del plato MENGUA, el hambre baja) →
///     beber → recoger → devolver; todos los pasos en orden y la vajilla fuera de las manos al final.
///  2. LAS MISIONES: N misiones del catálogo diario (las obligatorias primero) puestas como las pone la jornada a quien ya está en su
///     puesto (WorkdayDirector.PerformForTest): a los T s hay una actuación en marcha (no de pie quieto): pasos empezados, la herramienta
///     del paso en la mano, los huesos del brazo moviéndose y, si el paso lo lleva, su efecto.
///  3. EL UNIFORME: se pone (los cubos de la ropa cambian de color, aparecen las piezas «Uniforme_…», el pelo y la cara no cambian) y se
///     quita (todo vuelve a su material). Y el reparto: qué uniforme lleva cada tarea.
///  4. EL INSECTO-HUMANO: un insecto-humano de prueba (o uno de un microcosmos cargado) come con su orquesta (cuchara, bocados, el plato
///     mengua) y trabaja con su actuación (herramienta en la mano, brazos, efecto) y su delantal; si hay un MicroJornada en la escena, a
///     la hora de comer recoge su bandeja.
/// Todas las esperas, acotadas (tiempo real). Se añade sola al orquestador.
/// </summary>
public class ActividadesTest : MonoBehaviour, ITestUnit
{
    public int Group => 97;
    public bool ParallelSafe => false;
    [Tooltip("Misiones que se prueban (las primeras del catálogo diario, obligatorias primero).")]
    public int sampleMissions = 10;
    [Tooltip("Segundos tras «llegar» en que debe verse la actuación.")]
    public float performWithin = 2.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<ActividadesTest>() == null) tr.gameObject.AddComponent<ActividadesTest>();
    }

    static void Log(string s) => Debug.Log("[Actividades] " + s);
    readonly List<GameObject> _temp = new List<GameObject>();

    public IEnumerator Run()
    {
        TestProbe.Begin("Actividades: la orquesta de la comida, las misiones que se hacen de verdad y los uniformes");
        GameObject human = PickHuman();
        if (TestProbe.Check("hay un humano de cubos para la prueba", human != null))
        {
            yield return HumanMeal(human);
            yield return Missions();
            yield return Uniform(human);
        }
        UniformTable();
        yield return Insect();
        foreach (GameObject g in _temp) if (g != null) Destroy(g);
        _temp.Clear();
        TestProbe.End();
    }

    // Un miembro del personal con cuerpo de cubos (no el jugador ni la Magnate), quieto: sin nada en su cola.
    static GameObject PickHuman()
    {
        foreach (string n in WorkdayDirector.Staff)
        {
            if (n == "Player") continue;
            GameObject g = WorkdayDirector.Body(n);
            if (g != null && g.activeInHierarchy && ActivityKit.IsHuman(g) && g.GetComponent<Anima>() != null && !g.GetComponent<Anima>().death) return g;
        }
        foreach (Anima a in FindObjectsByType<Anima>(FindObjectsSortMode.None))
            if (a != null && !a.death && !a.CompareTag("Player") && ActivityKit.IsHuman(a.gameObject) && a.GetComponent<Magnate>() == null) return a.gameObject;
        return null;
    }

    static void Quiet(GameObject g)
    {
        if (g.TryGetComponent(out ActionQueue q)) { q.ClearOwn(); q.ClearPlayer(); }
        if (g.TryGetComponent(out IdleBehavior idle)) idle.enabled = false;
    }

    GameObject Table(Vector3 at, float s)
    {
        GameObject t = GameObject.CreatePrimitive(PrimitiveType.Cube);
        t.name = "Mesa_prueba_actividades";
        t.transform.position = at + Vector3.up * 0.375f * s;
        t.transform.localScale = new Vector3(1.2f, 0.75f, 0.8f) * s;
        _temp.Add(t);
        return t;
    }

    static float Feet(GameObject g) { Transform b = g.transform.Find(CubeHumanoid.BodyName); return b != null ? b.position.y : g.transform.position.y; }

    // ── 1. La comida de un humano ───────────────────────────────────────────────────────────────────────────────
    IEnumerator HumanMeal(GameObject g)
    {
        Quiet(g);
        BodyMetabolism body = g.GetComponent<BodyMetabolism>();
        if (body != null) body.hunger = 0.9f;
        float hunger0 = body != null ? body.hunger : 0f;
        Vector3 fw = g.transform.forward; fw.y = 0f; fw = fw.sqrMagnitude > 0.01f ? fw.normalized : Vector3.forward;
        GameObject table = Table(new Vector3(g.transform.position.x, Feet(g), g.transform.position.z) + fw * 0.9f, 1f);
        var meal = new MealRitual("arroz con verduras");
        meal.TakeTray(g, true, 0.75f);
        for (float t = 0f; t < 0.5f; t += Time.deltaTime) { meal.Carry(g); yield return null; }
        meal.SitDown(g, table.transform, true);
        TestProbe.Check("comida: cubiertos en las manos (tenedor y cuchillo)", ActivityKit.Held(g, true) == ActivityKit.Prop.Fork && ActivityKit.Held(g, false) == ActivityKit.Prop.Knife,
            $"{ActivityKit.Held(g, true)} / {ActivityKit.Held(g, false)}");
        CreatureRig rig = g.GetComponent<CreatureRig>();
        Transform fore = rig != null ? rig.Get(BodyPart.ForearmRight) : null;
        float arc = 0f, midScale = -1f; Quaternion? f0 = null;
        float until = Time.time + 6f, stop = Time.realtimeSinceStartup + 20f;
        bool done = false;
        while (!done && Time.realtimeSinceStartup < stop)
        {
            done = meal.Eat(g, until);
            if (fore != null) { if (f0 == null) f0 = fore.localRotation; else arc = Mathf.Max(arc, Quaternion.Angle(f0.Value, fore.localRotation)); }
            if (midScale < 0f && meal.bitesTaken >= MealRitual.Bites / 2) midScale = meal.FoodScale;
            yield return null;
        }
        TestProbe.Check("comida: se come todos los bocados en su tiempo", done && meal.bitesTaken == MealRitual.Bites, $"{meal.bitesTaken}/{MealRitual.Bites}");
        TestProbe.Check("comida: la comida del plato mengua (a mitad < 70 %, al final 0)", midScale >= 0f && midScale < 0.7f && meal.FoodScale <= 0.01f, $"a mitad {midScale:0.00}, al final {meal.FoodScale:0.00}");
        TestProbe.Check("comida: el brazo sube a la boca y baja al plato (codo > 50° de recorrido)", arc > 50f, $"recorrido del antebrazo {arc:0}°");
        TestProbe.Check("comida: bebe entre bocados", meal.sips >= 2, $"{meal.sips} tragos");
        if (body != null) TestProbe.Check("comida: el hambre baja con los bocados (Metabolism del cuerpo)", body.hunger < hunger0 - 0.5f, $"{hunger0:0.00} → {body.hunger:0.00}");
        meal.PickUp(g);
        yield return null;
        meal.Return(g);
        yield return null;
        string want = "bandeja → sentarse → cubiertos → bocado → beber → recoger → devolver";
        TestProbe.Check("comida: todos los pasos en orden", string.Join(" → ", meal.steps) == want, string.Join(" → ", meal.steps));
        TestProbe.Check("comida: al devolver, las manos vacías", ActivityKit.Held(g, true) == ActivityKit.Prop.None && ActivityKit.Held(g, false) == ActivityKit.Prop.None);
        if (rig != null) DailyPoses.Stand().ApplyFinal(g.GetComponent<Anima>());
        // El restaurante usa la misma orquesta (DineSpell.Meal) y la casa también (HomeNeedSpell.Meal): se miran en vivo en la jornada.
        Log($"comida de «{g.name}»: {string.Join(" → ", meal.steps)}; {meal.bitesTaken} bocados, {meal.sips} tragos.");
    }

    // ── 2. Las misiones se hacen ────────────────────────────────────────────────────────────────────────────────
    IEnumerator Missions()
    {
        var all = AreaFunctions.SanctuaryMissions().ToList();
        // (la clase de Hatha —guía, demostrador y correctores— la lleva YogaClass con su secuencia: no pasa por la actuación)
        var sample = all.Where(f => f.mandatory && !OwnSystem(f.id)).Concat(all.Where(f => !f.mandatory && !OwnSystem(f.id))).Take(Mathf.Max(1, sampleMissions)).ToList();
        int ok = 0, own = 0; var bad = new List<string>();
        GameObject g = PickHuman();
        if (g == null) yield break;
        CreatureRig rig = g.GetComponent<CreatureRig>();
        foreach (var f in sample)
        {
            Quiet(g);
            ActivityPerformer.Stop(g);
            float t0 = Time.realtimeSinceStartup, end = t0 + performWithin;
            var seen = new HashSet<string>(); int effects = 0; float spread = 0f;
            var firsts = new Dictionary<BodyPart, Quaternion>();
            ActivityKit.Prop held = ActivityKit.Prop.None; ActivityPerformer.Step step = null;
            while (Time.realtimeSinceStartup < end)
            {
                WorkdayDirector.PerformForTest(g, f.area, f.title, f.id, dress: false);
                ActivityPerformer ap = ActivityPerformer.Of(g);
                if (ap != null)
                {
                    seen.Add(ap.Doing); effects = Mathf.Max(effects, ap.effects); step = ap.Current;
                    held = ActivityKit.Held(g, true) != ActivityKit.Prop.None ? ActivityKit.Held(g, true) : ActivityKit.Held(g, false);
                    // El gesto se mueve: el mayor recorrido de cualquiera de sus huesos desde el primer fotograma.
                    if (rig != null && step != null && step.arms != null)
                        foreach (var b in step.arms.bones)
                        {
                            Transform bone = rig.Get(b.part); if (bone == null) continue;
                            if (!firsts.TryGetValue(b.part, out Quaternion q0)) firsts[b.part] = bone.localRotation;
                            else spread = Mathf.Max(spread, Quaternion.Angle(q0, bone.localRotation));
                        }
                }
                yield return null;
            }
            ActivityPerformer now = ActivityPerformer.Of(g);
            bool performing = now != null && now.performance != null;
            bool tool = step == null || (step.right == ActivityKit.Prop.None && step.left == ActivityKit.Prop.None) || held != ActivityKit.Prop.None;
            bool moving = step == null || step.arms == null || spread > 4f;
            bool fx = step == null || step.fx == ActivityKit.Fx.None || effects > 0 || step.fxEvery > performWithin;
            if (Performances.HasOwnSteps(f.id)) own++;
            if (performing && tool && moving && fx) ok++;
            else bad.Add($"{f.id} (actúa {performing}, herramienta {tool}: {held}, brazo {spread:0}°, efectos {effects})");
            Log($"«{f.id}» → {(now != null ? now.performance.title : "nada")}: {string.Join(", ", seen)}; en la mano {held}; brazo {spread:0}°; efectos {effects}.");
        }
        ActivityPerformer.Stop(g);
        TestProbe.Check($"misiones: las {sample.Count} probadas se HACEN a los {performWithin:0.0} s de llegar (actuación, herramienta, brazos y efecto)", ok == sample.Count, bad.Count > 0 ? string.Join(" · ", bad) : $"{ok}/{sample.Count}");
        TestProbe.Check("misiones: las probadas tienen pasos propios (no el bucle genérico)", own == sample.Count, $"{own}/{sample.Count}");
        int total = all.Count, withSteps = all.Count(f => Performances.HasOwnSteps(f.id));
        Log($"catálogo diario: {withSteps}/{total} misiones con pasos propios; el resto, bucle genérico de su clase.");
        TestProbe.Check("misiones: todo el catálogo diario tiene actuación (propia o genérica)", all.All(f => Performances.ForMission(f.id) != null));
        TestProbe.Check("misiones: las del día y las de prefijo también (material, obras, obrador)", new[] { "mat_guadua", "mat_bahareque", "mat_micelio", "mat_cuerda", "obra_material_Muro", "obra_trabajo_Muro", "prep_pan" }.All(Performances.HasOwnSteps));
        // En cuanto deja de llamarlo la jornada (se va a comer, a otra misión), se para solo y suelta la herramienta.
        WorkdayDirector.PerformForTest(g, "Huerto", "Riego", "hue_riego", dress: false);
        float stopBy = Time.realtimeSinceStartup + 3f;
        while (ActivityPerformer.Of(g) != null && Time.realtimeSinceStartup < stopBy) yield return null;
        TestProbe.Check("misiones: al dejar su puesto, la actuación se para sola y suelta la herramienta", ActivityPerformer.Of(g) == null && ActivityKit.Held(g, true) == ActivityKit.Prop.None);
    }

    static bool OwnSystem(string id) => id.StartsWith("yog_") && id != "yog_sala" && id != "yog_aseos";

    // ── 3. El uniforme ──────────────────────────────────────────────────────────────────────────────────────────
    IEnumerator Uniform(GameObject g)
    {
        UniformWearer.Want(g, Uniforms.Kind.None);
        yield return null;
        Transform body = g.transform.Find(CubeHumanoid.BodyName);
        CreatureRig rig = g.GetComponent<CreatureRig>();
        Renderer chest = rig != null && rig.Get(BodyPart.Chest) != null ? rig.Get(BodyPart.Chest).GetComponent<CubeJoint>()?.segment?.GetComponent<Renderer>() : null;
        Transform head = rig != null ? rig.Get(BodyPart.Head) : null;
        if (!TestProbe.Check("uniforme: el humano tiene torso y cabeza de cubos", chest != null && head != null)) yield break;
        Material chest0 = chest.sharedMaterial;
        var head0 = head.GetComponentsInChildren<Renderer>(true).ToDictionary(r => r, r => r.sharedMaterial);
        WorkdayDirector.PerformForTest(g, "Cocina", "Servicio del restaurante (cocinar la carta)", "coc_servicio", dress: true);
        yield return null;
        TestProbe.Check("uniforme: al llegar a la cocina se pone el de cocina", UniformWearer.Of(g) == Uniforms.Kind.Cocina, UniformWearer.Of(g).ToString());
        TestProbe.Check("uniforme: la chaquetilla es blanca (el torso cambió de color)", chest.sharedMaterial != chest0 && chest.sharedMaterial.color.grayscale > 0.85f, chest.sharedMaterial.color.ToString());
        TestProbe.Check("uniforme: lleva gorro y delantal (piezas «Uniforme_…»)", g.GetComponentsInChildren<Transform>().Count(t => t.name.StartsWith(CubeInsect.UniformPrefix) && t.gameObject.activeInHierarchy) >= 3);
        TestProbe.Check("uniforme: el pelo y la cara no cambian (su identidad)", head0.All(kv => kv.Key == null || kv.Key.sharedMaterial == kv.Value));
        ActivityPerformer.Stop(g);
        UniformWearer.Want(g, Uniforms.ForWork(null, null, null));   // libre: su ropa
        yield return null;
        TestProbe.Check("uniforme: al acabar vuelve a su ropa (el torso, a su material de siempre)", UniformWearer.Of(g) == Uniforms.Kind.None && chest.sharedMaterial == chest0);
        TestProbe.Check("uniforme: no queda ninguna pieza del uniforme", !g.GetComponentsInChildren<Transform>().Any(t => t.name.StartsWith(CubeInsect.UniformPrefix) && t.gameObject.activeInHierarchy));
        // El de la obra y el de nadar (neopreno, con prioridad mientras nada).
        UniformWearer.Want(g, Uniforms.Kind.Obra);
        bool helmet = g.GetComponentsInChildren<Transform>().Any(t => t.name == CubeInsect.UniformPrefix + "casco" && t.gameObject.activeInHierarchy);
        TestProbe.Check("uniforme: el de obra lleva casco y chaleco reflectante", helmet && g.GetComponentsInChildren<Transform>().Any(t => t.name == CubeInsect.UniformPrefix + "banda"));
        UniformWearer.Want(g, Uniforms.Kind.None);
        yield return null;
    }

    static void UniformTable()
    {
        var cases = new (string id, string area, string doing, Uniforms.Kind want)[]
        {
            ("coc_servicio", "Cocina", "Servicio", Uniforms.Kind.Cocina), ("coc_atender", "Cocina", "Barra", Uniforms.Kind.Sala),
            ("hos_quirofano", AreaWork.Hospital, "Quirófano", Uniforms.Kind.Quirofano), ("enf_visita", AreaWork.Hospital, "Visita", Uniforms.Kind.BataMedica),
            ("hos_veterinaria", AreaWork.Hospital, "Vet", Uniforms.Kind.Veterinaria), ("hue_riego", "Huerto", "Riego", Uniforms.Kind.Huerto),
            ("obra_trabajo_Muro", "Construcción", "Obra", Uniforms.Kind.Obra), ("lab_biorreactor", "Carne Cultivada", "Bio", Uniforms.Kind.Laboratorio),
            ("bio_pcr", "Biología", "PCR", Uniforms.Kind.Laboratorio), ("mec_vehiculos", "Mecánica", "Taller", Uniforms.Kind.Taller),
            (null, "Gimnasio", "Entrenamiento: Gimnasio", Uniforms.Kind.Gimnasio), (null, "Sala de Yoga", "Entrenamiento: Sala de Yoga", Uniforms.Kind.None),
            (null, "Cocina", "Comida", Uniforms.Kind.None), (null, null, null, Uniforms.Kind.None), ("yog_guia", "Sala de Yoga", "Clase", Uniforms.Kind.None),
        };
        var bad = cases.Where(c => Uniforms.ForWork(c.id, c.area, c.doing) != c.want).Select(c => $"{c.id ?? c.doing}: {Uniforms.ForWork(c.id, c.area, c.doing)} (pide {c.want})").ToList();
        TestProbe.Check("uniforme: cada tarea con el suyo (cocina, sala, quirófano, bata, veterinaria, huerto, obra, laboratorio, taller, gimnasio) y la comida, el yoga y lo libre con su ropa", bad.Count == 0, string.Join(" · ", bad));
    }

    // ── 4. El insecto-humano ────────────────────────────────────────────────────────────────────────────────────
    IEnumerator Insect()
    {
        GameObject h = new GameObject("Insecto_prueba_actividades");
        _temp.Add(h);
        Vector3 at = Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 6f : Vector3.up * 200f;
        h.transform.position = at;
        CubeInsect ci = CubeInsect.DressFolk(h, "Ant", 1.2f, "Prueba");
        yield return null;
        TestProbe.Check("insecto: tiene mano (la punta del brazo delantero)", ActivityKit.Hand(h, true) != null);
        float s = ActivityKit.Scale(h);
        GameObject table = Table(h.transform.position + h.transform.forward * 0.9f * s, s);
        var meal = new MealRitual(MicroJornada.EraDish(-2600));
        meal.TakeTray(h, true, 0.75f);
        yield return null;
        meal.SitDown(h, table.transform, false);
        TestProbe.Check("insecto: come con cuchara", ActivityKit.Held(h, true) == ActivityKit.Prop.Spoon, ActivityKit.Held(h, true).ToString());
        float until = Time.time + 4f, stop = Time.realtimeSinceStartup + 15f; bool done = false; float mid = -1f;
        while (!done && Time.realtimeSinceStartup < stop)
        {
            done = meal.Eat(h, until);
            if (mid < 0f && meal.bitesTaken >= MealRitual.Bites / 2) mid = meal.FoodScale;
            yield return null;
        }
        meal.PickUp(h); meal.Return(h);
        TestProbe.Check("insecto: la comida se hace entera y el plato mengua", done && mid >= 0f && mid < 0.7f && meal.Returned, $"{meal.bitesTaken} bocados, a mitad {mid:0.00}, pasos {string.Join(" → ", meal.steps)}");
        // Trabaja: la actuación de cocinar (cucharón en la mano, sus brazos, el vapor) y su delantal.
        ActivityPerformer.Performance cook = Performances.ForKind(AreaFunctions.Kind.Cook);
        float end = Time.realtimeSinceStartup + performWithin; ActivityKit.Prop held = ActivityKit.Prop.None; int fx = 0; bool martial = false;
        UniformWearer.Want(h, Uniforms.Kind.Cocina);
        while (Time.realtimeSinceStartup < end)
        {
            ActivityPerformer ap = ActivityPerformer.Keep(h, cook, table.transform.position);
            held = ActivityKit.Held(h, true); fx = ap.effects; martial |= ci.InMartial;
            yield return null;
        }
        TestProbe.Check("insecto: trabaja con la herramienta en la mano y su efecto", held == ActivityKit.Prop.Ladle && fx > 0, $"{held}, {fx} efectos");
        TestProbe.Check("insecto: se pone el delantal de su oficio", CubeInsect.HasWorkUniform(h) && UniformWearer.Of(h) == Uniforms.Kind.Cocina);
        UniformWearer.Want(h, Uniforms.Kind.None);
        yield return null;
        TestProbe.Check("insecto: y se lo quita", !CubeInsect.HasWorkUniform(h));
        ActivityPerformer.Stop(h);
        // Uno de verdad, si hay un microcosmos con jornada cargado: a la hora de comer recoge su bandeja (o le sirven en la mesa).
        MicroJornada mj = FindObjectsByType<MicroJornada>(FindObjectsSortMode.None).FirstOrDefault(m => m != null && m.isActiveAndEnabled && m.dine != null && m.follow == null);
        if (mj == null) { Log("no hay ningún microcosmos con jornada cargado: la prueba en vivo del comedor de la era se salta (se ve en el pase de los microcosmos)."); yield break; }
        // A las 11 (la comida): la hora la decide su reloj, así que se adelanta el reloj (adelantar nunca hace retroceder el día).
        if (TimeController.timeController != null && MicroJornada.BlockAt(TimeController.timeController.Hour) != "comida") TimeController.timeController.SetHour(WorkRoster.StartHour + WorkRoster.FirstBlock + 0.05f);
        mj.ChooseNow("comida");
        float limit = Time.realtimeSinceStartup + 75f;
        while (mj.MealStage < 3 && Time.realtimeSinceStartup < limit) yield return null;
        TestProbe.Check($"insecto en su microcosmos («{mj.name}»): a la hora de comer recoge la bandeja y se sienta a comer", mj.MealStage >= 3 && mj.Meal != null, $"paso {mj.MealStage}");
        mj.ChooseNow("trabajo");
    }
}
