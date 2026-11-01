using UnityEngine;

/// <summary>
/// VELOCIDAD DEL JUEGO elegida en INICIO (Berón 2026-11-01: «¿sería posible agregar un control de tiempo en home? para poder
/// ver x2 o x4»). Es <c>Time.timeScale</c>: todo lo que corre con el tiempo escalado (el reloj del día —Clock avanza con
/// <c>Time.deltaTime</c>—, las etapas de vida, el celo, la gestación, el hambre, la física y los NavMeshAgent) va N veces
/// más deprisa, igual que en las pruebas que ya corrían a ×3–×6 (JornadaTest, TreeOfFieldsTest…).
///
/// Qué decide: la elección se GUARDA (PlayerPrefs «Inicio.Velocidad») y se aplica a CADA escena que se carga desde Inicio
/// (también «Reiniciar esta escena»). La pantalla de Inicio en sí corre a ×1 (es un menú) y un Play directo desde el editor
/// (SampleScene, las pruebas) también, para no cambiar las pruebas ni el trabajo en el editor. En el juego se cambia desde
/// el panel de pausa (F10) y, en las escenas de ciclo de vida, desde su panel.
/// ×8 es la última: más allá, con la física a 50 Hz, cada fotograma tendría que simular demasiados pasos.
/// </summary>
public static class GameSpeed
{
    public static readonly float[] Options = { 1f, 2f, 4f, 8f };
    const string Key = "Inicio.Velocidad";

    /// <summary>La velocidad elegida (guardada entre sesiones).</summary>
    public static float Chosen
    {
        get
        {
            float v = PlayerPrefs.GetFloat(Key, 1f);
            foreach (float o in Options) if (Mathf.Approximately(o, v)) return o;
            return 1f;
        }
        set { PlayerPrefs.SetFloat(Key, value); PlayerPrefs.Save(); }
    }

    /// <summary>Guarda <paramref name="speed"/> y, si el juego no está en pausa (timeScale 0), la aplica ya.</summary>
    public static void Set(float speed)
    {
        Chosen = speed;
        if (Time.timeScale > 0f) Apply();
    }

    /// <summary>Aplica la velocidad elegida al tiempo del juego.</summary>
    public static void Apply()
    {
        // Solo el timeScale. El tope por fotograma (Time.maximumDeltaTime, 1/3 s) se deja: si el PC no llega (a ×8, un
        // fotograma de más de ~40 ms), el juego va algo más lento que lo elegido en vez de encadenar decenas de pasos de física.
        Time.timeScale = Chosen;
    }

    /// <summary>Texto corto para botones y avisos: «×4».</summary>
    public static string Label(float v) => "×" + v.ToString("0");

    /// <summary>Fila de botones ×1 ×2 ×4 ×8 (IMGUI). Devuelve true si se cambió. <paramref name="current"/> = la marcada.</summary>
    public static bool Buttons(Rect r, float current, out float picked, GUIStyle style = null)
    {
        picked = current;
        float w = r.width / Options.Length;
        bool changed = false;
        for (int i = 0; i < Options.Length; i++)
        {
            bool on = Mathf.Approximately(current, Options[i]);
            Color old = GUI.backgroundColor;
            if (on) GUI.backgroundColor = new Color(0.45f, 0.85f, 1f);
            var br = new Rect(r.x + i * w + 2f, r.y, w - 4f, r.height);
            bool hit = style != null ? GUI.Button(br, Label(Options[i]), style) : GUI.Button(br, Label(Options[i]));
            GUI.backgroundColor = old;
            if (hit && !on) { picked = Options[i]; changed = true; }
        }
        return changed;
    }
}
