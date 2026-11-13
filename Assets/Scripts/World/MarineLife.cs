using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// VIDA MARINA (focas, ballenas, orcas): nadan de verdad por la LAGUNA central, fuera del NavMesh (el NavMesh solo cubre el
/// hielo). Antes el NavMeshAgent los "pegaba" al borde de hielo más cercano: las focas se quedaban amontonadas en la
/// orilla y las ballenas, quietas. Su día:
///   · CON HAMBRE → nadan al banco de peces más cercano y comen al atravesarlo (el banco los alimenta, `Swarm`).
///   · EN CELO (hembra adulta) → buscan a un macho adulto de su especie (la reproducción necesita tenerlo cerca).
///   · CRÍAS → siguen a su madre (o a la hembra adulta más cercana) y maman a su lado.
///   · SACIADOS → merodean por la laguna a distintas profundidades.
///   · FOCAS → además SALEN A DESCANSAR al hielo (de noche, o un rato tras comer), cada una a un punto distinto de la orilla
///     (como en una colonia real, separadas entre sí), y vuelven al agua al tener hambre o al amanecer.
/// Ir al baño, dormir, celo y edad los siguen llevando sus componentes de siempre.
/// </summary>
public class MarineLife : MonoBehaviour
{
    public static readonly Vector3 Lagoon = new Vector3(60f, 0f, 25f);   // = PolarLandscapeBuilder.Lagoon
    const float LagoonRadius = 26f, SwimY = -1f;

    public float speed = 2f;
    public bool hauls;   // sale a descansar al hielo (focas)
    Animal _a;
    EstrusState _estrus;
    Vector3 _target;
    float _nextPick, _restUntil, _restCooldown;
    bool _onIce;
    Vector3 _beach;
    IcebergVisitor _visitor;   // vive en un iceberg: es su hogar en vez de la laguna (docs/vida-en-icebergs.md)
    Transform _parent;
    Vector3 Home => _visitor != null && _visitor.berg != null ? _visitor.berg.transform.position : Lagoon;
    float _angleSeed, _nextKill, _nextSuckle;
    // Etograma marino (Ethogram, docs/natural-interactions.md): descansadero en el hielo, respiradero, grava para mudar.
    Affordance _haul, _spot;

    /// <summary>¿Está en un hábito natural (respirando por su respiradero, frotándose en la grava, patrullando los témpanos,
    /// descansando en un descansadero)? Lo cuenta el censo de <see cref="AnimalVitals"/>.</summary>
    // Tumbada en su trozo de orilla (con o sin descansadero marcado) también está en su hábitat: descansa a propósito.
    public bool InHabitat => _withMom || (_spot != null && _spotUntil >= 0f) || (_haul != null && transform.position.y > -0.3f) || (_onIce && transform.position.y > -0.3f);
    bool _withMom;
    float _nextSpot, _spotUntil;
    static int _count;
    // BAJO EL HIELO: las placas de la banquisa («Casquete») van de IceTop (0) a −1,5 m. Nadando a −1 m se metían DENTRO de
    // la placa (respiraderos, orillas, ir a por la pareja): por debajo del hielo se bucea por debajo de su cara inferior.
    /// <summary>La cara inferior del hielo (placas y HIELO FIJO: 1,5 m). Solo se nada por debajo del hielo fijo (docs/hielo-y-respiraderos.md): las placas son pared (P20).</summary>
    const float IceBottom = -SeaRule.PlateThickness;
    /// <summary>Nombre de los colisionadores del hielo fijo.</summary>
    public const string ThinIce = "Hielo_fijo";
    float _halfH = 0.4f, _halfLen;   // medio alto y medio largo del cuerpo (en el mundo)


    public bool OnIce => _onIce;

