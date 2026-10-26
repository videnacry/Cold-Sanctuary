using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA HISTORIA DE AMBROSIO (primer microcosmos del recién llegado; Berón 2026-10-26). El jugador aparece y ve, delante,
/// a Ambrosio, sus hermanas (Medea y Momo) y Atlas EMPUJANDO a los dos viejos (Anciano_Pintor y Shanidar) hacia la cueva:
///  1. Ambrosio va hacia él: «salva a mi familia»; lo lleva junto a Shanidar y le pide que empuje; vuelve a su viejo.
///  2. Empujar 1 min → Shanidar se DERRUMBA: tomar la MELAZA de Ambrosio y dársela; se levanta; seguir empujando.
///  3. Otro minuto → llega un DEPREDADOR: los viejos se quedan quietos y los demás van a empujarlo; el jugador también:
///     cada empujón es una animación de <see cref="PredatorPushSeconds"/> s; con <see cref="PredatorPushesNeeded"/> se va.
///  4. Todos vuelven a los viejos y empujan 1 min más hasta la entrada de la cueva → Ambrosio se derrumba y MUERE.
///  5. Sakshi se acerca y le dice que vuelva al mesocosmos → sale.
/// Una sola tecla (E) hace lo que toca según dónde está (<see cref="Act"/>); el test de experiencia la llama igual.
/// </summary>
public class AmbrosioStory : MonoBehaviour
{
    public const float PushPhaseSeconds = 60f, PredatorPushSeconds = 10f;
    public const int PredatorPushesNeeded = 5;
    [Tooltip("Cuánto crece el elenco (las hormigas a la escala del jugador).")]
    public float castScale = 2.5f;
    [Tooltip("Distancia de la salida a la cueva (m).")]
    public float path = 24f;

    public enum Beat { Intro, AskHelp, Push1, Collapse, Push2, Predator, Push3, Death, Sakshi, Done }
    public Beat Now { get; private set; } = Beat.Intro;
    public static AmbrosioStory Instance { get; private set; }

    /// <summary>Segundos que el jugador lleva empujando en el tramo actual; empujones al depredador; ¿lleva melaza?</summary>
    public float PushedThisPhase { get; private set; }
    /// <summary>A qué distancia del jugador estaba Ambrosio cuando le pidió ayuda (−1: aún no).</summary>
    public float AskedAtDistance { get; private set; } = -1f;
    public int PredatorPushes { get; private set; }
    public bool CarryingHoneydew => _drop != null;
    public bool PlayerPushing => _pushTarget != null;
    public bool PlayerPushingPredator => _predatorPushUntil > Time.time;
    public Transform Ambrosio => _ambrosio;
    public Transform Shanidar => _oldB;
    public Transform Predator => _predator;
    public Transform Sakshi => _sakshi;
    public Transform Cave => _cave;

    Transform _ambrosio, _medea, _momo, _atlas, _sakshi, _oldA, _oldB, _cave, _predator, _player;
    Vector3 _dir, _caveAt;
    float _t0, _beatAt, _predatorPushUntil, _nextBob;
    Transform _pushTarget, _drop;
    bool _collapsed, _ambrosioFree;
    readonly Dictionary<Transform, Transform> _pushes = new Dictionary<Transform, Transform>();   // quién empuja a quién
    readonly Dictionary<Transform, float> _travel = new Dictionary<Transform, float>();           // metros recorridos

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    Transform Find(string n) { foreach (Transform t in GetComponentsInChildren<Transform>(true)) if (t.name == n) return t; return null; }

