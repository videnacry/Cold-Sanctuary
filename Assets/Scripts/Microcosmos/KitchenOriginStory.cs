using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CÓMO NACIÓ LA COCINA (docs/novela.md, «Cómo nació la cocina»): la historia jugable del microcosmos de la Cocina, al que el
/// trabajador de turno manda al recién llegado al acabar su iniciación (otoño, la era del fuego, las 19 h). Las reencarnaciones,
/// sin que nadie lo diga: Nasatya (Ambrosio), el Señor del Fuego (Héspero), la Tirana de las Feromonas (Medea), el Chamán
/// (Sakshi) y el Bufón (Momo).
///  1. Intro: anochece; la tribu come frío (migas duras, granos que no se mastican). Solo hay una brasa, y la guarda el Señor
///     del Fuego. Nasatya, enorme y blando, pasa ligero y veloz con un resto sobre la cabeza y pide ayuda.
///  2. Carry: Kushal le ayuda a llevar <see cref="ScrapsNeeded"/> restos AL HOGAR (E junto a un resto lo coge; E junto al fuego
///     lo deja). Nasatya lleva los suyos a la vez. La Tirana manda a las demás por su rastro… y a Nasatya le deja hacer.
///  3. Accident: el Bufón hace malabares y un grano cae en la piedra caliente: chisporrotea, huele dulce. El fuego «mastica».
///  4. Vigil: la VIGILIA DEL FUEGO (docs/microcosmos-eras-and-observation.md §4.1): la llama baja; Kushal la alimenta
///     <see cref="FeedsNeeded"/> veces (E junto al fuego) mientras los restos se tuestan. El Señor del Fuego queda absorto
///     mirando la llama; el Chamán no mira el fuego: lo mira a él.
///  5. Circle: toda la tribu alrededor del fuego: nadie queda fuera del círculo. Nasatya da su trozo a la más pequeña; la
///     Tirana calla. El Chamán: «aquí volveremos siempre» — el lugar será, mucho después, la cocina del santuario.
///  6. Farewell: Nasatya se recuesta de lado (su postura rara) y despide a Kushal → Progress.MarkStory y vuelta al mesocosmos.
/// Una sola tecla (E) hace lo que toca (<see cref="Act"/>). Toda espera tiene tope: si el jugador no llega, Nasatya o el Señor
/// del Fuego terminan la tarea y la historia sigue. Si faltan piezas (prueba fuera de la escena), las crea.
/// </summary>
public class KitchenOriginStory : MonoBehaviour
{
    public const int ScrapsNeeded = 4, FeedsNeeded = 3, NasatyaQuota = 4, ScrapCount = 10;
    public const float CarryLimit = 240f, VigilLimit = 120f, BusyLimit = 15f, TripLimit = 40f;
    public const float PickRadius = 2.8f, HearthRadius = 4.5f, FlameDecay = 0.05f, FeedCooldown = 1.5f, NasatyaSpeed = 5f;
    public enum Beat { Intro, Carry, Accident, Vigil, Circle, Farewell, Done }

    public static KitchenOriginStory Instance { get; private set; }
    /// <summary>Al acabar (con el nombre de la escena), justo antes de pedir la vuelta al mesocosmos.</summary>
    public static event System.Action<string> Finished;

    public Beat Now { get; private set; } = Beat.Intro;
    /// <summary>Los beats por los que ha pasado, en orden (las pruebas miran que no se salte ninguno).</summary>
    public readonly List<Beat> History = new List<Beat>();
    public int ScrapsDelivered { get; private set; }
    public int NasatyaDelivered { get; private set; }
    public int Feeds { get; private set; }
    public float Flame { get; private set; } = 0.6f;
    public bool Carrying => _held != null;
    public bool CanFeed => Time.time >= _feedReady;
    /// <summary>La historia terminó y pidió volver al mesocosmos (MobWorldLoader.ExitMobWorld si está en un microcosmos).</summary>
    public bool ExitRequested { get; private set; }
    /// <summary>Se acabó el tiempo de una tarea y la terminó otro (Nasatya los restos, el Señor del Fuego la llama).</summary>
    public bool TimedOut { get; private set; }
    public Transform Hearth => _hearth;
    public Transform Nasatya => _nasatya;

