using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// COCINERO VISIBLE (docs/kitchen-simulation.md §3: «cadena de acciones espaciales, no un botón»): convierte cada paso
/// de la receta que le asigna el <see cref="RestaurantKitchen"/> (vía su <see cref="CookingStation"/>) en un trayecto
/// y una acción que SE VEN: va a la nevera o a la despensa y saca el ingrediente (lo lleva en la mano), lo lava en el
/// fregadero, lo corta en su tabla, lo pone en su sartén u olla del fogón y lo remueve, lo pasa al bol, emplata en la
/// isla y deja el plato en la barra de servicio. Cuatro cocineros comparten la cocina, cada uno con su puesto.
/// Avanza el paso de la receta solo cuando termina la acción física (CookingStation.externallyDriven).
/// </summary>
[RequireComponent(typeof(CookingStation))]
public class KitchenCook : MonoBehaviour
{
    [Header("Puesto propio")]
    public Transform stove, pan, pot, board, bowl;
    [Header("Compartido")]
    public Transform[] fridges, pantries;
    public Transform sink, island, pass;
    public float walkSpeed = 1.3f;
    public float actionSeconds = 2.2f;

    CookingStation _cs;
    CreatureRig _rig;
    Transform _target;
    Vector3 _dest;
    float _actUntil;
    string _step;
    Transform _held;          // modelo del ingrediente o plato que lleva en la mano
    Transform _inVessel;      // lo que está cocinándose en su sartén/olla/bol
    Transform _vessel;
    enum Phase { Idle, Walking, Acting }
    Phase _phase;
    float _t;

    public static readonly List<Transform> Served = new List<Transform>();

    void Start() { _cs = GetComponent<CookingStation>(); _cs.externallyDriven = true; _rig = GetComponent<CreatureRig>(); }

