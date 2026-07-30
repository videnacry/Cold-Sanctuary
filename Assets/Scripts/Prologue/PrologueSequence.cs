using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// El guion del PRÓLOGO (docs/area-progression.md "Apertura"): una secuencia de **beats** que avanza cuando
/// cada uno se completa (<see cref="CompleteBeat"/>, llamado por el trigger real de ese beat — máquina,
/// misión de cuidado, portal…) o, en demo, sola por tiempo (<see cref="autoDemo"/>). No implementa las
/// escenas (eso es Unity); es el **hilo** que las encadena y las registra, reutilizando piezas existentes
/// (`VirtualizationMachine` = entrar, `YogaPortal` = salir, recetas de virtualización, misiones de cuidado).
/// </summary>
public class PrologueSequence : MonoBehaviour
{
    [TextArea]
    public string[] beats =
    {
        "Enfermería: exámenes médicos y vacuna del recién llegado (chequeo).",
        "Kushal curiosea → prueba la máquina de avatares (VirtualizationMachine) → entra al Microcosmos (pre-fuego).",
        "Era de La Recolectora: apoya y anima a los débiles; llévalos a la cueva-refugio (CarryToRefuge).",
        "Ve a la sala de meditación (YogaPortal) para volver al Mesocosmos.",
        "De vuelta y aprobado → primer trabajo (Cocina).",
    };

    [Tooltip("Demo: avanza solo por tiempo para ver el guion en consola.")]
    public bool autoDemo = false;
    [Min(0.5f)] public float autoInterval = 3f;

    public UnityEvent onFinished = new UnityEvent();

    int _i = -1;
    float _next;
    bool _done;

    void Start() { Advance(); }

    void Update()
    {
        if (autoDemo && !_done && Time.time >= _next)
        {
            _next = Time.time + autoInterval;
            CompleteBeat();
        }
    }

    /// <summary>Marca el beat actual como cumplido y pasa al siguiente. Lo llama el trigger real de cada beat.</summary>
    public void CompleteBeat()
    {
        if (_done) return;
        Advance();
    }

    void Advance()
    {
        _i++;
        if (_i < beats.Length)
        {
            Debug.Log($"[Prólogo] {_i + 1}/{beats.Length}: {beats[_i]}");
        }
        else
        {
            _done = true;
            Debug.Log("[Prólogo] Fin del prólogo → a su primer trabajo (la Cocina).");
            onFinished?.Invoke();
        }
    }
}
