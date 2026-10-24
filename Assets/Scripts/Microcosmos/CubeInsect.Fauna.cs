using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PIEZAS DE LA FAUNA DE CADA ERA (InsectMorphology.Fauna.cs; fuentes en docs/microcosmos-fauna-eras.md), sobre el
/// mismo cuerpo de cubos articulado y centauro de siempre:
/// - escorpión: la cola de 5 segmentos con el telson y el aguijón, curvada sobre el dorso; las PINZAS son sus brazos
///   al erguirse; los peines bajo el vientre;
/// - solífugo: los quelíceros enormes (y el flagelo del macho) y los pedipalpos largos con ventosa, que son sus brazos;
/// - cangrejo fantasma: pinzas desiguales (sus brazos), ojos en pedúnculo y el abdomen plegado (la carrera de lado es de la conducta);
/// - cochinilla: urópodos y el marsupio de la hembra; caracol: concha en espiral con bandas y labio, tentáculos con
///   ojos; babosa: manto con pneumostoma y pie rugoso; lombriz: anillos y clitelo;
/// - cigarra: ojos a los lados, alas transparentes con manchas, timbales del macho, ninfa con patas excavadoras;
///   luciérnaga: la linterna que brilla; tijereta: fórceps; cucaracha: cercos; Boreus: el pico y las alas en cerdas.
/// Fases propias: la larva ZOEA del cangrejo (en el mar), la megalopa, la ninfa de cigarra, la manca, la larva de 6 patas
/// del ácaro, el caracol sin labio, la lombriz sin clitelo, la larva luminosa de la luciérnaga.
/// </summary>
public partial class CubeInsect
{
    Transform _pivot;
    /// <summary>DEFENSA PASIVA (0 = normal · 1 = del todo): la cochinilla se hace BOLA (Armadillidium), el caracol se METE
    /// en la concha, la babosa y la lombriz se ENCOGEN. Lo fija la conducta (PassiveDefence) mientras dura el peligro.</summary>
    public float curl;
    int _curlMode; float _curlShown;   // 1 bola · 2 concha · 3 encogerse (InsectMorphology.FaunaTraits.curl)

    // ══ FASES ══════════════════════════════════════════════════════════════════════════════════════════════════
    static CubeInsect FaunaStage(GameObject host, InsectMorphology.Spec sp, Color body, float length, Metamorphosis.Phase phase, bool female)
    {
        if (sp == null || sp.fx == null) return null;
        if (phase == Metamorphosis.Phase.Larva && sp.fx.zoea) return Zoea(host, body, length);
        InsectMorphology.Spec alt = InsectMorphology.StageSpec(sp, phase, female);
        if (alt != null) return Build(host, InsectMorphology.ForSex(alt, female), body, length);
        if (phase == Metamorphosis.Phase.Larva && sp.fx.larvaLegs)
        {
            // Larva con patitas (luciérnaga: cazadora de caracoles, aplanada y con placas; Boreus: en el musgo). La de la
            // luciérnaga BRILLA: dos puntos de luz en el penúltimo segmento.
            CubeInsect ci = Grub(host, sp, body, length, true);
            if (sp.fx.lantern > 0)
            {
                Transform root = host.transform.Find("Insecto");
                for (int s = -1; s <= 1; s += 2) Glow(Part(root, "Linterna_larva", new Vector3(s * 0.04f, 0.1f, -0.35f), new Vector3(0.05f, 0.05f, 0.05f), LanternLight));
            }
            return ci;
        }
        return null;
    }

