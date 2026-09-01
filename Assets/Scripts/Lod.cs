using UnityEngine;

/// <summary>
/// LOD de simulación (docs/checklist.md — análisis de escala): las ánimas LEJOS del foco (jugador/cámara) "piensan
/// más lento" — sus ticks CAROS (sobre todo `SenseThreats`, que es O(n²), y el mantenimiento de `Restore`) se
/// **espacian por distancia**, permitiendo MUCHAS más ánimas en escena sin caída de FPS. Cerca = ritmo pleno; lejos =
/// hasta `maxSlow×` más lento. **No altera la lógica**, solo su frecuencia (cuando el jugador se acerca, resumen a
/// ritmo pleno). Complementa la deuda de partición espacial anotada en `SenseThreats`.
/// </summary>
public static class Lod
{
    public static Transform focus;         // foco de detalle (jugador/cámara); se auto-busca (tag Player) si es null
    public static float nearDist = 60f;    // dentro de este radio: ritmo pleno
    public static float farDist  = 250f;   // más allá: ritmo mínimo
    public static float maxSlow  = 8f;     // factor máximo de ralentización (lejos)

    static float _nextFocusSearch;

    /// <summary>Multiplicador del intervalo de tick por distancia al foco: 1 cerca → `maxSlow` lejos (≥1 siempre).
    /// Multiplícalo por el intervalo base de un tick throttled para que los lejanos corran menos veces.</summary>
    public static float SlowFactor(Vector3 pos)
    {
        Transform f = Focus();
        if (f == null) return 1f;
        float d = Vector3.Distance(pos, f.position);
        float t = Mathf.Clamp01((d - nearDist) / Mathf.Max(1f, farDist - nearDist));
        return Mathf.Lerp(1f, maxSlow, t);
    }

    static Transform Focus()
    {
        if (focus != null) return focus;
        if (Time.time < _nextFocusSearch) return null;   // no buscar cada frame si aún no hay Player
        _nextFocusSearch = Time.time + 2f;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) focus = p.transform;
        return focus;
    }
}
