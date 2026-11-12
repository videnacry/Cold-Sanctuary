using UnityEngine;

/// <summary>
/// EL CICLO DE VIDA DE UNA PERSONA (docs/ciclo-de-vida.md §Humanos; Berón 2026-11-02: «el ciclo de vida de los humanos y de los
/// insectos-humanos es el mismo»). Una sola pieza para un humano del santuario y para un insecto-humano de una era civilizada,
/// sobre los componentes del cuerpo que ya existían:
///   · NECESIDADES: <see cref="BodyMetabolism"/> (hambre, sueño, baño, suciedad) y <see cref="NeedsDriver"/> (las pide); quien
///     tiene <see cref="Household"/> las atiende en su casa (<see cref="HomeNeedSpell"/>);
///   · EDAD: <see cref="BodyAging"/> (crecer comiendo y durmiendo, desgaste, niño → joven → adulto → anciano → muere de viejo);
///   · FAMILIA (esta pieza): CORTEJO de dos adultos de distinto sexo que comparten casa → PAREJA → EMBARAZO (de noche, juntos en
///     casa) → PARTO a los 9 meses → el hijo entra en la familia, sigue a su madre de niño y crece. En el microcosmos el hijo sale
///     con una especie de su árbol genealógico (<see cref="FolkCouple.Birth"/>, <see cref="InsectGenealogy"/>).
/// EL CALENDARIO es el del santuario (<see cref="TimeController.DaysPerYear"/>): crecer dura <see cref="growYears"/> años, la vida
/// <see cref="lifeYears"/>, el embarazo <see cref="gestationMonths"/> meses; en las escenas de ciclo de vida el año se comprime
/// (como en el grupo 58). Cada paso se anuncia en <see cref="OnEvent"/> (lo marca <see cref="CicloHumanoPanel"/>).
/// <see cref="MicroJornada"/> (en el microcosmos) es solo el HORARIO por encima: trabajo, comida, práctica y casa.
/// </summary>
[RequireComponent(typeof(BodyAging))]
public class PersonLife : MonoBehaviour
{
    public bool female = true;
    public PersonLife partner, mother, father;
    public Household home;
    [Range(0f, 1f)] public float courtship;
    public bool pregnant;
    public int children;
    public int maxChildren = 2;
    [Tooltip("Meses del embarazo (en el calendario del santuario).")]
    public float gestationMonths = 9f;
    [Tooltip("Meses mínimos entre un hijo y el siguiente.")]
    public float monthsBetweenChildren = 12f;
    [Tooltip("Horas de juego juntos (cerca, en casa) para que el cortejo acabe en pareja.")]
    public float courtHours = 1.5f;
    [Tooltip("Años para crecer de recién nacido a adulto, y años de vida a carga normal (el desgaste de 0 a 1).")]
    public float growYears = 18f, lifeYears = 80f;
    [Tooltip("Vive libre (sin horario de jornada): sus necesidades se atienden a cualquier hora. Las escenas de ciclo de vida.")]
    public bool freeLife;

    /// <summary>Un paso del ciclo de alguien: comer, dormir, bano, lavarse, cortejo, pareja, embarazo, parto, junto, joven, adulto,
    /// anciano, vejez (muere de viejo) o muerte (otra causa).</summary>
    public static event System.Action<GameObject, string> OnEvent;
    public static void Report(GameObject who, string key)
    {
        if (who == null) return;
        OnEvent?.Invoke(who, key);
    }

    /// <summary>Los días de juego que lleva el reloj (para medir sueños y esperas).</summary>
    public static float NowDays
    {
        get { TimeController tc = TimeController.timeController; return tc != null ? tc.Day + tc.Hour / 24f : Time.time / 3600f; }
    }
    public static float DaysPerYear => TimeController.timeController != null ? TimeController.timeController.DaysPerYear : 28f;
    static float SecondsPerGameDay => TimeController.timeController != null ? TimeController.timeController.SecondsPerGameDay : 3600f;

    /// <summary>La estatura de <paramref name="t"/> ahora (su cápsula a su escala; si no tiene, 1,75 m a su escala): los sitios y
    /// las distancias del hogar y del cortejo se miden con ella, no en metros fijos (la escala del microcosmos puede cambiar).</summary>
    public static float HeightOf(Transform t)
    {
        if (t == null) return 1.75f;
        CapsuleCollider c = t.GetComponent<CapsuleCollider>();
        float h = c != null ? c.height : 1.75f;
        return Mathf.Max(0.2f, h * Mathf.Abs(t.lossyScale.y));
    }

