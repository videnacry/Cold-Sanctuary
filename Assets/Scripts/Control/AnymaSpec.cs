using UnityEngine;

/// <summary>
/// Rango de multiplicador de stats (docs/anyma-factory-and-control.md §2): sobre la MEDIA de la especie. La mayoría de
/// animas no superan ~2× la media; casos extraordinarios (strongman ×3–4) son raros → por defecto **0.8..2**.
/// </summary>
[System.Serializable]
public class StatRange
{
    [Min(0.1f)] public float min = 0.8f;
    [Min(0.1f)] public float max = 2f;
    public float Roll() => Random.Range(Mathf.Min(min, max), Mathf.Max(min, max));
}

/// <summary>
/// SPEC de creación de un anima (docs/anyma-factory-and-control.md §2) — objeto con propiedades **opcionales** que la
/// <see cref="AnymaFactory"/> usa para habilitar/deshabilitar caminos. Lo **no fijado = RANDOM dentro de márgenes**
/// (spec vacío → todo random). Convive con el sistema actual: por dentro instancia un `Animal` de la especie y
/// aplica encima el spec. Sexualidad/pensamientos/vínculos random quedan como extensión (campos preparados).
/// </summary>
[System.Serializable]
public class AnymaSpec
{
    [Tooltip("Especie (clave de catálogo: Bear/Wolf/Ant/…). Vacío = elige una al azar del pool de la factory.")]
    public string species = "";

    [Tooltip("Si randomizar los stats sobre la media de la especie (multiplicador en este rango).")]
    public bool randomizeStats = true;
    public StatRange randomStatsLimitations = new StatRange();   // 0.8..2 por defecto

    [Tooltip("Hambre inicial (0..1). <0 = random.")]
    public float hunger = -1f;
    [Tooltip("Sueño inicial (0..1). <0 = random.")]
    public float sleepiness = -1f;

    [Tooltip("Si fijar el hogar (HomeOrigin). Si no, el hogar = posición de spawn.")]
    public bool hasHome = false;
    public Vector3 home;

    [Tooltip("Rol familiar: 'pair'/'cub'/'friend'/'solitary'/'' (vacío = sin rol).")]
    public string familyRole = "";
}
