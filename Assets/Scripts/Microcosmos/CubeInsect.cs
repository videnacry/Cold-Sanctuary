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
public partial class CubeInsect : MonoBehaviour
{
    class Leg { public Transform coxa, troch, tibia; public Quaternion coxaRest, trochRest, tibiaRest, armCoxa, armTroch, armTibia; public int side, pair; public bool arm; }
    Transform _torso, _neck; Quaternion _torsoRest, _neckRest;
    float _push, _pushUntil;
    /// <summary>Empuja con los brazos durante este fotograma (y 0,25 s): <paramref name="k"/> 0 = codos doblados, 1 = estirados.</summary>
    public void Push(float k) { _push = Mathf.Clamp01(k); _pushUntil = Time.time + 0.25f; }
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
    /// <summary>Fase que está dibujada ahora (larva, pupa, ninfa, joven o adulto).</summary>
    public Metamorphosis.Phase Shown => _shown;
    public bool female = true;   // el sexo dibujado (InsectMorphology.ForSex); si el Animal es del otro, se redibuja
    [SerializeField, HideInInspector] Metamorphosis.Phase _shown = Metamorphosis.Phase.Adult;   // se guarda con la escena (la galería, las fases)
    float _nextPhaseCheck;

    public static CubeInsect Dress(GameObject host, string species, Color body, float length) => Dress(host, species, body, length, Metamorphosis.Phase.Adult);

