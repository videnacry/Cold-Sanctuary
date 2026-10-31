using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ÁREA DE ALFOMBRAS VOLADORAS (docs/alfombras-plan.md, Berón 2026-10-27). Ya no hay escaleras ni puentes: se va de un
/// piso a otro y de un edificio a otro en alfombra. Cada área es de un TIPO, y sus alfombras solo llevan a las de su red:
/// - <see cref="Kind.Edificio"/>: una en cada piso de un edificio (y en el suelo de hielo, a su pie, y en su techo): a los
///   demás pisos del MISMO edificio.
/// - <see cref="Kind.Santuario"/>: en el techo de cada edificio y en la plataforma central de la residencia: a los demás
///   edificios.
/// - <see cref="Kind.Residencia"/> (plataforma central de la residencia) ↔ <see cref="Kind.Habitacion"/> (una en cada casa
///   de árbol; su alfombra es también su esterilla de yoga).
/// Así cada selector tiene pocas opciones. Las áreas del mismo piso (el techo: edificio + santuario) comparten
/// <see cref="surface"/>: entre ellas se va andando.
/// </summary>
public class CarpetPad : MonoBehaviour
{
    public enum Kind { Edificio, Santuario, Residencia, Habitacion }

    public static readonly List<CarpetPad> All = new List<CarpetPad>();

    [Tooltip("Nombre que se ve en el diálogo de destinos (el piso, el edificio o la habitación).")]
    public string displayName = "Alfombras";
    public Kind kind;
    [Tooltip("Edificio al que pertenece (las alfombras de edificio solo van a sus pisos).")]
    public string building = "";
    [Tooltip("Piso dentro del edificio (0 = suelo de hielo; el techo es el más alto). Ordena los destinos.")]
    public int floor;
    [Tooltip("El piso donde está: las áreas con la misma superficie se unen andando.")]
    public string surface = "";
    [Tooltip("Huecos de aterrizaje (dónde se posa cada alfombra), en coordenadas locales.")]
    public Vector3[] slots = new Vector3[0];

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    /// <summary>¿Vuela una alfombra de aquí hasta <paramref name="other"/>? (misma red)</summary>
    public bool FliesTo(CarpetPad other)
    {
        if (other == null || other == this) return false;
        switch (kind)
        {
            case Kind.Edificio: return other.kind == Kind.Edificio && other.building == building && other.surface != surface;   // (no a otra esterilla del mismo piso)
            case Kind.Santuario: return other.kind == Kind.Santuario;
            case Kind.Residencia: return other.kind == Kind.Habitacion;
            default: return other.kind == Kind.Residencia;
        }
    }

    // Las áreas cuyas alfombras pueden acabar aquí (la misma red de vuelos).
    bool SameNetwork(CarpetPad p) => kind == Kind.Edificio ? p.kind == Kind.Edificio && p.building == building
        : kind == Kind.Santuario ? p.kind == Kind.Santuario : p.kind == Kind.Residencia || p.kind == Kind.Habitacion;

    /// <summary>Destinos de las alfombras de aquí, en orden (los pisos de abajo arriba, los edificios por nombre).</summary>
    public List<CarpetPad> Destinations()
    {
        var l = new List<CarpetPad>();
        // Un destino por PISO (en la sala de yoga cada esterilla es una alfombra: no se lista cada una).
        var seen = new HashSet<string>();
        foreach (CarpetPad p in All) if (FliesTo(p) && seen.Add(p.surface)) l.Add(p);
        l.Sort((a, b) => a.floor != b.floor && kind == Kind.Edificio ? a.floor.CompareTo(b.floor) : string.CompareOrdinal(a.displayName, b.displayName));
        return l;
    }

