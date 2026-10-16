using UnityEngine;

/// <summary>
/// CARA DE CUBOS con EXPRESIONES: la cabeza deja de ser un cubo liso y se arma con piezas —ojos (blanco + pupila),
/// párpados, cejas, hocico/nariz, comisuras y una MANDÍBULA articulada— cada una con su hueso en el
/// <see cref="CreatureRig"/> (Jaw, BrowLeft/Right, EyelidLeft/Right, MouthLeft/Right), así que una expresión es una
/// POSTURA más de la cara. La expresión sale del estado del ser (dormido, alerta, estrés, satisfacción, enfermedad) o
/// se fuerza con <see cref="Show"/>; parpadea solo.
/// Sirve a humanos (cara plana) y animales (con hocico: <c>snout</c> &gt; 0).
/// </summary>
public class CubeFace : MonoBehaviour
{
    public enum Expression { Neutral, Happy, Sad, Angry, Afraid, Surprised, Sleepy, Asleep }

    public Expression current = Expression.Neutral;
    public Transform browL, browR, lidL, lidR, jaw, mouthL, mouthR, earL, earR;
    [SerializeField] Vector3 _lidScale;
    [SerializeField] float _earRestX;

    Anima _a;
    Animal _animal;
    Expression _forced; float _forcedUntil;
    float _blinkAt, _blinkT;
    float _jawOpen, _browTilt, _browY, _lidOpen = 1f, _mouthCurve, _earTilt;

    /// <summary>Fuerza una expresión unos segundos (un hechizo de pensamiento, una caricia, un susto…).</summary>
    public void Show(Expression e, float seconds = 3f) { _forced = e; _forcedUntil = Time.time + seconds; }

    void Awake() { _a = GetComponentInParent<Anima>(); _animal = _a as Animal; _blinkAt = Time.time + Random.Range(2f, 5f); }

    Expression FromState()
    {
        if (_a == null) return Expression.Neutral;
        if (_a.death || _a.asleep) return Expression.Asleep;
        if (_a.aware) return _animal != null && _animal.Forage != null && _animal.Forage.eatsPrey ? Expression.Angry : Expression.Afraid;
        if (_a.stress > 0.65f) return Expression.Angry;
        if (_a.sickness > 0.4f || (_animal != null && _animal.hungry > 0.85f)) return Expression.Sad;
        if (_a.sleepiness > 0.75f) return Expression.Sleepy;
        if (_a.satisfaction > 0.6f) return Expression.Happy;
        return Expression.Neutral;
    }

    void Update()
    {
        current = Time.time < _forcedUntil ? _forced : FromState();
        // Objetivos por expresión: mandíbula (grados), inclinación de cejas (+ = enfado: interior abajo), altura de cejas,
        // apertura de párpados (0–1.3), curva de la boca (+ sonrisa), orejas (+ = atrás/gachas).
        float jaw = 0f, tilt = 0f, by = 0f, lid = 1f, curve = 0f, ear = 0f;
        switch (current)
        {
            case Expression.Happy:     jaw = 6f;  tilt = -5f;  by = 0.01f;  lid = 0.75f; curve = 1f;   ear = -10f; break;
            case Expression.Sad:       jaw = 0f;  tilt = -18f; by = 0f;     lid = 0.6f;  curve = -1f;  ear = 35f;  break;
            case Expression.Angry:     jaw = 14f; tilt = 22f;  by = -0.02f; lid = 0.7f;  curve = -0.6f; ear = 40f; break;
            case Expression.Afraid:    jaw = 10f; tilt = -15f; by = 0.03f;  lid = 1.3f;  curve = -0.5f; ear = 50f; break;
            case Expression.Surprised: jaw = 22f; tilt = 0f;   by = 0.04f;  lid = 1.3f;  curve = 0f;   ear = -15f; break;
            case Expression.Sleepy:    jaw = 3f;  tilt = -6f;  by = -0.01f; lid = 0.35f; curve = 0f;   ear = 20f;  break;
            case Expression.Asleep:    jaw = 0f;  tilt = 0f;   by = -0.01f; lid = 0.05f; curve = 0f;   ear = 25f;  break;
        }
        // Parpadeo.
        if (current != Expression.Asleep && Time.time >= _blinkAt) { _blinkT = 0.14f; _blinkAt = Time.time + Random.Range(2.5f, 6f); }
        if (_blinkT > 0f) { _blinkT -= Time.deltaTime; lid = 0.05f; }

        float k = 1f - Mathf.Exp(-10f * Time.deltaTime);
        _jawOpen = Mathf.Lerp(_jawOpen, jaw, k); _browTilt = Mathf.Lerp(_browTilt, tilt, k); _browY = Mathf.Lerp(_browY, by, k);
        _lidOpen = Mathf.Lerp(_lidOpen, lid, _blinkT > 0f ? 1f : k); _mouthCurve = Mathf.Lerp(_mouthCurve, curve, k); _earTilt = Mathf.Lerp(_earTilt, ear, k);
        Apply();
    }

