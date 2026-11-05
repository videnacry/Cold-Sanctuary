using UnityEngine;

/// <summary>
/// VIDA DE LOS INSECTOS DE HIELO (docs/ice-sanctuary-ecology.md §2–§3), sobre animas compuestas reales:
///   · ABEJA DE LAS NIEVES (~20 cm, modificada): VUELA (sin NavMesh) alrededor de su árbol carnívoro; de día pecorea en
///     círculos a 3–10 m y, si hay un CADÁVER cerca del árbol (lo que el árbol atrae y adormece), va a recoger sus
///     nutrientes y los lleva al NIDO de las ramas → el árbol crece un poco (el bucle de nutrientes). De noche descansa en
///     el nido. Si una araña se le acerca, huye hacia arriba.
///   · ARAÑA DE LAS NIEVES (~30 cm): ACECHA pegada al tronco de un árbol con nidos; cuando una abeja pasa a su alcance,
///     se lanza, la muerde (veneno) y vuelve a su puesto.
/// </summary>
public class IceInsectLife : MonoBehaviour
{
    public enum Kind { Bee, Spider }
    public Kind kind = Kind.Bee;
    public float flySpeed = 3f;

    // COLONIA (plantilla de la abeja como anima compuesta: su oficio cambia con la EDAD, como en Apis mellifera).
    public enum Role { Reina, Limpiadora, Nodriza, Procesadora, Guardiana, Pecoreadora, Zangano }
    public Hive hive;
    public bool isQueen;
    bool _queenDeathLogged;
    // REPRODUCCIÓN DE LA COLONIA (docs/ciclo-de-vida.md §Abeja): como en Apis mellifera, la reina joven sale en VUELO NUPCIAL a
    // la zona de congregación de zánganos y se aparea en el aire (en la naturaleza con 10–20; aquí con los que haya); guarda
    // el esperma de por vida y ya no vuelve a salir. Huevo fecundado → obrera; sin fecundar → ZÁNGANO (24 días). El zángano no
    // trabaja: come miel, vuela a la zona de congregación por la tarde y muere al aparearse (cae).
    [Tooltip("(reina) ¿Ya se apareó? Las reinas de una colonia establecida, sí; la reina virgen sale en vuelo nupcial.")]
    public bool mated = true;
    [Tooltip("ZÁNGANO (macho): no trabaja; vuela a la zona de congregación y muere al aparearse.")]
    public bool drone;
    /// <summary>(zángano) murió al aparearse con la reina (muerte natural, no un accidente).</summary>
    public bool diedMating { get; private set; }
    /// <summary>(reina) cópulas en vuelo nupcial; y si, sin aparearse en 3 semanas, pone solo huevos de zángano.</summary>
    public int matings { get; private set; }
    public bool droneLayer { get; private set; }
    /// <summary>Cada cópula en vuelo nupcial: (reina, zángano).</summary>
    public static event System.Action<IceInsectLife, IceInsectLife> Mated;
    int _eggsLaid, _missedFlights; float _flightSince = -1f, _courtUntil; bool _inWindow;
    public float ageDays = -1f;   // -1 = se reparte al empezar (la colonia tiene abejas de todas las edades)
    public Role role { get; private set; }
    int _cell = -1; float _work; Vector3 _source; bool _dancing; float _danceT; float _loadNectar;
    public string Doing { get; private set; } = "";
    public SpiderWeb web;          // (araña) su tela
    public bool Stuck => Time.time < _stuckUntil;
    float _stuckUntil, _freeUntil; // (abeja) pegada en una tela

    Animal _self;
    NestTree _home;
    Transform _nest;
    Vector3 _perch;
    float _angle, _height, _radius;
    Animal _carcass;
    bool _carrying;
    float _strikeCooldown, _nextSearch;
    bool _queenLongevity;
    // NADA SIN TOPE (JornadaTest: abejas «atascadas > 20 s» a 12–19 m, junto a su panal): una celdilla cuyo posadero queda
    // pegado a la rama (OutOfBranches la saca cada fotograma y nunca llega a 8 cm), una fuente de néctar dentro de un tronco…
    // la abeja se quedaba suspendida empujando contra la rama para siempre. Ahora: la celdilla se trabaja desde donde llegue
    // (≤ 40 cm) o se deja; y si en 6 s no se ha movido sin estar trabajando, lo suelta todo y se aparta un poco.
    bool _working;                    // trabajando en su sitio (celdilla, danza, racimo, telaraña, flor): quieta a propósito
    float _cellSince; int _skipCell = -1; float _skipUntil;
    Vector3 _stallPos, _unstickTo, _origin; float _stallAt, _unstickUntil;
    float _strikeAt = -1f;   // (araña) desde cuándo salta tras una abeja

