using System;
using UnityEngine;

/// <summary>Compuesto bioquímico de un ser (docs/anima-architecture.md §10.1).</summary>
public enum Humor { Adrenalina, Serotonina, Cortisol, Glucosa, Calcio }

/// <summary>
/// Bioquímica ("humores") de un ánima: estado DINÁMICO que modula aptitudes y mente (docs §10.1).
/// Es el combustible de energía/ánimo; las acciones lo producen/consumen y comida/descanso lo reparan.
/// Distinto de la química-hechizo (tabla periódica). Valores 0..1. Modelo híbrido: las aptitudes
/// (estructura) se mantienen; los humores las modulan.
/// </summary>
[Serializable]
public class Humores
{
    [Range(0f, 1f)] public float adrenalina = 0.4f;
    [Range(0f, 1f)] public float serotonina = 0.5f;
    [Range(0f, 1f)] public float cortisol   = 0.3f;
    [Range(0f, 1f)] public float glucosa    = 0.6f;
    [Range(0f, 1f)] public float calcio     = 0.7f;

    /// <summary>Energía/combustible disponible ≈ glucosa (+ empuje de adrenalina). 0..1.</summary>
    public float Energia => Mathf.Clamp01(glucosa * 0.7f + adrenalina * 0.3f);

    /// <summary>Positividad del ánimo ≈ serotonina − cortisol. −1..1.</summary>
    public float Positividad => Mathf.Clamp(serotonina - cortisol, -1f, 1f);

    public void Produce(Humor h, float amt) => Set(h, Get(h) + amt);
    public void Consume(Humor h, float amt) => Set(h, Get(h) - amt);

    /// <summary>Deriva lento hacia una línea base (comer/descansar lo acelerarían; aquí auto, para demo).</summary>
    public void Regen(float dt)
    {
        adrenalina = Mathf.MoveTowards(adrenalina, 0.4f, 0.05f * dt);
        serotonina = Mathf.MoveTowards(serotonina, 0.5f, 0.03f * dt);
        cortisol   = Mathf.MoveTowards(cortisol,   0.3f, 0.03f * dt);
        glucosa    = Mathf.MoveTowards(glucosa,    0.6f, 0.02f * dt);
        calcio     = Mathf.MoveTowards(calcio,     0.7f, 0.01f * dt);
    }

    public float Get(Humor h)
    {
        switch (h)
        {
            case Humor.Adrenalina: return adrenalina;
            case Humor.Serotonina: return serotonina;
            case Humor.Cortisol:   return cortisol;
            case Humor.Glucosa:    return glucosa;
            case Humor.Calcio:     return calcio;
            default:               return 0f;
        }
    }

    void Set(Humor h, float v)
    {
        v = Mathf.Clamp01(v);
        switch (h)
        {
            case Humor.Adrenalina: adrenalina = v; break;
            case Humor.Serotonina: serotonina = v; break;
            case Humor.Cortisol:   cortisol   = v; break;
            case Humor.Glucosa:    glucosa    = v; break;
            case Humor.Calcio:     calcio     = v; break;
        }
    }
}
