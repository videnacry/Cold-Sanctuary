using UnityEngine;

/// <summary>
/// REPRODUCCIÓN (ciclo de vida, pasos 2-3: cortejo → concepción → parto). Cierra el ciclo: los adultos en celo
/// (<see cref="EstrusState"/>) que encuentran pareja compatible conciben; tras la gestación **paren** una cría, que es
/// una copia del progenitor puesta en etapa `child` → crece por el ciclo existente (`Animal.Init`→`LifeStage`). Auto-
/// añadido en `Animal.Init`.
///
/// **GATEADO OFF por defecto** (`Enabled`): la reproducción CAMBIA la población, así que arranca desactivada por
/// seguridad; el compañero (o un test) la activa. Además, condiciones estrictas: solo la HEMBRA gesta (una cría por
/// pareja), en celo, saciada, con **cooldown** y un **tope blando** de población por especie → no explota. El parto
/// (<see cref="SpawnOffspring"/>) reusa `Instantiate` (un adulto es su propio template, como en `FamilyGenerator`).
/// </summary>
public class Reproduction : MonoBehaviour
{
    /// <summary>Gate GLOBAL: la reproducción solo ocurre si está en true. **ACTIVADA** (2026-08-28) → población
    /// auto-sostenible; contenida por celo+saciada+pareja+cooldown+softPopulationCap. Poner false para desactivarla.</summary>
    public static bool Enabled = true;

    [Tooltip("Duración de la gestación (segundos × velocidad de juego).")]
    [Min(0.1f)] public float gestationSeconds = 20f;
    [Tooltip("Radio para encontrar pareja.")]
    [Min(0.5f)] public float mateRadius = 6f;
    [Tooltip("Descanso tras parir antes de volver a concebir (segundos × velocidad).")]
    [Min(0f)] public float cooldownSeconds = 60f;
    [Tooltip("Tope BLANDO de población de la especie: no concebir por encima (evita explosión).")]
    [Min(1)] public int softPopulationCap = 200;

    Animal _animal;
    EstrusState _estrus;
    float _cooldownUntil;
    bool _pregnant;
    float _birthTime;

    void Awake() { _animal = GetComponent<Animal>(); _estrus = GetComponent<EstrusState>(); }

    void Update()
    {
        if (!Enabled || _animal == null || _animal.death) return;

        if (_pregnant) { if (Time.time >= _birthTime) GiveBirth(); return; }
        if (!CanConceive()) return;
        string sp = _animal.SpeciesName;
        // Pulgón: partenogénesis (hembras que paren hijas sin macho). Reina de hormiga/termita: guarda el esperma del
        // vuelo nupcial y pone sin volver a aparearse. El resto necesita pareja.
        if (Parthenogenetic(sp) || Eusocial(sp) || FindMate() != null) Conceive();
    }

    bool CanConceive()
    {
        if (_animal.lifeStage != LifeStage.adult) return false;   // solo adultos
        if (_animal.sex != Sex.female) return false;              // gesta la hembra → una cría por pareja (no doble)
        if (Time.time < _cooldownUntil) return false;
        if (_estrus == null || !_estrus.InEstrus) return false;   // en celo
        if (_animal.hungry >= 0f) return false;                   // saciada (hungry>=0 = tiene hambre)
        if (HiveBreeds(_animal.SpeciesName)) return false;          // la abeja nace del panal (Hive), no de aquí
        if (Eusocial(_animal.SpeciesName) && !IsColonyQueen()) return false;   // solo la reina pone; las obreras son estériles
        if (LivingCount() >= softPopulationCap) return false;   // cuenta solo los VIVOS (antes contaba también los cadáveres)
        int planCap = PopulationPlan.Cap(_animal.SpeciesName);
        if (planCap > 0 && LivingCount() >= planCap) return false;   // equilibrio con los humanos: no más de 1,5× lo que toca (PopulationPlan)
        return true;
    }

