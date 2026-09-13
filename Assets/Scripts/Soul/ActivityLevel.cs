using UnityEngine;

/// <summary>Zona de la carga de actividad (docs/consciousness-mechanics.md §activity).</summary>
public enum ActivityZone { Desentrenado, Bendecido, Sobrecarga }

/// <summary>
/// NIVEL DE ACTIVIDAD (docs/consciousness-mechanics.md §activity) — el modelo científico REAL del deporte:
///   • **Carga aguda (ATL)** = media de los últimos ~**7 días** (fatiga reciente).
///   • **Carga crónica (CTL)** = media de los últimos ~**28 días** (**forma/fitness** acumulada).
///   • **ACWR = aguda / crónica**: la "ventana de 7 días" que intuías. Zona **BENDECIDA (óptima) = 0.8–1.3** (fitness y
///     fatiga en equilibrio → rendimiento/creatividad máximos, cf. Yerkes-Dodson: arousal óptimo, no máximo). &lt;0.8 =
///     **desentrenado**; &gt;1.3 = **sobrecarga** (lesión/burnout).
///   • **Frescura (TSB) = crónica − aguda**; **Fitness = crónica** → es lo que crece despacio con actividad SOSTENIDA y
///     alimenta el crecimiento acumulado de aptitudes (las 70+). Se actualiza por día de juego (EWMA con τ=7 y τ=28).
/// Cada actividad (correr, yoga, cazar, trabajar) llama <see cref="AddLoad"/>. Opt-in y graceful.
/// </summary>
public class ActivityLevel : MonoBehaviour
{
    [Min(1f)] public float acuteTau = 7f;      // días — fatiga
    [Min(1f)] public float chronicTau = 28f;   // días — forma
    [Min(1f)] public float minutesPerGameDay = 1440f;

    float _acute, _chronic, _dayLoad, _minAccum;

    /// <summary>Carga aguda (fatiga, ~7 días).</summary>
    public float Acute => _acute;
    /// <summary>Carga crónica = FORMA/fitness (~28 días); base del crecimiento acumulado.</summary>
    public float Chronic => _chronic;
    /// <summary>Ratio agudo:crónico. Zona bendecida 0.8–1.3.</summary>
    public float Acwr => _chronic > 0.01f ? _acute / _chronic : (_acute > 0.01f ? 2f : 1f);
    /// <summary>Frescura (TSB) = crónica − aguda.</summary>
    public float Freshness => _chronic - _acute;

    public ActivityZone Zone
    {
        get { float r = Acwr; return r < 0.8f ? ActivityZone.Desentrenado : (r > 1.3f ? ActivityZone.Sobrecarga : ActivityZone.Bendecido); }
    }

    /// <summary>Registra carga de una actividad (correr/yoga/cazar/trabajar) en el día en curso.</summary>
    public void AddLoad(float amount) { if (amount > 0f) _dayLoad += amount; }

    /// <summary>Cierra un día: actualiza las medias exponenciales (EWMA) de carga aguda y crónica. Público para el test.</summary>
    public void RollDay()
    {
        float kA = 1f - Mathf.Exp(-1f / acuteTau);
        float kC = 1f - Mathf.Exp(-1f / chronicTau);
        _acute   += (_dayLoad - _acute)   * kA;
        _chronic += (_dayLoad - _chronic) * kC;
        _dayLoad = 0f;
    }

    void Update()
    {
        int speed = TimeController.timeController != null ? Mathf.Max(1, TimeController.timeController.TimeSpeed) : 1;
        _minAccum += Time.deltaTime * speed;             // ~ minutos de juego (segundos reales × velocidad)
        while (_minAccum >= minutesPerGameDay) { _minAccum -= minutesPerGameDay; RollDay(); }
    }
}
