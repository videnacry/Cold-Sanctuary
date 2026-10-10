using UnityEngine;

/// <summary>
/// APARIENCIA POR STATS (docs/anyma-factory-and-control.md §1): los stats moldean el cuerpo. Mucha **fuerza/masa** →
/// más **músculo/grosor**; muchas **reservas de grasa** → más **grosor** aún. Se expresa como el **ANCHO** (ejes X/Z)
/// relativo a la **altura** (Y) — así NO pisa a `LifeStage`, que controla el tamaño/altura por etapa: este componente
/// solo ajusta la proporción ancho↔alto cada poco. Gentil y barato. Útil sobre todo en animas de la `AnymaFactory`
/// (stats random → tamaños/grosores variados) y tras un `TransformationSpell` que suba fuerza.
/// </summary>
[RequireComponent(typeof(Anima))]
public class StatPhysique : MonoBehaviour
{
    [Tooltip("Cuánto ensancha cada punto de fuerza por encima de la media (1).")]
    [Min(0f)] public float strengthWidth = 0.15f;
    [Tooltip("Cuánto ensancha la grasa (fatReserves 0..1 → hasta este extra).")]
    [Min(0f)] public float fatWidth = 0.35f;
    [Tooltip("Límites del factor de ancho (para no deformar en exceso).")]
    public float minWidth = 0.8f, maxWidth = 1.8f;

    Anima _a;
    float _next;
    bool _captured;
    float _ratioX = 1f, _ratioZ = 1f;   // proporción ancho/alto de la ESPECIE (capturada una vez)

    void Awake() { _a = GetComponent<Anima>(); }

    void Update()
    {
        if (_a == null || _a.death) return;
        if (Time.time < _next) return;
        _next = Time.time + 0.5f;

        Vector3 s = transform.localScale;
        if (s.y <= 0.00001f) return;
        if (!_captured)   // tras Init/LifeStage: guarda las proporciones naturales (un saltamontes es largo, no cuadrado)
        {
            _ratioX = s.x / s.y; _ratioZ = s.z / s.y; _captured = true;
        }

        float width = 1f + strengthWidth * Mathf.Max(0f, _a.strength - 1f) + fatWidth * Mathf.Clamp01(_a.fatReserves);
        width = Mathf.Clamp(width, minWidth, maxWidth);

        // La ALTURA (Y) la manda LifeStage; moldeamos el ANCHO (X/Z) preservando la proporción de la especie.
        transform.localScale = new Vector3(s.y * _ratioX * width, s.y, s.y * _ratioZ * width);
    }
}
