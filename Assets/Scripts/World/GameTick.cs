using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RITMO REPARTIDO (docs/rendimiento.md): hace un trabajo uno de cada N frames, repartido entre instancias por <c>seed</c>, sin perder
/// tiempo de juego: <see cref="Due"/> devuelve en <c>dt</c> todo el Time.deltaTime acumulado desde la última vez, así que lo que
/// integra por segundo (humores, pasto) da lo mismo. Va por frames, no por segundos: a pocos FPS un intervalo de 0,2 s se cumpliría
/// en cada frame y no ahorraría nada. Mientras corre el TestRunner toca en cada frame, como antes: hay pruebas que miran 0,4 s.
/// </summary>
public struct GameTick
{
    float _pending;

    /// <summary>¿Toca este frame? Acumula el Time.deltaTime de este frame; si toca, lo entrega en <paramref name="dt"/>.</summary>
    public bool Due(int everyFrames, int seed, out float dt)
    {
        return Step(ColdSanctuary.TestRunner.Running ? 1 : everyFrames, seed, Time.frameCount, Time.deltaTime, out dt);
    }

    /// <summary>El mismo paso con frame y dt explícitos (lo usa la prueba).</summary>
    public bool Step(int everyFrames, int seed, int frame, float frameDt, out float dt)
    {
        _pending += frameDt;
        dt = 0f;
        if (everyFrames > 1 && ((frame + (seed & 0xFFFF)) % everyFrames) != 0) return false;
        dt = _pending;
        _pending = 0f;
        return true;
    }
}

/// <summary>
/// LOG TRANQUILO (docs/rendimiento.md): en Play, las etiquetas charlatanas salen como mucho una vez cada <see cref="Seconds"/> s. Cada
/// Debug.Log cuesta milisegundos en el editor (consola y fichero) y dentro del script que lo llama cuenta como su tiempo propio.
/// Avisos y errores no se tocan, y con el TestRunner en marcha sale todo. Apagar: PlayerPrefs «ColdSanctuary.LogQuieto» = 0.
/// </summary>
public sealed class LogQuieto : ILogHandler
{
    public const string PrefKey = "ColdSanctuary.LogQuieto";
    public const float Seconds = 5f;
    static readonly HashSet<string> Quiet = new HashSet<string> { "[Mente]", "[Hábito]", "[Uniforme]", "[Mascaras]", "[Agua]", "[Ruta]", "[Alfombra]", "[Árbol]" };
    readonly ILogHandler _inner;
    readonly Dictionary<string, float> _next = new Dictionary<string, float>();

    LogQuieto(ILogHandler inner) { _inner = inner; }

    public static void Install()
    {
        if (PlayerPrefs.GetInt(PrefKey, 1) == 0 || Debug.unityLogger.logHandler is LogQuieto) return;
        var q = new LogQuieto(Debug.unityLogger.logHandler);
        Debug.unityLogger.logHandler = q;
        Application.quitting += () => { if (Debug.unityLogger.logHandler == q) Debug.unityLogger.logHandler = q._inner; };   // en el editor, lo de los builders sale entero
    }

    /// <summary>¿Saldría ahora un mensaje con esta etiqueta? (para no montar el texto si no).</summary>
    public bool Pass(string tag)
    {
        if (ColdSanctuary.TestRunner.Running || !Quiet.Contains(tag)) return true;
        float now = Time.unscaledTime;
        if (_next.TryGetValue(tag, out float at) && now < at) return false;
        _next[tag] = now + Seconds;
        return true;
    }

    public void LogFormat(LogType logType, Object context, string format, params object[] args)
    {
        if (logType == LogType.Log && args != null && args.Length == 1 && args[0] is string s && s.Length > 2 && s[0] == '[')
        {
            int end = s.IndexOf(']');
            if (end > 0 && !Pass(s.Substring(0, end + 1))) return;
        }
        _inner.LogFormat(logType, context, format, args);
    }

    public void LogException(System.Exception exception, Object context) => _inner.LogException(exception, context);
}
