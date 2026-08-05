using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Primera virtualización de YOGA — **upa-yoga de cuello** ("Yoga for Success" de Sadhguru) en el Mesocosmos
/// (docs/upa-yoga-mission.md). Enseña el cuerpo **por partes**: en cada fase, los grupos de teclas
/// (**WASD** = izquierda, **IJKL** = derecha; los de caminar y de <see cref="HeadLook"/>) **cambian de
/// dueño** y pasan a controlar un **pie**, un **hombro** o el **cuello**.
///
/// La mecánica es de **ritmo**: las **fichas** llegan a los paneles-tecla (**caen** o **crecen** según
/// <see cref="tileMode"/>); si pulsas **justo**, **aciertas** (+punto); si no, **fallas** (−punto). El
/// movimiento **sucede** igual (va guiado), pero perder el ritmo **rompe la comunión**: **tiembla la parte del
/// cuerpo que se está moviendo** (el grupo de teclas activo, no el jugador entero) y se resienten
/// **aliento / energía / fatiga** (se repone descansando/comiendo). **Más puntos = más recompensa.** Las fichas
/// son (por dentro) **elementos** que, en orden, formarían los **compuestos** que el movimiento libera; por
/// fuera muestran **solo la letra de la tecla** — o el elemento si <see cref="showElement"/>.
///
/// Andamiaje OnGUI (UI-mix). Pendiente: articular el avatar rigged, suprimir el input normal, y cablear los
/// efectos a `PlayerStats`/humores de verdad (hoy son un medidor interno + logs).
/// </summary>
public class UpaYogaSession : MonoBehaviour
{
    [Tooltip("Arranca la sesión al empezar (para el sandbox).")]
    public bool autoStart = true;
    [Tooltip("Tecla para saltar de fase (la de interacción del juego: F).")]
    public KeyCode nextKey = KeyCode.F;
    [Tooltip("Segundos de cada media respiración (inhala / exhala) — también el pulso de las fichas.")]
    public float halfBreath = 4f;

    [Header("Ritmo (Guitar-Hero)")]
    [Tooltip("Cuánto tarda una ficha en caer hasta la tecla.")]
    public float travelTime = 1.6f;
    [Tooltip("Ventana de acierto alrededor del momento exacto.")]
    public float hitWindow = 0.18f;
    public int notesPerPhase = 8;
    [Tooltip("false = la ficha muestra solo la letra de la tecla; true = muestra el elemento químico.")]
    public bool showElement = false;
    public enum TileMode { Fall, Grow }
    [Tooltip("Grow = la ficha aparece sobre la tecla y crece hasta llenarla (púlsala al llenarse); Fall = cae (Guitar-Hero).")]
    public TileMode tileMode = TileMode.Grow;

    [Header("Rig (opcional): asigna los huesos y el yoga los mueve; vacío = solo UI/scaffold")]
    [Tooltip("Si se asigna, resuelve neck/hombros desde la fuente única BodyPart (rig.Get). Si no, usa los Transform de abajo.")]
    public CreatureRig rig;
    public Transform neck;
    public Transform leftShoulder, rightShoulder;
    public float partSpeed = 90f, yawLimit = 70f, pitchLimit = 55f, shoulderLimit = 40f;

    static int _activeCount;
    public static bool Active => _activeCount > 0;

