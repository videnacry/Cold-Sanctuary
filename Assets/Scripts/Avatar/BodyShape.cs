using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS STATS FÍSICAS MOLDEAN EL CUERPO DE CUBOS (docs/body-shape-from-stats.md). El peso (bodyMass) se reparte en tres
/// componentes que se ven distinto:
///   · ESTATURA (armazón): la parte del peso que explican fuerza y agilidad juntas → un cuerpo alto pesa y empuja más
///     sin dejar de ser ágil. Escala el cuerpo entero (±12 %).
///   · MÚSCULO: la fuerza engrosa los segmentos, y más los que ese ser ENTRENA: con las regiones del cuerpo
///     (PlayerStats.bodyStats: hombros/codos/manos → brazos; caderas/rodillas/pies → piernas; espalda → tronco) o, si no
///     las tiene, por su perfil — la agilidad sesga el músculo hacia las PIERNAS (correr, saltar), la fuerza sin agilidad
///     hacia hombros y brazos, la resistencia lo hace fibroso (más largo que ancho).
///   · GRASA: el peso que NO explican armazón ni músculo. Se deposita sobre todo en el TRONCO (abdomen, patrón androide)
///     y en CADERAS y MUSLOS (ginoide), poco en antebrazos y pantorrillas; la resistencia la reduce. (La grasa no se
///     concentra en el miembro entrenado: lo que crece ahí es el músculo — no hay «reducción localizada».)
/// Vale para humanos (CubeHumanoid) y animales (CubeCreature): lee las articulaciones por su BodyPart.
/// </summary>
public class BodyShape : MonoBehaviour
{
    [Tooltip("Cada cuántos segundos se recalcula (el cuerpo cambia despacio con el entrenamiento y la dieta).")]
    public float refreshEvery = 2f;
    [Range(0f, 1f)] public float smoothing = 0.25f;

    public float Height { get; private set; } = 1f;
    public float Muscle { get; private set; }
    public float Fat { get; private set; }
    public float LegBias { get; private set; }

    Anima _a;
    Transform _body;
    Vector3 _bodyBaseScale;
    readonly Dictionary<BodyPart, CubeJoint> _joints = new Dictionary<BodyPart, CubeJoint>();
    float _next;
    bool _init;

    enum Group { Arm, Leg, Trunk, Neck, Other }

    static Group GroupOf(BodyPart p)
    {
        switch (p)
        {
            case BodyPart.ShoulderLeft: case BodyPart.ShoulderRight: case BodyPart.ForearmLeft: case BodyPart.ForearmRight:
            case BodyPart.HandLeft: case BodyPart.HandRight: case BodyPart.ScapulaLeft: case BodyPart.ScapulaRight: return Group.Arm;
            case BodyPart.ThighLeft: case BodyPart.ThighRight: case BodyPart.ShinLeft: case BodyPart.ShinRight:
            case BodyPart.FootLeft: case BodyPart.FootRight: return Group.Leg;
            case BodyPart.Chest: case BodyPart.Back: case BodyPart.Hips: return Group.Trunk;
            case BodyPart.Neck: return Group.Neck;
            default: return Group.Other;
        }
    }

    void Start() { Init(); Apply(true); }

    void Init()
    {
        if (_init) return;
        _a = GetComponent<Anima>();
        _body = transform.Find(CubeHumanoid.BodyName);
        if (_body != null) _bodyBaseScale = _body.localScale;
        _joints.Clear();
        foreach (CubeJoint j in GetComponentsInChildren<CubeJoint>())
            if (!_joints.ContainsKey(j.part) && GroupOf(j.part) != Group.Other) _joints[j.part] = j;
        _init = true;
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + refreshEvery;
        Apply(false);
    }

