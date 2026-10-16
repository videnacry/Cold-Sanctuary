using UnityEngine;

/// <summary>
/// VISITANTE HAMBRIENTO del huerto (misión «Alimentar a los visitantes»): un animalito del hielo (liebre ártica,
/// lemming, perdiz nival…) hecho de bloques que va a por el cultivo sano más cercano. El jugador lo resuelve con
/// BONDAD, no echándolo: se acerca y le DA DE COMER (F, o el hechizo «alimentar»); saciado, se despide y se marcha
/// (eso cuenta). Si llega antes a la planta, le da un mordisco, se queda masticando y vuelve a por otra: sigue con
/// hambre hasta que alguien lo alimenta. Ya no se resuelve «solo» por pasar cerca (antes el avance subía sin hacer nada).
/// </summary>
public class HungryVisitorMob : MeditationMob, IInteractable
{
    public string speciesName = "liebre ártica";
    [Min(0.1f)] public float feedRadius = 2.5f;
    [Min(0.1f)] public float biteRadius = 0.9f;
    [Min(0f)] public float chewSeconds = 7f;   // masticar da tiempo al jugador a llegar antes del siguiente mordisco
    public float searchRadius = 40f;

    GardenCrop _target;
    float _chew;
    float _leaveTimer = -1f;
    Vector3 _leaveDir;
    float _bob;

    public bool Fed => _leaveTimer >= 0f;

    /// <summary>Nombre con artículo («la liebre ártica», «el lemming»).</summary>
    public string The => (speciesName == "lemming" || speciesName.StartsWith("ganso") ? "el " : "la ") + speciesName;
    /// <summary>Con «a» delante: «a la liebre ártica», «al lemming».</summary>
    public string ToThe => The.StartsWith("el ") ? "al " + speciesName : "a " + The;

    // ── IInteractable (F) ──────────────────────────────────────────────────────
    public string InteractLabel => $"Dar de comer {ToThe}";
    public bool CanInteract => !Fed && !IsDissolved;
    public void Interact() => Feed("tú");

    /// <summary>Alimentar: el visitante se sacia, deja el cultivo en paz y se marcha (resuelto).</summary>
    public bool Feed(string by)
    {
        if (Fed || IsDissolved) return false;
        _leaveTimer = 2.5f;
        _leaveDir = player != null ? (transform.position - player.position) : Random.insideUnitSphere;
        _leaveDir.y = 0f;
        if (_leaveDir.sqrMagnitude < 0.01f) _leaveDir = Vector3.forward;
        _leaveDir.Normalize();
        Debug.Log($"[Huerto] {by} da de comer {ToThe}: saciado, deja el cultivo en paz y se va.");
        return true;
    }

