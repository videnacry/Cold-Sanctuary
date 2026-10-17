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
        _home = Nearest<IceCarnivorousTree>(transform.position, 200f);
        if (_home != null)
        {
            foreach (Transform t in _home.GetComponentsInChildren<Transform>()) if (t.name.StartsWith("Nido_Abejas")) { _nest = t; break; }
            Bounds b = TreeBounds(_home.transform);
            _radius = Mathf.Clamp(b.extents.x, 4f, 12f);
            if (kind == Kind.Spider)   // puesto de acecho: en el tronco, a media altura
            {
                Vector3 d = Random.insideUnitSphere; d.y = 0f; d = d.sqrMagnitude > 0.01f ? d.normalized : Vector3.forward;
                _perch = _home.transform.position + d * 1.1f + Vector3.up * Random.Range(2f, 5f);
                transform.position = _perch;
            }
        }
        _angle = Random.value * 360f; _height = Random.Range(3f, 9f);
    }

    // El cuerpo de cubos ya está a su tamaño real modificado (20–30 cm); las etapas de vida del insecto (pensadas para la
    // escala del microcosmos) lo dejaban en ~0. Se mantiene a escala 1.
    void LateUpdate() { if (transform.localScale.x < 0.6f) transform.localScale = Vector3.one; }

    void Update()
    {
        if (_self != null && _self.death) return;
        if (kind == Kind.Bee) BeeLife(); else SpiderLife();
    }

    // ── Abeja ───────────────────────────────────────────────────────────────────────────────────

    void BeeLife()
    {
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
        if (bee != null && !bee.death && _strikeCooldown <= 0f)
        {
            if (FlyTo(bee.transform.position, 6f) < 0.25f)   // salto + mordisco con veneno
            {
                bee.death = true;
                _strikeCooldown = 20f;
                Debug.Log("[Hielo] La araña de las nieves caza a una abeja de las nieves junto a su árbol.");
            }
            return;
        }
        FlyTo(_perch, 1.2f);   // vuelve a su puesto en el tronco (trepa)
        transform.rotation = Quaternion.LookRotation(Vector3.down + (transform.position - _home.transform.position).normalized * 0.3f);
    }

    // ── Utilidades ──────────────────────────────────────────────────────────────────────────────

    float FlyTo(Vector3 p, float speed)
    {
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