    /// <summary>Recalcula estatura, músculo y grasa desde las stats y los aplica (instantáneo si <paramref name="snap"/>).</summary>
    public void Apply(bool snap)
    {
        Init();
        if (_a == null) return;
        float str = Mathf.Max(0.1f, _a.strength), mass = Mathf.Max(0.1f, _a.bodyMass), agi = Mathf.Max(0.1f, _a.agility), end = Mathf.Max(0.1f, _a.endurance);

        // Armazón: lo que fuerza Y agilidad sostienen (un ser alto es pesado, fuerte y aún ágil). Su exceso sobre 1 = estatura.
        float frame = Mathf.Min(mass, 0.5f * (str + agi));
        float height = 1f + Mathf.Clamp(0.2f * (frame - 1f), -0.15f, 0.15f);   // más marcado: antes ±2 % no se notaba
        // Músculo: la fuerza por encima de lo que el armazón trae de serie.
        float muscle = Mathf.Clamp(str - 0.5f * frame - 0.5f, -0.4f, 1.5f);
        // Grasa: el peso que no explican armazón ni músculo; la resistencia la quema.
        float fat = Mathf.Clamp((mass - frame - 0.3f * Mathf.Max(0f, muscle)) / Mathf.Max(0.5f, end), 0f, 1.5f);

        // ¿Dónde está el músculo? Por las regiones entrenadas (si las hay) o por el perfil de stats.
        float arms = 1f, legs = 1f, trunk = 1f;
        if (_a is IBody b)
        {
            float R(BodyPart p) => b.GetBodyPartStats(p).strength;
            arms = 0.5f + (R(BodyPart.Shoulders) + R(BodyPart.Elbows) + R(BodyPart.Hands)) / 3f;
            legs = 0.5f + (R(BodyPart.Hips) + R(BodyPart.Knees) + R(BodyPart.Feet)) / 3f;
            trunk = 0.5f + R(BodyPart.Back);
        }
        float legBias = Mathf.Clamp((agi - str) * 0.6f, -0.5f, 0.6f);   // ágil → piernas; fuerte sin agilidad → brazos/hombros
        legs *= 1f + legBias; arms *= 1f - 0.6f * legBias;
        float lean = Mathf.Clamp01((end - 1f) * 0.5f);                     // resistencia → fibroso

        float k = snap ? 1f : smoothing;
        Height = Mathf.Lerp(Height, height, k); Muscle = Mathf.Lerp(Muscle, muscle, k); Fat = Mathf.Lerp(Fat, fat, k); LegBias = legBias;

        if (_body != null && _bodyBaseScale != Vector3.zero) _body.localScale = _bodyBaseScale * Height;
        foreach (var kv in _joints)
        {
            Group g = GroupOf(kv.Key);
            float m = Muscle * (g == Group.Arm ? arms : g == Group.Leg ? legs : g == Group.Trunk ? trunk : 0.5f) * (1f - 0.4f * lean);
            float f = Fat * FatShare(kv.Key);
            float w = 1f + 0.65f * m + 0.9f * f;                // ancho (más marcado: con stats ~1±0,5 los cuerpos se veían iguales)
            float d = 1f + 0.5f * m + (g == Group.Trunk ? 1.2f : 0.7f) * f;   // fondo (la tripa sale hacia delante)
            Vector2 cur = kv.Value.Girth;
            kv.Value.SetGirth(Mathf.Lerp(cur.x, w, k), Mathf.Lerp(cur.y, d, k));
        }
        UpdateAnatomy();
    }


    // ── ANATOMÍA VISIBLE ─────────────────────────────────────────────────────────────────────────
    // El grosor de los segmentos dice "más ancho", pero no DÓNDE está el músculo o la grasa. Encima de cada segmento se
    // modelan los volúmenes reales (de la misma tela/piel que el segmento, así se ven como relieve de la ropa):
    //   MÚSCULO → pectorales, deltoides, bíceps, cuádriceps, gemelos, glúteos y, si hay poca grasa, la tableta abdominal.
    //   GRASA   → barriga (más baja y adelantada), michelines en las caderas, papada y el pecho más blando.
    //   Mujer   → pecho y caderas algo más anchas (de serie, más con la grasa).
    readonly Dictionary<string, Transform> _anat = new Dictionary<string, Transform>();
    bool? _fem;

