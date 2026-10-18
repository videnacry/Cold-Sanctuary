using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The virtualization machine — the universal trigger for mob / Microcosmos missions,
/// present in every area (docs §3). Replaces the door-based KitchenEntrance flow.
///
/// Flow:
///   Player approaches → InteractionController shows the prompt.
///   Interact → ConfirmationPanel "¿Entrar a la simulación?"
///   Confirm  → onEnterAnimation (player enters machine + closes eyes) → MeditationSession.Open()
///              → black screen → mission menu → chosen mission plays.
///   Interact again while in a mission → ends it (MeditationSession.EndMission()).
/// </summary>
[RequireComponent(typeof(Collider))]
public class VirtualizationMachine : MonoBehaviour, IInteractable
{
    [Header("Area")]
    public string areaName = "esta zona";

    [Header("References")]
    [Tooltip("This area's reality-shift driver.")]
    public RealityShiftController shift;

    [Tooltip("Missions offered by this machine (only isAvailable ones are listed). Modo misión in-place.")]
    public MobMission[] missions;

    [Header("Mundo mob (escena propia)")]
    [Tooltip("Si se rellena, la máquina carga esta ESCENA de mundo mob (MobWorldLoader) en vez del " +
             "menú de misiones in-place. Debe estar en Build Settings (ver MobWorldSceneBuilder).")]
    public string mobWorldSceneName = "";

    [Header("Cinematic")]
    [Tooltip("Player entering the machine and closing eyes. Plays before the screen goes black.")]
    public UnityEvent onEnterAnimation;

    bool SceneMode => !string.IsNullOrEmpty(mobWorldSceneName);

    // ── IInteractable ─────────────────────────────────────────────────────────

    public string InteractLabel =>
        (!SceneMode && MeditationSession.Instance.IsInMission)
            ? "Salir de la simulación"
            : $"Usar la máquina de {areaName} (virtualización o simulación)";

    public bool CanInteract =>
        SceneMode
            ? (MobWorldLoader.Instance != null && !MobWorldLoader.Instance.IsBusy && !MobWorldLoader.Instance.IsInMobWorld)
            : (shift != null && !MeditationSession.Instance.IsBusy);

    public void Interact()
    {
        // VIRTUALIZACIÓN o SIMULACIÓN: la máquina sirve para las dos. Primero se elige (docs/jornada-y-rotacion.md).
        if (WorkdayDirector.Instance != null && _menu == 0 && !(!SceneMode && MeditationSession.Instance.IsInMission)) { _menu = 1; GameKeys.MenuOpen = true; return; }
        EnterSimulation();
    }

    /// <summary>SIMULACIÓN: el microcosmos (o el mundo mob) del área.</summary>
    public void EnterSimulation()
    {
        // Modo escena: cargar el mundo mob propio (salida vía WorldExitPortal dentro de la escena).
        if (SceneMode)
        {
            Confirm(() => EnterWithTank(GameObject.FindGameObjectWithTag("Player")?.transform));
            return;
        }

        // Modo in-place (menú de misiones + RealityShiftController).
        if (shift == null) return;
        if (MeditationSession.Instance.IsInMission) { MeditationSession.Instance.EndMission(); return; }
        Confirm(OpenSession);
    }

    /// <summary>Entra al microcosmos POR EL TANQUE: máscara, subir, sumergirse y entonces cargar la escena. Sin tanque, directo.</summary>
    public void EnterWithTank(Transform who)
    {
        VirtualizationTank tank = GetComponentInChildren<VirtualizationTank>();
        if (tank != null && who != null) StartCoroutine(tank.Enter(who, () => MobWorldLoader.Instance.EnterMobWorld(mobWorldSceneName)));
        else MobWorldLoader.Instance.EnterMobWorld(mobWorldSceneName);
    }

    void Confirm(System.Action onConfirm)
    {
        if (ConfirmationPanel.Instance != null)
            ConfirmationPanel.Instance.Show(
                title:       "Máquina de virtualización",
                message:     $"¿Entrar a la simulación de {areaName}?",
                onConfirm:   () => onConfirm(),
                onCancel:    null,
                confirmText: "Entrar",
                cancelText:  "Cancelar");
        else
            onConfirm(); // fallback: no panel wired
    }

    void OpenSession()
    {
        MeditationSession.Instance.Open(
            missions,
            shift,
            enterAnimation: () => onEnterAnimation?.Invoke());
    }

    // ── Menú: 1 virtualización (misiones de trabajo del área) · 2 simulación (microcosmos) ─────────────────
    int _menu;   // 0 cerrado · 1 elegir modo · 2 elegir misión
    System.Collections.Generic.List<WorkRoster.Mission> _list;
    string _toast; float _toastUntil;

    void Update()
    {
        if (_menu == 0) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
        int k = -1;
        for (int i = 1; i <= 9; i++) if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i)) k = i;
        if (k < 0) return;
        if (_menu == 1)
        {
            if (k == 1) { _list = AreaWork.OfArea(areaName, ConstructionProject.All, TimeController.timeController != null ? TimeController.timeController.Day : 0); _menu = 2; }
            else if (k == 2) { Close(); EnterSimulation(); }
        }
        else if (_menu == 2 && _list != null && k <= _list.Count)
        {
            WorkRoster.Mission m = _list[k - 1];
            WorkdayDirector.Instance.PlayerVirtualizes(m);
            _toast = $"Virtualización: «{m.title}» completada."; _toastUntil = Time.time + 4f;
            Close();
        }
    }

    void Close() { _menu = 0; GameKeys.MenuOpen = false; }

    void OnGUI()
    {
        if (_menu == 0) { if (Time.time < _toastUntil) GUI.Box(new Rect(Screen.width * 0.5f - 220, 60, 440, 28), _toast); return; }
        var lines = new System.Collections.Generic.List<string>();
        if (_menu == 1)
        {
            lines.Add($"Máquina de {areaName}");
            lines.Add("1 · Virtualización: misiones de trabajo del área");
            lines.Add("2 · Simulación: el microcosmos");
        }
        else
        {
            lines.Add($"Virtualización · {areaName}");
            if (_list == null || _list.Count == 0) lines.Add("(esta área aún no tiene misiones de trabajo)");
            else for (int i = 0; i < _list.Count && i < 9; i++) lines.Add($"{i + 1} · {_list[i].title} ({_list[i].hours:0.#} h)");
        }
        lines.Add("Esc · salir");
        float w = 520, h = 26 + lines.Count * 22;
        GUI.Box(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.5f - h * 0.5f, w, h), "");
        for (int i = 0; i < lines.Count; i++) GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f + 12, Screen.height * 0.5f - h * 0.5f + 8 + i * 22, w - 24, 22), lines[i]);
    }

    // ── Startup / Gizmos ────────────────────────────────────────────────────────

    void Start()
    {
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger) col.isTrigger = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.35f);
        var col = GetComponent<Collider>();
        if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
