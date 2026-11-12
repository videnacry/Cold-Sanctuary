using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ALFOMBRA VOLADORA (una esterilla grande de la pista de aterrizaje). Automática: al TOCARLA el jugador se le muestran
/// los destinos (las otras pistas) en un diálogo (↑/↓ + Intro o clic); elige y la cámara pasa a 3ª persona, el pasajero
/// se SIENTA y la alfombra lo lleva (sube, cruza y baja). Al llegar se levanta y vuelve a ver las opciones (seguir o
/// bajarse). Los trabajadores la usan solos para cambiar de grupo de áreas (CarpetPad.Route).
/// </summary>
[RequireComponent(typeof(Collider))]
public class FlyingCarpet : MonoBehaviour
{
    public static readonly List<FlyingCarpet> All = new List<FlyingCarpet>();
    static readonly HashSet<GameObject> _riders = new HashSet<GameObject>();
    public static bool Riding(GameObject g) => g != null && _riders.Contains(g);
    /// <summary>Otro vehículo que lleva a alguien (la línea de las cavernas, <see cref="SiteLine"/>): cuenta como ir en alfombra, y así nada más lo mueve.</summary>
    public static void Carry(GameObject g, bool on) { if (g == null) return; if (on) _riders.Add(g); else _riders.Remove(g); }
    public static int Trips { get; private set; }

