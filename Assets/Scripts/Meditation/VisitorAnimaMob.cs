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
        Debug.Log($"[Huerto] {by} da de comer {ToThe}: saciado, deja el cultivo en paz.");
        _resolveAt = Time.time + 1.5f;
        return true;
    }

    void Update()
    {
        if (_resolveAt >= 0f && Time.time >= _resolveAt) Dissolve();
    }
}
