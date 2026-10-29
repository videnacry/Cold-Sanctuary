using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VIDA DIARIA del cuerpo humano (docs/vida-diaria.md): comer en el RESTAURANTE (pedir en la barra, sentarse a una mesa
/// y comer), ir al BAÑO (los aseos de cada área) y DORMIR en su cama (tumbado, con los ojos cerrados). Lo dispara
/// <see cref="NeedsDriver"/> cuando el metabolismo aprieta. Las posturas (sentado, tumbado, de pie) son las mismas asanas
/// de cubos del yoga.
/// </summary>
public static class DailyPoses
{
    static readonly BodyPart[] All =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head,
        BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft, BodyPart.ForearmRight, BodyPart.HandLeft, BodyPart.HandRight,
        BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };

    internal static AsanaPoseSpell Make(string name, float root, params (BodyPart, Vector3)[] set)
    {
        var parts = new (BodyPart, Vector3)[All.Length];
        for (int i = 0; i < All.Length; i++)
        {
            Vector3 e = Vector3.zero;
            foreach (var s in set) if (s.Item1 == All[i]) e = s.Item2;
            parts[i] = (All[i], e);
        }
        return new AsanaPoseSpell { asanaName = name, parts = parts, duration = 0.5f, reachFraction = 1f, rootHeight = root };
    }

    /// <summary>Sentado en una silla: muslos horizontales, rodillas a 90°, manos sobre la mesa.</summary>
    public static AsanaPoseSpell Sit() => Make("sentado", 0.5f, (BodyPart.ThighLeft, new Vector3(-90, 0, 0)), (BodyPart.ThighRight, new Vector3(-90, 0, 0)),
        (BodyPart.ShinLeft, new Vector3(90, 0, 0)), (BodyPart.ShinRight, new Vector3(90, 0, 0)),
        (BodyPart.ShoulderLeft, new Vector3(-40, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-40, 0, 0)), (BodyPart.ForearmLeft, new Vector3(-50, 0, 0)), (BodyPart.ForearmRight, new Vector3(-50, 0, 0)));
    /// <summary>Tumbado boca arriba (en la cama).</summary>
    public static AsanaPoseSpell Lie() => Make("tumbado", 0.12f, (BodyPart.Hips, new Vector3(-90, 0, 0)));
    /// <summary>Meditación (piernas cruzadas, espalda recta, manos en el regazo): Sadhguru en la plataforma en su tiempo libre.</summary>
    public static AsanaPoseSpell Meditate() => Make("meditación", 0.5f, (BodyPart.ThighLeft, new Vector3(-80, 0, 38)), (BodyPart.ThighRight, new Vector3(-80, 0, -38)),
        (BodyPart.ShinLeft, new Vector3(140, 0, 0)), (BodyPart.ShinRight, new Vector3(140, 0, 0)),
        (BodyPart.ShoulderLeft, new Vector3(-20, 0, 8)), (BodyPart.ShoulderRight, new Vector3(-20, 0, -8)), (BodyPart.ForearmLeft, new Vector3(-60, 0, -30)), (BodyPart.ForearmRight, new Vector3(-60, 0, 30)));
    /// <summary>De pie (deshace sentarse o tumbarse).</summary>
    public static AsanaPoseSpell Stand() => Make("de pie", -1f);

    // ── EXAMEN DE INICIACIÓN (enfermería): posturas del que EXAMINA y del EXAMINADO ──
    /// <summary>Paciente sentado con el hombro izquierdo descubierto hacia fuera (vacuna en el deltoides).</summary>
    public static AsanaPoseSpell PatientShoulder() => Make("hombro para la vacuna", 0.5f, (BodyPart.ThighLeft, new Vector3(-90, 0, 0)), (BodyPart.ThighRight, new Vector3(-90, 0, 0)),
        (BodyPart.ShinLeft, new Vector3(90, 0, 0)), (BodyPart.ShinRight, new Vector3(90, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(0, 0, 25)), (BodyPart.Neck, new Vector3(0, 25, 0)),
        (BodyPart.ShoulderRight, new Vector3(-35, 0, 0)), (BodyPart.ForearmRight, new Vector3(-55, 0, 0)));
    /// <summary>El que vacuna: brazo derecho a la altura del hombro del paciente, con la jeringa.</summary>
    public static AsanaPoseSpell ExaminerSyringe() => Make("vacunar", -1f, (BodyPart.Back, new Vector3(10, 0, 0)),
        (BodyPart.ShoulderRight, new Vector3(-80, 0, 0)), (BodyPart.ForearmRight, new Vector3(-45, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-60, 0, 15)), (BodyPart.ForearmLeft, new Vector3(-30, 0, 0)));
}

/// <summary>COMER EN EL RESTAURANTE: va a la barra de servicio (el pase del centro de la cocina, por el lado de las alfombras
/// de la planta), espera a que le sirvan, toma una ración de la carta, sube al comedor del anillo, se sienta a una mesa
/// libre, come con calma (~20 min de juego) y se levanta. Si no hay restaurante, come donde haya comida.
/// Cada tramo va a UN destino fijo sobre el NavMesh (como <see cref="RestaurantService.Mover"/>), con PathMover y alfombras,
/// y todo tiene tope: si no avanza, lo deja (JornadaTest 2026-10-29: «pedir en la barra» atascado 100 s y «buscar mesa» en
/// círculos: iba a cualquier FoodContainer —también los de las obras— y a la barra y de vuelta a la misma pista sin pausa).</summary>
public class DineSpell : QueuedSpell
{
    public float meal = 0.75f;
    public float eatMinutes = 20f;
    public float serveSeconds = 22f;        // le sirven en la barra (~9 min de juego). Más que la ventana de 20 s del vigía: la ida a la barra y la vuelta a la MISMA pista de alfombras (el comedor está en el anillo) no se leen como una vuelta en círculo
    public float barTimeout = 150f, seatTimeout = 120f;   // de otra torre a la barra hay varios vuelos en alfombra
    const float NoProgress = 7f;            // s sin acercarse (sin volar): recalcula; la segunda vez, lo deja
    static readonly Dictionary<Transform, float> _taken = new Dictionary<Transform, float>();   // silla → hasta cuándo (si alguien la deja a medias, caduca)

    int _phase;              // 0 ir a la barra · 1 le sirven · 2 ir a la mesa · 3 comer · 4 levantarse
    Transform _seat;
    float _until;
    string _dish;
    // El tramo en curso: un destino fijo, ya sobre el NavMesh.
    Vector3 _snap; bool _hasGoal; float _legStart, _checkAt, _bestLeft; Vector3 _checkPos; int _restarts;

    public override string Label => _phase switch { 0 => "pedir en la barra del restaurante", 1 => "esperar su plato en la barra", 2 => "buscar mesa", 3 => $"comer {_dish}", _ => "levantarse" };

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        switch (_phase)
        {
            case 0:
            {
                if (!_hasGoal)
                {
                    FoodContainer any = NearestFood(self.transform.position);
                    if (any == null) { Debug.Log($"[Cuerpo] «{self.name}» tiene hambre pero no hay comida en ningún sitio."); return true; }
                    BeginLeg(self, BarPoint(any, self));
                }
                int r = Walk(self, barTimeout, "la barra", 3f);   // (en la barra, entre la gente que pide: a 3 m ya le atienden; Kushal se quedaba a 2 m tras otro y lo dejaba)
                if (r == 0) return false;
                if (r < 0) { ReleaseBarSlot(self); return true; }   // no llega: lo deja (la necesidad volverá a pedirlo); nunca toma la comida desde lejos
                _phase = 1; _until = Time.time + serveSeconds; _hasGoal = false;
                FoodContainer near = NearestFood(self.transform.position);
                if (near != null) Face(self.transform, near.transform.position);
                return false;
            }
            case 1:
            {
                if (Time.time < _until) return false;
                ReleaseBarSlot(self);
                FoodContainer fc = NearestFood(self.transform.position);
                if (fc == null || (fc.transform.position - self.transform.position).sqrMagnitude > 36f || !fc.Take())
                { Debug.Log($"[Restaurante] «{self.name}» llega a la barra pero ya no queda comida."); return true; }
                _dish = fc.dishName;
                Debug.Log($"[Restaurante] «{self.name}» pide en la barra y le sirven {_dish}.");
                _seat = FreeSeat();
                if (_seat == null) { _phase = 3; _until = Time.time + EatSeconds(); return false; }
                _taken[_seat] = Time.time + 600f;
                BeginLeg(self, ChairFront(_seat));
                _phase = 2;
                return false;
            }
            case 2:
            {
                int r = Walk(self, seatTimeout, $"una mesa ({_seat?.parent?.name})");
                if (r == 0) return false;
                if (r < 0 || _seat == null)   // no llegó a la silla: come de pie donde está (antes se le colocaba en ella desde lejos)
                { if (_seat != null) _taken.Remove(_seat); _seat = null; _until = Time.time + EatSeconds(); _phase = 3; return false; }
                Vector3 table = _seat.parent != null ? _seat.parent.position : _seat.position;
                Vector3 look = table - _seat.position; look.y = 0f;
                self.transform.position = new Vector3(_seat.position.x, self.transform.position.y, _seat.position.z);   // de delante de la silla a la silla (~0,5 m)
                if (look.sqrMagnitude > 1e-4f) self.transform.rotation = Quaternion.LookRotation(look);
                DailyPoses.Sit().ApplyFinal(self);
                Debug.Log($"[Restaurante] «{self.name}» se sienta a comer en {_seat.parent?.name}.");
                _until = Time.time + EatSeconds();
                _phase = 3;
                return false;
            }
            case 3:
            {
                if (_seat != null) DailyPoses.Sit().ApplyFinal(self);   // sigue sentado (las asanas bloquean los huesos un instante)
                if (Time.time < _until) return false;
                BodyMetabolism body = self.GetComponent<BodyMetabolism>();
                if (body != null) { body.Feed(meal); body.toilet = Mathf.Clamp01(body.toilet + 0.25f); }
                Debug.Log($"[Cuerpo] «{self.name}» termina de comer {_dish}.");
                _phase = 4;
                return false;
            }
            default:
                DailyPoses.Stand().ApplyFinal(self);
                if (_seat != null) _taken.Remove(_seat);
                return true;
        }
    }

    float EatSeconds() => eatMinutes * (TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 1f);

    // ── El tramo: un destino fijo sobre el NavMesh, PathMover (y sus alfombras), con tope y sin dar vueltas ──

    void BeginLeg(Anima self, Vector3 goal)
    {
        _hasGoal = true; _restarts = 0;
        // EL DESTINO, SOBRE EL NAVMESH (como RestaurantService.Mover): junto a un mueble el punto pedido cae dentro del margen
        // de su obstáculo y el camino acababa a medio metro: nunca «llegaba» y rondaba el mueble.
        _snap = UnityEngine.AI.NavMesh.SamplePosition(goal, out UnityEngine.AI.NavMeshHit h, 1.2f, UnityEngine.AI.NavMesh.AllAreas) && Mathf.Abs(h.position.y - goal.y) < 0.8f ? h.position : goal;
        _legStart = _checkAt = Time.time; _checkPos = self.transform.position; _bestLeft = float.MaxValue;
        PathMover.Of(self.transform).Restart();
    }

    /// <summary>1 = llegó · 0 = sigue · −1 = no llega (tope, o sin acercarse).</summary>
    int Walk(Anima self, float timeout, string what, float nearEnough = 1.5f)
    {
        Transform t = self.transform;
        PathMover pm = PathMover.Of(t);
        bool riding = FlyingCarpet.Riding(self.gameObject);
        Vector3 here = t.position - _snap; float hy = here.y - pm.PivotHeight; here.y = 0f;
        if (!riding && here.magnitude < 0.45f && Mathf.Abs(hy) < 1.3f) return 1;
        float left = pm.StepToward(_snap, 1.6f);
        riding = FlyingCarpet.Riding(self.gameObject);
        bool level = Mathf.Abs(t.position.y - pm.PivotHeight - _snap.y) < 1.3f;
        if (!riding && level && left < 0.45f) return 1;
        if (Time.time - _legStart > timeout)
        { Debug.Log($"[Restaurante] «{self.name}» no llega a {what} en {timeout:0} s (a {left:0.0} m): lo deja."); return -1; }
        if (riding) { _checkAt = Time.time; _checkPos = t.position; _bestLeft = float.MaxValue; return 0; }   // volando: lo lleva la alfombra
        if (level ? left < _bestLeft - 0.5f : (t.position - _checkPos).magnitude > 1f)   // en su piso, acercarse; en otro, andar hacia sus alfombras
        { _bestLeft = level ? left : float.MaxValue; _checkAt = Time.time; _checkPos = t.position; }
        if (Time.time - _checkAt > NoProgress)
        {
            // SIN ACERCARSE en 10 s (algo lo sujeta, o el camino rodea sin llegar): a menos de 1,5 m del sitio basta; si no,
            // recalcula una vez y, si sigue igual, lo deja (antes esperaba 90 s quieto o daba vueltas hasta rendirse).
            if (level && left < nearEnough) return 1;
            if (_restarts++ == 0) { pm.Restart(); _checkAt = Time.time; _checkPos = t.position; _bestLeft = left; return 0; }
            Debug.Log($"[Restaurante] «{self.name}» no avanza hacia {what} (a {left:0.0} m; se movió {(t.position - _checkPos).magnitude:0.0} m en {NoProgress:0} s): lo deja.");
            return -1;
        }
        return 0;
    }

    static void Face(Transform t, Vector3 p) { Vector3 d = p - t.position; d.y = 0f; if (d.sqrMagnitude > 1e-4f) t.rotation = Quaternion.LookRotation(d); }

    // ── Sitios ──

    // LA COMIDA de la carta del restaurante (los contenedores de RestaurantKitchen, en el pase). Antes valía CUALQUIER
    // FoodContainer —también la pila de la obra o el de «mantenimiento»— y se iba a «pedir» a otra torre o a un mueble.
    static FoodContainer NearestFood(Vector3 from)
    {
        FoodContainer best = null; float bestSq = float.MaxValue;
        bool restaurant = false;
        foreach (RestaurantKitchen rk in Object.FindObjectsByType<RestaurantKitchen>(FindObjectsSortMode.None))
        {
            if (rk == null || rk.dishes == null) continue;
            foreach (RestaurantKitchen.DishStock d in rk.dishes)
            {
                FoodContainer fc = d != null ? d.container : null;
                if (fc == null) continue;
                restaurant = true;
                if (!fc.HasFood) continue;
                float sq = (fc.transform.position - from).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; best = fc; }
            }
        }
        if (restaurant) return best;
        foreach (FoodContainer fc in Object.FindObjectsByType<FoodContainer>(FindObjectsSortMode.None))   // sin restaurante: donde haya comida
        {
            if (fc == null || !fc.HasFood) continue;
            float sq = (fc.transform.position - from).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = fc; }
        }
        return best;
    }

    // DONDE SE PIDE: a 1 m del pase, por el lado de las alfombras de la planta (por donde se llega del comedor o de otra torre;
    // el mismo lado que usa el camarero), repartidos a lo largo de la barra para no pisarse.
    static Vector3 BarPoint(FoodContainer fc, Anima self)
    {
        Transform pass = fc.transform.parent;
        if (pass == null || !pass.name.StartsWith("Barra"))
        {
            Vector3 d = self.transform.position - fc.transform.position; d.y = 0f; d = d.sqrMagnitude > 1e-4f ? d.normalized : Vector3.forward;
            Vector3 q = fc.transform.position + d * 1.2f; q.y = fc.transform.position.y - 1f;
            return q;
        }
        CarpetPad pad = CarpetPad.For(pass.position + Vector3.up * 0.5f);
        Vector3 from = pad != null ? pad.transform.position : self.transform.position;
        float side = Vector3.Dot(from - pass.position, pass.forward) >= 0f ? 1f : -1f;
        // UN SITIO POR PERSONA (JornadaTest 2026-10-29: a las 11:00 llegan diez a la vez y con cinco sitios por el nombre
        // caían dos en el mismo; el segundo no llegaba nunca a su punto y se quedaba quieto hasta rendirse). Cinco a lo largo
        // de la barra y, si están todos, una segunda fila detrás; el sitio caduca solo si alguien lo deja a medias.
        int slot = ClaimBarSlot(self);
        float along = (BarOrder[slot % 5] - 2) * 0.9f, depth = 1.0f + (slot / 5) * 0.9f;
        Vector3 p = pass.position + pass.forward * side * depth + pass.right * along; p.y = pass.position.y;
        return p;
    }

    static readonly int[] BarOrder = { 2, 1, 3, 0, 4 };   // del centro a los extremos
    static readonly Dictionary<int, (Anima who, float until)> _bar = new Dictionary<int, (Anima, float)>();

    static int ClaimBarSlot(Anima self)
    {
        ReleaseBarSlot(self);
        int free = -1;
        for (int i = 0; i < 10 && free < 0; i++)
            if (!_bar.TryGetValue(i, out var c) || c.who == null || c.until < Time.time) free = i;
        if (free < 0) free = Mathf.Abs(self.name.GetHashCode()) % 10;   // más de diez a la vez: comparte
        _bar[free] = (self, Time.time + 200f);
        return free;
    }

    static void ReleaseBarSlot(Anima self)
    {
        foreach (var kv in new List<KeyValuePair<int, (Anima who, float until)>>(_bar)) if (kv.Value.who == self) _bar.Remove(kv.Key);
    }

    // Por fuera de la silla (como RestaurantService.ChairFront): ahí se pone quien se va a sentar. El centro de la silla queda
    // dentro del obstáculo de la mesa y el camino rondaba la mesa sin llegar.
    static Vector3 ChairFront(Transform chair)
    {
        Transform mesa = chair.parent;
        Vector3 o = chair.position - mesa.position; o.y = 0f; o = o.sqrMagnitude > 1e-4f ? o.normalized : mesa.forward;
        Vector3 p = chair.position + o * 0.55f; p.y = mesa.position.y;
        return p;
    }

    // Una silla libre del comedor («Asiento» de una «Mesa_»), la más cercana a las alfombras del anillo (por donde se llega):
    // ni la de otro del personal ni la mesa de un turista.
    static Transform FreeSeat()
    {
        GameObject dining = GameObject.Find("Comedor_Gourmet");
        if (dining == null) return null;
        Vector3 from = dining.transform.position;
        Transform area = dining.transform.parent;
        if (area != null) foreach (CarpetPad p in CarpetPad.All) if (p != null && p.surface.Contains("#anillo") && p.transform.IsChildOf(area)) { from = p.transform.position; break; }
        var busy = new HashSet<Transform>();
        if (RestaurantService.Instance != null)
            foreach (RestaurantService.Table t in RestaurantService.Instance.Tables) if (t.occupant != null || t.cleaner != null) busy.Add(t.mesa);
        Transform best = null; float bestSq = float.MaxValue;
        foreach (Transform t in dining.GetComponentsInChildren<Transform>())
        {
            if (t.name != "Asiento" || t.parent == null || !t.parent.name.StartsWith("Mesa_") || busy.Contains(t.parent)) continue;   // las sillas de las MESAS (el «Asiento» del inodoro de los aseos también se llama así)
            if (_taken.TryGetValue(t, out float until) && until > Time.time) continue;
            float sq = (t.position - from).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = t; }
        }
        return best;
    }
}

