using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA RONDA DEL ALIMENTADOR (Berón 2026-11-05; docs/alimentador.md): no hay comedero. Quien tiene la misión sigue a cada depredador de tierra por el
/// mapa, al llegar evalúa si tiene hambre y le deja su ración o lo marca, y va a por el siguiente. La LISTA ES COMPARTIDA: cada alimentador se
/// reserva uno, así un grupo se reparte el trabajo y nadie mira dos veces al mismo.
/// La lista SOLO SE RENUEVA CUANDO ESTÁ COMPLETA (Berón 2026-11-05): puede durar varios días, pasa de una jornada a otra, y nadie sale de ella sin estar comprobado:
/// un depredador al que no se llega vuelve a la cola, porque uno con hambre caza herbívoros.
///
/// SEGURIDAD (el oso no es un perro): nadie se acerca a menos de <see cref="SafeDistance"/>, la ración se lanza desde ahí y se cae a unos metros del
/// animal, nunca en la mano; si el animal se le acerca, el alimentador se retira y lo apunta. La regla real es mucho más dura (Svalbard: 300 m, 500 m
/// de marzo a junio; los zoos: contacto protegido, el cuidador nunca comparte espacio); aquí va a la escala comprimida del santuario.
/// </summary>
public static class PredatorRound
{
    public enum State { Pending, Claimed, Checked, Fed }

    public class Entry
    {
        public Animal animal;
        public State state;
        public string by;
        /// <summary>Veces que no se le pudo atender: vuelve a la cola, detrás de los demás.</summary>
        public int misses;
        public override string ToString() => $"{(animal != null ? animal.name : "?")}: {state}{(by != null ? $" ({by})" : "")}";
    }

    /// <summary>Con <c>hungry</c> por encima de esto se le da de comer (como la vieja guardia: −0,05).</summary>
    public const float HungryAt = -0.05f;
    public const float RationKg = 0.5f;

    static readonly List<Entry> _list = new List<Entry>();
    static float _nextScan;

    /// <summary>Los metros a los que se queda el alimentador. Oso 18 (el mapa mide 500 m: los 300 de Svalbard no caben), lobo 10, el resto 5.</summary>
    public static float SafeDistance(string species) => species == "Bear" || species == "PolarBear" ? 18f : species == "Wolf" ? 10f : 5f;

    /// <summary>Raciones de 500 g por visita: la ración diaria de su especie repartida en las rondas del día (3).</summary>
    public static int RationsFor(string species)
    {
        float kg = FoodChain.Intake.TryGetValue(species, out float i) ? i : 1f;
        return Mathf.Clamp(Mathf.CeilToInt(kg / 3f / RationKg), 1, 8);
    }

    /// <summary>¿Lo atiende la ronda? Los carnívoros de tierra del santuario, sin crías, ni los del mar, ni los de un recinto de la Cría.</summary>
    public static bool Served(Animal a)
    {
        if (a == null || a.death) return false;
        bool orphan = Orphan(a);   // (2026-11-11) la cría huérfana de cualquier especie de tierra: biberón
        if (!orphan && (a.lifeStage == LifeStage.child || a.Forage == null || !a.Forage.eatsPrey)) return false;
        if (a.GetComponent<MarineLife>() != null || a.GetComponent<IceInsectLife>() != null || a.GetComponentInParent<CriaFeeder>() != null) return false;
        // (2026-11-11) NI LOS INGRESADOS: les da de comer la veterinaria (VetCare), en la mano. En la lista no se les alcanzaba nunca («Lobo_paciente»,
        // 20 veces pendiente) y, como la lista solo se renueva completa, nadie más volvía a comer: en 12 h de juego los lobos pasaron de 4 a 1 y el oso
        // adulto murió de inanición.
        if (VetCare.Admitted(a)) return false;
        return Mathf.Abs(a.transform.position.x) <= 250f && Mathf.Abs(a.transform.position.z) <= 250f;   // (fuera, los escenarios de prueba)
    }

    /// <summary>¿Está hecho? Solo el comprobado sin hambre o el que recibió su ración.</summary>
    public static bool Done(Entry e) => e.state == State.Checked || e.state == State.Fed;

    /// <summary>La lista en curso. Se renueva solo si todos están hechos; mientras, los muertos salen y los nuevos adultos entran.</summary>
    public static List<Entry> Today()
    {
        _list.RemoveAll(e => e.animal == null || e.animal.death);
        if (_list.Count == 0 || _list.TrueForAll(Done)) { _list.Clear(); Scan(); _nextScan = Time.time + 20f; }
        else if (Time.time >= _nextScan) { _nextScan = Time.time + 20f; Scan(); }
        return _list;
    }

