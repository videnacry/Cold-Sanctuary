using UnityEngine;

/// <summary>
/// PILAR SOCIAL / de BONDS por COMPOSICIÓN (docs/anima-architecture.md · soul-relations-reincarnation §2 —
/// fase 5). **No hay vía directa con el jugador**: cada `Anima` **familiariza con CUALQUIER otro ser cercano**
/// (un `ITarget` — animal, jugador vía `PlayerTarget`, etc.) **según las circunstancias** (aquí: la cercanía).
/// Usa el sistema de bonds **universal de `Anima`** (`GrowBond`/`GetBond`, con etapa-de-vida × trauma × aura),
/// no un vínculo player-céntrico. La "comodidad": un ser con mente cercano se reconforta según el bond mutuo.
/// Reemplaza el patrón player-first de `CompanionBase` → un compañero = `Anima + SoulComposition + BondPillar`.
/// *Falta (siguiente):* base kármica por especie (foca↔oso −, perro↔humano +) como punto de partida del bond.
/// </summary>
public class BondPillar : MonoBehaviour
{
    public Anima anima;

    [Tooltip("Radio en el que se percibe/familiariza con otros seres.")]
    public float proximityRadius = 4f;
    [Tooltip("Cuánto crece el bond por segundo de CERCANÍA (familiarización por circunstancia).")]
    public float familiarityPerSecond = 0.4f;
    [Tooltip("Ritmo de 'comodidad' que da a la MENTE de un vecino, escalado por el bond mutuo.")]
    public float comfortRate = 0.02f;
    public MindChannel comfortChannel = MindChannel.MentalFatigue;
    [Min(0.1f)] public float scanInterval = 0.5f;

    float _next;

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); }

    void Update()
    {
        if (anima == null || Time.time < _next) return;
        float dt = scanInterval;
        _next = Time.time + scanInterval;

        Collider[] cols = Physics.OverlapSphere(transform.position, proximityRadius);
        foreach (Collider c in cols)
        {
            if (c == null || c.gameObject == gameObject) continue;
            ITarget t = c.GetComponent<ITarget>();
            if (t == null || t.Dead) continue;

            // KARMA: la PRIMERA vez que se cruzan, el bond no arranca en 0 sino en la relación kármica de especie
            // (foca↔oso −, perro↔humano +). Especie nueva (lobo↔komodo) → 0. La karma NEGATIVA no siembra bond
            // (el rechazo lo lleva el sistema de THREAT, por poder); solo la positiva da confianza inicial.
            if (anima.GetBond(t) == null)
            {
                Anima other = c.GetComponent<Anima>();
                float karma = other != null ? SpeciesKarma.RelationOf(anima, other.SpeciesName) : 0f;
                if (karma > 0f) anima.GrowBond(t, BondType.Friend, karma);
            }

            // Familiarización por circunstancia (cercanía): crece el bond con CUALQUIER ser (incl. el jugador-ITarget).
            anima.GrowBond(t, BondType.Friend, familiarityPerSecond * dt);

            // Comodidad: si el vecino tiene mente, se reconforta según nuestro bond (sustituye al "restaurar al jugador").
            IMind mind = c.GetComponent<IMind>();
            if (mind != null)
            {
                Bond b = anima.GetBond(t);
                float val = b != null ? b.value : 0f;
                if (val > 0f) mind.RestoreMind(comfortRate * (val / 100f) * dt, comfortChannel);
            }
        }
    }
}
