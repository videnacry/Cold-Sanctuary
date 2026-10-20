using System.Collections;
using UnityEngine;

/// <summary>Un visitante con hambre al que se puede dar de comer (animal real o figura de cubos).</summary>
public interface IHungryVisitor
{
    bool Fed { get; }
    bool Feed(string by);
    string ToThe { get; }
    Transform transform { get; }
}

/// <summary>
/// VISITANTE QUE ES UN ANIMA DE VERDAD (misión «Alimentar a los visitantes»): se engancha a un animal compuesto del
/// santuario (liebre ártica = Bunny, reno = Deer: Animal + SpeciesBody + Mind + Forager…). No se le ordena ir al huerto:
/// se le pone HAMBRE y su propia IA (volición → pastar → Forager) elige la planta que más le APETECE (DietPreference por
/// cultivo) y la muerde — «por conclusión propia». El jugador lo resuelve dándole de comer (F o «alimentar»): se sacia,
/// deja el cultivo y sigue viviendo en el santuario (no se destruye).
/// </summary>
public class VisitorAnimaMob : MeditationMob, IInteractable, IHungryVisitor
{
    [Tooltip("Hambre con la que llega (≥ 1 = va a buscar comida ya).")]
    public float startHunger = 1.6f;
    public string displayName = "liebre ártica";
    public bool feminine = true;

    Animal _animal;
    float _resolveAt = -1f;

    public bool Fed => _resolveAt >= 0f;
    public string ToThe => (feminine ? "a la " : "al ") + displayName;
    public string InteractLabel => $"Dar de comer {ToThe}";
    public bool CanInteract => !Fed && !IsDissolved;
    public void Interact() => Feed("tú");

    protected override void Awake()
    {
        base.Awake();
        keepOnDissolve = true;   // es un animal del santuario: al resolverse sigue vivo
        _animal = GetComponent<Animal>();
    }

    IEnumerator Start()
    {
        yield return null; yield return null;   // deja que Animal.Init fije sus valores de especie
        if (_animal != null) _animal.hungry = Mathf.Max(_animal.hungry, startHunger);
        Debug.Log($"[Huerto] Llega {displayName} con hambre (hambre {(_animal != null ? _animal.hungry : 0f):0.0}).");
    }

    public bool Feed(string by)
    {
        if (Fed || IsDissolved) return false;
        if (_animal != null)
        {
            _animal.hungry = -0.5f;   // saciado: su volición deja de buscar comida
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            ITarget pt = p != null ? p.GetComponent<ITarget>() : null;
            if (pt != null) _animal.GrowBond(pt, BondType.Friend, 10f);   // quien te da de comer te cae bien
        }
        Debug.Log($"[Huerto] {by} da de comer {ToThe}: se sienta a comer y luego se marcha.");
        _resolveAt = Time.time + 1000f;   // (lo resuelve la comida: se sienta, come y se va)
        StartCoroutine(EatThenLeave());
        return true;
    }

    // SE SIENTA A COMER (un montoncito de comida delante, quieto, agachado) y LUEGO SE MARCHA, lejos del huerto.
    IEnumerator EatThenLeave()
    {
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
        Vector3 fwd = transform.forward; fwd.y = 0f; if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        GameObject food = GameObject.CreatePrimitive(PrimitiveType.Cube);
        food.name = "Comida_visitante";
        Destroy(food.GetComponent<Collider>());
        food.transform.position = transform.position + fwd.normalized * 0.8f + Vector3.up * 0.08f;
        food.transform.localScale = new Vector3(0.45f, 0.16f, 0.45f);
        Renderer fr = food.GetComponent<Renderer>(); if (fr != null) fr.material.color = new Color(0.55f, 0.75f, 0.3f);
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, scale.y * 0.82f, scale.z);   // sentado / agachado comiendo
        float until = Time.time + 6f;
        while (Time.time < until)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
            if (_animal != null) _animal.hungry = -0.5f;
            food.transform.localScale = Vector3.Lerp(new Vector3(0.45f, 0.16f, 0.45f), new Vector3(0.1f, 0.04f, 0.1f), 1f - (until - Time.time) / 6f);
            yield return null;
        }
        Destroy(food);
        transform.localScale = scale;
        Debug.Log($"[Huerto] {displayName} termina de comer y se marcha.");
        // Se va: lejos del cultivo más cercano, unos segundos, y desaparece de la misión.
        GardenCrop c = GardenCrop.Nearest(transform.position, 40f);
        Vector3 away = c != null ? transform.position - c.transform.position : -fwd; away.y = 0f;
        Vector3 to = transform.position + away.normalized * 14f;
        if (agent != null && agent.enabled && agent.isOnNavMesh) { agent.isStopped = false; agent.SetDestination(to); }
        float leaveUntil = Time.time + 4f;
        while (Time.time < leaveUntil)
        {
            if (agent == null || !agent.enabled) transform.position = Vector3.MoveTowards(transform.position, to, 3f * Time.deltaTime);
            yield return null;
        }
        _resolveAt = Time.time;
    }

    void Update()
    {
        if (_resolveAt >= 0f && Time.time >= _resolveAt) Dissolve();
    }
}
