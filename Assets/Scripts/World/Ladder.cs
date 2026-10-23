using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// ESCALERA VERTICAL (la única escalera de las casas del árbol y plataformas elevadas): dos extremos interactuables, el
/// PIE (subir) y LO ALTO (bajar). Se sube con F (interactuar), con ESPACIO (trepar, como un árbol: <see cref="PlayerClimber"/>)
/// o con el hechizo <c>climb</c> de la consola (<see cref="ClimbLadderSpell"/>). Mientras sube, el cuerpo queda pegado a la
/// escalera (sin gravedad ni control) y brazos y piernas alternan peldaño a peldaño; al llegar, lo deja en la plataforma
/// (o en el suelo). La IA la usa sola: un <see cref="NavMeshLink"/> une pie y plataforma, y quien sigue una ruta
/// (<see cref="PathMover"/>, <see cref="WalkToSpell"/>) al llegar a un extremo trepa (<see cref="AutoClimb"/>).
/// </summary>
public class Ladder : MonoBehaviour, IInteractable
{
    public bool top;
    public Transform ladder;     // la escalera (pie a ras de suelo; forward = hacia la plataforma)
    public float deckY = 11f;    // altura de la plataforma sobre el pie
    public float speed = 1.6f;   // m/s a aptitudes 1 (se escala por fuerza·agilidad)

    public const float FootOffset = 1.25f;   // el pie de la escalera queda a esto de ella, por fuera (para colocar cosas al pie)
    public const float Gap = 0.06f;          // del borde de la plataforma al eje de la escalera: pegada, sin hueco por donde caer

    static readonly List<Ladder> _all = new List<Ladder>();
    static readonly HashSet<Transform> _climbing = new HashSet<Transform>();

    /// <summary>¿<paramref name="who"/> está ahora en una escalera? (Nadie lo mueve mientras: ni andar, ni el suelo.)</summary>
    public static bool IsClimbing(Transform who) => who != null && _climbing.Contains(who);

    void OnEnable() => _all.Add(this);
    void OnDisable() => _all.Remove(this);

    public string InteractLabel => top ? "Bajar por la escalera" : "Subir por la escalera";
    public bool CanInteract => !IsClimbing(Player());

    public void Interact() => TryClimb(Player());

    static Transform Player() { GameObject p = GameObject.FindGameObjectWithTag("Player"); return p != null ? p.transform : null; }

    /// <summary>Empieza a subir (desde el pie) o a bajar (desde lo alto) a <paramref name="who"/>.</summary>
    public bool TryClimb(Transform who)
    {
        if (who == null || IsClimbing(who)) return false;
        StartCoroutine(Climb(who, !top));
        return true;
    }

    /// <summary>El extremo de escalera más cercano a <paramref name="p"/> dentro de <paramref name="range"/> m.</summary>
    public static Ladder Nearest(Vector3 p, float range)
    {
        Ladder best = null; float bestD = range;
        foreach (Ladder l in _all)
        {
            float d = Vector3.Distance(l.transform.position, p);
            if (d < bestD) { bestD = d; best = l; }
        }
        return best;
    }

    /// <summary>Para quien sigue una ruta del NavMesh: si la siguiente esquina queda muy arriba (o abajo) y está junto al
    /// pie (o a lo alto) de una escalera, trepa. Devuelve true mientras está en la escalera (no debe andar).</summary>
    public static bool AutoClimb(Transform who, Vector3 nextCorner)
    {
        if (IsClimbing(who)) return true;
        float dy = nextCorner.y - who.position.y;
        if (Mathf.Abs(dy) < 2.5f) return false;
        foreach (Ladder l in _all)
        {
            if (l.top == dy > 0f) continue;   // del pie solo se sube; de lo alto solo se baja
            Vector3 d = l.transform.position - who.position;
            if (Mathf.Abs(d.y) > 2.5f) continue;
            d.y = 0f;
            if (d.magnitude > 2f) continue;
            // Solo si la ruta CRUZA esta escalera: la esquina siguiente es su otro extremo. Antes bastaba con que el destino
            // quedara más arriba (o abajo) estando junto a ella; si el destino caía unos metros por encima o por debajo de
            // donde se le puede llegar (p. ej. el punto del área bajo una entreplanta), subía, el destino seguía «abajo»,
            // bajaba, seguía «arriba»… y se quedaba subiendo y bajando sin parar.
            Vector3 other = l.top ? l.FootLanding : l.TopLanding;
            Vector3 o = nextCorner - other;
            if (Mathf.Abs(o.y) > 2.5f) continue;
            o.y = 0f;
            if (o.magnitude <= 3f) return l.TryClimb(who);
        }
        return false;
    }

