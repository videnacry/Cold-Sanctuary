using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FITOPLANCTON: el "césped del mar" — el PRODUCTOR base de la cadena marina (docs/ice-sanctuary-ecology.md §2.2).
/// Vive de agua+luz: fotosintetiza y regenera su biomasa con el tiempo (más de día). Inmóvil (deriva imperceptible).
/// Es ITarget + IEdible: los enjambres (krill/pez) lo PASTAN por cercanía (Swarm.GrazePhytoplankton) → su biomasa baja
/// y el enjambre crece. Nunca "muere": si lo agotan, se recupera desde casi 0. Barato: un MonoBehaviour marcador con
/// un registro estático (como GrassPatch/Swarm), sin NavMesh/IA. Base de todo: fitoplancton→krill→pez→foca/pingüino/…
/// </summary>
public class Phytoplankton : MonoBehaviour, ITarget, IEdible
{
    public static readonly List<Phytoplankton> All = new List<Phytoplankton>();

    [Header("Biomasa / fotosíntesis (vive de agua+luz)")]
    [Tooltip("Biomasa actual (hace de 'lp': cuánto puede ser pastado).")]
    public float biomass = 40f;
    public float maxBiomass = 100f;
    [Tooltip("Biomasa/seg que regenera al fotosintetizar (escalada por la velocidad del juego).")]
    public float regenPerSecond = 0.6f;
    [Tooltip("Factor de fotosíntesis de noche (0 = no crece sin luz; 1 = igual que de día). Usa el reloj (Clock).")]
    [Range(0f, 1f)] public float nightFactor = 0.2f;
    [Tooltip("Masa (kg) por unidad de biomasa, para Mass/Grams.")]
    public float perBiomassMass = 0.1f;

    [Header("Cultivo en un edificio-árbol (las cortinas del fondo abierto; docs/mascaras-de-oxigeno.md §Fitoplancton)")]
    [Tooltip("Edificio-árbol cuyo jardinero lo cuida (vacío = fitoplancton del mar, sin cuidados).")]
    public string building = "";
    [Tooltip("Estado del cultivo (1 = óptimo): luz, nutrientes y cortinas limpias. Baja cada día; lo repone el jardinero.")]
    [Range(0f, 1f)] public float condition = 1f;
    public const float DecayPerDay = 0.3f;
    /// <summary>Minutos de juego en cuidar una cortina: limpiar el biofilm, medir y abonar (nitrato, fosfato, hierro).</summary>
    public const float CareMinutes = 2f;
    int _lastDay = -1;
    /// <summary>Cuidados del jardinero: el cultivo vuelve a estar en óptimas condiciones.</summary>
    public void Care(string who)
    {
        float b = condition; condition = 1f;
        Debug.Log($"[Fitoplancton] «{who}» cuida {name} de {building}: estado {b * 100f:0}% → 100% (biomasa {biomass:0}).");
    }

    void OnEnable()  => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Update()
    {
        // Fotosíntesis: crece con la LUZ (reloj del día vía Clock). De noche crece a nightFactor; sin Clock en escena el
        // reloj se queda de día (IsDay=true) → crece a tope. Se autoregenera desde casi 0 (nunca "muere"). En un edificio-árbol, según su estado.
        TimeController tc0 = TimeController.timeController;
        if (!string.IsNullOrEmpty(building) && tc0 != null) { int day = tc0.Day; if (_lastDay < 0) _lastDay = day; if (day != _lastDay) { condition = Mathf.Max(0f, condition - DecayPerDay * (day - _lastDay)); _lastDay = day; } }
        if (biomass >= maxBiomass) return;
        TimeController tc = TimeController.timeController;
        byte timeScale = tc != null ? tc.TimeSpeed : (byte)1;
        float light = (tc == null || tc.IsDay) ? 1f : nightFactor;
        float care = !string.IsNullOrEmpty(building) ? 0.25f + 0.75f * condition : 1f;   // un cultivo descuidado (cortinas sucias, sin nutrientes) crece menos
        biomass = Mathf.Min(maxBiomass, biomass + regenPerSecond * light * care * timeScale * Time.deltaTime);
    }

    public static Phytoplankton Nearest(Vector3 position)
    {
        Phytoplankton nearest = null;
        float bestDistSqr = float.MaxValue;
        foreach (Phytoplankton p in All)
        {
            if (p == null || p.Consumed) continue;
            float distSqr = (p.transform.position - position).sqrMagnitude;
            if (distSqr < bestDistSqr) { bestDistSqr = distSqr; nearest = p; }
        }
        return nearest;
    }

    // ── ITarget ────────────────────────────────────────────────────────────────
    public float Mass     => biomass * perBiomassMass;
    public float Speed    => 0f;
    public char  Faction  => 'p';            // productor (ni presa animal ni depredador)
    public bool  Dead     => biomass <= 0.5f;
    public bool  Consumed => biomass <= 0f;  // agotado momentáneamente; se recupera (no despawnea)
    public void  Hurt(float damage) => biomass = Mathf.Max(0f, biomass - damage);

    // ── IEdible ────────────────────────────────────────────────────────────────
    public OrganicMaterial Material => OrganicMaterial.Grass;   // "césped del mar": carb/minerals (ver Metabolism)
    public float Nutrition => 1f;
    public float Toughness => 0f;            // blando: sin resistencia al pastoreo
    public float Grams     => biomass * perBiomassMass;

    public float Consume(float biteSize)
    {
        float eaten = Mathf.Min(biteSize, biomass);
        biomass -= eaten;
        return eaten;                        // biomasa pastada (Swarm la convierte en crecimiento); se autoregenera
    }
}