    // Carriles: 0..3 = WASD (grupo izquierdo), 4..7 = IJKL (grupo derecho).
    static readonly KeyCode[] LaneKey = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L };
    static readonly string[] LaneLetter = { "W", "A", "S", "D", "I", "J", "K", "L" };
    static readonly string[] LaneElement = { "Ca", "Mg", "K", "Na", "Fe", "Zn", "O", "H" }; // placeholder (enlazar con Chemistry)

    class Phase
    {
        public string title, subtitle, left, right, compound;
        public bool breath;
        public int[] pattern; // índices de carril; vacío = solo postura (sin fichas)
        public Phase(string t, string s, string l, string r, bool b, int[] pat, string comp)
        { title = t; subtitle = s; left = l; right = r; breath = b; pattern = pat; compound = comp; }
    }

    static readonly Phase[] Sequence =
    {
        new Phase("POSTURA BASE · PIES", "Sepáralos cómodos a la altura de las caderas, mirando al frente",
                  "Pie izquierdo", "Pie derecho", false, new int[0], ""),
        new Phase("POSTURA BASE · HOMBROS", "Bájalos y alinéalos (manos y espalda se acomodan solas)",
                  "Hombros", "Hombros", false, new int[0], ""),
        new Phase("CUELLO · 1 · Cabeceo", "Inhala arriba (I) · exhala abajo (K)",
                  "—", "Cuello", true, new[] { 4, 6 }, "flujo de oxigeno"),
        new Phase("CUELLO · 2 · Giro", "Inhala a los lados (J/L) · exhala al centro",
                  "—", "Cuello", true, new[] { 5, 7 }, "descarga cervical"),
        new Phase("CUELLO · 3 · Oreja al hombro", "Inhala al centro · exhala bajando la oreja (J/L)",
                  "—", "Cuello", true, new[] { 5, 7 }, "drenaje"),
        new Phase("CUELLO · 4 · Rotacion", "Sube por atras y baja por delante (I->L->K->J)",
                  "—", "Cuello", true, new[] { 4, 7, 6, 5 }, "circulacion completa"),
        new Phase("HOMBROS · Rotacion", "Inhala subiendo · exhala bajando · 3 atras + 3 adelante",
                  "Hombro izquierdo", "Hombro derecho", true, new[] { 4, 7, 6, 5, 0, 3, 2, 1 }, "liberacion de tension"),
    };

    class Note { public int lane; public float t; public string element; public bool done; public bool hit; }

    int _i = -1;
    bool _running;
    float _song, _breathT;
    readonly List<Note> _notes = new List<Note>();
    int _score, _hits, _misses;
    float _tremble, _energy = 0.7f, _fatigue = 0.3f;
    float _yaw, _pitch, _lSh, _rSh;                 // acumuladores del rig (parte activa)
    Quaternion _neckHome, _lShHome, _rShHome;       // rotaciones de reposo (para restaurar al terminar)

    void Start() { if (autoStart) Begin(); }

    public void Begin()
    {
        if (_running) return;
        _running = true; _activeCount++;
        _score = 0; _hits = 0; _misses = 0; _tremble = 0f; _energy = 0.7f; _fatigue = 0.3f;
        if (rig != null)   // fuente única de partes: resuelve los huesos desde BodyPart
        {
            if (rig.Get(BodyPart.Neck) != null) neck = rig.Get(BodyPart.Neck);
            if (rig.Get(BodyPart.ShoulderLeft) != null) leftShoulder = rig.Get(BodyPart.ShoulderLeft);
            if (rig.Get(BodyPart.ShoulderRight) != null) rightShoulder = rig.Get(BodyPart.ShoulderRight);
        }
        if (neck != null) _neckHome = neck.localRotation;
        if (leftShoulder != null) _lShHome = leftShoulder.localRotation;
        if (rightShoulder != null) _rShHome = rightShoulder.localRotation;
        StartPhase(0);
        Debug.Log("[UpaYoga] Sesion iniciada (ritmo). Pulsa la tecla justo cuando la ficha llega a su panel. F = saltar fase.");
    }

    public void End()
    {
        if (!_running) return;
        _running = false; _activeCount = Mathf.Max(0, _activeCount - 1);
        if (neck != null) neck.localRotation = _neckHome;
        if (leftShoulder != null) leftShoulder.localRotation = _lShHome;
        if (rightShoulder != null) rightShoulder.localRotation = _rShHome;
        string tier = _score >= 12 ? "GRANDE" : _score >= 6 ? "media" : _score > 0 ? "pequena" : "ninguna";
        Debug.Log($"[UpaYoga] Fin. Puntos={_score} (aciertos {_hits}/fallos {_misses}). Recompensa {tier}. " +
                  "Los movimientos van orquestados (siempre salen); los fallos solo temblaron y gastaron aliento/energia.");
    }

    void StartPhase(int idx)
    {
        _i = idx; _song = 0f; _breathT = 0f; _notes.Clear();
        _yaw = 0f; _pitch = 0f; _lSh = 0f; _rSh = 0f;
        if (_i < 0 || _i >= Sequence.Length) return;
        Phase p = Sequence[_i];
        if (p.pattern != null && p.pattern.Length > 0)
        {
            float beat = halfBreath;              // una ficha por media respiración
            float first = travelTime + 0.6f;
            for (int n = 0; n < notesPerPhase; n++)
            {
                int lane = p.pattern[n % p.pattern.Length];
                _notes.Add(new Note { lane = lane, t = first + n * beat, element = LaneElement[lane] });
            }
        }
    }

    void Update()
    {
        if (!_running) return;
        _song += Time.deltaTime;
        _breathT += Time.deltaTime; if (_breathT >= halfBreath * 2f) _breathT -= halfBreath * 2f;
        _tremble = Mathf.Max(0f, _tremble - Time.deltaTime * 0.6f);
        DriveBody();   // mueve el rig (si hay huesos asignados) con las teclas de la parte activa

        // Aciertos: al pulsar una tecla de carril, busca la ficha más cercana dentro de la ventana.
        for (int lane = 0; lane < LaneKey.Length; lane++)
        {
            if (!Input.GetKeyDown(LaneKey[lane])) continue;
            Note best = null; float bestD = hitWindow;
            foreach (Note nt in _notes)
            {
                if (nt.done || nt.lane != lane) continue;
                float d = Mathf.Abs(_song - nt.t);
                if (d <= bestD) { bestD = d; best = nt; }
            }
            if (best != null)
            {
                best.done = true; best.hit = true; _hits++; _score++;
                _energy = Mathf.Min(1f, _energy + 0.02f); _fatigue = Mathf.Max(0f, _fatigue - 0.01f);
            }
        }

        // Fallos: ficha que pasó su momento sin pulsarse → temblor + gasto.
        bool allDone = true; float lastT = 0f;
        foreach (Note nt in _notes)
        {
            if (nt.t > lastT) lastT = nt.t;
            if (nt.done) continue;
            if (_song > nt.t + hitWindow)
            {
                nt.done = true; _misses++; _score = Mathf.Max(0, _score - 1);
                _tremble = Mathf.Min(1f, _tremble + 0.35f);
                _energy = Mathf.Max(0f, _energy - 0.04f); _fatigue = Mathf.Min(1f, _fatigue + 0.03f);
            }
            else allDone = false;
        }

        // Avance: F salta siempre; las fases de movimiento auto-avanzan al acabar; las de postura esperan F.
        bool advance = Input.GetKeyDown(nextKey);
        if (!advance)
        {
            Phase p = Sequence[_i];
            if (p.pattern != null && p.pattern.Length > 0 && allDone && _song > lastT + 1f) advance = true;
        }
        if (advance)
        {
            if (_i + 1 >= Sequence.Length) { End(); return; }
            StartPhase(_i + 1);
        }
    }

    // Mueve el RIG (si hay huesos asignados): en fases de cuello, IJKL rotan `neck`; en hombros, W/S→izq, I/K→der.
    // El temblor por perder comunión se suma como jitter a la parte activa. Sin hueso asignado, no-op (solo UI).
    void DriveBody()
    {
        float jit = Mathf.Sin(Time.time * 47f) * _tremble * 8f;
        if (_i >= 2 && _i <= 5)                                     // fases de CUELLO
        {
            if (neck == null) return;
            if (Input.GetKey(KeyCode.L)) _yaw += partSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.J)) _yaw -= partSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.K)) _pitch += partSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.I)) _pitch -= partSpeed * Time.deltaTime;
            _yaw = Mathf.Clamp(_yaw, -yawLimit, yawLimit);
            _pitch = Mathf.Clamp(_pitch, -pitchLimit, pitchLimit);
            neck.localRotation = _neckHome * Quaternion.Euler(_pitch + jit, _yaw + jit, 0f);
        }
        else if (_i == 6)                                           // fase de HOMBROS
        {
            if (rightShoulder != null)
            {
                float d = (Input.GetKey(KeyCode.I) ? 1f : 0f) - (Input.GetKey(KeyCode.K) ? 1f : 0f);
                _rSh = Mathf.Clamp(_rSh + d * partSpeed * Time.deltaTime, -shoulderLimit, shoulderLimit);
                rightShoulder.localRotation = _rShHome * Quaternion.Euler(_rSh + jit, 0f, 0f);
            }
            if (leftShoulder != null)
            {
                float d = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
                _lSh = Mathf.Clamp(_lSh + d * partSpeed * Time.deltaTime, -shoulderLimit, shoulderLimit);
                leftShoulder.localRotation = _lShHome * Quaternion.Euler(_lSh + jit, 0f, 0f);
            }
        }
    }

    // ── UI (mix OnGUI): título + subtítulo + aliento + fichas cayendo sobre las teclas (forma de teclado) ──
    void OnGUI()
    {
        if (!_running || _i < 0 || _i >= Sequence.Length) return;
        Phase p = Sequence[_i];
        float w = Screen.width, h = Screen.height;

        GUIStyle title = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 22, fontStyle = FontStyle.Bold };
        GUIStyle sub = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
        GUIStyle small = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };

        GUI.Label(new Rect(w * 0.5f - 300f, 30f, 600f, 30f), p.title, title);
        GUI.Label(new Rect(w * 0.5f - 300f, 62f, 600f, 24f), p.subtitle, sub);

        if (p.breath)
        {
            bool inh = _breathT < halfBreath;
            float f = inh ? (_breathT / halfBreath) : 1f - ((_breathT - halfBreath) / halfBreath);
            float bw = 260f, bx = w * 0.5f - bw * 0.5f, by = 92f;
            GUI.Box(new Rect(bx, by, bw, 12f), GUIContent.none);
            Color old = GUI.color; GUI.color = inh ? new Color(0.5f, 0.8f, 1f) : new Color(1f, 0.8f, 0.5f);
            GUI.Box(new Rect(bx, by, bw * Mathf.Clamp01(f), 12f), GUIContent.none); GUI.color = old;
            GUI.Label(new Rect(bx, by + 12f, bw, 18f), inh ? "INHALA" : "EXHALA", small);
        }

        // Geometría de teclas (forma de teclado): W sobre A-S-D; I sobre J-K-L. Son los objetivos de las fichas.
        float k = 40f, gap = 6f, keyY = h - 130f, fretTop = 150f;
        float cxL = w * 0.34f, cxR = w * 0.66f;
        float[] lx = { cxL, cxL - (k + gap), cxL, cxL + (k + gap), cxR, cxR - (k + gap), cxR, cxR + (k + gap) };
        float[] ly = { keyY - (k + gap), keyY, keyY, keyY, keyY - (k + gap), keyY, keyY, keyY };

        // ¿Qué grupos están "en movimiento" en esta fase? Solo ESOS tiemblan al perder comunión.
        bool leftActive = p.left != "—", rightActive = p.right != "—";
        float sh = _tremble * 7f;
        Vector2 shk = new Vector2(Mathf.Sin(Time.time * 40f) * sh, Mathf.Cos(Time.time * 37f) * sh);

        // Fichas: caen (Fall) o crecen sobre la tecla (Grow). Solo se dibujan cerca de su momento.
        foreach (Note nt in _notes)
        {
            if (nt.done) continue;
            float rel = nt.t - _song;                    // >0 aún no; ~0 en la tecla
            if (rel > travelTime || rel < -hitWindow) continue;
            float prog = Mathf.Clamp01(1f - (rel / travelTime)); // 0 lejos .. 1 en la tecla
            bool near = Mathf.Abs(rel) <= hitWindow;
            Vector2 off = LaneActive(nt.lane, leftActive, rightActive) ? shk : Vector2.zero;
            // Grow: la ficha va SOBRE la tecla (que ya muestra su letra) → muestra el ELEMENTO. Fall: la letra
            // (cae lejos de la tecla). `showElement` fuerza el elemento también en Fall.
            string cap = (tileMode == TileMode.Grow || showElement) ? nt.element : LaneLetter[nt.lane];
            Color old = GUI.color; GUI.color = near ? new Color(0.6f, 1f, 0.6f) : new Color(0.9f, 0.9f, 0.55f);
            if (tileMode == TileMode.Fall)
            {
                float y = Mathf.Lerp(fretTop, ly[nt.lane], prog);
                GUI.Box(new Rect(lx[nt.lane] - k * 0.45f + off.x, y - k * 0.45f + off.y, k * 0.9f, k * 0.9f), cap);
            }
            else // Grow: aparece pequeña sobre la tecla y crece hasta llenarla; hay que pulsarla al llenarse.
            {
                float s = Mathf.Lerp(k * 0.25f, k * 0.95f, prog);
                GUI.Box(new Rect(lx[nt.lane] - s * 0.5f + off.x, ly[nt.lane] - s * 0.5f + off.y, s, s), cap);
            }
            GUI.color = old;
        }

        // Teclas objetivo (resaltadas al pulsar). Tiemblan si SU parte se está moviendo (pérdida de comunión).
        for (int lane = 0; lane < LaneKey.Length; lane++)
        {
            Vector2 off = LaneActive(lane, leftActive, rightActive) ? shk : Vector2.zero;
            Color old = GUI.color;
            if (Application.isPlaying && Input.GetKey(LaneKey[lane])) GUI.color = new Color(0.6f, 1f, 0.6f);
            GUI.Box(new Rect(lx[lane] - k * 0.5f + off.x, ly[lane] - k * 0.5f + off.y, k, k), LaneLetter[lane]);
            GUI.color = old;
        }
        GUI.Label(new Rect(cxL - 90f, keyY + k * 0.5f + 6f, 180f, 20f), p.left, small);
        GUI.Label(new Rect(cxR - 90f, keyY + k * 0.5f + 6f, 180f, 20f), p.right, small);
        if (!string.IsNullOrEmpty(p.compound))
            GUI.Label(new Rect(w * 0.5f - 160f, keyY + k * 0.5f + 28f, 320f, 20f), "libera: " + p.compound, small);

        // HUD.
        int e = Mathf.RoundToInt(_energy * 100f), fa = Mathf.RoundToInt(_fatigue * 100f);
        GUI.Label(new Rect(20f, 20f, 280f, 22f), $"Puntos: {_score}   ok {_hits} / fallo {_misses}", small);
        GUI.Label(new Rect(20f, 42f, 280f, 20f), "Energia " + e + "%  ·  Fatiga " + fa + "%", small);
        GUI.Label(new Rect(w * 0.5f - 120f, h - 34f, 240f, 22f),
                  (_i < Sequence.Length - 1 ? "F -> siguiente" : "F -> terminar"), small);
    }

    // ¿El carril pertenece a un grupo que se está moviendo en esta fase? (0..3 = izq WASD, 4..7 = der IJKL)
    static bool LaneActive(int lane, bool leftActive, bool rightActive)
        => (lane < 4 && leftActive) || (lane >= 4 && rightActive);
}