    void Start()
    {
        _ambrosio = Find("Ambrosio"); _medea = Find("Medea"); _momo = Find("Momo"); _atlas = Find("Atlas"); _sakshi = Find("Sakshi");
        _oldA = Find("Anciano_Pintor"); _oldB = Find("Shanidar"); _cave = Find("Cueva");
        if (_ambrosio == null || _oldA == null || _oldB == null || _cave == null) { Debug.LogWarning("[Ambrosio] Falta el elenco de la historia."); enabled = false; return; }
        // El tableau tenía su propio director e impulsos: aquí manda la historia.
        foreach (string n in new[] { "Level1Director", "TribeCohesion" }) foreach (MonoBehaviour mb in GetComponentsInChildren<MonoBehaviour>(true)) if (mb.GetType().Name == n) mb.enabled = false;
        // La cueva sigue siendo el REFUGIO (CarryToRefuge): los viejos (WeakOne) cuentan como a salvo al llegar.
        foreach (MonoBehaviour mb in _cave.GetComponents<MonoBehaviour>()) if (!(mb is CarryToRefuge)) mb.enabled = false;
        _dir = Vector3.back;   // de la salida a la cueva (−Z)
        _caveAt = Ground(_cave.position - _dir * 2.2f);
        Vector3 start = _caveAt - _dir * path;
        foreach (Transform t in new[] { _ambrosio, _medea, _momo, _atlas, _sakshi, _oldA, _oldB }) Take(t);
        Place(_oldA, start + new Vector3(-1.6f, 0f, 0f)); Place(_oldB, start + new Vector3(1.6f, 0f, 0f));
        Place(_ambrosio, start + new Vector3(-1.6f, 0f, 1.6f)); Place(_atlas, start + new Vector3(-2.4f, 0f, 1.4f));
        Place(_medea, start + new Vector3(1.0f, 0f, 1.5f)); Place(_momo, start + new Vector3(2.2f, 0f, 1.5f));
        if (_sakshi != null) Place(_sakshi, _caveAt + new Vector3(3f, 0f, 1f));
        _pushes[_ambrosio] = _oldA; if (_atlas != null) _pushes[_atlas] = _oldA;
        if (_medea != null) _pushes[_medea] = _oldB; if (_momo != null) _pushes[_momo] = _oldB;
        _travel[_oldA] = 0f; _travel[_oldB] = 0f;
        _t0 = Time.time; Go(Beat.Intro);
        Debug.Log("[Ambrosio] La historia empieza: la familia empuja a los viejos hacia la cueva.");
    }

    // Toma el control de un personaje del tableau: sin navegación ni impulsos, a la escala del jugador.
    void Take(Transform t)
    {
        if (t == null) return;
        foreach (Behaviour b in t.GetComponents<Behaviour>())
        {
            string n = b.GetType().Name;
            if (n == "NavMeshAgent" || n == "SocialImpulse" || n == "ImpulseController" || n == "AiBrain" || n == "IdleBehavior" || n == "AnimaController"
                || n == "ActionQueue" || n == "HoneydewProducer" || n == "AcuteStressResponse" || n == "Mind" || n == "HomeImpulse") b.enabled = false;
        }
        Rigidbody rb = t.GetComponent<Rigidbody>(); if (rb != null) rb.isKinematic = true;
        // Cuerpo de cubos de hormiga humanizada (Ambrosio, de pulgón), con los pies en el suelo: el tableau era de cápsulas.
        MeshRenderer cap = t.GetComponent<MeshRenderer>();
        Color c = cap != null && cap.sharedMaterial != null ? cap.sharedMaterial.color : new Color(0.4f, 0.3f, 0.25f);
        if (cap != null) cap.enabled = false;
        t.localScale = Vector3.one;
        float h = t == _atlas ? 2.0f : (t == _oldA || t == _oldB) ? 1.6f : 1.75f;
        CubeInsect.DressMyrmidon(t.gameObject, c, h, t == _atlas ? 1.25f : 1f, aphid: t == _ambrosio);
        CapsuleCollider cc = t.GetComponent<CapsuleCollider>(); if (cc != null) { cc.center = Vector3.up * h * 0.5f; cc.height = h; cc.radius = 0.35f; }
    }

