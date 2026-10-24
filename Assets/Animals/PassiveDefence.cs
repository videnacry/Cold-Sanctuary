using UnityEngine;

/// <summary>
/// DEFENSA PASIVA (docs/era-species-behavior.md §2): en vez de huir, el ser se ACORAZA y se queda quieto hasta que pasa el
/// peligro — la cochinilla se hace BOLA (conglobación), el caracol se METE en la concha, la babosa se CONTRAE y se balancea,
/// la lombriz se RETRAE a su galería. Mientras dura, su `armadura` sube (la bola/concha es coraza): `Predation.Defense` la
/// lee, así que un depredador que ya no puede con ella la DESCARTA solo (`Forager.SelectPrey`) — no hay regla «el pájaro
/// no come cochinillas»: emerge de los stats.
///
/// No es un flag de especie que se dispare siempre: <see cref="ThreatResponder.Decide"/> elige `Reaction.Hold` solo si
/// el ser NO puede huir (el otro es más ágil que él) y no gana peleando. Cuánto tarda en desenrollarse depende de SUS
/// stats: más estrés y menos temple → más rato; y del sexo (los machos de Armadillidium tardan más bajo olor de
/// depredador porque, al buscar hembras, se exponen más: Zamora-Camacho 2023, PeerJ 11:e16696, doi 10.7717/peerj.16696;
/// el tiempo depende además del estado previo: Matsuno &amp; Moriyama 2012, Acta Biol Hung 63:80,
/// doi 10.1556/ABiol.63.2012.Suppl.2.9). Auto-añadido en `Animal.Init` a las especies de <see cref="ModeOf"/>.
/// El cuerpo de cubos lee <see cref="Holding"/>/<see cref="Mode"/> para dibujar la bola o la concha.
/// </summary>
[RequireComponent(typeof(Animal))]
public class PassiveDefence : MonoBehaviour
{
    public enum Kind { None, Conglobate, Withdraw, Contract, Retract }

    public Kind Mode { get; private set; }
    /// <summary>Acorazado ahora (bola / dentro de la concha / contraída / retraída).</summary>
    public bool Holding { get; private set; }

    float _boost, _baseSeconds;
    Animal _a;

    /// <summary>Qué hace cada especie, cuánta coraza gana y cuánto dura de base (s de juego real). [A] = calibrar en vivo.</summary>
    public static (Kind kind, float armourBoost, float baseSeconds) ModeOf(string species) => species switch
    {
        "Woodlouse" => (Kind.Conglobate, 1.2f, 20f),   // bola de terguitos calcificados; latencia de decenas de s (PeerJ 11:e16696)
        "Snail"     => (Kind.Withdraw,   1.5f, 45f),   // dentro de la concha (y epifragma si se alarga) [A]
        "Slug"      => (Kind.Contract,   0.4f, 30f),   // semiesfera + balanceo «varios minutos» (Rowson et al. 2014) + moco
        "Earthworm" => (Kind.Retract,    0.6f, 25f),   // se encoge y se ancla con las quetas en la galería [A]
        _           => (Kind.None, 0f, 0f),
    };

    public static bool Has(string species) => ModeOf(species).kind != Kind.None;
    public static bool IsHolding(Component c) => c != null && c.TryGetComponent(out PassiveDefence d) && d.Holding;

    void Awake()
    {
        _a = GetComponent<Animal>();
        (Kind k, float boost, float secs) = ModeOf(_a != null ? _a.SpeciesName : null);
        Mode = k; _boost = boost; _baseSeconds = secs;
    }

    /// <summary>¿Prefiere acorazarse antes que huir de <paramref name="threat"/>? Solo si NO puede escapar: el otro es más
    /// ágil (o yo casi no me muevo). Un caracol siempre; una cochinilla ante una araña sí, ante otro caracol no.</summary>
    public bool Prefers(Anima self, Anima threat)
    {
        if (Mode == Kind.None || self == null) return false;
        float theirs = threat != null ? threat.agility : 1f;
        return self.agility < 0.5f || theirs >= self.agility * 0.9f;
    }

    /// <summary>Segundos que se queda acorazado: base × (1 + estrés) ÷ temple, y más en los machos (PeerJ 11:e16696).</summary>
    public float HoldSeconds(Anima self)
    {
        if (self == null) return _baseSeconds;
        float s = _baseSeconds * (1f + Mathf.Clamp01(self.stress)) / Mathf.Clamp(self.composure, 0.5f, 2f);
        if (self is Animal an && an.sex == Sex.male) s *= 1.3f;
        return s * Random.Range(0.7f, 1.3f);   // cada vez distinto (depende del estado previo: Matsuno & Moriyama 2012)
    }

    public void Begin()
    {
        if (Holding || _a == null) return;
        Holding = true;
        _a.armadura += _boost;
    }

    public void End()
    {
        if (!Holding || _a == null) return;
        Holding = false;
        _a.armadura = Mathf.Max(0f, _a.armadura - _boost);
    }

    void OnDisable() => End();
}
