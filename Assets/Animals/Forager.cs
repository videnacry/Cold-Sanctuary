using System.Collections;
using UnityEngine;

/// <summary>
/// Componente de FORRAJEO (docs/anima-dissolving-animal.md). Encapsula "qué/dónde comer" Y el forrajeo entero:
/// **presa** (carnívoro; por PROXIMIDAD + STATS vía `SelectPrey`/`Predation`, ya no una tabla Diet), **pasto** y/o
/// **banco de peces** (herbívoro/pescador). Los flags (`eatsPrey`/`eatsGrass`/`eatsFish`) son **combinables** — un
/// **omnívoro** marca varios y `SelectTarget` elige la fuente más cercana. **Portable**: cualquier `Anima` lo lleva
/// con su combinación (el "qué come" es config de componente, no la subclase `Carnivore`/`Herbivore`).
///
/// Incluye la conducta completa: `SelectTarget` (elegir) + `Hunt`/`Graze` (perseguir/pastar por `Locomotion`) +
/// `Eat` (ingerir: nutrición + bond + metabolismo). `Carnivore.Feed`/`Herbivore.Feed` solo delegan.
/// </summary>
public class Forager : MonoBehaviour
{
    [Tooltip("Come PRESA (carnívoro): busca Animas cazables cerca por stats (Predation), ya no por una tabla Diet.")]
    public bool eatsPrey;
    [Tooltip("Come PASTO (herbívoro terrestre).")]
    public bool eatsGrass;
    [Tooltip("Come PECES/banco (herbívoro/consumidor marino/pescador).")]
    public bool eatsFish;
    [Tooltip("Alcance de detección de presa POR PERCEPCIÓN: radio = perception × esto (deriva de stats; crece con la " +
             "evolución de la percepción). Reemplaza los rangos por-presa de la vieja Diet. Tunable.")]
    public float huntRangePerPerception = 80f;
    [Tooltip("Radio mínimo de búsqueda de presa (por si la percepción es muy baja).")]
    public float minHuntRadius = 20f;
    [Tooltip("Peso de la distancia en la preferencia de presa (más cerca = preferida). Tunable.")]
    public float distanceWeight = 0.01f;

    /// <summary>Fija los flags de comida por ESPECIE (data; antes era el hook ConfigureForager de Carnivore/Herbivore/
    /// especies). Carnívoros comen presa; Oso/Zorro además pescan; herbívoros terrestres pastan; marinos pescan.</summary>
    public void ConfigureForSpecies(string species)
    {
        switch (species)
        {
            case "Wolf": case "Malamute":            eatsPrey = true; break;
            case "Bear": case "Fox":                 eatsPrey = true; eatsFish = true; break;   // cazan y pescan
            case "Bunny": case "Deer":               eatsGrass = true; break;
            case "Whale": case "Seal":               eatsFish = true; break;                    // herbívoros/consumidores marinos
            // Insectos
            case "Ant":                              eatsPrey = true; break;                    // carnívora; caza invertebrados pequeños
            case "Aphid":                            eatsGrass = true; break;                   // savia de plantas
            case "Ladybug":                          eatsPrey = true; break;                    // depredadora de pulgones
            case "Spider":                           eatsPrey = true; break;                    // depredadora; emboscada
            case "Cricket":                          eatsGrass = true; eatsPrey = true; break;  // omnívora oportunista
        }
    }

    /// <summary>El objetivo de comida más cercano entre las fuentes que come (presa/pasto/pez), o null. Un omnívoro
    /// (varios flags) elige la más cercana; el carnívoro/herbívoro puro solo tiene una fuente activa.</summary>
    public GameObject SelectTarget(Animal self)
    {
        if (self == null) return null;
        Vector3 pos = self.transform.position;
        GameObject prey  = eatsPrey  ? SelectPrey(self) : null;                          // presa por proximidad + stats
        GameObject grass = eatsGrass ? GrassPatch.Nearest(pos)?.gameObject : null;
        GameObject fish  = eatsFish  ? FishSchool.Nearest(pos)?.gameObject : null;
        return Nearest(pos, prey, grass, fish);
    }