    void Update()
    {
        if (_cs == null || _cs.kitchen == null || !_cs.kitchen.Open) return;
        string step = _cs.CurrentStep;
        if (step == null) { _phase = Phase.Idle; return; }
        if (_phase == Phase.Idle || step != _step) Plan(step);

        switch (_phase)
        {
            case Phase.Walking:
                Vector3 to = _dest - transform.position; to.y = 0f;
                if (to.magnitude < 0.12f) { _phase = Phase.Acting; _actUntil = Time.time + actionSeconds; Face(_target); OnArrive(); break; }
                transform.position += to.normalized * Mathf.Min(walkSpeed * Time.deltaTime, to.magnitude);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 10f * Time.deltaTime);
                break;
            case Phase.Acting:
                Gesture();
                if (Time.time >= _actUntil) { OnDone(); _phase = Phase.Idle; _cs.AdvanceStep(); }
                break;
        }
    }

    // ── Paso → estación ─────────────────────────────────────────────────────────────────────────

    enum Act { Take, Wash, Cut, Cook, Stir, Season, Plate }
    Act _act;

    static bool Has(string s, params string[] w) { foreach (string x in w) if (s.Contains(x)) return true; return false; }

    void Plan(string step)
    {
        _step = step;
        string s = step.ToLowerInvariant();
        if (Has(s, "emplata", "sirve", "arma ", "dispón", "enrolla", "corona", "espolvorea", "monta")) { _act = Act.Plate; _target = island; }
        else if (Has(s, "toma", "ve a la nevera", "saca", "coge")) { _act = Act.Take; _target = Pick(IsPantry(s) ? pantries : fridges); }
        else if (Has(s, "lava", "enjuaga")) { _act = Act.Wash; _target = sink; }
        else if (Has(s, "corta", "pica", "pulveriza", "prepara la", "prepara el", "desamarg")) { _act = Act.Cut; _target = board; }
        else if (Has(s, "calienta", "hierve", "cuece", "fríe", "sofríe", "saltea", "tuesta", "asa ", "sella", "vierte y cuece", "haz la salsa", "prepara la masa")) { _act = Act.Cook; _target = stove; }
        else if (Has(s, "revuelve", "bate", "mezcla", "remueve", "agrega", "añade", "integra", "liga", "une ", "incorpora")) { _act = Act.Stir; _target = _inVessel != null && _vessel == bowl ? bowl : (_inVessel != null ? stove : bowl); }
        else if (Has(s, "especia", "condimenta", "rectifica", "aliña", "sazón")) { _act = Act.Season; _target = _target != null ? _target : stove; }
        else { _act = Act.Cut; _target = board; }
        if (_target == null) _target = board != null ? board : stove;
        _dest = Approach(_target);
        _phase = Phase.Walking;
    }

    static bool IsPantry(string s) => Has(s, "avena", "arroz", "quinoa", "lenteja", "garbanzo", "frijol", "teff", "amaranto", "fideo", "lino", "especia", "canela", "comino", "almendra", "maní", "dátil");

    Transform Pick(Transform[] options)
    {
        Transform best = null; float bd = float.MaxValue;
        if (options != null) foreach (Transform t in options) { if (t == null) continue; float d = (t.position - transform.position).sqrMagnitude; if (d < bd) { bd = d; best = t; } }
        return best;
    }

    // Delante de la estación (su -Z local, donde están las puertas/el frente), a 0,7 m, a la altura del cocinero.
    Vector3 Approach(Transform station)
    {
        Transform basis = station;
        // utensilios sobre la encimera: el frente lo marca el mueble que hay debajo (el fogón o el tramo de encimera)
        if (station == pan || station == pot || station == board || station == bowl) basis = station.parent != null ? station.parent : station;
        Vector3 p = basis.position - basis.forward * 0.75f;
        if (station == island || station == pass) p = station.position + (transform.position - station.position).normalized * 0.9f;
        p.y = transform.position.y;
        return p;
    }

    void Face(Transform t)
    {
        if (t == null) return;
        Vector3 d = t.position - transform.position; d.y = 0f;
        if (d.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(d);
    }

    // ── Acciones visibles ───────────────────────────────────────────────────────────────────────

    string IngredientOf(string step)
    {
        string s = step.ToLowerInvariant();
        foreach (string w in new[] { "huevo", "tomate", "banana", "avena", "arroz", "quinoa", "lenteja", "legumbre", "yogur", "leche", "fruta", "verdura", "espinaca", "col" })
            if (s.Contains(w)) return w;
        string dish = (_cs.CurrentDish ?? "").ToLowerInvariant();
        return dish.Contains("huevo") ? "huevo" : dish.Contains("ensalada") ? "verdura" : dish.Contains("avena") ? "avena" : dish.Contains("yogur") ? "yogur" : "verdura";
    }

    void OnArrive()
    {
        switch (_act)
        {
            case Act.Take:
                DropHeld();
                _held = KitchenProps.Ingredient(null, IngredientOf(_step), Vector3.zero);
                Hold(_held);
                Debug.Log($"[Cocina] «{name}» saca {IngredientOf(_step)} de {(_target != null ? _target.name : "?")}.");
                break;
            case Act.Cook:
            case Act.Stir:
                Transform v = _act == Act.Cook ? (Has(_step.ToLowerInvariant(), "hierve", "cuece") ? pot : pan) : (_target == bowl ? bowl : (_vessel != null ? _vessel : pan));
                if (_held != null && v != null) { PutIn(v); }
                break;
        }
    }

    void OnDone()
    {
        switch (_act)
        {
            case Act.Stir:
                if (_inVessel != null && (_step.ToLowerInvariant().Contains("revuelve") || _step.ToLowerInvariant().Contains("bate")))
                    Replace(_inVessel, KitchenProps.Ingredient(null, "huevo revuelto", Vector3.zero));
                break;
            case Act.Cut:
                if (_held != null) Debug.Log($"[Cocina] «{name}» corta {_held.name.Replace("Ingrediente_", "")} en su tabla.");
                break;
            case Act.Plate:
                DropHeld();
                if (_inVessel != null) { Destroy(_inVessel.gameObject); _inVessel = null; }
                Transform dish = KitchenProps.Dish(null, _cs.CurrentDish ?? "platillo", Vector3.zero);
                LeaveAtPass(dish);
                Debug.Log($"[Cocina] «{name}» emplata {_cs.CurrentDish} y lo deja en la barra de servicio.");
                break;
        }
    }

    void PutIn(Transform vessel)
    {
        if (_inVessel != null) Destroy(_inVessel.gameObject);
        _inVessel = _held; _held = null; _vessel = vessel;
        _inVessel.SetParent(vessel, false);
        _inVessel.localPosition = Vector3.up * 0.03f;
        _inVessel.localRotation = Quaternion.identity;
        Debug.Log($"[Cocina] «{name}» pone {_inVessel.name.Replace("Ingrediente_", "")} en {vessel.name}.");
    }

    void Replace(Transform old, Transform with)
    {
        with.SetParent(old.parent, false);
        with.localPosition = old.localPosition;
        Destroy(old.gameObject);
        _inVessel = with;
    }

    void Hold(Transform item)
    {
        Transform hand = _rig != null ? _rig.Get(BodyPart.HandRight) : null;
        item.SetParent(hand != null ? hand : transform, false);
        item.localPosition = hand != null ? new Vector3(0f, -0.1f, 0.05f) : new Vector3(0.3f, 1f, 0.3f);
        item.localRotation = Quaternion.identity;
    }

    void DropHeld() { if (_held != null) { Destroy(_held.gameObject); _held = null; } }

    void LeaveAtPass(Transform dish)
    {
        Transform p = pass != null ? pass : island;
        int slot = Served.Count % 12;
        dish.SetParent(p, true);
        dish.position = p.position + p.right * (-1.1f + (slot % 6) * 0.42f) + p.forward * ((slot / 6) * 0.3f - 0.15f) + Vector3.up * (p.GetComponent<Collider>() != null ? p.GetComponent<Collider>().bounds.max.y - p.position.y : 0.92f);
        Served.Add(dish);
        if (Served.Count > 12) { Transform oldest = Served[0]; Served.RemoveAt(0); if (oldest != null) Destroy(oldest.gameObject); }
    }

    // Brazos: remover en círculo, cortar arriba-abajo, coger/emplatar con el brazo extendido.
    void Gesture()
    {
        if (_rig == null) return;
        _t += Time.deltaTime;
        float x, z = 0f;
        switch (_act)
        {
            case Act.Stir: case Act.Cook: x = -55f + 12f * Mathf.Sin(_t * 6f); z = 12f * Mathf.Cos(_t * 6f); break;
            case Act.Cut: x = -45f + 18f * Mathf.Abs(Mathf.Sin(_t * 8f)); break;
            case Act.Season: x = -70f; z = 10f * Mathf.Sin(_t * 14f); break;
            case Act.Wash: x = -40f + 8f * Mathf.Sin(_t * 9f); break;
            default: x = -60f; break;
        }
        SetBone(BodyPart.ShoulderRight, Quaternion.Euler(x, 0f, z));
        SetBone(BodyPart.ForearmRight, Quaternion.Euler(-30f, 0f, 0f));
        SetBone(BodyPart.ShoulderLeft, Quaternion.Euler(_act == Act.Cut || _act == Act.Wash ? -40f : -15f, 0f, 0f));
    }

    void SetBone(BodyPart p, Quaternion q)
    {
        Transform t = _rig.Get(p);
        if (t == null) return;
        _rig.LockPose(p, 0.2f);
        t.localRotation = q;
    }
}