    /// <summary>Tope de nacimientos por escena (sin explosión demográfica).</summary>
    public const int MaxBirthsPerScene = 24;
    /// <summary>«Juntos»: a menos de estas estaturas el uno del otro (lo que mide una casa del microcosmos: ~10 m).</summary>
    public const float Near = 6f;
    static int _births;

    Anima _a; BodyAging _ag; MicroJornada _j;
    char _stage; bool _known, _dead, _elder, _together, _courting;
    float _gestDays, _sinceBirthDays = 1e6f, _next, _accDays, _tickDays;

    public bool Dead => _dead;
    public BodyAging Aging => _ag != null ? _ag : (_ag = GetComponent<BodyAging>());
    public char Stage => Aging != null ? Aging.Stage : LifeStage.adult;
    public bool Adult => Stage == LifeStage.adult && Aging != null && !Aging.IsElder;
    /// <summary>Días de juego que lleva embarazada (0 si no).</summary>
    public float GestationDays => pregnant ? _gestDays : 0f;
    public float GestationTotalDays => gestationMonths / 12f * DaysPerYear;

    void Awake() { _a = GetComponent<Anima>(); _ag = GetComponent<BodyAging>(); _j = GetComponent<MicroJornada>(); }

    void Start()
    {
        if (home != null) home.Join(gameObject);
        ApplyCalendar();
    }

    /// <summary>Crecer y envejecer van con el año del santuario (lo comprimen las escenas de ciclo de vida).</summary>
    public void ApplyCalendar()
    {
        if (Aging == null) return;
        _ag.growDays = Mathf.Max(1e-4f, growYears * DaysPerYear);
        _ag.lifespanDays = Mathf.Max(1e-4f, lifeYears * DaysPerYear);
    }

    void Update()
    {
        float dDays = Time.deltaTime / Mathf.Max(1e-4f, SecondsPerGameDay);
        if (pregnant) _gestDays += dDays;
        _sinceBirthDays += dDays;
        _accDays += dDays;
        if (Time.time < _next) return;
        _next = Time.time + 0.25f;
        _tickDays = _accDays; _accDays = 0f;
        if (_a == null) _a = GetComponent<Anima>();
        if (_dead) return;
        if (_a != null && _a.death) { Die(); return; }
        ApplyCalendar();

        char st = Stage;
        if (!_known) { _stage = st; _known = true; _elder = Aging != null && Aging.IsElder; }
        if (st != _stage)
        {
            _stage = st;
            if (st == LifeStage.teen) { Report(gameObject, "joven"); GrownUp(); }
            else if (st == LifeStage.adult) Report(gameObject, "adulto");
        }
        if (!_elder && Aging != null && Aging.IsElder) { _elder = true; Report(gameObject, "anciano"); Debug.Log($"[Vida] «{name}» llega a la vejez (desgaste {Aging.wear:0.00})."); }

        if (st == LifeStage.child) FollowMother();
        if (female) { Courtship(); Conception(); }
        if (pregnant && _gestDays >= GestationTotalDays) GiveBirth();
    }

    // ── Niñez: con su madre ──
    void FollowMother()
    {
        if (mother == null || mother.Dead) return;
        float h = HeightOf(mother.transform);
        float d = Flat(mother.transform.position - transform.position);
        if (!_together && d <= 2f * h) { _together = true; Report(gameObject, "junto"); }
        if (_j == null) _j = GetComponent<MicroJornada>();
        if (_j != null) { _j.follow = mother.transform; return; }   // en el microcosmos la sigue su jornada
        if (NeedsDriver.Attending(GetComponent<ActionQueue>()) || (_a != null && _a.asleep)) return;
        // (2026-11-11) EN EL SANTUARIO, con PathMover (alfombras y troncos de los edificios-árbol), no con MicroMove (solo NavMesh): siguiendo a su
        // madre a los pisos altos, MicroMove le llevaba «a mano» por las esquinas con la altura del cuerpo y el agente en el piso de abajo, y
        // GroundGuard lo bajaba de golpe («Neo flotaba 8, 14, 64, 86 m … baja al suelo», ×21, rondas 53–54).
        if (d > 1.2f * h) { Vector3 to = mother.transform.position - mother.transform.forward * 0.6f * h; if (ArbolCampos.All.Count > 0) PathMover.Of(transform).StepToward(to, 1.6f * h / 1.75f); else MicroMove.Step(transform, to, 1.6f * h / 1.75f, 0.8f * h); }
    }

