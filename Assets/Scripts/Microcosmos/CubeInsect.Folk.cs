using UnityEngine;

/// <summary>
/// INSECTOS-HUMANOS DE LAS ERAS CIVILIZADAS (docs/microcosmos-civilizaciones.md §6; Berón 2026-11-01: «a los insectos-humanos
/// se les debe dar caras más humanas y cabello… todos tienen un tamaño más similar para que puedan interactuar de igual a
/// igual»):
/// - <see cref="DressFolk"/>: la especie, ERGUIDA y a la ESTATURA de la persona (1,6–1,9 m): una mariquita y una mantis
///   miden lo mismo de pie y se miran a los ojos;
/// - <see cref="Humanize"/>: cara humana (piel, nariz, labios, orejas, cejas del color del pelo) y CABELLO con los peinados
///   de cubos del santuario (<see cref="CubeHair"/>), sin quitar antenas ni mandíbulas: se reconocen como insectos;
/// - <see cref="Martial"/>: las formas de la MANTIS (Bruce Lee, <see cref="MantisFist"/>) mueven los brazos raptoriales.
/// </summary>
public partial class CubeInsect
{
    /// <summary>Insecto-humano de era civilizada (siempre erguido).</summary>
    public bool folk;

    // Mientras DressFolk viste: Dress usa la ficha de insecto-humano (InsectMorphology.ForFolk).
    static bool _dressingFolk;

    // ── LA POSTURA RECOGIDA (2026-11-18, docs/plan-microcosmos-realismo.md §1.1 b y §3.1; Berón: «el ancho es importante») ──────
    // De pie, la hormiga real abre las patas 2,5 m a escala humana (P3 a −30°, tibia 60° bajo la horizontal). El vecino las lleva
    // RECOGIDAS: la rodilla más cerrada deja la tibia casi vertical, bajo el cuerpo, y el par trasero va algo más atrás. Calculado con
    // la geometría de BuildLeg: ancho de pie ~0,90 L y andando (coxa ±25°, trocánter alzado) ~1,0 L, frente a 1,24 L y 1,41 L con las
    // patas abiertas; el cuerpo queda ~0,08 L más alto (FitHeight lo devuelve a su estatura). Lo mide FolkScaleTest (grupo 119).
    /// <summary>¿Llevan los vecinos (DressFolk) la postura recogida? (Las estatuas y la galería, igual.)</summary>
    public static bool TuckFolkLegs = true;
    /// <summary>Ángulo de la rodilla (fémur → tibia) en la postura recogida (100° con las patas abiertas).</summary>
    public const float FolkTuckKnee = 130f;
    /// <summary>Reposo de la coxa del par trasero en la postura recogida (−30° con las patas abiertas).</summary>
    public const float FolkTuckRearYaw = -35f;
    /// <summary>Cuánto sube el vientre (en L) con la postura recogida, para estimar el cuerpo sin tenerlo delante (MicroBody.OfSpecies).</summary>
    public const float FolkTuckBellyLift = 0.08f;

    /// <summary>
    /// LA HUELLA del cuerpo en el suelo (m, en el marco del anfitrión): x = ancho de lado a lado, y = largo de delante a atrás, con
    /// todo lo visible (patas, brazos, antenas, pelo). Con <paramref name="walking"/>, el BARRIDO de la marcha: cada pata de andar
    /// recorre su paso (coxa ±25°, trocánter alzado en la fase de vuelo, como en Update) y se toma lo más ancho y lo más largo; la
    /// pose se deja como estaba. Mide las esquinas de cada cubo (no sus cajas alineadas, que engordan las piezas giradas).
    /// </summary>
    public Vector2 Footprint(bool walking)
    {
        if (_legs.Count == 0) Awake();
        Transform root = transform.Find("Insecto");
        if (root == null) return Vector2.zero;
        var saved = new System.Collections.Generic.List<(Transform t, Quaternion q)>();
        foreach (Leg l in _legs) { if (l.coxa != null) saved.Add((l.coxa, l.coxa.localRotation)); if (l.troch != null) saved.Add((l.troch, l.troch.localRotation)); }
        Vector2 best = Vector2.zero;
        int steps = walking ? 9 : 1;
        const float amp = 25f;
        try
        {
            for (int k = 0; k < steps; k++)
            {
                float ph = walking ? Mathf.Lerp(-1f, 1f, k / (float)(steps - 1)) : 0f;
                if (walking)
                    foreach (Leg l in _legs)
                    {
                        if (l.coxa == null || l.arm) continue;   // los brazos (erguido) no andan
                        l.coxa.localRotation = l.coxaRest * Quaternion.Euler(0f, ph * amp * l.side, 0f);
                        if (l.troch != null) l.troch.localRotation = l.trochRest * Quaternion.Euler(-Mathf.Max(0f, ph) * amp * 0.6f, 0f, 0f);
                    }
                Vector2 f = Span(root);
                best = new Vector2(Mathf.Max(best.x, f.x), Mathf.Max(best.y, f.y));
            }
        }
        finally { foreach (var (t, q) in saved) if (t != null) t.localRotation = q; }
        return best;
    }

