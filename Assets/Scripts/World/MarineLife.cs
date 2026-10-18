using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// VIDA MARINA (focas, ballenas, orcas): nadan de verdad por la LAGUNA central, fuera del NavMesh (el NavMesh solo cubre el
/// hielo). Antes el NavMeshAgent los "pegaba" al borde de hielo más cercano: las focas se quedaban amontonadas en la
/// orilla y las ballenas, quietas. Su día:
///   · CON HAMBRE → nadan al banco de peces más cercano y comen al atravesarlo (el banco los alimenta, `Swarm`).
///   · EN CELO (hembra adulta) → buscan a un macho adulto de su especie (la reproducción necesita tenerlo cerca).
///   · CRÍAS → siguen a su madre (o a la hembra adulta más cercana) y maman a su lado.
///   · SACIADOS → merodean por la laguna a distintas profundidades.
///   · FOCAS → además SALEN A DESCANSAR al hielo (de noche, o un rato tras comer), cada una a un punto distinto de la orilla
///     (como en una colonia real, separadas entre sí), y vuelven al agua al tener hambre o al amanecer.
/// Ir al baño, dormir, celo y edad los siguen llevando sus componentes de siempre.
/// </summary>
public class MarineLife : MonoBehaviour
{
    static readonly Vector3 Lagoon = new Vector3(60f, 0f, 25f);   // = PolarLandscapeBuilder.Lagoon
    const float LagoonRadius = 26f, SwimY = -1f;

    public float speed = 2f;
    public bool hauls;   // sale a descansar al hielo (focas)
    Animal _a;
    EstrusState _estrus;
    Vector3 _target;
    float _nextPick, _restUntil, _restCooldown;
    bool _onIce;
    Vector3 _beach;
    float _angleSeed, _nextKill;
    static int _count;

    public bool OnIce => _onIce;

    void Start()
    {
        _a = GetComponent<Animal>();
        _estrus = GetComponent<EstrusState>();
        string sp = _a != null ? _a.SpeciesName : "";
        hauls = sp == "Seal";
        speed = sp == "Orca" ? 3.2f : sp == "Whale" ? 1.6f : 2.2f;
        if (TryGetComponent(out NavMeshAgent nav)) nav.enabled = false;   // nada: no anda por el NavMesh
        if (TryGetComponent(out Rigidbody rb)) { rb.isKinematic = true; rb.useGravity = false; }
        if (transform.position.y > -0.3f && !hauls) transform.position = new Vector3(transform.position.x, SwimY, transform.position.z);
        _restCooldown = Time.time + Random.Range(20f, 60f);
        _angleSeed = (_count++ * 67f) % 360f;   // cada foca, su trozo de orilla
    }

    void LateUpdate()   // después de la IA genérica (que intentaba moverlos por el NavMesh)
    {
        if (_a == null || _a.death) return;
        if (TryGetComponent(out NavMeshAgent nav) && nav.enabled) nav.enabled = false;
        bool night = TimeController.timeController != null && !TimeController.timeController.IsDay;

        // ── FOCAS: descanso en el hielo ──
        if (hauls)
        {
            if (_onIce)
            {
                bool wake = (_a.hungry >= 0.2f) || (!night && Time.time > _restUntil);
                if (!wake) { Settle(); return; }
                _onIce = false; _restCooldown = Time.time + Random.Range(40f, 90f);
                _target = Lagoon + Random.insideUnitSphere * 10f; _target.y = SwimY;
            }
            else if ((night || (_a.hungry < -0.2f && Time.time > _restCooldown)) && _a.lifeStage != LifeStage.child)
            {
                if (_beach == Vector3.zero) _beach = FindBeach();
                if (_beach != Vector3.zero && Move(_beach, speed * 0.6f, onIce: true) < 0.5f)
                {
                    _onIce = true; _restUntil = Time.time + Random.Range(60f, 140f);
                    Debug.Log($"[Marinos] «{name}» sale a descansar al hielo en ({_beach.x:0},{_beach.z:0}).");
                }
                return;
            }
        }

        // ── CRÍA: sigue a su madre ──
        if (_a.lifeStage == LifeStage.child)
        {
            Animal mom = NearestAdultFemale();
            if (mom != null)
            {
                Vector3 side = mom.transform.right * 1.4f;
                MarineLife ml = mom.GetComponent<MarineLife>();
                if (ml != null && ml.OnIce) { _onIce = true; Move(mom.transform.position + side, speed, onIce: true); return; }
                _onIce = false;
                Vector3 t = mom.transform.position + side; t.y = Mathf.Min(t.y, SwimY);
                Move(t, speed * 1.2f, false);
                return;
            }
        }

        // ── Hambre → banco de peces; celo → pareja; si no, merodeo por la laguna ──
        if (_a.hungry >= 0.5f && _a.SpeciesName == "Orca" && Time.time > _nextKill)   // ORCA: si tiene MUCHA hambre caza una foca en el agua (rara vez); si no, pesca
        {
            Animal prey = null; float pd = 25f;
            foreach (GameObject g in AnimalPopulations.Of("Seal"))
                if (g != null && g.TryGetComponent(out Animal s) && !s.death && s.transform.position.y < -0.3f) { float dd = Vector3.Distance(s.transform.position, transform.position); if (dd < pd) { pd = dd; prey = s; } }
            if (prey != null)
            {
                if (Move(prey.transform.position, speed * 1.3f, false) < 1.5f) { prey.Hurt(9999f); _a.hungry = -0.9f; _nextKill = Time.time + 300f; Debug.Log("[Marinos] Una orca caza una foca en la laguna."); }
                return;
            }
        }
        if (_a.hungry >= 0f)
        {
            Swarm fish = Swarm.Nearest(transform.position, SwarmKind.Fish) ?? Swarm.Nearest(transform.position);
            if (fish != null) { Vector3 f = fish.transform.position; f.y = Mathf.Min(f.y, SwimY); Move(f, speed * 1.2f, false); return; }
        }
        if (_estrus != null && _estrus.InEstrus && _a.sex == Sex.female && _a.lifeStage == LifeStage.adult)
        {
            Animal male = NearestAdult(Sex.male);
            if (male != null) { Move(male.transform.position, speed, false); return; }
        }
        if (Time.time > _nextPick || (transform.position - _target).sqrMagnitude < 1f)
        {
            Vector2 r = Random.insideUnitCircle * LagoonRadius;
            _target = new Vector3(Lagoon.x + r.x, SwimY - Random.Range(0f, 2.5f), Lagoon.z + r.y);
            _nextPick = Time.time + Random.Range(8f, 18f);
        }
        Move(_target, speed * 0.6f, false);
    }