    void Apply()
    {
        if (jaw != null) jaw.localRotation = Quaternion.Euler(_jawOpen, 0f, 0f);
        if (browL != null) { browL.localRotation = Quaternion.Euler(0f, 0f, -_browTilt); browL.localPosition = Y(browL.localPosition, _browBaseY + _browY); }
        if (browR != null) { browR.localRotation = Quaternion.Euler(0f, 0f, _browTilt); browR.localPosition = Y(browR.localPosition, _browBaseY + _browY); }
        if (lidL != null) lidL.localScale = new Vector3(_lidScale.x, _lidScale.y * Mathf.Clamp(1.4f - _lidOpen, 0.05f, 1.4f), _lidScale.z);
        if (lidR != null) lidR.localScale = new Vector3(_lidScale.x, _lidScale.y * Mathf.Clamp(1.4f - _lidOpen, 0.05f, 1.4f), _lidScale.z);
        if (mouthL != null) mouthL.localRotation = Quaternion.Euler(0f, 0f, -18f * _mouthCurve);
        if (mouthR != null) mouthR.localRotation = Quaternion.Euler(0f, 0f, 18f * _mouthCurve);
        if (earL != null) earL.localRotation = Quaternion.Euler(_earRestX - _earTilt, 0f, 0f);
        if (earR != null) earR.localRotation = Quaternion.Euler(_earRestX - _earTilt, 0f, 0f);
    }

    [SerializeField] float _browBaseY;
    static Vector3 Y(Vector3 v, float y) => new Vector3(v.x, y, v.z);

