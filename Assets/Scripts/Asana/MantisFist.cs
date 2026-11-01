using System.Collections.Generic;

/// <summary>
/// LAS FORMAS DE BRUCE LEE EN SU CUERPO DE MANTIS (docs/microcosmos-civilizaciones.md §4). Las mismas claves que
/// <see cref="JeetKuneDo"/> (el cuerpo humano), adaptadas a los brazos raptoriales (<see cref="CubeInsect.Martial"/>):
/// - guardia: los dos brazos raptoriales plegados delante del pecho, en la línea central (la «oración» de la mantis = la
///   guardia de Wing Chun), con un leve balanceo;
/// - wingchun: man sau delante (un brazo medio extendido) y wu sau detrás (el otro plegado);
/// - cadena: golpes alternos muy rápidos (0,15 s cada uno; el golpe real de la mantis dura 60–290 ms, Rossoni y Niven 2020);
/// - muneco: los dos pares trabajan el muñeco (desvío con el segundo par, golpe con el raptorial);
/// - pasos: el balanceo de HOJA al viento (la mantis se mece para camuflarse y para medir la distancia por paralaje) es su
///   juego de pies;
/// - pulgada: carga plegado 1,6 s y extensión en 0,1 s (la fuerza corta, como el chasquido del camarón pistola);
/// - notas: brazos bajos, escribiendo; agua: balanceo lento con los brazos abiertos.
/// </summary>
public static class MantisFist
{
    /// <summary>Un fotograma: alcance de cada brazo raptorial (0 plegado – 1 extendido), altura, balanceo, segundo par y duración (s).</summary>
    public struct Frame
    {
        public float reachL, reachR, lift, sway, mid, seconds;
        public Frame(float l, float r, float lift, float sway, float mid, float s) { reachL = l; reachR = r; this.lift = lift; this.sway = sway; this.mid = mid; seconds = s; }
    }

    static readonly Dictionary<string, Frame[]> _forms = new Dictionary<string, Frame[]>
    {
        ["guardia"] = new[] { new Frame(0f, 0f, 0.6f, -0.1f, 0.2f, 0.8f), new Frame(0f, 0f, 0.62f, 0.1f, 0.2f, 0.8f) },
        ["wingchun"] = new[] { new Frame(0.45f, 0f, 0.65f, 0f, 0.3f, 0.7f), new Frame(0.5f, 0.05f, 0.68f, 0f, 0.3f, 0.7f) },
        ["cadena"] = new[] { new Frame(1f, 0f, 0.8f, 0f, 0.2f, 0.15f), new Frame(0f, 1f, 0.8f, 0f, 0.2f, 0.15f) },
        ["muneco"] = new[] { new Frame(0.2f, 0f, 0.6f, 0f, 1f, 0.3f), new Frame(1f, 0f, 0.75f, 0f, 0.6f, 0.2f), new Frame(0f, 1f, 0.55f, 0f, 0.2f, 0.25f), new Frame(0.5f, 0.5f, 0.4f, 0f, 0.8f, 0.3f) },
        ["pasos"] = new[] { new Frame(0f, 0f, 0.6f, -0.7f, 0.2f, 0.45f), new Frame(0f, 0f, 0.6f, 0.7f, 0.2f, 0.45f) },
        ["pulgada"] = new[] { new Frame(0.15f, 0f, 0.7f, 0f, 0.2f, 1.6f), new Frame(1f, 0f, 0.8f, 0f, 0.2f, 0.1f), new Frame(1f, 0f, 0.8f, 0f, 0.2f, 0.5f) },
        ["notas"] = new[] { new Frame(0.15f, 0.1f, 0.15f, 0f, 0.1f, 1.5f), new Frame(0.25f, 0.1f, 0.2f, 0f, 0.1f, 1.5f) },
        ["agua"] = new[] { new Frame(0.3f, 0.3f, 0.4f, -0.4f, 0.5f, 2f), new Frame(0.3f, 0.3f, 0.45f, 0.4f, 0.5f, 2f) },
    };

    public static IEnumerable<string> FormKeys => _forms.Keys;

    /// <summary>Los fotogramas de la forma (vacío si no existe).</summary>
    public static Frame[] Form(string key) => key != null && _forms.TryGetValue(key, out Frame[] f) ? f : new Frame[0];

    /// <summary>La postura a los <paramref name="t"/> s del comienzo del bucle (interpolada entre fotogramas).</summary>
    public static Frame At(string key, float t)
    {
        Frame[] f = Form(key);
        if (f.Length == 0) return new Frame(0f, 0f, 0.5f, 0f, 0.2f, 1f);
        float total = 0f; foreach (Frame x in f) total += x.seconds;
        t = total > 0f ? t % total : 0f;
        for (int i = 0; i < f.Length; i++)
        {
            if (t > f[i].seconds) { t -= f[i].seconds; continue; }
            Frame a = f[(i + f.Length - 1) % f.Length], b = f[i];
            float k = f[i].seconds > 0f ? UnityEngine.Mathf.SmoothStep(0f, 1f, t / f[i].seconds) : 1f;
            return new Frame(UnityEngine.Mathf.Lerp(a.reachL, b.reachL, k), UnityEngine.Mathf.Lerp(a.reachR, b.reachR, k), UnityEngine.Mathf.Lerp(a.lift, b.lift, k),
                             UnityEngine.Mathf.Lerp(a.sway, b.sway, k), UnityEngine.Mathf.Lerp(a.mid, b.mid, k), b.seconds);
        }
        return f[f.Length - 1];
    }
}