    Transform _player, _hearth, _stone, _pile, _flame, _nasatya, _tirana, _fireLord, _shaman, _jester, _lair;
    Light _light;
    readonly List<Transform> _scraps = new List<Transform>(), _piled = new List<Transform>(), _ring = new List<Transform>();
    readonly HashSet<Transform> _taken = new HashSet<Transform>();
    readonly HashSet<string> _said = new HashSet<string>();
    readonly List<MonoBehaviour> _paused = new List<MonoBehaviour>();
    Transform _held, _nHeld, _nTarget;
    float _beatAt, _feedReady, _nTripAt;
    Material _cooked;
    int _cookedCount;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    Transform Find(string n) { foreach (Transform t in GetComponentsInChildren<Transform>(true)) if (t.name == n) return t; return null; }

    void Start()
    {
        _hearth = Find("Brasa"); _stone = Find("Piedra_del_hogar");
        _nasatya = Find("Nasatya"); _tirana = Find("La Tirana de las Feromonas"); _fireLord = Find("El Señor del Fuego");
        _shaman = Find("El Chamán"); _jester = Find("El Bufón"); _lair = Find("Guarida");
        EnsurePieces();
        Transform scraps = Find("Restos_del_hogar");
        if (scraps != null) foreach (Transform s in scraps) _scraps.Add(s);

        // Nasatya y la Tirana los lleva la historia (no su rutina de siempre); el Bufón también, para su malabar.
        Pause(_nasatya); Pause(_tirana); Pause(_jester);
        if (_nasatya != null) _nasatya.rotation = Quaternion.Euler(0f, _nasatya.eulerAngles.y, 0f);   // de pie: hoy anda ligero

        // La LLAMA sobre la brasa (crece y mengua con la vigilia) y su luz: a las 19 h es lo único que alumbra.
        var fl = GameObject.CreatePrimitive(PrimitiveType.Cube); fl.name = "Llama"; Destroy(fl.GetComponent<Collider>());
        fl.transform.SetParent(_hearth, false);
        Renderer hr = _hearth.GetComponent<Renderer>();
        Material flameMat = new Material(hr != null && hr.sharedMaterial != null ? hr.sharedMaterial : new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")));
        SetColor(flameMat, new Color(1f, 0.75f, 0.2f));
        fl.GetComponent<Renderer>().sharedMaterial = flameMat;
        _flame = fl.transform;
        _light = new GameObject("Luz_del_hogar").AddComponent<Light>(); _light.transform.SetParent(_hearth, false);
        _light.transform.localPosition = Vector3.up * 2f; _light.type = LightType.Point; _light.color = new Color(1f, 0.6f, 0.25f); _light.range = 18f;
        _cooked = new Material(flameMat); SetColor(_cooked, new Color(0.55f, 0.3f, 0.12f));   // tostado
        UpdateFlame();
        Go(Beat.Intro);
    }

    static void SetColor(Material m, Color c) { m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c); }

    void Pause(Transform t)
    {
        if (t == null) return;
        foreach (MonoBehaviour mb in t.GetComponents<MonoBehaviour>())
            if (mb is MicroLounger || mb is MicroForager || mb is MicroLeader) { if (mb.enabled) _paused.Add(mb); mb.enabled = false; }
    }

