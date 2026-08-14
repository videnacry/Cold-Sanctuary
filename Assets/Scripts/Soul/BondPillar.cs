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
    [Tooltip("Cuánto baja/sube MI stress por punto de relación con cada vecino (buena compañía calma; mala inquieta).")]
    public float stressEasePerPoint = 0.0004f;
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

            Anima other = c.GetComponent<Anima>();
            Bond existing = anima.GetBond(t);
            float rel;   // relación efectiva (para el efecto): el bond acumulado si ya se conocen; si no, karma/openness.
            if (existing == null)
            {
                // KARMA de especie (foca↔oso −, perro↔humano +). Si NO hay relación específica (especie nueva,
                // p.ej. lobo↔komodo), se resuelve por OPENNESS = disposición GENERAL del ser (si en total sus
                // relaciones son + o −). La NEGATIVA no siembra bond (el rechazo lo lleva el THREAT, por poder).
                float karma = other != null ? SpeciesKarma.RelationOf(anima, other.SpeciesName) : 0f;
                if (Mathf.Approximately(karma, 0f)) karma = SpeciesKarma.Openness(anima);
                if (karma > 0f) anima.GrowBond(t, BondType.Friend, karma);
                rel = karma;
            }
            else rel = existing.value;

            // Familiarización por circunstancia (cercanía): crece el bond con CUALQUIER ser (incl. el jugador-ITarget).
            anima.GrowBond(t, BondType.Friend, familiarityPerSecond * dt);

            // EFECTO: la buena compañía CALMA (baja mi stress); la mala INQUIETA (lo sube).
            anima.stress = Mathf.Clamp01(anima.stress - rel * stressEasePerPoint * dt);

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