    // ── EL MENÚ DEL JUGADOR, POR ÁREAS (Berón 2026-10-30: «Hospital», no «Hospital (hospitalización)», «Hospital
    // (veterinaria)»…). La red de vuelos (FliesTo/Destinations, la que usan los trabajadores) no cambia: esto solo agrupa lo
    // que se le ofrece al jugador. Cada área sale UNA vez y la alfombra va directa a su pista PRINCIPAL: la de su piso más
    // bajo (la entrada, la recepción; siempre la misma, no depende de dónde esté uno). Los pisos y anillos de un área solo
    // se listan uno a uno cuando uno ya está en ella.

    /// <summary>Una opción del menú de vuelo: a qué pista va, o (si <see cref="tower"/>) abre las áreas de esa torre.</summary>
    public struct MenuEntry { public string label; public CarpetPad target; public string tower; }

    bool IsRing => surface.Contains("#anillo");

    // El piso del que cuelga un anillo (su área de alfombras de piso); el resto, la misma pista.
    CarpetPad OwnerFloor()
    {
        if (!IsRing) return this;
        for (Transform t = transform.parent; t != null; t = t.parent)
            foreach (CarpetPad p in All)
                if (p != this && p.kind == Kind.Edificio && p.building == building && !p.IsRing && p.transform.parent == t) return p;
        return this;
    }

    // «Hospital (veterinaria) · planta 3» → «Hospital (veterinaria)».
    string BaseName() { int i = displayName.IndexOf(" · "); return i > 0 ? displayName.Substring(0, i) : displayName; }

    /// <summary>El ÁREA de esta pista: sin «· planta N»; un anillo es de su planta; «Hospital (veterinaria)» es «Hospital» si
    /// el edificio tiene otra planta «Hospital» (si no, «Dojo (artes marciales)» se queda entero).</summary>
    public string AreaName
    {
        get
        {
            string n = OwnerFloor().BaseName();
            int par = n.IndexOf(" (");
            if (par > 0)
            {
                string root = n.Substring(0, par);
                foreach (CarpetPad p in All) if (p != null && p.kind == Kind.Edificio && p.building == building && p.BaseName() == root) return root;
            }
            return n;
        }
    }

    // Una de las plantas de un área, en corto, para cuando uno ya está en ella: «Veterinaria · planta 3», «Anillo de urgencias».
    string FloorLabel(string area)
    {
        string b = BaseName();
        if (b.Length > area.Length + 3 && b.StartsWith(area + " (") && b.EndsWith(")"))
        {
            string inner = b.Substring(area.Length + 2, b.Length - area.Length - 3);
            return char.ToUpper(inner[0]) + inner.Substring(1) + displayName.Substring(b.Length);
        }
        return displayName;
    }

    // ¿Es un piso de servicio de su torre (el suelo de hielo, el techo con la pista del santuario) y no un área?
    bool ServicePad()
    {
        if (floor == 0) return true;
        foreach (CarpetPad p in All) if (p != null && p.kind != Kind.Edificio && p.surface == surface) return true;
        return false;
    }

    // La pista principal de un grupo: la del piso más bajo, mejor la del piso que la de su anillo.
    static CarpetPad Main(List<CarpetPad> l)
    {
        CarpetPad best = null;
        foreach (CarpetPad p in l)
            if (best == null || p.floor < best.floor || (p.floor == best.floor && best.IsRing && !p.IsRing)) best = p;
        return best;
    }

    // De abajo arriba; en el mismo piso, la planta antes que su anillo.
    static int ByFloor(MenuEntry x, MenuEntry y) =>
        x.target.floor != y.target.floor ? x.target.floor.CompareTo(y.target.floor)
        : x.target.IsRing != y.target.IsRing ? x.target.IsRing.CompareTo(y.target.IsRing) : string.CompareOrdinal(x.label, y.label);

    /// <summary>El nombre corto de la pista para «Has llegado a …»: la del santuario, el de su torre (no la lista de sus áreas).</summary>
    public string ShortName => kind == Kind.Santuario ? building : displayName;

