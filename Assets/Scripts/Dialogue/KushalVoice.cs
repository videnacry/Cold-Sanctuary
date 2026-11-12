using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA VOZ DE KUSHAL (OnGUI). Kushal es un personaje: cuando otro le habla (le pide ayuda, le da un paso de la iniciación,
/// le asigna una misión, le cuenta una historia…), él CONTESTA, siempre con humildad y aceptando ayudar (Berón 2026-10-29).
/// Sus frases y pensamientos van ARRIBA, en su propio panel, para no quitar sitio a lo que le dicen los demás (abajo:
/// <see cref="StorySubtitle"/>, <see cref="LessonPrompt"/>, el <see cref="DialoguePanel"/>, el aviso de la Magnate). El panel
/// nunca baja hasta lo de abajo: su borde inferior se para antes del panel entrante más alto que se dibujó. Es de pantalla
/// (OnGUI), así que vale igual en 1ª y en 3ª persona.
///
///   KushalVoice.Reply(KushalVoice.Ctx.Help);                 // frase del repertorio de ese contexto (sin repetir seguida)
///   KushalVoice.Reply(KushalVoice.Ctx.Help, "Un amigo.");     // frase concreta
///   KushalVoice.Think(KushalVoice.Thought.Arrival);           // pensamiento: en cursiva y entre paréntesis
///   KushalVoice.Hear("Ambrosio", "¡Ayúdanos!", Ctx.Help);     // la frase entrante (abajo) + su respuesta (arriba)
///
/// Las frases salen un momento después de la del otro (para que se lea como respuesta). Tiempo sin escalar: también con
/// el juego en pausa o acelerado por las pruebas. Se auto-crea y persiste entre escenas.
/// </summary>
public class KushalVoice : MonoBehaviour
{
    public const string Name = "Kushal";

    /// <summary>A qué contesta Kushal.</summary>
    public enum Ctx { Help, Instruction, Mission, Thanks, Listen, Farewell, Correction, Patience, Comfort }

    /// <summary>Momentos clave en que Kushal piensa para sí.</summary>
    public enum Thought { Arrival, FirstInitiation, EnterMicrocosmos, Grief }

    // ── Repertorio: humilde y siempre dispuesto ──────────────────────────────────────────────────────────────────
    static readonly Dictionary<Ctx, string[]> Lines = new Dictionary<Ctx, string[]>
    {
        { Ctx.Help, new[] {
            "Claro, cuenta conmigo.",
            "Con gusto te ayudo. ¿Qué hago?",
            "Aquí estoy; voy ahora mismo.",
            "Sí, lo haré con todo mi cuidado.",
            "Por supuesto. Dime cómo y lo hago.",
            "Será un honor ayudarte." } },
        { Ctx.Instruction, new[] {
            "Con gusto, enséñame cómo.",
            "Entendido; lo hago contigo.",
            "Gracias por tu paciencia. Voy.",
            "Te sigo, paso a paso.",
            "Sí. Aprendo de ti con gusto.",
            "De acuerdo; lo intento ahora." } },
        { Ctx.Mission, new[] {
            "Gracias, haré lo que pueda.",
            "De acuerdo, allá voy.",
            "Cuenta conmigo para eso.",
            "Gracias por confiarme esto; voy.",
            "Con gusto. Daré lo mejor de mí." } },
        { Ctx.Thanks, new[] {
            "No hay de qué; fue un gusto.",
            "Gracias a ti por dejarme ayudar.",
            "Lo hicimos juntos.",
            "Para eso estoy, siempre." } },
        { Ctx.Listen, new[] {
            "Te escucho, con gusto.",
            "Gracias por contármelo.",
            "Lo tendré presente.",
            "Aquí estoy, dime." } },
        { Ctx.Farewell, new[] {
            "Gracias por todo. Vuelvo cuando me necesites.",
            "Ha sido un honor. Hasta pronto.",
            "Gracias. Me llevo lo aprendido." } },
        { Ctx.Correction, new[] {
            "Tienes razón, perdona. Vuelvo contigo.",
            "Gracias por avisarme; voy.",
            "Perdón. Ya regreso a mi puesto." } },
        { Ctx.Patience, new[] {
            "Lo entiendo. Vuelvo cuando pueda ayudar más.",
            "Está bien; te espero, sin prisa.",
            "Gracias igualmente. Seguiré trabajando." } },
        { Ctx.Comfort, new[] {
            "Calma, aquí estoy contigo.",
            "No temas: no me voy.",
            "Soy un amigo. Te cuido." } },
    };

    static readonly Dictionary<Thought, string[]> Thoughts = new Dictionary<Thought, string[]>
    {
        { Thought.Arrival, new[] {
            "Todo es nuevo aquí… ojalá pueda ser útil.",
            "Qué lugar tan grande. Escucharé y aprenderé.",
            "Respira. Estoy donde tengo que estar." } },
        { Thought.FirstInitiation, new[] {
            "Mi primera iniciación. Haré lo que me pidan, con calma.",
            "Quiero hacerlo bien, por ellos.",
            "Despacio y con atención: así se aprende." } },
        { Thought.EnterMicrocosmos, new[] {
            "Otro mundo, tan pequeño… Aquí también alguien necesitará ayuda.",
            "Qué extraño es ser tan pequeño. Miraré con atención.",
            "Respiro hondo. Estoy aquí para ayudar." } },
        { Thought.Grief, new[] {
            "Lo dio todo por los suyos… Ojalá yo sepa hacer lo mismo.",
            "Ojalá hubiera podido hacer más." } },
    };

    // ── Estado ───────────────────────────────────────────────────────────────────────────────────────────────────
    struct Entry { public string text; public bool thought, said; public string key; public float showAt, until; }
    static readonly List<Entry> _entries = new List<Entry>();
    static readonly Dictionary<string, string> _lastPick = new Dictionary<string, string>();
    static KushalVoice _inst;
    static bool _wasInMicro;

    /// <summary>Lo último que DIJO / PENSÓ (ya en pantalla), y cuántas respuestas lleva dichas.</summary>
    public static string LastSpoken { get; private set; }
    public static string LastThought { get; private set; }
    public static int Spoken { get; private set; }

    /// <summary>Dónde se dibujó el panel por última vez (coordenadas GUI: y hacia abajo) y en qué frame.</summary>
    public static Rect LastRect { get; private set; }
    public static int LastDrawFrame { get; private set; } = -100;

    /// <summary>¿Hay algo suyo en pantalla ahora mismo?</summary>
    public static bool Visible { get { float now = Time.unscaledTime; foreach (Entry e in _entries) if (e.showAt <= now && now < e.until) return true; return false; } }

    // ── API ──────────────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Kushal contesta a quien le habla (en <paramref name="ctx"/>). Sin <paramref name="line"/>, una del repertorio,
    /// nunca la misma dos veces seguidas. Devuelve la frase.</summary>
    public static string Reply(Ctx ctx, string line = null, float delay = 1.3f)
    {
        string key = "r:" + ctx;
        if (string.IsNullOrEmpty(line)) line = PickFrom(key, Lines[ctx]);
        else _lastPick[key] = line;
        Queue(line, false, key, delay);
        return line;
    }

    /// <summary>Kushal piensa para sí (cursiva, entre paréntesis).</summary>
    public static string Think(Thought t, string line = null, float delay = 0.6f)
    {
        string key = "t:" + t;
        if (string.IsNullOrEmpty(line)) line = PickFrom(key, Thoughts[t]);
        else _lastPick[key] = line;
        Queue(line, true, key, delay);
        return line;
    }

    /// <summary>Como <see cref="Think"/>, pero solo la primera vez en la partida (llegada, primera iniciación).</summary>
    public static string ThinkOnce(Thought t, float delay = 0.6f) => _thoughtOnce.Add(t) ? Think(t, null, delay) : null;
    static readonly HashSet<Thought> _thoughtOnce = new HashSet<Thought>();

    /// <summary>Alguien le dice <paramref name="text"/> a Kushal: la frase entrante, abajo (<see cref="StorySubtitle"/>), y su
    /// respuesta, arriba.</summary>
    public static string Hear(string who, string text, Ctx ctx, float secs = 6f, float delay = 1.3f)
    {
        StorySubtitle.Show(who, text, secs);
        return Reply(ctx, null, delay);
    }

    /// <summary>Una frase del repertorio de <paramref name="ctx"/> sin repetir la anterior (no la pone en pantalla).</summary>
    public static string Pick(Ctx ctx) => PickFrom("r:" + ctx, Lines[ctx]);

    /// <summary>¿Es una frase del repertorio humilde de Kushal (de cualquier contexto, o del dado)?</summary>
    public static bool IsHumble(string text, Ctx? ctx = null)
    {
        if (string.IsNullOrEmpty(text)) return false;
        foreach (KeyValuePair<Ctx, string[]> kv in Lines)
            if ((ctx == null || kv.Key == ctx.Value) && System.Array.IndexOf(kv.Value, text) >= 0) return true;
        return false;
    }

    public static IReadOnlyList<string> Pool(Ctx ctx) => Lines[ctx];

    /// <summary>Quita todo lo suyo de la pantalla (y lo pendiente).</summary>
    public static void Clear() => _entries.Clear();

    /// <summary>Cómo se escribe en pantalla: «frase» si la dice; (frase) en cursiva si la piensa.</summary>
    public static string Format(string text, bool thought)
        => thought ? $"<b>{Name}</b> <i><color=#c9dcff>({text})</color></i>" : $"<b>{Name}:</b> «{text}»";

    // ── Maquetación (pura: la usan el OnGUI y la prueba) ─────────────────────────────────────────────────────────

    /// <summary>Margen superior: por debajo del aviso de meditación (y 8–36) y de los avisos centrados (y 60–88).</summary>
    public const float TopMargin = 96f;
    /// <summary>Hueco mínimo entre su panel y el panel entrante de abajo.</summary>
    public const float Gap = 10f;

    /// <summary>El rectángulo del panel de Kushal para una pantalla <paramref name="sw"/>×<paramref name="sh"/>, con
    /// <paramref name="contentH"/> px de contenido y el panel entrante empezando en <paramref name="incomingTop"/>. Arriba y
    /// centrado; nunca pasa de 0,4·alto ni toca lo entrante (si no cabe, se encoge).</summary>
    public static Rect Layout(float sw, float sh, float contentH, float incomingTop)
    {
        float w = Mathf.Min(Mathf.Max(460f, sw * 0.42f), sw - 32f);
        float limit = Mathf.Min(sh * 0.4f, incomingTop - Gap);
        float top = Mathf.Min(TopMargin, Mathf.Max(4f, limit - contentH));   // en pantallas muy bajas, sube
        float h = Mathf.Max(0f, Mathf.Min(contentH, limit - top));
        return new Rect((sw - w) * 0.5f, top, w, h);
    }

    /// <summary>Dónde empieza (y) el panel entrante más alto que se está dibujando: subtítulos, aviso de lección, panel de
    /// diálogo, aviso de la Magnate. Sin ninguno, el alto de la pantalla.</summary>
    public static float IncomingTop(float sh)
    {
        float top = sh;
        int f = Time.frameCount;
        if (f - StorySubtitle.LastDrawFrame <= 2) top = Mathf.Min(top, StorySubtitle.LastRect.yMin);
        if (f - LessonPrompt.LastDrawFrame <= 2) top = Mathf.Min(top, LessonPrompt.LastRect.yMin);
        if (MagnateWarden.Speaking) top = Mathf.Min(top, sh - 120f);   // su caja: (alto − 120, 60 px)
        DialogueManager dm = DialogueManager.Instance;
        if (dm != null && dm.IsPlaying && dm.panel != null)
        {
            RectTransform rt = dm.panel.transform as RectTransform;
            Canvas c = dm.panel.GetComponentInParent<Canvas>();
            if (rt != null && c != null && c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                var corners = new Vector3[4]; rt.GetWorldCorners(corners);
                top = Mathf.Min(top, sh - Mathf.Max(corners[1].y, corners[2].y));   // (pantalla: y hacia arriba → GUI)
            }
        }
        return top;
    }

    // ── Interno ──────────────────────────────────────────────────────────────────────────────────────────────────

    static string PickFrom(string key, string[] pool)
    {
        _lastPick.TryGetValue(key, out string last);
        if (pool.Length == 1) { _lastPick[key] = pool[0]; return pool[0]; }
        string pick = pool[Random.Range(0, pool.Length)];
        if (pick == last) pick = pool[(System.Array.IndexOf(pool, pick) + 1 + Random.Range(0, pool.Length - 1)) % pool.Length];
        _lastPick[key] = pick;
        return pick;
    }

    static void Queue(string text, bool thought, string key, float delay)
    {
        Ensure();
        float now = Time.unscaledTime;
        // Si aún no ha salido otra del MISMO contexto (pasos muy seguidos), la sustituye: no se amontonan.
        _entries.RemoveAll(e => e.key == key && e.showAt > now);
        float show = now + Mathf.Max(0f, delay);
        float secs = Mathf.Clamp(2.5f + text.Length * 0.06f, 4f, 9f);
        _entries.Add(new Entry { text = text, thought = thought, key = key, showAt = show, until = show + secs });
        Debug.Log($"[Kushal] {(thought ? "piensa" : "contesta")}: {(thought ? "(" + text + ")" : "«" + text + "»")}");
    }

    static void Ensure()
    {
        if (_inst != null) return;
        var go = new GameObject("KushalVoice_HUD");
        _inst = go.AddComponent<KushalVoice>();
        DontDestroyOnLoad(go);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() => Ensure();   // para notar la entrada a un microcosmos aunque nadie le haya hablado aún

    void Update()
    {
        float now = Time.unscaledTime;
        for (int i = 0; i < _entries.Count; i++)
        {
            Entry e = _entries[i];
            if (e.showAt > now || e.said) continue;
            if (e.thought) LastThought = e.text; else { LastSpoken = e.text; Spoken++; }
            e.said = true; _entries[i] = e;   // (contada una sola vez)
        }
        _entries.RemoveAll(e => now >= e.until);

        // Al entrar en un microcosmos, piensa para sí.
        bool inMicro = MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy;
        if (inMicro && !_wasInMicro) Think(Thought.EnterMicrocosmos, null, 2f);
        _wasInMicro = inMicro;
    }

    void OnGUI()
    {
        float now = Time.unscaledTime;
        var shown = new List<Entry>();
        for (int i = _entries.Count - 1; i >= 0 && shown.Count < 2; i--)   // las dos más nuevas, la más nueva arriba
            if (_entries[i].showAt <= now && now < _entries[i].until) shown.Add(_entries[i]);
        if (shown.Count == 0) return;

        var style = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true, richText = true, alignment = TextAnchor.MiddleLeft };
        float sw = Screen.width, sh = Screen.height;
        float w = Layout(sw, sh, 0f, sh).width;
        var texts = new string[shown.Count]; var hs = new float[shown.Count];
        float total = 0f;
        for (int i = 0; i < shown.Count; i++)
        {
            texts[i] = Format(shown[i].text, shown[i].thought);
            hs[i] = style.CalcHeight(new GUIContent(texts[i]), w - 24f) + 14f;
            total += hs[i] + (i > 0 ? 6f : 0f);
        }
        Rect area = Layout(sw, sh, total, IncomingTop(sh));
        if (area.height < 20f) return;
        LastRect = area; LastDrawFrame = Time.frameCount;

        float y = area.y;
        for (int i = 0; i < shown.Count; i++)
        {
            if (y + hs[i] > area.yMax + 0.5f) break;   // no cabe: nunca invade lo de abajo
            var box = new Rect(area.x, y, area.width, hs[i]);
            GUI.Box(box, GUIContent.none);
            if (!shown[i].thought) GUI.Box(box, GUIContent.none);   // la voz, más opaca que el pensamiento
            GUI.Label(new Rect(box.x + 12f, box.y + 7f, box.width - 24f, hs[i] - 14f), texts[i], style);
            y += hs[i] + 6f;
        }
    }
}
