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
            : (shift == null || !MeditationSession.Instance.IsBusy);   // sin microcosmos: solo virtualización

    public void Interact()
    {
        // VIRTUALIZACIÓN o SIMULACIÓN: la máquina sirve para las dos. Primero se elige (docs/jornada-y-rotacion.md).
        if (!SceneMode && MeditationSession.Instance.IsInMission) { EnterSimulation(); return; }   // dentro de una misión in-place: salir
        ShowMenu();
    }

    /// <summary>SIMULACIÓN: el microcosmos (o el mundo mob) del área.</summary>
    public void EnterSimulation()
    {
        // Modo escena: cargar el mundo mob propio (salida vía WorldExitPortal dentro de la escena).
        if (SceneMode)
        {
            EnterWithTank(GameObject.FindGameObjectWithTag("Player")?.transform);   // ya se eligió en el diálogo
            return;
        }

        // Modo in-place (menú de misiones + RealityShiftController).
        if (shift == null) return;
        if (MeditationSession.Instance.IsInMission) { MeditationSession.Instance.EndMission(); return; }
        Confirm(OpenSession);
    }

    /// <summary>Entra al microcosmos POR EL TANQUE: máscara, subir, sumergirse y entonces cargar la escena. Sin tanque, directo.</summary>
    public void EnterWithTank(Transform who, string scene = null)
    {
        scene = string.IsNullOrEmpty(scene) ? mobWorldSceneName : scene;
        VirtualizationTank tank = GetComponentInChildren<VirtualizationTank>();
        if (tank != null && who != null) StartCoroutine(tank.Enter(who, () => MobWorldLoader.Instance.EnterMobWorld(scene)));
        else MobWorldLoader.Instance.EnterMobWorld(scene);
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

    // ── Diálogo al TOCAR la máquina (o F): virtualización (misiones de trabajo, cada una en su escena) o simulación ──
    // Se elige con ↑/↓ + Intro o con clic (ChoiceDialog). Todas las misiones del catálogo son accesibles desde cualquier
    // máquina (las áreas sin máquina, como el laboratorio, el gimnasio, la obra o la guardia, por «otras áreas»).
    string _toast; float _toastUntil;

    System.Collections.Generic.List<WorkRoster.Mission> TodayMissions() =>
        AreaWork.Today(TimeController.timeController != null ? TimeController.timeController.Day : 0, ConstructionProject.All);

    /// <summary>Abre el diálogo de la máquina.</summary>
    public void ShowMenu()
    {
        if (ChoiceDialog.IsOpen) return;
        // Cada máquina, SOLO lo de su área: sus misiones de trabajo (virtualización) y su microcosmos (simulación), si lo hay.
        var opts = new System.Collections.Generic.List<string>();
        var acts = new System.Collections.Generic.List<System.Action>();
        if (areaName == "Cocina") { opts.Add("Virtualización: servicio a los turistas (eres el cocinero)"); acts.Add(() => ChooseId("coc_servicio")); }
        if (areaName == AreaWork.Hospital) { opts.Add("Virtualización: iniciar a un turista (análisis, vacuna y normas)"); acts.Add(() => ChooseId("enf_iniciacion")); }
        opts.Add("Virtualización: misiones de trabajo de esta área"); acts.Add(() => ShowArea(areaName));
        if (SceneMode || shift != null) { opts.Add("Simulación: el microcosmos del área"); acts.Add(EnterSimulation); }
        // Simulaciones de COMPASIÓN (docs/compassion-simulations.md): momentos históricos reales, sobre esta misma área agrandada.
        if (areaName == "Cocina") { opts.Add("Simulación: Le Chambon-sur-Lignon, 1942 (el pueblo que escondió a miles)"); acts.Add(() => EnterWithTank(GameObject.FindGameObjectWithTag("Player")?.transform, "Microcosmos_LeChambon")); }
        if (areaName == AreaWork.Hospital) { opts.Add("Simulación: Irena Sendler, Varsovia 1942 (los niños del gueto)"); acts.Add(() => EnterWithTank(GameObject.FindGameObjectWithTag("Player")?.transform, "Microcosmos_IrenaSendler")); }
        // MITO (docs/microcosmos-myrmidons.md): Egina y los mirmidones, sobre la Construcción agrandada.
        if (areaName == "Construcción") { opts.Add("Simulación: Egina, el reino de los mirmidones (Éaco y Zeus)"); acts.Add(() => EnterWithTank(GameObject.FindGameObjectWithTag("Player")?.transform, "Microcosmos_Egina")); }
        // FÍSICA (docs/atomic-plant.md): la escalera de la física vive en la planta atómica. «El microcosmos del área» (arriba) es el
        // de Newton, Curie y Einstein; Eratóstenes, el primer tramo, tiene el suyo.
        if (areaName == "Planta Atómica" && Application.CanStreamedLevelBeLoaded("Microcosmos_Eratostenes")) { opts.Add("Simulación: Eratóstenes mide la Tierra (~240 a. C.)"); acts.Add(() => EnterWithTank(GameObject.FindGameObjectWithTag("Player")?.transform, "Microcosmos_Eratostenes")); }
        // HISTORIAS (docs/microcosmos-historias.md): grandes figuras sobre el área que mejor las cuenta.
        foreach (var (area, label, scene) in StoryScenes)
            if (areaName == area) { string sc = scene; opts.Add(label); acts.Add(() => EnterWithTank(GameObject.FindGameObjectWithTag("Player")?.transform, sc)); }
        ChoiceDialog.Show($"Máquina de virtualización · {areaName}", opts.ToArray(), i => acts[i](), null, transform, 4.5f);
    }

    static readonly (string area, string label, string scene)[] StoryScenes =
    {
        ("Sala de Yoga", "Simulación: Patanjali y los ocho miembros del yoga", "Microcosmos_Patanjali"),
        ("Gimnasio",     "Simulación: la tregua de Navidad, Flandes 1914 (Primera Guerra Mundial)", "Microcosmos_Tregua1914"),
        ("Enfermería",   "Simulación: Florence Nightingale en Escútari, 1854", "Microcosmos_Nightingale"),
        ("Enfermería",   "Simulación: Siddhārtha y los cuatro encuentros (Buda)", "Microcosmos_Buda"),
        ("Mecánica",     "Simulación: los hermanos Wright, de la bicicleta al avión (1903)", "Microcosmos_Wright"),
    };

    void ShowArea(string area)
    {
        var list = new System.Collections.Generic.List<WorkRoster.Mission>();
        foreach (WorkRoster.Mission m in TodayMissions()) if (m.area == area) list.Add(m);
        var opts = new System.Collections.Generic.List<string>();
        foreach (WorkRoster.Mission m in list) opts.Add($"{m.title} ({m.hours:0.#} h)");
        opts.Add("← Volver");
        ChoiceDialog.Show($"Virtualización · {area}", opts.ToArray(), i => { if (i >= list.Count) ShowMenu(); else Choose(list[i]); }, ShowMenu, transform, 4.5f);
    }

    void ChooseId(string id)
    {
        foreach (WorkRoster.Mission m in TodayMissions()) if (m.id == id) { Choose(m); return; }
        foreach (WorkRoster.Mission m in AreaWork.Catalog) if (m.id == id) { Choose(m.Copy()); return; }
    }

    void Choose(WorkRoster.Mission m)
    {
        // Cada virtualización tiene SU ESCENA (Virtual_<clave>), con una copia del área real y de su gente: se entra por el
        // tanque y se carga. (Si aún no se ha generado, se juega en el área real como respaldo.)
        string scene = VirtualMissions.SceneName(m.id);
        if (Application.CanStreamedLevelBeLoaded(scene) && MobWorldLoader.Instance != null && !MobWorldLoader.Instance.IsInMobWorld)
        {
            VirtualMissionRunner.Pending = m;
            EnterWithTank(GameObject.FindGameObjectWithTag("Player")?.transform, scene);
            return;
        }
        Transform who = GameObject.FindGameObjectWithTag("Player")?.transform;
        VirtualizationTank tank = GetComponentInChildren<VirtualizationTank>();
        Transform front = tank != null && tank.front != null ? tank.front : transform;
        Transform area = transform.parent != null ? transform.parent : transform;
        void Go()
        {
            if (who != null) WorkdayDirector.Teleport(who, front.position + Vector3.up * 0.1f);
            VirtualInPlace.Begin(m, area, front);
        }
        if (tank != null && who != null) StartCoroutine(tank.Enter(who, Go)); else Go();
    }

    // Al TOCARLA el jugador se muestra el diálogo (no hace falta pulsar nada).
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !CanInteract) return;
        if (MobWorldLoader.HasInstance && (MobWorldLoader.Instance.IsBusy || MobWorldLoader.Instance.IsInMobWorld)) return;
        ShowMenu();
    }

    void OnGUI()
    {
        if (Time.time < _toastUntil) GUI.Box(new Rect(Screen.width * 0.5f - 220, 60, 440, 28), _toast);
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