    /// <summary>Las áreas de la torre <paramref name="tower"/> (sin el suelo de hielo ni el techo), cada una con su pista principal,
    /// de abajo arriba.</summary>
    public static List<MenuEntry> AreasOfTower(string tower)
    {
        var groups = new Dictionary<string, List<CarpetPad>>();
        var order = new List<string>();
        foreach (CarpetPad p in All)
        {
            if (p == null || p.kind != Kind.Edificio || p.building != tower || p.ServicePad()) continue;
            string a = p.AreaName;
            if (!groups.TryGetValue(a, out var l)) { groups[a] = l = new List<CarpetPad>(); order.Add(a); }
            l.Add(p);
        }
        var r = new List<MenuEntry>();
        foreach (string a in order) r.Add(new MenuEntry { label = a, target = Main(groups[a]) });
        r.Sort(ByFloor);
        return r;
    }

    /// <summary>Lo que el JUGADOR ve en el selector de esta alfombra. Edificio: una opción por área de la torre (las plantas y
    /// anillos de la suya, uno a uno); santuario: una por torre (las de varias áreas abren un segundo menú, <see cref="MenuEntry.tower"/>);
    /// residencia y habitaciones: sus destinos, como siempre.</summary>
    public List<MenuEntry> PlayerMenu()
    {
        var r = new List<MenuEntry>();
        if (kind == Kind.Edificio)
        {
            string mine = ServicePad() ? null : AreaName;
            var groups = new Dictionary<string, List<CarpetPad>>();
            var order = new List<string>();
            var seen = new HashSet<string> { surface };
            foreach (CarpetPad p in All)
            {
                if (p == null || p.kind != Kind.Edificio || p.building != building || !seen.Add(p.surface)) continue;
                string a = p.AreaName;
                if (a == mine) { r.Add(new MenuEntry { label = p.FloorLabel(a), target = p }); continue; }   // su propia área: planta a planta
                if (!groups.TryGetValue(a, out var l)) { groups[a] = l = new List<CarpetPad>(); order.Add(a); }
                l.Add(p);
            }
            foreach (string a in order) r.Add(new MenuEntry { label = a, target = Main(groups[a]) });
            r.Sort(ByFloor);
            return r;
        }
        if (kind == Kind.Santuario)
        {
            var towers = new Dictionary<string, List<CarpetPad>>();
            foreach (CarpetPad p in All)
                if (FliesTo(p)) { if (!towers.TryGetValue(p.building, out var l)) towers[p.building] = l = new List<CarpetPad>(); l.Add(p); }
            foreach (var kv in towers)
            {
                List<MenuEntry> areas = AreasOfTower(kv.Key);
                if (areas.Count >= 2) r.Add(new MenuEntry { label = kv.Key + " ›", tower = kv.Key });
                else r.Add(new MenuEntry { label = kv.Key, target = areas.Count == 1 ? areas[0].target : Main(kv.Value) });
            }
            r.Sort((x, y) => string.CompareOrdinal(x.label, y.label));
            return r;
        }
        foreach (CarpetPad p in Destinations()) r.Add(new MenuEntry { label = p.displayName, target = p });
        return r;
    }

    /// <summary>Un hueco libre para que aterrice una alfombra (mundo).</summary>
    public Vector3 FreeSlot(FlyingCarpet except = null) => except != null ? Claim(except) : SlotPoint(0);

    // QUÉ ALFOMBRA OCUPA CADA HUECO de esta pista (Berón 2026-10-28: se solapaban). Cada alfombra que aparca o viene de
    // camino se queda con el hueco libre más bajo; si hay más alfombras que huecos, se APILA encima de la del mismo hueco
    // (una capa de StackStep). Es una cuenta, sin física: barato aunque haya muchas pistas.
    public const float StackStep = 0.03f;
    readonly Dictionary<FlyingCarpet, int> _claim = new Dictionary<FlyingCarpet, int>();
    bool _synced;

