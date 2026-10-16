using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FAUNA DE CUBOS CON HUESOS (docs/anyma-factory-and-control §1): todo animal —también los del agua— se construye como
/// un esqueleto de ARTICULACIONES (<see cref="CubeJoint"/>) con un cubo por segmento, a TAMAÑO REAL y mirando a +Z:
///   · Cuadrúpedo: cadera → lomo/pecho → cuello → cabeza; patas delanteras hombro→antebrazo→mano y traseras
///     muslo→espinilla→pie; cola en tres tramos; orejas articuladas.
///   · Cetáceo (ballena, orca): cabeza → tórax → vientre → cola → pedúnculo → aleta caudal; aletas pectorales y dorsal.
///   · Pinnípedo (foca): cuerpo fusiforme en tramos, aletas delanteras articuladas y traseras hacia atrás.
///   · Ave (pingüino): erguida, aletas-ala, patas cortas, pico como hocico con mandíbula.
///   · Insecto: tórax, abdomen, cabeza, 6 patas de dos tramos, antenas (y alas en la abeja).
/// La CABEZA se arma con cubos (<see cref="CubeFace"/>): ojos, párpados, cejas, hocico y mandíbula → expresiones.
/// Cada hueso se registra en el <see cref="CreatureRig"/> con su <see cref="BodyPart"/>, así que posturas, emociones y
/// la marcha (<see cref="CubeGait"/>) mueven el cuerpo igual que en los humanos.
/// </summary>
public static class CubeCreature
{
    public enum Plan { Quadruped, Cetacean, Pinniped, Bird, Insect }

    public class Spec
    {
        public Plan plan = Plan.Quadruped;
        public float length = 1f, height = 0.7f, width = 0.3f;   // metros (alto = a la cruz/lomo; en cetáceos, grosor)
        public Vector3 head = new Vector3(0.25f, 0.24f, 0.28f);
        public float snout = 0.12f, neck = 0.2f;
        public Vector2 ear = new Vector2(0.07f, 0.1f);           // ancho, alto (0 = sin orejas visibles)
        public float tail = 0.4f, tailThick = 0.08f, legThick = 0f;
        public float bulk = 0.42f;                                // grosor del tronco respecto a la altura
        public Color fur = Color.white, belly = Color.white, eye = new Color(0.15f, 0.12f, 0.1f), accent = Color.gray;
        public bool antlers, wings, tallDorsal;
    }

    static Color C(float r, float g, float b) => new Color(r, g, b);

