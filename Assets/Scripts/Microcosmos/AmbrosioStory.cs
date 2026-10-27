using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA HISTORIA DE AMBROSIO (primer microcosmos del recién llegado; Berón 2026-10-26). El jugador aparece y ve, delante,
/// a Ambrosio, sus hermanas (Medea y Momo) y Atlas EMPUJANDO a los dos viejos (Sakshi, que crió a Ambrosio, y Shanidar)
/// hacia la cueva; Ruth y Héspero van delante. La escena la hace la plantilla de microcosmos (MicrocosmTemplate):
///  1. Ambrosio va hacia él: «salva a mi familia»; lo lleva junto a Shanidar y le pide que empuje; vuelve a su viejo.
///  2. Empujar 1 min → Shanidar se DERRUMBA: tomar la MELAZA de Ambrosio y dársela; se levanta; seguir empujando.
///  3. Otro minuto → llega una MARIQUITA (come pulgones: va a por Ambrosio): los viejos se quedan quietos y los demás van a empujarla; el jugador también:
///     cada empujón es una animación de <see cref="PredatorPushSeconds"/> s; con <see cref="PredatorPushesNeeded"/> se va.
///  4. Todos vuelven a los viejos y empujan 1 min más hasta la entrada de la cueva → Ambrosio se derrumba y MUERE.
///  5. Medea llora a Ambrosio; Sakshi, ya a salvo, se acerca y le dice que vuelva al mesocosmos → sale.
/// Una sola tecla (E) hace lo que toca según dónde está (<see cref="Act"/>); el test de experiencia la llama igual.
/// </summary>
public class AmbrosioStory : MonoBehaviour
{
    public const float PushPhaseSeconds = 60f, PredatorPushSeconds = 10f;
    public const int PredatorPushesNeeded = 5;
    [Tooltip("Cuánto crece el elenco (las hormigas a la escala del jugador).")]
    public float castScale = 2.5f;
    [Tooltip("Distancia de la salida a la cueva (m).")]
    public float path = 90f;   // largo (Berón 2026-10-27): tres minutos empujando, y se nota que avanzan

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

    Transform _ambrosio, _medea, _momo, _atlas, _sakshi, _oldA, _oldB, _oldC, _oldD, _cave, _predator, _player, _ruth, _hespero;
    IEnumerable<Transform> Olds { get { foreach (Transform o in new[] { _oldA, _oldB, _oldC, _oldD }) if (o != null) yield return o; } }
    Vector3 _dir, _side, _caveAt;
    float _t0, _beatAt, _predatorPushUntil, _nextBob;
    Transform _pushTarget, _drop;
    bool _collapsed, _ambrosioFree;
    readonly Dictionary<Transform, Transform> _pushes = new Dictionary<Transform, Transform>();   // quién empuja a quién
    readonly Dictionary<Transform, float> _travel = new Dictionary<Transform, float>();           // metros recorridos
    readonly Dictionary<Transform, float> _pushTime = new Dictionary<Transform, float>();         // segundos empujado

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    Transform Find(string n) { foreach (Transform t in GetComponentsInChildren<Transform>(true)) if (t.name == n) return t; return null; }

