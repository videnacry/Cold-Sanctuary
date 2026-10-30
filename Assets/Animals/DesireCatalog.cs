using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Un DESEO seleccionable (docs/volition-selection-engine.md §3): cuánto lo quiere el ser AHORA (`NeedProbe`, de sus
/// drives) y qué acción despacha (`Dispatch`, un handler que YA existe). El destino es que un `Deseo` sea una
/// `MindPhrase(PhraseCategory.Deseo)` con este binding resuelto por catálogo (D3b3); por ahora es solo el binding.
/// </summary>
public class Desire
{
    public readonly string key;
    public readonly Func<Animal, float> NeedProbe;   // 0..1+ : necesidad actual (de los drives)
    public readonly Action<Animal> Dispatch;         // corre la acción existente (paso por defecto del suprahechizo)
    public readonly string capability;               // clave de confianza (D3a) para D3b3; null = sin ponderar
    public readonly Func<Animal, List<QueuedSpell>> StepsBuilder;   // opcional: descompone el deseo en sub-hechizos

    public Desire(string key, Func<Animal, float> need, Action<Animal> dispatch, string capability = null,
                  Func<Animal, List<QueuedSpell>> steps = null)
    {
        this.key = key; NeedProbe = need; Dispatch = dispatch; this.capability = capability; StepsBuilder = steps;
    }

    /// <summary>CONJURA el deseo como SUPRAHECHIZO (docs/spells-suprahechizo §3): una cola de sub-hechizos. Por defecto
    /// un solo paso = la acción de siempre (`Dispatch`); si el deseo trae `StepsBuilder`, usa su descomposición
    /// (analizar→acercarse→actuar…). La potencia = la necesidad actual (para el equilibrio: un hechizo más fuerte lo tumba).</summary>
    public SuperSpell Conjure(Animal self)
    {
        List<QueuedSpell> steps = StepsBuilder != null ? StepsBuilder(self)
                                                       : new List<QueuedSpell> { new DispatchStep(Dispatch, key) };
        return new SuperSpell { name = key, steps = steps, power = Mathf.Max(0.1f, NeedProbe(self)) };
    }
}

/// <summary>Un sub-hechizo que ejecuta una ACCIÓN existente (Action&lt;Animal&gt;) una vez y termina. El ladrillo con el
/// que un deseo-suprahechizo envuelve su conducta actual; se insertan más pasos (analizar/levantarse/acercarse) antes.</summary>
public class DispatchStep : QueuedSpell
{
    readonly Action<Animal> _act;
    readonly string _label;
    public DispatchStep(Action<Animal> act, string label = "acción") { _act = act; _label = label; }
    public override string Label => _label;
    public override bool Tick(Anima self) { if (_act != null && self is Animal an) _act(an); return true; }
}

/// <summary>
/// Catálogo de DESEOS (docs/volition-selection-engine.md §5): `key → { NeedProbe, Dispatch }`. Cada `Dispatch` llama
/// a un handler que YA existe — el motor de volición reubica la DECISIÓN, no reescribe la conducta.
///
/// **D3b1 (paridad):** solo `eat` (= lo único que decidía `ActiveBehaveTick` además del reflejo de amenaza). En D3b2
/// se añaden `rest`/`wander`/`defend` (este último como deseo con piso de seguridad); en D3b3 los deseos pasan a ser
/// frases y la selección pondera por `EffectiveWeight` (confianza D3a) + gate por receptor (E2).
/// </summary>
public static class DesireCatalog
{
    static List<Desire> _all;
    public static List<Desire> All { get { if (_all == null) _all = Build(); return _all; } }

    static List<Desire> Build() => new List<Desire>
    {
        // COMER: necesidad = hambre (>0 cuando hungry>=0, como el guard actual). Despacha el Feed existente
        // (Forager.Hunt/Graze) — idéntico a lo que hacía RespondToHunger.
        // No lo siente quien NO PUEDE forrajear (la cría lactante, la pupa: su hambre la atiende «nurse», abajo) ni quien
        // acaba de comprobar que no tiene comida a su alcance (Forager.GaveUp: el lobo en su perrera, el pingüino lejos del
        // banco): antes «eat» ganaba igual, Feed no hacía nada y el animal se quedaba plantado queriendo comer.
        new Desire("eat",
            // (el animal ENCERRADO —cría de un recinto de la Cría, paciente de la perrera— no forrajea: se la dan sus cuidadores,
            // ver «nurse» abajo; antes salía a cazar a las crías del recinto de al lado o a pescar desde la Cría y se plantaba)
            self => self.hungry >= 0f && Metamorphosis.CanForage(self) && !Confined(self) && (self.Forage == null || !self.Forage.GaveUp) ? Mathf.Max(0.01f, self.hungry) : 0f,
            self => self.StartCoroutine(self.Feed()),
            "forage",
            // SUPRAHECHIZO de comer: LEVANTARSE (paso compartido) → ANALIZAR → COMER. El 'levantarse' es la MISMA factory
            // que usan caminar/trepar (ActionSteps.GetUp): si mañana se parte en más pasos, este deseo lo hereda solo.
            // Más adelante: acercarse, cazar/pacer, o pedir-ayuda si no hay energía (docs/spells-suprahechizo §3).
            self =>
            {
                List<QueuedSpell> steps = new List<QueuedSpell>(ActionSteps.GetUp());
                steps.Add(new DispatchStep(a => Debug.Log($"[Deseo] «{a.name}» analiza: ¿comida cerca o a buscar?"), "analizar"));
                steps.Add(new DispatchStep(a => a.StartCoroutine(a.Feed()), "comer"));
                return steps;
            }),

        // BUSCAR PAREJA (reproducción paso 1): en celo, dirigirse al gradiente de Estrus (otras animas en celo). Dormido
        // hasta que Volition esté activo (D3b2); el cortejo/concepción llega en el paso 2. docs/environmental-navigation.md.
        new Desire("mate",
            self => { EstrusState e = self.GetComponent<EstrusState>(); return e != null && e.InEstrus ? W_MATE : 0f; },
            self =>
            {
                if (self.nav == null || !self.nav.isOnNavMesh) return;
                if (PenWander(self)) return;   // en un recinto de la Cría: no hay pareja que alcanzar; curiosea por su recinto
                // Todo destino, ALCANZABLE (camino completo por el NavMesh): antes iba hacia la pareja/llamada/rastro aunque
                // estuviera en otra placa de hielo o tras un muro, llegaba al final del camino posible y se plantaba ahí
                // «queriendo pareja» (JornadaTest: pingüinos atascados > 20 s con «mate»).
                // Si alguien de su especie LLAMA (destellos de luciérnaga, canto de cigarra) y lo percibe, va hacia él.
                MatingSignal caller = MatingSignal.NearestCaller(self, 40f * Mathf.Max(0.3f, self.perception));
                if (caller != null && self.sex == Sex.female && Flat(caller.transform.position - self.transform.position) > 2.5f && GoTo(self, "mate", caller.transform.position)) return;
                Vector3 g = TraceField.Trail(self.transform.position, TraceChannel.Estrus);
                if (g.sqrMagnitude > 0.0001f && GoTo(self, "mate", self.transform.position + g.normalized * 6f)) return;
                // Sin llamada ni rastro de celo que seguir: BUSCA (antes se quedaba plantado «queriendo pareja» todo el día):
                // hacia el adulto del otro sexo de su especie más cercano o, si no ve ninguno, un paseo de búsqueda.
                Animal mate = null; float md = 40f;
                foreach (GameObject o in self.Population)
                {
                    if (o == null || o == self.gameObject || !o.TryGetComponent(out Animal oa) || oa.death || oa.lifeStage != LifeStage.adult || oa.sex == self.sex) continue;
                    float dd = Vector3.Distance(o.transform.position, self.transform.position);
                    if (dd < md) { md = dd; mate = oa; }
                }
                // JUNTO A LA PAREJA: el cortejo es ir y venir a su lado, con pausas (antes, a < 2 m, el paseo al azar de 8 m
                // lo alejaba y el «ir hacia ella» lo traía: daba vueltas en círculo, 16 m en 20 s para 0,3 m netos).
                if (mate != null && md <= 2.5f) { Transform mt = mate.transform; Stroll(self, "cortejo", () => mt.position + Ring(1f, 2.5f), 1.5f, 3.5f, 0.6f); return; }
                if (mate != null && GoTo(self, "mate", mate.transform.position)) return;
                Explore(self, "buscar pareja");
            },
            "mate"),

        // ── APREMIOS SOCIALES como DESEOS (docs/consciousness-mechanics.md §4: la sim social es capacidad de Anima por
        // config; compite en la MISMA arena que comer/celo → de la pugna, resuelta por los stats propios, emerge la
        // personalidad). Reusan el registro de bonds por-miembro (Anima.bonds). Solo mueven vía NavMesh (Animal).

        // CUIDAR: acercarse al ser QUERIDO (bond+) que está en apuros (estrés/enfermo). Necesidad = vínculo × su apuro,
        // amplificada por la afabilidad del que cuida. Es el "Tend" de Sakshi/la tribu, ahora compitiendo con el hambre.
        new Desire("tend",
            self => SocialNeed(self, care: true) * Mathf.Max(0.3f, self.afabilidad) * W_TEND,
            self => ApproachBondTarget(self, care: true),
            "tend"),

        // NUTRIR (MIGRACIÓN del postnatal a la arena): el padre/madre ALIMENTA a una CRÍA (lifeStage='c') VINCULADA y
        // hambrienta. Necesidad = vínculo × hambre de la cría × afabilidad → compite con comer/amenaza como un deseo más
        // (antes lo conducía directo `PostNatalManager`). Despacha acercarse + nutrir (baja el hambre de la cría).
        new Desire("nurse",
            self => { CubNeed(self, out float n); return n * Mathf.Max(0.3f, self.afabilidad) * W_NURSE; },
            self => NurseCub(self),
            "tend"),

        // SER NUTRIDA (el otro lado de «nurse»): la CRÍA que aún no forrajea y tiene hambre no sale a cazar ni a pescar —
        // llama y se arrima a su madre/padre (el adulto vinculado más cercano) para que la alimente, o, sin él (la cría
        // huérfana de un recinto de la Cría), espera la toma en su sitio (su HomeOrigin). Antes «eat» ganaba, Feed no hacía
        // nada (CanForage = false) y la cría se quedaba plantada «queriendo comer» o daba vueltas por el recinto.
        new Desire("nurse",
            // También el ENCERRADO (recinto de la Cría, perrera del hospital): sus cuidadores le dan la toma (CriaFeeder, VetCare).
            self => self.hungry >= 0f && ((self.lifeStage == LifeStage.child && !Metamorphosis.CanForage(self)) || Confined(self)) ? Mathf.Max(0.01f, self.hungry) : 0f,
            self => AwaitFeeding(self),
            "follow"),

        // GUARDAR EL NIDO (tijereta, solífugo, escorpión; BroodRide.Guards): la madre con crías pequeñas VINCULADAS se queda
        // en casa vigilándolas (las tijeretas: juntan, limpian y defienden la puesta, Kölliker 2007). Necesidad = vínculo
        // con sus crías × afabilidad → una madre poco afable o hambrienta (eat gana) las deja más; el resto, no lo siente.
        new Desire("brood",
            self => BroodRide.Guards(self.SpeciesName) && self.sex == Sex.female ? BroodTie(self) * Mathf.Max(0.3f, self.afabilidad) * W_BROOD : 0f,
            self =>
            {
                if (self.nav == null || !self.nav.isOnNavMesh) return;
                if (Vector3.Distance(self.transform.position, self.HomeOrigin) > 1.5f) Move(self, self.HomeOrigin);
                else { self.nav.ResetPath(); self.Loco?.Idle(1f); self.alertness = Mathf.Max(self.alertness, 0.3f); }   // en el nido, atenta
            },
            "tend"),

        // SEGUIR/COHESIÓN: acercarse al ser querido más cercano (mantener la manada unida). Necesidad = vínculo × lejanía,
        // amplificada por la sociabilidad. Débil por diseño (fondo) → el hambre/amenaza lo superan; los muy sociales lo sienten más.
        new Desire("follow",
            self => SocialNeed(self, care: false) * Mathf.Max(0.2f, self.sociability) * W_FOLLOW,
            self => ApproachBondTarget(self, care: false),
            "follow"),

        // DESCANSAR (drive biológico real que faltaba): necesidad = sueño + fatiga. Despacha idle (se detiene; el
        // Restore pasivo recupera). Compite con todo → un ser agotado prioriza parar sobre deambular/comer un poco.
        new Desire("rest",
            self => Mathf.Clamp01((self.sleepiness + self.mentalFatigue) * 0.5f),
            self => { if (self.nav != null && self.nav.isOnNavMesh) self.nav.ResetPath(); self.Loco?.Idle(1f); },
            "rest"),

        // VOLVER AL HOGAR (cohesión de nido / territorialidad): deriva hacia el `HomeOrigin` cuando se aleja y no hay
        // apremio. Cubre el LADO CRÍA de la migración postnatal (quedarse cerca del nido) y a cualquier ser territorial.
        // Débil (fondo) → el hambre/amenaza/social lo superan. docs/sanctuary1-level-and-live-tests §3.
        // En casa (dentro de su zona) no lo siente; si su casa no se puede alcanzar desde donde está, deja de sentirlo un rato
        // (antes: SetDestination al punto exacto del hogar cada decisión → la colonia entera se arremolinaba en ese punto
        // —«da vueltas en círculo (homebound)»— y el que tenía la casa fuera de su trozo de NavMesh llegaba al final del
        // camino posible y se plantaba allí para siempre —«atascado > 20 s (homebound)»—).
        new Desire("homebound",
            self => HomeNeed(self),
            self => GoHome(self),
            "homebound"),

        // HÁBITO NATURAL (docs/natural-interactions.md): lo que la especie HACE en la naturaleza con su entorno —chupar
        // savia, cortar hierba para el termitero, acechar en un respiradero, llevar guijarros al nido, asolearse— sobre un
        // Affordance real, según su Ethogram. Gana al merodeo al azar (explore) cuando el ser está en calma y tiene algo
        // que hacer cerca; el hambre/amenaza/cría lo superan. Lo ejecuta el componente Habit (se añade solo).
        new Desire("habit",
            // Cansado y con guarida/descansadero cerca, el hábito ABSORBE al «rest» (descansa en su sitio natural, no donde caiga);
            // con hambre y comida natural cerca, compite con «eat».
            self =>
            {
                Habit h = self.GetComponent<Habit>(); if (h == null) h = self.gameObject.AddComponent<Habit>();
                if (!h.HasOptions()) return 0f;
                float tired = h.CanRestHere ? Mathf.Clamp01((self.sleepiness + self.mentalFatigue) * 0.5f) + 0.05f : 0f;
                // Con hambre y una forma natural de comer cerca (savia, bayas, respiradero), compite con «eat» de igual a igual.
                float hunger = h.CanFeedHere && self.hungry >= 0f ? Mathf.Max(0.01f, self.hungry) * 1.05f : 0f;
                return Mathf.Max(W_HABIT * Lucidity.ClarityOf(self), Mathf.Max(tired, hunger));
            },
            self => self.GetComponent<Habit>()?.Step(),
            "habit"),

        // EXPLORAR (impulso de base del ser SACIADO — curiosidad/neophilia, docs/environmental-navigation.md §N5/N8):
        // necesidad BAJA (solo gana cuando no hay hambre/amenaza/social) y ESCALADA por la mente curiosa (razón+creatividad)
        // → los curiosos exploran más; los apáticos se quedan. Despacha un vagabundeo corto.
        new Desire("explore",
            self => W_EXPLORE * Mathf.Clamp01((self.reasoning + self.creativity) * 0.5f),
            // Paseo a SALTOS con pausas (andar, pararse a mirar, seguir) sesgado hacia delante (LifeStage.WanderTarget: no vuelve por
            // donde vino) y siempre a un sitio alcanzable. Antes: un punto al azar a ≤ 8 m EN CADA DECISIÓN (cada ~2,5 s), aunque
            // aún fuera hacia el anterior → zigzag sin avanzar («da vueltas en círculo (explore)»), o un punto fuera del NavMesh
            // en el que se quedaba plantado. (la cría de un recinto de la Cría curiosea por su recinto: PenWander)
            self => { if (self.nav != null && self.nav.isOnNavMesh && !PenWander(self)) Explore(self, "explore"); },
            "explore"),
    };

    /// <summary>¿Está ENCERRADO al cuidado de alguien? Cría de un recinto de la Cría (hijo de un <see cref="CriaFeeder"/>) o
    /// animal ingresado en el hospital veterinario (<see cref="VetCare.Admitted"/>, el lobo de la perrera). No forrajea (se lo
    /// dan) y pasea solo por su recinto.</summary>
    public static bool Confined(Animal self) => self != null && (self.GetComponentInParent<CriaFeeder>() != null || VetCare.Admitted(self));

    /// <summary>
    /// UN ANIMAL ENCERRADO (recinto de la Cría, perrera) curiosea por su recinto: va a un sitio ALCANZABLE a 0,8–2,2 m del
    /// suyo, se para unos segundos y vuelta a empezar. Antes: un sitio al azar sin comprobar que se pudiera llegar (tras la
    /// guarida, una roca o el muro el agente nunca llegaba y, como «aún iba», no elegía otro: «atascado > 20 s queriendo
    /// explorar»), y sin pausas daba vueltas al recinto («da vueltas en círculo»). Devuelve false si no está encerrado.
    /// </summary>
    static bool PenWander(Animal self)
    {
        if (!Confined(self)) return false;
        Vector3 home = self.HomeOrigin;
        Stroll(self, "recinto", () => home + Ring(0.8f, 2.2f), 1.5f, 4f, 0.6f);
        return true;
    }

    // ══ MOVIMIENTO DE LOS DESEOS: siempre a sitios ALCANZABLES, con tope y sin re-apuntar en cada decisión ══════════════
    // Un «viaje» por animal: hacia dónde va, por qué deseo, desde cuándo y cuánto ha avanzado. Si en 5 s no se acerca (un muro,
    // otro animal en medio) o tarda más de 40 s, lo deja; ese deseo no vuelve a intentar ese viaje en unos segundos.
    class Trip { public string key; public Vector3 goal; public bool going; public float since, best, bestAt, pauseUntil, failUntil, lastCall; public int calls, started; }
    /// <summary>Diagnóstico: el viaje de un animal (deseo, si va, destino, pausas, llamadas).</summary>
    public static string TripInfo(Animal a) => a != null && _trips.TryGetValue(a, out Trip t) ? $"viaje «{t.key}» va={t.going} meta={t.goal:F1} pausa={t.pauseUntil - Time.time:0.0} fallo={t.failUntil - Time.time:0.0} llamadas={t.calls} (hace {Time.time - t.lastCall:0.0} s) empezados={t.started}" : "sin viaje";
    static readonly Dictionary<Animal, Trip> _trips = new Dictionary<Animal, Trip>();
    static readonly Dictionary<Animal, float> _homeBlocked = new Dictionary<Animal, float>();
    static int _homeLogs;
    /// <summary>Contadores para el diagnóstico (viajes empezados, atascados, paseos sin sitio alcanzable).</summary>
    public static int TripsStarted, TripsStalled, StrollNoPick, Strolls;

    static Trip TripOf(Animal a, string key)
    {
        if (!_trips.TryGetValue(a, out Trip t)) _trips[a] = t = new Trip();
        if (t.key != key) { t.key = key; t.going = false; t.pauseUntil = 0f; t.failUntil = 0f; }   // otro deseo: viaje nuevo
        return t;
    }

    static float Flat(Vector3 v) => new Vector2(v.x, v.z).magnitude;
    static Vector3 Ring(float rMin, float rMax) { Vector2 r = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(rMin, rMax); return new Vector3(r.x, 0f, r.y); }

    // ¿Sigue de camino? true mientras avanza hacia su destino; false al llegar o al atascarse (y entonces lo deja).
    static bool Going(Animal self, Trip t)
    {
        if (!t.going) return false;
        NavMeshAgent nav = self.nav;
        if (nav == null || !nav.isOnNavMesh) { t.going = false; return false; }
        if (nav.pathPending) { if (Time.time - t.since > 3f) { t.going = false; t.failUntil = Time.time + 4f; TripsStalled++; nav.ResetPath(); return false; } return true; }   // (camino que no llega a calcularse: tope)
        float rem = nav.hasPath ? nav.remainingDistance : 0f;
        if (rem <= nav.stoppingDistance + 0.35f) { t.going = false; return false; }   // llegó
        if (rem < t.best - 0.25f) { t.best = rem; t.bestAt = Time.time; }
        if (Time.time - t.bestAt > 5f || Time.time - t.since > 40f)                    // atascado o tope
        {
            t.going = false; t.failUntil = Time.time + 8f; TripsStalled++;
            nav.ResetPath();
            return false;
        }
        return true;
    }

    static void Walk(Animal self, Trip t, Vector3 at)
    {
        Move(self, at);
        TripsStarted++; t.started++; t.goal = at; t.going = true; t.since = t.bestAt = Time.time; t.best = float.MaxValue;
    }

    // Ir andando: destino Y paso de andar (la pausa anterior lo dejó en «quieto», velocidad 0: con solo SetDestination el
    // agente tenía destino pero no andaba).
    static void Move(Animal self, Vector3 to)
    {
        if (self.nav == null || !self.nav.isOnNavMesh) return;
        // Con el camino YA calculado (NavReach acaba de comprobar que se llega): SetPath, sin esperar a que el agente lo pida y
        // lo calcule otra vez (con cientos de obstáculos tallando, SetDestination podía quedarse «pendiente» y el animal quieto).
        if (NavReach.LastPathTo(self, to, out NavMeshPath p)) { self.Loco?.SetGait(false, 1f); if (self.nav.SetPath(p)) return; }
        if (self.Loco != null) self.Loco.Walk(to, 1f); else self.nav.SetDestination(to);
    }

    /// <summary>Va hacia <paramref name="dest"/> si se puede llegar (camino completo); true si va (o ya iba). Re-apunta solo
    /// si el destino se movió (una pareja que anda). false si no se puede llegar, si ya está allí o si se acaba de atascar.</summary>
    static bool GoTo(Animal self, string key, Vector3 dest)
    {
        Trip t = TripOf(self, key);
        if (Time.time < t.failUntil) return false;
        if (t.going && Flat(dest - t.goal) < 1.5f) { if (Going(self, t)) return true; if (Time.time < t.failUntil) return false; }
        if (!NavReach.Reachable(self, dest, out Vector3 at)) return false;
        if (Flat(at - self.transform.position) < 0.8f) return false;
        Walk(self, t, at);
        return true;
    }

    /// <summary>PASEO A SALTOS: anda a un sitio alcanzable que elige <paramref name="pick"/>, al llegar se para
    /// <paramref name="pauseMin"/>–<paramref name="pauseMax"/> s (mira, olfatea) y elige otro. Nunca re-apunta a medio camino.</summary>
    static void Stroll(Animal self, string key, Func<Vector3> pick, float pauseMin, float pauseMax, float minHop)
    {
        Trip t = TripOf(self, key);
        bool was = t.going; Strolls++; t.calls++; t.lastCall = Time.time;
        if (Going(self, t)) return;
        if (was) { t.pauseUntil = Time.time + UnityEngine.Random.Range(pauseMin, pauseMax); self.Loco?.Idle(t.pauseUntil - Time.time); }
        if (Time.time < t.pauseUntil) return;
        Vector3 here = self.transform.position;
        for (int i = 0; i < 8; i++)
        {
            Vector3 p = pick();
            if (Flat(p - here) < minHop) continue;
            if (NavReach.Reachable(self, p, out Vector3 at) && Flat(at - here) >= minHop) { Walk(self, t, at); return; }
        }
        StrollNoPick++; t.pauseUntil = Time.time + 1f;   // nada alcanzable ahora: lo reintenta en un momento
    }

    // Explorar: saltos de 4–10 m (según su territorio) sesgados hacia delante, con pausas cortas.
    static void Explore(Animal self, string key)
    {
        float step = Mathf.Clamp(self.HomeRadius * 0.08f, 4f, 10f);
        Transform tr = self.transform;
        Stroll(self, key, () => LifeStage.WanderTarget(tr.position, tr.forward, step, UnityEngine.Random.insideUnitCircle), 1f, 3f, 2f);
    }

    // Su zona de casa: dentro, «volver a casa» no se siente.
    static float HomeZone(Animal self) => Mathf.Clamp(self.HomeRadius * 0.08f, 3f, 10f);

    static float HomeNeed(Animal self)
    {
        if (_homeBlocked.TryGetValue(self, out float until) && Time.time < until) return 0f;
        float d = Flat(self.transform.position - self.HomeOrigin);
        if (d <= HomeZone(self)) return 0f;
        return W_HOME * Mathf.Clamp01(d / Mathf.Max(1f, self.HomeRadius));
    }

    // Vuelve a SU sitio de la zona de casa (cada uno el suyo: no todos al mismo punto). Si no hay camino completo, un trecho
    // hacia allí si el NavMesh lo permite; si tampoco, «casa inalcanzable desde aquí» y deja de sentirlo 90 s.
    static void GoHome(Animal self)
    {
        if (self.nav == null || !self.nav.isOnNavMesh) return;
        float a = (self.GetHashCode() % 360) * Mathf.Deg2Rad;
        Vector3 spot = self.HomeOrigin + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * HomeZone(self) * 0.5f;
        if (GoTo(self, "homebound", spot)) return;
        Trip t = TripOf(self, "homebound");
        if (Time.time >= t.failUntil && NavReach.PartialEnd(self, spot, out Vector3 end) && Flat(end - self.transform.position) > 2f) { Walk(self, t, end); return; }
        _homeBlocked[self] = Time.time + 90f;
        if (_homeLogs++ < 20) Debug.Log($"[Fauna] «{self.name}» no puede volver a casa desde ({self.transform.position.x:0},{self.transform.position.z:0}) (hogar en ({self.HomeOrigin.x:0},{self.HomeOrigin.z:0})): lo deja estar un rato.");
    }

    // ── PESOS DE LA ARENA (docs/consciousness-mechanics.md §4) — afinables. Filosofía: la SUPERVIVENCIA gana cuando
    // aprieta (eat = hambre, que puede ser >1; la amenaza es reflejo aparte), pero lo SOCIAL gana cuando el ser está
    // saciado. El celo NO debe atropellar a comer. La resolución final depende de los STATS propios → personalidad.
    const float W_MATE    = 0.6f;    // celo moderado (antes 1.0 → atropellaba el hambre media)
    const float W_TEND    = 0.9f;    // cuidar al vínculo en apuro: fuerte (× afabilidad)
    const float W_FOLLOW  = 0.35f;   // cohesión: de fondo (× sociabilidad); el hambre/amenaza lo superan
    const float W_EXPLORE = 0.08f;   // curiosidad de base: muy bajo → solo gana cuando el ser está saciado y en calma
    const float W_NURSE   = 1.0f;    // nutrir a la cría: fuerte (× afabilidad) → un padre atiende a su cría hambrienta
    const float W_HOME    = 0.12f;   // volver al nido: de fondo → solo gana cuando el ser está saciado y lejos de casa
    const float W_BROOD   = 0.5f;    // guardar el nido: por debajo de nutrir y del hambre fuerte, por encima de los hábitos
    const float W_HABIT   = 0.2f;    // hábitos naturales: por encima del merodeo y del volver a casa; el hambre/social/cría lo superan

    // Vínculo con las crías propias aún pequeñas (0..1): el más fuerte. 0 si no tiene.
    static float BroodTie(Animal self)
    {
        float best = 0f;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || b.target == null) continue;
            Animal cub = (b.target as Component)?.GetComponentInParent<Animal>();
            if (cub == null || cub == self || cub.death || cub.lifeStage != LifeStage.child) continue;
            best = Mathf.Max(best, Mathf.Clamp01(b.value / 100f));
        }
        return best;
    }

    // Cría más necesitada: recorre los bonds, busca Animal en etapa CRÍA ('c') y hambrienta; devuelve urgencia = vínculo×hambre.
    static Animal CubNeed(Animal self, out float need)
    {
        need = 0f; Animal best = null;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || b.target == null) continue;
            Animal cub = (b.target as Component)?.GetComponentInParent<Animal>();
            if (cub == null || cub == self || cub.death || cub.lifeStage != LifeStage.child) continue;
            float urg = Mathf.Clamp01(b.value / 100f) * Mathf.Clamp01(cub.hungry);
            if (urg > need) { need = urg; best = cub; }
        }
        return best;
    }

    // Nutrir: ir a la cría más necesitada y, si está cerca, bajarle el hambre (le pasa alimento); cuesta algo al que nutre.
    static void NurseCub(Animal self)
    {
        Animal cub = CubNeed(self, out _);
        if (cub == null) return;
        if (self.nav != null && self.nav.isOnNavMesh) Move(self, cub.transform.position);
        if (Vector3.Distance(self.transform.position, cub.transform.position) <= 2.5f)
        {
            cub.hungry = Mathf.Max(0f, cub.hungry - 0.3f);   // amamantar/compartir comida
            self.exhaustion += 0.5f;                          // cuidar cansa un poco
        }
    }

    // La cría hambrienta se arrima al adulto vinculado más cercano (su madre/padre) o, si no lo tiene, se queda en su sitio
    // esperando la toma. Pasos cortos y con destino fijo (sin vagar por el recinto).
    static void AwaitFeeding(Animal self)
    {
        if (self.nav == null || !self.nav.enabled || !self.nav.isOnNavMesh) return;
        Transform parent = null; float best = 30f;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || b.target == null) continue;
            Animal o = (b.target as Component)?.GetComponentInParent<Animal>();
            if (o == null || o == self || o.death || o.lifeStage != LifeStage.adult || o.SpeciesName != self.SpeciesName) continue;
            float d = Vector3.Distance(o.transform.position, self.transform.position);
            if (d < best) { best = d; parent = o.transform; }
        }
        Vector3 to = parent != null ? parent.position : self.HomeOrigin;
        if (Vector3.Distance(self.transform.position, to) > (parent != null ? 2f : 1.5f)) Move(self, to);
        else { self.nav.ResetPath(); self.Loco?.Idle(1f); }
    }

    // Necesidad social: recorre los bonds POSITIVOS y devuelve la mayor "urgencia" — cuidar = vínculo×apuro del otro;
    // seguir = vínculo×lejanía. 0..1+. No mueve; solo puntúa (lo usa NeedProbe).
    static float SocialNeed(Animal self, bool care)
    {
        float best = 0f;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || b.target == null) continue;
            Transform t = b.target.transform; if (t == null) continue;
            Anima o = t.GetComponentInParent<Anima>(); if (o == null || o == self) continue;
            float tie = Mathf.Clamp01(b.value / 100f);
            float urg = care ? Mathf.Clamp01(o.stress)                                   // cuidar: cuanto peor está el otro
                             : Mathf.Clamp01(Vector3.Distance(self.transform.position, t.position) / 20f);  // seguir: cuán lejos
            best = Mathf.Max(best, tie * urg);
        }
        return best;
    }

    // Despacho social: navega hacia el ser querido elegido (el más urgente por cuidado, o el más cercano por cohesión).
    static void ApproachBondTarget(Animal self, bool care)
    {
        if (self.nav == null || !self.nav.isOnNavMesh) return;
        Transform best = null; float bestScore = 0f;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || b.target == null) continue;
            Transform t = b.target.transform; if (t == null) continue;
            Anima o = t.GetComponentInParent<Anima>(); if (o == null || o == self) continue;
            float tie = Mathf.Clamp01(b.value / 100f);
            float urg = care ? Mathf.Clamp01(o.stress)
                             : Mathf.Clamp01(Vector3.Distance(self.transform.position, t.position) / 20f);
            float score = tie * urg;
            if (score > bestScore) { bestScore = score; best = t; }
        }
        if (best != null) Move(self, best.position);
    }
}

