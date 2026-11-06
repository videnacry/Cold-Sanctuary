using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>LAS AVES FRÍAS QUE VUELAN con <see cref="SnowPetrelLife"/> (docs/fauna-fria-santuarios.md), y las del santuario aéreo: pocas, por memoria.</summary>
public static class ColdBirds
{
    public static readonly Dictionary<string, int> AirBudget = new Dictionary<string, int> { { "PolarSkua", 2 }, { "CapePetrel", 4 }, { "WanderingAlbatross", 2 }, { "ArcticTern", 2 } };
    public static bool Is(string species) => species == "SnowPetrel" || (species != null && AirBudget.ContainsKey(species));
}


/// <summary>
/// EL DÍA DE UN PETREL DE LAS NIEVES (Berón 2026-11-05; docs/reglas-animales.md, ficha; docs/vida-en-icebergs.md). Vuela: no usa NavMesh.
///   · HAMBRE → al banco de kril o de peces más cercano; baja a coger de la superficie, como los petreles.
///   · DESCANSA → en lo alto de un iceberg (se ven bandadas posadas en ellos). Se queda unas horas y va con él si deriva.
///   · CRÍA → nunca en un iceberg: en su nido de la roca de la isla. Pareja, huevo, relevo y pollo.
///   · POLLO → no se mueve del nido hasta que empluma; los padres van y vienen a darle de comer.
/// </summary>
public class SnowPetrelLife : MonoBehaviour
{
    public float speed = 12f;
    public const float RestHours = 4f, BroodSeconds = 25f;
    Animal _a;
    EstrusState _estrus;
    SnowPetrelNest _nest;
    Vector3 _perch;
    Iceberg _berg;
    float _restUntil, _nextRest, _broodUntil, _phase;
    bool _perched;
    Animal _chick;
    Transform _wingL, _wingR, _parent;
    public bool Perched => _perched;
    public Iceberg OnBerg => _perched ? _berg : null;
    public SnowPetrelNest Nest => _nest;

    void Start()
    {
        _a = GetComponent<Animal>(); _estrus = GetComponent<EstrusState>(); _parent = transform.parent;
        if (TryGetComponent(out NavMeshAgent nav)) nav.enabled = false;
        if (TryGetComponent(out Rigidbody rb)) { rb.isKinematic = true; rb.useGravity = false; }
        if (TryGetComponent(out CreatureRig rig)) { _wingL = rig.Get(BodyPart.ShoulderLeft); _wingR = rig.Get(BodyPart.ShoulderRight); }
        if (_a != null) _nest = SnowPetrelNest.Claim(_a);
        _phase = Random.value * 6f; _nextRest = Time.time + Random.Range(30f, 90f);
    }

    /// <summary>Que vaya ya a posarse (pruebas).</summary>
    public void RestNow(Iceberg on = null) { _nextRest = 0f; if (on != null) _berg = on; }
    /// <summary>¿Está criando (pollo, huevo que incubar, su pollo en el nido o en celo)? Entonces va al nido y no descansa en los icebergs.</summary>
    public bool Breeding => _a == null || _a.lifeStage == LifeStage.child || Egg.ToIncubate(_a) != null || NestChick() != null || (_a.lifeStage == LifeStage.adult && ((_estrus != null && _estrus.InEstrus) || (GetComponent<Reproduction>() is Reproduction r && r.Pregnant) || PartnerBreeding()));

