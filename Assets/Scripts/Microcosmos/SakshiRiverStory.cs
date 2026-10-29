using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL RÍO DE SAKSHI (docs/microcosmos-sakshi-origin.md): el microcosmos de la Sala de Yoga, al que la Magnate envía al
/// recién llegado tras su primera clase (Berón 2026-10-27). Antes de Ambrosio: el río arrastró a SAKSHI, una cría, lejos de
/// los suyos y la dejó en la orilla sin fuerzas; solo puede OBSERVAR.
///  1. El jugador (Kushal) llega a la orilla y la ve. Una araña pequeña, con poca hambre, se acerca con sigilo: prefiere
///     hormigas a gusanos y va a por ella.
///  2. El depredador es más débil que Kushal: basta EMPUJARLO (E junto a él, <see cref="PushSeconds"/> s cada empujón). Cada
///     empujón lo aparta, pero vuelve, y su hambre crece.
///  3. Tras <see cref="PushesNeeded"/> empujones el hambre le puede: carga contra Sakshi. Justo entonces llega la TRIBU (que
///     huía de los depredadores del norte hacia el único lugar seguro, la orilla) y entre todos lo echan.
///  4. Sakshi mira a Kushal: ahí nace su costumbre de quedarse observando. La tribu se la lleva; Kushal vuelve.
/// Una sola tecla (E) hace lo que toca (<see cref="Act"/>).
/// </summary>
public class SakshiRiverStory : MonoBehaviour
{
    public const float PushSeconds = 6f;
    public const int PushesNeeded = 3;
    public enum Beat { Intro, Stalk, Push, Charge, Tribe, Bond, Done }
    public Beat Now { get; private set; } = Beat.Intro;
    public static SakshiRiverStory Instance { get; private set; }
    public int Pushes { get; private set; }
    public bool PlayerPushing => _pushUntil > Time.time;
    public Transform Sakshi => _sakshi;
    public Transform Predator => _pred;

    Transform _sakshi, _pred, _player, _tribeAt;
    readonly List<Transform> _tribe = new List<Transform>();
    float _beatAt, _pushUntil, _nextBob;
    readonly HashSet<string> _said = new HashSet<string>();

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    Transform Find(string n) { foreach (Transform t in GetComponentsInChildren<Transform>(true)) if (t.name == n) return t; return null; }

    void Start()
    {
        _sakshi = Find("Sakshi"); _tribeAt = Find("Llegada_tribu");
        foreach (string n in new[] { "Hespero_joven", "Ruth_joven", "Anciana_Aurea", "Anciana_Vela" }) { Transform t = Find(n); if (t != null) _tribe.Add(t); }
        if (_sakshi == null) { Debug.LogWarning("[Río] Falta Sakshi."); enabled = false; return; }
        _sakshi.rotation *= Quaternion.Euler(0f, 0f, 80f);   // tumbada de lado, sin fuerzas
        // La ARAÑA PEQUEÑA (una cría de licósida, de poca hambre): más débil que Kushal; a la escala del microcosmos, 1,6 m.
        var go = new GameObject("Depredador_debil"); go.transform.SetParent(transform, true);
        CubeInsect.Dress(go, "Spider", new Color(0.42f, 0.32f, 0.22f), 1.6f);
        Vector3 side = Vector3.Cross(Vector3.up, _sakshi.forward).normalized;
        go.transform.position = Ground(_sakshi.position + side * 14f + _sakshi.forward * 6f);
        _pred = go.transform;
        Go(Beat.Intro);
    }

    void Go(Beat b) { Now = b; _beatAt = Time.time; Debug.Log($"[Río] → {b}"); }
    void Once(string k, bool when, System.Action a) { if (when && _said.Add(k)) a(); }
    static void Say(string who, string text, float s = 7f) { StorySubtitle.Show(who, text, s); Debug.Log($"[Río] {who}: «{text}»"); }
    // Frase dirigida a Kushal: él contesta (arriba, KushalVoice).
    static void Say(string who, string text, float s, KushalVoice.Ctx reply) { Say(who, text, s); KushalVoice.Reply(reply); }
    static void Goal(string text) => LessonPrompt.Show(text, null, 9999f);