    void Start()
    {
        _ambrosio = Find("Ambrosio"); _medea = Find("Medea"); _momo = Find("Momo"); _atlas = Find("Atlas"); _sakshi = Find("Sakshi");
        _ruth = Find("Ruth"); _hespero = Find("Hespero"); _oldC = Find("Anciano_Pintor"); _oldD = Find("Anciana");
        // Los VIEJOS que la familia empuja (la novela: «cuatro que desertaron para quedarse con ellos»): Shanidar, SAKSHI (la que
        // crió a Ambrosio; Berón 2026-10-27), el Anciano Pintor y la Anciana. Si la escena es vieja, solo el Pintor y Shanidar (en las
        // escenas viejas, el Anciano Pintor). Ambrosio empuja a Sakshi; el jugador, a Shanidar.
        _oldA = _sakshi != null ? _sakshi : _oldC; _oldB = Find("Shanidar"); _cave = Find("Cueva");
        if (_oldA == _oldC) _oldC = null;
        if (_ambrosio == null || _oldA == null || _oldB == null || _cave == null) { Debug.LogWarning("[Ambrosio] Falta el elenco de la historia."); enabled = false; return; }
        // El tableau tenía su propio director e impulsos: aquí manda la historia.
        foreach (string n in new[] { "Level1Director", "TribeCohesion" }) foreach (MonoBehaviour mb in GetComponentsInChildren<MonoBehaviour>(true)) if (mb != null && mb.GetType().Name == n) mb.enabled = false;   // (la copia gigante del área deja scripts vacíos)
        // La cueva sigue siendo el REFUGIO (CarryToRefuge): los viejos (WeakOne) cuentan como a salvo al llegar.
        foreach (MonoBehaviour mb in _cave.GetComponents<MonoBehaviour>()) if (mb != null && !(mb is CarryToRefuge) && !(mb is WorldLabel)) mb.enabled = false;
        // El camino: de donde empieza la familia («Inicio_familia», si la escena lo marca) hasta la cueva.
        Transform from = Find("Inicio_familia");
        _dir = Vector3.back;   // las escenas viejas: de la salida a la cueva (−Z)
        if (from != null) { Vector3 d = _cave.position - from.position; d.y = 0f; if (d.sqrMagnitude > 1f) { _dir = d.normalized; path = d.magnitude - 2.2f; } }
        _side = Vector3.Cross(_dir, Vector3.up).normalized;
        _caveAt = Ground(_cave.position - _dir * 2.2f);
        Vector3 start = _caveAt - _dir * path;
        foreach (Transform t in new[] { _ambrosio, _medea, _momo, _atlas, _oldA, _oldB, _oldC, _oldD, _ruth, _hespero }) Take(t);
        Vector3 L(float side, float back) => start + _side * side - _dir * back;
        Place(_oldA, L(-1.6f, 0f)); Place(_oldB, L(1.6f, 0f)); Place(_oldC, L(-4.0f, 0.3f)); Place(_oldD, L(4.0f, 0.3f));
        Place(_ambrosio, L(-1.6f, 1.6f)); Place(_atlas, L(-2.4f, 1.4f));
        Place(_medea, L(4.0f, 1.6f)); Place(_momo, L(2.8f, 2.6f));
        // Ruth y Héspero van delante, abriendo camino hasta la cueva, y allí esperan.
        if (_ruth != null) Place(_ruth, _caveAt + _side * 3f - _dir * 1f);
        if (_hespero != null) Place(_hespero, _caveAt - _side * 3f - _dir * 1f);
        // Ambrosio empuja a Sakshi; Atlas, al Pintor; Medea, a la Anciana; el jugador, a Shanidar. MOMO, «floja» (la novela), va
        // al lado haciendo bromas y empuja poco (Momo, más abajo).
        _pushes[_ambrosio] = _oldA; if (_atlas != null) _pushes[_atlas] = _oldC != null ? _oldC : _oldA;
        if (_medea != null) _pushes[_medea] = _oldD != null ? _oldD : _oldB;
        foreach (Transform o in Olds) _travel[o] = 0f;
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
        if (t.GetComponentInChildren<CubeInsect>(true) != null) return;   // la plantilla ya lo trae vestido (mirmidón)
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

    // Apoya en el suelo el punto más bajo del cuerpo (más <paramref name="lift"/>): tumbado, el origen (los pies) ya no es lo más bajo.
    float _deathLift;
    void RestOnGround(Transform t, float lift)
    {
        if (t == null) return;
        Renderer[] rs = t.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) return;
        Bounds b = rs[0].bounds; foreach (Renderer r in rs) b.Encapsulate(r.bounds);
        t.position += Vector3.up * (Ground(t.position).y + lift - b.min.y);
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
        if (who == null) return;   // (quien no está en esta escena no habla)
        StorySubtitle.Show(n == "Hespero" ? "Héspero" : n.Replace('_', ' '), text, secs);   // en pantalla (PlaneMessenger no existe en el microcosmos: no se veía nada)
        Debug.Log($"[Ambrosio] {n}: «{text}»");
    }

    void Goal(string text) { LessonPrompt.Show(text, null, 9999f); }