    /// <summary>PRESA por PROXIMIDAD + STATS (reemplaza la `Diet`): `OverlapSphere` busca `Anima`s comestibles
    /// cercanas —incluidas **carcasas** y el **jugador**—; descarta la PROPIA especie (no canibalismo) y las que un
    /// **vínculo** protege (`CanHarm`); una presa VIVA solo cuenta si mi poder EFECTIVO (con manada) supera su defensa
    /// (`Predation`). Prefiere lo más **fácil** (poder/defensa) y **cercano**; una carcasa es "gratis". Emergente:
    /// quién es presa sale de stats, no de una tabla fija.</summary>
    GameObject SelectPrey(Animal self)
    {
        Vector3 pos = self.transform.position;
        float myPower = Predation.EffectivePower(self);   // con manada
        float radius = Mathf.Max(minHuntRadius, self.perception * huntRangePerPerception);   // detección POR STATS
        GameObject best = null; float bestScore = float.NegativeInfinity;
        foreach (Collider col in Physics.OverlapSphere(pos, radius))
        {
            Anima a = col.GetComponentInParent<Anima>();
            if (a == null || a == self) continue;
            IEdible food = a.GetComponent<IEdible>();
            if (food == null || food.Consumed) continue;                                   // no comestible / ya consumida
            ITarget t = a.GetComponent<ITarget>();
            if (t == null) continue;
            bool carcass = t.Dead;
            float defense = Predation.Defense(a);
            if (!carcass)   // presa VIVA
            {
                if (a.SpeciesName != null && a.SpeciesName == self.SpeciesName) continue;   // no CAZAR la propia especie
                if (myPower < defense) continue;          // solo si puedo con ella (con manada)
                if (!self.CanHarm(t)) continue;           // un vínculo la protege
            }
            // Carcasa: comestible siempre (scavenging), incluso de la propia especie.
            float dist = Vector3.Distance(pos, a.transform.position);
            float ease = carcass ? 3f : myPower / Mathf.Max(0.1f, defense);                 // más fácil = preferida
            float score = ease - dist * distanceWeight;                                     // más cerca = preferida
            if (score > bestScore) { bestScore = score; best = a.gameObject; }
        }
        return best;
    }

    static GameObject Nearest(Vector3 pos, params GameObject[] gos)
    {
        GameObject best = null; float bestSq = float.MaxValue;
        foreach (GameObject go in gos)
        {
            if (go == null) continue;
            float d = (go.transform.position - pos).sqrMagnitude;
            if (d < bestSq) { bestSq = d; best = go; }
        }
        return best;
    }

    /// <summary>Da un mordisco a `food` y aplica la INGESTA a `self`: nutrición (baja el hambre + `Metabolism` opt-in),
    /// **bond** con quien dejó la comida (compartir alimenta el vínculo), y marca la 1ª sólida de una cría. Devuelve
    /// la nutrición obtenida. Multi-consumo: varios pueden morder el mismo `food` (pool compartido de `IEdible`).</summary>
    public float Eat(Animal self, IEdible food, GameObject foodObject, float biteSize)
    {
        if (self == null || food == null) return 0f;
        float nutrition = food.Consume(biteSize);
        self.hungry -= nutrition;
        self.GetComponent<Metabolism>()?.AbsorbFood(nutrition, food.Material);   // opt-in: construye stats / grasa
        FoodItem dropped = foodObject != null ? foodObject.GetComponent<FoodItem>() : null;
        if (dropped != null && dropped.droppedBy != null)
            self.GrowBond(dropped.droppedBy, BondType.Friend, nutrition);        // quien te trae comida te cae bien
        if (self.lifeStage == LifeStage.child && dropped != null)
            self.firstSolidEaten = true;
        return nutrition;
    }