    void UpdateAnatomy()
    {
        if (_body == null) return;   // solo cuerpos humanos de cubos
        if (_fem == null) _fem = CubeHair.For(name).feminine;
        float m = Mathf.Max(0f, Muscle), f = Mathf.Max(0f, Fat), fem = _fem.Value ? 1f : 0f;
        float lean = Mathf.Clamp01(1f - f * 2.5f);   // la grasa tapa la definición
        for (int s = -1; s <= 1; s += 2)
        {
            string sd = s < 0 ? "L" : "R";
            Bump($"Pectoral_{sd}", BodyPart.Chest, new Vector3(s * 0.09f, 0.2f, 0.12f), new Vector3(0.15f, 0.11f, 0.05f), m * (1f - fem) * 1.2f + f * 0.5f);
            Bump($"Pecho_{sd}", BodyPart.Chest, new Vector3(s * 0.085f, 0.16f, 0.12f), new Vector3(0.13f, 0.11f, 0.08f), fem * (0.8f + f * 0.8f));
            Bump($"Deltoides_{sd}", s < 0 ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight, new Vector3(0f, -0.04f, 0f), new Vector3(0.12f, 0.1f, 0.12f), m * 1.3f);
            Bump($"Biceps_{sd}", s < 0 ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight, new Vector3(0f, -0.17f, 0.04f), new Vector3(0.08f, 0.12f, 0.05f), m * 1.5f * lean + f * 0.3f);
            Bump($"Cuadriceps_{sd}", s < 0 ? BodyPart.ThighLeft : BodyPart.ThighRight, new Vector3(0f, -0.17f, 0.06f), new Vector3(0.12f, 0.22f, 0.05f), m * 1.2f + f * 0.4f);
            Bump($"Gemelo_{sd}", s < 0 ? BodyPart.ShinLeft : BodyPart.ShinRight, new Vector3(0f, -0.12f, -0.055f), new Vector3(0.09f, 0.14f, 0.05f), m * 1.2f + 0.2f);
            Bump($"Gluteo_{sd}", BodyPart.Hips, new Vector3(s * 0.08f, -0.04f, -0.1f), new Vector3(0.15f, 0.14f, 0.07f), 0.35f + m * 0.8f + f * 1.0f + fem * 0.4f);
            Bump($"Michelin_{sd}", BodyPart.Hips, new Vector3(s * 0.17f, 0.06f, 0f), new Vector3(0.06f, 0.08f, 0.18f), (f - 0.15f) * 2f + fem * 0.4f);
        }
        for (int r = 0; r < 3; r++)   // tableta: 2 × 3 en el abdomen (solo con músculo y poca grasa)
            for (int s = -1; s <= 1; s += 2)
                Bump($"Abdominal_{r}_{s}", BodyPart.Back, new Vector3(s * 0.045f, 0.2f - r * 0.065f, 0.112f), new Vector3(0.07f, 0.05f, 0.025f), (m - 0.15f) * 2.5f * lean);
        Bump("Barriga", BodyPart.Back, new Vector3(0f, 0.07f, 0.11f), new Vector3(0.3f, 0.17f, 0.12f), (f - 0.1f) * 2.2f);
        Bump("Papada", BodyPart.Neck, new Vector3(0f, 0.06f, 0.05f), new Vector3(0.12f, 0.05f, 0.06f), (f - 0.4f) * 2f);
    }

    // Un volumen pegado al hueso (en el espacio del hueso, como su cubo) que crece con `amount` (0 = no se ve).
    void Bump(string id, BodyPart part, Vector3 local, Vector3 size, float amount)
    {
        if (!_joints.TryGetValue(part, out CubeJoint j) || j.segment == null) return;
        amount = Mathf.Clamp(amount, 0f, 1.6f);
        if (!_anat.TryGetValue(id, out Transform t) || t == null)
        {
            if (amount < 0.05f) return;
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = id;
            // relieve de cubo, hundido en el segmento: solo asoma lo que marca la stat
            if (Application.isPlaying) Object.Destroy(g.GetComponent<Collider>()); else Object.DestroyImmediate(g.GetComponent<Collider>());
            g.transform.SetParent(j.transform, false);
            Renderer src = j.segment.GetComponent<Renderer>();
            if (src != null) g.GetComponent<Renderer>().sharedMaterial = src.sharedMaterial;
            _anat[id] = t = g.transform;
        }
        bool show = amount >= 0.05f;
        if (t.gameObject.activeSelf != show) t.gameObject.SetActive(show);
        if (!show) return;
        // Escala con el segmento (su grosor ya incluye músculo/grasa) y con la cantidad; el relieve sale hacia fuera.
        Vector2 g2 = j.Girth;
        Vector3 off = new Vector3(local.x * g2.x, local.y, local.z * g2.y);
        // HUNDIDO en el segmento: solo asoma su curva, para que se funda con el cuerpo en vez de parecer una bola pegada.
        t.localPosition = off - new Vector3(Mathf.Sign(local.x) * Mathf.Min(Mathf.Abs(local.x), size.x * 0.15f) * (local.z == 0f ? 1f : 0f), 0f, Mathf.Sign(local.z) * size.z * 0.35f * amount);
        t.localScale = new Vector3(size.x * g2.x * (0.75f + 0.45f * amount), size.y * (0.85f + 0.35f * amount), size.z * amount * 1.6f);
    }
    // Reparto de la GRASA por segmento (androide + ginoide; poca en extremidades distales).
    static float FatShare(BodyPart p)
    {
        switch (p)
        {
            case BodyPart.Chest: case BodyPart.Back: return 1f;      // abdomen / tronco
            case BodyPart.Hips: return 0.9f;
            case BodyPart.ThighLeft: case BodyPart.ThighRight: return 0.7f;
            case BodyPart.ShoulderLeft: case BodyPart.ShoulderRight: return 0.45f;
            case BodyPart.Neck: return 0.35f;
            case BodyPart.ShinLeft: case BodyPart.ShinRight: case BodyPart.ForearmLeft: case BodyPart.ForearmRight: return 0.2f;
            default: return 0.05f;                                     // manos, pies, escápulas
        }
    }
}
