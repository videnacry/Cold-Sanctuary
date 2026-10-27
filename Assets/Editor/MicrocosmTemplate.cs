using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// LA PLANTILLA DE MICROCOSMOS (Berón 2026-10-27): todo microcosmos se crea a partir de TRES DATOS que hay que dar sí o sí
/// —su FECHA, su HORA y la PLATAFORMA del santuario a la que pertenece— y con ellos la plantilla lo monta siempre igual
/// (docs/microcosmos-areas.md):
/// - la plataforma del santuario, AGRANDADA ×10 (sus muebles son montañas) y entera;
/// - la flora y los insectos NATIVOS de esa fecha y ese lugar (MicroBiome), a escala de hormiga, hasta el agua;
/// - el SOL a la hora pedida (el alba, el mediodía del solsticio…);
/// - su fecha en la raíz (<see cref="MicroEra"/>: las conductas preguntan qué existía entonces) y el aviso al entrar;
/// - la entrada, el portal de salida, el agua alrededor y su historia en el hueco más libre.
/// Sin fecha, hora o plataforma no se construye (<see cref="MicrocosmSpec"/> no lo deja crear).
/// </summary>
internal sealed class MicrocosmSpec
{
    public readonly string scene, display, platform, date;
    public readonly float hour;
    public readonly int year;
    public readonly string place;
    public Color tint = new Color(0.7f, 0.7f, 0.7f);
    public SampleSceneBuilder.MissionKind kind = SampleSceneBuilder.MissionKind.Channel;
    public System.Func<Transform, int> story;

    /// <param name="scene">Nombre de la escena (Microcosmos_…).</param>
    /// <param name="display">Su nombre: elige el bioma (MicroBiome) y la época (Eras) si no se dan.</param>
    /// <param name="date">La fecha que se cuenta: «25 de diciembre de 1914», «solsticio de verano, ~240 a. C.».</param>
    /// <param name="hour">La hora (0–24): 5,5 = el alba; 12 = mediodía.</param>
    /// <param name="platform">El área del santuario que se agranda (p. ej. «Infirmary_Area»).</param>
    public MicrocosmSpec(string scene, string display, string date, float hour, string platform, int? year = null, string place = null)
    {
        if (string.IsNullOrWhiteSpace(scene) || string.IsNullOrWhiteSpace(display)) throw new System.ArgumentException("El microcosmos necesita escena y nombre.");
        if (string.IsNullOrWhiteSpace(date)) throw new System.ArgumentException($"«{display}»: falta su FECHA.");
        if (hour < 0f || hour >= 24f) throw new System.ArgumentException($"«{display}»: la HORA va de 0 a 24 (dada: {hour}).");
        if (string.IsNullOrWhiteSpace(platform)) throw new System.ArgumentException($"«{display}»: falta la PLATAFORMA del santuario a la que pertenece.");
        this.scene = scene; this.display = display; this.date = date; this.hour = hour; this.platform = platform;
        var era = Eras.OfMicrocosmos(display);
        this.year = year ?? (era.HasValue ? era.Value.year : Eras.Present);
        this.place = place ?? (era.HasValue ? era.Value.place : display);
    }
}

internal static class MicrocosmTemplate
{
    /// <summary>Construye la escena del microcosmos según la plantilla (en <paramref name="origin"/>, lejos del santuario).</summary>
    public static void Build(MicrocosmSpec s, Vector3 origin) => MicrocosmosAreaSceneBuilder.BuildStoryScene(s, origin);

    /// <summary>El sol a la hora <paramref name="hour"/>: sale por el este a las 6, culmina a las 12 (60°) y se pone a las 18;
    /// de noche, una luz de luna tenue y fría.</summary>
    public static void Sun(Light sun, float hour)
    {
        float day = Mathf.InverseLerp(6f, 18f, hour);
        bool night = hour < 6f || hour >= 18f;
        float elev = night ? 25f : Mathf.Max(4f, Mathf.Sin(day * Mathf.PI) * 60f);
        float az = night ? 200f : Mathf.Lerp(90f, 270f, day);   // este → sur → oeste
        sun.transform.rotation = Quaternion.Euler(elev, az, 0f);
        float warm = night ? 0f : 1f - Mathf.Clamp01(Mathf.Sin(day * Mathf.PI) * 1.6f);   // alba y ocaso, dorados
        sun.color = night ? new Color(0.62f, 0.7f, 0.9f) : Color.Lerp(new Color(1f, 0.97f, 0.92f), new Color(1f, 0.72f, 0.45f), warm);
        sun.intensity = night ? 0.35f : Mathf.Lerp(0.7f, 1.2f, Mathf.Sin(day * Mathf.PI));
    }

    /// <summary>La fecha en la raíz del microcosmos (MicroEra): año, lugar, fecha, hora y plataforma.</summary>
    public static void Stamp(GameObject root, MicrocosmSpec s)
    {
        MicroEra me = root.GetComponent<MicroEra>() ?? root.AddComponent<MicroEra>();
        me.year = s.year; me.place = s.place; me.date = s.date; me.hour = s.hour; me.platform = s.platform;
    }

    // ── EL PEDIDO DESDE EL EDITOR: pide los tres datos y crea el microcosmos ─────────────────────────────────────────
    [MenuItem("Tools/Cold Sanctuary/Nuevo microcosmos (plantilla)…")]
    static void Open() => EditorWindow.GetWindow<MicrocosmTemplateWindow>(true, "Nuevo microcosmos", true);
}

public class MicrocosmTemplateWindow : EditorWindow
{
    string _name = "Mi microcosmos", _date = "", _place = "";
    float _hour = 10f;
    int _platform;
    string[] _areas;

    void OnEnable()
    {
        var l = new List<string>();
        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None)) l.Add(a.gameObject.name);
        l.Sort();
        _areas = l.Count > 0 ? l.ToArray() : new[] { "Infirmary_Area" };
    }

    void OnGUI()
    {
        EditorGUILayout.HelpBox("Un microcosmos es una plataforma del santuario agrandada ×10, con la flora y los insectos de su fecha y lugar. Abre la escena principal para elegir la plataforma.", MessageType.Info);
        _name = EditorGUILayout.TextField("Nombre", _name);
        _date = EditorGUILayout.TextField("Fecha (obligatoria)", _date);
        _hour = EditorGUILayout.Slider("Hora", _hour, 0f, 23.99f);
        _platform = EditorGUILayout.Popup("Plataforma del santuario", _platform, _areas);
        _place = EditorGUILayout.TextField("Lugar (opcional)", _place);
        GUI.enabled = !string.IsNullOrWhiteSpace(_date) && !string.IsNullOrWhiteSpace(_name);
        if (GUILayout.Button("Crear el microcosmos"))
        {
            string scene = "Microcosmos_" + _name.Replace(' ', '_');
            var spec = new MicrocosmSpec(scene, _name, _date, _hour, _areas[_platform], place: string.IsNullOrWhiteSpace(_place) ? null : _place);
            MicrocosmTemplate.Build(spec, new Vector3(6000f, 0f, 14000f + Mathf.Abs(scene.GetHashCode() % 20) * 600f));
            Debug.Log($"[Plantilla] «{_name}» creado: Assets/Scenes/{scene}.unity ({_date}, {_hour:0.0} h, sobre {_areas[_platform]}).");
            Close();
        }
        GUI.enabled = true;
    }
}