    /// <summary>Medidas reales y colores por especie (fauna del hielo y del microcosmos).</summary>
    public static Spec SpecFor(string species)
    {
        string s = (species ?? "").ToLowerInvariant();
        if (s.Contains("whale")) return new Spec { plan = Plan.Cetacean, length = 12f, height = 2.6f, width = 2.4f, head = new Vector3(2.2f, 2.2f, 3f), snout = 0f, fur = C(0.48f, 0.58f, 0.68f), belly = C(0.86f, 0.9f, 0.92f), tail = 0f };
        if (s.Contains("orca")) return new Spec { plan = Plan.Cetacean, length = 7f, height = 1.6f, width = 1.4f, head = new Vector3(1.3f, 1.3f, 1.6f), snout = 0f, fur = C(0.16f, 0.17f, 0.2f), belly = C(0.96f, 0.96f, 0.96f), tallDorsal = true };
        if (s.Contains("seal")) return new Spec { plan = Plan.Pinniped, length = 1.7f, height = 0.45f, width = 0.5f, head = new Vector3(0.26f, 0.24f, 0.28f), snout = 0.1f, ear = Vector2.zero, fur = C(0.72f, 0.75f, 0.8f), belly = C(0.86f, 0.88f, 0.9f) };
        if (s.Contains("penguin")) return new Spec { plan = Plan.Bird, length = 0.38f, height = 1.0f, width = 0.42f, head = new Vector3(0.2f, 0.2f, 0.22f), snout = 0.12f, ear = Vector2.zero, fur = C(0.15f, 0.16f, 0.2f), belly = C(0.97f, 0.97f, 0.95f), accent = C(0.98f, 0.8f, 0.35f) };
        if (s.Contains("polarbear") || s == "bear") return new Spec { length = 2.2f, height = 1.3f, width = 0.75f, head = new Vector3(0.38f, 0.36f, 0.46f), snout = 0.22f, ear = new Vector2(0.1f, 0.08f), tail = 0.12f, tailThick = 0.1f, bulk = 0.55f, fur = C(0.95f, 0.93f, 0.86f), belly = C(0.92f, 0.9f, 0.82f) };
        if (s.Contains("wolf")) return new Spec { length = 1.2f, height = 0.8f, width = 0.32f, head = new Vector3(0.26f, 0.25f, 0.3f), snout = 0.17f, ear = new Vector2(0.08f, 0.13f), tail = 0.45f, tailThick = 0.1f, fur = C(0.84f, 0.85f, 0.88f), belly = C(0.96f, 0.96f, 0.97f), eye = C(0.85f, 0.65f, 0.2f) };
        if (s.Contains("malamute") || s.Contains("husky")) return new Spec { length = 1.0f, height = 0.63f, width = 0.32f, head = new Vector3(0.25f, 0.24f, 0.28f), snout = 0.13f, ear = new Vector2(0.08f, 0.1f), tail = 0.38f, tailThick = 0.11f, fur = C(0.62f, 0.64f, 0.7f), belly = C(0.97f, 0.97f, 0.97f), eye = C(0.45f, 0.65f, 0.9f) };
        if (s.Contains("fox")) return new Spec { length = 0.65f, height = 0.32f, width = 0.2f, head = new Vector3(0.16f, 0.15f, 0.17f), snout = 0.1f, ear = new Vector2(0.06f, 0.09f), tail = 0.36f, tailThick = 0.1f, fur = C(0.97f, 0.97f, 0.98f), belly = C(0.99f, 0.99f, 0.99f) };
        if (s.Contains("bunny") || s.Contains("hare")) return new Spec { length = 0.5f, height = 0.3f, width = 0.2f, head = new Vector3(0.14f, 0.14f, 0.16f), snout = 0.04f, ear = new Vector2(0.05f, 0.2f), tail = 0.06f, tailThick = 0.07f, bulk = 0.6f, fur = C(0.97f, 0.97f, 0.98f), belly = C(0.99f, 0.99f, 0.99f) };
        if (s.Contains("deer")) return new Spec { length = 1.8f, height = 1.2f, width = 0.42f, head = new Vector3(0.24f, 0.26f, 0.36f), snout = 0.18f, neck = 0.45f, ear = new Vector2(0.07f, 0.14f), tail = 0.12f, tailThick = 0.08f, bulk = 0.38f, fur = C(0.78f, 0.7f, 0.62f), belly = C(0.95f, 0.93f, 0.9f), antlers = true };
        if (s.Contains("meerkat")) return new Spec { length = 0.3f, height = 0.15f, width = 0.1f, head = new Vector3(0.07f, 0.07f, 0.08f), snout = 0.04f, ear = new Vector2(0.02f, 0.02f), tail = 0.22f, tailThick = 0.03f, fur = C(0.86f, 0.78f, 0.64f), belly = C(0.93f, 0.88f, 0.78f) };
        if (s.Contains("bee")) return new Spec { plan = Plan.Insect, length = 0.5f, height = 0.22f, width = 0.22f, head = new Vector3(0.15f, 0.14f, 0.13f), fur = C(0.98f, 0.92f, 0.6f), belly = C(0.95f, 0.95f, 0.9f), accent = C(0.3f, 0.28f, 0.3f), wings = true };
        if (PlanForInsect(s)) return new Spec { plan = Plan.Insect, length = 0.5f, height = 0.2f, width = 0.2f, head = new Vector3(0.14f, 0.12f, 0.12f), fur = InsectColor(s), belly = InsectColor(s) * 0.85f + new Color(0, 0, 0, 1), accent = C(0.2f, 0.2f, 0.22f), wings = s.Contains("ladybug") || s.Contains("beetle") };
        return new Spec();
    }

    static bool PlanForInsect(string s) =>
        s.Contains("ant") || s.Contains("aphid") || s.Contains("ladybug") || s.Contains("spider") || s.Contains("cricket") ||
        s.Contains("termite") || s.Contains("beetle") || s.Contains("mantis") || s.Contains("grasshopper");

    static Color InsectColor(string s) =>
        s.Contains("ladybug") ? C(0.92f, 0.35f, 0.3f) : s.Contains("aphid") ? C(0.65f, 0.85f, 0.5f) : s.Contains("mantis") || s.Contains("grasshopper") || s.Contains("cricket") ? C(0.55f, 0.75f, 0.45f)
        : s.Contains("spider") ? C(0.3f, 0.28f, 0.3f) : s.Contains("termite") ? C(0.9f, 0.85f, 0.75f) : C(0.35f, 0.25f, 0.2f);

