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
    public static int Trips { get; private set; }

    public CarpetPad pad;
    public float speed = 18f, cruise = 26f;
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
        WorkdayDirector.Teleport(who.transform, transform.position + Vector3.up * 0.05f);
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
        float top = Mathf.Max(from.y, dest.y) + cruise;
        float dist = Vector3.Distance(new Vector3(from.x, 0f, from.z), new Vector3(dest.x, 0f, dest.z));
        float T = Mathf.Max(3f, (dist + 2f * cruise) / speed), t = 0f;
        Quaternion face = Quaternion.LookRotation(Flat(dest - from));
        while (t < T)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / T);
            float s = Mathf.SmoothStep(0f, 1f, u);
            Vector3 p = Vector3.Lerp(from, dest, s);
            p.y = Mathf.Lerp(from.y, dest.y, s) + Mathf.Sin(u * Mathf.PI) * (top - Mathf.Max(from.y, dest.y)) + Mathf.Sin(t * 3f) * 0.05f;   // sube, cruza y baja (con un leve vaivén)
            transform.SetPositionAndRotation(p, Quaternion.Slerp(transform.rotation, face, Time.deltaTime * 2f));
            if (who == null) { yield return null; continue; }   // (quien iba sentado ya no existe —un cuerpo de prueba destruido en pleno vuelo—: la alfombra termina sola)
            who.transform.SetPositionAndRotation(p + Vector3.up * (0.12f + rootAbove), transform.rotation);
            if (a != null) DailyPoses.Sit().ApplyFinal(a);   // sentado, tranquilo, esperando desembarcar
            yield return null;
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
        who.transform.position = dest + Vector3.up * (0.05f + rootAbove);   // de pie ENCIMA de la alfombra (antes se bajaba 1,6 m a un lado y en una casa del árbol caía al vacío)
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
}
