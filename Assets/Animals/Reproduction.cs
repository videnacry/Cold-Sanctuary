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
        if (FindMate() != null) Conceive();
    }

    bool CanConceive()
    {
        if (_animal.lifeStage != LifeStage.adult) return false;   // solo adultos
        if (_animal.sex != Sex.female) return false;              // gesta la hembra → una cría por pareja (no doble)
        if (Time.time < _cooldownUntil) return false;
        if (_estrus == null || !_estrus.InEstrus) return false;   // en celo
        if (_animal.hungry >= 0f) return false;                   // saciada (hungry>=0 = tiene hambre)
        if (_animal.Population.Count >= softPopulationCap) return false;
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
        int speed = TimeController.timeController != null ? Mathf.Max(1, TimeController.timeController.TimeSpeed) : 1;
        _birthTime = Time.time + gestationSeconds / speed;   // gestación acelerada por la velocidad de juego
    }

    void GiveBirth()
    {
        _pregnant = false;
        _cooldownUntil = Time.time + cooldownSeconds;
        SpawnOffspring();
    }

    /// <summary>Pare una cría: copia del progenitor puesta en etapa `child` (crece por `Animal.Init`→`LifeStage`).
    /// Devuelve el `Animal` de la cría. Público para el test del ciclo de vida.</summary>
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
            ba.sex = Random.value < 0.5f ? Sex.female : Sex.male;
        }
        return ba;
    }
}
