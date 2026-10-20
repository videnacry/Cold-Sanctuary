using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ATENCIÓN VISUAL (docs/atencion-visual.md). Lo que vemos no es lo que entra por los ojos, sino lo que la atención
/// SELECCIONA: el ojo solo ve nítido una zona pequeña (la fóvea, ~2°) y la mirada salta (sacadas) a lo más SALIENTE.
/// La saliencia mezcla lo que sale "de abajo" (contraste de color, movimiento, cercanía: lo que se acerca o se mueve
/// roba la atención) con lo que sale "de arriba" (lo que nos importa: la misión, y los VÍNCULOS en sus dos extremos, lo
/// que amamos porque lo buscamos y lo que tememos u odiamos porque lo enfrentamos o huimos). Cada anima tiene su
/// PERFIL (pesos numéricos: hay quien atiende más a lo que se mueve, quien a los colores apagados y quietos…) y sus
/// OJOS (campo visual, alcance, sensibilidad al movimiento y al color): así hay diversidad de miradas.
/// Cada poco, el anima lanza (inconscientemente) el hechizo ENFOCAR sobre lo más saliente; el jugador puede lanzarlo a
/// conciencia (`enfocar` en el lanzador) y fijarlo un rato. En primera persona, la cámara "acerca" lo enfocado (un leve
/// zoom, como la atención que se estrecha) y vuelve a abrirse al soltarlo.
/// </summary>
public class VisualAttention : MonoBehaviour
{
    public enum Profile { Equilibrado, Vigilante, Esteta, Contemplativo, Afectivo }

    [Header("Perfil de atención (pesos)")]
    public Profile profile = Profile.Equilibrado;
    [Range(-1f, 2f)] public float contrastWeight = 1f, proximityWeight = 1f, motionWeight = 1f, bondWeight = 1f, missionWeight = 1f;

    [Header("Ojos")]
    [Tooltip("Campo visual total (grados). Presas: muy amplio (ojos a los lados); depredadores y humanos: frontal.")]
    [Range(30f, 340f)] public float fieldOfView = 180f;
    [Tooltip("Alcance en el que distingue algo (m), escalado por la percepción del anima.")]
    public float range = 30f;
    [Range(0f, 2f)] public float motionSensitivity = 1f, colorSensitivity = 1f;

    [Header("Estado")]
    public Transform focus;                 // lo que enfoca ahora
    public float focusSalience;             // cuánto (0..)
    public bool conscious;                  // enfocado a propósito (el jugador) hasta `holdUntil`
    float _holdUntil, _next;
    readonly Dictionary<Transform, Vector3> _lastPos = new Dictionary<Transform, Vector3>();
    Anima _a;

    public static VisualAttention Player { get; private set; }
    public event System.Action<Transform> Focused;

    /// <summary>Grados de "zoom" de la atención (para la cámara en primera persona).</summary>
    public float ZoomDegrees => focus == null ? 0f : Mathf.Clamp(focusSalience * 6f, 0f, conscious ? 14f : 8f);

    void Awake()
    {
        _a = GetComponent<Anima>();
        ApplyProfile(profile);
        if (CompareTag("Player")) Player = this;
    }

    /// <summary>Pesos de cada perfil (modelos de atención).</summary>
    public void ApplyProfile(Profile p)
    {
        profile = p;
        switch (p)
        {
            case Profile.Vigilante: contrastWeight = 0.6f; proximityWeight = 1.4f; motionWeight = 1.8f; bondWeight = 0.8f; missionWeight = 0.8f; break;   // lo que se mueve y se acerca
            case Profile.Esteta: contrastWeight = 1.8f; proximityWeight = 0.6f; motionWeight = 0.6f; bondWeight = 0.8f; missionWeight = 0.6f; break;      // colores que destacan
            case Profile.Contemplativo: contrastWeight = -0.6f; proximityWeight = 0.4f; motionWeight = -0.8f; bondWeight = 1f; missionWeight = 0.6f; break; // lo apagado y lo quieto
            case Profile.Afectivo: contrastWeight = 0.5f; proximityWeight = 0.8f; motionWeight = 0.6f; bondWeight = 2f; missionWeight = 1f; break;       // los vínculos (amor y rechazo)
            default: contrastWeight = proximityWeight = motionWeight = bondWeight = missionWeight = 1f; break;
        }
    }

    /// <summary>Hechizo ENFOCAR a conciencia: fija la atención en <paramref name="target"/> unos segundos.</summary>
    public void FocusOn(Transform target, float seconds = 8f)
    {
        focus = target; conscious = true; _holdUntil = Time.time + seconds; focusSalience = 2f;
        Focused?.Invoke(target);
        Debug.Log($"[Atención] «{name}» enfoca a propósito «{target.name}».");
    }