    // ZOEA: la larva del cangrejo, en el plancton (5 estadios): caparazón globoso, ojos enormes, una espina dorsal larga
    // y otra rostral, el abdomen segmentado doblado bajo el cuerpo con el telson en horquilla y los maxilípedos nadadores.
    static CubeInsect Zoea(GameObject host, Color body, float length)
    {
        Transform root = NewRoot(host, length, out CubeInsect ci);
        Color pale = Color.Lerp(body, new Color(0.92f, 0.9f, 0.85f), 0.55f), eye = new Color(0.08f, 0.08f, 0.1f), dot = new Color(0.75f, 0.25f, 0.15f);
        Part(root, "Caparazon_zoea", new Vector3(0f, 0.32f, 0.05f), new Vector3(0.34f, 0.32f, 0.36f), pale);
        for (int s = -1; s <= 1; s += 2) Part(root, "Ojo_zoea", new Vector3(s * 0.16f, 0.36f, 0.2f), new Vector3(0.12f, 0.13f, 0.13f), eye);
        Part(root, "Espina_dorsal", new Vector3(0f, 0.68f, -0.02f), new Vector3(0.035f, 0.42f, 0.035f), pale, -12f, 0f);
        Part(root, "Espina_rostral", new Vector3(0f, 0.16f, 0.24f), new Vector3(0.03f, 0.03f, 0.3f), pale, 55f, 0f);
        for (int k = 0; k < 5; k++)   // el abdomen: hacia atrás y luego hacia abajo y delante, bajo el caparazón
        {
            float a = k * 0.5f;
            Part(root, "Abdomen_zoea", new Vector3(0f, 0.22f - Mathf.Sin(a) * 0.12f, -0.16f - Mathf.Cos(a) * 0.08f + k * 0.01f), new Vector3(0.09f, 0.07f, 0.08f), pale, k * 28f, 0f);
        }
        for (int s = -1; s <= 1; s += 2)
        {
            Part(root, "Telson_horquilla", new Vector3(s * 0.04f, 0.02f, -0.05f), new Vector3(0.02f, 0.02f, 0.12f), pale, 0f, s * 25f);
            Part(root, "Maxilipedo", new Vector3(s * 0.15f, 0.2f, 0.08f), new Vector3(0.18f, 0.02f, 0.03f), pale, 0f, s * 20f);
        }
        Part(root, "Cromatoforo", new Vector3(0f, 0.48f, 0.05f), new Vector3(0.06f, 0.01f, 0.06f), dot);
        return ci;
    }

    // ══ COLOR DE LOS SEGMENTOS ═════════════════════════════════════════════════════════════════════════════════
    static Color SegColor(InsectMorphology.Spec sp, string seg, Color body, Color dark)
    {
        if (seg == "Peciolo") return dark;
        var fx = sp.fx; if (fx == null) return body;
        if (seg == "Clitelo") return Color.Lerp(body, new Color(0.9f, 0.55f, 0.45f), 0.55f);   // la «silla» de la lombriz adulta
        if (fx.accent.a > 0f && (fx.shell > 0f || seg == "Pronoto")) return fx.accent;          // el cuerpo del caracol (la concha es el tinte) · el pronoto rosado de la luciérnaga
        // Placas y anillos alternos un poco más oscuros (terguitos, pereion, anillos): se lee la segmentación.
        char last = seg[seg.Length - 1];
        if (char.IsDigit(last) && (last - '0') % 2 == 1) { Color c = body * 0.88f; c.a = 1f; return c; }
        return body;
    }

    static readonly Color LanternLight = new Color(0.85f, 1f, 0.35f);
    static readonly Dictionary<Color, Material> _glow = new Dictionary<Color, Material>();