    // Tumbada en el hielo (a ras de suelo), sin moverse: descansa y, de noche, duerme (SleepCycle).
    void Settle()
    {
        Vector3 p = transform.position;
        if (Physics.Raycast(p + Vector3.up * 2f, Vector3.down, out RaycastHit h, 6f, ~0, QueryTriggerInteraction.Ignore) && h.collider.GetComponentInParent<Animal>() == null)
            transform.position = new Vector3(p.x, h.point.y + 0.25f, p.z);
    }

    float Move(Vector3 to, float v, bool onIce)
    {
        Vector3 d = to - transform.position;
        float dist = d.magnitude;
        if (dist > 0.01f)
        {
            Vector3 step = d / dist * Mathf.Min(dist, v * Time.deltaTime);
            Vector3 np = transform.position + step;
            if (onIce && Physics.Raycast(np + Vector3.up * 2f, Vector3.down, out RaycastHit h, 6f, ~0, QueryTriggerInteraction.Ignore) && h.point.y > -0.3f && h.collider.GetComponentInParent<Animal>() == null)
                np.y = Mathf.MoveTowards(transform.position.y, h.point.y + 0.25f, 2f * Time.deltaTime);   // sube al hielo por la orilla
            else if (!onIce) np.y = Mathf.Min(np.y, -0.4f);   // nadando: siempre bajo la superficie
            transform.position = np;
            Vector3 flat = step; flat.y = 0f;
            if (flat.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 4f * Time.deltaTime);
        }
        return dist;
    }

    // Un punto de la orilla de la laguna (hielo a ras del agua) en una dirección propia de cada foca, separado de las demás.
    Vector3 FindBeach()
    {
        float baseAngle = _angleSeed + Random.Range(-15f, 15f);
        for (int tries = 0; tries < 12; tries++)
        {
            float a = (baseAngle + tries * 30f) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            for (float r = 10f; r < 60f; r += 1.5f)
            {
                Vector3 p = Lagoon + dir * r;
                if (!Physics.Raycast(p + Vector3.up * 5f, Vector3.down, out RaycastHit h, 10f, ~0, QueryTriggerInteraction.Ignore) || h.point.y < -0.3f) continue;
                Vector3 beach = h.point + dir * 2.5f + Vector3.up * 0.25f;   // un poco tierra adentro
                bool crowded = false;
                foreach (MarineLife o in FindObjectsByType<MarineLife>(FindObjectsSortMode.None))
                    if (o != this && o._beach != Vector3.zero && Vector3.Distance(o._beach, beach) < 3.5f) { crowded = true; break; }
                if (!crowded) return beach;
                break;
            }
        }
        return Vector3.zero;
    }

    Animal NearestAdultFemale() => NearestAdult(Sex.female);

    Animal NearestAdult(char sex)
    {
        Animal best = null; float bd = 40f;
        foreach (GameObject g in _a.Population)
        {
            if (g == null || g == gameObject || !g.TryGetComponent(out Animal o) || o.death) continue;
            if (o.lifeStage != LifeStage.adult || o.sex != sex) continue;
            float d = Vector3.Distance(o.transform.position, transform.position);
            if (d < bd) { bd = d; best = o; }
        }
        return best;
    }
}
