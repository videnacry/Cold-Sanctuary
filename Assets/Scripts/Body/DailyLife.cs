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

    static AsanaPoseSpell Make(string name, float root, params (BodyPart, Vector3)[] set)
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
    /// <summary>Paciente sentado con el brazo derecho extendido y apoyado (tensión, extracción de sangre).</summary>
    public static AsanaPoseSpell PatientArmOut() => Make("brazo extendido", 0.5f, (BodyPart.ThighLeft, new Vector3(-90, 0, 0)), (BodyPart.ThighRight, new Vector3(-90, 0, 0)),
        (BodyPart.ShinLeft, new Vector3(90, 0, 0)), (BodyPart.ShinRight, new Vector3(90, 0, 0)),
        (BodyPart.ShoulderRight, new Vector3(-80, 0, -15)), (BodyPart.ShoulderLeft, new Vector3(-30, 0, 0)), (BodyPart.ForearmLeft, new Vector3(-60, 0, 0)));
    /// <summary>Paciente sentado, la cabeza un poco alzada y quieta (le miran las pupilas).</summary>
    public static AsanaPoseSpell PatientHeadUp() => Make("cabeza alzada", 0.5f, (BodyPart.ThighLeft, new Vector3(-90, 0, 0)), (BodyPart.ThighRight, new Vector3(-90, 0, 0)),
        (BodyPart.ShinLeft, new Vector3(90, 0, 0)), (BodyPart.ShinRight, new Vector3(90, 0, 0)), (BodyPart.Neck, new Vector3(-12, 0, 0)),
        (BodyPart.ShoulderLeft, new Vector3(-35, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-35, 0, 0)), (BodyPart.ForearmLeft, new Vector3(-55, 0, 0)), (BodyPart.ForearmRight, new Vector3(-55, 0, 0)));
    /// <summary>Paciente sentado con el hombro izquierdo descubierto hacia fuera (vacuna en el deltoides).</summary>
    public static AsanaPoseSpell PatientShoulder() => Make("hombro para la vacuna", 0.5f, (BodyPart.ThighLeft, new Vector3(-90, 0, 0)), (BodyPart.ThighRight, new Vector3(-90, 0, 0)),
        (BodyPart.ShinLeft, new Vector3(90, 0, 0)), (BodyPart.ShinRight, new Vector3(90, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(0, 0, 25)), (BodyPart.Neck, new Vector3(0, 25, 0)),
        (BodyPart.ShoulderRight, new Vector3(-35, 0, 0)), (BodyPart.ForearmRight, new Vector3(-55, 0, 0)));
    /// <summary>El que examina, inclinado, con las dos manos delante (tensión, extracción: sujeta el brazo y la aguja).</summary>
    public static AsanaPoseSpell ExaminerHands() => Make("examinar con las manos", -1f, (BodyPart.Back, new Vector3(18, 0, 0)),
        (BodyPart.ShoulderLeft, new Vector3(-65, 0, 10)), (BodyPart.ShoulderRight, new Vector3(-65, 0, -10)), (BodyPart.ForearmLeft, new Vector3(-35, 0, 0)), (BodyPart.ForearmRight, new Vector3(-35, 0, 0)));
    /// <summary>El que examina las pupilas: la linterna (mano derecha) a la altura de los ojos del paciente.</summary>
    public static AsanaPoseSpell ExaminerPenlight() => Make("linterna en las pupilas", -1f, (BodyPart.Back, new Vector3(12, 0, 0)), (BodyPart.Neck, new Vector3(10, 0, 0)),
        (BodyPart.ShoulderRight, new Vector3(-95, 0, -10)), (BodyPart.ForearmRight, new Vector3(-30, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-50, 0, 15)), (BodyPart.ForearmLeft, new Vector3(-60, 0, 0)));
    /// <summary>El que vacuna: brazo derecho a la altura del hombro del paciente, con la jeringa.</summary>
    public static AsanaPoseSpell ExaminerSyringe() => Make("vacunar", -1f, (BodyPart.Back, new Vector3(10, 0, 0)),
        (BodyPart.ShoulderRight, new Vector3(-80, 0, 0)), (BodyPart.ForearmRight, new Vector3(-45, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-60, 0, 15)), (BodyPart.ForearmLeft, new Vector3(-30, 0, 0)));
}