    // Material que BRILLA (emisivo): la linterna de la luciérnaga.
    static Transform Glow(Transform t)
    {
        Renderer r = t.GetComponent<Renderer>();
        Color c = r.sharedMaterial.color;
        if (!_glow.TryGetValue(c, out Material m) || m == null)
        {
            m = new Material(r.sharedMaterial) { name = "Linterna_" + ColorUtility.ToHtmlStringRGB(c) };
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * 2.5f);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            _glow[c] = m;
        }
        r.sharedMaterial = m;
        return t;
    }

    // ══ PIEZAS ═════════════════════════════════════════════════════════════════════════════════════════════════
    static void FaunaParts(CubeInsect ci, Transform pivot, InsectMorphology.Spec sp, Dictionary<string, (float z, float len, float wid, float hei)> segZ, float y0, float total, Color body, Color dark)
    {
        var fx = sp.fx;
        float front = total * 0.5f, rear = -total * 0.5f;
        var head = segZ[sp.body[0].name];
        var tail = segZ[sp.body[sp.body.Length - 1].name];
        Color pale = Color.Lerp(body, new Color(0.95f, 0.92f, 0.85f), 0.55f);

        // ESCORPIÓN: la cola (metasoma de 5 segmentos, cada uno algo más largo) curvada hacia arriba y adelante, el
        // telson con la vesícula del veneno y el aguijón.
        if (fx.tailLen > 0f)
        {
            float[] f = { 0.15f, 0.16f, 0.17f, 0.18f, 0.2f };
            var t = new GameObject("Cola").transform; t.SetParent(pivot, false);
            t.localPosition = new Vector3(0f, y0 + tail.hei * 0.6f, rear + 0.01f); t.localRotation = Quaternion.Euler(-25f, 180f, 0f);
            Transform j = t; float at = 0f;
            for (int k = 0; k < 5; k++)
            {
                float l = fx.tailLen * f[k], w = fx.tailWidth * (1f - 0.04f * k);
                j = Joint(j, "Metasoma", at, k == 0 ? 0f : -33f);
                Part(j, "Metasoma", new Vector3(0f, 0f, l * 0.5f), new Vector3(w, w * 0.9f, l), k % 2 == 0 ? body : body * 0.9f + Color.black * 0.1f);
                at = l;
            }
            j = Joint(j, "Telson", at, -30f);
            float tl = fx.tailLen * 0.14f;
            Part(j, "Telson", new Vector3(0f, 0f, tl * 0.4f), new Vector3(fx.tailWidth * 1.25f, fx.tailWidth * 1.1f, tl * 0.75f), body);
            Part(j, "Aguijon", new Vector3(0f, -fx.tailWidth * 0.3f, tl * 0.85f), new Vector3(fx.tailWidth * 0.3f, fx.tailWidth * 0.3f, tl * 0.4f), dark, 35f, 0f);
        }
        if (fx.pectines > 0f)   // los peines (órganos sensoriales del suelo), tras el último par de patas
            for (int s = -1; s <= 1; s += 2) Part(pivot, "Peine", new Vector3(s * 0.035f, y0 - 0.004f, head.z - head.len * 0.5f - 0.02f), new Vector3(fx.pectines, 0.008f, 0.015f), pale, 0f, s * 55f);

        // PINZAS (escorpión, cangrejo) y PEDIPALPOS CON VENTOSA (solífugo): brazos al erguirse, delante al andar.
        if (fx.chelae > 0f || fx.palpLegs > 0f)
            for (int s = -1; s <= 1; s += 2)
            {
                float k = fx.chelae > 0f && s > 0 ? fx.chelaAsym : 1f;   // la pinza derecha, mayor (cangrejo)
                var root = new GameObject(s < 0 ? "Pinza_I" : "Pinza_D").transform; root.SetParent(pivot, false);
                root.localPosition = new Vector3(s * head.wid * 0.35f, y0 + head.hei * 0.4f, front - 0.02f);
                root.localRotation = Quaternion.Euler(-8f, s * (fx.palpLegs > 0f ? 15f : 30f), 0f);
                if (fx.chelae > 0f)
                {
                    float c = fx.chelae * k, w = fx.chelaWidth * k;
                    Part(root, "Femur_pinza", new Vector3(0f, 0f, c * 0.15f), new Vector3(w * 0.6f, w * 0.6f, c * 0.3f), body);
                    Transform el = Joint(root, "Tibia", c * 0.3f, 10f);
                    el.localRotation = Quaternion.Euler(10f, -s * 55f, 0f);   // el «codo» se dobla hacia dentro, por delante de la boca
                    Part(el, "Patela_pinza", new Vector3(0f, 0f, c * 0.14f), new Vector3(w * 0.7f, w * 0.7f, c * 0.28f), body);
                    Transform hand = Joint(el, "Mano", c * 0.28f, 0f);
                    hand.localRotation = Quaternion.Euler(0f, s * 35f, 0f);
                    // La mano (palma) es lo que se ve de lejos: grande y, en el cangrejo fantasma, clara (crema) con los dedos.
                    Color palm = fx.crustacean ? Color.Lerp(body, new Color(0.96f, 0.93f, 0.86f), 0.55f) : body;
                    Part(hand, "Mano_pinza", new Vector3(0f, 0f, c * 0.14f), new Vector3(w * 2.4f, w * 1.9f, c * 0.32f), palm);
                    Part(hand, "Dedo_fijo", new Vector3(s * w * 0.6f, 0f, c * 0.42f), new Vector3(w * 0.6f, w * 0.7f, c * 0.3f), palm * 0.8f + dark * 0.2f);
                    Part(hand, "Dedo_movil", new Vector3(-s * w * 0.6f, w * 0.1f, c * 0.42f), new Vector3(w * 0.6f, w * 0.7f, c * 0.3f), dark, 0f, s * 14f);
                }
                else
                {
                    float c = fx.palpLegs, w = sp.legWidth * 1.2f;
                    Part(root, "Femur_palpo", new Vector3(0f, 0f, c * 0.18f), new Vector3(w, w, c * 0.36f), body);
                    Transform ti = Joint(root, "Tibia", c * 0.36f, 25f);
                    Part(ti, "Tibia_palpo", new Vector3(0f, 0f, c * 0.18f), new Vector3(w * 0.9f, w * 0.9f, c * 0.36f), body);
                    Transform ta = Joint(ti, "Tarso", c * 0.36f, 15f);
                    Part(ta, "Tarso_palpo", new Vector3(0f, 0f, c * 0.13f), new Vector3(w * 0.8f, w * 0.8f, c * 0.26f), body);
                    Part(ta, "Ventosa", new Vector3(0f, 0f, c * 0.27f), new Vector3(w * 1.6f, w * 1.6f, w * 0.8f), pale);   // el órgano adhesivo
                }
                ci._legs.Insert(0, PincerLeg(root));
            }

        // SOLÍFUGO: los quelíceros como tenazas de dos piezas, hacia delante; el macho con el flagelo encima.
        if (fx.bigChelicerae > 0f)
            for (int s = -1; s <= 1; s += 2)
            {
                float q = fx.bigChelicerae;
                var r = new GameObject("Queliceros").transform; r.SetParent(pivot, false);
                r.localPosition = new Vector3(s * head.wid * 0.22f, y0 + head.hei * 0.55f, front - 0.01f);
                Part(r, "Quelicero", new Vector3(0f, 0f, q * 0.5f), new Vector3(head.wid * 0.38f, head.hei * 0.6f, q), body);
                Part(r, "Diente_superior", new Vector3(0f, head.hei * 0.15f, q + 0.02f), new Vector3(head.wid * 0.12f, head.hei * 0.2f, q * 0.25f), dark, 20f, 0f);
                Part(r, "Diente_inferior", new Vector3(0f, -head.hei * 0.2f, q + 0.01f), new Vector3(head.wid * 0.12f, head.hei * 0.2f, q * 0.22f), dark, -20f, 0f);
                if (fx.flagellum) Part(r, "Flagelo", new Vector3(0f, head.hei * 0.45f, q * 0.6f), new Vector3(0.012f, 0.012f, q * 0.6f), dark, -35f, 0f);
            }

        // CANGREJO: ojos en pedúnculos (club) y el abdomen plegado bajo el caparazón.
        if (fx.eyeStalks > 0f)
            for (int s = -1; s <= 1; s += 2)
            {
                var r = new GameObject("Antena").transform; r.SetParent(pivot, false);   // («Antena»: va con la cabeza al erguirse)
                r.localPosition = new Vector3(s * head.wid * 0.32f, y0 + head.hei, front - 0.06f);
                // Ocypode: el ojo es una MAZA negra que ocupa media pedúnculo (y en el macho sigue un «estilo» por encima).
                Part(r, "Pedunculo", new Vector3(0f, fx.eyeStalks * 0.5f, 0f), new Vector3(0.06f, fx.eyeStalks, 0.06f), body);
                Part(r, "Cornea", new Vector3(0f, fx.eyeStalks * 0.78f, 0.01f), new Vector3(0.12f, fx.eyeStalks * 0.5f, 0.12f), new Color(0.06f, 0.06f, 0.08f));
                Part(r, "Brillo_cornea", new Vector3(0f, fx.eyeStalks * 0.88f, 0.072f), new Vector3(0.035f, 0.035f, 0.005f), Color.white);
            }
        if (fx.apron > 0f) Part(pivot, "Abdomen_plegado", new Vector3(0f, y0 - 0.006f, head.z - head.len * 0.1f), new Vector3(head.wid * fx.apron, 0.012f, head.len * 0.55f), pale);

        // COCHINILLA: los urópodos (dos puntas en la cola) y el marsupio de la hembra (bolsa clara bajo el vientre).
        if (fx.uropods > 0f)
            for (int s = -1; s <= 1; s += 2) Part(pivot, "Uropodo", new Vector3(s * 0.06f, y0 + 0.02f, rear - fx.uropods * 0.35f), new Vector3(0.025f, 0.02f, fx.uropods), dark, 0f, s * 15f);
        if (fx.broodPouch) Part(pivot, "Marsupio", new Vector3(0f, y0 - 0.015f, front - total * 0.38f), new Vector3(0.38f, 0.04f, total * 0.32f), Color.Lerp(pale, Color.white, 0.4f));

        // CARACOL: la concha en espiral (vueltas que se estrechan, giradas), con sus bandas y, en el adulto, el labio.
        if (fx.shell > 0f && segZ.TryGetValue("Pie", out var pie))
        {
            float sh = fx.shell;
            var shell = new GameObject("Concha").transform; shell.SetParent(pivot, false);
            shell.localPosition = new Vector3(0f, y0 + pie.hei + sh * 0.42f, pie.z - pie.len * 0.12f);
            Color band = body * 0.45f; band.a = 1f;
            // ESPIRAL LOGARÍTMICA vista de lado (enrollada sobre el eje lateral): cada vuelta mide ~0,55 de la anterior.
            // La vuelta del cuerpo (la mayor, con la abertura) queda abajo y delante, sobre el pie; las demás suben por
            // detrás y por encima hasta el ápice, como una cúpula. Antes eran rodajas apiladas en columna.
            int steps = Mathf.Max(4, fx.whorls * 5);
            float turn = 2f * Mathf.PI, endA = fx.whorls * turn * 0.8f;
            Part(shell, "Vuelta", new Vector3(0f, sh * 0.02f, -sh * 0.04f), new Vector3(sh * 0.5f, sh * 0.62f, sh * 0.66f), body);   // el núcleo (rellena el centro)
            for (int k = 0; k <= steps; k++)
            {
                float a = endA * k / steps, f = Mathf.Pow(0.55f, a / turn), r = sh * 0.34f * f;
                Vector3 at = new Vector3(0f, -Mathf.Cos(a) * r, Mathf.Sin(a) * r * -1f);
                float size = sh * 0.46f * f;
                Part(shell, "Vuelta", at, new Vector3(sh * 0.56f * Mathf.Lerp(0.55f, 1f, f), size, size * 1.15f), body, -a * Mathf.Rad2Deg, 0f);
                if (fx.shellBands && f > 0.45f)
                    for (int b = -1; b <= 1; b += 2) Part(shell, "Banda", at + new Vector3(b * sh * 0.2f * Mathf.Lerp(0.55f, 1f, f), 0f, 0f), new Vector3(sh * 0.04f, size * 1.02f, size * 1.17f), band, -a * Mathf.Rad2Deg, 0f);
            }
            float fa = Mathf.Pow(0.55f, endA / turn);
            Part(shell, "Apice", new Vector3(0f, -Mathf.Cos(endA) * sh * 0.34f * fa, -Mathf.Sin(endA) * sh * 0.34f * fa), Vector3.one * sh * 0.1f, band);
            if (fx.shellLip) Part(shell, "Labio", new Vector3(0f, -sh * 0.34f, sh * 0.2f), new Vector3(sh * 0.58f, sh * 0.1f, sh * 0.08f), band, -60f, 0f);   // el peristoma engrosado del adulto
        }
        // TENTÁCULOS: los dos de arriba, largos, con el ojo en la punta; los dos de abajo, cortos (olfato).
        if (fx.tentacles > 0f)
            for (int s = -1; s <= 1; s += 2)
            {
                var r = new GameObject("Antena").transform; r.SetParent(pivot, false);
                r.localPosition = new Vector3(s * head.wid * 0.25f, y0 + head.hei * 0.9f, head.z + head.len * 0.35f);
                r.localRotation = Quaternion.Euler(-50f, s * 20f, 0f);
                Color skin = fx.accent.a > 0f ? fx.accent : body;
                Part(r, "Tentaculo", new Vector3(0f, 0f, fx.tentacles * 0.5f), new Vector3(0.025f, 0.025f, fx.tentacles), skin);
                Part(r, "Ojo_tentaculo", new Vector3(0f, 0f, fx.tentacles), new Vector3(0.04f, 0.04f, 0.04f), new Color(0.12f, 0.1f, 0.1f));
                Part(pivot, "Tentaculo_menor", new Vector3(s * head.wid * 0.3f, y0 + head.hei * 0.35f, head.z + head.len * 0.5f + fx.tentacles * 0.15f), new Vector3(0.02f, 0.02f, fx.tentacles * 0.3f), skin, 10f, s * 25f);
            }
        // BABOSA: el pneumostoma (orificio respiratorio) a la derecha del manto y las arrugas del pie.
        if (fx.mantle && segZ.TryGetValue("Manto", out var manto))
        {
            Part(pivot, "Pneumostoma", new Vector3(manto.wid * 0.5f + 0.003f, y0 + manto.hei * 0.55f, manto.z), new Vector3(0.008f, 0.03f, 0.03f), new Color(0.05f, 0.05f, 0.05f));
            if (segZ.TryGetValue("Pie", out var foot))
                for (int k = -2; k <= 2; k++) Part(pivot, "Arruga", new Vector3(k * foot.wid * 0.18f, y0 + foot.hei + 0.003f, foot.z), new Vector3(0.012f, 0.008f, foot.len * 0.9f), dark);
        }

        // LUCIÉRNAGA: la linterna bajo la punta del abdomen (y asomando por detrás), que brilla.
        for (int k = 0; k < fx.lantern; k++)
            Glow(Part(pivot, "Linterna", new Vector3(0f, y0 + 0.015f, rear + 0.05f + k * 0.075f), new Vector3(tail.wid * 0.7f, 0.03f, 0.07f), LanternLight));

        // TIJERETA (fórceps) y CUCARACHA (cercos): al final del abdomen.
        if (fx.forceps > 0f)
            for (int s = -1; s <= 1; s += 2)
            {
                var r = new GameObject("Forceps").transform; r.SetParent(pivot, false);
                r.localPosition = new Vector3(s * tail.wid * 0.25f, y0 + tail.hei * 0.5f, rear + 0.01f);
                r.localRotation = Quaternion.Euler(0f, 180f - s * (fx.forcepsCurved ? 25f : 4f), 0f);
                if (fx.forcepsCurved)   // macho: ancho en la base, curvo y con dientes
                {
                    Part(r, "Cerco", new Vector3(0f, 0f, fx.forceps * 0.25f), new Vector3(0.04f, 0.025f, fx.forceps * 0.5f), dark);
                    Transform tip = Joint(r, "Cerco", fx.forceps * 0.5f, 0f); tip.localRotation = Quaternion.Euler(0f, s * 60f, 0f);
                    Part(tip, "Cerco", new Vector3(0f, 0f, fx.forceps * 0.25f), new Vector3(0.022f, 0.02f, fx.forceps * 0.5f), dark);
                    Part(r, "Diente", new Vector3(s * 0.02f, 0f, fx.forceps * 0.2f), new Vector3(0.02f, 0.015f, 0.02f), dark);
                }
                else Part(r, "Cerco", new Vector3(0f, 0f, fx.forceps * 0.5f), new Vector3(0.02f, 0.018f, fx.forceps), dark);
            }
        if (fx.cerci > 0f)
            for (int s = -1; s <= 1; s += 2) Part(pivot, "Cerco", new Vector3(s * tail.wid * 0.3f, y0 + tail.hei * 0.6f, rear - fx.cerci * 0.4f), new Vector3(0.02f, 0.02f, fx.cerci), dark, -15f, s * 25f);

        // CIGARRA: los ojos compuestos a los lados de la cabeza, las manchas de las alas y los timbales del macho.
        if (fx.bigEyes)
            for (int s = -1; s <= 1; s += 2)
            {
                var r = new GameObject("Antena").transform; r.SetParent(pivot, false);
                r.localPosition = new Vector3(s * head.wid * 0.5f, y0 + head.hei * 0.6f, head.z);
                Part(r, "Ojo_compuesto", Vector3.zero, new Vector3(0.08f, 0.09f, 0.09f), new Color(0.3f, 0.25f, 0.2f));
            }
        if (fx.wingSpots && segZ.TryGetValue("Abdomen", out var ab))
        {
            float wl = ab.len * fx.wingLen, wz = ab.z + 0.05f + ab.len * (1.05f - fx.wingLen) * 0.5f;
            for (int s = -1; s <= 1; s += 2)
                for (int k = 0; k < 3; k++)
                    Part(pivot, "Mancha_ala", new Vector3(s * (0.08f + k * 0.01f), y0 + ab.hei + 0.02f, wz + wl * (0.3f - k * 0.15f)), new Vector3(0.035f, 0.006f, 0.035f), new Color(0.1f, 0.08f, 0.07f));
        }
        if (fx.tymbal && segZ.TryGetValue("Abdomen", out var abd))
            for (int s = -1; s <= 1; s += 2)
            {
                Part(pivot, "Timbal", new Vector3(s * abd.wid * 0.5f, y0 + abd.hei * 0.6f, abd.z + abd.len * 0.4f), new Vector3(0.01f, abd.hei * 0.45f, abd.len * 0.16f), pale);
                Part(pivot, "Operculo", new Vector3(s * abd.wid * 0.22f, y0 - 0.004f, abd.z + abd.len * 0.38f), new Vector3(abd.wid * 0.4f, 0.01f, abd.len * 0.2f), pale);
            }
        // NINFA DE CIGARRA: el fémur delantero muy grueso y con dientes, para cavar.
        if (fx.digger)
            foreach (Transform t in pivot.GetComponentsInChildren<Transform>(true))
                if (t.name == "Femur" && t.parent != null && t.parent.parent != null && FindAncestor(t, "Pata_0_") != null)
                {
                    t.localScale = new Vector3(t.localScale.x * 2.6f, t.localScale.y * 2.6f, t.localScale.z);
                    for (int k = 0; k < 3; k++) Part(t.parent, "Diente_excavador", new Vector3(0f, -t.localScale.y * 0.6f, t.localScale.z * (0.3f + k * 0.25f)), new Vector3(t.localScale.x * 0.25f, t.localScale.y * 0.5f, t.localScale.x * 0.25f), dark);
                }

        // BOREUS: el pico (rostro) hacia abajo; el macho con las alas en cerdas curvas, la hembra con muñones.
        if (fx.rostrum > 0f)
        {
            var r = new GameObject("Antena").transform; r.SetParent(pivot, false);
            r.localPosition = new Vector3(0f, y0 + head.hei * 0.3f, head.z + head.len * 0.45f); r.localRotation = Quaternion.Euler(60f, 0f, 0f);
            Part(r, "Rostro", new Vector3(0f, 0f, fx.rostrum * 0.5f), new Vector3(head.wid * 0.35f, head.wid * 0.3f, fx.rostrum), body);
        }
        if ((fx.bristleWings || fx.wingPads) && segZ.TryGetValue("Torax", out var th))
            for (int s = -1; s <= 1; s += 2)
                if (fx.bristleWings)
                    for (int k = 0; k < 2; k++) Part(pivot, "Cerda_alar", new Vector3(s * th.wid * 0.3f, y0 + th.hei + 0.03f, th.z - k * 0.06f), new Vector3(0.012f, 0.012f, 0.2f - k * 0.04f), dark, -25f, s * (160f - k * 10f));
                else Part(pivot, "Munon_alar", new Vector3(s * th.wid * 0.3f, y0 + th.hei + 0.005f, th.z), new Vector3(0.07f, 0.012f, 0.07f), dark);
    }

    void CurlTick()
    {
        if (_curlMode == 0 || _pivot == null) return;
        _curlShown = Mathf.MoveTowards(_curlShown, Mathf.Clamp01(curl), Time.deltaTime * 3f);
        float c = _curlShown;
        if (_curlMode == 1)        // bola: el cuerpo se acorta y se abomba, sin patas a la vista
            _pivot.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.75f, 1.6f, 0.42f), c);
        else if (_curlMode == 3)   // encogerse: más corto y más alto (la babosa en semiesfera, la lombriz retraída)
            _pivot.localScale = Vector3.Lerp(Vector3.one, new Vector3(1.15f, 1.35f, 0.55f), c);
        else if (_torso != null)   // concha: el cuerpo blando (cabeza y pie delantero) desaparece dentro
            _torso.localScale = Vector3.one * Mathf.Lerp(1f, 0.05f, c);
        foreach (Leg l in _legs) if (l.coxa != null && _curlMode == 1) l.coxa.localScale = Vector3.one * Mathf.Lerp(1f, 0.2f, c);
    }

    // Modo de defensa de un cuerpo ya construido (al cargar la escena): por sus piezas.
    static int CurlModeOf(Transform root)
    {
        if (FindDeep(root, "Concha") != null) return 2;
        if (FindDeep(root, "Pereion_1") != null) return 1;
        if (FindDeep(root, "Pneumostoma") != null || FindDeep(root, "Prostomio") != null) return 3;
        return 0;
    }

    static Transform FindAncestor(Transform t, string prefix) { for (Transform p = t; p != null; p = p.parent) if (p.name.StartsWith(prefix)) return p; return null; }

    // Las pinzas y los pedipalpos del solífugo, como BRAZOS: al erguirse cuelgan junto al cuerpo con el codo doblado.
    static Leg PincerLeg(Transform root)
    {
        int side = root.name.EndsWith("I") ? -1 : 1;
        var l = new Leg { coxa = root, coxaRest = root.localRotation, side = side, pair = 99, arm = true };
        l.armCoxa = Quaternion.Euler(0f, side * 160f, 0f);
        l.tibia = FindDeep(root, "Tibia_art");
        if (l.tibia != null) { l.tibiaRest = l.tibia.localRotation; l.armTibia = Quaternion.Euler(-60f, 0f, 0f); }
        return l;
    }
}
