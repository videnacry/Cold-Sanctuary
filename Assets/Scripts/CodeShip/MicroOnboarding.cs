using UnityEngine;

/// <summary>
/// LA GUÍA DE INICIO (CodeShip ux-acciones.md §1 y misiones.md: una carta abre la misión y deja el objetivo marcado; docs/plan-microcosmos-
/// realismo.md §4.5; Berón 2026-11-18: «no sé qué hacer ni cómo jugar»). La primera vez que se entra en un microcosmos con historia, al
/// cerrar su tarjeta de intro, cuatro avisos cortos, de uno en uno, en el mismo sitio (arriba en el centro, sin tapar los mandos):
///  1. qué hay que hacer ahora (el objetivo, que sigue fijo arriba a la izquierda);
///  2. dónde está: el menú ☰ › Tareas (el botón del menú se marca);
///  3. «Ir» te lleva andando; «Parar» o moverte lo para;
///  4. al llegar, la acción se hace sola o con E; los retos de código, en su hoja.
/// Cada aviso pasa solo cuando el jugador hace lo que dice, con «Entendido», o a los <see cref="StepSeconds"/> s (ninguna espera sin tope).
/// «Saltar guía» la cierra. Se recuerda por escena (PlayerPrefs); en el menú, «Guía: qué hago aquí» la repite.
/// </summary>
public class MicroOnboarding : MonoBehaviour
{
    public const float StepSeconds = 40f;
    /// <summary>Dónde está el botón del menú (lo pone el menú al dibujarse): la guía lo marca.</summary>
    public static Rect MenuButton;
    /// <summary>El aviso que se ve (−1 = ninguno): pruebas.</summary>
    public static int Step { get; private set; } = -1;

