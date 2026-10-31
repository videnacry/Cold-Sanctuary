using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TEST DEL MICROCOSMOS DE BRUCE LEE (docs/microcosmos-bruce-lee.md; Dojo). Tres partes, cada espera acotada:
/// (1) LAS POSTURAS (<see cref="JeetKuneDo"/>) caben en su cuerpo: en un Bruce Lee de cubos de 1,72 m, cada postura deja las
///     manos donde dice su IK (objetivos dentro del largo del brazo), los dos pies en el suelo, nada bajo el suelo, la cabeza
///     por encima de la cadera y ningún hueso en NaN; y cada forma tiene fotogramas.
/// (2) LA HISTORIA AVANZA: carga «Microcosmos_BruceLee» (Build Settings), mira que su <see cref="StoryMission"/> tenga sus pasos
///     con objetivos y que el maestro (<see cref="MartialForms"/>) tenga forma y sitio para cada paso; luego da por hechos los pasos
///     uno a uno (todos menos el último: así no se marca la historia como terminada en el progreso) y comprueba que el paso
///     sube y que Bruce Lee va a la estación de ese paso y hace su forma.
/// (3) LAS REGLAS DEL PISO: las estaciones de la historia separadas ≥ 2 m entre sí y dentro de los muros, y las POSICIONES
///     RESERVADAS del Dojo (PlatformSlots) copiadas en el piso gigante, con la historia en su plaza principal.
/// Se añade solo al orquestador cuando se piden las pruebas (como <see cref="MicrocosmosFloorTest"/>).
/// </summary>
public class BruceLeeStoryTest : MonoBehaviour, ITestUnit
{
    public int Group => 60;   // tras todo lo demás (los microcosmos, 43–44; la vida del santuario, 50–55): carga y descarga una escena
    public bool ParallelSafe => false;
    public const string Scene = "Microcosmos_BruceLee";
    public float loadTimeout = 60f;