    /// <summary>
    /// Viste según la FASE de su metamorfosis (Metamorphosis.PhaseOf): LARVA (gusano segmentado; con patitas si la especie
    /// caza de larva, como la mariquita), PUPA (inmóvil, con las patas plegadas), NINFA (el adulto sin alas) o ADULTO. La
    /// hormiga león es al revés de lo que parece: la larva es el cazador de embudo y el ADULTO es alado.
    /// </summary>
    /// <paramref name="length"/> es el largo del ADULTO: cada fase se dibuja a su tamaño real relativo a él
    /// (<see cref="InsectMorphology.PhaseScale"/>: la ninfa de cucaracha ×0,85, la zoea del cangrejo ×0,03, la larva
    /// madura de luciérnaga ×1,4), en el mundo vivo igual que en la galería.
    public static CubeInsect Dress(GameObject host, string species, Color body, float length, Metamorphosis.Phase phase, bool female = true)
    {
        float adult = length;
        length *= InsectMorphology.PhaseScale(species, phase);
        InsectMorphology.Spec sp = InsectMorphology.ForSex(InsectMorphology.For(species), female);   // macho o hembra (dimorfismo)
        // Las fases con forma PROPIA (la zoea y la megalopa del cangrejo, la ninfa excavadora de la cigarra, la manca de la
        // cochinilla, la larva de 6 patas del ácaro, el caracol sin labio, la lombriz sin clitelo…): CubeInsect.Fauna.cs.
        CubeInsect ci = FaunaStage(host, sp, body, length, phase, female);
        if (ci != null) { }
        else if (sp.name == "Antlion" && phase == Metamorphosis.Phase.Adult) ci = Build(host, InsectMorphology.Named("AntlionAdult"), body, length);
        else if (sp.name == "Antlion" && phase == Metamorphosis.Phase.Larva) ci = Build(host, sp, body, length);
        else if (phase == Metamorphosis.Phase.Larva) ci = Grub(host, sp, body, length, sp.name == "Ladybug" || sp.name == "Antlion");
        else if (phase == Metamorphosis.Phase.Pupa) ci = Pupa(host, body, length);
        else if (phase == Metamorphosis.Phase.Nymph) ci = Build(host, InsectMorphology.Wingless(sp), body, length);
        else ci = Build(host, sp, body, length);
        ci.species = species; ci.tint = body; ci.lengthL = adult; ci._shown = phase; ci.female = female;
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
            Part(pivot, s.name, new Vector3(0f, y0 + s.hei * 0.5f, cz), new Vector3(s.wid, s.hei, s.len), SegColor(sp, s.name, body, dark));
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
            var c = segZ["Abdomen"];
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
        if (sp.wings)   // tegminas / alas plegadas sobre el abdomen (desde su base: cortas en la tijereta, largas en tejado en la cigarra)
        {
            var ab = segZ["Abdomen"];
            float wl = sp.fx != null ? sp.fx.wingLen : 1.05f;
            Color wc = sp.fx != null && sp.fx.clearWings ? new Color(0.88f, 0.92f, 0.95f, 0.35f) : Color.Lerp(body, new Color(0.6f, 0.65f, 0.4f), 0.4f);
            for (int s = -1; s <= 1; s += 2) Part(pivot, "Ala", new Vector3(s * 0.02f * (sp.fx != null && sp.fx.clearWings ? 4f : 1f), y0 + ab.hei + 0.01f, ab.z + 0.05f + ab.len * (1.05f - wl) * 0.5f), new Vector3(ab.wid * 0.6f, 0.01f, ab.len * wl), wc, 0f, sp.fx != null && sp.fx.clearWings ? s * 6f : 0f);
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
        if (sp.spider && (sp.fx == null || !sp.fx.crustacean))   // quelíceros con colmillo y pedipalpos (no en los cangrejos)
        {
            var ct = segZ[sp.body[0].name];
            bool palps = sp.fx == null || (sp.fx.chelae <= 0f && sp.fx.palpLegs <= 0f);   // el escorpión y el solífugo los tienen propios
            for (int s = -1; s <= 1; s += 2)
            {
                if (sp.fx == null || sp.fx.bigChelicerae <= 0f) Part(pivot, "Queliceros", new Vector3(s * 0.04f, y0 + 0.02f, ct.z + ct.len * 0.5f + 0.02f), new Vector3(0.06f, 0.12f, 0.06f), dark, 20f, 0f);
                if (palps) Part(pivot, "Pedipalpo", new Vector3(s * 0.09f, y0 + 0.03f, ct.z + ct.len * 0.5f + 0.1f), new Vector3(0.02f, 0.02f, 0.25f), dark, 15f, s * 15f);
                // MACHO: los bulbos copuladores en la punta de los pedipalpos, como guantes de boxeo.
                if (sp.palpBulbs) Part(pivot, "Bulbo_pedipalpo", new Vector3(s * 0.12f, y0 + 0.06f, ct.z + ct.len * 0.5f + 0.22f), new Vector3(0.07f, 0.07f, 0.08f), dark);
            }
        }
        // DIMORFISMO: el cuerno del macho de pelotero (en la cabeza, hacia arriba y atrás) y el oviscapto de la hembra.
        if (sp.horn) Part(pivot, "Cuerno", new Vector3(0f, headY + head.hei * 0.6f, headZ), new Vector3(0.05f, 0.25f, 0.05f), dark, -25f, 0f);
        if (sp.ovipositor > 0f)
        {
            var tail = segZ[sp.body[sp.body.Length - 1].name];
            Part(pivot, "Oviscapto", new Vector3(0f, y0 + tail.hei * 0.4f, tail.z - tail.len * 0.5f - sp.ovipositor * 0.5f), new Vector3(0.02f, 0.025f, sp.ovipositor), dark, -8f, 0f);
        }

        // PATAS: de la coxa al tarso, con la pose de reposo de la ficha; luego se apoya el cuerpo para que los tarsos toquen el suelo.
        string thoraxName = sp.spider ? sp.body[0].name : (segZ.ContainsKey("Torax") ? "Torax" : segZ.ContainsKey("Protorax") ? "Protorax" : segZ.ContainsKey("Pronoto") ? "Pronoto" : sp.body[1].name);
        var tx = segZ[thoraxName];
        if (sp.name == "Mantis" || sp.name == "Antlion") tx = (segZ["Abdomen"].z + segZ["Abdomen"].len * 0.35f, segZ["Abdomen"].len * 0.3f, segZ["Abdomen"].wid, segZ["Abdomen"].hei);
        if (sp.elytra && !segZ.ContainsKey("Torax")) tx = (segZ["Elitros"].z + segZ["Elitros"].len * 0.25f, segZ["Elitros"].len * 0.5f, segZ["Elitros"].wid * 0.6f, segZ["Elitros"].hei);
        if (sp.fx != null && sp.fx.legTo > sp.fx.legFrom)   // patas a lo largo de un tramo (cochinilla: 7 pares; cucaracha; ácaro)
        {
            float front = total * 0.5f, a = front - sp.fx.legFrom * total, b = front - sp.fx.legTo * total, w = 0f;
            foreach (var s in sp.body) w = Mathf.Max(w, s.wid);
            tx = ((a + b) * 0.5f, (a - b) / 0.8f, w * 0.9f, maxH);
        }
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
        // Piezas de la fauna de cada era (cola con aguijón, pinzas, ojos en pedúnculo, concha, tentáculos, fórceps, linterna…).
        if (sp.fx != null) FaunaParts(ci, pivot, sp, segZ, y0, total, body, dark);

        // ── CENTAURO (el microcosmos: insectos con cara e inteligencia humana) ──────────────────────────────────────
        // La parte delantera (cabeza y tórax, con las patas delanteras) cuelga de un TORSO que se ENDEREZA cuando el insecto
        // está tranquilo: cabeza arriba, las patas delanteras como BRAZOS con manos, y anda con el resto (la hormiga a 4,
        // la araña a 6). Cuando el cuerpo toma el control (corre, huye, tiene miedo o hambre) baja el torso y vuelve a ser
        // un animal a 6 (u 8) patas. La cabeza lleva CARA (ojos, cejas y boca) y un cuello que la mantiene mirando al frente.
        float pivotZ = sp.body.Length >= 3 ? segZ[sp.body[1].name].z - segZ[sp.body[1].name].len * 0.5f : segZ[sp.body[sp.body.Length > 1 ? 1 : 0].name].z;
        // ARAÑA: la cintura va detrás del cefalotórax, que se endereza entero (con los quelíceros y el 1.er par como brazos);
        // antes giraba desde su centro y apenas se levantaba.
        if (sp.spider) pivotZ = segZ[sp.body[0].name].z - segZ[sp.body[0].name].len * 0.5f;
        // MANTIS (2 pares de brazos): la cintura va entre el 2.º y el 3.er par; se yergue sobre las dos patas de atrás.
        if (sp.armPairs >= 2 && pairs >= 3) pivotZ = tx.z - tx.len * 0.2f;
        if (sp.fx != null && sp.fx.waist > 0f) pivotZ = total * 0.5f - sp.fx.waist * total;   // la cintura de la ficha (cangrejo, ácaro, lombriz)
        float chestH = sp.body.Length >= 3 ? segZ[sp.body[1].name].hei : head.hei;
        var torso = new GameObject("Torso").transform; torso.SetParent(pivot, false); torso.localPosition = new Vector3(0f, y0 + chestH * 0.5f, pivotZ);
        var neck = new GameObject("Cuello").transform; neck.SetParent(torso, false);
        neck.localPosition = torso.InverseTransformPoint(pivot.TransformPoint(new Vector3(0f, headY, headZ - head.len * 0.5f)));
        // En la araña el cuello sale del FRENTE y lo alto del cefalotórax: su cabeza quedaba metida dentro y no se le veía la cara.
        if (sp.spider) neck.localPosition = torso.InverseTransformPoint(pivot.TransformPoint(new Vector3(0f, y0 + head.hei, headZ + head.len * 0.5f)));
        var move = new List<Transform>();
        bool ArmLeg(Transform c) => c.name.StartsWith("Pata_") && c.name.Length > 5 && c.name[5] - '0' < sp.armPairs;
        foreach (Transform c in pivot) if (c != torso && c.localPosition.z > pivotZ + 1e-3f && !(c.name.StartsWith("Pata_") && !ArmLeg(c))) move.Add(c);
        foreach (Transform c in pivot) if (ArmLeg(c) && !move.Contains(c)) move.Add(c);
        foreach (Transform c in move)
        {
            bool headPart = (c.name == head.name && !sp.spider) || c.name == "Antena" || c.name == "Mandibula" || c.name == "Queliceros";
            c.SetParent(headPart ? neck : torso, true);
        }
        if (sp.body.Length < 3 || sp.spider) Part(torso, "Pecho", new Vector3(0f, 0f, 0.06f), new Vector3(head.wid * 0.9f, chestH * 0.8f, 0.14f), body);   // un pecho al que se unen los brazos
        // La cabeza (en la araña, que no la tiene separada, se le hace una sobre el pecho) y su CARA en el frente:
        // ojos (con pupila y brillo), cejas y boca, siempre DENTRO de la cabeza.
        Transform hc = null; foreach (Transform t in neck) if (t.name == head.name && t.GetComponent<Renderer>() != null) hc = t;
        if (hc == null) { hc = Part(neck, "Cabeza", new Vector3(0f, 0.08f, 0.06f), new Vector3(0.22f, 0.18f, 0.18f), body); if (!sp.spider) hc.name = "Cabeza_humana"; }
        Vector3 hf = hc.localPosition + new Vector3(0f, 0f, hc.localScale.z * 0.5f + 0.002f);
        float hw = hc.localScale.x, hh = hc.localScale.y;
        float ew = hw * 0.28f, eh = hh * 0.3f;
        // Con OJOS EN PEDÚNCULO (cangrejo) esos son sus ojos: suben a lo alto de la cabeza, y la cara lleva cejas y boca.
        bool stalks = sp.fx != null && sp.fx.eyeStalks > 0f;
        if (stalks)
            foreach (Transform t in neck)
                if (t.name == "Antena" && t.Find("Pedunculo") != null)
                    t.localPosition = hc.localPosition + new Vector3(Mathf.Sign(t.localPosition.x) * hw * 0.3f, hh * 0.45f, hc.localScale.z * 0.15f);
        for (int s = -1; s <= 1 && !stalks; s += 2)
        {
            Part(neck, "Ojo", hf + new Vector3(s * hw * 0.24f, hh * 0.12f, 0f), new Vector3(ew, eh, 0.006f), Color.white);
            Part(neck, "Pupila", hf + new Vector3(s * hw * 0.24f, hh * 0.1f, 0.004f), new Vector3(ew * 0.5f, eh * 0.55f, 0.005f), new Color(0.08f, 0.06f, 0.05f));
            Part(neck, "Brillo", hf + new Vector3(s * hw * 0.21f, hh * 0.16f, 0.007f), new Vector3(ew * 0.18f, eh * 0.18f, 0.004f), Color.white);
            Part(neck, "Ceja", hf + new Vector3(s * hw * 0.24f, hh * 0.32f, 0.003f), new Vector3(ew * 1.1f, hh * 0.05f, 0.005f), dark);
        }
        if (stalks) for (int s = -1; s <= 1; s += 2) Part(neck, "Ceja", hf + new Vector3(s * hw * 0.24f, hh * 0.2f, 0.003f), new Vector3(ew * 1.1f, hh * 0.05f, 0.005f), dark);
        Part(neck, "Boca", hf + new Vector3(0f, -hh * 0.22f, 0.003f), new Vector3(hw * 0.3f, hh * 0.05f, 0.005f), new Color(0.25f, 0.1f, 0.1f));
        // COLUMNA HUMANA: un cubo más entre la cintura y el tórax, para que al enderezarse quede erguido de verdad (y no
        // agachado): la cintura es la articulación que se levanta; el tórax, la cabeza y los brazos van encima.
        float spine = 0.22f;
        var waist = new GameObject("Cintura").transform; waist.SetParent(pivot, false); waist.localPosition = torso.localPosition;
        torso.SetParent(waist, true); torso.localPosition = new Vector3(0f, 0f, spine);
        // En la mantis la cintura va DETRÁS del tórax (entre el 2.º y el 3.er par): la columna llega hasta él, sin hueco.
        float lead = sp.armPairs >= 2 && sp.body.Length >= 3 ? Mathf.Max(0f, segZ[sp.body[1].name].z - segZ[sp.body[1].name].len * 0.5f - pivotZ) : 0f;
        Part(waist, "Columna", new Vector3(0f, 0f, (spine + lead) * 0.5f), new Vector3(Mathf.Max(0.06f, head.wid * 0.45f), Mathf.Max(0.06f, chestH * 0.6f), spine + lead), body);
        // Manos en las patas delanteras (al final del tarso).
        foreach (Leg l in ci._legs)
            if (l.pair < sp.armPairs)
            {
                Transform tip = null; foreach (Transform t in l.coxa.GetComponentsInChildren<Transform>()) if (t.name == "Punta") tip = t;
                if (tip != null) Part(tip, "Mano", Vector3.zero, new Vector3(sp.legWidth * 2.2f, sp.legWidth * 1.4f, sp.legWidth * 2.2f), dark);
                // Pose de BRAZO (en el marco del torso erguido): colgando junto al cuerpo, con el codo doblado hacia delante.
                // El 2.º par de brazos (mantis) cuelga más abierto y con el codo menos doblado, para no chocar con el primero.
                l.arm = true;
                l.armCoxa = Quaternion.Euler(0f, l.side * (l.pair == 0 ? 168f : 145f), 0f);
                l.armTroch = Quaternion.Euler(l.pair == 0 ? -10f : -25f, 0f, 0f);
                l.tibia = FindDeep(l.coxa, "Tibia_art"); if (l.tibia != null) { l.tibiaRest = l.tibia.localRotation; l.armTibia = Quaternion.Euler(l.pair == 0 ? -70f : -40f, 0f, 0f); }
            }
        ci._torso = waist; ci._neck = neck; ci._torsoRest = waist.localRotation; ci._neckRest = neck.localRotation;
        ci._pivot = pivot; ci._curlMode = sp.fx != null ? sp.fx.curl : 0;
        return ci;
    }

    static Leg BuildLeg(Transform pivot, InsectMorphology.Spec sp, int p, int s, Vector3 hip, Color c, List<Transform> tips)
    {
        float w = sp.legWidth;
        float[] L = sp.legs[p];
        bool raptor = sp.raptorial && p == 0, jump = sp.jumper && p == sp.legs.Length - 1;
        // Reposo de la coxa (pro/retracción): P1 +30°, P2 0°, P3 −30° (arañas: +45, +15, −15, −45).
        // (con otro número de pares, repartidos igual: los 7 de la cochinilla, los 3 de la larva del ácaro)
        int np = sp.legs.Length; float edge = sp.spider ? 45f : 30f;
        float restYaw = np <= 1 ? 0f : Mathf.Lerp(edge, -edge, p / (float)(np - 1));
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

    // ══ ROPA Y HERIDAS (sobre el cuerpo centauro: siguen al torso y a la cabeza cuando se enderezan o se agachan) ══════
    public enum Hat { None, Brimmed, Cap, Bonnet, Spike, Turban }

    // La cabeza («Cuello» la lleva) y el torso, y la caja de la cabeza en el marco del cuello.
    static bool Anchors(GameObject host, out Transform torso, out Transform neck, out Bounds head)
    {
        torso = FindDeep(host.transform, "Torso"); neck = FindDeep(host.transform, "Cuello"); head = default;
        if (torso == null || neck == null) return false;
        Transform h = null; foreach (Transform t in neck) if (t.name == "Cabeza" && t.GetComponent<Renderer>() != null) h = t;
        if (h == null) foreach (Transform t in neck) if (t.GetComponent<Renderer>() != null) { h = t; break; }
        head = h != null ? new Bounds(h.localPosition, h.localScale) : new Bounds(Vector3.zero, Vector3.one * 0.2f);
        return true;
    }

    /// <summary>Ropa GRIEGA (Egina): quitón sobre el torso con su cinturón, corona de encina y barba.</summary>
    public static void WearGreek(GameObject host, Color chiton, bool wreath, bool beard)
    {
        if (!Anchors(host, out Transform torso, out Transform neck, out Bounds head)) return;
        Color belt = new Color(0.55f, 0.4f, 0.2f);
        Part(torso, "Quiton", new Vector3(0f, 0f, 0.08f), new Vector3(head.size.x * 1.25f, head.size.y * 1.4f, 0.24f), chiton);
        Part(torso, "Quiton_falda", new Vector3(0f, 0f, -0.06f), new Vector3(head.size.x * 1.4f, head.size.y * 1.6f, 0.1f), chiton);
        Part(torso, "Cinturon", new Vector3(0f, 0f, 0.02f), new Vector3(head.size.x * 1.3f, head.size.y * 1.45f, 0.03f), belt);
        if (wreath)
            for (int k = 0; k < 8; k++)
            {
                float a = k * Mathf.PI / 4f;
                Part(neck, "Corona_encina", head.center + new Vector3(Mathf.Cos(a) * head.extents.x * 1.05f, head.extents.y * 0.85f, Mathf.Sin(a) * head.extents.z * 1.05f), new Vector3(head.size.x * 0.22f, head.size.y * 0.16f, head.size.x * 0.22f), new Color(0.25f, 0.42f, 0.2f));
            }
        if (beard) Part(neck, "Barba", head.center + new Vector3(0f, -head.extents.y * 0.75f, head.extents.z * 0.9f), new Vector3(head.size.x * 0.7f, head.size.y * 0.55f, head.size.z * 0.3f), new Color(0.92f, 0.92f, 0.9f));
    }

    /// <summary>Ropa de los AÑOS 40 (Le Chambon, Irena Sendler): abrigo sobre el torso, bufanda y sombrero de ala o gorra.</summary>
    public static void Wear1940s(GameObject host, Color coat, Hat hat)
    {
        if (!Anchors(host, out Transform torso, out Transform neck, out Bounds head)) return;
        Color felt = new Color(0.22f, 0.2f, 0.18f), wool = new Color(0.55f, 0.18f, 0.16f);
        Part(torso, "Abrigo", new Vector3(0f, 0f, 0.06f), new Vector3(head.size.x * 1.3f, head.size.y * 1.5f, 0.28f), coat);
        for (int k = 0; k < 3; k++) Part(torso, "Boton", new Vector3(0f, head.size.y * 0.76f, 0.02f + k * 0.07f), new Vector3(0.02f, 0.01f, 0.02f), new Color(0.15f, 0.12f, 0.1f));
        Part(neck, "Bufanda", head.center + new Vector3(0f, -head.extents.y * 1.1f, -head.extents.z * 0.4f), new Vector3(head.size.x * 1.1f, head.size.y * 0.3f, head.size.z * 1.0f), wool);
        WearHat(host, hat);
    }

    /// <summary>Solo el tocado (sobre cualquier ropa: quitón, abrigo…).</summary>
    public static void WearHat(GameObject host, Hat hat)
    {
        if (!Anchors(host, out _, out Transform neck, out Bounds head)) return;
        Color felt = new Color(0.22f, 0.2f, 0.18f);
        if (hat == Hat.Brimmed)
        {
            Part(neck, "Sombrero_ala", head.center + Vector3.up * head.extents.y * 1.05f, new Vector3(head.size.x * 1.7f, head.size.y * 0.12f, head.size.z * 1.7f), felt);
            Part(neck, "Sombrero_copa", head.center + Vector3.up * head.extents.y * 1.45f, new Vector3(head.size.x * 1.0f, head.size.y * 0.55f, head.size.z * 1.0f), felt);
        }
        else if (hat == Hat.Cap)
        {
            Part(neck, "Gorra", head.center + Vector3.up * head.extents.y * 1.1f, new Vector3(head.size.x * 1.1f, head.size.y * 0.3f, head.size.z * 1.05f), felt);
            Part(neck, "Visera", head.center + new Vector3(0f, head.extents.y * 0.95f, head.extents.z * 1.3f), new Vector3(head.size.x * 0.9f, head.size.y * 0.06f, head.size.z * 0.5f), felt);
        }
        else if (hat == Hat.Bonnet)   // cofia blanca de enfermera (Escútari, 1854)
            Part(neck, "Cofia", head.center + new Vector3(0f, head.extents.y * 0.9f, -head.extents.z * 0.2f), new Vector3(head.size.x * 1.15f, head.size.y * 0.4f, head.size.z * 1.1f), new Color(0.96f, 0.96f, 0.94f));
        else if (hat == Hat.Spike)    // casco de pincho alemán de 1914 (Pickelhaube), con su funda de tela
        {
            Part(neck, "Casco", head.center + Vector3.up * head.extents.y * 1.05f, new Vector3(head.size.x * 1.15f, head.size.y * 0.45f, head.size.z * 1.2f), new Color(0.45f, 0.45f, 0.38f));
            Part(neck, "Pincho", head.center + Vector3.up * head.extents.y * 1.6f, new Vector3(head.size.x * 0.12f, head.size.y * 0.35f, head.size.x * 0.12f), new Color(0.45f, 0.45f, 0.38f));
        }
        else if (hat == Hat.Turban)   // tela enrollada (India antigua)
            Part(neck, "Turbante", head.center + Vector3.up * head.extents.y * 1.0f, new Vector3(head.size.x * 1.2f, head.size.y * 0.5f, head.size.z * 1.2f), new Color(0.95f, 0.9f, 0.8f));
    }

    /// <summary>
    /// HERIDAS (BodyInjuries): sin antebrazo derecho (falta desde el codo de la pata/brazo delantero derecho), el ojo
    /// izquierdo cerrado (un párpado en lugar del ojo) y la cojera (una pata trasera pisa menos y el cuerpo cabecea).
    /// </summary>
    public static void DrawInjuries(GameObject host, BodyInjuries inj)
    {
        if (inj == null) return;
        CubeInsect ci = host.GetComponent<CubeInsect>();
        if (inj.missingRightForearm)
            foreach (Transform t in host.GetComponentsInChildren<Transform>(true))
                if (t.name == "Pata_0_D") { Transform tib = FindDeep(t, "Tibia_art"); if (tib != null) tib.gameObject.SetActive(false); Transform mu = FindDeep(t, "Femur"); if (mu != null) Part(mu.parent, "Munon", mu.localPosition + Vector3.forward * mu.localScale.z * 0.5f, new Vector3(mu.localScale.x * 1.3f, mu.localScale.y * 1.3f, mu.localScale.x * 1.3f), new Color(0.7f, 0.55f, 0.5f)); }
        if (inj.blindLeftEye)
        {
            Transform neck = FindDeep(host.transform, "Cuello");
            if (neck != null)
            {
                Transform eye = null, pupil = null, shine = null;
                foreach (Transform t in neck) { if (t.localPosition.x >= 0f) continue; if (t.name == "Ojo") eye = t; if (t.name == "Pupila") pupil = t; if (t.name == "Brillo") shine = t; }
                if (pupil != null) pupil.gameObject.SetActive(false);
                if (shine != null) shine.gameObject.SetActive(false);
                if (eye != null) { eye.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.45f, 0.3f, 0.28f)); eye.localScale = new Vector3(eye.localScale.x, eye.localScale.y * 0.25f, eye.localScale.z); }   // párpado cerrado
            }
        }
        if (ci != null) ci._limp = inj.limp;
    }
    bool _limp, _injuriesDrawn;

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
                // Es BRAZO si cuelga del torso (las patas de andar cuelgan del pivote): el 1.er par, y el 2.º en la mantis.
                lg.arm = t.parent != null && t.parent.name != "Pivote";
                if (lg.arm) { lg.armCoxa = Quaternion.Euler(0f, lg.side * (lg.pair == 0 ? 168f : 145f), 0f); lg.armTroch = Quaternion.Euler(lg.pair == 0 ? -10f : -25f, 0f, 0f); lg.tibia = FindDeep(t, "Tibia_art"); if (lg.tibia != null) { lg.tibiaRest = lg.tibia.localRotation; lg.armTibia = Quaternion.Euler(lg.pair == 0 ? -70f : -40f, 0f, 0f); } }
                _legs.Add(lg);
            }
            else if (t.name.StartsWith("Pierna_")) _limbs.Add((t, t.name.EndsWith("I") ? -1 : 1, false));
            else if (t.name.StartsWith("Brazo_")) _limbs.Add((t, t.name.EndsWith("I") ? -1 : 1, true));
            else if (t.name == "Mandibula" && t.childCount > 0) _jaws.Add((t, Mathf.Abs(Mathf.DeltaAngle(0f, t.localEulerAngles.y)), t.localPosition.x < 0f ? -1 : 1));
            else if (t.name.StartsWith("Ala_")) _wings.Add(t);
            else if (t.name.StartsWith("Pinza_")) _legs.Insert(0, PincerLeg(t));   // las pinzas (escorpión, cangrejo) y los pedipalpos del solífugo: brazos
        }
        _pivot = root.Find("Pivote"); _curlMode = CurlModeOf(root);
        _spider = root.GetComponentsInChildren<Transform>(true).Length > 0 && root.Find("Pivote/Queliceros") != null;
        _torso = FindDeep(root, "Cintura") ?? FindDeep(root, "Torso"); _neck = FindDeep(root, "Cuello"); _torsoRest = Quaternion.identity; _neckRest = Quaternion.identity;
        _last = transform.position;
        if (_seed < 0f) _seed = Random.Range(0f, 100f);
    }

    /// <summary>Fija la postura YA (también en el editor, sin esperar a Update): erguido (centauro, con brazos) o animal
    /// (a todas las patas). La deja fijada para el Play (<see cref="agitation"/>). Lo usa la galería de insectos-humanos.</summary>
    public void PoseNow(bool upright)
    {
        if (_legs.Count == 0) Awake();
        agitation = upright ? 0f : 1f; _calm = upright ? 1f : 0f;
        if (_torso != null) _torso.localRotation = _torsoRest * Quaternion.Euler(-88f * _calm, 0f, 0f);
        if (_neck != null) _neck.localRotation = _neckRest * Quaternion.Euler(88f * _calm, 0f, 0f);
        foreach (Leg l in _legs)
        {
            if (l.coxa == null || !l.arm) continue;
            l.coxa.localRotation = Quaternion.Slerp(l.coxaRest, l.armCoxa, _calm);
            if (l.troch != null) l.troch.localRotation = Quaternion.Slerp(l.trochRest, l.armTroch, _calm);
            if (l.tibia != null) l.tibia.localRotation = Quaternion.Slerp(l.tibiaRest, l.armTibia, _calm);
        }
    }

    void Update()
    {
        // HERIDAS (BodyInjuries): se dibujan en cuanto se aplican (sin antebrazo, ojo cerrado, cojera).
        if (!_injuriesDrawn && Time.time >= _nextPhaseCheck) { BodyInjuries inj = GetComponent<BodyInjuries>(); if (inj == null) _injuriesDrawn = true; else if (inj.Applied) { DrawInjuries(gameObject, inj); _injuriesDrawn = true; } }
        // METAMORFOSIS: si cambia de fase (larva → pupa → adulto, ninfa → adulto), se redibuja.
        if (!string.IsNullOrEmpty(species) && Time.time >= _nextPhaseCheck)
        {
            _nextPhaseCheck = Time.time + 1f;
            Animal an = GetComponent<Animal>();
            if (an != null)
            {
                Metamorphosis.Phase ph = Metamorphosis.PhaseOf(an);
                bool fem = an.sex != Sex.male;   // macho o hembra según el animal (dimorfismo sexual)
                if (ph != _shown || fem != female) { Dress(gameObject, species, tint, lengthL, ph, fem); Awake(); }
            }
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
        if (_torso != null) _torso.localRotation = _torsoRest * Quaternion.Euler(-88f * _calm, 0f, 0f);
        if (_neck != null) _neck.localRotation = _neckRest * Quaternion.Euler(88f * _calm, 0f, 0f);
        // DEFENSA PASIVA (curl, lo fija la conducta): la cochinilla se hace bola, el caracol se mete en la concha…
        // la conducta decide cuándo: acorazado ante un peligro (PassiveDefence) o aletargado (el caracol sellado en invierno)
        if (_curlMode != 0) curl = PassiveDefence.IsHolding(this) || Dormant.IsResting(this) ? 1f : 0f;
        CurlTick();
        // Marcha: TRÍPODE alterno (P1 izq + P2 der + P3 izq, y luego las otras) o TETRÁPODO en arañas; erguido, sin las delanteras.
        foreach (Leg l in _legs)
        {
            if (l.coxa == null) continue;
            int group = (l.pair % 2 == 0) == (l.side < 0) ? 0 : 1;
            float ph = Mathf.Sin(_phase + group * Mathf.PI);
            float la = _limp && l.side > 0 && l.pair == _legs[_legs.Count - 1].pair ? 0.3f : 1f;   // cojera: la trasera derecha apenas da el paso
            Quaternion cx = l.coxaRest * Quaternion.Euler(0f, ph * amp * la * l.side, 0f);
            Quaternion tr = l.trochRest * Quaternion.Euler(-Mathf.Max(0f, ph) * amp * 0.6f, 0f, 0f);   // se alza en la fase de vuelo
            if (l.arm && _torso != null)   // brazos cuando está erguido (balanceo suave al andar)
            {
                Quaternion armSwing = Quaternion.Euler(Mathf.Sin(_phase + (l.side < 0 ? 0f : Mathf.PI)) * amp * 0.4f, 0f, 0f);
                cx = Quaternion.Slerp(cx, l.armCoxa * armSwing, _calm);
                tr = Quaternion.Slerp(tr, l.armTroch, _calm);
                // EMPUJAR (Berón 2026-10-29: son humanoides, empujan con brazos y manos): brazos adelante a la altura del pecho y el
                // codo se estira con cada empujón.
                if (Time.time < _pushUntil && l.pair == 0) { tr = l.armTroch * Quaternion.Euler(70f + 15f * _push, 0f, 0f);   /* (+: hacia delante; con − los brazos iban hacia atrás) */ if (l.tibia != null) l.tibia.localRotation = Quaternion.Slerp(l.armTibia, l.tibiaRest, 0.35f + 0.6f * _push); l.coxa.localRotation = l.armCoxa; if (l.troch != null) l.troch.localRotation = tr; continue; }
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