    static void Scan()
    {
        foreach (Animal a in Object.FindObjectsByType<Animal>(FindObjectsSortMode.None))
            if (Served(a) && !_list.Exists(e => e.animal == a)) _list.Add(new Entry { animal = a, state = State.Pending });
    }

    /// <summary>Empieza otra lista ya, aunque falte alguien (solo las pruebas).</summary>
    public static void NewRound() { _list.Clear(); _nextScan = 0f; _fedAt.Clear(); }

    public static int Left { get { int n = 0; foreach (Entry e in Today()) if (!Done(e)) n++; return n; } }

    /// <summary>(2026-11-11) EL ORDEN DE LA RONDA (menor = antes), por urgencia: (1) los que aún no se intentaron antes que los que no se
    /// alcanzaron (no se pierde el turno con uno inalcanzable); (2) los depredadores adultos antes que las crías del biberón; (3) el más
    /// hambriento; (4) el más grande (oso, lobo, zorro); (5) el más cercano. La lista sigue renovándose solo completa (Berón 2026-11-05); el
    /// orden decide a quién se va primero. Vivo49: con 37 en la lista y el más cercano primero, dos osas murieron de hambre detrás de polluelos.</summary>
    public static double Priority(Animal a, int misses, Vector3 from)
    {
        if (a == null) return double.MaxValue;
        int kind = Orphan(a) ? 1 : 0;
        int hunger = Mathf.RoundToInt((9f - Mathf.Clamp(a.hungry, -1f, 9f)) * 10f);   // 0 (muy hambriento) … 100 (saciado)
        int apex = a.SpeciesName == "Bear" ? 0 : a.SpeciesName == "Wolf" ? 1 : a.SpeciesName == "ArcticFox" ? 2 : 3;
        float dist = Mathf.Min((a.transform.position - from).sqrMagnitude, 999999f);
        return Mathf.Min(misses, 9) * 1e12 + kind * 1e10 + hunger * 1e7 + apex * 1e6 + dist;
    }

    /// <summary>¿Hay un pendiente más urgente que <paramref name="cur"/> (otro tipo antes, o mucho más hambriento)? El alimentador lo deja y va
    /// a por él (sin contarlo como no alcanzado).</summary>
    public static Entry MoreUrgent(Entry cur, Vector3 from)
    {
        if (cur == null || cur.animal == null) return null;
        double mine = Priority(cur.animal, 0, from) - Mathf.Min((cur.animal.transform.position - from).sqrMagnitude, 999999f);
        Entry best = null; double bd = mine - 2e7;   // (al menos 0,2 de hambre más, u otro tipo antes)
        foreach (Entry e in Today())
        {
            if (e == cur || e.state != State.Pending || e.animal == null || e.misses > 0) continue;
            double p = Priority(e.animal, 0, from) - Mathf.Min((e.animal.transform.position - from).sqrMagnitude, 999999f);
            if (p < bd) { bd = p; best = e; }
        }
        return best;
    }

    /// <summary>Suelta la reserva sin contarla como no alcanzado (la deja para ir a otro más urgente).</summary>
    public static void Release(Entry e) { if (e != null && e.state == State.Claimed) { e.state = State.Pending; e.by = null; } }

    /// <summary>Se reserva <paramref name="e"/> (si sigue pendiente).</summary>
    public static bool ClaimThis(Entry e, string who) { if (e == null || e.state != State.Pending) return false; e.state = State.Claimed; e.by = who; return true; }

    /// <summary>Se reserva el depredador pendiente más urgente (<see cref="Priority"/>): nadie más irá a por él. null = la lista está hecha.</summary>
    public static Entry Claim(string who, Vector3 from)
    {
        Entry best = null; double bd = double.MaxValue;
        foreach (Entry e in Today())
        {
            if (e.state != State.Pending || e.animal == null) continue;
            double d = Priority(e.animal, e.misses, from);   // (2026-11-11) por urgencia; antes: los no intentados y el más cercano
            if (d < bd) { bd = d; best = e; }
        }
        if (best != null) { best.state = State.Claimed; best.by = who; }
        return best;
    }

    /// <summary>Hecho: se apunta en la lista compartida. Solo <see cref="State.Checked"/> o <see cref="State.Fed"/>.</summary>
    public static void Report(Entry e, State s)
    {
        if (e == null) return;
        e.state = s;
        if (s == State.Fed && e.animal != null) _fedAt[e.animal] = Time.time;
        Debug.Log($"[Alimentador] {e.by}: «{(e.animal != null ? e.animal.name : "?")}» {(s == State.Fed ? "tenía hambre: ración lanzada" : "no tiene hambre: marcado")}. Quedan {Left}.");
    }

