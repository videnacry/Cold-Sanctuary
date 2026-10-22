using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SIMULACIÓN DE COMPASIÓN (docs/compassion-simulations.md): reencarna un momento histórico real en el que una persona o
/// un pueblo protegió a otros arriesgándolo todo — Le Chambon-sur-Lignon (1940–44) o Irena Sendler en Varsovia (1942–43).
/// El jugador guía a los refugiados (E: «sígueme»), los esconde, les lleva pan, les consigue papeles y los pone a salvo,
/// esquivando las patrullas. Al poner a salvo a <see cref="goal"/>, la <see cref="HistoryCard"/> cuenta lo que pasó de
/// verdad, con sus fuentes. Una sola tecla (E), con la acción del sitio en el HUD.
/// </summary>
[RequireComponent(typeof(HistoryCard))]
public class CompassionMission : MonoBehaviour
{
    public string title = "Le Chambon-sur-Lignon, 1942";
    public int goal = 10;
    [Tooltip("Irena Sendler: cada niño a salvo deja su nombre real en el frasco del manzano.")]
    public bool recordNames;
    public float reach = 2.5f;

    public static CompassionMission Instance { get; private set; }
    public static readonly List<Refugee> Refugees = new List<Refugee>();

    Transform _player;
    int _bread;
    bool _done;
    HistoryCard _card;
    string _note; float _noteUntil;
    readonly HashSet<Refugee> _counted = new HashSet<Refugee>();

    public int Saved
    {
        get { int n = 0; foreach (Refugee r in Refugees) if (r != null && r.Saved) n++; return n; }
    }

    void OnEnable() => Instance = this;
    void OnDisable() { if (Instance == this) Instance = null; }

    void Start()
    {
        Refugees.Clear();
        Refugees.AddRange(FindObjectsByType<Refugee>(FindObjectsSortMode.None));
        _card = GetComponent<HistoryCard>();
        if (recordNames) _card.extra = () => NameJar.Instance != null ? "En el frasco: " + string.Join(", ", NameJar.Instance.names) : null;
    }

    public void Notify(string msg) { _note = msg; _noteUntil = Time.time + 4f; Debug.Log($"[Compasión] {msg}"); }

