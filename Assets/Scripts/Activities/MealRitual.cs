using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA ORQUESTA DE LA COMIDA (docs/actividades-y-uniformes.md §2; Berón 2026-11-04: «en la hora de la comida me ha parecido que los
/// humanos solo se sientan en la mesa? comer debe tener su propia orquesta/animaciones/objetos»). Los pasos, en orden:
///  1. BANDEJA: recoge en la barra (o en la despensa de casa, o en el mostrador del comedor de la era) la bandeja con su plato —la
///     ración de verdad de un <see cref="FoodContainer"/>, con su comida y su guarnición— y un vaso; la lleva con las dos manos.
///  2. SENTARSE: deja el plato y el vaso en la mesa, delante de él.
///  3. CUBIERTOS: tenedor en la derecha y cuchillo en la izquierda (el insecto-humano, cuchara).
///  4. BOCADOS: el brazo baja al plato y sube a la boca; en cada bocado la comida del plato MENGUA (se ve) y el cuerpo se sacia un
///     poco (<see cref="BodyMetabolism.Feed"/>; y, si tiene <see cref="Metabolism"/>, los nutrientes del plato).
///  5. BEBER: cada tres bocados deja el cuchillo, coge el vaso, bebe y lo vuelve a dejar.
///  6. RECOGER: plato vacío y vaso a la bandeja, de pie.
///  7. DEVOLVER: lleva la bandeja al punto de devolución (el extremo de la barra, el fregadero de casa, el mostrador) y la deja.
/// Para un cuerpo de cubos humano y para un insecto-humano (con <see cref="CubeInsect.Martial"/>: el brazo a la boca). Lo usan
/// <see cref="DineSpell"/> (el restaurante), <see cref="HomeNeedSpell"/> (comer en casa: residencias del santuario y del microcosmos) y
/// <see cref="MicroJornada"/> (la comida del mediodía en el comedor de la era). Todo va por tiempo: ninguna espera sin tope; si quien
/// lo conduce deja de llamarlo, <see cref="Sweep"/> recoge la vajilla.
/// </summary>
public class MealRitual
{
    public const int Bites = 9;
    public readonly string dish;
    public int bitesTaken { get; private set; }
    public int sips { get; private set; }
    /// <summary>Lo que queda en el plato (1 = lleno, 0 = vacío).</summary>
    public float FoodLeft => 1f - bitesTaken / (float)Bites;
    /// <summary>Los pasos que ya se hicieron, en orden («bandeja», «sentarse», «cubiertos», «bocado», «beber», «recoger», «devolver»).</summary>
    public readonly List<string> steps = new List<string>();
    public bool Returned { get; private set; }
    /// <summary>El tamaño de la comida del plato ahora (1 al servir): lo que mira la prueba.</summary>
    public float FoodScale => _food != null && _food.gameObject.activeSelf ? _food.localScale.x / Mathf.Max(1e-5f, _food0.x) : 0f;

    /// <summary>Comidas en marcha (para recoger la vajilla de las que se quedaron a medias).</summary>
    static readonly List<MealRitual> Live = new List<MealRitual>();
    /// <summary>Comidas completas (con todos sus pasos) desde el arranque; bocados dados.</summary>
    public static int Completed { get; private set; }
    public static int TotalBites { get; private set; }

    GameObject _who;
    Transform _tray, _plate, _cup, _food, _side;
    Vector3 _food0, _side0;
    float _eatStart = -1f, _eatEnd, _lastTick, _drinkUntil = -1f, _meal = 0.75f;
    bool _sitting, _onTable;
    int _nextBite;
    AsanaPoseSpell _sit, _stand;
    BodyMetabolism _body; Metabolism _metab;

    public MealRitual(string dish) { this.dish = string.IsNullOrEmpty(dish) ? "plato del día" : dish; }

    static float S(GameObject g) => ActivityKit.Scale(g);
    void Touch() { _lastTick = Time.time; if (!Live.Contains(this)) Live.Add(this); }
    void Step(string s) { steps.Add(s); }

    // ── 1. La bandeja ───────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Recoge la bandeja (<paramref name="tray"/>) o solo el plato (en casa) con la ración y un vaso.</summary>
    public void TakeTray(GameObject who, bool tray = true, float meal = 0.75f)
    {
        _who = who; _meal = meal; Touch();
        _body = who.GetComponent<BodyMetabolism>(); _metab = who.GetComponent<Metabolism>();
        float s = S(who);
        _tray = new GameObject($"Bandeja_{who.name}").transform;
        if (tray) KitchenProps.C(_tray, new Vector3(0f, -0.008f, 0f), new Vector3(0.44f, 0.016f, 0.32f), new Color(0.55f, 0.36f, 0.22f), 0f, "Bandeja");
        _plate = KitchenProps.Dish(_tray, dish, new Vector3(-0.06f, 0f, 0f));
        _food = _plate.Find("Comida"); _side = _plate.Find("Guarnicion");
        if (_food != null) _food0 = _food.localScale; if (_side != null) _side0 = _side.localScale;
        _cup = KitchenProps.C(_tray, new Vector3(0.15f, 0.045f, 0.06f), new Vector3(0.06f, 0.09f, 0.06f), new Color(0.82f, 0.9f, 0.95f, 0.75f), 0f, "Vaso");
        KitchenProps.C(_cup, new Vector3(0f, 0.28f, 0f), new Vector3(0.85f, 0.3f, 0.85f), new Color(0.55f, 0.75f, 0.95f), 0f, "Agua");
        _tray.SetParent(who.transform, false);
        _tray.localScale = Vector3.one * (s / Mathf.Max(1e-4f, who.transform.lossyScale.y));
        PlaceInHands();
        Step("bandeja");
        Debug.Log($"[Comida] «{who.name}» recoge {(tray ? "la bandeja" : "el plato")} con {dish}.");
    }

    // La bandeja delante del pecho, a la altura de las manos.
    void PlaceInHands()
    {
        if (_who == null || _tray == null) return;
        float s = S(_who), feet = Feet(_who) - _who.transform.position.y;
        float up = ActivityKit.IsInsect(_who) ? 0.5f : 1.0f, ahead = ActivityKit.IsInsect(_who) ? 0.35f : 0.38f;
        _tray.SetParent(_who.transform, true);
        _tray.position = _who.transform.position + Vector3.up * (feet + up * s) + _who.transform.forward * ahead * s;
        _tray.rotation = Quaternion.LookRotation(Flat(_who.transform.forward));
    }

    /// <summary>De camino con la bandeja (cada fotograma): brazos que la sujetan.</summary>
    public void Carry(GameObject who)
    {
        if (_tray == null || who == null) return;
        Touch();
        if (ActivityKit.IsHuman(who)) ActivityKit.Apply(who, Performances.M.Carry, 0f);
        else if (who.TryGetComponent(out CubeInsect ci)) ci.Martial(0.5f, 0.5f, 0.35f, 0f, 0.5f);
    }

    // ── 2–3. Sentarse, el plato en la mesa y los cubiertos ─────────────────────────────────────────────────────

    /// <summary>Ya sentado (o de pie, si no hay silla) a <paramref name="table"/>: el plato y el vaso a la mesa y los cubiertos a la mano.</summary>
    public void SitDown(GameObject who, Transform table, bool sitting)
    {
        _who = who; _sitting = sitting; Touch();
        if (_tray == null) TakeTray(who, false, _meal);
        float s = S(who);
        if (table != null)
        {
            Vector3 fw = Flat(who.transform.forward);
            Vector3 at = who.transform.position + fw * 0.42f * s;
            at.y = TableTop(who, table, at);
            // El plato y el vaso salen de la bandeja a la mesa (la bandeja vacía, al lado).
            _plate.SetParent(null, true); _plate.position = at; _plate.rotation = Quaternion.LookRotation(fw);
            _cup.SetParent(null, true); _cup.position = at + Vector3.Cross(Vector3.up, fw) * 0.2f * s + Vector3.up * 0.045f * s; _cup.rotation = _plate.rotation;
            _tray.SetParent(null, true); _tray.position = at - Vector3.Cross(Vector3.up, fw) * 0.3f * s + fw * 0.05f * s; _tray.rotation = _plate.rotation;
            _onTable = true;
        }
        Step("sentarse");
        if (ActivityKit.IsInsect(who)) ActivityKit.Hold(who, true, ActivityKit.Prop.Spoon);
        else { ActivityKit.Hold(who, true, ActivityKit.Prop.Fork); ActivityKit.Hold(who, false, ActivityKit.Prop.Knife); }
        Step("cubiertos");
        Debug.Log($"[Comida] «{who.name}» {(sitting ? "se sienta" : "se queda de pie")}, deja el plato {(table != null ? "en la mesa" : "en la bandeja")} y coge los cubiertos.");
    }

    // ── 4–5. Comer y beber ──────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Un fotograma de la comida, que acaba a la hora <paramref name="until"/> (Time.time). true = plato vacío (ya comió).
    /// Reparte los <see cref="Bites"/> bocados por el tiempo; cada tres, un trago.</summary>
    /// <summary>Acaba en <paramref name="seconds"/> s lo que queda (los bocados que faltan, seguidos): a las 12 hay misión.</summary>
    public void Hurry(float seconds) { if (_eatStart >= 0f) _eatEnd = Mathf.Min(_eatEnd, Time.time + seconds); }

    public bool Eat(GameObject who, float until)
    {
        if (who == null) return true;
        Touch();
        if (_eatStart < 0f) { _eatStart = Time.time; _eatEnd = Mathf.Max(until, Time.time + 2f); _nextBite = 0; }
        float span = (_eatEnd - _eatStart) / Bites;
        float t = Time.time - _eatStart;
        bool human = ActivityKit.IsHuman(who);
        Anima a = who.GetComponent<Anima>();
        if (human && a != null)
        {
            if (_sitting) (_sit ??= DailyPoses.Sit()).ApplyFinal(a); else (_stand ??= DailyPoses.Stand()).ApplyFinal(a);
        }
        // El trago (tras cada tercer bocado): el vaso a la boca con la izquierda.
        if (Time.time < _drinkUntil)
        {
            float dk = 1f - Mathf.Abs((_drinkUntil - Time.time) / 1.6f * 2f - 1f);   // 0 → 1 → 0
            if (human) ActivityKit.Apply(who, Drink, dk * Drink.period * 0.5f);
            else if (who.TryGetComponent(out CubeInsect dci)) dci.Martial(0.3f + 0.7f * dk, 0.1f, 0.3f + 0.7f * dk, 0f, 0.4f);
            return false;
        }
        if (_cup != null && !_cup.gameObject.activeSelf) { _cup.gameObject.SetActive(true); ActivityKit.Hold(who, false, human ? ActivityKit.Prop.Knife : ActivityKit.Prop.None); }
        // El bocado: el brazo baja al plato y sube a la boca (el bocado se da arriba).
        float frac = span > 0f ? (t % span) / span : 1f;
        float k = 0.5f - 0.5f * Mathf.Cos(frac * Mathf.PI * 2f);
        if (human) ActivityKit.Apply(who, Bite, k * Bite.period * 0.5f);
        else if (who.TryGetComponent(out CubeInsect ci)) ci.Martial(0.1f, k, 0.3f + 0.7f * k, 0f, 0.3f);
        int due = Mathf.Min(Bites, Mathf.FloorToInt(t / Mathf.Max(0.01f, span) + 0.5f));
        while (_nextBite < due) TakeBite(who);
        if (bitesTaken >= Bites && Time.time >= _eatEnd) return true;
        return false;
    }

    void TakeBite(GameObject who)
    {
        _nextBite++; bitesTaken++; TotalBites++;
        if (bitesTaken == 1 || !steps.Contains("bocado")) Step("bocado");
        float left = FoodLeft;
        if (_food != null) { _food.localScale = new Vector3(_food0.x * Mathf.Max(0.15f, left), _food0.y, _food0.z * Mathf.Max(0.15f, left)); if (left <= 0.01f) _food.gameObject.SetActive(false); }
        if (_side != null && left < 0.5f) { _side.localScale = _side0 * Mathf.Max(0f, left * 2f); if (left <= 0.01f) _side.gameObject.SetActive(false); }
        float portion = _meal / Bites;
        if (_body != null) _body.Feed(portion);
        if (_metab != null) foreach (var n in Nutrients(dish)) _metab.Absorb(n.nutrient, portion * n.share);
        ActivityKit.Burst(ActivityKit.Tip(who, true), ActivityKit.Fx.Crumbs, S(who) * 0.6f);
        if (bitesTaken % 3 == 0 && bitesTaken < Bites) Sip(who);
    }

    void Sip(GameObject who)
    {
        sips++;
        if (!steps.Contains("beber")) Step("beber");
        _drinkUntil = Time.time + 1.6f;
        if (_cup != null) _cup.gameObject.SetActive(false);   // (está en la mano)
        ActivityKit.Hold(who, false, ActivityKit.Prop.Cup);
    }

    /// <summary>Los nutrientes de un plato por sus palabras (la proteína del huevo, los hidratos del arroz…): reparto 0–1.</summary>
    public static (string nutrient, float share)[] Nutrients(string dish)
    {
        string d = (dish ?? "").ToLowerInvariant();
        if (d.Contains("carne") || d.Contains("huevo") || d.Contains("pollo") || d.Contains("pescado") || d.Contains("tofu") || d.Contains("lenteja") || d.Contains("legumbre"))
            return new[] { ("protein", 0.55f), ("fat", 0.25f), ("carb", 0.2f) };
        if (d.Contains("yogur") || d.Contains("queso") || d.Contains("leche")) return new[] { ("protein", 0.35f), ("fat", 0.35f), ("minerals", 0.3f) };
        if (d.Contains("ensalada") || d.Contains("verdura") || d.Contains("fruta")) return new[] { ("carb", 0.6f), ("minerals", 0.4f) };
        return new[] { ("carb", 0.65f), ("protein", 0.2f), ("minerals", 0.15f) };   // cereales, pan, arroz, potaje…
    }

    // ── 6–7. Recoger y devolver ─────────────────────────────────────────────────────────────────────────────────

    /// <summary>De pie: plato y vaso a la bandeja, bandeja a las manos. Suelta los cubiertos (van en la bandeja).</summary>
    public void PickUp(GameObject who)
    {
        Touch();
        if (_tray == null || who == null) return;
        ActivityKit.ClearProps(who);
        float s = S(who);
        if (_plate == null) { Finish(); return; }
        _tray.SetParent(who.transform, false);   // (la escala de la bandeja es relativa a quien la lleva)
        _plate.SetParent(_tray, false); _plate.localPosition = new Vector3(-0.06f, 0f, 0f); _plate.localRotation = Quaternion.identity; _plate.localScale = Vector3.one;
        if (_cup != null) { _cup.gameObject.SetActive(true); _cup.SetParent(_tray, false); _cup.localPosition = new Vector3(0.15f, 0.045f, 0.06f); _cup.localRotation = Quaternion.identity; _cup.localScale = new Vector3(0.06f, 0.09f, 0.06f); }
        KitchenProps.C(_tray, new Vector3(-0.06f, 0.03f, 0.08f), new Vector3(0.02f, 0.006f, 0.16f), new Color(0.73f, 0.75f, 0.78f), 20f, "Cubiertos");
        _tray.localScale = Vector3.one * (s / Mathf.Max(1e-4f, who.transform.lossyScale.y));
        _onTable = false;
        PlaceInHands();
        Step("recoger");
        Debug.Log($"[Comida] «{who.name}» recoge el plato ({bitesTaken} bocados, {sips} tragos) y lleva la bandeja a devolver.");
    }

    /// <summary>Deja la bandeja en el punto de devolución (delante de él, a la altura de la encimera). Se recoge sola en unos segundos.</summary>
    public void Return(GameObject who)
    {
        Touch();
        if (_tray == null) { Finish(); return; }
        float s = who != null ? S(who) : 1f;
        _tray.SetParent(null, true);
        if (who != null)
        {
            Vector3 fw = Flat(who.transform.forward);
            _tray.position = new Vector3(who.transform.position.x, Feet(who) + (ActivityKit.IsInsect(who) ? 0.45f : 0.92f) * s, who.transform.position.z) + fw * 0.55f * s;
        }
        Returned = true;
        Step("devolver");
        Object.Destroy(_tray.gameObject, 4f);
        _tray = null; _plate = null; _cup = null;
        if (who != null) ActivityKit.ClearProps(who);
        Completed++;
        Debug.Log($"[Comida] «{(who != null ? who.name : "?")}» devuelve la bandeja: {string.Join(" → ", steps)}.");
        Finish();
    }

    /// <summary>La comida se corta a medias: la vajilla desaparece y suelta los cubiertos.</summary>
    public void Abort()
    {
        if (_who != null) ActivityKit.ClearProps(_who);
        if (_tray != null) Object.Destroy(_tray.gameObject);
        if (_plate != null && _plate.parent == null) Object.Destroy(_plate.gameObject);
        if (_cup != null && _cup.parent == null) Object.Destroy(_cup.gameObject);
        _tray = null; _plate = null; _cup = null;
        Finish();
    }

    void Finish() { Live.Remove(this); }

    /// <summary>Recoge la vajilla de las comidas que nadie conduce desde hace 6 s (cortadas a medias).</summary>
    public static void Sweep()
    {
        for (int i = Live.Count - 1; i >= 0; i--)
        {
            MealRitual m = Live[i];
            if (m == null) { Live.RemoveAt(i); continue; }
            if (Time.time - m._lastTick > 6f || m._who == null) { Debug.Log($"[Comida] Se recoge la vajilla de una comida a medias ({m.dish})."); m.Abort(); }
        }
    }

    // ── Gestos y medidas ────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>El bocado: de la mano en el plato (A) a la mano en la boca (B), con el brazo derecho.</summary>
    public static readonly ActivityKit.Arms Bite = new ActivityKit.Arms(2f,
        (BodyPart.ShoulderRight, new Vector3(-42f, 0f, 0f), new Vector3(-38f, 0f, -16f)),
        (BodyPart.ForearmRight, new Vector3(-48f, 0f, 0f), new Vector3(-128f, 0f, 22f)),
        (BodyPart.ShoulderLeft, new Vector3(-40f, 0f, 0f), new Vector3(-40f, 0f, 0f)),
        (BodyPart.ForearmLeft, new Vector3(-50f, 0f, 0f), new Vector3(-50f, 0f, 0f)),
        (BodyPart.Neck, new Vector3(14f, 0f, 0f), new Vector3(4f, 0f, 0f)));
    /// <summary>El trago: el vaso de la mesa a la boca con el brazo izquierdo, la cabeza un poco atrás.</summary>
    public static readonly ActivityKit.Arms Drink = new ActivityKit.Arms(2f,
        (BodyPart.ShoulderLeft, new Vector3(-40f, 0f, 0f), new Vector3(-40f, 0f, 16f)),
        (BodyPart.ForearmLeft, new Vector3(-50f, 0f, 0f), new Vector3(-130f, 0f, -22f)),
        (BodyPart.Neck, new Vector3(10f, 0f, 0f), new Vector3(-12f, 0f, 0f)));

    // Los pies (la base del cuerpo de cubos del humano; el pivote del insecto).
    static float Feet(GameObject g)
    {
        Transform body = g.transform.Find(CubeHumanoid.BodyName);
        return body != null ? body.position.y : g.transform.position.y;
    }

    // La altura del tablero de la mesa en ese punto: lo que haya debajo (sin contarse a sí mismo); si no hay nada razonable, la de una
    // mesa normal para su estatura (0,75 de 1,75 m).
    static float TableTop(GameObject who, Transform table, Vector3 at)
    {
        float s = S(who), feet = Feet(who);
        float lo = feet + 0.35f * s, hi = feet + 1.3f * s;
        float best = float.NaN;
        foreach (RaycastHit h in Physics.RaycastAll(new Vector3(at.x, feet + 1.6f * s, at.z), Vector3.down, 1.6f * s, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.transform.IsChildOf(who.transform)) continue;
            if (h.point.y < lo || h.point.y > hi) continue;
            if (float.IsNaN(best) || h.point.y > best) best = h.point.y;
        }
        if (!float.IsNaN(best)) return best + 0.005f;
        // Sin colisionador: lo más alto de la mesa por sus renderizadores (su tablero), si cae en lo razonable.
        if (table != null)
        {
            float top = float.NaN;
            foreach (Renderer r in table.GetComponentsInChildren<Renderer>())
            {
                float y = r.bounds.max.y;
                if (y >= lo && y <= hi && (float.IsNaN(top) || y > top) && r.bounds.Contains(new Vector3(at.x, r.bounds.center.y, at.z))) top = y;
            }
            if (!float.IsNaN(top)) return top + 0.005f;
        }
        return feet + 0.75f * s;
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-6f ? v.normalized : Vector3.forward; }
}