    Vector3 Ground(Vector3 p)
    {
        // El suelo, no las cabezas: antes el rayo daba en el propio personaje y cada paso lo subía un poco más (139 m).
        RaycastHit[] hits = Physics.RaycastAll(p + Vector3.up * 20f, Vector3.down, 60f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
        foreach (RaycastHit h in hits)
            if (h.collider.GetComponentInParent<Anima>() == null && h.collider.GetComponentInParent<CharacterController>() == null && (_predator == null || !h.collider.transform.IsChildOf(_predator))) return h.point;
        return p;
    }

    void Place(Transform t, Vector3 p) { if (t == null) return; t.position = Ground(p) /* (pies en el suelo) */; t.rotation = Quaternion.LookRotation(_dir); }

    Transform PlayerT()
    {
        if (_player == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); _player = p != null ? p.transform : null; }
        return _player;
    }

    void Go(Beat b) { Now = b; _beatAt = Time.time; Debug.Log($"[Ambrosio] → {b}"); }

    void Say(Transform who, string text, float secs = 7f)
    {
        string n = who != null ? who.name : "";
        PlaneMessenger.Send(n, text, secs);
        Debug.Log($"[Ambrosio] {n}: «{text}»");
    }

    void Goal(string text) { LessonPrompt.Show(text, null, 9999f); }

    // ── LA TECLA E: lo que toca según dónde está el jugador ─────────────────────────────────────────────────────
    public string Act()
    {
        Transform p = PlayerT(); if (p == null) return "sin jugador";
        if (Now == Beat.Predator && _predator != null)
        {
            if (Near(p, _predator, 3.2f) && !PlayerPushingPredator) { _predatorPushUntil = Time.time + PredatorPushSeconds; StopPushing(); return "empujas al depredador"; }
            return "acércate al depredador";
        }
        if (_collapsed)
        {
            if (_drop != null && Near(p, _oldB, 2.4f)) { Feed(); return "le das la melaza"; }
            if (_drop == null && Near(p, _ambrosio, 2.4f)) { TakeHoneydew(); return "tomas la melaza de Ambrosio"; }
            StopPushing();
            return _drop == null ? "ve con Ambrosio a por su melaza" : "llévale la melaza a Shanidar";
        }
        if (_pushTarget != null) { StopPushing(); return "dejas de empujar"; }
        foreach (Transform o in new[] { _oldB, _oldA })
            if (o != null && Near(p, o, 2.6f)) { StartPushing(o); return $"empujas a {o.name}"; }
        return "nada que hacer aquí";
    }

    static bool Near(Transform a, Transform b, float r) { if (a == null || b == null) return false; Vector3 d = a.position - b.position; d.y = 0f; return d.magnitude <= r; }

    void StartPushing(Transform old)
    {
        _pushTarget = old;
        PlayerController pc = PlayerT().GetComponent<PlayerController>(); if (pc != null) pc.HoldBody(true);
    }

    void StopPushing()
    {
        if (_pushTarget == null) return;
        _pushTarget = null;
        PlayerController pc = PlayerT() != null ? PlayerT().GetComponent<PlayerController>() : null; if (pc != null) pc.HoldBody(false);
        Pose(PlayerT(), false, false);
    }