    void Start()
    {
        _self = GetComponent<Animal>();
        if (kind == Kind.Bee) SwarmModels.Dim(gameObject);   // (Berón 2026-11-04: deslumbraban como luces) mate y sin brillo, aunque su prefab sea el de antes, casi blanco
        UnityEngine.AI.NavMeshAgent nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav != null) nav.enabled = false;   // vuela / trepa: no anda por el NavMesh
        if (TryGetComponent(out Rigidbody rb)) { rb.isKinematic = true; rb.useGravity = false; }   // vuela: la gravedad no la arrastra
        _home = Nearest<NestTree>(transform.position, 200f);
        if (_home != null)
        {
            foreach (Transform t in _home.GetComponentsInChildren<Transform>()) if (t.name.StartsWith("Nido_Abejas")) { _nest = t; break; }
            if (_nest != null && hive == null) hive = _nest.GetComponent<Hive>();
            if (kind == Kind.Bee && hive != null && ageDays < 0f)
            {
                // La primera abeja de cada colmena es la REINA; las demás, obreras de edades repartidas.
                bool hasQueen = false;
                foreach (IceInsectLife o in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None)) if (o != this && o.hive == hive && o.isQueen) hasQueen = true;
                isQueen = !hasQueen;
                ageDays = isQueen ? 30f : Random.Range(0f, 30f);
            }
            Bounds b = TreeBounds(_home.transform);
            _radius = Mathf.Clamp(b.extents.x, 4f, 12f);
            if (kind == Kind.Spider)
            {
                // TELA ORBICULAR junto al panal (por donde pasan las abejas) y la araña en su centro. Antes acechaba "en el
                // tronco", pero el punto quedaba DENTRO del tronco (y las abejas y arañas aparecían dentro del árbol).
                Vector3 anchor = _nest != null ? _nest.position : _home.transform.position + Vector3.up * 6f;
                Vector3 outward = anchor - _home.transform.position; outward.y = 0f;
                outward = outward.sqrMagnitude > 0.01f ? outward.normalized : Vector3.forward;
                Vector3 side = Vector3.Cross(Vector3.up, outward) * (Random.value < 0.5f ? 1f : -1f);
                Vector3 c = anchor + outward * 0.8f + side * 1.4f + Vector3.down * 0.9f;
                web = SpiderWeb.Build(c, side, 0.9f, _home.transform);
                _perch = c;
                transform.position = _perch;
            }
            else if (hive != null)   // la abeja empieza en su panal, no a ras de suelo junto al tronco
                transform.position = hive.combCenter + Random.insideUnitSphere * 0.4f + Vector3.down * 0.2f;
        }
        _angle = Random.value * 360f; _height = Random.Range(3f, 9f);
    }

    // El cuerpo de cubos ya está a su tamaño real modificado (20–30 cm); las etapas de vida del insecto (pensadas para la
    // escala del microcosmos) lo dejaban en ~0. Se mantiene a escala 1.
    // Se mueve en LateUpdate, DESPUÉS de la IA genérica del animal (merodeo de LifeStage/voluntad), que también empujaba
    // su transform y la alejaba del panal: el insecto vuela por su cuenta.
    // (2026-11-04, prueba 28: «la abeja de las nieves mide 8,10 m (escala (1.41, 1.00, 1.41))») Una abeja no puede crecer ni colgar de un
    // objeto escalado: si su escala en el mundo se va de su tamaño (1; la reina 1,35), vuelve a él (y se suelta de ese padre); si lleva
    // algo más grande que ella (comida, una pieza), lo suelta. Lo avisa una vez, con el culpable, para encontrar la causa.
    float _nextSizeCheck; bool _sizeLogged;
    void KeepBeeSize()
    {
        float want = isQueen ? 1.35f : 1f;
        Vector3 s = transform.lossyScale;
        string why = null;
        if (Mathf.Abs(s.x - want) > 0.15f || Mathf.Abs(s.y - want) > 0.15f || Mathf.Abs(s.z - want) > 0.15f)
        {
            why = $"escala {s} (padre «{(transform.parent != null ? transform.parent.name : "ninguno")}»)";
            if (transform.parent != null && (transform.parent.lossyScale - Vector3.one).sqrMagnitude > 1e-4f) transform.SetParent(null, true);
            transform.localScale = Vector3.one * want;
        }
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (r == null || !r.enabled || Mathf.Max(r.bounds.size.x, r.bounds.size.z) < 1.5f) continue;
            why = (why != null ? why + "; " : "") + $"lleva «{r.name}» de {Mathf.Max(r.bounds.size.x, r.bounds.size.z):0.0} m";
            if (_self is ICarrier c && c.CarriedFood != null && r.transform.IsChildOf(c.CarriedFood.transform)) c.Drop(transform.position);
        }
        if (why != null && !_sizeLogged) { _sizeLogged = true; Debug.LogWarning($"[Abejas] «{name}» no tenía su tamaño: {why}. Vuelve a ~20 cm."); }
    }

    void LateUpdate()
    {
        // (la ARAÑITA recién salida del saco sí es pequeña —~5 % de su madre— y crece con sus mudas: LifeStage)
        if (kind == Kind.Spider && _self != null && _self.lifeStage != LifeStage.adult) { if (transform.localScale.x < 0.05f) transform.localScale = Vector3.one * 0.05f; }
        else if (transform.localScale.x < 0.6f) transform.localScale = Vector3.one;
        if (kind == Kind.Bee && Time.time >= _nextSizeCheck) { _nextSizeCheck = Time.time + 2f; KeepBeeSize(); }
        if (_self != null && _self.death)
        {
            if (isQueen && !_queenDeathLogged) { _queenDeathLogged = true; Debug.LogWarning($"[Colmena] Ha muerto la reina de «{(hive != null ? hive.name : "?")}» a {(hive != null ? Vector3.Distance(transform.position, hive.combCenter) : -1f):0.0} m del panal: {Doing}, edad {ageDays:0} días, hambre {_self.hungry:0.00}."); }
            FallDead(); return;
        }
        // La REINA vive años (2–5 en la abeja melífera [A]); las obreras de invierno, meses (StageProfile «SnowBee»).
        if (isQueen && !_queenLongevity && _self != null && _self.AdultStage != null) { _self.AdultStage.stageDays = 3 * 365; _queenLongevity = true; }
        if (kind == Kind.Bee)
        {
            _working = false;
            if (Time.time < _unstickUntil) { Doing = "se aparta"; FlyTo(_unstickTo, flySpeed); }
            else BeeLife();
            // DENTRO DE SU PANAL trabaja (2026-11-02): la cavidad mide ~1 × 1 m y las celdillas están a palmos; la reina que
            // va de celdilla en celdilla o la procesadora que ronda el panal recorren menos de 50 cm en 20 s a propósito, y la
            // batería las contaba como «atascadas > 20 s» (SnowBee a 0,3–0,5 m del centro del panal, comprobado en vivo).
            if (!_working && hive != null && hive.Inside(transform.position)) _working = true;
            KeepBodyOutOfBranches();
            Unstick();
            if (_self != null) _self.busy = _working;   // trabajando: ocupada (como el animal que come); la IA genérica no la mueve
        }
        else SpiderLife();
    }

    // MUERTA CAE (2026-11-02): la abeja que caza la araña junto al panal se quedaba suspendida en el aire para siempre, a 16–25 m
    // (la batería la contaba como «atascada > 20 s»: las 6 SnowBee del grupo 50 eran cadáveres). Una vez, cae a lo que tenga
    // debajo (una rama, el hielo); el zángano que muere al aparearse ya caía así.
    bool _fell;
    void FallDead()
    {
        if (_fell) return;
        _fell = true;
        float best = float.MaxValue; Vector3 at = transform.position;
        foreach (RaycastHit h in Physics.RaycastAll(transform.position + Vector3.up * 0.05f, Vector3.down, 120f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(transform) && h.collider.GetComponentInParent<IceInsectLife>() == null && h.distance < best) { best = h.distance; at = h.point; }
        if (best < float.MaxValue) transform.position = at + Vector3.up * 0.05f;
    }

    // Si en 6 s no se ha alejado 30 cm de donde estaba y no está trabajando en su sitio, suelta lo que hacía y se aparta 1–2 m.
    // (2026-11-01) La medida es desde un ANCLA fija, no desde el último fotograma: empujando contra una rama, FlyTo la mete y
    // OutOfBranches la saca cada fotograma (a ×6, pasos de 10–40 cm), y con «10 cm desde la posición anterior» ese vaivén
    // reiniciaba el reloj sin parar: la abeja quedaba suspendida junto al panal sin soltarse nunca (JornadaTest, grupo 50:
    // SnowBee #15/#20/#31/#39/#43/#47 «atascado > 20 s» a 11–14 m en (−112…−127, −42…−50) y (88, 10, 155)). Un rodeo de verdad
    // (Hover a 0,3 m del centro del panal, la guardiana en la piquera) se aleja más de 30 cm y no cuenta.
    void Unstick()
    {
        if (_working || Time.time < _unstickUntil) { _stallPos = transform.position; _stallAt = Time.time; return; }
        if ((transform.position - _stallPos).sqrMagnitude > 0.09f) { _stallPos = transform.position; _stallAt = Time.time; return; }
        if (Time.time - _stallAt < 6f) return;
        if (_cell >= 0) { _skipCell = _cell; _skipUntil = Time.time + 20f; }
        _cell = -1; _source = Vector3.zero; _dancing = false; _carcass = null; _work = 0f;
        Vector3 away = Random.onUnitSphere; away.y = Mathf.Abs(away.y) * 0.5f;
        if (hive != null) { Vector3 o = transform.position - hive.combCenter; o.y = 0f; if (o.sqrMagnitude > 0.01f) away = (o.normalized + away * 0.5f).normalized; }
        _unstickTo = transform.position + away * Random.Range(1f, 2f);
        _unstickUntil = Time.time + 2.5f;
        _stallPos = transform.position; _stallAt = Time.time;
    }

    // EL CUERPO ENTERO fuera de las ramas (JornadaTest 2026-10-29: abejas «dentro de Rama / Rama_Plataforma»). FlyTo solo
    // sacaba el PIVOTE, y no en todos los caminos (el racimo, la danza, el arranque en el panal y la locomoción del animal la
    // mueven sin FlyTo); el cuerpo de cubos mide 20–30 cm y su centro queda por encima del pivote. Al final de cada fotograma
    // se mira el CENTRO del cuerpo con un margen de medio cuerpo y, si está dentro de un bloque del árbol, sale por la cara
    // más cercana.
    Renderer[] _rends;
    void KeepBodyOutOfBranches()
    {
        if (_rends == null) _rends = GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds(transform.position, Vector3.zero); bool any = false;
        foreach (Renderer r in _rends) { if (r == null || !r.enabled || r is ParticleSystemRenderer || r is LineRenderer) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        float clear = Mathf.Clamp(Mathf.Min(b.extents.x, b.extents.y, b.extents.z) * 0.5f, 0.02f, 0.12f) + 0.03f;
        Vector3 c = b.center, moved = OutOfBranches(c, clear, clear + 0.01f);
        if ((moved - c).sqrMagnitude > 1e-8f) transform.position += moved - c;
    }


    // ── Abeja ───────────────────────────────────────────────────────────────────────────────────

    void BeeLife()
    {
        if (Time.time < _stuckUntil) { _working = true; Doing = "pegada en una telaraña"; transform.position += Random.insideUnitSphere * 0.004f; return; }   // se agita
        if (Time.time > _freeUntil) foreach (SpiderWeb w in SpiderWeb.All)
            if (w.Catches(transform.position)) { _stuckUntil = Time.time + 5f; _freeUntil = _stuckUntil + 4f; Debug.Log("[Hielo] Una abeja de las nieves queda pegada en una telaraña."); return; }
        if (hive != null) { ColonyLife(); return; }
        if (_home == null)   // sin árbol cerca (abeja suelta de otra escena): pecorea en corro donde está, no se queda suspendida
        {
            if (_origin == Vector3.zero) _origin = transform.position;
            _angle += 20f * Time.deltaTime;
            FlyTo(_origin + Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * 2.5f + Vector3.up * Mathf.Sin(Time.time * 0.6f) * 0.8f, flySpeed * 0.5f);
            return;
        }
        SnowSpiderBehavior spider = Nearest<SnowSpiderBehavior>(transform.position, 1.5f);
        if (spider != null) { FlyTo(transform.position + (transform.position - spider.transform.position).normalized * 3f + Vector3.up * 2f, flySpeed * 1.8f); return; }

        bool day = TimeController.timeController == null || TimeController.timeController.IsDay;
        if (!day) { _working = true; if (_nest != null) FlyTo(_nest.position + Vector3.up * 0.3f, flySpeed); return; }   // de noche, al nido

        if (_carrying)
        {
            Vector3 n = _nest != null ? _nest.position : _home.transform.position + Vector3.up * 8f;
            if (FlyTo(n, flySpeed) < 0.5f)
            {
                _carrying = false;
                { IceCarnivorousTree grows = _home.GetComponent<IceCarnivorousTree>(); if (grows != null && grows.growthPerKill > 0f) _home.transform.localScale = Vector3.Min(_home.transform.localScale * 1.002f, Vector3.one * grows.maxScale); }   // los nutrientes del muerto acaban en el árbol (bucle de nutrientes; los de la residencia no crecen: sostienen casas)
                Debug.Log($"[Hielo] Una abeja de las nieves deja nutrientes en su nido: el árbol «{_home.name}» crece.");
            }
            return;
        }
        if ((_carcass == null || !_carcass.death) && Time.time >= _nextSearch) { _nextSearch = Time.time + 2f; _carcass = NearestCarcass(); }
        if (_carcass != null)
        {
            if (FlyTo(_carcass.transform.position + Vector3.up * 0.5f, flySpeed) < 0.6f)
            {
                _carrying = true;
                Debug.Log($"[Hielo] Una abeja de las nieves recoge nutrientes del cadáver de «{_carcass.name}».");
                _carcass = null;
            }
            return;
        }
        // Pecoreo: vuelta lenta alrededor de la copa, subiendo y bajando.
        _angle += 25f * Time.deltaTime;
        Vector3 c = _home.transform.position;
        Vector3 target = c + Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * _radius + Vector3.up * (_height + Mathf.Sin(Time.time * 0.7f) * 1.5f);
        FlyTo(target, flySpeed * 0.7f);
    }


    // ── Colonia: el panal trabajado como en la vida real ──────────────────────────────────────────
    // Oficio por EDAD (polietismo): 0–3 días limpiadora · 3–10 nodriza (alimenta larvas con miel y polen) · 10–18
    // procesadora (recibe el néctar, lo abanica hasta hacerlo miel y la opercula; también fabrica cera) · 18–21 guardiana
    // de la piquera · 21+ pecoreadora (sale a por néctar y polen, vuelve, BAILA la danza del contoneo sobre el panal —el
    // ángulo dice la dirección de la fuente respecto al sol— y descarga el néctar en una celdilla de arriba).
    // La REINA recorre el panal poniendo un huevo en cada celdilla limpia. De noche o con frío todas forman el RACIMO
    // sobre el panal y tiritan para mantener la cría a ~35 °C (gastan miel).
    void ColonyLife()
    {
        float daysPerSec = 1f / Mathf.Max(0.1f, hive.DaySeconds);
        ageDays += Time.deltaTime * daysPerSec;
        role = isQueen ? Role.Reina : drone ? Role.Zangano : ageDays < 3f ? Role.Limpiadora : ageDays < 10f ? Role.Nodriza : ageDays < 18f ? Role.Procesadora : ageDays < 21f ? Role.Guardiana : Role.Pecoreadora;
        if (role == Role.Procesadora || role == Role.Guardiana || role == Role.Pecoreadora) { if (Reverts()) role = Role.Nodriza; }
        if (isQueen && transform.localScale.x < 1.3f) transform.localScale = Vector3.one * 1.35f;   // la reina, más larga
        // COMEN MIEL de su colmena (las nodrizas alimentan a la reina con jalea): sin esto siempre tenían hambre.
        if (_self != null && _self.hungry >= 0f && hive.honeyStock > 0.5f && Vector3.Distance(transform.position, hive.combCenter) < 1.5f) { hive.Eat(0.5f); _self.hungry = -0.6f; }

        SnowSpiderBehavior spider = Nearest<SnowSpiderBehavior>(transform.position, 1.5f);
        if (spider != null && role != Role.Guardiana) { Doing = "huye de la araña"; FlyTo(transform.position + (transform.position - spider.transform.position).normalized * 3f + Vector3.up * 2f, flySpeed * 1.8f); return; }

        bool day = TimeController.timeController == null || TimeController.timeController.IsDay;
        Doing = role.ToString();
        if (day && ageDays >= 6f && NuptialFlight()) return;   // la reina virgen (≥ 6 días) sale a aparearse en las horas cálidas
        if (!day)   // RACIMO de invierno/noche sobre el panal: tiritan y comen miel
        {
            Vector3 spot = hive.combCenter + Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * 0.25f;
            FlyTo(spot, flySpeed); _working = true;
            hive.Eat(0.02f * Time.deltaTime);
            return;
        }

        switch (role)
        {
            case Role.Reina:
                if (!mated)
                {
                    // REINA VIRGEN: no pone (salvo que, sin aparearse en 3 semanas, se vuelva «zanganera»).
                    if (!droneLayer) { if (_cell < 0) Hover(0.3f); break; }
                }
                // Huevo fecundado (obrera) salvo uno de cada 8 sin fecundar (zángano: la colonia cría algunos), o todos si es zanganera.
                if (Work(Hive.Cell.Empty, false, 1.5f)) { hive.LayEgg(_cell, droneLayer || ++_eggsLaid % 8 == 0); _cell = -1; }
                else if (_cell < 0) Hover(0.3f);   // sin celdilla libre recorre el panal (antes se quedaba clavada todo el día)
                break;
            case Role.Limpiadora:
                if (Work(Hive.Cell.Dirty, false, 2f)) { hive.Set(_cell, Hive.Cell.Empty); _cell = -1; }
                else if (_cell < 0) Hover(0.35f);
                break;
            case Role.Nodriza:
                if (Work(Hive.Cell.Larva, false, 1.2f))
                {
                    hive.Eat(0.5f);   // jalea / pan de abeja para la larva
                    if (hive.Since(_cell) >= 6f) hive.Set(_cell, Hive.Cell.CappedBrood);   // a los 6 días de larva, opercula la celdilla
                    _cell = -1;
                }
                else if (_cell < 0) Hover(0.35f);
                break;
            case Role.Procesadora:
                if (Work(Hive.Cell.Nectar, true, 4f)) { hive.Set(_cell, Hive.Cell.Honey); hive.AddHoney(2f); _cell = -1; Debug.Log("[Colmena] el néctar, abanicado y deshidratado, ya es miel: celdilla operculada."); }
                else if (_cell < 0) Hover(0.35f);
                break;
            case Role.Guardiana:
                Vector3 gate = hive.entrance != null ? hive.entrance.position + Vector3.down * 0.15f : hive.combCenter + Vector3.down * 0.5f;
                if (spider != null) FlyTo(spider.transform.position, flySpeed * 1.5f);   // planta cara a la araña
                else FlyTo(gate + new Vector3(Mathf.Sin(Time.time * 2f + _angle) * 0.25f, 0f, Mathf.Cos(Time.time * 2f + _angle) * 0.25f), flySpeed * 0.5f);
                break;
            case Role.Zangano: DroneLife(); break;
            default: Forage(); break;
        }
    }

    // ── Reproducción de la colonia: vuelo nupcial ───────────────────────────────────────────────

    /// <summary>ZONA DE CONGREGACIÓN de zánganos de esta colmena: en el aire, lejos del panal (en la naturaleza a cientos de
    /// metros y 10–40 m de altura; aquí, sobre el mar de su isla).</summary>
    Vector3 CongregationArea()
    {
        float a = (Mathf.Abs(hive.GetHashCode()) % 360) * Mathf.Deg2Rad;
        Vector3 c = _home != null ? _home.transform.position : hive.transform.position;
        return new Vector3(c.x + Mathf.Cos(a) * 14f, c.y + 12f, c.z + Mathf.Sin(a) * 14f);
    }

    /// <summary>Hora de vuelos nupciales: las horas cálidas de la tarde (los zánganos vuelan sobre todo de 13 a 16 h [A]).</summary>
    static bool FlightHours() { TimeController tc = TimeController.timeController; return tc == null || (tc.Hour >= 11f && tc.Hour < 17f); }

    /// <summary>(reina virgen) ¿Está ahora en su vuelo nupcial?</summary>
    public bool OnNuptialFlight => isQueen && !mated && !droneLayer && FlightHours() && _flightSince >= 0f;

    // La reina virgen sale a la zona de congregación; un zángano que la alcanza se aparea con ella en el aire y muere.
    // Con al menos una cópula y un rato de vuelo vuelve al panal fecundada (en la naturaleza: 1–3 vuelos y 10–20 zánganos).
    bool NuptialFlight()
    {
        if (!isQueen || mated || droneLayer) return false;
        if (!FlightHours())
        {
            // Acaba la tarde: si se apareó, vuelve fecundada; si no, un día de vuelos perdido. Sin aparearse en ~3 semanas (dos
            // días de juego: 26 días de vida con el año de 28) se vuelve ZANGANERA: pone solo huevos sin fecundar.
            if (_inWindow)
            {
                _inWindow = false; _flightSince = -1f;
                if (matings > 0) mated = true;
                else if (++_missedFlights >= 2) { droneLayer = true; Debug.Log($"[Colmena] La reina de «{hive.name}» no se ha apareado en 3 semanas: solo pondrá zánganos."); }
            }
            return false;
        }
        _inWindow = true;
        if (_flightSince < 0f) { _flightSince = Time.time; Debug.Log($"[Colmena] La reina virgen de «{hive.name}» sale en VUELO NUPCIAL."); }
        Doing = "vuelo nupcial";
        Vector3 dca = CongregationArea();
        _angle += 50f * Time.deltaTime;
        FlyTo(dca + Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * 2f, flySpeed);
        if (matings > 0 && Time.time - _flightSince > 15f)
        {
            mated = true; _flightSince = -1f;
            Debug.Log($"[Colmena] La reina de «{hive.name}» vuelve fecundada ({matings} zángano{(matings > 1 ? "s" : "")}): empieza a poner.");
        }
        return true;
    }

    // El ZÁNGANO: de día, en las horas de vuelo, a la zona de congregación (persigue a la reina virgen si la hay); si no, en
    // el panal (las obreras lo alimentan: come miel, arriba).
    void DroneLife()
    {
        if (!FlightHours()) { Doing = "zángano en el panal"; Hover(0.3f); return; }
        IceInsectLife queen = null;
        foreach (IceInsectLife q in Cached<IceInsectLife>()) if (q != null && q.OnNuptialFlight && q.hive == hive) { queen = q; break; }
        if (queen != null && Vector3.Distance(queen.transform.position, transform.position) < 12f)
        {
            Doing = "persigue a la reina";
            if (FlyTo(queen.transform.position, flySpeed * 1.4f) < 0.5f) MateWith(queen);
            return;
        }
        Doing = "zona de congregación";
        _angle -= 45f * Time.deltaTime;
        FlyTo(CongregationArea() + Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * 3f + Vector3.up * Mathf.Sin(Time.time) * 0.5f, flySpeed);
    }

    void MateWith(IceInsectLife queen)
    {
        queen.matings++;
        diedMating = true;
        Debug.Log($"[Colmena] Un zángano se aparea con la reina de «{queen.hive.name}» en pleno vuelo y muere (cae).");
        Mated?.Invoke(queen, this);
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit h, 80f, ~0, QueryTriggerInteraction.Ignore)) transform.position = h.point + Vector3.up * 0.05f;
        if (_self != null) _self.death = true;
    }

    // Va a una celdilla en ese estado y trabaja ahí <paramref name="seconds"/> s; true al terminar.
    bool Work(Hive.Cell need, bool top, float seconds)
    {
        if (_cell < 0 || _cell >= hive.state.Count || hive.state[_cell] != need)
        {
            _cell = hive.Find(need, transform.position, top); _work = 0f; _cellSince = Time.time;
            if (_cell >= 0 && _cell == _skipCell && Time.time < _skipUntil) _cell = -1;   // esa celdilla no se alcanza: otra vez será
        }
        if (_cell < 0) return false;
        Vector3 perch = hive.Perch(_cell);
        float dd = FlyTo(perch, Vector3.Distance(transform.position, perch) > 1.5f ? flySpeed * 1.5f : flySpeed * 0.6f); Doing = $"{role} → celdilla {_cell} a {dd:0.0} m";
        // El posadero pegado a una rama no se alcanza (la abeja no se mete en la rama): a 4 s, la trabaja desde donde está si
        // está al lado (≤ 40 cm); si no, la deja para otra.
        if (dd > 0.08f && Time.time - _cellSince > 4f)
        {
            if (dd > 0.4f) { _skipCell = _cell; _skipUntil = Time.time + 20f; _cell = -1; return false; }
        }
        else if (dd > 0.08f) return false;
        _working = true;
        _work += Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(hive.cells[_cell].position - transform.position);
        return _work >= seconds;
    }

    void Hover(float r)
    {
        _angle += 40f * Time.deltaTime;
        FlyTo(hive.combCenter + Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * r, flySpeed * 0.4f);
    }

    void Forage()
    {
        if (_dancing)   // DANZA DEL CONTONEO sobre el panal: recto contoneándose (ángulo = dirección de la fuente) y vuelta
        {
            _danceT += Time.deltaTime; _working = true;
            Vector3 dir = _source - hive.transform.position; dir.y = 0f; dir = dir.sqrMagnitude > 0.01f ? dir.normalized : Vector3.forward;
            float phase = _danceT % 2f;
            Vector3 c = hive.combCenter;
            Vector3 p = phase < 1f ? c + dir * (phase - 0.5f) * 0.3f + Vector3.Cross(Vector3.up, dir) * Mathf.Sin(_danceT * 40f) * 0.03f
                                   : c + Quaternion.Euler(0f, 180f * (phase - 1f) * ((int)(_danceT / 2f) % 2 == 0 ? 1f : -1f), 0f) * dir * 0.15f;
            transform.position = Vector3.Lerp(transform.position, p, 10f * Time.deltaTime);
            if (_danceT >= 6f)
            {
                _dancing = false;
                Debug.Log($"[Colmena] una pecoreadora baila la danza del contoneo: fuente a {Vector3.Distance(_source, hive.transform.position):0} m hacia {(dir.x >= 0 ? "el este" : "el oeste")}.");
            }
            return;
        }
        if (_loadNectar > 0f)   // de vuelta: descarga el néctar en una celdilla de arriba
        {
            if (Work(Hive.Cell.Empty, true, 1f)) { hive.Set(_cell, Hive.Cell.Nectar); _cell = -1; _loadNectar = 0f; }
            else if (_cell < 0) { _loadNectar = 0f; }   // panal lleno: se lo dan a una procesadora (aquí, se pierde)
            return;
        }
        if (_source == Vector3.zero || Random.value < 0.0005f)
        {
            // Una fuente: el cadáver más cercano al árbol (nutrientes) o flores al pie de otros árboles (sotobosque).
            if ((_carcass == null || !_carcass.death) && Time.time >= _nextSearch) { _nextSearch = Time.time + 2f; _carcass = NearestCarcass(); }
            Vector2 r = Random.insideUnitCircle.normalized * Random.Range(6f, 18f);
            _source = _carcass != null ? _carcass.transform.position : _home.transform.position + new Vector3(r.x, 0f, r.y);
        }
        if (FlyTo(_source + Vector3.up * 0.4f, flySpeed) < 0.5f)
        {
            _work += Time.deltaTime; _working = true;
            if (_work > 2.5f) { _work = 0f; _loadNectar = 1f; _dancing = true; _danceT = 0f; _cell = -1; }
        }
    }
    Animal NearestCarcass()
    {
        Animal best = null; float bd = 30f;
        foreach (Animal a in Cached<Animal>())
        {
            if (a == null || !a.death || a == _self) continue;
            float d = Vector3.Distance(a.transform.position, _home.transform.position);
            if (d < bd) { bd = d; best = a; }
        }
        return best;
    }

    // ── Araña ───────────────────────────────────────────────────────────────────────────────────

    void SpiderLife()
    {
        if (_home == null) return;
        if (_self != null) _self.busy = false;   // (al final, si queda al acecho)
        _strikeCooldown -= Time.deltaTime;
        SnowBeeBehavior bee = Nearest<SnowBeeBehavior>(transform.position, 1.6f);
        // La REINA no sale a pecorear: la araña caza obreras al acecho, no entra al panal (sin reina la colonia se pierde). Solo corre peligro en su vuelo nupcial.
        if (bee != null && bee.TryGetComponent(out IceInsectLife ql) && ql.isQueen && !ql.OnNuptialFlight) bee = null;
        if (web != null)  // primero, la que está pegada en SU tela
            foreach (IceInsectLife o in Cached<IceInsectLife>())
                if (o != null && o.kind == Kind.Bee && o.Stuck && web.Catches(o.transform.position) && o.TryGetComponent(out SnowBeeBehavior sb)) { bee = sb; break; }
        if (bee != null && !bee.death && _strikeCooldown <= 0f)
        {
            if (_strikeAt < 0f) _strikeAt = Time.time;
            if (FlyTo(bee.transform.position, 6f) < 0.25f)   // salto + mordisco con veneno
            {
                bee.death = true;
                if (_self != null) _self.hungry = -0.8f;   // se la come: queda saciada un buen rato
                _strikeCooldown = 20f; _strikeAt = -1f;
                Debug.Log("[Hielo] La araña de las nieves caza a una abeja de las nieves junto a su árbol.");
            }
            // FALLA: si en 1,5 s no la alcanza, vuelve a su tela y espera (antes saltaba sin parar tras las abejas del panal
            // de al lado: ida y vuelta, «da vueltas en círculo», 15–22 m en 20 s para menos de 1 m neto).
            else if (Time.time - _strikeAt > 1.5f) { _strikeCooldown = Random.Range(25f, 45f); _strikeAt = -1f; }
            return;
        }
        _strikeAt = -1f;
        // CORTEJO (macho adulto saciado): cuando la hembra adulta está en celo va a SU tela y la pulsa con las patas (las
        // arañas orbiculares cortejan haciendo vibrar los hilos de la tela de la hembra) y se queda cerca un rato.
        if (_self != null && _self.sex == Sex.male && _self.lifeStage == LifeStage.adult && _self.hungry < 0f)
        {
            IceInsectLife her = null;
            foreach (IceInsectLife o in Cached<IceInsectLife>())
                if (o != null && o != this && o.kind == Kind.Spider && o._self != null && !o._self.death && o._self.sex == Sex.female && o._self.lifeStage == LifeStage.adult
                    && Vector3.Distance(o.transform.position, transform.position) < 25f) { her = o; break; }
            if (her != null && her.TryGetComponent(out EstrusState e) && e.InEstrus) _courtUntil = Time.time + 30f;
            if (her != null && Time.time < _courtUntil && !(her._self.TryGetComponent(out Reproduction rp) && rp.Pregnant))
            {
                Doing = "corteja: pulsa la tela de la hembra";
                Vector3 edge = her.transform.position + (transform.position - her.transform.position).normalized * 0.7f;
                FlyTo(edge + Vector3.up * Mathf.Sin(Time.time * 14f) * 0.02f, 1.2f);
                return;
            }
        }
        // (2026-11-05, ronda 8: «8 SnowSpider atascadas > 20 s») En el centro de su tela ACECHA, quieta a propósito: ocupada, como la
        // abeja que trabaja en el panal. Su deseo genérico («explore») no la mueve: el insecto vuela por su cuenta.
        if (FlyTo(_perch, 1.2f) < 0.3f && _self != null) _self.busy = true;   // vuelve al centro de su tela
        if (web != null) transform.rotation = Quaternion.LookRotation(Vector3.down, web.transform.forward); else transform.rotation = Quaternion.LookRotation(Vector3.down + (transform.position - _home.transform.position).normalized * 0.3f);
    }

    // ── Utilidades ──────────────────────────────────────────────────────────────────────────────

    float FlyTo(Vector3 p, float speed)
    {
        if (kind == Kind.Bee && hive != null) p = hive.Via(transform.position, p);   // al panal se entra y se sale por la piquera
        Vector3 d = p - transform.position;
        float dist = d.magnitude;
        if (dist > 0.01f)
        {
            transform.position += d / dist * Mathf.Min(dist, speed * Time.deltaTime);
            if (kind == Kind.Bee) transform.position = OutOfBranches(transform.position);
            Vector3 flat = d; flat.y = 0f;
            if (kind == Kind.Bee && flat.sqrMagnitude > 0.001f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 6f * Time.deltaTime);
        }
        return dist;
    }

    // La abeja NO se mete en las ramas: el panal cuelga de una rama y sus celdillas, el racimo y los rodeos quedaban a
    // veces DENTRO del bloque «Rama»/«Rama_Plataforma» (o del tronco). Si su centro cae dentro de uno, sale por la cara
    // más cercana (+4 cm). Solo bloques del árbol (no el panal ni las celdillas, donde trabaja).
    // (2026-11-01) Hueco para 48 colliders y hasta 3 pasadas: junto al panal hay decenas de celdillas, abejas y escarcha, y con
    // 8 la rama a veces no entraba en la lista (la abeja seguía «dentro de Rama», JornadaTest); y al salir de un segmento por
    // su cara más cercana podía quedar dentro del segmento contiguo (se solapan un 5 %), que ya se había mirado.
    static readonly Collider[] _near = new Collider[48];
    static Vector3 OutOfBranches(Vector3 p, float margin = 0f, float push = 0.04f)
    {
        for (int pass = 0; pass < 3; pass++)
        {
            Vector3 before = p;
            p = OutOfBranchesOnce(p, margin, push);
            if ((p - before).sqrMagnitude < 1e-10f) break;
        }
        return p;
    }
    static Vector3 OutOfBranchesOnce(Vector3 p, float margin, float push)
    {
        int n = Physics.OverlapSphereNonAlloc(p, Mathf.Max(0.06f, margin * 1.8f), _near, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            if (!(_near[i] is BoxCollider box) || !(box.name.StartsWith("Rama") || box.name.StartsWith("Tronco"))) continue;
            Transform t = box.transform;
            Vector3 local = t.InverseTransformPoint(p) - box.center, half = box.size * 0.5f, s = t.lossyScale;
            // Penetración por eje, en metros del mundo; sale por la menor.
            float px = (half.x - Mathf.Abs(local.x)) * Mathf.Abs(s.x) + margin, py = (half.y - Mathf.Abs(local.y)) * Mathf.Abs(s.y) + margin, pz = (half.z - Mathf.Abs(local.z)) * Mathf.Abs(s.z) + margin;
            if (px < 0f || py < 0f || pz < 0f) continue;   // (el radio de la consulta la rozaba por fuera)
            // (2026-11-05, ronda 8: «SnowBee dentro de Tronco», quieta) El tronco son bloques apilados: salir por arriba o por abajo la
            // metía en el bloque de al lado y las 3 pasadas se agotaban en ese vaivén. Del tronco se sale siempre de lado.
            if (box.name.StartsWith("Tronco")) py = float.PositiveInfinity;
            if (px <= py && px <= pz) local.x = Mathf.Sign(local.x == 0f ? 1f : local.x) * (half.x + push / Mathf.Max(1e-4f, Mathf.Abs(s.x)));
            else if (py <= pz) local.y = Mathf.Sign(local.y == 0f ? 1f : local.y) * (half.y + push / Mathf.Max(1e-4f, Mathf.Abs(s.y)));
            else local.z = Mathf.Sign(local.z == 0f ? 1f : local.z) * (half.z + push / Mathf.Max(1e-4f, Mathf.Abs(s.z)));
            p = t.TransformPoint(local + box.center);
        }
        return p;
    }

    // LISTAS COMPARTIDAS (refrescadas cada segundo): antes cada insecto buscaba en toda la escena varias veces por frame.
    static readonly System.Collections.Generic.Dictionary<System.Type, (Object[] items, float at)> _cache = new System.Collections.Generic.Dictionary<System.Type, (Object[], float)>();
    // REVERSIÓN A NODRIZA: si en la colmena hay larvas y ninguna obrera tiene edad de nodriza (3–10 días), las obreras mayores
    // vuelven a criar (hasta 3 por colmena). Es real: en colonias sin nodrizas, pecoreadoras reactivan sus glándulas
    // hipofaríngeas y vuelven a alimentar larvas (Robinson, Page, Strambi & Robinson 1992, Ethology 90:336; Huang & Robinson
    // 1996, Behav Ecol Sociobiol 39:147). Sin esto, con el año comprimido de la escena de ciclo de vida todas las obreras eran
    // pecoreadoras antes de la primera larva y nadie la operculaba (ni pupa, ni abeja nueva, ni zángano).
    const int MaxReverted = 3;
    static readonly System.Collections.Generic.Dictionary<Hive, (float at, bool ageNurses, System.Collections.Generic.List<IceInsectLife> reverted)> _nurses = new System.Collections.Generic.Dictionary<Hive, (float, bool, System.Collections.Generic.List<IceInsectLife>)>();
    bool Reverts()
    {
        if (hive == null) return false;
        if (!_nurses.TryGetValue(hive, out var n) || Time.time - n.at >= 1f)
        {
            bool age = false;
            foreach (IceInsectLife o in Cached<IceInsectLife>())
                if (o != null && o.hive == hive && o.kind == Kind.Bee && !o.isQueen && !o.drone && o.ageDays >= 3f && o.ageDays < 10f && (o._self == null || !o._self.death)) { age = true; break; }
            var rev = n.reverted ?? new System.Collections.Generic.List<IceInsectLife>();
            rev.RemoveAll(b => b == null || (b._self != null && b._self.death));
            if (age || hive.Count(Hive.Cell.Larva) == 0) rev.Clear();
            n = (Time.time, age, rev);
            _nurses[hive] = n;
        }
        if (n.ageNurses || hive.Count(Hive.Cell.Larva) == 0) return false;
        if (n.reverted.Contains(this)) return true;
        if (n.reverted.Count >= MaxReverted) return false;
        n.reverted.Add(this);
        Debug.Log($"[Colmena] «{hive.name}» tiene larvas y ninguna nodriza: una obrera de {ageDays:0} días vuelve a criar.");
        return true;
    }

    static T[] Cached<T>() where T : Object
    {
        if (_cache.TryGetValue(typeof(T), out var c) && Time.time - c.at < 1f) return (T[])c.items;
        T[] items = FindObjectsByType<T>(FindObjectsSortMode.None);
        _cache[typeof(T)] = (items, Time.time);
        return items;
    }

    static T Nearest<T>(Vector3 p, float r) where T : Component
    {
        T best = null; float bd = r;
        foreach (T c in Cached<T>())
        {
            if (c == null) continue;
            float d = Vector3.Distance(c.transform.position, p);
            if (d < bd) { bd = d; best = c; }
        }
        return best;
    }

    static Bounds TreeBounds(Transform t)
    {
        Bounds b = new Bounds(t.position, Vector3.one);
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
        return b;
    }
}
