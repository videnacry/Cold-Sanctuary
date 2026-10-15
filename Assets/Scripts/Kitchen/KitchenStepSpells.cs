using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HECHIZOS DE COCINA POR PASO (docs/typed-spells-and-queues.md §6.1, kitchen-simulation.md): `take`, `putIn`, `beat`.
/// Cada paso de una receta deja de ser un "step" genérico y pasa a ser un hechizo encolable **ligado a una estación
/// exacta**: se toma DE un ingrediente/nevera (o del contenido de un recipiente), se pone EN un recipiente
/// (olla/bol/plancha/mesa de emplatado) y se bate DENTRO de un recipiente con algo. Cada uno exige **RANGO** = alcance
/// del miembro (2 m + percepción); más lejos solo se llega **con magia** (paga energía de <see cref="MagicReserves"/>
/// por metro extra — la "distancia irreal"). Si el hechizo coincide con el verbo del paso actual de la receta del
/// <see cref="CookingStation"/>, lo AVANZA; si no, igual tiene su efecto físico y el anima recuerda qué toca.
/// </summary>
public abstract class KitchenStepSpell : QueuedSpell
{
    public GameObject target;
    public string targetName = "estación";
    /// <summary>Julios por metro de alcance extra cuando el miembro no llega (alcance a distancia irreal con magia).</summary>
    public static float magicJoulesPerMeter = 50f;

    /// <summary>Verbo para casar con el texto del paso de la receta.</summary>
    protected abstract KitchenStepMatch.Verb Verb { get; }

    /// <summary>Alcance del miembro (mismo criterio que `use` de la consola).</summary>
    public static float LimbReach(Anima self) => 2f + Mathf.Max(0f, self.perception - 1f);

    /// <summary>Distancia horizontal del jugador a la SUPERFICIE de la estación (no a su centro): con una nevera de 1,5 m
    /// de ancho, el centro quedaba fuera del alcance aunque estuvieras pegado a ella.</summary>
    public static float DistanceTo(Anima self, GameObject station)
    {
        Vector3 from = self.transform.position;
        Collider col = station.GetComponentInChildren<Collider>();
        Vector3 to = col != null && !col.isTrigger ? col.ClosestPoint(from) : station.transform.position;
        from.y = 0f; to.y = 0f;
        return Vector3.Distance(from, to);
    }

    /// <summary>¿Llega a la estación? Con el miembro, o pagando energía con magia. `magic` = true si hizo falta magia.</summary>
    public static bool InReach(Anima self, GameObject station, out bool magic)
    {
        magic = false;
        float dist = DistanceTo(self, station);
        float reach = LimbReach(self);
        if (dist <= reach) return true;
        MagicReserves mr = self.GetComponent<MagicReserves>();
        if (mr != null && mr.unlocked && mr.PayEnergy((dist - reach) * magicJoulesPerMeter)) { magic = true; return true; }
        return false;
    }

    /// <summary>Plantilla: comprueba destino y rango, ejecuta el efecto (`Act`, puede durar varios ticks) y, al
    /// terminar con éxito, avanza la receta si el verbo coincide con el paso actual.</summary>
    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        if (target == null) { Say(self, $"¿Dónde está {targetName}?", false); return true; }
        if (!_started)
        {
            if (!InReach(self, target, out bool magic)) { Say(self, $"no alcanzo {targetName} — acércate (walkTo).", false); return true; }
            if (magic) Debug.Log($"[Cocina] «{self.name}» alcanza {targetName} con magia.");
            if (!Begin(self, out string why)) { Say(self, why, false); return true; }
            _started = true;
        }
        if (!Act(self)) return false;                       // aún en curso (batir lleva tiempo)
        AdvanceRecipe(self);
        return true;
    }
    bool _started;

    /// <summary>Validación + efecto inmediato. false = falla con `why`.</summary>
    protected abstract bool Begin(Anima self, out string why);
    /// <summary>Efecto continuo; true cuando termina. Por defecto, instantáneo.</summary>
    protected virtual bool Act(Anima self) => true;

    void AdvanceRecipe(Anima self)
    {
        CookingStation cs = self.GetComponent<CookingStation>();
        string step = cs != null ? cs.CurrentStep : null;
        if (step == null) return;
        if (KitchenStepMatch.Matches(step, Verb)) cs.AdvanceStep();
        else Say(self, $"hecho, pero el paso que toca es «{step}».", true);
    }

    /// <summary>Feedback: log + pensamiento del anima (la UI de pensamiento lo muestra).</summary>
    protected static void Say(Anima self, string msg, bool positive)
    {
        Debug.Log($"[Cocina] «{self.name}»: {msg}");
        Mind m = self.GetComponent<Mind>();
        if (m != null) { m.lastThought = msg; m.lastPositive = positive; m.lastIntensity = 0.5f; m.lastThoughtTime = Time.time; }
    }
}

/// <summary>TOMAR: de un ingrediente/nevera (fuente infinita, no se mueve) o el contenido de un recipiente.
/// Las manos solo sostienen una cosa: hay que `putIn` antes de tomar otra. `take("Nevera")` sin `item` solo la abre.</summary>
public class TakeSpell : KitchenStepSpell
{
    public string item;   // opcional: qué sacar de la nevera ("huevos"); si no, el nombre del destino
    public override string Label => $"tomar {item ?? targetName}";
    protected override KitchenStepMatch.Verb Verb => KitchenStepMatch.Verb.Take;

