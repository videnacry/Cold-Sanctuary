using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VIRTUALIZACIÓN EN EL ÁREA REAL: la misión se practica en la PROPIA área del santuario (con sus muebles y la gente que
/// esté), no en una escena aparte. Sus pasos se anclan a objetos reales (la camilla, el analizador, el tanque…) y el
/// jugador va GUIADO: una baliza de luz sobre el siguiente paso, el texto de qué hacer y hacia dónde queda. Al terminar
/// cuenta como hecha (WorkdayDirector) y se «despierta» delante de la máquina.
/// (Si existe una escena Virtual_&lt;clave&gt; con su propio runner, también funciona: es el modo antiguo.)
/// </summary>
public class VirtualMissionRunner : MonoBehaviour
{
    /// <summary>La misión que la máquina mandó practicar.</summary>
    public static WorkRoster.Mission Pending;
    public static VirtualMissionRunner Active { get; private set; }

    public string key;
    public string title;
    public VirtualMissionStation[] stations;
    public int next;
    public bool done;
    public bool inPlace;
    public Transform returnPoint;
    public System.Action<int> onStep;   // lo que pasa al completar cada paso (posturas del examen…)
    public System.Action onEnd;
    float _exitAt = -1f;
    Transform _beacon, _player;

    void OnEnable() { Active = this; }
    void OnDisable() { if (Active == this) Active = null; }
    void OnDestroy() { onEnd?.Invoke(); }

    /// <summary>Empieza la misión en su área real. <paramref name="anchors"/>: un punto (o objeto) por paso.</summary>
    public static VirtualMissionRunner StartInPlace(WorkRoster.Mission m, Transform[] anchors, Transform returnPoint)
    {
        if (Active != null) Destroy(Active.gameObject);
        var go = new GameObject($"Virtualizacion_{m.id}");
        var r = go.AddComponent<VirtualMissionRunner>();
        r.key = VirtualMissions.Key(m.id); r.title = m.title; r.inPlace = true; r.returnPoint = returnPoint;
        Pending = m;
        string[] steps = VirtualMissions.Steps(m.id);
        var list = new List<VirtualMissionStation>();
        for (int i = 0; i < steps.Length; i++)
        {
            Transform a = i < anchors.Length ? anchors[i] : null;
            var st = new GameObject($"Paso_{i + 1}").transform;
            st.SetParent(go.transform, false);
            st.position = a != null ? a.position : (returnPoint != null ? returnPoint.position : Vector3.zero);
            var col = st.gameObject.AddComponent<SphereCollider>(); col.isTrigger = true; col.radius = 1.6f;
            var vs = st.gameObject.AddComponent<VirtualMissionStation>(); vs.runner = r; vs.step = steps[i];
            list.Add(vs);
        }
        r.stations = list.ToArray();
        r.MakeBeacon();
        Debug.Log($"[Virtualización] «{m.title}» en el área real: {steps.Length} pasos, guiados con una baliza.");
        return r;
    }

    void MakeBeacon()
    {
        var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = "Baliza_virtualizacion";
        Destroy(b.GetComponent<Collider>());
        b.transform.SetParent(transform, false);
        b.transform.localScale = new Vector3(0.25f, 6f, 0.25f);
        Renderer rd = b.GetComponent<Renderer>();
        if (rd != null) rd.material.color = new Color(1f, 0.85f, 0.3f, 0.6f);
        _beacon = b.transform;
    }

    public bool IsNext(VirtualMissionStation s) => !done && next < stations.Length && stations[next] == s;

    public void Complete(VirtualMissionStation s)
    {
        if (!IsNext(s)) return;
        s.done = true; next++;
        Debug.Log($"[Virtualización] «{title}»: paso {next}/{stations.Length} — {s.step}.");
        onStep?.Invoke(next - 1);
        if (next < stations.Length) return;
        done = true;
        WorkRoster.Mission m = Pending != null && VirtualMissions.Key(Pending.id) == key ? Pending : null;
        if (m != null && WorkdayDirector.Instance != null) WorkdayDirector.Instance.PlayerVirtualizes(m);
        Debug.Log($"[Virtualización] Misión «{title}» completada.");
        Pending = null;
        _exitAt = Time.time + 3f;
    }

    void Update()
    {
        if (_beacon != null)
        {
            bool show = !done && next < stations.Length;
            _beacon.gameObject.SetActive(show);
            if (show) _beacon.position = stations[next].transform.position + Vector3.up * (3.2f + Mathf.Sin(Time.time * 3f) * 0.2f);
        }
        if (_exitAt > 0f && Time.time >= _exitAt)
        {
            _exitAt = -1f;
            if (inPlace)
            {
                // Se «despierta» delante de la máquina: la virtualización termina.
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null && returnPoint != null) WorkdayDirector.Teleport(p.transform, returnPoint.position + Vector3.up * 0.1f);
                Destroy(gameObject);
            }
            else if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) MobWorldLoader.Instance.ExitMobWorld();
        }
    }

    // Hacia dónde queda el siguiente paso, en palabras (delante, a la izquierda…) y a cuántos metros.
    string Direction()
    {
        if (next >= stations.Length) return "";
        if (_player == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); _player = p != null ? p.transform : null; }
        Camera cam = Camera.main;
        if (_player == null || cam == null) return "";
        Vector3 d = stations[next].transform.position - _player.position; d.y = 0f;
        float dist = d.magnitude;
        if (dist < 1.8f) return "<color=#8FE08F>¡Aquí! Pulsa F</color>";
        Vector3 f = cam.transform.forward; f.y = 0f;
        float ang = Vector3.SignedAngle(f, d, Vector3.up);
        string where = Mathf.Abs(ang) < 30f ? "delante" : Mathf.Abs(ang) > 150f ? "detrás de ti" : ang > 0f ? "a tu derecha" : "a tu izquierda";
        return $"a {dist:0} m, {where} (sigue la luz)";
    }

    GUIStyle _style;
    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint || stations == null) return;
        if (_style == null) _style = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true, wordWrap = true };
        float h = 60 + stations.Length * 20 + 20;
        GUI.Box(new Rect(12, 80, 430, h), "");
        GUI.Label(new Rect(22, 86, 410, 20), $"<b>Virtualización · {title}</b>", _style);
        for (int i = 0; i < stations.Length; i++)
        {
            string mark = i < next ? "<color=#8FE08F>✓</color>" : i == next ? "<color=#FFD27A>▶</color>" : "·";
            GUI.Label(new Rect(22, 108 + i * 20, 410, 20), $"{mark} {i + 1}. {stations[i].step}", _style);
        }
        string help = done ? "<color=#8FE08F>¡Completada! Volviendo a la máquina…</color>"
                           : $"<b>Ahora:</b> {stations[next].step} — {Direction()}";
        GUI.Label(new Rect(22, 112 + stations.Length * 20, 410, 44), help, _style);
    }
}