    /// <summary>PASTAR/pescar (herbívoro/consumidor de banco): va a la fuente más cercana (por `Locomotion`) y come,
    /// bajando el hambre; si es un banco de peces, lo reduce (`Graze`). Movido desde `Herbivore.Feed` (etapa 3).</summary>
    public IEnumerator Graze(Animal self)
    {
        if (self == null || (self.lifeStage != LifeStage.adult && self.lifeStage != LifeStage.teen)) yield break;
        self.busy = true;

        GameObject food = SelectTarget(self);
        Transform foodSource = food != null ? food.transform : null;

        // Los marinos nadan sobre mar abierto sin NavMesh horneado debajo → guardar (Loco.Walk ya lo respeta,
        // pero sin el guard el while no avanzaría nunca).
        if (foodSource != null && self.nav != null && self.nav.isOnNavMesh)
        {
            float walkInterval = TimeController.timeController.TimeSpeedMinuteSecs / 30;
            while (Vector3.Distance(self.transform.position, foodSource.position) > 3f)
            {
                self.Loco.Walk(foodSource.position, walkInterval);
                yield return new WaitForSeconds(walkInterval);
            }
        }

        float interval = TimeController.timeController.TimeSpeedMinuteSecs / Random.Range(7, 12);
        float feed = self.Body.GetMealWeight(self.rig.mass) * 0.6f;
        if (self.Group.fed.Length > 0) { interval *= 1.2f; feed *= 1.5f; }   // con crías, come más despacio y más
        self.Loco.Idle(interval);
        yield return new WaitForSeconds(interval);
        FishSchool fs = (eatsFish && foodSource != null) ? foodSource.GetComponent<FishSchool>() : null;
        if (fs != null)
        {
            // PEZ: la ingesta la hace el MORDISCO-POR-COLISIÓN (FishSchool.OnTriggerStay) mientras el depredador reposa
            // DENTRO del banco (su trigger) → aquí solo se navega/reposa, sin alimentar (evita doble ingesta).
            yield return new WaitForSeconds(interval);
        }
        else
        {
            self.hungry -= feed;   // PASTO: come aquí como siempre
            yield return new WaitForSeconds(interval);
            self.hungry -= feed;
        }
        self.busy = false;
    }

    /// <summary>CAZAR (carnívoro): elige presa (Diet), la persigue (por `Locomotion`), la hiere al alcance y come al
    /// abatirla; si sobra, la lleva a las crías. Movido desde `Carnivore.Feed` (etapa 3). La depredación va por stats
    /// (`Predation` decide en `Diet.SelectPrey`).</summary>
    public IEnumerator Hunt(Animal self)
    {
        if (self == null || (self.lifeStage != LifeStage.teen && self.lifeStage != LifeStage.adult)) yield break;

        GameObject prey = SelectTarget(self);
        if (prey == null)
        {
            yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 5);
            yield break;
        }
        self.busy = true;
        ITarget victim = prey.GetComponent<ITarget>();
        if (victim == null) { self.busy = false; yield break; }
        bool wasAliveAtStart = !victim.Dead;   // para la confianza-por-uso: solo cuenta como CAZA si estaba viva (no carroña)
        prey.GetComponent<Anima>()?.RespondToThreat(self.gameObject);   // la presa reacciona a la amenaza
        Vector3 location = prey.transform.position;
        float distance = Vector3.Distance(self.transform.position, location);
        float cansancio = 0;
        do
        {
            float interval = TimeController.timeController.TimeSpeedMinuteSecs / 60;
            location = prey.transform.position;
            self.Loco.GoTo(location);
            distance = Vector3.Distance(location, self.transform.position);
            if (victim.Dead)
            {
                if (distance < 6)
                {
                    IEdible food = prey.GetComponent<IEdible>();
                    if (food == null || food.Consumed) break;
                    if (self.hungry < -self.Body.GetMealMaxWeight(self.rig.mass)) break;
                    self.Loco.Idle(TimeController.timeController.TimeSpeedMinuteSecs / 30);
                    Eat(self, food, prey, self.BiteSize);
                }
                else
                {
                    self.Loco.SetGait(true, TimeController.timeController.TimeSpeedMinuteSecs / 30);
                }
                yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 30);
            }
            else
            {
                Anima victimLiving = prey.GetComponent<Anima>();
                bool preyAware = victimLiving != null && victimLiving.aware;
                if (distance < 300 || preyAware)
                {
                    self.Loco.SetGait(true, interval);
                    cansancio += 0.01f;
                    if (distance < 8) victim.Hurt(0.8f);
                    yield return new WaitForSeconds(interval);
                }
                else
                {
                    self.Loco.SetGait(false, TimeController.timeController.TimeSpeedMinuteSecs / 20);
                    yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 20);
                }
            }
        } while (distance < 700 && cansancio < 1);
        self.exhaustion += cansancio;

        // Confianza-por-uso (D2): una CAZA de presa viva refuerza el combate si la abatí; si me rendí (agotado), lo merma.
        // Así el depredador exitoso se vuelve más osado y el que nunca caza (herbívoro) no. Ver capabilities-and-embodiment.md §4.
        if (wasAliveAtStart) self.RecordUse(Capability.Combat, victim.Dead);

        // Si la presa quedó como FoodItem sin consumir, llevarla a las crías.
        FoodItem remains = prey != null ? prey.GetComponent<FoodItem>() : null;
        if (victim.Dead && remains != null && !remains.Consumed && self.Group?.fed?.Length > 0)
            (self as ICarrier)?.PickUp(remains);

        self.busy = false;
    }
}