    // De joven deja de ir detrás de su madre: en el microcosmos hace la jornada de su madre (su casa, su comedor, su trabajo).
    void GrownUp()
    {
        if (_j == null) _j = GetComponent<MicroJornada>();
        if (_j == null) return;
        _j.follow = null;
        MicroJornada mj = mother != null ? mother.GetComponent<MicroJornada>() : null;
        if (mj != null) { _j.work = mj.work; _j.dine = mj.dine; _j.practice = mj.practice; _j.home = mj.home; _j.role = ""; }
    }

    // ── Cortejo y pareja (lo lleva ella) ──
    void Courtship()
    {
        if (partner != null || !Adult || home == null) return;
        PersonLife best = null; float bd = float.MaxValue;
        foreach (GameObject m in home.members)
        {
            if (m == null || m == gameObject || !m.TryGetComponent(out PersonLife o)) continue;
            if (o.female || o.partner != null || o.Dead || !o.Adult || o == father) continue;
            float d = Flat(o.transform.position - transform.position);
            if (d < bd) { bd = d; best = o; }
        }
        if (best == null) return;
        if (bd > Near * HeightOf(transform)) return;   // juntos (en la misma casa, cerca)
        if (!_courting) { _courting = true; Report(gameObject, "cortejo"); Report(best.gameObject, "cortejo"); Debug.Log($"[Vida] «{name}» y «{best.name}» se cortejan."); }
        float gameHours = _tickDays * 24f;   // lo que ha pasado desde la última vuelta (horas de juego)
        courtship = Mathf.Clamp01(courtship + gameHours / Mathf.Max(0.05f, courtHours));
        best.courtship = courtship;
        if (courtship >= 1f) Pair(this, best);
    }

    /// <summary>Forman PAREJA (comparten casa; en el microcosmos, su linaje se une y ella lleva <see cref="FolkCouple"/>).</summary>
    public static void Pair(PersonLife she, PersonLife he)
    {
        if (she == null || he == null) return;
        she.partner = he; he.partner = she; she.courtship = he.courtship = 1f;
        if (she.home != null && he.home != she.home) she.home.Join(he.gameObject);
        else if (she.home == null && he.home != null) he.home.Join(she.gameObject);
        if (she.TryGetComponent(out FolkLineage a) && he.TryGetComponent(out FolkLineage b))
        {
            a.partner = b; b.partner = a;
            if (she.GetComponent<FolkCouple>() == null) she.gameObject.AddComponent<FolkCouple>();
        }
        Report(she.gameObject, "pareja"); Report(he.gameObject, "pareja");
        Debug.Log($"[Vida] «{she.name}» y «{he.name}» forman pareja.");
    }

    // ── Embarazo y parto (ella) ──
    void Conception()
    {
        if (pregnant || partner == null || partner.Dead || !Adult || !partner.Adult || children >= maxChildren || _births >= MaxBirthsPerScene) return;
        if (_sinceBirthDays < monthsBetweenChildren / 12f * DaysPerYear) return;
        float hour = TimeController.timeController != null ? TimeController.timeController.Hour : 22f;
        bool night = hour >= 21f || hour < 6f;
        bool asleep = _a != null && _a.asleep && partner.TryGetComponent(out Anima pa) && pa.asleep;
        if (!(night || asleep)) return;
        if (Flat(partner.transform.position - transform.position) > Near * HeightOf(transform)) return;   // juntos en casa
        pregnant = true; _gestDays = 0f;
        Report(gameObject, "embarazo");
        Debug.Log($"[Vida] «{name}» se queda embarazada de «{partner.name}» (parto en {GestationTotalDays:0.###} días de juego: {gestationMonths:0} meses).");
    }

    void GiveBirth()
    {
        pregnant = false; _gestDays = 0f; _sinceBirthDays = 0f;
        if (partner == null) return;
        GameObject kid = TryGetComponent(out FolkLineage fl) && partner.TryGetComponent(out FolkLineage pfl)
            ? FolkCouple.Birth(fl, pfl, Random.value)
            : HumanBirth(this, partner);
        if (kid == null) return;
    }