    // Cortejo RICO: entre las parejas compatibles cercanas, prefiere la de mayor VÍNCULO (bond) y más cerca — no la
    // primera que aparece. Sin bond, decide la proximidad. (Afinidad de especie ⟶ uniforme aquí, misma especie.)
    Animal FindMate()
    {
        Animal best = null; float bestScore = float.NegativeInfinity;
        foreach (Collider col in Physics.OverlapSphere(transform.position, mateRadius))
        {
            Animal other = col.GetComponentInParent<Animal>();
            if (other == null || other == _animal || other.death) continue;
            if (other.lifeStage != LifeStage.adult) continue;
            if (other.sex == _animal.sex) continue;                            // sexo opuesto
            if (other.SpeciesName != _animal.SpeciesName) continue;            // misma especie

            float score = 0f;
            ITarget t = other.GetComponent<ITarget>();
            if (t != null) { Bond b = _animal.GetBond(t); if (b != null) score += b.value; }   // vínculo = preferencia
            score -= Vector3.Distance(transform.position, other.transform.position) * 0.1f;      // más cerca, mejor
            if (score > bestScore) { bestScore = score; best = other; }
        }
        return best;
    }

    void Conceive()
    {
        _pregnant = true;
        // GESTACIÓN real de la especie (meses) escalada al calendario del santuario (Clock: día y año configurables).
        TimeController tc = TimeController.timeController;
        _birthTime = Time.time + (tc != null ? tc.SecondsForMonths(Calendar(_animal.SpeciesName).gestationMonths) : gestationSeconds);
    }

    void GiveBirth()
    {
        _pregnant = false;
        TimeController tcb = TimeController.timeController;
        _cooldownUntil = Time.time + Mathf.Max(cooldownSeconds, tcb != null ? tcb.SecondsForMonths(Calendar(_animal.SpeciesName).intervalMonths) : 600f);   // una cría por temporada (calendario real de la especie)
        SpawnOffspring();
    }

    /// <summary>Pare una cría: copia del progenitor puesta en etapa `child` (crece por `Animal.Init`→`LifeStage`).
    /// Devuelve el `Animal` de la cría. Público para el test del ciclo de vida.</summary>

    int LivingCount()
    {
        int n = 0;
        foreach (GameObject g in _animal.Population) if (g != null && g.TryGetComponent(out Animal a) && !a.death) n++;
        return n;
    }

    /// <summary>Se reproduce sin macho (partenogénesis cíclica del pulgón, doi 10.3897/compcytogen.18.136095).</summary>
    public static bool Parthenogenetic(string sp) => sp == "Aphid";
    /// <summary>Eusocial: en la colonia solo pone la reina (hormiga, termita). [A]</summary>
    public static bool Eusocial(string sp) => sp == "Ant" || sp == "Termite";
    /// <summary>La cría de la especie la lleva otro sistema (abeja de las nieves → `Hive`: huevo, larva, pupa en celdillas).</summary>
    public static bool HiveBreeds(string sp) => sp == "SnowBee";

    // La REINA de una colonia = la hembra adulta más antigua del nido (el menor id de instancia entre las hembras adultas
    // vivas de su especie que comparten hogar). Determinista y sin campo nuevo en escena.
    bool IsColonyQueen()
    {
        float r = Mathf.Max(5f, _animal.HomeRadius * 2f);
        foreach (GameObject g in _animal.Population)
        {
            if (g == null || g == gameObject || !g.TryGetComponent(out Animal o) || o.death) continue;
            if (o.sex != Sex.female || o.lifeStage != LifeStage.adult) continue;
            if (Vector3.Distance(o.HomeOrigin, _animal.HomeOrigin) > r) continue;
            if (string.CompareOrdinal(o.name + o.HomeOrigin, _animal.name + _animal.HomeOrigin) < 0) return false;   // desempate estable entre hembras (GetInstanceID está obsoleto en Unity 6)
        }
        return true;
    }

    // Tiempo mínimo entre partos de una misma madre (≈ una temporada de cría; se recalibra con el calendario del juego).

