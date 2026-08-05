using UnityEngine;

/// <summary>
/// LEGIBILIDAD (docs/emotion-model.md §6): lee la **orquesta emocional de OTROS** `Anima` cercanos (su
/// <see cref="EmotionExpression"/>: valencia/activación/emoción) y la traduce a **qué siente / qué quiere /
/// qué va a hacer** + una **aproximabilidad** (¿buen momento para el vínculo?). Es la base del hechizo
/// **lector-de-mentes** y de crear bonds con crías/adultos. El **alcance y el detalle** los gradúa la
/// **percepción** del observador (poca = solo "algo le pasa"; alta = lectura fina). HUD opcional (jugador/debug).
/// </summary>
public class EmotionReader : MonoBehaviour
{
    [Tooltip("Para leer su percepción (alcance + detalle). Si null, se auto-busca; sin Anima, percepción = 1.")]
    public Anima observer;
    public float baseRadius = 6f;
    public bool showHUD = true;

    public EmotionExpression Target { get; private set; }
    public string Reading { get; private set; }
    public float Approachability { get; private set; }   // 0..1

    void Start() { if (observer == null) observer = GetComponent<Anima>(); }

    void Update()
    {
        float perc = observer != null ? Mathf.Max(0.1f, observer.perception) : 1f;
        float r2 = (baseRadius * perc) * (baseRadius * perc);

        EmotionExpression best = null; float bestD = r2;
        foreach (EmotionExpression e in FindObjectsOfType<EmotionExpression>())
        {
            if (e == null || e.gameObject == gameObject) continue;
            float d = (e.transform.position - transform.position).sqrMagnitude;
            if (d <= bestD) { bestD = d; best = e; }
        }
        Target = best;
        if (best == null) { Reading = ""; Approachability = 0f; return; }

        // Aproximabilidad: calma + valencia positiva = receptivo; miedo/ira = dale espacio.
        Approachability = Mathf.Clamp01(best.Valence - best.Arousal * 0.3f);

        // Fidelidad por percepción: baja → lectura vaga; alta → siente + quiere + hará.
        float fidelity = Mathf.Clamp01((perc - 0.6f) / 0.8f);
        if (fidelity < 0.34f) { Reading = "algo le pasa…"; return; }

        string quiere, hara;
        if (best.Valence >= 0.5f) { quiere = "abierto al vínculo"; hara = best.Arousal > 0.55f ? "acercarse/jugar" : "quedarse contigo"; }
        else if (best.Arousal > 0.55f) { quiere = "en guardia — dale espacio"; hara = "huir o amenazar"; }
        else { quiere = "necesita consuelo"; hara = "encogerse"; }

        Reading = fidelity < 0.67f
            ? $"siente: {best.Emotion}"
            : $"siente: {best.Emotion} · quiere: {quiere} · hará: {hara}";
    }

    void OnGUI()
    {
        if (!showHUD || Target == null || string.IsNullOrEmpty(Reading)) return;
        if (Camera.main == null) return;
        Vector3 sp = Camera.main.WorldToScreenPoint(Target.transform.position + Vector3.up);
        if (sp.z <= 0f) return;
        float y = Screen.height - sp.y;
        GUIStyle st = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 13 };
        string bar = Approachability > 0.6f ? "● receptivo" : Approachability > 0.3f ? "◐" : "○ dale espacio";
        GUI.Label(new Rect(sp.x - 150f, y - 26f, 300f, 22f), Reading, st);
        GUI.Label(new Rect(sp.x - 150f, y - 8f, 300f, 18f), bar, st);
    }
}
