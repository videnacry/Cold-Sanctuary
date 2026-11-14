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
                if (PenWander(self)) return;   // en un recinto de la Cría: no hay pareja que alcanzar; curiosea por su recinto (ANTES del NavMesh: ronda 83, PenWalker lo apaga)
                if (self.nav == null || !self.nav.isOnNavMesh) return;
                // Todo destino, ALCANZABLE (camino completo por el NavMesh): antes iba hacia la pareja/llamada/rastro aunque
                // estuviera en otra placa de hielo o tras un muro, llegaba al final del camino posible y se plantaba ahí
                // «queriendo pareja» (JornadaTest: pingüinos atascados > 20 s con «mate»).
                // Si alguien de su especie LLAMA (destellos de luciérnaga, canto de cigarra) y lo percibe, va hacia él.
                MatingSignal caller = MatingSignal.NearestCaller(self, 40f * Mathf.Max(0.3f, self.perception));
                if (caller != null && self.sex == Sex.female && Flat(caller.transform.position - self.transform.position) > 2.5f && GoTo(self, "mate", caller.transform.position)) return;
                // Sin llamada: hacia el adulto del otro sexo de su especie más cercano que VE; si no ve ninguno, sigue el rastro del
                // celo y, sin rastro, un paseo de búsqueda (antes se quedaba plantado «queriendo pareja» todo el día).
                // (2026-11-02) El rastro, solo si no ve pareja, y cada tramo de 6 m hasta el final: en la colonia el rastro es el de
                // todos (también el SUYO: cada adulto en celo deja Estrus) y su pendiente cambiaba a cada decisión; re-apuntando
                // cada 2,5 s el pingüino iba y venía 30 m en 20 s para 1,5 m netos («da vueltas en círculo (mate)», en vivo).
                Animal mate = null; float md = 40f;
                foreach (GameObject o in self.Population)
                {
                    if (o == null || o == self.gameObject || !o.TryGetComponent(out Animal oa) || oa.death || oa.lifeStage != LifeStage.adult || oa.sex == self.sex) continue;
                    float dd = Vector3.Distance(o.transform.position, self.transform.position);
                    if (dd < md) { md = dd; mate = oa; }
                }
                // JUNTO A LA PAREJA: el cortejo es ir y venir a su lado, con pausas (antes, a < 2 m, el paseo al azar de 8 m
                // lo alejaba y el «ir hacia ella» lo traía: daba vueltas en círculo, 16 m en 20 s para 0,3 m netos).
                // (2026-11-14, ronda 103) CON MARGEN: el oso de (−118, −123) estaba a ~2,5 m de su pareja y cada decisión cambiaba de «cortejo» (a ≤ 2,5 m) a
                // «ir hacia la pareja» o «buscar pareja» (a más): al cambiar de clave su viaje se ponía a cero (pausa y fallo a 0) y no hacía ni lo uno ni lo
                // otro, quieto 100–200 s, con otros animales encajados contra él. Ya en cortejo, sigue en cortejo hasta 4 m.
                bool courting = _trips.TryGetValue(self, out Trip ct) && ct.key == "cortejo";
                if (mate != null && md <= (courting ? CourtingKeep : 2.5f)) { Transform mt = mate.transform; Stroll(self, "cortejo", () => mt.position + Ring(1.5f, 3f), 1.5f, 3.5f, 1.5f); return; }   // (ronda 72: el mínimo de un salto, 1,5 m)
                if (mate != null && GoTo(self, "mate", mate.transform.position)) return;
                if (mate == null)
                {
                    Trip tr = TripOf(self, "rastro");
                    if (Going(self, tr)) return;
                    Vector3 g = TraceField.Trail(self.transform.position, TraceChannel.Estrus);
                    if (g.sqrMagnitude > 0.0001f && Time.time >= tr.failUntil && NavReach.Reachable(self, self.transform.position + g.normalized * 6f, out Vector3 at) && Flat(at - self.transform.position) >= 2f) { Walk(self, tr, at); return; }
                }
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
                else { NavTrace.Reset(self, "brood (vigila el nido)"); self.Loco?.Idle(1f); self.alertness = Mathf.Max(self.alertness, 0.3f); }   // en el nido, atenta
            },
            "tend"),

        // ── PROCESOS NATURALES (LifeHistory; docs/ciclo-de-vida.md) ──
        // BEBER: con sed, al bebedero o comer nieve (los que sacan el agua de la comida no lo sienten: Hydration).
        new Desire("drink",
            self => { Hydration h = self.GetComponent<Hydration>(); return h != null && !Confined(self) ? h.Need * W_DRINK : 0f; },
            self => Drink(self),
            "forage"),
        // GUARIDA DE MATERNIDAD (oso polar, malamute): la preñada entra antes del parto, pare y cría dentro, y sale con las crías.
        new Desire("den",
            self => DenNeed(self),
            self => StayInDen(self),
            "tend"),
        // INCUBAR (pingüino emperador): con el huevo sobre las patas, quieto y en ayunas; el macho va a por el huevo.
        new Desire("incubate",
            self => Egg.ToIncubate(self) != null ? W_INCUBATE : 0f,
            self => Incubate(self),
            "tend"),
        // LA CRÍA SIGUE A SU MADRE en tierra (osezno, cachorro, polluelo junto a sus padres; LifeHistory.youngFollow).
        new Desire("heel",
            self => HeelNeed(self),
            self => Heel(self),
            "follow"),

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
            self => { if (self.nav != null && self.nav.isOnNavMesh) NavTrace.Reset(self, "rest (descansa)"); self.Loco?.Idle(1f); },
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
            // (2026-11-13, ronda 83) PenWander ANTES de mirar el NavMesh: la cría del recinto anda sin agente (PenWalker) y con la comprobación delante
            // nunca se llegaba a PenWander tras el primer paseo (PenWalker 10/10 en 2 días; «quiere explore, sin viaje, agente apagado»).
            self => { if (PenWander(self)) return; if (self.nav != null && self.nav.isOnNavMesh) Explore(self, "explore"); },
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
        // (2026-11-11, vivo59) Su sitio, si sigue cerca; si no, donde está: con la Cría en el piso del árbol (a 108 m) su HomeOrigin se quedó en el
        // recinto viejo, los pasitos caían lejos de todo NavMesh y las crías no se movían nunca («recinto» 1 802 s sin moverse, Cachorro_1/2/3).
        // (2026-11-11, vivo61) Y A UN SITIO DE VERDAD: con pasitos de 0,8–2,2 m el punto alcanzable más cercano era su propio sitio (meta =
        // su posición: «llegaba» al momento, 124–283 viajes en 800–1 700 llamadas, 1 800 s sin moverse: Polluelo_1/2/3, Osezno_2). Ahora un punto
        // de 1,5 a 5 m, con camino completo (PenGoal); si no hay ninguno, descansa (sin viaje).
        Trip t = TripOf(self, "recinto");
        t.calls++; t.lastCall = Time.time;
        // (2026-11-13, ronda 82) EN UN RECINTO DE LA CRÍA, SIN NAVMESH: el recinto gira con el árbol y su NavMesh se recoloca a cada paso; la cría
        // anda en las coordenadas del recinto (PenWalker). Mismo ritmo: un sitio a 1,5–4 m, se para 4–9 s, otro.
        PenWalker pw = PenWalker.For(self);
        if (pw != null)
        {
            if (pw.Going) return true;
            if (pw.TakeArrival()) { Outcome(self, t, true, "llegó (recinto)"); t.pauseUntil = Time.time + UnityEngine.Random.Range(4f, 9f); return true; }
            if (Time.time < t.pauseUntil) return true;
            if (pw.PickGoal()) { t.restNoSpot = false; t.goal = pw.GoalWorld; t.started++; TripsStarted++; return true; }
            t.restNoSpot = true; t.pauseUntil = Time.time + UnityEngine.Random.Range(4f, 9f); self.Loco?.Idle(1f);
            return true;
        }
        if (t.going) { if (Going(self, t)) return true; t.pauseUntil = Time.time + UnityEngine.Random.Range(4f, 9f); self.Loco?.Idle(1f); return true; }   // llegó (o se atascó): mira y olfatea
        if (Time.time < t.pauseUntil || Time.time < t.failUntil) return true;
        if (PenGoal(self, out Vector3 goal)) { t.restNoSpot = false; Walk(self, t, goal); return true; }
        t.restNoSpot = true;
        t.pauseUntil = Time.time + UnityEngine.Random.Range(4f, 9f); self.Loco?.Idle(1f);
        return true;
    }

    /// <summary>(2026-11-11) Un sitio del recinto de 1,5 a 5 m de la cría (alrededor de su sitio, o de donde está si su sitio guardado quedó lejos),
    /// con camino completo. false si no hay ninguno en 16 intentos.</summary>
    public static bool PenGoal(Animal self, out Vector3 goal)
    {
        goal = self != null ? self.transform.position : Vector3.zero;
        if (self == null) return false;
        Vector3 here = self.transform.position;
        Vector3 home = Vector3.Distance(self.HomeOrigin, here) > 6f ? here : self.HomeOrigin;
        for (int i = 0; i < 16; i++)
        {
            Vector3 p = home + Ring(1.5f, 5f);
            if (NavReach.Reachable(self, p, out Vector3 at, 1f) && Flat(at - here) >= 1.5f && Flat(at - here) <= 5.5f && !Detour(self, at, here)) { goal = at; return true; }   // (sin rodeos: Polluelo_2, 2,3 m por 6 esquinas, vivo62)
        }
        return false;
    }

    // ══ MOVIMIENTO DE LOS DESEOS: siempre a sitios ALCANZABLES, con tope y sin re-apuntar en cada decisión ══════════════
    // Un «viaje» por animal: hacia dónde va, por qué deseo, desde cuándo y cuánto ha avanzado. Si en 5 s no se acerca (un muro,
    // otro animal en medio) o tarda más de 40 s, lo deja; ese deseo no vuelve a intentar ese viaje en unos segundos.
    class Trip { public string key; public Vector3 goal; public bool going, restNoSpot, unfroze; public Vector3 lastDir; public float since, best, bestAt, pauseUntil, failUntil, lastCall, reissueAt, frozenSince = -1f, unfrozeWarpAt; public int calls, started, sinceFrame, bestFrame, frozenFrame; public Vector3 frozenPos; public string endWhy; public float endAt; }
    /// <summary>(2026-11-11) ¿Descansa a propósito? La cría del recinto sin sitio a 1,5–5 m (PenGoal) se queda mirando: no está «atascada».
    /// (2026-11-13, vivo75) Y cualquier paseo sin salto posible (Stroll: PickHop no encuentra nada): se para y mira, como la cría del recinto.</summary>
    public static bool RestingByChoice(Animal a) => a != null && _trips.TryGetValue(a, out Trip t) && t.restNoSpot && !t.going;

    /// <summary>(2026-11-13, vivo75) Segundos con camino, velocidad pedida y sin moverse antes de reiniciar el agente (y, a la segunda, dejar el viaje).</summary>
    const float FrozenSeconds = 1.5f;
    /// <summary>(2026-11-14, ronda 96) Fotogramas mínimos de cada tope de un viaje (quieto, atascado, sin camino, pendiente): con el tiempo ×6 de
    /// las pruebas un fotograma son hasta 4 s de juego, y un tope en segundos saltaba en 1–2 fotogramas, antes de que el agente pudiera andar.</summary>
    public const int MinFrames = 4;
    /// <summary>¿Han pasado <paramref name="secs"/> s de juego Y <see cref="MinFrames"/> fotogramas desde (<paramref name="since"/>, <paramref name="frame"/>)?</summary>
    static bool Past(float since, int frame, float secs) => Time.time - since > secs && Time.frameCount - frame >= MinFrames;
    /// <summary>Agentes reiniciados por estar quietos con camino, y viajes dejados por seguir quietos tras reiniciar (diagnóstico).</summary>
    public static int FrozenRestarts, FrozenTrips, EdgeNudges, StaleRepaths, FrozenWarps;
    static int _frozenLogs;
    /// <summary>(2026-11-13) Si el agente está a &lt; 0,3 m del borde del NavMesh, el punto 0,6 m hacia dentro (de los dos lados de la normal del
    /// borde, el que queda más lejos de él).</summary>
    static bool InwardFromEdge(NavMeshAgent nav, out Vector3 inside)
    {
        inside = nav.nextPosition;
        if (!NavMesh.FindClosestEdge(nav.nextPosition, out NavMeshHit eh, nav.areaMask) || eh.distance >= 0.3f) return false;
        Vector3 n = new Vector3(eh.normal.x, 0f, eh.normal.z);
        if (n.sqrMagnitude < 1e-4f) { n = nav.nextPosition - eh.position; n.y = 0f; }
        if (n.sqrMagnitude < 1e-4f) return false;
        n.Normalize();
        float bestEdge = eh.distance; bool found = false;
        foreach (float s in new[] { 1f, -1f })
        {
            if (!NavMesh.SamplePosition(eh.position + n * (0.6f * s), out NavMeshHit h, 0.4f, nav.areaMask)) continue;
            if (!NavMesh.FindClosestEdge(h.position, out NavMeshHit e2, nav.areaMask) || e2.distance <= bestEdge + 0.1f) continue;
            bestEdge = e2.distance; inside = h.position; found = true;
        }
        return found;
    }
    /// <summary>(2026-11-13) QUIETO CON CAMINO: 0 = anda o no le toca andar; 1 = lleva más de <see cref="FrozenSeconds"/> quieto (primera vez:
    /// reiniciar el agente); 2 = otra vez quieto tras reiniciarlo (dejar el viaje y probar otra dirección).</summary>
    /// (2026-11-14, ronda 96) «Moverse» es que el CUERPO se haya movido (no la velocidad del agente, que tras un salto hecho entre dos fotogramas
    /// lentos ya es 0), y hacen falta ≥ <see cref="FrozenSeconds"/> Y ≥ <see cref="MinFrames"/> fotogramas.
    static int FrozenStep(Trip t, bool wants, float speedNow, float now, int frame)
    {
        if (!wants || speedNow > 0.05f) { t.frozenSince = -1f; return 0; }
        if (t.frozenSince < 0f) { t.frozenSince = now; t.frozenFrame = frame; return 0; }
        if (now - t.frozenSince <= FrozenSeconds || frame - t.frozenFrame < MinFrames) return 0;
        t.frozenSince = -1f;
        if (!t.unfroze) { t.unfroze = true; return 1; }
        return 2;
    }
    /// <summary>(Pruebas) FrozenStep sobre una serie (quiere andar, velocidad, tiempo): los resultados separados por comas.</summary>
    public static string FrozenStepsForTest(params (bool wants, float speed, float time, int frame)[] steps)
    {
        var t = new Trip(); var r = new List<string>();
        foreach (var s in steps) r.Add(FrozenStep(t, s.wants, s.speed, s.time, s.frame).ToString());
        return string.Join(",", r);
    }
    /// <summary>(2026-11-13) OTRA DIRECCIÓN: tras un viaje que no avanzó o un paseo sin salto, el «hacia delante» del siguiente gira 60–120° a un lado
    /// (no vuelve a probar contra lo mismo).</summary>
    static void TurnAway(Animal self, Trip t)
    {
        Vector3 f = t.lastDir.sqrMagnitude > 0.5f ? t.lastDir : self.transform.forward; f.y = 0f;
        if (f.sqrMagnitude < 1e-4f) f = Vector3.forward;
        // (2026-11-13, ronda 90) DE LADO, NO DE VUELTA: con 90–270° el siguiente salto podía volver justo por donde vino, y con el vigilante de
        // viajes (más viajes dados por acabados) los lobos y el emperador iban y venían en un sitio pequeño («da vueltas», 4 en la ronda 90). Ahora
        // gira 60–120° a un lado u otro: prueba otra dirección sin deshacer el camino.
        float turn = UnityEngine.Random.Range(60f, 120f) * (UnityEngine.Random.value < 0.5f ? -1f : 1f);
        t.lastDir = (Quaternion.Euler(0f, turn, 0f) * f.normalized).normalized;
    }
    /// <summary>(Pruebas) Un paseo «explore» cuyo salto nunca vale (siempre su propio sitio): ¿descansa a propósito y cambia de dirección?</summary>
    public static bool StrollWithoutHopForTest(Animal a, out float turned)
    {
        turned = 0f;
        if (a == null) return false;
        Trip t = TripOf(a, "explore"); t.going = false; t.pauseUntil = 0f; t.lastDir = Vector3.right;
        Stroll(a, "explore", () => a.transform.position, 1f, 3f, 1e5f);   // (ronda 77: con 3 m, el salto de repuesto de PickHop sí encontraba sitio; a 100 km no hay)
        turned = Vector3.Angle(Vector3.right, t.lastDir);
        return RestingByChoice(a);
    }
    /// <summary>Viajes que se quedaron sin camino lejos de su meta (diagnóstico).</summary>
    public static int NoPathTrips;
    static int _noPathLogs;
    /// <summary>(Pruebas) Un viaje a <paramref name="goal"/> recién empezado cuyo agente se ha quedado sin camino: ¿Going lo sigue llevando?</summary>
    public static bool GoingWithoutPathForTest(Animal a, Vector3 goal)
    {
        if (a == null || a.nav == null || !a.nav.isOnNavMesh) return false;
        a.nav.ResetPath();
        var t = new Trip { key = "prueba", goal = goal, going = true, since = Time.time, bestAt = Time.time, best = float.MaxValue };
        return Going(a, t);
    }
    /// <summary>Diagnóstico: el viaje de un animal (deseo, si va, destino, pausas, llamadas).</summary>
    /// <summary>(2026-11-11) El estado del agente de un animal, para los avisos de «atascado»: camino, cálculo pendiente, estado, velocidad,
    /// sobre el NavMesh y distancia al destino.</summary>
    /// <summary>(2026-11-11) UN SOLO DUEÑO DEL CUERPO: al ganar <paramref name="newKey"/> (Volition), el viaje de otro deseo se acaba (sin
    /// tocar el agente: el nuevo pone su camino) y el hábito, si no es él quien gana, suelta al animal.</summary>
    public static void Handover(Animal a, string newKey)
    {
        if (a == null) return;
        if (_trips.TryGetValue(a, out Trip t) && t.going && t.key != newKey && !SameTrip(t.key, newKey)) { t.going = false; Outcome(a, t, false, $"otro deseo («{newKey}»)"); }
        if (newKey != "habit" && a.TryGetComponent(out Habit h) && h.Active) h.Release();
        // (2026-11-14, ronda 99) La cría que deja de esperar la toma vuelve a la prioridad de evitación normal: el lobo #75, ya explorando, seguía
        // con la 20 de esperar (los demás lo rodeaban como si estuviera quieto).
        if (newKey != "nurse") { _homeWait.Remove(a); if (a.nav != null && a.nav.isActiveAndEnabled && a.nav.avoidancePriority == WaitingPriority) a.nav.avoidancePriority = DefaultPriority; }
    }
    // (el deseo «habit» y el viaje del deseo se llaman igual; «explore» usa el viaje «explore»; «buscar pareja», «cortejo» y «rastro» son de «mate»; «recinto», de «explore»)
    static bool SameTrip(string tripKey, string desireKey) => tripKey == desireKey || (desireKey == "mate" && (tripKey == "buscar pareja" || tripKey == "cortejo" || tripKey == "rastro")) || (desireKey == "explore" && tripKey == "recinto");

    /// <summary>(Pruebas) Empieza un viaje <paramref name="key"/> a <paramref name="goal"/>.</summary>
    public static void StartTripForTest(Animal a, string key, Vector3 goal) { if (a == null) return; Trip t = TripOf(a, key); Walk(a, t, goal); }

    /// <summary>(2026-11-11) ¿Lleva un viaje de deseo en marcha (para NavTrace)?</summary>
    public static bool LiveTrip(Animal a, out string key) { key = null; if (a == null || !_trips.TryGetValue(a, out Trip t) || !t.going) return false; key = t.key; return true; }

    public static string NavInfo(Animal a)
    {
        NavMeshAgent n = a != null ? a.nav : null;
        if (n == null) return "sin agente";
        if (!n.isActiveAndEnabled) return "agente apagado";
        // (2026-11-11) Y lo que puede tenerlo quieto con camino: el agente desfasado del cuerpo, la evitación, un enlace, updatePosition, el agente
        // más cercano y la distancia al borde del NavMesh (un borde tallado lo frena).
        Vector3 gap = n.nextPosition - n.transform.position; gap.y = 0f;
        float nearest = float.MaxValue;
        foreach (NavMeshAgent o in UnityEngine.Object.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None)) if (o != null && o != n && o.isActiveAndEnabled) nearest = Mathf.Min(nearest, Vector3.Distance(o.transform.position, n.transform.position));
        string edge = n.isOnNavMesh && NavMesh.FindClosestEdge(n.nextPosition, out NavMeshHit eh, n.areaMask) ? $"{eh.distance:0.00} m" : "—";
        CharacterLevel cl = a.GetComponent<CharacterLevel>();
        string more = $", energía {(cl != null ? $"{cl.currentEnergy:0}/{cl.MaxEnergy:0}" : "—")}, interno a {gap.magnitude:0.00} m del cuerpo, evitación {n.obstacleAvoidanceType} (prioridad {n.avoidancePriority}), en enlace {n.isOnOffMeshLink}, updatePosition {n.updatePosition}, agente más cercano a {(nearest < float.MaxValue ? nearest.ToString("0.0") : "—")} m, borde a {edge}";
        return $"agente: en NavMesh {n.isOnNavMesh}, camino {n.hasPath}, pendiente {n.pathPending}, estado {n.pathStatus}, velocidad {n.velocity.magnitude:0.00} (pedida {n.speed:0.0}){more}, destino a {Vector3.Distance(n.transform.position, n.destination):0.0} m, parado {n.isStopped} · {SteerInfo(n)}";
    }
    /// <summary>(2026-11-13, vivo75) Lo que el agente QUIERE hacer (velocidad deseada, rumbo, esquinas, camino caducado, aceleración, frenada):
    /// con camino y velocidad pedida, si la deseada también es 0 el problema es el camino o el rumbo; si no, lo frena otra cosa (evitación, cuerpo).</summary>
    static string SteerInfo(NavMeshAgent n)
    {
        if (n == null || !n.isOnNavMesh) return "sin rumbo";
        Vector3 st = n.steeringTarget - n.transform.position; st.y = 0f;
        Rigidbody rb = n.GetComponent<Rigidbody>();
        string body = rb == null ? "sin Rigidbody" : rb.isKinematic ? "cinemático" : $"físico a {rb.linearVelocity.magnitude:0.0} m/s";
        return $"deseada {n.desiredVelocity.magnitude:0.00}, rumbo a {st.magnitude:0.00} m, esquinas {(n.hasPath ? n.path.corners.Length : 0)}, caducado {n.isPathStale}, aceleración {n.acceleration:0.0}, frenada {n.stoppingDistance:0.0} m (autoBraking {n.autoBraking}), queda {n.remainingDistance:0.0} m, altura interna {n.nextPosition.y - n.transform.position.y:0.00} m, cuerpo {body}";
    }

    public static string TripInfo(Animal a) => (a != null && _trips.TryGetValue(a, out Trip t) ? $"viaje «{t.key}» va={t.going} meta={t.goal:F1} pausa={t.pauseUntil - Time.time:0.0} fallo={t.failUntil - Time.time:0.0} último final={(t.endWhy != null ? $"«{t.endWhy}» hace {Time.time - t.endAt:0.0} s" : "—")} llamadas={t.calls} (hace {Time.time - t.lastCall:0.0} s) empezados={t.started}" : "sin viaje") + StateInfo(a);
    /// <summary>(2026-11-14, ronda 99) Lo que puede tener a un animal sin que su deseo se despache: dormido, ocupado, alerta, peleando, y cuántas
    /// decisiones de su volición se saltaron por eso (el lobo #75 y el oso #61 «sin llamar» 19 y 149 s junto a la guarida).</summary>
    public static string StateInfo(Animal a)
    {
        if (a == null) return "";
        Volition v = a.GetComponent<Volition>();
        return $" · estado: dormido {a.asleep}, ocupado {a.busy}, alerta {a.aware}, peleando {a.fighting}" + (v != null ? $", decisiones {v.Choices} (saltadas: dormido {v.SkippedAsleep}, ocupado {v.SkippedBusy}, alerta {v.SkippedAware})" : "");
    }
    static readonly Dictionary<Animal, Trip> _trips = new Dictionary<Animal, Trip>();
    // (2026-11-13, ronda 88) EL VIGILANTE DE VIAJES. Going (quieto con camino, atascado, sin camino, llegó) solo corría cuando el deseo se volvía
    // a despachar, y eso pasaba cada 10–15 s (ronda 88: 20 adultos «atascados > 20 s» con «deseada 3–5, velocidad 0», 14 llamadas en minutos,
    // y FrozenRestarts = 2 en toda la partida): un viaje parado esperaba ese rato sin que nadie lo viera. Ahora cada 0,5 s se pasa por los
    // viajes en marcha; el deseo, al volver, encuentra el viaje ya acabado y sigue (pausa o siguiente salto).
    static float _superviseAt;
    static readonly List<Animal> _superviseList = new List<Animal>();
    public static int Supervised;
    public static void SuperviseTrips() { if (Time.time < _superviseAt) return; _superviseAt = Time.time + 0.5f; SuperviseTripsNow(); }
    // (2026-11-14, ronda 103) AUTORRECUPERACIÓN. Rondas 101–103: 13 «atascados» con «último final» variado (llegó 7, sin camino 4, pendiente 2),
    // 2–5 por ronda entre más de 100 animales. Cinco de ellos (zorros #5 y #69, renos #42 y #76, oso #18) en el MISMO sitio, (−118, −123):
    // un oso en cortejo quieto allí y otros animales pegados a él (a 0,0–0,8 m). Agentes encajados unos en otros: con camino y «deseada 0».
    // Para lo que quede, una regla de rescate: el animal que quiere moverse (explorar, volver a casa, buscar pareja, seguir a su madre), que no
    // duerme, no come, no pelea y no se ha movido 0,5 m en ≥ 15 s y ≥ 8 fotogramas, se coloca en el punto libre del NavMesh más cercano
    // (a 3–6 m, sin nadie a < 1,2 m). Como mucho una vez cada 30 s, y lo dice («[Fauna] rescate …»), para no tapar fallos sistemáticos.
    public const float RescueStill = 12f, RescueEvery = 20f;
    /// <summary>(Diagnóstico) Por qué no se rescató al último que lo necesitaba (espera entre rescates, sin sitio libre).</summary>
    public static string RescueBlocked = "";
    public static int SelfRecoveries, RescueNoSpot;
    static int _rescueLogs;
    static readonly Dictionary<Animal, (Vector3 at, float since, int frame, float last)> _rescue = new Dictionary<Animal, (Vector3, float, int, float)>();
    /// <summary>(2026-11-14, ronda 105) ¿Lo rescató el vigilante hace menos de <paramref name="secs"/> s? (JornadaTest: el salto de 3–6 m del rescate no es
    /// «dar vueltas».)</summary>
    public static bool RescuedRecently(Animal a, float secs) => a != null && _rescue.TryGetValue(a, out var r) && r.last > 0f && Time.time - r.last < secs && SelfRecoveries > 0;
    static void SelfRecover(Animal a)
    {
        if (a.nav == null || !a.nav.isActiveAndEnabled || !a.nav.isOnNavMesh) return;
        Vector3 pos = a.transform.position;
        if (!_rescue.TryGetValue(a, out var r) || Flat(pos - r.at) > 0.5f) { _rescue[a] = (pos, Time.time, Time.frameCount, r.last); return; }
        string desire = a.TryGetComponent(out Volition v) ? v.LastDesire : "";
        // (ronda 105) Ni «nurse»: la madre que espera a 2,5–4 m a que su cría llegue está quieta a propósito (los rescates de la ronda 105 movían
        // 3 m cada 20 s a zorras con «nurse»).
        // (ronda 104) Más amplio: cualquiera que quiera algo que se hace andando (no descansar, incubar, la guarida, beber, un hábito ni la cría que
        // espera la toma en su sitio), salvo dormido, comiendo o peleando. Antes solo explore/homebound/mate/heel y sin «descanso a propósito»:
        // los lobos de la guarida y el zorro #86 quedaban fuera. Y 12 s y 5 fotogramas (con 8 fotogramas, a 2,5 s cada uno, la prueba los marcaba antes).
        bool idleDesire = desire == "" || desire == "nurse" || desire == "rest" || desire == "brood" || desire == "den" || desire == "incubate" || desire == "drink" || desire == "habit" || _homeWait.Contains(a);
        if (idleDesire || a.death || a.asleep || a.busy || a.fighting || (a.Forage != null && a.Forage.EatingNow)) { _rescue[a] = (pos, Time.time, Time.frameCount, r.last); return; }
        if (Time.time - r.since < RescueStill || Time.frameCount - r.frame < 5) return;
        if (Time.time - r.last < RescueEvery) { RescueBlocked = "espera"; return; }
        for (int ring = 0; ring < 2; ring++)
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = pos + Quaternion.Euler(0f, i * 45f + ring * 22.5f, 0f) * Vector3.forward * (ring == 0 ? 3f : 6f);
                if (!NavMesh.SamplePosition(p, out NavMeshHit hit, 2f, a.nav.areaMask) || CrowdedSpot(a, hit.position)) continue;
                SelfRecoveries++;
                string why = TripInfo(a);
                if (_rescueLogs++ < 40) Debug.Log($"[Fauna] rescate «{a.name}» ({desire}): {Time.time - r.since:0} s sin moverse en ({pos.x:0.0},{pos.z:0.0}) → ({hit.position.x:0.0},{hit.position.z:0.0}) · {why} · {NavInfo(a)}");
                a.nav.ResetPath(); a.nav.Warp(hit.position);
                if (_trips.TryGetValue(a, out Trip t)) { t.going = false; t.pauseUntil = 0f; t.failUntil = 0f; t.frozenSince = -1f; t.unfroze = false; }
                NavTrace.Mark(a, "rescate (punto libre cercano)", hit.position);
                _rescue[a] = (hit.position, Time.time, Time.frameCount, Time.time);
                return;
            }
        RescueBlocked = "sin sitio libre"; RescueNoSpot++;
        _rescue[a] = (pos, Time.time, Time.frameCount, Time.time);   // (sin sitio libre: lo reintenta en 20 s)
    }

    public static void SuperviseTripsNow()
    {
        _superviseList.Clear(); _superviseList.AddRange(_trips.Keys);
        foreach (Animal a in _superviseList)
        {
            if (a == null) { _trips.Remove(a); continue; }
            SelfRecover(a);
            if (!_trips.TryGetValue(a, out Trip t) || !t.going) continue;
            if (a.death || a.asleep || a.busy) continue;
            Supervised++;
            bool on = a.nav != null && a.nav.isActiveAndEnabled && a.nav.isOnNavMesh;
            if (!on) { t.going = false; Outcome(a, t, false, "sin agente"); continue; }
            // Si el viaje acaba aquí (llegó, se atascó), la pausa de mirar y olfatear que ponía el paseo al volver.
            if (!Going(a, t) && !t.going) { t.pauseUntil = Mathf.Max(t.pauseUntil, Time.time + UnityEngine.Random.Range(1f, 3f)); a.Loco?.Idle(1f); }
        }
    }

    // (2026-11-13, ronda 78) CÓMO ACABAN LOS PASEOS (explore y el recinto): cuándo, adónde y si llegó. Para JornadaTest: quien explora un sitio
    // pequeño con metas distintas que alcanza no «da vueltas» (Polluelo_1, el emperador #37 y el oso #45: caminos completos de 4–13 m, sin fallos).
    static readonly Dictionary<Animal, List<(float t, Vector3 goal, bool reached)>> _outcomes = new Dictionary<Animal, List<(float, Vector3, bool)>>();
    static void Outcome(Animal self, Trip t, bool reached, string why)
    {
        if (t != null) { t.endWhy = why; t.endAt = Time.time; }   // (ronda 102) cómo acabó el último viaje, para TripInfo
        if (self == null || t == null || (t.key != "explore" && t.key != "recinto")) return;
        if (!_outcomes.TryGetValue(self, out var l)) _outcomes[self] = l = new List<(float, Vector3, bool)>();
        l.Add((Time.time, t.goal, reached));
        if (l.Count > 24) l.RemoveAt(0);
    }
    /// <summary>(2026-11-13) ¿Explora de verdad? En los últimos <paramref name="window"/> s: al menos un paseo acabado, todos llegaron y ninguna meta
    /// repite otra (a &lt; 1 m). Un fallo o una meta repetida = sí puede estar dando vueltas.</summary>
    public static bool FreshExploring(IEnumerable<(float t, Vector3 goal, bool reached)> events, float now, float window)
    {
        var seen = new List<Vector3>();
        foreach (var e in events)
        {
            if (now - e.t > window) continue;
            if (!e.reached) return false;
            foreach (Vector3 s in seen) if (Flat(s - e.goal) < 1f) return false;
            seen.Add(e.goal);
        }
        return seen.Count > 0;
    }
    /// <summary>¿Este animal explora de verdad en los últimos <paramref name="window"/> s? (ver <see cref="FreshExploring"/>)</summary>
    public static bool ExploringFreshly(Animal a, float window = 20f) => a != null && _outcomes.TryGetValue(a, out var l) && FreshExploring(l, Time.time, window);
    static readonly Dictionary<Animal, float> _homeBlocked = new Dictionary<Animal, float>();
    static int _homeLogs;
    /// <summary>Contadores para el diagnóstico (viajes empezados, atascados, paseos sin sitio alcanzable).</summary>
    public static int TripsStarted, TripsStalled, StrollNoPick, Strolls;

    static int _noPickLogs, _stallLogs, _stillLogs;
    static readonly Dictionary<Animal, (Vector3 at, float since, float logged)> _still = new Dictionary<Animal, (Vector3, float, float)>();
    static Trip TripOf(Animal a, string key)
    {
        if (!_trips.TryGetValue(a, out Trip t)) _trips[a] = t = new Trip();
        if (t.key != key) { t.key = key; t.going = false; t.restNoSpot = false; t.pauseUntil = 0f; t.failUntil = 0f; }   // otro deseo: viaje nuevo
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
        if (nav.pathPending) { if (Past(t.since, t.sinceFrame, 3f)) { t.going = false; t.failUntil = Time.time + 4f; TripsStalled++; Outcome(self, t, false, "pendiente"); NavTrace.Reset(self, "deseo: camino pendiente > 3 s"); return false; } return true; }   // (camino que no llega a calcularse: tope)
        // Con camino y sin velocidad: una pausa (Idle, velocidad 0) de otra conducta, como «no alcanza comida; descansa», le dejaba quieto con
        // el viaje en marcha hasta darlo por atascado (osos, lobos, crías: «velocidad pedida 0,0 … sin WalkSpell»). Quien lleva el viaje, anda.
        if (nav.hasPath && nav.speed < 0.05f && !self.busy) self.Loco?.SetGait(false, 0.1f);
        // (2026-11-11) SIN CAMINO NO ES «LLEGÓ»: si el agente se queda sin camino (un SetPath descartado, un ResetPath de otra conducta) lejos de su
        // meta, el viaje vuelve a pedirlo; si en 3 s sigue sin él, se da por fallido (con su pausa) en vez de empezar otro cada segundo sin
        // moverse («[Fauna] atascado … empezados=136 · camino False … destino a 0,0 m», 333 líneas en vivo: renos, pingüinos, zorros, insectos).
        if (!nav.hasPath && Flat(t.goal - self.transform.position) > nav.stoppingDistance + 0.6f)
        {
            if (Past(t.since, t.sinceFrame, 3f))
            {
                t.going = false; t.failUntil = Time.time + 4f; TripsStalled++; Outcome(self, t, false, "sin camino"); NoPathTrips++;
                if (_noPathLogs < 30) { _noPathLogs++; Debug.Log($"[Fauna] «{self.name}» se queda sin camino hacia {t.goal} ({Flat(t.goal - self.transform.position):0.0} m): {NavInfo(self)} · {NavTrace.Last(self)}."); }
                return false;
            }
            if (Time.time >= t.reissueAt) { t.reissueAt = Time.time + 0.5f; self.Loco?.SetGait(false, 0.1f); SyncPath(self, nav, t.goal, "deseo: lo vuelve a pedir"); }
            return true;
        }
        float rem = nav.hasPath ? Remaining(nav) : 0f;
        if (rem <= nav.stoppingDistance + 0.35f) { t.going = false; Outcome(self, t, true, "llegó"); return false; }   // llegó
        // (2026-11-13, suite 80) CAMINO CADUCADO O SIN ESQUINAS: las 8 crías de la Cría quietas a la vez tenían «caducado True» (el NavMesh del árbol
        // que gira se vuelve a colocar cada paso y su camino deja de valer) y el zorro y el buey que volvían a casa desde 145–209 m, camino con
        // «esquinas 0» y «deseada» 3–15: ninguno andaba y el agente no lo recalculaba solo. Se le vuelve a pedir el camino (cada 0,5 s como mucho),
        // y eso no cuenta como estar quieto (el tope de 5 s sin acercarse sigue valiendo).
        bool repathed = false;
        // (ronda 85) Solo si no está ya calculando: pedirlo otra vez cada 0,5 s mientras estaba «pendiente» volvía a empezar el cálculo (reno #34).
        // (ronda 88) Sin la condición de «no anda» que se añadió entonces (20 atascados en la ronda 88).
        if (nav.hasPath && !nav.pathPending && Time.time >= t.reissueAt && (nav.isPathStale || nav.path.corners.Length == 0))
        {
            t.reissueAt = Time.time + 0.5f; t.frozenSince = -1f; StaleRepaths++;
            self.Loco?.SetGait(false, 0.1f);
            SyncPath(self, nav, t.goal, nav.isPathStale ? "deseo: camino caducado, lo vuelve a pedir" : "deseo: camino sin esquinas, lo vuelve a pedir");
            repathed = true;
        }
        // (2026-11-13, vivo75) QUIETO CON CAMINO: 17 «atascado > 20 s» (osos, bueyes, renos, zorros, lobos y las crías de la Cría) tenían camino
        // completo, velocidad pedida 2–5, sin parar, el agente pegado al cuerpo… y velocidad 0,00, viaje tras viaje (cada uno esperaba 5 s a darse
        // por atascado y el siguiente salía igual de quieto). A los 1,5 s se reinicia el agente donde está (Warp) y se le vuelve a dar el camino;
        // si sigue quieto, se deja el viaje y el siguiente sale hacia otro lado. El aviso dice qué quería hacer el agente (velocidad deseada, rumbo).
        // (ronda 96) Se mueve si el CUERPO se ha movido > 0,2 m desde la última vez que se le vio quieto (la velocidad del agente engaña con fotogramas lentos).
        float bodyMoved = t.frozenSince < 0f ? 0f : Flat(self.transform.position - t.frozenPos);
        int fz = FrozenStep(t, !repathed && nav.hasPath && nav.speed > 0.05f && !nav.isStopped && !self.busy, bodyMoved > 0.2f ? 1f : 0f, Time.time, Time.frameCount);
        if (t.frozenSince == Time.time) t.frozenPos = self.transform.position;
        if (fz > 0)
        {
            if (_frozenLogs < 40) { _frozenLogs++; Debug.Log($"[Fauna] «{self.name}» ({t.key}) quieto con camino {FrozenSeconds:0.0} s ({(fz == 1 ? "vuelve a andar" : "deja el viaje y prueba otro lado")}): {NavInfo(self)} · {NavTrace.Last(self)}."); }
            if (fz == 1)
            {
                FrozenProbe.Attach(self, $"quieto con camino {FrozenSeconds:0.0} s en «{t.key}»");   // (rondas 94–95) sonda de los primeros
                // (2026-11-13, ronda 77) Sin Warp ni camino nuevo (86 reinicios y más atascados, 17 → 34): solo se le devuelve el paso de andar.
                FrozenRestarts++; t.bestAt = Time.time;
                self.Loco?.SetGait(false, 0.1f);
                return true;
            }
            // (2026-11-13, ronda 78) PEGADO AL BORDE: Osezno_1 y Polluelo_2 (recinto de la Cría) seguían quietos con camino y el agente a 0,00–0,16 m
            // del borde del NavMesh («deseada 0,00» con 2 esquinas a 1,9–2,7 m), viaje tras viaje. A la segunda vez quieto, si está pegado al
            // borde, se le mete 0,6 m hacia dentro (una vez por viaje: no es el reinicio de cada 1,5 s de la ronda 77).
            if (InwardFromEdge(nav, out Vector3 inside)) { EdgeNudges++; NavTrace.Mark(self, "deseo: pegado al borde (0,6 m hacia dentro)", inside); nav.Warp(inside); }
            // (ronda 92) Y tras ≥ 3 s quieto con camino, el agente se reinicia una vez donde está (no cada 1,5 s como en la ronda 77): la velocidad
            // deseada 3–5 con velocidad 0 que no se arregla sola.
            else if (Time.time >= t.unfrozeWarpAt) { t.unfrozeWarpAt = Time.time + 15f; FrozenWarps++; nav.ResetPath(); nav.Warp(self.transform.position); }
            t.unfroze = false;
            t.going = false; t.failUntil = Time.time + 4f; TripsStalled++; Outcome(self, t, false, "quieto con camino"); FrozenTrips++; TurnAway(self, t);
            NavTrace.Reset(self, "deseo: quieto con camino");
            return false;
        }
        if (rem < t.best - 0.25f) { t.best = rem; t.bestAt = Time.time; t.bestFrame = Time.frameCount; }
        if (Past(t.bestAt, t.bestFrame, 5f) || Past(t.since, t.sinceFrame, 40f))                    // atascado o tope
        {
            if (_stallLogs < 30) { _stallLogs++; Debug.Log($"[Fauna] «{self.name}» se atasca yendo a {t.goal} desde {self.transform.position}: quedan {rem:0.0} m, velocidad pedida {nav.speed:0.0}, real {nav.velocity.magnitude:0.0}, parado {nav.isStopped}, camino {nav.pathStatus}{(nav.speed < 0.05f ? $", sin velocidad: {(self.Walk != null && self.Walk.IsCharging ? "en postura de carga" : self.TryGetComponent(out CharacterLevel cl) ? $"energía {cl.currentEnergy:0.0}/{cl.MaxEnergy:0.0}" : "sin WalkSpell ni energía")}" : "")}."); }
            t.going = false; t.failUntil = Time.time + 8f; TripsStalled++; Outcome(self, t, false, "atascado o tope"); TurnAway(self, t);   // (2026-11-13) el siguiente, hacia otro lado
            NavTrace.Reset(self, "deseo: atascado o tope");
            return false;
        }
        return true;
    }

    // Lo que le queda por el camino. remainingDistance es Infinito cuando el camino tiene más esquinas de las que el agente ya calculó: con
    // eso nunca «mejoraba» ni «llegaba», y a los 5 s el viaje se daba por atascado aunque fuera a toda velocidad (ronda 22: liebres, renos,
    // pingüinos y bueyes «quedan Infinito m … real 5,0»). Entonces se suma el camino de esquina en esquina.
    static float Remaining(NavMeshAgent nav)
    {
        float r = nav.remainingDistance;
        if (!float.IsInfinity(r) && !float.IsNaN(r)) return r;
        Vector3[] c = nav.path.corners;
        if (c == null || c.Length == 0) return Vector3.Distance(nav.transform.position, nav.destination);
        float sum = Vector3.Distance(nav.transform.position, c[Mathf.Min(1, c.Length - 1)]);
        for (int i = 1; i < c.Length - 1; i++) sum += Vector3.Distance(c[i], c[i + 1]);
        return sum;
    }

    static void Walk(Animal self, Trip t, Vector3 at)
    {
        TripsStarted++; t.started++; Vector3 dir = at - self.transform.position; dir.y = 0f; if (dir.sqrMagnitude > 1f) t.lastDir = dir.normalized; t.goal = at; t.since = t.bestAt = Time.time; t.sinceFrame = t.bestFrame = Time.frameCount; t.best = float.MaxValue;
        // (ronda 92) El reloj de «quieto con camino» NO se pone a cero con cada viaje: los animales que empezaban un salto tras otro sin moverse
        // nunca llegaban a 1,5 s en el mismo viaje (FrozenTrips = 0 con decenas de animales a velocidad 0). FrozenStep lo pone a cero al andar.
        t.restNoSpot = false;
        // (2026-11-11, ronda 59) Un camino que no lleva (parcial, de una esquina, o que acaba a < 1 m de donde empieza: otra isla del NavMesh)
        // no es un viaje: el agente «llegaba» al momento, se quedaba sin camino y el paseo esperaba. Se da por fallido ya, con su pausa.
        t.going = Move(self, at);
        if (!t.going) { t.failUntil = Time.time + 4f; TripsStalled++; Outcome(self, t, false, "camino que no lleva"); BadPaths++; }
    }
    /// <summary>Viajes que no se empezaron porque el camino no llevaba a ninguna parte (diagnóstico).</summary>
    public static int BadPaths;

    // Ir andando: destino Y paso de andar (la pausa anterior lo dejó en «quieto», velocidad 0: con solo SetDestination el
    // agente tenía destino pero no andaba). true = lleva camino de verdad.
    /// <summary>(2026-11-14, ronda 104) Volver a pedir el camino a <paramref name="to"/> AL MOMENTO (CalculatePath + SetPath). Lo que quedaba
    /// con SetDestination (el viaje sin camino y el camino caducado) se calculaba en segundo plano y daba los «pendiente» que quedaban.</summary>
    static bool SyncPath(Animal self, NavMeshAgent nav, Vector3 to, string why)
    {
        NavMeshPath p = new NavMeshPath();
        NavTrace.Mark(self, why, to);
        return nav.CalculatePath(to, p) && p.status == NavMeshPathStatus.PathComplete && p.corners.Length >= 2 && nav.SetPath(p);
    }

    /// <summary>(2026-11-14) Para la madre y la cría (NurseCub, AwaitFeeding): ir al sitio y, si no hay camino completo hasta él (b2fe17d: Move ya
    /// no lo pide en segundo plano), al punto más cercano al que sí se llega (el final del camino parcial), calculado al momento (SyncPath).
    /// Así no se quedan quietas ante un sitio al que no se llega del todo (la guarida, la madre sobre algo).</summary>
    public static int NearestFallbacks;
    static bool MoveOrNearest(Animal self, Vector3 to)
    {
        if (Move(self, to)) return true;
        NavMeshAgent nav = self.nav;
        if (nav == null || !nav.isOnNavMesh) return false;
        if (!NavReach.PartialEnd(self, to, out Vector3 end) || Flat(end - self.transform.position) < 0.5f) return false;
        NearestFallbacks++;
        self.Loco?.SetGait(false, 1f);
        return SyncPath(self, nav, end, "deseo: al punto más cercano (camino parcial)");
    }

    static bool Move(Animal self, Vector3 to)
    {
        NavMeshAgent nav = self.nav;
        if (nav == null || !nav.isOnNavMesh) return false;
        // (2026-11-11) EL AGENTE CON EL CUERPO: si su posición interna se ha quedado lejos del cuerpo (lo movió otra cosa sin Warp), el camino
        // sale de allí y el agente «llega» enseguida sin moverse (lo mismo que le pasaba a la gente del microcosmos, MicroMove).
        Vector3 gap = nav.nextPosition - self.transform.position; gap.y = 0f;
        if (gap.sqrMagnitude > 0.25f) { NavTrace.Mark(self, $"deseo: agente desfasado {gap.magnitude:0.0} m (Warp)", self.transform.position); nav.Warp(self.transform.position); }
        self.Loco?.SetGait(false, 1f);
        // Con el camino YA calculado (NavReach acaba de comprobar que se llega): SetPath, sin esperar a que el agente lo pida y lo calcule otra
        // vez (con cientos de obstáculos tallando, SetDestination podía quedarse «pendiente» y el animal quieto).
        if (NavReach.LastPathTo(self, to, out NavMeshPath p))
        {
            int n = p.corners.Length; float span = n > 1 ? Flat(p.corners[n - 1] - p.corners[0]) : 0f;
            NavTrace.Mark(self, $"deseo: SetPath ({p.status}, {n} esquinas, {span:0.0} m)", to);
            if (p.status != NavMeshPathStatus.PathComplete || n < 2 || span < 0.5f) return false;   // (0,5 m: los pasitos de las crías del recinto son de 0,6–2,2 m)
            if (nav.SetPath(p) && (nav.hasPath || nav.pathPending)) return true;
        }
        // (2026-11-14, ronda 102) Sin camino ya calculado, se calcula AQUÍ (CalculatePath, al momento) y se pone con SetPath. SetDestination lo
        // pide en segundo plano: con cientos de agentes quedaba «pendiente» > 3 s y el viaje fallaba (zorro #5: «último final=«pendiente»», y
        // otra vez «pendiente True» en el viaje siguiente).
        NavMeshPath own = new NavMeshPath();
        if (nav.CalculatePath(to, own) && own.status == NavMeshPathStatus.PathComplete && own.corners.Length >= 2)
        {
            NavTrace.Mark(self, $"deseo: SetPath calculado ({own.corners.Length} esquinas)", to);
            if (nav.SetPath(own) && nav.hasPath) return true;
        }
        // (ronda 104) Y si tampoco hay camino completo, no se pide en segundo plano (SetDestination quedaba «pendiente» y el viaje fallaba igual,
        // más tarde: «último final=«pendiente»» en 8 de 16 rescates, zorro #86 «pendiente True, PathInvalid»): el viaje no empieza y lo dice.
        NavTrace.Mark(self, "deseo: sin camino completo (no se pide)", to);
        return false;
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
        // (2026-11-11) DIAGNÓSTICO EN VIVO: un paseo (explore, el recinto) que en 20 s no le ha movido medio metro lo dice, una vez por minuto y
        // animal, con su viaje y su agente (rondas 50–51: renos, zorros, orcas y las crías de la Cría «quieren explore y no se mueven»).
        {
            Vector3 pos = self.transform.position;
            if (!_still.TryGetValue(self, out var st) || Flat(pos - st.at) > 0.5f) _still[self] = st = (pos, Time.time, st.logged);
            else if (Time.time - st.since >= 20f && Time.time >= st.logged && _stillLogs < 200)
            {
                _stillLogs++; _still[self] = (st.at, st.since, Time.time + 60f);
                Debug.Log($"[Fauna] atascado «{self.name}» ({key}) {Time.time - st.since:0} s sin moverse en ({pos.x:0.0},{pos.y:0.0},{pos.z:0.0}): {TripInfo(self)} · {NavInfo(self)}");
            }
        }
        if (Going(self, t)) return;
        if (was) { t.pauseUntil = Time.time + UnityEngine.Random.Range(pauseMin, pauseMax); self.Loco?.Idle(t.pauseUntil - Time.time); }
        if (Time.time < t.pauseUntil) return;
        Vector3 here = self.transform.position;
        if (PickHop(self, pick, minHop, out Vector3 hop)) { Walk(self, t, hop); return; }
        // Nada alcanzable ahora: se para y mira (descanso, como la cría del recinto sin sitio: RestingByChoice) y lo reintenta en un momento
        // hacia OTRO lado (2026-11-13, vivo75: el siguiente salto volvía a buscar contra lo mismo).
        StrollNoPick++; t.pauseUntil = Time.time + 1f; t.restNoSpot = true; TurnAway(self, t);
        if (_noPickLogs < 30 && self.nav != null) { _noPickLogs++; Debug.Log($"[Fauna] «{self.name}» ({key}) no encuentra adónde ir en {here}: agente tipo {self.nav.agentTypeID}, área {self.nav.areaMask}, nextPosition a {(self.nav.nextPosition - here).magnitude:0.00} m, velocidad {self.nav.speed:0.0}, en NavMesh {self.nav.isOnNavMesh}."); }
    }

    /// <summary>(2026-11-11, ronda 72) EL SALTO DE UN PASEO: a ≥ <paramref name="minHop"/> (nunca menos de 1,5 m), con camino completo y sin rodeo.
    /// Antes, si los saltos largos no valían, se aceptaba uno «corto» desde 0,8 m: la meta quedaba a 0,5–2 m («EmperorPenguin (explore) … meta en
    /// su sitio», un oso a 1,6 m, un pingüino a 1,8 m) y el viaje «llegaba» al momento, una y otra vez. Ahora el corto también respeta el mínimo;
    /// si nada vale, no hay salto (pausa y se reintenta).</summary>
    /// <summary>(2026-11-14, ronda 98) ¿El salto de <paramref name="from"/> a <paramref name="to"/> acaba o pasa a menos de <see cref="CubClearance"/> m de
    /// una cría que espera la toma? Esas no se apartan (prioridad de evitación 20): el adulto que exploraba por la guarida se quedaba sin avanzar
    /// contra ellas, se «atascaba», giraba y volvía a intentarlo (lobo #87: 44–62 m en 20 s, «deseo: atascado o tope»).</summary>
    public const float CubClearance = 2.5f;
    static bool ThroughWaitingCubs(Animal self, Vector3 from, Vector3 to)
    {
        foreach (Animal c in _homeWait)
        {
            if (c == null || c == self) continue;
            if (PassesNear(from, to, c.transform.position)) return true;
        }
        return false;
    }
    /// <summary>¿Pasa el tramo a menos de <see cref="CubClearance"/> m del punto <paramref name="cub"/>?</summary>
    public static bool PassesNear(Vector3 from, Vector3 to, Vector3 cub)
    {
        Vector3 d = to - from; d.y = 0f; Vector3 w = cub - from; w.y = 0f;
        float k = d.sqrMagnitude > 1e-4f ? Mathf.Clamp01(Vector3.Dot(w, d) / d.sqrMagnitude) : 0f;
        return (w - d * k).magnitude < CubClearance;
    }

    // (ronda 102) Para todos los paseos, no solo las crías: los pingüinos #19 y #42 de la colonia saltaban a sitios con otro pingüino encima,
    // se atascaban contra él, giraban y volvían a probar («da vueltas» 31–33 m en 20 s, «deseo: atascado o tope»).
    /// <summary>(2026-11-14, ronda 101) ¿Hay otro animal a menos de 1,2 m de <paramref name="at"/>? Las crías de la guarida (lobos #10, #79) daban
    /// pasitos de 1,6–1,9 m que acababan encima de otra cría (a 0,8–1,5 m) y su agente no avanzaba («deseada 0,00»).</summary>
    static bool CrowdedSpot(Animal self, Vector3 at)
    {
        foreach (GameObject g in Animal.wholePopulation)
        {
            if (g == null || g == self.gameObject) continue;
            Vector3 d = g.transform.position - at; d.y = 0f;
            if (d.sqrMagnitude < 1.44f) return true;
        }
        return false;
    }

    static bool PickHop(Animal self, Func<Vector3> pick, float minHop, out Vector3 at)
    {
        at = self.transform.position; minHop = Mathf.Max(minHop, 1.5f);
        Vector3 here = self.transform.position;
        for (int i = 0; i < 8; i++)
        {
            Vector3 p = pick();
            if (Flat(p - here) < minHop) continue;
            if (NavReach.Reachable(self, p, out at) && Flat(at - here) >= minHop && !Detour(self, at, here) && !ThroughWaitingCubs(self, here, at) && !CrowdedSpot(self, at)) return true;
        }
        // Al borde de una colonia sobre el hielo o en un recinto pequeño, los saltos largos caen en el agua o tras la valla: saltos más cortos, pero
        // no por debajo del mínimo.
        for (int i = 0; i < 8; i++)
        {
            Vector2 r = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minHop, Mathf.Max(minHop * 1.5f, 3f));
            Vector3 p = here + new Vector3(r.x, 0f, r.y);
            if (NavReach.Reachable(self, p, out at) && Flat(at - here) >= minHop && !Detour(self, at, here) && !ThroughWaitingCubs(self, here, at) && !CrowdedSpot(self, at)) return true;
        }
        at = here; return false;
    }

    /// <summary>(Pruebas) La meta que elegiría el paseo «explore» de un adulto: a ≥ 3 m y con camino completo.</summary>
    public static bool ExploreGoalForTest(Animal a, out Vector3 at)
    {
        at = a != null ? a.transform.position : Vector3.zero;
        if (a == null || a.nav == null || !a.nav.isOnNavMesh) return false;
        float step = Mathf.Clamp(a.HomeRadius * 0.08f, 4f, 10f); Transform tr = a.transform;
        return PickHop(a, () => LifeStage.WanderTarget(tr.position, tr.forward, step, UnityEngine.Random.insideUnitCircle), 3f, out at);
    }

    /// <summary>(2026-11-14, ronda 105) ¿El camino a <paramref name="to"/> es mucho más largo que la recta (&gt; 2× + 5 m)? (a cualquier distancia; Detour
    /// solo mira los paseos cortos)</summary>
    static bool LongWay(Animal self, Vector3 to)
    {
        if (!NavReach.Reachable(self, to, out Vector3 at, 3f) || !NavReach.LastPathTo(self, at, out NavMeshPath p) || p.corners.Length < 3) return false;
        float len = 0f;
        for (int i = 1; i < p.corners.Length; i++) len += Vector3.Distance(p.corners[i - 1], p.corners[i]);
        return len > 2f * Flat(at - self.transform.position) + 5f;
    }

    // ¿El camino hasta ese sitio da un RODEO (más del doble de la distancia recta + 1,5 m)? En el recinto de la Cría (la guarida,
    // el comedero, el muro) un sitio a 2 m se alcanzaba por un camino de 5–6 m alrededor de la guarida: el polluelo y el
    // cachorro iban y venían 15–17 m en 20 s para 1 m netos («da vueltas en círculo», en vivo). Un paseo es a un sitio que se ve.
    static bool Detour(Animal self, Vector3 at, Vector3 here)
    {
        if (Flat(at - here) >= 4f) return false;   // (solo los paseos cortos: el recinto, la cría; el adulto explora a 4–10 m)
        if (!NavReach.LastPathTo(self, at, out NavMeshPath p) || p.corners.Length < 3) return false;
        float len = 0f;
        for (int i = 1; i < p.corners.Length; i++) len += Vector3.Distance(p.corners[i - 1], p.corners[i]);
        return len > 2f * Flat(at - here) + 1.5f;
    }

    // Explorar: saltos de 4–10 m (según su territorio) sesgados hacia delante, con pausas cortas.
    static void Explore(Animal self, string key)
    {
        // LA CRÍA no se aleja: curiosea a 1–2,5 m de donde está, con pausas largas para mirar y olfatear (los polluelos de
        // pingüino en su guardería, los cachorros junto a la guarida). Con los saltos de 4–10 m de un adulto y su colonia o su
        // recinto alrededor, casi todo «hacia delante» quedaba fuera y solo se alcanzaba volver: iba y venía 15–17 m en 20 s
        // para 0,5–2 m netos («da vueltas en círculo», Polluelo_1/2/3 y Cachorro_2/3, JornadaTest 2026-11-01).
        if (self.lifeStage == LifeStage.child)
        {
            Transform ct = self.transform;
            Stroll(self, key, () => ct.position + Ring(1.5f, 3f), 4f, 8f, 1.5f);   // (ronda 72: ≥ 1,5 m)
            return;
        }
        float step = Mathf.Clamp(self.HomeRadius * 0.08f, 4f, 10f);
        Transform tr = self.transform;
        // (2026-11-11, vivo74) «HACIA DELANTE» ES HACIA DONDE IBA (el último salto), no hacia donde mira el cuerpo: al llegar se paraba girado y el
        // siguiente salto volvía por donde vino (zorros, bueyes, pingüinos: saltos de 11–13 m, 17–43 m en 20 s para 0–1,9 m netos).
        Trip et = TripOf(self, key);
        Stroll(self, key, () => LifeStage.WanderTarget(tr.position, et.lastDir.sqrMagnitude > 0.5f ? et.lastDir : tr.forward, step, UnityEngine.Random.insideUnitCircle), 1f, 3f, 3f);   // (ronda 72: ≥ 3 m)
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

    /// <summary>Largo de cada tramo del camino a casa (m).</summary>
    public const float HomeLeg = 35f;
    /// <summary>Tramos de vuelta a casa empezados (diagnóstico).</summary>
    public static int HomeLegs, StraightLegs;
    /// <summary>(2026-11-13) El punto a <paramref name="d"/> m a lo largo de una polilínea (false si es más corta).</summary>
    public static bool PointAlong(Vector3[] corners, float d, out Vector3 at)
    {
        at = Vector3.zero;
        if (corners == null || corners.Length < 2) return false;
        for (int i = 0; i < corners.Length - 1; i++)
        {
            float seg = Vector3.Distance(corners[i], corners[i + 1]);
            if (seg >= d) { at = Vector3.Lerp(corners[i], corners[i + 1], d / Mathf.Max(seg, 1e-4f)); return true; }
            d -= seg;
        }
        return false;
    }

    // Vuelve a SU sitio de la zona de casa (cada uno el suyo: no todos al mismo punto). Si no hay camino completo, un trecho
    // hacia allí si el NavMesh lo permite; si tampoco, «casa inalcanzable desde aquí» y deja de sentirlo 90 s.
    static void GoHome(Animal self)
    {
        if (self.nav == null || !self.nav.isOnNavMesh) return;
        // Ya en casa: Volition re-despacha el deseo cada segundo, y estando encima del sitio GoTo decía «no» y salía «no puede volver a casa» a 2 m de ella.
        if (HomeNeed(self) <= 0f) return;
        float a = (self.GetHashCode() % 360) * Mathf.Deg2Rad;
        Vector3 spot = self.HomeOrigin + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * HomeZone(self) * 0.5f;
        // (2026-11-13, ronda 78) CASA LEJOS, POR TRAMOS: un zorro a 187 m y un reno a 140 m de casa no llegaban a moverse (camino con 0 esquinas y
        // velocidad 0, o «pendiente» > 3 s: 56–132 viajes fallidos). Más allá de HomeLeg m va a un punto de su camino a HomeLeg m y, al llegar,
        // al siguiente. Un tramo (o el trecho de un camino parcial) en marcha se sigue hasta el final, sin re-apuntar cada segundo.
        Trip t = TripOf(self, "homebound");
        if (t.going && Flat(t.goal - spot) > 1.5f) { if (Going(self, t)) return; if (Time.time < t.failUntil) return; }
        if (Flat(spot - self.transform.position) > HomeLeg && Time.time >= t.failUntil && NavReach.Reachable(self, spot, out Vector3 full)
            && NavReach.LastPathTo(self, full, out NavMeshPath fp) && PointAlong(fp.corners, HomeLeg, out Vector3 leg) && NavReach.Reachable(self, leg, out Vector3 legAt))
        { HomeLegs++; Walk(self, t, legAt); return; }
        // (2026-11-13, ronda 85) Y LEJOS, NUNCA DE UNA VEZ: si el tramo por el camino no sale (camino que no vale, cálculo lento), un tramo en LÍNEA
        // RECTA hacia casa (o algo desviado) a HomeLeg m. Antes se caía en GoTo(casa entera): SetDestination a 150 m quedaba «pendiente» > 3 s y
        // fallaba, una vez y otra (zorro #38: «camino False, pendiente True … destino a 154 m», 116 viajes; zorros y bueyes atascados).
        if (Flat(spot - self.transform.position) > HomeLeg)
        {
            if (Time.time < t.failUntil) return;
            Vector3 dir = spot - self.transform.position; dir.y = 0f; dir.Normalize();
            foreach (float ang in new[] { 0f, 25f, -25f, 50f, -50f })
            {
                Vector3 p = self.transform.position + Quaternion.Euler(0f, ang, 0f) * dir * HomeLeg;
                if (NavReach.Reachable(self, p, out Vector3 sAt, 8f) && Flat(sAt - self.transform.position) > 10f) { HomeLegs++; StraightLegs++; Walk(self, t, sAt); return; }
            }
            _homeBlocked[self] = Time.time + 90f;
            if (_homeLogs++ < 20) Debug.Log($"[Fauna] «{self.name}» no encuentra tramo hacia casa desde ({self.transform.position.x:0},{self.transform.position.z:0}) (hogar en ({self.HomeOrigin.x:0},{self.HomeOrigin.z:0}), a {Flat(spot - self.transform.position):0} m): lo deja estar un rato.");
            return;
        }
        if (GoTo(self, "homebound", spot)) return;
        if (Time.time < t.failUntil) return;   // (se acaba de atascar: lo reintenta; no es que no haya camino)
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
    const float W_DRINK   = 0.8f;    // beber (× sed 1–1,5): como un hambre media; el hambre fuerte y la amenaza la superan
    const float W_DEN     = 1.2f;    // guarida de la perra: por encima del hambre media (sale si tiene MUCHA hambre y vuelve)
    const float W_DEN_FAST = 2.5f;   // guarida de la osa: no sale (ayuna) hasta que las crías tengan ~3 meses
    const float W_INCUBATE = 2.5f;   // incubar: el emperador no deja el huevo en 2 meses (ayuna)
    const float W_HEEL    = 0.7f;    // la cría sigue a su madre: por encima de explorar/volver a casa/hábitos

    // Vínculo con las crías propias aún pequeñas (0..1): el más fuerte. 0 si no tiene.
    static float BroodTie(Animal self)
    {
        float best = 0f;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || Gone(b.target)) continue;
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
            if (b == null || b.value <= 0f || Gone(b.target)) continue;
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
        // A su lado se para: seguía re-apuntando a la cría, que a su vez la seguía a ella («heel»), y daban vueltas juntas (lobo: 25 m en 20 s para 0 netos).
        // (2026-11-11, vivo62) CERCA, LA MADRE ESPERA y la cría va a ella: a ≤ 4 m las dos iban a donde estaba la otra y se cambiaban el sitio
        // (lobos «nurse» 16–17 m en 20 s para 0–1,9 m netos, con MeetPoint). Lejos, a medio camino.
        float dc = Vector3.Distance(self.transform.position, cub.transform.position);
        // (2026-11-13, vivo75) …pero solo si la cría VIENE (se mueve): la que espera la toma quieta en casa (sin vínculo hacia la madre, AwaitFeeding
        // sin madre) no venía nunca, y la madre la esperaba a 2,5–4 m para siempre. Con la cría quieta, la madre va hasta ella.
        bool cubComing = cub.nav != null && cub.nav.isActiveAndEnabled && cub.nav.velocity.magnitude > 0.2f;
        if (dc > 2.5f && dc <= 4f && !cubComing) { if (self.nav != null && self.nav.isOnNavMesh) MoveOrNearest(self, cub.transform.position); return; }
        if (dc > 2.5f && dc <= 4f) { if (self.nav != null && self.nav.isOnNavMesh) NavTrace.Reset(self, "nurse (espera a la cría)"); self.Loco?.Idle(1f); return; }
        if (dc <= 2.5f) { if (self.nav != null && self.nav.isOnNavMesh) NavTrace.Reset(self, "nurse (junto a la cría)"); FeedCub(self, cub); return; }
        // (2026-11-14, ronda 105) UN DESTINO QUE NO CAMBIA A CADA SEGUNDO: con la cría al otro lado de algo (camino de 89,8 m para unos pocos en línea
        // recta), el punto de encuentro a medio camino caía cada segundo en otro sitio y el camino cambiaba de lado del obstáculo: la madre (zorro
        // #57, «nurse») iba y venía 174 m en 20 s para 1,2 m netos. Con rodeo, va a la cría misma (que espera quieta), y no se vuelve a pedir
        // el camino mientras el que lleva acabe a < 2 m del destino nuevo.
        if (self.nav != null && self.nav.isOnNavMesh)
        {
            Vector3 meet = MeetPoint(self.transform.position, cub.transform.position);
            if (LongWay(self, meet)) meet = cub.transform.position;
            if (self.nav.hasPath && Flat(self.nav.destination - meet) < 2f) return;
            MoveOrNearest(self, meet);
        }
    }

    /// <summary>(2026-11-11) DONDE SE ENCUENTRAN la madre que va a dar la toma y la cría que la busca: lejos, a MEDIO CAMINO; cerca (≤ 4 m), donde
    /// está el otro. Antes cada uno iba a donde estaba el otro al decidir: se cruzaban, quedaban otra vez a la misma distancia y volvían a
    /// cambiarse el sitio (lobos «da vueltas en círculo (nurse)», 18–22 m en 20 s, rondas 50 y 51).</summary>
    static Vector3 MeetPoint(Vector3 me, Vector3 other) => Vector3.Distance(me, other) > 4f ? Vector3.Lerp(me, other, 0.5f) : other;

    /// <summary>Cada toma: (madre o padre, cría). Para las escenas de ciclo de vida (la crianza vista, no supuesta).</summary>
    public static event Action<Animal, Animal> CubFed;

    /// <summary>Una toma: AMAMANTA (mamíferos) o REGURGITA (pingüino) — baja el hambre de la cría; cuidar cansa un poco.</summary>
    public static void FeedCub(Animal self, Animal cub)
    {
        cub.hungry = Mathf.Max(0f, cub.hungry - 0.3f);
        self.exhaustion += 0.5f;
        CubFed?.Invoke(self, cub);
    }

    // En la guarida: amamanta a las crías vinculadas que tenga al lado y tengan hambre.
    static void FeedCubsNear(Animal self, float within)
    {
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || Gone(b.target)) continue;
            Animal cub = (b.target as Component)?.GetComponentInParent<Animal>();
            if (cub == null || cub == self || cub.death || cub.lifeStage != LifeStage.child || cub.hungry < 0f) continue;
            if (Vector3.Distance(self.transform.position, cub.transform.position) <= within) FeedCub(self, cub);
        }
    }

    // ── PROCESOS NATURALES de las especies del santuario (LifeHistory; docs/ciclo-de-vida.md) ──────────────────────────

    // BEBER: al bebedero si lo hay a mano; si no, come nieve allí mismo (el pingüino en tierra, el malamute sin bebedero).
    static void Drink(Animal self)
    {
        Hydration h = self.GetComponent<Hydration>();
        if (h == null) return;
        if (h.Life != null && h.Life.water == LifeHistory.Water.Drinks)
        {
            WaterSource w = WaterSource.Nearest(self.transform.position, 60f);
            if (w != null)
            {
                if (Flat(w.transform.position - self.transform.position) <= 1.8f) { Stop(self, 3f); h.Drink("bebe del bebedero"); return; }
                if (GoTo(self, "drink", w.transform.position)) return;
            }
        }
        Stop(self, 2f);
        h.Drink("come nieve");
    }

    static void Stop(Animal self, float seconds)
    {
        if (self.nav != null && self.nav.enabled && self.nav.isOnNavMesh) NavTrace.Reset(self, "DesireCatalog línea 594");
        self.Loco?.Idle(seconds);
    }

    // GUARIDA: va a su guarida y se queda dentro (ayunando si es la osa) amamantando a las crías que tenga al lado.
    static float DenNeed(Animal self)
    {
        MaternityDen md = self.GetComponent<MaternityDen>();
        if (md == null || !md.Wants || md.Life == null) return 0f;
        return md.Life.denFasts ? W_DEN_FAST : W_DEN;
    }

    static void StayInDen(Animal self)
    {
        MaternityDen md = self.GetComponent<MaternityDen>();
        if (md == null || !md.hasSite) return;
        if (md.Life != null && md.Life.denFasts) self.hungry = Mathf.Min(self.hungry, -0.05f);   // ayuna: vive de su grasa
        if (Flat(md.site - self.transform.position) > 1.2f)
        {
            if (GoTo(self, "den", md.site)) return;
            md.site = self.transform.position;   // la guarida no se alcanza desde aquí: se hace la suya donde está
        }
        Stop(self, 1f);
        FeedCubsNear(self, 3f);
    }

    // INCUBAR: el que tiene el huevo sobre las patas se queda quieto (y ayuna); el macho del emperador va a por él.
    static void Incubate(Animal self)
    {
        Egg e = Egg.ToIncubate(self);
        if (e == null) return;
        self.hungry = Mathf.Min(self.hungry, -0.05f);   // incubando no come: el emperador ayuna ~115 días
        if ((e.incubator != self || (e.Life != null && e.Life.eggNest)) && Flat(e.transform.position - self.transform.position) > 0.8f && GoTo(self, "incubate", e.transform.position)) return;
        Stop(self, 1f);
    }

    /// <summary>(2026-11-11) El adulto vinculado (madre o padre, de su especie, vivo) de una cría; null = huérfana (PredatorRound.Orphan).</summary>
    public static Animal ParentOfCub(Animal cub) => cub == null ? null : ParentOf(cub);

    // LA CRÍA SIGUE A SU MADRE (o a su padre: el adulto vinculado más cercano) en tierra.
    static Animal ParentOf(Animal self)
    {
        Animal best = null; float bd = float.MaxValue;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || Gone(b.target)) continue;
            Animal o = (b.target as Component)?.GetComponentInParent<Animal>();
            if (o == null || o == self || o.death || o.lifeStage != LifeStage.adult || o.LifeSpecies != self.LifeSpecies) continue;
            float d = Vector3.Distance(o.transform.position, self.transform.position);
            if (d < bd) { bd = d; best = o; }
        }
        return best;
    }

    static float HeelNeed(Animal self)
    {
        if (self.lifeStage != LifeStage.child || self.nav == null || !self.nav.enabled) return 0f;
        LifeHistory life = LifeHistory.Of(self.LifeSpecies);
        if (!life.youngFollow) return 0f;
        Animal p = ParentOf(self);
        if (p == null) return 0f;
        float d = Flat(p.transform.position - self.transform.position);
        if (d <= life.followDistance) return 0f;
        return W_HEEL * (0.3f + 0.7f * Mathf.Clamp01(d / (life.followDistance * 4f)));
    }

    static void Heel(Animal self)
    {
        Animal p = ParentOf(self);
        if (p == null) return;
        float r = LifeHistory.Of(self.LifeSpecies).followDistance;
        if (!GoTo(self, "heel", p.transform.position + Ring(0.4f, Mathf.Max(0.5f, r * 0.6f)))) GoTo(self, "heel", p.transform.position);
    }

    // La cría hambrienta se arrima al adulto vinculado más cercano (su madre/padre) o, si no lo tiene, se queda en su sitio
    // esperando la toma. Pasos cortos y con destino fijo (sin vagar por el recinto).
    /// <summary>(2026-11-13, ronda 85) ¿Es una CRÍA con su madre (o padre) vinculada a ≤ <paramref name="within"/> m? La cría que pide la toma va
    /// donde va su madre (MeetPoint): si la madre anda por la guarida, la cría la sigue, y eso no es «dar vueltas» (lobo #57, «AwaitFeeding»).</summary>
    public static bool WithParent(Animal self, float within = 5f)
    {
        if (self == null || self.lifeStage != LifeStage.child) return false;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || Gone(b.target)) continue;
            Animal o = (b.target as Component)?.GetComponentInParent<Animal>();
            if (o != null && o != self && !o.death && o.lifeStage == LifeStage.adult && o.SpeciesName == self.SpeciesName && Vector3.Distance(o.transform.position, self.transform.position) <= within) return true;
        }
        return false;
    }

    /// <summary>(2026-11-14) El sitio de espera de una cría en su casa: a <see cref="CubSpotRadius"/> m de su HomeOrigin, en un ángulo propio (de su
    /// id): dos crías de la misma guarida no esperan en el mismo punto.</summary>
    public const float CubSpotRadius = 2.5f;
    /// <summary>(2026-11-14) Distancia a la pareja hasta la que sigue el cortejo una vez empezado (empieza a ≤ 2,5 m).</summary>
    public const float CourtingKeep = 4f;
    /// <summary>(2026-11-14) Prioridad de evitación de la cría que espera la toma (más baja = más importante: los demás la rodean) y la normal.</summary>
    public const int WaitingPriority = 20, DefaultPriority = 50;
    public static Vector3 CubWaitSpot(Animal self) => CubWaitSpot(self.HomeOrigin, self.GetHashCode());
    public static Vector3 CubWaitSpot(Vector3 home, int id)
    {
        float a = (Mathf.Abs(id * 137) % 360) * Mathf.Deg2Rad;   // (137: reparte ids seguidos por todo el círculo)
        return home + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * CubSpotRadius;
    }

    static void AwaitFeeding(Animal self)
    {
        if (self.nav == null || !self.nav.enabled || !self.nav.isOnNavMesh) return;
        Transform parent = null; float best = 30f;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || Gone(b.target)) continue;
            Animal o = (b.target as Component)?.GetComponentInParent<Animal>();
            if (o == null || o == self || o.death || o.lifeStage != LifeStage.adult || o.SpeciesName != self.SpeciesName) continue;
            float d = Vector3.Distance(o.transform.position, self.transform.position);
            if (d < best) { best = d; parent = o.transform; }
        }
        // (2026-11-14, ronda 98) A la madre solo si ESTÁ (quieta o casi): la cría iba hacia la madre que andaba por la guarida, la madre seguía y
        // la cría volvía a su sitio a 5–8 m, una y otra vez (lobos #1, #5: «da vueltas» 19–27 m en 20 s, «SetPath … 4,9–7,6 m» a su sitio cada
        // 3–5 s). Si la madre anda, la cría espera en su sitio; la madre que va a darle la toma va a ella (NurseCub).
        if (parent != null && parent.TryGetComponent(out NavMeshAgent pm) && pm.isActiveAndEnabled && pm.velocity.magnitude > 0.5f) parent = null;
        Vector3 to = parent != null ? MeetPoint(self.transform.position, parent.position) : self.HomeOrigin;
        // (2026-11-13, vivo75) Sin madre cerca, la toma se espera EN CASA, no en su punto exacto: las crías de la guarida de los lobos tenían el mismo
        // HomeOrigin (-205,-190) y, a 1,5 m, se empujaban unas a otras alrededor de él sin llegar nunca («da vueltas en círculo (nurse)», 19–27 m
        // en 20 s para 0–1,5 m netos, «deseo: SetDestination» hacia (-205,-190) cada pocos segundos). A ≤ 4 m de casa ya está en casa.
        // (2026-11-13, ronda 77) …y el «en casa» es el punto del NavMesh más cercano a ella: el HomeOrigin de la guarida (y = 0) no está sobre el NavMesh
        // (la guarida lo talla), Move no tenía camino calculado y caía en SetDestination; el agente acababa a > 4 m y seguía dando vueltas
        // (#85, #90: «deseo: SetDestination» hacia (-205,0,0,0,-190,0) cada 4–5 s, 19–28 m en 20 s para 0–1,9 m netos).
        bool atHome = false;
        if (parent == null)
        {
            // (2026-11-14, ronda 93) CADA CRÍA, SU SITIO: todas esperaban la toma en el mismo punto de la guarida y se apartaban unas a otras a
            // empujones (lobos #2, #22: «da vueltas» 16–23 m en 20 s junto a (-205,-190)). Ahora cada una tiene el suyo, a 2,5 m de casa en
            // su propio ángulo (como el sitio de cada uno al volver a casa, GoHome).
            Vector3 own = CubWaitSpot(self);
            if (NavReach.Reachable(self, own, out Vector3 hs, 3f) || NavReach.Reachable(self, self.HomeOrigin, out hs, 6f)) to = hs;
            // (2026-11-13, ronda 78) Y una vez en casa, no sale por un empujón: hasta 7 m sigue «en casa» (#16: SetPath de 4,9–6,7 m al punto de
            // casa cada 2–7 s, 18–25 m en 20 s; los otros lobos de la guarida la apartaban y volvía).
            bool was = _homeWait.Contains(self);
            atHome = was ? Flat(self.transform.position - to) <= 3f : Flat(self.transform.position - to) <= 1f;
        }
        if (atHome) _homeWait.Add(self); else _homeWait.Remove(self);
        // (2026-11-14, ronda 97) LA CRÍA QUE ESPERA NO SE APARTA: en la guarida los adultos que pasan la apartaban (el NavMesh separa a los agentes
        // que se tocan), y la cría volvía a su sitio una y otra vez (#60, #38: «da vueltas» 18–26 m en 20 s junto a su sitio, con «pide la toma»
        // o un camino de 4–5 m a él). Esperando, su agente es más importante (prioridad de evitación más baja): son los que pasan quienes la rodean.
        self.nav.avoidancePriority = atHome ? WaitingPriority : DefaultPriority;
        // (2026-11-13, ronda 78) La madre que SE VA (anda alejándose de la cría) no se persigue: la cría la llama quieta (#83: «AwaitFeeding»
        // hacia un punto que se movía, (-202,-193) → (-209,-191), 18–22 m en 20 s). Cuando la madre viene o se para, va a ella.
        if (parent != null && Vector3.Distance(self.transform.position, parent.position) > 2f && parent.TryGetComponent(out NavMeshAgent pn) && pn.isActiveAndEnabled
            && pn.velocity.magnitude > 0.5f && Vector3.Dot(pn.velocity, self.transform.position - parent.position) <= 0f)
        { NavTrace.Reset(self, "AwaitFeeding (la madre se va: la llama)"); self.Loco?.Idle(1f); return; }
        if (parent != null ? Vector3.Distance(self.transform.position, parent.position) > 2f : !atHome) MoveOrNearest(self, to);
        else
        {
            NavTrace.Reset(self, "AwaitFeeding (pide la toma)"); self.Loco?.Idle(1f);
            self.nav.avoidancePriority = WaitingPriority;   // (junto a su madre, pidiendo: tampoco se la aparta)
            // PIDE Y LE DAN: la cría pegada a su madre (o padre) mama, o pide con el pico y le regurgitan: una toma cada 2 s de
            // juego. La toma no esperaba solo a que el «nurse» del adulto ganara a sus otros deseos: en la isla del pingüino el
            // macho (hambriento tras 2 meses incubando) y la hembra (que vuelve del mar) casi nunca lo elegían y el polluelo
            // crecía sin una toma (grupo 58: «Crianza» no se vio). En la naturaleza la súplica de la cría desencadena la toma.
            Animal p = parent != null ? parent.GetComponent<Animal>() : null;
            if (p != null && self.lifeStage == LifeStage.child && !p.asleep && BroodRide.ParentalCare(self.LifeSpecies) && Time.time >= (_begAt.TryGetValue(self, out float at) ? at : 0f))
            { _begAt[self] = Time.time + 2f; FeedCub(p, self); }
        }
    }
    static readonly System.Collections.Generic.Dictionary<Animal, float> _begAt = new System.Collections.Generic.Dictionary<Animal, float>();
    static readonly HashSet<Animal> _homeWait = new HashSet<Animal>();   // crías que ya esperan la toma en casa (histéresis 4 → 7 m)

    // ¿El ser del vínculo ya no está? b.target es una interfaz (Animal.ITarget): «== null» no ve el objeto DESTRUIDO de Unity y su
    // .transform lanzaba MissingReferenceException (812 veces en la batería del 2026-11-02: PenguinBehavior destruido, SocialNeed).
    static bool Gone(object target) => target == null || (target is UnityEngine.Object o && o == null);

    // Necesidad social: recorre los bonds POSITIVOS y devuelve la mayor "urgencia" — cuidar = vínculo×apuro del otro;
    // seguir = vínculo×lejanía. 0..1+. No mueve; solo puntúa (lo usa NeedProbe).
    static float SocialNeed(Animal self, bool care)
    {
        float best = 0f;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || Gone(b.target)) continue;
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
            if (b == null || b.value <= 0f || Gone(b.target)) continue;
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
        _lastOwner = null;
        if (!self.nav.CalculatePath(h.position, _path) || _path.status != NavMeshPathStatus.PathComplete) return false;
        // (2026-11-13, suite 80) UN CAMINO QUE NO SALE DE DONDE ESTÁ O NO LLEGA AL SITIO NO VALE: zorros que volvían a casa desde 88–174 m
        // recibían un camino «completo» de 2 esquinas y 2,7–9,7 m («SetPath (PathComplete, 2 esquinas, 2,7 m)» hacia una meta a 174 m) y a
        // los pocos segundos se quedaban sin camino, una vez y otra. Si el camino empieza lejos del cuerpo (el agente desfasado), se coloca
        // el agente en el cuerpo y se calcula otra vez; si aun así no sale de él o no acaba en el sitio, no se llega.
        if (!PathFits(self, h.position, out float startOff, out float endOff))
        {
            Vector3 gap = self.nav.nextPosition - self.transform.position; gap.y = 0f;
            if (startOff > 1f && gap.magnitude > 1f) { self.nav.Warp(self.transform.position); if (!self.nav.CalculatePath(h.position, _path) || _path.status != NavMeshPathStatus.PathComplete) return false; }
            if (!PathFits(self, h.position, out startOff, out endOff))
            {
                BadPaths++;
                if (_badLogs++ < 20) Debug.Log($"[Fauna] «{self.name}»: camino «completo» que no vale hacia {h.position} ({Vector3.Distance(self.transform.position, h.position):0.0} m): empieza a {startOff:0.0} m del agente y acaba a {endOff:0.0} m del sitio, {_path.corners.Length} esquinas.");
                return false;
            }
        }
        at = h.position; _lastOwner = self; _lastTo = at;
        return true;
    }
    /// <summary>Caminos «completos» descartados porque no salían del cuerpo o no acababan en el sitio (diagnóstico).</summary>
    public static int BadPaths;
    static int _badLogs;
    static bool PathFits(Animal self, Vector3 to, out float startOff, out float endOff)
    {
        Vector3[] c = _path.corners;
        startOff = endOff = float.MaxValue;
        if (c == null || c.Length == 0) return false;
        // (ronda 82) El inicio se mide desde el AGENTE, no desde el cuerpo: los 16 zorros de vivo82 empezaban siempre a 3,1 m del cuerpo (el cuerpo
        // sobre algo fuera del NavMesh; el agente, en el punto del NavMesh más cercano, y el Warp lo deja igual). Ese camino sí vale: sale del agente.
        Vector3 s = c[0] - self.nav.nextPosition, e = c[c.Length - 1] - to; s.y = 0f; e.y = 0f;
        startOff = s.magnitude; endOff = e.magnitude;
        return startOff <= 1f && endOff <= 1f;
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