    void Update()
    {
        _bob += Time.deltaTime * 8f;
        if (Fed)
        {
            _leaveTimer -= Time.deltaTime;
            Step(_leaveDir, moveSpeed * Time.deltaTime);
            Face(_leaveDir);
            if (_leaveTimer <= 0f) Dissolve();
            return;
        }

        if (_chew > 0f) { _chew -= Time.deltaTime; return; }   // masticando el mordisco

        if (_target == null || _target.Eaten) _target = GardenCrop.Nearest(transform.position, searchRadius);
        if (_target == null) { Wander(); return; }

        Vector3 to = _target.transform.position - transform.position; to.y = 0f;
        if (to.magnitude <= biteRadius)
        {
            if (_target.Bite(char.ToUpper(The[0]) + The.Substring(1))) _chew = chewSeconds;
            _target = null;   // tras masticar, busca otra planta (sigue con hambre)
            return;
        }
        Step(to, moveSpeed * Time.deltaTime);
        Face(to);
        // Saltito al andar: se lee como un animal, no como una pieza que se desliza.
        Transform body = transform.childCount > 0 ? transform.GetChild(0) : null;
        if (body != null) body.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(_bob)) * 0.08f - Lift, 0f);
    }

    Vector3 _wanderTarget;
    void Wander()
    {
        if ((_wanderTarget - transform.position).sqrMagnitude < 0.3f || _wanderTarget == Vector3.zero)
        {
            Vector2 r = Random.insideUnitCircle * 4f;
            _wanderTarget = transform.position + new Vector3(r.x, 0f, r.y);
        }
        Vector3 d = _wanderTarget - transform.position; d.y = 0f;
        Step(d, moveSpeed * 0.4f * Time.deltaTime);
        Face(d);
    }

    void Face(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude > 1e-4f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
    }

    // ── Cuerpo de bloques ──────────────────────────────────────────────────────

    const float Lift = 0.35f;

    public static readonly string[] Species = { "liebre ártica", "lemming", "perdiz nival", "ganso de las nieves" };

    /// <summary>Crea el visitante (raíz con trigger para que F lo detecte; cuerpo de bloques como primer hijo).</summary>
    public static HungryVisitorMob Create(Vector3 pos, int speciesIndex)
    {
        string sp = Species[Mathf.Abs(speciesIndex) % Species.Length];
        GameObject root = new GameObject($"Visitante ({sp})");
        root.transform.position = pos + Vector3.up * Lift;   // la raíz flota un poco: el paso (SphereCast) no roza el suelo
        SphereCollider sc = root.AddComponent<SphereCollider>();
        sc.isTrigger = true; sc.radius = 0.6f; sc.center = Vector3.up * 0.05f;
        GameObject body = new GameObject("Cuerpo");
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = Vector3.down * Lift;
        Transform b = body.transform;

        Color fur = new Color(0.95f, 0.96f, 0.98f), dark = new Color(0.25f, 0.25f, 0.28f);
        switch (sp)
        {
            case "liebre ártica":
                Part(b, new Vector3(0f, 0.35f, 0f), new Vector3(0.4f, 0.35f, 0.6f), fur);
                Part(b, new Vector3(0f, 0.55f, 0.32f), new Vector3(0.28f, 0.26f, 0.28f), fur);
                Part(b, new Vector3(-0.07f, 0.82f, 0.3f), new Vector3(0.07f, 0.32f, 0.05f), fur);
                Part(b, new Vector3(0.07f, 0.82f, 0.3f), new Vector3(0.07f, 0.32f, 0.05f), fur);
                Part(b, new Vector3(-0.07f, 0.97f, 0.3f), new Vector3(0.07f, 0.06f, 0.05f), dark);   // puntas negras
                Part(b, new Vector3(0.07f, 0.97f, 0.3f), new Vector3(0.07f, 0.06f, 0.05f), dark);
                Legs(b, 0.14f, 0.2f, fur);
                break;
            case "lemming":
                Color brown = new Color(0.78f, 0.66f, 0.52f);
                Part(b, new Vector3(0f, 0.15f, 0f), new Vector3(0.28f, 0.2f, 0.38f), brown);
                Part(b, new Vector3(0f, 0.2f, 0.22f), new Vector3(0.2f, 0.17f, 0.16f), brown);
                Part(b, new Vector3(0f, 0.2f, 0.31f), new Vector3(0.06f, 0.05f, 0.03f), dark);
                break;
            case "perdiz nival":
                Part(b, new Vector3(0f, 0.3f, 0f), new Vector3(0.32f, 0.3f, 0.42f), fur);
                Part(b, new Vector3(0f, 0.52f, 0.2f), new Vector3(0.2f, 0.2f, 0.2f), fur);
                Part(b, new Vector3(0f, 0.5f, 0.33f), new Vector3(0.06f, 0.05f, 0.08f), dark);
                Part(b, new Vector3(-0.07f, 0.08f, 0f), new Vector3(0.05f, 0.16f, 0.05f), fur);
                Part(b, new Vector3(0.07f, 0.08f, 0f), new Vector3(0.05f, 0.16f, 0.05f), fur);
                break;
            default:   // ganso de las nieves
                Part(b, new Vector3(0f, 0.45f, 0f), new Vector3(0.38f, 0.32f, 0.6f), fur);
                Part(b, new Vector3(0f, 0.75f, 0.28f), new Vector3(0.1f, 0.4f, 0.1f), fur);
                Part(b, new Vector3(0f, 0.95f, 0.35f), new Vector3(0.14f, 0.13f, 0.2f), fur);
                Part(b, new Vector3(0f, 0.93f, 0.5f), new Vector3(0.08f, 0.06f, 0.12f), new Color(0.95f, 0.65f, 0.55f));
                Part(b, new Vector3(-0.08f, 0.15f, 0f), new Vector3(0.06f, 0.3f, 0.06f), new Color(0.95f, 0.65f, 0.55f));
                Part(b, new Vector3(0.08f, 0.15f, 0f), new Vector3(0.06f, 0.3f, 0.06f), new Color(0.95f, 0.65f, 0.55f));
                break;
        }
        HungryVisitorMob mob = root.AddComponent<HungryVisitorMob>();
        mob.speciesName = sp;
        mob.moveSpeed = sp == "lemming" ? 0.9f : sp == "liebre ártica" ? 1.3f : 1.1f;   // más lentos que el jugador: se les puede alcanzar
        return mob;
    }

    static void Legs(Transform b, float x, float z, Color c)
    {
        for (int i = 0; i < 4; i++)
            Part(b, new Vector3(i % 2 == 0 ? -x : x, 0.09f, i < 2 ? z : -z), new Vector3(0.08f, 0.18f, 0.1f), c);
    }

    static void Part(Transform parent, Vector3 lp, Vector3 size, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = lp;
        g.transform.localScale = size;
        Renderer r = g.GetComponent<Renderer>();
        r.material.color = c;
        if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", c);
    }
}

