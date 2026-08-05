using UnityEngine;

/// <summary>
/// Efectos de pantalla por ESTADO (docs/stats-as-truth.md §6 · "cámara artística"). Lee los drives mentales
/// del jugador (`mentalFatigue`/`sleepiness`/`stress`/`satisfaction`, heredados de `Anima`) y pinta
/// superposiciones a pantalla completa: **sueño** → párpados (barras negras arriba/abajo que se cierran) +
/// tinte oscuro; **fatiga** → tinte gris (desaturación fingida); **estrés** → viñeta roja en los bordes;
/// **satisfacción alta** → tinte cálido. Complementa los efectos de *transform* de <see cref="CameraManager"/>
/// (shake/FOV) y **completa su placeholder de blackout** (expone <see cref="Fade"/>). Es el **2º actuador de
/// la emoción** (junto a la postura del `CreatureRig`). OnGUI (sin post-proceso → sin dependencias de assets).
/// Sin `PlayerStats` = no-op; usa `debugPreview`/`debugAutoCycle` para verlo en el sandbox.
/// </summary>
public class ScreenEffects : MonoBehaviour
{
    public static ScreenEffects Instance { get; private set; }

    [Tooltip("Si es null, se auto-busca en escena.")]
    public PlayerStats stats;
    [Range(0f, 1f), Tooltip("Fuerza los efectos (fatiga/sueño/estrés) para previsualizar sin jugador.")]
    public float debugPreview = 0f;
    [Tooltip("Oscila debugPreview solo para el sandbox.")]
    public bool debugAutoCycle = false;

    [Header("Colores")]
    public Color sleepColor   = new Color(0f, 0f, 0.02f);
    public Color fatigueColor = new Color(0.5f, 0.5f, 0.5f);
    public Color stressColor  = new Color(0.4f, 0f, 0f);
    public Color joyColor     = new Color(1f, 0.85f, 0.5f);

    float _fadeElapsed, _fadeDur = -1f;
    Color _fadeColor = Color.black;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }
    void Start() { if (stats == null) stats = FindObjectOfType<PlayerStats>(); }

    /// <summary>Fundido breve al color y de vuelta (pico a mitad). Lo llama, p. ej., el blackout de CameraManager.</summary>
    public void Fade(float duration, Color color)
    {
        _fadeDur = Mathf.Max(0.01f, duration); _fadeElapsed = 0f; _fadeColor = color;
    }

    void Update()
    {
        if (debugAutoCycle) debugPreview = Mathf.PingPong(Time.time * 0.15f, 1f);
        if (_fadeDur > 0f)
        {
            _fadeElapsed += Time.deltaTime;
            if (_fadeElapsed >= _fadeDur) _fadeDur = -1f;
        }
    }

    float Ch(float statVal) => Mathf.Clamp01(Mathf.Max(debugPreview, statVal));

    void OnGUI()
    {
        float w = Screen.width, h = Screen.height;
        Texture2D tex = Texture2D.whiteTexture;

        float sleep   = Ch(stats != null ? stats.sleepiness : 0f);
        float fatigue = Ch(stats != null ? stats.mentalFatigue : 0f);
        float stress  = Ch(stats != null ? stats.stress : 0f);
        float joy     = stats != null ? stats.satisfaction : debugPreview;

        // Fatiga: tinte gris a pantalla completa (desaturación fingida).
        if (fatigue > 0.01f) Fill(new Rect(0f, 0f, w, h), fatigueColor, fatigue * 0.35f, tex);

        // Satisfacción alta: tinte cálido suave.
        if (joy > 0.5f) Fill(new Rect(0f, 0f, w, h), joyColor, (joy - 0.5f) * 0.3f, tex);

        // Estrés: viñeta roja en los 4 bordes (túnel).
        if (stress > 0.01f)
        {
            float b = Mathf.Lerp(20f, Mathf.Min(w, h) * 0.18f, stress);
            float a = stress * 0.6f;
            Fill(new Rect(0f, 0f, w, b), stressColor, a, tex);          // arriba
            Fill(new Rect(0f, h - b, w, b), stressColor, a, tex);       // abajo
            Fill(new Rect(0f, 0f, b, h), stressColor, a, tex);          // izquierda
            Fill(new Rect(w - b, 0f, b, h), stressColor, a, tex);       // derecha
        }

        // Sueño: párpados (barras negras que se cierran) + tinte oscuro.
        if (sleep > 0.01f)
        {
            float lid = sleep * h * 0.5f;
            Fill(new Rect(0f, 0f, w, lid), sleepColor, 0.95f, tex);
            Fill(new Rect(0f, h - lid, w, lid), sleepColor, 0.95f, tex);
            Fill(new Rect(0f, 0f, w, h), sleepColor, sleep * 0.25f, tex);
        }

        // Fade (blackout de CameraManager u otros): triángulo 0→1→0.
        if (_fadeDur > 0f)
        {
            float p = _fadeElapsed / _fadeDur;
            float a = 1f - Mathf.Abs(p * 2f - 1f);   // 0 en los extremos, 1 a mitad
            Fill(new Rect(0f, 0f, w, h), _fadeColor, a, tex);
        }
    }

    static void Fill(Rect r, Color c, float alpha, Texture2D tex)
    {
        Color old = GUI.color;
        GUI.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(alpha));
        GUI.DrawTexture(r, tex);
        GUI.color = old;
    }
}