/// <summary>
/// ¿SE PUEDE LLEGAR? Lo comparten los deseos, la huida y el mantenimiento del animal: el punto del NavMesh más cercano a un
/// sitio (≤ 2 m) con un camino COMPLETO desde donde está el agente. Un destino fuera del NavMesh (en el agua, dentro de un
/// mueble, en otra placa de hielo, tras el muro de un recinto) hacía que el agente fuera al final del camino posible y se
/// quedara allí plantado para siempre.
/// </summary>
public static class NavReach
{
    static readonly NavMeshPath _path = new NavMeshPath();
    static Animal _lastOwner; static Vector3 _lastTo;

    /// <summary>El último camino calculado (Reachable/PartialEnd), si era de este animal hacia ese punto.</summary>
    public static bool LastPathTo(Animal self, Vector3 to, out NavMeshPath path)
    {
        path = _path;
        return self != null && self == _lastOwner && (to - _lastTo).sqrMagnitude < 0.01f && _path.corners.Length > 0;
    }

    public static bool Reachable(Animal self, Vector3 p, out Vector3 at, float sample = 2f)
    {
        at = p;
        if (self == null || self.nav == null || !self.nav.enabled || !self.nav.isOnNavMesh) return false;
        if (!NavMesh.SamplePosition(p, out NavMeshHit h, sample, self.nav.areaMask)) return false;
        if (!self.nav.CalculatePath(h.position, _path) || _path.status != NavMeshPathStatus.PathComplete) return false;
        at = h.position; _lastOwner = self; _lastTo = at;
        return true;
    }

    /// <summary>El final del camino POSIBLE hacia un sitio al que no se llega del todo (camino parcial).</summary>
    public static bool PartialEnd(Animal self, Vector3 p, out Vector3 end)
    {
        end = p;
        if (self == null || self.nav == null || !self.nav.enabled || !self.nav.isOnNavMesh) return false;
        if (!NavMesh.SamplePosition(p, out NavMeshHit h, 6f, self.nav.areaMask)) return false;
        if (!self.nav.CalculatePath(h.position, _path) || _path.status != NavMeshPathStatus.PathPartial || _path.corners.Length == 0) return false;
        end = _path.corners[_path.corners.Length - 1]; _lastOwner = self; _lastTo = end;
        return true;
    }
}