    void Start()
    {
        if (GetComponentInParent<CriaFeeder>() != null) { Destroy(this); return; }   // cría de un recinto de la Cría: vive en su recinto (NavMesh), no en el mar
        _a = GetComponent<Animal>();
        _estrus = GetComponent<EstrusState>();
        string sp = _a != null ? _a.SpeciesName : "";
        _visitor = GetComponent<IcebergVisitor>();
        _parent = transform.parent;
        hauls = sp == "Seal" || sp == "Walrus" || _visitor != null;
        speed = sp == "Orca" ? 3.2f : sp == "Whale" ? 1.6f : sp == "Walrus" ? 1.8f : 2.2f;
        if (TryGetComponent(out NavMeshAgent nav)) nav.enabled = false;   // nada: no anda por el NavMesh
        if (TryGetComponent(out Rigidbody rb)) { rb.isKinematic = true; rb.useGravity = false; }
        if (transform.position.y > -0.3f && !hauls) transform.position = new Vector3(transform.position.x, SwimY, transform.position.z);
        _restCooldown = Time.time + Random.Range(20f, 60f);
        _angleSeed = (_count++ * 67f) % 360f;   // cada foca, su trozo de orilla
        Bounds b = new Bounds(transform.position, Vector3.zero); bool any = false;
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) { if (r is ParticleSystemRenderer) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        if (any) { _halfH = Mathf.Clamp(b.extents.y, 0.2f, 1.6f); _halfLen = Mathf.Max(b.extents.x, b.extents.z); }
    }

