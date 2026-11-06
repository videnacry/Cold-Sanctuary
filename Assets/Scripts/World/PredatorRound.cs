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
        if (a == null || a.death || a.lifeStage == LifeStage.child || a.Forage == null || !a.Forage.eatsPrey) return false;
        if (a.GetComponent<MarineLife>() != null || a.GetComponent<IceInsectLife>() != null || a.GetComponentInParent<CriaFeeder>() != null) return false;
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
    public static void NewRound() { _list.Clear(); _nextScan = 0f; }

    public static int Left { get { int n = 0; foreach (Entry e in Today()) if (!Done(e)) n++; return n; } }

    /// <summary>Se reserva el depredador pendiente más cercano: nadie más irá a por él. null = la lista está hecha.</summary>
    public static Entry Claim(string who, Vector3 from)
    {
        Entry best = null; float bd = float.MaxValue;
        foreach (Entry e in Today())
        {
            if (e.state != State.Pending) continue;
            // Los que aún no se han intentado van antes; luego, el más cercano.
            float d = e.misses * 1000000f + (e.animal.transform.position - from).sqrMagnitude;
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

    /// <summary>Si se le da de comer al llegar: ¿tiene hambre?</summary>
    public static bool IsHungry(Animal a) => a != null && !a.death && a.hungry > HungryAt;

    /// <summary>El punto donde se queda quien va a verlo: en la línea entre el animal y él, a su distancia.</summary>
    public static Vector3 StandOff(Animal a, Vector3 from)
    {
        Vector3 dir = from - a.transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.back;
        return a.transform.position + dir.normalized * SafeDistance(a.SpeciesName);
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