    public CarpetPad pad;
    public float speed = 18f, cruise = 26f;
    /// <summary>(2026-11-04) El GROSOR de la alfombra (su canto con el núcleo de circuitos y la costura de LED, CarpetPad.BuildCarpet):
    /// quien va sentado o se baja, encima de él.</summary>
    public const float Thickness = 0.1f;   // (2026-11-11, petición #58: «más gruesas, con circuitos dentro»; la sonda de la ronda 65 midió 1 cm en la escena guardada: hay que reconstruirla)
    public bool Flying { get; private set; }
    float _ignoreUntil;

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    // Se ofrece UNA vez por cada vez que el jugador la pisa. Antes solo al entrar: si venía de otra alfombra con su diálogo
    // aún abierto (se cierra al alejarse 3,5 m), al pisar esta no pasaba nada y había que salir y volver (Berón 2026-10-27).
    bool _wantOffer;
    void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) { _wantOffer = true; TryOffer(other.gameObject); } }
    void OnTriggerStay(Collider other) { if (_wantOffer && other.CompareTag("Player")) TryOffer(other.gameObject); }
    void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) _wantOffer = false; }

    void TryOffer(GameObject who)
    {
        if (Flying || Time.time < _ignoreUntil || Riding(who)) return;
        Transform open = ChoiceDialog.Anchor;
        if (ChoiceDialog.IsOpen && open == transform) { _wantOffer = false; return; }   // ya es el suyo
        if (ChoiceDialog.IsOpen && (open == null || open.GetComponent<FlyingCarpet>() == null)) return;   // otro diálogo (no de alfombra): espera
        _wantOffer = false;
        Offer(who, false);   // el de la alfombra de al lado, si seguía abierto, se sustituye
    }

    /// <summary>Diálogo de la alfombra para el jugador, en dos niveles (Berón 2026-10-27): «Hacer yoga» → las prácticas
    /// que sabe; «Volar» → los destinos de su red. Si no sabe ningún yoga, va directo a los destinos.</summary>
    public void Offer(GameObject who, bool arrived)
    {
        var yogas = arrived ? new List<YogaSession>() : KnownYoga(who);
        if (yogas.Count == 0) { OfferFlights(who, arrived, false); return; }
        var opts = new List<string> { "Hacer yoga", "Volar" };
        ChoiceDialog.Show(Title(), opts.ToArray(),
            i => { if (i == 0) OfferYoga(who, yogas); else OfferFlights(who, false, true); },
            () => _ignoreUntil = Time.time + 2f, transform, 3.5f);   // alejarse lo cierra
    }

    string Title() => pad == null ? "Alfombra voladora" : pad.kind == CarpetPad.Kind.Edificio ? $"Alfombra de {pad.building}"
        : pad.kind == CarpetPad.Kind.Santuario ? "Alfombra del santuario" : pad.kind == CarpetPad.Kind.Residencia ? "Alfombra de la residencia" : "Tu alfombra";

    // Cada alfombra es también una ESTERILLA: las prácticas que sabe quien la pisa (su grimorio).
    void OfferYoga(GameObject who, List<YogaSession> yogas)
    {
        var opts = new List<string>();
        foreach (YogaSession y in yogas) opts.Add(y.name);
        opts.Add("← Volver");
        ChoiceDialog.Show($"{Title()} · ¿qué yoga?", opts.ToArray(),
            i => { if (i < yogas.Count) { Yoga(who, yogas[i]); _ignoreUntil = Time.time + 60f; } else Offer(who, false); },
            () => _ignoreUntil = Time.time + 2f, transform, 3.5f);
    }

    // Por ÁREAS (CarpetPad.PlayerMenu, Berón 2026-10-30): en una torre, sus áreas (una opción cada una; las plantas de la
    // suya, una a una); en el santuario, las torres (las de varias áreas abren un segundo menú con ellas). Sin «Volar a:».
    void OfferFlights(GameObject who, bool arrived, bool back)
    {
        var menu = pad != null ? pad.PlayerMenu() : new List<CarpetPad.MenuEntry>();
        var opts = new List<string>();
        if (arrived) opts.Add("Bajarse aquí");
        foreach (CarpetPad.MenuEntry m in menu) opts.Add(m.label);
        if (back) opts.Add("← Volver");
        int off = arrived ? 1 : 0;
        string title = arrived ? $"Has llegado a {pad?.ShortName}" : pad == null ? Title() : pad.kind == CarpetPad.Kind.Edificio ? $"{Title()} · ¿a qué área?"
            : pad.kind == CarpetPad.Kind.Santuario ? $"{Title()} · ¿a qué torre?" : pad.kind == CarpetPad.Kind.Residencia ? $"{Title()} · ¿a qué habitación?" : Title();
        ChoiceDialog.Show(title, opts.ToArray(),
            i =>
            {
                int d = i - off;
                if (d >= 0 && d < menu.Count) { if (menu[d].tower != null) OfferTower(who, menu[d].tower, arrived, back); else Fly(who, menu[d].target); }
                else if (back && d == menu.Count) Offer(who, false);
                else _ignoreUntil = Time.time + 2f;
            },
            () => _ignoreUntil = Time.time + 2f, transform, 3.5f);   // alejarse lo cierra
    }

    // Las áreas de una torre, desde el santuario: la alfombra va directa a la pista principal (la del piso más bajo) del área.
    void OfferTower(GameObject who, string tower, bool arrived, bool back)
    {
        var areas = CarpetPad.AreasOfTower(tower);
        var opts = new List<string>();
        foreach (CarpetPad.MenuEntry m in areas) opts.Add(m.label);
        opts.Add("← Volver");
        ChoiceDialog.Show($"{tower} · ¿a qué área?", opts.ToArray(),
            i => { if (i < areas.Count) Fly(who, areas[i].target); else OfferFlights(who, arrived, back); },
            () => _ignoreUntil = Time.time + 2f, transform, 3.5f);
    }

    // Hechizo de cada práctica (SpellCatalog): saberlo = poder hacerla en la alfombra.
    static readonly (string session, string spell)[] YogaSpells =
    {
        ("Upa-yoga de cuello", "upa-yoga"), ("Yoga Namaskar", "yoga-namaskar"), ("Saludo al Sol", "yoga"), ("Ashtanga Primaria", "ashtanga-primaria"),
    };

    /// <summary>Las prácticas de yoga que sabe <paramref name="who"/> (según su grimorio), en orden de aprendizaje.</summary>
    public static List<YogaSession> KnownYoga(GameObject who)
    {
        var l = new List<YogaSession>();
        Grimoire g = who != null ? who.GetComponent<Grimoire>() : null;
        if (g == null) return l;
        foreach (var (session, spell) in YogaSpells)
            if (g.Knows(spell)) { YogaSession s = YogaLibrary.Find(session); if (s != null) l.Add(s); }
        return l;
    }

    // Yoga sobre la alfombra-esterilla: la práctica elegida, paso a paso (como el hechizo `yoga`).
    void Yoga(GameObject who, YogaSession session)
    {
        Anima a = who.GetComponent<Anima>(); if (a == null || session == null) return;
        if (!who.TryGetComponent(out ActionQueue q)) q = who.AddComponent<ActionQueue>();
        WorkdayDirector.Teleport(who.transform, transform.position + Vector3.up * (Thickness + 0.02f));
        who.transform.rotation = transform.rotation;
        foreach (QueuedSpell s in session.Build()) q.EnqueuePlayer(s);
        Debug.Log($"[Alfombra] «{who.name}» hace «{session.name}» en la alfombra-esterilla.");
    }

    /// <summary>Lleva a <paramref name="who"/> sentado hasta la pista <paramref name="to"/>.</summary>
    public void Fly(GameObject who, CarpetPad to)
    {
        if (Flying || who == null || to == null) return;
        StartCoroutine(Trip(who, to));
    }

    IEnumerator Trip(GameObject who, CarpetPad to)
    {
        Flying = true; _riders.Add(who); Trips++;
        // A qué altura va su origen sobre sus pies (el jugador, por los pies; muchos personajes, por la pelvis, ~1 m): para
        // sentarlo encima de la alfombra y dejarlo de pie encima, no medio hundido.
        float rootAbove = float.MaxValue;
        foreach (RaycastHit h in Physics.RaycastAll(who.transform.position + Vector3.up * 0.3f, Vector3.down, 2.5f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(who.transform) && h.point.y <= who.transform.position.y + 0.05f) rootAbove = Mathf.Min(rootAbove, who.transform.position.y - h.point.y);   // el suelo más cercano bajo él
        rootAbove = rootAbove == float.MaxValue ? 0f : Mathf.Clamp(rootAbove, 0f, 1.6f);
        bool player = who.CompareTag("Player");
        Anima a = who.GetComponent<Anima>();
        var agent = who.GetComponent<UnityEngine.AI.NavMeshAgent>();
        var cc = who.GetComponent<CharacterController>();
        var pc = who.GetComponent<PlayerController>();
        bool agentWas = agent != null && agent.enabled, ccWas = cc != null && cc.enabled;
        if (agent != null) agent.enabled = false;
        if (cc != null) cc.enabled = false;
        if (pc != null) pc.HoldBody(true);   // sentado en la alfombra: no anda, pero puede mirar el paisaje
        if (player && CameraManager.Instance != null) CameraManager.Instance.SwitchTo(CameraMode.ThirdPerson, forced: true);
        Debug.Log($"[Alfombra] «{who.name}» se sienta en la alfombra y vuela de {pad?.displayName} a {to.displayName}.");

        Vector3 from = transform.position, dest = to.FreeSlot(this);
        // (2026-11-04) Los TRAMOS del vuelo: el arco de siempre o, si cruzaría un volumen prohibido (el ring de almohadas), sale por
        // debajo hasta fuera de él, sube por fuera, cruza por encima y baja (ver Legs). Cada tramo, con su tiempo.
        foreach (var (l0, l1, arc) in Legs(from, dest, cruise))
        {
            float top = Mathf.Max(l0.y, l1.y) + (arc ? cruise : 0f);
            float dist = Vector3.Distance(new Vector3(l0.x, 0f, l0.z), new Vector3(l1.x, 0f, l1.z));
            float T = arc ? Mathf.Max(3f, (dist + 2f * cruise) / speed) : Mathf.Max(0.6f, Vector3.Distance(l0, l1) / (speed * 0.6f)), t = 0f;
            Vector3 flat = Flat(dest - from);
            Quaternion face = flat.sqrMagnitude > 1e-4f ? Quaternion.LookRotation(flat) : transform.rotation;
            while (t < T)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / T);
                float s = Mathf.SmoothStep(0f, 1f, u);
                Vector3 p = Vector3.Lerp(l0, l1, s);
                if (arc) p.y = Mathf.Lerp(l0.y, l1.y, s) + Mathf.Sin(u * Mathf.PI) * (top - Mathf.Max(l0.y, l1.y)) + Mathf.Sin(t * 3f) * 0.05f;   // sube, cruza y baja (con un leve vaivén)
                transform.SetPositionAndRotation(p, Quaternion.Slerp(transform.rotation, face, Time.deltaTime * 2f));
                if (who == null) { yield return null; continue; }   // (quien iba sentado ya no existe —un cuerpo de prueba destruido en pleno vuelo—: la alfombra termina sola)
                who.transform.SetPositionAndRotation(p + Vector3.up * (Thickness + 0.05f + rootAbove), transform.rotation);
                if (a != null) DailyPoses.Sit().ApplyFinal(a);   // sentado, tranquilo, esperando desembarcar
                yield return null;
            }
        }
        transform.position = dest;
        pad = to;
        if (who == null)
        {
            // SIN PASAJERO: antes la corrutina moría aquí (MissingReference) y la alfombra se quedaba «volando» para siempre; en
            // la batería completa las pruebas destruyen sus cuerpos en pleno vuelo y una torre se iba quedando sin alfombras.
            _riders.Remove(who);
            Flying = false; _ignoreUntil = Time.time + 1f;
            Debug.Log($"[Alfombra] Una alfombra llega sin pasajero a {to.displayName}.");
            yield break;
        }
        // DESEMBARCAR: se levanta al lado de la alfombra.
        if (a != null) DailyPoses.Stand().ApplyFinal(a);
        who.transform.position = dest + Vector3.up * (Thickness + 0.02f + rootAbove);   // de pie ENCIMA de la alfombra (antes se bajaba 1,6 m a un lado y en una casa del árbol caía al vacío)
        if (cc != null) cc.enabled = ccWas;
        if (agent != null) { agent.enabled = agentWas; if (agent.enabled && agent.isOnNavMesh) agent.Warp(who.transform.position); }
        if (pc != null) pc.HoldBody(false);
        if (player && CameraManager.Instance != null) CameraManager.Instance.SwitchTo(CameraManager.Instance.preferredMode, forced: true);
        _riders.Remove(who);
        Flying = false;
        _ignoreUntil = Time.time + 1f;
        Debug.Log($"[Alfombra] «{who.name}» llega a {to.displayName} y se baja.");
        if (player) Offer(who, true);   // por contacto: vuelve a ver las opciones (seguir o bajarse)
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v : Vector3.forward; }

    // ── VOLÚMENES PROHIBIDOS (2026-11-04) ──────────────────────────────────────────────────────────────────────────────
    /// <summary>Quien registra volúmenes que las alfombras no cruzan (el ring de almohadas: estaba encima de la plataforma central y las
    /// alfombras que salían de ella lo atravesaban). Cada proveedor devuelve sus cajas en el mundo.</summary>
    public static readonly List<System.Func<IEnumerable<Bounds>>> NoFlyProviders = new List<System.Func<IEnumerable<Bounds>>>();
    const float NoFlyMargin = 2f;

    static List<Bounds> NoFly()
    {
        var l = new List<Bounds>();
        foreach (var p in NoFlyProviders) { try { foreach (Bounds b in p()) { Bounds e = b; e.Expand(new Vector3(NoFlyMargin * 2f, NoFlyMargin * 2f, NoFlyMargin * 2f)); l.Add(e); } } catch (System.Exception) { } }
        return l;
    }

    static bool InFootprint(Bounds b, Vector3 p) => p.x > b.min.x && p.x < b.max.x && p.z > b.min.z && p.z < b.max.z;

    /// <summary>¿El arco de siempre entre <paramref name="a"/> y <paramref name="b"/> pasa por alguna caja?</summary>
    static bool ArcHits(Vector3 a, Vector3 b, float cruise, List<Bounds> boxes)
    {
        float top = Mathf.Max(a.y, b.y) + cruise;
        for (int i = 1; i < 40; i++)
        {
            float u = i / 40f, s = Mathf.SmoothStep(0f, 1f, u);
            Vector3 p = Vector3.Lerp(a, b, s);
            p.y = Mathf.Lerp(a.y, b.y, s) + Mathf.Sin(u * Mathf.PI) * (top - Mathf.Max(a.y, b.y));
            foreach (Bounds x in boxes) if (x.Contains(p)) return true;
        }
        return false;
    }

    /// <summary>
    /// LOS TRAMOS DEL VUELO: (desde, hasta, ¿en arco?). Sin volúmenes prohibidos en medio, el arco de siempre. Si no:
    ///  · un extremo DEBAJO de una caja (la plataforma central, bajo el ring) sale en recto, por debajo, hasta fuera de su huella;
    ///  · un extremo ENCIMA o DENTRO de ella (la pista del propio ring) sube en vertical hasta por encima;
    ///  · entre esos dos puntos, el arco; si aún cruzara, sube en vertical por fuera, cruza en recto por encima y baja.
    /// </summary>
    public static List<(Vector3 a, Vector3 b, bool arc)> Legs(Vector3 from, Vector3 dest, float cruise)
    {
        var legs = new List<(Vector3, Vector3, bool)>();
        List<Bounds> boxes = NoFly();
        if (boxes.Count == 0 || !ArcHits(from, dest, cruise, boxes)) { legs.Add((from, dest, true)); return legs; }
        Vector3 Out(Vector3 p, Vector3 toward, List<(Vector3, Vector3, bool)> pre, bool atEnd)
        {
            foreach (Bounds b in boxes)
            {
                if (!InFootprint(b, p)) continue;
                Vector3 q;
                if (p.y < b.min.y)
                {
                    // Debajo: en recto hasta el borde de la huella más cercano (hacia el otro extremo si empata), a su misma altura.
                    float y = Mathf.Min(p.y + 1.5f, b.min.y - 0.5f);
                    float dxMin = p.x - b.min.x, dxMax = b.max.x - p.x, dzMin = p.z - b.min.z, dzMax = b.max.z - p.z;
                    float m = Mathf.Min(Mathf.Min(dxMin, dxMax), Mathf.Min(dzMin, dzMax));
                    q = m == dxMin ? new Vector3(b.min.x - 0.5f, y, p.z) : m == dxMax ? new Vector3(b.max.x + 0.5f, y, p.z)
                      : m == dzMin ? new Vector3(p.x, y, b.min.z - 0.5f) : new Vector3(p.x, y, b.max.z + 0.5f);
                }
                else q = new Vector3(p.x, Mathf.Max(p.y, b.max.y + 0.5f), p.z);   // encima o dentro: en vertical hasta por encima
                if (atEnd) pre.Add((q, p, false)); else pre.Add((p, q, false));
                return q;
            }
            return p;
        }
        var head = new List<(Vector3, Vector3, bool)>(); var tail = new List<(Vector3, Vector3, bool)>();
        Vector3 a = Out(from, dest, head, false), z = Out(dest, from, tail, true);
        legs.AddRange(head);
        if (!ArcHits(a, z, cruise, boxes)) legs.Add((a, z, true));
        else
        {
            float high = Mathf.Max(a.y, z.y);
            foreach (Bounds b in boxes) high = Mathf.Max(high, b.max.y + 1f);
            Vector3 a2 = new Vector3(a.x, high, a.z), z2 = new Vector3(z.x, high, z.z);
            legs.Add((a, a2, false)); legs.Add((a2, z2, false)); legs.Add((z2, z, false));
        }
        legs.AddRange(tail);
        return legs;
    }
}
