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