    protected override bool Begin(Anima self, out string why)
    {
        KitchenHands hands = KitchenHands.Of(self);
        if (string.IsNullOrEmpty(item) && target.name.ToLowerInvariant().Contains("nevera"))
        { why = null; Debug.Log($"[Cocina] «{self.name}» abre la nevera."); return true; }   // abrir sin sacar nada
        if (hands.held != null) { why = $"ya tengo {hands.held} en las manos — ponlo en algún sitio (putIn)."; return false; }
        KitchenVessel v = target.GetComponent<KitchenVessel>();
        if (v != null || KitchenVessel.LooksLikeVessel(target.name))
        {
            if (v == null || v.contents.Count == 0) { why = $"{targetName} está vacío."; return false; }
            hands.held = v.Describe(); v.Clear();          // se lleva TODO el contenido (pasar de la olla al bol)
        }
        else hands.held = string.IsNullOrEmpty(item) ? target.name : item;
        why = null;
        Debug.Log($"[Cocina] «{self.name}» toma {hands.held}.");
        return true;
    }
}

/// <summary>PONER EN: deja lo que sostiene dentro de un RECIPIENTE (olla/bol/plancha/mesa de emplatado).</summary>
public class PutInSpell : KitchenStepSpell
{
    public override string Label => $"poner en {targetName}";
    protected override KitchenStepMatch.Verb Verb => KitchenStepMatch.Verb.PutIn;

    protected override bool Begin(Anima self, out string why)
    {
        KitchenHands hands = KitchenHands.Of(self);
        if (hands.held == null) { why = "no tengo nada en las manos (take primero)."; return false; }
        KitchenVessel v = KitchenVessel.Of(target);
        if (v == null) { why = $"{targetName} no es un recipiente (olla, bol, plancha o mesa)."; return false; }
        v.contents.Add(hands.held);
        Debug.Log($"[Cocina] «{self.name}» pone {hands.held} en {targetName}.");
        hands.held = null;
        why = null;
        return true;
    }
}

/// <summary>BATIR/REVOLVER: dentro de un recipiente con algo. Lleva TIEMPO: menos con más fuerza y agilidad.</summary>
public class BeatSpell : KitchenStepSpell
{
    public float baseSeconds = 2.5f;
    float _until;
    KitchenVessel _v;

    public override string Label => $"batir en {targetName}";
    protected override KitchenStepMatch.Verb Verb => KitchenStepMatch.Verb.Beat;

    protected override bool Begin(Anima self, out string why)
    {
        _v = target.GetComponent<KitchenVessel>();
        if (_v == null || _v.contents.Count == 0) { why = $"no hay nada que batir en {targetName}."; return false; }
        float skill = Mathf.Max(0.25f, 0.5f * (self.strength + self.agility));
        _until = Time.time + baseSeconds / skill;
        why = null;
        return true;
    }

    protected override bool Act(Anima self)
    {
        if (Time.time < _until) return false;
        _v.beaten = true;
        Debug.Log($"[Cocina] «{self.name}» bate {_v.Describe()} en {targetName}.");
        return true;
    }
}

/// <summary>Lo que el anima sostiene en las manos al cocinar (una cosa a la vez). Se añade solo.</summary>
public class KitchenHands : MonoBehaviour
{
    public string held;
    public static KitchenHands Of(Anima self) => self.GetComponent<KitchenHands>() ?? self.gameObject.AddComponent<KitchenHands>();
}

/// <summary>Un RECIPIENTE de cocina y su contenido. Las primitivas del restaurante cuyo nombre es de recipiente
/// (olla/bol/plancha/sartén/mesa/plato) lo reciben solas al primer `putIn`.</summary>
public class KitchenVessel : MonoBehaviour
{
    public List<string> contents = new List<string>();
    public bool beaten;

    static readonly string[] VesselWords = { "olla", "bol", "plancha", "sart", "mesa", "plato" };

    public static bool LooksLikeVessel(string name)
    {
        string n = name.ToLowerInvariant();
        foreach (string w in VesselWords) if (n.Contains(w)) return true;
        return false;
    }

    public static KitchenVessel Of(GameObject go)
    {
        KitchenVessel v = go.GetComponent<KitchenVessel>();
        if (v == null && LooksLikeVessel(go.name)) v = go.AddComponent<KitchenVessel>();
        return v;
    }

    public string Describe() => (beaten ? "batido de " : "") + string.Join(" + ", contents);
    public void Clear() { contents.Clear(); beaten = false; }
}

/// <summary>Casa un hechizo de cocina con el TEXTO de un paso de receta por su verbo ("toma los huevos" ↔ take,
/// "revuelve los huevos" ↔ beat, "agrega la fruta" ↔ putIn). Los pasos sin verbo de estos (cortar, calentar…)
/// siguen avanzando con `step`/`use`.</summary>
public static class KitchenStepMatch
{
    public enum Verb { Take, PutIn, Beat }

    static readonly string[] TakeWords  = { "toma", "ve a", "agarra", "coge", "saca" };
    static readonly string[] PutInWords = { "agrega", "añade", "vierte", "pon ", "integra", "incorpora", "emplata",
                                            "dispón", "monta", "une ", "sirve", "arma", "compón", "enrolla", "corona", "espolvorea" };
    static readonly string[] BeatWords  = { "revuelve", "bate", "mezcla", "remueve", "tritura", "licúa", "liga" };

    public static bool Matches(string step, Verb verb)
    {
        if (string.IsNullOrEmpty(step)) return false;
        string s = step.ToLowerInvariant() + " ";
        string[] words = verb == Verb.Take ? TakeWords : verb == Verb.PutIn ? PutInWords : BeatWords;
        foreach (string w in words) if (s.StartsWith(w) || s.Contains(" " + w)) return true;
        return false;
    }
}