/// <summary>
/// Hechizo «alimentar» (cola del jugador): va al visitante hambriento más cercano —aunque se mueva— y le da de comer al
/// llegar. Sin ratón ni puntería: basta con lanzarlo cerca del huerto.
/// </summary>
public class FeedVisitorSpell : QueuedSpell
{
    public float searchRadius = 30f, reach = 2.2f, timeout = 25f;
    HungryVisitorMob _target;
    WalkToSpell _walk;
    float _t;

    public override string Label => _target != null ? $"alimentar {_target.ToThe}" : "alimentar";

    public override bool Tick(Anima self)
    {
        _t += Time.deltaTime;
        if (_target == null || _target.Fed || _target.IsDissolved) { _target = Nearest(self.transform.position); _walk = null; }
        if (_target == null) { Debug.Log("[Hechizo] No hay ningún visitante con hambre cerca."); return true; }
        if (_t > timeout) { Debug.Log($"[Hechizo] No alcanzas {_target.ToThe}."); return true; }
        Vector3 d = _target.transform.position - self.transform.position; d.y = 0f;
        if (d.magnitude <= reach) { _target.Feed($"«{self.name}»"); return true; }
        if (_walk == null) _walk = new WalkToSpell { target = _target.transform, targetName = _target.name, stopDistance = reach * 0.6f, maxSpeed = true, giveUpAfter = 6f };
        if (_walk.Tick(self)) _walk = null;   // llegó (o se movió): se recalcula al siguiente paso
        return false;
    }

    HungryVisitorMob Nearest(Vector3 p)
    {
        HungryVisitorMob best = null; float bestD = searchRadius;
        foreach (HungryVisitorMob v in Object.FindObjectsByType<HungryVisitorMob>(FindObjectsSortMode.None))
        {
            if (v.Fed) continue;
            float dd = Vector3.Distance(v.transform.position, p);
            if (dd < bestD) { bestD = dd; best = v; }
        }
        return best;
    }
}