    /// <summary>¿Hay hielo (o algo sólido: un edificio sobre el hielo) encima del mar en (x, z)?</summary>
    public static bool IceAbove(Vector3 p)
    {
        int n = Physics.RaycastNonAlloc(new Vector3(p.x, 8f, p.z), Vector3.down, _hits, 14f, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
            if (_hits[i].point.y > -0.3f && _hits[i].collider.GetComponentInParent<Anima>() == null) return true;   // (no un animal ni una persona encima)
        return false;
    }
    static readonly RaycastHit[] _hits = new RaycastHit[8];

    /// <summary>¿Lo que hay encima de (x, z) es pared para la fauna (una placa, la isla, un edificio)? Contra eso no se bucea, aunque mida 1,5 m: es una decisión de diseño (P20). El hielo fijo no cuenta.</summary>
    public static bool ThickAbove(Vector3 p)
    {
        int n = Physics.RaycastNonAlloc(new Vector3(p.x, 8f, p.z), Vector3.down, _hits, 14f, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
            if (_hits[i].point.y > -0.3f && _hits[i].collider.name != ThinIce && _hits[i].collider.GetComponentInParent<Anima>() == null) return true;
        return false;
    }

    /// <summary>La cara de arriba del hielo grueso en la vertical de <paramref name="p"/>.</summary>
    static bool ThickTop(Vector3 p, out float top)
    {
        top = float.NegativeInfinity;
        int n = Physics.RaycastNonAlloc(new Vector3(p.x, 8f, p.z), Vector3.down, _hits, 14f, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
            if (_hits[i].point.y > -0.3f && _hits[i].collider.name != ThinIce && _hits[i].collider.GetComponentInParent<Anima>() == null && _hits[i].point.y > top) top = _hits[i].point.y;
        return n > 0 && !float.IsNegativeInfinity(top);
    }

    // Profundidad a la que nada por debajo de la placa (el lomo, 15 cm bajo su cara inferior).
    float UnderIceY => IceBottom - _halfH - 0.15f;

    // ¿Hay hielo sobre el cuerpo, no solo sobre su pivote? Una orca mide 6–8 m: con el pivote en agua abierta, el cuerpo entra en la placa.
    bool IceOverBody(Vector3 p)
    {
        float r = _halfLen * 0.6f;
        return IceAbove(p) || (r > 0.5f && (IceAbove(p + Vector3.right * r) || IceAbove(p + Vector3.left * r) || IceAbove(p + Vector3.forward * r) || IceAbove(p + Vector3.back * r)));
    }

    void LateUpdate()   // después de la IA genérica (que intentaba moverlos por el NavMesh)
    {
        if (_a == null || _a.death) return;
        if (TryGetComponent(out NavMeshAgent nav) && nav.enabled) nav.enabled = false;
        // Siempre se endereza (el balanceo de frotarse lo vuelve a fijar Habitat en el mismo fotograma): ninguna física ni
        // giro acumulado la deja de lado o panza arriba.

        Vector3 fw = transform.forward; fw.y = 0f;
        if (fw.sqrMagnitude > 1e-4f && Vector3.Angle(transform.up, Vector3.up) > 0.5f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(fw.normalized), 3f * Time.deltaTime);
        bool night = TimeController.timeController != null && !TimeController.timeController.IsDay;
        // (2026-11-04) Se aparta del SUBMARINO DE OBRA (docs/submarino-de-obra.md) si su casco pasa a menos de 6 m: nada hacia el lado contrario.
        if (!_onIce && transform.position.y < -0.3f)
        {
            ConstructionSubmarine sub = ConstructionSubmarine.Nearest(transform.position, 6f + _halfLen);
            if (sub != null) { Vector3 away = transform.position - sub.HullCenter; if (away.sqrMagnitude < 0.01f) away = Vector3.right; Move(transform.position + away.normalized * 4f, speed * 1.5f, false); return; }
        }

        // ── FOCAS: descanso en el hielo ──
        if (hauls)
        {
            if (_onIce)
            {
                // (2026-11-02) TUMBADA SIN HIELO DEBAJO: un descansadero en el borde de la placa (o fuera de ella) dejaba a la foca «en el
                // hielo» tumbada en el FONDO del mar (Settle la bajaba a lo primero que hubiera: y = −2), quieta 60–140 s y de vuelta al
                // mismo sitio en cuanto descansaba (Seal «atascada > 20 s» ×34 en (87, −2, 30) y (60, −2, −6), JornadaTest). Se levanta y
                // busca otra orilla.
                bool noIce = !SolidAt(transform.position);
                if (noIce) _beach = Vector3.zero;
                bool wake = noIce || (_a.hungry >= 0.2f) || (!night && Time.time > _restUntil);
                if (!wake) { Settle(); return; }
                _onIce = false; _restCooldown = Time.time + Random.Range(40f, 90f);
                if (_haul != null) { _haul.Release(_a); _haul = null; _beach = Vector3.zero; }
                if (_visitor != null) { transform.SetParent(_parent, true); _target = _visitor.berg.Approach; _target.y = SwimY; }
                else { _target = Lagoon + Random.insideUnitSphere * 10f; _target.y = SwimY; }
            }
            else if ((night || (_a.hungry < -0.2f && Time.time > _restCooldown)) && _a.lifeStage != LifeStage.child)
            {
                if (_beach == Vector3.zero) _beach = FindBeach();
                // Al iceberg se llega por delante de su repisa: la quilla no choca y atravesarla no sería real.
                if (_visitor != null && Flat(transform.position - _visitor.berg.Approach) > 3f && !SolidAt(transform.position)) { Move(new Vector3(_visitor.berg.Approach.x, SwimY, _visitor.berg.Approach.z), speed, false); return; }
                if (_visitor != null) _beach = _visitor.berg.HaulSpot(_angleSeed);   // (la repisa deriva: se recalcula)
                if (_beach != Vector3.zero && Move(_beach, speed * 0.6f, onIce: true) < 0.5f)
                {
                    _onIce = true; _restUntil = Time.time + Random.Range(60f, 140f);
                    if (_visitor != null)
                    {
                        var (why, hours) = IcebergVisitor.Stay(_a.SpeciesName, _visitor.berg.grounded, TimeController.timeController != null ? TimeController.timeController.Day : 0);
                        _restUntil = Time.time + hours * (TimeController.timeController != null ? TimeController.timeController.SecondsPerGameDay / 24f : 150f);
                        transform.SetParent(_visitor.berg.transform, true);   // va con el iceberg
                        Debug.Log($"[Icebergs] «{name}» sube a {_visitor.berg.name}: {why}, {hours:0} h.");
                    }
                    Debug.Log($"[Marinos] «{name}» sale a descansar al hielo en ({_beach.x:0},{_beach.z:0}).");
                }
                return;
            }
        }

        _withMom = false;
        // ── CRÍA: sigue a su madre ──
        if (_a.lifeStage == LifeStage.child)
        {
            Animal mom = NearestAdultFemale();
            if (mom != null)
            {
                // MAMA A SU LADO (en el agua o tumbadas en el hielo): con hambre y pegada a ella, una toma cada 2 s de juego.
                // Antes solo la seguía: el «nurse» de la madre (DesireCatalog) anda por el NavMesh hacia la cría y en el mar no
                // tiene agente, así que la foca nunca amamantaba (grupo 58: «Crianza: la foca amamanta» no se veía).
                // (A su lado según su tamaño: una ballena de 14 m tiene el centro lejos del de su ballenato aunque lo lleve pegado.)
                MarineLife ml = mom.GetComponent<MarineLife>();
                float reach = 2.5f + (ml != null ? ml._halfLen : 0f) + _halfLen;
                if (_a.hungry >= 0f && Time.time >= _nextSuckle && Vector3.Distance(mom.transform.position, transform.position) <= reach)
                { _nextSuckle = Time.time + 2f; DesireCatalog.FeedCub(mom, _a); }
                Vector3 side = mom.transform.right * 1.4f;
                if (ml != null && ml.OnIce) { _onIce = true; Move(mom.transform.position + side, speed, onIce: true); return; }
                _onIce = false;
                // (2026-11-02) PEGADA A SU MADRE mientras ella está en su hábitat (patrullando los témpanos, respirando): la cría está
                // quieta a su lado a propósito, también en su hábitat. La orca cría «atascada > 20 s» en (45, −3, 12) y (83, −3, 30)
                // estaba a 1,4 m de su madre, que patrullaba los témpanos (en vivo).
                _withMom = ml != null && ml.InHabitat && Vector3.Distance(mom.transform.position, transform.position) <= reach + 1f;
                Vector3 t = mom.transform.position + side; t.y = Mathf.Min(t.y, SwimY);
                Move(t, speed * 1.2f, false);
                return;
            }
        }

        // ── Hambre → banco de peces; celo → pareja; si no, merodeo por la laguna ──
        if (_a.hungry >= 0.5f && _a.SpeciesName == "Orca" && Time.time > _nextKill)   // ORCA: si tiene MUCHA hambre caza una foca en el agua (rara vez); si no, pesca
        {
            Animal prey = null; float pd = 25f;
            foreach (GameObject g in AnimalPopulations.Of("Seal"))
                if (g != null && g.TryGetComponent(out Animal s) && !s.death && s.transform.position.y < -0.3f && !(s.TryGetComponent(out MarineLife sm) && sm.OnIce)) { float dd = Vector3.Distance(s.transform.position, transform.position); if (dd < pd) { pd = dd; prey = s; } }
            if (prey != null)
            {
                if (Move(prey.transform.position, speed * 1.3f, false) < 1.5f) { prey.Hurt(9999f); _a.hungry = -0.9f; _nextKill = Time.time + 300f; Debug.Log("[Marinos] Una orca caza una foca en la laguna."); }
                return;
            }
        }
        if (_a.hungry >= 0f)
        {
            // (la morsa baja a los bivalvos del fondo; los demás, al banco en agua abierta)
            Swarm fish = _a.Forage != null && _a.Forage.eatsClams ? Swarm.Nearest(transform.position, SwarmKind.Clam) : OpenWaterSwarm(transform.position);
            if (fish != null) { Vector3 f = fish.transform.position; f.y = Mathf.Min(f.y, SwimY); Move(f, speed * 1.2f, false); return; }
        }
        if (_estrus != null && _estrus.InEstrus && _a.sex == Sex.female && _a.lifeStage == LifeStage.adult)
        {
            Animal male = NearestAdult(Sex.male);
            if (male != null) { Move(male.transform.position, speed, false); return; }
        }
        if (Habitat()) return;
        // (llegada EN PLANTA: bajo una placa nada a UnderIceY —la orca, a −3,25 m— y un destino a −1…−3,5 m justo encima no
        //  se alcanzaba nunca en 3D: se quedaba debajo, subiendo y bajando, hasta el siguiente sorteo)
        if (Time.time > _nextPick || new Vector2(transform.position.x - _target.x, transform.position.z - _target.z).sqrMagnitude < 1f)
        {
            Vector2 r = Random.insideUnitCircle * LagoonRadius;
            if (_visitor != null && _visitor.berg != null) r = Random.insideUnitCircle.normalized * _visitor.berg.SwimRadius;   // en corro, fuera de la quilla
            _target = new Vector3(Home.x + r.x, SwimY - Random.Range(0f, 2.5f), Home.z + r.y);
            _nextPick = Time.time + Random.Range(8f, 18f);
        }
        Move(_target, speed * 0.6f, false);
    }

    // EL BANCO EN AGUA ABIERTA más cercano (los de peces primero; si no hay, cualquiera). Uno que huyendo se metió bajo una
    // placa queda a −1 m, DENTRO del hielo (la placa va de 0 a −1,5 m): bajo ella el depredador bucea a UnderIceY y nunca lo
    // toca, y con hambre se quedaba debajo para siempre (la orca, JornadaTest 2026-11-01). null si todos están bajo el hielo.
    static Swarm OpenWaterSwarm(Vector3 from)
    {
        Swarm best = null, bestAny = null; float bd = float.MaxValue, bdAny = float.MaxValue;
        foreach (Swarm s in Swarm.All)
        {
            if (s == null || s.Consumed || (s.kind != SwarmKind.Fish && s.kind != SwarmKind.Krill) || IceAbove(s.transform.position) || UnderTree(s.transform.position, 6f)) continue;   // (ni los de debajo de un edificio-árbol: se meten en su tronco)
            float d = (s.transform.position - from).sqrMagnitude;
            if (s.kind == SwarmKind.Fish && d < bd) { bd = d; best = s; }
            if (d < bdAny) { bdAny = d; bestAny = s; }
        }
        return best ?? bestAny;
    }

    /// <summary>¿(x, z) cae bajo el tronco de un edificio-árbol (con <paramref name="margin"/> m de más) y por encima del fondo marino?</summary>
    public static bool UnderTree(Vector3 p, float margin)
    {
        foreach (ArbolCampos t in ArbolCampos.All)
        {
            if (t == null) continue;
            Vector3 l = t.transform.InverseTransformPoint(p);
            if (Mathf.Max(Mathf.Abs(l.x), Mathf.Abs(l.z)) < t.waterHalf + margin && l.y > SeaRule.FloorY) return true;
        }
        return false;
    }
    /// <summary>¿Hay hielo o tierra (no el fondo del mar) bajo (x, z)? Lo primero que hay debajo, a ras del agua o más alto.</summary>
    static bool SolidAt(Vector3 p)
    {
        int n = Physics.RaycastNonAlloc(new Vector3(p.x, p.y + 2f, p.z), Vector3.down, _hits, 8f, ~0, QueryTriggerInteraction.Ignore);
        float best = float.MaxValue; bool solid = false;
        for (int i = 0; i < n; i++)
        {
            if (_hits[i].collider.GetComponentInParent<Animal>() != null || _hits[i].distance >= best) continue;
            best = _hits[i].distance; solid = _hits[i].point.y > -0.3f;
        }
        return solid;
    }

    // Tumbada en el hielo (a ras de suelo), sin moverse: descansa y, de noche, duerme (SleepCycle).
    void Settle()
    {
        Vector3 p = transform.position;
        if (Physics.Raycast(p + Vector3.up * 2f, Vector3.down, out RaycastHit h, 6f, ~0, QueryTriggerInteraction.Ignore) && h.collider.GetComponentInParent<Animal>() == null)
            transform.position = new Vector3(p.x, h.point.y + 0.25f, p.z);
    }

    float Move(Vector3 to, float v, bool onIce)
    {
        Vector3 cur = transform.position;
        // Bajo una placa de hielo y queriendo salir al hielo: primero sale a agua abierta (hacia la laguna); trepar desde
        // debajo la metía dentro de la placa.
        bool underIce = cur.y < -0.3f && IceAbove(cur);
        if (onIce && underIce) { Vector3 open = _visitor != null && _visitor.berg != null ? cur - _visitor.berg.transform.position : Home - cur; open.y = 0f; to = cur + (open.sqrMagnitude > 0.01f ? open.normalized : Vector3.forward) * 3f; to.y = SwimY; onIce = false; }
        Vector3 d = to - transform.position;
        float dist = d.magnitude;
        if (dist > 0.01f)
        {
            Vector3 step = d / dist * Mathf.Min(dist, v * Time.deltaTime);
            Vector3 np = cur + step;
            if (onIce && Physics.Raycast(np + Vector3.up * 2f, Vector3.down, out RaycastHit h, 6f, ~0, QueryTriggerInteraction.Ignore) && h.point.y > -0.3f && h.collider.GetComponentInParent<Animal>() == null)
            {
                float top = h.point.y + 0.25f;
                np.y = Mathf.MoveTowards(cur.y, top, 2f * Time.deltaTime);   // sube al hielo por la orilla
                if (cur.y < top - 0.3f && !IceAbove(cur)) { np.x = cur.x; np.z = cur.z; }   // en el borde: sube ANTES de avanzar (no se mete en la placa)
            }
            else if (!onIce)
            {
                np.y = Mathf.Min(np.y, -0.4f);   // nadando: siempre bajo la superficie
                // Las PLACAS y la isla son paredes: se bordean (P20, docs/hielo-y-respiraderos.md). Bajo el HIELO FIJO se bucea; una orca que roza el borde con el cuerpo, también (IceOverBody).
                Vector3 flatDir = new Vector3(step.x, 0f, step.z);
                Vector3 ahead = flatDir.sqrMagnitude > 1e-8f ? np + flatDir.normalized * (_halfH + 1f) : np;
                Vector3 flatNow = new Vector3(step.x, 0f, step.z);
                Vector3 nose = flatNow.sqrMagnitude > 1e-8f ? np + flatNow.normalized * (_halfLen + 0.6f) : np;
                if (ThickAbove(cur))   // (una aparición o un empujón lo dejó dentro: sale hacia la laguna, sin pasar por debajo)
                {
                    Vector3 open = _visitor != null && _visitor.berg != null ? cur - _visitor.berg.transform.position : Home - cur; open.y = 0f;   // (a un visitante su hogar es el iceberg: hacia él se metía más; sale hacia fuera)
                    np = cur + open.normalized * v * Time.deltaTime;
                    // Al despertar en su descansadero del casquete salía nadando POR DENTRO del hielo hasta el borde: va por encima y se tira al agua.
                    np.y = _visitor == null && ThickTop(cur, out float top) ? top + _halfH : cur.y;
                }
                else if (ThickAbove(np) || ThickAbove(nose))
                {
                    bool free = false;
                    foreach (float turn in new[] { 50f, -50f, 90f, -90f })   // lo bordea
                    {
                        Vector3 side = Quaternion.Euler(0f, turn, 0f) * flatNow;
                        if (side.sqrMagnitude > 1e-8f && !ThickAbove(cur + side + side.normalized * (_halfLen + 0.6f))) { np.x = cur.x + side.x; np.z = cur.z + side.z; free = true; break; }
                    }
                    if (!free) { np.x = cur.x; np.z = cur.z; }
                    dist = 0f; _nextPick = 0f;   // lo que busca queda tras el hielo: ha llegado al borde, y elige otro sitio
                }
                else if (IceOverBody(np) || IceAbove(ahead))
                {
                    float under = UnderIceY;
                    // EN PLANTA bajo el hielo: aquí la altura la manda la placa (UnderIceY), así que el paso horizontal no puede
                    // salir del vector 3D: con el destino casi encima (un banco de peces a −1 m, la orca a −3,25 m) el paso era
                    // casi vertical, la placa lo devolvía abajo y no avanzaba (orca «atascada > 20 s» en (40,−3,15), (46,−3,13) y
                    // (86,−3,24), JornadaTest 2026-11-01).
                    Vector3 flatTo = new Vector3(d.x, 0f, d.z); float fd = flatTo.magnitude;
                    if (fd > 1e-4f) { float s = Mathf.Min(fd, v * Time.deltaTime); np.x = cur.x + flatTo.x / fd * s; np.z = cur.z + flatTo.z / fd * s; }
                    np.y = Mathf.MoveTowards(cur.y, Mathf.Min(np.y, under), 2.5f * Time.deltaTime);
                    if (cur.y > under + 0.05f && IceAbove(np) && !IceAbove(cur)) { np.x = cur.x; np.z = cur.z; }
                    dist = Vector3.Distance(new Vector3(to.x, 0f, to.z), new Vector3(np.x, 0f, np.z));   // bajo el hielo cuenta la distancia en planta
                }
            }
            // (2026-11-04) Los EDIFICIOS-ÁRBOL tienen el fondo abierto (docs/edificio-arbol.md): ballenas, orcas y focas no se meten por el
            // hueco a su tronco («cae» en el tronco de Entrenamiento, jornada 50): si el paso entra bajo un tronco, se queda fuera.
            if (!onIce && UnderTree(np, 2f) && !UnderTree(cur, 2f)) { np.x = cur.x; np.z = cur.z; np.y = Mathf.Min(np.y, cur.y); }
            transform.position = np;
            Vector3 flat = step; flat.y = 0f;
            if (flat.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 4f * Time.deltaTime);
        }
        return dist;
    }

    // SACIADOS: lo que su especie hace con el entorno marino (Ethogram). Foca → sube a RESPIRAR por un respiradero del
    // hielo (y lo mantiene abierto; el oso que acecha allí la ve salir). Ballena → se FROTA contra la grava para mudar.
    // Orca → patrulla junto a los témpanos donde descansan las focas. true = está en ello.
    bool Habitat()
    {
        string sp = _a.SpeciesName;
        if (sp == "Walrus") return false;   // su hábito es el descanso en la orilla (hauls)
        if (_spot == null)
        {
            if (Time.time < _nextSpot) return false;
            _nextSpot = Time.time + Random.Range(40f, 100f);
            AffordanceKind k = sp == "Seal" ? AffordanceKind.BreathingHole : sp == "Whale" ? AffordanceKind.RubbingGravel : AffordanceKind.HaulOut;
            _spot = Affordance.Nearest(k, transform.position, 90f, k == AffordanceKind.HaulOut ? null : _a);
            if (sp == "Orca" && _spot != null && (_spot.lastUser == null || Time.time - _spot.lastUseTime > 200f)) _spot = null;   // solo los témpanos con focas recientes
            if (_spot == null) return false;
            _spotUntil = -1f;
        }
        Vector3 at = _spot.transform.position;
        if (sp == "Orca") { Vector3 off = (Lagoon - at); off.y = 0f; at += off.normalized * 4f; }
        // (2026-11-13, ronda 78) LA FOCA LLEGA AL RESPIRADERO POR DEBAJO DEL HIELO FIJO: a SwimY (−1 m) el destino quedaba dentro de la placa
        // (0 a −1,5 m) y las que no conseguían el agujero (solo cabe una) se quedaban allí, dentro de «Hielo_fijo» (tres focas en (61,−1,−1)).
        // Se acerca a nado por debajo de su cara inferior; solo la que lo ocupa sube por el agujero (abajo, «asoma de morro»).
        at.y = sp == "Seal" ? UnderIceY : SwimY;
        if (_spotUntil < 0f)
        {
            if (Move(at, speed, false) > 1.2f) return true;
            if (sp != "Orca" && !_spot.Occupy(_a)) { _spot = null; return false; }
            _spotUntil = Time.time + (sp == "Seal" ? Random.Range(10f, 18f) : Random.Range(15f, 30f));
            Debug.Log($"[Marinos] «{name}» {(sp == "Seal" ? "sale a respirar por su respiradero" : sp == "Whale" ? "se frota contra la grava para mudar la piel" : "patrulla junto a los témpanos de las focas")}.");
        }
        // (2026-11-05, ronda 8: «Orca dentro de Casquete (47, −2, 4)», en su hábitat) Patrullando junto a los témpanos ya no la lleva Move: se
        // queda a SwimY con el cuerpo bajo el borde de la placa. Si tiene hielo encima, baja por debajo de su cara inferior.
        if (sp == "Orca" && IceOverBody(transform.position))
        {
            Vector3 here = transform.position; here.y = Mathf.MoveTowards(here.y, UnderIceY, 2.5f * Time.deltaTime);
            transform.position = here;
        }
        if (sp == "Seal")
        {
            // Asoma por el agujero del hielo fijo, de morro: el agujero es de 1,6 m y la foca mide 1,7 (docs/hielo-y-respiraderos.md).
            float y = IceAbove(at) ? UnderIceY : -0.45f;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(at.x, y, at.z), speed * Time.deltaTime);
            Vector3 head = transform.forward; head.y = 0f; if (head.sqrMagnitude < 1e-4f) head = Vector3.forward;
            transform.rotation = Quaternion.LookRotation(head.normalized) * Quaternion.Euler(-65f, 0f, 0f);
            _spot.Touch(_a);   // el oso que acecha la oye/ve respirar
        }
        else if (sp == "Whale")
        {
            // Se frota de costado contra la grava: un balanceo de ±35° (antes rodaba 40°/s sin parar y acababa panza arriba,
            // volcada 137–178°).

            Vector3 fwd = transform.forward; fwd.y = 0f; if (fwd.sqrMagnitude < 1e-4f) fwd = Vector3.forward;
            transform.rotation = Quaternion.LookRotation(fwd.normalized) * Quaternion.Euler(0f, 0f, 35f * Mathf.Sin((Time.time - _spotUntil) * 1.2f));
        }
        if (Time.time < _spotUntil) return true;
        _spot.Release(_a); _spot = null;
        return false;
    }

    static float Flat(Vector3 v) => new Vector2(v.x, v.z).magnitude;

    // Un punto de la orilla de la laguna (hielo a ras del agua) en una dirección propia de cada foca, separado de las demás.
    // Primero, un DESCANSADERO (HaulOut) libre del entorno; si no hay, uno propio por su ángulo.
    Vector3 FindBeach()
    {
        if (_visitor != null && _visitor.berg != null) return _visitor.berg.HaulSpot(_angleSeed);
        _haul = Affordance.Nearest(AffordanceKind.HaulOut, transform.position, 120f, _a);
        if (_haul != null && !SolidAt(_haul.transform.position)) _haul = null;   // (sin hielo debajo no es descansadero: la foca acababa tumbada en el fondo)
        if (_haul != null && _haul.Occupy(_a)) { _haul.Touch(_a); return _haul.transform.position + Vector3.up * 0.25f; }
        _haul = null;
        float baseAngle = _angleSeed + Random.Range(-15f, 15f);
        for (int tries = 0; tries < 12; tries++)
        {
            float a = (baseAngle + tries * 30f) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            for (float r = 10f; r < 60f; r += 1.5f)
            {
                Vector3 p = Lagoon + dir * r;
                if (!Physics.Raycast(p + Vector3.up * 5f, Vector3.down, out RaycastHit h, 10f, ~0, QueryTriggerInteraction.Ignore) || h.point.y < -0.3f) continue;
                Vector3 beach = h.point + dir * 2.5f + Vector3.up * 0.25f;   // un poco tierra adentro
                bool crowded = false;
                foreach (MarineLife o in FindObjectsByType<MarineLife>(FindObjectsSortMode.None))
                    if (o != this && o._beach != Vector3.zero && Vector3.Distance(o._beach, beach) < 3.5f) { crowded = true; break; }
                if (!crowded) return beach;
                break;
            }
        }
        return Vector3.zero;
    }

    Animal NearestAdultFemale() => NearestAdult(Sex.female);

    Animal NearestAdult(char sex)
    {
        Animal best = null; float bd = 40f;
        foreach (GameObject g in _a.Population)
        {
            if (g == null || g == gameObject || !g.TryGetComponent(out Animal o) || o.death) continue;
            if (o.lifeStage != LifeStage.adult || o.sex != sex) continue;
            float d = Vector3.Distance(o.transform.position, transform.position);
            if (d < bd) { bd = d; best = o; }
        }
        return best;
    }
}