    /// <summary>Calendario reproductor REAL por especie (meses): gestación e intervalo mínimo entre partos de la misma madre.
    /// Foca común 10–11 m, cría anual; pingüino emperador ~2 m de incubación, anual; oso polar ~8 m (implantación
    /// diferida), cada 3 años; ballena jorobada ~11,5 m, cada 2–3 años; orca ~17 m, cada ~5 años; zorro ártico ~52 días;
    /// lobo ~63 días, ambos anuales. Verificado en PubMed (2026-10-21): perro 63 ± 2 d desde la ovulación
    /// (doi 10.1111/j.1439-0531.2011.01773.x) → malamute; reno 198–240 d, partos sincronizados anuales
    /// (doi 10.1016/j.theriogenology.2009.01.022) → ciervo; oso pardo intervalo entre partos ~2,4 años
    /// (doi 10.1371/journal.pone.0176251) → Bear. Conejo ~31 d (re-cría casi inmediata) y suricata ~70 d son
    /// valores de referencia general (PubMed no los da directos); la suricata dominante cría varias veces al año
    /// (doi 10.1111/j.1365-2656.2007.01318.x).</summary>
    public static (float gestationMonths, float intervalMonths) Calendar(string sp) => sp switch
    {
        "Seal" => (10.5f, 12f), "Penguin" => (2f, 12f), "Bear" => (8f, 29f), "PolarBear" => (8f, 36f),
        "Whale" => (11.5f, 30f), "Orca" => (17f, 60f), "Fox" => (1.7f, 12f), "Wolf" => (2.1f, 12f),
        "Malamute" => (2.1f, 7f), "Deer" => (7.3f, 12f), "Bunny" => (1f, 1.5f), "Meerkat" => (2.3f, 4f),
        // INSECTOS (docs/insect-biology-audit.md): "gestación" = del apareamiento a que sale la cría (incubación del
        // huevo; en el pulgón, la ninfa ya formada dentro de la madre), e "intervalo" = entre una cría y la siguiente de
        // la misma madre. Cada parto aquí es UNA cría, así que el intervalo es corto: ponen muchas, de una en una. El
        // tope de población (softPopulationCap / PopulationPlan) frena el resto. [A] = entomología general; calibrar
        // en vivo (cambian la dinámica del ecosistema).
        "Aphid" => (0.2f, 0.05f),       // vivíparo: pare ninfas vivas, varias al día [A]
        "Ant" => (0.4f, 0.1f),          // huevo ≈1–2 sem; la reina pone sin parar [A]
        "Termite" => (0.8f, 0.1f),      // huevo ≈3–4 sem; la reina pone sin parar [A]
        "Ladybug" => (0.15f, 0.2f),     // huevo ≈4–5 días; puestas repetidas durante semanas [A]
        "Cricket" => (0.5f, 0.15f),     // huevo ≈2 sem en el suelo [A]
        "Grasshopper" => (1f, 0.5f),    // ooteca en el suelo ≈1 mes (sin diapausa en la sabana) [A]
        "Mantis" => (1.2f, 0.4f),       // ooteca ≈3–6 sem; varias por hembra [A]
        "DungBeetle" => (0.3f, 1f),     // un huevo por bola-nido; pocas bolas: estratega K entre los insectos [A]
        "Antlion" => (1f, 0.3f),        // huevo en la arena ≈1 mes; el adulto vive pocas semanas [A]
        "Spider" => (0.8f, 1f),         // saco de huevos ≈2–4 sem; pocos sacos [A]
        _ => (2f, 12f),
    };
    public Animal SpawnOffspring()
    {
        Vector3 pos = transform.position + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
        GameObject baby = Instantiate(_animal.gameObject, pos, transform.rotation);
        baby.name = _animal.SpeciesName + "_cria";
        Animal ba = baby.GetComponent<Animal>();
        if (ba != null)
        {
            // Antes de que corra su Start/Init (Unity lo difiere): arranca como CRÍA y crece desde ahí.
            ba.lifeStage = LifeStage.child;
            ba.inheritedHome = _animal.HomeOrigin;   // su hogar es el de la madre, no el punto exacto donde nació
            ba.sex = Random.value < 0.5f ? Sex.female : Sex.male;
        }
        return ba;
    }
}
