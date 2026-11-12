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
