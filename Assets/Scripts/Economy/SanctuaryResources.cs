using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Libro mayor (ledger) de recursos del santuario ACTIVO (docs/world-topology-and-planes.md §4/§7).
/// Los <see cref="AreaProducer"/> aportan de forma pasiva/activa; construcción, mejoras y guerra
/// gastan. La escala de tiempo (Mesocosmos lento / Macrocosmos rápido) NO vive aquí: la dan los
/// productores al tickear en tiempo de juego (TimeController), así que subir la velocidad acelera
/// toda la economía sin tocar este ledger.
///
/// Singleton con auto-bootstrap (mismo patrón que MobWorldLoader/MeditationSession), para funcionar
/// aunque SampleSceneBuilder no lo cablee.
///
/// NOTA (multi-santuario / guerra): de momento hay UN santuario activo. La regla de §7 —"en guerra
/// solo ves los recursos de tu santuario"— se modela con <see cref="atWar"/> y <see cref="sanctuaryName"/>;
/// cuando existan varios santuarios, esto se extenderá a un ledger por santuario.
/// </summary>
public class SanctuaryResources : MonoBehaviour
{
    static SanctuaryResources _instance;
    public static bool HasInstance => _instance != null;
    public static SanctuaryResources Instance => _instance != null ? _instance : Bootstrap();

    [Tooltip("Nombre del santuario activo (para el HUD y la regla de visibilidad en guerra).")]
    public string sanctuaryName = "Santuario Terrestre";

    [Tooltip("En guerra, el jugador solo ve los recursos del santuario del que forma parte (docs §7).")]
    public bool atWar = false;

    readonly Dictionary<SanctuaryResource, float> _amounts = new Dictionary<SanctuaryResource, float>();

    /// <summary>Se dispara cuando cambia cualquier total (para HUD u otras vistas).</summary>
    public event Action OnChanged;

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    /// <summary>Total actual de un recurso (0 si nunca se tocó).</summary>
    public float Get(SanctuaryResource r) => _amounts.TryGetValue(r, out float v) ? v : 0f;

    /// <summary>Añade (o resta, si es negativo) un recurso. Nunca baja de 0.</summary>
    public void Add(SanctuaryResource r, float amount)
    {
        if (amount == 0f) return;
        _amounts[r] = Mathf.Max(0f, Get(r) + amount);
        OnChanged?.Invoke();
    }

    /// <summary>Gasta si hay suficiente; devuelve false (sin gastar) si no alcanza.</summary>
    public bool TrySpend(SanctuaryResource r, float amount)
    {
        if (amount <= 0f) return true;
        if (Get(r) < amount) return false;
        _amounts[r] = Get(r) - amount;
        OnChanged?.Invoke();
        return true;
    }

    /// <summary>Todos los tipos con su total actual (para el HUD).</summary>
    public IEnumerable<KeyValuePair<SanctuaryResource, float>> All()
    {
        foreach (SanctuaryResource r in Enum.GetValues(typeof(SanctuaryResource)))
            yield return new KeyValuePair<SanctuaryResource, float>(r, Get(r));
    }

    static SanctuaryResources Bootstrap()
    {
        var go = new GameObject("SanctuaryResources (auto)");
        _instance = go.AddComponent<SanctuaryResources>();
        return _instance;
    }
}
