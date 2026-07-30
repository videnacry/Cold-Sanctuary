using UnityEngine;

/// <summary>
/// Engancha la rutina de cuidado a los DRIVES REALES de una cría (`Animal`) — docs/cria-simulation.md §2/§3,
/// fauna-gameplay.md. Recibe las acciones del <see cref="VirtualPointer"/> (estación "Cria") y actúa sobre
/// el `Animal` real: **leer estado**, bajar **estrés** (calmar/arrullar), bajar **hambre** (alimentar) y
/// **ganar bond** (`Anima.GrowBond`, que ya factoriza el **trauma**). Regla clave (el bond SE GANA): si el
/// **estrés está alto, la cría rechaza el contacto** y el bond NO sube — hay que calmarla primero.
///
/// Va en la cría real (`Animal`, que genera `FamilyGenerator`). Sin `Animal` (p. ej. el placeholder del
/// sandbox), solo registra las acciones — el enganche a drives requiere una cría real.
/// </summary>
public class CriaCareTarget : VirtualTask
{
    public string station = "Cria";
    [Tooltip("La cría (Animal). Vacío → se busca un Animal en este objeto.")]
    public Animal cria;
    [Tooltip("Estrés por encima del cual la cría rechaza el contacto (no sube bond).")]
    [Range(0f, 1f)] public float stressBlocksBondAbove = 0.5f;

    ITarget _player;
    bool _warned;

    void Awake()
    {
        if (cria == null) cria = GetComponent<Animal>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.GetComponent<ITarget>();
    }

    public override void Submit(string stationId, string actionId)
    {
        if (stationId != station) return;

        if (cria == null)
        {
            if (!_warned) { Debug.Log("[Cría] (demo) CriaCareTarget sin Animal — cablear a una cría real para tocar drives."); _warned = true; }
            Debug.Log($"[Cría] (demo) acción «{actionId}».");
            return;
        }

        switch (actionId)
        {
            case "LeerEstado":
                Debug.Log($"[Cría] Estado de «{cria.name}»: hambre {cria.hungry:0.0}, estrés {cria.stress:0.00}, bond {BondValue():0}, trauma {cria.trauma:0}.");
                break;
            case "Calmar":
            case "Arrullar":
                cria.stress = Mathf.Max(0f, cria.stress - 0.2f);
                Debug.Log($"[Cría] «{cria.name}» se calma (estrés → {cria.stress:0.00}).");
                break;
            case "Alimentar":
                cria.hungry = Mathf.Max(0f, cria.hungry - 3f);
                Debug.Log($"[Cría] «{cria.name}» come (hambre → {cria.hungry:0.0}).");
                TryBond(2f, "alimento");
                break;
            case "Asear":
                TryBond(1.5f, "aseo");
                break;
            default:
                Debug.Log($"[Cría] acción «{actionId}».");
                break;
        }
    }

    void TryBond(float amount, string what)
    {
        if (_player == null) return;
        if (cria.stress > stressBlocksBondAbove)
        {
            Debug.Log($"[Cría] «{cria.name}» rechaza el {what}: estrés alto ({cria.stress:0.00}) — cálmala primero (el bond se gana).");
            return;
        }
        cria.GrowBond(_player, BondType.Imprint, amount);   // GrowBond ya factoriza el trauma
        Debug.Log($"[Cría] «{cria.name}» acepta el {what}: bond → {BondValue():0}.");
    }

    float BondValue()
    {
        if (_player == null) return 0f;
        Bond b = cria.GetBond(_player);
        return b != null ? b.value : 0f;
    }
}
