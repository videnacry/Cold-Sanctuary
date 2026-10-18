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
    public enum Role { Reina, Limpiadora, Nodriza, Procesadora, Guardiana, Pecoreadora }
    public Hive hive;
    public bool isQueen;
    public float ageDays = -1f;   // -1 = se reparte al empezar (la colonia tiene abejas de todas las edades)
    public Role role { get; private set; }
    int _cell = -1; float _work; Vector3 _source; bool _dancing; float _danceT; float _loadNectar;
    public string Doing { get; private set; } = "";
    public SpiderWeb web;          // (araña) su tela
    public bool Stuck => Time.time < _stuckUntil;
    float _stuckUntil, _freeUntil; // (abeja) pegada en una tela

    Animal _self;
    IceCarnivorousTree _home;
    Transform _nest;
    Vector3 _perch;
    float _angle, _height, _radius;
    Animal _carcass;
    bool _carrying;
    float _strikeCooldown, _nextSearch;

    void Start()
    {
        _self = GetComponent<Animal>();
        UnityEngine.AI.NavMeshAgent nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav != null) nav.enabled = false;   // vuela / trepa: no anda por el NavMesh
        if (TryGetComponent(out Rigidbody rb)) { rb.isKinematic = true; rb.useGravity = false; }   // vuela: la gravedad no la arrastra
        _home = Nearest<IceCarnivorousTree>(transform.position, 200f);
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
    void LateUpdate()
    {
        if (transform.localScale.x < 0.6f) transform.localScale = Vector3.one;
        if (_self != null && _self.death) return;
        if (kind == Kind.Bee) BeeLife(); else SpiderLife();
    }


    // ── Abeja ───────────────────────────────────────────────────────────────────────────────────

    void BeeLife()
    {
        if (Time.time < _stuckUntil) { Doing = "pegada en una telaraña"; transform.position += Random.insideUnitSphere * 0.004f; return; }   // se agita
        if (Time.time > _freeUntil) foreach (SpiderWeb w in SpiderWeb.All)
            if (w.Catches(transform.position)) { _stuckUntil = Time.time + 5f; _freeUntil = _stuckUntil + 4f; Debug.Log("[Hielo] Una abeja de las nieves queda pegada en una telaraña."); return; }
        if (hive != null) { ColonyLife(); return; }
        if (_home == null) return;
        SnowSpiderBehavior spider = Nearest<SnowSpiderBehavior>(transform.position, 1.5f);
        if (spider != null) { FlyTo(transform.position + (transform.position - spider.transform.position).normalized * 3f + Vector3.up * 2f, flySpeed * 1.8f); return; }

        bool day = TimeController.timeController == null || TimeController.timeController.IsDay;
        if (!day) { if (_nest != null) FlyTo(_nest.position + Vector3.up * 0.3f, flySpeed); return; }   // de noche, al nido

        if (_carrying)
        {
            Vector3 n = _nest != null ? _nest.position : _home.transform.position + Vector3.up * 8f;
            if (FlyTo(n, flySpeed) < 0.5f)
            {
                _carrying = false;
                _home.transform.localScale *= 1.002f;   // los nutrientes del muerto acaban en el árbol (bucle de nutrientes)
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
        float daysPerSec = 1f / Mathf.Max(0.1f, hive.secondsPerDay);
        ageDays += Time.deltaTime * daysPerSec;
        role = isQueen ? Role.Reina : ageDays < 3f ? Role.Limpiadora : ageDays < 10f ? Role.Nodriza : ageDays < 18f ? Role.Procesadora : ageDays < 21f ? Role.Guardiana : Role.Pecoreadora;
        if (isQueen && transform.localScale.x < 1.3f) transform.localScale = Vector3.one * 1.35f;   // la reina, más larga
        // COMEN MIEL de su colmena (las nodrizas alimentan a la reina con jalea): sin esto siempre tenían hambre.
        if (_self != null && _self.hungry >= 0f && hive.honeyStock > 0.5f && Vector3.Distance(transform.position, hive.combCenter) < 1.5f) { hive.Eat(0.5f); _self.hungry = -0.6f; }

        SnowSpiderBehavior spider = Nearest<SnowSpiderBehavior>(transform.position, 1.5f);
        if (spider != null && role != Role.Guardiana) { Doing = "huye de la araña"; FlyTo(transform.position + (transform.position - spider.transform.position).normalized * 3f + Vector3.up * 2f, flySpeed * 1.8f); return; }

        bool day = TimeController.timeController == null || TimeController.timeController.IsDay;
        Doing = role.ToString();
        if (!day)   // RACIMO de invierno/noche sobre el panal: tiritan y comen miel
        {
            Vector3 spot = hive.combCenter + Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * 0.25f;
            FlyTo(spot, flySpeed);
            hive.Eat(0.02f * Time.deltaTime);
            return;
        }

        switch (role)
        {
            case Role.Reina:
                if (Work(Hive.Cell.Empty, false, 1.5f)) { hive.Set(_cell, Hive.Cell.Egg); _cell = -1; }
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
            default: Forage(); break;
        }
    }

    // Va a una celdilla en ese estado y trabaja ahí <paramref name="seconds"/> s; true al terminar.
    bool Work(Hive.Cell need, bool top, float seconds)
    {
        if (_cell < 0 || _cell >= hive.state.Count || hive.state[_cell] != need) { _cell = hive.Find(need, transform.position, top); _work = 0f; }
        if (_cell < 0) return false;
        Vector3 perch = hive.Perch(_cell);
        float dd = FlyTo(perch, Vector3.Distance(transform.position, perch) > 1.5f ? flySpeed * 1.5f : flySpeed * 0.6f); Doing = $"{role} → celdilla {_cell} a {dd:0.0} m";
        if (dd > 0.08f) return false;
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
            _danceT += Time.deltaTime;
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
            _work += Time.deltaTime;
            if (_work > 2.5f) { _work = 0f; _loadNectar = 1f; _dancing = true; _danceT = 0f; _cell = -1; }
        }
    }
    Animal NearestCarcass()
    {
        Animal best = null; float bd = 30f;
        foreach (Animal a in FindObjectsByType<Animal>(FindObjectsSortMode.None))
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
        _strikeCooldown -= Time.deltaTime;
        SnowBeeBehavior bee = Nearest<SnowBeeBehavior>(transform.position, 1.6f);
        if (web != null)   // primero, la que está pegada en SU tela
            foreach (IceInsectLife o in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None))
                if (o.kind == Kind.Bee && o.Stuck && web.Catches(o.transform.position) && o.TryGetComponent(out SnowBeeBehavior sb)) { bee = sb; break; }
        if (bee != null && !bee.death && _strikeCooldown <= 0f)
        {
            if (FlyTo(bee.transform.position, 6f) < 0.25f)   // salto + mordisco con veneno
            {
                bee.death = true;
                if (_self != null) _self.hungry = -0.8f;   // se la come: queda saciada un buen rato
                _strikeCooldown = 20f;
                Debug.Log("[Hielo] La araña de las nieves caza a una abeja de las nieves junto a su árbol.");
            }
            return;
        }
        FlyTo(_perch, 1.2f);   // vuelve al centro de su tela
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
            Vector3 flat = d; flat.y = 0f;
            if (kind == Kind.Bee && flat.sqrMagnitude > 0.001f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 6f * Time.deltaTime);
        }
        return dist;
    }

    static T Nearest<T>(Vector3 p, float r) where T : Component
    {
        T best = null; float bd = r;
        foreach (T c in FindObjectsByType<T>(FindObjectsSortMode.None))
        {
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