/// <summary>IR AL BAÑO: busca los aseos más cercanos, entra en una cabina (puerta cerrada), usa el inodoro seco (tapa con
/// serrín), se lava las manos y sale. Si es el jugador, la cámara pasa a panorámica mientras entra y sale.</summary>
public class ToiletSpell : QueuedSpell
{
    /// <summary>Quien no llegó a los aseos lo vuelve a intentar pasado un rato (no en bucle cada 2 s desde el mismo sitio).</summary>
    public static readonly Dictionary<Anima, float> RetryAt = new Dictionary<Anima, float>();
    public static bool CanTry(Anima a) => a == null || !RetryAt.TryGetValue(a, out float t) || Time.time >= t;
    public float minutes = 5f;
    int _phase;
    Transform _toilet;
    CarpetWalkSpell _walk;
    float _until;
    Vector3 _outside;

    public override string Label => _phase == 0 ? "ir al baño" : "en el baño";

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        if (_phase == 0)
        {
            if (_toilet == null)
            {
                _toilet = Nearest(self.transform.position);
                if (_toilet == null) { Debug.Log($"[Cuerpo] «{self.name}» necesita ir al baño y no encuentra aseos."); return true; }
                _walk = new CarpetWalkSpell { target = _toilet, targetName = "los aseos", stopDistance = 1.4f, giveUpAfter = 120f };   // los aseos pueden estar en otro piso o torre: en alfombra
                if (self.GetComponent<PlayerController>() != null) Panoramic(_toilet);
            }
            if (!_walk.Tick(self)) return false;
            Vector3 gap = _toilet.position - self.transform.position;
            if (new Vector2(gap.x, gap.z).magnitude > 2.5f || Mathf.Abs(gap.y) > 2.5f)   // no llegó: antes se le metía igual en la cabina (saltos de 80 m)
            { Debug.Log($"[Cuerpo] «{self.name}» no llega a los aseos (a {gap.magnitude:0} m): lo deja para luego."); RetryAt[self] = Time.time + 45f; return true; }
            _outside = self.transform.position;
            Move(self.transform, _toilet.position + Vector3.up * 0.05f);   // dentro de la cabina
            Debug.Log($"[Cuerpo] «{self.name}» entra en los aseos (inodoro seco: tapa con serrín).");
            _until = Time.time + minutes * (TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 1f);
            _phase = 1;
            return false;
        }
        // Si otro conductor se lo llevó de la cabina (alfombra, guardia…), NO se le devuelve de golpe a la puerta (salto de 60 m:
        // JornadaTest 2026-10-29, Gohageneis): solo sale de la cabina a la puerta (≤ 2,5 m) si sigue dentro.
        Vector3 inCabin = _toilet != null ? _toilet.position - self.transform.position : Vector3.one * 99f;
        if (new Vector2(inCabin.x, inCabin.z).magnitude > 1.5f || Mathf.Abs(inCabin.y) > 1.5f)
        { BodyMetabolism b0 = self.GetComponent<BodyMetabolism>(); if (b0 != null) b0.toilet = 0f; return true; }
        if (Time.time < _until) return false;
        Move(self.transform, _outside);
        BodyMetabolism body = self.GetComponent<BodyMetabolism>();
        if (body != null) body.toilet = 0f;
        Debug.Log($"[Cuerpo] «{self.name}» sale de los aseos tras lavarse las manos.");
        if (self.GetComponent<PlayerController>() != null) Panoramic(self.transform);
        return true;
    }

    static void Panoramic(Transform target)
    {
        if (CameraManager.Instance == null) return;
        CameraManager.Instance.RequestRobbery(new CameraRobbery { type = RobberyType.OrbitTarget, orbitTarget = target, holdDuration = 3f, returnOnFinish = true });
    }

    static void Move(Transform t, Vector3 p)
    {
        CharacterController cc = t.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        t.position = p;
        if (cc != null) cc.enabled = true;
    }

    static Transform Nearest(Vector3 from)
    {
        Transform best = null; float bestSq = float.MaxValue;
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (t.name != "Inodoro_seco") continue;
            float sq = (t.position - from).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = t; }
        }
        return best;
    }
}