    Vector3 SlotPoint(int i)
    {
        int n = Mathf.Max(1, slots.Length);
        Vector3 s = slots.Length > 0 ? slots[i % n] : Vector3.up * 0.012f;
        s.y += (i / n) * StackStep;
        return transform.TransformPoint(s);
    }

    /// <summary>Reserva para <paramref name="c"/> el hueco libre más bajo de esta pista (y lo suelta de la anterior).
    /// Devuelve dónde se posa.</summary>
    public Vector3 Claim(FlyingCarpet c)
    {
        if (!_synced)   // las que ya estaban aparcadas aquí al empezar: su hueco, el más cercano
        {
            _synced = true;
            foreach (FlyingCarpet f in FlyingCarpet.All)
                if (f != null && f.pad == this && !f.Flying && !_claim.ContainsKey(f)) _claim[f] = Nearest(f.transform.position);
        }
        foreach (CarpetPad p in All) if (p != this) p._claim.Remove(c);
        if (_claim.TryGetValue(c, out int have)) return SlotPoint(have);
        int i = 0; while (_claim.ContainsValue(i)) i++;
        _claim[c] = i;
        return SlotPoint(i);
    }

    int Nearest(Vector3 w)
    {
        int best = 0; float bd = float.MaxValue;
        for (int i = 0; i < Mathf.Max(1, slots.Length); i++)
        {
            float d = (SlotPoint(i) - w).sqrMagnitude;
            if (d < bd && !_claim.ContainsValue(i)) { bd = d; best = i; }
        }
        return best;
    }

    /// <summary>¿Cuántas alfombras tiene (aparcadas o llegando)?</summary>
    public int Carpets => _claim.Count;

    /// <summary>Una alfombra posada aquí y libre (la más cercana a <paramref name="near"/>).</summary>
    public FlyingCarpet IdleCarpet(Vector3 near)
    {
        FlyingCarpet best = null; float bd = float.MaxValue;
        foreach (FlyingCarpet c in FlyingCarpet.All)
        {
            if (c.Flying || c.pad != this) continue;
            float d = (c.transform.position - near).sqrMagnitude;
            if (d < bd) { bd = d; best = c; }
        }
        return best;
    }

    /// <summary>Si aquí no queda ninguna alfombra libre, ACUDE una de su red (de donde haya más): las alfombras se van
    /// quedando donde aterrizan y, sin esto, un piso podía quedarse sin ninguna.</summary>
    public FlyingCarpet Summon(Vector3 near)
    {
        FlyingCarpet c = IdleCarpet(near);
        if (c != null) return c;
        var count = new Dictionary<CarpetPad, int>();
        foreach (FlyingCarpet f in FlyingCarpet.All)
            if (!f.Flying && f.pad != null && f.pad != this && SameNetwork(f.pad))
                count[f.pad] = count.TryGetValue(f.pad, out int n) ? n + 1 : 1;
        CarpetPad from = null; int most = 0;
        foreach (var kv in count) if (kv.Value > most) { most = kv.Value; from = kv.Key; }
        if (from == null)
        {
            // Ninguna en su red: el jugador vuela directo del santuario a un área (PlayerMenu) y deja la alfombra en la red de esa
            // torre; con el tiempo la del santuario podía vaciarse. Acude una de cualquier pista que tenga dos o más libres.
            count.Clear();
            foreach (FlyingCarpet f in FlyingCarpet.All)
                if (!f.Flying && f.pad != null && f.pad != this) count[f.pad] = count.TryGetValue(f.pad, out int n) ? n + 1 : 1;
            foreach (var kv in count) if (kv.Value > Mathf.Max(1, most)) { most = kv.Value; from = kv.Key; }
        }
        if (from == null) return null;
        c = from.IdleCarpet(transform.position);
        if (c == null) return null;
        c.transform.SetPositionAndRotation(FreeSlot(c), transform.rotation);
        c.pad = this;
        return c;
    }