    void LateUpdate()
    {
        if (_a == null || _a.death) return;
        if (TryGetComponent(out NavMeshAgent nav) && nav.enabled) nav.enabled = false;
        if (_a.lifeStage == LifeStage.child) { Sit(_nest != null ? _nest.transform.position : transform.position); return; }   // el pollo no sale del nido
        Egg egg = Egg.ToIncubate(_a);
        if (egg != null) { Unperch(); Fly(egg.transform.position + Vector3.up * 0.1f, true); return; }
        Animal chick = NestChick();
        if (chick != null && _chick != chick) { _chick = chick; _broodUntil = Time.time + BroodSeconds; }
        if (_chick != null && _chick.lifeStage != LifeStage.child) _chick = null;
        if (_chick != null && (Time.time < _broodUntil || (_a.hungry < 0f && _chick.hungry >= 0f)))   // con él los primeros días; luego va y viene
        {
            Unperch();
            if (Fly(_chick.transform.position + Vector3.right * 0.5f, true) < 1f) DesireCatalog.FeedCub(_a, _chick);
            return;
        }
        Reproduction rep = GetComponent<Reproduction>();
        bool courting = _a.lifeStage == LifeStage.adult && ((_estrus != null && _estrus.InEstrus) || (rep != null && rep.Pregnant) || PartnerBreeding());
        if (courting && _nest != null && _a.hungry < 0f) { Unperch(); Fly(_nest.transform.position, true); return; }   // (cría en la roca, nunca en un iceberg)
        if (_a.hungry >= 0f)
        {
            Unperch();
            Swarm krill = Swarm.Nearest(transform.position, SwarmKind.Krill), fish = Swarm.Nearest(transform.position, SwarmKind.Fish);
            Swarm s = krill == null ? fish : fish == null ? krill : (krill.transform.position - transform.position).sqrMagnitude <= (fish.transform.position - transform.position).sqrMagnitude ? krill : fish;
            if (s != null) { Vector3 at = s.transform.position; at.y = Mathf.Max(at.y, -1f); Fly(at, false); return; }
        }
        if (_perched)
        {
            Sit(_berg != null ? _berg.PerchSpot(_phase) : _perch);
            if (Time.time >= _restUntil) { Unperch(); _nextRest = Time.time + Random.Range(60f, 180f); }
            return;
        }
        if (Time.time >= _nextRest)
        {
            if (_berg == null) _berg = NearestBerg();   // (uno por descanso: elegido en cada fotograma, con dos icebergs cambiaba de rumbo y no llegaba nunca)
            _perch = _berg != null ? _berg.PerchSpot(_phase) : _nest != null ? _nest.transform.position : transform.position;
            if (Fly(_perch, true) < 1f)
            {
                _perched = true;
                _restUntil = Time.time + RestHours * (TimeController.timeController != null ? TimeController.timeController.SecondsPerGameDay / 24f : 150f);
                if (_berg != null) { transform.SetParent(_berg.transform, true); Debug.Log($"[Petrel] «{name}» se posa en {_berg.name}."); }
            }
            return;
        }
        // Planea en corro alrededor de su banco.
        Vector3 c = Swarm.Nearest(transform.position, SwarmKind.Krill) is Swarm k && k != null ? k.transform.position : transform.position;
        float t = Time.time * 0.25f + _phase;
        Fly(new Vector3(c.x + Mathf.Cos(t) * 25f, 12f + 4f * Mathf.Sin(t * 1.7f), c.z + Mathf.Sin(t) * 25f), false);
    }

    void Unperch()
    {
        if (!_perched) return;
        _perched = false;
        _berg = null;
        transform.SetParent(_parent, true);
    }

    static Iceberg NearestBerg()
    {
        Iceberg b = null;
        foreach (Iceberg i in Iceberg.All) if (i != null && (b == null || Random.value < 0.5f)) b = i;   // uno al azar: todos están a tiro de ala
        return b;
    }

    Animal NestChick()
    {
        if (_nest == null) return null;
        foreach (GameObject go in Animal.wholePopulation)
            if (go != null && go.TryGetComponent(out Animal a) && !a.death && a.lifeStage == LifeStage.child && a.SpeciesName == _a.SpeciesName && (a.transform.position - _nest.transform.position).sqrMagnitude < 9f) return a;
        return null;
    }

    bool PartnerBreeding()
    {
        if (_nest == null) return false;
        foreach (Animal p in _nest.pair)
            if (p != null && p != _a && !p.death && p.TryGetComponent(out EstrusState e) && e.InEstrus) return true;
        return false;
    }

    void Sit(Vector3 at)
    {
        transform.position = Vector3.MoveTowards(transform.position, at, 2f * Time.deltaTime);
        Wings(0f);
    }

    float Fly(Vector3 to, bool land)
    {
        Vector3 cur = transform.position, d = to - cur;
        float dist = d.magnitude;
        if (dist < 0.05f) { Wings(0f); return dist; }
        float v = land && dist < 8f ? Mathf.Max(2f, speed * dist / 8f) : speed;
        Vector3 goal = to;
        if (!land || dist > 20f) goal.y = Mathf.Max(to.y, dist > 30f ? 24f : to.y);   // en crucero, por encima de los edificios bajos y del hielo
        transform.position = Vector3.MoveTowards(cur, goal, v * Time.deltaTime);
        Vector3 flat = new Vector3(d.x, 0f, d.z);
        if (flat.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 5f * Time.deltaTime);
        Wings(1f);
        return dist;
    }

    void Wings(float open)
    {
        float flap = open * 28f * Mathf.Sin(Time.time * 7f + _phase);
        if (_wingL != null) _wingL.localRotation = Quaternion.Euler(0f, 0f, flap);
        if (_wingR != null) _wingR.localRotation = Quaternion.Euler(0f, 0f, -flap);
    }
}