    // Ancho (x) y largo (z) de todo lo visible bajo «Insecto», en el marco del anfitrión (m).
    Vector2 Span(Transform root)
    {
        float x0 = float.MaxValue, x1 = float.MinValue, z0 = float.MaxValue, z1 = float.MinValue;
        foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>())
        {
            if (!r.enabled) continue;
            MeshFilter mf = r.GetComponent<MeshFilter>();
            Bounds mb = mf != null && mf.sharedMesh != null ? mf.sharedMesh.bounds : new Bounds(Vector3.zero, Vector3.one);
            Matrix4x4 m = transform.worldToLocalMatrix * r.transform.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                Vector3 c = new Vector3((i & 1) == 0 ? mb.min.x : mb.max.x, (i & 2) == 0 ? mb.min.y : mb.max.y, (i & 4) == 0 ? mb.min.z : mb.max.z);
                Vector3 p = m.MultiplyPoint3x4(c);
                x0 = Mathf.Min(x0, p.x); x1 = Mathf.Max(x1, p.x); z0 = Mathf.Min(z0, p.z); z1 = Mathf.Max(z1, p.z);
            }
        }
        return x1 < x0 ? Vector2.zero : new Vector2((x1 - x0) * Mathf.Abs(transform.lossyScale.x), (z1 - z0) * Mathf.Abs(transform.lossyScale.z));
    }

    /// <summary>Viste a <paramref name="host"/> como <paramref name="species"/> erguida de <paramref name="height"/> m, con la
    /// cara y el pelo de <paramref name="name"/> (<see cref="CubeHair.For"/>).</summary>
    public static CubeInsect DressFolk(GameObject host, string species, float height, string name, bool female = true)
    {
        CubeInsect ci;
        _dressingFolk = true;
        try { ci = Dress(host, species, InsectCast.ColorOf(species), Mathf.Max(0.5f, height), Metamorphosis.Phase.Adult, female); }
        finally { _dressingFolk = false; }
        ci.PoseNow(true);
        ci.folk = true;
        FitHeight(host, height);
        Humanize(host, CubeHair.For(name));
        return ci;
    }

    /// <summary>Escala el cuerpo (ya erguido) para que mida <paramref name="height"/> de los pies a la coronilla.</summary>
    public static void FitHeight(GameObject host, float height)
    {
        Transform root = host.transform.Find("Insecto");
        if (root == null || height <= 0f) return;
        if (!Measure(root, host.transform, out float low, out float high) || high - low < 0.05f) return;
        float k = height / (high - low);
        root.localScale *= k;
        // Los pies, en el suelo del anfitrión (y = 0 en su marco).
        if (Measure(root, host.transform, out low, out _)) root.localPosition -= new Vector3(0f, low, 0f);
    }

    static bool Measure(Transform root, Transform host, out float low, out float high)
    {
        low = float.MaxValue; high = float.MinValue; bool any = false;
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (!r.enabled) continue;
            Bounds b = r.bounds;
            float lo = host.InverseTransformPoint(b.min).y, hi = host.InverseTransformPoint(b.max).y;
            low = Mathf.Min(low, Mathf.Min(lo, hi)); high = Mathf.Max(high, Mathf.Max(lo, hi)); any = true;
        }
        return any;
    }

    /// <summary>Cara humana y cabello sobre la cabeza del insecto (la de <see cref="Anchors"/>).</summary>
    public static void Humanize(GameObject host, CubeHair.Look look)
    {
        if (!Anchors(host, out _, out Transform neck, out Bounds head)) return;
        foreach (Transform t in neck) if (t.name == "Cara_humana" || t.name == "Marco_cabello") DestroyNow(t.gameObject);
        Transform face = new GameObject("Cara_humana").transform; face.SetParent(neck, false);
        float w = head.size.x, h = head.size.y, front = head.max.z;
        Color skin = look.skin, hair = look.hair;
        // La piel de la cara (detrás de los ojos y la boca, que van a +0,002…+0,007 del frente).
        Part(face, "Piel_cara", new Vector3(head.center.x, head.center.y - h * 0.04f, front + 0.0012f), new Vector3(w * 0.84f, h * 0.8f, 0.0016f), skin);
        Part(face, "Nariz", new Vector3(head.center.x, head.center.y - h * 0.06f, front + w * 0.05f), new Vector3(w * 0.13f, h * 0.2f, w * 0.1f), skin * 0.93f + new Color(0f, 0f, 0f, 0.07f));
        Part(face, "Labios", new Vector3(head.center.x, head.center.y - h * 0.22f, front + 0.0045f), new Vector3(w * 0.3f, h * 0.07f, 0.004f), look.hasLips ? look.lips : new Color(0.6f, 0.32f, 0.3f));
        for (int s = -1; s <= 1; s += 2)
            Part(face, "Oreja", new Vector3(head.center.x + s * (head.extents.x + w * 0.04f), head.center.y, head.center.z), new Vector3(w * 0.08f, h * 0.28f, head.size.z * 0.22f), skin);
        // Cejas del color del pelo.
        foreach (Transform t in neck) if (t.name == "Ceja") { Renderer r = t.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = KitchenProps.Mat(hair); }
        // El CABELLO del santuario, en un marco escalado: CubeHair está hecho para una cabeza de 0,24 × 0,26 × 0,24.
        Transform frame = new GameObject("Marco_cabello").transform; frame.SetParent(neck, false);
        frame.localPosition = head.center;
        float s0 = Mathf.Max(0.001f, w / 0.24f);
        frame.localScale = Vector3.one * s0;
        CubeHair.Build(frame, Vector3.zero, new Vector3(0.24f, h / s0, head.size.z / s0), look);
    }

    /// <summary>Camisa sencilla sobre el torso (la ropa de diario de cualquier era).</summary>
    public static void WearShirt(GameObject host, Color shirt)
    {
        if (!Anchors(host, out Transform torso, out _, out Bounds head)) return;
        Part(torso, "Camisa", new Vector3(0f, 0f, 0.06f), new Vector3(head.size.x * 1.25f, head.size.y * 1.4f, 0.26f), shirt);
    }

    /// <summary>El par de una pata por su nombre («Pata_12_I» → 12): el ciempiés tiene 21 pares (antes se leía un solo dígito).</summary>
    static int PairOf(string n)
    {
        int end = n.IndexOf('_', 5);
        return end > 5 && int.TryParse(n.Substring(5, end - 5), out int p) ? p : n[5] - '0';
    }

    // ══ LAS FORMAS DE LA MANTIS (Bruce Lee) ═══════════════════════════════════════════════════════════════════════
    float _mUntil = -1f, _mL, _mR, _mLift, _mSway, _mMid;

    /// <summary>
    /// Fija este fotograma (y 0,3 s) la postura de los BRAZOS RAPTORIALES: <paramref name="reachL"/>/<paramref name="reachR"/>
    /// 0 = plegado en la guardia de «rezo» (tibia contra el fémur, en la línea central), 1 = golpe extendido; <paramref name="lift"/>
    /// 0 = colgando, 1 = a la altura de la cabeza; <paramref name="sway"/> −1…1 el balanceo de hoja (el juego de pies de la
    /// mantis); <paramref name="mid"/> 0…1 el segundo par de brazos, del costado (0) al frente (1).
    /// </summary>
    public void Martial(float reachL, float reachR, float lift, float sway, float mid)
    {
        _mL = Mathf.Clamp01(reachL); _mR = Mathf.Clamp01(reachR); _mLift = Mathf.Clamp01(lift); _mSway = Mathf.Clamp(sway, -1f, 1f); _mMid = Mathf.Clamp01(mid);
        _mUntil = Time.time + 0.3f;
        if (!Application.isPlaying) ApplyMartial();   // en el editor (pruebas, capturas): ya
    }

    /// <summary>¿Está ahora mismo en una forma marcial?</summary>
    public bool InMartial => Time.time <= _mUntil;

    void LateUpdate()
    {
        if (Time.time > _mUntil)   // (sin forma marcial: nada que hacer; la fauna no paga ni un Find por fotograma)
        {
            if (_swaying) { Transform root = transform.Find("Insecto"); if (root != null) root.localRotation = Quaternion.identity; _swaying = false; }
            return;
        }
        ApplyMartial();
    }
    bool _swaying;

    void ApplyMartial()
    {
        if (_legs.Count == 0) Awake();
        foreach (Leg l in _legs)
        {
            if (l.coxa == null || !l.arm) continue;
            if (l.pair == 0)
            {
                float reach = l.side < 0 ? _mL : _mR;
                // Sube el brazo hacia delante (como Push: + = adelante) y abre la tibia: plegada (la de reposo, en navaja) → recta.
                if (l.troch != null) l.troch.localRotation = l.armTroch * Quaternion.Euler(30f + 50f * _mLift + 20f * reach, 0f, 0f);
                l.coxa.localRotation = Quaternion.Slerp(l.armCoxa, l.armCoxa * Quaternion.Euler(0f, -l.side * 12f, 0f), 1f - reach);   // en guardia, los codos hacia la línea central
                if (l.tibia != null) l.tibia.localRotation = Quaternion.Slerp(l.tibiaRest, Quaternion.Euler(8f, 0f, 0f), reach);
            }
            else
            {
                if (l.troch != null) l.troch.localRotation = l.armTroch * Quaternion.Euler(60f * _mMid, 0f, 0f);
                if (l.tibia != null) l.tibia.localRotation = Quaternion.Slerp(l.armTibia, l.tibiaRest, 0.3f * _mMid);
            }
        }
        Transform root = transform.Find("Insecto");
        if (root != null) { root.localRotation = Quaternion.Euler(0f, 0f, _mSway * 9f); _swaying = true; }
    }
}