    Transform Player()
    {
        if (_player == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p != null) _player = p.transform; }
        return _player;
    }

    List<Refugee> Followers()
    {
        var l = new List<Refugee>();
        foreach (Refugee r in Refugees) if (r != null && r.Leader == _player && !r.Saved) l.Add(r);
        return l;
    }

    static T Near<T>(Vector3 p, float extra, System.Func<T, float> radius) where T : MonoBehaviour
    {
        T best = null; float bd = float.MaxValue;
        foreach (T t in FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            float d = Vector3.Distance(new Vector3(t.transform.position.x, p.y, t.transform.position.z), p);
            if (d <= radius(t) + extra && d < bd) { bd = d; best = t; }
        }
        return best;
    }

    /// <summary>Qué haría E aquí (texto del HUD) y la acción.</summary>
    string Action(out System.Action act)
    {
        act = null;
        Transform pl = Player(); if (pl == null) return null;
        Vector3 p = pl.position;
        List<Refugee> f = Followers();

        Passage pa = Near<Passage>(p, reach, x => x.radius);
        if (pa != null && pa.exit != null)
        {
            if (!pa.Ready) return $"{pa.label}: aún no ha vuelto";
            if (pa.alone) { act = () => { foreach (Refugee r in f) r.Follow(null); Cross(pa, new List<Refugee>()); }; return f.Count > 0 ? $"E · cruzar {pa.label} (solo tú: ellos no tienen pase)" : $"E · cruzar {pa.label}"; }
            act = () => Cross(pa, f);
            return f.Count > 0 ? $"E · cruzar por {pa.label} con {Take(pa, f)}" : $"E · cruzar por {pa.label}";
        }
        SafeHaven sh = Near<SafeHaven>(p, reach, x => x.radius);
        if (sh != null && f.Count > 0)
        {
            if (sh.Full) return $"{sh.label}: ya no caben más";
            act = () => Deliver(sh, f);
            return $"E · dejar a {f.Count} a salvo en {sh.label}";
        }
        PaperForge pf = Near<PaperForge>(p, reach, x => x.radius);
        if (pf != null)
        {
            int need = 0; foreach (Refugee r in f) if (!r.hasPapers) need++;
            if (need > 0 && pf.blanks > 0) { act = () => Forge(pf, f); return $"E · papeles falsos para {Mathf.Min(need, pf.blanks)} ({pf.blanks} impresos)"; }
            if (need > 0) return $"{pf.label}: no quedan impresos";
        }
        BreadSource bs = Near<BreadSource>(p, reach, x => x.radius);
        if (bs != null && _bread < bs.carry) { act = () => { _bread = bs.carry; Notify($"Llevas {_bread} raciones de {bs.label}."); }; return $"E · coger pan ({bs.label})"; }
        HidingPlace hp = Near<HidingPlace>(p, reach, x => x.radius);
        if (hp != null)
        {
            if (f.Count > 0 && !hp.Full) { act = () => HideAll(hp, f); return $"E · esconder en {hp.label} ({hp.Count}/{hp.capacity})"; }
            if (_bread > 0) { act = () => { hp.AddFood(_bread); Notify($"Dejas {_bread} raciones en {hp.label} (despensa: {hp.food:0})."); _bread = 0; }; return $"E · dejar pan en {hp.label} (despensa: {hp.food:0})"; }
            if (hp.Count > 0 && f.Count == 0) { act = () => Notify($"Salen {hp.CallOut(pl)} de {hp.label} y te siguen."); return $"E · sacar a los escondidos de {hp.label}"; }
        }
        Refugee near = null; float bd = reach;
        foreach (Refugee r in Refugees)
        {
            if (r == null || r.Saved || r.Hidden || r.Leader == pl) continue;
            float d = Vector3.Distance(r.transform.position, p);
            if (d < bd) { bd = d; near = r; }
        }
        if (near != null) { Refugee n = near; act = () => { n.Follow(pl); Notify($"{n.displayName} te sigue."); }; return $"E · «{near.displayName}, sígueme»"; }
        return null;
    }

    static int Take(Passage pa, List<Refugee> f) => pa.perTrip > 0 ? Mathf.Min(pa.perTrip, f.Count) : f.Count;

    void Cross(Passage pa, List<Refugee> f)
    {
        int n = Take(pa, f);
        Vector3 to = pa.exit.position;
        Teleport(_player, to);
        for (int i = 0; i < n; i++) Teleport(f[i].transform, to + Quaternion.Euler(0f, i * 50f, 0f) * Vector3.forward * 1.5f);
        for (int i = n; i < f.Count; i++) f[i].Follow(null);   // los que no caben esperan aquí
        pa.Used();
        Notify(n < f.Count ? $"Por {pa.label} solo caben {n}: los demás esperan." : $"Cruzáis por {pa.label}.");
    }

    void Deliver(SafeHaven sh, List<Refugee> f)
    {
        int n = 0;
        foreach (Refugee r in f) { if (sh.Full) break; r.MarkSaved(); sh.received++; n++; Vector2 o = Random.insideUnitCircle * sh.radius * 0.5f; Teleport(r.transform, sh.transform.position + new Vector3(o.x, 0f, o.y)); }
        Notify($"{n} a salvo en {sh.label}.");
    }

    void Forge(PaperForge pf, List<Refugee> f)
    {
        int n = 0;
        foreach (Refugee r in f) if (!r.hasPapers && pf.blanks > 0) { r.hasPapers = true; pf.blanks--; n++; }
        Notify($"{n} con papeles nuevos: las patrullas ya no los detienen.");
    }

    void HideAll(HidingPlace hp, List<Refugee> f)
    {
        int n = 0;
        foreach (Refugee r in f) if (hp.TryHide(r)) n++;
        Notify($"{n} escondidos en {hp.label}." + (hp.food < hp.Count ? " La despensa está casi vacía: trae pan." : ""));
    }

    static void Teleport(Transform t, Vector3 to)
    {
        var cc = t.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false;
        var nav = t.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav != null && nav.isOnNavMesh) nav.Warp(to); else t.position = to;
        if (cc != null) cc.enabled = true;
    }

    void Update()
    {
        if (_card.IsOpen) return;
        foreach (Refugee r in Refugees)
            if (r != null && r.Saved && _counted.Add(r) && recordNames) NameJar.Record(r.displayName);
        if (!_done && Saved >= goal) Finish();
        if (GameKeys.DialogOpen || !GameKeys.GetKeyDown(KeyCode.E)) return;
        Action(out System.Action act);
        act?.Invoke();
    }

    void Finish()
    {
        _done = true; _card.ShowFinal();
        CharacterLevel lv = Player() != null ? Player().GetComponentInParent<CharacterLevel>() : null;
        if (lv == null) lv = FindFirstObjectByType<CharacterLevel>();
        if (lv != null) lv.GainBondXp(50f);   // marga de los Vínculos: cuidar de otros
        Notify($"¡{Saved} a salvo! Lo que pasó de verdad, en la tarjeta.");
    }

    void OnGUI()
    {
        if (_card == null || _card.IsOpen) return;
        GUIStyle wrap = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 15 };
        string hint = Action(out _);
        GUI.Box(new Rect(10f, 10f, 380f, 74f), "");
        GUI.Label(new Rect(20f, 14f, 360f, 22f), title, wrap);
        GUI.Label(new Rect(20f, 36f, 360f, 22f), $"A salvo: {Saved}/{goal}   ·   te siguen: {Followers().Count}   ·   pan: {_bread}", wrap);
        if (!string.IsNullOrEmpty(hint)) GUI.Label(new Rect(20f, 58f, 360f, 22f), hint, wrap);
        if (Time.time < _noteUntil) GUI.Label(new Rect(Screen.width * 0.5f - 250f, Screen.height - 80f, 500f, 50f), _note, new GUIStyle(wrap) { alignment = TextAnchor.MiddleCenter });
    }
}
