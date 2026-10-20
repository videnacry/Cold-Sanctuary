using UnityEngine;

/// <summary>Lleva la misión dentro de su escena: pasos en orden, HUD y, al terminar, la cuenta como hecha y sale.</summary>
public class VirtualMissionRunner : MonoBehaviour
{
    /// <summary>La misión que la máquina mandó practicar (la escena la lee al empezar).</summary>
    public static WorkRoster.Mission Pending;
    public static VirtualMissionRunner Active { get; private set; }

    public string key;
    public string title;
    public VirtualMissionStation[] stations;
    public int next;
    public bool done;
    float _exitAt = -1f;

    void OnEnable() { Active = this; }
    void OnDisable() { if (Active == this) Active = null; }

    public bool IsNext(VirtualMissionStation s) => !done && next < stations.Length && stations[next] == s;

    public void Complete(VirtualMissionStation s)
    {
        if (!IsNext(s)) return;
        s.done = true; next++;
        Debug.Log($"[Virtualización] «{title}»: paso {next}/{stations.Length} — {s.step}.");
        if (next < stations.Length) return;
        done = true;
        WorkRoster.Mission m = Pending != null && VirtualMissions.Key(Pending.id) == key ? Pending : null;
        if (m != null && WorkdayDirector.Instance != null) WorkdayDirector.Instance.PlayerVirtualizes(m);
        Debug.Log($"[Virtualización] Misión «{title}» completada en su escena.");
        Pending = null;
        _exitAt = Time.time + 3f;   // sale sola al poco (también se puede usar el portal)
    }

    void Update()
    {
        if (_exitAt > 0f && Time.time >= _exitAt)
        {
            _exitAt = -1f;
            if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) MobWorldLoader.Instance.ExitMobWorld();
        }
    }

    GUIStyle _style;
    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;
        if (_style == null) _style = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true };
        float h = 30 + stations.Length * 20 + 20;
        GUI.Box(new Rect(12, 80, 400, h), "");
        GUI.Label(new Rect(22, 86, 380, 20), $"<b>Virtualización · {title}</b>", _style);
        for (int i = 0; i < stations.Length; i++)
        {
            string mark = i < next ? "<color=#8FE08F>✓</color>" : i == next ? "<color=#FFD27A>▶</color>" : "·";
            GUI.Label(new Rect(22, 108 + i * 20, 380, 20), $"{mark} {i + 1}. {stations[i].step}", _style);
        }
        GUI.Label(new Rect(22, 108 + stations.Length * 20, 380, 20), done ? "<color=#8FE08F>¡Completada! Volviendo…</color>" : "Ve al puesto marcado y pulsa F.", _style);
    }
}