    // Donde se pisa al pie y arriba (los dos extremos del enlace del NavMesh).
    Vector3 FootLanding => ladder.TransformPoint(new Vector3(0f, 0f, -FootOffset));
    Vector3 TopLanding => ladder.TransformPoint(new Vector3(0f, deckY, 1.6f));

    /// <summary>Sube (up) o baja al <paramref name="who"/> por la escalera.</summary>
    public IEnumerator Climb(Transform who, bool up)
    {
        _climbing.Add(who);
        CharacterController cc = who.GetComponent<CharacterController>();
        PlayerController pc = who.GetComponent<PlayerController>();
        if (pc != null) pc.HoldBody(true);   // no anda, pero la cámara sigue libre mientras trepa
        if (cc != null) cc.enabled = false;
        CreatureRig rig = who.GetComponent<CreatureRig>();

        // Trepar como un árbol (PlayerClimber): más rápido con fuerza·agilidad, más lento con peso.
        float pace = speed;
        CharacterLevel lvl = who.GetComponent<CharacterLevel>();
        if (lvl != null) pace *= Mathf.Clamp(lvl.aptitudes.strength * lvl.aptitudes.agility / Mathf.Max(0.3f, lvl.aptitudes.bodyMass), 0.5f, 2f);

        Vector3 baseP = ladder.position - ladder.forward * 0.45f;            // pegado a la escalera, por fuera
        float y0 = up ? 0.05f : deckY + 0.1f, y1 = up ? deckY + 0.1f : 0.05f;
        who.rotation = Quaternion.LookRotation(ladder.forward);
        float yy = y0, phase = 0f;
        Debug.Log($"[Escalera] «{who.name}» {(up ? "sube" : "baja")} por la escalera de «{ladder.parent?.name}».");
        while (who != null && (up ? yy < y1 : yy > y1))
        {
            yy += (up ? 1f : -1f) * pace * Time.deltaTime;
            phase += pace * Time.deltaTime / 0.35f * Mathf.PI;
            who.position = new Vector3(baseP.x, ladder.position.y + yy + 1f, baseP.z);   // +1: el pivote va en el centro del cuerpo
            if (rig != null) Limbs(rig, phase);
            yield return null;
        }
        _climbing.Remove(who);
        if (who == null) yield break;
        if (pc != null) pc.HoldBody(false);
        // Al llegar arriba, un paso hacia la plataforma; abajo, un paso atrás.
        who.position = up ? ladder.position + ladder.forward * 1.4f + Vector3.up * (deckY + 1.2f) : ladder.position - ladder.forward * FootOffset + Vector3.up * 1.1f;
        if (cc != null) cc.enabled = true;
        Debug.Log($"[Escalera] «{who.name}» llega {(up ? "arriba" : "al suelo")}.");
    }

    static void Limbs(CreatureRig rig, float phase)
    {
        float s = Mathf.Sin(phase);
        Set(rig, BodyPart.ShoulderLeft, -150f + 25f * s); Set(rig, BodyPart.ShoulderRight, -150f - 25f * s);
        Set(rig, BodyPart.ThighLeft, -40f - 30f * s);     Set(rig, BodyPart.ThighRight, -40f + 30f * s);
        Set(rig, BodyPart.ShinLeft, 60f + 20f * s);       Set(rig, BodyPart.ShinRight, 60f - 20f * s);
    }