    /// <summary>No se pudo atender (no se le alcanzó, se acercó demasiado o no quedan raciones): vuelve a la cola. Nunca sale de la lista.</summary>
    public static void Requeue(Entry e, string why)
    {
        if (e == null) return;
        e.state = State.Pending; e.by = null; e.misses++;
        string msg = $"[Alimentador] «{(e.animal != null ? e.animal.name : "?")}» sigue pendiente: {why} ({e.misses}ª vez).";
        if (e.misses >= 3) Debug.LogWarning(msg); else Debug.Log(msg);
    }

    /// <summary>(2026-11-11) ¿Se le lanza ración? Con hambre y sin una ración reciente: al dar de comer al último, la lista se renueva y el mismo
    /// animal vuelve; con la primera ración aún en el suelo seguía «con hambre» y se le lanzaba otra (la ración DOBLE, no solo el registro).
    /// Una ración vale <see cref="RefeedGameHours"/> h de juego; dentro de ese tiempo se marca comprobado.</summary>
    public static bool ShouldFeed(Animal a) => IsHungry(a) && (Orphan(a) || !(a != null && _fedAt.TryGetValue(a, out float t) && Time.time - t < RefeedSeconds));   // (la huérfana: a demanda, como en la Cría)
    public const float RefeedGameHours = 4f;
    static float RefeedSeconds => (TimeController.timeController != null ? TimeController.timeController.SecondsPerGameDay : 3600f) * RefeedGameHours / 24f;
    static readonly Dictionary<Animal, float> _fedAt = new Dictionary<Animal, float>();

    /// <summary>(2026-11-11) CRÍA HUÉRFANA: una cría que aún no forrajea (<see cref="Metamorphosis.CanForage"/>) sin madre ni padre vivo vinculado
    /// (<see cref="DesireCatalog.ParentOfCub"/>), suelta (no en un recinto de la Cría ni ingresada: a esas las alimentan CriaFeeder y VetCare). Sin
    /// esto no comía nunca: tras 12 h, los dos oseznos sin la osa (muerta de hambre) seguían en el mapa sin una toma. La ronda la recoge y el
    /// alimentador le da el BIBERÓN en la mano (<see cref="BottleFeed"/>; leche de la Cría, no las raciones de carne: INFERIDO).</summary>
    public static bool Orphan(Animal a) => a != null && !a.death && a.lifeStage == LifeStage.child && OrphanSpecies.Contains(a.SpeciesName) && !Metamorphosis.CanForage(a) && BroodRide.ParentalCare(a.LifeSpecies)
        && !DesireCatalog.Confined(a) && DesireCatalog.ParentOfCub(a) == null;

    /// <summary>(2026-11-11) Las especies cuya cría no sobrevive sin sus padres y que el alimentador cría a biberón: oseznos, lobeznos, zorreznos y
    /// cachorros de foca. No los polluelos de pingüino (vivo49: inflaban la lista a 37 y no se les alcanzaba en el hielo). INFERIDO: la documentación
    /// (docs/ciclo-de-vida.md) no dice que la colonia alimente a un polluelo ajeno; en la naturaleza un polluelo sin padres no suele salir adelante.</summary>
    public static readonly HashSet<string> OrphanSpecies = new HashSet<string> { "Bear", "Wolf", "ArcticFox", "Seal" };

    /// <summary>A qué distancia se queda: la del adulto de su especie; a la cría huérfana, en la mano (1,5 m).</summary>
    public static float SafeDistanceFor(Animal a) => Orphan(a) ? 1.5f : SafeDistance(a.SpeciesName);

    /// <summary>El biberón: una toma, como la de la Cría (CriaFeeder): saciada una comida.</summary>
    public static void BottleFeed(Animal a, string by)
    {
        if (a == null || a.death) return;
        a.hungry = a.Body != null ? -a.Body.GetMealWeight(a.Mass) : -0.5f;
        if (a.hungry >= 0f) a.hungry = -0.5f;
        Debug.Log($"[Alimentador] «{by}» da el biberón a «{a.name}» (cría huérfana).");
    }

