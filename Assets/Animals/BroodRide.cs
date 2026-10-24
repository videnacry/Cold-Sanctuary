using UnityEngine;

/// <summary>
/// CUIDADO DE LA CRÍA de los artrópodos que lo tienen (docs/era-species-behavior.md §4). Dos formas reales:
/// - **A lomos** (<see cref="Carries"/>): las crías del ESCORPIÓN suben al dorso de la madre al nacer y viven allí, sin
///   comer (del vitelo), hasta su primera muda; luego bajan y se dispersan (Polis &amp; Sissom 1990, en Polis (ed.) The
///   Biology of Scorpions, Stanford Univ. Press). Las de la ARAÑA LOBO (Lycosidae) igual, sobre el abdomen, ~1 semana
///   (Foelix 2011, Biology of Spiders, 3.ª ed.). Este componente las lleva: la cría sigue el lomo de la madre.
/// - **Guardar el nido** (<see cref="Guards"/>): la madre TIJERETA junta, limpia y defiende sus huevos y luego alimenta a
///   las ninfas regurgitando (Kölliker 2007, Behav Ecol Sociobiol 61:1489; Pasquier et al. 2025, Horm Behav 176:105839,
///   doi 10.1016/j.yhbeh.2025.105839); la hembra de SOLÍFUGO guarda los huevos en su madriguera (Punzo 1998, The Biology
///   of Camel-Spiders). Eso no vive aquí: <see cref="Reproduction"/> le da a la madre un VÍNCULO con cada cría y los deseos
///   «nurse» (alimentar) y «brood» (quedarse en el nido) de <see cref="DesireCatalog"/> hacen el resto según SUS stats
///   (afabilidad, hambre de las crías).
/// </summary>
[RequireComponent(typeof(Animal))]
public class BroodRide : MonoBehaviour
{
    /// <summary>Crías que viajan a lomos de la madre, y cuántos meses (≈ hasta la primera muda). [A] = calibrar.</summary>
    public static float RideMonths(string species) => species switch
    {
        "Scorpion" => 0.5f,   // 1–3 semanas según especie (Polis & Sissom 1990) [A]
        "Spider"   => 0.25f,  // araña lobo: ~1 semana sobre el abdomen (Foelix 2011) [A]
        _          => 0f,
    };
    public static bool Carries(string species) => RideMonths(species) > 0f;

    /// <summary>La madre se queda guardando el nido mientras tenga crías pequeñas (vínculo + etapa de cría).</summary>
    public static bool Guards(string species) => species == "Earwig" || species == "CamelSpider" || species == "Scorpion";

    /// <summary>La madre se encariña con lo que pare (base de «nurse»/«brood»). Las especies sin cuidado no.</summary>
    public static bool ParentalCare(string species) => Carries(species) || Guards(species);

    public Animal mother;
    Animal _a;
    float _until;
    Vector3 _offset;
    bool _riding, _mounted, _wasKinematic;

    void Start()
    {
        _a = GetComponent<Animal>();
        TimeController tc = TimeController.timeController;
        _until = Time.time + (tc != null ? tc.SecondsForMonths(RideMonths(_a.SpeciesName)) : 120f);
        _offset = new Vector3(Random.Range(-0.3f, 0.3f), 0f, Random.Range(-0.35f, 0.25f));
        _riding = mother != null;
    }

    void LateUpdate()
    {
        if (!_riding) return;
        if (mother == null || mother.death || _a == null || _a.death || Time.time >= _until || _a.lifeStage != LifeStage.child)
        {
            Dismount();
            return;
        }
        if (!_mounted)   // primer frame tras su Init (el NavMeshAgent y el Rigidbody ya están puestos)
        {
            _mounted = true;
            SetNav(false);
            if (_a.rig != null) { _wasKinematic = _a.rig.isKinematic; _a.rig.isKinematic = true; }
        }
        _a.busy = true;                                    // no va a ningún lado: vive en el lomo
        _a.hungry = Mathf.Min(_a.hungry, -0.2f);           // no come hasta la muda: vive del vitelo
        Transform m = mother.transform;
        transform.position = m.position + m.rotation * Vector3.Scale(_offset, m.lossyScale) + Vector3.up * BackHeight();
        transform.rotation = m.rotation * Quaternion.Euler(0f, _offset.x * 200f, 0f);
    }

    // Altura del dorso de la madre: lo alto de sus renderers sobre su pivote.
    float BackHeight()
    {
        float top = mother.transform.position.y;
        foreach (Renderer r in mother.GetComponentsInChildren<Renderer>()) if (r.enabled && !r.transform.IsChildOf(transform)) top = Mathf.Max(top, r.bounds.max.y);
        return top - mother.transform.position.y;
    }

    void Dismount()
    {
        _riding = false;
        if (_a != null)
        {
            _a.busy = false;
            Vector3 p = transform.position; p.y = mother != null ? mother.transform.position.y : p.y;
            transform.position = p;
            if (_mounted && _a.rig != null) _a.rig.isKinematic = _wasKinematic;
            if (_mounted) SetNav(true);
            if (mother != null && !mother.death) Debug.Log($"[Cría] «{name}» baja del lomo de «{mother.name}» tras la primera muda.");
        }
        enabled = false;
    }

    void SetNav(bool on)
    {
        if (_a == null || _a.nav == null) return;
        if (!on) { if (_a.nav.enabled && _a.nav.isOnNavMesh) _a.nav.ResetPath(); _a.nav.enabled = false; return; }
        _a.nav.enabled = true;
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas)) _a.nav.Warp(hit.position);
    }
}