    static readonly string[] Stations = { "Escuela de Yip Man", "Muñeco de madera", "Bahía de Hong Kong", "Jun Fan Gung Fu Institute", "Long Beach 1964", "Reposo de 1970" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<BruceLeeStoryTest>() == null) tr.gameObject.AddComponent<BruceLeeStoryTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Bruce Lee: posturas en su cuerpo, la historia avanza, 2 m y plazas reservadas");
        yield return Poses();
        yield return Story();
        TestProbe.End();
    }

    // ── (1) Las posturas en el cuerpo de 1,72 m ──────────────────────────────────────────────────────────────────────
    IEnumerator Poses()
    {
        GameObject g = new GameObject("Prueba Bruce Lee");
        g.transform.position = new Vector3(9300f, 500f, 9300f);   // lejos de todo, en el aire (el suelo es el del cuerpo)
        Anima a = g.AddComponent<Anima>();
        a.strength = 1.3f; a.agility = 1.6f; a.bodyMass = 1.0f; a.endurance = 1.4f;
        CubeHumanoid.Dress(g, Color.black, Color.white, 1.72f);
        yield return null; yield return null;   // BodyShape (Start) ajusta la estatura a sus stats
        CreatureRig rig = g.GetComponent<CreatureRig>();
        Transform hips = rig != null ? rig.Get(BodyPart.Hips) : null;
        Transform body = hips != null ? hips.parent : null;
        if (!TestProbe.Check("Bruce Lee tiene cuerpo de cubos humano (CreatureRig)", rig != null && body != null)) { Destroy(g); yield break; }
        TestProbe.Near("mide 1,72 m (delgado y atlético: las stats no cambian la estatura)", Height(g), 1.72f, 0.06f);

        foreach (string k in JeetKuneDo.FormKeys) TestProbe.Check($"forma «{k}» tiene fotogramas", JeetKuneDo.Form(k).Length > 0);

        foreach (AsanaPoseSpell p in JeetKuneDo.AllPoses())
        {
            p.ApplyFinal(a);
            yield return null;
            string why = "";
            Transform chest = rig.Get(BodyPart.Chest);
            foreach (var r in p.reaches)
            {
                Vector3 sh = r.end == BodyPart.HandLeft ? JeetKuneDo.ShoulderL : JeetKuneDo.ShoulderR;
                float need = Vector3.Distance(sh, r.localTarget);
                if (need > JeetKuneDo.ArmReach + 0.005f) why += $"{r.end} fuera del brazo ({need:0.00} > {JeetKuneDo.ArmReach}); ";
                Vector3 goal = chest.TransformPoint(r.localTarget - new Vector3(0f, 1.29f, 0f));
                Transform hand = rig.Get(r.end);
                float err = hand != null ? Vector3.Distance(hand.position, goal) : 99f;
                if (err > 0.04f) why += $"{r.end} a {err * 100f:0} cm de su objetivo; ";
            }
            float lowL = Lowest(body, rig.Get(BodyPart.FootLeft)), lowR = Lowest(body, rig.Get(BodyPart.FootRight)), lowAll = Lowest(body, hips);
            if (lowL > 0.1f || lowR > 0.1f) why += $"un pie en el aire (izq {lowL:0.00}, der {lowR:0.00}); ";
            if (lowAll < -0.02f) why += $"algo bajo el suelo ({lowAll:0.00}); ";
            Transform head = rig.Get(BodyPart.Head);
            if (head == null || head.position.y < hips.position.y + 0.35f) why += "la cabeza no está sobre la cadera; ";
            foreach (Transform t in hips.GetComponentsInChildren<Transform>())
                if (float.IsNaN(t.position.x) || float.IsNaN(t.rotation.x)) { why += $"NaN en {t.name}; "; break; }
            TestProbe.Check($"postura «{p.asanaName}» cabe en el cuerpo", why.Length == 0, why);
        }
        Destroy(g);
    }

    static float Height(GameObject g)
    {
        bool any = false; Bounds b = default;
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) { if (!r.enabled) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return any ? b.size.y : 0f;
    }

    // El punto más bajo de los segmentos que cuelgan de <paramref name="from"/>, en el marco del cuerpo (unidades nativas).
    static float Lowest(Transform body, Transform from)
    {
        float low = float.MaxValue;
        if (body == null || from == null) return low;
        foreach (CubeJoint j in from.GetComponentsInChildren<CubeJoint>())
        {
            Transform s = j.segment; if (s == null) continue;
            for (int c = 0; c < 8; c++)
            {
                Vector3 corner = new Vector3((c & 1) == 0 ? -0.5f : 0.5f, (c & 2) == 0 ? -0.5f : 0.5f, (c & 4) == 0 ? -0.5f : 0.5f);
                low = Mathf.Min(low, body.InverseTransformPoint(s.TransformPoint(corner)).y);
            }
        }
        return low;
    }

    // ── (2) y (3) La escena ──────────────────────────────────────────────────────────────────────────────────────────
    IEnumerator Story()
    {
        if (!TestProbe.Check($"«{Scene}» está en Build Settings", Application.CanStreamedLevelBeLoaded(Scene), "genera «Tools/Cold Sanctuary/Build Microcosmos de historias: Bruce Lee (Dojo)»")) yield break;
        Scene s = SceneManager.GetSceneByName(Scene);
        bool loadedHere = false;
        if (!s.isLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(Scene, LoadSceneMode.Additive);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (op != null && !op.isDone && Time.realtimeSinceStartup < until) yield return null;
            s = SceneManager.GetSceneByName(Scene);
            if (!TestProbe.Check($"«{Scene}» se carga", s.isLoaded, $"no cargó en {loadTimeout:0} s")) yield break;
            loadedHere = true;
        }
        yield return null; yield return null;   // Start de la misión y del maestro

        StoryMission m = null; MartialForms forms = null; MicroFloor floor = null; Transform root = null;
        foreach (GameObject g in s.GetRootGameObjects())
        {
            if (m == null) m = g.GetComponentInChildren<StoryMission>(true);
            if (forms == null) forms = g.GetComponentInChildren<MartialForms>(true);
            if (floor == null) { floor = g.GetComponentInChildren<MicroFloor>(true); if (floor != null) root = floor.transform; }
        }
        if (TestProbe.NotNull("la historia (StoryMission)", m) && TestProbe.NotNull("Bruce Lee enseña con el cuerpo (MartialForms)", forms))
        {
            TestProbe.Check("la historia es la de Bruce Lee", m.title.Contains("Bruce Lee"), m.title);
            TestProbe.Check("tiene entre 10 y 16 pasos (5 escenas)", m.steps.Length >= 10 && m.steps.Length <= 16, $"{m.steps.Length} pasos");
            TestProbe.Check("el maestro se llama «Bruce Lee» y es humano de cubos", forms.name == "Bruce Lee" && forms.GetComponent<CreatureRig>() != null && forms.GetComponent<Anima>() != null);
            TestProbe.Check("una forma y un sitio por paso", forms.formByStep.Length == m.steps.Length && forms.spotByStep.Length == m.steps.Length, $"{forms.formByStep.Length} formas, {forms.spotByStep.Length} sitios, {m.steps.Length} pasos");
            string bad = "";
            for (int i = 0; i < m.steps.Length; i++)
            {
                var st = m.steps[i];
                if (st.targets == null || st.targets.Length == 0 || System.Array.Exists(st.targets, x => x == null)) bad += $"paso {i + 1} sin objetivo; ";
                if (i < forms.formByStep.Length && JeetKuneDo.Form(forms.formByStep[i]).Length == 0) bad += $"paso {i + 1}: forma «{forms.formByStep[i]}» desconocida; ";
                if (i < forms.spotByStep.Length && forms.spotByStep[i] == null) bad += $"paso {i + 1} sin sitio; ";
            }
            TestProbe.Check("cada paso tiene objetivo, forma y sitio", bad.Length == 0, bad);

            // Avanzar: todos los pasos menos el último.
            int start = m.Step; string adv = "";
            for (int i = start; i < m.steps.Length - 1; i++)
            {
                if (!m.CompleteCurrentForTest()) { adv += $"no avanza en el paso {i + 1}; "; break; }
                yield return null; yield return null;
                if (m.Step != i + 1) { adv += $"paso {m.Step} en vez de {i + 1}; "; break; }
                Transform spot = forms.SpotOf(m.Step);
                float d = spot != null ? Vector2.Distance(new Vector2(spot.position.x, spot.position.z), new Vector2(forms.transform.position.x, forms.transform.position.z)) : 0f;
                if (d > forms.standOff + 0.6f) adv += $"paso {m.Step + 1}: Bruce a {d:0.0} m de su estación; ";
                if (forms.Current != forms.formByStep[m.Step]) adv += $"paso {m.Step + 1}: hace «{forms.Current}» y no «{forms.formByStep[m.Step]}»; ";
            }
            TestProbe.Check("los pasos avanzan y Bruce Lee va a cada estación con su forma", adv.Length == 0 && m.Step == m.steps.Length - 1, adv.Length > 0 ? adv : $"llega al paso {m.Step + 1}/{m.steps.Length}");
        }

        // (3) 2 m entre estaciones, dentro de los muros, y las plazas reservadas.
        if (TestProbe.NotNull("el piso del microcosmos (MicroFloor)", floor))
        {
            var boxes = new List<(string, Bounds)>();
            Transform story = root.Find("Historia");
            foreach (string n in Stations)
            {
                Transform st = Find(story, n);
                if (st == null) { TestProbe.Check($"estación «{n}»", false, "no está"); continue; }
                bool any = false; Bounds b = default;
                foreach (Renderer r in st.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
                if (any) boxes.Add((n, b));
            }
            Bounds marks = default; bool anyMark = false;
            if (story != null) foreach (Transform t in story.GetComponentsInChildren<Transform>()) if (t.name == "Marca de tiza") { Bounds b = t.GetComponent<Renderer>().bounds; if (!anyMark) { marks = b; anyMark = true; } else marks.Encapsulate(b); }
            if (anyMark) boxes.Add(("Juego de pies (Los Ángeles)", marks));
            string close = "", outside = "";
            for (int i = 0; i < boxes.Count; i++)
            {
                if (!floor.Inside(boxes[i].Item2, 0f)) outside += boxes[i].Item1 + "; ";
                for (int j = i + 1; j < boxes.Count; j++)
                {
                    float gap = GapXZ(boxes[i].Item2, boxes[j].Item2);
                    if (gap < FurnitureSpacing.Standard) close += $"{boxes[i].Item1} ↔ {boxes[j].Item1}: {gap:0.0} m; ";
                }
            }
            TestProbe.Check("las estaciones de la historia, a ≥ 2 m unas de otras", close.Length == 0, close.Length > 0 ? close : $"{boxes.Count} estaciones");
            TestProbe.Check("las estaciones, dentro de los muros del piso", outside.Length == 0, outside);

            Transform giant = root.Find("Bruce Lee_gigante");
            PlatformSlots slots = giant != null ? giant.GetComponent<PlatformSlots>() : null;
            if (TestProbe.NotNull("el Dojo gigante trae sus posiciones reservadas (PlatformSlots)", slots))
            {
                TestProbe.Check("plazas reservadas suficientes (historia, personajes, fauna, flora)",
                    slots.story.Count >= PlatformSlots.MinStory && slots.characters.Count >= PlatformSlots.MinCharacters && slots.fauna.Count >= PlatformSlots.MinFauna && slots.flora.Count >= PlatformSlots.MinFlora, slots.Summary());
                if (slots.story.Count > 0 && story != null)
                {
                    Vector3 want = slots.story[0];   // la copia del piso gigante ya está en sus locales (×10 por la escala del piso)
                    Vector3 wantWorld = giant.TransformPoint(want);
                    float d = Vector2.Distance(new Vector2(wantWorld.x, wantWorld.z), new Vector2(story.position.x, story.position.z));
                    TestProbe.Check("la historia está en su plaza reservada principal", d <= 1.5f, $"a {d:0.0} m de la plaza");
                }
            }
            Transform acab = giant != null ? Find(giant, "Acabado") : null;
            TestProbe.Check("el Dojo del microcosmos está terminado (su acabado a la vista, sin andamio)", acab != null && acab.gameObject.activeInHierarchy && Find(giant, "Andamio_obra") == null);
        }

        if (loadedHere)
        {
            AsyncOperation un = SceneManager.UnloadSceneAsync(s);
            float until = Time.realtimeSinceStartup + loadTimeout;
            while (un != null && !un.isDone && Time.realtimeSinceStartup < until) yield return null;
        }
    }


    static float GapXZ(Bounds a, Bounds b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x));
        float dz = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    static Transform Find(Transform root, string name)
    {
        if (root == null) return null;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }
}
