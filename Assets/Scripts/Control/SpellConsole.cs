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
    string _input = "";
    readonly List<string> _log = new List<string>();

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) _open = !_open;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        PollInbox();
#endif
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
        ("stock",     "stock"),
        ("teach",     "teach({destiny:\"\", skill:\"yoga\"})"),
        ("analysis",  "analysis({destiny:\"\"})"),
        ("vaccinate", "vaccinate({destiny:\"\"})"),
        ("status",    "status({visible:true, destiny:\"self\", seconds:20})"),
        ("open",      "open({destiny:\"\"})"),
        ("close",     "close({destiny:\"\"})"),
    };

    void OnGUI()
    {
        if (!_open) return;
        float w = 560f, h = 24f, x = 12f, y = Screen.height - 240f;
        GUI.Box(new Rect(x - 4f, y - 4f, w + 8f, h + 212f), "");
        GUI.Label(new Rect(x, y - 2f, w, h), "<b>Grimorio</b>  (escribe y pulsa Tab para autocompletar la plantilla; Enter lanza)");
        GUI.SetNextControlName("spellInput");
        _input = GUI.TextField(new Rect(x, y + 20f, w, h), _input);
        GUI.FocusControl("spellInput");

        // Sugerencias (difusas): el prefijo = lo escrito antes del '('. Vacío → muestra todo (vista del grimorio).
        string prefix = _input;
        int paren = prefix.IndexOf('(');
        if (paren >= 0) prefix = prefix.Substring(0, paren);
        var sug = Suggest(prefix.Trim());

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Tab && sug.Count > 0)   // Tab → rellena la plantilla del 1º
        { _input = sug[0].template; e.Use(); }
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            if (!string.IsNullOrWhiteSpace(_input)) { Dispatch(_input.Trim()); _input = ""; }
            e.Use();
        }

        for (int i = 0; i < sug.Count && i < 7; i++)
            GUI.Label(new Rect(x, y + 48f + i * 16f, w, 16f), (i == 0 ? "➤ " : "   ") + $"<b>{sug[i].name}</b>  <size=10>{sug[i].template}</size>");
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

    void Log(string s) { _log.Add(s); if (_log.Count > 10) _log.RemoveAt(0); Debug.Log($"[Hechizo] {s}"); }

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
            case "clean": case "limpiar": Clean(self); break;
            case "stock": case "abastecer": Stock(self); break;
            case "teach": case "ensenar": CastOn<TeachSpell>(self, args); break;
            case "analysis": case "analisis": CastOn<ClinicalAnalysisSpell>(self, args); break;
            case "vaccinate": case "vacunar": CastOn<VaccinateSpell>(self, args); break;
            case "status": case "estado": Status(self, args); break;
            case "open": case "abrir": AreaCmd(self, args, false); break;
            case "close": case "cerrar": AreaCmd(self, args, true); break;
            default: Log($"No conozco el hechizo «{cmd}»."); break;
        }
    }

    // Avanza el paso de cocina del poseído (cocinar por teclado; combínalo con walkTo a cada estación).
    void Step(Anima self)
    {
        CookingStation cs = self.GetComponent<CookingStation>();
        if (cs == null) { Log($"«{self.name}» no está cocinando."); return; }
        cs.AdvanceStep();
        Log($"«{self.name}»: siguiente paso de cocina.");
    }

    // ACCIÓN CON RANGO (first slice de "steps = hechizos con alcance"): usar una estación (nevera/plancha…) exige estar
    // dentro del ALCANCE (del miembro; la magia lo ampliará). Si no llegas, FALLA → el jugador debe `walkTo` primero.
    void UseStation(Anima self, Dictionary<string, string> args)
    {
        string name = args.TryGetValue("destiny", out string d) ? d : (args.TryGetValue("character", out string c) ? c : null);
        if (name == null) { Log("use necesita un destiny (p. ej. Nevera)."); return; }
        GameObject station = GameObject.Find(name);
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
        var q = self.GetComponent<ActionQueue>() ?? self.gameObject.AddComponent<ActionQueue>();
        q.EnqueuePlayer(spell);
        Log($"«{self.name}» → {spell.Label}.");
    }

    // Estación por nombre: exacto primero; si no, la más cercana entre objetos etiquetados (WorldLabel) o recipientes.
    static GameObject ResolveStation(Anima self, string name)
    {
        GameObject exact = GameObject.Find(name);
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
    void Yoga(Anima self)
    {
        var q = self.GetComponent<ActionQueue>() ?? self.gameObject.AddComponent<ActionQueue>();
        foreach (QueuedSpell s in SunSalutation.Sequence()) q.EnqueuePlayer(s);
        Log($"«{self.name}» empieza el Saludo al Sol (12 asanas).");
    }

    // STATUS por hechizo: muestra el HUD de estado de un anima, o los stocks de un área (santuario/restaurante).
    void Status(Anima self, Dictionary<string, string> args)
    {
        string name = args.TryGetValue("destiny", out string d) ? d : "self";
        GameObject go = ResolveObject(self, name);
        if (go == null) { Log($"«{self.name}»: no encuentro {name}."); return; }
        StatusSpell sp = self.GetComponent<StatusSpell>() ?? self.gameObject.AddComponent<StatusSpell>();
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
        OpenSpell sp = self.GetComponent<OpenSpell>() ?? self.gameObject.AddComponent<OpenSpell>();
        sp.targetObject = go; sp.close = close;
        sp.reset = args.TryGetValue("reset", out string r) && (r == "true" || r == "1");
        sp.Cast(self, null);
        Log($"«{self.name}» {(close ? "cierra" : "abre")} {go.name}.");
    }

    // Resuelve un objeto por nombre: "self"/vacío = uno mismo; si no, un GameObject por name (áreas), o un anima.
    GameObject ResolveObject(Anima self, string name)
    {
        if (string.IsNullOrEmpty(name) || name == "self" || name == "me" || name == "yo") return self.gameObject;
        GameObject go = GameObject.Find(name);
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
        CleanSpell cs = self.GetComponent<CleanSpell>() ?? self.gameObject.AddComponent<CleanSpell>();
        cs.Cast(self, null);
        Log($"«{self.name}» limpia.");
    }

    // Abastecer como HECHIZO (inventario): llena el estante más cercano dentro del alcance.
    void Stock(Anima self)
    {
        StockSpell ss = self.GetComponent<StockSpell>() ?? self.gameObject.AddComponent<StockSpell>();
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
        T spell = self.GetComponent<T>() ?? self.gameObject.AddComponent<T>();
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
            Log($"«{self.name}»: ¿Dónde está {name}?");
            return;
        }

        var q = self.GetComponent<ActionQueue>() ?? self.gameObject.AddComponent<ActionQueue>();
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

        // ¿existe el anima pero fuera de alcance? entonces "no lo ve" → null (lo preguntará)
        foreach (Anima a in FindObjectsOfType<Anima>())
            if (a != self && NameMatches(a.name, name)) return null;

        // si no es anima, puede ser un ÁREA / gameobject por nombre (restaurante, santuario…)
        GameObject area = GameObject.Find(name);
        return area != null ? area.transform : null;
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
