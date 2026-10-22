using UnityEngine;

/// <summary>
/// LA CONSOLA DEL REACTOR (sala de control): F → se abre el panel. Es el interactuable principal de la planta:
/// - 1-4 elige barra (regulación, shim 1, shim 2, transitoria); ↑/↓ la saca o la mete (el mecanismo es lento a propósito).
/// - R rearma tras un disparo (solo con todas las barras dentro); X = SCRAM manual; P = bomba del primario;
///   M = modo automático (la barra de regulación sigue la potencia pedida); +/− cambia esa potencia; Esc cierra.
/// Muestra lo que muestra una consola real: potencia (lineal y logarítmica), PERIODO, reactividad (en dólares),
/// temperatura del combustible y de la piscina, posición de barras y conductividad. El arranque enseña la física:
/// subcrítico con la fuente → crítico (la potencia se mantiene sola) → supercrítico con periodo > 10 s → la
/// temperatura frena la subida (coeficiente negativo). Sin dosímetro no se opera.
/// </summary>
public class ReactorConsole : MonoBehaviour, IInteractable
{
    public ResearchReactor reactor;

    bool _open;
    int _rod = 1;
    static readonly string[] RodNames = { "Regulación", "Shim 1", "Shim 2", "Transitoria" };

    public string InteractLabel => "Consola del reactor";
    public bool CanInteract => !_open;

    public void Interact()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && p.GetComponent<PersonalDosimeter>() == null) { PlantNotice.Say("Sin dosímetro no se opera el reactor: cógelo en la taquilla de la entrada."); return; }
        if (reactor == null) reactor = ResearchReactor.Instance;
        if (reactor == null) return;
        _open = true; GameKeys.MenuOpen = true;
    }

    void Close() { _open = false; GameKeys.MenuOpen = false; }

    void OnDisable() { if (_open) Close(); }

    void Update()
    {
        if (!_open || reactor == null) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
        for (int i = 0; i < 4; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) _rod = i;
        float dir = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
        if (dir != 0f && !(reactor.autoMode && _rod == 0)) reactor.MoveRod((ResearchReactor.Rod)_rod, dir, Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.X)) reactor.Scram("manual");
        if (Input.GetKeyDown(KeyCode.R) && !reactor.Rearm()) PlantNotice.Say("Para rearmar, todas las barras tienen que estar dentro.");
        if (Input.GetKeyDown(KeyCode.P)) reactor.pumpOn = !reactor.pumpOn;
        if (Input.GetKeyDown(KeyCode.M) && !reactor.scrammed) reactor.autoMode = !reactor.autoMode;
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus)) reactor.setpoint = Mathf.Min(1e6f, reactor.setpoint * 2f);
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) reactor.setpoint = Mathf.Max(1f, reactor.setpoint * 0.5f);
    }

    void OnGUI()
    {
        if (!_open || reactor == null) return;
        ResearchReactor r = reactor;
        Rect box = new Rect(Screen.width * 0.5f - 300f, Screen.height * 0.5f - 230f, 600f, 460f);
        GUI.Box(box, "Consola del reactor TRIGA");
        GUILayout.BeginArea(new Rect(box.x + 16f, box.y + 26f, box.width - 32f, box.height - 36f));
        GUIStyle s = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true, wordWrap = true };

        string state = r.scrammed ? $"<color=#ff6060>DISPARADO (SCRAM): {r.scramReason}</color>" :
                       r.Reactivity() < -0.0005f ? "subcrítico" : r.Reactivity() > 0.0005f ? "<color=#ffd060>supercrítico</color>" : "<color=#80ff80>crítico</color>";
        string period = float.IsInfinity(r.periodSeconds) || r.periodSeconds > 999f ? "∞" : $"{r.periodSeconds:0.0} s";
        GUILayout.Label($"Estado: {state}", s);
        GUILayout.Label($"Potencia: <b>{ResearchReactor.FormatPower(r.power)}</b>   (log: {Mathf.Log10(Mathf.Max(r.power, 1e-6f)):0.0})   ·   periodo: {period}", s);
        GUILayout.Label($"Reactividad: {r.Reactivity() * 1e5f:0} pcm = {r.Dollars:0.00} $   (1 $ = β = {r.beta * 100f:0.00} %: más allá, supercrítico inmediato)", s);
        GUILayout.Label($"Combustible: {r.fuelTemp:0} °C (disparo a {r.fuelTempLimit:0})   ·   piscina: {r.poolTemp:0.0} °C (disparo a {r.poolTempLimit:0})", s);
        GUILayout.Label($"Bomba del primario: {(r.pumpOn ? "en marcha" : "<color=#ffd060>PARADA</color>")}   ·   conductividad del agua: {(r.conductivity > 2f ? "<color=#ffd060>" : "")}{r.conductivity:0.00} µS/cm{(r.conductivity > 2f ? " (cambiar resinas)</color>" : "")}", s);
        GUILayout.Label($"Flujo en la posición de irradiación: {r.Flux:0.0e0} n/cm²·s", s);
        GUILayout.Space(6f);
        for (int i = 0; i < 4; i++)
        {
            int bars = Mathf.RoundToInt(r.rodPos[i] * 20f);
            string sel = i == _rod ? "▶ " : "   ";
            string auto = i == 0 && r.autoMode ? $"  (AUTO → {ResearchReactor.FormatPower(r.setpoint)})" : "";
            GUILayout.Label($"{sel}{i + 1}. {RodNames[i]}: [{new string('█', bars)}{new string('·', 20 - bars)}] {r.rodPos[i] * 100f:0}% fuera{auto}", s);
        }
        GUILayout.Space(6f);
        GUILayout.Label("<i>1-4 barra · ↑/↓ sacar/meter · R rearmar · X SCRAM · P bomba · M automático · +/− potencia pedida · Esc cerrar</i>", s);
        GUILayout.Label(Hint(r), s);
        GUILayout.EndArea();
    }

    static string Hint(ResearchReactor r)
    {
        if (r.scrammed && r.scramReason != "parado") return "Tras un disparo: mete todas las barras (↓) y rearma con R. Piensa qué lo causó.";
        if (r.scrammed) return "Arranque: rearma (R) y saca las barras de compensación poco a poco. Mira cómo la fuente se multiplica al acercarte a crítico.";
        if (r.periodSeconds < 10f) return "<color=#ffd060>Periodo corto (< 10 s): la potencia sube demasiado deprisa. Mete un poco la barra.</color>";
        if (r.power > 5e5f && !r.pumpOn) return "<color=#ffd060>A esta potencia, sin bomba, la piscina se calienta.</color>";
        if (r.fuelTemp > 150f) return "La temperatura del combustible frena la reacción sola: es el coeficiente negativo del UZrH.";
        return "Estable cuando el periodo es ∞: tantos neutrones nacen como se pierden (k = 1).";
    }
}
