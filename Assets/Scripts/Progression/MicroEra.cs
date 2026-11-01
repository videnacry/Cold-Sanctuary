using UnityEngine;

// (En su propio archivo: Unity solo guarda en las escenas un MonoBehaviour cuyo archivo se llama como la clase; dentro de
// Era.cs se perdía al guardar la escena del microcosmos.)
/// <summary>La FECHA de un microcosmos: va en su raíz; mientras su escena está cargada, <see cref="Eras.Now"/> es su año.</summary>
public class MicroEra : MonoBehaviour
{
    public static MicroEra Current { get; private set; }
    public int year = Eras.Present;
    public string place = "";
    [Tooltip("La fecha que se cuenta (día, estación o época), de la plantilla de microcosmos.")]
    public string date = "";
    [Tooltip("La hora del día (0–24): de ella sale la posición del sol.")]
    public float hour = 10f;
    [Tooltip("La plataforma del santuario que este microcosmos agranda (su área).")]
    public string platform = "";

    // Al entrar: dónde y cuándo estás (fecha, hora y la plataforma del santuario de la que es este microcosmos).
    void Start()
    {
        if (string.IsNullOrEmpty(date)) return;
        int h = Mathf.FloorToInt(hour), m = Mathf.RoundToInt((hour - h) * 60f) % 60;
        LessonPrompt.Show($"{place} · {date} · {h:00}:{m:00} · sobre la plataforma: {platform}", null, 8f);
    }

    void OnEnable() { Current = this; _guardAt = 0f; }

    // EL BLOQUEO DE HUMANOS (InsectOnly): en un microcosmos solo hay insectos. Al cargar y cada 2 s, quien aparezca con cuerpo
    // humano en esta escena (personajes, spawners, copias de la jornada) se convierte en su insecto.
    float _guardAt;
    void Update()
    {
        if (Time.time < _guardAt) return;
        _guardAt = Time.time + 2f;
        InsectOnly.EnforceScene(gameObject.scene);
    }
    void OnDisable() { if (Current == this) Current = null; }
}