    static void Set(CreatureRig rig, BodyPart p, float x)
    {
        Transform t = rig.Get(p);
        if (t == null) return;
        rig.LockPose(p, 0.2f);
        t.localRotation = Quaternion.Euler(x, 0f, 0f);
    }

    /// <summary>Monta una escalera vertical de cubos: <paramref name="localFoot"/> = su pie (a ras de suelo, en
    /// <paramref name="parent"/>), <paramref name="yaw"/> = hacia dónde queda la plataforma, <paramref name="height"/> =
    /// altura del suelo de la plataforma. Dos largueros, peldaños cada 35 cm, los dos extremos interactuables y el enlace
    /// del NavMesh para la IA.</summary>
    public static Transform Build(Transform parent, Vector3 localFoot, float yaw, float height, Color wood, Color woodDark)
    {
        Transform l = new GameObject("Escalera").transform; l.SetParent(parent, false);
        l.localPosition = localFoot; l.localRotation = Quaternion.Euler(0f, yaw, 0f);
        for (int s = -1; s <= 1; s += 2) Block(l, new Vector3(s * 0.32f, (height + 1f) * 0.5f, 0f), new Vector3(0.09f, height + 1f, 0.09f), woodDark, "Larguero");
        for (float h = 0.35f; h < height + 0.9f; h += 0.35f) Block(l, new Vector3(0f, h, 0f), new Vector3(0.64f, 0.05f, 0.06f), wood, "Peldano");
        End(l, new Vector3(0f, 1f, -0.7f), false, height, "Pie de la escalera");
        End(l, new Vector3(0f, height + 1f, 0.9f), true, height, "Lo alto de la escalera");
        NavMeshLink link = l.gameObject.AddComponent<NavMeshLink>();
        link.startPoint = new Vector3(0f, 0f, -FootOffset); link.endPoint = new Vector3(0f, height, 1.6f);
        link.width = 0.8f; link.bidirectional = true; link.costModifier = 2f;   // trepar cuesta más que andar
        WorldLabel wl = l.gameObject.AddComponent<WorldLabel>(); wl.text = "Escalera [F]"; wl.height = 2.4f;
        return l;
    }

    static void End(Transform l, Vector3 local, bool isTop, float height, string name)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(l, false);
        g.transform.localPosition = local; g.transform.localScale = new Vector3(1.4f, 2f, 1.2f);
        g.GetComponent<Collider>().isTrigger = true;
        g.GetComponent<Renderer>().enabled = false;
        Ladder e = g.AddComponent<Ladder>();
        e.top = isTop; e.ladder = l; e.deckY = height;
    }

    static void Block(Transform parent, Vector3 local, Vector3 size, Color c, string name)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        Collider col = g.GetComponent<Collider>(); if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
    }
}

/// <summary>Hechizo TREPAR (<c>climb</c> en la consola): va a la escalera más cercana y la sube (o la baja, si está arriba).</summary>
public class ClimbLadderSpell : QueuedSpell
{
    public float searchRadius = 40f;
    Ladder _end;
    WalkToSpell _walk;
    bool _started;

    public override string Label => _end != null ? (_end.top ? "bajar por la escalera" : "subir por la escalera") : "trepar";

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        if (_started) return !Ladder.IsClimbing(self.transform);
        if (_end == null)
        {
            _end = Ladder.Nearest(self.transform.position, searchRadius);
            if (_end == null) { Debug.Log("[Hechizo] No hay ninguna escalera cerca."); return true; }
        }
        Vector3 d = _end.transform.position - self.transform.position; d.y = 0f;
        if (d.magnitude <= 1.5f && Mathf.Abs(_end.transform.position.y - self.transform.position.y) < 2.5f)
        { _started = _end.TryClimb(self.transform); return !_started; }
        if (_walk == null) _walk = new WalkToSpell { target = _end.transform, targetName = "la escalera", stopDistance = 0.8f, giveUpAfter = 6f };
        if (_walk.Tick(self))
        {
            _walk = null;
            if (d.magnitude > 2.5f) { Debug.Log("[Hechizo] No llegas a la escalera."); return true; }
            _started = _end.TryClimb(self.transform); return !_started;
        }
        return false;
    }
}