    Transform PlayerT()
    {
        if (_player == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); _player = p != null ? p.transform : null; }
        return _player;
    }

    Vector3 Ground(Vector3 p)
    {
        RaycastHit[] hits = Physics.RaycastAll(p + Vector3.up * 20f, Vector3.down, 60f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
        foreach (RaycastHit h in hits)
            if (h.collider.GetComponentInParent<Anima>() == null && h.collider.GetComponentInParent<CharacterController>() == null && (_pred == null || !h.collider.transform.IsChildOf(_pred))) return h.point;
        return p;
    }

    bool Walk(Transform t, Vector3 to, float stop, float speed)
    {
        if (t == null) return true;
        Vector3 d = to - t.position; d.y = 0f;
        if (d.magnitude <= stop) return true;
        t.position = Ground(t.position + Vector3.ClampMagnitude(d, speed * Time.deltaTime));
        t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(d), Time.deltaTime * 6f);
        return false;
    }

    static bool Near(Transform a, Transform b, float r) { if (a == null || b == null) return false; Vector3 d = a.position - b.position; d.y = 0f; return d.magnitude <= r; }

    // ── LA TECLA E ─────────────────────────────────────────────────────────────────────────────────────────────
    public string Act()
    {
        Transform p = PlayerT(); if (p == null) return "sin jugador";
        if ((Now == Beat.Stalk || Now == Beat.Push) && _pred != null)
        {
            if (PlayerPushing) return "ya empujas";
            if (!Near(p, _pred, 3.4f)) return "acércate a la araña";
            _pushUntil = Time.time + PushSeconds;
            if (Now == Beat.Stalk) Go(Beat.Push);
            return "empujas a la araña";
        }
        return "nada que hacer aquí";
    }

    void Update()
    {
        Transform p = PlayerT(); if (p == null || _sakshi == null) return;
        if (Input.GetKeyDown(KeyCode.E)) Debug.Log($"[Río] (E) {Act()}");
        float t = Time.time - _beatAt;
        switch (Now)
        {
            case Beat.Intro:   // llega a la orilla; la ve
                if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsBusy) { _beatAt = Time.time; break; }
                Once("narra", t > 1f, () => Say("", "La orilla del río, al amanecer. Una cría de hormiga yace de lado, empapada: el agua la ha traído desde muy lejos. Solo puede mirar.", 8f));
                Once("sakshi1", t > 5f, () => Say("Sakshi", "…¿Quién eres? No puedo… moverme.", 6f, KushalVoice.Ctx.Comfort));
                if (t > 8f) { Go(Beat.Stalk); Goal("Protege a Sakshi: empuja a la araña (E junto a ella)"); Say("", "Entre la hierba, una araña pequeña se acerca despacio. Prefiere las hormigas a los gusanos.", 7f); }
                break;
            case Beat.Stalk:   // se acerca con sigilo
                Walk(_pred, _sakshi.position + (_pred.position - _sakshi.position).normalized * 1.6f, 0.1f, 0.6f);
                Once("sakshi2", t > 6f, () => Say("Sakshi", "…Viene hacia mí.", 4f, KushalVoice.Ctx.Comfort));
                break;
            case Beat.Push:
                if (PlayerPushing)
                {
                    // Kushal se pone entre las dos y la empuja: la araña retrocede despacio.
                    Vector3 away = (_pred.position - _sakshi.position); away.y = 0f; away = away.sqrMagnitude > 0.01f ? away.normalized : Vector3.forward;
                    Walk(_pred, _pred.position + away, 0f, 1.2f);
                    Vector3 spot = _pred.position - away * 2.2f; spot.y = p.position.y;
                    WorkdayDirector.Teleport(p, Vector3.Lerp(p.position, spot, Time.deltaTime * 5f));
                    p.rotation = Quaternion.LookRotation(away);
                    if (Time.time >= _nextBob) { _nextBob = Time.time + 1.2f; PushPose(p, Mathf.Sin(Time.time * 2.6f) > 0f); }
                }
                else
                {
                    if (_pushUntil > 0f) { _pushUntil = 0f; Pushes++; PushPose(p, false); Debug.Log($"[Río] Empujón {Pushes}/{PushesNeeded}."); Goal($"Protege a Sakshi: empuja a la araña (E junto a ella) · {Pushes}/{PushesNeeded}"); }
                    if (Pushes == 1) Once("narra2", true, () => Say("", "La araña se aparta… y vuelve. Cada vez tiene más hambre.", 6f));
                    if (Pushes >= PushesNeeded) { Go(Beat.Charge); Say("", "El hambre le puede: ya no le importa el empujón. Carga contra Sakshi.", 6f); break; }
                    Walk(_pred, _sakshi.position + (_pred.position - _sakshi.position).normalized * 1.6f, 0.1f, 0.6f + Pushes * 0.3f);
                }
                break;
            case Beat.Charge:   // carga… y llega la tribu
                Walk(_pred, _sakshi.position + (_pred.position - _sakshi.position).normalized * 1.8f, 0.1f, 3f);
                int i = 0;
                foreach (Transform m in _tribe) { Walk(m, _sakshi.position + Quaternion.Euler(0f, 60f * i - 90f, 0f) * Vector3.forward * 3f, 0.2f, 4.5f); i++; }
                Once("hespero", t > 2f, () => Say("Héspero (joven)", "¡Allí, en la orilla! ¡Una cría! ¡Rodead a la araña!", 5f));
                if (t > 6f) { Go(Beat.Tribe); Goal(""); LessonPrompt.Show("", null, 0.1f); }
                break;
            case Beat.Tribe:   // entre todos la echan
                Vector3 off = (_pred.position - _sakshi.position); off.y = 0f; off = off.sqrMagnitude > 0.01f ? off.normalized : Vector3.right;
                Walk(_pred, _pred.position + off * 3f, 0f, 3.5f);
                Once("ruth", t > 2f, () => Say("Ruth (joven)", "¡Se va! Tranquila, pequeña, ya estás con nosotros.", 6f));
                if (t > 6f) { Destroy(_pred.gameObject); _pred = null; Go(Beat.Bond); }
                break;
            case Beat.Bond:   // Sakshi mira a Kushal; la tribu se la lleva
                {
                    PlayerController pc = p.GetComponent<PlayerController>(); if (pc != null && t < 1f) pc.LookAtPoint(_sakshi.position + Vector3.up * 0.4f);
                    Once("sakshi3", t > 1f, () => Say("Sakshi", "…Me has protegido. Te recordaré… y miraré, como tú me miras.", 7f, KushalVoice.Ctx.Thanks));
                    Once("aurea", t > 6f, () => Say("Anciana Áurea", "Gracias, gusano. Nos la llevamos a casa; tú vuelve a tu mundo.", 6f, KushalVoice.Ctx.Farewell));
                    if (t > 12f)
                    {
                        Go(Beat.Done);
                        Progress.MarkStory(gameObject.scene.name);
                        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) MobWorldLoader.Instance.ExitMobWorld();
                    }
                }
                break;
        }
    }

    static void PushPose(Transform p, bool b)
    {
        Anima a = p != null ? p.GetComponent<Anima>() : null; if (a == null) return;
        if (!b) { DailyPoses.Stand().ApplyFinal(a); return; }
        DailyPoses.Make("empuja", -1f, (BodyPart.Back, new Vector3(26, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-80, 0, 8)), (BodyPart.ShoulderRight, new Vector3(-80, 0, -8)),
            (BodyPart.ThighLeft, new Vector3(-35, 0, 0)), (BodyPart.ShinLeft, new Vector3(40, 0, 0)), (BodyPart.ThighRight, new Vector3(20, 0, 0))).ApplyFinal(a);
    }
}