    // ── LA TECLA E: lo que toca según dónde está el jugador ─────────────────────────────────────────────────────
    public string Act()
    {
        Transform p = PlayerT(); if (p == null) return "sin jugador";
        if (Now == Beat.Predator && _predator != null)
        {
            if (_predatorPushUntil > 0f && !PlayerPushingPredator) { EndPredatorPush(p); if (_predator == null) return "el depredador se va"; }   // el anterior acabó en este mismo fotograma: cuenta
            if (Near(p, _predator, _predatorReach + 1.8f) && !PlayerPushingPredator) { _predatorPushUntil = Time.time + PredatorPushSeconds; StopPushing(); return "empujas al depredador"; }
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
                Vector3 nextTo = _oldB.position - _dir * 1.6f + _side * 1.6f;
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
                Lines(Now, Time.time - _beatAt);
                if (PushedThisPhase >= PushPhaseSeconds)
                {
                    if (Now == Beat.Push1) { StopPushing(); _collapsed = true; Lay(_oldB, true); Go(Beat.Collapse);
                        Say(_ambrosio, "¡Shanidar se ha derrumbado: no le queda energía! Deja de empujar, toma mi melaza y dásela.", 8f);
                        Goal("Toma la melaza de Ambrosio (E junto a él) y dásela a Shanidar"); }
                    else if (Now == Beat.Push2) { StopPushing(); SpawnPredator(); Go(Beat.Predator);
                        Say(_ambrosio, "¡Una mariquita! Viene a por mí: los pulgones somos su comida. Los viejos, quietos. ¡Empujadla! Cinco empujones y se irá.", 8f);
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
            case Beat.Death:   // Ambrosio se derrumba en la entrada (el jugador lo mira); la escena, como en la novela (docs/novela.md, «La entrada de la cueva»)
            {
                Face(p, _ambrosio);
                float t = Time.time - _beatAt;
                Quaternion down = Quaternion.LookRotation(_dir) * Quaternion.Euler(0f, 0f, 90f), held = Quaternion.LookRotation(_dir) * Quaternion.Euler(0f, 0f, 60f);   // de lado (antes 175°: boca arriba girando sobre los pies, se hundía en el suelo)
                // Cae de lado (3 s); cuando Momo lo levanta, queda medio alzado en sus brazos (sigue tumbado: no se pone en pie).
                _ambrosio.rotation = t < 11f ? Quaternion.Slerp(Quaternion.LookRotation(_dir), down, Mathf.Clamp01(t / 3f)) : Quaternion.Slerp(down, held, Mathf.Clamp01((t - 11f) / 2f));
                RestOnGround(_ambrosio, _deathLift);   // el cuerpo, tumbado, apoya en el suelo: nunca lo atraviesa
                Once("ambrosio_alivio", t > 1.5f, () => StorySubtitle.Show("", "A unos pasos de la entrada, Ambrosio se tumba, aliviado de haber puesto a su familia a salvo.", 7f));   // (la novela no le da últimas palabras)
                // Medea, diminuta, intenta levantarlo sola y no puede: se inclina, tira, vuelve a caer.
                if (_medea != null)
                {
                    Walk(_medea, _ambrosio.position - _side * 1.2f, 0.4f, dt);
                    if (t > 3f && t < 11f) _medea.rotation = Quaternion.LookRotation(_side) * Quaternion.Euler(25f + 15f * Mathf.Abs(Mathf.Sin(t * 2.5f)), 0f, 0f);
                    Once("medea_alzar", t > 3f, () => Say(_medea, "¡Ambrosio, levántate! ¡Ayudadme a alzarlo! ¡Sakshi, Shanidar!", 6f));
                }
                Once("viejos_no_pueden", t > 5f, () => Say(_oldB, "…No podemos, pequeña. Ya no nos quedan fuerzas.", 5f));
                // Atlas, el único adulto, quiere consolarla; ella lo aparta.
                if (_atlas != null)
                {
                    if (t > 6f && t < 9f) Walk(_atlas, _medea != null ? _medea.position - _side * 1.1f : _ambrosio.position, 0.4f, dt);
                    else if (t >= 9f) Walk(_atlas, _ambrosio.position - _side * 3.4f - _dir * 1.5f, 0.3f, dt);   // apartado
                    Once("atlas_consuela", t > 7f, () => Say(_atlas, "Medea… ven, déjalo descansar.", 4f));
                    Once("medea_aparta", t > 8.5f, () => Say(_medea, "¡Déjame! ¡No lo toques!", 4f));
                }
                // Momo se desborda en llanto, lo levanta sin esfuerzo (la más fuerte, en secreto) y le ofrece sus tesoros.
                if (_momo != null)
                {
                    if (t > 9.5f) Walk(_momo, _ambrosio.position + _side * 1.1f, 0.4f, dt);
                    Once("momo_llora", t > 10f, () => Say(_momo, "…¡Ambrosio! ¡No, no, no!", 4f));
                    Once("momo_tesoros", t > 13f, () => Say(_momo, "Toma, mi hongo… mis hojitas, todas. ¡Come, Ambrosio, come!", 6f));
                    if (t > 11f) _deathLift = Mathf.MoveTowards(_deathLift, 0.6f, dt * 0.4f);   // en brazos de Momo, medio alzado
                }
                Once("medea_llanto", t > 16f, () => Say(_medea, "¡No me dejes! ¡Llévame contigo!", 6f));
                // Héspero, delante, con la cara rígida: lo ve caer… y se vuelve, rápido, hacia otro lado.
                if (_hespero != null)
                {
                    Vector3 look = _ambrosio.position - _hespero.position; look.y = 0f;
                    if (look.sqrMagnitude > 0.01f) _hespero.rotation = Quaternion.Slerp(_hespero.rotation, Quaternion.LookRotation(t < 18f ? look : -look), dt * (t < 18f ? 2f : 9f));
                }
                if (t > 21f)
                {
                    Anima am = _ambrosio.GetComponent<Anima>(); if (am != null) am.death = true;   // muere de verdad (su alma, SoulRecord, sigue: renace en la cocina)
                    Debug.Log("[Ambrosio] Ambrosio muere en la entrada de la cueva."); Go(Beat.Sakshi);
                }
                break;
            }
            case Beat.Sakshi:
                Face(p, _sakshi);
                if (_sakshi == null || Walk(_sakshi, p.position, 2.2f, dt))
                {
                    if (Time.time - _beatAt < 30f && _sakshiSaid == false)
                    {
                        _sakshiSaid = true; _beatAt = Time.time;
                        Say(_sakshi, "Me trajo hasta aquí, como yo lo llevé a él cuando nació distinto. Lo dio todo por su familia; su alma sigue su camino. Tú vuelve al mesocosmos: allí te esperan.", 9f);
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

    // Lo que se dicen por el camino (docs/microcosmos-level1.md: Héspero quiere dejar atrás a los débiles, Momo bromea,
    // Ambrosio se agota): cada frase, una vez, a su segundo del tramo.
    readonly HashSet<string> _said = new HashSet<string>();
    void Once(string key, bool when, System.Action say) { if (when && _said.Add(key)) say(); }

    void Lines(Beat b, float t)
    {
        if (b == Beat.Push1)
        {
            Once("shanidar_gracias", t > 3f, () => Say(_oldB, "Gracias, joven. Mis patas ya no me sostienen…", 6f));
            Once("hespero_dejadlos", t > 18f, () => Say(_hespero, "¡Dejadlos atrás! ¡Así no llegaremos nunca a la cueva!", 6f));
            Once("ambrosio_nadie", t > 24f, () => Say(_ambrosio, "¡Nadie se queda atrás, Héspero! Ellos nos criaron.", 6f));
            Once("sakshi_ambrosio", t > 36f, () => Say(_oldA, "Ambrosio, hijo, guarda fuerzas… tu melaza es para todos.", 6f));
            Once("momo_animo", t > 10f, () => Say(_momo, "¡Arriba esos ánimos! Yo empujo con la sonrisa, que también cuenta.", 6f));
            Once("medea_momo", t > 46f, () => Say(_medea, "¡Momo, empuja de verdad!", 4f));
            Once("momo_responde", t > 49f, () => Say(_momo, "¡Si ya empujo! …Con la mirada, que es más fuerte de lo que parece.", 5f));
        }
        else if (b == Beat.Push2)
        {
            Once("momo_broma", t > 10f, () => Say(_momo, "¡Shanidar, si te duermes empujando te cobro el viaje!", 6f));
            Once("ruth_camino", t > 30f, () => Say(_ruth, "¡Por aquí! El camino está despejado hasta la cueva.", 6f));
        }
        else if (b == Beat.Push3)
        {
            Once("ambrosio_cansado", t > 6f, () => Say(_ambrosio, "Me cuesta… pero ya se ve la cueva. Un poco más.", 6f));
            Once("atlas_animo", t > 30f, () => Say(_atlas, "¡Vamos, familia! ¡Ya casi!", 5f));
            Once("medea_cae", t > 40f, () => Say(_medea, "¡Estoy bien! ¡Sigo! ¡Sigo empujando!", 5f));
        }
    }

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
        foreach (Transform old in Olds)
        {
            if (old == null || (old == _oldB && _collapsed)) continue;
            if (moveOld && _travel[old] < path)
            {
                // A EMPUJONES (Berón 2026-10-27: «que los empujes muevan más a las viejas»): cada empujón (1,2 s) es un tirón
                // hacia delante y una pausa; de media, lo justo para llegar en tres tramos (exacto, a cualquier velocidad del juego).
                _pushTime[old] = (_pushTime.TryGetValue(old, out float pt) ? pt : 0f) + dt;
                float strokes = _pushTime[old] / 1.2f, k = Mathf.Floor(strokes);
                float target = Mathf.Min(path, speed * 1.2f * (k + Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((strokes - k) * 1.6f))));
                float m = Mathf.Max(0f, target - _travel[old]);
                _travel[old] += m;
                old.position = Ground(old.position + _dir * m) /* (pies en el suelo) */;
            }
        }
        float bob = Mathf.Sin(Time.time * Mathf.PI * 2f / 1.2f) * 0.3f;   // un empujón cada 1,2 s (se ve el impulso)
        foreach (var kv in _pushes)
        {
            Transform who = kv.Key, old = kv.Value;
            if (who == null || (who == _ambrosio && _ambrosioFree)) continue;
            if (old == _oldB && _collapsed) { Walk(who, old.position - _dir * 2.2f + _side * 1.5f, 0.3f, dt); continue; }
            Vector3 side = who == _atlas || who == _momo ? _side * 0.8f : (-_side) * 0.3f;
            Vector3 spot = old.position - _dir * (1.3f + 0.5f) + side - _dir * bob;
            Walk(who, spot, 0.05f, dt);
            who.rotation = Quaternion.LookRotation(_dir) * Quaternion.Euler(12f + 18f * Mathf.Abs(bob) / 0.3f, 0f, 0f);   // inclinado hacia delante, más al empujar
            // Medea, diminuta, empuja con todo: de vez en cuando se cae de bruces y se vuelve a levantar (docs/novela.md).
            if (who == _medea && Mathf.Repeat(Time.time, 9f) < 1.1f) who.rotation = Quaternion.LookRotation(_dir) * Quaternion.Euler(70f * Mathf.Sin(Mathf.Repeat(Time.time, 9f) / 1.1f * Mathf.PI), 0f, 0f);
        }
        // MOMO, «floja» (docs/novela.md): va al lado de Shanidar animando y haciendo bromas; de vez en cuando (3 s de cada 15)
        // se acuerda de empujar un poco.
        if (_momo != null && !(_collapsed && Now == Beat.Collapse))
        {
            float k = Mathf.Repeat(Time.time, 15f);
            Vector3 spot = k < 3f ? _oldB.position - _dir * 1.8f + _side * 0.9f : _oldB.position + _side * 2.6f - _dir * 0.4f;
            Walk(_momo, spot, 0.1f, dt);
            _momo.rotation = Quaternion.LookRotation(_dir) * Quaternion.Euler(k < 3f ? 18f : 0f, 0f, 0f);
        }
    }

    // El jugador, detrás de su viejo, con el gesto de empujar.
    void KeepBehind(Transform p, Transform old, float dt)
    {
        float bob = Mathf.Sin(Time.time * Mathf.PI * 2f / 1.2f);
        Vector3 spot = old.position - _dir * (1.4f + 0.5f) + (-_side) * 0.6f - _dir * bob * 0.12f;
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
        // Una MARIQUITA (Coccinellidae): la depredadora de los PULGONES (viene a por Ambrosio) y más débil que las hormigas,
        // que en la naturaleza la echan a empujones de sus pulgones (Berón 2026-10-27: la araña lobo era imposible de ahuyentar).
        var go = new GameObject("Depredador_mariquita");
        go.transform.SetParent(transform, true);
        CubeInsect.Dress(go, "Ladybug", new Color(0.8f, 0.12f, 0.1f), 2.4f);   // a la escala de la hormiga (docs/microcosmos-areas.md)
        go.transform.position = Ground(mid + _side * 14f);
        go.transform.rotation = Quaternion.LookRotation((-_side));
        _predator = go.transform;
        PredatorPushes = 0;
        // Hasta dónde llegan sus patas en el suelo (medio ancho de su caja, en horizontal).
        Bounds bb = new Bounds(go.transform.position, Vector3.zero);
        foreach (Renderer rd in go.GetComponentsInChildren<Renderer>()) bb.Encapsulate(rd.bounds);
        _predatorReach = Mathf.Max(2f, Mathf.Max(bb.extents.x, bb.extents.z));
    }

    void PredatorFight(Transform p, float dt)
    {
        if (_predator == null) return;
        Vector3 mid = (_oldA.position + _oldB.position) * 0.5f;
        float r = _predatorReach;   // su caparazón: nadie se mete debajo
        Vector3 front = mid + _side * (r + 2.5f + PredatorPushes * 1.2f);   // cada empujón lo aleja
        Walk(_predator, front, 0.2f, dt);
        _predator.rotation = Quaternion.LookRotation((-_side));
        // Los demás (no los viejos) empujan al depredador, contra la punta de sus patas.
        int i = 0;
        foreach (Transform who in new[] { _ambrosio, _atlas, _medea, _momo })
        {
            if (who == null) continue;
            float bob = Mathf.Sin(Time.time * Mathf.PI * 2f / 1.2f + i) * 0.15f;
            Walk(who, _predator.position + (-_side) * (r + 0.3f + bob) + (-_dir) * (-1.8f + i * 1.2f), 0.05f, dt);
            who.rotation = Quaternion.LookRotation(_side) * Quaternion.Euler(15f, 0f, 0f);
            i++;
        }
        if (PlayerPushingPredator)
        {
            // Al lado de la fila de la familia: se ve la mariquita y a los que empujan con él.
            Vector3 spot = _predator.position + (-_side) * (r + 0.8f) + (-_dir) * (r * 0.6f + 2.2f); spot.y = p.position.y;
            WorkdayDirector.Teleport(p, Vector3.Lerp(p.position, spot, dt * 6f));
            Vector3 look = _predator.position - p.position; look.y = 0f;
            p.rotation = Quaternion.LookRotation(look.sqrMagnitude > 0.01f ? look : _side);
            float ph = Mathf.Sin(Time.time * Mathf.PI * 2f / 2.5f);   // empujón lento, de fuerza
            if (Time.time >= _nextBob) { _nextBob = Time.time + 1.25f; Pose(p, true, ph > 0f); }
        }
        else if (_predatorPushUntil > 0f) EndPredatorPush(p);   // acaba de terminar un empujón de 10 s
    }

    void EndPredatorPush(Transform p)
    {
        _predatorPushUntil = 0f; PredatorPushes++;
        Pose(p, false, false);
        Debug.Log($"[Ambrosio] Empujón {PredatorPushes}/{PredatorPushesNeeded} al depredador.");
        Goal($"Empuja al depredador (E junto a él): {PredatorPushes}/{PredatorPushesNeeded}");
        if (PredatorPushes >= PredatorPushesNeeded)
        {
            Say(_ambrosio, "¡Se va volando! Gracias… ¡Volvamos con los viejos!", 6f);
            Destroy(_predator.gameObject, 4f);
            _predatorLeaving = _predator; _predator = null;
            Goal("Empuja a Shanidar hasta la cueva (E junto a él)");
            Go(Beat.Push3); PushedThisPhase = 0f; _ambrosioFree = true;
        }
    }
    Transform _predatorLeaving;
    float _predatorReach = 2.5f;

    void LateUpdate()
    {
        if (_predatorLeaving != null) _predatorLeaving.position += (_side * 6f + Vector3.up * 3f) * Time.deltaTime;   // se va volando
    }
}