    void TakeHoneydew()
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(g.GetComponent<Collider>());
        g.name = "Gota_de_melaza"; g.transform.SetParent(PlayerT(), false);
        g.transform.localPosition = new Vector3(0.25f, 1.1f, 0.45f); g.transform.localScale = Vector3.one * 0.25f;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.98f, 0.82f, 0.3f));
        _drop = g.transform;
        Honeydew().AddCharge();   // la melaza es una carga del hechizo de la maleza (HoneydewSpell), como la de las gotas del suelo
        Say(_ambrosio, "Toma, es mi melaza. ¡Corre, dásela!", 5f); Goal("Lleva la melaza a Shanidar (E junto a él)");
    }

    void Feed()
    {
        Destroy(_drop.gameObject); _drop = null; _collapsed = false;
        HoneydewSpell h = Honeydew(); Anima me = PlayerT().GetComponent<Anima>();
        if (h.charges > 0 && me != null) { h.charges--; h.Cast(me, _oldB.GetComponent<ITarget>() ?? _oldB.GetOrAdd<AnimaTarget>()); }   // le repone la energía
        Lay(_oldB, false);
        Say(_oldB, "…Gracias. Ya puedo seguir.", 5f);
        Say(_ambrosio, "¡Se levanta! Vuelve a empujarlo.", 5f); Goal("Empuja a Shanidar hacia la cueva (E junto a él)");
        Go(Beat.Push2); PushedThisPhase = 0f;
    }

    // El hechizo de la maleza del jugador; si se lo añade la historia, sin tecla propia (aquí la E la reparte Act).
    HoneydewSpell Honeydew()
    {
        HoneydewSpell h = PlayerT().GetComponent<HoneydewSpell>();
        if (h == null) { h = PlayerT().gameObject.AddComponent<HoneydewSpell>(); h.castKey = KeyCode.None; }
        return h;
    }

    // ── EL GUION ─────────────────────────────────────────────────────────────────────────────────────────────────
    void Update()
    {
        Transform p = PlayerT();
        if (p == null) return;
        if (Input.GetKeyDown(KeyCode.E)) Debug.Log($"[Ambrosio] (E) {Act()}");
        float dt = Time.deltaTime;
        switch (Now)
        {
            case Beat.Intro:   // la familia empuja; el jugador la ve unos segundos y Ambrosio deja a su viejo y va hacia él
                if (!Arrived(p)) { _beatAt = Time.time; Animate(dt, moveOld: false); break; }
                if (Time.time - _beatAt < 4f) { Animate(dt, moveOld: false); break; }
                Animate(dt, moveOld: false);
                _ambrosioFree = true;
                if (Walk(_ambrosio, p.position, 2.6f, dt))
                {
                    Vector3 gap = _ambrosio.position - p.position; gap.y = 0f; AskedAtDistance = gap.magnitude;
                    Say(_ambrosio, "¡Eh, tú! ¡Ayúdanos, por favor! Salva a mi familia: los viejos ya no pueden andar y la cueva está lejos.", 8f);
                    Go(Beat.AskHelp);
                }
                break;
            case Beat.AskHelp:   // lo lleva junto a Shanidar y le pide que empuje; él vuelve a su viejo
                Animate(dt, moveOld: false);
                Vector3 nextTo = _oldB.position - _dir * 1.6f + Vector3.right * 1.6f;
                if (Walk(_ambrosio, nextTo, 0.4f, dt) && Time.time - _beatAt > 2f)
                {
                    Say(_ambrosio, "Empuja a Shanidar por detrás, así (E). Yo vuelvo con el mío.", 7f);
                    Goal("Empuja a Shanidar hacia la cueva (E junto a él)");
                    _ambrosioFree = false;
                    Go(Beat.Push1); PushedThisPhase = 0f;
                }
                break;
            case Beat.Push1: case Beat.Push2: case Beat.Push3:
                if (_ambrosioFree || !Near(_ambrosio, _pushes[_ambrosio], 2.4f)) { _ambrosioFree = !Walk(_ambrosio, Behind(_pushes[_ambrosio]), 0.3f, dt); }
                bool pushing = _pushTarget != null;
                if (pushing) { PushedThisPhase += dt; KeepBehind(p, _pushTarget, dt); }
                Animate(dt, moveOld: pushing);
                if (PushedThisPhase >= PushPhaseSeconds)
                {
                    if (Now == Beat.Push1) { StopPushing(); _collapsed = true; Lay(_oldB, true); Go(Beat.Collapse);
                        Say(_ambrosio, "¡Shanidar se ha derrumbado: no le queda energía! Deja de empujar, toma mi melaza y dásela.", 8f);
                        Goal("Toma la melaza de Ambrosio (E junto a él) y dásela a Shanidar"); }
                    else if (Now == Beat.Push2) { StopPushing(); SpawnPredator(); Go(Beat.Predator);
                        Say(_ambrosio, "¡Un depredador! Los viejos, quietos. ¡Empújalo con nosotros! Cinco empujones y se irá.", 8f);
                        Goal($"Empuja al depredador (E junto a él): {PredatorPushes}/{PredatorPushesNeeded}"); }
                    else { StopPushing(); Go(Beat.Death); Say(_medea, "¡La cueva! ¡Hemos llegado!", 5f); }
                }
                break;
            case Beat.Collapse:
                Animate(dt, moveOld: false);
                break;
            case Beat.Predator:
                PredatorFight(p, dt);
                break;
            case Beat.Death:   // Ambrosio se derrumba en la entrada (el jugador lo mira)
                Face(p, _ambrosio);
                float k = Mathf.Clamp01((Time.time - _beatAt) / 3f);
                _ambrosio.rotation = Quaternion.Slerp(Quaternion.LookRotation(_dir), Quaternion.LookRotation(_dir) * Quaternion.Euler(0f, 0f, 175f), k);
                if (Time.time - _beatAt > 1.5f && Time.time - _beatAt < 1.5f + dt * 1.5f) Say(_ambrosio, "…Lo hemos… conseguido.", 5f);
                if (Time.time - _beatAt > 5f)
                {
                    Anima am = _ambrosio.GetComponent<Anima>(); if (am != null) am.death = true;   // muere de verdad (su alma, SoulRecord, sigue: renace en la cocina)
                    Debug.Log("[Ambrosio] Ambrosio muere en la entrada de la cueva."); Go(Beat.Sakshi);
                }
                break;
            case Beat.Sakshi:
                Face(p, _sakshi);
                if (_sakshi == null || Walk(_sakshi, p.position, 2.2f, dt))
                {
                    if (Time.time - _beatAt < 30f && _sakshiSaid == false)
                    {
                        _sakshiSaid = true; _beatAt = Time.time;
                        Say(_sakshi, "Ambrosio lo dio todo por su familia. Su alma sigue su camino. Tú vuelve al mesocosmos: allí te esperan.", 8f);
                        Goal("Vuelves al mesocosmos…");
                    }
                    if (_sakshiSaid && Time.time - _beatAt > 6f)
                    {
                        Go(Beat.Done);
                        Progress.MarkStory(gameObject.scene.name);
                        LessonPrompt.Show("", null, 0.1f);
                        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) MobWorldLoader.Instance.ExitMobWorld();
                    }
                }
                break;
        }
    }
    bool _sakshiSaid;

    Vector3 Behind(Transform old) => old.position - _dir * 1.3f;   // detrás de su viejo

    // La cámara del jugador, hacia lo que importa ahora (la muerte de Ambrosio, Sakshi que se acerca).
    static void Face(Transform p, Transform t)
    {
        if (t == null) return;
        PlayerController pc = p.GetComponent<PlayerController>(); if (pc != null) pc.LookAtPoint(t.position + Vector3.up * 1.0f);
    }

    // ¿Ya está el jugador aquí, de pie junto a la familia? (al cargar la escena aún llega por el tanque, o cae).
    bool Arrived(Transform p)
    {
        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsBusy) return false;
        Vector3 d = p.position - _oldB.position;
        return Mathf.Abs(d.y) < 3f && new Vector2(d.x, d.z).magnitude < 25f;
    }

    // Mueve a un personaje hacia un punto; true al llegar.
    bool Walk(Transform t, Vector3 to, float stop, float dt)
    {
        if (t == null) return true;
        Vector3 d = to - t.position; d.y = 0f;
        if (d.magnitude <= stop) return true;
        Vector3 step = Vector3.ClampMagnitude(d, 2.8f * dt);
        t.position = Ground(t.position + step) /* (pies en el suelo) */;
        t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(d), dt * 8f);
        return false;
    }

    // Los que empujan, detrás de su viejo, con el vaivén del empujón; y los viejos avanzan si el jugador empuja.
    void Animate(float dt, bool moveOld)
    {
        float speed = path / (3f * PushPhaseSeconds);   // tres tramos de un minuto hasta la cueva
        foreach (Transform old in new[] { _oldA, _oldB })
        {
            if (old == null || (old == _oldB && _collapsed)) continue;
            if (moveOld && _travel[old] < path)
            {
                float m = Mathf.Min(speed * dt, path - _travel[old]);
                _travel[old] += m;
                old.position = Ground(old.position + _dir * m) /* (pies en el suelo) */;
            }
        }
        float bob = Mathf.Sin(Time.time * Mathf.PI * 2f / 1.2f) * 0.15f;   // un empujón cada 1,2 s
        foreach (var kv in _pushes)
        {
            Transform who = kv.Key, old = kv.Value;
            if (who == null || (who == _ambrosio && _ambrosioFree)) continue;
            if (old == _oldB && _collapsed) { Walk(who, old.position - _dir * 2.2f + Vector3.right * 1.5f, 0.3f, dt); continue; }
            Vector3 side = who == _atlas || who == _momo ? Vector3.right * 0.8f : Vector3.left * 0.3f;
            Vector3 spot = old.position - _dir * (1.3f + 0.5f) + side - _dir * bob;
            Walk(who, spot, 0.05f, dt);
            who.rotation = Quaternion.LookRotation(_dir) * Quaternion.Euler(12f + 10f * Mathf.Abs(bob) / 0.15f, 0f, 0f);   // inclinado hacia delante
        }
    }

    // El jugador, detrás de su viejo, con el gesto de empujar.
    void KeepBehind(Transform p, Transform old, float dt)
    {
        float bob = Mathf.Sin(Time.time * Mathf.PI * 2f / 1.2f);
        Vector3 spot = old.position - _dir * (1.4f + 0.5f) + Vector3.left * 0.6f - _dir * bob * 0.12f;
        spot.y = p.position.y;
        WorkdayDirector.Teleport(p, Vector3.Lerp(p.position, spot, dt * 6f));
        p.rotation = Quaternion.LookRotation(_dir);
        if (Time.time >= _nextBob) { _nextBob = Time.time + 0.6f; Pose(p, true, bob > 0f); }
    }

    // Postura de empujar (dos fotogramas) en el cuerpo de cubos del jugador.
    static void Pose(Transform p, bool push, bool b)
    {
        Anima a = p != null ? p.GetComponent<Anima>() : null;
        if (a == null) return;
        if (!push) { DailyPoses.Stand().ApplyFinal(a); return; }
        DailyPoses.Make(b ? "empuja (estira)" : "empuja (carga)", -1f,
            (BodyPart.Back, new Vector3(b ? 30 : 22, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(b ? -85 : -75, 0, 8)), (BodyPart.ShoulderRight, new Vector3(b ? -85 : -75, 0, -8)),
            (BodyPart.ForearmLeft, new Vector3(b ? -5 : -35, 0, 0)), (BodyPart.ForearmRight, new Vector3(b ? -5 : -35, 0, 0)),
            (BodyPart.ThighLeft, new Vector3(-35, 0, 0)), (BodyPart.ShinLeft, new Vector3(40, 0, 0)), (BodyPart.ThighRight, new Vector3(20, 0, 0))).ApplyFinal(a);
    }

    // Derrumbarse (de lado) o levantarse.
    void Lay(Transform t, bool down)
    {
        if (t == null) return;
        t.rotation = Quaternion.LookRotation(_dir) * (down ? Quaternion.Euler(0f, 0f, 85f) : Quaternion.identity);
        t.position = Ground(t.position) + Vector3.up * (down ? 0.25f : 0f);
    }

    // ── EL DEPREDADOR ───────────────────────────────────────────────────────────────────────────────────────────
    void SpawnPredator()
    {
        Vector3 mid = (_oldA.position + _oldB.position) * 0.5f;
        // Una ARAÑA LOBO (Lycosidae): caza hormigas al acecho en el suelo, sin tela. Cuerpo de cubos de su ficha (CubeInsect),
        // a la escala del microcosmos (la hormiga, del tamaño de una persona; la araña, unas dos veces su largo).
        var go = new GameObject("Depredador_arana_lobo");
        go.transform.SetParent(transform, true);
        CubeInsect.Dress(go, "Spider", new Color(0.32f, 0.25f, 0.18f), 3.2f);
        go.transform.position = Ground(mid + Vector3.right * 14f);
        go.transform.rotation = Quaternion.LookRotation(Vector3.left);
        _predator = go.transform;
        PredatorPushes = 0;
    }

    void PredatorFight(Transform p, float dt)
    {
        if (_predator == null) return;
        Vector3 mid = (_oldA.position + _oldB.position) * 0.5f;
        Vector3 front = mid + Vector3.right * (4.5f + PredatorPushes * 1.2f);   // cada empujón lo aleja
        Walk(_predator, front, 0.2f, dt);
        _predator.rotation = Quaternion.LookRotation(Vector3.left);
        // Los demás (no los viejos) empujan al depredador.
        int i = 0;
        foreach (Transform who in new[] { _ambrosio, _atlas, _medea, _momo })
        {
            if (who == null) continue;
            float bob = Mathf.Sin(Time.time * Mathf.PI * 2f / 1.2f + i) * 0.15f;
            Walk(who, _predator.position + Vector3.left * (2.2f + bob) + Vector3.forward * (-1.5f + i), 0.05f, dt);
            who.rotation = Quaternion.LookRotation(Vector3.right) * Quaternion.Euler(15f, 0f, 0f);
            i++;
        }
        if (PlayerPushingPredator)
        {
            Vector3 spot = _predator.position + Vector3.left * 3.3f + Vector3.forward * 3.2f; spot.y = p.position.y;   // al lado (la cámara no se mete en la araña)
            WorkdayDirector.Teleport(p, Vector3.Lerp(p.position, spot, dt * 6f));
            p.rotation = Quaternion.LookRotation(Vector3.right);
            float ph = Mathf.Sin(Time.time * Mathf.PI * 2f / 2.5f);   // empujón lento, de fuerza
            if (Time.time >= _nextBob) { _nextBob = Time.time + 1.25f; Pose(p, true, ph > 0f); }
        }
        else if (_predatorPushUntil > 0f)   // acaba de terminar un empujón de 10 s
        {
            _predatorPushUntil = 0f; PredatorPushes++;
            Pose(p, false, false);
            Debug.Log($"[Ambrosio] Empujón {PredatorPushes}/{PredatorPushesNeeded} al depredador.");
            Goal($"Empuja al depredador (E junto a él): {PredatorPushes}/{PredatorPushesNeeded}");
            if (PredatorPushes >= PredatorPushesNeeded)
            {
                Say(_ambrosio, "¡Se va! ¡Volvamos con los viejos!", 6f);
                Destroy(_predator.gameObject, 4f);
                _predatorLeaving = _predator; _predator = null;
                Goal("Empuja a Shanidar hasta la cueva (E junto a él)");
                Go(Beat.Push3); PushedThisPhase = 0f; _ambrosioFree = true;
            }
        }
    }
    Transform _predatorLeaving;

    void LateUpdate()
    {
        if (_predatorLeaving != null) _predatorLeaving.position += Vector3.right * 6f * Time.deltaTime;   // huye
    }
}