    public static bool IsInsect(string species) => SpecFor(species).plan == Plan.Insect;

    // ── Materiales compartidos por color ────────────────────────────────────────────────────────
    static readonly Dictionary<Color, Material> _mats = new Dictionary<Color, Material>();
    public static Material Mat(Color c)
    {
        if (_mats.TryGetValue(c, out Material m) && m != null) return m;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        m = new Material(sh) { color = c, name = $"Cubo_{ColorUtility.ToHtmlStringRGB(c)}" };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
#if UNITY_EDITOR
        // En el editor (prefabs de animales) el material tiene que ser un ASSET: si no, el prefab guardado pierde el color.
        if (!Application.isPlaying)
        {
            const string dir = "Assets/Generated/CubeMats";
            string path = $"{dir}/{m.name}.mat";
            Material existing = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) m = existing;
            else
            {
                if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Generated")) UnityEditor.AssetDatabase.CreateFolder("Assets", "Generated");
                if (!UnityEditor.AssetDatabase.IsValidFolder(dir)) UnityEditor.AssetDatabase.CreateFolder("Assets/Generated", "CubeMats");
                UnityEditor.AssetDatabase.CreateAsset(m, path);
            }
        }
#endif
        return _mats[c] = m;
    }

    public const string BodyName = "CuerpoCubos";

    /// <summary>Construye el cuerpo de <paramref name="species"/> bajo <paramref name="root"/> (oculta mallas previas).</summary>
    public static CreatureRig Build(GameObject root, string species)
    {
        Transform old = root.transform.Find(BodyName);
        if (old != null) Object.DestroyImmediate(old.gameObject);
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>()) r.enabled = false;

        Spec sp = SpecFor(species);
        Transform body = new GameObject(BodyName).transform;
        body.SetParent(root.transform, false);
        if (!root.TryGetComponent(out CreatureRig rig)) rig = root.AddComponent<CreatureRig>();
        rig.humanoidAnimator = null;
        rig.bindings.Clear();
        switch (sp.plan)
        {
            case Plan.Cetacean: Cetacean(body, sp, rig); break;
            case Plan.Pinniped: Pinniped(body, sp, rig); break;
            case Plan.Bird: Bird(body, sp, rig); break;
            case Plan.Insect: Insect(body, sp, rig); break;
            default: Quadruped(body, sp, rig); break;
        }
        foreach (CubeJoint j in body.GetComponentsInChildren<CubeJoint>()) j.Capture();
        rig.Rebuild();
        if (!root.TryGetComponent(out CubeGait gait)) gait = root.AddComponent<CubeGait>();
        gait.plan = sp.plan;
        return rig;
    }

    // ── Planes ──────────────────────────────────────────────────────────────────────────────────

    static void Quadruped(Transform body, Spec sp, CreatureRig rig)
    {
        float L = sp.length, H = sp.height, W = sp.width;
        float thick = H * sp.bulk, legLen = H - thick * 0.5f, legW = sp.legThick > 0f ? sp.legThick : Mathf.Max(0.03f, W * 0.3f);
        CubeJoint hips = J(body, BodyPart.Hips, new Vector3(0f, legLen, -L * 0.22f), new Vector3(W, thick, L * 0.42f), new Vector3(0f, 0f, L * 0.06f), sp.fur, rig);
        CubeJoint chest = J(hips.transform, BodyPart.Chest, new Vector3(0f, 0f, L * 0.3f), new Vector3(W * 1.06f, thick * 1.06f, L * 0.4f), new Vector3(0f, 0.02f * H, L * 0.1f), sp.fur, rig);
        J(chest.transform, BodyPart.Back, new Vector3(0f, -thick * 0.42f, 0f), new Vector3(W * 0.9f, thick * 0.2f, L * 0.55f), new Vector3(0f, 0f, -L * 0.12f), sp.belly, rig);   // vientre

        // Cuello inclinado hacia arriba y delante; la cabeza se endereza.
        float neckLen = Mathf.Max(sp.neck, sp.head.z * 0.6f);
        CubeJoint neck = J(chest.transform, BodyPart.Neck, new Vector3(0f, thick * 0.25f, L * 0.27f), new Vector3(W * 0.5f, W * 0.5f, neckLen), new Vector3(0f, 0f, neckLen * 0.5f), sp.fur, rig);
        neck.transform.localRotation = Quaternion.Euler(-40f, 0f, 0f);
        Head(neck.transform, new Vector3(0f, 0f, neckLen), Quaternion.Euler(40f, 0f, 0f), sp, rig);

        // Cola en tres tramos (hacia atrás y algo caída).
        Tail(hips.transform, new Vector3(0f, thick * 0.25f, -L * 0.15f), sp, rig);

        // Patas: delanteras desde el pecho, traseras desde la cadera.
        for (int s = -1; s <= 1; s += 2)
        {
            bool left = s < 0;
            float fl = legLen - thick * 0.3f;
            Leg(chest.transform, new Vector3(W * 0.36f * s, -thick * 0.3f, L * 0.2f), fl, legW, sp.fur,
                left ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight, left ? BodyPart.ForearmLeft : BodyPart.ForearmRight, left ? BodyPart.HandLeft : BodyPart.HandRight, rig, false);
            Leg(hips.transform, new Vector3(W * 0.36f * s, -thick * 0.3f, -L * 0.04f), fl, legW * 1.1f, sp.fur,
                left ? BodyPart.ThighLeft : BodyPart.ThighRight, left ? BodyPart.ShinLeft : BodyPart.ShinRight, left ? BodyPart.FootLeft : BodyPart.FootRight, rig, true);
        }
    }

    static void Leg(Transform parent, Vector3 at, float len, float w, Color c, BodyPart a, BodyPart b, BodyPart foot, CreatureRig rig, bool hind)
    {
        float up = len * 0.46f, low = len * 0.44f, paw = len * 0.1f;
        CubeJoint j1 = J(parent, a, at, new Vector3(w, up, w * 1.1f), new Vector3(0f, -up * 0.5f, 0f), c, rig);
        // Las traseras tienen el «tobillo alto» (corvejón): el muslo va algo adelante y la espinilla atrás.
        if (hind) j1.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
        CubeJoint j2 = J(j1.transform, b, new Vector3(0f, -up, 0f), new Vector3(w * 0.85f, low, w * 0.85f), new Vector3(0f, -low * 0.5f, 0f), c, rig);
        if (hind) j2.transform.localRotation = Quaternion.Euler(24f, 0f, 0f);
        CubeJoint j3 = J(j2.transform, foot, new Vector3(0f, -low, 0f), new Vector3(w * 1.1f, paw, w * 1.5f), new Vector3(0f, -paw * 0.5f, w * 0.25f), c * 0.92f + new Color(0, 0, 0, 1), rig);
        if (hind) j3.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
    }

    static void Tail(Transform parent, Vector3 at, Spec sp, CreatureRig rig)
    {
        if (sp.tail <= 0.001f) return;
        float seg = sp.tail / 3f;
        CubeJoint t1 = J(parent, BodyPart.Tail, at, new Vector3(sp.tailThick, sp.tailThick, seg), new Vector3(0f, 0f, -seg * 0.5f), sp.fur, rig);
        t1.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
        CubeJoint t2 = J(t1.transform, BodyPart.Tail2, new Vector3(0f, 0f, -seg), new Vector3(sp.tailThick * 1.1f, sp.tailThick * 1.1f, seg), new Vector3(0f, 0f, -seg * 0.5f), sp.fur, rig);
        t2.transform.localRotation = Quaternion.Euler(-10f, 0f, 0f);
        J(t2.transform, BodyPart.Tail3, new Vector3(0f, 0f, -seg), new Vector3(sp.tailThick, sp.tailThick, seg), new Vector3(0f, 0f, -seg * 0.5f), sp.belly, rig);
    }

    static void Head(Transform parent, Vector3 at, Quaternion rot, Spec sp, CreatureRig rig)
    {
        Vector3 hs = sp.head;
        CubeJoint head = J(parent, BodyPart.Head, at, hs, new Vector3(0f, 0f, hs.z * 0.3f), sp.fur, rig);
        head.transform.localRotation = rot;
        Transform earL = null, earR = null;
        if (sp.ear.y > 0.001f)
            for (int s = -1; s <= 1; s += 2)
            {
                CubeJoint e = J(head.transform, s < 0 ? BodyPart.EarLeft : BodyPart.EarRight, new Vector3(hs.x * 0.3f * s, hs.y * 0.48f, hs.z * 0.15f),
                                new Vector3(sp.ear.x, sp.ear.y, sp.ear.x * 0.45f), new Vector3(0f, sp.ear.y * 0.5f, 0f), sp.fur, rig);
                e.transform.localRotation = Quaternion.Euler(-8f, 0f, 12f * s);
                if (s < 0) earL = e.transform; else earR = e.transform;
            }
        if (sp.antlers)   // cornamenta de reno: ramas de cubos
            for (int s = -1; s <= 1; s += 2)
            {
                Transform a = new GameObject("Cornamenta").transform; a.SetParent(head.transform, false);
                a.localPosition = new Vector3(hs.x * 0.25f * s, hs.y * 0.5f, 0f);
                a.localRotation = Quaternion.Euler(-20f, 0f, -25f * s);
                Seg(a, new Vector3(0f, 0.25f, 0f), new Vector3(0.04f, 0.5f, 0.04f), C(0.85f, 0.8f, 0.7f));
                Seg(a, new Vector3(0f, 0.3f, 0.1f), new Vector3(0.03f, 0.03f, 0.22f), C(0.85f, 0.8f, 0.7f));
                Seg(a, new Vector3(0.05f * s, 0.5f, -0.05f), new Vector3(0.12f, 0.03f, 0.03f), C(0.85f, 0.8f, 0.7f));
            }
        CubeFace.Build(head.transform, new Vector3(0f, 0f, hs.z * 0.3f), hs, sp.snout, sp.fur, sp.eye, rig, earL, earR);
    }

    static void Cetacean(Transform body, Spec sp, CreatureRig rig)
    {
        float L = sp.length, H = sp.height, W = sp.width;
        CubeJoint chest = J(body, BodyPart.Chest, new Vector3(0f, H * 0.5f, 0f), new Vector3(W, H, L * 0.3f), Vector3.zero, sp.fur, rig);
        Seg(chest.transform, new Vector3(0f, -H * 0.38f, 0f), new Vector3(W * 0.85f, H * 0.26f, L * 0.31f), sp.belly);   // vientre claro
        // Cabeza hacia delante (la cara de cubos: ojos a los lados, mandíbula larga).
        CubeJoint head = J(chest.transform, BodyPart.Head, new Vector3(0f, 0f, L * 0.15f), sp.head, new Vector3(0f, 0f, sp.head.z * 0.5f), sp.fur, rig);
        CubeFace.Build(head.transform, new Vector3(0f, 0f, sp.head.z * 0.5f), sp.head, 0f, sp.fur, sp.eye, rig);
        if (sp.tallDorsal)   // mancha blanca tras el ojo de la orca
            for (int s = -1; s <= 1; s += 2) Seg(head.transform, new Vector3(sp.head.x * 0.5f * s, sp.head.y * 0.15f, sp.head.z * 0.15f), new Vector3(0.02f, sp.head.y * 0.25f, sp.head.z * 0.35f), sp.belly);
        // Tronco → cola → pedúnculo → aleta caudal (ondula en vertical al nadar).
        CubeJoint back = J(chest.transform, BodyPart.Back, new Vector3(0f, 0f, -L * 0.15f), new Vector3(W * 0.88f, H * 0.85f, L * 0.26f), new Vector3(0f, 0f, -L * 0.13f), sp.fur, rig);
        CubeJoint tail = J(back.transform, BodyPart.Tail, new Vector3(0f, 0f, -L * 0.26f), new Vector3(W * 0.55f, H * 0.55f, L * 0.2f), new Vector3(0f, 0f, -L * 0.1f), sp.fur, rig);
        CubeJoint tail2 = J(tail.transform, BodyPart.Tail2, new Vector3(0f, 0f, -L * 0.2f), new Vector3(W * 0.28f, H * 0.3f, L * 0.13f), new Vector3(0f, 0f, -L * 0.065f), sp.fur, rig);
        CubeJoint fluke = J(tail2.transform, BodyPart.Fluke, new Vector3(0f, 0f, -L * 0.13f), new Vector3(L * 0.28f, H * 0.06f, L * 0.08f), new Vector3(0f, 0f, -L * 0.04f), sp.fur, rig);
        Seg(fluke.transform, new Vector3(0f, -H * 0.035f, -L * 0.04f), new Vector3(L * 0.27f, H * 0.02f, L * 0.07f), sp.belly);
        // Aleta dorsal y pectorales.
        float dh = sp.tallDorsal ? H * 0.9f : H * 0.15f;
        J(back.transform, BodyPart.DorsalFin, new Vector3(0f, H * 0.42f, -L * 0.08f), new Vector3(W * 0.08f, dh, L * 0.07f), new Vector3(0f, dh * 0.5f, -L * 0.02f), sp.fur, rig);
        for (int s = -1; s <= 1; s += 2)
        {
            float fin = sp.tallDorsal ? L * 0.12f : L * 0.3f;   // la ballena jorobada tiene aletas larguísimas
            CubeJoint f = J(chest.transform, s < 0 ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight, new Vector3(W * 0.5f * s, -H * 0.25f, L * 0.06f),
                            new Vector3(fin, H * 0.05f, L * 0.06f), new Vector3(fin * 0.5f * s, 0f, 0f), sp.fur, rig);
            f.transform.localRotation = Quaternion.Euler(0f, -20f * s, -30f * s);
        }
    }

    static void Pinniped(Transform body, Spec sp, CreatureRig rig)
    {
        float L = sp.length, H = sp.height, W = sp.width;
        CubeJoint chest = J(body, BodyPart.Chest, new Vector3(0f, H * 0.5f, L * 0.05f), new Vector3(W, H, L * 0.35f), Vector3.zero, sp.fur, rig);
        CubeJoint back = J(chest.transform, BodyPart.Back, new Vector3(0f, -H * 0.05f, -L * 0.17f), new Vector3(W * 0.85f, H * 0.82f, L * 0.3f), new Vector3(0f, 0f, -L * 0.14f), sp.fur, rig);
        CubeJoint tail = J(back.transform, BodyPart.Tail, new Vector3(0f, -H * 0.05f, -L * 0.28f), new Vector3(W * 0.5f, H * 0.5f, L * 0.16f), new Vector3(0f, 0f, -L * 0.08f), sp.fur, rig);
        Seg(chest.transform, new Vector3(0f, -H * 0.4f, 0f), new Vector3(W * 0.8f, H * 0.2f, L * 0.34f), sp.belly);
        for (int i = 0; i < 6; i++)   // manchas
            Seg(i < 3 ? chest.transform : back.transform, new Vector3((i % 2 == 0 ? 0.2f : -0.25f) * W, H * 0.48f, ((i % 3) - 1) * L * 0.08f), new Vector3(W * 0.12f, 0.01f, W * 0.1f), sp.fur * 0.75f + new Color(0, 0, 0, 1));
        CubeJoint neck = J(chest.transform, BodyPart.Neck, new Vector3(0f, H * 0.1f, L * 0.17f), new Vector3(W * 0.6f, H * 0.6f, sp.head.z * 0.6f), new Vector3(0f, 0f, sp.head.z * 0.3f), sp.fur, rig);
        neck.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
        Head(neck.transform, new Vector3(0f, 0f, sp.head.z * 0.6f), Quaternion.Euler(25f, 0f, 0f), sp, rig);
        for (int s = -1; s <= 1; s += 2)
        {
            bool left = s < 0;
            // Aletas delanteras: hombro → antebrazo → mano, apoyadas en el suelo.
            CubeJoint sh = J(chest.transform, left ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight, new Vector3(W * 0.45f * s, -H * 0.2f, L * 0.08f),
                             new Vector3(0.06f, H * 0.3f, 0.08f), new Vector3(0f, -H * 0.15f, 0f), sp.fur, rig);
            sh.transform.localRotation = Quaternion.Euler(-30f, 0f, 25f * s);
            CubeJoint fa = J(sh.transform, left ? BodyPart.ForearmLeft : BodyPart.ForearmRight, new Vector3(0f, -H * 0.3f, 0f), new Vector3(0.05f, H * 0.18f, 0.12f), new Vector3(0f, -H * 0.09f, 0.03f), sp.fur, rig);
            J(fa.transform, left ? BodyPart.HandLeft : BodyPart.HandRight, new Vector3(0f, -H * 0.18f, 0f), new Vector3(0.04f, 0.03f, 0.16f), new Vector3(0f, 0f, 0.06f), sp.fur * 0.85f + new Color(0, 0, 0, 1), rig);
            // Aletas traseras hacia atrás, al final de la cola.
            CubeJoint th = J(tail.transform, left ? BodyPart.ThighLeft : BodyPart.ThighRight, new Vector3(W * 0.12f * s, 0f, -L * 0.16f), new Vector3(0.08f, 0.04f, L * 0.1f), new Vector3(0f, 0f, -L * 0.05f), sp.fur, rig);
            th.transform.localRotation = Quaternion.Euler(0f, 15f * s, 0f);
            J(th.transform, left ? BodyPart.FootLeft : BodyPart.FootRight, new Vector3(0f, 0f, -L * 0.1f), new Vector3(0.14f, 0.02f, L * 0.08f), new Vector3(0f, 0f, -L * 0.04f), sp.fur * 0.85f + new Color(0, 0, 0, 1), rig);
        }
    }

    static void Bird(Transform body, Spec sp, CreatureRig rig)
    {
        float H = sp.height, W = sp.width, D = sp.length;
        float leg = H * 0.1f, torso = H * 0.6f;
        CubeJoint hips = J(body, BodyPart.Hips, new Vector3(0f, leg, 0f), new Vector3(W, H * 0.18f, D), new Vector3(0f, H * 0.09f, 0f), sp.fur, rig);
        CubeJoint chest = J(hips.transform, BodyPart.Chest, new Vector3(0f, H * 0.18f, 0f), new Vector3(W * 0.92f, torso, D * 0.95f), new Vector3(0f, torso * 0.5f, 0f), sp.fur, rig);
        Seg(chest.transform, new Vector3(0f, torso * 0.45f, D * 0.42f), new Vector3(W * 0.75f, torso * 0.95f, D * 0.2f), sp.belly);   // pechera blanca
        Seg(hips.transform, new Vector3(0f, H * 0.09f, D * 0.42f), new Vector3(W * 0.8f, H * 0.18f, D * 0.2f), sp.belly);
        CubeJoint neck = J(chest.transform, BodyPart.Neck, new Vector3(0f, torso, 0f), new Vector3(W * 0.55f, H * 0.08f, D * 0.6f), new Vector3(0f, H * 0.04f, 0f), sp.fur, rig);
        Seg(neck.transform, new Vector3(0f, H * 0.04f, D * 0.25f), new Vector3(W * 0.5f, H * 0.07f, D * 0.15f), sp.accent);   // mancha amarilla
        Spec headSpec = new Spec { head = sp.head, snout = sp.snout, ear = Vector2.zero, fur = sp.fur, belly = sp.belly, eye = sp.eye };
        Head(neck.transform, new Vector3(0f, H * 0.08f + sp.head.y * 0.5f, 0f), Quaternion.identity, headSpec, rig);
        for (int s = -1; s <= 1; s += 2)
        {
            bool left = s < 0;
            // Aletas-ala: hombro → antebrazo (cuelgan pegadas al costado).
            CubeJoint sh = J(chest.transform, left ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight, new Vector3(W * 0.5f * s, torso * 0.85f, 0f),
                             new Vector3(0.04f, torso * 0.45f, D * 0.45f), new Vector3(0f, -torso * 0.22f, 0f), sp.fur, rig);
            sh.transform.localRotation = Quaternion.Euler(0f, 0f, 8f * s);
            J(sh.transform, left ? BodyPart.ForearmLeft : BodyPart.ForearmRight, new Vector3(0f, -torso * 0.45f, 0f), new Vector3(0.03f, torso * 0.3f, D * 0.32f), new Vector3(0f, -torso * 0.15f, 0f), sp.fur, rig);
            // Patas cortas con pie palmeado.
            CubeJoint th = J(hips.transform, left ? BodyPart.ThighLeft : BodyPart.ThighRight, new Vector3(W * 0.2f * s, 0f, 0f), new Vector3(0.07f, leg * 0.6f, 0.07f), new Vector3(0f, -leg * 0.3f, 0f), sp.fur, rig);
            CubeJoint sn = J(th.transform, left ? BodyPart.ShinLeft : BodyPart.ShinRight, new Vector3(0f, -leg * 0.6f, 0f), new Vector3(0.05f, leg * 0.35f, 0.05f), new Vector3(0f, -leg * 0.17f, 0f), C(0.3f, 0.3f, 0.32f), rig);
            J(sn.transform, left ? BodyPart.FootLeft : BodyPart.FootRight, new Vector3(0f, -leg * 0.38f, 0f), new Vector3(0.1f, 0.025f, 0.14f), new Vector3(0f, -0.012f, 0.05f), C(0.35f, 0.35f, 0.38f), rig);
        }
        CubeJoint tail = J(hips.transform, BodyPart.Tail, new Vector3(0f, H * 0.04f, -D * 0.45f), new Vector3(W * 0.4f, 0.03f, D * 0.3f), new Vector3(0f, 0f, -D * 0.12f), sp.fur, rig);
        tail.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
    }

    static void Insect(Transform body, Spec sp, CreatureRig rig)
    {
        float L = sp.length, H = sp.height, W = sp.width;
        float legY = H * 0.6f;
        CubeJoint thorax = J(body, BodyPart.Chest, new Vector3(0f, legY, 0f), new Vector3(W * 0.8f, H * 0.6f, L * 0.3f), Vector3.zero, sp.fur, rig);
        CubeJoint abd = J(thorax.transform, BodyPart.Tail, new Vector3(0f, 0f, -L * 0.15f), new Vector3(W, H * 0.7f, L * 0.45f), new Vector3(0f, 0f, -L * 0.22f), sp.belly, rig);
        if (sp.wings || sp.accent != Color.gray)
            for (int i = 0; i < 3; i++)   // franjas del abdomen (abeja) o manchas
                Seg(abd.transform, new Vector3(0f, H * 0.36f, -L * (0.1f + i * 0.12f)), new Vector3(W * 1.02f, H * 0.05f, L * 0.05f), sp.accent);
        CubeJoint head = J(thorax.transform, BodyPart.Head, new Vector3(0f, H * 0.05f, L * 0.15f), sp.head, new Vector3(0f, 0f, sp.head.z * 0.5f), sp.fur, rig);
        CubeFace.Build(head.transform, new Vector3(0f, 0f, sp.head.z * 0.5f), sp.head, 0f, sp.fur, C(0.1f, 0.1f, 0.12f), rig);
        for (int s = -1; s <= 1; s += 2)
        {
            CubeJoint ant = J(head.transform, s < 0 ? BodyPart.AntennaLeft : BodyPart.AntennaRight, new Vector3(sp.head.x * 0.25f * s, sp.head.y * 0.5f, sp.head.z * 0.7f),
                              new Vector3(0.012f, L * 0.22f, 0.012f), new Vector3(0f, L * 0.11f, 0f), sp.accent, rig);
            ant.transform.localRotation = Quaternion.Euler(35f, 0f, -20f * s);
            if (sp.wings)
            {
                CubeJoint w = J(thorax.transform, s < 0 ? BodyPart.ScapulaLeft : BodyPart.ScapulaRight, new Vector3(W * 0.3f * s, H * 0.32f, 0f),
                                new Vector3(L * 0.35f, 0.006f, L * 0.18f), new Vector3(L * 0.17f * s, 0f, -L * 0.05f), C(0.92f, 0.96f, 0.99f), rig);
                w.transform.localRotation = Quaternion.Euler(0f, 25f * s, 10f * s);
            }
        }
        BodyPart[] legs = { BodyPart.LimbA, BodyPart.LimbB, BodyPart.LimbC, BodyPart.LimbD, BodyPart.LimbE, BodyPart.LimbF };
        for (int i = 0; i < 6; i++)
        {
            int s = i % 2 == 0 ? -1 : 1;
            float z = L * (0.1f - (i / 2) * 0.1f);
            CubeJoint up = J(thorax.transform, legs[i], new Vector3(W * 0.4f * s, 0f, z), new Vector3(0.025f, legY * 0.75f, 0.025f), new Vector3(0f, legY * 0.37f, 0f), sp.accent, rig);
            up.transform.localRotation = Quaternion.Euler(0f, 0f, -70f * s);   // muslo hacia fuera y arriba
            CubeJoint low = JNoBind(up.transform, new Vector3(0f, legY * 0.75f, 0f), new Vector3(0.02f, legY * 1.3f, 0.02f), new Vector3(0f, legY * 0.65f, 0f), sp.accent);
            low.transform.localRotation = Quaternion.Euler(0f, 0f, -90f * s);   // tibia hacia abajo hasta el suelo
        }
    }

    // ── Piezas ──────────────────────────────────────────────────────────────────────────────────

    static CubeJoint J(Transform parent, BodyPart part, Vector3 pivot, Vector3 size, Vector3 cubeOffset, Color c, CreatureRig rig)
    {
        CubeJoint j = JNoBind(parent, pivot, size, cubeOffset, c);
        j.part = part;
        j.name = part.ToString();
        rig.bindings.Add(new CreatureRig.PartBinding { part = part, bone = j.transform });
        return j;
    }

    static CubeJoint JNoBind(Transform parent, Vector3 pivot, Vector3 size, Vector3 cubeOffset, Color c)
    {
        GameObject jointGO = new GameObject("Articulacion");
        jointGO.transform.SetParent(parent, false);
        jointGO.transform.localPosition = pivot;
        Transform cube = Seg(jointGO.transform, cubeOffset, size, c);
        CubeJoint j = jointGO.AddComponent<CubeJoint>();
        j.segment = cube;
        return j;
    }

    static Transform Seg(Transform parent, Vector3 local, Vector3 size, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "cubo";
        Collider col = g.GetComponent<Collider>();
        if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = local;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = Mat(c);
        return g.transform;
    }
}
