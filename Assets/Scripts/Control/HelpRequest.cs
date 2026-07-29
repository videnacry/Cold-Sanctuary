using System.Collections;
using UnityEngine;

/// <summary>
/// Lado EMISOR de una petición entre personajes (docs/anima-architecture.md §11.7). Un ser pide algo a
/// otro; si el otro (su <see cref="HelpResponder"/>) contesta SÍ, se ejecuta una **posesión consentida =
/// alma compartida**: por un momento el receptor comparte el objetivo del emisor y hacen lo mismo (p. ej.
/// ir juntos a un sitio — el caso de las introducciones a nuevas áreas: el veterano "lleva" al novato).
///
/// MVP: la acción compartida es "ir juntos a un objetivo" (se inserta temporalmente un
/// <see cref="FollowBrain"/> de alta relevancia en el receptor y se retira al terminar). El paso siguiente
/// es compartir también los PENSAMIENTOS (una instancia de mente/madre compartida → "las mismas frases").
/// </summary>
public class HelpRequest : MonoBehaviour
{
    [Header("Demo (opcional): pedir al arrancar")]
    public bool autoAskOnStart = false;
    public AnimaController autoResponder;
    public Transform autoGoal;
    public float shareDuration = 6f;

    void Start()
    {
        if (autoAskOnStart && autoResponder != null && autoGoal != null)
            AskGoTogether(autoResponder, autoGoal, shareDuration);
    }

    /// <summary>Pide a <paramref name="responder"/> ir juntos a <paramref name="goal"/>. Si acepta, comparten alma un rato.</summary>
    public void AskGoTogether(AnimaController responder, Transform goal, float duration)
    {
        if (responder == null || goal == null) return;
        AnimaController from = GetComponent<AnimaController>();
        Debug.Log($"[Petición] «{name}» pide a «{responder.name}» ir juntos a «{goal.name}».");

        HelpResponder resp = responder.GetComponent<HelpResponder>();
        bool accepts = resp == null || resp.WouldAccept(from);   // sin HelpResponder → acepta por defecto
        if (!accepts) return;

        // Alma compartida (MVP): el receptor asume el objetivo con un cerebro de alta relevancia.
        FollowBrain shared = responder.gameObject.AddComponent<FollowBrain>();
        shared.target = goal;
        shared.relevance = 5f;   // por encima de su IA/follow habitual → conduce durante el momento compartido
        responder.RefreshBrains();
        Debug.Log($"[Petición] SÍ → alma compartida: «{name}» y «{responder.name}» van juntos a «{goal.name}» ({duration:0.0}s).");

        StartCoroutine(EndShare(responder, shared, duration));
    }

    IEnumerator EndShare(AnimaController responder, FollowBrain shared, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (shared != null) Destroy(shared);
        if (responder != null)
        {
            responder.RefreshBrains();
            Debug.Log($"[Petición] Fin del alma compartida; «{responder.name}» retoma su propia mente.");
        }
    }
}
