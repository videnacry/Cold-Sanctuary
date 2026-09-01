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
    [Tooltip("Masa (kg) por unidad de biomasa, para Mass/Grams.")]
    public float perBiomassMass = 0.1f;

    void OnEnable()  => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Update()
    {
        // Fotosíntesis: regenera a ritmo constante (asume luz). GANCHO: cuando exista un ciclo día/noche en el
        // proyecto (hoy `TimeController` solo lleva velocidad, sin reloj horario), gatear esto por la luz solar.
        if (biomass >= maxBiomass) return;
        byte timeScale = TimeController.timeController != null ? TimeController.timeController.TimeSpeed : (byte)1;
        biomass = Mathf.Min(maxBiomass, biomass + regenPerSecond * timeScale * Time.deltaTime);
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
