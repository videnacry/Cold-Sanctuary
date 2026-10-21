using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ARTRÓPODO ARTICULADO DE CUBOS, hecho con los datos de docs/insect-morphology.md (InsectMorphology):
/// - el cuerpo por segmentos (cabeza, tórax, pecíolo, gáster…) con sus largos y anchos relativos a L;
/// - las patas de 5 segmentos (coxa, trocánter, fémur, tibia, tarso) con su pose de reposo; en las arañas, 8 patas de 7
///   segmentos (con patela y metatarso), quelíceros y pedipalpos;
/// - sus rasgos: antenas acodadas (hormigas, abejas), mandíbulas que se abren, élitros, alas (dos pares en las abejas),
///   patas raptoras (mantis) y saltadoras (saltamontes, grillo), sifones y estilete (pulgón);
/// - la marcha: TRÍPODE alterno en los insectos y TETRÁPODO alterno en las arañas (balanceo de la coxa y alzado del
///   trocánter en la fase de vuelo de cada pata).
/// La forma es visual: el cuerpo físico (collider, Anima, NavMeshAgent) sigue siendo el del anfitrión.
/// Los MIRMIDONES (las hormigas humanas del microcosmos, DressMyrmidon) son bípedos y con cara, pero con antena acodada y patas de hormiga.
/// </summary>
public class CubeInsect : MonoBehaviour
{
    class Leg { public Transform coxa, troch, tibia; public Quaternion coxaRest, trochRest, tibiaRest, armCoxa, armTroch, armTibia; public int side, pair; }
    Transform _torso, _neck; Quaternion _torsoRest, _neckRest;
    /// <summary>0 = tranquilo (erguido, centauro, con brazos) · 1 = el cuerpo manda (a todas las patas). Lo puede fijar la conducta (miedo, hambre…); si no, se deduce de la velocidad.</summary>
    public float agitation = -1f;   // −1 = automático (por la velocidad)
    float _calm = 1f;
    Lucidity _lucidity; bool _lucidityChecked;
    readonly List<Leg> _legs = new List<Leg>();
    readonly List<(Transform t, int side, bool arm)> _limbs = new List<(Transform, int, bool)>();
    readonly List<(Transform t, float open, int side)> _jaws = new List<(Transform, float, int)>();
    readonly List<Transform> _wings = new List<Transform>();
    Vector3 _last; float _phase, _seed = -1f;
    bool _spider;

    // ══ INSECTOS Y ARAÑAS, según su morfología ══════════════════════════════════════════════════════════════════
    /// <summary>Viste a <paramref name="host"/> como la especie <paramref name="species"/> de <paramref name="length"/> m de largo (L).</summary>
    // Lo que se dibujó (para redibujar al cambiar de fase: larva → pupa → adulto, o ninfa → adulto).
    public string species;
    public Color tint;
    public float lengthL = 1f;
    Metamorphosis.Phase _shown = Metamorphosis.Phase.Adult;
    float _nextPhaseCheck;

    public static CubeInsect Dress(GameObject host, string species, Color body, float length) => Dress(host, species, body, length, Metamorphosis.Phase.Adult);

    /// <summary>
    /// Viste según la FASE de su metamorfosis (Metamorphosis.PhaseOf): LARVA (gusano segmentado; con patitas si la especie
    /// caza de larva, como la mariquita), PUPA (inmóvil, con las patas plegadas), NINFA (el adulto sin alas) o ADULTO. La
    /// hormiga león es al revés de lo que parece: la larva es el cazador de embudo y el ADULTO es alado.
    /// </summary>
    public static CubeInsect Dress(GameObject host, string species, Color body, float length, Metamorphosis.Phase phase)
    {
        InsectMorphology.Spec sp = InsectMorphology.For(species);
        CubeInsect ci;
        if (sp.name == "Antlion" && phase == Metamorphosis.Phase.Adult) ci = Build(host, InsectMorphology.Named("AntlionAdult"), body, length);
        else if (sp.name == "Antlion" && phase == Metamorphosis.Phase.Larva) ci = Build(host, sp, body, length);
        else if (phase == Metamorphosis.Phase.Larva) ci = Grub(host, sp, body, length, sp.name == "Ladybug" || sp.name == "Antlion");
        else if (phase == Metamorphosis.Phase.Pupa) ci = Pupa(host, body, length);
        else if (phase == Metamorphosis.Phase.Nymph) ci = Build(host, InsectMorphology.Wingless(sp), body, length);
        else ci = Build(host, sp, body, length);
        ci.species = species; ci.tint = body; ci.lengthL = length; ci._shown = phase;
        return ci;
    }

