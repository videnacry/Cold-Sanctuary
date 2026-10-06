using UnityEngine;

/// <summary>
/// HECHIZO DE COMUNICACIÓN — "todas las animas pueden hablar entre sí". Su ACCIÓN NATURAL es vocalizar/expresar (la
/// <see cref="Mind"/> ya *piensa*); el hechizo lo lleva a **charla**: cada cierto rato, dos animas cercanas que lo
/// tengan **alternan sus pensamientos** como diálogo (ligado al **campo de pensamientos** y a los **pensamientos
/// nativos** de cada una — ver docs/spell-genesis-and-sanctuary-progression.md §7). Al hablar hay **contagio
/// emocional** (ciencia real: la emoción se mimetiza entre próximos): el que escucha mueve sus humores hacia la
/// VALENCIA del que habla, escalado por el **vínculo** (más bond → más contagio); y charlar en positivo **crece la
/// amistad**. Opt-in = es un hechizo que se APRENDE (no todos lo tienen innato), coherente con el modelo de desbloqueo
/// por camino. En el mundo insecto (humanoides) se ve como diálogo; mecánicamente = intercambiar pensamientos.
/// </summary>
public class CommunicationSpell : SpellBase
{
    [Header("Charla")]
    [Tooltip("Radio en el que busca con quién hablar (si 0, usa `range` de SpellBase o 6 por defecto).")]
    [Min(0f)] public float chatRange = 6f;
    [Tooltip("Segundos BASE entre charlas (esporádicas: se le suma un jitter aleatorio).")]
    [Min(1f)] public float chatInterval = 12f;
    [Tooltip("Cuánto se contagia la valencia del interlocutor (×bond). Emoción compartida.")]
    [Range(0f, 1f)] public float contagion = 0.15f;
    [Tooltip("Solo habla con quien ya tiene algo de vínculo (evita 'hablar con cualquiera'). 0 = habla con cualquiera.")]
    [Range(0f, 100f)] public float minBondToChat = 0f;

    Anima _self;
    Mind _mind;
    float _next;
    static readonly Collider[] _hits = new Collider[24];

    void Awake()
    {
        _self = GetComponent<Anima>();
        _mind = GetComponent<Mind>();
        _next = Time.time + Random.Range(1f, chatInterval);
    }

    void Update()
    {
        PollInput();   // opt-in: si se asigna spellKey, el jugador puede iniciar una charla a mano
        if (_mind == null || _self == null || _self.death) return;
        if (Time.time < _next) return;
        _next = Time.time + chatInterval + Random.Range(0f, chatInterval);   // esporádico
        Converse();
    }

    protected override void OnCastPressed() => Converse();   // tecla manual = hablar ahora

    // ── Entradas de SpellBase (abstractos): targeting por IA/jugador ────────────────────────────
    public override bool CanCast(Anima caster, ITarget target) => _mind != null;

    /// <summary>Hablar con un objetivo CONCRETO (IA/targeting); si no tiene mente, cae a buscar al más cercano.</summary>
    public override void Cast(Anima caster, ITarget target)
    {
        Mind partner = target != null ? target.transform.GetComponentInParent<Mind>() : null;
        if (partner == null) { Converse(); return; }
        Anima pa = partner.GetComponent<Anima>();
        float bond = pa is ITarget it && _self.GetBond(it) is Bond b ? b.value : 0f;
        DoExchange(partner, pa, Mathf.Clamp01(bond / 100f));
    }

    /// <summary>Busca al interlocutor más cercano con mente (preferiendo al más vinculado) y alternan un pensamiento.</summary>
    public void Converse()
    {
        Mind partner = FindPartner(out Anima partnerAnima, out float bond01);
        if (partner != null) DoExchange(partner, partnerAnima, bond01);
    }

    void DoExchange(Mind partner, Anima partnerAnima, float bond01)
    {
        if (partner == null) return;
        string mine  = _mind.SpeakNow();
        string yours = partner.SpeakNow();
        if (string.IsNullOrEmpty(mine) && string.IsNullOrEmpty(yours)) return;
        Debug.Log($"[Charla] «{name}»: \"{mine}\"  ↔  «{partner.name}»: \"{yours}\"");

        // CONTAGIO EMOCIONAL (×vínculo): cada uno mueve sus humores hacia la valencia del otro.
        Contagiate(_mind, partner.lastPositive, bond01);
        Contagiate(partner, _mind.lastPositive, bond01);

        // Charlar en positivo estrecha la amistad (el vínculo nace/crece conversando).
        if (_mind.lastPositive && partner.lastPositive && partnerAnima is ITarget it)
            _self.GrowBond(it, BondType.Friend, 0.5f);
    }

    void Contagiate(Mind listener, bool speakerPositive, float bond01)
    {
        float amt = contagion * Mathf.Lerp(0.3f, 1f, bond01);   // más vínculo → más contagio
        if (speakerPositive) listener.humores.Produce(Humor.Serotonina, amt);
        else                 listener.humores.Produce(Humor.Cortisol, amt);
    }

    Mind FindPartner(out Anima partnerAnima, out float bond01)
    {
        partnerAnima = null; bond01 = 0f;
        float r = chatRange > 0f ? chatRange : (range > 0f ? range : 6f);
        int n = Physics.OverlapSphereNonAlloc(transform.position, r, _hits);
        Mind best = null; float bestScore = -1f;
        for (int i = 0; i < n; i++)
        {
            Anima other = _hits[i] != null ? _hits[i].GetComponentInParent<Anima>() : null;
            if (other == null || other == _self || other.death) continue;
            Mind om = other.GetComponent<Mind>();
            if (om == null) continue;

            float bond = (other is ITarget it && _self.GetBond(it) is Bond b) ? b.value : 0f;
            if (bond < minBondToChat) continue;
            // preferir al más vinculado; a igualdad, al más cercano.
            float score = bond + 1f - (other.transform.position - transform.position).magnitude / Mathf.Max(0.01f, r);
            if (score > bestScore) { bestScore = score; best = om; partnerAnima = other; bond01 = Mathf.Clamp01(bond / 100f); }
        }
        return best;
    }
}