    // Sin la escena del microcosmos (una prueba suelta): las piezas mínimas, alrededor de este objeto.
    void EnsurePieces()
    {
        if (_hearth == null) _hearth = Piece("Brasa", new Vector3(-6f, 0.4f, 3f), new Vector3(1.2f, 0.8f, 1.2f), new Color(1f, 0.45f, 0.15f));
        if (_stone == null) _stone = Piece("Piedra_del_hogar", new Vector3(-4.2f, 0.2f, 2f), new Vector3(2f, 0.4f, 1.6f), new Color(0.5f, 0.48f, 0.45f));
        if (_nasatya == null) _nasatya = RuntimeAnt("Nasatya", new Vector3(8f, 0f, 4f), new Color(0.75f, 0.68f, 0.55f), 2.4f, 2f);
        if (_tirana == null) _tirana = RuntimeAnt("La Tirana de las Feromonas", new Vector3(0f, 0f, 5f), new Color(0.45f, 0.12f, 0.12f), 2.3f, 1.25f);
        if (_fireLord == null) _fireLord = RuntimeAnt("El Señor del Fuego", new Vector3(-7.5f, 0f, 3f), new Color(0.3f, 0.2f, 0.18f), 2f, 1.1f);
        if (_shaman == null) _shaman = RuntimeAnt("El Chamán", new Vector3(-5f, 0f, 5.5f), new Color(0.35f, 0.4f, 0.45f), 1.8f, 0.95f);
        if (_jester == null) _jester = RuntimeAnt("El Bufón", new Vector3(4f, 0f, 8f), new Color(0.75f, 0.5f, 0.2f), 1.7f, 0.95f);
        if (Find("Restos_del_hogar") == null)
        {
            Transform parent = new GameObject("Restos_del_hogar").transform; parent.SetParent(transform, false);
            for (int i = 0; i < ScrapCount; i++)
            {
                float a = (i * 36f + 18f) * Mathf.Deg2Rad, r = 12f + (i % 3) * 3f;
                Piece($"Resto_del_hogar_{i + 1}", new Vector3(Mathf.Cos(a) * r, 0.4f, Mathf.Sin(a) * r), Vector3.one * 0.8f, i % 3 == 0 ? new Color(0.85f, 0.3f, 0.25f) : new Color(0.9f, 0.75f, 0.45f)).SetParent(parent, true);
            }
        }
    }

    Transform Piece(string n, Vector3 local, Vector3 size, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = n; Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(transform, false); g.transform.localPosition = local; g.transform.localScale = size;
        Renderer r = g.GetComponent<Renderer>(); Material m = new Material(r.sharedMaterial); SetColor(m, c); r.sharedMaterial = m;
        return g.transform;
    }

    Transform RuntimeAnt(string n, Vector3 local, Color c, float h, float bulk)
    {
        var g = new GameObject(n); g.transform.SetParent(transform, false); g.transform.localPosition = local;
        CubeInsect.DressMyrmidon(g, c, h, bulk);
        WorldLabel wl = g.AddComponent<WorldLabel>(); wl.text = n; wl.height = h + 0.4f;
        return g.transform;
    }

    void Go(Beat b) { Now = b; _beatAt = Time.time; History.Add(b); Debug.Log($"[Cocina-origen] → {b}"); }
    void Once(string k, bool when, System.Action a) { if (when && _said.Add(k)) a(); }
    static void Say(string who, string text, float s = 7f) { StorySubtitle.Show(who, text, s); Debug.Log($"[Cocina-origen] {who}: «{text}»"); }
    // Frase dirigida a Kushal: él contesta (arriba, KushalVoice).
    static void Say(string who, string text, float s, KushalVoice.Ctx reply) { Say(who, text, s); KushalVoice.Reply(reply); }
    static void Goal(string text) => LessonPrompt.Show(text, null, 9999f);

    /// <summary>Se salta la espera del beat actual (las pruebas: los beats guionizados avanzan sin esperar sus segundos).</summary>
    public void SkipWait() { _beatAt -= 1000f; }

