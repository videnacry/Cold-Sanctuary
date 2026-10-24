using System;
using System.Collections.Generic;
using UnityEngine;

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
        new Desire("eat",
            self => self.hungry >= 0f ? Mathf.Max(0.01f, self.hungry) : 0f,
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
                // Si alguien de su especie LLAMA (destellos de luciérnaga, canto de cigarra) y lo percibe, va hacia él.
                MatingSignal caller = MatingSignal.NearestCaller(self, 40f * Mathf.Max(0.3f, self.perception));
                if (caller != null && self.sex == Sex.female) { self.nav.SetDestination(caller.transform.position); return; }
                Vector3 g = TraceField.Trail(self.transform.position, TraceChannel.Estrus);
                if (g.sqrMagnitude > 0.0001f) self.nav.SetDestination(self.transform.position + g.normalized * 6f);
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

        // GUARDAR EL NIDO (tijereta, solífugo, escorpión; BroodRide.Guards): la madre con crías pequeñas VINCULADAS se queda
        // en casa vigilándolas (las tijeretas: juntan, limpian y defienden la puesta, Kölliker 2007). Necesidad = vínculo
        // con sus crías × afabilidad → una madre poco afable o hambrienta (eat gana) las deja más; el resto, no lo siente.
        new Desire("brood",
            self => BroodRide.Guards(self.SpeciesName) && self.sex == Sex.female ? BroodTie(self) * Mathf.Max(0.3f, self.afabilidad) * W_BROOD : 0f,
            self =>
            {
                if (self.nav == null || !self.nav.isOnNavMesh) return;
                if (Vector3.Distance(self.transform.position, self.HomeOrigin) > 1.5f) self.nav.SetDestination(self.HomeOrigin);
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
        new Desire("homebound",
            self => W_HOME * Mathf.Clamp01(Vector3.Distance(self.transform.position, self.HomeOrigin) / Mathf.Max(1f, self.HomeRadius)),
            self => { if (self.nav != null && self.nav.isOnNavMesh) self.nav.SetDestination(self.HomeOrigin); },
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
            // UnityEngine.Random explícito: el `using System` de arriba trae System.Random y el nombre corto es ambiguo (CS0104).
            self => { if (self.nav != null && self.nav.isOnNavMesh) { Vector2 r = UnityEngine.Random.insideUnitCircle * 8f; self.nav.SetDestination(self.transform.position + new Vector3(r.x, 0f, r.y)); } },
            "explore"),
    };

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
        if (self.nav != null && self.nav.isOnNavMesh) self.nav.SetDestination(cub.transform.position);
        if (Vector3.Distance(self.transform.position, cub.transform.position) <= 2.5f)
        {
            cub.hungry = Mathf.Max(0f, cub.hungry - 0.3f);   // amamantar/compartir comida
            self.exhaustion += 0.5f;                          // cuidar cansa un poco
        }
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
        if (best != null) self.nav.SetDestination(best.position);
    }
}