/// <summary>COMER EN EL RESTAURANTE: va a la barra de servicio, pide (toma una ración del pase), busca una silla libre,
/// se sienta, come con calma (~20 min de juego) y se levanta. Si no hay restaurante, come donde haya comida.</summary>
public class DineSpell : QueuedSpell
{
    public float meal = 0.75f;
    public float eatMinutes = 20f;
    static readonly HashSet<Transform> _taken = new HashSet<Transform>();

    int _phase;              // 0 ir a la barra · 1 ir a la mesa · 2 comer · 3 levantarse
    FoodContainer _food;
    Transform _seat;
    WalkToSpell _walk;
    float _until;
    string _dish;

    public override string Label => _phase switch { 0 => "pedir en la barra del restaurante", 1 => "buscar mesa", 2 => $"comer {_dish}", _ => "levantarse" };

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        switch (_phase)
        {
            case 0:
                if (_food == null || !_food.HasFood)
                {
                    _food = Nearest(self.transform.position);
                    if (_food == null) { Debug.Log($"[Cuerpo] «{self.name}» tiene hambre pero no hay comida en ningún sitio."); return true; }
                    _walk = new WalkToSpell { target = _food.transform, targetName = $"la barra ({_food.name})", stopDistance = 1.3f, giveUpAfter = 10f };
                }
                if (!_walk.Tick(self)) return false;
                if (!_food.Take()) { _food = null; return false; }
                _dish = _food.dishName;
                Debug.Log($"[Restaurante] «{self.name}» pide en la barra y le sirven {_dish}.");
                _seat = FreeSeat(self.transform.position);
                if (_seat != null) { _taken.Add(_seat); _walk = new WalkToSpell { target = _seat, targetName = $"una mesa ({_seat.parent?.name})", stopDistance = 0.5f, giveUpAfter = 12f }; }
                _phase = _seat != null ? 1 : 2;
                if (_phase == 2) _until = Time.time + EatSeconds();
                return false;
            case 1:
                if (!_walk.Tick(self)) return false;
                Vector3 table = _seat.parent != null ? _seat.parent.position : _seat.position;
                Vector3 look = table - _seat.position; look.y = 0f;
                self.transform.position = new Vector3(_seat.position.x, self.transform.position.y, _seat.position.z);
                if (look.sqrMagnitude > 1e-4f) self.transform.rotation = Quaternion.LookRotation(look);
                DailyPoses.Sit().ApplyFinal(self);
                Debug.Log($"[Restaurante] «{self.name}» se sienta a comer en {_seat.parent?.name}.");
                _until = Time.time + EatSeconds();
                _phase = 2;
                return false;
            case 2:
                if (_seat != null) DailyPoses.Sit().ApplyFinal(self);   // sigue sentado (las asanas bloquean los huesos un instante)
                if (Time.time < _until) return false;
                BodyMetabolism body = self.GetComponent<BodyMetabolism>();
                if (body != null) { body.Feed(meal); body.toilet = Mathf.Clamp01(body.toilet + 0.25f); }
                Debug.Log($"[Cuerpo] «{self.name}» termina de comer {_dish}.");
                _phase = 3;
                return false;
            default:
                DailyPoses.Stand().ApplyFinal(self);
                if (_seat != null) _taken.Remove(_seat);
                return true;
        }
    }

    float EatSeconds() => eatMinutes * (TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 1f);

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

    // Las sillas de las mesas del comedor («Asiento»): la libre más cercana.
    static Transform FreeSeat(Vector3 from)
    {
        GameObject dining = GameObject.Find("Comedor_Gourmet");
        if (dining == null) return null;
        Transform best = null; float bestSq = float.MaxValue;
        foreach (Transform t in dining.GetComponentsInChildren<Transform>())
        {
            if (t.name != "Asiento" || _taken.Contains(t)) continue;
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
    public float minutes = 5f;
    int _phase;
    Transform _toilet;
    WalkToSpell _walk;
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
                _walk = new WalkToSpell { target = _toilet, targetName = "los aseos", stopDistance = 1.4f, giveUpAfter = 12f };
                if (self.GetComponent<PlayerController>() != null) Panoramic(_toilet);
            }
            if (!_walk.Tick(self)) return false;
            _outside = self.transform.position;
            Move(self.transform, _toilet.position + Vector3.up * 0.05f);   // dentro de la cabina
            Debug.Log($"[Cuerpo] «{self.name}» entra en los aseos (inodoro seco: tapa con serrín).");
            _until = Time.time + minutes * (TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 1f);
            _phase = 1;
            return false;
        }
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