    Transform PlayerT()
    {
        if (_player == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); _player = p != null ? p.transform : null; }
        return _player;
    }

    static float Flat(Vector3 a, Vector3 b) { Vector3 d = a - b; d.y = 0f; return d.magnitude; }
    public bool NearHearth(Vector3 p) => _hearth != null && Flat(p, _hearth.position) <= HearthRadius;

    /// <summary>El resto libre más cercano a <paramref name="from"/> (de la historia; los de Nasatya y los ya entregados no).</summary>
    public Transform NextScrap(Vector3 from, float within = float.MaxValue)
    {
        Transform best = null; float bd = within;
        foreach (Transform s in _scraps)
        {
            if (s == null || _taken.Contains(s) || s == _nTarget) continue;
            float d = Flat(s.position, from);
            if (d <= bd) { bd = d; best = s; }
        }
        return best;
    }

    // Un resto de la tribu (MicroFood) que nadie lleva también vale: la cocina se hace con lo que hay.
    Transform NearFood(Vector3 from)
    {
        foreach (MicroFood f in MicroFood.All)
            if (f != null && f.carrier == null && !f.leaf && Flat(f.transform.position, from) <= PickRadius) return f.transform;
        return null;
    }

    void Hold(Transform s, Transform by)
    {
        _taken.Add(s);
        MicroFood mf = s.GetComponent<MicroFood>(); if (mf != null) mf.carrier = transform;   // las recolectoras ya no lo buscan
        s.SetParent(by, true); s.localPosition = Vector3.up * 2.1f; s.localRotation = Quaternion.identity;
    }

    // Al HOGAR: sobre la piedra caliente, junto a la brasa, en un montón.
    void Drop(Transform s)
    {
        if (_pile == null) { _pile = new GameObject("Monton_del_hogar").transform; _pile.SetParent(transform, true); _pile.position = _stone.position; }
        int k = _piled.Count;
        s.SetParent(_pile, true);
        s.position = _stone.position + Vector3.up * (0.4f + (k / 4) * 0.5f) + new Vector3(((k % 4) - 1.5f) * 0.45f, 0f, ((k % 2) - 0.5f) * 0.4f);
        s.localScale = Vector3.one * 0.4f;   // en el montón, partido en trozos
        _piled.Add(s);
    }

    // ── LA TECLA E ─────────────────────────────────────────────────────────────────────────────────────────────
    public string Act()
    {
        Transform p = PlayerT(); if (p == null) return "sin jugador";
        if (Now == Beat.Carry)
        {
            if (_held == null)
            {
                Transform s = NextScrap(p.position, PickRadius) ?? NearFood(p.position);
                if (s == null) return "acércate a un resto (miga, grano, fruta)";
                Hold(s, p); _held = s;
                Goal($"Lleva el resto al hogar del Señor del Fuego (E junto al fuego) · {ScrapsDelivered}/{ScrapsNeeded}");
                return "coges un resto";
            }
            if (!NearHearth(p.position)) return "llévalo al hogar, junto a la brasa";
            Drop(_held); _held = null; ScrapsDelivered++;
            Debug.Log($"[Cocina-origen] Kushal deja un resto en el hogar: {ScrapsDelivered}/{ScrapsNeeded}.");
            if (ScrapsDelivered < ScrapsNeeded) Goal($"Ayuda a Nasatya: lleva restos al hogar (E junto a un resto, E junto al fuego) · {ScrapsDelivered}/{ScrapsNeeded}");
            return "dejas el resto junto al fuego";
        }
        if (Now == Beat.Vigil)
        {
            if (!NearHearth(p.position)) return "acércate al fuego";
            if (!CanFeed) return "la llama aún está comiendo";
            Feeds++; Flame = Mathf.Min(1f, Flame + 0.35f); _feedReady = Time.time + FeedCooldown;
            CookSome();
            Debug.Log($"[Cocina-origen] Kushal alimenta la llama: {Feeds}/{FeedsNeeded}.");
            Goal($"La vigilia: mantén viva la llama (E junto al fuego) · {Mathf.Min(Feeds, FeedsNeeded)}/{FeedsNeeded}");
            return "echas una brizna seca al fuego";
        }
        return "nada que hacer aquí";
    }

    // Cada vez que la llama come, una parte del montón se tuesta.
    void CookSome()
    {
        int want = Mathf.CeilToInt(_piled.Count * Mathf.Min(1f, Feeds / (float)FeedsNeeded));
        for (; _cookedCount < want && _cookedCount < _piled.Count; _cookedCount++)
        {
            Renderer r = _piled[_cookedCount] != null ? _piled[_cookedCount].GetComponent<Renderer>() : null;
            if (r != null) r.sharedMaterial = _cooked;
        }
    }

    void UpdateFlame()
    {
        if (_flame == null) return;
        float f = Mathf.Max(0.05f, Flame) * (0.9f + 0.1f * Mathf.Sin(Time.time * 9f));
        _flame.localScale = new Vector3(0.6f, 0.4f + 2.2f * f, 0.6f);
        _flame.localPosition = Vector3.up * (0.5f + _flame.localScale.y * 0.5f);
        if (_light != null) _light.intensity = 1f + 5f * Flame;
    }

    void Update()
    {
        Transform p = PlayerT();
        if (Input.GetKeyDown(KeyCode.E) && p != null) Debug.Log($"[Cocina-origen] (E) {Act()}");
        UpdateFlame();
        float t = Time.time - _beatAt;
        switch (Now)
        {
            case Beat.Intro:
                if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsBusy && t < BusyLimit) break;   // tras el fundido (con tope)
                Once("narra", t > 1f, () => Say("", "Otoño, al anochecer, en una cocina gigante. La tribu come frío lo que encuentra: migas duras, granos que no se dejan masticar. Solo hay una brasa, y la guarda el Señor del Fuego.", 9f));
                Once("fuego", t > 6f, () => Say("El Señor del Fuego", "Acercaos a la luz. Es para todos: nadie se queda fuera del círculo.", 6f));
                if (t > 8f) NasatyaWork();
                Once("nasatya", t > 11f, () => Say("", "Algo enorme y blando pasa a tu lado, ligero y veloz, con un resto sobre la cabeza.", 6f));
                Once("pide", t > 15f, () => Say("Nasatya", "¿Me ayudas, pequeño? Lo llevo junto al fuego: allí el frío no se lo come.", 7f, KushalVoice.Ctx.Help));
                if (t > 18f) { Go(Beat.Carry); Goal($"Ayuda a Nasatya: lleva restos al hogar (E junto a un resto, E junto al fuego) · 0/{ScrapsNeeded}"); }
                break;

            case Beat.Carry:
                NasatyaWork();
                TiranaWatches(t);
                if (ScrapsDelivered >= ScrapsNeeded || t > CarryLimit)
                {
                    if (ScrapsDelivered < ScrapsNeeded) { TimedOut = true; Say("Nasatya", "Ya está: lo que faltaba lo he traído yo. ¡Gracias por venir!", 6f, KushalVoice.Ctx.Thanks); }
                    if (_held != null) { Drop(_held); _held = null; }
                    if (_nHeld != null) { Drop(_nHeld); _nHeld = null; }
                    Goal(""); LessonPrompt.Show("", null, 0.1f);
                    Go(Beat.Accident);
                }
                break;

            case Beat.Accident:   // el malabar del Bufón: el primer grano tostado
                Step(_jester, _hearth.position + Vector3.right * 3f, 4f, 1.5f);
                Step(_nasatya, _hearth.position + Vector3.forward * 4f, 3f, 1.5f);
                Step(_tirana, _hearth.position + Vector3.back * 5f, 3.5f, 1.5f);
                Once("bufon", t > 1f, () => Say("El Bufón", "¡Mirad! ¡Sé tener tres granos en el aire! …¡Uy!", 5f));
                Once("cae", t > 5f, () =>
                {
                    if (_piled.Count > 0) { Transform g = _piled[_piled.Count - 1]; g.position = _hearth.position + Vector3.up * 0.9f + Vector3.right * 0.3f; Renderer r = g.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = _cooked; }
                    Say("", "Un grano cae junto a la brasa. Chisporrotea. Un olor nuevo, dulce y tostado, recorre la cocina gigante.", 7f);
                });
                Once("prueba", t > 11f, () => Say("El Señor del Fuego", "Está… blando. Y dulce. El fuego lo ha masticado por nosotros.", 6f));
                Once("chaman", t > 16f, () => Say("El Chamán", "Lo que el frío endurece, el fuego lo abre.", 5f));
                Once("tirana", t > 20f, () => Say("La Tirana de las Feromonas", "¡Que nadie toque nada! Esto lo reparto yo.", 5f));
                Once("pide", t > 24f, () => Say("El Señor del Fuego", "Hay que tostarlo todo en la piedra. Pero la brasa es pequeña: si se apaga, se acabó. Ayúdame a mantenerla viva.", 8f, KushalVoice.Ctx.Help));
                if (t > 28f) { Flame = 0.45f; Go(Beat.Vigil); Goal($"La vigilia: mantén viva la llama (E junto al fuego) · 0/{FeedsNeeded}"); }
                break;

            case Beat.Vigil:   // la vigilia del fuego: el guardián, absorto; el Chamán lo mira a él
                Flame = Mathf.Max(0.15f, Flame - FlameDecay * Time.deltaTime);   // nunca se apaga del todo: el Señor del Fuego la sopla
                if (_fireLord != null) { Vector3 d = _hearth.position - _fireLord.position; d.y = 0f; if (d.sqrMagnitude > 0.01f) _fireLord.rotation = Quaternion.Slerp(_fireLord.rotation, Quaternion.LookRotation(d), Time.deltaTime * 3f); }
                if (_shaman != null && _fireLord != null) { Vector3 d = _fireLord.position - _shaman.position; d.y = 0f; if (d.sqrMagnitude > 0.01f) _shaman.rotation = Quaternion.Slerp(_shaman.rotation, Quaternion.LookRotation(d), Time.deltaTime * 3f); }
                Once("absorto", t > 3f, () => Say("", "El Señor del Fuego ya no oye a nadie: mira la llama, absorto, y la alimenta. El Chamán, a su lado, no mira el fuego: lo mira a él.", 9f));
                Once("baja", Flame < 0.3f, () => Say("El Señor del Fuego", "…Se encoge. Dale de comer.", 4f));
                if ((Feeds >= FeedsNeeded && Flame >= 0.5f) || t > VigilLimit)
                {
                    if (Feeds < FeedsNeeded) { TimedOut = true; Flame = 1f; Say("El Señor del Fuego", "Ya la tengo. Mírala: ya no se apaga.", 5f); }
                    for (int i = _cookedCount; i < _piled.Count; i++) { Renderer r = _piled[i] != null ? _piled[i].GetComponent<Renderer>() : null; if (r != null) r.sharedMaterial = _cooked; }
                    Goal(""); LessonPrompt.Show("", null, 0.1f);
                    GatherRing();
                    Go(Beat.Circle);
                }
                break;

            case Beat.Circle:   // nadie queda fuera del círculo
                for (int i = 0; i < _ring.Count; i++)
                {
                    float a = i * (360f / _ring.Count) * Mathf.Deg2Rad;
                    Step(_ring[i], _hearth.position + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 5.5f, 3.5f, 0.6f);
                }
                Once("circulo", t > 2f, () => Say("El Señor del Fuego", "Que nadie se quede fuera del círculo. Ni la más pequeña.", 6f));
                Once("tirana", t > 8f, () => Say("La Tirana de las Feromonas", "Primero yo. Luego quien yo diga…", 4f));
                Once("da", t > 12f, () => Say("", "Nasatya parte su trozo y se lo da a la recolectora más pequeña. La Tirana lo ve… y calla.", 7f));
                Once("blanda", t > 18f, () => Say("La Tirana de las Feromonas", "…Come tú también, Nasatya.", 4f));
                Once("lugar", t > 22f, () => Say("El Chamán", "Aquí, donde el fuego cambia la comida y la comida se reparte, volveremos siempre. Este será nuestro lugar.", 8f));
                Once("nace", t > 30f, () => Say("", "Así nació la cocina: una brasa, una piedra caliente y un círculo donde nadie se queda fuera. Mucho después, en el mismo sitio, el santuario levantaría la suya.", 9f));
                if (t > 39f) { Go(Beat.Farewell); if (_nasatya != null) _nasatya.rotation = Quaternion.Euler(0f, _nasatya.eulerAngles.y, 0f) * Quaternion.Euler(0f, 0f, 70f); }
                break;

            case Beat.Farewell:   // el «tell»: Nasatya se recuesta de lado y mira a Kushal como si lo conociera
                {
                    PlayerController pc = p != null ? p.GetComponent<PlayerController>() : null;
                    if (pc != null && t < 1f && _nasatya != null) pc.LookAtPoint(_nasatya.position + Vector3.up * 1f);
                    Once("tell", t > 1f, () => Say("", "Nasatya se recuesta de lado, en su postura rara, y te mira como si te conociera de antes.", 7f));
                    Once("adios", t > 6f, () => Say("Nasatya", "Gracias, pequeño. Vuelve a tu mundo: aquí el fuego queda en buenas manos.", 6f, KushalVoice.Ctx.Farewell));
                    if (t > 12f) Finish();
                }
                break;
        }
    }

    void Finish()
    {
        Go(Beat.Done);
        foreach (MonoBehaviour mb in _paused) if (mb != null && !(mb is MicroLeader)) mb.enabled = true;   // la vida sigue (la líder, a su guarida)
        string scene = gameObject.scene.name;
        Progress.MarkStory(scene);
        ExitRequested = true;
        Finished?.Invoke(scene);
        Debug.Log($"[Cocina-origen] Terminada ({ScrapsDelivered} restos de Kushal, {NasatyaDelivered} de Nasatya, {Feeds} veces la llama{(TimedOut ? "; con ayuda por tiempo" : "")}): vuelta al mesocosmos.");
        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) MobWorldLoader.Instance.ExitMobWorld();
    }

    static bool Step(Transform t, Vector3 to, float speed, float arrive) => t == null || MicroMove.Step(t, to, speed, arrive);

    // NASATYA lleva restos al hogar, ligero y veloz (cada viaje, con tope). Deja los demás para Kushal.
    void NasatyaWork()
    {
        if (_nasatya == null || _hearth == null) return;
        if (NasatyaDelivered >= NasatyaQuota && _nHeld == null) { Step(_nasatya, _hearth.position + Vector3.forward * 4f, 3f, 1.5f); return; }
        if (_nHeld == null)
        {
            if (_nTarget == null || _taken.Contains(_nTarget))
            {
                _nTarget = null;
                Transform s = NextScrap(_nasatya.position);
                // Le deja a Kushal siempre los que le faltan.
                int free = 0; foreach (Transform x in _scraps) if (x != null && !_taken.Contains(x)) free++;
                if (s == null || free <= ScrapsNeeded - ScrapsDelivered - (_held != null ? 1 : 0)) { Step(_nasatya, _hearth.position + Vector3.forward * 4f, 3f, 1.5f); return; }
                _nTarget = s; _nTripAt = Time.time;
            }
            if (Step(_nasatya, _nTarget.position, NasatyaSpeed, 1.4f)) { Hold(_nTarget, _nasatya); _nHeld = _nTarget; _nTarget = null; _nTripAt = Time.time; }
            else if (Time.time - _nTripAt > TripLimit) _nTarget = null;
            return;
        }
        if (Step(_nasatya, _hearth.position, NasatyaSpeed, HearthRadius - 1f) || Time.time - _nTripAt > TripLimit)
        {
            Drop(_nHeld); _nHeld = null; NasatyaDelivered++; _nTripAt = Time.time;
            Debug.Log($"[Cocina-origen] Nasatya deja un resto en el hogar ({NasatyaDelivered}).");
        }
    }

    // LA TIRANA: manda a las recolectoras por su rastro; a Nasatya lo espía… y le deja hacer.
    void TiranaWatches(float t)
    {
        if (_tirana == null) return;
        if (t > 20f && t < 45f && _nasatya != null) Step(_tirana, _nasatya.position + (_tirana.position - _nasatya.position).normalized * 3f, 3.5f, 1f);
        else if (_lair != null) Step(_tirana, _lair.position + _lair.forward * 4f, 3.5f, 1.5f);
        Once("ordena", t > 22f, () => Say("La Tirana de las Feromonas", "¡Recolectoras! Lo que se recoge va a la guarida, por mi rastro. Nadie come sin mi permiso.", 7f));
        Once("blanda", t > 32f, () => Say("La Tirana de las Feromonas", "…Tú no, Nasatya. Tú llévalo adonde quieras.", 5f));
        Once("chaman", t > 38f, () => Say("El Chamán", "Con él es blanda. Nadie sabe por qué.", 5f));
    }

    // Todo el elenco (y las recolectoras, que dejan de recoger) se sienta alrededor del fuego.
    void GatherRing()
    {
        _ring.Clear();
        foreach (Transform t in new[] { _fireLord, _shaman, _nasatya, _tirana, _jester }) if (t != null) _ring.Add(t);
        foreach (MicroForager mf in GetComponentsInChildren<MicroForager>(true))
        {
            if (_ring.Contains(mf.transform) || _ring.Count >= 18) continue;
            if (mf.enabled) { _paused.Add(mf); mf.enabled = false; }
            _ring.Add(mf.transform);
        }
    }
}
