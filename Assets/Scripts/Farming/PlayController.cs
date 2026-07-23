using UnityEngine;

/// <summary>
/// Input de "juego" del jugador para el farming no-violento (docs/world-topology-and-planes.md §4.1, MVP).
///
/// Acércate a una criatura (<see cref="PlayableCreature"/>) y pulsa la tecla de juego para darle un
/// 'toque': baja su tensión. Repite rápido hasta serenarla. Luego, cuando esté serena, dale de comer
/// con la interacción normal (F / clic — la gestiona InteractionController + IInteractable).
///
/// Se coloca en el Player (junto a PlayerController/InteractionController). Tecla propia (no mouse0)
/// para no chocar con atacar (PlayerCombat) ni con clic-interactuar.
/// </summary>
public class PlayController : MonoBehaviour
{
    [Tooltip("Tecla de 'toque de juego'. Por defecto V (distinta de atacar/interactuar).")]
    public KeyCode playKey = KeyCode.V;

    [Tooltip("Alcance del toque de juego, en metros.")]
    [Min(0.5f)] public float playRange = 3f;

    [Tooltip("Segundos mínimos entre toques (permite tapear rápido).")]
    [Min(0f)] public float playCooldown = 0.15f;

    float _lastPlay = -999f;

    void Update()
    {
        if (!Input.GetKeyDown(playKey)) return;
        if (Time.time - _lastPlay < playCooldown) return;

        PlayableCreature target = FindNearestPlayable();
        if (target == null) return;

        _lastPlay = Time.time;
        target.Play();
    }

    PlayableCreature FindNearestPlayable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, playRange);
        PlayableCreature best = null;
        float bestSqr = float.MaxValue;

        foreach (Collider col in hits)
        {
            PlayableCreature pc = col.GetComponentInParent<PlayableCreature>();
            if (pc == null || !pc.IsPlayable) continue;

            float d = (pc.transform.position - transform.position).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = pc; }
        }
        return best;
    }
}
