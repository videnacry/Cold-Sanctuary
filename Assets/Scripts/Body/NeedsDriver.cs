using UnityEngine;

/// <summary>
/// EL CUERPO PIDE (etapa 2 del cuerpo unificado): cuando el metabolismo (<see cref="BodyMetabolism"/>) aprieta, el
/// cuerpo mete su necesidad en el carril PROPIO de la cola (que manda sobre el del jugador y sobre el merodeo):
/// hambre → ir a la comida más cercana y comer; sueño → dormir hasta descansar (una amenaza lo despierta).
/// Para humanos y compañeros; los animales siguen con su Volición por ahora.
/// </summary>
[RequireComponent(typeof(ActionQueue))]
public class NeedsDriver : MonoBehaviour
{
    [Range(0f, 1f)] public float eatAbove = 0.6f;
    [Range(0f, 1f)] public float sleepAbove = 0.85f;
    public float checkEvery = 2f;

    float _next;
    Anima _a;
    BodyMetabolism _body;
    ActionQueue _q;

    void Awake()
    {
        _a = GetComponent<Anima>();
        _body = GetComponent<BodyMetabolism>();
        _q = GetComponent<ActionQueue>();
    }

    void Update()
    {
        if (_a == null || _body == null || _a.death || Time.time < _next) return;
        if (Orchestra.Busy(gameObject)) return;   // en una iniciación (examen, clase, cocina, huerto o acompañándolo): sus necesidades esperan (se acaba con tope)
        _next = Time.time + checkEvery;
        foreach (QueuedSpell s in _q.Own) if (s is EatSpell || s is SleepSpell || s is DineSpell || s is ToiletSpell) return;   // ya atendiendo una necesidad

        if (!_a.asleep && _a.sleepiness >= sleepAbove)
        {
            // A dormir a SU cama (arboleda residencial), si tiene dormitorio; si no, donde esté.
            Bedroom room = Bedroom.Of(_a);
            if (room != null && room.bed != null && Vector3.Distance(room.bed.position, transform.position) > 2f)
                _q.EnqueueOwn(new CarpetWalkSpell { target = room.bed, targetName = $"su cama ({room.name})", stopDistance = 1f });   // a su casa del árbol, en alfombra
            _q.EnqueueOwn(new SleepSpell());
            return;
        }
        if (!_a.asleep && _body.toilet >= 0.85f && ToiletSpell.CanTry(_a)) { _q.EnqueueOwn(new ToiletSpell()); return; }   // el cuerpo pide ir al baño: busca los aseos
        if (!_a.asleep && _body.hunger >= eatAbove) _q.EnqueueOwn(new DineSpell());   // a comer al restaurante (pedir, sentarse, comer)
    }
}

/// <summary>COMER: camina (por el NavMesh) a la comida más cercana con raciones, toma una y sacia el cuerpo.</summary>
public class EatSpell : QueuedSpell
{
    public float meal = 0.7f;
    FoodContainer _food;
    WalkToSpell _walk;

    public override string Label => _food != null ? $"comer de {_food.name}" : "buscar comida";

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        if (_food == null || !_food.HasFood)
        {
            _food = Nearest(self.transform.position);
            if (_food == null) { Debug.Log($"[Cuerpo] «{self.name}» tiene hambre pero no hay comida en ningún sitio."); return true; }
            _walk = new WalkToSpell { target = _food.transform, targetName = _food.name, stopDistance = 1.2f };
        }
        if (!_walk.Tick(self)) return false;   // aún de camino

        BodyMetabolism body = self.GetComponent<BodyMetabolism>();
        if (_food.Take())
        {
            if (body != null) body.Feed(meal);
            Debug.Log($"[Cuerpo] «{self.name}» come {_food.dishName} de «{_food.name}».");
        }
        return true;
    }

    static FoodContainer Nearest(Vector3 from)
    {
        FoodContainer best = null; float bestSq = float.MaxValue;
        foreach (FoodContainer fc in Object.FindObjectsByType<FoodContainer>(FindObjectsSortMode.None))
        {
            if (fc == null || !fc.HasFood) continue;
            float sq = (fc.transform.position - from).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = fc; }
        }
        return best;
    }
}

/// <summary>DORMIR: se queda dormido hasta descansar (sueño &lt; 0,1); una amenaza (`aware`) lo despierta.</summary>
public class SleepSpell : QueuedSpell
{
    bool _started;
    public override string Label => "dormir";

    [Tooltip("Hora a la que se despierta tras el salto temporal (amanecer).")]
    public const float WakeHour = 6.5f;

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        if (!_started)
        {
            _started = true; self.asleep = true;
            // EN SU CAMA: se tumba boca arriba encima y cierra los ojos (si la tiene a mano; si no, duerme donde esté).
            Bedroom room = Bedroom.Of(self);
            if (room != null && room.bed != null && Vector3.Distance(room.bed.position, self.transform.position) < 3f)
            {
                CharacterController cc = self.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false;
                // Sobre el colchón (su cara de arriba está a +0,50 de la raíz de la cama) y con la cabeza en la almohada: la
                // pose Lie pone la cabeza hacia −z del cuerpo y la almohada está en +z de la cama, así que se gira 180°.
                self.transform.position = room.bed.position + Vector3.up * 0.5f;
                self.transform.rotation = room.bed.rotation * Quaternion.Euler(0f, 180f, 0f);
                if (cc != null) cc.enabled = true;
                DailyPoses.Lie().ApplyFinal(self);
                Debug.Log($"[Cuerpo] «{self.name}» se acuesta en su cama ({room.name}).");
            }
            self.GetComponentInChildren<CubeFace>()?.Show(CubeFace.Expression.Asleep, 3600f);
            Debug.Log($"[Cuerpo] «{self.name}» se duerme.");
            // SALTO TEMPORAL: si es el cuerpo que maneja el JUGADOR, el juego pasa directo al amanecer (no se corta).
            // La claridad va atada al reloj (TimeController.IsDay), así que al fijar la hora cambia el día/noche.
            // (Kushal/Magnate no duermen por ahora, así que no disparan esto.)
            if (self.GetComponent<PlayerBrain>() != null && TimeController.timeController != null)
            {
                TimeController.timeController.SetHour(WakeHour);
                self.sleepiness = 0f;   // despierta descansado al amanecer
                Debug.Log($"[Tiempo] «{self.name}» duerme → salto al amanecer ({WakeHour:0.0} h).");
            }
        }
        if (!(self.aware || self.sleepiness < 0.1f) && Bedroom.Of(self) != null) DailyPoses.Lie().ApplyFinal(self);   // sigue tumbado
        if (self.aware || self.sleepiness < 0.1f)
        {
            self.asleep = false;
            DailyPoses.Stand().ApplyFinal(self);
            self.GetComponentInChildren<CubeFace>()?.Show(CubeFace.Expression.Neutral, 0.1f);
            Debug.Log($"[Cuerpo] «{self.name}» se despierta{(self.aware ? " (amenaza)" : " descansado")}.");
            return true;
        }
        return false;
    }
}