    // LARVA: gusano de segmentos que se estrechan, pálido y blando, con la cabeza oscura; patitas si caza de larva.
    static CubeInsect Grub(GameObject host, InsectMorphology.Spec sp, Color body, float length, bool legs)
    {
        Transform root = NewRoot(host, length, out CubeInsect ci);
        Color pale = Color.Lerp(body, new Color(0.95f, 0.92f, 0.85f), 0.65f), dark = body * 0.5f; dark.a = 1f;
        int n = 8;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)(n - 1), w = Mathf.Lerp(0.2f, 0.12f, Mathf.Abs(t - 0.35f) * 1.6f);
            Part(root, "Segmento", new Vector3(0f, w * 0.5f, 0.45f - t * 0.9f), new Vector3(w, w * 0.9f, 0.12f), i == 0 ? dark : pale);
        }
        if (legs) for (int p = 0; p < 3; p++) for (int s = -1; s <= 1; s += 2) Part(root, "Patita", new Vector3(s * 0.12f, 0.03f, 0.3f - p * 0.08f), new Vector3(0.1f, 0.02f, 0.02f), dark);
        return ci;
    }

    // PUPA: inmóvil, ovalada, con las patas y las alas plegadas contra el cuerpo.
    static CubeInsect Pupa(GameObject host, Color body, float length)
    {
        Transform root = NewRoot(host, length, out CubeInsect ci);
        Color tan = Color.Lerp(body, new Color(0.8f, 0.68f, 0.45f), 0.6f), line = tan * 0.7f; line.a = 1f;
        Part(root, "Pupa_cabeza", new Vector3(0f, 0.11f, 0.32f), new Vector3(0.2f, 0.18f, 0.18f), tan);
        Part(root, "Pupa_torax", new Vector3(0f, 0.13f, 0.1f), new Vector3(0.26f, 0.24f, 0.28f), tan);
        Part(root, "Pupa_abdomen", new Vector3(0f, 0.12f, -0.2f), new Vector3(0.24f, 0.22f, 0.38f), tan);
        for (int s = -1; s <= 1; s += 2) Part(root, "Patas_plegadas", new Vector3(s * 0.06f, 0.02f, 0.1f), new Vector3(0.03f, 0.02f, 0.4f), line);
        return ci;
    }

    static Transform NewRoot(GameObject host, float length, out CubeInsect ci)
    {
        foreach (MeshRenderer mr in host.GetComponents<MeshRenderer>()) mr.enabled = false;
        Transform old = host.transform.Find("Insecto"); if (old != null) DestroyNow(old.gameObject);
        var root = new GameObject("Insecto").transform;
        root.SetParent(host.transform, false);
        Vector3 ls = host.transform.lossyScale;
        root.localScale = new Vector3(1f / Mathf.Max(0.001f, ls.x), 1f / Mathf.Max(0.001f, ls.y), 1f / Mathf.Max(0.001f, ls.z)) * length;
        ci = host.GetComponent<CubeInsect>() ?? host.AddComponent<CubeInsect>();
        ci._legs.Clear(); ci._limbs.Clear(); ci._jaws.Clear(); ci._wings.Clear();
        return root;
    }

    static CubeInsect Build(GameObject host, InsectMorphology.Spec sp, Color body, float length)
    {
        foreach (MeshRenderer mr in host.GetComponents<MeshRenderer>()) mr.enabled = false;
        Transform old = host.transform.Find("Insecto"); if (old != null) DestroyNow(old.gameObject);
        var root = new GameObject("Insecto").transform;
        root.SetParent(host.transform, false);
        Vector3 ls = host.transform.lossyScale;
        root.localScale = new Vector3(1f / Mathf.Max(0.001f, ls.x), 1f / Mathf.Max(0.001f, ls.y), 1f / Mathf.Max(0.001f, ls.z)) * length;
        MeshFilter mf = host.GetComponent<MeshFilter>();
        bool capsule = mf != null && mf.sharedMesh != null && mf.sharedMesh.name.Contains("Capsule");
        Vector3 ground = capsule ? Vector3.down * (1f / Mathf.Max(0.001f, root.localScale.y)) : Vector3.zero;   // (en unidades de L)
        var pivot = new GameObject("Pivote").transform; pivot.SetParent(root, false);

        var ci = host.GetComponent<CubeInsect>() ?? host.AddComponent<CubeInsect>();
        ci._legs.Clear(); ci._limbs.Clear(); ci._jaws.Clear(); ci._wings.Clear(); ci._spider = sp.spider;
        Color dark = body * 0.55f; dark.a = 1f;
        Color wing = new Color(0.85f, 0.92f, 1f, 0.35f);

        // EJE DEL CUERPO: de la cabeza (delante, +z) hacia atrás.
        float total = 0f; foreach (var s in sp.body) total += s.len;
        float z = total * 0.5f, maxH = 0f;
        float y0 = sp.bodyHeight;
        var segZ = new Dictionary<string, (float z, float len, float wid, float hei)>();
        foreach (var s in sp.body)
        {
            float cz = z - s.len * 0.5f;
            Part(pivot, s.name, new Vector3(0f, y0 + s.hei * 0.5f, cz), new Vector3(s.wid, s.hei, s.len), s.name == "Peciolo" ? dark : body);
            segZ[s.name] = (cz, s.len, s.wid, s.hei);
            maxH = Mathf.Max(maxH, s.hei);
            z -= s.len;
        }
        var head = sp.body[0];
        float headZ = segZ[head.name].z, headY = y0 + head.hei * 0.5f;

        // ANTENAS (acodadas: escapo + codo + funículo).
        if (sp.antenna > 0f)
            for (int s = -1; s <= 1; s += 2)
            {
                var a = new GameObject("Antena").transform; a.SetParent(pivot, false);
                a.localPosition = new Vector3(s * head.wid * 0.25f, headY + head.hei * 0.4f, headZ + head.len * 0.4f);
                if (sp.name == "Aphid") a.localRotation = Quaternion.Euler(-150f, s * 18f, 0f);   // hacia atrás sobre el cuerpo
                else a.localRotation = Quaternion.Euler(-30f, s * 25f, 0f);
                if (sp.elbowedAntenna)
                {
                    float scape = sp.antenna * 0.45f;
                    Part(a, "Escapo", new Vector3(0f, 0f, scape * 0.5f), new Vector3(0.02f, 0.02f, scape), dark);
                    var elbow = new GameObject("Codo").transform; elbow.SetParent(a, false); elbow.localPosition = new Vector3(0f, 0f, scape); elbow.localRotation = Quaternion.Euler(70f, s * 20f, 0f);
                    float fun = sp.antenna - scape;
                    Part(elbow, "Funiculo", new Vector3(0f, 0f, fun * 0.5f), new Vector3(0.018f, 0.018f, fun), dark);
                }
                else Part(a, "Antena", new Vector3(0f, 0f, sp.antenna * 0.5f), new Vector3(0.015f, 0.015f, sp.antenna), dark);
            }

        // MANDÍBULAS (se abren y cierran).
        if (sp.mandible > 0f)
            for (int s = -1; s <= 1; s += 2)
            {
                var j = new GameObject("Mandibula").transform; j.SetParent(pivot, false);
                j.localPosition = new Vector3(s * head.wid * 0.25f, headY - head.hei * 0.2f, headZ + head.len * 0.5f);
                j.localRotation = Quaternion.Euler(0f, -s * sp.mandibleOpen, 0f);
                Part(j, "Mandibula", new Vector3(0f, 0f, sp.mandible * 0.5f), new Vector3(0.03f, 0.025f, sp.mandible), dark);
                ci._jaws.Add((j, sp.mandibleOpen, s));
            }

        // RASGOS.
        if (sp.siphons)   // pulgón: sifones, cauda y estilete
        {
            var c = segZ["Cuerpo"];
            for (int s = -1; s <= 1; s += 2) Part(pivot, "Sifon", new Vector3(s * c.wid * 0.25f, y0 + c.hei * 0.85f, c.z - c.len * 0.3f), new Vector3(0.03f, 0.03f, 0.15f), dark, -30f, 0f);
            Part(pivot, "Cauda", new Vector3(0f, y0 + c.hei * 0.4f, c.z - c.len * 0.5f - 0.03f), new Vector3(0.05f, 0.04f, 0.06f), dark);
        }
        if (sp.stylet) Part(pivot, "Estilete", new Vector3(0f, y0, headZ - 0.02f), new Vector3(0.02f, 0.02f, 0.2f), dark, 70f, 0f);
        if (sp.elytra)   // élitros: la cúpula ya es el segmento; la sutura central y, en la mariquita, los puntos
        {
            var e = segZ["Elitros"];
            Part(pivot, "Sutura", new Vector3(0f, y0 + e.hei + 0.005f, e.z), new Vector3(0.012f, 0.01f, e.len * 0.95f), dark);
            if (sp.name == "Ladybug")
                for (int i = 0; i < 6; i++)
                    Part(pivot, "Punto", new Vector3((i % 2 == 0 ? -1 : 1) * e.wid * (0.15f + 0.1f * (i / 2 % 2)), y0 + e.hei + 0.006f, e.z + e.len * (0.25f - (i / 2) * 0.22f)), new Vector3(0.1f, 0.01f, 0.1f), new Color(0.05f, 0.05f, 0.05f));
        }
        if (sp.wings)   // tegminas / alas plegadas sobre el abdomen
        {
            var ab = segZ["Abdomen"];
            for (int s = -1; s <= 1; s += 2) Part(pivot, "Ala", new Vector3(s * 0.02f, y0 + ab.hei + 0.01f, ab.z + 0.05f), new Vector3(ab.wid * 0.6f, 0.01f, ab.len * 1.05f), Color.Lerp(body, new Color(0.6f, 0.65f, 0.4f), 0.4f));
        }
        if (sp.beeWings)   // dos pares de alas (anterior 0,65 L, posterior 0,45 L), plegadas; aletean al volar
        {
            var th = segZ["Torax"];
            for (int s = -1; s <= 1; s += 2)
                for (int k = 0; k < 2; k++)
                {
                    var w = new GameObject(k == 0 ? "Ala_anterior" : "Ala_posterior").transform; w.SetParent(pivot, false);
                    w.localPosition = new Vector3(s * th.wid * 0.3f, y0 + th.hei, th.z - k * 0.05f);
                    w.localRotation = Quaternion.Euler(0f, s * (160f - k * 8f), 0f);
                    float wl = k == 0 ? 0.65f : 0.45f;
                    Part(w, "Membrana", new Vector3(0f, 0f, wl * 0.5f), new Vector3(k == 0 ? 0.22f : 0.15f, 0.005f, wl), wing);
                    ci._wings.Add(w);
                }
        }
        if (sp.spider)   // quelíceros con colmillo y pedipalpos
        {
            var ct = segZ["Cefalotorax"];
            for (int s = -1; s <= 1; s += 2)
            {
                Part(pivot, "Queliceros", new Vector3(s * 0.04f, y0 + 0.02f, ct.z + ct.len * 0.5f + 0.02f), new Vector3(0.06f, 0.12f, 0.06f), dark, 20f, 0f);
                Part(pivot, "Pedipalpo", new Vector3(s * 0.09f, y0 + 0.03f, ct.z + ct.len * 0.5f + 0.1f), new Vector3(0.02f, 0.02f, 0.25f), dark, 15f, s * 15f);
            }
        }

        // PATAS: de la coxa al tarso, con la pose de reposo de la ficha; luego se apoya el cuerpo para que los tarsos toquen el suelo.
        string thoraxName = sp.spider ? "Cefalotorax" : (segZ.ContainsKey("Torax") ? "Torax" : segZ.ContainsKey("Protorax") ? "Protorax" : segZ.ContainsKey("Pronoto") ? "Pronoto" : sp.body[1].name);
        var tx = segZ[thoraxName];
        if (sp.name == "Mantis" || sp.name == "Antlion") tx = (segZ["Abdomen"].z + segZ["Abdomen"].len * 0.35f, segZ["Abdomen"].len * 0.3f, segZ["Abdomen"].wid, segZ["Abdomen"].hei);
        if (sp.name == "Ladybug" || sp.name == "DungBeetle") tx = (segZ["Elitros"].z + segZ["Elitros"].len * 0.25f, segZ["Elitros"].len * 0.5f, segZ["Elitros"].wid * 0.6f, segZ["Elitros"].hei);
        int pairs = sp.legs.Length;
        var tips = new List<Transform>();
        for (int p = 0; p < pairs; p++)
            for (int s = -1; s <= 1; s += 2)
            {
                float lz = tx.z + tx.len * (pairs == 1 ? 0f : 0.4f - 0.8f * p / (pairs - 1));
                if (sp.raptorial && p == 0) lz = segZ["Protorax"].z + segZ["Protorax"].len * 0.35f;   // la mantis: patas raptoras en el «cuello»
                var leg = BuildLeg(pivot, sp, p, s, new Vector3(s * tx.wid * 0.45f, y0 + 0.02f, lz), dark, tips);
                ci._legs.Add(leg);
            }
        // Apoyo: los tarsos al suelo (y el vientre nunca por debajo de él).
        float minTip = float.MaxValue; foreach (Transform t in tips) minTip = Mathf.Min(minTip, root.InverseTransformPoint(t.position).y - pivot.localPosition.y);
        float lift = minTip == float.MaxValue ? 0f : -minTip;
        pivot.localPosition = ground + Vector3.up * Mathf.Max(lift, -y0 * 0.9f);

        // ── CENTAURO (el microcosmos: insectos con cara e inteligencia humana) ──────────────────────────────────────
        // La parte delantera (cabeza y tórax, con las patas delanteras) cuelga de un TORSO que se ENDEREZA cuando el insecto
        // está tranquilo: cabeza arriba, las patas delanteras como BRAZOS con manos, y anda con el resto (la hormiga a 4,
        // la araña a 6). Cuando el cuerpo toma el control (corre, huye, tiene miedo o hambre) baja el torso y vuelve a ser
        // un animal a 6 (u 8) patas. La cabeza lleva CARA (ojos, cejas y boca) y un cuello que la mantiene mirando al frente.
        float pivotZ = sp.body.Length >= 3 ? segZ[sp.body[1].name].z - segZ[sp.body[1].name].len * 0.5f : segZ[sp.body[sp.body.Length > 1 ? 1 : 0].name].z;
        if (sp.spider) pivotZ = segZ["Cefalotorax"].z;
        float chestH = sp.body.Length >= 3 ? segZ[sp.body[1].name].hei : head.hei;
        var torso = new GameObject("Torso").transform; torso.SetParent(pivot, false); torso.localPosition = new Vector3(0f, y0 + chestH * 0.5f, pivotZ);
        var neck = new GameObject("Cuello").transform; neck.SetParent(torso, false);
        neck.localPosition = torso.InverseTransformPoint(pivot.TransformPoint(new Vector3(0f, headY, headZ - head.len * 0.5f)));
        var move = new List<Transform>();
        foreach (Transform c in pivot) if (c != torso && c.localPosition.z > pivotZ + 1e-3f && !(c.name.StartsWith("Pata_") && !c.name.StartsWith("Pata_0_"))) move.Add(c);
        foreach (Transform c in pivot) if (c.name.StartsWith("Pata_0_") && !move.Contains(c)) move.Add(c);
        foreach (Transform c in move)
        {
            bool headPart = c.name == head.name || c.name == "Antena" || c.name == "Mandibula" || c.name == "Queliceros";
            c.SetParent(headPart ? neck : torso, true);
        }
        if (sp.body.Length < 3 || sp.spider) Part(torso, "Pecho", new Vector3(0f, 0f, 0.06f), new Vector3(head.wid * 0.9f, chestH * 0.8f, 0.14f), body);   // un pecho al que se unen los brazos
        // Cara: ojos (con pupila y brillo), cejas y boca en el frente de la cabeza.
        Vector3 hf = neck.InverseTransformPoint(pivot.TransformPoint(new Vector3(0f, headY, headZ + head.len * 0.5f + 0.002f)));
        float ew = head.wid * 0.28f, eh = head.hei * 0.3f;
        for (int s = -1; s <= 1; s += 2)
        {
            Part(neck, "Ojo", hf + new Vector3(s * head.wid * 0.24f, head.hei * 0.12f, 0f), new Vector3(ew, eh, 0.006f), Color.white);
            Part(neck, "Pupila", hf + new Vector3(s * head.wid * 0.24f, head.hei * 0.1f, 0.004f), new Vector3(ew * 0.5f, eh * 0.55f, 0.005f), new Color(0.08f, 0.06f, 0.05f));
            Part(neck, "Brillo", hf + new Vector3(s * head.wid * 0.21f, head.hei * 0.16f, 0.007f), new Vector3(ew * 0.18f, eh * 0.18f, 0.004f), Color.white);
            Part(neck, "Ceja", hf + new Vector3(s * head.wid * 0.24f, head.hei * 0.32f, 0.003f), new Vector3(ew * 1.1f, head.hei * 0.05f, 0.005f), dark);
        }
        Part(neck, "Boca", hf + new Vector3(0f, -head.hei * 0.22f, 0.003f), new Vector3(head.wid * 0.3f, head.hei * 0.05f, 0.005f), new Color(0.25f, 0.1f, 0.1f));
        // Manos en las patas delanteras (al final del tarso).
        foreach (Leg l in ci._legs)
            if (l.pair == 0)
            {
                Transform tip = null; foreach (Transform t in l.coxa.GetComponentsInChildren<Transform>()) if (t.name == "Punta") tip = t;
                if (tip != null) Part(tip, "Mano", Vector3.zero, new Vector3(sp.legWidth * 2.2f, sp.legWidth * 1.4f, sp.legWidth * 2.2f), dark);
                // Pose de BRAZO (en el marco del torso erguido): colgando junto al cuerpo, con el codo doblado hacia delante.
                l.armCoxa = Quaternion.Euler(0f, l.side * 168f, 0f);
                l.armTroch = Quaternion.Euler(-10f, 0f, 0f);
                l.tibia = FindDeep(l.coxa, "Tibia_art"); if (l.tibia != null) { l.tibiaRest = l.tibia.localRotation; l.armTibia = Quaternion.Euler(-70f, 0f, 0f); }
            }
        ci._torso = torso; ci._neck = neck; ci._torsoRest = torso.localRotation; ci._neckRest = neck.localRotation;
        return ci;
    }

    static Leg BuildLeg(Transform pivot, InsectMorphology.Spec sp, int p, int s, Vector3 hip, Color c, List<Transform> tips)
    {
        float w = sp.legWidth;
        float[] L = sp.legs[p];
        bool raptor = sp.raptorial && p == 0, jump = sp.jumper && p == sp.legs.Length - 1;
        // Reposo de la coxa (pro/retracción): P1 +30°, P2 0°, P3 −30° (arañas: +45, +15, −15, −45).
        float restYaw = sp.spider ? new[] { 45f, 15f, -15f, -45f }[p] : new[] { 30f, 0f, -30f }[Mathf.Min(p, 2)];
        if (raptor) restYaw = 75f; if (jump) restYaw = -70f;
        var coxa = new GameObject($"Pata_{p}_{(s < 0 ? "I" : "D")}").transform; coxa.SetParent(pivot, false);
        coxa.localPosition = hip; coxa.localRotation = Quaternion.Euler(0f, s * (90f - restYaw), 0f);
        Part(coxa, "Coxa", new Vector3(0f, 0f, sp.coxa * 0.5f), new Vector3(w * 1.3f, w * 1.3f, sp.coxa), c);
        Transform troch = Joint(coxa, "Trocanter", sp.coxa, raptor ? 20f : jump ? -30f : sp.spider ? -45f : -40f);
        Part(troch, "Trocanter", new Vector3(0f, 0f, sp.troch * 0.5f), new Vector3(w * 1.2f, w * 1.2f, sp.troch), c);
        if (sp.spider)
        {
            // fémur (45° arriba) · patela (doblada 90°) · tibia · metatarso · tarso
            Transform fe = Joint(troch, "Femur", sp.troch, 0f); Part(fe, "Femur", new Vector3(0f, 0f, L[0] * 0.5f), new Vector3(w, w, L[0]), c);
            Transform pa = Joint(fe, "Patela", L[0], 90f); Part(pa, "Patela", new Vector3(0f, 0f, L[1] * 0.5f), new Vector3(w, w, L[1]), c);
            Transform ti = Joint(pa, "Tibia", L[1], 15f); Part(ti, "Tibia", new Vector3(0f, 0f, L[2] * 0.5f), new Vector3(w * 0.9f, w * 0.9f, L[2]), c);
            Transform me = Joint(ti, "Metatarso", L[2], 10f); Part(me, "Metatarso", new Vector3(0f, 0f, L[3] * 0.5f), new Vector3(w * 0.8f, w * 0.8f, L[3]), c);
            Transform ta = Joint(me, "Tarso", L[3], 5f); Part(ta, "Tarso", new Vector3(0f, 0f, L[4] * 0.5f), new Vector3(w * 0.7f, w * 0.7f, L[4]), c);
            tips.Add(Tip(ta, L[4]));
        }
        else
        {
            float fw = jump ? w * 3f : raptor ? w * 1.6f : w;   // fémur saltador muy grueso; raptor con espinas (más ancho)
            Transform fe = Joint(troch, "Femur", sp.troch, raptor ? -70f : 0f); Part(fe, "Femur", new Vector3(0f, 0f, L[0] * 0.5f), new Vector3(fw, fw, L[0]), c);
            // tibia: rodilla a 100° (saltadora y raptora, plegadas contra el fémur)
            Transform ti = Joint(fe, "Tibia", L[0], jump ? 172f : raptor ? 168f : 100f); Part(ti, "Tibia", new Vector3(0f, 0f, L[1] * 0.5f), new Vector3(w * 0.85f, w * 0.85f, L[1]), c);
            if (raptor) for (int k = 0; k < 3; k++) Part(fe, "Espina", new Vector3(0f, -fw * 0.6f, L[0] * (0.3f + k * 0.25f)), new Vector3(w * 0.3f, w * 0.8f, w * 0.3f), c);
            Transform ta = Joint(ti, "Tarso", L[1], jump ? -150f : raptor ? 0f : -45f); Part(ta, "Tarso", new Vector3(0f, 0f, L[2] * 0.5f), new Vector3(w * 0.7f, w * 0.6f, L[2]), c);
            tips.Add(Tip(ta, L[2]));
        }
        return new Leg { coxa = coxa, troch = troch, coxaRest = coxa.localRotation, trochRest = troch.localRotation, side = s, pair = p };
    }

    static Transform FindDeep(Transform root, string name) { foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t; return null; }

    static Transform Joint(Transform parent, string name, float at, float pitch)
    {
        var j = new GameObject(name + "_art").transform; j.SetParent(parent, false);
        j.localPosition = new Vector3(0f, 0f, at); j.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        return j;
    }

    static Transform Tip(Transform parent, float at) { var t = new GameObject("Punta").transform; t.SetParent(parent, false); t.localPosition = new Vector3(0f, 0f, at); return t; }

    static Transform Part(Transform parent, string name, Vector3 pos, Vector3 size, Color c, float pitch = 0f, float yaw = 0f)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        DestroyNow(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos; g.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f); g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
        return g.transform;
    }

    static void DestroyNow(Object o) { if (o == null) return; if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }

    // ══ MIRMIDÓN (hormiga humana: los humanos del microcosmos) ═══════════════════════════════════════════════════
    /// <summary>
    /// Viste a <paramref name="host"/> como MIRMIDÓN: una hormiga de verdad (la morfología de la ficha) con CARA e
    /// inteligencia humana, en postura de CENTAURO: tranquila va erguida, con la cabeza arriba y las patas delanteras como
    /// brazos con manos, andando a 4 patas; cuando el cuerpo toma el control corre a 6, como un animal.
    /// <paramref name="height"/> ≈ su altura erguida; <paramref name="bulk"/> la ensancha (Nasatya, la «almohada gigante»);
    /// <paramref name="aphid"/> = pulgón (Ambrosio).
    /// </summary>
    public static CubeInsect DressMyrmidon(GameObject host, Color body, float height, float bulk = 1f, bool aphid = false)
    {
        CubeInsect ci = Dress(host, aphid ? "Aphid" : "Ant", body, height * (aphid ? 1.1f : 1.35f));
        Transform root = host.transform.Find("Insecto");
        if (root != null && bulk != 1f) root.localScale = new Vector3(root.localScale.x * bulk, root.localScale.y * Mathf.Sqrt(bulk), root.localScale.z);
        return ci;
    }

    // ══ MOVIMIENTO ═════════════════════════════════════════════════════════════════════════════════════════════
    void Awake()
    {
        Transform root = transform.Find("Insecto");
        if (root == null || _legs.Count > 0 || _limbs.Count > 0) { _last = transform.position; return; }
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.StartsWith("Pata_") && t.name.Length >= 8)
            {
                Transform tr = t.Find("Trocanter_art");
                var lg = new Leg { coxa = t, troch = tr, coxaRest = t.localRotation, trochRest = tr != null ? tr.localRotation : Quaternion.identity, side = t.name.EndsWith("I") ? -1 : 1, pair = t.name[5] - '0' };
                if (lg.pair == 0) { lg.armCoxa = Quaternion.Euler(0f, lg.side * 168f, 0f); lg.armTroch = Quaternion.Euler(-10f, 0f, 0f); lg.tibia = FindDeep(t, "Tibia_art"); if (lg.tibia != null) { lg.tibiaRest = lg.tibia.localRotation; lg.armTibia = Quaternion.Euler(-70f, 0f, 0f); } }
                _legs.Add(lg);
            }
            else if (t.name.StartsWith("Pierna_")) _limbs.Add((t, t.name.EndsWith("I") ? -1 : 1, false));
            else if (t.name.StartsWith("Brazo_")) _limbs.Add((t, t.name.EndsWith("I") ? -1 : 1, true));
            else if (t.name == "Mandibula" && t.childCount > 0) _jaws.Add((t, Mathf.Abs(Mathf.DeltaAngle(0f, t.localEulerAngles.y)), t.localPosition.x < 0f ? -1 : 1));
            else if (t.name.StartsWith("Ala_")) _wings.Add(t);
        }
        _spider = root.GetComponentsInChildren<Transform>(true).Length > 0 && root.Find("Pivote/Queliceros") != null;
        _torso = FindDeep(root, "Torso"); _neck = FindDeep(root, "Cuello"); _torsoRest = Quaternion.identity; _neckRest = Quaternion.identity;
        _last = transform.position;
        if (_seed < 0f) _seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        // METAMORFOSIS: si cambia de fase (larva → pupa → adulto, ninfa → adulto), se redibuja.
        if (!string.IsNullOrEmpty(species) && Time.time >= _nextPhaseCheck)
        {
            _nextPhaseCheck = Time.time + 1f;
            Animal an = GetComponent<Animal>();
            if (an != null) { Metamorphosis.Phase ph = Metamorphosis.PhaseOf(an); if (ph != _shown) { Dress(gameObject, species, tint, lengthL, ph); Awake(); } }
        }
        Vector3 v = (transform.position - _last) / Mathf.Max(Time.deltaTime, 1e-4f); v.y = 0f;
        _last = transform.position;
        if (_seed < 0f) _seed = Random.Range(0f, 100f);
        float speed = v.magnitude;
        _phase += speed * Time.deltaTime * 4f;
        float amp = Mathf.Clamp01(speed / 1.5f) * 25f;
        // Marcha: TRÍPODE alterno (P1 izq + P2 der + P3 izq, y luego las otras) o TETRÁPODO en arañas (mismo patrón con P4).
        // CENTAURO: tranquilo → erguido (torso arriba, cabeza al frente, patas delanteras como brazos); el cuerpo manda
        // (corre, o la conducta lo agita: miedo, hambre…) → a todas las patas, como un animal.
        float Lw = transform.Find("Insecto") != null ? transform.Find("Insecto").lossyScale.z : 1f;
        // Postura según la MENTE (Lucidity): lúcido = erguido, con brazos; si no, el cuerpo manda. Sin Lucidity, por la velocidad.
        if (_lucidity == null && !_lucidityChecked) { _lucidity = GetComponent<Lucidity>(); _lucidityChecked = true; }
        float target = agitation >= 0f ? agitation : _lucidity != null ? (_lucidity.Lucid ? 0f : 1f) : (speed > 1.8f * Mathf.Max(0.1f, Lw) ? 1f : 0f);
        if (_torso != null) _calm = Mathf.MoveTowards(_calm, 1f - target, Time.deltaTime * 2.5f);
        if (_torso != null) _torso.localRotation = _torsoRest * Quaternion.Euler(-75f * _calm, 0f, 0f);
        if (_neck != null) _neck.localRotation = _neckRest * Quaternion.Euler(75f * _calm, 0f, 0f);
        // Marcha: TRÍPODE alterno (P1 izq + P2 der + P3 izq, y luego las otras) o TETRÁPODO en arañas; erguido, sin las delanteras.
        foreach (Leg l in _legs)
        {
            if (l.coxa == null) continue;
            int group = (l.pair % 2 == 0) == (l.side < 0) ? 0 : 1;
            float ph = Mathf.Sin(_phase + group * Mathf.PI);
            Quaternion cx = l.coxaRest * Quaternion.Euler(0f, ph * amp * l.side, 0f);
            Quaternion tr = l.trochRest * Quaternion.Euler(-Mathf.Max(0f, ph) * amp * 0.6f, 0f, 0f);   // se alza en la fase de vuelo
            if (l.pair == 0 && _torso != null)   // brazos cuando está erguido (balanceo suave al andar)
            {
                Quaternion armSwing = Quaternion.Euler(Mathf.Sin(_phase + (l.side < 0 ? 0f : Mathf.PI)) * amp * 0.4f, 0f, 0f);
                cx = Quaternion.Slerp(cx, l.armCoxa * armSwing, _calm);
                tr = Quaternion.Slerp(tr, l.armTroch, _calm);
                if (l.tibia != null) l.tibia.localRotation = Quaternion.Slerp(l.tibiaRest, l.armTibia, _calm);
            }
            l.coxa.localRotation = cx;
            if (l.troch != null) l.troch.localRotation = tr;
        }
        foreach (var (t, side, arm) in _limbs)
        {
            if (t == null) continue;
            float s = Mathf.Sin(_phase + (side < 0 ? 0f : Mathf.PI)) * amp * (arm ? -0.8f : 1f);
            t.localRotation = Quaternion.Euler(s, 0f, 0f);
        }
        // Mandíbulas: se abren y cierran de vez en cuando (y al andar, un poco).
        float open = Mathf.Max(0f, Mathf.Sin(Time.time * 0.7f + _seed)) * 25f;
        foreach (var (t, rest, side) in _jaws) if (t != null) t.localRotation = Quaternion.Euler(0f, -side * (rest + open), 0f);
        // Alas: aleteo rápido (≈90° de amplitud) cuando se mueve deprisa; plegadas en reposo.
        foreach (Transform w in _wings)
        {
            if (w == null) continue;
            float side = w.localPosition.x < 0f ? -1f : 1f;
            float flap = speed > 0.5f ? Mathf.Sin(Time.time * 110f) * 45f : 0f;
            w.localRotation = Quaternion.Euler(0f, side * (speed > 0.5f ? 90f : 160f), flap * side);
        }
    }
}