    // ── Construcción ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Arma la cara sobre la articulación <paramref name="head"/> (su cubo es la caja del cráneo de tamaño
    /// <paramref name="size"/>, centrado en <paramref name="center"/>, mirando a +Z). <paramref name="snout"/> = largo del
    /// hocico (0 = cara plana humana). Registra los huesos de la cara en el rig.
    /// </summary>
    public static CubeFace Build(Transform head, Vector3 center, Vector3 size, float snout, Color skin, Color eyeColor, CreatureRig rig,
                                 Transform earL = null, Transform earR = null)
    {
        CubeFace f = head.gameObject.AddComponent<CubeFace>();
        Color dark = new Color(0.12f, 0.12f, 0.14f), white = new Color(0.97f, 0.97f, 0.95f), lip = skin * 0.8f + new Color(0.1f, 0f, 0f, 1f);
        float front = center.z + size.z * 0.5f;
        float eyeY = center.y + size.y * 0.12f, eyeX = size.x * 0.24f, eye = Mathf.Min(size.x, size.y) * 0.2f;

        for (int s = -1; s <= 1; s += 2)
        {
            Vector3 ep = new Vector3(eyeX * s, eyeY, front + 0.001f);
            Cube(head, "Ojo", ep, new Vector3(eye, eye, eye * 0.2f), white);
            Cube(head, "Pupila", ep + new Vector3(0f, 0f, eye * 0.12f), new Vector3(eye * 0.5f, eye * 0.55f, eye * 0.1f), eyeColor);
            // Párpado: pivote arriba del ojo; escalar su Y lo baja (cerrar) sin moverse de sitio.
            Transform lid = Pivot(head, s < 0 ? "Parpado_L" : "Parpado_R", ep + new Vector3(0f, eye * 0.5f, eye * 0.18f));
            Cube(lid, "Parpado", new Vector3(0f, -0.5f, 0f), new Vector3(1f, 1f, 1f), skin * 0.92f + new Color(0, 0, 0, 1));
            lid.localScale = new Vector3(eye * 1.15f, eye * 0.75f, eye * 0.12f);
            Transform brow = Pivot(head, s < 0 ? "Ceja_L" : "Ceja_R", new Vector3(eyeX * s, eyeY + eye * 0.85f, front + 0.005f));
            Cube(brow, "Ceja", Vector3.zero, new Vector3(eye * 1.3f, eye * 0.22f, eye * 0.18f), dark);
            if (s < 0) { f.lidL = lid; f.browL = brow; } else { f.lidR = lid; f.browR = brow; }
            f._browBaseY = brow.localPosition.y;
            Bind(rig, s < 0 ? BodyPart.EyelidLeft : BodyPart.EyelidRight, lid);
            Bind(rig, s < 0 ? BodyPart.BrowLeft : BodyPart.BrowRight, brow);
        }
        f._lidScale = f.lidL.localScale;

        // Hocico (animales) o nariz (humanos), boca con comisuras y mandíbula articulada.
        float mouthY = center.y - size.y * 0.28f;
        float mz = front;
        if (snout > 0f)
        {
            Transform sn = Pivot(head, "Hocico", new Vector3(0f, mouthY + size.y * 0.1f, front));
            Cube(sn, "Hocico", new Vector3(0f, 0f, snout * 0.5f), new Vector3(size.x * 0.55f, size.y * 0.38f, snout), skin);
            Cube(sn, "Nariz", new Vector3(0f, size.y * 0.15f, snout + 0.002f), new Vector3(size.x * 0.22f, size.y * 0.12f, size.z * 0.06f), dark);
            Bind(rig, BodyPart.Snout, sn);
            mz = front + snout * 0.9f;
        }
        else Cube(head, "Nariz", new Vector3(0f, center.y - size.y * 0.05f, front + size.z * 0.05f), new Vector3(size.x * 0.12f, size.y * 0.16f, size.z * 0.12f), skin * 0.95f + new Color(0, 0, 0, 1));

        Transform jaw = Pivot(head, "Mandibula", new Vector3(0f, mouthY, front - size.z * 0.35f));
        float jawLen = (mz - (front - size.z * 0.35f));
        Cube(jaw, "Mandibula", new Vector3(0f, -size.y * 0.06f, jawLen * 0.5f), new Vector3(size.x * (snout > 0f ? 0.5f : 0.7f), size.y * 0.12f, jawLen), skin);
        Cube(jaw, "Boca", new Vector3(0f, size.y * 0.005f, jawLen - 0.002f), new Vector3(size.x * 0.34f, size.y * 0.035f, size.z * 0.03f), lip);
        f.jaw = jaw; Bind(rig, BodyPart.Jaw, jaw);
        for (int s = -1; s <= 1; s += 2)
        {
            Transform c = Pivot(head, s < 0 ? "Comisura_L" : "Comisura_R", new Vector3(size.x * 0.17f * s, mouthY + size.y * 0.02f, mz + 0.003f));
            Cube(c, "Comisura", new Vector3(size.x * 0.05f * s, 0f, 0f), new Vector3(size.x * 0.1f, size.y * 0.03f, size.z * 0.03f), lip);
            if (s < 0) f.mouthL = c; else f.mouthR = c;
            Bind(rig, s < 0 ? BodyPart.MouthLeft : BodyPart.MouthRight, c);
        }
        f.earL = earL; f.earR = earR;
        if (earL != null) f._earRestX = earL.localEulerAngles.x;
        return f;
    }

    static void Bind(CreatureRig rig, BodyPart p, Transform t)
    {
        if (rig != null) rig.bindings.Add(new CreatureRig.PartBinding { part = p, bone = t });
    }

    static Transform Pivot(Transform parent, string name, Vector3 local)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = local;
        return t;
    }

    static void Cube(Transform parent, string name, Vector3 local, Vector3 size, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        Collider col = g.GetComponent<Collider>();
        if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = local;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = CubeCreature.Mat(c);
    }
}
