using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DIRECTOR de circunstancias del Nivel 1 (docs/microcosmos-level1.md §Beats, rebanada 3) — al estilo de
/// <c>MobWorldDirector</c>: **no mueve a nadie**. Secuencia los BEATS del alba **sembrando circunstancias**
/// (campos de pensamiento/humor, estímulos) y **leyendo el estado emergente** (<see cref="TribeCohesion"/>, la
/// cercanía de los que cuidan). Quien mueve son los <see cref="SocialImpulse"/> (rebanada 2); el director solo
/// decide CUÁNDO cambia la circunstancia y lo hace legible por consola.
///
/// Además ENGANCHA LA EMOCIÓN (rebanada 3): añade <see cref="EmotionExpression"/> a cada miembro del elenco (leen su
/// Anima+Mind y publican valencia/activación/tensión) → sus reacciones son legibles.
///
/// Beats (compactados; los nacimientos de crías/tribu de Héspero quedan como estímulos FUTUROS, se registran):
///  1. Sakshi OBSERVA y halla al pulgón Ambrosio.
///  2. CUIDADO: la tribu lo tiende; Sakshi se vuelca → la cohesión baja.
///  3. ABANDONO: la cohesión cae → resentimiento (campo Fuego/cortisol sobre la tribu).
///  4. DESERCIÓN: los ancianos frágiles se quedan; el grupo se va.
///  5. CLÍMAX/MUERTE: Ambrosio colapsa; duelo (campo Agua/cortisol sobre él); tableau final.
///
/// Balance-safe: si falta el elenco o la cohesión, cada espera cae por timeout y avanza igual.
/// </summary>
public class Level1Director : MonoBehaviour
{
    [Header("Ritmo (segundos de juego por beat / timeouts)")]
    public float observeSeconds = 8f;
    public float careTimeout    = 30f;
    public float abandonTimeout = 30f;
    public float desertSeconds  = 8f;

    [Tooltip("Cohesión por debajo de la cual se considera que el cuidado ya separó a Sakshi del grupo (beat 3).")]
    [Range(0f, 1f)] public float careCohesion = 0.5f;

    readonly Dictionary<string, GameObject> _cast = new Dictionary<string, GameObject>();
    TribeCohesion _cohesion;
    GameObject _ambrosio;

    void Start()
    {
        DiscoverCast();
        _cohesion = GetComponent<TribeCohesion>();
        if (_cohesion == null) _cohesion = gameObject.AddComponent<TribeCohesion>();
        HookEmotion();
        StartCoroutine(RunBeats());
    }

    void DiscoverCast()
    {
        foreach (SoulRecord s in FindObjectsOfType<SoulRecord>())
        {
            if (s == null || string.IsNullOrEmpty(s.soulName)) continue;
            _cast[s.soulName] = s.gameObject;
            if (s.soulName == "Ambrosio") _ambrosio = s.gameObject;
        }
    }

    // Emoción legible en todo el elenco (rebanada 3).
    void HookEmotion()
    {
        foreach (KeyValuePair<string, GameObject> kv in _cast)
        {
            GameObject go = kv.Value;
            if (go == null) continue;
            EmotionExpression e = go.GetComponent<EmotionExpression>();
            if (e == null) e = go.AddComponent<EmotionExpression>();
            if (e.mind == null) e.mind = go.GetComponent<Mind>();
        }
    }

    IEnumerator RunBeats()
    {
        yield return null;   // deja que todos corran su Start/Resolve (SoulComposition) primero

        // ── Beat 1: OBSERVAR ────────────────────────────────────────────────
        Log("Beat 1 · Sakshi OBSERVA y halla al pulgon deforme Ambrosio.");
        yield return Wait(observeSeconds);

        // ── Beat 2: CUIDADO (la tribu lo tiende; Sakshi se vuelca → cohesion baja) ──
        Log("Beat 2 · CUIDADO: la tribu tiende a Ambrosio; Sakshi se vuelca (su Follow pierde contra Tend).");
        yield return WaitUntilCohesionBelow(careCohesion, careTimeout);

        // ── Beat 3: ABANDONO (resentimiento) ────────────────────────────────
        Log("Beat 3 · ABANDONO: la cohesion cayo — la tribu se distancia. Resentimiento en el ambiente.");
        SpawnField("Campo_Resentimiento", TribeCentroidPos(), ElementalTone.Fuego, Humor.Cortisol, 0.06f, 8f);
        yield return WaitUntilAbandonedOr(abandonTimeout);

        // ── Beat 4: DESERCIÓN ───────────────────────────────────────────────
        Log("Beat 4 · DESERCION: 4 ancianos fragiles se quedan (WeakOne); el grupo se marcha. Momo se queda.");
        // (los nacimientos de Medea/Momo y la llegada de la tribu de Hespero = estimulos FUTUROS: spawn de nuevas animas.)
        yield return Wait(desertSeconds);

        // ── Beat 5: CLÍMAX / MUERTE ─────────────────────────────────────────
        Log("Beat 5 · CLIMAX: Ambrosio colapsa cerca de la cueva. Medea no logra levantarlo; Momo lo levanta; " +
            "Hespero se ablanda. MUERTE. Medea endurece; Momo toma el 'trono'. (Tableau final del alba.)");
        if (_ambrosio != null)
        {
            SpawnField("Campo_Duelo", _ambrosio.transform.position, ElementalTone.Agua, Humor.Cortisol, 0.08f, 5f);
            // Los que cuidaban ya están a su lado (Tend llegó): el duelo (grief) surge del campo + su emoción.
        }
        Log("Nivel 1 completo (director): los beats se sembraron; la conducta la resolvieron los impulsos + stats.");
    }

    // ── Utilidades de circunstancia ─────────────────────────────────────────

    void SpawnField(string name, Vector3 pos, ElementalTone tone, Humor humor, float humorPerSecond, float radius)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.position = pos;
        ThoughtField f = go.AddComponent<ThoughtField>();
        f.tone = tone; f.radius = radius; f.pull = 3f;
        f.nudgesHumor = true; f.humor = humor; f.humorPerSecond = humorPerSecond;
    }

    Vector3 TribeCentroidPos()
    {
        Vector3 sum = Vector3.zero; int n = 0;
        foreach (KeyValuePair<string, GameObject> kv in _cast)
            if (kv.Value != null) { sum += kv.Value.transform.position; n++; }
        return n > 0 ? sum / n : transform.position;
    }

    // ── Esperas ─────────────────────────────────────────────────────────────

    float Speed => TimeController.timeController != null ? Mathf.Max(1, TimeController.timeController.TimeSpeed) : 1;

    IEnumerator Wait(float gameSeconds)
    {
        float t = 0f;
        while (t < gameSeconds) { t += Time.deltaTime * Speed; yield return null; }
    }

    IEnumerator WaitUntilCohesionBelow(float threshold, float timeout)
    {
        float t = 0f;
        while (t < timeout)
        {
            if (_cohesion != null && _cohesion.MemberCount >= 2 && _cohesion.Cohesion < threshold) yield break;
            t += Time.deltaTime * Speed; yield return null;
        }
    }

    IEnumerator WaitUntilAbandonedOr(float timeout)
    {
        float t = 0f;
        while (t < timeout)
        {
            if (_cohesion != null && _cohesion.Abandoned) yield break;
            t += Time.deltaTime * Speed; yield return null;
        }
    }

    void Log(string msg) => Debug.Log("[Nivel1] " + msg);
}
