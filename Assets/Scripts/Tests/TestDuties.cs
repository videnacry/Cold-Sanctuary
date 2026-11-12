using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// (2026-11-11) LAS CONDUCTAS PROPIAS DE UNA MISIÓN (la ronda del jardinero, el alimentador, la veterinaria, Linduarda) siguen
/// encendidas en el cuerpo cuando otra prueba toma a esa persona: la jornada (JornadaTest, grupo 50) dejaba a Gohageneis en plena
/// ronda de Servicios y la clase de yoga (grupo 51) lo llevaba a su esterilla A LA VEZ que FruitGardener lo sacaba del tronco: dejó
/// el viaje a medias, GroundGuard lo bajó 17 m («salto de 17,49 m») y acabó a 173 m de su esterilla. Las pruebas que toman
/// personas las pausan con <see cref="Pause"/> y las devuelven con <see cref="Resume"/>.
/// </summary>
public static class TestDuties
{
    static readonly System.Type[] Kinds = { typeof(FruitGardener), typeof(PredatorFeeder), typeof(VetCare), typeof(Linduarda) };

    /// <summary>Apaga las conductas propias encendidas de <paramref name="g"/>; devuelve las que apagó.</summary>
    public static List<Behaviour> Pause(GameObject g)
    {
        var l = new List<Behaviour>();
        if (g == null) return l;
        foreach (System.Type k in Kinds)
            if (g.GetComponent(k) is Behaviour b && b.enabled) { b.enabled = false; l.Add(b); }
        return l;
    }

    /// <summary>Las vuelve a encender.</summary>
    public static void Resume(List<Behaviour> l)
    {
        if (l == null) return;
        foreach (Behaviour b in l) if (b != null) b.enabled = true;
    }
}
