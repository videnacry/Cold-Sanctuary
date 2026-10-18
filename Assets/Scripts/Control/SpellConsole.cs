using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LANZADOR DE HECHIZOS POR TECLADO (docs/typed-spells-and-queues.md) — la otra cara del principio "toda acción es un
/// hechizo": el jugador puede **escribir** hechizos además de pulsar botones. Mientras POSEE a un anima, abre la consola
/// (tecla `~`) y escribe, p. ej.: <c>walkTo({destiny:"Marilia", maxSpeed:true})</c>. Se resuelve el **destino** por
/// nombre (otra anima que el poseído ALCANCE a ver, o un área/gameobject por su `name`) y se **encola** en la cola del
/// jugador (<see cref="ActionQueue"/>). Si no lo encuentra/ve, el poseído "piensa" <c>¿Dónde está X?</c>. Es un
/// lanzador de FUNCIONES: `walkTo` hoy; el registro se amplía (tomar, agachar, posturas de yoga por bodyPart…).
///
/// Prototipo OnGUI (campo de texto). El teclado virtual y la UI declarativa son el paso siguiente.
/// </summary>
public class SpellConsole : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.BackQuote;   // ~ abre/cierra la consola
    [Tooltip("Alcance base de 'visión' para resolver destinos-anima (×percepción del poseído).")]
    [Min(1f)] public float baseVisibleRange = 20f;

    bool _open;
    int _openedFrame = -1;
    string _input = "";
    readonly List<string> _log = new List<string>();

    /// <summary>¿El lanzador está abierto? Mientras lo esté, el teclado es SOLO suyo (<see cref="GameKeys"/> enmudece).</summary>
    public static bool Capturing { get; private set; }

    // Respuesta del último hechizo, visible unos segundos aunque el lanzador esté cerrado.
    string _feedback; float _feedbackUntil; bool _feedbackBad;

    void Update()
    {
        if (!_open)
        {
            // ~ o Intro abren el lanzador (con el hechizo propuesto ya escrito: otro Intro lo lanza).
            if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Open();
        }
        else if (Time.frameCount != _openedFrame)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(toggleKey)) { _input = ""; Close(); }   // salir SIN lanzar (y sin guardar lo escrito)
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Submit();
        }
        Capturing = _open;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        PollInbox();
#endif
    }

    void OnDisable() { Capturing = false; }

    void Open()
    {
        _open = true; _openedFrame = Time.frameCount; Capturing = true;
        if (string.IsNullOrEmpty(_input) && Proposed != null) _input = Proposed;
    }

    void Close() { _open = false; Capturing = false; GUI.FocusControl(null); }

    // Intro: lanza lo escrito y cierra (el teclado vuelve al juego). Con la barra vacía, solo cierra.
    void Submit()
    {
        string line = (_input ?? "").Replace("`", "").Replace("~", "").Trim();
        _input = "";
        Close();
        if (line.Length == 0) return;
        int before = _logSeq;
        Dispatch(line);
        string answer = _logSeq != before ? _log[_log.Count - 1] : $"Lanzado: {line}";
        Feedback(answer);
    }

    void Feedback(string s)
    {
        _feedback = s; _feedbackUntil = Time.time + 6f;
        string l = s.ToLowerInvariant();
        _feedbackBad = l.Contains("¿dónde") || l.Contains("no ") || l.Contains("necesita") || l.Contains("desconocido") || l.Contains("no conoces");
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // ENTRADA DIRECTA (pruebas en vivo): cada línea que aparezca en `<proyecto>/Temp/spell-inbox.txt` se lanza como si
    // se hubiera escrito en la consola, sin teclado ni ratón (`echo 'walkTo({destiny:"Nevera"})' >> Temp/spell-inbox.txt`).
    // El fichero se borra al leerlo; el resultado sale en el log como `[Hechizo] …`. Solo en editor / development build.
    float _nextPoll;

    void PollInbox()
    {
        if (Time.unscaledTime < _nextPoll) return;
        _nextPoll = Time.unscaledTime + 0.5f;
        string path = System.IO.Path.Combine(Application.dataPath, "..", "Temp", "spell-inbox.txt");
        if (!System.IO.File.Exists(path)) return;
        string[] lines;
        try { lines = System.IO.File.ReadAllLines(path); System.IO.File.Delete(path); }
        catch (System.IO.IOException) { return; }   // aún escribiéndose: se reintenta en el siguiente sondeo
        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            Debug.Log($"[Hechizo] (inbox) {line}");
            Dispatch(line);
        }
    }
