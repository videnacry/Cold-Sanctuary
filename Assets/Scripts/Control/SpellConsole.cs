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

    void Update() { if (Input.GetKeyDown(toggleKey)) _open = !_open; }

    void OnGUI()
    {
        if (!_open) return;
        float w = 560f, h = 24f, x = 12f, y = Screen.height - 120f;
        GUI.Box(new Rect(x - 4f, y - 4f, w + 8f, h + 92f), "");
        GUI.Label(new Rect(x, y - 2f, w, h), "<b>Hechizos</b>  p.ej.  walkTo({destiny:\"Marilia\", maxSpeed:true})");
        GUI.SetNextControlName("spellInput");
        _input = GUI.TextField(new Rect(x, y + 20f, w, h), _input);
        GUI.FocusControl("spellInput");

        Event e = Event.current;
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            if (!string.IsNullOrWhiteSpace(_input)) { Dispatch(_input.Trim()); _input = ""; }
            e.Use();
        }
        for (int i = 0; i < _log.Count && i < 3; i++)
            GUI.Label(new Rect(x, y + 48f + i * 16f, w, 16f), _log[_log.Count - 1 - i]);
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
            case "bond":    CastOn<BondSpell>(self, args); break;
            case "thought": CastOn<ThoughtSpell>(self, args); break;
            case "yoga": case "surya": Yoga(self); break;
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

    // Yoga: encola el Saludo al Sol (12 asanas) en la cola del poseído → adopta las posturas por bodyPart.
    void Yoga(Anima self)
    {
        var q = self.GetComponent<ActionQueue>() ?? self.gameObject.AddComponent<ActionQueue>();
        foreach (QueuedSpell s in SunSalutation.Sequence()) q.EnqueuePlayer(s);
        Log($"«{self.name}» empieza el Saludo al Sol (12 asanas).");
    }

    // Hechizos de MODIFICACIÓN (bond/thought) sobre un destino: resuelve el anima por nombre EN CUALQUIER PARTE
    // (configurar una relación con alguien aunque nunca se haya visto — crear amistad remota, docs anyma-factory §3).
    void CastOn<T>(Anima self, Dictionary<string, string> args) where T : SpellBase
    {
        string name = args.TryGetValue("destiny", out string d) ? d
                    : args.TryGetValue("character", out string c) ? c : null;
        ITarget it = name != null ? ResolveAnimaAnywhere(name) : null;
        if (it == null) { Log($"«{self.name}»: no encuentro a {name}."); return; }
        T spell = self.GetComponent<T>() ?? self.gameObject.AddComponent<T>();
        if (args.TryGetValue("amount", out string am) && float.TryParse(am, out float amt)) spell.force = amt;
        if (spell is ThoughtSpell ts) ts.positive = !(args.TryGetValue("positive", out string p) && (p == "false" || p == "0"));
        if (spell.CanCast(self, it)) { spell.Cast(self, it); Log($"«{self.name}» lanza {typeof(T).Name} sobre {name}."); }
    }

    static ITarget ResolveAnimaAnywhere(string name)
    {
        foreach (Anima a in FindObjectsOfType<Anima>())
            if (!a.death && NameMatches(a.name, name) && a is ITarget it) return it;
        return null;
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