    /// <summary>(2026-11-11) A partir de aquí «se muere de hambre» (más de una comida de retraso): no espera al turno del alimentador.</summary>
    public const float StarvingAt = 1f;
    /// <summary>Raciones de urgencia dejadas (diagnóstico y pruebas).</summary>
    public static int EmergencyFed { get; private set; }
    static float _nextEmergency;
    /// <summary>(2026-11-11) URGENCIA FUERA DE TURNO: cada 5 s (o ya, con <paramref name="now"/>), a cada depredador de tierra servido que se muere de
    /// hambre (≥ <see cref="StarvingAt"/>) y sin ración reciente, el almacén le deja su ración junto a él, si no hay ningún alimentador de turno.
    /// Suite 63: una osa adulta murió de inanición al empezar (hambre 46) antes del primer turno. Lo llama el laboratorio en cada Update.</summary>
    public static void EmergencyTick(bool now = false)
    {
        if (!now && Time.time < _nextEmergency) return;
        _nextEmergency = Time.time + 5f;
        if (!now) foreach (PredatorFeeder f in Object.FindObjectsByType<PredatorFeeder>(FindObjectsSortMode.None)) if (f != null && f.isActiveAndEnabled) return;   // (de turno: lo lleva él, por urgencia)
        foreach (Animal a in Object.FindObjectsByType<Animal>(FindObjectsSortMode.None))
        {
            if (!Served(a) || Orphan(a) || a.hungry < StarvingAt || !ShouldFeed(a)) continue;
            int n = TakeRations(RationsFor(a.SpeciesName));
            if (n <= 0) { Debug.LogWarning($"[Alimentador] «{a.name}» se muere de hambre (hambre {a.hungry:0.0}) y el almacén está vacío."); continue; }
            Vector3 from = a.transform.position + a.transform.forward * SafeDistanceFor(a);
            Throw(a, n, from);
            _fedAt[a] = Time.time; EmergencyFed++;
            foreach (Entry e in _list) if (e.animal == a && !Done(e)) e.state = State.Fed;
            Debug.Log($"[Alimentador] Urgencia: «{a.name}» ({a.SpeciesName}) se muere de hambre (hambre {a.hungry:0.0}) fuera de turno: el almacén le deja {n} raciones junto a él.");
        }
    }

    /// <summary>Si se le da de comer al llegar: ¿tiene hambre?</summary>
    public static bool IsHungry(Animal a) => a != null && !a.death && a.hungry > HungryAt;

    /// <summary>El punto donde se queda quien va a verlo: en la línea entre el animal y él, a su distancia.</summary>
    public static Vector3 StandOff(Animal a, Vector3 from)
    {
        Vector3 dir = from - a.transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.back;
        return a.transform.position + dir.normalized * SafeDistanceFor(a);
    }

    /// <summary>La ración, lanzada desde <paramref name="from"/>: cae a 3 m del animal, de su lado, y luego la come por su cuenta (Forager la ve como carne).</summary>
    public static FoodItem Throw(Animal a, int rations, Vector3 from)
    {
        Vector3 dir = from - a.transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.back;
        Vector3 at = a.transform.position + dir.normalized * 3f;
        if (Physics.Raycast(at + Vector3.up * 4f, Vector3.down, out RaycastHit h, 10f, ~0, QueryTriggerInteraction.Ignore)) at = h.point + Vector3.up * 0.08f;
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "Racion_carne_cultivada";
        Object.Destroy(g.GetComponent<Collider>());
        g.transform.position = at; g.transform.localScale = new Vector3(0.3f, 0.12f, 0.24f);
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.72f, 0.3f, 0.3f));
        FoodItem fi = g.AddComponent<FoodItem>(); fi.material = OrganicMaterial.Meat; fi.grams = rations * RationKg * 1000f;
        return fi;
    }

    /// <summary>Saca raciones del almacén del laboratorio; las que haya.</summary>
    public static int TakeRations(int want)
    {
        int got = 0;
        foreach (CulturedMeatLab lab in CulturedMeatLab.All)
        {
            if (lab == null) continue;
            int n = Mathf.Min(want - got, lab.rationsStored);
            lab.rationsStored -= n; got += n;
        }
        return got;
    }

    /// <summary>El almacén de raciones (junto al laboratorio).</summary>
    public static Transform Store => CulturedMeatLab.All.Count > 0 && CulturedMeatLab.All[0] != null ? CulturedMeatLab.All[0].feeder : null;

    /// <summary>La visita del jugador (paso con E): el pendiente más cercano. Evalúa, lanza o marca. Devuelve lo que ha hecho.</summary>
    public static string PlayerVisit(GameObject p)
    {
        Entry e = Claim("Kushal", p.transform.position);
        if (e == null) return "La lista de esta ronda ya está hecha.";
        if (!IsHungry(e.animal)) { Report(e, State.Checked); return $"«{e.animal.name}» no tiene hambre: marcado."; }
        int n = TakeRations(RationsFor(e.animal.SpeciesName));
        if (n == 0) { Requeue(e, "tiene hambre y no quedan raciones"); return $"«{e.animal.name}» tiene hambre, pero no quedan raciones: sigue pendiente."; }
        Throw(e.animal, n, p.transform.position);
        Report(e, State.Fed);
        return $"«{e.animal.name}» tenía hambre: {n} raciones lanzadas desde lejos.";
    }
}