    /// <summary>Un HIJO HUMANO: cuerpo de cubos con la estatura de adulto de sus padres (BodyAging lo encoge a la del recién nacido).</summary>
    public static GameObject HumanBirth(PersonLife mother, PersonLife father)
    {
        bool female = Random.value < 0.5f;
        int k = mother.children + 1;
        string nm = $"{(female ? "Hija" : "Hijo")} de {Short(mother.name)} y {Short(father.name)} ({k})";
        var g = new GameObject(nm);
        g.transform.SetParent(mother.transform.parent, true);
        float h = 0.5f * (HeightOf(mother.transform) + HeightOf(father.transform));
        g.transform.position = mother.transform.position - mother.transform.forward * 0.4f * h;
        var col = g.AddComponent<CapsuleCollider>(); col.height = h; col.radius = 0.2f * h; col.center = Vector3.up * h * 0.5f;
        var ma = mother.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (ma != null) { var ag = g.AddComponent<UnityEngine.AI.NavMeshAgent>(); ag.radius = ma.radius; ag.height = h; ag.speed = ma.speed; ag.angularSpeed = ma.angularSpeed; }
        CubeHumanoid.Dress(g, Color.HSVToRGB((Mathf.Abs(nm.GetHashCode()) % 100) / 100f, 0.4f, 0.8f), new Color(0.86f, 0.7f, 0.58f), h);
        var wl = g.AddComponent<WorldLabel>(); wl.text = nm; wl.height = h + 0.3f;
        Born(g, mother, father, female);
        return g;
    }

    /// <summary>
    /// EL RECIÉN NACIDO entra en la vida (humano o insecto-humano): cuerpo con necesidades y edad 0 (<see cref="Equip"/>), su
    /// madre y su padre, la casa de la familia, y el anuncio «parto».
    /// </summary>
    public static PersonLife Born(GameObject kid, PersonLife mother, PersonLife father, bool female)
    {
        PersonLife pl = Equip(kid, female, newborn: true);
        pl.mother = mother; pl.father = father; pl.freeLife = mother != null && mother.freeLife;
        if (mother != null) { mother.children++; if (mother.home != null) mother.home.Join(kid); }
        if (father != null) father.children++;
        _births++;
        Report(mother != null ? mother.gameObject : kid, "parto");
        Debug.Log($"[Vida] Nace «{kid.name}» de «{(mother != null ? mother.name : "?")}» y «{(father != null ? father.name : "?")}».");
        return pl;
    }

    /// <summary>
    /// EL CUERPO DE UNA PERSONA (el mismo para un humano y un insecto-humano): <see cref="Anima"/>, metabolismo, edad, cola de
    /// acciones, el impulso de las necesidades y esta pieza. Un recién nacido arranca con crecimiento y desgaste 0.
    /// </summary>
    public static PersonLife Equip(GameObject g, bool female, bool newborn = false)
    {
        g.GetOrAdd<Anima>();
        g.GetOrAdd<BodyMetabolism>();
        BodyAging ag = g.GetOrAdd<BodyAging>();
        if (newborn) { ag.growth = 0f; ag.wear = 0f; }
        g.GetOrAdd<ActionQueue>();
        g.GetOrAdd<NeedsDriver>();
        PersonLife pl = g.GetOrAdd<PersonLife>();
        pl.female = female;
        return pl;
    }

    // ── Muerte ──
    void Die()
    {
        _dead = true;
        bool old = Aging != null && Aging.wear >= 1f;
        Report(gameObject, old ? "vejez" : "muerte");
        Debug.Log($"[Vida] «{name}» muere {(old ? "de viejo/a" : "(otra causa)")}.");
        ActionQueue q = GetComponent<ActionQueue>(); if (q != null) q.ClearOwn();
        if (_j == null) _j = GetComponent<MicroJornada>();
        if (_j != null) _j.enabled = false;
        HomeMove.Hold(transform);
        if (_a != null)
        {
            if (GetComponent<CreatureRig>() != null) DailyPoses.Lie().ApplyFinal(_a);
            else { Transform ins = transform.Find("Insecto"); if (ins != null) ins.localRotation = Quaternion.Euler(90f, 0f, 0f); }
        }
        WorldLabel wl = GetComponent<WorldLabel>(); if (wl != null) wl.text = name + " (†)";
        if (home != null) home.Leave(gameObject);
        pregnant = false;
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
    static string Short(string n) { int p = n.IndexOf(' '); return p > 0 ? n.Substring(0, p) : n; }
}