    static MicroOnboarding _inst;
    string _scene = "";
    float _since;
    int _startStoryStep, _startArrivals;
    GUIStyle _st, _btn;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (_inst != null) return;
        var go = new GameObject("MicroOnboarding");
        DontDestroyOnLoad(go);
        _inst = go.AddComponent<MicroOnboarding>();
    }

    static string Key(string scene) => "cs.guia." + scene;

    /// <summary>La guía otra vez, desde el principio (menú y ajustes de Tareas).</summary>
    public static void Restart()
    {
        StoryMission sm = StoryMission.Instance;
        if (sm == null || _inst == null) return;
        PlayerPrefs.DeleteKey(Key(sm.gameObject.scene.name));
        _inst.Begin(sm);
    }

    /// <summary>Lo que hizo el jugador («menu», «tareas», «ir», «llegar»): si es lo que pide el aviso, pasa al siguiente.</summary>
    public static void Notice(string what)
    {
        if (_inst == null || Step < 0) return;
        if ((Step == 1 && (what == "menu" || what == "tareas")) || (Step == 2 && what == "ir")) _inst.Next();
    }

    void Begin(StoryMission sm)
    {
        _scene = sm.gameObject.scene.name;
        Step = 0; _since = Time.unscaledTime;
        _startStoryStep = sm.Step; _startArrivals = AutoTravel.Arrivals;
        Debug.Log($"[Guía] Empieza la guía de inicio en {_scene}.");
    }

    void Next()
    {
        Step++; _since = Time.unscaledTime;
        if (Step > 3) Done();
    }

    void Done()
    {
        if (_scene.Length > 0) PlayerPrefs.SetInt(Key(_scene), 1);
        Step = -1;
        Debug.Log("[Guía] Guía de inicio terminada.");
    }

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running) { Step = -1; return; }
        StoryMission sm = StoryMission.Instance;
        if (sm == null) { Step = -1; return; }
        if (Step < 0)
        {
            // La primera vez en esta escena, al cerrarse la tarjeta de intro.
            if (!sm.CardShowing && !sm.Finished && sm.gameObject.scene.name != _scene && PlayerPrefs.GetInt(Key(sm.gameObject.scene.name), 0) == 0) Begin(sm);
            return;
        }
        if (sm.gameObject.scene.name != _scene) { Step = -1; return; }
        // Pasa solo cuando el jugador lo hace.
        if (Step == 1 && (MicroRootMenu.Open || HudModals.Current == HudModals.Modal.Tareas)) Next();
        else if (Step == 2 && AutoTravel.Instance != null && (AutoTravel.Instance.Traveling || AutoTravel.Arrivals > _startArrivals)) Next();
        else if (Step == 3 && (sm.Step != _startStoryStep || CodeChallengeSheet.IsOpen)) Next();
        if (Step >= 0 && Time.unscaledTime - _since > StepSeconds) Next();
    }

    string Text()
    {
        StoryMission sm = StoryMission.Instance;
        string goal = sm != null && sm.CurrentStep != null ? sm.CurrentStep.goal : "";
        switch (Step)
        {
            case 0: return $"<b>Qué hacer ahora</b>: {goal}.\nEl objetivo se queda arriba a la izquierda y cambia con cada paso de la historia.";
            case 1: return $"<b>Dónde</b>: abre {MicroRootMenu.KeyHint} y toca <b>Misiones › Tareas</b>. Ahí está todo lo que hay que hacer, con su distancia.";
            case 2: return "<b>Ir</b>: toca «Ir» en una tarea y te lleva andando. «■ Parar», o moverte tú, para el viaje.";
            case 3: return WebLigera.Active
                ? "<b>Lo de ahora</b> está siempre abajo en el centro: el botón de la acción principal. Cada puesto (cocina, botiquín, campo, taller, yoga, el perro) tiene su misión de <b>retos de código</b>, uno tras otro."
                : "<b>Lo de ahora</b> está siempre abajo en el centro: el botón de la acción principal, o <b>R</b> (la historia, con <b>E</b>). Cada puesto (cocina, botiquín, campo, taller, yoga, el perro) tiene su misión de <b>retos de código</b>, uno tras otro.";
            default: return "";
        }
    }

    void OnGUI()
    {
        if (Step < 0 || ColdSanctuary.TestRunner.Running || CodeChallengeSheet.IsOpen) return;
        bool big = HudModals.Current == HudModals.Modal.Tareas || HudModals.Current == HudModals.Modal.Areas || HudModals.Current == HudModals.Modal.Misiones;
        if (big) return;
        if (_st == null)
        {
            _st = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true, fontSize = WebLigera.Active ? 16 : 14 };
            _btn = new GUIStyle(GUI.skin.button) { fontSize = WebLigera.Active ? 15 : 13 };
        }
        float u = WebLigera.Active ? Mathf.Max(44f, Mathf.Min(Screen.width, Screen.height) * 0.07f) : 28f;
        float w = Mathf.Min(480f, Screen.width - 16f);
        float textH = _st.CalcHeight(new GUIContent(Text()), w - 20f);
        float h = textH + u + 22f;
        float y = Mathf.Max(AutoTravel.BarTop + u + 24f, Screen.height * 0.26f);
        var r = new Rect((Screen.width - w) * 0.5f, y, w, h);
        GUI.Box(r, ""); GUI.Box(r, ""); GUI.Box(r, "");
        GUI.Label(new Rect(r.x + 10f, r.y + 8f, w - 20f, textH), Text(), _st);
        GUI.Label(new Rect(r.x + 10f, r.yMax - u - 6f, w * 0.3f, u), $"<size=11>{Step + 1} de 4</size>", _st);
        if (GUI.Button(new Rect(r.xMax - u * 6.6f - 10f, r.yMax - u - 6f, u * 3.2f, u), "Saltar guía", _btn)) { Done(); return; }
        if (GUI.Button(new Rect(r.xMax - u * 3.2f - 6f, r.yMax - u - 6f, u * 3.2f, u), Step == 3 ? "✓ Entendido" : "Entendido", _btn)) { Next(); return; }
        // Paso 2: marca el botón del menú (un marco ámbar alrededor), para saber dónde tocar.
        if (Step == 1 && MenuButton.width > 0f)
        {
            Color c = GUI.color; GUI.color = new Color(1f, 0.8f, 0.3f, 0.5f + 0.5f * Mathf.PingPong(Time.unscaledTime * 2f, 1f));
            Rect m = new Rect(MenuButton.x - 4f, MenuButton.y - 4f, MenuButton.width + 8f, MenuButton.height + 8f);
            GUI.DrawTexture(new Rect(m.x, m.y, m.width, 3f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(m.x, m.yMax - 3f, m.width, 3f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(m.x, m.y, 3f, m.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(m.xMax - 3f, m.y, 3f, m.height), Texture2D.whiteTexture);
            GUI.color = c;
        }
    }
}