#endif

    // GRIMORIO: catálogo de hechizos con su PLANTILLA por defecto (forma completa). El jugador no memoriza: escribe algo,
    // ve sugerencias (tolera fallos, ordena por similitud) y con Tab rellena la plantilla y cambia las properties.
    static readonly (string name, string template)[] Spells =
    {
        ("walkTo",    "walkTo({destiny:\"\", maxSpeed:false, distance:1})"),
        ("use",       "use({destiny:\"Nevera\"})"),
        ("take",      "take({item:\"Huevos\"})"),
        ("putIn",     "putIn({destiny:\"Plancha\"})"),
        ("beat",      "beat({destiny:\"Plancha\"})"),
        ("step",      "step"),
        ("bond",      "bond({destiny:\"\", amount:20})"),
        ("thought",   "thought({destiny:\"\", positive:true})"),
        ("yoga",      "yoga"),
        ("clean",     "clean"),
        ("feed",      "feed"),
        ("stock",     "stock"),
        ("teach",     "teach({destiny:\"\", skill:\"yoga\"})"),
        ("analysis",  "analysis({destiny:\"\"})"),
        ("vaccinate", "vaccinate({destiny:\"\"})"),
        ("status",    "status({visible:true, destiny:\"self\", seconds:20})"),
        ("open",      "open({destiny:\"\"})"),
        ("close",     "close({destiny:\"\"})"),
        ("virtualize", "virtualize({destiny:\"Huerto\"})"),
        ("lookAt",    "lookAt({destiny:\"Magnate\"})"),
        ("cancel",    "cancel({all:false})"),
        ("exit",      "exit"),
        ("look",      "look({yaw:90, pitch:0})"),
        ("asana",     "asana({name:\"\"})"),   // hacer el paso que el maestro pide (o una asana conocida por su nombre)
    };

    // ── Propuesta (lo que un maestro te pide) + qué hechizos CONOCE el poseído ───────────────────────────────────
    static string _proposed, _proposedWhy;
    static float _proposedUntil;

    /// <summary>Un maestro/misión PROPONE un hechizo: sale arriba de las sugerencias y en la barra; Tab (o Intro con la
    /// barra vacía) lo lanza.</summary>
    public static void Propose(string spellLine, string why = null, float seconds = 30f)
    { _proposed = spellLine; _proposedWhy = why; _proposedUntil = Time.time + seconds; }

    static string Proposed => !string.IsNullOrEmpty(_proposed) && Time.time < _proposedUntil ? _proposed : null;

    // Hechizos que se APRENDEN (id del Grimorio). Los demás son básicos (andar, mirar, usar, tomar…): siempre conocidos.
    static readonly Dictionary<string, string> RequiresSkill = new Dictionary<string, string>
    {
        { "yoga", "yoga" }, { "teach", "teach" }, { "analysis", "clinico" }, { "vaccinate", "clinico" },
    };

    public static bool KnowsSpell(Anima self, string spell)
    {
        if (!RequiresSkill.TryGetValue(spell, out string id)) return true;
        Grimoire g = self != null ? self.GetComponent<Grimoire>() : null;
        return g != null && g.Knows(id);
    }

    // BARRA SIEMPRE VISIBLE (cerrada): recuerda que se puede escribir y muestra el hechizo propuesto.
    void DrawClosedBar()
    {
        string p = Proposed;
        float w = 560f, h = p != null ? 46f : 26f, x = 12f, y = Screen.height - h - 12f;
        GUI.Box(new Rect(x - 4f, y - 2f, w + 8f, h + 4f), "");
        var st = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 13 };
        GUI.Label(new Rect(x, y, w, 22f), "<b>✦ Hechizos</b>  <size=11>(<b>Intro</b> o <b>~</b> para escribir)</size>", st);
        if (p != null) GUI.Label(new Rect(x, y + 22f, w, 22f), (_proposedWhy != null ? _proposedWhy + " → " : "") + $"<b>{p}</b>  <size=11>(Intro y otra vez Intro lo lanza)</size>", st);
        DrawFeedback(y);
    }

    // Respuesta del último hechizo (verde si salió, ámbar si no), justo encima de la barra.
    void DrawFeedback(float barY)
    {
        if (string.IsNullOrEmpty(_feedback) || Time.time > _feedbackUntil) return;
        var st = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 14, wordWrap = true };
        Rect r = new Rect(12f, barY - 34f, 560f, 28f);
        GUI.Box(new Rect(r.x - 4f, r.y - 2f, r.width + 8f, r.height + 4f), "");
        GUI.Label(r, $"<color={(_feedbackBad ? "#ffcc66" : "#9fe8a0")}>{(_feedbackBad ? "✗" : "✓")} {_feedback}</color>", st);
    }

    void OnGUI()
    {
        if (!_open) { DrawClosedBar(); return; }
        float w = 560f, h = 24f, x = 12f, y = Screen.height - 240f;
        GUI.Box(new Rect(x - 4f, y - 4f, w + 8f, h + 212f), "");
        GUI.Label(new Rect(x, y - 2f, w, h), "<b>Grimorio</b>  (Tab autocompleta · <b>Intro</b> lanza · <b>Esc</b> sale sin lanzar) — mientras escribes, Kushal no se mueve");
        GUI.SetNextControlName("spellInput");
        _input = GUI.TextField(new Rect(x, y + 20f, w, h), _input);
        GUI.FocusControl("spellInput");

        // Sugerencias (difusas): el prefijo = lo escrito antes del '('. Vacío → muestra todo (vista del grimorio).
        string prefix = _input;
        int paren = prefix.IndexOf('(');
        if (paren >= 0) prefix = prefix.Substring(0, paren);
        // Sugerencias = los hechizos que el poseído CONOCE (los que aún no, al final y en gris, sin autocompletar);
        // arriba del todo, el que un maestro te está pidiendo ahora.
        Anima me = PossessedAnima();
        var known = new List<(string name, string template)>();
        var unknown = new List<(string name, string template)>();
        string p = Proposed;
        if (p != null) known.Add(("pedido", p));
        foreach (var s in Suggest(prefix.Trim())) (KnowsSpell(me, s.name) ? known : unknown).Add(s);

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Tab && known.Count > 0)   // Tab → rellena la plantilla del 1º
        { _input = known[0].template; e.Use(); }
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) { _input = ""; Close(); e.Use(); }

        int row = 0;
        for (int i = 0; i < known.Count && row < 7; i++, row++)
            GUI.Label(new Rect(x, y + 48f + row * 16f, w, 16f), (i == 0 ? "➤ " : "   ") + $"<b>{known[i].name}</b>  <size=10>{known[i].template}</size>");
        for (int i = 0; i < unknown.Count && row < 7; i++, row++)
            GUI.Label(new Rect(x, y + 48f + row * 16f, w, 16f), $"   <color=#888888>{unknown[i].name}  <size=10>(aún no lo conoces)</size></color>");
        for (int i = 0; i < _log.Count && i < 2; i++)
            GUI.Label(new Rect(x, y + 48f + 7 * 16f + i * 16f, w, 16f), _log[_log.Count - 1 - i]);
    }

    // Búsqueda difusa (tolerante a fallos): startsWith > contiene > solape de caracteres. Ordena por similitud.
    static List<(string name, string template)> Suggest(string q)
    {
        var scored = new List<(int score, string name, string template)>();
        foreach (var s in Spells)
        {
            string n = s.name.ToLowerInvariant(); string ql = q.ToLowerInvariant();
            int score;
            if (string.IsNullOrEmpty(ql)) score = 1;
            else if (n == ql) score = 1000;
            else if (n.StartsWith(ql)) score = 500 - (n.Length - ql.Length);
            else if (n.Contains(ql)) score = 200;
            else { int overlap = 0; foreach (char c in ql) if (n.IndexOf(c) >= 0) overlap++; score = overlap * 20 - Mathf.Abs(n.Length - ql.Length); }
            if (score > 0) scored.Add((score, s.name, s.template));
        }
        scored.Sort((a, b) => b.score.CompareTo(a.score));
        var outp = new List<(string, string)>();
        foreach (var s in scored) outp.Add((s.name, s.template));
        return outp;
    }

    int _logSeq;
    void Log(string s) { _logSeq++; _log.Add(s); if (_log.Count > 10) _log.RemoveAt(0); Debug.Log($"[Hechizo] {s}"); }

    // ── Parse + dispatch ────────────────────────────────────────────────────────────────────────
    void Dispatch(string line)
    {
        Anima self = PossessedAnima();
        if (self == null) { Log("No posees a nadie ahora mismo."); return; }

        string cmd = line;
        var args = new Dictionary<string, string>();
        int p = line.IndexOf('(');
        if (p >= 0)
        {
            cmd = line.Substring(0, p).Trim();
            int q = line.LastIndexOf(')');
            string inside = q > p ? line.Substring(p + 1, q - p - 1) : line.Substring(p + 1);
            inside = inside.Trim().Trim('{', '}');
            foreach (string part in inside.Split(','))
            {
                string t = part.Trim();
                if (t.Length == 0) continue;
                int c = t.IndexOf(':');
                if (c < 0) args["destiny"] = Clean(t);                 // un solo valor suelto = destino
                else args[Clean(t.Substring(0, c)).ToLowerInvariant()] = Clean(t.Substring(c + 1));
            }
        }

        switch (cmd.Trim().ToLowerInvariant())
        {
            case "walkto": WalkTo(self, args); break;
            case "step": case "cook": Step(self); break;
            case "use": UseStation(self, args); break;
            case "take":  EnqueueKitchen(self, args, new TakeSpell { item = args.TryGetValue("item", out string it) ? it : null }); break;
            case "putin": case "put": EnqueueKitchen(self, args, new PutInSpell()); break;
            case "beat":  EnqueueKitchen(self, args, new BeatSpell()); break;
            case "bond":    CastOn<BondSpell>(self, args); break;
            case "thought": CastOn<ThoughtSpell>(self, args); break;
            case "yoga": case "surya": Yoga(self); break;
            case "asana": case "postura": Asana(self, args); break;
            case "clean": case "limpiar": Clean(self); break;
            case "feed": case "alimentar": Feed(self); break;
            case "stock": case "abastecer": Stock(self); break;
            case "teach": case "ensenar": CastOn<TeachSpell>(self, args); break;
            case "analysis": case "analisis": CastOn<ClinicalAnalysisSpell>(self, args); break;
            case "vaccinate": case "vacunar": CastOn<VaccinateSpell>(self, args); break;
            case "status": case "estado": Status(self, args); break;
            case "open": case "abrir": AreaCmd(self, args, false); break;
            case "close": case "cerrar": AreaCmd(self, args, true); break;
            case "queue": case "cola": LogQueue(self); break;
            case "where": case "donde": LogWhere(args.TryGetValue("destiny", out string wf) ? wf : null); break;
            case "cancel": case "cancelar": CancelQueue(self, args); break;
            case "virtualize": case "virtualizar": Virtualize(self, args); break;
            case "exit": case "salir": if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) { MobWorldLoader.Instance.ExitMobWorld(); Log($"«{self.name}» sale del microcosmos."); } else Log("No estás en un microcosmos."); break;
            case "look": case "mirar": LookCmd(self, args, false); break;
            case "lookat": case "mirara": LookCmd(self, args, true); break;
            default: Log($"No conozco el hechizo «{cmd}»."); break;
        }
    }

    // HECHIZO DE VIRTUALIZACIÓN: entra al MICROCOSMOS de un área (su propia escena). Junto a la máquina del área es
    // gratis (la máquina lo lanza); a distancia se lanza con MAGIA y cuesta maná (requiere el maná desbloqueado: yoga 2).
    const float VirtualizeManaCost = 20f;
    void Virtualize(Anima self, Dictionary<string, string> args)
    {
        string area = args.TryGetValue("destiny", out string d) ? d : null;
        VirtualizationMachine best = null; float bestSq = float.MaxValue;
        foreach (VirtualizationMachine vm in FindObjectsOfType<VirtualizationMachine>())
        {
            if (string.IsNullOrEmpty(vm.mobWorldSceneName)) continue;
            if (area != null && !NameMatches(vm.areaName, area)) continue;
            float sq = (vm.transform.position - self.transform.position).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = vm; }
        }
        if (best == null) { Log($"«{self.name}»: no conozco el microcosmos de {area ?? "aquí"}."); return; }
        if (MobWorldLoader.Instance.IsBusy || MobWorldLoader.Instance.IsInMobWorld) { Log("Ya estás entrando o dentro de un microcosmos."); return; }

        bool atMachine = bestSq <= 6f * 6f;   // estar en el área, junto a su máquina
        if (!atMachine && bestSq <= 30f * 30f)   // la máquina está aquí cerca: ir andando hasta el tanque y entonces virtualizar
        {
            VirtualizationMachine vmTarget = best;
            Enqueue(self, new WalkToSpell { target = vmTarget.transform, targetName = "el tanque de virtualización", stopDistance = 2f });
            Enqueue(self, new ActionNowSpell("entrar al tanque", a => { Log($"«{a.name}» entra al tanque de {vmTarget.areaName}."); vmTarget.EnterWithTank(a.transform); }));
            Log($"«{self.name}» va al tanque de virtualización de {best.areaName}.");
            return;
        }
        if (!atMachine)
        {
            CharacterLevel cl = self.GetComponent<CharacterLevel>();
            if (cl == null || !cl.ManaUnlocked) { Log($"«{self.name}»: aún no sabes virtualizar con magia (practica yoga) — ve a la máquina de {best.areaName}."); return; }
            if (cl.currentMana < VirtualizeManaCost) { Log($"«{self.name}»: no tienes maná suficiente ({cl.currentMana:0}/{VirtualizeManaCost:0})."); return; }
            cl.currentMana -= VirtualizeManaCost;
        }
        Log($"«{self.name}» virtualiza el microcosmos de {best.areaName} {(atMachine ? "con la máquina (entra al tanque)" : $"con magia (−{VirtualizeManaCost:0} maná)")}.");
        if (atMachine) best.EnterWithTank(self.transform);   // máscara + tanque de agua
        else MobWorldLoader.Instance.EnterMobWorld(best.mobWorldSceneName);
    }

    // CÁMARA por hechizo (apuntar sin ratón): look({yaw:90, pitch:-20}) gira; lookAt({destiny:"Magnate"}) mira hacia algo.
    void LookCmd(Anima self, Dictionary<string, string> args, bool at)
    {
        PlayerController pc = self.GetComponent<PlayerController>();
        if (pc == null) { Log($"«{self.name}» no tiene cámara propia."); return; }
        if (at)
        {
            string name = args.TryGetValue("destiny", out string d) ? d : null;
            GameObject go = name != null ? ResolveObject(self, name) : null;
            if (go == null) { Log($"«{self.name}»: no encuentro {name}."); return; }
            pc.LookAtPoint(go.transform.position + Vector3.up * 1f);
            Log($"«{self.name}» mira a {go.name}.");
            return;
        }
        float yaw = args.TryGetValue("yaw", out string y) && float.TryParse(y, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fy) ? fy : 0f;
        float pitch = args.TryGetValue("pitch", out string p) && float.TryParse(p, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fp) ? fp : 0f;
        pc.Look(yaw, pitch);
        Log($"«{self.name}» gira {yaw:0}° y {pitch:0}°.");
    }

    // Diagnóstico: qué hay en la cola del poseído (carril propio, que manda, y el del jugador) y dónde está.
    void LogQueue(Anima self)
    {
        ActionQueue q = self.GetComponent<ActionQueue>();
        var sb = new System.Text.StringBuilder($"«{self.name}» en {self.transform.position:F1} · cola propia: ");
        if (q != null) foreach (QueuedSpell s in q.Own) sb.Append(s.Label).Append(" | ");
        sb.Append(" · cola del jugador: ");
        if (q != null) foreach (QueuedSpell s in q.Player) sb.Append(s.Label).Append(" | ");
        GameObject tagged = GameObject.FindGameObjectWithTag("Player");
        Log(sb.ToString());
        Log($"  self: tag={self.tag} padre={(self.transform.parent != null ? self.transform.parent.name : "-")} mismo={(GameObject.FindGameObjectWithTag("Player") == self.gameObject)} · " +
            $"Player(tag): {(tagged != null ? tagged.name + " en " + tagged.transform.position.ToString("F1") + " anima=" + (tagged.GetComponent<Anima>() != null) : "ninguno")}");
        PlayerController dpc = self.GetComponent<PlayerController>(); CharacterController dcc = self.GetComponent<CharacterController>();
        WalkSpell dws = self.GetComponent<WalkSpell>(); CharacterLevel dcl = self.GetComponent<CharacterLevel>();
        Log($"  control: PlayerController={(dpc != null ? dpc.enabled.ToString() : "-")} CC={(dcc != null ? dcc.enabled.ToString() : "-")} WalkSpell={(dws != null ? dws.enabled + (dws.IsCharging ? " (cargando)" : "") + $" v={dws.LastSpeed:0.00} dir={dws.LastDir}" : "-")} nadando={(dpc != null && dpc.IsSwimming)} energía={(dcl != null ? dcl.currentEnergy.ToString("0") : "-")} timeScale={Time.timeScale}");
        var touching = new System.Text.StringBuilder();
        foreach (Collider c in Physics.OverlapSphere(self.transform.position, 0.8f, ~0, QueryTriggerInteraction.Ignore))
            if (!c.transform.IsChildOf(self.transform)) touching.Append(c.name).Append(' ');
        Log($"  toca: {touching} · cámara tapada por: {CameraManager.LastOccluder ?? "nada"}");
        Camera camD = Camera.main; Transform anc = self.transform.Find("CameraPivot/ThirdPersonAnchor") ?? self.transform.Find("ThirdPersonAnchor");
        if (camD != null) Log($"  cámara: rel {self.transform.InverseTransformPoint(camD.transform.position)} rot {camD.transform.eulerAngles} fov {camD.fieldOfView:0} · ancla {(anc != null ? anc.localPosition + " " + anc.localEulerAngles + " padre " + anc.parent.name : "—")}");
        CubeWalkCycle wcD = self.GetComponentInChildren<CubeWalkCycle>(); CreatureRig rgD = self.GetComponentInChildren<CreatureRig>();
        Transform thD = rgD != null ? rgD.Get(BodyPart.ThighLeft) : null;
        Log($"  andar: ciclo={(wcD != null ? $"{wcD.enabled} blend {wcD.Blend:0.00} v {wcD.LastSpeed:0.0}" : "NO")} rig={(rgD != null ? rgD.gameObject.name : "NO")} muslo={(thD != null ? thD.localEulerAngles.ToString() : "NO")} bloqueado={(rgD != null && rgD.IsPoseLocked(BodyPart.ThighLeft))} timeScale={Time.timeScale}");
        AnimaController myAc = self.GetComponent<AnimaController>();
        if (myAc != null) Log($"  cerebro activo: {(myAc.Active != null ? myAc.Active.GetType().Name : "ninguno")} · FollowBrain={(self.GetComponent<FollowBrain>() != null ? (self.GetComponent<FollowBrain>().target != null ? self.GetComponent<FollowBrain>().target.name : "sin objetivo") : "-")}");
        foreach (AnimaController ac in FindObjectsOfType<AnimaController>())
            if (ac.Active is PlayerBrain) Log($"  PlayerBrain activo en «{ac.name}» {ac.transform.position:F1}");
    }

    // Avanza el paso de cocina del poseído (cocinar por teclado; combínalo con walkTo a cada estación).
    // `step` y `use` van a la COLA como los demás hechizos de cocina: antes eran inmediatos y, mezclados en una tanda con
    // walkTo/take/putIn/beat (encolados), se adelantaban (un `step` avanzaba la receta antes que el `putIn` pendiente).
    void Step(Anima self)
    {
        CookingStation cs = self.GetComponent<CookingStation>();
        if (cs == null) { Log($"«{self.name}» no está cocinando."); return; }
        Enqueue(self, new ActionNowSpell("siguiente paso de cocina", a => { cs.AdvanceStep(); Log($"«{a.name}»: siguiente paso de cocina."); }));
    }

    static void Enqueue(Anima self, QueuedSpell s)
    {
        ActionQueue q = self.GetComponent<ActionQueue>();
        if (q == null) q = self.gameObject.AddComponent<ActionQueue>();
        q.EnqueuePlayer(s);
    }

    // ACCIÓN CON RANGO (first slice de "steps = hechizos con alcance"): usar una estación (nevera/plancha…) exige estar
    // dentro del ALCANCE (del miembro; la magia lo ampliará). Si no llegas, FALLA → el jugador debe `walkTo` primero.
    void UseStation(Anima self, Dictionary<string, string> args)
    {
        Enqueue(self, new ActionNowSpell("usar", a => UseStationNow(a, args)));
    }

    void UseStationNow(Anima self, Dictionary<string, string> args)
    {
        string name = args.TryGetValue("destiny", out string d) ? d : (args.TryGetValue("character", out string c) ? c : null);
        if (name == null) { Log("use necesita un destiny (p. ej. Nevera)."); return; }
        GameObject station = FindNearestNamed(name, self.transform.position);
        if (station == null) { Log($"«{self.name}»: ¿Dónde está {name}?"); return; }
        float reach = 2f + Mathf.Max(0f, self.perception - 1f);          // alcance del miembro; la magia lo ampliará
        if (KitchenStepSpell.DistanceTo(self, station) > reach)
        { Log($"«{self.name}»: no alcanzo {name} — acércate (walkTo)."); return; }   // falla por rango
        CookingStation cs = self.GetComponent<CookingStation>();
        if (cs != null) cs.AdvanceStep();
        Log($"«{self.name}» usa {name}.");
    }

    // HECHIZOS DE COCINA POR PASO (take/putIn/beat, KitchenStepSpells.cs): resuelve la estación EXACTA por nombre
    // ("Plancha" → la «Plancha N» etiquetada más cercana) y lo ENCOLA tras lo que ya haya (walkTo primero → llega → toma).
    // El rango se comprueba al EJECUTARSE, no al encolar.
    void EnqueueKitchen(Anima self, Dictionary<string, string> args, KitchenStepSpell spell)
    {
        string name = args.TryGetValue("destiny", out string d) ? d : (args.TryGetValue("character", out string c) ? c : null);
        if (name == null) { Log($"{spell.GetType().Name} necesita un destiny (p. ej. Huevos, Olla)."); return; }
        GameObject station = ResolveStation(self, name);
        if (station == null) { Log($"«{self.name}»: ¿Dónde está {name}?"); return; }
        spell.target = station; spell.targetName = station.name;
        if (!self.TryGetComponent(out ActionQueue q)) q = self.gameObject.AddComponent<ActionQueue>();
        q.EnqueuePlayer(spell);
        Log($"«{self.name}» → {spell.Label}.");
    }

    // El objeto ACTIVO con ese nombre exacto MÁS CERCANO. `GameObject.Find` devolvía uno cualquiera: hay varias «Nevera»
    // (restaurante, tour, virtualización) y `walkTo` llevaba a una mientras `use` comprobaba el alcance contra otra.
    static GameObject FindNearestNamed(string name, Vector3 from)
    {
        GameObject best = null; float bestSq = float.MaxValue;
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (!string.Equals(t.name, name, System.StringComparison.OrdinalIgnoreCase)) continue;
            float sq = (t.position - from).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = t.gameObject; }
        }
        return best;
    }

    // Estación por nombre: exacto primero; si no, la más cercana entre objetos etiquetados (WorldLabel) o recipientes.
    static GameObject ResolveStation(Anima self, string name)
    {
        GameObject exact = FindNearestNamed(name, self.transform.position);
        if (exact != null) return exact;
        GameObject best = null; float bestSq = float.MaxValue;
        var candidates = new List<Component>();
        candidates.AddRange(FindObjectsOfType<WorldLabel>());
        candidates.AddRange(FindObjectsOfType<KitchenVessel>());
        foreach (Component comp in candidates)
        {
            if (!NameMatches(comp.name, name)) continue;
            float sq = (comp.transform.position - self.transform.position).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = comp.gameObject; }
        }
        return best;
    }

    // Yoga: encola el Saludo al Sol (12 asanas) en la cola del poseído → adopta las posturas por bodyPart.
    // ASANA: hace el paso que el maestro te está pidiendo en la lección (TeachingSession) o, fuera de una lección, la asana
    // conocida con ese nombre (de upa-yoga o del Saludo al Sol).
    void Asana(Anima self, Dictionary<string, string> args)
    {
        TeachingSession ts = TeachingSession.For(self);
        if (ts != null && ts.CastCurrentStep(self)) { Log($"«{self.name}» hace el paso que le pide el maestro."); return; }
        string want = args.TryGetValue("name", out string n) ? n : (args.TryGetValue("destiny", out string d) ? d : "");
        if (string.IsNullOrWhiteSpace(want)) { Log("¿Qué asana? asana({name:\"…\"})"); return; }
        var all = new List<QueuedSpell>();
        all.AddRange(UpaYoga.NeckSequence());
        all.AddRange(SunSalutation.Sequence());
        foreach (QueuedSpell s in all)
            if (s is AsanaPoseSpell a && a.asanaName.ToLowerInvariant().Contains(want.ToLowerInvariant()))
            {
                if (!self.TryGetComponent(out ActionQueue q)) q = self.gameObject.AddComponent<ActionQueue>();
                q.EnqueuePlayer(a);
                Log($"«{self.name}» hace {a.asanaName}.");
                return;
            }
        Log($"No conozco ninguna asana llamada «{want}».");
    }

    void Yoga(Anima self)
    {
        if (!self.TryGetComponent(out ActionQueue q)) q = self.gameObject.AddComponent<ActionQueue>();
        foreach (QueuedSpell s in SunSalutation.Sequence()) q.EnqueuePlayer(s);
        Log($"«{self.name}» empieza el Saludo al Sol (12 asanas).");
    }

    // STATUS por hechizo: muestra el HUD de estado de un anima, o los stocks de un área (santuario/restaurante).
    void Status(Anima self, Dictionary<string, string> args)
    {
        string name = args.TryGetValue("destiny", out string d) ? d : "self";
        GameObject go = ResolveObject(self, name);
        if (go == null) { Log($"«{self.name}»: no encuentro {name}."); return; }
        if (!self.TryGetComponent(out StatusSpell sp)) sp = self.gameObject.AddComponent<StatusSpell>();
        sp.targetObject = go;
        sp.visible = !(args.TryGetValue("visible", out string v) && (v == "false" || v == "0"));   // visible:false = ocultar
        if (args.TryGetValue("seconds", out string s) && float.TryParse(s, out float sec)) sp.seconds = sec;
        sp.Cast(self, null);
        Log($"«{self.name}» {(sp.visible ? "mira" : "oculta")} el estado de {go.name}.");
    }

    // ABRIR/CERRAR un área con el mismo hechizo (property close).
    void AreaCmd(Anima self, Dictionary<string, string> args, bool close)
    {
        string name = args.TryGetValue("destiny", out string d) ? d : null;
        GameObject go = name != null ? ResolveObject(self, name) : null;
        if (go == null) { Log("¿qué área? (destiny)"); return; }
        if (!self.TryGetComponent(out OpenSpell sp)) sp = self.gameObject.AddComponent<OpenSpell>();
        sp.targetObject = go; sp.close = close;
        sp.reset = args.TryGetValue("reset", out string r) && (r == "true" || r == "1");
        sp.Cast(self, null);
        Log($"«{self.name}» {(close ? "cierra" : "abre")} {go.name}.");
    }

    // Resuelve un objeto por nombre: "self"/vacío = uno mismo; si no, un GameObject por name (áreas), o un anima.
    GameObject ResolveObject(Anima self, string name)
    {
        if (string.IsNullOrEmpty(name) || name == "self" || name == "me" || name == "yo") return self.gameObject;
        GameObject go = FindNearestNamed(name, self.transform.position);
        if (go != null) return go;
        // Las áreas se llaman en escena "Kitchen_Area" pero el jugador las conoce por su nombre visible ("Cocina").
        foreach (SanctuaryArea area in FindObjectsOfType<SanctuaryArea>())
            if (area.displayName != null && string.Equals(area.displayName, name, System.StringComparison.OrdinalIgnoreCase))
                return area.gameObject;
        ITarget it = ResolveAnimaAnywhere(name);
        return (it as Component)?.gameObject;
    }

    // Limpieza como HECHIZO: limpia la mancha más cercana dentro del alcance (falla si no hay/estás lejos).
    void Clean(Anima self)
    {
        if (!self.TryGetComponent(out CleanSpell cs)) cs = self.gameObject.AddComponent<CleanSpell>();
        cs.Cast(self, null);
        Log($"«{self.name}» limpia.");
    }

    // CANCELAR: quita lo que el jugador está haciendo ahora (la cabeza de su cola) o, con all:true, toda su cola. Sin
    // esto, una clase de yoga larga de la Magnate (imitar) bloqueaba cualquier otra orden durante minutos.
    void CancelQueue(Anima self, Dictionary<string, string> args)
    {
        ActionQueue q = self.GetComponent<ActionQueue>();
        if (q == null || q.Player.Count == 0) { Log("No hay nada que cancelar."); return; }
        bool all = args.TryGetValue("all", out string a) && (a == "true" || a == "1");
        string what = q.Player[0].Label;
        if (all) q.ClearPlayer(); else q.RemovePlayerAt(0);
        Log(all ? $"«{self.name}» cancela toda su cola." : $"«{self.name}» deja de {what}.");
    }

    // Alimentar como HECHIZO: va al visitante hambriento más cercano (huerto) y le da de comer al llegar.
    void Feed(Anima self)
    {
        if (!self.TryGetComponent(out ActionQueue q)) q = self.gameObject.AddComponent<ActionQueue>();
        q.EnqueuePlayer(new FeedVisitorSpell());
        Log($"«{self.name}» va a dar de comer al visitante hambriento más cercano.");
    }

    // Abastecer como HECHIZO (inventario): llena el estante más cercano dentro del alcance.
    void Stock(Anima self)
    {
        if (!self.TryGetComponent(out StockSpell ss)) ss = self.gameObject.AddComponent<StockSpell>();
        ss.Cast(self, null);
        Log($"«{self.name}» abastece.");
    }

    // Hechizos de MODIFICACIÓN (bond/thought) sobre un destino: resuelve el anima por nombre EN CUALQUIER PARTE
    // (configurar una relación con alguien aunque nunca se haya visto — crear amistad remota, docs anyma-factory §3).
    void CastOn<T>(Anima self, Dictionary<string, string> args) where T : SpellBase
    {
        string name = args.TryGetValue("destiny", out string d) ? d
                    : args.TryGetValue("character", out string c) ? c : null;
        ITarget it = name == null ? null : IsSelf(name) ? TargetOf(self) : ResolveAnimaAnywhere(name);
        if (it == null) { Log($"«{self.name}»: no encuentro a {name}."); return; }
        if (!self.TryGetComponent(out T spell)) spell = self.gameObject.AddComponent<T>();
        if (args.TryGetValue("amount", out string am) && float.TryParse(am, out float amt)) spell.force = amt;
        if (spell is ThoughtSpell ts) ts.positive = !(args.TryGetValue("positive", out string p) && (p == "false" || p == "0"));
        if (spell is TeachSpell te && args.TryGetValue("skill", out string sk)) te.skillId = sk;
        if (spell.CanCast(self, it)) { spell.Cast(self, it); Log($"«{self.name}» lanza {typeof(T).Name} sobre {name}."); }
    }

    static ITarget ResolveAnimaAnywhere(string name)
    {
        // Prefiere el nombre exacto; si no, el primero que lo contenga.
        Anima best = null;
        foreach (Anima a in FindObjectsOfType<Anima>())
        {
            if (a.death || !NameMatches(a.name, name)) continue;
            if (string.Equals(a.name, name, System.StringComparison.OrdinalIgnoreCase)) { best = a; break; }
            if (best == null) best = a;
        }
        return best != null ? TargetOf(best) : null;
    }

    static bool IsSelf(string name) => name == "self" || name == "me" || name == "yo";

    // Los Animal ya son ITarget; el resto de anima (compañeros, Magnate…) recibe un AnimaTarget adaptador.
    static ITarget TargetOf(Anima a)
    {
        if (a is ITarget it) return it;
        ITarget existing = a.GetComponent<ITarget>();
        return existing ?? a.gameObject.AddComponent<AnimaTarget>();
    }

    static string Clean(string s) => s.Trim().Trim('"', '\'', ' ');

    void WalkTo(Anima self, Dictionary<string, string> args)
    {
        string name = args.TryGetValue("destiny", out string d) ? d
                    : args.TryGetValue("character", out string c) ? c : null;
        if (string.IsNullOrEmpty(name)) { Log("walkTo necesita un destiny."); return; }

        Transform target = ResolveDestiny(self, name);
        if (target == null)
        {
            // el poseído no lo ve/encuentra → lo "piensa"
            Mind m = self.GetComponent<Mind>();
            if (m != null) { m.lastThought = $"¿Dónde está {name}?"; m.lastPositive = false; m.lastIntensity = 0.6f; m.lastThoughtTime = Time.time; }
            // ¿Existe pero no lo ve? Se lo dice con la distancia y el rumbo (antes solo "¿Dónde está?", sin pista).
            Anima far = null; float farD = float.MaxValue;
            foreach (Anima a in FindObjectsByType<Anima>(FindObjectsSortMode.None))
                if (a != self && !a.death && NameMatches(a.name, name)) { float dd = Vector3.Distance(a.transform.position, self.transform.position); if (dd < farD) { farD = dd; far = a; } }
            if (far != null) { Log($"No ves a {name} desde aquí: está a {farD:0} m hacia el {Rumbo(far.transform.position - self.transform.position)}. Acércate o ve a un área (walkTo({{destiny:\"Cocina\"}}))."); return; }
            Log($"«{self.name}»: ¿Dónde está {name}?");
            return;
        }

        if (!self.TryGetComponent(out ActionQueue q)) q = self.gameObject.AddComponent<ActionQueue>();
        var spell = new WalkToSpell { target = target, targetName = name };
        spell.maxSpeed = args.TryGetValue("maxspeed", out string ms) && (ms == "true" || ms == "1");
        if (args.TryGetValue("distance", out string ds) && float.TryParse(ds, out float dist)) spell.stopDistance = dist;
        q.EnqueuePlayer(spell);
        Log($"«{self.name}» → {spell.Label}.");
    }

    // Destino = una ANIMA que el poseído alcance a ver (por nombre), o un ÁREA/gameobject por su `name`.
    Transform ResolveDestiny(Anima self, string name)
    {
        float reach = baseVisibleRange * Mathf.Max(0.5f, self.perception);
        Anima bestAnima = null; float bestSq = reach * reach;
        foreach (Anima a in FindObjectsOfType<Anima>())
        {
            if (a == self || a.death) continue;
            if (!NameMatches(a.name, name)) continue;
            float sq = (a.transform.position - self.transform.position).sqrMagnitude;
            if (sq <= bestSq) { bestSq = sq; bestAnima = a; }
        }
        if (bestAnima != null) return bestAnima.transform;

        // un ÁREA por su nombre visible («Cocina», «Huerto», «Mecánica»): antes solo valía el del GameObject (Kitchen_Area)
        foreach (SanctuaryArea sa in FindObjectsOfType<SanctuaryArea>())
            if (string.Equals(sa.displayName, name, System.StringComparison.OrdinalIgnoreCase)) return sa.transform;

        // ¿existe el anima pero fuera de alcance? entonces "no lo ve" → null (lo preguntará)
        foreach (Anima a in FindObjectsOfType<Anima>())
            if (a != self && NameMatches(a.name, name)) return null;

        // si no es anima, puede ser un ÁREA / gameobject por nombre (restaurante, santuario…)
        GameObject area = FindNearestNamed(name, self.transform.position);
        return area != null ? area.transform : null;
    }


    // DIAGNÓSTICO: dónde está cada humano, a cuánto del suelo que tiene debajo y qué hace (¿alguien flotando?).
    void LogWhere(string filter)
    {
        foreach (Anima a in FindObjectsByType<Anima>(FindObjectsSortMode.None))
        {
            bool human = !(a is Animal) && a.GetComponent<PlayableCreature>() == null;
            if (filter == "humanos" || filter == "cuerpos" ? !human : filter == null ? (a is Animal || a.GetComponentInChildren<CubeJoint>() == null) : a.name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
            Vector3 p = a.transform.position;
            string below = Physics.Raycast(p + Vector3.up * 0.5f, Vector3.down, out RaycastHit h, 60f, ~0, QueryTriggerInteraction.Ignore)
                ? $"{p.y - h.point.y:0.0} m sobre {h.collider.name}" : "nada debajo";
            var cc = a.GetComponent<CharacterController>(); var nav = a.GetComponent<UnityEngine.AI.NavMeshAgent>();
            Log($"  {a.name} ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}) {below} · cc={(cc != null ? cc.enabled.ToString() : "-")} nav={(nav != null ? (nav.enabled ? (nav.isOnNavMesh ? "on" : "OFF-mesh") : "off") : "-")}");
            if (filter == "cuerpos" || filter == "humanos") { BodyShape bs = a.GetComponent<BodyShape>(); Log($"    stats fuerza {a.strength:0.00} agilidad {a.agility:0.00} resistencia {a.endurance:0.00} masa {a.bodyMass:0.00} → {(bs != null ? $"estatura ×{bs.Height:0.00} músculo {bs.Muscle:0.00} grasa {bs.Fat:0.00}" : "SIN BodyShape")}"); }
            if (a.TryGetComponent(out IceInsectLife ins)) Log($"    insecto: {ins.kind} reina={ins.isQueen} edad={ins.ageDays:0.0} días · {ins.Doing} · colmena {(ins.hive != null ? ins.hive.transform.position.ToString("0") : "—")}");
        }
    }

    static string Rumbo(Vector3 d)
    {
        float ang = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg; if (ang < 0f) ang += 360f;
        string[] n = { "norte", "noreste", "este", "sureste", "sur", "suroeste", "oeste", "noroeste" };
        return n[Mathf.RoundToInt(ang / 45f) % 8];
    }
    static bool NameMatches(string objName, string query)
    {
        if (string.IsNullOrEmpty(objName)) return false;
        string o = objName.ToLowerInvariant(), n = query.ToLowerInvariant();
        return o == n || o.StartsWith(n) || o.Contains(n);
    }

    // El anima que el jugador MANEJA ahora (su AnimaController.Active es un PlayerBrain).
    static Anima PossessedAnima()
    {
        foreach (AnimaController ac in FindObjectsOfType<AnimaController>())
            if (ac != null && ac.Active is PlayerBrain && ac.TryGetComponent(out Anima a)) return a;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        return p != null ? p.GetComponent<Anima>() : null;
    }
}

/// <summary>Hechizo de cola de un solo paso: ejecuta una acción instantánea en su turno (mantiene el orden de la tanda).</summary>
public class ActionNowSpell : QueuedSpell
{
    readonly string _label; readonly System.Action<Anima> _act;
    public ActionNowSpell(string label, System.Action<Anima> act) { _label = label; _act = act; }
    public override string Label => _label;
    public override bool Tick(Anima self) { _act?.Invoke(self); return true; }
}
