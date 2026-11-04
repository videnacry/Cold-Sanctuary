using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CHARLAS ENTRE PERSONAS (docs/conversaciones.md; Berón 2026-11-04: «se supone que tenemos un sistema montado para que surjan
/// conversaciones entre ánimas, ¿no? En este caso los humanos e insectos humanos deberían poder hacerlo»).
///
/// El sistema de ánimas ya tenía la charla (<see cref="CommunicationSpell"/>: dos <see cref="Mind"/> alternan un pensamiento, hay
/// contagio emocional y crece la amistad), pero era un hechizo que nadie llevaba, solo hablaba quien tenía Mente, no se veía (solo
/// el registro) y no sabía nada del trabajo, la familia ni las necesidades. Este director lo lleva a las PERSONAS:
///   · QUIÉN: los que llevan <see cref="PersonLife"/> (humanos e insectos-humanos, en el santuario y en el microcosmos), el personal
///     de la jornada (<see cref="WorkdayDirector.staff"/>) y quien lleve <see cref="Conversable"/>. Nunca el jugador. Humano↔humano,
///     insecto↔insecto y humano↔insecto, si están en la MISMA escena.
///   · CUÁNDO: cada <see cref="scanSeconds"/> (un temporizador, no cada fotograma) busca parejas libres y quietas a menos de
///     <see cref="rangeHeights"/> estaturas; puntúa cercanía, vínculo, actividad compartida (el mismo puesto, la comida, la misma
///     franja libre) y las ganas de hablar (tiempo sin charlar); en turno de trabajo la probabilidad baja.
///   · CÓMO: un tema del contexto (<see cref="ConversationTopics"/>) y un guion de 4–6 frases que se TURNAN (A, B, A, B…), una cada
///     <see cref="lineMinutes"/> minutos de juego, en un bocadillo sobre quien habla con el tema (OnGUI, como los nombres).
///   · QUÉ DEJA: cada frase sube el vínculo (más si la charla es buena), mueve el estrés y los humores del que escucha (contagio,
///     como CommunicationSpell) y la cara; al acabar queda en la memoria de los dos (el tema, con quién) y en <see cref="Records"/>.
///   · PRIORIDADES: una charla NUNCA mueve a nadie (habla donde está: en su puesto, a la mesa, en su franja libre). Se corta en
///     cuanto uno tiene una necesidad urgente (hambre, baño, sueño, angustia), la está atendiendo, duerme, muere, la historia lo
///     retiene o se separan.
///   · COSTE: como mucho <see cref="maxConcurrent"/> charlas a la vez y <see cref="maxStartsPerScan"/> nuevas por vuelta.
/// No corre durante las pruebas de otras escenas (<see cref="AllowDuringTests"/>), como el horario libre.
/// </summary>
public class ConversationDirector : MonoBehaviour
{
    public static ConversationDirector Instance { get; private set; }
    /// <summary>Las pruebas de las charlas (grupo 88) lo dejan correr; en las demás pruebas no hay charlas (no tocan sus cuentas).</summary>
    public static bool AllowDuringTests;
    public static bool Running => Instance != null && Instance.isActiveAndEnabled && (!ColdSanctuary.TestRunner.Running || AllowDuringTests);

    [Header("Coste acotado")]
    [Tooltip("Charlas a la vez como mucho (en toda la escena).")]
    [Min(1)] public int maxConcurrent = 4;
    [Tooltip("Cada cuántos segundos se buscan parejas (temporizador).")]
    [Min(0.25f)] public float scanSeconds = 2f;
    [Tooltip("Charlas nuevas por vuelta como mucho.")]
    [Min(1)] public int maxStartsPerScan = 2;

    [Header("Cuándo surge")]
    [Tooltip("A cuántas estaturas como mucho (la del más alto) se ponen a hablar.")]
    [Min(0.5f)] public float rangeHeights = 2.6f;
    [Tooltip("A cuántas estaturas se da la charla por terminada (se han separado).")]
    [Min(1f)] public float breakHeights = 4f;
    [Tooltip("Probabilidad base de empezar por vuelta para una pareja que puede (se escala por sociabilidad, ganas y turno).")]
    [Range(0f, 1f)] public float startChance = 0.55f;
    [Tooltip("En turno de trabajo se habla menos (× esto).")]
    [Range(0f, 1f)] public float onShiftFactor = 0.4f;
    [Tooltip("Minutos de juego que alguien espera tras una charla antes de otra.")]
    [Min(0f)] public float personCooldownMinutes = 20f;
    [Tooltip("Minutos de juego antes de que la MISMA pareja vuelva a hablar.")]
    [Min(0f)] public float pairCooldownMinutes = 45f;

    [Header("Ritmo")]
    [Tooltip("Minutos de juego entre una frase y la siguiente (con el día de 1 h real, 1 min = 2,5 s).")]
    [Min(0.1f)] public float lineMinutes = 1f;
    [Tooltip("Tope de cualquier charla (minutos de juego): ninguna espera sin fin.")]
    [Min(1f)] public float maxMinutes = 20f;

    [Header("Efecto")]
    [Tooltip("Vínculo que gana quien escucha por frase (×EffectiveBondGrowthRate; la mitad quien habla). Una charla mala, un tercio.")]
    [Min(0f)] public float bondPerLine = 0.8f;
    [Tooltip("Estrés que quita una frase buena a quien la escucha (una mala suma la mitad).")]
    [Min(0f)] public float stressPerLine = 0.03f;
    [Tooltip("Necesidad que ya es urgente y corta la charla (hambre, baño, sueño).")]
    [Range(0f, 1f)] public float urgentAbove = 0.85f;

    [Header("Bocadillos")]
    [Min(1f)] public float maxDrawDistance = 35f;

    /// <summary>Las charlas en marcha.</summary>
    public readonly List<Conversation> Active = new List<Conversation>();
    /// <summary>Las charlas terminadas (las últimas 200), para las pruebas y el panel.</summary>
    public static readonly List<Conversation> Records = new List<Conversation>();
    /// <summary>Un paso de una charla: «inicio», «linea», «fin» o «interrumpida».</summary>
    public static event System.Action<Conversation, string> OnEvent;
    /// <summary>Lo más que ha habido a la vez (pruebas).</summary>
    public int PeakConcurrent { get; private set; }
    /// <summary>Bocadillos pintados en pantalla (pruebas: se ven).</summary>
    public static int BubblesDrawn;

    readonly List<Anima> _people = new List<Anima>();
    readonly Dictionary<Anima, Vector3> _lastPos = new Dictionary<Anima, Vector3>();
    readonly Dictionary<Anima, bool> _settled = new Dictionary<Anima, bool>();
    readonly Dictionary<Anima, float> _freeAt = new Dictionary<Anima, float>();
    readonly Dictionary<Anima, float> _lastTalk = new Dictionary<Anima, float>();
    readonly Dictionary<long, float> _pairFreeAt = new Dictionary<long, float>();
    static readonly Dictionary<Anima, List<string>> _memory = new Dictionary<Anima, List<string>>();
    readonly List<Anima> _free = new List<Anima>();
    readonly HashSet<Anima> _taken = new HashSet<Anima>();
    float _nextScan, _nextRefresh;
    GUIStyle _style;

    // ── Arranque: uno por escena con personas ──────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        SceneManager.sceneLoaded += OnLoaded;
        Ensure();
    }

    static void OnLoaded(Scene s, LoadSceneMode m) => Ensure();

    /// <summary>El director de la escena; lo crea si hay personas (PersonLife, jornada o Conversable) y aún no existe.</summary>
    public static ConversationDirector Ensure()
    {
        if (Instance != null) return Instance;
        if (!Application.isPlaying) return null;
        ConversationDirector existing = FindAnyObjectByType<ConversationDirector>();
        if (existing != null) return Instance = existing;
        if (FindAnyObjectByType<PersonLife>() == null && FindAnyObjectByType<WorkdayDirector>() == null && FindAnyObjectByType<Conversable>() == null) return null;
        return new GameObject("ConversationDirector_AUTO").AddComponent<ConversationDirector>();
    }

    void Awake() { if (Instance == null) Instance = this; }
    void OnEnable() { if (Instance == null) Instance = this; }
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        _memory.Clear();
    }

    // ── Tiempo de juego ────────────────────────────────────────────────────────────────────

    /// <summary>Segundos (Time.time) de un minuto de juego; sin reloj, 2,5 s (el día de 1 h real).</summary>
    public static float MinuteSecs
    {
        get
        {
            TimeController tc = TimeController.timeController;
            float m = tc != null ? tc.TimeSpeedMinuteSecs : 2.5f;
            return m > 0f && m < 30f ? m : 2.5f;   // sin Clock en la escena el reloj dice 60 s: las charlas irían a cámara lenta
        }
    }
    /// <summary>Segundos entre frases (legibles: entre 1,5 y 6 s de juego).</summary>
    public float LineSeconds => Mathf.Clamp(lineMinutes * MinuteSecs, 1.5f, 6f);

    // ── Quién es persona ───────────────────────────────────────────────────────────────────

    /// <summary>¿Charla <paramref name="a"/>? Las personas (PersonLife), el personal de la jornada y los Conversable; nunca el jugador.</summary>
    public static bool IsPerson(Anima a)
    {
        if (a == null) return false;
        if (a.CompareTag("Player") || a.GetComponent<PlayerBrain>() != null) return false;
        if (a.TryGetComponent(out AnimaController ac) && ac.Active is PlayerBrain) return false;
        if (a.TryGetComponent(out Conversable cv)) return !cv.optOut;
        if (a.GetComponent<PersonLife>() != null) return true;
        WorkdayDirector wd = WorkdayDirector.Instance;
        if (wd != null) { string n = a.name.Replace("_Post", ""); foreach (string s in wd.staff) if (s == n) return true; }
        return false;
    }

    /// <summary>¿Es un insecto-humano (linaje de insecto o cuerpo de insecto)?</summary>
    public static bool IsInsect(Anima a) => a != null && (a.GetComponent<FolkLineage>() != null || a.GetComponent<CubeInsect>() != null);

    /// <summary>«humano-humano», «insecto-insecto» o «humano-insecto».</summary>
    public static string PairKind(Anima a, Anima b)
    {
        bool ia = IsInsect(a), ib = IsInsect(b);
        return ia && ib ? "insecto-insecto" : !ia && !ib ? "humano-humano" : "humano-insecto";
    }

    void Refresh()
    {
        _people.Clear();
        foreach (PersonLife p in SceneCache.All<PersonLife>(2f)) Add(p != null ? p.GetComponent<Anima>() : null);
        foreach (Conversable c in SceneCache.All<Conversable>(2f)) Add(c != null ? c.GetComponent<Anima>() : null);
        WorkdayDirector wd = WorkdayDirector.Instance;
        if (wd != null)
            foreach (string n in wd.staff)
            {
                if (n == "Player") continue;
                GameObject g = WorkdayDirector.Body(n);
                Add(g != null ? g.GetComponent<Anima>() : null);
            }
        // Olvidar a quien ya no está (muertos o descargados).
        _tmpKeys.Clear();
        foreach (var kv in _lastPos) if (kv.Key == null) _tmpKeys.Add(kv.Key);
        foreach (Anima k in _tmpKeys) { _lastPos.Remove(k); _settled.Remove(k); _freeAt.Remove(k); _lastTalk.Remove(k); }
    }
    readonly List<Anima> _tmpKeys = new List<Anima>();

    void Add(Anima a)
    {
        if (a == null || !a.isActiveAndEnabled || _people.Contains(a) || !IsPerson(a)) return;
        _people.Add(a);
    }

    // ── Libre, urgente, quieto ─────────────────────────────────────────────────────────────

    /// <summary>¿Tiene <paramref name="a"/> una necesidad que ya no espera? (hambre, baño, sueño o angustia)</summary>
    public bool Urgent(Anima a, out string need)
    {
        need = null;
        if (a == null) return false;
        BodyMetabolism m = a.GetComponent<BodyMetabolism>();
        if (m != null && m.hunger >= urgentAbove) need = "hambre";
        else if (m != null && m.toilet >= urgentAbove) need = "baño";
        else if (a.sleepiness >= urgentAbove) need = "sueño";
        else if (a.stress >= 0.92f) need = "angustia";
        return need != null;
    }

    /// <summary>¿Puede hablar ahora? Si no, por qué (para cortar una charla).</summary>
    public string Busy(Anima a)
    {
        if (a == null || a.death) return "ya no está";
        if (a.TryGetComponent(out PersonLife pl) && pl.Dead) return "ya no está";
        if (a.asleep) return "se ha dormido";
        if (Urgent(a, out string need)) return "necesidad: " + need;
        if (NeedsDriver.Attending(a.GetComponent<ActionQueue>())) return "necesidad: la está atendiendo";
        GameObject g = a.gameObject;
        if (Orchestra.Busy(g)) return "una iniciación le necesita";
        if (a.TryGetComponent(out MicroJornada mj) && mj.hold) return "la historia le necesita";
        if (RestaurantService.Drives(g)) return "sirve en el restaurante";
        if (TreeTrip.Busy(g)) return "va de viaje";
        return null;
    }

    bool CoolingDown(Anima a) => _freeAt.TryGetValue(a, out float t) && Time.time < t;
    static long PairKey(Anima a, Anima b)
    {
        int x = a.GetHashCode(), y = b.GetHashCode();   // (el hash de un Object es su identificador)
        if (x > y) { int s = x; x = y; y = s; }
        return ((long)x << 32) ^ (uint)y;
    }
    bool PairCooling(Anima a, Anima b) => _pairFreeAt.TryGetValue(PairKey(a, b), out float t) && Time.time < t;

    /// <summary>Ganas de hablar (0–1): sube con el tiempo sin charlar (2 h de juego hasta el máximo).</summary>
    public float SocialNeed(Anima a)
    {
        if (!_lastTalk.TryGetValue(a, out float t)) return 1f;
        return Mathf.Clamp01((Time.time - t) / (120f * MinuteSecs));
    }

    public bool InConversation(Anima a) { foreach (Conversation c in Active) if (c.a == a || c.b == a) return true; return false; }
    public Conversation Of(Anima a) { foreach (Conversation c in Active) if (c.a == a || c.b == a) return c; return null; }

    // ── El bucle ───────────────────────────────────────────────────────────────────────────

    void Update()
    {
        if (!Running) { if (Active.Count > 0) EndAll("pausa"); return; }
        float now = Time.time;
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            if (i >= Active.Count) continue;
            Conversation c = Active[i];
            if (now >= c.checkAt)
            {
                c.checkAt = now + 0.5f;
                string why = Breaks(c);
                if (why != null) { End(c, why, true); continue; }
            }
            if (now - c.startedAt > maxMinutes * MinuteSecs) { End(c, "se alarga demasiado", true); continue; }
            if (now >= c.nextLineAt)
            {
                if (c.next >= c.script.Count) End(c, "fin", false);
                else SayNext(c);
            }
        }
        if (Active.Count > PeakConcurrent) PeakConcurrent = Active.Count;
        if (now < _nextScan) return;
        _nextScan = now + scanSeconds;
        if (now >= _nextRefresh) { _nextRefresh = now + 5f; Refresh(); }
        Scan();
    }

    string Breaks(Conversation c)
    {
        string wa = Busy(c.a); if (wa != null) return $"{Name(c.a)}: {wa}";
        string wb = Busy(c.b); if (wb != null) return $"{Name(c.b)}: {wb}";
        float h = Mathf.Max(PersonLife.HeightOf(c.a.transform), PersonLife.HeightOf(c.b.transform));
        if (Flat(c.a.transform.position - c.b.transform.position) > breakHeights * h) return "se han separado";
        return null;
    }

    void Scan()
    {
        // Quieto = se ha movido menos de un tercio de su estatura desde la vuelta anterior (no se habla de camino al puesto).
        foreach (Anima a in _people)
        {
            if (a == null) continue;
            Vector3 p = a.transform.position;
            float h = PersonLife.HeightOf(a.transform);
            _settled[a] = _lastPos.TryGetValue(a, out Vector3 was) && (p - was).magnitude < 0.33f * h;
            _lastPos[a] = p;
        }
        if (Active.Count >= maxConcurrent) return;

        _free.Clear(); _taken.Clear();
        foreach (Anima a in _people)
            if (a != null && !InConversation(a) && !CoolingDown(a) && _settled.TryGetValue(a, out bool s) && s && Busy(a) == null) _free.Add(a);
        if (_free.Count < 2) return;
        // Orden al azar (nadie tiene siempre la primera palabra).
        for (int i = _free.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); Anima t = _free[i]; _free[i] = _free[j]; _free[j] = t; }

        int started = 0;
        foreach (Anima a in _free)
        {
            if (started >= maxStartsPerScan || Active.Count >= maxConcurrent) break;
            if (_taken.Contains(a)) continue;
            Anima best = null; float bestScore = float.MinValue; string bestShared = null;
            foreach (Anima b in _free)
            {
                if (b == a || _taken.Contains(b) || b.gameObject.scene != a.gameObject.scene || PairCooling(a, b)) continue;
                float h = Mathf.Max(PersonLife.HeightOf(a.transform), PersonLife.HeightOf(b.transform));
                float range = rangeHeights * h;
                float d = Flat(a.transform.position - b.transform.position);
                if (d > range) continue;
                string shared = ConversationTopics.SharedActivity(a, b);
                float score = (1f - d / range) * 2f + Bond(a, b) / 50f + (shared != null ? 0.6f : 0f) + 0.5f * (SocialNeed(a) + SocialNeed(b)) * 0.5f;
                if (score > bestScore) { bestScore = score; best = b; bestShared = shared; }
            }
            if (best == null) continue;
            float soc = Mathf.Clamp(0.5f * (a.sociability + best.sociability), 0.3f, 1.5f);
            float want = 0.5f + 0.5f * Mathf.Max(SocialNeed(a), SocialNeed(best));
            float chance = startChance * soc * want * (OnShift(a) || OnShift(best) ? onShiftFactor : 1f);
            if (Random.value >= chance) continue;
            Begin(a, best, bestShared);
            _taken.Add(a); _taken.Add(best);
            started++;
        }
    }

    static bool OnShift(Anima a)
    {
        if (a.TryGetComponent(out MicroJornada mj) && mj.OnShift) return true;
        return WorkdayDirector.Instance != null && WorkdayDirector.Instance.OnShift(a.gameObject);
    }

    // ── Empezar, hablar, terminar ──────────────────────────────────────────────────────────

    /// <summary>Empieza una charla entre <paramref name="a"/> (abre) y <paramref name="b"/> (pruebas: forzarla).</summary>
    public Conversation Begin(Anima a, Anima b, string shared = null)
    {
        if (a == null || b == null || a == b || InConversation(a) || InConversation(b)) return null;
        ConversationTopics.Script s = ConversationTopics.Compose(a, b, shared ?? ConversationTopics.SharedActivity(a, b), RecentTopics(a, b));
        var c = new Conversation
        {
            a = a, b = b, kind = PairKind(a, b), topic = s.topic, topicTitle = s.title, positive = s.positive,
            script = s.lines, startedAt = Time.time, nextLineAt = Time.time + 0.2f, checkAt = Time.time + 0.5f,
            bondAB0 = Bond(a, b), bondBA0 = Bond(b, a), stressA0 = a.stress, stressB0 = b.stress,
            day = TimeController.timeController != null ? TimeController.timeController.Day : 0,
            hour = TimeController.timeController != null ? TimeController.timeController.Hour : 0f,
        };
        Active.Add(c);
        if (Active.Count > PeakConcurrent) PeakConcurrent = Active.Count;
        Debug.Log($"[Charla] «{Name(a)}» y «{Name(b)}» ({c.kind}) hablan de {c.topicTitle}{(c.positive ? "" : " (de mal humor)")}.");
        OnEvent?.Invoke(c, "inicio");
        return c;
    }

    void SayNext(Conversation c)
    {
        int i = c.next++;
        Anima speaker = (i % 2 == 0) ? c.a : c.b, listener = speaker == c.a ? c.b : c.a;   // TURNOS: A, B, A, B…
        string text = c.script[i];
        c.said.Add(new Conversation.Line { who = speaker, text = text, time = Time.time });
        c.current = text; c.currentSpeaker = speaker; c.shownAt = Time.time;
        c.nextLineAt = Time.time + LineSeconds * Random.Range(0.85f, 1.15f);
        Affect(speaker, listener, c.positive);
        Debug.Log($"[Charla] «{Name(speaker)}»: «{text}»");
        OnEvent?.Invoke(c, "linea");
    }

    // Lo que deja una frase: vínculo (más a quien escucha), estrés y humores de quien escucha hacia el tono de la charla (contagio
    // emocional, como CommunicationSpell), desahogo de quien habla, y la cara de los dos.
    void Affect(Anima speaker, Anima listener, bool positive)
    {
        float k = positive ? 1f : 0.35f;
        listener.GrowBond(Target(speaker), BondType.Friend, bondPerLine * k);
        speaker.GrowBond(Target(listener), BondType.Friend, bondPerLine * k * 0.5f);
        listener.stress = Mathf.Clamp01(listener.stress + (positive ? -stressPerLine : stressPerLine * 0.5f));
        speaker.stress = Mathf.Clamp01(speaker.stress - stressPerLine * 0.5f);   // contarlo alivia
        if (listener.TryGetComponent(out Mind lm)) lm.humores.Produce(positive ? Humor.Serotonina : Humor.Cortisol, 0.04f);
        if (speaker.TryGetComponent(out Mind sm)) sm.humores.Produce(Humor.Serotonina, 0.02f);
        float secs = LineSeconds * 1.2f;
        listener.GetComponentInChildren<CubeFace>()?.Show(positive ? CubeFace.Expression.Happy : CubeFace.Expression.Sad, secs);
        speaker.GetComponentInChildren<CubeFace>()?.Show(positive ? CubeFace.Expression.Happy : CubeFace.Expression.Neutral, secs);
    }

    /// <summary>Termina una charla: <paramref name="interrupted"/> = cortada (necesidad, separación…), si no, acabó su guion.</summary>
    public void End(Conversation c, string reason, bool interrupted)
    {
        if (c == null || c.over) return;
        c.over = true; c.interrupted = interrupted; c.endReason = reason; c.endedAt = Time.time;
        Active.Remove(c);
        if (c.a != null && c.b != null)
        {
            if (!interrupted && c.positive) { c.a.GrowBond(Target(c.b), BondType.Friend, bondPerLine); c.b.GrowBond(Target(c.a), BondType.Friend, bondPerLine); }
            c.bondAB1 = Bond(c.a, c.b); c.bondBA1 = Bond(c.b, c.a); c.stressA1 = c.a.stress; c.stressB1 = c.b.stress;
            float now = Time.time;
            float cd = Mathf.Clamp(personCooldownMinutes * MinuteSecs, 5f, 900f);
            _freeAt[c.a] = now + cd * Random.Range(0.8f, 1.2f); _freeAt[c.b] = now + cd * Random.Range(0.8f, 1.2f);
            _pairFreeAt[PairKey(c.a, c.b)] = now + Mathf.Clamp(pairCooldownMinutes * MinuteSecs, 5f, 1800f);
            _lastTalk[c.a] = now; _lastTalk[c.b] = now;
            if (c.said.Count > 0) { Remember(c.a, c.b, c.topic); Remember(c.b, c.a, c.topic); }
        }
        Records.Add(c);
        if (Records.Count > 200) Records.RemoveAt(0);
        Debug.Log($"[Charla] «{Name(c.a)}» y «{Name(c.b)}» {(interrupted ? "cortan la charla (" + reason + ")" : "terminan de hablar")} tras {c.said.Count} frases; " +
                  $"vínculo {c.bondAB0:0.0}→{c.bondAB1:0.0} / {c.bondBA0:0.0}→{c.bondBA1:0.0}, estrés {c.stressA0:0.00}→{c.stressA1:0.00} / {c.stressB0:0.00}→{c.stressB1:0.00}.");
        OnEvent?.Invoke(c, interrupted ? "interrumpida" : "fin");
    }

    public void EndAll(string reason) { for (int i = Active.Count - 1; i >= 0; i--) if (i < Active.Count) End(Active[i], reason, true); }

    /// <summary>Pruebas: todos pueden volver a hablar ya (sin esperas entre charlas).</summary>
    public void ResetCooldowns() { _freeAt.Clear(); _pairFreeAt.Clear(); }

    // ── Memoria ────────────────────────────────────────────────────────────────────────────

    static void Remember(Anima who, Anima with, string topic)
    {
        if (!_memory.TryGetValue(who, out List<string> m)) _memory[who] = m = new List<string>();
        m.Add($"{topic}|{Name(with)}");
        if (m.Count > 8) m.RemoveAt(0);
    }

    /// <summary>Lo último que ha hablado <paramref name="who"/> («tema|con quién»; las 8 últimas charlas).</summary>
    public static IReadOnlyList<string> MemoryOf(Anima who) => who != null && _memory.TryGetValue(who, out List<string> m) ? m : (IReadOnlyList<string>)System.Array.Empty<string>();

    static HashSet<string> RecentTopics(Anima a, Anima b)
    {
        var set = new HashSet<string>();
        foreach (string s in MemoryOf(a)) { int p = s.IndexOf('|'); if (p > 0 && s.Substring(p + 1) == Name(b)) set.Add(s.Substring(0, p)); }
        return set;
    }

    // ── Utilidades ─────────────────────────────────────────────────────────────────────────

    /// <summary>El ITarget de una persona (los humanos no lo son por sí mismos: se les pone el adaptador AnimaTarget).</summary>
    public static ITarget Target(Anima a)
    {
        if (a == null) return null;
        ITarget t = a.GetComponent<ITarget>();
        return t ?? a.gameObject.AddComponent<AnimaTarget>();
    }

    /// <summary>Vínculo (0–100) de <paramref name="from"/> hacia <paramref name="to"/>.</summary>
    public static float Bond(Anima from, Anima to)
    {
        if (from == null || to == null) return 0f;
        ITarget t = to.GetComponent<ITarget>();
        Bond b = t != null ? from.GetBond(t) : null;
        return b != null ? b.value : 0f;
    }

    /// <summary>El nombre que se ve (sin lo que está haciendo, que la jornada añade al rótulo).</summary>
    public static string Name(Anima a)
    {
        if (a == null) return "?";
        if (a.TryGetComponent(out MicroJornada mj)) return mj.DisplayName;
        if (a.TryGetComponent(out Mind m) && !string.IsNullOrEmpty(m.identity)) return m.identity;
        string n = a.TryGetComponent(out WorldLabel wl) && !string.IsNullOrEmpty(wl.text) ? wl.text : a.name;
        int cut = n.IndexOf('\n'); if (cut > 0) n = n.Substring(0, cut);
        cut = n.IndexOf(" · "); if (cut > 0) n = n.Substring(0, cut);
        return n.Replace("_Post", "");
    }

    /// <summary>El nombre de pila (la primera palabra).</summary>
    public static string Short(Anima a) { string n = Name(a); int p = n.IndexOf(' '); return p > 0 ? n.Substring(0, p) : n; }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }

    // ── Bocadillos (como los nombres: OnGUI proyectado, solo los cercanos a la cámara) ─────

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint || Active.Count == 0) return;
        Camera cam = Camera.main;
        if (cam == null) return;
        if (_style == null) _style = new GUIStyle(GUI.skin.box) { fontSize = 12, wordWrap = true, richText = true, alignment = TextAnchor.MiddleCenter };
        float linger = LineSeconds * 1.6f;
        foreach (Conversation c in Active)
        {
            Anima s = c.currentSpeaker;
            if (s == null || string.IsNullOrEmpty(c.current) || Time.time - c.shownAt > linger) continue;
            float h = PersonLife.HeightOf(s.transform);
            Vector3 w = s.transform.position + Vector3.up * (h + 0.75f * h / 1.75f);
            if ((w - cam.transform.position).sqrMagnitude > maxDrawDistance * maxDrawDistance) continue;
            Vector3 sp = cam.WorldToScreenPoint(w);
            if (sp.z <= 0f) continue;
            Color col = c.positive ? ElementsStatus.ColorOf(ElementLevel.Ideal) : ElementsStatus.ColorOf(ElementLevel.Deficiente);
            var content = new GUIContent($"<size=10><b>{Short(s)}</b> · {c.topicTitle}</size>\n<color=#{ColorUtility.ToHtmlStringRGB(col)}>«{c.current}»</color>");
            float width = 230f, height = _style.CalcHeight(content, width);
            GUI.Box(new Rect(sp.x - width * 0.5f, Screen.height - sp.y - height, width, height), content, _style);
            c.drawn++; BubblesDrawn++;
        }
    }
}

/// <summary>Una charla entre dos personas (la lleva <see cref="ConversationDirector"/>).</summary>
public class Conversation
{
    public struct Line { public Anima who; public string text; public float time; }

    public Anima a, b;                       // a abre; luego se turnan
    public string kind;                      // humano-humano, insecto-insecto, humano-insecto
    public string topic, topicTitle;         // clave del tema (trabajo, familia, era…) y cómo se ve
    public bool positive;                    // el tono (buena charla / quejas)
    public List<string> script;              // el guion (A, B, A, B…)
    public readonly List<Line> said = new List<Line>();
    public int next;
    public string current; public Anima currentSpeaker; public float shownAt; public int drawn;
    public float startedAt, endedAt, nextLineAt, checkAt;
    public bool over, interrupted; public string endReason;
    public float bondAB0, bondBA0, bondAB1, bondBA1, stressA0, stressB0, stressA1, stressB1;
    public int day; public float hour;

    public bool Has(Anima x) => x == a || x == b;
    public Anima Other(Anima x) => x == a ? b : a;
}