    // El jugador entra en un área sin alfombras libres: acude una.
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || IdleCarpet(other.transform.position) != null) return;
        if (Summon(other.transform.position) != null) Debug.Log($"[Alfombra] Acude una alfombra a «{displayName}».");
    }

    /// <summary>El área en la que está (o la de su piso) quien está en <paramref name="p"/>: la más cercana, contando la
    /// altura cuatro veces (un piso de arriba, aunque esté justo encima, no es «aquí»).</summary>
    public static CarpetPad For(Vector3 p)
    {
        CarpetPad deck = OnDeck(p);
        if (deck != null) return deck;
        // Uno está sobre el piso de DEBAJO de sus pies (o en él): el de arriba, aunque quede más cerca en planta, no es «aquí»
        // (en el anillo de 4 m sobre la planta 1, la planta 2 quedaba más cerca y se iba y volvía de un piso a otro).
        CarpetPad best = null; float bd = float.MaxValue;
        for (int pass = 0; pass < 2 && best == null; pass++)
            foreach (CarpetPad pad in All)
            {
                Vector3 d = pad.transform.position - p;
                if (pass == 0 && d.y > 1.5f) continue;
                float dy = d.y * 4f; d.y = 0f;
                float s = d.sqrMagnitude + dy * dy;
                if (s < bd) { bd = s; best = pad; }
            }
        return best;
    }

    // EL PISO QUE PISA: si bajo p hay una plataforma (un piso, un anillo…) con área de alfombras PROPIA, es la suya, aunque la
    // de otro piso quede más cerca contando la altura (en una mesa del anillo del comedor, lejos de sus alfombras, salía la
    // planta de la cocina, 4 m más abajo: el turista se iba volando a la cocina). La plataforma más honda de la jerarquía
    // manda (el anillo es hijo del área de su planta). Cacheado por collider.
    static readonly Dictionary<Collider, CarpetPad> _deckOf = new Dictionary<Collider, CarpetPad>();
    static int _deckPads = -1;

    static CarpetPad OnDeck(Vector3 p)
    {
        if (_deckPads != All.Count) { _deckOf.Clear(); _deckPads = All.Count; }
        RaycastHit[] hits = Physics.RaycastAll(p + Vector3.up * 0.5f, Vector3.down, 3f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit h in hits)
        {
            Collider id = h.collider;
            if (!_deckOf.TryGetValue(id, out CarpetPad deck))
            {
                int bestDepth = -1;
                foreach (CarpetPad pad in All)
                {
                    Transform floor = pad != null ? pad.transform.parent : null;
                    if (floor == null || !h.transform.IsChildOf(floor)) continue;
                    int depth = 0; for (Transform x = floor; x != null; x = x.parent) depth++;
                    if (depth > bestDepth) { bestDepth = depth; deck = pad; }
                }
                _deckOf[id] = deck;
            }
            if (deck == null) continue;
            // Varias áreas en esa plataforma (o la del suelo de hielo, hija de la planta baja): la de ese piso más cercana.
            CarpetPad best = null; float bd = float.MaxValue;
            foreach (CarpetPad pad in All)
            {
                if (pad == null || pad.transform.parent != deck.transform.parent) continue;
                Vector3 d = pad.transform.position - p; float dy = d.y * 4f; d.y = 0f;
                float s = d.sqrMagnitude + dy * dy;
                if (s < bd) { bd = s; best = pad; }
            }
            return best;
        }
        return null;
    }

    /// <summary>
    /// Ruta de un trabajador (IA) hacia <paramref name="to"/>: si el destino está en OTRO piso o edificio, sigue la cadena
    /// de alfombras (piso → techo → techo del destino → su piso), andando dentro de cada piso. Cambia <paramref name="to"/>
    /// por el área a la que tiene que andar; true = vuela ahora (no hay que moverlo).
    /// </summary>
    static float _noCarpetLog;
    // LA ÚLTIMA DECISIÓN de Route para cada uno (solo se formatea al pedirla: el registro de quien «no avanza»). Kushal se
    // quedaba quieto en el techo de Servicios sin volar a la barra y el registro no decía qué rama de la ruta lo paraba.
    struct Note { public string why; public CarpetPad a, b, next; public Vector3 to, at; public float time; public int frame; }
    static readonly Dictionary<GameObject, Note> _notes = new Dictionary<GameObject, Note>();
    static bool Noted(GameObject g, bool r, string why, CarpetPad a = null, CarpetPad b = null, CarpetPad next = null, Vector3 to = default)
    { _notes[g] = new Note { why = why, a = a, b = b, next = next, to = to, at = g.transform.position, time = Time.time, frame = Time.frameCount }; return r; }
    /// <summary>Qué decidió la última ruta de <paramref name="g"/> (para los registros de atascos).</summary>
    public static string LastRoute(GameObject g)
    {
        if (g == null || !_notes.TryGetValue(g, out Note n)) return "sin ruta";
        return $"ruta hace {Time.frameCount - n.frame} fotogramas: {n.why} (desde ({n.at.x:0.0},{n.at.y:0.0},{n.at.z:0.0}), de {n.a?.displayName ?? "—"} a {n.b?.displayName ?? "—"}, siguiente {n.next?.displayName ?? "—"}, andar a ({n.to.x:0.0},{n.to.y:0.0},{n.to.z:0.0}))";
    }
    public static bool Route(GameObject g, ref Vector3 to)
    {
        if (FlyingCarpet.Riding(g)) return Noted(g, true, "volando");
        // El ÁRBOL DE LOS CAMPOS (docs/edificio-arbol.md): entre sus pisos no hay alfombras; a la puerta y nadando por la raíz.
        // (Aquí y no en PathMover: la jornada, las iniciaciones y los hechizos llaman a Route directamente.)
        ArbolCampos.Hop tree = ArbolCampos.Route(g, ref to);
        if (tree == ArbolCampos.Hop.Swimming) return Noted(g, true, "nadando por el árbol");
        if (tree == ArbolCampos.Hop.Walk) return Noted(g, false, "andando por el árbol", to: to);
        CarpetPad a = For(g.transform.position), b = For(to);
        if (a == null || b == null || a.surface == b.surface) return Noted(g, false, "mismo piso", a, b, to: to);   // mismo piso: andando
        CarpetPad next = NextHop(a, b);
        if (next == null) return Noted(g, false, "sin camino de alfombras", a, b, to: to);
        // A OTRA ÁREA DE ESTE PISO (en el techo: de la del santuario a la del edificio). Si ya está junto a ella (a menos de 3 m),
        // sale de ella: en el techo de Servicios las dos pistas están casi juntas y «la más cercana» cambiaba de una a otra a
        // cada paso; Kushal se quedaba entre las dos mandado de una a la otra y nunca volaba a la barra (2026-10-30).
        if (next.surface == a.surface)
        {
            if ((g.transform.position - next.transform.position).sqrMagnitude > 9f) { to = next.transform.position; return Noted(g, false, "a la otra pista del piso", a, b, next, to); }
            a = next; next = NextHop(a, b);
            if (next == null || next.surface == a.surface) return Noted(g, false, "sin vuelo desde la otra pista", a, b, next, to);
        }
        if ((g.transform.position - a.transform.position).sqrMagnitude > 9f) { to = a.transform.position; return Noted(g, false, "a su pista", a, b, next, to); }   // andar al área
        FlyingCarpet c = a.Summon(g.transform.position);
        if (c == null) { if (Time.time > _noCarpetLog) { _noCarpetLog = Time.time + 10f; Debug.LogWarning($"[Ruta] «{g.name}»: no hay ninguna alfombra libre en la red de {a.building} ({a.displayName}) para ir a {next.displayName}."); } return Noted(g, false, "sin alfombra libre", a, b, next, to); }
        Debug.Log($"[Ruta] «{g.name}»: de {a.building} · {a.displayName} hacia {b.building} · {b.displayName} (destino {to}); vuela a {next.displayName}.");
        c.Fly(g, next);
        return Noted(g, true, "sale volando", a, b, next, to);
    }

    /// <summary>
    /// EMPUJÓN: quien lleva un rato QUIETO junto a una pista de alfombras (a menos de 6 m, en su piso) camino de otro piso sale
    /// volando desde ella hacia <paramref name="to"/>, aunque la ruta normal le mande andar (en el techo de Servicios Kushal
    /// se quedaba entre sus dos pistas sin moverse ni volar, y Goluis a 2 m de la de la sala de yoga: no comían, JornadaTest
    /// 2026-10-31). Devuelve true si sale volando.
    /// </summary>
    public static bool ForceHop(GameObject g, Vector3 to)
    {
        if (g == null || FlyingCarpet.Riding(g)) return false;
        CarpetPad b = For(to);
        if (b == null) return false;
        Vector3 p = g.transform.position;
        CarpetPad from = null, hop = null; float bd = 36f;
        foreach (CarpetPad pad in All)
        {
            if (pad == null || pad.surface == b.surface) continue;
            Vector3 d = pad.transform.position - p;
            if (Mathf.Abs(d.y) > 2.5f) continue;
            d.y = 0f;
            if (d.sqrMagnitude >= bd) continue;
            CarpetPad next = NextHop(pad, b);
            if (next == null || next.surface == pad.surface) continue;
            bd = d.sqrMagnitude; from = pad; hop = next;
        }
        if (from == null) return false;
        FlyingCarpet c = from.Summon(p);
        if (c == null) return false;
        Debug.Log($"[Ruta] «{g.name}» llevaba un rato quieto junto a {from.displayName}: sale volando desde ella hacia {hop.displayName} (destino {b.displayName}).");
        c.Fly(g, hop);
        return Noted(g, true, "empujón: sale volando desde la pista de al lado", from, b, hop, to);
    }

    // Búsqueda en anchura por la red: andar dentro del piso no cuesta, cada vuelo cuenta uno. Devuelve el primer paso.
    static CarpetPad NextHop(CarpetPad a, CarpetPad b)
    {
        var cost = new Dictionary<CarpetPad, int> { { a, 0 } };
        var first = new Dictionary<CarpetPad, CarpetPad> { { a, null } };
        var dq = new LinkedList<CarpetPad>(); dq.AddFirst(a);
        while (dq.Count > 0)
        {
            CarpetPad u = dq.First.Value; dq.RemoveFirst();
            if (u.surface == b.surface) return first[u] ?? u;
            foreach (CarpetPad v in All)
            {
                if (v == u) continue;
                bool walk = v.surface == u.surface, fly = !walk && u.FliesTo(v);
                if (!walk && !fly) continue;
                int c = cost[u] + (fly ? 1 : 0);
                if (cost.TryGetValue(v, out int old) && old <= c) continue;
                cost[v] = c; first[v] = first[u] ?? v;
                if (walk) dq.AddFirst(v); else dq.AddLast(v);
            }
        }
        return null;
    }

    // ── CONSTRUCCIÓN (la usan los constructores de la escena) ──────────────────────────────────────────────────────

    /// <summary>Monta un ÁREA DE ALFOMBRAS: una zona marcada en el suelo de <paramref name="w"/> × <paramref name="d"/> m con
    /// <paramref name="carpets"/> alfombras posadas (y un hueco más para las que lleguen).</summary>
    public static CarpetPad BuildZone(Transform parent, Vector3 worldPos, Quaternion rot, Kind kind, string building, int floor, string surface,
                                      string displayName, int carpets, float w = 7.6f, float d = 4.4f)
    {
        Transform t = new GameObject($"Alfombras_{kind}_{displayName}").transform; t.SetParent(parent, true);
        t.SetPositionAndRotation(worldPos, rot);
        Color mark = kind == Kind.Santuario ? new Color(0.35f, 0.5f, 0.75f) : kind == Kind.Edificio ? new Color(0.8f, 0.62f, 0.3f) : new Color(0.55f, 0.72f, 0.5f);
        KitchenProps.C(t, new Vector3(0f, 0.006f, 0f), new Vector3(w, 0.012f, d), new Color(0.9f, 0.87f, 0.8f), 0f, "Zona");
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(t, new Vector3(0f, 0.008f, s * (d * 0.5f - 0.08f)), new Vector3(w, 0.014f, 0.16f), mark, 0f, "Borde");
            KitchenProps.C(t, new Vector3(s * (w * 0.5f - 0.08f), 0.008f, 0f), new Vector3(0.16f, 0.014f, d), mark, 0f, "Borde");
        }
        BoxCollider trig = t.gameObject.AddComponent<BoxCollider>(); trig.isTrigger = true; trig.center = new Vector3(0f, 1f, 0f); trig.size = new Vector3(w, 2f, d);
        CarpetPad pad = t.gameObject.AddComponent<CarpetPad>();
        pad.displayName = displayName; pad.kind = kind; pad.building = building; pad.floor = floor; pad.surface = surface;
        int n = Mathf.Max(1, Mathf.FloorToInt((w - 0.4f) / 2.5f));
        var slots = new List<Vector3>();
        for (int i = 0; i < n; i++) slots.Add(new Vector3(-(n - 1) * 1.25f + i * 2.5f, 0.012f, 0f));   // a ras del suelo: finas, no se hunde nadie en ellas
        pad.slots = slots.ToArray();
        for (int i = 0; i < Mathf.Min(carpets, slots.Count); i++) BuildCarpet(t, pad, slots[i], i);
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>();
        wl.text = kind == Kind.Santuario ? "Alfombras del santuario" : kind == Kind.Edificio ? $"Alfombras de {building} · {displayName}"
                : kind == Kind.Residencia ? "Alfombras a las habitaciones" : "Alfombra (y esterilla de yoga)";
        wl.height = 2.4f;
        return pad;
    }

    static readonly Color[] Colors = { new Color(0.75f, 0.2f, 0.25f), new Color(0.2f, 0.35f, 0.7f), new Color(0.85f, 0.6f, 0.15f), new Color(0.3f, 0.6f, 0.4f), new Color(0.55f, 0.3f, 0.65f), new Color(0.15f, 0.55f, 0.6f) };

    /// <summary>Una alfombra (2 × 3 m: también vale de esterilla de yoga grande) posada en <paramref name="local"/>.</summary>
    public static FlyingCarpet BuildCarpet(Transform zone, CarpetPad pad, Vector3 local, int i)
    {
        Transform c = new GameObject($"Alfombra_{i + 1}").transform; c.SetParent(zone, false);
        c.localPosition = local;
        BoxCollider bc = c.gameObject.AddComponent<BoxCollider>(); bc.isTrigger = true; bc.center = new Vector3(0f, 0.6f, 0f); bc.size = new Vector3(2.1f, 1.2f, 3.1f);
        KitchenProps.C(c, new Vector3(0f, 0.008f, 0f), new Vector3(2f, 0.016f, 3f), Colors[i % Colors.Length], 0f, "Tejido");
        KitchenProps.C(c, new Vector3(0f, 0.01f, 0f), new Vector3(1.5f, 0.016f, 2.4f), new Color(0.95f, 0.85f, 0.45f), 0f, "Dibujo");
        for (int e = -1; e <= 1; e += 2) KitchenProps.C(c, new Vector3(0f, 0.006f, e * 1.58f), new Vector3(1.9f, 0.012f, 0.16f), Color.white, 0f, "Flecos");
        FlyingCarpet fc = c.gameObject.AddComponent<FlyingCarpet>(); fc.pad = pad;
        return fc;
    }
}