    void Update()
    {
        if (conscious && Time.time < _holdUntil) return;
        conscious = false;
        if (Time.time < _next) return;
        _next = Time.time + 0.25f;
        if (_a != null && _a.asleep) { focus = null; return; }   // dormido no se ve nada
        var best = Scan(out float s);
        if (best != focus && best != null) Focused?.Invoke(best);   // enfoca (sin pensarlo)
        focus = best; focusSalience = s;
    }

    /// <summary>Lo visible ordenado por saliencia (lo usa también el lanzador para sugerir).</summary>
    public List<(Transform t, float score)> Ranking(int max = 8)
    {
        var l = new List<(Transform, float)>();
        foreach (Anima o in FindObjectsByType<Anima>(FindObjectsSortMode.None))
        {
            if (o == null || o == _a || o.death) continue;
            float sc = Salience(o.transform, o);
            if (sc > 0f) l.Add((o.transform, sc));
        }
        l.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        if (l.Count > max) l.RemoveRange(max, l.Count - max);
        return l;
    }

    Transform Scan(out float best)
    {
        best = 0f; Transform pick = null;
        foreach (var (t, sc) in Ranking(1)) { pick = t; best = sc; }
        return pick;
    }

    /// <summary>SALIENCIA de algo para este anima (0 = no lo ve o no le importa).</summary>
    public float Salience(Transform t, Anima other)
    {
        float reach = range * Mathf.Max(0.2f, _a != null ? _a.perception : 1f);
        Vector3 eye = transform.position + Vector3.up * 1.5f;
        Vector3 d = t.position + Vector3.up * 1f - eye;
        float dist = d.magnitude;
        if (dist > reach || dist < 0.3f) return 0f;
        if (Vector3.Angle(transform.forward, d) > fieldOfView * 0.5f) return 0f;                       // fuera del campo visual
        if (Physics.Raycast(eye, d.normalized, out RaycastHit hit, dist - 0.4f, ~0, QueryTriggerInteraction.Ignore)
            && !hit.transform.IsChildOf(t) && !hit.transform.IsChildOf(transform)) return 0f;           // algo lo tapa

        float proximity = 1f - dist / reach;                                                          // cerca → más
        Vector3 last = _lastPos.TryGetValue(t, out Vector3 lp) ? lp : t.position; _lastPos[t] = t.position;
        float motion = Mathf.Clamp01((t.position - last).magnitude / 0.25f / 4f) * motionSensitivity; // m/s → 0..1
        float contrast = Contrast(t) * colorSensitivity;
        float bond = 0f;
        ITarget it = other != null ? other.GetComponent<ITarget>() : null;
        if (_a != null && it != null) { Bond b = _a.GetBond(it); if (b != null) bond = Mathf.Clamp01(Mathf.Abs(b.value) / 50f); }   // los dos EXTREMOS llaman
        float threat = other is Animal an && an.Forage != null && an.Forage.eatsPrey ? 0.3f : 0f;          // un depredador siempre llama algo la atención
        float mission = MissionRelevance(t);
        float s = contrastWeight * contrast + proximityWeight * proximity + motionWeight * motion + bondWeight * (bond + threat) + missionWeight * mission;
        return Mathf.Max(0f, s);
    }

    // Contraste del color dominante del objeto con un fondo de hielo/nieve (claro y frío).
    static readonly Color Background = new Color(0.85f, 0.9f, 0.95f);
    static float Contrast(Transform t)
    {
        Renderer r = t.GetComponentInChildren<Renderer>();
        if (r == null || r.sharedMaterial == null) return 0f;
        Color c = r.sharedMaterial.HasProperty("_BaseColor") ? r.sharedMaterial.GetColor("_BaseColor") : r.sharedMaterial.color;
        Color.RGBToHSV(c, out float h, out float sat, out float v);
        float lum = Mathf.Abs(v - 0.9f), chroma = sat;   // lo oscuro y lo muy saturado destacan sobre la nieve
        return Mathf.Clamp01(lum * 0.6f + chroma * 0.8f);
    }

    // Lo que tiene que ver con mi misión de ahora (la persona a la que acompaño, el área a la que voy) importa más.
    float MissionRelevance(Transform t)
    {
        WorkdayDirector wd = WorkdayDirector.Instance;
        if (wd == null || wd.today == null || TimeController.timeController == null) return 0f;
        WorkRoster.Worker w = wd.roster.Find(CompareTag("Player") ? "Player" : name.Replace("_Post", ""));
        if (w == null) return 0f;
        string area = wd.Where(w, TimeController.timeController.Hour, out _);
        if (area == null) return 0f;
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
            if (a.displayName == area && Vector3.Distance(a.transform.position, t.position) < 15f) return 0.6f;
        return 0f;
    }
}
